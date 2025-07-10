using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public static class AESWrappersHelper
{
	public static ZString GetCurrency(JobComInvoiceHeader invoiceHeader) => invoiceHeader.JZ_RX_NKInvoice_Currency;

	const string Currency000 = "000";
	public static ZDecimal GetTotalAmount(JobComInvoiceHeader invoiceHeader, CusEntryHeader entryHeader)
	{
		var amount = ZDecimal.Zero;
		if (invoiceHeader.JZ_RX_NKInvoice_Currency != Currency000)
		{
			amount += entryHeader.InvoiceLines.Sum(line => line.JI_LinePrice);
		}
		return amount;
	}

	public static TransportMediumInfoCommonWrapper GetActiveBorderTransportMeans(JobDeclaration declaration)
	{
		var mot = declaration.ZG_BorderTransportMeans;
		var id = declaration.JE_VesselName.IsEmpty ? declaration.JE_VoyageFlightNo : declaration.JE_VesselName;
		var nationality = declaration.JE_RN_NKTransportNationality;

		return !declaration.JE_TransportMode.IsEmpty && (!mot.IsEmpty || !id.IsEmpty || !nationality.IsEmpty)
			? new TransportMediumInfoCommonWrapper(mot, id, nationality, declaration) : null;
	}

	public static ZString GetLineNumberForSupportingDocument(CusSupportingInfo document) => (document == null || document.CSI_ItemNumber.IsEmpty) ? string.Empty : document.CSI_ItemNumber.ToString();
}
