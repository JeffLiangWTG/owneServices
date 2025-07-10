using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.Business;

public class ExportOperationDataProvider : IExportOperation
{
	public static ExportOperationDataProvider New(CusEntryHeader entryHeader) => entryHeader == null ? null : new ExportOperationDataProvider(entryHeader);

	public ExportOperationDataProvider(CusEntryHeader entryHeader)
	{
		this.entryHeader = entryHeader;
		declaration = entryHeader.Declaration;
	}

	protected readonly CusEntryHeader entryHeader;
	protected readonly JobDeclaration declaration;

	public string AirWaybill => declaration.JE_TransportMode == Enterprise.Core.Constants.TransportModes.Air ? declaration?.JE_MasterBill.ReturnNullIfEmpty() : null;

	public string GoodsDeclarationReferenceNumber => CusEntryNumberHelper.MovementReferenceNumberWithoutVersion(entryHeader.MovementReferenceNumber);

	public int? GoodsDeclarationReferenceNumberVersion => CusEntryNumberHelper.MovementReferenceNumberVersion(entryHeader.MovementReferenceNumber);

	public string SuccessorOperation => entryHeader.EntryInstruction?.CEI_NextProcedure.ReturnNullIfEmpty();

	public string SpecificCircumstanceIndicator => entryHeader.EntryInstruction.IsSimplified ? null : declaration.JE_SpecificCircumstanceIndicator.ToString();

	public string CommunicationLanguage => declaration.JE_DeclarationLanguage.ReturnNullIfEmpty()?.ToLowerInvariant();
}
