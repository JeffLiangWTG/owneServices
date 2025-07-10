using Enterprise.ZArchitecture.GUI;
#if DEBUG
using Enterprise.ZArchitecture.GUI.Testing;
#endif

namespace Enterprise.Customs.KR.GUI
{
	public partial class EntryInstructionLayoutUserControl : ZUserControl
	{
		public EntryInstructionLayoutUserControl()
		{
			InitializeComponent();
#if DEBUG
			MissingResourceStringChecker.ExcludeFromTest(this.UseTypeDropEdit);
#endif
		}
	}
}
