using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.CustomerService.Business.Testing
{
	[TestedType(typeof(IncidentApprovalAttachmentCollection))]
	sealed class IncidentApprovalAttachmentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<IncidentApprovalAttachmentCollection>
	{
		public void TestLoad()
		{
			var incident = Factory.New<IncidentApproval>();
			incident.DocManagerInfo.AddFileOrDocument(new byte[] { 1, 1, 1, 1 }, "Doc A", "INT");

			var attachmentCollection = new IncidentApprovalAttachmentCollection(incident);
			attachmentCollection.Load();
			AssertEquals(1, attachmentCollection.Count);

			incident.DocManagerInfo.AddFileOrDocument(new byte[] { 1, 1, 1, 1 }, "Doc B", "INT");
			attachmentCollection.Load();
			AssertEquals(2, attachmentCollection.Count);

			((BusinessObject)incident.DocManagerInfo.AllEDocs[0]).Delete();
			attachmentCollection.Load();
			AssertEquals(1, attachmentCollection.Count);
		}

		protected override IncidentApprovalAttachmentCollection GetCollectionToTest()
		{
			var incident = Factory.New<IncidentApproval>();
			return new IncidentApprovalAttachmentCollection(incident);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new IncidentApprovalAttachment();
		}
	}
}
