using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class CarnetCustomsAreaDetailsControlBag : ControlBag
	{
		CarnetCustomsAreaDetailsControlBag()
		{
			CustomsOfficeCodeFindBox = RegisterControl(nameof(CarnetCustomsAreaDetailsControlBag.CustomsOfficeCodeFindBox));
			DepartmentCodeFindBox = RegisterControl(nameof(CarnetCustomsAreaDetailsControlBag.DepartmentCodeFindBox));
			BondedAreaCodeFindBox = RegisterControl(nameof(CarnetCustomsAreaDetailsControlBag.BondedAreaCodeFindBox));
		}

		public static CarnetCustomsAreaDetailsControlBag Instance => instance ?? (instance = new CarnetCustomsAreaDetailsControlBag());
		[ThreadStatic]
		static CarnetCustomsAreaDetailsControlBag instance;

		public ControlReference CustomsOfficeCodeFindBox { get; }
		public ControlReference DepartmentCodeFindBox { get; }
		public ControlReference BondedAreaCodeFindBox { get; }

		protected override Control CreateTemplate() => new CarnetCustomsAreaDetailsUserControl();
	}
}
