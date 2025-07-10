using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class IncidentDetailsControlBag : ControlBag
	{
		IncidentDetailsControlBag()
		{
			IncidentCodeDropEdit = RegisterControl(nameof(IncidentDetailsUserControl.IncidentCodeDropEdit));
			InformationTextBox = RegisterControl(nameof(IncidentDetailsUserControl.InformationTextBox));
			EndorsementDateEdit = RegisterControl(nameof(IncidentDetailsUserControl.EndorsementDateEdit));
			EndorsementAuthorityTextBox = RegisterControl(nameof(IncidentDetailsUserControl.EndorsementAuthorityTextBox));
			EndorsementCountryCodeDropEdit = RegisterControl(nameof(IncidentDetailsUserControl.EndorsementCountryCodeDropEdit));
			EndorsementPlaceTextBox = RegisterControl(nameof(IncidentDetailsUserControl.EndorsementPlaceTextBox));
			LocationOfGoodsUserControl = RegisterControl(nameof(IncidentDetailsUserControl.LocationOfGoodsUserControl));
			EventCountryCodeDropEdit = RegisterControl(nameof(IncidentDetailsUserControl.EventCountryCodeDropEdit));
			TransportMeansGroupUserControl = RegisterControl(nameof(IncidentDetailsUserControl.TransportMeansGroupUserControl));
		}

		public static IncidentDetailsControlBag Instance => instance ?? (instance = new IncidentDetailsControlBag());

		[ThreadStatic]
		static IncidentDetailsControlBag instance;

		public ControlReference IncidentCodeDropEdit { get; }

		public ControlReference InformationTextBox { get; }

		public ControlReference EndorsementDateEdit { get; }

		public ControlReference EndorsementAuthorityTextBox { get; }

		public ControlReference EndorsementCountryCodeDropEdit { get; }

		public ControlReference EndorsementPlaceTextBox { get; }

		public ControlReference LocationOfGoodsUserControl { get; }

		public ControlReference EventCountryCodeDropEdit { get; }

		public ControlReference TransportMeansGroupUserControl { get; }

		protected override Control CreateTemplate() => new IncidentDetailsUserControl();
	}
}
