using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsSADHeaderSafetyAndSecurityBlockWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Should be exception when nctsHeader is null", () => GetWrapper(null));
		AssertExceptionThrown<ArgumentNullException>("Should be exception when nctsHeader.MovementHeader is null", () => GetWrapper(Factory.New<NctsHeader>()));
	}

	public void TestSpecificCircumstanceIndicator()
	{
		AssertEquals(nameof(headerSecurityBlockWrapper.SpecificCircumstanceIndicator), ZString.Empty, headerSecurityBlockWrapper.SpecificCircumstanceIndicator);

		nctsMovementHeader.BM_BTAIndicator = "E";
		AssertEquals(nameof(headerSecurityBlockWrapper.SpecificCircumstanceIndicator), "E", headerSecurityBlockWrapper.SpecificCircumstanceIndicator);
	}

	public void TestPlaceOfLoadingCode()
	{
		ZString unlocoCode = "LCODE";
		AssertEquals("Default PlaceOfLoadingCode", "", headerSecurityBlockWrapper.PlaceOfLoadingCode);

		nctsMovementHeader.BM_PlaceOfLoading = unlocoCode;
		AssertEquals("PlaceOfLoadingCode", unlocoCode, headerSecurityBlockWrapper.PlaceOfLoadingCode);
	}

	public void TestTransportChargesMethodOfPayment()
	{
		AssertEquals(nameof(headerSecurityBlockWrapper.TransportChargesMethodOfPayment), ZString.Empty, headerSecurityBlockWrapper.TransportChargesMethodOfPayment);

		nctsMovementHeader.BM_MethodOfPayment = "X";
		AssertEquals(nameof(headerSecurityBlockWrapper.TransportChargesMethodOfPayment), "X", headerSecurityBlockWrapper.TransportChargesMethodOfPayment);
	}

	public void TestCommercialReferenceNumber()
	{
		AssertEquals(nameof(headerSecurityBlockWrapper.CommercialReferenceNumber), ZString.Empty, headerSecurityBlockWrapper.CommercialReferenceNumber);

		nctsMovementHeader.BM_AdditionalText = "ADDITIONAL TEXT";
		AssertEquals(nameof(headerSecurityBlockWrapper.CommercialReferenceNumber), "ADDITIONAL TEXT", headerSecurityBlockWrapper.CommercialReferenceNumber);
	}

	public void TestConveyanceReferenceNumber()
	{
		AssertEquals(nameof(headerSecurityBlockWrapper.ConveyanceReferenceNumber), ZString.Empty, headerSecurityBlockWrapper.ConveyanceReferenceNumber);

		nctsMovementHeader.BM_ConveyanceNumber = "2";
		AssertEquals(nameof(headerSecurityBlockWrapper.ConveyanceReferenceNumber), "2", headerSecurityBlockWrapper.ConveyanceReferenceNumber);
	}

	public void TestPlaceOfUnloadingCode()
	{
		AssertEquals(nameof(headerSecurityBlockWrapper.PlaceOfUnloadingCode), ZString.Empty, headerSecurityBlockWrapper.PlaceOfUnloadingCode);

		nctsMovementHeader.BM_PlaceOfUnloading = "LOC";
		AssertEquals(nameof(headerSecurityBlockWrapper.PlaceOfUnloadingCode), "LOC", headerSecurityBlockWrapper.PlaceOfUnloadingCode);
	}

	public void TestTransitCountries()
	{
		AssertNotNull(nameof(headerSecurityBlockWrapper.TransitCountries), headerSecurityBlockWrapper.TransitCountries);
		AssertArrayEqualsByElements(nameof(headerSecurityBlockWrapper.TransitCountries), Array.Empty<ZString>(), headerSecurityBlockWrapper.TransitCountries.ToArray());

		nctsHeader.Itinerary.AddNew().CountryCode = "CH";
		nctsHeader.Itinerary.AddNew().CountryCode = "DE";
		AssertArrayEqualsByElements(nameof(headerSecurityBlockWrapper.TransitCountries), new ZString[] { "CH", "DE" }, headerSecurityBlockWrapper.TransitCountries.ToArray());
	}

	public void TestConsignorWhenNoLinkedAddress()
	{
		nctsHeader.SecurityConsignor.E2_OA_Address = ZGuid.Empty;
		AssertType<SADEmptyTraderWrapper>(nameof(headerSecurityBlockWrapper.Consignor), headerSecurityBlockWrapper.Consignor);
	}

	public void TestConsignorWhenLinkedAddress()
	{
		var supplier = Factory.New<OrgHeader>();
		supplier.CustomsCodes.AddNew("EOR", "385040449", "IT");
		var address = supplier.Addresses.AddNew();
		address.CompanyName = "IKEA";
		address.Address1 = "MAIN";
		address.Address2 = "ADDRESS";
		address.Postcode = "4000";
		address.City = "ABCEXPMEL";
		address.OA_RN_NKCountryCode = "ZA";

		nctsHeader.SecurityConsignor.E2_OA_Address = address.PK;
		CombineAssertions(() =>
		{
			var consignor = headerSecurityBlockWrapper.Consignor;
			AssertType<SADTraderWrapper>(consignor);
			AssertEquals("IdCountryCode", "IT", consignor.IdCountryCode);
			AssertEquals("ID", "385040449", consignor.ID);
			AssertEquals("Name", "IKEA", consignor.Name);
			AssertEquals("Address", "MAIN ADDRESS", consignor.Address);
			AssertEquals("Postcode", "4000", consignor.Postcode);
			AssertEquals("City", "ABCEXPMEL", consignor.City);
			AssertEquals("CountryCode", "ZA", consignor.CountryCode);
		});
	}

	public void TestConsigneeWhenNoLinkedAddress()
	{
		nctsHeader.SecurityConsignee.E2_OA_Address = ZGuid.Empty;
		AssertType<SADEmptyTraderWrapper>(nameof(headerSecurityBlockWrapper.Consignee), headerSecurityBlockWrapper.Consignee);
	}

	public void TestConsigneeWhenLinkedAddress()
	{
		var supplier = Factory.New<OrgHeader>();
		supplier.CustomsCodes.AddNew("EOR", "385040449", "IT");
		var address = supplier.Addresses.AddNew();
		address.CompanyName = "YKK MEDITERRANEO SPA";
		address.Address1 = "ZONA IND. CAMPOLUNGO";
		address.Postcode = "63100";
		address.City = "ASCOLI PICENO";
		address.OA_RN_NKCountryCode = "IT";

		nctsHeader.SecurityConsignee.E2_OA_Address = address.PK;
		CombineAssertions(() =>
		{
			var consignee = headerSecurityBlockWrapper.Consignee;
			AssertType<SADTraderWrapper>(consignee);
			AssertEquals("IdCountryCode", "IT", consignee.IdCountryCode);
			AssertEquals("ID", "385040449", consignee.ID);
			AssertEquals("Name", "YKK MEDITERRANEO SPA", consignee.Name);
			AssertEquals("Address", "ZONA IND. CAMPOLUNGO", consignee.Address);
			AssertEquals("Postcode", "63100", consignee.Postcode);
			AssertEquals("City", "ASCOLI PICENO", consignee.City);
			AssertEquals("CountryCode", "IT", consignee.CountryCode);
		});
	}

	public void TestCarrierWhenNoLinkedAddress()
	{
		nctsHeader.BH_OH_Carrier = ZGuid.Empty;
		AssertType<SADEmptyTraderWrapper>(nameof(headerSecurityBlockWrapper.Carrier), headerSecurityBlockWrapper.Carrier);
	}

	public void TestCarrierWhenLinkedAddress()
	{
		var carrierOrgHeader = Factory.New<OrgHeader>();
		carrierOrgHeader.CustomsCodes.AddNew("EOR", "374293943", "IT");
		var address = carrierOrgHeader.MainAddress;
		address.CompanyName = "MAERSK Italia";
		address.Address1 = "Via Magazzini del Cotone";
		address.Postcode = "16128";
		address.City = "GENOVA";
		address.OA_RN_NKCountryCode = "IT";

		nctsHeader.BH_OH_Carrier = carrierOrgHeader.PK;
		CombineAssertions(() =>
		{
			var carrier = headerSecurityBlockWrapper.Carrier;
			AssertType<SADTraderWrapper>(carrier);
			AssertEquals("IdCountryCode", "IT", carrier.IdCountryCode);
			AssertEquals("ID", "374293943", carrier.ID);
			AssertEquals("Name", "MAERSK Italia", carrier.Name);
			AssertEquals("Address", "Via Magazzini del Cotone", carrier.Address);
			AssertEquals("Postcode", "16128", carrier.Postcode);
			AssertEquals("City", "GENOVA", carrier.City);
			AssertEquals("CountryCode", "IT", carrier.CountryCode);
		});
	}

	public void TestSealsNumber()
	{
		AssertEquals(nameof(headerSecurityBlockWrapper.SealsNumber), 0, headerSecurityBlockWrapper.SealsNumber);

		nctsMovementHeader.BM_SealQty = 9;
		AssertEquals(nameof(headerSecurityBlockWrapper.SealsNumber), 9, headerSecurityBlockWrapper.SealsNumber);
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.NewDepartureNctsHeader();
		nctsMovementHeader = nctsHeader.MovementHeader;
		headerSecurityBlockWrapper = GetWrapper(nctsHeader);
	}
	NctsHeader nctsHeader;
	NctsDepartureMovementHeader nctsMovementHeader;
	NctsSADHeaderSafetyAndSecurityBlockWrapper headerSecurityBlockWrapper;

	NctsSADHeaderSafetyAndSecurityBlockWrapper GetWrapper(NctsHeader nctsHeader) => new NctsSADHeaderSafetyAndSecurityBlockWrapper(nctsHeader);
}
