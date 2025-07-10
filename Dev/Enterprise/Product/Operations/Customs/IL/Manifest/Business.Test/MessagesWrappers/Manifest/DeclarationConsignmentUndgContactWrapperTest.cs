using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Manifest.Business.MessagesWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationConsignmentUndgContactWrapperTest : DataProviderTestCase<IDeclarationConsignmentUNDangerousGoodsContact>
	{
		public void TestName()
		{
			AssertEquals("Name must be equal to the expected value", "ACME Inc.", Provider.Name.Value);
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

		public void TestContact()
		{
			AssertNotNull("Contact", Provider.Contact);
			AssertEquals("Contact count", 1, Provider.Contact.Count);
		}

		public void TestNewOrNull()
		{
			AssertNull("Must be null", DeclarationConsignmentUndgContactWrapper.NewOrNull(null));
			AssertNotNull("Must be not null", DeclarationConsignmentUndgContactWrapper.NewOrNull(Factory.New<OrgContact>()));
		}

		protected override IDeclarationConsignmentUNDangerousGoodsContact GetProvider()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "ACME Inc.";
			var dgContact = Factory.New<OrgContact>();
			dgContact.OC_OH = orgHeader.PK;

			var mainAddress = orgHeader.MainAddress;
			mainAddress.OA_Phone = "1234567890";
			mainAddress.OA_Email = "email@acme.com";

			return DeclarationConsignmentUndgContactWrapper.NewOrNull(dgContact);
		}
	}
}
