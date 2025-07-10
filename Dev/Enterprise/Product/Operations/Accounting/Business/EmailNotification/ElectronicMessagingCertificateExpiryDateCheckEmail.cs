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

namespace Enterprise.Accounting.ElectronicMessaging.ElectronicMessagingCertificateExpiryDateCheck
{
	public class ElectronicMessagingCertificateExpiryDateCheckEmail : AccountingEmailDef
	{
		public ElectronicMessagingCertificateExpiryDateCheckEmail(GlbCompany company, Guid recipientGuid, IEnumerable<EInvoicingCertificateCredential> credentials)
		{
			Argument.NotNull(credentials, nameof(credentials));
			Argument.NotNull(company, nameof(company));

			ContentType = EmailContentTypes.HTML;
			RecipientGuid = recipientGuid;
			Subject = $"E-Reporting Certificate expiry notification[{company.GC_Code}]";
			Body = GetBodyCore(GetHyperlink(), credentials);

			string GetHyperlink()
			{
				return string.Format("<a href='{0}'>{1}</a>"
				, ObjectFactory.Get<IShowViewFormUrlCreator>().Create(ControllerIDs.GlbCompany, company.PK.ToGuid())
				, $"{company.HumanReadableName} [{company.GC_Code}]{company.GC_Name}");
			}
		}

		public ElectronicMessagingCertificateExpiryDateCheckEmail(GlbBranch branch, Guid recipientGuid, IEnumerable<EInvoicingCertificateCredential> credentials)
		{
			Argument.NotNull(credentials, nameof(credentials));
			Argument.NotNull(branch, nameof(branch));

			ContentType = EmailContentTypes.HTML;
			RecipientGuid = recipientGuid;
			Subject = $"E-Reporting Certificate expiry notification[{branch.Company.GC_Code} - {branch.GB_Code}]";
			Body = GetBodyCore(GetHyperlink(), credentials);

			string GetHyperlink()
			{
				return string.Format("<a href='{0}'>{1}</a>"
					, ObjectFactory.Get<IShowViewFormUrlCreator>().Create(ControllerIDs.GlbBranch, branch.PK.ToGuid())
					, $"{branch.HumanReadableName} [{branch.GB_Code}]{branch.GB_BranchName}");
			}
		}

		protected Guid RecipientGuid { get; set; }

		string GetBodyCore(string hyperlink, IEnumerable<EInvoicingCertificateCredential> credentials)
		{
			const string bodyformat = @"<html>
<style>
</style>
<body>
<div class=WordSection1>
<p class=MsoNormal>The following E-Invoicing certificates are closed to the
expiry date.<o:p></o:p></p>
<p class=MsoNormal><o:p>&nbsp;</o:p></p>
<p class=MsoNormal>{0}<o:p></o:p></p>
{1}
<p class=MsoNormal><o:p>&nbsp;</o:p></p>
<p class=MsoNormal>Please check and make the necessary extension if required.<o:p></o:p></p>
</div>
</body>
</html>";
			var crendentialLists = string.Join(System.Environment.NewLine
				, credentials.Select((x, index) => $"<p class=MsoNormal>{index + 1}. {x.GP_MailBoxID}, Issuer: {x.IssuerNameCommonName}, Expiry Date: {x.GP_ExpiryDate:dd-MMM-yy HH:mm}<o:p></o:p></p>")
			);
			return string.Format(bodyformat, hyperlink, crendentialLists);
		}

		protected override string GetSubject()
		{
			return Subject;
		}

		protected override string GetBody()
		{
			return Body;
		}

		protected override void SendCore()
		{
			Env.OutgoingMailManager.CreateAndSave(this, GetRecipient(), GroupSourceLocator.GetFromRegistryItem(AccountingConfigurationRegistry.Instance.EInvoicingCertificateExpiryNotificationGroup));
		}

		protected override void CreateCore(ITransactionParticipant factory)
		{
			Env.OutgoingMailManager.Create(factory, this, GetRecipient(), GroupSourceLocator.GetFromRegistryItem(AccountingConfigurationRegistry.Instance.EInvoicingCertificateExpiryNotificationGroup));
		}

		protected override Guid GetRecipient()
		{
			return RecipientGuid;
		}

		protected override GuidRegistryItem Recipient
		{
			get { return null; }
		}
	}
}
