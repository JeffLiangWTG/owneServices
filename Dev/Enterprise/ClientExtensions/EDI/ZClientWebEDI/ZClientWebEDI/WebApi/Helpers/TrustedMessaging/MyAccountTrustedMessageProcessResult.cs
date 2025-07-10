using Enterprise.Client.EDI.TrustedMessaging.Business;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class MyAccountTrustedMessageProcessResult<T>
		where T : TrustedInfo
	{
		public MyAccountTrustedMessageProcessResult(MyAccountTrustedMessageProcessStatus status)
		{
			ProcessStatus = status;
			Info = default;
		}

		public MyAccountTrustedMessageProcessResult(MyAccountTrustedMessageProcessStatus status, T info)
		{
			ProcessStatus = status;
			Info = info;
		}

		public MyAccountTrustedMessageProcessResult(MyAccountTrustedMessageProcessStatus status, T info, EdiTrustedSystem trustedSystem)
		{
			ProcessStatus = status;
			Info = info;
			TrustedSystem = trustedSystem;
		}

		public MyAccountTrustedMessageProcessStatus ProcessStatus;
		public T Info;
		public EdiTrustedSystem TrustedSystem;

		public ErrorMessages ErrorMessages
		{
			get
			{
				switch (ProcessStatus)
				{
					case MyAccountTrustedMessageProcessStatus.SecretKeyNotUpToDate:
						return new ErrorMessages(ErrorCodes.Codes.Critical_SecretKeyNotUpToDate, ErrorCodes.Descriptions.Critical_SecretKeyNotUpToDate);
					case MyAccountTrustedMessageProcessStatus.InfoExpired:
						return new ErrorMessages(ErrorCodes.Codes.Critical_InfoExpired, ErrorCodes.Descriptions.Critical_InfoExpired);
					case MyAccountTrustedMessageProcessStatus.Malformed:
						return new ErrorMessages(ErrorCodes.Codes.Critical_MessageMalformed, ErrorCodes.Descriptions.Critical_MessageMalformed);
					case MyAccountTrustedMessageProcessStatus.InvalidSystemInfo:
						return new ErrorMessages(ErrorCodes.Codes.Validation_InvalidSystem, ErrorCodes.Descriptions.Validation_InvalidSystem);
					case MyAccountTrustedMessageProcessStatus.CertificateMismatched:
						return new ErrorMessages(ErrorCodes.Codes.Critical_CertificateMismatched, ErrorCodes.Descriptions.Critical_CertificateMismatched);
					default:
						return new ErrorMessages();
				}
			}
		}
	}

	public enum MyAccountTrustedMessageProcessStatus
	{
		Successful = 0,
		SecretKeyNotUpToDate = 1,
		InfoExpired = 2,
		Malformed = 3,
		MissingCriticalClaims = 4,
		InvalidSystemInfo = 5,
		CertificateMismatched = 6,
	}
}
