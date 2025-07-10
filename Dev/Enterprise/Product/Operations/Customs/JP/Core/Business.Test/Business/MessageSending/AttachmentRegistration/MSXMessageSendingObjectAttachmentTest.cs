using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(MSXMessageSendingObjectAttachment))]
	sealed class MSXMessageSendingObjectAttachmentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFile()
		{
			var declaration = Factory.New<JobDeclaration>();
			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var header = declaration.CustomsEntryHeaders.AddNew();
			var msxMessageSendingObject = new MSXMessageSendingObject(header);
			var attachment = msxMessageSendingObject.Attachments.AddNew();
			attachment.File = eDoc.UniqueKey;
			AssertEquals(eDoc, attachment.Document);
			AssertEquals("Invoice.pdf", attachment.Document.FileName);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<CusEntryHeader>();
			var parent = new MSXMessageSendingObject(header);
			return new MSXMessageSendingObjectAttachment(parent);
		}
	}
}
