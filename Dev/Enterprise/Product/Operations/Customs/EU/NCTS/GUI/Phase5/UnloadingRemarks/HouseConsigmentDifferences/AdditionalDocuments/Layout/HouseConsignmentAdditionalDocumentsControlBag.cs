using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class HouseConsignmentAdditionalDocumentsControlBag : ControlBag
	{
		public HouseConsignmentAdditionalDocumentsControlBag()
		{
			SequenceNumberTextBox = RegisterControl(nameof(HouseConsignmentAdditionalDocumentsDetailsUserControl.SequenceNumberTextBox));
			KindDropEdit = RegisterControl(nameof(HouseConsignmentAdditionalDocumentsDetailsUserControl.KindDropEdit));
			DocTypeCodeFindBox = RegisterControl(nameof(HouseConsignmentAdditionalDocumentsDetailsUserControl.DocTypeCodeFindBox));
			ReferenceNumberTextBox = RegisterControl(nameof(HouseConsignmentAdditionalDocumentsDetailsUserControl.ReferenceNumberTextBox));
			TextTextBox = RegisterControl(nameof(HouseConsignmentAdditionalDocumentsDetailsUserControl.TextTextBox));
			StatusLabel = RegisterControl(nameof(HouseConsignmentAdditionalDocumentsDetailsUserControl.StatusLabel));
		}

		public static HouseConsignmentAdditionalDocumentsControlBag Instance => instance ?? (instance = new HouseConsignmentAdditionalDocumentsControlBag());

		[ThreadStatic]
		static HouseConsignmentAdditionalDocumentsControlBag instance;

		protected override Control CreateTemplate() => new HouseConsignmentAdditionalDocumentsDetailsUserControl();

		public ControlReference SequenceNumberTextBox { get; }
		public ControlReference KindDropEdit { get; }
		public ControlReference DocTypeCodeFindBox { get; }
		public ControlReference ReferenceNumberTextBox { get; }
		public ControlReference TextTextBox { get; }
		public ControlReference StatusLabel { get; }
	}
}
