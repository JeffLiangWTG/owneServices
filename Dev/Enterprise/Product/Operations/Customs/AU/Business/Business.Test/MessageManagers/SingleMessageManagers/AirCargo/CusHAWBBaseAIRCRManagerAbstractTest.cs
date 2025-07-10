using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Moq;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class CusHAWBBaseAIRCRManagerAbstractTest : CMRMessageManagerAbstractTest
	{
		public abstract void TestMessageFriendlyName();

		public void TestRequiresAmendment()
		{
			var masterAwb = Factory.NewWithValidTestData<CusMAWB>();
			var typeOfBill = HAWB.GetType();

			var houseAwb = (CusHAWBBase)Factory.New(typeOfBill);
			houseAwb.FillWithValidTestData();

			masterAwb.ChildBills.Add(houseAwb);

			houseAwb.CMRMessageStatus.Code = CMRBaseStatuses.Codes.AmendmentAccepted;
			houseAwb.CS_CM = ZGuid.Empty;

			Factory.Save();

			var houseAwbInNewFacotry = (CusHAWBBase)NewFactory().Load(typeOfBill, houseAwb.PK);
			AssertNull("Precodition", houseAwbInNewFacotry.MAWB);

			houseAwbInNewFacotry.CS_CM = masterAwb.PK;
			AssertNotNull("Should not be null as the CS_CM is valid.", houseAwbInNewFacotry.MAWB);

			var manager = GetManager(houseAwbInNewFacotry);
			Assert("Precodition", manager.HasActiveMessages);
			Assert("Should be false as the CS_CM is changed from an empty value.", manager.RequiresAmendment());
		}

		public void TestBusinessObject()
		{
			AssertEquals(HAWB, Manager.BusinessObject);
		}

		public void TestGenerateOriginalMessages()
		{
			var result = Manager.GenerateOriginalMessages(HAWB);
			AssertEquals("Length", 1, result.Length);
			AssertEquals(typeof(CMRAIRCRMessage), result[0].GetType());
		}

		public void TestGenerateAmendmentMessages()
		{
			var result = Manager.GenerateAmendmentMessages(HAWB);
			AssertEquals("Length", 1, result.Length);
			AssertEquals(typeof(CMRAIRCRMessage), result[0].GetType());
		}

		public void TestGenerateWithdrawalMessages()
		{
			var result = Manager.GenerateWithdrawalMessages(HAWB);
			AssertEquals("Length", 1, result.Length);
			AssertEquals(typeof(CMRAIRCRMessage), result[0].GetType());
		}

		public void TestGetStatus()
		{
			SetStatus("123");
			AssertEquals("GetStatus()", "123", ((CusHAWBBaseAIRCRManager)Manager).GetStatus());
		}

		public void TestResetToOriginal()
		{
			HAWB.Messages.AddNew(typeof(CMRAIRCRMessage));
			HAWB.CS_IsResponsePending = true;
			HAWB.MAWB.CM_HouseMessageIsSent = true;
			SetStatus("123");
			AssertEquals("GetStatus()", "123", ((CusHAWBBaseAIRCRManager)Manager).GetStatus());
			Manager.ResetToOriginal();
			AssertEquals("GetStatus()", CMRBaseStatuses.Codes.NotSent, ((CusHAWBBaseAIRCRManager)Manager).GetStatus());
			AssertEquals("Message Status", EDIMessage.Status.Discarded, HAWB.Messages[0].EM_Status);
			AssertEquals(false, HAWB.CS_IsResponsePending);
			AssertEquals(false, HAWB.MAWB.CM_HouseMessageIsSent);
		}

		public void TestNotifications()
		{
			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "11111111111111");
			AssertEquals(true, ManagerTest.GetNotificationsForSendingAnOriginal().ContainsWarning("The current company (" + GlbCompany.CurrentCompany.GC_Name + ") ABN cannot be greater than 11 characters."));
			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "");
			AssertEquals(true, ManagerTest.GetNotificationsForSendingAnOriginal().ContainsWarning("The current company (" + GlbCompany.CurrentCompany.GC_Name + ") ABN has not been entered."));

			var mockQuerier = new Mock<IServiceManagerQuerier>();
			mockQuerier.Setup(q => q.CheckStateOfNamedServiceTask(It.IsAny<string>())).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

			using (ObjectFactory.Substitute(mockQuerier.Object))
			{
				GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "1234");
				AssertEquals(0, ManagerTest.GetNotificationsForSendingAnOriginal().WarningCount);
			}
		}

		public void TestStatusCalculators()
		{
			AssertEquals("Length", 2, ((CusHAWBBaseAIRCRManager)Manager).StatusCalculators.Length);
			AssertEquals("StatusCalculators", typeof(CusHAWBStatusCalculator), ((CusHAWBBaseAIRCRManager)Manager).StatusCalculators[0].GetType());
			AssertEquals("StatusCalculators", typeof(CusHAWBMessageStatusCalculator), ((CusHAWBBaseAIRCRManager)Manager).StatusCalculators[1].GetType());
		}

		protected abstract CMRMessageManager GetManager(CusHAWBBase houseBill);

		protected abstract CusHAWBBase HAWB { get; }

		CusHAWBAIRCRMessageManager manger;
		protected CusHAWBAIRCRMessageManager ManagerTest => manger ?? (manger = new CusHAWBAIRCRMessageManager(Factory.New<CusHAWB>()));

		protected override void SetStatus(ZString status)
		{
			HAWB.CS_MsgStatus = status;
		}
	}
}
