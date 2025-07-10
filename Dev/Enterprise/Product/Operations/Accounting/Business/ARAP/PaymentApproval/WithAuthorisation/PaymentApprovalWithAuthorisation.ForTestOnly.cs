#if DEBUG

using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public partial class PaymentApprovalWithAuthorisation
	{
		public MultilingualString AV_Calc_DescriptionOfAuthorisationRequiredCore_ForTestOnly => AV_Calc_DescriptionOfAuthorisationRequiredCore;

		public string FirstAuthorisationLogText_ForTestOnly => FirstAuthorisationLogText;

		public string FirstAuthorisationWithdrawnLogText_ForTestOnly => FirstAuthorisationWithdrawnLogText;

		public string SecondAuthorisationLogText_ForTestOnly => SecondAuthorisationLogText;

		public string SecondAuthorisationWithdrawnLogText_ForTestOnly => SecondAuthorisationWithdrawnLogText;

		public string ThirdAuthorisationLogText_ForTestOnly => ThirdAuthorisationLogText;

		public string ThirdAuthorisationWithdrawnLogText_ForTestOnly => ThirdAuthorisationWithdrawnLogText;

		public string PaymentCancelledLogText_ForTestOnly => PaymentCancelledLogText;

		public SecurityCheckpoint FirstApprovalCheckpoint_ForTestOnly => FirstApprovalCheckpoint;

		public SecurityCheckpoint SecondApprovalCheckpoint_ForTestOnly => SecondApprovalCheckpoint;

		public SecurityCheckpoint ThirdApprovalCheckpoint_ForTestOnly => ThirdApprovalCheckpoint;

		public SecurityCheckpoint CancelApprovalCheckpoint_ForTestOnly => CancelApprovalCheckpoint;

		public SecurityCheckpoint CancelEPaymentSecurityCheckPoint_ForTestOnly => CancelEPaymentSecurityCheckPoint;

		public MultilingualString GetDescriptionForDisplay_ForTestOnly(PaymentAuthorisationSettings setting)
		{
			return GetDescriptionForDisplay(setting);
		}

		public ZString UserPostedFullyApprovedTransactionLogText_ForTestOnly => UserPostedFullyApprovedTransactionLogText;
	}
}

#endif
