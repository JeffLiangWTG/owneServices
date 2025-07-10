using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI
{
	public sealed class PreviousDocumentsFieldsControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new PreviousDocumentsFieldsUserControl();

		public static PreviousDocumentsFieldsControlBag Instance => instance ?? (instance = new PreviousDocumentsFieldsControlBag());

		[ThreadStatic]
		static PreviousDocumentsFieldsControlBag instance;

		PreviousDocumentsFieldsControlBag()
		{
			CodeCodeFindBox = RegisterControl(nameof(PreviousDocumentsFieldsUserControl.CodeCodeFindBox));
			ReferenceTextBox = RegisterControl(nameof(PreviousDocumentsFieldsUserControl.ReferenceTextBox));
		}

		public ControlReference CodeCodeFindBox { get; }

		public ControlReference ReferenceTextBox { get; }
	}
}
