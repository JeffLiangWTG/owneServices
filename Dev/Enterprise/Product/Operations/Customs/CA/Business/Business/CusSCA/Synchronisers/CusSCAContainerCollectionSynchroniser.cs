
namespace Enterprise.Customs.CA.Business
{
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.EntityFramework;
	using Enterprise.Freight.Forwarding.Business;

	public class CusSCAContainerCollectionSynchroniser : Customs.Business.BusinessObjectCollectionSynchroniser
	{
		public CusSCAContainerCollectionSynchroniser(ForwardingConsol source, CusSCAOceanBill destination)
			: base(source, destination)
		{
		}

		protected new ForwardingConsol Source
		{
			get { return (ForwardingConsol)base.Source; }
		}

		protected new CusSCAOceanBill Destination
		{
			get { return (CusSCAOceanBill)base.Destination; }
		}

		#region Synchronise

		protected override void ForceSynchroniseCore(bool isDeleted)
		{
			base.ForceSynchroniseCore(isDeleted);
			if (!isDeleted)
			{
				DeleteOrAddContainers();
			}
		}

		protected override void HookElementSynchronisers()
		{
			if (!Destination.IsDeleted)
			{
				HookSynchronisationForExistingContainers();
			}
		}

		void HookSynchronisationForExistingContainers()
		{
			var destinationList = new List<CusSCAContainer>();
			foreach (CusSCAContainer container in Destination.Containers)
			{
				destinationList.Add(container);
			}
			foreach (ForwardingContainer sourceContainer in Source.Containers)
			{
				var synchroniser = ElementSynchronisers.FindMatchingSource<CusSCAContainerSynchroniser>(sourceContainer);
				if (synchroniser != null)
				{
					synchroniser.SetEnabled(IsEnabled, DetectEnabled);
					destinationList.Remove(synchroniser.Destination);
				}
				else
				{
					var container = sourceContainer.JC_ContainerNum;
					CusSCAContainer destinationContainer = null;
					while ((destinationContainer = destinationList.FirstOrDefault(x => !x.IsDeleted && x.CN_ContainerNumber == container)) != null)
					{
						synchroniser = ElementSynchronisers.FindMatchingDestination<CusSCAContainerSynchroniser>(destinationContainer);
						if (synchroniser == null)
						{
							ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(new CusSCAContainerSynchroniser(destinationContainer, sourceContainer), IsEnabled, DetectEnabled);
							destinationList.Remove(destinationContainer);
							break;
						}
						else
						{
							synchroniser.SetEnabled(IsEnabled, DetectEnabled);
							destinationList.Remove(destinationContainer);
						}
					}
				}
			}
		}

		void DeleteOrAddContainers()
		{
			var sourceList = new List<ForwardingContainer>();
			foreach (ForwardingContainer container in Source.Containers)
			{
				sourceList.Add(container);
			}
			var destinationList = new List<CusSCAContainer>();
			foreach (CusSCAContainer container in Destination.Containers)
			{
				destinationList.Add(container);
			}

			if (sourceList.Count > 0)
			{
				while (destinationList.Count > 0)
				{
					var existingDestinatinationContainer = destinationList[0];
					destinationList.Remove(existingDestinatinationContainer);
					var synchroniser = ElementSynchronisers.FindMatchingDestination<CusSCAContainerSynchroniser>(existingDestinatinationContainer);
					if (existingDestinatinationContainer.IsDeleted || existingDestinatinationContainer.IsDeleting)
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
							if (sourceList.Contains(synchroniser.Source))
							{
								sourceList.Remove(synchroniser.Source);
								synchroniser.Synchronise();
								continue;
							}
						}
						else
						{
							existingDestinatinationContainer = FindMatchingContainerAndAddSynchroniser(sourceList, destinationList, existingDestinatinationContainer);
						}

						if (existingDestinatinationContainer != null && existingDestinatinationContainer.CN_TypeOfContainer != CusSCAHouse.NonContaineriseID)
						{
							existingDestinatinationContainer.Delete();
						}
					}
				}

				foreach (var sourceContainer in sourceList)
				{
					var destinationContainer = Destination.Containers.AddNew();
					var synchroniser = new CusSCAContainerSynchroniser(destinationContainer, sourceContainer);
					ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
					synchroniser.Synchronise();
				}
			}
			else
			{
				destinationList.ForEach(container => { if (container.CN_TypeOfContainer != CusSCAHouse.NonContaineriseID) { container.Delete(); } });
			}
		}

		CusSCAContainer FindMatchingContainerAndAddSynchroniser(List<ForwardingContainer> sourceList, List<CusSCAContainer> destinationList, CusSCAContainer containerToFind)
		{
			ForwardingContainer existingContainer = null;
			var alreadyProcessedContainers = new List<ForwardingContainer>();
			while ((existingContainer = sourceList.FirstOrDefault(container => !alreadyProcessedContainers.Contains(container) && container.JC_ContainerNum == containerToFind.CN_ContainerNumber)) != null)
			{
				alreadyProcessedContainers.Add(existingContainer);
				var synchroniser = ElementSynchronisers.FindMatchingSource<CusSCAContainerSynchroniser>(existingContainer);
				if (synchroniser != null)
				{
					sourceList.Remove(existingContainer);
					destinationList.Remove(synchroniser.Destination);
					synchroniser.Synchronise();
					break;
				}
				else
				{
					synchroniser = new CusSCAContainerSynchroniser(containerToFind, existingContainer);
					ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
					synchroniser.Synchronise();
					sourceList.Remove(existingContainer);
					containerToFind = null;
					break;
				}
			}
			return containerToFind;
		}

		protected override IEnumerable<BusinessObjectCollection> GetCollectionsToHookCountChangedEvent()
		{
			return new[] { Source.Containers };
		}

		#endregion

	}
}
