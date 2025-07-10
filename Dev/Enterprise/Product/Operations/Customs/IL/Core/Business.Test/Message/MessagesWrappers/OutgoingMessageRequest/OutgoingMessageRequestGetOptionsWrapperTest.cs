using System;
using CargoWise.Customs.IL.MessageDefinitions.OutgoingMessageRequest;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class OutgoingMessageRequestGetOptionsWrapperTest : DataProviderTestCase<IOutgoingMessageRequestGetOptions>
	{
		public void TestNewOrNull()
		{
			AssertNull("serviceName is Must", OutgoingMessageRequestGetOptionsWrapper.NewOrNull(null, new ZDateTime(2025, 03, 26), new ZDateTime(2025, 03, 27)));
			AssertNull("fromDate is Must", OutgoingMessageRequestGetOptionsWrapper.NewOrNull("ServiceName", ZDateTime.Empty, new ZDateTime(2025, 03, 27)));
			AssertNull("toDate is Must", OutgoingMessageRequestGetOptionsWrapper.NewOrNull("ServiceName", new ZDateTime(2025, 03, 27), ZDateTime.Empty));
			AssertNotNull(OutgoingMessageRequestGetOptionsWrapper.NewOrNull("ServiceName", new ZDateTime(2025, 03, 26), new ZDateTime(2025, 03, 27)));
		}

		public void TestFromDate()
		{
			AssertEquals(new DateTime(2025, 03, 26), Provider.FromDate);
			AssertEquals(DateTimeKind.Utc, Provider.FromDate.Kind);
		}

		public void TestToDate()
		{
			AssertEquals(new DateTime(2025, 03, 27), Provider.ToDate);
			AssertEquals(DateTimeKind.Utc, Provider.ToDate.Kind);
		}

		public void TestServiceName()
		{
			AssertEquals("ServiceName", Provider.ServiceName);
		}

		public void TestBatchId()
		{
			AssertNullOrEmpty(Provider.BatchId);
		}

		public void TestCorrelationId()
		{
			AssertNullOrEmpty(Provider.CorrelationId);
		}

		protected override IOutgoingMessageRequestGetOptions GetProvider()
		{
			return OutgoingMessageRequestGetOptionsWrapper.NewOrNull("ServiceName", new ZDateTime(2025, 03, 26), new ZDateTime(2025, 03, 27));
		}
	}
}
