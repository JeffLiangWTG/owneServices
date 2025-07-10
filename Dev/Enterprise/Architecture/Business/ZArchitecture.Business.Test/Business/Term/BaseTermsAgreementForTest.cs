using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Enterprise.ZArchitecture.Environment;
using WTG.TrustedMessaging.Models;

namespace Enterprise.ZArchitecture.Business.Testing
{
	class BaseTermsAgreementForTest : BaseTermsAgreement
	{
		public bool LocalDisplaySatisfied;

		public override string Type { get => throw new NotImplementedException(); }

		public bool IsLoadSucceed { get; set; } = true;

		public bool IsFallbackAssigned { get; set; }

		public override bool IsCurrentUserAllowedToAcknowledgeAgreement => true;

		public override string ErrorMessageForAcknowledgementNotAllowed => null;

		public ErrorMessage ResponseErrorMessage_Exposed
		{
			get => ResponseErrorMessage;
			set => ResponseErrorMessage = value;
		}

		protected override bool IsLocalDisplayConditionSatisfied()
		{
			return LocalDisplaySatisfied;
		}

		public override async Task<bool> TryLoadTerm()
		{
			return await Task.FromResult(IsLoadSucceed);
		}

		public ZDialogResult ShowTermsFetchError_Exposed() => ShowTermsFetchError();
		public void ReportServerSideErrorSilently_Exposed(IEnumerable<ErrorMessage> errorMessages, Exception innerException) => ReportServerSideErrorSilently(errorMessages, innerException);

		public string Contents_Exposed
		{
			get => Contents;
			set => GetType().BaseType.GetProperty("Contents").SetValue(this, value);
		}
	}
}
