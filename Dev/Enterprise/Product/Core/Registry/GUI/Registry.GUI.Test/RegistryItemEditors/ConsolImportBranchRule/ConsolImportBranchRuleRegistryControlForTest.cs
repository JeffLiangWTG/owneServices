using CargoWise.Types;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class ConsolImportBranchRuleRegistryControlForTest : ConsolImportBranchRuleRegistryControl
	{
		public ZString DefaultToDestinationDischargePortCaption
		{
			get { return defaultToDestinationDischargePortLabel.CaptionResourceString.Caption; }
		}

		public ZString DefaultToOriginLoadPortCaption
		{
			get { return defaultToOriginLoadPortLabel.CaptionResourceString.Caption; }
		}
	}
}
