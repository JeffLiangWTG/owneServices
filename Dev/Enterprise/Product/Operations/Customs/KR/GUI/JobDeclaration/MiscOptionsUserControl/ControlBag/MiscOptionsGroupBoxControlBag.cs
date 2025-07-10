using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class MiscOptionsGroupBoxControlBag : ControlBag
	{
		public static MiscOptionsGroupBoxControlBag Instance => instance ?? (instance = new MiscOptionsGroupBoxControlBag());

		[ThreadStatic]
		static MiscOptionsGroupBoxControlBag instance;

		protected override Control CreateTemplate() => new MiscOptionsGroupBoxUserControl();

		MiscOptionsGroupBoxControlBag()
		{
			MiscellaneousGroupBox = RegisterControl(nameof(MiscOptionsGroupBoxUserControl.MiscellaneousGroupBox));
			ReturnGroupBox = RegisterControl(nameof(MiscOptionsGroupBoxUserControl.ReturnGroupBox));
			SouthNorthTradeGroupBox = RegisterControl(nameof(MiscOptionsGroupBoxUserControl.SouthNorthTradeGroupBox));
			AdditionalCargoGroupBox = RegisterControl(nameof(MiscOptionsGroupBoxUserControl.AdditionalCargoGroupBox));
			PenaltyDeclarationGroupBox = RegisterControl(nameof(MiscOptionsGroupBoxUserControl.PenaltyDeclarationGroupBox));
			RefundRequestGroupBox = RegisterControl(nameof(MiscOptionsGroupBoxUserControl.RefundRequestGroupBox));
		}

		public ControlReference MiscellaneousGroupBox { get; }
		public ControlReference ReturnGroupBox { get; }
		public ControlReference SouthNorthTradeGroupBox { get; }
		public ControlReference AdditionalCargoGroupBox { get; }
		public ControlReference PenaltyDeclarationGroupBox { get; }
		public ControlReference RefundRequestGroupBox { get; }
	}
}
