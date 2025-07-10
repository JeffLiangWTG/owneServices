using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.UniversalDataBuss.Integration;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.IT.DataTransfer.Universal;

public class CustomsEntryHeaderDataObjectWriter : EU.DataTransfer.Universal.CustomsEntryHeaderDataObjectWriter
{
	public CustomsEntryHeaderDataObjectWriter(IDataWritingManager manager, EU.DataTransfer.Universal.UniversalDataObjectWriterHelper helper)
		: base(manager, helper)
	{
	}

	protected override void PopulateAddInfo(Customs.Business.CusEntryHeader entryHeaderBO, UniversalCustoms.EntryHeader entryHeaderData)
	{
		base.PopulateAddInfo(entryHeaderBO, entryHeaderData);

		var entryHeader = (CusEntryHeader)entryHeaderBO;
		if (!entryHeader.CustomsChannel.IsEmpty)
		{
			entryHeaderData.AddInfoCollection.Add(
				new UniversalDataBuss.DataObjects.Universal.AddInfo() { Key = CusEntryHeader.Schema.CustomsChannel, Value = entryHeader.CustomsChannel });
		}
	}

	protected override Customs.DataTransfer.Universal.CustomsEntryNumberDataObjectWriter GetNewCustomsEntryNumberDataObjectWriter()
	{
		return new CustomsEntryNumberDataObjectWriter(writeManager, helper);
	}
}
