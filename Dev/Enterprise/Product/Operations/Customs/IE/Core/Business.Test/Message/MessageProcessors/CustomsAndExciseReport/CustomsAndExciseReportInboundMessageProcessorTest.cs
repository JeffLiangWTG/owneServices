using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using FlexCel.XlsAdapter;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestsSubclassesOf(typeof(CustomsAndExciseReportInboundMessageProcessor<>))]
	public abstract class CustomsAndExciseReportInboundMessageProcessorTest<TMessageProcessor, TDataProvider> : TestCaseWithFactory
		where TMessageProcessor : CustomsAndExciseReportInboundMessageProcessor<TDataProvider>
	{
		public void TestMessageFriendlyName()
		{
			AssertEquals("Message Processor should have a Correct FriendlyName.", MessageFriendlyName, Processor.MessageFriendlyName);
		}

		public void TestEndToEndProcessing()
		{
			var (outgoingMessage, incomingMessage) = CreateSetupData();
			using (incomingMessage.Factory.AddDisposableService())
			{
				var responseDetail = ResponseMessageDetails.GetResponseDetail(incomingMessage.EM_ApplicationCode, incomingMessage.EM_MessageType, incomingMessage.EM_MessageSubType, incomingMessage.EM_MessageText);
				var processorType = responseDetail.ProcessorType;
				var processor = (TMessageProcessor)Activator.CreateInstance(processorType, logger, responseDetail.XmlObjectType);
				processor.PreProcessMessage(incomingMessage);
				CombineAssertions("PreProcess", () =>
				{
					AssertEquals("incomingMessage.EM_GB", outgoingMessage.Branch.PK, incomingMessage.EM_GB);
					AssertEquals("incomingMessage.EM_LinkUniqueID", outgoingMessage.PK, incomingMessage.EM_LinkUniqueID);
					AssertEquals("incomingMessage.EM_LinkTable", outgoingMessage.TableName, incomingMessage.EM_LinkTable);
					AssertEquals("incomingMessage.EM_Status", EDIMessage.Status.PreProcessedOK, incomingMessage.EM_Status);
				});
				processor.ProcessMessage(incomingMessage);
				CombineAssertions("Process", () =>
				{
					AssertProcessResult(outgoingMessage, incomingMessage);
				});
			}
		}

		protected void ProcessMessage(CustomsAndExciseReportInboundMessage message)
		{
			using (message.Factory.AddDisposableService())
			{
				Processor.PreProcessMessage(message);
				if (message.EM_Status != EDIMessage.Status.Error)
				{
					Processor.ProcessMessage(message);
				}
			}
		}

		void AssertMessageInterpretation(CustomsAndExciseReportInboundMessage incomingMessage, ZString expectedInterpretation)
		{
			AssertXMLEquals(
				"EM_MessageInterpretation",
				expectedInterpretation.RemoveLineBreakingsAndIndents(),
				incomingMessage.EM_MessageInterpretation.RemoveLineBreakingsAndIndents()
			);
		}

		protected void AssertProcessResult(CustomsAndExciseReportOutboundMessage messageAttachee, CustomsAndExciseReportInboundMessage incomingMessage)
		{
			AssertEquals("Message should have been set PRS.", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			AssertMessageInterpretation(incomingMessage, ExpectedMessageInterpretation);
			AssertProcessResultCore(messageAttachee);
		}

		protected virtual void AssertProcessResultCore(CustomsAndExciseReportOutboundMessage messageAttachee)
		{
			AssertEquals("LinkedObject should have been set to Received.", EDIMessageStatusList.Codes.Received, messageAttachee.EM_Status);
			AssertEDocAttachment(messageAttachee);
			MessageProcessorNotificationTestHelper.AssertEmail($"{MessageFriendlyName} ({MessageType})",
				new string[] { $"{MessageFriendlyName} ({MessageType}) Response for IER00001000" },
				new string[] { "staff1@where.com" });
		}

		protected virtual (CustomsAndExciseReportOutboundMessage outgoingMessage, CustomsAndExciseReportInboundMessage incomingMessage) CreateSetupData(string incomingMessageText = null)
		{
			var outgoingMessage = Factory.New<CustomsAndExciseReportOutboundMessage>();
			outgoingMessage.EM_MessageNum = "IER00001000";
			outgoingMessage.EM_GB = Branch.PK;
			outgoingMessage.EM_SystemCreateUser = Staff.GS_Code;
			outgoingMessage.EM_MessageType = MessageType;
			outgoingMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;

			var incomingMessage = CreateNewIncomingMessage(incomingMessageText);
			incomingMessage.EM_LinkedObject = outgoingMessage;

			return (outgoingMessage, incomingMessage);
		}

		protected virtual CustomsAndExciseReportInboundMessage CreateNewIncomingMessage(string incomingMessageText = null)
		{
			var message = Factory.New<CustomsAndExciseReportInboundMessage>();
			message.EM_GB = Branch.PK;
			message.EM_MessageType = MessageType;
			message.EM_MessageText = incomingMessageText ?? MessageText;
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			message.EM_Status = EDIMessage.Status.Queued;
			return message;
		}

		protected GlbCompany Company
		{
			get
			{
				if (company == null)
				{
					company = Factory.New<GlbCompany>();
					company.GC_Code = "CIE";
					company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
				}
				return company;
			}
		}
		GlbCompany company;

		protected GlbBranch Branch
		{
			get
			{
				if (branch == null)
				{
					branch = Company.Branches.AddNew();
					branch.GB_Code = "BIE";
					branch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
				}
				return branch;
			}
		}
		GlbBranch branch;

		protected GlbStaff Staff => staff ??= MessageProcessorNotificationTestHelper.SetupStaffData(Factory);
		GlbStaff staff;

		protected abstract ZString MessageFriendlyName { get; }

		protected abstract TMessageProcessor Processor { get; }

		protected abstract ZString MessageType { get; }

		protected abstract ZString MessageText { get; }

		protected abstract ZString ExpectedMessageInterpretation { get; }

		protected virtual (
			string FileName,
			(int row, (int column, string expectedValue)[] values)[] Cells
		) XlsAttachmentTestCases => default;

		protected override void SetUp()
		{
			base.SetUp();
			logger = new LoggingInformation();
		}
		protected LoggingInformation logger;

		void AssertEDocAttachment(CustomsAndExciseReportOutboundMessage outgoingMessage)
		{
			if (XlsAttachmentTestCases != default)
			{
				var doc = outgoingMessage.DocManagerInfo.Files[0];
				AssertContains("Document name", XlsAttachmentTestCases.FileName, doc.FileName);

				var testStream = new MemoryStream(doc.ImageData);
				var xlsFile = new XlsFile(testStream, false);
				xlsFile.ActiveSheet = 1;

				foreach (var expectedRow in XlsAttachmentTestCases.Cells)
				{
					foreach (var expectedValue in expectedRow.values)
					{
						AssertEquals($"Value on cell [{expectedRow.row}][{expectedValue.column}].", expectedValue.expectedValue, xlsFile.GetCellValue(expectedRow.row, expectedValue.column));
					}
				}
			}
		}
	}
}
