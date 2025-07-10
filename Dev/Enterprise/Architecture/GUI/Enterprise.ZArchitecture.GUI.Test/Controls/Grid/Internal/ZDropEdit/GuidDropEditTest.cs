using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.ZArchitecture.GUI
{
	sealed class GuidDropEditTest : TestCaseWithFactory
	{
		public void TestFormatValueHandlesStringAndGuid()
		{
			var styleInfo = new ZGuidDropEditColumnStyleInfo();
			using (var style = new ZGuidDropEditColumnStyleWithGetCodeExposed(styleInfo))
			{
				var code = new DummyCodeDescription { Code = "COD", PK = ZGuid.NewZGuid() };
				style.ListOverride = new List<ICodeDescription> { code };

				AssertEquals(code.Code, style.FormatValueObject(null, code.PK));
				AssertEquals(code.Code, style.FormatValueObject(null, code.Code));
			}
		}

		class DummyCodeDescription : ICodeDescription
		{
			public object PK { get; set; }
			public string Code { get; set; }
			public string Description { get; set; }
		}

		public void TestEndToEnd()
		{
			var bizobj = Factory.New<DummyBusinessObjectWithList>();

			var relatedBizObj0 = bizobj.List.AddNew();
			relatedBizObj0.Z0_Code = "Z";
			relatedBizObj0.Z0_Guid = relatedBizObj0.PK;

			var relatedBizObj1 = bizobj.List.AddNew();
			relatedBizObj1.Z0_Code = "A";
			relatedBizObj1.Z0_Guid = relatedBizObj1.PK;

			var relatedBizObj2 = bizobj.List.AddNew();
			relatedBizObj2.Z0_Code = "C";
			relatedBizObj2.Z0_Guid = relatedBizObj2.PK;

			var relatedBizObj3 = bizobj.List.AddNew();
			relatedBizObj3.Z0_Code = "D";
			relatedBizObj3.Z0_Guid = relatedBizObj3.PK;

			relatedBizObj0.LookupList.Add(relatedBizObj0);
			relatedBizObj0.LookupList.Add(relatedBizObj1);
			relatedBizObj0.LookupList.Add(relatedBizObj2);

			using (var form = new ZGuidDropEditTestForm(bizobj))
			{
				form.Show();
				UserIdleWorker.Flush();

				KeySender.PostKeyDown(form, form.Handle, Keys.Tab);
				KeySender.PostKeyDown(form, form.Handle, Keys.Tab);
				Application.DoEvents();

				AssertEquals(typeof(ZGridGuidDropEdit), form.ActiveControl.GetType());

				var codeEdit = (ZGridGuidDropEdit)form.ActiveControl;

				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.A);
				Application.DoEvents();
				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.Tab);
				Application.DoEvents();

				AssertEquals("Codes should be correctly linked", relatedBizObj1.Z0_Guid, relatedBizObj0.Z0_Guid);

				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.Shift | Keys.Tab);
				Application.DoEvents();
				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.D);
				Application.DoEvents();
				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.Tab);
				Application.DoEvents();

				AssertEquals(ZGuid.Invalid, relatedBizObj0.Z0_Guid);

				form.zGuidDropEditColumnStyleInfo1.Parse = delegate(ZString code) { return relatedBizObj3.PK; };

				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.Shift | Keys.Tab);
				Application.DoEvents();
				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.A);
				Application.DoEvents();
				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.Tab);
				Application.DoEvents();

				AssertEquals("Codes should be correctly linked", relatedBizObj1.Z0_Guid, relatedBizObj0.Z0_Guid);

				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.Shift | Keys.Tab);
				Application.DoEvents();
				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.D);
				Application.DoEvents();
				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.Tab);
				Application.DoEvents();

				AssertEquals(relatedBizObj3.PK, relatedBizObj0.Z0_Guid);
			}
		}

		public void TestDropEditSelectionPopulateCorrectGUIDForDuplicateCodes()
		{
			var bizobj = Factory.New<DummyBusinessObjectWithList>();
			bizobj.List.RemoveAll();

			var relatedBizObj0 = bizobj.List.AddNew();
			relatedBizObj0.Z0_Code = "A";
			relatedBizObj0.Z0_Guid = relatedBizObj0.PK;

			var relatedBizObj1 = bizobj.List.AddNew();
			relatedBizObj1.Z0_Code = "A";
			relatedBizObj1.Z0_Guid = relatedBizObj1.PK;

			relatedBizObj0.LookupList.Add(relatedBizObj0);
			relatedBizObj0.LookupList.Add(relatedBizObj1);

			using (var form = new ZGuidDropEditTestForm(bizobj))
			{
				form.Show();
				UserIdleWorker.Flush();

				KeySender.PostKeyDown(form, form.Handle, Keys.Tab);
				KeySender.PostKeyDown(form, form.Handle, Keys.Tab);
				Application.DoEvents();

				AssertEquals(typeof(ZGridGuidDropEdit), form.ActiveControl.GetType());

				var codeEdit = (ZGridGuidDropEdit)form.ActiveControl;
				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.Alt | Keys.Down);
				Application.DoEvents();
				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.Tab);
				Application.DoEvents();

				AssertEquals("Codes should be correctly linked", relatedBizObj0.PK, bizobj.List[0].Z0_Guid);

				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.Shift | Keys.Tab);
				Application.DoEvents();
				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.Alt | Keys.Down);
				Application.DoEvents();
				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.Down);
				Application.DoEvents();
				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.Tab);
				Application.DoEvents();

				AssertEquals("Codes should be correctly linked", relatedBizObj1.PK, bizobj.List[0].Z0_Guid);
			}
		}

		public void TestDropEditSelectionChangedCanTriggerColumnStartedEditing()
		{
			var bizobj = Factory.New<DummyBusinessObjectWithList>();

			var relatedBizObj1 = Factory.New<DummyBusinessObjectInList>();
			relatedBizObj1.Z0_Code = "B";
			Factory.Save();

			using (var form = new ZGuidDropEditTestForm(bizobj))
			{
				form.zGuidDropEditColumnStyleInfo1.BindToList = "LookupListFromDataBase";
				form.Show();
				UserIdleWorker.Flush();

				KeySender.PostKeyDown(form, form.Handle, Keys.Tab);
				KeySender.PostKeyDown(form, form.Handle, Keys.Tab);
				Application.DoEvents();

				AssertEquals(typeof(ZGridGuidDropEdit), form.ActiveControl.GetType());

				var codeEdit = (ZGridGuidDropEdit)form.ActiveControl;
				codeEdit.SelectItem("B");
				Application.DoEvents();
				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.Tab);
				Application.DoEvents();

				Assert(bizobj.List.Count > 0);
				AssertEquals("Codes should be correctly linked", relatedBizObj1.PK, bizobj.List[0].Z0_Guid);
			}
		}

		public void TestReadOnlyColumnDisplaysCodeText()
		{
			var bizobj = Factory.New<DummyBusinessObjectWithList>();

			var relatedBizObj0 = bizobj.List.AddNew();
			relatedBizObj0.Z0_Code = "Z";
			relatedBizObj0.Z0_Guid = relatedBizObj0.PK;
			relatedBizObj0.LookupList.Add(relatedBizObj0);

			var relatedBizObj1 = bizobj.List.AddNew();
			relatedBizObj1.Z0_Code = "A";
			relatedBizObj1.Z0_Guid = relatedBizObj1.PK;
			relatedBizObj1.LookupList.Add(relatedBizObj1);

			var relatedBizObj2 = bizobj.List.AddNew();
			relatedBizObj2.Z0_Code = "C";
			relatedBizObj2.Z0_Guid = relatedBizObj2.PK;
			relatedBizObj2.LookupList.Add(relatedBizObj2);

			bizobj.SetReadOnlyIncludingChildren(true);

			relatedBizObj0.SetReadOnlyIncludingChildren(true);
			relatedBizObj1.SetReadOnlyIncludingChildren(true);
			relatedBizObj2.SetReadOnlyIncludingChildren(true);

			using (var form = new ZGuidDropEditTestForm(bizobj))
			{
				form.Show();
				form.Grid.Focus();

				AssertEquals(typeof(DataGridTextBox), form.ActiveControl.GetType());
				var textBox = (DataGridTextBox)form.ActiveControl;
				AssertEquals("should have code value not guid or raw data", "Z", textBox.Text);
			}
		}

		public void TestSorting()
		{
			var bizobj = Factory.New<DummyBusinessObjectWithList>();

			var relatedBizObj0 = bizobj.List.AddNew();
			relatedBizObj0.Z0_Code = "Z";
			relatedBizObj0.Z0_Guid = relatedBizObj0.PK;
			relatedBizObj0.LookupList.Add(relatedBizObj0);

			var relatedBizObj1 = bizobj.List.AddNew();
			relatedBizObj1.Z0_Code = "A";
			relatedBizObj1.Z0_Guid = relatedBizObj1.PK;
			relatedBizObj1.LookupList.Add(relatedBizObj1);

			var relatedBizObj2 = bizobj.List.AddNew();
			relatedBizObj2.Z0_Code = "C1";
			relatedBizObj2.Z0_Guid = relatedBizObj2.PK;
			relatedBizObj2.LookupList.Add(relatedBizObj2);

			var relatedBizObj3 = bizobj.List.AddNew();
			relatedBizObj3.Z0_Code = "C0";
			relatedBizObj3.Z0_Guid = relatedBizObj3.PK;
			relatedBizObj3.LookupList.Add(relatedBizObj3);

			using (var form = new ZGuidDropEditTestForm(bizobj))
			{
				form.Show();
				form.Grid.Focus();

				const int ClickX = 80;
				const int ClickY = 10;

				var grid = form.Grid;
				var eventArgs = new MouseEventArgs(MouseButtons.Left, 1, ClickX, ClickY, 0);
				grid.PerformMouseDownForTest(eventArgs, -1, 0);
				grid.PerformMouseUpForTest(eventArgs, -1, 0);

				AssertEquals("Click will hit ColumnHeader", DataGrid.HitTestType.ColumnHeader, form.Grid.HitTest(new Point(ClickX, ClickY)).Type);
				AssertEquals("A", ((DummyBusinessObjectInList)form.Grid.ListManager.List[0]).Z0_Code);
				AssertEquals("C0", ((DummyBusinessObjectInList)form.Grid.ListManager.List[1]).Z0_Code);
				AssertEquals("C1", ((DummyBusinessObjectInList)form.Grid.ListManager.List[2]).Z0_Code);
				AssertEquals("Z", ((DummyBusinessObjectInList)form.Grid.ListManager.List[3]).Z0_Code);

				grid.PerformMouseDownForTest(eventArgs, -1, 0);
				grid.PerformMouseUpForTest(eventArgs, -1, 0);

				AssertEquals("Z", ((DummyBusinessObjectInList)form.Grid.ListManager.List[0]).Z0_Code);
				AssertEquals("C1", ((DummyBusinessObjectInList)form.Grid.ListManager.List[1]).Z0_Code);
				AssertEquals("C0", ((DummyBusinessObjectInList)form.Grid.ListManager.List[2]).Z0_Code);
				AssertEquals("A", ((DummyBusinessObjectInList)form.Grid.ListManager.List[3]).Z0_Code);
			}
		}

		[NUnit.Framework.ExpectNoExceptions]
		public void TestGetPK_WhenCurrencyManagerIsNull_ShouldNotThrow()
		{
			using (var columnStyle = new ZGuidDropEditColumnStyle(new ZGuidDropEditColumnStyleInfo()))
			{
				columnStyle.GetPK(null, 1, "ABC");
			}
		}

		[TestExcludeZWinFormHasTypedConstructor]
		[TestExcludeZWinFormsAllHaveFormBashers]
		public class ZGuidDropEditTestForm : ZTestForm
		{
			public ZGuidDropEditTestForm(BusinessObject bizObj)
				: base(bizObj)
			{
			}

			public ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1;

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				this.Grid.ColumnStyles.Clear();
				Grid.BindTo = "List";
				zGuidDropEditColumnStyleInfo1 = new ZGuidDropEditColumnStyleInfo();
				zGuidDropEditColumnStyleInfo1.BindToList = "LookupList";
				zGuidDropEditColumnStyleInfo1.Caption = "LOL";
				zGuidDropEditColumnStyleInfo1.ColumnName = "Z0_Guid";
				this.Grid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			}
		}

		public void TestGetEmptyColumnValue()
		{
			var columnInfo = new ZGuidDropEditColumnStyleInfo();
			columnInfo.ColumnName = "qwerty";
			using (var style = new ZGuidDropEditColumnStyleWithGetCodeExposed(columnInfo))
			{
				AssertEquals(ZGuid.Empty, style.GetColumnGuidValue(GetCurrencyManager(), 0));
			}
		}

		public void TestGetCodeForNonZGuidPK_ShouldReportUsefulMessage()
		{
			var columnInfo = new ZGuidDropEditColumnStyleInfo();
			columnInfo.ColumnName = "MaiColumn";
			using (var style = new ZGuidDropEditColumnStyleWithGetCodeExposed(columnInfo))
			{
				style.GetColumnValueAtRowOverride = "Nya nya not a guid";

				style.GetColumnGuidValue(GetCurrencyManager(), 0);
				try
				{
					AssertEquals(
@"Expected value [Nya nya not a guid] for ColumnName [MaiColumn] to be a ZGuid but it was a System.String.
source.Position = -1, source.Count = 0, rowNum = 0.
PropertyDescriptor Type: <null>, Name:",
						ErrorReporter.LastMessageReported.Trim());
				}
				finally
				{
					ExceptionReporterTestListener.Instance.Clear();
				}
			}
		}

		[NUnit.Framework.ExpectNoExceptions]
		public void TestGetCodeForNonZGuidListItemPK_ShouldNotExplode()
		{
			var columnInfo = new ZGuidDropEditColumnStyleInfo();
			columnInfo.ColumnName = "MaiColumn";
			using (var style = new ZGuidDropEditColumnStyleWithGetCodeExposed(columnInfo))
			{
				var listItem = new TestGetCodeForNonZGuidListItemPK_TestingClass
				{
					PKReturns = "So you want to cast me?"
				};
				style.ListOverride = new List<ICodeDescription> { listItem };
				style.GetCode(GetCurrencyManager(), 1, ZGuid.NewZGuid());
			}
		}

		public void TestPrepareEditControl_WhenDropListIsNull()
		{
			var styleInfo = new ZGuidDropEditColumnStyleInfo();

			using (var style = new ZGuidDropEditColumnStyleWithGetCodeExposed(styleInfo))
			using (var grid = new ZGrid())
			{
				style.parentDataGrid = grid;
				style.parentDataGrid.ReadOnly = true;
				style.IsEditing = false;
				style.DropEdit.List = null;
				style.DropEdit.Name = "Test";

				style.PrepareEditControlExposed(GetCurrencyManager(), 0, new Rectangle(), false);

				AssertEquals("Test_DropEditListIsNull", ErrorReporter.LastKeyReported);
				AssertEquals("Control is:\r\nTest (ZGridGuidDropEdit)\r\nrowNum: 0\r\nsource.Count: 0\r\nDropEdit.CurrentItem != null: False", ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();
			}
		}

		public void TestZGuidDropEditAddText()
		{
			var bizobj = Factory.New<DummyBusinessObjectWithList>();

			var relatedBizObj0 = bizobj.List.AddNew();
			relatedBizObj0.Z0_Code = "Z";
			relatedBizObj0.Z0_Guid = relatedBizObj0.PK;
			relatedBizObj0.LookupList.Add(relatedBizObj0);

			using (var form = new ZGuidDropEditTestForm(bizobj))
			{
				form.Show();
				form.Grid.Focus();

				var grid = form.Grid as DataGrid;

				AssertEquals("Z", ((DummyBusinessObjectInList)form.Grid.ListManager.List[0]).Z0_Code);
			}
		}

		class ZGuidDropEditColumnStyleWithGetCodeExposed : ZGuidDropEditColumnStyle
		{
			internal ZGuidDropEditColumnStyleWithGetCodeExposed(ZGuidDropEditColumnStyleInfo columnInfo)
				: base(columnInfo)
			{
			}

			protected override IEnumerable<ICodeDescription> GetList(object valueAtRow)
			{
				return ListOverride;
			}

			internal IEnumerable<ICodeDescription> ListOverride { get; set; }

			internal new string GetCode(CurrencyManager source, int rowNum)
			{
				return base.GetCode(source, rowNum);
			}

			internal string GetCode(CurrencyManager source, int rowNum, ZGuid pk)
			{
				return source.List != null && rowNum > -1 && rowNum < source.List.Count
					? GetCode(source.List[rowNum], pk)
					: string.Empty;
			}

			internal void PrepareEditControlExposed(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly)
			{
				PrepareEditControl(source, rowNum, bounds, readOnly);
			}
		}

		static CurrencyManager GetCurrencyManager()
		{
			return (CurrencyManager)Activator.CreateInstance(typeof(CurrencyManager), BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { Array.Empty<Guid>() }, System.Globalization.CultureInfo.CurrentCulture);
		}
	}
}
