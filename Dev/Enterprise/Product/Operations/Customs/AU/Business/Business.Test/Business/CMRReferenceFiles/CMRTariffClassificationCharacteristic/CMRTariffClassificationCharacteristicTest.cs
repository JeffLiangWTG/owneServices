using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRTariffClassificationCharacteristic))]
	sealed class CMRTariffClassificationCharacteristicTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => CMRTariffClassificationCharacteristic.New(Factory);

		public void TestIsTariffClassificationCharacteristic()
		{
			var tariffClassificationCharacteristic29 = CMRTariffClassificationCharacteristic.New(Factory);
			tariffClassificationCharacteristic29.TC_TariffClassificationSnapshotTariffClassificationNumber = "24029129";
			tariffClassificationCharacteristic29.TC_CharacteristicCode = 29;
			var tariffClassificationCharacteristic34 = CMRTariffClassificationCharacteristic.New(Factory);
			tariffClassificationCharacteristic34.TC_TariffClassificationSnapshotTariffClassificationNumber = "24029134";
			tariffClassificationCharacteristic34.TC_CharacteristicCode = 34;
			Factory.Save();

			AssertEquals(true, CMRTariffClassificationCharacteristic.IsTariffClassificationCharacteristic(Factory, "24029129", 29));
			AssertEquals(false, CMRTariffClassificationCharacteristic.IsTariffClassificationCharacteristic(Factory, "24029129", 34));
			AssertEquals(true, CMRTariffClassificationCharacteristic.IsTariffClassificationCharacteristic(Factory, "24029129", new short[] { 29, 34 }));

			AssertEquals(false, CMRTariffClassificationCharacteristic.IsTariffClassificationCharacteristic(Factory, "24029134", 29));
			AssertEquals(true, CMRTariffClassificationCharacteristic.IsTariffClassificationCharacteristic(Factory, "24029134", 34));
			AssertEquals(true, CMRTariffClassificationCharacteristic.IsTariffClassificationCharacteristic(Factory, "24029134", new short[] { 29, 34 }));
		}

		public void TestIsTariffExciseEquivalentGoods()
		{
			var tariffClassificationCharacteristic29 = CMRTariffClassificationCharacteristic.New(Factory);
			tariffClassificationCharacteristic29.TC_TariffClassificationSnapshotTariffClassificationNumber = "24029129";
			tariffClassificationCharacteristic29.TC_CharacteristicCode = 29;
			var tariffClassificationCharacteristic34 = CMRTariffClassificationCharacteristic.New(Factory);
			tariffClassificationCharacteristic34.TC_TariffClassificationSnapshotTariffClassificationNumber = "24029134";
			tariffClassificationCharacteristic34.TC_CharacteristicCode = 34;
			var tariffClassificationCharacteristic35 = CMRTariffClassificationCharacteristic.New(Factory);
			tariffClassificationCharacteristic35.TC_TariffClassificationSnapshotTariffClassificationNumber = "24029135";
			tariffClassificationCharacteristic35.TC_CharacteristicCode = 35;
			var tariffClassificationCharacteristic36 = CMRTariffClassificationCharacteristic.New(Factory);
			tariffClassificationCharacteristic36.TC_TariffClassificationSnapshotTariffClassificationNumber = "24029136";
			tariffClassificationCharacteristic36.TC_CharacteristicCode = 36;
			var tariffClassificationCharacteristic55 = CMRTariffClassificationCharacteristic.New(Factory);
			tariffClassificationCharacteristic55.TC_TariffClassificationSnapshotTariffClassificationNumber = "24029155";
			tariffClassificationCharacteristic55.TC_CharacteristicCode = 55;
			Factory.Save();

			AssertEquals("Code 29 is EEG classification", true, CMRTariffClassificationCharacteristic.IsTariffExciseEquivalentGoods(Factory, "24029129"));
			AssertEquals("Code 34 is EEG classification", true, CMRTariffClassificationCharacteristic.IsTariffExciseEquivalentGoods(Factory, "24029134"));
			AssertEquals("Code 35 is EEG classification", true, CMRTariffClassificationCharacteristic.IsTariffExciseEquivalentGoods(Factory, "24029135"));
			AssertEquals("Code 36 is EEG classification", true, CMRTariffClassificationCharacteristic.IsTariffExciseEquivalentGoods(Factory, "24029136"));
			AssertEquals("Code 55 is not EEG classification", false, CMRTariffClassificationCharacteristic.IsTariffExciseEquivalentGoods(Factory, "24029155"));
			AssertEquals("Code 66 is not EEG classification", false, CMRTariffClassificationCharacteristic.IsTariffExciseEquivalentGoods(Factory, "24029166"));
		}
	}
}
