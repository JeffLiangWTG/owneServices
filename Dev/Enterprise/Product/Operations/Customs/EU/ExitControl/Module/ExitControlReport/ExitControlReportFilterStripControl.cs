using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.Module
{
	public partial class ExitControlReportFilterStripControl : ZFilterStripControl
	{
		[Obsolete("Use the constructor that takes a module, collection and filter strip, this paramless constructor is just for the designer.", true)]
		public ExitControlReportFilterStripControl() : base()
		{
			InitializeComponent();
		}

		public ExitControlReportFilterStripControl(IBusinessObjectCollection gridCollection, ExitControlReportFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
