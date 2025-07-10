using Enterprise.ZArchitecture.Core;

namespace Enterprise.Security.Provider
{
	class DocumentsCheckpointHelper : StmMenuItemCheckpointHelper
	{
		internal override string ModuleIDPrefix
		{
			get { return (NoResString)"Doc"; }
		}

		internal override string ModuleIDSuffix
		{
			get { return (NoResString)"Documents"; }
		}

		internal override MultilingualString DisplayText
		{
			get { return ResString.GetMultilingualString("eb7cc692-eb9b-4b00-9542-81d62e388efe", "Documents"); }
		}

		internal override string FallbackHint
		{
			get { return Res.GetString("18cf36f0-e29f-43b0-9bd3-94199fcf3165", "No description set for this document. Please provide one on the corresponding Document Customize screen."); }
		}
	}
}
