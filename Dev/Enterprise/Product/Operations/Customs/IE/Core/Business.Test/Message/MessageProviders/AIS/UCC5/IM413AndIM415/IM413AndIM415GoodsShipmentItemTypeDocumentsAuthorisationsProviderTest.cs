using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	sealed class IM413AndIM415GoodsShipmentItemTypeDocumentsAuthorisationsProviderTest : DataProviderTestCase<IM413AndIM415GoodsShipmentItemTypeDocumentsAuthorisationsProvider>
	{
		public void TestIGoodsShipmentItemTypeDocumentsAuthorisations()
		{
			Assert("Should implement IGoodsShipmentItemTypeDocumentsAuthorisations", Provider is IGoodsShipmentItemTypeDocumentsAuthorisations);
		}

		public void TestSimplifiedDeclarationDocuments()
		{
			SetUpTestData();
			invoiceLine.PreviousDocuments.AddNew().CSI_Code = "Y012";
			invoiceLine.PreviousDocuments.AddNew().CSI_Code = "U112";
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.PreviousDocuments.AddNew().CSI_Code = "A008";

			var simplifiedDeclarationDocuments = Provider.SimplifiedDeclarationDocuments;
			AssertEquals(3, simplifiedDeclarationDocuments.Count);
			Assert(simplifiedDeclarationDocuments.First() is IM413AndIM415SimplifiedDeclarationDocumentWritingOffProvider);
		}

		public void TestAdditionalInformations()
		{
			SetUpTestData();
			var addInfo1 = invoiceLine.AdditionalInfos.AddNew();
			addInfo1.CSI_SubType = "INF";
			addInfo1.CSI_Code = "A123";
			var addInfo2 = invoiceLine.AdditionalInfos.AddNew();
			addInfo2.CSI_SubType = "INF";
			addInfo2.CSI_Code = "A124";
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			var addInfo3 = invoiceLine2.AdditionalInfos.AddNew();
			addInfo3.CSI_SubType = "INF";
			addInfo3.CSI_Code = "A111";

			var additionalInformations = Provider.AdditionalInformations;
			AssertEquals(3, additionalInformations.Count);
			Assert(additionalInformations.First() is AdditionalInformationProvider);
		}

		public void TestProducedDocuments()
		{
			SetUpTestData();
			invoiceLine.SupportingDocuments.AddNew().CSI_Code = "A123";
			invoiceLine.SupportingDocuments.AddNew().CSI_Code = "A124";
			var tariff1 = invoiceLine.CusLineTariffDetails.AddNew();
			tariff1.BZ_Qty1 = 98.67;
			tariff1.BZ_UQ1 = "VCT";
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.SupportingDocuments.AddNew().CSI_Code = "A111";
			var tariff2 = invoiceLine2.CusLineTariffDetails.AddNew();
			tariff2.BZ_Qty1 = 14.44;
			tariff2.BZ_UQ1 = "KLT";

			var producedDocuments = Provider.ProducedDocuments;
			AssertEquals(5, producedDocuments.Count);
			Assert(producedDocuments.First() is IM413AndIM415ProducedDocumentsWritingOffProvider);
		}

		protected override IM413AndIM415GoodsShipmentItemTypeDocumentsAuthorisationsProvider GetProvider()
		{
			SetUpTestData();
			return new IM413AndIM415GoodsShipmentItemTypeDocumentsAuthorisationsProvider(entryLine);
		}

		void SetUpTestData()
		{
			if (entryHeader == null)
			{
				var declaration = Factory.New<JobDeclaration>();
				instruction = declaration.CustomsEntryInstructions.AddNew();
				invoiceHeader = declaration.Invoices.AddNew();
				invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = instruction.PK;
				entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
			}
		}

		CusEntryInstruction instruction;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
	}
}
