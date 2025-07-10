using CargoWise.EntityFramework;

namespace CargoWise.Bi.Product.Manager.Business
{
	public class AuditTableCollection : NonPersistentBusinessObjectCollection<AuditTable>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AuditTable("", "");
		}
	}
}
