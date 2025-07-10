using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class TransportEquipmentWrapperTest : Customs.Business.Testing.DataProviderTestCase<TransportEquipmentWrapper>
	{
		protected override TransportEquipmentWrapper GetProvider()
		{
			var wrapper = TransportEquipmentWrapper.New("E1", new List<string>() { "1", "2" });
			return wrapper;
		}

		public void TestContainerIdentificationNumber()
		{
			AssertEquals("ContainerIdentificationNumber should be equal to value entered in first parameter.", "E1", GetProvider().ContainerIdentificationNumber);
		}

		public void TestGoodsReference()
		{
			CombineAssertions(() =>
			{
				AssertEquals("There should be 2 GoodsReference.", 2, Provider.GoodsReference.Count);
				AssertEquals("DeclarationGoodsItemNumber should be equal to value entered in second parameter first value.", "1", GetProvider().GoodsReference.ElementAt(0).DeclarationGoodsItemNumber);
				AssertEquals("DeclarationGoodsItemNumber should be equal to value entered in second parameter second value.", "2", GetProvider().GoodsReference.ElementAt(1).DeclarationGoodsItemNumber);
			});
		}
	}
}
