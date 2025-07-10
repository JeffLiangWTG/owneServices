using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DE.DataTransfer.Universal;

public class DeclarationDataObjectWriter : EU.DataTransfer.Universal.DeclarationDataObjectWriter, ITopLevelDataObjectWriter
{
	public DeclarationDataObjectWriter(IDataWritingManager manager) : base(manager)
	{
	}

	protected override Customs.DataTransfer.Universal.CommercialInvoiceHeaderDataObjectWriter GetNewCommercialInvoiceHeaderDataObjectWriter(Customs.Business.CusEntryHeader relatedEntry)
		=> new CommercialInvoiceHeaderDataObjectWriter(writeManager, helper, landedCostDataWriter, relatedEntry);
}
