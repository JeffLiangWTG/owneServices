using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Accounting.GUI
{
	public class ReverseReceivablesInvoiceWhenAPTransactionsArePaidSecurityOverrideProvider : InvoicingSecurityOverrideProvider
	{
		public ReverseReceivablesInvoiceWhenAPTransactionsArePaidSecurityOverrideProvider(InvoicingBase transaction)
			: base(transaction)
		{
		}

		public ReverseReceivablesInvoiceWhenAPTransactionsArePaidSecurityOverrideProvider(MultipleReversingProviderForHeader reversingProvider)
			: base(reversingProvider)
		{
		}

		#region Overrides

		protected override SecurityCore[] RequestSecurityCoresForOneLoginCredential(SecurityCheckpoint checkPoint)
		{
			UserSecurityOverride = RequestLoginCredentials(checkPoint);
			return new SecurityCore[1] { UserSecurityOverride };
		}
		protected override SecurityCore RequestLoginCredentials(SecurityCheckpoint checkPoint)
		{
			SecurityCore security = null;
			if (ReversingProvider != null & UserSecurityOverride != null)
			{
				var checkPointOverride = UserSecurityOverride.SecurityInstance.FindCheckPoint(checkPoint.LookupKey);
				if (checkPointOverride != null && checkPointOverride.IsAllowed)
				{
					security = UserSecurityOverride;
				}
			}
			if (security == null)
			{
				security = base.RequestLoginCredentials(checkPoint);
			}
			return security;
		}

		protected override string GetSecurityOverrideMessageCore(SecurityCheckpoint checkPoint)
		{
			string result = string.Empty;

			if (checkPoint == Env.Security.ReverseReceivablesInvoiceWhenAPTransactionsArePaid ||
				checkPoint.Code.EndsWith(SecurityCore.AllowReversalWhenRelatedAPTrArePaid))
			{
				result = ReverseReceivablesInvoiceWhenAPTransactionsArePaidSecurityOverrideMessage;
			}
			else if (checkPoint == Env.Security.ReverseARSelfBilledInvoiceWhenAPArePaid ||
				checkPoint.Code.EndsWith(SecurityCore.AllowSelfBilledReversalWhenRelatedAPTrArePaid))
			{
				result = ReverseSelfBilledReceivablesInvoiceWhenAPTransactionsArePaidSecurityOverrideMessage;
			}

			result += base.GetSecurityOverrideMessageCore(checkPoint);

			return result;
		}

		#endregion

		string ReverseReceivablesInvoiceWhenAPTransactionsArePaidSecurityOverrideMessage
		{
			get
			{
				return Res.GetString("64f30fe8-8c98-486d-8b40-50ab96be3e3d", @"This Receivable Invoice has at least one related Payable Invoice that is already paid, either partially or fully.
You do not have security rights to reverse AR Invoices that have related AP Invoices that are paid.");
			}
		}

		string ReverseSelfBilledReceivablesInvoiceWhenAPTransactionsArePaidSecurityOverrideMessage
		{
			get
			{
				return Res.GetString("a27476f4-51cf-4dfa-a367-5973893758cf", @"This Self-Billed Receivable Invoice has at least one related Payable Invoice that is already paid, either partially or fully.
You do not have security rights to reverse Self-Billed AR Invoices that have related AP Invoices that are paid.");
			}
		}
	}
}
