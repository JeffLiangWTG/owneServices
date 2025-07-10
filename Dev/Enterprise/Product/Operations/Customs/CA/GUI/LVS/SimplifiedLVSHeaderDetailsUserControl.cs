using System;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class SimplifiedLVSHeaderDetailsUserControl : ZUserControl
	{
		public SimplifiedLVSHeaderDetailsUserControl()
		{
			InitializeComponent();
			this.ImporterOrganisationControl.OrganisationFindBox.ReadOnly = true;
		}

		#region INCOTERMS Explanation

		protected void IncoTermExplainButton_Click(object sender, EventArgs e)
		{
			var form = new IncoTermDescriptionForm(JZ_IncoTermBoundDropDownEdit.Text);
			try
			{
				ZFormModaliser.Show(form, ParentForm as ZForm);
			}
			finally
			{
				form.Dispose();
			}
		}

		#endregion
	}
}
