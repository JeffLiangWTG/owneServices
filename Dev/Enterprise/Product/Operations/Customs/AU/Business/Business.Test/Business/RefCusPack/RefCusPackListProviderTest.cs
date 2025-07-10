using System;
using Enterprise.MasterFiles.Business.Customs;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class RefCusPackListProviderTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestGetPackConversionTypeList()
		{
			var provider = new RefCusPackListProvider();

			using (AUCustomsDataRegistry.Instance.EnableNewPackTypeConversions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var list = provider.GetPackConversionTypeList(Factory);
				AssertEquals(", CIP, DTP, AMS, AFR, GMB, GMP, PKD", list.CodesAsString);
			}

			Factory.ClearCachedValue<RPTypeList>("AU.RefCusPackListProvider.GetPackConversionTypeList");
			using (AUCustomsDataRegistry.Instance.EnableNewPackTypeConversions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var list = provider.GetPackConversionTypeList(Factory);
				AssertEquals(", CIP, DTP, AMS, AFR, GMB, GMP, PKD, SCR, ACR", list.CodesAsString);
			}
		}

		public void TestLoaderAU()
		{
			var provider = MasterFiles.Business.RefCusPackListProvider.Loader.GetRefCusPackListProvider(Factory, Core.Constants.CountryCodes.Australia);
			AssertType<RefCusPackListProvider>(provider);
		}
	}
}
