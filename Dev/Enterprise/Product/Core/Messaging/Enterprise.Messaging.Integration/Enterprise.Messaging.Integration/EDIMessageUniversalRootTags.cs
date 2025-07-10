using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Messaging.Integration
{
	public static class EdiMessageTags
	{
		public static class UniversalMessage
		{
			public static class RootTags
			{
				public const string Shipment = "UniversalShipment";
				public const string Event = "UniversalEvent";
				public const string Activity = "UniversalActivity";
				public const string Transaction = "UniversalTransaction";
				public const string TransactionBatch = "UniversalTransactionBatch";
				public const string Schedule = "UniversalSchedule";

				public static IEnumerable<string> GetNonRequestTags()
				{
					return new List<string>()
					{
						Shipment,
						Event,
						Activity,
						Transaction,
						TransactionBatch,
						Schedule
					};
				}

				public static bool IsRequestTag(string tagName) => !GetNonRequestTags().Contains(tagName);
			}
		}
	}
}
