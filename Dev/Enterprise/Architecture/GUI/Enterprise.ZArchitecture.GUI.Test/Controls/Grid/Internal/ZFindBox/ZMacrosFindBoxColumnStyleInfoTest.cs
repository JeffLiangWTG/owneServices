using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZMacrosFindBoxColumnStyleInfoTest : TestCaseWithDummy
	{
		public void TestAntlrDefaultValues()
		{
			using var form = new ZForm(Dummy);
			var childDummy = Dummy.Collection.AddNew();
			childDummy.Z0_Code = "AB<";
			childDummy.Z0_NVarCharMax = "<Z0_Code>";
			var grid = new ZGrid();
			grid.BindTo = "Collection";
			var columnStyleInfo = new ZMacrosFindBoxColumnStyleInfo();
			columnStyleInfo.MacroType = MacroType.Antlr;
			columnStyleInfo.BindToList = "Lookups+DummyList";
			columnStyleInfo.ModuleID = ModuleIDs.NotAssigned;
			columnStyleInfo.ColumnName = "Z0_NVarCharMax";

			grid.ColumnStyles.Add(columnStyleInfo);
			grid.Dock = DockStyle.Fill;
			form.Controls.Add(grid);
			form.Show();

			var findBox = (ZMacrosFindBox)((ZMacrosFindBoxColumnStyle)grid.Columns[0].ColumnStyle).EditControl;
			CombineAssertions(() =>
			{
				AssertEquals("UseMcrEvaluator", true, findBox.UseMcrEvaluator);
				AssertEquals("DefaultCollectionIndex", 0, findBox.DefaultCollectionIndex);
				AssertEquals("OpeningMacroTag", string.Empty, findBox.OpeningMacroTag);
				AssertEquals("ClosingMacroTag", string.Empty, findBox.ClosingMacroTag);
			});
		}

		public void TestShowIndexDefaultValueForStyleInfo()
		{
			var styleInfo = new ZMacrosFindBoxColumnStyleInfo();
			AssertEquals(true, styleInfo.ShowIndex);
		}

		public void TestShowIndexDefaultValueForFindBox()
		{
			using (var form = new ZForm(Dummy))
			{
				var childDummy = Dummy.Collection.AddNew();
				childDummy.Z0_Code = "AB<";
				childDummy.Z0_NVarCharMax = "<Z0_Code>";
				var grid = new ZGrid();
				grid.BindTo = "Collection";
				var columnStyleInfo = new ZMacrosFindBoxColumnStyleInfo();
				columnStyleInfo.BindToList = "Lookups+DummyList";
				columnStyleInfo.ModuleID = ModuleIDs.NotAssigned;
				columnStyleInfo.ColumnName = "Z0_NVarCharMax";

				grid.ColumnStyles.Add(columnStyleInfo);
				grid.Dock = DockStyle.Fill;
				form.Controls.Add(grid);
				form.Show();

				var macroFindBox = (ZMacrosFindBox)((ZMacrosFindBoxColumnStyle)grid.Columns[0].ColumnStyle).EditControl;
				AssertEquals(true, macroFindBox.ShowIndex);
			}
		}

		public void TestPreviewShouldEscapeAllSpecialCharacters()
		{
			AssertPreviewShouldEscapeAllSpecialCharacters(@"\\abc", @"""<Z0_Description>"" == ""\\\\abc""", @"""\\abc"" == ""\\abc""", true);
		}

		public void TestPreviewShouldNotEscapeAllSpecialCharacters()
		{
			AssertPreviewShouldEscapeAllSpecialCharacters(@"\\abc", @"""<Z0_Description>"" == ""\\abc""", @"""\\abc"" == ""\\abc""", false);
		}

		void AssertPreviewShouldEscapeAllSpecialCharacters(string description, string macro, string expectedText, bool shouldEscapeAllSpecialCharacters)
		{
			using (var form = new ZForm(Dummy))
			{
				var childDummy = Dummy.Collection.AddNew();
				childDummy.Z0_Description = description;
				childDummy.Z0_NVarCharMax = macro;
				var grid = new ZGrid();
				grid.BindTo = "Collection";
				var columnStyleInfo = new ZMacrosFindBoxColumnStyleInfo();
				columnStyleInfo.BindToList = "Lookups+DummyList";
				columnStyleInfo.ModuleID = ModuleIDs.NotAssigned;
				columnStyleInfo.ColumnName = "Z0_NVarCharMax";
				columnStyleInfo.ShouldEscapeAllSpecialCharacters = shouldEscapeAllSpecialCharacters;

				grid.ColumnStyles.Add(columnStyleInfo);
				grid.Dock = DockStyle.Fill;
				form.Controls.Add(grid);
				form.Show();

				var macroFindBox = (ZMacrosFindBox)((ZMacrosFindBoxColumnStyle)grid.Columns[0].ColumnStyle).EditControl;
				macroFindBox.CodeBox.contextMenuManager.InitializeContextMenu();
				macroFindBox.CodeBox.ContextMenuStrip.Show();

				var insertTemplateMenuItem = macroFindBox.CodeBox.ContextMenuStrip.Items
					.Cast<ToolStripItem>().Where(t => t is ZToolStripMenuItem)
					.First(t => (t as ZToolStripMenuItem).Text == "Preview");
				Assert(insertTemplateMenuItem.Visible);

				insertTemplateMenuItem.PerformClick();
				using (var templateForm = ZFormModaliser.LastFormShownForTest)
				{
					AssertEquals("TextTemplatePreviewForm", templateForm.GetType().Name);
					var previewText = (templateForm as TextTemplatePreviewForm).textBox.Text;
					AssertEquals(expectedText, previewText);
				}
			}
		}

		public void TestSelectFromPopupFormShouldEscapeAllSpecialCharacters()
		{
			AssertSelectFromPopupFormShouldEscapeAllSpecialCharacters(true);
			AssertSelectFromPopupFormShouldEscapeAllSpecialCharacters(false);
		}

		void AssertSelectFromPopupFormShouldEscapeAllSpecialCharacters(bool shouldEscapeAllSpecialCharacters)
		{
			using (var form = new ZForm(Dummy))
			{
				var grid = new ZGrid();
				grid.BindTo = "Collection";
				var columnStyleInfo = new ZMacrosFindBoxColumnStyleInfo();
				columnStyleInfo.BindToList = "Lookups+DummyList";
				columnStyleInfo.ModuleID = ModuleIDs.NotAssigned;
				columnStyleInfo.ColumnName = "Z0_NVarChar";
				columnStyleInfo.MacroType = MacroType.DocEngine;
				columnStyleInfo.ShouldEscapeAllSpecialCharacters = shouldEscapeAllSpecialCharacters;

				grid.ColumnStyles.Add(columnStyleInfo);
				grid.Dock = DockStyle.Fill;
				form.Controls.Add(grid);
				form.Show();

				var macroFindBox = (ZMacrosFindBox)((ZMacrosFindBoxColumnStyle)grid.Columns[0].ColumnStyle).EditControl;

				bool result = false;
				var mock = new Mock<IMapTreePresentationManager>();
				mock.Setup(p => p.ShowPresentationManagerForm(It.IsAny<bool>(), It.IsAny<bool>()))
					.Callback((bool p1, bool p2) =>
					{
						result = p2;
					});
				macroFindBox.mapTreePresenter = mock.Object;

				macroFindBox.SelectFromPopupForm();

				AssertEquals(shouldEscapeAllSpecialCharacters, result);
			}
		}

		public void TestPreview()
		{
			using (var form = new ZForm(Dummy))
			{
				var childDummy = Dummy.Collection.AddNew();
				childDummy.Z0_Code = "AB<";
				childDummy.Z0_NVarCharMax = "<Z0_Code>";
				var grid = new ZGrid();
				grid.BindTo = "Collection";
				var columnStyleInfo = new ZMacrosFindBoxColumnStyleInfo();
				columnStyleInfo.BindToList = "Lookups+DummyList";
				columnStyleInfo.ModuleID = ModuleIDs.NotAssigned;
				columnStyleInfo.ColumnName = "Z0_NVarCharMax";

				grid.ColumnStyles.Add(columnStyleInfo);
				grid.Dock = DockStyle.Fill;
				form.Controls.Add(grid);
				form.Show();

				var macroFindBox = (ZMacrosFindBox)((ZMacrosFindBoxColumnStyle)grid.Columns[0].ColumnStyle).EditControl;
				macroFindBox.CodeBox.contextMenuManager.InitializeContextMenu();
				macroFindBox.CodeBox.ContextMenuStrip.Show();

				var insertTemplateMenuItem = macroFindBox.CodeBox.ContextMenuStrip.Items
					.Cast<ToolStripItem>().Where(t => t is ZToolStripMenuItem)
					.First(t => (t as ZToolStripMenuItem).Text == "Preview");
				Assert(insertTemplateMenuItem.Visible);

				insertTemplateMenuItem.PerformClick();
				using (var templateForm = ZFormModaliser.LastFormShownForTest)
				{
					AssertEquals("TextTemplatePreviewForm", templateForm.GetType().Name);
					var previewText = (templateForm as TextTemplatePreviewForm).textBox.Text;
					AssertEquals("AB<", previewText);
				}
			}
		}

		public void TestPreviewFindMacro()
		{
			var childDummy = Dummy.Collection.AddNew();
			childDummy.Z0_Code = "code";
			childDummy.Z0_VarCharMax = "123";
			childDummy.Z0_NVarCharMax = "<Collection.Find(\"{Z0_Code}\" == \"code\").Z0_VarCharMax>";

			EvaluateNVarCharMaxAsMacro(false, childDummy.Z0_VarCharMax);
			EvaluateNVarCharMaxAsMacro(true, childDummy.Z0_VarCharMax);
		}

		public void TestPreviewFirstMacro()
		{
			var childDummy = Dummy.Collection.AddNew();
			childDummy.Z0_Code = "code";
			childDummy.Z0_VarCharMax = "123";
			childDummy.Z0_NVarCharMax = "<Collection.First(\"<Z0_Code>\" == \"code\").Z0_VarCharMax>";

			EvaluateNVarCharMaxAsMacro(true, childDummy.Z0_VarCharMax);
		}

		public void TestPreviewMacroInvalid_MismatchedSymbols()
		{
			var childDummy = Dummy.Collection.AddNew();
			childDummy.Z0_NVarCharMax = "<Collection.First(\"<>>";

			EvaluateNVarCharMaxAsMacro(false, string.Empty);
			EvaluateNVarCharMaxAsMacro(true, string.Empty);
		}

		public void TestPreviewMacroInvalid_NonexistentValue()
		{
			var childDummy = Dummy.Collection.AddNew();
			childDummy.Z0_NVarCharMax = "<ThisIsNotAValidInput>";

			EvaluateNVarCharMaxAsMacro(false, string.Empty);
			EvaluateNVarCharMaxAsMacro(true, string.Empty);
		}

		public void TestPreviewMacroResultHasEscapedCharacters()
		{
			var childDummy = Dummy.Collection.AddNew();
			childDummy.Z0_Code = "AB<";
			childDummy.Z0_NVarCharMax = "<Z0_Code>";

			EvaluateNVarCharMaxAsMacro(false, childDummy.Z0_Code);
			EvaluateNVarCharMaxAsMacro(true, childDummy.Z0_Code);

			childDummy.Z0_Code = "code";
			childDummy.Z0_VarCharMax = "AB<";
			childDummy.Z0_NVarCharMax = "<Collection.First(\"<Z0_Code>\" == \"code\").Z0_VarCharMax>";

			EvaluateNVarCharMaxAsMacro(true, childDummy.Z0_VarCharMax);
		}

		void EvaluateNVarCharMaxAsMacro(bool useFieldChangeMacroEvaluator, string expected)
		{
			using (var form = new ZForm(Dummy))
			{
				var grid = new ZGrid();
				grid.BindTo = "Collection";

				var columnStyleInfo = new ZMacrosFindBoxColumnStyleInfo();
				columnStyleInfo.ColumnName = "Z0_NVarCharMax";
				columnStyleInfo.UseFieldChangeMacroEvaluatorForPreview = useFieldChangeMacroEvaluator;

				grid.ColumnStyles.Add(columnStyleInfo);
				form.Controls.Add(grid);
				form.Show();

				var macroFindBox = (ZMacrosFindBox)((ZMacrosFindBoxColumnStyle)grid.Columns[0].ColumnStyle).EditControl;
				macroFindBox.CodeBox.contextMenuManager.InitializeContextMenu();

				var insertTemplateMenuItem = macroFindBox.CodeBox.ContextMenuStrip.Items
					.Cast<ToolStripItem>().Where(t => t is ZToolStripMenuItem)
					.First(t => (t as ZToolStripMenuItem).Text == "Preview");

				insertTemplateMenuItem.PerformClick();
				using (var templateForm = ZFormModaliser.LastFormShownForTest)
				{
					var previewText = (templateForm as TextTemplatePreviewForm).textBox.Text;
					AssertEquals(expected, previewText);
				}
			}
		}

		public void TestColumnStyleType()
		{
			AssertEquals(typeof(ZMacrosFindBoxColumnStyle), new ZMacrosFindBoxColumnStyleInfo().ColumnStyleType);
		}

		public void TestCharacterCasing()
		{
			AssertEquals(CharacterCasing.Normal, new ZMacrosFindBoxColumnStyleInfo().CharacterCasing);
		}

		public void TestAntlrMacro()
		{
			using (var form = new ZForm(Dummy))
			{
				var grid = new ZGrid();
				grid.BindTo = "Collection";
				var columnStyleInfo = new ZMacrosFindBoxColumnStyleInfo();
				columnStyleInfo.BindToList = "Lookups+DummyList";
				columnStyleInfo.ModuleID = ModuleIDs.NotAssigned;
				columnStyleInfo.ColumnName = "Z0_NVarChar";
				columnStyleInfo.MacroType = MacroType.Antlr;

				grid.ColumnStyles.Add(columnStyleInfo);
				grid.Dock = DockStyle.Fill;
				form.Controls.Add(grid);
				form.Show();

				var macroFindBox = (ZMacrosFindBox)((ZMacrosFindBoxColumnStyle)grid.Columns[0].ColumnStyle).EditControl;

				AssertNull(macroFindBox.antlrMacroForm);
				macroFindBox.SelectFromPopupForm();
				AssertNotNull(macroFindBox.antlrMacroForm);
				AssertNotNull(macroFindBox.Current);
				AssertNotNull(macroFindBox.PropertyDescriptor);
			}
		}

		public void TestRoots()
		{
			using (var form = new ZForm(Dummy))
			{
				var grid = new ZGrid();
				grid.BindTo = "Collection";
				var columnStyleInfo = new ZMacrosFindBoxColumnStyleInfo();
				columnStyleInfo.BindToList = "Lookups+DummyList";
				columnStyleInfo.ModuleID = ModuleIDs.NotAssigned;
				columnStyleInfo.ColumnName = "Z0_NVarChar";

				grid.ColumnStyles.Add(columnStyleInfo);
				grid.Dock = DockStyle.Fill;
				form.Controls.Add(grid);
				form.Show();

				var macroFindBox = (ZMacrosFindBox)((ZMacrosFindBoxColumnStyle)grid.Columns[0].ColumnStyle).EditControl;

				AssertEquals(typeof(DummyChildBusinessObject), macroFindBox.Roots[0].GetType());
				AssertEquals(typeof(DummyBusinessObject), macroFindBox.Roots[1].GetType());
				AssertEquals(typeof(DummyChildBusinessObject), (macroFindBox.RootTypes[0]));
				AssertEquals(typeof(DummyBusinessObject), (macroFindBox.RootTypes[1]));
				AssertEquals(false, macroFindBox.IsUsedForExpressions);

				macroFindBox.SelectFromPopupForm();
				AssertEquals(DummyBizoSchema.Z0_NVarChar.MaxLength, macroFindBox.mapTreePresenter.MacroMaxLength);
				AssertEquals(macroFindBox.Roots, macroFindBox.mapTreePresenter.ParentBusinessObjects);
			}
		}

		public void TestRootTypes()
		{
			using (var form = new ZForm(Dummy))
			{
				var grid = new ZGrid
				{
					BindTo = "Collection"
				};
				var columnStyleInfo = new ZMacrosFindBoxColumnStyleInfo
				{
					BindToList = "Lookups+DummyList",
					ModuleID = ModuleIDs.NotAssigned,
					ColumnName = "Z0_NVarChar"
				};

				grid.ColumnStyles.Add(columnStyleInfo);
				grid.Dock = DockStyle.Fill;
				form.Controls.Add(grid);
				form.Show();

				var macroFindBox = (ZMacrosFindBox)((ZMacrosFindBoxColumnStyle)grid.Columns[0].ColumnStyle).EditControl;
				AssertSequencesEqual("RootTypes", new[]
				{
					typeof(DummyChildBusinessObject),
					typeof(DummyBusinessObject)
				}, macroFindBox.RootTypes);

				macroFindBox.DataFieldsOnly = false;
				macroFindBox.SelectFromPopupForm();
				AssertSequencesEqual("mapTreePresenter.ParentTypes", new[]
				{
					typeof(DummyChildBusinessObject),
					typeof(DummyBusinessObject)
				}, macroFindBox.mapTreePresenter.ParentTypes);

				macroFindBox.DataFieldsOnly = true;
				macroFindBox.SelectFromPopupForm();
				AssertEquals("mapTreePresenter.ParentTypes, DataFieldsOnly", typeof(DummyChildBusinessObject), macroFindBox.mapTreePresenter.ParentTypes.Single());
			}
		}

		public void TestMcrRootTypes()
		{
			using (var form = new ZForm(Dummy))
			{
				var grid = new ZGrid
				{
					BindTo = "Collection"
				};
				var columnStyleInfo = new ZMacrosFindBoxColumnStyleInfo();
				columnStyleInfo.MacroType = MacroType.Antlr;

				grid.ColumnStyles.Add(columnStyleInfo);
				grid.Dock = DockStyle.Fill;
				form.Controls.Add(grid);
				form.Show();

				var macroFindBox = (ZMacrosFindBox)((ZMacrosFindBoxColumnStyle)grid.Columns[0].ColumnStyle).EditControl;
				var dummy = Factory.New<DummyWithWorkflow>();
				var task = dummy.WorkflowItems.Triggers.AddNew();
				macroFindBox.Current = task;
				macroFindBox.PropertyDescriptor = TypeDescriptor.GetProperties(task).Find("TriggerConditions+TriggerConditionValue", false);

				macroFindBox.SelectFromPopupForm();
				AssertEquals("mapTreePresenter.ParentTypes, Trigger Condition = MCR", typeof(DummyEnterpriseBusinessObject), macroFindBox.mapTreePresenter.ParentTypes.Single().BaseType);
			}
		}

		public void TestRootTypesWithAdditionalType()
		{
			var dummyBO = Factory.New<DummyBusinessObjectForRootType>();

			using (var form = new ZForm(dummyBO))
			{
				var grid = new ZGrid
				{
					BindTo = "Collection"
				};
				var columnStyleInfo = new ZMacrosFindBoxColumnStyleInfo
				{
					BindToList = "Lookups+DummyList",
					ModuleID = ModuleIDs.NotAssigned,
					ColumnName = "Z0_NVarChar"
				};

				grid.ColumnStyles.Add(columnStyleInfo);
				grid.Dock = DockStyle.Fill;
				form.Controls.Add(grid);
				form.Show();

				var macroFindBox = (ZMacrosFindBox)((ZMacrosFindBoxColumnStyle)grid.Columns[0].ColumnStyle).EditControl;
				AssertSequencesEqual("RootTypes", new[]
				{
					typeof(DummyChildBusinessObjectForRootType),
					typeof(DummyBusinessObjectForRootType)
				}, macroFindBox.RootTypes);

				macroFindBox.DataFieldsOnly = true;
				macroFindBox.SelectFromPopupForm();
				AssertSequencesEqual("mapTreePresenter.ParentTypes", new[]
				{
					typeof(DummyChildBusinessObjectForRootType),
					typeof(DummyBusinessObjectForRootType)
				}, macroFindBox.mapTreePresenter.ParentTypes);
			}
		}

		public void TestIncludeParentJobInRoots()
		{
			AssertIncludeParentJobInRoots(true, new[]
				{
					typeof(DummyChildBusinessObjectForRootType),
					typeof(DummyBusinessObjectForRootType)
				});

			AssertIncludeParentJobInRoots(false, new[]
				{
					typeof(DummyChildBusinessObjectForRootType)
				});
		}

		void AssertIncludeParentJobInRoots(bool include, Type[] expectedRootTypes)
		{
			var dummyBO = Factory.New<DummyBusinessObjectForRootType>();

			using (var form = new ZForm(dummyBO))
			{
				var grid = new ZGrid
				{
					BindTo = "Collection"
				};
				var columnStyleInfo = new ZMacrosFindBoxColumnStyleInfo
				{
					BindToList = "Lookups+DummyList",
					ModuleID = ModuleIDs.NotAssigned,
					ColumnName = "Z0_NVarChar",
					IncludeParentJobInRoots = include
				};

				grid.ColumnStyles.Add(columnStyleInfo);
				grid.Dock = DockStyle.Fill;
				form.Controls.Add(grid);
				form.Show();

				var macroFindBox1 = (ZMacrosFindBox)((ZMacrosFindBoxColumnStyle)grid.Columns[0].ColumnStyle).EditControl;

				AssertSequencesEqual("RootTypes", expectedRootTypes, macroFindBox1.RootTypes);
			}
		}

		public void TestPredefinedRoots()
		{
			using (var form = new ZForm(Dummy))
			{
				var grid = new ZGrid();
				grid.BindTo = "Collection";
				var columnStyleInfo = new ZMacrosFindBoxColumnStyleInfo();
				columnStyleInfo.BindToList = "Lookups+DummyList";
				columnStyleInfo.ModuleID = ModuleIDs.NotAssigned;
				columnStyleInfo.ColumnName = "Z0_NVarCharMax";

				columnStyleInfo.UsePredefinedRoots = true;
				columnStyleInfo.RootTypes = new[] { typeof(DummyBusinessObjectWithCalculatedCodePropertyCollection) };
				columnStyleInfo.Roots = new[] { Factory.New<DummyBusinessObject>() };
				columnStyleInfo.UsePredefinedRoots = true;
				columnStyleInfo.IsUsedForExpressions = true;

				grid.ColumnStyles.Add(columnStyleInfo);
				grid.Dock = DockStyle.Fill;
				form.Controls.Add(grid);
				form.Show();

				var macroFindBox = ((ZMacrosFindBox)((ZMacrosFindBoxColumnStyle)grid.Columns[0].ColumnStyle).EditControl);

				AssertEquals(typeof(DummyBusinessObject), macroFindBox.Roots[0].GetType());
				AssertEquals(typeof(DummyBusinessObjectWithCalculatedCodePropertyCollection), (macroFindBox.RootTypes[0]));
				AssertEquals(true, macroFindBox.IsUsedForExpressions);

				macroFindBox.SelectFromPopupForm();
				AssertEquals(DummyBizoSchema.Z0_NVarCharMax.MaxLength, macroFindBox.mapTreePresenter.MacroMaxLength);
			}
		}

		public void TestDynamicRootsFromAttribute()
		{
			var dummy = Factory.New<DummyWithDynamicRoots>();
			using (var form = new ZForm(dummy))
			{
				var grid = new ZGrid();
				grid.BindTo = "Herp";
				var columnStyleInfo = new ZMacrosFindBoxColumnStyleInfo();
				columnStyleInfo.ModuleID = ModuleIDs.NotAssigned;
				columnStyleInfo.ColumnName = "Z0_NVarCharMax";
				columnStyleInfo.IsUsedForExpressions = true;

				grid.ColumnStyles.Add(columnStyleInfo);
				grid.Dock = DockStyle.Fill;
				form.Controls.Add(grid);
				form.Show();

				var macroFindBox = ((ZMacrosFindBox)((ZMacrosFindBoxColumnStyle)grid.Columns[0].ColumnStyle).EditControl);

				AssertArrayEqualsByElements(new[] { typeof(object), typeof(DummyWithDynamicRoots) }, macroFindBox.RootTypes);
				AssertArrayEqualsByElements(new[] { dummy.GetType() }, macroFindBox.Roots.Select(t => t.GetType()).ToArray());
			}
		}

		public void TestXmlType()
		{
			var mockMapTreePresentationManager = new Mock<IMapTreePresentationManager>();
			var dummy = Factory.New<DummyWithDynamicRoots>();
			using (var form = new ZForm(dummy))
			using (ObjectFactory.Substitute(mockMapTreePresentationManager.Object))
			{
				var grid = new ZGrid();
				grid.BindTo = "Herp";
				var columnStyleInfo = new ZMacrosFindBoxColumnStyleInfo();
				columnStyleInfo.ModuleID = ModuleIDs.NotAssigned;
				columnStyleInfo.ColumnName = "Z0_NVarCharMax";
				columnStyleInfo.IsUsedForExpressions = true;
				columnStyleInfo.XmlType = typeof(DummyForXmlType);

				grid.ColumnStyles.Add(columnStyleInfo);
				grid.Dock = DockStyle.Fill;
				form.Controls.Add(grid);
				form.Show();

				var macroFindBox = ((ZMacrosFindBox)((ZMacrosFindBoxColumnStyle)grid.Columns[0].ColumnStyle).EditControl);
				AssertEquals(typeof(DummyForXmlType), macroFindBox.XmlType);
				macroFindBox.SelectFromPopupForm();
				mockMapTreePresentationManager.VerifySet(x => x.XmlType = typeof(DummyForXmlType));
			}
		}

		class DummyForXmlType
		{
			public ZString? Code { get; set; }
		}

		class DummyWithDynamicRootsCollection : BusinessObjectCollection<DummyWithDynamicRoots>
		{
			public DummyWithDynamicRootsCollection(BusinessObjectFactory factory) : base(factory)
			{
			}
		}

		class DummyWithDynamicRoots : DummyBusinessObject
		{
			public DummyWithDynamicRoots(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public DummyWithDynamicRootsCollection Herp
			{
				get => derp ?? (derp = new DummyWithDynamicRootsCollection(Factory));
			}
			DummyWithDynamicRootsCollection derp;

			[RootTypeProvider(nameof(ZNVarCharMaxTypes), nameof(ZNVarCharMaxRoots))]
			public override ZString Z0_NVarCharMax { get => base.Z0_NVarCharMax; set => base.Z0_NVarCharMax = value; }

			public Type[] ZNVarCharMaxTypes() => new[] { typeof(object), typeof(DummyWithDynamicRoots) };
			public BusinessObject[] ZNVarCharMaxRoots() => new[] { this };
		}

		public void TestInsertTemplateMenuItem()
		{
			using (var form = new ZForm(Dummy))
			{
				var grid = new ZGrid();
				grid.BindTo = "Collection";
				var columnStyleInfo = new ZMacrosFindBoxColumnStyleInfo();
				columnStyleInfo.BindToList = "Lookups+DummyList";
				columnStyleInfo.ModuleID = ModuleIDs.NotAssigned;
				columnStyleInfo.ColumnName = "Z0_NVarCharMax";

				grid.ColumnStyles.Add(columnStyleInfo);
				grid.Dock = DockStyle.Fill;
				form.Controls.Add(grid);
				form.Show();

				var macroFindBox = (ZMacrosFindBox)((ZMacrosFindBoxColumnStyle)grid.Columns[0].ColumnStyle).EditControl;
				macroFindBox.CodeBox.contextMenuManager.InitializeContextMenu();
				macroFindBox.CodeBox.ContextMenuStrip.Show();

				var insertTemplateMenuItem = macroFindBox.CodeBox.ContextMenuStrip.Items.Cast<ZToolStripMenuItem>().First(i => i.Text == "Insert Template");
				Assert(insertTemplateMenuItem.Visible);

				insertTemplateMenuItem.DropDownItems.Cast<ZToolStripMenuItem>().First(i => i.Text == "Create Template").PerformClick();
				using (var templateForm = ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertEquals("ExpressionTemplateForm", templateForm.GetType().Name);
				}
			}
		}

		public void TestMacroSelectedOverCodeBoxMaxLength()
		{
			using (var form = new ZForm(Dummy))
			{
				var grid = new ZGrid();
				grid.BindTo = "Collection";
				var columnStyleInfo = new ZMacrosFindBoxColumnStyleInfo();
				columnStyleInfo.BindToList = "Lookups+DummyList";
				columnStyleInfo.ModuleID = ModuleIDs.NotAssigned;
				columnStyleInfo.ColumnName = "Z0_NVarChar";
				columnStyleInfo.AllowMultipleMacroses = true;

				grid.ColumnStyles.Add(columnStyleInfo);
				grid.Dock = DockStyle.Fill;
				form.Controls.Add(grid);

				var button = new ZButton();
				button.Location = new Point(30, 30);
				form.Controls.Add(button);

				form.Show();

				var macroFindBox = (ZMacrosFindBox)((ZMacrosFindBoxColumnStyle)grid.Columns[0].ColumnStyle).EditControl;
				var maxLength = macroFindBox.CodeBox.MaxLength;
				var macro = ZString.Replicate('A', maxLength + 10);
				var validMacroText = macro.ToString().Substring(0, maxLength);

				button.Click += delegate
				{
					macroFindBox.MacroSelected(macro.ToString());
				};

				button.PerformClick();

				Assert($"Macro length cannot be greater than {maxLength} symbols", macroFindBox.Text.Length <= maxLength);
				AssertEquals(validMacroText, macroFindBox.CodeBox.Text);
			}
		}

		public void TestCodeBoxShouldBeUpdatedCorrectlyWhenInsertMacro()
		{
			using (var form = new ZForm(Dummy))
			{
				var grid = new ZGrid();
				grid.BindTo = "Collection";
				var columnStyleInfo = new ZMacrosFindBoxColumnStyleInfo();
				columnStyleInfo.BindToList = "Lookups+DummyList";
				columnStyleInfo.ModuleID = ModuleIDs.NotAssigned;
				columnStyleInfo.ColumnName = "Z0_NVarChar";
				columnStyleInfo.AllowMultipleMacroses = true;

				grid.ColumnStyles.Add(columnStyleInfo);
				grid.Dock = DockStyle.Fill;
				form.Controls.Add(grid);

				var button = new ZButton();
				button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(30, 30);
				form.Controls.Add(button);

				form.Show();

				var macroFindBox = (ZMacrosFindBox)((ZMacrosFindBoxColumnStyle)grid.Columns[0].ColumnStyle).EditControl;
				macroFindBox.CodeBox.Text = "<Macro1>";
				var textInMacroBox = "<Macro1><Macro2>";

				button.Click += delegate
				{
					macroFindBox.MacroSelected(textInMacroBox);
				};

				button.PerformClick();
				AssertEquals("CodeBox's contents should be equal to textMacroBox's contents", textInMacroBox, macroFindBox.CodeBox.Text);
			}
		}
	}
}
