using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Documents.DocDataObjects.Testing
{
	class PackageAndMarksAndNumbersInfoTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetPackageType()
		{
			var package = Factory.New<BasePackage>();
			package.CW_PackType = "CT";

			var invoiceLine = Factory.New<JobComInvoiceLine>();
			NUnit.Framework.Assert.That(new PackageAndMarksAndNumbersInfoForTest(invoiceLine).GetPackageType(package), NUnit.Framework.Is.EqualTo("CT"), "Package type should match CW_PackType.");
		}

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Exception expected when parameter is null", () => new PackageAndMarksAndNumbersInfo(null));
		}

		[ExpectNoExceptions]
		public void TestPackage()
		{
			var packageAndMarksAndNumbersInfo = new PackageAndMarksAndNumbersInfo(invoiceLine);
			NUnit.Framework.Assert.That(packageAndMarksAndNumbersInfo.Package, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), nameof(PackageAndMarksAndNumbersInfo.Package));

			var packageCtInvoiceLine1 = invoiceLine.PackagesPivot.AddNew();
			packageCtInvoiceLine1.CHC_CW = packageCt.PK;
			packageCtInvoiceLine1.CHC_NumberOfPacks = 99;
			packageAndMarksAndNumbersInfo = new PackageAndMarksAndNumbersInfo(invoiceLine);
			NUnit.Framework.Assert.That(packageAndMarksAndNumbersInfo.Package, NUnit.Framework.Is.EqualTo("99 CT;").Using(CustomComparers.TypeComparison), nameof(PackageAndMarksAndNumbersInfo.Package));

			packageCtInvoiceLine1.CHC_CW = ZGuid.Empty;
			packageAndMarksAndNumbersInfo = new PackageAndMarksAndNumbersInfo(invoiceLine);
			NUnit.Framework.Assert.That(packageAndMarksAndNumbersInfo.Package, NUnit.Framework.Is.EqualTo("99;").Using(CustomComparers.TypeComparison), nameof(PackageAndMarksAndNumbersInfo.Package));

			packageCtInvoiceLine1.CHC_CW = packageCt.PK;
			var package2InvoiceLine1 = invoiceLine.PackagesPivot.AddNew();
			package2InvoiceLine1.CHC_CW = packagePk.PK;
			package2InvoiceLine1.CHC_NumberOfPacks = 38;
			packageAndMarksAndNumbersInfo = new PackageAndMarksAndNumbersInfo(invoiceLine);
			NUnit.Framework.Assert.That(packageAndMarksAndNumbersInfo.Package, NUnit.Framework.Is.EqualTo("99 CT, 38 PK;").Using(CustomComparers.TypeComparison), nameof(PackageAndMarksAndNumbersInfo.Package));

			package2InvoiceLine1.CHC_CW = packageBulk.PK;
			packageAndMarksAndNumbersInfo = new PackageAndMarksAndNumbersInfo(invoiceLine, ignoreBulkPackage: true);
			NUnit.Framework.Assert.That(packageAndMarksAndNumbersInfo.Package, NUnit.Framework.Is.EqualTo("99 CT;").Using(CustomComparers.TypeComparison), nameof(PackageAndMarksAndNumbersInfo.Package));
		}

		[ExpectNoExceptions]
		public void TestMarksAndNumbers()
		{
			var packageAndMarksAndNumbersInfo = new PackageAndMarksAndNumbersInfo(invoiceLine);
			NUnit.Framework.Assert.That(packageAndMarksAndNumbersInfo.MarksAndNumbers, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), nameof(PackageAndMarksAndNumbersInfo.MarksAndNumbers));

			var packageCtInvoiceLine1 = invoiceLine.PackagesPivot.AddNew();
			packageCtInvoiceLine1.CHC_CW = packageCt.PK;
			packageAndMarksAndNumbersInfo = new PackageAndMarksAndNumbersInfo(invoiceLine);
			NUnit.Framework.Assert.That(packageAndMarksAndNumbersInfo.MarksAndNumbers, NUnit.Framework.Is.EqualTo("M&N1;").Using(CustomComparers.TypeComparison), nameof(PackageAndMarksAndNumbersInfo.MarksAndNumbers));

			packageCtInvoiceLine1.CHC_CW = ZGuid.Empty;
			packageAndMarksAndNumbersInfo = new PackageAndMarksAndNumbersInfo(invoiceLine);
			NUnit.Framework.Assert.That(packageAndMarksAndNumbersInfo.MarksAndNumbers, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), nameof(PackageAndMarksAndNumbersInfo.MarksAndNumbers));

			packageCtInvoiceLine1.CHC_CW = packageCt.PK;
			var packagePkInvoiceLine1 = invoiceLine.PackagesPivot.AddNew();
			packagePkInvoiceLine1.CHC_CW = packagePk.PK;
			packagePkInvoiceLine1.CHC_NumberOfPacks = 38;
			packageAndMarksAndNumbersInfo = new PackageAndMarksAndNumbersInfo(invoiceLine);
			NUnit.Framework.Assert.That(packageAndMarksAndNumbersInfo.MarksAndNumbers, NUnit.Framework.Is.EqualTo("M&N1, M&N2;").Using(CustomComparers.TypeComparison), nameof(PackageAndMarksAndNumbersInfo.MarksAndNumbers));
		}

		[ExpectNoExceptions]
		public void TestContainsBulkPackages()
		{
			var packageAndMarksAndNumbersInfo = new PackageAndMarksAndNumbersInfo(invoiceLine);
			NUnit.Framework.Assert.That(packageAndMarksAndNumbersInfo.ContainsBulkPackages, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), nameof(PackageAndMarksAndNumbersInfo.ContainsBulkPackages));

			var packageCtInvoiceLine1 = invoiceLine.PackagesPivot.AddNew();
			packageCtInvoiceLine1.CHC_CW = packageCt.PK;
			packageCtInvoiceLine1.CHC_NumberOfPacks = 99;
			packageAndMarksAndNumbersInfo = new PackageAndMarksAndNumbersInfo(invoiceLine);
			NUnit.Framework.Assert.That(packageAndMarksAndNumbersInfo.ContainsBulkPackages, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), nameof(PackageAndMarksAndNumbersInfo.ContainsBulkPackages));

			var packageBulkInvoiceLine1 = invoiceLine.PackagesPivot.AddNew();
			packageBulkInvoiceLine1.CHC_CW = packageBulk.PK;
			packageAndMarksAndNumbersInfo = new PackageAndMarksAndNumbersInfo(invoiceLine);
			NUnit.Framework.Assert.That(packageAndMarksAndNumbersInfo.ContainsBulkPackages, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), nameof(PackageAndMarksAndNumbersInfo.ContainsBulkPackages));
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			invoiceLine = declaration.InvoiceLines.AddNew();

			var billPackingGroup = declaration.Bills.AddNew().PackingGroups.AddNew();

			packageCt = declaration.Packages.AddNew();
			packageCt.CW_CR_HouseContainer = billPackingGroup.PK;
			packageCt.CW_PackQty = 100;
			packageCt.CW_PackType = "CT";
			packageCt.CW_MarksAndNos = "M&N1";

			packagePk = declaration.Packages.AddNew();
			packagePk.CW_CR_HouseContainer = billPackingGroup.PK;
			packagePk.CW_PackQty = 200;
			packagePk.CW_PackType = "PK";
			packagePk.CW_MarksAndNos = "M&N2";

			packageBulk = declaration.Packages.AddNew();
			packageBulk.CW_PackType = "VG";
			packageBulk.CW_MarksAndNos = "Bulk";
		}
		JobDeclaration declaration;
		BasePackage packageCt;
		BasePackage packagePk;
		BasePackage packageBulk;
		JobComInvoiceLine invoiceLine;
	}

	public class PackageAndMarksAndNumbersInfoForTest : PackageAndMarksAndNumbersInfo
	{
		public PackageAndMarksAndNumbersInfoForTest(JobComInvoiceLine invoiceLine) : base(invoiceLine)
		{
		}

		public new string GetPackageType(BasePackage package) => base.GetPackageType(package);
	}
}
