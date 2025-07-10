using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.NCTS.Business.DocumentWrappers;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsHeaderDocumentSupporter : EU.NCTS.Business.NctsHeaderDocumentSupporter
	{
		public NctsHeaderDocumentSupporter(NctsHeader parent) : base(parent)
		{
		}

		#region Implementation

		NctsHeader NctsHeader => (NctsHeader)BusinessObject;

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
			=> dataContext == DataContext.EuNcts ? CreateEuNctsDocumentWrappers() : base.GetDocumentWrappersInternal(dataContext, commandBeingRun);

		DocumentWrapper[] CreateEuNctsDocumentWrappers() => new[] { NctsHeaderDocumentWrapper.New(NctsHeader, NctsHeader.Factory) }.WhereNotNull().ToArray();

		#endregion
	}
}
