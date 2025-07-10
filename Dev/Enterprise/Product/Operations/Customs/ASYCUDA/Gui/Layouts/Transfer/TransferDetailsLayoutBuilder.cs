using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public class TransferDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, CommonTransferDetailsControlBag> where T : Business.AsycudaTransferHeader
	{
		public override CommonTransferDetailsControlBag CommonBag { get; } = CommonTransferDetailsControlBag.Instance;

		protected override int MaxColumns => 3;
	}
}
