using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class ImportClassificationForm : BaseClassificationForm
	{
		public ImportClassificationForm(Classification classification) : base(classification)
		{
		}

		protected override BaseClassificationUserControl GetUserControl()
		{
			return new ImportClassificationUserControl();
		}

		public override string FormCaption
		{
			get { return "Import Classification Lookup"; }
		}

		protected override SecurityCheckpoint AuditSecurity
		{
			get { return Env.Security.ImportClassificationAudit; }
		}
	}
}
