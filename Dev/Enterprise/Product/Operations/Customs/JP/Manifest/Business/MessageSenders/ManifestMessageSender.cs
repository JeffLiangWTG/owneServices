using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Customs.JP.Common.JPMessageActionList;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public class ManifestMessageSender : Common.MessageSender
	{
		public ManifestMessageSender(IEnumerable<ManifestMessageSendingObject> sendingObjects, AsycudaManifestHeader header, ZString procedureCode)
			: base(header)
		{
			this.sendingObjects = sendingObjects;
			this.header = header;
			this.procedureCode = procedureCode;
			this.sendingObjectParent = sendingObjects.FirstOrDefault()?.Parent;
		}

		public ManifestMessageSender(MessageVisualObject visualObject, AsycudaManifestHeader header, ZString procedureCode) : base(header)
		{
			this.visualObject = visualObject;
			this.sendingObjects = (visualObject.ContentProvider as ManifestMessageContentProvider)?.SendingObjects ?? Array.Empty<ManifestMessageSendingObject>();
			this.sendingObjectParent = sendingObjects.FirstOrDefault()?.Parent;

			this.header = header;
			this.procedureCode = procedureCode;
		}

		readonly string procedureCode;
		readonly AsycudaManifestHeader header;
		readonly MessageVisualObject visualObject;
		readonly IEnumerable<ManifestMessageSendingObject> sendingObjects;
		readonly ManifestMessageSendingObjectParent sendingObjectParent;

		public EDIMessage ManualExportMessage()
		{
			var message = CreateEDIMessage();

			if (message != null)
			{
				message.EM_ApplicationReference = EDIMessage.FlatFile;
			}

			return message;
		}

		public void UpdateBillStatuses(bool isManualExport = false)
		{
			sendingObjects.Select(x => x.Bill).ForEach(b => b.ABL_MessageStatus = isManualExport ? JPMessageStatusList.Codes.Exported : JPMessageStatusList.Codes.Sending);

			if (!isManualExport)
			{
				switch (procedureCode)
				{
					case JPProcedureCodeList.Codes.NVC01:
						sendingObjects.ForEach(sendingObject =>
						{
							sendingObject.Bill.ABL_BillStatus = sendingObject.Action.ToString() switch
							{
								NVC01MessageActionList.Codes.Nine => JPCustomsStatusList.Codes.AWR,
								NVC01MessageActionList.Codes.Five => JPCustomsStatusList.Codes.AWA,
								NVC01MessageActionList.Codes.One => JPCustomsStatusList.Codes.AWD,
								_ => sendingObject.Bill.ABL_BillStatus
							};
						});
						break;
					case JPProcedureCodeList.Codes.HCH01:
						if (sendingObjectParent.EndSendMessage)
						{
							header.MasterBill.ABL_BillStatus = JPMasterBillStatusList.Codes.AWE;
						}
						sendingObjects.Select(x => x.Bill).ForEach(b => b.ABL_BillStatus = JPCustomsStatusList.Codes.AWR);
						break;
					case JPProcedureCodeList.Codes.HDE:
						header.MasterBill.ABL_BillStatus = JPMasterBillStatusList.Codes.AWE;
						break;
					case JPProcedureCodeList.Codes.HDF01:
						foreach (var sendingObject in sendingObjects)
						{
							switch (sendingObject.Action)
							{
								case "":
									sendingObject.Bill.ABL_BillStatus = JPCustomsStatusList.Codes.AWR;
									break;
								case HDF01MessageActionList.Codes.C:
									sendingObject.Bill.ABL_BillStatus = JPCustomsStatusList.Codes.AWC;
									break;
								case HDF01MessageActionList.Codes.D:
									sendingObject.Bill.ABL_BillStatus = JPCustomsStatusList.Codes.AWD;
									break;
							}
						}
						break;
				}
			}
		}

		ZBlob MessageData { get; set; }

		public bool SendMessageAndUpdateBillMessageStatus()
		{
			var result = SendMessage();
			if (result)
			{
				UpdateBillStatuses();
				header.AddEventWithReference(Events.InterchangeSent, MessageData);
			}

			return result;
		}

		void LogAuthorised(EDIMessage message)
		{
			if (visualObject?.HasChangedValues() ?? false)
			{
				header.AddEventWithReference(Events.Authorised, string.Format(IStmALogExtensions.Reference.SendingWithOverriddenValues, message.EM_MessageNum));
			}
		}

		protected override bool GenerateMessage()
		{
			var message = CreateEDIMessage();

			if (message != null)
			{
				EDIInterchange.CreateFromMessage(message);
				message.EM_FormattedMessageTextInfo.RefreshBinding();

				return true;
			}
			else
			{
				return false;
			}
		}

		EDIMessage CreateEDIMessage()
		{
			var message = header.Factory.New<EDIMessage>();
			message.ProcedureCode = procedureCode;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message.EM_Status = EDIMessageStatusList.Codes.Sent;
			message.EM_SendWithMessageErrors = header.HasMessageErrors;

			try
			{
				var sendingObjectsToSend = GetSendingObjectsToSend();
				message.EM_MessageData = visualObject?.BuildMessage() ?? ManifestNACCSMessageBuilder.BuildNACCSMessage(sendingObjectsToSend);
				header.Messages.Add(message);
				message.EM_LinkedObject = header;

				message.EM_MessageNumInfo.ValueChanged += (s, e) =>
				{
					var newHeader = JPMessageUtils.ConvertStringToMessage(JPMessageUtils.ConvertMessageToString(JPMessageUtils.ExtractHeader(message.EM_MessageData)).Replace(EDIMessage.MessageNumberPlaceHolder.PadRight(EDIMessage.MessageReferenceLength), message.EM_MessageNum.Right(EDIMessage.MessageReferenceLength)));
					Array.Copy(newHeader, message.EM_MessageData, newHeader.Length);
					MessageData = message.EM_MessageData;

					var interchange = message.Interchange;
					if (interchange != null)
					{
						interchange.EI_InterchangeNum = message.EM_MessageNum;
						interchange.EI_BodyData = message.EM_MessageData;
					}
					LogAuthorised(message);
				};
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				message.Delete();

				if (ex is JPMessageSchemaException)
				{
					var errors = new System.Collections.Specialized.StringCollection { ex.Message };
					MessageInitiator.MessageSendErrorAlert(errors);

					return null;
				}
				else
				{
					throw;
				}
			}

			return message;
		}

		IEnumerable<ManifestMessageSendingObject> GetSendingObjectsToSend()
		{
			if (header.IsHCH && header.MessageSendingContext.EndSendMessage)
			{
				var dummySendingObjectForHCH01End = sendingObjectParent.DummySendObjectProvider.DummySendingObjectForHCH01End;
				dummySendingObjectForHCH01End.ShouldSend = true;
				var sendingObjectsToSend = sendingObjects.ToList();
				sendingObjectsToSend.Add(dummySendingObjectForHCH01End);
				return sendingObjectsToSend;
			}
			return sendingObjects;
		}

		protected override bool ShouldNotifyUserOfASuccessfulSend => false;
	}
}
