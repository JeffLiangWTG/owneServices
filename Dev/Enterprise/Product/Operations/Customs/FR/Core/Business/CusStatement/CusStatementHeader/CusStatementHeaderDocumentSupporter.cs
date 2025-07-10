using System;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.FR.Business.CusStatement
{
	public class CusStatementHeaderDocumentSupporter : DocumentSupporter
	{
		public CusStatementHeaderDocumentSupporter(CusStatementHeader statementHeader)
			: base(statementHeader)
		{
		}

		public override BusinessContext BusinessContext => BusinessContext.Statement;

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.CustomsDeclarationCustomiseDocument;

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
			=> (dataContext is DataContext.Statement) ? [DocumentWrapperHelper.GetFRSpecificDocumentWrapper(dataContext, BusinessObject)] : Array.Empty<DocumentWrapper>();

		protected override DataContext[] GetSupportedDataContexts() => [DataContext.Statement];

		public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}
	}
}
