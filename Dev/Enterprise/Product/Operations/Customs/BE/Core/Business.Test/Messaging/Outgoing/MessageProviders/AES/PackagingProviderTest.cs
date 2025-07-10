using System;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class PackagingProviderTest : Customs.Business.Testing.DataProviderTestCase<PackagingProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new PackagingProvider(null, 0));
	}

	public void TestSequenceNumber()
	{
		AssertEquals(999, provider.SequenceNumber);
	}

	public void TestTypeOfPackages()
	{
		packagePivot.Package.CW_PackType = "Pck";
		AssertEquals("Pck", provider.TypeOfPackages);
	}

	public void TestNumberOfPackages()
	{
		packagePivot.CHC_NumberOfPacks = 1546;
		AssertEquals(1546, provider.NumberOfPackages);
	}

	public void TestShippingMarks()
	{
		packagePivot.Package.CW_MarksAndNos = "Mark and the numbers";
		AssertEquals("Mark and the numbers", provider.ShippingMarks);
	}

	protected override PackagingProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		packagePivot = Factory.NewWithValidTestData<InvoiceLinePackagePivot>();
		provider = new PackagingProvider(packagePivot, 999);
	}

	InvoiceLinePackagePivot packagePivot;
	PackagingProvider provider;
}
