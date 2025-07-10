using System;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	public partial class EMCSValidationSettingsForm : ZChildForm
	{
		public EMCSValidationSettingsForm(EMCSJobDeclaration declaration)
			: base(declaration)
		{
			InitializeComponent();
		}

		public override string FormHeading => Res.GetString("2ECB6429-05AC-4751-A4AA-42DEAEE99BA8", "Validation Settings");

		void ConfirmButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
