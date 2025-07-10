using CargoWise.EntityFramework;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Module
{
	public partial class ImportFromTemporaryStorageRegisterFilterStripControl : ZFilterStripControl
	{
		public ImportFromTemporaryStorageRegisterFilterStripControl(ActiveBusinessObjectCollection<CusTempStorageRegLine> gridCollection, ImportFromTemporaryStorageRegisterFilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
		}
	}
}
