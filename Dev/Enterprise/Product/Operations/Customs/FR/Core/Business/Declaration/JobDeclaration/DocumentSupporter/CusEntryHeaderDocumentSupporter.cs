using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class CusEntryHeaderDocumentSupporter : EU.Business.Declaration.CusEntryHeaderDocumentSupporter
	{
		public CusEntryHeaderDocumentSupporter(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		new CusEntryHeader EntryHeader => (CusEntryHeader)base.EntryHeader;

		internal CusEntryHeader WrapperDataSource => wrapperDataSource ??= GetRevertedEntryHeaderIfSnapshotExisting(EntryHeader.PK) ?? EntryHeader;
		CusEntryHeader wrapperDataSource;

		internal static CusEntryHeader GetRevertedEntryHeaderIfSnapshotExisting(ZGuid entryHeaderPk)
		{
			var universalFactory = new UniversalDataBuss.DataObjects.Core.UniversalObjectFactory(new ReadOnlyBusinessObjectFactory());
			var entryHeaderFromAnotherFactory = universalFactory.Load<CusEntryHeader>(entryHeaderPk);
			if (entryHeaderFromAnotherFactory?.GetNewAmendmentSnapshotManager() is AmendmentSnapshotManager manager && manager.HasLodgedSnapshot)
			{
				manager.RestoreEntryHeaderInMemoryOnly(SnapshotRevertingStrategy.Override, universalFactory);
				universalFactory.FireCleanupAfterSaving();//This clean up is invoked to unlock the mutex of entry merging, otherwise an exception would be thrown because the Save() is never invoked for this factory
				return entryHeaderFromAnotherFactory;
			}
			else
			{
				return null;
			}
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun) => (dataContext is
			DataContext.FRSADH or
			DataContext.SADH or
			DataContext.LiquidationDetails or
			DataContext.T2L or
			DataContext.T2LF or
			DataContext.IDD
			) ? [DocumentWrapperHelper.GetFRSpecificDocumentWrapper(dataContext, WrapperDataSource)] : base.GetDocumentWrappersInternal(dataContext, commandBeingRun);

		protected override DataContext[] GetSupportedDataContexts()
		{
			var result = new List<DataContext>(base.GetSupportedDataContexts())
			{
				DataContext.LiquidationDetails,
				DataContext.T2L,
				DataContext.T2LF,
				DataContext.FRSADH,
				DataContext.IDD
			};
			return result.ToArray();
		}

		public override string GetFilterValue(DocumentFilters filterName)
		{
			switch (filterName)
			{
				case DocumentFilters.MSGBKRCTYMOD:
					return WrapperDataSource.Declaration.MessageTypeForDocumentFilter + Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(WrapperDataSource.CountryCode) + WrapperDataSource.Declaration.JE_TransportMode;
				case DocumentFilters.T2LDocumentSupport:
					return WrapperDataSource.T2LApplicableEntryLines.Any() ? "Y" : "N";
				case DocumentFilters.T2LFDocumentSupport:
					return WrapperDataSource.T2LFApplicableEntryLines.Any() ? "Y" : "N";

				default:
					return base.GetFilterValue(filterName);
			}
		}
	}
}
