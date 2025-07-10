using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;
using CusEntryInstruction = Enterprise.Customs.ES.Business.Declaration.CusEntryInstruction;
using CusEntryLine = Enterprise.Customs.ES.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class T2LPOUSPresentationGoodItemWrapperTest : WrapperHelperTest<T2LPOUSPresentationGoodItemWrapper>
	{
		public void TestPreviousDocuments()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty PreviousDocuments list", 0, wrapper.PreviousDocuments.Count);

				var prevdoc1 = declaration.PreviousDocuments.AddNew();
				prevdoc1.CSI_Code = "9001";

				var prevdoc2 = invoiceHeader.PreviousDocuments.AddNew();
				prevdoc2.CSI_Code = "9002";

				var prevdoc3 = invoiceLine.PreviousDocuments.AddNew();
				prevdoc3.CSI_Code = "9003";

				var prevdoc4 = invoiceLine.PreviousDocuments.AddNew();
				prevdoc4.CSI_Code = "9004";

				wrapper = GetWrapper(entryLine);
				var documents = wrapper.PreviousDocuments;
				AssertEquals("Expected filled PreviousDocuments (all from declaration, invoiceHeader and invoiceLines)", 4, documents.Count);
				AssertSame("Cached PreviousDocuments", wrapper.PreviousDocuments, documents);
			});
		}

		public void TestPreviousDocumentsWhitTotalGrossWeightInKGFromInvoiceLine()
		{
			invoiceLine.JI_Weight = 10;
			invoiceLine.JI_WeightUQ = "KG";

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_Weight = 1000;
			invoiceLine2.JI_WeightUQ = "G";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: Expected TotalGrossWeightInKG", (ZDecimal)11, entryLine.TotalGrossWeightInKG);

				var prevdoc1 = invoiceHeader.PreviousDocuments.AddNew();
				prevdoc1.CSI_Code = "9002";

				var prevdoc2 = invoiceLine.PreviousDocuments.AddNew();
				prevdoc2.CSI_Code = "9003";
				prevdoc2.CSI_Quantity = 20;

				var prevdoc3 = invoiceLine.PreviousDocuments.AddNew();
				prevdoc3.CSI_Code = "9004";

				wrapper = GetWrapper(entryLine);
				var documents = wrapper.PreviousDocuments;
				AssertEquals("Expected filled PreviousDocuments (all from declaration, invoiceHeader and invoiceLines)", 3, documents.Count);
				AssertSame("Cached PreviousDocuments", wrapper.PreviousDocuments, documents);
				AssertContainsExactElementsInAnyOrder("Expected quantity in documents", new ZDecimal[] { 11, 20, 11 }, documents.Select(x => x.Quantity));
			});
		}

		public void TestFirstCSI_PackQty()
		{
			var pack1 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
			var packageInfo1 = Factory.New<Customs.Business.BasePackage>();
			pack1.CHC_CW = packageInfo1.PK;
			pack1.CHC_NumberOfPacks = 4;
			invoiceLine.PackagesPivot.Add(pack1);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: Expected FirstCSI_PackQty", 4, entryLine.FirstPackQty());

				var prevdoc1 = invoiceHeader.PreviousDocuments.AddNew();
				prevdoc1.CSI_Code = "9002";

				var prevdoc2 = invoiceLine.PreviousDocuments.AddNew();
				prevdoc2.CSI_Code = "9003";
				prevdoc2.CSI_PackQty = 10;

				var prevdoc3 = invoiceLine.PreviousDocuments.AddNew();
				prevdoc3.CSI_Code = "9004";

				wrapper = GetWrapper(entryLine);
				var documents = wrapper.PreviousDocuments;
				AssertEquals("Expected filled PreviousDocuments (all from invoiceHeader and invoiceLines)", 3, documents.Count);
				AssertSame("Cached PreviousDocuments", wrapper.PreviousDocuments, documents);
				AssertContainsExactElementsInAnyOrder("Expected number of packages in documents/packaging", new ZInt[] { 4, 10, 4 }, documents.Select(x => x.Packaging.NumberOfPackages));
			});
		}

		public void TestFirstCSI_PackType()
		{
			var pack1 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
			var packageInfo1 = Factory.New<Customs.Business.BasePackage>();
			pack1.CHC_CW = packageInfo1.PK;
			pack1.Package.CW_PackType = "AA";
			invoiceLine.PackagesPivot.Add(pack1);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: Expected FirstCSI_PackType", "AA", entryLine.FirstPackType());

				var prevdoc1 = invoiceHeader.PreviousDocuments.AddNew();
				prevdoc1.CSI_Code = "9002";

				var prevdoc2 = invoiceLine.PreviousDocuments.AddNew();
				prevdoc2.CSI_Code = "9003";
				prevdoc2.CSI_PackType = "BB";

				var prevdoc3 = invoiceLine.PreviousDocuments.AddNew();
				prevdoc3.CSI_Code = "9004";

				wrapper = GetWrapper(entryLine);
				var documents = wrapper.PreviousDocuments;
				AssertEquals("Expected filled PreviousDocuments (all from invoiceHeader and invoiceLines)", 3, documents.Count);
				AssertSame("Cached PreviousDocuments", wrapper.PreviousDocuments, documents);
				AssertContainsExactElementsInAnyOrder("Expected type of packages in documents/packaging", new ZString[] { "AA", "BB", "AA" }, documents.Select(x => x.Packaging.TypeOfPackages));
			});
		}

		public void TestT2LT2LFgoodsItemNumber()
		{
			invoiceLine.ZG_T2LItemNumber = 20;
			AssertEquals("Expected filled T2LT2LFgoodsItemNumber", 20, wrapper.T2LT2LFgoodsItemNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge", true, mergeResult);

			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

			wrapper = GetWrapper(entryLine);
		}
		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		CusEntryLine entryLine;
		T2LPOUSPresentationGoodItemWrapper wrapper;

		T2LPOUSPresentationGoodItemWrapper GetWrapper(CusEntryLine entryLine) => new T2LPOUSPresentationGoodItemWrapper(entryLine);

		protected override T2LPOUSPresentationGoodItemWrapper GetProvider() => wrapper;
	}
}
