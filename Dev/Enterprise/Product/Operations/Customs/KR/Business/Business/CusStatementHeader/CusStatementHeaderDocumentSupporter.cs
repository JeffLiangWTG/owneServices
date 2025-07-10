using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class CusStatementHeaderDocumentSupporter : DocumentSupporter
	{
		public CusStatementHeaderDocumentSupporter(CusStatementHeader cusStatement) : base(cusStatement)
		{
		}

		public override BusinessContext BusinessContext => BusinessContext.Statement;

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.CustomsDeclarationCustomiseDocument;

		protected CusStatementHeader CusStatementHeader => (CusStatementHeader)BusinessObject;

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, BusinessObject);
			if (result != null)
			{
				return result;
			}
			return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.Statement, BusinessObject) };
		}

		public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		protected override DataContext[] GetSupportedDataContexts() => new[] { DataContext.Statement };
	}
}
