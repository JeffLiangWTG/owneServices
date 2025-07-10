using CargoWise.Customs.MX.MessageContracts;

namespace Enterprise.Customs.MX.Manifest.Business
{
	internal class STWrapper : ISTTransactionSetHeader
	{
		public STWrapper()
		{
		}

		string ISTTransactionSetHeader.TransactionSetIdentifierCode => SEA309Constants.TrasactionSetIdentifierCode;

		string ISTTransactionSetHeader.TransactionSetIdentifierName => MXMessage.MessageNumberPlaceHolder;
	}
}
