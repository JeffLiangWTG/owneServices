using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business.Customs;
using ECC = Enterprise.Core.Constants;

namespace Enterprise.Customs.BR.Business.Testing
{
	sealed class RefCusPackListProviderTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestGetCustomsPackListBR()
		{
			CreateRefCusCodeList();
			var expectedListIsCUSUQ = new string[] { "M3", "PARES", "M2" };
			var expectedListIsPKG = new string[] { "01", "02", "03" };

			var provider = new RefCusPackListProvider();
			var list = provider.GetCustomsPackList(Factory, RPTypeList.Codes.AMSManifest, ECC.CountryCodes.Brazil);
			AssertCollectionNotContains(expectedListIsCUSUQ, list.GetAllCodes());

			list = provider.GetCustomsPackList(Factory, RPTypeList.Codes.CommercialInvoice, ECC.CountryCodes.Brazil);
			AssertContainsExactElementsInAnyOrder(expectedListIsCUSUQ, list.GetAllCodes());

			list = provider.GetCustomsPackList(Factory, ZString.Empty, ECC.CountryCodes.Brazil);
			AssertContainsExactElementsInAnyOrder(expectedListIsPKG, list.GetAllCodes());
		}

		public void TestLoaderBR()
		{
			var provider = MasterFiles.Business.RefCusPackListProvider.Loader.GetRefCusPackListProvider(Factory, ECC.CountryCodes.Brazil);
			AssertType<RefCusPackListProvider>("Provider should be Enterprise.Customs.BR.Business.RefCusPackListProvider", provider);
		}

		void CreateRefCusCodeList()
		{
			var cusuqCodes = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("M3", "Cubic meter"),
				new KeyValuePair<string, string>("PARES", "Pairs"),
				new KeyValuePair<string, string>("M2", "Square metres")
			};

			ReferenceTestDataHelper.CreateRefCusCodeList(Factory, new KeyValuePair<string, string>(ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs UQ"), cusuqCodes);

			var pkgCodes = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("01", "AMARRADO/ATADO/FEIXE"),
				new KeyValuePair<string, string>("02", "BARRICA DE FERRO"),
				new KeyValuePair<string, string>("03", "BARRICA DE FIBRA DE VIDRO")
			};

			ReferenceTestDataHelper.CreateRefCusCodeList(Factory, new KeyValuePair<string, string>(ECC.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "Package"), pkgCodes);
		}

		public void TestGetDeclarationPackTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", "ASYCO", "PT1", "PT1 Desc", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList("TW", "TWCIU", "PT2", "PT2 Desc", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList("UNE", "UNPKG", "PT3", "PT3 Desc", new ZDateTime(1900, 1, 1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList("BR", "PKG", "PT4", "PT4 Desc", new ZDateTime(2000, 1, 1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var provider = new RefCusPackListProvider();
			var list = provider.GetDeclarationPackTypeList(Factory);

			AssertContainsExactElementsInAnyOrder(new[] { "PT3", "PT4" }, list.GetAllCodes());
		}
	}
}
