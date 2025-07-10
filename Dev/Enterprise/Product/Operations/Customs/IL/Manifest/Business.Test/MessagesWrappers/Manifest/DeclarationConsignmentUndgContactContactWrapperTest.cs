using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Manifest.Business.MessagesWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationConsignmentUndgContactContactWrapperTest : DataProviderTestCase<IDeclarationConsignmentUNDangerousGoodsContactContact>
	{
		public void TestName()
		{
			AssertEquals("Name must be equal to the expected value", "John Doe", Provider.Name.Value);
		}

		public void TestCommunication()
		{
			AssertNotNull("Communication", Provider.Communication);
			AssertEquals("Communication count", 2, Provider.Communication.Count);
			AssertEquals("First Communication ID", "1234567890", Provider.Communication.First().Id.Value);
			AssertEquals("First Communication TypeID", "TE", Provider.Communication.First().TypeId.Value);
			AssertEquals("Second Communication ID", "email@acme.com", Provider.Communication.Last().Id.Value);
			AssertEquals("Second Communication TypeID", "EM", Provider.Communication.Last().TypeId.Value);
		}

		public void TestNewOrNull()
		{
			AssertNull("Must be null", DeclarationConsignmentUNDGContactContactWrapper.NewOrNull(null));
			AssertNotNull("Must be not null", DeclarationConsignmentUNDGContactContactWrapper.NewOrNull(Factory.New<OrgContact>()));
		}

		protected override IDeclarationConsignmentUNDangerousGoodsContactContact GetProvider()
		{
			var dgContact = Factory.New<OrgContact>();
			dgContact.OC_ContactName = "John Doe";
			dgContact.OC_Phone = "1234567890";
			dgContact.OC_Email = "email@acme.com";

			return DeclarationConsignmentUNDGContactContactWrapper.NewOrNull(dgContact);
		}
	}
}
