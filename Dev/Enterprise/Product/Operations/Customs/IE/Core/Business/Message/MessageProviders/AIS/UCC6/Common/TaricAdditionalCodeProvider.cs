using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class TaricAdditionalCodeProvider : ITaricAdditionalCode
	{
		public TaricAdditionalCodeProvider(int sequenceNumber, JobComInvoiceLine invoiceLine)
		{
			SequenceNumber = sequenceNumber.ToString();
			this.invoiceLine = invoiceLine;
		}

		readonly JobComInvoiceLine invoiceLine;

		public string TaricAdditionalCode => invoiceLine.JI_SupplementaryCode1 + invoiceLine.JI_SupplementaryCode2 + invoiceLine.JI_AdditionalSupplements;

		public string SequenceNumber { get; }
	}
}
