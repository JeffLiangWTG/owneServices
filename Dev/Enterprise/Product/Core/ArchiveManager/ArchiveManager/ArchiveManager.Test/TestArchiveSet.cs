using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Test
{
	class TestArchiveSet : IArchiveSet
	{
		public TestArchiveSet(IArchiveSystemDescriptor descriptor, string stageName, Guid schedulePK, ArchiveItem mainItem, string mainArchiveItemNK = null, List<IArchiveItem> childItems = null)
		{
			SystemDescriptor = descriptor;
			StageName = stageName;
			ScheduleIdentifier = schedulePK;
			MainArchiveItem = mainItem;
			MainArchiveItemNK = mainArchiveItemNK;

			itemDictionary = new Dictionary<Guid, IArchiveItem>();
			itemDictionary.Add(MainArchiveItem.PK, MainArchiveItem);

			if (childItems != null && childItems.Any())
			{
				childItems.ForEach(item => itemDictionary.Add(item.PK, item));
			}
		}

		#region IArchiveSet Members

		public IArchiveItem MainArchiveItem { get; private set; }

		public string MainArchiveItemNK { get; private set; }

		public ZQuery MainArchiveableTypeFilter
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public Guid IgnoredPK
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public int Count
			=> itemDictionary.Count;

		public long TimeTakenToDelete { get; set; }
		public int TotalNumberOfDocumentsGeneratedInSet { get; set; }
		public long TimeTakenToGenerateDocuments { get; set; }
		public long TimeTakenToDeleteAllDocuments { get; set; }

		public IArchiveSystemDescriptor SystemDescriptor { get; }

		public string StageName { get; }

		public Guid ScheduleIdentifier { get; }

		public IArchiveItem GetArchiveItem(Guid itemPK)
			=> throw new NotImplementedException();

		public Guid MainItemFK { get => throw new NotImplementedException(); }

		public ReadOnlyCollection<IArchiveItem> GetArchiveItems()
		{
			if (itemCollection == null)
			{
				var itemList = new List<IArchiveItem>(itemDictionary.Values);
				itemCollection = new ReadOnlyCollection<IArchiveItem>(itemList);
			}

			return itemCollection;
		}

		public IArchiveStepResult Load(IArchiveLogger logger)
			=> throw new NotImplementedException();

		public void MarkToIgnoreForThisRun()
			=> throw new NotImplementedException();

		public ReadOnlyDictionary<string, ITableProcessingInfo> GetTotalDocumentsDeletedFromArchiveItems()
		{
			var totalDocumentsDeleted = new Dictionary<string, ITableProcessingInfo>();
			var itemsWithDocumentsDeleted = GetArchiveItems().Where(item => item.TotalDocumentsDeleted > 0);

			if (itemsWithDocumentsDeleted.Any())
			{
				var totalStorageDocsDeleted = itemsWithDocumentsDeleted.Sum(item => item.TotalDocumentsDeleted);
				var totalStorageMainsDeleted = itemsWithDocumentsDeleted.Count();

				totalDocumentsDeleted.Add(StorageDocsSchema.Constants.TableName, new TableProcessingInfo(totalStorageDocsDeleted));
				totalDocumentsDeleted.Add(StorageMainSchema.Constants.TableName, new TableProcessingInfo(totalStorageMainsDeleted));
			}

			this.totalDocumentsDeleted = new ReadOnlyDictionary<string, ITableProcessingInfo>(totalDocumentsDeleted);
			return this.totalDocumentsDeleted;
		}

		ReadOnlyDictionary<string, ITableProcessingInfo> totalDocumentsDeleted;

		#endregion

		readonly Dictionary<Guid, IArchiveItem> itemDictionary;
		ReadOnlyCollection<IArchiveItem> itemCollection;
	}
}
