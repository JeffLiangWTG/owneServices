using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;

namespace Enterprise.Customs.IE.H7.Business
{
	public class AdditionalFiscalReferenceProvider : IAdditionalFiscalReference
	{
		public AdditionalFiscalReferenceProvider(string id)
		{
			vatIdentificationNumber = id;
		}

		readonly string vatIdentificationNumber;

		public string Role => "FR5";

		public string VatIdentificationNumber => vatIdentificationNumber;

		public string SequenceNumber => "1";
	}
}
