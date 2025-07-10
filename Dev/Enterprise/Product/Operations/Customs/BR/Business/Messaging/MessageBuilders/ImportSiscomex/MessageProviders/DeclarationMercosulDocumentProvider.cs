using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.ImportSiscomex.Outgoing;

namespace Enterprise.Customs.BR.Business.ImportSiscomex
{
	public class DeclarationMercosulDocumentProvider : IDeclarationMercosulDocument
	{
		public DeclarationMercosulDocumentProvider(MercosulForeignDeclaration mercosulDocument)
		{
			this.mercosulDocument = Argument.NotNull(mercosulDocument, nameof(mercosulDocument));
		}

		readonly MercosulForeignDeclaration mercosulDocument;

		public static DeclarationMercosulDocumentProvider New(MercosulForeignDeclaration mercosulDocument) => mercosulDocument == null ? null : new DeclarationMercosulDocumentProvider(mercosulDocument);

		public string ReferenceType => mercosulDocument.CSI_SubType;

		public string ReferenceNumber => mercosulDocument.CSI_Description;

		public string CertificateNumber => mercosulDocument.CSI_Code;

		public string CountryCode => mercosulDocument.CSI_RN_NKCountryCode;

		public string QtyStart => mercosulDocument.CSI_ReferenceNumber;

		public string QtyEnd => mercosulDocument.CSI_ReferenceNumber2;

		public int ItemNumber => mercosulDocument.CSI_ItemNumber;

		public decimal QtyUnitStatistic => mercosulDocument.CSI_Quantity3;

		public override bool Equals(object obj)
		{
			var result = false;
			if (obj is DeclarationMercosulDocumentProvider other && other != null)
			{
				result = other.GetType() == GetType()
					&& other.ReferenceType == ReferenceType
					&& other.ReferenceNumber == ReferenceNumber
					&& other.CertificateNumber == CertificateNumber
					&& other.CountryCode == CountryCode
					&& other.QtyStart == QtyStart
					&& other.QtyEnd == QtyEnd
					&& other.ItemNumber == ItemNumber
					&& other.QtyUnitStatistic == QtyUnitStatistic;
			}
			return result;
		}

		public override int GetHashCode()
		{
			return ReferenceType.GetHashCode()
				^ ReferenceNumber.GetHashCode()
				^ CertificateNumber.GetHashCode()
				^ CountryCode.GetHashCode()
				^ QtyStart.GetHashCode()
				^ QtyEnd.GetHashCode()
				^ ItemNumber.GetHashCode()
				^ QtyUnitStatistic.GetHashCode();
		}
	}
}

