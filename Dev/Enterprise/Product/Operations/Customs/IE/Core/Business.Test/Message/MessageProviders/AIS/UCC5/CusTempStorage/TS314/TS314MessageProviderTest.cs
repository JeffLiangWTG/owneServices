using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage.Testing
{
	sealed class TS314MessageProviderTest : DataProviderTestCase<TS314MessageProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<System.ArgumentException>("Should throw ArgumentException with null message sending object.", () => new TS314MessageProvider(null));
				AssertExceptionThrown<System.ArgumentException>("Should throw ArgumentException with null header.", () => new TS314MessageProvider(new TemporaryStorageMessageSendingObject(null)));
				AssertNoExceptionThrown(() => GetProvider());
			});
		}

		public void TestITS314Header()
		{
			Assert("Should implement ITS314Header", Provider is ITS314Header);
		}

		public void TestDeclaration()
		{
			Assert("Should implement ITS314DeclarationType", Provider.Declaration is ITS314DeclarationType);
			AssertType<TS314DeclarationTypeProvider>(Provider.Declaration);
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
