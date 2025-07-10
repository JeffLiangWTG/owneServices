using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.EU.DataTransfer.Universal
{
	public class CustomsEntryHeaderDataObjectWriter : Customs.DataTransfer.Universal.CustomsEntryHeaderDataObjectWriter
	{
		public CustomsEntryHeaderDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper)
			: base(manager, helper)
		{
		}

		protected override Customs.DataTransfer.Universal.CustomsEntryNumberDataObjectWriter GetNewCustomsEntryNumberDataObjectWriter()
			=> new CustomsEntryNumberDataObjectWriter(writeManager, helper);

		protected override bool ShouldPopulatePaymentInformationData => true;
	}
}
