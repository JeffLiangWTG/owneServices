using System.Linq;
using System.Reflection;
using Enterprise.MailManager.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core.Test;
using NUnit.Framework;

[assembly: Enterprise.MailManager.MailSubscriber(typeof(DemoClass))]
[assembly: Enterprise.MailManager.MailSubscriber(typeof(AnotherDemoClass))]

namespace Enterprise.MailManager.Testing
{
	[TestedType(typeof(MailSubscriberAttribute))]
	sealed class MailSubscriberAttributeTest : AssemblyMetaDataAttributeTestCase<MailSubscriberAttribute>
	{
		IMailFilterProvider DemoFilterProvider => typeof(DemoClass).Assembly.GetCustomAttributes<MailSubscriberAttribute>().Single(m => m.Type == typeof(DemoClass));

		public void TestMailSubscriberFindsAttributes()
		{
			var filters = DemoFilterProvider.GetFilters().ToDictionary(f => f.Code);
			Assert("Should load filters on the bizo it points to", !filters.ContainsKey("TS0"));
			Assert("Should load all filters - TS1", filters.ContainsKey("TS1"));
			Assert("Should load all filters - TS2", filters.ContainsKey("TS1"));
		}

		public void TestTryGetValueOnlyEvaluatesRequestedCode()
		{
			AssertEquals(0, DemoClass.TS1Count.Value);
			AssertEquals(0, DemoClass.TS2Count.Value);

			DemoFilterProvider.TryGetFilter("TS1", out _);

			AssertEquals(1, DemoClass.TS1Count.Value);
			AssertEquals("When loading TS1 we don't need to evaluate TS2 - so dont", 0, DemoClass.TS2Count.Value);
		}

		public void TestAllSubscribersHaveFilters()
		{
			foreach (var sub in AssemblyMetaDataReader.GetAttributes<MailSubscriberAttribute>())
			{
				var mailFilterMethods = sub.Type.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(m => m.GetCustomAttribute<MailFilterAttribute>() != null).ToArray();
				if (!mailFilterMethods.Any())
				{
					Fail(
$@"{sub.Type.Name} is marked as a MailSubscriber, but no filter methods were found on the type.

To provide a filter, add a public, static no-args method that returns your IMailFilter (you can base it off a ZQuery using QueryMailFilter) with the MailFilterAttribute.

Example
[MailFilter]
public static IMailFilter FooQuery() => new QueryMailFilter(""XYZ"", subject: ""foo"");");
				}

				AssertNoExceptionThrown("An exception was thrown when extracting/creating one of your message filters. Please ensure all filters are public, static, take no arguments and return an IMailFilter.", () =>
				{
					sub.GetFilters().ToArray();
				});
			}
		}
	}
}
