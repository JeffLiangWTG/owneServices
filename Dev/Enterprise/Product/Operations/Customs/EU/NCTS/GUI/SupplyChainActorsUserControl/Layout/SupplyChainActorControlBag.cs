using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class SupplyChainActorControlBag : ControlBag
	{
		public SupplyChainActorControlBag()
		{
			RoleDropEdit = RegisterControl(nameof(SupplyChainActorUserControl.RoleDropEdit));
			ReferenceTextBox = RegisterControl(nameof(SupplyChainActorUserControl.ReferenceTextBox));
			OwnerOrganisationFindBox = RegisterControl(nameof(SupplyChainActorUserControl.OwnerOrganisationFindBox));
		}

		public static SupplyChainActorControlBag Instance => instance ?? (instance = new SupplyChainActorControlBag());

		[ThreadStatic]
		static SupplyChainActorControlBag instance;

		public ControlReference RoleDropEdit { get; }

		public ControlReference ReferenceTextBox { get; }

		public ControlReference OwnerOrganisationFindBox { get; }

		protected override Control CreateTemplate() => new SupplyChainActorUserControl();
	}
}
