
namespace Enterprise.Customs.CA.Business
{
	using CargoWise.EntityFramework;
	using Enterprise.Customs.Business;
	using Enterprise.Freight.Business;
	using Enterprise.MasterFiles.Business;

	public class UNDGCollectionSynchroniser : GenericCollectionSynchroniser<UNDGSynchroniser>
	{
		public UNDGCollectionSynchroniser(PackLine source, IUNDGDataItemProvider destination)
			: base(source, (BusinessObject)destination, source.UNDGs, destination.UNDGs)
		{
		}

		protected override bool CompareBizosEqual(BusinessObject source, BusinessObject destination)
		{
			var undgSource = (UNDGDataItem)source;
			var undgDestination = (UNDGDataItem)destination;
			return undgSource.DI_DG == undgDestination.DI_DG &&
							undgSource.DI_DGFlashPoint == undgDestination.DI_DGFlashPoint &&
							undgSource.DI_OC_DGContact == undgDestination.DI_OC_DGContact;
		}

		protected override UNDGSynchroniser CreateNewSynchroniser(BusinessObject destination, BusinessObject source)
		{
			return new UNDGSynchroniser((UNDGDataItem)destination, (UNDGDataItem)source);
		}

		protected override void HookEvents()
		{
			base.HookEvents();
			((PackLine)Source).UNDGs.CountChanged -= ActiviveCollection_CountChanged;
			((PackLine)Source).UNDGs.CountChanged += ActiviveCollection_CountChanged;
		}

		protected override void UnHookEvents()
		{
			base.UnHookEvents();
			((PackLine)Source).UNDGs.CountChanged -= ActiviveCollection_CountChanged;
		}

		protected void ActiviveCollection_CountChanged(object sender, System.EventArgs e)
		{
			Synchronise();
		}
	}
}
