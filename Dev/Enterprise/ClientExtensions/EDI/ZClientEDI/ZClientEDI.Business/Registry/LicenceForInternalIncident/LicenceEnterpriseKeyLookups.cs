using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class LicenceEnterpriseKeyLookups : ZLookups
	{
		public LicenceEnterpriseKeyLookups(LicenceEnterpriseKey parent)
			: base(parent) { }

		#region Implementation

		protected new LicenceEnterpriseKey Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (LicenceEnterpriseKey)base.Parent; }
		}

		#endregion

		public LicenceEnterpriseCollection LicenceEnterpriseKeyList
		{
			get { return licenceEnterpriseKeyList ?? (licenceEnterpriseKeyList = new LicenceEnterpriseCollection(Parent.CurrentFactory)); }
		}
		LicenceEnterpriseCollection licenceEnterpriseKeyList;
	}
}

