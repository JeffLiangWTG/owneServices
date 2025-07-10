using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor
{
	public class SystemXmlMessageProcessorBase : StandardXmlProcessor
	{
		#region SupportedMessages

		public void AddSupportedMessage(string code, string objectFactoryTypeId)
		{
			messageActionTypeMap[code] = new Tuple<string, Type>(objectFactoryTypeId, null);
		}

		public void AddSupportedMessage(string code, Type actionType)
		{
			messageActionTypeMap[code] = new Tuple<string, Type>(null, actionType);
		}

		public void ClearSupportedMessages()
		{
			messageActionTypeMap.Clear();
		}

		readonly Dictionary<string, Tuple<string, Type>> messageActionTypeMap = new Dictionary<string, Tuple<string, Type>>();

		#endregion

		internal override bool NewMessageAvailable(out string[] companyCodes)
		{
			companyCodes = null;
			return false;
		}

		internal override bool ProcessNewMessages(CancellationToken token)
		{
			var messageBatch = new MessageBatch(GetEDIMessagePKs());
			return ProcessMessageBatch(messageBatch, token);
		}

		internal override bool AlwaysProcessOneByOne
		{
			get { return true; }
		}

		internal static List<ZGuid> LoadEDIMessagePKs(BusinessObjectFactory factory, IEnumerable<string> messageSubTypes,
			int batchSize, bool mostRecentFirst)
		{
			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add(ZSqlParameter.New("@AppCode", ApplicationCodeList.Codes.SYS, EDIMessageSchema.EM_ApplicationCode));
			sqlParams.Add(ZSqlParameter.New("@MessageStatus", EDIMessage.Status.Queued, EDIMessageSchema.EM_Status));
			sqlParams.Add(ZSqlParameter.New("@ReceiveTransmit", EDIMessage.Direction.Receive, EDIMessageSchema.EM_ReceiveTransmit));

			StringBuilder sqlBuilder = new StringBuilder();
			sqlBuilder.AppendLine("SELECT TOP " + batchSize + " " + EDIMessageSchema.PK.Name + " FROM dbo.EDIMessage WHERE");
			sqlBuilder.AppendLine(EDIMessageSchema.Constants.EM_ApplicationCode + " = @AppCode");
			sqlBuilder.AppendLine("AND " + EDIMessageSchema.Constants.EM_Status + " = @MessageStatus");
			sqlBuilder.AppendLine("AND " + EDIMessageSchema.Constants.EM_ReceiveTransmit + " = @ReceiveTransmit");
			sqlBuilder.Append("AND " + EDIMessageSchema.Constants.EM_MessageSubType + " IN (");

			int index = 0;
			foreach (string code in messageSubTypes)
			{
				string paramName = "@MST" + index;
				if (index > 0)
				{
					sqlBuilder.Append(", ");
				}
				sqlBuilder.Append(paramName);
				sqlParams.Add(ZSqlParameter.New(paramName, code, EDIMessageSchema.EM_MessageSubType));
				++index;
			}
			sqlBuilder.AppendLine(")");
			sqlBuilder.Append("ORDER BY EM_SystemCreateTimeUtc");
			if (mostRecentFirst)
			{
				sqlBuilder.Append(" desc");
			}
			sqlBuilder.AppendLine();

			var queryResult = new DynamicBusinessObjectCollection(factory);
			queryResult.Load(sqlBuilder.ToString(), sqlParams);

			var result = new List<ZGuid>(queryResult.Count);
			Array.ForEach(queryResult.ToArray<DynamicBusinessObject>(), (DynamicBusinessObject dynamicBusinessObject) => { result.Add((ZGuid)dynamicBusinessObject[EDIMessageSchema.PK.Name]); });
			return result;
		}

		internal override List<ZGuid> GetEDIMessagePKs()
		{
			return LoadEDIMessagePKs(FactoryProvider.Current, messageActionTypeMap.Keys, 200, false);
		}

		internal override IMessageAction GetMessageAction(ZString messageType, ZString messageSubType)
		{
			return GetMessageAction(messageType, messageSubType, FactoryProvider);
		}

		protected virtual IMessageAction GetMessageAction(ZString messageType, ZString messageSubType, BusinessObjectFactoryProvider factoryProvider)
		{
			IMessageAction result = null;
			if (messageType == SystemMessage.MessageType)
			{
				Tuple<string, Type> actionType;
				if (messageActionTypeMap.TryGetValue(messageSubType, out actionType))
				{
					Type type = (actionType.Item1 != null)
						? ObjectFactory.GetType(actionType.Item1)
						: actionType.Item2;
					if (type != null)
					{
						result = (IMessageAction)Activator.CreateInstance(type, FactoryProvider);
					}
				}
			}
			return result;
		}
	}

	public class SystemXmlMessageProcessor : SystemXmlMessageProcessorBase
	{
		public SystemXmlMessageProcessor()
		{
			AddSupportedMessage(SystemMessageList.Codes.CustomerServiceResponse, "CustomerServiceResponseMessageAction");
			AddSupportedMessage(SystemMessageList.Codes.ReferenceDataUpdate, typeof(ReferenceDataUpdateMessageAction));
			AddSupportedMessage(SystemMessageList.Codes.TranslationFeedbackEntry, typeof(TranslationFeedbackLifecycleManager.TranslationFeedbackEntryMessageAction));
			AddSupportedMessage(SystemMessageList.Codes.TranslationFeedbackUpdate, typeof(TranslationFeedbackLifecycleManager.TranslationFeedbackUpdateMessageAction));
			AddSupportedMessage(SystemMessageList.Codes.LinkTrack, typeof(LinkTrackMessageAction));
			AddSupportedMessage(SystemMessageList.Codes.LicenceUsageRequest, typeof(LicenceUsageRequestMessageAction));
			AddSupportedMessage(SystemMessageList.Codes.StaffReportRequest, typeof(StaffReportRequestMessageAction));
			AddSupportedMessage(SystemMessageList.Codes.LogsRequest, typeof(LogsRequestMessageAction));
		}
	}
}
