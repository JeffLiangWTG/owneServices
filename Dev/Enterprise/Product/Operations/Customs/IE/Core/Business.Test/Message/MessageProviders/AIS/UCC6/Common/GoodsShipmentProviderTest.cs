using System;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class GoodsShipmentProviderTest : DataProviderTestCase<GoodsShipmentProvider>
	{
		public void TestIGoodsShipment()
		{
			Assert("Should implement IGoodsShipment", Provider is IGoodsShipment);
		}

		public void TestCountryOfDispatch()
		{
			SetUpTestData();
			declaration.JE_GoodsOrigin = "IE";
			AssertEquals("IE", Provider.CountryOfDispatch);
		}

		public void TestDestination()
		{
			SetUpTestData();
			declaration.ZG_RegionOfDestination = "75C";
			var destination = Provider.Destination;
			AssertEquals("CountryOfDestination", "", destination.CountryOfDestination);
			AssertEquals("RegionOfDestination", "75C", destination.RegionOfDestination);
			AssertSame("Cached", destination, Provider.Destination);
		}

		public void TestWarehouse_Normal()
		{
			SetUpTestData();

			var toWarehouseOwner = Factory.New<OrgHeader>();
			var toWarehouseOwnerAddress = toWarehouseOwner.MainAddress;
			toWarehouseOwnerAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "U18033IE", Core.Constants.CountryCodes.Ireland);
			entryInstruction.CEI_OA_Warehouse2 = toWarehouseOwnerAddress.PK;

			var toWarehouseUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			toWarehouseUsage.AGC_Code = "CW1";
			toWarehouseUsage.AGC_Number = "R17017IE";
			toWarehouseUsage.AGC_OH_Owner = toWarehouseOwner.PK;

			var warehouse = Provider.Warehouse;
			AssertEquals("Type", "R", warehouse.Type);
			AssertEquals("Id", "U18033IE", warehouse.Id);
			AssertSame("Cached", warehouse, Provider.Warehouse);
		}

		public void TestWarehouse_WhenTypeIsEmpty()
		{
			SetUpTestData();

			var toWarehouseOwner = Factory.New<OrgHeader>();
			var toWarehouseOwnerAddress = toWarehouseOwner.MainAddress;
			toWarehouseOwnerAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "U18033IE", Core.Constants.CountryCodes.Ireland);
			entryInstruction.CEI_OA_Warehouse2 = toWarehouseOwnerAddress.PK;

			AssertNull(Provider.Warehouse);
		}

		public void TestWarehouse_WhenIdIsEmpty()
		{
			SetUpTestData();

			var toWarehouseOwner = Factory.New<OrgHeader>();
			entryInstruction.CEI_OA_Warehouse2 = toWarehouseOwner.MainAddress.PK;

			var toWarehouseUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			toWarehouseUsage.AGC_Code = "CW1";
			toWarehouseUsage.AGC_Number = "R17017IE";
			toWarehouseUsage.AGC_OH_Owner = toWarehouseOwner.PK;

			AssertNull(Provider.Warehouse);
		}

		public void TestConsignment()
		{
			SetUpTestData();
			var consignment = Provider.Consignment;
			AssertNotNull(consignment);
			AssertSame("Cached", consignment, Provider.Consignment);
		}

		public void TestGoodsShipmentItems()
		{
			SetUpTestData();
			var entryLine2 = entryHeader.MergedLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			var goodsShipmentItems = Provider.GoodsShipmentItems;
			AssertEquals(2, goodsShipmentItems.Count);
			AssertType<GoodsShipmentItemProvider>(goodsShipmentItems.First());
		}

		public void TestNatureOfTransaction()
		{
			SetUpTestData();
			invoice.JZ_ValuationCode = "AI";
			AssertEquals("NatureOfTransaction", "AI", GetProvider().NatureOfTransaction);
		}

		public void TestTotalAmountInvoiced()
		{
			SetUpTestData();
			invoice.JZ_InvoiceAmount = 12;
			AssertEquals("TotalAmountInvoiced", 12m, GetProvider().TotalAmountInvoiced);
		}

		public void TestInvoiceCurrency()
		{
			SetUpTestData();
			invoice.JZ_RX_NKInvoice_Currency = "EN";
			AssertEquals("InvoiceCurrency", "EN", GetProvider().InvoiceCurrency);
		}

		public void TestDateOfAcceptance()
		{
			SetUpTestData();
			entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 1, 1);
			AssertEquals("DateOfAcceptance", new DateTime(2023, 1, 1), GetProvider().DateOfAcceptance);
		}

		public void TestExchangeRate()
		{
			SetUpTestData();
			invoice.JZ_InvoiceCurrExRate = 1;
			AssertEquals("ExchangeRate", 1m, GetProvider().ExchangeRate);
		}

		public void TestAdditionalSupplyChainActors()
		{
			SetUpTestData();
			entryInstruction.CusSupplyChainActorReferences.AddNew();
			entryInstruction.CusSupplyChainActorReferences.AddNew();
			AssertEquals("2 CusSupplyChainActorReferences", 2, Provider.AdditionalSupplyChainActors.Count);
		}

		public void TestBuyer()
		{
			Assert("Will be implemented in following work items", true);
		}

		public void TestSeller()
		{
			Assert("Will be implemented in following work items", true);
		}

		public void TestExporter()
		{
			var exporter = Provider.Exporter;
			AssertNull("No exporter on invoice and declaration level", exporter);
			AssertSame("Cached", exporter, Provider.Exporter);

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			declaration.SupplierDocumentaryAddress.OrganisationPK = supplier.PK;
			var provider = new GoodsShipmentProvider(1, new EntryHeaderWrapper(entryHeader));
			Assert("No invoice exporter, has declaration exporter", provider.Exporter is PartyProvider);

			invoice.JZ_OA_ExporterAddress = supplier.MainAddress.PK;
			provider = new GoodsShipmentProvider(2, new EntryHeaderWrapper(entryHeader));
			AssertNull("Has invoice exporter", provider.Exporter);
		}

		public void TestDeliveryTerms()
		{
			SetUpTestData();
			invoice.JZ_IncoTerm = "AIS";
			invoice.ZG_AgreedPlaceCode = "AB";
			invoice.JZ_IncoTermPlace = "WAD";

			CombineAssertions(() =>
			{
				AssertEquals("IncotermCode", "AIS", GetProvider().DeliveryTerms.IncotermCode);
				AssertNull("UNLOCODE", Provider.DeliveryTerms.UNLOCODE);
				AssertEquals("CountryCode", "AB", Provider.DeliveryTerms.CountryCode);
				AssertEquals("Place", "WAD", Provider.DeliveryTerms.Place);
			});
		}

		public void TestMemberStateTerritory()
		{
			Assert("Will be implemented in following work items", true);
		}

		public void TestPreviousDocuments()
		{
			SetUpTestData();
			var doc1 = invoice.PreviousDocuments.AddNew();
			doc1.CSI_Code = "PD1";
			doc1.CSI_ReferenceNumber = "DOC001";

			var doc2 = entryInstruction.PreviousDocuments.AddNew();
			doc2.CSI_Code = "PD2";
			doc2.CSI_ReferenceNumber = "DOC002";

			var doc3 = invoice.PreviousDocuments.AddNew();
			doc3.CSI_Code = "PD3";
			doc3.CSI_ReferenceNumber = "DOC003";

			var doc1Duplicate = invoice.PreviousDocuments.AddNew();
			doc1Duplicate.CSI_Code = "PD1";
			doc1Duplicate.CSI_ReferenceNumber = "DOC001";

			AssertContainsExactElementsInAnyOrder(new[] { "PD1|DOC001", "PD2|DOC002", "PD3|DOC003" }, Provider.PreviousDocuments.Select(x => x.Type + "|" + x.Reference));
		}

		public void TestSupportingDocuments()
		{
			SetUpTestData();
			var doc1 = entryInstruction.SupportingDocuments.AddNew();
			doc1.CSI_Code = "SD1";
			doc1.CSI_ItemNumber = 1;
			doc1.CSI_AdditionalDescription = "DOC001";
			doc1.CSI_DateOfExpiry = ZDate.BrettsBirthday;
			doc1.CSI_ReferenceNumber = "REF001";

			var doc2 = entryInstruction.SupportingDocuments.AddNew();
			doc2.CSI_Code = "SD2";
			doc2.CSI_ItemNumber = 2;
			doc2.CSI_AdditionalDescription = "DOC002";
			doc2.CSI_DateOfExpiry = ZDate.BrettsBirthday.AddDays(1);
			doc2.CSI_ReferenceNumber = "REF002";

			var doc3 = invoice.SupportingDocuments.AddNew();
			doc3.CSI_Code = "SD3";
			doc3.CSI_ItemNumber = 3;
			doc3.CSI_AdditionalDescription = "DOC003";
			doc3.CSI_DateOfExpiry = ZDate.BrettsBirthday;
			doc3.CSI_ReferenceNumber = "REF003";

			var doc1Duplicate = invoice.SupportingDocuments.AddNew();
			doc1Duplicate.CSI_Code = "SD1";
			doc1Duplicate.CSI_ItemNumber = 1;
			doc1Duplicate.CSI_AdditionalDescription = "DOC001";
			doc1Duplicate.CSI_DateOfExpiry = ZDate.BrettsBirthday;
			doc1Duplicate.CSI_ReferenceNumber = "REF001";

			AssertContainsExactElementsInAnyOrder(new[] { "SD1|REF001", "SD2|REF002", "SD3|REF003", }, Provider.SupportingDocuments.Select(x => x.Type + "|" + x.Reference));
		}

		public void TestAdditionalReferences()
		{
			SetUpTestData();
			var ref1 = entryInstruction.AdditionalInfos.AddNew();
			ref1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			ref1.CSI_Code = "AR1";
			ref1.CSI_ReferenceNumber = "REF001";

			var ref2 = entryInstruction.AdditionalInfos.AddNew();
			ref2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			ref2.CSI_Code = "AR2";
			ref2.CSI_ReferenceNumber = "REF002";

			var ref3 = invoice.AdditionalInfos.AddNew();
			ref3.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			ref3.CSI_Code = "AR3";
			ref3.CSI_ReferenceNumber = "REF003";

			var ref1Duplicate = invoice.AdditionalInfos.AddNew();
			ref1Duplicate.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			ref1Duplicate.CSI_Code = "AR1";
			ref1Duplicate.CSI_ReferenceNumber = "REF001";

			AssertContainsExactElementsInAnyOrder(new[] { "AR1|REF001", "AR2|REF002", "AR3|REF003", }, Provider.AdditionalReferences.Select(x => x.Type + "|" + x.Reference));
		}

		public void TestAdditionalInformations()
		{
			SetUpTestData();
			var inf1 = entryInstruction.AdditionalInfos.AddNew();
			inf1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			inf1.CSI_Code = "AI1";
			inf1.CSI_Description = "DESC001";

			var inf2 = entryInstruction.AdditionalInfos.AddNew();
			inf2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			inf2.CSI_Code = "AI2";
			inf2.CSI_Description = "DESC002";

			var inf3 = invoice.AdditionalInfos.AddNew();
			inf3.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			inf3.CSI_Code = "AI3";
			inf3.CSI_Description = "DESC003";

			var inf1Duplicate = invoice.AdditionalInfos.AddNew();
			inf1Duplicate.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			inf1Duplicate.CSI_Code = "AI1";
			inf1Duplicate.CSI_Description = "DESC001";

			AssertContainsExactElementsInAnyOrder(new[] { "AI1|DESC001", "AI2|DESC002", "AI3|DESC003", }, Provider.AdditionalInformations.Select(x => x.Code + "|" + x.Text));
		}

		public void TestAdditionsAndDeductions()
		{
			SetUpTestData();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			var charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = "CT1";
			charge.J7_Amount = 10m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			var charge2 = invoiceLine2.Charges.AddNew();
			charge2.J7_ChargeType = "CT1";
			charge2.J7_Amount = 20m;

			var apportionedCharge1 = invoiceLine.ApportionedCharges.AddNew();
			apportionedCharge1.J7_ChargeType = "CT1";
			apportionedCharge1.J7_Amount = 1m;
			apportionedCharge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

			var apportionedCharge2 = invoiceLine2.ApportionedCharges.AddNew();
			apportionedCharge2.J7_ChargeType = "CT2";
			apportionedCharge2.J7_Amount = 2m;
			apportionedCharge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

			CombineAssertions(() =>
			{
				AssertEquals("AdditionsAndDeductions", 2, Provider.AdditionsAndDeductions.Count);
				AssertContainsExactElementsInAnyOrder("There should be 2 AdditionsAndDeductions elements as both Charges and Apportioned Charges should be merged by charge type.", new string[] { "CT1|31", "CT2|2" }, Provider.AdditionsAndDeductions.Select(x => x.Code + "|" + x.Amount.ToString()));
			});
		}

		public void TestAdditionalFiscalReferences()
		{
			SetUpTestData();
			var orgHeader = Factory.New<OrgHeader>();
			_ = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;

			var reference1 = entryInstruction.FiscalReferences.AddNew();
			reference1.CFR_Code = "CD1";
			reference1.CFR_OA_Owner = orgAddress.PK;

			var reference2 = entryInstruction.FiscalReferences.AddNew();
			reference2.CFR_Code = "CD2";

			CombineAssertions(() =>
			{
				AssertType<AdditionalFiscalReferenceProvider[]>("Type", Provider.AdditionalFiscalReferences);
				AssertEquals("AdditionalFiscalReferences", 2, Provider.AdditionalFiscalReferences.Count);
				AssertContainsExactElementsInExactOrder("Additional Fiscal Reference Elements", new string[] { "1|CD1|123", "2|CD2|" }, Provider.AdditionalFiscalReferences.Select(x => $"{x.SequenceNumber}|{x.Role}|{x.VatIdentificationNumber}"));
			});
		}

		public void TestPostalCharges()
		{
			Assert("Will be implemented in following work items", true);
		}

		public void TestAuthorisations()
		{
			Assert("Will be implemented in following work items", true);
		}

		public void TestCommodity()
		{
			Assert("Will be implemented in following work items", true);
		}

		public void TestContainerIds()
		{
			Assert("Will be implemented in following work items", true);
		}

		public void TestCustomsValuation()
		{
			Assert("Will be implemented in following work items", true);
		}

		public void TestDeclarationGoodsItemNumber()
		{
			Assert("Will be implemented in following work items", true);
		}

		public void TestOrigin()
		{
			Assert("Will be implemented in following work items", true);
		}

		public void TestPackages()
		{
			Assert("Will be implemented in following work items", true);
		}

		public void TestProcedure()
		{
			Assert("Will be implemented in following work items", true);
		}

		public void TestReferenceNumberUCR()
		{
			Assert("Will be implemented in following work items", true);
		}

		public void TestTransportAndInsuranceCostsToTheDestination()
		{
			Assert("Will be implemented in following work items", true);
		}

		public void TestTransportDocuments()
		{
			SetUpTestData();
			var tra1 = entryInstruction.AdditionalInfos.AddNew();
			tra1.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			tra1.CSI_Code = "TD1";
			tra1.CSI_ReferenceNumber = "REF1";
			tra1.CSI_Description = "Description1";
			var tra2 = entryInstruction.AdditionalInfos.AddNew();
			tra2.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			tra2.CSI_Code = "TD2";
			tra2.CSI_ReferenceNumber = "REF2";
			tra2.CSI_Description = "Description2";

			AssertContainsExactElementsInAnyOrder("TransportDocuments", new[] { "TD1|REF1", "TD2|REF2" }, Provider.TransportDocuments.Select(x => x.Type + "|" + x.Reference));
		}

		public void TestValuationAdjustment()
		{
			Assert("Will be implemented in following work items", true);
		}

		public void TestSequenceNumber()
		{
			AssertEquals("SequenceNumber", "1", Provider.SequenceNumber);
		}

		protected override GoodsShipmentProvider GetProvider()
		{
			SetUpTestData();
			return new GoodsShipmentProvider(1, new EntryHeaderWrapper(entryHeader));
		}

		void SetUpTestData()
		{
			if (entryHeader == null)
			{
				declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
				entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				invoice = declaration.Invoices.AddNew();
				invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
			}
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
	}
}
