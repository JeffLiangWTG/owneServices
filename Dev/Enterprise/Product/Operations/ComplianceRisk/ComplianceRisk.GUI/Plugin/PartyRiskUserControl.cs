using Enterprise.ComplianceRisk.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ComplianceRisk.GUI
{
	public partial class PartyRiskUserControl : ZUserControl
	{
		public PartyRiskUserControl(ComplianceRiskPlugInBusinessObject plugInBizO)
		{
			PlugInBizO = plugInBizO;
			InitializeComponent();
		}

		ComplianceRiskPlugInBusinessObject PlugInBizO { get; }

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource: PlugInBizO, dataMember: "");
		}
	}
}
