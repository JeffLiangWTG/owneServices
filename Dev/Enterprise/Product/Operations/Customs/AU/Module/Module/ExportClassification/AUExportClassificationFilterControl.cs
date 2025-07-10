using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Module
{
	/// <summary>
	/// Filter control for AUExportClassification.
	/// </summary>
	public partial class AUExportClassificationFilterControl : Customs.Module.CusClassificationFilterControl<AUTariffFilterStrip>
	{
		public AUExportClassificationFilterControl()
		{
			InitializeComponent();
		}

		public AUExportClassificationFilterControl(IBusinessObjectCollection gridCollection, AUExportClassificationFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}

