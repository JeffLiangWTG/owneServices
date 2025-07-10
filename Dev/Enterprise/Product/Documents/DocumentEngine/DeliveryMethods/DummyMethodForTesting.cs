namespace Enterprise.DocumentEngine.DeliveryMethods
{
	sealed class DummyMethodForTesting : DeliveryMethod
	{
		public bool SetPropertiesFromDeliveryInstructionsCalled;

		protected override void SetPropertiesFromDeliveryInstructions(DeliveryInstructions instructions)
		{
			SetPropertiesFromDeliveryInstructionsCalled = true;
			base.SetPropertiesFromDeliveryInstructions(instructions);
		}
	}
}
