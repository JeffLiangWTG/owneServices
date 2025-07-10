using Enterprise.Customs.EU.NCTS.GUI;

namespace Enterprise.Customs.FR.GUI.NCTS
{
	class DepartureDetailsLayoutBuilder<T> : EU.NCTS.GUI.DepartureDetailsLayoutBuilder<T> where T : EU.NCTS.Business.NctsHeader
	{
		public override DepartureDetailsControlBag CommonBag => DepartureDetailsControlBag.Instance;

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();
			SetVisibility(CommonBag.DateLimitDateEdit, x => !x.MovementHeader.IsTIRDeclaration, x => x.MovementHeader.BM_InBondEntryTypeInfo);
		}
	}
}
