using System;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class CalculateFreightForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes the bizObj, this constructor is just for the designer", true)]
		public CalculateFreightForm() { }

		public CalculateFreightForm(CalculateFreightBizObj bizObj)
			: base(bizObj)
		{
			amountToEUBorderCalcEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("{F2B6954A-EF43-411A-A3C5-B4FCFD873059}", "Amount to EU Border");
			amountAfterEUBorderCalcEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("{4B27705E-03D7-4742-A93C-A87005F84D66}", "Amount after EU Border");
		}

		public new CalculateFreightBizObj BusinessEntity => (CalculateFreightBizObj)base.BusinessEntity;

		public override string FormVerb => string.Empty;

		void cancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			if (BusinessEntity.Calculate())
			{
				Close();
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("{8711B7B3-6036-4391-81E8-14AB985264BE}", "Please fix all the errors before proceeding"), Res.GetString("{02C8D2AD-70F6-473E-B008-67899E00A0A5}", "Unable to Calculate"));
			}
		}
	}
}
