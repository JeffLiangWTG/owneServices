using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.Integration;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.EU.DataTransfer.Universal
{
	public class CustomsEntryNumberDataObjectReader : Customs.DataTransfer.Universal.CustomsEntryNumberDataObjectReader<CusEntryHeader>
	{
		public CustomsEntryNumberDataObjectReader(
			UniversalCustoms.EntryNumber entryNumberDataObject,
			IXmlImportLogger logger,
			Customs.DataTransfer.Universal.UniversalDataObjectReaderHelper helper,
			CusEntryHeader parent) : base(entryNumberDataObject, logger, helper, parent)
		{
		}
	}
}
