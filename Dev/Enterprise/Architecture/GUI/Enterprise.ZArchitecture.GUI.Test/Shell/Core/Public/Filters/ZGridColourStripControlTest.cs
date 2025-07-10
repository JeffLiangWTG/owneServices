using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZGridColourStripControlTest : TestCaseWithFactory
	{
		public void TestStripIsTakenFromControl()
		{
			var filterStrip = new FilterStripBusinessObjectForTest();
			using (var filterControl = new FilterControlForTest(filterStrip))
			{
				var gridColorStrip = new GridColourStripBusinessObject(filterControl.FilterBusinessObject, null, null);
				using (var control = new ZGridColourStripControlForTest(gridColorStrip, filterControl))
				{
					var result = control.GetNewZFilterStripForTest();
					AssertEquals(typeof(FilterStripForTest), result.GetType());
				}
			}
		}

		public void TestStripProcessDialogKey()
		{
			var filterStrip = new FilterStripBusinessObjectForTest();
			using (var filterControl = new FilterControlForTest(filterStrip))
			{
				var gridColorStrip = new GridColourStripBusinessObject(filterControl.FilterBusinessObject, null, null);
				using (var control = new ZGridColourStripControlForTest(gridColorStrip, filterControl))
				{
					AssertEquals(0, control.StripsCount);
					control.ProcessDialogKeyExposed(Keys.Control | Keys.A);
					AssertEquals(0, control.StripsCount);
					control.ProcessDialogKeyExposed(Keys.Control | Keys.E);
					AssertEquals(1, control.StripsCount);
				}
			}
		}

		#region Test Classes

		class FilterStripBusinessObjectForTest : FilterStripBusinessObject
		{
			protected override ModuleFilterCollection GetModuleFiltersCore()
			{
				var result = new ModuleFilterCollection();
				result.AddTextFilter("desc", delegate { return new ZQuery(); });
				return result;
			}
		}

		class ZGridColourStripControlForTest : ZGridColourStripControl
		{
			public ZGridColourStripControlForTest(GridColourStripBusinessObject filter, StripControl baseFilterControl)
				: base(filter, baseFilterControl)
			{
			}

			public ZFilterStrip GetNewZFilterStripForTest()
			{
				return base.NewZFilterStrip();
			}

			public bool ProcessDialogKeyExposed(Keys keyData)
			{
				return base.ProcessDialogKey(keyData);
			}

			protected internal override void AddStrip()
			{
				StripsCount++;
			}

			public int StripsCount { get; set; }
		}

		class FilterControlForTest : ZFilterStripControl
		{
			public FilterControlForTest(FilterStripBusinessObject filter)
				: base(null, filter)
			{
			}

			protected internal override ZFilterStrip NewZFilterStrip()
			{
				fFilter = new FilterStripForTest();
				return fFilter;
			}

			FilterStripForTest fFilter;

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
				fFilter?.Dispose();
			}
		}

		class FilterStripForTest : ZFilterStrip
		{
		}

		#endregion
	}
}
