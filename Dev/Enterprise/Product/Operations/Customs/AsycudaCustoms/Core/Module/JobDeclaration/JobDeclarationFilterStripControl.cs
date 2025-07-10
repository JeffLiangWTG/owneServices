using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AsycudaCustoms.Module
{
	public partial class JobDeclarationFilterStripControl : Customs.Module.JobDeclarationFilterStripControl
	{
		public JobDeclarationFilterStripControl(JobDeclarationModule module, IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(module, gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
