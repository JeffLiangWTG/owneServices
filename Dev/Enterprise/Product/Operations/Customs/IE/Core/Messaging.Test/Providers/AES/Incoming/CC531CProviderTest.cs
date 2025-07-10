using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES.Testing
{
	class CC531CProviderTest : TestCaseWithFactory
	{
		CC531Provider provider;
		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC531Provider(new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC531C.Cc531C
			{
				TimerExpiryForSupplementaryDeclaration = new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.ctypes.TimerExpiryForSupplementaryDeclarationType
				{
					LodgementOfSupplementaryDeclarationExpiryDate = ZDateTime.BrettsBirthday.ToDateTime().AddDays(1),
					LodgementOfSupplementaryDeclarationStartDate = ZDateTime.BrettsBirthday.ToDateTime(),
					TimerExpiryInformation = "Timer Expire Information Test Data"
				},
				ExportOperation = new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.ctypes.ExportOperationType59
				{
					Mrn = "TEST_MRN"
				}
			});
		}

		public void TestMrn()
		{
			AssertEquals("TEST_MRN", provider.MovementReferenceNumber);
		}

		public void TestTimerStartDate()
		{
			AssertEquals(ZDateTime.BrettsBirthday, provider.SupplementaryDeclarationLodgementStart);
		}

		public void TestTimerExpiryDate()
		{
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(1), provider.SupplementaryDeclarationLodgementEnd);
		}

		public void TestTimerInformation()
		{
			AssertEquals("Timer Expire Information Test Data", provider.TimerExpiryInformation);
		}
	}
}
