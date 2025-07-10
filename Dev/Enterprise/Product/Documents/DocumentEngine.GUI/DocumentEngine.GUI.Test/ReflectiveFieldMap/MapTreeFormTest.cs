using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.ReflectiveFieldMap;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.ValueProviders;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.DocumentEngine.ReflectiveFieldMap.MemberDescription;
using static Enterprise.DocumentEngineCore.DocumentSupport.DataContextMapList;
using DataContext = Enterprise.Core.Constants.DataContext;
using DocumentWrapperForTest = Enterprise.DocumentEngine.ReflectiveFieldMap.Testing.DocumentWrapperForTest;

namespace Enterprise.DocumentEngine.GUI.ReflectiveFieldMap.Testing
{
	[TestedType(typeof(MapTreeForm))]
	sealed class MapTreeFormTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestShouldReturnUnEscapeValueWhenUseTextMacroProcessor()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = @"A\<";
			var documentSupportable = shipment as IDocumentSupportable;
			var shipmentBO = shipment as BusinessObject;

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = documentSupportable;

			using (var form = new MapTreeForm(Wrapper, null, ".ForwardingShipment", new BusinessObject[] { shipmentBO }))
			{
				form.Show();

				form.textBoxMacro.Text = "<JS_HouseBill>";
				form.buttonEvaluateMacro.PerformClick();

				AssertEquals(@"A\<", form.textBoxEvaluationResult.Text);

				form.textBoxMacro.Text = @"<If(""<JS_HouseBill>"" == ""A\\\<"", ""\<true"",""\<false"")>";
				form.buttonEvaluateMacro.PerformClick();

				AssertEquals(@"<true", form.textBoxEvaluationResult.Text);

				form.textBoxMacro.Text = @"<SubString(""aaaa\<bbbb"",2,5)>";
				form.buttonEvaluateMacro.PerformClick();

				AssertEquals(@"aa<bb", form.textBoxEvaluationResult.Text);
			}

			shipment.JS_HouseBill = @"\\AB";
			using (var form = new MapTreeForm(Wrapper, null, ".ForwardingShipment", new BusinessObject[] { shipmentBO }, shouldEscapeAllSpecialCharacters: true))
			{
				form.Show();

				form.textBoxMacro.Text = @"""<JS_HouseBill>"" == ""\\\\AB""";
				form.buttonEvaluateMacro.PerformClick();

				AssertEquals(@"""\\AB"" == ""\\AB""", form.textBoxEvaluationResult.Text);
			}
		}

		[RequiresSTA]
		public void TestEditMacroPanelResize()
		{
			using (var form = new MapTreeForm(Wrapper))
			{
				form.Show();
				form.Height = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(150);
				form.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(500);
				Assert($"Should be within 1% of minimum height: {CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(171)} vs {form.editMacroPanel.Height}",
					Math.Abs(1.0 - (CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(171) / (float)form.editMacroPanel.Height)) < 0.01);
				Assert($"Should be within 1% of minimum width: {CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(750)} vs {form.editMacroPanel.Width}",
					Math.Abs(1.0 - (CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(750) / (float)form.editMacroPanel.Width)) < 0.01);

				AssertEquals("Should be Bottom", System.Windows.Forms.AnchorStyles.Bottom, form.buttonEvaluateMacro.Anchor);

				AssertEquals("Should not contains Top", System.Windows.Forms.AnchorStyles.Bottom
						| System.Windows.Forms.AnchorStyles.Left
						| System.Windows.Forms.AnchorStyles.Right, form.textBoxEvaluationResult.Anchor);
			}
		}

		public void TestFormCaption()
		{
			foreach (var value in new[] { true, false })
			{
				using (var form = new MapTreeForm(Wrapper))
				{
					form.UseMcrEvaluator = value;
					form.Show();
					var prefix = value ? "MCR " : string.Empty;
					AssertStartsWith("Caption of MapTreeForm should reflect whether it is using MCR engine or not", $"{prefix}Data Field Map", form.Text);
				}
			}
		}

		[RequiresSTA]
		public void TestEvaluationResult()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "S00008888";
			var documentSupportable = shipment as IDocumentSupportable;

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = documentSupportable;

			using (var form = new MapTreeForm(Wrapper, documentSupportable.DocumentSupporter, ".ForwardingShipment"))
			{
				form.Show();

				form.textBoxMacro.Text = "<JS_HouseBill>";
				form.buttonEvaluateMacro.PerformClick();

				AssertEquals("S00008888", form.textBoxEvaluationResult.Text);
			}
		}

		class DummyXmlObject
		{
			public ZString StringProp { get; set; }

			public List<ZString> StringArrayProp { get; set; }
		}

		[RequiresSTA]
		public void TestEvaluationResultOnMCR()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "S00008888";

			using (var form = new MapTreeForm(GetWrapper(new[] { shipment.GetType() }, null, null, typeof(DummyXmlObject)), parentBusinessObjects: new BusinessObject[] { (BusinessObject)shipment }) {  UseMcrEvaluator = true, })
			{
				form.xmlObject = new DummyXmlObject { StringProp = "Foo", StringArrayProp = new List<ZString> { "Bar", } };
				form.Show();

				form.textBoxMacro.Text = "JS_HouseBill + \" - \" + @UXML.StringProp + \" - \" + @UXML.StringArrayProp[0]";
				form.buttonEvaluateMacro.PerformClick();

				AssertEquals("S00008888 - Foo - Bar", form.textBoxEvaluationResult.Text);
			}
		}

		public void TestButtonEvaluateJob_Click() => CombineAssertions(() =>
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "S00008888";

			Factory.Save();

			using var form = new MapTreeForm(GetWrapper([shipment.GetType()], null, null, typeof(DummyXmlObject)), parentBusinessObjects: new BusinessObject[] { Factory.GetNull(shipment.GetType()) }) { UseMcrEvaluator = true, };
			form.Show();
			Application.DoEvents();

			form.textBoxMacro.Text = "\"<JS_HouseBill>\" == \"S00008888\"";
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormClosing(dialog =>
			{
				var popup = (EmbeddedModulePopup)dialog;
				popup.Module_ForTest.PerformSearch_ForTest();
				((IFindBoxPopup)popup).SelectRowByPK(shipment.PK);
				var selectedElements = popup.Module_ForTest.DisplayGrid.SelectedElements;
				typeof(EmbeddedModulePopup).GetMethod("OnSelected", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(popup, new object[] { selectedElements });
				AssertEquals("Selected count", 1, selectedElements.Length);
				AssertEquals("Module Selection", "JobShipmentModule", popup.Module_ForTest.GetFormattedName());
			});
			form.buttonEvaluateJob.PerformClick();
			AssertEquals("EvaluationResult", "True", form.textBoxEvaluationResult.Text);
		});

		public void TestButtonEvaluateJob_Caption()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			using var form = new MapTreeForm(GetWrapper([shipment.GetType()], null, null, typeof(DummyXmlObject)), parentBusinessObjects: new BusinessObject[] { Factory.GetNull(shipment.GetType()) }) { UseMcrEvaluator = true, };
			form.Show();

			AssertEquals("Evaluate Job", form.buttonEvaluateJob.CaptionResourceString.Caption);
		}

		public void TestButtonEvaluateJob_Visibility() => CombineAssertions(() =>
		{
			AssertButtonEvaluateJobVisibility(useMcrEvaluator: true, expectedVisible: true);
			AssertButtonEvaluateJobVisibility(useMcrEvaluator: false, expectedVisible: false);

			return;

			void AssertButtonEvaluateJobVisibility(bool useMcrEvaluator, bool expectedVisible)
			{
				var shipment = Factory.New<Forwarding.IForwardingShipment>();
				using var form = new MapTreeForm(GetWrapper([shipment.GetType()], null, null, typeof(DummyXmlObject)), parentBusinessObjects: new BusinessObject[] { Factory.GetNull(shipment.GetType()) }) { UseMcrEvaluator = useMcrEvaluator, };
				form.Show();
				Application.DoEvents();

				AssertEquals($"UseMcrEvaluator {useMcrEvaluator}", expectedVisible, form.buttonEvaluateJob.Visible);
			}
		});

		public void TestEvaluateButtonLocations() => CombineAssertions(() =>
		{
			AssertEvaluateButtonsLocation(useMcrEvaluator: true,
				expectedButtonEvaluateJobLocation: form => ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(form.buttonAddMacro.Location.X) + 100, 83),
				expectedButtonEvaluateMacroLocation: form => ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(form.buttonAddMacro.Location.X) - 100, 83));
			AssertEvaluateButtonsLocation(useMcrEvaluator: false,
				expectedButtonEvaluateJobLocation: _ => ControlDpiScalingHelper.NewScaledPoint(372, 83),
				expectedButtonEvaluateMacroLocation: form => ControlDpiScalingHelper.NewScaledPoint(323, 83));

			return;

			void AssertEvaluateButtonsLocation(bool useMcrEvaluator, Func<MapTreeForm, Point> expectedButtonEvaluateJobLocation, Func<MapTreeForm, Point> expectedButtonEvaluateMacroLocation)
			{
				var shipment = Factory.New<Forwarding.IForwardingShipment>();
				using var form = new MapTreeForm(GetWrapper([shipment.GetType()], null, null, typeof(DummyXmlObject)), parentBusinessObjects: new BusinessObject[] { Factory.GetNull(shipment.GetType()) }) { UseMcrEvaluator = useMcrEvaluator, };
				form.Show();
				Application.DoEvents();

				AssertEquals($"UseMcrEvaluator {useMcrEvaluator}: buttonEvaluateJob", expectedButtonEvaluateJobLocation(form), form.buttonEvaluateJob.Location);
				AssertEquals($"UseMcrEvaluator {useMcrEvaluator}: buttonEvaluateMacro", expectedButtonEvaluateMacroLocation(form), form.buttonEvaluateMacro.Location);
			}
		});

		[RequiresSTA]
		public void TestEvaluationResultOfProxyModelOnMCR()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "S00008888";

			var dummy = Factory.New<DummyWithWorkflow>();
			var task = dummy.WorkflowItems.Triggers.AddNew();
			var dataModel = TriggerConditionEvaluator.GetConditionsDataContext(new TriggerDataModel(Lazy.Create(() => task.P9_ActualDate)), new LogEventDataModel(new ExampleLog(task)), (BusinessObject)shipment, () => ((BusinessObject)shipment, task.WorkflowDescriptor));
			var proxyModel = WorkflowMacroDataContextManager.GetProxyModel(dataModel);

			using (var form = new MapTreeForm(GetWrapper(new[] { proxyModel.GetType() }, null, null, typeof(DummyXmlObject)), parentBusinessObjects: new object[] { proxyModel }) { UseMcrEvaluator = true, })
			{
				form.Show();

				form.textBoxMacro.Text = "Source.JS_HouseBill";
				form.buttonEvaluateMacro.PerformClick();

				AssertEquals(shipment.JS_HouseBill, form.textBoxEvaluationResult.Text);
			}
		}

		[RequiresSTA]
		public void TestNodeSelectionOnMCR()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "S00008888";

			using (var form = new MapTreeForm(GetWrapper(new[] { shipment.GetType() }, null, null, typeof(DummyXmlObject), false, 0), parentBusinessObjects: new BusinessObject[] { (BusinessObject)shipment }) { UseMcrEvaluator = true, ShowEditor = true, })
			{
				form.Show();

				form.mapTreeUserControl.mapTreeView.SelectedNode = form.mapTreeUserControl.mapTreeView.Nodes.Find("JS_HouseBill", false).FirstOrDefault() as MapTreeNode;
				form.NodeDoubleClicked(form.mapTreeUserControl.SelectedTreeNode);
				AssertEquals("JS_HouseBill", form.textBoxMacro.Text);

				form.textBoxMacro.Text = string.Empty;

				form.tabControl.SelectedTab = form.xmlTab;

				AssertNotNull(form.xmlTreeUserControl.mapTreeView.Nodes.Find("StringArrayProp", false).FirstOrDefault());
				AssertNotNull(form.xmlTreeUserControl.mapTreeView.Nodes.Find("StringProp", false).FirstOrDefault());

				form.xmlTreeUserControl.mapTreeView.SelectedNode = form.xmlTreeUserControl.mapTreeView.Nodes.Find("StringArrayProp", false).FirstOrDefault() as MapTreeNode;
				form.NodeDoubleClicked(form.xmlTreeUserControl.SelectedTreeNode);
				AssertEquals("@UXML.StringArrayProp[0]", form.textBoxMacro.Text);

				form.textBoxMacro.Text = string.Empty;

				form.xmlTreeUserControl.mapTreeView.SelectedNode = form.xmlTreeUserControl.mapTreeView.Nodes.Find("StringProp", false).FirstOrDefault() as MapTreeNode;
				form.NodeDoubleClicked(form.xmlTreeUserControl.SelectedTreeNode);
				AssertEquals("@UXML.StringProp", form.textBoxMacro.Text);
			}
		}

		[RequiresSTA]
		public void TestNodeSelectionOfProxyModelOnMCR()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "S00008888";

			var dummy = Factory.New<DummyWithWorkflow>();
			var task = dummy.WorkflowItems.Triggers.AddNew();
			var dataModel = TriggerConditionEvaluator.GetConditionsDataContext(new TriggerDataModel(Lazy.Create(() => task.P9_ActualDate)), new LogEventDataModel(new ExampleLog(task)), (BusinessObject)shipment, () => ((BusinessObject)shipment, task.WorkflowDescriptor));
			var proxyModel = WorkflowMacroDataContextManager.GetProxyModel(dataModel);

			using (var form = new MapTreeForm(GetWrapper(new[] { proxyModel.GetType() }, null, null, typeof(DummyXmlObject), false, 0), parentBusinessObjects: new object[] { proxyModel }) { UseMcrEvaluator = true, ShowEditor = true, })
			{
				form.Show();

				form.mapTreeUserControl.mapTreeView.SelectedNode = form.mapTreeUserControl.mapTreeView.Nodes.Find("Source", false).FirstOrDefault() as MapTreeNode;
				form.NodeDoubleClicked(form.mapTreeUserControl.SelectedTreeNode);
				AssertEquals("Source", form.textBoxMacro.Text);
			}
		}

		[RequiresSTA]
		public void TestEvaluationWithDateTimeResult()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_E_ARV = new DateTime(2024, 1, 1, 10, 10, 10);
			shipment.JS_E_DEP = new DateTime(2024, 1, 1, 10, 10, 10, 200);
			var dummy = Factory.New<DummyWithWorkflow>();
			var task = dummy.WorkflowItems.Triggers.AddNew();

			var dataModel = TriggerConditionEvaluator.GetConditionsDataContext(new TriggerDataModel(Lazy.Create(() => task.P9_ActualDate)), new LogEventDataModel(new ExampleLog(task)), (BusinessObject)shipment, () => ((BusinessObject)shipment, task.WorkflowDescriptor));
			var proxyModel = WorkflowMacroDataContextManager.GetProxyModel(dataModel);

			using (var form = new MapTreeForm(GetWrapper(new[] { proxyModel.GetType() }, null, null, typeof(DummyXmlObject)), parentBusinessObjects: new object[] { proxyModel }) { UseMcrEvaluator = true, })
			{
				form.Show();

				form.textBoxMacro.Text = "Source.JS_E_ARV";
				form.buttonEvaluateMacro.PerformClick();

				AssertEquals("Monday, 01 January 2024 10:10:10", form.textBoxEvaluationResult.Text);

				form.textBoxMacro.Text = "Source.JS_E_DEP";
				form.buttonEvaluateMacro.PerformClick();

				AssertEquals("Monday, 01 January 2024 10:10:10.200", form.textBoxEvaluationResult.Text);
			}
		}

		[RequiresSTA]
		public void TestEvaluationWithDateTimeOffsetResult()
		{
			var dummyWithWorkflow = Factory.New<DummyWithWorkflow>();
			var dummyTask = dummyWithWorkflow.WorkflowItems.Triggers.AddNew();
			dummyTask.P9_ActualDateOffset = new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.FromHours(11));

			var dummyTask2 = dummyWithWorkflow.WorkflowItems.Triggers.AddNew();
			dummyTask2.P9_ActualDateOffset = new DateTimeOffset(2024, 1, 1, 10, 20, 20, 200, TimeSpan.FromHours(12));

			var dummy = Factory.New<DummyWithWorkflow>();
			var task = dummy.WorkflowItems.Triggers.AddNew();

			var dataModel = TriggerConditionEvaluator.GetConditionsDataContext(new TriggerDataModel(Lazy.Create(() => task.P9_ActualDate)), new LogEventDataModel(new ExampleLog(task)), dummyTask, () => (dummyTask, task.WorkflowDescriptor));
			var proxyModel = WorkflowMacroDataContextManager.GetProxyModel(dataModel);

			using (var form = new MapTreeForm(GetWrapper(new[] { proxyModel.GetType() }, null, null, typeof(DummyXmlObject)), parentBusinessObjects: new object[] { proxyModel }) { UseMcrEvaluator = true, })
			{
				form.Show();

				form.textBoxMacro.Text = "Source.P9_ActualDateOffset";
				form.buttonEvaluateMacro.PerformClick();

				AssertEquals("Monday, 01 January 2024 10:00:00 +11:00", form.textBoxEvaluationResult.Text);
			}

			var dataModel2 = TriggerConditionEvaluator.GetConditionsDataContext(new TriggerDataModel(Lazy.Create(() => task.P9_ActualDate)), new LogEventDataModel(new ExampleLog(task)), dummyTask2, () => (dummyTask2, task.WorkflowDescriptor));
			var proxyModel2 = WorkflowMacroDataContextManager.GetProxyModel(dataModel2);
			using (var form = new MapTreeForm(GetWrapper(new[] { proxyModel2.GetType() }, null, null, typeof(DummyXmlObject)), parentBusinessObjects: new object[] { proxyModel2 }) { UseMcrEvaluator = true, })
			{
				form.Show();

				form.textBoxMacro.Text = "Source.P9_ActualDateOffset";
				form.buttonEvaluateMacro.PerformClick();

				AssertEquals("Monday, 01 January 2024 10:20:20.2000000 +12:00", form.textBoxEvaluationResult.Text);
			}
		}

		public void TestEvaluationWithEmptyResult()
		{
			using (var form = new MapTreeForm(Wrapper))
			{
				form.Show();
				form.textBoxMacro.Text = "<JS_HouseBill>";
				form.buttonEvaluateMacro.PerformClick();

				AssertNullOrEmpty(form.textBoxEvaluationResult.Text);
			}
		}

		public void TestEvaluationResultByUsingTextMacroProcessor()
		{
			using (var manager = new MapTreePresentationManager())
			{
				var businessObject = new BusinessObjectFactory().New<DummyBusinessObject>();
				businessObject.Z0_NVarChar = "TestValue";
				manager.ParentTypes = new[] { typeof(DummyBusinessObject) };
				manager.ParentBusinessObjects = new BusinessObject[] { businessObject };
				manager.ShowPresentationManagerForm();
				manager.form.mapTreeUserControl.mapTreeView.SelectedNode = manager.form.mapTreeUserControl.mapTreeView.Nodes[0];
				manager.form.actionButton.PerformClick();
				manager.form.textBoxMacro.Text = "<Z0_NVarChar>";
				manager.form.buttonEvaluateMacro.PerformClick();
				AssertEquals("TestValue", manager.form.textBoxEvaluationResult.Text);
			}
		}

		public void TestEvaluationVariable()
		{
			using (var manager = new MapTreePresentationManager())
			{
				var variables = new Dictionary<string, object>
				{
					{ "env", new MasterFiles.Business.Macros.Environment() }
				};
				var scope = new MacroScope();
				scope.SetVariable(variables.Keys.First(), variables.Values.First());

				manager.VariableParentTypes = variables.Select(variable => variable.Value.GetType()).ToArray();
				manager.VariableNames = variables.Keys.ToArray();
				manager.ParentTypes = new[] { typeof(DummyBusinessObject) };
				manager.ParentBusinessObjects = new Object[] { scope };
				manager.ShowPresentationManagerForm();
				manager.form.tabControl.SelectedTab = manager.form.variablesTab;
				manager.form.textBoxMacro.Text = "@env.LocalCurrency";
				manager.form.UseMcrEvaluator = true;
				manager.form.buttonEvaluateMacro_Click();
				AssertEquals("AUD", manager.form.textBoxEvaluationResult.Text);
			}
		}

		[RequiresSTA]
		public void TestEvaluationResultByUsingTextMacroProcessor_DataSourcePrefix()
		{
			using (var manager = new MapTreePresentationManager())
			{
				SetupMapTreePresentationManager(manager);

				CombineAssertions(() =>
				{
					var textBoxMacro = manager.form.textBoxMacro;
					var buttonEvaluateMacro = manager.form.buttonEvaluateMacro;
					var textBoxEvaluationResult = manager.form.textBoxEvaluationResult;

					textBoxMacro.Text = "<JS_HouseBill>";
					manager.form.buttonEvaluateMacro.PerformClick();
					AssertEquals("S00008888", textBoxEvaluationResult.Text);

					textBoxMacro.Text = "<JE_HouseBill>";
					manager.form.buttonEvaluateMacro.PerformClick();
					AssertEquals("B0001", textBoxEvaluationResult.Text);

					textBoxMacro.Text = "<XXX>";
					manager.form.buttonEvaluateMacro.PerformClick();
					AssertEquals("", textBoxEvaluationResult.Text);

					textBoxMacro.Text = "<_DataSource.JobDeclaration.JE_HouseBill>";
					buttonEvaluateMacro.PerformClick();
					AssertEquals("B0001", textBoxEvaluationResult.Text);

					textBoxMacro.Text = "<_DataSource.JobDeclaration.XXX>";
					buttonEvaluateMacro.PerformClick();
					AssertEquals("", textBoxEvaluationResult.Text);
				});
			}
		}

		public void TestSelectedSingleMacro()
		{
			using (var manager = new MapTreePresentationManager())
			{
				SetupMapTreePresentationManager(manager);

				CombineAssertions(() =>
				{
					var mapForm = manager.form;
					var textBoxMacro = mapForm.textBoxMacro;
					var mapTreeView = mapForm.mapTreeUserControl.mapTreeView;

					var node1 = mapTreeView.Nodes[0] as MapTreeNode;
					mapTreeView.SelectedNode = node1;
					mapForm.NodeDoubleClicked(node1);
					AssertEquals("", textBoxMacro.Text);

					var node2 = mapTreeView.Nodes[0].Nodes.Find("JS_HouseBill", false).FirstOrDefault() as MapTreeNode;
					mapTreeView.SelectedNode = node2;
					mapForm.NodeDoubleClicked(node2);
					AssertEquals("<JS_HouseBill>", textBoxMacro.Text);

					var node3 = mapTreeView.Nodes[1].Nodes.Find("JE_HouseBill", false).FirstOrDefault() as MapTreeNode;
					mapTreeView.SelectedNode = node3;
					mapForm.NodeDoubleClicked(node3);
					AssertEquals("<JS_HouseBill><_DataSource.JobDeclaration.JE_HouseBill>", textBoxMacro.Text);
				});
			}
		}

		void SetupMapTreePresentationManager(MapTreePresentationManager manager)
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "S00008888";
			var shipmentBO = shipment as BusinessObject;
			var declaration = Factory.New<Customs.IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_HouseBill = "B0001";
			var declarationBO = declaration as BusinessObject;
			manager.ParentTypes = new[] { shipmentBO.GetType(), declarationBO.GetType() };
			manager.ParentBusinessObjects = new BusinessObject[] { shipmentBO, declarationBO };
			manager.ShowEditField = true;
			manager.ShowPresentationManagerForm();
		}

		public void TestNodeSelected()
		{
			using (var manager = new MapTreePresentationManager())
			{
				SetupMapTreePresentationManager(manager);

				CombineAssertions(() =>
				{
					var mapForm = manager.form;
					var wrapper = mapForm.Wrapper;
					var mapTreeUserControl = mapForm.mapTreeUserControl;
					var mapTreeView = mapTreeUserControl.mapTreeView;
					var filterTextBox = mapTreeUserControl.mapFilterTreeView.FilterTextBox;

					const string shipmentTopLevelDataSourceInformation = @"Data Source Type: ForwardingShipment
Namespace: Enterprise.Freight.Forwarding.Business";
					const string declarationTopLevelDataSourceInformation = @"Data Source Type: JobDeclaration
Namespace: Enterprise.Customs.AU.Declaration.Business";

					var node1 = mapTreeView.Nodes[0] as MapTreeNode;
					mapTreeView.SelectedNode = node1;
					mapForm.NodeDoubleClicked(node1);
					AssertEquals("SelectedDocDataProviderReflector", 0, wrapper.SelectedDocDataProviderReflector);
					AssertEquals("TopLevelDataSourceInformation", shipmentTopLevelDataSourceInformation, wrapper.TopLevelDataSourceInformation);

					var node2 = mapTreeView.Nodes[1].Nodes.Find("JE_HouseBill", false).FirstOrDefault() as MapTreeNode;
					mapTreeView.SelectedNode = node2;
					mapForm.NodeDoubleClicked(node2);
					AssertEquals("SelectedDocDataProviderReflector: JobDeclaration", 1, wrapper.SelectedDocDataProviderReflector);
					AssertEquals("TopLevelDataSourceInformation: JobDeclaration", declarationTopLevelDataSourceInformation, wrapper.TopLevelDataSourceInformation);

					var node3 = mapTreeView.Nodes[0].Nodes.Find("JS_HouseBill", false).FirstOrDefault() as MapTreeNode;
					mapTreeView.SelectedNode = node3;
					mapForm.NodeDoubleClicked(node3);
					AssertEquals("SelectedDocDataProviderReflector: Shipment", 0, wrapper.SelectedDocDataProviderReflector);
					AssertEquals("TopLevelDataSourceInformation", shipmentTopLevelDataSourceInformation, wrapper.TopLevelDataSourceInformation);

					filterTextBox.Text = "JE_House";
					KeySender.SendKeyDownToProcessCmdKey(filterTextBox, Keys.Enter);
					AssertEquals("Tree node text after filtering", 1, mapTreeView.Nodes.Count);
					var node4 = mapTreeView.Nodes[0].Nodes.Find("JE_HouseBill", false).FirstOrDefault() as MapTreeNode;
					mapTreeView.SelectedNode = node4;
					mapForm.NodeDoubleClicked(node4);
					AssertEquals("SelectedDocDataProviderReflector on filtertreeView: JobDeclaration", 1, wrapper.SelectedDocDataProviderReflector);
					AssertEquals("TopLevelDataSourceInformation on filtertreeView: JobDeclaration", declarationTopLevelDataSourceInformation, wrapper.TopLevelDataSourceInformation);
				});
			}
		}

		DataReflectorValueProviderWrapper Wrapper
		{
			get
			{
				return GetWrapper();
			}
		}

		DataReflectorValueProviderWrapper GetWrapper(Type[] boTypes = null, string[] variableNames = null, Type[] variableParentTypes = null, Type xmlType = null, bool withTags = true, int defaultCollectionIndex = 1, IMacroLibrary[] lib = null)
		{
			DocDataProviderReflector[] reflectors = null;

			if (boTypes != null)
			{
				var filter = new DocDataReflectorFilter();
				reflectors = Array.ConvertAll(boTypes, baseType => new DocDataProviderReflector(baseType, filter, withTags ? MacroTagTypes.Document : MacroTagTypes.None) { DefaultIndex = defaultCollectionIndex, });
			}
			else
			{
				reflectors = new[] { new DocDataProviderReflector(typeof(DocumentWrapperForTest), withTags ? MacroTagTypes.Document : MacroTagTypes.None) { DefaultIndex = defaultCollectionIndex } };
			}

			var valueProvider = lib == null ? new ValueProviderMap() : (new LibraryValueProviderMap { Libraries = lib.ToImmutableArray() });
			var xmlWrapper = xmlType != null ? new DocDataProviderReflector(xmlType, new UniversalXmlDataReflectorFilter(), withTags ? MacroTagTypes.Document : MacroTagTypes.None) { DefaultIndex = defaultCollectionIndex, } : null;
			var variableReflectors = variableParentTypes != null ? Array.ConvertAll(variableParentTypes, baseType => new DocDataProviderReflector(baseType, new DocDataReflectorFilter(), withTags ? MacroTagTypes.Document : MacroTagTypes.None) { DefaultIndex = defaultCollectionIndex, }) : null;

			return new DataReflectorValueProviderWrapper(reflectors, variableNames, variableReflectors, xmlWrapper, valueProvider, DataReflectorValueProviderWrapper.Mode.Select);
		}

		public void TestDoubleClick()
		{
			var reflector = new DocDataProviderReflector(typeof(DocumentWrapperForTest));
			var valueProvider = new ValueProviderMap();
			var wrapper = new DataReflectorValueProviderWrapper(new DocDataProviderReflector[] { reflector }, valueProvider, DataReflectorValueProviderWrapper.Mode.Select);
			using (var form = new MapTreeForm(wrapper))
			{
				form.Show();
				AssertEquals("form.closeButton.Visible", true, form.closeButton.Visible);
				AssertEquals("form.selectAndCloseButton.Text", "&Select", form.actionButton.Text);

				AssertEquals("reflector.SelectedMacro", null, wrapper.SelectedMacro);
				var docDataProviderIsInNode = form.mapTreeUserControl.mapTreeView.Nodes[0] as MapTreeNode;
				form.mapTreeUserControl.mapTreeView.SelectedNode = docDataProviderIsInNode;
				form.NodeDoubleClicked(docDataProviderIsInNode);
				AssertEquals("reflector.SelectedMacro", "<DocDataProviderIsIn>", wrapper.SelectedMacro);
				var zStringIsInNode = form.mapTreeUserControl.mapTreeView.Nodes[3] as MapTreeNode;
				form.mapTreeUserControl.mapTreeView.SelectedNode = zStringIsInNode;
				form.NodeDoubleClicked(zStringIsInNode);
				AssertEquals("reflector.SelectedMacro", "<ZStringIsIn>", wrapper.SelectedMacro);
			}
		}

		public void TestFormHeading()
		{
			var reflector = new DocDataProviderReflector(typeof(IBusiness));
			var valueProvider = new ValueProviderMap();
			var wrapper = new DataReflectorValueProviderWrapper(new[] { reflector }, valueProvider, DataReflectorValueProviderWrapper.Mode.Select);
			using (var form = new MapTreeForm(wrapper))
			{
				AssertEquals("Data Field Map - Business", form.FormHeading);
			}

			reflector = new DocDataProviderReflector(typeof(DocumentWrapperForTest));
			wrapper = new DataReflectorValueProviderWrapper(new[] { reflector }, valueProvider, DataReflectorValueProviderWrapper.Mode.Select);
			using (var form = new MapTreeForm(wrapper))
			{
				AssertEquals("Data Field Map - DocumentWrapperForTest", form.FormHeading);
			}
		}

		public void TestFormHeadingForProxyModel()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var task = dummy.WorkflowItems.Triggers.AddNew();
			var dataModel = TriggerConditionEvaluator.GetConditionsDataContext(new TriggerDataModel(Lazy.Create(() => task.P9_ActualDate)), new LogEventDataModel(new ExampleLog(task)), task.ParentBusinessObject, () => (task.ParentBusinessObject, task.WorkflowDescriptor));
			var reflector = new DocDataProviderReflector(WorkflowMacroDataContextManager.GetProxyModel(dataModel).GetType());
			var valueProvider = new ValueProviderMap();
			var wrapper = new DataReflectorValueProviderWrapper(new[] { reflector }, valueProvider, DataReflectorValueProviderWrapper.Mode.Select);
			using (var form = new MapTreeForm(wrapper))
			{
				AssertEquals("Data Field Map - DummyEventDataModel", form.FormHeading);
			}
		}

		public void TestAddClick()
		{
			var reflector = new DocDataProviderReflector(typeof(DocumentWrapperForTest));
			var valueProvider = new ValueProviderMap();
			var wrapper = new DataReflectorValueProviderWrapper(new[] { reflector }, valueProvider, DataReflectorValueProviderWrapper.Mode.Select);
			using (var form = new MapTreeForm(wrapper))
			{
				form.ShowEditor = true;

				form.Show();
				Application.DoEvents();

				AssertEquals("", form.textBoxMacro.Text);

				var docDataProviderIsInNode = form.mapTreeUserControl.mapTreeView.Nodes[0] as MapTreeNode;
				form.mapTreeUserControl.mapTreeView.SelectedNode = docDataProviderIsInNode;
				form.NodeDoubleClicked(docDataProviderIsInNode);
				AssertEquals("<DocDataProviderIsIn>", form.textBoxMacro.Text);

				var zStringIsInNode = form.mapTreeUserControl.mapTreeView.Nodes[3] as MapTreeNode;
				form.mapTreeUserControl.mapTreeView.SelectedNode = zStringIsInNode;
				form.NodeDoubleClicked(zStringIsInNode);
				AssertEquals("<DocDataProviderIsIn><ZStringIsIn>", form.textBoxMacro.Text);

				form.textBoxMacro.SelectionStart = 15;
				form.textBoxMacro.SelectionLength = 10;
				form.NodeDoubleClicked(zStringIsInNode);
				AssertEquals("<DocDataProvide<ZStringIsIn>ringIsIn>", form.textBoxMacro.Text);
			}
		}

		[DeveloperOnlyTest]
		public void TestCopySelectedText()
		{
			var reflector = new DocDataProviderReflector(typeof(DocumentWrapperForTest));
			var valueProvider = new ValueProviderMap();
			var wrapper = new DataReflectorValueProviderWrapper(new[] { reflector }, valueProvider, DataReflectorValueProviderWrapper.Mode.Select);
			using (var form = new MapTreeForm(wrapper))
			{
				form.ShowEditor = true;

				form.Show();
				Application.DoEvents();

				AssertEquals("", form.textBoxMacro.Text);

				var docDataProviderIsInNode = form.mapTreeUserControl.mapTreeView.Nodes[0] as MapTreeNode;
				form.mapTreeUserControl.mapTreeView.SelectedNode = docDataProviderIsInNode;
				form.NodeDoubleClicked(docDataProviderIsInNode);
				AssertEquals("<DocDataProviderIsIn>", form.textBoxMacro.Text);

				var keyEventArgs = new KeyEventArgs(Keys.C | Keys.Control);
				typeof(Control).InvokeMember("OnKeyUp", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, form, new object[] { keyEventArgs });
				Application.DoEvents();

				AssertEquals("<DocDataProviderIsIn>", SafeClipboard.GetText());

				form.textBoxMacro.SelectionStart = 4;
				form.textBoxMacro.SelectionLength = 12;

				typeof(Control).InvokeMember("OnKeyUp", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, form, new object[] { keyEventArgs });
				Application.DoEvents();
				AssertEquals("DataProvider", SafeClipboard.GetText());
			}
		}

		public void TestInCopyMode()
		{
			using (var form = new MapTreeForm(new DataReflectorValueProviderWrapper(new DocDataProviderReflector[] { new DocDataProviderReflector(new MapElement("YELALA", typeof(DocumentWrapperForTest))) }, new ValueProviderMap(), DataReflectorValueProviderWrapper.Mode.Browse)))
			{
				form.Show();
				AssertEquals("form.closeButton.Visible", true, form.closeButton.Visible);
				AssertEquals("form.selectAndCloseButton.Text", "Copy to Clipboard", form.actionButton.Text);
			}
		}

		[RequiresSTA]
		public void TestInSelectMode()
		{
			var reflector = new DocDataProviderReflector(typeof(DocumentWrapperForTest));
			var valueProvider = new ValueProviderMap();
			var wrapper = new DataReflectorValueProviderWrapper(new DocDataProviderReflector[] { reflector }, valueProvider, DataReflectorValueProviderWrapper.Mode.Select);
			using (var form = new MapTreeForm(wrapper))
			{
				form.Show();
				AssertEquals("form.closeButton.Visible", true, form.closeButton.Visible);
				AssertEquals("form.selectAndCloseButton.Text", "&Select", form.actionButton.Text);

				AssertEquals("reflector.SelectedMacro", null, wrapper.SelectedMacro);
				var docDataProviderIsInNode = form.mapTreeUserControl.mapTreeView.Nodes[0] as MapTreeNode;
				form.mapTreeUserControl.mapTreeView.SelectedNode = docDataProviderIsInNode;
				form.actionButton.PerformClick();
				AssertEquals("reflector.SelectedMacro", "<DocDataProviderIsIn>", wrapper.SelectedMacro);
			}
		}

		[RequiresSTA]
		public void TestTitleSetup()
		{
			using (var form = new MapTreeForm(new DataReflectorValueProviderWrapper(new DocDataProviderReflector[] { new DocDataProviderReflector(typeof(DocumentWrapperForTest)) }, new ValueProviderMap(), DataReflectorValueProviderWrapper.Mode.Select)))
			{
				form.Show();
				AssertEquals("form.Text", "Data Field Map - DocumentWrapperForTest", form.Text);
			}
		}

		public void TestMacroEvaluator()
		{
			var reflector = new DocDataProviderReflector(typeof(DocumentWrapperForTest));
			var valueProvider = new ValueProviderMap();
			var wrapper = new DataReflectorValueProviderWrapper(new[] { reflector }, valueProvider, DataReflectorValueProviderWrapper.Mode.Select);

			var dummy = Factory.New<DummyBODocSupportable>();
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			using (var form = new MapTreeForm(wrapper, dummy.DocumentSupporter, nameof(DataContext.UnitTest)))
			{
				form.Show();
				Assert("Evaluate button is shown", form.evaluateMacroButton.Visible);
			}

			using (var form = new MapTreeForm(wrapper, dummy.DocumentSupporter, nameof(DataContext.GenericChargeSheet)))
			{
				form.Show();
				Assert("Evaluate button is not shown", !form.evaluateMacroButton.Visible);
			}

			using (var form = new MapTreeForm(wrapper, dummy.DocumentSupporter, ".ForwardingShipment"))
			{
				form.Show();
				Assert("Evaluate button is shown", form.evaluateMacroButton.Visible);
			}
		}

		public void TestTabsShowRightPanel()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "S00008888";

			var shipmentType = new[] { shipment.GetType() };

			using (var form = new MapTreeForm(GetWrapper(shipmentType, null, shipmentType, typeof(DummyXmlObject), false, 0), parentBusinessObjects: new BusinessObject[] { (BusinessObject)shipment }) { UseMcrEvaluator = true, ShowEditor = true, })
			{
				form.Show();
				AssertNotNull(form.tabControl.SelectedTab.Controls["rightPanel"]);

				form.tabControl.SelectedTab = form.xmlTab;
				AssertNotNull(form.tabControl.SelectedTab.Controls["rightPanel"]);

				form.tabControl.SelectedTab = form.dataPropertiesTab;
				AssertNotNull(form.tabControl.SelectedTab.Controls["rightPanel"]);

				form.tabControl.SelectedTab = form.variablesTab;
				AssertNotNull(form.tabControl.SelectedTab.Controls["rightPanel"]);
			}
		}
		public void TestTreeViewDoesNotShowVariablesTabForEmptyVariableReflector()
		{
			var reflector = new DocDataProviderReflector(typeof(DocumentWrapperForTest));
			var valueProvider = new ValueProviderMap();
			var wrapper = new DataReflectorValueProviderWrapper(new[] { reflector }, valueProvider, DataReflectorValueProviderWrapper.Mode.Select);
			using (var form = new MapTreeForm(wrapper))
			{
				form.ShowEditor = true;

				form.Show();
				Application.DoEvents();

				AssertNull(form.variablesTab);
				AssertNull(form.variableTreeUserControl);
			}
		}

		[RequiresSTA]
		public void TestTreeViewPopulatesVariables()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "S00008888";

			var shipmentType = new[] { shipment.GetType() };

			using (var form = new MapTreeForm(GetWrapper(shipmentType, null, null, typeof(DummyXmlObject), false, 0), parentBusinessObjects: new BusinessObject[] { (BusinessObject)shipment }) { UseMcrEvaluator = true, ShowEditor = true, })
			{
				form.Show();

				AssertNull(form.variableTreeUserControl);
			}

			var variableType = shipmentType;

			using (var form = new MapTreeForm(GetWrapper(shipmentType, null, variableType, typeof(DummyXmlObject), false, 0), parentBusinessObjects: new BusinessObject[] { (BusinessObject)shipment }) { UseMcrEvaluator = true, ShowEditor = true, })
			{
				form.Show();

				var variableReflectors = Array.ConvertAll(variableType, baseType => new DocDataProviderReflector(baseType, new DocDataReflectorFilter(), MacroTagTypes.Document) { DefaultIndex = 1, });

				Assert(variableReflectors.Length > 0);
				Assert(form.variableTreeUserControl.mapTreeView.Nodes.Count > 0);
				AssertEquals(variableReflectors[0].Members.Count, form.variableTreeUserControl.mapTreeView.Nodes.Count);
				AssertEquals(variableReflectors[0].Members[0].GetFullPath(), form.variableTreeUserControl.mapTreeView.Nodes[0].Name);
			}

			variableType = new[] { shipment.GetType(), shipment.GetType() };

			using (var form = new MapTreeForm(GetWrapper(shipmentType, new string[] { "env" }, variableType, typeof(DummyXmlObject), false, 0), parentBusinessObjects: new BusinessObject[] { (BusinessObject)shipment }) { UseMcrEvaluator = true, ShowEditor = true, })
			{
				form.Show();

				var variableReflectors = Array.ConvertAll(variableType, baseType => new DocDataProviderReflector(baseType, new DocDataReflectorFilter(), MacroTagTypes.Document) { DefaultIndex = 1, });

				AssertEquals(variableReflectors[0].Members.Count, form.variableTreeUserControl.mapTreeView.Nodes[0].Nodes.Count);
				AssertEquals("env", form.variableTreeUserControl.mapTreeView.Nodes[0].Text);
				AssertEquals(variableReflectors[0].Members[0].GetFullPath(), form.variableTreeUserControl.mapTreeView.Nodes[0].Nodes[0].Name);
			}

			variableType = new[] { shipment.GetType(), shipment.GetType() };

			using (var form = new MapTreeForm(GetWrapper(shipmentType, new string[] { "env1", "env2" }, variableType, typeof(DummyXmlObject), false, 0), parentBusinessObjects: new BusinessObject[] { (BusinessObject)shipment }) { UseMcrEvaluator = true, ShowEditor = true, })
			{
				form.Show();

				var variableReflectors = Array.ConvertAll(variableType, baseType => new DocDataProviderReflector(baseType, new DocDataReflectorFilter(), MacroTagTypes.Document) { DefaultIndex = 1, });

				Assert(variableReflectors.Length > 0);
				Assert(form.variableTreeUserControl.mapTreeView.Nodes.Count > 0);
				AssertEquals(variableReflectors[0].Members.Count, form.variableTreeUserControl.mapTreeView.Nodes[0].Nodes.Count);
				AssertEquals("env1", form.variableTreeUserControl.mapTreeView.Nodes[0].Text);
				AssertEquals(variableReflectors[0].Members[0].GetFullPath(), form.variableTreeUserControl.mapTreeView.Nodes[0].Nodes[0].Name);
				AssertEquals(variableReflectors[1].Members.Count, form.variableTreeUserControl.mapTreeView.Nodes[1].Nodes.Count);
				AssertEquals("env2", form.variableTreeUserControl.mapTreeView.Nodes[1].Text);
				AssertEquals(variableReflectors[1].Members[0].GetFullPath(), form.variableTreeUserControl.mapTreeView.Nodes[1].Nodes[0].Name);
			}
		}

		public void TestShowMacrosInTab()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "S00008888";

			using (var form = new MapTreeForm(GetWrapper(new[] { shipment.GetType() }, null, null, typeof(DummyXmlObject), false, 0, new IMacroLibrary[] { new LibraryValueProviderMapTest.TestMacroLibrary() }), parentBusinessObjects: new BusinessObject[] { (BusinessObject)shipment }) { UseMcrEvaluator = true, ShowEditor = true, })
			{
				form.Show();
				form.tabControl.SelectedTab = form.macrosTab;
				AssertEquals("Macros Tab should only have one row", 1, form.MacroGridList.Count);
				AssertEquals("Correct Usage value", "Usage", form.MacroGridList[0].Usage);
				AssertEquals("Correct Description value", "Description", form.MacroGridList[0].Description);
			}
		}

		public void TestErrorMessageExtender()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "S00008888";
			string MyMessageExtender(string input)
			{
				return "This is a test";
			}

			using (var form = new MapTreeForm(GetWrapper(new[] { shipment.GetType() }, null, null, typeof(DummyXmlObject), false, 0, new IMacroLibrary[] { new LibraryValueProviderMapTest.TestMacroLibrary() }), parentBusinessObjects: new BusinessObject[] { (BusinessObject)shipment }, errorMessageExtender: MyMessageExtender) { UseMcrEvaluator = true, ShowEditor = true, })
			{
				form.Show();

				form.textBoxMacro.Text = "JS_HouseBills";
				form.buttonEvaluateMacro.PerformClick();

				Assert("Error message should contain my message", form.textBoxEvaluationResult.Text.Contains("This is a test"));
			}
		}

		[RequiresSTA]
		public void TestNoExceptionsWhenSaveMapTreeForm()
		{
			using (var form = new MapTreeForm(Wrapper))
			{
				form.Show();
				Application.DoEvents();
				AssertNoExceptionThrown(() => form.FireSaveButton());
			}
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public override void TestBashingForm()
		{
			base.TestBashingForm();
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public override void TestBoundListsAreNotLoadedOnAccess()
		{
			base.TestBoundListsAreNotLoadedOnAccess();
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public override void TestFormIsFullyTranslatable()
		{
			base.TestFormIsFullyTranslatable();
		}

		protected override Form GetFormToBashCore()
		{
			return new MapTreeForm(new DataReflectorValueProviderWrapper(new DocDataProviderReflector[] { new DocDataProviderReflector(typeof(DocumentWrapperForTest)) }, new ValueProviderMap(), DataReflectorValueProviderWrapper.Mode.Select));
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			return control.Name == "textBoxMacro" || control.Name == "textBoxEvaluationResult" || base.ShouldIgnoreMissingBindingMember(control);
		}
	}
}

