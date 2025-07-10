using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRInstrumentCharacteristic))]
	sealed class CMRInstrumentCharacteristicTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsInstrumentCharacteristic()
		{
			var instrumentCharacteristic1 = Factory.New<CMRInstrumentCharacteristic>();
			instrumentCharacteristic1.IK_InstrumentNumber = "111111";
			instrumentCharacteristic1.IK_CharacteristicCode = 1;
			var instrumentCharacteristic2 = Factory.New<CMRInstrumentCharacteristic>();
			instrumentCharacteristic2.IK_InstrumentNumber = "222222";
			instrumentCharacteristic2.IK_CharacteristicCode = 2;
			Assert("no exception on empty", !CMRInstrumentCharacteristic.IsInstrumentCharacteristic(string.Empty, 1, Factory));
			Assert("no exception on invalid number", !CMRInstrumentCharacteristic.IsInstrumentCharacteristic("333333", 1, Factory));
			Assert("is charcateristic", CMRInstrumentCharacteristic.IsInstrumentCharacteristic("111111", 1, Factory));
			Assert("is charcateristic", CMRInstrumentCharacteristic.IsInstrumentCharacteristic("222222", 2, Factory));
			Assert("is NOT charcateristic", !CMRInstrumentCharacteristic.IsInstrumentCharacteristic("111111", 2, Factory));
			Assert("is NOT charcateristic", !CMRInstrumentCharacteristic.IsInstrumentCharacteristic("222222", 3, Factory));
		}

		protected override BusinessObject GetNewBusinessObject() => CMRInstrumentCharacteristic.New(Factory);
	}
}
