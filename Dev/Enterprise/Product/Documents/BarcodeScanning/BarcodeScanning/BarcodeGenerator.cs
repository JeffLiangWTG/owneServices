using CargoWise.Types;

namespace Enterprise.Barcode.Business
{
	/// <summary>
	/// Summary description for BarcodeGenerator.
	/// </summary>
	public class BarcodeGenerator
	{
		public TextBarcode CreateDocumentBarcode(ZString refType)
		{
			return CreateDocumentBarcode(refType, ZString.Empty, ZString.Empty);
		}

		/// <summary>
		/// Creates a bitmap barcode in 128 B encoding
		/// </summary>
		/// <param name="refType">3-letter Reference Type (e.g. SHP)</param>
		/// <param name="refValue">Code of the BusinessObject (e.g. S00001000)</param>
		public TextBarcode CreateDocumentBarcode(ZString refType, ZString refValue)
		{
			return CreateDocumentBarcode(refType, refValue, ZString.Empty);
		}

		/// <summary>
		/// Creates a bitmap barcode in 128 B encoding
		/// </summary>
		/// <param name="refType">3-letter Reference Type (e.g. SHP)</param>
		/// <param name="refValue">Code of the BusinessObject (e.g. S00001000)</param>
		/// <param name="docType">3-letter Document Type (e.g. "MSC")</param>
		public TextBarcode CreateDocumentBarcode(ZString refType, ZString refValue, ZString docType)
		{
			return CreateDocumentBarcode(refType, refValue, docType, ZString.Empty);
		}

		/// <summary>
		/// Creates a bitmap barcode in 128 B encoding
		/// </summary>
		/// <param name="refType">3-letter Reference Type (e.g. SHP)</param>
		/// <param name="refValue">Code of the BusinessObject (e.g. S00001000)</param>
		/// <param name="docType">3-letter Document Type (e.g. "MSC")</param>
		/// <param name="companyCode">3-letter Company Code (e.g. "EDI")</param>
		public TextBarcode CreateDocumentBarcode(ZString refType, ZString refValue, ZString docType, ZString companyCode)
		{
			if (refType == (ZString)null)
			{
				refType = ZString.Empty;
			}

			if (refValue == (ZString)null)
			{
				refValue = ZString.Empty;
			}

			if (docType == (ZString)null)
			{
				docType = ZString.Empty;
			}

			if (companyCode == (ZString)null)
			{
				companyCode = ZString.Empty;
			}

			if (refType.IsEmpty && refValue.IsEmpty && !docType.IsEmpty)
			{
				return CreateDocTypeBarcode(docType);
			}
			else if (!refType.IsEmpty && !refValue.IsEmpty)
			{
				string barcodeText = "^" + refType + "=" + refValue + ((docType.IsEmpty) ? "" : ";" + docType) + ((companyCode.IsEmpty) ? "" : "@" + companyCode) + "|";
				return new TextBarcode(barcodeText, true);
			}
			else
			{
				return new TextBarcode(ZString.Empty);
			}
		}

		/// <summary>
		/// Creates a shipment barcode [Company DocType Origin Destination Housebill]
		/// </summary>
		/// <param name="companyCode">3-letter company code</param>
		/// <param name="docType">3-letter DocType</param>
		/// <param name="origin">3-letter IATA code</param>
		/// <param name="destination">3-letter IATA code</param>
		/// <param name="houseBill">Housebill number. Cannot be empty.</param>
		public TextBarcode CreateShipmentBarcode(ZString companyCode, ZString docType, ZString origin, ZString destination, ZString houseBill)
		{
			if (companyCode.Length == 3 && docType.Length == 3 && origin.Length == 3 && destination.Length == 3 && !houseBill.IsEmpty)
			{
				string barcodeText = "[" + companyCode + docType + origin + destination + houseBill + "]";
				return new TextBarcode(barcodeText, true);
			}
			return new TextBarcode(ZString.Empty);
		}

		/// <summary>
		/// Create a barcode for a Document type e.g. ^DOC=INV|
		/// </summary>
		/// <param name="docType">3-letter document type</param>
		/// <param name="MaxWidthInPixels">Maximum width of the barcode (Min. 300 or No. of chars * 11 + 75)</param>
		/// <param name="MaxHeightInPixels">Maximum height of the barcode (Min. 80)</param>
		public TextBarcode CreateDocTypeBarcode(ZString docType)
		{
			if (!docType.IsEmpty)
			{
				return new TextBarcode("^DOC=" + docType + "|", true); // Hard-coded argument
			}
			else
			{
				return new TextBarcode(ZString.Empty);
			}
		}
	}
}
