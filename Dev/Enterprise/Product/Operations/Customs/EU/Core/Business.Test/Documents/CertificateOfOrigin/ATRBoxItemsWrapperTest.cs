using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin.Testing
{
	public class ATRBoxItemsWrapperTest : TestCaseWithFactory
	{
		protected virtual ATRBoxItemsWrapper GetNewATRBoxItemsBuilder(IEnumerable<BaseJobComInvoiceLine> invoiceLines) => new ATRBoxItemsWrapper(invoiceLines);

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Exception expected when parameter is null", () => GetNewATRBoxItemsBuilder(null));
		}

		[ExpectNoExceptions]
		public void TestItemsInfoBox9()
		{
			AssertWithEmptyInvoiceLineCollection(nameof(ATRBoxItemsWrapper.ItemsInfoBox9), x => x.ItemsInfoBox9);

			var invoiceLines = SetupData();
			var boxItemsBuilder = GetNewATRBoxItemsBuilder(invoiceLines);

			NUnit.Framework.Assert.That(boxItemsBuilder.ItemsInfoBox9, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), $"[PRE-CONDITION] {nameof(ATRBoxItemsWrapper.ItemsInfoBox9)}");
			boxItemsBuilder.Build();

			NUnit.Framework.Assert.That(boxItemsBuilder.ItemsInfoBox9, NUnit.Framework.Is.EqualTo(ExpectedBox9), nameof(ATRBoxItemsWrapper.ItemsInfoBox9));
		}

		protected virtual ZString ExpectedBox9 => @"1
2
3";

		[ExpectNoExceptions]
		public void TestMarksNumberBox10()
		{
			AssertWithEmptyInvoiceLineCollection(nameof(ATRBoxItemsWrapper.MarksNumberBox10), x => x.MarksNumberBox10);

			var invoiceLines = SetupData();
			var boxItemsBuilder = GetNewATRBoxItemsBuilder(invoiceLines);

			NUnit.Framework.Assert.That(boxItemsBuilder.MarksNumberBox10, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), $"[PRE-CONDITION] {nameof(ATRBoxItemsWrapper.MarksNumberBox10)}");
			boxItemsBuilder.Build();

			NUnit.Framework.Assert.That(boxItemsBuilder.MarksNumberBox10, NUnit.Framework.Is.EqualTo(ExpectedGrossMassVolumeBox10), nameof(ATRBoxItemsWrapper.MarksNumberBox10));
		}

		protected virtual ZString ExpectedGrossMassVolumeBox10 => @"M&N1, M&N2; 50 CT; INVOICE1LINE1 STUFF
M&N2; 150 CT; INVOICE1LINE2 STUFF
INVOICE2LINE1 STUFF";

		[ExpectNoExceptions]
		public void TestGrossWeightBox11()
		{
			AssertWithEmptyInvoiceLineCollection(nameof(ATRBoxItemsWrapper.GrossWeightBox11), x => x.GrossWeightBox11);

			var invoiceLines = SetupData();
			var boxItemsBuilder = GetNewATRBoxItemsBuilder(invoiceLines);

			NUnit.Framework.Assert.That(boxItemsBuilder.GrossWeightBox11, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), $"[PRE-CONDITION] {nameof(ATRBoxItemsWrapper.GrossWeightBox11)}");
			boxItemsBuilder.Build();

			NUnit.Framework.Assert.That(boxItemsBuilder.GrossWeightBox11, NUnit.Framework.Is.EqualTo(ExpectedInvoicesBox11), nameof(ATRBoxItemsWrapper.GrossWeightBox11));
		}

		protected virtual ZString ExpectedInvoicesBox11 => @"" + System.Environment.NewLine + System.Environment.NewLine;

		[ExpectNoExceptions]
		protected void AssertWithEmptyInvoiceLineCollection(string propertyName, Func<ATRBoxItemsWrapper, ZString> propertyValueFunc)
		{
			var emptyATRBoxItemsBuilder = GetNewATRBoxItemsBuilder(Enumerable.Empty<JobComInvoiceLine>());
			emptyATRBoxItemsBuilder.Build();
			NUnit.Framework.Assert.That(propertyValueFunc(emptyATRBoxItemsBuilder), NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), $"When there are no Invoice Lines, {propertyName}");
		}

		IEnumerable<JobComInvoiceLine> SetupData()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			var billPackingGroup = declaration.Bills.AddNew().PackingGroups.AddNew();
			var package1 = declaration.Packages.AddNew();
			package1.CW_CR_HouseContainer = billPackingGroup.PK;
			package1.CW_PackQty = 100;
			package1.CW_PackType = "VG";
			package1.CW_MarksAndNos = "M&N1";
			var package2 = declaration.Packages.AddNew();
			package2.CW_CR_HouseContainer = billPackingGroup.PK;
			package2.CW_PackQty = 200;
			package2.CW_PackType = "CT";
			package2.CW_MarksAndNos = "M&N2";

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "INV2";
			var invoice2line1 = invoice2.InvoiceLines.AddNew();
			invoice2line1.JI_Tariff = "123";
			invoice2line1.JI_LineNo = 1;
			invoice2line1.JI_Description = "invoice2line1 stuff";
			invoice2line1.JI_Weight = 1m;
			invoice2line1.JI_WeightUQ = "KG";

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV1";
			var invoice1line2 = invoice1.InvoiceLines.AddNew();
			invoice1line2.JI_Tariff = "123";
			invoice1line2.JI_LineNo = 2;
			invoice1line2.JI_InvoiceQuantity = 120m;
			invoice1line2.JI_InvoiceUQ = "PAC";
			invoice1line2.JI_Description = "invoice1line2 stuff";
			invoice1line2.JI_Weight = 2m;
			invoice1line2.JI_WeightUQ = "KG";
			var packageCtInvoiceLine2 = invoice1line2.PackagesPivot.AddNew();
			packageCtInvoiceLine2.CHC_CW = package2.PK;
			packageCtInvoiceLine2.CHC_NumberOfPacks = 150;

			var invoice1line1 = invoice1.InvoiceLines.AddNew();
			invoice1line1.JI_Tariff = "123";
			invoice1line1.JI_LineNo = 1;
			invoice1line1.JI_InvoiceQuantity = 110m;
			invoice1line1.JI_InvoiceUQ = "BOX";
			invoice1line1.JI_Description = "invoice1line1 stuff";
			invoice1line1.JI_Weight = 11m;
			invoice1line1.JI_WeightUQ = "KG";
			var packageVgInvoiceLine1 = invoice1line1.PackagesPivot.AddNew();
			packageVgInvoiceLine1.CHC_CW = package1.PK;
			packageVgInvoiceLine1.CHC_NumberOfPacks = 99;
			var packageCtInvoiceLine1 = invoice1line1.PackagesPivot.AddNew();
			packageCtInvoiceLine1.CHC_CW = package2.PK;
			packageCtInvoiceLine1.CHC_NumberOfPacks = 50;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			var entryLine2 = entryHeader.MergedLines.AddNew();
			var entryLine3 = entryHeader.MergedLines.AddNew();

			invoice1line1.JI_CL = entryLine1.PK;
			invoice1line2.JI_CL = entryLine2.PK;
			invoice2line1.JI_CL = entryLine3.PK;

			AddDataToInvoiceLine(invoice1line1);

			yield return invoice1line1;
			yield return invoice1line2;
			yield return invoice2line1;
		}

		protected virtual void AddDataToInvoiceLine(JobComInvoiceLine invoiceLine)
		{
		}
	}
}
