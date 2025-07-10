using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class SEDDetails008ControlBag : ControlBag
	{
		SEDDetails008ControlBag()
		{
			StayPeriodDropEdit = RegisterControl(nameof(SEDDetails008ControlBag.StayPeriodDropEdit));
			CustomsOfficeCodeFindBox = RegisterControl(nameof(SEDDetails008ControlBag.CustomsOfficeCodeFindBox));
			DepartmentCodeFindBox = RegisterControl(nameof(SEDDetails008ControlBag.DepartmentCodeFindBox));
			HasItemsDropEdit = RegisterControl(nameof(SEDDetails008ControlBag.HasItemsDropEdit));
			WeaponDropEdit = RegisterControl(nameof(SEDDetails008ControlBag.WeaponDropEdit));
			DrugDropEdit = RegisterControl(nameof(SEDDetails008ControlBag.DrugDropEdit));
			AnimalsDropEdit = RegisterControl(nameof(SEDDetails008ControlBag.AnimalsDropEdit));
			EndangeredItemsDropEdit = RegisterControl(nameof(SEDDetails008ControlBag.EndangeredItemsDropEdit));
			CounterfeitDropEdit = RegisterControl(nameof(SEDDetails008ControlBag.CounterfeitDropEdit));
			CommercialUseItemsDropEdit = RegisterControl(nameof(SEDDetails008ControlBag.CommercialUseItemsDropEdit));
			ExcessTimeLimitItemsDropEdit = RegisterControl(nameof(SEDDetails008ControlBag.ExcessTimeLimitItemsDropEdit));
			PornographyDropEdit = RegisterControl(nameof(SEDDetails008ControlBag.PornographyDropEdit));
			BranchCodeGuidFindBox = RegisterControl(nameof(SEDDetails008ControlBag.BranchCodeGuidFindBox));
			BrokerCodeFindBox = RegisterControl(nameof(SEDDetails008ControlBag.BrokerCodeFindBox));
			ServiceCodeFindBox = RegisterControl(nameof(SEDDetails008ControlBag.ServiceCodeFindBox));
		}

		public static SEDDetails008ControlBag Instance => instance ?? (instance = new SEDDetails008ControlBag());

		[ThreadStatic]
		static SEDDetails008ControlBag instance;

		public ControlReference StayPeriodDropEdit { get; }
		public ControlReference CustomsOfficeCodeFindBox { get; }
		public ControlReference DepartmentCodeFindBox { get; }
		public ControlReference HasItemsDropEdit { get; }
		public ControlReference WeaponDropEdit { get; }
		public ControlReference DrugDropEdit { get; }
		public ControlReference AnimalsDropEdit { get; }
		public ControlReference EndangeredItemsDropEdit { get; }
		public ControlReference CounterfeitDropEdit { get; }
		public ControlReference CommercialUseItemsDropEdit { get; }
		public ControlReference ExcessTimeLimitItemsDropEdit { get; }
		public ControlReference PornographyDropEdit { get; }
		public ControlReference BranchCodeGuidFindBox { get; }
		public ControlReference BrokerCodeFindBox { get; }
		public ControlReference ServiceCodeFindBox { get; }

		protected override Control CreateTemplate() => new SEDDetails008UserControl();
	}
}
