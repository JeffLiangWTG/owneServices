using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.Module
{
	public partial class ImportFromSumARegisterFilterStripControl : ZFilterStripControl
	{
		public ImportFromSumARegisterFilterStripControl(ActiveBusinessObjectCollection<CusTempStorageRegLine> gridCollection, ImportFromSumARegisterFilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
		}
	}
}
