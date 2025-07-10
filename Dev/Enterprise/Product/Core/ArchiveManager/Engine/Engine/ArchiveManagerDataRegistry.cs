using System;
using CargoWise.Types;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ArchiveManager.Engine
{
	public sealed class ArchiveManagerDataRegistry : RegistryItemSet
	{
		public static ArchiveManagerDataRegistry Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new ArchiveManagerDataRegistry();
				}

				return instance;
			}
		}

		public override bool IsForProductivityWise => true;

		public bool IsArchiveRelationshipsTableUpToDateForStage(string suffix, string stageName)
		{
			return GetLastVersionRelationshipsTableWasCreatedForItem(suffix, stageName).Value == new EnterpriseInformationRetriever().VersionNumber;
		}

		public void SetLastVersionRelationshipsTableWasCreatedForStage(string suffix, string stageName)
		{
			GetLastVersionRelationshipsTableWasCreatedForItem(suffix, stageName).SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				new EnterpriseInformationRetriever().VersionNumber);
		}

		public IArchiveWatermark GetWatermark(Guid ownerGuid, string stageName)
			=> GetWatermarkRegistry(stageName, ownerGuid);

		public void SetWatermark(Guid ownerGuid, string stageName, IArchiveWatermark watermark)
			=> SetWatermarkRegistry(watermark, stageName, ownerGuid);

		IArchiveWatermark GetWatermarkRegistry(string stageName, Guid ownerGuid)
		{
			var datewatermark = GetDateWatermark(stageName).GetValueWithoutFallback(ownerGuid, Guid.Empty, Guid.Empty);
			var nkWatermark = GetNKWatermark(stageName).GetValueWithoutFallback(ownerGuid, Guid.Empty, Guid.Empty);
			var pkWatermark = GetPKWatermark(stageName).GetValueWithoutFallback(ownerGuid, Guid.Empty, Guid.Empty);

			return new ArchiveWatermark { WatermarkDate = new ZDateTime(datewatermark), WatermarkNK = nkWatermark, WatermarkPK = pkWatermark };
		}

		void SetWatermarkRegistry(IArchiveWatermark watermark, string stageName, Guid ownerGuid)
		{
			var watermarkDate = !watermark.WatermarkDate.IsValid ? ZDateTime.MinSmallDateTimeValue.ToDateTime() : watermark.WatermarkDate.ToDateTime();
			GetDateWatermark(stageName).SetValue(ownerGuid, Guid.Empty, Guid.Empty, watermarkDate);
			GetNKWatermark(stageName).SetValue(ownerGuid, Guid.Empty, Guid.Empty, watermark.WatermarkNK);
			GetPKWatermark(stageName).SetValue(ownerGuid, Guid.Empty, Guid.Empty, watermark.WatermarkPK);
		}

		DateTimeRegistryItem GetDateWatermark(string stageName)
		{
			return GetItem("ArchiveWatermark|" + stageName, delegate
			{
				return new DateTimeRegistryItem("ArchiveWatermark|" + stageName, null,
					(NoResString)"Archive Watermark Date",
					(NoResString)"Non-Visible registry to store archive watermark date for each archive schedule",
					RegistryStorageFlags.Company,
					RegistryOptions.IsHidden,
					ZDateTime.MinSmallDateTimeValue.ToDateTime());
			});
		}

		StringRegistryItem GetNKWatermark(string stageName)
		{
			return GetItem("ArchiveWatermarkNK|" + stageName, () =>
			{
				return new StringRegistryItem("ArchiveWatermarkNK|" + stageName, null,
					(NoResString)"Archive Watermark NK",
					(NoResString)"Non-Visible registry to store archive watermark NK for each archive schedule.",
					RegistryStorageFlags.Company,
					RegistryOptions.IsHidden,
					string.Empty);
			});
		}

		GuidRegistryItem GetPKWatermark(string stageName)
		{
			return GetItem("ArchiveWatermarkPK|" + stageName, () =>
			{
				return new GuidRegistryItem("ArchiveWatermarkPK|" + stageName, null,
					(NoResString)"Archive Watermark PK",
					(NoResString)"Non-Visible registry to store archive watermark PK for each archive schedule.",
					RegistryStorageFlags.Company,
					RegistryOptions.IsHidden,
					Guid.Empty);
			});
		}

		StringRegistryItem GetLastVersionRelationshipsTableWasCreatedForItem(string suffix, string stageName)
		{
			var key = $"LastVersionRelationshipsTableWasCreatedForStage{suffix}{stageName}";
			var registryItem = GetItem(key, () =>
			{
				return new StringRegistryItem(key, null,
					(NoResString)"Last Version Archive Relationships Table Was Created For Stage",
					(NoResString)"Non-Visible registry to track when we last created the ArchiveRelationships table for a certain archive stage.",
					RegistryStorageFlags.Company,
					RegistryOptions.IsHidden,
					string.Empty);
			});

			return registryItem;
		}

		[ThreadStatic]
		static ArchiveManagerDataRegistry instance;
	}
}
