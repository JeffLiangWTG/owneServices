using System;
using System.Data;
using System.Text.Json;
using System.Xml.Schema;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using BaseEDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.IE.Business
{
	public abstract class InboundEDIMessage : EDIMessage
	{
		protected InboundEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		public static BaseEDIMessage GetOriginalMessage(BusinessObjectFactory factory, ZString applicationCode, ZString transactionId)
		{
			BaseEDIMessage originalMessage = null;
			if (!transactionId.IsEmpty)
			{
				var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, applicationCode)
					.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit)
					.AddToFilter(EDIMessageSchema.EM_ApplicationReference, transactionId)
					.AddToFilter(EDIMessageSchema.EM_Status, new[] { EDIMessageStatusList.Codes.Sent, EDIMessageStatusList.Codes.Acknowledged });
				query.OrderBy = EDIMessageSchema.Constants.EM_SystemCreateTimeUtc + OrderByClause.Descending;
				originalMessage = factory.LoadTop1<BaseEDIMessage>(query);
			}
			return originalMessage;
		}

		public static BaseEDIMessage GetOriginalMessageWithoutTID(BusinessObjectFactory factory, ZString applicationCode, ZGuid linkUniqueId, ZDateTime systemCreateTimeUtc)
		{
			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, applicationCode)
				.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit)
				.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, linkUniqueId)
				.AddToFilter(EDIMessageSchema.EM_Status, new[] { EDIMessageStatusList.Codes.Sent, EDIMessageStatusList.Codes.Acknowledged })
				.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, systemCreateTimeUtc);
			query.OrderBy = EDIMessageSchema.Constants.EM_SystemCreateTimeUtc + OrderByClause.Descending;
			return factory.LoadTop1<BaseEDIMessage>(query);
		}

		public TDataProvider GetDataProvider<TDataProvider>(Type xmlObjectType, Action exceptionHandling = null, bool withSchemaValidations = false)
		{
			TDataProvider result = default;
			try
			{
				result = GetDataProviderCore<TDataProvider>(xmlObjectType, withSchemaValidations);
			}
			catch (Exception ex) when (ex.InnerException is XmlSchemaValidationException || ex.InnerException is InvalidOperationException || ex is JsonException)
			{
				exceptionHandling?.Invoke();
			}
			return result;
		}

		protected virtual TDataProvider GetDataProviderCore<TDataProvider>(Type xmlObjectType, bool withSchemaValidations = false)
		{
			TDataProvider result = default;
			var mailBoxItemProvider = GetMailBoxItemProvider(xmlObjectType, withSchemaValidations);
			var xmlObject = mailBoxItemProvider?.GetType().GetProperty(nameof(MailBoxItemProvider<object>.Message))?.GetValue(mailBoxItemProvider);
			if (xmlObject != null)
			{
				result = (TDataProvider)Activator.CreateInstance(DecideDataProviderType<TDataProvider>(xmlObjectType), xmlObject);
			}
			return result;
		}

		protected virtual Type DecideDataProviderType<TDataProvider>(Type xmlObjectType)
		{
			return typeof(TDataProvider);
		}

		protected override bool ResetToQueuedStatusPreservesMessageType => true;
		protected override bool ResetToQueuedStatusPreservesMessageSubType => true;

		MailBoxItemProvider GetMailBoxItemProvider(Type xmlObjectType, bool withSchemaValidations)
		{
			MailBoxItemProvider result = null;
			var mailBoxItemType = typeof(MailBoxItemProvider<>).MakeGenericType(xmlObjectType);
			using (var reader = GetEM_MessageTextReader())
			{
				result = (MailBoxItemProvider)Activator.CreateInstance(mailBoxItemType, new object[] { reader, withSchemaValidations });
			}
			return result;
		}

		public override ZString EM_MessageInterpretation
		{
			get => MessageInterpretationNoteManager.Value;
			set
			{
				var oldValue = EM_MessageInterpretation;
				base.EM_MessageInterpretation = value;
				EM_MessageInterpretationInfo.RefreshBinding(oldValue);
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		}
	}
}
