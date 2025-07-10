using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaConsolContainerCollectionSynchroniser : BusinessObjectCollectionSynchroniser
	{
		public AsycudaConsolContainerCollectionSynchroniser(ForwardingConsol source, AsycudaManifestHeader destination)
			: base(source, destination)
		{
		}

		protected new AsycudaManifestHeader Destination
		{
			get { return (AsycudaManifestHeader)base.Destination; }
		}

		protected new ForwardingConsol Source
		{
			get { return (ForwardingConsol)base.Source; }
		}

		protected override void OnDetectEnabledChanged()
		{
			if (DetectEnabled)
			{
				Destination.Containers.Load();
			}
			base.OnDetectEnabledChanged();
		}

		protected override void HookElementSynchronisers()
		{
			if (!Destination.AMA_OverrideFreightDefaults)
			{
				if (!Source.IsAir)  // Can't ask destination as it won't necessarily be set yet
				{
					SynchroniseContainers(GetSourceContainersToSynchronise());
				}
			}
		}

		protected virtual AsycudaContainerSynchroniser GetNewContainerSynchroniser(AsycudaContainer cusContainer, ForwardingContainer sourceContainer)
			=> new AsycudaContainerSynchroniser(cusContainer, sourceContainer, null);

		IEnumerable<ForwardingContainer> GetSourceContainersToSynchronise()
		{
			// I dn't know why returning Source.Containers isn't good enough, but if we use that instead of the below they we have ooruble bringing over the corrrect number of containers and deleting superflous ones during synch. The below looks a bit OTT but is spot on... somewhy. 
			var containers = new List<ForwardingContainer>();
			{
				foreach (ForwardingShipment sourceShipment in Source.Shipments)
				{
					var containersForThisSourceShipmentOnThisConsol = sourceShipment.ContainersOnConsol(Source).ToArray<ForwardingContainer>();
					if (containersForThisSourceShipmentOnThisConsol != null)
					{
						containers.AddRange(containersForThisSourceShipmentOnThisConsol);
					}
				}
				return containers.Where(fc => fc != null && !fc.IsDeleted
							 && fc.Consol != null && !fc.Consol.IsDomestic()).Distinct();
			}
		}

		void SynchroniseContainers(IEnumerable<ForwardingContainer> sourceContainers)
		{
			DeleteContainersIfNotInSource(sourceContainers);
			if (!SyncChangesDetected)
			{
				var existingContainers = Destination.Containers.ToList();
				foreach (var sourceContainer in sourceContainers)
				{
					var cusContainer = GetOrCreateContainer(sourceContainer, existingContainers);
					if (SyncChangesDetected)
					{
						return;
					}
					if (cusContainer != null)
					{
						ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(GetNewContainerSynchroniser(cusContainer, sourceContainer), IsEnabled, DetectEnabled);
					}
				}
			}
		}

		void DeleteContainersIfNotInSource(IEnumerable<ForwardingContainer> sourceCollection)
		{
			var shouldDeleteContainers = Destination.ShouldDeleteContainersDuringSynch;
			var collection = new List<ForwardingContainer>(sourceCollection);
			foreach (var container in Destination.Containers.OfType<AsycudaContainer>().ToArray())
			{
				if (!shouldDeleteContainers)
				{
					var sourceContainer = collection.FirstOrDefault(x => x.JC_ContainerNum == container.ACN_ContainerNumber);
					if (sourceContainer != null)
					{
						collection.Remove(sourceContainer);
						continue;
					}
				}
				if (DetectEnabled)
				{
					SyncChangesDetected = true;
					return;
				}
				container.Delete();
			}
		}

		AsycudaContainer GetOrCreateContainer(ForwardingContainer sourceContainer, List<AsycudaContainer> existingContainers)
		{
			var containerNumber = sourceContainer.JC_ContainerNum;
			var result = existingContainers.FirstOrDefault(ac => ac.ACN_ContainerNumber == containerNumber && !ac.IsDeleted);
			if (result != null)
			{
				if (DetectEnabled)
				{
					SyncChangesDetected = true;
					return null;
				}
			}
			else if (!Destination.ShouldDeleteContainersDuringSynch)
			{
				if (DetectEnabled)
				{
					SyncChangesDetected = true;
					return null;
				}
				using (Destination.Containers.SuspendSettingHasChanges())
				{
					result = Destination.Containers.AddNew();
					result.ACN_ContainerNumber = sourceContainer.JC_ContainerNum;
				}
			}
			return result;
		}

		#region Hook/UnHook Events

		protected override void HookEvents()
		{
			base.HookEvents();
			foreach (ForwardingShipment s in Source.Shipments)
			{
				s.OuterPackLines.CountChanged -= OuterPackLines_CountChanged;
				s.OuterPackLines.CountChanged += OuterPackLines_CountChanged;
			}
		}

		void OuterPackLines_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			var pack = (PackLine)e.BizObject;
			if (pack != null)
			{
				if (e.ItemAdded)
				{
					pack.Containers.CountChanged -= Collection_CountChanged;
					pack.Containers.CountChanged += Collection_CountChanged;
				}
				if (e.ItemRemoved)
				{
					pack.Containers.CountChanged -= Collection_CountChanged;
				}
			}
		}

		protected override void UnHookEvents()
		{
			base.UnHookEvents();
			foreach (ForwardingShipment s in Source.Shipments)
			{
				s.OuterPackLines.CountChanged -= OuterPackLines_CountChanged;
			}
		}

		protected override IEnumerable<BusinessObjectCollection> GetCollectionsToHookCountChangedEvent()
		{
			var list = new List<BusinessObjectCollection>();
			foreach (ForwardingShipment s in Source.Shipments)
			{
				list.AddRange(from PackLine pack in s.OuterPackLines where !pack.IsDeleted select (BusinessObjectCollection)pack.Containers);
			}
			return list.ToArray();
		}

		#endregion
	}
}
