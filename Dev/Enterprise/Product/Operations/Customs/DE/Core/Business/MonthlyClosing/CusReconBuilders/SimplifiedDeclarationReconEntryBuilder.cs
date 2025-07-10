using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	abstract class SimplifiedDeclarationReconEntryBuilder
	{
		public SimplifiedDeclarationReconEntryBuilder(CusEntryHeader entryHeader, IImportDecHeader provider)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			this.provider = Argument.NotNull(provider, nameof(provider));
			factory = this.entryHeader.Factory;
		}

		public CusReconEntry CreateLodgedEntryAndSnapshot()
		{
			var declaration = entryHeader.Declaration;

			reconEntry = factory.New<CusReconEntry>();
			reconEntry.CRE_EntryType = provider.DeclarationType;
			reconEntry.CRE_GB_Branch = declaration.JE_GB;
			reconEntry.CRE_OA_DeclarantAddress = declaration.DeclarantAddress.PK;
			reconEntry.CRE_OA_ImporterAddress = declaration.ImporterDocumentaryAddress.Address?.PK ?? ZGuid.Empty;
			reconEntry.CRE_OA_RepresentativeAddress = declaration.Representative?.PK ?? ZGuid.Empty;
			reconEntry.CRE_OA_BuyingAgentAddress = declaration.BuyingAgentAddress?.PK ?? ZGuid.Empty;
			reconEntry.CRE_CH_OriginalEntry = entryHeader.PK;

			var snapshotProvider = new SimplifiedDeclarationEntrySnapshotProvider(provider);
			var snapshotBuilder = new CusReconEntrySnapshotBuilder(snapshotProvider);
			var snapShot = reconEntry.CusReconSnapshots.AddNew();
			snapShot.CRS_Type = CusReconConstants.Lodged;
			snapShot.CRS_SnapshotXml = snapshotBuilder.GetXMLMessage();

			BuildLodgedLines();

			return reconEntry;
		}

		public CusReconEntry CreateCurrentSnapshot()
		{
			reconEntry = entryHeader.GetCusReconEntryFromDB();
			if (reconEntry != null)
			{
				var lodgedSnapshot = Customs.Business.CusReconSnapshot.Loader.GetSingleCusReconSnapshot(factory, CusReconConstants.Lodged, reconEntry);
				var lodge = CusReconEntrySnapshotBuilder.Deserialize(lodgedSnapshot.CRS_SnapshotXml);

				var existingCurrentSnapshot = reconEntry.CurrentSnapshot;
				if (existingCurrentSnapshot != null)
				{
					reconEntry.CusReconSnapshots.Delete(existingCurrentSnapshot);
				}

				var snapshotProvider = new SimplifiedDeclarationEntrySnapshotProvider(provider);
				var snapshotBuilder = new CusReconEntrySnapshotBuilder(snapshotProvider);
				var cur = snapshotBuilder.GenerateMessage();
				if (Diff(cur, lodge))
				{
					AddCurrentSnapshot(cur);
				}
				else if (HasChangesNotInSnapshotProvider())
				{
					AddCurrentSnapshot(snapshotBuilder.GenerateEmptyMessage());
				}

				BuildCurrentLines();
			}

			return reconEntry;

			void AddCurrentSnapshot(DEMonthlyClosingEntrySnapshot snapshotObj)
			{
				var snapShot = reconEntry.CusReconSnapshots.AddNew();
				snapShot.CRS_Type = CusReconConstants.Current;
				snapShot.CRS_SnapshotXml = CusReconEntrySnapshotBuilder.Serialize(snapshotObj);
			}
		}

		protected abstract void BuildLodgedLines();

		protected abstract void BuildCurrentLines();

		protected internal abstract bool HasChangesNotInSnapshotProvider();

		bool Diff(DEMonthlyClosingEntrySnapshot cur, DEMonthlyClosingEntrySnapshot ldg)
		{
			bool docsDiffer;

			var currentDocsEmpty = cur.Document.IsNullOrEmpty();
			var lodgedDocsEmpty = ldg.Document.IsNullOrEmpty();

			if (currentDocsEmpty && lodgedDocsEmpty)
			{
				docsDiffer = false;
			}
			else if (currentDocsEmpty || lodgedDocsEmpty)
			{
				docsDiffer = true;
			}
			else if (cur.Document.Length != ldg.Document.Length)
			{
				docsDiffer = true;
			}
			else
			{
				docsDiffer = !cur.Document.EqualIgnoringOrder(ldg.Document, new DEMonthlyClosingEntrySnapshotDocumentEqualityComparer());
			}

			return docsDiffer;
		}

		protected CusReconEntry PersistedReconEntry => ReadOnlyFactory.Load<CusReconEntry>(reconEntry.PK);

		ReadOnlyBusinessObjectFactory ReadOnlyFactory => readOnlyFactoryCached ?? (readOnlyFactoryCached = new ReadOnlyBusinessObjectFactory());
		ReadOnlyBusinessObjectFactory readOnlyFactoryCached;

		protected CusReconEntry reconEntry;
		readonly CusEntryHeader entryHeader;
		readonly IImportDecHeader provider;
		readonly BusinessObjectFactory factory;
	}
}
