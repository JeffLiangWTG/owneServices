using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(WowDeliveryInstructionsHelper))]
	class WOWDeliveryInstructionsHelperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestUpdateDeliveryInstructions()
		{
			DeliveryInstructions instruc = new DeliveryInstructions();
			instruc.Destination = DeliveryInstructionDestination.Preview;
			instruc.DocumentPackTitle = WowDeliveryInstructionsHelper.LandedCostingDocument;
			WowDeliveryInstructionsHelper helper = WowDeliveryInstructionsHelper.New(instruc);
			AssertEquals("AllowModifyAndPreviewInExcel", false, helper.DelvInstructions.AllowModifyAndPreviewInExcel);
			helper.AddExtraInfoOnDeliveryInstructions();
			AssertEquals("AllowModifyAndPreviewInExcel", true, helper.DelvInstructions.AllowModifyAndPreviewInExcel);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return WowDeliveryInstructionsHelper.New(new DeliveryInstructions());
		}
	}
}
