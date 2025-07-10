using System;
using CargoWise.Common;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Newtonsoft.Json;

namespace Enterprise.Accounting.ElectronicMessaging.TaxCore
{
	public interface ITaxCoreEInvoiceResponseReader
	{
		TaxCoreEInvoiceResponse Read(string base64Payload);
	}

	public class TaxCoreEInvoiceResponseReader : ITaxCoreEInvoiceResponseReader
	{
		TaxCoreEInvoiceResponse ITaxCoreEInvoiceResponseReader.Read(string base64Payload) => Read(base64Payload); // Not calling Stream.Read

		protected TaxCoreEInvoiceResponse Read(string base64Payload)
		{
			Argument.NotNullOrEmpty(base64Payload, nameof(base64Payload));
			var payload = base64Payload.ToUTF8FromBase64();

			TaxCoreEInvoiceResponse result;
			if (!TooLargeInvoiceResponseMessage.Equals(payload, StringComparison.OrdinalIgnoreCase))
			{
				try
				{
					result = JsonConvert.DeserializeObject<TaxCoreEInvoiceResponse>(payload);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					throw new DeserializationException(ex.Message, PayloadCaption, payload, ex);
				}
			}
			else
			{
				throw new DeserializationException(Res.GetString("a8f263b6-2457-4e38-83f9-b44ed9fc8e16", "E-Invoice is too large to process."), PayloadCaption, payload);
			}

			return result;
		}

		string PayloadCaption => Res.GetString("39848fbc-8a01-4522-829b-846220265066", "Received Response from govt.");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Error Message not displayed in user interface.")]
		const string TooLargeInvoiceResponseMessage = "The page was not displayed because the request entity is too large.";
	}
}
