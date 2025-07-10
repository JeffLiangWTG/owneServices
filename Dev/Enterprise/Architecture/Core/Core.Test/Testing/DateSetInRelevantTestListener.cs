using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public class DateSetInRelevantTestListener : BaseTestListener
	{
		DateSetInRelevantTestListener()
		{ }

		[ThreadSafe]
		static DateSetInRelevantTestListener instance;

		public static DateSetInRelevantTestListener Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new DateSetInRelevantTestListener();
				}

				return instance;
			}
		}

		public override void BeforeEachTest(DateTime startTime)
		{
			base.BeforeEachTest(startTime);

			ResetWorkingDaysCalledFlag();
			TestCaseWithFactory.ResetWasAssertDbHitsCalled();
		}

		public override void EndTest(TestCase test, DateTime endTime)
		{
			base.EndTest(test, endTime);

			if (HaveWeBrokenThatMostSacredBondOfTimeSensitiveTestingAndTestDateAttribute(test) && !IsTestExemptFromListener(test))
			{
				TestCase.HtmlFail(string.Format(failMessage, test.Name));
			}
		}

		bool HaveWeBrokenThatMostSacredBondOfTimeSensitiveTestingAndTestDateAttribute(TestCase test)
		{
			return workingDaysCalled
				&& TestCaseWithFactory.GetWasAssertDbHitsCalled()
				&& !HasTestDateAttributeBeenSetForCurrentTest(test);
		}

		bool HasTestDateAttributeBeenSetForCurrentTest(TestCase test)
		{
			return TestDateAttribute.IsActive || test.GetType().GetMethod(test.Name).GetCustomAttributes(typeof(TestDateAttribute), true).Length > 0;
		}

		bool IsTestExemptFromListener(TestCase test)
		{
			return test.GetType().GetMethod(test.Name).GetCustomAttributes(typeof(ExemptFromDateSetInRelevantTestListenerAttribute), true).Length > 0;
		}

		bool workingDaysCalled;

		void ResetWorkingDaysCalledFlag()
		{
			workingDaysCalled = false;
		}

		public void SetWorkingDaysCalled()
		{
			workingDaysCalled = true;
		}

		public const string failMessage = @"
<!DOCTYPE html>
<html>
<body>
<p><b>{0}</b> calls AssertDbHits or AssertDbHitsForAllFactories, AND runs date-sensitive code, however it does NOT set the TestDateAttribute.</p>
<p>Please add an appropriate TestDate attribute to <b>{0}</b>, to prevent annoying amnesties!</p>
<p>For example, ""[TestDate(2017, 10, 03)]"" will set your test on a workday. ""[TestDate(2017, 10, 01)]"" will set it on a weekend.</p>
</body>
</html>";
	}
}
