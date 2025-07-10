using System.Drawing;
using CargoWise.EntityFramework;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ServiceManager.Module
{
	public partial class StmServiceTaskFilterControl : ZFilterStripControl
	{
		public StmServiceTaskFilterControl(IBusinessObjectCollection gridCollection, StmServiceTaskFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		public StmServiceTaskFilterControl()
		{
			InitializeComponent();
		}

		void FilteredGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			if (e.ObjectAtRow is StmServiceTask task)
			{
				switch (task.StatusString.ToString())
				{
					case ServiceManagerHelper.StatusUnknown: e.Colour = Color.Gold; break;
					case ServiceManagerHelper.StatusIdle: e.Colour = Color.LightBlue; break;
					case ServiceManagerHelper.StatusRunning: e.Colour = Color.YellowGreen; break;
					case ServiceManagerHelper.StatusLastRunFailed: e.Colour = Color.LightYellow; break;
					default: e.Colour = Color.Empty; break;
				}

				if (task.ErrorCountLast24Hours > 0)
				{
					e.Colour = Color.Orange;
				}
			}
		}
	}
}
