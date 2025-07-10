using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Billing.Business
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class ClientFaxPrice : AutoClientFaxPrice
	{
		public ClientFaxPrice(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region ReadOnly

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return !EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion
	}
}
