using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.GUI.FilterStrips;
using Enterprise.ZArchitecture.Web.GUI.Utilities;
using Enterprise.ZArchitecture.Web.Modules.Testing;
using ZFilterGridModule = Enterprise.ZArchitecture.Web.Modules.ZFilterGridModule;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	class SearchControlForTest : SearchControl
	{
		public ZFilterStripControl FilterStripControlForTest;

		public override void Dispose()
		{
			if (dummyFilterStripGridModule != null)
			{
				dummyFilterStripGridModule.Dispose();
			}
			base.Dispose();
		}

		public SearchControlForTest()
		{
			GridLayoutControl = new ZGridLayoutControl();
			ResultsGridDiv = new HtmlGenericControl();
			Buttons = new PlaceHolder();
			FilterStripControlForTest = new ZFilterStripControl();
		}

		protected override void LoadAndPopulateGrid()
		{
			// do nothing for this test
		}

		public void CustomNewButtonClickForTest(object sender, EventArgs e)
		{
			CustomNewButtonClickInvoked = true;
			NewButtonUrl = null;
		}

		public bool CustomNewButtonClickInvoked;

		public void CustomViewButtonClickForTest(object sender, EventArgs e)
		{
			CustomViewButtonClickInvoked = true;
			ViewButtonUrl = null;
		}

		public bool CustomViewButtonClickInvoked;

		public void OnLoad()
		{
			base.OnLoad(EventArgs.Empty);
		}

		public override void RePopulateGrid()
		{
			RePopulateGridCounter++;
		}

		public int RePopulateGridCounter;

		public void InitialiseFilterStripControlForTest()
		{
			InitialiseFilterStripControlInternal(FilterStripControlForTest, dummyFilterStripGridModule);
		}

		public override ZFilterGridModule Module
		{
			get
			{
				if (overrideModule)
				{
					if (dummyFilterStripGridModule == null)
					{
						dummyFilterStripGridModule = new DummyZFilterStripGridModule(new BusinessObjectFactory(), Page);
					}
					return dummyFilterStripGridModule;
				}
				else
				{
					return base.Module;
				}
			}
		}

		DummyZFilterStripGridModule dummyFilterStripGridModule;

		public bool OverrideModule
		{
			set
			{
				overrideModule = value;
			}
		}

		bool overrideModule;

		public string GridColumnsLayoutKey
		{
			get
			{
				return ModuleGridLayoutHelper.GetGridColumnsLayoutKey();
			}
		}

		public GridLayoutRegistry GridLayoutRegistry
		{
			get
			{
				return gridLayoutRegistry ?? (gridLayoutRegistry = new GridLayoutRegistry());
			}
		}
		GridLayoutRegistry gridLayoutRegistry;
	}
}
