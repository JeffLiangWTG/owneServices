using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	[ModuleID(ModuleId.ProcessHeader, true)]
	public class ProcessHeaderCollection : ActiveBusinessObjectCollection<ProcessHeader>, IProcessHeaderCollection
	{
		public static ProcessHeaderCollection GetCollectionForJobIncludingJobLevelWorkflow(IWorkflowProvider job, ProcessJobHeader jobLevelWorkflow)
		{
			return new ProcessHeaderCollection(job, jobLevelWorkflow);
		}

		public static ProcessHeaderCollection GetCollectionForJobNotIncludingJobLevelWorkflow(ProcessJobHeader jobLevelWorkflow)
		{
			return new ProcessHeaderCollection(jobLevelWorkflow);
		}

		public ProcessHeaderCollection(BusinessObjectFactory factory)
			: this(factory, new ZQuery(ProcessHeaderSchema.FH_P0_Template, null))
		{
		}

		public ProcessHeaderCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, query)
		{
		}

		ProcessHeaderCollection(ProcessJobHeader jobHeader)
			: base(jobHeader.Factory, jobHeader, jobHeader.GetAllProcessHeadersInJobQuery(allowFetchOnlyFromLocalCacheIfNotInDatabase: false), ProcessHeaderSchema.FH_FH_ParentHeader)
		{
			this.processJobHeader = jobHeader;
		}

		ProcessHeaderCollection(IWorkflowProvider parent, ProcessJobHeader jobHeader)
			: base(jobHeader.Factory, (BusinessObject)parent, jobHeader.GetAllProcessHeadersInJobQuery(allowFetchOnlyFromLocalCacheIfNotInDatabase: false), ProcessHeaderSchema.FH_ParentId)
		{
			this.processJobHeader = jobHeader;
		}

		public ProcessHeaderCollection(ProcessTaskTemplate template, ZQuery additionalFilter = null)
			: base(template.Factory, template, additionalFilter ?? new ZQuery(), ProcessHeaderSchema.FH_P0_Template)
		{
			this.template = template;
		}

		readonly ProcessJobHeader processJobHeader;
		readonly ProcessTaskTemplate template;

		#region ActiveBusinessObjectCollection Overrides

		protected override void SetDefaultsForNewElementCore(ProcessHeader newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			SetDefaultsForNewWorkflow(newElement);
		}

		internal void SetDefaultsForNewWorkflow(ProcessHeader workflow)
		{
			var jobHeader = JobHeader;

			if (jobHeader != null)
			{
				workflow.FH_FH_ParentHeader = jobHeader.PK;
				workflow.FH_ParentId = jobHeader.FH_ParentId;
				workflow.FH_ParentTableCode = jobHeader.FH_ParentTableCode;
				workflow.FH_WorkflowType = jobHeader.FH_WorkflowType;

				workflow.SetBufferManagementComponentIfBlank();

				if (workflow.FH_GG_ReleaseGroup.IsEmpty)
				{
					if (jobHeader.FH_GG_ReleaseGroup.IsEmpty)
					{
						if (ShouldApplyReleaseGroupFromExistingWorkflowsOnAdd)
						{
							var existingWorkflow = jobHeader.ProcessHeaders.FirstOrDefault(h => h.FH_GG_ReleaseGroup.IsValid);

							if (existingWorkflow != null)
							{
								workflow.FH_GG_ReleaseGroup = existingWorkflow.FH_GG_ReleaseGroup;
							}
						}
					}
					else
					{
						workflow.FH_GG_ReleaseGroup = jobHeader.FH_GG_ReleaseGroup;
						if (jobHeader.FH_IsReleaseGroupSetByTemplate)
						{
							workflow.FH_IsReleaseGroupSetByTemplate = true;
						}
					}
				}

				var workflowTemplate = workflow.GetTemplate();
				if (workflowTemplate != null)
				{
					var templateSystem = BMSystem.GetForTemplate(workflowTemplate);
					if (templateSystem != null)
					{
						SetNewBMComponent(templateSystem, workflow);
					}
				}
			}
		}

		bool ShouldApplyReleaseGroupFromExistingWorkflowsOnAdd => !JobHeader.IsTemplateBeingApplied || BMSRegistry.Instance.ReleaseGroupsCanBeAssignedFromOtherWorkflowsWhenApplyingTemplates.Value;

		void SetNewBMComponent(BMSystem templateSystem, ProcessHeader workflow)
		{
			if (!workflow.IsTemplate && templateSystem != null && !templateSystem.Components.Select(c => c.PK).Contains(workflow.FH_FC_CurrentComponent))
			{
				var firstComponent = workflow.GetFirstBMComponent(templateSystem);
				if (firstComponent != null)
				{
					workflow.FH_FC_CurrentComponent = firstComponent.PK;
				}
			}
		}

		public void AddAndSetDefaults(ProcessHeader newElement)
		{
			Add(newElement);
			SetDefaultsForNewElementCore(newElement);
		}

		protected override IComparer GetSortComparerForProperty(PropertyDescriptor property, ListSortDirection direction)
		{
			return property.Name == (NoResString)"Sequence" ? new WorkflowSequenceComparer(property, direction) : base.GetSortComparerForProperty(property, direction);
		}

		public ProcessHeader FirstOpenWorkFlow => this.Where(x => x.IsOpen).OrderBy(x => x, GetWorkflowSequenceComparer()).FirstOrDefault();

		IComparer<BusinessObject> GetWorkflowSequenceComparer()
		{
			return new WorkflowSequenceComparer(TypeDescriptor.GetProperties(typeof(ProcessHeader))[nameof(ProcessHeader.Sequence)], ListSortDirection.Ascending);
		}

		protected override bool AllowNew
		{
			get { return template == null || (BMSRegistryProvider.IsBufferManagementEnabled && BMSystem.GetForTemplate(template) != null); }
		}

		protected override void CancelNew(int index)
		{
			IProcessHeader processHeader = template != null ? template.ProcessHeaders[index] : null;
			var tasksPointingToDeletedWorkflow = Array.Empty<ProcessTask>();

			if (processHeader != null)
			{
				tasksPointingToDeletedWorkflow = template.WorkflowItems.Tasks.Cast<ProcessTask>()
					.Where(x => x.IsDeleted && x.P9_FH_ProcessHeader.Equals(processHeader.PK)).ToArray();

				foreach (var task in tasksPointingToDeletedWorkflow)
				{
					task.P9_FH_ProcessHeader = ZGuid.Empty;
				}
			}

			base.CancelNew(index);

			foreach (var task in tasksPointingToDeletedWorkflow)
			{
				task.Validation.ValidateP9_FH_ProcessHeader();
			}
		}

		#endregion

		#region WorkflowSequenceComparer

		class WorkflowSequenceComparer : PropertyComparer
		{
			internal WorkflowSequenceComparer(PropertyDescriptor property, ListSortDirection direction)
				: base(property, direction)
			{
			}

			#region Comparison Constant Values

			// for comparison methods:
			// if returned < 0, the first value is smaller than the second
			// if the return == 0, the two values are the same
			// if returned > 0, the first value is greater than the second
			const int XGreaterThanY = 1;
			const int YGreaterThanX = -1;
			const int XEqualsY = 0;

			#endregion

			public override int Compare(BusinessObject x, BusinessObject y)
			{
				if (x == y)
				{
					return XEqualsY;
				}

				if (x is ProcessHeader processHeaderX && y is ProcessHeader processHeaderY)
				{
					var sequenceX = GetSequenceSegments(processHeaderX.Sequence);
					var sequenceY = GetSequenceSegments(processHeaderY.Sequence);
					var jobHeaderX = processHeaderX as ProcessJobHeader;
					var jobHeaderY = processHeaderY as ProcessJobHeader;

					if (sequenceX.Length == 0 && jobHeaderX == null)
					{
						return XGreaterThanY;
					}
					else if (sequenceY.Length == 0 && jobHeaderY == null)
					{
						return YGreaterThanX;
					}
					else
					{
						return Direction == ListSortDirection.Ascending
							? CompareWorkflowSequence(sequenceX, sequenceY)
							: CompareWorkflowSequence(sequenceY, sequenceX);
					}
				}

				return base.Compare(x, y);
			}

			[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
			static int[] GetSequenceSegments(string sequence)
			{
				return sequence.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s)).ToArray();
			}

			static int CompareWorkflowSequence(int[] sequenceX, int[] sequenceY)
			{
				var length = Math.Max(sequenceX.Length, sequenceY.Length);

				for (int i = 0; i < length; i++)
				{
					if (i >= sequenceX.Length)
					{
						return YGreaterThanX;
					}
					else if (i >= sequenceY.Length)
					{
						return XGreaterThanY;
					}
					else
					{
						var value1 = sequenceX[i];
						var value2 = sequenceY[i];

						if (value1 == value2)
						{
							continue;
						}
						else
						{
							return value1.CompareTo(value2);
						}
					}
				}

				return XEqualsY;
			}
		}

		#endregion

		#region IProcessHeaderCollection Members

		void IProcessHeaderCollection.Delete(IBusiness bizo)
		{
			base.Delete((ProcessHeader)bizo);
		}

		IProcessHeader IProcessHeaderCollection.AddNew()
		{
			return base.AddNew();
		}

		protected override void OnAdded(ProcessHeader businessObject)
		{
			base.OnAdded(businessObject);

			if (processJobHeader != null)
			{
				businessObject.FH_ParentId = processJobHeader.FH_ParentId;
				businessObject.FH_ParentTableCode = processJobHeader.FH_ParentTableCode;
				businessObject.FH_IsApproved = processJobHeader.FH_IsApproved;
				if (businessObject.FH_GG_ReleaseGroup.IsEmpty)
				{
					businessObject.FH_GG_ReleaseGroup = processJobHeader.FH_GG_ReleaseGroup;
				}
			}
			else if (template != null)
			{
				businessObject.FH_P0_Template = template.PK;

				if (this.OfType<ProcessJobHeader>().FirstOrDefault() is ProcessJobHeader jobHeader)
				{
					businessObject.FH_IsApproved = jobHeader.FH_IsApproved;
				}
			}

			if (template != null && businessObject.IsWorkflow)
			{
				SetProcessHeaderOnTemplateTasksIfRequired();
			}
		}

		void SetProcessHeaderOnTemplateTasksIfRequired()
		{
			var tasksWithoutWorkflows = template.WorkflowItems.Tasks.Cast<ProcessTask>().Where(x => !x.IsDeleted && !x.P9_FH_ProcessHeader.IsValid).ToArray();

			if (tasksWithoutWorkflows.Any())
			{
				IProcessHeader workflow = null;
				var provider = new ProcessJobHeaderProvider();

				foreach (var task in tasksWithoutWorkflows)
				{
					workflow = workflow ?? ((IProcessJobHeaderProvider)provider).GetDefaultWorkflowForTask(task);

					if (workflow != null)
					{
						task.P9_FH_ProcessHeader = workflow.PK;
					}
				}
			}
		}

		IProcessHeader IProcessHeaderCollection.this[int index]
		{
			get { return base[index]; }
		}

		public new IEnumerator<IProcessHeader> GetEnumerator()
		{
			return base.GetEnumerator();
		}

		IDictionary<IProcessHeader, IProcessHeader> IProcessHeaderCollection.CloneWorkflowsAndLinksForTemplates(IProcessJobHeader targetJobHeader, IProcessHeaderCollection targetCollection)
		{
			return CloneWorkflowAndLinksForTemplates((ProcessJobHeader)targetJobHeader, (ProcessHeaderCollection)targetCollection);
		}

		#endregion

		#region Implementation

		ProcessJobHeader JobHeader
		{
			get { return processJobHeader ?? this.OfType<ProcessJobHeader>().SingleOrDefault(); }
		}

		IDictionary<IProcessHeader, IProcessHeader> CloneWorkflowAndLinksForTemplates(ProcessJobHeader targetJobHeader, ProcessHeaderCollection targetCollection)
		{
			var sourceToCloneMap = new Dictionary<IProcessHeader, IProcessHeader>();

			if (targetJobHeader != null)
			{
				var targetTemplate = targetJobHeader.Template;

				foreach (var sourceWorkflow in this.ToArray())
				{
					if (sourceWorkflow.IsWorkflow)
					{
						var clone = (ProcessHeader)sourceWorkflow.Clone(new BusinessObjectCloneArgs());

						targetCollection.Add(clone);
						sourceToCloneMap.Add(sourceWorkflow, clone);
						clone.FH_FH_ParentHeader = targetJobHeader.PK;
					}
					else
					{
						targetJobHeader.CopyPersistentValuesFrom(sourceWorkflow);
						targetCollection.Add(targetJobHeader);
						sourceToCloneMap.Add(sourceWorkflow, targetJobHeader);
					}
				}

				foreach (var kvp in sourceToCloneMap)
				{
					var clone = kvp.Value as ITagable;

					foreach (var tagLink in kvp.Key.TagLinks)
					{
						clone?.AddTag(tagLink.TagMagnitude);
					}
				}

				var allLinks = sourceToCloneMap.SelectMany(kvp => kvp.Key.LinksFromMeToOthers.ToArray()
					.Union(kvp.Key.LinksFromOthersToMe.ToArray()))
					.Distinct()
					.Cast<ProcessHeaderLink>().ToArray();

				foreach (var link in allLinks)
				{
					var cloneLink = (ProcessHeaderLink)link.Clone(new BusinessObjectCloneArgs(new[] { ProcessHeaderLinkSchema.FP_FH_HeaderFrom.Name, ProcessHeaderLinkSchema.FP_FH_HeaderTo.Name }));

					cloneLink.Template = targetTemplate;
					cloneLink.FP_FH_HeaderFrom = (sourceToCloneMap.TryGetValue(link.HeaderFrom, out IProcessHeader clonedHeaderFrom)) ? clonedHeaderFrom.PK : link.FP_FH_HeaderFrom;
					cloneLink.FP_FH_HeaderTo = (sourceToCloneMap.TryGetValue(link.HeaderTo, out IProcessHeader clonedHeaderTo)) ? clonedHeaderTo.PK : link.FP_FH_HeaderTo;
				}
			}

			return sourceToCloneMap;
		}

		#endregion
	}
}
