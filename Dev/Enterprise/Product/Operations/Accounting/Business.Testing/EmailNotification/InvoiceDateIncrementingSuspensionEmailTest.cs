using System;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.EmailNotification
{
	public class InvoiceDateIncrementingSuspensionEmailTest : MasterFiles.Business.Testing.AccountingEmailDefTest
	{
		protected override Type EmailDefType
		{
			get
			{
				return typeof(InvoiceDateIncrementingSuspensionEmail);
			}
		}

		[TestDate(2015, 5, 1)]
		public void TestEmail()
		{
			var mail = new InvoiceDateIncrementingSuspensionEmail(InvoiceDateIncrementingSuspensionEmail.EmailType.SuspensionLiftedByUser);
			string expectedSubject = @"The Invoice Date Incrementing Suspension has been lifted.";
			AssertEquals("Mail subject", expectedSubject, GetSubject(mail));
			string expectedBody = @"<p>The Invoice Date Incrementing Suspension has been lifted by CargoWise Support for Company EDI.  Local operation time: 01-May-15 00:00:00. UTC operation time: 01-May-15 00:00:00.</p>

<p>The ""Current Invoice Date"" has been reverted to Today and will resume daily incrementing.</p>";
			AssertEquals("Mail Body", expectedBody, GetBody(mail));
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.MonthEndSuspension.Code);
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-10).ToDateTime());
			mail = new InvoiceDateIncrementingSuspensionEmail(InvoiceDateIncrementingSuspensionEmail.EmailType.SuspensionNeedsToBeLifted, Env.CurrentCompany.Code);
			expectedSubject = @"The Invoice Date Incrementing Suspension needs to be lifted.";
			AssertEquals("Mail subject", expectedSubject, GetSubject(mail));
			expectedBody = @"<p>Invoice Date Incrementing is suspended for Company EDI.</p><p>While the daily incrementing suspension is in effect, the ""Current Invoice Date"" will remain as 30-Apr-15.<br>To lift the suspension, go to Manage > Receivables > Receivables Transactions > Actions > Reinstate Daily Invoice Date Incrementing.</p>";
			AssertEquals("Mail Body", expectedBody, GetBody(mail));
			var nonCurrentCompany = new TestObjectCreator(Factory).NonCurrentCompany;
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(nonCurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.MonthEndSuspension.Code);
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(nonCurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-10).ToDateTime());
			mail = new InvoiceDateIncrementingSuspensionEmail(InvoiceDateIncrementingSuspensionEmail.EmailType.SuspensionNeedsToBeLifted, nonCurrentCompany.GC_Code);
			expectedSubject = @"The Invoice Date Incrementing Suspension needs to be lifted.";
			AssertEquals("Mail subject", expectedSubject, GetSubject(mail));
			expectedBody = string.Format(@"<p>Invoice Date Incrementing is suspended for Company {0}.</p><p>While the daily incrementing suspension is in effect, the ""Current Invoice Date"" will remain as 30-Apr-15.<br>To lift the suspension, go to Manage > Receivables > Receivables Transactions > Actions > Reinstate Daily Invoice Date Incrementing.</p>", nonCurrentCompany.GC_Code);
			AssertEquals("Mail Body", expectedBody, GetBody(mail));
		}
	}
}
