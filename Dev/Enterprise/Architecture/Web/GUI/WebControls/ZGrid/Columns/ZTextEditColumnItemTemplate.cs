using System.Web.UI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Item style for Text Column
	/// </summary>
	public class ZTextEditColumnItemTemplate : ZItemTemplate
	{
		public ZTextEditColumnItemTemplate(ZTemplateColumn column) : base(column)
		{
		}

		protected internal override ISelfBindingWebControl GetControl()
		{
			ZTextLabel label = new ZTextLabel();

			ZTextEditColumn textEditColumn = Column;
			if (textEditColumn != null)
			{
				label.EnableHtmlEncoding = textEditColumn.EnableHtmlEncoding;
				textEditColumn.TextTransform = textEditColumn.TextTransform;
			}

			return label;
		}

		protected override void SetupAutoSizeColumnControl(ISelfBindingWebControl innerControl)
		{
			base.SetupAutoSizeColumnControl(innerControl);
			Column.ItemStyle.Width = Unit.Empty;
		}

		new protected ZTextEditColumn Column
		{
			get { return base.Column as ZTextEditColumn; }
		}
	}
}
