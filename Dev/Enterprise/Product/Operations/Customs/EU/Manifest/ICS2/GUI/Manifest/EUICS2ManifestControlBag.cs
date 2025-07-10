using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	public class EUICS2ManifestControlBag : ControlBag
	{
		public static EUICS2ManifestControlBag Instance => manifestControlBag.Value;

		public EUICS2ManifestControlBag()
		{
			SpecificCircumstanceIndicatorDropEdit = RegisterControl(nameof(EUICS2ManifestFieldsUserControl.SpecificCircumstanceIndicatorDropEdit));
			ReEntryIndicatorCheckBox = RegisterControl(nameof(EUICS2ManifestFieldsUserControl.ReEntryIndicatorCheckBox));
			PreviousMRNTextBox = RegisterControl(nameof(EUICS2ManifestFieldsUserControl.PreviousMRNTextBox));
			BranchGuidFindBox = RegisterControl(nameof(EUICS2ManifestFieldsUserControl.BranchGuidFindBox));
			LocalReferenceNumberTextBox = RegisterControl(nameof(EUICS2ManifestFieldsUserControl.LocalReferenceNumberTextBox));
			MOTIdentifierTypeDropEdit = RegisterControl(nameof(EUICS2ManifestFieldsUserControl.MOTIdentifierTypeDropEdit));
			MOTIdentifierTextBox = RegisterControl(nameof(EUICS2ManifestFieldsUserControl.MOTIdentifierTextBox));
			TransportDocumentTypeDropEdit = RegisterControl(nameof(EUICS2ManifestFieldsUserControl.TransportDocumentTypeDropEdit));
			RegistrationNumberTextBox = RegisterControl(nameof(EUICS2ManifestFieldsUserControl.RegistrationNumberTextBox));
			AddressedMemberStateDropEdit = RegisterControl(nameof(EUICS2ManifestFieldsUserControl.AddressedMemberStateDropEdit));
			CustomsProfileDropEdit = RegisterControl(nameof(EUICS2ManifestFieldsUserControl.CustomsProfileDropEdit));
			ActualDepartureDateEdit = RegisterControl(nameof(EUICS2ManifestFieldsUserControl.ActualDepartureDateEdit));
			MeansOfTransportTypeDropEdit = RegisterControl(nameof(EUICS2ManifestFieldsUserControl.MeansOfTransportTypeDropEdit));
			VehicleRegistrationAndNationalityUserControl = RegisterControl(nameof(EUICS2ManifestFieldsUserControl.VehicleRegistrationAndNationalityUserControl));
			ReceptacleUserControl = RegisterControl(nameof(EUICS2ManifestFieldsUserControl.ReceptacleUserControl));
			SplitConsignmentIndicatorCheckBox = RegisterControl(nameof(EUICS2ManifestFieldsUserControl.SplitConsignmentIndicatorCheckBox));
			OriginCodeFindBox = RegisterControl(nameof(EUICS2ManifestFieldsUserControl.OriginCodeFindBox));
			FinalDestinationCodeFindBox = RegisterControl(nameof(EUICS2ManifestFieldsUserControl.FinalDestinationCodeFindBox));
		}

		public ControlReference SpecificCircumstanceIndicatorDropEdit { get; }
		public ControlReference ReEntryIndicatorCheckBox { get; }
		public ControlReference PreviousMRNTextBox { get; }
		public ControlReference BranchGuidFindBox { get; }
		public ControlReference LocalReferenceNumberTextBox { get; }
		public ControlReference MOTIdentifierTypeDropEdit { get; }
		public ControlReference MOTIdentifierTextBox { get; }
		public ControlReference TransportDocumentTypeDropEdit { get; }
		public ControlReference RegistrationNumberTextBox { get; }
		public ControlReference AddressedMemberStateDropEdit { get; }
		public ControlReference CustomsProfileDropEdit { get; }
		public ControlReference ActualDepartureDateEdit { get; }
		public ControlReference MeansOfTransportTypeDropEdit { get; }
		public ControlReference VehicleRegistrationAndNationalityUserControl { get; }
		public ControlReference ReceptacleUserControl { get; }
		public ControlReference SplitConsignmentIndicatorCheckBox { get; }
		public ControlReference OriginCodeFindBox { get; }
		public ControlReference FinalDestinationCodeFindBox { get; }

		protected override Control CreateTemplate() => new EUICS2ManifestFieldsUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		readonly static Lazy<EUICS2ManifestControlBag> manifestControlBag = new Lazy<EUICS2ManifestControlBag>(() => new EUICS2ManifestControlBag());
	}
}
