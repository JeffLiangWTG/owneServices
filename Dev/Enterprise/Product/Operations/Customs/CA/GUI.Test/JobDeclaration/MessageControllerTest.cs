using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class MessageControllerTest : Customs.GUI.Testing.AmendmentDetectionOnSavingWithBackDoorSupporterTest
	{
		[NUnit.Framework.TestDate(2008, 01, 17)]
		public override void TestContinueWithSaveWhenUsersChooseToSendMessages()
		{
			base.TestContinueWithSaveWhenUsersChooseToSendMessages();
		}

		[NUnit.Framework.TestDate(2008, 01, 17)]
		public override void TestContinueWithSaveForSavingOptionsIsCancelled()
		{
			base.TestContinueWithSaveForSavingOptionsIsCancelled();
		}

		[NUnit.Framework.TestDate(2008, 01, 17)]
		public override void TestContinueWithSaveForSavingWithoutSending()
		{
			base.TestContinueWithSaveForSavingWithoutSending();
		}

		public override void TestContinueWithSaveWhenThereAreErrorsNotificationsFromManager()
		{
			Assert(true);//there are no error conditions from message managers as of now (18-Oct-2007)
		}

		[NUnit.Framework.TestDate(2008, 01, 17)]
		protected override IMessageManageableBizObj GetBizObjWithMessagesToSendButMessageErrors()
		{
			JobDeclaration declaration = GetAmendableDeclaration();
			declaration.JE_TransportMode = ZString.Empty;
			AssertHasMessageErrors(declaration.JE_TransportModeInfo);
			declaration.Transports.RemoveAndDeleteAll(); // To Stop Red Error being thrown.
			return declaration;
		}

		[NUnit.Framework.TestDate(2008, 01, 17)]
		protected override IBackDoorSavingSupportableBizObj GetBizObjWithMessagesToSendButWithCancelledSavingOptions()
		{
			var jobDeclaration = GetAmendableJobDeclaration();
			var declaration = (JobDeclaration)jobDeclaration;

			jobDeclaration.GetMessageManagerForAmendmentDetectionReturns = null;

			var actions = new CAMessageSendingActionCollection(declaration, MessageSendingMessageType.Amendment);
			actions.IsCancelled = true;
			var messageManager = new JobDeclarationMessageManager(declaration, actions);

			jobDeclaration.GetMessageManagerForAmendmentDetectionReturns = messageManager;
			declaration.Invoices[0].JZ_InvoiceNumber = "INV122131";
			return declaration;
		}

		[NUnit.Framework.TestDate(2008, 01, 17)]
		protected override IMessageManageableBizObj GetBizObjWithMessagesToSendButWithErrorsFromMessageManager()
		{
			var declaration = GetAmendableDeclaration();
			declaration.CustomsEntryHeaders[0].CH_Status = MessageStatusList.Codes.Sent;
			return declaration;
		}

		[NUnit.Framework.TestDate(2008, 01, 17)]
		protected override IBackDoorSavingSupportableBizObj GetBizObjWithMessagesToSendButWithSaveWithoutSendingOptions()
		{
			var jobDeclaration = GetAmendableJobDeclaration();
			var declaration = (JobDeclaration)jobDeclaration;

			jobDeclaration.GetMessageManagerForAmendmentDetectionReturns = null;

			var mockActions = new Mock<CAMessageSendingActionCollection>(declaration, MessageSendingMessageType.Amendment) { CallBase = true };
			mockActions.Setup(m => m.IsCancelled).Returns(false);

			var actions = mockActions.Object;
			actions[0].CA_SaveWithoutSending = true;
			var messageManager = new JobDeclarationMessageManager(declaration, actions);

			jobDeclaration.GetMessageManagerForAmendmentDetectionReturns = messageManager;
			declaration.Invoices[0].JZ_InvoiceNumber = "INV122131";

			return declaration;
		}

		protected override IMessageManageableBizObj GetBizObjWithMessagesToSendButWithWarningsFromMessageManager() => GetBizObjWithMessagesToSendButMessageErrors();

		protected override IMessageManageableBizObj GetBizObjWithoutAnyMessagesToSend() => GetAmendableDeclaration();

		protected override IShowPreSaveDialog GetFormOrPlugInToWhichAmendmentDetectionIsHookedUp(IMessageManageableBizObj bizObj) => new JobDeclarationForm((JobDeclaration)bizObj);

		[NUnit.Framework.TestDate(2008, 01, 17)]
		JobDeclarationForTesting GetUnMergedDeclaration()
		{
			var helper = new Business.Testing.DeclarationTestHelper(Factory, false);
			int year = checked(ZDateTime.Now.Year - 1);
			OrgHeader orgHeader = OrgHeader.LoadFromCode(Factory, "ABIGAS");
			OrgHeader orgHeader2 = OrgHeader.LoadFromCode(Factory, "ABABEU");
			OrgHeader orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader3.OH_IsShippingLine = true;
			orgHeader3.OH_IsShippingProvider = true;
			orgHeader2.PrimaryRegistrationNumber.Number = "90093519530";
			orgHeader.SetLocalCustomsCode("CID", "955411R");
			var result = Factory.New<JobDeclarationForTesting>();
			BaseJobDeclaration baseJobDeclaration = result;
			baseJobDeclaration.JE_OH_Importer = orgHeader2.PK;
			baseJobDeclaration.JE_MessageType = "IMP";
			baseJobDeclaration.JE_TransportMode = "SEA";
			baseJobDeclaration.JE_VesselName = "ADMIRALENGRACHT";
			baseJobDeclaration.JE_VoyageFlightNo = "92";
			baseJobDeclaration.JE_OH_ShippingLine = orgHeader3.PK;
			baseJobDeclaration.JE_ContainerCount = (short)1;
			baseJobDeclaration.JE_ContainerMode = "FCL";
			baseJobDeclaration.JE_DateAtFinalDestination = new ZDateTime(year, 1, 17);
			baseJobDeclaration.JE_DateAtOrigin = new ZDateTime(year, 1, 3);
			baseJobDeclaration.JE_DateOfArrival = new ZDateTime(year, 1, 17);
			baseJobDeclaration.JE_DateOfFirstArrival = new ZDateTime(year, 1, 17);
			baseJobDeclaration.JE_DeclarationReference = "B00001001";
			baseJobDeclaration.JE_ExportDate = new ZDateTime(year, 1, 3);
			baseJobDeclaration.JE_ExportGoodsType = "OT";
			baseJobDeclaration.JE_GB = GlbBranch.CurrentBranch.PK;
			baseJobDeclaration.JE_GoodsDescription = "STATIONERY";
			baseJobDeclaration.JE_HouseBill = "HBL9387439";
			baseJobDeclaration.JE_MasterBill = "OBL93874394";
			baseJobDeclaration.JE_MergeBy = "NON";
			baseJobDeclaration.JE_MessageSubType = "FRM";
			baseJobDeclaration.JE_OwnerRef = "PO038349";
			baseJobDeclaration.JE_PaymentMethod = "BRK";
			baseJobDeclaration.JE_RL_NKFinalDestination = "AUSYD";
			baseJobDeclaration.JE_RL_NKOrigin = "HKHKG";
			baseJobDeclaration.JE_RL_NKPortOfArrival = "AUSYD";
			baseJobDeclaration.JE_RL_NKPortOfFirstArrival = "AUSYD";
			baseJobDeclaration.JE_RL_NKPortOfLoading = "HKHKG";
			baseJobDeclaration.JE_ShipmentIncoTerm = "FOB";
			baseJobDeclaration.JE_TotalNoOfPacks = 5;
			baseJobDeclaration.JE_TotalNoOfPacksPackType = "PKG";
			baseJobDeclaration.JE_TotalVolumeUnit = "M3";
			baseJobDeclaration.JE_TotalWeight = 10.000m;
			baseJobDeclaration.JE_TotalWeightUnit = "KG";
			baseJobDeclaration.JobComInvoiceGroupHeaders[0].JZ_InvoiceNumber = "All Invoices";
			baseJobDeclaration.JobComInvoiceGroupHeaders[0].Charges.AddNew("OFT", 1000m, baseJobDeclaration.LocalCurrencyCode);
			baseJobDeclaration.JobComInvoiceGroupHeaders[0].Charges.AddNew("ONS", 20m, baseJobDeclaration.LocalCurrencyCode);
			BaseCusContainer baseCusContainer = baseJobDeclaration.CusContainers.AddNew();
			baseCusContainer.CO_ContainerNumber = "MOLU9387391";
			baseCusContainer.CO_FCL_LCL_AIR = "FCL";
			baseCusContainer.CO_RC = base.Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20GP").PK;
			BaseJobComInvoiceHeader baseJobComInvoiceHeader = baseJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			baseJobComInvoiceHeader.JZ_IncoTerm = "FOB";
			baseJobComInvoiceHeader.JZ_InvoiceAmount = 12500.0000m;
			baseJobComInvoiceHeader.JZ_InvoiceCurrExRate = 1.000000000m;
			baseJobComInvoiceHeader.JZ_InvoiceCurrLandedCostExRate = 1.000000000m;
			baseJobComInvoiceHeader.JZ_InvoiceDate = new ZDateTime(year, 1, 13);
			baseJobComInvoiceHeader.JZ_InvoiceNumber = "1";
			baseJobComInvoiceHeader.JZ_PaymentExRate = 1.000000000m;
			baseJobComInvoiceHeader.JZ_RX_NKInvoice_Currency = "AUD";
			baseJobComInvoiceHeader.JZ_Weight = 10.000m;
			baseJobComInvoiceHeader.JZ_WeightUQ = "KG";
			baseJobComInvoiceHeader.JZ_OH_Buyer = orgHeader.PK;
			BaseJobComInvoiceLine baseJobComInvoiceLine = baseJobComInvoiceHeader.JobComInvoiceLines.AddNew();
			baseJobComInvoiceLine.JI_LineNo = (short)1;
			baseJobComInvoiceLine.JI_LinePrice = 2500.0000m;
			baseJobComInvoiceLine.JI_WeightUQ = "KG";
			BaseJobComInvoiceLine baseJobComInvoiceLine2 = baseJobComInvoiceHeader.JobComInvoiceLines.AddNew();
			baseJobComInvoiceLine2.JI_LineNo = (short)2;
			baseJobComInvoiceLine2.JI_LinePrice = 10000.0000m;
			baseJobComInvoiceLine2.JI_WeightUQ = "KG";
			baseJobDeclaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			var declaration = (JobDeclaration)result;
			declaration.JE_MessageType = Enterprise.Customs.CA.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = ZString.Empty;
			declaration.JE_TransportMode = Enterprise.Customs.CA.Business.TransportTypeList.Codes.Sea;
			declaration.JE_DateAtFinalDestination = new ZDateTime(2008, 1, 17);
			declaration.JE_DateAtOrigin = new ZDateTime(2008, 1, 3);
			declaration.JE_DateOfArrival = new ZDateTime(2008, 1, 17);
			declaration.JE_DateOfFirstArrival = new ZDateTime(2008, 1, 17);
			declaration.JE_ExportDate = new ZDateTime(2008, 1, 3);
			declaration.JE_RL_NKOrigin = helper.CATOR.Code;
			declaration.JE_RL_NKPortOfLoading = helper.CAVAR.Code;
			declaration.JE_RL_NKFinalDestination = helper.AUSYD.Code;
			declaration.JE_RL_NKPortOfArrival = helper.AUMEL.Code;
			declaration.JE_RL_NKPortOfFirstArrival = helper.AUMEL.Code;

			declaration.Invoices[0].Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1000m, declaration.LocalCurrencyCode);
			declaration.Invoices[0].Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 20m, declaration.LocalCurrencyCode);

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines[0];
			invoiceLine1.JI_Description = "BALL POINT PENS";
			invoiceLine1.JI_Tariff = "9608100000";
			invoiceLine1.JI_CustomsQuantity = 10000.0000m;
			invoiceLine1.JI_CustomsUnitQty = "KGM";
			invoiceLine1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1000m, declaration.LocalCurrencyCode);

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines[1];
			invoiceLine2.JI_Description = "IN THE FORM OF BOOKLETS";
			invoiceLine2.JI_Tariff = "4813100000";
			invoiceLine2.JI_CustomsQuantity = 200000.0000m;
			invoiceLine2.JI_CustomsUnitQty = "KGM";
			invoiceLine2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 20m, declaration.LocalCurrencyCode);
			return result;
		}

		protected override Type GetTypeOfDeclarationToMock() => typeof(JobDeclaration);

		protected override void SetUp()
		{
			base.SetUp();
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
		}

		JobDeclarationForTesting GetAmendableJobDeclaration()
		{
			var result = GetUnMergedDeclaration();
			var declaration = (JobDeclaration)result;

			declaration.DoMerge();

			Business.CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			EDIMessage message = entry.Messages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = MessageTypeList.Codes.DataLoadingModule;
			Factory.Save();

			entry.CH_Status = MessageStatusList.Codes.Sent;
			Factory.Save(); // force system to add a log

			var mockActions = new Mock<CAMessageSendingActionCollection>(declaration, MessageSendingMessageType.Amendment) { CallBase = true };
			mockActions.Setup(m => m.IsCancelled).Returns(false);

			CAMessageSendingActionCollection actions = mockActions.Object;
			actions[0].CA_SendMessage = true;
			var messageManager = new JobDeclarationMessageManager(declaration, actions);

			result.GetMessageManagerForAmendmentDetectionReturns = messageManager;

			return result;
		}

		[NUnit.Framework.TestDate(2008, 01, 17)]
		JobDeclaration GetAmendableDeclaration()
		{
			var jobDeclaration = GetAmendableJobDeclaration();
			return jobDeclaration;
		}

		sealed class JobDeclarationForTesting : JobDeclaration
		{
			public JobDeclarationForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public JobDeclarationMessageManager GetMessageManagerForAmendmentDetectionReturns { get; set; }

			protected override JobDeclarationMessageManager GetMessageManagerForAmendmentDetection() => GetMessageManagerForAmendmentDetectionReturns;
		}
	}
}
