using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Support.Testing
{
	[TestsSubclassesOf(typeof(OperationalActionMethodApplicator))]
	public abstract class OperationalActionMethodApplicatorTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation
		protected void ApplyApplicator(BusinessObject[] targets, string expectedLogText)
		{
			ApplyApplicator("", targets, expectedLogText);
		}

		protected void ApplyApplicator(BusinessObject[] targets, string expectedLogText, bool saveFactoryOnSuccess)
		{
			ApplyApplicator("", targets, expectedLogText, saveFactoryOnSuccess);
		}

		protected void ApplyApplicator(string message, BusinessObject[] targets, string expectedLogText)
		{
			ApplyApplicator(message, targets, expectedLogText, false);
		}

		protected void ApplyApplicator(string message, BusinessObject[] targets, string expectedLogText, bool saveFactoryOnSuccess)
		{
			AssertMultilineASCIIEquals(message, expectedLogText, SimulateRun(targets, saveFactoryOnSuccess).MessagesString());
		}

		protected void ApplyApplicatorWhereLogOrderIsUnimportant(BusinessObject[] targets, string[] expectedLogTexts)
		{
			ApplyApplicator(targets, expectedLogTexts, false);
		}

		protected void ApplyApplicator(BusinessObject[] targets, string[] expectedLogTexts, bool saveFactoryOnSuccess)
		{
			var log = SimulateRun(targets, saveFactoryOnSuccess).MessagesString();
			foreach (var expectedLogText in expectedLogTexts)
			{
				Assert(expectedLogText, log.Contains(expectedLogText));
			}

			AssertEquals(expectedLogTexts.Sum(x => x.Length), log.Length);
		}

		protected void ApplyApplicatorLogOrderIsUnimportantIgnoreString(BusinessObject[] targets, string[] expectedLogTexts, string ignoredChars, bool saveFactoryOnSuccess = false)
		{
			var log = SimulateRun(targets, saveFactoryOnSuccess).MessagesString();
			foreach (var expectedLogText in expectedLogTexts)
			{
				Assert(expectedLogText, log.Contains(expectedLogText));
			}

			AssertEquals(expectedLogTexts.Sum(x => x.Length), log.Replace(ignoredChars, string.Empty).Length);
		}

		protected DummyOperationalActionSectionLog SimulateRun(BusinessObject[] targets, bool saveOnSuccess)
		{
			var log = new DummyOperationalActionSectionLog();
			Applicator.InitialiseBeforeAllBatchesRun();
			Applicator.InitialiseBeforeIndividiualBatchRun();
			Applicator.Apply(log, targets);
			log.Verify();
			if (saveOnSuccess && log.HighestErrorLevelEncountered < OperationalActionLogErrorLevel.Error && targets.Length > 0)
			{
				targets[0].Factory.Save();
			}

			if (Applicator.SupportsSummary)
			{
				log.messages.Add("<-- Summary -->");
				Applicator.SummaryLog(log);
			}

			return log;
		}

		protected OperationalActionMethodApplicator Applicator
		{
			get
			{
				return applicator ?? (applicator = (OperationalActionMethodApplicator)GetNewBusinessObject());
			}
		}

		OperationalActionMethodApplicator applicator;
		#endregion
	}
}
