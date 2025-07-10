using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage.Testing
{
	sealed class TS314MessageProviderTest : DataProviderTestCase<TS314MessageProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<System.ArgumentException>("Should throw ArgumentException with null header.", () => new TS314MessageProvider(null));
		}

		public void TestITS314Header()
		{
			Assert("Should implement ITS314Header", Provider is ITS314Header);
		}

		public void TestDeclaration()
		{
			Assert("Should be IDeclaration14", Provider.Declaration is IDeclaration14);
		}

		protected override TS314MessageProvider GetProvider() => new TS314MessageProvider(new TemporaryStorageMessageSendingObject(header));

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		}

		TemporaryStorageHeader header;
	}
}
