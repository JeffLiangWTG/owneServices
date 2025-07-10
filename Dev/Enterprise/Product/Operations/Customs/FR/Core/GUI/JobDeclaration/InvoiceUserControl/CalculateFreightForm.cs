using System;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.FR.GUI
{
	public partial class CalculateFreightForm : EU.GUI.CalculateFreightForm
	{
		public CalculateFreightForm(CalculateFreightBizObj bizObj)
			: base(bizObj)
		{
			amountAfterEUBorderCalcEdit.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("FFA8EBDB-E03C-4833-9E26-768391FEDA5C", "Amount in EU Border");
			var form = amountConvertToLocalCurrencyControl.FindForm();
			form.Load += new EventHandler(SetDefaultFocus);
		}
		void SetDefaultFocus(object sender, EventArgs e)
		{
			amountConvertToLocalCurrencyControl.Controls[0].Select();
		}
	}
}
