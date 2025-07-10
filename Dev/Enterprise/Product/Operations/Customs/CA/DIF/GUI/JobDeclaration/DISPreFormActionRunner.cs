using System.Windows.Forms;
using Enterprise.Customs.GUI;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.DIF.GUI
{
	public sealed class JobDeclarationDISPreFormActionRunner : IDISPreFormActionRunner
	{
		public JobDeclarationDISPreFormActionRunner(IDISHost disHost)
		{
			this.disHost = disHost;
		}
		readonly IDISHost disHost;

		public bool Execute()
		{
			bool result = true;

			if (disHost.NeedToDoPreFormAction())
			{
				var form = new PromptToMergeForm();
				try
				{
					var dialogResult = ZFormModaliser.ShowDialogAndDispose(form);
					if (dialogResult == DialogResult.Yes)
					{
						result = disHost.DoPreFormAction();
					}
					else if (dialogResult == DialogResult.Cancel)
					{
						result = false;
					}
				}
				finally
				{
					form.Dispose();
				}
			}

			return result;
		}
	}
}
