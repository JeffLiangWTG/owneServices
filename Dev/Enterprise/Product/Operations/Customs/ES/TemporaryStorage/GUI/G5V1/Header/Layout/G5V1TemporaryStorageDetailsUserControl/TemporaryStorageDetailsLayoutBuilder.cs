using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public class TemporaryStorageDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, G5V1TemporaryStorageDetailsUserControlBag>
		where T : TemporaryStorageHeader
	{
		public override G5V1TemporaryStorageDetailsUserControlBag CommonBag { get; } = G5V1TemporaryStorageDetailsUserControlBag.Instance;

		protected override int MaxColumns => 2;
	}
}
