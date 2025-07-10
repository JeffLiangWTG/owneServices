using CargoWise.ComponentModel.Design;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI
{
	[SuppressResourceStringContainerControlNameKeyPrefix]
	public class RelatedJobsTabPage : ZTabPage
	{
		#region CompileTimeBindingMemberCheck

		public CompileTimeCheckBindingMemberCollection CompileTimeBindingMemberCheck
		{
			get
			{
				CompileTimeCheckBindingMemberCollection result = new CompileTimeCheckBindingMemberCollection();
				result.Add(new CompileTimeCheckBindingMember(KBindingSource.GetBindingSource(this).DataSourceType, typeof(RelatedJobCollection), RelatedJobsUserControl.GetBindingMember()));
				return result;
			}
			set { }
		}

		#endregion

		public RelatedJobsTabPage()
		{
			InitialiseComponent();
		}

		void InitialiseComponent()
		{
			Text = Enterprise.ZArchitecture.GUI.UserControls.Res.GetString("18236ed1-378e-40b0-9a6b-fde69723abed", "Related Jobs");

			if (RelatedJobsUserControl == null)
			{
				RelatedJobsUserControl = new RelatedJobsUserControl();

				this.SuspendLayout();
				this.Controls.Add(RelatedJobsUserControl);
				this.ResumeLayout(true);
			}
		}

		public override bool ExcludeFromBindingOnSave
		{
			get { return true; }
		}

		internal RelatedJobsUserControl RelatedJobsUserControl;
	}
}
