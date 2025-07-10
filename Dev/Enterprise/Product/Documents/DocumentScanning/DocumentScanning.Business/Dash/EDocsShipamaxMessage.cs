using System;
using System.Data;
using System.Text.Json;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Dash.Integration;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.DocumentScanning.Business
{
	public sealed class EDocsShipamaxMessage : EDIMessage, ICanBeSavedByDocumentFactory, IParseRequest
	{
		public EDocsShipamaxMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		DocumentFactory MasterFactory
		{
			get
			{
				if (masterFactory == null)
				{
					if (Factory is NumberedBusinessObjectFactory numberedBusinessObjectFactory)
					{
						masterFactory = numberedBusinessObjectFactory.MasterFactory;
					}
					else
					{
						masterFactory = new DocumentFactoryProvider().GetFactory(Factory);
					}
				}

				return masterFactory;
			}
		}

		DocumentFactory masterFactory;

		public StorageDocsBase LinkedEDoc
		{
			get
			{
				if (linkedEDoc == null)
				{
					var referencedStorageMain = MasterFactory.Load<StorageMain>(new ZGuid(EM_ApplicationReference));

					if (referencedStorageMain == null)
					{
						return null;
					}

					linkedEDoc = MasterFactory.GetFactory(referencedStorageMain.SM_DB).Load<StorageDocsBase>(EM_LinkUniqueID);
				}

				return linkedEDoc;
			}
		}

		StorageDocsBase linkedEDoc;

		public new BusinessObject EM_LinkedObject
		{
			get => LinkedEDoc;
		}

		public EDocsShipamaxMessageData MessageData
		{
			get
			{
				if (messageData == null)
				{
					if (EM_MessageData.IsEmpty)
					{
						messageData = new EDocsShipamaxMessageData(null, null);
					}
					else
					{
						var jsonText = MessageEncoding.UTF8WithoutBOM.GetString(EM_MessageData);
						messageData = JsonSerializer.Deserialize<EDocsShipamaxMessageData>(jsonText);
					}
				}

				return messageData;
			}
			set
			{
				messageData = value ?? new EDocsShipamaxMessageData(null, null);
				EM_MessageData = messageData.ParseResultXml == null && messageData.ParseDataJson == null
					? ZBlob.Empty
					: new ZBlob(MessageEncoding.UTF8WithoutBOM.GetBytes(JsonSerializer.Serialize(messageData)));
			}
		}

		public override ZBlob EM_MessageData
		{
			get
			{
				return base.EM_MessageData;
			}
			set
			{
				base.EM_MessageData = value;
				messageData = null;
			}
		}

		EDocsShipamaxMessageData messageData;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = EDIInterchange.ApplicationCodes.ShipamaxIntegration;
			EM_ReceiveTransmit = ReceiveTransmitList.Codes.Internal;
		}

		#region IParseRequest

		public string StatusName
		{
			get
			{
				var status = ZString.Empty;

				switch (StatusCode)
				{
					case EDIMessageStatusList.Codes.Queued:
						status = ResString.GetMultilingualString("0F508F28-79ED-418E-A1A3-3F16318FB76F", "Unparsed");
						break;

					case EDIMessageStatusList.Codes.Sent:
						status = ResString.GetMultilingualString("C2D49EE3-1A94-4DC9-8A99-012C51EACE1B", "Processing");
						break;

					case EDIMessageStatusList.Codes.Failed:
						status = ResString.GetMultilingualString("CFB41076-8FB7-4BA2-85ED-FB56B14FF25D", "Failed");
						break;

					case EDIMessageStatusList.Codes.ProcessedOK:
						status = ResString.GetMultilingualString("4D3972CC-BD83-444A-9D55-90B7CE13EBA1", "Complete");
						break;

					case EDIMessageStatusList.Codes.PreProcessedOK:
						status = ResString.GetMultilingualString("316DB966-FAA5-4CA3-9D59-E85DCF4F1704", "Need Review");
						break;

					case EDIMessageStatusList.Codes.Discarded:
						status = ResString.GetMultilingualString("4AA76978-396F-455E-A273-EAFCBF745015", "Discarded");
						break;

					case EDIMessageStatusList.Codes.Error:
						status = ResString.GetMultilingualString("B9FD86EB-5EB4-4A87-8FE1-9008E336B1BD", "Error");
						break;

					case EDIMessageStatusList.Codes.Cancelled:
						status = ResString.GetMultilingualString("F84905EF-9F8E-4098-AF25-7BA5512B5113", "User Canceled");
						break;

					case EDIMessageStatusList.Codes.Withdrawn:
						status = ResString.GetMultilingualString("6195FBAB-3829-4484-BFC6-6CB9B83A30B6", "Disabled");
						break;
				}

				return status;
			}
		}

		public string StatusCode => EM_Status;

		public bool IsRequestCancelled => EM_Status == EDIMessageStatusList.Codes.Cancelled;

		public bool IsRequestCompleted => EM_Status == EDIMessageStatusList.Codes.ProcessedOK || EM_Status == EDIMessageStatusList.Codes.PreProcessedOK;

		public bool IsRequestInProgress => EM_Status == EDIMessageStatusList.Codes.Sent;

		public bool IsRequestError => EM_Status == EDIMessageStatusList.Codes.Error || EM_Status == EDIMessageStatusList.Codes.Failed;

		public void UpdateStatus(string status)
		{
			if (!IsValidStatus(status))
			{
				throw new InvalidOperationException($"Status {status} is not supported");
			}

			EM_Status = status;
		}

		bool IsValidStatus(string status)
		{
			return status == EDIMessageStatusList.Codes.Queued
				|| status == EDIMessageStatusList.Codes.ProcessedOK
				|| status == EDIMessageStatusList.Codes.PreProcessedOK
				|| status == EDIMessageStatusList.Codes.Cancelled
				|| status == EDIMessageStatusList.Codes.Error
				|| status == EDIMessageStatusList.Codes.Failed
				|| status == EDIMessageStatusList.Codes.Withdrawn
				|| status == EDIMessageStatusList.Codes.Sent
				|| status == EDIMessageStatusList.Codes.Warning;
		}

		#endregion
	}
}
