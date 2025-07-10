using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class FinalPriceExtensionRequestNewControlBag : ControlBag
	{
		FinalPriceExtensionRequestNewControlBag()
		{
			MessageTypeDropEdit = RegisterControl(nameof(FinalPriceExtensionRequestNewControlBag.Instance.MessageTypeDropEdit));
			CustomsOfficeCodeFindBox = RegisterControl(nameof(FinalPriceExtensionRequestNewControlBag.Instance.CustomsOfficeCodeFindBox));
			BranchGuidFindBox = RegisterControl(nameof(FinalPriceExtensionRequestNewControlBag.Instance.BranchGuidFindBox));
		}

		public static FinalPriceExtensionRequestNewControlBag Instance => instance ?? (instance = new FinalPriceExtensionRequestNewControlBag());

		[ThreadStatic]
		static FinalPriceExtensionRequestNewControlBag instance;

		protected override Control CreateTemplate() => new FinalPriceExtensionRequestNewControl();

		public ControlReference MessageTypeDropEdit { get; }
		public ControlReference CustomsOfficeCodeFindBox { get; }
		public ControlReference BranchGuidFindBox { get; }
	}
}
