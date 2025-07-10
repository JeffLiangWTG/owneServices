using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class DepartureDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, DepartureDetailsControlBag> where T : Business.NctsHeader
	{
		public override DepartureDetailsControlBag CommonBag => DepartureDetailsControlBag.Instance;

		protected override int MaxColumns => 1;

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();

			SetVisibility(CommonBag.TirCarnetNumberTextBox, x => x.MovementHeader.IsTIRDeclaration, x => x.MovementHeader.BM_InBondEntryTypeInfo);
			SetVisibility(CommonBag.SimplifiedProcedureAndReducedDataSetUserControl, x => !x.MovementHeader.IsTIRDeclaration, x => x.MovementHeader.BM_InBondEntryTypeInfo);
			SetVisibility(CommonBag.DateLimitDateEdit, x => x.MovementHeader.IsSimplifiedNctsProcedure && !x.MovementHeader.IsTIRDeclaration, x => x.MovementHeader.IsSimplifiedNctsProcedureInfo, x => x.MovementHeader.BM_InBondEntryTypeInfo);
			SetVisibility(CommonBag.AdditionalDeclarationTypeDropEdit, x => x.Configuration.UseAdditionalDeclarationType);
			SetVisibility(CommonBag.PresentationDateTimeOffsetEdit, x => x.Configuration.UsePresentationDateTime);
			SetVisibility(CommonBag.OverrideFreightDetailsCheckBox, x => x.IsPluggedIn);
		}
	}
}
