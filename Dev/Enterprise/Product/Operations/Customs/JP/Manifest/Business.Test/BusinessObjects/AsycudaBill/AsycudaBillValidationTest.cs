using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.JP.Manifest.Business.Test
{
	public abstract class AsycudaBillValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckABL_GoodsDescription()
		{
			var expectedWarning = "The designated character string sent to customs is";

			Header.AMA_TransportMode = "SEA";
			Bill.ABL_GoodsDescription = "123456789012345678901";
			AssertNoWarningContaining(Bill.ABL_GoodsDescriptionInfo, expectedWarning);

			Bill.ABL_GoodsDescription = "1234567890123456789012";
			AssertNoWarningContaining(Bill.ABL_GoodsDescriptionInfo, expectedWarning);

			Header.AMA_TransportMode = "AIR";
			Bill.ABL_GoodsDescription = "123456789012345678901";
			AssertNoWarningContaining(Bill.ABL_GoodsDescriptionInfo, expectedWarning);

			Bill.ABL_GoodsDescription = "1234567890123456789013";
			AssertHasWarningContaining(Bill.ABL_GoodsDescriptionInfo, expectedWarning);
		}

		public void TestCheckABL_ManifestQty()
		{
			Header.AMA_TransportMode = "AIR";
			var expectedWarningHead = "The maximum allowable quantity is 999,999.";

			Bill.ABL_ManifestQty = 999999;
			AssertNoMessageError(Bill.ABL_ManifestQtyInfo, expectedWarningHead);
			Bill.ABL_ManifestQty = 1000000;
			AssertHasMessageError(Bill.ABL_ManifestQtyInfo, expectedWarningHead);

			Header.AMA_TransportMode = "SEA";
			expectedWarningHead = "The maximum allowable quantity is 99,999,999.";

			Bill.ABL_ManifestQty = 99999999;
			AssertNoMessageError(Bill.ABL_ManifestQtyInfo, expectedWarningHead);
			Bill.ABL_ManifestQty = 100000000;
			AssertHasMessageError(Bill.ABL_ManifestQtyInfo, expectedWarningHead);
		}

		public void TestCheckABL_NetWeight()
		{
			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			var expectedWarning = "Weight cannot be negative.";

			Bill.ABL_NetWeight = 1m;
			AssertNoError(Bill.ABL_NetWeightInfo, expectedWarning);
			Bill.ABL_NetWeight = -1m;
			AssertHasError(Bill.ABL_NetWeightInfo, expectedWarning);
		}

		public virtual void TestCheckABL_ShipperStreet2()
		{
			AssertByteLength(Bill.ABL_ShipperStreet2Info, 35);
		}

		public void TestABL_ShipperCity()
		{
			AssertByteLength(Bill.ABL_ShipperCityInfo, 35);
		}

		public void TestCheckABL_ShipperPostcode()
		{
			AssertByteLength(Bill.ABL_ShipperPostcodeInfo, 9);
		}

		public void TestABL_ShipperPhone()
		{
			AssertPhoneNumberLength(Bill.ABL_ShipperPhoneInfo);
		}

		public void TestABL_ConsigneeStreet2()
		{
			AssertByteLength(Bill.ABL_ConsigneeStreet2Info, 35);
		}

		public void TestABL_ConsigneeCity()
		{
			AssertByteLength(Bill.ABL_ConsigneeCityInfo, 35);
		}

		public void TestABL_ConsigneePostcode()
		{
			AssertByteLength(Bill.ABL_ConsigneePostcodeInfo, 9);
		}

		public void TestABL_ConsigneePhone()
		{
			AssertPhoneNumberLength(Bill.ABL_ConsigneePhoneInfo);
		}

		public void TestABL_NotifyPartyStreet2()
		{
			AssertByteLength(Bill.ABL_NotifyPartyStreet2Info, 35);
		}

		public void TestABL_NotifyPartyCity()
		{
			AssertByteLength(Bill.ABL_NotifyPartyCityInfo, 35);
		}

		public void TestABL_NotifyPartyPostcode()
		{
			AssertByteLength(Bill.ABL_NotifyPartyPostcodeInfo, 9);
		}

		public void TestCheckABL_NotifyPartyPhone()
		{
			AssertPhoneNumberLength(Bill.ABL_NotifyPartyPhoneInfo);
		}

		void AssertByteLength(ZPropertyInfo info, int maxLength)
		{
			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;

			var expectedWarningPart = "The designated character string sent to customs is";
			var validString = ZString.Replicate('駅', maxLength / 2);
			Bill.SetPropertyValue(info.Name, validString);
			AssertNoWarningContaining(info, expectedWarningPart);

			Bill.SetPropertyValue(info.Name, new ZString(validString + "ああ"));
			AssertHasWarningContaining(info, expectedWarningPart);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			Bill.SetPropertyValue(info.Name, new ZString(validString + "あい"));
			AssertNoWarningContaining(info, expectedWarningPart);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;

			validString = ZString.Replicate('a', maxLength);
			Bill.SetPropertyValue(info.Name, validString);
			AssertNoWarningContaining(info, expectedWarningPart);

			Bill.SetPropertyValue(info.Name, new ZString(validString + "a"));
			AssertHasWarningContaining(info, expectedWarningPart);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			Bill.SetPropertyValue(info.Name, new ZString(validString + "i"));
			AssertNoWarningContaining(info, expectedWarningPart);
		}

		void AssertPhoneNumberLength(ZPropertyInfo info)
		{
			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			var expectedWarningPart = "The designated character string sent to customs will be";
			ZString validString = "+1 223-456-789";
			Bill.SetPropertyValue(info.Name, validString);
			AssertNoWarningContaining(info, expectedWarningPart);
			Bill.SetPropertyValue(info.Name, new ZString(validString + "1"));
			AssertHasWarningContaining(info, expectedWarningPart);
		}

		protected virtual AsycudaBill GetAsycudaBillForTest() => Header.Bills.AddNew();

		protected AsycudaManifestHeader Header => header ??= Factory.New<AsycudaManifestHeader>();
		AsycudaManifestHeader header;

		protected AsycudaBill Bill => bill ??= GetAsycudaBillForTest();
		AsycudaBill bill;
	}
}
