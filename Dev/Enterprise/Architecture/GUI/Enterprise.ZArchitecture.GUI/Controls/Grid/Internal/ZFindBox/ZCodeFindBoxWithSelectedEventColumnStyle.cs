using System;
using System.ComponentModel;
using Enterprise.ZArchitecture.GUI.Grid.Internal;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZCodeFindBoxWithSelectedEventColumnStyle : ZCodeFindBoxColumnStyle
	{
		public ZCodeFindBoxWithSelectedEventColumnStyle(ZCodeFindBoxWithSelectedEventColumnStyleInfo columnInfo)
			: this(() => new ZGridFindBoxWithSelectedEvent(), columnInfo)
		{
		}

		protected ZCodeFindBoxWithSelectedEventColumnStyle(Func<ZGridFindBoxWithSelectedEvent> gridFindBox1, ZCodeFindBoxWithSelectedEventColumnStyleInfo columnInfo) : base(gridFindBox1, columnInfo)
		{
			((ZGridFindBoxWithSelectedEvent)EditControl).Selected += columnInfo.OnSelected;
		}
	}

	public class ZCodeFindBoxWithSelectedEventColumnStyleInfo : ZCodeFindBoxColumnStyleInfo, IMultiTypeColumnStyleInfo
	{
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get { return typeof(ZCodeFindBoxWithSelectedEventColumnStyle); }
		}

		public event EmbeddedModulePopup.SelectedEventHandler Selected;

		public void OnSelected(object sender, EmbeddedModulePopup.SelectedEventArgs e)
		{
			Selected?.Invoke(sender, e);
		}
	}
}
