using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(RecentCommissionsRow))]
	public class RecentCommissionsRowTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new RecentCommissionsRow();
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesRecentCommissionsRow()
		{
			var row = (RecentCommissionsRow)GetNewBusinessObject();

			var decimalList = new List<string>
			{
				nameof(row.Total),
				nameof(row.TotalCurrentMonth),
				nameof(row.TotalPreviousMonth),
				nameof(row.Total2MonthsAgo),
				nameof(row.Total3MonthsAgo),
				nameof(row.TotalOver3MonthsAgo)
			};

			var tester = new DecimalPlacesAttributeTester(row);
			tester.CheckSetter(decimalList, nameof(row.DecimalPlaces));
		}
	}
}
