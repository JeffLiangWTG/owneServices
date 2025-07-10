using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Testing
{
	public static class TransactionIdHelper
	{
		public static ZString GetRandomSuffix(string text, string transactionId, int suffixLength)
		{
			var transactionIdIndex = text.IndexOf(transactionId);
			if (transactionIdIndex == -1)
			{
				return ZString.Empty;
			}

			return text.Substring(transactionIdIndex + transactionId.Length, suffixLength).TrimEnd('"');
		}
	}
}
