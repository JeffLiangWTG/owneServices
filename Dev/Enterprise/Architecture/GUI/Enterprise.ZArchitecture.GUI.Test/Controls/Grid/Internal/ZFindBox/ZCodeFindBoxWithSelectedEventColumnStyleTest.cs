using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules.Internal;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZCodeFindBoxWithSelectedEventColumnStyleTest : TestCaseWithDummy
	{
		public void TestSelectedEventFired()
		{
			var dummyChild1 = Dummy.Collection.AddNew();
			dummyChild1.Z0_Code = "Z1";
			Factory.Save();

			using (var form = new ZForm(Dummy))
			{
				var eventFired = false;

				var grid = new ZGrid();
				grid.BindTo = "Collection";
				var columnStyleInfo = new ZCodeFindBoxWithSelectedEventColumnStyleInfo();
				columnStyleInfo.BindToList = "Lookups+DummyList";
				columnStyleInfo.ModuleID = DummyModuleIDs.Dummy;
				columnStyleInfo.ColumnName = "Z0_Code";
				grid.ColumnStyles.Add(columnStyleInfo);
				grid.Dock = System.Windows.Forms.DockStyle.Fill;

				columnStyleInfo.Selected += (sender, e) => eventFired = true;

				form.Controls.Add(grid);
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals("FindBox.Code when grid shows", "Z1", ((IFindBox)((ZCodeFindBoxColumnStyle)grid.Columns[0].ColumnStyle).EditControl).Code);

					using (var col = new ZCodeFindBoxWithSelectedEventColumnStyleForTest(columnStyleInfo))
					{
						col.SetParentGrid(grid);
						form.Show();
						grid.Focus();
						grid.GetNextControl(grid, false).Focus();

						var findBox = col.FindBox;
						var popupDecisionProvider = new PopupModuleDecisionProvider(findBox);
						col.EnterEditControlExposed();

						popupDecisionProvider.HandleFindBoxOKButton(Array.Empty<BusinessObject>());
						AssertEquals("Selected not called when no element is selected", false, eventFired);

						popupDecisionProvider.HandleFindBoxOKButton(new[] { dummyChild1 });
						AssertEquals("Selected event called when item is selected", true, eventFired);
						AssertEquals("Z1 Selected", "Z1", findBox.Code);
					}
				});
			}
		}

		protected override Type TypeOfDummy => typeof(DummyEnterpriseBusinessObject);

		sealed class ZCodeFindBoxWithSelectedEventColumnStyleForTest : ZCodeFindBoxWithSelectedEventColumnStyle
		{
			public ZCodeFindBoxWithSelectedEventColumnStyleForTest(ZCodeFindBoxWithSelectedEventColumnStyleInfo columnInfo)
				: base(columnInfo)
			{
			}

			public void EnterEditControlExposed()
			{
				EnterEditControl(this, EventArgs.Empty);
			}
		}
	}
}
