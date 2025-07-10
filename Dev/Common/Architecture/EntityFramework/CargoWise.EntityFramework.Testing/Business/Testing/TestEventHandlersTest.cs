namespace CargoWise.EntityFramework.Testing
{
	sealed class TestEventHandlersTest : TestCaseWithDummy
	{
		public void TestEventsFiringIsRegistered()
		{
			using (var handlers = new EventHandlerTestHelper(Dummy))
			{
				Dummy.Z0_Description = "Some description";
				handlers.AssertValueChangedEventFired("Handlers should register firing the ValueChanged for the modified property", nameof(Dummy.Z0_DescriptionInfo));
				handlers.AssertValueChangedEventNotFired("Handlers should not register firing the ValueChanged for properties which have not been modified", nameof(Dummy.Z0_CodeInfo));
			}
		}
	}
}
