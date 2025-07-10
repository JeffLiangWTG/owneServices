using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MailManager.MailFilters.Testing.MailTestHelpers;

namespace Enterprise.MailManager.MailFilters.Testing
{
	sealed class QueryMailFilterTests : TestCaseWithFactory
	{
		ZQuery FromBobQuery
			=> new ZQuery(MailDBItemsSchema.MI_From, "bob@tmart.com").AddToFilter(MailDBItemsSchema.MI_Direction, "RCV");

		public void TestNext()
		{
			CreateMail(Factory, "Youngest", "bob@tmart.com", recieved: DateTime.Now, application: "BOB");
			CreateMail(Factory, "Oldest", "bob@tmart.com", recieved: DateTime.Now.AddDays(-10), application: "BOB");
			CreateMail(Factory, "Middle", "bob@tmart.com", recieved: DateTime.Now.AddDays(-5), application: "BOB");

			Factory.Save();
			var filter = new QueryMailFilter("BOB", from: "bob@tmart.com");

			AssertEquals("Oldest", filter.Load(Factory, 1).Single().MI_Subject);
			AssertEquals("Youngest", filter.Load(Factory, 3).Last().MI_Subject);
		}

		public void TestFilterOrderBy()
		{
			CreateMail(Factory, "Youngest", "bob@tmart.com", recieved: DateTime.Now, application: "BOB");
			CreateMail(Factory, "Oldest", "bob@tmart.com", recieved: DateTime.Now.AddDays(-10), application: "BOB");
			CreateMail(Factory, "Middle", "bob@tmart.com", recieved: DateTime.Now.AddDays(-5), application: "BOB");

			Factory.Save();

			var filter = new QueryMailFilter("BOB", from: "bob@tmart.com");
			var subjects = filter.Load(Factory).Select(m => m.MI_Subject.ToString());
			AssertArrayEqualsByElements("Query should order the results such that the oldest item is processed first.", new[] { "Oldest", "Middle", "Youngest" }, subjects.ToArray());

			var manualFilter = new QueryMailFilter("BOB", FromBobQuery);
			subjects = manualFilter.Load(Factory).Select(m => m.MI_Subject.ToString());
			AssertArrayEqualsByElements("Query should order the results such that the oldest item is processed first - even when the query is passed into the filter directly.", new[] { "Oldest", "Middle", "Youngest" }, subjects.ToArray());
		}

		public void TestSubjectsComparison()
		{
			var filter = new QueryMailFilter("HEL", subjects: new string[] { "hi", "greetings", "hello" }, subjectComparison: SQLComparisonOperator.StartsWith);
			Assert("Starts with 'hello', so the filter should match it", filter.CanProcess(CreateMail(Factory, "hello there", "bob@tmart.com")));
			Assert("Starts with 'hi', so the filter should match it", filter.CanProcess(CreateMail(Factory, "hi bob", "notbob@tmart.com")));
			Assert("Contains 'greetings', but at the start so it should not be matched", !filter.CanProcess(CreateMail(Factory, "Merry greetings", "notbob@tmart.com")));
			Assert("Completely off, matches nothing and should be ignored", !filter.CanProcess(CreateMail(Factory, "G'Day Mate", "notbob@tmart.com")));
		}

		public void TestFromParamWithComparison()
		{
			var filter = new QueryMailFilter("XYZ", fromComparison: SQLComparisonOperator.Contains, from: "@bob.com");
			Assert("Matches the FROM filter, should be matched", filter.CanProcess(CreateMail(Factory, "hello there", "bob@bob.com")));
			Assert("Matches the FROM filter (because it's CONTAINS), should be matched", filter.CanProcess(CreateMail(Factory, "hello there", "Bob The First <bob@bob.com>")));
			Assert("Completely wrong FROM-Address, should not match filter", !filter.CanProcess(CreateMail(Factory, "hello there", "Not Bob <bob@notbob.com>")));
			Assert("Completely wrong FROM-Address, should not match filter", !filter.CanProcess(CreateMail(Factory, "hello there", "Bob The First <bob@bobby.com>")));
		}

		public void TestParams()
		{
			var filter = new QueryMailFilter("BOB", from: "bob@tmart.com");
			Assert("Emails from bob should return true", filter.CanProcess(CreateMail(Factory, "Hello", "bob@tmart.com")));
			Assert("Emails from notbob should return false", !filter.CanProcess(CreateMail(Factory, "Hello", "notbob@tmart.com")));

			filter = new QueryMailFilter("BOB", from: "bob@tmart.com", subject: "Hi");
			Assert("Subject & For params should be AND not OR. The subject does not match so the filter shouldn't.", !filter.CanProcess(CreateMail(Factory, "Hello", "bob@tmart.com")));
			Assert("Subject & For params should be AND not OR. This matches all subfilters and should match.", filter.CanProcess(CreateMail(Factory, "Hi", "bob@tmart.com")));

			filter = new QueryMailFilter("BOB", statuses: new string[] { MailStatus.Processed }, subject: null);
			Assert("Should match mailitems with a status that matches the filters status", filter.CanProcess(CreateMail(Factory, "Hello", "bob@tmart.com", status: MailStatus.Processed)));
			Assert("Should ONLY match mailitems with a status that matches the filters status", !filter.CanProcess(CreateMail(Factory, "Hi", "notbob@tmart.com")));
		}

		public void TestConstructor()
		{
			var fromBob = new QueryMailFilter("BOB", FromBobQuery);
			AssertEquals("The code should be unmolested.", "BOB", fromBob.Code);
			AssertEquals("The supplied query should match the returned one.", FromBobQuery.LiteralTextADO, fromBob.Query.LiteralTextADO);
		}

		public void TestQueryCantBeModified()
		{
			var fromBob = new QueryMailFilter("BOB", FromBobQuery);
			fromBob.Query.AddToFilter(JoinCondition.Or, MailDBItemsSchema.MI_From, "jane@tmart.com");

			AssertNotContains("The Query property should not be mutatable externally.", fromBob.Query.LiteralTextADO, "jane@tmart.com");
		}

		public void TestCanProcess()
		{
			var fromBob = CreateMail(Factory, "", "bob@tmart.com");
			var fromJane = CreateMail(Factory, "", "jane@tmart.com");

			var filter = new QueryMailFilter("BOB", FromBobQuery);
			Assert("When the mailitem matches the query CanProcess should return true", filter.CanProcess(fromBob));
			Assert("When the mailitem doesnt match the query CanProcess should return false", !filter.CanProcess(fromJane));
		}
	}
}
