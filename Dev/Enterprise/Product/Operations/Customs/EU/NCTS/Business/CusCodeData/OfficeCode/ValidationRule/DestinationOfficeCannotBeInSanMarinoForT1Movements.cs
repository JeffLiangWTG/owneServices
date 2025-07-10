using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class DestinationOfficeCannotBeInSanMarinoForT1Movements : RefCountryValidationRule
	{
		readonly NctsEuOfficeCode officeCode;

		public DestinationOfficeCannotBeInSanMarinoForT1Movements(NctsEuOfficeCode officeCode)
		{
			this.officeCode = officeCode;
		}

		public override bool IsApplied
		{
			get
			{
				return officeCode.CY_Code.EqualsIgnoringCase(EuOfficeCodesTypes.Codes.OfficeOfDestination)
					&& officeCode.Parent is BusinessObject parent
					&& (parent as NctsDepartureMovementHeader ?? (parent as NctsHeader)?.MovementHeader) is NctsDepartureMovementHeader movementHeader
					&& movementHeader.BM_InBondEntryType.EqualsIgnoringCase(NctsDeclarationTypeList.Codes.T1);
			}
		}

		protected override ValidationResult ValidateCore(RefCountry country)
		{
			return country.Code == Core.Constants.CountryCodes.SanMarino
				? ValidationResult.Invalid(Res.GetString("ab1a432a-c5a3-41b5-b833-f52f034f7eff", "Destination Office cannot be in San Marino for T1 Movements."))
				: ValidationResult.Valid;
		}
	}
}
