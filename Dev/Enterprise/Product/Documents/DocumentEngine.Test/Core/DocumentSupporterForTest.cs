using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.Public.Testing
{
	public class DocumentSupporterForTest : DocumentSupporter
	{
		public DocumentSupporterForTest(BusinessObject businessObject)
			: base(businessObject)
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Organisation; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { throw new NotImplementedException(); }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			throw new NotImplementedException();
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			throw new NotImplementedException();
		}
	}
}
