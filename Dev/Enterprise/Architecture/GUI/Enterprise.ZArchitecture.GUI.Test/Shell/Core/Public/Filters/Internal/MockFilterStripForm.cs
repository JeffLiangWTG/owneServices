using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class MockFilterStripForm : ZForm
	{
		readonly ZFilterStripControlForTest filterStripControl;

		public MockFilterStripForm(MockFilterStripBizO businessEntity)
			: base(businessEntity)
		{
			filterStripControl = new ZFilterStripControlForTest(DataSource.Dummies, DataSource);
			var columnStyleInfo = new ZTextBoxColumnStyleInfo();
			columnStyleInfo.ColumnName = DummyBizoSchema.Constants.Z0_VarCharMax;
			filterStripControl.FilteredGrid.BindTo = "Dummies";
			filterStripControl.FilteredGrid.ColumnStyles.Add(columnStyleInfo);
			Controls.Add(filterStripControl);
		}

		public new MockFilterStripBizO DataSource
		{
			get { return (MockFilterStripBizO)base.DataSource; }
		}

		public ZFilterStripControlForTest FilterStripControl
		{
			get { return filterStripControl; }
		}

		public ZFilterStrip AddFilterStrip(string filterDescription)
		{
			AddFilterStripCore(filterDescription, null);
			var panel = FilterStripControl.Controls.Find("FilterStripsPanel", false)[0];
			return (ZFilterStrip)panel.Controls[panel.Controls.Count - 1];
		}

		public ZFilterStrip AddFilterStrip(string filterDescription, GroupStripControl group)
		{
			AddFilterStripCore(filterDescription, group);
			return (ZFilterStrip)group.Controls[3].Controls[group.Controls[3].Controls.Count - 1];
		}

		void AddFilterStripCore(string filterDescription, GroupStripControl group)
		{
			var filterStripBizO = DataSource.FilterStrips.AddNew();
			filterStripBizO.FilterDescription = filterDescription;
			if (group != null)
			{
				filterStripBizO.GroupName = group.GroupName;
			}
			FilterStripControl.AddFilterStrip(filterStripBizO);
		}

		public GroupStripControl AddGroupStripControl(string groupName)
		{
			FilterStripControl.AddGroupFilterControl(groupName);
			return GetGroupStripControl(groupName);
		}

		public GroupStripControl GetGroupStripControl(string groupName)
		{
			return GroupStripControls.FirstOrDefault(groupStripControl => groupStripControl.GroupName == groupName);
		}

		IEnumerable<GroupStripControl> GroupStripControls
		{
			get { return FilterStripsPanel.Controls.OfType<GroupStripControl>(); }
		}

		Control FilterStripsPanel
		{
			get { return FilterStripControl.Controls.Find("FilterStripsPanel", false)[0]; }
		}

		public ZDropEdit GetComparisonList(ZFilterStrip strip)
		{
			return ControlTestHelper.FindControls<ZDropEdit>(strip).SingleOrDefault(x => x.Name == "OperatorDropEdit");
		}

		public ZCodeFindBox GetFindBox(ZFilterStrip strip)
		{
			return ControlTestHelper.FindControls<ZCodeFindBox>(strip).SingleOrDefault(x => x.Name == "PropertyFindBox");
		}

		public ZFilterCollectionFindBox GetFilterCollectionFindBox(ZFilterStrip strip)
		{
			return ControlTestHelper.FindControls<ZFilterCollectionFindBox>(strip).SingleOrDefault(x => x.Name == "FilterCollectionFindBox");
		}

		public ZDropEdit GetModuleList(ZFilterStrip strip)
		{
			return ControlTestHelper.FindControls<ZDropEdit>(strip).SingleOrDefault(x => x.Name == "ModuleDropEdit");
		}
	}
}
