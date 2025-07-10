using System;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.AU.GUI
{
	public partial class AdditionalContingencyDataForm : ZChildForm
	{
		public AdditionalContingencyDataForm(AdditionalContingencyData contingencyData)
			: base(contingencyData)
		{
			this.contingencyData = contingencyData;
		}

		readonly AdditionalContingencyData contingencyData;

		#region Buttons

		void YesButtonX_Click(object sender, EventArgs e)
		{
			contingencyData.RunPreSaveValidation();
			if (contingencyData.HasErrors)
			{
				ShowErrorsDialog();
			}
			else
			{
				DialogResult = DialogResult.Yes;
			}
		}

		private void NoButtonX_Click(object sender, EventArgs e)
		{
			contingencyData.RunPreSaveValidation();
			if (contingencyData.HasErrors)
			{
				ShowErrorsDialog();
			}
			else
			{
				DialogResult = DialogResult.No;
			}
		}

		#endregion

		#region Visibility

		public void ControlVisibility()
		{
			originPremiseID.Visible = !contingencyData.OriginPremiseInfo.ReadOnly;
			originPremiseIDExpain.Visible = !contingencyData.OriginPremiseInfo.ReadOnly;
			originPremiseIDLabel.Visible = !contingencyData.OriginPremiseInfo.ReadOnly;
		}

		#endregion
	}
}
