using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC525C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES.Testing
{
	class CC525CProviderTest : TestCaseWithFactory
	{
		public void TestMovementReferenceNumber()
		{
			AssertEquals("MRN", provider.MovementReferenceNumber);
		}

		public void TestReleaseDate()
		{
			AssertEquals(ZDateTime.BrettsBirthday, provider.ReleaseDate);
		}

		public void TestIsStored()
		{
			AssertEquals(ZBool.False, provider.IsStored);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC525CProvider(new Cc525C()
			{
				ExportOperation = new ExportOperationType13
				{
					Mrn = "MRN",
					ReleaseDate = ZDateTime.BrettsBirthday.ToDateTime(),
					StoringFlag = "0"
				}
			});
		}
		CC525CProvider provider;
	}
}
