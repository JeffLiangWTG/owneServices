using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	class NctsPhase5DeparturePackageCollectionSynchroniser : BusinessObjectCollectionSynchroniser
	{
		public NctsPhase5DeparturePackageCollectionSynchroniser(NctsCommonCargoDesc goodsLine, ForwardingShipment source, ForwardingPackLine[] sourcePackLines) : base(source, goodsLine)
		{
			this.sourcePackLines = sourcePackLines;
		}
		readonly ForwardingPackLine[] sourcePackLines;

		protected IEnumerable<ForwardingPackLine> GetSourcePacksToSynchronise()
		{
			if (sourcePackLines == null)
			{
				var result = new List<ForwardingPackLine>();
				var bills = Source.OuterPackLines.ToArray<ForwardingPackLine>();
				result.AddRange(bills);
				return result;
			}
			else
			{
				return sourcePackLines;
			}
		}

		protected new ForwardingShipment Source => (ForwardingShipment)base.Source;

		protected new NctsCommonCargoDesc Destination => (NctsCommonCargoDesc)base.Destination;

		protected override void ForceSynchroniseCore(bool isDeleted)
		{
			base.ForceSynchroniseCore(isDeleted);
			if (!isDeleted)
			{
				DeleteOrAddPacks();
			}
		}

		protected override void HookElementSynchronisers()
		{
			if (!Destination.IsDeleted)
			{
				HookSynchronisationForExistingPacks();
			}
		}

		void HookSynchronisationForExistingPacks()
		{
			var destPacksList = Destination.Packages.ToList();
			foreach (var sourcePack in GetSourcePacksToSynchronise())
			{
				var synchroniser = ElementSynchronisers.FindMatchingSource<NctsPhase5DeparturePackageSynchroniser>(sourcePack);
				if (synchroniser != null)
				{
					synchroniser.SetEnabled(IsEnabled, synchroniser.DetectEnabled);
					destPacksList.Remove(synchroniser.Destination);
				}
				else
				{
					var sourceNumberOfPacks = sourcePack.JL_PackageCount;
					var sourcePackType = sourcePack.JL_F3_NKPackType;
					NctsPackage destPack;
					while ((destPack = destPacksList.FirstOrDefault(x => !x.IsDeleted && x.B5_UnitCount == sourceNumberOfPacks && PackageTypeMatches(sourcePackType, x.B5_UnitType))) != null)
					{
						synchroniser = ElementSynchronisers.FindMatchingDestination<NctsPhase5DeparturePackageSynchroniser>(destPack);
						if (synchroniser == null)
						{
							ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(new NctsPhase5DeparturePackageSynchroniser(destPack, sourcePack), IsEnabled, DetectEnabled);
							destPacksList.Remove(destPack);
							break;
						}
						else
						{
							synchroniser.SetEnabled(IsEnabled, synchroniser.DetectEnabled);
							destPacksList.Remove(destPack);
						}
					}
				}
			}
		}

		bool PackageTypeMatches(string sourcePackType, string destPackType) => Converter.GetTwoCharacterUnitType(sourcePackType).MappedCode == destPackType;

		InvoiceLineFromOrderLineSynchroniser converter;
		InvoiceLineFromOrderLineSynchroniser Converter => converter ?? (converter = new InvoiceLineFromOrderLineSynchroniser());

		void DeleteOrAddPacks()
		{
			var sourcePacks = GetSourcePacksToSynchronise().ToList();
			var existingDestPacks = Destination.Packages.ToList();
			if (sourcePacks.Count > 0)
			{
				while (existingDestPacks.Count > 0)
				{
					var existingDestPack = existingDestPacks[0];
					existingDestPacks.Remove(existingDestPack);
					var synchroniser = ElementSynchronisers.FindMatchingDestination<NctsPhase5DeparturePackageSynchroniser>(existingDestPack);
					if (existingDestPack.IsDeleted || existingDestPack.IsDeleting)
					{
						if (synchroniser != null)
						{
							ElementSynchronisers.Remove(synchroniser);
						}
					}
					else
					{
						if (synchroniser != null)
						{
							if (sourcePacks.Contains(synchroniser.Source))
							{
								sourcePacks.Remove(synchroniser.Source);
								synchroniser.Synchronise();
								continue;
							}
						}
						else
						{
							existingDestPack = FindMatchingPackAndAddSynchroniser(sourcePacks, existingDestPacks, existingDestPack);
						}

						if (existingDestPack != null)
						{
							existingDestPack.Delete();
						}
					}
				}

				foreach (var sourcePack in sourcePacks)
				{
					var destinationBill = Destination.Packages.AddNew();
					var synchroniser = new NctsPhase5DeparturePackageSynchroniser(destinationBill, sourcePack);
					ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
					synchroniser.Synchronise();
				}
			}
			else
			{
				existingDestPacks.ForEach(nctsPack =>
				{
					nctsPack.Delete();
				});
			}
		}

		NctsPackage FindMatchingPackAndAddSynchroniser(List<ForwardingPackLine> sourcePacks, List<NctsPackage> destPacks, NctsPackage destPack)
		{
			ForwardingPackLine existingSourcePack;
			var alreadyProcessedSourcePacks = new List<ForwardingPackLine>();
			while ((existingSourcePack = sourcePacks.FirstOrDefault(pack => !alreadyProcessedSourcePacks.Contains(pack) && IsPackageMatched(pack, destPack))) != null)
			{
				alreadyProcessedSourcePacks.Add(existingSourcePack);
				var synchroniser = ElementSynchronisers.FindMatchingSource<NctsPhase5DeparturePackageSynchroniser>(existingSourcePack);
				if (synchroniser != null)
				{
					sourcePacks.Remove(existingSourcePack);
					destPacks.Remove(synchroniser.Destination);
					synchroniser.Synchronise();
					break;
				}
				else
				{
					synchroniser = new NctsPhase5DeparturePackageSynchroniser(destPack, existingSourcePack);
					ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
					synchroniser.Synchronise();
					sourcePacks.Remove(existingSourcePack);
					destPack = null;
					break;
				}
			}
			return destPack;
		}

		bool IsPackageMatched(ForwardingPackLine sourcePack, NctsPackage destPack)
		{
			return PackageTypeMatches(sourcePack.JL_F3_NKPackType, destPack.B5_UnitType) && sourcePack.JL_PackageCount == destPack.B5_UnitCount;
		}

		protected override IEnumerable<BusinessObjectCollection> GetCollectionsToHookCountChangedEvent() => new[] { Source.OuterPackLines };
	}
}

