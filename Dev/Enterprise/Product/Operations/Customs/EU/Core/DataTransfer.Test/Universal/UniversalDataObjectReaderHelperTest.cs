using CargoWise.Types;

namespace Enterprise.Customs.EU.DataTransfer.Universal.Testing
{
	class UniversalDataObjectReaderHelperTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestDv1DetailsLinkDictionary()
		{
			var testHelper = new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.CountryCodes.UnitedKingdom);
			var id1 = ZGuid.NewZGuid();
			var id2 = ZGuid.NewZGuid();
			var id3 = ZGuid.NewZGuid();
			testHelper.RegisterDv1DetailsPK(1, id1);
			testHelper.RegisterDv1DetailsPK(2, id2);
			testHelper.RegisterDv1DetailsPK(2, id3);

			CombineAssertions(() =>
			{
				AssertEquals(null, testHelper.GetDv1DetailsPK(null));
				AssertEquals(null, testHelper.GetDv1DetailsPK(0));
				AssertEquals(id1, testHelper.GetDv1DetailsPK(1));
				AssertEquals(id3, testHelper.GetDv1DetailsPK(2));
				AssertEquals(null, testHelper.GetDv1DetailsPK(3));
			});
		}
	}
}
