using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(EarlyReleaseMiscMessageSendingObject))]
	sealed class EarlyReleaseMiscMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			entry.Messages.AddNew().EM_MessageType = ElectronicDocumentTypeList.Codes._5BD;
			return new EarlyReleaseMiscMessageSendingObject(entry);
		}

		public void TestMaxLength()
		{
			var entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			entry.Messages.AddNew().EM_MessageType = ElectronicDocumentTypeList.Codes._5BD;

			var messageSendingObject = new EarlyReleaseMiscMessageSendingObject(entry);

			AssertEquals(50, messageSendingObject.AmendmentReasonInfo.MaxLength);
			AssertEquals(2, messageSendingObject.SecurityTypeInfo.MaxLength);
			AssertEquals(20, messageSendingObject.OtherSecurityTypeInfo.MaxLength);
			AssertEquals(2, messageSendingObject.ReasonForEarlyRemovalInfo.MaxLength);
		}

		public void TestGetValidation()
		{
			var entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			entry.Messages.AddNew().EM_MessageType = ElectronicDocumentTypeList.Codes._5BD;

			var messageSendingObject = new EarlyReleaseMiscMessageSendingObject(entry);

			AssertEquals(typeof(EarlyReleaseMiscMessageSendingObjectValidation), messageSendingObject.Validation.GetType());
		}

		public void TestLookups()
		{
			var entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			entry.Messages.AddNew().EM_MessageType = ElectronicDocumentTypeList.Codes._5BD;

			var messageSendingObject = new EarlyReleaseMiscMessageSendingObject(entry);

			AssertEquals(typeof(EarlyReleaseMiscMessageSendingObjectLookups), messageSendingObject.Lookups.GetType());
		}
	}
}
