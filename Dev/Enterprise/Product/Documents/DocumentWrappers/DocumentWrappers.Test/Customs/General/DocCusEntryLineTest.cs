using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.General.Testing
{
	[TestedType(typeof(DocCusEntryLine))]
	sealed class DocCusEntryLineTest : DocBaseCusEntryLineAbstractTest<CusEntryLine, DocCusEntryLine>
	{
		protected override DocCusEntryLine CreateEntryLineWrapper(ICusEntryLine entryLineInternal)
		{
			return DocCusEntryLine.New((CusEntryLine)entryLineInternal, Factory);
		}

		protected override string TestingCountry
		{
			get { return Core.Constants.CountryCodes.Eritrea; }
		}

		public override void TestLinePricesWithCurrency()
		{
			AssertEquals(DocEntryLineMergeOfTwoInvoiceLines.LinePricesWithCurrency, "200.05 FJD");
		}

		protected override ICusEntryLine EntryLineMergeOfTwoInvoiceLines
		{
			get
			{
				if (fEntryLineMergeOfTwoInvoiceLines == null)
				{
					var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
					mockDeclaration.Protected().Setup<bool>("IsCustomsHeaderAmendmentATotalReplacement").Returns(false);
					mockDeclaration.Protected().Setup<bool>("IsCustomsLineAmendmentATotalReplacement").Returns(false);

					var declaration = mockDeclaration.Object;
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
					var header = declaration.Invoices.AddNew();
					header.JZ_InvoiceNumber = "1";
					header.JZ_RX_NKInvoice_Currency = "FJD";

					var invoiceLine = header.JobComInvoiceLines.AddNew();
					invoiceLine.JI_LinePrice = 200.05m;
					invoiceLine.JI_Tariff = "123";
					invoiceLine.JI_LineNo = 1;

					declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
					declaration.JE_MessageType = "IMP";
					declaration.DoMerge();
					Assert("Must have at least one CustomsEntryHeader", declaration.CustomsEntryHeaders.Count > 0);
					Assert("Must have at least one CustomsEntryHeader", declaration.CustomsEntryHeaders[0].MergedLines.Count > 0);
					fEntryLineMergeOfTwoInvoiceLines = declaration.CustomsEntryHeaders[0].MergedLines[0];
				}
				return fEntryLineMergeOfTwoInvoiceLines;
			}
		}

		protected override DocCusEntryLine DocEntryLineMergeOfTwoInvoiceLines
		{
			get { return CreateEntryLineWrapper(EntryLineMergeOfTwoInvoiceLines); }
		}

		CusEntryLine fEntryLineMergeOfTwoInvoiceLines;
	}
}
