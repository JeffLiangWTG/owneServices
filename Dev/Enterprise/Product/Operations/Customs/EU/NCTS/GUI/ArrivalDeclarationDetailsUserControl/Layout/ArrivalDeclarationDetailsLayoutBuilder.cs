using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class ArrivalDeclarationDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, ArrivalDeclarationDetailsControlBag> where T : Business.NctsHeader
	{
		public override ArrivalDeclarationDetailsControlBag CommonBag => ArrivalDeclarationDetailsControlBag.Instance;

		public override bool NarrowColumnForMediumControls => true;

		protected override int MaxColumns => 1;

		protected override void SetDefaultCaptions()
		{
			base.SetDefaultCaptions();

			SetCaption(CommonBag.AcceptanceDateEdit, _ => Res.GetData("257b6fb3-a387-4595-9338-29cff3a99749", "Acceptance Date"));
		}
	}
}
