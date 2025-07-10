using System;
using System.Collections.Generic;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.EmailNotification;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing
{
	public class GlobalEInvoiceTokenNotificationEmailCreator : IElectronicMessagingNotificationEmailCreator
	{
		public AccountingEmailDef Create(GlbBranch branch, GlbCompany company, Guid recipientGuid, IEnumerable<EInvoicingCertificateCredential> credentials)
		{
			if (branch == null && company == null)
			{
				throw new ArgumentNullException("Branch or Company can't be null.", innerException: null);
			}

			if (branch != null && company != null)
			{
				throw new ArgumentException("Branch or Company should be null.", innerException: null);
			}

			return branch != null
				? new EInvoiceTokenNotificationEmail(branch, recipientGuid, credentials)
				: new EInvoiceTokenNotificationEmail(company, recipientGuid, credentials);
		}
	}
}
