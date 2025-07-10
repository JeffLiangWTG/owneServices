using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.GeneralLedgerData.Business
{
	public class AccGeneralLedgerDataLookups : AutoAccGeneralLedgerDataLookups
	{
		public AccGeneralLedgerDataLookups(AutoAccGeneralLedgerData parent) : base(parent)
		{
		}

		#region Organizations

		public OrgHeaderCollection Organizations
		{
			get
			{
				return new OrgHeaderCollection(Factory);
			}
		}

		#endregion

		#region Jobs

		public JobHeaderCollection Jobs
		{
			get { return new JobHeaderCollection(Factory); }
		}

		#endregion

		#region ChargeCodes

		public AccChargeCodeCollection ChargeCodes
		{
			get { return new AccChargeCodeCollection(Factory); }
		}

		#endregion

	}
}
