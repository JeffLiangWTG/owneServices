using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Customs.EU.NCTS.Business.NctsConstants;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsSupportingDocumentPhase5DepartureValidation : EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentValidation, IRuleG0321Checker
	{
		public NctsSupportingDocumentPhase5DepartureValidation(NctsSupportingDocument parent)
			: base(parent)
		{
		}

		protected new NctsSupportingDocument Parent => (NctsSupportingDocument)base.Parent;

		protected virtual ZString ItemNumberPropertyDescription => ZString.Empty;

		protected ValidationRuleConfiguration ValidationRuleConfiguration => Parent.Header?.Configuration.ValidationRuleConfiguration;

		protected INctsSupportingDocumentDeparturePhase5ValidationDecider Decider => Parent.ValidationDecider as INctsSupportingDocumentDeparturePhase5ValidationDecider;

		public bool CheckRuleG0321()
		{
			var parent = Parent;
			return parent.CSI_ReferenceNumber.IsEmpty
				&& !parent.CSI_Code.IsEmpty
				&& (Decider?.IsRuleG0321Active ?? false);
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			var parent = Parent;
			if (CheckRuleG0321())
			{
				parent.CSI_ReferenceNumberInfo.AddWarning(ValidationRuleConfiguration.Messages.G0321Message);
			}
			else if (parent.Parent != null && !(Decider?.IsRuleG0321Active ?? false) && parent.RefCusCode.HasAttributeForMandatoryValidation(UniversalReferenceConstants.RefCusCodeListAttributeTypes.Reference))
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_ReferenceNumberInfo);
			}

			ValidateCSI_ReferenceNumberIsValid();
			CheckCSI_ReferenceNumber_PR30Rule();
		}

		protected void ValidateCSI_ReferenceNumberIsValid()
		{
			var parent = Parent;
			if (parent.Parent is NctsDepartureCargoDesc || parent.Parent is NctsBill)
			{
				if (parent.Header is NctsHeader nctsHeader && (Decider?.IsRuleNR0006Active ?? false)
					&& parent.CSI_Code == UniversalReferenceConstants.RefCusCodeList.Codes.C651)
				{
					var arcError = EU.Business.EMCSARCValidationHelper.ValidateACRNumberIsValid(parent.CSI_ReferenceNumber, parent.Factory);
					if (!arcError.IsEmpty)
					{
						parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("67CA63C5-17D1-4B1C-86B0-8E59D2A3A938", "[NR0006] {0}", arcError));
					}
				}
			}
		}

		protected override void CheckCSI_ReferenceNumber2()
		{
			base.CheckCSI_ReferenceNumber2();
			if (Parent.IsInPhase5TransitionPeriod)
			{
				UniversalValidationHelper.CheckMaxLength(Parent.CSI_ReferenceNumber2Info, 26, NctsConstants.ValidationRuleMessagePrefixes.E1117);
			}
			else
			{
				UniversalValidationHelper.CheckMaxLength(Parent.CSI_ReferenceNumber2Info, 35);
			}
		}

		protected override void CheckCSI_ItemNumber()
		{
			base.CheckCSI_ItemNumber();

			var parent = Parent;
			if (parent.RefCusCode.HasAttributeForMandatoryValidation(UniversalReferenceConstants.RefCusCodeListAttributeTypes.ItemNumber))
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_ItemNumberInfo, ItemNumberPropertyDescription);
			}
		}

		protected override void CheckCodeCore()
		{
			MandatoryValidation.CheckEntered(Parent.CSI_CodeInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo);
		}

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();
			CheckCodeE1301Rule();
		}

		void CheckCodeE1301Rule()
		{
			var supportingDocument = Parent;
			if (GetHeaderWhenParentIsNctsDepartureMovementHeaderOrNctsBill(supportingDocument.Parent) is NctsHeader header)
			{
				new RuleE1301Validator(header).Validate(
					supportingDocument.CSI_CodeInfo,
					Res.GetString("8F211135-6664-4287-B316-17BEC2AD4CD2", "Supporting Documents"),
					Decider is { IsRuleE1301Active: true });
			}
		}

		void CheckCSI_ReferenceNumber_PR30Rule()
		{
			var parent = Parent;
			if ((Decider?.IsRuleRP30Active ?? false)
				&& parent.CSI_Code == NctsTypeOfSupportingDocument.Codes._7P17)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_ReferenceNumberInfo,
					propertyDescription: Res.GetString("DA1AEC04-3271-4AFD-A6A6-94C072479233", "Certificate/Reference number"), messagePrefix: "[RP30] ");
			}
		}

		static NctsHeader GetHeaderWhenParentIsNctsDepartureMovementHeaderOrNctsBill(BusinessObject supportingDocumentParent)
			=> (supportingDocumentParent as NctsDepartureMovementHeader)?.Header ?? (supportingDocumentParent as NctsBill)?.Header;
	}
}
