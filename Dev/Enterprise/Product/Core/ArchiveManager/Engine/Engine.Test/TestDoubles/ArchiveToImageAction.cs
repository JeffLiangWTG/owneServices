using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Engine.Test.TestDoubles
{
	public class ArchiveToImageAction : IArchivePreparationAction
	{
		public ArchiveToImageAction(IArchiveLogger logger, IArchiveSet archiveSet, IArchiveableBusinessObjectProviderCache providerCache)
		{
			this.logger = logger;
			this.archiveSet = archiveSet;
			this.providerCache = providerCache;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public int StallActionForSeconds { get; set; }

		public void Execute()
		{
			logger.LogInfo(archiveSet.SystemDescriptor.Code, $"Generating detailed job reports to archive");

			if (StallActionForSeconds > 0)
			{
				System.Threading.Thread.Sleep(StallActionForSeconds * 1000);
			}

			var storage = new DummyArchiveStorage();
			using (var transactionManager = ((IArchiveAction)storage).BeginTransactionWithManager())
			{
				foreach (var image in Generate(archiveSet))
				{
					storage.Store(image);
				}

				((IArchiveAction)storage).Execute();
				transactionManager.CommitTransaction();
			}
		}

		public IEnumerable<ArchiveImage> Generate(IArchiveSet archiveSet)
		{
			var factory = new BusinessObjectFactory();

			foreach (var item in archiveSet.GetArchiveItems())
			{
				ArchiveImage image = null;

				if (item.PKColumn == DummyBizoSchema.PK)
				{
					var provider = providerCache.GetProvider(item.PKColumn.TableName);
					var archiveableBizOs = provider.LoadArchiveableBusinessObjects(item, factory);

					var archiveableBizO = archiveableBizOs[0];

					if (archiveableBizO != null)
					{
						image = GetDummyBusinessObjectImage("DummyBusinessObject", archiveableBizO.ArchiveableBusinessObject, DummyBizoSchema.Z0_Code, DummyBizoSchema.Z0_Date);
					}
				}
				else if (item.PKColumn.TableName == DummyDependentBizoSchema.PK.TableName)
				{
					var bizO = factory.Load<DummyDependantBusinessObject>(item.PK);
					image = GetDummyBusinessObjectImage("DummyDependantBusinessObject", bizO, DummyDependentBizoSchema.ZD1_Code, DummyDependentBizoSchema.ZD1_Number);
				}

				if (image != null)
				{
					var topLevelParentItem = FindTopLevelParentItem(item);

					if (topLevelParentItem != null)
					{
						var parentProvider = providerCache.GetProvider(topLevelParentItem.PKColumn.TableName);
						var archiveableParents = parentProvider.LoadArchiveableBusinessObjects(topLevelParentItem, factory);

						var archiveableParent = archiveableParents[0];

						image.MetaDataList.Add(new KeyValuePair<string, string>("Parent " + archiveableParent.NaturalKey.KeyType.Code, archiveableParent.NaturalKey.Value));
					}

					if (item.IsReversed)
					{
						image.MetaDataList.Add(new KeyValuePair<string, string>("IsReversed", item.IsReversed.ToString()));
					}

					yield return image;
				}
			}
		}

		IArchiveItem FindTopLevelParentItem(IArchiveItem startingItem)
		{
			var currentItem = startingItem;
			while (currentItem.ParentPK != Guid.Empty)
			{
				currentItem = archiveSet.GetArchiveItem(currentItem.ParentPK);
			}

			return currentItem.PK != startingItem.PK
				? currentItem
				: null;
		}

		readonly IArchiveLogger logger;
		readonly IArchiveSet archiveSet;
		readonly IArchiveableBusinessObjectProviderCache providerCache;

		ArchiveImage GetDummyBusinessObjectImage(string prefixText, BusinessObject dummy, SchemaColumn codeColumn, SchemaColumn dateColumn)
		{
			var image = GetDocumentOutput(prefixText, dummy, codeColumn, dateColumn);
			var archiveImage = new ArchiveImage(image);
			archiveImage.MetaDataList.Add(new KeyValuePair<string, string>("Code", dummy[codeColumn].ToString()));
			return archiveImage;
		}

		byte[] GetDocumentOutput(string prefixText, BusinessObject dummy, SchemaColumn codeColumn, SchemaColumn dateColumn)
		{
			var dummyReportText = prefixText + " " + dummy[codeColumn] + " " + dummy[dateColumn].ToString();
			var reportBytes = System.Text.Encoding.Unicode.GetBytes(dummyReportText);
			return reportBytes;
		}
	}
}
