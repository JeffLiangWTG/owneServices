using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.GB.Business.NCTS;
using Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.NCTS.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using NctsHeader = Enterprise.Customs.GB.Business.NctsHeader;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5.Testing
{
	public abstract class NctsBaseProcessorTest<T> : TestCaseWithFactory
	{
		public void TestProcessMessage()
		{
			var jobNum = 1001;
			foreach (var testCase in ProcessorTestCases)
			{
				jobNum++;

				using (testCase.SetupRegistry?.Invoke())
				{
					var transitStatusForTest = testCase.InitialTransitStatus;
					var header = responseHelper.CreateDefaultHeaderForTest(Factory, NctsMovementType.Codes.DepartureAndArrival, transitStatusForTest, jobNo: "BH_" + jobNum);
					header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					header.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
					header.MovementHeader.BM_PaperlessInbondNum = testCase.LRN ?? "TRATESTGB12308021209";
					header.MovementHeader.BM_Phase = testCase.InitialPhase;

					var mrn = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
					mrn.CE_EntryNum = testCase.MRN ?? "BH_" + jobNum;
					testCase.SetUpHeader?.Invoke(header);
					testCase.SetupGuaranteesAndTransactions?.Invoke(header);

					if (testCase.OutgoingMessageSubType.IsEmpty)
					{
						testCase.OutgoingMessageSubType = testCase.ExpectedMovementType == NctsMovementType.Codes.Arrival ? "007" : "015";
					}
					var outgoingMessage = responseHelper.CreateDefaultOutgoingMessage(Factory, header.BH_JobReference, testCase.IncomingMessageText, ZDateTime.UtcNow, testCase.OutgoingMessageSubType, "REMOVEME_" + jobNum);
					var incomingMessage = responseHelper.CreateDefaultIncomingMessage(Factory, header, testCase.IncomingMessageText, ZDateTime.UtcNow, testCase.MessageSubType, "REMOVEME_" + jobNum);
					responseHelper.SetupMessagesForTest(Factory, outgoingMessage, incomingMessage, header);
					incomingMessage.EM_LinkedObject = null;
					Factory.Save();

					var serviceLogger = new TestServiceLogger();
					var processor = new NctsResponseMessageProcessorPhase5(serviceLogger);
					processor.ExecuteBatch();
					Factory.Save();

					var factory = new BusinessObjectFactory();
					var reloadedHeader = factory.Load<NctsHeader>(header.PK);
					AssertEquals(2, reloadedHeader.LinkedMessages.Count);

					var lastIncomingMessage = reloadedHeader.LinkedMessages.LastIncomingMessage;
					AssertEquals(typeof(T).Name.Substring(2, 3), lastIncomingMessage.EM_MessageType);
					AssertEquals("Header status", testCase.ExpectedNewMessageStatus, reloadedHeader.EffectiveMessageStatus);
					AssertEquals("Message status", EDIMessageStatusList.Codes.ProcessedOK, lastIncomingMessage.EM_Status);
					if (reloadedHeader.IsDepartureMovement)
					{
						AssertEquals("Declaration status", testCase.ExpectedNewDeclarationStatus, reloadedHeader.MovementHeader.BM_CustomsStatus);
					}
					if (reloadedHeader.IsArrivalMovement)
					{
						AssertEquals("Arrival status", testCase.ExpectedNewArrivalStatus, reloadedHeader.ArrivalMovementHeader.BM_CustomsStatus);
					}

					if (testCase.ExpectedPhase != null)
					{
						if (reloadedHeader.IsDepartureMovement)
						{
							AssertEquals("Phase", testCase.ExpectedPhase, reloadedHeader.MovementHeader.BM_Phase);
						}
						else
						{
							AssertEquals("Phase", testCase.ExpectedPhase, reloadedHeader.ArrivalMovementHeader.BM_Phase);
						}
					}

					AssertXMLEquals("Message interpretation", testCase.ExpectedNewMessageInterpretation.Replace("\r\n", ""), lastIncomingMessage.EM_MessageInterpretation.Replace("\r\n", ""));

					testCase.HeaderAssertion?.Invoke(reloadedHeader);
					testCase.MessageAssertion?.Invoke(lastIncomingMessage);
					testCase.GuaranteesAndTransactionsAssertion?.Invoke(reloadedHeader);

					outgoingMessage = reloadedHeader.GetOutgoingMessage(lastIncomingMessage);
					AssertEquals(outgoingMessage.EM_Status, EDIMessage.Status.Acknowledged);

					mrn = CusEntryNumber.LoadOrCreate(reloadedHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
					mrn.CE_EntryNum = "Deleted";
					if (reloadedHeader.IsDepartureMovement)
					{
						reloadedHeader.MovementHeader.BM_PaperlessInbondNum = ZString.Empty;
					}
					factory.Save();
				}
			}
		}

		public static void CreateZZRefTestValuesIfNeeded(ZString officeCode, ZString dataGroupingCode, ZString[] officePurpose, ZString desc)
		{
			var factory = new BusinessObjectFactory();
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(factory);

			var eunzzz = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(dataGroupingCode, parent: eunzzz);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office Code");
			helper.CreateNewOrGetExistingCusCodeList(officeCode, dataGroupingCode, desc , officePurpose);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, dataGroupingCode);
			factory.Save();
		}

		public void TestProcessMessage_ExcludeBH_IsActiveFalse()
		{
			foreach (var testCase in ProcessorTestCases)
			{
				if (!string.IsNullOrEmpty(testCase?.MRN) || !string.IsNullOrEmpty(testCase?.LRN))
				{
					var transitStatus = testCase.InitialTransitStatus ?? string.Empty;
					var messageStatus = testCase.ExpectedNewMessageStatus;
					var expectedMovementType = testCase.ExpectedMovementType;

					var departureHeader = responseHelper.SetupPhase5MessagesForTest(Factory, NctsMovementType.Codes.Departure, string.Empty, transitStatus, messageStatus, jobNo: "NCT0001D");
					departureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					departureHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Acknowledged;
					departureHeader.MovementHeader.BM_Phase = testCase?.InitialPhase;
					departureHeader.BH_IsActive = true;

					if (departureHeader?.MovementHeader != null)
					{
						if (!string.IsNullOrEmpty(testCase?.MRN))
						{
							var mrn = CusEntryNumber.LoadOrCreate(departureHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
							mrn.CE_EntryNum = testCase.MRN;
						}

						if (!string.IsNullOrEmpty(testCase?.LRN))
						{
							var lrn = CusEntryNumber.LoadOrCreate(departureHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
							departureHeader.MovementHeader.BM_PaperlessInbondNum = testCase.LRN;
						}
					}

					var arrivalHeader = responseHelper.SetupPhase5MessagesForTest(Factory, NctsMovementType.Codes.Arrival, string.Empty, transitStatus, messageStatus, jobNo: "NCT0001A");
					arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					arrivalHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Acknowledged;
					arrivalHeader.ArrivalMovementHeader.BM_Phase = testCase?.InitialPhase;
					arrivalHeader.BH_IsActive = true;

					if (arrivalHeader?.ArrivalMovementHeader != null)
					{
						if (!string.IsNullOrEmpty(testCase?.MRN))
						{
							var mrn = CusEntryNumber.LoadOrCreate(arrivalHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
							mrn.CE_EntryNum = testCase.MRN;
						}

						if (!string.IsNullOrEmpty(testCase?.LRN))
						{
							var lrn = CusEntryNumber.LoadOrCreate(arrivalHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
							arrivalHeader.ArrivalMovementHeader.BM_PaperlessInbondNum = testCase.LRN;
						}
					}

					var incomingMessage = testCase.IncomingMessageText;
					var jobNumber = testCase.CorrelationIdentifier;
					if (!string.IsNullOrEmpty(jobNumber))
					{
						switch (expectedMovementType)
						{
							case NctsMovementType.Codes.Departure:
							case NctsMovementType.Codes.DepartureAndArrival:
								departureHeader.BH_HeaderType = expectedMovementType;
								departureHeader.BH_JobReference = jobNumber;
								break;
							case NctsMovementType.Codes.Arrival:
								arrivalHeader.BH_JobReference = jobNumber;
								break;
						}
						incomingMessage = incomingMessage.Replace($"<correlationIdentifier>{jobNumber}</correlationIdentifier>", $"<correlationIdentifier>{jobNumber}/123</correlationIdentifier>");
					}

					Factory.Save();

					var ediMessage = CreateTestInboundMessage();
					ediMessage.EM_MessageText = incomingMessage;
					var serviceLogger = new TestServiceLogger();
					var processor = new NctsResponseMessageProcessorPhase5(serviceLogger).GetApplicationTypeProcessorCore(ediMessage) as NctsBaseProcessor<T>;
					processor.ProcessMessage(ediMessage);

					var headerFromMessage = processor.NctsHeaderItem;
					AssertNotNull("NCTS header should not be null", headerFromMessage);

					departureHeader.BH_IsActive = false;
					arrivalHeader.BH_IsActive = false;
					Factory.Save();
					processor.ProcessMessage(ediMessage);
					headerFromMessage = processor.NctsHeaderItem;

					AssertNull("NCTS header should be null", headerFromMessage);
				}
				else
				{
					AssertNull("Test case MRN and LRN is null", testCase?.MRN ?? testCase?.LRN);
				}
			}
		}

		public void TestMessageShouldBeDiscarded()
		{
			var testCases = MessageShouldBeDiscardedTestCases ?? new MessageShouldBeDiscardedTestCase[] { new MessageShouldBeDiscardedTestCase() };
			var header = responseHelper.SetupPhase5MessagesForTest(Factory, NctsMovementType.Codes.DepartureAndArrival, string.Empty, LogicalStatusList.Codes.Sent);
			var ediMessage = CreateTestInboundMessage();

			var serviceLogger = new TestServiceLogger();
			var processor = new NctsResponseMessageProcessorPhase5(serviceLogger).GetApplicationTypeProcessorCore(ediMessage) as NctsBaseProcessor<T>;
			AssertNotNull("Could not get message processor for this type", processor);

			foreach (var testCase in testCases)
			{
				testCase.Setup?.Invoke(header);

				var shouldDiscard = processor.MessageShouldBeDiscarded(header, out var reasonText);
				AssertEquals(testCase.Description + ": ShouldBeDiscarded", testCase.ExpectedResult, shouldDiscard);
				if (shouldDiscard)
				{
					AssertContains(testCase.Description + ": Reason", testCase.ExpectedReasonText, reasonText);
				}
			}
		}

		public void TestAcceptMovementType()
		{
			var ediMessage = CreateTestInboundMessage();
			var serviceLogger = new TestServiceLogger();
			var testCase = ProcessorTestCases.OfType<MessageProcessorTestCase>().FirstOrDefault();
			var processor = new NctsResponseMessageProcessorPhase5(serviceLogger).GetApplicationTypeProcessorCore(ediMessage) as NctsBaseProcessor<T>;
			AssertEquals("Expected AcceptMovementType", testCase.ExpectedAcceptMovementType, processor.AcceptMovementType);
		}

		[ExpectNoExceptions]
		public void TestGetNctsHeaderFromMRN()
		{
			var testCase = ProcessorTestCases.OfType<MessageProcessorTestCase>().FirstOrDefault();
			if (!string.IsNullOrEmpty(testCase?.MRN))
			{
				var transitStatus = testCase.InitialTransitStatus ?? string.Empty;
				var messageStatus = testCase.ExpectedNewMessageStatus;
				var expectedMovementType = testCase.ExpectedMovementType;

				var departureHeader = responseHelper.SetupPhase5MessagesForTest(Factory, NctsMovementType.Codes.Departure, string.Empty, transitStatus, messageStatus, jobNo: "NCT0001D");
				departureHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Acknowledged;
				departureHeader.MovementHeader.BM_Phase = testCase?.InitialPhase;
				var mrn = CusEntryNumber.LoadOrCreate(departureHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
				mrn.CE_EntryNum = testCase.MRN;

				var arrivalHeader = responseHelper.SetupPhase5MessagesForTest(Factory, NctsMovementType.Codes.Arrival, string.Empty, transitStatus, messageStatus, jobNo: "NCT0001A");
				arrivalHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Acknowledged;
				arrivalHeader.ArrivalMovementHeader.BM_Phase = testCase?.InitialPhase;
				mrn = CusEntryNumber.LoadOrCreate(arrivalHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
				mrn.CE_EntryNum = testCase.MRN;

				var departureAndArrivalHeader = responseHelper.SetupPhase5MessagesForTest(Factory, NctsMovementType.Codes.DepartureAndArrival, string.Empty, transitStatus, messageStatus, jobNo: "NCT0001DA");
				arrivalHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Acknowledged;
				arrivalHeader.ArrivalMovementHeader.BM_Phase = testCase?.InitialPhase;
				mrn = CusEntryNumber.LoadOrCreate(arrivalHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
				mrn.CE_EntryNum = testCase.MRN;

				var incomingMessage = testCase.IncomingMessageText;
				var jobNumber = testCase.CorrelationIdentifier;
				if (!string.IsNullOrEmpty(jobNumber))
				{
					switch (expectedMovementType)
					{
						case NctsMovementType.Codes.Departure:
						case NctsMovementType.Codes.DepartureAndArrival:
							departureHeader.BH_HeaderType = expectedMovementType;
							departureHeader.BH_JobReference = jobNumber;
							break;
						case NctsMovementType.Codes.Arrival:
							arrivalHeader.BH_JobReference = jobNumber;
							break;
					}
					incomingMessage = incomingMessage.Replace($"<correlationIdentifier>{jobNumber}</correlationIdentifier>", $"<correlationIdentifier>{jobNumber}/123</correlationIdentifier>");
				}

				Factory.Save();

				var ediMessage = CreateTestInboundMessage();
				ediMessage.EM_MessageText = incomingMessage;
				var serviceLogger = new TestServiceLogger();
				var processor = new NctsResponseMessageProcessorPhase5(serviceLogger).GetApplicationTypeProcessorCore(ediMessage) as NctsBaseProcessor<T>;
				processor.ProcessMessage(ediMessage);

				CombineAssertions(() =>
				{
					var headerFromMessage = processor.NctsHeaderItem;
					AssertNotNull("NCTS header should not be null", headerFromMessage);
					if (!string.IsNullOrEmpty(jobNumber))
					{
						AssertEquals("Should match the job number", jobNumber, headerFromMessage.BH_JobReference);
					}
					switch (expectedMovementType)
					{
						case NctsMovementType.Codes.Departure:
							AssertEquals("Should match departure header", departureHeader.PK, headerFromMessage.PK);
							break;
						case NctsMovementType.Codes.Arrival:
							AssertEquals("Should match arrival header", arrivalHeader.PK, headerFromMessage.PK);
							break;
						default:
							Assert("Should match either departure or arrival header", headerFromMessage.PK == departureHeader.PK || headerFromMessage.PK == arrivalHeader.PK);
							break;
					}
				});
			}
		}

		[ExpectNoExceptions]
		public void TestGetNctsHeader_JobNotFoundEmailsNotificationGroup()
		{
			var groupNotificationRegistryItem = new NctsGroupNotification(Core.Constants.EmailTo.NominatedGroup, CreateGlbGroup(Factory).PK);

			using (EU.Registry.EUCustomsDataRegistry.Instance.SendNctsErrors.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, groupNotificationRegistryItem))
			{
				var testCase = ProcessorTestCases.OfType<MessageProcessorTestCase>().FirstOrDefault();
				if (!string.IsNullOrEmpty(testCase?.MRN))
				{
					var transitStatus = testCase.InitialTransitStatus ?? string.Empty;
					var messageStatus = testCase.ExpectedNewMessageStatus;
					var expectedMovementType = testCase.ExpectedMovementType;

					var departureHeader = responseHelper.SetupPhase5MessagesForTest(Factory, NctsMovementType.Codes.Departure, string.Empty, transitStatus, messageStatus, jobNo: "NCT0001D");
					departureHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Acknowledged;
					departureHeader.MovementHeader.BM_Phase = testCase?.InitialPhase;
					var mrn = CusEntryNumber.LoadOrCreate(departureHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
					mrn.CE_EntryNum = "ZZZ";

					var arrivalHeader = responseHelper.SetupPhase5MessagesForTest(Factory, NctsMovementType.Codes.Arrival, string.Empty, transitStatus, messageStatus, jobNo: "NCT0001A");
					arrivalHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Acknowledged;
					arrivalHeader.ArrivalMovementHeader.BM_Phase = testCase?.InitialPhase;
					mrn = CusEntryNumber.LoadOrCreate(arrivalHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
					mrn.CE_EntryNum = "ZZZ";

					var departureAndArrivalHeader = responseHelper.SetupPhase5MessagesForTest(Factory, NctsMovementType.Codes.DepartureAndArrival, string.Empty, transitStatus, messageStatus, jobNo: "NCT0001DA");
					departureAndArrivalHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Acknowledged;
					departureAndArrivalHeader.ArrivalMovementHeader.BM_Phase = testCase?.InitialPhase;
					mrn = CusEntryNumber.LoadOrCreate(departureAndArrivalHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
					mrn.CE_EntryNum = "ZZZ";

					var incomingMessageText = testCase.IncomingMessageText;
					var jobNumber = testCase.CorrelationIdentifier;

					if (!string.IsNullOrEmpty(jobNumber))
					{
						switch (expectedMovementType)
						{
							case NctsMovementType.Codes.Departure:
							case NctsMovementType.Codes.DepartureAndArrival:
								departureHeader.BH_HeaderType = expectedMovementType;
								departureHeader.BH_JobReference = jobNumber;
								break;
							case NctsMovementType.Codes.Arrival:
								arrivalHeader.BH_JobReference = jobNumber;
								break;
						}
						incomingMessageText = incomingMessageText.Replace($"<correlationIdentifier>{jobNumber}</correlationIdentifier>", $"<correlationIdentifier>{jobNumber}/123</correlationIdentifier>");
					}

					Factory.Save();

					var ediMessage = CreateTestInboundMessage();
					ediMessage.EM_MessageText = incomingMessageText;
					responseHelper.MakeIncomingInterchangeForTest(Factory, ediMessage);
					var serviceLogger = new TestServiceLogger();
					var processor = new NctsResponseMessageProcessorPhase5(serviceLogger).GetApplicationTypeProcessorCore(ediMessage) as NctsBaseProcessor<T>;

					processor.ProcessMessage(ediMessage);
					Factory.Save();

					var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();

					CombineAssertions(() =>
					{
						AssertNull("NCTS header not found", processor.NctsHeaderItem);
						AssertEquals("email subject", "NCTS Phase 5 job not found for incoming message Response", email.Subject);
						AssertEquals("email recipient count", 2, email.Recipients.Count);
						AssertEquals("1st email recipient in group", "staff1@emailgroup.com", email.Recipients[0].Email);
						AssertEquals("2nd email recipient in group", "staff2@emailgroup.com", email.Recipients[1].Email);
						AssertContains("email body with hyperlink to message", expectedEmailBody, email.Body);
					});
				}
			}
		}

		GlbGroup CreateGlbGroup(BusinessObjectFactory factory)
		{
			var emailGroup = factory.NewWithValidTestData<GlbGroup>();
			var staff1 = factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "staff1@emailgroup.com";
			emailGroup.Staff.Add(staff1);
			var staff2 = factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "staff2@emailgroup.com";
			emailGroup.Staff.Add(staff2);
			factory.Save();
			return emailGroup;
		}

		protected NCTSInboundEDIMessage CreateTestInboundMessage()
		{
			var ediMessage = Factory.New<NCTSInboundEDIMessage>();
			var messageType = typeof(T).Name;
			ZString messageCode = null;
			if (messageType.Length > 3)
			{
				messageCode = messageType[2] == '0' ? messageType.Substring(3, 3) : messageType.Substring(2, 3);
			}
			ediMessage.EM_MessageSubType = messageCode;
			ediMessage.EM_MessageType = "GB";
			ediMessage.EM_MessageNum = "123";
			return ediMessage;
		}

		protected abstract IEnumerable<MessageProcessorTestCase> ProcessorTestCases { get; }

		protected virtual IEnumerable<MessageShouldBeDiscardedTestCase> MessageShouldBeDiscardedTestCases => null;

		protected readonly CtcNctsResponseHelperTest responseHelper = new CtcNctsResponseHelperTest
		{
			MessageApplicationCode = EDIInterchange.ApplicationCodes.GbCustomsNCTS,
			ResourceNamePrefix = "Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Testing.NCTS.Processors.Phase5.TestFiles."
		};

		public void SetupGuaranteesAndTransactions(EU.NCTS.Business.NctsHeader nctsHeader) => SetupGuaranteesAndTransactionsCore(nctsHeader, 25000m, -25000m);
		protected virtual void SetupGuaranteesAndTransactionsCore(EU.NCTS.Business.NctsHeader nctsHeader, decimal boundAmount, decimal transactionValue, string transactionComment = "NCTS Transaction")
		{
			EU.NCTS.Business.Testing.NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
			var cusGuaranteeHeader = CreateCusGuaranteeHeader(Factory, "ABC123", ZGuid.Empty);
			_ = cusGuaranteeHeader.AddTransaction(nctsHeader.GetPermitReference(), transactionComment, "", "", transactionValue, 0, status: PermitTransactionStatusList.Codes.Pending);
			nctsHeader.Principal.E2_OA_Address = cusGuaranteeHeader.PermitHolder.MainAddress.PK;
			var movementHeader = nctsHeader.MovementHeader;

			var nctsGuarantee = nctsHeader.IsPhase5Departure ? movementHeader.Guarantees[0] : nctsHeader.Guarantees[0];
			nctsGuarantee.PW_BondType = "1";
			nctsGuarantee.PW_Password = "AR1";
			nctsGuarantee.PW_CPH_Guarantee = cusGuaranteeHeader.PK;
			nctsGuarantee.PW_BondAmount = boundAmount;
		}

		protected EU.Business.CusGuaranteeHeader CreateCusGuaranteeHeader(BusinessObjectFactory factory, string number, ZGuid holderPK)
		{
			var cusGuaranteeHeader = factory.NewWithValidTestData<EU.Business.CusGuaranteeHeader>();
			cusGuaranteeHeader.CPH_ApplicationCode = Customs.Common.Shared.CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			cusGuaranteeHeader.CPH_Type = EU.Business.CodeDescriptionPairLists.EUGuaranteeTypeList.Codes.TRA;
			cusGuaranteeHeader.CPH_SubType = "1";
			cusGuaranteeHeader.CPH_Number = number;
			cusGuaranteeHeader.CPH_StartDate = ZDate.Today.AddYears(-1);
			cusGuaranteeHeader.CPH_EndDate = ZDate.Today.AddYears(1);
			cusGuaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			if (holderPK.IsValid)
			{
				cusGuaranteeHeader.CPH_OH_PermitHolder = holderPK;
			}
			_ = cusGuaranteeHeader.AddTransaction("0001", "Opening Balance", "", "", 100000m, 0, transactionType: Customs.Business.PermitTransactionTypeList.Codes.OBL);
			cusGuaranteeHeader.CPH_Balance = 100000m;
			return cusGuaranteeHeader;
		}

		protected void AssertTransactions(EU.NCTS.Business.NctsHeader nctsHeader) => AssertTransactionsCore(nctsHeader, 2, -25000m);
		protected virtual void AssertTransactionsCore(EU.NCTS.Business.NctsHeader nctsHeader, int expectedCount, decimal expectedValue, string expectedStatus = PermitTransactionStatusList.Codes.Confirmed, string expectedComment = "NCTS Transaction")
		{
			var factory = new BusinessObjectFactory();
			var nctsGuarantee = factory.Load<NctsGuarantee>(nctsHeader.MovementHeader.Guarantees[0].PK);

			var expectedReference = nctsHeader.MovementHeader.BM_PaperlessInbondNum;
			var expectedType = PermitTransactionTypeList.Codes.TRA;
			var expectedCategory = PermitTransactionCategoryList.Codes.CUM;

			CombineAssertions(() =>
			{
				var transactions = nctsGuarantee.CusGuarantee.GetTransactions().ToArray();
				AssertEquals($"Transaction count should be {expectedCount}", expectedCount, transactions.Length);
				var newTransaction = transactions[expectedCount - 1];
				newTransaction = factory.Load<SharedCusPermitLineTransaction>(newTransaction.PK);
				AssertEquals("Transaction value", expectedValue, newTransaction.CPL_TranValue);
				AssertEquals("Reference", expectedReference, newTransaction.CPL_Reference);
				AssertEquals("Comment", expectedComment, newTransaction.CPL_Comment);
				AssertEquals("Type", expectedType, newTransaction.CPL_TransactionType);
				AssertEquals("Category", expectedCategory, newTransaction.CPL_TransactionCategory);
				AssertEquals("Status", expectedStatus, newTransaction.CPL_TransactionStatus);
			});
		}

		readonly string expectedEmailBody = @"Unable to find a linked business object for message (Interchange Number:24, Message Number:123, Message Type:GB). Message Status set to FAL.</p><p><a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=EDIMessage&BusinessEntityPK";
	}
}
