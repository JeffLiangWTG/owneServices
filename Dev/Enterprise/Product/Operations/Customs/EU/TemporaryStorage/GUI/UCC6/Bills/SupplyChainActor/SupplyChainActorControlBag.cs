using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public class SupplyChainActorControlBag : ControlBag
	{
		SupplyChainActorControlBag()
		{
			RoleDropEdit = RegisterControl(nameof(SupplyChainActorUserControl.RoleDropEdit));
			OwnerOrganisationFindBox = RegisterControl(nameof(SupplyChainActorUserControl.OwnerOrganisationFindBox));
			ReferenceNumberTextBox = RegisterControl(nameof(SupplyChainActorUserControl.ReferenceNumberTextBox));
		}
		public static SupplyChainActorControlBag Instance => instance ?? (instance = new SupplyChainActorControlBag());

		[ThreadStatic]
		static SupplyChainActorControlBag instance;

		protected override Control CreateTemplate() => new SupplyChainActorUserControl();

		public ControlReference RoleDropEdit;
		public ControlReference OwnerOrganisationFindBox;
		public ControlReference ReferenceNumberTextBox;
	}
}
