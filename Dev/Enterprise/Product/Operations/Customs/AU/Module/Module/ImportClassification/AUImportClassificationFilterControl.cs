using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Module
{
	/// <summary>
	/// Filter control for AUImportClassification.
	/// </summary>
	public partial class AUImportClassificationFilterControl : Customs.Module.CusClassificationFilterControl<AUTariffFilterStrip>
	{
		public AUImportClassificationFilterControl()
		{
			InitializeComponent();
		}

		public AUImportClassificationFilterControl(IBusinessObjectCollection gridCollection, AUImportClassificationFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
