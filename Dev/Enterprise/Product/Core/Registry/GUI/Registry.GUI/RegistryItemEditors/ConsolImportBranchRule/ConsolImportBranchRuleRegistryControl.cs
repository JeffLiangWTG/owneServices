using System;

namespace Enterprise.Registry.GUI
{
	public partial class ConsolImportBranchRuleRegistryControl : ShipmentImportBranchRuleRegistryControl
	{
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			defaultToDestinationDischargePortLabel.CaptionResourceString = Res.GetData("ConsolImportBranchRuleRegistryControl|0d5257e8-8ccc-4a21-854c-b2fe3dbdc0d9", "Default to Branch Related to Discharge Port");
			defaultToOriginLoadPortLabel.CaptionResourceString = Res.GetData("ConsolImportBranchRuleRegistryControl|9402e1d5-e013-446e-aaef-24f40b69e5e9", "Default to Branch Related to Load Port");
		}
	}
}
