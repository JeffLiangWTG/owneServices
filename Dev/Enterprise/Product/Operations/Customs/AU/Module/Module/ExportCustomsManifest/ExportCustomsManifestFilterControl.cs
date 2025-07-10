using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Module
{
	public partial class ExportCustomsManifestFilterControl : ZFilterStripControl
	{
		public ExportCustomsManifestFilterControl()
		{
			InitializeComponent();
		}

		public ExportCustomsManifestFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
