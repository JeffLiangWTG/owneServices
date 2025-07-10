using CargoWise.EntityFramework;
using Enterprise.Customs.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CA.Module
{
	public partial class CACusPermitFilterControl : CusPermitFilterControl
	{
		public CACusPermitFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
		}
	}
}
