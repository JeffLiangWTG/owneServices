using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class Question8To11GroupBoxUserControl : ZUserControl
	{
		public Question8To11GroupBoxUserControl()
		{
			InitializeComponent();

			Question8Panel.UpdateLayout(new Question8Layout());
			Question9Panel.UpdateLayout(new Question9Layout());
			Question10Panel.UpdateLayout(new Question10Layout());
			Question11Panel.UpdateLayout(new Question11Layout());
		}
	}
}
