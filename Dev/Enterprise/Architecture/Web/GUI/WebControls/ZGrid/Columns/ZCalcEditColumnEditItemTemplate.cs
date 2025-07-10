using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Template for editing controls in the grid
	/// </summary>
	public class ZCalcEditColumnEditItemTemplate : ZCalcEditColumnItemTemplate, IEditItemTemplate
	{
		public ZCalcEditColumnEditItemTemplate(ZCalcEditColumn column) : base(column)
		{
		}

		new ZCalcEditColumn Column
		{
			get { return base.Column as ZCalcEditColumn; }
		}

		protected internal override ISelfBindingWebControl GetControl()
		{
			ZNumericTextBox textBox = new ZNumericTextBox();
			textBox.BindToDecimals = Column.BindToDecimals;
			textBox.Decimals = Column.Decimals;
			textBox.ValidateBindToDecimals = Column.ValidateBindToDecimals;
			textBox.AutoPostBack = Column.AutoPostBack;
			textBox.ID = Column.ID;
			if (Column != null)
			{
				textBox.TextTransform = Column.TextTransform;
			}
			if (textBox.AutoPostBack)
			{
				textBox.PostDataChanged += new EventHandler(Column.ZOwner.OnPostDataChanged);
			}
			return textBox;
		}

		protected override void SetupAutoSizeColumnControl(ISelfBindingWebControl innerControl)
		{
			base.SetupAutoSizeColumnControl(innerControl);
			var textBox = innerControl as ZNumericTextBox;
			textBox.Width = Unit.Pixel(75);
			Column.ItemStyle.Width = Unit.Pixel(80);
		}

		protected override void InstantiateInCore(Control container)
		{
			base.InstantiateInCore(container);
			((TableCell)container).HorizontalAlign = HorizontalAlign.Right;
		}
	}
}
