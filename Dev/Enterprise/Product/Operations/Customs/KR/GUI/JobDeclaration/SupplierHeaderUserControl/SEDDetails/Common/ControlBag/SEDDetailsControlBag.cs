using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class SEDDetailsControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new SEDDetailsUserControl();

		[ThreadStatic]
		static SEDDetailsControlBag instance;

		public static SEDDetailsControlBag Instance => instance ?? (instance = new SEDDetailsControlBag());

		SEDDetailsControlBag()
		{
			GoodsOriginCodeFindBox = RegisterControl(nameof(SEDDetailsUserControl.GoodsOriginCodeFindBox));
			COODeterminationRuleDropEdit = RegisterControl(nameof(SEDDetailsUserControl.COODeterminationRuleDropEdit));
			COOIssueStatusDropEdit = RegisterControl(nameof(SEDDetailsUserControl.COOIssueStatusDropEdit));
			COOLabelLocationDropEdit = RegisterControl(nameof(SEDDetailsUserControl.COOLabelLocationDropEdit));
			ManufacturerIPCCodeFindBox = RegisterControl(nameof(SEDDetailsUserControl.ManufacturerIPCCodeFindBox));
			ManufacturerUnipassIDTextBox = RegisterControl(nameof(SEDDetailsUserControl.ManufacturerUnipassIDTextBox));
			ManufacturerAddressControl = RegisterControl(nameof(SEDDetailsUserControl.ManufacturerAddressControl));
			ManufacturerGuidFindBox = RegisterControl(nameof(SEDDetailsUserControl.ManufacturerGuidFindBox));
			BuyerIDTextBox = RegisterControl(nameof(SEDDetailsUserControl.BuyerIDTextBox));
			BuyerGuidFindBox = RegisterControl(nameof(SEDDetailsUserControl.BuyerGuidFindBox));
			SupplierGuidFindBox = RegisterControl(nameof(SEDDetailsUserControl.SupplierGuidFindBox));
			SupplierUnipassIDTextBox = RegisterControl(nameof(SEDDetailsUserControl.SupplierUnipassIDTextBox));
		}

		public ControlReference GoodsOriginCodeFindBox { get; }
		public ControlReference COODeterminationRuleDropEdit { get; }
		public ControlReference COOIssueStatusDropEdit { get; }
		public ControlReference COOLabelLocationDropEdit { get; }
		public ControlReference ManufacturerIPCCodeFindBox { get; }
		public ControlReference ManufacturerUnipassIDTextBox { get; }
		public ControlReference ManufacturerAddressControl { get; }
		public ControlReference ManufacturerGuidFindBox { get; }
		public ControlReference BuyerIDTextBox { get; }
		public ControlReference BuyerGuidFindBox { get; }
		public ControlReference SupplierGuidFindBox { get; }
		public ControlReference SupplierUnipassIDTextBox { get; }
	}
}
