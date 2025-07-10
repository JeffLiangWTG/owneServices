using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class StandaloneCommercialInvoiceDataObjectWriter : DataTransfer.Universal.StandaloneCommercialInvoiceDataObjectWriter
	{
		internal protected StandaloneCommercialInvoiceDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override DataTransfer.Universal.CommercialInvoiceHeaderDataObjectWriter GetNewCommercialInvoiceHeaderDataObjectWriter(DataTransfer.Universal.UniversalDataObjectWriterHelper helper)
		{
			return new CommercialInvoiceHeaderDataObjectWriter(writeManager, (helper));
		}
	}
}
