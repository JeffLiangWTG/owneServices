using CargoWise.Common;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.EmailNotification
{
	public class PeriodClosureEmailNotificationProcessorEmail : AccountingEmailDef
	{
		public PeriodClosureEmailNotificationProcessorEmail(string subject, string body)
			: base()
		{
			Argument.NotNull(subject, "subject");
			Argument.NotNullOrEmpty(body, "body");

			Subject = subject;
			Body = body;
		}

		protected override GuidRegistryItem Recipient => AccountingConfigurationRegistry.Instance.AutoPeriodClosureNotifyGroup;

		protected override string GetBody()
		{
			return Body;
		}

		protected override string GetSubject()
		{
			return Subject;
		}
	}
}
