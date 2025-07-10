using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class ContainersAndSealsUserControl : ZUserControl
	{
		public ContainersAndSealsUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			header = dataSource as NctsHeader;
			var departureMovementHeader = header?.MovementHeader;
			if (departureMovementHeader != null)
			{
				departureMovementHeader.BM_SealTypeInfo.ValueChanged -= BM_SealTypeInfo_ValueChanged;
				departureMovementHeader.BM_SealTypeInfo.ValueChanged += BM_SealTypeInfo_ValueChanged;
				BM_SealTypeInfo_ValueChanged(null, EventArgs.Empty);
			}
		}

		protected void BM_SealTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			var isPackageSeal = header != null && header.MovementHeader.BM_SealType == SealTypeList.Codes.PackageSeal;
			SealTabControl.SelectedTab = isPackageSeal ? PackageTabPage : ContainerTabPage;
			PackageTabPage.TabVisible = isPackageSeal;
			ContainerTabPage.TabVisible = !isPackageSeal;
		}

		protected override void Dispose(bool disposing)
		{
			if (header != null)
			{
				header.MovementHeader.BM_SealTypeInfo.ValueChanged -= BM_SealTypeInfo_ValueChanged;
			}
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
		NctsHeader header;
	}
}
