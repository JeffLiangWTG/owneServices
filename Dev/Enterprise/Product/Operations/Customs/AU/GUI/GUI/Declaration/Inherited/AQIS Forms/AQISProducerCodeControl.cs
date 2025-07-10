using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public class AQISProducerCodeControl : AQISControl
	{
		protected override void AddInfoButton_Click(object sender, System.EventArgs e)
		{
			if (AQIS != null)
			{
				AQISProducerCodeForm editForm = new AQISProducerCodeForm(AQIS);
				Form parentForm = FindForm();
				editForm.Icon = parentForm.Icon;
				ZFormModaliser.Show(editForm, parentForm);
			}
		}
	}
}
