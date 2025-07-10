using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.EmailNotification
{
	public class InvoiceDateIncrementingSuspensionEmail : AccountingEmailDef
	{
		public enum EmailType
		{
			SuspensionLiftedByUser,
			SuspensionNeedsToBeLifted
		}

		public InvoiceDateIncrementingSuspensionEmail(EmailType emailType, string companyCode = "")
		{
			ContentType = EmailContentTypes.HTML;
			CurrentEmailType = emailType;
			CompanyCode = companyCode;
		}

		readonly EmailType CurrentEmailType;
		readonly ZString CompanyCode;

		protected override GuidRegistryItem Recipient
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.InvoiceDateIncrementingSuspensionNotifyGroup;
			}
		}

		protected override string GetBody()
		{
			var body = string.Empty;

			if (CurrentEmailType == EmailType.SuspensionLiftedByUser)
			{
				body = Res.GetString("ba57d39f-ad80-4078-8782-a5e0c9d883ca",
@"<p>The Invoice Date Incrementing Suspension has been lifted by {0} for Company {1}.  Local operation time: {2}. UTC operation time: {3}.</p>

<p>The ""Current Invoice Date"" has been reverted to Today and will resume daily incrementing.</p>",
				Env.CurrentUser.FullName, Env.CurrentCompany.Code, ZDateTime.Now, ZDateTime.UtcNow);
			}
			else if (CurrentEmailType == EmailType.SuspensionNeedsToBeLifted)
			{
				GlbCompany company = null;
				if (!string.IsNullOrWhiteSpace(CompanyCode))
				{
					company = new BusinessObjectFactory().LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, CompanyCode);
				}
				var currentInvDate = ARDefaultInvoiceAndPostDateCalculator.GetDefaultDate(ZDateTime.MinSmallDateTimeValue, company);

				body = Res.GetString("2ead4435-93f5-4e3b-b21a-f848c3ed85bf",
					@"<p>Invoice Date Incrementing is suspended for Company {0}.</p><p>While the daily incrementing suspension is in effect, the ""Current Invoice Date"" will remain as {1}.<br>To lift the suspension, go to Manage > Receivables > Receivables Transactions > Actions > Reinstate Daily Invoice Date Incrementing.</p>",
					CompanyCode, currentInvDate.ToShortDateString());
			}

			return body;
		}

		protected override string GetSubject()
		{
			var subject = string.Empty;

			if (CurrentEmailType == EmailType.SuspensionLiftedByUser)
			{
				subject = Res.GetString("fc46a8b2-01bb-4177-8a76-45493d8b6f8d", "The Invoice Date Incrementing Suspension has been lifted.");
			}
			else if (CurrentEmailType == EmailType.SuspensionNeedsToBeLifted)
			{
				subject = Res.GetString("76514588-52d5-4bfd-8bea-2fb2cc5e44c4", "The Invoice Date Incrementing Suspension needs to be lifted.");
			}

			return subject;
		}
	}
}
