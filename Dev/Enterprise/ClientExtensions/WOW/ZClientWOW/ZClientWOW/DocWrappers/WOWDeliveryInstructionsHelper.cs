using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Client.Wow
{
	class WowDeliveryInstructionsHelper : DeliveryInstructionsHelper
	{
		protected WowDeliveryInstructionsHelper(DeliveryInstructions instructions)
			: base(instructions)
		{
		}

		public new static WowDeliveryInstructionsHelper New(DeliveryInstructions instructions)
		{
			return new WowDeliveryInstructionsHelper(instructions);
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}

		protected override void AddExtraInfoOnDeliveryInstructionsCore()
		{
			base.AddExtraInfoOnDeliveryInstructionsCore();
			if (DelvInstructions.Destination == DeliveryInstructionDestination.Preview &&
				DelvInstructions.DocumentPackTitle == LandedCostingDocument)
			{
				DelvInstructions.AllowModifyAndPreviewInExcel = true;
			}
		}

		internal const string LandedCostingDocument = "Woolworths Landed Costing";
	}
}
