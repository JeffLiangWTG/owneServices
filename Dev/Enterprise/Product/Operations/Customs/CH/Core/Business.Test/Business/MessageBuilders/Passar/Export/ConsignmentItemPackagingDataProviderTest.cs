using System.Linq;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(ConsignmentItemPackagingDataProvider))]
sealed class ConsignmentItemPackagingDataProviderTest : BasePassarDataProviderTest<ConsignmentItemPackagingDataProvider>
{
	public void TestNew() => CombineAssertions(() =>
	{
		AssertNull("Argument == null", ConsignmentItemPackagingDataProvider.New(null, 1));
		AssertNull("Argument: empty collection", ConsignmentItemPackagingDataProvider.New(Enumerable.Empty<Customs.Business.InvoiceLinePackagePivot>(), 1));
		AssertNotNull("InvoiceLinePackagePivot.Package != null", ConsignmentItemPackagingDataProvider.New(new[] { PackageInvoiceLine }, 1));

		PackageInvoiceLine.CHC_CW = ZGuid.Empty;
		AssertNull("InvoiceLinePackagePivot.Package == null", ConsignmentItemPackagingDataProvider.New(new[] { PackageInvoiceLine }, 1));
	});

	public void TestProvider() => CombineAssertions(() =>
	{
		const string packageType = "ABC";
		const string marksAndNos = "MARK AND NOS 1";
		const int numberOfPacks = 2;

		Package.CW_PackType = packageType;
		Package.CW_MarksAndNos = marksAndNos;

		PackageInvoiceLine.CHC_NumberOfPacks = numberOfPacks;

		var dataProvider = ConsignmentItemPackagingDataProvider.New(new[] { PackageInvoiceLine }, 1);

		AssertEquals(nameof(dataProvider.SequenceNumber), 1, dataProvider.SequenceNumber);
		AssertEquals(nameof(dataProvider.TypeOfPackages), packageType, dataProvider.TypeOfPackages);
		AssertEquals(nameof(dataProvider.NumberOfPackages), numberOfPacks, dataProvider.NumberOfPackages);
		AssertEquals(nameof(dataProvider.ShippingMarks), marksAndNos, dataProvider.ShippingMarks);
		AssertEquals(nameof(dataProvider.BypackCorrelationIdentifier), null, dataProvider.BypackCorrelationIdentifier);
		AssertEquals(nameof(dataProvider.BypackType), null, dataProvider.BypackType);
	});

	public void TestNumberOfPackages()
	{
		RefCusCodeTestHelper.CreateUNPKGCodeList(Factory);

		Package.CW_PackType = RefCusCodeTestHelper.UNPKGCodeNoBulk;
		PackageInvoiceLine.CHC_NumberOfPacks = 0;

		var dataProvider = ConsignmentItemPackagingDataProvider.New(new[] { PackageInvoiceLine }, 1);

		AssertEquals("PackType not bulk and NumberOfPacks is zero, value is mapped", 0, dataProvider.NumberOfPackages);

		PackageInvoiceLine.CHC_NumberOfPacks = 1;
		AssertEquals("PackType not bulk and NumberOfPacks is greater than zero, value is mapped", 1, dataProvider.NumberOfPackages);

		Package.CW_PackType = RefCusCodeTestHelper.UNPKGCodeNoBulk;
		AssertEquals("PackType bulk and NumberOfPacks is greater than zero, value is mapped", 1, dataProvider.NumberOfPackages);

		Package.CW_PackType = RefCusCodeTestHelper.UNPKGCodeWithBulkYes;
		PackageInvoiceLine.CHC_NumberOfPacks = 0;
		AssertEquals("PackType bulk and NumberOfPacks is zero, value is null", null, dataProvider.NumberOfPackages);
	}

	public void TestNewCollection()
	{
		Package.CW_PackType = "ABC";
		Package.CW_MarksAndNos = "MARK AND NOS 1";

		var packageInvoiceLine = InvoiceLine.PackagesPivot.Cast<InvoiceLinePackagePivot>().FirstOrDefault();
		packageInvoiceLine.CHC_NumberOfPacks = 2;

		var package2 = EntryLine.Declaration.Packages.AddNew();
		package2.CW_PackType = "DEF";
		package2.CW_MarksAndNos = "MARK AND NOS 2";

		var invoiceLine2 = EntryLine.InvoiceLines.AddNew();
		var packageInvoiceLine2 = invoiceLine2.PackagesPivot.AddNew();
		packageInvoiceLine2.CHC_CW = package2.PK;
		packageInvoiceLine2.CHC_NumberOfPacks = 3;

		var packageInvoiceLine3 = InvoiceLine.PackagesPivot.AddNew();
		packageInvoiceLine3.CHC_CW = package.PK;
		packageInvoiceLine3.CHC_NumberOfPacks = 4;
		var packageInvoiceLine4 = invoiceLine2.PackagesPivot.AddNew();
		packageInvoiceLine4.CHC_CW = package2.PK;
		packageInvoiceLine4.CHC_NumberOfPacks = 5;
		InvoiceLine.PackagesPivot.AddNew();

		var dataProviders = ConsignmentItemPackagingDataProvider.NewCollection(EntryLine).OrderBy(x => x.TypeOfPackages).ToArray();

		CombineAssertions(() =>
		{
			AssertEquals("Sequence Number", 1, dataProviders[0].SequenceNumber);
			AssertEquals("PackagingType", "ABC", dataProviders[0].TypeOfPackages);
			AssertEquals("Quantity", 6, dataProviders[0].NumberOfPackages);
			AssertEquals("PackagingReferenceNumber", "MARK AND NOS 1", dataProviders[0].ShippingMarks);

			AssertEquals("Sequence Number", 2, dataProviders[1].SequenceNumber);
			AssertEquals("PackagingType", "DEF", dataProviders[1].TypeOfPackages);
			AssertEquals("Quantity", 8, dataProviders[1].NumberOfPackages);
			AssertEquals("PackagingReferenceNumber", "MARK AND NOS 2", dataProviders[1].ShippingMarks);
		});
	}

	Package Package => package ??= CreatePackage();
	Package package;
	Package CreatePackage()
	{
		var package = Declaration.Packages.AddNew();
		var packageInvoiceLine = InvoiceLine.PackagesPivot.AddNew();
		packageInvoiceLine.CHC_CW = package.PK;
		return package;
	}

	InvoiceLinePackagePivot PackageInvoiceLine => Package.InvoiceLinePivotCollection.Cast<InvoiceLinePackagePivot>().First();

	protected override ConsignmentItemPackagingDataProvider CreateDataProvider() => ConsignmentItemPackagingDataProvider.New(EntryLine.RandomLine.PackagesPivot.Cast<InvoiceLinePackagePivot>(), 1);
}
