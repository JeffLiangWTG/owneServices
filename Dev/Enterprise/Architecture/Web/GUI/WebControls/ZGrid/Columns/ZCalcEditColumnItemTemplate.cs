using System.Web.UI;
using System.Web.UI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Item template for ZCalcEditColumn
	/// </summary>
	public class ZCalcEditColumnItemTemplate : ZItemTemplate
	{
		public ZCalcEditColumnItemTemplate(ZCalcEditColumn column) : base(column)
		{
		}

		new ZCalcEditColumn Column
		{
			get { return base.Column as ZCalcEditColumn; }
		}

		protected internal override ISelfBindingWebControl GetControl()
		{
			ZNumericLabel label;

			if (!string.IsNullOrEmpty(Column.BindToCurrencySymbol))
			{
				label = new ZMoneyLabel();
				((ZMoneyLabel)label).BindToCurrencySymbol = Column.BindToCurrencySymbol;
			}
			else
			{
				label = new ZNumericLabel();
			}
			label.ShowNotifications = true;
			label.BindToDecimals = Column.BindToDecimals;
			label.Decimals = Column.Decimals;
			label.ShowGroupSeparators = Column.ShowGroupSeparators;

			return label;
		}

		protected override void InstantiateInCore(Control container)
		{
			base.InstantiateInCore(container);
			if (container != null && container is TableCell)
			{
				((TableCell)container).HorizontalAlign = HorizontalAlign.Right;
			}
		}
	}
}
