using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsBillAdditionalDocumentValidation : AdditionalInfoValidation, IRuleG0321Checker
	{
		public NctsBillAdditionalDocumentValidation(AdditionalInfo parent) : base(parent)
		{
		}

		public bool CheckRuleG0321()
		{
			var parent = Parent;
			return IsPhase5RuleActive(parent, x => x.IsRuleG0321Active)
				&& parent.CSI_ReferenceNumber.IsEmpty
				&& !parent.CSI_Code.IsEmpty
				&& (parent.IsAnAdditionalReference || parent.IsATransportDocument);
		}

		protected new NctsBillAdditionalDocument Parent => (NctsBillAdditionalDocument)base.Parent;

		NctsHeader Header => Parent?.Header;

		protected ValidationRuleConfiguration ValidationRuleConfiguration => Header?.Configuration.ValidationRuleConfiguration;

		protected override void CheckCSI_ReferenceNumber()
		{
			var parent = Parent;
			var info = parent.CSI_ReferenceNumberInfo;

			if (CheckRuleG0321())
			{
				parent.CSI_ReferenceNumberInfo.AddWarning(ValidationRuleConfiguration.Messages.G0321Message);
			}

			if (!(ValidationRuleConfiguration?.IsRuleG0321Active ?? false))
			{
				PropertyIsMandatoryWhenHasAttributeWithValueY(info, UniversalReferenceConstants.RefCusCodeListAttributeTypes.Reference);
			}

			if (IsPhase5RuleActive(parent, x => x.IsRuleTR0062Active))
			{
				NctsValidationHelper.CheckRuleTR0062(parent);
			}
		}

		protected override void CheckCSI_Description()
		{
			base.CheckCSI_Description();
			PropertyIsMandatoryWhenHasAttributeWithValueY(Parent.CSI_DescriptionInfo, UniversalReferenceConstants.RefCusCodeListAttributeTypes.Complement);
		}

		void PropertyIsMandatoryWhenHasAttributeWithValueY(ZPropertyInfo propertyInfo, string attributeName)
		{
			if (Parent.RefCusCode.HasAttributeForMandatoryValidation(attributeName))
			{
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
			}
		}

		protected override void CheckCSI_Status()
		{
			if (Parent.Header.IsPhase5Arrival)
			{
				ListValidation.ErrorIfInvalidCode(Parent.CSI_StatusInfo);
			}

			if (IsOtherFieldsEnabled)
			{
				ValidateCSI_Code();
			}
		}

		protected override void CheckCSI_Code()
		{
			if (IsCodeEnabled)
			{
				var parent = Parent;

				if (IsCodeMandatory)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_CodeInfo);
				}

				if (ShouldCodeBeInTheList)
				{
					ListValidation.MessageErrorIfInvalidCode(parent.CSI_CodeInfo);
				}

				if (parent.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation
					&& parent.CSI_Code == NctsConstants.AdditionalInfoCodes.InEXSWhereNegotiableBillsOfLadingAndUnknownConsigneeParticulars
					&& Header is NctsHeader header
					&& header.IsDepartureMovement
					&& IsPhase5RuleActive(parent, x => x.IsRuleR3062Active))
				{
					var cl009CountryCodes = UniversalLookupsHelper.GetCountryC0009List(parent.Factory, RefDataGroupingCodes.EuropeanUnionEUN);
					if (cl009CountryCodes.ContainsCode(header.MovementHeader.BM_RL_NKDestinationPort)
						|| (parent.Parent is NctsBill bill && bill.GoodsItems.Any(goodsItem => cl009CountryCodes.ContainsCode(goodsItem.BY_RN_NKCountryOfDestination))))
					{
						parent.CSI_CodeInfo.AddMessageError(ValidationRuleConfiguration.Messages.R3062Message);
					}
				}
			}
		}

		protected override void CheckCSI_SubType()
		{
			base.CheckCSI_SubType();
			CheckMaxCountForSubType();
			ValidateSubTypeMessageErrorIfNotEntered();
			CheckSubTypeRuleE1301();
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_SubTypeInfo);
		}

		protected override bool IsCodeMandatory => Parent.CSI_Status == NctsBillAdditionalDocumentStatusList.Codes.NEW;

		void CheckSubTypeRuleE1301()
		{
			var additionalInfo = Parent;
			var isRuleActive = IsPhase5RuleActive(Parent, x => x.IsRuleE1301Active);
			if (Header is NctsHeader header && additionalInfo.CSI_SubType.ToString().In(AdditionalInfoSubTypeList.Codes.AdditionalInformation, AdditionalInfoSubTypeList.Codes.AdditionalReference))
			{
				new RuleE1301Validator(header).Validate(additionalInfo.CSI_SubTypeInfo, AdditionalInformationAndAdditionalReference, isRuleActive);
			}
		}

		string AdditionalInformationAndAdditionalReference => Res.GetString("0B3A555A-404E-43AD-8229-0FCC18D96F5A", "Additional Information and Additional Reference");

		void CheckMaxCountForSubType()
		{
			var bill = Parent.Parent;
			var header = Header;
			var subType = Parent.CSI_SubType;
			var info = Parent.CSI_SubTypeInfo;

			const int maxNumberOfDocumentsPerType = 99;

			if (!header.IsDepartureMovement ||
				bill.AdditionalDocuments.Count <= maxNumberOfDocumentsPerType ||
				bill.AdditionalDocuments.Count(x => x.CSI_SubType == subType) <= maxNumberOfDocumentsPerType)
			{
				return;
			}

			var errorMessage = subType.ToString() switch
			{
				AdditionalInfoSubTypeList.Codes.AdditionalInformation => IsPhase5RuleActive(Parent, x => x.IsRuleTR0031Active)
					? ValidationRuleConfiguration.Messages.TR0031Message(maxNumberOfDocumentsPerType) : null,
				AdditionalInfoSubTypeList.Codes.AdditionalReference => IsPhase5RuleActive(Parent, x => x.IsRuleTR0032Active)
					? ValidationRuleConfiguration.Messages.TR0032Message(maxNumberOfDocumentsPerType) : null,
				AdditionalInfoSubTypeList.Codes.TransportDocument => IsPhase5RuleActive(Parent, x => x.IsRuleTR0033Active)
					? ValidationRuleConfiguration.Messages.TR0033Message(maxNumberOfDocumentsPerType) : null,
				_ => null
			};

			if (!errorMessage.IsNullOrEmpty())
			{
				info.AddMessageError(errorMessage);
			}
		}

		void ValidateSubTypeMessageErrorIfNotEntered()
		{
			var parent = Parent;
			if (parent.CSI_SubType.IsEmpty && Header.IsPhase5Departure)
			{
				parent.CSI_SubTypeInfo.AddMessageError(Res.GetString("2E012D4A-E6B4-4B8F-B81E-0B9354A9029F", "You have not entered a Kind of Document"));
			}
		}

		bool IsPhase5RuleActive(NctsBillAdditionalDocument doc, Func<INctsBillAdditionalDocumentPhase5ValidationDecider, bool> ruleCheck)
		{
			if (doc.BillAdditionalDocumentValidationDecider is INctsBillAdditionalDocumentPhase5ValidationDecider phase5ValidationDecider)
			{
				return ruleCheck(phase5ValidationDecider);
			}

			return false;
		}
	}
}
