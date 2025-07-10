using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineExDocEstablishmentAndTimeCollection : DependentBusinessObjectCollection<QuarantineExDocEstablishmentAndTime, QuarantineExDocLine>
	{
		public QuarantineExDocEstablishmentAndTimeCollection(QuarantineExDocLine line)
			: base(line)
		{
		}

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			base.SetCollectionRelationships(dependent);
			((QuarantineExDocEstablishmentAndTime)dependent).ParentCollection = this;
		}

		CachedProperty<ProcessCount> processCounts;
		public ProcessCount ProcessCounts => Factory.GetValue(ref processCounts, GetProcessCounts);

		ProcessCount GetProcessCounts()
		{
			ZDateTime earliestPackingStartDate = ZDate.Empty;
			ZDateTime earliestPackingEndDate = ZDate.Empty;
			ZDateTime earliestFreezingStartDate = ZDate.Empty;
			ZDateTime latestSlaughterEndDate = ZDate.Empty;
			bool hasHarvest = false;
			int hasCatcherVessel = 0;
			int hasCatcherBoat = 0;
			int hasFreezing = 0;
			bool hasPacking = false;
			bool hasProcessing = false;
			bool hasSlaughter = false;
			bool hasStorage = false;
			int hasAquacultureFarm = 0;
			foreach (QuarantineExDocEstablishmentAndTime process in this)
			{
				if (process.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.Packing &&
					(earliestPackingStartDate.IsEmpty || process.EE_StartDate < earliestPackingStartDate))
				{
					earliestPackingStartDate = process.EE_StartDate;
				}
				if (process.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.Packing &&
					(earliestPackingEndDate.IsEmpty || process.EE_EndDate < earliestPackingEndDate))
				{
					earliestPackingEndDate = process.EE_EndDate;
				}
				if (process.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.Freezing &&
					(earliestFreezingStartDate.IsEmpty || process.EE_StartDate < earliestFreezingStartDate))
				{
					earliestFreezingStartDate = process.EE_StartDate;
				}
				if (process.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.Slaughter &&
					(latestSlaughterEndDate.IsEmpty || process.EE_EndDate > latestSlaughterEndDate))
				{
					latestSlaughterEndDate = process.EE_EndDate;
				}
				hasHarvest |= process.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.Harvest;
				hasCatcherVessel += process.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.CatcherVessel ? 1 : 0;
				hasCatcherBoat += process.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.CatcherBoat ? 1 : 0;
				hasFreezing += process.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.Freezing ? 1 : 0;
				hasPacking |= process.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.Packing;
				hasProcessing |= process.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.Processing;
				hasSlaughter |= process.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.Slaughter;
				hasStorage |= process.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.Storage;
				hasAquacultureFarm += process.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.AquacultureFarm ? 1 : 0;
			}
			return new ProcessCount(hasHarvest, hasCatcherVessel, hasCatcherBoat, hasFreezing, hasPacking, hasProcessing, hasSlaughter, hasStorage, hasAquacultureFarm, earliestPackingStartDate, earliestPackingEndDate, earliestFreezingStartDate, latestSlaughterEndDate);
		}

		public QuarantineExDocEstablishmentAndTime FindByProcessTypeAndEstablishment(ZString processType, ZString establishmentNumber)
		{
			QuarantineExDocEstablishmentAndTime result = null;

			foreach (QuarantineExDocEstablishmentAndTime currentProcess in this)
			{
				if (currentProcess.EE_ProcessingType == processType && currentProcess.EE_AuthorisationEstablishmentID == establishmentNumber)
				{
					result = currentProcess;
					break;
				}
			}

			return result;
		}

		public void SynchroniseProcessAddresses()
		{
			foreach (QuarantineExDocEstablishmentAndTime process in this)
			{
				process.SyncAddress();
			}
		}

		#region Cloning Stuff
		public void Clone(QuarantineExDocEstablishmentAndTimeCollection collectionToClone, Dictionary<ZGuid, ZGuid> jobDocAddressPKPairs)
		{
			RemoveAndDeleteAll();
			foreach (QuarantineExDocEstablishmentAndTime process in collectionToClone)
			{
				var clonedProcess = process.Clone(jobDocAddressPKPairs);
				using (clonedProcess.SuspendSettingHasChanges())
				using (clonedProcess.GetValidationSuspender())
				{
					((IBusinessObjectInternals)clonedProcess).IsCopying = true;
					try
					{
						Add(clonedProcess);
					}
					finally
					{
						((IBusinessObjectInternals)clonedProcess).IsCopying = false;
					}
				}
			}
		}
		#endregion
	}
}
