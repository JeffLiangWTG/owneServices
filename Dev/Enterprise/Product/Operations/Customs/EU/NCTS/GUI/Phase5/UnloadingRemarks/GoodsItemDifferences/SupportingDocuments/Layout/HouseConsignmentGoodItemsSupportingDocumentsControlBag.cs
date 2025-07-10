using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class HouseConsignmentGoodItemsSupportingDocumentsControlBag : ControlBag
	{
		public HouseConsignmentGoodItemsSupportingDocumentsControlBag()
		{
			SequenceNumberTextBox = RegisterControl(nameof(HouseConsignmentGoodItemsSupportingDocumentsUserControl.SequenceNumberTextBox));
			DocTypeCodeFindBox = RegisterControl(nameof(HouseConsignmentGoodItemsSupportingDocumentsUserControl.DocTypeCodeFindBox));
			ReferenceNumberTextBox = RegisterControl(nameof(HouseConsignmentGoodItemsSupportingDocumentsUserControl.ReferenceNumberTextBox));
			ComplementInfoTextBox = RegisterControl(nameof(HouseConsignmentGoodItemsSupportingDocumentsUserControl.ComplementInfoTextBox));
			StatusLabel = RegisterControl(nameof(HouseConsignmentGoodItemsSupportingDocumentsUserControl.StatusLabel));
		}

		public static HouseConsignmentGoodItemsSupportingDocumentsControlBag Instance => instance ?? (instance = new HouseConsignmentGoodItemsSupportingDocumentsControlBag());

		[ThreadStatic]
		static HouseConsignmentGoodItemsSupportingDocumentsControlBag instance;

		protected override Control CreateTemplate() => new HouseConsignmentGoodItemsSupportingDocumentsUserControl();

		public ControlReference SequenceNumberTextBox { get; }
		public ControlReference DocTypeCodeFindBox { get; }
		public ControlReference ReferenceNumberTextBox { get; }
		public ControlReference ComplementInfoTextBox { get; }
		public ControlReference StatusLabel { get; }
	}
}
