using CargoWise.Windows.UI;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class Question5To7UserControl : ZUserControl
	{
		public Question5To7UserControl()
		{
			InitializeComponent();

			SetQuestions();
		}

		void SetQuestions()
		{
			Question5ALabel.CaptionResourceString = Constants.ValuationDeclarationQuestions.Q5A;
			Question5BLabel.CaptionResourceString = Constants.ValuationDeclarationQuestions.Q5B;
			Question5ALongLabel.CaptionResourceString = Constants.ValuationDeclarationQuestions.Q5ALong;
			Question5BLongLabel.CaptionResourceString = Constants.ValuationDeclarationQuestions.Q5BLong;
			Question5CLabel.CaptionResourceString = Constants.ValuationDeclarationQuestions.Q5C;
			Question5DLabel.CaptionResourceString = Constants.ValuationDeclarationQuestions.Q5D;
			Question5ELabel.CaptionResourceString = Constants.ValuationDeclarationQuestions.Q5E;
			Question5ETextLabel.CaptionResourceString = Constants.ValuationDeclarationQuestions.Q5Eb;
			Question6ALabel.CaptionResourceString = Constants.ValuationDeclarationQuestions.Q6A;
			Question6BLabel.CaptionResourceString = Constants.ValuationDeclarationQuestions.Q6B;
			Question7ALabel.CaptionResourceString = Constants.ValuationDeclarationQuestions.Q7A;
			Question7BLabel.CaptionResourceString = Constants.ValuationDeclarationQuestions.Q7B;
		}

		public void BindTo934()
		{
			BindingSource.DataSourceType = typeof(JobDeclaration);
			BindingSource.SetBindingMember(Question5ADropEdit,  nameof(JobComInvoiceHeader.ValuationQuestion7A_IMP));
			BindingSource.SetBindingMember(Question5BDropEdit,  nameof(JobComInvoiceHeader.ValuationQuestion7B_IMP));
			BindingSource.SetBindingMember(Question5CDropEdit,  nameof(JobComInvoiceHeader.ValuationQuestion7C));
			BindingSource.SetBindingMember(Question5DDropEdit,  nameof(JobComInvoiceHeader.ValuationQuestion7D));
			BindingSource.SetBindingMember(Question5EDropEdit,  nameof(JobComInvoiceHeader.ValuationQuestion7EA));
			BindingSource.SetBindingMember(Question5ETextBox,  nameof(JobComInvoiceHeader.ValuationQuestion7EB));

			BindingSource.SetBindingMember(Question6ADropEdit,  nameof(JobComInvoiceHeader.ValuationQuestion8A));
			BindingSource.SetBindingMember(Question6BDropEdit,  nameof(JobComInvoiceHeader.ValuationQuestion8B));

			BindingSource.SetBindingMember(Question7ADropEdit,  nameof(JobComInvoiceHeader.ValuationQuestion9A));
			BindingSource.SetBindingMember(Question7BDropEdit,  nameof(JobComInvoiceHeader.ValuationQuestion9B));
		}

		public void BindToMessageSendingObject()
		{
			BindingSource.DataSourceType = typeof(ValuationDeclarationMessageSendingObjectParent);
			Controls.ChangeBindingPaths(BindingSource, ControlExtensionMethods.BindingPathForMessageSending);
			Question5ADropEdit.SetReadOnly(true);
			Question5BDropEdit.SetReadOnly(true);
			Question5CDropEdit.SetReadOnly(true);
			Question5DDropEdit.SetReadOnly(true);
			Question5EDropEdit.SetReadOnly(true);
			Question5ETextBox.SetReadOnly(true);
			Question6ADropEdit.SetReadOnly(true);
			Question6BDropEdit.SetReadOnly(true);
			Question7ADropEdit.SetReadOnly(true);
			Question7BDropEdit.SetReadOnly(true);
		}
	}
}
