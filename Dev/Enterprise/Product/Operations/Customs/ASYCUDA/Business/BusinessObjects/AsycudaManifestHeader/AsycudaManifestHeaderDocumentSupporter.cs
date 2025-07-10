using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaManifestHeaderDocumentSupporter : DocumentSupporter
	{
		public AsycudaManifestHeaderDocumentSupporter(AsycudaManifestHeader header)
			: base(header)
		{
		}

		#region DataContext Consts

		public const string AsycudaManifestHeader = "AsycudaManifestHeader";

		#endregion

		public new AsycudaManifestHeader BusinessObject => (AsycudaManifestHeader)base.BusinessObject;

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			var result = AsycudaManifestHeaderDocWrapper.New(BusinessObject, commandBeingRun?.SU_MenuName ?? ZString.Empty, commandBeingRun?.SU_FilterList ?? ZString.Empty);
			return result == null ? null : new DocumentWrapper[] { result };
		}

		protected override DataContext[] GetSupportedDataContexts() => new[] { DataContext.AsycudaManifestHeader };

		public override BusinessContext BusinessContext => BusinessContext.AsycudaManifest;

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.None;

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var message = base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
			switch (dataContextValue.DataContext)
			{
				case DataContext.AsycudaManifestHeader:
					if ((commandBeingRun?.SU_MenuName ?? ZString.Empty) == AsycudaManifestHeaderDocWrapper.ZAManifestWithBarcode)
					{
						message = Res.GetString("{50AFA177-5568-4AD5-8E52-5AEAB5CBE83B}", "Cannot produce this Document because there is no South African Manifest data.");
					}
					break;

				default:
					break;
			}
			return message;
		}
	}
}
