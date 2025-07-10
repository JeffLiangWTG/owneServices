using System;
using CargoWise.IO;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	sealed class MailSaverBaseTest_WithNullLogger : MailSaverBaseTest
	{
		protected override void SetUp()
		{
			TestCaseHelper.ClearTable(MailDBItemsSchema.Constants.TableName);
			pop3Downloader = new TestPop3Downloader();
			logger = null;
			saver = new TestMailSaver(pop3Downloader, logger);
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}
	}
}
