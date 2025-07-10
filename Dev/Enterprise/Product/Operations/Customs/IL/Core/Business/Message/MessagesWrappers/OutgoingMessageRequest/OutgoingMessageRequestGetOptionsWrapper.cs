using System;
using CargoWise.Common;
using CargoWise.Customs.IL.MessageDefinitions.OutgoingMessageRequest;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business
{
	public class OutgoingMessageRequestGetOptionsWrapper : IOutgoingMessageRequestGetOptions
	{
		OutgoingMessageRequestGetOptionsWrapper(string serviceName, ZDateTime fromDate, ZDateTime toDate)
		{
			ServiceName = serviceName;
			FromDate = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(fromDate.ToDateTime(), DateTimeKind.Utc), TimeZoneInfo.Utc);
			ToDate = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(toDate.ToDateTime(), DateTimeKind.Utc), TimeZoneInfo.Utc);
		}

		internal static OutgoingMessageRequestGetOptionsWrapper NewOrNull(string serviceName, ZDateTime fromDate, ZDateTime toDate)
			=> serviceName.IsNullOrEmpty() || fromDate.IsEmpty || toDate.IsEmpty
			? null
			: new OutgoingMessageRequestGetOptionsWrapper(serviceName, fromDate, toDate);

		public DateTime FromDate { get; }

		public DateTime ToDate { get; }

		public string ServiceName { get; }

		public string BatchId => null;

		public string CorrelationId => null;
	}
}
