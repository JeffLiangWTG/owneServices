using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.DFD.Business.Organisations
{
	class DFDOrgHeader : OrgHeader
	{
		public DFDOrgHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		protected override bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool shouldBeReadOnly = false;
			string propertyName = property.Name;
			if (IsARAPAccount &&
				(propertyName == "OH_Code" ||
				 propertyName == "OH_ScreeningStatus" ||
				 propertyName == "OH_FullName" ||
				 propertyName == "OH_RL_NKClosestPort" ||
				 propertyName == "OH_Language" ||
				 propertyName.StartsWith("MainWebURL") ||
				 propertyName.StartsWith("PrimaryRegistrationNumber")))
			{
				shouldBeReadOnly = !DFDSecurityCheckpoints.OrgModifyDetailsARAP.IsAllowed;
			}
			return shouldBeReadOnly || base.GetReadOnlySecurity(property);
		}

		public bool IsARAPAccount
		{
			get
			{
				foreach (OrgCompanyData companyData in CompanyDataCollection)
				{
					if (companyData.OB_IsDebtor || companyData.OB_IsCreditor)
					{
						return true;
					}
				}

				return false;
			}
		}
	}
}
