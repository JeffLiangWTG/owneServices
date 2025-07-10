using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Moq;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusUnderbondUBMREQManagerTest : CMRMessageManagerAbstractTest
	{
		public void TestRequiresAmendment()
		{
			var masterAwb = Factory.NewWithValidTestData<CusMAWB>();
			var houseAwb = masterAwb.ChildBills.AddNew();
			houseAwb.FillWithValidTestData();

			var cusUnderbond = houseAwb.Underbonds.AddNew();
			cusUnderbond.FillWithValidTestData();
			cusUnderbond.UnderbondStatus.Code = CMRBaseStatuses.Codes.AmendmentAccepted;

			houseAwb.CS_CM = ZGuid.Empty;

			Factory.Save();

			var underbondInNewFactory = NewFactory().Load<CusUnderbond>(cusUnderbond.PK);

			var houseAwbInNewFacotry = (CusHAWB)underbondInNewFactory.LinkedObject;
			AssertNull("Precodition", houseAwbInNewFacotry.MAWB);

			houseAwbInNewFacotry.CS_CM = masterAwb.PK;
			AssertNotNull("Should not be null as the CS_CM is valid.", houseAwbInNewFacotry.MAWB);

			var manager = new CusUnderbondUBMREQManager(underbondInNewFactory);
			Assert("Precodition", manager.HasActiveMessages);
			Assert("Should be false as the CS_CM is changed from an empty value.", manager.RequiresAmendment());
		}

		public void TestBusinessObject()
		{
			AssertEquals(Underbond, Manager.BusinessObject);
		}

		public void TestDelayMethod()
		{
			var underbond = Factory.New<CusUnderbond>();
			var manager = new CusUnderbondUBMREQManager(underbond);
			manager.DelayAction(false);
			AssertEquals("", underbond.C4_Status);
			manager.DelayAction(true);
			AssertEquals(CMRUnderbondStatuses.Codes.UnderbondSendingDelayed, underbond.C4_Status);
		}

		public void TestQueriesForSending()
		{
			var mawb = Factory.New<CusMAWB>();
			var underbond = mawb.Underbonds.AddNew();
			mawb.AllUnderbonds.Load();
			underbond.LinkedObject = mawb;
			var manager = new CusUnderbondUBMREQManager(underbond);
			var collection = manager.GetQueriesForSending();
			var question = "";
			var caption = "";
			foreach (Customs.Business.MessageSendingQuery query in collection)
			{
				question = query.Question;
				caption = query.Caption;
				query.Delegate(true);
			}
			AssertEquals("No CARST messages were found to permit the acceptance of this underbond. \r\n\r\nDo you want to delay sending it and have CargoWise One send it automatically when a CARST arrives?", question);
			AssertEquals("Delay sending of Underbond - " + underbond.C4_SendersMessageReference + "?", caption);
			AssertEquals(CMRUnderbondStatuses.Codes.UnderbondSendingDelayed, underbond.C4_Status);
			AssertEquals(true, manager.PreventSend);
		}

		public void TestSCAQueriesForSending()
		{
			var cmrOceanBill = Factory.New<CusSCAOceanBill>();
			var container = cmrOceanBill.Containers.AddNew();
			var house = cmrOceanBill.HouseBills.AddNew();
			var pivot = container.Pivots.AddNew();
			var underbond = container.Underbonds.AddNew();
			cmrOceanBill.AllUnderbonds.Load();

			var manager = new CusUnderbondUBMREQManager(underbond);
			var collection = manager.GetQueriesForSending();
			var question = "";
			var caption = "";
			foreach (Customs.Business.MessageSendingQuery query in collection)
			{
				question = query.Question;
				caption = query.Caption;
				query.Delegate(true);
			}

			AssertEquals("No CARST messages were found to permit the acceptance of this underbond. \r\n\r\nDo you want to delay sending it and have CargoWise One send it automatically when a CARST arrives?", question);
			AssertEquals("Delay sending of Underbond - " + underbond.C4_SendersMessageReference + "?", caption);
			AssertEquals(CMRUnderbondStatuses.Codes.UnderbondSendingDelayed, underbond.C4_Status);
			AssertEquals(true, manager.PreventSend);
		}

		public void TestMessageFriendlyName()
		{
			AssertEquals("MessageFriendlyName", "Underbond Request for:  (HouseBill)", Manager.MessageFriendlyName);

			Underbond.C4_SendersMessageReference = "CuckooSqueaker";
			AssertEquals("MessageFriendlyName", "Underbond Request for: CuckooSqueaker (HouseBill)", Manager.MessageFriendlyName);
		}

		public void TestGenerateOriginalMessages()
		{
			var result = Manager.GenerateOriginalMessages(Underbond);
			AssertEquals("Length", 1, result.Length);
			AssertEquals(typeof(CMRUBMREQMessage), result[0].GetType());
		}

		public void TestGenerateOriginalMessagesForNullLinkObject()
		{
			var message = Factory.New<DummyBusinessObject>();
			Underbond.LinkedObject = null;
			AssertNoExceptionThrown(() => { EDIMessage[] result = Manager.GenerateOriginalMessages(Underbond); Assert(result.Length == 0); });
		}

		public void TestGenerateAmendmentMessages()
		{
			var result = Manager.GenerateAmendmentMessages(Underbond);
			AssertEquals("Length", 1, result.Length);
			AssertEquals(typeof(CMRUBMREQMessage), result[0].GetType());
		}

		public void TestGenerateWithdrawalMessages()
		{
			var result = Manager.GenerateWithdrawalMessages(Underbond);
			AssertEquals("Length", 1, result.Length);
			AssertEquals(typeof(CMRUBMREQMessage), result[0].GetType());
		}

		public void TestGetStatus()
		{
			SetStatus("123");
			AssertEquals("GetStatus()", "123", ((CusUnderbondUBMREQManagerForTest)Manager).GetStatus());
		}

		public void TestResetToOriginal()
		{
			Underbond.Messages.AddNew(typeof(CMRUBMREQMessage));
			SetStatus("123");
			AssertEquals("GetStatus()", "123", ((CusUnderbondUBMREQManagerForTest)Manager).GetStatus());
			Manager.ResetToOriginal();
			AssertEquals("GetStatus()", CMRBaseStatuses.Codes.NotSent, ((CusUnderbondUBMREQManagerForTest)Manager).GetStatus());
			AssertEquals("Message Status", EDIMessage.Status.Discarded, Underbond.Messages[0].EM_Status);
		}

		public void TestStatusCalculators()
		{
			AssertEquals("Length", 1, ((CusUnderbondUBMREQManagerForTest)Manager).StatusCalculators.Length);
			AssertEquals("StatusCalculators", typeof(CusUnderbondStatusCalculator), ((CusUnderbondUBMREQManagerForTest)Manager).StatusCalculators[0].GetType());
		}

		public void TestWeCantSendWithoutALinkedObject()
		{
			Underbond.LinkedObject = null;
			Assert("Error Reason Exists", ((CusUnderbondUBMREQManagerForTest)Manager).GetCommonNotificationsForSending().ContainsError("You can't send this underbond because it is not linked to anything."));
		}

		public void TestWeCantSendWithoutAValidLinkedObject()
		{
			Assert("Error Reason Exists", new CusUnderbondUBMREQManagerNoLinkedObjectForTest(Underbond).GetCommonNotificationsForSending().ContainsError("You can't send this underbond because it is not linked to a valid entity. (It is linked to 'Air Cargo House')"));
		}

		public void TestGetHeaderForHAWB()
		{
			AssertGetHeader(typeof(CusHAWB), typeof(CusHAWBUnderbondMovementRequestHeader));
		}

		public void TestGetHeaderForCTOHAWB()
		{
			var mawb = Factory.New<CTOCusMAWB>();
			AssertGetHeader(mawb.ChildBills.AddNew(), typeof(CTOCusHAWBUnderbondMovementRequestHeader));
		}

		public void TestGetHeaderForMAWB()
		{
			AssertGetHeader(typeof(CusMAWB), typeof(CusMAWBUnderbondMovementRequestHeader));
		}

		public void TestGetHeaderForCusSCAContainer()
		{
			AssertGetHeader(typeof(CusSCAContainer), typeof(CusSCAContainerUnderbondMovementRequestHeader));
		}

		public void TestGetHeaderForCusSCAPivot()
		{
			AssertGetHeader(typeof(CusSCAPivot), typeof(CusSCAPivotUnderbondMovementRequestHeader));
		}

		public void TestGetHeaderForCusSeaManOBLDetail()
		{
			AssertGetHeader(typeof(CusSeaManOBLDetail), typeof(CusSeaManOBLDetailUnderbondMovementRequestHeader));
		}

		public void TestNotifications()
		{
			var manger = new CusUnderbondUBMREQManagerForTest(Underbond);
			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "11111111111111");
			AssertEquals(true, manger.GetCommonNotificationsForSending().ContainsWarning("The current company (" + GlbCompany.CurrentCompany.GC_Name + ") ABN cannot be greater than 11 characters."));

			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "");
			AssertEquals(true, manger.GetCommonNotificationsForSending().ContainsWarning("The current company (" + GlbCompany.CurrentCompany.GC_Name + ") ABN has not been entered."));

			var mockQuerier = new Mock<IServiceManagerQuerier>();
			mockQuerier.Setup(q => q.CheckStateOfNamedServiceTask(It.IsAny<string>())).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

			using (ObjectFactory.Substitute(mockQuerier.Object))
			{
				GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "123");
				AssertEquals(0, manger.GetCommonNotificationsForSending().WarningCount);
			}
		}

		protected override void SetStatus(ZString status)
		{
			Underbond.UnderbondStatus.Code = status;
		}

		protected override CMRMessageManager GetManager() => new CusUnderbondUBMREQManagerForTest(Underbond);

		protected override void SetUp()
		{
			base.SetUp();
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
		}

		void AssertGetHeader(Type bizoType, Type expectedHeader)
		{
			AssertGetHeader(Factory.New(bizoType), expectedHeader);
		}

		void AssertGetHeader(BusinessObject bizo, Type expectedHeader)
		{
			Underbond.LinkedObject = (ICusUnderbondDependentCollectionParent)bizo;
			AssertEquals("HeaderType", expectedHeader, ((CusUnderbondUBMREQManagerForTest)Manager).GetHeader(Underbond).GetType());
		}

		CusUnderbond underbond;
		CusUnderbond Underbond
		{
			get
			{
				if (underbond == null)
				{
					var mawb = Factory.New<CusMAWB>();
					var hawb = mawb.ChildBills.AddNew();
					underbond = (CusUnderbond)((ICusUnderbondDependentCollectionParent)hawb).Underbonds.AddNew();
				}
				return underbond;
			}
		}

		sealed class CusUnderbondUBMREQManagerForTest : CusUnderbondUBMREQManager
		{
			public CusUnderbondUBMREQManagerForTest(CusUnderbond underbond) : base(underbond)
			{
			}

			internal new Customs.Business.MessageSendingNotificationCollection GetCommonNotificationsForSending() => base.GetCommonNotificationsForSending();
			internal new IUnderbondMovementRequestHeader GetHeader(CusUnderbond underbond) => base.GetHeader(underbond);
			internal new ICalculatedCusStatusCalculator[] StatusCalculators => base.StatusCalculators;
			internal new string GetStatus() => base.GetStatus();
		}

		sealed class CusUnderbondUBMREQManagerNoLinkedObjectForTest : CusUnderbondUBMREQManager
		{
			public CusUnderbondUBMREQManagerNoLinkedObjectForTest(CusUnderbond underbond) : base(underbond)
			{
			}

			internal new Customs.Business.MessageSendingNotificationCollection GetCommonNotificationsForSending() => base.GetCommonNotificationsForSending();
			protected override IUnderbondMovementRequestHeader GetHeader(CusUnderbond underbond) => null;
		}
	}
}
