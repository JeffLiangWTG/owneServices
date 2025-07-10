using Enterprise.ZArchitecture.Core;

namespace Enterprise.Security.Provider
{
	class VisualizerFormsCheckpointHelper : StmMenuItemCheckpointHelper
	{
		internal override string ModuleIDPrefix
		{
			get { return (NoResString)"Form"; }
		}

		internal override string ModuleIDSuffix
		{
			get { return (NoResString)"Forms"; }
		}

		internal override MultilingualString DisplayText
		{
			get { return ResString.GetMultilingualString("1cd81567-dc38-40bd-8fae-64c1d02b5296", "Forms"); }
		}

		internal override string FallbackHint
		{
			get { return Res.GetString("0030b3ef-2704-46dc-8ba3-f154c24bc5a0", "No description set for this form. Please provide one on the corresponding Forms Customize screen."); }
		}
	}
}
