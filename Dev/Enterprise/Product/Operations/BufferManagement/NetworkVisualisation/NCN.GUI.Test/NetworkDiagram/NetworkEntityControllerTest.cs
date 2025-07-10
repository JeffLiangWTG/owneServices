using System;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.VisualBoards.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	class NetworkEntityControllerTest : NetworkGUITestCase
	{
		#region Create Job

		public void TestCreateJobSynchronously_DefaultDialogResult()
		{
			var controller = CreateController();
			AssertNotNull(controller.CreateJob("ORG", Factory));
		}

		public void TestCreateJobSynchronously()
		{
			var controller = CreateController();
			var fireSaveButtonInvoker = new ZFormModaliser.PreShowInvoker(form =>
			{
				var zForm = (ZForm)form;
				((BusinessObject)zForm.BusinessEntity).FillWithValidTestData();
				zForm.FireSaveButton();
			});
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(fireSaveButtonInvoker);

			AssertNotNull(controller.CreateJob("INQ", Factory));
		}

		#endregion

		#region FireSave

		public void TestSetsName_ShapeWithNameFillsJobNameAutomatically()
		{
			var shape = CreateDiagram(Factory, name: "Workitem Name");
			shape.BNS_JobType = "WKI";

			var networkViewModel = CreateNetworkViewModel(shape);
			var network = networkViewModel.GetJobNetwork();

			Factory.Save();

			var fireSaveButtonInvoker = new ZFormModaliser.PreShowInvoker(form =>
			{
				var zForm = (ZForm)form;
				var bizo = ((BusinessObject)zForm.DataSource);

				zForm.FireSaveButton();
			});
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(fireSaveButtonInvoker);

			var action = new CreateJobAction(networkViewModel, linkOnly: true);
			action.ExecuteForEntityWithoutAccessCheck(shape);

			var jobHeader = (ProcessJobHeader)shape.ProcessHeader;
			AssertNotNull(jobHeader);
			AssertNotNull(jobHeader.Parent);
			AssertEquals("name should be the name of the shape", "Workitem Name", ((IProposedNetworkEntity)jobHeader).JobName);
		}

		public void TestSetsName_UserChangesNameInFormAndShapeAssumesNewName()
		{
			var shape = CreateDiagram(Factory, name: "Workitem Name");
			shape.BNS_JobType = "WKI";
			var networkViewModel = CreateNetworkViewModel(shape);
			var network = networkViewModel.GetJobNetwork();

			Factory.Save();

			var fireSaveButtonInvoker = new ZFormModaliser.PreShowInvoker(form =>
			{
				var zForm = (ZForm)form;
				var bizo = ((BusinessObject)zForm.DataSource);
				bizo.FindPropertyInfo(WorkItemSchema.Constants.WKI_Summary).PropertyDescriptor.SetValue(bizo, new ZString("Swag"));

				zForm.FireSaveButton();
			});
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(fireSaveButtonInvoker);

			var action = new CreateJobAction(networkViewModel, linkOnly: true);
			action.ExecuteForEntityWithoutAccessCheck(shape);

			var jobHeader = (ProcessJobHeader)shape.ProcessHeader;
			AssertNotNull(jobHeader);
			AssertNotNull(jobHeader.Parent);
			AssertEquals("name should be the name of the shape", "Swag", ((IProposedNetworkEntity)jobHeader).JobName);
		}

		public void TestFireSave_FiresSaveButton()
		{
			using (Globals.SetIsWinzorForTest(false))
			{
				var diagram1 = CreateDiagram(Factory, name: "diagram1");
				var diagram2 = CreateDiagram(Factory, name: "diagram2");
				var section = CreateNetworkBoardSection(diagram1);
				var viewModel = CreateBoardSectionViewModel(section, isPreview: true);

				Factory.Save();

				using (DisableAsyncBehaviour())
				{
					using (var form = new VisualBoardForm(VisualBoardsTestHelper.CreateSlideshowViewModel(section.Board)))
					{
						form.Show();

						var networkControl = form.FindAll<BoardSectionDiagramControl>().Single();
						AssertNotNull(networkControl.Network);
						AssertEquals(diagram1.PK, networkControl.Network.DiagramEntity.PK);

						((DiagramBoardSectionConfiguration)section.Configuration).DiagramPK = diagram2.PK;
						Factory.Save();

						form.ReloadBoard();
						Application.DoEvents();

						AssertEquals(true, networkControl.IsDisposed);

						networkControl = form.FindAll<BoardSectionDiagramControl>().Single();
						AssertEquals(diagram2.PK, networkControl.Network.DiagramEntity.PK);
					}
				}
			}
		}

		#endregion

		#region ViewDiagram

		public void TestViewDiagram_SavedAndHasNoChanges()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();

			var diagram = CreateDiagram(jobHeader1);
			var subDiagram = CreateShape(jobHeader2, diagram);

			Factory.Save();

			var controller = CreateController();
			controller.ViewDiagram(subDiagram);

			using (var form = Application.OpenForms.OfType<NetworkDiagramForm>().SingleOrDefault())
			{
				AssertNotNull(form);
			}
		}

		public void TestViewDiagram_NotSaved()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();

			var diagram = CreateDiagram(jobHeader1);
			var subDiagram = CreateShape(jobHeader2, diagram);

			AssertEquals(true, subDiagram.HasChanges);
			AssertEquals(false, subDiagram.IsInDatabase);

			var controller = CreateController();
			controller.ViewDiagram(subDiagram);

			using (var form = Application.OpenForms.OfType<NetworkDiagramForm>().SingleOrDefault())
			{
				AssertEquals("Should still not be saved", true, subDiagram.HasChanges);
				AssertNull("Should not show any notifications to the user", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertNotNull(form);
			}
		}

		public void TestViewDiagram_SavedButHasChanges()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();

			var diagram = CreateDiagram(jobHeader1);
			var subDiagram = CreateShape(jobHeader2, diagram);

			Factory.Save();

			((IProposedNetworkEntity)subDiagram).Name = "Dat Sub-Diagram";
			AssertEquals(true, subDiagram.HasChanges);

			var controller = CreateController();
			AssertExceptionThrown<InvalidOperationException>(() => controller.ViewDiagram(subDiagram));

			using (var form = Application.OpenForms.OfType<NetworkDiagramForm>().SingleOrDefault())
			{
				AssertNull(form);
			}
		}

		public void TestViewDiagram_NotADiagram()
		{
			var diagram = CreateDiagram(Factory);
			var shape = CreateShape(diagram);

			Factory.Save();

			var controller = CreateController();
			controller.ViewDiagram(shape);

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			using (var form = Application.OpenForms.OfType<NetworkDiagramForm>().SingleOrDefault())
			{
				AssertNotNull(form);
			}
		}

		#endregion

		#region OpenLinkedEntity

		public void TestOpenLinkedEntity()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Workflow");
			var diagram = CreateDiagram(jobHeader);

			Factory.Save();

			var controller = CreateController();
			controller.OpenLinkedEntity(jobHeader, ControllerIDs.ProcessHeader);
			Application.DoEvents();

			using (var form = Application.OpenForms.OfType<ZOrganisationsForm>().FirstOrDefault())
			{
				AssertNotNull(form);
				var grid = form.FindSingle<ZGrid>(x => x.Name == "WorkflowsGrid");
				var selected = (BusinessObject)grid.ListManager.Current;

				AssertEquals("The job header should have been selected instead of the workflow, and yet...", jobHeader.PK, selected.PK);
			}
		}

		public void TestOpenLinkedEntity_NoLinkedJob()
		{
			var diagram = CreateDiagram(Factory);

			Factory.Save();

			var openFormsCount = Application.OpenForms.Count;
			var controller = CreateController();
			controller.OpenLinkedEntity(null, null);

			AssertEquals(openFormsCount, Application.OpenForms.Count);
			AssertEquals("This shape is not linked to a job, workflow or another diagram. You can link this shape from the Linked Entity option, or by copying and pasting a hyperlink for an entity directly onto the shape.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region Double Click
#if !WINZOR
		public void TestDoubleClick()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();

			var org1 = Factory.Load<OrgHeader>(jobHeader1.FH_ParentId);
			var org2 = Factory.Load<OrgHeader>(jobHeader2.FH_ParentId);

			var diagram = NetworkTestCase.CreateNetwork(NetworkTestCase.CreateDiagram(Factory)).DiagramEntity;
			var diagram1 = NetworkTestCase.CreateShapeAtLocation(jobHeader1, diagram, 10, 10, 200, 200, "diagram1").Shape;
			var diagram2 = NetworkTestCase.CreateShapeAtLocation(jobHeader2, diagram, 210, 210, 200, 200, "diagram2").Shape;

			var network = NetworkTestCase.CreateNetwork(diagram.Shape);
			Factory.Save();

			using (var form = (NetworkDiagramForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagram))
			{
				form.Show();
				var initialOpenFormsCount = Application.OpenForms.Count;

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				Application.DoEvents();

				DoubleClickNode(null, form);
				DoubleClickNode(diagram1, form);

				using (var formOrg = Application.OpenForms.OfType<ZOrganisationsForm>().FirstOrDefault())
				{
					AssertContains(org1.OH_Code, formOrg.Text);
					AssertNull("There should be no notification with respect to job or workflow linkage", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				diagram1.ProcessJobHeader.FH_ParentId = ZGuid.Empty;
				Factory.Save();

				DoubleClickNode(diagram1, form);

				using (var formOrg = Application.OpenForms.OfType<ZOrganisationsForm>().FirstOrDefault())
				{
					AssertContains(@"Unable to find the job this workflow belongs to.
Technical details: parent table code = OH", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertContains("Unable to show the form. Task is null. WorkflowProvider is null", ErrorReporter.LastMessageReported);
				}

				ErrorReporter.Clear();
				UnitTestUserNotification.Instance.ClearMessages();

				DoubleClickNode(diagram2, form);

				using (var formOrg = Application.OpenForms.OfType<ZOrganisationsForm>().FirstOrDefault())
				{
					AssertContains(org2.OH_Code, formOrg.Text);
					AssertNull("There should be no notification with respect to job or workflow linkage", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				var textControl = FindNode(diagram2, form).FindChildren<TextBoxWithPlaceholder>().LastOrDefault();

				DoubleClickNode(diagram2, form, textControl);

				AssertEquals("No forms should be opened since the double click took place within a text area.", initialOpenFormsCount, Application.OpenForms.Count);
				AssertNull("There should be no notification with respect to job or workflow linkage", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
#endif
		#endregion

		#region PickDiagramToImport

		public void TestPickDiagramToImport()
		{
			var diagram1 = CreateJobAndDiagram(Factory);
			var diagram2 = CreateJobAndDiagram(Factory);
			var diagram3 = CreateJobAndDiagram(Factory);
			var network = CreateNetwork(diagram1);

			using (BusinessObjectModulePickerTest.SelectRecordsOnDialogShown(diagram2))
			{
				var controller = CreateController();
				var result = controller.PickEntity(ModuleIDs.NetworkDiagram);
				AssertEquals(diagram2, result);
			}
		}

		public void TestPickAndImportEntity()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var workflow1_1 = jobHeader1.ProcessHeaders[0];
			var workflow1_2 = jobHeader1.ProcessHeaders.AddNew();
			var link11_12 = workflow1_1.GetOrCreateDependencyLink(workflow1_2);

			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var workflow2_1 = jobHeader2.ProcessHeaders[0];
			var workflow2_2 = jobHeader2.ProcessHeaders.AddNew();
			var link21_22 = workflow2_1.GetOrCreateDependencyLink(workflow2_2);

			var diagram1 = CreateNetwork(jobHeader1.GetDefaultDiagram()).DiagramEntity;
			AssertEquals(2, diagram1.Children.Count());
			AssertEquals(1, diagram1.Shape.AllAttachments.Count(a => a.BNA_Type == NetworkVisualisation.Business.AttachmentTypeList.Codes.Dependency));
			var diagram2 = CreateNetwork(jobHeader2.GetDefaultDiagram()).DiagramEntity;
			AssertEquals(2, diagram2.Children.Count());
			AssertEquals(1, diagram2.Shape.AllAttachments.Count(a => a.BNA_Type == NetworkVisualisation.Business.AttachmentTypeList.Codes.Dependency));

			var workflow2_1Shape = diagram2.Children.First(s => s.RelatedEntityPK == workflow2_1.PK);
			var workflow2_2Shape = diagram2.Children.First(s => s.RelatedEntityPK == workflow2_2.PK);
			workflow2_1Shape.Y = 20;
			workflow2_2Shape.Y = 40;
			workflow2_1Shape.X = 120;
			workflow2_2Shape.X = 140;

			Factory.Save();

			var diagram2InNewFactory = new BusinessObjectFactory().Load<BMNCNShape>(diagram2.PK);
			var network = CreateNetwork(diagram1.Shape);

			AssertEquals(2, network.Entities.Count);

			using (BusinessObjectModulePickerTest.SelectRecordsOnDialogShown(diagram2InNewFactory))
			{
				var result = (ShapeNetworkEntity)network.PickAndImportEntities(diagram1).First();
				AssertNotEquals("Should have created a new shape when importing", diagram2.PK, result.PK);
				AssertNotEquals("Should reload shape in network diagram's factory", diagram2InNewFactory, result);
				AssertEquals("Should reload shape in network diagram's factory", diagram1.Factory, result.Factory);

				AssertDiagramHasChildShape(diagram1.Shape, result.Shape);
				AssertEquals(jobHeader2.PK, result.RelatedEntityPK);

				AssertEquals("Network diagram should have a new child shape for the copied diagram", 3, diagram1.Children.Count());
				AssertEquals(1, diagram1.Shape.AllAttachments.Count(a => a.BNA_Type == NetworkVisualisation.Business.AttachmentTypeList.Codes.Dependency));

				AssertEquals("Copied diagram should still have its own child shapes so it can be opened separately", 2, result.Children.Count());
				AssertEquals(1, result.Shape.AllAttachments.Count(a => a.BNA_Type == NetworkVisualisation.Business.AttachmentTypeList.Codes.Dependency));

				workflow2_1Shape = result.Children.First(s => s.RelatedEntityPK == workflow2_1.PK);
				workflow2_2Shape = result.Children.First(s => s.RelatedEntityPK == workflow2_2.PK);

				AssertEquals("Network should have two workflow entities for the network diagram, one for the copied sub-diagram, and two for the sub-diagram's workflows", 5, network.Entities.Count);
			}
		}

		#endregion

		#region LinkEntity

		public void TestLinkEntity_ForVisibleShape_ShouldMoveParentChildLinks()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var childJobHeader = CreateJobHeader<OrgHeader>();

			VisualBoardsTestHelper.MakeChildOf(childJobHeader, jobHeader1);

			var diagram = CreateDiagram(jobHeader1, name: "diagram");
			var subDiagram = CreateShape(childJobHeader, diagram, "subDiagram");
			var network = CreateNetwork(diagram);

			using (BusinessObjectModulePickerTest.SelectRecordsOnDialogShown(jobHeader2))
			{
				AssertIsParent(childJobHeader, jobHeader1);
				AssertIsNotParent(childJobHeader, jobHeader2);

				network.LinkEntity(diagram, ModuleIDs.ProcessHeader);

				AssertIsParent(childJobHeader, jobHeader2);
				AssertIsNotParent(childJobHeader, jobHeader1);
			}
		}

		public void TestLinkEntity_NoVisibleShape_ShouldNotMoveParentChildLinks()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var childJobHeader = CreateJobHeader<OrgHeader>();

			var parentChildLink = BMSTestHelper.MakeChildOfAndGetLink(childJobHeader, jobHeader1);

			var diagram = CreateDiagram(jobHeader1);
			var network = CreateNetwork(diagram);

			using (BusinessObjectModulePickerTest.SelectRecordsOnDialogShown(jobHeader2))
			{
				network.LinkEntity(diagram, ModuleIDs.ProcessHeader);

				AssertEquals(jobHeader2.PK, diagram.BNS_RelatedEntityID);
				AssertEquals(jobHeader1.PK, parentChildLink.FP_FH_HeaderTo);
			}
		}

		#endregion

		#region UnlinkEntity

		public void TestUnlinkEntity_HandlesBufferAndAnnotationShapes()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var diagram = CreateDiagram(jobHeader1);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var buffer = CreateShape(diagram, shapeType: ShapeTypeList.Codes.Buffer);

			var annotation = networkViewModel.CreateNewAnnotation(diagram);

			AssertNoExceptionThrown(() => network.UnlinkEntity(diagram));
		}

		public void TestUnlinkEntity_DeleteExistingLinks()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow1 = CreateWorkflow(jobHeader, "w1");

			var childJobHeader = CreateJobHeader<OrgHeader>(false);

			var diagram1 = CreateDiagram(jobHeader, name: "Homeless");
			var networkViewModel = CreateNetworkViewModel(diagram1);
			var network = networkViewModel.GetJobNetwork();

			var shape1 = CreateShape(workflow1, diagram1, name: "Phoneless");
			var shape2 = networkViewModel.CreateNewShape(diagram1);
			network.LinkEntity(shape2, childJobHeader);

			var depAttachment = (NetworkAttachment)network.CreateRelationship(shape1, shape2);

			AssertNotNull(depAttachment);

			var depLink = workflow1.Links.Single(a => a.FP_LinkType == ProcessHeaderLinkTypeList.Codes.Dependency);
			var pchLink = childJobHeader.Links.Single(a => a.FP_LinkType == ProcessHeaderLinkTypeList.Codes.ParentChild);

			AssertEquals(depLink.PK, depAttachment.Attachment.BNA_FP_ProcessHeaderLink);

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(f =>
			{
				var form = (ZForm)f;
				form.Tag = UnlinkEntityVariant.DeleteHeaderLinks;
			});

			network.UnlinkEntity(shape2);

			AssertEquals("Unlink should definitely clear the existing link", ZGuid.Empty, depAttachment.Attachment.BNA_FP_ProcessHeaderLink);
			AssertEquals(true, depLink.IsDeleted);
			AssertEquals(true, pchLink.IsDeleted);
		}

		public void TestUnlinkEntity_PersistExistingLinks()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow1 = CreateWorkflow(jobHeader, "w1");

			var childJobHeader = CreateJobHeader<OrgHeader>(false);
			var diagramShape1 = CreateDiagram(jobHeader, name: "Homeless");
			var networkViewModel = CreateNetworkViewModel(diagramShape1);
			var network = networkViewModel.GetJobNetwork();
			var diagram1 = network.DiagramEntity;
			var shape1 = CreateShape(workflow1, diagram1, name: "Phoneless");
			var shape2 = networkViewModel.CreateNewShape(diagram1);
			network.LinkEntity(shape2, childJobHeader);

			var depAttachment = (NetworkAttachment)network.CreateRelationship(shape1, shape2);

			AssertNotNull(depAttachment);

			var depLink = workflow1.Links.Single(a => a.FP_LinkType == ProcessHeaderLinkTypeList.Codes.Dependency);
			var pchLink = childJobHeader.Links.Single(a => a.FP_LinkType == ProcessHeaderLinkTypeList.Codes.ParentChild);

			AssertEquals(depLink.PK, depAttachment.Attachment.BNA_FP_ProcessHeaderLink);

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(f =>
			{
				var form = (ZForm)f;
				form.Tag = UnlinkEntityVariant.PersistHeaderLinks;
			});
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			network.UnlinkEntity(shape2);

			AssertEquals("Unlink should definitely clear the existing link", ZGuid.Empty, depAttachment.Attachment.BNA_FP_ProcessHeaderLink);
			AssertEquals(false, depLink.IsDeleted);
			AssertEquals(false, pchLink.IsDeleted);
		}

		#endregion

		#region GetJobFromClipBoard

		public void TestGetJobFromClipboardWithClassicLoaderHyperlinks()
		{
			DataRegistry.Instance.WebHyperlinksEnabled = false;

			TestGetJobFromClipboardCore();
		}

		public void TestGetJobFromClipboardWithWebHyperlinks()
		{
			DataRegistry.Instance.WebHyperlinksEnabled = true;
			WebDataRegistry.Instance.RootServicesUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://cw1.web.services/foo");

			TestGetJobFromClipboardCore();
		}

		void TestGetJobFromClipboardCore()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();

			Factory.Save();

			using (var orgForm = (ZForm)ZControllerFactory.Create(ControllerIDs.Organisation).ShowEditForm((BusinessObject)jobHeader.Parent))
			using (new ClipboardTestHelper().MockClipboard())
			{
				ZFormMenuStrategy.CopyHyperlinkToClipboard(orgForm);

				AssertArrayEqualsByElements(new[] { jobHeader.Parent }, CreateController().GetJobsFromClipboard(Factory).ToArray());
			}
		}

		public void TestGetJobFromClipboard_ClearClipBoard()
		{
			using (new ClipboardTestHelper().MockClipboard())
			{
				AssertEquals(false, CreateController().GetJobsFromClipboard(Factory).Any());
			}
		}

		public void TestGetJobFromClipboard_JobDoesNotExist()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (var orgForm = (ZForm)ZControllerFactory.Create(ControllerIDs.Organisation).ShowEditForm(org))
			using (new ClipboardTestHelper().MockClipboard())
			{
				ZFormMenuStrategy.CopyHyperlinkToClipboard(orgForm);

				org.Delete();
				Factory.Save();

				AssertEquals(false, CreateController().GetJobsFromClipboard(Factory).Any());
			}
		}

		#endregion

		#region Progress Notification

		public void TestApproveDiagram_ShouldShowProgressUpdater()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			NetworkTestCase.CreateShape(diagram);
			NetworkTestCase.CreateShape(diagram);

			Factory.Save();

			using (var form = (NetworkDiagramForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				var networkViewModel = form.NetworkViewModel;
				var network = form.Network;

				AssertNull(ZFormModaliser.LastFormShownForTest);

				var toggleApprovalMenuItem = networkViewModel.GetSecondLevelNetworkActionsMenuItem_ForTesting(diagram, "Actions", "Approve Diagram");
				AssertNotNull(toggleApprovalMenuItem);
				toggleApprovalMenuItem.Action.Execute();

				AssertNotNull(ZFormModaliser.LastFormShownForTest);
				AssertType<ProgressForm>(ZFormModaliser.LastFormShownForTest);
			}
		}

		#endregion
	}
}
