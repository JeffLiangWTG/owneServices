using System.ComponentModel;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MailManager.Business
{
	[TestedType(typeof(MailAttachmentCollection))]
	sealed class MailAttachmentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSortHumanReadableAttachmentSize()
		{
			var size1000KB = Encoding.Default.GetBytes(new string('A', 1024 * 1000));
			var size200KB = Encoding.Default.GetBytes(new string('A', 1024 * 200));
			var size30KB = Encoding.Default.GetBytes(new string('A', 1024 * 30));

			Attachment1.MA_Data = size1000KB;
			Attachment2.MA_Data = size200KB;
			Attachment3.MA_Data = size30KB;

			AssertEquals(3, MyItem.MailAttachments.Count);

			MyItem.MailAttachments.Sort("HumanReadableAttachmentSize", ListSortDirection.Ascending);
			AssertEquals(size30KB.Length, MyItem.MailAttachments[0].MA_Data.Length);
			AssertEquals(size200KB.Length, MyItem.MailAttachments[1].MA_Data.Length);
			AssertEquals(size1000KB.Length, MyItem.MailAttachments[2].MA_Data.Length);

			MyItem.MailAttachments.Sort("HumanReadableAttachmentSize", ListSortDirection.Descending);
			AssertEquals(size1000KB.Length, MyItem.MailAttachments[0].MA_Data.Length);
			AssertEquals(size200KB.Length, MyItem.MailAttachments[1].MA_Data.Length);
			AssertEquals(size30KB.Length, MyItem.MailAttachments[2].MA_Data.Length);
		}

		public void TestMailAttachmentCollection()
		{
			AssertNotNull("Collection", new MailAttachmentCollection(MyItem, MyItem.Factory));
		}

		public void TestGetMailAttachment()
		{
			MyCollection.Add(Attachment1);
			AssertEquals("MyCollection[0]", Attachment1, MyCollection[0]);
		}

		public void TestSetMailAttachment()
		{
			TestGetMailAttachment();
		}

		#region Implemementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			MyItem = Factory.New<MailItem>();
			return MyItem.MailAttachments;
		}

		protected override void SetUp()
		{
			base.SetUp();
			MyItem = Factory.New<MailItem>();
			Attachment1 = Factory.New<MailAttachment>();
			Attachment2 = Factory.New<MailAttachment>();
			Attachment3 = Factory.New<MailAttachment>();

			Attachment1.MA_MI = MyItem.PK;
			Attachment2.MA_MI = MyItem.PK;
			Attachment3.MA_MI = MyItem.PK;

			MyCollection = MyItem.MailAttachments;
		}

		MailAttachmentCollection MyCollection;
		MailItem MyItem;
		MailAttachment Attachment1;
		MailAttachment Attachment2;
		MailAttachment Attachment3;
		#endregion
	}
}
