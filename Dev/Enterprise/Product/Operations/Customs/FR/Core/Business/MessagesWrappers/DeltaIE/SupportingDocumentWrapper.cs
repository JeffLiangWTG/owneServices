using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class SupportingDocumentWrapper : ISupportingDocument
	{
		SupportingDocumentWrapper(CusSupportingInfo supportingInfo, ZString customsOffice)
		{
			this.supportingInfo = Argument.NotNull(supportingInfo, nameof(supportingInfo));
			this.customsOffice = customsOffice;
		}

		readonly CusSupportingInfo supportingInfo;
		readonly ZString customsOffice;

		public static SupportingDocumentWrapper New(CusSupportingInfo document, ZString customsOffice) => document == null ? null : new SupportingDocumentWrapper(document, customsOffice);

		public string DateOfValidity => dateOfValidity ?? (dateOfValidity = supportingInfo.CSI_DateOfExpiry.ToString("yyyy-MM-dd"));
		string dateOfValidity;

		public string DocumentLineItemNumber => documentLineItemNumber ?? (documentLineItemNumber = supportingInfo.CSI_LineNo.ToString());
		string documentLineItemNumber;

		public string IssuingAuthorityName => issuingAuthorityName ?? (issuingAuthorityName = supportingInfo.CSI_AdditionalDescription);
		string issuingAuthorityName;

		public string CcQualifier => ccQualifier ?? (ccQualifier = customsOffice.StartsWith(Core.Constants.CountryCodes.France) ? ZString.Empty : Core.Constants.CountryCodes.France);
		string ccQualifier;

		public string ReferenceNumber => referenceNumber ?? (referenceNumber = supportingInfo.CSI_ReferenceNumber);
		string referenceNumber;

		public string Type => type ?? (type = supportingInfo.CSI_Code);
		string type;

		public double Amount => amount == 0d ? (amount = (double)supportingInfo.CSI_Value) : 0d;
		double amount;

		public string Currency => currency ?? (currency = supportingInfo.CSI_RX_NKCurrency);
		string currency;

		public double Quantity => quantity == 0d ? (quantity = (double)supportingInfo.CSI_Quantity) : 0d;
		double quantity;

		public string MeasurementUnitAndQualifier => measurementUnitAndQualifier ?? (measurementUnitAndQualifier = supportingInfo.CSI_UnitOfQuantity);
		string measurementUnitAndQualifier;

		public string ComplementOfInformation => complementOfInformation ?? (complementOfInformation = supportingInfo.CSI_Description);
		string complementOfInformation;
	}
}
