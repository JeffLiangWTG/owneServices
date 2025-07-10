using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZSaveConcurrencyExceptionTest : TransactionedTestCase
	{
		public void TestAdditionalInfo()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory { RefreshEnabled = false };

			DummyBusinessObject dummy = factory1.New<DummyBusinessObject>();

			dummy.Z0_Code = "ABC";
			dummy.Z0_Description = "qwerty";

			ConcurrencyInfo.SetConcurrencyPolicy(dummy.Row, "Z0_Code", ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(dummy.Row, "Z0_Description", ConcurrencyPolicy.Strict);

			factory1.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			DummyBusinessObject dummyInOtherFactory = factory2.Load<DummyBusinessObject>(dummy.PK);

			dummyInOtherFactory.Z0_Code = "XXX";
			dummyInOtherFactory.Z0_Description = "zyxwvu";

			factory2.Save();

			dummy.Z0_Code = "123";
			dummy.Z0_Description = "abcdef";

			try
			{
				factory1.Save();

				Fail("ZSaveConcurrencyException was expected to be thrown here.");
			}
			catch (ZSaveConcurrencyException ex)
			{
				Assert("ZSaveConcurrencyException should have additional information about concurrency error.", ex.Message.Contains("Additional Information"));
			}
		}
	}
}
