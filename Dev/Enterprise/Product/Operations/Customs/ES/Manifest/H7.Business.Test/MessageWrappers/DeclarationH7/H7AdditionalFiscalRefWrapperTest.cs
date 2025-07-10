using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing;

public class H7AdditionalFiscalRefWrapperTest : DataProviderTestCase<H7AdditionalFiscalRefWrapper>
{
	public void TestRole()
	{
		bill.ABL_SellerRegNo = "SellerRegNo";
		AssertEquals("Should map to FR5 when SellerRegNo available", "FR5", wrapper.Role);

		bill.ABL_SellerRegNo = ZString.Empty;
		AssertEquals("Should be empty when SellerRegNo empty", ZString.Empty, wrapper.Role);
	}

	public void TestId()
	{
		bill.ABL_SellerRegNo = "SellerRegNo";
		AssertEquals("SellerRegNo", wrapper.Id);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var broker = Factory.NewWithValidTestData<GlbStaff>();
		var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		header.AMA_GS_NKCustomsAgent = broker.GS_Code;
		bill = header.Bills.AddNew();

		wrapper = new H7AdditionalFiscalRefWrapper(bill);
	}

	protected override H7AdditionalFiscalRefWrapper GetProvider()
	{
		return wrapper;
	}

	H7AdditionalFiscalRefWrapper wrapper;
	AsycudaBill bill;
}
