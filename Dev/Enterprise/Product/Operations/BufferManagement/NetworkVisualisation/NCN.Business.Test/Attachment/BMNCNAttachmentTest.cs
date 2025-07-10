using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(BMNCNAttachment))]
	class BMNCNAttachmentTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLoadedAttachment_ShouldNotHaveAStackTrace_ShouldNotDieHorribly()
		{
			var arrow = Factory.NewWithValidTestData<BMNCNAttachment>();
			arrow.BNA_BNS_FromShape = ZGuid.Empty;
			Factory.Save();
			ErrorReporter.Clear();

			var newFac = new BusinessObjectFactory();
			var loadedArrow = newFac.Load<BMNCNAttachment>(arrow.PK);
			loadedArrow.BNA_GS_NKApprovedBy = "BUT";
			newFac.Save();
			Assert(ErrorReporter.LastMessageReported != null); 
			Assert("We should not report an error when loading an attachment with reportable property values from the db, and yet...",
				string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
		}

		public void TestAttachmentFromShape_EmptyValuesShouldCauseReports()
		{
			var arrow = Factory.NewWithValidTestData<BMNCNAttachment>();
			arrow.BNA_BNS_FromShape = ZGuid.Empty;

			Factory.Save();
			AssertContains("We should have had an error reported due to the truly imbecilic manner in which we made a null FromShape value, and yet...", "A BMNCNAttachment was created with an empty BNA_BNS_FromShape:", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestIsBuffered()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow3");

			var diagram = NetworkTestCase.CreateDiagram(jobHeader, isScaled: true);
			var shape1 = NetworkTestCase.CreateShape(workflow1, diagram);
			var shape2 = NetworkTestCase.CreateShape(workflow2, diagram);
			var shape3 = NetworkTestCase.CreateShape(workflow3, diagram);

			var arrow1_2 = shape1.MakeVisiblePrerequisiteOf(shape2, diagram);
			var arrow2_3 = shape2.MakeVisiblePrerequisiteOf(shape3, diagram);

			AssertEquals(false, arrow1_2.IsBuffered);
			AssertEquals(false, arrow2_3.IsBuffered);

			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, shape1))
			{
				new AddBufferAction(networkViewModel).GetChildActions().Cast<AddBufferAction.AddBufferForLinkAction>().Single().Execute();
			}

			AssertEquals(true, arrow1_2.IsBuffered);
			AssertEquals(false, arrow2_3.IsBuffered);

			var buffer = network.Shapes.OfType<BMNCNBufferShape>().Single();
			AssertEquals(2, buffer.DependencyAttachments.Count);

			AssertEquals(false, buffer.DependencyAttachments[0].IsBuffered);
			AssertEquals(false, buffer.DependencyAttachments[1].IsBuffered);
		}

		public void TestApproved()
		{
			var attachment = Factory.New<BMNCNAttachment>();
			AssertEquals(false, attachment.IsApproved);

			attachment.BNA_GS_NKApprovedBy = GlbStaff.CurrentUser.GS_Code;
			AssertEquals(true, attachment.IsApproved);
		}

		public void TestAppearance()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "diagram");
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var shape1 = networkViewModel.CreateNewShape(diagram);
			var shape2 = networkViewModel.CreateNewShape(diagram);

			shape1.Name = "shape1";
			shape2.Name = "shape2";

			var arrow = (NetworkAttachment)network.CreateRelationship(shape1, shape2);
			AssertEquals(ArrowAppearance.Normal, arrow.Appearance);
			AssertEquals(BMConstants.NecessityDependencyArrowColorName, arrow.BackColor);

			network.DiagramEntity.Approve(GlbStaff.CurrentUser.GS_Code);
			AssertEquals(ArrowAppearance.Dashed, arrow.Appearance);
			AssertEquals(BMConstants.NecessityDependencyArrowColorName, arrow.BackColor);
			AssertEquals("shape1 -> shape2 (this arrow has not been approved)", arrow.DisplayText);

			arrow.Attachment.Approve(GlbStaff.CurrentUser.GS_Code);
			arrow.Attachment.BNA_IsDecouple = true;
			AssertEquals(ArrowAppearance.Dotted, arrow.Appearance);
			AssertEquals(BMConstants.DecoupledArrowColorName, arrow.BackColor);
			AssertEquals("shape1 -> shape2 (this arrow has been decoupled)", arrow.DisplayText);
		}

		public void TestRelationshipAppearance_NoProcessHeaderLink()
		{
			VisualBoardsTestCase.EnableBMSInRegistry();
			var system = VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var diagram = NetworkTestCase.CreateDiagram(jobHeader, name: "diagram");
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var shape1 = networkViewModel.CreateNewShape(diagram);
			var shape2 = networkViewModel.CreateNewShape(diagram);

			shape1.Name = "shape1";
			shape2.Name = "shape2";

			var workflowShape1 = networkViewModel.CreateNewWorkflow(diagram);
			var workflowShape2 = networkViewModel.CreateNewWorkflow(diagram);

			workflowShape1.Name = "shlep1";
			workflowShape2.Name = "shlep2";

			var arrow1 = (NetworkAttachment)network.CreateRelationship(shape1, shape2);
			AssertEquals(ArrowAppearance.Normal, arrow1.Appearance);
			AssertEquals(BMConstants.NecessityDependencyArrowColorName, arrow1.BackColor);
			AssertEquals("shape1 -> shape2", arrow1.DisplayText);

			arrow1.Attachment.BNA_FP_ProcessHeaderLink = ZGuid.Empty;
			AssertEquals(ArrowAppearance.Normal, arrow1.Appearance);
			AssertEquals(BMConstants.NecessityDependencyArrowColorName, arrow1.BackColor);
			AssertEquals("shape1 -> shape2", arrow1.DisplayText);

			var arrow2 = (NetworkAttachment)network.CreateRelationship(workflowShape1, workflowShape2);
			AssertEquals(ArrowAppearance.Normal, arrow2.Appearance);
			AssertEquals(BMConstants.NecessityDependencyArrowColorName, arrow2.BackColor);
			AssertEquals("shlep1 -> shlep2", arrow2.DisplayText);

			arrow2.Attachment.BNA_FP_ProcessHeaderLink = ZGuid.Empty;
			AssertEquals(BMConstants.DependencyArrowWithoutLinkColorName, arrow2.BackColor);
			AssertEquals("shlep1 -> shlep2 (this arrow does not represent a true dependency)", arrow2.DisplayText);
		}

		public void TestHumanReadableName()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			var diagram = NetworkTestCase.CreateDiagram(jobHeader);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var shape1 = NetworkTestCase.CreateShape(workflow1, diagram);
			var shape2 = NetworkTestCase.CreateShape(workflow2, diagram);
			var shape3 = NetworkTestCase.CreateShape(workflow3, diagram);

			var dependencyAttachment = shape1.MakeVisiblePrerequisiteOf(shape2, diagram);

			new CreateResourceDependencyAction(networkViewModel, DependencyDirection.PreRequisite).ExecuteForShapes(shape2, shape3);

			var resourceDependency = shape3.AllAttachments.Single(a => a.IsResourceDependency);

			AssertEquals("Dependency Arrow", dependencyAttachment.HumanReadableName);
			AssertEquals("Resource Dependency", resourceDependency.HumanReadableName);

			dependencyAttachment.BNA_FP_ProcessHeaderLink = ZGuid.Empty;
			AssertEquals("Shape Connection", dependencyAttachment.HumanReadableName);
		}

		#region Delete

		public void TestDelete_ShouldAlsoDeleteBuffer()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);

			var dependency = (NetworkAttachment)network.CreateRelationship(childShape1, childShape2);
			AssertNull(dependency.GetBuffer());
			dependency.CreateBuffer();

			var buffer = dependency.GetBuffer();
			AssertNotNull(buffer);
			AssertEquals("New Buffer", buffer.Name);
			AssertEquals(ShapeTypeList.Codes.Buffer, buffer.Shape.BNS_ShapeType);

			Factory.Save();

			var loadedDependency = new BusinessObjectFactory().Load<BMNCNAttachment>(dependency.PK);
			var loadedBuffer = loadedDependency.GetBuffer();

			var attachmentQuery = new ZQuery(BMNCNAttachmentSchema.BNA_BNS_ToShape, loadedBuffer.PK);
			attachmentQuery.AddToFilter(new ZQuery(BMNCNAttachmentSchema.BNA_BNS_FromShape, loadedBuffer.PK), JoinCondition.Or);
			var bufferAttachments = loadedDependency.Factory.Load<BMNCNAttachment>(attachmentQuery);
			AssertEquals("Should be two attachments linking the buffer to from/to shapes.", 2, bufferAttachments.Length);
			AssertCollectionNotContains(loadedDependency, bufferAttachments);

			loadedDependency.Delete();

			Assert("Deleting dependency should delete buffer too", loadedBuffer.IsDeleted);

			foreach (var attachment in bufferAttachments)
			{
				Assert("Deleting dependency should delete buffer attachments too", attachment.IsDeleted);
			}
		}

		public void TestDeleteDependency_WhenApprovedArrowExists()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var diagram = NetworkTestCase.CreateDiagram(jobHeader);
			var shape1 = NetworkTestCase.CreateShape(workflow1, diagram);
			var shape2 = NetworkTestCase.CreateShape(workflow2, diagram);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var arrow = shape1.MakeVisiblePrerequisiteOf(shape2, diagram);
			var dependency = arrow.ProcessHeaderLink;

			network.SwitchToScaled();
			networkViewModel.ToggleApproval();

			var ex = AssertExceptionThrown<CannotDeleteException>(() => arrow.Delete());
			AssertEquals("This arrow has been approved and cannot be deleted. It should be decoupled instead.", ex.Message);

			networkViewModel.ToggleApproval();
			AssertNoExceptionThrown(() => dependency.Delete());
		}

		#endregion

		public void TestCreateBuffer()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);

			var dependency = (NetworkAttachment)network.CreateRelationship(childShape1, childShape2);
			AssertNull(dependency.GetBuffer());
			dependency.CreateBuffer();

			var buffer = dependency.GetBuffer();
			AssertNotNull(buffer);
			AssertEquals("New Buffer", buffer.Name);
			AssertEquals(ShapeTypeList.Codes.Buffer, buffer.Shape.BNS_ShapeType);

			var bufferEntity = (INetworkEntity)buffer;
			AssertEquals(2, bufferEntity.Links.Count());
			AssertEquals(1, bufferEntity.PreRequisiteLinks.Count());
			AssertEquals(1, bufferEntity.PostRequisiteLinks.Count());
			AssertEquals(childShape1, bufferEntity.PreRequisiteLinks.ElementAt(0).From);
			AssertEquals(childShape2, bufferEntity.PostRequisiteLinks.ElementAt(0).To);

			AssertEquals(2, childShape1.AsShape().PostrequisiteShapes.Count());

			Factory.Save();

			var loadedDependency = new BusinessObjectFactory().Load<BMNCNAttachment>(dependency.PK);
			AssertEquals(2, loadedDependency.FromShape.PostrequisiteShapes.Count());
			var loadedBuffer = loadedDependency.GetBuffer();
			AssertNotNull(loadedBuffer);
			AssertEquals(buffer.PK, loadedBuffer.PK);
		}

		public void TestCreateDuplicateBuffer()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);

			var dependency = (NetworkAttachment)network.CreateRelationship(childShape1, childShape2);
			AssertNotNull(dependency.CreateBuffer());

			AssertExceptionThrown<InvalidOperationException>(() => dependency.CreateBuffer());
		}

		public void TestCreateBuffer_ShouldPositionToRightOfSourceShape()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();

			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);

			((INetworkEntity)childShape1).X = 100;
			((INetworkEntity)childShape1).Width = 300;
			((INetworkEntity)childShape1).Y = 200;
			((INetworkEntity)childShape1).Height = 400;

			var dependency = (NetworkAttachment)network.CreateRelationship(childShape1, childShape2);
			AssertNull(dependency.GetBuffer());
			var buffer = dependency.CreateBuffer();

			AssertEquals("Should locate buffer against right edge of source shape", 400d, ((INetworkEntity)buffer).X);
			AssertEquals("Should align buffer centrally along vertical axis with source shape", 365d, ((INetworkEntity)buffer).Y);
			AssertEquals("Should default to 3x resolution increment (for now)", 300d, ((INetworkEntity)buffer).Width);
			AssertEquals("Default height", 70d, ((INetworkEntity)buffer).Height);
		}

		public void TestBackColor()
		{
			var attachment = Factory.New<BMNCNAttachment>();
			AssertEquals(BMConstants.NecessityDependencyArrowColorName, attachment.BackColor);

			attachment.BNA_IsDecouple = true;
			AssertEquals(BMConstants.DecoupledArrowColorName, attachment.BackColor);

			attachment.BNA_Type = AttachmentTypeList.Codes.ResourceDependency;
			AssertEquals(BMConstants.ResourceDependencyArrowColorName, attachment.BackColor);
		}

		public void TestDisplayText()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			workflow1.FH_CompletionStatement = "workflow1";
			workflow2.FH_CompletionStatement = "workflow2";

			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			var defaultDiagram = jobHeader.GetDefaultDiagram();
			var network = NetworkTestCase.CreateNetwork(defaultDiagram);
			var attachment = NetworkTestCase.CreateDependencyAttachment(defaultDiagram, link, workflow1.GetDefaultShape(defaultDiagram), workflow2.GetDefaultShape(defaultDiagram));
			AssertEquals("workflow1 -> workflow2", attachment.AsEntity(network).DisplayText);
		}

		public void TestDisplayText_ForResourceDependencies()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			diagram.SwitchToScaled();

			var shape1 = networkViewModel.CreateNewShape(diagram);
			var shape2 = networkViewModel.CreateNewShape(diagram);

			shape1.Name = "shape1";
			shape2.Name = "shape2";

			var dependencyAttachment = network.CreateRelationship(shape1, shape2).AsAttachment();
			AssertEquals("shape1 -> shape2", dependencyAttachment.AsEntity(network).DisplayText);
			dependencyAttachment.Delete();

			networkViewModel.SelectEntities(new[] { shape1, shape2 });
			var action = new CreateResourceDependencyAction(networkViewModel, DependencyDirection.PreRequisite);
			action.ExecuteAfterActivatingEntity_ForTest(shape1);
			var resourceDependencyAttachment = shape1.DependencyAttachments.Single(a => a.IsResourceDependency && a.Attachment.BNA_BNS_ToShape == shape2.PK);
			AssertEquals("Resource Dependency:\r\nshape1 -> shape2", resourceDependencyAttachment.DisplayText);
		}

		public void TestResourceDependenciesAndNormalDependencies_ShouldNotReportDeveloperException()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			diagram.SwitchToScaled();

			var shape1 = networkViewModel.CreateNewShape(diagram);
			var shape2 = networkViewModel.CreateNewShape(diagram);
			AssertEquals("Precondition", 2, networkViewModel.Nodes.Count());

			var dependencyAttachment = network.CreateRelationship(shape1, shape2).AsAttachment();

			networkViewModel.SelectEntities(new[] { shape1, shape2 });
			AssertEquals("Precondition", 2, networkViewModel.SelectedEntities.Count());
			var action = new CreateResourceDependencyAction(networkViewModel, DependencyDirection.PreRequisite);
			NetworkActionTestHelper.MakeActionsRefreshOnSelectionChanged(networkViewModel, action);
			action.ExecuteAfterActivatingEntity_ForTest(shape1);

			AssertEquals("Could not do action because of existing DEP link.", false, shape1.DependencyAttachments.Any(a => a.IsResourceDependency && a.To.PK == shape2.PK));

			dependencyAttachment.Delete();

			action.ExecuteAfterActivatingEntity_ForTest(shape1);
			var resourceDependencyAttachment = shape1.DependencyAttachments.Single(a => a.IsResourceDependency && a.To.PK == shape2.PK);

			Factory.Save();

			var loadedDiagram = new BusinessObjectFactory().Load<BMNCNShape>(diagram.PK);
			AssertEquals(2, loadedDiagram.ChildShapes.Count);
			AssertCollectionContains(loadedDiagram.ChildShapes, s => s.PK == shape1.PK);
			AssertCollectionContains(loadedDiagram.ChildShapes, s => s.PK == shape2.PK);
		}

		public void TestRefreshAppearancePropertiesTriggered()
		{
			var attachment = Factory.New<BMNCNAttachment>();
			var timesAppearanceChangeTriggered = 0;
			var timesBackColorChangeTriggered = 0;
			var timesDisplayTextChangeTriggered = 0;

			attachment.PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
			{
				switch (e.PropertyName)
				{
					case nameof(BMNCNAttachment.Appearance):
						timesAppearanceChangeTriggered++;
						break;
					case nameof(BMNCNAttachment.BackColor):
						timesBackColorChangeTriggered++;
						break;
					case nameof(BMNCNAttachment.DisplayText):
						timesDisplayTextChangeTriggered++;
						break;
					default:
						break;
				}
			};

			attachment.BNA_FP_ProcessHeaderLink = ZGuid.NewZGuid();
			AssertEquals(1, timesAppearanceChangeTriggered);
			AssertEquals(1, timesBackColorChangeTriggered);
			AssertEquals(1, timesDisplayTextChangeTriggered);

			attachment.BNA_GS_NKApprovedBy = "ABC";
			AssertEquals(2, timesAppearanceChangeTriggered);
			AssertEquals(2, timesBackColorChangeTriggered);
			AssertEquals(2, timesDisplayTextChangeTriggered);

			attachment.BNA_IsDecouple = true;
			AssertEquals(3, timesAppearanceChangeTriggered);
			AssertEquals(3, timesBackColorChangeTriggered);
			AssertEquals(3, timesDisplayTextChangeTriggered);

			attachment.BNA_Type = "XYZ";
			AssertEquals(4, timesAppearanceChangeTriggered);
			AssertEquals(4, timesBackColorChangeTriggered);
			AssertEquals(4, timesDisplayTextChangeTriggered);

			//Doesn't trigger a property change for other props
			AssertEquals(false, attachment.BNA_IsHidden);
			attachment.BNA_IsHidden = true;
			AssertEquals(4, timesAppearanceChangeTriggered);
			AssertEquals(4, timesBackColorChangeTriggered);
			AssertEquals(4, timesDisplayTextChangeTriggered);
		}

		// this region compensates for the nullable BNA_BNS_FromShape column, which is default set to ZGuid.Empty and interferes with error reporting.
		// feel free to reassign BNA_BNS_FromShape where required, as long as the field is not ZGuid.Empty :)
		#region BusinessObject Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			var bizo = base.GetNewBusinessObject();
			((BMNCNAttachment)bizo).BNA_BNS_FromShape = Factory.New<BMNCNShape>().PK;

			return bizo;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var bizo = base.GetNewBusinessObjectForDeleteTest(factory);
			((BMNCNAttachment)bizo).BNA_BNS_FromShape = factory.New<BMNCNShape>().PK;

			return bizo;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var bizo = base.GetBusinessObjectForFetchForLoad();
			((BMNCNAttachment)bizo).BNA_BNS_FromShape = Factory.New<BMNCNShape>().PK;

			return bizo;
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion
	}
}
