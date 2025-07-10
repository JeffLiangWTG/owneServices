using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ES.Module
{
	public partial class JobDeclarationFilterStripControl : EU.Module.JobDeclarationFilterStripControl
	{
		public JobDeclarationFilterStripControl(JobDeclarationModule module, IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(module, gridCollection, filterBusinessObject)
		{
		}
		protected override bool SupportsExitControlCore => true;
	}
}
