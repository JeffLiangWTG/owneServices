using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.Scheduler.Business;

namespace Enterprise.DocumentEngine.Business.Testing
{
	sealed class PrintJobDeliveryInfosLinkTest : TestCaseWithFactory
	{
		public void TestAddThenGet()
		{
			var printJob = Factory.New<StmPrintJob>();
			var deliveryInfos = new[]
			{
				new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document),
				new DeliveryInfo(DeliveryInfo.DeliveryFormats.TIFF)
			};

			var link = PrintJobDeliveryInfosLink.GetInstance(Factory);
			link.Add(printJob, deliveryInfos);

			AssertArrayEqualsByElements("link returns empty collection of elements",
				deliveryInfos, link.Get(printJob).ToArray());
		}

		public void TestAddNullDeliveryInfos()
		{
			var printJob = Factory.New<StmPrintJob>();

			var link = PrintJobDeliveryInfosLink.GetInstance(Factory);
			link.Add(printJob, null);

			AssertArrayEqualsByElements("link returns same collection of DeliveryInfos",
				Enumerable.Empty<DeliveryInfo>().ToArray(), link.Get(printJob).ToArray());
		}

		public void TestGetInstanceOnNullFactoryThrowsException()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => PrintJobDeliveryInfosLink.GetInstance(null));
		}
	}
}
