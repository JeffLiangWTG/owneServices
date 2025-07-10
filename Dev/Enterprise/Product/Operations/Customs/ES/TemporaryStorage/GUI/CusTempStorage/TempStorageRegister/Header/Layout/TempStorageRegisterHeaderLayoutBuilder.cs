using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public class TempStorageRegisterHeaderLayoutBuilder<T> : ColumnLayoutBuilder<T, TempStorageRegisterHeaderControlBag> where T : CusTempStorageRegHeader
	{
		public override TempStorageRegisterHeaderControlBag CommonBag => TempStorageRegisterHeaderControlBag.Instance;

		protected override int MaxColumns => 2;
	}
}
