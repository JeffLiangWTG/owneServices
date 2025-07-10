using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Messaging.Module
{
	public partial class EDIMessageFilterControl : ZFilterStripControl
	{
		public EDIMessageFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
		}

		public EDIMessageFilterControl()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				ErrorReporter.ReportOnce("Cannot use parameterless constructor in production code. For design use only.");
			}
			InitializeComponent();
		}
	}
}
