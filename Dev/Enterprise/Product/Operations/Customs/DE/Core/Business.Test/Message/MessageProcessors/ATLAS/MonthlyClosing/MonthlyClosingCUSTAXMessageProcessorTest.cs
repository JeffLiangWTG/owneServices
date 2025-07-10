using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	[TestedType(typeof(MonthlyClosingCUSTAXMessageProcessor))]
	sealed class MonthlyClosingCUSTAXMessageProcessorTest : MessageProcessorAbstractTest<MonthlyClosingCUSTAXMessageProcessor, AtlasInboundEDIMessage<ICUSTAX>>
	{
		[ExpectNoExceptions]
		public void TestCusEntryLineUpdateDutyPercent()
		{
			SetupTestData();
			var cusReconEntry = declaration.CusReconEntries.AddNew();
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			cusReconEntry.CRE_CH_OriginalEntry = cusEntryHeader.PK;
			var cusReconEntryLine1 = cusReconEntry.CusReconEntryLines.AddNew();
			cusReconEntryLine1.CRL_LineNumber = 1;
			cusReconEntryLine1.CRL_OriginalEntryLineNumber = 1;
			var cusReconEntryLine2 = cusReconEntry.CusReconEntryLines.AddNew();
			cusReconEntryLine2.CRL_LineNumber = 2;
			cusReconEntryLine2.CRL_OriginalEntryLineNumber = 2;

			var entryLine1 = cusEntryHeader.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;

			var entryLine2 = cusEntryHeader.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine2.CL_DutyPercent = 5;

			var line1 = new Mock<ICUSTAXLine>();
			line1.Setup(x => x.LineNumber).Returns("1");
			line1.Setup(x => x.Duties).Returns(GetDuties());

			var line2 = new Mock<ICUSTAXLine>();
			line2.Setup(x => x.LineNumber).Returns("2");
			line2.Setup(x => x.Duties).Returns(Array.Empty<ICUSTAXLineDuty>());

			var line3 = new Mock<ICUSTAXLine>();
			line3.Setup(x => x.LineNumber).Returns("3");
			line3.Setup(x => x.Duties).Returns(GetDuties());

			dataProviderMock.Setup(x => x.Lines).Returns([line1.Object, line2.Object, line3.Object]);

			ProcessMessage(message);
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(entryLine1.CL_DutyPercent, Is.EqualTo((ZDecimal)1));
				NUnit.Framework.Assert.That(entryLine2.CL_DutyPercent, Is.EqualTo((ZDecimal)5));
				NUnit.Framework.Assert.That(logger.UserLogStrings, Does.Contain("\tThe corresponding Line with Line Number 3 couldn't be found."));
			});

			ICUSTAXLineDuty[] GetDuties()
			{
				var duty1 = new Mock<ICUSTAXLineDuty>();
				duty1.Setup(x => x.DutyRates).Returns(GetRates(1));
				duty1.Setup(x => x.ChargeType).Returns("A0000");

				var duty2 = new Mock<ICUSTAXLineDuty>();
				duty2.Setup(x => x.DutyRates).Returns(GetRates(2));
				duty2.Setup(x => x.ChargeType).Returns("A0001");
				return [duty1.Object, duty2.Object];
			}

			ICUSTAXLineDutyRate[] GetRates(decimal rate)
			{
				var rate1 = new Mock<ICUSTAXLineDutyRate>();
				rate1.Setup(x => x.Rate).Returns(rate);
				rate1.Setup(x => x.AssessmentScale).Returns("00");

				var rate2 = new Mock<ICUSTAXLineDutyRate>();
				rate2.Setup(x => x.Rate).Returns(rate + 7m);
				rate2.Setup(x => x.AssessmentScale).Returns("01");

				return [rate1.Object, rate2.Object];
			}
		}

		public void TestGetLinkedObject()
		{
			SetupTestData();

			ProcessMessage(message);
			AssertEquals("LinkedObject from ReferenceNumber", declaration, message.EM_LinkedObject);
		}

		public void TestGetLinkedObject_MRN()
		{
			SetupTestData();
			mrnEntryNumber.CE_EntryNum = "19DE485154386041M4";
			dataProviderMock.Setup(x => x.MRN).Returns("19DE485154386041M4");
			Factory.Save();

			ProcessMessage(message);
			AssertEquals("LinkedObject from MRMN", declaration, message.EM_LinkedObject);
		}

		public void TestNoLinkedObject()
		{
			SetupTestData();

			dataProviderMock.Setup(x => x.ReferenceNumber).Returns("ATB150000000000000000");
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertNull("No LinkedObject", message.EM_LinkedObject);
				AssertEquals("Message status == error", EDIMessage.Status.Error, message.EM_Status);
			});
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			SetupTestData();

			messageMock.Setup(m => m.DataProvider).Returns((ICUSTAX)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestProcessMessage()
		{
			SetupTestData();

			ProcessMessage(message);
			AssertEquals("EM_Status", AesEDIMessage.Status.ProcessedOK, message.EM_Status);
		}

		public void TestSetLogbookFinalizationFlag()
		{
			SetupTestData();

			CombineAssertions(() =>
			{
				AssertEquals("Initially there's no note", false, declaration.Notes.FindByDescription(MonthlyClosingHelper.FinalizationFlagNoteDescription).Any());
				ProcessMessage(message);

				var stmNote = declaration.Notes.FindByDescription(MonthlyClosingHelper.FinalizationFlagNoteDescription).Single();
				AssertEquals("New note created", "1", stmNote.ST_NoteDataAsText);

				stmNote.ST_NoteDataAsText = ZString.Empty;
				ProcessMessage(message);
				AssertEquals("Existing note updated", "1", stmNote.ST_NoteDataAsText);
			});
		}

		public void TestSetLogbookRegistrationNumber()
		{
			SetupTestData();

			ProcessMessage(message);
			AssertEquals("ATB150000620520195875", message.GetLogbookRegistrationNumber());
		}

		public void TestPopulateLogbookRegNum_MRN()
		{
			SetupTestData();
			mrnEntryNumber.CE_EntryNum = "19DE485154386041M4";
			dataProviderMock.Setup(x => x.MRN).Returns("19DE485154386041M4");
			Factory.Save();

			ProcessMessage(message);
			AssertEquals("ATB150000620520195875, 19DE485154386041M4", message.GetLogbookRegistrationNumber());
		}

		public void TestUpdateReconDeclarationStatus()
		{
			SetupTestData();

			CombineAssertions(() =>
			{
				for (int i = 1; i <= 6; i++)
				{
					dataProviderMock.Setup(x => x.CompletionFlag).Returns(i.ToString());
					ProcessMessage(message);
					AssertEquals($"CRD_CustomsStatus when CompletionFlag = '{i}'", $"TX{i}", declaration.CRD_CustomsStatus);
				}

				declaration.CRD_CustomsStatus = ZString.Empty;
				dataProviderMock.Setup(x => x.CompletionFlag).Returns("7");
				ProcessMessage(message);
				AssertEquals("Invalid CompletionFlag, CRD_CustomsStatus not updated", ZString.Empty, declaration.CRD_CustomsStatus);

				dataProviderMock.Setup(x => x.CompletionFlag).Returns(ZString.Empty);
				ProcessMessage(message);
				AssertEquals("CompletionFlag not specified, CRD_CustomsStatus not updated", ZString.Empty, declaration.CRD_CustomsStatus);
			});
		}

		public void TestUpdateEntryLineCustomsValue()
		{
			SetupTestData();

			var cusReconEntry = declaration.CusReconEntries.AddNew();
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			cusReconEntry.CRE_CH_OriginalEntry = cusEntryHeader.PK;
			var cusReconEntryLine = cusReconEntry.CusReconEntryLines.AddNew();
			cusReconEntryLine.CRL_LineNumber = 1;
			cusReconEntryLine.CRL_OriginalEntryLineNumber = 5;
			var entryLine = cusEntryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 5;
			entryLine.CL_CustomsValue = 1;
			cusReconEntry.EntryHeader.AllEntryLines.Add(entryLine);

			CombineAssertions(() =>
			{
				dataProviderMockLine.Setup(x => x.CustomsValue).Returns(() => null);
				ProcessMessage(message);
				AssertEquals("line.CustomsValue is null", 1m, entryLine.CL_CustomsValue);

				dataProviderMockLine.Setup(x => x.CustomsValue).Returns(() => 17m);
				ProcessMessage(message);
				AssertEquals("line.CustomsValue == 17", 17m, entryLine.CL_CustomsValue);

				entryLine.CL_LineNumber = 1;
				entryLine.CL_CustomsValue = 1;
				ProcessMessage(message);
				AssertEquals("line.CustomsValue is null", 1m, entryLine.CL_CustomsValue);
				Assert(logger.UserLogStrings.Contains("\tThe corresponding Line with Line Number 1 couldn't be found."));
			});
		}

		public void TestUpdateReconEntryLineStatus()
		{
			SetupTestData();

			var entry = declaration.CusReconEntries.AddNew();
			var line = entry.CusReconEntryLines.AddNew();
			line.CRL_LineNumber = 1;

			CombineAssertions(() =>
			{
				for (int i = 1; i <= 6; i++)
				{
					dataProviderMockLine.Setup(x => x.LineCompletionFlag).Returns(i.ToString());
					ProcessMessage(message);
					AssertEquals($"CRL_CustomsStatus when CompletionFlag = '{i}'", $"TX{i}", line.CRL_CustomsStatus);
				}

				line.CRL_CustomsStatus = ZString.Empty;
				dataProviderMockLine.Setup(x => x.LineCompletionFlag).Returns("7");
				ProcessMessage(message);
				AssertEquals("Invalid CompletionFlag, CRL_CustomsStatus not updated", ZString.Empty, line.CRL_CustomsStatus);

				dataProviderMockLine.Setup(x => x.LineCompletionFlag).Returns(string.Empty);
				ProcessMessage(message);
				AssertEquals("CompletionFlag not specified, CRL_CustomsStatus not updated", ZString.Empty, declaration.CRD_CustomsStatus);

				line.CRL_LineNumber = 2;
				dataProviderMockLine.Setup(x => x.LineCompletionFlag).Returns("1");
				ProcessMessage(message);
				AssertEquals("No line with matching LineNumber", ZString.Empty, line.CRL_CustomsStatus);
			});
		}

		public void TestGenerateEmail()
		{
			SetupTestData();

			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;

			dataProviderMock.Setup(x => x.MRN).Returns("19DE485154386041M4");
			ProcessMessage(message);

			var reference = declaration.CRD_JobReferenceNumber;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();

			var subject = $"Monthly Closing CUSTAX – Customs Tax Assessment Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = $@"<strong>Monthly Closing CUSTAX – Customs Tax Assessment Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=DEMonthlyClosing&BusinessEntityPK=" + declaration.PK;
			var bodyMessageSummary = $@"Your Monthly Closing Declaration for Job {reference} received a Customs Tax Assessment. For details please follow the link to the job.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
									+ "<tr><td>MRN</td><td>19DE485154386041M4</td></tr>"
									+ "<tr><td>Registration Number</td><td>ATB150000620520195875</td></tr>"
									+ "<tr><td>Local Reference Number</td><td>MAS/22/11/22027</td></tr>"
									+ "</table>";
			AssertEmailForSingleRecipientWithTable(ZString.Empty, email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
		}

		public void TestCusEntryLineFeesDeleted()
		{
			SetupTestData();

			dataProviderMock.Setup(x => x.Lines).Returns(new[] { dataProviderMockLineForFees.Object });

			var cusReconEntry = declaration.CusReconEntries.AddNew();
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			cusReconEntry.CRE_CH_OriginalEntry = cusEntryHeader.PK;
			var cusReconEntryLine = cusReconEntry.CusReconEntryLines.AddNew();
			cusReconEntryLine.CRL_LineNumber = 2;
			cusReconEntryLine.CRL_OriginalEntryLineNumber = 5;
			var entryLine = cusEntryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 5;
			var feeToBeDeleted = entryLine.ConfirmedFees.AddNew();
			var feeToStay = entryLine.Fees.AddNew();
			feeToStay.CF_Source = "CW1";
			cusReconEntry.EntryHeader.AllEntryLines.Add(entryLine);

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("cusEntryLineFee with CF_Source = 'CUS' deleted", true, feeToBeDeleted.IsDeleted);
				AssertEquals("cusEntryLineFee with CF_Source = 'CW1' not deleted", false, feeToStay.IsDeleted);
			});
		}

		public void TestCusEntryLineFeesCreated()
		{
			SetupTestData();

			dataProviderMock.Setup(x => x.Lines).Returns(new[] { dataProviderMockLineForFees.Object });
			var cusReconEntry = declaration.CusReconEntries.AddNew();
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			cusReconEntry.CRE_CH_OriginalEntry = cusEntryHeader.PK;
			var cusReconEntryLine = cusReconEntry.CusReconEntryLines.AddNew();
			cusReconEntryLine.CRL_LineNumber = 2;
			cusReconEntryLine.CRL_OriginalEntryLineNumber = 5;
			var entryLine = cusEntryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 5;
			entryLine.ConfirmedFees.RemoveAll(); //Ensure Empty

			ProcessMessage(message);

			AssertEquals("New cusEntryLineFee Created", dataProviderMockLineForFees.Object.Duties.Count, entryLine.ConfirmedFees.Count);
		}

		public void TestLogIfCorrespondingCusReconEntryLineNotFound_InvalidLineNumber()
		{
			SetupTestData();

			var cusReconEntry = declaration.CusReconEntries.AddNew();
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			cusReconEntry.CRE_CH_OriginalEntry = cusEntryHeader.PK;
			var cusReconEntryLine = cusReconEntry.CusReconEntryLines.AddNew();
			cusReconEntryLine.CRL_LineNumber = 2;
			cusReconEntryLine.CRL_OriginalEntryLineNumber = 5;
			var entryLine = cusEntryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 5;
			entryLine.ConfirmedFees.RemoveAll(); //Ensure Empty

			ProcessMessage(message);

			Assert(logger.UserLogStrings.Contains("\tThe corresponding Line with Line Number 1 couldn't be found."));
		}

		public void TestLogIfCorrespondingCusReconEntryLineNotFound_LinkedEntryHeaderNull()
		{
			SetupTestData();

			var cusReconEntry = declaration.CusReconEntries.AddNew();
			var cusReconEntryLine = cusReconEntry.CusReconEntryLines.AddNew();
			cusReconEntryLine.CRL_LineNumber = 1;
			cusReconEntryLine.CRL_OriginalEntryLineNumber = 5;
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			var entryLine = cusEntryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 5;
			entryLine.ConfirmedFees.RemoveAll(); //Ensure Empty

			ProcessMessage(message);

			Assert(logger.UserLogStrings.Contains("\tThe corresponding Line with Line Number 1 couldn't be found."));
		}

		public void TestLogIfCorrespondingCusReconEntryLineNotFound_NoOriginalLine()
		{
			SetupTestData();

			var cusReconEntry = declaration.CusReconEntries.AddNew();
			var cusReconEntryLine = cusReconEntry.CusReconEntryLines.AddNew();
			cusReconEntryLine.CRL_LineNumber = 1;
			cusReconEntryLine.CRL_OriginalEntryLineNumber = 5;
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			var entryLine = cusEntryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.ConfirmedFees.RemoveAll(); //Ensure Empty

			ProcessMessage(message);

			Assert(logger.UserLogStrings.Contains("\tThe corresponding Line with Line Number 1 couldn't be found."));
		}

		protected override ZString MessageFriendlyName => "Monthly Closing CUSTAX Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<ICUSTAX>> Processor => new MonthlyClosingCUSTAXMessageProcessor(logger);

		void SetupTestData()
		{
			declaration = Factory.NewWithValidTestData<CusReconDeclaration>();

			mrnEntryNumber = CusEntryNumber.LoadOrCreate(declaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = "ATB150000620520195875";

			outgoingMessage = CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(declaration, "ABC123456");
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAtlasSystem;
			outgoingMessage.EM_MessageType = EDIMessageTypeList.Codes.Import;
			outgoingMessage.EM_MessageSubType = MonthlyClosingMessageSubTypeList.Codes.MonthlyClosingFreeCirculation;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			dataProviderMockLine = new Mock<ICUSTAXLine>();
			dataProviderMockLine.Setup(x => x.LineNumber).Returns("1");
			dataProviderMockLine.Setup(x => x.LineCompletionFlag).Returns(ImportCompletionFlagList.Codes._6);
			dataProviderMockLine.Setup(x => x.Duties).Returns(Array.Empty<ICUSTAXLineDuty>());

			var dataProviderMockLineForFeesDutyLine = new Mock<ICUSTAXLineDuty>();
			dataProviderMockLineForFeesDutyLine.Setup(e => e.ChargeAmount).Returns(1000.0m);
			dataProviderMockLineForFeesDutyLine.Setup(e => e.ChargeType).Returns("C1234");
			dataProviderMockLineForFeesDutyLine.Setup(e => e.BaseValue).Returns(500m);
			dataProviderMockLineForFeesDutyLine.Setup(e => e.MethodOfCalculation).Returns("ANY");
			dataProviderMockLineForFeesDutyLine.Setup(e => e.MethodOfPayment).Returns("MOP");
			dataProviderMockLineForFeesDutyLine.Setup(m => m.DutyRates).Returns(Array.Empty<ICUSTAXLineDutyRate>());

			dataProviderMockLineForFees = new Mock<ICUSTAXLine>();
			dataProviderMockLineForFees.Setup(x => x.LineNumber).Returns("2");
			dataProviderMockLineForFees.Setup(x => x.LineCompletionFlag).Returns(ImportCompletionFlagList.Codes._4);
			dataProviderMockLineForFees.Setup(x => x.Duties).Returns(new[] { dataProviderMockLineForFeesDutyLine.Object });

			dataProviderMock = new Mock<ICUSTAX>();
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns("ATB150000620520195875");
			dataProviderMock.Setup(x => x.MessageIdentifier).Returns("ABC123456");
			dataProviderMock.Setup(x => x.LocalReferenceNumber).Returns("MAS/22/11/22027");
			dataProviderMock.Setup(x => x.CompletionFlag).Returns(ZString.Empty);
			dataProviderMock.Setup(x => x.RegistrationDate).Returns(new DateTime(2020, 9, 17));
			dataProviderMock.Setup(x => x.Lines).Returns(new[] { dataProviderMockLine.Object });

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<ICUSTAX>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;

			Factory.Save();
		}

		CusEntryNumber mrnEntryNumber;
		CusReconDeclaration declaration;
		Mock<ICUSTAX> dataProviderMock;
		Mock<ICUSTAXLine> dataProviderMockLine;
		Mock<AtlasInboundEDIMessage<ICUSTAX>> messageMock;
		Mock<ICUSTAXLine> dataProviderMockLineForFees;
		AtlasInboundEDIMessage<ICUSTAX> message;
		EDIMessage outgoingMessage;
	}
}
