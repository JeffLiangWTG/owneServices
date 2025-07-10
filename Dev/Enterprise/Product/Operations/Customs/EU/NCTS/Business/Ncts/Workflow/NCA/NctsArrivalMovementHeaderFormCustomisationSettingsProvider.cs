using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsArrivalMovementHeaderFormCustomisationSettingsProvider : FormCustomisationSettingsProvider
	{
		protected override string[] GetPropertiesThatAffectWorkflow()
		{
			return new string[]
			{
				NctsArrivalMovementHeader.Schema.ExportFlag,
				NctsArrivalMovementHeader.Schema.AuthorizationCode,
				NctsArrivalMovementHeader.Schema.DestinationCustomsOfficeCodeForArrival,
				NctsArrivalMovementHeader.Schema.BM_NoChangesToReport,
				NctsArrivalMovementHeader.Schema.BM_StateOfSealsBoolean
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

			tabs.Add(ResString.GetMultilingualString("3A8A4BA1-C7A6-4EB1-8FFF-7D936AF5D199", "Custom Fields"), TabNames.CustomFieldsTabPage, isVisible: true);
			tabs.Add(ResString.GetMultilingualString("7D16886A-1E97-4C63-9627-DCE65C536EF6", "Workflow & Tracking"), "WorkflowTabPage");
			tabs.Add(ResString.GetMultilingualString("4EE14710-BC75-4E94-B04D-D99777EFB07E", "Billing"), "BillingTabPage");
			tabs.Add(ResString.GetMultilingualString("D66D5A97-C0AB-426F-894F-92E58BD66832", "Messages"), "MessagesTabPage");
			tabs.Add(ResString.GetMultilingualString("ECF88DAB-74F9-4E0B-A6F8-9C0327082002", "House Consignments"), "HouseConsignmentsTabPage");
			tabs.Add(ResString.GetMultilingualString("B1828DE4-1C6B-4D25-AE51-745EC7B16AF5", "Transport & Containers"), "TransportAndPackagingTabPage");
			tabs.Add(ResString.GetMultilingualString("98336A46-EFB3-4318-A9DD-A09CD5DA9090", "Doc Data"), "DocDataTabPage");
			tabs.Add(ResString.GetMultilingualString("B9D940DD-7D38-4328-BBDA-907ED61C01DD", "eDocs"), "eDocsTabPage");
			tabs.Add(ResString.GetMultilingualString("5F2C9329-1502-42C1-AACD-5FB0EB2302A7", "Notes"), "NotesTabPage");
			tabs.Add(ResString.GetMultilingualString("48863ECD-11FE-4EB4-80E3-CC0DC89B6C1C", "Logs"), "EventTabPage");

			tabs.ResumeValidation();

			return tabs;
		}
	}
}
