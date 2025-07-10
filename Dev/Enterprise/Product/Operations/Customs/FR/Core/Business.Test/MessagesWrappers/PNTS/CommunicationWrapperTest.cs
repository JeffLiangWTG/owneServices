using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	public class CommunicationWrapperTest : Customs.Business.Testing.DataProviderTestCase<CommunicationWrapper>
	{
		public void TestIdentifier()
		{
			AssertEquals("Identifier should equal OC_Email", "a@a.com", Provider.Identifier);
		}

		public void TestType()
		{
			AssertEquals("Type should equal EM", "EM", Provider.Type);
		}

		public void TestParametersWithEmailAndPhone()
		{
			var contact = Factory.New<OrgContact>();
			contact.OC_Email = "a@a.com";
			contact.OC_Phone = "12345678";
			var wrapper = CommunicationWrapper.New(contact);
			AssertEquals("Identifier should equal OC_Email", "a@a.com", wrapper.Identifier);
			AssertEquals("Type should equal EM", "EM", wrapper.Type);
		}

		public void TestParametersWithPhone()
		{
			var contact = Factory.New<OrgContact>();
			contact.OC_Phone = "12345678";
			var wrapper = CommunicationWrapper.New(contact);

			AssertEquals("Identifier should equal OC_Phone", "12345678", wrapper.Identifier);
			AssertEquals("Type should equal 'TE'", "TE", wrapper.Type);
		}

		public void TestParametersWithEmptyContact()
		{
			var contact = Factory.New<OrgContact>();
			var wrapper = CommunicationWrapper.New(contact);

			AssertEquals("Identifier should equal OC_Phone", string.Empty, wrapper.Identifier);
			AssertEquals("Type should equal string.Empty", string.Empty, wrapper.Type);
		}

		protected override CommunicationWrapper GetProvider()
		{
			var contact = Factory.New<OrgContact>();
			contact.OC_Email = "a@a.com";
			return CommunicationWrapper.New(contact);
		}
	}
}
