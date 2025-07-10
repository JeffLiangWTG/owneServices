
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class BaseRecordLoaderAndCreator
	{
		public BaseRecordLoaderAndCreator(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		#region Implementation

		protected readonly BusinessObjectFactory Factory;

		#region Proxy Org

		OrgHeader fProxyOrg;
		public OrgHeader ProxyOrg
		{
			get
			{
				if (fProxyOrg == null)
				{
					fProxyOrg = GlbBranch.CurrentBranch.OrgProxy;
				}
				return fProxyOrg;
			}
			set
			{
				fProxyOrg = value;
			}
		}

		#endregion

		#region ProxyBranch

		GlbBranch fProxyBranch;
		public GlbBranch ProxyBranch
		{
			get
			{
				if (fProxyBranch == null)
				{
					fProxyBranch = GlbBranch.CurrentBranch;
				}
				return fProxyBranch;
			}
			set
			{
				fProxyBranch = value;
			}
		}

		#endregion

		#region SetOrgProxyFromPremiseID

		protected void SetOrgProxyFromPremiseID(ZString premiseID)
		{
			if (!premiseID.IsEmpty)
			{
				ZQuery filter = new ZQuery(OrgCusCodeSchema.OK_CustomsRegNo, premiseID);
				OrgCusCode[] codes = (OrgCusCode[])Factory.Load(typeof(OrgCusCode), filter);
				foreach (OrgCusCode code in codes)
				{
					OrgHeader possibleProxyOrg = code.Header;
					if (possibleProxyOrg.OH_IsUnpackDepot && possibleProxyOrg.IsProxyOrg(GlbCompany.CurrentCompany))
					{
						ProxyOrg = possibleProxyOrg;
						foreach (GlbBranch branch in GlbCompany.GetCurrentCompany(Factory).Branches)
						{
							if (branch.GB_OH_OrgProxy == ProxyOrg.PK)
							{
								ProxyBranch = branch;
								break;
							}
						}
						break;
					}
				}
			}
		}

		#endregion

		#endregion
	}
}
