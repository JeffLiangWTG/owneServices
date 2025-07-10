using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsDepartureMovementHeaderFormCustomisationSettingsProvider : FormCustomisationSettingsProvider
	{
		protected override string[] GetPropertiesThatAffectWorkflow()
		{
			return new string[]
			{
				NctsDepartureMovementHeader.Schema.BM_InBondEntryType,
				NctsDepartureMovementHeader.Schema.BM_AdditionalDeclarationType,
				NctsDepartureMovementHeader.Schema.BM_InlandTransportMode,
				NctsDepartureMovementHeader.Schema.IsSimplifiedNctsProcedure,
				NctsDepartureMovementHeader.Schema.BM_RL_NKDestinationPort
			};
		}

		class TabNames
		{
			public const string CustomFieldsTabPage = "CustomFieldsTabPage";
		}

		protected override FormCustomisableElementCollection GetDisplayTabs()
		{
			FormCustomisableElementCollection tabs = new FormCustomisableElementCollection();

			tabs.SuspendValidation();

			var tab = tabs.Add(ResString.GetMultilingualString("F8722EE3-8486-4E3A-BDB7-7370A6CFD6E0", "Details"), "DepartureDeclarationTabPage");
			tabs.Add(ResString.GetMultilingualString("03BA8E57-4A4E-41B1-8AFF-E0B8792E9BBA", "Custom Fields"), TabNames.CustomFieldsTabPage, isVisible: true);
			tabs.Add(ResString.GetMultilingualString("1DA4596D-E7E7-4A8C-A08F-0B08A979B7CA", "Workflow & Tracking"), "WorkflowTabPage");
			tabs.Add(ResString.GetMultilingualString("0B985E04-1612-45F9-9C89-E8A002A79EBD", "Billing"), "BillingTabPage");
			tabs.Add(ResString.GetMultilingualString("B80006C4-969F-4941-A57C-F7CEBD24E085", "Messages"), "MessagesTabPage");
			tabs.Add(ResString.GetMultilingualString("52507215-501C-42DC-9D14-1B34EB0FB62F", "House Consignments"), "HouseConsignmentsTabPage");
			tabs.Add(ResString.GetMultilingualString("BA7FA42B-C302-434B-ACAE-1E29BA05D021", "Movements"), "MovementsTabPage");
			tabs.Add(ResString.GetMultilingualString("4283C631-258C-45D1-B416-B976D350D744", "Services"), "ServicesTabUserControl");
			tabs.Add(ResString.GetMultilingualString("5E1EA6DA-6451-40FC-81A3-C7F1C8DD8618", "Transport & Containers"), "TransportAndPackagingTabPage");
			tabs.Add(ResString.GetMultilingualString("77730783-2B2E-408A-99E1-BF39D13B2D78", "Unloading Remarks"), "UnloadingRemarksTabPage", isVisible: true);
			tabs.Add(ResString.GetMultilingualString("37AD9110-6E0B-49B8-AF1F-72C7B59668ED", "Doc Data"), "DocDataTabPage");
			tabs.Add(ResString.GetMultilingualString("7195929D-5EAF-44A9-89F9-CF121C83462D", "eDocs"), "eDocsTabPage");
			tabs.Add(ResString.GetMultilingualString("F8AE2A83-99C6-4F02-9F8A-62963C4AF7F4", "Notes"), "NotesTabPage");
			tabs.Add(ResString.GetMultilingualString("8208D443-E188-4E0C-9911-28798D334C70", "Logs"), "EventTabPage");

			tabs.ResumeValidation();

			return tabs;
		}
	}
}
