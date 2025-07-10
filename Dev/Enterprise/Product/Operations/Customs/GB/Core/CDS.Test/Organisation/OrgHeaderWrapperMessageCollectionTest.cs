using CargoWise.EntityFramework;
using Enterprise.Customs.GB.CDS.Organisation;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(OrgHeaderWrapperMessageCollection))]
	sealed class OrgHeaderWrapperMessageCollectionTest : EDIMessageCollectionTest
	{
		public void TestLoadEDIMessages()
		{
			var organisation1 = Factory.New<OrgHeader>();
			var organisation1Wrapper = new OrgHeaderWrapper(organisation1);
			var message1 = Factory.New<CDSDISQueryMessage>();
			message1.EM_LinkedObject = organisation1;
			var message2 = Factory.New<CDSEDIMessage>();
			message2.EM_LinkedObject = organisation1;
			var message3 = Factory.New<CDSDISQueryMessage>();
			message3.EM_LinkedObject = organisation1;

			var organisation2 = Factory.New<OrgHeader>();
			var organisation2Wrapper = new OrgHeaderWrapper(organisation2);

			AssertEquals(2, organisation1Wrapper.Messages.Count);
			AssertEquals(message1, organisation1Wrapper.Messages[0]);
			AssertEquals(message3, organisation1Wrapper.Messages[1]);
			AssertEquals(0, organisation2Wrapper.Messages.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new OrgHeaderWrapperMessageCollection(Factory.New<OrgHeader>());
	}
}
