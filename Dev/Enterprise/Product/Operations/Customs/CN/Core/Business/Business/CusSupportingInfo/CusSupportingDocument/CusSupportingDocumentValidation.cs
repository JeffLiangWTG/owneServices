using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public class CusSupportingDocumentValidation : Customs.Business.CusSupportingInfoValidation
	{
		public CusSupportingDocumentValidation(CusSupportingDocument parent) : base(parent)
		{
		}

		public new CusSupportingDocument Parent => (CusSupportingDocument)base.Parent;

		internal IValidationModeProvider ValidationModeProvider => Parent.Parent?.Declaration;

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();

			if (!Parent.IsCertificateOfOrigin)
			{
				var sourceValue = Parent.CSI_Code;
				var targetInfo = Parent.CSI_CodeInfo;
				MandatoryValidation.CheckEntered(targetInfo);
				MandatoryValidation.CheckEntered(Parent.DocumentTypeInfo);

				if (!sourceValue.IsEmpty)
				{
					if (Parent.Parent != null)
					{
						if (Parent.Parent.CusSupportingDocuments.Cast<CusSupportingDocument>().Any(x => x != Parent && sourceValue == x.CSI_Code))
						{
							targetInfo.AddNotification(Res.GetString("80E99255-50D4-4225-BD4C-1E89B9612C4E", "The Document Type has been duplicated and must be unique."), ValidationModeProvider);
						}

						CheckCusSupportingDocumentsAcrossInstruction(Parent);
					}
				}
			}

			var declaration = Parent.Parent?.Declaration;
			if (declaration != null && declaration.IsTwoStepDeclaration)
			{
				var documentCodesSupportesTSD = CNRefCusCodeListTypes.GetSupportingDocumentsSupportsTSD(Parent.Factory, declaration.DateOfValuation);
				if (!documentCodesSupportesTSD.ContainsCode(Parent.CSI_Code))
				{
					Parent.CSI_CodeInfo.AddNotification(Res.GetString("AC2901B6-E598-4094-8660-02A2187E9A53", "This Document Type is not supported in two-step declaration clearance mode."), declaration);
				}
			}
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			if (!Parent.IsCertificateOfOrigin)
			{
				ValidationHelper.CheckMaxLength(Parent.CSI_ReferenceNumberInfo, Parent.CSI_ReferenceNumberInfo.MaxLength, ValidationModeProvider);
				Parent.CSI_ReferenceNumberInfo.AddNotificationIfNotEntered(ValidationModeProvider);
				if (!Parent.CSI_ReferenceNumber.IsEmpty)
				{
					var regesMather = new Regex("^(?=.*?[a-zA-Z0-9]).{1,}$", RegexOptions.Compiled);
					if (!regesMather.IsMatch(Parent.CSI_ReferenceNumber))
					{
						Parent.CSI_ReferenceNumberInfo.AddNotification(Res.GetString("68CD9914-1D22-472B-B017-E088C7D17B48", "{0} should contain at least one alphanumeric character.", Parent.CSI_ReferenceNumberInfo.HumanReadableName), ValidationModeProvider);
					}
				}
				ValidateCSI_Code();
			}
		}

		protected override void CheckCSI_LineNo()
		{
			base.CheckCSI_LineNo();
			if (!Parent.IsCertificateOfOrigin && Parent.RequiresLineNumber.EqualsIgnoringCase(Constants.UniversalReferenceConstants.RequiresLineNumberAttributeValue.Mandatory))
			{
				Parent.CSI_LineNoInfo.AddNotificationIfIsZero(ValidationModeProvider);
			}
		}

		#region public static checks

		public static void CheckCusSupportingDocumentsAcrossInstruction(CusSupportingDocument document, ZPropertyInfo targetInfoOnJobBo = null)
		{
			if (document?.Parent is JobComInvoiceLine invoiceLine && !document.CSI_ReferenceNumber.IsEmpty)
			{
				var targetInfo = targetInfoOnJobBo ?? document.CSI_CodeInfo;
				var allDocumentsUnderEntryInstruction = invoiceLine?.EntryInstruction?.CusSupportingDocuments;
				if (allDocumentsUnderEntryInstruction != null)
				{
					if (allDocumentsUnderEntryInstruction.Any(x => x.PK != document.PK && x.IsDifferentDocumentWithTheSameType(document)))
					{
						var message = document.IsCertificateOfOrigin
							? Res.GetString("aec82fcd-c2db-48c4-a6de-2f3ba1cc48d5", "Another Certificate of Origin with a different Number or Type has already been specified for the selected Entry Instruction.\nPlease select a different Entry Instruction for this Invoice Line if it is a different document.")
							: Res.GetString("81995D04-DDE5-4B6F-9725-821D4F9D9D0D", "This Document Type with a different Number has already been specified for the selected Entry Instruction.\nPlease select a different Entry Instruction for this Invoice Line if it is a different document.");
						targetInfo.AddNotification(message, invoiceLine);
					}

					if (document.IsLicense && allDocumentsUnderEntryInstruction.Any(x => x.PK != document.PK && x.IsLicense && !x.IsTheSameDocument(document)))
					{
						targetInfo.AddNotification(Res.GetString("737CF1E9-31B6-4EDE-8E84-C0B9B33C574B", "Only one License is allowed."), invoiceLine);
					}
				}
			}
		}

		public static void CheckCertificateOfOriginIsRequired(ZPropertyInfo propertyInfo, ZString documentNumber, bool isRequired, JobComInvoiceLine invoiceLine)
		{
			if (isRequired && documentNumber.IsEmpty && invoiceLine != null)
			{
				propertyInfo.AddNotification(Res.GetString("FC335E2D-21BE-495C-A28B-F3D6FE7F4342", "Certificate of Origin is required in order to use the selected preferential duty rate."), invoiceLine);
			}
		}

		public static void CheckCertificateOfOriginNumber(ZPropertyInfo propertyInfo, JobComInvoiceLine invoiceLine)
		{
			var sourceValue = (ZString)propertyInfo.Value;
			if (!sourceValue.IsEmpty && invoiceLine != null)
			{
				if (sourceValue.StartsWith("<", StringComparison.Ordinal))
				{
					propertyInfo.AddNotification(Res.GetString("A52B0401-A05A-4D59-BF84-5AAB30301CAE", "Certificate of Origin does not need to start with the Preferential Code, please select a Preferential Code on this Invoice Line."), invoiceLine);
				}
				else if (sourceValue.KeepAlphanumericCharacters() != sourceValue)
				{
					propertyInfo.AddNotification(Res.GetString("026C757D-F874-48FB-B152-B2E07A48431E", "Certificate of Origin number should be alphanumeric only."), invoiceLine);
				}
			}
		}

		#endregion
	}
}
