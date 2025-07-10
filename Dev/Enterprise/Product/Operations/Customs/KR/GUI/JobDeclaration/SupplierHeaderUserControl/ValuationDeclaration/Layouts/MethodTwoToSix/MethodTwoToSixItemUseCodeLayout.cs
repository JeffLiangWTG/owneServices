using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class MethodTwoToSixItemUseCodeLayout : IPanelLayoutProvider
	{
		PanelLayout IPanelLayoutProvider.Layout => PanelLayout;

		PanelLayout PanelLayout { get; }

		public MethodTwoToSixItemUseCodeLayout()
		{
			PanelLayout = CreatePanelLayout();
		}

		PanelLayout CreatePanelLayout()
		{
			var controlBag = MethodTwoToSixControlBag.InstanceForDeclaration;
			var layout = new PanelLayout();
			layout.RegisterControlBag(controlBag);

			var ruler1 = layout.CreateRuler(160);
			var ruler2 = layout.CreateRuler(300);

			layout.Include(0, ruler1, controlBag.SampleItemCheckBox);
			layout.Include(0, ruler1, controlBag.AdvertisingUseCheckBox);
			layout.Include(0, ruler1, controlBag.UseOfDefectiveRepairCheckBox);
			layout.Include(0, ruler1, controlBag.ReplacementItemCheckBox);
			layout.AddColumn();

			layout.Include(1, ruler2, controlBag.GiftOrFreeDonationCheckBox);
			layout.Include(1, ruler2, controlBag.ForProductionAndManufactureCheckBox);
			layout.Include(1, ruler2, controlBag.ItemUseCodeOtherReasonTextBox);

			return layout;
		}
	}
}
