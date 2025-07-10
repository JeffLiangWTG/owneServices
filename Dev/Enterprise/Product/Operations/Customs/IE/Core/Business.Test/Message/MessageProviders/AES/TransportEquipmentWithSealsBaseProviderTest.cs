using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	public class TransportEquipmentWithSealsBaseProviderTest : Customs.Business.Testing.DataProviderTestCase<TransportEquipmentWithSealsBaseProvider>
	{
		public void TestContainerIdentificationNumber()
		{
			containerNumber = "CONT12345";
			AssertEquals("ContainerIdentificationNumber", "CONT12345", Provider.ContainerIdentificationNumber);
		}

		public void TestSeals()
		{
			CombineAssertions(() =>
			{
				seals = new[] { "SL1", "SL2" };
				var provider = GetProvider();
				var sealValues = provider.Seals.ToArray();
				AssertEquals("sealValues.Count", 2, sealValues.Length);
				AssertEquals("sealValues[0]", "SL1", sealValues[0]);

				seals = null;
				provider = GetProvider();
				AssertEquals("Empty Seals", 0, provider.Seals.Count);
			});
		}

		public void TestGoodsReferences()
		{
			CombineAssertions(() =>
			{
				goodsReferences = new[] { "1", "2" };
				var provider = GetProvider();
				var goodsReferenceValues = provider.GoodsReferences.ToArray();
				AssertEquals("goodsReferenceValues.Count", 2, goodsReferenceValues.Length);
				AssertEquals("goodsReferenceValues[0]", "1", goodsReferenceValues[0]);
				AssertEquals("goodsReferenceValues[1]", "2", goodsReferenceValues[1]);

				goodsReferences = null;
				provider = GetProvider();
				AssertEquals("Empty GoodsReferences", 0, provider.GoodsReferences.Count);
			});
		}

		public void TestContainerIsFull()
		{
			AssertEquals("Container full", false, Provider.ContainerIsFull);
		}

		protected override TransportEquipmentWithSealsBaseProvider GetProvider() => new TransportEquipmentWithSealsBaseProvider
		{
			ContainerIdentificationNumber = containerNumber,
			Seals = seals,
			GoodsReferences = goodsReferences
		};

		string containerNumber;
		IReadOnlyCollection<string> seals;
		IReadOnlyCollection<string> goodsReferences;
	}
}
