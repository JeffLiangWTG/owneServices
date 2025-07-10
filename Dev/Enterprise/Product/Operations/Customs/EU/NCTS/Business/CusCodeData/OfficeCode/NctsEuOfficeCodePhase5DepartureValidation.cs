using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsEuOfficeCodePhase5DepartureValidation : NctsEuOfficeCodePhase5Validation
	{
		public NctsEuOfficeCodePhase5DepartureValidation(NctsEuOfficeCode parent) : base(parent)
		{
		}

		protected override void CheckCY_Code()
		{
			base.CheckCY_Code();
			var parent = Parent.Parent;
			if (parent is NctsDepartureMovementHeader movementHeader
				&& movementHeader.Header is NctsHeader header)
			{
				CheckRuleB1836(header, movementHeader);
				CheckRuleC0587_1(header, movementHeader);
				CheckRuleC0587_2(header, movementHeader);
				CheckCY_CodeTRADuplicatesInCustomsOffices(header, movementHeader);
			}
			CheckRuleG0587(parent);
			CheckR0006();
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			if (Parent.MovementHeader is NctsDepartureMovementHeader movementHeader
				&& movementHeader.Header is NctsHeader header)
			{
				CheckRuleC0030(movementHeader);
				CheckConditionTXTC0030Condition(movementHeader);
				CheckDataRuleC0030_1(header, movementHeader);
				CheckRuleR0103(header, movementHeader);
			}
			ValidateCY_Code();
		}

		protected override void CheckCY_Date()
		{
			base.CheckCY_Date();
			CheckCY_DateRuleR0005();
			CheckCY_DateRuleB1831OrRuleC0598();
		}

		void CheckRuleR0103(NctsHeader header, NctsDepartureMovementHeader movementHeader)
		{
			var parent = Parent;
			if (parent.ValidationDecider is INctsEuOfficeCodeDeparturePhase5ValidationDecider departurePhase5ValidationDecider
				&& departurePhase5ValidationDecider.IsRuleR0103Active
				&& parent.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit)
			{
				var exitForTransitOffice = movementHeader.ExitForTransitCustomsOfficeCodeList.FirstOrDefault();
				var transitOffice = NctsMovementHeaderValidationHelper.OfficeOfTransit(movementHeader);
				var destinationOffice = movementHeader.DestinationCustomsOfficeForDeparture;
				if (exitForTransitOffice != null && transitOffice != null && destinationOffice != null)
				{
					if (exitForTransitOffice.OfficeCode == transitOffice.OfficeCode || exitForTransitOffice.OfficeCode == destinationOffice.OfficeCode)
					{
						parent.CY_DataInfo.AddMessageError(header.Configuration.ValidationRuleConfiguration.Messages.R0103Message);
					}
				}
			}
		}

		void CheckRuleG0587(BusinessObject parent)
		{
			if (parent != null)
			{
				NctsDepartureMovementHeader movementHeader;
				var header = parent as NctsHeader;
				if (header != null)
				{
					movementHeader = header.MovementHeader;
				}
				else
				{
					movementHeader = parent as NctsDepartureMovementHeader;
					if (movementHeader != null)
					{
						header = movementHeader.Header;
					}
				}
				if (header != null
					&& movementHeader != null
					&& !movementHeader.HasExitForTransitOffice(ZString.Empty))
				{
					var cl247Codes = parent.Factory.GetCountryCodesNCTSCountryOutsideCustomsSecurityAgreementArea();
					if (header.Configuration.ValidationRuleConfiguration.IsRuleG0587Active && header.CountriesOfRouting.Any(x => cl247Codes.ContainsCode(x.CY_Data)))
					{
						Parent.CY_CodeInfo.AddMessageError(Res.GetString("610C3DCE-8CBB-42C9-B86A-23AE8AF117FB", "[G0587] You have not entered The Customs Office of Exit for Transit, Purpose Code TXT."));
					}
				}
			}
		}

		void CheckCY_DateRuleR0005()
		{
			var parent = Parent;
			var date = parent.CY_Date;
			if (!date.IsEmpty
				&& date.IsInThePast()
				&& parent.IsOfficeOfTransit
				&& Parent.MovementHeader is NctsDepartureMovementHeader movementHeader
				&& (movementHeader.BM_Phase.IsEmpty || movementHeader.BM_Phase == NctsMovementHeaderTransactionStatusList.Codes.Declaration)
				&& parent.ValidationDecider is INctsEuOfficeCodeDeparturePhase5ValidationDecider departurePhase5ValidationDecider
				&& departurePhase5ValidationDecider.IsRuleR0005Active
				&& parent.EffectiveHeader is NctsHeader header
				&& !(departurePhase5ValidationDecider.IsRuleB1904Active && header.IsInPhase5TransitionPeriod))
			{
				parent.CY_DateInfo.AddMessageError(Res.GetString("C941345C-3076-4C97-B8ED-FF56A0199019", "The Estimated Arrival Date Time for Customs Office of Transit can't be earlier or equal to current date time."));
			}
		}

		void CheckRuleB1836(NctsHeader header, NctsDepartureMovementHeader movementHeader)
		{
			if (IsRuleB1836Applicable)
			{
				var parent = Parent;
				if (movementHeader.BM_InBondEntryType.EqualsIgnoringCase(NctsPhase5DeclarationTypeList.Codes.TIR))
				{
					if (parent.IsOfficeOfTransit)
					{
						parent.CY_CodeInfo.AddWarning(Res.GetString("CCBC48FA-961F-48E5-A651-698DDEABC045", "[B1836] Customs Office with Purpose='TRA' is not required for a 'TIR' Declaration."));
					}
				}
				else
				{
					if ((parent.CY_Code.EqualsIgnoringCase(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination) || parent.CY_Code.EqualsIgnoringCase(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture))
						&& !movementHeader.HasTransitOffice()
						&& header.Factory.GetCountryCodesCTC().ContainsCode(parent.CY_Data.Left(2)))
					{
						parent.CY_CodeInfo.AddMessageError(Res.GetString("996179FD-92C3-473C-BF2C-D2207A127043", "[B1836] Customs Office with Purpose='TRA' is required."));
					}
				}
			}
		}

		void CheckDataRuleC0030_1(NctsHeader header, NctsDepartureMovementHeader movementHeader)
		{
			var parent = Parent;
			if (parent.ValidationDecider is INctsEuOfficeCodeDeparturePhase5ValidationDecider departurePhase5ValidationDecider
				&& parent.IsOfficeOfTransit
				&& !parent.CY_Data.IsEmpty
				&& departurePhase5ValidationDecider.IsRuleC0030_1Active
				&& IsTIRorT2SM(movementHeader))
			{
				parent.CY_DataInfo.AddMessageError(Res.GetString("53C29354-FAD2-4658-8221-EFF28EE41B29", "{0} Customs Office of Transit cannot be used for Declaration TIR or T2SM", ValidationRuleCodeConstants.C0030_1.GetRuleCodeMessagePrefix()));
			}

			static ZBool IsTIRorT2SM(NctsDepartureMovementHeader movementHeader) => movementHeader.IsTIRDeclaration || movementHeader.BM_InBondEntryType.EqualsIgnoringCase(NctsPhase5DeclarationTypeList.Codes.T2SM);
		}

		void CheckConditionTXTC0030Condition(NctsDepartureMovementHeader movementHeader)
		{
			var parent = Parent;
			if (IsRuleC0030Applicable
				&& movementHeader.HasExitForTransitOffice(ZString.Empty)
				&& !movementHeader.HasTransitOffice()
				&& parent.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit)
			{
				parent.CY_DataInfo.AddMessageError(NctsConstants.ValidationMessages.YouHaveNotEnteredACustomsOfficeOfTransitDeclared);
			}
		}

		bool IsRuleB1836Applicable
		{
			get
			{
				var parent = Parent;
				return parent.ValidationDecider is INctsEuOfficeCodeDeparturePhase5ValidationDecider departurePhase5ValidationDecider && departurePhase5ValidationDecider.IsRuleB1836Active
					&& parent.MovementHeader?.Header is NctsHeader header && header.IsInPhase5TransitionPeriod;
			}
		}

		bool IsRuleC0030Applicable
		{
			get
			{
				var parent = Parent;
				return parent.ValidationDecider is INctsEuOfficeCodeDeparturePhase5ValidationDecider departurePhase5ValidationDecider && departurePhase5ValidationDecider.IsRuleC0030Active
					&& !IsRuleB1836Applicable;
			}
		}

		void CheckRuleC0587_1(NctsHeader header, NctsDepartureMovementHeader movementHeader)
		{
			var parent = Parent;
			if (parent.CY_Code.EqualsIgnoringCase(OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit)
				&& header.Configuration.ValidationRuleConfiguration.IsRuleC0587_1Active
				&& movementHeader.IsSecurityTypeBTHOrEXI)
			{
				var transitOffices = movementHeader.CustomsOffices.Where(co => co.IsOfficeOfTransit);

				if (transitOffices.Any(x => x.IsOfficeCountryConsideredInEuForSafetyAndSecurity))
				{
					parent.CY_CodeInfo.AddMessageError(Res.GetString("1067D21F-1038-4183-A123-1BB88E7D3E41", "[C0587-1] A Customs Office of Exit for Transit (Purpose = TXT) must not be present"));
				}
			}
		}

		void CheckRuleC0587_2(NctsHeader header, NctsDepartureMovementHeader movementHeader)
		{
			var parent = Parent;
			var configuration = header.Configuration;
			if (parent.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit &&
				configuration.ValidationRuleConfiguration.IsRuleC0587_2Active &&
				(movementHeader.BM_InBondEntryType == NctsPhase5DeclarationTypeList.Codes.TIR ||
					movementHeader.BM_TypeOfSecurity != NctsTypeOfSecurityList.Codes.EXI))
			{
				var warning = Res.GetString("8FC8E9B3-8C2E-48CB-8E45-48476A414B45",
											"[C0587-2] Customs Office of Exit (TXT) will not be declared if the Security value is not EXI or if declaration type is TIR");
				parent.CY_CodeInfo.AddWarning(warning);
			}
		}

		void CheckCY_CodeTRADuplicatesInCustomsOffices(NctsHeader header, NctsDepartureMovementHeader movementHeader)
		{
			var parent = Parent;
			if (header.Configuration.ValidationRuleConfiguration.IsRuleR0003Active
				&& CY_CodeTRADuplicates(parent, movementHeader))
			{
				parent.CY_CodeInfo.AddMessageError(Res.GetString("2CD075EC-9BFF-460C-89AD-C0AE86A4EA0D", "[R0003] The entered Customs Office of Transit is already present."));
			}
		}

		bool CY_CodeTRADuplicates(NctsEuOfficeCode parent, NctsDepartureMovementHeader movementHeader)
		{
			return parent.IsOfficeOfTransit
				&& !parent.CY_Data.IsEmpty
				&& movementHeader.CustomsOfficesForDeparture.Where(x => x.IsOfficeOfTransit && x.CY_Data.EqualsIgnoringCase(Parent.CY_Data)).Take(2).Count() > 1;
		}

		void CheckCY_DateRuleB1831OrRuleC0598()
		{
			if (Parent.ValidationDecider is INctsEuOfficeCodeDeparturePhase5ValidationDecider departurePhase5ValidationDecider
				&& departurePhase5ValidationDecider.IsRuleB1831Active
				&& Parent.EffectiveHeader is NctsHeader header
				&& header.IsInPhase5TransitionPeriod)
			{
				CheckCY_DateRuleB1831();
			}
			else
			{
				CheckCY_DateRuleC0598();
			}
		}

		void CheckCY_DateRuleB1831()
		{
			var parent = Parent;
			if (parent.MovementHeader is NctsDepartureMovementHeader movementHeader
				&& movementHeader.IsSecurityTypeENTOrBTHOrEXI)
			{
				var customsOfficeForDeparture = movementHeader.CustomsOffices.Find(r => r.IsOfficeDeparture).FirstOrDefault();

				if ((customsOfficeForDeparture != null && !customsOfficeForDeparture.IsInCL010CountryList)
					&& (parent.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit
						&& parent.IsInCL010CountryList
						&& parent.CY_Date.IsEmpty))
				{
					parent.CY_DateInfo.AddMessageError(
						Res.GetString(
							"86D18163-F6D8-48C4-9484-AFA376B0465E",
							"{0} Time can't be empty for Customs Office with Purpose='TRA'.",
							ValidationRuleCodeConstants.B1831.GetRuleCodeMessagePrefix()));
				}
			}
		}

		void CheckCY_DateRuleC0598()
		{
			var parent = Parent;
			if (parent.ValidationDecider is INctsEuOfficeCodeDeparturePhase5ValidationDecider departurePhase5ValidationDecider
				&& departurePhase5ValidationDecider.IsRuleC0598Active
				&& parent.CY_Date.IsEmpty
				&& IsArrivalTimeMandatory())
			{
				parent.CY_DateInfo.AddError(Res.GetString("663E8467-689E-4958-A74A-6A054921900E", "[C0598] You have not entered a Time (Arrival Date and Time Estimated)."));
			}
		}

		void CheckR0006()
		{
			var parent = Parent;
			var currentOfficeCountryCode = parent.OfficeCountryCode;
			var movementHeader = parent.MovementHeader;
			var header = movementHeader?.Header;
			if (parent.ValidationDecider is INctsEuOfficeCodePhase5ValidationDecider phase5ValidationDecider
				&& phase5ValidationDecider.IsRuleR0006Active
				&& !currentOfficeCountryCode.IsEmpty
				&& parent.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination
				&& header != null
				&& !header.IsInPhase5TransitionPeriod)
			{
				if (parent.IsInCL112CountryList && !movementHeader.HasTransitOffice(currentOfficeCountryCode))
				{
					parent.CY_CodeInfo.AddMessageError(header.Configuration.ValidationRuleConfiguration.Messages.R0006aMessage);
				}
				else
				{
					if (parent.IsInCL010CountryList
						&& movementHeader.DepartureCustomsOffice is NctsEuOfficeCode departureCustomsOffice
						&& departureCustomsOffice.IsInCL112CountryList
						&& !movementHeader.TransitCustomsOfficeCodeList.Cast<NctsEuOfficeCode>().Any(x => x.IsInCL010CountryList))
					{
						parent.CY_CodeInfo.AddMessageError(header.Configuration.ValidationRuleConfiguration.Messages.R0006bMessage);
					}
				}
			}
		}

		bool IsArrivalTimeMandatory()
		{
			var parent = Parent;
			return parent.MovementHeader is NctsDepartureMovementHeader movementHeader
				&& parent.IsOfficeOfTransit
				&& movementHeader.IsSecurityTypeENTOrBTH
				&& parent.IsOfficeCountryConsideredInEuForSafetyAndSecurity;
		}

		void CheckRuleC0030(NctsDepartureMovementHeader movementHeader)
		{
			if (IsRuleC0030Applicable
				&& !movementHeader.HasTransitOffice()
				&& movementHeader.DepartureCustomsOffice is NctsEuOfficeCode officeOfDeparture
				&& movementHeader.DestinationCustomsOffice is NctsEuOfficeCode officeOfDestination)
			{
				var propertyInfo = Parent.CY_DataInfo;

				if (officeOfDeparture.OfficeCountryCode.EqualsIgnoringCase(Core.Constants.CountryCodes.Andorra)
					|| officeOfDestination.OfficeCountryCode.EqualsIgnoringCase(Core.Constants.CountryCodes.Andorra))
				{
					propertyInfo.AddMessageError(NctsConstants.ValidationMessages.YouHaveNotEnteredACustomsOfficeOfTransitDeclared);
				}
				else if (officeOfDeparture.IsInCL112CountryList || officeOfDestination.IsInCL112CountryList)
				{
					propertyInfo.AddMessageError(NctsConstants.ValidationMessages.YouHaveNotEnteredACustomsOfficeOfTransitDeclared);
				}
			}
		}
	}
}
