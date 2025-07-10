using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public class CusTempStorageRegLineItemDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, CusTempStorageRegLineItemDetailsControlBag> where T : ES.Business.CusTempStorage.CusTempStorageRegHeader
	{
		public override CusTempStorageRegLineItemDetailsControlBag CommonBag => CusTempStorageRegLineItemDetailsControlBag.Instance;

		protected override int MaxColumns => 2;
	}
}
