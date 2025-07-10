using CargoWise.Customs.MX.MessageContracts;

namespace Enterprise.Customs.MX.Manifest.Business
{
	internal class SEWrapper : ISETransactionSetTrailer
	{
		public SEWrapper()
		{
		}

		string ISETransactionSetTrailer.NumberIncludedSegments => SEA309Constants.IncludedGroups;

		string ISETransactionSetTrailer.TransactionSetControlNumber => MXMessage.MessageNumberPlaceHolder;
	}
}
