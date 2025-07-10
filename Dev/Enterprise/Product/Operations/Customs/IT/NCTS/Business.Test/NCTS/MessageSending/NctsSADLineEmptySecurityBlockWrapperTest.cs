using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsSADLineEmptySecurityBlockWrapperTest : TestCaseWithFactory
{
	public void TestConsignor()
	{
		AssertType<SADEmptyTraderWrapper>($"{nameof(lineSecurityBlockWrapper.Consignor)} type", lineSecurityBlockWrapper.Consignor);
	}

	public void TestConsignee()
	{
		AssertType<SADEmptyTraderWrapper>($"{nameof(lineSecurityBlockWrapper.Consignee)} type", lineSecurityBlockWrapper.Consignee);
	}

	public void TestTransportChargesMethodOfPayment()
	{
		AssertEquals(nameof(lineSecurityBlockWrapper.TransportChargesMethodOfPayment), ZString.Empty, lineSecurityBlockWrapper.TransportChargesMethodOfPayment);
	}

	public void TestCommercialReferenceNumber()
	{
		AssertEquals(nameof(lineSecurityBlockWrapper.CommercialReferenceNumber), ZString.Empty, lineSecurityBlockWrapper.CommercialReferenceNumber);
	}

	public void TestUNDangerousGoodsCode()
	{
		AssertEquals(nameof(lineSecurityBlockWrapper.UNDangerousGoodsCode), ZString.Empty, lineSecurityBlockWrapper.UNDangerousGoodsCode);
	}

	protected override void SetUp()
	{
		base.SetUp();
		lineSecurityBlockWrapper = new NctsSADLineEmptySecurityBlockWrapper();
	}
	NctsSADLineEmptySecurityBlockWrapper lineSecurityBlockWrapper;
}
