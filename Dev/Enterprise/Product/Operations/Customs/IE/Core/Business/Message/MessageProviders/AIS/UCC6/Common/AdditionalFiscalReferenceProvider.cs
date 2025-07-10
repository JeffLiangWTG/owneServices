using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class AdditionalFiscalReferenceProvider : IAdditionalFiscalReference
	{
		public AdditionalFiscalReferenceProvider(int sequenceNumber, string role, string vatIdentificationNumber)
		{
			this.SequenceNumber = sequenceNumber.ToString();
			this.Role = role;
			this.VatIdentificationNumber = vatIdentificationNumber;
		}

		public string Role { get; }

		public string VatIdentificationNumber { get; }

		public string SequenceNumber { get; }
	}
}
