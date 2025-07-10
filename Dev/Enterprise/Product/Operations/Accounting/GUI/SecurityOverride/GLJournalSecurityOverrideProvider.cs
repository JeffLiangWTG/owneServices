using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.GUI
{
	public partial class GLJournalSecurityOverrideProvider : SecurityOverrideProviderWithApprovalRequestSupport
	{
		public GLJournalSecurityOverrideProvider(bool showApprovalRequestButton = false, bool alwaysCreateApprovalRequest = false)
			: base(showApprovalRequestButton, alwaysCreateApprovalRequest)
		{
		}

		protected override SecurityCore RequestLoginCredentials(SecurityCheckpoint checkPoint)
		{
			SecurityCore security = null;
			while (true)
			{
				security = base.RequestLoginCredentials(checkPoint);

				if (LastLoginFormResult != DialogResult.OK)
				{
					break;
				}

				if (security != null)
				{
					if (!AccountingConfigurationRegistry.Instance.AllowOnTheSpotApprovalsOfGLJournals.Value)
					{
						Globals.Message.Show(Res.GetString("2d9059cd-8b7f-4722-a902-3fc4307221df",
@"The action cannot be performed because the Registry 'Allow On The Spot approvals for GL Journals' is set to No.

Please click on the Approval Request button to request approval instead."));
					}
					else if (Env.CurrentUser.PK == security.UserPK && !AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.Value)
					{
						Globals.Message.Show(Res.GetString("85400465-c025-474d-9dcf-549843239d57", "The action cannot be performed because the Registry 'Allow Users to approve own Journals' is set to No."));
					}
					else
					{
						break;
					}
				}
			}

			return security;
		}

		protected override string GetSecurityOverrideMessageCore(SecurityCheckpoint checkPoint)
		{
			var result = base.GetSecurityOverrideMessageCore(checkPoint);

			var note = "";
			if (!AccountingConfigurationRegistry.Instance.AllowOnTheSpotApprovalsOfGLJournals.Value)
			{
				note += "\r\n	" + Res.GetString("2bca99ab-1f9a-4104-8e90-125f61f30d36", "the Registry 'Allow On The Spot approvals for GL Journals' is set to No");
			}
			if (!AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.Value)
			{
				note += "\r\n	" + Res.GetString("0c4b69fe-6451-4cb6-b020-5bda761fd871", "the Registry 'Allow Users to approve own Journals' is set to No");
			}

			if (!string.IsNullOrEmpty(note))
			{
				result += "\r\n" + Res.GetString("98a393f1-6a68-4e78-8b6f-f0359e9b5f13", "Note:{0}.", note);
			}

			return result;
		}
	}
}
