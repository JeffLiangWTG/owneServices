using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class MethodTwoToSixDocumentLayoutBuilder<T> : ColumnLayoutBuilder<T, MethodTwoToSixControlBag> where T : BusinessObject
	{
		public MethodTwoToSixDocumentLayoutBuilder(MethodTwoToSixControlBag instance)
		{
			this.instance = instance;
		}
		readonly MethodTwoToSixControlBag instance;
		public override MethodTwoToSixControlBag CommonBag => instance;

		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		protected override int MaxColumns => 1;
	}
}
