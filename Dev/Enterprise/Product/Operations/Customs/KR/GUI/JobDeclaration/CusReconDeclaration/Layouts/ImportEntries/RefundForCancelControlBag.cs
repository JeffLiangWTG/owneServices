using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class RefundForCancelControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new RefundForCancelUserControl();

		[ThreadStatic]
		static RefundForCancelControlBag instance;

		public static RefundForCancelControlBag Instance => instance ?? (instance = new RefundForCancelControlBag());
		public RefundForCancelControlBag()
		{
			CancelReasonDropEdit = RegisterControl(nameof(RefundForCancelUserControl.CancelReasonDropEdit));
			DisposalNumberTextBox = RegisterControl(nameof(RefundForCancelUserControl.DisposalNumberTextBox));
			DisposalDateEdit = RegisterControl(nameof(RefundForCancelUserControl.DisposalDateEdit));
			GoodsLocationDescriptionLongTextControl = RegisterControl(nameof(RefundForCancelUserControl.GoodsLocationDescriptionLongTextControl));
			ResidualSubstanceDescriptionLongTextControl = RegisterControl(nameof(RefundForCancelUserControl.ResidualSubstanceDescriptionLongTextControl));
			DamageSituationLongTextControl = RegisterControl(nameof(RefundForCancelUserControl.DamageSituationLongTextControl));
			ExportEntryNumberTextBox = RegisterControl(nameof(RefundForCancelUserControl.ExportEntryNumberTextBox));
			ExportEntryLineNumberTextBox = RegisterControl(nameof(RefundForCancelUserControl.ExportEntryLineNumberTextBox));
		}

		public ControlReference CancelReasonDropEdit { get; set; }
		public ControlReference DisposalNumberTextBox { get; set; }
		public ControlReference DisposalDateEdit { get; set; }
		public ControlReference GoodsLocationDescriptionLongTextControl { get; set; }
		public ControlReference ResidualSubstanceDescriptionLongTextControl { get; set; }
		public ControlReference DamageSituationLongTextControl { get; set; }
		public ControlReference ExportEntryNumberTextBox { get; set; }
		public ControlReference ExportEntryLineNumberTextBox { get; set; }
	}
}
