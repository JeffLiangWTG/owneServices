using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(BMNCNChannel))]
	class BMNCNChannelTest : EnterpriseBusinessObjectTestCase
	{
		#region Color

		public void TestColor_GetsAndSetsEqualValue()
		{
			var channel = Factory.New<BMNCNChannel>();

			foreach (CodeDescriptionPair pair in channel.ColorList)
			{
				channel.Color = pair.Code;
				AssertEquals("There was obviously a problem encoding and/or decoding the color name to/from its KnownColor value. SAD!", pair.Code, channel.Color);
			}
		}

		public void TestColor_InvalidIntValue_ShouldReturnEmptyString()
		{
			var channel = Factory.New<BMNCNChannel>();
			channel.BNL_Color = int.MaxValue;

			AssertEquals(ZString.Empty, channel.Color);
		}

		public void TestColor_InvalidNameEntered_ShouldStoreZero()
		{
			var channel = Factory.New<BMNCNChannel>();
			channel.Color = "Squanch";

			AssertEquals(0, channel.BNL_Color);
			AssertEquals("Squanch", channel.Color);
		}

		public void TestColor_WhenNotSet_ShouldReturnEmptyString()
		{
			var channel = Factory.New<BMNCNChannel>();
			AssertEquals(ZString.Empty, channel.Color);
		}

		#endregion

		#region Sequence

		public void TestSequence_ShouldBeOneByDefault()
		{
			var channel = Factory.New<BMNCNChannel>();
			AssertEquals(1, channel.BNL_Sequence);
		}

		#endregion

		#region Delete

		public void TestDelete_ShouldAlsoDeleteRuleLinks_ButLeaveRule()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var rule = NetworkTestCase.CreateLevelingRule(diagram);
			var channel = NetworkTestCase.CreateChannel(diagram);
			var link = NetworkTestCase.CreateLevelingRuleChannelLink(rule, channel);

			Factory.Save();

			channel.Delete();

			AssertEquals(true, link.IsDeleted);
			AssertEquals(false, rule.IsDeleted);
			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		#region Height

		public void TestDefaultHeight()
		{
			var channel = Factory.New<BMNCNChannel>();
			AssertEquals(600, channel.BNL_Height);
			AssertEquals(CCPMConstants.DefaultChannelHeight, channel.BNL_Height);
		}

		#endregion

		#region Shapes

		public void TestShapes()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			var diagramShape = NetworkTestCase.CreateDiagram(jobHeader);
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();

			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;

			var shape1 = NetworkTestCase.CreateShapeAtLocation(workflow1, diagram, 0, 0, 100, 100, "Daddy Bear");
			var shape2 = NetworkTestCase.CreateShapeAtLocation(workflow2, diagram, 150, 250, 100, 100, "Mummy Bear");
			var shape3 = NetworkTestCase.CreateShapeAtLocation(workflow2, diagram, 300, 300, 100, 200, "Uncie Bear");
			var shape4 = NetworkTestCase.CreateShapeAtLocation(workflow3, diagram, 450, 500, 100, 100, "Bebe Bear");

			var channel1 = NetworkTestCase.CreateChannel(diagram.ShapeAsRootDiagram, "Porridge", height: 200);
			var channel2 = NetworkTestCase.CreateChannel(diagram.ShapeAsRootDiagram, "Chair", height: 200);
			var channel3 = NetworkTestCase.CreateChannel(diagram.ShapeAsRootDiagram, "Bed", height: 200);

			AssertContainsExactElementsInAnyOrder(new[] { shape1.Name }, channel1.Shapes.Select(s => s.BNS_Name));
			AssertContainsExactElementsInAnyOrder(new[] { shape2.Name, shape3.Name }, channel2.Shapes.Select(s => s.BNS_Name));
			AssertContainsExactElementsInAnyOrder(new[] { shape3.Name, shape4.Name }, channel3.Shapes.Select(s => s.BNS_Name));

			channel1.BNL_Sequence = 69;

			AssertContainsExactElementsInAnyOrder(new[] { shape3.Name, shape4.Name }, channel1.Shapes.Select(s => s.BNS_Name));
			AssertContainsExactElementsInAnyOrder(new[] { shape1.Name }, channel2.Shapes.Select(s => s.BNS_Name));
			AssertContainsExactElementsInAnyOrder(new[] { shape2.Name, shape3.Name }, channel3.Shapes.Select(s => s.BNS_Name));

			channel1.BNL_Sequence = 1;

			AssertContainsExactElementsInAnyOrder(new[] { shape1.Name }, channel1.Shapes.Select(s => s.BNS_Name));
			AssertContainsExactElementsInAnyOrder(new[] { shape2.Name, shape3.Name }, channel2.Shapes.Select(s => s.BNS_Name));
			AssertContainsExactElementsInAnyOrder(new[] { shape3.Name, shape4.Name }, channel3.Shapes.Select(s => s.BNS_Name));

			channel1.BNL_Height = 600;

			AssertContainsExactElementsInAnyOrder(new[] { shape1.Name, shape2.Name, shape3.Name, shape4.Name }, channel1.Shapes.Select(s => s.BNS_Name));
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ZString>(), channel2.Shapes.Select(s => s.BNS_Name));
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ZString>(), channel3.Shapes.Select(s => s.BNS_Name));

			shape1.Y = 650;

			AssertContainsExactElementsInAnyOrder(new[] { shape2.Name, shape3.Name, shape4.Name }, channel1.Shapes.Select(s => s.BNS_Name));
			AssertContainsExactElementsInAnyOrder(new[] { shape1.Name }, channel2.Shapes.Select(s => s.BNS_Name));
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ZString>(), channel3.Shapes.Select(s => s.BNS_Name));

			shape2.Height = 900;

			AssertContainsExactElementsInAnyOrder(new[] { shape2.Name, shape3.Name, shape4.Name }, channel1.Shapes.Select(s => s.BNS_Name));
			AssertContainsExactElementsInAnyOrder(new[] { shape1.Name, shape2.Name }, channel2.Shapes.Select(s => s.BNS_Name));
			AssertContainsExactElementsInAnyOrder(new[] { shape2.Name }, channel3.Shapes.Select(s => s.BNS_Name));
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			return diagram.Channels.AddNew();
		}

		protected override void SetupDataForSettingValueCallsRefreshBindingTestIfNeeded(ZPropertyInfo info)
		{
			base.SetupDataForSettingValueCallsRefreshBindingTestIfNeeded(info);

			if (info.Name == nameof(BMNCNChannel.Color))
			{
				info.Value = new ZString("Red");
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion
	}
}
