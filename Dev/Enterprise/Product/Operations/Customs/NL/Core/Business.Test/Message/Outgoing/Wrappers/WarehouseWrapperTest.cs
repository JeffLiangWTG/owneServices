using System;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class WarehouseWrapperTest : DataProviderTestCase<WarehouseWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new WarehouseWrapper(null));
	}

	public void TestId()
	{
		authorisationHeader.CPH_Number = "34321532542353";
		AssertEquals("34321532542353", wrapper.Id);
	}

	public void TestTypeCode()
	{
		authorisationHeader.CPH_Type = "ACT";
		AssertEquals("Authorized Consignee TIR", wrapper.TypeCode);
	}

	protected override void SetUp()
	{
		base.SetUp();
		authorisationHeader = Factory.New<CusAuthorisationHeader>();
		wrapper = new WarehouseWrapper(authorisationHeader);
	}
	CusAuthorisationHeader authorisationHeader;
	WarehouseWrapper wrapper;

	protected override WarehouseWrapper GetProvider() => wrapper;
}
