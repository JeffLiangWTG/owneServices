using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public sealed class TempStorageRegisterDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, TempStorageRegisterDetailsControlBag> where T : CusTempStorageRegHeader
	{
		public override TempStorageRegisterDetailsControlBag CommonBag => TempStorageRegisterDetailsControlBag.Instance;

		protected override int MaxColumns => 3;
	}
}
