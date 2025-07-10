using System.Collections.Immutable;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

public sealed class ExportTermsOfDeliveryWrapper
{
	public static ITermsOfDelivery NewOrNull(CusEntryHeader entryHeader, JobComInvoiceHeader invoiceHeader)
	{
		Argument.NotNull(entryHeader, nameof(entryHeader));
		Argument.NotNull(invoiceHeader, nameof(invoiceHeader));

		var subStyle = entryHeader.EntryInstruction?.CEI_SubStyle ?? ZString.Empty;
		if (subStyle.In(subStylesWhichDoesNotAllowTermsOfDelivery))
		{
			return null;
		}
		return TermsOfDeliveryWrapper.NewOrNull(invoiceHeader);
	}

	static readonly ImmutableArray<ZString> subStylesWhichDoesNotAllowTermsOfDelivery = new ZString[]
	{
		ITEntrySubStyleList.Codes.SimplifiedDeclarationOccasionallyB,
		ITEntrySubStyleList.Codes.SimplifiedDeclarationRegularlyC,
		ITEntrySubStyleList.Codes.PreliminarySimplifiedDeclarationE,
		ITEntrySubStyleList.Codes.PreliminarySimplifiedDeclarationF,
	}.ToImmutableArray();
}
