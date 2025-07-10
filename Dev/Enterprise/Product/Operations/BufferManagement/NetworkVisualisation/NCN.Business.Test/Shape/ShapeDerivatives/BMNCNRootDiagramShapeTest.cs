using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business.Test;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test.Shape.ShapeDerivatives
{
	[TestedType(typeof(BMNCNRootDiagramShape))]
	class BMNCNRootDiagramShapeTest : BaseShapeTestCase
	{
		#region Type Decider

		public void TestTypeDeciderForRootDiagrams()
		{
			var rootDiagram = Factory.NewWithValidTestData<BMNCNShape>();
			rootDiagram.BNS_ShapeType = DiagramShapeTypeList.Codes.Diagram;
			rootDiagram.BNS_BNS_RootShape = ZGuid.Empty;
			rootDiagram.BNS_Name = "Root Diagram";

			var childShape = Factory.NewWithValidTestData<BMNCNShape>();
			childShape.BNS_ShapeType = DiagramShapeTypeList.Codes.Shape;
			childShape.MakeChildOf(rootDiagram);
			childShape.BNS_Name = "Child Shape";

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedScaledRoot = newFactory.Load<BMNCNShape>(rootDiagram.PK);
			AssertType<BMNCNRootDiagramShape>("Root diagrams should load with the appropriate subclass. SAD!", loadedScaledRoot);

			var loadedChildShape = newFactory.Load<BMNCNShape>(childShape.PK);
			AssertType<BMNCNShape>("Child shapes should load with the base class. SAD!", loadedChildShape);
		}

		public void TestSaveRootDiagram_ShouldNotThrowException_WhenThereAreMoreThenOneInstanceLoaded()
		{
			var rootDiagram = Factory.NewWithValidTestData<BMNCNRootDiagramShape>();
			rootDiagram.BNS_ShapeType = DiagramShapeTypeList.Codes.Diagram;
			rootDiagram.BNS_BNS_RootShape = ZGuid.Empty;
			rootDiagram.BNS_Name = "Root Diagram";

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var diagramInstance1 = newFactory.Load<BMNCNRootDiagramShape>(rootDiagram.PK);
			diagramInstance1.IsNull = true;
			var diagramInstance2 = newFactory.Load<BMNCNShape>(rootDiagram.PK);
			diagramInstance2.BNS_Name = "SomeThing Changed";

			CombineAssertions("IsSavedByFactory should be true for both instances and should save without exception", () =>
			{
				AssertEquals(true, diagramInstance1.IsSavedByFactory);
				AssertEquals(true, diagramInstance2.IsSavedByFactory);

				AssertNoExceptionThrown(() => newFactory.Save());
			});
		}

		public void TestCloneRootDiagram_ShouldReturnCorrectType()
		{
			var diagram = Factory.NewWithValidTestData<BMNCNRootDiagramShape>();
			var clone = diagram.Clone();

			AssertType<BMNCNRootDiagramShape>(clone);
		}

		#endregion

		#region Channels

		public void TestDelete_ShouldDeleteChannelsAndRules()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var channel = NetworkTestCase.CreateChannel(diagram);
			var rule = NetworkTestCase.CreateLevelingRule(diagram);
			var link = NetworkTestCase.CreateLevelingRuleChannelLink(rule, channel);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedDiagram = newFactory.Load<BMNCNRootDiagramShape>(diagram.PK);

			loadedDiagram.Delete();
			newFactory.Save();

			AssertEquals(true, channel.IsDeleted);
			AssertEquals(true, rule.IsDeleted);
			AssertEquals(true, link.IsDeleted);
		}

		public void TestChannelsCollection_ForScaledDiagram_ShouldNotBeReadOnly()
		{
			var shape = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var channels = shape.Channels;
			AssertEquals(false, channels.ReadOnly);
		}

		public void TestChannelsCollection_ForNonScaledDiagram_ShouldBeReadOnly()
		{
			var shape = NetworkTestCase.CreateDiagram(Factory);
			var channels = shape.Channels;
			AssertEquals(true, channels.ReadOnly);
		}

		public void TestChangeChannel_ShouldSetHasChangesOnDiagram()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);

			Factory.Save();
			AssertEquals(false, diagram.HasChanges);

			var channel = diagram.Channels.AddNew();
			AssertEquals("The diagram should have changes when a new channel is added. SAD!", true, diagram.HasChanges);

			channel.FillWithValidTestData();
			Factory.Save();
			AssertEquals(false, channel.HasChanges);
			AssertEquals(false, diagram.HasChanges);

			channel.BNL_Name = "Squanch";
			AssertEquals(true, channel.HasChanges);
			AssertEquals("The diagram should have changes when any of its channels has changes. SAD!", true, diagram.HasChanges);
		}

		public void TestClone_ShouldCloneChannels()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var channel = diagram.Channels.AddNew();
			channel.BNL_Name = "Squanch";

			var clone = (BMNCNRootDiagramShape)diagram.Clone();
			AssertContainsExactElementsInAnyOrder("Cloning the diagrams should also clone its channels. SAD!", new[] { "Squanch" }, clone.Channels.Select(x => x.BNL_Name));
			AssertNotEquals("The cloned diagram should have new copies of the channels in the original diagram, not references to the same rows. SAD!", channel.PK, clone.Channels.Single().PK);
		}

		#endregion

		#region Shapes

		public void TestAllNestedShapes()
		{
			var diagram1 = NetworkTestCase.CreateDiagram(Factory, name: "diagram1");
			var shape1_1 = NetworkTestCase.CreateShape(diagram1, name: "shape1_1");
			var shape1_2 = NetworkTestCase.CreateShape(shape1_1, name: "shape1_2");

			var diagram2 = NetworkTestCase.CreateDiagram(Factory, name: "diagram1");
			var shape2_1 = NetworkTestCase.CreateShape(diagram2, name: "shape2_1");
			var shape2_2 = NetworkTestCase.CreateShape(shape2_1, name: "shape2_2");

			AssertContainsExactElementsInAnyOrder(new[] { shape1_1, shape1_2 }, diagram1.AllNestedShapes);
			AssertContainsExactElementsInAnyOrder(new[] { shape2_1, shape2_2 }, diagram2.AllNestedShapes);
		}

		#endregion

		#region Root

		public void TestBNS_BNS_Root_WhenSetWithNonEmptyValue_ShouldReportError()
		{
			var diagram1 = Factory.New<BMNCNRootDiagramShape>();
			var diagram2 = Factory.New<BMNCNRootDiagramShape>();
			AssertEquals(ZGuid.Empty, diagram2.BNS_BNS_RootShape);

			diagram2.BNS_BNS_RootShape = diagram1.PK;
			AssertEquals("Attempted to set a root diagram on another root diagram shape. This makes no sense. SAD!", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#endregion

		#region Properties

		public void TestShowNonScheduledSection_ShouldBeFalseByDefault()
		{
			var diagram = Factory.New<BMNCNRootDiagramShape>();
			AssertEquals(false, diagram.ShouldShowNonScheduledSection);
		}

		public void TestShowNonScheduledSection_ForScaledRootDiagram_WhenSet_ShouldNotReportError()
		{
			var diagram = Factory.New<BMNCNRootDiagramShape>();
			diagram.IsScaled = true;
			diagram.ShouldShowNonScheduledSection = true;
			AssertEquals("Setting the value on a scaled root diagram should work. SAD!", true, diagram.ShouldShowNonScheduledSection);
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);

			diagram.FillWithValidTestData();
			Factory.Save();

			var loadedDiagram = new BusinessObjectFactory().Load<BMNCNRootDiagramShape>(diagram.PK);
			AssertEquals("The value should have been stored in the shape's xml properties. SAD!", true, loadedDiagram.ShouldShowNonScheduledSection);
		}

		public void TestShowNonScheduledSection_ForNonScaledRootDiagram_WhenSet_ShouldReportError()
		{
			var diagram = Factory.New<BMNCNRootDiagramShape>();
			diagram.ShouldShowNonScheduledSection = true;
			AssertEquals("Tried to enable the non-scheduled section on a shape that isn't a scaled diagram. SAD!", ErrorReporter.LastMessageReported);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals("Setting the value on a nonscaled root diagram should still work, just to make the property more sensible. SAD!", true, diagram.ShouldShowNonScheduledSection);

			ErrorReporter.Clear();
		}

		public void TestShowNonScheduledSection_ForScaledDiagram_ShouldNotBeReadOnly()
		{
			var diagram = Factory.New<BMNCNRootDiagramShape>();
			diagram.IsScaled = true;
			AssertEquals("The property should not be readonly, because scaled diagrams may use it. SAD!", false, diagram.ShouldShowNonScheduledSectionInfo.ReadOnly);
		}

		public void TestShowNonScheduledSection_ForNonScaledDiagram_ShouldBeReadOnly()
		{
			var diagram = Factory.New<BMNCNRootDiagramShape>();
			AssertEquals("The property should be readonly, because only scaled diagrams may use it. SAD!", true, diagram.ShouldShowNonScheduledSectionInfo.ReadOnly);
		}

		public void TestXmlProperties_ShouldBeSerialised()
		{
			var diagram = Factory.NewWithValidTestData<BMNCNRootDiagramShape>();

			diagram.Width = 100;
			diagram.Left = 10;
			diagram.Top = 12;

			Factory.Save();

			var loadedDiagram = Factory.CreateNewFactory().Load<BMNCNShape>(diagram.PK);

			AssertEquals(100m, loadedDiagram.Width);
			AssertEquals(0m, loadedDiagram.Height);
			AssertEquals(10m, loadedDiagram.Left);
			AssertEquals(12m, loadedDiagram.Top);
		}

		#endregion

		#region Implementation

		protected override IEnumerable<string> XmlMemberNames => BMNCNShapeTest.BMNCNShape_XmlMemberNames.Where(w => w != nameof(BMNCNRootDiagramShape.Height)).Concat(new[]
		{
			nameof(BMNCNRootDiagramShape.ShouldShowNonScheduledSection),
			nameof(BMNCNRootDiagramShape.NonScheduledSectionWidth),
		});

		protected override BusinessObject GetNewBusinessObject()
		{
			return NetworkTestCase.CreateDiagram(Factory, isScaled: true);
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion
	}

	[TestedType(typeof(BMNCNRootDiagramShape))]
	class BMNCNRootDiagramShapeLinkEntityTest : ShapeLinkEntityTestCase<BMNCNRootDiagramShape>
	{
	}
}
