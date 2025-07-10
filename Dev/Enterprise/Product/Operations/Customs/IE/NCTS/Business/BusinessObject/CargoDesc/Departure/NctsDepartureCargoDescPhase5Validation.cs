using System.Linq;

namespace Enterprise.Customs.IE.NCTS.Business;

public sealed class NctsDepartureCargoDescPhase5Validation : EU.NCTS.Business.NctsDepartureCargoDescPhase5Validation
{
	public NctsDepartureCargoDescPhase5Validation(EU.NCTS.Business.NctsDepartureCargoDesc parent) : base(parent)
	{
	}

	new NctsDepartureCargoDesc Parent => (NctsDepartureCargoDesc)base.Parent;

	protected override void CheckBY_GrossWeight()
	{
		base.CheckBY_GrossWeight();
		var parent = Parent;
		if (!parent.IsInPhase5TransitionPeriod && parent.BY_GrossWeight.IsEmpty && parent.Packages.Cast<NctsPackage>().Any(package => !package.B5_UnitCount.IsEmpty))
		{
			parent.BY_GrossWeightInfo.AddMessageError(Res.GetString("B60EA6B9-2FBE-417F-8338-7750EED55F45", "[B2101] You have not entered Gross Mass."));
		}
	}

	protected override void CheckBY_GrossWeightUnit()
	{
		base.CheckBY_GrossWeightUnit();
		var parent = Parent;
		if (!parent.IsInPhase5TransitionPeriod && parent.BY_GrossWeightUnit.IsEmpty && parent.Packages.Cast<NctsPackage>().Any(package => !package.B5_UnitCount.IsEmpty))
		{
			parent.BY_GrossWeightUnitInfo.AddMessageError(Res.GetString("E9151EF7-FC05-4E1D-8496-5E9FFC7A4C2E", "[B2101] You have not entered Gross Mass Unit."));
		}
	}
}
