using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.GUI.DocumentMenu.DocDataPlugIn.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Macro;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.ReflectiveFieldMap.Testing
{
	sealed class MapTreePresentationManagerTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestShouldEscapeAllSpecialCharacters()
		{
			using (var manager = new MapTreePresentationManager())
			{
				manager.ShowEditField = true;
				manager.InitialText = @"""<Z0_Description>"" == ""\\\\abc""";

				var businessObject = Factory.New<DummyBusinessObject>();
				businessObject.Z0_Description = @"\\abc";

				manager.ParentTypes = new[] { typeof(DummyBusinessObject) };
				manager.ParentBusinessObjects = new BusinessObject[] { businessObject };
				manager.MacroSelected += manager_MacroSelected;
				manager.ShowPresentationManagerForm(true, true);
				manager.form.buttonEvaluateMacro.PerformClick();

				AssertEquals(@"""\\abc"" == ""\\abc""", manager.form.textBoxEvaluationResult.Text);
			}
		}

		public void TestGetDocumentDataContext_OverrideType()
		{
			using (var manager = new MapTreePresentationManager())
			{
				var consolProxy = Factory.New<DummyConsolBizOForMapTreePresentationManagerTest>();
				var consol = Factory.New<DummyConsolBusinessObject>();
				consolProxy.Set(consol);

				AssertType
				(
					"Precondition: DocumentSupporter return consol document supporter",
					typeof(DummyConsolBusinessObjectDocumentSupporter),
					((IDocumentSupportable)consolProxy).DocumentSupporter
				);

				AssertEquals
				(
					"GIVEN DummyConsolBizOForMapTreePresentationManagerTest type is different from DocumentSupporter provides WHEN GetDocumentDataContext THEN should return DummyConsolBusinessObject",
					".DummyConsolBusinessObject",
					manager.GetDocumentDataContext(consolProxy)
				);
			}
		}

		[TestExcludeBusinessObjectsAllHaveTestCasesAttribute]
		public class DummyConsolBizOForMapTreePresentationManagerTest : DummyEnterpriseBusinessObject, IDocumentSupportable, IDocumentSupportableOverrideType
		{
			DummyConsolBusinessObjectDocumentSupporter consolDocumentSupporter;
			DummyConsolBusinessObject consol;

			public DummyConsolBizOForMapTreePresentationManagerTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public void Set(DummyConsolBusinessObject consol)
			{
				consolDocumentSupporter = new DummyConsolBusinessObjectDocumentSupporter(consol);
				this.consol = consol;
			}

			DocumentSupporter IDocumentSupportable.DocumentSupporter => consolDocumentSupporter;

			string IDocumentSupportable.TableName => "DummyTableName";

			public Type DocumentSupportableType => consol.GetType();
		}

		public void TestGetDocumentDataContext()
		{
			using (var manager = new MapTreePresentationManager())
			{
				var shipment = Factory.New<Forwarding.IForwardingShipment>();
				AssertEquals(".ForwardingShipment", manager.GetDocumentDataContext((BusinessObject)shipment));

				var dummyBO = Factory.New<DummyBusinessObject>();
				AssertNullOrEmpty(manager.GetDocumentDataContext(dummyBO));

				AssertNullOrEmpty(manager.GetDocumentDataContext(null));
			}
		}

		public void TestShowPresentationManagerFormForDeveloperInfo()
		{
			var dummyConsol = Factory.New<DummyConsolBusinessObject>();
			dummyConsol.Z0_Code = "DC1";
			dummyConsol.Factory.Save();
			using (var manager = new MapTreePresentationManager())
			{
				using (var form = new DocDataPlugInTest.ZFormForTesting(dummyConsol))
				{
					form.ActiveControl = form.TextBox;
					form.PlugIns.Add(ControllerIDs.DocDataPlugIn);
					form.Show();

					manager.ShowPresentationManagerForm((BusinessObject)form.DataSource, form.ActiveControl);
					AssertEquals("Z0_Description (ZString)", manager.form.mapTreeUserControl.SelectedTreeNode.Text);
				}

				using (var form = new DocDataPlugInTest.ZFormForTesting(dummyConsol))
				{
					var grid = form.Grid;
					grid.BindTo = "Shipments";
					form.PlugIns.Add(ControllerIDs.DocDataPlugIn);
					form.Show();

					grid.BeginEdit(grid.TableStyles[0].GridColumnStyles[0], 0);
					manager.ShowPresentationManagerForm((BusinessObject)form.DataSource, form.ActiveControl);
					AssertEquals("Z0_Description (ZString)", manager.form.mapTreeUserControl.SelectedTreeNode.Text);
				}
			}
		}

		public void TestGetUserSelectionMacro()
		{
			using (var manager = new MapTreePresentationManager())
			{
				manager.ParentTypes = new Type[] { typeof(DummyBusinessObject) };
				var result = manager.GetUserSelectionMacro();
				AssertEquals(typeof(MapTreeForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				var form = ZFormModaliser.LastFormShownDialogForTest as MapTreeForm;
				AssertNotNull(form.Wrapper);
				AssertEquals(typeof(DummyBusinessObject), form.Wrapper.DocDataProviderReflectors[0].DocDataProviderType);
			}
		}

		public void TestShowPresentationManagerForm()
		{
			using (var manager = new MapTreePresentationManager())
			{
				var businessObject = Factory.New<DummyBusinessObject>();
				manager.ParentTypes = new[] { typeof(DummyBusinessObject) };
				manager.ParentBusinessObjects = new BusinessObject[] { businessObject };
				manager.MacroSelected += manager_MacroSelected;
				manager.ShowPresentationManagerForm();
				manager.form.mapTreeUserControl.mapTreeView.SelectedNode = manager.form.mapTreeUserControl.mapTreeView.Nodes[0];
				string expectedMacro = manager.form.mapTreeUserControl.SelectedMember.GetMacro();
				manager.form.actionButton.PerformClick();
				AssertEquals(expectedMacro, selectedMacro);
				AssertEquals(businessObject, manager.form.parentBusinessObjects[0]);
			}
		}

		public void TestMacroMaxLength()
		{
			using (var manager = new MapTreePresentationManager())
			{
				manager.ParentTypes = new[] { typeof(DummyBusinessObject) };
				manager.MacroSelected += manager_MacroSelected;

				selectedMacro = "";
				manager.MacroMaxLength = 256;
				manager.ShowPresentationManagerForm();
				manager.form.mapTreeUserControl.mapTreeView.SelectedNode = manager.form.mapTreeUserControl.mapTreeView.Nodes[0];
				string expectedMacro = manager.form.mapTreeUserControl.SelectedMember.GetMacro();
				manager.form.actionButton.PerformClick();
				AssertEquals(expectedMacro, selectedMacro);
				Assert(selectedMacro.Length > 12);

				selectedMacro = "";
				manager.MacroMaxLength = 12;
				manager.ShowPresentationManagerForm();
				manager.form.mapTreeUserControl.mapTreeView.SelectedNode = manager.form.mapTreeUserControl.mapTreeView.Nodes[0];
				manager.form.actionButton.PerformClick();
				AssertEquals("", selectedMacro);
			}
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestDataFieldsOnlyAndMacroFieldsOnly()
		{
			using (var manager = new MapTreePresentationManager())
			{
				manager.ParentTypes = new Type[] { typeof(DummyBusinessObject) };
				manager.MacroSelected += new MacroSelectedEventHandler(manager_MacroSelected);
				manager.DataFieldsOnly = true;
				manager.MacroFieldsOnly = true;
				manager.ShowPresentationManagerForm();
				Assert("Should only show properties tab", manager.form.dataPropertiesTab.TabVisible);
				Assert("Should only show properties tab", !manager.form.macrosTab.TabVisible);
				manager.form.Close();
				manager.form = null;
				manager.DataFieldsOnly = false;
				manager.MacroFieldsOnly = true;
				manager.ShowPresentationManagerForm();
				Assert("Should only show macros tab", !manager.form.dataPropertiesTab.TabVisible);
				Assert("Should only show macros tab", manager.form.macrosTab.TabVisible);
				manager.form.Close();
				manager.form = null;
				manager.DataFieldsOnly = false;
				manager.MacroFieldsOnly = false;
				manager.ShowPresentationManagerForm();
				Assert("Should show all tabs", manager.form.dataPropertiesTab.TabVisible);
				Assert("Should show all tabs", manager.form.macrosTab.TabVisible);
				manager.form.Close();
			}
		}

		public void TestMapTreePresenterDefaultLibrariesWhenHideXmlField()
		{
			using (var manager = new MapTreePresentationManager())
			{
				manager.ParentTypes = new Type[] { typeof(DummyBusinessObject) };
				manager.ShowXmlFields = false;
				manager.ShowPresentationManagerForm();
				Assert("MapTreePresenter should be initialized with StandardLibrary by default", manager.Libraries.Any(m => m is CargoWiseOneStandardLibrary));
			}
		}

		public void TestMapTreePresenterDefaultLibrariesWhenShowXmlField()
		{
			using (var manager = new MapTreePresentationManager())
			{
				manager.ParentTypes = new Type[] { typeof(DummyBusinessObject) };
				manager.ShowXmlFields = true;
				manager.ShowPresentationManagerForm();
				Assert("MapTreePresenter should be initialized with StandardLibrary by default", manager.Libraries.Any(m => m is CargoWiseOneStandardLibrary));
				Assert("MapTreePresenter should be initialized with UniversalMacroLibrary by default", manager.Libraries.Any(m => m is UniversalMacroLibrary));
			}
		}

		void manager_MacroSelected(string macro)
		{
			selectedMacro = macro;
		}
		string selectedMacro;
	}
}
