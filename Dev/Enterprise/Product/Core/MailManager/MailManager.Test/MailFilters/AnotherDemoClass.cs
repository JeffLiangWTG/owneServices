using Enterprise.MailManager.MailFilters;

namespace Enterprise.MailManager.Testing
{
	sealed class AnotherDemoClass
	{
		[MailFilter("TS0")]
		public static IMailFilter MakeTs0Filter()
			=> new QueryMailFilter("TS0", subject: "I am ts1");
	}
}
