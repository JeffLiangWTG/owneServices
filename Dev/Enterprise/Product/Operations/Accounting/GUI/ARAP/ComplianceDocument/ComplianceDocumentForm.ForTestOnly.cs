#if DEBUG

using System;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class ComplianceDocumentForm
	{
		public ContinueWithDelete ShowPreDeleteDialogs_ForTestOnly()
		{
			return ShowPreDeleteDialogs();
		}

		public void Delete_ForTestOnly()
		{
			Delete();
		}

		public virtual void OnPostButtonClick_ForTestOnly(object sender, EventArgs e)
		{
			OnPostButtonClick(sender, e);
		}
	}
}

#endif
