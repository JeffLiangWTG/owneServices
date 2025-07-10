using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.ElectronicMessaging.EmailNotification
{
	public class EInvoiceTokenNotificationEmail : AccountingEmailDef
	{
		readonly Parameters parameters = new Parameters();

		protected override GuidRegistryItem Recipient => null;

		protected override Guid GetRecipient() => parameters.Recipient;

		public EInvoiceTokenNotificationEmail(GlbCompany company, Guid recipient, IEnumerable<EInvoicingCertificateCredential> credentials)
			: this(null, company, recipient, credentials)
		{
			Argument.NotNull(company, "company");
		}

		public EInvoiceTokenNotificationEmail(GlbBranch branch, Guid recipient, IEnumerable<EInvoicingCertificateCredential> credentials)
			: this(branch, null, recipient, credentials)
		{
			Argument.NotNull(branch, "branch");
		}

		EInvoiceTokenNotificationEmail(GlbBranch branch, GlbCompany company, Guid recipient, IEnumerable<EInvoicingCertificateCredential> credentials)
		{
			Argument.NotNull(credentials, "credentials");
			base.ContentType = EmailContentTypes.HTML;

			parameters.Recipient = recipient;
			parameters.Branch = branch;
			parameters.Company = company ?? branch.Company;
			parameters.Credentials = credentials;
		}

		protected override void SendCore()
			=> Env.OutgoingMailManager.CreateAndSave(this, GetRecipient(), GroupSourceLocator.GetFromRegistryItem(AccountingConfigurationRegistry.Instance.EInvoicingCertificateExpiryNotificationGroup));

		protected override void CreateCore(ITransactionParticipant factory)
			=> Env.OutgoingMailManager.Create(factory, this, GetRecipient(), GroupSourceLocator.GetFromRegistryItem(AccountingConfigurationRegistry.Instance.EInvoicingCertificateExpiryNotificationGroup));

		protected override string GetSubject()
			=> (NoResString)$"E-Invoice Token Expiry Notification ({parameters.Company.GC_Code})";

		protected override string GetBody()
			=> (NoResString)@$"<html>
	<style></style>
	<body>
		<p>The following E-Invoicing Refresh Token is about to expire:</p>
		<p>Company {parameters.Company.GC_Code} : {parameters.Company.CompanyName}</p>
		<p>Refresh Token, Issue Date : {parameters.Credentials.First().GP_IssueDate:dd-MMMM-yyyy} , Expiry Date : {parameters.Credentials.First().GP_ExpiryDate:dd-MMMM-yyyy}</p>
		<p>Re-authorization is required before the refresh token expires.</p>
		<p>
			{GetHyperlink()}
		</p>
	</body>
</html>";

		protected virtual string GetHyperlink()
			=> parameters.Branch != null
				? $"<a href='{ObjectFactory.Get<IShowViewFormUrlCreator>().Create(ControllerIDs.GlbBranch, parameters.Branch.PK.ToGuid())}'>{$"{parameters.Branch.HumanReadableName} [{parameters.Branch.GB_Code}]{parameters.Branch.GB_BranchName}"}</a>"
				: $"<a href='{ObjectFactory.Get<IShowViewFormUrlCreator>().Create(ControllerIDs.GlbCompany, parameters.Company.PK.ToGuid())}'>{$"{parameters.Company.HumanReadableName} [{parameters.Company.GC_Code}]{parameters.Company.GC_Name}"}</a>";

		class Parameters
		{
			public GlbBranch Branch { get; set; }
			public GlbCompany Company { get; set; }
			public IEnumerable<EInvoicingCertificateCredential> Credentials { get; set; }
			public Guid Recipient {  get; set; }
		}
	}
}
