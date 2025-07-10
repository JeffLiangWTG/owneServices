using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class JPAFRContainerCollectionSynchroniser : Customs.Business.BusinessObjectCollectionSynchroniser
	{
		public JPAFRContainerCollectionSynchroniser(ForwardingShipment source, JPAFRBills destination, ForwardingConsol consolSource)
			: base(source, destination)
		{
			this.consolSource = consolSource;
		}
		readonly ForwardingConsol consolSource;

		public new ForwardingShipment Source
		{
			get { return (ForwardingShipment)base.Source; }
		}

		protected new JPAFRBills Destination
		{
			get { return (JPAFRBills)base.Destination; }
		}

		#region Synchronise

		protected override void ForceSynchroniseCore(bool isDeleted)
		{
			base.ForceSynchroniseCore(isDeleted);
			if (!isDeleted && Destination.ShouldSynchronise)
			{
				DeleteOrAddJPAFRContainers();
			}
		}

		protected override void HookElementSynchronisers()
		{
			if (!Destination.IsDeleted && Destination.ShouldSynchronise)
			{
				HookSynchronisationForExistingContainers();
			}
		}

		IEnumerable<ForwardingContainer> GetSourceContainersToSynchronise()
		{
			var containers = Source.ContainersOnConsol(consolSource).ToArray<ForwardingContainer>();

			return from ForwardingContainer container in containers
				   where container != null && !container.IsDeleted && container.JC_ContainerMode != Core.Constants.ContainerModes.BreakBulk
				   select container;
		}

		void HookSynchronisationForExistingContainers()
		{
			var containerList = new List<JPAFRContainer>(Destination.Containers);
			foreach (var sourceContainer in GetSourceContainersToSynchronise())
			{
				var synchroniser = ElementSynchronisers.FindMatchingSource<JPAFRContainerSynchroniser>(sourceContainer);
				if (synchroniser != null)
				{
					synchroniser.SetEnabled(IsEnabled, DetectEnabled);
					containerList.Remove(synchroniser.Destination);
				}
				else
				{
					var containerNumber = sourceContainer.JC_ContainerNum;
					JPAFRContainer container = null;
					while ((container = containerList.FirstOrDefault(x => !x.IsDeleted && x.JPC_ContainerNum == containerNumber)) != null)
					{
						synchroniser = ElementSynchronisers.FindMatchingDestination<JPAFRContainerSynchroniser>(container);
						if (synchroniser == null)
						{
							ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(GetContainerSynchroniser(container, sourceContainer), IsEnabled, DetectEnabled);
							containerList.Remove(container);
							break;
						}
						else
						{
							synchroniser.SetEnabled(IsEnabled, DetectEnabled);
							containerList.Remove(container);
						}
					}
				}
			}
		}

		void DeleteOrAddJPAFRContainers()
		{
			var containersList = new List<ForwardingContainer>(GetSourceContainersToSynchronise());
			var billContainers = new List<JPAFRContainer>(Destination.Containers);
			if (containersList.Count > 0)
			{
				while (billContainers.Count > 0)
				{
					var billContainer = billContainers[0];
					billContainers.Remove(billContainer);
					if (billContainer.IsDeleted)
					{
						var synchroniser = ElementSynchronisers.FindMatchingDestination<JPAFRContainerSynchroniser>(billContainer);
						if (synchroniser != null)
						{
							ElementSynchronisers.Remove(synchroniser);
						}
					}
					else
					{
						var synchroniser = ElementSynchronisers.FindMatchingDestination<JPAFRContainerSynchroniser>(billContainer);
						if (synchroniser != null)
						{
							if (containersList.Contains(synchroniser.Source))
							{
								containersList.Remove(synchroniser.Source);
								synchroniser.Synchronise();
								continue;
							}
						}
						else
						{
							billContainer = FindMatchingContainerAndAddSynchroniser(containersList, billContainers, billContainer);
						}

						if (billContainer != null)
						{
							billContainer.Delete();
						}
					}
				}

				foreach (var container in containersList)
				{
					var billContainer = Destination.Containers.AddNew();
					var synchroniser = GetContainerSynchroniser(billContainer, container);
					ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
					synchroniser.Synchronise();
				}
			}
			else
			{
				billContainers.ForEach(billContainer =>
					{
						billContainer.Delete();
					});
			}
		}

		JPAFRContainerSynchroniser GetContainerSynchroniser(JPAFRContainer billContainer, ForwardingContainer container)
		{
			return new JPAFRContainerSynchroniser(billContainer, container);
		}

		JPAFRContainer FindMatchingContainerAndAddSynchroniser(List<ForwardingContainer> containers, List<JPAFRContainer> billContainers, JPAFRContainer billContainer)
		{
			ForwardingContainer existingContainer = null;
			var alreadyProcessedContainers = new List<ForwardingContainer>();
			while ((existingContainer = containers.FirstOrDefault(container => !alreadyProcessedContainers.Contains(container) && container.JC_ContainerNum == billContainer.JPC_ContainerNum)) != null)
			{
				alreadyProcessedContainers.Add(existingContainer);
				var synchroniser = ElementSynchronisers.FindMatchingSource<JPAFRContainerSynchroniser>(existingContainer);
				if (synchroniser != null)
				{
					containers.Remove(existingContainer);
					billContainers.Remove(synchroniser.Destination);
					synchroniser.Synchronise();
					break;
				}
				else
				{
					synchroniser = GetContainerSynchroniser(billContainer, existingContainer);
					ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
					synchroniser.Synchronise();
					containers.Remove(existingContainer);
					billContainer = null;
					break;
				}
			}
			return billContainer;
		}

		#endregion

		#region Hook/UnHook Events

		protected override void HookEvents()
		{
			base.HookEvents();
			foreach (var containers in from PackLine pack in Source.OuterPackLines
									   where !pack.IsDeleted
									   select (BusinessObjectCollection)pack.Containers)
			{
				foreach (CommonContainer container in containers)
				{
					HookContainerModeChangeEvent(container);
				}
			}
		}

		protected override void UnHookEvents()
		{
			foreach (var containers in from PackLine pack in Source.OuterPackLines
									   where !pack.IsDeleted
									   select (BusinessObjectCollection)pack.Containers)
			{
				foreach (CommonContainer container in containers)
				{
					UnHookContainerModeChangeEvent(container);
				}
			}
			base.UnHookEvents();
		}

		protected override IEnumerable<BusinessObjectCollection> GetCollectionsToHookCountChangedEvent()
		{
			return new[] { Source.OuterPackLines }.Concat((from PackLine pack in Source.OuterPackLines where !pack.IsDeleted select (BusinessObjectCollection)pack.Containers));
		}

		protected override void Collection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				var packLine = e.BizObject as PackLine;
				if (packLine != null)
				{
					var containers = packLine.Containers;
					containers.CountChanged -= Collection_CountChanged;
					containers.CountChanged += Collection_CountChanged;

					if (!CollectionsToHookAndUnHookCountChangedEvent.Contains(containers))
					{
						CollectionsToHookAndUnHookCountChangedEvent.Add(containers);
					}
					foreach (CommonContainer container in containers)
					{
						HookContainerModeChangeEvent(container);
					}
				}
				else
				{
					var container = e.BizObject as CommonContainer;
					if (container != null)
					{
						HookContainerModeChangeEvent(container);
					}
				}
			}
			else
			{
				var packLine = e.BizObject as PackLine;
				if (packLine != null)
				{
					var containers = packLine.Containers;
					containers.CountChanged -= Collection_CountChanged;
					CollectionsToHookAndUnHookCountChangedEvent.Remove(containers);
					foreach (var container in containers)
					{
						ElementSynchronisers.Remove(container);
					}
					foreach (CommonContainer container in containers)
					{
						UnHookContainerModeChangeEvent(container);
					}
				}
				else
				{
					var container = e.BizObject as CommonContainer;
					if (container != null)
					{
						UnHookContainerModeChangeEvent(container);
					}
				}
			}
			base.Collection_CountChanged(sender, e);
		}

		void UnHookContainerModeChangeEvent(CommonContainer container)
		{
			container.JC_ContainerModeInfo.ValueChanged -= JC_ContainerModeInfo_ValueChanged;
		}

		void HookContainerModeChangeEvent(CommonContainer container)
		{
			container.JC_ContainerModeInfo.ValueChanged -= JC_ContainerModeInfo_ValueChanged;
			container.JC_ContainerModeInfo.ValueChanged += JC_ContainerModeInfo_ValueChanged;
		}

		void JC_ContainerModeInfo_ValueChanged(object sender, EventArgs e)
		{
			Synchronise();
		}

		#endregion
	}
}
