using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Registry.GUI
{
	[CodeAlive("Will be used in WI00626601")]
	public partial class NewsAnnouncementSectionTypeControl : RegistryZUserControl
	{
		public NewsAnnouncementSectionTypeControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			Grid.ReadOnly = readOnly;
		}
	}
}
