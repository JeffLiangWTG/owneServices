using System.Drawing;
using CargoWise.EntityFramework;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ServiceManager.Module
{
	public partial class ProcessControllerControl : ZFilterStripControl
	{
		public ProcessControllerControl(IBusinessObjectCollection gridCollection, ProcessControllerFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		void FilteredGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			if (e.ObjectAtRow is StmServiceHost host)
			{
				switch (host.IsResponding)
				{
					case StmServiceHost.ResponseStatus.Stopped:
						e.Colour = Color.Red;
						break;
					case StmServiceHost.ResponseStatus.Running:
						e.Colour = Color.YellowGreen;
						break;
					default:
						e.Colour = Color.Empty;
						break;
				}
			}
		}
	}
}
