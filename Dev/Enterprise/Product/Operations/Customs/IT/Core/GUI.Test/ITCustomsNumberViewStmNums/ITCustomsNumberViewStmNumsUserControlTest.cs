using System.Windows.Forms;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(ZChildForm))]
sealed class ITCustomsNumberViewStmNumsUserControlTest : ZFormBasherTest
{
	public void TestGridColumns()
	{
		using (var control = new ITCustomsNumberViewStmNumsUserControl(Provider.CustomsNumberWrappers))
		{
			control.Show();
			var numberRangesGrid = (ZGrid)control.Controls.Find("NumberRangesGrid", true)[0];
			AssertEquals(true, numberRangesGrid.GetColumnStyle(CustomsNumberViewStmNums.Schema.SN_FountainName).IsUnavailable);
			AssertEquals(false, numberRangesGrid.GetColumnStyle(ITCustomsNumberViewStmNumsWrapper.Schema.YearOfApplicability).IsUnavailable);
			AssertEquals(false, numberRangesGrid.GetColumnStyle(ITCustomsNumberViewStmNumsWrapper.Schema.AppliesTo).IsUnavailable);
		}
	}

	protected override Form GetFormToBashCore() => Form;

	ZChildForm Form
	{
		get
		{
			if (form == null)
			{
				form = new ZChildForm(Company);
				form.CaptionRenderingEnabled = true;
				form.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 712);
				var control = new ITCustomsNumberViewStmNumsUserControl(Provider.CustomsNumberWrappers);
				control.Name = "ITCustomsNumberViewStmNumsUserControl";
				control.Dock = DockStyle.Fill;
				form.Controls.Add(control);
				form.SetDataBinding(Company, "");
			}
			return form;
		}
	}
	ZChildForm form;

	ITCustomsNumberViewStmNumsCompanyProvider Provider => (ITCustomsNumberViewStmNumsCompanyProvider)Company.CustomsNumberProvider;

	GlbCompany Company => company ?? (company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
	GlbCompany company;
}
