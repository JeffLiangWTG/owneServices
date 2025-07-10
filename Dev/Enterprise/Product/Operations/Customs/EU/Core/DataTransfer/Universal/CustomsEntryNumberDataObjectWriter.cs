using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.EU.DataTransfer.Universal
{
	public class CustomsEntryNumberDataObjectWriter : Customs.DataTransfer.Universal.CustomsEntryNumberDataObjectWriter
	{
		public CustomsEntryNumberDataObjectWriter(
			IDataWritingManager manager,
			Customs.DataTransfer.Universal.UniversalDataObjectWriterHelper helper) : base(manager, helper)
		{
		}
	}
}
