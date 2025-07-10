using System.Collections.Generic;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc029c;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5.Testing
{
	sealed class CC029CProcessorTest : NctsBaseProcessorTest<Cc029CType>
	{
		protected override IEnumerable<MessageProcessorTestCase> ProcessorTestCases
		{
			get
			{
				yield return new MessageProcessorTestCase
				{
					SetUpHeader = SetupNctsHeader,
					SetupGuaranteesAndTransactions = SetupGuaranteesAndTransactions1,
					LRN = "TRATESTGB12308021209",
					MRN = "23GB000246YHVFYMJ0",
					InitialTransitStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated,
					IncomingMessageText = responseHelper.GetEmbeddedResourceFile("CC029C_Message.xml"),
					MessageSubType = "29C",
					ExpectedNewDeclarationStatus = NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit,
					ExpectedNewMessageStatus = LogicalStatusList.Codes.Accepted,
					ExpectedPhase = NCTS5DeparturePhaseList.Codes.Declaration,
					ExpectedNewMessageInterpretation = @"New detailed status: Goods Released for Transit at Departure</br>
Status granted on 02/08/2023</br>
Acceptance Date 02/08/2023",
					HeaderAssertion = AssertDocumentHasBeenGenerated,
					ExpectedMovementType = NctsMovementType.Codes.Departure,
					ExpectedAcceptMovementType = NctsMovementType.Codes.Departure,
					GuaranteesAndTransactionsAssertion = AssertTransactions1,
				};

				yield return new MessageProcessorTestCase
				{
					SetUpHeader = SetupNctsHeader,
					SetupGuaranteesAndTransactions = SetupGuaranteesAndTransactions2,
					LRN = "TRATESTGB12308021209",
					MRN = "23GB000246YHVFYMJ0",
					InitialTransitStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated,
					IncomingMessageText = responseHelper.GetEmbeddedResourceFile("CC029C_Message.xml"),
					MessageSubType = "29C",
					ExpectedNewDeclarationStatus = NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit,
					ExpectedNewMessageStatus = LogicalStatusList.Codes.Accepted,
					ExpectedPhase = NCTS5DeparturePhaseList.Codes.Declaration,
					ExpectedNewMessageInterpretation = @"New detailed status: Goods Released for Transit at Departure</br>
Status granted on 02/08/2023</br>
Acceptance Date 02/08/2023",
					HeaderAssertion = AssertDocumentHasBeenGenerated,
					ExpectedMovementType = NctsMovementType.Codes.Departure,
					ExpectedAcceptMovementType = NctsMovementType.Codes.Departure,
					GuaranteesAndTransactionsAssertion = AssertTransactions2,
				};
			}
		}

		void AssertDocumentHasBeenGenerated(NctsHeader header)
		{
			var queuedPrintJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, header.PK));
			AssertEquals("There should be a document in the transit declaration print job queue.", 1, queuedPrintJobs.Length);
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(queuedPrintJobs[0].SP_CustomProperties);

				var b12 = excelInterface.WorkSheets[0].GetCell(1, 2)?.FormattedValue;
				AssertContains("Phase 5 document", "EUROPEAN UNION", b12);
			}
		}

		protected override IEnumerable<MessageShouldBeDiscardedTestCase> MessageShouldBeDiscardedTestCases
		{
			get
			{
				yield return new MessageShouldBeDiscardedTestCase
				{
					Description = "BM_CustomsStatus=MRN",
					Setup = header =>
					{
						header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
					},
					ExpectedResult = false,
				};
				yield return new MessageShouldBeDiscardedTestCase
				{
					Description = "BM_CustomsStatus=PRE",
					Setup = header =>
					{
						header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.PreLodged;
					},
					ExpectedResult = true,
					ExpectedReasonText = "The message was discarded, because the Status at Customs of the declaration was PRE",
				};
				yield return new MessageShouldBeDiscardedTestCase
				{
					Description = "BM_CustomsStatus=ACK",
					Setup = header =>
					{
						header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.Acknowledged;
					},
					ExpectedResult = true,
					ExpectedReasonText = "The message was discarded, because the Status at Customs of the declaration was ACK",
				};
				yield return new MessageShouldBeDiscardedTestCase
				{
					Description = "BM_CustomsStatus=GIV",
					Setup = header =>
					{
						header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid;
					},
					ExpectedResult = true,
					ExpectedReasonText = "The message was discarded, because the Status at Customs of the declaration was GIV",
				};
				yield return new MessageShouldBeDiscardedTestCase
				{
					Description = "BM_CustomsStatus=AMR",
					Setup = header =>
					{
						header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested;
					},
					ExpectedResult = true,
					ExpectedReasonText = "The message was discarded, because the Status at Customs of the declaration was AMR",
				};
			}
		}

		void SetupNctsHeader(NctsHeader nctsHeader)
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.BH_JobReference = "TRATESTGB12308021209";
			nctsHeader.MovementHeader.BM_TypeOfSecurity = "EXI";
		}

		void SetupGuaranteesAndTransactions1(NctsHeader nctsHeader)
		{
			SetupGuaranteesAndTransactionsCore(nctsHeader, 25000m, -2500m);
		}

		void SetupGuaranteesAndTransactions2(NctsHeader nctsHeader)
		{
			SetupGuaranteesAndTransactionsCore(nctsHeader, 1000, -1000m, "NCTS departure");
		}

		void AssertTransactions1(NctsHeader nctsHeader)
		{
			AssertTransactionsCore(nctsHeader, 3, 1000, PermitTransactionStatusList.Codes.Confirmed, $"NCTS departure adjustment {nctsHeader.MovementHeader.BM_PaperlessInbondNum}");
		}

		void AssertTransactions2(NctsHeader nctsHeader)
		{
			AssertTransactionsCore(nctsHeader, 2, -1000, PermitTransactionStatusList.Codes.Confirmed, "NCTS departure");
		}
	}
}
