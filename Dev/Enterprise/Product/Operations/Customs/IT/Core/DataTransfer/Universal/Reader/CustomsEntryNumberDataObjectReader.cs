using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.IT.DataTransfer.Universal;

public sealed class CustomsEntryNumberDataObjectReader : EU.DataTransfer.Universal.CustomsEntryNumberDataObjectReader
{
	public CustomsEntryNumberDataObjectReader(
		UniversalCustoms.EntryNumber entryNumberDataObject,
		IXmlImportLogger logger,
		UniversalDataObjectReaderHelper helper,
		CusEntryHeader parent) : base(entryNumberDataObject, logger, helper, parent)
	{
	}

	protected override void PopulateBusinessObject(CusEntryNumber entryNumber)
	{
		base.PopulateBusinessObject(entryNumber);
		var entryNumberRow = GetColumnIndexer(entryNumber);
		SetValue(entryNumberRow, CusEntryNumSchema.CE_EntryLineReference, dataObject.EntryLineReference);
	}
}
