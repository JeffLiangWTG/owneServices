//CW1086:Do Not Use System.Web.Mail
using System.Web.Mail;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1086
	{
		public void Method()
		{
			//CW1086:Do Not Use System.Web.Mail
			_ = MailFormat.Html;
		}
	}
}
