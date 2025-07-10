using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class VehicleLayout : IPanelLayoutProvider
	{
		public VehicleLayout()
		{
			PanelLayout = CreatePanelLayout();
		}

		public PanelLayout Layout => PanelLayout;
		PanelLayout PanelLayout { get; }
		PanelLayout CreatePanelLayout()
		{
			var bag = VehicleControlBag.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(bag);
			var ruler1 = layout.CreateRuler(140);

			layout.Include(0, ruler1, bag.NameTextBox);
			layout.Include(0, ruler1, bag.ExhaustVolumeCalcEdit);
			layout.Include(0, ruler1, bag.ManufacturingCountryCodeFindBox);
			layout.Include(0, ruler1, bag.FirstRegistrationDateEdit);
			layout.AddColumn();
			layout.Include(1, ruler1, bag.VehicleIDNumberTextBox);
			layout.Include(1, ruler1, bag.ModelYearTextBox);
			layout.Include(1, ruler1, bag.SeatCapacityCalcEdit);
			layout.Include(1, ruler1, bag.CurrentRegistrationDateEdit);
			return layout;
		}
	}
}
