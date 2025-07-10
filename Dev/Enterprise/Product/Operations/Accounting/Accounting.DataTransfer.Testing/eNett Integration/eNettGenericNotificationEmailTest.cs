using System;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.DataTransfer.eNett_Integration.Testing
{
	public class eNettGenericNotificationEmailTest : AccountingEmailDefTest
	{
		protected override Type EmailDefType
		{
			get { return typeof(eNettGenericNotificationEmail); }
		}

		public void TestContent()
		{
			string message = "This is an error message";
			eNettGenericNotificationEmail email = new eNettGenericNotificationEmail(message);

			string expectedResult = string.Format(@"<PRE>
An error occurred during processing transaction(s) received from eNett.
Message: {0}
</PRE>", message);

			AssertEquals(expectedResult, email.GetBody_ForTestOnly());
		}

		public void TestContentType()
		{
			AssertEquals(new eNettGenericNotificationEmail(string.Empty).ContentType, EmailContentTypes.HTML);
		}
	}
}
