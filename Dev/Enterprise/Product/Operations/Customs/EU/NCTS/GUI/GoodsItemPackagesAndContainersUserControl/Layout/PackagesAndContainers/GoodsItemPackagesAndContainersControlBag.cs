using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class GoodsItemPackagesAndContainersControlBag : ControlBag
	{
		public GoodsItemPackagesAndContainersControlBag()
		{
			PackagesUserControl = RegisterControl(nameof(GoodsItemPackagesAndContainersUserControl.PackagesUserControl));
			ContainersUserControl = RegisterControl(nameof(GoodsItemPackagesAndContainersUserControl.ContainersUserControl));
		}

		public static GoodsItemPackagesAndContainersControlBag Instance => instance ?? (instance = new GoodsItemPackagesAndContainersControlBag());

		[ThreadStatic]
		static GoodsItemPackagesAndContainersControlBag instance;

		public ControlReference PackagesUserControl { get; }

		public ControlReference ContainersUserControl { get; }

		protected override Control CreateTemplate() => new GoodsItemPackagesAndContainersUserControl();
	}
}
