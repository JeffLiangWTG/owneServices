using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Summary description for ZTimelineColumnItemTemplate.
	/// </summary>
	public class ZDateTimeStatusColumnItemTemplate : ZItemTemplate
	{
		public ZDateTimeStatusColumnItemTemplate(ZDateTimeStatusColumn column)
			: base(column)
		{
		}

		new ZDateTimeStatusColumn Column
		{
			get { return base.Column as ZDateTimeStatusColumn; }
		}

		protected internal override ISelfBindingWebControl GetControl()
		{
			ZDateTimeStatusPanel panel = new ZDateTimeStatusPanel();
			panel.BindTo = Column.BindTo;
			panel.BindToStatus = Column.BindToStatus;
			panel.DateTimeFormat = Column.DateTimeFormat;
			panel.Width = new System.Web.UI.WebControls.Unit("100%");
			panel.Height = new System.Web.UI.WebControls.Unit("100%");
			return panel;
		}

		protected override void ChangeCell(System.Web.UI.WebControls.TableCell cell)
		{
			base.ChangeCell(cell);
			ZDateTimeStatusPanel panel = cell.Controls[0] as ZDateTimeStatusPanel;
			if (panel != null)
			{
				cell.CssClass = ZCssHelper.Join(cell.CssClass, panel.CssClass);
			}
		}
	}
}
