using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class SetterSuspenderTest : TestCase
	{
		public void TestIsSetterSuspended()
		{
			var suspender = new SetterSuspender();
			Assert("Should default to false as there is no property is marked as Suspended.", !suspender.IsSetterSuspended("AAA"));

			using (suspender.SuspendSetting("AAA"))
			{
				Assert("Should be true as AAA is marked as locked.", suspender.IsSetterSuspended("AAA"));
				using (suspender.ResumeSetting("AAA"))
				{
					Assert("Should be false as AAA is marked as unlocked.", !suspender.IsSetterSuspended("AAA"));
				}
				Assert("Should be false as BBB is marked as unlocked.", !suspender.IsSetterSuspended("BBB"));
			}

			Assert("Should be false as there is no property is marked as locked.", !suspender.IsSetterSuspended("AAA"));
		}

		public void TestLockOrUnlockSetter()
		{
			var suspender = new SetterSuspender();
			Assert("Should default to false as there is no property is marked as locked.", !suspender.IsSetterSuspended("AAA"));

			using (suspender.SuspendSetting("AAA"))
			{
				Assert("Should be true as AAA is marked as locked.", suspender.IsSetterSuspended("AAA"));

				using (suspender.SuspendSetting("AAA"))
				{
					Assert("Should be true as AAA is marked as locked again.", suspender.IsSetterSuspended("AAA"));

					using (suspender.ResumeSetting("AAA"))
					{
						Assert("Should be false as AAA is marked as unlocked.", !suspender.IsSetterSuspended("AAA"));
					}
				}
			}

			Assert("Should be false as there is no property is marked as locked.", !suspender.IsSetterSuspended("AAA"));
		}

		public void TestIsEnforceSetterSuspended()
		{
			var suspender = new SetterSuspender();
			Assert("Should default to false as there is no property is marked as Suspended.", !suspender.IsSetterSuspended("AAA"));

			using (suspender.SuspendEnforceSetting("AAA"))
			{
				Assert("Should be true as AAA is marked as locked.", suspender.IsSetterSuspended("AAA"));
				using (suspender.ResumeSetting("AAA"))
				{
					Assert("Should be true as SuspendEnforceSetting cannot be unlocked by resumeSetting", suspender.IsSetterSuspended("AAA"));
				}
			}
			Assert("Should be false as there is no property is marked as locked.", !suspender.IsSetterSuspended("AAA"));
		}
	}
}
