using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaContainer))]
	sealed class AsycudaContainerTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var cont = header.Containers.AddNew();
			AssertEquals(header, cont.Header);
		}

		public void TestDefaults()
		{
			var cont = Factory.New<AsycudaContainer>();
			AssertEquals(Core.Constants.Weight.Kilograms, cont.ACN_GoodsWeightUQ);
		}

		public void TestWeightInKilos()
		{
			var cont = Factory.New<AsycudaContainer>();
			cont.ACN_GoodsWeightUQ = Core.Constants.Weight.Pounds;
			cont.ACN_GoodsWeight = 2.205m;
			AssertEquals(1.000171m, cont.ACN_GoodsWeightInKilos);
			cont.ACN_GoodsWeight = 22.046m;
			AssertEquals(9.999897m, cont.ACN_GoodsWeightInKilos);
			cont.ACN_GoodsWeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals(22.046m, cont.ACN_GoodsWeightInKilos);
			cont.ACN_GoodsWeightUQ = "20";
			AssertNoExceptionThrown(() => { var test = cont.ACN_GoodsWeightInKilos; });
			AssertEquals(0.0m, cont.ACN_GoodsWeightInKilos);
		}

		public void TestDelete()
		{
			var header1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill1 = header1.Bills.AddNew();
			var cont1 = header1.Containers.AddNew();
			var pack1 = bill1.Packs.AddNew();
			cont1.Delete();
			Factory.Save();

			var header2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header2.AMA_JobReference = "2222";
			var bill2 = header2.Bills.AddNew();
			var cont2 = header2.Containers.AddNew();
			var pack2 = bill2.Packs.AddNew();
			pack2.ContainerPK = cont2.PK;
			cont2.Delete();
			Factory.Save();
			AssertNull(pack2.Pivot);
		}

		public void TestHumanReadableName()
		{
			var cont = Factory.New<AsycudaContainer>();
			cont.ACN_ContainerNumber = "CONT001";
			AssertEquals("Manifest Container CONT001", cont.HumanReadableName);
		}

		public void TestListAttributes()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(AsycudaContainer), nameof(AsycudaContainer.ACN_CommodityCode), false, x => x.ListDataSourceMember == "Lookups.CommodityCodes");
			AssertHasCustomAttribute<ListAttribute>(typeof(AsycudaContainer), nameof(AsycudaContainer.ACN_GoodsWeightUQ), false, x => x.ListDataSourceMember == "Lookups.WeightCodes");
			AssertHasCustomAttribute<ListAttribute>(typeof(AsycudaContainer), nameof(AsycudaContainer.ACN_SealType1), false, x => x.ListDataSourceMember == "Lookups.SealTypeList");
			AssertHasCustomAttribute<ListAttribute>(typeof(AsycudaContainer), nameof(AsycudaContainer.ACN_SealType2), false, x => x.ListDataSourceMember == "Lookups.SealTypeList");
			AssertHasCustomAttribute<ListAttribute>(typeof(AsycudaContainer), nameof(AsycudaContainer.ACN_SealType3), false, x => x.ListDataSourceMember == "Lookups.SealTypeList");
			AssertHasCustomAttribute<ListAttribute>(typeof(AsycudaContainer), nameof(AsycudaContainer.ACN_SealingPartyType), false, x => x.ListDataSourceMember == "Lookups.SealingPartyList");
			AssertHasCustomAttribute<ListAttribute>(typeof(AsycudaContainer), nameof(AsycudaContainer.ACN_SealingPartyType2), false, x => x.ListDataSourceMember == "Lookups.SealingPartyList");
			AssertHasCustomAttribute<ListAttribute>(typeof(AsycudaContainer), nameof(AsycudaContainer.ACN_SealingPartyType3), false, x => x.ListDataSourceMember == "Lookups.SealingPartyList");
			AssertHasCustomAttribute<ListAttribute>(typeof(AsycudaContainer), nameof(AsycudaContainer.ACN_EmptyFullIndicator), false, x => x.ListDataSourceMember == "Lookups.EmptyFullList");
			AssertHasCustomAttribute<ListAttribute>(typeof(AsycudaContainer), nameof(AsycudaContainer.ACN_Seal1UnloadingState), false, x => x.ListDataSourceMember == "Lookups.UnloadingStatesList");
			AssertHasCustomAttribute<ListAttribute>(typeof(AsycudaContainer), nameof(AsycudaContainer.ACN_Seal2UnloadingState), false, x => x.ListDataSourceMember == "Lookups.UnloadingStatesList");
			AssertHasCustomAttribute<ListAttribute>(typeof(AsycudaContainer), nameof(AsycudaContainer.ACN_Seal3UnloadingState), false, x => x.ListDataSourceMember == "Lookups.UnloadingStatesList");
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			return header.Containers.AddNew();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
	}
}
