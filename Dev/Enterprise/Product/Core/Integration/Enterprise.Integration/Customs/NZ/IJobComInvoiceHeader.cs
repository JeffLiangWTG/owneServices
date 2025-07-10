using CargoWise.Types;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class NZ
		{
			public interface IJobComInvoiceHeader : IBaseJobComInvoiceHeader
			{
				ZString JZ_IsGSTPrePaid { get; set; }
				ZString JZ_SupplierGSTNumber { get; set; }
				IJobComInvoiceLineViewCollection JobComInvoiceLines { get; }
		}
		}
	}
}
