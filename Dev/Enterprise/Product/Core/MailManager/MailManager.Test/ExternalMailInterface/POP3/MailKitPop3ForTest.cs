using System;
using System.Collections.Generic;
using System.Threading;

namespace Enterprise.MailManager.ExternalMailInterface.POP3.Testing
{
	sealed class MailKitPop3ForTest : MailKitPop3
	{
		public MailKitPop3ForTest(
			MailServerConnectionConfiguration mailServerConfiguration,
			UserPasswordAuthConfiguration userPasswordAuthConfiguration,
			Action<string, Exception, string> reportErrorAction = null)
				: base(mailServerConfiguration, userPasswordAuthConfiguration, reportErrorAction)
		{
		}

		public override int Count
		{
			get
			{
				return Messages.Count;
			}
		}

		public List<string> Messages { get; set; } = new List<string>();

		public void FillMessagesForTest()
		{
			for (int i = 0; i < 5; i++)
			{
				Messages.Add(Guid.NewGuid().ToString());
			}
		}

		public override void DeleteMessage(int index, CancellationToken cancellationToken = default)
		{
			Messages.RemoveAt(index);
		}

		public override string GetMessageUid(int index, CancellationToken cancellationToken = default)
		{
			return Messages[index];
		}
	}
}
