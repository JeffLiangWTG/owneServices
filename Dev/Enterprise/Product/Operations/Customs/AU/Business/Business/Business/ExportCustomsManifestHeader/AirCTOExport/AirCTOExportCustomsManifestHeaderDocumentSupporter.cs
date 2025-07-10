using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AirCTOExportCustomsManifestHeaderDocumentSupporter : DocumentSupporter
	{
		public AirCTOExportCustomsManifestHeaderDocumentSupporter(AirCTOExportCustomsManifestHeader parent)
			: base(parent)
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.AirCTOExport; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			switch (dataContext)
			{
				case Core.Constants.DataContext.AirCTOExport:
					return new DocumentWrapper[]
					{
						DocumentWrapperFactory.CreateCustomsWrapper(dataContext, Parent, "AU")
					};

				default:
					return System.Array.Empty<DocumentWrapper>();
			}
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Core.Constants.DataContext[]
			{
				Core.Constants.DataContext.AirCTOExport
			};
		}

		#region Implementation

		AirCTOExportCustomsManifestHeader Parent
		{
			get { return (AirCTOExportCustomsManifestHeader)base.BusinessObject; }
		}

		#endregion
	}
}
