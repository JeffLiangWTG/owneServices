using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.NCTS
{
	public class PreviousDocumentControlBag : ControlBag
	{
		PreviousDocumentControlBag()
		{
			ReferenceNumberCodeFindBox = RegisterControl(nameof(PreviousDocumentUserControl.ReferenceNumberCodeFindBox));
		}

		public static PreviousDocumentControlBag Instance => instance ?? (instance = new PreviousDocumentControlBag());

		[ThreadStatic]
		static PreviousDocumentControlBag instance;

		public ControlReference ReferenceNumberCodeFindBox { get; }

		protected override Control CreateTemplate() => new PreviousDocumentUserControl();
	}
}
