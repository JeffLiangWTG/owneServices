using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Manifest.Business.MessagesWrappers;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationConsignmentUNDangerousGoodsContactContactCommunicationWrapperTest : DataProviderTestCase<IDeclarationConsignmentUNDangerousGoodsContactContactCommunication>
	{
		public void TestId()
		{
			AssertEquals("ID1", Provider.Id.Value);
		}

		public void TestTypeId()
		{
			AssertEquals("TYPEID1", Provider.TypeId.Value);
		}

		public void TestNewOrNull()
		{
			AssertNull("Must be null when ID is null", DeclarationConsignmentUNDGContactContactCommunicationWrapper.NewOrNull(null, "TYPEID1"));
			AssertNotNull("Must be not null when ID is not null", DeclarationConsignmentUNDGContactContactCommunicationWrapper.NewOrNull("ID1", "TYPEID1"));
		}

		protected override IDeclarationConsignmentUNDangerousGoodsContactContactCommunication GetProvider()
		{
			return DeclarationConsignmentUNDGContactContactCommunicationWrapper.NewOrNull("ID1", "TYPEID1");
		}
	}
}
