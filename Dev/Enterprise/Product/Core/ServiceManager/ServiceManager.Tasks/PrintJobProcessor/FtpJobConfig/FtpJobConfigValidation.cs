using CargoWise.EntityFramework;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor
{
	public class FtpJobConfigValidation : AutoFtpJobConfigValidation
	{
		public FtpJobConfigValidation(AutoFtpJobConfig parent)
			: base(parent)
		{
		}

		protected override void CheckNotificationGroup_PK()
		{
			if ((Parent.NotifyOnFailure || Parent.NotifyOnSuccess) && !Parent.NotifyPrintUser)
			{
				MandatoryValidation.CheckEntered(Parent.NotificationGroup_PKInfo);
			}
			base.CheckNotificationGroup_PK();
		}
	}
}
