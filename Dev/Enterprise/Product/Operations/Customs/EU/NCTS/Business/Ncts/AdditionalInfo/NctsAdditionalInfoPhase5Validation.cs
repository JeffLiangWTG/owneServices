using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsAdditionalInfoPhase5Validation : NctsAdditionalInfoValidation, IRuleG0321Checker
	{
		public NctsAdditionalInfoPhase5Validation(NctsAdditionalInfo parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();

			ValidateCSI_CodeOnParentAsGoodsItem();
		}

		public bool CheckRuleG0321()
		{
			var parent = Parent;
			return IsPhase5RuleActive(parent, x => x.IsRuleG0321Active)
				&& parent.CSI_ReferenceNumber.IsEmpty
				&& !parent.CSI_Code.IsEmpty
				&& (parent.IsAnAdditionalReference || parent.IsATransportDocument);
		}

		protected override void CheckCSI_Code()
		{
			if (base.Parent.CSI_Code.SubstringSafe(0, 3) != "SGI")
			{
				if (Parent.IsArrival)
				{
					if (IsCodeEnabled)
					{
						if (IsCodeMandatory)
						{
							MandatoryValidation.CheckEntered(Parent.CSI_CodeInfo);
						}

						if (ShouldCodeBeInTheList)
						{
							ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo);
						}
					}
				}
				else
				{
					base.CheckCSI_Code();
				}
			}
		}

		protected override void CheckCSI_SubType()
		{
			base.CheckCSI_SubType();

			var parent = Parent;
			var info = parent.CSI_SubTypeInfo;
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(info);

			if (parent.ImportExportParent is NctsHeader header)
			{
				CheckMaxCountForSubType(header, header.AdditionalDocuments);

				if (IsPhase5RuleActive(parent, x => x.IsRuleR3060Active)
					&& parent.CSI_Code == UniversalReferenceConstants.AdditionalDocumentTypes._30600
					&& parent.IsAnAdditionalInformation)
				{
					var codesOfC0009 = UniversalLookupsHelper.GetCountryC0009List(parent.Factory, RefDataGroupingCodes.EuropeanUnionEUN);
					if (codesOfC0009.ContainsCode(header.MovementHeader.BM_RL_NKDestinationPort))
					{
						info.AddMessageError(Res.GetString("NCTSP5|AdditionalInfo CheckRule|R3060_MovementHeader", "[R3060] Doc. Kind = {0} and Doc. Type = {1} information cannot be declared at Consignment level Additional Document since Consignment level Destination Country is from CL009 ({2}) list.", AdditionalInfoSubTypeList.Codes.AdditionalInformation, UniversalReferenceConstants.AdditionalDocumentTypes._30600, UniversalReferenceConstants.CountryCodesTypes.CountryCodesCommonTransit));
					}
					else if (header.Bills.Any(bill => bill.GoodsItems.Any(goodsItem => codesOfC0009.ContainsCode(goodsItem.BY_RN_NKCountryOfDestination))))
					{
						info.AddMessageError(Res.GetString("NCTSP5|AdditionalInfo CheckRule|R3060_GoodsItem", "[R3060] Doc. Kind = {0} and Doc. Type = {1} information cannot be declared at Consignment level Additional Document since one of the Goods Items Destination Country is from CL009 ({2}) list.", AdditionalInfoSubTypeList.Codes.AdditionalInformation, UniversalReferenceConstants.AdditionalDocumentTypes._30600, UniversalReferenceConstants.CountryCodesTypes.CountryCodesCommonTransit));
					}
				}
			}

			if (parent.ImportExportParent is NctsCommonCargoDesc cargoDesc)
			{
				CheckMaxCountForSubType(cargoDesc.Header, cargoDesc.AdditionalInfos);
			}

			CheckCSI_SubType_E1301Rule();
		}

		protected override void CheckCSI_Status()
		{
			base.CheckCSI_Status();

			if (Parent.IsArrival)
			{
				ListValidation.ErrorIfInvalidCode(Parent.CSI_StatusInfo);
			}
		}

		protected override bool IsCodeMandatory => Parent.Parent is NctsCommonCargoDesc goodItem && goodItem.Header is NctsHeader header && header.IsArrivalMovement ? Parent.CSI_Status == NctsBillAdditionalDocumentStatusList.Codes.NEW : base.IsCodeMandatory;

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			var parent = Parent;

			if (CheckRuleG0321())
			{
				parent.CSI_ReferenceNumberInfo.AddWarning(ValidationRuleConfiguration.Messages.G0321Message);
			}

			if (CheckRuleC0015())
			{
				parent.CSI_ReferenceNumberInfo.AddMessageError(ValidationRuleConfiguration.Messages.C0015Message);
			}

			if (CheckRuleR0023())
			{
				parent.CSI_ReferenceNumberInfo.AddMessageError(ValidationRuleConfiguration.Messages.R0023Message);
			}

			if (IsPhase5RuleActive(parent, x => x.IsRuleTR0062Active))
			{
				NctsValidationHelper.CheckRuleTR0062(parent);
			}

			CheckReferenceNumberRuleE1104_1(parent);
		}

		void ValidateCSI_CodeOnParentAsGoodsItem()
		{
			var parent = Parent;
			if (parent.ParentAsGoodsItem is NctsCommonCargoDesc goodsItem && goodsItem.Header is NctsHeader header)
			{
				if (header.IsDepartureMovement && parent.IsAnAdditionalInformation && parent.CSI_Code == NctsConstants.AdditionalInfoCodes.InEXSWhereNegotiableBillsOfLadingAndUnknownConsigneeParticulars)
				{
					if (IsPhase5RuleActive(parent, x => x.IsRuleR3061Active)
					&& !header.IsInPhase5TransitionPeriod)
					{
						parent.AddRowMessageError(ValidationRuleConfiguration.Messages.R3061Message);
					}
				}
			}
		}

		void CheckCSI_SubType_E1301Rule()
		{
			var parent = Parent;
			new RuleE1301Validator(parent.ParentAsNctsHeader).Validate(
				parent.CSI_SubTypeInfo,
				Res.GetString("A1091E53-0D9E-4F51-9178-B4EECB853225", "Additional Information, Additional Reference and Transport Document"),
				IsPhase5RuleActive(parent, x => x.IsRuleE1301Active));
		}

		void CheckReferenceNumberRuleE1104_1(NctsAdditionalInfo parent)
		{
			var targetInfo = parent.CSI_ReferenceNumberInfo;
			var maxLengthReferenceNumberInTransitionPeriod = NctsConstants.CustomsFieldMaxLength.TransitionPeriod.AdditionalDocument.ReferenceNumber;

			if (parent.ParentAsGoodsItem?.Header is NctsHeader header
				&& IsPhase5RuleActive(parent, x => x.IsRuleE1104_1Active)
				&& header.IsDepartureMovement
				&& header.IsInPhase5TransitionPeriod
				&& (parent.IsATransportDocument || parent.IsAnAdditionalReference)
				&& parent.CSI_ReferenceNumber.Length > maxLengthReferenceNumberInTransitionPeriod)
			{
				var messageError = Res.GetString(
					"957FC4F5-4BA7-48B7-A74D-D2CCD4CC9104",
					"{0} During the transition period, which is active now, {1} cannot be longer than {2} characters.",
					ValidationRuleCodeConstants.E1104_1.GetRuleCodeMessagePrefix(),
					targetInfo.HumanReadableName,
					maxLengthReferenceNumberInTransitionPeriod);

				targetInfo.AddMessageError(messageError);
			}
		}

		void CheckMaxCountForSubType(NctsHeader header, INctsAdditionalInfoCollection<NctsAdditionalInfo> additionalInfos)
		{
			const int maxNumberOfDocumentsPerKind = 99;
			var subType = Parent.CSI_SubType;
			var info = Parent.CSI_SubTypeInfo;

			if (header is null || subType.IsEmpty || additionalInfos.Count <= maxNumberOfDocumentsPerKind)
			{
				return;
			}

			var numberOfDocumentsWithCurrentKind = additionalInfos.Count(x => x.CSI_SubType == subType);
			if (numberOfDocumentsWithCurrentKind <= maxNumberOfDocumentsPerKind)
			{
				return;
			}

			var isDeparture = header.IsDepartureMovement;
			var errorMessage = subType.ToString() switch
			{
				AdditionalInfoSubTypeList.Codes.AdditionalInformation when isDeparture && IsPhase5RuleActive(Parent, x => x.IsRuleTR0031Active) => ValidationRuleConfiguration.Messages.TR0031Message(maxNumberOfDocumentsPerKind),
				AdditionalInfoSubTypeList.Codes.AdditionalReference when isDeparture && IsPhase5RuleActive(Parent, x => x.IsRuleTR0032Active) => ValidationRuleConfiguration.Messages.TR0032Message(maxNumberOfDocumentsPerKind),
				AdditionalInfoSubTypeList.Codes.TransportDocument when isDeparture && IsPhase5RuleActive(Parent, x => x.IsRuleTR0033Active) => ValidationRuleConfiguration.Messages.TR0033Message(maxNumberOfDocumentsPerKind),
				_ => Res.GetString("3389DAD6-3186-4A62-9AB5-B78E6BA45CA2", "You may enter a maximum of {0} Additional Documents with Kind of Document {1}.", maxNumberOfDocumentsPerKind, subType)
			};

			info.AddMessageError(errorMessage);
		}

		bool CheckRuleC0015()
		{
			var parent = Parent;
			return parent.ParentAsGoodsItem?.Header is NctsHeader nctsHeader
				&& IsPhase5RuleActive(parent, x => x.IsRuleC0015Active)
				&& parent.CSI_ReferenceNumber.IsEmpty
				&& nctsHeader.IsDepartureMovement
				&& IsPreviousDocumentExcise(nctsHeader, parent);
		}

		bool CheckRuleR0023()
		{
			const string ZeroReferenceNumber = "0";
			var parent = Parent;
			return parent.ParentAsGoodsItem?.Header is NctsHeader nctsHeader
				&& IsPhase5RuleActive(parent, x => x.IsRuleR0023Active)
				&& parent.CSI_ReferenceNumber == ZeroReferenceNumber
				&& IsPreviousDocumentExcise(nctsHeader, parent);
		}

		bool IsPreviousDocumentExcise(NctsHeader header, NctsAdditionalInfo parent) => parent.IsAnAdditionalReference
			&& !parent.CSI_Code.IsEmpty
			&& header.GetCL234List().ContainsCode(parent.CSI_Code);

		bool IsPhase5RuleActive(NctsAdditionalInfo info, Func<INctsAdditionalInfoPhase5ValidationDecider, bool> ruleCheck)
		{
			if (info.AdditionalInfoValidationDecider is INctsAdditionalInfoPhase5ValidationDecider phase5ValidationDecider)
			{
				return ruleCheck(phase5ValidationDecider);
			}

			return false;
		}

		protected ValidationRuleConfiguration ValidationRuleConfiguration => Parent.Header?.Configuration.ValidationRuleConfiguration;
	}
}
