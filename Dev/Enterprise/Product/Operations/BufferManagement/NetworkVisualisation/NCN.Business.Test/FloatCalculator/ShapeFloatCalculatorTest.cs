using System.ComponentModel;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class ShapeFloatCalculatorTest : NetworkTestCase
	{
		#region Simple Pin Tests

		public void TestSimpleInnerPin()
		{
			var diagramShape = CreateDiagram(Factory, name: "diagram");
			var viewModel = CreateNetworkViewModel(diagramShape);
			var network = viewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var subDiagram1 = CreateShape(diagram, "subDiagram1");
			var shape1 = CreateShape(subDiagram1, "shape1");

			network.SwitchToScaled();
			diagram.ScheduledStartTimeLocal = ZDateTime.Now.AddDays(-1);

			subDiagram1.Width = 300;
			shape1.Width = 100;
			shape1.X = 100;
			shape1.Shape.PinShape(viewModel);

			ShapeFloatCalculator.CalculateFloatForChildren(network);

			AssertSchedule(shape1, earliestStart: 8m, latestStart: 8m, duration: 8m);
			AssertSchedule(subDiagram1, earliestStart: 0m, latestStart: 0m, duration: 24m);
		}

		public void TestSimpleInnerSubPin()
		{
			var diagramShape = CreateDiagram(Factory, name: "diagram");
			var viewModel = CreateNetworkViewModel(diagramShape);
			var network = viewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var subDiagram1 = CreateShape(diagram, "subDiagram1");
			var subSubDiagram1 = CreateShape(subDiagram1, "subSubDiagram1");
			var shape1 = CreateShape(subSubDiagram1, "shape1");

			network.SwitchToScaled();
			diagram.ScheduledStartTimeLocal = ZDateTime.Now.AddDays(-1);

			subDiagram1.Width = 300;
			subSubDiagram1.Width = 200;
			shape1.Width = 100;
			shape1.X = 100;
			shape1.Shape.PinShape(viewModel);

			ShapeFloatCalculator.CalculateFloatForChildren(network);

			AssertSchedule(shape1, earliestStart: 8m, latestStart: 8m, duration: 8m);
			AssertSchedule(subSubDiagram1, earliestStart: 0m, latestStart: 8m, duration: 16m);
			AssertSchedule(subDiagram1, earliestStart: 0m, latestStart: 0m, duration: 24m);
		}

		public void TestSimpleOuterPin()
		{
			var diagramShape = CreateDiagram(Factory, name: "diagram");
			var viewModel = CreateNetworkViewModel(diagramShape);
			var network = viewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var subDiagram1 = CreateShape(diagram, "subDiagram1");
			var subSubDiagram1 = CreateShape(subDiagram1, "subSubDiagram1");
			var shape1 = CreateShape(subSubDiagram1, "shape1");

			network.SwitchToScaled();
			diagram.ScheduledStartTimeLocal = ZDateTime.Now.AddDays(-1);

			subDiagram1.Width = 300;
			subDiagram1.Shape.PinShape(viewModel);
			subSubDiagram1.Width = 200;
			shape1.Width = 100;
			shape1.X = 100;

			ShapeFloatCalculator.CalculateFloatForChildren(network);

			AssertSchedule(shape1, earliestStart: 0m, latestStart: 16m, duration: 8m);
			AssertSchedule(subSubDiagram1, earliestStart: 0m, latestStart: 8m, duration: 16m);
			AssertSchedule(subDiagram1, earliestStart: 0m, latestStart: 0m, duration: 24m);
		}

		public void TestSimpleOuterSubPin()
		{
			var diagramShape = CreateDiagram(Factory, name: "diagram");
			var viewModel = CreateNetworkViewModel(diagramShape);
			var network = viewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var subDiagram1 = CreateShape(diagram, "subDiagram1");
			var subSubDiagram1 = CreateShape(subDiagram1, "subSubDiagram1");
			var shape1 = CreateShape(subSubDiagram1, "shape1");

			network.SwitchToScaled();
			diagram.ScheduledStartTimeLocal = ZDateTime.Now.AddDays(-1);

			subDiagram1.Width = 300;
			subSubDiagram1.Width = 200;
			subSubDiagram1.X = 100;
			subSubDiagram1.Shape.PinShape(viewModel);
			shape1.Width = 100;
			shape1.X = 100;

			ShapeFloatCalculator.CalculateFloatForChildren(network);

			AssertSchedule(shape1, earliestStart: 8m, latestStart: 16m, duration: 8m);
			AssertSchedule(subSubDiagram1, earliestStart: 8m, latestStart: 8m, duration: 16m);
			AssertSchedule(subDiagram1, earliestStart: 0m, latestStart: 0m, duration: 24m);
		}

		public void TestPinBetween()
		{
			var diagramShape = CreateDiagram(Factory, name: "diagram");
			var viewModel = CreateNetworkViewModel(diagramShape);
			var network = viewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var floatingSubDiagram = CreateShape(diagram, "floatingSubDiagram");
			var pinnedStart = CreateShape(diagram, "pinnedStart");
			var pinnedMiddly = CreateShape(floatingSubDiagram, "pinnedMiddly");
			var pinnedEnd = CreateShape(diagram, "pinnedEnd");

			network.SwitchToScaled();
			diagram.ScheduledStartTimeLocal = ZDateTime.Now.AddDays(-1);

			pinnedStart.Width = 100;
			pinnedStart.X = 0;

			pinnedMiddly.Width = 100;
			pinnedMiddly.X = 300;

			pinnedEnd.Width = 100;
			pinnedEnd.X = 600;

			pinnedStart.Shape.PinShape(viewModel);
			pinnedMiddly.Shape.PinShape(viewModel);
			pinnedEnd.Shape.PinShape(viewModel);

			floatingSubDiagram.Width = 200;

			pinnedStart.MakeVisiblePrerequisiteOf(floatingSubDiagram);
			floatingSubDiagram.MakeVisiblePrerequisiteOf(pinnedEnd);

			ShapeFloatCalculator.CalculateFloatForChildren(network);

			AssertSchedule(pinnedStart, earliestStart: 0m, latestStart: 0m, duration: 8m);
			AssertSchedule(pinnedMiddly, earliestStart: 24m, latestStart: 24m, duration: 8m);
			AssertSchedule(pinnedEnd, earliestStart: 48m, latestStart: 48m, duration: 8m);
			AssertSchedule(floatingSubDiagram, earliestStart: 16m, latestStart: 24m, duration: 16m);
		}

		#endregion

		#region Simple Float tests

		public void TestBasicFloatIdeaWorks()
		{
			var diagram = CreateDiagram(Factory, name: "root");
			var network = CreateNetwork(diagram);

			var startShape = CreateShape(diagram, "startShape").AsEntity(network);
			var ccShape = CreateShape(diagram, "ccShape").AsEntity(network);
			var endShape = CreateShape(diagram, "endShape").AsEntity(network);
			var nonCCShape = CreateShape(diagram, "hanger").AsEntity(network);

			network.SwitchToScaled();
			diagram.EarliestStartTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			startShape.Width = 100;
			ccShape.Width = 200;
			nonCCShape.Width = 100;
			endShape.Width = 100;

			startShape.MakeVisiblePrerequisiteOf(ccShape);
			startShape.MakeVisiblePrerequisiteOf(nonCCShape);
			ccShape.MakeVisiblePrerequisiteOf(endShape);
			nonCCShape.MakeVisiblePrerequisiteOf(endShape);

			ShapeFloatCalculator.CalculateFloatForChildren(network);
			AssertSchedule(startShape, earliestStart: 0m, latestStart: 0m, duration: 8m);
			AssertSchedule(ccShape, earliestStart: 8m, latestStart: 8m, duration: 16m);
			AssertSchedule(nonCCShape, earliestStart: 8m, latestStart: 16m, duration: 8m);
			AssertSchedule(endShape, earliestStart: 24m, latestStart: 24m, duration: 8m);
		}

		public void TestBasicFloatIdeaWorks_WithAWildlyRedundantPrereqLink()
		{
			var diagram = CreateDiagram(Factory, name: "root");
			var network = CreateNetwork(diagram);

			var startShape = CreateShape(diagram, "startShape").AsEntity(network);
			var ccShape = CreateShape(diagram, "ccShape").AsEntity(network);
			var endShape = CreateShape(diagram, "endShape").AsEntity(network);
			var nonCCShape = CreateShape(diagram, "hanger").AsEntity(network);

			network.SwitchToScaled();
			diagram.EarliestStartTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			startShape.Width = 100;
			ccShape.Width = 200;
			nonCCShape.Width = 100;
			endShape.Width = 100;

			startShape.MakeVisiblePrerequisiteOf(ccShape);
			startShape.MakeVisiblePrerequisiteOf(nonCCShape);
			startShape.MakeVisiblePrerequisiteOf(endShape);
			ccShape.MakeVisiblePrerequisiteOf(endShape);
			nonCCShape.MakeVisiblePrerequisiteOf(endShape);

			ShapeFloatCalculator.CalculateFloatForChildren(network);
			AssertSchedule(startShape, earliestStart: 0m, latestStart: 0m, duration: 8m);
			AssertSchedule(ccShape, earliestStart: 8m, latestStart: 8m, duration: 16m);
			AssertSchedule(nonCCShape, earliestStart: 8m, latestStart: 16m, duration: 8m);
			AssertSchedule(endShape, earliestStart: 24m, latestStart: 24m, duration: 8m);
		}

		public void TestShapesCanGetFloatFromParentDiagrams()
		{
			var diagram = CreateDiagram(Factory, name: "diagram");
			var network = CreateNetwork(diagram);
			var subDiagram1 = CreateShape(diagram, "subDiagram1").AsEntity(network);
			var shape1 = CreateShape(subDiagram1, "shape1");

			network.SwitchToScaled();
			diagram.EarliestStartTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			subDiagram1.Width = 200;
			shape1.Width = 100;

			ShapeFloatCalculator.CalculateFloatForChildren(network);

			AssertSchedule(shape1, earliestStart: 0m, latestStart: 8m, duration: 8m);
			AssertSchedule(subDiagram1, earliestStart: 0m, latestStart: 0m, duration: 16m);
		}

		public void TestLateSubdiagramOverhang()
		{
			var diagram = CreateDiagram(Factory, name: "root");
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var startShape = CreateShape(diagram, "startShape").AsEntity(network);
			var endShape = CreateShape(diagram, "endShape").AsEntity(network);
			var hangingSubdiagram = CreateShape(diagram, "HangingDiagram").AsEntity(network);
			var hungShape1 = CreateShape(hangingSubdiagram, "HungShape1");
			var hungShape2 = CreateShape(hangingSubdiagram, "HungShape2");

			network.SwitchToScaled();
			diagram.EarliestStartTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			startShape.Width = 100;
			endShape.Width = 100;
			hungShape1.Width = 100;
			hungShape2.Width = 100;
			hangingSubdiagram.Width = 300;

			startShape.MakeVisiblePrerequisiteOf(hangingSubdiagram);
			hungShape1.MakeVisiblePrerequisiteOf(hungShape2);
			hungShape2.MakeVisiblePrerequisiteOf(endShape);

			ShapeFloatCalculator.CalculateFloatForChildren(network);

			AssertSchedule(startShape, earliestStart: 0m, latestStart: 0m, duration: 8m);
			AssertSchedule("This shape has no float because it is locked in place by its children.", hangingSubdiagram, earliestStart: 8m, latestStart: 8m, duration: 24m);
			AssertSchedule(hungShape1, earliestStart: 8m, latestStart: 8m, duration: 8m);
			AssertSchedule(hungShape2, earliestStart: 16m, latestStart: 16m, duration: 8m);
			AssertSchedule(endShape, earliestStart: 24m, latestStart: 24m, duration: 8m);

			var newCCShape = networkViewModel.CreateNewShape(diagram);

			newCCShape.Width = 300;
			startShape.MakeVisiblePrerequisiteOf(newCCShape);
			newCCShape.MakeVisiblePrerequisiteOf(endShape);

			ShapeFloatCalculator.CalculateFloatForChildren(network);

			AssertSchedule(startShape, earliestStart: 0m, latestStart: 0m, duration: 8m);
			AssertSchedule(newCCShape, earliestStart: 8m, latestStart: 8m, duration: 24m);
			AssertSchedule(endShape, earliestStart: 32m, latestStart: 32m, duration: 8m);

			AssertSchedule("Now has float because its children aren't on the CC.", hangingSubdiagram, earliestStart: 8m, latestStart: 16m, duration: 24m);
			AssertSchedule(hungShape1, earliestStart: 8m, latestStart: 16m, duration: 8m);
			AssertSchedule(hungShape2, earliestStart: 16m, latestStart: 24m, duration: 8m);
		}

		public void TestEarlySubdiagramOverhang()
		{
			var diagramShape = CreateDiagram(Factory, name: "diagram");
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var startShape = CreateShape(diagram, "startShape");
			var endShape = CreateShape(diagram, "endShape");
			var hangingSubdiagram = CreateShape(diagram, "HangingDiagram");
			var hungShape1 = CreateShape(hangingSubdiagram, "HungShape1");
			var hungShape2 = CreateShape(hangingSubdiagram, "HungShape2");

			network.SwitchToScaled();
			diagram.ScheduledStartTimeLocal = ZDateTime.Now.AddDays(-1);

			startShape.Width = 100;
			endShape.Width = 100;
			hungShape1.Width = 100;
			hungShape2.Width = 100;
			hangingSubdiagram.Width = 300;

			startShape.MakeVisiblePrerequisiteOf(hungShape1);
			hungShape1.MakeVisiblePrerequisiteOf(hungShape2);
			hangingSubdiagram.MakeVisiblePrerequisiteOf(endShape);

			ShapeFloatCalculator.CalculateFloatForChildren(network);

			AssertSchedule(startShape, earliestStart: 0m, latestStart: 0m, duration: 8m);
			AssertSchedule("This shape has no float because it is locked in place by its children.", hangingSubdiagram, earliestStart: 0m, latestStart: 0m, duration: 24m);
			AssertSchedule(hungShape1, earliestStart: 8m, latestStart: 8m, duration: 8m);
			AssertSchedule(hungShape2, earliestStart: 16m, latestStart: 16m, duration: 8m);
			AssertSchedule(endShape, earliestStart: 24m, latestStart: 24m, duration: 8m);

			var newCCShape = networkViewModel.CreateNewShape(diagram);

			newCCShape.Width = 300;
			startShape.MakeVisiblePrerequisiteOf(newCCShape);
			newCCShape.MakeVisiblePrerequisiteOf(endShape);

			ShapeFloatCalculator.CalculateFloatForChildren(network);

			AssertSchedule(startShape, earliestStart: 0m, latestStart: 0m, duration: 8m);
			AssertSchedule(newCCShape, earliestStart: 8m, latestStart: 8m, duration: 24m);
			AssertSchedule(endShape, earliestStart: 32m, latestStart: 32m, duration: 8m);

			AssertSchedule("Now has float because its children aren't on the CC.", hangingSubdiagram, earliestStart: 0m, latestStart: 8m, duration: 24m);
			AssertSchedule(hungShape1, earliestStart: 8m, latestStart: 16m, duration: 8m);
			AssertSchedule(hungShape2, earliestStart: 16m, latestStart: 24m, duration: 8m);
		}

		public void TestDoubleSidedHang()
		{
			var diagramShape = CreateDiagram(Factory, name: "diagram");
			var network = CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;
			var startShape = CreateShape(diagram, "startShape");
			var endShape = CreateShape(diagram, "endShape");
			var hangingSubdiagram = CreateShape(diagram, "HangingDiagram");
			var hangerShape = CreateShape(hangingSubdiagram, "hanger");

			network.SwitchToScaled();
			diagram.ScheduledStartTimeLocal = ZDateTime.Now.AddDays(-1);

			startShape.Width = 200;
			endShape.Width = 200;
			hangerShape.Width = 100;

			startShape.MakeVisiblePrerequisiteOf(hangerShape);
			hangerShape.MakeVisiblePrerequisiteOf(endShape);

			hangingSubdiagram.Width = 200;
			ShapeFloatCalculator.CalculateFloatForChildren(network);

			AssertSchedule(startShape, earliestStart: 0m, latestStart: 0m, duration: 16m);
			AssertSchedule(hangerShape, earliestStart: 16m, latestStart: 16m, duration: 8m);
			AssertSchedule(endShape, earliestStart: 24m, latestStart: 24m, duration: 16m);
			AssertSchedule("This shape is allowed to 'float' around its child [hanger], but no further", hangingSubdiagram, earliestStart: 8m, latestStart: 16m, duration: 16m);

			hangingSubdiagram.Width = 300;
			ShapeFloatCalculator.CalculateFloatForChildren(network);

			AssertSchedule(startShape, earliestStart: 0m, latestStart: 0m, duration: 16m);
			AssertSchedule(hangerShape, earliestStart: 16m, latestStart: 16m, duration: 8m);
			AssertSchedule(endShape, earliestStart: 24m, latestStart: 24m, duration: 16m);
			AssertSchedule("It is still allowed to move freely around the child shape", hangingSubdiagram, earliestStart: 0m, latestStart: 16m, duration: 24m);

			hangingSubdiagram.Width = 400;
			ShapeFloatCalculator.CalculateFloatForChildren(network);

			AssertSchedule(startShape, earliestStart: 0m, latestStart: 0m, duration: 16m);
			AssertSchedule(hangerShape, earliestStart: 16m, latestStart: 16m, duration: 8m);
			AssertSchedule(endShape, earliestStart: 24m, latestStart: 24m, duration: 16m);
			AssertSchedule("Now it is so wide that it cannot move too far without becoming the CC.", hangingSubdiagram, earliestStart: 0m, latestStart: 8m, duration: 32m);
		}

		public void TestDoubleSubdiagramHangWithProp()
		{
			var diagramShape = CreateDiagram(Factory, name: "diagram");
			var network = CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;
			var startShape = CreateShape(diagram, "startShape");
			var endShape = CreateShape(diagram, "endShape");
			var hangingSubdiagram = CreateShape(diagram, "HangingDiagram");
			var innerHangingSubdiagram = CreateShape(hangingSubdiagram, "InnerHangingDiagram");
			var innerHangingProp = CreateShape(hangingSubdiagram, "InnerHangingProp");
			var hangerShape = CreateShape(hangingSubdiagram, "hanger");

			network.SwitchToScaled();
			diagram.ScheduledStartTimeLocal = ZDateTime.Now.AddDays(-1);

			startShape.Width = 300;
			endShape.Width = 300;
			hangerShape.Width = 100;
			innerHangingProp.Width = 100;
			innerHangingSubdiagram.Width = 200;
			hangingSubdiagram.Width = 300;

			startShape.MakeVisiblePrerequisiteOf(hangerShape);
			hangerShape.MakeVisiblePrerequisiteOf(endShape);
			innerHangingSubdiagram.MakeVisiblePrerequisiteOf(innerHangingProp);

			ShapeFloatCalculator.CalculateFloatForChildren(network);

			AssertSchedule(startShape, earliestStart: 0m, latestStart: 0m, duration: 24m);
			AssertSchedule(hangerShape, earliestStart: 24m, latestStart: 24m, duration: 8m);
			AssertSchedule(endShape, earliestStart: 32m, latestStart: 32m, duration: 24m);
			AssertSchedule("This shape is allowed to 'float' around its child [hanger], but no further", hangingSubdiagram, earliestStart: 8m, latestStart: 24m, duration: 24m);
			AssertSchedule(innerHangingSubdiagram, earliestStart: 8, latestStart: 24m, duration: 16m);
			AssertSchedule(innerHangingProp, earliestStart: 24m, latestStart: 40m, duration: 8m);
		}

		public void TestSubdiagramWithPropAndInternalDependencies()
		{
			var diagramShape = CreateDiagram(Factory, name: "diagram");
			var network = CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;
			var startShape = CreateShape(diagram, "startShape");
			var endShape = CreateShape(diagram, "endShape");
			var subDiagram = CreateShape(diagram, "subDiagram");
			var innerShape1 = CreateShape(subDiagram, "Shape1");
			var innerShape2 = CreateShape(subDiagram, "Shape2");

			network.SwitchToScaled();
			diagram.ScheduledStartTimeLocal = ZDateTime.Now.AddDays(-1);

			startShape.Width = 400;
			endShape.Width = 100;
			subDiagram.Width = 300;
			innerShape1.Width = 100;
			innerShape2.Width = 100;

			startShape.MakeVisiblePrerequisiteOf(innerShape2);
			innerShape1.MakeVisiblePrerequisiteOf(innerShape2);
			subDiagram.MakeVisiblePrerequisiteOf(endShape);

			ShapeFloatCalculator.CalculateFloatForChildren(network);

			AssertSchedule(startShape, earliestStart: 0m, latestStart: 0m, duration: 32m);
			AssertSchedule(innerShape1, earliestStart: 16m, latestStart: 24m, duration: 8m);
			AssertSchedule(innerShape2, earliestStart: 32m, latestStart: 32m, duration: 8m);
			AssertSchedule(subDiagram, earliestStart: 16m, latestStart: 16m, duration: 24m);
			AssertSchedule(endShape, earliestStart: 40m, latestStart: 40m, duration: 8m);
		}

		public void TestSubdiagramHasExternalDependencyHalfWayThrough()
		{
			var diagramShape = CreateDiagram(Factory, name: "diagram");
			var network = CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;
			var startShape = CreateShape(diagram, "startShape");
			var endShape = CreateShape(diagram, "endShape");
			var subDiagram = CreateShape(diagram, "subDiagram");

			var innerShape1 = CreateShape(subDiagram, "Shape1");
			var middlyExternalDep = CreateShape(diagram, "Middly");
			var innerShape2 = CreateShape(subDiagram, "Shape2");

			network.SwitchToScaled();
			diagram.ScheduledStartTimeLocal = ZDateTime.Now.AddDays(-1);

			startShape.Width = 300;
			endShape.Width = 300;
			subDiagram.Width = 300;
			innerShape1.Width = 100;
			middlyExternalDep.Width = 100;
			innerShape2.Width = 100;

			startShape.MakeVisiblePrerequisiteOf(middlyExternalDep);
			innerShape1.MakeVisiblePrerequisiteOf(middlyExternalDep);
			middlyExternalDep.MakeVisiblePrerequisiteOf(innerShape2);
			middlyExternalDep.MakeVisiblePrerequisiteOf(endShape);

			ShapeFloatCalculator.CalculateFloatForChildren(network);

			AssertSchedule(startShape, earliestStart: 0m, latestStart: 0m, duration: 24m);
			AssertSchedule(innerShape1, earliestStart: 16m, latestStart: 16m, duration: 8m);
			AssertSchedule(middlyExternalDep, earliestStart: 24m, latestStart: 24m, duration: 8m);
			AssertSchedule(innerShape2, earliestStart: 32m, latestStart: 32m, duration: 8m);
			AssertSchedule(subDiagram, earliestStart: 16m, latestStart: 16m, duration: 24m);
			AssertSchedule(endShape, earliestStart: 32m, latestStart: 32m, duration: 24m);
		}

		public void TestRefreshPropertiesOnlyWhenNecessary_SetOnSchedule()
		{
			var diagram = CreateDiagram(Factory, name: "root");
			var network = CreateNetwork(diagram);

			var startShape = CreateShape(diagram, "startShape").AsEntity(network);
			var ccShape = CreateShape(diagram, "ccShape").AsEntity(network);
			var endShape = CreateShape(diagram, "endShape").AsEntity(network);

			network.SwitchToScaled();
			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			startShape.Width = 100;
			ccShape.Width = 200;
			endShape.Width = 100;

			int valueChangedCount = 0;
			System.EventHandler handler = (s, e) =>
			{
				valueChangedCount++;
			};

			startShape.Shape.IsScaledInfo.ValueChanged += handler;
			startShape.Shape.ExplicitDurationMinutesInfo.ValueChanged += handler;
			startShape.Shape.NCNReleaseOffsetMinutesInfo.ValueChanged += handler;
			startShape.Shape.BufferPenetrationInfo.ValueChanged += handler;
			startShape.Shape.PenetratingBufferSizeInMinutesInfo.ValueChanged += handler;
			startShape.Shape.EarliestStartHoursInfo.ValueChanged += handler;
			startShape.Shape.LatestStartHoursInfo.ValueChanged += handler;
			startShape.Shape.EarliestStartTimeUtcInfo.ValueChanged += handler;
			startShape.Shape.EarliestFinishTimeUtcInfo.ValueChanged += handler;
			startShape.Shape.LatestStartTimeUtcInfo.ValueChanged += handler;
			startShape.Shape.LatestStartTimeUtcInfo.ValueChanged += handler;
			startShape.Shape.LatestFinishTimeUtcInfo.ValueChanged += handler;
			startShape.Shape.IsCriticalPathInfo.ValueChanged += handler;
			startShape.Shape.IsBufferedInfo.ValueChanged += handler;
			startShape.Shape.ScheduledStartTimeUtcInfo.ValueChanged += handler;
			startShape.Shape.ScheduledFinishTimeUtcInfo.ValueChanged += handler;

			startShape.MakeVisiblePrerequisiteOf(ccShape);
			startShape.MakeVisiblePrerequisiteOf(endShape);
			ccShape.MakeVisiblePrerequisiteOf(endShape);

			ShapeFloatCalculator.CalculateFloatForChildren(network);
			AssertEquals(5, valueChangedCount);
		}

		#endregion

		#region More complex Float Tests

		public void TestCrossDependency_MultiFallback()
		{
			var diagramShape = CreateDiagram(Factory, name: "diagram");
			var network = CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;
			var subdiagram1 = CreateShape(diagram, "SubDiagram1");
			var subdiagram2 = CreateShape(diagram, "SubDiagram2");

			var sub1StartShape = CreateShape(subdiagram1, "sub1Start");
			var sub1MiddlyShape = CreateShape(subdiagram1, "sub1Middly");

			var sub2StartShape = CreateShape(subdiagram2, "sub2Start");
			var sub2EndShape = CreateShape(subdiagram2, "sub2End");

			network.SwitchToScaled();
			diagram.ScheduledStartTimeLocal = ZDateTime.Now.AddDays(-1);

			subdiagram1.Width = subdiagram2.Width = 300;
			sub1StartShape.Width = sub1MiddlyShape.Width = sub2StartShape.Width = sub2EndShape.Width = 100;

			sub1StartShape.MakeVisiblePrerequisiteOf(sub1MiddlyShape);
			sub2StartShape.MakeVisiblePrerequisiteOf(sub1MiddlyShape);
			sub1MiddlyShape.MakeVisiblePrerequisiteOf(sub2EndShape);

			ShapeFloatCalculator.CalculateFloatForChildren(network);
			AssertSchedule(subdiagram1, earliestStart: 0m, latestStart: 0m, duration: 24m);
			AssertSchedule(subdiagram2, earliestStart: 0m, latestStart: 0m, duration: 24m);
			AssertSchedule(sub1StartShape, earliestStart: 0m, latestStart: 0m, duration: 8m);
			AssertSchedule(sub1MiddlyShape, earliestStart: 8m, latestStart: 8m, duration: 8m);
			AssertSchedule(sub2StartShape, earliestStart: 0m, latestStart: 0m, duration: 8m);
			AssertSchedule(sub2EndShape, earliestStart: 16m, latestStart: 16m, duration: 8m);
		}

		public void TestCrossDependency_WithSkew_MultiFallback()
		{
			var diagramShape = CreateDiagram(Factory, name: "diagram");
			var network = CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;
			var subdiagram1 = CreateShape(diagram, "SubDiagram1");
			var subdiagram2 = CreateShape(diagram, "SubDiagram2");

			var skewShape = CreateShape(diagram, name: "Skew Yew");
			var sub1StartShape = CreateShape(subdiagram1, "sub1Start");
			var sub1EndShape = CreateShape(subdiagram1, "sub1End");

			var sub2StartShape = CreateShape(subdiagram2, "sub2Start");
			var sub2EndShape = CreateShape(subdiagram2, "sub2End");

			network.SwitchToScaled();
			diagram.ScheduledStartTimeLocal = ZDateTime.Now.AddDays(-1);

			subdiagram1.Width = subdiagram2.Width = 300;
			skewShape.Width = sub1StartShape.Width = sub1EndShape.Width = sub2StartShape.Width = sub2EndShape.Width = 100;

			skewShape.MakeVisiblePrerequisiteOf(subdiagram1);
			sub1StartShape.MakeVisiblePrerequisiteOf(sub2EndShape);
			sub2StartShape.MakeVisiblePrerequisiteOf(sub1EndShape);

			ShapeFloatCalculator.CalculateFloatForChildren(network);
			AssertSchedule(subdiagram1, earliestStart: 8m, latestStart: 8m, duration: 24m);
			AssertSchedule(subdiagram2, earliestStart: 0m, latestStart: 8m, duration: 24m);
			AssertSchedule(sub1StartShape, earliestStart: 8m, latestStart: 16m, duration: 8m);
			AssertSchedule(sub1EndShape, earliestStart: 8m, latestStart: 24m, duration: 8m);
			AssertSchedule(sub2StartShape, earliestStart: 0m, latestStart: 16m, duration: 8m);
			AssertSchedule(sub2EndShape, earliestStart: 16m, latestStart: 24m, duration: 8m);
		}

		public void TestCrossDependency()
		{
			var diagramShape = CreateDiagram(Factory, name: "diagram");
			var network = CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;
			var subDiagram1 = CreateShape(diagram, "subDiagram1");
			var subDiagram2 = CreateShape(diagram, "subDiagram2");

			var shape1_1 = CreateShape(subDiagram1, "shape1_1");
			var shape2_1 = CreateShape(subDiagram2, "shape2_1");

			shape1_1.MakeVisiblePrerequisiteOf(subDiagram2);

			network.SwitchToScaled();
			diagram.ScheduledStartTimeLocal = ZDateTime.Now.AddDays(-1);

			subDiagram1.Width = subDiagram2.Width = 300;
			shape1_1.Width = shape2_1.Width = 200;

			ShapeFloatCalculator.CalculateFloatForChildren(network);
			AssertSchedule(subDiagram1, earliestStart: 0m, latestStart: 0m, duration: 24m);
			AssertSchedule(subDiagram2, earliestStart: 16m, latestStart: 16m, duration: 24m);
			AssertSchedule(shape1_1, earliestStart: 0m, latestStart: 0m, duration: 16m);
			AssertSchedule(shape2_1, earliestStart: 16m, latestStart: 24m, duration: 16m);
		}

		public void TestCrossDependency_ShouldNotRaiseException()
		{
			var diagramShape = CreateDiagram(Factory, name: "diagram");
			var network = CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;

			var shape1 = CreateShape(diagram, "shape1");
			var shape2 = CreateShape(diagram, "shape2");
			var shape3 = CreateShape(diagram, "shape3");
			var shape4 = CreateShape(diagram, "shape4");

			var shape1_1 = CreateShape(shape1, "shape1_1");
			var shape1_2 = CreateShape(shape1, "shape1_2");

			var shape3_1 = CreateShape(shape3, "shape3_1");
			var shape3_2 = CreateShape(shape3, "shape3_2");

			var shape4_1 = CreateShape(shape4, "shape4_1");
			var shape4_2 = CreateShape(shape4, "shape4_2");

			shape1_1.MakeVisiblePrerequisiteOf(shape2);
			shape3_1.MakeVisiblePrerequisiteOf(shape2);
			shape4_1.MakeVisiblePrerequisiteOf(shape2);
			shape2.MakeVisiblePrerequisiteOf(shape1_2);
			shape2.MakeVisiblePrerequisiteOf(shape3_2);
			shape2.MakeVisiblePrerequisiteOf(shape4_2);

			network.SwitchToScaled();
			diagram.ScheduledStartTimeLocal = ZDateTime.Now.AddDays(-1);

			ShapeFloatCalculator.CalculateFloatForChildren(network);
			AssertSchedule(diagram, earliestStart: 0m, latestStart: 0m, duration: 24m);
		}

		public void TestCrossDependency_PushEntitiesEarly()
		{
			var diagramShape = CreateDiagram(Factory, name: "diagram");
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();

			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;

			var subDiagram = CreateShape(diagram, "subDiagram");
			SetShapeOffset(subDiagram, diagram, 200, 0);
			SetShapeSize(subDiagram, diagram, 300, 50);

			var shape1 = CreateShape(subDiagram, "shape1");
			SetShapeOffset(shape1, subDiagram, 0, 0);
			SetShapeSize(shape1, subDiagram, 100, 50);

			var shape2 = CreateShape(subDiagram, "shape2");
			SetShapeOffset(shape2, subDiagram, 100, 0);
			SetShapeSize(shape2, subDiagram, 200, 50);
			shape1.MakeVisiblePrerequisiteOf(shape2);

			var shape3 = CreateShape(diagram, "shape3");
			SetShapeOffset(shape3, diagram, 0, 50);
			SetShapeSize(shape3, diagram, 100, 50);
			shape3.MakeVisiblePrerequisiteOf(shape1);

			networkViewModel.PushAsEarlyAsPossible();

			AssertSchedule(subDiagram, earliestStart: 8m, latestStart: 8m, duration: 24m);
			AssertSchedule(shape1, earliestStart: 8m, latestStart: 8m, duration: 8m);
			AssertSchedule(shape2, earliestStart: 16m, latestStart: 16m, duration: 16m);
		}

		public void TestCrossDependency_DoesNotDuplicateEdges()
		{
			var diagramShape = CreateDiagram(Factory, name: "diagram");
			var network = CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;
			var subDiagram1 = CreateShape(diagram, "subDiagram1");
			var subDiagram2 = CreateShape(diagram, "subDiagram2");

			var shape1_1 = CreateShape(subDiagram1, "shape1_1");
			var shape1_2 = CreateShape(subDiagram1, "shape1_2");
			var shape2_1 = CreateShape(subDiagram2, "shape2_1");

			shape1_1.MakeVisiblePrerequisiteOf(subDiagram2);
			shape1_2.MakeVisiblePrerequisiteOf(subDiagram2);

			network.SwitchToScaled();

			var snapshot = new ScheduleNodeSnapshot(network);

			snapshot.Calculate();
			AssertEquals("Cross dependency edges should not be duplicated", 8, snapshot.ScheduleNetwork.Graph.EdgeCount);
		}

		#endregion

		#region Performance

		[ExpectNoExceptions]
		public void TestSetSchedule_WhenNothingHasChanged_ShouldNotRefreshShape()
		{
			var diagram = CreateDiagram(Factory);
			var shape1 = CreateShape(diagram);
			var shape2 = CreateShape(diagram);
			var shape3 = CreateShape(diagram);

			var network = CreateNetwork(diagram);
			network.SwitchToScaled();

			// Ensure schedules are constructed
			ShapeFloatCalculator.CalculateFloatForChildren(network);

			network.Entities.GetInstance(diagram).PropertyChanged += DontCallPropertyChanged_Okay;
			network.Entities.GetInstance(shape1).PropertyChanged += DontCallPropertyChanged_Okay;
			network.Entities.GetInstance(shape2).PropertyChanged += DontCallPropertyChanged_Okay;
			network.Entities.GetInstance(shape3).PropertyChanged += DontCallPropertyChanged_Okay;

			ShapeFloatCalculator.CalculateFloatForChildren(network);
		}

		void DontCallPropertyChanged_Okay(object sender, PropertyChangedEventArgs e)
		{
			Fail("There's no point calling PropertyChanged here because nothing has changed.");
		}

		[TestDate(2015, 7, 14)]
		public void TestCalculateSchedules_WhenNetworkHasNoDate_ShouldNotIncludeDates()
		{
			var diagram = CreateDiagram(Factory);
			var shape1 = CreateShape(diagram);
			var shape2 = CreateShape(diagram);
			var shape3 = CreateShape(diagram);

			shape1.MakeVisiblePrerequisiteOf(shape2, diagram);
			shape2.MakeVisiblePrerequisiteOf(shape3, diagram);

			var network = CreateNetwork(diagram);
			network.SwitchToScaled();

			ShapeFloatCalculator.CalculateFloatForChildren(network);

			AssertEquals(0m, shape1.EarliestStartHours);
			AssertEquals(24m, shape2.EarliestStartHours);
			AssertEquals(48m, shape3.EarliestStartHours);

			AssertEquals(24m, shape1.EarliestFinishHours);
			AssertEquals(48m, shape2.EarliestFinishHours);
			AssertEquals(72m, shape3.EarliestFinishHours);

			AssertEquals(ZDateTime.Empty, shape1.EarliestStartTimeUtc);
			AssertEquals(ZDateTime.Empty, shape2.EarliestStartTimeUtc);
			AssertEquals(ZDateTime.Empty, shape3.EarliestStartTimeUtc);

			AssertEquals(ZDateTime.Empty, shape1.EarliestFinishTimeUtc);
			AssertEquals(ZDateTime.Empty, shape2.EarliestFinishTimeUtc);
			AssertEquals(ZDateTime.Empty, shape3.EarliestFinishTimeUtc);

			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow;
			ShapeFloatCalculator.CalculateFloatForChildren(network);

			AssertEquals(ZDateTime.UtcNow, shape1.EarliestStartTimeUtc);
			AssertEquals(ZDateTime.UtcNow.AddHours(24), shape2.EarliestStartTimeUtc);
			AssertEquals(ZDateTime.UtcNow.AddHours(48), shape3.EarliestStartTimeUtc);

			AssertEquals(ZDateTime.UtcNow.AddHours(24), shape1.EarliestFinishTimeUtc);
			AssertEquals(ZDateTime.UtcNow.AddHours(48), shape2.EarliestFinishTimeUtc);
			AssertEquals(ZDateTime.UtcNow.AddHours(72), shape3.EarliestFinishTimeUtc);
		}

		[ExpectNoExceptions]
		public void TestCalculateSchedulesForChildrenWithCertainConfig_ShouldThrowNoExceptions()
		{
			var diagram = CreateDiagram(Factory);
			diagram.IsScaled = true;

			var shape1 = CreateShape(diagram);
			var shape2 = CreateShape(diagram);
			var shape2_1 = CreateShape(shape2);
			var shape2_1_1 = CreateShape(shape2_1);
			var shape2_2 = CreateShape(shape2);
			var shape2_2_1 = CreateShape(shape2_2);

			var network = CreateNetwork(diagram);
			shape1.AsEntity(network).MakeVisiblePrerequisiteOf(shape2_1_1.AsEntity(network));
			shape2_1.AsEntity(network).MakeVisiblePrerequisiteOf(shape2_2_1.AsEntity(network));

			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow;
			ShapeFloatCalculator.CalculateFloatForChildren(network);
		}

		#endregion

		#region Implementation

		void AssertSchedule(ShapeNetworkEntity shape, decimal earliestStart, decimal latestStart, decimal duration)
		{
			AssertSchedule("", shape, earliestStart, latestStart, duration);
		}

		void AssertSchedule(string message, ShapeNetworkEntity shape, decimal earliestStart, decimal latestStart, decimal duration)
		{
			var node = shape.Schedule;
			CombineAssertions(message, () =>
			{
				AssertEquals("earliestStart", earliestStart, node.EarliestStartHours);
				AssertEquals("latestStart", latestStart, node.LatestStartHours);
				AssertEquals("duration", duration, node.EstimatedDurationHoursIncludingChildren);
			});
		}

		#endregion
	}
}
