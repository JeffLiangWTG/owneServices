using System;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Environment
{
	public class HtmlNotificationEmailSender
	{
		public EmailDef CreateEmail(ZString subject, ZString htmlBody)
		{
			HtmlEmailDef result = new HtmlEmailDef();
			result.Subject = subject;
			result.LoadHtmlUsingTemplate(htmlBody);
			return result;
		}

		public EmailDef CreateEmail(ZString subject, ZString htmlBody, bool attachLogo)
		{
			HtmlEmailDef result = new HtmlEmailDef();
			result.Subject = subject;
			result.LoadHtmlUsingTemplate(htmlBody, attachLogo);
			return result;
		}

		public EmailDef CreateEmail(ZString subject, ZString htmlBody, Guid? compPK = null, Guid? branchPK = null, Guid? deptPk = null)
		{
			HtmlEmailDef result = new HtmlEmailDef();
			result.Subject = subject;
			result.LoadHtmlUsingTemplate(htmlBody, null,
								(compPK ?? EnvProxy.Instance.CurrentCompany.PK),
								(branchPK ?? EnvProxy.Instance.CurrentBranch.PK),
								(deptPk ?? EnvProxy.Instance.CurrentDepartment.PK));
			return result;
		}

		public EmailDef CreateSystemNotificationEmail(ZString subject, ZString htmlBody)
		{
			return CreateEmail(subject, htmlBody, Guid.Empty, Guid.Empty, Guid.Empty);
		}
	}
}
