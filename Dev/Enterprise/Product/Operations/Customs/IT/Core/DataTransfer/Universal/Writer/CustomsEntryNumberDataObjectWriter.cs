using Enterprise.UniversalDataBuss.Integration;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.IT.DataTransfer.Universal;

public sealed class CustomsEntryNumberDataObjectWriter : EU.DataTransfer.Universal.CustomsEntryNumberDataObjectWriter
{
	public CustomsEntryNumberDataObjectWriter(
		IDataWritingManager manager,
		Customs.DataTransfer.Universal.UniversalDataObjectWriterHelper helper) : base(manager, helper)
	{
	}

	protected override UniversalCustoms.EntryNumber PopulateDataObject(Common.CusEntryNumber entryNumberBO)
	{
		var entryNumber = base.PopulateDataObject(entryNumberBO);
		entryNumber.EntryLineReference = entryNumberBO.CE_EntryLineReference;
		return entryNumber;
	}
}
