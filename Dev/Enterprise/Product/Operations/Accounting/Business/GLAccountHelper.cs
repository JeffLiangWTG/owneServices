using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public static class GLAccountHelper
	{
		static string GLAccountSecurityErrorMessage
		{
			get { return Res.GetString("7BBD9436-814D-44F0-AD62-ECEB6615AB09", "Please select a valid GL Account. You cannot select a GL Account without Mapping as you do not have following security right: "); }
		}

		public static void CheckLocalAccountDescriptorHasMapping(DependentTransactionLine line)
		{
			if (line == null || line.IsDeleted || line.TransactionHeader == null || line.TransactionHeader.IsDeleted) { return; }

			var security = GetSecurityCheckPoint(line.TransactionHeader.AH_Ledger);
			if (security == null) { return; }

			if (!security.IsAllowed && AccGLAccountDescriptor.GetLocalAccountDescriptor(line.Factory, line.AL_AG) == null)
			{
				line.AL_AGInfo.AddError(GLAccountSecurityErrorMessage + security.DisplayTextPathToSecurityRight);
			}
		}

		public static void CheckLocalAccountDescriptorHasMapping(TransactionHeader header)
		{
			if (header == null || header.IsDeleted) { return; }

			var security = GetSecurityCheckPoint(header.AH_Ledger);
			if (security == null) { return; }

			if (!security.IsAllowed && AccGLAccountDescriptor.GetLocalAccountDescriptor(header.Factory, header.AH_AG) == null)
			{
				header.AH_AGInfo.AddError(GLAccountSecurityErrorMessage + security.DisplayTextPathToSecurityRight);
			}
		}

		static Security.SecurityCheckpoint GetSecurityCheckPoint(string ledger)
		{
			switch (ledger)
			{
				case LedgerTypes.AccountsPayable:
					return Environment.Env.Security.NewPayablesAllowGLAccountWithoutMapping;
				case LedgerTypes.AccountsReceivable:
					return Environment.Env.Security.NewReceivablesAllowGLAccountWithoutMapping;
				case LedgerTypes.CashBook:
					return Environment.Env.Security.NewCashBookAllowGLAccountWithoutMapping;
				case LedgerTypes.General:
					return Environment.Env.Security.GeneralLedgerJournalAllowGLAccountWithoutMapping;
				default:
					return null;
			}
		}
	}
}