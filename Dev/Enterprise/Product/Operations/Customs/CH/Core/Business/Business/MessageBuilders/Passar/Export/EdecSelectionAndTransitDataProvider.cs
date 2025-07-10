using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.Business;

public class EdecSelectionAndTransitDataProvider : ISelectionAndTransit
{
	public static EdecSelectionAndTransitDataProvider New(CusEntryHeader entryHeader) => entryHeader == null ? null : new EdecSelectionAndTransitDataProvider(entryHeader);

	public EdecSelectionAndTransitDataProvider(CusEntryHeader entryHeader)
	{
		this.entryHeader = entryHeader;
		declaration = entryHeader.Declaration;
	}

	protected readonly CusEntryHeader entryHeader;
	protected readonly JobDeclaration declaration;

	public string CustomsOfficeNumber => declaration.JE_CustomsOffice;

	public string DeclarantNumber => declaration.CHDPassword?.GP_UserID;

	public string DeclarationTime => entryHeader.EntryInstruction?.CEI_SubStyle;

	public string OriginalTraderIdentificationNumber => declaration.Supplier?.GetUIDNumber().Left(12);

	public string TraderIdentificationNumber => declaration.Company?.GC_CustomsRegistrationNo;
}
