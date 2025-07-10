using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class CarnetMiscellaneousOptionsControlBag : ControlBag
	{
		CarnetMiscellaneousOptionsControlBag()
		{
			BranchGuidFindBox = RegisterControl(nameof(CarnetMiscellaneousOptionsControlBag.BranchGuidFindBox));
			BrokerCodeFindBox = RegisterControl(nameof(CarnetMiscellaneousOptionsControlBag.BrokerCodeFindBox));
		}

		public static CarnetMiscellaneousOptionsControlBag Instance => instance ?? (instance = new CarnetMiscellaneousOptionsControlBag());
		[ThreadStatic]
		static CarnetMiscellaneousOptionsControlBag instance;

		public ControlReference BranchGuidFindBox { get; }
		public ControlReference BrokerCodeFindBox { get; }

		protected override Control CreateTemplate() => new CarnetMiscUserControl();
	}
}
