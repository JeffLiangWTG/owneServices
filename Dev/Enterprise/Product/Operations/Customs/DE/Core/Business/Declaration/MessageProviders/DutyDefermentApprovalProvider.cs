using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public sealed class DutyDefermentApprovalProvider : IDutyDefermentApproval
	{
		public DutyDefermentApprovalProvider(IDefermentAccount baseProvider)
		{
			defermentAccount = Argument.NotNull(baseProvider, nameof(baseProvider));
		}

		public string Type => defermentAccount.Type;

		public string ApplicationType => defermentAccount.ApplicationType;

		public string AccountPrefix => defermentAccount.AccountPrefix;

		public string AccountNumber => defermentAccount.AccountNumber;

		public string AuthorisationNumber => defermentAccount.AuthorisationNumber;

		public string Applicant => defermentAccount.Applicant;

		readonly IDefermentAccount defermentAccount;
	}
}
