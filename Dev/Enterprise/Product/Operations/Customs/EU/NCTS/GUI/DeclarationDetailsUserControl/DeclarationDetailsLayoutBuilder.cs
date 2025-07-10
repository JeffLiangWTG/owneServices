using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class DeclarationDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, DeclarationDetailsControlBag> where T : Business.NctsDepartureMovementHeader
	{
		public override DeclarationDetailsControlBag CommonBag => DeclarationDetailsControlBag.Instance;

		protected override int MaxColumns => 2;
	}
}
