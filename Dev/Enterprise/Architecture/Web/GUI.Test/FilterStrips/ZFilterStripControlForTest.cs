using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Internal;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips.Testing
{
	sealed class ZFilterStripControlForTest : ZFilterStripControl
	{
		public ZFilterStripControlForTest()
		{
			Table = new HtmlTable();
		}

		public HtmlTable HtmlTableExposed
		{
			get { return Table; }
		}

		public void OnPage_PreRender()
		{
			base.Page_PreRender(null, EventArgs.Empty);
		}

		public override bool FilterWasChanged
		{
			get
			{
				return fFilterWasChanged;
			}
		}
		public bool fFilterWasChanged;

		public bool AssertLayoutControlsVisibility(bool visibility)
		{
			return UnsavedFilterRow.Visible == visibility;
		}

		public void OnPage_Load(object sender, EventArgs e)
		{
			base.Page_Load(sender, e);
		}

		public void AddFilterStripExposed(ZGuid pk)
		{
			AddFilterStrip(pk);
		}

		public FilterStrip GetFilterStripDataSourceExposed(ZGuid pk)
		{
			return GetFilterStripDataSource(pk);
		}

		public new List<ZFilterStripRow> FilterStripRows
		{
			get
			{
				return base.FilterStripRows;
			}
		}

		public new ZFilterStripRowFooter Footer
		{
			get
			{
				return base.Footer;
			}
		}

		public new void AddFilterStrip(FilterStrip strip)
		{
			base.AddFilterStrip(strip);
		}
	}
}
