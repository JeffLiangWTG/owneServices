using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin.Testing
{
	class EURGoodsSummaryWrapperTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Exception expected when parameter is null", () => new EURGoodsSummaryWrapper(invoiceLines: null));
		}

		[ExpectNoExceptions]
		public void TestEmptyItems()
		{
			var wrapper = (IEURGoodsSummary)new EURGoodsSummaryWrapper(Array.Empty<JobComInvoiceLine>());
			CombineAssertions("When there are no invoice lines", () =>
			 {
				 NUnit.Framework.Assert.That(wrapper.Description, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Description");
				 NUnit.Framework.Assert.That(wrapper.WeightAndVolume, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "WeightAndVolume");
				 NUnit.Framework.Assert.That(wrapper.InvoiceNumbers, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "InvoiceNumbers");
			 });
		}

		[ExpectNoExceptions]
		public void TestDescription()
		{
			var invoiceLines = GetInvoiceLines();
			var wrapper = (IEURGoodsSummary)new EURGoodsSummaryWrapper(invoiceLines);

			var expectedItemsInfo =
				"1; M&N1, M&N2; 99 VG, 50 CT; INVOICE1LINE1 STUFF\r\n" +
				"2; M&N2; 150 CT; INVOICE1LINE2 STUFF\r\n" +
				"3; INVOICE2LINE1 STUFF\r\n" +
				"-------------------------------------------------------------------------------------------------------";
			NUnit.Framework.Assert.That(wrapper.Description, NUnit.Framework.Is.EqualTo(expectedItemsInfo).Using(CustomComparers.TypeComparison), "Description");
		}

		[ExpectNoExceptions]
		public void TestWeightAndVolume()
		{
			var invoiceLines = GetInvoiceLines();
			var wrapper = (IEURGoodsSummary)new EURGoodsSummaryWrapper(invoiceLines);

			var expectedWeightsOrVolumes =
				"1.1 KG\r\n" +
				"1.2 KG\r\n" +
				"2.1 KG";
			NUnit.Framework.Assert.That(wrapper.WeightAndVolume, NUnit.Framework.Is.EqualTo(expectedWeightsOrVolumes).Using(CustomComparers.TypeComparison), "WeightAndVolume");
		}

		[ExpectNoExceptions]
		public void TestInvoiceNumbers()
		{
			var invoiceLines = GetInvoiceLines();
			GetInvoiceLines();
			var wrapper = (IEURGoodsSummary)new EURGoodsSummaryWrapper(invoiceLines);

			var expectedInvoices =
				"INV1\r\n" +
				"INV2";
			NUnit.Framework.Assert.That(wrapper.InvoiceNumbers, NUnit.Framework.Is.EqualTo(expectedInvoices).Using(CustomComparers.TypeComparison), "InvoiceNumbers");
		}

		IEnumerable<JobComInvoiceLine> GetInvoiceLines()
		{
			var declaration = Factory.New<JobDeclaration>();
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
			yield return invoice1line1;
			yield return invoice1line2;
			yield return invoice2line1;
		}
	}
}
