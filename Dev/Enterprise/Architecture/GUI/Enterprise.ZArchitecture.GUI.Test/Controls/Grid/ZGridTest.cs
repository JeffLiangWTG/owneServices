using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.BuildTools;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.Abstractions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Grid;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Mono.Cecil;
using Moq;
using NUnit.Framework;

#if !WINZOR
using CargoWise.StaticAnalysis;
#endif

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZGridTest : TestCaseWithDummy
	{
		public void TestDeleteMenuItemTest()
		{
			using (var grid = new ZGrid())
			{
				AssertEquals("&Delete", grid.DeleteMenuItem.Text);
				grid.RemoveAction = RemoveAction.Remove;
				AssertEquals("&Remove", grid.DeleteMenuItem.Text);
			}
		}
#if !WINZOR
		public void TestDuplicatedGridIds()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var assemblies = BuildXml.Instance.GetAllAssembliesToBuild(true);
			var gridIds = new List<string>();

			foreach (var assemblyName in assemblies)
			{
				gridIds.AddRange(GetGridIdsFromAssembly(assemblyName, binPath));
			}

			var existedDupGridIds = GetDuplicatedGridIdsBaseline();
			var duplicated = gridIds.GroupBy(id => id).Where(g => g.Count() > 1).Select(g => g.Key);
			var newDuplicated = duplicated.Where(id => !existedDupGridIds.Contains(id));

			var duplicatedIds = string.Empty;
			if (newDuplicated.Any())
			{
				duplicatedIds = string.Join(",", newDuplicated);
			}
			AssertNullOrEmpty("duplicated ZGrid.GridId", duplicatedIds);
		}

		public bool IsNotTargetPrefix(string assemblyName)
		{
#pragma warning disable CS0436 // Type conflicts with imported type - due to InternalsVisibleTo
			bool isNetCoreTargetFrameworkPrefix = assemblyName.StartsWith(CommonAssemblyInfo.CWNetCoreSubfolder, StringComparison.OrdinalIgnoreCase);
#pragma warning restore CS0436 // Type conflicts with imported type
#if NETFRAMEWORK
			return isNetCoreTargetFrameworkPrefix;
#elif NET
			return !isNetCoreTargetFrameworkPrefix;
#else
#error Unexpected target platform
#endif
		}

		List<string> GetGridIdsFromAssembly(string assemblyName, string binPath)
		{
			var gridIds = new List<string>();

			if (IsNotTargetPrefix(assemblyName))
			{
				return gridIds;
			}

			var readSymbols = File.Exists(Path.Combine(binPath, Path.GetFileNameWithoutExtension(assemblyName) + ".pdb"));

			AssemblyDefinition assemblyDefinition;

			try
			{
				assemblyDefinition = AssemblyDefinition.ReadAssembly(Path.Combine(binPath, assemblyName), new ReaderParameters() { ReadSymbols = readSymbols });
			}
			catch (InvalidOperationException ex)
			{
				if (readSymbols && ex.Message == "Operation is not valid due to the current state of the object.")
				{
					throw new InvalidOperationException($"Error processing assembly. This Mono.Cecil error is thrown when the pdb does not match the dll. See: https://github.com/brutaldev/StrongNameSigner/issues/40#issuecomment-314053923. If this is an *.XmlSerializers.dll, is the assembly being created from <CWGenerateXmlSerializationAssemblies> as well as a post-build call to CargoWise.SGen.exe? assembly name = {assemblyName}, path = {binPath}. Original exception = {ex}", ex);
				}
				else
				{
					throw;
				}
			}

			AssertEquals("Expected an assembly containing exactly 1 module", assemblyDefinition.Modules.Count, 1);

			((DefaultAssemblyResolver)assemblyDefinition.Modules[0].AssemblyResolver).RemoveSearchDirectory(".");
			((DefaultAssemblyResolver)assemblyDefinition.Modules[0].AssemblyResolver).AddSearchDirectory(binPath);

			var assembly = Assembly.Load(assemblyDefinition.FullName);
			foreach (var type in assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Control))))
			{
				var typeDefinition = assemblyDefinition.Modules[0].GetType(type.FullName, true) as TypeDefinition;
				{
					if (typeDefinition != null && typeDefinition.HasMethods)
					{
						foreach (var methodDefinition in typeDefinition.Methods)
						{
							if (methodDefinition == null || methodDefinition.DeclaringType.Namespace.StartsWith("System") || methodDefinition.DeclaringType.Namespace.StartsWith("Microsoft"))
							{
								continue;
							}
							var state = new ExecutionState();
							if (methodDefinition.HasBody)
							{
								state.CallStack.Push(methodDefinition);
								var analyzer = new MethodCallAnalyzer() { AnlayzeReadonlyFields = true };
								analyzer.MethodCall += new MethodCallAnalyzer.MethodCallDelegate((MethodReference callMethod, object instance, object[] parameters, Context context) =>
								{
									if (callMethod.Name == typeof(ZGrid).GetProperty(nameof(ZGrid.GridId)).SetMethod.Name)
									{
										if (instance is StringConstant stringConstant)
										{
											gridIds.Add(stringConstant.Value);
										}
									}
									return null;
								});
								analyzer.Analyze(methodDefinition, state);
							}
						}
					}
				}
			}
			return gridIds;
		}

		HashSet<string> GetDuplicatedGridIdsBaseline()
		{
			var baseline = new HashSet<string>();
			using var resourceRetriever = new EmbeddedResourceRetriever();
			using var stream = resourceRetriever.GetStream("DuplicatedGridIdsBaseline.txt");
			using var reader = new StreamReader(stream);
			foreach (var line in reader.ReadToEnd().SplitByLine())
			{
				baseline.Add(line);
			}
			return baseline;
		}
#endif
		public void TestGridDefaultColumnsShouldKeepVisibleColumnsFirst()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			using (var grid = new ZTestGrid())
			using (var form = new ZForm(dummy))
			{
				form.Size = new Size(300, 500);
				grid.Dock = DockStyle.Fill;
				dummy.Collection.AddNew();
				form.Controls.Add(grid);
				grid.BindTo = "Collection";
				grid.Columns.AddTextColumn("Z0_Code", 60, false, true, true);
				grid.Columns.AddTextColumn("Z0_Description", 60, true, true, true);
				grid.Columns.AddTextColumn("Z0_Long", 60, false, true, true);
				grid.Columns.AddTextColumn("Z0_Number", 60, true, false, false);

				form.Show();

				AssertEquals(4, grid.Columns.Count);
				AssertEquals("Z0_Description", grid.Columns[0].ColumnName);
				AssertEquals("Z0_Number", grid.Columns[1].ColumnName);
				AssertEquals("Z0_Code", grid.Columns[2].ColumnName);
				AssertEquals("Z0_Long", grid.Columns[3].ColumnName);

				AssertEquals(4, grid.DefaultColumns.Count);
				AssertEquals("Z0_Description", grid.DefaultColumns[0].ColumnName);
				AssertEquals("Z0_Number", grid.DefaultColumns[1].ColumnName);
				AssertEquals("Z0_Code", grid.DefaultColumns[2].ColumnName);
				AssertEquals("Z0_Long", grid.DefaultColumns[3].ColumnName);
			}
		}

		public void TestSetCurrentCellToFirst_WhenAllColumnsAreReadOnly()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Collection.Add(Factory.New<ReadOnlyDummyChild>());

			using (var grid = new ZGrid())
			using (var form = new ZForm(dummy))
			{
				grid.BindTo = "Collection";

				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "Z0_Bool" });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "Z0_Decimal" });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "Z0_Number" });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "Z0_Xml" });

				form.Controls.Add(grid);
				form.Show();

				AssertEquals("current cell row should be 0.", 0, grid.CurrentCell.RowNumber);
				AssertEquals("current cell column should be 0.", 0, grid.CurrentCell.ColumnNumber);
			}
		}

		public void TestSetCurrentCellToFirstEditable_WhenNotAllColumnsAreReadOnly()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Collection.Add(Factory.New<ReadOnlyDummyChild>());

			using (var grid = new ZGrid())
			using (var form = new ZForm(dummy))
			{
				grid.BindTo = "Collection";

				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "Z0_Bool" });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "Z0_Decimal" });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "Z0_Description" });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "Z0_Number" });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "Z0_Xml" });

				form.Controls.Add(grid);
				form.Show();

				AssertEquals("current cell row should be 0.", 0, grid.CurrentCell.RowNumber);
				AssertEquals("current cell column should be first editable column.", 2, grid.CurrentCell.ColumnNumber);
			}
		}

		public void TestSetCurrentCellToFirst_WhenNotAllColumnsAreReadOnly_ButVisibleColumnsAreReadOnly()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Collection.Add(Factory.New<ReadOnlyDummyChild>());

			using (var grid = new ZGrid())
			using (var form = new ZForm(dummy))
			{
				grid.BindTo = "Collection";

				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "Z0_Bool" });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "Z0_Decimal" });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "Z0_Number" });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "Z0_Xml" });

				var descriptionColInfo = new ZTextBoxColumnStyleInfo { ColumnName = "Z0_Description" };
				grid.ColumnStyles.Add(descriptionColInfo);

				form.Controls.Add(grid);
				form.Show();

				AssertEquals("current cell row should be 0.", 0, grid.CurrentCell.RowNumber);
				AssertEquals("current cell column should be 4.", 4, grid.CurrentCell.ColumnNumber);

				descriptionColInfo.IsVisible = false;
				grid.RefreshTableStyles();
				form.Show();

				AssertEquals("current cell row should be 0.", 0, grid.CurrentCell.RowNumber);
				AssertEquals("current cell column should be 0.", 0, grid.CurrentCell.ColumnNumber);
			}
		}

		class ReadOnlyDummyChild : DummyChildBusinessObject
		{
			public ReadOnlyDummyChild(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			[ReadOnly(true)]
			public override ZBool Z0_Bool { get => base.Z0_Bool; set => base.Z0_Bool = value; }

			[ReadOnly(true)]
			public override ZDecimal Z0_Decimal { get => base.Z0_Decimal; set => base.Z0_Decimal = value; }

			[ReadOnly(true)]
			public override ZInt Z0_Number { get => base.Z0_Number; set => base.Z0_Number = value; }

			protected bool Z0_Xml_ReadOnly => true;
		}

		[GuiTest]
		public void TestSetCurrentCellToFirstNonReadOnlyColumnShouldNotWorkIfIsWholeRowSelectedOnClick()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			using (var grid = new ZTestGrid())
			using (var form = new ZForm(dummy))
			{
				grid.IsWholeRowSelectedOnClick = true;
				form.Size = new Size(300, 500);
				grid.Dock = DockStyle.Fill;
				dummy.Collection.AddNew();
				dummy.Collection.AddNew();
				form.Controls.Add(grid);
				grid.BindTo = "Collection";
				grid.Columns.AddTextColumn("Z0_Code", 60, true, true, true);
				grid.Columns.AddTextColumn("Z0_Description", 60, true, true, true);
				grid.Columns.AddTextColumn("Z0_Long", 60, true, true, true);
				grid.Columns.AddTextColumn("Z0_IsSystem", 60, true, true, true);
				grid.Columns.AddTextColumn("Z0_Decimal", 60, true, true, true);
				grid.Columns.AddTextColumn("Z0_Guid", 60, true, true, true);
				grid.Columns.AddTextColumn("Z0_Money", 60, true, true, true);
				grid.Columns.AddTextColumn("Z0_Number", 60, false, false, false);

				form.Show();

				AssertEquals("current cell row should be 0.", 0, grid.CurrentCell.RowNumber);
				AssertEquals("current cell column should be 0.", 0, grid.CurrentCell.ColumnNumber);
			}
		}

		[GuiTest]
		public void TestClickBlankInGridWouldNotResetCurrentCell()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			using (var grid = new ZTestGrid())
			using (var form = new ZForm(dummy))
			{
				form.Size = new Size(500, 500);
				grid.Dock = DockStyle.Fill;
				dummy.Collection.AddNew();
				dummy.Collection.AddNew();
				form.Controls.Add(grid);
				grid.BindTo = "Collection";
				grid.Columns.AddTextColumn("Z0_Code", 30);
				grid.Columns.AddTextColumn("Z0_Description", 60);
				grid.Columns.AddTextColumn("Z0_Money", 60);
				form.Show();

				AssertEquals("the row of the current cell should be 0.", 0, grid.CurrentCell.RowNumber);
				AssertEquals("the column of the current cell should be 0.", 0, grid.CurrentCell.ColumnNumber);

				grid.BeginEdit(grid.Columns[1].ColumnStyle, 1);
				Application.DoEvents();

				AssertEquals("the row of the current cell should be 1.", 1, grid.CurrentCell.RowNumber);
				AssertEquals("the column of the current cell should be 1.", 1, grid.CurrentCell.ColumnNumber);

				var pos = grid.GetCellBounds(2, 2).Location;
				grid.FireMouseDown(pos.X + 50, pos.Y + 50);

				AssertEquals("current cell should not be reset to the first column of the row.", 1, grid.CurrentCell.RowNumber);
				AssertEquals("current cell should not be reset to the first column of the row.", 1, grid.CurrentCell.ColumnNumber);
			}
		}

		public void TestSetCellFromMouseClickWhenListManagerIsNull()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			using (var grid = new ZTestGrid())
			using (var form = new ZForm(dummy))
			{
				dummy.Collection.AddNew();
				dummy.Collection.AddNew();
				form.Controls.Add(grid);
				grid.BindTo = "Collection";
				grid.Columns.AddTextColumn("Z0_Code", 30);
				form.Show();
				AssertNotNull("ListManager is not null", grid.ListManager);
				grid.CurrentCell = new DataGridCell(1, 0);
				grid.IsListManagerNotNull = false;
				AssertNull("ListManager should be null", grid.ListManager);
				var pos = grid.GetCellBounds(0, 0).Location;
				var mousePos = new Point(pos.X + 2, pos.Y);
				grid.SetMousePosition(mousePos);
				grid.SetMouseButtonState(MouseButtons.Left);
				AssertNoExceptionThrown(() => grid.OnEnter(EventArgs.Empty));
			}
		}

		public void TestIsNextRowValidWhenListManagerIsNull()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			using (var grid = new ZTestGrid())
			using (var form = new ZForm(dummy))
			{
				dummy.Collection.AddNew();
				dummy.Collection.AddNew();
				form.Controls.Add(grid);
				grid.BindTo = "Collection";
				grid.Columns.AddTextColumn("Z0_Code", 30);
				form.Show();
				AssertNotNull("ListManager is not null", grid.ListManager);
				grid.CurrentCell = new DataGridCell(1, 0);
				grid.IsListManagerNotNull = false;
				AssertNull("ListManager should be null", grid.ListManager);
				var pos = grid.GetCellBounds(0, 0).Location;
				var mousePos = new Point(pos.X + 2, pos.Y);
				grid.SetMousePosition(mousePos);
				grid.SetMouseButtonState(MouseButtons.Left);
				var funct = typeof(ZGrid).GetMethod("IsNextRowValid", BindingFlags.NonPublic | BindingFlags.Instance);
				AssertNoExceptionThrown(() => funct.Invoke(grid, new object[] { true, 0 }));
			}
		}

		public void TestSelectedElements_NoErrorReportWhenDataGridRowsAreNotYetCreated()
		{
			using (var form = new ZForm())
			using (var grid = new ZTestGrid())
			{
				form.Controls.Add(grid);
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Code });

				Dummy.Collection.AddNew();
				Dummy.Collection.AddNew();
				Dummy.Collection.AddNew();
				Dummy.Collection.AddNew();

				form.SetDataBinding(Dummy, "");
				grid.SetDataBinding(Dummy, "Collection");

				AssertEquals(4, grid.ListManager.Count);
				AssertEquals(0, grid.DataGridRowsLength);
				AssertEquals(0, grid.SelectedElements.Length); // Access grid.SelectedElements

				form.Show();
				Application.DoEvents();

				AssertEquals(5, grid.DataGridRowsLength); // 4 initial rows + 1 row in grid for entering new data

				using (Dummy.Collection.SuspendListChanged())
				{
					Dummy.Collection.AddNew();
					Dummy.Collection.AddNew();
					Dummy.Collection.AddNew();

					AssertEquals(7, grid.ListManager.Count);
					AssertEquals(5, grid.DataGridRowsLength);
					AssertEquals(0, grid.SelectedElements.Length); // Access grid.SelectedElements
				}
			}
		}

		public void TestSetBackreferenceToItselfOnGridColumnsCollection()
		{
			using (var grid = new ZTestGrid())
			{
				Assert(grid.Columns.Grid == grid);
			}
		}

		public class DummyChildEnterpriseBusinessObjectA : DummyChildEnterpriseBusinessObject
		{
			public DummyChildEnterpriseBusinessObjectA(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		public class DummyChildEnterpriseBusinessObjectB : DummyChildEnterpriseBusinessObject
		{
			public DummyChildEnterpriseBusinessObjectB(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		public void TestCopyPreviousRow()
		{
			var obj1 = Factory.New<DummyBusinessObject>();
			var obj2 = Factory.New<DummyBusinessObject>();
			var obj3 = Factory.New<DummyBusinessObject>();
			var obj4 = Factory.New<DummyBusinessObject>();
			obj1.Z0_Code = "z1";
			obj2.Z0_Code = "z2";
			obj3.Z0_Code = "z3";
			obj4.Z0_Code = "z4";
			Factory.Save();

			var dummy1 = Factory.New<DummyChildEnterpriseBusinessObjectA>();
			Dummy.Collection.Add(dummy1);
			dummy1.Z0_Code = "111";
			dummy1.Z0_Description = "ONE";
			dummy1.Z0_NVarCharMax = "1";
			dummy1.Z0_VarCharMax = "11111";
			dummy1.Z0_Guid = obj1.PK;

			dummy1.Z0_VarCharMax_ReadOnly = true;
			var dummy2 = Factory.New<DummyChildEnterpriseBusinessObjectB>();
			Dummy.Collection.Add(dummy2);
			dummy2.Z0_Code = "222";
			dummy2.Z0_Description = "TWO";
			dummy2.Z0_NVarCharMax = "2";
			dummy2.Z0_VarCharMax = "22222";
			dummy2.Z0_Guid = obj2.PK;
			dummy2.Z0_VarCharMax_ReadOnly = true;
			var dummy3 = Factory.New<DummyChildEnterpriseBusinessObjectA>();
			Dummy.Collection.Add(dummy3);
			dummy3.Z0_Code = "333";
			dummy3.Z0_NVarCharMax = "3";
			dummy3.Z0_Description = "THREE";
			dummy3.Z0_VarCharMax = "33333";
			dummy3.Z0_Guid = obj3.PK;

			using (var form = new ZForm(Dummy))
			{
				form.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 500);

				var grid = new ZTestGrid();
				grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
				grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 200);
				form.Controls.Add(grid);

				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Code });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Description });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_NVarCharMax, IsReadOnly = true });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_VarCharMax });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "RelatedDummy+" + DummyBizoSchema.Constants.Z0_Code });

				form.Show();
				grid.SetDataBinding(Dummy, "Collection");

				AssertEquals("Precondition: ", 5, grid.ColumnStyles.Count);

				AssertEquals("dummy1.Z0_VarCharMaxInfo.ReadOnly", true, dummy1.Z0_VarCharMaxInfo.ReadOnly);
				AssertEquals("dummy2.Z0_VarCharMaxInfo.ReadOnly", true, dummy2.Z0_VarCharMaxInfo.ReadOnly);
				AssertEquals("dummy3.Z0_VarCharMaxInfo.ReadOnly", false, dummy3.Z0_VarCharMaxInfo.ReadOnly);

				grid.CurrentCell = new DataGridCell(0, 0);

				FocusAndEditGrid(grid);
				AssertEquals("Precondition", new DataGridCell(0, 0), grid.CurrentCell);

				grid.LastFocusedColumn.TextBox.Visible = true;

				PostKeyToGrid(grid, Keys.F9);
				AssertGridResults("Should not have done anything. Nothing above to Copy Down.", new DataGridCell(0, 0), grid, "111", "ONE", "z1", "222", "TWO", "z2", "333", "THREE", "z3");

				PostKeyToGrid(grid, Keys.Tab);
				AssertGridResults("Should have moved to the Right.", new DataGridCell(0, 1), grid, "111", "ONE", "z1", "222", "TWO", "z2", "333", "THREE", "z3");

				PostKeyToGrid(grid, Keys.F9);
				AssertGridResults("Should not have done anything here either. Nothing above to Copy Down.", new DataGridCell(0, 1), grid, "111", "ONE", "z1", "222", "TWO", "z2", "333", "THREE", "z3");

				grid.LastFocusedColumn.TextBox.Visible = true;
				PostKeyToGrid(grid, Keys.Tab);
				AssertGridResults("Should now be on the next line.", new DataGridCell(0, 4), grid, "111", "ONE", "z1", "222", "TWO", "z2", "333", "THREE", "z3");

				PostKeyToGrid(grid, Keys.Tab);
				AssertGridResults("Should now be on the next line.", new DataGridCell(1, 0), grid, "111", "ONE", "z1", "222", "TWO", "z2", "333", "THREE", "z3");

				PostKeyToGrid(grid, Keys.F9);
				AssertGridResults("Should have copied down and moved right.", new DataGridCell(1, 1), grid, "111", "ONE", "z1", "111", "TWO", "z2", "333", "THREE", "z3");

				PostKeyToGrid(grid, Keys.F9);
				AssertGridResults("Should have copied down and moved right", new DataGridCell(1, 4), grid, "111", "ONE", "z1", "111", "ONE", "z2", "333", "THREE", "z3");

				PostKeyToGrid(grid, Keys.F9);
				AssertGridResults("Should have copied down and moved to the next line", new DataGridCell(2, 0), grid, "111", "ONE", "z1", "111", "ONE", "z1", "333", "THREE", "z3");

				dummy1.Z0_Code = "BWU";
				dummy1.Z0_Description = "BWUHAHA";
				dummy1.RelatedDummy.Z0_Code = "BZZZ";

				PostKeyToGrid(grid, Keys.F9);
				AssertGridResults("Should have copied down and moved right.", new DataGridCell(2, 1), grid, "BWU", "BWUHAHA", "BZZZ", "111", "ONE", "z1", "111", "THREE", "z3");

				PostKeyToGrid(grid, Keys.F9);
				AssertGridResults("Should have copied down and moved to the right again.", new DataGridCell(2, 3), grid, "BWU", "BWUHAHA", "BZZZ", "111", "ONE", "z1", "111", "ONE", "z3");

				AssertEquals("Precondition: dummy3.Z0_VarCharMax", "33333", dummy3.Z0_VarCharMax);

				PostKeyToGrid(grid, Keys.F9);
				AssertGridResults("Should have copied down and moved to the right again.", new DataGridCell(2, 4), grid, "BWU", "BWUHAHA", "BZZZ", "111", "ONE", "z1", "111", "ONE", "z3");

				PostKeyToGrid(grid, Keys.F9);
				AssertGridResults("Should have copied down the prev cell and moved to the next empty line.", new DataGridCell(3, 0), grid, "BWU", "BWUHAHA", "BZZZ", "111", "ONE", "z1", "111", "ONE", "z1");
				AssertEquals("dummy3.Z0_VarCharMax", "22222", dummy3.Z0_VarCharMax);

				dummy2.Z0_Code = "WOD";
				dummy2.Z0_Description = "WODGER";
				dummy2.RelatedDummy.Z0_Code = "elk";

				PostKeyToGrid(grid, Keys.F9);
				AssertEquals(4, Dummy.Collection.Count);
				Dummy.Collection[3].Z0_Guid = obj4.PK;
				AssertGridResults("Should have created the new row and copied down, then moved right.", new DataGridCell(3, 1), grid, "BWU", "BWUHAHA", "BZZZ", "WOD", "WODGER", "elk", "111", "ONE", "z1", "111", "Default", "z4");

				PostKeyToGrid(grid, Keys.F9);
				AssertGridResults("Should have copied down onto the new row and moved right again skipping the readonly column.", new DataGridCell(3, 3), grid, "BWU", "BWUHAHA", "BZZZ", "WOD", "WODGER", "elk", "111", "ONE", "z1", "111", "ONE", "z4");

				PostKeyToGrid(grid, Keys.F9);
				AssertGridResults("Should have copied down onto the new row and moved right again skipping the readonly column.", new DataGridCell(3, 4), grid, "BWU", "BWUHAHA", "BZZZ", "WOD", "WODGER", "elk", "111", "ONE", "z1", "111", "ONE", "z4");

				grid.CurrentCell = new DataGridCell(3, 2);
				PostKeyToGrid(grid, Keys.F9);
				AssertGridResults("Nothing should happen, the column is Read Only.", new DataGridCell(3, 2), grid, "BWU", "BWUHAHA", "BZZZ", "WOD", "WODGER", "elk", "111", "ONE", "z1", "111", "ONE", "z4");

				grid.CurrentCell = new DataGridCell(1, 3);
				PostKeyToGrid(grid, Keys.F9);
				AssertGridResults("Nothing should happen, the cell is Read Only.", new DataGridCell(1, 3), grid, "BWU", "BWUHAHA", "BZZZ", "WOD", "WODGER", "elk", "111", "ONE", "z1", "111", "ONE", "z4");

				grid.CurrentCell = new DataGridCell(2, 1);
				PostKeyToGrid(grid, Keys.F9);
				AssertGridResults("Should have copied down over the existing value and moved right over readonly column.", new DataGridCell(2, 3), grid, "BWU", "BWUHAHA", "BZZZ", "WOD", "WODGER", "elk", "111", "WODGER", "z1", "111", "ONE", "z4");
			}
		}

		public void TestAltUpWhenAllowNewIsToggled()
		{
			var obj1 = Factory.New<DummyBusinessObjectWithAllowNewToggleCollection>();

			using (var form = new ZForm(obj1))
			{
				form.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 500);

				var grid = new ZTestGrid();
				grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
				grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 200);
				form.Controls.Add(grid);

				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Code });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Description });

				form.Show();
				grid.SetDataBinding(obj1, "AllowNewCollection");

				AssertEquals("Precondition: ", 0, obj1.AllowNewCollection.Count);
				AssertEquals("Precondition: ", true, obj1.AllowNewCollection.AllowNew);

				grid.CurrentCell = new DataGridCell(0, 0);
				FocusAndEditGrid(grid);
				PostKeyToGrid(grid, Keys.A);
				AssertEquals("Precondition", new DataGridCell(0, 0), grid.CurrentCell);
				AssertEquals("Precondition: ", 1, obj1.AllowNewCollection.Count);
				AssertEquals("Precondition: ", true, obj1.AllowNewCollection.AllowNew);

				grid.LastFocusedColumn.TextBox.Visible = true;

				grid.CurrentCell = new DataGridCell(1, 0);
				AssertEquals("A new row should be added when entering new cell", 2, obj1.AllowNewCollection.Count);
				AssertEquals("AllowNew is now false: ", false, obj1.AllowNewCollection.AllowNew);
				PostKeyToGrid(grid, Keys.A);
				AssertNoExceptionThrown(() => { PostKeyToGrid(grid, Keys.Alt | Keys.Up); });
			}
		}

		internal class DummyBusinessObjectWithAllowNewToggleCollection : DummyBusinessObject
		{
			public DummyBusinessObjectWithAllowNewToggleCollection(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public DummyBusinessObjectAllowNewToggleCollection AllowNewCollection
			{
				get
				{
					if (allowNewCollection == null)
					{
						allowNewCollection = new DummyBusinessObjectAllowNewToggleCollection(this.Factory);
					}
					return allowNewCollection;
				}
			}
			DummyBusinessObjectAllowNewToggleCollection allowNewCollection;
		}

		internal class DummyBusinessObjectAllowNewToggleCollection : DummyBusinessObjectCollection
		{
			public DummyBusinessObjectAllowNewToggleCollection(BusinessObjectFactory factory) : base(factory) { }
			public DummyBusinessObjectAllowNewToggleCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter) { }

			protected override bool AllowNewCore
			{
				get
				{
					return Count < 2;
				}
			}

			protected override BusinessObject CreateInitialisedBusinessObjectFromRow(DataRow row)
			{
				ActionForCreateInitialisedBusinessObjectFromRow?.Invoke();
				return base.CreateInitialisedBusinessObjectFromRow(row);
			}

			public Action ActionForCreateInitialisedBusinessObjectFromRow { get; set; }
		}

		public void TestCallingRefreshBindingWhileCopyPreviousRow()
		{
			var obj1 = Factory.New<DummyBusinessObject>();
			obj1.Z0_Code = "z1";
			Factory.Save();

			var dummy1 = Factory.New<DummyChildEnterpriseBusinessObject>();
			Dummy.Collection.Add(dummy1);
			dummy1.Z0_Code = "111";
			dummy1.Z0_Description = "ONE";
			dummy1.Z0_NVarCharMax = "1";
			dummy1.Z0_VarCharMax = "11111";
			dummy1.Z0_Guid = obj1.PK;

			using (var form = new ZForm(Dummy))
			{
				form.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 500);

				var grid = new ZTestGrid();
				grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
				grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 200);
				form.Controls.Add(grid);

				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Code });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Description });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_NVarCharMax, IsReadOnly = true });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_VarCharMax });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "RelatedDummy+" + DummyBizoSchema.Constants.Z0_Code });

				form.Show();
				grid.SetDataBinding(Dummy, "Collection");

				AssertEquals("Precondition: ", 5, grid.ColumnStyles.Count);

				grid.CurrentCell = new DataGridCell(0, 0);

				FocusAndEditGrid(grid);
				AssertEquals("Precondition", new DataGridCell(0, 0), grid.CurrentCell);

				grid.LastFocusedColumn.TextBox.Visible = true;

				grid.CurrentCell = new DataGridCell(1, 0);
				AssertEquals("A new row should be added when entering new cell", 2, Dummy.Collection.Count);
				(Dummy.Collection[1]).Z0_CodeInfo.ValueChanged += Z0_CodeInfo_ValueChanged;
				PostKeyToGrid(grid, Keys.F9);
				AssertEquals("New row should not be removed after pressing F9", 2, Dummy.Collection.Count);
			}
		}

		public void TestExpectedPropertyInfoWhileCopyPrevious()
		{
			var dummy1 = Factory.New<DummyBusinessObjectWithCustomField>();
			Dummy.Collection.Add(dummy1);

			var dummy2 = Factory.New<DummyBusinessObjectWithCustomField>();
			Dummy.Collection.Add(dummy2);

			using (var form = new ZForm(Dummy))
			{
				form.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 500);

				var grid = new ZTestGrid();
				grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
				grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 200);
				form.Controls.Add(grid);

				var cusStyleInfo = new ZTextBoxColumnStyleInfo { ColumnName = "STR" };
				((IOverridablePropertyDescriptor)cusStyleInfo).PropertyDescriptor = new NullPropertyDescriptor("STR", null);
				grid.ColumnStyles.Add(cusStyleInfo);

				form.Show();
				grid.SetDataBinding(Dummy, "Collection");
				AssertEquals("Precondition: ", 1, grid.ColumnStyles.Count);

				grid.CurrentCell = new DataGridCell(1, 0);
				Application.DoEvents();
				AssertEquals("Precondition", new DataGridCell(1, 0), grid.CurrentCell);

				PostKeyToGrid(grid, Keys.F9);
				AssertEquals("Current property info is null. Column propertyDescriptor: Enterprise.ZArchitecture.GUI.Testing.ZGridTest+NullPropertyDescriptor, column mapping name: STR", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		void Z0_CodeInfo_ValueChanged(object sender, EventArgs e)
		{
			Dummy.Collection.RefreshBinding();
		}

#if !WINZOR

		public void TestDragDropRow()
		{
			using (var form = new ZForm(Dummy))
			{
				for (var i = 0; i < 20; i++)
				{
					Dummy.Collection.AddNew();
				}

				var grid = InitialiseFormAndCreateGrid(form, true);
				grid.Top = ControlDpiScalingHelper.ScaleToCurrentDpiY(30);

				form.Show();

				grid.SetIsClickingRowHeader(true);
				grid.CopySelectedRowsAllowed = false;
				grid.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, ControlDpiScalingHelper.ScaleToCurrentDpiX(15), ControlDpiScalingHelper.ScaleToCurrentDpiY(145), 0));
				Assert(!grid.IsDragging);
			}
		}

		public void TestDragDropColumn()
		{
			using (var form = new ZForm(Dummy))
			{
				for (var i = 0; i < 20; i++)
				{
					Dummy.Collection.AddNew();
				}

				var grid = InitialiseFormAndCreateGrid(form, true);
				grid.Top = ControlDpiScalingHelper.ScaleToCurrentDpiY(30);

				form.Show();

				grid.SetIsClickingColumnHeader(true);
				grid.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, ControlDpiScalingHelper.ScaleToCurrentDpiX(60), ControlDpiScalingHelper.ScaleToCurrentDpiY(15), 0));
				Assert(!grid.IsDragging);
			}
		}

		public void TestInvalidateLayoutShouldBeCalledAfterBaseMouseWheeling()
		{
			using (var form = new ZForm(Dummy))
			{
				var grid = InitialiseFormAndCreateGrid(form, true);
				form.Show();

				var mouseWhellWasCalled = false;
				var invalidateWasCalled = false;

				grid.MouseWheel += (_, __) => { mouseWhellWasCalled = true; };
				grid.Invalidated += (_, __) =>
				{
					invalidateWasCalled = true;
					Assert("MouseWhell should have be called", mouseWhellWasCalled);
				};

				grid.OnMouseWhell(new MouseEventArgs(MouseButtons.Middle, 0, 0, 0, 1));

				Assert("MouseWhell should be called", mouseWhellWasCalled);
				AssertEquals("Invalidate should NOT be called when delta is 0 or more", false, invalidateWasCalled);

				mouseWhellWasCalled = false;

				grid.OnMouseWhell(new MouseEventArgs(MouseButtons.Middle, 0, 0, 0, 0));

				Assert("MouseWhell should be called", mouseWhellWasCalled);
				AssertEquals("Invalidate should NOT be called when delta is 0 or more", false, invalidateWasCalled);

				mouseWhellWasCalled = false;

				grid.OnMouseWhell(new MouseEventArgs(MouseButtons.Middle, 0, 0, 0, -1));

				Assert("MouseWhell should be called", mouseWhellWasCalled);
				Assert("Invalidate should be called when delta is less then 0", invalidateWasCalled);

				mouseWhellWasCalled = false;
				invalidateWasCalled = false;

				grid.OnMouseWhell(new MouseEventArgs(MouseButtons.Middle, 0, 0, 0, -10));

				Assert("MouseWhell should be called", mouseWhellWasCalled);
				Assert("Invalidate should be called when delta is less then 0", invalidateWasCalled);
			}
		}

		public void TestInvalidateLayoutOnEndEditWhenMouseWheeling()
		{
			using (var form = new ZForm(Dummy))
			{
				var grid = InitialiseFormAndCreateGrid(form, true);
				grid.MouseWheel += (_, __) => { grid.EndEdit(); };
				form.Show();

				var invalidateWasCalled = false;
				grid.Invalidated += (_, __) => { invalidateWasCalled = true; };
				grid.OnMouseWhell(new MouseEventArgs(MouseButtons.Middle, 0, 0, 0, 10));

				Assert("Invalidate should be called when end edit while mouseWheeling to redraw grid", invalidateWasCalled);
			}
		}

#endif

		void AssertGridResults(string assertionMessage, DataGridCell expectedCellLocation, ZGrid grid, params string[] cellValues)
		{
			AssertEquals(assertionMessage + " - grid.CurrentCell", expectedCellLocation, grid.CurrentCell);

			for (var counter = 0; counter < cellValues.Length; counter += 3)
			{
				var rowIndex = counter / 3;
				ZString expectedZ0_Code = cellValues[counter];
				ZString expectedZ0_Description = cellValues[counter + 1];
				ZString expectedRelatedZ0_Code = cellValues[counter + 2];
				AssertEquals(assertionMessage + " (" + expectedCellLocation.ToString() + ") [" + rowIndex.ToString() + "].Z0_Code", expectedZ0_Code, Dummy.Collection[rowIndex].Z0_Code);
				AssertEquals(assertionMessage + " (" + expectedCellLocation.ToString() + ") [" + rowIndex.ToString() + "].Z0_Description", expectedZ0_Description, Dummy.Collection[rowIndex].Z0_Description);
				AssertEquals(assertionMessage + " (" + expectedCellLocation.ToString() + ") [" + rowIndex.ToString() + "].RealtedDummy+Z0_Code", expectedRelatedZ0_Code, Dummy.Collection[rowIndex].RelatedDummy.Z0_Code);
			}
		}

		internal static void PostKeyToGrid(ZGrid grid, Keys key)
		{
			KeySender.PostKeyDown(grid.LastFocusedColumn.EditControl, key);
			Application.DoEvents();
		}

		public void TestIsErroredEnablesMenuOption()
		{
			using (var testForm = new ZForm(Dummy))
			{
				testForm.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 500);

				var masterGrid = new ZGrid { Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0), Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 200) };
				testForm.Controls.Add(masterGrid);
				masterGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Description });
				masterGrid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Number });

				testForm.Show();
				masterGrid.IsErrored = false;
				var noErrorCount = masterGrid.ContextMenu.MenuItems.Count;

				masterGrid.IsErrored = true;
				masterGrid.OnPopup_CallForTesting();

				var withErrorCount = masterGrid.ContextMenu.MenuItems.Count;
				Assert(withErrorCount > noErrorCount);

				masterGrid.ContextMenu.MenuItems[withErrorCount - 1].PerformClick();
				AssertEquals(false, masterGrid.IsErrored);

				masterGrid.OnPopup_CallForTesting();
				AssertEquals(noErrorCount, masterGrid.ContextMenu.MenuItems.Count);
			}
		}

		public void TestReOrderColumns()
		{
			using (var testForm = new ZForm(Dummy))
			{
				var masterGrid = new ZGrid();
				testForm.Controls.Add(masterGrid);
				var info1 = new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Description };
				var info2 = new ZCalcEditColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Number };
				var info3 = new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_VarCharMax };
				masterGrid.ColumnStyles.Add(info1);
				masterGrid.ColumnStyles.Add(info2);
				masterGrid.ColumnStyles.Add(info3);

				masterGrid.ReOrderColumns([DummyBizoSchema.Constants.Z0_Number, DummyBizoSchema.Constants.Z0_VarCharMax]);

				AssertEquals("Info2", info2, masterGrid.ColumnStyles[0]);
				AssertEquals("Info3", info3, masterGrid.ColumnStyles[1]);
				AssertEquals("Info1", info1, masterGrid.ColumnStyles[2]);

				masterGrid.SetDataBinding(Dummy, "Collection", Dummy.Collection.GetType().Name);
				testForm.Show();

				masterGrid.ReOrderColumns([DummyBizoSchema.Constants.Z0_NVarCharMax]);
				AssertEquals("Info2", info2, masterGrid.ColumnStyles[0]);
				AssertEquals("Info3", info3, masterGrid.ColumnStyles[1]);
				AssertEquals("Info1", info1, masterGrid.ColumnStyles[2]);

				masterGrid.ReOrderColumns([DummyBizoSchema.Constants.Z0_Description]);
				AssertEquals("Info1", info1, masterGrid.ColumnStyles[0]);
				AssertEquals("Info2", info2, masterGrid.ColumnStyles[1]);
				AssertEquals("Info3", info3, masterGrid.ColumnStyles[2]);
			}
		}

		public void TestAddOrRemoveColumnsFromAvailableColumns()
		{
			using (var testForm = new ZForm(Dummy))
			{
				var masterGrid = new ZGrid();
				testForm.Controls.Add(masterGrid);
				masterGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Description });
				masterGrid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Number });
				masterGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_VarCharMax });

				masterGrid.SetDataBinding(Dummy, "Collection", Dummy.Collection.GetType().Name);
				testForm.Show();
				masterGrid.RemoveFromAvailableColumns(DummyBizoSchema.Constants.Z0_Number, DummyBizoSchema.Constants.Z0_VarCharMax);
				AssertEquals(1, masterGrid.Columns.Count);
				AssertEquals(DummyBizoSchema.Constants.Z0_Description, masterGrid.Columns[0].ColumnStyle.MappingName);

				masterGrid.AddToAvailableColumns(DummyBizoSchema.Constants.Z0_VarCharMax);
				AssertEquals(2, masterGrid.Columns.Count);
				AssertEquals(DummyBizoSchema.Constants.Z0_Description, masterGrid.Columns[0].ColumnStyle.MappingName);
				AssertEquals(DummyBizoSchema.Constants.Z0_VarCharMax, masterGrid.Columns[1].ColumnStyle.MappingName);

				masterGrid.RemoveFromAvailableColumns(DummyBizoSchema.Constants.Z0_Description, DummyBizoSchema.Constants.Z0_VarCharMax);
				masterGrid.AddToAvailableColumns(DummyBizoSchema.Constants.Z0_VarCharMax, DummyBizoSchema.Constants.Z0_Number);
				masterGrid.ResetColumns();

				AssertEquals(2, masterGrid.Columns.Count);
				AssertEquals(DummyBizoSchema.Constants.Z0_VarCharMax, masterGrid.Columns[0].ColumnStyle.MappingName);
				AssertEquals(DummyBizoSchema.Constants.Z0_Number, masterGrid.Columns[1].ColumnStyle.MappingName);
			}
		}

		public void TestAddColumn()
		{
			using (var testForm = new ZForm(Dummy))
			{
				var masterGrid = new ZGrid();
				testForm.Controls.Add(masterGrid);
				masterGrid.AddColumn(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Description });
				masterGrid.AddColumn(new ZCalcEditColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Number });

				masterGrid.SetDataBinding(Dummy, "Collection", Dummy.Collection.GetType().Name);

				AssertEquals(2, masterGrid.Columns.Count);
				AssertEquals(2, masterGrid.DefaultColumns.Count);

				AssertEquals(DummyBizoSchema.Constants.Z0_Description, masterGrid.Columns[0].ColumnStyle.MappingName);
				AssertEquals(DummyBizoSchema.Constants.Z0_Number, masterGrid.Columns[1].ColumnStyle.MappingName);

				AssertEquals(DummyBizoSchema.Constants.Z0_Description, masterGrid.DefaultColumns[0].ColumnStyle.MappingName);
				AssertEquals(DummyBizoSchema.Constants.Z0_Number, masterGrid.DefaultColumns[1].ColumnStyle.MappingName);
			}
		}

		public void TestRemoveAndDisposeColumn()
		{
			using (var testForm = new ZForm(Dummy))
			{
				var masterGrid = new ZGrid();
				testForm.Controls.Add(masterGrid);
				masterGrid.AddColumn(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Description });
				masterGrid.AddColumn(new ZCalcEditColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Number });

				masterGrid.SetDataBinding(Dummy, "Collection", Dummy.Collection.GetType().Name);

				AssertEquals(2, masterGrid.Columns.Count);
				AssertEquals(2, masterGrid.DefaultColumns.Count);

				var columnStyleToRemove = masterGrid.Columns[DummyBizoSchema.Constants.Z0_Description].ColumnStyle as ZTextBoxColumnStyle;
				var editControl = columnStyleToRemove.EditControl;

				masterGrid.RemoveAndDisposeColumn(DummyBizoSchema.Constants.Z0_Description);

				Assert("ColumnStyle & EditControl should be disposed", editControl.IsDisposed);
				AssertEquals(1, masterGrid.Columns.Count);
				AssertEquals(1, masterGrid.DefaultColumns.Count);
				AssertEquals(DummyBizoSchema.Constants.Z0_Number, masterGrid.Columns[0].ColumnStyle.MappingName);
				AssertEquals(DummyBizoSchema.Constants.Z0_Number, masterGrid.DefaultColumns[0].ColumnStyle.MappingName);
			}
		}

		public void TestSetIndexFromSearchString()
		{
			using (var form = new ZForm(Dummy))
			{
				DummyChildBusinessObject dummyBizo1 = Dummy.Collection.AddNew();
				DummyChildBusinessObject dummyBizo2 = Dummy.Collection.AddNew();
				DummyChildBusinessObject dummyBizo3 = Dummy.Collection.AddNew();
				DummyChildBusinessObject dummyBizo4 = Dummy.Collection.AddNew();
				DummyChildBusinessObject dummyBizo5 = Dummy.Collection.AddNew();
				DummyChildBusinessObject dummyBizo6 = Dummy.Collection.AddNew();
				dummyBizo3.Z0_Description = "CuckooSqueaker";
				dummyBizo6.Z0_Description = "Notbob";

				form.Controls.Add(new TextBox());
				form.Show();
				var grid = InitialiseFormAndCreateGrid(form, true);

				AssertEquals(0, grid.CurrentRowIndex);
				grid.SetIndexFromSearchString("Cuckoo", "Z0_Description");
				AssertEquals(2, grid.CurrentRowIndex);
				grid.SetIndexFromSearchString("Notbob", "Z0_Description");
				AssertEquals(5, grid.CurrentRowIndex);
				grid.SetIndexFromSearchString("Gibbiceps", "Z0_Description");
				AssertEquals("No result search should not have selected a different column", 5, grid.CurrentRowIndex);
			}
		}

		public void TestSelectRowAfterScrolling()
		{
			using (var form = new ZForm(Dummy))
			{
				for (var i = 0; i < 20; i++)
				{
					Dummy.Collection.AddNew();
				}

				// having a read only column as the first column means that during OnEnter it will (if the bug is present)
				// set the CurrentCell to 0, 1 and will scroll to ensure it is visible, making the wrong row get selected

				form.Controls.Add(new TextBox());
				var grid = InitialiseFormAndCreateGrid(form, true);
				grid.Top = ControlDpiScalingHelper.ScaleToCurrentDpiY(30);

				form.Show();
				grid.GridVScrolled(null, new ScrollEventArgs(ScrollEventType.LargeIncrement, 20));
				var hti = grid.HitTest(ControlDpiScalingHelper.ScaleToCurrentDpiX(15), ControlDpiScalingHelper.ScaleToCurrentDpiY(145));
				AssertEquals("PreCondition: 145 clicks Row 19", 19, hti.Row);
				AssertEquals("PreCondition: 15 clicks the RowHeader", DataGrid.HitTestType.RowHeader, hti.Type);
				grid.SetMouseButtonState(MouseButtons.Left);
				grid.SetMousePosition(CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 145));
				grid.OnEnter(EventArgs.Empty);
				grid.FireMouseDown(ControlDpiScalingHelper.ScaleToCurrentDpiX(15), ControlDpiScalingHelper.ScaleToCurrentDpiY(145));
				grid.SetMouseButtonState(MouseButtons.None);
				grid.SetMousePosition(Point.Empty);

				AssertEquals(1, grid.SelectedRowCount);

				for (var i = 0; i < grid.List.Count; i++)
				{
					if (i != 19)
					{
						Assert("Row " + i + " should not be selected", !grid.IsSelected(i));
					}
				}

				Assert("Row 19 should be selected", grid.IsSelected(19));
			}

			Assert(true);
		}

		[ExpectNoExceptions]
		public void TestChangePositionCausesCancelElementDoesNotThrow()
		{
			using (var form = new ZForm(Dummy))
			{
				var grid = InitialiseFormAndCreateGrid(form);
				form.Show();

				// add a committed element
				Dummy.Collection.AddNew();

				// add a new uncommitted element by editing the next row
				grid.CurrentCell = new DataGridCell(1, 0);

				// setting the position will cause the uncommitted element to be cancelled but because it is nested
				// in the position changed event it will stop the notification from propogating to the grid.
				grid.ListManager.Position = 0;

				Application.DoEvents(); // allow repaint to be processed
			}
		}

		public void TestPositionChangedDoesNotResetSelection()
		{
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();

			using (var form = new ZForm(Dummy))
			{
				var grid = InitialiseFormAndCreateGrid(form);
				form.Show();

				grid.Select(0);
				grid.Select(1);
				grid.Select(2);
				grid.Select(3);

				grid.ListManager.Position = 3;

				Assert(grid.IsSelected(0));
				Assert(grid.IsSelected(1));
				Assert(grid.IsSelected(2));
				Assert(grid.IsSelected(3));
			}
		}

		ZTestGrid InitialiseFormAndCreateGrid(ZForm testForm)
		{
			return InitialiseFormAndCreateGrid(testForm, false);
		}

		ZTestGrid InitialiseFormAndCreateGrid(ZForm testForm, bool addSecondColumn)
		{
			testForm.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 500);

			var grid = new ZTestGrid { Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0), Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 200) };
			testForm.Controls.Add(grid);

			if (addSecondColumn)
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Code, IsReadOnly = true });
			}
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Description });
			grid.SetDataBinding(Dummy, "Collection");

			return grid;
		}

		#region Row Finder

		public void TestFindTextMenuItemsExist()
		{
			using (var grid = new ZGrid())
			{
				var findMenuItem = FindMenuItem(grid, "&Find");
				AssertNotNull("The 'Find' menu item was not found on the ZGrid context menu.", findMenuItem);
				AssertEquals("The 'Find' menu item should be visible on the ZGrid context menu.", true, findMenuItem.Visible);

				var findNextMenuItem = FindMenuItem(grid, "Find &Next");
				AssertNotNull("The 'Find Next' menu item was not found on the ZGrid context menu.", findNextMenuItem);
				AssertEquals("The 'Find Next' menu item should be visible on the ZGrid context menu.", true, findNextMenuItem.Visible);
			}
		}

		#endregion

		#region Excel tests

		public void TestExportToExcelMenuItemExists()
		{
			using (var grid = new ZGrid())
			{
				var excelExportAllColumnsMenuItem = FindMenuItem(grid, "Export All Columns To Excel");
				AssertNotNull("The 'Export All Columns To Excel' menu item was not found on the ZGrid context menu.", excelExportAllColumnsMenuItem);
				AssertEquals("The 'Export All Columns To Excel' menu item should be visible on the ZGrid context menu.", true, excelExportAllColumnsMenuItem.Visible);

				var excelExportVisibleColumnsMenuItem = FindMenuItem(grid, "Export Visible Columns To Excel");
				AssertNotNull("The 'Export Visible Columns To Excel' menu item was not found on the ZGrid context menu.", excelExportVisibleColumnsMenuItem);
				AssertEquals("The 'Export Visible Columns To Excel' menu item should be visible on the ZGrid context menu.", true, excelExportVisibleColumnsMenuItem.Visible);
			}
		}

		public void TestExportToExcelMenuItem_ExportAllColumns()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();

			using (var form = new ZForm(dummy))
			using (var grid = new TestZGrid())
			{
				grid.BindTo = "Collection";
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBusinessObject.Schema.Z0_Code });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBusinessObject.Schema.Z0_Decimal });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBusinessObject.Schema.Z0_NVarChar, IsVisible = false });
				grid.ColumnStyles.Add(new ZDateEditColumnStyleInfo { ColumnName = DummyBusinessObject.Schema.Z0_Date });
				form.Controls.Add(grid);
				grid.SetDataBinding(dummy, "Collection", dummy.Collection.GetType().Name);
				form.Show();
				Application.DoEvents();

				try
				{
					var menuAllColumns = grid.ContextMenu.MenuItems.FindByText("Export All Columns To Excel");
					AssertNotNull(menuAllColumns);
					menuAllColumns.PerformClick();

					var exportedAllColumns = grid.lastExporter.LastExportedColumnsForTest;
					AssertEquals(4, exportedAllColumns.Count);
					AssertNotNull("Should export Z0_Code column", exportedAllColumns[DummyBusinessObject.Schema.Z0_Code]);
					AssertNotNull("Should export Z0_Decimal column", exportedAllColumns[DummyBusinessObject.Schema.Z0_Decimal]);
					AssertNotNull("Should export Z0_NVarChar column", exportedAllColumns[DummyBusinessObject.Schema.Z0_NVarChar]);
					AssertNotNull("Should export Z0_Date column", exportedAllColumns[DummyBusinessObject.Schema.Z0_Date]);
				}
				finally
				{
					DeleteIfExists(grid.lastExporter.LastExportedFileNameForTest);
				}
			}
		}

		public void TestExportToExcelMenuItem_ExportVisibleColumns()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();

			using (var form = new ZForm(dummy))
			using (var grid = new TestZGrid())
			{
				grid.BindTo = "Collection";
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBusinessObject.Schema.Z0_Code });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBusinessObject.Schema.Z0_Decimal });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBusinessObject.Schema.Z0_NVarChar, IsVisible = false });
				grid.ColumnStyles.Add(new ZDateEditColumnStyleInfo { ColumnName = DummyBusinessObject.Schema.Z0_Date });
				form.Controls.Add(grid);
				grid.SetDataBinding(dummy, "Collection", dummy.Collection.GetType().Name);
				form.Show();
				Application.DoEvents();

				try
				{
					var menuVisibleColumns = grid.ContextMenu.MenuItems.FindByText("Export Visible Columns To Excel");
					AssertNotNull(menuVisibleColumns);
					menuVisibleColumns.PerformClick();

					var exportedVisibleColumns = grid.lastExporter.LastExportedColumnsForTest;
					AssertEquals(3, exportedVisibleColumns.Count);
					AssertNotNull("Should export Z0_Code column", exportedVisibleColumns[DummyBusinessObject.Schema.Z0_Code]);
					AssertNotNull("Should export Z0_Decimal column", exportedVisibleColumns[DummyBusinessObject.Schema.Z0_Decimal]);
					AssertNull("Shouldn't export Z0_NVarChar column", exportedVisibleColumns[DummyBusinessObject.Schema.Z0_NVarChar]);
					AssertNotNull("Should export Z0_Date column", exportedVisibleColumns[DummyBusinessObject.Schema.Z0_Date]);
				}
				finally
				{
					DeleteIfExists(grid.lastExporter.LastExportedFileNameForTest);
				}
			}
		}

		#endregion

		#region ContextMenu Tests

		public void TestShortcutKeys_WhenSearchingForMenuItem_ShouldSkipSearchingSubMenuItems_ForMenuItemsWithExclusionAttribute()
		{
			var alwaysLoadsMenuItemPopupCount = 0;
			var neverLoadsMenuItemPopupCount = 0;

			using (var grid = new ZGrid())
			{
				var menuItemThatAllowsPopUpForShortcuts = new MenuItem((NoResString)string.Empty);
				menuItemThatAllowsPopUpForShortcuts.Popup += (sender, args) => alwaysLoadsMenuItemPopupCount++;
				menuItemThatAllowsPopUpForShortcuts.MenuItems.Add((NoResString)string.Empty);
				grid.ContextMenu.MenuItems.Add(menuItemThatAllowsPopUpForShortcuts);

				var menuItemThatDoesNotAllowPopUpForShortcuts = new MenuItemThatShouldNotPopUpToCheckShortcuts_ForTest((NoResString)string.Empty);
				menuItemThatDoesNotAllowPopUpForShortcuts.Popup += (sender, args) => neverLoadsMenuItemPopupCount++;
				menuItemThatDoesNotAllowPopUpForShortcuts.MenuItems.Add((NoResString)string.Empty);
				grid.ContextMenu.MenuItems.Add(menuItemThatDoesNotAllowPopUpForShortcuts);

				KeySender.SendKeyDownToProcessCmdKey(grid, Keys.Control | Keys.Shift | Keys.S);
				Application.DoEvents();

				AssertEquals("The first menu item doesn't have the attribute, so it should have allowed the grid to simulate its popup in order to check for shortcuts. SAD!", 1, alwaysLoadsMenuItemPopupCount);
				AssertEquals("The second menu item has the attribute, so it should not have allowed the grid to simulate its popup in order to check for shortcuts. SAD!", 0, neverLoadsMenuItemPopupCount);
			}
		}

		[DoNotPopUpToCheckShortcuts]
		class MenuItemThatShouldNotPopUpToCheckShortcuts_ForTest : MenuItem
		{
			public MenuItemThatShouldNotPopUpToCheckShortcuts_ForTest(string text)
				: base(text)
			{
			}
		}

		public void TestAddDuplicateSelectedRowsMenuItem()
		{
			using (var grid = new ZGrid())
			{
				AssertNull(FindMenuItem(grid, "Du&plicate"));
				grid.AddDuplicateSelectedRowsMenuItem();
				AssertNotNull(FindMenuItem(grid, "Du&plicate"));
				grid.AddDuplicateSelectedRowsMenuItem();
				AssertEquals("MenuItem should not be added more than once", 1, CountMenuItem(grid, "Du&plicate"));
			}
		}

		public void TestDuplicateSelectedRowsMenuItemClick()
		{
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();

			using (var form = new ZForm(Dummy))
			{
				var grid = InitialiseFormAndCreateGrid(form);
				form.Show();
				AssertEquals("List should have 3 rows ", 3, grid.List.Count);

				Dummy.Collection[0].Z0_Description = "0";
				Dummy.Collection[1].Z0_Description = "1";
				Dummy.Collection[2].Z0_Description = "2";

				// test multiple selected rows duplication
				grid.Select(1);
				grid.Select(2);

				grid.AddDuplicateSelectedRowsMenuItem();
				var menuItem = FindMenuItem(grid, "Du&plicate");

				menuItem.PerformClick();
				AssertEquals("List should have original rows plus duplicate rows", 5, grid.List.Count);
				AssertEquals("Row 1 should be cloned", "1", Dummy.Collection[3].Z0_Description);
				AssertEquals("Row 2 should be cloned", "2", Dummy.Collection[4].Z0_Description);
			}
		}

		public void TestDuplicateSelectedRowsMenuItemClickWhenNotAllowNew()
		{
			Dummy.Collection.AddNew();
			Dummy.Collection.SetAllowNew(false);

			using (var form = new ZForm(Dummy))
			{
				var grid = InitialiseFormAndCreateGrid(form);
				form.Show();
				AssertEquals("List should have 1 row", 1, grid.List.Count);

				grid.Select(0);
				grid.AddDuplicateSelectedRowsMenuItem();
				var menuItem = FindMenuItem(grid, "Du&plicate");

				menuItem.PerformClick();
				AssertEquals("List should not have changed", 1, grid.List.Count);
			}
		}

		[ExpectNoExceptions]
		public void TestDuplicateSelectedRowsMenuItemClickWithNullList()
		{
			using (var grid = new ZGrid())
			{
				grid.AddDuplicateSelectedRowsMenuItem();
				AssertNull("Precondition", grid.List);
				FindMenuItem(grid, "Du&plicate").PerformClick();
			}
		}

		public void TestDuplicateSelectedRowsWithUncommitted()
		{
			var collection = new ActiveBusinessObjectCollection<DummyDependantCloneableBizo>(Factory, Dummy);

			using (var form = new ZForm(Dummy) { Size = ControlDpiScalingHelper.NewScaledSize(300, 300) })
			{
				var grid = new ZTestGrid { Location = ControlDpiScalingHelper.NewScaledPoint(0, 0), Size = ControlDpiScalingHelper.NewScaledSize(200, 200) };
				form.Controls.Add(grid);
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyDependentBizoSchema.Constants.ZD1_Code });
				grid.SetDataBinding(collection, "");

				form.Show();
				grid.CurrentCell = new DataGridCell(0, 0);
				Application.DoEvents();

				collection.DeleteAll(); // Remove any other elements
				grid.ListManager.AddNew(); // Add uncommitted element

				AssertEquals("Collection should have 1 uncommitted row", 1, collection.Count);
				Assert("Collection should have 1 uncommitted row", ((IBusinessObjectInternals)collection[0]).IsUnCommittedRow);

				grid.Select(0);
				Assert(((IBusinessObjectInternals)grid.SelectedElements[0]).IsUnCommittedRow);

				grid.AddDuplicateSelectedRowsMenuItem();
				var menuItem = FindMenuItem(grid, "Du&plicate");
				menuItem.PerformClick();

				AssertEquals("New element should be added", 2, grid.List.Count);
				Assert("All elements should be committed", !((IBusinessObjectInternals)grid.List[0]).IsUnCommittedRow);
				Assert("All elements should be committed", !((IBusinessObjectInternals)grid.List[1]).IsUnCommittedRow);

				grid.CurrentCell = new DataGridCell(2, 0); // Go to new row
				AssertEquals("There should be only 1 element added - new uncommitted for row #2 in the grid", 3, grid.List.Count);
			}
		}

		class DummyDependantCloneableBizo : DummyDependantBusinessObject
		{
			public DummyDependantCloneableBizo(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			protected override bool SupportsCloneCore()
			{
				return true;
			}
		}

		static MenuItem FindMenuItem(ZGrid grid, string itemName)
		{
			MenuItem result = null;
			foreach (MenuItem item in grid.ContextMenu.MenuItems)
			{
				if (item.Text == itemName)
				{
					result = item;
					break;
				}
			}
			return result;
		}

		static int CountMenuItem(ZGrid grid, string itemName)
		{
			var result = 0;
			foreach (MenuItem item in grid.ContextMenu.MenuItems)
			{
				if (item.Text == itemName)
				{
					result++;
				}
			}
			return result;
		}

		#endregion

		public void TestRMBRowHeaderWhileFocusingSelectsRow()
		{
			using (var testForm = new ZForm(Dummy))
			{
				testForm.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 500);

				var testGrid = new ZTestGrid { Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0), Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 200), IsWholeRowSelectedOnClick = true };
				testForm.Controls.Add(testGrid);
				var button = new ZButton();
				testForm.Controls.Add(button);

				testGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Description });
				testGrid.SetDataBinding(Dummy, "Collection", Dummy.Collection.GetType().Name);

				testForm.Show();

				button.Focus();

				var mouseArgs = new MouseEventArgs(MouseButtons.Right, 1, ControlDpiScalingHelper.ScaleToCurrentDpiX(1), ControlDpiScalingHelper.ScaleToCurrentDpiY(30), 0);
				var hti = testGrid.HitTest(mouseArgs.X, mouseArgs.Y);
				AssertEquals("Should click first row", 0, hti.Row);
				Assert("Should click row header", hti.Type == DataGrid.HitTestType.RowHeader);

				AssertEquals(0, testGrid.SelectedElements.Length);
				Assert(!testGrid.Focused);

				testGrid.OnMouseDown(mouseArgs); // bug in OSelectableGrid would cause IndexOutOfRange exceptions in OnPaint
				Application.DoEvents();

				AssertEquals(1, testGrid.SelectedElements.Length);
				Assert(testGrid.Focused);
			}
		}

		public void TestMouseDownOnEditableGridWithWholeRowSelect()
		{
			Assert("Precondition: No UnitTestUserNotifications", UnitTestUserNotification.Instance.LastMessage.WasNone);

			using (var testForm = new ZForm(Dummy))
			{
				testForm.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 500);

				var testGrid = new ZTestGrid { Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0), Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 200), IsWholeRowSelectedOnClick = true };
				testForm.Controls.Add(testGrid);

				testGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Description });
				testGrid.SetDataBinding(Dummy, "Collection", Dummy.Collection.GetType().Name);

				testForm.Show();

				var mouseArgs = new MouseEventArgs(MouseButtons.Left, 1, ControlDpiScalingHelper.ScaleToCurrentDpiX(40), ControlDpiScalingHelper.ScaleToCurrentDpiY(30), 0);
				var hti = testGrid.HitTest(mouseArgs.X, mouseArgs.Y);
				AssertEquals("Should click first row", 0, hti.Row);
				Assert("Shouldn't click row header", hti.Type != DataGrid.HitTestType.RowHeader);

				testGrid.OnMouseDown(mouseArgs); // bug in OSelectableGrid would cause IndexOutOfRange exceptions in OnPaint
				Application.DoEvents();

				Assert("No UnitTestUserNotifications", UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestElementTypeFromCollection()
		{
			using (var testForm = new ZForm(Dummy))
			{
				testForm.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 500);

				var masterGrid = new ZTestGrid();
				testForm.Controls.Add(masterGrid);

				masterGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Description });
				masterGrid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Number });

				masterGrid.SetDataBinding(Dummy, "Collection", Dummy.Collection.GetType().Name);
				testForm.Show();
				Application.DoEvents();

				AssertEquals("Should return the correct ElementTypeFromCollection", typeof(DummyChildEnterpriseBusinessObject), masterGrid.ElementTypeFromCollection);
			}
		}

		public void TestGetSetColumnWidth()
		{
			using (var testForm = new ZForm(Dummy))
			{
				testForm.Size = new Size(300, 500);

				var masterGrid = new ZGrid { Location = new Point(0, 0), Size = new Size(200, 200) };
				testForm.Controls.Add(masterGrid);
				var info1 = new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Description, Width = 50 };
				var info2 = new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Number, Width = 30 };
				masterGrid.ColumnStyles.Add(info1);
				masterGrid.ColumnStyles.Add(info2);

				AssertEquals("GetColumnWidth", 50, masterGrid.GetColumnWidth(DummyBizoSchema.Constants.Z0_Description));
				AssertEquals("GetColumnWidth", 30, masterGrid.GetColumnWidth(DummyBizoSchema.Constants.Z0_Number));

				masterGrid.SetColumnWidth(info1.ColumnName, 60);
				masterGrid.SetColumnWidth(info2.ColumnName, 35);

				AssertEquals("Column width set", ControlDpiScalingHelper.ScaleToCurrentDpiX(60), info1.Width);
				AssertEquals("Column width set", ControlDpiScalingHelper.ScaleToCurrentDpiX(35), info2.Width);

				AssertEquals("GetColumnWidth", ControlDpiScalingHelper.ScaleToCurrentDpiX(60), masterGrid.GetColumnWidth(DummyBizoSchema.Constants.Z0_Description));
				AssertEquals("GetColumnWidth", ControlDpiScalingHelper.ScaleToCurrentDpiX(35), masterGrid.GetColumnWidth(DummyBizoSchema.Constants.Z0_Number));

				masterGrid.SetDataBinding(Dummy, "Collection", Dummy.Collection.GetType().Name);
				testForm.Show();

				masterGrid.SetColumnWidth(info1.ColumnName, 70);
				masterGrid.SetColumnWidth(info2.ColumnName, 40);

				AssertEquals("Column width set", ControlDpiScalingHelper.ScaleToCurrentDpiX(70), masterGrid.Columns[info1.ColumnName].ColumnStyle.Width);
				AssertEquals("Column width set", ControlDpiScalingHelper.ScaleToCurrentDpiX(40), masterGrid.Columns[info2.ColumnName].ColumnStyle.Width);

				AssertEquals("GetColumnWidth", ControlDpiScalingHelper.ScaleToCurrentDpiX(70), masterGrid.GetColumnWidth(DummyBizoSchema.Constants.Z0_Description));
				AssertEquals("GetColumnWidth", ControlDpiScalingHelper.ScaleToCurrentDpiX(40), masterGrid.GetColumnWidth(DummyBizoSchema.Constants.Z0_Number));

				AssertEquals("GetColumnWidth", ControlDpiScalingHelper.ScaleToCurrentDpiX(0), masterGrid.GetColumnWidth("IDon'tExist"));
			}
		}

		public void TestGetSetColumnCaption()
		{
			using (var testForm = new ZForm(Dummy))
			{
				testForm.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 500);

				var masterGrid = new ZGrid();
				masterGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
				masterGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 200);
				testForm.Controls.Add(masterGrid);
				var info1 = new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Description };
				var info2 = new ZCalcEditColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Number };
				masterGrid.ColumnStyles.Add(info1);
				masterGrid.ColumnStyles.Add(info2);

				masterGrid.SetColumnCaption(info1.ColumnName, "One");
				masterGrid.SetColumnCaption(info2.ColumnName, "Two");

				AssertEquals("Column caption set", "One", info1.Caption);
				AssertEquals("Column caption set", "Two", info2.Caption);

				AssertEquals("GetColumnCaption", "One", masterGrid.GetColumnCaption(DummyBizoSchema.Constants.Z0_Description));
				AssertEquals("GetColumnCaption", "Two", masterGrid.GetColumnCaption(DummyBizoSchema.Constants.Z0_Number));

				masterGrid.SetDataBinding(Dummy, "Collection", Dummy.Collection.GetType().Name);
				testForm.Show();

				masterGrid.SetColumnCaption(info1.ColumnName, "~One");
				masterGrid.SetColumnCaption(info2.ColumnName, "~Two");

				AssertEquals("Column caption set", "~One", masterGrid.Columns[info1.ColumnName].ColumnStyle.HeaderText);
				AssertEquals("Column caption set", "~Two", masterGrid.Columns[info2.ColumnName].ColumnStyle.HeaderText);

				AssertEquals("GetColumnCaption", "~One", masterGrid.GetColumnCaption(DummyBizoSchema.Constants.Z0_Description));
				AssertEquals("GetColumnCaption", "~Two", masterGrid.GetColumnCaption(DummyBizoSchema.Constants.Z0_Number));

				AssertEquals("GetColumnCaption", "", masterGrid.GetColumnCaption("IDon'tExist"));
			}
		}

		public void TestSetColumnModuleID()
		{
			using (var testForm = new ZForm(Dummy))
			{
				testForm.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 500);

				var masterGrid = new ZGrid { Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0), Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 200) };
				testForm.Controls.Add(masterGrid);
				var info = new ZCodeFindBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Code, ModuleID = DummyModuleIDs.Dummy };

				masterGrid.ColumnStyles.Add(info);

				masterGrid.SetColumnModuleID(info.ColumnName, DummyModuleIDs.Dummy2);
				AssertEquals("Column ModuleID set", DummyModuleIDs.Dummy2, info.ModuleID);

				masterGrid.SetColumnModuleID(info.ColumnName, DummyModuleIDs.Dummy);
				AssertEquals("Column ModuleID set", DummyModuleIDs.Dummy, info.ModuleID);
			}
		}

		public void TestSetColumnGroupName()
		{
			using (var testForm = new ZForm(Dummy))
			{
				testForm.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 500);

				var masterGrid = new ZGrid { Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0), Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 200) };
				testForm.Controls.Add(masterGrid);
				var info1 = new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Description };
				var info2 = new ZCalcEditColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Number };
				masterGrid.ColumnStyles.Add(info1);
				masterGrid.ColumnStyles.Add(info2);

				masterGrid.SetColumnGroupName(info1.ColumnName, new ResourceStringData("", "One"));
				masterGrid.SetColumnGroupName(info2.ColumnName, new ResourceStringData("", "Two"));

				AssertEquals("Column group name set", "One", info1.GroupName.Caption);
				AssertEquals("Column group name set", "Two", info2.GroupName.Caption);

				masterGrid.SetDataBinding(Dummy, "Collection", Dummy.Collection.GetType().Name);
				testForm.Show();

				masterGrid.SetColumnGroupName(info1.ColumnName, new ResourceStringData("", "~Two"));
				masterGrid.SetColumnGroupName(info2.ColumnName, new ResourceStringData("", "~Two"));

				AssertEquals("Column group name set", "~Two", info1.GroupName.Caption);
				AssertEquals("Column group name set", "~Two", info2.GroupName.Caption);
			}
		}

		/// <summary>
		/// Exceptions should be reported simply and cleanly
		/// If the error is transient the user may remove the red cross in the grid through the context menu
		/// </summary>
		public void TestGridWithExceptionThrownInInfoDuringBind()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			var mock = Factory.New<DummyChildBusinessObjectZ0_CodeInfoException>();

			dummy.Collection.Add(mock);
			dummy.RegisterEditableChildObject(dummy.Collection);

			using (var testForm = new ZChildForm(dummy))
			{
				var grid = new ZGrid { Dock = DockStyle.Fill };
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = AutoDummyBizo.Schema.Z0_Code });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = AutoDummyBizo.Schema.Z0_Description });
				testForm.Controls.Add(grid);

				grid.SetDataBinding(dummy, "Collection", dummy.Collection.GetType().Name);
				testForm.Show();
				//Force grid to redraw
				grid.Invalidate();
				grid.Update();

				AssertEquals(grid.ListManager.Count, 1);
				Assert(testForm.IsHandleCreated);
				Assert(grid.IsHandleCreated);

#if !WINZOR
				AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
#else
				// Winzor throws a second time originating CargoWiseTestWinzorDispatcherContext.OpenForm > RenderFormInTestContext > form.ReadyToRender
				AssertEquals(2, ExceptionReporterTestListener.Instance.Count);
#endif

				var ex = ExceptionReporterTestListener.Instance[0];
				Assert(ex.InnerException is InvalidOperationException);
				Assert(ex.Message == "TestGridWithExceptionThrownInInfoDuringBind");
				ErrorReporter.Clear();
			}
		}

		class DummyChildBusinessObjectZ0_CodeInfoException : DummyChildBusinessObject
		{
			public DummyChildBusinessObjectZ0_CodeInfoException(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override ZPropertyInfo Z0_CodeInfo
			{
				get
				{
					throw new InvalidOperationException("TestGridWithExceptionThrownInInfoDuringBind");
				}
			}
		}

		public void TestGridWithExceptionThrownInZDecimalDuringBind()
		{
			var mock = Factory.New<DummyChildBusinessObjectZ0_DecimalException>();

			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Collection.Add(mock);
			dummy.RegisterEditableChildObject(dummy.Collection);

			using (var testForm = new ZChildForm(dummy))
			{
				var grid = new ZGrid { Dock = DockStyle.Fill };
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = AutoDummyBizo.Schema.Z0_Code });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = AutoDummyBizo.Schema.Z0_Decimal });
				testForm.Controls.Add(grid);
				testForm.Show();
				Application.DoEvents(); //may not be necessary

				grid.SetDataBinding(dummy, "Collection");

				using (var bitmap = new Bitmap(grid.Width, grid.Height))
				{
					grid.Invalidate();
					Application.DoEvents(); //this call is necessary so grid.Invalidate does something
				}

				AssertEquals(1, ExceptionReporterTestListener.Instance.Count);

				var ex = ExceptionReporterTestListener.Instance[0];
				Assert(ex.InnerException is InvalidOperationException);
				Assert(ex.Message.StartsWith("TestGridWithExceptionThrownInZDecimalDuringBind"));
				ErrorReporter.Clear();
			}
		}

		class DummyChildBusinessObjectZ0_DecimalException : DummyChildBusinessObject
		{
			public DummyChildBusinessObjectZ0_DecimalException(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override ZDecimal Z0_Decimal
			{
				get
				{
					throw new InvalidOperationException("TestGridWithExceptionThrownInZDecimalDuringBind");
				}

				set
				{
					base.Z0_Decimal = value;
				}
			}
		}

		public void TestSetColumnVisible()
		{
			using (var testForm = new ZForm(Dummy))
			{
				testForm.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 500);

				var masterGrid = new ZGrid { Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0), Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 200) };
				testForm.Controls.Add(masterGrid);
				var info1 = new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Description };
				var info2 = new ZCalcEditColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Number };
				var info3 = new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_VarCharMax };
				masterGrid.ColumnStyles.Add(info1);
				masterGrid.ColumnStyles.Add(info2);
				masterGrid.ColumnStyles.Add(info3);

				masterGrid.SetColumnVisible(false, info1.ColumnName);
				masterGrid.SetColumnVisible(true, info2.ColumnName);
				masterGrid.SetColumnVisible(false, info3.ColumnName);

				AssertEquals("Column hidden", false, info1.IsVisible);
				AssertEquals("Column visible", true, info2.IsVisible);
				AssertEquals("Column visible", false, info3.IsVisible);

				masterGrid.SetDataBinding(Dummy, "Collection", Dummy.Collection.GetType().Name);
				testForm.Show();

				masterGrid.SetColumnVisible(true, info1.ColumnName);
				masterGrid.SetColumnVisible(false, info2.ColumnName);

				AssertEquals("Column visible", true, masterGrid.Columns[info1.ColumnName].IsVisible);
				AssertEquals("Column hidden", false, masterGrid.Columns[info2.ColumnName].IsVisible);
				AssertEquals("Column hidden", false, masterGrid.Columns[info3.ColumnName].IsVisible);

				masterGrid.SetColumnVisible(true, [info2.ColumnName, info3.ColumnName]);
				AssertEquals("Column visible", true, info1.IsVisible);
				AssertEquals("Column visible", true, info2.IsVisible);
				AssertEquals("Column visible", true, info3.IsVisible);

				masterGrid.SetAllColumnsVisible(false);
				AssertEquals("Column visible", false, info1.IsVisible);
				AssertEquals("Column visible", false, info2.IsVisible);
				AssertEquals("Column visible", false, info3.IsVisible);
			}
		}

		public void TestSetColumnMandatory()
		{
			using (var testForm = new ZForm(Dummy))
			{
				testForm.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 500);

				var masterGrid = new ZGrid { Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0), Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 200) };
				testForm.Controls.Add(masterGrid);
				var info1 = new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Description };
				var info2 = new ZCalcEditColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Number };
				var info3 = new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_VarCharMax };
				masterGrid.ColumnStyles.Add(info1);
				masterGrid.ColumnStyles.Add(info2);
				masterGrid.ColumnStyles.Add(info3);

				masterGrid.SetColumnMandatory(info1.ColumnName, false);
				masterGrid.SetColumnMandatory(info2.ColumnName, true);
				masterGrid.SetColumnMandatory(info3.ColumnName, false);

				info3.IsVisible = true;
				var saveInfo1Visibility = info1.IsVisible;
				var saveInfo3Visibility = info3.IsVisible;
				AssertEquals("Column Optional", false, info1.IsMandatory);
				AssertEquals("Optional Column visibility should not be changed", saveInfo1Visibility, info1.IsVisible);
				AssertEquals("Column Mandatory", true, info2.IsMandatory);
				AssertEquals("Column visibility should be set when a column has been set to mandatory", true, info2.IsVisible);
				AssertEquals("Column Mandatory", false, info3.IsMandatory);
				AssertEquals("Column visibility should not be changed", saveInfo3Visibility, info3.IsVisible);

				masterGrid.SetDataBinding(Dummy, "Collection", Dummy.Collection.GetType().Name);
				testForm.Show();

				masterGrid.SetColumnMandatory(info1.ColumnName, true);
				masterGrid.SetColumnMandatory(info2.ColumnName, false);

				AssertEquals("Column Mandatory", true, masterGrid.Columns[info1.ColumnName].IsMandatory);
				AssertEquals("Column visibility should be set when a column has been set to mandatory", true, info1.IsVisible);
				AssertEquals("Column Optional", false, masterGrid.Columns[info2.ColumnName].IsMandatory);
				AssertEquals("Column Optional", false, masterGrid.Columns[info3.ColumnName].IsMandatory);
			}
		}

		public void TestSettingMandatoryColumnSetsVisibilty()
		{
			using (var testForm = new ZForm(Dummy))
			{
				testForm.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 500);

				var masterGrid = new ZGrid { Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0), Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 200) };
				testForm.Controls.Add(masterGrid);
				var info0 = new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Code };
				var info1 = new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Description };
				var info2 = new ZCalcEditColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Number };
				var info3 = new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_VarCharMax };
				masterGrid.ColumnStyles.Add(info0);
				info0.IsMandatory = true;
				masterGrid.ColumnStyles.Add(info1);
				info1.IsVisible = false;
				masterGrid.ColumnStyles.Add(info2);
				info2.IsVisible = false;
				masterGrid.ColumnStyles.Add(info3);
				info3.IsVisible = true;

				masterGrid.SetDataBinding(Dummy, "Collection", Dummy.Collection.GetType().Name);
				testForm.Show();
				AssertEquals("Pre-condition", true, info0.IsVisible);
				AssertEquals("Pre-condition", false, info1.IsVisible);
				AssertEquals("Pre-condition", false, info2.IsVisible);
				AssertEquals("Pre-condition", true, info3.IsVisible);
				masterGrid.ResetColumns();
				AssertEquals("Reset Column visibility", true, info0.IsVisible);
				AssertEquals("Reset Column visibility", false, info1.IsVisible);
				AssertEquals("Reset Column visibility", false, info2.IsVisible);
				AssertEquals("Reset Column visibility", true, info3.IsVisible);

				masterGrid.SetColumnMandatory(info1.ColumnName, true);
				masterGrid.SetColumnMandatory(info2.ColumnName, true);
				masterGrid.SetColumnMandatory(info3.ColumnName, false);

				AssertEquals("Column Mandatory", true, info1.IsMandatory);
				AssertEquals("Column visibility should also be changed", true, info1.IsVisible);
				AssertEquals("Column Mandatory", true, info2.IsMandatory);
				AssertEquals("Column visibility should be set when a column has been set to mandatory", true, info2.IsVisible);
				AssertEquals("Column Mandatory", false, info3.IsMandatory);
				AssertEquals("Column visibility should not be changed", true, info3.IsVisible);

				masterGrid.SetDataBinding(Dummy, "Collection", Dummy.Collection.GetType().Name);
				testForm.Show();
				masterGrid.ResetColumns();
				AssertEquals("Reset should not affect this Column visibility", true, info1.IsVisible);
				AssertEquals("Reset should not affect this Column visibility as column was set to mandatory", true, info2.IsVisible);
			}
		}

		public void TestValidationOnCurrentCellChange()
		{
			Dummy.Collection.RemoveAll();
			Dummy.Collection.Add(Factory.New(typeof(ValidationDummy)));
			Dummy.RegisterEditableChildObject(Dummy.Collection);

			using (var testForm = new ZChildForm(Dummy))
			{
				var grid = new ZTestGrid { Dock = DockStyle.Fill };
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = AutoDummyBizo.Schema.Z0_Code });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = AutoDummyBizo.Schema.Z0_Description });
				testForm.Controls.Add(grid);

				grid.SetDataBinding(Dummy, "Collection", Dummy.Collection.GetType().Name);
				testForm.Show();

				AssertEquals("Initial HasNotifications", false, Dummy.HasNotifications());

				grid.CurrentCell = new DataGridCell(0, 0);
				grid.CurrentCell = new DataGridCell(0, 1);

				AssertEquals("Grid Current Type", typeof(ValidationDummy), grid.ListManager.GetCurrent().GetType());
				AssertEquals("HasNotifications after changing cell", true, Dummy.HasNotifications());
			}
		}

		[ExpectNoExceptions]
		public void TestRemoveAction_DeleteChecker()
		{
			Dummy.Collection.RemoveAll();
			Dummy.Collection.Add(Factory.New(typeof(DummyChildEnterpriseBusinessObject)));

			var mock = new Mock<DeleteChecker>(MockBehavior.Strict);
			mock.Setup(o => o.DeleteDetails(It.IsAny<BusinessObject>())).Returns(new DeleteDetails.Disallow(testMessage));

			var hash = new Hashtable();
			hash.Add("DummyBizo", new TestObjectHandle(new ArrayList() { mock.Object }));
			using (ObjectFactory.Substitute("BusinessObjectStrategies", hash))
			using (var testForm = new ZChildForm(Dummy))
			{
				var grid = new ZTestGrid { Dock = DockStyle.Fill };
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = AutoDummyBizo.Schema.Z0_Code });
				testForm.Controls.Add(grid);

				grid.SetDataBinding(Dummy, "Collection", Dummy.Collection.GetType().Name);
				testForm.Show();
				grid.Select(0);
				KeySender.PostKeyDown(grid, Keys.Delete);
				Application.DoEvents();

				AssertEquals("LastMessage WasError", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("LastMessage Text", testMessage + System.Environment.NewLine, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}
		const string testMessage = "Test Message";

		public void TestRemoveFromListWhenOnRemovingFromListIsHooked()
		{
			using (var testForm = new ZChildForm(Dummy))
			{
				var grid = new ZTestGrid { Dock = DockStyle.Fill };
				grid.HookToOnRemovingBizOFromList();
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = AutoDummyBizo.Schema.Z0_Code });
				testForm.Controls.Add(grid);

				grid.SetDataBinding(Dummy, "Collection", Dummy.Collection.GetType().Name);
				testForm.Show();

				DummyChildBusinessObject bizOToDelete = Dummy.Collection[0];
				bizOToDelete.Z0_Bool = false;//do not remove
				grid.Select(0);
				KeySender.PostKeyDown(grid, Keys.Delete);
				Application.DoEvents();
				AssertEquals(true, Dummy.Collection.Contains(bizOToDelete));

				bizOToDelete.Z0_Bool = true;//do remove
				grid.Select(0);
				KeySender.PostKeyDown(grid, Keys.Delete);
				Application.DoEvents();
				AssertEquals(false, Dummy.Collection.Contains(bizOToDelete));
			}
		}

		public void TestRightClickContextMenu_MovingCurrencyManagerPositionDoesNotCauseException()
		{
			using (var testForm = new ZChildForm())
			{
				var dummies = new DummyBusinessObjectCollection(Factory);

				var testGrid = new ZTestGrid { Dock = DockStyle.Fill };
				testGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = AutoDummyBizo.Schema.Z0_Code });
				testGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = AutoDummyBizo.Schema.Z0_Description });
				testForm.Controls.Add(testGrid);

				testForm.Show();
				testGrid.SetDataBinding(dummies, "");

				dummies.AddNew();
				dummies.AddNew();
				dummies.AddNew();
				dummies.AddNew();

				testGrid.CurrentCell = new DataGridCell(4, 2);
				AssertEquals("ListManager Position before MouseDown", 4, testGrid.ListManager.Position);

				var mouseArgs = new MouseEventArgs(MouseButtons.Right, 1, ControlDpiScalingHelper.ScaleToCurrentDpiX(10), ControlDpiScalingHelper.ScaleToCurrentDpiY(30), 0);
				AssertEquals("Hit on Row 0", 0, testGrid.HitTest(mouseArgs.X, mouseArgs.Y).Row);

				testGrid.OnMouseDown(mouseArgs);
				AssertEquals("ListManager Position before MouseDown", 0, testGrid.ListManager.Position);

				Application.DoEvents();
				// wait for exception in Paint: IndexOutOfRange Exception, fixed by EndCurrentEdit in OnMouseDown handling of Right Click.
				// PaintWithErrorHandling catches and eats exception, so we report it in ZTestGrid.OnPaint().
			}
		}

		public void TestClickFormOpensOnTop()
		{
			using (var testForm = new ZForm(Dummy))
			{
				Dummy.Collection.SetReadOnlyIncludingChildren(true);
				Dummy.Collection.AddNew();

				testForm.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 500);
				var masterGrid = new ZTestGrid();
				masterGrid.MouseDown += MasterGrid_MouseDown;
				testForm.Activated += TestForm_Activated;
				testForm.Controls.Add(masterGrid);
				masterGrid.Dock = DockStyle.Fill;

				var info1 = new ZTextBoxColumnStyleInfo();
				var info2 = new ZCalcEditColumnStyleInfo();
				info1.ColumnName = "Z0_Description";
				info1.IsReadOnly = true;
				info2.ColumnName = "Z0_Number";
				info2.IsReadOnly = true;
				masterGrid.ColumnStyles.Add(info1);
				masterGrid.ColumnStyles.Add(info2);

				masterGrid.BindTo = "Collection";
				testForm.Show();
				Application.DoEvents();

				AssertEquals("Grid's form activated after show", true, IsActivated);
				IsActivated = false;

				masterGrid.CurrentCell = new DataGridCell(0, 1);

				try
				{
					masterGrid.FireMouseDown(ControlDpiScalingHelper.ScaleToCurrentDpiX(16), ControlDpiScalingHelper.ScaleToCurrentDpiY(25), 2);
					Application.DoEvents();

					AssertNotNull("MouseDown should have opened the child form.", OpenedForm);
					AssertEquals("Child form visiblility", true, OpenedForm.Visible);
					AssertEquals("Grid's form should not be reactivated after the double click. NOTE: This test result is sensitive to UI interference.", false, IsActivated);
				}
				finally
				{
					OpenedForm.Close();
				}
			}
		}

		#region TestClickFormOpensOnTop Support Fields & Methods

		Form OpenedForm;
		bool IsActivated;

		void MasterGrid_MouseDown(object sender, MouseEventArgs e)
		{
			var grid = sender as DataGrid;
			if (grid.HitTest(e.X, e.Y).Type == DataGrid.HitTestType.RowHeader)
			{
				OpenedForm = new Form();
				OpenedForm.Show();
			}
		}

		void TestForm_Activated(object sender, EventArgs e)
		{
			IsActivated = true;
		}

		#endregion

		public void TestEditControlVisibility()
		{
			var superDummy = Factory.New<SuperDummyBusinessObject>();
			superDummy.Collection.AddNew();
			superDummy.Collection[0].Collection.AddNew();

			using (var testForm = new ZForm(superDummy))
			{
				testForm.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 500);

				var masterGrid = new ZGrid { Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0), Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 200) };

				var info1 = new ZTextBoxColumnStyleInfo { ColumnName = "Z0_Description" };
				var info2 = new ZCalcEditColumnStyleInfo { ColumnName = "Z0_Number" };
				masterGrid.ColumnStyles.Add(info1);
				masterGrid.ColumnStyles.Add(info2);

				var childGrid = new ZGrid();
				childGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 200);
				childGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 200);
				childGrid.ColumnStyles.Add(info1);
				childGrid.ColumnStyles.Add(info2);

				testForm.Controls.Add(masterGrid);
				testForm.Controls.Add(childGrid);

				masterGrid.BindTo = "Collection";
				childGrid.BindTo = "Collection.Collection";

				testForm.Show();
				Application.DoEvents();

				AssertEquals("MasterGrid initally contains focus", true, masterGrid.ContainsFocus);
				AssertEquals("MasterGrid.LastFocusedColumn EditControl visible", true, masterGrid.LastFocusedColumn.EditControl.Visible);
				Assert("MasterGrid.LastFocusedColumn EditControl Bounds.Width > 0", masterGrid.LastFocusedColumn.EditControl.Bounds.Width > 0);

				childGrid.Focus();
				Application.DoEvents();

				AssertEquals("ChildGrid contains focus", true, childGrid.ContainsFocus);
				AssertEquals("ChildGrid.LastFocusedColumn EditControl visible", true, childGrid.LastFocusedColumn.EditControl.Visible);
				Assert("ChildGrid.LastFocusedColumn EditControl Bounds.Width > 0", childGrid.LastFocusedColumn.EditControl.Bounds.Width > 0);
				AssertEquals("MasterGrid.LastFocusedColumn EditControl Bounds.Width", 0, masterGrid.LastFocusedColumn.EditControl.Bounds.Width);

				masterGrid.Focus();
				Application.DoEvents();

				AssertEquals("MasterGrid initally contains focus", true, masterGrid.ContainsFocus);
				AssertEquals("MasterGrid.LastFocusedColumn EditControl visible", true, masterGrid.LastFocusedColumn.EditControl.Visible);
				Assert("MasterGrid.LastFocusedColumn EditControl Bounds.Width > 0", masterGrid.LastFocusedColumn.EditControl.Bounds.Width > 0);
				AssertEquals("ChildGrid.LastFocusedColumn EditControl Bounds.Width", 0, childGrid.LastFocusedColumn.EditControl.Bounds.Width);
			}
		}

		public void TestImplementsIListEditableControl()
		{
			AssertEquals(
				"ZGrid must implement IListEditableControl to communicate to UserIdleWorker when it is being 'browsed'",
				true, typeof(IListEditableControl).IsAssignableFrom(typeof(ZGrid)));
		}

		public void TestWholeRowClickWithRowError()
		{
			using (var testForm = new ZChildForm(Dummy))
			{
				var masterGrid = new ZTestGrid { Dock = DockStyle.Fill, IsWholeRowSelectedOnClick = true };
				masterGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "Z0_Description" });
				masterGrid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo { ColumnName = "Z0_Number" });

				Dummy.Collection.SetReadOnlyIncludingChildren(true);
				testForm.Controls.Add(masterGrid);
				masterGrid.BindTo = "Collection";
				testForm.Show();

				DummyChildBusinessObject dummyChild = Dummy.Collection.AddNew();
				dummyChild.AddRowError("Test Error");

				masterGrid.FireMouseDown(ControlDpiScalingHelper.ScaleToCurrentDpiX(100), ControlDpiScalingHelper.ScaleToCurrentDpiY(29));
				AssertEquals("Row 0 selected", true, masterGrid.IsSelected(0));
			}
		}

		public void TestChildIsReadOnlyWhenNoParent()
		{
			var superDummy = Factory.New<SuperDummyBusinessObject>();

			using (var testForm = new ZForm(superDummy))
			{
				var masterGrid = new ZGrid();
				var childGrid = new ZGrid();

				SetupGridForMasterDetailTesting(testForm, superDummy, masterGrid, childGrid);

				FocusAndEditGrid(masterGrid);

				AssertEquals("Initial Master Count when Master Focused", 1, masterGrid.ListManager.Count);
				AssertEquals("Initial Child Count when Master Focused", 0, childGrid.ListManager.Count);

				AssertEquals("Initial Master ReadOnly when Master Focused", false, masterGrid.ReadOnly);
				AssertEquals("Initial Child ReadOnly when Master Focused", false, childGrid.ReadOnly);

				FocusAndEditGrid(childGrid);

				AssertEquals("Initial Master Count when Child Focused", 0, masterGrid.ListManager.Count);
				AssertEquals("Initial Child Count when Child Focused", 0, childGrid.ListManager.Count);

				AssertEquals("Initial Master ReadOnly when Child Focused", false, masterGrid.ReadOnly);
				AssertEquals("Initial Child ReadOnly when Child Focused", true, childGrid.ReadOnly);

				FocusAndEditGrid(masterGrid);
				PostKeysToGridAndSetCurrentCellToColumnIndex1(masterGrid, Keys.T);

				FocusAndEditGrid(childGrid);

				AssertEquals("Master Count after adding to MasterGrid.List", 1, masterGrid.ListManager.Count);
				AssertEquals("Child Count after adding to MasterGrid.List", 1, childGrid.ListManager.Count);

				AssertEquals("Master ReadOnly after adding to MasterGrid.List", false, masterGrid.ReadOnly);
				AssertEquals("Child ReadOnly after adding to MasterGrid.List", false, childGrid.ReadOnly);

				masterGrid.ListManager.List.Clear();

				AssertEquals("Master Count after MasterGrid.List Cleared", 0, masterGrid.ListManager.Count);
				AssertEquals("Child Count after MasterGrid.List Cleared", 0, childGrid.ListManager.Count);

				AssertEquals("Master ReadOnly after MasterGrid.List Cleared", false, masterGrid.ReadOnly);
				AssertEquals("Child ReadOnly after MasterGrid.List Cleared", true, childGrid.ReadOnly);
			}
		}

		[UseSnapshotProtection]
		public void TestChildCancelsAdditionalRowWhenFocusingMaster()
		{
			var superDummy = Factory.New<SuperDummyBusinessObject>();

			using (var testForm = new ZForm(superDummy))
			{
				var masterGrid = new ZGrid();
				var childGrid = new ZGrid();

				SetupGridForMasterDetailTesting(testForm, superDummy, masterGrid, childGrid);

				FocusAndEditGrid(masterGrid);
				PostKeysToGridAndSetCurrentCellToColumnIndex1(masterGrid, Keys.T);

				FocusAndEditGrid(childGrid);
				PostKeysToGridAndSetCurrentCellToColumnIndex1(childGrid, Keys.X);
				childGrid.CurrentCell = new DataGridCell(1, 0);
				Application.DoEvents();

				FocusAndEditGrid(masterGrid);

				AssertEquals("Committed Row Count", 1, childGrid.ListManager.Count);
			}
		}

		public void TestIsEditing()
		{
			using (var form = new ZChildForm(Dummy))
			{
				var grid = new ZTestGrid { Dock = DockStyle.Fill };
				IEditableControl editableGrid = grid;
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(AutoDummyBizo.Schema.Z0_Code, 50));

				form.Controls.Add(grid);
				form.Show();
				grid.SetDataBinding(Dummy, "Collection");
				Application.DoEvents();

				form.ActiveControl = null;
				AssertEquals("Grid not editing initially", false, editableGrid.IsEditing);
				((IBindingList)Dummy.Collection).AddNew();
				grid.Focus();
				AssertEquals("Grid is editing when there is a new uncommitted row", true, editableGrid.IsEditing);
				((ICancelAddNew)Dummy.Collection).EndNew(Dummy.Collection.Count - 1);
				AssertEquals("Grid not editing when the uncommitted row is committed", false, editableGrid.IsEditing);
				grid.BeginEdit(grid.TableStyles[0].GridColumnStyles[0], 0);
				AssertEquals("Grid editing cell", true, editableGrid.IsEditing);
			}
		}

		static void PostKeysToGridAndSetCurrentCellToColumnIndex1(ZGrid grid, params Keys[] keys)
		{
			foreach (var key in keys)
			{
				KeySender.PostKeyDown(grid.LastFocusedColumn.EditControl, key);
				Application.DoEvents();
			}

			grid.CurrentCell = new DataGridCell(grid.CurrentCell.RowNumber, 1);
			Application.DoEvents();
		}

		static void SetupGridForMasterDetailTesting(ZForm testForm, BusinessObject dataSouce, ZGrid masterGrid, ZGrid childGrid)
		{
			testForm.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 500);

			masterGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
			masterGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 200);

			var info1 = new ZTextBoxColumnStyleInfo();
			var info2 = new ZCalcEditColumnStyleInfo();
			info1.ColumnName = "Z0_Description";
			info2.ColumnName = "Z0_Number";
			masterGrid.ColumnStyles.Add(info1);
			masterGrid.ColumnStyles.Add(info2);

			childGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 200);
			childGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 200);
			childGrid.ColumnStyles.Add(info1);
			childGrid.ColumnStyles.Add(info2);

			testForm.Controls.Add(masterGrid);
			testForm.Controls.Add(childGrid);

			masterGrid.BindTo = "Collection";
			childGrid.BindTo = "Collection.Collection";

			testForm.Show();
		}

		public void TestRemoveAction()
		{
			using (var testForm = new ZTestForm(Dummy))
			{
				testForm.Grid.RemoveAction = RemoveAction.NoRemovePossible;

				Dummy.Collection.RemoveAll();
				DummyBaseBusinessObject dummyBase1 = Dummy.Collection.AddNew();
				DummyBaseBusinessObject dummyBase2 = Dummy.Collection.AddNew();
				DummyBaseBusinessObject dummyBase3 = Dummy.Collection.AddNew();
				dummyBase1.Z0_Description = "1";
				dummyBase2.Z0_Description = "2";
				dummyBase3.Z0_Description = "3";

				var row1 = ((INeedRow)dummyBase1).Row;
				var row2 = ((INeedRow)dummyBase2).Row;
				var row3 = ((INeedRow)dummyBase3).Row;

				row1.AcceptChanges();
				row2.AcceptChanges();
				row3.AcceptChanges();

				testForm.Show();

				AssertEquals("Dummy Collection Populated", 3, Dummy.Collection.Count);
				AssertEquals("Grid Item Count", 3, testForm.Grid.ListManager.Count);

				testForm.Grid.Select(0);
				KeySender.PostKeyDown(testForm.Grid, Keys.Delete);
				Application.DoEvents();

				AssertEquals("Dummy Collection With Grid.NoRemovePossible", 3, Dummy.Collection.Count);
				AssertEquals("Grid Item Count", 3, testForm.Grid.ListManager.Count);

				testForm.Grid.RemoveAction = RemoveAction.Remove;

				testForm.Grid.Select(0);
				KeySender.PostKeyDown(testForm.Grid, Keys.Delete);
				Application.DoEvents();

				AssertEquals("Dummy Collection With Grid.Remove", 2, Dummy.Collection.Count);
				AssertEquals("Grid Item Count", 2, testForm.Grid.ListManager.Count);
				AssertEquals("Row 1 RowState", DataRowState.Unchanged, row1.RowState);
				AssertEquals("Row 2 RowState", DataRowState.Unchanged, row2.RowState);
				AssertEquals("Row 3 RowState", DataRowState.Unchanged, row3.RowState);

				testForm.Grid.RemoveAction = RemoveAction.RemoveAndDelete;

				testForm.Grid.Select(1);
				KeySender.PostKeyDown(testForm.Grid, Keys.Delete);
				Application.DoEvents();

				AssertEquals("Dummy Collection With Grid.Remove", 1, Dummy.Collection.Count);
				AssertEquals("Grid Item Count", 1, testForm.Grid.ListManager.Count);
				AssertEquals("Row 1 RowState", DataRowState.Unchanged, row1.RowState);
				AssertEquals("Row 2 RowState", DataRowState.Unchanged, row2.RowState);
				AssertEquals("Row 3 RowState", DataRowState.Deleted, row3.RowState);
			}
		}

		public void TestRemoveAction_NotWhenAllowRemoveFalse()
		{
			using (var testForm = new ZTestForm(Dummy))
			{
				testForm.Grid.RemoveAction = RemoveAction.Remove;
				Dummy.Collection.RemoveAll();
				Dummy.Collection.AddNew();
				Dummy.Collection.AddNew();
				Dummy.Collection.AddNew();
				testForm.Show();

				AssertEquals("Dummy Collection Populated", 3, Dummy.Collection.Count);
				AssertEquals("Grid Item Count", 3, testForm.Grid.ListManager.Count);

				Dummy.Collection.SetAllowRemove(false);
				testForm.Grid.Select(0);
				KeySender.PostKeyDown(testForm.Grid, Keys.Delete);
				Application.DoEvents();
				AssertEquals("Not removed when AllowRemove = false", 3, Dummy.Collection.Count);

				Dummy.Collection.SetAllowRemove(true);
				testForm.Grid.Select(0);
				KeySender.PostKeyDown(testForm.Grid, Keys.Delete);
				Application.DoEvents();
				AssertEquals("Removed when AllowRemove = true", 2, Dummy.Collection.Count);
			}
		}

		public void TestRemoveAction_NotWhenAllowReadOnlyRowsToBeDeletedIsFalse()
		{
			using (var testForm = new ZTestForm(Dummy))
			{
				testForm.Grid.RemoveAction = RemoveAction.Remove;

				Dummy.Collection.RemoveAll();
				Dummy.Collection.AddNew();
				Dummy.Collection.AddNew();
				Dummy.Collection.AddNew();
				testForm.Show();

				AssertEquals("Dummy Collection Populated", 3, Dummy.Collection.Count);
				AssertEquals("Grid Item Count", 3, testForm.Grid.ListManager.Count);

				Dummy.Collection.SetAllowRemove(true);

				((ZGridColumnInfo)testForm.Grid.ColumnStyles[0]).IsReadOnly = true;
				((ZGridColumnInfo)testForm.Grid.ColumnStyles[1]).IsReadOnly = true;

				testForm.Grid.AllowReadOnlyRowsToBeDeleted = false;
				testForm.Grid.Select(0);
				KeySender.PostKeyDown(testForm.Grid, Keys.Delete);
				Application.DoEvents();
				AssertEquals("Not removed when AllowRemove = false", 3, Dummy.Collection.Count);

				testForm.Grid.AllowReadOnlyRowsToBeDeleted = true;
				testForm.Grid.Select(0);
				KeySender.PostKeyDown(testForm.Grid, Keys.Delete);
				Application.DoEvents();
				AssertEquals("Removed when AllowRemove = true", 2, Dummy.Collection.Count);
			}
		}

		public void TestGridRestoreColumnSettings()
		{
			using (var testForm = new ZTestGridForm(Dummy))
			{
				testForm.Show();

				//We need to use a grid on a tab, as those grids are late bound.
				testForm.TabGrid.Columns[AutoDummyBizo.Schema.Z0_Description].ColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(312);
			}

			using (var testForm = new ZTestGridForm(Dummy))
			{
				testForm.Show();
				AssertEquals("Z0_Description column width", ControlDpiScalingHelper.ScaleToCurrentDpiX(312), testForm.TabGrid.Columns[AutoDummyBizo.Schema.Z0_Description].ColumnStyle.Width);
			}
		}

		public void TestFlattenedPropertiesWork()
		{
			const string FlatPropertyName = "Self+Z0_Description";

			using (var testForm = new ZChildForm(Dummy))
			{
				var masterGrid = new ZTestGrid { Dock = DockStyle.Fill, IsWholeRowSelectedOnClick = true };
				masterGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = FlatPropertyName });

				testForm.Controls.Add(masterGrid);
				masterGrid.BindTo = "Collection";

				testForm.Show();
				Application.DoEvents();

				AssertEquals("Should find the flattened property", FlatPropertyName, masterGrid.TableStyles[0].GridColumnStyles[0].PropertyDescriptor.Name);
			}
		}

		public void TestMouseUpOnDisposedGrid()
		{
			AssertNoExceptionThrown(delegate
			{
				using (var grid = new ZTestGrid())
				{
					grid.Dispose();
					grid.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, ControlDpiScalingHelper.ScaleToCurrentDpiX(0), ControlDpiScalingHelper.ScaleToCurrentDpiY(0), 0));
				}
			});
		}

		public void TestSelectAllElements_SelectSingleElement_UnSelectAll()
		{
			var dummy = DummyBusinessObject.New(Factory);
			var child1 = dummy.Collection.AddNew();
			var child2 = dummy.Collection.AddNew();
			var child3 = dummy.Collection.AddNew();

			using (var form = new ZForm(dummy))
			{
				var grid = new ZGrid();
				grid.BindTo = "Collection";
				form.Controls.Add(grid);

				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Description });
				form.Show();
				AssertEquals("Precondition: No rows should be selected.", 0, grid.SelectedRowCount);

				grid.SelectAllElements();
				AssertEquals("IsSelected(0)", true, grid.IsSelected(0));
				AssertEquals("IsSelected(1)", true, grid.IsSelected(1));
				AssertEquals("IsSelected(2)", true, grid.IsSelected(2));

				grid.SelectSingleElement(child3);
				AssertEquals("IsSelected(0)", false, grid.IsSelected(0));
				AssertEquals("IsSelected(1)", false, grid.IsSelected(1));
				AssertEquals("IsSelected(2)", true, grid.IsSelected(2));

				grid.SelectSingleElement(child2);
				AssertEquals("IsSelected(0)", false, grid.IsSelected(0));
				AssertEquals("IsSelected(1)", true, grid.IsSelected(1));
				AssertEquals("IsSelected(2)", false, grid.IsSelected(2));

				grid.SelectAllElements(x => x == child1 || x == child3);
				AssertEquals("IsSelected(0)", true, grid.IsSelected(0));
				AssertEquals("IsSelected(1)", false, grid.IsSelected(1));
				AssertEquals("IsSelected(2)", true, grid.IsSelected(2));

				grid.SelectAllElements();
				grid.UnSelectAll();
				AssertEquals("IsSelected(0)", false, grid.IsSelected(0));
				AssertEquals("IsSelected(1)", false, grid.IsSelected(1));
				AssertEquals("IsSelected(2)", false, grid.IsSelected(2));
			}
		}

		public void TestBindingContext()
		{
			using (var form = new ZForm(Dummy))
			{
#pragma warning disable CW1105 // Do not use System.Windows.Forms.UserControl or System.Windows.Forms.KUserControl Class
				var userControl = new UserControl();
#pragma warning restore CW1105 // Do not use System.Windows.Forms.UserControl or System.Windows.Forms.KUserControl Class
				userControl.Font = OFont.GetFontBold(); // a font on the parent control causes DataGrid.OnFontChanged() to be raised which accesses BindingContext at the wrong time
				var grid = new ZGrid();
				userControl.Controls.Add(grid);
				form.Controls.Add(userControl);

				form.Show();
				AssertEquals("BindingContext taken from parent form, otherwise the form responds very slowly", typeof(ZBindingContext), grid.BindingContext.GetType());
			}
		}

		public void TestDeleteKeyWithAllModifiers()
		{
			CombineAssertions(() =>
			{
				AssertRowDeletedWhenPressingKey(Keys.Delete);
				AssertRowDeletedWhenPressingKey(Keys.Shift | Keys.Delete);
				AssertRowDeletedWhenPressingKey(Keys.Control | Keys.Delete);
				AssertRowDeletedWhenPressingKey(Keys.Alt | Keys.Delete);
				AssertRowDeletedWhenPressingKey(Keys.Shift | Keys.Control | Keys.Delete);
				AssertRowDeletedWhenPressingKey(Keys.Control | Keys.Alt | Keys.Delete);
				AssertRowDeletedWhenPressingKey(Keys.Shift | Keys.Alt | Keys.Delete);
				AssertRowDeletedWhenPressingKey(Keys.Shift | Keys.Control | Keys.Alt | Keys.Delete);
			});
		}

		void AssertRowDeletedWhenPressingKey(Keys key)
		{
			using (var form = new ZForm(Dummy))
			{
				var grid = new ZGridNotificationsTestCase.DummyZGrid();
				grid.BindTo = "Collection";
				form.Controls.Add(grid);

				var columnInfo = new ZTextBoxColumnStyleInfo();
				columnInfo.ColumnName = DummyBizoSchema.Constants.Z0_Description;
				grid.ColumnStyles.Add(columnInfo);

				form.Show();

				Dummy.Collection.AddNew();
				var msg = new Message();

				Dummy.Collection.SetReadOnlyIncludingChildren(true);
				grid.Select(0);
				grid.ProcessCmdKey(ref msg, key);
				AssertEquals("Item shouldn't be deleted", 1, Dummy.Collection.Count);

				Dummy.Collection.SetReadOnlyIncludingChildren(false);
				grid.Select(0);
				grid.ProcessCmdKey(ref msg, key);
				AssertEquals("Item should be deleted", 0, Dummy.Collection.Count);
			}
		}

		public void TestHasColumnLabelCaptionRenderer()
		{
			var grid = new ZGrid();
			Assert(grid.GetExtension<IAutomaticLabelExtension>() != null);
			Assert(grid.GetExtension<IAutomaticLabelExtension>() is ZGridColumnLabelRenderer);
			grid.Dispose();
		}

		public void TestBindGridToTopLevelBO()
		{
			var dummy2 = Factory.New<DummyBusinessObject>();

			using (var form = new ZTestForm(Dummy))
			{
				form.Show();
				form.Grid.SetDataBinding(null, "");
				AssertNull("Grid list should be null", form.Grid.List);

				form.Grid.BindTo = "Collection";
				form.SetDataBinding(dummy2, "");

				dummy2.Collection.AddNew();
				dummy2.Collection.AddNew();
				dummy2.Collection.AddNew();
				AssertEquals("Grid is bound and populated", 3, form.Grid.List.Count);
			}
		}

		public void TestTabOutParsesDollarSymbolCorrectly()
		{
			var obj1 = Factory.New<DummyBusinessObject>();
			obj1.Z0_Code = "z1";
			Factory.Save();

			DummyChildBusinessObject dummy1 = Dummy.Collection.AddNew();
			dummy1.Z0_Money = 0;
			dummy1.Z0_Guid = obj1.PK;

			using (var form = new ZForm(Dummy))
			{
				form.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 200);

				var grid = new ZTestGrid();
				grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
				grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 200);
				grid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Money });
				form.Controls.Add(grid);

				form.Show();
				grid.SetDataBinding(Dummy, "Collection");
				grid.Focus();

				var zCalcEditColumnStyle = grid.TableStyles[0].GridColumnStyles[0] as ZCalcEditColumnStyle;

				using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(new CultureInfo("en-AU")))
				{
					grid.BeginEdit(zCalcEditColumnStyle, 0);
					zCalcEditColumnStyle.TextBox.Text = "$1,123.321";
					zCalcEditColumnStyle.EditControl.Visible = true;
					var textBox = (DataGridTextBox)zCalcEditColumnStyle.TextBox;
					textBox.IsInEditOrNavigateMode = false;
					KeySender.SendKeyDownToProcessCmdKey(grid, Keys.Tab);

					AssertEquals("1123.321", grid[0, 0].ToString());
					grid.CurrentCell = new DataGridCell(0, 0);
					var value = 1123.321m;
					AssertEquals(value.ToString(zCalcEditColumnStyle.Format), zCalcEditColumnStyle.TextBox.Text);

					using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(new CultureInfo("it-IT")))
					{
						grid.BeginEdit(zCalcEditColumnStyle, 0);
						zCalcEditColumnStyle.TextBox.Text = "€1.123,321";
						zCalcEditColumnStyle.EditControl.Visible = true;
						textBox = (DataGridTextBox)zCalcEditColumnStyle.TextBox;
						textBox.IsInEditOrNavigateMode = false;
						KeySender.SendKeyDownToProcessCmdKey(grid, Keys.Tab);

						AssertEquals("1123.321", grid[0, 0].ToString());
						grid.CurrentCell = new DataGridCell(0, 0);
						AssertEquals(value.ToString(zCalcEditColumnStyle.Format), zCalcEditColumnStyle.TextBox.Text);
					}
				}
			}
		}

		public void TestAddNew_WithMaximumRows_DoesNotThrowWhenBoundListIsNotADataView()
		{
			var dummy = DummyBusinessObject.New(Factory);
			dummy.Collection.AddNew();

			using (var form = new ZForm(dummy))
			{
				var grid = new ZGrid();
				grid.MaximumRows = 2;
				grid.BindTo = "Collection";
				form.Controls.Add(grid);

				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Description });
				form.Show();

				AssertNoExceptionThrown(() => dummy.Collection.AddNew());
			}
		}

		public void TestDelete_WithMaximumRows_DoesNotThrowWhenBoundListIsNotADataView()
		{
			var dummy = DummyBusinessObject.New(Factory);
			dummy.Collection.AddNew();
			var childToRemove = dummy.Collection.AddNew();

			using (var form = new ZForm(dummy))
			{
				var grid = new ZGrid();
				grid.MaximumRows = 2;
				grid.BindTo = "Collection";
				form.Controls.Add(grid);

				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Description });
				form.Show();

				AssertNoExceptionThrown(() => dummy.Collection.RemoveAndDelete(childToRemove));
			}
		}

		#region Refresh AllowNew in Grid

		public void TestRefreshAllowNewInGrid()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Code = "D1";
			var superDummy = Factory.NewWithValidTestData<AllowNewDummy>();
			superDummy.Z0_Code = "S1";
			Factory.Save();

			using (var form = new ZForm(superDummy))
			using (var grid = new AllowNewGrid())
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "Z0_Code" });
				grid.BindTo = "SuperCollection";
				form.Controls.Add(grid);

				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				Application.DoEvents();

				superDummy.SuperCollection.AllowNewForTest = true;
				superDummy.SuperCollection.RefreshBinding();
				AssertEquals(superDummy.SuperCollection.Count + 1, grid.DataGridRowsLength);

				superDummy.SuperCollection.AllowNewForTest = false;
				superDummy.SuperCollection.RefreshBinding();
				AssertEquals(superDummy.SuperCollection.Count, grid.DataGridRowsLength);

				superDummy.SuperCollection.AllowNewForTest = true;
				superDummy.RefreshBindingIncludingChildren();
				AssertEquals(superDummy.SuperCollection.Count + 1, grid.DataGridRowsLength);

				superDummy.SuperCollection.AllowNewForTest = false;
				superDummy.RefreshBindingIncludingChildren();
				AssertEquals(superDummy.SuperCollection.Count, grid.DataGridRowsLength);

				superDummy.SuperCollection.AllowNewForTest = true;
				((IBusinessObjectState)superDummy.SuperCollection).RefreshBindingIncludingChildren();
				AssertEquals(superDummy.SuperCollection.Count + 1, grid.DataGridRowsLength);

				superDummy.SuperCollection.AllowNewForTest = false;
				((IBusinessObjectState)superDummy.SuperCollection).RefreshBindingIncludingChildren();
				AssertEquals(superDummy.SuperCollection.Count, grid.DataGridRowsLength);
			}
		}

		class AllowNewCollection : ActiveBusinessObjectCollection<DummyBusinessObject>
		{
			public AllowNewCollection(BusinessObjectFactory factory)
				: base(factory)
			{
				AllowNewForTest = true;
			}

			protected override bool AllowNew
			{
				get { return AllowNewForTest; }
			}

			public bool AllowNewForTest { get; set; }
		}

		class AllowNewDummy : DummyBusinessObject
		{
			public AllowNewDummy(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public AllowNewCollection SuperCollection
			{
				get
				{
					if (superCollection == null)
					{
						superCollection = new AllowNewCollection(Factory);
						RegisterEditableChildObject(superCollection);
					}
					return superCollection;
				}
			}
			AllowNewCollection superCollection;
		}

		class AllowNewGrid : ZGrid
		{
			public new int DataGridRowsLength
			{
				get { return base.DataGridRowsLength; }
			}
		}

		#endregion

		#region IListEditableControl

		public void TestPreserveCurrentSelection()
		{
			using (var form = new ZForm(Dummy))
			using (var grid = new ZGrid())
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Description });

				form.Controls.Add(grid);
				grid.SetDataBinding(Dummy, "Collection", Dummy.Collection.GetType().Name);
				form.Show();
				Dummy.Collection.AddNew();
				Dummy.Collection.AddNew();

				grid.CurrentCell = new DataGridCell(1, 0);
				IListEditableControl listEditableControl = grid;
				using (listEditableControl.PreserveCurrentSelection())
				{
					grid.CurrentCell = new DataGridCell(0, 0);
				}
				AssertEquals("Current cell selection preserved", new DataGridCell(1, 0), grid.CurrentCell);
			}
		}

		[ExpectNoExceptions]
		public void TestPreserveCurrentSelection_NoExceptionIfCellSelectionNoLongerValid()
		{
			using (var form = new ZForm(Dummy))
			using (var grid = new ZGrid())
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Description });

				form.Controls.Add(grid);
				form.Show();
				grid.SetDataBinding(Dummy, "Collection");
				Dummy.Collection.AddNew();
				Dummy.Collection.AddNew();

				grid.CurrentCell = new DataGridCell(0, 1);
				IListEditableControl listEditableControl = grid;
				using (listEditableControl.PreserveCurrentSelection())
				{
					grid.CurrentCell = new DataGridCell(0, 0);
					Dummy.Collection.RemoveAndDeleteAll();
				}
			}
		}

		#endregion

		[ExpectNoExceptions]
		public void TestListManagerDataError_RethrowsDatabaseUpgradeException()
		{
			var bizChild = Factory.New<DummyChildBusinessObject>();

			var bizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			bizo.Collection.Add(bizChild);
			bizo.RegisterEditableChildObject(bizo.Collection);

			var mockEnv = new DbEnvironmentWithMockGuiPluginForTest();
			using (DbEnv.SetTemporaryDbEnvironment(mockEnv))
			using (var testForm = new ZChildForm(bizo))
			{
				var grid = new TestZGrid { Dock = DockStyle.Fill };
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = AutoDummyBizo.Schema.Z0_Code });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = AutoDummyBizo.Schema.Z0_Decimal });
				testForm.Controls.Add(grid);

				var guidFindBox = new ZGuidFindBoxThatThrowsDatabaseUpgradedException();
				testForm.Controls.Add(guidFindBox);

				var textBox = new ZTextBox();
				testForm.Controls.Add(textBox);

				grid.SetDataBinding(bizo, "Collection");
				guidFindBox.SetDataBinding(bizo, "Collection.Z0_Guid");
				guidFindBox.BindToList = "Collection";

				testForm.Show();
				grid.Focus();
				grid.Select(0);
				guidFindBox.ShouldThrow = true;

				textBox.Focus();
				mockEnv.MockPlugin.Verify(x => x.HandleDatabaseUpgradeException(It.IsAny<DatabaseUpgradeException>()), Times.Once);
				mockEnv.MockPlugin.VerifyNoOtherCalls();
			}
		}

		public void TestDataVersionLogsMenuItemUsingAuditData()
		{
			var testBizoWithLogs = Factory.New<DummyEnterpriseBusinessObject>();
			testBizoWithLogs.IsDataVersionsAutoLogged = true;
			var testBizoWithoutLogs = Factory.New<DummyEnterpriseBusinessObject>();
			testBizoWithoutLogs.IsDataVersionsAutoLogged = false;
			var gridBizo = Factory.New<DummyBusinessObject>();
			gridBizo.Collection.AddRange(testBizoWithLogs, testBizoWithoutLogs);

			using (var form = new ZTestGridForm(gridBizo))
			{
				form.Show();

				form.TabGrid.SetCurrentHitTestForTest(1, 0);
				form.TabGrid.OnPopup_CallForTesting();
				AssertEquals("viewDataVersionLogsMenuItem.Visible", expected: false, form.TabGrid.viewDataVersionLogsMenuItem.Visible);
				AssertEquals("viewDataVersionLogsMenuItem.Enabled", expected: false, form.TabGrid.viewDataVersionLogsMenuItem.Enabled);

				form.TabGrid.SetCurrentHitTestForTest(0, 0);
				form.TabGrid.OnPopup_CallForTesting();
				AssertEquals("viewDataVersionLogsMenuItem.Visible", expected: true, form.TabGrid.viewDataVersionLogsMenuItem.Visible);
				AssertEquals("viewDataVersionLogsMenuItem.Enabled", expected: true, form.TabGrid.viewDataVersionLogsMenuItem.Enabled);

				ZFormModaliser.LastFormShownForTest = null;
				form.TabGrid.viewDataVersionLogsMenuItem.PerformClick();
				using (var shownForm = ZFormModaliser.LastFormShownForTest as IZAuditLogsForm)
				{
					AssertNotNull("ZAuditLogsForm should be shown if bizo support logging.", shownForm);
				}

				form.IsDataVersionLogsMenuItemVisible = false;
				form.TabGrid.OnPopup_CallForTesting();
				AssertEquals("viewDataVersionLogsMenuItem.Visible", expected: false, form.TabGrid.viewDataVersionLogsMenuItem.Visible);
				AssertEquals("viewDataVersionLogsMenuItem.Enabled", expected: false, form.TabGrid.viewDataVersionLogsMenuItem.Enabled);

				ZFormModaliser.LastFormShownForTest = null;
				form.TabGrid.viewDataVersionLogsMenuItem.PerformClick();
				AssertNull("ZAuditLogsForm should not be shown if bizo doesn't support logging.", ZFormModaliser.LastFormShownForTest);

				form.IsDataVersionLogsMenuItemVisible = true;
				form.TabGrid.OnPopup_CallForTesting();
				AssertEquals("viewDataVersionLogsMenuItem.Visible", expected: true, form.TabGrid.viewDataVersionLogsMenuItem.Visible);
				AssertEquals("viewDataVersionLogsMenuItem.Enabled", expected: true, form.TabGrid.viewDataVersionLogsMenuItem.Enabled);

				SystemDataRegistry.Instance.BiAuditServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "No valid");
				ZFormModaliser.LastFormShownForTest = null;
				form.TabGrid.viewDataVersionLogsMenuItem.PerformClick();
				AssertNull("ZAuditLogsForm should not be shown if audit server is not valid.", ZFormModaliser.LastFormShownForTest);
				var errorMessage = "Audit server is not valid.";
				AssertEquals(errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSetDataBindingForMetadataPropertiesWhenDataSourceIsEmptyCollection()
		{
			var bizChild = Factory.New<DummyChildBusinessObject>();

			var bizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			bizo.Collection.Add(bizChild);
			bizo.RegisterEditableChildObject(bizo.Collection);

			using (var testForm = new ZChildForm(bizo))
			{
				var grid = new TestZGrid { Dock = DockStyle.Fill };
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = AutoDummyBizo.Schema.Z0_Code });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = AutoDummyBizo.Schema.Z0_Decimal });
				testForm.Controls.Add(grid);

				var guidFindBox = new ZGuidFindBox();
				testForm.Controls.Add(guidFindBox);

				grid.SetDataBinding(bizo.Collection, "");
				guidFindBox.SetDataBinding(bizo.Collection, "Z0_Guid");
				guidFindBox.BindToList = "";

				testForm.Show();

				using (bizo.Collection.SuspendListChanged())
				{
					bizo.Collection.Remove(bizChild);
					AssertNoExceptionThrown("SetDataBinding should not throw exception.", () => guidFindBox.SetDataBinding(bizo.Collection, "Z0_Guid"));
				}

				bizo.Collection.Add(bizChild);
				AssertEquals("Should resume binding.", false, grid.ListManager.IsBindingSuspended);
			}
			ErrorReporter.Clear();
		}

		#region Test Classes

		public class ZTestGrid : ZGrid
		{
			public new void OnEnter(EventArgs e)
			{
				base.OnEnter(e);
			}

			public void HookToOnRemovingBizOFromList()
			{
				OnRemovingBizOFromList += OnRemovingBizO;
			}

			public ContinueWithRemove OnRemovingBizO(BusinessObject bizO)
			{
				return ((DummyBaseBusinessObject)bizO).Z0_Bool ? ContinueWithRemove.Remove : ContinueWithRemove.CancelRemoval;
			}

			public new void GridVScrolled(object sender, ScrollEventArgs e)
			{
				base.GridVScrolled(sender, e);
			}

			public new void OnMouseDown(MouseEventArgs e)
			{
				base.OnMouseDown(e);
			}

			public new void OnMouseMove(MouseEventArgs e)
			{
				base.OnMouseMove(e);
			}

			public new void OnMouseUp(MouseEventArgs e)
			{
				base.OnMouseUp(e);
			}

			public void FireMouseDown(int x, int y)
			{
				FireMouseDown(x, y, 1);
			}

			public void FireMouseDown(int x, int y, int clicks)
			{
				OnMouseDown(new MouseEventArgs(MouseButtons.Left, clicks, x, y, 0));
			}

			protected override MouseButtons MouseButtonState
			{
				get { return mouseButtonStateSet ? mouseButtonState : base.MouseButtonState; }
			}

			public void SetMouseButtonState(MouseButtons mouseButtons)
			{
				mouseButtonStateSet = (mouseButtons != MouseButtons.None);
				this.mouseButtonState = mouseButtons;
			}

			bool mouseButtonStateSet;
			MouseButtons mouseButtonState;

			protected override Point LocalMousePosition
			{
				get { return mousePositionSet ? mousePosition : base.LocalMousePosition; }
			}

			public void SetMousePosition(Point mousePos)
			{
				mousePositionSet = (mousePos != Point.Empty);
				this.mousePosition = mousePos;
			}

			bool mousePositionSet;
			Point mousePosition;

			protected override void OnPaint(PaintEventArgs e)
			{
				try
				{
					base.OnPaint(e);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Globals.Message.ShowDeveloperException(ex);
					throw;
				}
			}

			public new Type ElementTypeFromCollection
			{
				get { return base.ElementTypeFromCollection; }
			}

			public new int DataGridRowsLength
			{
				get { return base.DataGridRowsLength; }
			}

#if !WINZOR

			public void OnMouseWhell(MouseEventArgs e)
			{
				base.OnMouseWheel(e);
			}

			public void SetIsClickingRowHeader(bool value) { isClickingRowHeader = value; }

			public void SetIsClickingColumnHeader(bool value) { isClickingColumnHeader = value; }

#endif
		}

		#region class Validation Dummy

		internal class ValidationDummy : DummyChildEnterpriseBusinessObject
		{
			public ValidationDummy(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			protected override DummyBizoValidation GetNewValidation()
			{
				return new ValidationDummyValidation(this);
			}
		}

		internal class DummyBusinessObjectWithCustomField : DummyChildEnterpriseBusinessObject, ICustomFieldProvider
		{
			readonly CustomBusinessObject customBusinessObject;
			public DummyBusinessObjectWithCustomField(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
				var stringProp = new DummyCustomProperty("STR", new ZString("String"), typeof(ZString));
				var boolProp = new DummyCustomProperty("BOOL", ZBool.False, typeof(ZBool));

				customBusinessObject = new CustomBusinessObject(this, CustomPropertyCollectionBuilder.GetCustomProperties(new ICustomProperty[] { stringProp, boolProp }));
			}

			CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
			{
				return customBusinessObject;
			}
		}

		class DummyCustomProperty : ICustomProperty
		{
			public DummyCustomProperty(string identifier, object value, Type type)
			{
				this.identifier = identifier;
				this.value = value;
				this.type = type;
			}

			readonly string identifier;
			object value;
			readonly Type type;

			public string Identifier
			{
				get { return identifier; }
			}

			public object GetValue(BusinessObject parent)
			{
				return value;
			}

			public bool TrySetValue(BusinessObject parent, object value)
			{
				this.value = value;
				return true;
			}

			public DynamicBusinessObjectProperty Info
			{
				get { return new DynamicBusinessObjectProperty(type, false, metaData: type == typeof(ZString) ? new DynamicMetaData[] { new MaxLengthImpl(26) } : Array.Empty<DynamicMetaData>()); }
			}

			public void Validate(BusinessObject parent)
			{
			}

			IEnumerable<ICustomProperty> ICustomProperty.RelatedProperties => Enumerable.Empty<ICustomProperty>();

			ICustomColumnDefinition ICustomProperty.CustomColumnDefinition => null;

			bool ICustomProperty.IsDeleted => false;
		}

		class NullPropertyDescriptor : PropertyDescriptor
		{
			public NullPropertyDescriptor(string name, Attribute[] attributes)
				: base(name, attributes)
			{
			}

			public override Type ComponentType
			{
				get { return typeof(object); }
			}

			public override bool IsReadOnly
			{
				get { return true; }
			}

			public override Type PropertyType
			{
				get { return typeof(object); }
			}

			public override bool CanResetValue(object component)
			{
				return true;
			}

			public override object GetValue(object component)
			{
				return null;
			}

			public override void ResetValue(object component)
			{
			}

			public override void SetValue(object component, object value)
			{
			}

			public override bool ShouldSerializeValue(object component)
			{
				return false;
			}
		}

		class MaxLengthImpl : DynamicMetaData
		{
			public MaxLengthImpl(int maxLength) : base(MetaDataTypes.MaxLength, maxLength) { }
		}

		internal class ValidationDummyValidation : DummyBizoValidation
		{
			public ValidationDummyValidation(ValidationDummy parent) : base(parent) { }

			protected override void CheckZ0_Code()
			{
				base.CheckZ0_Code();
				Parent.Z0_CodeInfo.AddError("Error");
			}
		}

		#endregion

		internal class DbEnvironmentWithMockGuiPluginForTest : BaseDbEnvironment
		{
			public Mock<IDbConnectionGuiPlugin> MockPlugin = new Mock<IDbConnectionGuiPlugin>();
			public override IDbConnectionGuiPlugin ConnectionGuiPlugin => MockPlugin.Object;
		}

		class ZGuidFindBoxThatThrowsDatabaseUpgradedException : ZGuidFindBox
		{
			internal bool ShouldThrow;

			protected override void OnFormatValue(ConvertEventArgs e)
			{
				if (ShouldThrow)
				{
					throw new DatabaseUpgradedException();
				}
				base.OnFormatValue(e);
			}
		}

		#endregion

		#region Implementation

		new DummyEnterpriseBusinessObject Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyEnterpriseBusinessObject>()); }
		}
		DummyEnterpriseBusinessObject dummy;

		internal static void FocusAndEditGrid(ZGrid grid)
		{
			grid.Focus();
			Application.DoEvents();

			grid.CurrentCell = new DataGridCell(0, 1);
			Application.DoEvents();

			grid.CurrentCell = new DataGridCell(0, 0);
			Application.DoEvents();
		}

		#endregion
	}

#if !WINZOR

	sealed class ZGridNonTransactionTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestCurrentCellSetter_WhenDatabaseUpgraded_DbConnectionIsHealthy()
		{
			var bumpedSchemaVersion = ObjectFactory.Get<IDatabaseAspectVersions>().SchemaVersion.Major + 10;

			var versionMock = new Mock<IDatabaseAspectVersions>();
			versionMock
				.Setup(x => x.SchemaVersion)
				.Returns(new VersionLabel(bumpedSchemaVersion, 0));

			var mockEnv = new ZGridTest.DbEnvironmentWithMockGuiPluginForTest();

			var factory = new BusinessObjectFactory();
			var obj1 = factory.New<ZGridTest.DummyBusinessObjectWithAllowNewToggleCollection>();

			using (DbEnv.SetTemporaryDbEnvironment(mockEnv))
			using (var form = new ZForm(obj1))
			{
				form.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 500);

				var grid = new ZGridTest.ZTestGrid();
				grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
				grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 200);
				form.Controls.Add(grid);

				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Code });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Description });

				form.Show();
				grid.SetDataBinding(obj1, "AllowNewCollection");

				AssertEquals("Precondition: ", 0, obj1.AllowNewCollection.Count);
				AssertEquals("Precondition: ", true, obj1.AllowNewCollection.AllowNew);
				grid.CurrentCell = new DataGridCell(0, 0);
				ZGridTest.FocusAndEditGrid(grid);
				ZGridTest.PostKeyToGrid(grid, Keys.A);
				AssertEquals("Precondition", new DataGridCell(0, 0), grid.CurrentCell);
				AssertEquals("Precondition: ", 1, obj1.AllowNewCollection.Count);
				AssertEquals("Precondition: ", true, obj1.AllowNewCollection.AllowNew);

				grid.LastFocusedColumn.TextBox.Visible = true;

				Db.Connection.CloseConnection();
				var countDatabaseUpgradedException = 0;
				ConnectionState? createRowConnectionState = null;
				bool? createRowIsUpgradeCheckDisabled = null;
				// Setting the CurrentCell to a new row will create a new business object.
				// Simulate it hitting the database, e.g., to check a registry.
				obj1.AllowNewCollection.ActionForCreateInitialisedBusinessObjectFromRow = () =>
				{
					try
					{
						createRowConnectionState = Db.Connection.State;
						createRowIsUpgradeCheckDisabled = Db.Connection.IsUpgradeCheckDisabled;
						Db.Connection.EnsureIsOpen();
					}
					catch (DatabaseUpgradedException)
					{
						++countDatabaseUpgradedException;
						throw;
					}
				};

				using (ObjectFactory.Substitute(versionMock.Object))
				{
					grid.CurrentCell = new DataGridCell(1, 0);
				}

				CombineAssertions(() =>
				{
					AssertEquals("countDatabaseUpgradedException", 0, countDatabaseUpgradedException);
					AssertEquals("createRowConnectionState", ConnectionState.Closed, createRowConnectionState);
					AssertEquals("createRowIsUpgradeCheckDisabled", true, createRowIsUpgradeCheckDisabled);

					AssertEquals("ConnectionState", ConnectionState.Closed, Db.Connection.State);
					AssertEquals("IsUpgradeCheckDisabled", false, Db.Connection.IsUpgradeCheckDisabled);
					AssertEquals("DatabaseUpgradedExceptionHasBeenThrown", false, Db.Connection.DatabaseUpgradedExceptionHasBeenThrown);
					AssertEquals("TotalErrorCount", 0, ErrorReporter.TotalErrorCount);
				});
			}
		}
	}

#endif

}
