using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.MY.Module
{
	public partial class JobDeclarationFilterStripControl : Customs.Module.JobDeclarationFilterStripControl
	{
		public JobDeclarationFilterStripControl(JobDeclarationModule module, IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(module, gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}

