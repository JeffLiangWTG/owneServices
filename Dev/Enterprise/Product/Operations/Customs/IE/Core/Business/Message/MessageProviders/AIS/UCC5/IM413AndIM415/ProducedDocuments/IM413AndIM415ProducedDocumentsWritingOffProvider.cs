using System;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM413AndIM415ProducedDocumentsWritingOffProvider : IGoodsShipmentItemTypeProducedDocumentsWritingOff
	{
		public static IM413AndIM415ProducedDocumentsWritingOffProvider New(SupportingDocument supportingDocument)
		{
			IM413AndIM415ProducedDocumentsWritingOffProvider result = null;
			if (supportingDocument != null)
			{
				result = new IM413AndIM415ProducedDocumentsWritingOffProvider(supportingDocument);
			}
			return result;
		}

		IM413AndIM415ProducedDocumentsWritingOffProvider(SupportingDocument supportingDocument)
		{
			this.supportingDocument = supportingDocument;
		}
		readonly SupportingDocument supportingDocument;

		public string Id => supportingDocument.CSI_ReferenceNumber;

		public string Type => supportingDocument.CSI_Code;

		public string IssuingAuthorityNameSubmitter => supportingDocument.CSI_AdditionalDescription;

		public string IssuingAuthorityNameRoleCode => supportingDocument.CSI_ReferenceNumber2;

		public DateTime? DateOfValidity => supportingDocument.CSI_DateOfExpiry.IsValid ? DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(supportingDocument.CSI_DateOfExpiry, removeMillisecond: true) : null;

		public string MeasurementUnit => supportingDocument.CSI_UnitOfQuantity;

		public decimal Quantity => supportingDocument.CSI_Quantity;

		public string Currency => supportingDocument.CSI_RX_NKCurrency;

		public decimal Amount => supportingDocument.CSI_Value;
	}
}
