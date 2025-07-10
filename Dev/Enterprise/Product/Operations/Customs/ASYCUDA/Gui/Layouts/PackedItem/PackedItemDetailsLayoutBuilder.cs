using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public class PackedItemDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, CommonPackedItemDetailsControlBag> where T : Business.AsycudaPack
	{
		public override CommonPackedItemDetailsControlBag CommonBag { get; } = CommonPackedItemDetailsControlBag.Instance;

		protected override int MaxColumns => 3;
	}
}
