using System;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AddInfoForm : ZChildForm
	{
		public AddInfoForm(AUAddInfo addInfoBusinessObject) : base(addInfoBusinessObject)
		{
		}

		public static void ShowForm(Form parentForm, AUAddInfo currentAddInfo, AddInfoSavedEventHandler addInfoSavedEvent)
		{
			var auAddInfo = currentAddInfo.Clone();
			auAddInfo.Validation.ValidateAll();

			AddInfoForm editAddInfoForm = new AddInfoForm(auAddInfo);
			editAddInfoForm.AddInfoSaved += addInfoSavedEvent;
			editAddInfoForm.Icon = parentForm.Icon;
			ZFormModaliser.Show(editAddInfoForm, parentForm);
		}

		public event AddInfoSavedEventHandler AddInfoSaved;

		#region Implemenatation

		protected AUAddInfo fAddInfo;

		private void CancelBoundButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion

		private void OKBoundButton_Click(object sender, EventArgs e)
		{
			if (AddInfoSaved != null)
			{
				AddInfoSaved(this, new AddInfoEventArgs((AUAddInfo)BusinessEntity));
			}
			Close();
		}
	}

	public delegate void AddInfoSavedEventHandler(object sender, AddInfoEventArgs e);
}
