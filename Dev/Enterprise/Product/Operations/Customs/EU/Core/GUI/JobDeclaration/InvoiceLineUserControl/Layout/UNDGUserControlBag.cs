using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class UNDGUserControlBag : ControlBag
	{
		public static UNDGUserControlBag Instance => uNDGUserControlBag.Value;

		UNDGUserControlBag()
		{
			DGLinkLabel = RegisterControl(nameof(UNDGUserControl.DGLinkLabel));
			DGGuidFindBox = RegisterControl(nameof(UNDGUserControl.DGGuidFindBox));
			FlashpointUserControl = RegisterControl(nameof(UNDGUserControl.FlashpointUserControl));
		}

		public ControlReference DGLinkLabel { get; }
		public ControlReference DGGuidFindBox { get; }
		public ControlReference FlashpointUserControl { get; }

		protected override Control CreateTemplate() => new UNDGUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<UNDGUserControlBag> uNDGUserControlBag = new Lazy<UNDGUserControlBag>(() => new UNDGUserControlBag());
	}
}
