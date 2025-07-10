using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Moq;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	public class NetworkEntityControllerTest : TestCase
	{
		public void TestNotifyActionCannotBeExecuted()
		{
			interactionImplementor.Setup(i => i.ShowError(It.IsAny<string>(), It.IsAny<string>()))
				.Callback<string, string>((message, caption) =>
				{
					AssertEquals(@"This action cannot be executed for the given shape(s) due to the following reasons:
Entity1: This can't be done
Entity2: Because it's against the law", message);
					AssertEquals("Action cannot be executed", caption);
				});

			var result = new NetworkActionAccessibility(new NetworkActionDenialReason[] {
				new NetworkActionDenialReason(entity1, "This can't be done"),
				new NetworkActionDenialReason(entity2, "Because it's against the law"),
			});
			controller.NotifyActionCannotBeExecuted(result);
		}

		public void TestNotifyActionExecutedPartially()
		{
			interactionImplementor.Setup(i => i.ShowError(It.IsAny<string>(), It.IsAny<string>()))
				.Callback<string, string>((message, caption) =>
				{
					AssertEquals(@"This action failed to execute for some of the given shapes due to the following reasons:
Entity1: This can't be done
Entity2: Because it's against the law", message);
					AssertEquals("Action was executed partially", caption);
				});

			var result = new NetworkActionAccessibility(new NetworkActionDenialReason[] {
				new NetworkActionDenialReason(entity1, "This can't be done"),
				new NetworkActionDenialReason(entity2, "Because it's against the law"),
			});
			controller.NotifyActionExecutedPartially(result);
		}

		protected MockRepository mocks;
		protected Mock<INetworkUserInteractionImplementor> interactionImplementor;
		protected NetworkEntityController controller;
		protected Entity entity1;
		protected Entity entity2;

		protected override void SetUp()
		{
			base.SetUp();

			mocks = new MockRepository(MockBehavior.Default);
			interactionImplementor = new Mock<INetworkUserInteractionImplementor>();
			controller = new NetworkEntityController(interactionImplementor.Object);
			entity1 = new Entity() { Name = "Entity1" };
			entity2 = new Entity() { Name = "Entity2" };
		}
	}
}
