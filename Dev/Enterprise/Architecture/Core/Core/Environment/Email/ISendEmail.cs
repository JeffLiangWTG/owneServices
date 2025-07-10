using CargoWise.Application;

namespace Enterprise.ZArchitecture.Environment
{
	public static class SendEmail
	{
		public static string ToDatabaseHealthCheckNotificationGroup(string subject, string body)
		{
			return Instance.ToDatabaseHealthCheckNotificationGroup(subject, body);
		}

		static ISendEmail Instance
		{
			get { return fInstance ?? (fInstance = ObjectFactory.Get<ISendEmail>()); }
		}

		static ISendEmail fInstance;
	}

	public interface ISendEmail
	{
		string ToDatabaseHealthCheckNotificationGroup(string subject, string body);
	}
}
