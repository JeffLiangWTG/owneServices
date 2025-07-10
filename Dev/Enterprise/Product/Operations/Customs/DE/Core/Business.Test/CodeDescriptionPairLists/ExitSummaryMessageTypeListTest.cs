using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	class ExitSummaryMessageTypeListTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestGetMessageTypesForArrival()
		{
			NUnit.Framework.Assert.That(ExitSummaryMessageTypeList.ArrivalMessageTypes.CodesAsString, Is.EqualTo("ANT, PRE"));
			var arrivalMessageTypes = ExitSummaryMessageTypeList.ArrivalMessageTypes;
			NUnit.Framework.Assert.That(ExitSummaryMessageTypeList.ArrivalMessageTypes, Is.SameAs(arrivalMessageTypes), "cached");
		}

		[ExpectNoExceptions]
		public void TestGetMessageTypesForDeparture()
		{
			NUnit.Framework.Assert.That(ExitSummaryMessageTypeList.DepartureMessageTypes.CodesAsString, Is.EqualTo("NOT"));
			var departureMessageTypes = ExitSummaryMessageTypeList.DepartureMessageTypes;
			NUnit.Framework.Assert.That(ExitSummaryMessageTypeList.DepartureMessageTypes, Is.SameAs(departureMessageTypes), "cached");
		}
	}
}
