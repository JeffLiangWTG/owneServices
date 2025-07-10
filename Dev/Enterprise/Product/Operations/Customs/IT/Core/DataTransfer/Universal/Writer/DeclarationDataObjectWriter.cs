using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.IT.DataTransfer.Universal;

public class DeclarationDataObjectWriter : EU.DataTransfer.Universal.DeclarationDataObjectWriter
{
	public DeclarationDataObjectWriter(IDataWritingManager manager) : base(manager)
	{
	}

	protected override Customs.DataTransfer.Universal.CustomsEntryInstructionDataObjectWriter GetNewCustomsEntryInstructionDataObjectWriter() => new CustomsEntryInstructionDataObjectWriter(writeManager, helper);
	protected override Customs.DataTransfer.Universal.CustomsEntryHeaderDataObjectWriter GetNewCustomsEntryHeaderDataObjectWriter() => new CustomsEntryHeaderDataObjectWriter(writeManager, helper);
}
