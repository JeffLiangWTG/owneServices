using System;
using System.Collections.Generic;
using MailKit;
using Moq;

namespace Enterprise.MailManager.ExternalMailInterface.IMAP.Testing
{
	sealed class TestMailKitImapForMockMailFolder : MailKitImap
	{
		public TestMailKitImapForMockMailFolder(
			Mock<IMailFolder> mockInbox,
			MailServerConnectionConfiguration mailServerConfiguration,
			UserPasswordAuthConfiguration userPasswordAuthConfiguration,
			Action<string, Exception, string> reportErrorAction = null)
			: base(mailServerConfiguration, userPasswordAuthConfiguration, reportErrorAction)
		{
			mockInbox.Setup(m => m.IsOpen)
					 .Returns(true);
			dummyMailFolder = mockInbox.Object;
		}

		public TestMailKitImapForMockMailFolder(
			IMailFolder mailFolder,
			MailServerConnectionConfiguration mailServerConfiguration,
			UserPasswordAuthConfiguration userPasswordAuthConfiguration,
			Action<string, Exception, string> reportErrorAction = null)
			: base(mailServerConfiguration, userPasswordAuthConfiguration, reportErrorAction)
		{
			dummyMailFolder = mailFolder;
		}

		readonly IMailFolder dummyMailFolder;
		public override IMailFolder Inbox => dummyMailFolder;

		internal override bool FillAndCheckMessageIdsIfNeeded(string id)
		{
			messageIds.Add(id, new MessageSummary(0) { UniqueId = new UniqueId(888) });
			return true;
		}

		public override bool IsConnected => true;
		public override bool IsAuthenticated => true;

		public Action<IList<int>> batchDeleteAction;
		protected override void BatchDeleteAction(IList<int> numbers)
		{
			batchDeleteAction?.Invoke(numbers);
			base.BatchDeleteAction(numbers);
		}

		public Action<int> deleteAction;
		protected override void DeleteAction(int number)
		{
			deleteAction?.Invoke(number);
			base.DeleteAction(number);
		}
	}
}
