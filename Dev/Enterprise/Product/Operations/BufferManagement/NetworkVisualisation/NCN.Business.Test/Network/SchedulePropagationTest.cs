using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestUtcOffset(10, 0, 0)]
	[TestDate(2019, 1, 1)]
	class SchedulePropagationTest : NetworkTestCase
	{
		#region Valid Shape Edits

		public void TestMoveShape_WhenLinkedToScaledDiagram_ShouldPropagateSchedule()
		{
			PerformShapeEditOnShapeLinkedToScaledDiagram_AssertingOutcomeOnSchedule(
				(network, shapeLinkedToScaledDiagram, scaledDiagram) => NetworkTestCase.SetShapeOffset(shapeLinkedToScaledDiagram, scaledDiagram, network, 100, 0),
				ZDateTime.Now.AddDays(1), ZDateTime.Now.AddDays(4),
				ZDateTime.Now.AddDays(1), ZDateTime.Now.AddDays(4)
				);
		}

		public void TestResizeShape_WhenLinkedToScaledDiagram_ShouldPropagateSchedule()
		{
			PerformShapeEditOnShapeLinkedToScaledDiagram_AssertingOutcomeOnSchedule(
				(network, shapeLinkedToScaledDiagram, scaledDiagram) => NetworkTestCase.SetShapeSize(network, shapeLinkedToScaledDiagram, scaledDiagram, 400, 100),
				ZDateTime.Now, ZDateTime.Now.AddDays(4),
				ZDateTime.Now, ZDateTime.Now.AddDays(4)
				);
		}

		public void TestResizeShape_WhenLinkedToScaledDiagram_ButPropagationFlagDisabled_ShouldNotPropagateSchedule()
		{
			PerformShapeEditOnShapeLinkedToScaledDiagram_AssertingOutcomeOnSchedule(
				(network, shapeLinkedToScaledDiagram, scaledDiagram) =>
				{
					shapeLinkedToScaledDiagram.ShouldSynchroniseScheduleWithLinkedEntity = false;
					NetworkTestCase.SetShapeSize(network, shapeLinkedToScaledDiagram, scaledDiagram, 400, 100);
				},
				ZDateTime.Now, ZDateTime.Now.AddDays(4),
				ZDateTime.Now, ZDateTime.Now.AddDays(3)
				);
		}

		delegate void ShapeEditAction(JobNetwork network, BMNCNShape shapeLinkedToScaledDiagram, BMNCNRootDiagramShape scaledDiagram);

		void PerformShapeEditOnShapeLinkedToScaledDiagram_AssertingOutcomeOnSchedule(ShapeEditAction shapeEditAction, ZDateTime expectedShapeStartTimeAfterEdit, ZDateTime expectedShapeFinishTimeAfterEdit, ZDateTime expectedLinkedDiagramStartTimeAfterEdit, ZDateTime expectedLinkedDiagramFinishTimeAfterEdit)
		{
			var scaledDiagram1 = NetworkTestCase.CreateDiagram(Factory, isScaled: true, scheduledStartTimeUTC: ZDateTime.UtcNow);
			var shape = NetworkTestCase.CreateShape(scaledDiagram1);

			var scaledDiagram2 = NetworkTestCase.CreateDiagram(Factory, isScaled: true);

			var network = NetworkTestCase.CreateNetwork(scaledDiagram1);
			NetworkTestCase.SetShapeSize(network, shape, scaledDiagram1, 300, 100);

			network.RefreshSchedules();
			network.LinkEntity(shape, scaledDiagram2);

			Factory.Save();

			CombineAssertions("Pre-condition: shape has schedule dates set, but they're not yet propagated to the linked diagram", () =>
			{
				AssertEquals("Shape Scheduled Start", ZDateTime.Now, shape.ScheduledStartTimeLocal);
				AssertEquals("Shape Scheduled Finish", ZDateTime.Now.AddDays(3), shape.ScheduledFinishTimeLocal);

				AssertEquals("Linked diagram Scheduled Start", ZDateTime.Empty, scaledDiagram2.ScheduledStartTimeLocal);
				AssertEquals("Linked diagram Scheduled Finish", ZDateTime.Empty, scaledDiagram2.ScheduledFinishTimeLocal);
			});

			network.Refresh(RefreshType.Saving);
			Factory.Save();

			CombineAssertions("Now that the network has been saved, schedule should be propagated to the linked diagram", () =>
			{
				AssertEquals("Shape Scheduled Start", ZDateTime.Now, shape.ScheduledStartTimeLocal);
				AssertEquals("Shape Scheduled Finish", ZDateTime.Now.AddDays(3), shape.ScheduledFinishTimeLocal);

				AssertEquals("Linked diagram Scheduled Start", ZDateTime.Now, scaledDiagram2.ScheduledStartTimeLocal);
				AssertEquals("Linked diagram Scheduled Finish", ZDateTime.Now.AddDays(3), scaledDiagram2.ScheduledFinishTimeLocal);
			});

			shapeEditAction(network, shape, scaledDiagram1);
			network.Refresh(RefreshType.Saving);
			Factory.Save();

			CombineAssertions("After editing the shape and saving the network, the shape's schedule should be updated, and it should also be propagated to the linked diagram", () =>
			{
				AssertEquals("Shape Scheduled Start", expectedShapeStartTimeAfterEdit, shape.ScheduledStartTimeLocal);
				AssertEquals("Shape Scheduled Finish", expectedShapeFinishTimeAfterEdit, shape.ScheduledFinishTimeLocal);

				AssertEquals("Linked diagram Scheduled Start", expectedLinkedDiagramStartTimeAfterEdit, scaledDiagram2.ScheduledStartTimeLocal);
				AssertEquals("Linked diagram Scheduled Finish", expectedLinkedDiagramFinishTimeAfterEdit, scaledDiagram2.ScheduledFinishTimeLocal);
			});
		}

		public void TestMoveShape_WhenLinkedToScaledDiagram_AndScaledDiagramContainsChildShapes_ShouldPropagateScheduleToLinkedDiagramAndAllNestedChildren()
		{
			var scaledDiagram1 = NetworkTestCase.CreateDiagram(Factory, isScaled: true, scheduledStartTimeUTC: ZDateTime.UtcNow);
			var shapeOnScaledDiagram1 = NetworkTestCase.CreateShape(scaledDiagram1);

			var network1 = NetworkTestCase.CreateNetwork(scaledDiagram1);
			NetworkTestCase.SetShapeSize(network1, shapeOnScaledDiagram1, scaledDiagram1, 1200, 400);
			network1.RefreshSchedules();

			var scaledDiagram2 = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var shapeOnScaledDiagram2_1 = NetworkTestCase.CreateShape(scaledDiagram2);
			var shapeOnScaledDiagram2_2 = NetworkTestCase.CreateShape(scaledDiagram2);
			var shapeOnScaledDiagram2_3 = NetworkTestCase.CreateShape(scaledDiagram2);
			var shapeOnScaledDiagram2_4 = NetworkTestCase.CreateShape(shapeOnScaledDiagram2_3);

			var network2 = NetworkTestCase.CreateNetwork(scaledDiagram2);
			NetworkTestCase.SetShapeSize(network2, shapeOnScaledDiagram2_1, scaledDiagram2, 300, 100);
			NetworkTestCase.SetShapeSize(network2, shapeOnScaledDiagram2_2, scaledDiagram2, 300, 100);
			NetworkTestCase.SetShapeSize(network2, shapeOnScaledDiagram2_3, scaledDiagram2, 600, 200);
			NetworkTestCase.SetShapeSize(network2, shapeOnScaledDiagram2_4, shapeOnScaledDiagram2_3, 300, 100);
			NetworkTestCase.SetShapeOffset(shapeOnScaledDiagram2_2, scaledDiagram2, network2, 300, 0);
			NetworkTestCase.SetShapeOffset(shapeOnScaledDiagram2_3, scaledDiagram2, network2, 600, 0);
			NetworkTestCase.SetShapeOffset(shapeOnScaledDiagram2_4, shapeOnScaledDiagram2_3, network2, 200, 0);

			shapeOnScaledDiagram2_1.MakeVisiblePrerequisiteOf(shapeOnScaledDiagram2_2, scaledDiagram2);
			shapeOnScaledDiagram2_2.MakeVisiblePrerequisiteOf(shapeOnScaledDiagram2_3, scaledDiagram2);

			network2.RefreshSchedules();
			Factory.Save();

			network1.LinkEntity(shapeOnScaledDiagram1, scaledDiagram2, null, autoShowRelationships: false);
			network1.Refresh(RefreshType.Saving);
			Factory.Save();

			CombineAssertions("Now that the network has been saved, schedule should be propagated to the linked diagram, and all its nested children shapes", () =>
			{
				AssertEquals("shapeOnScaledDiagram1 Scheduled Start", ZDateTime.Now, shapeOnScaledDiagram1.ScheduledStartTimeLocal);
				AssertEquals("shapeOnScaledDiagram1 Scheduled Finish", ZDateTime.Now.AddDays(12), shapeOnScaledDiagram1.ScheduledFinishTimeLocal);

				AssertEquals("scaledDiagram2 Scheduled Start", ZDateTime.Now, scaledDiagram2.ScheduledStartTimeLocal);
				AssertEquals("scaledDiagram2 Scheduled Finish", ZDateTime.Now.AddDays(12), scaledDiagram2.ScheduledFinishTimeLocal);

				AssertEquals("shapeOnScaledDiagram2_1 Scheduled Start", ZDateTime.Now, shapeOnScaledDiagram2_1.ScheduledStartTimeLocal);
				AssertEquals("shapeOnScaledDiagram2_1 Scheduled Finish", ZDateTime.Now.AddDays(3), shapeOnScaledDiagram2_1.ScheduledFinishTimeLocal);

				AssertEquals("shapeOnScaledDiagram2_2 Scheduled Start", ZDateTime.Now.AddDays(3), shapeOnScaledDiagram2_2.ScheduledStartTimeLocal);
				AssertEquals("shapeOnScaledDiagram2_2 Scheduled Finish", ZDateTime.Now.AddDays(6), shapeOnScaledDiagram2_2.ScheduledFinishTimeLocal);

				AssertEquals("shapeOnScaledDiagram2_3 Scheduled Start", ZDateTime.Now.AddDays(6), shapeOnScaledDiagram2_3.ScheduledStartTimeLocal);
				AssertEquals("shapeOnScaledDiagram2_3 Scheduled Finish", ZDateTime.Now.AddDays(12), shapeOnScaledDiagram2_3.ScheduledFinishTimeLocal);

				AssertEquals("shapeOnScaledDiagram2_4 Scheduled Start", ZDateTime.Now.AddDays(8), shapeOnScaledDiagram2_4.ScheduledStartTimeLocal);
				AssertEquals("shapeOnScaledDiagram2_4 Scheduled Finish", ZDateTime.Now.AddDays(11), shapeOnScaledDiagram2_4.ScheduledFinishTimeLocal);
			});

			NetworkTestCase.SetShapeSize(network1, shapeOnScaledDiagram1, scaledDiagram1, 1200, 400);
			NetworkTestCase.SetShapeOffset(shapeOnScaledDiagram1, scaledDiagram1, network1, 100, 0);
			network1.Refresh(RefreshType.Saving);
			Factory.Save();

			CombineAssertions("Now that the shape linked to the scaled diagram has been moved and the network saved, the updated schedule should be propagated to the linked diagram, and all its nested children shapes", () =>
			{
				AssertEquals("shapeOnScaledDiagram1 Scheduled Start", ZDateTime.Now.AddDays(1), shapeOnScaledDiagram1.ScheduledStartTimeLocal);
				AssertEquals("shapeOnScaledDiagram1 Scheduled Finish", ZDateTime.Now.AddDays(13), shapeOnScaledDiagram1.ScheduledFinishTimeLocal);

				AssertEquals("scaledDiagram2 Scheduled Start", ZDateTime.Now.AddDays(1), scaledDiagram2.ScheduledStartTimeLocal);
				AssertEquals("scaledDiagram2 Scheduled Finish", ZDateTime.Now.AddDays(13), scaledDiagram2.ScheduledFinishTimeLocal);

				AssertEquals("shapeOnScaledDiagram2_1 Scheduled Start", ZDateTime.Now.AddDays(1), shapeOnScaledDiagram2_1.ScheduledStartTimeLocal);
				AssertEquals("shapeOnScaledDiagram2_1 Scheduled Finish", ZDateTime.Now.AddDays(4), shapeOnScaledDiagram2_1.ScheduledFinishTimeLocal);

				AssertEquals("shapeOnScaledDiagram2_2 Scheduled Start", ZDateTime.Now.AddDays(4), shapeOnScaledDiagram2_2.ScheduledStartTimeLocal);
				AssertEquals("shapeOnScaledDiagram2_2 Scheduled Finish", ZDateTime.Now.AddDays(7), shapeOnScaledDiagram2_2.ScheduledFinishTimeLocal);

				AssertEquals("shapeOnScaledDiagram2_3 Scheduled Start", ZDateTime.Now.AddDays(7), shapeOnScaledDiagram2_3.ScheduledStartTimeLocal);
				AssertEquals("shapeOnScaledDiagram2_3 Scheduled Finish", ZDateTime.Now.AddDays(13), shapeOnScaledDiagram2_3.ScheduledFinishTimeLocal);

				AssertEquals("shapeOnScaledDiagram2_4 Scheduled Start", ZDateTime.Now.AddDays(9), shapeOnScaledDiagram2_4.ScheduledStartTimeLocal);
				AssertEquals("shapeOnScaledDiagram2_4 Scheduled Finish", ZDateTime.Now.AddDays(12), shapeOnScaledDiagram2_4.ScheduledFinishTimeLocal);
			});
		}

		public void TestPropagateToMultipleLinkedDiagrams_ShouldInvokeProgressReporter()
		{
			var mainDiagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var shape1 = NetworkTestCase.CreateShape(mainDiagram, name: "Shape1");
			var shape2 = NetworkTestCase.CreateShape(mainDiagram, name: "Shape2");
			var shape3 = NetworkTestCase.CreateShape(mainDiagram, name: "Shape3");
			var shape4 = NetworkTestCase.CreateShape(mainDiagram, name: "Shape4");
			var shape5 = NetworkTestCase.CreateShape(mainDiagram, name: "Shape5");
			var shape6 = NetworkTestCase.CreateShape(mainDiagram, name: "Shape6");

			var otherDiagram1 = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var otherDiagram2 = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var otherDiagram3 = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var otherDiagram4 = NetworkTestCase.CreateDiagram(Factory, isScaled: true);

			NetworkTestCase.LinkToRelatedDiagram(shape1, otherDiagram1);
			NetworkTestCase.LinkToRelatedDiagram(shape2, otherDiagram2);
			NetworkTestCase.LinkToRelatedDiagram(shape3, otherDiagram3);
			NetworkTestCase.LinkToRelatedDiagram(shape4, otherDiagram4);

			Factory.Save();

			var reporter = new DummyProgressReporter();
			var reporterProvider = new Mock<IProgressReporterProvider>(MockBehavior.Strict);

			reporterProvider.Setup(m => m.CreateProgressReporter("Propagating scheduling changes to linked diagrams", 4, true)).Returns(reporter);
			var networkViewModel = CreateNetworkViewModel(mainDiagram, progressReporterProvider: reporterProvider.Object);
			var network = networkViewModel.GetJobNetwork();

			CombineAssertions("Pre-condition: linked diagrams should have no schedules yet", () =>
			{
				AssertEquals(ZDateTime.Empty, otherDiagram1.ScheduledStartTimeUtc);
				AssertEquals(ZDateTime.Empty, otherDiagram2.ScheduledStartTimeUtc);
				AssertEquals(ZDateTime.Empty, otherDiagram3.ScheduledStartTimeUtc);
				AssertEquals(ZDateTime.Empty, otherDiagram4.ScheduledStartTimeUtc);
			});

			AssertEquals(0, reporter.ItemsProcessed);
			AssertEquals(false, reporter.IsDisposed);

			mainDiagram.ScheduledStartTimeUtc = ZDateTime.UtcNow;
			network.Refresh(RefreshType.Saving);
			Factory.Save();

			CombineAssertions("Should propagate valid start times to the linked diagrams", () =>
			{
				AssertNotEquals(ZDateTime.Empty, otherDiagram1.ScheduledStartTimeUtc);
				AssertNotEquals(ZDateTime.Empty, otherDiagram2.ScheduledStartTimeUtc);
				AssertNotEquals(ZDateTime.Empty, otherDiagram3.ScheduledStartTimeUtc);
				AssertNotEquals(ZDateTime.Empty, otherDiagram4.ScheduledStartTimeUtc);
			});

			AssertEquals("Should notify about only those shapes which require schedule propagation", 4, reporter.ItemsProcessed);
			AssertEquals(true, reporter.IsDisposed);
		}

		public void TestPropagateSchedules_WhenMultipleNestedLinkedDiagramsExist_ShouldPropagateDeeply()
		{
			var rootDiagramSet = new DiagramAndShapeSet(Factory, "rootDiagramSet", shapeWidthInTimeSlots: 8);

			var nestedDiagramSet1_1 = new DiagramAndShapeSet(rootDiagramSet.Shape1, "nestedDiagramSet1_1", shapeWidthInTimeSlots: 4);
			var nestedDiagramSet1_2 = new DiagramAndShapeSet(rootDiagramSet.Shape2, "nestedDiagramSet1_2", shapeWidthInTimeSlots: 4);

			var nestedDiagramSet2_1 = new DiagramAndShapeSet(nestedDiagramSet1_1.Shape1, "nestedDiagramSet2_1", shapeWidthInTimeSlots: 2);
			var nestedDiagramSet2_2 = new DiagramAndShapeSet(nestedDiagramSet1_1.Shape2, "nestedDiagramSet2_2", shapeWidthInTimeSlots: 2);
			var nestedDiagramSet2_3 = new DiagramAndShapeSet(nestedDiagramSet1_2.Shape1, "nestedDiagramSet2_3", shapeWidthInTimeSlots: 2);
			var nestedDiagramSet2_4 = new DiagramAndShapeSet(nestedDiagramSet1_2.Shape2, "nestedDiagramSet2_4", shapeWidthInTimeSlots: 2);

			var nestedDiagramSet3_1 = new DiagramAndShapeSet(nestedDiagramSet2_1.Shape1, "nestedDiagramSet3_1", shapeWidthInTimeSlots: 1);
			var nestedDiagramSet3_2 = new DiagramAndShapeSet(nestedDiagramSet2_1.Shape2, "nestedDiagramSet3_2", shapeWidthInTimeSlots: 1);
			var nestedDiagramSet3_3 = new DiagramAndShapeSet(nestedDiagramSet2_2.Shape1, "nestedDiagramSet3_3", shapeWidthInTimeSlots: 1);
			var nestedDiagramSet3_4 = new DiagramAndShapeSet(nestedDiagramSet2_2.Shape2, "nestedDiagramSet3_4", shapeWidthInTimeSlots: 1);
			var nestedDiagramSet3_5 = new DiagramAndShapeSet(nestedDiagramSet2_3.Shape1, "nestedDiagramSet3_5", shapeWidthInTimeSlots: 1);
			var nestedDiagramSet3_6 = new DiagramAndShapeSet(nestedDiagramSet2_3.Shape2, "nestedDiagramSet3_6", shapeWidthInTimeSlots: 1);
			var nestedDiagramSet3_7 = new DiagramAndShapeSet(nestedDiagramSet2_4.Shape1, "nestedDiagramSet3_7", shapeWidthInTimeSlots: 1);
			var nestedDiagramSet3_8 = new DiagramAndShapeSet(nestedDiagramSet2_4.Shape2, "nestedDiagramSet3_8", shapeWidthInTimeSlots: 1);

			var reporter = new DummyProgressReporter();
			var reporterProvider = new Mock<IProgressReporterProvider>(MockBehavior.Strict);

			reporterProvider.Setup(m => m.CreateProgressReporter("Propagating scheduling changes to linked diagrams", 14, true)).Returns(reporter);
			var networkViewModel = CreateNetworkViewModel(rootDiagramSet.RootDiagram, progressReporterProvider: reporterProvider.Object);
			var network = networkViewModel.GetJobNetwork();

			var allOtherDiagrams = Factory.Load<BMNCNRootDiagramShape>(new ZQuery(BMNCNShapeSchema.BNS_Name, SQLComparisonOperator.StartsWith, "nestedDiagramSet"));

			AssertEquals(14, allOtherDiagrams.Length);

			CombineAssertions("Pre-condition: linked diagrams shouldn't have any ScheduledStartTimeUtc set", () =>
			{
				foreach (var diagram in allOtherDiagrams)
				{
					AssertEquals(diagram.Name, ZDateTime.Empty, diagram.ScheduledStartTimeUtc);
				}
			});

			Factory.Save();

			AssertEquals(0, reporter.ItemsProcessed);
			AssertEquals(false, reporter.IsDisposed);

			network.DiagramShape.ScheduledStartTimeUtc = ZDateTime.UtcNow;
			network.Refresh(RefreshType.Saving);
			Factory.Save();

			AssertDiagramAndShapeSetScheduledStartTimeUtc(rootDiagramSet, ZDateTime.UtcNow, ZDateTime.UtcNow, ZDateTime.UtcNow.AddDays(8));

			AssertDiagramAndShapeSetScheduledStartTimeUtc(nestedDiagramSet1_1, ZDateTime.UtcNow, ZDateTime.UtcNow, ZDateTime.UtcNow.AddDays(4));
			AssertDiagramAndShapeSetScheduledStartTimeUtc(nestedDiagramSet1_2, ZDateTime.UtcNow.AddDays(8), ZDateTime.UtcNow.AddDays(8), ZDateTime.UtcNow.AddDays(12));

			AssertDiagramAndShapeSetScheduledStartTimeUtc(nestedDiagramSet2_1, ZDateTime.UtcNow, ZDateTime.UtcNow, ZDateTime.UtcNow.AddDays(2));
			AssertDiagramAndShapeSetScheduledStartTimeUtc(nestedDiagramSet2_2, ZDateTime.UtcNow.AddDays(4), ZDateTime.UtcNow.AddDays(4), ZDateTime.UtcNow.AddDays(6));
			AssertDiagramAndShapeSetScheduledStartTimeUtc(nestedDiagramSet2_3, ZDateTime.UtcNow.AddDays(8), ZDateTime.UtcNow.AddDays(8), ZDateTime.UtcNow.AddDays(10));
			AssertDiagramAndShapeSetScheduledStartTimeUtc(nestedDiagramSet2_4, ZDateTime.UtcNow.AddDays(12), ZDateTime.UtcNow.AddDays(12), ZDateTime.UtcNow.AddDays(14));

			AssertDiagramAndShapeSetScheduledStartTimeUtc(nestedDiagramSet3_1, ZDateTime.UtcNow, ZDateTime.UtcNow, ZDateTime.UtcNow.AddDays(1));
			AssertDiagramAndShapeSetScheduledStartTimeUtc(nestedDiagramSet3_2, ZDateTime.UtcNow.AddDays(2), ZDateTime.UtcNow.AddDays(2), ZDateTime.UtcNow.AddDays(3));
			AssertDiagramAndShapeSetScheduledStartTimeUtc(nestedDiagramSet3_3, ZDateTime.UtcNow.AddDays(4), ZDateTime.UtcNow.AddDays(4), ZDateTime.UtcNow.AddDays(5));
			AssertDiagramAndShapeSetScheduledStartTimeUtc(nestedDiagramSet3_4, ZDateTime.UtcNow.AddDays(6), ZDateTime.UtcNow.AddDays(6), ZDateTime.UtcNow.AddDays(7));
			AssertDiagramAndShapeSetScheduledStartTimeUtc(nestedDiagramSet3_5, ZDateTime.UtcNow.AddDays(8), ZDateTime.UtcNow.AddDays(8), ZDateTime.UtcNow.AddDays(9));
			AssertDiagramAndShapeSetScheduledStartTimeUtc(nestedDiagramSet3_6, ZDateTime.UtcNow.AddDays(10), ZDateTime.UtcNow.AddDays(10), ZDateTime.UtcNow.AddDays(11));
			AssertDiagramAndShapeSetScheduledStartTimeUtc(nestedDiagramSet3_7, ZDateTime.UtcNow.AddDays(12), ZDateTime.UtcNow.AddDays(12), ZDateTime.UtcNow.AddDays(13));
			AssertDiagramAndShapeSetScheduledStartTimeUtc(nestedDiagramSet3_8, ZDateTime.UtcNow.AddDays(14), ZDateTime.UtcNow.AddDays(14), ZDateTime.UtcNow.AddDays(15));

			void AssertDiagramAndShapeSetScheduledStartTimeUtc(DiagramAndShapeSet set, ZDateTime diagramScheduledStartTime, ZDateTime shape1ScheduledStartTime, ZDateTime shape2ScheduledStartTime)
			{
				CombineAssertions("ScheduledStartTimeUtc of shapes within set should be propagated correctly", () =>
				{
					AssertEquals(set.RootDiagram.Name, diagramScheduledStartTime, set.RootDiagram.ScheduledStartTimeUtc);
					AssertEquals(set.Shape1.Name, shape1ScheduledStartTime, set.Shape1.ScheduledStartTimeUtc);
					AssertEquals(set.Shape2.Name, shape2ScheduledStartTime, set.Shape2.ScheduledStartTimeUtc);
				});
			}

			AssertEquals(14, reporter.ItemsProcessed);
			AssertEquals(true, reporter.IsDisposed);

			AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		class DiagramAndShapeSet
		{
			internal DiagramAndShapeSet(BMNCNShape shapeToWhichDiagramShouldBeLinked, string diagramName, int shapeWidthInTimeSlots)
				: this(shapeToWhichDiagramShouldBeLinked.Factory, diagramName, shapeWidthInTimeSlots)
			{
				NetworkTestCase.LinkToRelatedDiagram(shapeToWhichDiagramShouldBeLinked, RootDiagram);
			}

			internal DiagramAndShapeSet(BusinessObjectFactory factory, string diagramName, int shapeWidthInTimeSlots)
			{
				RootDiagram = NetworkTestCase.CreateDiagram(factory, diagramName, isScaled: true);
				Shape1 = NetworkTestCase.CreateShape(RootDiagram, name: "Shape 1 on " + RootDiagram.Name);
				Shape2 = NetworkTestCase.CreateShape(RootDiagram, name: "Shape 2 on " + RootDiagram.Name);

				var shapeWidth = shapeWidthInTimeSlots * 100;
				var network = NetworkTestCase.CreateNetwork(RootDiagram);

				NetworkTestCase.SetShapeSize(network, Shape1, RootDiagram, shapeWidth, shapeWidth);
				NetworkTestCase.SetShapeSize(network, Shape2, RootDiagram, shapeWidth, shapeWidth);

				NetworkTestCase.SetShapeOffset(Shape1, RootDiagram, network, 0, 0);
				NetworkTestCase.SetShapeOffset(Shape2, RootDiagram, network, shapeWidth, 0);
			}

			internal BMNCNRootDiagramShape RootDiagram { get; }
			internal BMNCNShape Shape1 { get; }
			internal BMNCNShape Shape2 { get; }
		}

		#endregion

		#region Invalid Shape Edits

		public void TestResizeShape_WhenLinkedToScaledDiagram_AndLinkedDiagramResizeIsInvalid_ShouldDisplayErrorBeforePropagatingSchedule_AnsweringYes()
		{
			PerformInvalidShapeResizeOnShapeLinkedToScaledDiagram_AssertingOutcomeOnSchedule(ZDialogResult.Yes, shouldCallController: true);
		}

		public void TestResizeShape_WhenLinkedToScaledDiagram_AndLinkedDiagramResizeIsInvalid_ShouldDisplayErrorBeforePropagatingSchedule_AnsweringNo()
		{
			PerformInvalidShapeResizeOnShapeLinkedToScaledDiagram_AssertingOutcomeOnSchedule(ZDialogResult.No, shouldCallController: false);
		}

		void PerformInvalidShapeResizeOnShapeLinkedToScaledDiagram_AssertingOutcomeOnSchedule(ZDialogResult dialogResultWhenSavingInvalidResize, bool shouldCallController)
		{
			var scaledDiagram1 = NetworkTestCase.CreateDiagram(Factory, isScaled: true, scheduledStartTimeUTC: ZDateTime.UtcNow, name: "Karen Walker");
			var shapeOnDiagram1 = NetworkTestCase.CreateShape(scaledDiagram1);

			var scaledDiagram2 = NetworkTestCase.CreateDiagram(Factory, isScaled: true, name: "Anastasia Beaverhausen");
			var shapeOnDiagram2 = NetworkTestCase.CreateShape(scaledDiagram2);

			var controller = new Mock<IBMNetworkEntityController>();

			var repeat = controller.Setup(m => m.ViewDiagram(It.Is<BMNCNShape>(p => p.PK == scaledDiagram2.PK)));

			var network1 = NetworkTestCase.CreateNetwork(scaledDiagram1, controller: controller.Object);
			NetworkTestCase.SetShapeSize(network1, shapeOnDiagram1, scaledDiagram1, 300, 100);
			var network2 = NetworkTestCase.CreateNetwork(scaledDiagram2);
			NetworkTestCase.SetShapeSize(network2, shapeOnDiagram2, scaledDiagram2, 300, 100);

			network1.RefreshSchedules();
			network2.RefreshSchedules();

			Factory.Save();

			network1.LinkEntity(shapeOnDiagram1, scaledDiagram2);

			CombineAssertions("Pre-condition: shape has schedule dates set, but they're not yet propagated to the linked diagram", () =>
			{
				AssertEquals("Shape Scheduled Start", ZDateTime.Now, shapeOnDiagram1.ScheduledStartTimeLocal);
				AssertEquals("Shape Scheduled Finish", ZDateTime.Now.AddDays(3), shapeOnDiagram1.ScheduledFinishTimeLocal);

				AssertEquals("Linked diagram Scheduled Start", ZDateTime.Empty, scaledDiagram2.ScheduledStartTimeLocal);
				AssertEquals("Linked diagram Scheduled Finish", ZDateTime.Empty, scaledDiagram2.ScheduledFinishTimeLocal);
			});

			network1.Refresh(RefreshType.Saving);
			Factory.Save();

			CombineAssertions("Now that the network has been saved, schedule should be propagated to the linked diagram", () =>
			{
				AssertEquals("Shape Scheduled Start", ZDateTime.Now, shapeOnDiagram1.ScheduledStartTimeLocal);
				AssertEquals("Shape Scheduled Finish", ZDateTime.Now.AddDays(3), shapeOnDiagram1.ScheduledFinishTimeLocal);

				AssertEquals("Linked diagram Scheduled Start", ZDateTime.Now, scaledDiagram2.ScheduledStartTimeLocal);
				AssertEquals("Linked diagram Scheduled Finish", ZDateTime.Now.AddDays(3), scaledDiagram2.ScheduledFinishTimeLocal);
			});

			NetworkTestCase.SetShapeSize(network1, shapeOnDiagram1, scaledDiagram1, 200, 100);

			UnitTestUserNotification.Instance.AddAnswer(dialogResultWhenSavingInvalidResize);
			network1.Refresh(RefreshType.Saving);
			Factory.Save();

			CombineAssertions("After resizing a shape and saving the network, the moved shape's schedule should be updated, but it should not be propagated to the linked diagram because that would cause the diagram surface to be too small for its largest shape", () =>
			{
				AssertEquals("Shape Scheduled Start", ZDateTime.Now, shapeOnDiagram1.ScheduledStartTimeLocal);
				AssertEquals("Shape Scheduled Finish", ZDateTime.Now.AddDays(2), shapeOnDiagram1.ScheduledFinishTimeLocal);

				AssertEquals("Linked diagram Scheduled Start", ZDateTime.Now, scaledDiagram2.ScheduledStartTimeLocal);
				AssertEquals("Linked diagram Scheduled Finish", ZDateTime.Now.AddDays(3), scaledDiagram2.ScheduledFinishTimeLocal);
			});

			AssertEquals("Cannot Propagate Scheduling Changes", UnitTestUserNotification.Instance.LastMessage.Caption);
			AssertEquals(
@"Cannot propagate scheduling changes to linked diagrams because it would introduce conflicts. The diagram with the conflict is named [Anastasia Beaverhausen].

The system attempted to update the diagram with the following information:
• Scheduled Start Time: 01 Jan 2019 10:00
• Scheduled Finish Time: 03 Jan 2019 10:00

Would you like to open this diagram to attempt to fix the conflict?", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSchedulePropagation_WhenLinkedToShapeWithinScaledDiagram_ShouldNotPropagate()
		{
			var scaledDiagram1 = NetworkTestCase.CreateDiagram(Factory, isScaled: true, scheduledStartTimeUTC: ZDateTime.UtcNow);
			var shapeWithinDiagram1 = NetworkTestCase.CreateShape(scaledDiagram1);

			var scaledDiagram2 = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var shapeWithinDiagram2 = NetworkTestCase.CreateShape(scaledDiagram2);

			var network1 = NetworkTestCase.CreateNetwork(scaledDiagram1);
			NetworkTestCase.SetShapeSize(network1, shapeWithinDiagram1, scaledDiagram1, 300, 100);

			var network2 = NetworkTestCase.CreateNetwork(scaledDiagram2);
			NetworkTestCase.SetShapeSize(network2, shapeWithinDiagram2, scaledDiagram2, 300, 100);

			network1.RefreshSchedules();
			network2.RefreshSchedules();
			Factory.Save();

			network1.LinkEntity(shapeWithinDiagram1, shapeWithinDiagram2);

			network1.Refresh(RefreshType.Saving);
			Factory.Save();

			CombineAssertions("Schedule should be propagated to the linked shape because we only propagate schedules to root diagrams. Trying to reconcile sub-diagram propagation is too hard to understand.", () =>
			{
				AssertEquals("Shape Scheduled Start", ZDateTime.Now, shapeWithinDiagram1.ScheduledStartTimeLocal);
				AssertEquals("Shape Scheduled Finish", ZDateTime.Now.AddDays(3), shapeWithinDiagram1.ScheduledFinishTimeLocal);

				AssertEquals("Linked diagram Scheduled Start", ZDateTime.Empty, scaledDiagram2.ScheduledStartTimeLocal);
				AssertEquals("Linked diagram Scheduled Finish", ZDateTime.Empty, scaledDiagram2.ScheduledFinishTimeLocal);

				AssertEquals("Shape within linked diagram Scheduled Start", ZDateTime.Empty, shapeWithinDiagram2.ScheduledStartTimeLocal);
				AssertEquals("Shape within linked diagram Scheduled Finish", ZDateTime.Empty, shapeWithinDiagram2.ScheduledFinishTimeLocal);
			});
		}

		public void TestSchedulePropagation_WhenLinkedToScaledDiagramContainingFarFlungShape_ShouldNotPropagate()
		{
			var scaledDiagram1 = NetworkTestCase.CreateDiagram(Factory, isScaled: true, scheduledStartTimeUTC: ZDateTime.UtcNow, name: "Karen Walker");
			var shapeWithinDiagram1 = NetworkTestCase.CreateShape(scaledDiagram1);

			var scaledDiagram2 = NetworkTestCase.CreateDiagram(Factory, isScaled: true, name: "Anastasia Beaverhausen");
			var shape1WithinDiagram2 = NetworkTestCase.CreateShape(scaledDiagram2, "Parent shape");
			var shape2WithinDiagram2 = NetworkTestCase.CreateShape(shape1WithinDiagram2, "Nested shape");
			var shape3WithinDiagram2 = NetworkTestCase.CreateShape(scaledDiagram2, "Far flung Shape");

			shape1WithinDiagram2.MakeVisiblePrerequisiteOf(shape3WithinDiagram2, scaledDiagram2);

			var controller = new Mock<IBMNetworkEntityController>();

			controller.Setup(m => m.ViewDiagram(It.Is<BMNCNShape>(p => p.PK == scaledDiagram2.PK)));
			var network1 = NetworkTestCase.CreateNetwork(scaledDiagram1, controller: controller.Object);
			NetworkTestCase.SetShapeSize(network1, shapeWithinDiagram1, scaledDiagram1, 300, 100);

			var network2 = NetworkTestCase.CreateNetwork(scaledDiagram2);

			NetworkTestCase.SetShapeSize(network2, shape1WithinDiagram2, scaledDiagram2, 300, 400);
			NetworkTestCase.SetShapeSize(network2, shape2WithinDiagram2, shape1WithinDiagram2, 300, 100);
			NetworkTestCase.SetShapeSize(network2, shape3WithinDiagram2, scaledDiagram2, 300, 400);

			NetworkTestCase.SetShapeOffset(shape1WithinDiagram2, scaledDiagram2, network2, 0, 100);
			NetworkTestCase.SetShapeOffset(shape2WithinDiagram2, shape1WithinDiagram2, network2, 0, 100);
			NetworkTestCase.SetShapeOffset(shape3WithinDiagram2, scaledDiagram2, network2, 1500, 100);

			network1.RefreshSchedules();
			network2.RefreshSchedules();

			Factory.Save();

			network1.LinkEntity(shapeWithinDiagram1, scaledDiagram2);

			network1.Refresh(RefreshType.Saving);
			Factory.Save();

			CombineAssertions("After linking the diagram and saving the network, the schedule should not be propagated to the linked diagram because that would cause the diagram surface to be too small for its furthest away shape", () =>
			{
				AssertEquals("Shape Scheduled Start", ZDateTime.Now, shapeWithinDiagram1.ScheduledStartTimeLocal);
				AssertEquals("Shape Scheduled Finish", ZDateTime.Now.AddDays(3), shapeWithinDiagram1.ScheduledFinishTimeLocal);

				AssertEquals("Linked diagram Scheduled Start", ZDateTime.Empty, scaledDiagram2.ScheduledStartTimeLocal);
				AssertEquals("Linked diagram Scheduled Finish", ZDateTime.Empty, scaledDiagram2.ScheduledFinishTimeLocal);
			});

			AssertEquals("Cannot Propagate Scheduling Changes", UnitTestUserNotification.Instance.LastMessage.Caption);
			AssertEquals(
@"Cannot propagate scheduling changes to linked diagrams because it would introduce conflicts. The diagram with the conflict is named [Anastasia Beaverhausen].

The system attempted to update the diagram with the following information:
• Scheduled Start Time: 01 Jan 2019 10:00
• Scheduled Finish Time: 04 Jan 2019 10:00

Would you like to open this diagram to attempt to fix the conflict?", UnitTestUserNotification.Instance.LastMessage.Text);
			controller.Verify(m => m.ViewDiagram(It.Is<BMNCNShape>(p => p.PK == scaledDiagram2.PK)), Times.Once());
		}

		public void TestPropagateSchedules_WhenInfinitelyRecursiveNestedLinkedDiagramsExist_WhenTheUserGivesEnthusiasticConsent_ShouldPropagateSensibly()
		{
			var diagram1 = NetworkTestCase.CreateDiagram(Factory, isScaled: true, name: "Diagram 1");
			var shape1 = NetworkTestCase.CreateShape(diagram1, name: "Shape 1");

			var diagram2 = NetworkTestCase.CreateDiagram(Factory, isScaled: true, name: "Diagram 2");
			var shape2 = NetworkTestCase.CreateShape(diagram2, name: "Shape 2");

			var diagram3 = NetworkTestCase.CreateDiagram(Factory, isScaled: true, name: "Diagram 3");
			var shape3 = NetworkTestCase.CreateShape(diagram3, name: "Shape 3");

			NetworkTestCase.LinkToRelatedDiagram(shape1, diagram2);
			NetworkTestCase.LinkToRelatedDiagram(shape2, diagram3);
			NetworkTestCase.LinkToRelatedDiagram(shape3, diagram1); // Causes circularly-related diagrams.

			var network1 = NetworkTestCase.CreateNetwork(diagram1);
			var network2 = NetworkTestCase.CreateNetwork(diagram2);
			var network3 = NetworkTestCase.CreateNetwork(diagram3);

			NetworkTestCase.SetShapeSize(network1, shape1, diagram1, 300, 100);
			NetworkTestCase.SetShapeSize(network2, shape2, diagram2, 200, 100);
			NetworkTestCase.SetShapeSize(network3, shape3, diagram3, 100, 100);

			NetworkTestCase.SetShapeOffset(shape1, diagram1, network1, 100, 0);
			NetworkTestCase.SetShapeOffset(shape2, diagram2, network2, 100, 0);
			NetworkTestCase.SetShapeOffset(shape3, diagram3, network3, 100, 0); // Would cause the first diagram to be re-scheduled if this schedule were copied.

			Factory.Save();

			network1.DiagramShape.ScheduledStartTimeUtc = ZDateTime.UtcNow;
			network1.Refresh(RefreshType.Saving);
			Factory.Save();

			CombineAssertions("By default the propagation will still happen, but just stop when we encounter the recursion into diagrams that have already been propagated.", () =>
			{
				AssertEquals("diagram1", ZDateTime.UtcNow, diagram1.ScheduledStartTimeUtc);
				AssertEquals("diagram2", ZDateTime.UtcNow.AddDays(1), diagram2.ScheduledStartTimeUtc);
				AssertEquals("diagram3", ZDateTime.UtcNow.AddDays(2), diagram3.ScheduledStartTimeUtc);
			});

			AssertMultilineASCIIEquals(
@"A circular relationship between linked diagrams has been found. Would you like to proceed with updating schedules anyway? The process will stop once it reaches a diagram that has already had its schedule updated.

Shape: Shape 3
Diagram containing shape: Diagram 3
", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestPropagateSchedules_WhenInfinitelyRecursiveNestedLinkedDiagramsExist_WhenTheUserDeniesConsent_ShouldNotPropagateToAnything()
		{
			var diagram1 = NetworkTestCase.CreateDiagram(Factory, isScaled: true, name: "Diagram 1");
			var shape1 = NetworkTestCase.CreateShape(diagram1, name: "Shape 1");

			var diagram2 = NetworkTestCase.CreateDiagram(Factory, isScaled: true, name: "Diagram 2");
			var shape2 = NetworkTestCase.CreateShape(diagram2, name: "Shape 2");

			var diagram3 = NetworkTestCase.CreateDiagram(Factory, isScaled: true, name: "Diagram 3");
			var shape3 = NetworkTestCase.CreateShape(diagram3, name: "Shape 3");

			NetworkTestCase.LinkToRelatedDiagram(shape1, diagram2);
			NetworkTestCase.LinkToRelatedDiagram(shape2, diagram3);
			NetworkTestCase.LinkToRelatedDiagram(shape3, diagram1); // Causes circularly-related diagrams.

			var network1 = NetworkTestCase.CreateNetwork(diagram1);
			var network2 = NetworkTestCase.CreateNetwork(diagram2);
			var network3 = NetworkTestCase.CreateNetwork(diagram3);

			NetworkTestCase.SetShapeSize(network1, shape1, diagram1, 300, 100);
			NetworkTestCase.SetShapeSize(network2, shape2, diagram2, 200, 100);
			NetworkTestCase.SetShapeSize(network3, shape3, diagram3, 100, 100);

			NetworkTestCase.SetShapeOffset(shape1, diagram1, network1, 100, 0);
			NetworkTestCase.SetShapeOffset(shape2, diagram2, network2, 100, 0);
			NetworkTestCase.SetShapeOffset(shape3, diagram3, network3, 100, 0); // Would cause the first diagram to be re-scheduled if this schedule were copied.

			Factory.Save();

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);

			network1.DiagramShape.ScheduledStartTimeUtc = ZDateTime.UtcNow;
			network1.Refresh(RefreshType.Saving);
			Factory.Save();

			CombineAssertions("If the user cancels the operation, no propagation should happen. This scenario is worthy of a cancel before any changes are made because there's no way to know which diagram is the 'real' topmost diagram since the relationship is circular.", () =>
			{
				AssertEquals("diagram1", ZDateTime.UtcNow, diagram1.ScheduledStartTimeUtc);
				AssertEquals("diagram2", ZDateTime.Empty, diagram2.ScheduledStartTimeUtc);
				AssertEquals("diagram3", ZDateTime.Empty, diagram3.ScheduledStartTimeUtc);
			});

			AssertMultilineASCIIEquals(
@"A circular relationship between linked diagrams has been found. Would you like to proceed with updating schedules anyway? The process will stop once it reaches a diagram that has already had its schedule updated.

Shape: Shape 3
Diagram containing shape: Diagram 3
", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartmentPK);
			Factory.Save();
		}

		#endregion
	}
}
