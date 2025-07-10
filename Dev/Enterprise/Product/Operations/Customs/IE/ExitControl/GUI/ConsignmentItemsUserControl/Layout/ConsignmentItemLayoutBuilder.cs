using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.ExitControl.GUI
{
	public class ConsignmentItemLayoutBuilder<T> : ColumnLayoutBuilder<T, ConsignmentItemControlBag> where T : EU.ExitControl.Business.CusExitConsignmentItem
	{
		public override ConsignmentItemControlBag CommonBag => ConsignmentItemControlBag.Instance;

		protected override int MaxColumns => 1;
	}
}
