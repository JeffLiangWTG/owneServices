using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class CusMiscRequestHeaderViewControlBag : ControlBag
	{
		CusMiscRequestHeaderViewControlBag()
		{
			MessageTypeDropEdit = RegisterControl(nameof(CusMiscRequestHeaderViewControl.MessageTypeDropEdit));
			CustomsOfficeCodeFindBox = RegisterControl(nameof(CusMiscRequestHeaderViewControl.CustomsOfficeCodeFindBox));
			CustomsDivisionCodeFindBox = RegisterControl(nameof(CusMiscRequestHeaderViewControl.CustomsDivisionCodeFindBox));
			BranchGuidFindBox = RegisterControl(nameof(CusMiscRequestHeaderViewControl.BranchGuidFindBox));
			RequestDetailsTextBox = RegisterControl(nameof(CusMiscRequestHeaderViewControl.RequestDetailsTextBox));

			ApplicationNumberTextBox = RegisterControl(nameof(CusMiscRequestHeaderViewControl.ApplicationNumberTextBox));
			StatusDropEdit = RegisterControl(nameof(CusMiscRequestHeaderViewControl.StatusDropEdit));
			CustomsReviewStatusDropEdit = RegisterControl(nameof(CusMiscRequestHeaderViewControl.CustomsReviewStatusDropEdit));
			RequestDateEdit = RegisterControl(nameof(CusMiscRequestHeaderViewControl.RequestDateEdit));
			ReviewDateEdit = RegisterControl(nameof(CusMiscRequestHeaderViewControl.ReviewDateEdit));
			EntryCountCalcEdit = RegisterControl(nameof(CusMiscRequestHeaderViewControl.EntryCountCalcEdit));
		}

		public static CusMiscRequestHeaderViewControlBag Instance => instance ?? (instance = new CusMiscRequestHeaderViewControlBag());

		[ThreadStatic]
		static CusMiscRequestHeaderViewControlBag instance;

		protected override Control CreateTemplate() => new CusMiscRequestHeaderViewControl();

		public ControlReference MessageTypeDropEdit { get; }
		public ControlReference CustomsOfficeCodeFindBox { get; }
		public ControlReference CustomsDivisionCodeFindBox { get; }
		public ControlReference BranchGuidFindBox { get; }
		public ControlReference RequestDetailsTextBox { get; }
		public ControlReference ApplicationNumberTextBox { get; }
		public ControlReference StatusDropEdit { get; }
		public ControlReference CustomsReviewStatusDropEdit { get; }
		public ControlReference RequestDateEdit { get; }
		public ControlReference ReviewDateEdit { get; }
		public ControlReference EntryCountCalcEdit { get; }
	}
}
