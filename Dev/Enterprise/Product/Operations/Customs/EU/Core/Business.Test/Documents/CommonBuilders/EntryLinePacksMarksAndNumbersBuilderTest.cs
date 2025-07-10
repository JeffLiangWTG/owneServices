using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Documents;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing.Documents.Common
{
	public class EntryLinePacksMarksAndNumbersBuilderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Expected exception when entryline is null", () => new EntryLinePacksMarksAndNumbersBuilder(null));
		}

		[ExpectNoExceptions]
		public void TestBuild()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var package1 = declaration.Packages.AddNew();
			package1.CW_PackQty = 1;
			package1.CW_PackType = "CT";
			package1.CW_MarksAndNos = "IND";

			var package2 = declaration.Packages.AddNew();
			package2.CW_PackQty = 3;
			package2.CW_PackType = "RR";
			package2.CW_MarksAndNos = "ARG";

			var packagesPivotCollection = invoiceLine.PackagesPivot;
			var packagePivot1 = packagesPivotCollection.AddNew();
			packagePivot1.CHC_CW = package1.PK;
			packagePivot1.CHC_NumberOfPacks = 1;
			packagePivot1.CHC_JE = invoiceLine.Declaration.PK;

			var packagePivot2 = packagesPivotCollection.AddNew();
			packagePivot2.CHC_CW = package2.PK;
			packagePivot2.CHC_NumberOfPacks = 1;
			packagePivot2.CHC_JE = invoiceLine.Declaration.PK;

			var builder = new EntryLinePacksMarksAndNumbersBuilder(entryLine);

			NUnit.Framework.Assert.That(builder.Build(), NUnit.Framework.Is.EqualTo("IND, 1 CT; ARG, 1 RR").Using(CustomComparers.TypeComparison), "when useCommaBetweenPackageNumberAndType is false, no comma between type and number is expected");
		}
	}
}
