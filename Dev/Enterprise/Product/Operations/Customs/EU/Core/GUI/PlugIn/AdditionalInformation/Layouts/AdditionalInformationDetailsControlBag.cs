using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public class AdditionalInformationDetailsControlBag : ControlBag
	{
		protected AdditionalInformationDetailsControlBag()
		{
			KindDropEdit = RegisterControl(nameof(AdditionalInformationDetailsUserControl.KindDropEdit));
			FullTypeCodeFindBox = RegisterControl(nameof(AdditionalInformationDetailsUserControl.FullTypeCodeFindBox));
			ReferenceTextBox = RegisterControl(nameof(AdditionalInformationDetailsUserControl.ReferenceTextBox));
			DescriptionTextBox = RegisterControl(nameof(AdditionalInformationDetailsUserControl.DescriptionTextBox));
			DetailTextBox = RegisterControl(nameof(AdditionalInformationDetailsUserControl.DetailTextBox));
			CurrencyDropEdit = RegisterControl(nameof(AdditionalInformationDetailsUserControl.CurrencyDropEdit));
			AmountCalcEdit = RegisterControl(nameof(AdditionalInformationDetailsUserControl.AmountCalcEdit));
		}

		public static AdditionalInformationDetailsControlBag Instance => instance ?? (instance = new AdditionalInformationDetailsControlBag());

		[ThreadStatic]
		static AdditionalInformationDetailsControlBag instance;

		protected override Control CreateTemplate() => new AdditionalInformationDetailsUserControl();

		public ControlReference KindDropEdit { get; }

		public ControlReference FullTypeCodeFindBox { get; }

		public ControlReference ReferenceTextBox { get; }

		public ControlReference DescriptionTextBox { get; }

		public ControlReference DetailTextBox { get; }

		public ControlReference CurrencyDropEdit { get; }

		public ControlReference AmountCalcEdit { get; }
	}
}
