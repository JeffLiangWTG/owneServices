using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Messaging.MessageBuilders;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class SupportingDocumentValidation : EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentValidation
	{
		public SupportingDocumentValidation(SupportingDocument parent)
			: base(parent)
		{
		}

		protected new SupportingDocument Parent => (SupportingDocument)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			CheckMixedStatusEntriesAndMultipleEntryInstructions();
		}

		protected override void CheckCSI_UnitOfQuantity()
		{
			base.CheckCSI_UnitOfQuantity();
			if (Parent.ShowDropEditForUnitOfQuantity)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CSI_UnitOfQuantityInfo);
			}
		}

		protected override void CheckDuplicatedDocuments(ZPropertyInfo info)
		{
			if (!Parent.IsCodeAPermitType)
			{
				base.CheckDuplicatedDocuments(info);
			}
		}

		protected override void CheckCSI_DateOfIssue()
		{
			base.CheckCSI_DateOfIssue();
			var parent = Parent;
			var declaration = parent.Declaration;
			var codeIs1003AndDeferTypeIsL = declaration != null && parent.CSI_Code == VATDeferStrategyCodeList.Codes.VisaFreeAi2VatAndDutiesAdditionalCode && declaration.ZG_VATDeferType == VATProcedureList.Codes.L;
			var codeIs1003AndDeferTypeIsNotL = declaration != null && parent.CSI_Code == VATDeferStrategyCodeList.Codes.VisaFreeAi2VatAndDutiesAdditionalCode && declaration.ZG_VATDeferType != VATProcedureList.Codes.L;
			var codeIsVATNumberRelated = declaration != null && declaration.VATNumberSupporter.CodeIsVATNumberRelated(parent);

			if (parent.CSI_DateOfIssue.IsEmpty)
			{
				if (codeIsVATNumberRelated)
				{
					parent.CSI_DateOfIssueInfo.AddMessageError(Res.GetString("A85F6C69-06DB-4AC4-A4E4-0C49DC15646E", "The date should be declaration submission date."));
				}
				else if (!parent.CSI_IsDTP || codeIs1003AndDeferTypeIsL)
				{
					parent.CSI_DateOfIssueInfo.AddMessageError(MessageBuilderHelper.MessageSupportingDocNeedDate(parent.CSI_ReferenceNumber));
				}
			}

			if (codeIs1003AndDeferTypeIsL)
			{
				if (parent.CSI_DateOfIssue > declaration.DateOfValuation)
				{
					parent.CSI_DateOfIssueInfo.AddMessageError(Res.GetString("00CC799F-1170-489B-B664-0B11ADD2103A", "This type of VAT procedure is not valid yet, starting date is {0:dd/MM/yyyy}.", parent.CSI_DateOfIssue));
				}
			}

			if (codeIs1003AndDeferTypeIsNotL)
			{
				MandatoryValidation.MessageErrorIfIsEntered(parent.CSI_DateOfIssueInfo);
			}

			if (!parent.CSI_DateOfIssue.IsEmpty && parent.CSI_DateOfIssue < ZDateTime.Today.AddYears(-25))
			{
				parent.CSI_DateOfIssueInfo.AddMessageError(Res.GetString("BFC1CBBC-7F46-4E1D-AD3D-DAAA28029DC1", "The date '{0}' is more than 25 years old and thus is not valid.", parent.CSI_DateOfIssue));
			}
		}

		protected override void CheckCSI_DateOfIssueIsValidZDateTimeRange()
		{
			TypeValidation.CheckValidZDateTimeRange(Parent.CSI_DateOfIssueInfo, new TypeValidationLimits() { PastYearsBeforeError = DateRangeValidation.MaximumPastYears });
		}

		protected override void CheckCSI_Quantity3()
		{
			base.CheckCSI_Quantity3();
			CompareValidation.CheckNumberNotNegative(Parent.CSI_Quantity3Info);

			var parent = Parent;
			if (parent.CSI_Quantity3 > 0 && !parent.IsD48)
			{
				Parent.CSI_Quantity3Info.AddMessageError(Res.GetString("2096A7F8-84E5-40C7-B575-34FF3CDDCD5B", "D48 Duration in Months is only available for D48 document.", parent.CSI_Quantity3));
			}
		}

		protected override void CheckUnitAndCurrencyWithOtherDocuments()
		{
			if (Parent.IsCodeAPermitType)
			{
				Parent.RemoveRowMessageError(InconsistentUnitOrCurrency);
			}
			else
			{
				base.CheckUnitAndCurrencyWithOtherDocuments();
			}
		}

		protected void CheckMixedStatusEntriesAndMultipleEntryInstructions()
		{
			if (Parent.Parent is JobDeclaration declaration)
			{
				if (declaration.IsG2WithMixedStatusEntries)
				{
					Parent.AddRowMessageError(MixedStatusMessage);
				}
				else
				{
					Parent.RemoveRowMessageError(MixedStatusMessage);
				}
			}

			if (Parent.Parent is JobComInvoiceHeader header)
			{
				if (header.IsInvoiceUsedOverMultipleEntryInstructions)
				{
					Parent.AddRowMessageError(MultipleInstructionsMessage);
				}
				else
				{
					Parent.RemoveRowMessageError(MultipleInstructionsMessage);
				}
			}
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			var parent = Parent;
			var referenceNumber = parent.CSI_ReferenceNumber;
			var info = parent.CSI_ReferenceNumberInfo;
			if (!parent.CSI_IsDTP && referenceNumber.IsEmpty)
			{
				var declaration = parent.Declaration;
				if (declaration != null)
				{
					if (declaration.IsDeltaC)
					{
						MandatoryValidation.MessageErrorIfNotEntered(info);
					}
				}
			}
			if (ShouldWarnOnCSI_ReferenceNumberLength && referenceNumber.Length > CSI_ReferenceNumberMaxLength)
			{
				info.AddWarning(string.Format(lengthWarningMessage, CSI_ReferenceNumberMaxLength));
			}
		}

		protected override void CheckCSI_AdditionalDescription()
		{
			base.CheckCSI_AdditionalDescription();
			var parent = Parent;
			if (ShouldWarnOnCSI_AdditionalDescriptionLength && parent.CSI_AdditionalDescription.Length > CSI_AdditionalDescriptionMaxLength)
			{
				parent.CSI_AdditionalDescriptionInfo.AddWarning(string.Format(lengthWarningMessage, CSI_AdditionalDescriptionMaxLength));
			}
		}

		protected override void CheckCSI_Description()
		{
			base.CheckCSI_Description();
			var parent = Parent;
			if (ShouldWarnOnCSI_DescriptionLength && parent.CSI_Description.Length > CSI_DescriptionMaxLength)
			{
				parent.CSI_DescriptionInfo.AddWarning(string.Format(lengthWarningMessage, CSI_DescriptionMaxLength));
			}
		}

		protected virtual bool ShouldWarnOnCSI_ReferenceNumberLength => true;
		const int CSI_ReferenceNumberMaxLength = 35;

		protected virtual bool ShouldWarnOnCSI_AdditionalDescriptionLength => true;
		const int CSI_AdditionalDescriptionMaxLength = 35;

		protected virtual bool ShouldWarnOnCSI_DescriptionLength => true;
		const int CSI_DescriptionMaxLength = 260;

		static string lengthWarningMessage => Res.GetString("94839F63-8227-4E73-A7CE-398E2D955ED3", "Maximum length of this field has been exceeded. Only the first {0} characters will be sent in message to Customs.");

		static string MixedStatusMessage => Res.GetString("0f43f5aa-79f5-4cea-a16d-40621451c81a", "This declaration has entries with multiple statuses");
		static string MultipleInstructionsMessage => Res.GetString("dc24b4c7-4dea-44b0-8255-5a966db51402", "This invoice contains lines which are used over multiple entry instructions");
	}
}
