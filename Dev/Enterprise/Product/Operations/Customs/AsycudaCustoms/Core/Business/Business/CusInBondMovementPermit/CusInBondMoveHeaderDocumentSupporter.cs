using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CusInBondMoveHeaderDocumentSupporter : DocumentSupporter
	{
		public CusInBondMoveHeaderDocumentSupporter(CusInBondMoveHeader header)
			: base(header)
		{
		}

		public override CargoWise.Definitions.BusinessContext BusinessContext => CargoWise.Definitions.BusinessContext.CusInBondHeader;

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.None;

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return DocumentWrapperFactory.GenerateGenericWrappers(dataContext, BusinessObject);
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			return new DataContext[] { DataContext.CusInBondHeader };
		}

		public override bool StorageDocsAreEditableIfInRelated => true;
	}
}
