using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public class TempStorageRegisterPremisesLayoutBuilder<T> : ColumnLayoutBuilder<T, TempStorageRegisterPremisesControlBag> where T : CusTempStorageRegPremises
	{
		public override TempStorageRegisterPremisesControlBag CommonBag => TempStorageRegisterPremisesControlBag.Instance;

		protected override int MaxColumns => 1;
	}
}
