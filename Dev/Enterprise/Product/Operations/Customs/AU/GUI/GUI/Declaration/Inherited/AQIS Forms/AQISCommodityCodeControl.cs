using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AQISCommodityCodeControl : AQISControl
	{
		protected override void AddInfoButton_Click(object sender, System.EventArgs e)
		{
			if (AQIS != null)
			{
				var editForm = new AQISCommodityCodeForm(AQIS);
				var parentForm = FindForm();
				editForm.Icon = parentForm.Icon;
				ZFormModaliser.Show(editForm, parentForm);
			}
		}
	}
}
