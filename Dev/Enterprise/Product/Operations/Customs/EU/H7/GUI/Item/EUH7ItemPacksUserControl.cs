using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public partial class EUH7ItemPacksUserControl : ZUserControl, IAdditionalTabPage
	{
		public EUH7ItemPacksUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			manifestHeader = dataSource as AsycudaManifestHeader;

			if (manifestHeader != null)
			{
				dataMember = "Bills.PackedItems";
			}
			else
			{
				var bill = dataSource as AsycudaBill;
				if (bill != null)
				{
					manifestHeader = bill.Header;
					dataMember = "PackedItems";
				}
			}
			base.SetDataBinding(dataSource, dataMember);
		}
		AsycudaManifestHeader manifestHeader;

		public ZUserControl AdditionalTabPageUserControl => this;

		public ResourceStringData AdditionalTabPageCaption => Res.GetData("5ea06eb1-9dad-4169-9130-3d95ea13fc62", "Packs");

		public AdditionalTabPageVisibility AdditionalControlVisibility => new AdditionalTabPageVisibility(h => true, null);

		public int TabPageSequence => 0;
	}
}
