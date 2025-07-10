using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientLicenceFeeCollection : ActiveBusinessObjectCollection<ClientLicenceFee>
	{
		public ClientLicenceFeeCollection(LicenceCompany master)
			: base(master.Factory, master, new ZQuery(), ClientLicenceFeeSchema.L8_LC)
		{
			Master = master;
		}

		readonly LicenceCompany Master;

		public IEnumerable<ClientLicenceFee> GetMatched(ZDateTime billingDate)
		{
			return this.Cast<ClientLicenceFee>().Where(x => x.IsDateRangeMatched(billingDate));
		}

		#region Implementation

		protected override void SetRelationshipDefaultsForElementCore(ClientLicenceFee newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			newElement.L8_LC = Master.PK;
		}

		protected override bool AllowNew
		{
			get { return EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed; }
		}

		#endregion
	}
}

