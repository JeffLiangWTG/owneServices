using System;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class CMRAddInfoForm : ZChildForm
	{
		public CMRAddInfoForm(AUAddInfo addInfoBusinessObject) : base(addInfoBusinessObject)
		{
#if WINZOR
			this.AddInfoBoundTextBox.Multiline = true;
			this.AddInfoBoundTextBox.IsDynamicMultiline = true;
#endif
		}

		public static void ShowForm(Form parentForm, AUAddInfo currentAddInfo, AddInfoSavedEventHandler addInfoSavedEvent)
		{
			var anAddInfo = currentAddInfo.Clone();
			anAddInfo.Validation.ValidateAll();

			var editAddInfoForm = new CMRAddInfoForm(anAddInfo);
			editAddInfoForm.AddInfoSaved += addInfoSavedEvent;
			editAddInfoForm.Icon = parentForm.Icon;
			ZFormModaliser.Show(editAddInfoForm, parentForm);
		}

		public event AddInfoSavedEventHandler AddInfoSaved;

		void CancelBoundButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void OKBoundButton_Click(object sender, EventArgs e)
		{
			AddInfoSaved?.Invoke(this, new AddInfoEventArgs((AUAddInfo)BusinessEntity));
			Close();
		}
	}
}
