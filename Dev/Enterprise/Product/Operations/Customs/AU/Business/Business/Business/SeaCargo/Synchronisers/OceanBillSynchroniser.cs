using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class OceanBillSynchroniser : BusinessObjectSynchroniser
	{
		public OceanBillSynchroniser(CusSCAOceanBill destination, CommonConsol source)
			: base(destination, source)
		{
		}

		public new CusSCAOceanBill Destination
		{
			get { return (CusSCAOceanBill)base.Destination; }
		}

		public new ForwardingConsol Source
		{
			get { return (ForwardingConsol)base.Source; }
		}

		#region Implementation

		protected override void OnSynchronise(SynchroniseEventArgs e)
		{
			if (e.Action == SynchroniseAction.Force)
			{
				EnsureSCAContainers();
			}
			base.OnSynchronise(e);
			foreach (ContainerSynchroniser synch in ContainerSynchronisers)
			{
				synch.Synchronise(e);
			}
		}

		protected override void HookSynchronisers()
		{
			FieldSynchroniser oceanBillNumberSynchroniser = new FieldSynchroniser(Destination.CB_OceanBillInfo, Source.JK_MasterBillNumInfo, true);
			oceanBillNumberSynchroniser.Format += OceanBillNumberSynchroniser_Format;
			Synchronisers.Add(oceanBillNumberSynchroniser);
			Synchronisers.Add(new FieldSynchroniser(Destination.CB_VesselNameInfo, Source.JK_VesselOfLastImportTransportInfo, true));
			Synchronisers.Add(new FieldSynchroniser(Destination.CB_VoyageInfo, Source.JK_VoyageOfLastImportTransportInfo, true));
			Synchronisers.Add(new FieldSynchroniser(Destination.CB_RL_NKPortOfLoadingInfo, Source.JK_RL_NKLoadForFirstImportTransportInfo, true));
			Synchronisers.Add(new FieldSynchroniser(Destination.CB_RL_NKPortOfDischargeInfo, Source.JK_RL_NKDiscForLastImportTransportInfo, true));
			Synchronisers.Add(new FieldSynchroniser(Destination.CB_OH_ShippingLineInfo, () => Source.ShippingLinePK, () => new[] { Source.JK_OA_ShippingLineAddressInfo }, true));
			Synchronisers.Add(new FieldSynchroniser(Destination.CB_DateOfDepartureInfo, Source.JK_JX_JA_E_DEPInfo, true));
			Synchronisers.Add(new FieldSynchroniser(Destination.CB_DateOfArrivalInfo, Source.JK_JX_JB_E_ARVInfo, true));
			HookCollectionSynchronisers();
		}

		void HookCollectionSynchronisers()
		{
			Source.Containers.CountChanged += Containers_CountChanged;
			LoadContainerSynchronisers();
		}

		void LoadContainerSynchronisers()
		{
			foreach (CommonContainer container in Source.Containers)
			{
				if (!ContainerSynchroniserExists(container))
				{
					CusSCAContainer sCAContainer = Destination.Containers.Find(container.JC_ContainerNum);
					if (sCAContainer != null && ShouldSynchroniseContainer(sCAContainer))
					{
						ContainerSynchroniser synch = GetNewContainerSynchroniser(sCAContainer, container, Source);
						if (!sCAContainer.IsInDatabase)
						{
							synch.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
						}
						else
						{
							synch.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
						}
						ContainerSynchronisers.Add(synch);
					}
				}
			}
		}

		bool ShouldSynchroniseContainer(CusSCAContainer container)
		{
			bool result = true;
			foreach (CusSCAPivot pivot in container.Pivots)
			{
				if (pivot.HouseBill != null && !CMRStatusHelper.CanDelete(pivot.HouseBill.CA_MessageStatus))
				{
					result = false;
					break;
				}
			}
			return result;
		}

		void EnsureSCAContainers()
		{
			foreach (CommonContainer container in Source.Containers)
			{
				CusSCAContainer sCAContainer = Destination.Containers.Find(container.JC_ContainerNum);
				if (sCAContainer == null)
				{
					sCAContainer = Destination.Containers.AddNew();
					sCAContainer.CN_ContainerNumber = container.JC_ContainerNum;
				}
			}
			LoadContainerSynchronisers();
		}

		void UnHookCollectionSynchronisers()
		{
			Source.Containers.CountChanged -= Containers_CountChanged;
			foreach (ContainerSynchroniser synchroniser in ContainerSynchronisers)
			{
				synchroniser.SetEnabled(false, synchroniser.DetectEnabled);
			}
		}

		protected override void UnHookSynchronisers()
		{
			base.UnHookSynchronisers();
			UnHookCollectionSynchronisers();
		}

		#endregion

		ArrayList fContainerSynchronisers;
		protected ArrayList ContainerSynchronisers
		{
			get
			{
				if (fContainerSynchronisers == null)
				{
					fContainerSynchronisers = new ArrayList();
				}
				return fContainerSynchronisers;
			}
		}

		ContainerSynchroniser FindContainerSynchroniser(CommonContainer containerToFind)
		{
			ContainerSynchroniser result = null;
			for (int i = ContainerSynchronisers.Count - 1; i >= 0; i--)
			{
				ContainerSynchroniser synchroniser = (ContainerSynchroniser)ContainerSynchronisers[i];
				if (synchroniser.Source == containerToFind)
				{
					result = synchroniser;
				}
			}
			return result;
		}

		void RemoveContainerSynchroniser(CommonContainer containerToRemove)
		{
			var synchroniserToRemove = FindContainerSynchroniser(containerToRemove);
			if (synchroniserToRemove != null)
			{
				synchroniserToRemove.SetEnabled(false, synchroniserToRemove.DetectEnabled);
				if (Destination.Containers.Contains(synchroniserToRemove.Destination.PK))
				{
					Destination.Containers.RemoveAndDelete(synchroniserToRemove.Destination);
				}
				else
				{
					synchroniserToRemove.Destination.Delete();
				}
				ContainerSynchronisers.Remove(synchroniserToRemove);
			}
		}

		bool ContainerSynchroniserExists(CommonContainer containerToCheck)
		{
			return FindContainerSynchroniser(containerToCheck) != null;
		}

		protected virtual ContainerSynchroniser GetNewContainerSynchroniser(CusSCAContainer sCAContainer, CommonContainer jobContainer, ForwardingConsol parent)
		{
			return new ContainerSynchroniser(sCAContainer, jobContainer, parent);
		}

		void Containers_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (!DestinationDeletableBusinessObjectSynchroniser.IsSenderRefreshingByDataRefreshBus(sender))
			{
				if (e.ItemAdded)
				{
					CommonContainer addedContainer = e.BizObject as CommonContainer;
					if (addedContainer != null && !ContainerSynchroniserExists(addedContainer))
					{
						CusSCAContainer addedSCAContainer = Destination.Containers.AddNew();
						ContainerSynchroniser addedContainerSynchoniser = GetNewContainerSynchroniser(addedSCAContainer, addedContainer, this.Source);
						addedContainerSynchoniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
						ContainerSynchronisers.Add(addedContainerSynchoniser);
					}
				}
				else if (e.ItemRemoved)
				{
					CommonContainer deletedContainer = e.BizObject as CommonContainer;
					if (deletedContainer != null)
					{
						RemoveContainerSynchroniser(deletedContainer);
					}
				}
			}
		}

		void OceanBillNumberSynchroniser_Format(object sender, FieldSynchroniser.ConvertEventArgs e)
		{
			if (e.Value is ZString)
			{
				e.Value = new ZString(e.Value).ToUpper();
			}
		}
	}
}
