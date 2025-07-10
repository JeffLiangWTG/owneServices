using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Business;
#if NETFRAMEWORK
using Enterprise.ZArchitecture.Core;
#endif
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class JobNetworkTest : NetworkTestCase
	{
		#region Entity

		public void TestEntities_WhenLoadRelatedEntities_ThenProcessHeaderLinkQueryShouldNotHaveORPredicate()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var diagram = CreateDiagram(jobHeader);
			var firstWorkflow = CreateWorkflow(jobHeader, "Firkflow");
			var firstShape = CreateShape(firstWorkflow, diagram);

			for (var i = 0; i < 10; i++)
			{
				var workflow = CreateWorkflow(jobHeader, "Workflow " + i);
				var shape = CreateShape(workflow, diagram);
				firstShape.MakeVisiblePrerequisiteOf(shape, diagram);
			}

			Factory.Save();

			using (Db.Connection.TrackExecutedCommands(includeStackTrace: true))
			{
				var loadedDiagram = new BusinessObjectFactory { RefreshEnabled = false }.Load<BMNCNRootDiagramShape>(diagram.PK);
				CreateNetwork(loadedDiagram); // hits occur here.

				var relevantQueries = Db.Connection.ExecutedCommands.Where(x => x.Contains($"FROM {ProcessHeaderLinkSchema.Constants.SqlSchemaName}.{ProcessHeaderLinkSchema.Constants.TableName}")).ToArray();
				AssertEquals("should have ProcessHeaderLink query", 2, relevantQueries.Length);

				var badCommands = relevantQueries.Where(x => x.Contains(" or FP_FH_Header", StringComparison.OrdinalIgnoreCase));
				AssertContainsExactElementsInAnyOrder("We must use table valued parameters instead of long lists of ORs. SAD!", Array.Empty<string>(), badCommands);

				var queriesWithTvp = relevantQueries.Where(x => x.Contains("SELECT Value FROM @CWO", StringComparison.OrdinalIgnoreCase));
				AssertEquals("The queries should both use a TVP rather than an IN clause. SAD!", 2, queriesWithTvp.Count());
			}
		}

		public void TestEntities_Create_SetsOwner()
		{
			var diagram = CreateDiagram(CreateJobHeader<OrgHeader>());
			var network = CreateNetwork(diagram);
			AssertEquals("Poking the lazy collection.", 0, network.Entities.Count);

			var shape1 = Factory.New<BMNCNShape>();
			shape1.MakeChildOf(diagram);

			AssertEquals(1, network.Entities.Count);
			AssertEquals(network.DiagramEntity, shape1.AsEntity(network).Owner);
		}

		public void TestClonedWorkflowsAreCreatedAsEntities()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var defaultDiagram = jobHeader.GetDefaultDiagram();
			AssertEquals(2, defaultDiagram.ChildShapes.Count);

			var network = CreateNetwork(defaultDiagram);
			AssertEquals(2, network.Entities.Count);

			var workflow1Shape = defaultDiagram.ChildShapes[0];
			var workflow2Shape = defaultDiagram.ChildShapes[1];

			jobHeader.ProcessHeaders.AddNew();

			defaultDiagram.UpdateDefaultNetwork();
			network.FullRefresh();
			AssertEquals(3, network.Entities.Count);

			var workflow3 = workflow1.CloneWorkflow();
			var workflow4 = workflow2.CloneWorkflow();
			var s = workflow3.GetDefaultShape(defaultDiagram);
			defaultDiagram.ChildShapes.RefreshBinding();

			network.FullRefresh();
			AssertEquals(5, defaultDiagram.ChildShapes.Count);
			AssertEquals(5, network.Entities.Count);
		}

		public void TestNetworkNameIsNotNull()
		{
			var diagram = CreateDiagram(Factory);
			var jobNetwork = CreateNetwork(diagram);
			AssertEquals(BMNCNShape.DefaultDiagramName, jobNetwork.Name);
		}

		#endregion

		#region Owner

		public void TestOwner_ShouldBeSetOnInitialLoad()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var defaultDiagram = jobHeader.GetDefaultDiagram();
			AssertEquals(2, defaultDiagram.ChildShapes.Count);

			var network = CreateNetwork(defaultDiagram);
			AssertEquals(2, network.Entities.Count);

			var workflow1Shape = defaultDiagram.ChildShapes[0];
			var workflow2Shape = defaultDiagram.ChildShapes[1];

			AssertEquals(defaultDiagram, workflow1Shape.AsEntity(network).Owner.Shape);
			AssertEquals(defaultDiagram, workflow2Shape.AsEntity(network).Owner.Shape);
		}

		public void TestOwner_NewEntity()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var diagram = jobHeader.GetDefaultDiagram();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			AssertEquals(1, network.Entities.Count);

			var newShape = networkViewModel.CreateNewWorkflow(diagram);
			var newEntity = (INetworkEntity)newShape;

			AssertNotNull(newShape.Owner);

			newEntity.X = 50;
			newEntity.Y = 40;

			AssertEquals(50.0, newEntity.X);
			AssertEquals(40.0, newEntity.Y);
		}

		public void TestOwner_SubDiagram()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var workflow1_1 = jobHeader1.ProcessHeaders[0];
			var workflow1_2 = jobHeader1.ProcessHeaders.AddNew();

			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var workflow2_1 = jobHeader2.ProcessHeaders[0];
			var workflow2_2 = jobHeader2.ProcessHeaders.AddNew();

			var diagram1 = NetworkTestCase.CreateDiagram(jobHeader1);
			NetworkTestCase.CreateShape(workflow1_1, diagram1);
			NetworkTestCase.CreateShape(workflow1_2, diagram1);

			var diagram2 = NetworkTestCase.CreateDiagram(jobHeader2);
			NetworkTestCase.CreateShape(workflow2_1, diagram2);
			NetworkTestCase.CreateShape(workflow2_2, diagram2);

			Factory.Save();

			JobNetwork network = null;
			BMNCNShape[] importedShapes;
			var controller = NetworkTestCase.CreateMockableController(Mocks);

			controller.Setup(m => m.PickEntity(ModuleIDs.NetworkDiagram, It.IsAny<bool>())).Returns(diagram2);
			using (ObjectFactory.Substitute(controller.Object))
			{
				network = CreateNetwork(diagram1);
				AssertEquals(2, network.Entities.Count);

				importedShapes = network.PickAndImportEntities(diagram1).AsShapes().ToArray();
			}

			AssertEquals(5, network.Entities.Count);
			AssertEquals(3, importedShapes.Length);
			var importedJobShape = importedShapes[0];
			var importedWorkflow1Shape = importedJobShape.ChildShapes.First(s => s.BNS_RelatedEntityID == workflow2_1.PK);
			var importedWorkflow2Shape = importedJobShape.ChildShapes.First(s => s.BNS_RelatedEntityID == workflow2_2.PK);

			AssertEquals(jobHeader2.PK, importedJobShape.BNS_RelatedEntityID);

			AssertCollectionContains(importedJobShape, network.Shapes);
			AssertCollectionContains(importedWorkflow1Shape, network.Shapes);
			AssertCollectionContains(importedWorkflow2Shape, network.Shapes);

			AssertEquals(diagram1.AsEntity(network), importedJobShape.AsEntity(network).Owner);
			AssertEquals(importedJobShape.AsEntity(network), importedWorkflow1Shape.AsEntity(network).Owner);
			AssertEquals(importedJobShape.AsEntity(network), importedWorkflow2Shape.AsEntity(network).Owner);
		}

		public void TestOwner_ShownEntity()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			var diagram = CreateDiagram(jobHeader);
			AssertEquals(0, diagram.ChildShapes.Count);

			var network = CreateNetwork(diagram);
			AssertEquals(0, network.Entities.Count);

			var workflowShape = network.ShowEntity(workflow, diagram).Single().AsShape();

			AssertEquals(diagram, workflowShape.AsEntity(network).Owner.Shape);
		}

		#endregion

		#region Controller Actions

		[ExpectNoExceptions]
		public void TestViewEntity()
		{
			var controller = NetworkTestCase.CreateMockableController(Mocks);
			var diagram = CreateJobAndDiagram(Factory);

			controller.Setup(m => m.OpenLinkedEntity(It.IsAny<BusinessObject>(), It.IsAny<ControllerID>()));
			using (ObjectFactory.Substitute(controller.Object))
			{
				var network = CreateNetwork(diagram);
				network.OpenLinkedEntity(diagram);
			}
		}

		[ExpectNoExceptions]
		public void TestShowNetworkDiagramsModule()
		{
			var controller = NetworkTestCase.CreateMockableController(Mocks);
			var diagram = CreateJobAndDiagram(Factory);

			controller.Setup(m => m.ShowNetworkDiagramsModule(It.IsAny<INetworkEntity>()));
			using (ObjectFactory.Substitute(controller.Object))
			{
				var network = CreateNetwork(diagram);
				network.ShowNetworkDiagramsModule(diagram);
			}
		}

		#endregion

		#region Link Entity

		public void TestLinkEntity_WhenParentShapeIsAlsoLinked_ShouldCreateParentChildLink()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var diagram = CreateDiagram(jobHeader1);
			var subDiagram = CreateShape(diagram);

			var network = CreateNetwork(diagram);

			AssertIsNotParent(jobHeader2, jobHeader1);

			AssertEquals(true, network.LinkEntity(subDiagram, jobHeader2));
			AssertIsParent(jobHeader2, jobHeader1);
		}

		public void TestLinkEntity_WhenGrandparentShapeIsAlsoLinked_ShouldCreateParentChildLink()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var diagram = CreateDiagram(jobHeader1);
			var subDiagram = CreateShape(diagram);
			var subSubDiagram = CreateShape(subDiagram);

			var network = CreateNetwork(diagram);

			AssertIsNotParent(jobHeader2, jobHeader1);

			AssertEquals(true, network.LinkEntity(subSubDiagram, jobHeader2));
			AssertIsParent(jobHeader2, jobHeader1);
		}

		public void TestLinkEntity_WhenMultipleAncestorShapesAreAlsoLinked_ShouldCreateParentChildLinkToMostDirectLinkedAncestor()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var diagram = CreateDiagram(jobHeader1);
			var subDiagram = CreateShape(jobHeader2, diagram);
			var subSubDiagram = CreateShape(subDiagram);
			var subSubSubDiagram = CreateShape(subSubDiagram);

			var network = CreateNetwork(diagram);

			AssertIsNotParent(jobHeader3, jobHeader1);
			AssertIsNotParent(jobHeader3, jobHeader2);

			AssertEquals(true, network.LinkEntity(subSubSubDiagram, jobHeader3));
			AssertIsNotParent(jobHeader3, jobHeader1);
			AssertIsParent(jobHeader3, jobHeader2);
		}

		public void TestLinkEntity_Validation_WorkflowLinkAllowed()
		{
			var controller = NetworkTestCase.CreateMockableController(Mocks);
			var originalJobHeader = CreateJobHeader<OrgHeader>();
			var diagram1 = CreateDiagram(originalJobHeader);
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = CreateWorkflow(jobHeader, "Theres a nasal spray for that");

			controller.Setup(m => m.PickEntity(ModuleIDs.ProcessHeader, It.IsAny<bool>())).Returns(workflow);
			using (ObjectFactory.Substitute(controller.Object))
			{
				var network = CreateNetwork(diagram1);
				var header = network.DiagramEntity.ProcessHeader;

				network.LinkEntity(diagram1, ModuleIDs.ProcessHeader);

				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);

				AssertEquals(workflow, diagram1.ProcessHeader);
				AssertEquals(@"The entity being linked has associated workflow dependencies (pre/post-requisites and/or child entities). Do you want to add all child entities to the diagram that are necessary to display the dependencies in full? If you choose ""No"" the direct pre/post-requisite links (if any) between the displayed entities will be added only.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestLinkEntity_CreatePCHLinks()
		{
			var controller = NetworkTestCase.CreateMockableController(Mocks);
			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var diagram1 = CreateDiagram(jobHeader);
			var shape = CreateShape(diagram1);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];

			controller.Setup(m => m.PickEntity(ModuleIDs.ProcessHeader, It.IsAny<bool>())).Returns(workflow);
			using (ObjectFactory.Substitute(controller.Object))
			{
				var network = CreateNetwork(diagram1);
				var header = network.DiagramEntity.ProcessHeader;

				AssertEquals(0, header.ChildLinks.Count());

				network.LinkEntity(shape, ModuleIDs.ProcessHeader);

				var link = header.ChildLinks.SingleOrDefault();

				AssertNotNull(link);
				AssertEquals(ProcessHeaderLinkTypeList.Codes.ParentChild, link.FP_LinkType);
				AssertEquals(1, header.ChildLinks.Count());
			}
		}

		public void TestLinkEntity_CreateDEPLinks()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow1 = CreateWorkflow(jobHeader, "w1");
			var workflow2 = CreateWorkflow(jobHeader, "w2");

			var diagram1 = CreateDiagram(jobHeader, name: "Homeless");
			var shape1 = CreateShape(diagram1, name: "Phoneless");
			var shape2 = CreateShape(diagram1, name: "Moan less");
			var attachment = shape1.MakeVisiblePrerequisiteOf(shape2, diagram1);

			var network = CreateNetwork(diagram1);

			network.LinkEntity(shape1, workflow1);

			AssertEquals(workflow1, shape1.ProcessHeader);
			AssertEquals(0, workflow1.PostrequisiteWorkflows.Count());

			network.LinkEntity(shape2, workflow2);

			AssertEquals(workflow2, shape2.ProcessHeader);
			AssertEquals(workflow2, workflow1.PostrequisiteWorkflows.Single());

			var link = workflow1.Links.Single();
			AssertEquals(link.PK, attachment.BNA_FP_ProcessHeaderLink);
		}

		public void TestLinkEntity_LinkDEPHeaderFromOtherFactory()
		{
			var controller = NetworkTestCase.CreateMockableController(Mocks);
			var newFactory = Factory.CreateNewFactory();

			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(newFactory, false);
			var workflow1 = CreateWorkflow(jobHeader, "w1");
			var workflow2 = CreateWorkflow(jobHeader, "w2");

			newFactory.Save();

			var diagram1 = CreateDiagram(Factory.Load<ProcessJobHeader>(jobHeader.PK), name: "Journ");
			var shape1 = CreateShape(Factory.Load<ProcessHeader>(workflow1.PK), diagram1, name: "ChippyTheHippy");
			var shape2 = CreateShape(diagram1, name: "WheatbickTheBeatnick");

			controller.Setup(m => m.PickEntity(ModuleIDs.ProcessHeader, It.IsAny<bool>())).Returns(workflow2);
			using (ObjectFactory.Substitute(controller.Object))
			{
				var network = CreateNetwork(diagram1);

				var attachment = network.CreateRelationship(shape1, shape2).AsAttachment();

				AssertEquals(ZGuid.Empty, attachment.BNA_FP_ProcessHeaderLink);

				network.LinkEntity(shape2, ModuleIDs.ProcessHeader);
				AssertNotNull(attachment.ProcessHeaderLink);
			}
		}

		public void TestLinkEntity_LinkDEPWithExstingHeaderLink()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow1 = CreateWorkflow(jobHeader, "w1");
			var workflow2 = CreateWorkflow(jobHeader, "w2");

			Assert(workflow1.MakePrerequisiteOf(workflow2));
			var link = workflow1.Links.Single();

			var diagram1 = CreateDiagram(jobHeader, name: "Journ");
			var shape1 = CreateShape(workflow1, diagram1, name: "ChippyTheHippy");
			var shape2 = CreateShape(diagram1, name: "WheatbickTheBeatnick");

			var network = CreateNetwork(diagram1);

			var attachment = network.CreateRelationship(shape1, shape2).AsAttachment();

			AssertEquals(ZGuid.Empty, attachment.BNA_FP_ProcessHeaderLink);

			network.LinkEntity(shape2, workflow2);
			AssertEquals(link.PK, attachment.BNA_FP_ProcessHeaderLink);
		}

		public void TestLinkEntity_UnlinkDEPLinks()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow1 = CreateWorkflow(jobHeader, "w1");
			var workflow2 = CreateWorkflow(jobHeader, "w2");

			var diagram1 = CreateDiagram(jobHeader, name: "Homeless");
			var shape1 = CreateShape(workflow1, diagram1, name: "Phoneless");
			var shape2 = CreateShape(workflow2, diagram1, name: "Moan less");

			var controller = NetworkTestCase.CreateMockableControllerWithMockableInteractionImplementor(Mocks);

			controller
				.Setup(m => m.UserInteractionImplementor.ShowMultiOptionDialog(
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<UnlinkEntityVariant>(),
					It.IsAny<ButtonStripAction<UnlinkEntityVariant>[]>()))
				.Returns(UnlinkEntityVariant.PersistHeaderLinks);

			using (ObjectFactory.Substitute(controller.Object))
			{
				var network = CreateNetwork(diagram1);

				var attachment = network.CreateRelationship(shape1, shape2).AsAttachment();

				AssertNotNull(attachment);

				var link = workflow1.Links.Single();
				AssertEquals(link.PK, attachment.BNA_FP_ProcessHeaderLink);

				network.UnlinkEntity(shape1);
				AssertEquals("Unlink should definitely clear the existing link", ZGuid.Empty, attachment.BNA_FP_ProcessHeaderLink);

				network.LinkEntity(shape1, workflow1);
				AssertEquals(link.PK, attachment.BNA_FP_ProcessHeaderLink);
			}
		}

		public void TestLinkEntity_OwnChild()
		{
			var controller = NetworkTestCase.CreateMockableController(Mocks);
			var jobHeader = CreateJobHeader<OrgHeader>();
			var diagram1 = CreateDiagram(jobHeader);
			var shape = CreateShape(diagram1);

			var workflow = jobHeader.ProcessHeaders[0];

			controller.Setup(m => m.PickEntity(ModuleIDs.ProcessHeader, false)).Returns(workflow);
			using (ObjectFactory.Substitute(controller.Object))
			{
				var network = CreateNetwork(diagram1);
				var header = network.DiagramEntity.ProcessHeader;

				AssertEquals(0, header.ChildLinks.Count());

				network.LinkEntity(shape, ModuleIDs.ProcessHeader);

				AssertEquals(0, header.ChildLinks.Count());
			}
		}

		public void TestLinkEntity_Parent()
		{
			var controller = NetworkTestCase.CreateMockableController(Mocks);
			var jobHeader1 = CreateJobHeader<OrgHeader>(false);
			var jobHeader2 = CreateJobHeader<OrgHeader>(true);
			var workflow = jobHeader2.ProcessHeaders[0];

			var diagram = CreateDiagram(Factory, name: "Top-level diagram");
			var shapeWithoutWorkflow = CreateShape(diagram, "shapeWithoutWorkflow");
			var shapeWithWorkflow = CreateShape(workflow, diagram, "shapeWithWorkflow");

			controller.Setup(m => m.PickEntity(ModuleIDs.ProcessHeader, false)).Returns(jobHeader1);

			using (ObjectFactory.Substitute(controller.Object))
			{
				var network = CreateNetwork(diagram);

				AssertEquals(false, workflow.IsChildOf(jobHeader1));

				network.LinkEntity(network.DiagramEntity, ModuleIDs.ProcessHeader);

				AssertEquals("Linking the diagram surface to jobHeader1 should make that a parent of the nested shape's linked workflow", true, workflow.IsChildOf(jobHeader1));
			}
		}

		public void TestLinkEntity_ForRootDiagram_WhenNestedChildrenExist_ShouldLinkDirectDescendents()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var jobHeader3 = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);

			var diagram = CreateDiagram(Factory, name: "Top-level diagram");
			var subDiagram = CreateShape(diagram, "subDiagram");
			var subSubDiagram1 = CreateShape(jobHeader2, subDiagram, "subSubDiagram1");
			var subSubDiagram2 = CreateShape(jobHeader3, subDiagram, "subSubDiagram2");

			var network = CreateNetwork(diagram);

			AssertIsNotParent(jobHeader2, jobHeader1);
			AssertIsNotParent(jobHeader3, jobHeader1);

			network.LinkEntity(network.DiagramEntity, jobHeader1);

			AssertIsParent("Linking the diagram entity should find all top-most descendents and create PCH links", jobHeader2, jobHeader1);
			AssertIsParent("Linking the diagram entity should find all top-most descendents and create PCH links", jobHeader3, jobHeader1);
		}

		public void TestLinkEntity_DontDeleteJobWorkflows()
		{
			var controller = NetworkTestCase.CreateMockableController(Mocks);
			var diagram1 = CreateDiagram(CreateJobHeader<OrgHeader>());
			var jobHeader = CreateJobHeader<OrgHeader>();

			controller.Setup(m => m.PickEntity(ModuleIDs.ProcessHeader, false)).Returns(jobHeader);

			using (ObjectFactory.Substitute(controller.Object))
			{
				var network = CreateNetwork(diagram1);
				var header = network.DiagramEntity.ProcessHeader;

				AssertNotNull(header.Parent);
				AssertEquals(false, header.IsDeleted);
				network.LinkEntity(diagram1, ModuleIDs.ProcessHeader);
				AssertEquals(false, header.IsDeleted);
			}
		}

		[ExpectNoExceptions]
		public void TestLinkEntity()
		{
			var controller = NetworkTestCase.CreateMockableController(Mocks);
			var diagram1 = CreateJobAndDiagram(Factory);
			var jobHeader = CreateJobHeader<OrgHeader>();

			controller.Setup(m => m.PickEntity(ModuleIDs.ProcessHeader, false)).Returns(jobHeader);

			using (ObjectFactory.Substitute(controller.Object))
			{
				var networkViewModel = CreateNetworkViewModel(diagram1);
				var network = networkViewModel.GetJobNetwork();
				var wasRefreshed = false;
				network.Refreshed += delegate
				{
					wasRefreshed = true;
				};

				var startingCreateActionCount = networkViewModel.GetCreateEntityActions().Count();
				AssertNotEquals(0, networkViewModel.GetCreateEntityActions().Count());

				network.LinkEntity(diagram1, ModuleIDs.ProcessHeader);
				AssertEquals(true, wasRefreshed);
				AssertEquals(startingCreateActionCount, networkViewModel.GetCreateEntityActions().Count());
			}
		}

		public void TestLinkEntity_NoEntitySelected()
		{
			var controller = NetworkTestCase.CreateMockableController(Mocks);
			var diagram1 = CreateJobAndDiagram(Factory);

			controller.Setup(m => m.PickEntity(ModuleIDs.ProcessHeader, false)).Returns((BusinessObject)null);

			using (ObjectFactory.Substitute(controller.Object))
			{
				var network = CreateNetwork(diagram1);
				var wasRefreshed = false;
				network.Refreshed += delegate
				{
					wasRefreshed = true;
				};

				network.LinkEntity(diagram1, ModuleIDs.ProcessHeader);
				AssertEquals(false, wasRefreshed);
			}
		}

		public void TestLinkEntity_EntityAlreadyExistsOnDiagram()
		{
			var controller = NetworkTestCase.CreateMockableController(Mocks);
			var diagram1 = CreateJobAndDiagram(Factory);

			controller.Setup(m => m.PickEntity(ModuleIDs.ProcessHeader, false)).Returns(diagram1.ProcessJobHeader);

			using (ObjectFactory.Substitute(controller.Object))
			{
				var network = CreateNetwork(diagram1);
				var wasRefreshed = false;
				network.Refreshed += delegate
				{
					wasRefreshed = true;
				};

				network.LinkEntity(diagram1, ModuleIDs.ProcessHeader);
				AssertEquals(false, wasRefreshed);
				AssertEquals("Cannot link this Business Entity to the shape because another shape with this Business Entity already exists on the diagram.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestLinkEntity_EntityAlreadyExistsOnDiagram_AsHiddenEntity()
		{
			var controller = NetworkTestCase.CreateMockableController(Mocks);
			var diagram1 = CreateJobAndDiagram(Factory);
			var jobHeader2 = CreateJobHeader<OrgHeader>(false, "Other job Header");
			var subDiagram = CreateShape(jobHeader2, diagram1, "Subdiagram");

			var workflow = CreateWorkflow(jobHeader2, "I am a Workflow; AMA.");

			Factory.Save();

			controller.Setup(m => m.PickEntity(ModuleIDs.ProcessHeader, It.IsAny<bool>())).Returns(workflow);
			using (ObjectFactory.Substitute(controller.Object))
			{
				var networkViewModel = CreateNetworkViewModel(diagram1, controller: controller.Object);
				var network = networkViewModel.GetJobNetwork();

				var shape = networkViewModel.CreateNewShape(diagram1);

				var wasRefreshed = false;
				network.Refreshed += delegate
				{
					wasRefreshed = true;
				};

				network.LinkEntity(shape, ModuleIDs.ProcessHeader);
				AssertEquals(false, wasRefreshed);
				AssertEquals("Cannot link to an entity that is already a hidden entity on the diagram.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestLinkEntity_EntityAlreadyExistsOnDiagram_AsHiddenEntityOfParent()
		{
			var controller = NetworkTestCase.CreateMockableController(Mocks);
			var diagram1 = CreateJobAndDiagram(Factory);
			var workflow = CreateWorkflow(diagram1.ProcessJobHeader, "I am a Workflow; AMA.");

			controller.Setup(m => m.PickEntity(ModuleIDs.ProcessHeader, It.IsAny<bool>())).Returns(workflow);

			using (ObjectFactory.Substitute(controller.Object))
			{
				var networkViewModel = CreateNetworkViewModel(diagram1, controller: controller.Object);
				var network = networkViewModel.GetJobNetwork();
				var shape = networkViewModel.CreateNewShape(diagram1);

				var wasRefreshed = false;
				network.Refreshed += delegate
				{
					wasRefreshed = true;
				};

				network.LinkEntity(shape, ModuleIDs.ProcessHeader);
				AssertEquals("Link entity is allowed because it's a hidden entity of the diagram", true, wasRefreshed);
			}
		}

		#region Link to existing/descendants 

		public void TestLinkEntity_LinkWorkflowToShape()
		{
			var controller = NetworkTestCase.CreateMockableController(Mocks);
			//here we create a job header 1 and link this job header to the diagram1
			var diagram1 = CreateJobAndDiagram(Factory);

			//this is job header 2 with one workflow. we assume the normal workflow is a 'child' of a job level workflow
			//it means we do not create a real link in the ProcessHeaderLink table, but assume a 'virtual' link.
			var otherJobHeader = CreateJobHeader<OrgHeader>(false);
			//not a real child with a solid link (row in the ProcessHeaderLink table), but ordinary workflow
			var childWorkflow = CreateWorkflow(otherJobHeader, "Child Workflow 1");
			//we link childWorkflow to the shape on the diagram, which is a normal case and happens very often.
			var childShape = CreateShape(childWorkflow, diagram1, "Child Shape 1");

			controller.Setup(m => m.PickEntity(ModuleIDs.ProcessHeader, false)).Returns(otherJobHeader);

			using (ObjectFactory.Substitute(controller.Object))
			{
				var network = CreateNetwork(diagram1);
				var wasRefreshed = false;
				network.Refreshed += delegate
				{
					wasRefreshed = true;
				};

				network.LinkEntity(diagram1, ModuleIDs.ProcessHeader);

				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);

				AssertEquals(@"The entity being linked has associated workflow dependencies (pre/post-requisites and/or child entities). Do you want to add all child entities to the diagram that are necessary to display the dependencies in full? If you choose ""No"" the direct pre/post-requisite links (if any) between the displayed entities will be added only.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, wasRefreshed);
			}
		}

		public void TestLinkEntity_LinkCreated()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var childWorkflow1A = CreateWorkflow(jobHeader1, "Child Workflow 1A");
			AssertCollectionContains("GIVEN jobHeader1 and childWorkflow1A", childWorkflow1A, jobHeader1.ProcessHeaders);

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var childWorkflow2A = CreateWorkflow(jobHeader2, "Child Workflow 2A");
			AssertCollectionContains("GIVEN jobHeader2 and childWorkflow2A", childWorkflow2A, jobHeader2.ProcessHeaders);

			Factory.Save();

			BMSTestCaseWithFactory.AssertIsNotParent("GIVEN no link between childWorkflow1A and jobHeader2", childWorkflow1A, jobHeader2);

			var diagram = CreateDiagram(Factory);
			var parentShape = CreateShape(diagram, name: "Parent Shape 1");
			var childShape = CreateShape(parentShape, name: "Child Shape 1");
			AssertCollectionContains("GIVEN parentShape and childShape", childShape, parentShape.ChildShapes);

			var network = CreateNetwork(diagram);

			network.LinkEntity(childShape, childWorkflow1A);
			AssertNull("WHEN linkEntity childShape->childWorkflow, THEN no error shown", UnitTestUserNotification.Instance.LastMessage.Text);

			network.LinkEntity(parentShape, jobHeader2);
			AssertNull("WHEN linkEntity parentShape->jobHeader2, THEN no error shown", UnitTestUserNotification.Instance.LastMessage.Text);

			BMSTestCaseWithFactory.AssertIsParent("THEN PCH link should be create for childWorkflow1A and jobHeader2", childWorkflow1A, jobHeader2);
		}

		public void TestLinkEntity_LinkCreatedToDiagramShape_PromptShown_ShowChildren()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflowA = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow A");
			var workflowB = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow B");

			AssertCollectionContains("Workflow A should be added to Job Header", workflowA, jobHeader.ProcessHeaders);
			AssertCollectionContains("Workflow B should be added to Job Header", workflowB, jobHeader.ProcessHeaders);

			Factory.Save();

			var diagram = CreateDiagram(Factory);
			var network = CreateNetwork(diagram);

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);

			network.LinkEntity(diagram, jobHeader);

			AssertEquals(@"The entity being linked has associated workflow dependencies (pre/post-requisites and/or child entities). Do you want to add all child entities to the diagram that are necessary to display the dependencies in full? If you choose ""No"" the direct pre/post-requisite links (if any) between the displayed entities will be added only.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(2, network.Shapes.Count);
		}

		public void TestLinkEntity_LinkCreatedToShape_PromptNotShown()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflowA = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow A");
			var workflowB = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow B");

			AssertCollectionContains("Workflow A should be added to Job Header", workflowA, jobHeader.ProcessHeaders);
			AssertCollectionContains("Workflow B should be added to Job Header", workflowB, jobHeader.ProcessHeaders);

			Factory.Save();

			var diagram = CreateDiagram(Factory);
			var network = CreateNetwork(diagram);
			var shape = CreateShape(diagram, name: "Job Header or something");

			network.LinkEntity(shape, jobHeader);

			AssertNull("There should be no prompt displayed", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(1, network.Shapes.Count);
		}

		public void TestLinkEntity_LinkCreated_DoNotShowChildren()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var childWorkflow1A = CreateWorkflow(jobHeader1, "Child Workflow 1A");
			var childWorkflow1B = CreateWorkflow(jobHeader1, "Child Workflow 1B");

			AssertCollectionContains("GIVEN jobHeader1 and childWorkflow1A", childWorkflow1A, jobHeader1.ProcessHeaders);
			AssertCollectionContains("GIVEN jobHeader1 and childWorkflow1B", childWorkflow1B, jobHeader1.ProcessHeaders);

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var childWorkflow2A = CreateWorkflow(jobHeader2, "Child Workflow 2A");
			var childWorkflow2B = CreateWorkflow(jobHeader2, "Child Workflow 2B");

			AssertCollectionContains("GIVEN jobHeader2 and childWorkflow2A", childWorkflow2A, jobHeader2.ProcessHeaders);
			AssertCollectionContains("GIVEN jobHeader2 and childWorkflow2B", childWorkflow2B, jobHeader2.ProcessHeaders);

			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var childWorkflow3A = CreateWorkflow(jobHeader3, "Child Workflow 3A");
			var childWorkflow3B = CreateWorkflow(jobHeader3, "Child Workflow 3B");

			AssertCollectionContains("GIVEN jobHeader3 and childWorkflow3A", childWorkflow3A, jobHeader3.ProcessHeaders);
			AssertCollectionContains("GIVEN jobHeader3 and childWorkflow3B", childWorkflow3B, jobHeader3.ProcessHeaders);

			jobHeader2.GetOrCreateLinkToParent(jobHeader1);
			jobHeader3.GetOrCreateLinkToParent(jobHeader2);

			Assert(jobHeader2.IsChildOf(jobHeader1));
			Assert(jobHeader3.IsChildOf(jobHeader2));

			var diagram = CreateDiagram(Factory);
			var jobHeader1Shape = CreateShape(diagram, name: "Job Header 1");

			var network = CreateNetwork(diagram);

			network.LinkEntity(jobHeader1Shape, jobHeader1);

			AssertEquals(1, network.Shapes.Count);
		}

		public void TestLinkEntity_LinkNotCreated()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var parentWorkflow = CreateWorkflow(jobHeader, "Parent Workflow");
			var childWorkflow = CreateWorkflow(jobHeader, "Child Workflow");

			childWorkflow.GetOrCreateLinkToParent(parentWorkflow);

			BMSTestCaseWithFactory.AssertIsParent("GIVEN parent-child relationship", childWorkflow, parentWorkflow);

			Factory.Save();

			var diagram = CreateDiagram(Factory);
			var parentShape = CreateShape(diagram, name: "Parent Shape 1");
			var childShape = CreateShape(parentShape, name: "Child Shape 1");
			AssertCollectionContains("GIVEN parentShape and childShape", childShape, parentShape.ChildShapes);

			var network = CreateNetwork(diagram);

			network.LinkEntity(childShape, childWorkflow);
			AssertNull("WHEN linkEntity childShape->childWorkflow, THEN no error shown", UnitTestUserNotification.Instance.LastMessage.Text);

			network.LinkEntity(parentShape, jobHeader);
			AssertNull("WHEN LinkEntity parentShape->jobHeader, THEN no error shown", UnitTestUserNotification.Instance.LastMessage.Text);

			BMSTestCaseWithFactory.AssertIsNotParent("THEN ProcessHeaderLink should not be created between childWorkflow and jobHeader", childWorkflow, jobHeader);
			BMSTestCaseWithFactory.AssertIsParent("THEN ProcessHeaderLink should still exist between childWorkflow and parentWorkflow", childWorkflow, parentWorkflow);
		}

		public void TestLinkEntity_ChildWorkflowIsOnDifferentShape()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var childWorkflow = CreateWorkflow(jobHeader, "Child Workflow 1");
			AssertCollectionContains("GIVEN jobHeader and childWorkflow", childWorkflow, jobHeader.ProcessHeaders);

			Factory.Save();

			var diagram = CreateDiagram(Factory);
			var parentShape1 = CreateShape(diagram, name: "Parent Shape 1");
			var childShape1 = CreateShape(parentShape1, name: "Child Shape 1");
			AssertCollectionContains("GIVEN parentShape and childShape", childShape1, parentShape1.ChildShapes);

			var parentShape2 = CreateShape(diagram, name: "Parent Shape 2");
			AssertCollectionNotContains("GIVEN unrelated parentShape2", childShape1, parentShape2.ChildShapes);

			var network = CreateNetwork(diagram);

			network.LinkEntity(childShape1, childWorkflow);
			AssertNull("WHEN LinkEntity childShape->childWorkflow, THEN no error shown", UnitTestUserNotification.Instance.LastMessage.Text);

			network.LinkEntity(parentShape2, jobHeader);
			AssertNull("WHEN LinkEntity parentShape2->jobHeader, THEN should not error because we allow ancestors/descendants to be linked on separate subdiagram/shape", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestPrerequisiteNotCreatedForEntityLinkedToDiagram()
		{
			var job1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow_job1 = CreateWorkflow(job1, "workflow 1 for job1");

			var job2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow_job2 = CreateWorkflow(job2, "workflow 1 for job2");

			var job3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow_job3 = CreateWorkflow(job3, "workflow 1 for job3");

			var job4 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow_job4 = CreateWorkflow(job4, "workflow 1 for job4");

			Factory.Save();

			var diagram = CreateDiagram(Factory);

			var shape_job2 = CreateShape(diagram, name: "linked to job2");

			var shape_job3 = CreateShape(diagram, name: "linked to job3");

			var shape_job4 = CreateShape(diagram, name: "linked to job4");

			var network = CreateNetwork(diagram);
			network.LinkEntity(shape_job2, job2);
			network.LinkEntity(shape_job3, job3);
			network.LinkEntity(shape_job4, job4);

			network.CreateRelationship(shape_job2, shape_job3);

			network.CreateRelationship(shape_job4, shape_job3);

			network.LinkEntity(diagram, job1);
			Factory.Save();

			AssertEquals(3, job1.ChildLinks.Count());
			AssertNotNull(job1.ChildLinks.SingleOrDefault(x => x.HeaderFrom == job2));
			AssertNotNull(job1.ChildLinks.SingleOrDefault(x => x.HeaderFrom == job3));
			AssertNotNull(job1.ChildLinks.SingleOrDefault(x => x.HeaderFrom == job4));

			AssertEquals(2, diagram.AllAttachments.Count);
			AssertEquals(diagram.AllAttachments[0].BNA_Type, AttachmentTypeList.Codes.Dependency);
			AssertEquals(diagram.AllAttachments[1].BNA_Type, AttachmentTypeList.Codes.Dependency);

			Assert(job2.IsPrerequisiteOf(job3));
			Assert(job4.IsPrerequisiteOf(job3));

			AssertEquals("Job linked to diagram should not have prerequisites", 0, job1.PrerequisiteLinks.Count());
		}

		public void TestPrerequisiteNotCreatedForEntityLinkedToDiagram_WithChildWorkflowsDownTheHierarchy()
		{
			var job1_linkedToDiagram = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow_job1_linkedToDiagram = CreateWorkflow(job1_linkedToDiagram, "workflow 1 for job1");

			var job2_parent = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow_job2_parent = CreateWorkflow(job2_parent, "workflow 1 for job2");

			var job3_childForJob2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow_job3_child = CreateWorkflow(job3_childForJob2, "workflow 1 for job3");

			var job4_standaloneOnDiagram = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow_job4_standaloneOnDiagram = CreateWorkflow(job4_standaloneOnDiagram, "workflow 1 for job4");

			Factory.Save();

			var diagram = CreateDiagram(Factory);

			var shape_job2_parent = CreateShape(diagram, name: "linked to job2_parent");

			var shape_job3_child = CreateShape(shape_job2_parent, name: "linked to job3_childForJob2");

			var shape_job4_standalone = CreateShape(diagram, name: "linked to job4");

			var network = CreateNetwork(diagram);
			network.LinkEntity(shape_job2_parent, job2_parent);
			network.LinkEntity(shape_job3_child, job3_childForJob2);
			network.LinkEntity(shape_job4_standalone, job4_standaloneOnDiagram);

			network.CreateRelationship(shape_job3_child, shape_job4_standalone);

			network.LinkEntity(diagram, job1_linkedToDiagram);
			Factory.Save();

			AssertEquals(2, job1_linkedToDiagram.ChildLinks.Count());
			AssertNotNull(job1_linkedToDiagram.ChildLinks.SingleOrDefault(x => x.HeaderFrom == job2_parent));
			AssertNotNull(job1_linkedToDiagram.ChildLinks.SingleOrDefault(x => x.HeaderFrom == job4_standaloneOnDiagram));

			Assert(job3_childForJob2.IsPrerequisiteOf(job4_standaloneOnDiagram));

			AssertEquals("Job linked to diagram should not have prerequisites", 0, job1_linkedToDiagram.PrerequisiteLinks.Count());
		}

		public void TestLinkEntity_CircularRelationshipIsNotAllowedBetweenParentAndChild()
		{
			var parentJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Parent Job Header");
			var workflow1_parentJobHeader = CreateWorkflow(parentJobHeader, "Workflow 1 for Parent Job Header");

			var childJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Child Job Header");
			var workflow1_childJobHeader = CreateWorkflow(childJobHeader, "Workflow 1 for Child Job Header");

			childJobHeader.GetOrCreateLinkToParent(parentJobHeader);

			Factory.Save();
			Assert(childJobHeader.IsChildOf(parentJobHeader));

			var diagram = CreateDiagram(Factory);

			var parentShape = CreateShape(diagram, name: "Parent Shape");
			var childShape = CreateShape(parentShape, name: "Child Shape");
			var viewModelProvider = new JobNetworkNodeViewModelProvider();
			var networkViewModel = CreateNetworkViewModel(diagram, nodeViewModelProvider: viewModelProvider);
			var network = networkViewModel.GetJobNetwork();

			network.LinkEntity(childShape, parentJobHeader);

			var parentShapeViewModel = viewModelProvider.Create(network.Entities.GetInstance(parentShape), networkViewModel);

			//now try to link 'childJobHeader' (which has 'parentJobHeader' as a parent) to the 'parentShape'
			//please note, 'parentJobHeader' is already linked to the 'childShape'
			//this is potential circular dependency between 'parentShape' Entity and Entity linked to his child shape and should not be allowed. 
			network.LinkEntity(parentShape, childJobHeader);

			//so we do not want this circular relation therefore RelationshipValidationFailure error should be reported
			AssertEquals("Cannot link this Business Entity to the shape because the parent of this Business Entity is already linked to the child shape. This would cause a circular dependency between entities.", UnitTestUserNotification.Instance.LastMessage.Text);

			network.UnlinkEntity(childShape);
			network.LinkEntity(parentShape, childJobHeader); // this is fine now

			network.LinkEntity(childShape, parentJobHeader);
			//no circular relation allowed, therefore RelationshipValidationFailure error should be reported
			AssertEquals("Cannot link this Business Entity to the shape because the child of this Business Entity is already linked to the parent shape. This would cause a circular dependency between entities.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestLinkEntity_CircularRelationshipIsNotAllowedWhenNestedChildren()
		{
			var grandparentJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Grandparent Job Header");
			var workflow1_grandparentJobHeader = CreateWorkflow(grandparentJobHeader, "Workflow 1 for grandparent Job Header");

			var parentJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Parent Job Header");
			var workflow1_parentJobHeader = CreateWorkflow(parentJobHeader, "Workflow 1 for Parent Job Header");

			var childJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Child Job Header");
			var workflow1_childJobHeader = CreateWorkflow(childJobHeader, "Workflow 1 for Child Job Header");

			parentJobHeader.GetOrCreateLinkToParent(grandparentJobHeader);
			childJobHeader.GetOrCreateLinkToParent(parentJobHeader);

			Factory.Save();
			Assert(parentJobHeader.IsChildOf(grandparentJobHeader));
			Assert(childJobHeader.IsChildOf(parentJobHeader));

			var diagram = CreateDiagram(Factory);

			var grandparentShape = CreateShape(diagram, name: "Grandparent Shape");
			var parentShape = CreateShape(grandparentShape, name: "Parent Shape");
			var childShape = CreateShape(parentShape, name: "Child Shape");
			var network = CreateNetwork(diagram);

			network.LinkEntity(childShape, parentJobHeader);

			network.LinkEntity(parentShape, childJobHeader);
			AssertEquals("Cannot link this Business Entity to the shape because the parent of this Business Entity is already linked to the child shape. This would cause a circular dependency between entities.", UnitTestUserNotification.Instance.LastMessage.Text);

			network.LinkEntity(childShape, grandparentJobHeader);
			network.LinkEntity(parentShape, childJobHeader);
			AssertEquals("Cannot link this Business Entity to the shape because the parent of this Business Entity is already linked to the child shape. This would cause a circular dependency between entities.", UnitTestUserNotification.Instance.LastMessage.Text);

			network.LinkEntity(grandparentShape, childJobHeader);
			AssertEquals("Cannot link this Business Entity to the shape because the parent of this Business Entity is already linked to the child shape. This would cause a circular dependency between entities.", UnitTestUserNotification.Instance.LastMessage.Text);

			network.LinkEntity(grandparentShape, parentJobHeader);
			AssertEquals("Cannot link this Business Entity to the shape because the parent of this Business Entity is already linked to the child shape. This would cause a circular dependency between entities.", UnitTestUserNotification.Instance.LastMessage.Text);

			network.UnlinkEntity(childShape);
			network.LinkEntity(grandparentShape, childJobHeader); // this is fine now

			network.LinkEntity(parentShape, parentJobHeader);
			AssertEquals("Cannot link this Business Entity to the shape because the child of this Business Entity is already linked to the parent shape. This would cause a circular dependency between entities.", UnitTestUserNotification.Instance.LastMessage.Text);

			network.LinkEntity(childShape, parentJobHeader);
			AssertEquals("Cannot link this Business Entity to the shape because the child of this Business Entity is already linked to the parent shape. This would cause a circular dependency between entities.", UnitTestUserNotification.Instance.LastMessage.Text);

			network.LinkEntity(childShape, grandparentJobHeader);
			AssertEquals("Cannot link this Business Entity to the shape because the child of this Business Entity is already linked to the parent shape. This would cause a circular dependency between entities.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestLinkEntity_NewlyLinkedEntityChildDepedency_OfAndTo_ExistingEntityOnDiagram()
		{
			CreateSystem("ORG");

			var postrequisiteJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var prerequisiteJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflowWithLinksToExistingEntities = CreateWorkflow(jobHeader, "This is complete or something");

			AssertContainsExactElementsInAnyOrder("Precondition", new[] { workflowWithLinksToExistingEntities }, jobHeader.ChildHeaders);

			workflowWithLinksToExistingEntities.MakePrerequisiteOf(postrequisiteJobHeader);
			prerequisiteJobHeader.MakePrerequisiteOf(workflowWithLinksToExistingEntities);

			var diagram = CreateDiagram(Factory);
			var postrequisiteShape = CreateShape(postrequisiteJobHeader, diagram);
			var prerequisiteShape = CreateShape(prerequisiteJobHeader, diagram);
			var shape = CreateShape(diagram);

			Factory.Save();

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);

			network.LinkEntity(shape, jobHeader);

			AssertEquals(@"The entity being linked has associated workflow dependencies (pre/post-requisites and/or child entities). Do you want to add all child entities to the diagram that are necessary to display the dependencies in full? If you choose ""No"" the direct pre/post-requisite links (if any) between the displayed entities will be added only.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertContainsExactElementsInAnyOrder(new[] { prerequisiteJobHeader, postrequisiteJobHeader, jobHeader }, diagram.ChildShapes.Select(s => s.ProcessHeader));
			AssertContainsExactElementsInAnyOrder(new[] { workflowWithLinksToExistingEntities }, shape.ChildShapes.Select(s => s.ProcessHeader));
		}	

		public void TestLinkEntity_NewlyLinkedEntityDepedency_OfAndTo_ExistingEntityChildOnDiagram()
		{
			CreateSystem("ORG");

			var jobHeaderWithPrerequisiteChild = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var prerequisiteChild = CreateWorkflow(jobHeaderWithPrerequisiteChild, "this is complete or whatever");
			var jobHeaderWithPostrequisiteChild = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var postrequisiteChild = CreateWorkflow(jobHeaderWithPostrequisiteChild, "this is complete or whatever");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			AssertContainsExactElementsInAnyOrder("Precondition", new[] { prerequisiteChild }, jobHeaderWithPrerequisiteChild.ChildHeaders);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { postrequisiteChild }, jobHeaderWithPostrequisiteChild.ChildHeaders);

			jobHeader.MakePrerequisiteOf(postrequisiteChild);
			prerequisiteChild.MakePrerequisiteOf(jobHeader);

			Factory.Save();

			var diagram = CreateDiagram(Factory);
			var parentShape1 = CreateShape(jobHeaderWithPostrequisiteChild, diagram);
			var parentShape2 = CreateShape(jobHeaderWithPrerequisiteChild, diagram);
			var shape = CreateShape(diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);

			network.LinkEntity(shape, jobHeader);

			AssertEquals(@"The entity being linked has associated workflow dependencies (pre/post-requisites and/or child entities). Do you want to add all child entities to the diagram that are necessary to display the dependencies in full? If you choose ""No"" the direct pre/post-requisite links (if any) between the displayed entities will be added only.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader, jobHeaderWithPostrequisiteChild, jobHeaderWithPrerequisiteChild }, diagram.ChildShapes.Select(s => s.ProcessHeader));
			AssertContainsExactElementsInAnyOrder(new[] { postrequisiteChild }, parentShape1.ChildShapes.Select(s => s.ProcessHeader));
			AssertContainsExactElementsInAnyOrder(new[] { prerequisiteChild }, parentShape2.ChildShapes.Select(s => s.ProcessHeader));
		}

		public void TestLinkEntity_NewlyLinkedEntityChildDepedency_OfAndTo_MultipleExistingEntitiesOnDiagram()
		{
			CreateSystem("ORG");

			var postrequisiteJobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var postrequisiteJobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var childWorkflow = CreateWorkflow(jobHeader, "this is once again complete or whatever");

			AssertContainsExactElementsInAnyOrder("Precondition", new[] { childWorkflow }, jobHeader.ChildHeaders);

			childWorkflow.MakePrerequisiteOf(postrequisiteJobHeader1);
			childWorkflow.MakePrerequisiteOf(postrequisiteJobHeader2);

			var diagram = CreateDiagram(Factory);
			var parentShape1 = CreateShape(postrequisiteJobHeader1, diagram);
			var parentShape2 = CreateShape(postrequisiteJobHeader2, diagram);
			var shape = CreateShape(diagram);

			Factory.Save();

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);

			network.LinkEntity(shape, jobHeader);

			AssertEquals(@"The entity being linked has associated workflow dependencies (pre/post-requisites and/or child entities). Do you want to add all child entities to the diagram that are necessary to display the dependencies in full? If you choose ""No"" the direct pre/post-requisite links (if any) between the displayed entities will be added only.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertContainsExactElementsInAnyOrder(new[] { postrequisiteJobHeader1, postrequisiteJobHeader2, jobHeader }, diagram.ChildShapes.Select(s => s.ProcessHeader));
			AssertContainsExactElementsInAnyOrder(new[] { childWorkflow }, shape.ChildShapes.Select(s => s.ProcessHeader));
		}

		public void TestLinkEntity_NewlyLinkedEntityChildDepedency_OfAndTo_MultipleExistingChildEntitiesOnDiagram()
		{
			CreateSystem("ORG");

			var postrequisiteJobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var postrequisiteWorkflow1 = CreateWorkflow(postrequisiteJobHeader1, "this workflow is complete ig");
			var postrequisiteJobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var postrequisiteWorkflow2 = CreateWorkflow(postrequisiteJobHeader2, "this workflow is complete ig");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var childWorkflow = CreateWorkflow(jobHeader, "this is once again complete or whatever");

			AssertContainsExactElementsInAnyOrder("Precondition", new[] { postrequisiteWorkflow1 }, postrequisiteJobHeader1.ChildHeaders);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { postrequisiteWorkflow2 }, postrequisiteJobHeader2.ChildHeaders);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { childWorkflow }, jobHeader.ChildHeaders);

			childWorkflow.MakePrerequisiteOf(postrequisiteWorkflow1);
			childWorkflow.MakePrerequisiteOf(postrequisiteWorkflow2);

			var diagram = CreateDiagram(Factory);
			var parentShape1 = CreateShape(postrequisiteJobHeader1, diagram);
			var parentShape2 = CreateShape(postrequisiteJobHeader2, diagram);
			var shape = CreateShape(diagram);

			Factory.Save();

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);

			network.LinkEntity(shape, jobHeader);

			AssertEquals(@"The entity being linked has associated workflow dependencies (pre/post-requisites and/or child entities). Do you want to add all child entities to the diagram that are necessary to display the dependencies in full? If you choose ""No"" the direct pre/post-requisite links (if any) between the displayed entities will be added only.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertContainsExactElementsInAnyOrder(new[] { postrequisiteJobHeader1, postrequisiteJobHeader2, jobHeader }, diagram.ChildShapes.Select(s => s.ProcessHeader));
			AssertContainsExactElementsInAnyOrder("Postrequisite 1", new[] { postrequisiteWorkflow1 }, parentShape1.ChildShapes.Select(s => s.ProcessHeader));
			AssertContainsExactElementsInAnyOrder("Postrequisite 2", new[] { postrequisiteWorkflow2 }, parentShape2.ChildShapes.Select(s => s.ProcessHeader));
			AssertContainsExactElementsInAnyOrder(new[] { childWorkflow }, shape.ChildShapes.Select(s => s.ProcessHeader));
		}

		public void TestLinkEntity_NewlyLinkedEntityChildDepedency_OfAndTo_MultipleExistingEntitiesOnDiagram_Postreq()
		{
			CreateSystem("ORG");

			var prerequisiteJobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var prerequisiteJobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var childWorkflow = CreateWorkflow(jobHeader, "this is once again complete or whatever");

			AssertContainsExactElementsInAnyOrder("Precondition", new[] { childWorkflow }, jobHeader.ChildHeaders);

			prerequisiteJobHeader1.MakePrerequisiteOf(childWorkflow);
			prerequisiteJobHeader2.MakePrerequisiteOf(childWorkflow);

			var diagram = CreateDiagram(Factory);
			var parentShape1 = CreateShape(prerequisiteJobHeader1, diagram);
			var parentShape2 = CreateShape(prerequisiteJobHeader2, diagram);
			var shape = CreateShape(diagram);

			Factory.Save();

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);

			network.LinkEntity(shape, jobHeader);

			AssertEquals(@"The entity being linked has associated workflow dependencies (pre/post-requisites and/or child entities). Do you want to add all child entities to the diagram that are necessary to display the dependencies in full? If you choose ""No"" the direct pre/post-requisite links (if any) between the displayed entities will be added only.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertContainsExactElementsInAnyOrder(new[] { prerequisiteJobHeader1, prerequisiteJobHeader2, jobHeader }, diagram.ChildShapes.Select(s => s.ProcessHeader));
			AssertContainsExactElementsInAnyOrder(new[] { childWorkflow }, shape.ChildShapes.Select(s => s.ProcessHeader));
		}

		public void TestLinkEntity_NewlyLinkedEntityChildDepedency_OfAndTo_MultipleExistingChildEntitiesOnDiagram_Prerequisite()
		{
			CreateSystem("ORG");

			var prerequisiteJobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var prerequisiteWorkflow1 = CreateWorkflow(prerequisiteJobHeader1, "this workflow is complete ig");
			var prerequisiteJobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var prerequisiteWorkflow2 = CreateWorkflow(prerequisiteJobHeader2, "this workflow is complete ig");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var childWorkflow = CreateWorkflow(jobHeader, "this is once again complete or whatever");

			AssertContainsExactElementsInAnyOrder("Precondition", new[] { prerequisiteWorkflow1 }, prerequisiteJobHeader1.ChildHeaders);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { prerequisiteWorkflow2 }, prerequisiteJobHeader2.ChildHeaders);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { childWorkflow }, jobHeader.ChildHeaders);

			prerequisiteWorkflow1.MakePrerequisiteOf(childWorkflow);
			prerequisiteWorkflow2.MakePrerequisiteOf(childWorkflow);

			var diagram = CreateDiagram(Factory);
			var parentShape1 = CreateShape(prerequisiteJobHeader1, diagram);
			var parentShape2 = CreateShape(prerequisiteJobHeader2, diagram);
			var shape = CreateShape(diagram);

			Factory.Save();

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);

			network.LinkEntity(shape, jobHeader);

			AssertEquals(@"The entity being linked has associated workflow dependencies (pre/post-requisites and/or child entities). Do you want to add all child entities to the diagram that are necessary to display the dependencies in full? If you choose ""No"" the direct pre/post-requisite links (if any) between the displayed entities will be added only.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertContainsExactElementsInAnyOrder(new[] { prerequisiteJobHeader1, prerequisiteJobHeader2, jobHeader }, diagram.ChildShapes.Select(s => s.ProcessHeader));
			AssertContainsExactElementsInAnyOrder(new[] { prerequisiteWorkflow1 }, parentShape1.ChildShapes.Select(s => s.ProcessHeader));
			AssertContainsExactElementsInAnyOrder(new[] { prerequisiteWorkflow2 }, parentShape2.ChildShapes.Select(s => s.ProcessHeader));
			AssertContainsExactElementsInAnyOrder(new[] { childWorkflow }, shape.ChildShapes.Select(s => s.ProcessHeader));
		}

		public void TestLinkEntity_NewlyLinkedEntityChildDependency_OfAndTo_ExistingEntityChildOnDiagram()
		{
			CreateSystem("ORG");

			var jobHeaderWithPrerequisiteChild = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var prerequisiteChild = CreateWorkflow(jobHeaderWithPrerequisiteChild, "this is complete or whatever");
			var jobHeaderWithPostrequisiteChild = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var postrequisiteChild = CreateWorkflow(jobHeaderWithPostrequisiteChild, "this is complete or whatever");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var childWorkflow = CreateWorkflow(jobHeader, "this is once again complete or whatever");

			AssertContainsExactElementsInAnyOrder("Precondition", new[] { prerequisiteChild }, jobHeaderWithPrerequisiteChild.ChildHeaders);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { postrequisiteChild }, jobHeaderWithPostrequisiteChild.ChildHeaders);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { childWorkflow }, jobHeader.ChildHeaders);

			childWorkflow.MakePrerequisiteOf(postrequisiteChild);
			prerequisiteChild.MakePrerequisiteOf(childWorkflow);

			var diagram = CreateDiagram(Factory);
			var parentShape1 = CreateShape(jobHeaderWithPostrequisiteChild, diagram);
			var parentShape2 = CreateShape(jobHeaderWithPrerequisiteChild, diagram);
			var shape = CreateShape(diagram);

			Factory.Save();

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);

			network.LinkEntity(shape, jobHeader);

			AssertEquals(@"The entity being linked has associated workflow dependencies (pre/post-requisites and/or child entities). Do you want to add all child entities to the diagram that are necessary to display the dependencies in full? If you choose ""No"" the direct pre/post-requisite links (if any) between the displayed entities will be added only.", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertContainsExactElementsInAnyOrder(new[] { jobHeader, jobHeaderWithPostrequisiteChild, jobHeaderWithPrerequisiteChild }, diagram.ChildShapes.Select(s => s.ProcessHeader));
			AssertContainsExactElementsInAnyOrder(new[] { postrequisiteChild }, parentShape1.ChildShapes.Select(s => s.ProcessHeader));
			AssertContainsExactElementsInAnyOrder(new[] { prerequisiteChild }, parentShape2.ChildShapes.Select(s => s.ProcessHeader));
			AssertContainsExactElementsInAnyOrder(new[] { childWorkflow }, shape.ChildShapes.Select(s => s.ProcessHeader));
		}
		public void TestLinkEntity_NewlyLinkedEntityChildDependency_OfAndTo_ExistingEntitiesOnDiagram()
		{
			CreateSystem(new[] { "ORG" });

			var prerequisiteJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var postrequisiteJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var childHeader = VisualBoardsTestHelper.CreateWorkflow(Factory, "there is a completion statement here");

			VisualBoardsTestHelper.MakeChildOf(childHeader, jobHeader);
			prerequisiteJobHeader.MakePrerequisiteOf(childHeader);
			childHeader.MakePrerequisiteOf(postrequisiteJobHeader);

			var diagram = CreateDiagram(Factory);
			var postrequisiteShape = CreateShape(postrequisiteJobHeader, diagram);
			var prerequisiteShape = CreateShape(prerequisiteJobHeader, diagram);
			var shape = CreateShape(diagram);

			Factory.Save();

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);

			network.LinkEntity(shape, jobHeader);

			AssertEquals(@"The entity being linked has associated workflow dependencies (pre/post-requisites and/or child entities). Do you want to add all child entities to the diagram that are necessary to display the dependencies in full? If you choose ""No"" the direct pre/post-requisite links (if any) between the displayed entities will be added only.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader, postrequisiteJobHeader, prerequisiteJobHeader }, diagram.ChildShapes.Select(s => s.ProcessHeader));
			AssertContainsExactElementsInAnyOrder(new[] { childHeader }, shape.ChildShapes.Select(s => s.ProcessHeader));
		}

		public void TestLinkEntity_NewlyLinkedEntityChildDependency_OfAndTo_ExistingEntitiesOnDiagram_LinkingPostAndPreReqSeparately()
		{
			CreateSystem(new[] { "ORG" });

			var prerequisiteJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var postrequisiteJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var childHeader = VisualBoardsTestHelper.CreateWorkflow(Factory, "there is a completion statement here");

			VisualBoardsTestHelper.MakeChildOf(childHeader, jobHeader);
			prerequisiteJobHeader.MakePrerequisiteOf(childHeader);
			childHeader.MakePrerequisiteOf(postrequisiteJobHeader);

			var diagram = CreateDiagram(Factory);
			var shapeWithLinkedChildHeader = CreateShape(jobHeader, diagram);
			var prerequisiteShape = CreateShape(diagram);
			var postrequisiteShape = CreateShape(diagram);

			Factory.Save();

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
			network.LinkEntity(prerequisiteShape, prerequisiteJobHeader);

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
			network.LinkEntity(postrequisiteShape, postrequisiteJobHeader);

			AssertEquals(@"The entity being linked has associated workflow dependencies (pre/post-requisites and/or child entities). Do you want to add all child entities to the diagram that are necessary to display the dependencies in full? If you choose ""No"" the direct pre/post-requisite links (if any) between the displayed entities will be added only.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader, postrequisiteJobHeader, prerequisiteJobHeader }, diagram.ChildShapes.Select(s => s.ProcessHeader));
			AssertContainsExactElementsInAnyOrder(new[] { childHeader }, shapeWithLinkedChildHeader.ChildShapes.Select(s => s.ProcessHeader));
		}

		public void TestLinkEntity_NewlyLinkedEntityChildDependency_OfAndTo_ChildrenOfExistingEntitiesOnDiagram_LinkingPostAndPreReqSeparately()
		{
			CreateSystem(new[] { "ORG" });

			var prerequisiteJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var prerequisiteJobHeaderChild = CreateWorkflow(prerequisiteJobHeader, "this is complete complete");
			var postrequisiteJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var postrequisiteJobHeaderChild = CreateWorkflow(postrequisiteJobHeader, "this is complete complete");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var childHeader = VisualBoardsTestHelper.CreateWorkflow(Factory, "there is a completion statement here");

			VisualBoardsTestHelper.MakeChildOf(childHeader, jobHeader);
			prerequisiteJobHeaderChild.MakePrerequisiteOf(childHeader);
			childHeader.MakePrerequisiteOf(postrequisiteJobHeaderChild);

			var diagram = CreateDiagram(Factory);
			var shapeWithLinkedChildHeader = CreateShape(jobHeader, diagram);
			var prerequisiteShape = CreateShape(diagram);
			var postrequisiteShape = CreateShape(diagram);

			Factory.Save();

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
			network.LinkEntity(prerequisiteShape, prerequisiteJobHeader);

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
			network.LinkEntity(postrequisiteShape, postrequisiteJobHeader);

			AssertEquals(@"The entity being linked has associated workflow dependencies (pre/post-requisites and/or child entities). Do you want to add all child entities to the diagram that are necessary to display the dependencies in full? If you choose ""No"" the direct pre/post-requisite links (if any) between the displayed entities will be added only.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader, postrequisiteJobHeader, prerequisiteJobHeader }, diagram.ChildShapes.Select(s => s.ProcessHeader));
			AssertContainsExactElementsInAnyOrder(new[] { childHeader }, shapeWithLinkedChildHeader.ChildShapes.Select(s => s.ProcessHeader));
			AssertContainsExactElementsInAnyOrder(new[] { postrequisiteJobHeaderChild }, postrequisiteShape.ChildShapes.Select(s => s.ProcessHeader));
			AssertContainsExactElementsInAnyOrder(new[] { prerequisiteJobHeaderChild }, prerequisiteShape.ChildShapes.Select(s => s.ProcessHeader));
		}

		public void TestLinkEntity_NewlyLinkedEntityChildDepedency_OfAndTo_ExistingEntityOnDiagram_UserRespondsNo()
		{
			CreateSystem("ORG");

			var postrequisiteJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var prerequisiteJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflowWithLinksToExistingEntities = CreateWorkflow(jobHeader, "This is complete or something");

			AssertContainsExactElementsInAnyOrder("Precondition", new[] { workflowWithLinksToExistingEntities }, jobHeader.ChildHeaders);

			workflowWithLinksToExistingEntities.MakePrerequisiteOf(postrequisiteJobHeader);
			prerequisiteJobHeader.MakePrerequisiteOf(workflowWithLinksToExistingEntities);

			var diagram = CreateDiagram(Factory);
			var postrequisiteShape = CreateShape(postrequisiteJobHeader, diagram);
			var prerequisiteShape = CreateShape(prerequisiteJobHeader, diagram);
			var shape = CreateShape(diagram);

			Factory.Save();

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);

			network.LinkEntity(shape, jobHeader);

			AssertEquals(@"The entity being linked has associated workflow dependencies (pre/post-requisites and/or child entities). Do you want to add all child entities to the diagram that are necessary to display the dependencies in full? If you choose ""No"" the direct pre/post-requisite links (if any) between the displayed entities will be added only.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader, postrequisiteJobHeader, prerequisiteJobHeader }, diagram.ChildShapes.Select(s => s.ProcessHeader));
			AssertEquals(0, shape.ChildShapes.Count);
			AssertCollectionNotContains("No child shapes should have been added to the diagram.", workflowWithLinksToExistingEntities, shape.ChildShapes.Select(s => s.ProcessHeader));
		}

		public void TestLinkEntity_NewlyLinkedEntityChildDepedency_OfAndTo_ExistingEntityOnDiagram_UserRespondsCancel()
		{
			CreateSystem("ORG");

			var postrequisiteJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var prerequisiteJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflowWithLinksToExistingEntities = CreateWorkflow(jobHeader, "This is complete or something");

			AssertContainsExactElementsInAnyOrder("Precondition", new[] { workflowWithLinksToExistingEntities }, jobHeader.ChildHeaders);

			workflowWithLinksToExistingEntities.MakePrerequisiteOf(postrequisiteJobHeader);
			prerequisiteJobHeader.MakePrerequisiteOf(workflowWithLinksToExistingEntities);

			var diagram = CreateDiagram(Factory);
			var postrequisiteShape = CreateShape(postrequisiteJobHeader, diagram);
			var prerequisiteShape = CreateShape(prerequisiteJobHeader, diagram);
			var shape = CreateShape(diagram);

			Factory.Save();

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Cancel);

			network.LinkEntity(shape, jobHeader);

			AssertEquals(@"The entity being linked has associated workflow dependencies (pre/post-requisites and/or child entities). Do you want to add all child entities to the diagram that are necessary to display the dependencies in full? If you choose ""No"" the direct pre/post-requisite links (if any) between the displayed entities will be added only.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertNotEquals("The entity should not be linked", jobHeader, shape.ProcessHeader);
		}
		#endregion

		public void TestLinkEntity_ShouldMoveDependencyLinksToSelectedEntity()
		{
			var diagram = CreateDiagram(Factory);

			CreateShape(diagram, name: "subDiagram");

			var subDiagram1 = CreateShape(diagram, name: "subDiagram1");
			var subDiagram2 = CreateShape(diagram, name: "subDiagram2");
			var subDiagram3 = CreateShape(diagram, name: "subDiagram3");

			var arrow1_2 = subDiagram1.MakeVisiblePrerequisiteOf(subDiagram2, diagram);
			var arrow2_3 = subDiagram2.MakeVisiblePrerequisiteOf(subDiagram3, diagram);

			var jobHeader1 = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var jobHeader3 = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);

			jobHeader1.FH_CompletionStatement = "jobHeader1";
			jobHeader2.FH_CompletionStatement = "jobHeader2";
			jobHeader3.FH_CompletionStatement = "jobHeader3";

			var workflow1 = CreateWorkflow(jobHeader1, "workflow1");
			var workflow2 = CreateWorkflow(jobHeader2, "workflow2");
			var workflow3 = CreateWorkflow(jobHeader3, "workflow3");
			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);
			var task3 = CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			var controller = NetworkTestCase.CreateMockableController(Mocks);

			controller
				.SetupSequence(m => m.PickEntity(It.IsAny<ModuleIdentifier>(), It.IsAny<bool>()))
				.Returns(jobHeader1).Returns(jobHeader2).Returns(jobHeader3);

			using (ObjectFactory.Substitute(controller.Object))
			{
				var networkViewModel = CreateNetworkViewModel(diagram);
				var network = networkViewModel.GetJobNetwork();
				network.SwitchToScaled();

				using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, subDiagram1))
				{
					new AddBufferAction(networkViewModel).GetChildActions().Cast<JobNetworkActionBase>().Single().Execute();
				}
				networkViewModel.PushAsLateAsPossible();

				var buffer = network.Shapes.OfType<BMNCNBufferShape>().Single();

				AssertEquals(WorkStatus.None, ((INetworkEntity)subDiagram1).Status);
				AssertEquals(WorkStatus.None, ((INetworkEntity)buffer).Status);
				AssertEquals(WorkStatus.None, ((INetworkEntity)subDiagram2).Status);
				AssertEquals(WorkStatus.None, ((INetworkEntity)subDiagram3).Status);

				AssertEquals(false, jobHeader1.IsPrerequisiteOf(jobHeader2));
				AssertEquals(false, jobHeader2.IsPrerequisiteOf(jobHeader3));

				network.LinkEntity(subDiagram1, ModuleIDs.ProcessHeader);
				network.LinkEntity(subDiagram2, ModuleIDs.ProcessHeader);
				network.LinkEntity(subDiagram3, ModuleIDs.ProcessHeader);

				Factory.Save();

				AssertEquals(WorkStatus.Startable, ((INetworkEntity)subDiagram1).Status);
				AssertEquals(WorkStatus.None, ((INetworkEntity)buffer).Status);
				AssertEquals(WorkStatus.Blocked, ((INetworkEntity)subDiagram2).Status);
				AssertEquals(WorkStatus.Blocked, ((INetworkEntity)subDiagram3).Status);

				AssertEquals(true, jobHeader1.IsPrerequisiteOf(jobHeader2));
				AssertEquals(true, jobHeader2.IsPrerequisiteOf(jobHeader3));

				task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				Factory.Save();

				AssertEquals(WorkStatus.Complete, ((INetworkEntity)subDiagram1).Status);
				AssertEquals(WorkStatus.Complete, ((INetworkEntity)buffer).Status);
				AssertEquals(WorkStatus.Startable, ((INetworkEntity)subDiagram2).Status);
				AssertEquals(WorkStatus.Blocked, ((INetworkEntity)subDiagram3).Status);
			}
		}

		public void TestDependencyLinkNegation_UpToGrandparents()
		{
			var diagram = CreateDiagram(Factory, name: "Dependency negation hierarchy");

			//prereq workitems
			var prereqGrandparentJobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false, description: "neg grand parent");
			var prereqGrandparentWorkflow1 = CreateWorkflow(prereqGrandparentJobHeader, "neg grand parent wf");
			var prereqGrandparentTask = CreateTask(prereqGrandparentWorkflow1, GlbStaff.CurrentUser.GS_Code, 60);

			var prereqParentJobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false, description: "neg parent");
			var prereqParentWorkflow1 = CreateWorkflow(prereqParentJobHeader, "neg parent wf");
			var prereqParentTask = CreateTask(prereqParentWorkflow1, GlbStaff.CurrentUser.GS_Code, 60);
			prereqParentJobHeader.GetOrCreateLinkToParent(prereqGrandparentJobHeader);

			var prereqChildJobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false, description: "neg child");
			var prereqChild_CompleteWorkflow = CreateWorkflow(prereqChildJobHeader, "neg child wf");
			var prereqChild_CompleteWorkflow_Task = CreateTask(prereqChild_CompleteWorkflow, GlbStaff.CurrentUser.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			prereqChildJobHeader.GetOrCreateLinkToParent(prereqParentJobHeader);

			//dependent workitems
			var dependentGrandparentJobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false, description: "neg dependent grandparent");
			var dependentGrandparentWorkflow1 = CreateWorkflow(dependentGrandparentJobHeader, "neg dependent grandparent wf");
			var dependentGrandparentTask = CreateTask(dependentGrandparentWorkflow1, GlbStaff.CurrentUser.GS_Code, 60);

			var dependentParentJobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false, description: "neg dependent parent");
			var dependentParentWorkflow1 = CreateWorkflow(dependentParentJobHeader, "neg dependent parent wf");
			var dependentParentTask = CreateTask(dependentParentWorkflow1, GlbStaff.CurrentUser.GS_Code, 60);
			dependentParentJobHeader.GetOrCreateLinkToParent(dependentGrandparentJobHeader);

			var dependentChildJobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false, description: "neg dependent child");
			var dependentChildWorkflow1 = CreateWorkflow(dependentChildJobHeader, "second top level");
			var dependentChildTask = CreateTask(dependentChildWorkflow1, GlbStaff.CurrentUser.GS_Code, 60);
			dependentChildJobHeader.GetOrCreateLinkToParent(dependentParentJobHeader);
			Factory.Save();

			//prereq shapes
			var prereqGrandparentSubDiagram = CreateShape(prereqGrandparentJobHeader, diagram, name: "neg grand parent");
			var prereqParentSubDiagram = CreateShape(prereqParentJobHeader, prereqGrandparentSubDiagram, name: "neg parent");
			var prereqChildSubDiagram = CreateShape(prereqChildJobHeader, prereqParentSubDiagram, name: "neg child");
			var childShape2 = CreateShape(prereqChild_CompleteWorkflow, parentShape: prereqChildSubDiagram, name: "Complete Workflow Shape");

			//dependent shapes
			var dependentGrandparentSubDiagram = CreateShape(dependentGrandparentJobHeader, diagram, name: "neg dependent grandparent");
			var dependentParentSubDiagram = CreateShape(dependentParentJobHeader, dependentGrandparentSubDiagram, name: "neg dependent parent");
			var dependentChildSubDiagram = CreateShape(dependentChildJobHeader, dependentParentSubDiagram, name: "neg dependent child");
			var dependentChildShape = CreateShape(dependentChildWorkflow1, dependentChildSubDiagram, name: "second top level Shape");
			var network = CreateNetwork(diagram);

			//dependency arrows
			childShape2.MakeVisiblePrerequisiteOf(dependentChildShape, diagram);
			AssertEquals(WorkStatus.Startable, ((INetworkEntity)dependentChildShape).Status);
			AssertEquals(WorkStatus.Startable, ((INetworkEntity)dependentGrandparentSubDiagram).Status);

			prereqGrandparentSubDiagram.MakeVisiblePrerequisiteOf(dependentGrandparentSubDiagram, diagram);
			AssertEquals(WorkStatus.Blocked, ((INetworkEntity)dependentGrandparentSubDiagram).Status);
			AssertEquals(WorkStatus.Startable, ((INetworkEntity)dependentChildShape).Status);
		}

		public void TestLinkEntities_WithOneParentAndTwoChildren_WithLinkBetweenJobChildAndShapeChild_WithShapeBecomingJob_ShouldNotHaveCircularDependencyError()
		{
			var parentJobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false, description: "Parent Job");
			var childJobHeader1 = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false, description: "Child Job 1");
			var childJobHeader2 = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false, description: "Child Job 2");

			BufferManagement.Business.Test.BMSTestHelper.MakeChildOf(childJobHeader1, parentJobHeader);

			Factory.Save();

			var diagram = CreateDiagram(parentJobHeader);
			var shape1 = CreateShape(childJobHeader1, diagram);
			var shape2 = CreateShape(diagram);

			shape1.MakeVisiblePrerequisiteOf(shape2, diagram);

			Factory.Save();

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.LinkEntity(shape2, childJobHeader2);

			foreach (var link in childJobHeader2.Links)
			{
				AssertNoErrors(link);
			}
		}

		#region Link to Scaled Diagram

		public void TestLinkEntityToScaledDiagram_WhenAlreadyLinkedOnCurrentDiagram_ShouldFail()
		{
			var mainDiagram = CreateDiagram(Factory, name: "Main Diagram", isScaled: false);
			var mainDiagramShape1 = CreateShape(mainDiagram);
			var mainDiagramShape2 = CreateShape(mainDiagram);
			var mainDiagramJobNetwork = CreateNetwork(mainDiagram);

			var referenceDiagram = CreateDiagram(Factory, name: "Reference Diagram", isScaled: true);

			LinkDiagramToShapeExpectNoPrompts(mainDiagramJobNetwork, mainDiagramShape1, referenceDiagram);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);
			Assert("Should not be able to link diagram to multiple shapes on the same diagram", !mainDiagramJobNetwork.LinkEntity(mainDiagramShape2, referenceDiagram));
			AssertEquals("Cannot link this Business Entity to the shape because another shape with this Business Entity already exists on the diagram.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestLinkEntityToScaledDiagram_WhenAlreadyLinkedOnAnotherNonScaledDiagram_ShouldSucceedWithoutPrompt()
		{
			var mainDiagram1 = CreateDiagram(Factory, name: "Main Diagram #1", isScaled: false);
			var mainDiagram1Shape = CreateShape(mainDiagram1);
			var mainDiagram1JobNetwork = CreateNetwork(mainDiagram1);

			var mainDiagram2 = CreateDiagram(Factory, name: "Main Diagram #2", isScaled: true);
			var mainDiagram2Shape = CreateShape(mainDiagram2);
			var mainDiagram2JobNetwork = CreateNetwork(mainDiagram2);

			var referenceDiagram = CreateDiagram(Factory, name: "Reference Diagram", isScaled: true);

			LinkDiagramToShapeExpectNoPrompts(mainDiagram1JobNetwork, mainDiagram1Shape, referenceDiagram);
			LinkDiagramToShapeExpectNoPrompts(mainDiagram2JobNetwork, mainDiagram2Shape, referenceDiagram);
		}

		public void TestLinkEntityToNonScaledDiagram_WhenAlreadyLinkedOnAnotherScaledDiagram_ShouldSucceedWithoutPrompt()
		{
			var mainDiagram1 = CreateDiagram(Factory, name: "Main Diagram #1", isScaled: true);
			var mainDiagram1Shape = CreateShape(mainDiagram1);
			var mainDiagram1JobNetwork = CreateNetwork(mainDiagram1);

			var mainDiagram2 = CreateDiagram(Factory, name: "Main Diagram #2", isScaled: false);
			var mainDiagram2Shape = CreateShape(mainDiagram2);
			var mainDiagram2JobNetwork = CreateNetwork(mainDiagram2);

			var referenceDiagram = CreateDiagram(Factory, name: "Reference Diagram", isScaled: true);

			LinkDiagramToShapeExpectNoPrompts(mainDiagram1JobNetwork, mainDiagram1Shape, referenceDiagram);
			LinkDiagramToShapeExpectNoPrompts(mainDiagram2JobNetwork, mainDiagram2Shape, referenceDiagram);
		}

		public void TestLinkEntityToScaledDiagramWithoutScheduleSync_WhenAlreadyLinkedOnAnotherScaledDiagramWithScheduleSync_ShouldSucceedWithoutPrompt()
		{
			var mainDiagram1 = CreateDiagram(Factory, name: "Main Diagram #1", isScaled: true);
			var mainDiagram1Shape = CreateShape(mainDiagram1);
			var mainDiagram1JobNetwork = CreateNetwork(mainDiagram1);

			var mainDiagram2 = CreateDiagram(Factory, name: "Main Diagram #2", isScaled: true);
			var mainDiagram2Shape = CreateShape(mainDiagram2);
			var mainDiagram2JobNetwork = CreateNetwork(mainDiagram2);

			var referenceDiagram = CreateDiagram(Factory, name: "Reference Diagram", isScaled: true);

			LinkDiagramToShapeExpectNoPrompts(mainDiagram1JobNetwork, mainDiagram1Shape, referenceDiagram);
			LinkDiagramToShapeExpectNoPrompts(mainDiagram2JobNetwork, mainDiagram2Shape, referenceDiagram, shouldSynchroniseSchedule: false);
		}

		public void TestLinkEntityToScaledDiagramWithScheduleSync_WhenAlreadyLinkedOnAnotherScaledDiagramWithoutScheduleSync_ShouldSucceedWithoutPrompt()
		{
			var mainDiagram1 = CreateDiagram(Factory, name: "Main Diagram #1", isScaled: true);
			var mainDiagram1Shape = CreateShape(mainDiagram1);
			var mainDiagram1JobNetwork = CreateNetwork(mainDiagram1);

			var mainDiagram2 = CreateDiagram(Factory, name: "Main Diagram #2", isScaled: true);
			var mainDiagram2Shape = CreateShape(mainDiagram2);
			var mainDiagram2JobNetwork = CreateNetwork(mainDiagram2);

			var referenceDiagram = CreateDiagram(Factory, name: "Reference Diagram", isScaled: true);

			LinkDiagramToShapeExpectNoPrompts(mainDiagram1JobNetwork, mainDiagram1Shape, referenceDiagram, shouldSynchroniseSchedule: false);
			LinkDiagramToShapeExpectNoPrompts(mainDiagram2JobNetwork, mainDiagram2Shape, referenceDiagram);
		}

		public void TestLinkEntityToScaledDiagram_WhenUnrelatedScaledDiagramLinksExist_ShouldSucceedWithoutPrompt()
		{
			var mainDiagram1 = CreateDiagram(Factory, name: "Main Diagram #1", isScaled: true);
			var mainDiagram1Shape = CreateShape(mainDiagram1);
			var mainDiagram1JobNetwork = CreateNetwork(mainDiagram1);

			var mainDiagram2 = CreateDiagram(Factory, name: "Main Diagram #2", isScaled: true);
			var mainDiagram2Shape = CreateShape(mainDiagram2);
			var mainDiagram2JobNetwork = CreateNetwork(mainDiagram2);

			var referenceDiagram1 = CreateDiagram(Factory, name: "Reference Diagram #1", isScaled: true);
			var referenceDiagram2 = CreateDiagram(Factory, name: "Reference Diagram #2", isScaled: true);

			LinkDiagramToShapeExpectNoPrompts(mainDiagram1JobNetwork, mainDiagram1Shape, referenceDiagram1);
			LinkDiagramToShapeExpectNoPrompts(mainDiagram2JobNetwork, mainDiagram2Shape, referenceDiagram2);
		}

		internal static void LinkDiagramToShapeExpectNoPrompts(JobNetwork jobNetwork, BMNCNShape shape, BMNCNRootDiagramShape diagramToLink, bool? shouldSynchroniseSchedule = true)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Assert($"Linking diagram '{diagramToLink.Name}' to a shape in network '{jobNetwork.Name}' was expected to succeed", jobNetwork.LinkEntity(shape, diagramToLink, shouldSynchroniseSchedule));
			Assert($"Expecting no confirmation prompts when linking diagram '{diagramToLink.Name}' to a shape in network '{jobNetwork.Name}'. Actual prompt: '{UnitTestUserNotification.Instance.LastMessage?.Text}'", UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		[ExpectNoExceptions]
		public void TestLinkEntityToScaledDiagram_WhenScaledDiagramIsAttachedToOriginalNonScaledDiagram()
		{
			var diagramA = CreateDiagram(Factory);
			var shapeA = CreateShape(diagramA);

			var diagramB = CreateDiagram(Factory);
			var diagramBScaled = CreateDiagram(Factory);
			diagramBScaled.SwitchToScaled();
			var attachment = diagramBScaled.Factory.New<BMNCNAttachment>();
			attachment.BNA_Type = AttachmentTypeList.Codes.SwitchToScaled;
			attachment.BNA_BNS_Owner = diagramB.PK;
			attachment.BNA_BNS_FromShape = diagramB.PK;
			attachment.BNA_BNS_ToShape = diagramBScaled.PK;

			var network = CreateNetwork(diagramA);
			AssertNoExceptionThrown(() => network.LinkEntity(shapeA, diagramBScaled));
		}

		#endregion

		[GuiTest]
		public void TestLinkEntity_AddsDefaultFilterToCollection_WhenLinkingToDiagram()
		{
			var diagram = CreateDiagram(Factory, name: "Top-level diagram");
			Factory.Save();

			ModuleTextFilter defaultFilter = null;

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(o =>
			{
				if (o is EmbeddedModulePopup popupForm)
				{
					defaultFilter = (ModuleTextFilter)popupForm.Module_ForTest.FilterBusinessObject[BMNCNShape.ModuleFilterConstants.DiagramType];
				}
			});

			var network = CreateNetwork(diagram);
			network.LinkEntity(network.DiagramEntity, ModuleIDs.NetworkDiagram);

			AssertNotNull(defaultFilter);
			AssertEquals(FilterVisibility.AlwaysVisible, defaultFilter.Visibility);
			AssertEquals(true, defaultFilter.ReadOnly);
			AssertEquals("DIA", defaultFilter.DefaultProperty);

			ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogsAndClearStackForTest();
		}

		#endregion

		#region Deleting Entities

		public void TestDeleteWorkflowOnNonDefaultDiagram_WhenPresentOnOtherDiagrams_ShouldPromptToHide()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			var defaultDiagram = jobHeader.GetDefaultDiagram();
			AssertEquals(1, defaultDiagram.ChildShapes.Count);

			var otherDiagram1 = CreateDiagram(jobHeader);
			var workflowShape1 = CreateShape(workflow, otherDiagram1);

			var otherDiagram2 = CreateDiagram(jobHeader);
			var workflowShape2 = CreateShape(workflow, otherDiagram2);

			Factory.Save();

			var network = CreateNetwork(otherDiagram1);
			AssertEquals(true, network.DeleteEntity(workflowShape1));
			AssertEquals("This entity is present on other diagrams and cannot be deleted. Would you like to hide the entity instead?", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals(false, workflow.IsDeleted);
			AssertEquals(true, workflowShape1.IsDeleted);
		}

		public void TestDeleteWorkflowOnNonDefaultDiagram_WhenPresentOnOtherDiagrams_ShouldPromptToHide_AnsweringNo()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			var defaultDiagram = jobHeader.GetDefaultDiagram();
			AssertEquals(1, defaultDiagram.ChildShapes.Count);

			var otherDiagram1 = CreateDiagram(jobHeader);
			var workflowShape1 = CreateShape(workflow, otherDiagram1);

			var otherDiagram2 = CreateDiagram(jobHeader);
			var workflowShape2 = CreateShape(workflow, otherDiagram2);

			Factory.Save();

			var network = CreateNetwork(otherDiagram1);
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Cancel);
			AssertEquals(false, network.DeleteEntity(workflowShape1));
			AssertEquals("This entity is present on other diagrams and cannot be deleted. Would you like to hide the entity instead?", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals(false, workflow.IsDeleted);
			AssertEquals(false, workflowShape1.IsDeleted);
		}

		public void TestDeleteWorkflowOnDefaultDiagram_WhenPresentOnOtherDiagrams_ShouldDeleteWorkflowAndUnLinkOtherShape()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			var defaultDiagram = jobHeader.GetDefaultDiagram();
			AssertEquals(1, defaultDiagram.ChildShapes.Count);

			var otherDiagram = CreateDiagram(jobHeader);
			var workflowShape = CreateShape(workflow, otherDiagram);

			Factory.Save();

			var network = CreateNetwork(defaultDiagram);
			AssertEquals(true, network.DeleteEntity(defaultDiagram.ChildShapes[0]));

			AssertEquals(true, workflow.IsDeleted);
			AssertEquals(false, workflowShape.IsDeleted);
			AssertNull(workflowShape.ProcessHeader);
		}

		public void TestDeleteWorkflowOnDefaultDiagram_WhenParentDeletedShouldNotExplode()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			var defaultDiagram = jobHeader.GetDefaultDiagram();
			AssertEquals(1, defaultDiagram.ChildShapes.Count);

			var otherDiagram = CreateDiagram(jobHeader);
			var workflowShape = workflow.GetDefaultShape(defaultDiagram);

			Factory.Save();

			var network = CreateNetwork(defaultDiagram);

			using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
			{
				defaultDiagram.Delete();
			}
		}

		public void TestDeleteEntity_ApprovedShape()
		{
			var resource = CreateStaffInCurrentBranchDept("DE", "Dave East");
			Factory.Save();

			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var approvedChildShape = networkViewModel.CreateNewShape(diagram);
			network.SwitchToScaled();

			using (Env.SetTemporaryUserContext(resource.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				networkViewModel.ToggleApproval();
			}

			var nonApprovedChildShape = networkViewModel.CreateNewShape(diagram);

			AssertEquals(true, network.DeleteEntity(nonApprovedChildShape));
			AssertEquals(true, nonApprovedChildShape.IsDeleted);
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals(false, network.DeleteEntity(approvedChildShape));
			AssertEquals(false, approvedChildShape.IsDeleted);
			AssertEquals("This shape cannot be deleted as it has been approved by [Dave East].", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region DeleteLink

		public void TestDeleteLinkOnNonDefaultDiagram_WhenPresentOnOtherDiagrams_ShouldPromptToHide()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			var defaultDiagram = jobHeader.GetDefaultDiagram();
			AssertEquals(2, defaultDiagram.ChildShapes.Count);

			var otherDiagram1 = CreateDiagram(jobHeader);
			var workflow1Shape1 = CreateShape(workflow1, otherDiagram1);
			var workflow2Shape1 = CreateShape(workflow2, otherDiagram1);
			var depAttachment1 = CreateDependencyAttachment(otherDiagram1, link, workflow1Shape1, workflow2Shape1);

			var otherDiagram2 = CreateDiagram(jobHeader);
			var workflow1Shape2 = CreateShape(workflow1, otherDiagram2);
			var workflow2Shape2 = CreateShape(workflow2, otherDiagram2);
			var depAttachment2 = CreateDependencyAttachment(otherDiagram2, link, workflow1Shape2, workflow2Shape2);

			Factory.Save();

			var network = CreateNetwork(otherDiagram1);
			AssertEquals(true, network.DeleteRelationship(depAttachment1));
			AssertEquals("This arrow is present on other diagrams and cannot be deleted. Would you like to hide the arrow instead?", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals(false, link.IsDeleted);
			AssertEquals(true, depAttachment1.IsDeleted);
		}

		public void TestDeleteLinkOnNonDefaultDiagram_WhenPresentOnOtherDiagrams_ShouldPromptToHide_AnsweringNo()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			var defaultDiagram = jobHeader.GetDefaultDiagram();
			AssertEquals(2, defaultDiagram.ChildShapes.Count);

			var otherDiagram1 = CreateDiagram(jobHeader);
			var workflow1Shape1 = CreateShape(workflow1, otherDiagram1);
			var workflow2Shape1 = CreateShape(workflow2, otherDiagram1);
			var depAttachment1 = CreateDependencyAttachment(otherDiagram1, link, workflow1Shape1, workflow2Shape1);

			var otherDiagram2 = CreateDiagram(jobHeader);
			var workflow1Shape2 = CreateShape(workflow1, otherDiagram2);
			var workflow2Shape2 = CreateShape(workflow2, otherDiagram2);
			var depAttachment2 = CreateDependencyAttachment(otherDiagram2, link, workflow1Shape2, workflow2Shape2);

			Factory.Save();

			var network = CreateNetwork(otherDiagram1);
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Cancel);
			AssertEquals(false, network.DeleteRelationship(depAttachment1));

			AssertEquals(false, link.IsDeleted);
			AssertEquals(false, depAttachment1.IsDeleted);
		}

		public void TestDeleteLinkOnDefaultDiagram_WhenPresentOnOtherDiagrams_ShouldDeleteAnyway()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			var defaultDiagram = jobHeader.GetDefaultDiagram();
			var defaultDiagramDEPAttachment = defaultDiagram.ChildShapes[0].DependencyAttachments[0];
			AssertEquals(2, defaultDiagram.ChildShapes.Count);

			var otherDiagram = CreateDiagram(jobHeader);
			var workflow1Shape = CreateShape(workflow1, otherDiagram);
			var workflow2Shape = CreateShape(workflow2, otherDiagram);
			var depAttachment1 = CreateDependencyAttachment(otherDiagram, link, workflow1Shape, workflow2Shape);

			Factory.Save();

			var network = CreateNetwork(defaultDiagram);
			AssertEquals(true, network.DeleteRelationship(defaultDiagramDEPAttachment));

			AssertEquals(true, link.IsDeleted);
			AssertEquals(true, defaultDiagramDEPAttachment.IsDeleted);
		}

		public void TestDeleteDeletedAttachment()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			var defaultDiagram = jobHeader.GetDefaultDiagram();
			var defaultDiagramDEPAttachment = defaultDiagram.ChildShapes[0].DependencyAttachments[0];
			AssertEquals(2, defaultDiagram.ChildShapes.Count);

			var otherDiagram = CreateDiagram(jobHeader);
			var workflow1Shape = CreateShape(workflow1, otherDiagram);
			var workflow2Shape = CreateShape(workflow2, otherDiagram);
			var depAttachment1 = CreateDependencyAttachment(otherDiagram, link, workflow1Shape, workflow2Shape);

			Factory.Save();

			var network = CreateNetwork(otherDiagram);
			depAttachment1.Delete();
			AssertNoExceptionThrown(() => network.DeleteRelationship(depAttachment1));
		}

		#endregion

		#region Properties

		public void TestIsReadOnly()
		{
			var diagram = CreateJobAndDiagram(Factory);
			var network = CreateNetwork(diagram);

			AssertEquals(false, network.IsReadOnly);

			diagram.IsReadOnly = true;
			AssertEquals(true, network.IsReadOnly);
		}

		public void TestIsReadOnly_OverriddenOnTheNetwork()
		{
			var diagram = CreateJobAndDiagram(Factory);
			var network = CreateNetwork(diagram);

			diagram.IsReadOnly = true;
			AssertEquals(true, network.IsReadOnly);

			network.IsReadOnly = false;
			AssertEquals(false, network.IsReadOnly);

			network.IsReadOnly = true;
			AssertEquals(true, network.IsReadOnly);
		}

		public void TestEntityName_ShouldBeJobDescription()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.OH_Code = "MAIORGSYD";

			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var network = CreateNetwork(jobHeader.GetDefaultDiagram());

			AssertEquals("Organization (MAIORGSYD) - Job Organization (MAIORGSYD) is complete.", network.Name);
		}

		#endregion

		#region HasChanges

		public void TestChangeJobName_ShouldSetHasChanges()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "MAIORGSYD";
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);
			var diagram = CreateDiagram(jobHeader);

			Factory.Save();

			var network = CreateNetwork(diagram);

			AssertEquals(false, diagram.HasChanges);

			((IProposedNetworkEntity)diagram).JobName = "URORGSYD";

			AssertEquals("URORGSYD", org.OH_FullName);
			AssertEquals(true, org.HasChanges);
			AssertEquals(true, diagram.HasChanges);
		}

		public void TestSetCompletionStatements_ShouldActivateHasChanges()
		{
			VisualBoardsTestHelper.MakeCompletionStatementTaskType("ORG", "COM");

			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader2.ProcessHeaders[0];

			var diagram = CreateDiagram(jobHeader1);
			var subDiagram = CreateShape(jobHeader2, diagram);
			var workflowShape = CreateShape(workflow, subDiagram);

			Factory.Save();

			var loadedDiagram = new BusinessObjectFactory().Load<BMNCNShape>(diagram.PK);

			var network = CreateNetwork(loadedDiagram);
			var loadedWorkflowShape = network.Shapes.Single(s => s.BNS_RelatedEntityID == workflow.PK);

			AssertEquals(false, loadedDiagram.HasChanges);
			((IProposedNetworkEntity)loadedWorkflowShape).CompletionCriteria =
				@"Such Criteria
					Very Completion
			Pls wrok on me
		Wow";
			loadedWorkflowShape.ProcessHeader.FH_DoNotStartBeforeDate = new ZDateTime();
			AssertEquals(true, loadedWorkflowShape.ProcessHeader.HasChanges);
			AssertEquals(true, loadedWorkflowShape.HasChanges);
			AssertEquals(true, loadedDiagram.HasChanges);
		}

		public void TestChangeAttachments_ShouldSetHasChanges()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "MAIORGSYD";
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);
			var diagram = CreateDiagram(jobHeader);

			Factory.Save();

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var shape1 = networkViewModel.CreateNewShape(diagram);
			var shape2 = networkViewModel.CreateNewShape(diagram);
			var link = network.CreateRelationship(shape1, shape2).AsAttachment();

			AssertEquals("Poke the entities collection to make sure it is initialised.", 2, network.Entities.Count);

			Factory.Save();

			AssertEquals(false, link.HasChanges);
			AssertEquals(false, diagram.HasChanges);

			link.HasChanges = true;

			AssertEquals(true, link.HasChanges);
			AssertEquals(true, diagram.HasChanges);
		}

		#endregion

		#region DefaultJobShape

		[TestDate(2015, 1, 1)]
		public void TestDefaultJobShape_InnoculateSelfAgainstDuplicity()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>(false);
			var workflow1 = CreateWorkflow(jobHeader1, "Workflow1");
			var workflow2 = CreateWorkflow(jobHeader1, "Workflow1");
			workflow1.MakePrerequisiteOf(workflow2);

			BMNCNShape defaultDiagram;
			BMNCNShape duplicateShape;

			TestDateAttribute.Date = new DateTime(2015, 1, 15);
			defaultDiagram = jobHeader1.GetDefaultDiagram();

			defaultDiagram.BNS_Name = "Shoop";
			AssertEquals(2, defaultDiagram.ChildShapes.Count);

			// We create a duplicate shape on a default diagram
			TestDateAttribute.Date = new DateTime(2015, 1, 16);
			duplicateShape = CreateShape(workflow1, defaultDiagram, "the dupe", ShapeTypeList.Codes.DefaultWorkflow);
			AssertEquals(false, duplicateShape.IsDeleted);
			AssertEquals(3, defaultDiagram.ChildShapes.Count);

			var network = CreateNetwork(defaultDiagram);
			AssertEquals("It is important to poke the Entities collection", 2, network.Entities.Count);
			AssertEquals("When we load the diagram it deletes the younger of the created shapes", true, duplicateShape.IsDeleted);

			AssertEquals("Shouldn't have reported a developer exception.", 0, ExceptionReporterTestListener.Instance.Count);

			var anotherDuplicateShape = CreateShape(workflow1, network.DiagramShape, "anotherDupe", ShapeTypeList.Codes.DefaultWorkflow);

			AssertEquals("It is important to poke the Entities collection", 2, network.Entities.Count);
			AssertEquals("Whilst the JobNetwork is monitoring shape creation it automatically drops duplicates.", true, anotherDuplicateShape.IsDeleted);

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestNonDefaultJobShape_DoesNotInnoculateSelfAgainstDuplicity()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>(false);
			var workflow1 = CreateWorkflow(jobHeader1, "Workflow1");
			var workflow2 = CreateWorkflow(jobHeader1, "Workflow1");
			workflow1.MakePrerequisiteOf(workflow2);

			var diagramShape = CreateDiagram(jobHeader1);
			AssertEquals(0, diagramShape.ChildShapes.Count);
			var shape1 = CreateShape(workflow1, diagramShape);
			var shape2 = CreateShape(workflow2, diagramShape);
			AssertEquals(2, diagramShape.ChildShapes.Count);

			var duplicateShape = CreateShape(workflow1, diagramShape, "the dupe");

			AssertEquals(false, duplicateShape.IsDeleted);
			AssertEquals(3, diagramShape.ChildShapes.Count);

			var network = CreateNetwork(diagramShape);
			AssertEquals("It is important to poke the Entities collection", 3, network.Entities.Count);
			AssertEquals("Don't delete the duplicate shape auto-magically", false, duplicateShape.IsDeleted);

			var anotherDuplicateShape = CreateShape(workflow1, network.DiagramEntity);
			AssertEquals("It is important to poke the Entities collection", 4, network.Entities.Count);
			AssertEquals("Don't delete this one either", false, anotherDuplicateShape.IsDeleted);
		}

		public void TestDefaultJobShape_NoNeedDetectDupShapesWhenInDB()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow1 = CreateWorkflow(jobHeader, "Workflow1");
			var workflow2 = CreateWorkflow(jobHeader, "Workflow1");
			workflow1.MakePrerequisiteOf(workflow2);
			var defaultShape = jobHeader.GetDefaultDiagram();
			defaultShape.BNS_Name = "Shoop";
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var duplicateShape = newFactory.Load<ProcessHeader>(workflow1.PK).GetDefaultShape(defaultShape);
			defaultShape.Factory.Save();

			var newNewFactory = newFactory.CreateNewFactory();
			var network = CreateNetwork(newNewFactory.Load<ProcessJobHeader>(jobHeader.PK).GetDefaultDiagram());
			AssertEquals("Should not report duplicated shapes when the entities are in DB.", 0, ExceptionReporterTestListener.Instance.Count);

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestDefaultJobShape_ShouldCreateAttachmentsForProcessHeaderLinks()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var workflow1_1 = jobHeader1.ProcessHeaders[0];
			workflow1_1.FH_CompletionStatement = "workflow1_1";
			var workflow1_2 = jobHeader1.ProcessHeaders.AddNew();
			workflow1_2.FH_CompletionStatement = "workflow1_2";
			var workflow1_3 = jobHeader1.ProcessHeaders.AddNew();
			workflow1_3.FH_CompletionStatement = "workflow1_3";

			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var workflow2_1 = jobHeader2.ProcessHeaders.AddNew();
			workflow2_1.FH_CompletionStatement = "workflow2_1";

			// In-scope dependencies (intra-job links)
			workflow1_1.MakePrerequisiteOf(workflow1_2);
			workflow1_2.MakePrerequisiteOf(workflow1_3);

			// Out-of-scope dependencies (inter-job links)
			jobHeader1.MakePrerequisiteOf(jobHeader2);
			workflow1_3.MakePrerequisiteOf(jobHeader2);
			workflow1_3.MakePrerequisiteOf(workflow2_1);

			var link11_12 = workflow1_1.LinksFromMeToOthers.Single();
			var link12_13 = workflow1_2.LinksFromMeToOthers.Single();

			var defaultShape = jobHeader1.GetDefaultDiagram();
			AssertEquals(3, defaultShape.ChildShapes.Count);

			CombineAssertions(() =>
			{
				AssertEquals(2, defaultShape.ChildDependencyAttachments.Count);
				AssertDiagramHasDependencyAttachment(defaultShape, link11_12, workflow1_1.GetDefaultShape(defaultShape), workflow1_2.GetDefaultShape(defaultShape));
				AssertDiagramHasDependencyAttachment(defaultShape, link12_13, workflow1_2.GetDefaultShape(defaultShape), workflow1_3.GetDefaultShape(defaultShape));
			});
		}

		public void TestDefaultJobShape_ShouldRefreshChildShapes_WhenAddedToJobNetwork()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var defaultDiagram = jobHeader.GetDefaultDiagram();
			AssertEquals(2, defaultDiagram.ChildShapes.Count);

			var newWorkflow = jobHeader.ProcessHeaders.AddNew();

			var newLink1 = workflow2.GetOrCreateDependencyLink(newWorkflow);
			var newLink2 = newWorkflow.GetOrCreateDependencyLink(workflow1);

			Factory.Save();

			AssertEquals(2, defaultDiagram.ChildShapes.Count);

			var loadedJobHeader = new BusinessObjectFactory().Load<ProcessJobHeader>(jobHeader.PK);
			var loadedDefaultDiagram = loadedJobHeader.GetDefaultDiagram();

			CreateNetwork(loadedDefaultDiagram);

			AssertEquals(3, loadedJobHeader.ProcessHeaders.Count);
			AssertEquals(3, loadedDefaultDiagram.ChildShapes.Count);
			AssertEquals(2, loadedDefaultDiagram.ChildDependencyAttachments.Count);

			var attachment1 = loadedDefaultDiagram.ChildDependencyAttachments.FirstOrDefault(l => l.BNA_FP_ProcessHeaderLink == newLink1.PK);
			var attachment2 = loadedDefaultDiagram.ChildDependencyAttachments.FirstOrDefault(l => l.BNA_FP_ProcessHeaderLink == newLink2.PK);

			AssertNotNull(attachment1);
			AssertNotNull(attachment2);
		}

		public void TestDefaultJobShape_ShouldRefreshChildAttachment_WhenAddedToJobNetwork()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			var defaultDiagram = jobHeader.GetDefaultDiagram();
			AssertEquals(3, defaultDiagram.ChildShapes.Count);
			AssertEquals(0, defaultDiagram.ChildDependencyAttachments.Count);

			var newLink1 = workflow1.GetOrCreateDependencyLink(workflow2);
			var newLink2 = workflow2.GetOrCreateDependencyLink(workflow3);

			Factory.Save();

			AssertEquals(3, defaultDiagram.ChildShapes.Count);

			var loadedDefaultDiagram = new BusinessObjectFactory().Load<ProcessJobHeader>(jobHeader.PK).GetDefaultDiagram();
			CreateNetwork(loadedDefaultDiagram);

			AssertEquals(3, loadedDefaultDiagram.ChildShapes.Count);
			AssertEquals(2, loadedDefaultDiagram.ChildDependencyAttachments.Count);

			var attachment1 = loadedDefaultDiagram.ChildDependencyAttachments.FirstOrDefault(l => l.BNA_FP_ProcessHeaderLink == newLink1.PK);
			var attachment2 = loadedDefaultDiagram.ChildDependencyAttachments.FirstOrDefault(l => l.BNA_FP_ProcessHeaderLink == newLink2.PK);

			AssertNotNull(attachment1);
			AssertNotNull(attachment2);
		}

		public void TestReload_ForDefaultDiagram_ShouldCreateMissingDependencyAttachments()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var defaultdiagram = jobHeader.GetDefaultDiagram();
			var network = CreateNetwork(defaultdiagram);

			AssertEquals(2, network.Entities.Count);
			AssertEquals(0, network.Entities[0].Links.Count());
			AssertEquals(0, network.Entities[1].Links.Count());

			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			AssertEquals(2, network.Entities.Count);
			AssertEquals(0, network.Entities[0].Links.Count());
			AssertEquals(0, network.Entities[1].Links.Count());

			network.FullRefresh();

			AssertEquals(2, network.Entities.Count);
			AssertEquals(1, network.Entities[0].Links.Count());
			AssertEquals(1, network.Entities[1].Links.Count());
		}

		[TestDateIncremental(0, 0, 1, 0)]
		[TestDate(2019, 5, 10)]
		public void TestCreatingDuplicateDiagrams_ShouldNotProduceErrorReport()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>(false);
			var workflow1 = CreateWorkflow(jobHeader1, "Workflow1");

			var defaultDiagram = jobHeader1.GetDefaultDiagram();

			var duplicateShape = CreateShape(workflow1, defaultDiagram, "the dupe");

			Assert("The duplicate shape was not created", !duplicateShape.IsDeleted && defaultDiagram.ChildShapes.Count == 2);

			var network = CreateNetwork(defaultDiagram);

			Assert("A duplicate shape was not deleted.", defaultDiagram.ChildShapes.Count == 1);
			AssertEquals("The duplicate shape error report should not have been reported.", 0, ExceptionReporterTestListener.Instance.Count);

			var anotherDuplicateShape = CreateShape(workflow1, network.DiagramShape, "anotherDupe");

			Assert("A duplicate shape was not deleted", defaultDiagram.ChildShapes.Count == 1);
			AssertEquals("The duplicate shape error report should not have been reported.", 0, ExceptionReporterTestListener.Instance.Count);

			ExceptionReporterTestListener.Instance.Clear();
		}

		#endregion

		#region CreateNewEntity

		public void TestCreateNewEntity_Names()
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
			var diagram = CreateJobAndDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var shape = networkViewModel.CreateNewShape(diagram);
			var annotation = networkViewModel.CreateNewAnnotation(diagram);

			var defaultDiagram = diagram.ProcessJobHeader.GetDefaultDiagram();
			var defaultDiagramNetwork = CreateNetwork(defaultDiagram);
			var defaultShape = networkViewModel.CreateNewWorkflow(network.DiagramEntity);

			AssertEquals("New Shape", shape.Name);
			AssertEquals("New Annotation", annotation.Name);
			AssertEquals("New Workflow", defaultShape.Name);
		}

		public void TestCreateNewEntity_ShouldAttachToParent()
		{
			var diagram = CreateJobAndDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var shape = networkViewModel.CreateNewShape(diagram);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDiagram = newFactory.Load<BMNCNShape>(diagram.PK);
			AssertEquals(1, loadedDiagram.ChildShapes.Count);
			AssertEquals(shape.PK, loadedDiagram.ChildShapes[0].PK);
		}

		public void TestCreateNewEntity_DefaultDiagramCreatesSingleEntity()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			var diagram = jobHeader.GetDefaultDiagram();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			AssertEquals(1, network.Entities.Count);

			var entity = networkViewModel.CreateNewWorkflow(diagram);
			var defaultShape = entity.ProcessHeader.GetDefaultShape(diagram);

			AssertEquals(entity.Shape, defaultShape);

			network.FullRefresh();

			AssertEquals(2, network.Entities.Count);
		}

		public void TestCreateEntity_Shape()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var diagram = jobHeader.GetDefaultDiagram();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			AssertEquals(2, network.Entities.Count);

			var entity3 = networkViewModel.CreateNewWorkflow(diagram);
			var workflow3 = entity3.ProcessHeader;
			AssertNotNull(workflow3);

			AssertEquals(3, network.Entities.Count);
			AssertEquals(3, jobHeader.ProcessHeaders.Count);
			AssertCollectionContains(workflow3, jobHeader.ProcessHeaders);
		}

		public void TestCreateEntity_Annotation()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var diagram = jobHeader.GetDefaultDiagram();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			AssertEquals(2, network.Entities.Count);

			var annotation = networkViewModel.CreateNewAnnotation(diagram);

			AssertNull(annotation.ProcessHeader);
			AssertEquals("New Annotation", annotation.Name);
			AssertEquals(ShapeTypeList.Codes.Annotation, annotation.Shape.BNS_ShapeType);
		}

		public void TestCreateEntity_WithinSubDiagram()
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();

			var diagram = CreateDiagram(jobHeader1);
			var subDiagram = CreateShape(jobHeader2, diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			AssertEquals(1, network.Entities.Count);

			var subDiagramSubEntity = networkViewModel.CreateNewWorkflow(subDiagram);
			AssertEquals(subDiagram, subDiagramSubEntity.Owner.Shape);
			var subDiagramWorkflow = subDiagramSubEntity.ProcessHeader;
			AssertNotNull(subDiagramWorkflow);

			AssertEquals(2, network.Entities.Count);
			AssertCollectionContains(subDiagramWorkflow, jobHeader2.ProcessHeaders);

			var diagramSubEntity = networkViewModel.CreateNewWorkflow(diagram);
			AssertEquals(network.DiagramEntity, diagramSubEntity.Owner);
			var diagramWorkflow = diagramSubEntity.ProcessHeader;
			AssertNotNull(diagramWorkflow);

			AssertEquals(3, network.Entities.Count);
			AssertCollectionContains(diagramWorkflow, jobHeader1.ProcessHeaders);
		}

		public void TestCreateEntity_WithinSubDiagram_ShouldPickUpReleaseGroup()
		{
			var group = Factory.New<GlbGroup>();

			var system = VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader1.ProcessHeaders[0];
			var workflow2 = jobHeader2.ProcessHeaders[0];
			workflow1.FH_GG_ReleaseGroup = group.PK;
			workflow2.FH_GG_ReleaseGroup = group.PK;

			var diagram = CreateDiagram(jobHeader1);
			var subDiagram = CreateShape(jobHeader2, diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			AssertEquals(1, network.Entities.Count);

			var subDiagramSubEntity = networkViewModel.CreateNewWorkflow(subDiagram);
			AssertEquals(subDiagram, subDiagramSubEntity.Owner.Shape);
			var subDiagramWorkflow = subDiagramSubEntity.ProcessHeader;
			AssertNotNull(subDiagramWorkflow);
			AssertEquals(group, subDiagramWorkflow.ReleaseGroup);

			AssertEquals(2, network.Entities.Count);
			AssertCollectionContains(subDiagramWorkflow, jobHeader2.ProcessHeaders);

			var diagramSubEntity = networkViewModel.CreateNewWorkflow(diagram);
			AssertEquals(diagram, diagramSubEntity.Owner.Shape);
			var diagramWorkflow = diagramSubEntity.ProcessHeader;
			AssertNotNull(diagramWorkflow);
			AssertEquals(group, diagramWorkflow.ReleaseGroup);

			AssertEquals(3, network.Entities.Count);
			AssertCollectionContains(diagramWorkflow, jobHeader1.ProcessHeaders);
		}

		public void TestCreateNewEntity_ShouldDefaultProcessHeaderDescription()
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = CreateJobHeader<OrgHeader>();
			var diagram = CreateDiagram(jobHeader);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			AssertEquals(0, network.Entities.Count);

			var workflowShape = networkViewModel.CreateNewWorkflow(diagram);
			AssertEquals("New Workflow", workflowShape.Shape.BNS_Name);
			AssertEquals("New Workflow", workflowShape.ProcessHeader.FH_CompletionStatement);

			((IProposedNetworkEntity)workflowShape).Name = "Not so new shape";
			AssertEquals("Not so new shape", workflowShape.Shape.BNS_Name);
			AssertEquals("Not so new shape", workflowShape.ProcessHeader.FH_CompletionStatement);
		}

		public void TestCreateNewEntity_ShouldReloadDiagramActions()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagram);

			var startingActionCount = networkViewModel.GetCoreCustomNetworkActions_ForTesting().Count();
			AssertNotEquals(0, startingActionCount);

			networkViewModel.CreateNewShape(diagram);
			AssertEquals(startingActionCount, networkViewModel.GetCoreCustomNetworkActions_ForTesting().Count());
		}

		public void TestCreateNewEntity_ForNetworkAppliedFromTemplate_ShouldNotReportValidationErrors()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateWorkflow1 = BMSTestHelper.CreateWorkflow(template, "Zoot! Review.", releaseGroupPK: config.ReleaseGroup.PK);
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template, "As a guilty mum...", releaseGroupPK: config.ReleaseGroup.PK);
			var templateWorkflow3 = BMSTestHelper.CreateWorkflow(template, "Blah blah dettol blah.", releaseGroupPK: config.ReleaseGroup.PK);

			BMSTestHelper.CreateDependencyLink(template, templateWorkflow1, templateWorkflow2);
			BMSTestHelper.CreateTask(template, templateWorkflow1, string.Empty);
			BMSTestHelper.CreateTask(template, templateWorkflow2, string.Empty);
			BMSTestHelper.CreateTask(template, templateWorkflow3, string.Empty);

			Factory.Save();

			var job = Factory.NewWithValidTestData<OrgHeader>();

			job.ApplyWorkflowTemplates();

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(job, Factory);
			var diagram = jobHeader.GetDefaultDiagram();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			AssertEquals(3, network.Entities.Count);

			networkViewModel.CreateNewWorkflow(diagram);

			AssertNoErrors(jobHeader);
		}

		#endregion

		#region Relationships By Attachments

		public void TestDiagramEntityLinks_ShouldUseAttachments()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "workflow1";
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "workflow2";
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_CompletionStatement = "workflow3";

			var link1_2 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link2_3 = workflow2.GetOrCreateDependencyLink(workflow3);

			var diagram = CreateDiagram(jobHeader);
			var workflow1Shape = CreateShape(workflow1, diagram);
			var workflow2Shape = CreateShape(workflow2, diagram);
			var workflow3Shape = CreateShape(workflow3, diagram);

			var dependencyAttachment = CreateDependencyAttachment(diagram, link1_2, workflow1Shape, workflow2Shape);

			var network = CreateNetwork(diagram);
			AssertEquals(3, network.Entities.Count);

			var entity1 = network.Shapes.First(e => e.PK == workflow1Shape.PK);
			var entity2 = network.Shapes.First(e => e.PK == workflow2Shape.PK);

			AssertEquals(1, entity1.PostRequisiteLinks.Count());
			AssertEquals(0, entity2.PostRequisiteLinks.Count());

			AssertEquals(0, entity1.PreRequisiteLinks.Count());
			AssertEquals(1, entity2.PreRequisiteLinks.Count());
		}

		#endregion

		#region Entities

		public void TestEntities()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var network = CreateNetwork(jobHeader.GetDefaultDiagram());
			AssertEquals(2, network.Entities.Count);

			Assert(network.Shapes.Any(s => s.ProcessHeader == workflow1));
			Assert(network.Shapes.Any(s => s.ProcessHeader == workflow2));
		}

		#endregion

		#region ShowEntity

		public void TestShowEntity_ShouldUseWorkflowName()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_CompletionStatement = "workflow1";

			Factory.Save();

			var diagram = CreateDiagram(jobHeader);
			var network = CreateNetwork(diagram);
			AssertEquals(0, network.Entities.Count);
			AssertEquals(1, network.Entities.GetInstance(diagram).HiddenEntities.Count);
			var shape = network.ShowEntity(workflow, diagram).First().AsShape();

			AssertEquals("workflow1", shape.BNS_Name);
			AssertEquals("workflow1", shape.Name);

			shape.Name = "Dat Shape";

			AssertEquals("Dat Shape", shape.BNS_Name);
			AssertEquals("Dat Shape", workflow.FH_CompletionStatement);
		}

		public void TestShowEntity_SubDiagram()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();

			var jobHeader2 = CreateJobHeader<OrgHeader>();
			jobHeader2.GetOrCreateLinkToParent(jobHeader1);

			var diagram = CreateDiagram(jobHeader1);
			var network = CreateNetwork(diagram);
			AssertEquals(0, network.Entities.Count);

			var shownEntities = network.ShowEntity(jobHeader2, diagram).AsShapes().ToArray();
			AssertEquals(1, network.Entities.Count);
			AssertEquals(1, shownEntities.Length);

			AssertEquals(jobHeader2.PK, shownEntities[0].BNS_RelatedEntityID);
		}

		public void TestShowEntity_OfSubdiagram()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var workflow1_1 = jobHeader1.ProcessHeaders[0];
			var workflow1_2 = jobHeader1.ProcessHeaders.AddNew();

			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var workflow2_1 = jobHeader2.ProcessHeaders[0];
			var workflow2_2 = jobHeader2.ProcessHeaders.AddNew();

			Factory.Save();

			var diagram = CreateDiagram(jobHeader1);
			var workflow1_1Shape = CreateShape(workflow1_1, diagram);
			var workflow1_2Shape = CreateShape(workflow1_2, diagram);

			var subDiagram = CreateShape(jobHeader2, diagram);
			var workflow2_1Shape = CreateShape(workflow2_1, subDiagram);

			var network = CreateNetwork(diagram);

			AssertEquals(0, network.Entities.GetInstance(diagram).HiddenEntities.Count);
			AssertEquals(1, network.Entities.GetInstance(subDiagram).HiddenEntities.Count);

			AssertEquals(4, network.Entities.Count);

			var workflow2_2Shape = network.ShowEntity(workflow2_2, subDiagram).Single().AsShape();
			AssertEquals(subDiagram, workflow2_2Shape.AsEntity(network).Owner.Shape);

			AssertEquals(0, network.Entities.GetInstance(subDiagram).HiddenEntities.Count);
			AssertEquals(5, network.Entities.Count);
		}

		public void TestShowEntity_OfSubDiagram_ShouldUpdateSubDiagramHiddenRelationshipsAndLinks()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader2.ProcessHeaders[0];
			var workflow2 = jobHeader2.ProcessHeaders.AddNew();

			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			var diagram = CreateDiagram(jobHeader1);
			var subDiagram = CreateShape(jobHeader2, diagram);
			var workflow1Shape = CreateShape(workflow1, subDiagram);

			var network = CreateNetwork(diagram);
			AssertEquals(2, network.Entities.Count);
			AssertEquals(0, network.Entities.GetInstance(subDiagram).HiddenRelationships.Count);

			var workflow2Shape = network.ShowEntity(workflow2, subDiagram).Single().AsShape();
			AssertEquals(3, network.Entities.Count);

			AssertEquals(subDiagram, workflow1Shape.ParentShape);
			AssertEquals(0, network.Entities.GetInstance(workflow1Shape).HiddenRelationships.Count);
			AssertEquals(0, network.Entities.GetInstance(workflow2Shape).HiddenRelationships.Count);
			AssertEquals(0, network.Entities.GetInstance(subDiagram).HiddenRelationships.Count);

			AssertEquals(0, workflow1.PrerequisiteLinks(new BMNCNShapeDescendantsStrategy()).Count());
			AssertEquals(1, workflow1.PostrequisiteLinks(new BMNCNShapeDescendantsStrategy()).Count());
			AssertEquals(1, workflow2.PrerequisiteLinks(new BMNCNShapeDescendantsStrategy()).Count());
			AssertEquals(0, workflow2.PostrequisiteLinks(new BMNCNShapeDescendantsStrategy()).Count());
		}

		public void TestShowEntity_BetweenSubDiagrams_ShouldUpdateSubDiagramHiddenRelationshipsAndLinks()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var jobHeader3 = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader2.ProcessHeaders[0];
			var workflow2 = jobHeader3.ProcessHeaders[0];

			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			var diagram = CreateDiagram(jobHeader1);
			var subDiagram1 = CreateShape(jobHeader2, diagram);
			var subDiagram2 = CreateShape(jobHeader3, diagram);
			var workflow1Shape = CreateShape(workflow1, subDiagram1);

			var network = CreateNetwork(diagram);
			AssertEquals(3, network.Entities.Count);
			AssertEquals(0, network.Entities.GetInstance(subDiagram1).HiddenRelationships.Count);
			AssertEquals(0, network.Entities.GetInstance(subDiagram2).HiddenRelationships.Count);

			var workflow2Shape = network.ShowEntity(workflow2, subDiagram2).First().AsShape();

			AssertEquals(4, network.Entities.Count);
			AssertEquals(0, network.Entities.GetInstance(subDiagram1).HiddenRelationships.Count);
			AssertEquals(0, network.Entities.GetInstance(subDiagram2).HiddenRelationships.Count);

			AssertEquals(0, workflow1.PrerequisiteLinks(new BMNCNShapeDescendantsStrategy()).Count());
			AssertEquals(1, workflow1.PostrequisiteLinks(new BMNCNShapeDescendantsStrategy()).Count());
			AssertEquals(1, workflow2.PrerequisiteLinks(new BMNCNShapeDescendantsStrategy()).Count());
			AssertEquals(0, workflow2.PostrequisiteLinks(new BMNCNShapeDescendantsStrategy()).Count());
		}

		#endregion

		#region HideEntity

		public void TestHideEntity_DontAddNonChildrenAsHiddenEntities()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];

			var diagramShape = CreateDiagram(jobHeader);
			var workflowShape = CreateShape(workflow1, diagramShape);

			var otherJobHeader = CreateJobHeader<OrgHeader>();
			var otherDiagram = CreateShape(otherJobHeader, diagramShape);

			var network = CreateNetwork(diagramShape);
			AssertEquals(2, network.Entities.Count);

			network.HideEntity(workflowShape);

			AssertEquals(1, network.Entities.Count);
			AssertEquals(true, network.Entities.GetInstance(diagramShape).HiddenEntities.Contains(workflow1));
			AssertEquals(false, network.Entities.GetInstance(diagramShape).HiddenEntities.Contains(otherJobHeader));

			network.HideEntity(otherDiagram);

			AssertEquals(0, network.Entities.Count);
			AssertEquals(false, network.Entities.GetInstance(diagramShape).HiddenEntities.Contains(otherJobHeader));
		}

		public void TestHideEntity_ShouldDeleteShapeButNotTouchProcessHeader()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];

			var diagramShape = CreateDiagram(jobHeader);
			var workflowShape = CreateShape(workflow1, diagramShape);

			var network = CreateNetwork(diagramShape);
			AssertEquals(1, network.Entities.Count);

			network.HideEntity(network.Entities[0]);

			AssertEquals(true, workflowShape.IsDeleted);
			AssertEquals(false, workflow1.IsDeleted);
			AssertEquals(0, network.Entities.Count);
		}

		public void TestHideEntity_WhenNoProcessHeaderPresent()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();

			var diagramShape = CreateDiagram(jobHeader);
			var childShape = Factory.New<BMNCNShape>();
			childShape.MakeChildOf(diagramShape);

			var network = CreateNetwork(diagramShape);
			AssertEquals(1, network.Entities.Count);

			network.HideEntity(network.Entities[0]);

			AssertEquals(true, childShape.IsDeleted);
			AssertEquals(0, network.Entities.Count);
		}

		public void TestHideSubDiagram_ShouldHideChildrenAlso()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var workflow1_1 = jobHeader1.ProcessHeaders[0];

			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var workflow2_1 = jobHeader2.ProcessHeaders[0];
			workflow2_1.FH_CompletionStatement = "workflow2_1";
			var workflow2_2 = jobHeader2.ProcessHeaders[0];
			workflow2_2.FH_CompletionStatement = "workflow2_2";

			jobHeader2.GetOrCreateLinkToParent(jobHeader1);

			Factory.Save();

			AssertEquals(true, jobHeader1.IsParentOf(jobHeader2));
			AssertEquals(true, jobHeader2.IsChildOf(jobHeader1));

			var diagramShape = CreateDiagram(jobHeader1);
			var subDiagramShape = CreateShape(jobHeader2, diagramShape);
			var workflow1Shape = CreateShape(workflow2_1, subDiagramShape);
			var workflow2Shape = CreateShape(workflow2_2, subDiagramShape);

			var network = CreateNetwork(diagramShape);
			AssertEquals(3, network.Entities.Count);
			AssertEquals(1, network.Entities.GetInstance(diagramShape).HiddenEntities.Count);
			AssertCollectionContains(workflow1_1, network.Entities.GetInstance(diagramShape).HiddenEntities);

			network.HideEntity(subDiagramShape);

			AssertEquals(0, network.Entities.Count);
			AssertEquals(2, network.Entities.GetInstance(diagramShape).HiddenEntities.Count);

			AssertCollectionContains(workflow1_1, network.Entities.GetInstance(diagramShape).HiddenEntities);
			AssertCollectionContains(jobHeader2, network.Entities.GetInstance(diagramShape).HiddenEntities);

			AssertEquals(true, workflow1Shape.IsDeleted);
			AssertEquals(true, workflow2Shape.IsDeleted);
		}

		public void TestHideEntity_OfSubdiagram()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var workflow1_1 = jobHeader1.ProcessHeaders[0];
			var workflow1_2 = jobHeader1.ProcessHeaders.AddNew();

			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var workflow2_1 = jobHeader2.ProcessHeaders[0];
			var workflow2_2 = jobHeader2.ProcessHeaders.AddNew();

			var diagram = CreateDiagram(jobHeader1);
			var workflow1_1Shape = CreateShape(workflow1_1, diagram);
			var workflow1_2Shape = CreateShape(workflow1_2, diagram);

			var subDiagram = CreateShape(jobHeader2, diagram);
			var workflow2_1Shape = CreateShape(workflow2_1, subDiagram);
			var workflow2_2Shape = CreateShape(workflow2_2, subDiagram);

			var network = CreateNetwork(diagram);

			AssertEquals(0, network.Entities.GetInstance(diagram).HiddenEntities.Count);
			AssertEquals(0, network.Entities.GetInstance(subDiagram).HiddenEntities.Count);

			AssertEquals(5, network.Entities.Count);

			var hiddenShape = network.HideEntity(workflow2_2Shape).Single().AsShape();
			AssertEquals(workflow2_2Shape, hiddenShape);
			AssertEquals(true, workflow2_2Shape.IsDeleted);

			AssertEquals(0, network.Entities.GetInstance(diagram).HiddenEntities.Count);
			AssertEquals(1, network.Entities.GetInstance(subDiagram).HiddenEntities.Count);
			AssertEquals(4, network.Entities.Count);
		}

		public void TestHideEntity_OfSubDiagram_ShouldUpdateSubDiagramHiddenRelationships()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader2.ProcessHeaders[0];
			var workflow2 = jobHeader2.ProcessHeaders.AddNew();

			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			var diagram = CreateDiagram(jobHeader1);
			var subDiagram = CreateShape(jobHeader2, diagram);
			var workflow1Shape = CreateShape(workflow1, subDiagram);
			var workflow2Shape = CreateShape(workflow2, subDiagram);

			var network = CreateNetwork(diagram);
			AssertEquals(3, network.Entities.Count);
			AssertEquals(1, network.Entities.GetInstance(subDiagram).HiddenRelationships.Count);

			network.HideEntity(workflow2Shape);
			AssertEquals(2, network.Entities.Count);
			AssertEquals(0, network.Entities.GetInstance(subDiagram).HiddenRelationships.Count);
		}

		public void TestHideEntity_BetweenSubDiagrams_ShouldUpdateSubDiagramHiddenRelationships()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var jobHeader3 = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader2.ProcessHeaders[0];
			var workflow2 = jobHeader3.ProcessHeaders[0];

			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			var diagram = CreateDiagram(jobHeader1);
			var subDiagram1 = CreateShape(jobHeader2, diagram);
			var subDiagram2 = CreateShape(jobHeader3, diagram);
			var workflow1Shape = CreateShape(workflow1, subDiagram1);
			var workflow2Shape = CreateShape(workflow2, subDiagram2);

			var network = CreateNetwork(diagram);
			AssertEquals(4, network.Entities.Count);
			AssertEquals(1, network.Entities.GetInstance(subDiagram1).HiddenRelationships.Count);
			AssertEquals(1, network.Entities.GetInstance(subDiagram2).HiddenRelationships.Count);

			network.HideEntity(workflow2Shape);

			AssertEquals(3, network.Entities.Count);
			AssertEquals(0, network.Entities.GetInstance(subDiagram1).HiddenRelationships.Count);
			AssertEquals(0, network.Entities.GetInstance(subDiagram2).HiddenRelationships.Count);
		}

		public void TestHideEntity_ShouldNotExecuteForDiagram_FoolProof()
		{
			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);

			AssertEquals("Precondition", 2, network.Entities.Count);

			AssertEquals(0, network.HideEntity(diagram).Count());

			AssertEquals(2, network.Entities.Count);
			AssertEquals(false, diagram.IsDeleted);
			AssertEquals(false, childShape1.IsDeleted);
			AssertEquals(false, childShape2.IsDeleted);

			AssertEquals("Should not try to hide the diagram node.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestHideEntity_ApprovedShape()
		{
			var resource = CreateStaffInCurrentBranchDept("DE", "Dave East");
			Factory.Save();

			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var approvedChildShape = networkViewModel.CreateNewShape(diagram);
			network.SwitchToScaled();

			using (Env.SetTemporaryUserContext(resource.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				networkViewModel.ToggleApproval();
			}

			var nonApprovedChildShape = networkViewModel.CreateNewShape(diagram);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				nonApprovedChildShape
			}, network.HideEntity(nonApprovedChildShape));
			AssertEquals(true, nonApprovedChildShape.IsDeleted);
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			AssertContainsExactElementsInAnyOrder(Array.Empty<ShapeNetworkEntity>(), network.HideEntity(approvedChildShape));
			AssertEquals(false, approvedChildShape.IsDeleted);
			AssertEquals("This shape cannot be hidden as it has been approved by [Dave East].", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[GuiTest]
		public void TestHideEntity_ForShapeWithBuffer_ShouldAlsoRemoveBuffer()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");

			var diagram = CreateDiagram(jobHeader);
			var shape1 = CreateShape(workflow1, diagram);
			var shape2 = CreateShape(workflow2, diagram);

			shape1.MakeVisiblePrerequisiteOf(shape2, diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, shape1))
			{
				new AddBufferAction(networkViewModel).GetChildActions().Single().AsJobNetworkAction().Execute();
			}

			var buffer = network.Shapes.OfType<BMNCNBufferShape>().Single();

			var hiddenEntities = network.HideEntity(shape2).AsShapes();
			AssertContainsExactElementsInAnyOrder(new[]
			{
				shape2, buffer
			}, hiddenEntities);
			AssertEquals(true, buffer.IsDeleted);
		}

		public void TestHideEntity_ShouldUnLinkRelationships()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var diagram = CreateDiagram(jobHeader1);
			var subDiagram = CreateShape(diagram);

			var network = CreateNetwork(diagram);
			network.LinkEntity(subDiagram, jobHeader2);

			AssertIsParent(jobHeader2, jobHeader1);

			network.HideEntity(subDiagram);

			AssertIsNotParent(jobHeader2, jobHeader1);
		}

		[GuiTest] // Shows the user choice dialog, but this test belongs here as it tests the business logic results of that choice.
		public void TestHideEntity_WhenDependenciesExist_AndUserCancels_ShouldNotHideEntity()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var diagram = CreateDiagram(Factory);
			var shape1 = CreateShape(jobHeader1, diagram);
			var shape2 = CreateShape(jobHeader2, diagram);
			var attachment = shape1.MakeVisiblePrerequisiteOf(shape2, diagram);

			var refresher = CreateRefresher();
			var network = new JobNetwork_ForUnlinkMenuTest(diagram, refresher, UnlinkEntityVariant.Cancel);
			network.HideEntity(shape1);

			AssertEquals("The user pressed the Cancel button, so the shape should not have been removed. SAD!", false, shape1.IsDeleted);
			AssertEquals("The user pressed the Cancel button, so the arrow should not have been removed. SAD!", false, attachment.IsDeleted);

			var link = attachment.ProcessHeaderLink;
			AssertNotNull("The user pressed the Cancel button, so the prerequisite link should not have been deleted. SAD!", link);
			AssertEquals("The user pressed the Cancel button, so the prerequisite link should not have been deleted. SAD!", false, link.IsDeleted);
		}

		class JobNetwork_ForUnlinkMenuTest : JobNetwork
		{
			readonly UnlinkEntityVariant simulatedSelectedMenuOption;

			public JobNetwork_ForUnlinkMenuTest(BMNCNRootDiagramShape diagram, JobNetworkRefresher refresher, UnlinkEntityVariant simulatedSelectedMenuOption)
				: base(new BMNetworkViewModel(diagram), CreateController(diagram.Factory, refresher), refresher)
			{
				this.simulatedSelectedMenuOption = simulatedSelectedMenuOption;
			}

			protected override UnlinkEntityVariant GetUnLinkVariantFromUser(ButtonStripAction<UnlinkEntityVariant>[] strips, BMNCNShape shape, ZString workflowText)
			{
				return simulatedSelectedMenuOption;
			}
		}

		#endregion

		#region HideRelationship

		public void TestHideRelationship_BetweenEntitiesAtDifferentHierarchies()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var jobHeader3 = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader2.ProcessHeaders[0];
			var workflow2 = jobHeader3.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "workflow1";
			workflow2.FH_CompletionStatement = "workflow2";

			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			var diagram = CreateDiagram(jobHeader1);
			var subDiagram1 = CreateShape(jobHeader2, diagram);
			var subDiagram2 = CreateShape(jobHeader3, diagram);
			var workflow1Shape = CreateShape(workflow1, subDiagram1);
			var workflow2Shape = CreateShape(workflow2, subDiagram2);
			var attachment = CreateDependencyAttachment(diagram, link, workflow1Shape, workflow2Shape);

			var network = CreateNetwork(diagram);
			var refreshed = false;
			network.Refreshed += (s, e) => refreshed = true;
			AssertEquals(4, network.Entities.Count);

			AssertEquals(0, network.Entities.GetInstance(subDiagram1).HiddenRelationships.Count);
			AssertEquals(0, network.Entities.GetInstance(subDiagram2).HiddenRelationships.Count);

			network.HideRelationship(attachment);

			AssertEquals(false, refreshed);
			AssertEquals(1, network.Entities.GetInstance(subDiagram1).HiddenRelationships.Count);
			AssertEquals(1, network.Entities.GetInstance(subDiagram2).HiddenRelationships.Count);
		}

		public void TestHideRelationship_ShouldAlsoRemoveBufferFromDiagramAndRefreshNetwork()
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
			var diagram = CreateDiagram(Factory, isScaled: true);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			AssertEquals(0, network.Entities.Count);

			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);
			childShape1.Name = "childShape1";
			childShape2.Name = "childShape2";

			AssertEquals(2, network.Entities.Count);

			var dependency = network.CreateRelationship(childShape1, childShape2).AsAttachment();
			AssertNull(dependency.GetBuffer());

			var action = new AddBufferAction(networkViewModel);
			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, childShape1))
			{
				action.GetChildActions().Single().AsJobNetworkAction().Execute();
			}

			AssertEquals(3, network.Entities.Count);
			var buffer = dependency.GetBuffer();
			AssertNotNull(buffer);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var newNetwork = CreateNetwork(newFactory.Load<BMNCNShape>(diagram.PK));
			var refreshed = false;
			newNetwork.Refreshed += (s, e) => refreshed = true;

			var loadedDependency = newNetwork.Entities.First().Links.First().AsAttachment();
			var loadedBuffer = loadedDependency.GetBuffer();

			AssertNotNull(loadedBuffer);
			AssertEquals(3, newNetwork.Entities.Count);

			newNetwork.HideRelationship(loadedDependency);
			AssertEquals(true, refreshed);
			AssertEquals(2, newNetwork.Entities.Count);
			AssertEquals(true, loadedBuffer.IsDeleted);
		}

		public void TestHideRelationship_ForBufferLink_ShouldRemoveBufferAndLinks()
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
			var diagram = CreateDiagram(CreateJobHeader<OrgHeader>(), isScaled: true);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			AssertEquals(0, network.Entities.Count);

			var childShape1 = networkViewModel.CreateNewWorkflow(diagram);
			var childShape2 = networkViewModel.CreateNewWorkflow(diagram);
			childShape1.Name = "childShape1";
			childShape2.Name = "childShape2";

			AssertEquals(2, network.Entities.Count);

			var dependency = network.CreateRelationship(childShape1, childShape2).AsAttachment();
			AssertNull(dependency.GetBuffer());

			var action = new AddBufferAction(networkViewModel);
			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, childShape1))
			{
				action.GetChildActions().Single().AsJobNetworkAction().Execute();
			}

			AssertEquals(3, network.Entities.Count);
			var buffer = dependency.GetBuffer();
			var relationships = ((IProposedNetworkEntity)buffer).Links.Cast<BMNCNAttachment>().ToArray();

			var refreshed = false;
			network.Refreshed += (s, e) => refreshed = true;

			network.HideRelationship(relationships.First());
			AssertEquals(true, refreshed);
			AssertEquals("This arrow cannot be removed independently from the connected buffer. Would you like to remove the buffer?", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals(true, buffer.IsDeleted);
			AssertEquals(true, relationships[0].IsDeleted);
			AssertEquals(true, relationships[1].IsDeleted);
		}

		public void TestHideRelationship_BetweenApprovedShapes()
		{
			var resource = CreateStaffInCurrentBranchDept("WTS", "Wonko the Sane");
			Factory.Save();

			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var approvedChildShape1 = networkViewModel.CreateNewShape(diagram);
			var approvedChildShape2 = networkViewModel.CreateNewShape(diagram);

			var arrow1 = network.CreateRelationship(approvedChildShape1, approvedChildShape2).AsAttachment();
			network.SwitchToScaled();

			using (Env.SetTemporaryUserContext(resource.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				networkViewModel.ToggleApproval();
			}

			var nonApprovedChildShape = networkViewModel.CreateNewShape(diagram);

			var arrow2 = network.CreateRelationship(approvedChildShape1, nonApprovedChildShape).AsAttachment();
			var arrow3 = network.CreateRelationship(nonApprovedChildShape, approvedChildShape2).AsAttachment();

			AssertEquals(true, network.HideRelationship(arrow3));
			AssertEquals(true, arrow3.IsDeleted);
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals(true, network.HideRelationship(arrow2));
			AssertEquals(true, arrow2.IsDeleted);
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals(false, network.HideRelationship(arrow1));
			AssertEquals(false, arrow1.IsDeleted);
			AssertEquals("This arrow cannot be hidden as has been approved by [Wonko the Sane].", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region Hidden Entities
		[GuiTest]
		public void TestHiddenEntities_AddedToCorrectParent()
		{
			var system = CreateSystem(new[] { "ORG" });
			var jobHeader = CreateJobHeader<OrgHeader>();
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var workflow1 = CreateWorkflow(jobHeader1, "workflow1");
			var diagramShape = CreateDiagram(jobHeader);

			Factory.Save();

			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();

			var subDiagram = CreateShape(jobHeader1, diagramShape);
			var subShape = CreateShape(workflow1, subDiagram);
			var unrelelated = CreateShape(subDiagram);
			network.LinkEntity(unrelelated, ModuleIDs.ProcessHeader);

			var freeWorkflow = CreateWorkflow(subDiagram.ProcessJobHeader, "Tasty, pasty, milky");

			Factory.Save();

			subDiagram.AsEntity(network).HiddenEntities.Reload();
			AssertCollectionContains(freeWorkflow, subDiagram.AsEntity(network).HiddenEntities);
			AssertCollectionNotContains(subShape.ProcessHeader, subDiagram.AsEntity(network).HiddenEntities);
			AssertCollectionNotContains(freeWorkflow, network.DiagramEntity.HiddenEntities);
			AssertCollectionNotContains(subShape.ProcessHeader, network.DiagramEntity.HiddenEntities);

			var subHeader = subShape.ProcessHeader;
			network.HideEntity(subShape);

			AssertCollectionContains(freeWorkflow, subDiagram.AsEntity(network).HiddenEntities);
			AssertCollectionContains(subHeader, subDiagram.AsEntity(network).HiddenEntities);
			AssertCollectionNotContains(freeWorkflow, network.DiagramEntity.HiddenEntities);
			AssertCollectionNotContains(subHeader, network.DiagramEntity.HiddenEntities);
		}

		public void TestHiddenEntities()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "workflow1";
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "workflow2";
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_CompletionStatement = "workflow3";

			Factory.Save();

			var diagramShape = CreateDiagram(jobHeader);
			var workflow1Shape = CreateShape(workflow1, diagramShape);

			var network = CreateNetwork(diagramShape);
			AssertEquals(1, network.Entities.Count);
			AssertEquals(2, network.Entities.GetInstance(diagramShape).HiddenEntities.Count);
			AssertCollectionContains(workflow2, network.Entities.GetInstance(diagramShape).HiddenEntities);
			AssertCollectionContains(workflow3, network.Entities.GetInstance(diagramShape).HiddenEntities);

			var workflow2Shape = network.ShowEntity(workflow2, diagramShape).Single().AsShape();

			AssertNotNull(workflow2Shape);
			AssertEquals(2, network.Entities.Count);
			AssertEquals(1, network.Entities.GetInstance(diagramShape).HiddenEntities.Count);

			AssertCollectionContains(workflow1Shape, network.Shapes);
			AssertCollectionContains(workflow2Shape, network.Shapes);
			AssertEquals(workflow3, network.Entities.GetInstance(diagramShape).HiddenEntities[0]);
		}

		public void TestHiddenEntities_ShouldIncludeChildJobs()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var workflow1_1 = jobHeader1.ProcessHeaders[0];
			workflow1_1.FH_CompletionStatement = "workflow1_1";
			var workflow1_2 = jobHeader1.ProcessHeaders.AddNew();
			workflow1_2.FH_CompletionStatement = "workflow1_2";

			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var workflow2_1 = jobHeader2.ProcessHeaders[0];
			workflow2_1.FH_CompletionStatement = "workflow2_1";

			jobHeader2.GetOrCreateLinkToParent(jobHeader1);

			Factory.Save();

			AssertEquals(true, jobHeader1.IsParentOf(jobHeader2));
			AssertEquals(true, jobHeader2.IsChildOf(jobHeader1));

			var diagramShape = CreateDiagram(jobHeader1);
			var workflow1Shape = CreateShape(workflow1_1, diagramShape);

			var network = CreateNetwork(diagramShape);
			AssertEquals(1, network.Entities.Count);
			AssertEquals(2, network.Entities.GetInstance(diagramShape).HiddenEntities.Count);
			AssertCollectionContains(workflow1_2, network.Entities.GetInstance(diagramShape).HiddenEntities);
			AssertCollectionContains(jobHeader2, network.Entities.GetInstance(diagramShape).HiddenEntities);

			var shownShapes = network.ShowEntity(jobHeader2, diagramShape).Select(e => e.AsShape()).ToArray();
			AssertEquals(1, shownShapes.Length);
			var jobHeader2Shape = shownShapes[0];

			AssertNotNull(jobHeader2Shape);
			AssertEquals(2, network.Entities.Count);
			AssertEquals(1, network.Entities.GetInstance(diagramShape).HiddenEntities.Count);
			AssertEquals(1, network.Entities.GetInstance(jobHeader2Shape).HiddenEntities.Count);

			AssertCollectionContains(workflow1Shape, network.Shapes);
			AssertCollectionContains(jobHeader2Shape, network.Shapes);
			AssertEquals(workflow1_2, network.Entities.GetInstance(diagramShape).HiddenEntities[0]);
		}

		public void TestHiddenEntities_WhenChangingDiagramJob()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "workflow1";
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "workflow2";
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_CompletionStatement = "workflow3";

			Factory.Save();

			var diagramShape = Factory.New<BMNCNShape>();
			diagramShape.BNS_ShapeType = ShapeTypeList.Codes.Diagram;

			var network = CreateNetwork(diagramShape);
			AssertEquals(0, network.Entities.Count);
			AssertEquals(0, network.Entities.GetInstance(diagramShape).HiddenEntities.Count);

			diagramShape.BNS_RelatedEntityID = jobHeader.PK;

			AssertEquals(0, network.Entities.Count);
			AssertEquals(3, network.Entities.GetInstance(diagramShape).HiddenEntities.Count);
		}

		#endregion

		#region Hidden Relationships

		public void TestHiddenRelationships()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "workflow1";
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "workflow2";
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_CompletionStatement = "workflow3";

			var link1_2 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link2_3 = workflow2.GetOrCreateDependencyLink(workflow3);

			var diagramShape = CreateDiagram(jobHeader);
			var workflow1Shape = CreateShape(workflow1, diagramShape);
			var workflow2Shape = CreateShape(workflow2, diagramShape);
			var workflow3Shape = CreateShape(workflow3, diagramShape);

			var link1_2Attachment = CreateDependencyAttachment(diagramShape, link1_2, workflow1Shape, workflow2Shape);

			var network = CreateNetwork(diagramShape);

			AssertEquals(0, ((IProposedNetworkEntity)workflow2Shape).PostRequisiteLinks.Count());
			AssertEquals(1, network.Entities.GetInstance(diagramShape).HiddenRelationships.Count);
			AssertEquals(link2_3, network.Entities.GetInstance(diagramShape).HiddenRelationships[0]);

			var link2_3Attachment = network.ShowRelationship(workflow2Shape, workflow3Shape).AsAttachment();

			AssertNotNull(link2_3Attachment);
			AssertEquals(link2_3.PK, link2_3Attachment.BNA_FP_ProcessHeaderLink);
			AssertEquals(diagramShape.PK, link2_3Attachment.BNA_BNS_Owner);

			AssertEquals(1, ((IProposedNetworkEntity)workflow2Shape).PostRequisiteLinks.Count());
			AssertEquals(0, network.Entities.GetInstance(diagramShape).HiddenRelationships.Count);
		}

		public void TestHiddenRelationships_SubDiagram()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader2.ProcessHeaders[0];
			var workflow2 = jobHeader2.ProcessHeaders.AddNew();
			var workflow3 = jobHeader2.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "workflow1";
			workflow2.FH_CompletionStatement = "workflow2";
			workflow3.FH_CompletionStatement = "workflow3";

			var link1_2 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link2_3 = workflow2.GetOrCreateDependencyLink(workflow3);

			var diagram = CreateDiagram(jobHeader1);
			var subDiagram = CreateShape(jobHeader2, diagram);
			var workflow1Shape = CreateShape(workflow1, subDiagram);
			var workflow2Shape = CreateShape(workflow2, subDiagram);

			var network = CreateNetwork(diagram);
			AssertEquals(3, network.Entities.Count);
			AssertEquals(1, network.Entities.GetInstance(subDiagram).HiddenRelationships.Count);

			var attachment = network.ShowRelationship(workflow1Shape, workflow2Shape).AsAttachment();
			AssertEquals(0, network.Entities.GetInstance(subDiagram).HiddenRelationships.Count);

			AssertEquals(link1_2, attachment.ProcessHeaderLink);
			AssertEquals(workflow1Shape.PK, attachment.BNA_BNS_FromShape);
			AssertEquals(workflow2Shape.PK, attachment.BNA_BNS_ToShape);
			AssertEquals(subDiagram.PK, attachment.BNA_BNS_Owner);
		}

		public void TestHiddenRelationships_WhenChangingDiagramJob()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "workflow1";
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "workflow2";
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_CompletionStatement = "workflow3";

			var link1_2 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link2_3 = workflow2.GetOrCreateDependencyLink(workflow3);

			var diagramShape = Factory.New<BMNCNShape>();
			diagramShape.BNS_ShapeType = ShapeTypeList.Codes.Diagram;

			var network = CreateNetwork(diagramShape);
			AssertEquals(0, network.Entities.GetInstance(diagramShape).HiddenRelationships.Count);

			var workflow1Shape = CreateShape(workflow1, diagramShape);
			var workflow2Shape = CreateShape(workflow2, diagramShape);

			diagramShape.BNS_RelatedEntityID = jobHeader.PK;

			AssertEquals("Should contain a hidden relationship for the link between shapes actually on the diagram", 1, network.Entities.GetInstance(diagramShape).HiddenRelationships.Count);
			AssertEquals(link1_2, network.Entities.GetInstance(diagramShape).HiddenRelationships[0]);
		}

		public void TestHiddenRelationships_BetweenEntitiesAtDifferentHierarchies()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var jobHeader3 = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader2.ProcessHeaders[0];
			var workflow2 = jobHeader3.ProcessHeaders[0];

			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			var diagram = CreateDiagram(jobHeader1);
			var subDiagram1 = CreateShape(jobHeader2, diagram);
			var subDiagram2 = CreateShape(jobHeader3, diagram);
			var workflow1Shape = CreateShape(workflow1, subDiagram1);
			var workflow2Shape = CreateShape(workflow2, subDiagram2);

			var network = CreateNetwork(diagram);
			AssertEquals(4, network.Entities.Count);

			AssertEquals(1, network.Entities.GetInstance(subDiagram1).HiddenRelationships.Count);
			AssertEquals(1, network.Entities.GetInstance(subDiagram2).HiddenRelationships.Count);

			var attachment = network.ShowRelationship(workflow1Shape, workflow2Shape);

			AssertEquals(0, network.Entities.GetInstance(subDiagram1).HiddenRelationships.Count);
			AssertEquals(0, network.Entities.GetInstance(subDiagram2).HiddenRelationships.Count);
		}

		public void TestShowRelationship_WhenRelationshipWouldCauseCircularDependency()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

			var diagram = CreateDiagram(jobHeader);
			var shape1 = CreateShape(workflow1, diagram, "shape1");
			var shape2 = CreateShape(workflow2, diagram, "shape2");

			var dependencyLink = workflow1.GetOrCreateDependencyLink(workflow2);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var nonLinkedShape1 = networkViewModel.CreateNewShape(diagram);
			var nonLinkedShape2 = networkViewModel.CreateNewShape(diagram);

			network.CreateRelationship(shape2, nonLinkedShape1);
			network.CreateRelationship(nonLinkedShape1, nonLinkedShape2);
			network.CreateRelationship(nonLinkedShape2, shape1);

			// shape2 -> nonLinkedShape1 -> nonLinkedShape2 -> shape1
			// There is a hidden dependency from shape1 -> shape2 via their ProcessHeaders. We're about to show it which should not cause the program to crash.

			var graph = new NetworkDependencyGraph(network);
			AssertEquals("Graph would be circular, but there is no visible link between subDiagram1 and subDiagram2 yet", true, graph.IsDirectedAcyclicGraph());

			AssertNull(network.ShowRelationship(shape1, shape2));
			AssertEquals("Creating this arrow would cause a circular dependency on this diagram.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(0, shape1.PostrequisiteLinks(new BMNCNShapeDescendantsStrategy()).Count());
			AssertEquals(0, shape2.PrerequisiteLinks(new BMNCNShapeDescendantsStrategy()).Count());

			AssertEquals("Pre-req depth goes as far as shape2", 1, nonLinkedShape1.Shape.PreRequisiteDepth);
			AssertEquals("Pre-req depth goes through nonLinkedShape1 and shape2", 2, nonLinkedShape2.Shape.PreRequisiteDepth);
			AssertEquals("Pre-req depth goes through nonLinkedShape2, nonLinkedShape1 and shape2", 3, shape1.PreRequisiteDepth);
			AssertEquals("shape2 has no pre-requisites", 0, shape2.PreRequisiteDepth);
		}

		#endregion

		#region DeleteEntity

		public void TestDeleteEntity()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var diagram = jobHeader.GetDefaultDiagram();
			var defaultShape2 = workflow2.GetDefaultShape(diagram);

			var network = CreateNetwork(diagram);
			AssertEquals(2, network.Entities.Count);

			network.DeleteEntity(defaultShape2);

			AssertEquals(1, network.Entities.Count);
			AssertEquals(true, workflow2.IsDeleted);
			AssertEquals(true, defaultShape2.IsDeleted);
			AssertEquals(workflow1.GetDefaultShape(diagram), network.Shapes[0]);
		}

		public void TestDeleteEntity_ShouldNotExecuteForDiagramWithJobHeader_FoolProof()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var diagram = jobHeader.GetDefaultDiagram();

			var network = CreateNetwork(diagram);
			AssertEquals("Precondition", 2, network.Entities.Count);

			AssertEquals(false, network.DeleteEntity(diagram));

			AssertEquals(2, network.Entities.Count);
			AssertEquals(false, diagram.IsDeleted);
			AssertEquals(false, jobHeader.IsDeleted);
			AssertEquals(false, workflow1.IsDeleted);
			AssertEquals(false, workflow2.IsDeleted);

			AssertEquals("Should not try to remove the diagram node.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestDeleteEntity_ShouldNotExecuteForDiagramWithNoJobHeader_FoolProof()
		{
			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);

			AssertEquals("Precondition", 2, network.Entities.Count);

			AssertEquals(false, network.DeleteEntity(diagram));

			AssertEquals(2, network.Entities.Count);
			AssertEquals(false, diagram.IsDeleted);
			AssertEquals(false, childShape1.IsDeleted);
			AssertEquals(false, childShape2.IsDeleted);

			AssertEquals("Should not try to remove the diagram node.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestDeleteEntity_BufferShape()
		{
			var diagram = CreateDiagram(Factory, isScaled: true);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			AssertEquals(0, network.Entities.Count);

			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);
			childShape1.Name = "childShape1";
			childShape2.Name = "childShape2";

			AssertEquals(2, network.Entities.Count);

			var dependency = network.CreateRelationship(childShape1, childShape2).AsAttachment();
			AssertNull(dependency.GetBuffer());

			var action = new AddBufferAction(networkViewModel);
			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, childShape1))
			{
				action.GetChildActions().Single().AsJobNetworkAction().Execute();
			}

			AssertEquals(3, network.Entities.Count);
			var buffer = dependency.GetBuffer();
			var relationships = ((IProposedNetworkEntity)buffer).Links.Cast<BMNCNAttachment>().ToArray();

			network.DeleteEntity(buffer);

			AssertEquals(true, buffer.IsDeleted);
			AssertEquals(true, relationships[0].IsDeleted);
			AssertEquals(true, relationships[1].IsDeleted);
		}

		public void TestDeleteEntity_NoProcessHeader()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var network = CreateNetwork(jobHeader.GetDefaultDiagram());

			var shape = CreateShape(network.DiagramEntity);

			AssertEquals(true, network.DeleteEntity(shape));
		}

		public void TestDeleteWorkflow_ShouldNotRefreshNetworkAtBusinessLevel()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var diagram = jobHeader.GetDefaultDiagram();
			var network = CreateNetwork(diagram);
			var refreshes = new List<RefreshType>();
			network.Refreshed += (s, e) => refreshes.Add(e.RefreshType);

			AssertEquals(2, network.Entities.Count);
			AssertCollectionContains(network.Shapes, e => e.PK == workflow1.GetDefaultShape(diagram).PK);
			AssertCollectionContains(network.Shapes, e => e.PK == workflow2.GetDefaultShape(diagram).PK);

			AssertEquals(0, refreshes.Count);
			workflow1.Delete();

			AssertEquals(1, refreshes.Count);
			AssertEquals("Should not have extra refreshes that could cause the network to redraw", RefreshType.EntitiesReloaded, refreshes[0]);

			AssertEquals(1, network.Entities.Count);
			AssertCollectionContains(network.Shapes, e => e.PK == workflow2.GetDefaultShape(diagram).PK);
		}

		public void TestDeleteRelationship_ShouldRefreshScale_UponSaving()
		{
			var diagramShape = CreateDiagram(Factory, name: "Diagram", isScaled: true);
			var network = CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;
			var shape1 = CreateShape(diagram, "shape1");
			var shape2 = CreateShape(diagram, "shape2");

			shape1.SetCoordinates(100, 100, 100, 100);
			shape2.SetCoordinates(100, 100, 200, 200);

			AssertEquals(2, network.Entities.Count);

			AssertEquals(0m, shape1.Shape.EarliestStartHours);
			AssertEquals(0m, shape2.Shape.EarliestStartHours);

			var relationship = network.CreateRelationship(shape1, shape2);
			network.Refresh(RefreshType.Saving);

			AssertEquals(0m, shape1.Shape.EarliestStartHours);
			AssertNotEquals(0m, shape2.Shape.EarliestStartHours);

			network.DeleteRelationship(relationship);
			network.Refresh(RefreshType.Saving);

			AssertEquals(0m, shape1.Shape.EarliestStartHours);
			AssertEquals(0m, shape2.Shape.EarliestStartHours);
		}

		#endregion

		#region CreateRelationship

		public void TestCreateRelationship_PromoteResourceDependency()
		{
			var diagram = CreateDiagram(CreateJobHeader<OrgHeader>());
			var shape1 = CreateShape(diagram, "Fish");
			var shape2 = CreateShape(diagram, "Chips");

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			new CreateResourceDependencyAction(networkViewModel, DependencyDirection.PostRequisite).ExecuteForShapes(shape2, shape1);

			AssertCollectionContains(shape2, shape1.PostrequisiteLinks(new BMNCNShapeDescendantsStrategy()).Cast<BMNCNAttachment>().Where(a => a.BNA_Type == AttachmentTypeList.Codes.ResourceDependency).Select(a => a.ToShape));
			AssertCollectionNotContains(shape2, shape1.PostrequisiteLinks(new BMNCNShapeDescendantsStrategy()).Cast<BMNCNAttachment>().Where(a => a.BNA_Type == AttachmentTypeList.Codes.Dependency).Select(a => a.ToShape));

			network.CreateRelationship(shape1, shape2);
			AssertEquals("A resource dependency already exists between [Fish] and [Chips]. Would you like to change this into a Dependency Arrow?", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertCollectionContains(shape2, shape1.PostrequisiteLinks(new BMNCNShapeDescendantsStrategy()).Cast<BMNCNAttachment>().Where(a => a.BNA_Type == AttachmentTypeList.Codes.Dependency).Select(a => a.ToShape));
			AssertCollectionNotContains(shape2, shape1.PostrequisiteLinks(new BMNCNShapeDescendantsStrategy()).Cast<BMNCNAttachment>().Where(a => a.BNA_Type == AttachmentTypeList.Codes.ResourceDependency).Select(a => a.ToShape));
		}

		public void TestCreateRelationship_ForAlreadyLinkedEntities_ShouldNotCreateDuplicate()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var diagram = CreateDiagram(jobHeader);
			var shape1 = CreateShape(workflow1, diagram);
			var shape2 = CreateShape(workflow2, diagram);

			var network = CreateNetwork(diagram);
			AssertEquals(2, network.Entities.Count);

			AssertEquals(0, shape1.PostRequisiteLinks.Count());
			AssertEquals(0, shape2.PreRequisiteLinks.Count());

			Factory.Save();
			var startingAttachmentCount = Factory.GetDatabaseCount(typeof(BMNCNAttachment));

			network.CreateRelationship(shape1, shape2);
			Factory.Save();
			AssertEquals(startingAttachmentCount + 1, Factory.GetDatabaseCount(typeof(BMNCNAttachment)));

			network.CreateRelationship(shape1, shape2);
			Factory.Save();
			AssertEquals(startingAttachmentCount + 1, Factory.GetDatabaseCount(typeof(BMNCNAttachment)));
		}

		public void TestCreateRelationship_ShouldCreateDependencyAndAttachment()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var diagram = jobHeader.GetDefaultDiagram();
			var network = CreateNetwork(diagram);
			AssertEquals(2, network.Entities.Count);

			AssertEquals(0, workflow1.GetDefaultShape(diagram).PostRequisiteLinks.Count());
			AssertEquals(0, workflow2.GetDefaultShape(diagram).PreRequisiteLinks.Count());

			network.CreateRelationship(workflow1.GetDefaultShape(diagram), workflow2.GetDefaultShape(diagram));

			AssertEquals(1, workflow1.GetDefaultShape(diagram).PostRequisiteLinks.Count());
			AssertEquals(1, workflow2.GetDefaultShape(diagram).PreRequisiteLinks.Count());

			AssertEquals(1, workflow1.LinksFromMeToOthers.Count());
			AssertEquals(1, workflow2.LinksFromOthersToMe.Count());

			var link = workflow1.LinksFromMeToOthers.Single();
			AssertEquals(ProcessHeaderLinkTypeList.Codes.Dependency, link.FP_LinkType);
		}

		public void TestCreateRelationship_LinkAlreadyExistsButShapeDoesNot()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			workflow1.MakePrerequisiteOf(workflow2);

			var diagram = jobHeader.GetDefaultDiagram();
			var network = CreateNetwork(diagram);
			AssertEquals(2, network.Entities.Count);

			AssertEquals(1, workflow1.GetDefaultShape(diagram).AsEntity(network).PostRequisiteLinks.Count());
			AssertEquals(1, workflow2.GetDefaultShape(diagram).AsEntity(network).PreRequisiteLinks.Count());

			AssertEquals(1, workflow1.LinksFromMeToOthers.Count());
			AssertEquals(1, workflow2.LinksFromOthersToMe.Count());
		}

		public void TestCreateRelationship_EntitiesWithinSameSubDiagram_OwnerShouldBeSubDiagram()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader2.ProcessHeaders[0];
			var workflow2 = jobHeader2.ProcessHeaders.AddNew();

			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			var diagram = CreateDiagram(jobHeader1);
			var subDiagram = CreateShape(jobHeader2, diagram);
			var workflow1Shape = CreateShape(workflow1, subDiagram);
			var workflow2Shape = CreateShape(workflow2, subDiagram);

			var network = CreateNetwork(diagram);
			AssertEquals(3, network.Entities.Count);

			network.CreateRelationship(workflow1Shape, workflow2Shape);
			AssertDiagramHasDependencyAttachment(subDiagram, link, workflow1Shape, workflow2Shape);
		}

		public void TestCreateRelationship_BetweenEntitiesOnAdjacentDiagramLevels_OwnerShouldBeOuterDiagram()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var jobHeader3 = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader2.ProcessHeaders[0];
			var workflow2 = jobHeader3.ProcessHeaders[0];

			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			var diagram = CreateDiagram(jobHeader1);
			var subDiagram = CreateShape(jobHeader2, diagram);
			var subSubDiagram = CreateShape(jobHeader3, subDiagram);
			var workflow1Shape = CreateShape(workflow1, subDiagram);
			var workflow2Shape = CreateShape(workflow2, subSubDiagram);

			var network = CreateNetwork(diagram);
			AssertEquals(4, network.Entities.Count);

			network.CreateRelationship(workflow1Shape, workflow2Shape);
			AssertDiagramHasDependencyAttachment(subDiagram, link, workflow1Shape, workflow2Shape);
		}

		public void TestCreateRelationship_BetweenEntitiesOnTwoDiagramLevelsApart_OwnerShouldBeOuterDiagram()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var jobHeader3 = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader1.ProcessHeaders[0];
			var workflow2 = jobHeader3.ProcessHeaders[0];

			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			var diagram = CreateDiagram(jobHeader1);
			var subDiagram = CreateShape(jobHeader2, diagram);
			var subSubDiagram = CreateShape(jobHeader3, subDiagram);
			var workflow1Shape = CreateShape(workflow1, diagram);
			var workflow2Shape = CreateShape(workflow2, subSubDiagram);

			var network = CreateNetwork(diagram);
			AssertEquals(4, network.Entities.Count);

			network.CreateRelationship(workflow1Shape, workflow2Shape);
			AssertDiagramHasDependencyAttachment(diagram, link, workflow1Shape, workflow2Shape);
		}

		public void TestCreateRelationship_BetweenEntitiesOnTwoDiagramWithDifferentHierarchies_OwnerShouldBeFirstDiagramWithSharedParentage()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var jobHeader3 = CreateJobHeader<OrgHeader>();
			var jobHeader4 = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader3.ProcessHeaders[0];
			var workflow2 = jobHeader4.ProcessHeaders[0];

			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			var diagram = CreateDiagram(jobHeader1);
			var subDiagram = CreateShape(jobHeader2, diagram);
			var subSubDiagram1 = CreateShape(jobHeader3, subDiagram);
			var subSubDiagram2 = CreateShape(jobHeader4, subDiagram);
			var workflow1Shape = CreateShape(workflow1, subSubDiagram1);
			var workflow2Shape = CreateShape(workflow2, subSubDiagram2);

			var network = CreateNetwork(diagram);
			AssertEquals(5, network.Entities.Count);

			network.CreateRelationship(workflow1Shape, workflow2Shape);
			AssertDiagramHasDependencyAttachment(subDiagram, link, workflow1Shape, workflow2Shape);
		}

		public void TestCreateRelationship_ShouldValidateShapes()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var child1 = networkViewModel.CreateNewShape(diagram);
			var child2 = networkViewModel.CreateNewShape(diagram);

			network.SwitchToScaled();

			AssertNoWarnings(child1);
			AssertNoWarnings(child2);

			var arrow = (NetworkAttachment)network.CreateRelationship(child1, child2);
			arrow.Validation.ValidateAll();
			AssertHasRowWarning(arrow, "The post-requisite is scheduled to begin before its pre-requisite is scheduled to complete.");
		}

		public void TestCreateRelationship_CausingCircularDependency_MixingParentChildAndDependentRelationships()
		{
			//		parent
			//			↑
			//		other
			//			↑
			//		child
			var jobHeader1 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var parentWorkflow = jobHeader1.ProcessHeaders.AddNew();
			var childWorkflow = jobHeader1.ProcessHeaders.AddNew();

			var jobHeader2 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var otherWorkflow = jobHeader2.ProcessHeaders.AddNew();

			childWorkflow.GetOrCreateLinkToParent(otherWorkflow);
			otherWorkflow.GetOrCreateLinkToParent(parentWorkflow);

			Assert("GIVEN child is child-of other", childWorkflow.IsChildOf(otherWorkflow));
			Assert("GIVEN other is child-of parent", otherWorkflow.IsChildOf(parentWorkflow));

			var diagram = CreateDiagram(jobHeader1);
			var parentShape = CreateShape(parentWorkflow, diagram, "parentShape");
			var childShape = CreateShape(childWorkflow, diagram, "childShape");
			var network = CreateNetwork(diagram);

			// parent -> child
			var arrow = network.CreateRelationship(parentShape, childShape).AsAttachment();

			AssertNull("WHEN linking parent as pre-req of child in NCN THEN should return null (not created)", arrow);
			AssertEquals(
				"THEN should show error message",
				@"Error - FP_FH_HeaderFrom: Invalid relationship. These workflows are already in a parent/child relationship and cannot also be in a pre/post requisite relationship. Please note that this may be an indirect relationship to the current workflow.
Error - FP_FH_HeaderTo: Invalid relationship. These workflows are already in a parent/child relationship and cannot also be in a pre/post requisite relationship. Please note that this may be an indirect relationship to the current workflow.",
				UnitTestUserNotification.Instance.LastMessage.Text);

			Factory.Save();
			AssertEquals("WHEN saving invalid link THEN prerequisite link should not be created", false, parentWorkflow.IsPrerequisiteOf(childWorkflow));
			AssertEquals("THEN arrow should not be saved", 0, childShape.PreRequisiteLinks.Count());

			//even if a child workflow is already on the diagram, we allow parent workflow to be on the diagram,
			//and to be linked to the shape. But should be no arrow(dependency link) between parent and child and vice versa."
			var anotherShape = CreateShape(otherWorkflow, diagram, "other shape");

			arrow = network.CreateRelationship(childShape, anotherShape).AsAttachment();

			AssertNull("WHEN linking parent as post-req of child in NCN THEN should return null (not created)", arrow);
			AssertEquals(
				"THEN should show error message",
				@"Error - FP_FH_HeaderFrom: Invalid relationship. These workflows are already in a parent/child relationship and cannot also be in a pre/post requisite relationship. Please note that this may be an indirect relationship to the current workflow.
Error - FP_FH_HeaderTo: Invalid relationship. These workflows are already in a parent/child relationship and cannot also be in a pre/post requisite relationship. Please note that this may be an indirect relationship to the current workflow.",
				UnitTestUserNotification.Instance.LastMessage.Text);

			Factory.Save();
			AssertEquals("WHEN saving invalid link THEN prerequisite link should not be created", false, childWorkflow.IsPrerequisiteOf(otherWorkflow));
			AssertEquals("THEN arrow should not be saved", 0, anotherShape.PreRequisiteLinks.Count());

			arrow = network.CreateRelationship(anotherShape, parentShape).AsAttachment();

			AssertNull("WHEN draw an arrow from shape with 'child' workflow to shape with 'parent' workflow THEN should return null (not created)", arrow);
			AssertEquals(
				"THEN should show error message",
				@"Error - FP_FH_HeaderFrom: Invalid relationship. These workflows are already in a parent/child relationship and cannot also be in a pre/post requisite relationship. Please note that this may be an indirect relationship to the current workflow.
Error - FP_FH_HeaderTo: Invalid relationship. These workflows are already in a parent/child relationship and cannot also be in a pre/post requisite relationship. Please note that this may be an indirect relationship to the current workflow.",
				UnitTestUserNotification.Instance.LastMessage.Text);

			Factory.Save();
			AssertEquals("WHEN saving invalid link THEN prerequisite link should not be created", false, otherWorkflow.IsPrerequisiteOf(parentWorkflow));
			AssertEquals("THEN arrow should not be saved", 0, parentShape.PreRequisiteLinks.Count());
		}

		public void TestCreateRelationship_CausingCircularDependency()
		{
			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var shape1 = networkViewModel.CreateNewShape(diagram);
			var shape2 = networkViewModel.CreateNewShape(diagram);
			var shape3 = networkViewModel.CreateNewShape(diagram);

			shape1.Name = "shape1";
			shape2.Name = "shape2";
			shape3.Name = "shape3";

			AssertNotNull(network.CreateRelationship(shape1, shape2));
			AssertNotNull(network.CreateRelationship(shape2, shape3));
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			AssertNull(network.CreateRelationship(shape3, shape1));
			AssertEquals("Creating this arrow would cause a circular dependency on this diagram.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(0, shape3.PostRequisiteLinks.Count());
			AssertEquals(0, shape1.PreRequisiteLinks.Count());

			AssertEquals(0, shape1.Shape.PreRequisiteDepth);
			AssertEquals(1, shape2.Shape.PreRequisiteDepth);
			AssertEquals(2, shape3.Shape.PreRequisiteDepth);
		}

		public void TestCreateRelationship_CausingCircularDependency_WhenAttachmentValidationSuppressed()
		{
			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var shape1 = networkViewModel.CreateNewShape(diagram);
			var shape2 = networkViewModel.CreateNewShape(diagram);
			var shape3 = networkViewModel.CreateNewShape(diagram);

			shape1.Name = "shape1";
			shape2.Name = "shape2";
			shape3.Name = "shape3";

			AssertNotNull(network.CreateRelationship(shape1, shape2));
			AssertNotNull(network.CreateRelationship(shape2, shape3));
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			AssertNotNull(network.CreateRelationship(shape3, shape1, ownerShape: diagram, useAttachementValidation: false));
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(1, shape3.PostRequisiteLinks.Count());
			AssertEquals(1, shape1.PreRequisiteLinks.Count());

			AssertEquals(-1, shape1.Shape.PreRequisiteDepth);
			AssertEquals(-1, shape2.Shape.PreRequisiteDepth);
			AssertEquals(-1, shape3.Shape.PreRequisiteDepth);
		}

		public void TestCreateRelationship_CausingCircularDependency_ImplicitDependencyFromParent()
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var subDiagram = networkViewModel.CreateNewShape(diagram);
			var shape1 = networkViewModel.CreateNewShape(subDiagram);
			var shape2 = networkViewModel.CreateNewShape(diagram);

			shape1.Name = "shape1";
			shape2.Name = "shape2";

			AssertNotNull(network.CreateRelationship(shape1, shape2));
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			AssertNull(network.CreateRelationship(shape2, subDiagram));
			AssertEquals("Creating this arrow would cause a circular dependency on this diagram.", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals(0, shape1.PreRequisiteLinks.Count());
			AssertEquals(0, shape1.Shape.PreRequisiteDepth);
			AssertEquals(1, shape2.Shape.PreRequisiteDepth);
		}

		public void TestCreateRelationship_CausingCircularDependency_ProcessHeaders()
		{
			BMSRegistry.Instance.AutomaticallyValidateWorkflowLoopsOnSave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
			var diagram1 = CreateDiagram(CreateJobHeader<OrgHeader>());

			var networkViewModel1 = CreateNetworkViewModel(diagram1);
			var network1 = networkViewModel1.GetJobNetwork();
			var shape1 = networkViewModel1.CreateNewWorkflow(diagram1);
			var shape2 = networkViewModel1.CreateNewWorkflow(diagram1);

			shape1.ProcessHeader.FH_CompletionStatement = "Workflow 1";
			shape2.ProcessHeader.FH_CompletionStatement = "Workflow 2";

			shape1.ProcessHeader.MakePrerequisiteOf(shape2.ProcessHeader);

			Assert(!shape2.HasErrors);

			network1.CreateRelationship(shape2, shape1);

			Assert(!shape2.HasErrors);

			network1.DiagramEntity.RunPreSaveValidation();

			Assert(shape2.HasErrors);

			var attachment = shape2.Attachments.Single();
			var errorMessage = @"This link is part of a looped dependency. The following workflows are involved in a loop:
Organization (XVBQP68SIYXQ) - Workflow 1
Organization (XVBQP68SIYXQ) - Workflow 2";
			AssertHasError(attachment.ProcessHeaderLink.FP_FH_HeaderFromInfo, errorMessage);
			AssertHasError(attachment.ProcessHeaderLink.FP_FH_HeaderToInfo, errorMessage);

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestCreateRelationship_WithAttachmentsNotAppearingAsArrowsOnDiagram_ShouldValidateAllIncludedEntities()
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
			var diagram1 = CreateDiagram(CreateJobHeader<OrgHeader>());
			var diagram2 = CreateDiagram(CreateJobHeader<OrgHeader>());

			var networkViewModel1 = CreateNetworkViewModel(diagram1);
			var network1 = networkViewModel1.GetJobNetwork();
			var networkViewModel2 = CreateNetworkViewModel(diagram2);
			var network2 = networkViewModel2.GetJobNetwork();

			var shape1 = networkViewModel1.CreateNewWorkflow(diagram1);
			var shape2 = networkViewModel1.CreateNewWorkflow(diagram1);
			var shapeOnDiagram2 = networkViewModel2.CreateNewWorkflow(diagram2);

			var attachment = Factory.New<BMNCNAttachment>();
			attachment.BNA_BNS_FromShape = shapeOnDiagram2.PK;
			attachment.BNA_BNS_ToShape = shape1.PK;
			attachment.BNA_BNS_Owner = diagram1.PK;
			attachment.BNA_Type = AttachmentTypeList.Codes.Dependency;

			AssertNoExceptionThrown(() => network1.CreateRelationship(shape1, shape2));
			AssertCollectionContains(diagram1.ChildDependencyAttachments, x => x.BNA_BNS_FromShape == shape1.PK && x.BNA_BNS_ToShape == shape2.PK);
		}

		public void TestCreateRelationship_FromParentToChild()
		{
			var diagram = CreateDiagram(CreateJobHeader<OrgHeader>());
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var subDiagram = CreateShape(CreateJobHeader<OrgHeader>(), network.DiagramEntity, name: "subDiagram");
			networkViewModel.Refresh();

			var subDiagramChild = networkViewModel.CreateNewShape(subDiagram);
			subDiagramChild.Name = "subDiagramChild";

			AssertNull(network.CreateRelationship(subDiagram, subDiagramChild));
			AssertEquals("Cannot create an arrow between a parent and its child.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(0, subDiagram.PostRequisiteLinks.Count());
			AssertEquals(0, subDiagramChild.PreRequisiteLinks.Count());
		}

		public void TestCreateRelationship_FromChildToParent()
		{
			var diagram = CreateDiagram(CreateJobHeader<OrgHeader>());
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var subDiagram = CreateShape(CreateJobHeader<OrgHeader>(), network.DiagramEntity, name: "subDiagram");
			networkViewModel.Refresh();

			var subDiagramChild = networkViewModel.CreateNewShape(subDiagram);
			subDiagramChild.Name = "subDiagramChild";

			AssertNull(network.CreateRelationship(subDiagramChild, subDiagram));
			AssertEquals("Cannot create an arrow between a parent and its child.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(0, subDiagramChild.PostRequisiteLinks.Count());
			AssertEquals(0, subDiagram.PreRequisiteLinks.Count());
		}

		public void TestCreateRelationship_FromGrandParentToChild()
		{
			var diagram = CreateDiagram(CreateJobHeader<OrgHeader>());
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var subDiagram = CreateShape(CreateJobHeader<OrgHeader>(), network.DiagramEntity, name: "subDiagram");
			var subSubDiagram = CreateShape(CreateJobHeader<OrgHeader>(), subDiagram, name: "subSubDiagram");
			networkViewModel.Refresh();

			var subSubDiagramChild = networkViewModel.CreateNewShape(subSubDiagram);
			subSubDiagramChild.Name = "subSubDiagramChild";

			AssertNull(network.CreateRelationship(subDiagram, subSubDiagramChild));
			AssertEquals("Cannot create an arrow between a parent and its child.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(0, subDiagram.PostRequisiteLinks.Count());
			AssertEquals(0, subSubDiagramChild.PostRequisiteLinks.Count());
		}

		public void TestCreateRelationship_FromChildToParentGrand()
		{
			var diagram = CreateDiagram(CreateJobHeader<OrgHeader>());
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var subDiagram = CreateShape(CreateJobHeader<OrgHeader>(), network.DiagramEntity, name: "subDiagram");
			var subSubDiagram = CreateShape(CreateJobHeader<OrgHeader>(), subDiagram, name: "subSubDiagram");
			networkViewModel.Refresh();

			var subSubDiagramChild = networkViewModel.CreateNewShape(subSubDiagram);
			subSubDiagramChild.Name = "subSubDiagramChild";

			AssertNull(network.CreateRelationship(subSubDiagramChild, subDiagram));
			AssertEquals("Cannot create an arrow between a parent and its child.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(0, subSubDiagramChild.PostRequisiteLinks.Count());
			AssertEquals(0, subDiagram.PreRequisiteLinks.Count());
		}

		public void TestCreateRelationship_DecoupledArrowExists_ShouldUnDecouple()
		{
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var link1_2 = workflow1.GetOrCreateDependencyLink(workflow2);

			var diagram = CreateDiagram(jobHeader);
			var shape1 = CreateShape(workflow1, diagram, "shape1");
			var shape2 = CreateShape(workflow2, diagram, "shape2");
			var arrow1_2 = CreateDependencyAttachment(diagram, link1_2, shape1, shape2);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			networkViewModel.ToggleApproval();

			arrow1_2.Decouple();

			AssertEquals(false, arrow1_2.IsDeleted);
			AssertEquals(true, arrow1_2.BNA_IsDecouple);
			AssertEquals(ArrowAppearance.Dotted, arrow1_2.AsEntity(network).Appearance);

			AssertNull(arrow1_2.ProcessHeaderLink);
			AssertEquals(true, link1_2.IsDeleted);

			AssertEquals(0, workflow1.PostrequisiteLinks.Count());
			AssertEquals(0, workflow2.PrerequisiteLinks.Count());

			var arrow = network.CreateRelationship(shape1, shape2).AsAttachment();

			AssertEquals(arrow1_2, arrow);
			AssertEquals(false, arrow.BNA_IsDecouple);
			AssertEquals(ArrowAppearance.Normal, arrow1_2.AsEntity(network).Appearance);
			AssertNotNull(arrow.ProcessHeaderLink);

			AssertEquals(1, workflow1.PostrequisiteLinks.Count());
			AssertEquals(1, workflow2.PrerequisiteLinks.Count());
		}

		public void TestCreateRelationship_ForNetworkAppliedFromTemplate_ShouldNotReportValidationErrors()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateWorkflow1 = BMSTestHelper.CreateWorkflow(template, "Zoot! Review.", releaseGroupPK: config.ReleaseGroup.PK);
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template, "As a guilty mum...", releaseGroupPK: config.ReleaseGroup.PK);
			var templateWorkflow3 = BMSTestHelper.CreateWorkflow(template, "Blah blah dettol blah.", releaseGroupPK: config.ReleaseGroup.PK);

			BMSTestHelper.CreateDependencyLink(template, templateWorkflow1, templateWorkflow2);
			BMSTestHelper.CreateTask(template, templateWorkflow1, string.Empty);
			BMSTestHelper.CreateTask(template, templateWorkflow2, string.Empty);
			BMSTestHelper.CreateTask(template, templateWorkflow3, string.Empty);

			Factory.Save();

			var job = Factory.NewWithValidTestData<OrgHeader>();

			job.ApplyWorkflowTemplates();

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(job, Factory);
			var network = NetworkTestCase.CreateNetwork(jobHeader.GetDefaultDiagram());
			AssertEquals(3, network.Entities.Count);

			var shape2 = network["As a guilty mum..."];
			var shape3 = network["Blah blah dettol blah."];

			network.CreateRelationship(shape2, shape3);

			AssertNoErrors(jobHeader);
		}

		public void TestCreateRelationship_WorkflowInheritsPrerequisitesFromJob()
		{
			var prerequisiteJob = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			prerequisiteJob.FH_CompletionStatement = "prerequisiteJob";
			var workflowForPrerequisiteJob = prerequisiteJob.ProcessHeaders[0];
			workflowForPrerequisiteJob.FH_CompletionStatement = "workflowForPrerequisiteJob";

			var job2 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			job2.FH_CompletionStatement = "job2";
			var workflowForJob2 = job2.ProcessHeaders[0];
			workflowForJob2.FH_CompletionStatement = "workflowForJob2";

			var job3 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			job3.FH_CompletionStatement = "job3";
			job3.GetOrCreateLinkToParent(job2);
			var workflowForJob3 = job3.ProcessHeaders[0];
			workflowForJob3.FH_CompletionStatement = "workflowForJob3";

			var job4 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			job4.FH_CompletionStatement = "job4";
			job4.GetOrCreateLinkToParent(job3);
			var workflowForJob4 = job4.ProcessHeaders[0];
			workflowForJob4.FH_CompletionStatement = "workflowForJob4";
			Factory.Save();

			var diagramShape = CreateDiagram(Factory);
			var network = CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;

			var shapeForPrerequisiteJob = CreateShape(prerequisiteJob, diagram, "Prerequisite Job");

			var shapeForJob2 = CreateShape(job2, diagram, "Job 2");

			var shapeForJob3 = CreateShape(job3, shapeForJob2, "Job 3");
			var shapeForWorkflowForJob3 = CreateShape(workflowForJob3, shapeForJob3, "Workflow for Job 3");

			var shapeForJob4 = CreateShape(job4, shapeForJob3, "Job 4");
			var shapeForWorkflowForJob4 = CreateShape(workflowForJob4, shapeForJob3, "Workflow for Job 4");

			var arrow = shapeForPrerequisiteJob.MakeVisiblePrerequisiteOf(shapeForJob3);
			Factory.Save();

			AssertEquals(1, workflowForJob4.GetPKsOfPrerequisitesUpTheTree().Length);
			AssertEquals(1, job4.GetPKsOfPrerequisitesUpTheTree().Length);
			AssertEquals(1, job3.PrerequisiteLinks.Count());
		}

		public void TestCreateRelationship_BetweenChildShapeAndParentShape_JobLevelHeaderAndItsWorkflowAreOnDiagram()
		{
			BMSRegistry.Instance.AutomaticallyValidateWorkflowLoopsOnSave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			VisualBoardsTestHelper.CreateSystem(Factory, "ORG");

			var diagram = CreateDiagram(Factory);

			// within one job
			//
			//    job
			//     ↑
			// workflow
			//
			// we assume the normal workflow is a 'child' of a job level workflow, it means we do not create a real link in the ProcessHeaderLink table, but mean a 'virtual' link.
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job Header");
			var workflow = CreateWorkflow(jobHeader, "Workflow for jobHeader");

			var network = CreateNetwork(diagram);
			var childShape = CreateShape(diagram, name: "Child Shape");
			network.LinkEntity(childShape, workflow);
			AssertNull("No errors shown, this is a valid case.", UnitTestUserNotification.Instance.LastMessage.Text);

			var parentShape = CreateShape(diagram, "Parent Shape");
			network.LinkEntity(parentShape, jobHeader);
			AssertNull(@"No errors shown: even if a child workflow is already on the diagram, we allow parent workflow to be 
on the diagram (linked to the shape). But should be no arrow (dependency link) between them.", UnitTestUserNotification.Instance.LastMessage.Text);

			Assert(!childShape.HasErrors);
			Assert(!parentShape.HasErrors);

			network.CreateRelationship(childShape, parentShape);

			Assert(!childShape.HasErrors);
			Assert(!parentShape.HasErrors);

			var attachment = childShape.AsEntity(network).Attachments.Single();
			Assert(!attachment.ProcessHeaderLink.HasErrors);

			network.DiagramEntity.RunPreSaveValidation();

			Assert(childShape.HasErrors);
			Assert(!parentShape.HasErrors);

			var errorText = @"This is a dependency link between workflows that are also involved in a Parent-Child relationship.";
			AssertHasError(attachment.ProcessHeaderLink.FP_FH_HeaderFromInfo, errorText);
			AssertHasError(attachment.ProcessHeaderLink.FP_FH_HeaderToInfo, errorText);

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestCreateRelationship_BetweenParentShapeAndChildShape_JobLevelHeaderAndItsWorkflowAreOnDiagram()
		{
			BMSRegistry.Instance.AutomaticallyValidateWorkflowLoopsOnSave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			VisualBoardsTestHelper.CreateSystem(Factory, "ORG");

			var diagram = CreateDiagram(Factory);

			// within one job
			//
			//    job
			//     ↑
			// workflow
			//
			// we assume the normal workflow is a 'child' of a job level workflow, it means we do not create a real link in the ProcessHeaderLink table, but mean a 'virtual' link.
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job Header");
			var workflow = CreateWorkflow(jobHeader, "Workflow for jobHeader");

			var network = CreateNetwork(diagram);
			var childShape = CreateShape(diagram, name: "Child Shape");
			network.LinkEntity(childShape, workflow);
			AssertNull("No errors shown, this is a valid case.", UnitTestUserNotification.Instance.LastMessage.Text);

			var parentShape = CreateShape(diagram, "Parent Shape");
			network.LinkEntity(parentShape, jobHeader);
			AssertNull(@"No errors shown: even if a child workflow is already on the diagram, we allow parent workflow to be 
on the diagram (linked to the shape). But should be no arrow (dependency link) between them.", UnitTestUserNotification.Instance.LastMessage.Text);

			Assert(!childShape.HasErrors);
			Assert(!parentShape.HasErrors);

			network.CreateRelationship(parentShape, childShape);

			Assert(!childShape.HasErrors);
			Assert(!parentShape.HasErrors);

			var attachment = parentShape.AsEntity(network).Attachments.Single();
			Assert(!attachment.ProcessHeaderLink.HasErrors);

			network.DiagramEntity.RunPreSaveValidation();

			Assert(childShape.HasErrors);
			Assert(!parentShape.HasErrors);

			var errorText = @"This is a dependency link between workflows that are also involved in a Parent-Child relationship.";
			AssertHasError(attachment.ProcessHeaderLink.FP_FH_HeaderFromInfo, errorText);
			AssertHasError(attachment.ProcessHeaderLink.FP_FH_HeaderToInfo, errorText);

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestCreateRelationship_BetweenChildAndParent_ParentAndChildAreOnDiagram()
		{
			VisualBoardsTestHelper.CreateSystem(Factory, "ORG");

			var diagram = CreateDiagram(Factory);

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job Header 1");
			var workflow1 = CreateWorkflow(jobHeader1, "Workflow for jobHeader1");

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job Header 2");
			CreateWorkflow(jobHeader2, "Workflow for jobHeader2");

			// PCH link between two different jobs
			//
			//   jobHeader2
			//       ↑
			//   workflow1
			VisualBoardsTestHelper.MakeChildOf(workflow1, jobHeader2);

			var childShape1 = CreateShape(workflow1, diagram, "Child Shape 1");
			AssertNull("No errors shown, this is a valid case.", UnitTestUserNotification.Instance.LastMessage.Text);

			var parentShape2 = CreateShape(diagram, "Parent Shape 2");
			var network = CreateNetwork(diagram);
			network.LinkEntity(parentShape2, jobHeader2);
			AssertNull("No errors shown, we allow parent to be linked to the shape if its child linked to another shape of this diagram and vice versa", UnitTestUserNotification.Instance.LastMessage.Text);

			network.CreateRelationship(childShape1, parentShape2);

			var errorText = @"Error - FP_FH_HeaderFrom: Invalid relationship. These workflows are already in a parent/child relationship and cannot also be in a pre/post requisite relationship. Please note that this may be an indirect relationship to the current workflow.
Error - FP_FH_HeaderTo: Invalid relationship. These workflows are already in a parent/child relationship and cannot also be in a pre/post requisite relationship. Please note that this may be an indirect relationship to the current workflow.";
			AssertEquals(errorText, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestCreateRelationship_BetweenParentAndChild_ParentAndChildAreOnDiagram()
		{
			VisualBoardsTestHelper.CreateSystem(Factory, "ORG");

			var diagram = CreateDiagram(Factory);

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job Header 1");
			var workflow1 = CreateWorkflow(jobHeader1, "Workflow for jobHeader1");

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job Header 2");
			CreateWorkflow(jobHeader2, "Workflow for jobHeader2");

			// PCH link between two different jobs
			//
			//   jobHeader2
			//       ↑
			//   workflow1
			VisualBoardsTestHelper.MakeChildOf(workflow1, jobHeader2);

			var childShape1 = CreateShape(workflow1, diagram, "Child Shape 1");
			AssertNull("No errors shown, this is a valid case.", UnitTestUserNotification.Instance.LastMessage.Text);

			var parentShape2 = CreateShape(diagram, "Parent Shape 2");
			var network = CreateNetwork(diagram);
			network.LinkEntity(parentShape2, jobHeader2);
			AssertNull("No errors shown, we allow parent to be linked to the shape if its child linked to another shape of this diagram and vice versa", UnitTestUserNotification.Instance.LastMessage.Text);

			//try to draw the link from shape with 'parent' workflow to shape with 'child' workflow
			network.CreateRelationship(parentShape2, childShape1);

			AssertEquals(@"Error - FP_FH_HeaderFrom: Invalid relationship. These workflows are already in a parent/child relationship and cannot also be in a pre/post requisite relationship. Please note that this may be an indirect relationship to the current workflow.
Error - FP_FH_HeaderTo: Invalid relationship. These workflows are already in a parent/child relationship and cannot also be in a pre/post requisite relationship. Please note that this may be an indirect relationship to the current workflow.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestCreateRelationship_JobLevelWorkflowLinkedToDiagramAndItsChildOnThisDiagram()
		{
			var diagram = CreateDiagram(Factory);

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job Header 1");
			var workflow1 = CreateWorkflow(jobHeader1, "Workflow for jobHeader1");

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job Header 2");
			var workflow2 = CreateWorkflow(jobHeader2, "Workflow for jobHeader2");

			// PCH link between two different jobs
			//
			//   jobHeader2
			//       ↑
			//   workflow1
			BufferManagement.Business.Test.BMSTestHelper.MakeChildOf(workflow1, jobHeader2);

			var shape = CreateShape(workflow1, diagram, "Shape 1");
			AssertNull("No errors shown: this is a valid case.", UnitTestUserNotification.Instance.LastMessage.Text);

			var network = CreateNetwork(diagram);
			network.LinkEntity(diagram, jobHeader2);

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);

			AssertEquals(@"The entity being linked has associated workflow dependencies (pre/post-requisites and/or child entities). Do you want to add all child entities to the diagram that are necessary to display the dependencies in full? If you choose ""No"" the direct pre/post-requisite links (if any) between the displayed entities will be added only.", UnitTestUserNotification.Instance.LastMessage.Text);

			Factory.Save();
			AssertEquals("When saving, invalid link between jobHeader2 and his child workflow1 should not be created", false, workflow1.IsPrerequisiteOf(jobHeader2));
			AssertEquals(0, shape.PreRequisiteLinks.Count());
		}

		#endregion

		#region DeleteRelationship

		public void TestDeleteRelationship()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape1 = NetworkTestCase.CreateShape(diagram, name: "shape1");
			var shape2 = NetworkTestCase.CreateShape(diagram, name: "shape2");

			var arrow = shape1.MakeVisiblePrerequisiteOf(shape2, diagram);

			var network = CreateNetwork(diagram);
			AssertEquals(2, network.Entities.Count);
			AssertEquals(1, shape1.PostRequisiteLinks.Count());
			AssertEquals(1, shape2.PreRequisiteLinks.Count());

			AssertEquals(true, network.DeleteRelationship(arrow));

			AssertEquals(true, arrow.IsDeleted);
			AssertEquals(0, shape1.PostRequisiteLinks.Count());
			AssertEquals(0, shape2.PreRequisiteLinks.Count());

			AssertEquals("Delete link between shapes 'shape1' and 'shape2'?", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestDeleteRelationship_ForShapesLinkedToWorkflows_ShouldIncludeExpandedMessage()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "A horse with the head of a rabbit");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "The body of a rabbit");

			workflow1.MakePrerequisiteOf(workflow2);

			var network = CreateNetwork(jobHeader.GetDefaultDiagram());
			AssertEquals(2, network.Entities.Count);

			AssertEquals(1, workflow1.LinksFromMeToOthers.Count());
			AssertEquals(1, workflow2.LinksFromOthersToMe.Count());

			var link = (BMNCNAttachment)network.Shapes.Single(s => s.Name == "A horse with the head of a rabbit").PostRequisiteLinks.Single();

			AssertEquals(true, network.DeleteRelationship(link));

			AssertEquals(true, link.IsDeleted);
			AssertEquals(0, workflow1.LinksFromMeToOthers.Count());
			AssertEquals(0, workflow2.LinksFromOthersToMe.Count());

			AssertEquals("Delete link between shapes 'A horse with the head of a rabbit' and 'The body of a rabbit'? This will also remove the pre-requisite relationship between the linked workflows.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestDeleteRelationship_CancelDelete()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "workflow1";
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "workflow2";

			workflow1.MakePrerequisiteOf(workflow2);

			var network = CreateNetwork(jobHeader.GetDefaultDiagram());
			AssertEquals(2, network.Entities.Count);

			AssertEquals(1, workflow1.LinksFromMeToOthers.Count());
			AssertEquals(1, workflow2.LinksFromOthersToMe.Count());

			var link = workflow1.LinksFromMeToOthers.Single();

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Cancel);
			AssertEquals(false, network.DeleteRelationship(link));

			AssertEquals(false, link.IsDeleted);
			AssertEquals(1, workflow1.LinksFromMeToOthers.Count());
			AssertEquals(1, workflow2.LinksFromOthersToMe.Count());

			AssertEquals("Delete link between shapes 'workflow1' and 'workflow2'?", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestDeleteRelationship_ShouldAlsoRemoveBufferFromDiagramAndRefreshNetwork()
		{
			var diagram = CreateDiagram(Factory, isScaled: true);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			AssertEquals(0, network.Entities.Count);

			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);
			childShape1.Name = "childShape1";
			childShape2.Name = "childShape2";

			AssertEquals(2, network.Entities.Count);

			var dependency = network.CreateRelationship(childShape1, childShape2).AsAttachment();
			AssertNull(dependency.GetBuffer());

			var action = new AddBufferAction(networkViewModel);
			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, childShape1))
			{
				action.GetChildActions().Single().AsJobNetworkAction().Execute();
			}

			AssertEquals(3, network.Entities.Count);
			var buffer = dependency.GetBuffer();
			AssertNotNull(buffer);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var newNetwork = CreateNetwork(newFactory.Load<BMNCNShape>(diagram.PK));
			var refreshed = false;
			newNetwork.Refreshed += (s, e) => refreshed = true;

			var entity = newNetwork["childShape1"].AsEntity(newNetwork);
			var loadedDependency = entity.Links.Select(l => l.AsAttachment()).Single(a => a.BNA_BNS_ToShape == childShape2.PK);
			var loadedBuffer = loadedDependency.GetBuffer();

			AssertNotNull(loadedBuffer);
			AssertEquals(3, newNetwork.Entities.Count);

			newNetwork.DeleteRelationship(loadedDependency);

			AssertEquals("Delete link between shapes 'childShape1' and 'childShape2'?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(true, refreshed);
			AssertEquals(2, newNetwork.Entities.Count);
			AssertEquals(true, loadedBuffer.IsDeleted);
		}

		public void TestDeleteRelationship_ForBufferLink_ShouldHideInstead()
		{
			var diagram = CreateDiagram(Factory, isScaled: true);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			AssertEquals(0, network.Entities.Count);

			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);
			childShape1.Name = "childShape1";
			childShape2.Name = "childShape2";

			AssertEquals(2, network.Entities.Count);

			var dependency = network.CreateRelationship(childShape1, childShape2).AsAttachment();
			AssertNull(dependency.GetBuffer());

			var action = new AddBufferAction(networkViewModel);
			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, childShape1))
			{
				action.GetChildActions().Single().AsJobNetworkAction().Execute();
			}

			AssertEquals(3, network.Entities.Count);
			var buffer = dependency.GetBuffer();
			var relationships = ((IProposedNetworkEntity)buffer).Links.Cast<BMNCNAttachment>().ToArray();

			var refreshed = false;
			network.Refreshed += (s, e) => refreshed = true;

			var deleteResult = network.DeleteRelationship(relationships[0]);
			AssertEquals(false, deleteResult);
			AssertEquals(true, refreshed);
			AssertEquals("This arrow cannot be removed independently from the connected buffer. Would you like to remove the buffer?", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals(true, buffer.IsDeleted);
			AssertEquals(true, relationships[0].IsDeleted);
			AssertEquals(true, relationships[1].IsDeleted);
		}

		public void TestDeleteRelationship_BetweenApprovedShapes()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var approvedChildShape1 = networkViewModel.CreateNewShape(diagram);
			var approvedChildShape2 = networkViewModel.CreateNewShape(diagram);

			var arrow1 = network.CreateRelationship(approvedChildShape1, approvedChildShape2).AsAttachment();
			network.SwitchToScaled();
			networkViewModel.ToggleApproval();

			var nonApprovedChildShape = networkViewModel.CreateNewShape(diagram);

			var arrow2 = network.CreateRelationship(approvedChildShape1, nonApprovedChildShape).AsAttachment();
			var arrow3 = network.CreateRelationship(nonApprovedChildShape, approvedChildShape2).AsAttachment();

			AssertEquals(true, network.DeleteRelationship(arrow3));
			AssertEquals(true, arrow3.IsDeleted);

			AssertEquals(true, network.DeleteRelationship(arrow2));
			AssertEquals(true, arrow2.IsDeleted);

			AssertEquals(false, network.DeleteRelationship(arrow1));
			AssertEquals(false, arrow1.IsDeleted);
			AssertEquals("This is an approved arrow and cannot be deleted. Would you like to decouple the arrow instead?\r\n\r\nDecoupling causes the dependency link to be removed but the arrow to be preserved on the project plan.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestDeleteResourceDependencyRelationship_BetweenApprovedShapes()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var approvedChildShape1 = networkViewModel.CreateNewShape(diagram);
			var approvedChildShape2 = networkViewModel.CreateNewShape(diagram);

			var arrow1 = network.CreateRelationship(approvedChildShape1, approvedChildShape2).AsAttachment();
			arrow1.BNA_Type = AttachmentTypeList.Codes.ResourceDependency;
			network.SwitchToScaled();
			networkViewModel.ToggleApproval();

			var nonApprovedChildShape = networkViewModel.CreateNewShape(diagram);

			var arrow2 = network.CreateRelationship(approvedChildShape1, nonApprovedChildShape).AsAttachment();
			var arrow3 = network.CreateRelationship(nonApprovedChildShape, approvedChildShape2).AsAttachment();

			AssertEquals(true, network.DeleteRelationship(arrow3));
			AssertEquals(true, arrow3.IsDeleted);

			AssertEquals(true, network.DeleteRelationship(arrow2));
			AssertEquals(true, arrow2.IsDeleted);

			AssertEquals(false, network.DeleteRelationship(arrow1));
			AssertEquals(false, arrow1.IsDeleted);
			AssertEquals("This is an approved arrow and cannot be deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestDeleteRelationship_ApprovedArrowExists_ShouldPromptToDecoupleInstead()
		{
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			var diagramShape = CreateDiagram(jobHeader);
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var shape1 = CreateShape(workflow1, diagram, "shape1");
			var shape2 = CreateShape(workflow2, diagram, "shape2");
			var shape3 = CreateShape(workflow3, diagram, "shape3");
			var arrow1_2 = shape1.MakeVisiblePrerequisiteOf(shape2);
			var link1_2 = arrow1_2.ProcessHeaderLink;

			network.SwitchToScaled();
			networkViewModel.ToggleApproval();
			var arrow2_3 = shape2.MakeVisiblePrerequisiteOf(shape3);
			var link2_3 = arrow2_3.ProcessHeaderLink;

			AssertEquals(true, arrow1_2.IsApproved);
			AssertEquals(false, arrow2_3.IsApproved);
			AssertEquals(ArrowAppearance.Normal, arrow1_2.AsEntity(network).Appearance);

			AssertEquals(false, network.DeleteRelationship(arrow1_2));
			AssertEquals("This is an approved arrow and cannot be deleted. Would you like to decouple the arrow instead?\r\n\r\nDecoupling causes the dependency link to be removed but the arrow to be preserved on the project plan.", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals(false, arrow1_2.IsDeleted);
			AssertEquals(true, link1_2.IsDeleted);
			AssertEquals(ArrowAppearance.Dotted, arrow1_2.AsEntity(network).Appearance);

			AssertEquals("Should be able to delete non-approved arrow", true, network.DeleteRelationship(arrow2_3));
			AssertEquals(true, arrow2_3.IsDeleted);
			AssertEquals(true, link2_3.IsDeleted);
		}

		public void TestDeleteRelationship_ApprovedArrowExists_ShouldPromptToDecoupleInstead_AnsweringCancel()
		{
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var link1_2 = workflow1.GetOrCreateDependencyLink(workflow2);

			var diagram = CreateDiagram(jobHeader);
			var shape1 = CreateShape(workflow1, diagram, "shape1");
			var shape2 = CreateShape(workflow2, diagram, "shape2");
			var arrow1_2 = CreateDependencyAttachment(diagram, link1_2, shape1, shape2);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			networkViewModel.ToggleApproval();

			AssertEquals(true, arrow1_2.IsApproved);
			AssertEquals(ArrowAppearance.Normal, arrow1_2.AsEntity(network).Appearance);

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Cancel);
			AssertEquals(false, network.DeleteRelationship(arrow1_2));
			AssertEquals("This is an approved arrow and cannot be deleted. Would you like to decouple the arrow instead?\r\n\r\nDecoupling causes the dependency link to be removed but the arrow to be preserved on the project plan.", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals(false, arrow1_2.IsDeleted);
			AssertEquals(false, link1_2.IsDeleted);
			AssertEquals(ArrowAppearance.Normal, arrow1_2.AsEntity(network).Appearance);
		}

		public void TestDeleteRelationship_AnotherApprovedArrowExists_ShouldPromptToDecoupleInstead()
		{
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			workflow1.FH_CompletionStatement = "workflow1";
			workflow2.FH_CompletionStatement = "workflow2";

			var link1_2 = workflow1.GetOrCreateDependencyLink(workflow2);

			var diagram = CreateDiagram(jobHeader);
			var shape1 = CreateShape(workflow1, diagram, "shape1");
			var shape2 = CreateShape(workflow2, diagram, "shape2");
			var arrow1_2 = CreateDependencyAttachment(diagram, link1_2, shape1, shape2);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			networkViewModel.ToggleApproval();

			AssertEquals(true, arrow1_2.IsApproved);

			var defaultNetwork = CreateNetwork(jobHeader.GetDefaultDiagram());
			var defaultArrow = defaultNetwork.Entities.First().Links.Single().AsAttachment();
			AssertEquals(false, defaultArrow.IsApproved);

			AssertEquals(true, defaultNetwork.DeleteRelationship(defaultArrow));
			AssertMultilineASCIIEquals("", "This arrow represents a dependency which has been approved on a project plan. Deleting the arrow on this diagram will cause the approved arrow to be decoupled.\r\n\r\nDecoupling causes the dependency link to be removed but the arrow to be preserved on the project plan.", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals(true, defaultArrow.IsDeleted);
			AssertEquals(true, link1_2.IsDeleted);
			AssertEquals("Should decouple approved arrow", true, arrow1_2.BNA_IsDecouple);
		}

		public void TestDeleteRelationship_AlreadyDecoupled()
		{
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var link1_2 = workflow1.GetOrCreateDependencyLink(workflow2);

			var diagram = CreateDiagram(jobHeader);
			var shape1 = CreateShape(workflow1, diagram, "shape1");
			var shape2 = CreateShape(workflow2, diagram, "shape2");
			var arrow = CreateDependencyAttachment(diagram, link1_2, shape1, shape2);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			networkViewModel.ToggleApproval();

			AssertEquals(true, arrow.IsApproved);
			AssertEquals(ArrowAppearance.Normal, arrow.AsEntity(network).Appearance);

			AssertEquals(false, network.DeleteRelationship(arrow));
			AssertEquals("This is an approved arrow and cannot be deleted. Would you like to decouple the arrow instead?\r\n\r\nDecoupling causes the dependency link to be removed but the arrow to be preserved on the project plan.", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals(true, arrow.BNA_IsDecouple);
			AssertEquals(true, link1_2.IsDeleted);
			AssertEquals(ArrowAppearance.Dotted, arrow.AsEntity(network).Appearance);

			AssertEquals(false, network.DeleteRelationship(arrow));
			AssertEquals("This arrow has already been decoupled and cannot be removed.", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals(true, arrow.BNA_IsDecouple);
		}

		public void TestDeleteRelationship_AlreadyDecoupledUnApprovedDiagram()
		{
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var link1_2 = workflow1.GetOrCreateDependencyLink(workflow2);

			var diagram = CreateDiagram(jobHeader);
			var shape1 = CreateShape(workflow1, diagram, "Shakiraquan");
			var shape2 = CreateShape(workflow2, diagram, "Dookmarriot");
			var arrow = CreateDependencyAttachment(diagram, link1_2, shape1, shape2);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			networkViewModel.ToggleApproval();

			AssertEquals(true, arrow.IsApproved);
			AssertEquals(ArrowAppearance.Normal, arrow.AsEntity(network).Appearance);

			AssertEquals(false, network.DeleteRelationship(arrow));
			AssertEquals("This is an approved arrow and cannot be deleted. Would you like to decouple the arrow instead?\r\n\r\nDecoupling causes the dependency link to be removed but the arrow to be preserved on the project plan.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();

			AssertEquals(true, arrow.BNA_IsDecouple);
			AssertEquals(true, link1_2.IsDeleted);
			AssertEquals(ArrowAppearance.Dotted, arrow.AsEntity(network).Appearance);

			networkViewModel.ToggleApproval();

			AssertEquals(true, network.DeleteRelationship(arrow));
			AssertEquals("Delete link between shapes 'Shakiraquan' and 'Dookmarriot'?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(true, arrow.IsDeleted);
		}

		#endregion

		#region UnlinkEntity

		public void TestUnLinkEntity_WhenNoLinkedParentsUpTheHierarchy_ShouldNotPromptUser()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var diagram = CreateDiagram(Factory, name: "Diahgrahm");
			var subDiagram = CreateShape(diagram, "Sub-Diahgrahm");
			var subSubDiagram = CreateShape(subDiagram, "Sub-Sub-Diahgrahm");
			var subSubSubDiagram = CreateShape(jobHeader, subSubDiagram, "Sub-Sub-Sub-Diahgrahm");

			var network = CreateNetwork(diagram);

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			AssertNotEquals(ZGuid.Empty, subSubSubDiagram.BNS_RelatedEntityID);

			network.UnlinkEntity(subSubSubDiagram);

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(ZGuid.Empty, subSubSubDiagram.BNS_RelatedEntityID);
		}

		public void TestUnLinkEntity_WhenMultipleParentsUpTheHierarchy_ShouldPromptAndDisconnectAllParentChildLinks()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var diagram = CreateDiagram(jobHeader1, name: "Diahgrahm");
			var subDiagram = CreateShape(jobHeader2, diagram, "Sub-Diahgrahm");
			var subSubDiagram = CreateShape(subDiagram, "Sub-Sub-Diahgrahm");
			var subSubSubDiagram = CreateShape(subSubDiagram, "Sub-Sub-Sub-Diahgrahm");

			var network = CreateNetwork(diagram);

			AssertEquals(true, network.LinkEntity(subSubSubDiagram, jobHeader3));
			jobHeader3.GetOrCreateLinkToParent(jobHeader1);

			AssertIsParent(jobHeader3, jobHeader1);
			AssertIsParent(jobHeader3, jobHeader2);

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			AssertNotEquals(ZGuid.Empty, subSubSubDiagram.BNS_RelatedEntityID);

			network.UnlinkEntity(subSubSubDiagram);

			AssertIsNotParent(jobHeader3, jobHeader1);
			AssertIsNotParent(jobHeader3, jobHeader2);
			AssertEquals(ZGuid.Empty, subSubSubDiagram.BNS_RelatedEntityID);

			AssertMultilineASCIIEquals("", @"The workflow being un-linked is a child of the below linked ancestor shapes' workflows. Would you like to disconnect these relationships?

[Diahgrahm]: [Organization (XVBQP68SIYXQ) - Job Organization (XVBQP68SIYXQ) is complete.]
[Sub-Diahgrahm]: [Organization (H5ZX52PAMCOI) - Job Organization (H5ZX52PAMCOI) is complete.]
", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestUnLinkEntity_ForRootDiagram_WhenMultipleChildrenDownTheHierarchy_ShouldPromptAndDisconnectAllParentChildLinks()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			jobHeader2.GetOrCreateLinkToParent(jobHeader1);
			jobHeader3.GetOrCreateLinkToParent(jobHeader1);

			var diagram = CreateDiagram(jobHeader1, name: "Diahgrahm");
			var subDiagram = CreateShape(diagram, "Sub-Diahgrahm");
			var subSubDiagram1 = CreateShape(jobHeader2, subDiagram, "Sub-Sub-Diahgrahm 1");
			var subSubDiagram2 = CreateShape(jobHeader3, subDiagram, "Sub-Sub-Diahgrahm 2");

			var network = CreateNetwork(diagram);

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(jobHeader1.PK, diagram.BNS_RelatedEntityID);

			AssertIsParent(jobHeader2, jobHeader1);
			AssertIsParent(jobHeader3, jobHeader1);

			network.UnlinkEntity(diagram);

			AssertIsNotParent(jobHeader2, jobHeader1);
			AssertIsNotParent(jobHeader3, jobHeader1);
			AssertEquals(ZGuid.Empty, diagram.BNS_RelatedEntityID);

			AssertMultilineASCIIEquals("", @"The workflow being un-linked is a parent of the below linked descendant shapes' workflows. Would you like to disconnect these relationships?

[Sub-Sub-Diahgrahm 1]: [Organization (H5ZX52PAMCOI) - Job Organization (H5ZX52PAMCOI) is complete.]
[Sub-Sub-Diahgrahm 2]: [Organization (ZGP5LX5SQPEB) - Job Organization (ZGP5LX5SQPEB) is complete.]
", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestUnLinkEntity_WhenMultipleParentsUpTheHierarchy_ShouldPromptAndDisconnectAllParentChildLinks_AnsweringNo()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var diagram = CreateDiagram(jobHeader1, name: "Diahgrahm");
			var subDiagram = CreateShape(jobHeader2, diagram, "Sub-Diahgrahm");
			var subSubDiagram = CreateShape(subDiagram, "Sub-Sub-Diahgrahm");
			var subSubSubDiagram = CreateShape(subSubDiagram, "Sub-Sub-Sub-Diahgrahm");

			var network = CreateNetwork(diagram);

			AssertEquals(true, network.LinkEntity(subSubSubDiagram, jobHeader3));
			jobHeader3.GetOrCreateLinkToParent(jobHeader1);

			AssertIsParent(jobHeader3, jobHeader1);
			AssertIsParent(jobHeader3, jobHeader2);

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(jobHeader3.PK, subSubSubDiagram.BNS_RelatedEntityID);

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
			network.UnlinkEntity(subSubSubDiagram);

			AssertIsParent(jobHeader3, jobHeader1);
			AssertIsParent(jobHeader3, jobHeader2);
			AssertEquals(ZGuid.Empty, subSubSubDiagram.BNS_RelatedEntityID);

			AssertMultilineASCIIEquals("", @"The workflow being un-linked is a child of the below linked ancestor shapes' workflows. Would you like to disconnect these relationships?

[Diahgrahm]: [Organization (XVBQP68SIYXQ) - Job Organization (XVBQP68SIYXQ) is complete.]
[Sub-Diahgrahm]: [Organization (H5ZX52PAMCOI) - Job Organization (H5ZX52PAMCOI) is complete.]
", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestUnLinkEntity_WhenMultipleParentsUpTheHierarchy_ShouldPromptAndDisconnectAllParentChildLinks_AnsweringCancel()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var diagram = CreateDiagram(jobHeader1, name: "Diahgrahm");
			var subDiagram = CreateShape(jobHeader2, diagram, "Sub-Diahgrahm");
			var subSubDiagram = CreateShape(subDiagram, "Sub-Sub-Diahgrahm");
			var subSubSubDiagram = CreateShape(subSubDiagram, "Sub-Sub-Sub-Diahgrahm");

			var network = CreateNetwork(diagram);

			AssertEquals(true, network.LinkEntity(subSubSubDiagram, jobHeader3));
			jobHeader3.GetOrCreateLinkToParent(jobHeader1);

			AssertIsParent(jobHeader3, jobHeader1);
			AssertIsParent(jobHeader3, jobHeader2);

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(jobHeader3.PK, subSubSubDiagram.BNS_RelatedEntityID);

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Cancel);
			bool isUnlinkSuccessful = network.UnlinkEntity(subSubSubDiagram, true);

			AssertIsParent(jobHeader3, jobHeader1);
			AssertIsParent(jobHeader3, jobHeader2);
			AssertEquals(jobHeader3.PK, subSubSubDiagram.BNS_RelatedEntityID);

			AssertEquals(false, subSubSubDiagram.IsDeleted);
			AssertEquals(true, subSubSubDiagram.Active);
			AssertEquals(false, isUnlinkSuccessful);

			AssertMultilineASCIIEquals("", @"The workflow being un-linked is a child of the below linked ancestor shapes' workflows. Would you like to disconnect these relationships?

[Diahgrahm]: [Organization (XVBQP68SIYXQ) - Job Organization (XVBQP68SIYXQ) is complete.]
[Sub-Diahgrahm]: [Organization (H5ZX52PAMCOI) - Job Organization (H5ZX52PAMCOI) is complete.]
", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestUnLinkEntity_BetweenLinkedShapesInHierarchy_ShouldNotRemoveChildShapes()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "jobHeader1");
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "jobHeader2");
			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "jobHeader3");
			var jobHeader4 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "jobHeader4");

			var diagram = CreateDiagram(Factory, name: "Diahgrahm");
			var subDiagram = CreateShape(diagram, "Sub-Diahgrahm");
			var subSubDiagram = CreateShape(subDiagram, "Sub-Sub-Diahgrahm");
			var subSubSubDiagram = CreateShape(subSubDiagram, "Sub-Sub-Sub-Diahgrahm");

			var network = CreateNetwork(diagram);

			network.LinkEntity(diagram, jobHeader1);
			network.LinkEntity(subDiagram, jobHeader2);
			network.LinkEntity(subSubDiagram, jobHeader3);
			network.LinkEntity(subSubSubDiagram, jobHeader4);

			AssertIsParent(jobHeader4, jobHeader3);
			AssertIsParent(jobHeader3, jobHeader2);
			AssertIsParent(jobHeader2, jobHeader1);

			AssertEquals(true, subDiagram.IsChildOf(diagram));
			AssertEquals(true, subSubDiagram.IsChildOf(subDiagram));
			AssertEquals(true, subSubSubDiagram.IsChildOf(subSubDiagram));

			AssertEquals(jobHeader2, subDiagram.ProcessHeader);
			AssertEquals(jobHeader3, subSubDiagram.ProcessHeader);
			AssertEquals(jobHeader4, subSubSubDiagram.ProcessHeader);

			Factory.Save();

			network.UnlinkEntity(subSubDiagram);
			AssertNoExceptionThrown(Factory.Save);

			AssertEquals(true, subDiagram.IsChildOf(diagram));
			AssertEquals(true, subSubDiagram.IsChildOf(subDiagram));
			AssertEquals(true, subSubSubDiagram.IsChildOf(subSubDiagram));

			AssertEquals(jobHeader1, diagram.ProcessHeader);
			AssertEquals(jobHeader2, subDiagram.ProcessHeader);
			AssertNull(subSubDiagram.ProcessHeader);
			AssertEquals(jobHeader4, subSubSubDiagram.ProcessHeader);
		}

		public void TestUnlinkEntity()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var diagram = CreateDiagram(jobHeader);

			var job2 = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			AssertEquals(0, network.Entities.Count);
			network.SwitchToScaled();

			var diagram2 = CreateShape(jobHeader2, diagram);
			var shape1 = networkViewModel.CreateNewShape(diagram);
			var workflow1 = shape1.ProcessHeader;

			AssertNull(shape1.ProcessHeader);
			AssertEquals(job, diagram.ProcessHeader.Parent);

			var wasRefreshed = false;
			network.Refreshed += delegate
			{
				wasRefreshed = true;
			};

			var diagramEntity = network.DiagramEntity;
			var startingCreateActionCount = networkViewModel.GetApplicableCreateEntityActions_ForTesting(diagramEntity).Count();
			AssertNotEquals(0, startingCreateActionCount);

			AssertEquals(false, diagram.IsDeleted);
			network.UnlinkEntity(diagram);
			AssertEquals(false, diagram.IsDeleted);

			AssertEquals(true, wasRefreshed);
			AssertEquals("Now that the entity is un-linked, cannot create a workflow shape anymore", startingCreateActionCount - 1, networkViewModel.GetApplicableCreateEntityActions_ForTesting(diagramEntity).Count());
			AssertCollectionNotContains(networkViewModel.GetApplicableCreateEntityActions_ForTesting(diagramEntity), a => a is CreateWorkflowAction);

			AssertNotEquals(jobHeader, diagram.ProcessHeader);
			AssertNull(shape1.ProcessHeader);
			AssertEquals(jobHeader2, diagram2.ProcessHeader);
		}

		public void TestUnlinkEntity_OriginalWorkflowNotDeleted()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = CreateWorkflow(jobHeader, "workflow");
			var diagram = CreateDiagram(jobHeader);
			var network = CreateNetwork(diagram);
			diagram.BNS_RelatedEntityID = workflow.PK;

			Assert(diagram.CanUnlinkEntity);
			AssertEquals(workflow.PK, diagram.ProcessHeader.PK);

			network.UnlinkEntity(diagram);

			AssertNull(diagram.ProcessHeader);
			Assert(!workflow.IsDeleted);
		}

		#endregion

		#region Import Diagrams

		#region Maintaining Scaled and Unscaled status

		public void TestImport_Variations()
		{
			CombineAssertions(() =>
			{
				AssertImportHasError(null, isParentScaled: false, isChildScaled: false);
				AssertImportHasError(null, isParentScaled: true, isChildScaled: false);
				AssertImportHasError("Scaled diagrams cannot be imported into non-scaled diagrams. To import a scaled diagram, this diagram must be scaled. You can create a scaled copy of this diagram using the Switch to Scaled Mode action.", isParentScaled: false, isChildScaled: true);
				AssertImportHasError(null, isParentScaled: true, isChildScaled: true);
			});
		}

		public void AssertImportHasError(string expectedError, bool isParentScaled, bool isChildScaled)
		{
			UnitTestUserNotification.Instance.ClearMessages();
			var diagram1 = CreateDiagram(Factory, name: "Silver", isScaled: isParentScaled);
			var diagram2 = CreateDiagram(Factory, name: "Red", isScaled: isChildScaled);

			var network = CreateNetwork(diagram1);

			network.ImportEntities(diagram1, diagram2);

			Func<bool, string> convertToStr = e => e ? "Scaled" : "Unscaled";

			AssertEquals(string.Format("Expected error '{0}' when importing [{1}] into [{2}].", expectedError, convertToStr(isChildScaled), convertToStr(isParentScaled)), expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		public void TestPickAndImportEntity_Null()
		{
			var diagram = CreateJobAndDiagram(Factory);

			JobNetwork network = null;
			var controller = NetworkTestCase.CreateMockableController(Mocks);

			controller.Setup(m => m.PickEntity(ModuleIDs.NetworkDiagram, It.IsAny<bool>())).Returns((BusinessObject)null);
			using (ObjectFactory.Substitute(controller.Object))
			{
				network = CreateNetwork(diagram);
				network.PickAndImportEntities(diagram);

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPickAndImportEntity_CannotImportCurrentlyShownDiagram()
		{
			var diagram = CreateJobAndDiagram(Factory);

			JobNetwork network = null;
			var controller = NetworkTestCase.CreateMockableController(Mocks);

			controller.Setup(m => m.PickEntity(ModuleIDs.NetworkDiagram, It.IsAny<bool>())).Returns(diagram);

			using (ObjectFactory.Substitute(controller.Object))
			{
				network = CreateNetwork(diagram);
				network.PickAndImportEntities(diagram);

				AssertEquals("A diagram cannot be imported into itself.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPickAndImportEntity_CannotImportHiddenEntities()
		{
			var diagram = CreateJobAndDiagram(Factory);
			var workflow = CreateWorkflow(diagram.ProcessJobHeader, "The Flowerpot Wars");
			var shape = CreateDiagram(workflow);

			Factory.Save();

			JobNetwork network = null;
			var controller = NetworkTestCase.CreateMockableController(Mocks);

			controller.Setup(m => m.PickEntity(It.IsAny<ModuleIdentifier>(), It.IsAny<bool>())).Returns(shape);

			using (ObjectFactory.Substitute(controller.Object))
			{
				network = CreateNetwork(diagram);
				network.PickAndImportEntities(diagram);

				AssertEquals("Cannot import an entity that already exists as a hidden entity.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPickAndImportEntity_CannotImportHiddenEntities_ForSubDiagramShapeWithNoJob()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "Icky Ock");

			var diagram = CreateDiagram(jobHeader);
			var subDiagram = CreateDiagram(Factory);

			var controller = NetworkTestCase.CreateMockableController(Mocks);

			controller.Setup(m => m.PickEntity(It.IsAny<ModuleIdentifier>(), It.IsAny<bool>())).Returns(subDiagram);
			using (ObjectFactory.Substitute(controller.Object))
			{
				var network = CreateNetwork(diagram);
				network.PickAndImportEntities(diagram);

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPickAndImportEntity_CannotImportCurrentlyShownSubDiagram()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var diagram1 = CreateDiagram(jobHeader1);
			var diagram2 = CreateShape(jobHeader2, diagram1);

			JobNetwork network = null;
			var controller = NetworkTestCase.CreateMockableController(Mocks);

			controller.Setup(m => m.PickEntity(It.IsAny<ModuleIdentifier>(), It.IsAny<bool>())).Returns(diagram2);
			using (ObjectFactory.Substitute(controller.Object))
			{
				network = CreateNetwork(diagram1);
				network.PickAndImportEntities(diagram1);

				AssertEquals("This shape is already present on the diagram.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPickAndImportEntity_CannotImportSubDiagramForCurrentlyShownBackingEntity()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var diagram1 = CreateDiagram(jobHeader1, name: "Root diagram");
			var diagram2 = CreateShape(jobHeader2, diagram1, name: "Existing shape linked to jobHeader2");
			var diagram3 = CreateDiagram(jobHeader2, name: "Diagram to import which is also linked to jobHeader2");

			JobNetwork network = null;
			var controller = NetworkTestCase.CreateMockableController(Mocks);

			controller.Setup(m => m.PickEntity(It.IsAny<ModuleIdentifier>(), It.IsAny<bool>())).Returns(diagram3);

			using (ObjectFactory.Substitute(controller.Object))
			{
				network = CreateNetwork(diagram1);
				network.PickAndImportEntities(diagram1);
				AssertEquals("The selected diagram or its descendants already exist as linked entities.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPickAndImportEntity_ShouldSetParentChildRelationship()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();

			var diagram1 = CreateDiagram(jobHeader1, name: "diagram1");
			var diagram2 = CreateDiagram(jobHeader2, name: "diagram2");

			JobNetwork network = null;
			var controller = NetworkTestCase.CreateMockableController(Mocks);

			controller.Setup(m => m.PickEntity(It.IsAny<ModuleIdentifier>(), It.IsAny<bool>())).Returns(diagram2);
			using (ObjectFactory.Substitute(controller.Object))
			{
				network = CreateNetwork(diagram1);
				AssertEquals(false, jobHeader2.IsChildOf(jobHeader1));

				var result = network.PickAndImportEntities(diagram1).Single().AsShape();
				AssertEquals("diagram2", result.Name);
				AssertEquals(true, jobHeader2.IsChildOf(jobHeader1));
			}
		}

		public void TestDontImportCircularReferences_Direct()
		{
			var jobHeader1 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader3 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var diagram = CreateDiagram(jobHeader1, name: "Root");
			var subDiagram = CreateShape(jobHeader2, diagram, name: "Child");
			var subSubDiagram = CreateShape(jobHeader3, subDiagram, name: "Grandchild");

			JobNetwork network = null;
			var controller = NetworkTestCase.CreateMockableController(Mocks);

			controller.Setup(m => m.PickEntity(It.IsAny<ModuleIdentifier>(), It.IsAny<bool>())).Returns(subDiagram);
			using (ObjectFactory.Substitute(controller.Object))
			{
				network = CreateNetwork(diagram);
				AssertEquals(2, network.Entities.Count);

				var importedShapes = network.PickAndImportEntities(subSubDiagram).ToArray();
				AssertEquals("This shape is already present on the diagram.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, subDiagram.IsChildOf(subSubDiagram));
			}
		}

		public void TestDontImportCircularReferences_IndirectReference()
		{
			var jobHeader1 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader3 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader4 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var diagram = CreateDiagram(jobHeader1, name: "Root");
			var subDiagram = CreateShape(jobHeader2, diagram, name: "Child");
			var subSubDiagram = CreateShape(jobHeader3, subDiagram, name: "Grandchild");

			var middlyDiagram = CreateDiagram(jobHeader4, name: "OtherRoot");

			JobNetwork network = null;
			var controller = NetworkTestCase.CreateMockableController(Mocks);

			controller.Setup(m => m.PickEntity(It.IsAny<ModuleIdentifier>(), It.IsAny<bool>())).Returns(subSubDiagram);
			using (ObjectFactory.Substitute(controller.Object))
			{
				network = CreateNetwork(middlyDiagram);
				network.PickAndImportEntities(middlyDiagram).ToArray();
			}

			network = null;
			controller = NetworkTestCase.CreateMockableController(Mocks);

			controller.Setup(m => m.PickEntity(It.IsAny<ModuleIdentifier>(), It.IsAny<bool>())).Returns(middlyDiagram);
			using (ObjectFactory.Substitute(controller.Object))
			{
				network = CreateNetwork(diagram);
				var importedShapes = network.PickAndImportEntities(subSubDiagram).ToArray();
				AssertEquals("The selected diagram or its descendants already exist as linked entities.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPickAndImportEntities_ForSubDiagramEntity()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var jobHeader3 = CreateJobHeader<OrgHeader>();
			var jobHeader4 = CreateJobHeader<OrgHeader>();

			var diagram = CreateDiagram(jobHeader1, name: "diagram");
			var subDiagram = CreateShape(jobHeader2, diagram, name: "subDiagram");
			var subSubDiagram = CreateShape(jobHeader3, subDiagram, name: "subSubDiagram");
			var subSubSubDiagram = CreateDiagram(jobHeader4, name: "subSubSubDiagram");

			var jobHeader4Workflow = jobHeader4.ProcessHeaders[0];
			var jobHeader4WorkflowShape = CreateShape(jobHeader4Workflow, subSubSubDiagram, name: "jobHeader4WorkflowShape");

			JobNetwork network = null;
			var controller = NetworkTestCase.CreateMockableController(Mocks);

			controller.Setup(m => m.PickEntity(It.IsAny<ModuleIdentifier>(), It.IsAny<bool>())).Returns(subSubSubDiagram);
			using (ObjectFactory.Substitute(controller.Object))
			{
				network = CreateNetwork(diagram);
				AssertEquals(2, network.Entities.Count);

				var importedShapes = network.PickAndImportEntities(subSubDiagram).AsShapes().ToArray();
				AssertEquals(2, importedShapes.Length);

				Factory.Save();

				var importedSubSubSubDiagram = importedShapes[0];
				var importedjobHeader4WorkflowShape = importedShapes[1];

				AssertEquals("subSubSubDiagram", importedSubSubSubDiagram.Name);
				AssertEquals("jobHeader4WorkflowShape", importedjobHeader4WorkflowShape.Name);

				AssertEquals("Should make imported (copied) sub-diagrams into shapes", ShapeTypeList.Codes.Shape, importedSubSubSubDiagram.BNS_ShapeType);
				AssertEquals("Should leave imported (copied) shapes as shapes", ShapeTypeList.Codes.Shape, importedjobHeader4WorkflowShape.BNS_ShapeType);

				AssertNotEquals("Should clone original shape when importing", subSubSubDiagram, importedSubSubSubDiagram);
				AssertNotEquals("Should clone original shape when importing", jobHeader4WorkflowShape, importedjobHeader4WorkflowShape);

				AssertEquals(subSubDiagram, importedSubSubSubDiagram.AsEntity(network).Owner.Shape);
				AssertEquals(importedSubSubSubDiagram, importedjobHeader4WorkflowShape.AsEntity(network).Owner.Shape);

				AssertEquals(4, network.Entities.Count);
				AssertEquals(true, jobHeader4.IsChildOf(jobHeader3));
			}
		}

		public void TestPickAndImportEntities_ImportedPinnedShapesShouldNotBePinned()
		{
			var diadad = CreateDiagram(Factory, name: "diadad");
			var shapeOfDiadad = CreateShape(diadad, name: "diadaughter");

			var diagramToImport = CreateDiagram(Factory, name: "diagramToImport");
			var shapeToImport = CreateShape(diagramToImport, name: "shapeToImport");

			shapeOfDiadad.PinShape(CreateNetworkViewModel(diadad));
			shapeToImport.PinShape(CreateNetworkViewModel(diagramToImport));

			JobNetwork network = null;
			var controller = NetworkTestCase.CreateMockableController(Mocks);

			controller.Setup(m => m.PickEntity(It.IsAny<ModuleIdentifier>(), It.IsAny<bool>())).Returns(diagramToImport);
			using (ObjectFactory.Substitute(controller.Object))
			{
				network = CreateNetwork(diadad);

				var importedShapes = network.PickAndImportEntities(diadad).AsShapes().ToArray();
				AssertEquals(2, importedShapes.Length);

				Factory.Save();

				var importedDiagramToImport = importedShapes[0];
				var importedShapeToImport = importedShapes[1];

				AssertEquals("Cloned shape should not be pinned when importing...", false, importedShapeToImport.IsPinned);
				AssertEquals("Should not disturb existing pins when importing", true, shapeOfDiadad.IsPinned);
			}
		}

		public void TestRootOfShape()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			var workflow4 = jobHeader.ProcessHeaders.AddNew();
			var workflow5 = jobHeader.ProcessHeaders.AddNew();

			var diagramShape = CreateDiagram(jobHeader);
			var workflow1Shape = CreateShape(workflow1, diagramShape);
			var workflow2Shape = CreateShape(workflow2, diagramShape);
			var workflow3Shape = CreateShape(workflow3, diagramShape);
			var workflow4Shape = CreateShape(workflow4, diagramShape);
			var workflow5Shape = CreateShape(workflow5, diagramShape);

			Factory.Save();

			var network = CreateNetwork(diagramShape);

			AssertEquals(true, network.DiagramEntity.Shape.IsSameEntity(diagramShape));

			AssertEquals(5, network.Entities.Count);

			AssertEquals(true, network.Entities.ShapeEntities.All(s => s.Root.Shape.IsSameEntity(diagramShape)));
		}

		#endregion

		#region Scale

		public void TestSwitchToScaledAndApprove_ShouldPersistFloatDetails()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var shape1_cc = networkViewModel.CreateNewShape(diagram);
			var shape2_cc = networkViewModel.CreateNewShape(diagram);
			var shape3_cc = networkViewModel.CreateNewShape(diagram);
			var shape4_nonCC = networkViewModel.CreateNewShape(diagram);
			var shape5_nonCC = networkViewModel.CreateNewShape(diagram);

			network.CreateRelationship(shape1_cc, shape2_cc);
			network.CreateRelationship(shape2_cc, shape3_cc);
			network.CreateRelationship(shape4_nonCC, shape3_cc);
			network.CreateRelationship(shape5_nonCC, shape3_cc);

			network.SwitchToScaled();
			networkViewModel.ToggleApproval();

			AssertEquals(true, shape1_cc.IsCriticalPath);
			AssertEquals(true, shape2_cc.IsCriticalPath);
			AssertEquals(true, shape3_cc.IsCriticalPath);
			AssertEquals(false, shape4_nonCC.IsCriticalPath);
			AssertEquals(false, shape5_nonCC.IsCriticalPath);

			AssertEquals(0m, shape1_cc.Shape.EarliestStartHours);
			AssertEquals(24m, shape2_cc.Shape.EarliestStartHours);
			AssertEquals(48m, shape3_cc.Shape.EarliestStartHours);
			AssertEquals(0m, shape4_nonCC.Shape.EarliestStartHours);
			AssertEquals(0m, shape5_nonCC.Shape.EarliestStartHours);

			AssertEquals(0m, shape1_cc.Shape.LatestStartHours);
			AssertEquals(24m, shape2_cc.Shape.LatestStartHours);
			AssertEquals(48m, shape3_cc.Shape.LatestStartHours);
			AssertEquals(24m, shape4_nonCC.Shape.LatestStartHours);
			AssertEquals(24m, shape5_nonCC.Shape.LatestStartHours);

			AssertEquals(24m, shape1_cc.Shape.EarliestFinishHours);
			AssertEquals(48m, shape2_cc.Shape.EarliestFinishHours);
			AssertEquals(72m, shape3_cc.Shape.EarliestFinishHours);
			AssertEquals(24m, shape4_nonCC.Shape.EarliestFinishHours);
			AssertEquals(24m, shape5_nonCC.Shape.EarliestFinishHours);

			AssertEquals(24m, shape1_cc.Shape.LatestFinishHours);
			AssertEquals(48m, shape2_cc.Shape.LatestFinishHours);
			AssertEquals(72m, shape3_cc.Shape.LatestFinishHours);
			AssertEquals(48m, shape4_nonCC.Shape.LatestFinishHours);
			AssertEquals(48m, shape5_nonCC.Shape.LatestFinishHours);

			AssertEquals(0m, shape1_cc.Shape.FloatHours);
			AssertEquals(0m, shape2_cc.Shape.FloatHours);
			AssertEquals(0m, shape3_cc.Shape.FloatHours);
			AssertEquals(24m, shape4_nonCC.Shape.FloatHours);
			AssertEquals(24m, shape5_nonCC.Shape.FloatHours);
		}

		public void TestScaleDescriptor()
		{
			var diagram = Factory.New<BMNCNShape>();
			var network = CreateNetwork(diagram);

			AssertNull(network.ScaleDescriptor);

			diagram.IsScaled = true;

			AssertNotNull(network.ScaleDescriptor);
			Assert(!network.ScaleDescriptor.IsBranchOrDepartmentInvalidated);
			Assert(!network.ScaleDescriptor.IsScaleDescriptorInvalidated);

			diagram.ScheduleBizo.BNC_GB_Branch = ZGuid.Empty;
			network = CreateNetwork(diagram);

			Assert(network.ScaleDescriptor.IsBranchOrDepartmentInvalidated);
			Assert(network.ScaleDescriptor.IsScaleDescriptorInvalidated);

			diagram.ScheduleBizo.BNC_GE_Department = ZGuid.Empty;
			network = CreateNetwork(diagram);

			Assert(network.ScaleDescriptor.IsBranchOrDepartmentInvalidated);
			Assert(network.ScaleDescriptor.IsScaleDescriptorInvalidated);

			diagram.ScheduleBizo.BNC_GB_Branch = Env.CurrentBranchPK;
			network = CreateNetwork(diagram);

			Assert(network.ScaleDescriptor.IsBranchOrDepartmentInvalidated);
			Assert(network.ScaleDescriptor.IsScaleDescriptorInvalidated);

			diagram.ScheduleBizo.BNC_GE_Department = Env.CurrentDepartmentPK;
			network = CreateNetwork(diagram);

			Assert(!network.ScaleDescriptor.IsBranchOrDepartmentInvalidated);
			Assert(!network.ScaleDescriptor.IsScaleDescriptorInvalidated);

			diagram.Delete();

			Assert(!network.ScaleDescriptor.IsBranchOrDepartmentInvalidated);
			Assert(network.ScaleDescriptor.IsScaleDescriptorInvalidated);
		}

		public void TestSwitchToScaled_ShouldPushEntitiesEarly()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			var link1_2 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link2_3 = workflow2.GetOrCreateDependencyLink(workflow3);

			var network = CreateNetwork(CreateDiagram(Factory));
			var diagram = network.DiagramEntity;
			var childShape1 = CreateShape(workflow1, diagram, "childShape1");
			var childShape2 = CreateShape(workflow2, diagram, "childShape2");
			var childShape3 = CreateShape(workflow3, diagram, "childShape3");

			var arrow1_2 = CreateDependencyAttachment(diagram, link1_2, childShape1, childShape2);
			var arrow2_3 = CreateDependencyAttachment(diagram, link2_3, childShape2, childShape3);

			AssertShapeOffset(childShape1, diagram, 0, 0);
			AssertShapeOffset(childShape2, diagram, 0, 0);
			AssertShapeOffset(childShape3, diagram, 0, 0);

			network.SwitchToScaled();

			AssertShapeOffset(childShape1, diagram, 0, 0);
			AssertShapeOffset(childShape2, diagram, 300, 0);
			AssertShapeOffset(childShape3, diagram, 600, 0);
		}

		public void TestSwitchToScaled_WhenChangingBufferName_ShouldBeUniqueWithinDiagram()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var shape1_cc = networkViewModel.CreateNewShape(diagram);
			var shape2_cc = networkViewModel.CreateNewShape(diagram);
			var shape3_cc = networkViewModel.CreateNewShape(diagram);

			var shape4_nonCC = networkViewModel.CreateNewShape(diagram);
			var shape5_nonCC = networkViewModel.CreateNewShape(diagram);

			shape1_cc.Name = "shape1_cc";
			shape2_cc.Name = "shape2_cc";
			shape3_cc.Name = "shape3_cc";
			shape4_nonCC.Name = "shape4_nonCC";
			shape5_nonCC.Name = "shape5_nonCC";

			network.CreateRelationship(shape1_cc, shape2_cc);
			network.CreateRelationship(shape2_cc, shape3_cc);
			network.CreateRelationship(shape4_nonCC, shape3_cc);
			network.CreateRelationship(shape5_nonCC, shape3_cc);

			network.SwitchToScaled();
			networkViewModel.PushAsLateAsPossible();

			AssertEquals(true, shape1_cc.IsOnCriticalPath);
			AssertEquals(true, shape2_cc.IsOnCriticalPath);
			AssertEquals(true, shape3_cc.IsOnCriticalPath);
			AssertEquals(false, shape4_nonCC.IsOnCriticalPath);
			AssertEquals(false, shape5_nonCC.IsOnCriticalPath);

			AssertEquals(0.0, shape1_cc.X);
			AssertEquals(300.0, shape2_cc.X);
			AssertEquals(600.0, shape3_cc.X);

			AssertEquals(300.0, shape4_nonCC.X);
			AssertEquals(300.0, shape5_nonCC.X);

			AssertEquals(24 * 60, shape1_cc.ExplicitDurationMinutes);
			AssertEquals(24 * 60, shape2_cc.ExplicitDurationMinutes);
			AssertEquals(24 * 60, shape3_cc.ExplicitDurationMinutes);
			AssertEquals(24 * 60, shape4_nonCC.ExplicitDurationMinutes);
			AssertEquals(24 * 60, shape5_nonCC.ExplicitDurationMinutes);

			AssertEquals(5, network.Entities.Count);

			new SuggestBufferAction(networkViewModel).ExecuteForEntityWithoutAccessCheck(diagram);

			AssertEquals(8, network.Entities.Count);

			var feedingBuffers = network.Entities.ShapeEntities.Where(s => s.IsBufferShape && s.Name.EndsWith("Feeding Buffer")).OrderBy(s => s.Name).ToArray();
			AssertEquals(2, feedingBuffers.Length);
			AssertEquals("shape4_nonCC Feeding Buffer", feedingBuffers[0].Name);
			AssertEquals("shape5_nonCC Feeding Buffer", feedingBuffers[1].Name);

			AssertEquals(400.0, feedingBuffers[0].X);
			AssertEquals(400.0, feedingBuffers[1].X);
			AssertEquals(200.0, feedingBuffers[0].Width);
			AssertEquals(200.0, feedingBuffers[1].Width);

			AssertEquals("Should move shapes to fit buffers", 100.0, shape4_nonCC.X);
			AssertEquals("Should move shapes to fit buffers", 100.0, shape5_nonCC.X);

			AssertNoErrors(feedingBuffers[0].NameInfo);
			AssertNoErrors(feedingBuffers[1].NameInfo);

			feedingBuffers[0].Name = "shape5_nonCC Feeding Buffer";
			AssertHasError(feedingBuffers[0].NameInfo, "There is another buffer with the same name in this diagram.");

			feedingBuffers[0].Name = "shape5_nonCC Feeding Buffer [2]";
			AssertNoErrors(feedingBuffers[0].NameInfo);
		}

		[TestDate(2014, 7, 24)]
		public void TestSwitchToScaled_WhenSizeAlreadyCorrect_ShouldSetDurations()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var workflow2 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "Workflow 2");

			VisualBoardsTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 3 * BMConstants.WorkingHoursPerDay * 60, estVariationFactor: 1);
			VisualBoardsTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 3 * BMConstants.WorkingHoursPerDay * 60, estVariationFactor: 1);

			var diagramShape = CreateDiagram(jobHeader, isScaled: true);
			var network = CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;
			var shape1 = CreateShape(workflow1, diagram, "Workflow 1");
			var shape2 = CreateShape(workflow2, diagram, "Workflow 2");

			shape1.MakeVisiblePrerequisiteOf(shape2);

			shape1.Width = 300;
			shape2.Width = 300;

			AssertEquals(1440, shape1.ExplicitDurationMinutes);
			AssertEquals(1440, shape2.ExplicitDurationMinutes);
		}

		#endregion

		#region Modify Affinities

		[GuiTest]
		public void TestModifyEntities()
		{
			var network = CreateNetwork(CreateDiagram(Factory));
			AssertNoExceptionThrown(() => network.ModifyAffinities(network.DiagramEntity));
		}

		#endregion

		#region RefreshShapeSchedules

		[TestDate(2014, 6, 4)]
		public void TestRefreshShapeSchedules()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			var diagram = CreateDiagram(jobHeader, scheduledStartTimeUTC: ZDateTime.UtcNow);
			var shape = CreateShape(workflow, diagram, "shape");

			var controller = NetworkTestCase.CreateMockableController(Mocks);
			controller.Setup(m => m.EditEntity(It.IsAny<IProposedNetworkEntity>()));

			using (ObjectFactory.Substitute(controller.Object))
			{
				var network = CreateNetwork(diagram);
				network.SwitchToScaled();

				AssertEquals(new ZDateTime(2014, 6, 4), shape.ScheduledStartTimeUtc);

				diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow.AddDays(1);
				AssertEquals(new ZDateTime(2014, 6, 4), shape.ScheduledStartTimeUtc);

				network.EditEntity(shape);
			}

			AssertEquals(new ZDateTime(2014, 6, 5), shape.ScheduledStartTimeUtc);
		}

		[TestDate(2014, 6, 4)]
		public void TestEditEntity_ShouldRefreshShapeSchedules()
		{
			var controller = NetworkTestCase.CreateMockableController(Mocks);
			controller.Setup(m => m.EditEntity(It.IsAny<IProposedNetworkEntity>()));
			using (ObjectFactory.Substitute(controller.Object))
			{
				var jobHeader = CreateJobHeader<OrgHeader>();
				var workflow = jobHeader.ProcessHeaders[0];

				var diagram = CreateDiagram(jobHeader, scheduledStartTimeUTC: ZDateTime.UtcNow);
				var shape = CreateShape(workflow, diagram, "shape");

				var network = CreateNetwork(diagram);
				network.SwitchToScaled();

				AssertEquals(new ZDateTime(2014, 6, 4), shape.ScheduledStartTimeUtc);

				diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow.AddDays(1);
				AssertEquals(new ZDateTime(2014, 6, 4), shape.ScheduledStartTimeUtc);

				network.EditEntity(shape);
				AssertEquals(new ZDateTime(2014, 6, 5), shape.ScheduledStartTimeUtc);
			}
		}

		#endregion

		#region CopiedFromDiagramShapeOnCreation Properties

		public void TestCopiedFromDiagramShapeOnCreationProperties_NewShapesShouldInheritValuesFromDiagramShape()
		{
			var diagram = CreateDiagram(Factory, name: "diagram");
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var shape1 = networkViewModel.CreateNewShape(diagram);

			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();

			var newShape = networkViewModel.CreateNewShape(diagram);

			AssertEquals(true, newShape.IsBuffered);
		}

		[TestDate(2014, 7, 13)]
		public void TestNCNOffset_ShouldInheritFromDiagramShape()
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var workflow1_1 = jobHeader1.ProcessHeaders[0];
			var workflow1_2 = jobHeader1.ProcessHeaders.AddNew();
			var workflow1_3 = jobHeader1.ProcessHeaders.AddNew();

			var diagram1 = CreateDiagram(jobHeader1);
			var shape1_1 = CreateShape(workflow1_1, diagram1);
			var shape1_2 = CreateShape(workflow1_2, diagram1);

			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var diagram2 = CreateDiagram(jobHeader2);

			var controller = NetworkTestCase.CreateMockableController(Mocks);

			controller.Setup(m => m.PickEntity(It.IsAny<ModuleIdentifier>(), It.IsAny<bool>())).Returns(diagram2);
			using (ObjectFactory.Substitute(controller.Object))
			{
				var networkViewModel = CreateNetworkViewModel(diagram1);
				var network = networkViewModel.GetJobNetwork();

				diagram1.NCNReleaseOffsetTime = new ZDateTime(2014, 1, 2, 1, 0, 0);

				AssertEquals(1500, diagram1.NCNReleaseOffsetMinutes);
				AssertEquals(1500, shape1_1.NCNReleaseOffsetMinutes);
				AssertEquals(1500, shape1_2.NCNReleaseOffsetMinutes);

				var newShape = networkViewModel.CreateNewWorkflow(diagram1).Shape;
				AssertEquals(1500, newShape.NCNReleaseOffsetMinutes);

				var shownShape = network.ShowEntity(workflow1_3, diagram1).Single().AsShape();
				AssertEquals(1500, shownShape.NCNReleaseOffsetMinutes);

				var importedShape = network.PickAndImportEntities(diagram1).Single().AsShape();
				AssertEquals(1500, importedShape.NCNReleaseOffsetMinutes);

				diagram1.NCNReleaseOffsetTime = new ZDateTime(2014, 1, 1, 1, 0, 0);

				AssertEquals(60, shape1_1.NCNReleaseOffsetMinutes);
				AssertEquals(60, shape1_2.NCNReleaseOffsetMinutes);
				AssertEquals(60, newShape.NCNReleaseOffsetMinutes);
				AssertEquals(60, shownShape.NCNReleaseOffsetMinutes);
				AssertEquals(60, importedShape.NCNReleaseOffsetMinutes);
			}
		}

		#endregion

		#region Attachment Type

		public void TestAttachmentTypes_DefaultDiagram()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var dependency = workflow1.GetOrCreateDependencyLink(workflow2);

			var defaultDiagram = jobHeader.GetDefaultDiagram();
			var shape1 = defaultDiagram.ChildShapes.Single(s => s.BNS_RelatedEntityID == workflow1.PK);
			var shape2 = defaultDiagram.ChildShapes.Single(s => s.BNS_RelatedEntityID == workflow2.PK);

			var dependencyAttachment = defaultDiagram.ChildDependencyAttachments.Single();

			AssertEquals(AttachmentTypeList.Codes.Dependency, dependencyAttachment.BNA_Type);
		}

		public void TestAttachmentTypes_DiagramCreatedThroughJobNetwork()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var diagram = CreateDiagram(jobHeader);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var shape1 = networkViewModel.CreateNewShape(diagram);
			var shape2 = networkViewModel.CreateNewShape(diagram);
			var shape3 = networkViewModel.CreateNewShape(diagram);
			var dependencyAttachment = network.CreateRelationship(shape1, shape2).AsAttachment();

			new CreateResourceDependencyAction(networkViewModel, DependencyDirection.PostRequisite).ExecuteForShapes(shape3.AsShape(), shape2.AsShape());
			var resourceDependencyArrow = shape3.DependencyAttachments.Single();

			AssertEquals(AttachmentTypeList.Codes.Dependency, dependencyAttachment.BNA_Type);
			AssertEquals(AttachmentTypeList.Codes.ResourceDependency, resourceDependencyArrow.Attachment.BNA_Type);
		}

		#endregion

		#region Paste

		public void TestPerformPaste_MultipleShapesSelected_DontPaste()
		{
			var diagram = CreateDiagram(Factory);
			var shape1 = CreateShape(diagram, "Shape1");
			var shape2 = CreateShape(diagram, "Shape2");
			var jobHeader1 = CreateJobHeader<OrgHeader>();

			var controller = NetworkTestCase.CreateMockableController(Mocks);
			controller.Setup(m => m.GetJobsFromClipboard(It.IsAny<BusinessObjectFactory>())).Returns(new[] { (BusinessObject)jobHeader1.Parent });
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			AssertEquals("Link fails because multiple shapes are selected.", false, network.TryHandlePaste(new[] { shape1, shape2 }));
			AssertEquals("Proof that it would have worked otherwise.", true, network.TryHandlePaste(Array.Empty<INetworkEntity>()));
		}

		public void TestPerformPaste_NoShapeSelected_PasteOnDiagram()
		{
			var diagram = CreateDiagram(Factory);
			var jobHeader1 = CreateJobHeader<OrgHeader>();

			var controller = NetworkTestCase.CreateMockableController(Mocks);
			controller.Setup(m => m.GetJobsFromClipboard(It.IsAny<BusinessObjectFactory>())).Returns(new[] { (BusinessObject)jobHeader1.Parent });
			var network = CreateNetwork(diagram, controller: controller.Object);
			AssertEquals("Link succeeds", true, network.TryHandlePaste(Array.Empty<INetworkEntity>()));
			AssertEquals(jobHeader1, diagram.ProcessHeader);
		}

		public void TestPerformPaste_Succeeds()
		{
			var diagram = CreateDiagram(Factory);
			var shape1 = CreateShape(diagram);
			var jobHeader1 = CreateJobHeader<OrgHeader>();

			var controller = NetworkTestCase.CreateMockableController(Mocks);
			controller.Setup(m => m.GetJobsFromClipboard(It.IsAny<BusinessObjectFactory>())).Returns(new[] { (BusinessObject)jobHeader1.Parent });
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			AssertEquals("Link succeeds", true, network.TryHandlePaste(new[] { shape1 }));
			AssertEquals(jobHeader1, shape1.ProcessHeader);
		}

		public void TestPerformPaste_ClipboardEmpty_Fails()
		{
			var diagram = CreateDiagram(Factory);
			var shape1 = CreateShape(diagram);
			var jobHeader1 = CreateJobHeader<OrgHeader>();

			var controller = NetworkTestCase.CreateMockableController(Mocks);
			controller.Setup(m => m.GetJobsFromClipboard(It.IsAny<BusinessObjectFactory>())).Returns(Enumerable.Empty<BusinessObject>());
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			AssertEquals("Null job", false, network.TryHandlePaste(new[] { shape1 }));
		}

		public void TestPerformPaste_DoNotPaste_WhenMultipleJobsInClipboard()
		{
			var diagram = CreateDiagram(Factory);
			var shape = CreateShape(diagram);
			var jobHeader1 = CreateJobHeader<DummyWithWorkflow>();
			var jobHeader2 = CreateJobHeader<DummyWithWorkflow>();
			var jobs = new[] { jobHeader1.Parent as BusinessObject, jobHeader2.Parent as BusinessObject };

			var controller = NetworkTestCase.CreateMockableController(Mocks);
			controller.Setup(m => m.GetJobsFromClipboard(It.IsAny<BusinessObjectFactory>()))
				.Returns(jobs);
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			AssertEquals(false, network.TryHandlePaste(new[] { shape }));
		}

		public void TestPerformPaste_DoNotPaste_WhenNoDiagramOrDefaultShapeSelected()
		{
			var diagram = CreateDiagram(Factory);

			var diagramShape = CreateShape(ShapeTypeList.Codes.Diagram);
			var defaultShape = CreateShape(ShapeTypeList.Codes.Shape);
			var annotationShape = CreateShape(ShapeTypeList.Codes.Annotation);
			var bufferShape = CreateShape(ShapeTypeList.Codes.Buffer);
			var defaultDiagramShape = CreateShape(ShapeTypeList.Codes.DefaultDiagram);
			var defaultWorkflowShape = CreateShape(ShapeTypeList.Codes.DefaultWorkflow);

			var jobHeader = CreateJobHeader<OrgHeader>();

			var controller = NetworkTestCase.CreateMockableController(Mocks);
			controller.Setup(m => m.GetJobsFromClipboard(It.IsAny<BusinessObjectFactory>()))
				.Returns((jobHeader.Parent as BusinessObject).WrapWithEnumerable());
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			AssertEquals(true, network.TryHandlePaste(new[] { diagramShape }));
			AssertEquals(jobHeader, diagramShape.ProcessHeader);
			AssertEquals(true, network.TryHandlePaste(new[] { defaultShape }));
			AssertEquals(jobHeader, defaultShape.ProcessHeader);

			AssertEquals(false, network.TryHandlePaste(new[] { annotationShape }));
			AssertEquals(false, network.TryHandlePaste(new[] { bufferShape }));
			AssertEquals(false, network.TryHandlePaste(new[] { defaultDiagramShape }));
			AssertEquals(false, network.TryHandlePaste(new[] { defaultWorkflowShape }));
		}

		public void TestPerformPaste_HeaderlessJob_Fails()
		{
			var diagram = CreateDiagram(Factory);
			var shape1 = CreateShape(diagram);
			var headerlessJob = Factory.NewWithValidTestData<OrgHeader>();

			var controller = NetworkTestCase.CreateMockableController(Mocks);
			controller.Setup(m => m.GetJobsFromClipboard(It.IsAny<BusinessObjectFactory>())).Returns(new[] { headerlessJob });
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			AssertEquals("headerless Job", false, network.TryHandlePaste(new[] { shape1 }));
		}

		public void TestPerformPaste_DiagramAlreadyLinked_Fails()
		{
			var diagram = CreateDiagram(Factory);
			var shape1 = CreateShape(diagram);
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var shape2 = CreateShape(jobHeader1, diagram);

			var controller = NetworkTestCase.CreateMockableController(Mocks);
			controller.Setup(m => m.GetJobsFromClipboard(It.IsAny<BusinessObjectFactory>())).Returns(new[] { (BusinessObject)jobHeader1.Parent });
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			AssertEquals("JobHeader already linked on diagram", false, network.TryHandlePaste(new[] { shape1 }));

			AssertEquals(null, shape1.ProcessHeader);
			AssertEquals(jobHeader1, shape2.ProcessHeader);
		}

		#endregion

		#region Copy and Paste Between Diagrams

		public void TestShouldCopyAndPasteShapesAndAnnotations_AndShowNoWarning_WhenOnlyShapesAndAnnotationsBelongToSelection()
		{
			var diagram1 = CreateDiagram(Factory, "Diagram 1");
			var shape1 = CreateShape(diagram1, "Shape 1");
			var shape2 = CreateShape(diagram1, "Shape 2");
			var shape3 = CreateShape(diagram1, "Shape 3");
			var annotation1 = CreateShape(diagram1, "Annotation 1", shapeType: ShapeTypeList.Codes.Annotation);
			var annotation2 = CreateShape(diagram1, "Annotation 2", shapeType: ShapeTypeList.Codes.Annotation);

			var diagram2 = CreateDiagram(Factory, "Diagram 2");

			var networkViewModel1 = CreateNetworkViewModel(diagram1);
			var network1 = networkViewModel1.GetJobNetwork();

			var networkViewModel2 = CreateNetworkViewModel(diagram2);
			var network2 = networkViewModel2.GetJobNetwork();

			network1.TryCopyShapeStateToClipBoard(new[] {
				shape1.AsEntity(network1),
				shape2.AsEntity(network1),
				annotation1.AsEntity(network1),
			});
			network2.PasteShapeFromClipBoard(networkViewModel2);

			AssertContainsExactElementsInAnyOrder("Should copy and paste selected shapes and annotations",
				new[] { "Shape 1", "Shape 2", "Annotation 1" }, network2.Shapes.Select(s => s.BNS_Name));
			AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestShouldCopyAndPasteLinkedShape_AndAssignParent_WhenCopyPastingLinkedShape()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var workItemX = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var diagram = CreateDiagram(jobHeader1);
			var shape = CreateShape(diagram, "WI X");

			var network = CreateNetwork(diagram);
			network.LinkEntity(shape, workItemX);

			AssertIsParent("workItemX should be a child of jobHeader1", workItemX, jobHeader1);

			var otherDiagram = CreateDiagram(jobHeader2);
			var otherNetworkViewModel = CreateNetworkViewModel(otherDiagram);
			var otherNetwork = otherNetworkViewModel.GetJobNetwork();

			network.TryCopyShapeStateToClipBoard(Enumerable.Repeat(shape.AsEntity(network), 1));
			otherNetwork.PasteShapeFromClipBoard(otherNetworkViewModel);

			AssertContainsExactElementsInAnyOrder("Should copy and paste shape",
				new[] { "WI X" }, otherNetwork.Shapes.Select(s => s.BNS_Name));

			AssertIsParent("workItemX should be a child of jobHeader2", workItemX, jobHeader2);
		}

		public void TestShouldCopyAndPasteShapesAndAnnotationsOnly_WithNoBuffers_AndShowWarning_WhenBuffersBelongToSelection()
		{
			var diagram1 = CreateDiagram(Factory, "Diagram 1");
			var shape1 = CreateShape(diagram1, "Shape 1");
			var shape2 = CreateShape(diagram1, "Shape 2");
			var shape3 = CreateShape(diagram1, "Shape 3");
			var annotation1 = CreateShape(diagram1, "Annotation 1", shapeType: ShapeTypeList.Codes.Annotation);
			var annotation2 = CreateShape(diagram1, "Annotation 2", shapeType: ShapeTypeList.Codes.Annotation);
			var buffer1 = CreateShape(diagram1, "Buffer 1", shapeType: ShapeTypeList.Codes.Buffer);

			var diagram2 = CreateDiagram(Factory, "Diagram 2");

			var networkViewModel1 = CreateNetworkViewModel(diagram1);
			var network1 = networkViewModel1.GetJobNetwork();

			var networkViewModel2 = CreateNetworkViewModel(diagram2);
			var network2 = networkViewModel2.GetJobNetwork();

			network1.TryCopyShapeStateToClipBoard(new[] {
				shape1.AsEntity(network1),
				shape2.AsEntity(network1),
				annotation1.AsEntity(network1),
				buffer1.AsEntity(network1),
			});
			network2.PasteShapeFromClipBoard(networkViewModel2);

			AssertContainsExactElementsInAnyOrder("Should copy and paste selected shapes and annotations only with no buffers",
				new[] { "Shape 1", "Shape 2", "Annotation 1" }, network2.Shapes.Select(s => s.BNS_Name));
			AssertEquals("The selection contains buffers. Each buffer relates to its own diagram and cannot be copied to another diagram. Only the selected shapes and/or annotations will be copied.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestShouldAllowPastingOntoTheSameDiagram()
		{
			var diagram1 = CreateDiagram(Factory, "Diagram 1");
			var shape1 = CreateShape(diagram1, "Shape 1");
			var shape2 = CreateShape(diagram1, "Shape 2");
			var shape3 = CreateShape(diagram1, "Shape 3");
			var annotation1 = CreateShape(diagram1, "Annotation 1", shapeType: ShapeTypeList.Codes.Annotation);
			var annotation2 = CreateShape(diagram1, "Annotation 2", shapeType: ShapeTypeList.Codes.Annotation);

			var networkViewModel1 = CreateNetworkViewModel(diagram1);
			var network1 = networkViewModel1.GetJobNetwork();

			network1.TryCopyShapeStateToClipBoard(new[] {
				shape1.AsEntity(network1),
				shape2.AsEntity(network1),
				annotation1.AsEntity(network1)
			});
			network1.PasteShapeFromClipBoard(networkViewModel1);

			AssertContainsExactElementsInAnyOrder("Should allow copying and pasting selected shapes and annotations onto the same diagram",
				new[] { "Shape 1", "Shape 2", "Shape 3", "Annotation 1", "Annotation 2", "Shape 1", "Shape 2", "Annotation 1" }, network1.Shapes.Select(s => s.BNS_Name));
		}

		public void TestShouldNotThrow_WhenCannotPasteChild()
		{
			BMSTestHelper.CreateSystem(Factory, "DUM");
			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeader.GetForParent(dummy, Factory);
			jobHeader.Name = "Job Header";
			var workflow = jobHeader.ProcessHeaders[0];

			var diagram = CreateDiagram(Factory, "Original Diagram");
			var shape1 = CreateShape(workflow, diagram, "Shape 1");

			Factory.Save();

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var extendedDiagram = CreateExtendedDiagram(diagram);

			var extendedNetworkViewModel = CreateNetworkViewModel(extendedDiagram);
			var extendedNetwork = extendedNetworkViewModel.GetJobNetwork();

			AssertContainsExactElementsInAnyOrder("Precondition",
				new[] { "Original Diagram", "Job Header", "Shape 1" }, extendedNetwork.Shapes.Select(s => s.BNS_Name));

			extendedNetwork.TryCopyShapeStateToClipBoard(extendedNetwork.Shapes.Select(s => s.AsEntity(extendedNetwork)));

			AssertNoExceptionThrown(() => network.PasteShapeFromClipBoard(networkViewModel));
		}

		BMNCNShape CreateExtendedDiagram(BMNCNShape originalDiagram)
		{
			var controller = CreateMockableControllerWithMockableInteractionImplementor(Mocks);
			controller
				.Setup(m => m.UserInteractionImplementor.HasUserConfirmed("Are you sure you want to create an extended diagram for the given shape?", "Confirmation Required"))
				.Returns(true);

			var networkViewModel = CreateNetworkViewModel(originalDiagram, controller: controller.Object);
			var action = new ExtendedNetworkAction(networkViewModel);

			action.ExecuteAfterActivatingEntity_ForTest(originalDiagram);

			var factoryForExtendedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForExtendedNetwork);

			var nameForExtendedNetwork = ExtendedNetworkAction.GetNameForExtendedNetwork(originalDiagram);
			var extendedDiagram = factoryForExtendedNetwork.LoadTop1<BMNCNShape>(new ZQuery(BMNCNShapeSchema.BNS_Name, nameForExtendedNetwork));
			return extendedDiagram;
		}

		#endregion

		#region Performance

		public void TestLoadingShapes_ShouldUseRootForeignKey_AndNoRecursiveHits()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var currentShape = (BMNCNShape)diagram;

			const int shapeDepth = 5;

			for (var i = 0; i < shapeDepth; i++)
			{
				currentShape = NetworkTestCase.CreateShape(currentShape);
			}

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedDiagram = newFactory.Load<BMNCNShape>(diagram.PK);

			var network = NetworkTestCase.CreateNetwork(loadedDiagram);

			AssertEquals(shapeDepth, network.Entities.Count);

			AssertDbHits(new Dictionary<string, int>
			{
				{ BMNCNAttachmentSchema.Constants.TableName, 2 }, // One actual hit, and one for the fetch hints. This far and no further.
				{ BMNCNShapeSchema.Constants.TableName, 3 }, // One for the diagram, and one for its descendants and all descendants' children.
			}, newFactory);
		}

		public void TestNetworkWithNoShapes_ShouldNotLoadEveryWorkflowAndDependencyLink()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			for (var i = 0; i < 10; i++)
			{
				var workflow = BMSTestHelper.CreateWorkflow(jobHeader1, i.ToString());
				jobHeader2.GetOrCreateDependencyLink(workflow);
			}

			var diagram = NetworkTestCase.CreateDiagram(Factory);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedDiagram = newFactory.Load<BMNCNShape>(diagram.PK);
			var network = NetworkTestCase.CreateNetwork(loadedDiagram);

			AssertEquals(0, network.Entities.Count);

			AssertEquals("Loading every row in this table would be a bad idea", 0, ((IBusinessObjectFactoryInternals)newFactory).AllBusinessObjects.OfType<ProcessHeader>().Count());
			AssertEquals("Loading every row in this table would be a bad idea", 0, ((IBusinessObjectFactoryInternals)newFactory).AllBusinessObjects.OfType<ProcessHeaderLink>().Count());
		}

		public void TestNetworkWithShapeNotLinkedToRealEntity_ShouldNotLoadEveryWorkflowAndDependencyLink()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			for (var i = 0; i < 10; i++)
			{
				var workflow = BMSTestHelper.CreateWorkflow(jobHeader1, i.ToString());
				jobHeader2.GetOrCreateDependencyLink(workflow);
			}

			var diagram = NetworkTestCase.CreateDiagram(Factory);
			NetworkTestCase.CreateShape(diagram);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedDiagram = newFactory.Load<BMNCNShape>(diagram.PK);
			var network = NetworkTestCase.CreateNetwork(loadedDiagram);

			AssertEquals(1, network.Entities.Count);

			AssertEquals("Loading every row in this table would be a bad idea", 0, ((IBusinessObjectFactoryInternals)newFactory).AllBusinessObjects.OfType<ProcessHeader>().Count());
			AssertEquals("Loading every row in this table would be a bad idea", 0, ((IBusinessObjectFactoryInternals)newFactory).AllBusinessObjects.OfType<ProcessHeaderLink>().Count());
		}

		public void TestNetworkWithShapeLinkedToRealEntity_ShouldNotLoadEveryWorkflowAndDependencyLink()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			for (var i = 0; i < 10; i++)
			{
				var workflow = BMSTestHelper.CreateWorkflow(jobHeader1, i.ToString());
				jobHeader2.GetOrCreateDependencyLink(workflow);
			}

			var diagram = NetworkTestCase.CreateDiagram(Factory);
			NetworkTestCase.CreateShape(jobHeader3, diagram);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedDiagram = newFactory.Load<BMNCNShape>(diagram.PK);
			var network = NetworkTestCase.CreateNetwork(loadedDiagram);

			AssertEquals(1, network.Entities.Count);

			AssertEquals("Loading every row in this table would be a bad idea", 1, ((IBusinessObjectFactoryInternals)newFactory).AllBusinessObjects.OfType<ProcessHeader>().Count());
			AssertEquals("Loading every row in this table would be a bad idea", 0, ((IBusinessObjectFactoryInternals)newFactory).AllBusinessObjects.OfType<ProcessHeaderLink>().Count());
		}

		public void TestDbHits_CompletionStatementTasks()
		{
			const int workflowCount = 50;
			const int completionStatementTaskCount = 3;

			BMSTestHelper.MakeCompletionStatementTaskType("ORG", "COM");

			var diagram = CreateDiagram(Factory);

			for (var i = 0; i < workflowCount; i++)
			{
				var jobHeader = i % 2 == 0
					? CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false)
					: CreateJobHeader<SalesEnquiry>(addDefaultProcessHeaderIfNone: false);
				var workflow = CreateWorkflow(jobHeader, "Shäpe " + i);

				for (var j = 0; j < completionStatementTaskCount; j++)
				{
					CreateTask(workflow, string.Empty, 0, "COM");
				}

				CreateShape(workflow, diagram);
			}

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedDiagram = newFactory.Load<BMNCNShape>(diagram.PK);
			var network = CreateNetwork(loadedDiagram);

			var criteria = network.Entities.Select(s => s.CompletionCriteria).ToArray();

			AssertEquals(workflowCount, criteria.Length);
			AssertDbHits(new Dictionary<string, int>
			{
				{ BMNCNAttachmentSchema.Constants.TableName, 2 },
				{ BMNCNShapeSchema.Constants.TableName, 3 },
				{ OrgColdCallRegisterSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ ProcessHeaderSchema.Constants.TableName, 1 },
				{ ProcessHeaderLinkSchema.Constants.TableName, 2 },
				{ ProcessTasksSchema.Constants.TableName, 2 },
			}, newFactory);
		}

		#endregion

		#region SwitchToScaled

		[TestDate(2015, 7, 14)]
		public void TestSwitchToScaled_ShouldNotOverwriteExistingDiagramDates()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow;
			diagram.ScheduledFinishTimeUtc = ZDateTime.UtcNow.AddSeconds(1);

			jobHeader.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddDays(-1);
			jobHeader.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddDays(-1).AddSeconds(1);

			var network = CreateNetwork(diagram);

			network.LinkEntity(diagram, jobHeader);

			AssertEquals(ZDateTime.UtcNow, diagram.ScheduledStartTimeUtc);
			AssertEquals(ZDateTime.UtcNow.AddSeconds(1), diagram.ScheduledFinishTimeUtc);

			JobNetwork.SwitchToScaled(diagram);

			AssertEquals(ZDateTime.UtcNow, diagram.ScheduledStartTimeUtc);
			AssertEquals(ZDateTime.UtcNow.AddSeconds(1), diagram.ScheduledFinishTimeUtc);
		}

		#endregion

		#region Validation

		public void TestShouldValidateAndPass_WhenThereAreNoErrors()
		{
			var diagram = CreateDiagram(Factory);

			var validator = new Mock<IJobNetworkValidator>();
			var networkViewModel = CreateNetworkViewModel(diagram, validator: validator.Object);
			var network = networkViewModel.GetJobNetwork();

			validator.Setup(m => m.Validate(network));
			Assert("Validation should pass", network.ValidateAndCheckThereAreNoErrors());
			Assert("There should be no validation error on shapes", !network.DiagramShape.HasErrors);
			Assert("There should be no validation error on shape entities", !network.DiagramEntity.HasErrors);

			AssertNull("Should validate without notifications", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestShouldValidateAndPass_WhenThereAreWarningsOnly()
		{
			var diagram = CreateDiagram(Factory);

			var validator = new Mock<IJobNetworkValidator>();
			var networkViewModel = CreateNetworkViewModel(diagram, validator: validator.Object);
			var network = networkViewModel.GetJobNetwork();

			var shape = CreateShape(diagram);
			var entity = shape.AsEntity(network);

			validator.Setup(m => m.Validate(It.IsAny<IJobNetwork>())).Callback(new Action<IJobNetwork>((n) =>
			{
				AssertEquals(network, n);
				shape.AddRowWarning("This is a way to emulate adding a warning on a shape during validation");
				entity.AddRowWarning("This is a way to emulate adding a warning on an entity during validation");
			}));
			Assert("Validation should pass despite the warnings", network.ValidateAndCheckThereAreNoErrors());
			Assert("There should be validation warnings on shapes", network.DiagramShape.HasWarnings);
			Assert("There should be validation warnings on shape entities", network.DiagramEntity.HasWarnings);
			Assert("There should be no validation errors on shapes", !network.DiagramShape.HasErrors);
			Assert("There should be no validation errors on shape entities", !network.DiagramEntity.HasErrors);

			AssertNull("Should validate without notifications", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestShouldValidateAndNotPass_WhenThereAreErrorsOnShapes()
		{
			var diagram = CreateDiagram(Factory);

			var validator = new Mock<IJobNetworkValidator>();
			var networkViewModel = CreateNetworkViewModel(diagram, validator: validator.Object);
			var network = networkViewModel.GetJobNetwork();

			var shape = CreateShape(diagram);

			validator.Setup(m => m.Validate(It.IsAny<IJobNetwork>())).Callback(new Action<IJobNetwork>((n) =>
			{
				AssertEquals(network, n);
				shape.AddRowError("This is a way to emulate adding an error on a shape during validation");
			}));
			Assert("Validation should not pass", !network.ValidateAndCheckThereAreNoErrors());
			Assert("There should be validation errors on shapes", network.DiagramShape.HasErrors);
			Assert("There should be no validation errors on shape entities", !network.DiagramEntity.HasErrors);

			AssertNull("Should validate without notifications", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestShouldValidateAndNotPass_WhenThereAreErrorsOnEntities()
		{
			var diagram = CreateDiagram(Factory);

			var validator = new Mock<IJobNetworkValidator>();
			var networkViewModel = CreateNetworkViewModel(diagram, validator: validator.Object);
			var network = networkViewModel.GetJobNetwork();

			var shape = CreateShape(diagram);
			var entity = shape.AsEntity(network);

			validator.Setup(m => m.Validate(It.IsAny<IJobNetwork>())).Callback(new Action<IJobNetwork>((n) =>
			{
				AssertEquals(network, n);
				entity.AddRowError("This is a way to emulate adding an error on an entity during validation");
			}));
			Assert("Validation should not pass", !network.ValidateAndCheckThereAreNoErrors());
			Assert("There should be no validation errors on shapes", !network.DiagramShape.HasErrors);
			Assert("There should be validation errors on shape entities", network.DiagramEntity.HasErrors);

			AssertNull("Should validate without notifications", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region Saving

		[TestDate(2015, 7, 14)]
		public void TestSaved_ShouldRefreshScale()
		{
			var diagram = CreateDiagram(Factory);
			var network = CreateNetwork(diagram);

			diagram.ScheduledStartTimeLocal = ZDateTime.Now;

			network.SwitchToScaled();

			var scaleSet = network.ScaleDescriptor.GetScaleSetForColumns(3);

			CombineAssertions("Original scale values", () =>
			{
				AssertEquals("14-Jul-15 00:00", scaleSet.ScalePoints[0].Label);
				AssertEquals("14-Jul-15 08:00", scaleSet.ScalePoints[1].Label);
				AssertEquals("14-Jul-15 16:00", scaleSet.ScalePoints[2].Label);
			});

			diagram.ScheduledStartTimeLocal = diagram.ScheduledStartTimeLocal.AddDays(1);
			scaleSet = network.ScaleDescriptor.GetScaleSetForColumns(3);

			CombineAssertions("We've not refreshed the diagram yet, so original scale values should still be present", () =>
			{
				AssertEquals("14-Jul-15 00:00", scaleSet.ScalePoints[0].Label);
				AssertEquals("14-Jul-15 08:00", scaleSet.ScalePoints[1].Label);
				AssertEquals("14-Jul-15 16:00", scaleSet.ScalePoints[2].Label);
			});

			network.Refresh(RefreshType.Saved);
			scaleSet = network.ScaleDescriptor.GetScaleSetForColumns(3);

			CombineAssertions("We've now refreshed the diagram, so scale values should be updated", () =>
			{
				AssertEquals("15-Jul-15 00:00", scaleSet.ScalePoints[0].Label);
				AssertEquals("15-Jul-15 08:00", scaleSet.ScalePoints[1].Label);
				AssertEquals("15-Jul-15 16:00", scaleSet.ScalePoints[2].Label);
			});
		}

		#endregion
	}
}
