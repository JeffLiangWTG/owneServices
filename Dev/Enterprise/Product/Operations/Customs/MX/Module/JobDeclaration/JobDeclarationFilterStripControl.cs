using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.MX.Module
{
	public partial class JobDeclarationFilterStripControl : Customs.Module.JobDeclarationFilterStripControl
	{
		[Obsolete("Do not call. Only for designer use.")]
		public JobDeclarationFilterStripControl()
		{
			InitializeComponent();
		}

		public JobDeclarationFilterStripControl(JobDeclarationModule module, IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(module, gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
