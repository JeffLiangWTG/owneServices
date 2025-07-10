using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class EarlyReleaseMiscMessageSendingObjectLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSecurityTypeList()
		{
			AssertEquals("01, 02, 03, 04, 05, 06, 07, 08, 09, 10, 99", sendingObject.Lookups.SecurityTypeList.CodesAsString);
		}

		public void TestReasonForEarLyRemovalList()
		{
			AssertEquals("01, 02, 03, 04, 05", sendingObject.Lookups.ReasonForEarLyRemovalList.CodesAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			entry.Messages.AddNew().EM_MessageType = ElectronicDocumentTypeList.Codes._5BD;

			sendingObject = new EarlyReleaseMiscMessageSendingObject(entry);
		}
		EarlyReleaseMiscMessageSendingObject sendingObject;
	}
}
