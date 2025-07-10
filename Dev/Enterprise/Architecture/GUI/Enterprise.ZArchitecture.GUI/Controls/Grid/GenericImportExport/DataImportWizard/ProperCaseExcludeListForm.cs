using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.ZArchitecture.GUI.DataMapping
{
	public partial class ProperCaseExcludeListForm : ZChildForm
	{
		internal ProperCaseExcludeListForm(ImportWizard wizard)
			: base(wizard.ProperCaseExcludeList.GetCopy())
		{
			this.wizard = wizard;

			ZFormPostingButtonsStrategy.SetupPosting(this, postingButtonsUserControl);
			DisplayMode = ODisplayMode.Browse;
		}

		readonly ImportWizard wizard;

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		public override string FormCaption
		{
			get { return ResString.GetMultilingualString("D64B4F67-07AF-4084-AC79-1900301CB985", "Proper Case Exclude List"); }
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		protected
#if DEBUG
		internal
#endif
		override void SaveInternal()
		{
			wizard.ProperCaseExcludeList.ReplaceWords(((ProperCaseExcludeWordCollection)DataSource));
		}
	}
}
