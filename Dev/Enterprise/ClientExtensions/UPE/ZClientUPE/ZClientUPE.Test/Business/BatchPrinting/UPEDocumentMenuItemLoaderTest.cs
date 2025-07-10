using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using StmMenuItemBase = Enterprise.DocumentEngine.Business.StmMenuItemBase;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPEDocumentMenuItemLoader))]
	sealed class UPEDocumentMenuItemLoaderTest : LoaderTestCase
	{
		public void TestLoadPortraitEntryPrint()
		{
			DocumentCommand documentCommand = Loader.LoadPortraitEntryPrint();
			AssertCorrectDocumentCommand(documentCommand, UPEDocumentMenuItemLoader.PortraitEntryPrintMenuName, BusinessContext.Customs, false, "");
		}

		#region Tax Invoice And Commercial Invoice

		public void TestLoadUPSTaxInvoiceAndCommercialInvoice()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			CreateDecoyAirCargoMenuItems(UPEDocumentMenuItemLoader.UPSTaxInvoiceAndCommercialInvoiceMenuName);
			DocumentCommand docCommand = Loader.LoadUPSTaxInvoiceAndCommercialInvoice();
			AssertCorrectDocumentCommand(docCommand, UPEDocumentMenuItemLoader.UPSTaxInvoiceAndCommercialInvoiceMenuName, BusinessContext.CusHAWB, true, "\"<BusinessObjectType>\" == \"UPECallout\" &&  \"<IsInReBillQueue>\" == \"N\" && \"<RegistryItem(AdditionalRegistryItemSet.Instance.EnableUPECustomisationsItem)>\" == \"True\"");
		}

		public void TestLoadUPSTaxInvoiceAndCommercialInvoiceQueueForBatchPrint()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			CreateDecoyAirCargoMenuItems(UPEDocumentMenuItemLoader.UPSTaxInvoiceAndCommercialInvoiceQueueForBatchPrintMenuName);
			DocumentCommand docCommand = Loader.LoadUPSTaxInvoiceAndCommercialInvoiceQueueForBatchPrint();
			AssertCorrectDocumentCommand(docCommand, UPEDocumentMenuItemLoader.UPSTaxInvoiceAndCommercialInvoiceQueueForBatchPrintMenuName, BusinessContext.CusHAWB, true, "\"<BusinessObjectType>\" == \"UPECallout\" &&  \"<IsInReBillQueue>\" == \"N\" && \"<RegistryItem(AdditionalRegistryItemSet.Instance.EnableUPECustomisationsItem)>\" == \"True\"");
		}

		#endregion
		#region Tax Invoice

		public void TestLoadUPSTaxInvoice()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			CreateDecoyAirCargoMenuItems(UPEDocumentMenuItemLoader.UPSTaxInvoiceMenuName);
			DocumentCommand documentCommand = Loader.LoadUPSTaxInvoice();
			AssertCorrectDocumentCommand(documentCommand, UPEDocumentMenuItemLoader.UPSTaxInvoiceMenuName, BusinessContext.CusHAWB, true, "\"<BusinessObjectType>\" == \"UPECallout\" &&  \"<IsInReBillQueue>\" == \"N\" && \"<RegistryItem(AdditionalRegistryItemSet.Instance.EnableUPECustomisationsItem)>\" == \"True\"");
		}

		public void TestLoadUPSTaxInvoiceQueueForBatchPrint()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			CreateDecoyAirCargoMenuItems(UPEDocumentMenuItemLoader.UPSTaxInvoiceQueueForBatchPrintMenuName);
			DocumentCommand documentCommand = Loader.LoadUPSTaxInvoiceQueueForBatchPrint();
			AssertCorrectDocumentCommand(documentCommand, UPEDocumentMenuItemLoader.UPSTaxInvoiceQueueForBatchPrintMenuName, BusinessContext.CusHAWB, true, "\"<BusinessObjectType>\" == \"UPECallout\" &&  \"<IsInReBillQueue>\" == \"N\" && \"<RegistryItem(AdditionalRegistryItemSet.Instance.EnableUPECustomisationsItem)>\" == \"True\"");
		}

		#endregion
		#region Alternate Broker

		public void TestLoadAlternateBrokerDocumentPack()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			DocumentCommand documentCommand = Loader.LoadAlternateBrokerDocumentPack();
			AssertCorrectDocumentCommand(documentCommand, UPEDocumentMenuItemLoader.AlternateBrokerDocumentPackMenuName, BusinessContext.Customs, true, "");
			AssertEquals("Alternate Broker Document Pack should be auto-deliverable", false, documentCommand.SU_PreventAutoDelivery);
			AssertEquals("Air Arrival Notice", documentCommand.Documents[0].SI_DocumentTitle);
			AssertEquals("Commerical Invoice (DIS)", documentCommand.Documents[1].SI_DocumentTitle);
			AssertEquals("Tax Invoice", documentCommand.Documents[2].SI_DocumentTitle);
			AssertEquals("Commercial Invoice (from DIS) must not be delivered if it doesn't exist", "PrintStandardInvoice=Y", documentCommand.Documents[1].SI_MenuTemplateFilter);
			AssertEquals("The tax invoice must be filtered by whether ITF/freight is chargable to the consignee", "PrintClientSpecificInvoice=Y", documentCommand.Documents[2].SI_MenuTemplateFilter);
		}

		public void TestLoadAlternateBrokerSplitNotification()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			DocumentCommand documentCommand = Loader.LoadAlternateBrokerSplitNotification();
			AssertCorrectDocumentCommand(documentCommand, UPEDocumentMenuItemLoader.AlternateBrokerSplitNotificationMenuName, BusinessContext.CusHAWB, true, "'<IsAlternateBrokerSplitShipment>' == 'Y' && '<RegistryItem(AdditionalRegistryItemSet.Instance.EnableUPECustomisationsItem)>' == 'True'".Replace("'", "\""));
			AssertEquals("Alternate Broker Split Notification should be auto-deliverable", false, documentCommand.SU_PreventAutoDelivery);
		}

		#endregion
		#region Letter of Authority

		public void TestLoadLetterOfAuthority()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			DocumentCommand documentCommand = Loader.LoadLetterOfAuthority();
			AssertCorrectDocumentCommand(documentCommand, UPEDocumentMenuItemLoader.LetterOfAuthorityMenuName, BusinessContext.Organisation, true, "");
			AssertEquals("Letter of Authority should be for manual delivery only", true, documentCommand.SU_PreventAutoDelivery);
		}

		#endregion
		#region Electronic Credit Note

		public void TestLoadElectronicCreditNote()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			CreateDecoyAirCargoMenuItems(UPEDocumentMenuItemLoader.ElectronicCreditNote);
			DocumentCommand documentCommand = Loader.LoadElectronicCreditNote();
			AssertCorrectDocumentCommand(documentCommand, UPEDocumentMenuItemLoader.ElectronicCreditNote, BusinessContext.Customs, true, "");
			AssertEquals("Electronic Credit Note should be for auto delivery", true, documentCommand.SU_PreventAutoDelivery);
		}

		#endregion
		#region Shipment Held Letter
		// There are 6 StmMenuItems for �Shipment Held Letter� (Customer Notifications):
		//
		// CusHAWB (finance) for importer
		// CusHAWB (finance) for consignor
		// CusHAWB (customs) for importer
		// CusHAWB (customs) for consignor
		// Declaration for importer
		// Declaration for consignor
		//
		// These first 4 are all different documents, the last 2 are the same as the CusHAWB (customs) documents, but had to be put into different menu items so they can be shown on the dec menu.

		public void TestIsShipmentHeldLetter()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			TestIsShipmentHeldLetter(true, new UPEDocumentMenuItemLoader(Factory).LoadCusHAWBHeldLetter(ShipmentHeldLetterRecipient.Consignee));
			TestIsShipmentHeldLetter(true, new UPEDocumentMenuItemLoader(Factory).LoadCusHAWBHeldLetter(ShipmentHeldLetterRecipient.Consignor));
			TestIsShipmentHeldLetter(true, new UPEDocumentMenuItemLoader(Factory).LoadFinanceHeldLetter(ShipmentHeldLetterRecipient.Consignee));
			TestIsShipmentHeldLetter(true, new UPEDocumentMenuItemLoader(Factory).LoadFinanceHeldLetter(ShipmentHeldLetterRecipient.Consignor));
			TestIsShipmentHeldLetter(false, new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoice());
			TestIsShipmentHeldLetter(false, new UPEDocumentMenuItemLoader(Factory).LoadAlternateBrokerDocumentPack());
		}

		public void TestIsShipmentHeldLetter(bool expectedIsShipmentHeldLetter, StmMenuItem menuItem)
		{
			AssertEquals(menuItem.SU_MenuName, expectedIsShipmentHeldLetter, UPEDocumentMenuItemLoader.IsShipmentHeldLetter(menuItem));
		}

		public void TestLoadCusHAWBHeldLetter_ForConsignee()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			CreateDecoyAirCargoMenuItems(UPEDocumentMenuItemLoader.ShipmentHeldLetterForConsigneeMenuName, "UPECallout");
			CreateDecoyAirCargoMenuItems(UPEDocumentMenuItemLoader.ShipmentHeldLetterForConsignorMenuName, "UPECallout");
			DocumentCommand documentCommand = Loader.LoadCusHAWBHeldLetter(ShipmentHeldLetterRecipient.Consignee);
			AssertCorrectDocumentCommand(documentCommand, UPEDocumentMenuItemLoader.ShipmentHeldLetterForConsigneeMenuName, BusinessContext.CusHAWB, true, "\"<BusinessObjectType>\" == \"UPECusHAWB\" && \"<RegistryItem(AdditionalRegistryItemSet.Instance.EnableUPECustomisationsItem)>\" == \"True\"");
			AssertEquals("Shipment Held Letter for Cargo Report should be auto-deliverable", false, documentCommand.SU_PreventAutoDelivery);
		}

		public void TestLoadCusHAWBHeldLetter_ForConsignor()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			CreateDecoyAirCargoMenuItems(UPEDocumentMenuItemLoader.ShipmentHeldLetterForConsigneeMenuName, "UPECallout");
			CreateDecoyAirCargoMenuItems(UPEDocumentMenuItemLoader.ShipmentHeldLetterForConsignorMenuName, "UPECallout");
			DocumentCommand documentCommand = Loader.LoadCusHAWBHeldLetter(ShipmentHeldLetterRecipient.Consignor);
			AssertCorrectDocumentCommand(documentCommand, UPEDocumentMenuItemLoader.ShipmentHeldLetterForConsignorMenuName, BusinessContext.CusHAWB, true, "\"<BusinessObjectType>\" == \"UPECusHAWB\" && \"<RegistryItem(AdditionalRegistryItemSet.Instance.EnableUPECustomisationsItem)>\" == \"True\"");
			AssertEquals("Shipment Held Letter for Cargo Report should be auto-deliverable", false, documentCommand.SU_PreventAutoDelivery);
		}

		public void TestLoadFinanceHeldLetter_ForConsignee()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			CreateDecoyAirCargoMenuItems(UPEDocumentMenuItemLoader.ShipmentHeldLetterForConsigneeMenuName, "UPECusHAWB");
			CreateDecoyAirCargoMenuItems(UPEDocumentMenuItemLoader.ShipmentHeldLetterForConsignorMenuName, "UPECusHAWB");
			DocumentCommand documentCommand = Loader.LoadFinanceHeldLetter(ShipmentHeldLetterRecipient.Consignee);
			AssertCorrectDocumentCommand(documentCommand, UPEDocumentMenuItemLoader.ShipmentHeldLetterForConsigneeMenuName, BusinessContext.CusHAWB, true, "\"<BusinessObjectType>\" == \"UPECallout\" && \"<RegistryItem(AdditionalRegistryItemSet.Instance.EnableUPECustomisationsItem)>\" == \"True\"");
			AssertEquals("Shipment Held Letter for Finance should be for manual delivery only", true, documentCommand.SU_PreventAutoDelivery);
		}

		public void TestLoadFinanceHeldLetter_ForConsignor()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			CreateDecoyAirCargoMenuItems(UPEDocumentMenuItemLoader.ShipmentHeldLetterForConsigneeMenuName, "UPECusHAWB");
			CreateDecoyAirCargoMenuItems(UPEDocumentMenuItemLoader.ShipmentHeldLetterForConsignorMenuName, "UPECusHAWB");
			DocumentCommand documentCommand = Loader.LoadFinanceHeldLetter(ShipmentHeldLetterRecipient.Consignor);
			AssertCorrectDocumentCommand(documentCommand, UPEDocumentMenuItemLoader.ShipmentHeldLetterForConsignorMenuName, BusinessContext.CusHAWB, true, "\"<BusinessObjectType>\" == \"UPECallout\" && \"<RegistryItem(AdditionalRegistryItemSet.Instance.EnableUPECustomisationsItem)>\" == \"True\"");
			AssertEquals("Shipment Held Letter for Finance should be for manual delivery only", true, documentCommand.SU_PreventAutoDelivery);
		}

		public void TestLoadJobDeclarationHeldLetter_ForConsignee()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			DocumentCommand documentCommand = Loader.LoadJobDeclarationHeldLetter(ShipmentHeldLetterRecipient.Consignee);
			AssertCorrectDocumentCommand(documentCommand, UPEDocumentMenuItemLoader.ShipmentHeldLetterForConsigneeMenuName, BusinessContext.Customs, true, "");
			AssertEquals("Shipment Held Letter for Declaration should be for manual delivery only", true, documentCommand.SU_PreventAutoDelivery);
		}

		public void TestLoadJobDeclarationHeldLetter_ForConsignor()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			DocumentCommand documentCommand = Loader.LoadJobDeclarationHeldLetter(ShipmentHeldLetterRecipient.Consignor);
			AssertCorrectDocumentCommand(documentCommand, UPEDocumentMenuItemLoader.ShipmentHeldLetterForConsignorMenuName, BusinessContext.Customs, true, "");
			AssertEquals("Shipment Held Letter for Declaration should be for manual delivery only", true, documentCommand.SU_PreventAutoDelivery);
		}

		#endregion
		#region Implementation
		void AssertCorrectDocumentCommand(DocumentCommand documentCommand, ZString expectedMenuName, BusinessContext expectedBusinessContext, ZBool expectedIsClientSpecific, ZString expectedFilterList)
		{
			AssertEquals("MenuItem loaded should have the correct BusinessContext", expectedBusinessContext.ToString(), documentCommand.SU_BusinessContext);
			AssertEquals("MenuItem loaded should be system defined", true, documentCommand.SU_IsSystemDefined);
			AssertEquals("MenuItem loaded should be client specific", expectedIsClientSpecific, documentCommand.SU_IsClientSpecific);
			AssertEquals("MenuItem should have correct name", expectedMenuName, documentCommand.SU_MenuName);
			if (!expectedFilterList.IsEmpty)
			{
				AssertEquals("MenuItem should have correct filter list", expectedFilterList, documentCommand.SU_FilterList);
			}
		}

		void CreateDecoyAirCargoMenuItems(string menuName)
		{
			CreateDecoyAirCargoMenuItems(menuName, "");
		}

		void CreateDecoyAirCargoMenuItems(string menuName, ZString decoyFilterList)
		{
			StmMenuItemBase decoyMenuItem = Factory.New<StmMenuItemBase>();
			decoyMenuItem.SU_MenuName = menuName;
			decoyMenuItem.SU_BusinessContext = nameof(BusinessContext.CusHAWB);
			decoyMenuItem.SU_IsClientSpecific = false;
			decoyMenuItem.SU_IsSystemDefined = true;
			StmMenuItemBase decoyMenuItem2 = Factory.New<StmMenuItemBase>();
			decoyMenuItem2.SU_MenuName = menuName;
			decoyMenuItem2.SU_BusinessContext = nameof(BusinessContext.CusHAWB);
			decoyMenuItem2.SU_IsClientSpecific = true;
			decoyMenuItem2.SU_IsSystemDefined = false;
			StmMenuItemBase decoyMenuItem3 = Factory.New<StmMenuItemBase>();
			decoyMenuItem3.SU_MenuName = menuName;
			decoyMenuItem3.SU_BusinessContext = "xxx";
			decoyMenuItem3.SU_IsClientSpecific = true;
			decoyMenuItem3.SU_IsSystemDefined = true;
			if (!decoyFilterList.IsEmpty)
			{
				StmMenuItemBase decoyMenuItem4 = Factory.New<StmMenuItemBase>();
				decoyMenuItem4.SU_MenuName = menuName;
				decoyMenuItem4.SU_BusinessContext = nameof(BusinessContext.CusHAWB);
				decoyMenuItem4.SU_IsClientSpecific = true;
				decoyMenuItem4.SU_IsSystemDefined = true;
				decoyMenuItem4.SU_FilterList = decoyFilterList;
			}
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new UPEDocumentMenuItemLoader(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Loader = new UPEDocumentMenuItemLoader(Factory);
		}

		UPEDocumentMenuItemLoader Loader;
		#endregion
	}
}
