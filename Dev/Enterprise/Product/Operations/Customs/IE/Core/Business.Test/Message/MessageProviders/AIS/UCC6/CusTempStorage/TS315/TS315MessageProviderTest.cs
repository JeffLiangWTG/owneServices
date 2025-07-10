using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage.Testing
{
	sealed class TS315MessageProviderTest : DataProviderTestCase<TS315MessageProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<System.ArgumentException>("Should throw ArgumentException with null header.", () => new TS315MessageProvider(null));
		}

		public void TestITS315Header()
		{
			Assert("Should implement ITS315Header", Provider is ITS315Header);
		}

		public void TestFallbackProcedure()
		{
			AssertType<FallbackProcedureProvider>("TS315MessageProvider.FallbackProcedure should return object of FallbackProcedure", Provider.FallbackProcedure);
		}

		public void TestDeclaration()
		{
			Assert("Should be IDeclaration07", Provider.Declaration is IDeclaration07);
		}

		protected override TS315MessageProvider GetProvider()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			return new TS315MessageProvider(new TemporaryStorageMessageSendingObject(header));
		}
	}
}
