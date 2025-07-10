using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.CN.Business.Testing
{
	class JobDeclarationSynchroniserTest : Customs.Business.Testing.JobDeclarationSynchroniserTest
	{
		public void TestHouseBillNumberSyncWithBKGNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "CNSHA";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("", declaration.JE_HouseBill);
			var cusEntryNumber = shipment.Numbers.AddNew();
			cusEntryNumber.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			cusEntryNumber.CE_EntryNum = "111111";
			AssertEquals("111111", declaration.JE_HouseBill);
			cusEntryNumber.CE_EntryNum = "222222";
			AssertEquals("222222", declaration.JE_HouseBill);
			cusEntryNumber.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS;
			AssertEquals("", declaration.JE_HouseBill);
			shipment.JS_HouseBill = "333333";
			AssertEquals("333333", declaration.JE_HouseBill);
			cusEntryNumber.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			AssertEquals("222222", declaration.JE_HouseBill);
			var cusEntryNumber2 = shipment.Numbers.AddNew();
			cusEntryNumber2.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			AssertEquals("222222", declaration.JE_HouseBill);
			cusEntryNumber2.CE_EntryNum = "444444";
			AssertEquals("333333", declaration.JE_HouseBill);
			shipment.Numbers.RemoveAndDelete(cusEntryNumber2);
			AssertEquals("222222", declaration.JE_HouseBill);
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("333333", declaration.JE_HouseBill);
		}

		public void TestHookSupplierAndImporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "CNSHA";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = supplier.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = importer.PK;
			declaration.JE_JS = shipment.PK;
			declaration.JE_OverrideFreightDefaults = true;
			Assert(!declaration.SupplierDocumentaryAddress.E2_AddressOverrideInfo.ReadOnly);
			Assert(!declaration.SupplierDocumentaryAddress.OrganisationPKInfo.ReadOnly);
			Assert(!declaration.ImporterDocumentaryAddress.E2_AddressOverrideInfo.ReadOnly);
			Assert(!declaration.ImporterDocumentaryAddress.OrganisationPKInfo.ReadOnly);
			declaration.JE_OverrideFreightDefaults = false;
			Assert(declaration.SupplierDocumentaryAddress.E2_AddressOverrideInfo.ReadOnly);
			Assert(declaration.SupplierDocumentaryAddress.OrganisationPKInfo.ReadOnly);
			Assert(declaration.ImporterDocumentaryAddress.E2_AddressOverrideInfo.ReadOnly);
			Assert(declaration.ImporterDocumentaryAddress.OrganisationPKInfo.ReadOnly);
		}
	}
}
