using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public class AQISPermitNumberControl : AQISControl
	{
		protected override void AddInfoButton_Click(object sender, System.EventArgs e)
		{
			if (AQIS != null)
			{
				AQISPermitNumberForm editForm = new AQISPermitNumberForm(AQIS);
				Form parentForm = FindForm();
				editForm.Icon = parentForm.Icon;
				ZFormModaliser.Show(editForm, parentForm);
			}
		}
	}
}
