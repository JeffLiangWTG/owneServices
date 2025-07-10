using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public class UCC6TemporaryStorageAdditionalInformationDetailsControlBag : ControlBag
	{
		protected UCC6TemporaryStorageAdditionalInformationDetailsControlBag()
		{
			KindDropEdit = RegisterControl(nameof(UCC6TemporaryStorageAdditionalInformationDetailsUserControl.KindDropEdit));
			FullTypeCodeFindBox = RegisterControl(nameof(UCC6TemporaryStorageAdditionalInformationDetailsUserControl.FullTypeCodeFindBox));
			ReferenceTextBox = RegisterControl(nameof(UCC6TemporaryStorageAdditionalInformationDetailsUserControl.ReferenceTextBox));
			DescriptionTextBox = RegisterControl(nameof(UCC6TemporaryStorageAdditionalInformationDetailsUserControl.DescriptionTextBox));
			DetailTextBox = RegisterControl(nameof(UCC6TemporaryStorageAdditionalInformationDetailsUserControl.DetailTextBox));
			CurrencyDropEdit = RegisterControl(nameof(UCC6TemporaryStorageAdditionalInformationDetailsUserControl.CurrencyDropEdit));
			AmountCalcEdit = RegisterControl(nameof(UCC6TemporaryStorageAdditionalInformationDetailsUserControl.AmountCalcEdit));
		}

		protected override Control CreateTemplate() => new UCC6TemporaryStorageAdditionalInformationDetailsUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		public static UCC6TemporaryStorageAdditionalInformationDetailsControlBag Instance => uCC6TemporaryStorageAdditionalInformationDetailsControlBag.Value;

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<UCC6TemporaryStorageAdditionalInformationDetailsControlBag> uCC6TemporaryStorageAdditionalInformationDetailsControlBag = new Lazy<UCC6TemporaryStorageAdditionalInformationDetailsControlBag>(() => new UCC6TemporaryStorageAdditionalInformationDetailsControlBag());

		public ControlReference KindDropEdit { get; }

		public ControlReference FullTypeCodeFindBox { get; }

		public ControlReference ReferenceTextBox { get; }

		public ControlReference DescriptionTextBox { get; }

		public ControlReference DetailTextBox { get; }

		public ControlReference CurrencyDropEdit { get; }

		public ControlReference AmountCalcEdit { get; }
	}
}
