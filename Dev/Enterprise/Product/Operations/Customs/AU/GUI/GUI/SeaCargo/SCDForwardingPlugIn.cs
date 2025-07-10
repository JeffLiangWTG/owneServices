using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public class SCDForwardingPlugIn : SCDPlugIn
	{
		public SCDForwardingPlugIn(IBusiness hostEntity) : base(hostEntity)
		{
		}

		#region Overrides

		protected internal override MultiMessageManager GetManager()
		{
			return null;
		}

		protected override IBusiness GetBusinessEntityForPlugIn_CMR()
		{
			return null;
		}

		protected override IBusiness GetBusinessEntityForPlugIn_Legacy()
		{
			return null;
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			return null;
		}

		protected override Control GetNewUserControl()
		{
			return null;
		}

		protected override ZBool HasUserControl
		{
			get { return false; }
		}
		#endregion

		#region Implementation

		protected ZGuid originalUnpackDepot;
		protected CommonConsol consol;

		protected void JK_OA_UnpackDepotAddressInfo_ValueChanged(object sender, EventArgs e)
		{
			if (!originalUnpackDepot.IsEmpty)
			{
				var address = Factory.Load<OrgAddress>(originalUnpackDepot);
				if (address != null && address.Header != null && address.Header.IsProxyOrgOfAnyCompany())
				{
					if (IsConsolDepotRegistered())
					{
						var newAddress = Factory.Load<OrgAddress>(consol.JK_OA_UnpackDepotAddress);
						if (newAddress == null || newAddress.Header == null || !newAddress.Header.IsProxyOrgOfAnyCompany())
						{
							Globals.Message.ShowError("Unable to change unpack depot address because Sea Cargo Depot Messaging has been started.", "Sea Cargo Depot");
							consol.JK_OA_UnpackDepotAddress = originalUnpackDepot;
						}
					}
				}
			}
		}

		protected bool IsConsolDepotRegistered()
		{
			bool result = false;
			foreach (CommonContainer container in consol.Containers)
			{
				StmALog sCDLog = container.Logs.MostRecentLogByEventTime(AutoEvents.SeaCargoDepotEvent);
				if (sCDLog != null)
				{
					if (sCDLog.SL_Reference.StartsWith(DepotEvents.ImpendingCargo)
						|| sCDLog.SL_Reference.StartsWith(DepotEvents.CargoArrived)
						|| sCDLog.SL_Reference.StartsWith(DepotEvents.CargoUnpacked))
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		protected internal override bool ForceCMR
		{
			get { return false; }
		}

		protected internal override bool ForceLegacy
		{
			get { return false; }
		}

		protected override ZDateTime DateOfFirstArrival
		{
			get
			{
				ZDateTime result = ZDateTime.Now;
				if (consol != null && !consol.IsDeleted)
				{
					if (consol.JK_DatePortOfFirstArrival.IsEmpty)
					{
						result = consol.JK_DatePortOfFirstArrival;
					}
					else if (!consol.JK_JX_JB_E_ARV.IsEmpty)
					{
						result = consol.JK_JX_JB_E_ARV;
					}
				}
				return result;
			}
		}

		#endregion
	}
}
