using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.UPE.ServiceTask
{
	class UPESaveConcurrencyExceptionResolverTest : TransactionedTestCase
	{
		public void TestConcurrencyHandling()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;

			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;

			var factory3 = new BusinessObjectFactory();
			factory3.RefreshEnabled = false;

			var factory1Mawb1 = factory1.New<UPECusMAWB>();
			factory1Mawb1.CM_MAWB = "123456";
			factory1.Save();

			var factory2Mawb1 = factory2.Load<UPECusMAWB>(factory1Mawb1.PK);
			factory2Mawb1.CM_MAWB = "654321";
			factory2Mawb1.CM_MessageReference = "reference";

			var factory2Mawb2 = factory2.New<UPECusMAWB>();
			factory2Mawb2.CM_MAWB = "222222";

			factory1Mawb1.CM_MAWB = "878787";
			factory1.Save();

			var buffer = new NotificationBuffer();
			AssertNoExceptionThrown(() => { UPESaveConcurrencyExceptionResolver.HandleException(() => { factory2.Save(); }, buffer); });

			Assert(buffer.AsString.Contains("CONCURRENCY: List of Concurrency objects:"));
			Assert(buffer.AsString.Contains(ZString.Format("BizObjectTableName: CusMAWB PK: {0}", factory2Mawb1.PK.ToString())));

			var factory3Mawb1Reloaded = factory3.Load<UPECusMAWB>(factory1Mawb1.PK);
			AssertEquals("MAWB", factory1Mawb1.CM_MAWB, factory3Mawb1Reloaded.CM_MAWB);
			AssertEquals("MessageReference", factory2Mawb1.CM_MessageReference, factory3Mawb1Reloaded.CM_MessageReference);

			var factory3Mawb2Reloaded = factory3.Load<UPECusMAWB>(factory2Mawb2.PK);
			AssertEquals("MAWB", factory2Mawb2.CM_MAWB, factory3Mawb2Reloaded.CM_MAWB);
		}
	}
}
