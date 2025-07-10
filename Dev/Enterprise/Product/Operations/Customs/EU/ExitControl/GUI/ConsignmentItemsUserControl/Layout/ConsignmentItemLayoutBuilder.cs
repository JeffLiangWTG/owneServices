using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public class ConsignmentItemLayoutBuilder<T> : ColumnLayoutBuilder<T, ConsignmentItemControlBag> where T : Business.CusExitConsignmentItem
	{
		public override ConsignmentItemControlBag CommonBag => ConsignmentItemControlBag.Instance;

		protected override int MaxColumns => 1;
	}
}
