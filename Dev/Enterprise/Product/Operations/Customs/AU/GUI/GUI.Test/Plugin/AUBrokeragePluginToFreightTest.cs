using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class AUBrokeragePluginToFreightTest : Customs.GUI.PlugIn.Testing.BaseBrokeragePlugInAbstractTest
	{
		[NUnit.Framework.ExpectNoExceptions]
		public void TestPlugInDisposingCorrectly_Issue00238997()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			Factory.Save();
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new Freight.Forwarding.GUI.ShipmentForm(shipment))
			{
				form.Show();
				var plugin = form.PlugIns.GetPlugIn(ZArchitecture.Modules.ControllerIDs.Customs.JobDeclaration);
				AssertEquals(typeof(AUBrokeragePluginToFreight), plugin.GetType());
				AssertNull(shipment.GetDeclaration());
				// Create Declaration in different factory
				var newFactory = new BusinessObjectFactory();
				var declaration = newFactory.New<JobDeclaration>();
				declaration.JE_JS = shipment.PK;
				newFactory.Save();
				Factory.Load<JobDeclaration>(declaration.PK);
			}
		}

		public void TestDeclarationAmendmentChecker()
		{
			var shipment = Factory.New<ForwardingShipment>();
			using (var plugIn = new TestAUBrokeragePluginToFreight(shipment))
			{
				AssertEquals("More checking and dialogs to show for au", typeof(MessagingActionsController), plugIn.GetNewMessagingActionsControllerInternal().GetType());
			}
		}

		public void TestImportJobDeclaration()
		{
			var shipment = Factory.New<ForwardingShipment>();
			using (var plugIn = new TestAUBrokeragePluginToFreight(shipment))
			{
				AssertEquals("ImportJobDeclaration for au", typeof(ImportJobDeclaration), plugIn.CreateDeclarationHelperInternal.GetNewImportJobDeclaration(shipment).GetType());
			}
		}

		public void TestOnSaveCompletedOrAborted()
		{
			JobDeclaration declaration = JobDeclaration as JobDeclaration;
			declaration.JE_DeclarationReference = "B12345678";
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = CustomsEntryStatus.AwaitingPreLodge.Code;
			EDIMessage message1 = declaration.Messages.AddNew();
			message1.EM_ReceiveTransmit = "TRX";
			EDIMessage message2 = entry.Messages.AddNew();
			message2.EM_ReceiveTransmit = "TRX";
			using (TestAUBrokeragePluginToFreight plugIn = new TestAUBrokeragePluginToFreight(Shipment))
			{
				Assert(!declaration.JE_DeclarationReference.IsEmpty);
				AssertEquals(1, declaration.Messages.Count);
				Assert(!message1.IsDeleted);
				Assert(!entry.CH_Status.IsEmpty);
				AssertEquals(1, entry.Messages.Count);
				Assert(!message2.IsDeleted);
				plugIn.OnSaveCompletedOrAborted(true);
				Assert(!declaration.JE_DeclarationReference.IsEmpty);
				AssertEquals(1, declaration.Messages.Count);
				Assert(!message1.IsDeleted);
				Assert(!entry.CH_Status.IsEmpty);
				AssertEquals(1, entry.Messages.Count);
				Assert(!message2.IsDeleted);
				plugIn.OnSaveCompletedOrAborted(false);
				Assert(declaration.JE_DeclarationReference.IsEmpty);
				AssertEquals(0, declaration.Messages.Count);
				Assert(message1.IsDeleted);
				Assert(entry.CH_Status.IsEmpty);
				AssertEquals(0, entry.Messages.Count);
				Assert(message2.IsDeleted);
			}
		}

		public void TestSynchroniseCreatesJobDeclaration()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			using (TestAUBrokeragePluginToFreight plugIn = new TestAUBrokeragePluginToFreight(shipment))
			{
				plugIn.OnGUIShown();
				var declaration = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_JS, shipment.PK));
				AssertNotNull(declaration);
			}
		}

		public void TestSyncContainerModeFromShipmentFCL()
		{
			TestSyncContainerModeFromShipment(Constants.ContainerModes.FCL, Constants.ContainerModes.Containerised);
		}

		public void TestSyncContainerModeFromShipmentLCL()
		{
			TestSyncContainerModeFromShipment(Constants.ContainerModes.LCL, Constants.ContainerModes.NonContainerised);
		}

		public void TestSyncContainerModeFromShipmentBulk()
		{
			TestSyncContainerModeFromShipment(Constants.ContainerModes.Bulk, Constants.ContainerModes.Bulk);
		}

		public void TestSyncContainerModeFromShipmentLiquid()
		{
			TestSyncContainerModeFromShipment(Constants.ContainerModes.Liquid, Constants.ContainerModes.Liquid);
		}

		public void TestSyncContainerModeFromShipmentBreakBulk()
		{
			TestSyncContainerModeFromShipment(Constants.ContainerModes.BreakBulk, Constants.ContainerModes.NonContainerised);
		}

		public void TestSynchronisePicksCorrectExportConsol()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			using (TestAUBrokeragePluginToFreight plugIn = new TestAUBrokeragePluginToFreight(shipment))
			{
				ForwardingConsol consolSingaporeUS = shipment.Consols.AddNew();
				ForwardingConsol consolAustraliaSingapore = shipment.Consols.AddNew();
				shipment.JS_RL_NKOrigin = "AUSNG";
				shipment.JS_RL_NKDestination = "USDEN";
				consolAustraliaSingapore.JK_RL_NKLoadPort = "AUSYD";
				consolAustraliaSingapore.JK_RL_NKDischargePort = "SGSIN";
				Transport transportAU_SG = consolAustraliaSingapore.Transports[0];
				transportAU_SG.JW_ETA = new ZDateTime(2004, 1, 20);
				consolSingaporeUS.JK_RL_NKLoadPort = "SGSIN";
				consolSingaporeUS.JK_RL_NKDischargePort = "USLAX";
				Transport transportSG_US = consolSingaporeUS.Transports[0];
				transportAU_SG.JW_ETA = new ZDateTime(2004, 1, 27);
				plugIn.OnUserControlShown();
				var declaration = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_JS, shipment.PK));
				AssertEquals("Port of Loading", consolAustraliaSingapore.JK_RL_NKLoadPort, declaration.JE_RL_NKPortOfLoading);
				AssertEquals("Port of Discharge", consolAustraliaSingapore.JK_RL_NKDischargePort, declaration.JE_RL_NKPortOfArrival);
			}
		}

		public void TestSynchronisePicksCorrectImportConsol()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			using (TestAUBrokeragePluginToFreight plugIn = new TestAUBrokeragePluginToFreight(shipment))
			{
				ForwardingConsol consolUSSingapore = shipment.Consols.AddNew();
				ForwardingConsol consolSingaporeAustralia = shipment.Consols.AddNew();
				shipment.JS_RL_NKOrigin = "USDEN";
				shipment.JS_RL_NKDestination = "AUSNG";
				consolUSSingapore.JK_RL_NKLoadPort = "USLAX";
				consolUSSingapore.JK_RL_NKDischargePort = "SGSIN";
				Transport transportUS_SG = consolUSSingapore.Transports[0];
				transportUS_SG.JW_ETA = new ZDateTime(2004, 01, 20);
				consolSingaporeAustralia.JK_RL_NKLoadPort = "SGSIN";
				consolSingaporeAustralia.JK_RL_NKDischargePort = "AUSYD";
				Transport transportSG_AU = consolSingaporeAustralia.Transports[0];
				transportSG_AU.JW_ETA = new ZDateTime(2004, 01, 27);
				plugIn.OnUserControlShown();
				var declaration = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_JS, shipment.PK));
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals("Port of Loading", consolSingaporeAustralia.JK_RL_NKLoadPort, declaration.JE_RL_NKPortOfLoading);
				AssertEquals("Port of Discharge", consolSingaporeAustralia.JK_RL_NKDischargePort, declaration.JE_RL_NKPortOfArrival);
			}
		}

		public void TestSynchroniseGoodsDescription()
		{
			string testGoodsDescription = "TEST DESCRIPTION OF THE GOODS";
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			using (TestAUBrokeragePluginToFreight plugIn = new TestAUBrokeragePluginToFreight(shipment))
			{
				ForwardingConsol consol = shipment.Consols.AddNew();
				Transport transport = consol.Transports[0];
				transport.JW_RL_NKLoadPort = "USLAX";
				transport.JW_RL_NKDiscPort = "AUSYD";
				shipment.JS_RL_NKOrigin = "USLAX";
				shipment.JS_RL_NKDestination = "AUSYD";
				shipment.JS_GoodsDescription = testGoodsDescription;
				plugIn.OnUserControlShown();
				JobDeclaration dec = plugIn.BusinessEntity as JobDeclaration;
				AssertEquals("Goods Description", testGoodsDescription, dec.JE_GoodsDescription);
			}
		}

		public void TestSynchroniseContainers()
		{
			const string ContainerNumber = "CRXU1234568";
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "DEFRA";
			shipment.JS_RL_NKDestination = "AUMEL";
			using (TestAUBrokeragePluginToFreight plugIn = new TestAUBrokeragePluginToFreight(shipment))
			{
				ForwardingConsol consol = shipment.Consols.AddNew();
				Transport transport = consol.Transports[0];
				consol.JK_RL_NKLoadPort = transport.JW_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = transport.JW_RL_NKDiscPort = "USLAX";
				CommonContainer container = consol.Containers.AddNew();
				PackLine packLine = shipment.OuterPackLines.AddNew();
				packLine.SetContainer(consol, container);
				container.JC_ContainerNum = ContainerNumber;
				container.JC_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20NOR").PK;
				plugIn.OnUserControlShown();
				JobDeclaration dec = plugIn.BusinessEntity as JobDeclaration;
				AssertEquals("Container Count", 1, dec.CusContainers.Count);
				AssertEquals("Container #", ContainerNumber, dec.CusContainers[0].CO_ContainerNumber);
			}
		}

		public void TestSynchroniserStopsWhenMessagesSend()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			using (TestAUBrokeragePluginToFreight plugIn = new TestAUBrokeragePluginToFreight(shipment))
			{
				ForwardingConsol consol = shipment.Consols.AddNew();
				Transport transport = consol.Transports[0];
				transport.JW_RL_NKLoadPort = "USLAX";
				transport.JW_RL_NKDiscPort = "AUSYD";
				shipment.JS_RL_NKOrigin = "USLAX";
				shipment.JS_RL_NKDestination = "AUSYD";
				Freight.Forwarding.Orders.Business.Order inv1Order = shipment.AttachedOrders.AddNew();
				inv1Order.JD_OrderNumber = "ORD NUM1";
				inv1Order.BuyerPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
				inv1Order.SupplierPK = inv1Order.BuyerPK;
				StmNote orderRefNote = shipment.Notes.AddNew();
				orderRefNote.ST_NoteText = "ORD INV1234";
				plugIn.OnUserControlShown();
				JobDeclaration dec = plugIn.BusinessEntity as JobDeclaration;
				dec.JE_MessageType = "IMP";
				JobComInvoiceHeader header = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				JobComInvoiceLine line = header.JobComInvoiceLines.AddNew();
				header.JZ_InvoiceAmount = 1000m;
				line.JI_LinePrice = 1000m;
				line.JI_Tariff = "2203.00.69 20";
				AssertEquals("JobDec origin synchronised", shipment.JS_RL_NKOrigin, dec.JE_RL_NKOrigin);
				shipment.JS_RL_NKOrigin = "USWXQ";
				AssertEquals("JobDec origin synchronised", shipment.JS_RL_NKOrigin, dec.JE_RL_NKOrigin);
				dec.CustomsEntryHeaders.AddNew();
				dec.Messages.AddNew();
				shipment.JS_RL_NKOrigin = "NZAKL";
				AssertEquals("JobDec origin synchronised", "USWXQ", dec.JE_RL_NKOrigin);
			}
		}

		public void TestWhatHappensWhenTwoPeopleCreateADeclarationAtTheSameTime()
		{
			ForwardingShipment shipment = SetupShipment("USLAX", "AUSYD");
			Factory.Save();
			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			ForwardingShipment secondFactoryShipment = secondFactory.Load<ForwardingShipment>(shipment.PK);
			JobDeclaration decFromFirstPlugIn;
			JobDeclaration decFromSecondPlugIn;
			using (TestAUBrokeragePluginToFreight plugIn1 = new TestAUBrokeragePluginToFreight(shipment))
			{
				plugIn1.OnUserControlShown();
				decFromFirstPlugIn = plugIn1.JobDeclaration;
				using (TestAUBrokeragePluginToFreight plugIn2 = new TestAUBrokeragePluginToFreight(secondFactoryShipment))
				{
					plugIn2.OnUserControlShown();
					decFromSecondPlugIn = plugIn2.JobDeclaration;
					AssertNull("Second plugin should be denied making brokerage job", decFromSecondPlugIn);
				}
			}
		}

		public void TestPluginLoadAfterFirstOneClosed()
		{
			ForwardingShipment shipment = SetupShipment("USLAX", "AUSYD");
			Factory.Save();
			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			ForwardingShipment secondFactoryShipment = secondFactory.Load<ForwardingShipment>(shipment.PK);
			JobDeclaration decFromFirstPlugIn;
			JobDeclaration decFromSecondPlugIn;
			using (TestAUBrokeragePluginToFreight plugIn1 = new TestAUBrokeragePluginToFreight(shipment))
			{
				plugIn1.OnUserControlShown();
				decFromFirstPlugIn = plugIn1.JobDeclaration;
				using (TestAUBrokeragePluginToFreight plugIn2 = new TestAUBrokeragePluginToFreight(secondFactoryShipment))
				{
					plugIn2.OnUserControlShown();
					decFromSecondPlugIn = plugIn2.JobDeclaration;
					AssertNull("Second plugin should be denied making brokerage job", decFromSecondPlugIn);
				}

				Factory.Save();
			}

			using (TestAUBrokeragePluginToFreight plugIn2 = new TestAUBrokeragePluginToFreight(secondFactoryShipment))
			{
				plugIn2.OnUserControlShown();
				decFromSecondPlugIn = plugIn2.JobDeclaration;
			}

			AssertNotNull("Second plugin should now be allowed to get brokerage job", decFromSecondPlugIn);
			AssertEquals("Dec1 and dec2 should be the same declaration", decFromFirstPlugIn.PK, decFromSecondPlugIn.PK);
		}

		public void TestETAAndETDGetSynchronised()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ForwardingConsol consol = shipment.Consols.AddNew();
			shipment.JS_E_DEP = new ZDateTime(2004, 6, 8);
			shipment.JS_E_ARV = new ZDateTime(2004, 6, 10);
			using (TestAUBrokeragePluginToFreight plugIn1 = new TestAUBrokeragePluginToFreight(shipment))
			{
				plugIn1.OnUserControlShown();
				JobDeclaration dec1 = plugIn1.JobDeclaration;
				AssertEquals("ETD", shipment.JS_E_DEP, dec1.JE_DateAtOrigin);
				AssertEquals("ETA", shipment.JS_E_ARV, dec1.JE_DateAtFinalDestination);
			}
		}

		public void TestQueryUserToCreateDeclaration()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			using (TestAUBrokeragePluginToFreight plugIn1 = new TestAUBrokeragePluginToFreight(shipment))
			{
				plugIn1.WillUserQueryYesToCreateDeclaration = false;
				plugIn1.OnUserControlShown();
				AssertEquals("User answers no", null, plugIn1.JobDeclaration);
			}

			using (TestAUBrokeragePluginToFreight plugIn1 = new TestAUBrokeragePluginToFreight(shipment))
			{
				plugIn1.WillUserQueryYesToCreateDeclaration = true;
				plugIn1.OnGUIShown();
				AssertNotNull("JobDeclaration should now be created", plugIn1.JobDeclaration);
			}

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (TestAUBrokeragePluginToFreight plugIn1 = new TestAUBrokeragePluginToFreight(shipment))
			{
				plugIn1.QueryUserToCreateDeclarationCalled = false;
				plugIn1.OnGUIShown();
				AssertNotNull("JobDeclaration should now be accessed", plugIn1.JobDeclaration);
				AssertEquals("QueryUserToCreateDeclaration should not be called anymore", false, plugIn1.QueryUserToCreateDeclarationCalled);
			}
		}

		public void TestStartPlugInSynchroniserCalledInConstructor()
		{
			using (TestAUBrokeragePluginToFreight plugIn = new TestAUBrokeragePluginToFreight(SetupShipment("AUSYD", "USLAX")))
			{
				AssertEquals("Synchroniser must be called in the constructor as synchronising must occur even though the brokerage tab isn't visible", true, plugIn.StartPlugInSynchroniserCalled);
			}
		}

		protected override Customs.Business.BaseJobDeclaration GetDeclaration()
		{
			var declaration = base.GetDeclaration();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			return declaration;
		}

		protected override ZPlugIn GetPlugInToTest() => new AUBrokeragePluginToFreight(Shipment);

		protected override void MakeThisDeclarationPackingRelevant(Customs.Business.BaseJobDeclaration declaration)
		{
			base.MakeThisDeclarationPackingRelevant(declaration);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		}

		void TestSyncContainerModeFromShipment(ZString packingMode, ZString expectedContainerMode)
		{
			ForwardingShipment shipment = SetupShipment("AUSYD", "USLAX");
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = packingMode;
			using (TestAUBrokeragePluginToFreight plugIn = new TestAUBrokeragePluginToFreight(shipment))
			{
				plugIn.OnGUIShown();
				var declaration = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_JS, shipment.PK));
				AssertNotNull(declaration);
				AssertEquals("ContainerMode", expectedContainerMode, declaration.JE_ContainerMode);
			}
		}

		ForwardingShipment SetupShipment(ZString origin, ZString destination)
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ForwardingConsol consol = shipment.Consols.AddNew();
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = origin;
			transport.JW_RL_NKDiscPort = destination;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			shipment.JS_INCO = "EXW";
			return shipment;
		}

		sealed class TestAUBrokeragePluginToFreight : AUBrokeragePluginToFreight
		{
			internal TestAUBrokeragePluginToFreight(ForwardingShipment shipment) : base(shipment)
			{
			}

			internal bool WillUserQueryYesToCreateDeclaration = true;

			internal bool QueryUserToCreateDeclarationCalled;

			internal bool StartPlugInSynchroniserCalled;

			internal bool OnDocumentRequestedWithNoCusEntryHeadersCalled;

			internal Customs.GUI.SendsMessagesToCustomsGUI GetNewMessagingActionsControllerInternal() => GetNewMessagingActionsController();

			internal CreateDeclarationHelper CreateDeclarationHelperInternal => (CreateDeclarationHelper)CreateDeclarationHelper;

			internal new JobDeclaration JobDeclaration => base.JobDeclaration;

			protected override void StartPlugInSynchroniser()
			{
				base.StartPlugInSynchroniser();
				StartPlugInSynchroniserCalled = true;
			}

			protected override bool QueryUser(ForwardingShipment shipment, ICollection<Customs.Business.CreateBrokerageQuestion> questions, Customs.Business.CreateDeclarationHelper.QuestionType type)
			{
				if (type != Customs.Business.CreateDeclarationHelper.QuestionType.ImportDeclarationQuery)
				{
					QueryUserToCreateDeclarationCalled = true;
					return WillUserQueryYesToCreateDeclaration;
				}

				return base.QueryUser(shipment, questions, type);
			}

			protected override void OnDocumentRequestedWithNoCusEntryHeaders(object sender, EventArgs e)
			{
				OnDocumentRequestedWithNoCusEntryHeadersCalled = true;
			}
		}
	}
}
