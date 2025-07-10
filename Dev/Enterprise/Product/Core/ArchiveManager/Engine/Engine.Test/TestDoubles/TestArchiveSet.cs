using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Engine.Test.TestDoubles
{
	internal class TestArchiveSet : IArchiveSet
	{
		public TestArchiveSet(IArchiveSystemDescriptor descriptor, string stageName, Guid scheduleIdentifier, ArchiveItem mainItem, string mainArchiveItemNK = null, List<IArchiveItem> childItems = null)
		{
			SystemDescriptor = descriptor;
			StageName = stageName;
			ScheduleIdentifier = scheduleIdentifier;
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

		public string MainArchiveItemNK { get; set; }

		public Guid IgnoredPK { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public Guid MainItemFK { get => throw new NotImplementedException(); }

		public ZQuery MainArchiveableTypeFilter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public int Count
			=> itemDictionary.Count;

		public int TotalNumberOfDocumentsGeneratedInSet { get; set; }

		public IArchiveSystemDescriptor SystemDescriptor { get; }

		public long TimeTakenToDelete { get; set; }
		public long TimeTakenToGenerateDocuments { get; set; }
		public long TimeTakenToDeleteAllDocuments { get; set; }

		public string StageName { get; }

		public Guid ScheduleIdentifier { get; }

		public IArchiveItem GetArchiveItem(Guid itemPK)
			=> throw new NotImplementedException();

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
			=> throw new NotImplementedException();

		#endregion

		readonly Dictionary<Guid, IArchiveItem> itemDictionary;
		ReadOnlyCollection<IArchiveItem> itemCollection;
	}
}
