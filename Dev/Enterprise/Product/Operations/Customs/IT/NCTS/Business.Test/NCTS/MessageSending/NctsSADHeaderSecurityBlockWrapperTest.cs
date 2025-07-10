using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsSADHeaderSecurityBlockWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Should be exception when nctsHeader is null", () => GetWrapper(null));
		AssertExceptionThrown<ArgumentNullException>("Should be exception when nctsHeader.MovementHeader is null", () => GetWrapper(Factory.New<NctsHeader>()));
	}

	public void TestSpecificCircumstanceIndicator()
	{
		AssertEquals(nameof(wrapper.SpecificCircumstanceIndicator), ZString.Empty, wrapper.SpecificCircumstanceIndicator);
	}

	public void TestPlaceOfLoadingCode()
	{
		ZString unlocoCode = "LCODE";
		AssertEquals("Default PlaceOfLoadingCode", "", wrapper.PlaceOfLoadingCode);

		nctsMovementHeader.BM_PlaceOfLoading = unlocoCode;
		AssertEquals("PlaceOfLoadingCode", unlocoCode, wrapper.PlaceOfLoadingCode);
	}

	public void TestTransportChargesMethodOfPayment()
	{
		AssertEquals(nameof(wrapper.TransportChargesMethodOfPayment), ZString.Empty, wrapper.TransportChargesMethodOfPayment);
	}

	public void TestCommercialReferenceNumber()
	{
		AssertEquals(nameof(wrapper.CommercialReferenceNumber), ZString.Empty, wrapper.CommercialReferenceNumber);
	}

	public void TestConveyanceReferenceNumber()
	{
		AssertEquals(nameof(wrapper.ConveyanceReferenceNumber), ZString.Empty, wrapper.ConveyanceReferenceNumber);
	}

	public void TestPlaceOfUnloadingCode()
	{
		AssertEquals(nameof(wrapper.PlaceOfUnloadingCode), ZString.Empty, wrapper.PlaceOfUnloadingCode);
	}

	public void TestTransitCountries()
	{
		AssertArrayEqualsByElements(nameof(wrapper.TransitCountries), Array.Empty<ZString>(), wrapper.TransitCountries.ToArray());
	}

	public void TestConsignor()
	{
		AssertType<SADEmptyTraderWrapper>(nameof(wrapper.Consignor), wrapper.Consignor);
	}

	public void TestConsignee()
	{
		AssertType<SADEmptyTraderWrapper>(nameof(wrapper.Consignee), wrapper.Consignee);
	}

	public void TestCarrier()
	{
		AssertType<SADEmptyTraderWrapper>(nameof(wrapper.Carrier), wrapper.Carrier);
	}

	public void TestSealsNumber()
	{
		AssertEquals(nameof(wrapper.SealsNumber), 0, wrapper.SealsNumber);

		nctsMovementHeader.BM_SealQty = 9;
		AssertEquals(nameof(wrapper.SealsNumber), 9, wrapper.SealsNumber);
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.NewDepartureNctsHeader();
		nctsMovementHeader = nctsHeader.MovementHeader;
		wrapper = GetWrapper(nctsHeader);
	}
	NctsHeader nctsHeader;
	NctsDepartureMovementHeader nctsMovementHeader;
	NctsSADHeaderSecurityBlockWrapper wrapper;

	NctsSADHeaderSecurityBlockWrapper GetWrapper(NctsHeader nctsHeader) => new NctsSADHeaderSecurityBlockWrapper(nctsHeader);
}
