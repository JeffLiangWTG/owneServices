using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing.KoreaSouth;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class InvoicingBasePDFPasswordProvider
	{
		public InvoicingBasePDFPasswordProvider(InvoicingBase invoice, DeliverableInfo deliverableInfo)
		{
			Invoice = invoice;
			DeliverableInfo = deliverableInfo;
		}

		public ZString GetPassword()
		{
			foreach (var result in PasswordGetters)
			{
				if (!result.IsEmpty)
				{
					return result;
				}
			}

			return ZString.Empty;
		}

		IEnumerable<ZString> PasswordGetters
		{
			get
			{
				yield return GetPasswordForKRElectronicInvoice();
			}
		}

		ZString GetPasswordForKRElectronicInvoice()
		{
			var password = ZString.Empty;

			if (DeliverableInfo?.MenuItem != null && DeliverableInfo.MenuItem.SU_MenuPath == EInvoicingKoreaSouthConstants.ElectronicInvoiceMenuPath)
			{
				var orgHeader = DeliverableInfo.OrgHeader as OrgHeader;
				password = InvoiceeIDProvider.GetInvoiceeID(orgHeader);

				if (password.IsEmpty && Invoice.Header != null && Invoice.Header.PK != orgHeader?.PK)
				{
					password = InvoiceeIDProvider.GetInvoiceeID(Invoice.Header);
				}

				if (password.IsEmpty)
				{
					password = AccountingConfigurationRegistry.Instance.ElectronicInvoiceDocumentFallbackPassword.Value;
				}
			}

			return password;
		}

		InvoicingBase Invoice { get; }
		DeliverableInfo DeliverableInfo { get; }
	}
}
