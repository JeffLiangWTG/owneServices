using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.DocumentDelivery;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsTadEdocSaver
	{
		public NctsTadEdocSaver(NctsHeader nctsHeader, NctsTadEdocSaverOptions options = null)
		{
			this.nctsHeader = nctsHeader;
			this.options = options ?? new NctsTadEdocSaverOptions();
		}

		public void RenderTadAndStoreInEdocs(IDocumentSupportable documentSupportable)
		{
			using (DisposableEnvironment.ForBranch(nctsHeader.BH_GB.ToGuid()))
			{
				if (documentSupportable.DocumentSupporter != null)
				{
					var docCommand = GetDocumentCommandThatWeWillFireAsIfUserClickedIt();
					var silentPrinter = new SilentDocumentPrinter(nctsHeader.Factory, documentSupportable, docCommand);
					silentPrinter.Print(ZGuid.Empty, 0, true, true, nctsHeader.DocManagerInfo, language: options.Language);
				}
			}
		}

		DocumentCommand GetDocumentCommandThatWeWillFireAsIfUserClickedIt()
		{
			var filter = new DocumentZQuery();
			filter.AddToFilter(StmMenuItemSchema.PK, new ZGuid("038cfc21-1521-44d9-b994-199c8e2584d1")); // PK of the "TAD/TSAD" menu option
			return nctsHeader.Factory.LoadTop1<DocumentCommand>(filter);
		}

		readonly NctsHeader nctsHeader;
		readonly NctsTadEdocSaverOptions options;
	}
}
