using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public sealed class ConsignmentItemControlBag : ControlBag
	{
		public ConsignmentItemControlBag()
		{
			ConsignmentItemPackingDetailsUserControl = RegisterControl(nameof(ConsignmentItemUserControl.ConsignmentItemPackingDetailsUserControl));
			ReportAdditionalDocumentsGridUserControl = RegisterControl(nameof(ConsignmentItemUserControl.ReportAdditionalDocumentsGridUserControl));
			AdditionalDocumentsLabel = RegisterControl(nameof(ConsignmentItemUserControl.AdditionalDocumentsLabel));
		}

		public static ConsignmentItemControlBag Instance => instance ?? (instance = new ConsignmentItemControlBag());

		[ThreadStatic]
		static ConsignmentItemControlBag instance;

		public ControlReference ConsignmentItemPackingDetailsUserControl { get; }
		public ControlReference ReportAdditionalDocumentsGridUserControl { get; }
		public ControlReference AdditionalDocumentsLabel { get; }

		protected override Control CreateTemplate() => new ConsignmentItemUserControl();
	}
}
