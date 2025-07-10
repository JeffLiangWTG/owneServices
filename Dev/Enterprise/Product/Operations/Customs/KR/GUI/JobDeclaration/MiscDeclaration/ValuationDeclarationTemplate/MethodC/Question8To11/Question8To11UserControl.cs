using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class Question8To11UserControl : ZUserControl
	{
		public Question8To11UserControl()
		{
			InitializeComponent();

			SetQuestion();
		}

		void SetQuestion()
		{
			Question8Label.CaptionResourceString = Constants.ValuationDeclarationQuestions.Q8;
			Question8ALabel.CaptionResourceString = Constants.ValuationDeclarationQuestions.Q8A;
			Question8BLabel.CaptionResourceString = Constants.ValuationDeclarationQuestions.Q8B;
			Question8CLabel.CaptionResourceString = Constants.ValuationDeclarationQuestions.Q8C;
			Question8DLabel.CaptionResourceString = Constants.ValuationDeclarationQuestions.Q8D;

			Question9Label.CaptionResourceString = Constants.ValuationDeclarationQuestions.Q9;
			Question9ALabel.CaptionResourceString = Constants.ValuationDeclarationQuestions.Q9A;
			Question9BLabel.CaptionResourceString = Constants.ValuationDeclarationQuestions.Q9B;

			Question10Label.CaptionResourceString = Constants.ValuationDeclarationQuestions.Q10;
			Question10ALabel.CaptionResourceString = Constants.ValuationDeclarationQuestions.Q10A;
			Question10BLabel.CaptionResourceString = Constants.ValuationDeclarationQuestions.Q10B;
			Question10CLabel.CaptionResourceString = Constants.ValuationDeclarationQuestions.Q10C;
			Question10DLabel.CaptionResourceString = Constants.ValuationDeclarationQuestions.Q10D;

			Question11Label.CaptionResourceString = Constants.ValuationDeclarationQuestions.Q11;
			Question11ALabel.CaptionResourceString = Constants.ValuationDeclarationQuestions.Q11A;
			Question11BLabel.CaptionResourceString = Constants.ValuationDeclarationQuestions.Q11B;
			Question11CLabel.CaptionResourceString = Constants.ValuationDeclarationQuestions.Q11C;
			Question11DLabel.CaptionResourceString = Constants.ValuationDeclarationQuestions.Q11D;
		}
	}
}
