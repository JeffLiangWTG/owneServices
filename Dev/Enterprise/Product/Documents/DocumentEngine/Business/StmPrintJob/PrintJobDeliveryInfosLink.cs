using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.Scheduler.Business;

namespace Enterprise.DocumentEngine.Business
{
	public sealed class PrintJobDeliveryInfosLink : IService
	{
		PrintJobDeliveryInfosLink(BusinessObjectFactory factory)
		{
			map = new Dictionary<StmPrintJob, IEnumerable<DeliveryInfo>>();
			factory.ServiceContainer.AddService(this);
		}

		readonly IDictionary<StmPrintJob, IEnumerable<DeliveryInfo>> map;

		public static PrintJobDeliveryInfosLink GetInstance(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, nameof(factory));

			return factory.ServiceContainer.GetService<PrintJobDeliveryInfosLink>()
				?? new PrintJobDeliveryInfosLink(factory);
		}

		public IEnumerable<StmPrintJob> PrintJobs => map.Keys;

		public void Add(StmPrintJob printJob, IEnumerable<DeliveryInfo> deliveryInfos)
		{
			if (printJob == null)
			{
				return;
			}

			map[printJob] = deliveryInfos ?? Enumerable.Empty<DeliveryInfo>();
		}

		public IEnumerable<DeliveryInfo> Get(StmPrintJob printJob)
		{
			IEnumerable<DeliveryInfo> result;

			if (map.TryGetValue(printJob, out result))
			{
				return result;
			}

			return Enumerable.Empty<DeliveryInfo>();
		}
	}
}
