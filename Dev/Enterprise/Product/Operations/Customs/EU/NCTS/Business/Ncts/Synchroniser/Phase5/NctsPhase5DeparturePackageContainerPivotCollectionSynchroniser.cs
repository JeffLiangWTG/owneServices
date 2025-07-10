using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	class NctsPhase5DeparturePackageContainerPivotCollectionSynchroniser : BusinessObjectCollectionSynchroniser
	{
		public NctsPhase5DeparturePackageContainerPivotCollectionSynchroniser(ForwardingPackLine source, NctsPackage package)
			: base(source, package)
		{
		}

		protected new ForwardingPackLine Source => (ForwardingPackLine)base.Source;

		protected new NctsPackage Destination => (NctsPackage)base.Destination;

		protected override void ForceSynchroniseCore(bool isDeleted)
		{
			base.ForceSynchroniseCore(isDeleted);
			if (!isDeleted)
			{
				DeleteOrAddContainerPivots();
			}
		}

		protected override void HookElementSynchronisers()
		{
			if (!Destination.IsDeleted)
			{
				HookSynchronisationForExistingContainerPivots();
			}
		}

		IEnumerable<ForwardingContainer> GetSourceContainers() => Source.Containers.Cast<ForwardingContainer>();

		void HookSynchronisationForExistingContainerPivots()
		{
			var destContainerPivotsList = Destination.ContainersPivot.ToList();
			foreach (var sourceContainerPivot in GetSourceContainers())
			{
				var synchroniser = ElementSynchronisers.FindMatchingSource<NctsPhase5DeparturePackageContainerPivotSynchroniser>(sourceContainerPivot);
				if (synchroniser != null)
				{
					synchroniser.SetEnabled(IsEnabled, synchroniser.DetectEnabled);
					destContainerPivotsList.Remove(synchroniser.Destination);
				}
				else
				{
					var sourceReference = sourceContainerPivot.JC_ContainerNum;
					NctsCusInBondContainerPackageGenPivot destContainerPivot;
					while ((destContainerPivot = destContainerPivotsList.FirstOrDefault(x => !x.IsDeleted && GetContainerNumber(x).EqualsIgnoringCase(sourceReference))) != null)
					{
						synchroniser = ElementSynchronisers.FindMatchingDestination<NctsPhase5DeparturePackageContainerPivotSynchroniser>(destContainerPivot);
						if (synchroniser == null)
						{
							ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(new NctsPhase5DeparturePackageContainerPivotSynchroniser(destContainerPivot, sourceContainerPivot), IsEnabled, DetectEnabled);
							destContainerPivotsList.Remove(destContainerPivot);
							break;
						}
						else
						{
							synchroniser.SetEnabled(IsEnabled, synchroniser.DetectEnabled);
							destContainerPivotsList.Remove(destContainerPivot);
						}
					}
				}
			}
		}

		ZString GetContainerNumber(NctsCusInBondContainerPackageGenPivot pivot) => pivot.Container?.BC_ContainerNum ?? ZString.Empty;

		void DeleteOrAddContainerPivots()
		{
			var sourceContainers = GetSourceContainers().ToHashSet();
			var existingDestContainerPivots = Destination.ContainersPivot.ToHashSet();
			if (sourceContainers.Count > 0 && Destination.Parent?.Header is NctsHeader header)
			{
				var headerContainerMapping = header.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>().ToDictionary(x => x.PK, y => y);
				var sourceMappings = new Dictionary<ForwardingContainer, NctsPhase5DeparturePackageContainerPivotSynchroniser>();
				var destinationMappings = new Dictionary<NctsCusInBondContainerPackageGenPivot, NctsPhase5DeparturePackageContainerPivotSynchroniser>();
				ElementSynchronisers.OfType<NctsPhase5DeparturePackageContainerPivotSynchroniser>().ToArray().ForEach(s =>
				{
					var destination = s.Destination;
					if (headerContainerMapping.ContainsKey(destination.XX_Relation2ID))
					{
						sourceMappings.Add(s.Source, s);
						destinationMappings.Add(destination, s);
					}
					else
					{
						ElementSynchronisers.Remove(s);
						existingDestContainerPivots.Remove(destination);
						destination.Delete();
					}
				});
				while (existingDestContainerPivots.Count > 0)
				{
					var existingDestContainerPivot = existingDestContainerPivots.First();
					existingDestContainerPivots.Remove(existingDestContainerPivot);
					if (destinationMappings.TryGetValue(existingDestContainerPivot, out var synchroniser))
					{
						destinationMappings.Remove(existingDestContainerPivot);
						sourceMappings.Remove(synchroniser.Source);
						headerContainerMapping.Remove(existingDestContainerPivot.XX_Relation2ID);
					}
					if (existingDestContainerPivot.IsDeleted || existingDestContainerPivot.IsDeleting)
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
							if (sourceContainers.Contains(synchroniser.Source))
							{
								sourceContainers.Remove(synchroniser.Source);
								synchroniser.Synchronise();
								continue;
							}
						}
						else
						{
							existingDestContainerPivot = FindMatchingContainerPivotAndAddSynchroniser(sourceContainers, existingDestContainerPivots, existingDestContainerPivot, sourceMappings, destinationMappings, headerContainerMapping);
						}
						if (existingDestContainerPivot != null)
						{
							existingDestContainerPivot.Delete();
						}
					}
				}

				foreach (var sourceContainer in sourceContainers)
				{
					var containerNum = sourceContainer.JC_ContainerNum;
					if (headerContainerMapping.Values.FirstOrDefault(c => c.BC_ContainerNum.EqualsIgnoringCase(containerNum)) is NctsDepartureHeaderContainer container)
					{
						headerContainerMapping.Remove(container.PK);
						var destinationContainerPivot = Destination.ContainersPivot.AddPivotFor(container);
						var synchroniser = new NctsPhase5DeparturePackageContainerPivotSynchroniser(destinationContainerPivot, sourceContainer);
						ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
						synchroniser.Synchronise();
					}
				}
			}
			else
			{
				existingDestContainerPivots.ForEach(nctsContainerPivot =>
				{
					nctsContainerPivot.Delete();
				});
			}
		}

		NctsCusInBondContainerPackageGenPivot FindMatchingContainerPivotAndAddSynchroniser(HashSet<ForwardingContainer> sourceContainers,
			HashSet<NctsCusInBondContainerPackageGenPivot> destContainerPivots, NctsCusInBondContainerPackageGenPivot destContainerPivot,
			Dictionary<ForwardingContainer, NctsPhase5DeparturePackageContainerPivotSynchroniser> sourceMappings,
			Dictionary<NctsCusInBondContainerPackageGenPivot, NctsPhase5DeparturePackageContainerPivotSynchroniser> destinationMappings,
			Dictionary<ZGuid, NctsDepartureHeaderContainer> headerContainerMapping)
		{
			var headerContainer = headerContainerMapping[destContainerPivot.XX_Relation2ID];
			var containerNumber = headerContainer.BC_ContainerNum;
			if (sourceContainers.FirstOrDefault(container => containerNumber.EqualsIgnoringCase(container.JC_ContainerNum)) is ForwardingContainer existingSourceContainer)
			{
				headerContainerMapping.Remove(destContainerPivot.XX_Relation2ID);
				if (sourceMappings.TryGetValue(existingSourceContainer, out var synchroniser))
				{
					sourceContainers.Remove(existingSourceContainer);
					var destination = synchroniser.Destination;
					destContainerPivots.Remove(destination);
					sourceMappings.Remove(existingSourceContainer);
					destinationMappings.Remove(destination);
					synchroniser.Synchronise();
				}
				else
				{
					synchroniser = new NctsPhase5DeparturePackageContainerPivotSynchroniser(destContainerPivot, existingSourceContainer);
					ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
					synchroniser.Synchronise();
					sourceContainers.Remove(existingSourceContainer);
					destContainerPivot = null;
				}
			}
			return destContainerPivot;
		}

		protected override IEnumerable<BusinessObjectCollection> GetCollectionsToHookCountChangedEvent() => new[] { Source.Containers };
	}
}
