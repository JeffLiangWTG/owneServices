using System.Windows.Forms;
using Enterprise.Accounting.Business.GLAccountFormat;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.GLAccountFormat
{
	public partial class FormatChangeForm : ZChildForm
	{
		ZArchitecture.ZTextBox zTextBox1;
		ZArchitecture.ZTextBox zTextBox2;
		ZButton ContinueButton;
		ZButton CancelBoundButton;

		public FormatChangeForm(GLAccountFormatter formatter)
			: base(formatter)
		{
			this.Formatter = formatter;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#region Implementation

		protected GLAccountFormatter Formatter;

		#region System stuff

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		void ContinueButton_Click(object sender, System.EventArgs e)
		{
			Formatter.ValidateAll();
			if (Formatter.HasErrors)
			{
				ShowErrorsDialog();
			}
			else
			{
				if (Formatter.CurrentFormat == "")
				{
					Formatter.Update(Formatter.NewFormat);
				}
				else
				{
					using (GLFormatMatchingForm form = new GLFormatMatchingForm(Formatter.CurrentFormat, Formatter.NewFormat))
					{
						DialogResult result = ZFormModaliser.ShowDialogWithoutDispose(form);
						if (result == DialogResult.OK)
						{
							Formatter.Update(form.UpdatedMask);
						}
					}
				}
				Close();
			}
		}

		void CancelBoundButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		#endregion
	}
}

