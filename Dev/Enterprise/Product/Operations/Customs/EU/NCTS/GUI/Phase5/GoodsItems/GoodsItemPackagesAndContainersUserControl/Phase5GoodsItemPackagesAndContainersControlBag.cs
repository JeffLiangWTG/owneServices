using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class Phase5GoodsItemPackagesAndContainersControlBag : ControlBag
	{
		public Phase5GoodsItemPackagesAndContainersControlBag()
		{
			DynamicPackagesUserControl = RegisterControl(nameof(Phase5GoodsItemPackagesAndContainersUserControl.DynamicPackagesUserControl));
			ContainersUserControl = RegisterControl(nameof(Phase5GoodsItemPackagesAndContainersUserControl.ContainersUserControl));
		}

		public static Phase5GoodsItemPackagesAndContainersControlBag Instance => instance ?? (instance = new Phase5GoodsItemPackagesAndContainersControlBag());

		[ThreadStatic]
		static Phase5GoodsItemPackagesAndContainersControlBag instance;

		public ControlReference DynamicPackagesUserControl { get; }

		public ControlReference ContainersUserControl { get; }

		protected override Control CreateTemplate() => new Phase5GoodsItemPackagesAndContainersUserControl();
	}
}
