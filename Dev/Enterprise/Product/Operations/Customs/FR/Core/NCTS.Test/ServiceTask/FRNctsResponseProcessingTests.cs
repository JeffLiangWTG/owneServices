using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using FRBusiness = Enterprise.Customs.EU.NCTS.Business;
using NctsTransitStatusList = Enterprise.Customs.FR.Business.NctsTransitStatusList;

namespace Enterprise.Customs.FR.NCTS.ServiceTask.Testing
{
	class FRNctsResponseProcessingTests : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();

			frNctsReponseHelper = new FRNctsReponseHelper();
		}

		public void TestProcessReceivedMessageWithoutOutgoingMessage()
		{
			var realIE029message = frNctsReponseHelper.GetEmbeddedResourceFile("DT029B_MESSAGE.xml");
			frNctsReponseHelper.SetupMessagesForTest(Factory, FRBusiness.NctsMovementType.Codes.Departure, realIE029message, FRBusiness.NctsMessageStatusList.Codes.DepartureDeclarationSent, false, NctsTransitStatusList.Codes.Unknown);
			serviceLogger = new TestServiceLogger();
			var processor = new FRNctsResponseMessageProcessor(serviceLogger);
			processor.ExecuteBatch();
			var processorLogs = processor.Logger.Logs;
			AssertEquals("The response processor should no generate any exception.", false, processorLogs.Any(x => x.Message.Contains("Exception")));
		}

		public void TestTadPrintedWhenReceivingIE029Response()
		{
			var realIE029message = frNctsReponseHelper.GetEmbeddedResourceFile("DT029B_MESSAGE.xml");
			var departure = SetupAndRunMessageProcessor(FRBusiness.NctsMovementType.Codes.Departure, realIE029message, NctsTransitStatusList.Codes.Unknown, FRBusiness.NctsMessageStatusList.Codes.DepartureDeclarationSent);
			var headerReloaded = new BusinessObjectFactory().Load<NctsHeader>(departure.PK);
			var printJobs = Factory.Load<StmPrintJob>(new ZQuery(ZArchitecture.Schema.StmPrintJobSchema.SP_ParentGuid, headerReloaded.PK));
			AssertEquals("Should have a document", 1, printJobs.Length);
			AssertContains("Document name should contain 'TAD'", "TAD", printJobs[0].SP_DocumentName);
		}

		public void TestGetFunctionalErrorInfo()
		{
			var realIEF96message = frNctsReponseHelper.GetEmbeddedResourceFile("DTF96A_MESSAGE.xml");
			var departure = SetupAndRunMessageProcessor(FRBusiness.NctsMovementType.Codes.Departure, realIEF96message, NctsTransitStatusList.Codes.Unknown, FRBusiness.NctsMessageStatusList.Codes.DepartureDeclarationSent);
			var headerReloaded = new BusinessObjectFactory().Load<NctsHeader>(departure.PK);
			AssertContains("<p>DATE LIMIT of CONTROL RESULT is in error : Infrigement to rule NAT003 - Original value : 20180425<br>CODE of PLACE OF UNLOADING in HEADER is in error : Infrigement to rule NAT004 - Original value : Bergerac<br></p>", headerReloaded.Messages.LastMessage.EM_MessageInterpretation);
		}

		public void TestGetXMLErrorInfo()
		{
			var realIEF97message = frNctsReponseHelper.GetEmbeddedResourceFile("DTF97A_MESSAGE.xml");
			var departure = SetupAndRunMessageProcessor(FRBusiness.NctsMovementType.Codes.Departure, realIEF97message, NctsTransitStatusList.Codes.Unknown, FRBusiness.NctsMessageStatusList.Codes.DepartureDeclarationSent);
			var headerReloaded = new BusinessObjectFactory().Load<NctsHeader>(departure.PK);
			AssertContains("<p>Error: Max characters exceeded(Header, line 7) - Original value : <br>Error: Other formatting error(, line 8) - Original value : 254%¨@*]<br></p>", headerReloaded.Messages.LastMessage.EM_MessageInterpretation);
		}

		public void TestGetGetGrantedTimeInfo()
		{
			var realIEF96message = frNctsReponseHelper.GetEmbeddedResourceFile("DT016A_MESSAGE.xml");
			var departure = SetupAndRunMessageProcessor(FRBusiness.NctsMovementType.Codes.Departure, realIEF96message, NctsTransitStatusList.Codes.Unknown, FRBusiness.NctsMessageStatusList.Codes.DepartureDeclarationSent);
			var headerReloaded = new BusinessObjectFactory().Load<NctsHeader>(departure.PK);
			AssertContains("Status granted on: 11/04/2019 11:28", headerReloaded.Messages.LastMessage.EM_MessageInterpretation);
		}

		public void TestExplainErrorPointer()
		{
			var processor = new DTCCF96AProcessor(new TestServiceLogger(), new LoggingInformation());
			ZString pointer = "HEA";
			var pointerInfo = processor.ExplainErrorPointer(pointer);
			AssertEquals("HEADER", pointerInfo);

			pointer = "GUA(2)";
			pointerInfo = processor.ExplainErrorPointer(pointer);
			AssertEquals("GUARANTEE 2", pointerInfo);

			pointer = "ERS(2), Date Limite";
			pointerInfo = processor.ExplainErrorPointer(pointer);
			AssertEquals("Should be capitalized", " DATE LIMITE of CONTROL RESULT 2", pointerInfo);

			pointer = "HEA.GDS(1)";
			pointerInfo = processor.ExplainErrorPointer(pointer);
			AssertEquals("GOODS ITEM 1 of HEADER", pointerInfo);

			pointer = "HEA.GDS(99).Net weight";
			pointerInfo = processor.ExplainErrorPointer(pointer);
			AssertEquals("NET WEIGHT of GOODS ITEM 99 in HEADER", pointerInfo);
		}

		[TestDate(2020, 09, 09, 14, 42, 0)]
		public void TestProcessReceivedMessage_CusRes016A()
		{
			var realIE016message = frNctsReponseHelper.GetEmbeddedResourceFile("DT016A_MESSAGE.xml");
			var departure = SetupAndRunMessageProcessor(FRBusiness.NctsMovementType.Codes.Departure, realIE016message, NctsTransitStatusList.Codes.Unknown, FRBusiness.NctsMessageStatusList.Codes.DepartureDeclarationSent);
			var headerReloaded = new BusinessObjectFactory().Load<NctsHeader>(departure.PK);
			AssertEquals(2, headerReloaded.Messages.Count);
			AssertEquals("016", headerReloaded.Messages.LastIncomingMessage.EM_MessageType);
			AssertEquals(NctsTransitStatusList.Codes.DeclarationRejected, headerReloaded.MovementHeader.BM_CustomsStatus);
			AssertEquals(headerReloaded.PK, headerReloaded.Messages.LastIncomingMessage.EM_LinkUniqueID);
		}

		[TestDate(2020, 09, 09, 14, 42, 0)]
		public void TestProcessReceivedMessage_CusRes028A()
		{
			var realIE028message = frNctsReponseHelper.GetEmbeddedResourceFile("DT028A_MESSAGE.xml");
			var departure = SetupAndRunMessageProcessor(FRBusiness.NctsMovementType.Codes.Departure, realIE028message, NctsTransitStatusList.Codes.Unknown, FRBusiness.NctsMessageStatusList.Codes.DepartureDeclarationSent);
			var headerReloaded = new BusinessObjectFactory().Load<NctsHeader>(departure.PK);
			AssertEquals(2, headerReloaded.Messages.Count);
			AssertEquals("028", headerReloaded.Messages.LastIncomingMessage.EM_MessageType);
			AssertEquals(NctsTransitStatusList.Codes.DeclarationMrnAllocated, headerReloaded.MovementHeader.BM_CustomsStatus);
			AssertEquals(true, headerReloaded.IsDepartureAmendmentAllowed);
			AssertEquals(headerReloaded.PK, headerReloaded.Messages.LastIncomingMessage.EM_LinkUniqueID);
		}

		[TestDate(2020, 09, 09, 14, 42, 0)]
		public void TestProcessReceivedMessage_CusRes029B()
		{
			var realIE029message = frNctsReponseHelper.GetEmbeddedResourceFile("DT029B_MESSAGE.xml");
			var departure = SetupAndRunMessageProcessor(FRBusiness.NctsMovementType.Codes.Departure, realIE029message, NctsTransitStatusList.Codes.Unknown, FRBusiness.NctsMessageStatusList.Codes.DepartureDeclarationSent);
			var headerReloaded = new BusinessObjectFactory().Load<NctsHeader>(departure.PK);
			AssertEquals(2, headerReloaded.Messages.Count);
			AssertEquals("029", headerReloaded.Messages.LastIncomingMessage.EM_MessageType);
			AssertEquals(NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture, headerReloaded.MovementHeader.BM_CustomsStatus);
			AssertEquals(headerReloaded.PK, headerReloaded.Messages.LastIncomingMessage.EM_LinkUniqueID);
		}

		[TestDate(2020, 09, 09, 14, 42, 0)]
		public void TestProcessReceivedMessage_CusResF96A()
		{
			var realIEF96message = frNctsReponseHelper.GetEmbeddedResourceFile("DTF96A_MESSAGE.xml");
			var departure = SetupAndRunMessageProcessor(FRBusiness.NctsMovementType.Codes.Departure, realIEF96message, NctsTransitStatusList.Codes.Unknown, FRBusiness.NctsMessageStatusList.Codes.DepartureDeclarationSent);
			var headerReloaded = new BusinessObjectFactory().Load<NctsHeader>(departure.PK);
			AssertEquals(2, headerReloaded.Messages.Count);
			AssertEquals("96", headerReloaded.Messages.LastIncomingMessage.EM_MessageType);
			AssertEquals(headerReloaded.PK, headerReloaded.Messages.LastIncomingMessage.EM_LinkUniqueID);
		}

		[TestDate(2020, 09, 09, 14, 42, 0)]
		public void TestProcessReceivedMessage_CusResF97A()
		{
			var realIEF97message = frNctsReponseHelper.GetEmbeddedResourceFile("DTF97A_MESSAGE.xml");
			var departure = SetupAndRunMessageProcessor(FRBusiness.NctsMovementType.Codes.Departure, realIEF97message, NctsTransitStatusList.Codes.Unknown, FRBusiness.NctsMessageStatusList.Codes.DepartureDeclarationSent);
			var headerReloaded = new BusinessObjectFactory().Load<NctsHeader>(departure.PK);
			AssertEquals(2, headerReloaded.Messages.Count);
			AssertEquals("97", headerReloaded.Messages.LastIncomingMessage.EM_MessageType);
			AssertEquals(headerReloaded.PK, headerReloaded.Messages.LastIncomingMessage.EM_LinkUniqueID);
		}

		[TestDate(2020, 09, 09, 14, 42, 0)]
		public void TestProcessReceivedMessage_CusRes008A()
		{
			var realIE008message = frNctsReponseHelper.GetEmbeddedResourceFile("DT008A_MESSAGE.xml");
			var departure = SetupAndRunMessageProcessor(FRBusiness.NctsMovementType.Codes.Departure, realIE008message, NctsTransitStatusList.Codes.Unknown, FRBusiness.NctsMessageStatusList.Codes.DepartureDeclarationSent);
			var headerReloaded = new BusinessObjectFactory().Load<NctsHeader>(departure.PK);
			AssertEquals(2, headerReloaded.Messages.Count);
			AssertEquals("008", headerReloaded.Messages.LastIncomingMessage.EM_MessageType);
			AssertEquals(headerReloaded.PK, headerReloaded.Messages.LastIncomingMessage.EM_LinkUniqueID);
		}

		[TestDate(2020, 09, 09, 14, 42, 0)]
		public void TestProcessReceivedMessage_CusResF03A()
		{
			var realIEF03message = frNctsReponseHelper.GetEmbeddedResourceFile("DTF03A_MESSAGE.xml");
			var departure = SetupAndRunMessageProcessor(FRBusiness.NctsMovementType.Codes.Departure, realIEF03message, NctsTransitStatusList.Codes.Unknown, FRBusiness.NctsMessageStatusList.Codes.DepartureDeclarationSent);
			var headerReloaded = new BusinessObjectFactory().Load<NctsHeader>(departure.PK);
			AssertEquals(2, headerReloaded.Messages.Count);
			AssertEquals("03", headerReloaded.Messages.LastIncomingMessage.EM_MessageType);
			AssertEquals(headerReloaded.PK, headerReloaded.Messages.LastIncomingMessage.EM_LinkUniqueID);
		}

		[TestDate(2020, 09, 09, 14, 42, 0)]
		public void TestProcessReceivedMessage_CusRes025A()
		{
			var realIE025message = frNctsReponseHelper.GetEmbeddedResourceFile("DT025A_MESSAGE_TEMPLATE.xml").Replace("{IrrHEA1020}", "0");
			var departure = SetupAndRunMessageProcessor(FRBusiness.NctsMovementType.Codes.Departure, realIE025message, NctsTransitStatusList.Codes.Unknown, FRBusiness.NctsMessageStatusList.Codes.DepartureDeclarationSent);
			var headerReloaded = new BusinessObjectFactory().Load<NctsHeader>(departure.PK);
			AssertEquals(2, headerReloaded.Messages.Count);
			AssertEquals("025", headerReloaded.Messages.LastIncomingMessage.EM_MessageType);
			AssertEquals(headerReloaded.PK, headerReloaded.Messages.LastIncomingMessage.EM_LinkUniqueID);
		}

		[TestDate(2020, 09, 09, 14, 42, 0)]
		public void TestProcessReceivedMessage_CusRes043A()
		{
			var realIE043message = frNctsReponseHelper.GetEmbeddedResourceFile("DT043A_MESSAGE.xml");
			var departure = SetupAndRunMessageProcessor(FRBusiness.NctsMovementType.Codes.Departure, realIE043message, NctsTransitStatusList.Codes.Unknown, FRBusiness.NctsMessageStatusList.Codes.DepartureDeclarationSent);
			var headerReloaded = new BusinessObjectFactory().Load<NctsHeader>(departure.PK);
			AssertEquals(2, headerReloaded.Messages.Count);
			AssertEquals("043", headerReloaded.Messages.LastIncomingMessage.EM_MessageType);
			AssertEquals(headerReloaded.PK, headerReloaded.Messages.LastIncomingMessage.EM_LinkUniqueID);
		}

		public void TestProcessReceivedMessage_CusRes004A()
		{
			var realIE004message = frNctsReponseHelper.GetEmbeddedResourceFile("DT004A_MESSAGE.xml");
			var departure = SetupAndRunMessageProcessor(FRBusiness.NctsMovementType.Codes.Departure, realIE004message, NctsTransitStatusList.Codes.Unknown, FRBusiness.NctsMessageStatusList.Codes.DepartureDeclarationSent);
			var headerReloaded = new BusinessObjectFactory().Load<NctsHeader>(departure.PK);
			AssertEquals(2, headerReloaded.Messages.Count);
			AssertEquals("004", headerReloaded.Messages.LastIncomingMessage.EM_MessageType);
			AssertEquals(headerReloaded.PK, headerReloaded.Messages.LastIncomingMessage.EM_LinkUniqueID);
		}

		public FRBusiness.NctsHeader GetProcessReceivedMessageHeader(ZString message, List<CusGuaranteeHeader> guaranteeHeaderList, OrgHeader org, NctsHeader nctsHeader = null, string manualGuaranteeNumber = null)
		{
			frNctsReponseHelper = new FRNctsReponseHelper();
			var realMessage = frNctsReponseHelper.GetEmbeddedResourceFile(message);
			var departure = SetupAndRunMessageProcessor(FRBusiness.NctsMovementType.Codes.Departure, realMessage, NctsTransitStatusList.Codes.Unknown, FRBusiness.NctsMessageStatusList.Codes.DepartureDeclarationSent, true, true, guaranteeHeaderList, org, nctsHeader, manualGuaranteeNumber);
			Factory.Save();

			var headerReloaded = new BusinessObjectFactory().Load<NctsHeader>(departure.PK);
			return headerReloaded;
		}

		public FRBusiness.NctsHeader SetupAndRunMessageProcessor(ZString movementType, string incomingMessageToTest, string transitStatusForTest = "", string messageStatusForTest = "", bool shouldCreateOutgoingMessage = true, bool shouldCreateNctsHeader = true, List<CusGuaranteeHeader> guaranteeHeaderList = null, OrgHeader org = null, NctsHeader nctsHeader = null, string manualGuaranteeNumber = null)
		{
			frNctsReponseHelper = frNctsReponseHelper ?? new FRNctsReponseHelper();

			NctsHeader nctsMovement = null;
			if (shouldCreateNctsHeader)
			{
				nctsMovement = frNctsReponseHelper.SetupMessagesForTest(Factory, movementType, incomingMessageToTest, transitStatusForTest, shouldCreateOutgoingMessage, messageStatusForTest, nctsHeader: nctsHeader);
				nctsMovement.Factory.Save();
			}
			if (!branchOwningJobForTesting.IsEmpty)
			{
				nctsMovement.BH_GB = branchOwningJobForTesting;
				nctsMovement.Factory.Save();
			}

			if (guaranteeHeaderList != null)
			{
				nctsMovement.Principal.E2_OA_Address = org.MainAddress.PK;
				foreach (var guaranteeHeader in guaranteeHeaderList)
				{
					var guarantee1 = nctsMovement.GetEffectiveGuarantees().AddNew();
					guarantee1.PW_BondAmount = 150m;
					guarantee1.PW_BondNumber = guaranteeHeader.CPH_Number;
					nctsMovement.ApportionedAmountToGuaranteesLiabilityAmount();
				}
				if (manualGuaranteeNumber != null)
				{
					var cusGuarantee = nctsMovement.GetEffectiveGuarantees().AddNew();
					cusGuarantee.PW_BondNumber = manualGuaranteeNumber;
				}
				nctsMovement.Factory.Save();
			}
			serviceLogger = new TestServiceLogger();
			var processor = new FRNctsResponseMessageProcessor(serviceLogger);
			processor.ExecuteBatch();

			return nctsMovement;
		}

		TestServiceLogger serviceLogger;
		FRNctsReponseHelper frNctsReponseHelper;
		readonly ZGuid branchOwningJobForTesting;
	}
}
