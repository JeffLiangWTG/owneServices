using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.MX.Module
{
	public partial class OrgSupplierPartFilterStripControl : Customs.Module.OrgSupplierPartFilterStripControl
	{
		[Obsolete("Do not call. Only for designer use.")]
		public OrgSupplierPartFilterStripControl()
		{
			InitializeComponent();
		}

		public OrgSupplierPartFilterStripControl(IBusinessObjectCollection gridCollection, OrgSupplierPartFilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
