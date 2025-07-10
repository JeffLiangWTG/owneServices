using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.SWL.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Client.SWL.GUI
{
	public class ShipnetSetupPlugIn : ZPlugIn
	{
		public ShipnetSetupPlugIn(IBusiness hostBusinessEntity)
			: base(hostBusinessEntity)
		{
			this.HostBusinessEntity.Factory.Saved += new BusinessObjectFactory.SavedEventHandler(HostOrg_FactorySaved);
		}

		#region Overrides

		public new OrgHeader HostBusinessEntity
		{
			get { return (OrgHeader)base.HostBusinessEntity; }
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				HostBusinessEntity.Factory.Saved -= new BusinessObjectFactory.SavedEventHandler(HostOrg_FactorySaved);
				fPlugInBizObj = null;
			}

			base.Dispose(disposing);
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return null; }
		}

		public override string Name
		{
			get { return Res.GetString("8041c172-edbf-4920-9360-0debf671bca8", "Setup Shipnet Data"); }
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override Control GetNewUserControl()
		{
			Control result = new ShipnetSetupUserControl();
			result.Dock = DockStyle.Fill;

			return result;
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return PlugInBizObj;
		}

		#endregion

		#region Implementation

		void HostOrg_FactorySaved(BusinessObjectFactory factory, bool saveSucceeded)
		{
			if (saveSucceeded && fPlugInBizObj != null)
			{
				SWLDataRegistry.Instance.SetOrDeleteShipnetSetupBusinessObject(HostBusinessEntity.CompanyData.PK, fPlugInBizObj);
				fPlugInBizObj.HasChanges = false;
			}
		}

		#region PlugInBizObj

		ShipnetSetupBusinessObject fPlugInBizObj;
		ShipnetSetupBusinessObject PlugInBizObj
		{
			get
			{
				if (fPlugInBizObj == null)
				{
					ShipnetSetupBusinessObject registryBizObj = (ShipnetSetupBusinessObject)SWLDataRegistry.Instance.GetShipnetSetupBusinessObject(HostBusinessEntity.CompanyData.PK);
					fPlugInBizObj = (ShipnetSetupBusinessObject)registryBizObj.Clone(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, HostBusinessEntity.CompanyData.PK.ToGuid()), HostBusinessEntity.Factory);
				}

				return fPlugInBizObj;
			}
		}

		#endregion

		#endregion
	}
}
