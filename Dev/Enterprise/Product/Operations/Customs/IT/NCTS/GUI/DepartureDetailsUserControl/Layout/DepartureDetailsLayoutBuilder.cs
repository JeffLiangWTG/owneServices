using Enterprise.Customs.IT.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.GUI;

public sealed class DepartureDetailsLayoutBuilder : EU.NCTS.GUI.DepartureDetailsLayoutBuilder<NctsHeader>
{
	protected override int MaxColumns => 1;

	protected override void SetDefaultVisibilities()
	{
		base.SetDefaultVisibilities();
		SetVisibility(CommonBag.DateLimitDateEdit, x => !x.MovementHeader.IsTIRDeclaration, x => x.MovementHeader.BM_InBondEntryTypeInfo);
	}
}
