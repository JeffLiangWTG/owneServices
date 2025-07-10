using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class TIRCanOnlyGoToEU : MustBeMemberOfEU
	{
		readonly NctsEuOfficeCode officeCode;

		public TIRCanOnlyGoToEU(NctsEuOfficeCode officeCode)
		{
			this.officeCode = officeCode;
		}

		public override bool IsApplied
		{
			get
			{
				return officeCode.Parent is BusinessObject parent
					&& (parent as NctsDepartureMovementHeader ?? (parent as NctsHeader)?.MovementHeader) is NctsDepartureMovementHeader movementHeader
					&& movementHeader.BM_InBondEntryType.EqualsIgnoringCase(NctsDeclarationTypeList.Codes.TIR);
			}
		}

		protected override string GetInvalidString(RefCountry country) => Res.GetString("e91b2e25-e908-4a50-9520-679ec203740d", "TIR can only go to countries in the EU.");
	}
}
