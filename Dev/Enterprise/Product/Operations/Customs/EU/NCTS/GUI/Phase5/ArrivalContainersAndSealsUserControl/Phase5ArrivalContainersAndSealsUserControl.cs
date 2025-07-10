using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class Phase5ArrivalContainersAndSealsUserControl : ZUserControl
	{
		public Phase5ArrivalContainersAndSealsUserControl()
		{
			InitializeComponent();
		}

		protected new NctsHeader DataSource => base.DataSource as NctsHeader;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			var provider = NctsPhase5LayoutProvider.GetLayoutProvider(DataSource.DefaultDataGroupingCode);
			var containersEquipmentGridType = provider.ArrivalContainersEquipmentGrid;
			var sealsGridType = provider.ArrivalSealsGrid;
			SetContainerEquipmentGrid();
			SetAdditionalSealsGrid();

			void SetContainerEquipmentGrid()
			{
				var userControl = (ZUserControl)Activator.CreateInstance(containersEquipmentGridType);
				ContainersEquipmentAndSealsSplitContainer.Panel1.Controls.Add(userControl);
				BindingSource.SetBindingMember(userControl, "ArrivalHeaderContainers");
				userControl.Dock = System.Windows.Forms.DockStyle.Fill;
			}

			void SetAdditionalSealsGrid()
			{
				var userControl = (ZUserControl)Activator.CreateInstance(sealsGridType);
				ContainersEquipmentAndSealsSplitContainer.Panel2.Controls.Add(userControl);
				BindingSource.SetBindingMember(userControl, "ArrivalHeaderContainers.Seals");
				userControl.Dock = System.Windows.Forms.DockStyle.Fill;
			}
		}
	}
}
