using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Edec.GoodsDeclarations;

namespace Enterprise.Customs.CH.Business;

public class EdecGoodsItemOriginDataProvider : IEdecGoodsItemOrigin
{
	public static EdecGoodsItemOriginDataProvider New(CusEntryLine entryLine) => entryLine == null ? null : new EdecGoodsItemOriginDataProvider(entryLine);

	EdecGoodsItemOriginDataProvider(CusEntryLine entryLine)
	{
		entryLine = Argument.NotNull(entryLine, nameof(entryLine));
		invoiceLine = entryLine.RandomLine;
	}
	readonly JobComInvoiceLine invoiceLine;

	public string OriginCountry => invoiceLine.JI_CountryOfOrigin;

	public bool Preference => invoiceLine.JI_PrimaryPreference == UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;

	public bool PreferenceConfirmation => invoiceLine.JI_PrimaryPreference == UniversalReferenceConstants.PrimaryPreferenceCodes.NormalTariff && invoiceLine.HasOriginDocument;
}
