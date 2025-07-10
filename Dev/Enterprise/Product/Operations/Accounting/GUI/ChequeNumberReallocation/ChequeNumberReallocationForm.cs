using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class ChequeNumberReallocationForm : ZChildForm
	{
		public ChequeNumberReallocationForm(ChequeNumberReallocator reallocator)
			: base(reallocator)
		{
			this.reallocator = reallocator;

			LabelCaptionRenderer captionRenderer = ChequeNumberTextBox.GetExtension<LabelCaptionRenderer>();
			captionRenderer.Caption = this.reallocator.Caption;
			captionRenderer.Options = StringRenderingOptions.Wrap;

			this.reallocator.HasChanges = false;
		}

		readonly ChequeNumberReallocator reallocator;
		bool preventClosing;

		bool ReallocatorHasErrors
		{
			get
			{
				reallocator.RunPreSaveValidation();
				return reallocator.HasErrors;
			}
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		void okButton_Click(object sender, System.EventArgs e)
		{
			preventClosing = ReallocatorHasErrors;
		}

		void cancelButton_Click(object sender, System.EventArgs e)
		{
			preventClosing = false;
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (preventClosing && ReallocatorHasErrors)
			{
				ShowErrorsDialog();
				e.Cancel = true;
			}
			else
			{
				base.OnClosing(e);
			}
		}
	}
}
