using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.Test;

namespace Enterprise.MailManager.Testing
{
	sealed class LogsRequestProcessorTest : TestCaseWithFactory
	{
		public void TestMessageFilter()
		{
			MailItem mailItem1 = Factory.New<MailItem>();
			mailItem1.MI_Direction = Enterprise.MailManager.MailDirection.Receive;
			mailItem1.MI_Status = Enterprise.MailManager.MailStatus.Queued;
			mailItem1.MI_Subject = LogsRequestProcessor.LogsRequestSubject + @" 2010/02/03 2010/02/10 ABC CS01234567 LON-SSQL-20B\MSSQLSERVER2 ODYSSEYSEIHAM";
			mailItem1.MI_ReceivedDateTime = ZDateTime.UtcNow;
			mailItem1.MI_SendDateTime = ZDateTime.UtcNow;

			MailItem mailItem2 = Factory.New<MailItem>();
			mailItem2.MI_Direction = Enterprise.MailManager.MailDirection.Receive;
			mailItem2.MI_Status = Enterprise.MailManager.MailStatus.Queued;
			mailItem2.MI_Subject = LogsRequestProcessor.LogsRequestSubject + @" 2010/02/03 2010/02/10 ABC CS01234567 SYD-SSQL-20B ODYSSEYFGLAKL";
			mailItem2.MI_ReceivedDateTime = ZDateTime.UtcNow;
			mailItem2.MI_SendDateTime = ZDateTime.UtcNow;

			Factory.Save();

			var helper = new MessageFilterTestHelper<LogsRequestProcessor, MailItem>();

			Assert(helper.Process(mailItem1));
			Assert(helper.Process(mailItem2));
			AssertEquals(2, helper.Log.Count);
			AssertEquals("Information|Enterprise Report Mailed", helper.Log[0]);
			AssertEquals("Information|Enterprise Report Mailed", helper.Log[1]);

			helper.Log.ClearLog();
			var ctx = helper.ProcessorFactory.GetContext(helper.Log);

			Assert(helper.Process(ctx, mailItem1));
			Assert(helper.Process(ctx, mailItem2));
			AssertEquals(2, helper.Log.Count);
		}
	}
}
