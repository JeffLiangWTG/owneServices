using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public class TempStoragePremisesDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, TempStoragePremisesDetailsControlBag>
		where T : CusTempStorageRegPremises
	{
		public override TempStoragePremisesDetailsControlBag CommonBag { get; } = TempStoragePremisesDetailsControlBag.Instance;

		protected override int MaxColumns => 1;
	}
}
