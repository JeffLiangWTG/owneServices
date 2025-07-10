using Enterprise.Customs.GUI;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.GUI
{
	public partial class EntryMessageUserControl : EU.GUI.EntryMessageUserControl
	{
		public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		protected override BaseCustomsEntryUserControl GetMessageUserControl() => new MessageUserControl(JobDeclaration);
	}
}
