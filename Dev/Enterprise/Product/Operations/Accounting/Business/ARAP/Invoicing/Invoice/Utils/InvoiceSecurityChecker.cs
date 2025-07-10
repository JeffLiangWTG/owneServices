using System;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public interface IInvoiceSecurityChecker
	{
		BasicSecuritySettings GetBasicSecuritySettings(string ledger, string type);
	}

	public partial class InvoiceSecurityChecker : IInvoiceSecurityChecker
	{
		BasicSecuritySettings IInvoiceSecurityChecker.GetBasicSecuritySettings(string ledger, string type)
		{
			return ledger switch
			{
				LedgerTypes.AccountsPayable => GetPayableSecuritySettings(type),
				_ => throw new NotImplementedException(),
			};
		}
	}

	partial class InvoiceSecurityChecker
	{
		BasicSecuritySettings GetPayableSecuritySettings(string type)
		{
			return type switch
			{
				TransactionTypes.Invoice => APInvoiceSecuritySettings,
				TransactionTypes.CreditNote => APCreditNoteSecuritySettings,
				_ => throw new NotImplementedException(),
			};
		}

		BasicSecuritySettings APInvoiceSecuritySettings { get; } = new BasicSecuritySettings
		{
			View = Env.Security.ViewPayablesTransaction,
			Edit = Env.Security.ViewPayablesTransaction,
			New = Env.Security.NewPayablesInvoice,
			Delete = Env.Security.ReversePayablesInvoice
		};

		BasicSecuritySettings APCreditNoteSecuritySettings { get; } = new BasicSecuritySettings
		{
			View = Env.Security.ViewPayablesTransaction,
			Edit = Env.Security.ViewPayablesTransaction,
			New = Env.Security.NewPayablesCreditNote,
			Delete = Env.Security.ReversePayablesCreditNote
		};
	}

	public class BasicSecuritySettings
	{
		public SecurityCheckpoint View { get; set; }
		public SecurityCheckpoint Edit { get; set; }
		public SecurityCheckpoint New { get; set; }
		public SecurityCheckpoint Delete { get; set; }
	}
}
