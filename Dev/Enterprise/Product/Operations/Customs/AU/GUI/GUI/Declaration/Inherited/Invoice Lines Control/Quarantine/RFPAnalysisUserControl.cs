using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI
{
	public partial class RFPAnalysisUserControl : ZUserControl
	{
		public RFPAnalysisUserControl()
		{
			InitializeComponent();
		}

		internal void UpdateDairyFieldsIfRequired(QuarantineExDocHeader quarantineExDocHeader)
		{
			var hasExDocMessages = quarantineExDocHeader.HasExDocMessages();
			QL_PercentOfMilkProteinCalcEdit.Visible = hasExDocMessages;
			QL_TotalWeightOfMilkProteinInMixturesCalcEdit.Visible = hasExDocMessages;
			QL_PercentOfMilkFatCalcEdit.Visible = hasExDocMessages;
			QL_TotalWeightOfMilkFatInMixturesCalcEdit.Visible = hasExDocMessages;
		}

		internal void UpdateEggFieldsIfRequired(QuarantineExDocHeader quarantineExDocHeader)
		{
			var isEGGActive = QuarantineExDocHeader.IsProduceTypeActive(EXDOCCommodityCodes.Codes.Eggs);
			EggGroupBox.Visible = isEGGActive && quarantineExDocHeader.QH_ProduceType == EXDOCCommodityCodes.Codes.Eggs;
		}

		internal void SetupVisibility(ZBool isNEXDOCSActive)
		{
			OtherGroupBox.Visible = isNEXDOCSActive;
		}
	}
}
