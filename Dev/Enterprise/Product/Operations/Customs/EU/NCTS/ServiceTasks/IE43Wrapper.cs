using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.ServiceTasks
{
	public class IE43Wrapper
	{
		public ZString MeansOfTransportAtDepartureIdentity { get; set; }
		public ZString MeansOfTransportAtDepartureNationality { get; set; }
		public ZDecimal TotalGrossMass { get; set; }
		public ZString TotalGrossMassUQ { get; set; }
		public IReadOnlyCollection<ZString> Seals { get; set; }
		public IReadOnlyCollection<IE43GoodsItemWrapper> GoodsItems { get; set; }
	}
}
