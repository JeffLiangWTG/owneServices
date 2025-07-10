using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsDepartureMovementHeaderPhase4Validation : NctsDepartureMovementHeaderValidation
	{
		public NctsDepartureMovementHeaderPhase4Validation(NctsDepartureMovementHeader parent)
			: base(parent)
		{
		}

		protected new INctsDepartureMovementHeaderPhase4ValidationDecider ValidationDecider => (INctsDepartureMovementHeaderPhase4ValidationDecider)base.ValidationDecider;

		protected override void CheckBM_InBondEntryType()
		{
			base.CheckBM_InBondEntryType();

			NctsHeader.CheckConditionR020(Parent.BM_InBondEntryTypeInfo);
		}

		protected override void CheckBM_TransportAtDeparture()
		{
			base.CheckBM_TransportAtDeparture();
			CheckConditionTR9090(NctsHeader, Parent.BM_TransportAtDepartureInfo);
		}

		void CheckConditionTR9090(NctsHeader nctsHeader, ZPropertyInfo info)
		{
			if (ValidationDecider?.IsRuleTR9090Active ?? false)
			{
				var departureMovement = nctsHeader.MovementHeader;
				var inlandTransportMode = departureMovement.BM_InlandTransportMode;
				if (!inlandTransportMode.IsEmpty)
				{
					if (inlandTransportMode == ModeOfTransportList.Codes._5_PostalConsignment || inlandTransportMode == ModeOfTransportList.Codes._7_FixedTransportInstallations)
					{
						if (!departureMovement.BM_TransportAtDeparture.IsEmpty)
						{
							info.AddMessageError(NctsHeaderValidationHelper.GetRuleExplanation(Rules_C_Conditions.Codes.TR9090, Res.GetString("16075A01-5DFB-430B-9585-1C5D2A81C95E", "Identity of Means of Transport at departure cannot be used.")));
						}
					}
					else if (!departureMovement.IsContainerised && departureMovement.BM_TransportAtDeparture.IsEmpty)
					{
						info.AddMessageError(NctsHeaderValidationHelper.GetRuleExplanation(Rules_C_Conditions.Codes.TR9090, Res.GetString("69FA92BB-AF65-4B71-AC38-609AEB0EF870", "Identity of Means of Transport at departure is required.")));
					}
				}
			}
		}

		protected override void CheckBM_RN_NKTransportAtDepartureCountry()
		{
			base.CheckBM_RN_NKTransportAtDepartureCountry();
			CheckConditionTR9095(NctsHeader, Parent.BM_RN_NKTransportAtDepartureCountryInfo);
		}

		void CheckConditionTR9095(NctsHeader nctsHeader, ZPropertyInfo info)
		{
			if (ValidationDecider?.IsRuleTR9095Active ?? false)
			{
				var inlandTransportMode = nctsHeader.MovementHeader.BM_InlandTransportMode;
				if (!inlandTransportMode.IsEmpty)
				{
					if (inlandTransportMode == ModeOfTransportList.Codes._2_RailTransport
						|| inlandTransportMode == ModeOfTransportList.Codes._5_PostalConsignment
						|| inlandTransportMode == ModeOfTransportList.Codes._7_FixedTransportInstallations
						)
					{
						if (!nctsHeader.MovementHeader.BM_RN_NKTransportAtDepartureCountry.IsEmpty)
						{
							info.AddMessageError(NctsHeaderValidationHelper.GetRuleExplanation(Rules_C_Conditions.Codes.TR9095, Res.GetString("CA81714E-D43F-4979-8529-24BD722C9DE1", "Nationality of Means of Transport at departure cannot be used.")));
						}
					}
					else if (!nctsHeader.MovementHeader.IsContainerised && nctsHeader.MovementHeader.BM_RN_NKTransportAtDepartureCountry.IsEmpty)
					{
						info.AddMessageError(NctsHeaderValidationHelper.GetRuleExplanation(Rules_C_Conditions.Codes.TR9095, Res.GetString("0161BD13-DF78-4464-A216-9452EB7B210E", "Nationality of Means of Transport at departure is required.")));
					}
				}
			}
		}

		protected override void CheckBM_TOLCarrierCode()
		{
			base.CheckBM_TOLCarrierCode();
			if (ShouldCheckConditionC010)
			{
				CheckConditionC010(NctsHeader, Parent.BM_TOLCarrierCodeInfo);
			}
		}

		void CheckConditionC010(NctsHeader nctsHeader, ZPropertyInfo info)
		{
			var departureMovementHeader = nctsHeader.MovementHeader;
			var transportModeAtBorder = departureMovementHeader.BM_ExportTransportMode;
			if (!transportModeAtBorder.IsEmpty
				&& transportModeAtBorder != ModeOfTransportList.Codes._2_RailTransport
				&& transportModeAtBorder != ModeOfTransportList.Codes._5_PostalConsignment
				&& transportModeAtBorder != ModeOfTransportList.Codes._7_FixedTransportInstallations
				&& departureMovementHeader.BM_TOLCarrierCode.IsEmpty)
			{
				info.AddMessageError(NctsHeaderValidationHelper.GetRuleExplanation(Rules_C_Conditions.Codes.C010, Res.GetString("324EE4E1-A6E9-47AA-902B-63D52F7E06B4", "Nationality of Means of Transport Crossing Border is required for this Transport Mode at the Border.")));
			}
		}

		protected virtual bool ShouldCheckConditionC010 => ValidationDecider?.IsRuleC010Active ?? false;

		protected override void CheckBM_TOLCarrierID()
		{
			base.CheckBM_TOLCarrierID();
			CheckConditionC011(NctsHeader, Parent.BM_TOLCarrierIDInfo);
		}

		void CheckConditionC011(NctsHeader nctsHeader, ZPropertyInfo info)
		{
			if (ValidationDecider?.IsRuleC011Active ?? false)
			{
				if (nctsHeader.MovementHeader.BM_TOLCarrierID.IsEmpty
					&& (!nctsHeader.MovementHeader.BM_TOLCarrierCode.IsEmpty
						|| nctsHeader.MovementHeader.BM_BTAIndicator == SpecificCircumstanceIndicator.Codes.RailModeOfTransport))
				{
					info.AddMessageError(NctsHeaderValidationHelper.GetRuleExplanation(Rules_C_Conditions.Codes.C011, Res.GetString("00BF1332-9A58-4708-A1C7-5DDD861493EF", "Identity of means of transport crossing border is required.")));
				}
			}
		}

		protected override void CheckBM_ConveyanceNumber()
		{
			base.CheckBM_ConveyanceNumber();
			CheckConditionC531(NctsHeader, Parent.BM_ConveyanceNumberInfo);
		}

		void CheckConditionC531(NctsHeader nctsHeader, ZPropertyInfo info)
		{
			if (ValidationDecider?.IsRuleC531Active ?? false)
			{
				if (nctsHeader.BH_FTZMove && nctsHeader.MovementHeader.IsAirExportTransportMode)
				{
					if (nctsHeader.MovementHeader.BM_ConveyanceNumber.IsEmpty)
					{
						MandatoryValidation.MessageErrorIfNotEntered(info);
					}
					else if (!IsIataOrIcaoFlightNumberFormat(nctsHeader.MovementHeader.BM_ConveyanceNumber))
					{
						info.AddWarning(NctsHeaderValidationHelper.GetRuleExplanation(Rules_C_Conditions.Codes.C531, Res.GetString("12B556CF-C408-4231-B33E-023833C4F56C", "Conveyance Reference Number must match the pattern for a flight number.")));
					}
				}
			}
		}

		static bool IsIataOrIcaoFlightNumberFormat(string refNo)
		{
			return Regex.IsMatch(refNo, @"^([a-zA-Z0-9]{1,3})\d{1,4}[a-zA-Z]?"); // Z7 or BA123c or LHR1234 or 1251234 or LHR1234X
		}

		protected override void CheckBM_RL_NKDestinationPort()
		{
			base.CheckBM_RL_NKDestinationPort();
			var parent = Parent;
			var nctsHeader = NctsHeader;
			var info = parent.BM_RL_NKDestinationPortInfo;
			var destinationCountry = parent.BM_RL_NKDestinationPort;
			if (!destinationCountry.IsEmpty)
			{
				UniversalValidationHelper.CheckCountryOfDispatchOrDestinationIsAtLeastOneC0009Code(nctsHeader.Factory, parent.BM_InBondEntryType, nctsHeader.BH_RL_NKImportLoadPort, destinationCountry, nctsHeader.DefaultDataGroupingCode, info);
			}
			else if (!parent.HasGoodsItemsWithCountryOfDestination)
			{
				info.AddMessageError(Res.GetString("EF976D56-6E00-4BB5-9DC4-5A48F4DBAD2D", "Destination Country must be filled either in Declaration tab or Goods tab."));
			}
		}

		protected override void CheckBM_SealType()
		{
			base.CheckBM_SealType();
			ListValidation.MessageErrorIfInvalidCode(Parent.BM_SealTypeInfo);

			if (!Parent.BM_SealQty.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_SealTypeInfo);
			}
		}

		protected override void CheckBM_SealQty()
		{
			base.CheckBM_SealQty();
			var parent = Parent;
			MandatoryValidation.CheckNotNegative(parent.BM_SealQtyInfo);

			var sealQty = parent.BM_SealQty;
			var isPackageSeal = parent.BM_SealType == SealTypeList.Codes.PackageSeal;
			var numberOfEnteredSeals = isPackageSeal ? GetNumberOfPackageSeals() : GetNumberOfContainerSeals();
			if (numberOfEnteredSeals > sealQty && sealQty >= 0)
			{
				if (isPackageSeal)
				{
					parent.BM_SealQtyInfo.AddMessageError(Res.GetString("2E797E9A-3E4C-432D-B20D-B75F690825BF", "The maximum number of seals entered in the package's grid should be {0}.", sealQty));
				}
				else
				{
					parent.BM_SealQtyInfo.AddMessageError(Res.GetString("3E891BE3-FBEA-48C5-A775-5AA0E7704B92", "The maximum number of seal numbers entered in the container's grid should be {0}.", sealQty));
				}
			}
			else if (numberOfEnteredSeals.IsEmpty && sealQty > 0)
			{
				parent.BM_SealQtyInfo.AddMessageError(Res.GetString("92708802-2451-43F9-B345-BC60BC8E8C09", "You have not entered a Seal Number."));
			}
		}

		protected override void CheckBM_RN_NKTransportAtDepartureTrailer1Nationality()
		{
			base.CheckBM_RN_NKTransportAtDepartureTrailer1Nationality();
			var parent = Parent;

			if (!parent.BM_TransportAtDepartureTrailer1RegNo.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.BM_RN_NKTransportAtDepartureTrailer1NationalityInfo);
			}
		}

		protected override void CheckBM_RN_NKTransportAtDepartureTrailer2Nationality()
		{
			base.CheckBM_RN_NKTransportAtDepartureTrailer2Nationality();

			var parent = Parent;
			if (!parent.BM_TransportAtDepartureTrailer2RegNo.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.BM_RN_NKTransportAtDepartureTrailer2NationalityInfo);
			}
		}

		ZInt GetNumberOfPackageSeals()
		{
			if (NctsHeader != null)
			{
				if (NctsHeader.IsPhase5)
				{
					return NctsHeader.CusSeals.Cast<CusSeal>().Count(x => !x.BK_SealNumber.IsEmpty);
				}
				else
				{
					return NctsHeader.Seals.Cast<Seal>().Count(x => !x.CY_Data.IsEmpty);
				}
			}
			else
			{
				return ZInt.Zero;
			}
		}

		ZInt GetNumberOfContainerSeals()
		{
			var result = 0;
			NctsHeader?.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>().ForEach(container =>
			{
				result += container.Seal1.IsEmpty ? 0 : 1;
				result += container.Seal2.IsEmpty ? 0 : 1;
				result += container.AdditionalSeals.Cast<CusSeal>().Count(x => !x.BK_SealNumber.IsEmpty);
			});
			return result;
		}
	}
}
