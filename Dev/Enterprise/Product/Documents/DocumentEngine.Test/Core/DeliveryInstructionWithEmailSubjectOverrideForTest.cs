using CargoWise.Types;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DeliveryInstructionWithEmailSubjectOverrideForTest : DeliveryInstructions, IDeliveryInstructionsWithEmailSubjectOverride
	{
		public DeliveryInstructionWithEmailSubjectOverrideForTest() { }

		public ZString EmailSubjectOverride
		{
			get { return emailSubjectOverride; }
			set { emailSubjectOverride = value; }
		}
		ZString emailSubjectOverride;
	}
}
