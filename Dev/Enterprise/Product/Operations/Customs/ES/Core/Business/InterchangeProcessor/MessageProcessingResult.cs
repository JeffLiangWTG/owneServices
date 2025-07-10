namespace Enterprise.Customs.ES.Business
{
	public sealed class MessageProcessingResult
	{
		MessageProcessingResult(bool isProcessed, bool isSuccessful, byte[] responseMessage, ResponseStatus responseStatus, string errorMessage)
		{
			IsProcessed = isProcessed;
			IsSuccessful = isSuccessful;
			ResponseMessage = responseMessage;
			ResponseStatus = responseStatus;
			ErrorMessage = errorMessage;
		}

		public bool IsProcessed { get; }

		public bool IsSuccessful { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "The design is relying on the ResponseMessage being null, ReadOnlyMemory<byte> does not provide this")]
		public byte[] ResponseMessage { get; }

		public ResponseStatus ResponseStatus { get; }

		public string ErrorMessage { get; }

		public static MessageProcessingResult Success(byte[] responseMessage)
		{
			return new MessageProcessingResult(true, true, responseMessage, ResponseStatus.Successful, null);
		}

		public static MessageProcessingResult Timeout(string message)
		{
			return new MessageProcessingResult(false, false, null, ResponseStatus.Timeout, message);
		}

		public static MessageProcessingResult RemoteServerError(string message)
		{
			return new MessageProcessingResult(false, false, null, ResponseStatus.InternalServerError, message);
		}

		public static MessageProcessingResult BadRequest(string message)
		{
			return new MessageProcessingResult(true, false, null, ResponseStatus.BadRequest, message);
		}

		public static MessageProcessingResult Unauthorized(string message)
		{
			return new MessageProcessingResult(true, false, null, ResponseStatus.Unauthorized, message);
		}

		public static MessageProcessingResult Duplicate(string message)
		{
			return new MessageProcessingResult(true, false, null, ResponseStatus.Duplicate, message);
		}
	}
}
