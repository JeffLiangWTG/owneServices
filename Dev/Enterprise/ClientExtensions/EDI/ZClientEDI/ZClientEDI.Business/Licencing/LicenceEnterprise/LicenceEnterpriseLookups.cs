using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Licensing.Billing.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class LicenceEnterpriseLookups : Licensing.Billing.Business.LicenceEnterpriseLookups
	{
		public LicenceEnterpriseLookups(AutoLicenceEnterprise parent)
			: base(parent)
		{
		}

		#region Organisations

		public override OrgHeaderCollection Headers
		{
			get { return new EDIOrgHeaderCollection(Factory); }
		}

		#endregion
	}
}

