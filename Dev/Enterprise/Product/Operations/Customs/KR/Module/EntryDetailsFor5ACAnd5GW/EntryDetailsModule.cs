using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.KR.Module
{
	public abstract class EntryDetailsModule : ZFilterGridModule
	{
		public override bool AllowDelete => false;
		public override bool AllowNew => false;
		public override bool AllowEdit => false;
		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;
		protected override IModuleDecisionProvider CreateDefaultModuleDecisionProvider() => new EntryDetailsFor5ACAnd5GWModuleDecisionProvider(this);

		protected class EntryDetailsFor5ACAnd5GWModuleDecisionProvider : DefaultModuleDecisionProvider
		{
			public EntryDetailsFor5ACAnd5GWModuleDecisionProvider(ZFilterModule module) : base(module)
			{
			}

			public override bool AllowExcelExport => false;
		}
	}
}
