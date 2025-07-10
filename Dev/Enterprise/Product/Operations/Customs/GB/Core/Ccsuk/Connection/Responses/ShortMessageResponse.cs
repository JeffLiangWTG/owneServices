using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.Connection.Exceptions.ShortMessage;

namespace Enterprise.Customs.GB.Ccsuk.Connection
{
	public abstract class ShortMessageResponse : ShortMessage
	{
		public ShortMessageResponse(string bodyText)
		{
			this.incomingBody = bodyText;
		}

		public abstract string ReasonForFailure { get; }

		public static ShortMessageResponse CreateResponseFromBodyText(string bodyText)
		{
			string messageTypeNumber = bodyText.Substring(2, 2);
			switch (messageTypeNumber)
			{
				case ShortMessageTypeCodes.Codes.LogonResponse:
					return new LogonResponse(bodyText);
				case ShortMessageTypeCodes.Codes.LogoffResponse:
					return new LogoffResponse(bodyText);
				case ShortMessageTypeCodes.Codes.CargoMessageResponse:
					return new CargoResponse(bodyText);
				case ShortMessageTypeCodes.Codes.ServiceErrorResponse:
					return new ErrorResponse(bodyText);
				case ShortMessageTypeCodes.Codes.ConnectionPoll:
					return new IncomingPollQuasiResponse(bodyText);
				case ShortMessageTypeCodes.Codes.PasswordChangeResponse:
					return new PasswordResponse(bodyText);
				default:
					throw new UnknownShortMessageCode(messageTypeNumber);
			}
		}

		public virtual bool WasOperationSuccessful
		{
			get
			{
				return this.ResponseCode == "00";
			}
		}

		protected override string ShortMessageNumber
		{
			get
			{
				return this.incomingBody.Substring(2, 2);
			}
		}

		protected override string ShortMessageBody
		{
			get
			{
				return incomingBody.Substring(14);
			}
		}

		public string ResponseCode
		{
			get
			{
				return ((ZString)ShortMessageBody).Left(2);
			}
		}

		public string ResponseReasonCode
		{
			get
			{
				return ((ZString)ShortMessageBody).Right(2);
			}
		}
	}
}
