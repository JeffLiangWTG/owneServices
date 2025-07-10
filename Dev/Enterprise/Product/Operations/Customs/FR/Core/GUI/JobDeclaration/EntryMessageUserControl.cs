using Enterprise.Customs.GUI;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.FR.GUI
{
	[CodeAlive("Template")]
	public partial class EntryMessageUserControl : EU.GUI.EntryMessageUserControl
	{
		public EntryMessageUserControl()
		{
		}

		protected override BaseCustomsEntryUserControl GetMessageUserControl() => new MessageUserControl();
	}
}
