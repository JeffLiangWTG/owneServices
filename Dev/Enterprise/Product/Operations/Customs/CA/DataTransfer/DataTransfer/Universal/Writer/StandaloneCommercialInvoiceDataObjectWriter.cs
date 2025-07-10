using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.CA.DataTransfer.Universal
{
	public class StandaloneCommercialInvoiceDataObjectWriter : Customs.DataTransfer.Universal.StandaloneCommercialInvoiceDataObjectWriter
	{
		internal protected StandaloneCommercialInvoiceDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override Customs.DataTransfer.Universal.CommercialInvoiceHeaderDataObjectWriter GetNewCommercialInvoiceHeaderDataObjectWriter(Customs.DataTransfer.Universal.UniversalDataObjectWriterHelper helper)
		{
			return new CommercialInvoiceHeaderDataObjectWriter(writeManager, helper);
		}
	}
}
