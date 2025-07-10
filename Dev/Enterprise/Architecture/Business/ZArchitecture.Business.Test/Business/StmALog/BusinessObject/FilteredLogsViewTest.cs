using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(FilteredLogsView))]
	sealed class FilteredLogsViewTest : BusinessObjectCollectionViewTestCase<FilteredLogsView>
	{
		public void TestCollection()
		{
			var predicate = new Predicate<StmALog>(x => x.SL_Reference.Contains("INCLUDEME"));

			Dummy.Logs.AddNew(Events.Departure, "INCLUDEME");
			Dummy.Logs.AddNew(Events.Arrival, "NOTME");
			Dummy.Logs.AddNew(Events.BookingConfirmed, "ALSOINCLUDEME");
			Dummy.Logs.AddNew(Events.QuotationAccepted, "NOTINFILTER");

			var filteredLogsView = new FilteredLogsView(Dummy, predicate);

			AssertContainsExactElementsInAnyOrder(new[] { Events.DepartureCode, Events.BookingConfirmedCode },
				filteredLogsView.Select(x => x.SL_SE_NKEvent));
		}

		#region Implementation

		DummyWithRelatedLogs Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyWithRelatedLogs>()); }
		}
		DummyWithRelatedLogs dummy;

		protected override FilteredLogsView GetCollectionToTest()
		{
			return new FilteredLogsView(Dummy, new Predicate<StmALog>(x => true));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(StmALog));
		}

		#endregion
	}
}
