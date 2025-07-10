using System;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class CSARevenueSummaryFormDetailsUserControl : ZUserControl
	{
		public CSARevenueSummaryFormDetailsUserControl()
		{
			InitializeComponent();
		}

		public new CusStatementHeader CurrentDataItem => base.CurrentDataItem as CusStatementHeader;

		void CalculateRSFButton_Clicked(object sender, EventArgs e)
		{
			var csaRevenueSummaryForm = CurrentDataItem;
			if (csaRevenueSummaryForm.HasChanges || !csaRevenueSummaryForm.IsInDatabase)
			{
				Globals.Message.Show(Res.GetString("04968D2B-555E-477A-AF4E-D024B7C32052", "CSA Revenue Summary Form has not been saved, please save it before perform calculation."));
			}
			else
			{
				var result = new CSARSFCalculator(csaRevenueSummaryForm).CalculateRSF();
				if (result)
				{
					Globals.Message.Show(Res.GetString("5B7E0E0C-D8D2-4ADE-AFB0-4D95DB1B6D50", "Calculate Successfully!"));
				}
				else
				{
					Globals.Message.Show(Res.GetString("9F430434-50D8-4C21-AB7B-DFEC2DACB2EF", "Calculate Failed!"));
				}
			}
		}
	}
}
