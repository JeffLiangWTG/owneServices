using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaPackCollectionSynchroniser : BusinessObjectCollectionSynchroniser
	{
		public AsycudaPackCollectionSynchroniser(ForwardingShipment source, AsycudaBill destination)
			: base(source, destination)
		{
		}

		protected new AsycudaBill Destination
		{
			get { return (AsycudaBill)base.Destination; }
		}

		protected new ForwardingShipment Source
		{
			get { return (ForwardingShipment)base.Source; }
		}

		protected override void HookElementSynchronisers()
		{
			if (Destination?.Header?.FeatureProvider.SupportsAsycudaPacks ?? false)
			{
				SynchronisePacks(GetSourcePacksToSynchronise());
			}
		}

		IEnumerable<PackLine> GetSourcePacksToSynchronise()
		{
			return Source.OuterPackLines.OfType<PackLine>().Where(p => p != null && !p.IsDeleted).Distinct();
		}

		protected virtual void SynchronisePacks(IEnumerable<PackLine> sourcePacks)
		{
			if (DoesSourceHaveDuplicatesContainerNumbers(sourcePacks))
			{
				Destination.Packs.RemoveAndDeleteAll();
			}
			else
			{
				DeletePacksIfNotInSource(sourcePacks);
			}

			if (!SyncChangesDetected)
			{
				var existingPacks = GetExistingPacks().Cast<AsycudaPack>().ToArray();
				foreach (var sourcePack in sourcePacks)
				{
					var asycudaPack = GetOrCreatePack(sourcePack, existingPacks);
					if (SyncChangesDetected)
					{
						return;
					}
					if (asycudaPack != null)
					{
						ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(GetAsycudaPackSynchroniser(asycudaPack, sourcePack), IsEnabled, DetectEnabled);
					}
				}
			}
		}

		IAsycudaPackCollection<AsycudaPack, AsycudaBill> GetExistingPacks() => Destination.Packs;

		protected virtual AsycudaPackSynchroniser GetAsycudaPackSynchroniser(AsycudaPack asycudaPack, PackLine sourcePack) => new AsycudaPackSynchroniser(asycudaPack, sourcePack);

		bool DoesSourceHaveDuplicatesContainerNumbers(IEnumerable<PackLine> sourcePacks)
		{
			return sourcePacks.Cast<PackLine>().GroupBy(x => x.JL_Calc_ContainerNum).Any(g => g.Take(2).Count() > 1);
		}

		void DeletePacksIfNotInSource(IEnumerable<PackLine> sourcePacks)
		{
			var collection = new List<PackLine>(sourcePacks);
			foreach (var ap in Destination.Packs.OfType<AsycudaPack>().ToArray())
			{
				if (ap.Container != null)
				{
					var sourcePack = collection.FirstOrDefault(x => x.JL_Calc_ContainerNum == ap.Container.ACN_ContainerNumber);
					if (sourcePack != null)
					{
						collection.Remove(sourcePack);
						continue;
					}
					if (DetectEnabled)
					{
						SyncChangesDetected = true;
						return;
					}
				}
				ap.Delete();
			}
		}

		AsycudaPack GetOrCreatePack(PackLine sourcePack, IEnumerable<AsycudaPack> existingPacks)
		{
			var containerNumber = sourcePack.JL_Calc_ContainerNum;
			var result = existingPacks.FirstOrDefault(ap => ap.Container != null && ap.Container.ACN_ContainerNumber == containerNumber && !ap.IsDeleted);

			if (result != null)
			{
				if (DetectEnabled)
				{
					SyncChangesDetected = true;
					return null;
				}
			}
			else
			{
				if (DetectEnabled)
				{
					SyncChangesDetected = true;
					return null;
				}
				using (Destination.Packs.SuspendSettingHasChanges())
				{
					if (Destination.Header != null)
					{
						result = Destination.Packs.AddNew();
						var asyCont = Destination.Header.Containers.OfType<AsycudaContainer>().FirstOrDefault(ac => ac.ACN_ContainerNumber == containerNumber);
						if (asyCont != null)
						{
							result.ContainerPK = asyCont.PK;
						}
					}
				}
			}
			return result;
		}

		protected override void OnDetectEnabledChanged()
		{
			if (DetectEnabled)
			{
				Destination.Packs.Load();
			}
			base.OnDetectEnabledChanged();
		}

		protected override void ForceSynchroniseCore(bool isDeleted)
		{
			CleanupStalePackSynchronisers();

			base.ForceSynchroniseCore(isDeleted);
		}

		void CleanupStalePackSynchronisers()
		{
			var existingPacks = GetExistingPacks();
			var packSynchronisers = ElementSynchronisers.Cast<AsycudaPackSynchroniser>().ToArray();
			foreach (var packSynchroniser in packSynchronisers)
			{
				if (packSynchroniser.Destination != null && !existingPacks.Contains(packSynchroniser.Destination))
				{
					ElementSynchronisers.Remove(packSynchroniser);
				}
			}
		}

		#region Hook/UnHook Events

		protected override void HookEvents()
		{
			base.HookEvents();
			foreach (PackLine pack in Source.OuterPackLines)
			{
				pack.Containers.CountChanged -= Containers_CountChanged;
				if (!pack.IsDeleted)
				{
					pack.Containers.CountChanged += Containers_CountChanged;
				}
			}
		}

		protected override void UnHookEvents()
		{
			base.UnHookEvents();
			foreach (PackLine pack in Source.OuterPackLines)
			{
				pack.Containers.CountChanged -= Containers_CountChanged;
			}
		}

		protected override IEnumerable<BusinessObjectCollection> GetCollectionsToHookCountChangedEvent()
		{
			yield return Source.OuterPackLines;
		}

		protected override void Collection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			base.Collection_CountChanged(sender, e);
			var pack = (PackLine)e.BizObject;
			if (pack != null)
			{
				if (e.ItemAdded)
				{
					pack.Containers.CountChanged -= Containers_CountChanged;
					pack.Containers.CountChanged += Containers_CountChanged;
				}
				if (e.ItemRemoved)
				{
					pack.Containers.CountChanged -= Containers_CountChanged;
				}
			}
		}

		void Containers_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			Synchronise();
		}

		#endregion
	}
}
