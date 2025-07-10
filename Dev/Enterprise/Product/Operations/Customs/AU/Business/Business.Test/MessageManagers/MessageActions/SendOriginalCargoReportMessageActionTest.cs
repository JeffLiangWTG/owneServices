using CargoWise.Application;
using Enterprise.Customs.Business;
using Moq;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SendOriginalCargoReportMessageActionTest : MessageActionAbstractTest
	{
		public void TestSendOriginalCargoReportMessageAction()
		{
			var oceanBill = CreateValidLCLOceanBill();
			var action = GetAction();
			var manager = new TestHelperCusSCAOceanBillMessageManager(oceanBill);
			var originals = action.WhichMessagesShouldWeSend(manager.ExposedGetAllMessageManagers());
			var resets = action.WhichMessagesShouldWeReset(manager.ExposedGetAllMessageManagers());
			var withdraws = action.WhichMessagesShouldWeWithdraw(manager.ExposedGetAllMessageManagers());
			AssertEquals("Should have 2 Orignal Message", 2, originals.Length);
			foreach (SingleMessageManager originalManager in originals)
			{
				AssertEquals("Single Message Manager Type", typeof(CusSCAHouseSEACRManager), originalManager.GetType());
			}
			AssertEquals("No Reset Messages", 0, resets.Length);
			AssertEquals("No Withdraws", 0, withdraws.Length);
		}

		public void TestSendOriginalCargoReportMessageActionWithErrors()
		{
			var oceanBill = CreateLCLOceanBillWithMessageErrors();
			var action = GetAction();
			var manager = new TestHelperCusSCAOceanBillMessageManager(oceanBill);
			manager.SendOriginalMessages(action);

			AssertEquals("Cargo Report Message should exist", 0, oceanBill.HouseBills[0].Messages.Count);
			AssertEquals("Cargo Report Message should exist", 0, oceanBill.HouseBills[1].Messages.Count);

			var mockQuerier = new Mock<IServiceManagerQuerier>();
			mockQuerier.Setup(q => q.CheckStateOfNamedServiceTask("AUS")).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

			using (ObjectFactory.Substitute(mockQuerier.Object))
			{
				oceanBill = CreateValidLCLOceanBill();
				action = GetAction();
				manager = new TestHelperCusSCAOceanBillMessageManager(oceanBill);
				manager.SendOriginalMessages(action);

				AssertEquals("Cargo Report Message should exist", 1, oceanBill.HouseBills[0].Messages.Count);
				AssertEquals("Cargo Report Message should exist", 1, oceanBill.HouseBills[1].Messages.Count);
			}
		}

		protected override BaseMessageAction GetAction() => new SendOriginalCargoReportMessageAction();
	}
}
