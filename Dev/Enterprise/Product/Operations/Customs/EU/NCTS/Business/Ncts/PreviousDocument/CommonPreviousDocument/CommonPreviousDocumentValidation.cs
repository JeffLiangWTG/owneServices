using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CommonPreviousDocumentValidation(CommonPreviousDocument parent) : PreviousDocumentValidation(parent), IRuleG0321Checker
	{
		protected new CommonPreviousDocument Parent => (CommonPreviousDocument)base.Parent;

		ICanBeImportOrExport DocumentParent => Parent.Parent;

		NctsHeader Header => GetHeaderWhenParentIsNctsHeaderOrNctsBill(DocumentParent) ?? (DocumentParent as NctsCommonCargoDesc).Header;

		ValidationRuleConfiguration ValidationRuleConfiguration => Header.Configuration.ValidationRuleConfiguration;

		public override void ValidateAll()
		{
			base.ValidateAll();

			Parent.ClearRowNotifications();
			CheckRowTR0030_1();
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();

			var parent = Parent;
			if (parent.ValidationDecider is { IsRuleG0321Active: false })
			{
				PropertyIsMandatoryWhenHasAttributeWithValueY(parent.CSI_ReferenceNumberInfo, UniversalReferenceConstants.RefCusCodeListAttributeTypes.Reference);
			}
			CheckCSI_ReferenceNumber_R0416Rule();
			CheckCSI_ReferenceNumber_NR0008Rule();

			var validationRuleConfiguration = ValidationRuleConfiguration;
			if (CheckRuleG0321())
			{
				parent.CSI_ReferenceNumberInfo.AddWarning(validationRuleConfiguration.Messages.G0321Message);
			}
			new CommonPreviousDocumentRuleNR0048Validation(parent).ValidateReferenceNumber(validationRuleConfiguration);
		}

		protected override void CheckCSI_ReferenceNumber2()
		{
			base.CheckCSI_ReferenceNumber2();
			var parent = Parent;
			if (ShouldCheckCSI_ReferenceNumber2Mandatory)
			{
				PropertyIsMandatoryWhenHasAttributeWithValueY(parent.CSI_ReferenceNumber2Info, UniversalReferenceConstants.RefCusCodeListAttributeTypes.Complement);
			}
			UniversalValidationHelper.CheckMaxLengthIfPhase5TransitionPeriod(parent.IsInPhase5TransitionPeriod, parent.CSI_ReferenceNumber2Info, 26);
		}

		protected virtual bool ShouldCheckCSI_ReferenceNumber2Mandatory => true;

		protected override void CheckCSI_SubType()
		{
		}

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();
			CheckCodeE1301Rule();
			CheckDocumentTypeG0026_1Rule();
		}

		void CheckDocumentTypeG0026_1Rule()
		{
			var commonPreviousDocument = Parent;
			if (DocumentParent is NctsBill nctsBill
				&& nctsBill.Header is NctsHeader header
				&& header.IsPhase5Departure
				&& commonPreviousDocument.ValidationDecider is ICommonPreviousDocumentDepartureValidationDecider { IsRuleG0026_1Active: true })
			{
				var codeInfo = commonPreviousDocument.CSI_CodeInfo;
				if (commonPreviousDocument.CSI_Code != NctsConstants.NctsTypeOfPreviousDocument.Codes.N830)
				{
					codeInfo.AddMessageError(Res.GetString("18CD4AF4-1FB5-49CD-9831-FA546ED90F96", "{0} House Consignment > Previous Document > Type > can only be N830.", ValidationRuleCodeConstants.G0026_1.GetRuleCodeMessagePrefix()));
				}

				if (header.MovementHeader.BM_AdditionalDeclarationType != NctsTypeOfAdditionalDeclarationList.Codes.A)
				{
					codeInfo.AddMessageError(Res.GetString("A1E2A182-4E53-4EC2-8E84-95CC40280715", "{0} House Consignment > Previous Documents > can be used only if Additional Declaration Type = 'A'.", ValidationRuleCodeConstants.G0026_1.GetRuleCodeMessagePrefix()));
				}
			}
		}

		void CheckCodeE1301Rule()
		{
			if (GetHeaderWhenParentIsNctsHeaderOrNctsBill(DocumentParent) is NctsHeader header)
			{
				var commonPreviousDocument = Parent;
				new RuleE1301Validator(header).Validate(Parent.CSI_CodeInfo, Res.GetString("4992A7DC-3509-4639-895E-3BA105F817D1", "Previous Document"), commonPreviousDocument.ValidationDecider is ICommonPreviousDocumentDepartureValidationDecider { IsRuleE1301Active: true });
			}
		}

		NctsHeader GetHeaderWhenParentIsNctsHeaderOrNctsBill(ICanBeImportOrExport documentParent) => documentParent as NctsHeader ?? (documentParent as NctsBill)?.Header;

		void PropertyIsMandatoryWhenHasAttributeWithValueY(ZPropertyInfo propertyInfo, string attributeName)
		{
			var parent = Parent;
			if (parent.RefCusCode.HasAttributeForMandatoryValidation(attributeName, parent.LevelAttributeValue))
			{
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
			}
		}

		void CheckCSI_ReferenceNumber_R0416Rule()
		{
			var referenceNumber = Parent.CSI_ReferenceNumber;
			if (Parent.Parent is NctsBill
				&& Parent.ValidationDecider is ICommonPreviousDocumentDepartureValidationDecider { IsRuleR0416Active: true }
				&& referenceNumber.Length >= 17
				&& referenceNumber[16] is not ('A' or 'B' or 'E'))
			{
				Parent.CSI_ReferenceNumberInfo.AddMessageError(ValidationRuleConfiguration.Messages.R0416Message);
			}
		}

		void CheckCSI_ReferenceNumber_NR0008Rule()
		{
			var parent = Parent;
			var referenceNumber = parent.CSI_ReferenceNumber;
			if (!referenceNumber.IsEmpty
				&& parent.CSI_Code == NctsConstants.NctsTypeOfPreviousDocument.Codes.N830
				&& parent.IsInPhase5DepartureHouseConsignment
				&& parent.ValidationDecider is ICommonPreviousDocumentDepartureValidationDecider  { IsRuleNR0008Active: true })
			{
				var mrnFormatValidationError = NctsValidationHelper.CheckMRNFormat(referenceNumber, parent.Factory, ZString.Empty);
				if (!mrnFormatValidationError.IsEmpty)
				{
					parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("CEDF54C0-858A-4590-88B3-A41726918361", "[NR0008]: {0}", mrnFormatValidationError));
				}
			}
		}

		void CheckRowTR0030_1()
		{
			if (Parent.IsNCTSPreviousDocument
				&& parent.ValidationDecider is { IsRuleTR0030_1Active: true }
				&& parent.IsInPhase5TransitionPeriod)
			{
				if ((DocumentParent is NctsBill bill && bill.MaxCountNCTSPreviousDocuments + Header.NCTSPreviousDocumentsCount > 9)
					|| (DocumentParent is NctsHeader header && header.MaxNCTSPreviousDocumentsCount > 9))
				{
					Parent.AddRowMessageError(ValidationRuleConfiguration.Messages.TR0030_1Message);
				}
			}
		}

		public bool CheckRuleG0321()
		{
			var parent = Parent;
			return parent.ValidationDecider is ICommonPreviousDocumentValidationDecider { IsRuleG0321Active: true }
					&& parent.CSI_ReferenceNumber.IsEmpty
					&& !parent.CSI_Code.IsEmpty;
		}
	}
}
