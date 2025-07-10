using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class PackageAuditFailureWrapperCollection : GenericWrapperCollection<PackageAuditFailureWrapper>
	{
		#region Constructors

		public PackageAuditFailureWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public PackageAuditFailureWrapperCollection(WhsPackageAudit audit, BusinessObjectFactory factory)
			: base(factory)
		{
			if (audit?.PackageAuditFailureLines != null)
			{
				Load(audit.PackageAuditFailureLines);
			}
		}

		#endregion
	}
}
