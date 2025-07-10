using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.MDM.Subscribers
{
	public abstract class OrgAddressSubscriber : PatternMatchingSubscriber<OrgAddress>
	{
		protected override string PKColumn
		{
			get { return OrgAddressSchema.Constants.PK; }
		}

		public override ITableSchema Table
		{
			get { return OrgAddressSchema.Instance; }
		}
	}
}
