using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing;

public class LineValueWrapperTest : DataProviderTestCase<LineValueWrapper>
{
	public void TestAmount()
	{
		item.API_GoodsValue = 100;
		AssertEquals((ZDecimal)100, wrapper.Amount);
	}

	public void TestCurrencyCode()
	{
		item.API_RX_NKGoodsValueCurrency = Constants.CurrencyCodes.Australia;
		AssertEquals(Constants.CurrencyCodes.Australia, wrapper.CurrencyCode);
	}

	public void TestAmount_RoundsToTwoDecimalPlaces()
	{
		item.API_GoodsValue = 20.4825;
		item.API_RX_NKGoodsValueCurrency = Constants.CurrencyCodes.Australia;

		CombineAssertions("Amount is rounded to two decimal places", () =>
		{
			AssertEquals((ZDecimal)20.48, wrapper.Amount);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		var broker = Factory.NewWithValidTestData<GlbStaff>();
		var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		header.AMA_GS_NKCustomsAgent = broker.GS_Code;
		var bill = header.Bills.AddNew();
		item = bill.PackedItems.AddNew();

		wrapper = new LineValueWrapper(item);
	}

	protected override LineValueWrapper GetProvider()
	{
		return wrapper;
	}

	LineValueWrapper wrapper;
	AsycudaPackedItem item;
}
