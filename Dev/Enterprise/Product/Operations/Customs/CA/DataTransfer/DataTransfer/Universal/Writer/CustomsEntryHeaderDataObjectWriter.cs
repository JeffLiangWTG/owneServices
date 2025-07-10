using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.CA.DataTransfer.Universal
{
	public class CustomsEntryHeaderDataObjectWriter : Customs.DataTransfer.Universal.CustomsEntryHeaderDataObjectWriter
	{
		public CustomsEntryHeaderDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper)
			: base(manager, helper)
		{
		}

		protected new UniversalDataObjectWriterHelper helper
		{
			get { return (UniversalDataObjectWriterHelper)base.helper; }
		}

		protected override Customs.DataTransfer.Universal.CustomsEntryLineDataObjectWriter GetNewCustomsEntryLineDataObjectWriter()
		{
			return new CustomsEntryLineDataObjectWriter(writeManager, helper);
		}
	}
}
