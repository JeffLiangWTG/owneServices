using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class LicenceDatabaseFlattened : AutoLicenceDatabaseFlattened
	{
		public GlbBranch Branch { get; set; }
		public AccTaxRate Tax { get; set; }
		public AccChargeCode SalesTax { get; set; }
		public OrgDebtorGroup DebtorGroup { get; set; }
	}
}

