using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public abstract class EInvoicingTransactionErrorNotificationEmailTest : TestCaseWithFactory
	{
		public void TestEmailSubjectAndBody()
		{
			var email = GetNewBusinessObject();
			AssertEquals("Email Subject match.", GetExpectedSubject(), email.GetSubject_forTest());
			var body = email.GetBody_forTest();
			var parts = GetExpectedBodyParts();
			foreach (var part in parts)
			{
				AssertContains(part, body);
			}
		}

		#region Implementation

		protected abstract EInvoicingTransactionErrorNotificationEmail GetNewBusinessObject();
		protected abstract ZString GetExpectedSubject();
		protected abstract IEnumerable<ZString> GetExpectedBodyParts();

		#endregion
	}
}