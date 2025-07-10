using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Billing.Business.Testing
{
	public class UsageCollectorTestHelper
	{
		public UsageCollectorTestHelper(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
		}

		public IUsageEDIMessage[] LoadUsageMessages() => LoadMessages("USG");
		public IUsageEDIMessage[] LoadUsageSummaryMessages() => LoadMessages("USS");

		IUsageEDIMessage[] LoadMessages(string messageType)
		{
			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, messageType);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, "TRX");
			query.AddToFilter(EDIMessageSchema.EM_IsActive, true);
			query.AddToFilter(EDIMessageSchema.EM_Status, "CAP");
			query.AddToFilter(EDIMessageSchema.EM_MessageType, messageType);

			var messages = factory.Load<IUsageEDIMessage>(query);
			return messages.OrderByDescending(m => m.EM_SystemCreateTimeUtc).ToArray();
		}

		public IUsageEDIMessage[] LoadUsageMessages(string featureCode) => LoadMessages(featureCode, "USG");
		public IUsageEDIMessage[] LoadUsageSummaryMessages(string featureCode) => LoadMessages(featureCode, "USS");

		public IUsageEDIMessage[] LoadMessages(string featureCode, string messageType)
		{
			var messages = LoadMessages(messageType);
			return messages.Where(m => m.GetProperty<string>(UsageProperties.FeatureCode) == featureCode).ToArray();
		}

		public bool AssertUsageMessagesContains(string featureCode, List<(string name, object value)> properties) => AssertMessagesContains(featureCode, "USG", properties);
		public bool AssertUsageSummaryMessagesContains(string featureCode, List<(string name, object value)> properties) => AssertMessagesContains(featureCode, "USS", properties);

		public bool AssertMessagesContains(string featureCode, string messageType, List<(string name, object value)> properties)
		{
			var messages = LoadMessages(featureCode, messageType);
			return messages.Any(m => DoesMessageContainsProperties(m));

			bool DoesMessageContainsProperties(IEDIMessage message)
			{
				foreach (var property in properties)
				{
					var propertyValueText = property.value is int || property.value is ZInt ? $"{property.value}" : $"\"{property.value}\"";

					if (!string.IsNullOrEmpty(property.value.ToString()) && !message.EM_MessageText.Contains($"\"{property.name}\": {propertyValueText}"))
					{
						return false;
					}
				}

				return true;
			}
		}

		public bool AssertUsageMessagesCount(string featureCode, int count) => AssertMessagesCount(featureCode, "USG", count);
		public bool AssertUsageSummmaryMessagesCount(string featureCode, int count) => AssertMessagesCount(featureCode, "USS", count);

		public bool AssertMessagesCount(string featureCode, string messageType, int count)
		{
			var messages = LoadMessages(featureCode, messageType);
			return messages.Length == count;
		}

		readonly BusinessObjectFactory factory;
	}
}
