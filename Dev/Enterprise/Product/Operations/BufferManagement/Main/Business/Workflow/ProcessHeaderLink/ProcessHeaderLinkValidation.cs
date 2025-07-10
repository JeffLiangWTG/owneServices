using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class ProcessHeaderLinkValidation : AutoProcessHeaderLinkValidation
	{
		public ProcessHeaderLinkValidation(AutoProcessHeaderLink parent)
			: base(parent)
		{
		}

		new ProcessHeaderLink Parent
		{
			get { return (ProcessHeaderLink)base.Parent; }
		}

		#region FP_FH_HeaderFrom

		protected override void CheckFP_FH_HeaderFrom()
		{
			base.CheckFP_FH_HeaderFrom();

			var headerFrom = Parent.HeaderFrom;
			var headerTo = Parent.HeaderTo;

			EnsureTemplateSetOnTemplateLink(Parent, headerFrom, headerTo);

			ListValidation.ErrorIfInvalidPK(Parent.FP_FH_HeaderFromInfo);
			CompareValidation.CheckNotEqual(Parent.FP_FH_HeaderFromInfo, Parent.FP_FH_HeaderToInfo);

			if (headerTo != null)
			{
				var linksFromOthersToMe = headerTo.LinksFromOthersToMe.Where(link => link.FP_LinkType == Parent.FP_LinkType);
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.FP_FH_HeaderFromInfo, linksFromOthersToMe);
			}

			if (headerFrom != null)
			{
				var parentWorkflowLinks = headerFrom.ParentLinks_ForBinding.Where(link => link.HeaderTo != null && link.HeaderTo.IsWorkflow).Take(2);

				if (parentWorkflowLinks.Count() > 1)
				{
					Parent.FP_FH_HeaderFromInfo.AddError(Res.GetString("bc338208-fc65-4e35-b71e-598cef091159", "The 'from' workflow already has a parent."));
				}
			}

			if (BMSRegistry.Instance.AutomaticallyValidateWorkflowLoopsOnSave.Value)
			{
				ValidateLoop(Parent, Parent.FP_FH_HeaderFromInfo);
			}

			ValidateDependency(headerFrom, headerTo, Parent.FP_FH_HeaderFromInfo, Parent);
		}

		#endregion

		#region FP_FH_HeaderTo

		protected override void CheckFP_FH_HeaderTo()
		{
			base.CheckFP_FH_HeaderTo();

			var headerFrom = Parent.HeaderFrom;
			var headerTo = Parent.HeaderTo;
			EnsureTemplateSetOnTemplateLink(Parent, headerFrom, headerTo);

			ListValidation.ErrorIfInvalidPK(Parent.FP_FH_HeaderToInfo);
			CompareValidation.CheckNotEqual(Parent.FP_FH_HeaderToInfo, Parent.FP_FH_HeaderFromInfo);

			if (headerFrom != null)
			{
				var linksFromMeToOthers = headerFrom.LinksFromMeToOthers.Where(link => link.FP_LinkType == Parent.FP_LinkType);
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.FP_FH_HeaderToInfo, linksFromMeToOthers);
			}

			if (BMSRegistry.Instance.AutomaticallyValidateWorkflowLoopsOnSave.Value)
			{
				ValidateLoop(Parent, Parent.FP_FH_HeaderToInfo);
			}

			ValidateDependency(headerFrom, headerTo, Parent.FP_FH_HeaderToInfo, Parent);
		}

		#endregion

		#region FP_LinkType

		protected override void CheckFP_LinkType()
		{
			base.CheckFP_LinkType();

			MandatoryValidation.CheckEntered(Parent.FP_LinkTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.FP_LinkTypeInfo);
		}

		#endregion

		#region Circular Dependency

		static void ValidateDependency(ProcessHeader headerFrom, ProcessHeader headerTo, ZPropertyInfo propertyInfo, ProcessHeaderLink parent)
		{
			if (headerFrom != null && headerTo != null)
			{
				var isPCHWithPrePost = parent.FP_LinkType == ProcessHeaderLinkTypeList.Codes.ParentChild && (headerFrom.IsPrerequisiteRecursiveOf(headerTo) || headerTo.IsPrerequisiteRecursiveOf(headerFrom));
				if (isPCHWithPrePost)
				{
					propertyInfo.AddError(Res.GetString("BF420679-4096-4811-B469-75703400C266", "Invalid relationship. These workflows are already in a pre/post requisite relationship and cannot also be in parent/child relationship. Please note that this may be an indirect relationship to the current workflow."));
				}

				var isDEPWithParentChild = parent.FP_LinkType == ProcessHeaderLinkTypeList.Codes.Dependency && (headerFrom.IsAncestorOf(headerTo) || headerTo.IsAncestorOf(headerFrom));
				if (isDEPWithParentChild)
				{
					propertyInfo.AddError(Res.GetString("E0615324-ADB4-4CA5-B886-24DA1315F808", "Invalid relationship. These workflows are already in a parent/child relationship and cannot also be in a pre/post requisite relationship. Please note that this may be an indirect relationship to the current workflow."));
				}

				if (parent.Template != null && parent.Template.PK != headerTo.FH_P0_Template && parent.Template.PK != headerFrom.FH_P0_Template)
				{
					propertyInfo.AddError(Res.GetString("d7bafb23-3538-4379-9c5b-3b5adaa48413", "Both the 'from' and the 'to' workflow cannot be on other Workflow Templates. This link would never be applied to a job."));
				}
			}
		}

		static void EnsureTemplateSetOnTemplateLink(ProcessHeaderLink parent, ProcessHeader headerFrom, ProcessHeader headerTo)
		{
			if (parent.Template == null && headerFrom != null && headerTo != null && (headerFrom.Template != null || headerTo.Template != null))
			{
				var fromTemplate = headerFrom.Template;
				var toTemplate = headerTo.Template;

				var message = string.Format(CultureInfo.InvariantCulture, (NoResString)@"This ProcessHeaderLink doesn't have a Template selected, but is a template ProcessHeaderLink. It must have been created independenly of ProcessHeaderLinkCollection, or I didn't write code properly.
From Workflow Template: {0}
To Workflow Template: {1}
Type = {2}
{3}", // Developer exception message
	GetDescription(fromTemplate),
	GetDescription(toTemplate),
	parent.FP_LinkType,
	parent.DisplayText);

				ErrorReporter.ReportOnce(message);
			}
		}

		static string GetDescription(ProcessTaskTemplate template)
		{
			var codeDescription = template != null ? (ICodeDescription)template : null;
			var result = string.IsNullOrEmpty(codeDescription.Code) ? codeDescription.Description : codeDescription.Code;

			return string.IsNullOrEmpty(result) ? (NoResString)"none" : result; // Developer exception message
		}

		void ValidateLoop(ProcessHeaderLink link, ZPropertyInfo propertyInfo)
		{
			if (link == null || link.HeaderFrom == null || link.HeaderTo == null)
			{
				return;
			}

			if (((IBusinessObjectInternals)link).IsInPreSaveValidation || link.IsLoopValidationForced
#if DEBUG
			|| Parent.IsPreSaveValidationEmulated_ForTest
#endif
			)
			{
				if (link.Template != null
					|| link.HeaderFrom.JobHeader == link.HeaderTo
					|| link.HeaderTo.JobHeader == link.HeaderFrom
					|| link.HeaderFrom.JobHeader != link.HeaderTo.JobHeader
					|| !link.HeaderFrom.IsTerminal && !link.HeaderTo.IsTerminal)
				{
					ProcessHeaderLinkCycleValidationHelper.CheckLoop(link,
						loopedWorkflowsHandler: (loopedWorkflows, relationshipDescription) =>
							propertyInfo.AddError(Res.GetString("8b1948a8-d30b-475e-a715-d2cdbdd906a2", "This link is part of a looped {0}. The following workflows are involved in a loop:{1}{2}", relationshipDescription, System.Environment.NewLine, loopedWorkflows)),
						hierarchyRelationshipBetweenDependenciesHandler: () => propertyInfo.AddError(Res.GetString("11cef40a-f574-4b39-a1ca-6589d7732a8a", "This is a dependency link between workflows that are also involved in a Parent-Child relationship.")));
				}
			}
		}

		#endregion
	}
}
