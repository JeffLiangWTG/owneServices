using System.Web.UI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Summary description for ZTextEditColumnEditItemStyle.
	/// </summary>
	public class ZTextEditColumnEditItemTemplate : ZTextEditColumnItemTemplate, IEditItemTemplate
	{
		public ZTextEditColumnEditItemTemplate(ZTemplateColumn column) : base(column)
		{
		}

		protected internal override ISelfBindingWebControl GetControl()
		{
			var control = new ZTextBox();

			if (Column != null)
			{
				control.CanBeEnabledByClient = Column.CanBeEnabledByClient;
				control.TextTransform = Column.TextTransform;
				control.AutoPostBack = Column.AutoPostBack;
				control.ID = Column.ID;
			}

			return control;
		}

		protected override void SetupAutoSizeColumnControl(ISelfBindingWebControl innerControl)
		{
			base.SetupAutoSizeColumnControl(innerControl);
			var textBox = innerControl as ZTextBox;
			textBox.Width = Unit.Percentage(97);
		}
	}
}
