using System.Linq;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class ExtensionMethodsTest : NetworkTestCase
	{
		public void TestSetWidthForDuration_WhenNoScaleSet()
		{
			var root = CreateDiagram(Factory, isScaled: true);
			var network = CreateNetwork(root);
			var entity = CreateShape(root).AsEntity(network);

			entity.SetWidthForDuration(100);
			AssertEquals(100.0, entity.Width);

			root.Scale = new ZInt(100).GetDateTimeFromMinutes();
			AssertEquals(480.0, entity.Width);
		}

		public void TestSetWidthForDuration_Annotation()
		{
			var root = CreateDiagram(Factory, isScaled: true);
			var network = CreateNetwork(root);
			var entity = CreateShape(root, shapeType: ShapeTypeList.Codes.Annotation).AsEntity(network);

			root.Scale = new ZInt(200).GetDateTimeFromMinutes();

			entity.SetWidthForDuration(100);
			AssertEquals(240.0, entity.Width);
		}

		public void TestGetShapesWithinSameDiagram()
		{
			var rootDiagram = CreateDiagram(CreateJobHeader<OrgHeader>(), name: "Root");
			var trunkDiagram = CreateShape(CreateJobHeader<OrgHeader>(), rootDiagram, name: "Fat Trunk");
			var redBranchDiagram = CreateShape(CreateJobHeader<OrgHeader>(), trunkDiagram, name: "Red Branch");
			var blueBranchDiagram = CreateShape(CreateJobHeader<OrgHeader>(), trunkDiagram, name: "Blue Branch");
			var redLeafShape1 = CreateShape(redBranchDiagram, "Red Leaf 1");
			var redLeafShape2 = CreateShape(redBranchDiagram, "Red Leaf 2");
			var redLink = redLeafShape1.MakeVisiblePrerequisiteOf(redLeafShape2, blueBranchDiagram);
			var blueLeafShape1 = CreateShape(blueBranchDiagram, "Blue Leaf 1");
			var blueLeafShape2 = CreateShape(blueBranchDiagram, "Blue Leaf 2");
			var blueLink = blueLeafShape1.MakeVisiblePrerequisiteOf(blueLeafShape2, redBranchDiagram);

			var anotherDiagram = CreateDiagram(Factory);
			CreateShape(anotherDiagram);
			CreateShape(anotherDiagram);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				trunkDiagram,
				redBranchDiagram,
				redLeafShape1,
				redLeafShape2,
				blueBranchDiagram,
				blueLeafShape1,
				blueLeafShape2,
			}, rootDiagram.GetShapesWithinSameDiagram());

			Factory.Save();

			var cleanFactory = Factory.CreateNewFactory();
			var loadedRoot = cleanFactory.Load<BMNCNShape>(rootDiagram.PK);
			var descendants = loadedRoot.GetShapesWithinSameDiagram();

			AssertEquals(7, descendants.Length);

			AssertTableHitCount(0, ProcessHeaderSchema.Constants.TableName, cleanFactory);
			AssertTableHitCount(2, BMNCNShapeSchema.Constants.TableName, cleanFactory);
			AssertTableHitCount(0, BMNCNAttachmentSchema.Constants.TableName, cleanFactory);
		}

		public void TestGetDefaultShape_ShouldNotSaveShapesWithNoRealChanges()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");
			jobHeader.FH_CompletionStatement = "jobHeader";
			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);
			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			var shapeCount = Factory.GetDatabaseCount(typeof(BMNCNShape));
			var attachmentCount = Factory.GetDatabaseCount(typeof(BMNCNAttachment));

			Factory.Save();
			AssertEquals(shapeCount, Factory.GetDatabaseCount(typeof(BMNCNShape)));
			AssertEquals(attachmentCount, Factory.GetDatabaseCount(typeof(BMNCNAttachment)));

			AssertEquals(false, jobHeader.HasChanges);

			var diagram = jobHeader.GetDefaultDiagram();
			var childShape1 = diagram.ChildShapes.Single(s => s.BNS_RelatedEntityID == workflow1.PK);
			var childShape2 = diagram.ChildShapes.Single(s => s.BNS_RelatedEntityID == workflow2.PK);

			AssertEquals(false, jobHeader.HasChanges);
			AssertEquals(false, diagram.HasChanges);
			AssertEquals(false, childShape1.HasChanges);
			AssertEquals(false, childShape2.HasChanges);

			Factory.Save();
			AssertEquals("Should be no shapes in the DB", shapeCount, Factory.GetDatabaseCount(typeof(BMNCNShape)));
			AssertEquals("Should be no attachments in the DB", attachmentCount, Factory.GetDatabaseCount(typeof(BMNCNAttachment)));

			AssertEquals(true, jobHeader.IsInDatabase);
			AssertEquals(false, diagram.IsInDatabase);
			AssertEquals(false, childShape1.IsInDatabase);
			AssertEquals(false, childShape2.IsInDatabase);

			var newDiagram = jobHeader.GetDefaultDiagram();
			var newChildShape1 = newDiagram.ChildShapes.Single(s => s.BNS_RelatedEntityID == workflow1.PK);
			var newChildShape2 = newDiagram.ChildShapes.Single(s => s.BNS_RelatedEntityID == workflow2.PK);

			AssertEquals("Should re-use shapes", diagram, newDiagram);
			AssertEquals("Should re-use shapes", childShape1, newChildShape1);
			AssertEquals("Should re-use shapes", childShape2, newChildShape2);

			AssertEquals(false, jobHeader.HasChanges);
			AssertEquals(false, newDiagram.HasChanges);
			AssertEquals(false, newChildShape1.HasChanges);
			AssertEquals(false, newChildShape2.HasChanges);

			newChildShape1.BNS_Name = "sprugle";
			newChildShape2.BNS_Name = "splartt";

			AssertEquals("Diagrams don't always need to save.", false, newDiagram.HasChanges);
			AssertEquals(true, newChildShape1.HasChanges);
			AssertEquals(true, newChildShape2.HasChanges);

			CombineAssertions("Now that child shapes have saveable changes, all shapes should be detected as IsSavedByFactory", () =>
			{
				AssertEquals("newDiagram", true, newDiagram.IsSavedByFactory);
				AssertEquals("newChildShape1", true, newChildShape1.IsSavedByFactory);
				AssertEquals("newChildShape2", true, newChildShape2.IsSavedByFactory);
			});

			Factory.Save();
			AssertEquals("Shapes and root diagram should be saved now that there's a real change", shapeCount + 3, Factory.GetDatabaseCount(typeof(BMNCNShape)));
			AssertEquals("Attachment is now saved too.", attachmentCount + 1, Factory.GetDatabaseCount(typeof(BMNCNAttachment)));

			AssertEquals(true, newDiagram.IsInDatabase);
			AssertEquals(true, newChildShape1.IsInDatabase);
			AssertEquals(true, newChildShape2.IsInDatabase);

			var newFactory = Factory.CreateNewFactory();

			var loadedJobHeader = newFactory.Load<ProcessJobHeader>(jobHeader.PK);
			var loadedDiagram = loadedJobHeader.GetDefaultDiagram();
			var loadedChildShape1 = loadedDiagram.ChildShapes.Single(s => s.BNS_RelatedEntityID == workflow1.PK);
			var loadedChildShape2 = loadedDiagram.ChildShapes.Single(s => s.BNS_RelatedEntityID == workflow2.PK);

			AssertSamePK(newDiagram, loadedDiagram);
			AssertSamePK(newChildShape1, loadedChildShape1);
			AssertSamePK(newChildShape2, loadedChildShape2);
		}
	}
}
