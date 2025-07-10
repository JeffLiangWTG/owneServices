using System.Collections.Specialized;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.MasterFiles.Business;
using Moq;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class MessageActionAbstractTest : SeaCargoTestCase
	{
		public void TestContinueWithSend()
		{
			var mockQuerier = new Mock<IServiceManagerQuerier>();
			mockQuerier.Setup(q => q.CheckStateOfNamedServiceTask(It.IsAny<string>())).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

			using (ObjectFactory.Substitute(mockQuerier.Object))
			{
				var oceanBill = CreateValidLCLOceanBill();
				var manager = new CusSCAOceanBillMessageManager(oceanBill);
				var action = GetAction();
				var actionResult = manager.SendOriginalMessages(action).Any();
				Assert("Continue with send should have passed and sent Original Messages from CusSCAOceanBillMessageManager : last message " + action.LastMessage, actionResult);
			}
		}

		public void TestContinueWithSendEvent()
		{
			var action = GetAction();
			action.ShowQuestion += new ContinueEventHandler(Action_ShowQuestion);
			var warnings = new StringCollection();
			warnings.Add("Warning Message 1 - This could be a problem");
			warnings.Add("Warning Message 2 - Another possible problem");
			action.SendWithMessageErrors = true;
			cancelAction = true;

			Assert("Should not have cancelled action", action.ContinueWithSend(warnings));
			Assert("Failed to call event", showQuestionCalled);
			cancelAction = false;
			Assert("Should have failed to continue", !action.ContinueWithSend(warnings));
		}

		public void TestOnNotifyUserOfASuccessfulSend()
		{
			var action = GetAction();
			const string SuccessMessage = "The last message was successfully sent";
			action.NotifyUserOfASuccessfulSend(SuccessMessage);
			AssertEquals("On Notify User Of A Successful Send", SuccessMessage, action.LastMessage);
		}

		public void TestMessageSendErrorAlert()
		{
			var action = GetAction();
			var errors = new StringCollection();
			errors.Add("The last message was not successfully sent");
			errors.Add("Errors exist");
			action.MessageSendErrorAlert(errors);
			AssertEquals("Message Send Error Alert", errors, action.MessageSendErrors);
		}

		public void TestWarnUserAboutSomething()
		{
			var action = GetAction();
			const string WarningMessage = "This could be a problem";
			const string WarningCaption = "Caption";
			action.WarnUserAboutSomething(WarningMessage, WarningCaption);
			AssertEquals("On Notify User Of A Successful Send", WarningMessage, action.WarningMessage);
			AssertEquals("On Notify User Of A Successful Send", WarningCaption, action.WarningCaption);
		}

		public void TestWarnUserEvent()
		{
			var action = GetAction();
			action.ShowWarning += new WarningEventHandler(Action_ShowWarning);
			action.WarnUserAboutSomething("This could be a problem", "Caption");
			Assert("Failed to call event", showWarningActionCalled);
		}

		public void TestYesNoQuery()
		{
			var action = GetAction();
			Assert(!action.YesNoQuery("", ""));
		}

		public void TestYesNoCancelQuery()
		{
			var action = GetAction();
			AssertEquals(Customs.Business.YesNoCancel.Cancel, action.YesNoCancelQuery("", ""));
		}

		public void TestNotifyUserOfAnInvalidOperation()
		{
			var action = GetAction();
			const string InvalidOperationMessage = "The last action resulted in an invalid operation";
			action.NotifyUserOfAnInvalidOperation(InvalidOperationMessage);
			AssertEquals("On Notify User Of A Successful Send", InvalidOperationMessage, action.InvalidOperationText);
		}

		protected abstract BaseMessageAction GetAction();

		protected override void SetUp()
		{
			Common.AU.CMR.Testing.CMRUtilitiesTest.SetupCertificates();
			base.SetUp();
		}

		protected const string CTOPremiseID = "G527H";

		protected virtual CusSCAOceanBill CreateValidLCLOceanBill()
		{
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = "41065894724";
			var result = CreateOceanBill();
			var house1 = result.HouseBills.AddNew();
			var house2 = result.HouseBills.AddNew();

			var container = result.Containers.AddNew();

			var pivot1 = container.Pivots.AddNew();
			house1.Pivot.Add(pivot1);

			var pivot2 = container.Pivots.AddNew();
			house2.Pivot.Add(pivot2);

			EnsureValidContainer(container, ContainerNumber1);
			EnsureValidPivot(pivot1, "Line - Pivot 1");
			EnsureValidPivot(pivot2, "Line - Pivot 2");

			EnsureValidHouse(house1, HouseBillNumber1);
			EnsureValidHouse(house2, HouseBillNumber2);

			const string Depot1PremiseID = "1234D";
			const string Depot2PremiseID = "CB12E";
			var containerUnderbond = (CusUnderbond)((ICusUnderbondDependentCollectionParent)container).Underbonds.AddNew();
			EnsureValidUnderbond(containerUnderbond, CTOPremiseID, Depot1PremiseID, true);

			var house1Underbond = (CusUnderbond)((ICusUnderbondDependentCollectionParent)house1.Pivot[0]).Underbonds.AddNew();
			EnsureValidUnderbond(house1Underbond, Depot1PremiseID, Depot2PremiseID);

			var house2Underbond = (CusUnderbond)((ICusUnderbondDependentCollectionParent)house2.Pivot[0]).Underbonds.AddNew();
			EnsureValidUnderbond(house2Underbond, Depot1PremiseID, Depot2PremiseID);

			return result;
		}

		protected virtual CusSCAOceanBill CreateLCLOceanBillWithMessageErrors()
		{
			var result = CreateOceanBill();
			result.CB_OH_ShippingLine = ZGuid.Empty;

			var house1 = result.HouseBills.AddNew();
			var house2 = result.HouseBills.AddNew();
			var container = result.Containers.AddNew();

			var pivot1 = container.Pivots.AddNew();
			house1.Pivot.Add(pivot1);

			var pivot2 = container.Pivots.AddNew();
			house2.Pivot.Add(pivot2);

			var containerUnderbond = (CusUnderbond)((ICusUnderbondDependentCollectionParent)container).Underbonds.AddNew();
			var house1Underbond = (CusUnderbond)((ICusUnderbondDependentCollectionParent)house1.Pivot[0]).Underbonds.AddNew();
			var house2Underbond = (CusUnderbond)((ICusUnderbondDependentCollectionParent)house2.Pivot[0]).Underbonds.AddNew();
			return result;
		}

		bool showQuestionCalled;
		bool cancelAction;
		bool showWarningActionCalled;

		void Action_ShowQuestion(object sender, ContinueEventArgs e)
		{
			showQuestionCalled = true;
			e.Cancel = cancelAction;
		}

		void Action_ShowWarning(object sender, WarningEventArgs e)
		{
			showWarningActionCalled = true;
		}

		protected sealed class TestHelperCusSCAOceanBillMessageManager : CusSCAOceanBillMessageManager
		{
			public TestHelperCusSCAOceanBillMessageManager(CusSCAOceanBill oceanBill)
				: base(oceanBill)
			{
			}

			public Customs.Business.SingleMessageManager[] ExposedGetAllMessageManagers() => GetAllMessageManagers();
		}
	}
}
