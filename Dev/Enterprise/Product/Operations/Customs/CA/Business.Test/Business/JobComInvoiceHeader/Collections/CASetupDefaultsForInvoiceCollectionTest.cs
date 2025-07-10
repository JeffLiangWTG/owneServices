using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CASetupDefaultsForInvoiceCollectionTest : TestCaseWithFactory
	{
		public void TestDefaultsForFirstInvoiceCore()
		{
			var testHelper = new DeclarationTestHelper(Factory, true);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			testHelper.CreateTransportLeg(declaration.Transports, testHelper.AUBNE.Code, testHelper.AUMEL.Code, ZDateTime.Now, ZDateTime.Now.AddDays(1));
			var importLeg = testHelper.CreateTransportLeg(declaration.Transports, testHelper.AUMEL.Code, testHelper.CATOR.Code, ZDateTime.Now.AddDays(2), ZDateTime.Now.AddDays(3));
			testHelper.CreateTransportLeg(declaration.Transports, testHelper.CATOR.Code, testHelper.CAVAR.Code, ZDateTime.Now.AddDays(4), ZDateTime.Now.AddDays(5));

			var firstInvoice = declaration.Invoices.AddNew();
			AssertEquals("CA_RL_NKLastPort", testHelper.AUMEL.Code, firstInvoice.CA_RL_NKLastPort);
			AssertEquals("JZ_ValuationDateOverride", ZDateTime.TruncateToDay(importLeg.JW_ETD), firstInvoice.JZ_ValuationDateOverride);
			AssertEquals("CA_TreatmentCode", "02", firstInvoice.CA_TreatmentCode);

			var shipment = Factory.New<ForwardingShipment>();
			testHelper.CreateTransportLeg(shipment.Transports, testHelper.AUBNE.Code, testHelper.AUMEL.Code, ZDateTime.Now, ZDateTime.Now.AddDays(1));
			importLeg = testHelper.CreateTransportLeg(shipment.Transports, testHelper.AUMEL.Code, testHelper.CATOR.Code, ZDateTime.Now.AddDays(2), ZDateTime.Now.AddDays(3));
			testHelper.CreateTransportLeg(shipment.Transports, testHelper.CATOR.Code, testHelper.CAVAR.Code, ZDateTime.Now.AddDays(4), ZDateTime.Now.AddDays(5));
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;

			firstInvoice = declaration.Invoices.AddNew();
			AssertEquals("CA_RL_NKLastPort", testHelper.AUMEL.Code, firstInvoice.CA_RL_NKLastPort);
			AssertEquals("JZ_ValuationDateOverride", ZDateTime.TruncateToDay(importLeg.JW_ETD), firstInvoice.JZ_ValuationDateOverride);
		}

		public void TestDefaultsForAdditionalInvoiceCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var firstInvoice = declaration.Invoices.AddNew();
			firstInvoice.JZ_RN_NKDefaultOrigin = "XX";
			firstInvoice.JZ_RW_NKOriginState = "YY";
			firstInvoice.CA_RN_NKExport = "US";
			firstInvoice.CA_USStateOfExport = "CA";
			firstInvoice.CA_RN_NKTranshipment = "SG";
			firstInvoice.BuyerDocumentaryAddress.E2_OA_Address = Factory.New<OrgHeader>().Addresses.AddNew().PK;
			firstInvoice.ExporterDocumentaryAddress.E2_OA_Address = Factory.New<OrgHeader>().Addresses.AddNew().PK;
			firstInvoice.SupplierPickupDeliveryAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
			firstInvoice.JZ_OA_ManufacturerAddress = Factory.New<OrgHeader>().Addresses.AddNew().PK;
			firstInvoice.CA_DepartmentRuling = "DR";
			firstInvoice.CA_RL_NKLastPort = "NZAKL";
			firstInvoice.CA_ConditionsOfSale = "CS";
			firstInvoice.CA_TermsOfPayment = "TP";
			firstInvoice.CA_ServicesInd = true;
			firstInvoice.CA_RoyaltyInd = false;
			firstInvoice.CA_OtherReference = "OR";
			firstInvoice.CA_TradeZone = "100B";
			firstInvoice.CA_USPortOfExit = "1010";
			firstInvoice.CA_TreatmentCode = "";
			firstInvoice.CA_TimeLimit = 6;
			firstInvoice.CA_TimeLimitCode = "D";
			firstInvoice.CA_ValueForDutyCode = "13";
			firstInvoice.JZ_ValuationDateOverride = ZDateTime.Now.AddDays(-1);

			var secondInvoice = declaration.Invoices.AddNew();
			AssertEquals("JZ_RN_NKDefaultOrigin", firstInvoice.JZ_RN_NKDefaultOrigin, secondInvoice.JZ_RN_NKDefaultOrigin);
			AssertEquals("JZ_RW_NKOriginState", firstInvoice.JZ_RW_NKOriginState, secondInvoice.JZ_RW_NKOriginState);
			AssertEquals("CA_RN_NKExport", firstInvoice.CA_RN_NKExport, secondInvoice.CA_RN_NKExport);
			AssertEquals("CA_USStateOfExport", firstInvoice.CA_USStateOfExport, secondInvoice.CA_USStateOfExport);
			AssertEquals("CA_RN_NKTranshipment", firstInvoice.CA_RN_NKTranshipment, secondInvoice.CA_RN_NKTranshipment);
			AssertEquals("JZ_OH_Buyer", firstInvoice.JZ_OH_Buyer, secondInvoice.JZ_OH_Buyer);
			AssertEquals("BuyerDocumentaryAddress", firstInvoice.BuyerDocumentaryAddress.E2_OA_Address, secondInvoice.BuyerDocumentaryAddress.E2_OA_Address);
			AssertEquals("JZ_OH_Consignee", firstInvoice.JZ_OH_Consignee, secondInvoice.JZ_OH_Consignee);
			AssertEquals("FinalConsigneeAddress", firstInvoice.FinalConsigneeAddress.E2_OA_Address, secondInvoice.FinalConsigneeAddress.E2_OA_Address);
			AssertEquals("ExporterDocumentaryAddress", firstInvoice.ExporterDocumentaryAddress.OrganisationPK, secondInvoice.ExporterDocumentaryAddress.OrganisationPK);
			AssertEquals("ExportBrokerDocumentaryAddress", firstInvoice.ExporterDocumentaryAddress.E2_OA_Address, secondInvoice.ExporterDocumentaryAddress.E2_OA_Address);
			AssertEquals("JZ_OA_ManufacturerAddress", firstInvoice.JZ_OA_ManufacturerAddress, secondInvoice.JZ_OA_ManufacturerAddress);
			AssertEquals("CA_DepartmentRuling", "DR", secondInvoice.CA_DepartmentRuling);
			AssertEquals("CA_RL_NKLastPort", "NZAKL", secondInvoice.CA_RL_NKLastPort);
			AssertEquals("CA_ConditionsOfSale", "CS", secondInvoice.CA_ConditionsOfSale);
			AssertEquals("CA_TermsOfPayment", "TP", secondInvoice.CA_TermsOfPayment);
			AssertEquals("CA_ServicesInd", true, secondInvoice.CA_ServicesInd);
			AssertEquals("CA_RoyaltyInd", false, secondInvoice.CA_RoyaltyInd);
			AssertEquals("CA_OtherReference", "OR", secondInvoice.CA_OtherReference);
			AssertEquals("CA_TradeZone", "100B", secondInvoice.CA_TradeZone);
			AssertEquals("CA_USPortOfExit", "1010", secondInvoice.CA_USPortOfExit);
			AssertEquals("CA_TreatmentCode", "02", secondInvoice.CA_TreatmentCode);
			AssertEquals("CA_TimeLimit", 6, secondInvoice.CA_TimeLimit);
			AssertEquals("CA_TimeLimitCode", "D", secondInvoice.CA_TimeLimitCode);
			AssertEquals("CA_ValueForDutyCode", "13", secondInvoice.CA_ValueForDutyCode);
			AssertEquals("JZ_ValuationDateOverride", firstInvoice.JZ_ValuationDateOverride, secondInvoice.JZ_ValuationDateOverride);

			secondInvoice.CA_TreatmentCode = "03";
			var thirdInvoice = declaration.Invoices.AddNew();
			AssertEquals("CA_TreatmentCode", "03", secondInvoice.CA_TreatmentCode);
		}

		public void TestDefaultSupplierDocumentaryAddress()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignorAddress = consignor.Addresses.AddNew();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Supplier = consignor.PK;
			declaration.SupplierDocumentaryAddress.E2_OA_Address = consignorAddress.PK;
			var invoice = declaration.Invoices.AddNew();
			AssertEquals("Defaults from Main Vendor Address", consignor.PK, invoice.SupplierDocumentaryAddress.OrganisationPK);
			AssertEquals("Defaults from Main Vendor Address", consignorAddress.PK, invoice.SupplierDocumentaryAddress.E2_OA_Address);
			invoice = declaration.Invoices.AddNew();
			AssertEquals("Defaults from Main Vendor Address", consignor.PK, invoice.SupplierDocumentaryAddress.OrganisationPK);
			AssertEquals("Defaults from Main Vendor Address", consignorAddress.PK, invoice.SupplierDocumentaryAddress.E2_OA_Address);

			declaration.SupplierDocumentaryAddress.E2_AddressOverride = true;
			declaration.SupplierDocumentaryAddress.E2_CompanyName = "TEST COMPANY";
			declaration.SupplierDocumentaryAddress.Address1 = "TEST COMPANY ADRESS 1";
			invoice = declaration.Invoices.AddNew();
			AssertEquals("Defaults from Overriden Main Vendor Address", "TEST COMPANY", invoice.SupplierDocumentaryAddress.E2_CompanyName);
			AssertEquals("Defaults from Overriden Main Vendor Address", "TEST COMPANY ADRESS 1", invoice.SupplierDocumentaryAddress.Address1);
		}

		public void TestDefaultSupplierPickupDeliveryAddress()
		{
			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			var shipperAddress = shipper.Addresses.AddNew();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Supplier = shipper.PK;
			declaration.SupplierPickupAddress.E2_OA_Address = shipperAddress.PK;
			var invoice = declaration.Invoices.AddNew();
			AssertEquals("Defaults from SupplierPickupAddress", shipper.PK, invoice.SupplierPickupDeliveryAddress.OrganisationPK);
			AssertEquals("Defaults from SupplierPickupAddress", shipperAddress.PK, invoice.SupplierPickupDeliveryAddress.E2_OA_Address);
			invoice = declaration.Invoices.AddNew();
			AssertEquals("Defaults from SupplierPickupAddress", shipper.PK, invoice.SupplierPickupDeliveryAddress.OrganisationPK);
			AssertEquals("Defaults from SupplierPickupAddress", shipperAddress.PK, invoice.SupplierPickupDeliveryAddress.E2_OA_Address);

			declaration.SupplierPickupAddress.E2_AddressOverride = true;
			declaration.SupplierPickupAddress.E2_CompanyName = "TEST COMPANY";
			declaration.SupplierPickupAddress.Address1 = "TEST COMPANY ADRESS 1";
			invoice = declaration.Invoices.AddNew();
			AssertEquals("Defaults from Overriden SupplierPickupAddress", "TEST COMPANY", invoice.SupplierPickupDeliveryAddress.E2_CompanyName);
			AssertEquals("Defaults from Overriden SupplierPickupAddress", "TEST COMPANY ADRESS 1", invoice.SupplierPickupDeliveryAddress.Address1);
		}

		public void TestDefaultFinalConsigneeAddress()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			var invoice = declaration.Invoices.AddNew();
			AssertEquals("Defaults from FinalConsigneeAddress", importer.PK, invoice.FinalConsigneeAddress.OrganisationPK);
			AssertEquals("Defaults from FinalConsigneeAddress", importer.MainAddress.PK, invoice.FinalConsigneeAddress.E2_OA_Address);
			invoice.FinalConsigneeAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			invoice = declaration.Invoices.AddNew();
			AssertEquals("Defaults from FinalConsigneeAddress", importer.PK, invoice.FinalConsigneeAddress.OrganisationPK);
			AssertEquals("Defaults from FinalConsigneeAddress", importer.MainAddress.PK, invoice.FinalConsigneeAddress.E2_OA_Address);
		}
	}
}
