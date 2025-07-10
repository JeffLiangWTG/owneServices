using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Accounting.Module
{
	public class JobProfitLossPluginToConsol : ZPlugIn
	{
		public JobProfitLossPluginToConsol(IBusiness hostEntity) : base(hostEntity)
		{
			SetupProfitLossContainer();
		}

		#region Profit and Loss

		void SetupProfitLossContainer()
		{
			IJobCostingPlugIn plugin = HostBusinessEntity as IJobCostingPlugIn;
			if (plugin != null)
			{
				JobProfitLoss items = new JobProfitLoss(HostBusinessEntity.Factory);
				items.SetConsol(plugin);
				plugin.ProfitLossContainer.Add(items);
			}
		}

		#endregion

		#region ZPlugIn Members

		public override string Name
		{
			get { return (NoResString)"Consol Profit/Loss"; }
		}

		public override bool CanDelete
		{
			get { return true; }
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override Control GetNewUserControl()
		{
			if (fUserControl == null)
			{
				fUserControl = new ConsolJobProfitLossControl();
			}

			return fUserControl;
		}

		ConsolJobProfitLossControl fUserControl;

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get	{ return Env.Licence.Core; }
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (fUserControl != null)
				{
					fUserControl.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		#endregion
	}
}
