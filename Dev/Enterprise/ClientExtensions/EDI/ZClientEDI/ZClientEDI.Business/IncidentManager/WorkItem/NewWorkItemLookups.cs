using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class NewWorkItemLookups : WorkItemActualLookups
	{
		public NewWorkItemLookups(NewWorkItem parent) : base(parent) { }

		public NewWorkItemLookups(BusinessObjectFactory factory) : base(factory) { }

		public new NewWorkItem Parent
		{
			get { return (NewWorkItem)base.Parent; }
		}

		//TODO WKI_Priority - must be in registry?
		public static class PatchToConstants
		{
			public const string None = "NON";
		}

		#region Work Item Type

		public static class WorkItemTypeConstants
		{
			public const string DefectFix = "FIX";
			public const string IssueFix = "ISS";
			public const string EnhanceCoPayment = "COP";
			public const string EnhanceClientSpecific = "CLI";
			public const string AmnestyFix = "AMN";
		}

		#endregion

		public CodeDescriptionPairList ActiveReviewTasks
		{
			get
			{
				return Factory.GetCachedValue("NewWorkItemLookups.ActiveReviewTasks", () =>
				{
					CodeDescriptionPairList result = new CodeDescriptionPairList();
					foreach (CodeDescriptionBool element in EDIDataRegistry.Instance.ReviewTasks.Value)
					{
						if (element.Bool)
						{
							result.AddPair(element.Code, element.Description);
						}
					}
					return result;
				});
			}
		}

		#region Error Log (Issue) List

		public HelpErrorLogCollection ErrorLogList
		{
			get
			{
				if (errorLogList == null)
				{
					errorLogList = new HelpErrorLogCollection(Factory);
				}

				return errorLogList;
			}
		}

		HelpErrorLogCollection errorLogList;

		#endregion

	}
}

