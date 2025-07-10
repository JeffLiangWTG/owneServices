using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsHeaderDocumentSupporter : EU.NCTS.Business.NctsHeaderDocumentSupporter
	{
		public NctsHeaderDocumentSupporter(EU.NCTS.Business.NctsHeader parent) : base(parent)
		{
		}

		NctsHeader NctsHeader => (NctsHeader)BusinessObject;

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun) => (dataContext is DataContext.EuNcts) ? [DocumentWrapperHelper.GetFRSpecificDocumentWrapper(dataContext, NctsHeader)] : base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
	}
}
