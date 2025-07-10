using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AE.Module;

public partial class JobDeclarationFilterControl : Customs.Module.JobDeclarationFilterStripControl
{
	public JobDeclarationFilterControl(JobDeclarationModule module, IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
		: base(module, gridCollection, filterBusinessObject)
	{
		InitializeComponent();
	}
}
