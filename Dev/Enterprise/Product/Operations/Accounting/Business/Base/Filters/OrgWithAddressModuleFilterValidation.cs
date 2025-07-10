using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Business.Base.Filters
{
	public class OrgWithAddressModuleFilterValidation : ModuleTextFilterValidation
	{
		public OrgWithAddressModuleFilterValidation(OrgWithAddressFilter parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly OrgWithAddressFilter parent;

		public void ValidateOrganization()
		{
			ValidateCalculatedProperty(parent.OrganizationInfo);
		}

		protected void CheckOrganization()
		{
			TypeValidation.CheckValidGuid(parent.OrganizationInfo);
		}

		public void ValidateAddress()
		{
			ValidateCalculatedProperty(parent.AddressInfo);
		}

		protected void CheckAddress()
		{
			TypeValidation.CheckValidGuid(parent.AddressInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateAddress();
			ValidateOrganization();
		}

		public override Type AutoValidationType
		{
			get { return this.GetType(); }
		}
	}
}
