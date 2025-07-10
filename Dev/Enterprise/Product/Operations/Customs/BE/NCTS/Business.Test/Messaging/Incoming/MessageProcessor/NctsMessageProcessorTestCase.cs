using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.Business.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	abstract class NctsMessageProcessorTestCase<TProcessor, TProvider> : MessageProcessorTestCase<TProcessor, TProvider>
		where TProcessor : BaseMessageProcessor<TProvider>
		where TProvider : class, IInboundProvider
	{
		const string LRN = "22045281480600000001";
		const string MRN = "22BE000000000012J1";
		const string WrongNumber = "1234567890";

		public void TestMessageTypesToInclude() => AssertSequencesEqual(new[] { MessageTypeToInclude }, Processor.MessageTypesToInclude);

		public void TestValidate_Failed()
		{
			MockProvider.Setup(x => x.LRN).Returns(SupportsSearchByLRN ? WrongNumber : LRN);
			MockProvider.Setup(x => x.MRN).Returns(SupportsSearchByMRN ? WrongNumber : MRN);
			if (SupportsSearchByEDIInterchange)
			{
				SetupEDIInterchangeWithLink(true);
			}

			Processor.PreProcessMessage(incomingMessage);

			AssertEquals($"EDIMessage Status Should be '{EDIMessageStatusList.Codes.Failed}'", EDIMessageStatusList.Codes.Failed, incomingMessage.EM_Status);
		}

		protected void AssertLockEventCreated(string initialCustomsStatus, string customsStatus, string messageStatus, string declarationType, string eventReference, string reference)
		{
			AssertLockUnlockEventCreated(initialCustomsStatus, customsStatus, messageStatus, declarationType, eventReference, reference, AutoEvents.LockForEditCode);
		}

		protected void AssertUnlockEventCreated(string initialCustomsStatus, string customsStatus, string messageStatus, string declarationType, string eventReference, string reference)
		{
			AssertLockUnlockEventCreated(initialCustomsStatus, customsStatus, messageStatus, declarationType, eventReference, reference, AutoEvents.UnlockForEditCode);
		}

		void AssertLockUnlockEventCreated(string initialCustomsStatus, string customsStatus, string messageStatus, string declarationType, string eventReference, string reference, string eventCode)
		{
			var header = CreateNctsHeader("", initialCustomsStatus, messageStatus: messageStatus);
			if (!string.IsNullOrEmpty(customsStatus))
			{
				Factory.Save();
				header.MovementHeader.BM_CustomsStatus = customsStatus;
				Factory.Save();
			}
			var declarationConfig = SetDeclarationConfig(declarationType, eventReference);
			if (MovementType == NctsMovementType.Codes.Departure)
			{
				incomingMessage.EM_LinkedObject = header.MovementHeader;
			}
			else
			{
				incomingMessage.EM_LinkedObject = header;
			}
			incomingMessage.EM_Status = "PPS";

			using (CustomsDataRegistry.Instance.DeclarationLockForEdit.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DeclarationLockConfigCollection() { declarationConfig }))
			{
				Processor.ProcessMessage(incomingMessage);
				Factory.Save();
			}

			var lckEventlog = header.Logs.Find(c => c.SL_SE_NKEvent == eventCode).First();
			AssertEquals($"EventReference for {eventCode}", reference, lckEventlog.SL_Reference);
		}

		DeclarationLockConfig SetDeclarationConfig(string declarationType, string eventReference)
		{
			var declarationConfig = new DeclarationLockConfig() { DeclarationType = declarationType, };
			var tabInfo = declarationConfig.TabInfos.AddNew();
			tabInfo.TabPage = tabInfo.Lookups.TabPageList.GetAllCodes().First();
			var eventInfo = declarationConfig.EventInfos.AddNew();
			eventInfo.EventReference = eventReference;
			eventInfo.EventType = Events.CustomsEntryStatusCode;
			eventInfo.EventSource = Core.Constants.Customs.EventLockSourceTypes.Codes.NctsHeader;
			return declarationConfig;
		}

		public void TestValidate_Discarded()
		{
			if (MessageCannotBeDiscarded)
			{
				AssertEquals(true, MessageCannotBeDiscarded);
			}
			else
			{
				AssertValidateMessageDeclaration(LRN, MRN, PreProcessNOKCustomsStatus, PreProcessNOKPhase, PreProcessNOKMessageStatus, EDIMessageStatusList.Codes.Discarded);
			}
		}

		public void TestValidate_PreProcessedOK() => AssertValidateMessageDeclaration(LRN, MRN, PreProcessedOKCustomsStatus, PreProcessedOKPhase, PreProcessOKMessageStatus, EDIMessageStatusList.Codes.PreProcessedOK);

		public void TestPendingTransactionlinkedtoTheConfirmedDepartureMessageAreConfirmed()
		{
			if (MessageConfirmsGuaranteeTransactions)
			{
				CombineAssertions(() =>
				{
					var permitHelper = new PermitTestDataHelper(Factory);
					var eoriCode1 = "123456789000";
					var org1 = Factory.NewWithValidTestData<OrgHeader>();
					org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					var cusGuaranteeHeaderList = new List<CusGuaranteeHeader>();
					var guaranteeHeader1 = GenerateGuaranteeHeader(org1, "19860101", "2204528148060XXXXXX", "111", 100m);
					Factory.Save();

					var query = permitHelper.GetPermitLineTransactionQuery("2204528148060XXXXXX", "CMT-CON", "111", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA);
					Factory.Save();
					var transactionRequested = Factory.Load<BaseCusPermitLineTransaction>(query);
					AssertEquals("Status of the first TRA transaction should be CONF (Confirmed)", PermitTransactionStatusList.Codes.Confirmed, transactionRequested[0].CPL_TransactionStatus);

					query = permitHelper.GetPermitLineTransactionQuery("2204528148060XXXXXX", "CMT-TO-CONF", "111", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL);
					transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);
					AssertEquals("Status of the first OBL transaction should be PND (Pending)", PermitTransactionStatusList.Codes.Pending, transactionRequested[0].CPL_TransactionStatus);
					AssertEquals("Only 1 OBL transaction should be found", 1, transactionRequested.Length);

					cusGuaranteeHeaderList.Add(guaranteeHeader1);

					var header = Factory.New<NctsHeader>();
					header.SetMovementType(NctsMovementType.Codes.Departure);

					header.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.Unknown;
					header.EffectiveMessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationSent;

					Factory.Save();

					SetupAndProcessMessage(cusGuaranteeHeaderList, org1, header);

					AssertEquals("The NCTS Header should have LRN 2204528148060XXXXXX", "2204528148060XXXXXX", header.MovementHeader.BM_PaperlessInbondNum);
					AssertEquals($"There should be {ExpectedMessagesAfterProcessing} message(s) on the movement header", ExpectedMessagesAfterProcessing, header.MovementHeader.Messages.Count);

					query = permitHelper.GetPermitLineTransactionQuery("2204528148060XXXXXX", "CMT-CON", "111", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA);
					Factory.Save();
					transactionRequested = Factory.Load<BaseCusPermitLineTransaction>(query);
					AssertEquals("After processing, the status of the first TRA transaction should be CONF (Confirmed)", PermitTransactionStatusList.Codes.Confirmed, transactionRequested[0].CPL_TransactionStatus);

					query = permitHelper.GetPermitLineTransactionQuery("2204528148060XXXXXX", "CMT-TO-CONF", "111", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL);
					transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);
					AssertEquals("After processing, the status of the first OBL transaction should be CONF (Confirmed)", PermitTransactionStatusList.Codes.Confirmed, transactionRequested[0].CPL_TransactionStatus);
					AssertEquals("After processing, Only 1 OBL transaction should be found", 1, transactionRequested.Length);

					var bond = header.MovementHeader.Guarantees.Cast<CusBondDetail>().FirstOrDefault(x => x.PW_BondNumber == "19860101");
					AssertEquals("The transaction should be referenced to the LRN", header.MovementHeader.BM_PaperlessInbondNum, transactionRequested[0].CPL_Reference);
					AssertEquals("The value on the transaction should be 100", 100m, transactionRequested[0].CPL_TranValue);
				});
			}
			else
			{
				AssertEquals(false, MessageConfirmsGuaranteeTransactions);
			}
		}

		protected virtual int ExpectedMessagesAfterProcessing => 1;

		void AssertValidateMessageDeclaration(string lrn, string mrn, ZString[] customsStatus, ZString[] phases, ZString[] messageStatusses, ZString expectedStatus)
		{
			NctsHeader SetupLRN(string lrnToUse, string status, string phase, string messageStatus)
			{
				MockProvider.Setup(x => x.LRN).Returns(lrnToUse);
				return CreateNctsHeader(lrnToUse, status, phase, messageStatus);
			}

			void SetupMRN(string mrnToUse, NctsHeader nctsHeaderToUse)
			{
				MockProvider.Setup(x => x.MRN).Returns(mrnToUse);
				Extensions.CreateMovementReferenceNumber(nctsHeaderToUse, mrnToUse);
			}

			foreach (var status in customsStatus)
			{
				foreach (var phase in phases)
				{
					foreach (var messageStatus in messageStatusses)
					{
						CombineAssertions($"BM_CustomsStatus: '{status}', BM_Phase: '{phase}', EffectiveMessageStatus: '{messageStatus}'", () =>
						{
							if (SupportsSearchByMRN)
							{
								SetupMRN(mrn, SetupLRN(WrongNumber, status, phase, messageStatus));
								Factory.Save();
								Processor.PreProcessMessage(incomingMessage);
								AssertEquals($"MRN: EDIMessage Status Should be '{expectedStatus}'", expectedStatus, incomingMessage.EM_Status);
							}
							if (SupportsSearchByLRN)
							{
								SetupMRN(WrongNumber, SetupLRN(lrn, status, phase, messageStatus));
								Factory.Save();
								Processor.PreProcessMessage(incomingMessage);
								AssertEquals($"LRN: EDIMessage Status Should be '{expectedStatus}'", expectedStatus, incomingMessage.EM_Status);
							}
							if (SupportsSearchByEDIInterchange)
							{
								SetupEDIInterchangeWithLink();
								Processor.PreProcessMessage(incomingMessage);
								AssertEquals($"EDI: EDIMessage Status Should be '{expectedStatus}'", expectedStatus, incomingMessage.EM_Status);
							}
						});
					}
				}
			}
		}

		protected virtual NctsHeader CreateNctsHeader(string lrn, string customsStatus, string phase = null, string messageStatus = null)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(MovementType);
			nctsHeader.EffectiveMessageStatus = messageStatus;
			var movementHeader = nctsHeader.IsDepartureMovement ? nctsHeader.MovementHeader : (NctsCommonMovementHeader)nctsHeader.ArrivalMovementHeader;
			movementHeader.BM_PaperlessInbondNum = lrn;
			movementHeader.BM_CustomsStatus = customsStatus;
			movementHeader.BM_Phase = phase;
			return nctsHeader;
		}

		protected abstract string MovementType { get; }

		protected abstract bool SupportsSearchByLRN { get; }

		protected abstract bool SupportsSearchByMRN { get; }

		protected virtual bool SupportsSearchByEDIInterchange { get; }

		protected virtual bool MessageCannotBeDiscarded => true;

		protected abstract Mock<TProvider> MockProvider { get; }

		protected abstract Mock<TProcessor> MockProcessor { get; }

		protected abstract ZString MessageTypeToInclude { get; }

		protected abstract ZString[] PreProcessedOKCustomsStatus { get; }

		protected virtual ZString[] PreProcessedOKPhase => new ZString[] { ZString.Empty };

		protected virtual ZString[] PreProcessOKMessageStatus => new ZString[] { ZString.Empty };

		protected virtual ZString[] PreProcessNOKCustomsStatus => new ZString[] { NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed };

		protected virtual ZString[] PreProcessNOKPhase => new ZString[] { ZString.Empty };

		protected virtual ZString[] PreProcessNOKMessageStatus => new ZString[] { ZString.Empty };

		protected virtual ZBool MessageConfirmsGuaranteeTransactions => ZBool.False;

		protected override TProcessor Processor => MockProcessor.Object;

		protected virtual NctsHeader SetupAndProcessMessage(List<CusGuaranteeHeader> guaranteeHeaderList, OrgHeader org, NctsHeader nctsHeader)
		{
			MockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");
			MockProvider.Setup(x => x.LRN).Returns("2204528148060XXXXXX");

			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_PaperlessInbondNum = "2204528148060XXXXXX";
			movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
			movementHeader.BM_AdditionalDeclarationType = "D";

			nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
			nctsHeader.Declarant.E2_OA_Address = org.MainAddress.PK;
			foreach (var guaranteeHeader in guaranteeHeaderList)
			{
				var guarantee1 = nctsHeader.MovementHeader.Guarantees.AddNew();
				guarantee1.PW_BondAmount = 150m;
				guarantee1.PW_BondNumber = guaranteeHeader.CPH_Number;
				nctsHeader.ApportionedAmountToGuaranteesLiabilityAmount();
			}

			Factory.Save();
			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			movementHeader.Messages.Add(incomingMessage);

			return nctsHeader;
		}

		protected void SetupEDIInterchangeWithLink(bool setupForFailure = false)
		{
			var sessionGUID = ZGuid.BrettsGuid;
			var originalOutgoingEDIMessage = Factory.New<NCTSMessage>();
			originalOutgoingEDIMessage.EM_Status = "SNT";

			if (!setupForFailure)
			{
				var header = CreateNctsHeader(LRN, "XXX");
				if (header.IsDepartureMovement)
				{
					originalOutgoingEDIMessage.EM_LinkTable = header.MovementHeader.TableName;
					originalOutgoingEDIMessage.EM_LinkUniqueID = header.MovementHeader.PK;
				}
				else
				{
					originalOutgoingEDIMessage.EM_LinkTable = header.TableName;
					originalOutgoingEDIMessage.EM_LinkUniqueID = header.PK;
				}
			}

			var outgoingInterchange = Factory.New<BECInterchange>();
			outgoingInterchange.EI_Status = "SNT";
			outgoingInterchange.EI_SessionGUID = sessionGUID;
			outgoingInterchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			originalOutgoingEDIMessage.EM_EI = outgoingInterchange.PK;

			var interchangeIncoming = Factory.New<BECInterchange>();
			interchangeIncoming.EI_SessionGUID = sessionGUID;
			interchangeIncoming.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;

			incomingMessage.EM_EI = interchangeIncoming.PK;
		}

		CusGuaranteeHeader GenerateGuaranteeHeader(OrgHeader org1, ZString pW_BondNumber, ZString transactionReference, ZString transactionAppId, ZDecimal valueTransaction)
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = pW_BondNumber;
			guaranteeHeader.CPH_OH_PermitHolder = org1.PK;
			guaranteeHeader.CPH_StartDate = new ZDate(2020, 7, 15);
			guaranteeHeader.CPH_SystemCreateTimeUtc = new ZDate(2020, 7, 15);
			guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader.AddTransaction(transactionReference, "CMT-TO-CONF", transactionAppId, "", valueTransaction, 0, PermitTransactionStatusList.Codes.Pending, transactionType: PermitTransactionTypeList.Codes.OBL, isAggregated: true);
			guaranteeHeader.AddTransaction(transactionReference, "CMT-CON", transactionAppId, "", -10, 0, PermitTransactionStatusList.Codes.Confirmed, isAggregated: true);
			AssertEquals(valueTransaction, guaranteeHeader.CPH_Calc_OpeningBalance);
			return guaranteeHeader;
		}

		protected new NCTSMessage CreateIncomingMessage(BusinessObjectFactory factory)
		{
			NCTSMessage bEMessage = factory.New<NCTSMessage>();
			bEMessage.EM_ReceiveTransmit = "RCV";
			return bEMessage;
		}

		protected override void SetUp()
		{
			base.SetUp();

			MockProvider.CallBase = true;
			MockProcessor.CallBase = true;
			MockProcessor.Setup(m => m.GetMessageDataProvider(It.IsAny<BEMessage>())).Returns(MockProvider.Object);
			incomingMessage = CreateIncomingMessage(Factory);
			Factory.Save();
		}
		protected NCTSMessage incomingMessage;
	}
}
