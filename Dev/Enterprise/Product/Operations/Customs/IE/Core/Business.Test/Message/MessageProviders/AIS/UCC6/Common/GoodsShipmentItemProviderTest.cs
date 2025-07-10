using System;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class GoodsShipmentItemProviderTest : DataProviderTestCase<GoodsShipmentItemProvider>
	{
		public void TestIGoodsShipmentItem()
		{
			Assert("Should implement IGoodsShipmentItem", Provider is IGoodsShipmentItem);
		}

		public void TestOrigin()
		{
			SetUpTestData();
			invoiceLine.JI_CountryOfOrigin = "IE";
			invoiceLine.ZG_CountryOfSupply = "FR";
			var origin = Provider.Origin;
			AssertEquals("CountryOfOrigin", "IE", origin.CountryOfOrigin);
			AssertEquals("CountryOfPreferentialOrigin", "FR", origin.CountryOfPreferentialOrigin);
			AssertSame("Cached", origin, Provider.Origin);
		}

		public void TestCountryOfDispatch()
		{
			SetUpTestData();
			invoiceLine.ZG_CountryOfDispatch = "IE";
			AssertEquals("IE", Provider.CountryOfDispatch);
		}

		public void TestDestination()
		{
			SetUpTestData();
			invoiceLine.ZG_CountryOfDestination = "FR";
			invoiceLine.ZG_RegionOfDestination = "75C";
			var destination = Provider.Destination;
			AssertEquals("CountryOfDestination", "FR", destination.CountryOfDestination);
			AssertEquals("RegionOfDestination", "75C", destination.RegionOfDestination);
			AssertSame("Cached", destination, Provider.Destination);
		}

		public void TestDeclarationGoodsItemNumber()
		{
			SetUpTestData();
			entryLine.CL_LineNumber = 1;
			AssertEquals("DeclarationGoodsItemNumber", "1", Provider.DeclarationGoodsItemNumber);
		}

		public void TestStatisticalValue()
		{
			SetUpTestData();
			entryLine.CL_StatisticalValue = 2;
			AssertEquals("StatisticalValue", (decimal)2, Provider.StatisticalValue);
		}

		public void TestNatureOfTransaction()
		{
			SetUpTestData();
			invoice.JZ_ValuationCode = "AI";
			AssertEquals("NatureOfTransaction", "AI", Provider.NatureOfTransaction);
		}

		public void TestReferenceNumberUCR()
		{
			SetUpTestData();
			declaration.JE_UCR = "AIS";
			AssertEquals("ReferenceNumberUCR", "AIS", Provider.ReferenceNumberUCR);
		}

		public void TestDateOfAcceptance()
		{
			SetUpTestData();
			entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 1, 16);
			AssertEquals("CEI_DateForDuty", new DateTime(2023, 1, 16), Provider.DateOfAcceptance);
			invoiceLine.JI_DateForDutyOverride = new ZDateTime(2023, 1, 1);
			AssertEquals("JI_DateForDutyOverride", new DateTime(2023, 1, 1), Provider.DateOfAcceptance);
		}

		public void TestAuthorisations()
		{
			SetUpTestData();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = orgHeader.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Ireland;
			cusCode.OK_CustomsRegNo = "EU1234567890";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunau = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU;
			helper.CreateCusMapType(eunau, MapDirectionList.Codes.OUT, "EUNAU", true);
			helper.CreateCusMap(eunau, "AVC", "C521", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			Factory.Save();
			var usages = invoiceLine.CusAuthorizationUsages.AddNew();
			usages.AGC_Code = "AVC";
			usages.AGC_Number = "123";
			usages.AGC_OH_Owner = orgHeader.PK;

			AssertEquals("Count", 1, Provider.Authorisations.Count);
			var auth = Provider.Authorisations.First();
			CombineAssertions(() =>
			{
				AssertEquals("Type", "C521", auth.Type);
				AssertEquals("ReferenceNumber", "123", auth.Reference);
				AssertEquals("HolderOfTheAuthorisation", "EU1234567890", auth.HolderOfTheAuthorisation);
			});
		}

		public void TestProcedure()
		{
			SetUpTestData();
			invoiceLine.JI_Procedure = "1234";
			var additionalProcedure = invoiceLine.AdditionalProcedureCodes.AddNew();
			additionalProcedure.CY_Code = "1234F48";

			CombineAssertions(() =>
			{
				AssertEquals("RequestedProcedure", "12", Provider.Procedure.RequestedProcedure);
				AssertEquals("PreviousProcedure", "34", Provider.Procedure.PreviousProcedure);
				AssertEquals("RequestedProcedure", "F48", Provider.Procedure.AdditionalProcedure.FirstOrDefault().AdditionalProcedure);
				AssertEquals("SequenceNumber", "1", Provider.Procedure.AdditionalProcedure.FirstOrDefault().SequenceNumber);
				AssertNull("CcQualifier", Provider.Procedure.AdditionalProcedure.FirstOrDefault().CcQualifier);
			});
		}

		public void TestAdditionalSupplyChainActors()
		{
			SetUpTestData();
			invoiceLine.CusSupplyChainActorReferences.AddNew();
			AssertEquals(1, Provider.AdditionalSupplyChainActors.Count);
		}

		public void TestBuyer()
		{
			SetUpTestData();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			invoice.JZ_OA_BuyerAddress = orgHeader.MainAddress.PK;
			var buyer = Provider.Buyer;
			AssertSame("Cached", buyer, Provider.Buyer);
		}

		public void TestSeller()
		{
			Assert("Will be implemented in following work items", true);
		}

		public void TestExporter()
		{
			var exporter = Provider.Exporter;
			AssertNull("No exporter at all", exporter);
			AssertSame("Cached", exporter, Provider.Exporter);

			ResetTestData();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_FullName = "Supplier";
			var anotherInvoice = declaration.Invoices.AddNew();
			anotherInvoice.JZ_OA_ExporterAddress = supplier.MainAddress.PK;
			var anotherInvoiceLine = anotherInvoice.InvoiceLines.AddNew();
			var anotherEntryLine = entryHeader.MergedLines.AddNew();
			anotherInvoiceLine.JI_CL = anotherEntryLine.PK;

			var supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			supplier2.OH_FullName = "Supplier2";
			declaration.SupplierDocumentaryAddress.OrganisationPK = supplier2.PK;
			var provider = new GoodsShipmentItemProvider(entryLine, new EntryHeaderWrapper(entryHeader));
			AssertEquals("No invoice exporter, has declaration exporter", "Supplier2", provider.Exporter.Name);

			var supplier3 = Factory.NewWithValidTestData<OrgHeader>();
			supplier3.OH_FullName = "Supplier3";
			invoice.JZ_OA_ExporterAddress = supplier3.MainAddress.PK;
			provider = new GoodsShipmentItemProvider(entryLine, new EntryHeaderWrapper(entryHeader));
			AssertEquals("Has invoice exporter", "Supplier3", provider.Exporter.Name);

			declaration.SupplierDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			invoice.JZ_OA_ExporterAddress = ZGuid.Empty;
			provider = new GoodsShipmentItemProvider(entryLine, new EntryHeaderWrapper(entryHeader));
			AssertNull("No invoice or declaration exporter", provider.Exporter);
		}

		public void TestMemberStateTerritory()
		{
			Assert("Will be implemented in following work items", true);
		}

		public void TestCommodity()
		{
			SetUpTestData();
			var commodity = Provider.Commodity;
			AssertSame("Cached", commodity, Provider.Commodity);
		}

		public void TestPackages()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UNPKG");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
			var unpackCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "T1", "Unpack code", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(unpackCode.PK, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "1");
			var bulkCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "T2", "Unpack code", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(bulkCode.PK, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "1");
			Factory.Save();

			SetUpTestData();

			var pk1 = declaration.Packages.AddNew();
			pk1.CW_PackType = "T1";
			pk1.CW_PackQty = 10;
			pk1.CW_MarksAndNos = "M1";
			var pk2 = declaration.Packages.AddNew();
			pk2.CW_PackType = "T2";
			pk2.CW_PackQty = 20;
			pk2.CW_MarksAndNos = "M2";

			CombineAssertions(() =>
			{
				AssertType<PackagingProvider[]>("Type", Provider.Packages);
				AssertContainsExactElementsInExactOrder("Packages Elements", new string[] { "T1|10|M1", "T2|0|M2" }, Provider.Packages.Select(x => $"{x.PackageType}|{x.PackageQuantity}|{x.ShippingMarks}"));
			});
		}

		public void TestPreviousDocuments()
		{
			SetUpTestData();
			var doc1 = invoiceLine.PreviousDocuments.AddNew();
			doc1.CSI_Code = "PD1";
			doc1.CSI_ReferenceNumber = "DOC001";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			var doc2 = invoiceLine2.PreviousDocuments.AddNew();
			doc2.CSI_Code = "PD2";
			doc2.CSI_ReferenceNumber = "DOC002";

			AssertContainsExactElementsInAnyOrder(new[] { "PD1|DOC001", "PD2|DOC002" }, Provider.PreviousDocuments.Select(x => x.Type + "|" + x.Reference));
		}

		public void TestSupportingDocuments()
		{
			SetUpTestData();
			var doc1 = invoiceLine.SupportingDocuments.AddNew();
			doc1.CSI_Code = "SD1";
			doc1.CSI_ReferenceNumber = "REF001";
			doc1.CSI_ItemNumber = 1;
			doc1.CSI_AdditionalDescription = "DOC001";
			doc1.CSI_DateOfExpiry = ZDate.BrettsBirthday;
			doc1.CSI_UnitOfQuantity = "BOX";
			doc1.CSI_Quantity = 1;
			doc1.CSI_RX_NKCurrency = "EUR";
			doc1.CSI_Value = 11;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			var doc2 = invoiceLine2.SupportingDocuments.AddNew();
			doc2.CSI_Code = "SD2";
			doc2.CSI_ReferenceNumber = "REF002";
			doc2.CSI_ItemNumber = 2;
			doc2.CSI_AdditionalDescription = "DOC002";
			doc2.CSI_DateOfExpiry = ZDate.BrettsBirthday.AddDays(1);

			var doc1Duplicate = invoiceLine2.SupportingDocuments.AddNew();
			doc1Duplicate.CSI_Code = "SD1";
			doc1Duplicate.CSI_ReferenceNumber = "REF001";
			doc1Duplicate.CSI_ItemNumber = 1;
			doc1Duplicate.CSI_AdditionalDescription = "DOC001";
			doc1Duplicate.CSI_DateOfExpiry = ZDate.BrettsBirthday;
			doc1Duplicate.CSI_UnitOfQuantity = "BOX";
			doc1Duplicate.CSI_Quantity = 1;
			doc1Duplicate.CSI_RX_NKCurrency = "EUR";
			doc1Duplicate.CSI_Value = 11;

			AssertContainsExactElementsInAnyOrder(new[] { "SD1|REF001", "SD2|REF002" }, Provider.SupportingDocuments.Select(x => x.Type + "|" + x.Reference));
		}

		public void TestTransportDocuments()
		{
			SetUpTestData();
			var tra1 = invoiceLine.AdditionalInfos.AddNew();
			tra1.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			tra1.CSI_Code = "TD1";
			tra1.CSI_ReferenceNumber = "REF001";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			var tra2 = invoiceLine2.AdditionalInfos.AddNew();
			tra2.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			tra2.CSI_Code = "TD2";
			tra2.CSI_ReferenceNumber = "REF002";

			var doc1Duplicate = invoiceLine2.AdditionalInfos.AddNew();
			doc1Duplicate.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			doc1Duplicate.CSI_Code = "TD1";
			doc1Duplicate.CSI_ReferenceNumber = "REF001";

			AssertContainsExactElementsInAnyOrder("TransportDocuments", new[] { "TD1|REF001", "TD2|REF002" }, Provider.TransportDocuments.Select(x => x.Type + "|" + x.Reference));
		}

		public void TestAdditionalReferences()
		{
			SetUpTestData();
			var ref1 = invoiceLine.AdditionalInfos.AddNew();
			ref1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			ref1.CSI_Code = "AR1";
			ref1.CSI_ReferenceNumber = "REF001";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			var ref2 = invoiceLine2.AdditionalInfos.AddNew();
			ref2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			ref2.CSI_Code = "AR2";
			ref2.CSI_ReferenceNumber = "REF002";

			var ref1Duplicate = invoiceLine2.AdditionalInfos.AddNew();
			ref1Duplicate.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			ref1Duplicate.CSI_Code = "AR1";
			ref1Duplicate.CSI_ReferenceNumber = "REF001";

			AssertContainsExactElementsInAnyOrder(new[] { "AR1|REF001", "AR2|REF002", }, Provider.AdditionalReferences.Select(x => x.Type + "|" + x.Reference));
		}

		public void TestAdditionalInformations()
		{
			SetUpTestData();
			var inf1 = invoiceLine.AdditionalInfos.AddNew();
			inf1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			inf1.CSI_Code = "AI1";
			inf1.CSI_Description = "DESC001";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			var inf2 = invoiceLine2.AdditionalInfos.AddNew();
			inf2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			inf2.CSI_Code = "AI2";
			inf2.CSI_Description = "DESC002";

			var inf1Duplicate = invoiceLine2.AdditionalInfos.AddNew();
			inf1Duplicate.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			inf1Duplicate.CSI_Code = "AI1";
			inf1Duplicate.CSI_Description = "DESC001";

			AssertContainsExactElementsInAnyOrder(new[] { "AI1|DESC001", "AI2|DESC002", }, Provider.AdditionalInformations.Select(x => x.Code + "|" + x.Text));
		}

		public void TestCustomsValuation()
		{
			AssertType<CustomsValuationProvider>(Provider.CustomsValuation);
		}

		public void TestValuationAdjustment()
		{
			SetUpTestData();
			AssertEquals("0000", Provider.ValuationAdjustment);
			invoice.RelatedIndicator = true;
			AssertEquals("1000", GetProvider().ValuationAdjustment);
			invoice.RelatedIndicator3 = true;
			AssertEquals("1010", GetProvider().ValuationAdjustment);
			invoice.RelatedIndicator3 = false;
			invoice.RelatedIndicator4 = true;
			AssertEquals("1001", GetProvider().ValuationAdjustment);

			invoiceLine.JI_RelatedIndicator = ValuationIndicatorCodeList.Codes.No;
			AssertEquals("0001", GetProvider().ValuationAdjustment);
			invoiceLine.JI_RelatedIndicator = ValuationIndicatorCodeList.Codes.SameAsInvoiceHeader;
			AssertEquals("1001", GetProvider().ValuationAdjustment);

			invoiceLine.ZG_RelatedIndicator2 = ValuationIndicatorCodeList.Codes.Yes;
			AssertEquals("1101", GetProvider().ValuationAdjustment);
			invoiceLine.ZG_RelatedIndicator4 = ValuationIndicatorCodeList.Codes.No;
			AssertEquals("1100", GetProvider().ValuationAdjustment);
			invoiceLine.ZG_RelatedIndicator4 = ValuationIndicatorCodeList.Codes.SameAsInvoiceHeader;
			AssertEquals("1101", GetProvider().ValuationAdjustment);
		}

		public void TestAdditionalFiscalReferences()
		{
			SetUpTestData();
			var orgHeader = Factory.New<OrgHeader>();
			_ = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;

			var reference1 = invoiceLine.FiscalReferences.AddNew();
			reference1.CFR_Code = "CD1";
			reference1.CFR_OA_Owner = orgAddress.PK;

			var reference2 = invoiceLine.FiscalReferences.AddNew();
			reference2.CFR_Code = "CD2";

			CombineAssertions(() =>
			{
				AssertType<AdditionalFiscalReferenceProvider[]>("Type", Provider.AdditionalFiscalReferences);
				AssertEquals("AdditionalFiscalReferences", 2, Provider.AdditionalFiscalReferences.Count);
				AssertContainsExactElementsInExactOrder("Additional Fiscal Reference Elements", new string[] { "1|CD1|123", "2|CD2|" }, Provider.AdditionalFiscalReferences.Select(x => $"{x.SequenceNumber}|{x.Role}|{x.VatIdentificationNumber}"));
			});
		}

		public void TestTransportAndInsuranceCostsToTheDestination()
		{
			Assert("Will be implemented in following work items", true);
		}

		public void TestContainerIds()
		{
			SetUpTestData();
			declaration.CusContainers.AddNew().CO_ContainerNumber = "ID1";
			declaration.CusContainers.AddNew().CO_ContainerNumber = "ID2";

			CombineAssertions(() =>
			{
				AssertType<string[]>("Type", Provider.ContainerIds);
				AssertContainsExactElementsInExactOrder("ContainerNumbers Elements", new string[] { "ID1", "ID2" }, Provider.ContainerIds);
			});
		}

		protected override GoodsShipmentItemProvider GetProvider()
		{
			SetUpTestData();
			return new GoodsShipmentItemProvider(entryLine, new EntryHeaderWrapper(entryHeader));
		}

		void ResetTestData()
		{
			entryHeader = null;
			SetUpTestData();
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
