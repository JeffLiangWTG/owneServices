using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsArrivalCargoDescValidation : NctsCommonCargoDescValidation
	{
		public NctsArrivalCargoDescValidation(NctsArrivalCargoDesc parent)
			: base(parent)
		{
		}

		protected new NctsArrivalCargoDesc Parent => (NctsArrivalCargoDesc)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateNewRecordHasDataSpecified();
			ValidateDifferenceBetweenDeclaredAndUnloadedValue();
			ValidateLiabilityTariff();
		}

		protected void ValidateNewRecordHasDataSpecified()
		{
			var parent = Parent;
			var error = Res.GetString("0E6D62DB-2EF2-498D-8784-BED2D1AE4542", "No data was specified.");
			parent.ClearRowNotificationsContaining(error);

			if (parent.BY_UnloadedState == NctsUnloadedStateList.Codes.NEW &&
				parent.BY_Description.IsEmpty &&
				parent.BY_CusC4Number.IsEmpty &&
				parent.BY_GrossWeight.IsEmpty &&
				parent.BY_NetWeight.IsEmpty &&
				parent.BY_CommodityCode.IsEmpty)
			{
				parent.AddRowError(error);
			}
		}

		protected void ValidateDifferenceBetweenDeclaredAndUnloadedValue()
		{
			var parent = Parent;
			var error = Res.GetString("C24FA9DD-1122-44B1-B238-742E5DDF8717", "You have not captured any differences for Unloaded Value.");
			parent.ClearRowNotificationsContaining(error);

			if (IsDifferenceInValid())
			{
				parent.AddRowMessageError(error);
			}
		}

		protected override void CheckBY_UnloadedState()
		{
			base.CheckBY_UnloadedState();
			var parent = Parent;
			var unloadedStateInfo = parent.BY_UnloadedStateInfo;

			MandatoryValidation.CheckEntered(unloadedStateInfo);

			if (parent.BY_UnloadedState != NctsUnloadedStateList.Codes.NEW || unloadedStateInfo.OriginalValue.ToString() != NctsUnloadedStateList.Codes.NEW)
			{
				ListValidation.ErrorIfInvalidCode(unloadedStateInfo);
			}

			switch (parent.BY_UnloadedState)
			{
				case NctsUnloadedStateList.Codes.DEC:
					if (IsDifferenceInValidInDEC())
					{
						unloadedStateInfo.AddMessageError(Res.GetString("14FCCC1F-670F-4D1E-A7A1-0C0D03D63654", "The goods item {0} of house consignment {1} has an unloaded state DEC, while there were changes in the related documents or packages. These changes will not be sent to customs unless you change the unloaded state to DIF.", parent.BY_LineNo, parent.Bill.MovementDetail.B9_SeqNo));
					}
					break;
				case NctsUnloadedStateList.Codes.DIF:
					if (IsDifferenceInValid())
					{
						unloadedStateInfo.AddMessageError(Res.GetString("0D4F41F2-BEB4-4F43-9D8E-B96633398C95", "The goods item {0} of house consignment {1} has an unloaded state DIF, while there are no changes in the related documents or packages. This might potentially give an error by customs. Please change the status to DEC.", parent.BY_LineNo, parent.Bill?.MovementDetail?.B9_SeqNo));
					}
					break;
				case NctsUnloadedStateList.Codes.MIS:
					if (IsDifferenceInValidInMIS())
					{
						unloadedStateInfo.AddMessageError(Res.GetString("AB0DF47F-35E8-4E36-B6EF-C5FA7A029011", "The goods item {0} of house consignment {1} has an unloaded state MIS, while there are changes (NEW) in the related documents or packages. These changes will not be sent to customs because the entire house will be sent as “missing”. Please change the unloaded state of the house consignment to DIF or delete the newly created goods items or documents.", parent.BY_LineNo, parent.Bill.MovementDetail.B9_SeqNo));
					}
					break;
			}

			if (Parent.Header?.Configuration.ValidationRuleConfiguration is ValidationRuleConfiguration configuration
				&& Parent.ValidationDecider is INctsArrivalCargoDescPhase5ValidationDecider validationDecider
				&& validationDecider.IsRuleNR0029Active
				&& Parent.BY_UnloadedState == NctsUnloadedStateList.Codes.NEW
				&& Parent.Packages.Count == 0)
			{
				Parent.BY_UnloadedStateInfo.AddMessageError(configuration.Messages.GetNR0029eMessage());
			}
		}

		bool IsDifferenceInValid()
		{
			var parent = Parent;
			var unloadedGoodsItem = parent.UnloadedGoodsItem;
			return parent.BY_UnloadedState == NctsUnloadedStateList.Codes.DIF &&
				   parent.BY_Description == unloadedGoodsItem.BY_Description &&
				   parent.BY_CusC4Number == unloadedGoodsItem.BY_CusC4Number &&
				   parent.BY_GrossWeight == unloadedGoodsItem.BY_GrossWeight &&
				   parent.BY_NetWeight == unloadedGoodsItem.BY_NetWeight &&
				   parent.BY_HarmonisedTariff.SubstringSafe(0, 8) == unloadedGoodsItem.BY_HarmonisedTariff.SubstringSafe(0, 8) &&
				   !parent.SupportingDocuments.Cast<NctsSupportingDocument>().Any(x => !x.CSI_Status.EqualsIgnoringCase(NctsUnloadedStateList.Codes.DEC)) &&
				   !parent.AdditionalInfos.Cast<NctsAdditionalInfo>().Any(x => !x.CSI_Status.EqualsIgnoringCase(NctsUnloadedStateList.Codes.DEC)) &&
				   !parent.Packages.Cast<NctsPackage>().Any(x => !x.B5_TypeOfDifference.EqualsIgnoringCase(NctsUnloadedStateList.Codes.DEC));
		}

		bool IsDifferenceInValidInDEC()
		{
			var parent = Parent;
			return parent.SupportingDocuments.Cast<NctsSupportingDocument>().Any(x => !x.CSI_Status.EqualsIgnoringCase(NctsUnloadedStateList.Codes.DEC)) ||
				   parent.AdditionalInfos.Cast<NctsAdditionalInfo>().Any(x => !x.CSI_Status.EqualsIgnoringCase(NctsUnloadedStateList.Codes.DEC)) ||
				   parent.Packages.Cast<NctsPackage>().Any(x => !x.B5_TypeOfDifference.EqualsIgnoringCase(NctsUnloadedStateList.Codes.DEC));
		}

		bool IsDifferenceInValidInMIS()
		{
			var parent = Parent;
			return parent.Packages.Cast<NctsPackage>().Any(x => x.B5_TypeOfDifference.EqualsIgnoringCase(NctsUnloadedStateList.Codes.NEW)) ||
				   parent.SupportingDocuments.Cast<NctsSupportingDocument>().Any(x => x.CSI_Status.EqualsIgnoringCase(NctsUnloadedStateList.Codes.NEW)) ||
				   parent.AdditionalInfos.Cast<NctsAdditionalInfo>().Any(x => x.CSI_Status.EqualsIgnoringCase(NctsUnloadedStateList.Codes.NEW));
		}

		protected override void CheckBY_HarmonisedTariff()
		{
			base.CheckBY_HarmonisedTariff();

			var parent = Parent;
			if (parent.ValidationDecider is INctsArrivalCargoDescPhase5ValidationDecider validationDecider
				&& parent.BY_UnloadedState.In(new ZString[] { NctsUnloadedStateList.Codes.NEW, NctsUnloadedStateList.Codes.DIF }))
			{
				if (validationDecider.IsRuleNR0004Active)
				{
					parent.CheckRuleNR0004(parent.BY_HarmonisedTariffInfo);
				}
				if (validationDecider.IsRuleNR0055Active)
				{
					parent.CheckRuleNR0055(parent.BY_HarmonisedTariffInfo);
				}
			}

			CheckBY_HarmonisedTariffIsValid();
		}

		protected virtual void CheckBY_HarmonisedTariffIsValid()
		{
			var parent = Parent;
			var harmonisedTariffInfo = parent.BY_HarmonisedTariffInfo;
			var unloadedState = parent.BY_UnloadedState;
			var harmonisedTariff = parent.BY_HarmonisedTariff;

			if (!NctsHelper.IsUnloadedStateAccepted(unloadedState))
			{
				if (!harmonisedTariff.IsEmpty && (harmonisedTariff.Length < HarmonisedTariffMinLength) && unloadedState.Equals(NctsUnloadedStateList.Codes.NEW))
				{
					harmonisedTariffInfo.AddMessageError(Res.GetString("AA5D885B-8EF5-4747-B998-32EF1C76190C", "Commodity Code must have at least {0} digits", HarmonisedTariffMinLength));
				}

				MandatoryValidation.MessageErrorIfNotEntered(harmonisedTariffInfo);

				if (parent.UniversalTariff == null)
				{
					harmonisedTariffInfo.AddMessageError(Res.GetString("9D61965A-2B30-4117-B710-88F7C48D289D", "There is no commodity code starting with {0}", harmonisedTariff));
				}
			}
		}

		protected virtual ZInt HarmonisedTariffMinLength => 6;

		protected override void BY_HarmonisedTariffCharacterCheck()
		{
			var parent = Parent;
			var unloadedState = parent.BY_UnloadedState;
			if (!NctsHelper.IsUnloadedStateAccepted(unloadedState))
			{
				CheckConditionA060(Parent, Parent.BY_HarmonisedTariffInfo);
			}

			void CheckConditionA060(NctsCommonCargoDesc goodsItem, ZPropertyInfo info)
			{
				var commodityCode = goodsItem.BY_HarmonisedTariff;
				if (!commodityCode.IsEmpty && commodityCode.Length != 8)
				{
					if (goodsItem.Bill.PreviousDocuments.Cast<CommonPreviousDocument>().Any(x => x.CSI_Code == PreviousDocumentTypes.GoodsDeclarationForExportation)
						|| goodsItem.Bill.Header.PreviousDocuments.Cast<CommonPreviousDocument>().Any(x => x.CSI_Code == PreviousDocumentTypes.GoodsDeclarationForExportation))
					{
						info.AddMessageError(NctsHeaderValidationHelper.GetRuleExplanation(Rules_C_Conditions.Codes.A060, Res.GetString("9ADC7AE7-9C73-47BD-8382-700A1770D92C", "Commodity code must be 8 positions long because the previous document is an export document.")));
					}
				}
			}
		}

		protected override void CheckBY_CusC4Number()
		{
			base.CheckBY_CusC4Number();
			if (Parent.BY_UnloadedState.Equals(NctsUnloadedStateList.Codes.NEW))
			{
				CheckBY_CusC4Number_ListValidation();
			}
		}

		protected override void CheckBY_Description()
		{
			base.CheckBY_Description();
			if (Parent.BY_UnloadedState.Equals(NctsUnloadedStateList.Codes.NEW))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BY_DescriptionInfo);
			}

			if (Parent.Header?.Configuration.ValidationRuleConfiguration is ValidationRuleConfiguration configuration
				&& Parent.ValidationDecider is INctsArrivalCargoDescPhase5ValidationDecider validationDecider
				&& validationDecider.IsRuleNR0029Active
				&& Parent.BY_UnloadedState == NctsUnloadedStateList.Codes.NEW
				&& Parent.BY_Description.IsEmpty)
			{
				Parent.BY_DescriptionInfo.AddMessageError(configuration.Messages.GetNR0029dMessage());
			}
		}

		protected override void CheckBY_GrossWeightUnit()
		{
			base.CheckBY_GrossWeightUnit();
			if (Parent.BY_UnloadedState.Equals(NctsUnloadedStateList.Codes.NEW))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BY_GrossWeightUnitInfo);
			}
		}

		protected override void CheckBY_GrossWeight()
		{
			base.CheckBY_GrossWeight();

			var parent = Parent;
			if (parent.Header?.Configuration.ValidationRuleConfiguration is ValidationRuleConfiguration configuration
				&& parent.ValidationDecider is INctsArrivalCargoDescPhase5ValidationDecider validationDecider
				&& validationDecider.IsRuleE1109_1Active)
			{
				var message = configuration.Messages.E1109_1Message(parent.BY_GrossWeightInfo.Description);
				UniversalValidationHelper.CheckMaxValueAndMaxDecimalLengthForWeightIfPhase5TransitionPeriod(parent.IsInPhase5TransitionPeriod, message, parent.BY_GrossWeightInfo, 11, 3);
			}
		}

		protected override void CheckBY_NetWeightUnit()
		{
			base.CheckBY_NetWeightUnit();
			if (Parent.BY_UnloadedState.Equals(NctsUnloadedStateList.Codes.NEW))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BY_NetWeightUnitInfo);
			}
		}

		protected override void CheckBY_NetWeight()
		{
			base.CheckBY_NetWeight();
			var parent = Parent;
			if (parent.Header?.Configuration.ValidationRuleConfiguration is ValidationRuleConfiguration configuration
				&& parent.ValidationDecider is INctsArrivalCargoDescPhase5ValidationDecider validationDecider
				&& validationDecider.IsRuleE1109_1Active)
			{
				var message = configuration.Messages.E1109_1Message(Parent.BY_NetWeightInfo.Description);
				UniversalValidationHelper.CheckMaxValueAndMaxDecimalLengthForWeightIfPhase5TransitionPeriod(parent.IsInPhase5TransitionPeriod, message, Parent.BY_NetWeightInfo, 11, 3);
			}
		}

		public void ValidateLiabilityTariff()
		{
			ValidateCalculatedProperty(Parent.LiabilityTariffInfo);
		}

		protected virtual void CheckLiabilityTariff()
		{
			var parent = Parent;
			if (parent.IsLiabilityCalculationForArrivalSupported)
			{
				CheckLiabilityTariffLength();
				CheckLiabilityListValidation();
			}

			void CheckLiabilityTariffLength()
			{
				var harmonisedTariff = parent.LiabilityTariff;
				if (!harmonisedTariff.IsEmpty && harmonisedTariff.Length < 10)
				{
					var info = parent.LiabilityTariffInfo;
					info.AddWarning(Res.GetString("E5D84A5C-BE64-40F9-9259-043173AD2BA3", "Please, for duties calculation, enter a 10-digits Tariff Code"));
				}
			}

			void CheckLiabilityListValidation()
			{
				if (Parent.Header != null)
				{
					if (LiabilityListNotificationTypeIsMessageError)
					{
						ListValidation.MessageErrorIfInvalidCode(Parent.LiabilityTariffInfo, Parent.Lookups.Tariffs);
					}
					else
					{
						ListValidation.WarnIfInvalidCode(Parent.LiabilityTariffInfo, Parent.Lookups.Tariffs);
					}
				}
			}
		}

		protected virtual bool LiabilityListNotificationTypeIsMessageError => true;

		protected override void CheckBY_Supplements()
		{
			base.CheckBY_Supplements();

			if (Parent.IsLiabilityCalculationForArrivalSupported)
			{
				NctsValidationHelper.CheckBY_Supplements_HasChildValidationNotification(Parent);
				NctsValidationHelper.CheckBY_Supplements_RuleNR0060(Parent, ValidationDecider, ValidationRuleConfiguration);
			}
		}

		protected override void CheckBY_RN_NKCountryOfOrigin()
		{
			base.CheckBY_RN_NKCountryOfOrigin();

			var goodsItem = Parent;
			if (goodsItem.IsLiabilityCalculationForArrivalSupported)
			{
				var parent = Parent;
				NctsValidationHelper.CheckBY_RN_NKCountryOfOriginIsValid(parent);
				NctsValidationHelper.CheckBY_RN_NKCountryOfOrigin_RuleNR0058(parent, ValidationDecider, ValidationRuleConfiguration);
			}
		}

		protected override void CheckBY_MonetaryValue()
		{
			base.CheckBY_MonetaryValue();

			if (Parent.IsLiabilityCalculationForArrivalSupported)
			{
				NctsValidationHelper.CheckBY_MonetaryValue_RuleNR0059(Parent, ValidationDecider, ValidationRuleConfiguration);
			}
		}

		protected override void CheckBY_CustomsSecondUnitQty()
		{
			base.CheckBY_CustomsSecondUnitQty();

			if (Parent.IsLiabilityCalculationForArrivalSupported)
			{
				NctsValidationHelper.CheckBY_CustomsSecondUnitQtyIsValid(Parent);
			}
		}

		protected virtual INotificationType TR0084NotificationType => CargoWise.EntityFramework.NotificationType.MessageError;

		protected override void CheckBY_CustomsSecondQuantity()
		{
			base.CheckBY_CustomsSecondUnitQty();

			var goodsItem = Parent;
			if (goodsItem.IsLiabilityCalculationForArrivalSupported)
			{
				NctsValidationHelper.CheckRuleTR0084(goodsItem.Header, goodsItem.BY_CustomsSecondQuantityInfo, goodsItem.BY_CustomsSecondUnitQty, goodsItem.BY_CustomsSecondQuantity, TR0084NotificationType);
			}
		}

		protected override void CheckBY_CustomsThirdQuantity()
		{
			base.CheckBY_CustomsThirdQuantity();

			var goodsItem = Parent;
			if (goodsItem.IsLiabilityCalculationForArrivalSupported)
			{
				NctsValidationHelper.CheckRuleTR0084(goodsItem.Header, goodsItem.BY_CustomsThirdQuantityInfo, goodsItem.BY_CustomsThirdUnitQty, goodsItem.BY_CustomsThirdQuantity, TR0084NotificationType);
			}
		}

		protected override void CheckBY_CustomsFourthQuantity()
		{
			base.CheckBY_CustomsFourthQuantity();

			var goodsItem = Parent;
			if (goodsItem.IsLiabilityCalculationForArrivalSupported)
			{
				NctsValidationHelper.CheckRuleTR0084(goodsItem.Header, goodsItem.BY_CustomsFourthQuantityInfo, goodsItem.BY_CustomsFourthUnitQty, goodsItem.BY_CustomsFourthQuantity, TR0084NotificationType);
			}
		}

		ValidationRuleConfiguration ValidationRuleConfiguration => Parent.Header?.Configuration.ValidationRuleConfiguration;

		INctsCargoDescValidationDecider ValidationDecider => Parent.ValidationDecider;
	}
}
