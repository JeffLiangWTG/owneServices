//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJPAFRBillsValidation
//
//    This class should be used for overriding validation in AutoJPAFRBillsValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using CurrencyCodes = Enterprise.Core.Constants.CurrencyCodes;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class JPAFRBillsValidation : AutoJPAFRBillsValidation
	{
		public JPAFRBillsValidation(AutoJPAFRBills parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				CheckNotificationForwardingParties();
				CheckOtherRelevantLawCodes();
				CheckContainersCount();
				ValidateAllCalculatedProperties();
			}
		}

		RefCountry RefCountryJapan
		{
			get { return refCountryJapan ?? (refCountryJapan = RefCountry.LoadFromCountryCode(Parent.Factory, Core.Constants.CountryCodes.Japan)); }
		}
		RefCountry refCountryJapan;

		protected new JPAFRBills Parent
		{
			get { return (JPAFRBills)base.Parent; }
		}

		protected override void CheckJPB_BillNumberIsWesternEuropean()
		{
			if (Parent.JPB_BillNumber.ExcludeChars(ValidationConstants.Constants.ValidNACCSCharactersForBillNumber).Length != 0)
			{
				Parent.JPB_BillNumberInfo.AddMessageError(ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);
			}
		}

		protected override void CheckJPB_BillNumber()
		{
			base.CheckJPB_BillNumber();
			var targetInfo = Parent.JPB_BillNumberInfo;
			var targetValue = Parent.JPB_BillNumber;
			var passingValidationSoFar = true;
			var parentHeader = Parent.Header;
			if (Parent.IsInDatabase)
			{
				ZString oldValue = (ZString)targetInfo.OriginalValue;
				if (targetValue != oldValue)
				{
					if (Parent.IsBillAlreadyRegistered)
					{
						targetInfo.AddError(ValidationConstants.Shared.ChangingFieldWhenBillIsRegistered(targetInfo.HumanReadableName, oldValue, targetValue));
						passingValidationSoFar = false;
					}
					else if (Parent.IsMessagingInProgress)
					{
						targetInfo.AddError(ValidationConstants.Shared.ChangingFieldWhenMessagingIsInProgress(targetInfo.HumanReadableName, oldValue, targetValue));
						passingValidationSoFar = false;
					}
				}
			}

			if (passingValidationSoFar)
			{
				if (targetValue.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				}
				else if (targetValue.Length < 6)
				{
					targetInfo.AddMessageError(ValidationConstants.Bill.BOLNumMissingNVOCCCode);
				}
				else if (parentHeader != null)
				{
					var isShippingLineEntry = parentHeader.JPH_IsShippingLineEntry;
					var companyPK = parentHeader.RegistryCompanyPK;
					var targetPrefix = isShippingLineEntry ? parentHeader.JPH_CarrierCode.ToString() : JPAFRRegistry.Instance.AFRNVOCCIDtoAddtoBillDuringSync.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);
					var bOLPrefix = targetValue.Left(4);

					if (!string.IsNullOrEmpty(targetPrefix) && bOLPrefix != targetPrefix.PadRight(4, '-'))
					{
						targetInfo.AddMessageError(isShippingLineEntry ? ValidationConstants.Header.MBOLNumDoesntMatchCarrierCode : ValidationConstants.Bill.BOLNumberNotStartWithNVOCCCodeInRegistry(targetPrefix));
					}
					else if (!bOLPrefix.IsValidBillPrefix())
					{
						targetInfo.AddMessageError(isShippingLineEntry ? ValidationConstants.Header.MBOLNumStartWithInvalidCarrierCode : ValidationConstants.Bill.BOLNumberStartWithInvalidNVOCCCode);
					}
					else
					{
						if (Parent.IsHBOLNumDuplicateWithinThisHeader())
						{
							targetInfo.AddMessageError(ValidationConstants.Bill.DuplicatedBillNumber);
						}

						if (Parent.HasHBOLReachedMaxAllowedWithinThisHeader())
						{
							targetInfo.AddMessageError(isShippingLineEntry ? ValidationConstants.Bill.BOLReachedMaxAllowed : ValidationConstants.Bill.HBOLReachedMaxAllowed);
						}
					}
				}
			}
		}

		void CheckNotificationForwardingParties()
		{
			if (Parent.NotificationForwardingParties.Count(notificationForwardingParty => !notificationForwardingParty.CY_Data.IsEmpty) > 3)
			{
				Parent.AddRowWarning(ValidationConstants.Bill.MaximumNotificationForwardingPartyExceeded);
			}
		}

		protected override void CheckJPB_RL_NKOrigin()
		{
			base.CheckJPB_RL_NKOrigin();
			var targetValue = Parent.JPB_RL_NKOrigin;
			var targetInfo = Parent.JPB_RL_NKOriginInfo;

			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JPB_RL_NKOriginInfo);
			var origin = Parent.Origin;
			if (origin != null && origin.RL_PortName.Length > ValidationConstants.Constants.PortNameMaxLength)
			{
				targetInfo.AddWarning(ValidationConstants.Shared.PortNameLengthExceeded);
			}

			if (!RefCountryJapan.ContainsUNLOCO(targetValue) && origin != null && origin.Country != null && RefCountryJapan.ContainsUNLOCO(origin))
			{
				targetInfo.AddWarning(ValidationConstants.Bill.InvalidPortOfOrigin);
			}
		}

		protected override void CheckJPB_RL_NKDelivery()
		{
			base.CheckJPB_RL_NKDelivery();
			var targetInfo = Parent.JPB_RL_NKDeliveryInfo;
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(targetInfo);
			var header = Parent.Header;
			var deliveryPort = Parent.Delivery;

			if (IsCustomsTransitIntended && header != null && header.JPH_RL_NKDischarge == Parent.JPB_RL_NKDelivery)
			{
				targetInfo.AddMessageError(ValidationConstants.Bill.DeliveryPortCannotEqualDischargePortForTranshipment);
			}
			else if (deliveryPort != null && deliveryPort.RL_PortName.Length > ValidationConstants.Constants.PortNameMaxLength)
			{
				targetInfo.AddWarning(ValidationConstants.Shared.PortNameLengthExceeded);
			}
		}

		protected override void CheckJPB_RL_NKFinalDestination()
		{
			base.CheckJPB_RL_NKFinalDestination();
			var targetInfo = Parent.JPB_RL_NKFinalDestinationInfo;
			ListValidation.MessageErrorIfInvalidCode(targetInfo);
			var finalDestination = Parent.FinalDestination;

			if (finalDestination != null && finalDestination.RL_PortName.Length > ValidationConstants.Constants.PortNameMaxLength)
			{
				targetInfo.AddWarning(ValidationConstants.Shared.PortNameLengthExceeded);
			}
		}

		protected override void CheckJPB_GoodsDescription()
		{
			base.CheckJPB_GoodsDescription();
			var targetValue = Parent.JPB_GoodsDescription;
			var targetInfo = Parent.JPB_GoodsDescriptionInfo;
			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			targetInfo.AddMessageErrorIfContainsInvalidNACCSCharacter(targetValue);
			var trimedValue = targetValue.Trim();
			if (trimedValue.Length < 3 || ValidationUtils.IsInappropriateGoodsDescription(trimedValue))
			{
				Parent.JPB_GoodsDescriptionInfo.AddWarning(ValidationConstants.Bill.InappropriateDescriptionOfGoods(targetValue));
			}
		}

		protected override void CheckJPB_Tariff()
		{
			base.CheckJPB_Tariff();
			var targetInfo = Parent.JPB_TariffInfo;
			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			var tariff = Parent.JPB_Tariff;
			if (!tariff.IsEmpty)
			{
				if ((tariff.Length != 6 || tariff.KeepNumericCharacters() != tariff))
				{
					targetInfo.AddMessageError(ValidationConstants.Bill.HSCodeInvalid);
				}
				else if (ValidationUtils.IsNonUniversalHSCode(tariff))
				{
					targetInfo.AddWarning(ValidationConstants.Bill.HSCodeNonUniversal);
				}
			}
		}

		protected override void CheckJPB_MarksAndNumbers()
		{
			base.CheckJPB_MarksAndNumbers();
			var targetInfo = Parent.JPB_MarksAndNumbersInfo;
			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			targetInfo.AddMessageErrorIfContainsInvalidNACCSCharacter(Parent.JPB_MarksAndNumbers);
		}

		protected override void CheckJPB_Remarks()
		{
			base.CheckJPB_Remarks();
			Parent.JPB_RemarksInfo.AddMessageErrorIfContainsInvalidNACCSCharacter(Parent.JPB_Remarks);
		}

		protected override void CheckJPB_ManifestQty()
		{
			base.CheckJPB_ManifestQty();
			var targetValue = Parent.JPB_ManifestQty;
			var targetInfo = Parent.JPB_ManifestQtyInfo;
			if (targetValue < 0)
			{
				targetInfo.AddError(ValidationConstants.Bill.ValueCannotBeNegative);
			}
			else if (targetValue == 0)
			{
				targetInfo.AddMessageError(ValidationConstants.Bill.ManifestQtyUseOneForUndescribable);
			}
			else if (targetValue > 99999999)
			{
				targetInfo.AddMessageError(ValidationConstants.Bill.ManifestQtyExceedingMaximumNumber);
			}
		}

		protected override void CheckJPB_ManifestUQ()
		{
			base.CheckJPB_ManifestUQ();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JPB_ManifestUQInfo);
		}

		protected override void CheckJPB_GrossWeight()
		{
			base.CheckJPB_GrossWeightUQ();
			var targetValue = Parent.JPB_GrossWeight;
			var targetInfo = Parent.JPB_GrossWeightInfo;
			if (targetValue < 0)
			{
				targetInfo.AddError(ValidationConstants.Bill.ValueCannotBeNegative);
			}
			else if (targetValue == 0)
			{
				targetInfo.AddMessageError(ValidationConstants.Bill.ValueEmpty(targetInfo.HumanReadableName));
			}
		}

		protected override void CheckJPB_GrossWeightUQ()
		{
			base.CheckJPB_GrossWeightUQ();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JPB_GrossWeightUQInfo);
		}

		protected override void CheckJPB_Volume()
		{
			base.CheckJPB_VolumeUQ();
			var targetValue = Parent.JPB_Volume;
			var targetInfo = Parent.JPB_VolumeInfo;
			if (targetValue < 0)
			{
				targetInfo.AddError(ValidationConstants.Bill.ValueCannotBeNegative);
			}
			else if (targetValue == 0)
			{
				targetInfo.AddMessageError(ValidationConstants.Bill.ValueEmpty(targetInfo.HumanReadableName));
			}
		}

		protected override void CheckJPB_VolumeUQ()
		{
			base.CheckJPB_VolumeUQ();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JPB_VolumeUQInfo);
		}

		protected override void CheckJPB_RN_NKGoodsOrigin()
		{
			base.CheckJPB_RN_NKGoodsOrigin();
			var targetInfo = Parent.JPB_RN_NKGoodsOriginInfo;
			ListValidation.MessageErrorIfInvalidCode(targetInfo);
			targetInfo.AddMessageErrorIfContainsInvalidNACCSCharacter(Parent.JPB_RN_NKGoodsOrigin);
		}

		protected override void CheckJPB_DG()
		{
			base.CheckJPB_DG();

			var billSubstance = Parent.Substance;
			if (billSubstance != null)
			{
				Parent.JPB_DGInfo.AddMessageErrorIfContainsInvalidNACCSCharacter(billSubstance.DG_ClassInfo, billSubstance.DG_Class);

				if (Parent.UNDGs.Any(c => c.DI_DG == billSubstance.PK))
				{
					Parent.JPB_DGInfo.AddMessageError(ValidationConstants.Bill.DuplicatedUNDG(billSubstance));
				}
			}
		}

		protected override void CheckJPB_DG_NKSubstance()
		{
			base.CheckJPB_DG_NKSubstance();
			var targetInfo = Parent.JPB_DG_NKSubstanceInfo;
			ListValidation.MessageErrorIfInvalidCode(targetInfo);
			targetInfo.AddMessageErrorIfContainsInvalidNACCSCharacter(Parent.JPB_DG_NKSubstance);
			var billSubstance = Parent.Substance;
			if (billSubstance != null)
			{
				targetInfo.AddMessageErrorIfContainsInvalidNACCSCharacter(billSubstance.DG_ClassInfo, billSubstance.DG_Class);
			}
		}

		protected override void CheckJPB_SpecialCargoCode()
		{
			base.CheckJPB_SpecialCargoCode();
			if (Parent.JPB_SpecialCargoCode.IsEmpty && !Parent.JPB_DG_NKSubstance.IsEmpty)
			{
				Parent.JPB_SpecialCargoCodeInfo.AddMessageError(ValidationConstants.Bill.SpecialCargoCodeIsRequiredForDangerousGoods);
			}
		}

		protected override void CheckJPB_FreightValue()
		{
			base.CheckJPB_FreightValue();
			var targetValue = Parent.JPB_FreightValue;
			var targetInfo = Parent.JPB_FreightValueInfo;
			if (targetValue < 0)
			{
				targetInfo.AddError(ValidationConstants.Bill.ValueCannotBeNegative);
			}
			else if (Parent.JPB_RX_NKFreightValueCurrency == CurrencyCodes.Japan
				&& targetValue.DecimalPlaces > 0)
			{
				targetInfo.AddWarning(ValidationConstants.Bill.FrightValueNoDecimalPartAllowedForJPY);
			}
			else if (targetValue.DecimalPlaces > 2)
			{
				targetInfo.AddWarning(ValidationConstants.Bill.FrightValueDecimalPartLimitForOtherCurrency);
			}
		}

		protected override void CheckJPB_RX_NKFreightValueCurrency()
		{
			base.CheckJPB_RX_NKFreightValueCurrency();
			var targetInfo = Parent.JPB_RX_NKFreightValueCurrencyInfo;
			if (!Parent.JPB_FreightValue.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(targetInfo);
				targetInfo.AddMessageErrorIfContainsInvalidNACCSCharacter(Parent.JPB_RX_NKFreightValueCurrency);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(targetInfo);
			}
		}

		protected override void CheckJPB_ContainerOperatorCode()
		{
			base.CheckJPB_ContainerOperatorCode();
			if (Parent.IsShippingLineEntry)
			{
				var targetValue = Parent.JPB_ContainerOperatorCode;
				var targetInfo = Parent.JPB_ContainerOperatorCodeInfo;
				if (!targetValue.IsEmpty)
				{
					AddMessageErrorIfNotDischargeInJapan(targetInfo);
					targetInfo.AddMessageErrorIfContainsInvalidNACCSCharacter(targetValue);
				}
			}
		}

		#region  Calculated Properties Validation

		void ValidateAllCalculatedProperties()
		{
			ValidateJPB_Calc_EFDT();
			ValidateJPB_Calc_ESDT();
			ValidateJPB_Calc_GoodsValue();
			ValidateJPB_Calc_RX_NKGoodsValueCurrency();
			ValidateJPB_Calc_TemporaryLandingDuration();
			ValidateJPB_Calc_TemporaryLandingReason();
			ValidateJPB_Calc_TransportMode();
			ValidateJPB_Calc_ArrivalBondedAreaCode();
			ValidateJPB_Calc_GeneralCustomsTransitApprovalNumber();
		}

		#region Validation Methods

		public void ValidateJPB_Calc_GoodsValue()
		{
			ValidateCalculatedProperty(Parent.JPB_Calc_GoodsValueInfo);
		}

		public void ValidateJPB_Calc_RX_NKGoodsValueCurrency()
		{
			ValidateCalculatedProperty(Parent.JPB_Calc_RX_NKGoodsValueCurrencyInfo);
		}

		public void ValidateJPB_Calc_TransportMode()
		{
			ValidateCalculatedProperty(Parent.JPB_Calc_TransportModeInfo);
		}

		public void ValidateJPB_Calc_ArrivalBondedAreaCode()
		{
			ValidateCalculatedProperty(Parent.JPB_Calc_ArrivalBondedAreaCodeInfo);
		}

		public void ValidateJPB_Calc_GeneralCustomsTransitApprovalNumber()
		{
			ValidateCalculatedProperty(Parent.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);
		}

		public void ValidateJPB_Calc_TemporaryLandingReason()
		{
			ValidateCalculatedProperty(Parent.JPB_Calc_TemporaryLandingReasonInfo);
		}

		public void ValidateJPB_Calc_TemporaryLandingDuration()
		{
			ValidateCalculatedProperty(Parent.JPB_Calc_TemporaryLandingDurationInfo);
		}

		public void ValidateJPB_Calc_ESDT()
		{
			ValidateCalculatedProperty(Parent.JPB_Calc_ESDTInfo);
		}

		public void ValidateJPB_Calc_EFDT()
		{
			ValidateCalculatedProperty(Parent.JPB_Calc_EFDTInfo);
		}

		#endregion
		#region Check Methoss
		#region Customs Transit Shared

		protected void CheckJPB_Calc_GoodsValue()
		{
			if (IsCustomsTransitIntended)
			{
				var targetInfo = Parent.JPB_Calc_GoodsValueInfo;
				var targetValue = Parent.JPB_Calc_GoodsValue;
				if (targetValue < 0)
				{
					targetInfo.AddError(ValidationConstants.Bill.ValueCannotBeNegative);
				}
				else if (targetValue.IsEmpty)
				{
					targetInfo.AddMessageError(ValidationConstants.InbondDetails.GoodValueCannotBeEmptyForTranshipment);
				}
				else
				{
					if (Parent.JPB_Calc_RX_NKGoodsValueCurrency == Enterprise.Core.Constants.CurrencyCodes.Japan
					&& targetValue.DecimalPlaces > 0)
					{
						targetInfo.AddWarning(ValidationConstants.InbondDetails.GoodValueNoDecimalPartAllowedFotJPY);
					}
					else if (targetValue.DecimalPlaces > 2)
					{
						targetInfo.AddWarning(ValidationConstants.InbondDetails.GoodValueDecimalPartLimitForOtherCurrency);
					}
					AddMessageErrorIfNotDischargeInJapan(targetInfo);
				}
			}
		}

		protected void CheckJPB_Calc_RX_NKGoodsValueCurrency()
		{
			var targetValue = Parent.JPB_Calc_RX_NKGoodsValueCurrency;
			var targetInfo = Parent.JPB_Calc_RX_NKGoodsValueCurrencyInfo;
			if (Parent.JPB_Calc_GoodsValue.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(targetInfo);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(targetInfo);
			}
			targetInfo.AddMessageErrorIfContainsInvalidNACCSCharacter(targetValue);
		}

		protected void CheckJPB_Calc_TransportMode()
		{
			if (IsCustomsTransitIntended)
			{
				var targetValue = Parent.JPB_Calc_TransportMode;
				var targetInfo = Parent.JPB_Calc_TransportModeInfo;
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(targetInfo, "TEST");
				if (!targetValue.IsEmpty)
				{
					AddMessageErrorIfNotDischargeInJapan(targetInfo);
				}
			}
		}

		protected void CheckJPB_Calc_ArrivalBondedAreaCode()
		{
			var targetValue = Parent.JPB_Calc_ArrivalBondedAreaCode;
			var targetInfo = Parent.JPB_Calc_ArrivalBondedAreaCodeInfo;
			if (IsCustomsTransitIntended)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}
			if (!targetValue.IsEmpty)
			{
				if (targetValue.Length != 5 || targetValue.KeepAlphanumericCharacters() != targetValue)
				{
					targetInfo.AddMessageError(ValidationConstants.InbondDetails.InvalidBondedAreaCode);
				}
				AddMessageErrorIfNotDischargeInJapan(targetInfo);
			}
		}

		#endregion

		#region Customs Transit of General Customs Transit

		protected void CheckJPB_Calc_GeneralCustomsTransitApprovalNumber()
		{
			var targetInfo = Parent.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo;
			var targetValue = Parent.JPB_Calc_GeneralCustomsTransitApprovalNumber;
			if (IsCustomsTransitIntended)
			{
				if (IsGeneralCustomsTransitIntended)
				{
					targetInfo.AddMessageErrorIfContainsInvalidNACCSCharacter(targetValue);
					AddMessageErrorIfCOCEmptyForVOCC(targetInfo);
					AddMessageErrorIfNotDischargeInJapan(targetInfo);
				}
				else if (!IsTemporaryLandingIntended && Parent.IsShippingLineEntry && targetValue.IsEmpty)
				{
					targetInfo.AddMessageError(ValidationConstants.InbondDetails.ValueMissingWhenGeneralCustomsTransitIntended);
				}
			}
		}

		#endregion
		#region Customs Transit of Temporary Landing

		protected void CheckJPB_Calc_TemporaryLandingReason()
		{
			var targetInfo = Parent.JPB_Calc_TemporaryLandingReasonInfo;
			var targetValue = Parent.JPB_Calc_TemporaryLandingReason;
			CheckTemporaryLandingInfoWithIntentionPredict(targetInfo, targetValue, () =>
			{
				ListValidation.MessageErrorIfInvalidCode(targetInfo);
			});
		}

		protected void CheckJPB_Calc_TemporaryLandingDuration()
		{
			var targetValue = Parent.JPB_Calc_TemporaryLandingDuration;
			var targetInfo = Parent.JPB_Calc_TemporaryLandingDurationInfo;
			if (targetValue < 0)
			{
				targetInfo.AddError(ValidationConstants.Bill.ValueCannotBeNegative);
			}
			else
			{
				CheckTemporaryLandingInfoWithIntentionPredict(targetInfo, targetValue, () => { }, ValidationConstants.InbondDetails.TemporaryLandingDurationZeroIsNotAllowed);
			}
		}

		protected void CheckJPB_Calc_ESDT()
		{
			var targetInfo = Parent.JPB_Calc_ESDTInfo;
			var targetValue = Parent.JPB_Calc_ESDT;
			CheckTemporaryLandingInfoWithIntentionPredict(targetInfo, targetValue, () =>
			{
				if (!Parent.InBondDetails.IsInDatabase && targetValue < ValidationUtils.GetCurrentJPDate)
				{
					targetInfo.AddMessageError(ValidationConstants.InbondDetails.EstimatesDateShouldBeAfterSystemDate);
				}
			});
		}

		protected void CheckJPB_Calc_EFDT()
		{
			var targetInfo = Parent.JPB_Calc_EFDTInfo;
			var targetValue = Parent.JPB_Calc_EFDT;
			CheckTemporaryLandingInfoWithIntentionPredict(targetInfo, targetValue, () =>
			{
				if (!Parent.InBondDetails.IsInDatabase && targetValue < ValidationUtils.GetCurrentJPDate)
				{
					targetInfo.AddMessageError(ValidationConstants.InbondDetails.EstimatesDateShouldBeAfterSystemDate);
				}

				var eSDTValue = Parent.JPB_Calc_ESDT;
				if (!eSDTValue.IsEmpty && targetValue < eSDTValue)
				{
					targetInfo.AddMessageError(ValidationConstants.InbondDetails.EFDTShouldBeAfterESDT);
				}
			});
		}

		#endregion

		#endregion

		#region Util

		bool IsTemporaryLandingIntended
		{
			get
			{
				return !(Parent.JPB_Calc_TemporaryLandingReason.IsEmpty
						&& Parent.JPB_Calc_TemporaryLandingDuration.IsEmpty
						&& Parent.JPB_Calc_ESDT.IsEmpty
						&& Parent.JPB_Calc_EFDT.IsEmpty);
			}
		}

		bool IsGeneralCustomsTransitIntended
		{
			get { return (Parent.IsShippingLineEntry && !Parent.JPB_Calc_GeneralCustomsTransitApprovalNumber.IsEmpty); }
		}

		bool IsCustomsTransitIntended
		{
			get
			{
				return !(
				Parent.JPB_Calc_GoodsValue.IsEmpty
				&& Parent.JPB_Calc_TransportMode.IsEmpty
				&& Parent.JPB_Calc_ArrivalBondedAreaCode.IsEmpty
				) ||
				HasOtherRelevantLawCodes ||
				IsGeneralCustomsTransitIntended ||
				IsTemporaryLandingIntended;
			}
		}

		bool HasOtherRelevantLawCodes
		{
			get { return Parent.OtherRelevantLaws.Any(relevantLaw => !relevantLaw.CY_Data.IsEmpty); }
		}

		void AddMessageErrorIfNotDischargeInJapan(ZPropertyInfo targetInfo)
		{
			if (!RefCountryJapan.ContainsUNLOCO(Parent.DischargePortCode))
			{
				targetInfo.AddMessageError(ValidationConstants.InbondDetails.EntryInvalidAsNotDischargeInJapan(targetInfo.HumanReadableName));
			}
		}

		protected void AddMessageErrorIfCOCEmptyForVOCC(ZPropertyInfo targetInfo)
		{
			if (Parent.IsShippingLineEntry)
			{
				var cocCode = Parent.JPB_ContainerOperatorCode;
				if (cocCode.IsEmpty || cocCode == ValidationConstants.Constants.ContainerOperatorBasketCode)
				{
					targetInfo.AddMessageError(ValidationConstants.Bill.EntryInvalidAsCOCCodeNotValid(targetInfo.HumanReadableName));
				}
			}
		}

		protected void CheckTemporaryLandingInfoWithIntentionPredict(ZPropertyInfo targetInfo, IZType targetValue, Action normalValidation, string emptyValueMessageOverride = null)
		{
			bool isTargetValueEmpty = targetValue.IsEmpty;
			if (IsCustomsTransitIntended)
			{
				if (IsGeneralCustomsTransitIntended)
				{
					if (!isTargetValueEmpty)
					{
						targetInfo.AddMessageError(ValidationConstants.InbondDetails.NoValueAllowedWhenGeneralCustomsTransitIntended);
					}
				}
				else if (isTargetValueEmpty)
				{
					targetInfo.AddMessageError(emptyValueMessageOverride ?? ValidationConstants.InbondDetails.ValueMissingWhenTemporaryLandingIntended(targetInfo.HumanReadableName));
				}
				else
				{
					normalValidation();
				}
			}
		}

		#endregion

		#endregion

		#region Check Collection

		void CheckOtherRelevantLawCodes()
		{
			if (Parent.OtherRelevantLaws.Count(relevantLaw => !relevantLaw.CY_Data.IsEmpty) > 5)
			{
				Parent.AddRowWarning(ValidationConstants.Bill.MaximumOtherRelevantLawExceeded);
			}
		}

		void CheckContainersCount()
		{
			var containerCount = Parent.Containers.Count;

			if (containerCount == 0)
			{
				Parent.AddRowMessageError(ValidationConstants.Bill.AtLeastOneContainerIsRequired);
			}
			else if (containerCount > 100)
			{
				Parent.AddRowMessageError(ValidationConstants.Bill.MaximumContainersCountExceeded);
			}
		}

		#endregion

	}
}
