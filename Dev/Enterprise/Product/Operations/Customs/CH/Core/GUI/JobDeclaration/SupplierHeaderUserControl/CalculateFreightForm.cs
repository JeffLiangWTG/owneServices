using System;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public partial class CalculateFreightForm : ZChildForm
{
	[Obsolete("Use the constructor that takes the bizObj, this constructor is just for the designer", true)]
	public CalculateFreightForm() { }

	public CalculateFreightForm(CalculateFreightBizObj bizObj)
		: base(bizObj)
	{
	}

	public new CalculateFreightBizObj BusinessEntity => (CalculateFreightBizObj)base.BusinessEntity;

	void cancelButton_Click(object sender, EventArgs e)
	{
		Close();
	}

	void OKButton_Click(object sender, EventArgs e)
	{
		if (BusinessEntity.AddCharges())
		{
			Close();
		}
		else
		{
			Globals.Message.ShowError(Res.GetString("E14A4CAC-F07D-4D17-9FE4-165A0B06F464", "Please fix all the errors before proceeding"), Res.GetString("E971EDE1-938D-441C-B577-872D680B3BBD", "Unable to Calculate"));
		}
	}
}
