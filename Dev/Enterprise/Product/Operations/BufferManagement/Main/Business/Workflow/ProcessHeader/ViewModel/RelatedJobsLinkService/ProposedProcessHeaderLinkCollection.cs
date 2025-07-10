using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.BufferManagement.Business
{
	public sealed class ProposedProcessHeaderLinkCollection : NonPersistentBusinessObjectCollection<ProposedProcessHeaderLink>
	{
		public ProposedProcessHeaderLinkCollection(ProcessJobHeader fromJobHeader, ProcessJobHeader toJobHeader)
			: base(fromJobHeader.Factory)
		{
			this.fromJobHeader = fromJobHeader;
			this.toJobHeader = toJobHeader;

			Build();
		}
		public ProposedProcessHeaderLinkCollection(ProcessJobHeader fromJobHeader, ProcessJobHeader toJobHeader, IEnumerable<IProcessTaskTemplate> headerLinkSourceTemplates)
			: base(fromJobHeader.Factory)
		{
			this.fromJobHeader = fromJobHeader;
			this.toJobHeader = toJobHeader;
			this.headerLinkSourceTemplates = headerLinkSourceTemplates;

			Build();
		}

		readonly ProcessJobHeader fromJobHeader;
		readonly ProcessJobHeader toJobHeader;
		readonly IEnumerable<IProcessTaskTemplate> headerLinkSourceTemplates;

		#region NonPersistentBusinessObjectCollection Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ProposedProcessHeaderLink(fromJobHeader, toJobHeader);
		}

		#endregion

		#region Implementation

		void Build()
		{
			var parent1 = fromJobHeader.Parent;
			var parent2 = toJobHeader.Parent;

			if (parent1 != null && parent2 != null)
			{
				var jobHeader1 = ProcessJobHeader.GetForParent(parent1, fromJobHeader.Factory, addDefaultProcessHeaderIfNone: false);
				var jobHeader2 = ProcessJobHeader.GetForParent(parent2, fromJobHeader.Factory, addDefaultProcessHeaderIfNone: false);

				var templatesForParent1 = GetApplicableTemplates(parent1, jobHeader1);
				var templatesForParent2 = GetApplicableTemplates(parent2, jobHeader2);

				var externalLinksForParent1 = GetExternalTemplateLinks(templatesForParent1.Values);
				var externalLinksForParent2 = GetExternalTemplateLinks(templatesForParent2.Values);

				var relevantLinks = externalLinksForParent1.Intersect(externalLinksForParent2).ToArray();

				if (headerLinkSourceTemplates != null)
				{
					var sourceTemplatePKs = new HashSet<ZGuid>(headerLinkSourceTemplates.Select(x => x.Identifier));
					relevantLinks = relevantLinks.Where(x => sourceTemplatePKs.Contains(x.HeaderFrom.FH_P0_Template) || sourceTemplatePKs.Contains(x.HeaderTo.FH_P0_Template)).ToArray();
				}

				var isSameJobType = parent1.WorkflowType == parent2.WorkflowType;

				if (isSameJobType)
				{
					relevantLinks = relevantLinks.Where(l =>
					{
						var template1 = Factory.Load<ProcessTaskTemplate>(l.HeaderFrom.FH_P0_Template);
						var template2 = Factory.Load<ProcessTaskTemplate>(l.HeaderTo.FH_P0_Template);
						return template1.GetWorkflowType() == template2.GetWorkflowType();
					}).ToArray();
				}

				var newLinks = new Dictionary<Tuple<ZGuid, ZGuid>, ProposedProcessHeaderLink>();

				foreach (var link in relevantLinks)
				{
					var jobHeaderForFromDirection = templatesForParent1.ContainsKey(link.HeaderFrom.FH_P0_Template) ? jobHeader1 : jobHeader2;
					var jobHeaderForToDirection = templatesForParent2.ContainsKey(link.HeaderTo.FH_P0_Template) ? jobHeader2 : jobHeader1;

					if (!isSameJobType && jobHeaderForFromDirection == jobHeaderForToDirection)
					{
						ReportSameJobHeaderFoundForTemplateLink(templatesForParent1.Values, templatesForParent2.Values, parent1, parent2);
					}
					else
					{
						ProposedProcessHeaderLink proposedLink;

						if (ProposedProcessHeaderLink.TryCreateFromTemplateLink(link, jobHeaderForFromDirection, jobHeaderForToDirection, out proposedLink))
						{
							var linkKey = Tuple.Create(proposedLink.FP_FH_HeaderFrom, proposedLink.FP_FH_HeaderTo);

							if (!newLinks.ContainsKey(linkKey))
							{
								newLinks.Add(linkKey, proposedLink);
								Add(proposedLink);
							}
						}
					}
				}
			}
		}

		IDictionary<ZGuid, ProcessTaskTemplate> GetApplicableTemplates(IWorkflowProvider parent, ProcessJobHeader jobHeader)
		{
			var result = new Dictionary<ZGuid, ProcessTaskTemplate>();

			FillWithCurrentlyMatchingTemplates(result, parent, Factory);
			FillWithPreviouslyAppliedTemplates(result, jobHeader);

			return result;
		}

		static void FillWithCurrentlyMatchingTemplates(IDictionary<ZGuid, ProcessTaskTemplate> matches, IWorkflowProvider parent, BusinessObjectFactory factory)
		{
			var allTemplates = ProcessTask.Loader.LoadTemplateMatches(parent, factory);

			for (var i = 0; i < allTemplates.Length; i++)
			{
				var template = allTemplates[i];
				var nextTemplate = i < allTemplates.Length - 1 ? allTemplates[i + 1] : null;

				matches.Add(template.PK, template);

				if (nextTemplate == null || !template.CanFallBackForTasks)
				{
					break; // This avoids checking the last template since there's no other templates to fall back to.
				}
			}
		}

		static void FillWithPreviouslyAppliedTemplates(IDictionary<ZGuid, ProcessTaskTemplate> matches, ProcessJobHeader jobHeader)
		{
			foreach (ProcessHeader workflow in jobHeader.ProcessHeaders)
			{
				if (!workflow.FH_ParentTemplateId.IsEmpty)
				{
					var templateVersionOfWorkflow = workflow.Factory.Load<ProcessHeader>(workflow.FH_ParentTemplateId);

					if (templateVersionOfWorkflow != null && !matches.ContainsKey(templateVersionOfWorkflow.FH_P0_Template))
					{
						var template = templateVersionOfWorkflow.Template;

						matches.Add(template.PK, template);
					}
				}
			}
		}

		static IEnumerable<ProcessHeaderLink> GetExternalTemplateLinks(IEnumerable<ProcessTaskTemplate> templates)
		{
			return
				from template in templates
				from ProcessHeaderLink link in template.ProcessHeaderLinks
				where link.IsExternalTemplateLink
				select link;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1121:DoNotIncludeColumnValuesOrNamesInErrorReporterKey", Justification = "Baseline")]
		static void ReportSameJobHeaderFoundForTemplateLink(IEnumerable<ProcessTaskTemplate> templatesForParent1, IEnumerable<ProcessTaskTemplate> templatesForParent2, IWorkflowProvider parent1, IWorkflowProvider parent2)
		{
			ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture,
				@"This is an external template link, but the same job header was found to be relevant for the from and to records.
Templates for job1 ({0}):
{1}

Templates for job2 ({2}):
{3}
",
				(parent1 as BusinessObject)?.HumanReadableName ?? string.Empty,
				string.Join(System.Environment.NewLine, templatesForParent1.Select(t => t.P0_Name)),
				(parent2 as BusinessObject)?.HumanReadableName ?? string.Empty,
				string.Join(System.Environment.NewLine, templatesForParent2.Select(t => t.P0_Name))
			));
		}

		#endregion
	}
}
