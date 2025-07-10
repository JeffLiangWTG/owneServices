using CargoWise.Customs.MX.MessageContracts;

namespace Enterprise.Customs.MX.Manifest.Business
{
	internal class GEWrapper : IGEFunctionalGroupTrailer
	{
		public GEWrapper()
		{
		}

		string IGEFunctionalGroupTrailer.NumberOfTransactionSetsIncluded => SEA309Constants.IncludedGroups;

		string IGEFunctionalGroupTrailer.GroupControlNumber => MXMessage.MessageNumberPlaceHolder;
	}
}
