using Enterprise.DocumentVisualizer.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentVisualizer.GUI
{
	public partial class NotificationsView : ZChildForm
	{
		public NotificationsView(BindableNotifications notifications)
			: base(notifications)
		{
			InitializeComponent();

			closeButton.Click += (s, e) => Close();
		}

		public override string FormHeading
		{
			get { return Res.GetString("cb770a17-8ae4-4574-a6e5-3f26a289e1b6", "New {0}", FormCaption); }
		}
	}
}
