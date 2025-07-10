using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Accounting.GUI
{
	public abstract class AccQueryClaimPlugIn : ZPlugIn
	{
		public AccQueryClaimPlugIn(IBusiness hostBusinessEntity) : base(hostBusinessEntity)
		{
		}

		#region Overrides

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return null; }
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		#endregion

		protected override Control GetNewUserControl()
		{
			Control result = new AccQueryClaimCollectionUserControl();
			result.Dock = DockStyle.Fill;
			return result;
		}

		protected OrgHeader Organisation
		{
			get { return (OrgHeader)HostBusinessEntity; }
		}

		protected BusinessObjectFactory fFactory
		{
			get { return HostBusinessEntity.Factory; }
		}
	}
}
