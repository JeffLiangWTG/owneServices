using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.GUI.Test;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.GUI;
using Enterprise.BufferManagement.GUI.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ProcessManagement.Integration;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Application = System.Windows.Forms.Application;
using TextBox = System.Windows.Controls.TextBox;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	[TestedType(typeof(NetworkDiagramForm))]
	class NetworkDiagramFormTest : ZFormBasherTest
	{
		#region Open as Diagram

		public void TestOpenAsDiagram_WhenDiagramShapeDeletedInAnotherForm_ShouldInformUserAndCloseForm()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "Jango Fett");
			var shape = NetworkTestCase.CreateShape(diagram, name: "Boba Fett");

			Factory.Save();

			using (var diagramForm = new NetworkDiagramForm(diagram))
			{
				diagramForm.Show();
				Application.DoEvents();

				using (var childShapeForm = NetworkGUITestCase.FindAndClickOpenAsDiagramMenuItem(diagramForm, shape))
				{
					diagramForm.BringToFront();

					var wasShapeDeleted = diagramForm.NetworkViewModel.Network.DeleteEntity(shape);
					var wasFormSaved = diagramForm.FireSaveButton();

					VisualBoardsTestCase.AssertSaved(wasFormSaved, childShapeForm.BusinessEntity);
					AssertEquals("wasShapeDeleted", true, wasShapeDeleted);
					AssertEquals("shape.IsDeleted", true, shape.IsDeleted);

					AssertEquals("Precondition: child shape form should still be shown", false, childShapeForm.IsDisposed);
					UnitTestUserNotification.Instance.ClearMessages(); // Ensures we aren't testing the other messages that have appeared during this test.

					childShapeForm.BringToFront();
					NetworkGUITestCase.DoEventsThoroughly();

					AssertEquals("Now that the user has attempted to go back to a form for an already-deleted diagram, architecture code should handle the situation and close the form.", "This record has been deleted in other form and cannot be used further. This form will be closed.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(true, childShapeForm.IsDisposed);
				}
			}

			AssertNullOrEmpty("ErrorReporter.LastMessageReported", ErrorReporter.LastMessageReported);
			AssertNull("Shape deletion should have been persisted", Factory.CreateNewFactory().Load<BMNCNShape>(shape.PK));
		}

		public void TestConcurrentDiagramsOpenWithShapeWithAttachmentApprovedInAnotherFactoryThenDelete_ShouldNotDieHorribly()
		{
			DiagramShapeWithAttachmentApprovedHelper((network, shape) => network.DeleteEntity(shape));
		}

		public void TestConcurrentDiagramsOpenWithShapeWithAttachmentApprovedInAnotherFactoryThenHide_ShouldNotDieHorribly()
		{
			DiagramShapeWithAttachmentApprovedHelper((network, shape) => network.HideEntity(shape));
		}

		void DiagramShapeWithAttachmentApprovedHelper(Action<INetwork, ShapeNetworkEntity> networkAction)
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "Diagram1", isScaled: true);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.DiagramShape.Factory.Save();

			var shape1 = networkViewModel.CreateNewShape(network.DiagramShape);
			shape1.Name = "Shape1";

			var shape2 = networkViewModel.CreateNewShape(network.DiagramShape);
			shape2.Name = "Shape2";

			network.CreateRelationship(shape1, shape2);

			Factory.Save();

			using (var diagramForm = new NetworkDiagramForm(diagram))
			{
				diagramForm.Show();
				Application.DoEvents();

				diagramForm.Network.CreateRelationship(shape1, shape2);

				AssertEquals(false, diagramForm.Network.DiagramEntity.EntityState.HasFlag(EntityState.Approved));
				diagramForm.NetworkViewModel.ToggleApproval();
				AssertEquals(true, diagramForm.Network.DiagramEntity.EntityState.HasFlag(EntityState.Approved));

				var newFactory = new BusinessObjectFactory()
				{
					RefreshEnabled = false
				};

				var newShape = networkViewModel.CreateNewShape(diagram);
				newShape.Name = "New Shape";

				Application.DoEvents();
				UnitTestUserNotification.Instance.AddOKAnswer();
				diagramForm.FireSaveButton();

				var loadedDiagram = newFactory.Load<BMNCNRootDiagramShape>(diagram.PK);

				using (var diagramForm2 = new NetworkDiagramForm(loadedDiagram))
				{
					diagramForm2.Show();
					Application.DoEvents();

					var loadedShape1 = diagramForm2.Network.Entities.Single(e => e.EntityPK == shape1.PK);
					var loadedNewShape = diagramForm2.Network.Entities.Single(e => e.EntityPK == newShape.PK);

					diagramForm2.Network.CreateRelationship(loadedShape1, loadedNewShape);
					UnitTestUserNotification.Instance.AddOKAnswer();
					AssertEquals(true, diagramForm2.Network.DiagramEntity.EntityState.HasFlag(EntityState.Approved));
					diagramForm2.NetworkViewModel.ToggleApproval();
					AssertEquals(false, diagramForm2.Network.DiagramEntity.EntityState.HasFlag(EntityState.Approved));
					diagramForm2.NetworkViewModel.ToggleApproval();

					AssertEquals(true, diagramForm2.Network.DiagramEntity.EntityState.HasFlag(EntityState.Approved));
					Application.DoEvents();
					UnitTestUserNotification.Instance.AddOKAnswer();
					diagramForm2.FireSaveButton();
					Application.DoEvents();
				}

				UnitTestUserNotification.Instance.AddOKAnswer();

				AssertEquals(true, diagramForm.Network.DiagramEntity.EntityState.HasFlag(EntityState.Approved));
				diagramForm.NetworkViewModel.ToggleApproval();
				AssertEquals(false, diagramForm.Network.DiagramEntity.EntityState.HasFlag(EntityState.Approved));
				AssertNoExceptionThrown(() => networkAction(diagramForm.NetworkViewModel.Network, shape1));
			}
		}

		public void TestOpenAsDiagram_WhenDiagramShapeDeletedInAnotherFactory_ShouldNotDieHorribly()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "Jango Fett", isScaled: true);
			diagram.ScrollPosition = ScrollPositionList.Codes.Default;
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			network.DiagramShape.Factory.Save();

			var shape = networkViewModel.CreateNewShape(network.DiagramShape);
			shape.Name = "Boba Fett";

			network.DiagramShape.Factory.Save();

			Factory.Save();

			using (var diagramForm = new NetworkDiagramForm(diagram))
			{
				diagramForm.Show();
				Application.DoEvents();

				using (var childShapeForm = NetworkGUITestCase.FindAndClickOpenAsDiagramMenuItem(diagramForm, shape.Shape))
				{
					childShapeForm.Show();
					Application.DoEvents();

					Factory.Save(); // save this new diagram into the db

					UnitTestUserNotification.Instance.ClearMessages(); // Ensures we aren't testing the other messages that have appeared during this test.

					childShapeForm.BringToFront();

					OpenDiagramInAnotherFactoryAndDeleteAShape(diagram, shape.Shape);
					AssertEquals(false, shape.IsDeleted);
					AssertEquals("Precondition: child shape form should still be shown", false, childShapeForm.IsDisposed);

					childShapeForm.BringToFront();
					NetworkGUITestCase.DoEventsThoroughly();

					AssertEquals("Now that the user has attempted to go back to a form for an already-deleted diagram, architecture code should handle the situation and close the form.",
						"You are currently modifying Jango Fett in another form. You may be able to save modifications from only one form.",
						UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessages(); // Ensures we aren't testing the other messages that have appeared during this test.

					AssertNoExceptionThrown("Poking our childShapeForm should not cause massive errors", () =>
					{
						NetworkGUITestCase.InvokeEditPropertiesActionOnFormAndEnterValueIntoField<ZTextBox>(childShapeForm, "ShapeNameTextBox", "Poke");
					});

					AssertEquals("Poke", childShapeForm.DataSource.Name);
					childShapeForm.FireSaveButton();
					AssertEquals(true, childShapeForm.IsDisposed);
				}

				AssertNoExceptionThrown("Poking our childShapeForm should not cause massive errors", () =>
				{
					diagramForm.Show();
					Application.DoEvents();
				});
			}

			AssertNull("Shape deletion should have been persisted", Factory.CreateNewFactory().Load<BMNCNShape>(shape.PK));
		}

		void OpenDiagramInAnotherFactoryAndDeleteAShape(BMNCNRootDiagramShape diagram, BMNCNShape shape)
		{
			var newFactory = new BusinessObjectFactory()
			{
				RefreshEnabled = false
			};
			var altDiagram = newFactory.Load<BMNCNShape>(diagram.PK);
			var altShape = newFactory.Load<BMNCNShape>(shape.PK);

			using (var diagramForm = new NetworkDiagramForm(altDiagram))
			{
				diagramForm.Show();
				Application.DoEvents();

				var killMe = NetworkGUITestCase.FindShapeOnForm(altShape, diagramForm);
				killMe.Delete();
			}

			newFactory.Save();
		}

		public void TestCloseFindFormOnBackgroundThread()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			Factory.Save();

			var exceptionMessage = string.Empty;

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkUserControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				// Simulate ZController.ShowEditform: modify DataContext (NetworkDiagramForm.NetworkDiagramControl.NetworkUserControl.ViewModel)
				networkUserControl.ApplyTemplate();

				form.OpenFinderForm();
				Application.DoEvents();
				AssertEquals("Precondition: FinderForm is shown", 1, Application.OpenForms.OfType<SearchFinderForm>().ToArray().Length);

				var task = Task.Run(() =>
				{
					try
					{
						AssertNotEquals("Precondition: background thread", 1, Thread.CurrentThread.ManagedThreadId);

						// Closing FinderForm in another thread: Simulate Heartbeat.KeepHeartbeatAlive > OnRemoteLogoff > Application.Exit > Form.RaiseFornClosedOnAppExit > Form.OnFormClosed > Modify DataContext (NetworkDiagramForm.NetworkDiagramControl.NetworkUserControl.ViewModel)
						form.CloseFinderForm();
					}
					catch (Exception ex)
					{
						exceptionMessage = ex.Message;
					}
				});

				task.Wait();
			}

			Application.DoEvents();
			CombineAssertions(() =>
			{
				AssertEquals("FinderForm should be closed", 0, Application.OpenForms.OfType<SearchFinderForm>().ToArray().Length);
				AssertNullOrEmpty("Should not have exception", exceptionMessage);
			});
		}

		public void TestCloseFinderForm_ShouldNotThrow_WhenDiagramFormIsNotFullyInitialised()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				AssertNoExceptionThrown(() =>
				{
					form.OpenFinderForm();
					form.CloseFinderForm();
				});
			}
		}

		public void TestOpenAsDiagramMenuItem_WhenDiagramUnsaved_ShouldPromptFormSave_AnsweringYes()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();
				var networkViewModel = form.NetworkViewModel;

				AssertEquals(false, diagram.HasChanges);

				var shape = networkViewModel.CreateNewShape(diagram);

				AssertEquals(true, diagram.HasChanges);

				using (var childForm = NetworkGUITestCase.FindAndClickOpenAsDiagramMenuItem(form, shape.Shape))
				{
					AssertNotNull("Network diagram hadn't been saved, but then the dialog's default option caused it to be so", childForm);
					AssertEquals("The form will attempt to save before performing this operation.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Confirmation Required", UnitTestUserNotification.Instance.LastMessage.Caption);

					NetworkGUITestCase.DoEventsThoroughly();
				}

				AssertEquals(false, diagram.HasChanges);

				NetworkGUITestCase.DoEventsThoroughly();
			}
		}

		public void TestOpenAsDiagramMenuItem_WhenDiagramUnsaved_ShouldPromptFormSave_AnsweringNo()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();
				var networkViewModel = form.NetworkViewModel;

				AssertEquals(false, diagram.HasChanges);

				var shape = networkViewModel.CreateNewShape(diagram);

				AssertEquals(true, diagram.HasChanges);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				using (var childForm = NetworkGUITestCase.FindAndClickOpenAsDiagramMenuItem(form, shape.Shape))
				{
					AssertNull("Network diagram hadn't been saved, and we answered 'no' to the dialog", childForm);
					AssertEquals("The form will attempt to save before performing this operation.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Confirmation Required", UnitTestUserNotification.Instance.LastMessage.Caption);
				}

				AssertEquals(true, diagram.HasChanges);
			}
		}

		#endregion

		#region Open Shape List Action

		public void TestNoNewAndDeleteMenuItemsAndButtons_WhenNetworkDiagramModuleIsEmbeddedPopup()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "Jango Fett");

			Factory.Save();

			using (var diagramForm = new NetworkDiagramForm(diagram))
			{
				diagramForm.Show();
				Application.DoEvents();

				NetworkGUITestCase.FindAndClickOpenShapeListMenuItem(diagramForm, diagram, (module) =>
				{
					AssertNotNull(module);

					AssertEquals(typeof(BMNCNShape), module.TypeOfTopLevelBusinessObject);
					AssertNull(module.ToolBarButtons.FindByText("New"));
					AssertNull(module.ToolBarButtons.FindByText("Delete"));
					AssertNull(module.NewMenuItem);
					AssertNull(module.DeleteMenuItem);
				});
			}
		}

		public void TestOpenShapeListAction()
		{
			var diagram1 = NetworkTestCase.CreateDiagram(Factory, name: "Kylo Ren");
			var shape11 = NetworkTestCase.CreateShape(diagram1, "VII");
			var shape12 = NetworkTestCase.CreateShape(diagram1, "VIII");
			var shape13 = NetworkTestCase.CreateShape(diagram1, "IX");

			var diagram2 = NetworkTestCase.CreateDiagram(Factory, name: "Han Solo");
			var shape21 = NetworkTestCase.CreateShape(diagram2, "IV");
			var shape22 = NetworkTestCase.CreateShape(diagram2, "V");
			var shape23 = NetworkTestCase.CreateShape(diagram2, "VI");

			Factory.Save();

			using (var diagramForm = new NetworkDiagramForm(diagram1))
			{
				diagramForm.Show();
				Application.DoEvents();

				NetworkGUITestCase.FindAndClickOpenShapeListMenuItem(diagramForm, diagram1, (module) =>
				{
					AssertNotNull(module);

					AssertEquals(typeof(BMNCNShape), module.TypeOfTopLevelBusinessObject);

					AssertEquals(3, module.GridCollection.Count);
					var shapes = module.GridCollection.Cast<BMNCNShape>();
					AssertContainsExactElementsInAnyOrder(new[] { "VII", "VIII", "IX" }, shapes.Select(s => s.BNS_Name));
				});
			}

			using (var diagramForm = new NetworkDiagramForm(diagram2))
			{
				diagramForm.Show();
				Application.DoEvents();

				NetworkGUITestCase.FindAndClickOpenShapeListMenuItem(diagramForm, diagram2, (module) =>
				{
					AssertNotNull(module);

					AssertEquals(typeof(BMNCNShape), module.TypeOfTopLevelBusinessObject);

					AssertEquals(3, module.GridCollection.Count);
					var shapes = module.GridCollection.Cast<BMNCNShape>();
					AssertContainsExactElementsInAnyOrder(new[] { "IV", "V", "VI" }, shapes.Select(s => s.BNS_Name));
				});
			}
		}

		public void TestOpenShapeListAction_ApplyFiltersToNetworkDiagramModule()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "Fighters");
			var shape1 = NetworkTestCase.CreateShape(diagram, "Foo");
			var shape2 = NetworkTestCase.CreateShape(diagram, "Foobar");
			var shape3 = NetworkTestCase.CreateShape(diagram, "Foolhardy", shapeType: DiagramShapeTypeList.Codes.Buffer);
			var shape4 = NetworkTestCase.CreateShape(diagram, "Bazbat");
			var shape5 = NetworkTestCase.CreateShape(diagram, "Steve");
			var annotation = NetworkTestCase.CreateShape(diagram, "McKing", ShapeTypeList.Codes.Annotation);

			Factory.Save();

			using (var diagramForm = new NetworkDiagramForm(diagram))
			{
				diagramForm.Show();
				Application.DoEvents();

				NetworkGUITestCase.FindAndClickOpenShapeListMenuItem(diagramForm, diagram, (module) =>
				{
					AssertNotNull(module);

					AssertEquals(typeof(BMNCNShape), module.TypeOfTopLevelBusinessObject);

					AssertEquals(5, module.GridCollection.Count);
					var shapes = module.GridCollection.Cast<BMNCNShape>();
					AssertContainsExactElementsInAnyOrder("Should return all shapes of type SHP and BUF by default", new[] { "Foo", "Foobar", "Foolhardy", "Bazbat", "Steve" }, shapes.Select(s => s.BNS_Name));

					var textFilterStrip = module.FilterBusinessObject.AddTextFilterStrip(BMNCNShape.ModuleFilterConstants.Name, "Foo");
					textFilterStrip.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
					module.FilterBusinessObject.SaveLayout("Phu");

					module.PerformSearch_ForTest();

					AssertEquals(3, module.GridCollection.Count);
					shapes = module.GridCollection.Cast<BMNCNShape>();
					AssertContainsExactElementsInAnyOrder("Should return SHPs and BUFs that start with Foo", new[] { "Foo", "Foobar", "Foolhardy" }, shapes.Select(s => s.BNS_Name));

					textFilterStrip = module.FilterBusinessObject.AddTextFilterStrip(BMNCNShape.ModuleFilterConstants.DiagramType, DiagramShapeTypeList.Codes.Buffer);
					textFilterStrip.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
					module.FilterBusinessObject.SaveLayout("Pfou");

					module.PerformSearch_ForTest();

					AssertEquals(1, module.GridCollection.Count);
					shapes = module.GridCollection.Cast<BMNCNShape>();
					AssertContainsExactElementsInAnyOrder("Should only return the buffer shape", new[] { "Foolhardy" }, shapes.Select(s => s.BNS_Name));

					textFilterStrip = module.FilterBusinessObject.AddTextFilterStrip(BMNCNShape.ModuleFilterConstants.DiagramType, DiagramShapeTypeList.Codes.Diagram);
					textFilterStrip.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
					module.FilterBusinessObject.SaveLayout("Ghoti");

					module.PerformSearch_ForTest();

					AssertEquals(0, module.GridCollection.Count);
					shapes = module.GridCollection.Cast<BMNCNShape>();
					AssertContainsExactElementsInAnyOrder("Should return NOTHING, especially not no stinkin' diagrams nor smelly annotations", Array.Empty<ZString>(), shapes.Select(s => s.BNS_Name));
				});
			}
		}

		#endregion

		#region General Form Behaviour

		public void TestOpeningFormTriggersValidation()
		{
			var diagram = NetworkTestCase.CreateNetwork(NetworkTestCase.CreateDiagram(Factory, isScaled: true)).DiagramEntity;
			var shape1 = NetworkTestCase.CreateShape(diagram);
			var shape2 = NetworkTestCase.CreateShape(diagram);

			var attachment = (NetworkAttachment)diagram.Network.CreateRelationship(shape1, shape2);
			shape1.SetCoordinates(300, 100, 0, 10);
			shape2.SetCoordinates(300, 100, 0, 19);
			AssertHasRowWarning(attachment, "The post-requisite is scheduled to begin before its pre-requisite is scheduled to complete.");

			using (var form = new NetworkDiagramForm(diagram.Shape))
			{
				form.Show();
				Application.DoEvents();
				var attachmentOnForm = form.Network.Entities.ShapeEntities.FirstOrDefault().Links.SingleOrDefault();
				AssertNoRowWarnings("No warning, because validation doesn't run on Show. (Don't ask me why.)", attachmentOnForm);
				form.FireSaveButton();
				AssertHasRowWarning(attachmentOnForm, "The post-requisite is scheduled to begin before its pre-requisite is scheduled to complete.");
			}
		}

		public void TestFireSaveButtonRaisesSavingEvent()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var refreshes = new List<RefreshType>();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();

				form.Network.Refresher.Refreshed += (s, e) => refreshes.Add(e.RefreshType);
				form.FireSaveButton();
			}

			AssertCollectionContains(RefreshType.Saving, refreshes);
		}

		public void TestSave_ShouldRefreshNetwork()
		{
			var diagram = Factory.New<BMNCNShape>();
			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();

				var refreshed = false;
				form.Network.Refreshed += (s, e) => refreshed = true;

				form.FireSaveButton();
				AssertEquals(true, refreshed);
			}
		}

		public void TestMaximumSize()
		{
			var diagram = Factory.New<BMNCNShape>();
			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();

				var expectedSize = ControlDpiScalingHelper.NewScaledSize(CCPMConstants.MaxDiagramFormSizeWidth, CCPMConstants.MaxDiagramFormSizeHeight);
				var actualSize = form.MaximumSize;

				AssertEquals("40inch monitor size", expectedSize, actualSize);
			}
		}

		public void TestSave_WhenNetworkIsNull_ShouldRefreshNetwork()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "Mai Frist Diagram");
			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				form.NetworkDiagramControl.SetDataBinding(null, string.Empty);

				AssertNoExceptionThrown(() => form.FireSaveButton());
			}
		}

		public void TestShowFormForNewShape_CreateNewEntity_ShouldSetHasChangesProperly()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "Mai Frist Diagram");

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();
				var networkViewModel = form.NetworkViewModel;
				AssertNotNull(networkViewModel);

				var shape = networkViewModel.CreateNewShape(diagram);
				((IProposedNetworkEntity)shape).Name = "Mai Workflow";
				AssertEquals(true, diagram.HasChanges);
				shape.X = shape.Y = 50;

				VisualBoardsTestCase.AssertSaved(form.FireSaveButton());
				AssertEquals(false, diagram.HasChanges);
				AssertNull(shape.ProcessHeader);
			}
		}

		public void TestMoveShape_ShouldSetHasChanges()
		{
			BMSTestHelper.CreateSystem(Factory, "DUM");
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var diagram = NetworkTestCase.CreateDiagram(jobHeader);
			diagram.BNS_Name = "Mai Frist Diagram";

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				var networkViewModel = form.NetworkViewModel;
				AssertNotNull(networkViewModel);

				var workflowShape = networkViewModel.CreateNewShape(diagram);
				((IProposedNetworkEntity)workflowShape).Name = "Mai Workflow";
				AssertEquals(true, diagram.HasChanges);
				AssertEquals(false, workflowShape.IsDiagram);
				workflowShape.X = workflowShape.Y = 50;

				VisualBoardsTestCase.AssertSaved(form.FireSaveButton());

				Application.DoEvents();

				AssertEquals(false, diagram.HasChanges);

				((INetworkEntity)workflowShape).X += 10;

				AssertEquals(true, diagram.HasChanges);
			}
		}

		[TestDate(2016, 3, 28)]
		public void TestOpenForm_NonScaledDiagram_ShouldNotSetHasChanges()
		{
			var diagramShape = NetworkTestCase.CreateDiagram(Factory, name: "Root", isScaled: false);
			var diagram = NetworkTestCase.CreateNetwork(diagramShape).DiagramEntity;

			var shape1 = NetworkTestCase.CreateShape(diagram, name: "S1");
			shape1.SetCoordinates(300, 300, 0, 100);
			var shape2 = NetworkTestCase.CreateShape(diagram, name: "S2");
			shape2.SetCoordinates(300, 300, 300, 100);

			shape1.MakeVisiblePrerequisiteOf(shape2);

			Factory.Save();

			TestDateAttribute.Date = new DateTime(2016, 4, 4);

			using (var form = new NetworkDiagramForm(diagram.Shape))
			{
				form.Show();
				Application.DoEvents();

				var network = form.Network;
				Assert(!form.BusinessEntityForHasChanges.HasChanges);
			}
		}

		[TestDate(2016, 3, 28)]
		public void TestOpenForm_ScaledDiagram_ShouldNotSetHasChanges()
		{
			var diagram = NetworkTestCase.CreateNetwork(NetworkTestCase.CreateDiagram(Factory, name: "Root", isScaled: true)).DiagramEntity;
			var shape1 = NetworkTestCase.CreateShape(diagram, name: "S1");
			shape1.SetCoordinates(300, 300, 0, 100);
			var shape2 = NetworkTestCase.CreateShape(diagram, name: "S2");
			shape2.SetCoordinates(300, 300, 300, 100);

			shape1.MakeVisiblePrerequisiteOf(shape2);

			Factory.Save();

			TestDateAttribute.Date = new DateTime(2016, 4, 4);

			using (var form = new NetworkDiagramForm(diagram.Shape))
			{
				form.Show();
				Application.DoEvents();

				var network = form.Network;
				Assert(!form.BusinessEntityForHasChanges.HasChanges);
			}
		}

		public void TestFormCaption()
		{
			var diagram = NetworkTestCase.CreateJobAndDiagram(Factory);
			diagram.BNS_Name = "Mai Frist Diagarm";

			using (var form = new NetworkDiagramForm(diagram))
			{
				AssertEquals("New Network Diagram", $"{form.FormVerb} {form.FormCaption}");
			}

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				AssertEquals("Edit Mai Frist Diagarm", $"{form.FormVerb} {form.FormCaption}");
			}
		}

		public void TestFormCaption_HasTextWhenDiagramIsUnnamed()
		{
			var diagram = NetworkTestCase.CreateJobAndDiagram(Factory);
			diagram.BNS_Name = "";

			using (var form = new NetworkDiagramForm(diagram))
			{
				AssertEquals("New Untitled Diagram", $"{form.FormVerb} {form.FormCaption}");
			}
		}

		public void TestDiagramScheduleActivatesSaveButton()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			diagram.BNS_Name = "Diagram1";
			diagram.ScrollPosition = ScrollPositionList.Codes.Default;
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			network.DiagramShape.Factory.Save();

			var shape1 = networkViewModel.CreateNewShape(network.DiagramShape);
			shape1.Name = "Shape1";

			network.DiagramShape.Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagram))
			{
				var postingButtons = (IPostingButtonsProvider)form;
				Assert(postingButtons.CommandButtonApply.Visible);
				AssertEquals("&New", postingButtons.CommandButtonApply.Text);

				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				Application.DoEvents();

				var nodes = networkDiagram.FindChildren<AdornedControl>().Where(x => x.DataContext is NodeViewModel).ToArray();
				var shapeNode = nodes.Select(n => n.DataContext).Cast<NodeViewModel>().Single(n => n.Name == "Shape1");
				var editProperties = shapeNode.MenuItems.WhereNotNull().Single(m => m.Name == "Edit Properties");
				editProperties.Action.Execute();

				diagram.ScheduledStartTimeLocal = ZDateTime.Now;
				Factory.Save();
				Application.DoEvents();

				AssertEquals("&Save", postingButtons.CommandButtonApply.Text);

				form.FireSaveButton();
				Application.DoEvents();
				AssertEquals("&New", postingButtons.CommandButtonApply.Text);
			}
		}

		public void TestSavingEditsToDiagramAffinities_ShouldUpdatePostingButtons()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			diagram.BNS_Name = "Diagram1";
			var network = NetworkTestCase.CreateNetwork(diagram);

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				var postingButtons = form as IPostingButtonsProvider;
				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				Application.DoEvents();

				AssertNotNull("Pre-condition", networkDiagram);
				var shape = NetworkTestCase.CreateShape(diagram, "Shape1");

				// Pre-condition: Form Posting buttons are those expected for a form with current changes (new shape)
				ZFormTestHelper.AssertCommandButtons(postingButtons,
						applyEnabled: true, postEnabled: true, cancelEnabled: true,
						applyText: "&Save", postText: "S&ave && Close", cancelText: "&Cancel");

				form.FindSingle<ZPostingButtonsUserControl>().SaveButton.PerformClick();

				// Pre-condition: Form Posting buttons are those expected for a from with no current changes
				ZFormTestHelper.AssertCommandButtons(postingButtons,
					applyEnabled: true, postEnabled: false, cancelEnabled: true,
					applyText: "&New", postText: "S&ave && Close", cancelText: "&Close");

				AssertEquals("Pre-condition", false, diagram.HasChanges);

				ZGrid editFormGrid = null;

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((o) =>
				{
					var popupForm = o as ModifyAffinitiesForm;
					if (popupForm != null)
					{
						popupForm.Load += (s, e) =>
						{
							popupForm.Show();
							popupForm.ShapeAffinitiesGrid.ListManager.AddNew();

							Application.DoEvents();
							editFormGrid = popupForm.ShapeAffinitiesGrid;

							popupForm.ShapeAffinitiesGrid[0, 0] = new ZString("TestAffinity");
							popupForm.ShapeAffinitiesGrid.ListManager.Position = 1;
							Application.DoEvents();
						};
					}
				});

				var editAffinities = networkDiagram.ViewModel.MenuItems.WhereNotNull().Single(m => m.Name == "Affinities").Items.Single(m => m.Name == "Edit Affinities");
				var editForm = editAffinities.Action.Execute();

				AssertEquals("Diagram should have changes after making modifications to root diagram affinities.", true, diagram.HasChanges);
				ZFormTestHelper.AssertCommandButtons(postingButtons,
						applyEnabled: true, postEnabled: true, cancelEnabled: true,
						applyText: "&Save", postText: "S&ave && Close", cancelText: "&Cancel");

				form.FindSingle<ZPostingButtonsUserControl>().SaveButton.PerformClick();

				AssertEquals("Diagram should have no changes after saving edits to root diagram affinities.", false, diagram.HasChanges);
				ZFormTestHelper.AssertCommandButtons(postingButtons,
					applyEnabled: true, postEnabled: false, cancelEnabled: true,
					applyText: "&New", postText: "S&ave && Close", cancelText: "&Close");
			}
		}

		public void TestBufferPenetrationCalculation_ShouldNotSetHasChanges()
		{
			var diagramShape = NetworkTestCase.CreateDiagram(Factory, name: "Root", isScaled: false);
			var network = NetworkTestCase.CreateNetwork(diagramShape);
			network.SwitchToScaled();
			diagramShape.ScheduleBizo.BNC_GB_Branch = Env.CurrentBranchPK;
			diagramShape.ScheduleBizo.BNC_GE_Department = Env.CurrentDepartmentPK;
			var diagram = network.DiagramEntity;

			var shape = Factory.NewWithValidTestData<BMNCNBufferShape>();
			shape.Name = "ProjectBuffer";
			shape.BufferPenetration = 0.5;
			shape.AsEntity(network).SetCoordinates(300, 300, 10, 10);
			shape.MakeChildOf(diagramShape);

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram.Shape))
			{
				form.Show();
				Application.DoEvents();

				Assert(!form.FindSingle<ZPostingButtonsUserControl>().SaveAndCloseButton.Enabled);
			}
		}

		public void TestShapeWithinShape_ShouldNotSetHasChanges()
		{
			var diagramShape = NetworkTestCase.CreateDiagram(Factory, name: "Root", isScaled: false);
			var network = NetworkTestCase.CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;
			var shape = NetworkTestCase.CreateShape(diagram, name: "S1");
			shape.SetCoordinates(300, 300, 10, 10);
			var shape1 = NetworkTestCase.CreateShape(shape, name: "S1_1");
			shape1.SetCoordinates(200, 100, 20, 20);

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram.Shape))
			{
				form.Show();
				Application.DoEvents();

				Assert(!form.FindSingle<ZPostingButtonsUserControl>().SaveAndCloseButton.Enabled);
			}
		}

		#endregion

		#region Scroll Position

		[TestDate(2016, 3, 24)]
		public void TestScrollToShapeOnFormOpen()
		{
			var earliestStartDays = 10;

			var diagramShape = NetworkTestCase.CreateDiagram(Factory, name: "The Big and Long...Diagram", isScaled: true);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			diagramShape.ScheduledStartTimeLocal = ZDateTime.Today.AddDays(-earliestStartDays).AddHours(10); //offset from UTC to current branch time (Brisbane)

			network.DiagramEntity.Factory.Save();

			var shape1 = CreateNewShape(networkViewModel, diagram.Shape, "Closed shape", 2, 0, "CLS");
			var shape2 = CreateNewShape(networkViewModel, diagram.Shape, "First open shape", 2, 6);
			var shape3 = CreateNewShape(networkViewModel, diagram.Shape, "Second open shape", 3, 10);
			var shape4 = CreateNewShape(networkViewModel, diagram.Shape, "The Long and Thick...Shape", 10, 26);

			var minutesInADay = 60 * 24;
			var columnsInADay = (minutesInADay / diagramShape.Scale.GetMinutesFromDateTimeSpan());
			var weekendDays = 2;
			var threeColumnOffset = diagram.ScaleUnitPixelSize * 3.0;
			var earliestToCurrentColumns = ((columnsInADay * (earliestStartDays - weekendDays))) * diagram.ScaleUnitPixelSize;

			var firstOpenShapePosition = shape2.X - threeColumnOffset;
			var currentColumnPosition = earliestToCurrentColumns - threeColumnOffset;

			diagramShape.ScrollPosition = ScrollPositionList.Codes.First;
			Factory.Save();
			AssertScrollPosition(diagram.Shape, "Should scroll to first open shape when scroll position is specified and diagram is not approved.", firstOpenShapePosition);

			diagramShape.ScrollPosition = ScrollPositionList.Codes.Current;
			Factory.Save();
			AssertScrollPosition(diagram.Shape, "Should scroll to current column when scroll position is specified and diagram is not approved.", currentColumnPosition);

			diagramShape.ScrollPosition = ScrollPositionList.Codes.Start;
			Factory.Save();
			AssertScrollPosition(diagram.Shape, "No scrolling when scroll position is start and diagram is not approved.", 0.0);

			diagramShape.ScrollPosition = ScrollPositionList.Codes.Default;
			Factory.Save();
			AssertScrollPosition(diagram.Shape, "No scrolling when scroll position is default and diagram is not approved.", 0.0);

			network.DiagramEntity.Approve(GlbStaff.CurrentUser.GS_Code);

			Factory.Save();
			AssertScrollPosition(diagram.Shape, "Should scroll to first open shape by default when diagram is approved and scroll position is default.", firstOpenShapePosition);

			diagramShape.ScrollPosition = ScrollPositionList.Codes.Current;
			Factory.Save();
			AssertScrollPosition(diagram.Shape, "Should scroll to current column when scroll position is specified and diagram is approved.", currentColumnPosition);

			diagramShape.ScrollPosition = ScrollPositionList.Codes.First;
			Factory.Save();
			AssertScrollPosition(diagram.Shape, "Should scroll to first open shape when scroll position is specified and diagram is approved.", firstOpenShapePosition);

			diagramShape.ScrollPosition = ScrollPositionList.Codes.Start;
			Factory.Save();
			AssertScrollPosition(diagram.Shape, "No scrolling when scroll position is start and diagram is approved.", 0.0);
		}

		[TestDate(2016, 3, 24)]
		public void TestScrollWithNoShapes()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			diagram.BNS_Name = "The Big and Long...Diagram";
			var network = NetworkTestCase.CreateNetwork(diagram);
			network.SwitchToScaled();
			diagram.EarliestStartTimeLocal = ZDateTime.Today.AddDays(-100);
			network.DiagramShape.Factory.Save();

			diagram.ScrollPosition = ScrollPositionList.Codes.First;
			network.DiagramShape.Factory.Save();
			AssertScrollPosition(diagram, "No scrolling to first open shape when scroll position is specified and diagram is not approved.", 0.0);

			diagram.ScrollPosition = ScrollPositionList.Codes.Current;
			network.DiagramShape.Factory.Save();
			AssertScrollPosition(diagram, "No scrolling to current column when scroll position is specified and diagram is not approved.", 0.0);

			diagram.ScrollPosition = ScrollPositionList.Codes.Start;
			network.DiagramShape.Factory.Save();
			AssertScrollPosition(diagram, "No scrolling when scroll position is start and diagram is not approved.", 0.0);
		}

		[TestDate(2016, 3, 24)]
		public void TestNoScrollToShapeOnFormOpen_AllClosedShapesInThePast()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			diagram.BNS_Name = "The Big and Long...Diagram";
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			diagram.ScheduledStartTimeUtc = ZDateTime.Today.AddDays(-100);
			Factory.Save();

			var shape1 = CreateNewShape(networkViewModel, diagram, "First Closed Shape", 2, 0, "CLS");
			var shape2 = CreateNewShape(networkViewModel, diagram, "Second Closed Shape", 2, 6, "CLS");
			var shape3 = CreateNewShape(networkViewModel, diagram, "Third Closed Shape", 3, 10, "CLS");
			var shape4 = CreateNewShape(networkViewModel, diagram, "Fourth Closed Shape", 10, 26, "CLS");

			diagram.ScrollPosition = ScrollPositionList.Codes.First;
			Factory.Save();

			diagram.ScrollPosition = ScrollPositionList.Codes.First;
			Factory.Save();
			AssertScrollPosition(diagram, "No scrolling to first open shape when scroll position is specified and diagram is not approved.", 0.0);

			diagram.ScrollPosition = ScrollPositionList.Codes.Current;
			Factory.Save();
			AssertScrollPosition(diagram, "No scrolling to current column when scroll position is specified and diagram is not approved.", 0.0);

			diagram.ScrollPosition = ScrollPositionList.Codes.Start;
			Factory.Save();
			AssertScrollPosition(diagram, "No scrolling when scroll position is start and diagram is not approved.", 0.0);

			diagram.ScrollPosition = ScrollPositionList.Codes.Default;
			Factory.Save();
			AssertScrollPosition(diagram, "No scrolling when scroll position is default and diagram is not approved.", 0.0);

			network.DiagramEntity.Approve(GlbStaff.CurrentUser.GS_Code);

			Factory.Save();
			AssertScrollPosition(diagram, "No scrolling to first open shape by default when diagram is approved and scroll position is default.", 0.0);

			diagram.ScrollPosition = ScrollPositionList.Codes.Current;
			Factory.Save();
			AssertScrollPosition(diagram, "No scrolling to current column when scroll position is specified and diagram is approved.", 0.0);

			diagram.ScrollPosition = ScrollPositionList.Codes.First;
			Factory.Save();
			AssertScrollPosition(diagram, "No scrolling to first open shape when scroll position is specified and diagram is approved.", 0.0);

			diagram.ScrollPosition = ScrollPositionList.Codes.Start;
			Factory.Save();
			AssertScrollPosition(diagram, "No scrolling when scroll position is start and diagram is approved.", 0.0);
		}

		[TestDate(2016, 3, 24)]
		public void TestDoesNotScrollToAnnotationOnFormOpen_FirstOpenShape()
		{
			var earliestStartDays = 10;

			var diagram = NetworkTestCase.CreateDiagram(Factory);
			diagram.BNS_Name = "El Diagrama Grande y Largo";
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			diagram.EarliestStartTimeLocal = ZDateTime.Today.AddDays(-earliestStartDays);
			network.DiagramShape.Factory.Save();

			var shape1 = CreateNewShape(networkViewModel, diagram, "La Forma Cerrada (CLOSED)", 2, 0, "CLS");
			var shape2 = CreateNewShape(networkViewModel, diagram, "La Primera Forma (OPEN)", 2, 6);
			var shape3 = CreateNewShape(networkViewModel, diagram, "La Segunda Forma (OPEN)", 3, 10);
			var shape4 = CreateNewShape(networkViewModel, diagram, "El Formo Largo y Gordo (OPEN)", 10, 26);

			var annotation = networkViewModel.CreateNewAnnotation(diagram);
			annotation.Shape.BNS_Name = "El Anotación";
			annotation.X = diagram.ScaleUnitPixelSize;

			var threeColumnOffset = diagram.ScaleUnitPixelSize * 3.0;
			var firstOpenShapePosition = shape2.X - threeColumnOffset;

			diagram.ScrollPosition = ScrollPositionList.Codes.First;
			network.DiagramShape.Factory.Save();
			AssertScrollPosition(diagram, "Should scroll to first open shape, NOT the annotation!", firstOpenShapePosition);
		}

		ShapeNetworkEntity CreateNewShape(NetworkViewModel networkViewModel, BMNCNShape diagram, ZString name, double width, double x, string status = "OPN")
		{
			var shape = networkViewModel.CreateNewShape(networkViewModel.GetJobNetwork().DiagramShape);
			shape.Shape.BNS_Name = name;
			shape.Shape.BNS_Status = status;
			shape.Width = diagram.ScaleUnitPixelSize * width;
			shape.X = diagram.ScaleUnitPixelSize * x;

			return shape;
		}

		static void AssertScrollPosition(BMNCNShape diagram, string message, double expectedValue)
		{
			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagram))
			{
				form.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(1000);
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				var scrollViewer = networkDiagram.MainDiagramControl.FindChildren<ScrollViewer>().Single(s => s.Name == "ScrollViewer");

				AssertEquals(message, expectedValue, scrollViewer.HorizontalOffset);
			}
		}

		#endregion

		#region Shape Status

		public void TestUnlinkedShapesShouldHaveSetStatusOptions()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			diagram.BNS_Name = "Diagram1";
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();

			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			jobHeader.FH_Status = "WRK";
			var workflow = jobHeader.ProcessHeaders.AddNew();

			var linkedShape = networkViewModel.CreateNewShape(diagram);
			linkedShape.Shape.BNS_Name = "Shape1";
			linkedShape.Shape.BNS_RelatedEntityID = workflow.PK;

			var unlinkedShape = networkViewModel.CreateNewShape(diagram);
			unlinkedShape.Shape.BNS_Name = "Shape2";

			var annotation = networkViewModel.CreateNewAnnotation(diagram);
			annotation.Shape.BNS_Name = "Booshga";

			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagram))
			{
				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				Application.DoEvents();
				var formNetworkViewModel = networkDiagram.ViewModel.NetworkViewModel;

				var linkedShapeNode = formNetworkViewModel.Nodes.Single(n => n.Name == "Shape1");
				var unlinkedShapeNode = formNetworkViewModel.Nodes.Single(n => n.Name == "Shape2");
				var annotationNode = formNetworkViewModel.Nodes.Single(n => n.Name == "Booshga");

				AssertNull(formNetworkViewModel.GetSecondLevelNetworkActionsMenuItem_ForTesting(linkedShapeNode.Entity, "Actions", "Set Status"));
				AssertNull(formNetworkViewModel.GetSecondLevelNetworkActionsMenuItem_ForTesting(annotationNode.Entity, "Actions", "Set Status"));
				AssertNotNull(formNetworkViewModel.GetSecondLevelNetworkActionsMenuItem_ForTesting(unlinkedShapeNode.Entity, "Actions", "Set Status"));
			}
		}

		public void TestLinkedStatus()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "New Horizons");
			var workflow1 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "Jupiter");
			var workflow2 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "Pluto");
			var workflow3 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "Neptune");
			var workflow4 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "Uranus");
			var workflow5 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "Saturn");
			var workflow6 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "Mars");
			var workflow7 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "Venus");
			var task1 = VisualBoardsTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 10, description: "Swing By", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = VisualBoardsTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 10, description: "Photograph", taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled);
			var task3 = VisualBoardsTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 10, description: "Discover", taskStatus: ProcessTaskStatusCodeList.Codes.Working);
			var task4 = VisualBoardsTestHelper.CreateTask(workflow4, GlbStaff.CurrentUser.GS_Code, 10, description: "Discover", taskStatus: ProcessTaskStatusCodeList.Codes.Suspended);
			var task5 = VisualBoardsTestHelper.CreateTask(workflow5, GlbStaff.CurrentUser.GS_Code, 10, description: "Discover", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);
			var task6 = VisualBoardsTestHelper.CreateTask(workflow6, GlbStaff.CurrentUser.GS_Code, 10, description: "Discover", taskStatus: ProcessTaskStatusCodeList.Codes.Open);

			var diagram = NetworkTestCase.CreateDiagram(Factory);
			diagram.BNS_Name = "Diagram1";
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			Factory.Save();

			var shape1 = networkViewModel.CreateNewShape(diagram);
			shape1.Shape.BNS_Name = "Shape1";
			shape1.Shape.BNS_RelatedEntityID = workflow1.PK;

			var shape2 = networkViewModel.CreateNewShape(diagram);
			shape2.Shape.BNS_Name = "Shape2";
			shape2.Shape.BNS_RelatedEntityID = workflow2.PK;

			var shape3 = networkViewModel.CreateNewShape(diagram);
			shape3.Shape.BNS_Name = "Shape3";
			shape3.Shape.BNS_RelatedEntityID = workflow3.PK;

			var shape4 = networkViewModel.CreateNewShape(diagram);
			shape4.Shape.BNS_Name = "Shape4";
			shape4.Shape.BNS_RelatedEntityID = workflow4.PK;

			var shape5 = networkViewModel.CreateNewShape(diagram);
			shape5.Shape.BNS_Name = "Shape5";
			shape5.Shape.BNS_RelatedEntityID = workflow5.PK;

			var shape6 = networkViewModel.CreateNewShape(diagram);
			shape6.Shape.BNS_Name = "Shape6";
			shape6.Shape.BNS_RelatedEntityID = workflow6.PK;

			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagram))
			{
				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				Application.DoEvents();

				var nodes = networkDiagram.FindChildren<AdornedControl>().Where(x => x.DataContext is NodeViewModel).ToArray();
				var shapeNode1 = nodes.Select(n => n.DataContext).Cast<NodeViewModel>().Single(n => n.Name == "Shape1");
				var shapeNode2 = nodes.Select(n => n.DataContext).Cast<NodeViewModel>().Single(n => n.Name == "Shape2");
				var shapeNode3 = nodes.Select(n => n.DataContext).Cast<NodeViewModel>().Single(n => n.Name == "Shape3");
				var shapeNode4 = nodes.Select(n => n.DataContext).Cast<NodeViewModel>().Single(n => n.Name == "Shape4");
				var shapeNode5 = nodes.Select(n => n.DataContext).Cast<NodeViewModel>().Single(n => n.Name == "Shape5");
				var shapeNode6 = nodes.Select(n => n.DataContext).Cast<NodeViewModel>().Single(n => n.Name == "Shape6");

				AssertEquals(shapeNode1.Status, WorkStatus.Complete);
				AssertEquals(shapeNode2.Status, WorkStatus.Cancelled);
				AssertEquals(shapeNode3.Status, WorkStatus.Working);
				AssertEquals(shapeNode4.Status, WorkStatus.Suspended);
				AssertEquals(shapeNode5.Status, WorkStatus.Startable);
				AssertEquals(shapeNode6.Status, WorkStatus.Startable);
			}
		}

		public void TestUnlinkedStatus()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			diagram.BNS_Name = "Diagram1";
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			network.DiagramShape.Factory.Save();

			var shape1 = networkViewModel.CreateNewShape(diagram);
			shape1.Shape.BNS_Name = "Shape1";
			shape1.Shape.BNS_Status = "CLS";

			var shape2 = networkViewModel.CreateNewShape(diagram);
			shape2.Shape.BNS_Name = "Shape2";
			shape2.Shape.BNS_Status = "CAN";

			var shape3 = networkViewModel.CreateNewShape(diagram);
			shape3.Shape.BNS_Name = "Shape3";
			shape3.Shape.BNS_Status = "WRK";

			var shape4 = networkViewModel.CreateNewShape(diagram);
			shape4.Shape.BNS_Name = "Shape4";
			shape4.Shape.BNS_Status = "SUS";

			var shape5 = networkViewModel.CreateNewShape(diagram);
			shape5.Shape.BNS_Name = "Shape5";
			shape5.Shape.BNS_Status = "ASN";

			var shape6 = networkViewModel.CreateNewShape(diagram);
			shape6.Shape.BNS_Name = "Shape6";
			shape6.Shape.BNS_Status = "OPN";

			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagram))
			{
				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				Application.DoEvents();

				var nodes = networkDiagram.FindChildren<AdornedControl>().Where(x => x.DataContext is NodeViewModel).ToArray();
				var shapeNode1 = nodes.Select(n => n.DataContext).Cast<NodeViewModel>().Single(n => n.Name == "Shape1");
				var shapeNode2 = nodes.Select(n => n.DataContext).Cast<NodeViewModel>().Single(n => n.Name == "Shape2");
				var shapeNode3 = nodes.Select(n => n.DataContext).Cast<NodeViewModel>().Single(n => n.Name == "Shape3");
				var shapeNode4 = nodes.Select(n => n.DataContext).Cast<NodeViewModel>().Single(n => n.Name == "Shape4");
				var shapeNode5 = nodes.Select(n => n.DataContext).Cast<NodeViewModel>().Single(n => n.Name == "Shape5");
				var shapeNode6 = nodes.Select(n => n.DataContext).Cast<NodeViewModel>().Single(n => n.Name == "Shape6");

				AssertEquals(shapeNode1.Status, WorkStatus.Complete);
				AssertEquals(shapeNode2.Status, WorkStatus.Cancelled);
				AssertEquals(shapeNode3.Status, WorkStatus.Working);
				AssertEquals(shapeNode4.Status, WorkStatus.Suspended);
				AssertEquals(shapeNode5.Status, WorkStatus.None);
				AssertEquals(shapeNode6.Status, WorkStatus.None);
			}
		}

		#endregion

		#region Hidden Items 

		public void TestHiddenCollectionShouldNotIncludeDeletedWorkflows()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var diagramShape = NetworkTestCase.CreateDiagram(jobHeader);
			var network = NetworkTestCase.CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;
			var subDiagram = NetworkTestCase.CreateShape(jobHeader, diagram, name: "SubDiagram");

			var workflowToDelete = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow to delete");

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram.Shape))
			{
				form.Show();
				Application.DoEvents();
				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				var nodes = networkDiagram.FindChildren<AdornedControl>().Where(x => x.DataContext is NodeViewModel).ToArray();
				var shapeNode = nodes.Select(n => n.DataContext).Cast<NodeViewModel>().Single(n => n.Name == "SubDiagram");

				foreach (var item in GetAllNetworkActionMenuItems(shapeNode))
				{
					AssertNotNull(item.Name);
				}

				Assert(GetAllNetworkActionMenuItems(shapeNode).Any(s => s.Name.Contains("Workflow to delete")));

				workflowToDelete.Delete();
				Factory.Save();

				foreach (var item in GetAllNetworkActionMenuItems(shapeNode))
				{
					AssertNotNull(item.Name);
				}

				Assert(!GetAllNetworkActionMenuItems(shapeNode).Any(s => s.Name.Contains("Workflow to delete")));
			}
		}

		static IEnumerable<NetworkActionMenuItem> GetAllNetworkActionMenuItems(NodeViewModel model)
		{
			model.ReloadMenuItems();
			return model.MenuItems.Where(i => i != null).SelectMany(i => i.SelectRecursive(s => s.Items.Where(q => q != null)).Append(i));
		}

		#endregion

		#region Default Diagrams

		WorkflowsUserControl GetWorkflowsUserControl(Form form)
		{
			var tabControl = form.FindAll<ZTemplateTabControl>().Single();

			var workflowTab = tabControl.TabPages.IndexOfKey("WorkflowTabPage");
			tabControl.SelectedIndex = workflowTab;

			var control = tabControl.TabPages[workflowTab].Controls[0];
			var userControl = control.FindAll<WorkflowsUserControl>().Single();

			return userControl;
		}

		void AssertDefaultDiagramExistsInDatabase(ProcessJobHeader jobHeader)
		{
			var defaultShapeQuery = new ZQuery(BMNCNShapeSchema.BNS_RelatedEntityID, jobHeader.PK);
			defaultShapeQuery.AddToFilter(BMNCNShapeSchema.BNS_ShapeType, ShapeTypeList.Codes.DefaultDiagram);
			defaultShapeQuery.IncludeBlob(BMNCNShapeSchema.BNS_LayoutData);
			defaultShapeQuery.FetchOnlyFromLocalCache = !jobHeader.IsInDatabase;

			AssertNotNull("Will create a new diagram, when the default should be loaded", jobHeader.Factory.LoadTop1<BMNCNShapeDefaultDiagram>(defaultShapeQuery));
		}

		public void TestUpdateNCNWhenEditedWorkflow_ShouldSaveAndUpdate()
		{
			var oldShapeName = "initial name";
			var newShapeName = "new name";

			var workItem = Factory.New<IWorkItem>();
			var jobHeader = GetJobHeaderWithCustomWorkflow(workItem, oldShapeName);
			var diagramShape = CreateDefaultDiagram(jobHeader);

			var descendents = diagramShape.GetShapesWithinSameDiagram();

			var shapeToBeEdited = descendents.Single();
			shapeToBeEdited.BNS_Name = "jess' wrath";
			Factory.Save();

			AssertDefaultDiagramExistsInDatabase(jobHeader);

			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.WorkItem).ShowEditForm((BusinessObject)workItem))
			{
				form.Show();
				Application.DoEvents();

				var userControl = GetWorkflowsUserControl(form);

				userControl.WorkflowsGrid[1, 2] = new ZString(newShapeName);
				userControl.WorkflowNCNTabControl_Exposed.SelectedTab = userControl.NCNTabPage_Exposed;
				Application.DoEvents();

				var networkDiagram = GetNetworkUserControl(userControl);

				var node = networkDiagram.FindNodeItemOrDefault(newShapeName);
				AssertNotNull(string.Format("Shape with name '{0}' was not found in the diagram. The diagram shapes should contain the correct names.", newShapeName), node);

				Application.DoEvents();
				Factory.Save();

				AssertEquals("List of workflows should show the correct Completion Statement", newShapeName, userControl.WorkflowsGrid[1, 2]);
			}
		}

#pragma warning disable CS0618 // Type or member is obsolete
		NetworkUserControl GetNetworkUserControl(WorkflowsUserControl userControl)
#pragma warning restore CS0618 // Type or member is obsolete
		{
#pragma warning disable CS0618 // Type or member is obsolete
			return (NetworkUserControl)userControl.WorkflowNCNTabControl_Exposed.FindAll<ZElementHost>().Single().Child;
#pragma warning restore CS0618 // Type or member is obsolete
		}

		public void TestUpdateNCNWhenEditedWorkflow_ShouldSaveAndUpdateErrorShapes()
		{
			var oldShapeName = "other initial name";
			var newShapeName = "new name";

			var workItem = Factory.New<IWorkItem>();
			var diagramShape = CreateDefaultDiagram(GetJobHeaderWithCustomWorkflow(workItem, "testshape"));

			var descendents = diagramShape.GetShapesWithinSameDiagram();
			var shape1 = descendents.SingleOrDefault(n => n.Name == "testshape");
			shape1.BNS_Name = oldShapeName;

			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.WorkItem).ShowEditForm((BusinessObject)workItem))
			{
				form.Show();
				Application.DoEvents();

				var userControl = GetWorkflowsUserControl(form);

				userControl.WorkflowNCNTabControl_Exposed.SelectedTab = userControl.NCNTabPage_Exposed;
				Application.DoEvents();

				var networkDiagram = GetNetworkUserControl(userControl);

				userControl.WorkflowsGrid[1, 2] = new ZString(newShapeName);

				var node = networkDiagram.FindNodeItemOrDefault(newShapeName);
				AssertNotNull(string.Format("Shape with name '{0}' was not found in the diagram. The diagram shapes should contain the correct names.", newShapeName), node);
				Factory.Save();
			}
		}

		ProcessJobHeader GetJobHeaderWithCustomWorkflow(IWorkItem workItem, string completionStatement)
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory, "WKI");
			Factory.Save();

			var jobHeader = ProcessJobHeader.GetForParent((IWorkflowProvider)workItem, Factory);
			jobHeader.ProcessHeaders[0].FH_CompletionStatement = completionStatement;

			Factory.Save();

			return jobHeader;
		}

		BMNCNShapeDefaultDiagram CreateDefaultDiagram(ProcessJobHeader jobHeader)
		{
			var diagramShape = jobHeader.Factory.New<BMNCNShapeDefaultDiagram>();
			diagramShape.BNS_RelatedEntityID = jobHeader.PK;
			Factory.Save();

			return diagramShape;
		}

		#endregion

		#region Buffer Penetration

		public void Test_BufferPenetration_WhenInvalidBranchOrDeparment_ShouldShowErrorMsgAndOpenDetailsForm()
		{
			BMSTestHelper.CreateSystem(Factory, "DUM");
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow");

			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var shape = NetworkTestCase.CreateShape(workflow, diagram);
			var nodeViewModelProvider = new JobNetworkNodeViewModelProvider();
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram, nodeViewModelProvider: nodeViewModelProvider);
			var network = networkViewModel.GetJobNetwork();

			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			diagram.ScheduleBizo.BNC_GB_Branch = Guid.Empty;
			diagram.ScheduleBizo.BNC_GE_Department = Guid.Empty;
			shape.ScheduleBizo.BNC_GB_Branch = Guid.Empty;
			shape.ScheduleBizo.BNC_GE_Department = Guid.Empty;

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var diagramFromNewFactory = newFactory.Load<BMNCNRootDiagramShape>(diagram.PK);

			using (var form = new NetworkDiagramForm(diagram))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				UnitTestUserNotification.PreviousMessage previousMessage = null;
				ShapeEntityDetailsForm detailsForm = null;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((dialogForm) =>
				{
					if (dialogForm is ShapeEntityDetailsForm)
					{
						previousMessage = UnitTestUserNotification.Instance.LastMessage;
						detailsForm = dialogForm as ShapeEntityDetailsForm;

						var detailsControl = detailsForm.FindSingle<ShapeDetailsUserControl>();
						var scaleControl = detailsControl.FindSingle<ShapePropertiesUserControl>();
						var branchFindBox = scaleControl.FindSingle<ZGuidFindBox>(c => c.Name == "ScheduleBranchFindBox");
						var departmentFindBox = scaleControl.FindSingle<ZGuidFindBox>(c => c.Name == "ScheduleDepartmentFindBox");

						AssertEquals(ZGuid.Empty, branchFindBox.Guid);
						AssertEquals(ZGuid.Empty, departmentFindBox.Guid);

						var shapeNetworkEntity = detailsForm.DataSource as ShapeNetworkEntity;

						AssertNotNull(shapeNetworkEntity);

						var scaleDescriptor = shapeNetworkEntity.Network.ScaleDescriptor;

						Assert(scaleDescriptor.IsScaleDescriptorInvalidated);
						Assert(scaleDescriptor.IsBranchOrDepartmentInvalidated);

						var scheduleBizo = shapeNetworkEntity.AsShape().ScheduleBizo;

						AssertHasError(scheduleBizo.BNC_GB_BranchInfo, "Please enter a Branch.");
						AssertHasError(scheduleBizo.BNC_GE_DepartmentInfo, "Please enter a Department.");

						scheduleBizo.BNC_GB_Branch = Env.CurrentBranchPK;
						scheduleBizo.BNC_GE_Department = Env.CurrentDepartmentPK;

						UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					}
				});

				form.Show();
				NetworkGUITestCase.DoEventsThoroughly();

				AssertNotNull("Error message was displayed before show details form", previousMessage);
				Assert("Error message was an error", previousMessage.WasError);
				AssertEquals("Error message caption", "Diagram needs Branch and Department", previousMessage.Caption);
				AssertEquals("Error message text", "In order to calculate values, this diagram requires a Branch and Department. Please select the missing Branch and Department values.", previousMessage.Text);

				AssertNotNull("Details Form was shown", detailsForm);
				AssertEquals("Last pop-up was the details form", detailsForm, ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		[TestDate(2016, 12, 9)]
		public void TestBufferPenetration_ClosedShapesShouldHaveEmptyBufferPenetration()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();
				var networkViewModel = form.NetworkViewModel;
				var network = form.Network;

				diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow.AddDays(-1);
				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				var shape1 = networkViewModel.CreateNewShape(network.DiagramEntity);
				shape1.SetWidthForDuration(100000);
				var shape2 = networkViewModel.CreateNewShape(network.DiagramEntity);
				shape1.Shape.MakeVisiblePrerequisiteOf(shape2.Shape, diagram);

				network.ScaleAndRefresh();
				networkViewModel.PushAsLateAsPossible();
				networkViewModel.SuggestAndAcceptAllBuffers();
				networkViewModel.ToggleApproval();

				form.FireSaveButton();
				Application.DoEvents();

				var buffer = network.Shapes.OfType<BMNCNBufferShape>().Single();
				AssertEquals(1m, buffer.BufferPenetration);

				shape1.Shape.BNS_Status = ProcessTaskStatusCodeList.Codes.Closed;
				form.FireSaveButton();
				Application.DoEvents();

				AssertEquals(0m, buffer.BufferPenetration);
			}
		}

		#endregion

		#region Leveling Rules

		public void TestLevelingRulesViolatedOnFormShow()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.DiagramShape.Factory.Save();

			var rule = NetworkTestCase.CreateLevelingRule(diagram, type: LevelingRuleTypeList.Codes.MaximumConcurrentEntities, value: 1, name: "Con the Fruiterer");

			var shape1 = networkViewModel.CreateNewShape(network.DiagramShape);
			shape1.Shape.BNS_Name = "Shape 1";
			shape1.X = 0;
			shape1.Y = 100;
			shape1.Width = 200;
			shape1.Height = 100;

			var shape2 = networkViewModel.CreateNewShape(network.DiagramShape);
			shape2.Shape.BNS_Name = "Shape 2";
			shape2.X = 0;
			shape2.Y = 300;
			shape2.Width = 200;
			shape2.Height = 100;

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				var diagramControl = networkDiagram.MainDiagramControl;

				var node1 = networkDiagram.FindNodeItemOrDefault("Shape 1");
				var node2 = networkDiagram.FindNodeItemOrDefault("Shape 2");

				CombineAssertions("Shapes should have the leveling rule violation applied", () =>
				{
					AssertHasWarningMessageOnShape(true, node1, "There are more shapes in this time slot than are allowed by the Leveling Rule named [Con the Fruiterer].");
					AssertHasWarningMessageOnShape(true, node2, "There are more shapes in this time slot than are allowed by the Leveling Rule named [Con the Fruiterer].");
				});
			}
		}

		public void TestDraggingAndResizingShapesShouldUpdateLevelingRuleViolations()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.DiagramShape.Factory.Save();

			var rule1 = NetworkTestCase.CreateLevelingRule(diagram, type: LevelingRuleTypeList.Codes.MaximumConcurrentEntities, value: 1, name: "Con the Fruiterer", colorName: System.Drawing.Color.Chartreuse.Name);
			var rule2 = NetworkTestCase.CreateLevelingRule(diagram, type: LevelingRuleTypeList.Codes.MinimumEntityStartGapSize, value: 2, name: "Kylie Mole", colorName: System.Drawing.Color.PapayaWhip.Name);
			var rule3 = NetworkTestCase.CreateLevelingRule(diagram, type: LevelingRuleTypeList.Codes.MinimumGapSizeBetweenEntities, value: 1, name: "Uncle Arthur", colorName: System.Drawing.Color.BurlyWood.Name);

			var shape1 = networkViewModel.CreateNewShape(network.DiagramShape);
			shape1.Shape.BNS_Name = "Shape 1";
			shape1.X = 0;
			shape1.Y = 100;
			shape1.Width = 200;
			shape1.Height = 100;

			var shape2 = networkViewModel.CreateNewShape(network.DiagramShape);
			shape2.Shape.BNS_Name = "Shape 2";
			shape2.X = 0;
			shape2.Y = 300;
			shape2.Width = 200;
			shape2.Height = 100;

			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagram))
			{
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				var diagramControl = networkDiagram.MainDiagramControl.FindName("NetworkControl") as NetworkView;

				var node1 = networkDiagram.FindNodeItemOrDefault("Shape 1");
				var shapeViewModel1 = node1.DataContext as NodeViewModel;

				var node2 = networkDiagram.FindNodeItemOrDefault("Shape 2");
				var shapeViewModel2 = node2.DataContext as NodeViewModel;

				CombineAssertions("Shapes should have the concurrency and start leveling rule violations applied", () =>
				{
					AssertHasWarningMessageOnShape(true, node1, "There are more shapes in this time slot than are allowed by the Leveling Rule named [Con the Fruiterer].");
					AssertHasWarningMessageOnShape(true, node2, "There are more shapes in this time slot than are allowed by the Leveling Rule named [Con the Fruiterer].");
					AssertHasWarningMessageOnShape(true, node1, "The distance between the start of shapes is too small, as specified in the Leveling Rule named [Kylie Mole].");
					AssertHasWarningMessageOnShape(true, node2, "The distance between the start of shapes is too small, as specified in the Leveling Rule named [Kylie Mole].");
				});

				shapeViewModel2.X = 100;
				diagramControl.RaiseEvent(new NodeDragCompletedEventArgs(NetworkView.NodeDragCompletedEvent, node2, new NodeViewModel[] { shapeViewModel1, shapeViewModel2 }));

				CombineAssertions("Shapes should still have the concurrency leveling rule violation applied, but only the second shape should violate the start rule", () =>
				{
					AssertHasWarningMessageOnShape(true, node1, "There are more shapes in this time slot than are allowed by the Leveling Rule named [Con the Fruiterer].");
					AssertHasWarningMessageOnShape(true, node2, "There are more shapes in this time slot than are allowed by the Leveling Rule named [Con the Fruiterer].");
					AssertHasWarningMessageOnShape(false, node1, "The distance between the start of shapes is too small, as specified in the Leveling Rule named [Kylie Mole].");
					AssertHasWarningMessageOnShape(true, node2, "The distance between the start of shapes is too small, as specified in the Leveling Rule named [Kylie Mole].");
				});

				shapeViewModel2.X = 200;
				diagramControl.RaiseEvent(new NodeDragCompletedEventArgs(NetworkView.NodeDragCompletedEvent, node2, new NodeViewModel[] { shapeViewModel1, shapeViewModel2 }));

				CombineAssertions("Only the second shape should have the gap violation applied", () =>
				{
					AssertNoWarningMessagesOnShape(node1);
					AssertHasWarningMessageOnShape(true, node2, "The distance between the end of one or more shapes and the start of this shape is too small, as specified in the Leveling Rule named [Uncle Arthur].");
				});

				shapeViewModel2.X = 300;
				diagramControl.RaiseEvent(new NodeDragCompletedEventArgs(NetworkView.NodeDragCompletedEvent, node2, new NodeViewModel[] { shapeViewModel1, shapeViewModel2 }));

				CombineAssertions("No violations", () =>
				{
					AssertNoWarningMessagesOnShape(node1);
					AssertNoWarningMessagesOnShape(node2);
				});

				shapeViewModel1.Width = 500;
				diagramControl.RaiseEvent(new NodeResizeCompletedEventArgs(NetworkView.NodeResizeCompletedEvent, node1, new NodeItem[] { node1, node2 }));

				CombineAssertions("First shape resized, shapes should once again violate the concurrency rule", () =>
				{
					AssertHasWarningMessageOnShape(true, node1, "There are more shapes in this time slot than are allowed by the Leveling Rule named [Con the Fruiterer].");
					AssertHasWarningMessageOnShape(true, node2, "There are more shapes in this time slot than are allowed by the Leveling Rule named [Con the Fruiterer].");
				});
			}
		}

		void AssertHasWarningMessageOnShape(bool hasWarning, NodeItem node, string message)
		{
			AssertEquals(hasWarning, node.FindChildren<Rectangle>().Where(r => r.ToolTip != null).Select(r => r.ToolTip.ToString()).Any(t => t.Contains(message)));
		}

		void AssertNoWarningMessagesOnShape(NodeItem node)
		{
			AssertEquals(false, node.FindChildren<Rectangle>().Where(r => r.ToolTip != null).Select(r => r.ToolTip.ToString()).Any());
		}

		#endregion

		#region Diagram Scheduling

		public void TestMoveShapesInsideDiagramDatesRange_NoDatesConflict()
		{
			var diagram = NetworkTestCase.CreateNetwork(NetworkTestCase.CreateDiagram(Factory, isScaled: true), refreshSchedules: true).DiagramEntity;

			using (var form = new NetworkDiagramForm(diagram.Shape))
			{
				form.Show();
				Application.DoEvents();
				var networkViewModel = form.NetworkViewModel;
				var network = form.Network;

				diagram.Shape.ScheduledStartTimeUtc = ZDateTime.UtcNow.AddDays(-10);
				diagram.Shape.ScheduledFinishTimeUtc = ZDateTime.UtcNow.AddDays(10);
				var shape1 = networkViewModel.CreateNewShape(network.DiagramEntity);
				shape1.SetWidthForDuration(3000);
				form.FireSaveButton();
				Application.DoEvents();

				diagram.Shape.ScheduledStartTimeUtc = ZDateTime.UtcNow.AddDays(-8);
				diagram.Shape.ScheduledFinishTimeUtc = ZDateTime.UtcNow.AddDays(8);
				network.EditEntity(network.DiagramEntity); // To cause schedules to update

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2017, 04, 21, 13, 24, 01)]
		public void TestScaledDiagramCanvasArea_WhenScheduledFinishTimeAdded_HeightShouldNotBeFixed()
		{
			var diagramShape = NetworkTestCase.CreateDiagram(Factory, isScaled: true, name: "NAAAME");
			var diagram = NetworkTestCase.CreateNetwork(diagramShape).DiagramEntity;

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagramShape))
			{
				form.Height = ControlDpiScalingHelper.ScaleToCurrentDpiX(1000);
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				var diagramControl = networkControl.MainDiagramControl;
				var zoomAndPanControl = (ZoomAndPanControl)diagramControl.FindName("ZoomAndPanControl");
				var originalHeight = (zoomAndPanControl.Content as AdornerDecorator).ActualHeight;

				var shapeEntity1 = NetworkTestCase.CreateShape(diagram);
				var node1 = diagramControl.ViewModel.CreateNewNode(new Location(0, 1), shapeEntity1);
				node1.Name = "BLAAANK";
				node1.Width = 300;
				node1.Height = 150;

				NetworkGUITestCase.InvokeEditPropertiesActionOnFormAndEnterValueIntoField<ZDateEdit>(form, "ScheduledStartDateEdit", "T");
				NetworkGUITestCase.InvokeEditPropertiesActionOnFormAndEnterValueIntoField<ZDateEdit>(form, "ScheduledFinishDateEdit", "T+10");

				var shapeEntity2 = NetworkTestCase.CreateShape(diagram);
				var node2 = diagramControl.ViewModel.CreateNewNode(new Location(300, 100), shapeEntity2);
				node2.Name = "BLONNK";
				node2.Width = 300;
				node2.Height = 150;
				NetworkGUITestCase.DoEventsThoroughly();

				AssertEquals("The height of the diagram control viewport should not change", originalHeight, (zoomAndPanControl.Content as AdornerDecorator).ActualHeight);

				node2.Y = 1000;
				form.Network.Refresher.Refresh(RefreshType.RedrawDiagram);
				NetworkGUITestCase.DoEventsThoroughly();

				AssertNotEquals("The height of the diagram control viewport should now change now that we have moved the shape", originalHeight, (zoomAndPanControl.Content as AdornerDecorator).ActualHeight);
			}
		}

		[TestDate(2019, 08, 21, 13, 29, 00)]
		public void TestScaledDiagram_UpdateStartAndFinishDates_FirstAndLastColumnDatesShouldMatch_NoShapes()
		{
			var diagramShape = NetworkTestCase.CreateDiagram(Factory, isScaled: true, name: "The Justified Ancients of Mummu");

			using (var form = new NetworkDiagramForm(diagramShape))
			{
				form.Height = ControlDpiScalingHelper.ScaleToCurrentDpiX(1000);
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				var diagramControl = networkControl.MainDiagramControl;

				NetworkGUITestCase.InvokeEditPropertiesActionOnFormAndEnterValueIntoField<ZDateEdit>(form, "ScheduledStartDateEdit", "T");
				NetworkGUITestCase.InvokeEditPropertiesActionOnFormAndEnterValueIntoField<ZDateEdit>(form, "ScheduledFinishDateEdit", "T+21");

				var scheduledGrid = NetworkTestHelper.GetDynamicGrid(diagramControl);
				var length = scheduledGrid.ScaleSet.ScalePoints.Count;
				AssertEquals("21-Aug-19 13:29", scheduledGrid.ScaleSet.ScalePoints[0].Label);
				AssertEquals("11-Sep-19 05:29", scheduledGrid.ScaleSet.ScalePoints[length - 1].Label);

				form.FireSaveButton();
				Application.DoEvents();

				scheduledGrid = NetworkTestHelper.GetDynamicGrid(diagramControl);
				length = scheduledGrid.ScaleSet.ScalePoints.Count;
				AssertEquals("21-Aug-19 13:29", scheduledGrid.ScaleSet.ScalePoints[0].Label);
				AssertEquals("11-Sep-19 05:29", scheduledGrid.ScaleSet.ScalePoints[length - 1].Label);

				NetworkGUITestCase.InvokeEditPropertiesActionOnFormAndEnterValueIntoField<ZDateEdit>(form, "ScheduledStartDateEdit", "T-7");

				scheduledGrid = NetworkTestHelper.GetDynamicGrid(diagramControl);
				length = scheduledGrid.ScaleSet.ScalePoints.Count;
				AssertEquals("14-Aug-19 13:29", scheduledGrid.ScaleSet.ScalePoints[0].Label);
				AssertEquals("11-Sep-19 05:29", scheduledGrid.ScaleSet.ScalePoints[length - 1].Label);

				form.FireSaveButton();
				Application.DoEvents();

				scheduledGrid = NetworkTestHelper.GetDynamicGrid(diagramControl);
				length = scheduledGrid.ScaleSet.ScalePoints.Count;
				AssertEquals("14-Aug-19 13:29", scheduledGrid.ScaleSet.ScalePoints[0].Label);
				AssertEquals("11-Sep-19 05:29", scheduledGrid.ScaleSet.ScalePoints[length - 1].Label);

				NetworkGUITestCase.InvokeEditPropertiesActionOnFormAndEnterValueIntoField<ZDateEdit>(form, "ScheduledFinishDateEdit", "T+28");

				scheduledGrid = NetworkTestHelper.GetDynamicGrid(diagramControl);
				length = scheduledGrid.ScaleSet.ScalePoints.Count;
				AssertEquals("14-Aug-19 13:29", scheduledGrid.ScaleSet.ScalePoints[0].Label);
				AssertEquals("18-Sep-19 05:29", scheduledGrid.ScaleSet.ScalePoints[length - 1].Label);

				form.FireSaveButton();
				Application.DoEvents();

				scheduledGrid = NetworkTestHelper.GetDynamicGrid(diagramControl);
				length = scheduledGrid.ScaleSet.ScalePoints.Count;
				AssertEquals("14-Aug-19 13:29", scheduledGrid.ScaleSet.ScalePoints[0].Label);
				AssertEquals("18-Sep-19 05:29", scheduledGrid.ScaleSet.ScalePoints[length - 1].Label);
			}
		}

		[TestDate(2019, 08, 21, 13, 29, 00)]
		public void TestScaledDiagram_UpdateStartAndFinishDates_FirstAndLastColumnDatesShouldMatch_WithShape()
		{
			var diagramShape = NetworkTestCase.CreateDiagram(Factory, isScaled: true, name: "Kopyright Liberation Front");

			using (var form = new NetworkDiagramForm(diagramShape))
			{
				form.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(2000);
				form.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(1000);
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				var diagramControl = networkControl.MainDiagramControl;

				NetworkGUITestCase.InvokeEditPropertiesActionOnFormAndEnterValueIntoField<ZDateEdit>(form, "ScheduledStartDateEdit", "T");
				NetworkGUITestCase.InvokeEditPropertiesActionOnFormAndEnterValueIntoField<ZDateEdit>(form, "ScheduledFinishDateEdit", "T+14");

				form.FireSaveButton();
				Application.DoEvents();

				var scheduledGrid = NetworkTestHelper.GetDynamicGrid(diagramControl);
				var length = scheduledGrid.ScaleSet.ScalePoints.Count;
				AssertEquals("21-Aug-19 13:29", scheduledGrid.ScaleSet.ScalePoints[0].Label);
				AssertEquals("04-Sep-19 05:29", scheduledGrid.ScaleSet.ScalePoints[length - 1].Label);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				var shape1 = form.NetworkViewModel.CreateNewShape(form.Network.DiagramEntity);
				shape1.SetCoordinates(width: 480, height: 480, x: 2000, y: 0);

				NetworkGUITestCase.InvokeEditPropertiesActionOnFormAndEnterValueIntoField<ZDateEdit>(form, "ScheduledStartDateEdit", "T+7");

				scheduledGrid = NetworkTestHelper.GetDynamicGrid(diagramControl);
				length = scheduledGrid.ScaleSet.ScalePoints.Count;
				CombineAssertions(() =>
				{
					AssertEquals("28-Aug-19 13:29", scheduledGrid.ScaleSet.ScalePoints[0].Label);
					AssertEquals("04-Sep-19 05:29", scheduledGrid.ScaleSet.ScalePoints[length - 1].Label);
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				});

				form.FireSaveButton();
				Application.DoEvents();

				scheduledGrid = NetworkTestHelper.GetDynamicGrid(diagramControl);
				length = scheduledGrid.ScaleSet.ScalePoints.Count;
				AssertEquals("28-Aug-19 13:29", scheduledGrid.ScaleSet.ScalePoints[0].Label);
				AssertEquals("04-Sep-19 05:29", scheduledGrid.ScaleSet.ScalePoints[length - 1].Label);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2017, 04, 21, 13, 24, 01)]
		public void TestMoveShapesInsideDiagramDatesRange_HasDatesConflict()
		{
			var diagram = NetworkTestCase.CreateNetwork(NetworkTestCase.CreateDiagram(Factory, isScaled: true), refreshSchedules: true).DiagramEntity;

			using (var form = new NetworkDiagramForm(diagram.Shape))
			{
				form.Show();
				Application.DoEvents();
				var networkViewModel = form.NetworkViewModel;
				var network = form.Network;

				diagram.Shape.ScheduledStartTimeUtc = ZDateTime.UtcNow.AddDays(-8);
				diagram.Shape.ScheduledFinishTimeUtc = ZDateTime.UtcNow.AddDays(8);

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				var shape1 = networkViewModel.CreateNewShape(network.DiagramEntity);
				shape1.SetWidthForDuration(4800);
				var shape2 = networkViewModel.CreateNewShape(network.DiagramEntity);
				shape2.X = 100;
				shape2.SetWidthForDuration(1200);
				networkViewModel.SuggestAndAcceptAllBuffers();
				form.FireSaveButton();
				Application.DoEvents();

				diagram.Shape.ScheduledStartTimeUtc = ZDateTime.UtcNow.AddDays(-3);
				diagram.Shape.ScheduledFinishTimeUtc = ZDateTime.UtcNow.AddDays(3);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				network.EditEntity(network.DiagramEntity); // To cause schedules to update
				AssertEquals("The diagram schedule dates have changed and this diagram contains shapes which are outside the new date range. \r\nPress OK if you want to move these shapes inside the diagram bounds. Otherwise press Cancel and modify the diagram Schedule Start/Finish Date.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				network.EditEntity(network.DiagramEntity); // To cause schedules to update
				Assert(ZFormModaliser.LastFormShownDialogForTest is ShapeEntityDetailsForm);
			}
		}

		[TestDate(2017, 04, 21, 13, 24, 01)]
		public void TestSetDiagramScheduleFinishDate_CannotResolveDatesConflict()
		{
			var diagram = NetworkTestCase.CreateNetwork(NetworkTestCase.CreateDiagram(Factory, isScaled: true), refreshSchedules: true).DiagramEntity;

			using (var form = new NetworkDiagramForm(diagram.Shape))
			{
				form.Show();
				Application.DoEvents();
				var networkViewModel = form.NetworkViewModel;
				var network = form.Network;

				diagram.Shape.ScheduledStartTimeUtc = ZDateTime.UtcNow.AddDays(-3);
				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				var shape1 = networkViewModel.CreateNewShape(network.DiagramEntity);
				shape1.SetWidthForDuration(6800);
				var shape2 = networkViewModel.CreateNewShape(network.DiagramEntity);
				shape2.SetWidthForDuration(1200);
				form.FireSaveButton();
				Application.DoEvents();

				diagram.Shape.ScheduledFinishTimeUtc = ZDateTime.UtcNow.AddDays(2);
				network.EditEntity(network.DiagramEntity); // To cause schedules to update
				AssertEquals("The time range of a diagram is too small to fit the biggest shape on this diagram and new schedule dates cannot be committed.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Exception Handling

		public void TestControllerShouldNotGenerateExceptions_WhenOpeningSearchFormFromMainMenu()
		{
			var diagramShape = NetworkTestCase.CreateDiagram(Factory, name: "Root", isScaled: false);
			var diagram = NetworkTestCase.CreateNetwork(diagramShape).DiagramEntity;

			using (var form = new NetworkDiagramForm(diagram.Shape))
			{
				form.Show();
				Application.DoEvents();

				AssertNoExceptionThrown(() =>
				{
					form.OpenFinderForm();
					form.Close();
				});
			}
		}

		public void TestShouldNotGenerateExceptions_AfterRemovingNodeFromDiagram_UsingContextMenu()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var diagramShape = NetworkTestCase.CreateDiagram(jobHeader);
			var network = NetworkTestCase.CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;
			var shape = NetworkTestCase.CreateShape(jobHeader, diagram, name: "Shape");

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram.Shape))
			{
				form.Show();
				Application.DoEvents();
				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				var formNetworkViewModel = networkDiagram.NetworkViewModel;
				var node = formNetworkViewModel.Nodes.Single();

				formNetworkViewModel.SelectSingleEntity(node.Entity);

				var mainDiagramArea = networkDiagram.MainDiagramControl;
				mainDiagramArea.OnNodeContextMenuOpening(node);

				var removeFromDiagramMenuItem = formNetworkViewModel.GetFirstLevelNetworkActionsMenuItem_ForTesting(node.Entity, "Remove from Diagram");
				AssertNotNull(removeFromDiagramMenuItem);
				NetworkActionAccessibilityTest.AssertAllowed(removeFromDiagramMenuItem.Action.CheckCanStartExecution());

				AssertNoExceptionThrown(() =>
				{
					mainDiagramArea.ExecuteContextMenuAction(removeFromDiagramMenuItem.Action);
					Application.DoEvents();
					Assert(!formNetworkViewModel.Nodes.Any());

					mainDiagramArea.CloseContextMenu();
				});
			}
		}

		#endregion

		#region Channels

		public void TestChannelGridLines_ForChanneledDiagram_WithShapeBelowFinalChannelHeight_ShouldNotDrawExtraHorizontalLine()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, "World Domination", isScaled: true);
			var shape = NetworkTestCase.CreateShape(diagram, "World Peace");
			var network = NetworkTestCase.CreateNetwork(diagram);

			NetworkTestCase.SetShapeOffset(shape, diagram, network, 0, 4000);

			var channel1 = NetworkTestCase.CreateChannel(diagram, "Halcyon");
			var channel2 = NetworkTestCase.CreateChannel(diagram, "Procyon");

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				var control = NetworkGUITestCase.FindNetworkUserControl(form);
				var grid = NetworkTestHelper.GetDynamicGrid(control.MainDiagramControl);

				var lines = NetworkTestHelper.GetChannelLines(grid);
				var headings = NetworkTestHelper.GetChannelHeaders(grid).Select(tb => tb.Text);

				AssertEquals("Should create one line per real channel, with the non-channeled channel just expanding to the end of the diagram", 2, lines.Count());
				AssertSequencesEqual(new[] { "Halcyon", "Procyon", "Non-channeled" }, headings);
			}
		}

		public void TestAddDiagramCompletionStatement_ForDiagramWithChannels_ShouldMoveChannelsDown()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, "Squanch", isScaled: true);
			NetworkTestCase.CreateChannel(diagram, "Baby Legs");
			NetworkTestCase.CreateChannel(diagram, "Regular Legs");

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				NetworkGUITestCase.DoEventsThoroughly();

				var control = NetworkGUITestCase.FindNetworkUserControl(form);
				var grid = NetworkTestHelper.GetDynamicGrid(control.MainDiagramControl);
				var startingGridPosition = NetworkTestHelper.GetRelativePosition(grid, control).Y;

				var completionCriteriaTextBox = control.FindChildren<TextBoxWithPlaceholder>().Single(x => x.Name == "CompletionCriteriaTextBox");
				completionCriteriaTextBox.Text = "Headward free now to rise";
				NetworkGUITestCase.DoEventsThoroughly();

				var gridPosition = NetworkTestHelper.GetRelativePosition(grid, control).Y;
				AssertEquals("The completion statement just replaced the placeholder text, so the grid shouldn't move. SAD!", startingGridPosition, gridPosition);

				completionCriteriaTextBox.Text = @"Headward free now to rise,
Headward free now to rise!";
				NetworkGUITestCase.DoEventsThoroughly();

				gridPosition = NetworkTestHelper.GetRelativePosition(grid, control).Y;
				AssertGreaterThan("A second completion statement line has been added, so the grid should have moved down. SAD!", gridPosition, startingGridPosition);
			}
		}

		public void TestRemoveDiagramCompletionStatement_ForDiagramWithChannels_ShouldMoveChannelsDown()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, "Squanch", isScaled: true);
			diagram.BNS_CompletionStatements = @"Encryption?
Who needs encryption?
Only people who have something to hide.
Oh wait, I mean everyone, and our entire economy.
Oh well... let's break it anyway.
Because voters are stupid and will hate us if we don't.";
			NetworkTestCase.CreateChannel(diagram, "Liberals who know nothing about tech");
			NetworkTestCase.CreateChannel(diagram, "Labor who knows nothing about tech");

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				NetworkGUITestCase.DoEventsThoroughly();

				var control = NetworkGUITestCase.FindNetworkUserControl(form).MainDiagramControl;
				var grid = NetworkTestHelper.GetDynamicGrid(control);
				var startingGridPosition = NetworkTestHelper.GetRelativePosition(grid, control).Y;

				var completionCriteriaTextBox = control.FindChildren<TextBoxWithPlaceholder>().Single(x => x.Name == "CompletionCriteriaTextBox");
				completionCriteriaTextBox.Text = @"Screw the country,
Let's just do whatever it takes to get elected.";
				NetworkGUITestCase.DoEventsThoroughly();

				var gridPosition = NetworkTestHelper.GetRelativePosition(grid, control).Y;
				AssertLessThan("The completion statement got shorter, so the grid should have moved up. SAD!", gridPosition, startingGridPosition);
			}
		}

		#endregion

		#region Non-Scheduled Section

		public void TestNonScheduledSection_EntityDetails_ShouldSayNonScheduledItems_InsteadOfDiagramName()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, "This is MY diagram!", isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				var control = NetworkGUITestCase.FindNetworkUserControl(form);

				// Headings on the main diagram area
				var entityDetails = control.MainDiagramControl.FindChildren<EntityDetails>().Single();
				var heading = entityDetails.FindChildren<TextBox>().Single(x => x.Text == "This is MY diagram!");
				AssertEquals(Visibility.Visible, heading.Visibility);

				var nonScheduledHeading = entityDetails.FindChildren<TextBox>().Single(x => x.Text == "Non-Scheduled Items");
				AssertEquals(Visibility.Hidden, nonScheduledHeading.Visibility);

				// Headings on the non-scheduled diagram area
				entityDetails = control.NonScheduledDiagramControl.FindChildren<EntityDetails>().Single();
				heading = entityDetails.FindChildren<TextBox>().Single(x => x.Text == "This is MY diagram!");
				AssertEquals(Visibility.Hidden, heading.Visibility);

				nonScheduledHeading = entityDetails.FindChildren<TextBox>().Single(x => x.Text == "Non-Scheduled Items");
				AssertEquals(Visibility.Visible, nonScheduledHeading.Visibility);
			}
		}

		public void TestNonScheduledSection_EntityDetails_ShouldBeReadOnly()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, "This is MY diagram!", isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				var control = NetworkGUITestCase.FindNetworkUserControl(form);

				AssertTextboxReadOnlyness(textBoxText => $"None of the textboxes in the scheduled section should be readonly, including the one with the text [{textBoxText}]. SAD!", control.MainDiagramControl, shouldTextboxesBeReadOnly: false);
				AssertTextboxReadOnlyness(textBoxText => $"All of the textboxes in the nonscheduled section should be readonly, including the one with the text [{textBoxText}]. SAD!", control.NonScheduledDiagramControl, shouldTextboxesBeReadOnly: true);

#pragma warning disable CS0618 // Type or member is obsolete
				void AssertTextboxReadOnlyness(Func<string, string> getMessageForControlText, DiagramAreaUserControl diagramControl, bool shouldTextboxesBeReadOnly)
				{
					var entityDetails = diagramControl.FindChildren<EntityDetails>().Single();
					var textBoxes = entityDetails.FindChildren<TextBox>().Where(x => x.Visibility == Visibility.Visible).ToArray();

					AssertEquals(3, textBoxes.Length);

					foreach (var textBox in textBoxes)
					{
						AssertEquals(getMessageForControlText(textBox.Text), shouldTextboxesBeReadOnly, textBox.IsReadOnly);
					}
				}
#pragma warning restore CS0618 // Type or member is obsolete
			}
		}

		public void TestDiagram_WithNonScheduledSection_ShouldHaveCorrectShapesInCorrectSections()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			var regularShape1 = NetworkTestCase.CreateShape(diagram, "Regular Shape 1");
			var regularShape2 = NetworkTestCase.CreateShape(diagram, "Regular Shape 2");
			regularShape1.IsNonScheduled = false;
			regularShape1.IsNonScheduled = false;
			regularShape1.MakeVisiblePrerequisiteOf(regularShape2, diagram);

			var nonscheduledShape1 = NetworkTestCase.CreateShape(diagram, "Nonscheduled Shape 1");
			var nonscheduledShape2 = NetworkTestCase.CreateShape(diagram, "Nonscheduled Shape 2");
			nonscheduledShape1.IsNonScheduled = true;
			nonscheduledShape2.IsNonScheduled = true;
			nonscheduledShape1.MakeVisiblePrerequisiteOf(nonscheduledShape2, diagram);

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram) { Size = ControlDpiScalingHelper.NewScaledSize(2000, 1200) })
			{
				form.Show();
				Application.DoEvents();

				var control = NetworkGUITestCase.FindNetworkUserControl(form);

				var mainAreaShapes = NetworkTestHelper.GetShapeControls(control.MainDiagramControl).ToArray();
				AssertContainsExactElementsInAnyOrder(new[] { "Regular Shape 1", "Regular Shape 2" }, mainAreaShapes.Select(GetShapeName));

				var nonScheduledShapes = NetworkTestHelper.GetShapeControls(control.NonScheduledDiagramControl).ToArray();
				AssertContainsExactElementsInAnyOrder(new[] { "Nonscheduled Shape 1", "Nonscheduled Shape 2" }, nonScheduledShapes.Select(GetShapeName));

				// Move the shapes around a little so that it's clearer to see visually how the arrows are aligned in case this test is failing.
				var shapeToMove = mainAreaShapes.Single(x => ((NodeViewModel)x.DataContext).Name == "Regular Shape 2");
				((NodeViewModel)shapeToMove.DataContext).Y = 100;

				shapeToMove = nonScheduledShapes.Single(x => ((NodeViewModel)x.DataContext).Name == "Nonscheduled Shape 1");
				((NodeViewModel)shapeToMove.DataContext).Y = 200;

				NetworkGUITestCase.DoEventsThoroughly();

				var mainAreaArrows = control.MainDiagramControl.FindChildren<CurvedArrow>();
				AssertContainsExactElementsInAnyOrder(new[] { "Regular Shape 1" }, mainAreaArrows.Select(GetArrowFromShape));

				var nonScheduledArrows = control.NonScheduledDiagramControl.FindChildren<CurvedArrow>();
				AssertContainsExactElementsInAnyOrder(new[] { "Nonscheduled Shape 1" }, nonScheduledArrows.Select(GetArrowFromShape));
			}

			NetworkGUITestCase.DoEventsThoroughly();
		}

		[TestDate(2019, 1, 18)]
		public void TestNonScheduledSection_ShouldNotHaveBackgroundColor_ForTimeInThePast()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;
			diagram.ScheduledStartTimeUtc = ZDateTime.Now.AddDays(-7);

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				var scheduledGrid = NetworkTestHelper.GetDynamicGrid(networkControl.MainDiagramControl);

				var color = Color.FromRgb(0xDD, 0xDD, 0xDD);
				var rectangles = GetColumnBackgrounds(scheduledGrid, color);
				AssertEquals(1, rectangles.Count());

				var nonScheduledGrid = NetworkTestHelper.GetDynamicGrid(networkControl.NonScheduledDiagramControl);
				rectangles = GetColumnBackgrounds(nonScheduledGrid, color);
				AssertEquals("The 'time in the past' rectangle should not be drawn on the nonscheduled section. SAD!", 0, rectangles.Count());
			}
		}

		public void TestNonScheduledSection_ShouldNotHaveBackgroundColor_ForAffinityViolations()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;
			var affinity = NetworkTestCase.CreateAffinity(diagram);

			var network = NetworkTestCase.CreateNetwork(diagram);
			Factory.Save();

			var scheduledShape1 = NetworkTestCase.CreateShape(diagram, "Scheduled Shape 1");
			var scheduledShape2 = NetworkTestCase.CreateShape(diagram, "Scheduled Shape 2");
			var nonScheduledShape1 = NetworkTestCase.CreateShape(diagram, "Non-Scheduled Shape 1");
			var nonScheduledShape2 = NetworkTestCase.CreateShape(diagram, "Non-Scheduled Shape 2");
			nonScheduledShape1.IsNonScheduled = true;
			nonScheduledShape2.IsNonScheduled = true;

			var scheduledEntity1 = scheduledShape1.AsEntity(network);
			var scheduledEntity2 = scheduledShape2.AsEntity(network);
			var nonScheduledEntity1 = nonScheduledShape1.AsEntity(network);
			var nonScheduledEntity2 = nonScheduledShape2.AsEntity(network);

			scheduledEntity1.Width = 300;
			scheduledEntity1.X = 100;
			scheduledEntity1.Y = 75;

			scheduledEntity2.Width = 300;
			scheduledEntity2.X = 200;
			scheduledEntity2.Y = 250;

			nonScheduledEntity1.Width = 300;
			nonScheduledEntity1.X = 500;
			nonScheduledEntity1.Y = 75;

			nonScheduledEntity2.Width = 300;
			nonScheduledEntity2.X = 600;
			nonScheduledEntity2.Y = 250;

			NetworkTestCase.LinkAffinity(network, scheduledShape1, affinity);
			NetworkTestCase.LinkAffinity(network, scheduledShape2, affinity);
			NetworkTestCase.LinkAffinity(network, nonScheduledShape1, affinity);
			NetworkTestCase.LinkAffinity(network, nonScheduledShape2, affinity);
			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				var scheduledGrid = NetworkTestHelper.GetDynamicGrid(networkControl.MainDiagramControl);

				var rectangles = GetColumnBackgrounds(scheduledGrid, Colors.Red);
				AssertEquals(1, rectangles.Count());

				var nonScheduledGrid = NetworkTestHelper.GetDynamicGrid(networkControl.NonScheduledDiagramControl);
				rectangles = GetColumnBackgrounds(nonScheduledGrid, Colors.Red);
				AssertEquals("Afinity violations should not be drawn on the nonscheduled section. SAD!", 0, rectangles.Count());
			}
		}

		public void TestNonScheduledSection_ShouldIgnoreLevelingRules()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			NetworkTestCase.CreateLevelingRule(diagram, LevelingRuleTypeList.Codes.MaximumConcurrentEntities, "Know your rules.", "DeepPink", 1);

			var shape1 = NetworkTestCase.CreateShape(diagram);
			shape1.BNS_Name = "Shape 1";
			shape1.IsNonScheduled = true;

			var shape2 = NetworkTestCase.CreateShape(diagram);
			shape2.BNS_Name = "Shape 2";
			shape2.IsNonScheduled = true;

			var network = NetworkTestCase.CreateNetwork(diagram);
			var entity1 = shape1.AsEntity(network);
			var entity2 = shape2.AsEntity(network);

			entity1.X = 0;
			entity1.Y = 100;
			entity1.Width = 200;
			entity1.Height = 100;

			entity2.X = 0;
			entity2.Y = 300;
			entity2.Width = 200;
			entity2.Height = 100;

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				var node1 = networkControl.FindNodeItemOrDefault("Shape 1");
				var node2 = networkControl.FindNodeItemOrDefault("Shape 2");

				CombineAssertions("Nonscheduled shapes should not have the leveling rule violation applied. SAD!", () =>
				{
					AssertNoWarningMessagesOnShape(node1);
					AssertNoWarningMessagesOnShape(node2);
				});

				var nonScheduledGrid = NetworkTestHelper.GetDynamicGrid(networkControl.NonScheduledDiagramControl);
				var rectangles = GetColumnBackgrounds(nonScheduledGrid, Colors.DeepPink);

				AssertEquals("Nonscheduled sections shouldn't have any special background colours due to leveling rule violations. SAD!", 0, rectangles.Count());
			}
		}

		public void TestNonScheduledSection_ShouldBeShownIfHidden_AfterEnablingTheOption()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = false;

			var shape1 = NetworkTestCase.CreateShape(diagram);
			shape1.BNS_Name = "Shape 1";
			shape1.IsNonScheduled = false;

			var shape2 = NetworkTestCase.CreateShape(diagram);
			shape2.BNS_Name = "Shape 2";
			shape2.IsNonScheduled = true;

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				var nonScheduledGrid = NetworkTestHelper.GetDynamicGrid(networkControl.NonScheduledDiagramControl);
				AssertNull(nonScheduledGrid);

				diagram.ShouldShowNonScheduledSection = true;
				form.Network.Refresher.Refresh(RefreshType.RedrawDiagram);
				Application.DoEvents();
				nonScheduledGrid = NetworkTestHelper.GetDynamicGrid(networkControl.NonScheduledDiagramControl);
				Assert(nonScheduledGrid.ActualWidth > 0);
			}
		}

		public void TestNonScheduledSection_ShouldBeHiddenIfShown_AfterDisablingTheOption()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			var shape1 = NetworkTestCase.CreateShape(diagram);
			shape1.BNS_Name = "Shape 1";
			shape1.IsNonScheduled = false;

			var shape2 = NetworkTestCase.CreateShape(diagram);
			shape2.BNS_Name = "Shape 2";
			shape2.IsNonScheduled = true;

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				var nonScheduledGrid = NetworkTestHelper.GetDynamicGrid(networkControl.NonScheduledDiagramControl);
				var scheduledGrid = NetworkTestHelper.GetDynamicGrid(networkControl.MainDiagramControl);
				var scheduledGridWidthBeforeHidingNonScheduled = scheduledGrid.ActualWidth;
				var nonScheduledGridWidth = nonScheduledGrid.ActualWidth;

				AssertNotNull(nonScheduledGrid);

				diagram.ShouldShowNonScheduledSection = false;
				form.Network.Refresher.Refresh(RefreshType.RedrawDiagram);
				Application.DoEvents();
				nonScheduledGrid = NetworkTestHelper.GetDynamicGrid(networkControl.NonScheduledDiagramControl);
				AssertNull(nonScheduledGrid);

				scheduledGrid = NetworkTestHelper.GetDynamicGrid(networkControl.MainDiagramControl);
				Assert(scheduledGrid.ActualWidth >= scheduledGridWidthBeforeHidingNonScheduled + nonScheduledGridWidth);
			}
		}

		static string GetShapeName(AdornedControl shapeControl)
		{
			return ((NodeViewModel)shapeControl.DataContext).Name;
		}

		static string GetArrowFromShape(CurvedArrow arrow)
		{
			return ((ConnectionViewModel)arrow.DataContext).SourceConnector.ParentNode.Name;
		}

		public void TestScrollScheduledSection_VerticalScroll_ShouldScrollNonScheduledSection()
		{
			AssertScrollOneSection_MaybeAffectsTheOther(shouldManuallyScrollScheduledSection: true, shouldScrollHorizontally: false);
		}

		public void TestScrollScheduledSection_HorizontalScroll_ShouldNotScrollNonScheduledSection()
		{
			AssertScrollOneSection_MaybeAffectsTheOther(shouldManuallyScrollScheduledSection: true, shouldScrollHorizontally: true);
		}

		public void TestScrollNonScheduledSection_VerticalScroll_ShouldScrollScheduledSection()
		{
			AssertScrollOneSection_MaybeAffectsTheOther(shouldManuallyScrollScheduledSection: false, shouldScrollHorizontally: false);
		}

		public void TestScrollNonScheduledSection_HorizontalScroll_ShouldNotScrollNonScheduledSection()
		{
			AssertScrollOneSection_MaybeAffectsTheOther(shouldManuallyScrollScheduledSection: false, shouldScrollHorizontally: true);
		}

		void AssertScrollOneSection_MaybeAffectsTheOther(bool shouldManuallyScrollScheduledSection, bool shouldScrollHorizontally)
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;
			var network = NetworkTestCase.CreateNetwork(diagram);

			var scheduledShape = NetworkTestCase.CreateShape(diagram);
			scheduledShape.AsEntity(network).X = 1000;
			scheduledShape.AsEntity(network).Y = 1000;

			var nonScheduledShape = NetworkTestCase.CreateShape(diagram);
			nonScheduledShape.IsNonScheduled = true;
			nonScheduledShape.AsEntity(network).Y = 1000;
			nonScheduledShape.AsEntity(network).X = 1000;

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				var scheduledScroll = networkControl.MainDiagramControl.FindChildren<ScrollViewer>().Single(x => x.Name == "ScrollViewer");
				var nonScheduledScroll = networkControl.NonScheduledDiagramControl.FindChildren<ScrollViewer>().Single(x => x.Name == "ScrollViewer");

				AssertEquals(0d, scheduledScroll.HorizontalOffset);
				AssertEquals(0d, scheduledScroll.VerticalOffset);
				AssertEquals(0d, nonScheduledScroll.HorizontalOffset);
				AssertEquals(0d, nonScheduledScroll.VerticalOffset);

				var controlToManuallyScroll = shouldManuallyScrollScheduledSection ? scheduledScroll : nonScheduledScroll;
				var otherScrollControl = controlToManuallyScroll == scheduledScroll ? nonScheduledScroll : scheduledScroll;

				if (shouldScrollHorizontally)
				{
					controlToManuallyScroll.ScrollToHorizontalOffset(500d);
					NetworkGUITestCase.DoEventsThoroughly();

					AssertEquals(500d, controlToManuallyScroll.HorizontalOffset);
					AssertEquals("The sections should horizontally scroll independently. SAD!", 0d, otherScrollControl.HorizontalOffset);
				}
				else
				{
					controlToManuallyScroll.ScrollToVerticalOffset(500d);
					NetworkGUITestCase.DoEventsThoroughly();

					AssertEquals(500d, controlToManuallyScroll.VerticalOffset);
					AssertEquals("The sections should vertically scroll together. SAD!", 500d, otherScrollControl.VerticalOffset);
				}
			}
		}

		public void TestOpenForm_WhenShowingNonScheduledSection_DiagramHasChanges_ShouldBeFalse()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;
			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				var entity = form.BusinessEntity;
				AssertEquals("When the form causes the slider to be well-adjusted, it should not set HasChanges to true. SAD!", false, entity.HasChanges);
			}
		}

		public void TestSectionSplitter_OnRefresh_ShouldRememberWhereItWasPlaced()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram) { Size = ControlDpiScalingHelper.NewScaledSize(1600, 1000) })
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				AssertEquals("The non-scheduled section should be 600 in width by default. SAD!", 600d, networkControl.NonScheduledDiagramControl.ActualWidth);

				networkControl.SetNonScheduledSectionWidth(1000d);
				NetworkGUITestCase.DoEventsThoroughly();
				AssertEquals(1000d, networkControl.NonScheduledDiagramControl.ActualWidth);

				networkControl.NetworkViewModel.Network.Refresh(RefreshType.RedrawDiagram);
				NetworkGUITestCase.DoEventsThoroughly();

				AssertEquals("Refreshing the diagram should not have tried moving the slider. SAD!", 1000d, networkControl.NonScheduledDiagramControl.ActualWidth);
			}
		}

		public void TestSectionSplitter_OnSaveAndReopen_ShouldRememberWhereItWasPlaced()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram) { Size = ControlDpiScalingHelper.NewScaledSize(1600, 1000) })
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				AssertEquals("The non-scheduled section should be 600 in width by default. SAD!", 600d, networkControl.NonScheduledDiagramControl.ActualWidth);

				networkControl.SetNonScheduledSectionWidth(1000d);
				NetworkGUITestCase.DoEventsThoroughly();
				AssertEquals(1000d, networkControl.NonScheduledDiagramControl.ActualWidth);

				form.FireSaveButton();
				form.Close();
			}

			using (var form = new NetworkDiagramForm(diagram) { Size = ControlDpiScalingHelper.NewScaledSize(1600, 1000) })
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				AssertEquals("The non-scheduled section should be the same size that the user left it upon last save. SAD!", 1000d, networkControl.NonScheduledDiagramControl.ActualWidth);
			}
		}

		public void TestSectionControls_MinAndMaxWidth_WithoutNonScheduledSection()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				AssertMinAndMaxWidthsForSectionControls(form, 0d, double.PositiveInfinity, 0d, double.PositiveInfinity);
			}
		}

		public void TestSectionControls_MinAndMaxWidth_WithNonScheduledSection()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;
			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				AssertMinAndMaxWidthsForSectionControls(form, 0d, 746d, 500d, 846d);
			}
		}

		public void TestSectionControls_MinAndMaxWidth_WhenShowingNonScheduledSection()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
				{
					var propertiesForm = (ZForm)dialog;
					propertiesForm.Shown += (sender, args) =>
					{
						var entity = (ShapeNetworkEntity)propertiesForm.BusinessEntity;
						entity.ShouldShowNonScheduledSection = true;
					};
				});

				form.Network.EditEntity(form.Network.DiagramEntity);
				NetworkGUITestCase.DoEventsThoroughly();

				AssertMinAndMaxWidthsForSectionControls(form, 0d, 746d, 500d, 846d);
			}
		}

		public void TestSectionControls_MinAndMaxWidth_WhenHidingNonScheduledSection()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;
			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
				{
					var propertiesForm = (ZForm)dialog;
					propertiesForm.Shown += (sender, args) =>
					{
						var entity = (ShapeNetworkEntity)propertiesForm.BusinessEntity;
						entity.ShouldShowNonScheduledSection = false;
					};
				});

				form.Network.EditEntity(form.Network.DiagramEntity);
				NetworkGUITestCase.DoEventsThoroughly();

				AssertMinAndMaxWidthsForSectionControls(form, 0d, double.PositiveInfinity, 0d, 846d); // it's okay to leave the MaxWidth of the non-scheduled section since it's hidden anyway.
			}
		}

		public void TestSectionControls_MinAndMaxWidth_WhenResizingTheWindow()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;
			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram) { Size = ControlDpiScalingHelper.NewScaledSize(1000, 800) })
			{
				form.Show();
				Application.DoEvents();

				AssertMinAndMaxWidthsForSectionControls(form, 0d, 526d, 500d, 626d);

				form.Size = ControlDpiScalingHelper.NewScaledSize(1200, 1000);
				NetworkGUITestCase.DoEventsThoroughly();

				AssertMinAndMaxWidthsForSectionControls(form, 0d, 726d, 500d, 826d);
			}
		}

		static void AssertMinAndMaxWidthsForSectionControls(NetworkDiagramForm form, double expectedMinScheduledWidth, double expectedMaxScheduledWidth, double expectedMinNonScheduledWidth, double expectedMaxNonScheduledWidth)
		{
			var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
			var networkControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
			var grid = networkControl.FindChildren<Grid>().Single(x => x.Name == "DiagramAreaControlsGrid");

			var scheduledColumn = grid.ColumnDefinitions[0];
			var nonScheduledColumn = grid.ColumnDefinitions[2];

			CombineAssertions("The Min and Max Width of the grid columns is set very carefully in order to prevent strange behaviors when resizing the form or adjusting the slider. If you change these values, you MUST functionally test thoroughly to ensure that it still behaves correctly.", () =>
			{
				AssertEquals("Scheduled Minimum Width", expectedMinScheduledWidth, scheduledColumn.MinWidth);
				AssertEquals("Scheduled Maximum Width", expectedMaxScheduledWidth, scheduledColumn.MaxWidth);
				AssertEquals("Non-Scheduled Minimum Width", expectedMinNonScheduledWidth, nonScheduledColumn.MinWidth);
				AssertEquals("Non-Scheduled Maximum Width", expectedMaxNonScheduledWidth, nonScheduledColumn.MaxWidth);
			});
		}

		public void TestRibbon_ShouldWorkForSelectedEntitiesInNonScheduledSection()
		{
			BMSRegistry.Instance.NCNRibbonEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			var shape = NetworkTestCase.CreateShape(diagram, "Squanch");
			shape.IsNonScheduled = true;

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				var node = networkControl.FindNodeItem("Squanch");
				node.IsSelected = true;
				Application.DoEvents();

				var ribbon = networkControl.RibbonControlExposed_ForTesting;
				var button = ribbon.Model.ButtonViewModels.Single(x => x.Action.GetName() == "Shape"); // Create shape action
				networkControl.ExecuteRibbonAction(button.Action);
				Application.DoEvents();

				var newShape = shape.ChildShapes.SingleOrDefault();
				AssertNotNull("The new shape should have been added as a child shape to the selected shape. If that didn't happen, it might mean that the ribbon action didn't know that a shape was selected. SAD!", newShape);
				AssertEquals("The new shape should be non-scheduled since its parent is non-scheduled. SAD!.", true, newShape.IsNonScheduled);

				var shapeThatShouldntExist = diagram.ChildShapes.SingleOrDefault(x => x != shape);
				AssertNull("No other shapes should have been created. SAD!", shapeThatShouldntExist);
			}
		}

		public void TestRibbon_ApproveNonApprovedShapesAndArrows_ShouldDisableOnceExecuted()
		{
			BMSRegistry.Instance.NCNRibbonEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var network = NetworkTestCase.CreateNetwork(diagram);

			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = VisualBoardsTestCase.CreateWorkflow(jobHeader, "A workflow");
			var workflow2 = VisualBoardsTestCase.CreateWorkflow(jobHeader, "Another workflow");
			workflow1.GetOrCreateDependencyLink(workflow2);

			var shape1 = NetworkTestCase.CreateShape(workflow1, diagram, "Squanch");
			var shape2 = NetworkTestCase.CreateShape(workflow2, diagram, "Squinch");

			network.CreateRelationship(shape1, shape2);
			network.DiagramEntity.Approve(GlbStaff.CurrentUser.GS_Code);

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				var ribbon = networkControl.RibbonControlExposed_ForTesting;
				ribbon.SelectTab(3);
				var approveNonApprovedButton = ribbon.Model.ButtonViewModels.Single(x => x.Action.GetName() == "Approve Non-approved Shapes and Arrows");
				var approveButton = ribbon.Model.ButtonViewModels.Single(x => x.Action.GetName() == "Approve Diagram");
				var unapproveButton = ribbon.Model.ButtonViewModels.Single(x => x.Action.GetName() == "Un-approve Diagram");

				CombineAssertions("Before execution", () =>
				{
					AssertEquals("Should be able to approve non approved arrow", true, approveNonApprovedButton.IsEnabled);
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Should already be approved", false, approveButton.IsEnabled);
					AssertEquals("Should be able to be un-approved", true, unapproveButton.IsEnabled);
				});

				networkControl.ExecuteRibbonAction(approveNonApprovedButton.Action);
				Application.DoEvents();

				CombineAssertions("After execution", () =>
				{
					AssertEquals("Should NOT be able to approve the now approved arrow.", false, approveNonApprovedButton.IsEnabled);
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Should already be approved", false, approveButton.IsEnabled);
					AssertEquals("Should be able to be un-approved", true, unapproveButton.IsEnabled);
				});
			}
		}

		public void TestOpenAsDiagram_ForNonScheduledShape_ShouldShowChildShapes()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			var shape = NetworkTestCase.CreateShape(diagram);
			shape.IsNonScheduled = true;

			var childShape = NetworkTestCase.CreateShape(shape, "I hope my visibility isn't too low!");

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				var networkViewModel = networkControl.NetworkViewModel;

				var action = new OpenAsDiagramAction(networkViewModel);
				NetworkDiagramForm otherDiagramForm = null;

				form.FireSaveButton();
				Application.DoEvents();

				try
				{
					action.ExecuteForEntityWithoutAccessCheck(shape.AsEntity((IJobNetwork)networkViewModel.Network));
					NetworkGUITestCase.DoEventsThoroughly();

					otherDiagramForm = BMSFormTestHelper.GetOpenForms<NetworkDiagramForm>().Single(x => x != form);
					var otherElementHost = otherDiagramForm.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
					var otherNetworkControl = (NetworkUserControl)otherElementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

					var shapesOnNewForm = NetworkTestHelper.GetShapeControls(otherNetworkControl.MainDiagramControl).Select(x => x.GetViewModel().Name);
					AssertContainsExactElementsInAnyOrder("The child shape should be shown when opening a non-scheduled shape as a diagram. SAD!", new[] { "I hope my visibility isn't too low!" }, shapesOnNewForm);
				}
				finally
				{
					otherDiagramForm?.Dispose();
					Application.DoEvents();
				}
			}
		}

		public void TestOpenAsDiagram_ForNonScheduledShape_NewDiagramSurfaceShouldMatchSizeOfOriginalShape()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			var shape = NetworkTestCase.CreateShape(diagram);
			shape.IsNonScheduled = true;

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				var networkViewModel = networkControl.NetworkViewModel;

				var action = new OpenAsDiagramAction(networkViewModel);
				NetworkDiagramForm otherDiagramForm = null;

				form.FireSaveButton();
				Application.DoEvents();

				try
				{
					action.ExecuteForEntityWithoutAccessCheck(shape.AsEntity((IJobNetwork)networkViewModel.Network));
					NetworkGUITestCase.DoEventsThoroughly();

					otherDiagramForm = BMSFormTestHelper.GetOpenForms<NetworkDiagramForm>().Single(x => x != form);
					var otherElementHost = otherDiagramForm.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
					var otherNetworkControl = (NetworkUserControl)otherElementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

					var diagramWidth = otherNetworkControl.MainDiagramControl.ViewModel.ContentWidth;
					AssertEquals("When you open a non-scheduled shape as a diagram, the opened diagram should be the same width as the shape, not some giant monstrosity. SAD!", 275d, diagramWidth);
				}
				finally
				{
					otherDiagramForm?.Dispose();
					Application.DoEvents();
				}
			}
		}

		public void TestSetWidth_ForNonScheduledShape_ShouldNotAdjustForScale()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				var action = new CreateShapeAction(networkControl.NetworkViewModel);
				networkControl.NonScheduledDiagramControl.ExecuteContextMenuAction(action);
				Application.DoEvents();

				var shapeControl = NetworkTestHelper.GetShapeControls(networkControl.NonScheduledDiagramControl).Single();
				var shape = shapeControl.GetViewModel().Entity.AsShape();
				AssertEquals("The shape width should just be set to what we told it to set, not adjusted for scale, because this is a non-scheduled shape. SAD!", 270m, shape.Width);
			}
		}

		#endregion

		#region Validation on Open

		public void TestShouldNotRunLoopValidationOnOpening_WhenNotSpecifiedInShape()
		{
			var network = GetNetworkWithCircularDependency();
			var diagramShape = network.DiagramShape;
			var diagramEntity = network.DiagramEntity;

			diagramShape.ShouldValidateLoopsOnOpen = false;
			ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowFormForNewEntity(diagramShape);
			using (var form = Application.OpenForms.OfType<NetworkDiagramForm>().SingleOrDefault())
			{
				AssertNotNull(form);
				Application.DoEvents();

				var shownDiagramEntity = form.NetworkDiagramControl.Network.DiagramEntity;
				AssertEquals("Should not have validation errors as the shape did not specify the need of running validation on opening", false, shownDiagramEntity.HasErrors);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("Should not show message boxes", ZFormModaliser.LastFormShownDialogForTest);
			}

			CombineAssertions("Ensure that the network would have validation errors if loop validation ran", () =>
			{
				Assert(!network.ValidateLoopsAndCheckThereAreNoErrors());
				Assert("Diagram entity should have errors", diagramEntity.HasErrors);
				Assert("Diagram entity should have errors related to circular dependencies", diagramEntity.GetErrors().Any(e => e.Message.Contains("A looped dependency presents on the diagram")));
			});
		}

		public void TestShouldRunLoopValidationOnOpening_WhenSpecifiedInShape()
		{
			var network = GetNetworkWithCircularDependency();
			var diagramShape = network.DiagramShape;

			diagramShape.ShouldValidateLoopsOnOpen = true;
			ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowFormForNewEntity(diagramShape);
			using (var form = Application.OpenForms.OfType<NetworkDiagramForm>().SingleOrDefault())
			{
				AssertNotNull(form);
				Application.DoEvents();

				var shownDiagramEntity = form.NetworkDiagramControl.Network.DiagramEntity;
				AssertEquals("Should have validation errors as the shape specified the need of running validation on opening", true, shownDiagramEntity.HasErrors);
				Assert("Should have circular dependency errors", shownDiagramEntity.GetErrors().Any(e => e.Message.Contains("A looped dependency presents on the diagram")));
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("Should not show message boxes", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		IJobNetwork GetNetworkWithCircularDependency()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "workflow1";
			workflow2.FH_CompletionStatement = "workflow2";
			workflow3.FH_CompletionStatement = "workflow3";

			var link1to2 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link2to3 = workflow2.GetOrCreateDependencyLink(workflow3);
			var link3to1 = workflow3.GetOrCreateDependencyLink(workflow1); //Circular dependency - should anger validation

			Factory.Save();

			var diagramShape = NetworkTestCase.CreateDiagram(jobHeader);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagramShape);

			NetworkTestCase.CreateShape(workflow1, diagramShape);
			NetworkTestCase.CreateShape(workflow2, diagramShape);
			NetworkTestCase.CreateShape(workflow3, diagramShape);

			return networkViewModel.GetJobNetwork();
		}

		#endregion

		#region Validation on Save

		public void TestShouldValidateNetworkDiagramOnSave()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape = NetworkTestCase.CreateShape(diagram);

			diagram.Name = "";
			shape.Name = "";

			using (ActiveBusinessObjectCollection.TrackIndexedCollectionCounts_ForTest())
			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				var diagramEntity = (ShapeNetworkEntity)form.Network.Entities.First();
				AssertNotNull(diagramEntity);

				var saveResult = form.FireSaveButton();

				AssertHasError("Should have validated on save", diagram.RootShape.BNS_NameInfo, "Please enter a Name.");
				AssertEquals("Saving should failed", ContinueWithSave.No, saveResult);
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();

				diagram.Name = "Not done yet";

				saveResult = form.FireSaveButton();
				AssertHasError("Should have validated on save", diagramEntity.Shape.BNS_NameInfo, "Please enter a Name.");
				AssertEquals("Saving should failed", ContinueWithSave.No, saveResult);
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();

				shape.Name = "Now we are done";

				saveResult = form.FireSaveButton();
				AssertNoErrors("Should have validated on save, and be good!", diagram.RootShape.BNS_NameInfo);
				AssertNoErrors("Should have validated on save, and be good!", diagramEntity.Shape.BNS_NameInfo);
				AssertEquals("Saving now be goodly", ContinueWithSave.Yes, saveResult);
				AssertEquals("We now have no more errors :)", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestShouldNotifyEachShapeAndShapeNetworkEntityOncePerValidation()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape1 = NetworkTestCase.CreateShape(diagram);
			var shape2 = NetworkTestCase.CreateShape(diagram);

			var shape1_1 = NetworkTestCase.CreateShape(shape1);
			var shape2_1 = NetworkTestCase.CreateShape(shape2);

			diagram.Name = "Diagram";
			shape1.Name = "Shape 1";
			shape2.Name = "Shape 2";
			shape1_1.Name = "Nested shape 1";
			shape2_1.Name = "Nested shape 2";

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				form.Network.CreateRelationship(shape1, shape2); // let's create some attachment just in case

				var actualEntityNotifications = new List<(string entityName, string changedPropertyName)>();
				var actualShapeNotifications = new List<(string shapeName, string changedPropertyName)>();

				void DiagramEntity_PropertyChanged(object sender, PropertyChangedEventArgs e)
				{
					var entity = (ShapeNetworkEntity)sender;
					actualEntityNotifications.Add((entity.Name, e.PropertyName));
				}

				void Shape_PropertyChanged(object sender, PropertyChangedEventArgs e)
				{
					var shape = (BMNCNShape)sender;
					actualShapeNotifications.Add((shape.Name, e.PropertyName));
				}

				foreach (var entity in form.Network.Entities.Cast<ShapeNetworkEntity>())
				{
					entity.PropertyChanged += DiagramEntity_PropertyChanged;
					entity.Shape.PropertyChanged += Shape_PropertyChanged;
				}
				form.Network.DiagramEntity.PropertyChanged += DiagramEntity_PropertyChanged;
				diagram.RootShape.PropertyChanged += Shape_PropertyChanged;

				form.FireSaveButton();

				var expectedEntityNotifications = new List<(string entityName, string changedPropertyName)>() {
					("Diagram", "HasNotifications"),
					("Diagram", "EntityState"),
					("Shape 1", "HasNotifications"),
					("Shape 1", "EntityState"),
					("Shape 2", "HasNotifications"),
					("Shape 2", "EntityState"),
					("Nested shape 1", "HasNotifications"),
					("Nested shape 1", "EntityState"),
					("Nested shape 2", "HasNotifications"),
					("Nested shape 2", "EntityState"),
				};
				var expectedShapeNotifications = new List<(string shapeName, string changedPropertyName)>() {
					("Diagram", "HasNotifications"),
					("Shape 1", "HasNotifications"),
					("Shape 2", "HasNotifications"),
					("Nested shape 1", "HasNotifications"),
					("Nested shape 2", "HasNotifications"),
				};

				AssertContainsExactElementsInAnyOrder(expectedEntityNotifications, actualEntityNotifications);
				AssertContainsExactElementsInAnyOrder(expectedShapeNotifications, actualShapeNotifications);

				shape1.BNS_Status = "SUS";

				actualEntityNotifications = new List<(string entityName, string changedPropertyName)>();
				actualShapeNotifications = new List<(string shapeName, string changedPropertyName)>();

				form.FireSaveButton();

				AssertContainsExactElementsInAnyOrder(expectedEntityNotifications, actualEntityNotifications);
				AssertContainsExactElementsInAnyOrder(expectedShapeNotifications, actualShapeNotifications);
			}
		}

		public void TestShouldNotifyEachShapeAndShapeNetworkEntityOncePerValidation_ScaledDiagram()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape1 = NetworkTestCase.CreateShape(diagram);
			var shape2 = NetworkTestCase.CreateShape(diagram);

			var shape1_1 = NetworkTestCase.CreateShape(shape1);
			var shape2_1 = NetworkTestCase.CreateShape(shape2);

			diagram.Name = "Diagram";
			shape1.Name = "Shape 1";
			shape2.Name = "Shape 2";
			shape1_1.Name = "Nested shape 1";
			shape2_1.Name = "Nested shape 2";

			diagram.SwitchToScaled();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				form.Network.CreateRelationship(shape1, shape2); // let's create some attachment just in case

				var actualEntityNotifications = new List<(string entityName, string changedPropertyName)>();
				var actualShapeNotifications = new List<(string shapeName, string changedPropertyName)>();

				void DiagramEntity_PropertyChanged(object sender, PropertyChangedEventArgs e)
				{
					var entity = (ShapeNetworkEntity)sender;
					actualEntityNotifications.Add((entity.Name, e.PropertyName));
				}

				void Shape_PropertyChanged(object sender, PropertyChangedEventArgs e)
				{
					var shape = (BMNCNShape)sender;
					actualShapeNotifications.Add((shape.Name, e.PropertyName));
				}

				foreach (var entity in form.Network.Entities.Cast<ShapeNetworkEntity>())
				{
					entity.PropertyChanged += DiagramEntity_PropertyChanged;
					entity.Shape.PropertyChanged += Shape_PropertyChanged;
				}
				form.Network.DiagramEntity.PropertyChanged += DiagramEntity_PropertyChanged;
				diagram.RootShape.PropertyChanged += Shape_PropertyChanged;

				form.FireSaveButton();

				AssertContainsExactElementsInAnyOrder(new List<(string entityName, string changedPropertyName)>() {
					("Diagram", "HasNotifications"),
					("Diagram", "EntityState"),
					("Shape 1", "HasNotifications"),
					("Shape 1", "EntityState"),
					("Shape 1", "StartDateReadableText"),
					("Shape 1", "FinishDateReadableText"),
					("Shape 2", "HasNotifications"),
					("Shape 2", "EntityState"),
					("Shape 2", "StartDateReadableText"),
					("Shape 2", "FinishDateReadableText"),
					("Nested shape 1", "HasNotifications"),
					("Nested shape 1", "EntityState"),
					("Nested shape 1", "StartDateReadableText"),
					("Nested shape 1", "FinishDateReadableText"),
					("Nested shape 2", "HasNotifications"),
					("Nested shape 2", "EntityState"),
					("Nested shape 2", "StartDateReadableText"),
					("Nested shape 2", "FinishDateReadableText"),
				}, actualEntityNotifications);
				AssertContainsExactElementsInAnyOrder(new List<(string shapeName, string changedPropertyName)>() {
					("Diagram", "HasNotifications"),
					("Shape 1", "HasNotifications"),
					("Shape 2", "HasNotifications"),
					("Nested shape 1", "HasNotifications"),
					("Nested shape 2", "HasNotifications"),
				}, actualShapeNotifications);

				shape1.BNS_Status = "SUS";

				actualEntityNotifications = new List<(string entityName, string changedPropertyName)>();
				actualShapeNotifications = new List<(string shapeName, string changedPropertyName)>();

				form.FireSaveButton();

				AssertContainsExactElementsInAnyOrder(new List<(string entityName, string changedPropertyName)>() {
					("Diagram", "HasNotifications"),
					("Diagram", "EntityState"),
					("Shape 1", "HasNotifications"),
					("Shape 1", "EntityState"),
					("Shape 2", "HasNotifications"),
					("Shape 2", "EntityState"),
					("Nested shape 1", "HasNotifications"),
					("Nested shape 1", "EntityState"),
					("Nested shape 2", "HasNotifications"),
					("Nested shape 2", "EntityState"),
				}, actualEntityNotifications);
				AssertContainsExactElementsInAnyOrder(new List<(string shapeName, string changedPropertyName)>() {
					("Diagram", "HasNotifications"),
					("Shape 1", "HasNotifications"),
					("Shape 2", "HasNotifications"),
					("Nested shape 1", "HasNotifications"),
					("Nested shape 2", "HasNotifications"),
				}, actualShapeNotifications);
			}
		}

		#endregion

		#region Search

		public void TestShouldResetSearchResultsAfterRefresherEvent()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var shape = networkViewModel.CreateNewShape(diagram);
			shape.Name = "Shape";

			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagram))
			{
				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkUserControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				Application.DoEvents();
				var formNetworkViewModel = networkUserControl.NetworkViewModel;
				var node = formNetworkViewModel.Nodes.Single(n => n.Name == "Shape");

				var searchForm = networkUserControl.OpenFinderForm();
				searchForm.SetSearchBox_ForTest("Shape");
				searchForm.Search_PerformClick_ForTest();
				AssertEquals(true, node.IsSelected);

				var entitiesForResfreshEvent = new ShapeNetworkEntity[] { (ShapeNetworkEntity)node.Entity };

				foreach (var refreshEvent in ((RefreshType[])Enum.GetValues(typeof(RefreshType))).Except(new RefreshType[] { RefreshType.None }))
				{
					searchForm.SetSearchBox_ForTest("Shape");
					searchForm.Search_PerformClick_ForTest();
					AssertEquals(true, node.IsSelected);
					AssertEquals(true, searchForm.HasPerformedSearch_ForTest);

					formNetworkViewModel.GetJobNetwork().Refresh(refreshEvent, entitiesForResfreshEvent);
					AssertEquals("Should reset search results", false, searchForm.HasPerformedSearch_ForTest);
				}
			}
		}

		public void TestShouldBeAbleToSearchAfterAddingNewShape()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var shape1 = networkViewModel.CreateNewShape(diagram);
			shape1.Name = "Shape1";

			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagram))
			{
				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkUserControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				Application.DoEvents();
				var formNetworkViewModel = networkUserControl.NetworkViewModel;
				var node1 = formNetworkViewModel.Nodes.Single(n => n.Name == "Shape1");

				var searchForm = networkUserControl.OpenFinderForm();
				searchForm.SetSearchBox_ForTest("Shape");
				searchForm.Search_PerformClick_ForTest();
				AssertEquals(true, node1.IsSelected);

				var shape2 = formNetworkViewModel.CreateNewShape(diagram);
				shape2.Name = "Shape2";
				Application.DoEvents();

				AssertEquals(networkUserControl.NetworkViewModel, formNetworkViewModel);
				AssertEquals(2, formNetworkViewModel.Nodes.Count());
				var node2 = formNetworkViewModel.Nodes.Single(n => n.Name == "Shape2");
				AssertEquals(true, node1.IsSelected);
				AssertEquals(false, node2.IsSelected);

				searchForm.Search_PerformClick_ForTest();
				AssertEquals(false, node1.IsSelected);
				AssertEquals(true, node2.IsSelected);
			}
		}

		#endregion

		#region Network View Model Integration Tests

		public void TestHasChanges_Top()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			diagram.ScrollPosition = ScrollPositionList.Codes.Default;
			network.SwitchToScaled();
			network.DiagramShape.Factory.Save();

			var shape1 = networkViewModel.CreateNewShape(network.DiagramShape);
			shape1.Shape.BNS_Name = "Shape1";

			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagram))
			{
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				var networkControlViewModel = (NetworkUserControlViewModel)networkDiagram.DataContext;

				var node = networkDiagram.FindChildren<AdornedControl>().Single(x => x.DataContext is NodeViewModel && ((NodeViewModel)x.DataContext).Name == "Shape1");
				var shapeViewModel = node.DataContext as NodeViewModel;
				var shape = shapeViewModel.Entity as ShapeNetworkEntity;

				shapeViewModel.X = 1;
				shapeViewModel.Y = 1;

				Application.DoEvents();

				AssertEquals(true, form.BusinessEntity.HasChanges);

				AssertEquals(ContinueWithSave.Yes, form.FireSaveButton());
				Application.DoEvents();

				AssertEquals(false, form.BusinessEntity.HasChanges);
			}
		}

		public void TestResizingShapeShouldNotLoseTheEditedName()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			network.DiagramShape.Factory.Save();

			var shapeName = "Shape1";
			var newText = "new text";

			var shape1 = networkViewModel.CreateNewShape(network.DiagramShape);
			shape1.Shape.BNS_Name = shapeName;

			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagram))
			{
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				var node = networkDiagram.FindChildren<AdornedControl>().Single(x => x.DataContext is NodeViewModel && ((NodeViewModel)x.DataContext).Name == shapeName);
				var shapeViewModel = node.DataContext as NodeViewModel;
				var shape = shapeViewModel.Entity as ShapeNetworkEntity;

				var textBoxes = node.FindChildren<TextBox>().ToArray();
				var textBox = textBoxes.Where(x => x.Text == shape1.Name).Single();

				Keyboard.Focus(textBox);
				textBox.Text = newText;

				AssertEquals(true, textBox.IsKeyboardFocused);

				Application.DoEvents();

				AssertEquals(shapeName, shape1.Name);
				AssertEquals(newText, textBox.Text);

				var resizeThumb = networkDiagram.FindChildren<ResizeThumb>().Single(t => t.HorizontalAlignment == System.Windows.HorizontalAlignment.Right && t.VerticalAlignment == VerticalAlignment.Bottom);
				resizeThumb.RaiseEvent(new DragDeltaEventArgs(1, 1));

				shape.Width++;

				AssertEquals(newText, textBox.Text);
			}
		}

		public void TestHasChanges_WithTriggerableAsync()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			diagram.ScrollPosition = ScrollPositionList.Codes.Default;
			network.SwitchToScaled();
			Factory.Save();

			var shape1 = networkViewModel.CreateNewShape(network.DiagramShape);
			shape1.Name = "Shape1";

			AssertCollectionContains(shape1.Shape, diagram.ChildShapes);
			AssertCollectionContains(shape1.Shape, network.Shapes);
			Factory.Save();
			var triggerable = new TriggerableAsyncStrategy();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(triggerable))
			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagram))
			{
				triggerable.DoAllActions();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				var networkControlViewModel = (NetworkUserControlViewModel)networkDiagram.DataContext;

				var node = networkDiagram.FindChildren<AdornedControl>().Single(x => x.DataContext is NodeViewModel && ((NodeViewModel)x.DataContext).Name == "Shape1");
				var shapeViewModel = node.DataContext as NodeViewModel;
				var shape = shapeViewModel.Entity as ShapeNetworkEntity;

				shapeViewModel.X = 1;
				shapeViewModel.Y = 1;

				triggerable.DoAllActions();

				AssertEquals(true, form.BusinessEntity.HasChanges);

				AssertEquals(ContinueWithSave.Yes, form.FireSaveButton());

				triggerable.DoAllActions();

				AssertEquals(false, shape.Shape.ScheduleBizo.HasChanges);
				AssertEquals(false, form.BusinessEntity.HasChanges);
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestSetSchedule_BindingRefreshes()
		{
			var jobHeader1 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var diagram = NetworkTestCase.CreateDiagram(jobHeader1);
			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow;
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			Factory.Save();

			var shape1 = networkViewModel.CreateNewShape(network.DiagramShape);
			shape1.Name = "Shape1";

			var shapeAffinity1 = new ShapeAffinity("Red", "RedAffinity", ZGuid.NewZGuid());
			diagram.ShapeAffinities.Add(shapeAffinity1);

			Factory.Save();

			using (var form = new NetworkDiagramForm(Factory.CreateNewFactory().Load<BMNCNShape>(diagram.PK)))
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				var networkControlViewModel = (NetworkUserControlViewModel)networkDiagram.DataContext;

				Application.DoEvents();

				var node = networkDiagram.FindChildren<AdornedControl>().Single(x => x.DataContext is NodeViewModel);
				var viewModel = node.DataContext as NodeViewModel;
				var shape = viewModel.Entity as ShapeNetworkEntity;

				AssertTextBox(node, "Planned Duration", "21:36");
				AssertTextBox(node, "Scheduled Start", "14 Jul 2015 10:00");
				AssertTextBox(node, "Scheduled Finish", "15 Jul 2015 07:36");

				shape.X += 100;
				shape.Width += 100;

				Application.DoEvents();

				node = networkDiagram.FindChildren<AdornedControl>().Single(x => x.DataContext is NodeViewModel);

				AssertTextBox(node, "Planned Duration", "21:36");
				AssertTextBox(node, "Scheduled Start", "14 Jul 2015 18:00");
				AssertTextBox(node, "Scheduled Finish", "15 Jul 2015 23:36");
			}
		}

		static void AssertTextBox(AdornedControl node, string tooltip, string text)
		{
			var label = node.FindChildren<System.Windows.Controls.Label>().SingleOrDefault(l => l.ToolTip?.ToString() == tooltip);

			AssertNotNull("Should find a label with tooltip " + tooltip, label);
			AssertEquals(text, label.Content.ToString());
		}

		public void TestCreateAnnotationDoesntCreateWrongShape()
		{
			var jobHeader1 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var diagram = NetworkTestCase.CreateDiagram(jobHeader1);
			var network = NetworkTestCase.CreateNetwork(diagram);

			Factory.Save();

			using (var form = new NetworkDiagramForm(Factory.CreateNewFactory().Load<BMNCNShape>(diagram.PK)))
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				var createAnnotationMenuItem = networkDiagram.ViewModel.MenuItems.WhereNotNull().Single(m => m.Name == "Create New...").Items.WhereNotNull().Single(m => m.Name == "Annotation");
				createAnnotationMenuItem.Action.Execute();
				Application.DoEvents();

				var node = networkDiagram.FindChildren<AdornedControl>().SingleOrDefault(x => x.DataContext is NodeViewModel);

				var annotation = node.DataContext as AnnotationViewModel;
				AssertNotNull("When you create an annotation it should have the correct viewModel", annotation);
			}
		}

#pragma warning disable CS0618 // Type or member is obsolete
		static int CountGridLines(DiagramAreaUserControl control)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			return control.FindChildren<DynamicGrid>().Single(g => g.Name == "ScaleGrid").FindChildren<Line>().Count();
		}

		public void TestGridLines()
		{
			var jobHeader1 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var diagram = NetworkTestCase.CreateDiagram(jobHeader1);
			var network = NetworkTestCase.CreateNetwork(diagram);
			network.SwitchToScaled();
			Factory.Save();

			using (var form = new NetworkDiagramForm(Factory.CreateNewFactory().Load<BMNCNShape>(diagram.PK)))
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				var networkControlViewModel = (NetworkUserControlViewModel)networkControl.DataContext;

				Application.DoEvents();

				var gridLineCount = CountGridLines(networkControl.MainDiagramControl);

				networkControlViewModel.ContentScale = 0.5;
				networkControlViewModel.NetworkModel.Refresh(RefreshType.RedrawDiagram);
				Application.DoEvents();

				AssertEquals(@"Even though scale has changed, the number of lines should have halved because we are showing 1/4 of the number of lines, 
					and there are twice as many columns visible.", gridLineCount / 2, CountGridLines(networkControl.MainDiagramControl));
			}
		}

		#region Affinities

		static int CountBackgroundCellSpan(DynamicGrid grid, Color color)
		{
			return (int)GetColumnBackgrounds(grid, color).Sum(r => Math.Ceiling(((decimal)r.Width) / 100));
		}

		static IEnumerable<Rectangle> GetColumnBackgrounds(DynamicGrid grid, Color color)
		{
			return grid.FindChildren<Rectangle>().Where(b => (b.Fill as SolidColorBrush)?.Color == color);
		}

		public void TestRefresh_Affinities()
		{
			var jobHeader1 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var diagram = NetworkTestCase.CreateDiagram(jobHeader1);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			Factory.Save();

			var shape1 = networkViewModel.CreateNewShape(network.DiagramShape);
			shape1.Name = "Shape1";
			var shape2 = networkViewModel.CreateNewShape(network.DiagramShape);
			shape2.Name = "Shape2";

			shape1.Width = 300;
			shape1.Y = 100;
			shape2.Width = 300;
			shape2.Y = 100;

			var shapeAffinity1 = new ShapeAffinity("Red", "RedAffinity", ZGuid.NewZGuid());
			diagram.ShapeAffinities.Add(shapeAffinity1);

			Factory.Save();

			using (var form = new NetworkDiagramForm(Factory.CreateNewFactory().Load<BMNCNShape>(diagram.PK)))
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				Application.DoEvents();
				var formNetworkViewModel = networkControl.ViewModel.NetworkViewModel;

				var grid = NetworkTestHelper.GetDynamicGrid(networkControl.MainDiagramControl);

				var shapeNode1 = formNetworkViewModel.Nodes.Single(n => n.Name == "Shape1");
				var shapeNode2 = formNetworkViewModel.Nodes.Single(n => n.Name == "Shape2");

				AssertEquals("There should be no blocks with a red background because there are no affinities", 0, CountBackgroundCellSpan(grid, Colors.Red));

				formNetworkViewModel.SelectSingleEntity(shapeNode1.Entity);
				var applyAffinityItem1 = formNetworkViewModel.GetSecondLevelNetworkActionsMenuItem_ForTesting(shapeNode1.Entity, "Affinities", "Apply Available Affinities")?.Items.SingleOrDefault();
				AssertNotNull(applyAffinityItem1);
				applyAffinityItem1.Action.Execute();
				Application.DoEvents();

				formNetworkViewModel.SelectSingleEntity(shapeNode2.Entity);
				var applyAffinityItem2 = formNetworkViewModel.GetSecondLevelNetworkActionsMenuItem_ForTesting(shapeNode2.Entity, "Affinities", "Apply Available Affinities")?.Items.SingleOrDefault();
				AssertNotNull(applyAffinityItem2);
				applyAffinityItem2.Action.Execute();
				Application.DoEvents();

				AssertEquals("After conflicting affinities are applied via menu item", 3, CountBackgroundCellSpan(grid, Colors.Red));

				var applyAffinityItem3 = formNetworkViewModel.GetSecondLevelNetworkActionsMenuItem_ForTesting(shapeNode2.Entity, "Affinities", "Remove Applied Affinities")?.Items.SingleOrDefault();
				AssertNotNull(applyAffinityItem3);
				applyAffinityItem3.Action.Execute();
				Application.DoEvents();

				grid = NetworkTestHelper.GetDynamicGrid(networkControl.MainDiagramControl);
				AssertEquals("After conflicting affinities are applied via menu item", 0, CountBackgroundCellSpan(grid, Colors.Red));
			}
		}

		public void TestSaveFormAfterAffinityChanges_ShouldSaveAndHaveNoPendingHasChanges()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape = NetworkTestCase.CreateShape(diagram, "Shape1");

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				AssertNotNull("Pre-condition", networkDiagram);
				var formNetworkViewModel = networkDiagram.ViewModel.NetworkViewModel;

				var affinity = new ShapeAffinity("Tomato", "TestAffinity", ZGuid.NewZGuid());
				diagram.ShapeAffinities.Add(affinity);
				formNetworkViewModel.Network.DiagramEntity.AvailableAffinities.Reload();
				AssertEquals(1, formNetworkViewModel.Network.DiagramEntity.AvailableAffinities.Count);

				var shapeNode = formNetworkViewModel.Nodes.Single(n => n.Name == "Shape1");

				var grid = networkDiagram.MainDiagramControl.FindChildren<DynamicGrid>().Single(g => g.Name == "ScaleGrid");

				AssertEquals("There should be no blocks with a Tomato background because there are no affinities", 0, CountBackgroundCellSpan(grid, Colors.Tomato));

				formNetworkViewModel.SelectSingleEntity(shapeNode.Entity);
				var applyAffinity = shapeNode.MenuItems.WhereNotNull().Single(m => m.Name == "Affinities").Items.Single(m => m.Name == "Apply Available Affinities").Items.Single();
				applyAffinity.Action.Execute();
				Application.DoEvents();

				AssertEquals(true, shape.HasChanges);

				var postingButtons = form as IPostingButtonsProvider;
				ZFormTestHelper.AssertCommandButtons(postingButtons,
						applyEnabled: true, postEnabled: true, cancelEnabled: true,
						applyText: "&Save", postText: "S&ave && Close", cancelText: "&Cancel");

				form.FireSaveButton();

				AssertEquals(false, shape.HasChanges);

				ZFormTestHelper.AssertCommandButtons(postingButtons,
					applyEnabled: true, postEnabled: false, cancelEnabled: true,
					applyText: "&New", postText: "S&ave && Close", cancelText: "&Close");
			}
		}

		public void TestDiagramWithChannels_WhenAffinityConflictsOccur_ShouldHighlightCorrectColumns()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var affinity = NetworkTestCase.CreateAffinity(diagram);

			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			Factory.Save();

			var shape1 = networkViewModel.CreateNewShape(network.DiagramShape);
			var shape2 = networkViewModel.CreateNewShape(network.DiagramShape);

			shape1.Width = 300;
			shape1.X = 100;
			shape1.Y = 75;
			shape2.Width = 300;
			shape2.X = 200;
			shape2.Y = 250;

			NetworkTestCase.LinkAffinity(network, shape1.Shape, affinity);
			NetworkTestCase.LinkAffinity(network, shape2.Shape, affinity);
			Factory.Save();

			AssertColumnPosition("The diagram without channels should put the red columns in the correct spot. SAD!", 400d);

			NetworkTestCase.CreateChannel(diagram);
			Factory.Save();

			AssertColumnPosition("The diagram with channels should move the red columns one slot to the right to match the shapes. SAD!", 500d);

			void AssertColumnPosition(string message, double expectedPosition)
			{
				using (var form = new NetworkDiagramForm(diagram))
				{
					form.Show();
					Application.DoEvents();

					var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
					var networkControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
					var grid = NetworkTestHelper.GetDynamicGrid(networkControl.MainDiagramControl);

					var rectangle = GetColumnBackgrounds(grid, Colors.Red).Single();
					var columnXPosition = rectangle.TranslatePoint(new Point(rectangle.ActualWidth, 0), grid).X;

					AssertEquals(message, expectedPosition, columnXPosition);
				}
			}
		}

		#endregion

		public void TestRefresh_PicksUpShapePositionChanges()
		{
			var jobHeader1 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var diagram = NetworkTestCase.CreateNetwork(NetworkTestCase.CreateDiagram(jobHeader1)).DiagramEntity;

			var shape = NetworkTestCase.CreateShape(diagram, "Shlep");
			shape.Width = 300;
			shape.Y = 100;

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram.Shape))
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				var network = (NetworkUserControlViewModel)networkDiagram.DataContext;

				Application.DoEvents();

				var nodes = networkDiagram.FindChildren<AdornedControl>().Where(x => x.DataContext is NodeViewModel).ToArray();
				var shapeNode = nodes.Select(n => n.DataContext).Cast<NodeViewModel>().Single(n => n.Name == "Shlep");

				shapeNode.Entity.X = 190;
				AssertEquals(190d, shapeNode.Entity.X);
				network.NetworkModel.Refresh(RefreshType.RedrawDiagram);

				Application.DoEvents();
				nodes = networkDiagram.FindChildren<AdornedControl>().Where(x => x.DataContext is NodeViewModel).ToArray();
				shapeNode = nodes.Select(n => n.DataContext).Cast<NodeViewModel>().Single(n => n.Name == "Shlep");

				AssertEquals(190d, shapeNode.Entity.X);
				AssertEquals(190d, shapeNode.X);
			}
		}

		public void TestRefresh_ShapesAtOriginStayPut()
		{
			var workItem = Factory.New<IWorkItem>();
			var jobHeader = GetJobHeaderWithCustomWorkflow(workItem, "testshape");
			var diagramShape = CreateDefaultDiagram(jobHeader);

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.WorkItem).ShowEditForm((BusinessObject)workItem))
			{
				form.Show();
				form.WindowState = FormWindowState.Maximized;
				Application.DoEvents();

				var userControl = GetWorkflowsUserControl(form);
				userControl.WorkflowNCNTabControl_Exposed.SelectedTab = userControl.NCNTabPage_Exposed;
				var networkDiagram = GetNetworkUserControl(userControl);
				Application.DoEvents();

				var cornerShape = networkDiagram.FindNodeItemOrDefault("testshape");
				cornerShape.Name = "Corner";
				cornerShape.Width = 200d;
				cornerShape.Height = 100d;
				cornerShape.X = 0d;
				cornerShape.Y = 55d;

				Application.DoEvents();
				Factory.Save();
				Application.DoEvents();

				var extraShapeWorkflow = VisualBoardsTestCase.CreateWorkflow(jobHeader, "Extra workflow");
				var extraShape1 = NetworkTestCase.CreateShape(diagramShape, "extra");
				extraShape1.BNS_Name = "extra";
				extraShape1.BNS_RelatedEntityID = extraShapeWorkflow.PK;
				extraShape1.Name = "extra";
				extraShape1.Width = 200d;
				extraShape1.Height = 100d;
				extraShape1.Top = 0d;
				extraShape1.Left = 300d;

				AssertEquals(true, (extraShape1 as INetworkEntity).IsPositioned);

				Application.DoEvents();
				Factory.Save();
				Application.DoEvents();
				cornerShape = networkDiagram.FindNodeItemOrDefault("testshape");
				cornerShape.Y = 0;

				Application.DoEvents();
				Factory.Save();
				Application.DoEvents();

				userControl.RelationshipDesignerUserControl.Network.Refresh(RefreshType.Saved);
				Application.DoEvents();
				var testCornerShapeB = networkDiagram.FindNodeItemOrDefault("testshape");
				var testExtraShapeB = networkDiagram.FindNodeItemOrDefault("extra");

				AssertEquals(300, (int)testExtraShapeB.X);
				AssertEquals(0, (int)testExtraShapeB.Y);
				AssertEquals(0, (int)testCornerShapeB.X);
				AssertEquals(0, (int)testCornerShapeB.Y);
			}
		}

		public void TestRefresh_NewShapeGetsPositionedAutomatically()
		{
			var workItem = Factory.New<IWorkItem>();
			var jobHeader = GetJobHeaderWithCustomWorkflow(workItem, "testshape");
			var diagramShape = CreateDefaultDiagram(jobHeader);

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.WorkItem).ShowEditForm((BusinessObject)workItem))
			{
				form.Show();
				form.WindowState = FormWindowState.Maximized;
				Application.DoEvents();

				var userControl = GetWorkflowsUserControl(form);
				userControl.WorkflowNCNTabControl_Exposed.SelectedTab = userControl.NCNTabPage_Exposed;
				var networkDiagram = GetNetworkUserControl(userControl);
				Application.DoEvents();

				var cornerShape = networkDiagram.FindNodeItemOrDefault("testshape");
				cornerShape.Name = "Corner";
				cornerShape.Width = 200d;
				cornerShape.Height = 100d;

				Application.DoEvents();
				Factory.Save();
				Application.DoEvents();

				var extraShapeWorkflow = VisualBoardsTestCase.CreateWorkflow(jobHeader, "Extra workflow");
				var extraShape1 = NetworkTestCase.CreateShape(diagramShape, "extra");
				extraShape1.BNS_Name = "extra";
				extraShape1.BNS_RelatedEntityID = extraShapeWorkflow.PK;
				extraShape1.Name = "extra";
				extraShape1.Width = 200d;
				extraShape1.Height = 100d;

				Application.DoEvents();
				Factory.Save();
				Application.DoEvents();
				var testCornerShape = networkDiagram.FindNodeItemOrDefault("testshape");
				var testExtraShape = networkDiagram.FindNodeItemOrDefault("extra");

				AssertEquals(0, (int)testExtraShape.X);
				AssertEquals(0, (int)testCornerShape.X);
				const int yValueAccordingToDefaults = 165;
				AssertEquals(yValueAccordingToDefaults, (int)testExtraShape.Y);
				AssertEquals(0, (int)testCornerShape.Y);

				Application.DoEvents();
				Factory.Save();
				Application.DoEvents();
				userControl.RelationshipDesignerUserControl.Network.Refresh(RefreshType.Saved);
				Application.DoEvents();

				testCornerShape = networkDiagram.FindNodeItemOrDefault("testshape");
				testExtraShape = networkDiagram.FindNodeItemOrDefault("extra");

				AssertEquals(0, (int)testExtraShape.X);
				AssertEquals(0, (int)testCornerShape.X);
				AssertEquals(yValueAccordingToDefaults, (int)testExtraShape.Y);
				AssertEquals(0, (int)testCornerShape.Y);
			}
		}

		public void TestOpenAndClosePropertiesForm_ForScaledDiagramWithChannels_ShouldNotMoveShapes()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			NetworkTestCase.CreateChannel(diagram);

			var network = NetworkTestCase.CreateNetwork(diagram);
			var shape = NetworkTestCase.CreateShape(network.DiagramEntity, "Squanch");
			shape.X = 100d;
			shape.Y = 100d;

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var diagramControl = ((NetworkUserControl)elementHost.Child).MainDiagramControl;
#pragma warning restore CS0618 // Type or member is obsolete

				var shapeControl = NetworkTestHelper.GetShapeControl(diagramControl, shape.Name);
				var position = NetworkTestHelper.GetRelativePosition(shapeControl, diagramControl);
				AssertEquals(206d, position.X);

				diagramControl.ViewModel.NetworkControlViewModel.MenuItems.Single(i => i?.Name == "Edit Properties").Action.Execute();
				NetworkGUITestCase.DoEventsThoroughly();

				shapeControl = NetworkTestHelper.GetShapeControl(diagramControl, shape.Name);
				position = NetworkTestHelper.GetRelativePosition(shapeControl, diagramControl);
				AssertEquals("Opening and closing the properties form shouldn't move the shapes. SAD!", 206d, position.X);
			}
		}

		public void TestCCIsUpdated()
		{
			var jobHeader1 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var diagram = NetworkTestCase.CreateNetwork(NetworkTestCase.CreateDiagram(jobHeader1, isScaled: true)).DiagramEntity;
			diagram.Shape.BNS_JobType = "ORG";
			diagram.Shape.Scale = diagram.Shape.ResolutionIncrement = new ZInt(60).GetDateTimeFromMinutes();

			var shape1 = NetworkTestCase.CreateShape(diagram, "ccShape");
			shape1.Width = 300;
			shape1.Y = 100;

			var shape2 = NetworkTestCase.CreateShape(diagram, "nonccShape");
			shape2.Width = 100;
			shape2.Y = 300;

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram.Shape))
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				var network = (NetworkUserControlViewModel)networkDiagram.DataContext;

				network.NetworkModel.Refresh(RefreshType.RefreshButton);

				Application.DoEvents();

				var nodes = networkDiagram.FindChildren<AdornedControl>().Where(x => x.DataContext is NodeViewModel).ToArray();
				var ccShapeNode = nodes.Select(n => n.DataContext).Cast<NodeViewModel>().Single(n => n.Name == "ccShape");
				var nonccShapeNode = nodes.Select(n => n.DataContext).Cast<NodeViewModel>().Single(n => n.Name == "nonccShape");

				AssertEquals(true, ccShapeNode.Entity.IsOnCriticalPath);
				AssertEquals(false, nonccShapeNode.Entity.IsOnCriticalPath);

				nonccShapeNode.Width = 900;
				network.NetworkModel.Refresh(RefreshType.Saving);

				network.NetworkViewModel.Refresh();
				Application.DoEvents();

				nodes = networkDiagram.FindChildren<AdornedControl>().Where(x => x.DataContext is NodeViewModel).ToArray();
				ccShapeNode = nodes.Select(n => n.DataContext).Cast<NodeViewModel>().Single(n => n.Name == "ccShape");
				nonccShapeNode = nodes.Select(n => n.DataContext).Cast<NodeViewModel>().Single(n => n.Name == "nonccShape");

				AssertEquals(false, ccShapeNode.Entity.IsOnCriticalPath);
				AssertEquals(true, nonccShapeNode.Entity.IsOnCriticalPath);
			}
		}

		public void TestSwitchToScaledMode_PersistsCorrectSize()
		{
			var jobHeader1 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var diagram = NetworkTestCase.CreateNetwork(NetworkTestCase.CreateDiagram(jobHeader1)).DiagramEntity;
			diagram.Shape.BNS_JobType = "ORG";
			diagram.Shape.Scale = diagram.Shape.ResolutionIncrement = new ZInt(60).GetDateTimeFromMinutes();

			var shape = NetworkTestCase.CreateShape(diagram, "Shlep");
			shape.Width = 300;
			shape.Y = 100;

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram.Shape))
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				var network = (NetworkUserControlViewModel)networkDiagram.DataContext;

				Application.DoEvents();

				var nodes = networkDiagram.FindChildren<AdornedControl>().Where(x => x.DataContext is NodeViewModel).ToArray();
				var shapeNode = nodes.Select(n => n.DataContext).Cast<NodeViewModel>().Single(n => n.Name == "Shlep");

				AssertEquals(300d, shapeNode.Width);
				AssertEquals(150d, shapeNode.Height);
			}
		}

		public void TestCreateShape_ShinkToFitInFixedParent()
		{
			var jobHeader1 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = VisualBoardsTestCase.CreateWorkflow(jobHeader1, "A workflow");

			var diagram = NetworkTestCase.CreateDiagram(jobHeader1, isScaled: true);
			diagram.BNS_JobType = "ORG";
			var subDiagram = NetworkTestCase.CreateShape(jobHeader2, diagram, name: "subDiagram");

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				var nodes = networkDiagram.FindChildren<AdornedControl>().Where(x => x.DataContext is NodeViewModel).ToArray();
				var subDiagramNode = nodes.Select(n => n.DataContext).Cast<NodeViewModel>().Single(n => n.Name == "subDiagram");
				subDiagramNode.Width = 100;
				subDiagramNode.Height = 100;

				Application.DoEvents();

				var controlViewModel = (NetworkUserControlViewModel)networkDiagram.DataContext;
				var networkViewModel = controlViewModel.NetworkViewModel;
				var network = (JobNetwork)controlViewModel.NetworkModel;
				subDiagram.PinShape(networkViewModel);

				var newShape = networkViewModel.CreateNewShape(subDiagram);
				newShape.Width = 600;
				newShape.Height = 600;

				var newShapeNode = networkDiagram.MainDiagramControl.ViewModel.CreateNewNode(new Location(0, 0), newShape);
				Application.DoEvents();

				AssertEquals(100d, subDiagramNode.Width);
				AssertEquals(100d, subDiagramNode.Height);
				AssertEquals(100d, newShapeNode.Width);
				AssertEquals(55d, newShapeNode.Height);
			}
		}

		public void TestCreateShape_GrowParentToFit()
		{
			var jobHeader1 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = VisualBoardsTestCase.CreateWorkflow(jobHeader1, "A workflow");

			var diagram = NetworkTestCase.CreateDiagram(jobHeader1, isScaled: true);
			diagram.BNS_JobType = "ORG";
			var setupNetwork = NetworkTestCase.CreateNetwork(diagram);
			var subDiagram = NetworkTestCase.CreateShape(jobHeader2, diagram, name: "subDiagram");

			var entityToImport = NetworkTestCase.CreateDiagram(Factory, name: "Entity to import", isScaled: true);
			var importNetwork = NetworkTestCase.CreateNetwork(entityToImport);
			var shapeThatMakesEntityToImportBigger = NetworkTestCase.CreateShape(entityToImport, "Embiggener");
			shapeThatMakesEntityToImportBigger.AsEntity(importNetwork).Width = 600;
			shapeThatMakesEntityToImportBigger.AsEntity(importNetwork).Height = 555;

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				var viewModel = (NetworkUserControlViewModel)networkDiagram.DataContext;
				var network = (JobNetwork)viewModel.NetworkModel;

				var nodes = networkDiagram.FindChildren<AdornedControl>().Where(x => x.DataContext is NodeViewModel).ToArray();
				var subDiagramNode = nodes.Select(n => n.DataContext).Cast<NodeViewModel>().Single(n => n.Name == "subDiagram");
				subDiagramNode.Width = 100;
				subDiagramNode.Height = 100;

				Application.DoEvents();

				using (BusinessObjectModulePickerTest.SelectRecordsOnDialogShown(entityToImport))
				{
					networkDiagram.MainDiagramControl.ViewModel.ImportEntity(new Point(0, 0), subDiagramNode.Entity);
				}

				Application.DoEvents();

				AssertEquals(600d, subDiagramNode.Width);
				AssertEquals(600d, subDiagramNode.Height);  // 555 + 40 (top margin) + 5 (bottom margin)
			}
		}

		public void TestMovingControls_ChildrenOnlyMoveRelativeToTheirParent()
		{
			var jobHeader1 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = VisualBoardsTestCase.CreateWorkflow(jobHeader1, "A workflow");

			var diagram = NetworkTestCase.CreateDiagram(jobHeader1);
			diagram.BNS_JobType = "ORG";
			var subDiagram = NetworkTestCase.CreateShape(jobHeader2, diagram, name: "subDiagram");
			var shape = NetworkTestCase.CreateShape(workflow, subDiagram, name: "shape");

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				var viewModel = (NetworkUserControlViewModel)networkDiagram.DataContext;

				var nodes = networkDiagram.FindChildren<AdornedControl>().Where(x => x.DataContext is NodeViewModel).ToArray();

				AssertEquals(2, nodes.Length);

				var subDiagramNode = nodes.Select(n => n.DataContext).Cast<NodeViewModel>().Single(n => n.Name == "subDiagram");
				var shapeNode = nodes.Select(n => n.DataContext).Cast<NodeViewModel>().Single(n => n.Name == "shape");

				subDiagramNode.Width = 400;
				subDiagramNode.Height = 400;
				subDiagramNode.X = 50;
				subDiagramNode.Y = 50;

				shapeNode.Width = 100;
				shapeNode.Height = 100;
				shapeNode.X = 100;
				shapeNode.Y = 100;

				Application.DoEvents();

				AssertEquals(50d, subDiagramNode.X);
				AssertEquals(50d, subDiagramNode.Y);
				AssertEquals(100d, shapeNode.X);
				AssertEquals(100d, shapeNode.Y);

				subDiagramNode.X -= 200;
				AssertEquals(0d, subDiagramNode.X);
				AssertEquals(50d, shapeNode.X);
			}
		}

		public void TestMovingControls_PinnedShapesArePinned()
		{
			var jobHeader1 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = VisualBoardsTestCase.CreateWorkflow(jobHeader1, "A workflow");

			var diagram = NetworkTestCase.CreateDiagram(jobHeader1);
			diagram.BNS_JobType = "ORG";
			var subDiagram = NetworkTestCase.CreateShape(jobHeader2, diagram, name: "subDiagram");
			var shape = NetworkTestCase.CreateShape(workflow, subDiagram, name: "shape");

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				var viewModel = (NetworkUserControlViewModel)networkDiagram.DataContext;

				var nodes = networkDiagram.FindChildren<AdornedControl>().Where(x => x.DataContext is NodeViewModel).ToArray();

				AssertEquals(2, nodes.Length);

				var subDiagramNode = nodes.Select(n => n.DataContext).Cast<NodeViewModel>().Single(n => n.Name == "subDiagram");
				var shapeNode = nodes.Select(n => n.DataContext).Cast<NodeViewModel>().Single(n => n.Name == "shape");

				subDiagramNode.Width = 400;
				subDiagramNode.Height = 400;
				subDiagramNode.X = 50;
				subDiagramNode.Y = 50;

				shapeNode.Width = 100;
				shapeNode.Height = 100;
				shapeNode.X = 100;
				shapeNode.Y = 100;

				Application.DoEvents();

				AssertEquals(50d, subDiagramNode.X);
				AssertEquals(50d, subDiagramNode.Y);
				AssertEquals(100d, shapeNode.X);
				AssertEquals(100d, shapeNode.Y);

				subDiagramNode.X -= 200;
				AssertEquals(0d, subDiagramNode.X);
				AssertEquals(50d, shapeNode.X);
			}
		}

		public void TestCtrlSWorksWhenTypingInWPFFields()
		{
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader.FH_CompletionStatement = "Weenus";
			var workflow = VisualBoardsTestCase.CreateWorkflow(jobHeader, "A workflow");
			var diagram = NetworkTestCase.CreateDiagram(jobHeader, name: jobHeader.FH_CompletionStatement);
			diagram.BNS_JobType = "ORG";

			diagram.RunPreSaveValidation();
			AssertNoErrors(diagram);

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				form.FireValidateAllForTest();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				var node = networkDiagram.MainDiagramControl.FindChildren<EntityDetails>().Single(x => x.DataContext is NodeViewModel);
				var textBoxes = node.FindChildren<TextBox>().ToArray();
				var textBox = textBoxes.Single(x => x.Text == jobHeader.FH_CompletionStatement);
				Keyboard.Focus(textBox);
				Application.DoEvents();
				textBox.Text = "WeenusBeanus";
				Application.DoEvents();

				AssertEquals(true, textBox.IsKeyboardFocused);

				VisualBoardsTestCase.AssertSaved(form.FireSaveButton());

				Application.DoEvents();
				AssertEquals(true, textBox.IsKeyboardFocused);

				AssertEquals("WeenusBeanus", diagram.BNS_Name);
			}

			AssertEquals("WeenusBeanus", Factory.CreateNewFactory().Load<BMNCNShape>(diagram.PK).BNS_Name);
		}

		public void TestExpectNoExceptionFromNodeWithDeletedEntity()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = VisualBoardsTestCase.CreateWorkflow(jobHeader, "A workflow");
			var diagram = NetworkTestCase.CreateDiagram(jobHeader);
			diagram.BNS_JobType = "ORG";

			var shape = NetworkTestCase.CreateShape(workflow, diagram);

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				var nodes = networkDiagram.FindChildren<AdornedControl>().Where(x => x.DataContext is NodeViewModel).ToArray();

				AssertEquals(1, nodes.Length);

				var node = nodes.Single();
				var nodeViewModel = (NodeViewModel)node.DataContext;

				shape.Delete();
				AssertNoExceptionThrown(() => networkDiagram.MainDiagramControl.ViewModel.DeleteNode(nodeViewModel));
				form.Refresh();
				Application.DoEvents();
			}
		}

		public void TestDeletedEntityDisappears()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = VisualBoardsTestCase.CreateWorkflow(jobHeader, "A workflow");
			var diagram = NetworkTestCase.CreateDiagram(jobHeader);
			diagram.BNS_JobType = "ORG";

			var shape = NetworkTestCase.CreateShape(workflow, diagram);

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				var viewModel = (NetworkUserControlViewModel)networkDiagram.DataContext;

				var nodes = networkDiagram.FindChildren<AdornedControl>().Where(x => x.DataContext is NodeViewModel).ToArray();

				AssertEquals(1, nodes.Length);

				var node = nodes.Single();
				var nodeViewModel = (NodeViewModel)node.DataContext;

				shape.Delete();
				Application.DoEvents();

				nodes = networkDiagram.FindChildren<AdornedControl>().Where(x => x.DataContext is NodeViewModel).ToArray();
				AssertEquals(0, nodes.Length);
			}
		}

		public void TestNewEntityAppears()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = VisualBoardsTestCase.CreateWorkflow(jobHeader, "A workflow");
			var diagram = NetworkTestCase.CreateDiagram(jobHeader);
			diagram.BNS_JobType = "ORG";

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				var viewModel = (NetworkUserControlViewModel)networkDiagram.DataContext;

				var nodes = networkDiagram.FindChildren<AdornedControl>().Where(x => x.DataContext is NodeViewModel).ToArray();

				AssertEquals(0, nodes.Length);

				var shape = NetworkTestCase.CreateShape(workflow, diagram);
				Application.DoEvents();

				nodes = networkDiagram.FindChildren<AdornedControl>().Where(x => x.DataContext is NodeViewModel).ToArray();
				AssertEquals(1, nodes.Length);
			}
		}

		public void TestBindingIsBound()
		{
			/*
			 * Hello Future Friend. Because this took a while to figure out for me, let me give you a hint:
			 * The condition that caused this test to fail was setting the WPF DataContext before calling control.Show.
			 * *Some* of the fields would drop their binding source for no reason.
			 */
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = VisualBoardsTestCase.CreateWorkflow(jobHeader, "A workflow");
			var diagram = NetworkTestCase.CreateDiagram(jobHeader, scheduledStartTimeUTC: ZDateTime.UtcNow);
			diagram.BNS_JobType = "ORG";
			var shape = NetworkTestCase.CreateShape(workflow, diagram);
			var network = NetworkTestCase.CreateNetwork(diagram);
			network.SwitchToScaled();

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				var viewModel = (NetworkUserControlViewModel)networkDiagram.DataContext;
				var node = networkDiagram.FindChildren<AdornedControl>().Single(x => x.DataContext is NodeViewModel);

				AssertNotNull("This should never happen.", node.DataContext);

				var label = node.FindChildren<System.Windows.Controls.Label>().SingleOrDefault(l => l.ToolTip?.ToString() == "Scheduled Start");

				AssertNotNull("Hopefully this is testing a control set via template, and not a control", label);

				var bindingExpression = label.GetBindingExpression(System.Windows.Controls.Label.ContentProperty);

				AssertNotNull("This shouldn't have been cleared!!!!", bindingExpression);
				AssertEquals("The label ought to be bound to the same object as it's parent.", node.DataContext, bindingExpression.DataItem);
				AssertEquals("StartDateReadableText", bindingExpression.ResolvedSourcePropertyName);
			}
		}

		public void TestResizingShape_ShouldSaveEditedName()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var shape1 = networkViewModel.CreateNewShape(network.DiagramEntity);
			shape1.Name = "Shape1";

			network.DiagramEntity.Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagram))
			{
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				var node = networkDiagram.FindChildren<AdornedControl>().Single(x => x.DataContext is NodeViewModel && ((NodeViewModel)x.DataContext).Name == "Shape1");
				var shapeViewModel = node.DataContext as NodeViewModel;
				var foundShape = (ShapeNetworkEntity)shapeViewModel.Entity;

				var textBoxes = node.FindChildren<TextBox>().ToArray();
				var textBox = textBoxes.Where(x => x.Text == shape1.Name).Single();

				AssertEquals("Shape1", textBox.Text);

				Keyboard.Focus(textBox);
				textBox.Text = "new text";

				AssertEquals(true, textBox.IsKeyboardFocused);

				Application.DoEvents();

				AssertEquals("Shape1", foundShape.Name);
				AssertEquals("new text", textBox.Text);

				var thumbs = networkDiagram.FindChildren<ResizeThumb>();
				AssertEquals("4 thumbs are expected, one at each corner", 4, thumbs.Count());
				var resizeThumb = thumbs.Single(t => t.HorizontalAlignment == System.Windows.HorizontalAlignment.Right && t.VerticalAlignment == VerticalAlignment.Bottom);
				resizeThumb.RaiseEvent(new DragDeltaEventArgs(1, 1));
				Application.DoEvents();

				AssertEquals("Change of shape name should be retained and not reverted", "new text", foundShape.Name);
				AssertEquals("Change of text box contents should be retained and not reverted", "new text", textBox.Text);
			}
		}

		public void TestCreateChildShape_WithoutLinkedEntity()
		{
			var jobHeader1 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var diagram = NetworkTestCase.CreateDiagram(jobHeader1);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var shape = networkViewModel.CreateNewShape(diagram);

			Factory.Save();

			using (var form = new NetworkDiagramForm(Factory.CreateNewFactory().Load<BMNCNShape>(diagram.PK)))
			{
				form.Show();
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				var nodes = networkDiagram.FindChildren<AdornedControl>().Where(x => x.DataContext is NodeViewModel).ToArray();
				var node = nodes.Select(n => n.DataContext).Cast<NodeViewModel>().Single(n => n.Name == "New Shape");
				var menuItem = node.MenuItems.WhereNotNull().Single(m => m.Name == "Create New...").Items.WhereNotNull().Single(m => m.Name == "Shape");
				menuItem.Action.Execute();
				Application.DoEvents();

				Factory.Save();
				var nodeShape = networkDiagram.FindChildren<NodeItem>().ToArray();
				AssertEquals("The diagram should contain 2 nodes - The parent AND the child", 2, nodeShape.Length);
			}
		}

		public void TestDoubleClick_OpenRelatedNetworkDiagram()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "Test Diagram");
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var action = new SwitchToScaledModeAction(networkViewModel);
			Factory.Save();

			action.ExecuteForEntityWithoutAccessCheck(diagram);
			action.FactoryForSpawnedNetwork_ExposedForTest.Save();

			var newFactory = new BusinessObjectFactory();
			var scaledDiagram = newFactory.LoadTop1<BMNCNShape>(new ZQuery(BMNCNShapeSchema.BNS_Name, "Test Diagram (scaled copy)"));
			AssertNotNull(scaledDiagram);

			using (var scaledDiagramForm = Application.OpenForms.OfType<NetworkDiagramForm>().FirstOrDefault(f => f.DataSource.PK == scaledDiagram.PK))
			{
				scaledDiagramForm.Close();
			}

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();

				var relatedDiagramsTabPage = form.RelatedDiagramsPageTab;
				var mainTabControl = (System.Windows.Forms.TabControl)form.Find(t => t.Name == "MainTabControl").First();
				mainTabControl.SelectedTab = relatedDiagramsTabPage;

				AssertEquals("There should be one diagram in the grid - linked scaled diagram", 1, form.DiagramsGrid.VisibleRowCount);

				using (var scaledDiagramForm = Application.OpenForms.OfType<NetworkDiagramForm>().FirstOrDefault(f => f.DataSource.PK == scaledDiagram.PK))
				{
					AssertNull("'Test Diagram (scaled copy)' diagram is not opened yet and not in the opened form cache.", scaledDiagramForm);
				}

				var rowNotificationsRectangle = form.DiagramsGrid.GetRowNotificationRectangle(0);
				var mouseEvent = new System.Windows.Forms.MouseEventArgs(MouseButtons.Left, 2, rowNotificationsRectangle.X, rowNotificationsRectangle.Y, 0);
				form.DiagramsGrid_MouseDoubleClick(form.DiagramsGrid, mouseEvent);

				using (var scaledDiagramForm = Application.OpenForms.OfType<NetworkDiagramForm>().FirstOrDefault(f => f.DataSource.PK == scaledDiagram.PK))
				{
					AssertNotNull("'Test Diagram (scaled copy)' diagram should be shown in the popup form when grid double click happened.", scaledDiagramForm);
				}
			}
		}

		#endregion

		#region Performance

		[SnailTest]
		[TestDate(2015, 7, 14)]
		public void TestLoadDiagramWithLongCriticalChain_DbHits()
		{
			BMSRegistry.Instance.NCNRibbonEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			BMSTestHelper.ClearUberFactory();
			const int ccLength = 100;

			BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.WorkItemWorkflowDescriptorCode);
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var job = BMSTestHelper.CreateJob<IWorkItem>(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader(job, addDefaultProcessHeaderIfNone: false, description: "Pluto Day");
			var diagramShape = NetworkTestCase.CreateDiagram(jobHeader);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;

			var previousShape = default(ShapeNetworkEntity);

			for (var i = 0; i < ccLength; i++)
			{
				var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "My name's yours" + i);
				var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60 * 8 * 3);
				var shape = NetworkTestCase.CreateShape(workflow, diagram, "What's Alaska?");

				if (i > 0)
				{
					previousShape.MakeVisiblePrerequisiteOf(shape);
				}

				previousShape = shape;
			}

			network.SwitchToScaled();
			networkViewModel.PushAsLateAsPossible();
			networkViewModel.ToggleApproval();

			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagram.Shape))
			{
				NetworkGUITestCase.DoEventsThoroughly(); // to make sure ZStmNoteTabPage is initialised (UserUdleWorker has enough idle time to run UpdateInitialNoteImageViaOnLayout)

				var formFactory = form.BusinessEntity.Factory;
				AssertDbHits(new Dictionary<string, int>
				{
					{ WorkItemSchema.Constants.TableName, 1 },
					{ BMNCNAttachmentSchema.Constants.TableName, 6 }, // 2 hits for InitialiseShapesAndAttachments (this is the correct number of DB hits for BMNCNAttachment on a diagram with no grandchildren. Please do not compromise here.) and 1 hit per every 32 shapes for the ribbon action ToggleResourceDependencyVisibilityAction
					{ BMNCNChannelSchema.Constants.TableName, 1 },
					{ BMNCNLevelingRuleSchema.Constants.TableName, 1 },
					{ BMNCNScheduleSchema.Constants.TableName, 2 }, // 1 hit per every 64 shapes
					{ BMNCNShapeSchema.Constants.TableName, 3 },
					{ ProcessHeaderSchema.Constants.TableName, 2 },
					{ ProcessHeaderLinkSchema.Constants.TableName, 2 },
					{ ProcessTasksSchema.Constants.TableName, 3 }, // 1 hit per every 64 shapes + 1
					{ StmNoteSchema.Constants.TableName, 1 },
					{ BMSystemSchema.Constants.TableName, 1 }, // 1 hit from the ribbon actions: CreateWorkflowActionBase -> IsLinkedToRealEntityEnabledForBMS
					{ BMSystemWorkflowDeterminerSchema.Constants.TableName, 1 }, // 1 hit from the ribbon actions: CreateWorkflowActionBase -> IsLinkedToRealEntityEnabledForBMS
				}, formFactory);

				formFactory.ResetDatabaseLoadCount();
				form.FireSaveButton();

				AssertMaxDbHits(new Dictionary<string, int>
				{
					{ GenCustomAddOnRuleAckSchema.Constants.TableName, 1 },
					{ GenCustomAddOnValueSchema.Constants.TableName, 1 },
					{ GlbCompanySchema.Constants.TableName, 1 },
					{ StmNoteSchema.Constants.TableName, 1 },
					{ WorkItemSchema.Constants.TableName, 1 },
					{ BMSystemReleaseGroupSchema.Constants.TableName, 1 },
					{ BMSystemSchema.Constants.TableName, 1 },
					{ BMSystemWorkflowDeterminerSchema.Constants.TableName, 1 },
					{ ProcessHeaderSchema.Constants.TableName, 1 },
					{ ProcessHeaderLinkSchema.Constants.TableName, 1 },
					{ ProcessTasksSchema.Constants.TableName, 1 },
					{ ProcessWorkflowExceptionTypeSchema.Constants.TableName, 1 }
				}, formFactory);
			}
		}

		[TestDate(2017, 10, 11)]
		public void TestVeryComplexNetworkDbHits_Validation()
		{
			var network = NetworkTestCase.CreateVeryComplexNetwork(Factory);
			Factory.ResetDatabaseLoadCount();

			AssertDbHits(new Dictionary<string, int>(), Factory);

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowFormForNewEntity(network.DiagramShape))
			{
				AssertDbHits(new Dictionary<string, int>
				{
					{ BMNCNAttachmentSchema.Constants.TableName, 1 },
					{ BMNCNShapeSchema.Constants.TableName, 1 },
				}, form.BusinessEntity.Factory);

				form.FireValidateAllForTest();

				AssertMaxDbHits(new Dictionary<string, int>
				{
					{ BMNCNAttachmentSchema.Constants.TableName, 1 },
					{ BMNCNShapeSchema.Constants.TableName, 1 },
					{ GenCustomAddOnRuleAckSchema.Constants.TableName, 3 },
					{ GenCustomAddOnValueSchema.Constants.TableName, 1 },
					{ GlbCompanySchema.Constants.TableName, 1 },
					{ OrgCollectionNoteSchema.Constants.TableName, 1 },
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ ProcessHeaderSchema.Constants.TableName, 2 },
					{ ProcessTasksSchema.Constants.TableName, 1 },
					{ RefCountrySchema.Constants.TableName, 1 },
					{ RefCurrencySchema.Constants.TableName, 1 },
					{ OrgCountryDataSchema.Constants.TableName, 1 },
					{ OrgCusCodeSchema.Constants.TableName, 1 },
					{ ProcessWorkflowExceptionTypeSchema.Constants.TableName, 1 }
				}, form.BusinessEntity.Factory);
			}
		}

		public void TestActiveBusinessObjectCollections_ForChannels_ShouldOnlyBeOnePerDiagram()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape1 = NetworkTestCase.CreateShape(diagram);
			var shape2 = NetworkTestCase.CreateShape(diagram);

			using (ActiveBusinessObjectCollection.TrackIndexedCollectionCounts_ForTest())
			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				form.DataSource.MarkAsNeedingValidationIncludingChildren();
				form.FireValidateAllForTest();

				AssertContainsExactElementsInAnyOrder("There should be one collection for the diagram and none for the other shapes. SAD!", new[]
					{
						new KeyValuePair<Type, int>(typeof(BMNCNChannelCollection), 1),
					}, ActiveBusinessObjectCollection.IndexedCollections_ForTest
						.OrderBy(kvp => kvp.Key.Name)
						.Where(kvp => kvp.Key == typeof(BMNCNChannelCollection))
				);
			}
		}

		#endregion

		#region Concurrency

		public void TestWhenShapeDeletedInOneFactory_AndShapeVisualisedInAnother_ShouldNotDieHorribly_ShouldDieGracefully()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape = NetworkTestCase.CreateShape(diagram);

			Factory.Save();

			using (var diagramForm = new NetworkDiagramForm(diagram))
			{
				diagramForm.Show();
				Application.DoEvents();

				var entity = NetworkGUITestCase.FindShapeOnForm(shape, diagramForm);

				CombineAssertions("PRE: nothing has been deleted yet", () =>
				{
					AssertEquals(false, shape.IsDeleted);
					AssertEquals(false, entity.IsDeleted);
				});

				OpenDiagramInAnotherFactoryAndDeleteAShape(diagram, shape);

				CombineAssertions("Our deleted shapes don't know they're already deleted", () =>
				{
					AssertEquals(false, shape.IsDeleted);
					AssertEquals(false, entity.IsDeleted);
				});

				entity.X += 1;
				entity.Y += 1;

				CombineAssertions("Our deleted shapes are still unaware they're already deleted", () =>
				{
					AssertEquals(false, shape.IsDeleted);
					AssertEquals(false, entity.IsDeleted);
				});

				diagramForm.FireSaveButton();

				CombineAssertions("Our shapes should now be aware they're deleted", () =>
				{
					AssertEquals(true, shape.IsDeleted);
					AssertEquals(true, entity.IsDeleted);
				});

				AssertNoExceptionThrown("Fiddling with a deleted shape should cause no exceptions, but instead...", () =>
				{
					entity.X += 1;
					entity.Y += 1;
				});
			}

			AssertNullOrEmpty("ErrorReporter.LastMessageReported", ErrorReporter.LastMessageReported);
			AssertNull("Shape deletion should have been persisted", Factory.CreateNewFactory().Load<BMNCNShape>(shape.PK));
		}

		public void TestWhenPinnedShapeDeletedInOneFactory_AndShapeVisualisedInAnother_ShouldNotDieHorribly_ShouldDieGracefully()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape = NetworkTestCase.CreateShape(diagram);
			shape.AllowChangingPinnedStatus_ForTest();
			shape.IsPinned = true;

			Factory.Save();

			using (var diagramForm = new NetworkDiagramForm(diagram))
			{
				diagramForm.Show();
				Application.DoEvents();

				var entity = NetworkGUITestCase.FindShapeOnForm(shape, diagramForm);

				CombineAssertions("PRE: nothing has been deleted yet", () =>
				{
					AssertEquals(false, shape.IsDeleted);
					AssertEquals(false, entity.IsDeleted);
				});

				CombineAssertions("PRE: We have pinned things", () =>
				{
					AssertEquals(true, shape.IsPinned);
					AssertEquals(true, entity.IsPinned);
				});

				OpenDiagramInAnotherFactoryAndDeleteAShape(diagram, shape);

				CombineAssertions("Our deleted shapes don't know they're already deleted", () =>
				{
					AssertEquals(false, shape.IsDeleted);
					AssertEquals(false, entity.IsDeleted);
				});

				entity.X += 1;
				entity.Y += 1;

				CombineAssertions("Our deleted shapes are still unaware they're already deleted", () =>
				{
					AssertEquals(false, shape.IsDeleted);
					AssertEquals(false, entity.IsDeleted);
				});

				diagramForm.FireSaveButton();

				CombineAssertions("Our shapes should now be aware they're deleted", () =>
				{
					AssertEquals(true, shape.IsDeleted);
					AssertEquals(true, entity.IsDeleted);
				});

				AssertNoExceptionThrown("Fiddling with a deleted shape should cause no exceptions, but instead...", () =>
				{
					entity.X += 1;
					entity.Y += 1;
				});
			}

			AssertNullOrEmpty("ErrorReporter.LastMessageReported", ErrorReporter.LastMessageReported);
			AssertNull("Shape deletion should have been persisted", Factory.CreateNewFactory().Load<BMNCNShape>(shape.PK));
		}

		public void TestWhenShapeDeletedInOneFactory_AndShapeVisualisedInAnother_AndShapeTypeAccessed_ShouldNotDieHorribly_ShouldDieGracefully()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape = NetworkTestCase.CreateShape(diagram);
			Factory.Save();

			using (var diagramForm = new NetworkDiagramForm(diagram))
			{
				diagramForm.Show();
				Application.DoEvents();

				var entity = NetworkGUITestCase.FindShapeOnForm(shape, diagramForm);

				CombineAssertions("PRE: nothing has been deleted yet", () =>
				{
					AssertEquals(false, shape.IsDeleted);
					AssertEquals(false, entity.IsDeleted);
				});

				OpenDiagramInAnotherFactoryAndDeleteAShape(diagram, shape);

				CombineAssertions("Our deleted shapes don't know they're already deleted", () =>
				{
					AssertEquals(false, shape.IsDeleted);
					AssertEquals(false, entity.IsDeleted);
				});

				AssertEquals(true, entity.Supports(NetworkActions.Hide));

				CombineAssertions("Our deleted shapes are still unaware they're already deleted", () =>
				{
					AssertEquals(false, shape.IsDeleted);
					AssertEquals(false, entity.IsDeleted);
				});

				diagramForm.FireSaveButton();

				CombineAssertions("Our shapes should now be aware they're deleted", () =>
				{
					AssertEquals(true, shape.IsDeleted);
					AssertEquals(true, entity.IsDeleted);
				});

				AssertNoExceptionThrown("Fiddling with a deleted shape should cause no exceptions, but instead...", () =>
				{
					AssertEquals(false, entity.Supports(NetworkActions.Hide));
				});
			}

			AssertNullOrEmpty("ErrorReporter.LastMessageReported", ErrorReporter.LastMessageReported);
			AssertNull("Shape deletion should have been persisted", Factory.CreateNewFactory().Load<BMNCNShape>(shape.PK));
		}
		#endregion

		#region Misc

		public void TestEntityDetailsBottomMargin_ShouldMatchConstantValue()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				NetworkGUITestCase.DoEventsThoroughly();

				var control = NetworkGUITestCase.FindNetworkUserControl(form).MainDiagramControl;

				var details = control.FindChildren<EntityDetails>().Single();
				var margin = details.Margin;

				AssertEquals(
					"There's no simple way to make the margin use the value defined in Constants (because it's set in XAML), so this test ensures that if the margin is changed, the value in Constants must also be changed.",
					margin.Bottom, Constants.EntityDetailsBottomMargin);
			}
		}

		#endregion

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var diagram = NetworkTestCase.CreateDiagram(jobHeader);
			diagram.BNS_JobType = "ORG";

			Factory.Save();

			return new NetworkDiagramForm(diagram);
		}

		protected override void SetUp()
		{
			base.SetUp();
			disableAsync = VisualBoardsTestCase.DisableAsyncBehaviour();
			BMSTestHelper.EnableBMSInRegistry();
		}

		IDisposable disableAsync;

		protected override void TearDown()
		{
			disableAsync.Dispose();
			base.TearDown();
		}

		#endregion
	}
}
