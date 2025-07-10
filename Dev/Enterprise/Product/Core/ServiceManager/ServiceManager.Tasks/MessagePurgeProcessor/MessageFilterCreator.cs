using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ServiceManager.Tasks.MessagePurgeProcessor
{
	class MessageFilterCreator : BaseFilterCreator
	{
		public MessageFilterCreator(Dictionary<ZString, List<MessageTypeParams>> messageTypes, ZDateTime minMessageCreateTimeUtc, ZDateTime maxMessageCreateTimeUtc)
			: base(minMessageCreateTimeUtc, maxMessageCreateTimeUtc)
		{
			MessageTypes = messageTypes;
			Condition = BuildCondition();
		}

		public Dictionary<ZString, List<MessageTypeParams>> MessageTypes { get; private set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		protected override string BuildCondition()
		{
			var filter = new ZQuery(EDIMessageSchema.EM_ApplicationCode, MessageTypes.Keys);
			filter.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, FakeMaxCreateTime);
			filter.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, FakeMinCreateTime);

			var newFilter = new ZQuery();

			foreach (var applicationCode in MessageTypes.Keys)
			{
				foreach (var messageType in MessageTypes[applicationCode])
				{
					var singleAppCodeFilter = new ZQuery(EDIMessageSchema.EM_ApplicationCode, applicationCode);
					AddMessageFilter(singleAppCodeFilter, messageType);
					newFilter.AddToFilter(singleAppCodeFilter, JoinCondition.Or);
				}
			}

			filter.AddToFilter(newFilter);
			return ReplaceFakeValuesByParameters(filter.LiteralTextSqlFormatted);
		}

		static void AddMessageFilter(ZQuery filter, MessageTypeParams messageType)
		{
			if (messageType.PurgeType.Equals(PurgeTypeList.MessageType))
			{
				filter.AddToFilter(EDIMessageSchema.EM_MessageType, messageType.MessageType);
			}
			else if (messageType.PurgeType.Equals(PurgeTypeList.MessageSubType))
			{
				filter.AddToFilter(EDIMessageSchema.EM_MessageSubType, messageType.MessageSubType);
			}
			else if (messageType.PurgeType.Equals(PurgeTypeList.MessageTypeAndSubType))
			{
				filter.AddToFilter(EDIMessageSchema.EM_MessageType, messageType.MessageType);
				filter.AddToFilter(EDIMessageSchema.EM_MessageSubType, messageType.MessageSubType);
			}
		}
	}

	class MessageTypeParams
	{
		public MessageTypeParams(ICodeDescription purgeType, string messageType, string messageSubType)
		{
			PurgeType = purgeType;
			MessageType = messageType;
			MessageSubType = messageSubType;
		}

		public ICodeDescription PurgeType { get; private set; }
		public string MessageType { get; private set; }
		public string MessageSubType { get; private set; }
	}
}
