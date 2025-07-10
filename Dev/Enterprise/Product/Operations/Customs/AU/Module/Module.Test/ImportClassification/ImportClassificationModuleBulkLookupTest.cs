using System;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	sealed class ImportClassificationModuleBulkLookupTest : TestCase
	{
		public void TestBulkLookupMenuItem()
		{
			using (ImportClassificationModuleForTesting module = new ImportClassificationModuleForTesting())
			{
				//no permission
				Env.Security.AUImportLookupBulkChange.IsAllowed = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module.BulkLookupChangeMenuItem_Click();
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains(Env.Security.AUImportLookupBulkChange.ErrorMessageForNotAllowed));
				//with permission
				Env.Security.AUImportLookupBulkChange.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module.BulkLookupChangeMenuItem_Click();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		sealed class ImportClassificationModuleForTesting : ImportClassificationModule
		{
			public void BulkLookupChangeMenuItem_Click() => BulkLookupChangeMenuItem_Click(null, EventArgs.Empty);
		}
	}
}
