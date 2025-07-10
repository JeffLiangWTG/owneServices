using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class DepartureDetailsControlBag : ControlBag
	{
		public DepartureDetailsControlBag()
		{
			CustomerReferenceNumberTextBox = RegisterControl(nameof(DepartureDetailsUserControl.CustomerReferenceNumberTextBox));
			DeclarationTypeDropEdit = RegisterControl(nameof(DepartureDetailsUserControl.DeclarationTypeDropEdit));
			AdditionalDeclarationTypeDropEdit = RegisterControl(nameof(DepartureDetailsUserControl.AdditionalDeclarationTypeDropEdit));
			TirCarnetNumberTextBox = RegisterControl(nameof(DepartureDetailsUserControl.TirCarnetNumberTextBox));
			CountryOfDispatchDropEdit = RegisterControl(nameof(DepartureDetailsUserControl.CountryOfDispatchDropEdit));
			CountryOfDestinationDropEdit = RegisterControl(nameof(DepartureDetailsUserControl.CountryOfDestinationDropEdit));
			SecurityDropEdit = RegisterControl(nameof(DepartureDetailsUserControl.SecurityDropEdit));
			GrossWeightCalcDropEdit = RegisterControl(nameof(DepartureDetailsUserControl.GrossWeightCalcDropEdit));
			LocationOfGoodsUserControl = RegisterControl(nameof(DepartureDetailsUserControl.LocationOfGoodsUserControl));
			DateLimitDateEdit = RegisterControl(nameof(DepartureDetailsUserControl.DateLimitDateEdit));
			SimplifiedProcedureAndReducedDataSetUserControl = RegisterControl(nameof(DepartureDetailsUserControl.SimplifiedProcedureAndReducedDataSetUserControl));
			PresentationDateTimeOffsetEdit = RegisterControl(nameof(DepartureDetailsUserControl.PresentationDateTimeOffsetEdit));
			CommunicationLanguageDropEdit = RegisterControl(nameof(DepartureDetailsUserControl.CommunicationLanguageDropEdit));
			TimeLimitForTransitCalcEdit = RegisterControl(nameof(DepartureDetailsUserControl.TimeLimitForTransitCalcEdit));
			OverrideFreightDetailsCheckBox = RegisterControl(nameof(DepartureDetailsUserControl.OverrideFreightDetailsCheckBox));
			CommercialReferenceNumberTextBox = RegisterControl(nameof(DepartureDetailsUserControl.CommercialReferenceNumberTextBox));
		}

		public static DepartureDetailsControlBag Instance => instance ?? (instance = new DepartureDetailsControlBag());

		[ThreadStatic]
		static DepartureDetailsControlBag instance;

		public ControlReference CustomerReferenceNumberTextBox { get; }

		public ControlReference DeclarationTypeDropEdit { get; }

		public ControlReference AdditionalDeclarationTypeDropEdit { get; }

		public ControlReference TirCarnetNumberTextBox { get; }

		public ControlReference CountryOfDispatchDropEdit { get; }

		public ControlReference CountryOfDestinationDropEdit { get; }

		public ControlReference SecurityDropEdit { get; }

		public ControlReference GrossWeightCalcDropEdit { get; }

		public ControlReference LocationOfGoodsUserControl { get; }

		public ControlReference DateLimitDateEdit { get; }

		public ControlReference SimplifiedProcedureAndReducedDataSetUserControl { get; }

		public ControlReference PresentationDateTimeOffsetEdit { get; }

		public ControlReference CommunicationLanguageDropEdit { get; }

		public ControlReference TimeLimitForTransitCalcEdit { get; }

		public ControlReference OverrideFreightDetailsCheckBox { get; }

		public ControlReference CommercialReferenceNumberTextBox { get; }

		protected override Control CreateTemplate() => new DepartureDetailsUserControl();
	}
}
