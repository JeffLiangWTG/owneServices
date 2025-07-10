using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class CommonDeliveryTermsWrapper : ICommonDeliveryTerms
{
	public CommonDeliveryTermsWrapper(CusEntryHeader entryHeader)
	{
		Argument.NotNull(entryHeader, nameof(entryHeader));
		declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
		invoiceHeader = entryHeader.RandomHeader;
	}
	readonly JobDeclaration declaration;
	readonly JobComInvoiceHeader invoiceHeader;

	public ZString Incoterm => GetIncoTermCode();

	public ZString UNLCode => GetIncoTermCode() != "XXX" ? GetUnlocode() : ZString.Empty;

	public ZString IncotermLocation => GetIncoTermCode() != "XXX" && GetUnlocode().IsEmpty ? GetIncoTermPlace() : ZString.Empty;

	public ZString DeliveryCountry => GetIncoTermCode() != "XXX" ? CommonWrappersHelper.GetIncotermPlaceCodeCountry(invoiceHeader, declaration) : ZString.Empty;

	public ZString DeliveryText => GetIncoTermCode() == "XXX" ? GetIncoTermPlace() : ZString.Empty;

	ZString GetIncoTermCode() => invoiceHeader.JZ_IncoTerm.IsEmpty ? declaration.JE_ShipmentIncoTerm : invoiceHeader.JZ_IncoTerm;

	ZString GetIncoTermPlace() => invoiceHeader.JZ_IncoTermPlace.IsEmpty ? declaration.JE_ShipmentIncoTermPlace : invoiceHeader.JZ_IncoTermPlace;

	ZString GetUnlocode() => CommonWrappersHelper.GetIncotermPlaceCode(invoiceHeader, declaration);
}
