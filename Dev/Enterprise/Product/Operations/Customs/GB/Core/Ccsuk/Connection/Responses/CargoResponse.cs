using System;

namespace Enterprise.Customs.GB.Ccsuk.Connection
{
	public class CargoResponse : ShortMessageResponse
	{
		/// <summary>
		/// For incoming responses to our uploads
		/// </summary> 
		public CargoResponse(string bodyText)
			: base(bodyText)
		{ }

		/// <summary>
		/// For sending TO ccuk to acknowledge what we received
		/// </summary> 
		public CargoResponse(Exception exception, string hostNameCosTheChildThreadCannotSeeTheEffingRegistry)
			: base(MakeResponseBody(exception, hostNameCosTheChildThreadCannotSeeTheEffingRegistry))
		{
		}

		static string MakeResponseBody(Exception exception, string hostNameCosTheChildThreadCannotSeeTheEffingRegistry)
		{
			string shortMessageCode = ShortMessageTypeCodes.Codes.CargoMessageResponse;
			string successFailurePayload = ConvertExceptionToSuccessFailurePayload(exception);
			return ShortMessage.ServiceMessageIdentifier   // SM
				+ shortMessageCode   // 08
				+ hostNameCosTheChildThreadCannotSeeTheEffingRegistry   // "CARGOWISE  "
				+ successFailurePayload;
		}

		static string ConvertExceptionToSuccessFailurePayload(Exception exception)
		{
			if (exception == null)
			{
				return "0000";
			}
			else if (exception is Messaging.Business.MessageProcessingException)
			{
				return "0201"; // Do not retry message processing
			}
			else
			{
				return "0202";
			}
		}

		public override string ReasonForFailure
		{
			get
			{
				switch (ResponseCode)
				{
					case "01":
						return "The cargo message was not delivered due a problem with the host, user or the message. Therefore the message should not be re-tried by the Participant System.";
					case "02":
						return "The cargo message was not delivered due to an internal error at the receiving end. The Participant System should terminate the Application and Socket Sessions, re-establish Socket and Application Sessions and then re-try the message.";

					default:
						throw new Exceptions.ShortMessage.UnknownResponseCode(ResponseCode);
				}
			}
		}

		public bool ShouldRetry
		{
			get
			{
				switch (this.ResponseReasonCode)
				{
					case "00":
						return true;  // for testing
					case "01":
						return false;
					case "02":
						return true;

					default:
						throw new Exceptions.ShortMessage.UnknownResponseReasonCode(ResponseCode);
				}
			}
		}
	}
}
