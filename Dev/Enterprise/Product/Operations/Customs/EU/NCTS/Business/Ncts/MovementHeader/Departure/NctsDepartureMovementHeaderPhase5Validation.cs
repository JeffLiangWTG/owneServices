using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.NCTS.Business.Ncts.Common;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.EU.NCTS.Business.NctsConstants;
using CusAuthorizationHeaderTypeList = Enterprise.Customs.Business.CusAuthorizationHeaderTypeList;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsDepartureMovementHeaderPhase5Validation(NctsDepartureMovementHeader parent)
		: NctsDepartureMovementHeaderValidation(parent)
	{
		protected new INctsDepartureMovementHeaderPhase5ValidationDecider ValidationDecider => (INctsDepartureMovementHeaderPhase5ValidationDecider)base.ValidationDecider;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateAdditionalTransportAtBorderListCount();
			ValidateAuthorizations();
			ValidateGuarantees();
		}

		public void ValidateGuarantees()
		{
			var movementHeader = Parent;
			var header = Parent.Header;
			var messageErrorForTR0086 = header.Configuration.ValidationRuleConfiguration.Messages.TR0086Message;
			var guarantees = movementHeader.Guarantees;
			guarantees.ForEach(x => x.ClearRowNotificationsContaining(messageErrorForTR0086));

			bool allGuaranteeFractionZero() => guarantees.All(x => x.PW_SuretyCode == LiabilityApplicablePercentageCodeList.Codes.ZER);

			var isRuleTR0086Active = movementHeader.ValidationDecider is INctsDepartureMovementHeaderPhase5ValidationDecider validation && validation.IsRuleTR0086Active;

			if (isRuleTR0086Active && !allGuaranteeFractionZero() && guarantees.Any(x => x.PW_Override))
			{
				var addWarning = false;
				if (guarantees.Any(x => x.PW_SuretyCode == LiabilityApplicablePercentageCodeList.Codes.ZER))
				{
					addWarning = true;
				}
				else
				{
					var roundingScale = header.Company.LocalCurrency.Decimals;
					var liabilitySum = guarantees.Sum(x => x.GetTotalAmountWithRevertedFactor());

					var goodItems = header.Bills.SelectMany(x => x.GoodsItems).ToArray();
					var goodsItemsSum = header.ApportionedAmount;
					addWarning = liabilitySum != goodsItemsSum;
				}

				if (addWarning)
				{
					guarantees.ForEach(x => x.AddRowWarning(messageErrorForTR0086));
				}
			}
		}

		protected override void CheckBM_TypeOfSecurity()
		{
			base.CheckBM_TypeOfSecurity();
			var parent = Parent;
			var targetInfo = parent.BM_TypeOfSecurityInfo;
			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			ListValidation.ErrorIfInvalidCode(targetInfo);

			var header = parent.Header;
			if (header is null)
			{
				return;
			}

			var securityIsEXIorBTH = parent.IsSecurityTypeBTHOrEXI;
			CheckBM_TypeOfSecurityRuleTR0048(targetInfo, header, securityIsEXIorBTH);
			CheckBM_TypeOfSecurityRuleTR0053(targetInfo, header, securityIsEXIorBTH);

			var securityTypeIsENTOrBTHOrEXI = parent.IsSecurityTypeENTOrBTHOrEXI;
			var countriesOfRoutingCount = header.CountriesOfRouting.Count;
			CheckBM_TypeOfSecurityRuleC0586AndB1848(targetInfo, securityTypeIsENTOrBTHOrEXI, countriesOfRoutingCount);
			CheckBM_TypeOfSecurityRuleB1848_1(targetInfo, securityTypeIsENTOrBTHOrEXI, countriesOfRoutingCount);

			CheckBM_TypeOfSecurityRuleNR0018(targetInfo);
			CheckBM_TypeOfSecurityRuleBR5410(targetInfo);
		}

		protected override void CheckBM_AircraftIDAtDeparture()
		{
			base.CheckBM_AircraftIDAtDeparture();

			var parent = Parent;
			if (!parent.BM_AircraftIDAtDeparture.IsEmpty && !parent.BM_TransportAtDeparture.IsEmpty)
			{
				parent.BM_AircraftIDAtDepartureInfo.AddMessageError(YouMayOnlyEnterEitherFlightNumberOrAircraftIDMessage);
			}
			UniversalValidationHelper.CheckMaxLengthIfPhase5TransitionPeriod(IsInPhase5TransitionPeriod, parent.BM_AircraftIDAtDepartureInfo, 27);

			CheckRuleNR0031();

			void CheckRuleNR0031()
			{
				if (ValidationDecider is INctsDepartureMovementHeaderPhase5ValidationDecider { IsRuleNR0031Active: true }
					&& Parent.BM_InlandTransportMode == ModeOfTransportList.Codes._4_AirTransport && Parent.BM_TransportAtDeparture.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_AircraftIDAtDepartureInfo, messagePrefix: ValidationRuleConfiguration.Messages.NR0031RuleCode.GetRuleCodeMessagePrefix(true));
				}
			}
		}

		protected override void CheckBM_GrossWeight()
		{
			var parent = Parent;
			if (ValidationDecider is { IsRuleR0994Active: true }
				&& !parent.IsGrossWeightValid)
			{
				parent.BM_GrossWeightInfo.AddMessageError(ValidationRuleConfiguration.Messages.R0994Message);
			}

			if (ValidationDecider?.IsRuleE1109Active ?? false)
			{
				var message = Parent.Header.Configuration.ValidationRuleConfiguration.Messages.E1109Message(Parent.BM_GrossWeightInfo.Description);
				UniversalValidationHelper.CheckMaxValueAndMaxDecimalLengthForWeightIfPhase5TransitionPeriod(IsInPhase5TransitionPeriod, message, parent.BM_GrossWeightInfo, 11, 3);
			}

			CheckRuleR0994_1(parent);
		}

		void CheckRuleR0994_1(NctsDepartureMovementHeader parent)
		{
			if (!parent.IsDeparturePhase5RuleActive(x => x.IsRuleR0994_1Active))
			{
				return;
			}

			var totalWeightOfBills = parent.TotalWeightOfBills;
			if (parent.GrossWeight != totalWeightOfBills)
			{
				parent.BM_GrossWeightInfo.AddWarning(ValidationRuleConfiguration.Messages.R0994_1Message(totalWeightOfBills.InKilogramsSafe.ToStringTrimZeros()));
			}
		}

		protected override ZBool IsExportDateMandatory => base.IsExportDateMandatory && Parent.BM_AdditionalDeclarationType != NctsTypeOfAdditionalDeclarationList.Codes.D && ValidationRuleConfiguration.IsRuleC0839Active;

		protected override ZString ExportDateMandatoryMessagePrefix => NctsConstants.ValidationRuleMessagePrefixes.C0839;

		protected override void CheckBM_TransportAtDeparture()
		{
			base.CheckBM_TransportAtDeparture();

			var parent = Parent;
			var value = parent.BM_TransportAtDeparture;
			var targetInfo = parent.BM_TransportAtDepartureInfo;

			if (!value.IsEmpty)
			{
				if (!ValidateVesselNameAtDepartureE1103())
				{
					UniversalValidationHelper.CheckMaxLengthIfPhase5TransitionPeriod(IsInPhase5TransitionPeriod, targetInfo, 27);
				}
			}
			else
			{
				if (!parent.BM_TransportAtDepartureType.IsEmpty && parent.ValidationDecider is INctsDepartureMovementHeaderPhase5ValidationDecider { IsRuleTR0049Active: true })
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo, messagePrefix: "[TR0049] ");
				}

				if (ValidationDecider is { IsRuleTR0054Active: true } && parent.BM_InlandTransportMode == ModeOfTransportList.Codes._2_RailTransport && parent.AdditionalWagons.Count > 0)
				{
					targetInfo.AddMessageError(Res.GetString("ACB4342E-835C-47D9-94A8-47B26B324B86", "[TR0054] Please capture first Transport ID in field Wagon No./Train No. before using grid Additional Wagon Numbers."));
				}

				if (IsB1892Applicable
					&& parent.Header.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>().All(c => !c.IsContainerised))
				{
					targetInfo.AddMessageError(ValidationRuleConfiguration.Messages.B1892Message(targetInfo.HumanReadableName));
				}
			}

			R0473Validation.CheckTransportAtDeparture(parent.BM_TransportAtDepartureType, targetInfo);

			var nctsBillDeparturePhase5ValidationDecider = Parent.Header?.Configuration.BillConfiguration.GetValidationDecider(Parent.Header) as INctsBillDeparturePhase5ValidationDecider;
			new NctsPhase5RuleR0474_1Validation(parent, parent.Header).ValidateTransportAtDeparture(targetInfo, nctsBillDeparturePhase5ValidationDecider);

			CheckRuleNR0031();
			B2101Validation.ValidateBM_TransportAtDeparture();

			CheckRuleN0002(targetInfo);

			void CheckRuleNR0031()
			{
				if (ValidationDecider is INctsDepartureMovementHeaderPhase5ValidationDecider { IsRuleNR0031Active: true }
					&& (Parent.BM_InlandTransportMode == ModeOfTransportList.Codes._2_RailTransport
					|| Parent.BM_InlandTransportMode == ModeOfTransportList.Codes._3_RoadTransport
					|| (Parent.BM_InlandTransportMode == ModeOfTransportList.Codes._4_AirTransport && Parent.BM_AircraftIDAtDeparture.IsEmpty)
					|| Parent.BM_InlandTransportMode == ModeOfTransportList.Codes._8_InlandWaterwayTransport
					|| Parent.BM_InlandTransportMode == ModeOfTransportList.Codes._9_OwnPropulsion
					))
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo, messagePrefix: ValidationRuleConfiguration.Messages.NR0031RuleCode.GetRuleCodeMessagePrefix(true));
				}
			}
		}

		bool IsB1892Applicable => (ValidationDecider?.IsRuleB1892Active ?? false) && IsInPhase5TransitionPeriod;

		bool ValidateVesselNameAtDepartureE1103()
		{
			var parent = Parent;

			const int vesselNameAtDepartureMaxLength = 27;

			if (parent.VesselNameAtDeparture.Length > vesselNameAtDepartureMaxLength
				&& parent.IsPhase5Departure
				&& parent.IsInlandWaterwayInlandTransport
				&& parent.BM_TransportAtDepartureType == NctsTransportTypeOfIdList.Codes._81
				&& IsInPhase5TransitionPeriod
				&& ValidationDecider.IsRuleE1103Active)
			{
				parent.BM_TransportAtDepartureInfo.AddMessageError(Res.GetString("EA00A5FD-61EF-499F-B673-968BA2A888C9", "[E1103] Vessel Name should be less than or equal to {0} Char.", vesselNameAtDepartureMaxLength));
				return true;
			}

			return false;
		}

		protected override void CheckBM_RN_NKTransportAtDepartureCountry()
		{
			base.CheckBM_RN_NKTransportAtDepartureCountry();

			var parent = Parent;
			var targetPropertyInfo = parent.BM_RN_NKTransportAtDepartureCountryInfo;
			if (IsRuleTR0050Applicable && !parent.BM_TransportAtDeparture.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetPropertyInfo, messagePrefix: ValidationRuleConfiguration.Messages.TR0050RuleCode.GetRuleCodeMessagePrefix(true));
			}

			if (parent.IsDeparturePhase5RuleActive(x => x.IsRuleNR0035Active)
				&& parent.InlandTransportModeAtDeparture != ModeOfTransportList.Codes._1_SeaTransport
				&& parent.InlandTransportModeAtDeparture != ModeOfTransportList.Codes._2_RailTransport)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetPropertyInfo, messagePrefix: ValidationRuleConfiguration.Messages.NR0035RuleCode.GetRuleCodeMessagePrefix(true));
			}

			Phase5RuleB1897Validator.ValidateBM_RN_NKTransportAtDepartureCountry();
			B2101Validation.ValidateBM_RN_NKTransportAtDepartureCountry();
			CheckRuleN0002(targetPropertyInfo);
		}

		protected virtual bool IsRuleTR0050Applicable => ValidationDecider is { IsRuleTR0050Active: true };

		protected override void CheckBM_RN_NKTransportAtDepartureTrailer1Nationality()
		{
			base.CheckBM_RN_NKTransportAtDepartureTrailer1Nationality();

			var parent = Parent;
			var targetPropertyInfo = parent.BM_RN_NKTransportAtDepartureTrailer1NationalityInfo;
			Phase5RuleB1897Validator.ValidateBM_RN_NKTransportAtDepartureTrailer1Nationality();
			B2101Validation.ValidateBM_RN_NKTransportAtDepartureTrailer1Nationality();

			if (IsRuleB1897AndRuleB2101NotApplicable
				&& !parent.BM_TransportAtDepartureTrailer1RegNo.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetPropertyInfo);
			}

			CheckRuleN0002(targetPropertyInfo);
		}

		protected override void CheckBM_RN_NKTransportAtDepartureTrailer2Nationality()
		{
			base.CheckBM_RN_NKTransportAtDepartureTrailer2Nationality();

			var parent = Parent;
			var targetPropertyInfo = parent.BM_RN_NKTransportAtDepartureTrailer2NationalityInfo;
			Phase5RuleB1897Validator.ValidateBM_RN_NKTransportAtDepartureTrailer2Nationality();
			B2101Validation.ValidateBM_RN_NKTransportAtDepartureTrailer2Nationality();

			if (IsRuleB1897AndRuleB2101NotApplicable
				&& !parent.BM_TransportAtDepartureTrailer2RegNo.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetPropertyInfo);
			}

			CheckRuleN0002(targetPropertyInfo);
		}

		protected override void CheckBM_AdditionalDeclarationType()
		{
			base.CheckBM_AdditionalDeclarationType();

			var targetInfo = Parent.BM_AdditionalDeclarationTypeInfo;
			ListValidation.MessageErrorIfInvalidCode(targetInfo);
			NctsHeader.CheckConditionR0520(targetInfo);
			CheckRuleTR0017();
			CheckRuleNR0073();

			void CheckRuleTR0017()
			{
				if ((ValidationDecider?.IsRuleTR0017Active ?? false) && Configuration.UseAdditionalDeclarationType)
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo, messagePrefix: NctsConstants.ValidationRuleMessagePrefixes.TR0017);
				}
			}

			void CheckRuleNR0073()
			{
				if ((ValidationDecider?.IsRuleNR0073Active ?? false)
					&& Parent.BM_AdditionalDeclarationType != NctsTypeOfAdditionalDeclarationList.Codes.D
					&& Parent.GoodsLocation.CGL_Type.IsEmpty
					&& Parent.GoodsLocation.CGL_Qualifier.IsEmpty
					&& Parent.GoodsLocation.AdditionalIdentifier.IsEmpty
					&& Parent.GoodsLocation.Address.E2_Postcode.IsEmpty
					&& Parent.GoodsLocation.Address.E2_RN_NKCountryCode.IsEmpty)
				{
					targetInfo.AddMessageError(ValidationRuleConfiguration.Messages.NR0073Message);
				}
			}
		}

		protected override void CheckBM_InlandTransportMode()
		{
			base.CheckBM_InlandTransportMode();
			CheckBM_InlandTransportMode_B1091Rule();

			var parent = Parent;
			parent.Validation.ValidateBM_ReducedDatasetIndicator();
		}

		protected override void CheckBM_TransportAtDepartureTrailer1RegNo()
		{
			base.CheckBM_TransportAtDepartureTrailer1RegNo();

			var parent = Parent;
			var transportAtDepartureTrailer1RegNoIsEmpty = parent.BM_TransportAtDepartureTrailer1RegNo.IsEmpty;
			var targetInfo = parent.BM_TransportAtDepartureTrailer1RegNoInfo;
			if (parent.BM_InlandTransportMode == ModeOfTransportList.Codes._2_RailTransport)
			{
				if (!transportAtDepartureTrailer1RegNoIsEmpty && !parent.BM_TransportAtDeparture.IsEmpty && IsInPhase5TransitionPeriod)
				{
					targetInfo.AddMessageError(Res.GetString("1594B140-A463-44FE-9035-6477178F6065", "You may only enter either a Train Number or a Wagon Number."));
				}
				else if (!IsInPhase5TransitionPeriod
						&& transportAtDepartureTrailer1RegNoIsEmpty
						&& ShouldValidateInlandTransportList())
				{
					MandatoryValidation.AddYouHaveNotEnteredMessage(targetInfo, Res.GetString("5C45DD81-D8DF-4C88-A409-2CE8FB9B2055", "Wagon Number"));
				}
			}

			UniversalValidationHelper.CheckMaxLengthIfPhase5TransitionPeriod(IsInPhase5TransitionPeriod, targetInfo, 27);
			B2101Validation.ValidateBM_TransportAtDepartureTrailer1RegNo();

			CheckRuleN0002(targetInfo);
		}

		protected virtual bool ShouldValidateInlandTransportList() => Parent.InlandTransportList.Any();

		protected override void CheckBM_TransportAtDepartureTrailer2RegNo()
		{
			base.CheckBM_TransportAtDepartureTrailer1RegNo();

			var parent = Parent;
			var targetPropertyInfo = parent.BM_TransportAtDepartureTrailer2RegNoInfo;

			UniversalValidationHelper.CheckMaxLengthIfPhase5TransitionPeriod(IsInPhase5TransitionPeriod, targetPropertyInfo, 27);
			B2101Validation.ValidateBM_TransportAtDepartureTrailer2RegNo();
			CheckRuleN0002(targetPropertyInfo);
		}

		protected override void CheckBM_TransportAtDepartureType()
		{
			base.CheckBM_TransportAtDepartureType();

			var parent = Parent;
			var targetInfo = parent.BM_TransportAtDepartureTypeInfo;

			if (parent.BM_TransportAtDepartureType == NctsTransportTypeOfIdList.Codes._99)
			{
				ListValidation.MessageErrorIfInvalidCode(targetInfo);
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(targetInfo);
			}

			CheckBM_TransportAtDepartureType_RuleB1891_1();

			CheckRuleNR0031();
			B2101Validation.ValidateBM_TransportAtDepartureType();

			CheckRuleN0002(targetInfo);

			void CheckRuleNR0031()
			{
				if (ValidationDecider is INctsDepartureMovementHeaderPhase5ValidationDecider { IsRuleNR0031Active: true }
					&& Parent.BM_InlandTransportMode == ModeOfTransportList.Codes._9_OwnPropulsion)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_TransportAtDepartureTypeInfo, messagePrefix: ValidationRuleConfiguration.Messages.NR0031RuleCode.GetRuleCodeMessagePrefix(true));
				}
			}
		}

		protected virtual void CheckBM_TransportAtDepartureType_RuleB1891_1()
		{
			var parent = Parent;
			if (parent.BM_TransportAtDepartureType.IsEmpty &&
				ValidationDecider.IsRuleB1891_1Active &&
				IsInPhase5TransitionPeriod &&
				parent.Header.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>().All(c => !c.IsContainerised))
			{
				parent.BM_TransportAtDepartureTypeInfo.AddMessageError(ValidationRuleConfiguration.Messages.B1891_1Message);
			}
		}

		protected override void CheckBM_ActiveBorderIdentificationType()
		{
			base.CheckBM_ActiveBorderIdentificationType();

			var parent = Parent;
			var targetInfo = parent.BM_ActiveBorderIdentificationTypeInfo;

			if (parent.BM_ActiveBorderIdentificationType == NctsTransportTypeOfIdList.Codes._99)
			{
				ListValidation.MessageErrorIfInvalidCode(targetInfo);
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(targetInfo);
			}

			if (parent.BM_ActiveBorderIdentificationType.IsEmpty)
			{
				if (ValidationDecider.IsRuleR0789Active
					&& BorderMethodOfTransportDetailsIsVisible
					&& parent.CustomsOfficesForDeparture.ContainsCode(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit))
				{
					targetInfo.AddMessageError(ValidationRuleConfiguration.Messages.R0789Message);
				}

				if (parent.AdditionalTransportAtBorderList.Any())
				{
					targetInfo.AddError(Res.GetString("FECC1C68-E94E-4A06-86A9-4F627880FEF5", "Enter the first Transport in the fields of Transport Border"));
				}

				if (ValidationDecider.IsRuleC0806Active
					&& BorderMethodOfTransportDetailsIsVisible
					&& CheckRuleC0806_NotInPhase5TransitionPeriod())
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo, messagePrefix: ValidationRuleConfiguration.Messages.C0806RuleCode.GetRuleCodeMessagePrefix(true));
				}

				if (ValidationDecider is { IsRuleB1806Active: true } && IsInPhase5TransitionPeriod && parent.BM_ExportTransportMode != ModeOfTransportList.Codes._5_PostalConsignment)
				{
					targetInfo.AddMessageError(Res.GetString("6C8F97BD-48B7-47E0-8105-2C7D74D657FA", "[B1806] You have not entered a Type Of Identification."));
				}

				if (CheckRuleB1838(parent))
				{
					targetInfo.AddMessageError(Res.GetString("7720656D-B569-4DFD-BCE3-1E17F332C062", $"{NctsConstants.ValidationRuleMessagePrefixes.B1838}Type of ID can't be empty."));
				}
			}
			B2101Validation.ValidateBM_ActiveBorderIdentificationType();
		}

		protected bool BorderMethodOfTransportDetailsIsVisible => NctsHelper.ShowExportTransportModeDetails(Parent);

		protected override void CheckBM_AircraftIDAtBorder()
		{
			base.CheckBM_AircraftIDAtBorder();

			var parent = Parent;
			UniversalValidationHelper.CheckMaxLengthIfPhase5TransitionPeriod(IsInPhase5TransitionPeriod, parent.BM_AircraftIDAtBorderInfo, 27);
		}

		protected override void CheckBM_TOLCarrierID()
		{
			base.CheckBM_TOLCarrierID();

			var parent = Parent;
			var value = parent.BM_TOLCarrierID;
			var targetInfo = parent.BM_TOLCarrierIDInfo;

			if (value.IsEmpty)
			{
				if (CheckRuleB1838(parent))
				{
					targetInfo.AddMessageError(Res.GetString("4D3672DD-F7F8-4EE1-A097-F8FC51D4A481", $"{NctsConstants.ValidationRuleMessagePrefixes.B1838}Transport ID can't be empty."));
				}

				if (!parent.BM_ExportTransportMode.IsEmpty && parent.BM_ExportTransportMode != ModeOfTransportList.Codes._5_PostalConsignment)
				{
					if (ValidationDecider.IsRuleC0806Active
						&& CheckRuleC0806_NotInPhase5TransitionPeriod())
					{
						MandatoryValidation.MessageErrorIfNotEntered(targetInfo, messagePrefix: ValidationRuleConfiguration.Messages.C0806RuleCode.GetRuleCodeMessagePrefix(true));
					}
					else if (!(ValidationDecider?.IsRuleB2101Active ?? false))
					{
						MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
					}
				}
			}
			else
			{
				UniversalValidationHelper.CheckMaxLengthIfPhase5TransitionPeriod(IsInPhase5TransitionPeriod, targetInfo, 27);
			}

			B2101Validation.ValidateBM_TOLCarrierID();

			R0473Validation.CheckTransportAtBorder(parent.BM_ActiveBorderIdentificationType, parent.BM_TOLCarrierIDInfo);
		}

		protected override void CheckBM_RN_NKTOLCarrierNationality()
		{
			base.CheckBM_RN_NKTOLCarrierNationality();

			var parent = Parent;
			var targetInfo = parent.BM_RN_NKTOLCarrierNationalityInfo;
			if (!parent.BM_ExportTransportMode.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(targetInfo);
			}

			if (ValidationDecider.IsRuleC0806Active)
			{
				if (CheckRuleC0806_NotInPhase5TransitionPeriod() && BorderMethodOfTransportDetailsIsVisible)
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo, messagePrefix: ValidationRuleConfiguration.Messages.C0806RuleCode.GetRuleCodeMessagePrefix(true));
				}
			}
			else
			{
				if (!(ValidationDecider?.IsRuleB2101Active ?? false) && !parent.BM_ExportTransportMode.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				}
			}

			CheckRuleB1850(targetInfo);
			B2101Validation.ValidateBM_RN_NKTOLCarrierNationality();
		}

		void CheckRuleB1850(ZPropertyInfo targetInfo)
		{
			var parent = Parent;
			var transportMode = parent.BM_ExportTransportMode;
			if (ValidationDecider is { IsRuleB1850Active: true }
			&& IsInPhase5TransitionPeriod
				&& !transportMode.IsEmpty
				&& transportMode != ModeOfTransportList.Codes._2_RailTransport
				&& parent.BM_RN_NKTOLCarrierNationality.IsEmpty)
			{
				targetInfo.AddMessageError(ValidationRuleConfiguration.Messages.B1850Message);
			}
		}

		protected override void CheckBM_ConveyanceNumber()
		{
			base.CheckBM_ConveyanceNumber();

			var parent = Parent;
			var targetInfo = parent.BM_ConveyanceNumberInfo;
			var conveyanceNumber = parent.BM_ConveyanceNumber;

			if (CheckRuleC0531())
			{
				TransportMeansValidationHelper.CheckReferenceNumber(parent, Parent.BM_ActiveBorderIdentificationType, targetInfo);
			}

			if (parent.BM_ExportTransportMode == ModeOfTransportList.Codes._4_AirTransport && !conveyanceNumber.IsEmpty && NctsHeader.IsRuleActive(x => x.IsRuleR0315Active))
			{
				if (conveyanceNumber.Length > 8)
				{
					parent.BM_ConveyanceNumberInfo.AddMessageError(ValidationRuleConfiguration.Messages.R0315aMessage);
				}
				else if (!conveyanceNumber.IsLettersAndNumbersOnlyOrEmpty)
				{
					parent.BM_ConveyanceNumberInfo.AddMessageError(ValidationRuleConfiguration.Messages.R0315bMessage);
				}
				else if (((string)conveyanceNumber).Any(char.IsLower))
				{
					parent.BM_ConveyanceNumberInfo.AddMessageError(ValidationRuleConfiguration.Messages.R0315cMessage);
				}
			}
		}

		bool CheckRuleC0531()
		{
			var result = true;
			var parent = Parent;
			if (ValidationDecider.IsRuleC0531Active
				&& parent.BM_ConveyanceNumber.IsEmpty
				&& parent.BM_ExportTransportMode == ModeOfTransportList.Codes._4_AirTransport
				&& NctsHelper.IsSecurityTypeBTHOrEXIOrENT(parent.BM_TypeOfSecurity))
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.BM_ConveyanceNumberInfo, messagePrefix: ValidationRuleConfiguration.Messages.C0531RuleCode.GetRuleCodeMessagePrefix(true));
				result = false;
			}
			return result;
		}

		bool CheckRuleB1838(NctsDepartureMovementHeader parent)
		{
			return (ValidationDecider?.IsRuleB1838Active ?? false)
				&& IsInPhase5TransitionPeriod
				&& (parent.BM_ExportTransportMode == ModeOfTransportList.Codes._2_RailTransport || !parent.BM_RN_NKTOLCarrierNationality.IsEmpty);
		}

		protected override void CheckBM_CustomsOfficeAtBorder()
		{
			base.CheckBM_CustomsOfficeAtBorder();

			var parent = Parent;
			var targetInfo = parent.BM_CustomsOfficeAtBorderInfo;
			var customsOfficeAtBorder = parent.BM_CustomsOfficeAtBorder;
			var exportTransportMode = parent.BM_ExportTransportMode;

			if (customsOfficeAtBorder.IsEmpty
				&& !exportTransportMode.IsEmpty
				&& exportTransportMode != ModeOfTransportList.Codes._5_PostalConsignment)
			{
				if (ValidationDecider.IsRuleC0806Active
					&& CheckRuleC0806_NotInPhase5TransitionPeriod())
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo, messagePrefix: ValidationRuleConfiguration.Messages.C0806RuleCode.GetRuleCodeMessagePrefix(true));
				}
				else if (!(ValidationDecider?.IsRuleB2101Active ?? false))
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				}
			}

			if (parent.IsRuleActive(x => x.IsRuleTR0052Active)
				&& !parent.BM_ActiveBorderIdentificationType.IsEmpty)
			{
				TransportMeansValidationHelper.CheckOfficeHasRequiredPurpose(parent, targetInfo);
			}

			B2101Validation.ValidateBM_CustomsOfficeAtBorder();
			NctsDepartureMovementHeaderPhase5RuleG0789Validation.CheckCustomsOfficeAtBorderRuleG0789(targetInfo, customsOfficeAtBorder, parent.Header, ValidationRuleConfiguration.Messages.G0789_1Message);
		}

		protected override void CheckBM_MethodOfPayment()
		{
			base.CheckBM_MethodOfPayment();
			CheckBM_TypeOfSecurityRuleC0337_2();
		}

		void CheckBM_TypeOfSecurityRuleC0337_2()
		{
			var parent = Parent;
			if ((ValidationDecider?.IsRuleC0337_2Active ?? false)
				&& !parent.BM_MethodOfPayment.IsEmpty
				&& parent.Header.Bills.Any(x => !x.B0_TransportPaymentMethod.IsEmpty))
			{
				parent.BM_MethodOfPaymentInfo.AddMessageError(ValidationRuleConfiguration.Messages.C0337_2Message);
			}
		}

		protected override void CheckBM_RN_NKCountryOfDispatch()
		{
			base.CheckBM_RN_NKCountryOfDispatch();

			var parent = Parent;

			new RuleE1301Validator(parent.Header).Validate(
			parent.BM_RN_NKCountryOfDispatchInfo,
			Res.GetString("5ECA2807-6BD3-4DEC-BE69-B3738EADCC4C", "Country of Dispatch"),
			ValidationDecider is { IsRuleE1301Active: true });

			ListValidation.MessageErrorIfInvalidCode(parent.BM_RN_NKCountryOfDispatchInfo);

			if (ValidationDecider?.IsRuleC0909Active ?? false)
			{
				CheckRuleC0909();
			}
		}

		void CheckRuleC0909()
		{
			var parent = Parent;
			if (parent.BM_RN_NKCountryOfDispatch.IsEmpty
				&& parent.Header.Bills.All(bill => bill.B0_RN_NKCountryOfExport.IsEmpty && bill.GoodsItems.All(goodsItem => goodsItem.BY_RN_NKCountryOfDispatch.IsEmpty)))
			{
				parent.BM_RN_NKCountryOfDispatchInfo.AddMessageError(NctsHeaderValidationHelper.C0909ValidationMessage);
			}
		}

		protected override void CheckTirCarnetNumberCore()
		{
			base.CheckTirCarnetNumberCore();

			var parent = Parent;
			var targetInfo = parent.TirCarnetNumberInfo;
			if (ValidationDecider?.IsRuleR0990Active ?? false)
			{
				CheckConditionR990(NctsHeader, targetInfo);
			}
			else if (parent.TirCarnetNumber.Length > 12)
			{
				targetInfo.AddMessageError(Res.GetString("A70417E2-E273-4404-9241-3AD6D24CFFF3", "The maximum length for TIR Carnet Num. is 12 characters."));
			}
		}

		void CheckConditionR990(NctsHeader nctsHeader, ZPropertyInfo info)
		{
			var tirCarnetNumber = nctsHeader.MovementHeader.TirCarnetNumber;
			var numberLength = tirCarnetNumber.Length;

			if (numberLength != 10 && numberLength != 11)
			{
				info.AddMessageError(Res.GetString("D98456A6-FDF7-4B54-8FE4-75C4FF0D157F", "(R0990) TIR Carnet Number can have a format of an10 or an11."));
			}
			if (!TirCarnetNumberValidationHelper.NumberIsValid(tirCarnetNumber))
			{
				info.AddMessageError(Res.GetString("3D459FCA-9C60-4E63-B3E3-A060B02AA1BE", "(R0990) TIR Carnet Number is not as per IRU Algorithm."));
			}
		}

		protected override void CheckBM_RL_NKDestinationPort()
		{
			base.CheckBM_RL_NKDestinationPort();

			if (Parent.BM_RL_NKDestinationPort.IsEmpty
				&& ValidationDecider.IsRuleC0343Active
				&& Parent.Header.Bills.Any(x => x.B0_RN_NKCountryOfDestination.IsEmpty))
			{
				Parent.BM_RL_NKDestinationPortInfo.AddMessageError(Res.GetString("765B026A-F912-4E79-89CB-690310F979E7", "[C0343] You have not entered Country of Destination. It is required either on Declaration or House Consignment Item."));
			}

			if (ValidationDecider?.IsRuleC0343_2Active ?? false)
			{
				CheckBM_RL_NKDestinationPort_RuleC0343_2();
			}
		}

		protected override void CheckBM_InBondEntryType()
		{
			base.CheckBM_InBondEntryType();
			var nctsHeader = NctsHeader;
			var parent = Parent;
			var inBondEntryTypeInfo = parent.BM_InBondEntryTypeInfo;

			CheckRulePLR0601(nctsHeader, inBondEntryTypeInfo);
			CheckConditionR0601_1(nctsHeader, inBondEntryTypeInfo);
			CheckConditionC0030(nctsHeader, inBondEntryTypeInfo);
			CheckConditionC0035_1(nctsHeader, inBondEntryTypeInfo);
			CheckConditionB1836(nctsHeader, inBondEntryTypeInfo);
			nctsHeader.CheckConditionR0520(inBondEntryTypeInfo);

			CheckConditionRP16(inBondEntryTypeInfo);
			CheckConditionR0020();
			CheckConditionR0020_1();
			CheckBM_InBondEntryType_ConditionR0911(parent);
			CheckBM_InBondEntryType_R0900_4();
			CheckBM_InBondEntryType_B1922();

			CheckRuleNR0056(nctsHeader, inBondEntryTypeInfo);
			CheckConditionR0507(nctsHeader, inBondEntryTypeInfo, x => x.BY_Type);
		}

		void CheckConditionR0507(NctsHeader nctsHeader, ZPropertyInfo info, Func<NctsCommonCargoDesc, ZString> valueProvider)
		{
			var goodsItemsEnum = nctsHeader.GetGoodsItems().OfType<NctsDepartureCargoDesc>();
			foreach (var item in goodsItemsEnum)
			{
				if (!item.CheckConditionR0507(info, valueProvider, item.BY_TypeInfo.HumanReadableName))
				{
					break;
				}
			}
		}

		void CheckRuleNR0056(NctsHeader nctsHeader, ZPropertyInfo info)
		{
			var isRuleNR0056Applicable = (ValidationDecider?.IsRuleNR0056Active ?? false) && Parent.IsTIRDeclaration;
			if (isRuleNR0056Applicable && !AllGoodsItemsHaveN952PreviousDocument())
			{
				info.AddMessageError(nctsHeader.Configuration.ValidationRuleConfiguration.Messages.NR0056Message);
			}

			bool AllGoodsItemsHaveN952PreviousDocument()
			{
				return nctsHeader.Bills
					.SelectMany(bill => bill.GoodsItems)
					.All(x => x.PreviousDocuments.HasN952PreviousDocument());
			}
		}

		void CheckRulePLR0601(NctsHeader nctsHeader, ZPropertyInfo info)
		{
			if (ValidationDecider?.IsRulePLR0601Active ?? false)
			{
				var goodItems = nctsHeader.Bills.SelectMany(i => i.GoodsItems);
				var inBondEntryType = nctsHeader.MovementHeader.BM_InBondEntryType;

				if (goodItems.Any(i => !RuleIsValid(i, i.SupportingDocuments, inBondEntryType, NctsPhase5DeclarationTypeList.Codes.T1)))
				{
					info.AddMessageError(Res.GetString("8A87714C-3F2F-4971-B211-E492B3DB4CC9", "(R0601) Invalid declaration type - T or T1 is allowed."));
				}

				if (goodItems.Any(i => !RuleIsValid(i, i.PreviousDocuments, inBondEntryType, NctsPhase5DeclarationTypeList.Codes.T2, NctsPhase5DeclarationTypeList.Codes.T2F)))
				{
					info.AddMessageError(Res.GetString("E4804E82-61B5-4B29-A580-D857DBCBE985", "(R0601) Invalid declaration Type - T or T2 or T2F is allowed."));
				}

				bool RuleIsValid(NctsDepartureCargoDesc goodsItem, IEnumerable<AutoCusSupportingInfo> docs, ZString entryType, params ZString[] declarationTypes)
				{
					return docs.All(d => d.CSI_Code != UniversalReferenceConstants.RefCusCodeList.Codes.C651 && d.CSI_Code != UniversalReferenceConstants.RefCusCodeList.Codes.C658)
							|| (goodsItem.BY_Type.IsEmpty && declarationTypes.Contains(entryType))
							|| (declarationTypes.Contains(goodsItem.BY_Type) && entryType == NctsPhase5DeclarationTypeList.Codes.T);
				}
			}
		}

		void CheckConditionC0030(NctsHeader nctsHeader, ZPropertyInfo propertyInfo)
		{
			var departureMovementHeader = nctsHeader.MovementHeader;
			if (IsC0030Applicable() && !departureMovementHeader.HasTransitOffice())
			{
				if (departureMovementHeader.BM_InBondEntryType == NctsPhase5DeclarationTypeList.Codes.T2)
				{
					propertyInfo.AddMessageError(NctsConstants.ValidationMessages.YouHaveNotEnteredACustomsOfficeOfTransitDeclared);
				}
				else if (departureMovementHeader.BM_InBondEntryType == NctsPhase5DeclarationTypeList.Codes.T
						&& nctsHeader.Bills.SelectMany(x => x.GoodsItems).Any(i => i.BY_Type.EqualsIgnoringCase(NctsPhase5DeclarationTypeList.Codes.T2)))
				{
					propertyInfo.AddMessageError(NctsConstants.ValidationMessages.YouHaveNotEnteredACustomsOfficeOfTransitDeclared);
				}
			}

			bool IsC0030Applicable() => (ValidationDecider?.IsRuleC0030Active ?? false) && !IsB1836Applicable;
		}

		bool IsB1836Applicable => (ValidationDecider?.IsRuleB1836Active ?? false) && IsInPhase5TransitionPeriod;

		void CheckConditionB1836(NctsHeader header, ZPropertyInfo propertyInfo)
		{
			if (IsB1836Applicable
				&& !Parent.HasTransitOffice()
				&& InBondEntryTypeIsTOrT2(header))
			{
				propertyInfo.AddMessageError(Res.GetString("996179FD-92C3-473C-BF2C-D2207A127043", "[B1836] Customs Office with Purpose='TRA' is required."));
			}
		}

		static bool InBondEntryTypeIsTOrT2(NctsHeader header) => header.MovementHeader is NctsDepartureMovementHeader departureMovementHeader
																&& (departureMovementHeader.BM_InBondEntryType == NctsPhase5DeclarationTypeList.Codes.T
																	|| departureMovementHeader.BM_InBondEntryType == NctsPhase5DeclarationTypeList.Codes.T2);

		void CheckConditionR0601_1(NctsHeader nctsHeader, ZPropertyInfo info)
		{
			var inBondEntryType = nctsHeader.MovementHeader?.BM_InBondEntryType ?? ZString.Empty;
			if (!inBondEntryType.IsEmpty && ValidationDecider.IsRuleR0601_1Active && !IsInPhase5TransitionPeriod)
			{
				var goodItems = nctsHeader.Bills.SelectMany(i => i.GoodsItems);
				var refCusCodeListCL234 = nctsHeader.GetCL234List();
				var hasN380 = nctsHeader.PreviousDocuments.Cast<AutoCusSupportingInfo>().Any(d => d.CSI_Code.EqualsIgnoringCase(UniversalReferenceConstants.PreviousDocumentTypes.CommercialInvoice));

				if (goodItems.Any(i => !RuleAdditionalInfoHasExciseIsValid(i, i.AdditionalInfos, hasN380, inBondEntryType, refCusCodeListCL234, NctsPhase5DeclarationTypeList.Codes.T1, NctsPhase5DeclarationTypeList.Codes.TIR)))
				{
					info.AddMessageError(Res.GetString("660CE2E1-DF97-413D-B164-E8AC2C14621C", "Please enter 'N380' in Previous Document and 'T1/TIR' as Declaration type at Consignment level when Additional Reference Type at Consignment item level has Excise Codes."));
				}

				if (goodItems.Any(i => !RuleSupportingDocumentHasExciseIsValid(i, i.SupportingDocuments, inBondEntryType, refCusCodeListCL234, NctsPhase5DeclarationTypeList.Codes.T2, NctsPhase5DeclarationTypeList.Codes.T2F)))
				{
					info.AddMessageError(Res.GetString("7ACABE42-9858-41DA-BFC4-A9549F61C316", "Declaration type should be T2 or T2F at Consignment level or Consignment Item level when Supporting Document Type at Consignment item level has Excise Codes."));
				}
			}

			bool RuleAdditionalInfoHasExciseIsValid(NctsDepartureCargoDesc goodsItem, IEnumerable<AdditionalInfo> infos, bool hasN380, ZString entryType, CodeDescriptionPairList codeListCL234, params ZString[] allowedDeclarationTypes)
			{
				return !(goodsItem.BY_Type.IsEmpty && infos.Any(d => d.CSI_SubType.EqualsIgnoringCase(AdditionalInfoSubTypeList.Codes.AdditionalReference) && codeListCL234.ContainsCode(d.CSI_Code)))
					|| (allowedDeclarationTypes.Contains(entryType) && hasN380);
			}

			bool RuleSupportingDocumentHasExciseIsValid(NctsDepartureCargoDesc goodsItem, IEnumerable<AutoCusSupportingInfo> supportingDocuments, ZString entryType, CodeDescriptionPairList codeListCL234, params ZString[] allowedDeclarationTypes)
			{
				return !(goodsItem.BY_Type.IsEmpty && supportingDocuments.Any(d => codeListCL234.ContainsCode(d.CSI_Code)))
					|| allowedDeclarationTypes.Contains(entryType);
			}
		}

		void CheckConditionRP16(ZPropertyInfo info)
		{
			var parent = Parent;
			if (parent.Guarantees.Count == 0
				&& NctsHeaderValidationHelper.IsConditionRP16(parent.Header))
			{
				info.AddError(Res.GetString("49128860-9B8C-437F-B9F7-B2D82F9F462C", "[RP16] You have not entered Guarantee."));
			}
		}

		void CheckConditionC0035_1(NctsHeader nctsHeader, ZPropertyInfo info)
		{
			if ((ValidationDecider is IRuleC0035_1Decider decider && decider.IsActive)
				&& NctsHeaderValidationHelper.IsInternalCommunityTransitProcedureType(nctsHeader.MovementHeader.BM_InBondEntryType)
				&& !HasPreviousDocAtAnyLevel())
			{
				info.AddMessageError(nctsHeader.Configuration.ValidationRuleConfiguration.Messages.C0035_1Message);
			}

			bool HasPreviousDocAtAnyLevel() => nctsHeader.PreviousDocuments.Any()
					|| nctsHeader.DepartureGoodsItems.Any(x => x.PreviousDocuments.Any());
		}

		protected override void CheckBM_SpecificCircumstance()
		{
			base.CheckBM_SpecificCircumstance();
			ListValidation.MessageErrorIfInvalidCode(Parent.BM_SpecificCircumstanceInfo);
		}

		protected override void CheckBM_GONumber()
		{
			base.CheckBM_GONumber();

			var isSimplifiedNctsProcedure = Parent.IsSimplifiedNctsProcedure;
			var nctsHeader = NctsHeader;
			var targetInfo = Parent.BM_GONumberInfo;
			CheckBM_GONumber_NR0007Rule();
			CheckBM_GONumber_G0114Rule();

			void CheckBM_GONumber_G0114Rule()
			{
				if (ValidationDecider.IsRuleG0114Active && isSimplifiedNctsProcedure && NoAuthorizedConsignorAuthorizationCode())
				{
					targetInfo.AddMessageError(Res.GetString("A202195C-D2AA-4884-9DEC-B4486D37731C", "[G0114] An ACR authorization is required during Simplified Procedure."));
				}
			}

			bool NoAuthorizedConsignorAuthorizationCode()
			{
				return Parent.CusAuthorizationUsages.All(e => !e.AGC_Code.EqualsIgnoringCase(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit));
			}

			void CheckBM_GONumber_NR0007Rule()
			{
				if (ValidationDecider is { IsRuleNR0007Active: true } && isSimplifiedNctsProcedure && nctsHeader?.DepartureHeaderContainers.Count == 0)
				{
					targetInfo.AddMessageError(Res.GetString("061E6F29-742E-4AD8-A0A9-431A223FEE36", "[NR0007] You have not entered any Containers/Equipment."));
				}
			}
		}

		protected override void CheckBM_ReducedDatasetIndicator()
		{
			base.CheckBM_ReducedDatasetIndicator();
			CheckRuleC0101_1();
			CheckConditionR350();
		}

		void CheckConditionR350()
		{
			bool ModeIsFeasible(ZString mode) => mode == ModeOfTransportList.Codes._1_SeaTransport || mode == ModeOfTransportList.Codes._2_RailTransport || mode == ModeOfTransportList.Codes._4_AirTransport;

			var movementHeader = Parent;
			if ((ValidationDecider?.IsRuleR0350Active ?? false)
				&& movementHeader.BM_ReducedDatasetIndicator
				&& ModeIsFeasible(movementHeader.BM_InlandTransportMode)
				&& !Parent.CusAuthorizationUsages.Any(x => x.AGC_Code == CusAuthorizationHeaderTypeList.Codes.TransitReducedDataset))
			{
				movementHeader.BM_ReducedDatasetIndicatorInfo.AddMessageError(Res.GetString("231FE6D8-A15A-4083-A3C5-9492A8355A07", "[R0350, R0352] An Authorization for Code=TRD is required when Reduced Data Set Indicator is Yes and Inland M.O.T. is one of these - 1(Sea Transport) or 2(Rail Transport) or 4(Air Transport)."));
			}
		}

		void CheckRuleC0101_1()
		{
			var parent = Parent;
			if (ValidationDecider is { IsRuleC0101_1Active: true }
				&& parent.BM_ReducedDatasetIndicator
				&& Parent.CusAuthorizationUsages.All(e => e.AGC_Code != CusAuthorizationHeaderTypeList.Codes.TransitReducedDataset))
			{
				var errorMessage = ValidationRuleConfiguration.Messages.C0101_1RuleCodeMessage(CusAuthorizationHeaderTypeList.Codes.TransitReducedDataset, CusAuthorizationHeaderTypeList.Descriptions.TransitReducedDataset);
				parent.BM_ReducedDatasetIndicatorInfo.AddMessageError(errorMessage);
			}
		}

		protected override void CheckBM_ForeignDestPortKCode()
		{
			base.CheckBM_ForeignDestPortKCode();

			var parent = Parent;
			var foreignDestPortKCodeInfo = parent.BM_ForeignDestPortKCodeInfo;

			ListValidation.MessageErrorIfInvalidCode(parent.BM_ForeignDestPortKCodeInfo);

			CheckBM_ForeignDestPortKCodeMandatory();
			CheckRuleB1858_1();

			if (ValidationDecider.IsRuleC0191_1Active && !IsInPhase5TransitionPeriod)
			{
				if (IsDecCombinedWithEntrySummaryOrEntryAndExitSummary(parent) && foreignDestPortKCodeInfo.Value.IsEmpty)
				{
					foreignDestPortKCodeInfo.AddMessageError(ValidationRuleConfiguration.Messages.C0191_1bMessage);
				}

				if (IsSecurityTypeNotUsedForSafetyAndSecurity(parent) && !foreignDestPortKCodeInfo.Value.IsEmpty)
				{
					foreignDestPortKCodeInfo.AddWarning(ValidationRuleConfiguration.Messages.C0191_1cMessage);
				}
			}

			if (ValidationDecider.IsRuleC0191_2Active && !IsSecurityTypeNotUsedForSafetyAndSecurity(Parent))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_ForeignDestPortKCodeInfo, ValidationCaptions.UnlocoPlaceOfUnloadingOrCountry, messagePrefix: ValidationRuleConfiguration.Messages.C0191_2RuleCode.GetRuleCodeMessagePrefix(true));
			}
		}

		void CheckRuleB1858_1()
		{
			if (!ValidationDecider.IsRuleB1858_1Active || !IsInPhase5TransitionPeriod)
			{
				return;
			}

			var nctsDepartureMovementHeader = Parent;
			var validationRuleConfigurationMessages = ValidationRuleConfiguration.Messages;

			var foreignDestPortKCodeInfo = nctsDepartureMovementHeader.BM_ForeignDestPortKCodeInfo;
			if (nctsDepartureMovementHeader.BM_TypeOfSecurity == NctsTypeOfSecurityList.Codes.NON)
			{
				if (!nctsDepartureMovementHeader.BM_ForeignDestPortKCode.IsEmpty)
				{
					foreignDestPortKCodeInfo.AddMessageError(validationRuleConfigurationMessages.B1858_1Message(foreignDestPortKCodeInfo.HumanReadableName));
				}
			}
			else if (nctsDepartureMovementHeader.BM_SpecificCircumstance != NctsSpecificCircumstanceIndicatorList.Codes.XXX)
			{
				MandatoryValidation.MessageErrorIfNotEntered(foreignDestPortKCodeInfo, messagePrefix: validationRuleConfigurationMessages.B1858_1RuleCode.GetRuleCodeMessagePrefix(true));
			}
		}

		protected virtual void CheckBM_ForeignDestPortKCodeMandatory()
		{
			var parent = Parent;
			var foreignDestPortKCode = parent.BM_ForeignDestPortKCode;
			var targetInfo = parent.BM_ForeignDestPortKCodeInfo;
			if (parent.BM_TypeOfSecurity != NctsTypeOfSecurityList.Codes.NON && ValidationDecider.IsRuleNR0038Active && targetInfo.Value.IsEmpty)
			{
				var msgError = parent.Header.Configuration.ValidationRuleConfiguration.Messages.NR0038Message;
				targetInfo.AddMessageError(msgError);
			}
			if (IsRuleC0387Applicable_PlaceOfUnloading_Code(parent))
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo, ValidationCaptions.UnlocoPlaceOfUnloadingOrCountry, messagePrefix: ValidationRuleConfiguration.Messages.C0387RuleCode.GetRuleCodeMessagePrefix(true));
			}
		}

		protected override void CheckBM_PortOfPresentationCode()
		{
			var parent = Parent;

			base.CheckBM_PortOfPresentationCode();

			ListValidation.MessageErrorIfInvalidCode(parent.BM_PortOfPresentationCodeInfo);

			Phase5RuleB1893_1Validation.ValidatePortOfPresentationCode();

			CheckBM_PortOfPresentationCodeMandatory();
		}

		void CheckBM_PortOfPresentationCodeMandatory()
		{
			var parent = Parent;
			var additionalDeclarationType = parent.BM_AdditionalDeclarationType;
			var targetPropertyDescription = ValidationCaptions.CountryOrUNLOCOForPlaceOfLoading;
			var targetInfo = parent.BM_PortOfPresentationCodeInfo;
			var isRuleB1893Applicable = (ValidationDecider?.IsRuleB1893Active ?? false) && IsInPhase5TransitionPeriod;
			var isRuleC0403Applicable = (ValidationDecider?.IsRuleC0403Active ?? false) && !isRuleB1893Applicable;

			if (!targetInfo.Value.IsEmpty)
			{
				return;
			}

			if (isRuleC0403Applicable
				&& IsAdditionalDeclarationTypeFilledAndNotPreLodged(additionalDeclarationType))
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo, targetPropertyDescription, ValidationRuleConfiguration.Messages.C0403RuleCode.GetRuleCodeMessagePrefix(true));
			}

			if (ShouldCheckRuleC0387ForBM_PortOfPresentationCode)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo, ValidationCaptions.UnlocoPortOfLoadingOrCountry, messagePrefix: ValidationRuleConfiguration.Messages.C0387RuleCode.GetRuleCodeMessagePrefix(true));
			}

			if (isRuleB1893Applicable
				&& IsSecurityCodeApplicable(parent))
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo, ValidationCaptions.PlaceOfLoading, messagePrefix: ValidationRuleConfiguration.Messages.B1893RuleCode.GetRuleCodeMessagePrefix(true));
			}

			if (parent.BM_TypeOfSecurity != NctsTypeOfSecurityList.Codes.NON && ValidationDecider.IsRuleNR0036Active && targetInfo.Value.IsEmpty)
			{
				var msgError = parent.Header.Configuration.ValidationRuleConfiguration.Messages.NR0036Message;
				targetInfo.AddMessageError(msgError);
			}

			if (ValidationDecider.IsRuleNR0088Active && !parent.IsSimplifiedNctsProcedure)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo, targetPropertyDescription, ValidationRuleConfiguration.Messages.NR0088RuleCode.GetRuleCodeMessagePrefix(true));
			}
		}

		protected override void CheckBM_PlaceOfLoading()
		{
			base.CheckBM_PlaceOfLoading();

			Phase5RuleB1893_1Validation.ValidatePlaceOfLoading();

			CheckBM_PlaceOfLoadingMandatory();
		}

		void CheckBM_PlaceOfLoadingMandatory()
		{
			var parent = Parent;
			var targetInfo = parent.BM_PlaceOfLoadingInfo;
			var portOfPresentationCode = parent.BM_PortOfPresentationCode;
			var additionalDeclarationType = parent.BM_AdditionalDeclarationType;
			var targetPropertyDescription = ValidationCaptions.PlaceOfLoading;
			var isRuleC0403_1Applicable = (ValidationDecider?.IsRuleC0403_1Active ?? false) && !IsInPhase5TransitionPeriod;
			var isRuleB1893_2Applicable = (ValidationDecider?.IsRuleB1893_2Active ?? false) && IsInPhase5TransitionPeriod;

			if (!targetInfo.Value.IsEmpty)
			{
				return;
			}

			if (!IsPortOfPresentationOfLengthFive(portOfPresentationCode))
			{
				if (isRuleC0403_1Applicable
					&& IsAdditionalDeclarationTypeFilledAndNotPreLodged(additionalDeclarationType))
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo, targetPropertyDescription, ValidationRuleConfiguration.Messages.C0403_1RuleCode.GetRuleCodeMessagePrefix(true));
				}

				if (ShouldCheckRuleC0387ForBM_PlaceOfLoading)
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo, ValidationCaptions.PlaceOfLoadingLocationDescription, messagePrefix: ValidationRuleConfiguration.Messages.C0387RuleCode.GetRuleCodeMessagePrefix(true));
				}

				if (isRuleB1893_2Applicable
					&& IsSecurityCodeApplicable(parent)
					&& !parent.BM_PortOfPresentationCode.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo, ValidationCaptions.LocationForPlaceOfLoadingDescription, messagePrefix: ValidationRuleConfiguration.Messages.B1893_2RuleCode.GetRuleCodeMessagePrefix(true));
				}
			}

			if (ValidationDecider.IsRuleNR0037Active)
			{
				var msgError = parent.Header.Configuration.ValidationRuleConfiguration.Messages.NR0037Message;
				MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(targetInfo, parent.BM_PortOfPresentationCodeInfo, msgError);
			}

			if (ValidationDecider.IsRuleNR0080Active && !IsInPhase5TransitionPeriod && parent.BM_PlaceOfLoading.IsEmpty)
			{
				targetInfo.AddWarning(ValidationRuleConfiguration.Messages.NR0080Message);
			}
		}

		protected virtual bool ShouldCheckRuleC0387ForBM_PortOfPresentationCode => ValidationDecider.IsRuleC0387Active
			&& Parent.BM_AdditionalDeclarationType == NctsTypeOfAdditionalDeclarationList.Codes.A;

		protected virtual bool ShouldCheckRuleC0387ForBM_PlaceOfLoading => ValidationDecider.IsRuleC0387Active && Parent.BM_PortOfPresentationCode.Length == 2;

		protected override void CheckBM_PlaceOfUnloading()
		{
			base.CheckBM_PlaceOfUnloading();

			var parent = Parent;
			var placeOfUnloadingInfo = parent.BM_PlaceOfUnloadingInfo;

			CheckBM_PlaceOfUnloadingMandatory();

			if (ValidationDecider.IsRuleC0191_1Active
				&& !IsInPhase5TransitionPeriod)
			{
				if (IsDecCombinedWithEntrySummaryOrEntryAndExitSummary(parent) && parent.BM_ForeignDestPortKCode.Length == 2)
				{
					MandatoryValidation.MessageErrorIfNotEntered(placeOfUnloadingInfo, messagePrefix: ValidationRuleConfiguration.Messages.C0191_1RuleCode.GetRuleCodeMessagePrefix(true));
				}

				if (IsSecurityTypeNotUsedForSafetyAndSecurity(parent) && !placeOfUnloadingInfo.Value.IsEmpty)
				{
					placeOfUnloadingInfo.AddWarning(ValidationRuleConfiguration.Messages.C0191_1aMessage);
				}
			}
		}

		protected override void CheckBM_PlaceOfUnloadingMandatory()
		{
			base.CheckBM_PlaceOfUnloadingMandatory();
			var parent = Parent;
			var placeOfUnloadingInfo = parent.BM_PlaceOfUnloadingInfo;

			if (ValidationDecider.IsRuleNR0039Active)
			{
				var msgError = parent.Header.Configuration.ValidationRuleConfiguration.Messages.NR0039Message;
				MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(parent.BM_PlaceOfUnloadingInfo, parent.BM_ForeignDestPortKCodeInfo, msgError);
			}

			if (IsRuleC0387Applicable_PlaceOfUnloading_Description(parent))
			{
				MandatoryValidation.MessageErrorIfNotEntered(placeOfUnloadingInfo, ValidationCaptions.PlaceOfUnLoadingLocationDescription, messagePrefix: ValidationRuleConfiguration.Messages.C0387RuleCode.GetRuleCodeMessagePrefix(true));
			}
		}

		bool IsDecCombinedWithEntrySummaryOrEntryAndExitSummary(NctsDepartureMovementHeader parent)
			=> parent.BM_TypeOfSecurity == NctsTypeOfSecurityList.Codes.ENT || parent.BM_TypeOfSecurity == NctsTypeOfSecurityList.Codes.BTH;

		bool IsSecurityTypeNotUsedForSafetyAndSecurity(NctsDepartureMovementHeader parent) => parent.BM_TypeOfSecurity == NctsTypeOfSecurityList.Codes.NON;

		protected override bool ShouldListValidatePlaceOfUnloading => false;

		void CheckBM_InlandTransportMode_B1091Rule()
		{
			if (ValidationDecider is INctsDepartureMovementHeaderPhase5ValidationDecider { IsRuleB1091Active: true }
				&& IsInPhase5TransitionPeriod
				&& Parent.BM_InlandTransportMode == ModeOfTransportList.Codes._5_PostalConsignment
			)
			{
				Parent.BM_InlandTransportModeInfo.AddMessageError(Res.GetString("DAF938C8-7DFC-48DD-B21E-44A20C5244A4", "[B1091] Inland M.O.T selected is not valid during transition period."));
			}
		}

		string YouMayOnlyEnterEitherFlightNumberOrAircraftIDMessage => Res.GetString("20D30619-F85C-4A38-A960-448BF5440F60", "You may only enter either a Flight Number or an Aircraft ID.");

		protected override void CheckBM_ExportTransportMode()
		{
			base.CheckBM_ExportTransportMode();
			CheckBM_ExportTransportModeRuleC0599();
			CheckBM_ExportTransportModeRuleC0599_1();
			CheckBM_ExportTransportModeRuleC0599_2();
			CheckBM_ExportTransportModeRuleB1889();
			CheckBM_ExportTransportModeRuleR0789_1();
		}

		void CheckConditionR0020()
		{
			var parent = Parent;
			if (parent.IsDeparturePhase5RuleActive(x => x.IsRuleR0020Active) &&
				NctsValidationHelper.IsEntryTypeT2OrT2F(parent.BM_InBondEntryType) &&
				parent.CustomsOfficesForDeparture.HasCL112OfficeOfDepartureCountry(parent.Factory) &&
				HasNoCL178PreviousDocument(parent))
			{
				parent.BM_InBondEntryTypeInfo.AddMessageError(ValidationMessages.PreviousDocumentOfTypeCL178IsRequiredForCustomsOfficeOfDepartureInCL112AndDeclarationTypeT2OrT2F);
			}
		}

		void CheckConditionR0020_1()
		{
			var parent = Parent;
			if (parent.IsDeparturePhase5RuleActive(x => x.IsRuleR0020_1Active) &&
				NctsValidationHelper.IsEntryTypeT2OrT2F(parent.BM_InBondEntryType) &&
				HasNoCL178PreviousDocument(parent))
			{
				parent.BM_InBondEntryTypeInfo.AddMessageError(NctsHeader.Configuration.ValidationRuleConfiguration.Messages.R0020_1bMessage);
			}
		}

		void CheckRuleN0002(ZPropertyInfo targetPropertyInfo)
		{
			if (ValidationDecider?.IsRuleN0002Active ?? false)
			{
				new NctsDepartureMovementHeaderOrBillPhase5RuleN0002Validation(Parent).ValidateRuleN0002(targetPropertyInfo);
			}
		}

		static bool HasNoCL178PreviousDocument(NctsDepartureMovementHeader parent) => !parent.Header.PreviousDocuments.HasCL178PreviousDocument(parent.Factory)
			&& (parent.Header.Bills.Count == 0
				|| parent.Header.Bills.Any(bill => bill.GoodsItems.Count == 0
					|| bill.GoodsItems.Cast<NctsDepartureCargoDesc>().Any(goodsItem => !goodsItem.PreviousDocuments.HasCL178PreviousDocument(parent.Factory))));

		protected override void CheckGoodsLocationDescription()
		{
			base.CheckGoodsLocationDescription();

			new NctsDepartureMovementHeaderPhase5RuleC0710Validation(Parent).Validate();

			CusGoodsLocationValidationHelper.ValidateInnerGoodsLocation(Parent);
		}

		ValidationRuleConfiguration ValidationRuleConfiguration => Configuration.ValidationRuleConfiguration;

		NctsConfiguration Configuration => NctsHeader.Configuration;

		bool CheckRuleC0806_NotInPhase5TransitionPeriod() => !IsInPhase5TransitionPeriod && NctsHelper.IsSecurityTypeBTHOrEXIOrENT(Parent.BM_TypeOfSecurity) && Parent.BM_AdditionalDeclarationType == NctsTypeOfAdditionalDeclarationList.Codes.A;

		NctsDepartureMovementHeaderPhase5RuleB2101Validation B2101Validation => b2101Validation ?? (b2101Validation = new NctsDepartureMovementHeaderPhase5RuleB2101Validation(Parent));
		NctsDepartureMovementHeaderPhase5RuleB2101Validation b2101Validation;

		NctsPhase5RuleR0473Validation R0473Validation => r0473Validation ?? (r0473Validation = new NctsPhase5RuleR0473Validation(Parent));
		NctsPhase5RuleR0473Validation r0473Validation;

		NctsDepartureMovementHeaderPhase5RuleB1897Validation Phase5RuleB1897Validator => phase5RuleB1897Validator ?? (phase5RuleB1897Validator = new NctsDepartureMovementHeaderPhase5RuleB1897Validation(Parent));
		NctsDepartureMovementHeaderPhase5RuleB1897Validation phase5RuleB1897Validator;

		NctsDepartureMovementHeaderPhase5RuleB1893_1Validation Phase5RuleB1893_1Validation => phase5RuleB1893_1Validation ?? (phase5RuleB1893_1Validation = new NctsDepartureMovementHeaderPhase5RuleB1893_1Validation(Parent));
		NctsDepartureMovementHeaderPhase5RuleB1893_1Validation phase5RuleB1893_1Validation;

		void CheckBM_InBondEntryType_ConditionR0911(NctsDepartureMovementHeader movementHeader)
		{
			var nctsHeader = movementHeader.Header;
			var declarationType = movementHeader.BM_InBondEntryType;

			if (ValidationDecider.IsRuleR0911Active
				&& !declarationType.IsEmpty
				&& declarationType != NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure
				&& declarationType != NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedureBetweenDifferentFiscalTerritories
				&& movementHeader.DepartureCustomsOfficeCodeCountry == Core.Constants.CountryCodes.SanMarino
				&& movementHeader.DestinationCustomsOffice is NctsEuOfficeCode { IsInCL010CountryList: true })
			{
				movementHeader.BM_InBondEntryTypeInfo.AddMessageError(nctsHeader.Configuration.ValidationRuleConfiguration.Messages.R0911Message);
			}
		}

		void CheckBM_InBondEntryType_R0900_4()
		{
			var parent = Parent;
			if (ValidationDecider is { IsRuleR0900_4Active: true } && parent.Guarantees.IsNullOrEmpty())
			{
				parent.BM_InBondEntryTypeInfo.AddMessageError(Res.GetString("E9A825C7-3241-4743-B9AF-09CD8A9ACD36", "[R0900-4] Guarantee data group is mandatory"));
			}
		}

		void CheckBM_InBondEntryType_B1922()
		{
			var declarationType = Parent.BM_InBondEntryType;
			if ((ValidationDecider?.IsRuleB1922Active ?? false)
				&& IsInPhase5TransitionPeriod)
			{
				var cl234GoodsItems = GetCL234GoodsItems().ToList();
				if (cl234GoodsItems.Count > 0)
				{
					var hasItemWithPreviousDocumentN830 = cl234GoodsItems.Any(x => x.PreviousDocuments.Any(d => d.CSI_Code == NctsConstants.NctsTypeOfPreviousDocument.Codes.N830));
					if (hasItemWithPreviousDocumentN830 && declarationType != NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration
						&& declarationType != NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure)
					{
						Parent.BM_InBondEntryTypeInfo.AddMessageError(Res.GetString("EFAC49FA-5D08-4337-AD03-6F372F1D9C5E", "[B1922] Declaration Type must be T1 or TIR."));
						return;
					}

					if (!hasItemWithPreviousDocumentN830 && declarationType != NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure
						&& declarationType != NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedureBetweenDifferentFiscalTerritories)
					{
						Parent.BM_InBondEntryTypeInfo.AddMessageError(Res.GetString("40FFD51C-B4F9-4044-BB1B-8104786A5E89", "[B1922] Declaration Type must be T2 or T2F."));
					}
				}
			}

			IEnumerable<NctsDepartureCargoDesc> GetCL234GoodsItems()
			{
				return NctsHeader.Bills.SelectMany(x => x.GoodsItems.Where(IsCL234GoodsItem));
			}

			bool IsCL234GoodsItem(NctsDepartureCargoDesc goodsItem) => goodsItem.BY_Type.IsEmpty
					&& goodsItem.AdditionalInfos.Any(d => d.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference && NctsHeader.GetCL234List().ContainsCode(d.CSI_Code));
		}

		void CheckBM_ExportTransportModeRuleC0599()
		{
			var parent = Parent;
			if (IsRuleC0599Applicable
				&& parent.BM_AdditionalDeclarationType == NctsTypeOfAdditionalDeclarationList.Codes.A
				&& parent.IsSecurityTypeENTOrBTHOrEXI
				&& parent.BM_ExportTransportMode.IsEmpty)
			{
				parent.BM_ExportTransportModeInfo.AddMessageError(Res.GetString("F65D39C2-A38F-42D6-A8AE-14884FF6A256", "{0} You have not entered a Transport Border Mode of Transport.", ValidationRuleConfiguration.Messages.C0599RuleCode.GetRuleCodeMessagePrefix()));
			}
		}

		void CheckBM_ExportTransportModeRuleC0599_1()
		{
			var parent = Parent;
			if (ValidationDecider is { IsRuleC0599_1Active: true } && parent.BM_InBondEntryType != NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland && IsInPhase5TransitionPeriod)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.BM_ExportTransportModeInfo, messagePrefix: ValidationRuleConfiguration.Messages.C0599_1RuleCode.GetRuleCodeMessagePrefix(true));
			}
		}

		void CheckBM_ExportTransportModeRuleC0599_2()
		{
			var parent = Parent;
			if (parent.ValidationDecider is INctsDepartureMovementHeaderPhase5ValidationDecider { IsRuleC0599_2Active: true }
				&& !IsInPhase5TransitionPeriod
				&& parent.IsSecurityTypeENTOrBTHOrEXI)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.BM_ExportTransportModeInfo, messagePrefix: ValidationRuleConfiguration.Messages.C0599_2RuleCode.GetRuleCodeMessagePrefix(true));
			}
		}

		bool IsRuleC0599Applicable => ValidationDecider is { IsRuleC0599Active: true } && !IsRuleB1889Applicable;

		bool IsRuleB1889Applicable => ValidationDecider is { IsRuleB1889Active: true } && IsInPhase5TransitionPeriod;

		void CheckBM_ExportTransportModeRuleB1889()
		{
			var parent = Parent;
			if (IsRuleB1889Applicable
				&& parent.IsSecurityTypeENTOrBTHOrEXI
				&& !parent.CustomsOffices.Where(office => office.IsOfficeDeparture && office.IsInCL010CountryList).Any()
				&& parent.BM_ExportTransportMode.IsEmpty)
			{
				parent.BM_ExportTransportModeInfo.AddMessageError(Res.GetString("88264E09-453E-42D8-ACE8-8AFC63B7E5F2", "{0} You have not entered a Transport Border Mode of Transport.", ValidationRuleConfiguration.Messages.B1889RuleCode.GetRuleCodeMessagePrefix()));
			}
		}

		void CheckBM_ExportTransportModeRuleR0789_1()
		{
			var parent = Parent;

			if (ValidationDecider.IsRuleR0789_1Active
				&& !parent.CustomsOfficesForDeparture.ContainsCode(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit)
				&& parent.BM_ExportTransportMode.IsEmpty)
			{
				parent.BM_ExportTransportModeInfo.AddMessageError(ValidationRuleConfiguration.Messages.R0789_1Message);
			}
		}

		protected override void CheckBM_ExportDate()
		{
			var parent = Parent;
			var propertyInfo = parent.BM_ExportDateInfo;

			base.CheckBM_ExportDate();
			CheckBM_ExportDateTR0092();

			if (ValidationDecider.IsRuleC0839_1Active && parent.IsSimplifiedNctsProcedure)
			{
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo, messagePrefix: ValidationRuleConfiguration.Messages.C0839_1RuleCode.GetRuleCodeMessagePrefix(true));
			}

			if (IsRuleCN839Applicable())
			{
				propertyInfo.AddMessageError(ValidationRuleConfiguration?.Messages?.GetCN839Message());
			}
		}

		void CheckBM_ExportDateTR0092()
		{
			var isRuleTR0092Active = ValidationDecider?.IsRuleTR0092Active ?? false;

			if (!isRuleTR0092Active)
			{
				return;
			}

			var parent = Parent;
			var entryDate = parent.BM_EntryDate;
			var exportDate = parent.BM_ExportDate;

			if (!entryDate.IsEmpty)
			{
				if (exportDate.Date < entryDate.Date)
				{
					parent.BM_ExportDateInfo.AddMessageError(parent.Header.Configuration.ValidationRuleConfiguration.Messages.TR0092aMessage);
				}
			}
			else
			{
				if (exportDate.IsInThePastDatePartOnly)
				{
					parent.BM_ExportDateInfo.AddMessageError(parent.Header.Configuration.ValidationRuleConfiguration.Messages.TR0092bMessage);
				}
			}
		}

		#region CheckBM_TypeOfSecurity Rules

		void CheckBM_TypeOfSecurityRuleTR0048(ZPropertyInfo targetInfo, NctsHeader header, ZBool securityIsEXIorBTH)
		{
			if (ValidationDecider is { IsRuleTR0048Active: true }
				&& securityIsEXIorBTH
				&& !Parent.CustomsOffices.ContainsCode(OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit))
			{
				targetInfo.AddMessageError(Res.GetString("478CDBE0-EF61-4D8E-A01A-097A7CE9438E", "[TR0048] You have not entered an Office of Exit for Transit with Purpose 'TXT'."));
			}
		}

		void CheckBM_TypeOfSecurityRuleTR0053(ZPropertyInfo targetInfo, NctsHeader header, ZBool securityIsEXIorBTH)
		{
			if (ValidationDecider is { IsRuleTR0053Active: true }
				&& securityIsEXIorBTH
				&& !Parent.CustomsOffices.Any<NctsEuOfficeCode>(o => o.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit || o.IsOfficeOfTransit))
			{
				targetInfo.AddMessageError(Res.GetString("B8C53012-5C2E-45A3-B12C-57D45388FF72", "[TR0053] You have not entered an Office of Exit for Transit with Purpose 'TXT'."));
			}
		}

		void CheckBM_TypeOfSecurityRuleC0586AndB1848(ZPropertyInfo targetInfo, ZBool securityTypeIsENTOrBTHOrEXI, int countriesOfRoutingCount)
		{
			if (securityTypeIsENTOrBTHOrEXI && countriesOfRoutingCount == 0)
			{
				if (IsRuleC0586Applicable)
				{
					targetInfo.AddMessageError(ValidationRuleConfiguration.Messages.C0586Message);
				}
				else if (IsRuleB1848Applicable)
				{
					targetInfo.AddMessageError(ValidationRuleConfiguration.Messages.B1848Message);
				}
			}
		}

		bool IsRuleC0586Applicable => ValidationDecider is { IsRuleC0586Active: true } && !IsRuleB1848Applicable;

		bool IsRuleB1848Applicable => ValidationDecider is INctsDepartureMovementHeaderPhase5ValidationDecider { IsRuleB1848Active: true } && IsInPhase5TransitionPeriod;

		void CheckBM_TypeOfSecurityRuleB1848_1(ZPropertyInfo targetInfo, ZBool securityTypeIsENTOrBTHOrEXI, int countriesOfRoutingCount)
		{
			if (ValidationDecider is { IsRuleB1848_1Active: true } && securityTypeIsENTOrBTHOrEXI && countriesOfRoutingCount < 2)
			{
				targetInfo.AddMessageError(ValidationRuleConfiguration.Messages.B1848_1Message);
			}
		}

		void CheckBM_TypeOfSecurityRuleNR0018(ZPropertyInfo targetInfo)
		{
			if (ValidationDecider is { IsRuleNR0018Active: true } && Parent.BM_TypeOfSecurity == NctsTypeOfSecurityList.Codes.NON)
			{
				if (!Parent.BM_RL_NKDestinationPort.IsEmpty && !Parent.Factory.IsEuropeanUnionForSafetyAndSecurityCountry(Parent.BM_RL_NKDestinationPort))
				{
					targetInfo.AddWarning(NctsHeader.Configuration.ValidationRuleConfiguration.Messages.NR0018Message);
				}
			}
		}

		void CheckBM_TypeOfSecurityRuleBR5410(ZPropertyInfo targetInfo)
		{
			if ((Parent.BM_TypeOfSecurity == NctsTypeOfSecurityList.Codes.ENT || Parent.BM_TypeOfSecurity == NctsTypeOfSecurityList.Codes.BTH) && (ValidationDecider?.IsRuleBR5410Active ?? false))
			{
				targetInfo.AddMessageError(NctsHeader.Configuration.ValidationRuleConfiguration.Messages.BR5410Message);
			}
		}

		bool IsDestinationCountryEmptyAtAllLevels()
		{
			return Parent.BM_RL_NKDestinationPort.IsEmpty && Parent.Header.Bills.Any(x => x.B0_RN_NKCountryOfDestination.IsEmpty && x.GoodsItems.Any(y => y.BY_RN_NKCountryOfDestination.IsEmpty));
		}

		bool IsDestinationCountryFilledAtDetailsTabAndLowerLevels()
		{
			return !Parent.BM_RL_NKDestinationPort.IsEmpty && Parent.Header.Bills.Any(x => !x.B0_RN_NKCountryOfDestination.IsEmpty || x.GoodsItems.Any(y => !y.BY_RN_NKCountryOfDestination.IsEmpty));
		}

		void CheckBM_RL_NKDestinationPort_RuleC0343_2()
		{
			if (IsDestinationCountryEmptyAtAllLevels() || IsDestinationCountryFilledAtDetailsTabAndLowerLevels())
			{
				var message = ValidationRuleConfiguration.Messages.C0343_2Message;
				Parent.BM_RL_NKDestinationPortInfo.AddMessageError(message);
			}
		}

		#endregion

		internal void ValidateAdditionalTransportAtBorderListCount()
		{
			ValidateCalculatedProperty(Parent.AdditionalTransportAtBorderListCountInfo);
		}

		void ValidateAuthorizations()
		{
			var authorizations = Parent.CusAuthorizationUsages;
			var header = Parent.Header;
			var messageErrorForTR0051 = header.Configuration.ValidationRuleConfiguration.Messages.TR0051Message;
			var acrAuth = authorizations.FirstOrDefault(x => x.AGC_Code == CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit);
			if (acrAuth != null)
			{
				acrAuth.ClearRowNotificationsContaining(messageErrorForTR0051);
				if (ValidationDecider is { IsRuleTR0051Active: true } && authorizations.All(x => x.AGC_Code != CusAuthorizationHeaderTypeList.Codes.SpecialSeals))
				{
					acrAuth.AddRowWarning(messageErrorForTR0051);
				}
			}
		}

		protected void CheckAdditionalTransportAtBorderListCount()
		{
			var additionalTransports = Parent.AdditionalTransportAtBorderList;
			if (additionalTransports.Count > 0)
			{
				Parent.AdditionalTransportAtBorderListCountInfo.AddNotificationBasedOnChildValidationStatus(
					Res.GetString("19DF70C4-75EC-466E-B7AD-82E9B8368167", "There are errors within the 'Additional Transport Border Means', please click on 'More..' to view the error information."),
					() => additionalTransports.ForEach(x => x.Validation.ValidateAll()),
					additionalTransports);
			}
		}

		bool IsRuleC0387Applicable_PlaceOfUnloading_Code(NctsDepartureMovementHeader parent) => ValidationDecider.IsRuleC0387Active && IsSecurityCodeApplicable(parent) && ShouldCheckRuleC0387ForBM_ForeignDestPortKCode && !(IsInPhase5TransitionPeriod && parent.BM_SpecificCircumstance == NctsSpecificCircumstanceIndicatorList.Codes.XXX);

		protected virtual bool ShouldCheckRuleC0387ForBM_ForeignDestPortKCode => true;

		bool IsRuleC0387Applicable_PlaceOfUnloading_Description(NctsDepartureMovementHeader parent) => ValidationDecider.IsRuleC0387Active && IsSecurityCodeApplicable(parent) && parent.BM_ForeignDestPortKCode.Length == 2;

		bool IsRuleB1897Applicable => ValidationDecider.IsRuleB1897Active && IsInPhase5TransitionPeriod;

		bool IsRuleB1897AndRuleB2101NotApplicable => !IsRuleB1897Applicable && !((ValidationDecider?.IsRuleB2101Active ?? false) && !IsInPhase5TransitionPeriod);

		bool IsSecurityCodeApplicable(NctsDepartureMovementHeader parent) => !parent.BM_TypeOfSecurity.IsEmpty && !IsSecurityTypeNotUsedForSafetyAndSecurity(parent);

		bool IsInPhase5TransitionPeriod => Parent.IsInPhase5TransitionPeriod;

		protected sealed override bool ShouldCheckMandatoryGoodsItem => ValidationRuleConfiguration.IsRuleTR0067Active;

		protected sealed override string MandatoryGoodsItemMessage => ValidationRuleConfiguration.Messages.TR0067Message;

		bool IsPortOfPresentationOfLengthFive(ZString portOfPresentationCode) => portOfPresentationCode.Length == 5;

		bool IsAdditionalDeclarationTypeFilledAndNotPreLodged(ZString additionalDeclarationType) => !additionalDeclarationType.IsEmpty && additionalDeclarationType != NctsTypeOfAdditionalDeclarationList.Codes.D;

		bool IsRuleCN839Applicable()
		{
			var parent = Parent;
			return parent.BM_ExportDate.IsEmpty
				&& parent.BM_AdditionalDeclarationType != NctsTypeOfAdditionalDeclarationList.Codes.D
				&& ValidationDecider.IsRuleCN839Active
				&& parent.CusAuthorizationUsages.Any(e => e.AGC_Code == CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit);
		}
	}
}
