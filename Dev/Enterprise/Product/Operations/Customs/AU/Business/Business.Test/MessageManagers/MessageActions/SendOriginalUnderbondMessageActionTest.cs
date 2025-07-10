using CargoWise.Application;
using Enterprise.Customs.Business.Interfaces;
using Moq;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SendOriginalUnderbondMessageActionTest : MessageActionAbstractTest
	{
		public void TestSendOriginalUnderbondMessageAction()
		{
			var oceanBill = CreateValidLCLOceanBill();
			var action = GetAction();
			var manager = new TestHelperCusSCAOceanBillMessageManager(oceanBill);
			var originals = action.WhichMessagesShouldWeSend(manager.ExposedGetAllMessageManagers());
			var resets = action.WhichMessagesShouldWeReset(manager.ExposedGetAllMessageManagers());
			var withdraws = action.WhichMessagesShouldWeWithdraw(manager.ExposedGetAllMessageManagers());
			AssertEquals("Should have 2 Orignal Message", 3, originals.Length);
			foreach (var originalManager in originals)
			{
				AssertEquals("Single Message Manager Type", typeof(CusUnderbondUBMREQManager), originalManager.GetType());
			}
			AssertEquals("No Reset Messages", 0, resets.Length);
			AssertEquals("No Withdraws", 0, withdraws.Length);
		}

		public void TestSendOriginalUnderbondMessageActionWithMessageErrors()
		{
			var oceanBill = CreateLCLOceanBillWithMessageErrors();
			var action = GetAction();
			var manager = new TestHelperCusSCAOceanBillMessageManager(oceanBill);
			manager.SendOriginalMessages(action);

			AssertEquals("Cargo Report Message should exist", 0, ((ICusUnderbondDependentCollectionParent)oceanBill.HouseBills[0].Pivot[0]).Underbonds[0].Messages.Count);
			AssertEquals("Cargo Report Message should exist", 0, ((ICusUnderbondDependentCollectionParent)oceanBill.Containers[0]).Underbonds[0].Messages.Count);

			var mockQuerier = new Mock<IServiceManagerQuerier>();
			mockQuerier.Setup(q => q.CheckStateOfNamedServiceTask("AUS")).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

			using (ObjectFactory.Substitute(mockQuerier.Object))
			{
				oceanBill = CreateValidLCLOceanBill();
				action = GetAction();
				manager = new TestHelperCusSCAOceanBillMessageManager(oceanBill);
				manager.SendOriginalMessages(action);

				AssertEquals("Cargo Report Message should exist", 1, ((ICusUnderbondDependentCollectionParent)oceanBill.HouseBills[0].Pivot[0]).Underbonds[0].Messages.Count);
				AssertEquals("Cargo Report Message should exist", 1, ((ICusUnderbondDependentCollectionParent)oceanBill.Containers[0]).Underbonds[0].Messages.Count);
			}
		}

		protected override BaseMessageAction GetAction() => new SendOrignalUnderbondMessageAction();
	}
}
