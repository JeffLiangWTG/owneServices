using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsHeaderCustomsOfficeRequirementHelper : CustomsOfficeRequirementHelper
	{
		public NctsHeaderCustomsOfficeRequirementHelper(NctsHeader header) : base(header)
		{
		}

		protected NctsHeader Header => (NctsHeader)OfficeCodeProvider;

		protected override string GetCacheKeyCombination()
		{
			var header = Header;
			var departureMovement = header.MovementHeader;
			var requirements = header.IsPhase5 ? "NctsDepartureMovementHeaderCustomsOfficeRequirementHelper.OtherRequirements" : "NctsHeaderCustomsOfficeRequirementHelper.OtherRequirements";
			return string.Join(".", requirements, header.BH_HeaderType, departureMovement?.BM_InBondEntryType, header.BH_ApplicationCode);
		}

		protected sealed override IEnumerable<CustomsOfficeRequirement> GetOtherRequirements() => Factory.GetCachedValue(GetCacheKeyCombination(), GetOtherRequirementsCore);

		protected virtual IEnumerable<CustomsOfficeRequirement> GetOtherRequirementsCore()
		{
			var result = new List<CustomsOfficeRequirement>();

			var nctsOfficeOfDestinationRequirement = GetNCTSOfficeOfDestinationRequirement();
			result.Add(nctsOfficeOfDestinationRequirement);

			if (Header.IsDepartureMovement)
			{
				var nctsOfficeOfDepartureRequirement = GetNCTSOfficeOfDepartureRequirement();
				result.Add(nctsOfficeOfDepartureRequirement);

				var nctsOfficeOfTransitRequirement = GetNCTSOfficeOfTransitRequirement();
				result.Add(nctsOfficeOfTransitRequirement);

				var nctsOfficeOfExitForTransitRequirement = GetNCTSOfficeOfExitForTransitRequirement();
				result.Add(nctsOfficeOfExitForTransitRequirement);
			}

			if (Header.IsArrivalMovement)
			{
				var nctsOfficeOfDestinationForArrivalRequirement = GetNCTSOfficeOfDestinationForArrivalRequirement();
				result.Add(nctsOfficeOfDestinationForArrivalRequirement);
			}
			return result.WhereNotNull();
		}

		protected virtual CustomsOfficeRequirement GetNCTSOfficeOfDestinationForArrivalRequirement()
		{
			var nctsOfficeOfDestinationForArrivalRequirement = new CustomsOfficeRequirement(
				officeRole: OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival,
				isMandatory: true,
				isLocalCountryOnly: false,
				friendlyName: OfficeCodes_NCTS.Descriptions.NCTSOfficeOfDestinationForArrival)
			{
				OfficeRolesForLookup = new ZString[] { OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination }
			};

			return nctsOfficeOfDestinationForArrivalRequirement;
		}

		protected virtual CustomsOfficeRequirement GetNCTSOfficeOfExitForTransitRequirement()
		{
			var nctsOfficeOfExitForTransitRequirement = new CustomsOfficeRequirement(
				officeRole: OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit,
				isMandatory: false,
				isLocalCountryOnly: false,
				friendlyName: OfficeCodes_NCTS.Descriptions.NCTSOfficeOfExitForTransit)
			{
				MaxOfficeCountLimit = !Header.IsPhase5 || Header.Configuration.ValidationRuleConfiguration.IsRuleTR0003Active ? 9 : null
			};

			return nctsOfficeOfExitForTransitRequirement;
		}

		protected virtual CustomsOfficeRequirement GetNCTSOfficeOfTransitRequirement()
		{
			var nctsHeader = Header;
			var departureMovement = nctsHeader.MovementHeader;
			var isPhase5 = nctsHeader.IsPhase5;
			var validationRuleConfiguration = nctsHeader.Configuration.ValidationRuleConfiguration;

			var nctsOfficeOfTransitRequirement = new CustomsOfficeRequirement(
				officeRole: OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit,
				isMandatory: departureMovement.IsInternalTransitProcedure || departureMovement.IsMixedConsignment,
				isLocalCountryOnly: false,
				friendlyName: OfficeCodes_NCTS.Descriptions.NCTSOfficeOfTransit)
			{
				MaxOfficeCountLimit = isPhase5 && validationRuleConfiguration.IsRuleTR0002Active ? 9 : null
			};

			if (isPhase5 && validationRuleConfiguration.IsRuleC0030Active)
			{
				nctsOfficeOfTransitRequirement.IsMandatory = false;
			}

			return nctsOfficeOfTransitRequirement;
		}

		protected virtual CustomsOfficeRequirement GetNCTSOfficeOfDepartureRequirement()
		{
			var nctsOfficeOfDepartureRequirement = new CustomsOfficeRequirement(
				officeRole: OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture,
				isMandatory: !Header.IsPhase5 || Header.Configuration.ValidationRuleConfiguration.IsRuleTR0007Active,
				isLocalCountryOnly: false,
				friendlyName: OfficeCodes_NCTS.Descriptions.NCTSOfficeOfDeparture);

			if (Header.IsPhase5)
			{
				nctsOfficeOfDepartureRequirement.ValidationMessage = Res.GetString("51AC8F26-8F2F-4711-BD0B-AB6544C0B64F",
					"[TR0007] The Declaration requires a Customs Office with Purpose 'DEP' (Departure Office).");

				if (!Header.Configuration.ValidationRuleConfiguration.IsRuleTR0009Active)
				{
					nctsOfficeOfDepartureRequirement.MaxOfficeCountLimit = null;
				}
			}

			return nctsOfficeOfDepartureRequirement;
		}

		protected virtual CustomsOfficeRequirement GetNCTSOfficeOfDestinationRequirement()
		{
			var nctsOfficeOfDestinationRequirement = new CustomsOfficeRequirement(
				officeRole: OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination,
				isMandatory: !Header.IsPhase5 || Header.Configuration.ValidationRuleConfiguration.IsRuleTR0006Active,
				isLocalCountryOnly: false,
				friendlyName: OfficeCodes_NCTS.Descriptions.NCTSOfficeOfDestination);

			if (Header.IsPhase5)
			{
				nctsOfficeOfDestinationRequirement.ValidationMessage = Res.GetString("6F7AB39B-06EC-439E-AB0E-F7B47E8365EE",
					"[TR0006] The Declaration requires a Customs Office with Purpose 'DES' (Destination Office).");

				if (!Header.Configuration.ValidationRuleConfiguration.IsRuleTR0008Active)
				{
					nctsOfficeOfDestinationRequirement.MaxOfficeCountLimit = null;
				}
			}

			return nctsOfficeOfDestinationRequirement;
		}
	}
}
