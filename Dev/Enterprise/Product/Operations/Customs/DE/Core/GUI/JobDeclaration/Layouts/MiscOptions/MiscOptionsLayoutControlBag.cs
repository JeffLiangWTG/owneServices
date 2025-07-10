using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public sealed class MiscOptionsLayoutControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new MiscOptionsLayoutUserControl();

		public static MiscOptionsLayoutControlBag Instance => instance ?? (instance = new MiscOptionsLayoutControlBag());

		[ThreadStatic]
		static MiscOptionsLayoutControlBag instance;

		MiscOptionsLayoutControlBag()
		{
			VATAccountNumberDropEdit = RegisterControl(nameof(MiscOptionsLayoutUserControl.VATAccountNumberDropEdit));
			VatPaymentPartyDropEdit = RegisterControl(nameof(MiscOptionsLayoutUserControl.VatPaymentPartyDropEdit));
			DutyAccountNumberDropEdit = RegisterControl(nameof(MiscOptionsLayoutUserControl.DutyAccountNumberDropEdit));
			VATClaimBackDropEdit = RegisterControl(nameof(MiscOptionsLayoutUserControl.VATClaimBackDropEdit));
			StatisticStatusDropEdit = RegisterControl(nameof(MiscOptionsLayoutUserControl.StatisticStatusDropEdit));
		}

		public ControlReference VATAccountNumberDropEdit;
		public ControlReference VatPaymentPartyDropEdit;
		public ControlReference DutyAccountNumberDropEdit;
		public ControlReference VATClaimBackDropEdit;
		public ControlReference StatisticStatusDropEdit;
	}
}
