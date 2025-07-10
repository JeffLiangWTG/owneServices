using CargoWise.Definitions;

namespace Enterprise.DocumentEngine
{
	public class DocumentMenuCustomisationDocumentSupporter : MenuCustomisationDocumentSupporter
	{
		public DocumentMenuCustomisationDocumentSupporter(MenuCustomisation parentBO)
			: base(parentBO)
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.DocumentCustomiser; }
		}
	}
}
