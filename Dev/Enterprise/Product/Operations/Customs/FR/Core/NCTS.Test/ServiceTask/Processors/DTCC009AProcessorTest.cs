using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC009A;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using CusTempStorageRegHeader = Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageRegHeader;
using CusTempStorageRegLine = Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageRegLine;

namespace Enterprise.Customs.FR.NCTS.ServiceTask.Testing
{
	sealed class DTCC009AProcessorTest : DTBaseProcessorTest<Cc009AType>
	{
		public void TestTemporaryStoreRolledBack_CanIniByCusHEA94()
		{
			var nctsHeader = SetupStorage();
			CusTempStorageRegHeader reg1Reloaded, reg2Reloaded;
			EU.TemporaryStorage.Business.CusTempStorageRegLine regLine1Reloaded, regLine2Reloaded;

			var realMessage = frNctsReponseHelper.GetEmbeddedResourceFile("DT009A_MESSAGE_TEMPLATE.xml").Replace("{CanIniByCusHEA94}", "1").Replace("{CanDecHEA93}", "0");
			var helper = new FRNctsResponseProcessingTests();
			helper.SetupAndRunMessageProcessor(EU.NCTS.Business.NctsMovementType.Codes.Departure, realMessage, EU.NCTS.Business.NctsTransitStatusList.Codes.Unknown, EU.NCTS.Business.NctsMessageStatusList.Codes.DepartureDeclarationSent, true, true, null, null, nctsHeader);
			Factory.Save();

			reg1Reloaded = new BusinessObjectFactory().LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, "FRJ_IST1"));
			regLine1Reloaded = reg1Reloaded.CusTempStorageRegLines.Cast<CusTempStorageRegLine>().Single();
			AssertEquals("The register should be refilled with the departure declaration packages when the declaration is cancelled", 100, regLine1Reloaded.SRL_PackagesRemaining);

			reg2Reloaded = new BusinessObjectFactory().LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, "FRJ_IST2"));
			regLine2Reloaded = reg2Reloaded.CusTempStorageRegLines.Cast<CusTempStorageRegLine>().Single();
			AssertEquals("The register should be refilled with the departure declaration packages when the declaration is cancelled", 200, regLine2Reloaded.SRL_PackagesRemaining);

			var mails = Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals("No error emails should be sent", 0, mails.Count);
		}

		public void TestTemporaryStoreRolledBack_CanDecHEA93()
		{
			var nctsHeader = SetupStorage();
			CusTempStorageRegHeader reg1Reloaded, reg2Reloaded;
			EU.TemporaryStorage.Business.CusTempStorageRegLine regLine1Reloaded, regLine2Reloaded;

			var realMessage = frNctsReponseHelper.GetEmbeddedResourceFile("DT009A_MESSAGE_TEMPLATE.xml").Replace("{CanIniByCusHEA94}", "0").Replace("{CanDecHEA93}", "1");
			var helper = new FRNctsResponseProcessingTests();
			helper.SetupAndRunMessageProcessor(EU.NCTS.Business.NctsMovementType.Codes.Departure, realMessage, EU.NCTS.Business.NctsTransitStatusList.Codes.Unknown, EU.NCTS.Business.NctsMessageStatusList.Codes.DepartureDeclarationSent, true, true, null, null, nctsHeader);
			Factory.Save();

			reg1Reloaded = new BusinessObjectFactory().LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, "FRJ_IST1"));
			regLine1Reloaded = reg1Reloaded.CusTempStorageRegLines.Cast<CusTempStorageRegLine>().Single();
			AssertEquals("The register should be refilled with the departure declaration packages when the declaration is cancelled", 100, regLine1Reloaded.SRL_PackagesRemaining);

			reg2Reloaded = new BusinessObjectFactory().LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, "FRJ_IST2"));
			regLine2Reloaded = reg2Reloaded.CusTempStorageRegLines.Cast<CusTempStorageRegLine>().Single();
			AssertEquals("The register should be refilled with the departure declaration packages when the declaration is cancelled", 200, regLine2Reloaded.SRL_PackagesRemaining);

			var mails = Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals("No error emails should be sent", 0, mails.Count);
		}

		public void TestTemporaryStoreRolledBack_NeitherCanIniByCusHEA94_NorCanDecHEA93()
		{
			var nctsHeader = SetupStorage();
			CusTempStorageRegHeader reg1Reloaded, reg2Reloaded;
			EU.TemporaryStorage.Business.CusTempStorageRegLine regLine1Reloaded, regLine2Reloaded;

			var realMessage = frNctsReponseHelper.GetEmbeddedResourceFile("DT009A_MESSAGE_TEMPLATE.xml").Replace("{CanIniByCusHEA94}", "0").Replace("{CanDecHEA93}", "0");
			var helper = new FRNctsResponseProcessingTests();
			helper.SetupAndRunMessageProcessor(EU.NCTS.Business.NctsMovementType.Codes.Departure, realMessage, EU.NCTS.Business.NctsTransitStatusList.Codes.Unknown, EU.NCTS.Business.NctsMessageStatusList.Codes.DepartureDeclarationSent, true, true, null, null, nctsHeader);
			Factory.Save();

			reg1Reloaded = new BusinessObjectFactory().LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, "FRJ_IST1"));
			regLine1Reloaded = reg1Reloaded.CusTempStorageRegLines.Cast<CusTempStorageRegLine>().Single();
			AssertEquals("The number of remaining packages should remain as it was since no cancellation occurs", 20, regLine1Reloaded.SRL_PackagesRemaining);

			reg2Reloaded = new BusinessObjectFactory().LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, "FRJ_IST2"));
			regLine2Reloaded = reg2Reloaded.CusTempStorageRegLines.Cast<CusTempStorageRegLine>().Single();
			AssertEquals("The number of remaining packages should remain as it was since no cancellation occurs", 130, regLine2Reloaded.SRL_PackagesRemaining);

			var mails = Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals("No error emails should be sent", 0, mails.Count);
		}

		NctsHeader SetupStorage()
		{
			var ist1 = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist1.SJH_JobReference = "FRJ_IST1";
			ist1.DDTNumber = "DDT1";
			ist1.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;

			var register1 = Factory.New<CusTempStorageRegHeader>();
			register1.SRH_Reference = "DDT1";
			register1.SRH_InternalReference = "FRJ_IST1";
			var regLine1 = register1.CusTempStorageRegLines.AddNew();
			regLine1.SRL_LineNumber = 1;

			var transaction1A = regLine1.CusTempStorageRegLineTransactions.AddNew();
			transaction1A.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			transaction1A.SRT_PackageQty = 100;

			var ist2 = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist2.DDTNumber = "DDT2";
			ist2.SJH_JobReference = "FRJ_IST2";
			ist2.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;

			var register2 = Factory.New<CusTempStorageRegHeader>();
			register2.SRH_Reference = "DDT2";
			register2.SRH_InternalReference = "FRJ_IST2";
			var regLine2 = register2.CusTempStorageRegLines.AddNew();
			regLine2.SRL_LineNumber = 1;

			var transaction2A = regLine2.CusTempStorageRegLineTransactions.AddNew();
			transaction2A.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			transaction2A.SRT_PackageQty = 200;

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.BH_JobReference = "NCT00050167";

			var goods1 = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goods1.BY_GrossWeight = 20m;
			var pack11 = goods1.Packages.AddNew();
			pack11.B5_UnitCount = 50;
			var pack12 = goods1.Packages.AddNew();
			pack12.B5_UnitCount = 30;

			var pd1 = goods1.PreviousDocuments.AddNew();
			pd1.CSI_Code = PreviousDocumentCodeList.Codes._337;
			pd1.CSI_ReferenceNumber = "FRJ_IST1";
			pd1.CSI_LineNo = 1;

			var goods2 = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goods2.BY_GrossWeight = 40m;
			var pack21 = goods2.Packages.AddNew();
			pack21.B5_UnitCount = 70;

			var pd2 = goods2.PreviousDocuments.AddNew();
			pd2.CSI_Code = PreviousDocumentCodeList.Codes._337;
			pd2.CSI_ReferenceNumber = "FRJ_IST2";
			pd2.CSI_LineNo = 1;

			var logger = new LoggingInformation();

			foreach (var transactionData in goods1.TemporaryStorageRegisterTransactionDataProvider.GetTemporaryStorageRegisterTransactionData())
			{
				register1.AddNewRegisterTransaction(logger, transactionData.RegisterLineNo, transactionData.CustomsReferenceNumber, transactionData.ReferenceType, transactionData.InternalReferenceNumber, transactionData.InternalReferenceType, transactionData.GrossMass, 0 - transactionData.PackageQuantity, transactionData.Comments);
			}

			foreach (var transactionData in goods2.TemporaryStorageRegisterTransactionDataProvider.GetTemporaryStorageRegisterTransactionData())
			{
				register2.AddNewRegisterTransaction(logger, transactionData.RegisterLineNo, transactionData.CustomsReferenceNumber, transactionData.ReferenceType, transactionData.InternalReferenceNumber, transactionData.InternalReferenceType, transactionData.GrossMass, 0 - transactionData.PackageQuantity, transactionData.Comments);
			}

			Factory.Save();

			var reg1Reloaded = new BusinessObjectFactory().LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, "FRJ_IST1"));
			var regLine1Reloaded = reg1Reloaded.CusTempStorageRegLines.Single();
			AssertEquals("The number of remaining packages should be according to the deducted ones", 20, regLine1Reloaded.SRL_PackagesRemaining);

			var reg2Reloaded = new BusinessObjectFactory().LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, "FRJ_IST2"));
			var regLine2Reloaded = reg2Reloaded.CusTempStorageRegLines.Single();
			AssertEquals("The number of remaining packages should be according to the deducted ones", 130, regLine2Reloaded.SRL_PackagesRemaining);

			return nctsHeader;
		}

		protected override IEnumerable<DTMessageProcessorTestCase> DTMessageProcessorTestCases
		{
			get
			{
				yield return new DTMessageProcessorTestCase
				{
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DT009A_MESSAGE_TEMPLATE.xml").Replace("{CanIniByCusHEA94}", "0").Replace("{CanDecHEA93}", "0"),
					ExpectedNewMessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.Ok,
					ExpectedNewDepartureStatus = FR.Business.NctsTransitStatusList.Codes.DeclarationRejected,
					ExpectedNewDetailedDepartureStatus = NctsDetailedStatusList.Codes.CancellationRefused,
					ExpectedNewMessageInterpretation = @"
<p>New departure status: Declaration Rejected</p>
<p>New detailed departure status: Cancellation Refused</p>
<p>Status granted on: 11/04/2019 11:28</p>"
				};

				yield return new DTMessageProcessorTestCase
				{
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DT009A_MESSAGE_TEMPLATE.xml").Replace("{CanIniByCusHEA94}", "1").Replace("{CanDecHEA93}", "1"),
					ExpectedNewMessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.Ok,
					ExpectedNewDepartureStatus = FR.Business.NctsTransitStatusList.Codes.DeclarationCancelled,
					ExpectedNewDetailedDepartureStatus = NctsDetailedStatusList.Codes.CancellationAccepted,
					ExpectedNewMessageInterpretation = @"
<p>New departure status: Declaration Canceled</p>
<p>New detailed departure status: Cancellation Accepted</p>
<p>Status granted on: 11/04/2019 11:28</p>"
				};

				yield return new DTMessageProcessorTestCase
				{
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DT009A_MESSAGE_TEMPLATE.xml").Replace("{CanIniByCusHEA94}", "0").Replace("{CanDecHEA93}", "1"),
					ExpectedNewMessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.Ok,
					ExpectedNewDepartureStatus = FR.Business.NctsTransitStatusList.Codes.DeclarationCancelled,
					ExpectedNewDetailedDepartureStatus = NctsDetailedStatusList.Codes.CancellationAccepted,
					ExpectedNewMessageInterpretation = @"
<p>New departure status: Declaration Canceled</p>
<p>New detailed departure status: Cancellation Accepted</p>
<p>Status granted on: 11/04/2019 11:28</p>"
				};

				yield return new DTMessageProcessorTestCase
				{
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DT009A_MESSAGE_TEMPLATE.xml").Replace("{CanIniByCusHEA94}", "1").Replace("{CanDecHEA93}", "0"),
					ExpectedNewMessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.Ok,
					ExpectedNewDepartureStatus = FR.Business.NctsTransitStatusList.Codes.DeclarationCancelled,
					ExpectedNewDetailedDepartureStatus = NctsDetailedStatusList.Codes.CancellationAccepted,
					ExpectedNewMessageInterpretation = @"
<p>New departure status: Declaration Canceled</p>
<p>New detailed departure status: Cancellation Accepted</p>
<p>Status granted on: 11/04/2019 11:28</p>"
				};
			}
		}

		protected override ZString initialDeclarationStatus => FR.Business.NctsTransitStatusList.Codes.DeclarationRejected;
	}
}
