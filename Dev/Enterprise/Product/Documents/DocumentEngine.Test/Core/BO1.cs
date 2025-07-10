using CargoWise.Types;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class BO1 : BOBase
	{
		public ZDecimal DecimalField { get; set; }

		public ZInt ZIntNumber { get; set; }
	}
}
