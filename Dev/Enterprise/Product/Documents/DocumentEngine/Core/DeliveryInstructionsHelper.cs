using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine
{
	public class DeliveryInstructionsHelper : NonPersistentBusinessObject, IObsoleteValidation
	{
		protected DeliveryInstructionsHelper(DeliveryInstructions instructions)
		{
			DelvInstructions = instructions;
		}

		public static DeliveryInstructionsHelper New(DeliveryInstructions instructions)
		{
			DeliveryInstructionsHelper result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(instructions);
			}
			else if (instructions != null)
			{
				result = new DeliveryInstructionsHelper(instructions);
			}

			return result;
		}

		public void AddExtraInfoOnDeliveryInstructions()
		{
			AddExtraInfoOnDeliveryInstructionsCore();
		}

		protected virtual void AddExtraInfoOnDeliveryInstructionsCore()
		{
		}

		protected delegate DeliveryInstructionsHelper NewDelegate(DeliveryInstructions instructions);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		public readonly DeliveryInstructions DelvInstructions;
	}
}
