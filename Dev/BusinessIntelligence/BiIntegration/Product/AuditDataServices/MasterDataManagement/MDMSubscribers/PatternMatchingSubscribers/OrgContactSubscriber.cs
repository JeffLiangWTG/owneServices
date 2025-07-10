using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.MDM.Subscribers
{
	public abstract class OrgContactSubscriber : PatternMatchingSubscriber<OrgContact>
	{
		protected override string PKColumn
		{
			get { return OrgContactSchema.Constants.PK; }
		}

		public override ITableSchema Table
		{
			get { return OrgContactSchema.Instance; }
		}
	}
}
