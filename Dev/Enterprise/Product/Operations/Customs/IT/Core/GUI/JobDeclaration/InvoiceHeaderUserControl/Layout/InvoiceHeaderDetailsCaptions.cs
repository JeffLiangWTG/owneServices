using System;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.IT.GUI;

sealed class InvoiceHeaderDetailsCaptions
{
	InvoiceHeaderDetailsCaptions()
	{
	}

	[ThreadStatic]
	static InvoiceHeaderDetailsCaptions instance;

	public static InvoiceHeaderDetailsCaptions Instance => instance ?? (instance = new InvoiceHeaderDetailsCaptions());

	internal readonly ResourceStringData IncoTermsCaption = Res.GetData("090E7C18-54C6-4EF6-AB87-3229EDF60E1A", "[20.1] INCO term");
	internal readonly ResourceStringData IncoTermPlaceCaption = Res.GetData("0EEF834B-8584-48CB-A809-947F09C55030", "[20.2] Place");
	internal readonly ResourceStringData IncotermPlaceCodeCaption = Res.GetData("1C3F1AFE-2EE4-4687-B946-9A53F315C5B1", "Inco. Place Code", "Inco. Place Code", "Incoterm Place Code", "Invoice Incoterm Place Code");
	internal readonly ResourceStringData AdditionalTermsCaption = Res.GetData("BD045857-DD7B-49FD-97E2-EA06EF6BBFF7", "Delivery Terms", "Delivery Terms", "Delivery Terms", "[14 01 000 000] Delivery Terms Text");
	internal readonly ResourceStringData AgreedPlaceCodeCaption = Res.GetData("EF07DBC5-2670-4D76-B528-3396DC5DEE64", "[20.3] Code");
	internal readonly ResourceStringData InvoiceAmountConvertToLocalCurrencyCaption = Res.GetData("E7838233-30DF-4A7C-848C-F2F9FADFA351", "[22] Inv. Amount");
	internal readonly ResourceStringData ValuationCodeCaption = Res.GetData("F16F37E2-F85D-4A13-B9E8-BD4F5F8C1C87", "[24] Tran. Nature");
	internal readonly ResourceStringData TransportChargesMethodOfPaymentCaption = Res.GetData("557A7CD9-CCB0-46F2-9CF9-F7CD1B4887D1", "Trans. Chrg. MoP");
	internal readonly ResourceStringData InvoiceCurrExRateCaption = Res.GetData("7527C2FF-0110-43F4-A154-89250CA1DF58", "Exchange Rate");
}
