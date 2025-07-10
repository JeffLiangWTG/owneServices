
namespace Enterprise.Customs.CA.Business
{
	using Enterprise.Customs.Business;
	using Enterprise.MasterFiles.Business;

	public class UNDGSynchroniser : BusinessObjectSynchroniser
	{
		public UNDGSynchroniser(UNDGDataItem destination, UNDGDataItem source)
			: base(destination, source)
		{
		}

		public new UNDGDataItem Source
		{
			get { return (UNDGDataItem)base.Source; }
		}

		public new UNDGDataItem Destination
		{
			get { return (UNDGDataItem)base.Destination; }
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			if (Source != null && !Source.IsDeleted && Destination != null && !Destination.IsDeleted)
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.DI_DGInfo, Source.DI_DGInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.DI_DGFlashPointInfo, Source.DI_DGFlashPointInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.DI_OC_DGContactInfo, Source.DI_OC_DGContactInfo));
			}
		}
	}
}
