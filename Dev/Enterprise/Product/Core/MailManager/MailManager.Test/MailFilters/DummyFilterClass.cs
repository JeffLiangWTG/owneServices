using System;
using System.Collections.Generic;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MessageProcessor;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	sealed class DummyFilterClass
	{
		readonly Dictionary<string, List<MailItem>> processedItems = new Dictionary<string, List<MailItem>>();

		public ICollection<MailItem> GetItemsProcessedBy(string methodName)
			=> processedItems.TryGetValue(methodName, out var res) ? res : Array.Empty<MailItem>();

		[MessageFilterCondition("MI_Subject", "^FOO")]
		public bool SubjectStartsWithFoo(MailItem mi)
			=> throw new InvalidOperationException("Should not execute the process methods, we're just filtering");

		[MessageFilterCondition("MI_Subject", "^BAR")]
		public bool SubjectStartsWithBar(MailItem mi)
			=> throw new InvalidOperationException("Should not execute the process methods, we're just filtering");

		[MessageFilterCondition("MI_From", "barry@gmail.com")]
		[MessageFilterCondition("MI_Subject", "Crap")]
		public bool EmailsFromBarryAboutCrap(MailItem mi)
			=> throw new InvalidOperationException("Should not execute the process methods, we're just filtering");
	}
}
