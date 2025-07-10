using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public partial class EUH7ItemTabControl : ZUserControl
	{
		public EUH7ItemTabControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			AsycudaManifestHeader manifestHeader = null;

			if (dataSource is AsycudaManifestHeader header)
			{
				manifestHeader = header;
			}
			else if (dataSource is AsycudaBill bill)
			{
				manifestHeader = bill.Header;
			}

			AddItemAdditionalTabPage(manifestHeader);
			base.SetDataBinding(dataSource, dataMember);
		}

		UserControlGenerateHelper additionalPackLevelUserControlsHelper;

		void AddItemAdditionalTabPage(AsycudaManifestHeader manifestHeader)
		{
			if (manifestHeader != null && additionalPackLevelUserControlsHelper == null)
			{
				var provider = (H7ApplicationGUIProvider)ApplicationGUIProvider.GetApplicationGuiProvider(manifestHeader);
				additionalPackLevelUserControlsHelper = new UserControlGenerateHelper(manifestHeader, provider.GetItemAdditionalTabPageUserControl());
				additionalPackLevelUserControlsHelper.AddAdditionalTabPages(ItemDetailsTabControl, this.BindingSource, "", () => new EUH7ItemAdditionalTabPageUserControl(), 0);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				additionalPackLevelUserControlsHelper?.DisposeTabPages();
			}
			base.Dispose(disposing);
		}
	}
}
