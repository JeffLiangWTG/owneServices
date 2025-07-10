using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CusTempStorage;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class TemporaryStorageHelperTest : TestCaseWithFactory
	{
		[TestDate(2022, 11, 16)]
		public void TestGetCusTempStorageRegLineCollection()
		{
			var cusTempStorageRegHeader1 = Factory.New<CusTempStorageRegHeader>();
			cusTempStorageRegHeader1.SRH_Reference = "TEST1";
			var cusTempStorageRegLine1 = cusTempStorageRegHeader1.CusTempStorageRegLines.AddNew();
			cusTempStorageRegLine1.SRL_LineNumber = 1;
			cusTempStorageRegLine1.SRL_LimitDate = new ZDate(2022, 11, 17);
			var cusTempStorageRegLine2 = cusTempStorageRegHeader1.CusTempStorageRegLines.AddNew();
			cusTempStorageRegLine2.SRL_LineNumber = 2;
			var cusTempStorageRegLine3 = cusTempStorageRegHeader1.CusTempStorageRegLines.AddNew();
			cusTempStorageRegLine3.SRL_LineNumber = 3;
			cusTempStorageRegLine3.SRL_LimitDate = new ZDate(2022, 11, 14);
			var cusTempStorageRegLine4 = cusTempStorageRegHeader1.CusTempStorageRegLines.AddNew();
			cusTempStorageRegLine4.SRL_LineNumber = 4;
			cusTempStorageRegLine4.SRL_CustomsStatus = CustomsStatusList.Codes.FIN;

			var cusTempStorageRegHeader2 = Factory.New<CusTempStorageRegHeader>();
			cusTempStorageRegHeader2.SRH_Reference = "TEST2";
			var cusTempStorageRegLine5 = cusTempStorageRegHeader2.CusTempStorageRegLines.AddNew();
			cusTempStorageRegLine5.SRL_LineNumber = 1;

			var cusTempStorageRegHeader3 = Factory.New<CusTempStorageRegHeader>();
			cusTempStorageRegHeader3.SRH_Reference = "TEST3";
			cusTempStorageRegHeader3.SRH_AppCode = "XXX";
			var cusTempStorageRegLine6 = cusTempStorageRegHeader3.CusTempStorageRegLines.AddNew();
			cusTempStorageRegLine6.SRL_LineNumber = 1;

			var cusTempStorageRegLine7 = cusTempStorageRegHeader1.CusTempStorageRegLines.AddNew();
			cusTempStorageRegLine7.SRL_LineNumber = 7;
			cusTempStorageRegLine7.SRL_CustomsStatus = CustomsStatusList.Codes.DEL;

			Factory.Save();

			CombineAssertions(() =>
			{
				var cusTempStorageRegLineCollection = TemporaryStorageHelper.GetCusTempStorageRegLineCollection(ZString.Empty, Factory);
				var completeFilter = cusTempStorageRegLineCollection.CompleteFilter;

				AssertEquals("cusTempStorageRegLine1", true, cusTempStorageRegLine1.MatchesFilter(completeFilter));
				AssertEquals("cusTempStorageRegLine2, SRL_LimitDate is null", true, cusTempStorageRegLine2.MatchesFilter(completeFilter));
				AssertEquals("cusTempStorageRegLine3, invalid SRL_LimitDate", false, cusTempStorageRegLine3.MatchesFilter(completeFilter));
				AssertEquals("cusTempStorageRegLine4, invalid SRL_CustomsStatus", false, cusTempStorageRegLine4.MatchesFilter(completeFilter));
				AssertEquals("cusTempStorageRegLine5", true, cusTempStorageRegLine5.MatchesFilter(completeFilter));
				AssertEquals("cusTempStorageRegLine6, invalid SRH_AppCode", false, cusTempStorageRegLine6.MatchesFilter(completeFilter));
				AssertEquals("cusTempStorageRegLine7, invalid SRL_CustomsStatus", false, cusTempStorageRegLine7.MatchesFilter(completeFilter));

				var filterString = completeFilter.GetAsWhereClause(true);
				AssertNotContains("No SRH_CustomsOffice filter", EFTA.TemporaryStorageRegister.Business.AutoCusTempStorageRegHeader.Schema.SRH_CustomsOffice, filterString, true);
			});
		}

		public void TestGetCusTempStorageRegLineCollection_CustomsOfficeIsNotEmpty()
		{
			var cusTempStorageRegHeader = Factory.New<CusTempStorageRegHeader>();
			cusTempStorageRegHeader.SRH_Reference = "TEST1";
			cusTempStorageRegHeader.SRH_CustomsOffice = "OFFICE1";
			var cusTempStorageRegLine = cusTempStorageRegHeader.CusTempStorageRegLines.AddNew();
			cusTempStorageRegLine.SRL_LineNumber = 1;
			var cusTempStorageRegHeader2 = Factory.New<CusTempStorageRegHeader>();
			cusTempStorageRegHeader2.SRH_Reference = "TEST2";
			var cusTempStorageRegLine2 = cusTempStorageRegHeader2.CusTempStorageRegLines.AddNew();
			cusTempStorageRegLine2.SRL_LineNumber = 1;
			Factory.Save();

			var cusTempStorageRegLineCollection = TemporaryStorageHelper.GetCusTempStorageRegLineCollection("OFFICE1", Factory);
			var completeFilter = cusTempStorageRegLineCollection.CompleteFilter;

			CombineAssertions(() =>
			{
				AssertEquals("cusTempStorageRegLine", true, cusTempStorageRegLine.MatchesFilter(completeFilter));
				AssertEquals("cusTempStorageRegLine2, invalid SRH_CustomsOffice", false, cusTempStorageRegLine2.MatchesFilter(completeFilter));
			});
		}
	}
}
