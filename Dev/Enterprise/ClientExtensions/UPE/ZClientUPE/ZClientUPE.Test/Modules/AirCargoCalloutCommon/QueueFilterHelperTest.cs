using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	public class QueueFilterHelperTest : TestCaseWithFactory
	{
		public void TestAddIsInProcessQueueToFilter()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			ZQuery query = new ZQuery();
			QueueFilterHelper.AddIsInProcessQueueToFilter(query);
			hAWB.CurrentQueue.P4_QueueName = "";
			Factory.Save();
			CusHAWB[] noMatches = (CusHAWB[])Factory.Load(typeof(CusHAWB), query);
			AssertEquals("Shouldnt match when no queue name specified", 0, noMatches.Length);
			hAWB.CurrentQueue.P4_QueueName = "XXX";
			Factory.Save();
			CusHAWB[] matches = (CusHAWB[])Factory.Load(typeof(CusHAWB), query);
			AssertEquals("Should match when queue name is specified", 1, matches.Length);
		}

		public void TestGetQueueRemarksFilter()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CurrentQueue.P4_Reason = "Remarks";
			Factory.Save();
			TestAddQueueRemarksToFilter(SQLComparisonOperator.Equal, (ZString)"Remarks", true);
			TestAddQueueRemarksToFilter(SQLComparisonOperator.Equal, (ZString)"Remar", false);
			TestAddQueueRemarksToFilter(SQLComparisonOperator.StartsWith, (ZString)"Remar", true);
		}

		void TestAddQueueRemarksToFilter(SQLComparisonOperator @operator, string remarks, bool expectMatch)
		{
			ZQuery query = QueueFilterHelper.GetQueueRemarksFilter(@operator, remarks);
			CusHAWB[] matches = (CusHAWB[])Factory.Load(typeof(CusHAWB), query);
			string queryAsText = ProcessQueueSchema.P4_Reason.Name + " " + @operator + " " + remarks;
			if (expectMatch)
			{
				AssertEquals("Expected a match for " + queryAsText, 1, matches.Length);
			}
			else
			{
				AssertEquals("Expected no matches for " + queryAsText, 0, matches.Length);
			}
		}

		public void TestAddSubQueryToProcessQueueToFilter()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			ZQuery query = new ZQuery();
			QueueFilterHelper.AddSubQueryToProcessQueueToFilter(query, JoinCondition.And, ProcessQueueSchema.P4_Reason, SQLComparisonOperator.StartsWith, (ZString)"Remar");
			hAWB.CurrentQueue.P4_Reason = "nomatch";
			Factory.Save();
			CusHAWB[] noMatches = (CusHAWB[])Factory.Load(typeof(CusHAWB), query);
			AssertEquals("Should find no matches", 0, noMatches.Length);
			hAWB.CurrentQueue.P4_Reason = "Remarks";
			Factory.Save();
			CusHAWB[] matches = (CusHAWB[])Factory.Load(typeof(CusHAWB), query);
			AssertEquals("Should find only 1 match", 1, matches.Length);
		}

		public void TestQueueNamesAndStatuses()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CurrentQueue.P4_QueueName = DefaultQueueCodeDescriptionPairList.Codes.EIR;
			CusHAWB decoyHAWB = mAWB.ChildBills.AddNew(typeof(CusHAWB));
			decoyHAWB.CurrentQueue.P4_QueueName = DefaultQueueCodeDescriptionPairList.Codes.Hold;
			Factory.Save();
			FilterBizObj.QueueStatus.Add(DefaultQueueCodeDescriptionPairList.Codes.EIR);
			ZQuery query = new ZQuery();
			QueueFilterHelper.AddQueueNameAndStatusesToFilter(query);
			CusHAWB[] matches = (CusHAWB[])Factory.Load(typeof(CusHAWB), query);
			AssertEquals("Only 1 match should be found", 1, matches.Length);
			AssertEquals("The correct record should be returned", hAWB.PK, matches[0].PK);
		}

		#region IsUnworked / IsUnworkedToday / Refund Enquiry
		public void TestIsUnworked()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB expectedResult = mAWB.ChildBills.AddNew();
			expectedResult.CurrentQueue.P4_Status = "HR";
			CusHAWB excludedResult = mAWB.ChildBills.AddNew();
			excludedResult.CurrentQueue.P4_Status = "HR";
			SetCusHAWBLastedEditedDate(excludedResult, testDate);
			Factory.Save();
			ZQuery query = new ZQuery();
			QueueFilterHelper.AddUnworkedToFilter(query, true);
			AssertExpectedResults(query, expectedResult);
		}

		public void TestIsUnworkedToday()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB[] expectedResults = new CusHAWB[2];
			expectedResults[0] = mAWB.ChildBills.AddNew();
			expectedResults[0].CS_HAWB = "H1";
			expectedResults[0].CurrentQueue.P4_Status = "HR";
			expectedResults[1] = mAWB.ChildBills.AddNew();
			expectedResults[1].CurrentQueue.P4_Status = "HR";
			expectedResults[1].CS_HAWB = "H2";
			SetCusHAWBLastedEditedDate(expectedResults[1], testDate.AddDays(-1));
			CusHAWB excludedResult = mAWB.ChildBills.AddNew();
			excludedResult.CurrentQueue.P4_Status = "HR";
			excludedResult.CS_HAWB = "exH1";
			SetCusHAWBLastedEditedDate(excludedResult, testDate);
			Factory.Save();
			ZQuery query = new ZQuery();
			QueueFilterHelper.AddUnworkedTodayToFilter(query, true);
			AssertExpectedResults(query, expectedResults);
		}

		[TestUtcOffset(10, 0, 0)]
		[TestDate(2000, 1, 29, 4, 0, 0)] //UTC time: 29/1/2000 4:00 AM(Current local time: 29/1/2000 2:00 PM)
		public void TestIsUnworkedToday_PostedTimeUTC()
		{
			ZQuery query = new ZQuery();
			QueueFilterHelper.AddUnworkedTodayToFilter(query, true);
			CusHAWB[] existingResults = Factory.Load<CusHAWB>(query);
			int numberOfExisingCusHAWB = existingResults.Length;
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CurrentQueue.P4_Status = "HR";
			hAWB.CS_HAWB = "exH2";
			Factory.Save();
			var editedLogDate = TestDateAttribute.Date;
			var localNow = ZDateTime.Now;
			var utcNow = ZDateTime.UtcNow;
			SetCusHAWBLastedEditedDate(hAWB, editedLogDate);
			//Add an EDT event log whose SL_PostedTimeUtc 29/1/2000 7:00 AM 
			hAWB.CS_ConsigneeCity = "CityOfSplaty";
			var originalCurrentDate = TestDateAttribute.Date;
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(-7);
			Factory.Save();
			//Reset current time and do query
			TestDateAttribute.Date = originalCurrentDate;
			query = new ZQuery();
			QueueFilterHelper.AddUnworkedTodayToFilter(query, true);
			CusHAWB[] actualResults = Factory.Load<CusHAWB>(query);
			AssertEquals("EDT event should NOT retrieved.", 0, actualResults.Length - numberOfExisingCusHAWB);
			//Now test EDT event that can be retrieved.
			CusHAWB hAWB1 = mAWB.ChildBills.AddNew();
			hAWB1.CurrentQueue.P4_Status = "HR";
			hAWB1.CS_HAWB = "exH2";
			Factory.Save();
			//Add an EDT event log whose SL_PostedTimeUtc is 28/1/2000 11:00 AM
			hAWB1.CS_ConsigneeCity = "CityOfSplaty";
			originalCurrentDate = TestDateAttribute.Date;
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(-1).AddHours(-3); //1:00 AM
			Factory.Save();
			//Reset current time and do query
			TestDateAttribute.Date = originalCurrentDate;
			query = new ZQuery();
			QueueFilterHelper.AddUnworkedTodayToFilter(query, true);
			actualResults = Factory.Load<CusHAWB>(query);
			AssertEquals("EDT event should be retrieved.", 1, actualResults.Length - numberOfExisingCusHAWB);
		}

		public void TestRefundEnquiry()
		{
			CusHAWB hawb = Factory.NewWithValidTestData<CusHAWB>();
			UPEJobDeclaration declaration = Factory.NewWithValidTestData<UPEJobDeclaration>();
			hawb.CS_JE_CustomsFormalEntry = declaration.PK;
			declaration.IsRefundEnquiry = true;
			Factory.Save();
			ZQuery query = new ZQuery();
			QueueFilterHelper.AddRefundEnquiryToFilter(query, true);
			CusHAWB[] actualResults = Factory.Load<CusHAWB>(query);
			AssertEquals(1, actualResults.Length);
			AssertEquals(hawb.PK, actualResults[0].PK);
			declaration.IsRefundEnquiry = false;
			Factory.Save();
			actualResults = Factory.Load<CusHAWB>(query);
			AssertEquals(0, actualResults.Length);
		}

		void SetCusHAWBLastedEditedDate(CusHAWB hAWB, ZDateTime lastEditedDate)
		{
			Factory.Save();
			hAWB.CS_ConsigneeCity = "CityOfSplaty";
			Factory.Save();
			StmALog editedLog = hAWB.Logs.MostRecentLogByEventTime(Events.EditedARecord);
			((INeedRow)editedLog).Row[StmALogSchema.SL_PostedTimeUtc.Name] = Env.Time.GetUtcFromLocalTime(lastEditedDate.ToDateTime());
			editedLog.HasChanges = true;
			Factory.Save();
		}

		void AssertExpectedResults(ZQuery query, params CusHAWB[] expectedResults)
		{
			CusHAWB[] actualResults = Factory.Load<CusHAWB>(query);
			AssertEquals("Correct number of matches", expectedResults.Length, actualResults.Length);
			foreach (CusHAWB expectedMatch in expectedResults)
			{
				AssertEquals("HAWB " + expectedMatch.CS_HAWB + " should exist in actual results", true, ArrayContainsPK(actualResults, expectedMatch));
			}
		}

		bool ArrayContainsPK(BusinessObject[] list, BusinessObject expectedItemInList)
		{
			foreach (BusinessObject next in list)
			{
				if (next.PK == expectedItemInList.PK)
				{
					return true;
				}
			}

			return false;
		}

		#endregion
		#region Implementation
		TestQueueFilterBusinessObject FilterBizObj;
		QueueFilterHelper QueueFilterHelper;
		ZDateTime testDate;
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			testDate = ZDateTime.Now;
			FilterBizObj = new TestQueueFilterBusinessObject();
			QueueFilterHelper = new QueueFilterHelper(FilterBizObj);
		}
		#endregion
	}
}
