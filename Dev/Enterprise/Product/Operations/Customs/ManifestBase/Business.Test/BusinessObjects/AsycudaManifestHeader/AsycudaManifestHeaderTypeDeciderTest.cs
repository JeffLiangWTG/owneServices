using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ManifestBase.Testing
{
	class AsycudaManifestHeaderTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoadZAOutturn()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "123";
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ZAOutturnAndGateInOrOut;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
			Factory.Save();
			Assert(new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK) is Integration.Customs.ZA.IAsycudaManifestHeader);
		}

		public void TestGetTypeForLoadAsycuda()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "456";
			header.AMA_ApplicationCode = "NVC";
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			Factory.Save();
			Assert(new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK) is Integration.Customs.ASYCUDA.IAsycudaManifestHeader);
		}

		public void TestGetTypeForLoadBase()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "789";
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Constants.ManifestBase;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			Factory.Save();
			AssertType<AsycudaManifestHeader>(new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK));
		}

		public void TestGetTypeForLoadTemporaryStorage()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "123";
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.TemporaryStorage;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			Factory.Save();
			Assert(new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK) is Integration.Customs.EU.ITemporaryStorageHeader);
		}

		public void TestGetTypeForLoadCountrySpecificTemporaryStorage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var header = Factory.New<AsycudaManifestHeader>();
				header.AMA_JobReference = "123";
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.TemporaryStorage;
				header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Ireland;
				Factory.Save();
				Assert(new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK) is Integration.Customs.IE.ITemporaryStorageHeader);
			}
		}

		public void TestGetTypeForLoadEuH7()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "123";
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.EuH7;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			Factory.Save();
			Assert(new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK) is Integration.Customs.ASYCUDA.EUH7.IAsycudaManifestHeader);
		}

		public void TestGetTypeForLoadCountrySpecificEuH7()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var header = Factory.New<AsycudaManifestHeader>();
				header.AMA_JobReference = "123";
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.EuH7;
				header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Ireland;
				Factory.Save();
				Assert(new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK) is Integration.Customs.IEH7.IAsycudaManifestHeader);
			}
		}
		public void TestGetTypeForNew()
		{
			AssertEquals(typeof(AsycudaManifestHeader), new AsycudaManifestHeaderTypeDecider().GetTypeForNew());
		}

		public void TestGetTypeForBinding()
		{
			AssertEquals(typeof(AsycudaManifestHeader), new AsycudaManifestHeaderTypeDecider().GetTypeForBinding());
		}
	}
}
