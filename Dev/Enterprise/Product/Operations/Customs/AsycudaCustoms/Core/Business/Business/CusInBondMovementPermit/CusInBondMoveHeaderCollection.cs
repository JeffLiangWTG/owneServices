using System.Collections;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CusInBondMoveHeaderCollection : ActiveBusinessObjectCollection<CusInBondMoveHeader>
	{
		public CusInBondMoveHeaderCollection(CusEntryInstruction entryInstruction)
			: base(entryInstruction.Factory, new AdhocCollectionRelationship(typeof(CusInBondMoveHeader)))
		{
			this.entryInstruction = entryInstruction;
		}

		public CusInBondMoveHeaderCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public void Reload()
		{
			((IList)this).Clear();
			GetOrCreateHeader(true);
			((IBindingList)this).ListChanged += CusInBondMoveHeaderCollection_ListChanged;
			var query = new ZQuery(CusInBondMoveHeaderSchema.BM_BH, header.PK);
			query.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, CusInBondMoveHeader.PermitNumberType);
			query.FetchOnlyFromLocalCache = !header.IsInDatabase;
			AddRange(Factory.Load<CusInBondMoveHeader>(query));
		}

		protected override void SetRelationshipDefaultsForElementCore(CusInBondMoveHeader newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			newElement.BM_BH = GetOrCreateHeader(true).PK;
		}

		readonly CusEntryInstruction entryInstruction;

		CusInBondHeader GetOrCreateHeader(bool shouldCreate = true)
		{
			if (header == null || header.IsDeleted)
			{
				var query = new ZQuery(CusInBondHeaderSchema.BH_ParentID, entryInstruction.PK);
				query.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondHeader.AsycudaEntryInstructionType);
				query.FetchOnlyFromLocalCache = !entryInstruction.IsInDatabase;
				header = Factory.LoadTop1<CusInBondHeader>(query);
				if (header == null && shouldCreate)
				{
					header = Factory.New<CusInBondHeader>();
					header.BH_ParentID = entryInstruction.PK;
					header.BH_ParentTableCode = entryInstruction.TablePrefix;
					header.BH_GB = entryInstruction.JobDeclaration?.JE_GB ?? GlbBranch.CurrentBranch.PK;
				}
			}

			return header;
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		internal CusInBondHeader header;

		void CusInBondMoveHeaderCollection_ListChanged(object sender, ListChangedEventArgs e)
		{
			if (Count == 0)
			{
				GetOrCreateHeader(false)?.Delete();
			}

			var declaration = entryInstruction?.JobDeclaration;
			if (declaration != null && declaration.IsDocManagerInfoLoaded)
			{
				var docManagerInfo = entryInstruction?.JobDeclaration?.DocManagerInfo;
				var relatedObjs = docManagerInfo.RelatedObjects;
				var storageMain = docManagerInfo.MasterFactory.GetStorageMainForPK(declaration.PK) as StorageMain;
				if (storageMain != null && storageMain.IsRelatedParentMainsLoaded)
				{
					var existingStorageMains = storageMain.RelatedParentMains.ToList();
					foreach (StorageMain existingStorageMain in existingStorageMains)
					{
						if (existingStorageMain.SM_Type == Core.Constants.RefDocTypes.AsycudaCusInBondMoveHeader && relatedObjs.All(a => a.PK != existingStorageMain.SM_ParentFK))
						{
							storageMain.RelatedParentMains.Remove(existingStorageMain);
							existingStorageMain.Delete();
						}
					}

					foreach (StorageMain newStorageMain in storageMain.LoadRelatedParentMains(true))
					{
						if (newStorageMain.SM_Type == Core.Constants.RefDocTypes.AsycudaCusInBondMoveHeader && existingStorageMains.All(a => a.PK != newStorageMain.PK))
						{
							storageMain.RelatedParentMains.Add(newStorageMain);
						}
					}
				}
			}
		}
	}
}
