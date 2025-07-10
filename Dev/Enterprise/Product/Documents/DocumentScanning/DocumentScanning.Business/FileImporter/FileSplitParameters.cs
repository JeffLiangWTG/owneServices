using System.Collections.Generic;
using System.Linq;

namespace Enterprise.DocumentScanning.Business
{
	//[Immutable]
	public class FileSplitParameters
	{
		public string ReferenceType { get; }
		public string ReferenceCode { get; }
		public string DocumentType { get; }
		public string CompanyCode { get; }
		public int StartPageInclusive { get; }
		public int EndPageExclusive { get; }

		public BaseBarcode InitialBarcode { get; }
		public IEnumerable<string> ScannedBarcodes { get; }

		public bool UseBarcodeType { get; }

		public FileSplitParameters(string referenceType, string referenceCode, string documentType, string companyCode, int startAt, int endExcl, BaseBarcode initialBarcode, IEnumerable<string> scannedBarcodes, bool useBarecodeType)
		{
			ReferenceType = referenceType;
			ReferenceCode = referenceCode;
			DocumentType = documentType;
			CompanyCode = companyCode;
			StartPageInclusive = startAt;
			EndPageExclusive = endExcl;

			InitialBarcode = initialBarcode;
			ScannedBarcodes = scannedBarcodes.ToArray();
			UseBarcodeType = useBarecodeType;
		}
	}
}
