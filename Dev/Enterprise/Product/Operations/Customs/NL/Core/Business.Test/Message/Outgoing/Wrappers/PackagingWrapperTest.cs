using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class PackagingWrapperTest : DataProviderTestCase<PackagingWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new PackagingWrapper(null, 1));
	}

	public void TestSequenceNumeric()
	{
		AssertEquals("SequenceNumeric", 1, wrapper.SequenceNumeric);
	}

	public void TestMarksNumbersID()
	{
		package.CW_MarksAndNos = "AS ABOVE";
		AssertEquals("AS ABOVE", wrapper.MarksNumbersID);
	}

	public void TestQuantityQuantity()
	{
		var packInvoiceLine1 = package.InvoiceLinePivotCollection.AddNew();
		packInvoiceLine1.CHC_NumberOfPacks = 4;
		AssertEquals(4, wrapper.QuantityQuantity);
	}

	public void TestTypeCode()
	{
		package.CW_PackType = "BX";
		AssertEquals("BX", wrapper.TypeCode);
	}

	protected override PackagingWrapper GetProvider() => wrapper;

	protected override void SetUp()
	{
		base.SetUp();
		package = Factory.New<BasePackage>();
		wrapper = new PackagingWrapper(package, 1);
	}
	BasePackage package;
	PackagingWrapper wrapper;
}
