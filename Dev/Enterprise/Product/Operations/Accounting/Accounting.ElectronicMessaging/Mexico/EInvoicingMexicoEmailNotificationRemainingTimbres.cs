using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico
{
	public class EInvoicingMexicoEmailNotificationRemainingTimbres : AccountingEmailDef
	{
		public EInvoicingMexicoEmailNotificationRemainingTimbres(AccEInvoicingBatch invoiceBatch, int remainingStamps)
		{
			base.ContentType = EmailContentTypes.HTML;
			CompanyCode = invoiceBatch.Company.GC_Code;
			RemainingStamps = remainingStamps;
		}

		protected override GuidRegistryItem Recipient => AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup;

		protected override string GetBody() => GetBodyEmail();

		protected override string GetSubject() => Res.GetString("1167878a-6c56-43e1-9ce6-d39fa1aec406", "E-Reporting '{0}' availability Notification [{1}]", "Folios/Timbres", CompanyCode);

		string GetBodyEmail()
		{
			var yourCompanyHasTimbresCount = Res.GetString("d59a76bc-6cdf-4d82-a62b-5bf60f3f7789", "Your company currently has the following number of '{0}' available", "Timbres");
			var checkQuantityIsSufficent = Res.GetString("d59a76bc-6cdf-4d82-a62b-5bf60f3f7780", "Please check if this quantity is sufficient.");
			var inCaseYourCompanyNeedsANewPack = Res.GetString("4a8edec5-9fd2-48c0-ac8d-500b3a6918e6", "In case your company needs a new pack of '{0}', please submit a new e-Request as listed", "Timbres");
			var belowList = Res.GetString("8338b7e6-260d-407a-b37b-14db02df0d30", "below and a new pack will be assigned as soon as possible.");
			var eRequestSummary = Res.GetString("e0d0302c-2a2e-448b-b09f-8414ddb69778", "Mexico Electronic Invoicing Production, Request for new pack of '{0}'", "Timbres");
			var summaryLabel = Res.GetString("1cff5f77-0b63-47b7-bfcb-c44afade1d87", "Summary");
			var detailsLabel = Res.GetString("fb226c83-a131-42ef-a1e8-625d4ae335eb", "Details");
			var detailsText = Res.GetString("0c11ff4e-bad1-4141-bc2e-aaf261298fc1", "Name and {0} of your Mexico Company", "RFC");

			return @$"<html><body>
<p>{yourCompanyHasTimbresCount}:</p>
<p><strong>{RemainingStamps}</strong></p>
<p>{checkQuantityIsSufficent}</p>
<p>{inCaseYourCompanyNeedsANewPack}<br/>{belowList}</p>
<p><strong><u>e-Request {detailsLabel}:</u></strong></p>
<p><strong>1) </strong>{summaryLabel} = &quot;{eRequestSummary}&quot;</p>
<p><strong>2) </strong>{detailsLabel} = <strong>&lt;{detailsText}&gt;</strong></p>
<a href=""https://myaccount.cargowise.com/Home/eRequestManagementPortal.aspx"">eRequest Management Portal</a>
</body></html>";
		}

		ZString CompanyCode { get; }
		int RemainingStamps { get; }
	}
}
