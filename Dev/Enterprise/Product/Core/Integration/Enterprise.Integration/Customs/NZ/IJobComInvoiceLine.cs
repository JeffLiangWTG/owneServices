using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class NZ
		{
			public interface IJobComInvoiceLine : IBaseJobComInvoiceLine
			{
				ZString PackagingMarks1 { get; set; }
				ZString JI_RX_NKLinePriceCurr { get; }
			}
		}
	}
}
