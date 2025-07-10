using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AQISUserControl : ZUserControl
	{
		public AQISUserControl()
		{
			InitializeComponent();
			UpdateComponentProperty();
		}

		void UpdateComponentProperty()
		{
			var premisesIdColumn = this.aQISPremisesIdProcessingTypeGrid.GetColumnStyle(nameof(AQISPremisesIdAndProcessingType.PremisesId)) as ZCodeFindBoxColumnStyleInfo;
			premisesIdColumn.ModuleID = CMRReferenceDataHelper.UseReferenceData ? Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList : Enterprise.ZArchitecture.Modules.ModuleIDs.Premises;
		}
	}
}
