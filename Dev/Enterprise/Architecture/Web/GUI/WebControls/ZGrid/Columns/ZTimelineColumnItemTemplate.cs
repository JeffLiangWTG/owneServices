using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Summary description for ZTimelineColumnItemTemplate.
	/// </summary>
	public class ZTimelineColumnItemTemplate : ZItemTemplate
	{
		public ZTimelineColumnItemTemplate(ZTimelineColumn column) : base(column)
		{
		}

		new ZTimelineColumn Column
		{
			get { return base.Column as ZTimelineColumn; }
		}

		protected internal override ISelfBindingWebControl GetControl()
		{
			ZTimelinePanel panel = new ZTimelinePanel();
			panel.BindTo = Column.BindTo;
			panel.BindToEstimated = Column.BindToEstimated;
			panel.DateTimeFormat = Column.DateTimeFormat;
			panel.Width = new System.Web.UI.WebControls.Unit("100%");
			panel.Height = new System.Web.UI.WebControls.Unit("100%");
			return panel;
		}

		protected override void ChangeCell(System.Web.UI.WebControls.TableCell cell)
		{
			base.ChangeCell(cell);
			ZTimelinePanel panel = cell.Controls[0] as ZTimelinePanel;
			if (panel != null)
			{
				cell.CssClass = ZCssHelper.Join(cell.CssClass, panel.CssClass);
			}
		}
	}
}
