using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(FreightWrapperFromDeclaration))]
	class CAFreightWrapperFromDeclarationTest : FreightWrapperTest
	{
		public override void TestOrgWrappersReturnTypesOnEmptyWrapper()
		{
			Assert(true);
		}

		public void TestCASpecificProperties()
		{
			var carrier = Factory.New<Universal.ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "Code";
			carrier.ZZ4_Description = "Carrier Name";
			carrier.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Canada;
			carrier.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var universalReferenceHelper = new Universal.Testing.UniversalReferenceTestDataHelper(newFactory);
			var code = universalReferenceHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Code", "Description", startDate, endDate);
			newFactory.Save();

			var importer1 = AddNewDebtor("IMP1");
			var importer2 = AddNewDebtor("IMP2");
			var importer3 = AddNewDebtor("IMP3");
			var billTo = AddNewDebtor("BILLTO");
			importer3.SetRelatedParty(billTo, RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyDirectionList.Codes.Delivery, Enterprise.Core.Constants.TransportModes.All, ZString.Empty);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			CusEntryNumber.New(declaration, CanadaAdditionalReferenceNumberTypes.Codes.PCN, declaration.CountryCode).CE_EntryNum = "12345XX";
			declaration.JE_OwnerRef = "OWNERREF";
			declaration.JE_GoodsDescription = "GOODS DESCRIPTION";
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "00006789";
			declaration.JE_RL_NKFinalDestination = "CATOR";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INVNUM";
			invoice.CA_USPortOfExit = "Code";
			var helper = new DeclarationTestHelper(Factory, true);
			declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "CCN3";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			entry.Messages.Add(helper.GetEDIReleaseResponseMessage("CCN3", "1"));
			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			SetDebtor(wrapper, importer1);
			AssertEquals("ReleaseStatuses.Count", 1, wrapper.CAReleaseStatus.Count);
			AssertEquals("CAPreviousCCN", "12345XX", wrapper.CAPreviousCCN);
			AssertEquals("CATransactionNo", "12345000067897", wrapper.CATransactionNo);
			Assert("CAHideCarrier", !wrapper.CAHideCarrier);

			AssertEquals("Invoice Numbers", 1, wrapper.CommercialInvoices.Count);
			AssertEquals("Owners Ref", "OWNERREF", wrapper.OrderNumbersWithOwnersReference);
			AssertEquals("GoodsDescription", "GOODS DESCRIPTION", wrapper.GoodsDescription);
			AssertEquals("DESTINATION", "CATOR - Toronto", wrapper.Destination.ToString());

			AssertEquals("CargoControlNumberForCanada", "CCN3", wrapper.CargoControlNumberForCanada);
			AssertEquals("PreviousCargoControlNumberForCanada", "12345XX", wrapper.PreviousCargoControlNumberForCanada);
			declaration.JE_CarrierCode = "Code";
			AssertEquals("CACarrierName", "Code - Carrier Name", wrapper.CACarrierName);
			AssertEquals("CAUSPortOfExit", "Code - Description", wrapper.CAUSPortOfExit);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 2, 1);
			declaration.JE_RL_NKFinalDestination = "CATOR";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "ID1";
			invoice1.JZ_OH_Buyer = importer1.PK;
			invoice1.InvoiceLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "ID2";
			invoice2.JZ_OH_Buyer = importer1.PK;
			invoice2.InvoiceLines.AddNew();
			var invoice3 = declaration.Invoices.AddNew();
			invoice3.JZ_InvoiceNumber = "ID3";
			invoice3.JZ_OH_Buyer = importer1.PK;
			invoice3.InvoiceLines.AddNew();
			var invoice4 = declaration.Invoices.AddNew();
			invoice4.JZ_InvoiceNumber = "ID4";
			invoice4.JZ_OH_Buyer = importer2.PK;
			invoice4.InvoiceLines.AddNew();
			var invoice5 = declaration.Invoices.AddNew();
			invoice5.JZ_InvoiceNumber = "ID5";
			invoice5.JZ_OH_Buyer = importer2.PK;
			invoice5.InvoiceLines.AddNew();
			var invoice6 = declaration.Invoices.AddNew();
			invoice6.JZ_InvoiceNumber = "ID6";
			invoice6.JZ_OH_Buyer = importer3.PK;
			invoice6.InvoiceLines.AddNew();
			declaration.DoMerge();

			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			SetDebtor(wrapper, importer1);
			AssertEquals("Invoice Numbers", 0, wrapper.CommercialInvoices.Count);
			AssertEquals("Owners Ref", "ID1,ID2,ID3", wrapper.OrderNumbersWithOwnersReference);
			AssertEquals("GoodsDescription", "Various low value shipments", wrapper.GoodsDescription);
			AssertEquals("DESTINATION", string.Empty, wrapper.Destination.ToString());
			SetDebtor(wrapper, importer2);
			AssertEquals("Invoice Numbers", 0, wrapper.CommercialInvoices.Count);
			AssertEquals("Owners Ref", "ID4,ID5", wrapper.OrderNumbersWithOwnersReference);
			AssertEquals("GoodsDescription", "Various low value shipments", wrapper.GoodsDescription);
			AssertEquals("DESTINATION", string.Empty, wrapper.Destination.ToString());
			SetDebtor(wrapper, billTo);
			AssertEquals("Invoice Numbers", 0, wrapper.CommercialInvoices.Count);
			AssertEquals("Owners Ref", "ID6", wrapper.OrderNumbersWithOwnersReference);
			AssertEquals("GoodsDescription", "Various low value shipments", wrapper.GoodsDescription);
			AssertEquals("DESTINATION", string.Empty, wrapper.Destination.ToString());

			AssertEquals("CargoControlNumberForCanada", string.Empty, wrapper.CargoControlNumberForCanada);
			AssertEquals("PreviousCargoControlNumberForCanada", string.Empty, wrapper.PreviousCargoControlNumberForCanada);
			AssertEquals("CACarrierName", string.Empty, wrapper.CACarrierName);
			AssertEquals("CAUSPortOfExit", string.Empty, wrapper.CAUSPortOfExit);

			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			SetDebtor(wrapper, importer1);
			AssertEquals("Invoice Numbers", 0, wrapper.CommercialInvoices.Count);
			AssertEquals("Owners Ref", "ID1,ID2,ID3,ID4,ID5,ID6", wrapper.OrderNumbersWithOwnersReference);
			AssertEquals("GoodsDescription", "Various low value shipments", wrapper.GoodsDescription);
			AssertEquals("DESTINATION", string.Empty, wrapper.Destination.ToString());

			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			var invoiceNull = Factory.New<ARInvoice>();
			wrapper.SetARInvoice(DocARInvoice.New(invoiceNull, Factory));
			ZString ownerRef = ZString.Empty;
			AssertNoExceptionThrown("Null debtor should not throw an exception", delegate
			{ ownerRef = wrapper.OrderNumbersWithOwnersReference; });

			declaration.Invoices.DeleteAll();
			invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "ID1";
			invoice1.InvoiceLines.AddNew();
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			SetDebtor(wrapper, importer1);
			AssertNoExceptionThrown("Null buyer should not throw an exception", delegate
			{ ownerRef = wrapper.OrderNumbersWithOwnersReference; });

			AssertEquals("Owners Ref", ZString.Empty, ownerRef);
		}

		OrgHeader AddNewDebtor(ZString code)
		{
			var result = Factory.New<OrgHeader>();
			result.OH_Code = code;
			result.OH_IsConsignee = true;
			result.OH_IsDebtor = true;
			return result;
		}

		void SetDebtor(FreightWrapperFromDeclaration wrapper, OrgHeader debtor)
		{
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = debtor.PK;
			wrapper.SetARInvoice(DocARInvoice.New(invoice, Factory));
		}
	}
}
