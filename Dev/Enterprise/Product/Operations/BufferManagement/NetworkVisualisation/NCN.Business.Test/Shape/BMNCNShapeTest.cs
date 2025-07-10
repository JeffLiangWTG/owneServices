using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ProcessManagement.Integration;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(BMNCNShape))]
	class BMNCNShapeTest : BaseShapeTestCase
	{
		#region Pins

		public void TestPinPersistence()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var viewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var shape1 = NetworkTestCase.CreateShape(diagram, "Pinny McPin Face");
			var shape2 = NetworkTestCase.CreateShape(diagram, "Pin Pals");

			shape1.PinShape(viewModel);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedDiagram = newFactory.Load<BMNCNRootDiagramShape>(diagram.PK);

			AssertEquals(true, loadedDiagram.ChildShapes.Single(s => s.BNS_Name == "Pinny McPin Face").IsPinned);
			AssertEquals(false, loadedDiagram.ChildShapes.Single(s => s.BNS_Name == "Pin Pals").IsPinned);
		}

		public void TestApprovedSubDiagramPositions()
		{
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var rootShape = NetworkTestCase.CreateDiagram(Factory, name: "root", isScaled: true);
			var network = NetworkTestCase.CreateNetwork(rootShape);
			var root = network.DiagramEntity;

			var trunk = NetworkTestCase.CreateShape(root, name: "trunk");
			trunk.X = 200;
			trunk.Y = 200;
			trunk.Width = 1000;
			trunk.Height = 1000;
			var branch = NetworkTestCase.CreateShape(trunk, name: "branch");
			branch.X = 400;
			branch.Y = 400;

			root.Approve(staff.GS_Code);
			trunk.Approve(staff.GS_Code);
			branch.Approve(staff.GS_Code);

			AssertEquals(0d, root.X);
			AssertEquals(0d, root.Y);
			AssertEquals(200d, trunk.X);
			AssertEquals(200d, trunk.Y);
			AssertEquals(400d, branch.X);
			AssertEquals(400d, branch.Y);
		}

		public void TestFixedShape_MaintainsFixedCoordsAfterSave_WhenParentMoves()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "diagrammalley", isScaled: true);
			var viewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = viewModel.GetJobNetwork();
			var diagramEntity = network.DiagramEntity;

			var parent = NetworkTestCase.CreateShape(diagramEntity, name: "Hagrid");
			NetworkTestCase.SetDimensionsForEntity(parent, 50, 50, 200, 200);

			var child = NetworkTestCase.CreateShape(parent, name: "Harry");
			NetworkTestCase.SetDimensionsForEntity(parent, 50, 50, 100, 100);

			child.Shape.PinShape(viewModel);

			child.X += 100;
			child.Y += 100;
			AssertEquals("Our child shape should now be fixed in place, but instead...", new Location(50d, 50d), new Location(child.X, child.Y));

			parent.X += 100;
			parent.Y += 100;
			AssertEquals("Our child shape should be fixed in place and therefore not move, but instead...", new Location(50d, 50d), new Location(child.X, child.Y));
		}

		public void TestFixedShape_MaintainsPositionAfterFixing_MaintainsPositionAfterUnfixing()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "diagrammalley", isScaled: true);
			var viewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = viewModel.GetJobNetwork();
			var diagramEntity = network.DiagramEntity;

			var parent = NetworkTestCase.CreateShape(diagramEntity, name: "Hagrid");
			NetworkTestCase.SetDimensionsForEntity(parent, 50, 50, 200, 200);

			var child = NetworkTestCase.CreateShape(parent, name: "Harry");
			NetworkTestCase.SetDimensionsForEntity(parent, 50, 50, 100, 100);

			child.Shape.PinShape(viewModel);

			child.X += 100;
			child.Y += 100;
			AssertEquals("Our child shape should now be fixed in place, but instead...", new Location(50d, 50d), new Location(child.X, child.Y));

			parent.X += 100;
			parent.Y += 100;
			AssertEquals("Our child shape should be fixed in place and therefore not move, but instead...", new Location(50d, 50d), new Location(child.X, child.Y));

			child.Shape.UnPinShape(viewModel);

			child.X += 10;
			child.Y += 10;
			AssertEquals("Our child shape is now unfixed and should move, but instead...", new Location(60d, 60d), new Location(child.X, child.Y));

			parent.X += 10;
			parent.Y += 10;
			AssertEquals("Our child shape is now unfixed and should move when its parent does, but instead...", new Location(70d, 70d), new Location(child.X, child.Y));
		}

		#endregion

		#region ShapeLocation

		public void TestShapeLocation_WithinNestedDiagram_ShouldAlwaysBeStoredRelativeToParent()
		{
			var jobHeader1 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader3 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var workflow1 = jobHeader1.ProcessHeaders.AddNew();
			var workflow2 = jobHeader2.ProcessHeaders.AddNew();
			var workflow3 = jobHeader3.ProcessHeaders.AddNew();

			var diagramShape = NetworkTestCase.CreateDiagram(jobHeader1);
			var network = NetworkTestCase.CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;
			var subDiagram = NetworkTestCase.CreateShape(jobHeader2, diagram);
			var subSubDiagram = NetworkTestCase.CreateShape(jobHeader3, subDiagram);

			var workflow1Shape = NetworkTestCase.CreateShape(workflow1, diagram);
			var workflow2Shape = NetworkTestCase.CreateShape(workflow2, subDiagram);
			var workflow3Shape = NetworkTestCase.CreateShape(workflow3, subSubDiagram);

			NetworkTestCase.SetShapeOffset(workflow1Shape, diagram, 10, 10);
			NetworkTestCase.SetShapeOffset(workflow2Shape, subDiagram, 20, 20);
			NetworkTestCase.SetShapeOffset(workflow3Shape, subSubDiagram, 30, 30);

			NetworkTestCase.SetShapeOffset(subDiagram, diagram, 40, 40);
			NetworkTestCase.SetShapeOffset(subSubDiagram, subDiagram, 50, 50);

			NetworkTestCase.AssertShapeOffset(workflow1Shape, diagram, 10, 10);
			NetworkTestCase.AssertShapeOffset(workflow2Shape, subDiagram, 20, 20);
			NetworkTestCase.AssertShapeOffset(workflow3Shape, subSubDiagram, 30, 30);

			NetworkTestCase.AssertShapeOffset(subDiagram, diagram, 40, 40);
			NetworkTestCase.AssertShapeOffset(subSubDiagram, subDiagram, 50, 50);

			AssertEquals("workflow1 shape is in its specified location - no offset from root diagram", 10.0, ((INetworkEntity)workflow1Shape).X);
			AssertEquals("workflow1 shape is in its specified location - no offset from root diagram", 10.0, ((INetworkEntity)workflow1Shape).Y);

			AssertEquals("subDiagram shape is in its specified location - no offset from root diagram", 40.0, ((INetworkEntity)subDiagram).X);
			AssertEquals("subDiagram shape is in its specified location - no offset from root diagram", 40.0, ((INetworkEntity)subDiagram).Y);

			AssertEquals("workflow2 shape location should be offset from root diagram by subDiagram's location", 60.0, ((INetworkEntity)workflow2Shape).X);
			AssertEquals("workflow2 shape location should be offset from root diagram by subDiagram's location", 60.0, ((INetworkEntity)workflow2Shape).Y);

			AssertEquals("subSubDiagram shape location should be offset from root diagram by subDiagram's location", 90.0, ((INetworkEntity)subSubDiagram).X);
			AssertEquals("subSubDiagram shape location should be offset from root diagram by subDiagram's location", 90.0, ((INetworkEntity)subSubDiagram).Y);

			AssertEquals("workflow3 shape location should be offset from root diagram by subSubDiagram's location", 120.0, ((INetworkEntity)workflow3Shape).X);
			AssertEquals("workflow3 shape location should be offset from root diagram by subSubDiagram's location", 120.0, ((INetworkEntity)workflow3Shape).Y);

			// Now let's move all shapes +10px according to their actual positions - this should set positions relative to their owner...

			((INetworkEntity)workflow3Shape).X = 130;
			((INetworkEntity)workflow3Shape).Y = 130;
			((INetworkEntity)subSubDiagram).X = 100;
			((INetworkEntity)subSubDiagram).Y = 100;
			((INetworkEntity)workflow2Shape).X = 70;
			((INetworkEntity)workflow2Shape).Y = 70;
			((INetworkEntity)subDiagram).X = 50;
			((INetworkEntity)subDiagram).Y = 50;
			((INetworkEntity)workflow1Shape).X = 20;
			((INetworkEntity)workflow1Shape).Y = 20;

			NetworkTestCase.AssertShapeOffset(workflow1Shape, diagram, 20, 20);
			NetworkTestCase.AssertShapeOffset(workflow2Shape, subDiagram, 30, 30);
			NetworkTestCase.AssertShapeOffset(workflow3Shape, subSubDiagram, 40, 40);
			NetworkTestCase.AssertShapeOffset(subDiagram, diagram, 50, 50);
			NetworkTestCase.AssertShapeOffset(subSubDiagram, subDiagram, 60, 60);

			AssertEquals(20.0, ((INetworkEntity)workflow1Shape).X);
			AssertEquals(20.0, ((INetworkEntity)workflow1Shape).Y);
			AssertEquals(50.0, ((INetworkEntity)subDiagram).X);
			AssertEquals(50.0, ((INetworkEntity)subDiagram).Y);
			AssertEquals(80.0, ((INetworkEntity)workflow2Shape).X);
			AssertEquals(80.0, ((INetworkEntity)workflow2Shape).Y);
			AssertEquals(110.0, ((INetworkEntity)subSubDiagram).X);
			AssertEquals(110.0, ((INetworkEntity)subSubDiagram).Y);
			AssertEquals(150.0, ((INetworkEntity)workflow3Shape).X);
			AssertEquals(150.0, ((INetworkEntity)workflow3Shape).Y);
		}

		#endregion

		#region Properties

		public void TestDelayPropertyChange()
		{
			var shape = NetworkTestCase.CreateDiagram(Factory);

			var changedProperties = new List<string>();

			shape.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName);

			using (shape.DelayPropertyChanged())
			{
				shape.BNS_Name = "BingBong";
				shape.BNS_Name = "Lamoo";
				shape.BNS_Name = "Cordle";
				shape.ShapeNotes = "AgglePaggle";

				AssertEquals(0, changedProperties.Count);
			}

			AssertContainsExactElementsInAnyOrder(new[] { "Name", "AdditionalDetail" }, changedProperties);
		}

		public void TestShouldSynchroniseScheduleWithLinkedEntity()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shapeOnDiagram = NetworkTestCase.CreateShape(diagram);
			var scaledDiagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var shapeOnScaledDiagram = NetworkTestCase.CreateShape(scaledDiagram);

			AssertPropertyDetails(expectedPropertyValues: true);

			Factory.Save();

			ReloadBizosInNewFactory();
			AssertPropertyDetails(expectedPropertyValues: true);

			diagram.ShouldSynchroniseScheduleWithLinkedEntity = false;
			shapeOnDiagram.ShouldSynchroniseScheduleWithLinkedEntity = false;
			scaledDiagram.ShouldSynchroniseScheduleWithLinkedEntity = false;
			shapeOnScaledDiagram.ShouldSynchroniseScheduleWithLinkedEntity = false;

			diagram.Factory.Save();

			ReloadBizosInNewFactory();
			AssertPropertyDetails(expectedPropertyValues: false);

			void AssertPropertyDetails(bool expectedPropertyValues)
			{
				AssertEquals(expectedPropertyValues, diagram.ShouldSynchroniseScheduleWithLinkedEntity);
				AssertEquals(true, diagram.ShouldSynchroniseScheduleWithLinkedEntityInfo.ReadOnly);

				AssertEquals(expectedPropertyValues, shapeOnDiagram.ShouldSynchroniseScheduleWithLinkedEntity);
				AssertEquals(true, shapeOnDiagram.ShouldSynchroniseScheduleWithLinkedEntityInfo.ReadOnly);

				AssertEquals(expectedPropertyValues, scaledDiagram.ShouldSynchroniseScheduleWithLinkedEntity);
				AssertEquals(false, scaledDiagram.ShouldSynchroniseScheduleWithLinkedEntityInfo.ReadOnly);

				AssertEquals(expectedPropertyValues, shapeOnScaledDiagram.ShouldSynchroniseScheduleWithLinkedEntity);
				AssertEquals(false, shapeOnScaledDiagram.ShouldSynchroniseScheduleWithLinkedEntityInfo.ReadOnly);
			}

			void ReloadBizosInNewFactory()
			{
				var newFactory = Factory.CreateNewFactory();

				diagram = newFactory.Load<BMNCNRootDiagramShape>(diagram.PK);
				shapeOnDiagram = newFactory.Load<BMNCNShape>(shapeOnDiagram.PK);
				scaledDiagram = newFactory.Load<BMNCNRootDiagramShape>(scaledDiagram.PK);
				shapeOnScaledDiagram = newFactory.Load<BMNCNShape>(shapeOnScaledDiagram.PK);
			}
		}

		public void TestDisplayCompletenessIndicator()
		{
			var diagramShape = NetworkTestCase.CreateDiagram(Factory);
			var network = NetworkTestCase.CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;
			var subDiagram = NetworkTestCase.CreateShape(diagram);

			AssertEquals("DisplayCompletenessIndicator should be editable on root diagrams only", false, diagram.DisplayCompletenessIndicator_ForBindingInfo.ReadOnly);
			AssertEquals("DisplayCompletenessIndicator should be editable on root diagrams only", true, subDiagram.DisplayCompletenessIndicator_ForBindingInfo.ReadOnly);

			subDiagram.DisplayCompletenessIndicator_ForBinding = false;

			AssertEquals("Should default to true", true, diagram.DisplayCompletenessIndicator_ForBinding);
			AssertEquals("Should default to true", true, subDiagram.DisplayCompletenessIndicator_ForBinding);

			AssertEquals("Serialised value should match bound value", true, diagram.Shape.DisplayCompletenessIndicator);
			AssertEquals("Serialised value should reflect what's actually set on this shape", false, subDiagram.Shape.DisplayCompletenessIndicator);

			diagram.DisplayCompletenessIndicator_ForBinding = false;

			AssertEquals("Root diagram value has changed", false, diagram.DisplayCompletenessIndicator_ForBinding);
			AssertEquals("Should inherit value from root diagram", false, subDiagram.DisplayCompletenessIndicator_ForBinding);

			AssertEquals("Serialised value should match bound value", false, diagram.Shape.DisplayCompletenessIndicator);
			AssertEquals("Serialised value should match bound value", false, subDiagram.Shape.DisplayCompletenessIndicator);
		}

		public void TestDisplayCompletenessIndicator_ForSubDiagrams_RootShapeValueShouldNotAffectNestedDiagramSerialisedValue()
		{
			var diagramShape = NetworkTestCase.CreateDiagram(Factory, name: "diagram");
			var network = NetworkTestCase.CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;
			var subDiagram = NetworkTestCase.CreateShape(diagram, "subDiagram");

			diagram.DisplayCompletenessIndicator_ForBinding = false;
			subDiagram.DisplayCompletenessIndicator_ForBinding = true;

			Factory.Save();

			var network1 = NetworkTestCase.CreateNetwork(Factory.CreateNewFactory().Load<BMNCNShape>(diagram.PK));
			var network2 = NetworkTestCase.CreateNetwork(Factory.CreateNewFactory().Load<BMNCNShape>(subDiagram.PK));

			AssertEquals(false, network1.DiagramEntity.DisplayCompletenessIndicator_ForBinding);
			AssertEquals(false, network1.Entities.ShapeEntities.Single(s => s.Name == "subDiagram").DisplayCompletenessIndicator_ForBinding);
			AssertEquals(false, network1.DiagramShape.DisplayCompletenessIndicator);
			AssertEquals(true, network1["subDiagram"].DisplayCompletenessIndicator);

			AssertEquals(true, network2.DiagramEntity.DisplayCompletenessIndicator_ForBinding);
			AssertEquals(true, network2.DiagramShape.DisplayCompletenessIndicator);
		}

		public void TestSupportedActions()
		{
			var diagram = NetworkTestCase.CreateJobAndDiagram(Factory);
			var shape = NetworkTestCase.CreateShape(diagram);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var annotation = networkViewModel.CreateNewAnnotation(diagram).AsShape();

			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();

			var buffer = network.Shapes.OfType<BMNCNBufferShape>().Single();

			AssertEquals(NetworkActions.Hide | NetworkActions.Show | NetworkActions.StyleDiagram | NetworkActions.EditEntity | NetworkActions.AddChildEntities | NetworkActions.GenericActions | NetworkActions.Affinities, diagram.SupportedActions);
			AssertEquals(NetworkActions.Hide | NetworkActions.Show | NetworkActions.StyleDiagram | NetworkActions.EditEntity | NetworkActions.AddChildEntities | NetworkActions.GenericActions | NetworkActions.Affinities, shape.SupportedActions);
			AssertEquals(NetworkActions.Hide | NetworkActions.Show | NetworkActions.StyleDiagram | NetworkActions.EditEntity | NetworkActions.AddChildEntities | NetworkActions.GenericActions, buffer.SupportedActions);
			AssertEquals(NetworkActions.Hide | NetworkActions.Show | NetworkActions.StyleDiagram | NetworkActions.EditEntity | NetworkActions.AddChildEntities | NetworkActions.GenericActions, annotation.SupportedActions);

			diagram.BNS_ShapeType = ShapeTypeList.Codes.DefaultDiagram;
			shape.BNS_ShapeType = ShapeTypeList.Codes.DefaultWorkflow;

			AssertEquals(NetworkActions.GenericActions | NetworkActions.Affinities, diagram.SupportedActions);
			AssertEquals(NetworkActions.GenericActions | NetworkActions.Affinities, shape.SupportedActions);
		}

		[TestDate(2014, 7, 12)]
		public void TestSuspendCalculationOfDateFields()
		{
			var diagram = Factory.New<BMNCNShape>();
			diagram.IsScaled = true;
			diagram.Scale = new ZInt(60).GetDateTimeFromMinutes();
			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow;

			var network = NetworkTestCase.CreateNetwork(diagram);
			var shape = NetworkTestCase.CreateShape(network.DiagramEntity, "Herp");
			AssertEquals(network.DiagramEntity, shape.Owner);

			shape.X = 100;
			shape.Width = 1000;

			AssertEquals(10m, shape.Shape.ExplicitDurationHours);
			AssertEquals(new ZDateTime(2014, 7, 13, 15, 00, 0), shape.Shape.ScheduledStartTimeUtc);

			shape.SuspendCalculation();

			shape.X = 300;
			shape.Width = 2000;

			AssertEquals(20m, shape.Shape.ExplicitDurationHours);
			AssertEquals(new ZDateTime(2014, 7, 13, 15, 00, 0), shape.Shape.ScheduledStartTimeUtc);

			shape.ResumeCalculation();

			AssertEquals(20m, shape.Shape.ExplicitDurationHours);
			AssertEquals(new ZDateTime(2014, 7, 13, 17, 00, 0), shape.Shape.ScheduledStartTimeUtc);
		}

		public void TestLoadProcessJobHeaderDoesntThrowException()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Ducks go quack");

			var diagram = NetworkTestCase.CreateDiagram(jobHeader);

			var shape = NetworkTestCase.CreateShape(diagram);
			shape.BNS_RelatedEntityID = workflow.PK;

			AssertNull(shape.ProcessJobHeader);

			shape.BNS_RelatedEntityID = jobHeader.PK;

			AssertNotNull(shape.ProcessJobHeader);
		}

		[TestDate(2014, 6, 17)]
		public void TestGetMinutesFromScaleSizeDoesNotExplode()
		{
			var diagram = Factory.New<BMNCNShape>();
			diagram.IsScaled = true;

			var network = NetworkTestCase.CreateNetwork(diagram);
			var shape = NetworkTestCase.CreateShape(network.DiagramEntity);
			AssertNoExceptionThrown(() => shape.Width = 4000000000000d);
		}

		public void TestApprovedBy_ShouldBeReadOnly()
		{
			var diagram = Factory.New<BMNCNShape>();
			AssertEquals("Approved By should always be readonly - diagrams should always be approved using " + nameof(ApproveDiagramAction), true, diagram.BNS_GS_NKApprovedByInfo.ReadOnly);
		}

		public void TestApprovedBy_AnnotationsShouldntBeApprovedEvenWhenApproved()
		{
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var shape = Factory.New<BMNCNShape>();
			((IApprovable)shape).Approve(staff.GS_Code);

			AssertEquals(true, shape.IsApproved);

			shape.BNS_ShapeType = ShapeTypeList.Codes.Annotation;

			AssertEquals(staff.GS_Code, shape.BNS_GS_NKApprovedBy);
			AssertEquals("Just because it's approved, doesn't mean it's approved", false, shape.IsApproved);
		}

		public void TestApprovedBy_ShapeTypeBash()
		{
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var shape = Factory.New<BMNCNShape>();

			foreach (CodeDescriptionPair pair in new ShapeTypeList())
			{
				shape.BNS_ShapeType = pair.Code;
				AssertNoExceptionThrown(() => ((IApprovable)shape).Approve(staff.GS_Code));
			}
		}

		[TestDate(2014, 7, 12)]
		public void TestNCNReleaseOffsetMinutes()
		{
			var diagram = Factory.New<BMNCNShape>();
			diagram.NCNReleaseOffsetMinutes = 10;
			AssertEquals((ZDateTime)TimeSpan.FromMinutes(10), diagram.NCNReleaseOffsetTime);

			diagram.NCNReleaseOffsetTime = new ZDateTime(2014, 1, 1, 10, 1, 0);
			AssertEquals(601, diagram.NCNReleaseOffsetMinutes);

			AssertEquals(true, diagram.NCNReleaseOffsetTimeInfo.ReadOnly);

			diagram.IsScaled = true;
			AssertEquals(false, diagram.NCNReleaseOffsetTimeInfo.ReadOnly);

			diagram.IsBuffered = true;
			AssertEquals(false, diagram.NCNReleaseOffsetTimeInfo.ReadOnly);

			((IApprovable)diagram).Approve(GlbStaff.CurrentUser.GS_Code);
			AssertEquals(true, diagram.NCNReleaseOffsetTimeInfo.ReadOnly);
		}

		public void TestApprovedDiagramType()
		{
			var diagram = Factory.New<BMNCNShape>();
			AssertEquals(ApprovedDiagramTypeList.Codes.NonApprovedDiagram, diagram.ApprovedDiagramType);

			((IApprovable)diagram).Approve(GlbStaff.CurrentUser.GS_Code);
			AssertEquals(ApprovedDiagramTypeList.Codes.NCNApprovedDiagram, diagram.ApprovedDiagramType);

			diagram.IsBuffered = true;
			AssertEquals(ApprovedDiagramTypeList.Codes.CCPMApprovedDiagram, diagram.ApprovedDiagramType);
		}

		public void TestShapesWithNoActivePropertySpecifiedInXml_ShouldUseDefaultValue()
		{
			var pk = ZGuid.NewZGuid();
			var xml = "<BNS_LayoutData></BNS_LayoutData>";

			var sql = string.Format(@"
				INSERT dbo.BMNCNShape (
					BNS_PK, BNS_LayoutData, BNS_SystemCreateTimeUtc, BNS_Status, BNS_ShapeType, BNS_SystemCreateUser, BNS_SystemLastEditTimeUtc, BNS_SystemLastEditUser
				) VALUES (
					'{0}', '{1}', GETDATE(), 'UNK', 'DIA', '~BP', GetUtcDate(), '~BP'
				)
				", pk, xml);

			Db.Connection.ExecuteNonQuery(sql);

			var shape = Factory.Load<BMNCNShape>(pk);
			AssertEquals(true, shape.Active);
		}

		public void TestXmlProperties_ShouldIgnoreBogusProperties()
		{
			var shape = Factory.NewWithValidTestData<BMNCNShape>();
			Factory.Save();

			var layoutData = @"
<BNS_LayoutData>
	<Left>82</Left>
	<Top>79.18</Top>
	<ThisPropertyDoesNotExist>QWERTY</ThisPropertyDoesNotExist>
	<BackgroundColor>Black</BackgroundColor>
</BNS_LayoutData>";

			var sql = $@"
UPDATE dbo.BMNCNShape
SET BNS_LayoutData = '{layoutData}',
BNS_SystemLastEditTimeUTC = getutcdate(),
BNS_SystemLastEditUser = '~BP'
WHERE BNS_PK = '{shape.PK}'
";

			Db.Connection.ExecuteNonQuery(sql);

			var loadedShape = Factory.CreateNewFactory().Load<BMNCNShape>(shape.PK);

			AssertEquals(82m, loadedShape.Left);
			AssertEquals(79.18m, loadedShape.Top);
		}

		public void TestXmlProperties_ShouldBeSerialised()
		{
			var parentShape = Factory.NewWithValidTestData<BMNCNShape>();
			var shape = Factory.NewWithValidTestData<BMNCNShape>();
			shape.BNS_ShapeType = ShapeTypeList.Codes.Shape;
			shape.BNS_BNS_RootShape = parentShape.PK;
			shape.BNS_BNS_ParentShape = parentShape.PK;

			shape.Width = 100;
			shape.Height = 120;
			shape.Left = 10;
			shape.Top = 12;

			Factory.Save();

			var loadedShape = Factory.CreateNewFactory().Load<BMNCNShape>(shape.PK);

			AssertEquals(100m, loadedShape.Width);
			AssertEquals(120m, loadedShape.Height);
			AssertEquals(10m, loadedShape.Left);
			AssertEquals(12m, loadedShape.Top);
		}

		public void TestJobName_ShouldComeFromParentWhenPresent()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "MAIORGSYD";
			var jobHeader = Factory.NewWithValidTestData<ProcessJobHeader>();
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			diagram.JobName = "Dat Diagram";

			AssertEquals("Dat Diagram", ((IProposedNetworkEntity)diagram).JobName);

			jobHeader.FH_ParentId = org.PK;
			jobHeader.FH_ParentTableCode = "OH";
			diagram.BNS_RelatedEntityID = jobHeader.PK;
			AssertEquals("MAIORGSYD", ((IProposedNetworkEntity)diagram).JobName);
		}

		public void TestDefaultValues()
		{
			var shape = Factory.New<BMNCNShape>();

			AssertEquals(ShapeTypeList.Codes.Diagram, shape.BNS_ShapeType);
			AssertEquals(true, shape.Active);

			AssertEquals("New Diagram", ((IProposedNetworkEntity)shape).Name);
			AssertEquals("Job Name", ((IProposedNetworkEntity)shape).JobName);
			AssertEquals(Color.Bisque, ((INetworkEntity)shape).BackColor);
		}

		public void TestNetworkNames()
		{
			var diagram = Factory.New<BMNCNShape>();
			var model = new BMNetworkViewModel(diagram);

			AssertEquals(ShapeTypeList.Codes.Diagram, diagram.BNS_ShapeType);
			AssertEquals("New Diagram", ((IProposedNetworkEntity)diagram).Name);
			AssertEquals("Job Name", ((IProposedNetworkEntity)diagram).JobName);
			AssertEquals(Color.Bisque, ((INetworkEntity)diagram).BackColor);
		}

		public void TestSetDiagramName_ShouldNotify()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var diagram = Factory.New<BMNCNShape>();
			diagram.BNS_ShapeType = ShapeTypeList.Codes.Diagram;

			var propertiesChanged = new HashSet<string>();
			diagram.PropertyChanged += (s, e) =>
			{
				if (!propertiesChanged.Contains(e.PropertyName))
				{
					propertiesChanged.Add(e.PropertyName);
				}
			};

			diagram.BNS_Name = "Dat Diagram";
			AssertEquals("We need this property declared as public so that WPF can bind to it, and the designer doesn't die", "Dat Diagram", diagram.Name);
			AssertEquals(1, propertiesChanged.Count);
			Assert(propertiesChanged.Contains("Name"));

			diagram.BNS_RelatedEntityID = jobHeader.PK;
			AssertEquals(3, propertiesChanged.Count);
			Assert(propertiesChanged.Contains("JobName"));
			Assert(propertiesChanged.Contains("JobNumber"));
		}

		public void TestJobType_ShouldBeReadOnlyWhenLinkedToARealJob()
		{
			var diagramShape = Factory.New<BMNCNShape>();
			AssertEquals(false, diagramShape.BNS_JobTypeInfo.ReadOnly);
			AssertEquals(ZString.Empty, diagramShape.BNS_JobType);

			var jobHeader = Factory.NewWithValidTestData<ProcessJobHeader>();
			diagramShape.BNS_RelatedEntityID = jobHeader.PK;
			AssertEquals(false, diagramShape.BNS_JobTypeInfo.ReadOnly);
			AssertEquals(ZString.Empty, diagramShape.BNS_JobType);

			jobHeader.FH_ParentId = Factory.New<OrgHeader>().PK;
			jobHeader.FH_ParentTableCode = "OH";
			AssertEquals(true, diagramShape.BNS_JobTypeInfo.ReadOnly);
			AssertEquals("ORG", diagramShape.BNS_JobType);
		}

		public void TestProcessHeader_ShouldBeReadOnlyForDefaultDiagrams()
		{
			var diagramShape = Factory.New<BMNCNShape>();
			AssertEquals(false, diagramShape.BNS_RelatedEntityIDInfo.ReadOnly);

			diagramShape.BNS_ShapeType = ShapeTypeList.Codes.DefaultDiagram;
			AssertEquals(true, diagramShape.BNS_RelatedEntityIDInfo.ReadOnly);
		}

		public void TestScaleProperties_ReadOnly()
		{
			var diagramEntity = NetworkTestCase.CreateNetwork(Factory.New<BMNCNShape>()).DiagramEntity;
			AssertEquals(true, diagramEntity.Shape.IsScaledInfo.ReadOnly);
			AssertEquals(true, diagramEntity.ResolutionIncrementInfo.ReadOnly);
			AssertEquals(true, diagramEntity.ScaleInfo.ReadOnly);

			diagramEntity.Shape.IsScaled = true;
			AssertEquals(true, diagramEntity.Shape.IsScaledInfo.ReadOnly);
			AssertEquals(false, diagramEntity.ResolutionIncrementInfo.ReadOnly);
			AssertEquals(false, diagramEntity.ScaleInfo.ReadOnly);

			var subDiagram = NetworkTestCase.CreateShape(diagramEntity);

			AssertEquals(true, subDiagram.Shape.IsScaledInfo.ReadOnly);
			AssertEquals(true, subDiagram.ResolutionIncrementInfo.ReadOnly);
			AssertEquals(true, subDiagram.ScaleInfo.ReadOnly);
		}

		public void TestApprovedShape_ShouldPersistSchedulingInfo()
		{
			var diagram = Factory.NewWithValidTestData<BMNCNShape>();
			diagram.IsScaled = true;
			diagram.Scale = new ZInt(60).GetDateTimeFromMinutes();
			((IApprovable)diagram).Approve(GlbStaff.CurrentUser.GS_Code);
			AssertEquals(true, diagram.IsApproved);

			diagram.ExplicitDurationMinutes = 60;

			var network = NetworkTestCase.CreateNetwork(diagram);
			var shape = NetworkTestCase.CreateShape(network.DiagramEntity, "Crepe");

			shape.Schedule = new NetworklessScheduleNode(shape)
			{
				EarliestStartHours = 10m,
				LatestStartHours = 16m,
				IsCriticalPath = true
			};

			/*
			 * This is the only legal way to specify the duration of a shape.
			 * The value is being double-persisted in BNS_Schedule for the sake of performance.
			 */
			shape.Width = 5 * 100d;

			Factory.Save();

			var loadedShape = new BusinessObjectFactory().Load<BMNCNShape>(shape.PK);
			NetworkTestCase.AssertShapeScheduleDurations(loadedShape, 10m, 15m, 16m, 21m, 6m, true);

			var loadedDiagram = new BusinessObjectFactory().Load<BMNCNShape>(diagram.PK);

			AssertEquals(60, diagram.ExplicitDurationMinutes);
		}

		public void TestSetApprovedByOutsideIApprovableInterface()
		{
			var shape = Factory.New<BMNCNShape>();
			AssertExceptionThrown<NotSupportedException>(() => shape.BNS_GS_NKApprovedBy = GlbStaff.CurrentUser.GS_Code);
		}

		#region Linked Entities

		public void TestIsLinkedToRealEntityEnabledForBMS()
		{
			VisualBoardsTestCase.EnableBMSInRegistry();
			var system = VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
			var disabledWorkflowType = system.RelatedWorkflowTypes.AddNew();
			disabledWorkflowType.FSW_WorkflowType = "INQ"; //sales enquiry
			disabledWorkflowType.FSW_IsActive = false;

			var jobHeaderEnabledForBMS = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeaderDisabledForBMS = VisualBoardsTestHelper.CreateJobHeader<SalesEnquiry>(Factory);
			var jobHeaderNoBMS = VisualBoardsTestHelper.CreateJobHeader<OrgOpportunity>(Factory);

			var diagram = NetworkTestCase.CreateDiagram(Factory);

			var shapeEnabledForBMS = NetworkTestCase.CreateShape(jobHeaderEnabledForBMS, diagram);
			var shapeDisabledForBM = NetworkTestCase.CreateShape(jobHeaderDisabledForBMS, diagram);
			var shapeNoBMS = NetworkTestCase.CreateShape(jobHeaderNoBMS, diagram);

			AssertEquals(true, shapeEnabledForBMS.IsLinkedToRealEntity);
			AssertEquals(true, shapeDisabledForBM.IsLinkedToRealEntity);
			AssertEquals(true, shapeNoBMS.IsLinkedToRealEntity);

			AssertEquals(jobHeaderEnabledForBMS, shapeEnabledForBMS.LinkedEntity);
			AssertEquals(jobHeaderDisabledForBMS, shapeDisabledForBM.LinkedEntity);
			AssertEquals(jobHeaderNoBMS, shapeNoBMS.LinkedEntity);

			AssertEquals(true, shapeEnabledForBMS.IsLinkedToRealEntityEnabledForBMS);
			AssertEquals(false, shapeDisabledForBM.IsLinkedToRealEntityEnabledForBMS);
			AssertEquals(false, shapeNoBMS.IsLinkedToRealEntityEnabledForBMS);
		}

		public void TestLinkedProcessHeaderDescription()
		{
			VisualBoardsTestCase.EnableBMSInRegistry();
			VisualBoardsTestHelper.CreateSystem(Factory, "ORG");

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, description: "Job Header 1");
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "Workflow 1");

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, description: "Job Header 2");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "Workflow 2");

			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, description: "Job Header 3");

			var diagram1 = NetworkTestCase.CreateDiagram(Factory, "Non-linked Diagram");
			var shape1 = NetworkTestCase.CreateShape(diagram1, "Non-linked Shape");
			var shape2 = NetworkTestCase.CreateShape(workflow1, diagram1, "Shape Linked To A Workflow");
			var shape3 = NetworkTestCase.CreateShape(jobHeader3, diagram1, "Shape Linked To A Job Header");

			var diagram2 = NetworkTestCase.CreateDiagram(jobHeader2, "Diagram Linked To A Job Header");

			var diagram3 = NetworkTestCase.CreateDiagram(workflow2, "Diagram Linked To A Workflow");

			AssertNull(diagram1.LinkedProcessHeaderDescription);
			AssertNull(shape1.LinkedProcessHeaderDescription);
			AssertEquals("Workflow 1", shape2.LinkedProcessHeaderDescription);
			AssertEquals("Job Header 3", shape3.LinkedProcessHeaderDescription);
			AssertEquals("Job Header 2", diagram2.LinkedProcessHeaderDescription);
			AssertEquals("Workflow 2", diagram3.LinkedProcessHeaderDescription);
		}

		#endregion

		public void TestRootShapeWorkflowDescription_And_RootShapeJobCodeAndDescription()
		{
			VisualBoardsTestCase.EnableBMSInRegistry();
			VisualBoardsTestHelper.CreateSystem(Factory, "ORG");

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, description: "Job Header 1");
			var job1 = jobHeader1.GetParent(Factory) as OrgHeader;
			job1.OH_Code = "AAA";
			job1.OH_FullName = "Job Name 1";

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, description: "Job Header 2");
			var job2 = jobHeader2.GetParent(Factory) as OrgHeader;
			job2.OH_Code = "BBB";
			job2.OH_FullName = "Job Name 2";
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "Workflow 2 - Linked To Root");

			var diagram1 = NetworkTestCase.CreateDiagram(Factory, "Root Diagram Not Linked");
			var shape1 = NetworkTestCase.CreateShape(diagram1, "Shape 1");

			var diagram2 = NetworkTestCase.CreateDiagram(jobHeader1, "Root Diagram Linked To A Job Header");
			var shape2 = NetworkTestCase.CreateShape(diagram2, "Shape 2");

			var diagram3 = NetworkTestCase.CreateDiagram(workflow2, "Root Diagram Linked To A Workflow");
			var shape3 = NetworkTestCase.CreateShape(diagram3, "Shape 3");

			Factory.Save();

			AssertNull(shape1.RootShapeWorkflowDescription);
			AssertNull(shape1.RootShapeJobCodeAndDescription);

			AssertEquals("Job Header 1", shape2.RootShapeWorkflowDescription);
			AssertEquals("Organization (AAA)", shape2.RootShapeJobCodeAndDescription);

			AssertEquals("Workflow 2 - Linked To Root", shape3.RootShapeWorkflowDescription);
			AssertEquals("Organization (BBB)", shape3.RootShapeJobCodeAndDescription);
		}

		#endregion

		#region Delete

		public void TestDelete_ShouldDeleteSchedule()
		{
			var shape = Factory.New<BMNCNShape>();
			var schedule = shape.GetOrCreateSchedule();

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedShape = newFactory.Load<BMNCNShape>(shape.PK);
			var loadedSchedule = loadedShape.ScheduleBizo;

			AssertNotNull(loadedSchedule);
			VisualBoardsTestCase.AssertSamePK(schedule, loadedSchedule);

			loadedShape.Delete();
			newFactory.Save();

			AssertEquals(true, loadedSchedule.IsDeleted);
		}

		public void TestDeleteWorkflow_ShouldDeleteDefaultDiagramShapes()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();

			var defaultDiagram = jobHeader.GetDefaultDiagram();
			var defaultWorkflowShape = defaultDiagram.ChildShapes.First(shape => shape.BNS_RelatedEntityID == workflow.PK);

			var anotherDiagram = NetworkTestCase.CreateDiagram(jobHeader);
			var anotherWorkflowShape = NetworkTestCase.CreateShape(workflow, anotherDiagram);

			Factory.Save();

			AssertEquals(workflow, anotherWorkflowShape.ProcessHeader);

			workflow.Delete();

			AssertEquals(true, defaultWorkflowShape.IsDeleted);
			AssertEquals(false, anotherWorkflowShape.IsDeleted);
			AssertNull(anotherWorkflowShape.ProcessHeader);

			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestDeleteShape_ShouldDeleteAllAttachments()
		{
			var shape = Factory.New<BMNCNShape>();
			var attachment = Factory.New<BMNCNAttachment>();
			attachment.BNA_BNS_FromShape = shape.PK;

			AssertEquals(false, attachment.IsDeleted);

			shape.Delete();
			AssertEquals(true, attachment.IsDeleted);
		}

		public void TestDeleteBuffer_WhenNoOtherBuffersExist_ShouldUnFlagIsBuffered()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			var diagram = NetworkTestCase.CreateDiagram(jobHeader, name: "diagram");
			var shape1 = NetworkTestCase.CreateShape(workflow1, diagram, "shape1");
			var shape2 = NetworkTestCase.CreateShape(workflow2, diagram, "shape2");
			var arrow = shape1.MakeVisiblePrerequisiteOf(shape2, diagram);

			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.ScaleAndRefresh();

			new SuggestBufferAction(networkViewModel).ExecuteForEntityWithoutAccessCheck(diagram);
			var buffer = network.Shapes.Single(s => s.IsBufferShape);
			new AcceptBufferAction(networkViewModel).ExecuteForEntityWithoutAccessCheck(buffer);

			AssertEquals(true, shape1.IsBuffered);
			AssertEquals(true, shape2.IsBuffered);

			buffer.Delete();

			AssertEquals(false, shape1.IsBuffered);
			AssertEquals(false, shape2.IsBuffered);
		}

		public void TestDeleteBuffer_WhenOtherBuffersExist_ShouldLeaveIsBufferedFlag()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			var workflow4 = jobHeader.ProcessHeaders.AddNew();
			var link1_2 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link2_3 = workflow2.GetOrCreateDependencyLink(workflow3);
			var link4_3 = workflow4.GetOrCreateDependencyLink(workflow3);

			var diagram = NetworkTestCase.CreateDiagram(jobHeader, name: "diagram");
			var shape1 = NetworkTestCase.CreateShape(workflow1, diagram, "shape1");
			var shape2 = NetworkTestCase.CreateShape(workflow2, diagram, "shape2");
			var shape3 = NetworkTestCase.CreateShape(workflow3, diagram, "shape3");
			var shape4 = NetworkTestCase.CreateShape(workflow4, diagram, "shape4");
			var arrow1_2 = shape1.MakeVisiblePrerequisiteOf(shape2, diagram);
			var arrow2_3 = shape2.MakeVisiblePrerequisiteOf(shape3, diagram);
			var arrow4_3 = shape4.MakeVisiblePrerequisiteOf(shape3, diagram);

			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.ScaleAndRefresh();

			new SuggestBufferAction(networkViewModel).ExecuteForEntityWithoutAccessCheck(diagram);

			var projectBuffer = network.Shapes.Single(s => s.IsBufferShape && s.Name.EndsWith("Project Buffer"));
			var feedingBuffer = network.Shapes.Single(s => s.IsBufferShape && s.Name.EndsWith("Feeding Buffer"));

			new AcceptBufferAction(networkViewModel).ExecuteForEntityWithoutAccessCheck(projectBuffer);
			new AcceptBufferAction(networkViewModel).ExecuteForEntityWithoutAccessCheck(feedingBuffer);

			AssertEquals(true, shape1.IsBuffered);
			AssertEquals(true, shape2.IsBuffered);

			feedingBuffer.Delete();

			AssertEquals(true, shape1.IsBuffered);
			AssertEquals(true, shape2.IsBuffered);

			projectBuffer.Delete();

			AssertEquals(false, shape1.IsBuffered);
			AssertEquals(false, shape2.IsBuffered);
		}

		public void TestDeactivateBuffer_WhenOtherBuffersExist_ShouldLeaveIsBufferedFlag()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			var workflow4 = jobHeader.ProcessHeaders.AddNew();
			var link1_2 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link2_3 = workflow2.GetOrCreateDependencyLink(workflow3);
			var link4_3 = workflow4.GetOrCreateDependencyLink(workflow3);

			var diagram = NetworkTestCase.CreateDiagram(jobHeader, name: "diagram");
			var shape1 = NetworkTestCase.CreateShape(workflow1, diagram, "shape1");
			var shape2 = NetworkTestCase.CreateShape(workflow2, diagram, "shape2");
			var shape3 = NetworkTestCase.CreateShape(workflow3, diagram, "shape3");
			var shape4 = NetworkTestCase.CreateShape(workflow4, diagram, "shape4");
			var arrow1_2 = shape1.MakeVisiblePrerequisiteOf(shape2, diagram);
			var arrow2_3 = shape2.MakeVisiblePrerequisiteOf(shape3, diagram);
			var arrow4_3 = shape4.MakeVisiblePrerequisiteOf(shape3, diagram);

			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			network.ScaleAndRefresh();

			new SuggestBufferAction(networkViewModel).ExecuteForEntityWithoutAccessCheck(diagram);

			var projectBuffer = network.Shapes.Single(s => s.IsBufferShape && s.Name.EndsWith("Project Buffer"));
			var feedingBuffer = network.Shapes.Single(s => s.IsBufferShape && s.Name.EndsWith("Feeding Buffer"));

			new AcceptBufferAction(networkViewModel).ExecuteForEntityWithoutAccessCheck(projectBuffer);
			new AcceptBufferAction(networkViewModel).ExecuteForEntityWithoutAccessCheck(feedingBuffer);

			AssertEquals(true, shape1.IsBuffered);
			AssertEquals(true, shape2.IsBuffered);

			feedingBuffer.Active = false;

			AssertEquals(true, shape1.IsBuffered);
			AssertEquals(true, shape2.IsBuffered);

			projectBuffer.Active = false;

			AssertEquals(false, shape1.IsBuffered);
			AssertEquals(false, shape2.IsBuffered);
		}

		public void TestDeleteBuffer_WhenOtherNonAcceptedBuffersExist_ShouldUnFlagIsBuffered()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			var workflow4 = jobHeader.ProcessHeaders.AddNew();
			var link1_2 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link2_3 = workflow2.GetOrCreateDependencyLink(workflow3);
			var link4_3 = workflow4.GetOrCreateDependencyLink(workflow3);

			var diagram = NetworkTestCase.CreateDiagram(jobHeader, name: "diagram");
			var shape1 = NetworkTestCase.CreateShape(workflow1, diagram, "shape1");
			var shape2 = NetworkTestCase.CreateShape(workflow2, diagram, "shape2");
			var shape3 = NetworkTestCase.CreateShape(workflow3, diagram, "shape3");
			var shape4 = NetworkTestCase.CreateShape(workflow4, diagram, "shape4");
			var arrow1_2 = shape1.MakeVisiblePrerequisiteOf(shape2, diagram);
			var arrow2_3 = shape2.MakeVisiblePrerequisiteOf(shape3, diagram);
			var arrow4_3 = shape4.MakeVisiblePrerequisiteOf(shape3, diagram);

			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			network.ScaleAndRefresh();

			new SuggestBufferAction(networkViewModel).ExecuteForEntityWithoutAccessCheck(diagram);

			var projectBuffer = network.Shapes.Single(s => s.IsBufferShape && s.Name.EndsWith("Project Buffer"));
			var feedingBuffer = network.Shapes.Single(s => s.IsBufferShape && s.Name.EndsWith("Feeding Buffer"));

			new AcceptBufferAction(networkViewModel).ExecuteForEntityWithoutAccessCheck(projectBuffer);

			AssertEquals(true, shape1.IsBuffered);
			AssertEquals(true, shape2.IsBuffered);

			projectBuffer.Delete();

			AssertEquals(false, shape1.IsBuffered);
			AssertEquals(false, shape2.IsBuffered);
		}

		#endregion

		#region DefaultShape

		public void TestDefaultShape_JobHeader_RegisterChildEditable()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "Ning nang nong");
			var defaultShape = jobHeader.GetDefaultDiagram();

			Factory.Save();

			defaultShape.HasChanges = true;
			AssertEquals(true, jobHeader.HasChanges);
		}

		public void TestChangingCompletionStatement_ShouldUpdateDefaultShape()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			workflow.FH_CompletionStatement = "workflow";

			var defaultDiagram = jobHeader.GetDefaultDiagram();
			var defaultWorkflowShape = defaultDiagram.ChildShapes.Single();

			var nonDefaultDiagram = NetworkTestCase.CreateDiagram(jobHeader);
			var network = NetworkTestCase.CreateNetwork(nonDefaultDiagram);
			var nonDefaultWorkflowShape = network.ShowEntity(workflow, nonDefaultDiagram).Single().AsShape();

			Factory.Save();

			jobHeader.FH_CompletionStatement = "Dat Job";
			workflow.FH_CompletionStatement = "Dis Workflow";

			AssertEquals("New Diagram", nonDefaultDiagram.BNS_Name);
			AssertEquals("workflow", nonDefaultWorkflowShape.BNS_Name);

			AssertEquals("Dat Job", defaultDiagram.BNS_Name);
			AssertEquals("Dis Workflow", defaultWorkflowShape.BNS_Name);
		}

		public void TestDefaultShapes_ShouldIgnoreUserCreatedDiagramShapes()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			var diagramShape = NetworkTestCase.CreateDiagram(jobHeader);
			var workflowShape = NetworkTestCase.CreateShape(workflow, diagramShape);

			var defaultDiagramShape = jobHeader.GetDefaultDiagram();
			var defaultWorkflowShape = workflow.GetDefaultShape(defaultDiagramShape);

			var network = NetworkTestCase.CreateNetwork(defaultDiagramShape);
			defaultWorkflowShape.AsEntity(network).X = 100;

			AssertNotEquals(diagramShape, defaultDiagramShape);
			AssertNotEquals(workflowShape, defaultWorkflowShape);

			AssertEquals(ShapeTypeList.Codes.DefaultDiagram, defaultDiagramShape.BNS_ShapeType);
			AssertEquals(ShapeTypeList.Codes.Diagram, diagramShape.BNS_ShapeType);
			AssertEquals(ShapeTypeList.Codes.DefaultWorkflow, defaultWorkflowShape.BNS_ShapeType);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedJobHeader = newFactory.Load<ProcessJobHeader>(jobHeader.PK);
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);

			AssertEquals(defaultDiagramShape.PK, loadedJobHeader.GetDefaultDiagram().PK);
			AssertEquals(defaultWorkflowShape.PK, loadedWorkflow.GetDefaultShape(loadedJobHeader.GetDefaultDiagram()).PK);
		}

		#endregion

		#region Stuff Relating to ProcessHeader

		public void TestPropertiesWhichRelyOnParentOfProcessHeader_ShouldNotThrowExceptionsOnAccess()
		{
			var jobHeader = Factory.NewWithValidTestData<ProcessJobHeader>();
			var workflow = jobHeader.ProcessHeaders.AddNew();
			var jobHeaderShape = Factory.New<BMNCNShape>();
			jobHeaderShape.BNS_RelatedEntityID = jobHeader.PK;
			var workflowShape = Factory.New<BMNCNShape>();
			workflowShape.BNS_RelatedEntityID = workflow.PK;

			var jobHeaderWithoutWorkflow = Factory.NewWithValidTestData<ProcessJobHeader>();
			AssertEquals(0, jobHeaderWithoutWorkflow.ProcessHeaders.Count);
			var jobHeaderWithoutWorkflowShape = Factory.New<BMNCNShape>();
			jobHeaderWithoutWorkflowShape.BNS_RelatedEntityID = jobHeaderWithoutWorkflow.PK;

			AssertNull(workflow.Parent);

			CombineAssertions("The following properties threw exceptions on access when there is no Parent", () =>
			{
				foreach (var property in typeof(BMNCNShape).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Concat(typeof(INetworkEntityWithChildren).GetProperties(BindingFlags.Public | BindingFlags.FlattenHierarchy)))
				{
					AssertNoExceptionThrown(property.Name, () => property.GetValue(jobHeaderShape, null));
					AssertNoExceptionThrown(property.Name, () => property.GetValue(workflowShape, null));
					AssertNoExceptionThrown(property.Name, () => property.GetValue(jobHeaderWithoutWorkflowShape, null));
				}
			});
		}

		public void TestDataRefreshBetweenFactoriesDoesNotDuplicateAffinities()
		{
			var factory1 = Factory.CreateNewFactory();
			factory1.RefreshEnabled = true;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(factory1, addDefaultProcessHeaderIfNone: false);
			var jobHeaderShape = NetworkTestCase.CreateDiagram(jobHeader);
			var anotherShape = NetworkTestCase.CreateShape(jobHeaderShape);

			var affinity1 = jobHeaderShape.ShapeAffinities.AddNew();
			var affinity2 = jobHeaderShape.ShapeAffinities.AddNew();

			var link1 = jobHeaderShape.ShapeAffinityLinks.AddNew();
			link1.ShapeAffinityPK = affinity1.PK;
			link1.ShapePK = anotherShape.PK;

			factory1.Save();

			var factory2 = Factory.CreateNewFactory();
			factory2.RefreshEnabled = true;

			var loadedShape = factory2.Load<BMNCNShape>(jobHeaderShape.PK);

			AssertEquals(2, loadedShape.ShapeAffinities.Count);
			AssertEquals(1, loadedShape.ShapeAffinityLinks.Count);
			AssertEquals(2, jobHeaderShape.ShapeAffinities.Count);
			AssertEquals(1, jobHeaderShape.ShapeAffinityLinks.Count);

			jobHeaderShape.BNS_CompletionStatements = "TESTABC";
			factory1.Save();

			AssertEquals(2, loadedShape.ShapeAffinities.Count);
			AssertEquals(1, loadedShape.ShapeAffinityLinks.Count);
			AssertEquals(2, jobHeaderShape.ShapeAffinities.Count);
			AssertEquals(1, jobHeaderShape.ShapeAffinityLinks.Count);
		}

		public void TestDeleteWorkflow_ShouldUnLinkShapeAndArrowsRatherThanDeleteThem()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Things that were");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Things that are");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "And some things");
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "That have not yet come to pass");

			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape1 = NetworkTestCase.CreateShape(workflow1, diagram);
			var shape2 = NetworkTestCase.CreateShape(workflow2, diagram);
			var shape3 = NetworkTestCase.CreateShape(workflow3, diagram);
			var shape4 = NetworkTestCase.CreateShape(workflow4, diagram);

			var arrow1 = shape1.MakeVisiblePrerequisiteOf(shape2, diagram);
			var arrow2 = shape2.MakeVisiblePrerequisiteOf(shape3, diagram);
			var arrow3 = shape3.MakeVisiblePrerequisiteOf(shape4, diagram);

			AssertEquals(workflow1, shape1.ProcessHeader);
			AssertEquals(workflow2, shape2.ProcessHeader);
			AssertEquals(workflow3, shape3.ProcessHeader);
			AssertEquals(workflow4, shape4.ProcessHeader);

			AssertNotNull(arrow1.ProcessHeaderLink);
			AssertNotNull(arrow2.ProcessHeaderLink);
			AssertNotNull(arrow3.ProcessHeaderLink);

			Factory.Save();

			workflow2.Delete();

			AssertEquals(workflow1, shape1.ProcessHeader);
			AssertNull(shape2.ProcessHeader);
			AssertEquals(workflow3, shape3.ProcessHeader);
			AssertEquals(workflow4, shape4.ProcessHeader);

			AssertEquals(false, shape1.IsDeleted);
			AssertEquals(false, shape2.IsDeleted);
			AssertEquals(false, shape3.IsDeleted);
			AssertEquals(false, shape4.IsDeleted);

			AssertEquals(false, arrow1.IsDeleted);
			AssertEquals(false, arrow2.IsDeleted);
			AssertEquals(false, arrow3.IsDeleted);

			AssertNull(arrow1.ProcessHeaderLink);
			AssertNull(arrow2.ProcessHeaderLink);
			AssertNotNull(arrow3.ProcessHeaderLink);

			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		#region Estimates

		public void TestEstimatesForBuffer()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var buffer = NetworkTestCase.CreateShape(diagram, shapeType: ShapeTypeList.Codes.Buffer);

			AssertEquals(0m, buffer.RemainingEstimateHoursIncludingChildren);
			AssertEquals(0m, buffer.TotalNonCancelledEstimatedHoursIncludingChildren);
		}

		public void TestEstimatesForAnnotation()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var annotation = NetworkTestCase.CreateShape(diagram, shapeType: ShapeTypeList.Codes.Annotation);

			AssertEquals(0m, annotation.RemainingEstimateHoursIncludingChildren);
			AssertEquals(0m, annotation.TotalNonCancelledEstimatedHoursIncludingChildren);
		}

		public void TestEstimatesForNonLinkedShape_NonScaled()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: false);

			var shapeOPN = NetworkTestCase.CreateShape(diagram);
			shapeOPN.ExplicitDurationMinutes = 1 * 60;
			shapeOPN.BNS_Status = ShapeStatusList.Codes.Open;

			var shapeASN = NetworkTestCase.CreateShape(diagram);
			shapeASN.ExplicitDurationMinutes = 1 * 60;
			shapeASN.BNS_Status = ShapeStatusList.Codes.Assigned;

			var shapeWRK = NetworkTestCase.CreateShape(diagram);
			shapeWRK.ExplicitDurationMinutes = 1 * 60;
			shapeWRK.BNS_Status = ShapeStatusList.Codes.Working;

			var shapeSUS = NetworkTestCase.CreateShape(diagram);
			shapeSUS.ExplicitDurationMinutes = 1 * 60;
			shapeSUS.BNS_Status = ShapeStatusList.Codes.Suspended;

			var shapeCLS = NetworkTestCase.CreateShape(diagram);
			shapeCLS.ExplicitDurationMinutes = 1 * 60;
			shapeCLS.BNS_Status = ShapeStatusList.Codes.Closed;

			var shapeCAN = NetworkTestCase.CreateShape(diagram);
			shapeCAN.ExplicitDurationMinutes = 1 * 60;
			shapeCAN.BNS_Status = ShapeStatusList.Codes.Cancelled;

			var shapeUNK = NetworkTestCase.CreateShape(diagram);
			shapeUNK.ExplicitDurationMinutes = 1 * 60;
			shapeUNK.BNS_Status = ShapeStatusList.Codes.Unknown;

			AssertEquals(0m, shapeOPN.RemainingEstimateHoursIncludingChildren);
			AssertEquals(0m, shapeOPN.TotalNonCancelledEstimatedHoursIncludingChildren);

			AssertEquals(0m, shapeASN.RemainingEstimateHoursIncludingChildren);
			AssertEquals(0m, shapeASN.TotalNonCancelledEstimatedHoursIncludingChildren);

			AssertEquals(0m, shapeWRK.RemainingEstimateHoursIncludingChildren);
			AssertEquals(0m, shapeWRK.TotalNonCancelledEstimatedHoursIncludingChildren);

			AssertEquals(0m, shapeSUS.RemainingEstimateHoursIncludingChildren);
			AssertEquals(0m, shapeSUS.TotalNonCancelledEstimatedHoursIncludingChildren);

			AssertEquals(0m, shapeCLS.RemainingEstimateHoursIncludingChildren);
			AssertEquals(0m, shapeCLS.TotalNonCancelledEstimatedHoursIncludingChildren);

			AssertEquals(0m, shapeCAN.RemainingEstimateHoursIncludingChildren);
			AssertEquals(0m, shapeCAN.TotalNonCancelledEstimatedHoursIncludingChildren);

			AssertEquals(0m, shapeUNK.RemainingEstimateHoursIncludingChildren);
			AssertEquals(0m, shapeUNK.TotalNonCancelledEstimatedHoursIncludingChildren);
		}

		public void TestEstimatesForNonLinkedShape_Scaled()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);

			var shapeOPN = NetworkTestCase.CreateShape(diagram);
			shapeOPN.ExplicitDurationMinutes = 1 * 60;
			shapeOPN.BNS_Status = ShapeStatusList.Codes.Open;

			var shapeASN = NetworkTestCase.CreateShape(diagram);
			shapeASN.ExplicitDurationMinutes = 2 * 60;
			shapeASN.BNS_Status = ShapeStatusList.Codes.Assigned;

			var shapeWRK = NetworkTestCase.CreateShape(diagram);
			shapeWRK.ExplicitDurationMinutes = 3 * 60;
			shapeWRK.BNS_Status = ShapeStatusList.Codes.Working;

			var shapeSUS = NetworkTestCase.CreateShape(diagram);
			shapeSUS.ExplicitDurationMinutes = 4 * 60;
			shapeSUS.BNS_Status = ShapeStatusList.Codes.Suspended;

			var shapeCLS = NetworkTestCase.CreateShape(diagram);
			shapeCLS.ExplicitDurationMinutes = 5 * 60;
			shapeCLS.BNS_Status = ShapeStatusList.Codes.Closed;

			var shapeCAN = NetworkTestCase.CreateShape(diagram);
			shapeCAN.ExplicitDurationMinutes = 6 * 60;
			shapeCAN.BNS_Status = ShapeStatusList.Codes.Cancelled;

			var shapeUNK = NetworkTestCase.CreateShape(diagram);
			shapeUNK.ExplicitDurationMinutes = 7 * 60;
			shapeUNK.BNS_Status = ShapeStatusList.Codes.Unknown;

			AssertEquals(1m, shapeOPN.RemainingEstimateHoursIncludingChildren);
			AssertEquals(1m, shapeOPN.TotalNonCancelledEstimatedHoursIncludingChildren);

			AssertEquals(2m, shapeASN.RemainingEstimateHoursIncludingChildren);
			AssertEquals(2m, shapeASN.TotalNonCancelledEstimatedHoursIncludingChildren);

			AssertEquals(3m, shapeWRK.RemainingEstimateHoursIncludingChildren);
			AssertEquals(3m, shapeWRK.TotalNonCancelledEstimatedHoursIncludingChildren);

			AssertEquals(4m, shapeSUS.RemainingEstimateHoursIncludingChildren);
			AssertEquals(4m, shapeSUS.TotalNonCancelledEstimatedHoursIncludingChildren);

			AssertEquals(0m, shapeCLS.RemainingEstimateHoursIncludingChildren);
			AssertEquals(5m, shapeCLS.TotalNonCancelledEstimatedHoursIncludingChildren);

			AssertEquals(0m, shapeCAN.RemainingEstimateHoursIncludingChildren);
			AssertEquals(0m, shapeCAN.TotalNonCancelledEstimatedHoursIncludingChildren);

			AssertEquals(7m, shapeUNK.RemainingEstimateHoursIncludingChildren);
			AssertEquals(7m, shapeUNK.TotalNonCancelledEstimatedHoursIncludingChildren);
		}

		public void TestEstimatesForLinkedToProcessHeader()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var parentProcessHeader = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");
			var childProcessHeader = jobHeader.ProcessHeaders.AddNew();
			BMSTestHelper.MakeChildOf(childProcessHeader, parentProcessHeader);

			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: false);
			var shapeForParent = NetworkTestCase.CreateShape(parentProcessHeader, diagram);
			var shapeForChild = NetworkTestCase.CreateShape(childProcessHeader, diagram);

			AssertEquals("Precondition", 0m, parentProcessHeader.RemainingEstimateHoursIncludingChildren);
			AssertEquals("Precondition", 0m, parentProcessHeader.TotalNonCancelledEstimatedHoursIncludingChildren);

			AssertEquals(0m, shapeForParent.RemainingEstimateHoursIncludingChildren);
			AssertEquals(0m, shapeForParent.TotalNonCancelledEstimatedHoursIncludingChildren);
			AssertEquals(0m, shapeForChild.RemainingEstimateHoursIncludingChildren);
			AssertEquals(0m, shapeForChild.TotalNonCancelledEstimatedHoursIncludingChildren);

			BMSTestHelper.CreateTask(parentProcessHeader, lowEstMinutes: 90, estVariationFactor: 1);
			BMSTestHelper.CreateTask(parentProcessHeader, lowEstMinutes: 60, estVariationFactor: 1, taskStatus: "CLS");

			BMSTestHelper.CreateTask(childProcessHeader, lowEstMinutes: 150, estVariationFactor: 1);
			BMSTestHelper.CreateTask(childProcessHeader, lowEstMinutes: 120, estVariationFactor: 1, taskStatus: "CLS");

			AssertEquals("Precondition", 2.5m, childProcessHeader.RemainingEstimateHoursIncludingChildren);
			AssertEquals("Precondition", 4.5m, childProcessHeader.TotalNonCancelledEstimatedHoursIncludingChildren);

			AssertEquals("Precondition", 4m, parentProcessHeader.RemainingEstimateHoursIncludingChildren);
			AssertEquals("Precondition", 7m, parentProcessHeader.TotalNonCancelledEstimatedHoursIncludingChildren);

			AssertEquals(2.5m, shapeForChild.RemainingEstimateHoursIncludingChildren);
			AssertEquals(4.5m, shapeForChild.TotalNonCancelledEstimatedHoursIncludingChildren);

			AssertEquals(4m, shapeForParent.RemainingEstimateHoursIncludingChildren);
			AssertEquals(7m, shapeForParent.TotalNonCancelledEstimatedHoursIncludingChildren);
		}

		public void TestEstimatesForDiagramAndShapeLinkedToIt()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var processHeader = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");

			var diagram1 = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var shapeLinkedToProcessHeader = NetworkTestCase.CreateShape(processHeader, diagram1);

			BMSTestHelper.CreateTask(processHeader, lowEstMinutes: 90, estVariationFactor: 1);
			BMSTestHelper.CreateTask(processHeader, lowEstMinutes: 60, estVariationFactor: 1, taskStatus: "CLS");

			AssertEquals("Precondition", 1.5m, shapeLinkedToProcessHeader.RemainingEstimateHoursIncludingChildren);
			AssertEquals("Precondition", 2.5m, shapeLinkedToProcessHeader.TotalNonCancelledEstimatedHoursIncludingChildren);

			var shapeOPN = NetworkTestCase.CreateShape(diagram1);
			shapeOPN.BNS_Status = ShapeStatusList.Codes.Open;
			shapeOPN.ExplicitDurationMinutes = 8 * 60;
			AssertEquals("Precondition", 8m, shapeOPN.ExplicitDurationHours);
			AssertEquals("Precondition", 8m, shapeOPN.RemainingEstimateHoursIncludingChildren);
			AssertEquals("Precondition", 8m, shapeOPN.TotalNonCancelledEstimatedHoursIncludingChildren);

			var shapeCLS = NetworkTestCase.CreateShape(diagram1);
			shapeCLS.BNS_Status = ShapeStatusList.Codes.Closed;
			shapeCLS.ExplicitDurationMinutes = 16 * 60;
			AssertEquals("Precondition", 16m, shapeCLS.ExplicitDurationHours);
			AssertEquals("Precondition", 0m, shapeCLS.RemainingEstimateHoursIncludingChildren);
			AssertEquals("Precondition", 16m, shapeCLS.TotalNonCancelledEstimatedHoursIncludingChildren);

			var diagram2 = NetworkTestCase.CreateDiagram(Factory, isScaled: false);
			var shapeLinkedToDiagram = NetworkTestCase.CreateShape(diagram2);
			LinkShapeToDiagram(shapeLinkedToDiagram, parentDiagram: diagram2, diagramToLink: diagram1);
			AssertEquals("Precondition", diagram1, shapeLinkedToDiagram.RelatedShape);

			Factory.Save();

			AssertEquals("Should be the sum of all remaining estimate hours", 8 + 1.5m, diagram1.RemainingEstimateHoursIncludingChildren);
			AssertEquals("Should be the sum of all total non cancelled estimated hours", 1.5m + 1m + 8m + 16m, diagram1.TotalNonCancelledEstimatedHoursIncludingChildren);

			AssertEquals("Should be the same as for the linked diagram", 9.5m, shapeLinkedToDiagram.RemainingEstimateHoursIncludingChildren);
			AssertEquals("Should be the same as for the linked diagram", 26.5m, shapeLinkedToDiagram.TotalNonCancelledEstimatedHoursIncludingChildren);
		}

		public void TestEstimatesShouldBubble()
		{
			var grandParentDiagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var linkedGrandParentDiagramShape = NetworkTestCase.CreateShape(grandParentDiagram);

			var parentDiagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var linkedParentDiagramShape = NetworkTestCase.CreateShape(parentDiagram);

			var childDiagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);

			LinkShapeToDiagram(linkedGrandParentDiagramShape, parentDiagram: grandParentDiagram, diagramToLink: parentDiagram);
			AssertEquals("Precondition", parentDiagram, linkedGrandParentDiagramShape.RelatedShape);

			LinkShapeToDiagram(linkedParentDiagramShape, parentDiagram: parentDiagram, diagramToLink: childDiagram);
			AssertEquals("Precondition", childDiagram, linkedParentDiagramShape.RelatedShape);

			var openGrandParentDiagramShape = NetworkTestCase.CreateShape(grandParentDiagram);
			openGrandParentDiagramShape.BNS_Status = ShapeStatusList.Codes.Open;
			openGrandParentDiagramShape.ExplicitDurationMinutes = 8 * 60;
			AssertEquals("Precondition", 8m, openGrandParentDiagramShape.ExplicitDurationHours);
			AssertEquals("Precondition", 8m, openGrandParentDiagramShape.RemainingEstimateHoursIncludingChildren);
			AssertEquals("Precondition", 8m, openGrandParentDiagramShape.TotalNonCancelledEstimatedHoursIncludingChildren);

			var closedGrandParentDiagramShape = NetworkTestCase.CreateShape(grandParentDiagram);
			closedGrandParentDiagramShape.BNS_Status = ShapeStatusList.Codes.Closed;
			closedGrandParentDiagramShape.ExplicitDurationMinutes = 8 * 60;
			AssertEquals("Precondition", 8m, closedGrandParentDiagramShape.ExplicitDurationHours);
			AssertEquals("Precondition", 0m, closedGrandParentDiagramShape.RemainingEstimateHoursIncludingChildren);
			AssertEquals("Precondition", 8m, closedGrandParentDiagramShape.TotalNonCancelledEstimatedHoursIncludingChildren);

			var openParentDiagramShape = NetworkTestCase.CreateShape(parentDiagram);
			openParentDiagramShape.BNS_Status = ShapeStatusList.Codes.Open;
			openParentDiagramShape.ExplicitDurationMinutes = 16 * 60;
			AssertEquals("Precondition", 16m, openParentDiagramShape.ExplicitDurationHours);
			AssertEquals("Precondition", 16m, openParentDiagramShape.RemainingEstimateHoursIncludingChildren);
			AssertEquals("Precondition", 16m, openParentDiagramShape.TotalNonCancelledEstimatedHoursIncludingChildren);

			var closedParentDiagramShape = NetworkTestCase.CreateShape(parentDiagram);
			closedParentDiagramShape.BNS_Status = ShapeStatusList.Codes.Closed;
			closedParentDiagramShape.ExplicitDurationMinutes = 16 * 60;
			AssertEquals("Precondition", 16m, closedParentDiagramShape.ExplicitDurationHours);
			AssertEquals("Precondition", 0m, closedParentDiagramShape.RemainingEstimateHoursIncludingChildren);
			AssertEquals("Precondition", 16m, closedParentDiagramShape.TotalNonCancelledEstimatedHoursIncludingChildren);

			var openChildDiagramShape = NetworkTestCase.CreateShape(childDiagram);
			openChildDiagramShape.BNS_Status = ShapeStatusList.Codes.Open;
			openChildDiagramShape.ExplicitDurationMinutes = 24 * 60;
			AssertEquals("Precondition", 24m, openChildDiagramShape.ExplicitDurationHours);
			AssertEquals("Precondition", 24m, openChildDiagramShape.RemainingEstimateHoursIncludingChildren);
			AssertEquals("Precondition", 24m, openChildDiagramShape.TotalNonCancelledEstimatedHoursIncludingChildren);

			var closedChildDiagramShape = NetworkTestCase.CreateShape(childDiagram);
			closedChildDiagramShape.BNS_Status = ShapeStatusList.Codes.Closed;
			closedChildDiagramShape.ExplicitDurationMinutes = 24 * 60;
			AssertEquals("Precondition", 24m, closedChildDiagramShape.ExplicitDurationHours);
			AssertEquals("Precondition", 0m, closedChildDiagramShape.RemainingEstimateHoursIncludingChildren);
			AssertEquals("Precondition", 24m, closedChildDiagramShape.TotalNonCancelledEstimatedHoursIncludingChildren);

			AssertEquals(24m, childDiagram.RemainingEstimateHoursIncludingChildren);
			AssertEquals(48m, childDiagram.TotalNonCancelledEstimatedHoursIncludingChildren);

			AssertEquals(24m + 16m, parentDiagram.RemainingEstimateHoursIncludingChildren);
			AssertEquals(48m + 16m + 16m, parentDiagram.TotalNonCancelledEstimatedHoursIncludingChildren);

			AssertEquals(24m + 16m + 8m, grandParentDiagram.RemainingEstimateHoursIncludingChildren);
			AssertEquals(48m + 16m + 16m + 8m + 8m, grandParentDiagram.TotalNonCancelledEstimatedHoursIncludingChildren);
		}

		public void TestShouldNotCrashBecauseOfCircularDependencyBetweenShapes()
		{
			var diagram0 = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var shape0 = NetworkTestCase.CreateShape(diagram0);

			var diagram1 = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var shape1 = NetworkTestCase.CreateShape(diagram1);

			var diagram2 = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var shape2 = NetworkTestCase.CreateShape(diagram2);

			LinkShapeToDiagram(shape0, parentDiagram: diagram0, diagramToLink: diagram1);
			AssertEquals("Precondition", diagram1, shape0.RelatedShape);

			LinkShapeToDiagram(shape1, parentDiagram: diagram1, diagramToLink: diagram2);
			AssertEquals("Precondition", diagram2, shape1.RelatedShape);

			LinkShapeToDiagram(shape2, parentDiagram: diagram2, diagramToLink: diagram1);
			AssertEquals("Precondition", diagram1, shape2.RelatedShape);

			AssertEquals(0m, diagram0.RemainingEstimateHoursIncludingChildren);
			AssertEquals(0m, diagram0.TotalNonCancelledEstimatedHoursIncludingChildren);

			AssertEquals(0m, diagram1.RemainingEstimateHoursIncludingChildren);
			AssertEquals(0m, diagram1.TotalNonCancelledEstimatedHoursIncludingChildren);

			AssertEquals(0m, diagram2.RemainingEstimateHoursIncludingChildren);
			AssertEquals(0m, diagram2.TotalNonCancelledEstimatedHoursIncludingChildren);
		}

		#endregion

		#region Non-Scheduled Shapes

		[TestDate(2019, 5, 14)]
		public void TestIsNonScheduled_WhenChangedToTrue_ShouldDeleteSchedule()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;
			diagram.ScheduledStartTimeUtc = ZDateTime.Now.AddDays(-10);

			var shape = NetworkTestCase.CreateShape(diagram, scheduledStartTimeUTC: ZDateTime.Now);
			var schedule = shape.ScheduleBizo;
			AssertNotNull(schedule);
			AssertEquals(ZDateTime.Now, schedule.BNC_ScheduledStartUtc);

			shape.IsNonScheduled = true;
			AssertEquals("Making the shape non-scheduled should delete its schedule. SAD!", true, schedule.IsDeleted);
			AssertEquals("Making the shape non-scheduled should delete its schedule. SAD!", ZDateTime.Empty, shape.ScheduledStartTimeUtc);
		}

		#endregion

		#region Planning Management Disabled in Registry

		public void TestSave_WhenPlanningManagementEnabled_ShouldNotReportError()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);

			NetworkTestCase.CreateDiagram(Factory);
			Factory.Save();

			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestSave_WhenBufferManagementWorkflowModeEnabled_ShouldReportError()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);

			NetworkTestCase.CreateDiagram(Factory);
			Factory.Save();

			AssertStartsWith("Registry set to BUF", @"A BMNCNShape was initialized, which shouldn't have happened because Planning Management isn't enabled in the registry.
Stack trace of shape constructor:", ErrorReporter.LastMessageReported);
			AssertContains("at Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNShape..ctor(BusinessObjectFactory factory, DataRow row)", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestSave_WhenEnhancedWorkflowManagementEnabled_ShouldReportError()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.EnhancedWorkflow);

			NetworkTestCase.CreateDiagram(Factory);
			Factory.Save();

			AssertStartsWith("Registry set to EWF", @"A BMNCNShape was initialized, which shouldn't have happened because Planning Management isn't enabled in the registry.
Stack trace of shape constructor:", ErrorReporter.LastMessageReported);
			AssertContains("at Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNShape..ctor(BusinessObjectFactory factory, DataRow row)", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestSave_WhenBasicWorkflowManagementEnabled_ShouldReportError()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.BasicWorkflow);

			NetworkTestCase.CreateDiagram(Factory);
			Factory.Save();

			AssertStartsWith("Registry set to BWF", @"A BMNCNShape was initialized, which shouldn't have happened because Planning Management isn't enabled in the registry.
Stack trace of shape constructor:", ErrorReporter.LastMessageReported);
			AssertContains("at Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNShape..ctor(BusinessObjectFactory factory, DataRow row)", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#endregion

		#region Logs

		public void TestNoStmALogs()
		{
			var shape = Factory.NewWithValidTestData<BMNCNShape>();

			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, shape.PK);
			AssertEquals("Should not create Add event", 0, Factory.Load<StmALog>(query).Length);

			shape.BNS_Name = "New name";
			Factory.Save();
			AssertEquals("Should not create Edit event", 0, Factory.Load<StmALog>(query).Length);

			shape.Delete();
			Factory.Save();
			AssertEquals("Should not create Delete event", 0, Factory.Load<StmALog>(query).Length);
		}

		#endregion

		#region Implementation

		public static void LinkShapeToDiagram(BMNCNShape shape, BMNCNRootDiagramShape parentDiagram, BMNCNRootDiagramShape diagramToLink)
		{
			var mocks = new MockRepository(MockBehavior.Default);
			var controller = NetworkTestCase.CreateMockableController(mocks);

			controller.Setup(m => m.PickEntity(It.IsAny<ModuleIdentifier>(), It.IsAny<bool>()))
				.Returns(diagramToLink);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(parentDiagram, controller: controller.Object);
			LinkedEntityMenuItemsTest.ClickLinkToDiagramAction(shape, networkViewModel);
			controller.Verify(m => m.PickEntity(It.IsAny<ModuleIdentifier>(), It.IsAny<bool>()), Times.Once());
		}

		internal static IEnumerable<string> BMNCNShape_XmlMemberNames
		{
			get
			{
				yield return "Active";
				yield return "BackColor";
				yield return "BufferType";
				yield return "CornerRadius";
				yield return "DisplayCompletenessIndicator";
				yield return "ForeColor";
				yield return "Height";
				yield return "IsNonScheduled";
				yield return "IsReadOnly";
				yield return "IsPinned";
				yield return "JobName";
				yield return "Left";
				yield return "ScrollPosition";
				yield return "ShapeAffinities";
				yield return "ShapeAffinityLinks";
				yield return "ShapeNotes";
				yield return "ShouldSynchroniseScheduleWithLinkedEntity";
				yield return "Top";
				yield return "Width";
				yield return "ZIndex";
				yield return "IsPositioned";
				yield return "ShapeInspectorVisible";
			}
		}

		protected override IEnumerable<string> XmlMemberNames => BMNCNShape_XmlMemberNames;

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion
	}

	#region INetworkEntity

	class BMNCNShapeINetworkEntityTest : NetworkTestCase
	{
		#region CanHaveChildren

		public void TestCanHaveChildren()
		{
			var annotation = CreateShape(ShapeTypeList.Codes.Annotation);
			var buffer = CreateShape(ShapeTypeList.Codes.Buffer);
			var defaultDiagram = CreateShape(ShapeTypeList.Codes.DefaultDiagram);
			var defaultDiagramShape = CreateShape(ShapeTypeList.Codes.DefaultWorkflow);
			var diagram = CreateDiagram(Factory);
			var shape = CreateShape(ShapeTypeList.Codes.Shape);

			AssertEquals(false, annotation.CanHaveChildren);
			AssertEquals(false, buffer.CanHaveChildren);
			AssertEquals(true, defaultDiagram.CanHaveChildren);
			AssertEquals(true, defaultDiagramShape.CanHaveChildren);
			AssertEquals(true, diagram.CanHaveChildren);
			AssertEquals(true, shape.CanHaveChildren);
		}

		#endregion

		#region AdditionalDetail

		public void TestAdditionalDetail()
		{
			MakeCompletionStatementTaskType("ORG", "COM");

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders.AddNew();
			var completionStatement1 = workflow.CompletionStatementTasksIncludingChildWorkflowTasks.AddNew();
			completionStatement1.P9_NotesAsString = "Do thing 1";
			var completionStatement2 = workflow.CompletionStatementTasksIncludingChildWorkflowTasks.AddNew();
			completionStatement2.P9_NotesAsString = "Do thing 2";
			var completionStatement3 = workflow.CompletionStatementTasksIncludingChildWorkflowTasks.AddNew();
			completionStatement3.P9_NotesAsString = "Do thing 3";

			var diagram = jobHeader.GetDefaultDiagram();
			var network = CreateNetwork(diagram);

			var defaultShape = workflow.GetDefaultShape(diagram);
			var entity = defaultShape.AsEntity(network);
			SetSchedule(entity, 0, 0);

			AssertEquals("Std. Est.: 0 hours   Float: none", entity.AdditionalDetail);
			AssertEquals(0m, defaultShape.ExplicitDurationLabel);
			AssertEquals(0m, defaultShape.RemainingEstimatedDuration);
			AssertEquals(0m, defaultShape.TotalActualDuration);
			AssertEquals(0m, defaultShape.TotalEstimatedDuration);
		}

		public void TestAdditionalDetail_WithFloatAndEstimates()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			var workflow4 = jobHeader.ProcessHeaders.AddNew();
			var workflow5 = jobHeader.ProcessHeaders.AddNew();
			var workflow6 = jobHeader.ProcessHeaders.AddNew();

			workflow1.MakePrerequisiteOf(workflow2);
			workflow1.MakePrerequisiteOf(workflow4);
			workflow2.MakePrerequisiteOf(workflow3);
			workflow3.MakePrerequisiteOf(workflow6);
			workflow4.MakePrerequisiteOf(workflow5);
			workflow5.MakePrerequisiteOf(workflow6);

			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 3 * 60, estVariationFactor: 1);
			CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 2 * 60, estVariationFactor: 1);
			CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 4 * 60, estVariationFactor: 1);
			CreateTask(workflow4, GlbStaff.CurrentUser.GS_Code, 3 * 60, estVariationFactor: 1);
			CreateTask(workflow5, GlbStaff.CurrentUser.GS_Code, 5 * 60, estVariationFactor: 1);
			CreateTask(workflow6, GlbStaff.CurrentUser.GS_Code, 3 * 60, estVariationFactor: 1);

			var diagram = jobHeader.GetDefaultDiagram();
			var network = CreateNetwork(diagram);

			var defaultShape1 = workflow1.GetDefaultShape(diagram);
			var entity1 = defaultShape1.AsEntity(network);
			SetSchedule(entity1, 0, 0);

			var defaultShape2 = workflow2.GetDefaultShape(diagram);
			var entity2 = defaultShape2.AsEntity(network);
			SetSchedule(entity2, 12, 10);

			var defaultShape3 = workflow3.GetDefaultShape(diagram);
			var entity3 = defaultShape3.AsEntity(network);
			SetSchedule(entity3, 8, 6);

			var defaultShape4 = workflow4.GetDefaultShape(diagram);
			var entity4 = defaultShape4.AsEntity(network);
			SetSchedule(entity4, 0, 0);

			var defaultShape5 = workflow5.GetDefaultShape(diagram);
			var entity5 = defaultShape5.AsEntity(network);
			SetSchedule(entity5, 0, 0);

			var defaultShape6 = workflow6.GetDefaultShape(diagram);
			var entity6 = defaultShape6.AsEntity(network);
			SetSchedule(entity6, 0, 0);

			AssertEquals("Std. Est.: 3 hours   Float: none", entity1.AdditionalDetail);
			AssertEquals("Std. Est.: 2 hours   Float: 2 hours", entity2.AdditionalDetail);
			AssertEquals("Std. Est.: 4 hours   Float: 2 hours", entity3.AdditionalDetail);
			AssertEquals("Std. Est.: 3 hours   Float: none", entity4.AdditionalDetail);
			AssertEquals("Std. Est.: 5 hours   Float: none", entity5.AdditionalDetail);
			AssertEquals("Std. Est.: 3 hours   Float: none", entity6.AdditionalDetail);

			AssertEquals(0m, defaultShape1.ExplicitDurationLabel);
			AssertEquals(3m, defaultShape1.RemainingEstimatedDuration);
			AssertEquals(0m, defaultShape1.TotalActualDuration);
			AssertEquals(3m, defaultShape1.TotalEstimatedDuration);

			AssertEquals(0m, defaultShape2.ExplicitDurationLabel);
			AssertEquals(2m, defaultShape2.RemainingEstimatedDuration);
			AssertEquals(0m, defaultShape2.TotalActualDuration);
			AssertEquals(2m, defaultShape2.TotalEstimatedDuration);

			AssertEquals(0m, defaultShape3.ExplicitDurationLabel);
			AssertEquals(4m, defaultShape3.RemainingEstimatedDuration);
			AssertEquals(0m, defaultShape3.TotalActualDuration);
			AssertEquals(4m, defaultShape3.TotalEstimatedDuration);

			AssertEquals(0m, defaultShape4.ExplicitDurationLabel);
			AssertEquals(3m, defaultShape4.RemainingEstimatedDuration);
			AssertEquals(0m, defaultShape4.TotalActualDuration);
			AssertEquals(3m, defaultShape4.TotalEstimatedDuration);

			AssertEquals(0m, defaultShape5.ExplicitDurationLabel);
			AssertEquals(5m, defaultShape5.RemainingEstimatedDuration);
			AssertEquals(0m, defaultShape5.TotalActualDuration);
			AssertEquals(5m, defaultShape5.TotalEstimatedDuration);

			AssertEquals(0m, defaultShape6.ExplicitDurationLabel);
			AssertEquals(3m, defaultShape6.RemainingEstimatedDuration);
			AssertEquals(0m, defaultShape6.TotalActualDuration);
			AssertEquals(3m, defaultShape6.TotalEstimatedDuration);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		void SetSchedule(ShapeNetworkEntity entity, decimal latestStartHours, decimal earliestStartHours)
		{
			entity.Schedule = new NetworklessScheduleNode(entity)
			{
				EarliestStartHours = earliestStartHours,
				LatestStartHours = latestStartHours,
				IsCriticalPath = false,
				EstimatedDurationHoursIncludingChildren = 4,
			};
		}

		[TestDate(2013, 12, 12, 8, 30, 0)]
		public void TestAdditionalDetail_WithFloatConsumption()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "GBLON"; // Using UTC zone because converting between local/UTC increments local time by zone offset, except that TestDateAttribute makes UTC and local both the same...

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			var workflow4 = jobHeader.ProcessHeaders.AddNew();
			var workflow5 = jobHeader.ProcessHeaders.AddNew();
			var workflow6 = jobHeader.ProcessHeaders.AddNew();

			workflow1.MakePrerequisiteOf(workflow2);
			workflow1.MakePrerequisiteOf(workflow4);
			workflow2.MakePrerequisiteOf(workflow3);
			workflow3.MakePrerequisiteOf(workflow6);
			workflow4.MakePrerequisiteOf(workflow5);
			workflow5.MakePrerequisiteOf(workflow6);

			workflow3.FH_AgreedDeliveryDate = new ZDateTime(2013, 12, 13, 10, 30, 0, DateTimeKind.Utc); // 10 working hours later
			workflow4.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddDays(2);

			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 3 * 60, estVariationFactor: 1);
			CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 2 * 60, estVariationFactor: 1);
			CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 4 * 60, estVariationFactor: 1);
			CreateTask(workflow4, GlbStaff.CurrentUser.GS_Code, 3 * 60, estVariationFactor: 1);
			CreateTask(workflow5, GlbStaff.CurrentUser.GS_Code, 5 * 60, estVariationFactor: 1);
			CreateTask(workflow6, GlbStaff.CurrentUser.GS_Code, 3 * 60, estVariationFactor: 1);

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var diagram = jobHeader.GetDefaultDiagram();
				var network = CreateNetwork(diagram);

				var entity1 = workflow1.GetDefaultShape(diagram).AsEntity(network);
				SetSchedule(entity1, 0, 0);

				AssertEquals("Std. Est.: 3 hours   Float: none", entity1.AdditionalDetail);

				var entity2 = workflow2.GetDefaultShape(diagram).AsEntity(network);
				SetSchedule(entity2, 10, 8);
				var threat = new DeliveryDateThreat(entity2, 1);
				entity2.Schedule.DeliveryDateThreat = threat;

				AssertMultilineASCIIEquals("",
@"Std. Est.: 2 hours   Float: 2 hours
Float consumption: 1 hour", entity2.AdditionalDetail);

				var entity3 = workflow3.GetDefaultShape(diagram).AsEntity(network);
				SetSchedule(entity3, 10, 8);

				threat = new DeliveryDateThreat(entity3, 1);
				entity3.Schedule.DeliveryDateThreat = threat;

				AssertMultilineASCIIEquals("",
@"Std. Est.: 4 hours   Float: 2 hours
Float consumption: 1 hour", entity3.AdditionalDetail);

				var entity4 = workflow4.GetDefaultShape(diagram).AsEntity(network);
				SetSchedule(entity4, 0, 0);
				AssertEquals("Std. Est.: 3 hours   Float: none", entity4.AdditionalDetail);

				var entity5 = workflow5.GetDefaultShape(diagram).AsEntity(network);
				SetSchedule(entity5, 0, 0);
				AssertEquals("Std. Est.: 5 hours   Float: none", entity5.AdditionalDetail);
				AssertEquals("Std. Est.: 3 hours", workflow6.GetDefaultShape(diagram).AsEntity(network).AdditionalDetail);
			}
		}

		public void TestAdditionalDetails_ShouldIncludeNotesWhenPresent()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			var diagramShape = CreateDiagram(jobHeader);
			var network = CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;
			var workflowShape = CreateShape(workflow, diagram);
			diagram.Shape.ShapeNotes = "Very Notes";
			workflowShape.Shape.ShapeNotes = "Such informative";

			SetSchedule(diagram, 0, 0);
			AssertEquals(
@"Std. Est.: 0 hours   Float: none
Very Notes", diagram.AdditionalDetail);

			SetSchedule(workflowShape, 0, 0);
			AssertEquals(
@"Std. Est.: 0 hours   Float: none
Such informative", workflowShape.AdditionalDetail);
		}

		public void TestAdditionalDetails_ShouldIncludePlannedDurationWhenPresent()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var task1 = CreateTask(workflow1, string.Empty, 60);
			var task2 = CreateTask(workflow2, string.Empty, 60);

			var diagramShape = CreateDiagram(jobHeader);
			var network = CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;
			diagram.ExplicitDurationMinutes = 150;
			diagram.Shape.ShapeNotes = "Very Notes";
			var workflowShape = CreateShape(workflow1, diagram);
			workflowShape.ExplicitDurationMinutes = 60;
			workflowShape.Shape.ShapeNotes = "Such informative";

			SetSchedule(diagram, 0, 0);
			AssertEquals(@"Planned Dur.: 2.5 hours
Std. Est.: 3 hours   Float: none
Very Notes", diagram.AdditionalDetail);

			SetSchedule(workflowShape, 0, 0);
			AssertEquals(
@"Planned Dur.: 1 hour
Std. Est.: 1.5 hours   Float: none
Such informative", workflowShape.AdditionalDetail);
		}

		public void TestAdditionalDetails_PlannedDuration_WhenGreaterThanOneDay()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var task1 = CreateTask(workflow1, string.Empty, 60);
			var task2 = CreateTask(workflow2, string.Empty, 60);

			var diagramShape = CreateDiagram(jobHeader);
			var network = CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;
			diagram.ExplicitDurationMinutes = BMConstants.WorkingHoursPerDay * 60;
			var workflowShape = CreateShape(workflow1, diagram);
			workflowShape.ExplicitDurationMinutes = BMConstants.WorkingHoursPerDay * 2 * 60;

			SetSchedule(diagram, 0, 0);
			AssertEquals(@"Planned Dur.: 1 day
Std. Est.: 3 hours   Float: none", diagram.AdditionalDetail);

			SetSchedule(workflowShape, 0, 0);
			AssertEquals(
@"Planned Dur.: 2 days
Std. Est.: 1.5 hours   Float: none", workflowShape.AdditionalDetail);

			diagram.ExplicitDurationMinutes = BMConstants.WorkingHoursPerDay * BMConstants.WorkingDaysPerWeek * 60;
			workflowShape.ExplicitDurationMinutes = (BMConstants.WorkingHoursPerDay * 60) - 60;

			AssertEquals(@"Planned Dur.: 1 week
Std. Est.: 3 hours   Float: none", diagram.AdditionalDetail);

			AssertEquals(
@"Planned Dur.: 7 hours
Std. Est.: 1.5 hours   Float: none", workflowShape.AdditionalDetail);

			workflowShape.ExplicitDurationMinutes = (BMConstants.WorkingHoursPerDay + 2) * 60;
			task1.P9_EstDuration = new ZInt((BMConstants.WorkingHoursPerDay + 2) * 60).GetDateTimeFromMinutes();
			task1.P9_EstimateVariationFactor = 1;

			AssertEquals(
@"Planned Dur.: 1.25 days
Std. Est.: 1.25 days   Float: none", workflowShape.AdditionalDetail);
		}

		#endregion

		#region Scale

		[TestDate(2014, 3, 3)]
		public void TestResolutionIncrement()
		{
			var diagram = Factory.New<BMNCNShape>();
			diagram.IsScaled = true;
			diagram.ResolutionIncrement = new ZDateTime(2014, 3, 4);

			AssertEquals(89280, diagram.AsDiagramEntity().ResolutionIncrement);
		}

		[TestDate(2014, 3, 3)]
		public void TestScale()
		{
			var diagram = Factory.New<BMNCNShape>();
			diagram.IsScaled = true;
			diagram.Scale = new ZDateTime(2014, 3, 4);

			AssertEquals(89280, diagram.AsDiagramEntity().Scale);
		}

		public void TestSetWidth_WhenScaled_ShouldUpdatePlannedDuration()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader2.ProcessHeaders[0];

			var diagramShape = CreateDiagram(jobHeader1);
			var network = CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;
			var subDiagram = CreateShape(jobHeader2, diagram);
			var workflowShape = CreateShape(workflow, subDiagram);

			AssertEquals(subDiagram, workflowShape.Owner);
			AssertEquals(diagram, workflowShape.Root);
			AssertEquals(false, diagram.IsScaled);
			AssertEquals(0, workflowShape.ExplicitDurationMinutes);

			workflowShape.Width = 100;
			AssertEquals(0, workflowShape.ExplicitDurationMinutes);

			diagram.Shape.IsScaled = true;
			AssertEquals(0, workflowShape.ExplicitDurationMinutes);

			workflowShape.Width = 200;
			const int defaultMinutesPerColumn = 60 * BMConstants.WorkingHoursPerDay;
			AssertEquals("Now that shape IsScaled, it has a valid, default scale, so ExplicitDurationMinutes can be set", defaultMinutesPerColumn * 2, workflowShape.ExplicitDurationMinutes);
		}

		#endregion

		#region Completion Criteria

		public void TestSetCompletionCriteria_OnShapesNotLinkedToARealJob()
		{
			MakeCompletionStatementTaskType("ORG", "COM");

			var diagram = Factory.New<BMNCNShape>();
			var subDiagram = Factory.New<BMNCNShape>();
			subDiagram.MakeChildOf(diagram);

			AssertEquals(ShapeTypeList.Codes.Diagram, diagram.BNS_ShapeType);
			AssertEquals(ShapeTypeList.Codes.Diagram, subDiagram.BNS_ShapeType);

			var network = CreateNetwork(diagram);

			((IProposedNetworkEntity)diagram).CompletionCriteria =
@"Thing 1 is done
Thing 2 is done";

			((IProposedNetworkEntity)subDiagram).CompletionCriteria =
@"Other thing 1 is done
Other thing 2 is done";

			AssertEquals(@"Thing 1 is done
Thing 2 is done", diagram.BNS_CompletionStatements);

			AssertEquals(@"Other thing 1 is done
Other thing 2 is done", subDiagram.BNS_CompletionStatements);

			var jobHeader = CreateJobHeader<OrgHeader>();
			diagram.BNS_RelatedEntityID = jobHeader.PK;

			((IProposedNetworkEntity)diagram).CompletionCriteria =
@"Dat thing 1 is done
Dat thing 2 is done";

			AssertEquals(2, diagram.ProcessHeader.CompletionStatementTasksIncludingChildWorkflowTasks.Count);
			AssertCollectionContains(diagram.ProcessHeader.CompletionStatementTasksIncludingChildWorkflowTasks.Cast<ProcessTask>(), t => t.P9_NotesAsString == "Dat thing 1 is done");
			AssertCollectionContains(diagram.ProcessHeader.CompletionStatementTasksIncludingChildWorkflowTasks.Cast<ProcessTask>(), t => t.P9_NotesAsString == "Dat thing 2 is done");
		}

		public void TestLinkShapeToRealJob_ShouldMoveCompletionStatements()
		{
			MakeCompletionStatementTaskType("ORG", "COM");

			var diagram = Factory.New<BMNCNShape>();
			AssertEquals(ShapeTypeList.Codes.Diagram, diagram.BNS_ShapeType);

			var network = CreateNetwork(diagram);

			((IProposedNetworkEntity)diagram).CompletionCriteria =
@"Thing 1 is done
Thing 2 is done";
			AssertEquals(@"Thing 1 is done
Thing 2 is done", diagram.BNS_CompletionStatements);

			var jobHeader = CreateJobHeader<OrgHeader>();

			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			workflow1.MakePrerequisiteOf(workflow2);
			workflow2.MakePrerequisiteOf(workflow3);

			diagram.BNS_RelatedEntityID = jobHeader.PK;

			AssertEquals(@"Thing 1 is done
Thing 2 is done", ((IProposedNetworkEntity)diagram).CompletionCriteria);
			AssertEquals(ZString.Empty, diagram.BNS_CompletionStatements);

			AssertEquals(2, jobHeader.CompletionStatementTasksIncludingChildWorkflowTasks.Count);
			AssertEquals(0, workflow1.CompletionStatementTasksIncludingChildWorkflowTasks.Count);
			AssertEquals(0, workflow2.CompletionStatementTasksIncludingChildWorkflowTasks.Count);
			AssertEquals(2, workflow3.CompletionStatementTasksIncludingChildWorkflowTasks.Count);
			AssertEquals("Thing 1 is done", workflow3.CompletionStatementTasksIncludingChildWorkflowTasks[0].P9_NotesAsString);
			AssertEquals("Thing 2 is done", workflow3.CompletionStatementTasksIncludingChildWorkflowTasks[1].P9_NotesAsString);
		}

		public void TestLinkShapeToRealJob_WithExistingCompletionStatements_ShouldMergeCompletionStatements()
		{
			MakeCompletionStatementTaskType("ORG", "COM");

			var diagram = Factory.New<BMNCNShape>();
			AssertEquals(ShapeTypeList.Codes.Diagram, diagram.BNS_ShapeType);

			var network = CreateNetwork(diagram);

			((IProposedNetworkEntity)diagram).CompletionCriteria =
@"Thing 1 is done
Thing 2 is done";
			AssertEquals(@"Thing 1 is done
Thing 2 is done", diagram.BNS_CompletionStatements);

			var jobHeader = CreateJobHeader<OrgHeader>();

			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			workflow2.CompletionStatementTasksIncludingChildWorkflowTasks.AddNew().P9_NotesAsString = "This first thing is done";

			workflow1.MakePrerequisiteOf(workflow2);
			workflow2.MakePrerequisiteOf(workflow3);

			diagram.BNS_RelatedEntityID = jobHeader.PK;

			AssertEquals(@"This first thing is done
Thing 1 is done
Thing 2 is done", ((IProposedNetworkEntity)diagram).CompletionCriteria);
			AssertEquals(ZString.Empty, diagram.BNS_CompletionStatements);

			AssertEquals(3, jobHeader.CompletionStatementTasksIncludingChildWorkflowTasks.Count);
			AssertEquals(0, workflow1.CompletionStatementTasksIncludingChildWorkflowTasks.Count);
			AssertEquals(1, workflow2.CompletionStatementTasksIncludingChildWorkflowTasks.Count);
			AssertEquals(2, workflow3.CompletionStatementTasksIncludingChildWorkflowTasks.Count);
			AssertEquals("Thing 1 is done", workflow3.CompletionStatementTasksIncludingChildWorkflowTasks[0].P9_NotesAsString);
			AssertEquals("Thing 2 is done", workflow3.CompletionStatementTasksIncludingChildWorkflowTasks[1].P9_NotesAsString);
		}

		#endregion

		#region Properties

		public void TestJobNumberAndJobName_ForLinkedShapeVariants()
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Try selecting a different signal");
			var org = (OrgHeader)workflow.Parent;
			org.OH_Code = "Tomahna";
			org.OH_FullName = "Reposition the phase";

			var diagram1 = CreateDiagram(Factory, name: "Try the dial on the right");
			var diagram2 = CreateDiagram(Factory, name: "A little more left");

			var network = CreateNetwork(diagram1);
			var entity = (IProposedNetworkEntity)network.Entities.GetInstance(diagram1);

			AssertEquals(string.Empty, entity.JobNumber);
			AssertEquals("Job Name", entity.JobName);

			diagram1.BNS_RelatedEntityID = workflow.PK;

			AssertEquals("Tomahna", entity.JobNumber);
			AssertEquals("JobName really means 'related entity name' which in this case is the workflow's 'Description' field", "Try selecting a different signal", entity.JobName);

			diagram1.BNS_RelatedEntityID = diagram2.PK;

			AssertEquals(string.Empty, entity.JobNumber);
			AssertEquals("A little more left", entity.JobName);
		}

		public void TestChangingShapeName_ForShapeLinkedToDiagram_ShouldNotChangeLinkedShapeName()
		{
			var diagram1 = CreateDiagram(Factory, name: "Try the dial on the right");
			var diagram2 = CreateDiagram(Factory, name: "A little more left");

			NetworkTestCase.LinkToRelatedDiagram(diagram1, diagram2);

			var network = CreateNetwork(diagram1);
			var entity = (IProposedNetworkEntity)network.Entities.GetInstance(diagram1);

			AssertEquals(string.Empty, entity.JobNumber);
			AssertEquals("A little more left", entity.JobName);

			entity.Name = "Try the centre-most dial";

			AssertEquals("Changing Name property should not affect the name of the linked diagram", "A little more left", diagram2.BNS_Name);
			AssertEquals("Changing Name property should update the BNS_Name property of the backing shape", "Try the centre-most dial", diagram1.BNS_Name);

			entity.JobName = "Try the centre-most dial";

			AssertEquals("Changing JobName property should change the name of the linked diagram", "Try the centre-most dial", diagram2.BNS_Name);
		}

		public void TestChangingTextProperties_ShouldChangeAttributesAppropriately()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "WKI");
			var job = BMSTestHelper.CreateJob<IWorkItem>(Factory);
			var workItem = (IWorkItem)job;
			workItem.WKI_Summary = "There's no way it'll go on for longer";

			var jobHeader = BMSTestHelper.CreateJobHeader(job, addDefaultProcessHeaderIfNone: false, description: "Broken Promises");
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Today's the day?");

			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape1 = NetworkTestCase.CreateShape(jobHeader, diagram, name: "Eternal delays");
			var shape2 = NetworkTestCase.CreateShape(workflow, shape1, name: "Defer defer defer");

			var network = NetworkTestCase.CreateNetwork(diagram);

			var shape1AsBound = (IProposedNetworkEntity)network.Entities.GetInstance(shape1);
			var shape2AsBound = (IProposedNetworkEntity)network.Entities.GetInstance(shape2);

			AssertEquals("Eternal delays", shape1AsBound.Name);
			AssertEquals("Defer defer defer", shape2AsBound.Name);

			AssertEquals("There's no way it'll go on for longer", shape1AsBound.JobName);
			AssertEquals("Today's the day?", shape2AsBound.JobName);

			shape1AsBound.Name += " :(";
			shape2AsBound.Name += " |-(";
			shape1AsBound.JobName += " :O";
			shape2AsBound.JobName += " :$";

			CombineAssertions("Property getters should return the values as set", () =>
			{
				AssertEquals("shape1AsBound.Name", "Eternal delays :(", shape1AsBound.Name);
				AssertEquals("shape2AsBound.Name", "Defer defer defer |-(", shape2AsBound.Name);

				AssertEquals("shape1AsBound.JobName", "There's no way it'll go on for longer :O", shape1AsBound.JobName);
				AssertEquals("shape2AsBound.JobName", "Today's the day? :$", shape2AsBound.JobName);
			});

			CombineAssertions("Set values should be stored in the correct places", () =>
			{
				AssertEquals("shape1.BNS_Name", "Eternal delays :(", shape1.BNS_Name);
				AssertEquals("shape2.BNS_Name", "Defer defer defer |-(", shape2.BNS_Name);

				AssertEquals("workItem.WKI_Summary: shapes linked to job-level workflows should display the name of the job", "There's no way it'll go on for longer :O", workItem.WKI_Summary);
				AssertEquals("workflow.FH_CompletionStatement: shapes linked to workflows should display the name of that workflow", "Today's the day? :$", workflow.FH_CompletionStatement);
			});
		}

		public void TestIsStartable_ForJobHeaders()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader1.ProcessHeaders[0];
			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var diagram1 = CreateDiagram(jobHeader1);

			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var workflow2 = jobHeader2.ProcessHeaders[0];
			var task2 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);
			var diagram2 = CreateDiagram(jobHeader2);

			workflow1.MakePrerequisiteOf(workflow2);

			AssertEquals(true, ((IProposedNetworkEntity)diagram1).IsStartable);
			AssertEquals(false, ((IProposedNetworkEntity)diagram2).IsStartable);
		}

		public void TestDescription()
		{
			var shape = Factory.New<BMNCNShape>();
			var entity = (IProposedNetworkEntity)shape;

			AssertEquals(string.Empty, entity.Description);

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var component = system.Components.AddNew();
			component.FC_Name = "Mai Bucket";

			var workflow = ProcessJobHeader.GetForParent(Factory.New<OrgHeader>(), Factory).ProcessHeaders[0];
			shape.BNS_RelatedEntityID = workflow.PK;

			AssertEquals("Mai Bucket", entity.Description);
		}

		public void TestEstimateSummary()
		{
			var shape = Factory.New<BMNCNShape>();
			var entity = (IProposedNetworkEntity)shape;

			AssertEquals(string.Empty, entity.EstimateSummary);

			var system = CreateSystem("ORG");
			var workflow = ProcessJobHeader.GetForParent(Factory.New<OrgHeader>(), Factory).ProcessHeaders[0];
			shape.BNS_RelatedEntityID = workflow.PK;

			var task = workflow.Parent.WorkflowItems.AddNew();
			task.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();

			AssertEquals("1.0 hrs to 2.0 hrs (1.5 standard estimate).", entity.EstimateSummary);
		}

		public void TestIsOnCriticalPath()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape1 = NetworkTestCase.CreateShape(diagram, ShapeTypeList.Codes.Shape);
			var shape2 = NetworkTestCase.CreateShape(diagram, ShapeTypeList.Codes.Shape);
			var shape3 = NetworkTestCase.CreateShape(diagram, ShapeTypeList.Codes.Shape);

			AssertEquals(false, shape1.IsOnCriticalPath);
			AssertEquals(false, shape2.IsOnCriticalPath);
			AssertEquals(false, shape3.IsOnCriticalPath);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3");

			workflow1.MakePrerequisiteOf(workflow3);

			shape1.BNS_RelatedEntityID = workflow1.PK;
			shape2.BNS_RelatedEntityID = workflow2.PK;
			shape3.BNS_RelatedEntityID = workflow3.PK;

			var task1 = BMSTestHelper.CreateTask(workflow1, lowEstMinutes: 60);
			var task2 = BMSTestHelper.CreateTask(workflow2, lowEstMinutes: 60);
			var task3 = BMSTestHelper.CreateTask(workflow3, lowEstMinutes: 60);

			Factory.Save();

			AssertEquals(true, shape1.IsOnCriticalPath);
			AssertEquals(false, shape2.IsOnCriticalPath);
			AssertEquals(true, shape3.IsOnCriticalPath);
		}

		public void TestIsStartable()
		{
			var shape1 = Factory.New<BMNCNShape>();
			var entity1 = (IProposedNetworkEntity)shape1;
			var shape2 = Factory.New<BMNCNShape>();
			var entity2 = (IProposedNetworkEntity)shape2;

			AssertEquals(true, entity1.IsStartable);
			AssertEquals(true, entity2.IsStartable);

			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<OrgHeader>(), Factory);

			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			workflow1.MakePrerequisiteOf(workflow2);

			shape1.BNS_RelatedEntityID = workflow1.PK;
			shape2.BNS_RelatedEntityID = workflow2.PK;

			var task1 = workflow1.Parent.WorkflowItems.AddNew();
			task1.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();
			task1.P9_FH_ProcessHeader = workflow1.PK;

			AssertEquals(true, entity1.IsStartable);
			AssertEquals(false, entity2.IsStartable);
		}

		public void TestName()
		{
			var shape = Factory.New<BMNCNShape>();
			shape.BNS_Name = "Mai Shape";
			var entity = (IProposedNetworkEntity)shape;

			AssertEquals("Mai Shape", entity.Name);
			entity.Name = "Dat Shape";
			AssertEquals("Dat Shape", shape.BNS_Name);

			var workflow = ProcessJobHeader.GetForParent(Factory.New<OrgHeader>(), Factory).ProcessHeaders[0];
			shape.BNS_RelatedEntityID = workflow.PK;
			workflow.FH_CompletionStatement = "Dis Workflow";

			AssertEquals("Dat Shape", entity.Name);

			entity.Name = "Dat Workflow";
			AssertEquals("Changing shape name shouldn't push changes to attached workflow", "Dis Workflow", workflow.FH_CompletionStatement);
		}

		public void TestName_ShouldBeKeptInSyncForDefaultDiagrams()
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Gimme");

			var defaultDiagram = workflow.JobHeader.GetDefaultDiagram();
			var defaultShape = workflow.GetDefaultShape(defaultDiagram);

			var network = NetworkTestCase.CreateNetwork(defaultDiagram);
			var entity = network.Entities.GetInstance(defaultShape);

			AssertEquals("Gimme", entity.Name);

			entity.Name += " gimme gimme";

			AssertEquals("Gimme gimme gimme", workflow.FH_CompletionStatement);
		}

		public void TestXYCoords()
		{
			var shape = Factory.New<BMNCNShape>();

			var owner = Factory.New<BMNCNShape>();
			shape.MakeChildOf(owner);

			var network = CreateNetwork(owner);

			var entity = shape.AsEntity(network);
			entity.X = 10;
			AssertEquals(10.0, entity.X);
			entity.Y = 100;
			AssertEquals(100.0, entity.Y);
		}

		public void TestJobNameReadOnly()
		{
			CreateSystem("ORG");

			var shape = Factory.New<BMNCNShape>();
			var entity = (IProposedNetworkEntity)shape;

			AssertEquals("Job Name is not readonly on a shape that is not linked to a job", false, entity.JobName_ReadOnly);

			var workflowForParentWithDescription = ProcessJobHeader.GetForParent(Factory.New<OrgHeader>(), Factory).ProcessHeaders[0];
			shape.BNS_RelatedEntityID = workflowForParentWithDescription.PK;
			AssertEquals("Job Name is not readonly on a shape that has a workflow with a job that has a description attribute", false, entity.JobName_ReadOnly);

			var workflowForParentWithoutDescription = ProcessJobHeader.GetForParent((IWorkflowProvider)Factory.New<IDtbBookingInstruction>(), Factory).ProcessHeaders[0];
			shape.BNS_RelatedEntityID = workflowForParentWithoutDescription.PK;
			AssertEquals("Job Name is readonly on a shape that has a workflow with a job that doesnt have a description attribute", true, entity.JobName_ReadOnly);
		}

		#endregion

		#region Notifications

		public void TestNotifications_NoNotifications()
		{
			var shape = Factory.New<BMNCNShape>();
			shape.BNS_Name = "Dat Shape";
			shape.Validation.ValidateBNS_Name();

			AssertEquals(0, ((INetworkEntity)shape).EntityNotifications.Count());
		}

		public void TestNotifications_FromProcessHeader()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "";

			workflow1.Validation.ValidateFH_CompletionStatement();

			var shape1 = Factory.New<BMNCNShape>();
			var shape2 = Factory.New<BMNCNShape>();

			shape1.BNS_RelatedEntityID = workflow1.PK;

			var entity1 = ((INetworkEntity)shape1);

			AssertEquals(1, entity1.EntityNotifications.Count());
			AssertEquals("Error - FH_CompletionStatement: Please enter a Description.", entity1.EntityNotifications.First().Message);
			AssertEquals(EntityNotifcationType.Error, entity1.EntityNotifications.First().NotificationType);
		}

		public void TestNotifications_FromShape()
		{
			var shape1 = Factory.New<BMNCNShape>();
			shape1.BNS_ShapeType = "R8Y";

			shape1.Validation.ValidateBNS_ShapeType();
			var entity1 = ((INetworkEntity)shape1);

			AssertEquals(1, entity1.EntityNotifications.Count());
			AssertEquals("Error - BNS_ShapeType: Enter a valid Shape Type.", entity1.EntityNotifications.First().Message);
			AssertEquals(EntityNotifcationType.Error, entity1.EntityNotifications.First().NotificationType);
		}

		public void TestNotifications_Refresh()
		{
			var shape1 = Factory.New<BMNCNShape>();
			shape1.BNS_ShapeType = "R8Y";

			shape1.Validation.ValidateBNS_ShapeType();
			var entity1 = ((INetworkEntity)shape1);
			AssertEquals(1, entity1.EntityNotifications.Count());

			shape1.BNS_ShapeType = ShapeTypeList.Codes.Diagram;
			shape1.Validation.ValidateBNS_ShapeType();
			AssertEquals(0, entity1.EntityNotifications.Count());
		}

		public void TestNotifications_ErrorsOnLinks()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Please Complete Me1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Please Complete Me2");

			var link1_2 = workflow1.GetOrCreateDependencyLink(workflow2);

			var diagram = CreateDiagram(jobHeader);
			var workflow1Shape = CreateShape(workflow1, diagram);
			var workflow2Shape = CreateShape(workflow2, diagram);

			var attachment = CreateDependencyAttachment(diagram, link1_2, workflow1Shape, workflow2Shape);

			var network = CreateNetwork(diagram);
			var shapeEntity1 = network.Entities.GetInstance(workflow1Shape);
			var shapeEntity2 = network.Entities.GetInstance(workflow2Shape);

			attachment.AddRowError("blah!");

			var entity1 = ((INetworkEntity)shapeEntity1);
			var entity2 = ((INetworkEntity)shapeEntity2);

			AssertContainsExactElementsInAnyOrder("Error - Dependency Arrow: blah!".WrapWithEnumerable(), entity1.EntityNotifications.Select(n => n.Message));
			AssertContainsExactElementsInAnyOrder("Error - Dependency Arrow: blah!".WrapWithEnumerable(), entity2.EntityNotifications.Select(n => n.Message));
		}

		public void TestHasNotifications()
		{
			var hasNotificationsChanged = false;
			var shape = Factory.New<BMNCNShape>();
			shape.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == "HasNotifications")
				{
					hasNotificationsChanged = true;
				}
			};

			AssertEquals(false, shape.HasNotifications);

			shape.BNS_ShapeType = "DAT";
			AssertEquals(true, shape.HasNotifications);
			AssertEquals(true, hasNotificationsChanged);

			shape.BNS_ShapeType = ShapeTypeList.Codes.Diagram;
			AssertEquals(false, shape.HasNotifications);
		}

		#endregion

		#region Hidden Items

		public void TestHiddenEntities()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			Factory.Save();

			var diagram = CreateDiagram(jobHeader);
			var workflow1Shape = CreateShape(workflow1, diagram);
			workflow1Shape.MakeChildOf(diagram);
			var network = CreateNetwork(diagram);
			AssertEquals(1, network.Entities.GetInstance(diagram).HiddenEntities.Count);
			AssertEquals(workflow2, network.Entities.GetInstance(diagram).HiddenEntities[0]);
		}

		public void TestHiddenEntities_ShouldOrderByName()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Two");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "One");

			Factory.Save();

			var diagram = CreateDiagram(jobHeader);
			var network = CreateNetwork(diagram);

			AssertEquals(2, network.Entities.GetInstance(diagram).HiddenEntities.Count);
			AssertEquals("One", network.Entities.GetInstance(diagram).HiddenEntities[0].Name);
			AssertEquals("Two", network.Entities.GetInstance(diagram).HiddenEntities[1].Name);
		}

		public void TestHiddenRelationships()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			var workflow4 = jobHeader.ProcessHeaders.AddNew();

			var link1_2 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link2_3 = workflow2.GetOrCreateDependencyLink(workflow3);
			var link3_4 = workflow3.GetOrCreateDependencyLink(workflow4);

			var diagram = CreateDiagram(jobHeader);
			var workflow1Shape = CreateShape(workflow1, diagram);
			var workflow2Shape = CreateShape(workflow2, diagram);
			var workflow3Shape = CreateShape(workflow3, diagram);

			var link1_2Attachment = CreateDependencyAttachment(diagram, link1_2, workflow1Shape, workflow2Shape);
			var network = CreateNetwork(diagram);

			AssertEquals("Should be just one hiddent relationship: 1->2 is already shown, 3->4 not included because shape4 is not shown on this diagram", 1, network.Entities.GetInstance(diagram).HiddenRelationships.Count);
			AssertEquals(link2_3, network.Entities.GetInstance(diagram).HiddenRelationships[0]);
		}

		public void TestHiddenRelationships_ShouldOrderByDisplayText()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Two");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Three");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "One");

			workflow1.GetOrCreateDependencyLink(workflow2);
			workflow2.GetOrCreateDependencyLink(workflow3);

			var diagram = CreateDiagram(jobHeader);
			CreateShape(workflow1, diagram);
			CreateShape(workflow2, diagram);
			CreateShape(workflow3, diagram);

			var network = CreateNetwork(diagram);
			AssertEquals(2, network.Entities.GetInstance(diagram).HiddenRelationships.Count);
			AssertEquals("Three -> One", network.Entities.GetInstance(diagram).HiddenRelationships[0].DisplayText);
			AssertEquals("Two -> Three", network.Entities.GetInstance(diagram).HiddenRelationships[1].DisplayText);
		}

		#endregion

		#region Affinity

		public void TestAddShapeAffinityLinksOnRootShape()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var diagramShape = CreateDiagram(jobHeader);
			var workflow1Shape = CreateShape(workflow1, diagramShape);
			var workflow2Shape = CreateShape(workflow2, diagramShape);

			Factory.Save();

			var network = CreateNetwork(diagramShape);

			var shapeAffinity1 = new ShapeAffinity("Colour1", "Benedict", ZGuid.NewZGuid());
			var shapeAffinity2 = new ShapeAffinity("Another Colour", "Steve", ZGuid.NewZGuid());
			network.DiagramShape.ShapeAffinities.Add(shapeAffinity1);
			network.DiagramShape.ShapeAffinities.Add(shapeAffinity2);

			network.DiagramShape.ShapeAffinityLinks.Add(new ShapeAffinityLink(diagramShape.PK, shapeAffinity1.AffinityPK));
			network.DiagramShape.ShapeAffinityLinks.Add(new ShapeAffinityLink(diagramShape.PK, shapeAffinity1.AffinityPK));

			AssertEquals(2, network.DiagramShape.ShapeAffinityLinks.Count);
		}

		public void TestAddShapeAffinityLinksOnChildShape()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var diagramShape = CreateDiagram(jobHeader);
			var workflow1Shape = CreateShape(workflow1, diagramShape);
			var workflow2Shape = CreateShape(workflow2, diagramShape);

			Factory.Save();

			var network = CreateNetwork(diagramShape);

			var shapeAffinity1 = new ShapeAffinity("Colour1", "Benedict", ZGuid.NewZGuid());
			var shapeAffinity2 = new ShapeAffinity("Another Colour", "Steve", ZGuid.NewZGuid());
			network.DiagramShape.ShapeAffinities.Add(shapeAffinity1);
			network.DiagramShape.ShapeAffinities.Add(shapeAffinity2);

			network.DiagramShape.ShapeAffinityLinks.Add(new ShapeAffinityLink(workflow1Shape.PK, shapeAffinity1.AffinityPK));
			network.DiagramShape.ShapeAffinityLinks.Add(new ShapeAffinityLink(workflow2Shape.PK, shapeAffinity1.AffinityPK));

			AssertEquals(2, network.DiagramShape.ShapeAffinityLinks.Count);

			AssertEquals(0, workflow1Shape.ShapeAffinityLinks.Count);
			AssertEquals(0, workflow2Shape.ShapeAffinityLinks.Count);
		}

		public void TestShapeAffinities()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var diagramShape = CreateDiagram(jobHeader);
			var workflow1Shape = CreateShape(workflow1, diagramShape);
			var workflow2Shape = CreateShape(workflow2, diagramShape);

			Factory.Save();

			var network = CreateNetwork(diagramShape);

			var shapeAffinity1 = new ShapeAffinity("Colour1", "Benedict", ZGuid.NewZGuid());
			var shapeAffinity2 = new ShapeAffinity("Another Colour", "Steve", ZGuid.NewZGuid());
			network.DiagramShape.ShapeAffinities.Add(shapeAffinity1);
			network.DiagramShape.ShapeAffinities.Add(shapeAffinity2);

			AssertEquals(2, network.DiagramShape.ShapeAffinities.Count);

			AssertEquals(0, workflow1Shape.ShapeAffinities.Count);
			AssertEquals(0, workflow2Shape.ShapeAffinities.Count);
		}

		public void TestAvailableAffinities()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var diagramShape = CreateDiagram(jobHeader);
			var workflow1Shape = CreateShape(workflow1, diagramShape);
			var workflow2Shape = CreateShape(workflow2, diagramShape);

			Factory.Save();

			var network = CreateNetwork(diagramShape);

			var shapeAffinity1 = new ShapeAffinity("Colour1", "Benedict", ZGuid.NewZGuid());
			var shapeAffinity2 = new ShapeAffinity("Another Colour", "Steve", ZGuid.NewZGuid());
			network.DiagramEntity.ShapeAffinities.Add(shapeAffinity1);
			network.DiagramEntity.ShapeAffinities.Add(shapeAffinity2);

			AssertEquals(2, network.DiagramShape.ChildShapes.Count);
			var child1 = ((INetworkEntity)network.DiagramEntity.Children.First());
			var child2 = ((INetworkEntity)network.DiagramEntity.Children.First());
			AssertEquals(2, child1.AvailableAffinities.Count);
			AssertEquals(2, child2.AvailableAffinities.Count);
		}

		public void TestAvailAbleAffinitiesWithLinks()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var diagramShape = CreateDiagram(jobHeader);
			var network = CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;
			var workflow1Shape = CreateShape(workflow1, diagram);

			Factory.Save();

			var shapeAffinity1 = new ShapeAffinity("Red", "Benedict", ZGuid.NewZGuid(), 2);
			var shapeAffinity2 = new ShapeAffinity("Blue", "Steve", ZGuid.NewZGuid(), 3);
			network.DiagramShape.ShapeAffinities.Add(shapeAffinity1);
			network.DiagramShape.ShapeAffinities.Add(shapeAffinity2);

			((IDiagramEntity)network.DiagramEntity).CreateAffinityLink(workflow1Shape, shapeAffinity1);

			var appliedAffinities = ((IDiagramEntity)network.DiagramEntity.Children.First()).AppliedAffinities;
			var unappliedAffinities = ((IDiagramEntity)network.DiagramEntity.Children.First()).AvailableAffinities;
			AssertEquals(1, appliedAffinities.Count);
			AssertEquals(1, unappliedAffinities.Count);

			var appliedAffinity = appliedAffinities.First();
			var nonAppliedAffinity = unappliedAffinities.First();

			AssertEquals(Color.Red, appliedAffinity.Colour);
			AssertEquals(2, appliedAffinity.AllowedConcurrency);
			AssertEquals(Color.Blue, nonAppliedAffinity.Colour);
			AssertEquals(3, nonAppliedAffinity.AllowedConcurrency);
		}

		public void TestLoadOfShapeAndShapeAffinities()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var diagramShape = CreateDiagram(jobHeader);
			var workflow1Shape = CreateShape(workflow1, diagramShape);

			Factory.Save();

			var network = CreateNetwork(diagramShape);

			var shapeAffinity1 = diagramShape.ShapeAffinities.AddNew();
			shapeAffinity1.Color = "Another Colour";
			shapeAffinity1.Name = "Steve";
			shapeAffinity1.AffinityPK = ZGuid.NewZGuid();
			var shapeAffinity2 = diagramShape.ShapeAffinities.AddNew();
			shapeAffinity2.Color = "TheBestColour";
			shapeAffinity2.Name = "Benedict";
			shapeAffinity2.AffinityPK = ZGuid.NewZGuid();

			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var loadedShape = newFactory.Load<BMNCNShape>(diagramShape.PK);
			AssertEquals(1, loadedShape.ChildShapes.Count);
			AssertEquals(2, loadedShape.ShapeAffinities.Count);
			AssertEquals(true, loadedShape.ShapeAffinities.Cast<ShapeAffinity>().Any(sa => sa.Color == "TheBestColour"));
			AssertEquals(true, loadedShape.ShapeAffinities.Cast<ShapeAffinity>().Any(sa => sa.Color == "Another Colour"));
		}

		public void TestCreateAffinityLink()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var diagramShape = CreateDiagram(jobHeader);
			var workflow1Shape = CreateShape(workflow1, diagramShape, ShapeTypeList.Codes.Shape);
			var workflow2Shape = CreateShape(workflow2, diagramShape, ShapeTypeList.Codes.Shape);

			Factory.Save();

			var network = CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;

			var shapeAffinity1 = new ShapeAffinity("Colour1", "Benedict", ZGuid.NewZGuid());
			var shapeAffinity2 = new ShapeAffinity("Another Colour", "Steve", ZGuid.NewZGuid());
			network.DiagramShape.ShapeAffinities.Add(shapeAffinity1);
			network.DiagramShape.ShapeAffinities.Add(shapeAffinity2);

			AssertEquals(0, network.DiagramShape.ShapeAffinityLinks.Count);

			((IDiagramEntity)diagram).CreateAffinityLink(workflow1Shape, shapeAffinity1);

			AssertEquals(true, diagramShape.HasChanges);

			AssertEquals(1, network.DiagramShape.ShapeAffinityLinks.Count);

			var onlyLink = ((ShapeAffinityLink)diagramShape.ShapeAffinityLinks.First());

			AssertEquals(workflow1Shape.PK, onlyLink.ShapePK);
			AssertEquals(shapeAffinity1.AffinityPK, onlyLink.ShapeAffinityPK);
		}

		public void TestRemoveAffinityLink()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];

			var diagramShape = CreateDiagram(jobHeader);
			var workflow1Shape = CreateShape(workflow1, diagramShape);

			Factory.Save();

			var network = CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;

			var shapeAffinity1 = new ShapeAffinity("Colour1", "Benedict", ZGuid.NewZGuid());
			var shapeAffinity2 = new ShapeAffinity("Another Colour", "Steve", ZGuid.NewZGuid());
			network.DiagramShape.ShapeAffinities.Add(shapeAffinity1);
			network.DiagramShape.ShapeAffinities.Add(shapeAffinity2);

			network.DiagramShape.ShapeAffinityLinks.Add(new ShapeAffinityLink(workflow1Shape.PK, shapeAffinity1.AffinityPK));
			network.DiagramShape.ShapeAffinityLinks.Add(new ShapeAffinityLink(workflow1Shape.PK, shapeAffinity2.AffinityPK));

			AssertEquals(2, network.DiagramShape.ShapeAffinityLinks.Count);

			((IDiagramEntity)diagram).RemoveAffinityLink(workflow1Shape, shapeAffinity1);

			AssertEquals(true, diagramShape.HasChanges);

			AssertEquals(2, diagramShape.ShapeAffinityLinks.Count);
			var onlyAppliedLinkLeft = diagramShape.ShapeAffinityLinks.Cast<ShapeAffinityLink>().First(l => l.IsApplied);

			AssertEquals(workflow1Shape.PK, onlyAppliedLinkLeft.ShapePK);
			AssertEquals(shapeAffinity2.AffinityPK, onlyAppliedLinkLeft.ShapeAffinityPK);
		}

		[TestDate(2014, 3, 3)]
		public void TestAddAffinity_ShouldSetTimestamp()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var diagramShape = CreateDiagram(jobHeader1);
			var network = CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;
			var subDiagram = CreateShape(jobHeader2, diagram);

			var diagramEntity = (IDiagramEntity)diagram;
			var subDiagramEntity = (INetworkEntity)subDiagram;

			var affinity1 = diagram.ShapeAffinities.AddNew();
			affinity1.Name = "Benedict Cumberbatch";
			affinity1.Color = "Hot Pink";
			var affinity2 = diagram.ShapeAffinities.AddNew();
			affinity2.Name = "Martin Freeman";
			affinity2.Color = "Lime Green";

			var date = ZDateTime.UtcNow;

			diagramEntity.CreateAffinityLink(subDiagramEntity, affinity1);
			AssertEquals(1, diagram.ShapeAffinityLinks.Count);
			var link = diagram.ShapeAffinityLinks[0];
			AssertAffinityLink(link, subDiagramEntity, affinity1.AffinityPK, true, date, ZDateTime.Empty);

			date = TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);

			((IDiagramEntity)diagram).RemoveAffinityLink(subDiagramEntity, affinity1);
			AssertEquals(1, diagram.ShapeAffinityLinks.Count);
			AssertAffinityLink(link, subDiagramEntity, affinity1.AffinityPK, false, date.AddDays(-1), date);

			date = TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);

			diagramEntity.CreateAffinityLink(subDiagramEntity, affinity1);
			AssertEquals(2, diagram.ShapeAffinityLinks.Count);
			link = diagram.ShapeAffinityLinks[1];
			AssertAffinityLink(link, subDiagramEntity, affinity1.AffinityPK, true, date, ZDateTime.Empty);

			date = TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);

			diagramEntity.CreateAffinityLink(subDiagramEntity, affinity2);
			AssertEquals(3, diagram.ShapeAffinityLinks.Count);
			link = diagram.ShapeAffinityLinks[2];
			AssertAffinityLink(link, subDiagramEntity, affinity2.AffinityPK, true, date, ZDateTime.Empty);
		}

		public void TestRemoveAffinity_AfterSavingDiagram()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var diagram = CreateDiagram(jobHeader1);
			var subDiagram = CreateShape(jobHeader2, diagram);

			var subDiagramEntity = (INetworkEntity)subDiagram;

			var network = CreateNetwork(diagram);
			var diagramEntity = (IDiagramEntity)network.DiagramEntity;

			var affinity1 = diagram.ShapeAffinities.AddNew();
			affinity1.Name = "Cumberbatchy";
			affinity1.Color = "Hot Pink";
			var affinity2 = diagram.ShapeAffinities.AddNew();
			affinity2.Name = "Owen Green";
			affinity2.Color = "Lime Green";

			diagramEntity.CreateAffinityLink(subDiagramEntity, affinity1);
			AssertEquals(1, diagram.ShapeAffinityLinks.Count);
			AssertEquals(true, diagram.ShapeAffinityLinks[0].IsApplied);

			Factory.Save();

			diagramEntity.RemoveAffinityLink(subDiagramEntity, affinity1);
			AssertEquals(false, diagram.ShapeAffinityLinks[0].IsApplied);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var newDiagram = newFactory.Load<BMNCNShape>(diagram.PK);
			var newSubDiagram = newFactory.Load<BMNCNShape>(subDiagram.PK);

			AssertEquals(false, newDiagram.ShapeAffinityLinks[0].IsApplied);
		}

		void AssertAffinityLink(ShapeAffinityLink link, INetworkEntity entity, ZGuid affinityPK, bool isApplied, ZDateTime appliedTime, ZDateTime removedTime)
		{
			AssertEquals(entity.EntityPK, link.ShapePK);
			AssertEquals(affinityPK, link.ShapeAffinityPK);
			AssertEquals(isApplied, link.IsApplied);
			AssertEquals(appliedTime, link.TimeApplied);
			AssertEquals(removedTime, link.TimeRemoved);

			if (isApplied)
			{
				AssertCollectionContains(entity.AppliedAffinities, a => a.AffinityPK == affinityPK);
				AssertCollectionNotContains(entity.AvailableAffinities, a => a.AffinityPK == affinityPK);
			}
			else
			{
				AssertCollectionNotContains(entity.AppliedAffinities, a => a.AffinityPK == affinityPK);
				AssertCollectionContains(entity.AvailableAffinities, a => a.AffinityPK == affinityPK);
			}
		}

		#endregion

		#region SupportedActions

		#region NetworkActions

		public void TestSupportedNetworkActions()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			var diagram = Factory.New<BMNCNShape>();
			var subDiagram = Factory.New<BMNCNShape>();
			AttachChildToParent(subDiagram, diagram);

			var workflow1Shape = CreateShape(workflow1, diagram);
			var workflow2Shape = CreateShape(workflow2, diagram);
			var dependency = CreateDependencyAttachment(diagram, link, workflow1Shape, workflow2Shape);

			var jobShape = CreateShape(jobHeader, diagram);

			var defaultDiagram = jobHeader.GetDefaultDiagram();
			var defaultWorkflowShape = defaultDiagram.ChildShapes.First();

			var networkViewModel = CreateNetworkViewModel(diagram);
			var defaultNetworkViewModel = CreateNetworkViewModel(defaultDiagram);

			AssertSupportedCoreCustomNetworkActions(networkViewModel, diagram,
				enabledActionTypes: new[]
				{
					typeof(ExtendedNetworkAction),
					typeof(CloneDiagramAction),
					typeof(OpenShapeListAction),
					typeof(SwitchToScaledModeAction),
				},
				applicableButNotEnabledActionTypes: new[]
				{
					typeof(ToggleResourceDependencyVisibilityAction),
					typeof(ConvertToWorkflowsAction)
				},
				notApplicableActionTypes: new[]
				{
					typeof(ApproveDiagramAction),
					typeof(AddBufferAction),
					typeof(CreateResourceDependencyAction),
					typeof(PinShapeAction),
					typeof(UnpinShapeAction),
					typeof(SuggestBufferAction),
					typeof(PushAllEntitiesAction),
					typeof(AcceptBufferAction),
					typeof(ApproveNonApprovedShapesAction),
					typeof(DecoupleAction)
				});

			AssertSupportedCoreCustomNetworkActions(networkViewModel, subDiagram,
				enabledActionTypes: new[]
				{
					typeof(ExtendedNetworkAction)
				},
				applicableButNotEnabledActionTypes: new[]
				{
					typeof(ConvertToWorkflowsAction)
				},
				notApplicableActionTypes: new[]
				{
					typeof(CloneDiagramAction),
					typeof(OpenShapeListAction),
					typeof(SwitchToScaledModeAction),
					typeof(ToggleResourceDependencyVisibilityAction),
					typeof(AddBufferAction),
					typeof(CreateResourceDependencyAction),
					typeof(PinShapeAction),
					typeof(UnpinShapeAction),
					typeof(SuggestBufferAction),
					typeof(PushAllEntitiesAction),
					typeof(AcceptBufferAction),
					typeof(ApproveDiagramAction),
					typeof(ApproveNonApprovedShapesAction),
					typeof(DecoupleAction)
				});

			AssertSupportedCoreCustomNetworkActions(networkViewModel, workflow1Shape,
				enabledActionTypes: new[]
				{
					typeof(ExtendedNetworkAction)
				},
				applicableButNotEnabledActionTypes: new[]
				{
					typeof(ConvertToWorkflowsAction)
				},
				notApplicableActionTypes: new[]
				{
					typeof(CloneDiagramAction),
					typeof(OpenShapeListAction),
					typeof(SwitchToScaledModeAction),
					typeof(ToggleResourceDependencyVisibilityAction),
					typeof(AddBufferAction),
					typeof(CreateResourceDependencyAction),
					typeof(PinShapeAction),
					typeof(UnpinShapeAction),
					typeof(SuggestBufferAction),
					typeof(PushAllEntitiesAction),
					typeof(AcceptBufferAction),
					typeof(ApproveDiagramAction),
					typeof(ApproveNonApprovedShapesAction),
					typeof(DecoupleAction)
				});

			AssertSupportedCoreCustomNetworkActions(defaultNetworkViewModel, defaultDiagram,
				enabledActionTypes: new[]
				{
					typeof(ExtendedNetworkAction),
					typeof(CloneDiagramAction)
				},
				applicableButNotEnabledActionTypes: Type.EmptyTypes,
				notApplicableActionTypes: new[]
				{
					typeof(SwitchToScaledModeAction),
					typeof(ToggleResourceDependencyVisibilityAction),
					typeof(CreateResourceDependencyAction),
					typeof(AddBufferAction),
					typeof(PinShapeAction),
					typeof(UnpinShapeAction),
					typeof(SuggestBufferAction),
					typeof(PushAllEntitiesAction),
					typeof(AcceptBufferAction),
					typeof(ApproveDiagramAction),
					typeof(ApproveNonApprovedShapesAction),
					typeof(ConvertToWorkflowsAction),
					typeof(DecoupleAction)
				});

			AssertSupportedCoreCustomNetworkActions(defaultNetworkViewModel, defaultWorkflowShape,
				enabledActionTypes: new[]
				{
					typeof(ExtendedNetworkAction)
				},
				applicableButNotEnabledActionTypes: Type.EmptyTypes,
				notApplicableActionTypes: new[]
				{
					typeof(CloneDiagramAction),
					typeof(OpenShapeListAction),
					typeof(SwitchToScaledModeAction),
					typeof(ToggleResourceDependencyVisibilityAction),
					typeof(CreateResourceDependencyAction),
					typeof(AddBufferAction),
					typeof(PinShapeAction),
					typeof(UnpinShapeAction),
					typeof(SuggestBufferAction),
					typeof(PushAllEntitiesAction),
					typeof(AcceptBufferAction),
					typeof(ApproveDiagramAction),
					typeof(ApproveNonApprovedShapesAction),
					typeof(ConvertToWorkflowsAction),
					typeof(DecoupleAction)
				});

			AssertSupportedCoreCustomNetworkActions(networkViewModel, jobShape,
				enabledActionTypes: new[]
				{
					typeof(ExtendedNetworkAction)
				},
				applicableButNotEnabledActionTypes: new[]
				{
					typeof(ConvertToWorkflowsAction)
				},
				notApplicableActionTypes: new[]
				{
					typeof(CloneDiagramAction),
					typeof(OpenShapeListAction),
					typeof(SwitchToScaledModeAction),
					typeof(ToggleResourceDependencyVisibilityAction),
					typeof(CreateResourceDependencyAction),
					typeof(AddBufferAction),
					typeof(PinShapeAction),
					typeof(UnpinShapeAction),
					typeof(SuggestBufferAction),
					typeof(PushAllEntitiesAction),
					typeof(AcceptBufferAction),
					typeof(ApproveDiagramAction),
					typeof(ApproveNonApprovedShapesAction),
					typeof(DecoupleAction)
				});
		}

		public void TestSupportedNetworkActions_PinnedShape()
		{
			var diagram = CreateDiagram(Factory, name: "Root");
			var shape = CreateShape(diagram, "Shape");
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			shape.PinShape(networkViewModel);

			AssertEquals(true, shape.AsEntity(network).IsPinned);

			AssertSupportedCoreCustomNetworkActions(networkViewModel, shape,
				enabledActionTypes: new[]
				{
					typeof(ExtendedNetworkAction),
					typeof(PushAllEntitiesAction),
					typeof(UnpinShapeAction),
				},
				applicableButNotEnabledActionTypes: new[]
				{
					typeof(AddBufferAction),
					typeof(ConvertToWorkflowsAction),
					typeof(CreateResourceDependencyAction),
				},
				notApplicableActionTypes: new[]
				{
					typeof(AcceptBufferAction),
					typeof(ApproveDiagramAction),
					typeof(ApproveNonApprovedShapesAction),
					typeof(CloneDiagramAction),
					typeof(OpenShapeListAction),
					typeof(DecoupleAction),
					typeof(PinShapeAction),
					typeof(SuggestBufferAction),
					typeof(SwitchToScaledModeAction),
					typeof(ToggleResourceDependencyVisibilityAction),
				});
		}

		public void TestSupportedNetworkActions_ScaledDiagram()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			var diagram = CreateDiagram(jobHeader, isScaled: true);
			var subDiagram = Factory.New<BMNCNShape>();
			AttachChildToParent(subDiagram, diagram);

			var workflow1Shape = CreateShape(workflow1, diagram);
			var workflow2Shape = CreateShape(workflow2, diagram);
			var dependency = CreateDependencyAttachment(diagram, link, workflow1Shape, workflow2Shape);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var bufferShape = networkViewModel.GetJobNetwork().Entities.GetInstance(dependency).CreateBuffer();
			bufferShape.Shape.Active = false;

			networkViewModel.CreateNodeViewModelsForEntitiesToLetNetworkActionsWork();

			AssertSupportedCoreCustomNetworkActions(networkViewModel, diagram,
				enabledActionTypes: new[]
				{
					typeof(ExtendedNetworkAction),
					typeof(CloneDiagramAction),
					typeof(OpenShapeListAction),
					typeof(SuggestBufferAction),
					typeof(PushAllEntitiesAction),
					typeof(ApproveDiagramAction),
					typeof(ConvertToWorkflowsAction)
				},
				applicableButNotEnabledActionTypes: new[]
				{
					typeof(SwitchToScaledModeAction),
					typeof(ToggleResourceDependencyVisibilityAction),
				},
				notApplicableActionTypes: new[]
				{
					typeof(AddBufferAction),
					typeof(CreateResourceDependencyAction),
					typeof(PinShapeAction),
					typeof(UnpinShapeAction),
					typeof(AcceptBufferAction),
					typeof(ApproveNonApprovedShapesAction),
					typeof(DecoupleAction)
				});

			AssertSupportedCoreCustomNetworkActions(networkViewModel, subDiagram,
				enabledActionTypes: new[]
				{
					typeof(ExtendedNetworkAction),
					typeof(PinShapeAction),
					typeof(PushAllEntitiesAction),
					typeof(ConvertToWorkflowsAction)
				},
				applicableButNotEnabledActionTypes: new[]
				{
					typeof(AddBufferAction),
					typeof(CreateResourceDependencyAction),
				},
				notApplicableActionTypes: new[]
				{
					typeof(CloneDiagramAction),
					typeof(OpenShapeListAction),
					typeof(SwitchToScaledModeAction),
					typeof(ToggleResourceDependencyVisibilityAction),
					typeof(SuggestBufferAction),
					typeof(AcceptBufferAction),
					typeof(ApproveDiagramAction),
					typeof(ApproveNonApprovedShapesAction),
					typeof(DecoupleAction),
					typeof(UnpinShapeAction),
				});

			AssertSupportedCoreCustomNetworkActions(networkViewModel, workflow1Shape,
				enabledActionTypes: new[]
				{
					typeof(ExtendedNetworkAction),
					typeof(PinShapeAction),
					typeof(PushAllEntitiesAction),
				},
				applicableButNotEnabledActionTypes: new[]
				{
					typeof(AddBufferAction),
					typeof(CreateResourceDependencyAction),
					typeof(ConvertToWorkflowsAction)
				},
				notApplicableActionTypes: new[]
				{
					typeof(CloneDiagramAction),
					typeof(OpenShapeListAction),
					typeof(SwitchToScaledModeAction),
					typeof(ToggleResourceDependencyVisibilityAction),
					typeof(SuggestBufferAction),
					typeof(AcceptBufferAction),
					typeof(ApproveDiagramAction),
					typeof(ApproveNonApprovedShapesAction),
					typeof(DecoupleAction),
					typeof(UnpinShapeAction),
				});

			AssertSupportedCoreCustomNetworkActions(networkViewModel, bufferShape,
				enabledActionTypes: new[]
				{
					typeof(PinShapeAction),
					typeof(PushAllEntitiesAction),
					typeof(AcceptBufferAction)
				},
				applicableButNotEnabledActionTypes: Type.EmptyTypes,
				notApplicableActionTypes: new[]
				{
					typeof(CreateResourceDependencyAction),
					typeof(CloneDiagramAction),
					typeof(OpenShapeListAction),
					typeof(SwitchToScaledModeAction),
					typeof(ToggleResourceDependencyVisibilityAction),
					typeof(AddBufferAction),
					typeof(SuggestBufferAction),
					typeof(ApproveDiagramAction),
					typeof(ApproveNonApprovedShapesAction),
					typeof(ConvertToWorkflowsAction),
					typeof(DecoupleAction),
					typeof(UnpinShapeAction),
				});
		}

		public void TestSupportedNetworkActions_DeletedShape()
		{
			var shape = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(shape);

			shape.Delete();
			AssertEquals(0, networkViewModel.GetCustomNetworkActions().Count(a => a.IsApplicable().IsAllowed));
		}

		public void TestSupportedNetworkActions_OrphanApprovedDiagram()
		{
			var staff = CreateStaffInCurrentBranchDept("DRO", "Drongo");
			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			diagram.EarliestStartTimeUtc = ZDateTime.UtcNow;

			var shape1 = networkViewModel.CreateNewShape(diagram);
			shape1.Name = "shape1";

			shape1.Approve(staff.GS_Code);

			AssertSupportedCoreCustomNetworkActions(networkViewModel, shape1,
				enabledActionTypes: new[]
				{
					typeof(UnapproveDiagramAction),
				},
				applicableButNotEnabledActionTypes: Type.EmptyTypes,
				notApplicableActionTypes: Type.EmptyTypes
				);
		}

		public void TestSupportedNetworkActions_ApprovedDiagram()
		{
			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			diagram.EarliestStartTimeUtc = ZDateTime.UtcNow;

			var shape1 = networkViewModel.CreateNewShape(diagram);
			var shape2 = networkViewModel.CreateNewShape(diagram);

			shape1.Name = "shape1";
			shape2.Name = "shape2";

			var arrow = network.CreateRelationship(shape1, shape2);

			networkViewModel.ToggleApproval();

			AssertSupportedCoreCustomNetworkActions(networkViewModel, diagram,
				enabledActionTypes: new[]
				{
					typeof(UnapproveDiagramAction),
				},
				applicableButNotEnabledActionTypes: new[]
				{
					typeof(ApproveNonApprovedShapesAction)
				},
				notApplicableActionTypes: new[]
				{
					typeof(ApproveDiagramAction),
					typeof(DecoupleAction)
				});

			AssertSupportedCoreCustomNetworkActions(networkViewModel, shape1,
				enabledActionTypes: Type.EmptyTypes,
				applicableButNotEnabledActionTypes: new[]
				{
					typeof(DecoupleAction)
				},
				notApplicableActionTypes: new[]
				{
					typeof(ApproveDiagramAction),
					typeof(ApproveNonApprovedShapesAction),
				});

			AssertSupportedCoreCustomNetworkActions(networkViewModel, shape2,
				enabledActionTypes: new[]
				{
					typeof(DecoupleAction)
				},
				applicableButNotEnabledActionTypes: Type.EmptyTypes,
				notApplicableActionTypes: Type.EmptyTypes);
		}

		public void TestSupportedNetworkActions_ShouldSeparateNonCcpmAndCcpmItems()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();

			CombineAssertions("Action locations", () =>
			{
				var actionItems = networkViewModel.GetSecondLevelNetworkActionsMenuItems_ForTesting(diagram, "Actions");
				AssertEquals("Show Extended Network", actionItems[0].Name);
				AssertEquals("Clone Diagram", actionItems[1].Name);
				AssertEquals("Open Shape List", actionItems[2].Name);
				AssertEquals("Create Job", actionItems[3].Name);
				AssertEquals("Convert Shape to Workflow", actionItems[4].Name);
				AssertEquals("Set Status", actionItems[5].Name);
				AssertEquals("Validate Workflow Loops", actionItems[6].Name);
				AssertNull("Should insert separator between non-CCPM and CCPM items", actionItems[7]);
				AssertEquals("Approve Diagram", actionItems[8].Name);
				AssertEquals("Switch to Scaled Mode", actionItems[9].Name);
				AssertEquals("Show Resource Dependencies", actionItems[10].Name);
			});
		}

		public void TestSupportedNetworkActions_CreateProjectBuffer()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape1 = NetworkTestCase.CreateShape(diagram);
			var shape2 = NetworkTestCase.CreateShape(diagram);
			var shape3 = NetworkTestCase.CreateShape(diagram);
			var shape4 = NetworkTestCase.CreateShape(diagram);

			shape1.MakeVisiblePrerequisiteOf(shape2, diagram);
			shape2.MakeVisiblePrerequisiteOf(shape3, diagram);
			shape4.MakeVisiblePrerequisiteOf(shape3, diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			networkViewModel.GetJobNetwork().SwitchToScaled();

			AssertEquals(true, shape1.IsCriticalPath);
			AssertEquals(true, shape2.IsCriticalPath);
			AssertEquals(true, shape3.IsCriticalPath);
			AssertEquals(false, shape4.IsCriticalPath);

			AssertNotNull("Diagram surface should have Create Project Buffer action", networkViewModel.GetCoreCustomNetworkAction_ForTesting<CreateProjectBufferAction>());

			AssertEquals(true, networkViewModel.IsCoreCustomNetworkActionApplicableToEntity<CreateProjectBufferAction>(shape1));
			AssertEquals(false, networkViewModel.IsCoreCustomNetworkActionEnabledForEntity<CreateProjectBufferAction>(shape1));

			AssertEquals(true, networkViewModel.IsCoreCustomNetworkActionApplicableToEntity<CreateProjectBufferAction>(shape2));
			AssertEquals(false, networkViewModel.IsCoreCustomNetworkActionEnabledForEntity<CreateProjectBufferAction>(shape2));

			AssertEquals(true, networkViewModel.IsCoreCustomNetworkActionApplicableToEntity<CreateProjectBufferAction>(shape3));
			AssertEquals("Last shape on the critical chain should have Create Project Buffer action enabled", true, networkViewModel.IsCoreCustomNetworkActionEnabledForEntity<CreateProjectBufferAction>(shape3));

			AssertEquals(true, networkViewModel.IsCoreCustomNetworkActionApplicableToEntity<CreateProjectBufferAction>(shape4));
			AssertEquals(false, networkViewModel.IsCoreCustomNetworkActionEnabledForEntity<CreateProjectBufferAction>(shape4));
		}

		#endregion

		#region CreateEntityActions

		public void TestSupportedCreateEntityActions()
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			var diagram = CreateDiagram(jobHeader);
			var defaultDiagram = jobHeader.GetDefaultDiagram();
			var unLinkedDiagram = CreateDiagram(Factory);

			var workflowShape = CreateShape(workflow, diagram);
			var defaultWorkflowShape = workflow.GetDefaultShape(defaultDiagram);
			var unLinkedShape = CreateShape(unLinkedDiagram);

			var annotation = CreateShape(diagram, shapeType: ShapeTypeList.Codes.Annotation);
			var buffer = CreateShape(diagram, shapeType: ShapeTypeList.Codes.Buffer);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var defaultDiagramNetworkViewModel = CreateNetworkViewModel(defaultDiagram);
			var unLinkedDiagramNetworkViewModel = CreateNetworkViewModel(unLinkedDiagram);

			AssertSupportedCreateActions(networkViewModel, diagram,
					enabledActionTypes: new[]
				{
					typeof(CreateShapeAction),
					typeof(CreateWorkflowAction),
					typeof(CreateJobActionCollection),
					typeof(CreateAnnotationAction)
				},
				applicableButNotEnabledActionTypes: new[]
				{
					typeof(CreateShapeFromClipboardAction),
				},
				notApplicableActionTypes: Type.EmptyTypes);

			AssertSupportedCreateActions(defaultDiagramNetworkViewModel, defaultDiagram,
				enabledActionTypes: new[]
				{
					typeof(CreateDefaultWorkflowAction),
					typeof(CreateAnnotationAction)
				},
				applicableButNotEnabledActionTypes: Type.EmptyTypes,
				notApplicableActionTypes: new[]
				{
					typeof(CreateShapeAction),
					typeof(CreateWorkflowAction),
					typeof(CreateShapeFromClipboardAction),
					typeof(CreateJobActionCollection),
				});

			AssertSupportedCreateActions(unLinkedDiagramNetworkViewModel, unLinkedDiagram,
				enabledActionTypes: new[]
				{
					typeof(CreateShapeAction),
					typeof(CreateJobActionCollection),
					typeof(CreateAnnotationAction)
				},
				applicableButNotEnabledActionTypes: new[]
				{
					typeof(CreateShapeFromClipboardAction)
				},
				notApplicableActionTypes: new[]
				{
					typeof(CreateWorkflowAction)
				});

			AssertSupportedCreateActions(networkViewModel, workflowShape,
				enabledActionTypes: new[]
				{
					typeof(CreateShapeAction),
					typeof(CreateWorkflowAction),
					typeof(CreateJobActionCollection),
					typeof(CreateAnnotationAction)
				},
				applicableButNotEnabledActionTypes: new[]
				{
					typeof(CreateShapeFromClipboardAction)
				},
				notApplicableActionTypes: Type.EmptyTypes);

			AssertSupportedCreateActions(defaultDiagramNetworkViewModel, defaultWorkflowShape,
				enabledActionTypes: Type.EmptyTypes,
				applicableButNotEnabledActionTypes: Type.EmptyTypes,
				notApplicableActionTypes: new[]
				{
					typeof(CreateShapeAction),
					typeof(CreateWorkflowAction),
					typeof(CreateShapeFromClipboardAction),
					typeof(CreateJobActionCollection),
					typeof(CreateAnnotationAction)
				});

			AssertSupportedCreateActions(unLinkedDiagramNetworkViewModel, unLinkedShape,
				enabledActionTypes: new[]
				{
					typeof(CreateShapeAction),
					typeof(CreateJobActionCollection),
					typeof(CreateAnnotationAction)
				},
				applicableButNotEnabledActionTypes: new[]
				{
					typeof(CreateShapeFromClipboardAction),
				},
				notApplicableActionTypes: new[]
				{
					typeof(CreateWorkflowAction)
				});
		}

		#endregion

		#region Implementation

		static void AssertSupportedCoreCustomNetworkActions(NetworkViewModel networkViewModel, INetworkEntity entity, Type[] enabledActionTypes, Type[] applicableButNotEnabledActionTypes, Type[] notApplicableActionTypes)
		{
			var shape = entity.AsShape();
			AssertSupportedActions(networkViewModel.GetCoreCustomNetworkActions_ForTesting().ToArray(), networkViewModel, shape, enabledActionTypes, applicableButNotEnabledActionTypes, notApplicableActionTypes);
		}

		static void AssertSupportedCreateActions(NetworkViewModel networkViewModel, BMNCNShape shape, Type[] enabledActionTypes, Type[] applicableButNotEnabledActionTypes, Type[] notApplicableActionTypes)
		{
			AssertSupportedActions(networkViewModel.GetCreateEntityActions().ToArray(), networkViewModel, shape, enabledActionTypes, applicableButNotEnabledActionTypes, notApplicableActionTypes);
		}

		static void AssertSupportedActions(INetworkAction[] entityActions, NetworkViewModel networkViewModel, BMNCNShape shape, Type[] enabledActionTypes, Type[] applicableButNotEnabledActionTypes, Type[] notApplicableActionTypes)
		{
			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, shape))
			{
				CombineAssertions(() =>
				{
					var enabledActions = entityActions.Cast<INetworkAction>().Where(a => a.IsEnabled().IsAllowed).ToArray();

					foreach (var type in enabledActionTypes)
					{
						var message = string.Format("Action of type [{1}] should be enabled for shape [{0}]", shape.BNS_Name, type.Name);
						AssertCollectionContains(message, enabledActions, a => a != null && a.GetType() == type);
					}

					var applicableButDisabledActions = entityActions.Cast<INetworkAction>().Where(a => a.IsApplicable().IsAllowed && !a.IsEnabled().IsAllowed).ToArray();

					foreach (var type in applicableButNotEnabledActionTypes)
					{
						var message = string.Format("Action of type [{1}] should be applicable, but not enabled for shape [{0}]", shape.BNS_Name, type.Name);
						AssertCollectionContains(message, applicableButDisabledActions, a => a != null && a.GetType() == type);
					}

					var applicableActions = entityActions.Cast<INetworkAction>().Where(a => a.IsApplicable().IsAllowed).ToArray();

					foreach (var type in notApplicableActionTypes)
					{
						var message = string.Format("Action of type [{1}] should not be applicable to shape [{0}]", shape.BNS_Name, type.Name);
						AssertCollectionNotContains(message, applicableActions, a => a != null && a.GetType() == type);
					}
				});
			}
		}

		#endregion

		#endregion

		#region ShapeType

		public void TestShapeType()
		{
			var shape = Factory.New<BMNCNShape>();
			var entity = (INetworkEntity)shape;
			AssertEquals(ShapeTypeList.Codes.Shape, entity.ShapeType);

			shape.BNS_ShapeType = ShapeTypeList.Codes.Annotation;
			AssertEquals(ShapeTypeList.Codes.Annotation, entity.ShapeType);

			shape.BNS_ShapeType = ShapeTypeList.Codes.Buffer;
			AssertEquals(ShapeTypeList.Codes.Buffer, entity.ShapeType);
		}

		#endregion

		#region CanCreateRelationship

		public void TestCanCreateRelationship()
		{
			var shape1 = Factory.New<BMNCNShape>();
			var shape2 = Factory.New<BMNCNShape>();
			var buffer = Factory.New<BMNCNShape>();
			buffer.BNS_ShapeType = ShapeTypeList.Codes.Buffer;

			AssertEquals(true, shape1.CanCreateRelationship(shape2));
			AssertEquals(true, shape2.CanCreateRelationship(shape1));

			AssertEquals(false, shape1.CanCreateRelationship(buffer));
			AssertEquals(false, shape2.CanCreateRelationship(buffer));

			AssertEquals(false, buffer.CanCreateRelationship(shape1));
			AssertEquals(false, buffer.CanCreateRelationship(shape2));

			AssertEquals(true, shape1.CanCreateRelationship(null));
			AssertEquals(true, shape2.CanCreateRelationship(null));
			AssertEquals(false, buffer.CanCreateRelationship(null));
		}

		#endregion

		#region CanDeleteUnderlyingEntity

		public void TestCanDeleteUnderlyingEntity()
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var diagram = CreateDiagram(jobHeader1);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var workflowShape = networkViewModel.CreateNewWorkflow(diagram);
			var nonLinkedShape = networkViewModel.CreateNewShape(diagram);
			var annotationShape = networkViewModel.CreateNewAnnotation(diagram);
			var subDiagramShape = networkViewModel.CreateNewShape(diagram);
			var jobShape = networkViewModel.CreateNewShape(diagram);

			network.LinkEntity(jobShape, jobHeader2);

			CombineAssertions(() =>
			{
				AssertEquals("workflowShape", true, workflowShape.Shape.CanDeleteUnderlyingEntity);
				AssertEquals("nonLinkedShape", false, nonLinkedShape.Shape.CanDeleteUnderlyingEntity);
				AssertEquals("annotationShape", false, annotationShape.Shape.CanDeleteUnderlyingEntity);
				AssertEquals("subDiagramShape", false, subDiagramShape.Shape.CanDeleteUnderlyingEntity);
				AssertEquals("jobShape", true, jobShape.Shape.CanDeleteUnderlyingEntity);
			});
		}

		public void TestCanDeleteUnderlyingEntity_DoesntThrowUpWhenEntityIsDeleted()
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var diagram = CreateDiagram(jobHeader1);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var workflowShape = networkViewModel.CreateNewWorkflow(diagram);
			var nonLinkedShape = networkViewModel.CreateNewShape(diagram);
			var annotationShape = networkViewModel.CreateNewAnnotation(diagram);
			var subDiagramShape = networkViewModel.CreateNewShape(diagram);
			var jobShape = networkViewModel.CreateNewShape(diagram);

			network.LinkEntity(jobShape, jobHeader2);

			ErrorReporter.Clear();

			foreach (var shape in new[] { workflowShape, nonLinkedShape, annotationShape, subDiagramShape, jobShape })
			{
				shape.Shape.Delete();
			}

			CombineAssertions("All the shapes.Shapes are deleted, so we expect false on all the shapes.", () =>
			{
				AssertEquals("workflowShape", false, workflowShape.Shape.CanDeleteUnderlyingEntity);
				AssertEquals("nonLinkedShape", false, nonLinkedShape.Shape.CanDeleteUnderlyingEntity);
				AssertEquals("annotationShape", false, annotationShape.Shape.CanDeleteUnderlyingEntity);
				AssertEquals("subDiagramShape", false, subDiagramShape.Shape.CanDeleteUnderlyingEntity);
				AssertEquals("jobShape", false, jobShape.Shape.CanDeleteUnderlyingEntity);
			});

			AssertEquals("We had no ErrorReporter reports", string.Empty, ErrorReporter.LastMessageReported);
		}

		#endregion

		#region CanUnlinkEntity

		public void TestCanUnlinkEntity()
		{
			var job = Factory.New<OrgHeader>();
			job.OH_FullName = "Power Rangers";
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var diagram = CreateDiagram(jobHeader);
			var shape = CreateShape(diagram);

			var diagramWithoutParent = CreateDiagram(Factory);

			AssertEquals(true, diagram.CanUnlinkEntity);
			AssertEquals(false, shape.CanUnlinkEntity);
			AssertEquals(false, diagramWithoutParent.CanUnlinkEntity);
		}

		public void TestCanUnlinkEntity_ProcessHeaderIsWorkflow()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = CreateWorkflow(jobHeader, "workflow");
			var shape = CreateDiagram(workflow);

			Assert(workflow.IsWorkflow);
			Assert(shape.CanUnlinkEntity);
		}

		public void TestCanUnlinkEntity_IsDefaultDiagramWorkflow()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = CreateWorkflow(jobHeader, "workflow");
			var defaultDiagram = CreateDefaultDiagram(jobHeader);
			var defaultWorkflowShape = CreateDefaultDiagramWorkflowShape(workflow);

			Assert(!defaultDiagram.CanUnlinkEntity);
			Assert(!defaultWorkflowShape.CanUnlinkEntity);
		}

		public void TestCanUnlinkEntity_IsDefaultDiagramChild()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = CreateWorkflow(jobHeader, "workflow");
			var root = CreateShape(ShapeTypeList.Codes.DefaultDiagram);
			var defaultDiagramChild = CreateShape(workflow, parentShape: root, shapeType: ShapeTypeList.Codes.DefaultWorkflow);

			Assert(!defaultDiagramChild.CanUnlinkEntity);
		}

		#endregion

		#region EntityState

		public void TestEntityState()
		{
			var diagram = CreateDiagram(Factory);
			var viewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = viewModel.GetJobNetwork();
			var root = network.DiagramEntity;
			var shape = CreateShape(root);

			AssertEquals(EntityState.None, shape.EntityState);

			shape.Shape.PinShape(viewModel);
			AssertEquals(EntityState.None | EntityState.Fixed, shape.EntityState);

			shape.Shape.Active = false;
			AssertEquals(EntityState.None | EntityState.Fixed | EntityState.Inactive, shape.EntityState);

			network.DiagramEntity.Approve(GlbStaff.CurrentUser.GS_Code);
			AssertEquals(EntityState.None | EntityState.Fixed | EntityState.Inactive | EntityState.NotApproved, shape.EntityState);

			shape.Approve(GlbStaff.CurrentUser.GS_Code);
			AssertEquals(EntityState.None | EntityState.Fixed | EntityState.Inactive | EntityState.Approved, shape.EntityState);

			try
			{
				shape.Shape.BNS_ShapeType = "AAA";
				AssertEquals(EntityState.None | EntityState.Fixed | EntityState.Inactive | EntityState.NotApproved | EntityState.HasErrors, shape.EntityState);
				AssertEquals("Unhandled type [AAA]", ErrorReporter.LastMessageReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestApprove_ForNonScheduledEntities_ShouldDoNothing()
		{
			var diagram = CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;
			var viewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = viewModel.GetJobNetwork();

			var scheduledShape = CreateShape(diagram);
			AssertEquals(false, scheduledShape.IsNonScheduled);

			var nonscheduledShape = CreateShape(diagram);
			nonscheduledShape.IsNonScheduled = true;

			var scheduledEntity = scheduledShape.AsEntity(network);
			AssertEquals(false, scheduledEntity.IsApproved);

			scheduledEntity.Approve("E");
			AssertEquals(true, scheduledEntity.IsApproved);

			var nonscheduledEntity = nonscheduledShape.AsEntity(network);
			AssertEquals(false, nonscheduledEntity.IsApproved);

			nonscheduledEntity.Approve("E");
			AssertEquals("The shape is non-scheduled, so attampting to approve it should do nothing. SAD!", false, nonscheduledEntity.IsApproved);
		}

		public void TestEntityState_AnnotationShape()
		{
			var root = CreateNetwork(CreateDiagram(Factory)).DiagramEntity;
			var annotation = CreateShape(root, shapeType: ShapeTypeList.Codes.Annotation);

			((IApprovable)annotation).Approve(GlbStaff.CurrentUser.GS_Code);
			AssertEquals(ZString.Empty, annotation.Shape.BNS_GS_NKApprovedBy);
			AssertEquals(EntityState.None, annotation.EntityState);

			((IApprovable)root).Approve(GlbStaff.CurrentUser.GS_Code);
			AssertEquals(ZString.Empty, annotation.Shape.BNS_GS_NKApprovedBy);
			AssertEquals(EntityState.None, annotation.EntityState);
		}

		public void TestEntityState_ForApprovedLinkedShapeOnBufferedDiagram_ShouldFixAlso()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var diagramShape = Factory.New<BMNCNShape>();
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var childShape = CreateShape(workflow, diagram);
			network.SwitchToScaled();
			diagram.ScheduledStartTimeLocal = ZDateTime.Now;

			AssertEquals(EntityState.None, childShape.EntityState);

			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			AssertEquals("Scaled + buffered + approved shapes should be fixed", EntityState.Approved | EntityState.Fixed, childShape.EntityState);

			var newShape = networkViewModel.CreateNewShape(diagram);
			AssertEquals(EntityState.NotApproved, newShape.EntityState);

			new ApproveNonApprovedShapesAction(networkViewModel).ExecuteForEntityWithoutAccessCheck(diagram);
			AssertEquals("All approved shapes are fixed", EntityState.Approved | EntityState.Fixed, newShape.EntityState);

			var anotherJobHeader = CreateJobHeader<OrgHeader>();
			newShape.Shape.BNS_RelatedEntityID = anotherJobHeader.PK;
			AssertEquals("Scaled + buffered + approved shapes should be fixed", EntityState.Approved | EntityState.Fixed, newShape.EntityState);
		}

		#endregion

		#region Clone

		[TestDate(2015, 3, 2)]
		public void TestClone_CloneRealJobHeaderLinks()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var diagram = CreateDiagram(jobHeader);
			var network = CreateNetwork(diagram);

			var clone = (BMNCNShape)diagram.Clone();

			AssertEquals(diagram.ProcessHeader, clone.ProcessHeader);
		}

		[TestDate(2015, 3, 2)]
		public void TestClone_ShapeWithoutJobHeader()
		{
			var diagram = CreateDiagram(Factory);
			var network = CreateNetwork(diagram);

			var clone = (BMNCNShape)diagram.Clone();

			AssertNull(diagram.ProcessHeader);
			AssertNull(clone.ProcessHeader);
		}

		[TestDate(2015, 3, 2)]
		public void TestClone_ShouldDuplicateSchedule()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var diagram = CreateDiagram(jobHeader);
			var network = CreateNetwork(diagram);
			network.SwitchToScaled();

			diagram.IsCriticalPath = true;
			diagram.EarliestStartTimeUtc = ZDateTime.UtcNow;

			AssertNotNull(diagram.ScheduleBizo);

			var clone = (BMNCNShape)diagram.Clone();

			AssertNotEquals(diagram.ScheduleBizo, clone.ScheduleBizo);

			AssertEquals(true, clone.IsCriticalPath);
			AssertEquals(ZDateTime.UtcNow, clone.EarliestStartTimeUtc);
		}

		[TestDate(2015, 4, 13)]
		public void TestCloneDiagramAndAllDescendants_ScheduleShouldNotDoubleOnStupidNetwork()
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = CreateJobHeader<OrgHeader>();
			var diagramShape = CreateDiagram(jobHeader);
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			network.SwitchToScaled();

			diagramShape.IsCriticalPath = true;
			diagramShape.EarliestStartTimeUtc = ZDateTime.UtcNow;

			var unlinkedJobHeader = CreateJobHeader<OrgHeader>();
			var linkedWorkflow = CreateWorkflow(unlinkedJobHeader, "It's Christmas!");

			var childShape = networkViewModel.CreateNewShape(diagram);
			var workflowShape = networkViewModel.CreateNewWorkflow(diagram);
			var subDiagram = networkViewModel.CreateNewShape(diagram);
			var grandChildShape = networkViewModel.CreateNewShape(subDiagram);

			childShape.Name = nameof(childShape);
			workflowShape.Name = nameof(workflowShape);
			subDiagram.Name = nameof(subDiagram);
			grandChildShape.Name = nameof(grandChildShape);

			var affinity = NetworkTestCase.CreateAffinity(diagram.Shape);
			var affinityLink = childShape.Shape.ShapeAffinityLinks.AddNew();
			affinityLink.ShapeAffinityPK = affinity.PK;

			CombineAssertions("Should create schedules for cloned shapes", () =>
			{
				foreach (var shape in network.Entities.ShapeEntities)
				{
					AssertNotNull(shape.Name, shape.Schedule);
				}
			});
			AssertNotNull(diagram.Shape.ScheduleBizo);

			Factory.Save();

			var shapeCollection = new BMNCNShapeCollection(Factory);
			shapeCollection.CountChanged += (s, e) =>
			{
				foreach (var shep in shapeCollection)
				{
					shep.GetOrCreateSchedule(); // Make schedules init in an evil way.
				}
			};

			var clone = diagram.CloneDiagramAndAllDescendants();

			AssertNotEquals(diagram.Shape.ScheduleBizo, clone.ScheduleBizo);

			var duplicates = new ActiveBusinessObjectCollection<BMNCNSchedule>(Factory).FindDuplicates(s => s.BNC_BNS_Shape, (s1, s2) => true).ToArray();
			AssertEquals(0, duplicates.Length);
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals(ZDateTime.UtcNow, clone.EarliestStartTimeUtc);
		}

		[TestDate(2015, 4, 13)]
		public void TestCloneDiagramAndAllDescendants_PokeSchedule()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var diagram = CreateDiagram(jobHeader);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();

			diagram.IsCriticalPath = true;
			diagram.EarliestStartTimeUtc = ZDateTime.UtcNow;

			var childshape = networkViewModel.CreateNewShape(diagram);
			var schedule = childshape.Shape.GetOrCreateSchedule();
			Factory.Save();

			var shapeCollection = new BMNCNShapeCollection(Factory);
			shapeCollection.CountChanged += (s, e) =>
			{
				foreach (var shep in shapeCollection)
				{
					shep.GetOrCreateSchedule(); // Make schedules init in an evil way.
				}
			};

			var clone = network.DiagramEntity.CloneDiagramAndAllDescendants();

			AssertNotEquals(diagram.ScheduleBizo, clone.ScheduleBizo);

			AssertNoExceptionThrown(() => Factory.Save());

			AssertEquals(ZDateTime.UtcNow, clone.EarliestStartTimeUtc);
		}

		public void TestCloneDiagramAndAllDescendants_ApprovedColumnsExcluded()
		{
			var staff = VisualBoardsTestHelper.CreateStaffInCurrentBranchDept(Factory, "APP", "Appleloosa");
			var jobHeader = CreateJobHeader<OrgHeader>();
			var diagram = CreateDiagram(jobHeader);
			((IApprovable)diagram).Approve(staff.GS_Code);

			AssertNoExceptionThrown(Factory.Save);

			var network = CreateNetwork(diagram);

			var clone = network.DiagramEntity.CloneDiagramAndAllDescendants();

			AssertEquals(staff.GS_Code, diagram.BNS_GS_NKApprovedBy);
			AssertEquals(true, diagram.IsApproved);

			AssertEquals(string.Empty, clone.BNS_GS_NKApprovedBy);
			AssertEquals(false, clone.IsApproved);

			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestCloneShapesWhenNameIsOutdated()
		{
			var shapeName = "original name";
			var workflowName = "new and improved";

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow = CreateWorkflow(jobHeader, workflowName);

			var jobShape = jobHeader.GetDefaultDiagram();

			Factory.Save();

			var network = CreateNetwork(jobShape);
			var shapeRep = network.Shapes.SingleOrDefault(n => n.ProcessHeader.PK == workflow.PK);
			workflow.FH_CompletionStatement = workflowName;
			shapeRep.BNS_Name = shapeName;

			Factory.Save();

			var clone = network.DiagramEntity.CloneDiagramAndAllDescendants();

			AssertNotNull("Diagram should display correct names after cloning.", clone.ChildShapes.SingleOrDefault(n => n.BNS_Name == workflowName));
		}

		public void TestCloneDiagramAndAllDescendants_SetClonedFromPK()
		{
			var staff = VisualBoardsTestHelper.CreateStaffInCurrentBranchDept(Factory, "JEN", "Cosy Ploy");
			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow = CreateWorkflow(jobHeader, "workflow");

			var jobShape = jobHeader.GetDefaultDiagram();
			var annotation = CreateShape(jobShape, name: "annotation", ShapeTypeList.Codes.Annotation);
			var workflowShape = workflow.GetDefaultShape(jobShape);
			var diagramEntity = CreateNetwork(jobShape).DiagramEntity;

			var clone = diagramEntity.CloneDiagramAndAllDescendants();

			AssertEquals(diagramEntity.PK, clone.ClonedFromPK);
			AssertEquals(annotation.PK, clone.ChildShapes.Single(n => n.BNS_Name == "annotation").ClonedFromPK);
			AssertEquals(workflowShape.PK, clone.ChildShapes.Single(n => n.BNS_Name == "workflow").ClonedFromPK);
		}

		public void TestCloneDiagramAndAllDescendants_DefaultWorkflowsBecomeShape()
		{
			var staff = VisualBoardsTestHelper.CreateStaffInCurrentBranchDept(Factory, "JEN", "Cosy Ploy");
			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow = CreateWorkflow(jobHeader, "Cardboard");

			var jobShape = jobHeader.GetDefaultDiagram();
			var annotation = CreateShape(jobShape, name: "PoketMonstor", ShapeTypeList.Codes.Annotation);
			var workflowShape = workflow.GetDefaultShape(jobShape);

			var clone = CreateNetwork(jobShape).DiagramEntity.CloneDiagramAndAllDescendants();

			AssertEquals(ShapeTypeList.Codes.Diagram, clone.BNS_ShapeType);
			AssertEquals(ShapeTypeList.Codes.Annotation, clone.ChildShapes.Single(n => n.BNS_Name == "PoketMonstor").BNS_ShapeType);
			AssertEquals(ShapeTypeList.Codes.Shape, clone.ChildShapes.Single(n => n.BNS_Name == "Cardboard").BNS_ShapeType);

			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestCloneDiagramAndAllDescendants()
		{
			var diagram = CreateDiagram(Factory, name: "diagram");
			var shape_ch1 = CreateShape(diagram, "shape ch1");
			var shape_ch2 = CreateShape(diagram, "shape ch2");
			var shape_ch1_ch1 = CreateShape(shape_ch1, "shape ch1 ch1");
			var shape_ch1_ch2 = CreateShape(shape_ch1, "shape ch1 ch2");

			shape_ch1.MakeVisiblePrerequisiteOf(shape_ch2, diagram);
			shape_ch1_ch1.MakeVisiblePrerequisiteOf(shape_ch1_ch2, diagram);

			AssertDependencyLink(shape_ch1, shape_ch2);
			AssertDependencyLink(shape_ch1_ch1, shape_ch1_ch2);

			var clonedShape = CreateNetwork(diagram).DiagramEntity.CloneDiagramAndAllDescendants();
			AssertEquals(diagram.BNS_Name, clonedShape.BNS_Name);
			AssertCollectionContains(shape_ch1.BNS_Name, clonedShape.ChildShapes.Select(s => s.BNS_Name));
			AssertCollectionContains(shape_ch2.BNS_Name, clonedShape.ChildShapes.Select(s => s.BNS_Name));

			var clonedShape_ch1 = clonedShape.ChildShapes.First(s => s.BNS_Name == shape_ch1.BNS_Name);
			var clonedShape_ch2 = clonedShape.ChildShapes.First(s => s.BNS_Name == shape_ch2.BNS_Name);
			AssertDependencyLink(clonedShape_ch1, clonedShape_ch2);

			AssertCollectionContains(shape_ch1_ch1.BNS_Name, clonedShape_ch1.ChildShapes.Select(s => s.BNS_Name));
			AssertCollectionContains(shape_ch1_ch2.BNS_Name, clonedShape_ch1.ChildShapes.Select(s => s.BNS_Name));

			var clonedShape_ch1_ch1 = clonedShape_ch1.ChildShapes.First(s => s.BNS_Name == shape_ch1_ch1.BNS_Name);
			var clonedShape_ch1_ch2 = clonedShape_ch1.ChildShapes.First(s => s.BNS_Name == shape_ch1_ch2.BNS_Name);
			AssertDependencyLink(clonedShape_ch1_ch1, clonedShape_ch1_ch2);
		}

		public void TestCloneDiagramAndAllDescendants_Affinities()
		{
			var diagramShape = CreateDiagram(Factory, name: "shape");
			var network = CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;
			var shape_ch1 = CreateShape(diagram, "shape ch1");
			var shape_ch2 = CreateShape(diagram, "shape ch2");

			var affinity1 = new ShapeAffinity("Red", "Charlie Brown", ZGuid.NewZGuid());
			var affinity2 = new ShapeAffinity("Blue", "Clifford The Big Red Dog", ZGuid.NewZGuid());
			diagramShape.ShapeAffinities.Add(affinity1);
			diagramShape.ShapeAffinities.Add(affinity2);

			AssertEquals(2, diagramShape.ShapeAffinities.Count);
			AssertEquals(0, shape_ch1.Shape.ShapeAffinities.Count);
			AssertEquals(0, shape_ch2.Shape.ShapeAffinities.Count);

			((IDiagramEntity)diagram).CreateAffinityLink(shape_ch1, affinity1);
			((IDiagramEntity)diagram).CreateAffinityLink(shape_ch2, affinity1);
			((IDiagramEntity)diagram).CreateAffinityLink(shape_ch2, affinity2);

			AssertEquals(3, diagram.Shape.ShapeAffinityLinks.Count);

			var clonedShape = diagram.CloneDiagramAndAllDescendants();

			AssertEquals(2, clonedShape.ShapeAffinities.Count);
			AssertEquals(3, clonedShape.ShapeAffinityLinks.Count);

			var affinity1PK = clonedShape.ShapeAffinities.Cast<ShapeAffinity>().Where(a => a.Name == "Charlie Brown").Select(a => a.AffinityPK).Single();
			var affinity2PK = clonedShape.ShapeAffinities.Cast<ShapeAffinity>().Where(a => a.Name == "Clifford The Big Red Dog").Select(a => a.AffinityPK).Single();

			var affinity1Links = clonedShape.ShapeAffinityLinks.Cast<ShapeAffinityLink>().Where(l => l.ShapeAffinityPK == affinity1PK);
			var affinity2Links = clonedShape.ShapeAffinityLinks.Cast<ShapeAffinityLink>().Where(l => l.ShapeAffinityPK == affinity2PK);

			AssertEquals(2, affinity1Links.Count());
			AssertEquals(1, affinity2Links.Count());

			AssertEquals("The cloned affinity links should link to the cloned shapes.", 2, affinity1Links.Count(l => clonedShape.ChildShapes.Select(s => s.PK).Any(s => s == l.ShapePK)));
			AssertEquals(1, affinity2Links.Count(l => clonedShape.ChildShapes.Select(s => s.PK).Any(s => s == l.ShapePK)));
		}

		static void AssertDependencyLink(BMNCNShape prereq, BMNCNShape postreq)
		{
			AssertCollectionContains(string.Format("{0} is not a prerequiste of {1}", prereq.BNS_Name, postreq.BNS_Name), postreq, prereq.Postrequisites(new BMNCNShapeDescendantsStrategy()));
		}

		public void TestCloneDiagramAndAllDescendents_ShouldNotCloneInvalidExistingAttachments()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();

			var diagram = CreateDiagram(jobHeader);
			var shape = CreateShape(diagram);
			var attachment = Factory.New<BMNCNAttachment>();
			attachment.BNA_Type = "DPS";
			attachment.BNA_BNS_ToShape = shape.PK;
			attachment.BNA_BNS_Owner = diagram.PK;

			Factory.Save();

			Db.Connection.ExecuteNonQuery(@$"update dbo.bmncnattachment
			set bna_type = '{AttachmentTypeList.Codes.Dependency}',
			BNA_SystemLastEditTimeUTC = getutcdate(),
			BNA_SystemLastEditUser = '~BP'
			where bna_pk = '{attachment.PK}'");

			var newFactory = new BusinessObjectFactory();
			newFactory.Load<BMNCNAttachment>(attachment.PK);

			var reloadedDiagram = newFactory.Load<BMNCNShape>(diagram.PK);
			var network = CreateNetwork(reloadedDiagram);

			AssertNoExceptionThrown("We should be able to clone a diagram with an invalid attachment without the invalid attachment error being reported, but instead...", () => network.DiagramEntity.CloneDiagramAndAllDescendants());
		}

		[TestDate(2024, 7, 18)]
		public void TestClone_CloneParentProperties()
		{
			var diagram = CreateDiagram(Factory, name: "diagram");
			var shape_ch1 = CreateShape(diagram, "shape ch1");

			var clone = CreateNetwork(diagram).DiagramEntity.CloneDiagramAndAllDescendants();
			var clonedShape_ch1 = clone.ChildShapes.First(s => s.BNS_Name == shape_ch1.BNS_Name);
			AssertNotNull(clonedShape_ch1.BNS_BNS_ParentShape);
			AssertNotNull(clonedShape_ch1.BNS_BNS_RootShape);
		}

		public void TestNonScheduledShapeRemainsNonScheduled_OnClone()
		{
			var diagram = CreateDiagram(Factory, name: "diagram");
			diagram.IsScaled = true;
			diagram.ShouldShowNonScheduledSection = true;
			diagram.ScheduledStartTimeUtc = ZDateTime.Now.AddDays(-10);
			var shape = NetworkTestCase.CreateShape(diagram,"shape1");
			shape.IsNonScheduled = true;

			var clone = CreateNetwork(diagram).DiagramEntity.CloneDiagramAndAllDescendants();
			var clonedShape = clone.ChildShapes.First(s => s.BNS_Name == shape.BNS_Name);
			Assert(clonedShape.IsNonScheduled);
		}

		#endregion

		#region Branch/Department

		public void TestIBranchDepartmentProviderMembers()
		{
			var otherBranch = Factory.New<GlbBranch>();
			var otherDepartment = Factory.New<GlbDepartment>();

			var shape = Factory.New<BMNCNShape>();
			AssertNull("Branch should be null when the shape's schedule is null. SAD!", BMSTestHelper.GetBranch(shape, Factory));
			AssertNull("Department should be null when the shape's schedule is null. SAD!", BMSTestHelper.GetDepartment(shape, Factory));

			var schedule = Factory.New<BMNCNSchedule>();
			schedule.BNC_BNS_Shape = shape.PK;
			schedule.BNC_GB_Branch = ZGuid.Empty;
			schedule.BNC_GE_Department = ZGuid.Empty;

			AssertNull("Branch should be null when the shape's schedule has no branch. SAD!", BMSTestHelper.GetBranch(shape, Factory));
			AssertNull("Department should be null when the shape's schedule has no department. SAD!", BMSTestHelper.GetDepartment(shape, Factory));

			schedule.BNC_GB_Branch = otherBranch.PK;
			schedule.BNC_GE_Department = otherDepartment.PK;

			AssertEquals("The schedule's branch should be used as the shape's branch. SAD!", otherBranch, BMSTestHelper.GetBranch(shape, Factory));
			AssertEquals("The schedule's department should be used as the shape's department. SAD!", otherDepartment, BMSTestHelper.GetDepartment(shape, Factory));
		}

		#endregion
	}

	#endregion

	#region Time Schedules

	[GuiTest]
	[TestDate(2014, 5, 19)]
	class BMNCNShapeTimeSchedulingTest : NetworkTestCase
	{
		public void TestApprovedShape_WithScheduledFinishTime_ShouldPersistSchedulingTimeInfo()
		{
			CreateTestData(scheduledFinishTimeUtc: new ZDateTime(2014, 6, 2)); // Due in two weeks time (10 working days), but the diagram's required duration is only 9 days.

			AssertShapeScheduleTimes(shape1,
				earliestStart: new ZDateTime(2014, 5, 20),
				earliestFinish: new ZDateTime(2014, 5, 23),
				latestStart: new ZDateTime(2014, 5, 20),
				latestFinish: new ZDateTime(2014, 5, 23),
				floatHours: 0,
				isCriticalPath: true);

			AssertShapeScheduleTimes(shape2,
				earliestStart: new ZDateTime(2014, 5, 23),
				earliestFinish: new ZDateTime(2014, 5, 28), // Weekend falls inside the 3 day duration
				latestStart: new ZDateTime(2014, 5, 23),
				latestFinish: new ZDateTime(2014, 5, 28),
				floatHours: 0,
				isCriticalPath: true);

			AssertShapeScheduleTimes(shape3,
				earliestStart: new ZDateTime(2014, 5, 28),
				earliestFinish: new ZDateTime(2014, 6, 2), // Weekend falls inside the 3 day duration
				latestStart: new ZDateTime(2014, 5, 28),
				latestFinish: new ZDateTime(2014, 6, 2),
				floatHours: 0,
				isCriticalPath: true);

			AssertShapeScheduleTimes(shape4,
				earliestStart: new ZDateTime(2014, 5, 20),
				earliestFinish: new ZDateTime(2014, 5, 23),
				latestStart: new ZDateTime(2014, 5, 23),
				latestFinish: new ZDateTime(2014, 5, 28),
				floatHours: 3 * BMConstants.WorkingHoursPerDay,
				isCriticalPath: false);
		}

		public void TestApprovedShape_WithScheduledStartTimeInTheFuture_ShouldCountFromFutureDate()
		{
			CreateTestData(scheduledStartTimeUtc: new ZDateTime(2014, 6, 2)); // Due to start in two weeks time

			AssertShapeScheduleTimes(shape1,
				earliestStart: new ZDateTime(2014, 6, 2),
				earliestFinish: new ZDateTime(2014, 6, 5),
				latestStart: new ZDateTime(2014, 6, 2),
				latestFinish: new ZDateTime(2014, 6, 5),
				floatHours: 0,
				isCriticalPath: true);

			AssertShapeScheduleTimes(shape2,
				earliestStart: new ZDateTime(2014, 6, 5),
				earliestFinish: new ZDateTime(2014, 6, 10), // Weekend falls inside the 3 day duration
				latestStart: new ZDateTime(2014, 6, 5),
				latestFinish: new ZDateTime(2014, 6, 10),
				floatHours: 0,
				isCriticalPath: true);

			AssertShapeScheduleTimes(shape3,
				earliestStart: new ZDateTime(2014, 6, 10),
				earliestFinish: new ZDateTime(2014, 6, 13),
				latestStart: new ZDateTime(2014, 6, 10),
				latestFinish: new ZDateTime(2014, 6, 13),
				floatHours: 0,
				isCriticalPath: true);

			AssertShapeScheduleTimes(shape4,
				earliestStart: new ZDateTime(2014, 6, 2),
				earliestFinish: new ZDateTime(2014, 6, 5),
				latestStart: new ZDateTime(2014, 6, 5),
				latestFinish: new ZDateTime(2014, 6, 10),
				floatHours: 3 * BMConstants.WorkingHoursPerDay,
				isCriticalPath: false);
		}

		public void TestApprovedShape_WithScheduledStartTimeInThePast_ShouldCountFromPastDate()
		{
			CreateTestData(scheduledStartTimeUtc: new ZDateTime(2014, 5, 5)); // Due to start two weeks ago

			AssertShapeScheduleTimes(shape1,
				earliestStart: new ZDateTime(2014, 5, 5),
				earliestFinish: new ZDateTime(2014, 5, 8),
				latestStart: new ZDateTime(2014, 5, 5),
				latestFinish: new ZDateTime(2014, 5, 8),
				floatHours: 0,
				isCriticalPath: true);

			AssertShapeScheduleTimes(shape2,
				earliestStart: new ZDateTime(2014, 5, 8),
				earliestFinish: new ZDateTime(2014, 5, 13), // Weekend falls inside the 3 day duration
				latestStart: new ZDateTime(2014, 5, 8),
				latestFinish: new ZDateTime(2014, 5, 13),
				floatHours: 0,
				isCriticalPath: true);

			AssertShapeScheduleTimes(shape3,
				earliestStart: new ZDateTime(2014, 5, 13),
				earliestFinish: new ZDateTime(2014, 5, 16),
				latestStart: new ZDateTime(2014, 5, 13),
				latestFinish: new ZDateTime(2014, 5, 16),
				floatHours: 0,
				isCriticalPath: true);

			AssertShapeScheduleTimes(shape4,
				earliestStart: new ZDateTime(2014, 5, 5),
				earliestFinish: new ZDateTime(2014, 5, 8),
				latestStart: new ZDateTime(2014, 5, 8),
				latestFinish: new ZDateTime(2014, 5, 13),
				floatHours: 3 * BMConstants.WorkingHoursPerDay,
				isCriticalPath: false);
		}

		public void TestApprovedShape_WithScheduledStartTimeInTheFuture_AndFinishTime_ShouldCountDownFromFinishDateAndPileUpAgainstStartDate()
		{
			CreateTestData(scheduledStartTimeUtc: new ZDateTime(2014, 6, 2), scheduledFinishTimeUtc: new ZDateTime(2014, 6, 10)); // Due to start in two weeks, but is due in less time than the schedule allows for

			AssertShapeScheduleTimes(shape1,
				earliestStart: new ZDateTime(2014, 6, 2),
				earliestFinish: new ZDateTime(2014, 6, 5),
				latestStart: new ZDateTime(2014, 6, 2),
				latestFinish: new ZDateTime(2014, 6, 5),
				floatHours: 0,
				isCriticalPath: true,
				scheduledStart: new ZDateTime(2014, 6, 2),
				scheduledFinish: new ZDateTime(2014, 6, 5));

			AssertShapeScheduleTimes(shape2,
				earliestStart: new ZDateTime(2014, 6, 5),
				earliestFinish: new ZDateTime(2014, 6, 10), // Weekend falls inside the 3 day duration
				latestStart: new ZDateTime(2014, 6, 5),
				latestFinish: new ZDateTime(2014, 6, 10),
				floatHours: 0,
				isCriticalPath: true,
				scheduledStart: new ZDateTime(2014, 6, 5),
				scheduledFinish: new ZDateTime(2014, 6, 10));

			AssertShapeScheduleTimes(shape3,
				earliestStart: new ZDateTime(2014, 6, 10),
				earliestFinish: new ZDateTime(2014, 6, 13),
				latestStart: new ZDateTime(2014, 6, 10),
				latestFinish: new ZDateTime(2014, 6, 13),
				floatHours: 0,
				isCriticalPath: true,
				scheduledStart: new ZDateTime(2014, 6, 10),
				scheduledFinish: new ZDateTime(2014, 6, 13));

			AssertShapeScheduleTimes(shape4,
				earliestStart: new ZDateTime(2014, 6, 2),
				earliestFinish: new ZDateTime(2014, 6, 5),
				latestStart: new ZDateTime(2014, 6, 5),
				latestFinish: new ZDateTime(2014, 6, 10),
				floatHours: 3 * BMConstants.WorkingHoursPerDay,
				isCriticalPath: false,
				scheduledStart: new ZDateTime(2014, 6, 5), // Uses latest start/finish for the schedule since we pushed late
				scheduledFinish: new ZDateTime(2014, 6, 10));

			networkViewModel.ToggleApproval();
			networkViewModel.PushAsEarlyAsPossible();
			networkViewModel.ToggleApproval();

			shape4 = network.Shapes.Single(s => s.PK == shape4.PK);

			AssertShapeScheduleTimes(shape4,
				earliestStart: new ZDateTime(2014, 6, 2),
				earliestFinish: new ZDateTime(2014, 6, 5),
				latestStart: new ZDateTime(2014, 6, 5),
				latestFinish: new ZDateTime(2014, 6, 10),
				floatHours: 3 * BMConstants.WorkingHoursPerDay,
				isCriticalPath: false,
				scheduledStart: new ZDateTime(2014, 6, 2), // Now uses earliest start/finish for the schedule since we pushed early
				scheduledFinish: new ZDateTime(2014, 6, 5));
		}

		public void TestUnScaledShapeAccessingSchedule()
		{
			var diagram = Factory.NewWithValidTestData<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = CreateNetwork(diagram);

			var shape = networkViewModel.CreateNewShape(diagram).Shape;
			shape.BNS_RelatedEntityID = ZGuid.Empty;

			AssertScheduleInfo(shape, 0m, 0m, 0m, 0m, 0, 0m, false);
		}

		public void TestShapeIsPositionedDependsOnLeftAndTop()
		{
			var diagramShape = CreateDiagram(Factory, name: "shape");
			var network = CreateNetwork(diagramShape);
			var manuallyPlacedShape = NetworkTestCase.CreateShape(diagramShape, "manual");

			manuallyPlacedShape.Name = "manual";
			manuallyPlacedShape.Width = 200d;
			manuallyPlacedShape.Height = 100d;

			AssertEquals(false, manuallyPlacedShape.IsPositioned);

			manuallyPlacedShape.Left = 111;
			manuallyPlacedShape.Top = 111;
			AssertEquals(true, manuallyPlacedShape.IsPositioned);
			manuallyPlacedShape.IsPositioned = false;
			AssertEquals(true, manuallyPlacedShape.IsPositioned);

			manuallyPlacedShape.Left = 0;
			manuallyPlacedShape.Top = 0;
			AssertEquals(true, manuallyPlacedShape.IsPositioned);
			manuallyPlacedShape.IsPositioned = false;
			AssertEquals(false, manuallyPlacedShape.IsPositioned);

			manuallyPlacedShape.IsPositioned = true;
			AssertEquals(true, manuallyPlacedShape.IsPositioned);
			AssertEquals(0m, manuallyPlacedShape.Left);
			AssertEquals(0m, manuallyPlacedShape.Top);
		}

		#region Implementation

		void CreateTestData(ZDateTime? scheduledStartTimeUtc = null, ZDateTime? scheduledFinishTimeUtc = null)
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			Factory.Save();

			var system = VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
			var diagram = CreateDiagram(CreateJobHeader<OrgHeader>(false));
			var creationNetworkViewModel = CreateNetworkViewModel(diagram);
			var creationNetwork = creationNetworkViewModel.GetJobNetwork();

			var shape1_cc = creationNetworkViewModel.CreateNewWorkflow(diagram);
			shape1_cc.ProcessHeader.FH_CompletionStatement = "1";
			var shape2_cc = creationNetworkViewModel.CreateNewWorkflow(diagram);
			shape2_cc.ProcessHeader.FH_CompletionStatement = "2";
			var shape3_cc = creationNetworkViewModel.CreateNewWorkflow(diagram);
			shape3_cc.ProcessHeader.FH_CompletionStatement = "3";
			var shape4_nonCC = creationNetworkViewModel.CreateNewWorkflow(diagram);
			shape4_nonCC.ProcessHeader.FH_CompletionStatement = "4";

			shape1_cc.Name = "shape1";
			shape2_cc.Name = "shape2";
			shape3_cc.Name = "shape3";
			shape4_nonCC.Name = "shape4";

			shape1_cc.MakeVisiblePrerequisiteOf(shape2_cc);
			shape2_cc.MakeVisiblePrerequisiteOf(shape3_cc);
			shape4_nonCC.MakeVisiblePrerequisiteOf(shape3_cc);

			creationNetwork.SwitchToScaled();

			if (scheduledStartTimeUtc != null)
			{
				creationNetwork.DiagramShape.ScheduledStartTimeUtc = scheduledStartTimeUtc.Value;
			}
			if (scheduledFinishTimeUtc != null)
			{
				creationNetwork.DiagramShape.ScheduledFinishTimeUtc = scheduledFinishTimeUtc.Value;
			}

			using (new DisposableList(creationNetwork.Entities.Select(e => new DisposableAction(() => e.SuspendCalculation(), () => e.ResumeCalculation()))))
			{
				creationNetwork.EditEntity(diagram);
				creationNetworkViewModel.PushAsLateAsPossible();
			}

			creationNetworkViewModel.ToggleApproval(ensureIsNowApproved: true);

			AssertEquals(3m * BMConstants.WorkingHoursPerDay, shape1_cc.Shape.ExplicitDurationHours);
			AssertEquals(3m * BMConstants.WorkingHoursPerDay, shape2_cc.Shape.ExplicitDurationHours);
			AssertEquals(3m * BMConstants.WorkingHoursPerDay, shape3_cc.Shape.ExplicitDurationHours);
			AssertEquals(3m * BMConstants.WorkingHoursPerDay, shape4_nonCC.Shape.ExplicitDurationHours);

			Factory.Save();

			var loadedDiagram = new BusinessObjectFactory().Load<BMNCNShape>(diagram.PK);

			networkViewModel = CreateNetworkViewModel(loadedDiagram);
			network = networkViewModel.GetJobNetwork();

			shape1 = loadedDiagram.ChildShapes.Single(s => s.BNS_RelatedEntityID == shape1_cc.Shape.BNS_RelatedEntityID);
			shape2 = loadedDiagram.ChildShapes.Single(s => s.BNS_RelatedEntityID == shape2_cc.Shape.BNS_RelatedEntityID);
			shape3 = loadedDiagram.ChildShapes.Single(s => s.BNS_RelatedEntityID == shape3_cc.Shape.BNS_RelatedEntityID);
			shape4 = loadedDiagram.ChildShapes.Single(s => s.BNS_RelatedEntityID == shape4_nonCC.Shape.BNS_RelatedEntityID);

			network.Refresh(RefreshType.RefreshButton);
		}

		BMNCNShape shape1, shape2, shape3, shape4;
		NetworkViewModel networkViewModel;
		IJobNetwork network;

		#endregion
	}

	#endregion
}
