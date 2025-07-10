using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class AuthorAndAuditorControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new AuthorAndAuditorUserControl();

		[ThreadStatic]
		static AuthorAndAuditorControlBag instance;

		public static AuthorAndAuditorControlBag Instance => instance ?? (instance = new AuthorAndAuditorControlBag());

		AuthorAndAuditorControlBag()
		{
			AuthorGuidDropEdit = RegisterControl(nameof(AuthorAndAuditorUserControl.AuthorGuidDropEdit));
			AuthorNameTextBox = RegisterControl(nameof(AuthorAndAuditorUserControl.AuthorNameTextBox));
			AuthorPhoneTextBox = RegisterControl(nameof(AuthorAndAuditorUserControl.AuthorPhoneTextBox));
			AuthorJobTitleTextBox = RegisterControl(nameof(AuthorAndAuditorUserControl.AuthorJobTitleTextBox));

			AuditorGuidDropEdit = RegisterControl(nameof(AuthorAndAuditorUserControl.AuditorGuidDropEdit));
			AuditorNameTextBox = RegisterControl(nameof(AuthorAndAuditorUserControl.AuditorNameTextBox));
			AuditorPhoneTextBox = RegisterControl(nameof(AuthorAndAuditorUserControl.AuditorPhoneTextBox));
			AuditorJobTitleTextBox = RegisterControl(nameof(AuthorAndAuditorUserControl.AuditorJobTitleTextBox));
		}

		public ControlReference AuthorGuidDropEdit { get; }
		public ControlReference AuthorNameTextBox { get; }
		public ControlReference AuthorPhoneTextBox { get; }
		public ControlReference AuthorJobTitleTextBox { get; }

		public ControlReference AuditorGuidDropEdit { get; }
		public ControlReference AuditorNameTextBox { get; }
		public ControlReference AuditorPhoneTextBox { get; }
		public ControlReference AuditorJobTitleTextBox { get; }
	}
}
