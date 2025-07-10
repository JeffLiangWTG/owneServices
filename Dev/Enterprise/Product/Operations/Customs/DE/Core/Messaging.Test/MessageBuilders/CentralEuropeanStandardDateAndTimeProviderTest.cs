using System;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.Testing
{
	class CentralEuropeanStandardDateAndTimeProviderTest : TestCase
	{
		[TestDate(2022, 4, 27, 17, 09, 33, 245)]
		[ExpectNoExceptions]
		public void TestNoMilliseconds()
		{
			NUnit.Framework.Assert.That(preparationDateTimeProvider.DateAndTime, Is.EqualTo(new DateTime(2022, 4, 27, 19, 09, 33)));
		}

		[TestDate(2022, 4, 27, 17, 09, 33)]
		[ExpectNoExceptions]
		public void TestDateAndTime()
		{
			NUnit.Framework.Assert.That(preparationDateTimeProvider.DateAndTime, Is.EqualTo(new DateTime(2022, 4, 27, 19, 09, 33)));
		}

		[TestDate(2022, 4, 27, 17, 11, 48)]
		[ExpectNoExceptions]
		public void TestDateAndTimeWithZeroSeconds()
		{
			preparationDateTimeProvider = new CentralEuropeanStandardDateAndTimeProvider(true);
			NUnit.Framework.Assert.That(preparationDateTimeProvider.DateAndTime, Is.EqualTo(new DateTime(2022, 4, 27, 19, 11, 00)));
		}

		[TestDate(2022, 4, 27, 17, 09, 33)]
		[ExpectNoExceptions]
		public void TestDate()
		{
			NUnit.Framework.Assert.That(preparationDateTimeProvider.Date, Is.EqualTo(new DateTime(2022, 4, 27, 0, 0, 0)));
		}

		[TestDate(2022, 4, 27, 17, 11, 48)]
		[ExpectNoExceptions]
		public void TestTime()
		{
			NUnit.Framework.Assert.That(preparationDateTimeProvider.Time, Is.EqualTo("19:11:48"));
		}

		[TestDate(2022, 4, 27, 17, 11, 48)]
		[ExpectNoExceptions]
		public void TestTimeWithZeroSeconds()
		{
			preparationDateTimeProvider = new CentralEuropeanStandardDateAndTimeProvider(true);
			NUnit.Framework.Assert.That(preparationDateTimeProvider.Time, Is.EqualTo("19:11:00"));
		}

		[ExpectNoExceptions]
		public void TestDateTimeStaticFromCreation()
		{
			var date = preparationDateTimeProvider.Date;
			var time = preparationDateTimeProvider.Time;

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(preparationDateTimeProvider.Date, Is.EqualTo(date), "Date does not change");
				System.Threading.Thread.Sleep(1000);
				NUnit.Framework.Assert.That(preparationDateTimeProvider.Time, Is.EqualTo(time), "Time does not change");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			preparationDateTimeProvider = new CentralEuropeanStandardDateAndTimeProvider();
		}
		CentralEuropeanStandardDateAndTimeProvider preparationDateTimeProvider;
	}
}
