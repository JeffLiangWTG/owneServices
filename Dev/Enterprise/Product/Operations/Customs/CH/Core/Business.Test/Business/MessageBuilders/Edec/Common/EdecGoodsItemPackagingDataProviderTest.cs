using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business.Testing;

public class EdecGoodsItemPackagingDataProviderTest : TestCaseWithFactory
{
	public void TestNew()
	{
		CombineAssertions(() =>
		{
			AssertNull("Argument == null", EdecGoodsItemPackagingDataProvider.New(null));
			AssertNull("Argument: empty collection", EdecGoodsItemPackagingDataProvider.New(Enumerable.Empty<Customs.Business.InvoiceLinePackagePivot>()));
			AssertNotNull("InvoiceLinePackagePivot.Package != null", EdecGoodsItemPackagingDataProvider.New(new[] { packageInvoiceLine }));

			packageInvoiceLine.CHC_CW = ZGuid.Empty;
			AssertNull("InvoiceLinePackagePivot.Package == null", EdecGoodsItemPackagingDataProvider.New(new[] { packageInvoiceLine }));
		});
	}

	public void TestProvider()
	{
		string packageType = "ABC";
		string marksAndNos = "MARK AND NOS 1";
		ZDecimal numberOfPacks = 2;

		package.CW_PackType = packageType;
		package.CW_MarksAndNos = marksAndNos;
		packageInvoiceLine.CHC_NumberOfPacks = numberOfPacks.ToZInt();

		var dataProvider = EdecGoodsItemPackagingDataProvider.New(new[] { packageInvoiceLine });

		CombineAssertions(() =>
		{
			AssertEquals(nameof(dataProvider.PackagingType), packageType, dataProvider.PackagingType);
			AssertEquals(nameof(dataProvider.Quantity), numberOfPacks, dataProvider.Quantity);
			AssertEquals(nameof(dataProvider.PackagingReferenceNumber), marksAndNos, dataProvider.PackagingReferenceNumber);
		});
	}

	public void TestNewCollection()
	{
		package.CW_PackType = "ABC";
		package.CW_MarksAndNos = "MARK AND NOS 1";
		packageInvoiceLine.CHC_NumberOfPacks = 2;

		var package2 = declaration.Packages.AddNew();
		package2.CW_PackType = "DEF";
		package2.CW_MarksAndNos = "MARK AND NOS 2";

		var invoiceLine2 = entryLine.InvoiceLines.AddNew();
		var packageInvoiceLine2 = invoiceLine2.PackagesPivot.AddNew();
		packageInvoiceLine2.CHC_CW = package2.PK;
		packageInvoiceLine2.CHC_NumberOfPacks = 3;

		var packageInvoiceLine3 = invoiceLine.PackagesPivot.AddNew();
		packageInvoiceLine3.CHC_CW = package.PK;
		packageInvoiceLine3.CHC_NumberOfPacks = 4;
		var packageInvoiceLine4 = invoiceLine2.PackagesPivot.AddNew();
		packageInvoiceLine4.CHC_CW = package2.PK;
		packageInvoiceLine4.CHC_NumberOfPacks = 5;
		invoiceLine.PackagesPivot.AddNew();

		var dataProviders = EdecGoodsItemPackagingDataProvider.NewCollection(entryLine).OrderBy(x => x.PackagingType).ToArray();

		CombineAssertions(() =>
		{
			AssertEquals("PackagingType", "ABC", dataProviders[0].PackagingType);
			AssertEquals("Quantity", 6m, dataProviders[0].Quantity);
			AssertEquals("PackagingReferenceNumber", "MARK AND NOS 1", dataProviders[0].PackagingReferenceNumber);

			AssertEquals("PackagingType", "DEF", dataProviders[1].PackagingType);
			AssertEquals("Quantity", 8m, dataProviders[1].Quantity);
			AssertEquals("PackagingReferenceNumber", "MARK AND NOS 2", dataProviders[1].PackagingReferenceNumber);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryLine = entryHeader.AllEntryLines.AddNew();

		invoiceLine = entryLine.InvoiceLines.AddNew();
		package = declaration.Packages.AddNew();
		packageInvoiceLine = invoiceLine.PackagesPivot.AddNew();
		packageInvoiceLine.CHC_CW = package.PK;
	}

	JobDeclaration declaration;
	CusEntryLine entryLine;
	BaseJobComInvoiceLine invoiceLine;
	BasePackage package;
	Customs.Business.InvoiceLinePackagePivot packageInvoiceLine;
}
