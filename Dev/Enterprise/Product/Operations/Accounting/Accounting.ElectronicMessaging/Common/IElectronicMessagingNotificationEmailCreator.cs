using System;
using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public interface IElectronicMessagingNotificationEmailCreator
	{
		AccountingEmailDef Create(GlbBranch branch, GlbCompany company, Guid recipientGuid, IEnumerable<EInvoicingCertificateCredential> credentials);
	}
}
