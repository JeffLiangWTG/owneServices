using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	sealed class MessageFilterMailFilterTest : TestCaseWithFactory
	{
		public void TestCreatesForGroup()
		{
			var filter = new MailFilterProvider(new[] { CreateConfig<DummyFilterClass>("ASS") }).GetFilters().Single();

			var emailsToRecognise = new[]
			{
				CreateMailItem("FOOD ORDER"),
				CreateMailItem("BARBER CLOSING DOWN"),
				CreateMailItem("Crap for sale", "barry@gmail.com")
			};

			var emailsToNotRecognise = new[]
			{
				CreateMailItem("Doesnt Match Anything"),
				CreateMailItem("Partial match doesn't count", "barry@gmail.com")
			};

			CombineAssertions(() =>
			{
				foreach (var mi in emailsToRecognise)
				{
					Assert($"The email <em>{mi.HumanReadableName}</em> matches a filter should have been recognised for processing", filter.CanProcess(mi));
				}

				foreach (var mi in emailsToNotRecognise)
				{
					Assert($"The email <em>{mi.HumanReadableName}</em> does not match any filter, so CanProcess should return false", !filter.CanProcess(mi));
				}
			});
		}

		MailItem CreateMailItem(string subject, string from = "noreply@gmail.com")
		{
			var mi = Factory.NewWithValidTestData<MailItem>();
			mi.MI_Subject = subject;
			mi.MI_From = from;
			return mi;
		}

		IMessageFilterConfig CreateConfig<T>(string code)
		{
			var config = new Mock<IMessageFilterConfig>();
			config.Setup(c => c.ServiceTaskCode).Returns(code);
			config.Setup(c => c.TypeAssemblyName).Returns(typeof(T).Assembly.FullName);
			config.Setup(c => c.TypeName).Returns(typeof(T).FullName);
			config.Setup(c => c.TableName).Returns(MailDBItemsSchema.Constants.TableName);

			return config.Object;
		}
	}
}
