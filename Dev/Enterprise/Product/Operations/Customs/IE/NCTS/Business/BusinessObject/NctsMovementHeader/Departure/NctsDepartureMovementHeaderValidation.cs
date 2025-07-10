using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal;
using CusAuthorizationUsage = Enterprise.Customs.EU.NCTS.Business.CusAuthorizationUsage;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class NctsDepartureMovementHeaderValidation : NctsDepartureMovementHeaderPhase5Validation
	{
		public NctsDepartureMovementHeaderValidation(NctsDepartureMovementHeader parent)
			: base(parent)
		{
		}

		protected override bool ShouldCheckRuleC0387ForBM_ForeignDestPortKCode =>
			!(Parent.BM_TypeOfSecurity.EqualsIgnoringCase(NctsTypeOfSecurityList.Codes.EXI) &&
				Parent.BM_SpecificCircumstance.EqualsIgnoringCase(NctsSpecificCircumstanceIndicatorList.Codes.XXX));

		protected override bool ShouldCheckRuleC0387ForBM_PortOfPresentationCode =>
			!Parent.BM_TypeOfSecurity.EqualsIgnoringCase(NctsTypeOfSecurityList.Codes.NON);

		protected override bool ShouldCheckRuleC0387ForBM_PlaceOfLoading =>
			!Parent.BM_TypeOfSecurity.EqualsIgnoringCase(NctsTypeOfSecurityList.Codes.NON);

		protected new NctsDepartureMovementHeader Parent => (NctsDepartureMovementHeader)base.Parent;

		protected override void CheckBM_RL_NKForeignDestPort()
		{
		}

		protected override void CheckBM_ExportTransportMode()
		{
			var parent = Parent;
			if (parent.BM_ExportTransportMode.IsEmpty)
			{
				var targetInfo = parent.BM_ExportTransportModeInfo;
				if (parent.BM_CustomsStatus.EqualsIgnoringCase(NCTS5DepartureCustomsStatusList.Codes.PreLodged) && !parent.BM_TypeOfSecurity.EqualsIgnoringCase(NctsTypeOfSecurityList.Codes.NON))
				{
					targetInfo.AddMessageError(Res.GetString("233EF3CF-F7B2-4AFB-9D42-F001F47AE12C", "{0} is mandatory for Security Declaration.", targetInfo.HumanReadableName));
				}
				else
				{
					MandatoryValidation.AddYouHaveNotEnteredMessage(targetInfo);
				}
			}
		}

		protected override void CheckBM_ForeignDestPortKCodeMandatory()
		{
			base.CheckBM_ForeignDestPortKCodeMandatory();
			var parent = Parent;
			var targetInfo = parent.BM_ForeignDestPortKCodeInfo;
			if (parent.BM_TypeOfSecurity != NctsTypeOfSecurityList.Codes.NON && parent.BM_TypeOfSecurity != NctsTypeOfSecurityList.Codes.EXI)
			{
				var description = Res.GetString("9742E7B1-9083-4A4A-90C8-67981317D5A3", "Country/Region Code or an UNLOCO for Place of Unloading");
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo, description, messagePrefix: "[C0191] ");
			}
		}

		protected override ZBool IsExportDateMandatory
		{
			get
			{
				var cusAuthorizationUsages = Parent.Header.IsPhase5Departure ? (IBusinessObjectCollection<CusAuthorizationUsage>)Parent.CusAuthorizationUsages : Parent.Header.CusAuthorizationUsages;
				return base.IsExportDateMandatory && cusAuthorizationUsages.Any(x => x.AGC_Code == CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit);
			}
		}

		protected override void CheckBM_ExportDate()
		{
			base.CheckBM_ExportDate();

			var date = Parent.BM_ExportDate;
			if (!date.IsEmpty
				&& Parent.Header.IsPhase5Departure
				&& Parent.BM_CustomsStatus.IsEmpty
				&& !date.IsInTheFutureDatePartOnly)
			{
				Parent.BM_ExportDateInfo.AddMessageError(Res.GetString("4D912FFD-C65E-4A21-A769-45DE2B0A49EF", "Date Limit cannot be earlier or equal to current date."));
			}
		}

		protected override void CheckBM_TransportAtDepartureType()
		{
			base.CheckBM_TransportAtDepartureType();

			var parent = Parent;
			if (parent.BM_TransportAtDepartureType.IsEmpty &&
				parent.IsInPhase5TransitionPeriod &&
				parent.Header.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>().All(c => !c.IsContainerised))
			{
				parent.BM_TransportAtDepartureTypeInfo.AddMessageError(Res.GetString("97043198-0901-45E3-9E94-D0F4D978787C", "You have not entered a Type of ID."));
			}
		}

		protected override void CheckBM_TransportAtDeparture()
		{
			base.CheckBM_TransportAtDeparture();
			var parent = Parent;
			if (parent.BM_TransportAtDeparture.IsEmpty && !parent.BM_TransportAtDepartureType.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.BM_TransportAtDepartureInfo);
			}
		}

		protected override void CheckBM_RN_NKTransportAtDepartureCountry()
		{
			base.CheckBM_RN_NKTransportAtDepartureCountry();
			var parent = Parent;
			if (parent.BM_RN_NKTransportAtDepartureCountry.IsEmpty && !parent.BM_TransportAtDepartureType.IsEmpty && parent.BM_InlandTransportMode != TransportModeCodeList.Codes.Rail)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.BM_RN_NKTransportAtDepartureCountryInfo);
			}
		}

		protected override void CheckTirCarnetExpiryDateMandatory()
		{
			//Intentionally kept blank
		}

		protected override void CheckBM_TOLCarrierCode()
		{
		}

		protected override void CheckBM_ActiveBorderIdentificationType()
		{
			if (IsTypeOfID_Required)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_ActiveBorderIdentificationTypeInfo);
			}
			else
			{
				base.CheckBM_ActiveBorderIdentificationType();
			}
		}

		bool IsTypeOfID_Required
		{
			get
			{
				var parent = Parent;
				switch (parent.BM_ExportTransportMode)
				{
					case ModeOfTransportList.Codes._1_SeaTransport:
					case ModeOfTransportList.Codes._2_RailTransport:
					case ModeOfTransportList.Codes._3_RoadTransport:
					case ModeOfTransportList.Codes._4_AirTransport:
					case ModeOfTransportList.Codes._8_InlandWaterwayTransport:
						return true;
					default:
						return false;
				}
			}
		}
	}
}
