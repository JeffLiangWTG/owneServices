using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class ExportClassificationForm : BaseClassificationForm
	{
		public ExportClassificationForm(Classification classification) : base(classification)
		{
			this.classification = classification;
		}

		protected override BaseClassificationUserControl GetUserControl()
		{
			return new ExportClassificationUserControl();
		}

		public override string FormCaption
		{
			get { return "Export Classification Lookup"; }
		}

		protected override SecurityCheckpoint AuditSecurity
		{
			get { return Env.Security.ExportClassificationAudit; }
		}
	}
}
