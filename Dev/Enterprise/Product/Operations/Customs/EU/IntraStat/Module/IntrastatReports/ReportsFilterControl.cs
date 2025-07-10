using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Intrastat.Module
{
	public partial class ReportsFilterControl : ZFilterStripControl<ReportsFilterStrip>
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public ReportsFilterControl()
		{
			InitializeComponent();
		}

		public ReportsFilterControl(IBusinessObjectCollection gridCollection, ReportsFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
