using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public class CustomsMessagingPlugInSupportOrgHeaderWrapper : ICustomsMessagingPlugInSupport
	{
		public CustomsMessagingPlugInSupportOrgHeaderWrapper(OrgHeader orgHeader)
		{
			Argument.NotNull(orgHeader, "orgHeader");
			this.orgHeader = orgHeader;
		}

		#region ICustomsMessagingPlugInSupport

		public BusinessObject Master
		{
			get { return this.orgHeader; }
		}

		public bool PlugInVisible
		{
			get
			{
				var impAddInfo = GetImpAddInfo();
				if (impAddInfo != null && impAddInfo.ZO_IsCSAApprovedImporter)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public event EventHandler PlugInVisibilityDataChanged
		{
			add
			{
				var impAddInfo = GetImpAddInfo();
				if (impAddInfo != null)
				{
					impAddInfo.ZO_IsCSAApprovedImporterInfo.ValueChanged += value;
				}
			}
			remove
			{
				var impAddInfo = GetImpAddInfo();
				if (impAddInfo != null)
				{
					impAddInfo.ZO_IsCSAApprovedImporterInfo.ValueChanged -= value;
				}
			}
		}

		OrgImpAddInfo GetImpAddInfo()
		{
			if (Enterprise.Customs.CA.Registry.CACustomsDataRegistry.Instance.CSAFunctionActive.Value && GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Canada)
			{
				return OrgImpAddInfo.Get(this.orgHeader);
			}
			else
			{
				return null;
			}
		}

		#endregion

		readonly OrgHeader orgHeader;
	}
}
