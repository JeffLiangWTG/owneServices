using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	// WI00488873 - Supporting Document  - double check the following
	class IE570And573CommonConsignmentItemProviderTest : DataProviderTestCase<IE570And573CommonConsignmentItemProvider>
	{
		public void TestSupportingDocuments()
		{
			MessageProviderTestHelper.SetupForCusSupportingInfo(Factory, headerOnly: false, codeType: Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "SD1");

			var doc1 = invoiceLine.SupportingDocuments.AddNew();
			doc1.CSI_Code = "SD1";
			doc1.CSI_ReferenceNumber = "DOC002";

			var sd1 = Provider.SupportingDocuments.FirstOrDefault();
			AssertType<SupportingDocumentProvider>("Type of SupportingDocument", sd1);
			AssertEquals("Type", "SD1", sd1.Type);
			AssertEquals("Reference", "DOC002", sd1.Reference);
		}

		public void TestPreviousDocuments()
		{
			var doc1 = invoiceLine.PreviousDocuments.AddNew();
			doc1.CSI_Code = "PD1";
			doc1.CSI_ReferenceNumber = "DOC001";

			var pd1 = Provider.PreviousDocuments.FirstOrDefault();
			AssertType<PreviousDocumentLineProvider>("Type of PreviousDocument", pd1);
			AssertEquals("Type", "PD1", pd1.Type);
			AssertEquals("Reference", "DOC001", pd1.Reference);
		}

		public void TestDeclarationGoodsItemNumber()
		{
			entryLine.CL_LineNumber = 2;
			AssertEquals("DeclarationGoodsItemNumber", (short)2, Provider.DeclarationGoodsItemNumber);
		}

		public void TestAdditionalSupplyChainActors()
		{
			var ca1 = invoiceLine.CusSupplyChainActorReferences.AddNew();
			ca1.CFR_Code = "CA1";
			ca1.CFR_Reference = "CA001";

			var asca1 = Provider.AdditionalSupplyChainActors.FirstOrDefault();
			AssertType<AdditionalSupplyChainActorProvider>("Type of AdditionalSupplyChainActor", asca1);
			AssertEquals("Role", "CA1", asca1.Role);
			AssertEquals("ID", "CA001", asca1.ID);
		}

		public void TestPackages()
		{
			var declaration = entryHeaderWrapper.Declaration;
			declaration.JE_MasterBill = "MB1";
			declaration.JE_TotalNoOfPacks = 1;
			invoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().ForEach(link => link.IsLinked = true);
			AssertType<PackagingProvider>("Type of elements in Packages", Provider.Packages.First());
		}

		public void TestAdditionalReferences()
		{
			var info1 = invoiceLine.AdditionalInfos.AddNew();
			info1.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
			info1.CSI_Code = "AR1";
			info1.CSI_ReferenceNumber = "AR001";

			var info2 = invoiceLine.AdditionalInfos.AddNew();
			info2.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			info2.CSI_Code = "AI1";
			info2.CSI_Description = "AI001";

			AssertEquals("Count", 1, Provider.AdditionalReferences.Count);
			var ar1 = Provider.AdditionalReferences.First();
			AssertType<AdditionalReferenceProvider>("Type of AdditionalReference", ar1);
			AssertEquals("Type", "AR1", ar1.Type);
			AssertEquals("Reference", "AR001", ar1.Reference);
		}

		public void TestAdditionalInformations()
		{
			var info1 = invoiceLine.AdditionalInfos.AddNew();
			info1.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
			info1.CSI_Code = "AR1";
			info1.CSI_ReferenceNumber = "AR001";

			var info2 = invoiceLine.AdditionalInfos.AddNew();
			info2.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			info2.CSI_Code = "AI1";
			info2.CSI_Description = "AI001";

			AssertEquals("Count", 1, Provider.AdditionalInformations.Count);
			var ai1 = Provider.AdditionalInformations.First();
			AssertType<AdditionalInformationProvider>("Type of AdditionalInformation", ai1);
			AssertEquals("Code", "AI1", ai1.Code);
			AssertEquals("Text", "AI001", ai1.Text);
		}

		protected override IE570And573CommonConsignmentItemProvider GetProvider() => new IE570And573CommonConsignmentItemProvider(entryLine, entryHeaderWrapper);

		protected override void SetUp()
		{
			base.SetUp();
			var testBizObjs = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
			entryHeaderWrapper = testBizObjs.entryHeaderWrapper;
			var entryLineWrapper = testBizObjs.entryLineWrapper;
			invoiceLine = entryLineWrapper.RandomInvoiceLine;
			entryLine = entryLineWrapper.EntryLine;
		}
		EntryHeaderWrapper entryHeaderWrapper;
		JobComInvoiceLine invoiceLine;
		CusEntryLine entryLine;
	}
}
