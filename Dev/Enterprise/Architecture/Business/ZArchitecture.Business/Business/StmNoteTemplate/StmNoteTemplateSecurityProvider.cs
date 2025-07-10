using System;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Business
{
	public class StmNoteTemplateSecurityProvider
	{
		public static SecurityCheckpoint SecurityCheckpointForPublish
		{
			get { return ((SecurityCore)EnvProxy.Instance.Security).PublishGlobalNoteTemplates; }
		}

		public bool CurrentUserCanPublish
		{
			get { return SecurityCheckpointForPublish.IsAllowed; }
		}

		public bool CurrentUserCanPublishAcrossAllCompanies
		{
			get
			{
				if (allCompaniesUserContext == null || allCompaniesUserContext.User?.LoginName != EnvProxy.Instance.CurrentUser.LoginName)
				{
					allCompaniesUserContext = EnvProxy.Instance.NewUserContext(EnvProxy.Instance.CurrentUser.LoginName, Guid.Empty, Guid.Empty);

					using (EnvProxy.Instance.SetTemporaryUserContext(allCompaniesUserContext))
					{
						currentUserCanPublishAcrossAllCompanies = CurrentUserCanPublish;
					}
				}

				return currentUserCanPublishAcrossAllCompanies;
			}
		}

		IUserContext allCompaniesUserContext;
		bool currentUserCanPublishAcrossAllCompanies;

#if DEBUG
		internal void ClearAllCompaniesUserContext()
		{
			allCompaniesUserContext = null;
		}
#endif

		public bool IsTemplateEditableByUser(StmNoteTemplate template)
		{
			return (!template.IsPublished || CurrentUserCanPublish) && (!template.IsAllCompanies || CurrentUserCanPublishAcrossAllCompanies);
		}
	}
}
