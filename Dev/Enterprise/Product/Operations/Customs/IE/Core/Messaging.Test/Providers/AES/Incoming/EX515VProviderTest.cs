using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX515V;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.AES.Testing
{
	class EX515VProviderTest : TestCaseWithFactory
	{
		public void TestLocalReferenceNumber()
		{
			AssertEquals("LRN", provider.LocalReferenceNumber);
		}

		public void TestMovementReferenceNumber()
		{
			AssertEquals("MRN", provider.MovementReferenceNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new EX515VProvider(new Ex515V
			{
				ExportOperation = new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_ctypes.DeclarationType7
				{
					Lrn = "LRN",
					Mrn = "MRN"
				}
			});
		}
		EX515VProvider provider;
	}
}
