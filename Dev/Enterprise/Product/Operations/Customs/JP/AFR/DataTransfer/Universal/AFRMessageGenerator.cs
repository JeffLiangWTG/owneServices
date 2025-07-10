using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public class AFRMessageGenerator
	{
		public AFRMessageGenerator(JPAFRHeader header, IDataObjectWriterStrategy writerStrategy, bool savingIsDoneExternally = false)
		{
			this.header = Argument.NotNull(header, "header");
			this.writerStrategy = writerStrategy;
			this.savingIsDoneExternally = savingIsDoneExternally;
		}
		readonly IDataObjectWriterStrategy writerStrategy;
		readonly bool savingIsDoneExternally;
		readonly JPAFRHeader header;

		BusinessObjectFactory Factory => header.Factory;

		public const string EnterpriseSenderID = "ENT";

		public delegate bool ShouldIncludeBillDelegate(ZGuid billPK, ref string functionType);

		#region Sending Methods

		#region Sending BLL Function Message

		public bool SendBLLMessageToCustoms(BLLFunction bllFunction)
		{
			var result = false;

			var actionPurpose = GetBLLFunctionActionPurpose(bllFunction.FunctionCode);
			var shipment = GenerateUniversalShipmentForBLLFunction(actionPurpose, bllFunction);
			if (shipment != null)
			{
				UpdateStatuses(actionPurpose, bllFunction);
				result = SendDataToCustoms(actionPurpose, shipment);
			}

			return result;
		}

		void UpdateStatuses(string actionPurpose, BLLFunction bllFunction)
		{
			var selectedbillPKs = bllFunction.SelectedBills.Cast<BLLFunctionBill>().Select(x => x.AFRBill.PK).ToArray();
			foreach (var bill in header.Bills)
			{
				if (bill.PK == bllFunction.MasterAFRBill.PK || selectedbillPKs.Contains(bill.PK))
				{
					switch (actionPurpose)
					{
						case MessagingTypeList.Codes.RegisterBLL:
							bill.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
							break;
						case MessagingTypeList.Codes.CancelBLL:
							bill.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLCancellation;
							break;
					}
				}
			}
		}

		UniversalShipment GenerateUniversalShipmentForBLLFunction(string actionPurpose, BLLFunction bllFunction)
		{
			var masterBill = bllFunction.MasterAFRBill;
			if (masterBill != null)
			{
				var dataContext = DataContextFactory.New();
				dataContext.AddDataTarget(DataContextType.AFRBill, bllFunction.JPM_BillOfLadingNumber);
				dataContext.SetWorkflowInfo(new WorkflowInfo
				{
					ActionPurpose = ListHelper.GetWithDescription<CodeDescriptionPair>(actionPurpose, Factory.GetCachedValue<MessagingTypeList>()),
					TriggerType = TriggerType.Manual
				});

				var shipment = new UniversalShipment(writerStrategy)
				{
					DataContext = dataContext,
					WayBillNumber = bllFunction.JPM_BillOfLadingNumber,
					WayBillType = new WayBillType
					{
						Code = WayBillTypeList.Codes.House,
						Description = WayBillTypeList.Descriptions.House
					}
				};
				shipment.SetAddInfoCollection(() => new List<AddInfo>
					{
						new AddInfo
						{
							Key = AddInfoConstants.Bill.BLLFunctionCode,
							Value = ((int)bllFunction.FunctionCode).ToString(CultureInfo.InvariantCulture)
						},
						new AddInfo
						{
							Key = AddInfoConstants.Bill.BLLChangeReasonCode,
							Value = bllFunction.JPM_ChangeReasonCode
						}
					});
				shipment.SetAddInfoGroupCollection(() => GetAddInfoGroup(bllFunction));

				return shipment;
			}
			return null;
		}

		static List<AddInfoGroup> GetAddInfoGroup(BLLFunction bllFunction)
		{
			var functionCode = bllFunction.FunctionCode;
			List<AddInfoGroup> result;
			switch (functionCode)
			{
				case BLLFunctionCode.RegisterSplit:
				case BLLFunctionCode.RegisterSwitch:
				case BLLFunctionCode.CancelSplit:
				case BLLFunctionCode.CancelSwitch:
					result = new List<AddInfoGroup>
						{
							new AddInfoGroup
							{
								Type = new CodeDescriptionPair
								{
									Code = AddInfoConstants.Bill.BLLOriginalBillNumbers,
									Description = AddInfoConstants.Bill.BLLOriginalBillNumbersDescription
								},
								AddInfoCollection = new List<AddInfo>
								{
									new AddInfo
									{
										Key = AddInfoConstants.Bill.BLLBillNumber,
										Value = bllFunction.JPM_BillOfLadingNumber
									}
								}
							},
							new AddInfoGroup
							{
								Type = new CodeDescriptionPair
								{
									Code = AddInfoConstants.Bill.BLLNewBillNumbers,
									Description = AddInfoConstants.Bill.BLLNewBillNumbersDescription
								},
								AddInfoCollection = bllFunction.SelectedBills.Cast<BLLFunctionBill>().Select((x,i) => new AddInfo
								{
									Key = AddInfoConstants.Bill.BLLBillNumber + "-" + (i + 1),
									Value = x.JPM_BillOfLadingNumber
								}).ToList()
							}
						};
					break;
				case BLLFunctionCode.CancelMerge:
				case BLLFunctionCode.RegisterMerge:
					result = new List<AddInfoGroup>
						{
							new AddInfoGroup
							{
								Type = new CodeDescriptionPair
								{
									Code = AddInfoConstants.Bill.BLLOriginalBillNumbers,
									Description = AddInfoConstants.Bill.BLLOriginalBillNumbersDescription
								},
								AddInfoCollection = bllFunction.SelectedBills.Cast<BLLFunctionBill>().Select((x,i) => new AddInfo
								{
									Key = AddInfoConstants.Bill.BLLBillNumber + "-" + (i + 1),
									Value = x.JPM_BillOfLadingNumber
								}).ToList()
							},
							new AddInfoGroup
							{
								Type = new CodeDescriptionPair
								{
									Code = AddInfoConstants.Bill.BLLNewBillNumbers,
									Description = AddInfoConstants.Bill.BLLNewBillNumbersDescription
								},
								AddInfoCollection = new List<AddInfo>
								{
									new AddInfo
									{
										Key = AddInfoConstants.Bill.BLLBillNumber,
										Value = bllFunction.JPM_BillOfLadingNumber
									}
								}
							}
						};
					break;
				default:
					result = new List<AddInfoGroup>();
					break;
			}
			return result;
		}

		static string GetBLLFunctionActionPurpose(BLLFunctionCode functionCode)
		{
			string result;
			switch (functionCode)
			{
				case BLLFunctionCode.RegisterSplit:
				case BLLFunctionCode.RegisterSwitch:
				case BLLFunctionCode.RegisterMerge:
					result = MessagingTypeList.Codes.RegisterBLL;
					break;
				case BLLFunctionCode.CancelSplit:
				case BLLFunctionCode.CancelSwitch:
				case BLLFunctionCode.CancelMerge:
					result = MessagingTypeList.Codes.CancelBLL;
					break;
				default:
					result = string.Empty;
					break;
			}
			return result;
		}

		#endregion

		public bool SendCompletionMessageToCustoms(ActionCode actionCode)
		{
			ZString headerAction = MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse;
			ZString headerActionDescription = MessagingTypeList.Descriptions.AdvanceCargoInformationRegistrationHouse;
			ZString billAction = FunctionTypeList.Codes.Registration;
			ZString billActionDescription = FunctionTypeList.Descriptions.Registration;
			if (actionCode == ActionCode.RegisterCompletionByAmendment)
			{
				headerAction = MessagingTypeList.Codes.UpdateAdvanceCargoInformationRegistrationHouse;
				headerActionDescription = MessagingTypeList.Descriptions.UpdateAdvanceCargoInformationRegistrationHouse;
				billAction = FunctionTypeList.Codes.Add;
				billActionDescription = FunctionTypeList.Descriptions.Add;
			}

			IShipmentDataContextManager manager = (IShipmentDataContextManager)header.GetUniversalDataContextManager();
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.AFR, header)));
			var headerData = (UniversalShipment)writer.GetDataObject(header);
			var dataContext = headerData.DataContext;
			dataContext.SetWorkflowInfo(new WorkflowInfo()
			{
				ActionPurpose = new CodeDescriptionPair() { Code = headerAction, Description = headerActionDescription },
				RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.AFR } },
				TriggerType = TriggerType.Manual
			});
			headerData.SetAddInfoCollection(() => headerData.AddInfoCollection.AddSafe(new AddInfo() { Key = AddInfoConstants.Header.InternalTransactionNumber, Value = AddInfoConstants.Header.MessageNumberPlaceHolder }));
			headerData.SubShipmentCollection.Clear();
			var billData = new UniversalShipment(writerStrategy);
			billData.DataContext = DataContextFactory.New();
			billData.DataContext.AddDataTarget(DataContextType.AFRBill, null);
			billData.DataContext.SetWorkflowInfo(new WorkflowInfo()
			{
				ActionPurpose = new CodeDescriptionPair() { Code = billAction, Description = billActionDescription },
				TriggerType = TriggerType.Manual
			});
			billData.SetAddInfoCollection(() => billData.AddInfoCollection.AddSafe(new AddInfo() { Key = AddInfoConstants.Bill.HouseBillRegisterCompletion, Value = AddInfoConstants.True }));
			headerData.SubShipmentCollection.Add(billData);
			header.JPH_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillRegistrationCompletion;
			return SendDataToCustoms(MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouseCompletion, headerData);
		}

		public bool SendDataToCustoms(string messageType, ShouldIncludeBillDelegate shouldIncludeBill)
		{
			IShipmentDataContextManager manager = (IShipmentDataContextManager)header.GetUniversalDataContextManager();
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.AFR, header)));
			var headerData = (UniversalShipment)writer.GetDataObject(header);
			return SendDataToCustomsCore(headerData, messageType, shouldIncludeBill);
		}

		public bool SendCMVToCustoms(string messageType, ShouldIncludeBillDelegate shouldIncludeBill)
		{
			var writer = new JPAFRHeaderDataObjectCMVWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.AFR, header)));
			var headerData = writer.GetDataObject(header);
			return SendDataToCustomsCore(headerData, messageType, shouldIncludeBill);
		}

		public bool SendDataToCustomsCore(UniversalShipment headerData, string messageType, ShouldIncludeBillDelegate shouldIncludeBill)
		{
			var dataContext = headerData.DataContext;
			dataContext.SetWorkflowInfo(new WorkflowInfo()
			{
				ActionPurpose = ListHelper.GetWithDescription<CodeDescriptionPair>(messageType, Factory.GetCachedValue<MessagingTypeList>()),
				RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.AFR } },
				TriggerType = TriggerType.Manual
			});
			headerData.SetAddInfoCollection(() => headerData.AddInfoCollection.AddSafe(new AddInfo() { Key = AddInfoConstants.Header.InternalTransactionNumber, Value = AddInfoConstants.Header.MessageNumberPlaceHolder }));
			UpdateStatuses(headerData, messageType, shouldIncludeBill);
			return SendDataToCustoms(messageType, headerData);
		}

		bool SendDataToCustoms(string messageType, UniversalShipment headerData)
		{
			var interchange = Factory.New<XmlEDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.XDC;
			interchange.EI_From = EnterpriseSenderID;
			interchange.EI_To = Enterprise.Customs.JP.AFR.Business.Constants.JapanCustomsReceipientID;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.eHubQueued;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			interchange.EI_GB = header.JPH_GB_Branch;
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				new XmlWriter().WriteXML(headerData, stream);
				using (var reader = new StreamReader(stream))
				{
					interchange.EI_BodyText = reader.ReadToEnd();
				}
			}
			var message = interchange.ContainedMessages.AddNew(typeof(JPAFRMessage));
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			message.EM_Status = EDIMessageStatusList.Codes.Sent;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message.EM_MessageText = interchange.EI_BodyText;
			message.EM_MessageOwner = messageType;
			message.EM_GB = header.JPH_GB_Branch;
			message.Saving += Message_Saving;
			message.Saved += Message_Saved;
			header.Messages.Add(message);
			if (HasConsol)
			{
				header.JPH_OverrideFreightDefaults = true;
			}
			var result = false;
			try
			{
				if (savingIsDoneExternally)
				{
					Factory.Saved += Factory_Saved;
				}
				else
				{
					SaveInternally();
					Factory.Save();
					message.Saving -= Message_Saving;
				}

				result = true;
			}
			catch (ZSaveException ex)
			{
				RestoreToOriginal(message, ex is ZSaveConcurrencyException);
				ZExceptionReporting.HandleSaveException(ex);
			}
			return result;
		}

		protected virtual void SaveInternally()
		{
		}

		void RestoreToOriginal(XmlEDIMessage message, bool isZSaveConcurrencyException)
		{
			if (!message.IsDeleted && !message.IsInDatabase)
			{
				header.Messages.RemoveAndDelete(message);
			}

			if (!isZSaveConcurrencyException)
			{
				RestoreIfNeeded(header.JPH_MessageStatusInfo, info => header.JPH_MessageStatus = (ZString)info.OriginalValue);
				foreach (var bill in header.Bills)
				{
					RestoreIfNeeded(bill.JPB_MessageStatusInfo, info => bill.JPB_MessageStatus = (ZString)info.OriginalValue);
				}
			}

			if (HasConsol)
			{
				RestoreIfNeeded(header.JPH_OverrideFreightDefaultsInfo, info => header.JPH_OverrideFreightDefaults = (ZBool)info.OriginalValue);
			}
		}

		ZBool HasConsol => header.Consol != null;

		static void RestoreIfNeeded(ZPropertyInfo propertyInfo, Action<ZPropertyInfo> restore)
		{
			if (propertyInfo.HasChanges)
			{
				restore?.Invoke(propertyInfo);
			}
		}

		public bool SendDepartureTimeRegistrationToCustoms(bool isCorrection)
		{
			ZString headerAction, headerActionDescription, billAction, billActionDescription;

			if (isCorrection)
			{
				headerAction = MessagingTypeList.Codes.DepartureTimeCorrection;
				headerActionDescription = MessagingTypeList.Descriptions.DepartureTimeCorrection;
				billAction = FunctionTypeList.Codes.Update;
				billActionDescription = FunctionTypeList.Descriptions.Update;
			}
			else
			{
				headerAction = MessagingTypeList.Codes.DepartureTimeRegistration;
				headerActionDescription = MessagingTypeList.Descriptions.DepartureTimeRegistration;
				billAction = FunctionTypeList.Codes.Registration;
				billActionDescription = FunctionTypeList.Descriptions.Registration;
			}

			IShipmentDataContextManager manager = (IShipmentDataContextManager)header.GetUniversalDataContextManager();
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.AFR, header)));
			var headerData = (UniversalShipment)writer.GetDataObject(header);
			var dataContext = headerData.DataContext;
			dataContext.SetWorkflowInfo(new WorkflowInfo()
			{
				ActionPurpose = new CodeDescriptionPair() { Code = headerAction, Description = headerActionDescription },
				RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.AFR } },
				TriggerType = TriggerType.Manual
			});
			headerData.SetAddInfoCollection(() => headerData.AddInfoCollection.AddSafe(new AddInfo() { Key = AddInfoConstants.Header.InternalTransactionNumber, Value = AddInfoConstants.Header.MessageNumberPlaceHolder }));

			if (headerData.SubShipmentCollection == null)
			{
				headerData.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			}
			if (headerData.SubShipmentCollection != null)
			{
				headerData.SubShipmentCollection.Clear();
				var billData = new UniversalShipment(writerStrategy);
				billData.DataContext = DataContextFactory.New();
				billData.DataContext.AddDataTarget(DataContextType.AFRBill, null);
				billData.DataContext.SetWorkflowInfo(new WorkflowInfo()
				{
					ActionPurpose = new CodeDescriptionPair() { Code = billAction, Description = billActionDescription },
					TriggerType = TriggerType.Manual
				});

				headerData.SubShipmentCollection.Add(billData);
			}
			header.JPH_MessageStatus = MessageStatusList.Codes.AwaitingDepartureTimeRegistration;
			return SendDataToCustoms(MessagingTypeList.Codes.DepartureTimeRegistration, headerData);
		}

		#endregion

		#region Message Modification Methods

		protected virtual void Message_Saving(EDIMessage message)
		{
			if (!message.EM_MessageNum.IsEmpty)
			{
				message.EM_MessageText = message.EM_MessageText.Replace(AddInfoConstants.Header.MessageNumberPlaceHolder, message.EM_MessageNum);
				var interchange = message.Interchange;
				if (interchange != null)
				{
					interchange.EI_BodyText = interchange.EI_BodyText.Replace(AddInfoConstants.Header.MessageNumberPlaceHolder, message.EM_MessageNum);
				}
			}
		}

		void Message_Saved(EDIMessage message, bool saveSucceeded)
		{
			if (saveSucceeded)
			{
				message.Saving -= Message_Saving;
				message.Saved -= Message_Saved;
			}
			else
			{
				if (!message.IsDeleted && !message.IsInDatabase)
				{
					var interchange = message.Interchange;
					if (!interchange?.IsDeleted ?? true)
					{
						interchange?.Delete();
					}
					message.Delete();
				}
			}
		}

		static void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				factory.Saved -= Factory_Saved;
			}
		}

		#endregion

		#region Status Related Methods

		void UpdateStatuses(UniversalShipment headerData, string messageType, ShouldIncludeBillDelegate shouldIncludeBill)
		{
			foreach (var bill in header.Bills)
			{
				var billData = GetBillData(headerData.SubShipmentCollection, bill.JPB_BillNumber);
				if (billData != null)
				{
					string functionType = null;
					if (shouldIncludeBill == null || shouldIncludeBill(bill.PK, ref functionType))
					{
						bill.JPB_MessageStatus = GetStatus(messageType, functionType);
						var dataContext = billData.DataContext;
						if (dataContext == null)
						{
							dataContext = DataContextFactory.New();
							dataContext.AddDataTarget(DataContextType.AFRBill, null);
						}
						if (functionType != null)
						{
							dataContext.SetWorkflowInfo(new WorkflowInfo()
							{
								ActionPurpose = ListHelper.GetWithDescription<CodeDescriptionPair>(functionType, Factory.GetCachedValue<FunctionTypeList>()),
								TriggerType = TriggerType.Manual
							});
						}
					}
					else
					{
						headerData.SubShipmentCollection.Remove(billData);
					}
				}
			}
		}

		static UniversalShipment GetBillData(DataObjectList<UniversalShipment> subShipmentCollection, ZString billNumber)
		{
			UniversalShipment result = null;
			if (subShipmentCollection != null)
			{
				result = subShipmentCollection.FirstOrDefault(x => x.WayBillNumber.GetValueOrDefault() == billNumber);
			}
			return result;
		}

		static ZString GetStatus(string messageType, string functionType)
		{
			var result = ZString.Empty;
			switch (messageType)
			{
				case MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse:
					result = MessageStatusList.Codes.AwaitingHouseBillRegistration;
					break;
				case MessagingTypeList.Codes.AdvanceCargoInformationRegistrationMaster:
					result = MessageStatusList.Codes.AwaitingMasterBillRegistration;
					break;
				case MessagingTypeList.Codes.DepartureTimeRegistration:
					result = MessageStatusList.Codes.AwaitingDepartureTimeRegistration;
					break;
				case MessagingTypeList.Codes.UpdateAdvanceCargoInformationRegistrationHouse:
					result = GetStatusForHouseUpdate(functionType);
					break;
				case MessagingTypeList.Codes.UpdateRegisteredAdvanceCargoInformationMaster:
					result = GetStatusForMasterUpdate(functionType);
					break;
				case MessagingTypeList.Codes.BlanketVesselChange:
					result = MessageStatusList.Codes.AwaitingBlanketVesselChangeCompletion;
					break;
			}
			return result;
		}

		static ZString GetStatusForHouseUpdate(string functionType)
		{
			var result = ZString.Empty;
			switch (functionType)
			{
				case FunctionTypeList.Codes.Add:
					result = MessageStatusList.Codes.AwaitingHouseBillAdd;
					break;
				case FunctionTypeList.Codes.Update:
					result = MessageStatusList.Codes.AwaitingHouseBillUpdate;
					break;
				case FunctionTypeList.Codes.Delete:
					result = MessageStatusList.Codes.AwaitingHouseBillDelete;
					break;
				case FunctionTypeList.Codes.Registration:
					result = MessageStatusList.Codes.AwaitingHouseBillRegistration;
					break;
			}
			return result;
		}

		static ZString GetStatusForMasterUpdate(string functionType)
		{
			var result = ZString.Empty;
			switch (functionType)
			{
				case FunctionTypeList.Codes.Add:
					result = MessageStatusList.Codes.AwaitingMasterBillAddAfterATD;
					break;
				case FunctionTypeList.Codes.Update:
					result = MessageStatusList.Codes.AwaitingMasterBillUpdate;
					break;
				case FunctionTypeList.Codes.Delete:
					result = MessageStatusList.Codes.AwaitingMasterBillDelete;
					break;
				case FunctionTypeList.Codes.Registration:
					result = MessageStatusList.Codes.AwaitingMasterBillRegistration;
					break;
			}
			return result;
		}

		#endregion
	}
}
