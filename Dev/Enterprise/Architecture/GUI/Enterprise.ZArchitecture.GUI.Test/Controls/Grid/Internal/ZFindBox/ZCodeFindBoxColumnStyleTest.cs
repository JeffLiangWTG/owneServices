using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Internal;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZCodeFindBoxColumnStyleTest : TestCaseWithDummy
	{
		public void TestModuleShowing()
		{
			using (var form = new ZForm(Dummy))
			{
				var grid = new ZGrid();
				grid.BindTo = "Collection";
				var columnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
				columnStyleInfo.BindToList = "Lookups+DummyList";
				columnStyleInfo.ModuleID = ModuleIDs.NotAssigned;
				columnStyleInfo.ColumnName = "Z0_Code";
				grid.ColumnStyles.Add(columnStyleInfo);
				grid.Dock = DockStyle.Fill;
				form.Controls.Add(grid);
				form.Show();

				grid.FindBoxColumnModuleShowing += delegate(object sender, FindBoxColumnModuleShowingEventArgs e)
				{
					AssertEquals(grid.Columns[0].ColumnStyle, e.ColumnStyle);
					e.ModuleID = DummyModuleIDs.Dummy;
				};

				((IFindBoxUserControl)((ZCodeFindBoxColumnStyle)grid.Columns[0].ColumnStyle).EditControl).SelectFromPopupForm();
				try
				{
					var popup = ZFormModaliser.ActiveForm as EmbeddedModulePopup;
					AssertNotNull(popup);

					Assert(popup.Module is DummyFilterGridModule);
				}
				finally
				{
					var disposable = ZFormModaliser.ActiveForm as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
			}
		}

		public void TestCharacterCasing()
		{
			DummyChildBusinessObject dummyChild = Dummy.Collection.AddNew();
			dummyChild.Z0_Code = "mEh";

			using (var form = new ZForm(Dummy))
			{
				var grid = new ZGrid();
				grid.BindTo = "Collection";
				var columnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
				columnStyleInfo.BindToList = "Lookups+DummyList";
				columnStyleInfo.ModuleID = ModuleIDs.NotAssigned;
				columnStyleInfo.CharacterCasing = CharacterCasing.Normal;
				columnStyleInfo.ColumnName = "Z0_Code";
				grid.ColumnStyles.Add(columnStyleInfo);
				grid.Dock = DockStyle.Fill;
				form.Controls.Add(grid);
				form.Show();

				AssertEquals("mEh", ((IFindBox)((ZCodeFindBoxColumnStyle)grid.Columns[0].ColumnStyle).EditControl).Code);
			}
		}

		public void TestAutoCompleteDisabling()
		{
			using (var style = new ZCodeFindBoxColumnStyle(new ZCodeFindBoxColumnStyleInfo { AutoCompleteDisabled = true }))
			{
				Assert("AutoCompleteDisabled on control must be set from style", ((IFindBoxUserControl)style.FindBox).AutoCompleteDisabled);
			}
		}

		public void TestGridFindBox_PopupSelected()
		{
			var dummyChild1 = Dummy.Collection.AddNew();
			dummyChild1.Z0_Code = "Z1";
			var dummyChild2 = Dummy.Collection.AddNew();
			dummyChild2.Z0_Code = "Z2";
			var dummyChild3 = Dummy.Collection.AddNew();
			dummyChild3.Z0_Code = "Z3";
			Factory.Save();

			using (var form = new ZForm(Dummy))
			{
				var grid = new ZGrid();
				grid.BindTo = "Collection";
				var columnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
				columnStyleInfo.BindToList = "Lookups+DummyList";
				columnStyleInfo.ModuleID = DummyModuleIDs.Dummy;
				columnStyleInfo.AllowModuleMultiSelect = true;
				columnStyleInfo.ColumnName = "Z0_Code";
				grid.ColumnStyles.Add(columnStyleInfo);
				grid.Dock = DockStyle.Fill;
				form.Controls.Add(grid);
				form.Show();

				AssertEquals("Z1", ((IFindBox)((ZCodeFindBoxColumnStyle)grid.Columns[0].ColumnStyle).EditControl).Code);

				using (var col = new ZCodeFindBoxColumnStyleForTestGridFindBoxPopupSelected(columnStyleInfo))
				{
					col.SetParentGrid(grid);
					form.Show();
					grid.Focus();
					grid.GetNextControl(grid, false).Focus();

					var findBox = col.FindBox;
					var popupDecisionProvider = new PopupModuleDecisionProviderWithMultipleSelect(findBox);
					col.CallEnterEditControlForTesting();

					CombineAssertions(() =>
					{
						popupDecisionProvider.HandleFindBoxOKButton(new[] { dummyChild1 });
						AssertEquals("PopupSelected called when only one selected dummyChild1", 1, col.PopupSelectedCalledCount);
						AssertEquals("Select Z1", "Z1", findBox.Code);

						popupDecisionProvider.HandleFindBoxOKButton(new[] { dummyChild2, dummyChild1 });
						AssertEquals("PopupSelected called when more than one selected", 2, col.PopupSelectedCalledCount);
						AssertEquals("First one is Z2", "Z2", findBox.Code);

						popupDecisionProvider.HandleFindBoxOKButton(new[] { dummyChild3, dummyChild2, dummyChild1 });
						AssertEquals("PopupSelected called +1 when more than  one selected", 3, col.PopupSelectedCalledCount);
						AssertEquals("First one is Z3", "Z3", findBox.Code);

						popupDecisionProvider.HandleFindBoxOKButton(new[] { dummyChild2 });
						AssertEquals("PopupSelected called when only one selected dummyChild2", 4, col.PopupSelectedCalledCount);
						AssertEquals("First one is Z2", "Z2", findBox.Code);
					});
				}
			}
		}

		class ZCodeFindBoxColumnStyleForTestGridFindBoxPopupSelected : ZCodeFindBoxColumnStyle
		{
			public ZCodeFindBoxColumnStyleForTestGridFindBoxPopupSelected(ZCodeFindBoxColumnStyleInfo columnInfo)
				: base(columnInfo)
			{
				PopupSelected += PopupSelectedForTest;
			}

			protected override object EditValue => null;

			public int PopupSelectedCalledCount;
			public void PopupSelectedForTest(object sender, EventArgs e)
			{
				PopupSelectedCalledCount++;
			}

			public void CallEnterEditControlForTesting()
			{
				EnterEditControl(this, EventArgs.Empty);
			}
		}

		new DummyEnterpriseBusinessObject Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyEnterpriseBusinessObject>()); }
		}
		DummyEnterpriseBusinessObject dummy;
	}
}
