using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class AccApportionmentTemplateFilterControl : ZFilterStripControl
	{
		public AccApportionmentTemplateFilterControl(IBusinessObjectCollection gridCollection, AccApportionmentTemplateFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}

