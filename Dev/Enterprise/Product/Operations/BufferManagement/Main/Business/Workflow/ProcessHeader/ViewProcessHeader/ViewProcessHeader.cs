using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	[DebuggerDisplay("{VFH_CompletionStatement}, Job: {VFH_JobCode}, Duration: {VFH_PlannedDurationInMinutes}, Table Code: {VFH_ParentTableCode}")]
	public class ViewProcessHeader : AutoViewProcessHeader,
		IWorkflow,
		ICCPMSchedulable,
		IDataVersionLoggingSupported
	{
		public ViewProcessHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(VFH_LastTransferType), ConcurrencyPolicy.Ignore);
		}

		#region Business Object Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			VFH_Status = WorkflowStatusList.Codes.Closed;
		}

		public override void Delete()
		{
			throw new NotSupportedException("This is a view and should never be deleted. It is intended for lightweight loading in places like the Release Gate where we also want the ability to mutate limited state.");
		}

		public override void OnSaving()
		{
			base.OnSaving();

			var infosWithChanges = ZPropertyInfoHash.Cast<ZPropertyInfo>().Except(AllowedPropertiesToBeSaved).Where(i => i.HasChanges).ToArray();

			if (infosWithChanges.Length > 0)
			{
				var message = new StringBuilder();
				message.AppendLine((NoResString)"You're updating aspects of ProcessHeader via ViewProcessHeader for which this view was not designed. It is intended for lightweight loading in places like the Release Gate where we also want the ability to mutate limited state. These changes won't be updated by the 'instead of update' trigger on ViewProcessHeader."); // Developer exception message
				message.AppendLine((NoResString)"Properties edited:"); // Developer exception message
				message.Append(string.Join(System.Environment.NewLine, infosWithChanges.Select(i => i.Name)));

				ErrorReporter.ReportOnce(message.ToString());
			}
		}

		IEnumerable<ZPropertyInfo> AllowedPropertiesToBeSaved
		{
			get
			{
				yield return VFH_FC_CurrentComponentInfo;
				yield return VFH_FC_DedicatedBufferInfo;
				yield return VFH_ReleaseDateTimeInfo;
				yield return VFH_SystemLastEditTimeUtcInfo;
				yield return VFH_SystemLastEditUserInfo;
				yield return VFH_LastTransferTypeInfo;
			}
		}

		#endregion

		#region Logging
		protected override string GetLogsParentTableName() => ProcessHeaderSchema.Constants.TableName;

		protected override string GetDataVersionLogsTablePrefix() => ProcessHeaderSchema.Constants.Prefix;

		bool IDataVersionLoggingSupported.IsDataVersionsAutoLogged => this.AreDataVersionsLogged();

		DataVersionLogValueFormatter IDataVersionLoggingSupported.DataVersionLogValueFormatter => new CustomDataVersionFormatter();

		class CustomDataVersionFormatter : DataVersionLogValueFormatter
		{
			protected override string GetDataColumnName(ZPropertyInfo property)
			{
				var propertyName = property.Name.Remove(0, 1);
				var column = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(propertyName, ProcessHeaderSchema.Constants.TableName);

				return column != null ? propertyName : base.GetDataColumnName(property);
			}
		}

		#endregion

		#region Properties

		[ReadOnly(true)]
		[BusinessObjectTestExclude]
		public override ZString VFH_Description
		{
			get => base.VFH_Description;
			set => throw new NotSupportedException("VFH_Description is a calculated property");
		}

		[ReadOnly(true)]
		[BusinessObjectTestExclude]
		public override ZString VFH_WorkflowType
		{
			get { return base.VFH_WorkflowType; }
			set
			{
				throw new NotSupportedException("VFH_WorkflowType is a calculated property");
			}
		}

		[ReadOnly(true)]
		[BusinessObjectTestExclude]
		public override ZString VFH_JobCode
		{
			get { return base.VFH_JobCode; }
			set
			{
				throw new NotSupportedException("VFH_JobCode is a calculated property");
			}
		}

		[RelatedBusinessObject(nameof(Component))]
		public override ZGuid VFH_FC_CurrentComponent
		{
			get { return base.VFH_FC_CurrentComponent; }
			set { base.VFH_FC_CurrentComponent = value; }
		}

		internal bool CanRelease { get; set; }

		#endregion

		#region Related Business Objects

		public GlbGroup ReleaseGroup => Factory.Load<GlbGroup>(VFH_GG_ReleaseGroup);

		public BMComponent Component => Factory.Load<BMComponent>(VFH_FC_CurrentComponent);

		public ViewProcessTaskCollection Tasks
		{
			get
			{
				if (tasks == null)
				{
					tasks = new ViewProcessTaskCollection(this);
					tasks.ApplySort(ViewProcessTaskSchema.P9_Sequence.Name, ListSortDirection.Ascending);
				}

				return tasks;
			}
		}

		ViewProcessTaskCollection tasks;

		#endregion

		#region Release < All of the state changes regarding this class happen here.

		public void ReleaseToBuffer(BMComponent buffer, IBMComponentLink link)
		{
			buffer.RequireBuffer();

			var oldValue = VFH_FC_CurrentComponent;

			VFH_FC_CurrentComponent = buffer.PK;
			VFH_FC_DedicatedBuffer = ZGuid.Empty;
			VFH_SystemLastEditTimeUtc = ZDateTime.UtcNow;
			VFH_SystemLastEditUser = Env.CurrentUser.Initials;

			this.OnComponentChanged(oldValue, ComponentChangeMode.ReleaseGate, link: link);
		}

		#endregion

		#region IWorkflow Members

		Guid IWorkflow.PK
		{
			get { return PK.ToGuid(); }
		}

		Guid IWorkflow.ReleaseGroupPK
		{
			get { return VFH_GG_ReleaseGroup.IsValid ? VFH_GG_ReleaseGroup.ToGuid() : Guid.Empty; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		int IWorkflow.PlannedDurationMinutes
		{
			get { return VFH_PlannedDurationInMinutes; }
			set { VFH_PlannedDurationInMinutes = value; }
		}

		public string Description
		{
			get { return VFH_CompletionStatement; }
		}

		public DateTime EarliestStartDateUtc
		{
			get { return VFH_DoNotStartBeforeDate.IsValid ? VFH_DoNotStartBeforeDate.ToDateTime() : default(DateTime); }
		}

		public DateTime JobEarliestStartDateUtc
		{
			get { return VFH_JobDoNotStartBeforeDate.IsValid ? VFH_JobDoNotStartBeforeDate.ToDateTime() : default(DateTime); }
		}

		public string JobDescription
		{
			get { return VFH_JobCode; }
		}

		string IWorkflow.WorkflowType
		{
			get { return VFH_WorkflowType; }
		}

		string IWorkflow.Status
		{
			get { return VFH_Status; }
		}

		IEnumerable<IWorkflowTask> IWorkflow.Tasks
		{
			get { return Tasks; }
		}

		Guid IWorkflow.CurrentComponentPK
		{
			get { return VFH_FC_CurrentComponent.IsValid ? VFH_FC_CurrentComponent.ToGuid() : default(Guid); }
			set { VFH_FC_CurrentComponent = value; }
		}

		Guid IWorkflow.JobLevelWorkflowPK
		{
			get { return VFH_FH_ParentHeader.IsValid ? VFH_FH_ParentHeader.ToGuid() : default(Guid); }
		}

		Guid IWorkflow.ParentId
		{
			get { return VFH_ParentId.IsValid ? VFH_ParentId.ToGuid() : default(Guid); }
		}

		string IWorkflow.ParentTableCode
		{
			get { return VFH_ParentTableCode; }
		}

		DateTime IWorkflow.LastTransferDateUtc
		{
			get { return VFH_ReleaseDateTime.IsValid ? VFH_ReleaseDateTime.ToDateTime() : default(DateTime); }
			set { VFH_ReleaseDateTime = value; }
		}

		string IWorkflow.LastTransferType
		{
			get => VFH_LastTransferType;
			set => VFH_LastTransferType = value;
		}

		#endregion

		#region ICCPMSchedulable Members

		public bool IsCcpmScheduleReleasable { get; set; }

		#endregion
	}
}
