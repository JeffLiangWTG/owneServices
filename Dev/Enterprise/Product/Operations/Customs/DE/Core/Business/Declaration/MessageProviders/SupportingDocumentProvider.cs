using System;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.DE.Business
{
	public sealed class SupportingDocumentProvider : ISupportingDocument
	{
		public static SupportingDocumentProvider NewOrNull(SupportingDocument document) => document == null ? null : new SupportingDocumentProvider(document);

		SupportingDocumentProvider(SupportingDocument document)
		{
			this.document = document;
		}
		readonly SupportingDocument document;

		public string FullType => document.CSI_FullType;

		public string Type => document.CSI_FullType.Left(4);

		public string Qualifier => document.CSI_FullType.SubstringSafe(4, 3).ValueOrNullIfEmpty();

		public string ReferenceNumber => MapProperty(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Reference) ? (string)document.CSI_ReferenceNumber : null;

		public int DocumentLineItemNumber => MapProperty(UniversalReferenceConstants.RefCusCodeListAttributes.Name.ItemNumber) ? (int)document.CSI_ItemNumber : 0;

		public string Complement => MapProperty(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Complement) ? (string)document.CSI_Description : null;

		public string Detail => MapProperty(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Detail) ? (string)document.CSI_ReferenceNumber2 : null;

		public string IssuingAuthorityName => MapProperty(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Authority) ? (string)document.CSI_AdditionalDescription : null;

		public DateTime? IssuingDate => MapProperty(UniversalReferenceConstants.RefCusCodeListAttributes.Name.IssuingDate) ? document.CSI_DateOfIssue.ToNullableDateTime() : null;

		public DateTime? ValidityDate => MapProperty(UniversalReferenceConstants.RefCusCodeListAttributes.Name.ValidityDate) ? document.CSI_DateOfExpiry.ToNullableDateTime() : null;

		public string MeasurementUnitAndQualifier => MapProperty(UniversalReferenceConstants.RefCusCodeListAttributes.Name.MeasurementUnit) ? (string)document.CSI_UnitOfQuantity2 : null;

		public string ComplementaryUnit => MapProperty(UniversalReferenceConstants.RefCusCodeListAttributes.Name.ComplementaryUnit) ? (string)document.CSI_UnitOfQuantity : null;

		public decimal Quantity => document.CSI_Quantity.Normalize();

		public string Currency => MapProperty(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Value) ? (string)document.CSI_RX_NKCurrency : null;

		public decimal Amount => document.CSI_Value.FormatDecimal(2);

		bool MapProperty(string attributeName)
		{
			bool result;
			if (document.Declaration?.IsExport ?? false)
			{
				result = RefCusCode?.HasAttribute(attributeName) ?? false;
			}
			else
			{
				result = true;
			}
			return result;
		}

		ZZRefCusCodeListCombined RefCusCode => CachedValueHelper.GetValue(ref refCusCode, () => ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(document.Factory, document.CSI_FullType, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, CargoWise.Types.ZDateTime.Today));
		CachedValue<ZZRefCusCodeListCombined> refCusCode;
	}
}
