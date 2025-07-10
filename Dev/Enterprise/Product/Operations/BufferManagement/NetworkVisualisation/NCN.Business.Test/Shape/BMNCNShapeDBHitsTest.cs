using System;
using System.Collections.Generic;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	public class BMNCNShapeDBHitsTest : BMSTestCaseWithFactory
	{
		[TestDate(2019, 07, 22)]
		public void TestDBHitsForShapeLinkedToDiagram_RemainingEstimateHoursIncludingChildren()
		{
			void action(BMNCNShape diagram)
			{
				var remainingHours = diagram.RemainingEstimateHoursIncludingChildren;
			}
			var expectedHits = new Dictionary<string, int>
			{
				{ BMNCNShape.Schema.TableName, 4 }, // 1) Loading shape above 2) ChildShapes of Main Diagram 3) RelatedShape of Linked Shape 4) ChildShapes of Related Diagram
				{ ProcessHeader.Schema.TableName, 1 },
				{ ProcessHeaderLink.Schema.TableName, 1 }, // ProcessHeader.ChildLinks
				{ ProcessTask.Schema.TableName, 1 },
			};

			AssertDBHitsForShapeLinkedToDiagram(action, expectedHits);
		}

		[TestDate(2019, 07, 22)]
		public void TestDBHitsForShapeLinkedToDiagram_TotalNonCancelledEstimatedHoursIncludingChildren()
		{
			void action(BMNCNShape diagram)
			{
				var remainingHours = diagram.TotalNonCancelledEstimatedHoursIncludingChildren;
			}
			var expectedHits = new Dictionary<string, int>
			{
				{ BMNCNShape.Schema.TableName, 4 }, // 1) Loading shape above 2) ChildShapes of Main Diagram 3) RelatedShape of Linked Shape 4) ChildShapes of Related Diagram
				{ ProcessHeader.Schema.TableName, 1 },
				{ ProcessHeaderLink.Schema.TableName, 1 }, // ProcessHeader.ChildLinks
				{ ProcessTask.Schema.TableName, 1 },
			};

			AssertDBHitsForShapeLinkedToDiagram(action, expectedHits);
		}

		void AssertDBHitsForShapeLinkedToDiagram(Action<BMNCNShape> diagramAction, Dictionary<string, int> expectedHits)
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "Main Diagram", isScaled: true);
			var linkedShape = NetworkTestCase.CreateShape(diagram, name: "Linked Shape");

			var relatedDiagram = NetworkTestCase.CreateDiagram(Factory, name: "Related Diagram", isScaled: true);

			BMNCNShapeTest.LinkShapeToDiagram(linkedShape, parentDiagram: diagram, diagramToLink: relatedDiagram);
			AssertEquals("Precondition", relatedDiagram, linkedShape.RelatedShape);

			const int childShapesCount = 10;

			for (int i = 0; i < childShapesCount; i++)
			{
				var dummyWorkflowProvider = BMSTestHelper.CreateDummyWorkflowProvider(Factory);
				var jobHeader = ProcessJobHeader.GetForParent(dummyWorkflowProvider, Factory);
				var processHeader = jobHeader.ProcessHeaders[0];
				BMSTestHelper.CreateTask(processHeader, lowEstMinutes: 60, estVariationFactor: 1, description: $"Task{i}");

				var shapeLinkedToProcessHeader = NetworkTestCase.CreateShape(processHeader, relatedDiagram, name: $"Shape{i}");
				AssertEquals("Precondition", 1m, shapeLinkedToProcessHeader.RemainingEstimateHoursIncludingChildren);
				AssertEquals("Precondition", 1m, shapeLinkedToProcessHeader.TotalNonCancelledEstimatedHoursIncludingChildren);
			}

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedDiagram = newFactory.Load<BMNCNShape>(diagram.PK);

			diagramAction(loadedDiagram);

			AssertDbHits(expectedHits, newFactory);
		}
	}
}
