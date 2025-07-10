using CargoWise.Customs.MX.MessageContracts;

namespace Enterprise.Customs.MX.Manifest.Business
{
	internal class IEAWrapper : IIEAInterchangeControlTrailer
	{
		public IEAWrapper()
		{
		}

		string IIEAInterchangeControlTrailer.NumberIncludedFunctionalGroups => SEA309Constants.IncludedGroups;

		string IIEAInterchangeControlTrailer.InterchangeControlNumber => MXMessage.MessageNumberPlaceHolder;
	}
}
