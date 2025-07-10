using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.Module
{
	public partial class SimplifiedDeclarationFilterStripControl : ZFilterStripControl
	{
		public SimplifiedDeclarationFilterStripControl(ActiveBusinessObjectCollection<CusReconEntry> gridCollection, SimplifiedDeclarationFilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
		}
	}
}
