using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(BMNCNChannelCollection))]
	class BMNCNChannelCollectionTest : ActiveBusinessObjectCollectionTestCase<BMNCNChannelCollection>
	{
		public void TestAddNew_ShouldIncrementDefaultSequence()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var channel1 = diagram.Channels.AddNew();
			var channel2 = diagram.Channels.AddNew();

			AssertEquals("The first channel should have a sequence of 1 by default. SAD!", 1, channel1.BNL_Sequence);
			AssertEquals("Each channel added should increment its sequence by 1. SAD!", 2, channel2.BNL_Sequence);

			channel1.BNL_Sequence = 100;
			var channel3 = diagram.Channels.AddNew();

			AssertEquals("The highest sequence number in the collection should be used to determine the next sequence number, even if it doesn't belong to the most recently-added channel. SAD!", 101, channel3.BNL_Sequence);
		}

		#region Implementation

		protected override BMNCNChannelCollection GetCollectionToTest()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			return diagram.Channels;
		}

		#endregion
	}
}
