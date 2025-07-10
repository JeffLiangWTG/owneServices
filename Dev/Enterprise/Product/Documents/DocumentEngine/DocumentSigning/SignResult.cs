using CargoWise.Types;

namespace Enterprise.DocumentEngine.DigitalSignature
{
	public class SignResult
	{
		public SignResult(ZString errorMessage, ZString transactionId)
		{
			ErrorMessage = errorMessage;
			TransactionId = transactionId;
		}

		public ZString ErrorMessage { get; }
		public ZString TransactionId { get; }
	}
}
