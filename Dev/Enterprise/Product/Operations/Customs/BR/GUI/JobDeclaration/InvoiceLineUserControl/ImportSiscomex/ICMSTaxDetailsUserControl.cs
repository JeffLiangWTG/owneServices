using System;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI
{
	public partial class ICMSTaxDetailsUserControl : ZUserControl
	{
		public ICMSTaxDetailsUserControl()
		{
			InitializeComponent();
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			var invoiceLine = CurrentDataItem as JobComInvoiceLine;

			if (invoiceLine != null)
			{
				invoiceLine.JI_ICMSBaseValueReductionPercentageInfo.ValueChanged -= JI_ICMSBaseValueReductionPercentageInfo_ValueChanged;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			var invoiceLine = CurrentDataItem as JobComInvoiceLine;

			if (invoiceLine != null)
			{
				invoiceLine.JI_ICMSBaseValueReductionPercentageInfo.ValueChanged += JI_ICMSBaseValueReductionPercentageInfo_ValueChanged;
				JI_ICMSBaseValueReductionPercentageInfo_ValueChanged(this, e);
			}
		}

		void JI_ICMSBaseValueReductionPercentageInfo_ValueChanged(object sender, EventArgs e)
		{
			var invoiceLine = CurrentDataItem as JobComInvoiceLine;
			ICMSFormulaExplanationButton.Enabled = !invoiceLine?.JI_ICMSFormula_ReadOnly ?? false;
		}

		void ICMSFormulaExplanationButton_Click(object sender, EventArgs e) => Globals.Message.Show(Res.GetString("3EF0DBCA-4B53-4A35-9324-3E059D4C46B4",
			@"Example:
Total amounts that make up the ICMS Base = BRL 12,561.41
ICMS Rate 18% and ICMS Base Reduction (%): 51.1111 %

BC ICMS Formula: BC- Reduction in BC ICMS
The amount above BRL 12,561.41 must be divided by (1-18%), that is, BRL 12,561.41 / 0.82,
which results in BRL 15,318.80, after which the base is reduced by 51.11111%, that is
BRL 15,318.80 - (15,318.80 x 51.11111%) = BRL 15,318.80 - BRL 7,829.61 = 7,489.19

BC ICMS Formula: BCR- Reduction of the rate that makes up BC ICMS and Reduction in BC ICMS
The amount above BRL 12,561.41 must be divided by (1-(0.18-(0.18*51.11111%))), that is, also
we reduced 51.11111% from the 18% Rate, to consider the applicable rate of 8.80 and formula
BRL 12,561.41/ 0.912, which results in BRL 13,773.47, after which the base is reduced by 51.11111%, that is
BRL 13,773.47 - (13,773.47 x 51.11111%) = BRL 13,773.47 - BRL 7,039.77 = 6,733.70"));
	}
}
