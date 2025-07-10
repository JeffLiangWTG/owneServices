using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class VisualizerMenuTemplatePivotValidationTest : TestCaseWithFactory
	{
		#region TestValidateSI_DataStoreName

		public void TestValidateSI_DataStoreName_Blank()
		{
			const string errorMessage = "Please enter a value.";

			var pivot = Factory.New<VisualizerMenuTemplatePivot>();
			pivot.SI_DataStoreName = ZString.Empty;
			AssertHasError(pivot.SI_DataStoreNameInfo, errorMessage);

			pivot.SI_DataStoreName = "StorageName";
			AssertNoError(pivot.SI_DataStoreNameInfo, errorMessage);
		}

		public void TestValidateSI_DataStoreName_Duplicate()
		{
			const string warningMessage = @"There are Forms in other Menu Items with the same Data Store Name, this means they will share data overrides. The other Forms with this Data Store Name are:
Consol > Departure/Blah/SomeDoc > Pivot1";

			var template = Factory.NewWithValidTestData<VisualizerTemplate>();

			const string consolBusinessContext = "Consol";
			const string shipmentBusinessContext = "Shipment";

			CreateMenuItemWithDocument(template, consolBusinessContext, "Departure/Blah", "SomeDoc", "Pivot1", "DataStorage1");
			CreateMenuItemWithDocument(template, consolBusinessContext, "Arrival/Blah", "AnotherDoc", "Pivot2", "DataStorage2");
			CreateMenuItemWithDocument(template, shipmentBusinessContext, ZString.Empty, "ThirdDoc", "Pivot3", "DataStorage1");

			Factory.Save();

			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_BusinessContext = consolBusinessContext;

			var systemDocument = Factory.New<VisualizerMenuTemplatePivot>();
			systemDocument.SI_SU = menuItem.PK;
			systemDocument.SI_IsSystemDefined = true;
			systemDocument.SI_DataStoreName = "DataStorage1";
			AssertHasWarning("Duplicate data store name when system pivot should show warning.", systemDocument.SI_DataStoreNameInfo, warningMessage);
			AssertNoErrors("No duplicate errors", systemDocument.SI_DataStoreNameInfo);

			systemDocument.SI_DataStoreName = "DataStorage3";
			AssertNoWarnings("Not a duplicate anymore", systemDocument.SI_DataStoreNameInfo);
			AssertNoErrors("No duplicate errors", systemDocument.SI_DataStoreNameInfo);

			var nonSystemDocument = Factory.New<VisualizerMenuTemplatePivot>();
			nonSystemDocument.SI_SU = menuItem.PK;
			nonSystemDocument.SI_IsSystemDefined = false;
			nonSystemDocument.SI_DataStoreName = "DataStorage1";
			AssertHasWarning("Duplicate data store name when non system pivot should show error", nonSystemDocument.SI_DataStoreNameInfo, warningMessage);
			AssertNoErrors("No duplicate errors", nonSystemDocument.SI_DataStoreNameInfo);

			nonSystemDocument.SI_DataStoreName = "DataStorage4";
			AssertNoWarning("Not a duplicate anymore", nonSystemDocument.SI_DataStoreNameInfo, warningMessage);
			AssertNoErrors("No duplicate errors", nonSystemDocument.SI_DataStoreNameInfo);
		}

		void CreateMenuItemWithDocument(VisualizerTemplate template, string businessContext, string menuPath, string menuName, string docTitle, string dataStoreName)
		{
			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_BusinessContext = businessContext;
			menuItem.SU_MenuPath = menuPath;
			menuItem.SU_MenuName = menuName;

			var document = (VisualizerMenuTemplatePivot)menuItem.Documents.AddNew();
			document.SI_DocumentTitle = docTitle;
			document.SI_DataStoreName = dataStoreName;
			document.SI_SU = menuItem.PK;
			document.SI_SO = template.PK;
		}

		#endregion

		#region TestValidateSI_MenuTemplateFilter

		public void TestValidateSI_MenuTemplateFilter()
		{
			const string errorMessage = "Macro has the following compilation errors";

			var menuItem = Factory.New<VisualizerMenuItem>();
			var document = (VisualizerMenuTemplatePivot)menuItem.Documents.AddNew();
			document.SI_SU = menuItem.PK;

			var validation = new VisualizerMenuTemplatePivotValidation(document);

			validation.ValidateAll();
			AssertNoRowErrorContaining(document, errorMessage);

			document.SI_MenuTemplateFilter = "bad macro";

			validation.ValidateAll();
			AssertHasRowErrorContaining(document, errorMessage);

			document.SI_MenuTemplateFilter = "1 == 1";

			validation.ValidateAll();
			AssertNoRowErrorContaining(document, errorMessage);
		}

		public void TestValidateSI_MenuTemplateFilter_Duplicate()
		{
			const string errorMessage = "There is another document with the same filter. It may cause a problem because only one form can be shown to the user at a time.";

			var menuItem = Factory.New<VisualizerMenuItem>();
			var document1 = (VisualizerMenuTemplatePivot)menuItem.Documents.AddNew();
			document1.SI_SU = menuItem.PK;

			var validation = new VisualizerMenuTemplatePivotValidation(document1);

			validation.ValidateSI_MenuTemplateFilter();
			AssertNoWarningContaining(document1.SI_MenuTemplateFilterInfo, errorMessage);

			var document2 = (VisualizerMenuTemplatePivot)menuItem.Documents.AddNew();
			document2.SI_SU = menuItem.PK;

			validation.ValidateSI_MenuTemplateFilter();
			AssertHasWarningContaining(document1.SI_MenuTemplateFilterInfo, errorMessage);

			document2.SI_MenuTemplateFilter = "1 == 1";

			validation.ValidateSI_MenuTemplateFilter();
			AssertNoWarningContaining(document1.SI_MenuTemplateFilterInfo, errorMessage);

			document1.SI_MenuTemplateFilter = "1 == 2";
			validation.ValidateSI_MenuTemplateFilter();
			AssertNoWarningContaining(document1.SI_MenuTemplateFilterInfo, errorMessage);

			document1.SI_MenuTemplateFilter = "false";
			validation.ValidateSI_MenuTemplateFilter();
			AssertNoWarningContaining(document1.SI_MenuTemplateFilterInfo, errorMessage);
		}

		#endregion

		#region TestValidateSI_DocumentTitle

		public void TestValidateSI_DocumentTitle_ForBillOfLading()
		{
			const string errorMessage = "Bills Of Lading document Titles can only be one on of the following: ORIGINAL and COPY.";

			var billOfLadingTemplate = Factory.Load<VisualizerTemplate>(new ZGuid("bd92bb18-0a32-4ea7-86fa-3fb0392be24a"));
			AssertNotNull("prerequisite: bill of lading template was found", billOfLadingTemplate);

			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "Custom Bill Of Lading";

			var document = (VisualizerMenuTemplatePivot)menuItem.Documents.AddNew();
			document.SI_SU = menuItem.PK;
			document.SI_SO = billOfLadingTemplate.PK;

			var validation = new VisualizerMenuTemplatePivotValidation(document);

			document.SI_DocumentTitle = "ORIGINAL";
			validation.ValidateSI_DocumentTitle();
			AssertNoErrorContaining(document.SI_DocumentTitleInfo, errorMessage);

			document.SI_DocumentTitle = "MY ORIGINAL";
			validation.ValidateSI_DocumentTitle();
			AssertHasErrorContaining(document.SI_DocumentTitleInfo, errorMessage);

			document.SI_DocumentTitle = "COPY - NON NEGOTIABLE";
			validation.ValidateSI_DocumentTitle();
			AssertHasErrorContaining(document.SI_DocumentTitleInfo, errorMessage);

			document.SI_DocumentTitle = "COPY";
			validation.ValidateSI_DocumentTitle();
			AssertNoErrorContaining(document.SI_DocumentTitleInfo, errorMessage);
		}

		public void TestValidateSI_DocumentTitle_NonBillOfLading()
		{
			const string errorMessage = "Bills Of Lading document Titles can only be one on of the following: ORIGINAL and COPY.";

			var nonBillOfLadingTemplate = Factory.Load<VisualizerTemplate>(new ZGuid("ef9ea643-a64d-46ad-b45c-feb73c339f61"));
			AssertNotNull("prerequisite: bill of lading template was found", nonBillOfLadingTemplate);

			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "Custom Bill Of Lading";

			var document = (VisualizerMenuTemplatePivot)menuItem.Documents.AddNew();
			document.SI_SU = menuItem.PK;
			document.SI_SO = nonBillOfLadingTemplate.PK;

			var validation = new VisualizerMenuTemplatePivotValidation(document);

			document.SI_DocumentTitle = "CargoDues";
			validation.ValidateSI_DocumentTitle();

			bool HasBillOfLadingNotification() => document.SI_DocumentTitleInfo.Notifications.Any(n => n.Message.Equals(errorMessage, StringComparison.InvariantCultureIgnoreCase));
			Assert("no Bill Of Lading notifications for non Bill Of Lading document", !HasBillOfLadingNotification());
		}

		#endregion
	}
}
