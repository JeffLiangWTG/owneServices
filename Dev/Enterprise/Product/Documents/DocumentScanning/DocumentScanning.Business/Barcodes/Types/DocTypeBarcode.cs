using CargoWise.Types;

namespace Enterprise.DocumentScanning.Business
{
	/// <summary>
	/// Decodes the string in a DocumentType barcode into its constituent parts -
	/// DocType, and RefType and/or RefCode if supplied
	/// </summary>
	public class DocTypeBarcode : BaseBarcode
	{
		public DocTypeBarcode(DocumentFactory masterFactory, ZString fullBarcodeText)
			: base(masterFactory, fullBarcodeText)
		{
			if (BarcodeHelper.IsDocTypeAndRefTypeBarcode(fullBarcodeText))
			{
				DocManagerCode = BarcodeHelper.EncodedRefType(fullBarcodeText);
				RefCode = BarcodeHelper.GetValueForCode(fullBarcodeText, DocManagerCode);

				DocType = SubstringSafe(fullBarcodeText, ';', 3);
				CompanyCode = BarcodeHelper.IsCompanySpecificBarcode(fullBarcodeText) ? SubstringSafe(fullBarcodeText, '@', 3) : ZString.Empty;
			}
			else if (BarcodeHelper.IsDocTypeOnlyBarcode(fullBarcodeText))
			{
				DocType = BarcodeHelper.GetValueForCode(fullBarcodeText, Core.Constants.FileFormats.DOC).Left(AutoStorageDocs.Schema.SC_DocTypeMaxLength);
			}
			else if (BarcodeHelper.IsRefTypeOnlyBarcode(fullBarcodeText))
			{
				DocManagerCode = BarcodeHelper.EncodedRefType(fullBarcodeText);
				RefCode = BarcodeHelper.GetValueForCode(fullBarcodeText, DocManagerCode);
			}
		}

		ZString SubstringSafe(ZString source, char startingSymbol, int length)
		{
			int index = source.IndexOf(startingSymbol);
			return index == -1 || index + length + 1 >= source.Length ? ZString.Empty : source.Substring(index + 1, length);
		}
	}
}
