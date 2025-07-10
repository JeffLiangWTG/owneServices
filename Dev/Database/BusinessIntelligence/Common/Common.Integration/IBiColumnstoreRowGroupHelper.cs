using System.Data;
using CargoWise.Data;

namespace CargoWise.Bi.Common.Integration
{
	public interface IBiColumnstoreRowGroupHelper
	{
		DataTable GetColumnstoreRowGroupPhysicalState(AdminConnection auditConnection);
	}
}
