using CargoWise.EntityFramework;
using CargoWise.Types;

namespace CargoWise.Bi.Product.Manager.Business
{
	public class CdcError : NonPersistentBusinessObject
	{
		public CdcError()
		{
		}

		public ZInt session_id { get; set; }
		public ZInt phase_number { get; set; }
		public ZDateTime entry_time { get; set; }
		public ZInt error_number { get; set; }
		public ZInt error_severity { get; set; }
		public ZInt error_state { get; set; }
		public ZString error_message { get; set; }
		public ZString start_lsn { get; set; }
		public ZString begin_lsn { get; set; }
		public ZString sequence_value { get; set; }
	}
}
