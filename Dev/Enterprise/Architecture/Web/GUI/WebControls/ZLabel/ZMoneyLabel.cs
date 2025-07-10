using System.ComponentModel;
using System.Web.UI;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Label to display money with the currency symbol prefix
	/// </summary>
	[DefaultProperty("Text"), ToolboxData("<{0}:ZMoneyLabel runat=server></{0}:ZMoneyLabel>")]
	class ZMoneyLabel : ZNumericLabel
	{
		#region BindToCurrencySymbol

		public string BindToCurrencySymbol
		{
			get { return fBindToCurrencySymbol; }
			set { fBindToCurrencySymbol = value; }
		}

		string fBindToCurrencySymbol;

		#endregion

		#region Overrides

		protected override void SetTextProperty(object dataSource)
		{
			base.SetTextProperty(dataSource);

			ZString currencySymbol = !string.IsNullOrEmpty(this.BindToCurrencySymbol) ? (ZString)ZPropertyAccessor.Get(dataSource, BindToCurrencySymbol) : ZString.Empty;
			if (!currencySymbol.IsEmpty)
			{
				if (!string.IsNullOrEmpty(Text))
				{
					Text = currencySymbol.Trim().ToString() + Text;
				}

				if (!string.IsNullOrEmpty(ToolTip))
				{
					ToolTip = currencySymbol.Trim().ToString() + ToolTip;
				}
			}
		}

		#endregion
	}
}
