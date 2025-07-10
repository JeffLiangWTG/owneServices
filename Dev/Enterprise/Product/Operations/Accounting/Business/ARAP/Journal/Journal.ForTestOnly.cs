#if DEBUG

using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Journal
{
	public partial class Journal
	{
		public ZGuid MatchingAccount_ForTestOnly => MatchingAccount;

		public ZString DebitCreditSignDefault_ForTestOnly => DebitCreditSignDefault;

		public ZGuid WHTAccount_ForTestOnly => WHTAccount;

		public ZGuid JournalAccountDefault_ForTestOnly => JournalAccountDefault;

		public AccTransactionHeaderValidation GetNewMatchingValidation_ForTestOnly()
		{
			return GetNewMatchingValidation();
		}
	}
}

#endif
