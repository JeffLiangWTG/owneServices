using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class RefDbExtendedPropertyHelperTest : TestCaseWithFactory
	{
		public void TestGetAndSetRefreshRequired()
		{
			using (var helper = new RefDbExtendedPropertyHelper())
			{
				RegisterDbConnectionToRollback(helper.Connection);
				DataUtils.DropDbExtendedProperty(helper.Connection, RefDbExtendedPropertyHelper.RefreshRequiredPropertyName);
				Assert("Refresh Required", !helper.GetRefreshRequired().HasValue);

				helper.SetRefreshRequired(false);
				AssertEquals("Refresh Required", "N", DataUtils.LoadDbExtendedProperty(helper.Connection, RefDbExtendedPropertyHelper.RefreshRequiredPropertyName));
				Assert("Refresh Required", !helper.GetRefreshRequired().Value);
				helper.SetRefreshRequired(true);
				AssertEquals("Refresh Required", "Y", DataUtils.LoadDbExtendedProperty(helper.Connection, RefDbExtendedPropertyHelper.RefreshRequiredPropertyName));
				Assert("Refresh Required", helper.GetRefreshRequired().Value);
			}
		}

		public void TestGetAndSetLastTariffDataUpdateDate()
		{
			using (var helper = new RefDbExtendedPropertyHelper())
			{
				RegisterDbConnectionToRollback(helper.Connection);

				DataUtils.DropDbExtendedProperty(helper.Connection, RefDbExtendedPropertyHelper.LastUpdateDatePropertyName);
				AssertEquals("Last Data Update", ZDateTime.Empty, helper.GetLastTariffDataUpdateDate());

				helper.SetLastTariffDataUpdateDate(new ZDateTime(2016, 7, 10));
				AssertEquals("Last Data Update", "20160710", DataUtils.LoadDbExtendedProperty(helper.Connection, RefDbExtendedPropertyHelper.LastUpdateDatePropertyName));
				AssertEquals("Last Data Update", new ZDateTime(2016, 7, 10), helper.GetLastTariffDataUpdateDate());
				helper.SetLastTariffDataUpdateDate(new ZDateTime(2016, 7, 5));
				AssertEquals("Last Data Update", "20160710", DataUtils.LoadDbExtendedProperty(helper.Connection, RefDbExtendedPropertyHelper.LastUpdateDatePropertyName));
				AssertEquals("Last Data Update", new ZDateTime(2016, 7, 10), helper.GetLastTariffDataUpdateDate());
				helper.SetLastTariffDataUpdateDate(new ZDateTime(2016, 7, 15));
				AssertEquals("Last Data Update", "20160715", DataUtils.LoadDbExtendedProperty(helper.Connection, RefDbExtendedPropertyHelper.LastUpdateDatePropertyName));
				AssertEquals("Last Data Update", new ZDateTime(2016, 7, 15), helper.GetLastTariffDataUpdateDate());

				DataUtils.SaveDbExtendedProperty(helper.Connection, RefDbExtendedPropertyHelper.LastUpdateDatePropertyName, "20160732");
				AssertEquals("Last Data Update", ZDateTime.Invalid, helper.GetLastTariffDataUpdateDate());
				helper.SetLastTariffDataUpdateDate(new ZDateTime(2016, 7, 5));
				AssertEquals("Last Data Update", "20160705", DataUtils.LoadDbExtendedProperty(helper.Connection, RefDbExtendedPropertyHelper.LastUpdateDatePropertyName));
				AssertEquals("Last Data Update", new ZDateTime(2016, 7, 5), helper.GetLastTariffDataUpdateDate());
			}
		}

		public void TestNoExceptionWhenRefDatabaseNotExist()
		{
			using (var helper = new RefDbExtendedPropertyHelperForTesting())
			{
				AssertNoExceptionThrown(() => AssertNull(helper.Connection));
			}
		}
	}
}
