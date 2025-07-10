using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Business.MasterFiles;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ExportPreviousDocumentValidation : PreviousDocumentValidation
	{
		public ExportPreviousDocumentValidation(PreviousDocument parent) : base(parent)
		{
		}

		protected override void CheckCSI_CodeCore()
		{
			var parent = Parent;
			var propertyInfo = parent.CSI_CodeInfo;
			ListValidation.MessageErrorIfInvalidCodeCaseSensitiveOrEmpty(propertyInfo);

			var code = parent.CSI_Code;
			if (parent.IsInExportInvoiceLinePreviousDocuments)
			{
				var invoiceLine = parent.Parent as JobComInvoiceLine;
				var invoiceLinePreviousDocuments = invoiceLine.PreviousDocuments.Cast<PreviousDocument>();
				var cei_SubStyle = invoiceLine.EntryInstruction?.CEI_SubStyle ?? ZString.Empty;

				switch (code)
				{
					case UniversalReferenceConstants.RefCusCodeList.Codes.Code_C651:
					case UniversalReferenceConstants.RefCusCodeList.Codes.Code_C658:
						ValidateCSI_CodeNoDuplicatesOfType(invoiceLinePreviousDocuments);
						break;
					case UniversalReferenceConstants.SupportingDocumentTypes.N955:
						ValidateCSI_CodeN955(cei_SubStyle);
						break;
					case UniversalReferenceConstants.SupportingDocumentTypes.N830:
						ValidateCSI_CodeN830(cei_SubStyle);
						break;
				}

				ValidateCSI_CodeMutuallyExclusive(invoiceLinePreviousDocuments);
			}
			else if (parent.IsInExportCusClassPartPivotPreviousDocuments)
			{
				var cusClassPartPivot = parent.Parent as CusClassPartPivot;
				var cusClassPartPivotPreviousDocuments = cusClassPartPivot.PreviousDocuments.Cast<PreviousDocument>();

				switch (code)
				{
					case UniversalReferenceConstants.RefCusCodeList.Codes.Code_C651:
					case UniversalReferenceConstants.RefCusCodeList.Codes.Code_C658:
						ValidateCSI_CodeNoDuplicatesOfType(cusClassPartPivotPreviousDocuments);
						break;
				}

				ValidateCSI_CodeMutuallyExclusive(cusClassPartPivotPreviousDocuments);
			}

			void ValidateCSI_CodeNoDuplicatesOfType(IEnumerable<PreviousDocument> previousDocuments)
			{
				if (previousDocuments.Any(x => x.PK != parent.PK && x.CSI_Code == code))
				{
					propertyInfo.AddMessageError(Res.GetString("875df19b-d31f-4c4b-a4fc-454a3e50df41", "A Previous Document of Type {0} has already been entered.", code));
				}
			}

			void ValidateCSI_CodeN955(ZString subStyle)
			{
				if (subStyle == ExportDeclarationTypeTimeList.Codes._13)
				{
					propertyInfo.AddMessageError(Res.GetString("986BA6A8-E2BE-418C-A01D-E624B4B3A437", "For the selected Type (Time + Procedure) you may not enter the Previous Document Type N955."));
				}
			}

			void ValidateCSI_CodeN830(ZString subStyle)
			{
				if (subStyle == ExportDeclarationTypeTimeList.Codes._11 || subStyle == ExportDeclarationTypeTimeList.Codes._12)
				{
					propertyInfo.AddMessageError(Res.GetString("63215BB8-3E8A-486E-BC43-2CA2AEA9274B", "For the selected Type (Time + Procedure) you may not enter the Previous Document Type N830."));
				}
			}

			void ValidateCSI_CodeMutuallyExclusive(IEnumerable<PreviousDocument> previousDocuments)
			{
				AddMessageErrorIfHasMutuallyExclusive(new ZString[] { UniversalReferenceConstants.RefCusCodeList.Codes.Code_9ZZX, UniversalReferenceConstants.RefCusCodeList.Codes.Code_9ZZY }, Res.GetString("d8b6224f-9d95-4d41-bef8-bb0c37d5ddd5", "The Previous Document Types 9ZZX and 9ZZY are mutually exclusive."));

				AddMessageErrorIfHasMutuallyExclusive(new ZString[] { UniversalReferenceConstants.RefCusCodeList.Codes.Code_C651, UniversalReferenceConstants.RefCusCodeList.Codes.Code_C658 }, Res.GetString("831ed3e6-f0cc-4c97-8fb0-f01327a35ab0", "The Previous Document Types C651 and C658 are mutually exclusive."));

				void AddMessageErrorIfHasMutuallyExclusive(ZString[] exclusiveSet, string messageError)
				{
					if (exclusiveSet.Contains(code) &&
						previousDocuments.Any(i => i.CSI_Code != code && exclusiveSet.Contains(i.CSI_Code)))
					{
						propertyInfo.AddMessageError(messageError);
					}
				}
			}
		}

		protected override void CheckCSI_ReferenceNumberCore()
		{
			base.CheckCSI_ReferenceNumberCore();
			var parent = Parent;
			var propertyInfo = parent.CSI_ReferenceNumberInfo;

			if (parent.CodeCusCodeList?.HasAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Reference, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes) ?? false)
			{
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
			}

			if (parent.IsInExportPreviousDocuments)
			{
				var referenceNumber = parent.CSI_ReferenceNumber;
				var code = parent.CSI_Code;
				if (!referenceNumber.IsEmpty)
				{
					if (parent.IsInExportInvoiceLineOrCusClassPartPivotPreviousDocuments && code == UniversalReferenceConstants.RefCusCodeList.Codes.Code_C651 && referenceNumber.Length != 21)
					{
						propertyInfo.AddMessageError(Res.GetString("5b24889f-0d03-42f1-8c35-f849ab6a2519", "The C651 Reference must have 21 characters."));
					}
					else if (code == UniversalReferenceConstants.SupportingDocumentTypes.N830)
					{
						var mrnError = MRNFormatValidator.CheckMRNFormat(referenceNumber, parent.Factory, ZString.Empty);
						if (!mrnError.IsEmpty)
						{
							propertyInfo.AddMessageError(mrnError);
						}
					}
				}
			}
		}

		protected override void CheckCSI_ItemNumber()
		{
			base.CheckCSI_ItemNumber();

			var parent = Parent;
			if (parent.IsInExportInvoiceLineOrCusClassPartPivotPreviousDocuments)
			{
				if (parent.CodeCusCodeList?.HasAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.ItemNumber, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes) ?? false)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_ItemNumberInfo);
				}
			}
		}

		protected override void CheckCSI_UnitOfQuantityCore()
		{
			base.CheckCSI_UnitOfQuantityCore();

			var parent = Parent;
			var propertyInfo = parent.CSI_UnitOfQuantityInfo;
			if (parent.IsInExportInvoiceLineOrCusClassPartPivotPreviousDocuments)
			{
				if (parent.CodeCusCodeList?.HasAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.MeasurementUnit, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes) ?? false)
				{
					MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
				}

				ListValidation.MessageErrorIfInvalidCode(propertyInfo);
			}
		}

		protected override void CheckCSI_QuantityCore()
		{
			var parent = Parent;
			var quantity = parent.CSI_Quantity;
			var propertyInfo = parent.CSI_QuantityInfo;
			if (parent.IsInExportInvoiceLineOrCusClassPartPivotPreviousDocuments)
			{
				var codeCusCodeList = parent.CodeCusCodeList;
				if (codeCusCodeList != null)
				{
					var mandatory = codeCusCodeList.HasAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.MeasurementUnit, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);
					if (!mandatory)
					{
						mandatory = !parent.CSI_UnitOfQuantity.IsEmpty && codeCusCodeList.HasAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.MeasurementUnit, UniversalReferenceConstants.RefCusCodeListAttributes.Value.No);
					}
					if (mandatory && quantity.IsEmpty)
					{
						propertyInfo.AddMessageError(Res.GetString("AC89F834-5931-41CC-AEAE-3AA2C55A1B19", "Please enter a Quantity between 0.00001 and 99999999999.99999."));
					}
				}

				if (!quantity.IsInteger && parent.Factory.IsIntegerRequiredUnitOfQuantity(parent.CSI_UnitOfQuantity))
				{
					propertyInfo.AddMessageError(Res.GetString("8EF73ACF-3109-4D75-BD3A-29EA4CE2D0F1", "Quantity should be an integer value."));
				}
			}
		}

		protected override void CheckCSI_QuantityIsValidZDecimal()
		{
			if (Parent.IsInExportInvoiceLineOrCusClassPartPivotPreviousDocuments)
			{
				TypeValidation.CheckValidDecimal(Parent.CSI_QuantityInfo, 16, 5);
			}
			else
			{
				base.CheckCSI_QuantityIsValidZDecimal();
			}
		}

		protected override void CheckCSI_DescriptionCore()
		{
			base.CheckCSI_DescriptionCore();
			var parent = Parent;
			if (parent.IsInExportInvoiceLineOrCusClassPartPivotPreviousDocuments && parent.CodeCusCodeList.HasAttributeForMandatoryValidation(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Complement))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_DescriptionInfo);
			}
		}

		protected override bool IsAvailable(string propertyName)
		{
			bool result = propertyName == PreviousDocument.Schema.CSI_Code || propertyName == Customs.Business.AutoCusSupportingInfo.Schema.CSI_ReferenceNumber;
			if (!result && Parent.IsInExportInvoiceLineOrCusClassPartPivotPreviousDocuments)
			{
				result = propertyName == Customs.Business.AutoCusSupportingInfo.Schema.CSI_Quantity || propertyName == Customs.Business.AutoCusSupportingInfo.Schema.CSI_UnitOfQuantity || propertyName == Customs.Business.AutoCusSupportingInfo.Schema.CSI_Description;
			}
			return result;
		}
	}
}
