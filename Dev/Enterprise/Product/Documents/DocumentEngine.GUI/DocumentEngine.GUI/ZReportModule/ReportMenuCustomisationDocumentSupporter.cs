using CargoWise.Definitions;

namespace Enterprise.DocumentEngine.GUI
{
	public class ReportMenuCustomisationDocumentSupporter : MenuCustomisationDocumentSupporter
	{
		public ReportMenuCustomisationDocumentSupporter(MenuCustomisation parentBO)
			: base(parentBO)
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.DocReportCustomiser; }
		}
	}
}
