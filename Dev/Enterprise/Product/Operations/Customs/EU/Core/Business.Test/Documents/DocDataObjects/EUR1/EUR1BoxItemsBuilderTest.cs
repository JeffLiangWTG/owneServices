using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Documents.DocDataObjects.Testing
{
	public class EUR1BoxItemsBuilderTest : TestCaseWithFactory
	{
		protected virtual EUR1BoxItemsBuilder GetNewEUR1BoxItemsBuilder(IEnumerable<JobComInvoiceLine> invoiceLines) => new EUR1BoxItemsBuilder(invoiceLines);

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Exception expected when parameter is null", () => GetNewEUR1BoxItemsBuilder(null));
		}

		[ExpectNoExceptions]
		public void TestItemsInfoBox8()
		{
			AssertWithEmptyInvoiceLineCollection(nameof(EUR1BoxItemsBuilder.ItemsInfoBox8), x => x.ItemsInfoBox8);

			var invoiceLines = SetupData();
			var boxItemsBuilder = GetNewEUR1BoxItemsBuilder(invoiceLines);

			NUnit.Framework.Assert.That(boxItemsBuilder.ItemsInfoBox8, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), $"[PRE-CONDITION] {nameof(EUR1BoxItemsBuilder.ItemsInfoBox8)}");
			boxItemsBuilder.Build();

			NUnit.Framework.Assert.That(boxItemsBuilder.ItemsInfoBox8, NUnit.Framework.Is.EqualTo(ExpectedBox8), nameof(EUR1BoxItemsBuilder.ItemsInfoBox8));
		}

		protected virtual ZString ExpectedBox8 => @"1; M&N1, M&N2; 99 VG, 50 CT; INVOICE1LINE1 STUFF
2; M&N2; 150 CT; INVOICE1LINE2 STUFF
3; INVOICE2LINE1 STUFF
-------------------------------------------------------------------------------------------------------";

		[ExpectNoExceptions]
		public void TestGrossMassVolumeBox9()
		{
			AssertWithEmptyInvoiceLineCollection(nameof(EUR1BoxItemsBuilder.GrossMassVolumeBox9), x => x.GrossMassVolumeBox9);

			var invoiceLines = SetupData();
			var boxItemsBuilder = GetNewEUR1BoxItemsBuilder(invoiceLines);

			NUnit.Framework.Assert.That(boxItemsBuilder.GrossMassVolumeBox9, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), $"[PRE-CONDITION] {nameof(EUR1BoxItemsBuilder.GrossMassVolumeBox9)}");
			boxItemsBuilder.Build();

			NUnit.Framework.Assert.That(boxItemsBuilder.GrossMassVolumeBox9, NUnit.Framework.Is.EqualTo(ExpectedGrossMassVolumeBox9), nameof(EUR1BoxItemsBuilder.GrossMassVolumeBox9));
		}

		protected virtual ZString ExpectedGrossMassVolumeBox9 => @"1.1 KG
1.2 KG
2.1 KG";

		[ExpectNoExceptions]
		public void TestInvoicesBox10()
		{
			AssertWithEmptyInvoiceLineCollection(nameof(EUR1BoxItemsBuilder.InvoicesBox10), x => x.InvoicesBox10);

			var invoiceLines = SetupData();
			var boxItemsBuilder = GetNewEUR1BoxItemsBuilder(invoiceLines);

			NUnit.Framework.Assert.That(boxItemsBuilder.InvoicesBox10, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), $"[PRE-CONDITION] {nameof(EUR1BoxItemsBuilder.InvoicesBox10)}");
			boxItemsBuilder.Build();

			NUnit.Framework.Assert.That(boxItemsBuilder.InvoicesBox10.Replace("\r", ZString.Empty).Replace("\n", "\r\n"), NUnit.Framework.Is.EqualTo(ExpectedInvoicesBox10), nameof(EUR1BoxItemsBuilder.InvoicesBox10));
		}

		protected virtual ZString ExpectedInvoicesBox10 => @"INV1
INV2";

		[ExpectNoExceptions]
		protected void AssertWithEmptyInvoiceLineCollection(string propertyName, Func<EUR1BoxItemsBuilder, ZString> propertyValueFunc)
		{
			var emptyEUR1BoxItemsBuilder = GetNewEUR1BoxItemsBuilder(Enumerable.Empty<JobComInvoiceLine>());
			emptyEUR1BoxItemsBuilder.Build();
			NUnit.Framework.Assert.That(propertyValueFunc(emptyEUR1BoxItemsBuilder), NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), $"When there are no Invoice Lines, {propertyName}");
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
			invoice2line1.JI_Weight = 2.1m;
			invoice2line1.JI_WeightUQ = "KG";

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV1";
			var invoice1line2 = invoice1.InvoiceLines.AddNew();
			invoice1line2.JI_Tariff = "123";
			invoice1line2.JI_LineNo = 2;
			invoice1line2.JI_InvoiceQuantity = 120m;
			invoice1line2.JI_InvoiceUQ = "PAC";
			invoice1line2.JI_Description = "invoice1line2 stuff";
			invoice1line2.JI_Weight = 1.2m;
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
			invoice1line1.JI_Weight = 1.1m;
			invoice1line1.JI_WeightUQ = "KG";
			invoice1line1.JI_Volume = 22m;
			invoice1line1.JI_VolumeUQ = "M3";
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
