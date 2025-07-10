using System;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico.Testing
{
	public class EInvoicingMexicoEmailNotificationRemainingTimbresTest : AccountingEmailDefTest
	{
		public void TestEmailSubject()
		{
			var emailDef = new EInvoicingMexicoEmailNotificationRemainingTimbres(InvoiceBatch, 1);
			var expectedSubject = "E-Reporting 'Folios/Timbres' availability Notification [EDI]";
			var subject = GetSubject(emailDef);

			AssertEquals(expectedSubject, subject);

			InvoiceBatch.Company.GC_Code = "BOB";
			emailDef = new EInvoicingMexicoEmailNotificationRemainingTimbres(InvoiceBatch, 1);
			expectedSubject = "E-Reporting 'Folios/Timbres' availability Notification [BOB]";
			subject = GetSubject(emailDef);

			AssertEquals(expectedSubject, subject);
		}

		public void TestEmailBody()
		{
			var emailDef = new EInvoicingMexicoEmailNotificationRemainingTimbres(InvoiceBatch, 1);
			var expectedBody = @"<html><body>
<p>Your company currently has the following number of 'Timbres' available:</p>
<p><strong>1</strong></p>
<p>Please check if this quantity is sufficient.</p>
<p>In case your company needs a new pack of 'Timbres', please submit a new e-Request as listed<br/>below and a new pack will be assigned as soon as possible.</p>
<p><strong><u>e-Request Details:</u></strong></p>
<p><strong>1) </strong>Summary = &quot;Mexico Electronic Invoicing Production, Request for new pack of 'Timbres'&quot;</p>
<p><strong>2) </strong>Details = <strong>&lt;Name and RFC of your Mexico Company&gt;</strong></p>
<a href=""https://myaccount.cargowise.com/Home/eRequestManagementPortal.aspx"">eRequest Management Portal</a>
</body></html>";

			var body = GetBody(emailDef);

			AssertEquals(expectedBody, body);

			emailDef = new EInvoicingMexicoEmailNotificationRemainingTimbres(InvoiceBatch, 3);
			expectedBody = @"<html><body>
<p>Your company currently has the following number of 'Timbres' available:</p>
<p><strong>3</strong></p>
<p>Please check if this quantity is sufficient.</p>
<p>In case your company needs a new pack of 'Timbres', please submit a new e-Request as listed<br/>below and a new pack will be assigned as soon as possible.</p>
<p><strong><u>e-Request Details:</u></strong></p>
<p><strong>1) </strong>Summary = &quot;Mexico Electronic Invoicing Production, Request for new pack of 'Timbres'&quot;</p>
<p><strong>2) </strong>Details = <strong>&lt;Name and RFC of your Mexico Company&gt;</strong></p>
<a href=""https://myaccount.cargowise.com/Home/eRequestManagementPortal.aspx"">eRequest Management Portal</a>
</body></html>";

			body = GetBody(emailDef);

			AssertEquals(expectedBody, body);
		}

		public void TestEmailContentType()
		{
			var emailDef = new EInvoicingMexicoEmailNotificationRemainingTimbres(InvoiceBatch, 1);
			AssertEquals("Content Type is HTML by default in constructor", EmailContentTypes.HTML.ContentTypeCode, emailDef.ContentType.ContentTypeCode);
		}

		AccEInvoicingBatch InvoiceBatch => invoiceBatch ??= new TestObjectCreator(Factory).CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
		AccEInvoicingBatch invoiceBatch;

		protected override Type EmailDefType => typeof(EInvoicingMexicoEmailNotificationRemainingTimbres);
	}
}
