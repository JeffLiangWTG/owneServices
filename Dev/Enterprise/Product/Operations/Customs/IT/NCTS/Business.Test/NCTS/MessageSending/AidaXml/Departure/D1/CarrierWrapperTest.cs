using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class CarrierWrapperTest : TestCaseWithFactory
{
	public void TestNewOrNull_ReturnsNullWrapperIfCarrierIsNull()
	{
		var carrierWrapper = CarrierWrapper.NewOrNull(carrier: null, principalHeader);
		AssertNull("CarrierWrapper", carrierWrapper);
	}

	public void TestNewOrNull_ReturnsNullWrapperIfCarrierDoesNotHaveIdentificationNumber()
	{
		var carrierWrapper = CarrierWrapper.NewOrNull(carrierHeader, principalHeader);
		AssertNull("CarrierWrapper", carrierWrapper);
	}

	public void TestNewOrNull_ReturnsWrapperIfCarrierHasEoriAndPrincipalIsNull()
	{
		carrierHeader.CustomsCodes.AddNew("EOR", "MYEORI");

		var carrierWrapper = CarrierWrapper.NewOrNull(carrierHeader, principal: null);
		AssertType<CarrierWrapper>("CarrierWrapper Type", carrierWrapper);
		AssertEquals("CarrierWrapper IdentificationNumber", "ITMYEORI", carrierWrapper.IdentificationNumber);
	}

	public void TestNewOrNull_ReturnsWrapperIfCarrierHasEoriAndPrincipalHasNot()
	{
		carrierHeader.CustomsCodes.AddNew("EOR", "MYEORI");

		var carrierWrapper = CarrierWrapper.NewOrNull(carrierHeader, principalHeader);
		AssertType<CarrierWrapper>("CarrierWrapper Type", carrierWrapper);
		AssertEquals("CarrierWrapper IdentificationNumber", "ITMYEORI", carrierWrapper.IdentificationNumber);
	}

	public void TestNewOrNull_ReturnsWrapperIfCarrierHasTcuAndPrincipalHasNot()
	{
		carrierHeader.CustomsCodes.AddNew("TCU", "MYTCU");

		var carrierWrapper = CarrierWrapper.NewOrNull(carrierHeader, principalHeader);
		AssertType<CarrierWrapper>("CarrierWrapper Type", carrierWrapper);
		AssertEquals("CarrierWrapper IdentificationNumber", "ITMYTCU", carrierWrapper.IdentificationNumber);
	}

	public void TestNewOrNull_ReturnsWrapperIfCarrierAndPrincipalHaveDifferentIdentificationNumber()
	{
		carrierHeader.CustomsCodes.AddNew("EOR", "MYEORI");
		principalHeader.CustomsCodes.AddNew("EOR", "ANOTHEREORI");

		var carrierWrapper = CarrierWrapper.NewOrNull(carrierHeader, principalHeader);
		AssertType<CarrierWrapper>("CarrierWrapper Type", carrierWrapper);
		AssertEquals("CarrierWrapper IdentificationNumber", "ITMYEORI", carrierWrapper.IdentificationNumber);
	}

	public void TestNewOrNull_ReturnsNullIfCarrierAndPrincipalHaveSameIdentificationNumber()
	{
		carrierHeader.CustomsCodes.AddNew("EOR", "MYEORI");
		principalHeader.CustomsCodes.AddNew("EOR", "MYEORI");

		var carrierWrapper = CarrierWrapper.NewOrNull(carrierHeader, principalHeader);
		AssertNull("CarrierWrapper", carrierWrapper);
	}

	protected override void SetUp()
	{
		base.SetUp();
		carrierHeader = Factory.New<OrgHeader>();
		principalHeader = Factory.New<OrgHeader>();
	}

	OrgHeader carrierHeader;
	OrgHeader principalHeader;
}
