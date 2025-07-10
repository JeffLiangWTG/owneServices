using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.DocumentDelivery;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business
{
	public abstract class BaseEDocsSaver<T>
		where T : class, IDocumentSupportable, IFactoryProvider
	{
		public BaseEDocsSaver(T target)
		{
			this.target = Argument.NotNull(target, "Target BizO must not be null");
		}

		protected readonly T target;

		public void RenderDocumentAndSaveInEDocs()
		{
			using (DisposableEnvironment.ForBranch(BranchPK.ToGuid()))
			{
				var docCommand = GetDocumentCommand();
				if (docCommand != null)
				{
					SilentDocumentPrinter silentPrinter = new SilentDocumentPrinter(Factory, target, docCommand);
					silentPrinter.Print(ZGuid.Empty, 0, true, name: DocName, title: DocTitle);
				}
			}
		}

		DocumentCommand GetDocumentCommand()
		{
			DocumentCommand result = null;
			DocumentSupporter documentSupporter = target.DocumentSupporter;
			if (documentSupporter != null)
			{
				var filter = new DocumentZQuery(true);
				filter.AddToFilter(StmMenuItemSchema.PK, DocumentMenuItemPK);
				result = Factory.LoadTop1<DocumentCommand>(filter);
			}
			return result;
		}

		ZGuid BranchPK => BranchPKCore;

		protected abstract ZGuid BranchPKCore { get; }

		ZString DocTitle => DocTitleCore;

		protected abstract ZString DocTitleCore { get; }

		ZString DocName => DocNameCore;

		protected abstract ZString DocNameCore { get; }

		ZGuid DocumentMenuItemPK => DocumentMenuItemPKCore;

		protected abstract ZGuid DocumentMenuItemPKCore { get; }

		BusinessObjectFactory Factory => target.Factory;
	}
}
