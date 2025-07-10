using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class IM433GoodsShipmentItemProviderTest : DataProviderTestCase<IM433GoodsShipmentItemProvider>
	{
		public void TestDeclarationGoodsItemNumber()
		{
			SetUpTestData();
			entryLine.CL_LineNumber = 1;
			AssertEquals("DeclarationGoodsItemNumber", "1", Provider.DeclarationGoodsItemNumber);
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

		public void TestCommodity()
		{
			SetUpTestData();
			var commodity = Provider.Commodity;
			CombineAssertions(() =>
			{
				AssertType<MCommodityType04Provider>(commodity);
				AssertSame("Cached", commodity, Provider.Commodity);
			});
		}

		public void TestPackagings()
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
				AssertType<PackagingProvider[]>("Type", Provider.Packagings);
				AssertContainsExactElementsInExactOrder("Packages Elements", new string[] { "T1|10|M1", "T2|0|M2" }, Provider.Packagings.Select(x => $"{x.PackageType}|{x.PackageQuantity}|{x.ShippingMarks}"));
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

		protected override IM433GoodsShipmentItemProvider GetProvider()
		{
			SetUpTestData();
			return new IM433GoodsShipmentItemProvider(entryLine, new EntryHeaderWrapper(entryHeader));
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
