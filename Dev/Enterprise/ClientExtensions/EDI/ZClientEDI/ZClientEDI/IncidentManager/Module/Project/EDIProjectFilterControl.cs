using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public partial class EDIProjectFilterControl : ProcessManagement.Module.ProjectFilterControl
	{
#if DEBUG
		[Obsolete("This constructor is just for the designer", true)]
		public EDIProjectFilterControl()
		{
			InitializeComponent();
		}
#endif

		public EDIProjectFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
