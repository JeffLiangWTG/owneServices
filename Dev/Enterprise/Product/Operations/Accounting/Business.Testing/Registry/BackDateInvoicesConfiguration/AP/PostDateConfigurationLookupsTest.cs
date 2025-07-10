using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	public abstract class PostDateConfigurationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestJobTypeList()
		{
			AssertEquals("The JobTypeList should contain 'ALL'", true, BizObj.JobTypeList.ContainsCode("ALL"));
			AssertEquals("The JobTypeList should contain 'SHP'", true, BizObj.JobTypeList.ContainsCode("SHP"));
			AssertEquals("The JobTypeList should contain 'CLL'", true, BizObj.JobTypeList.ContainsCode("CLL"));
			AssertEquals("The JobTypeList should contain 'CSH'", true, BizObj.JobTypeList.ContainsCode("CSH"));
			AssertEquals("The JobTypeList should contain 'BRK'", true, BizObj.JobTypeList.ContainsCode("BRK"));
			AssertEquals("The JobTypeList should contain 'FCN'", true, BizObj.JobTypeList.ContainsCode("FCN"));
			AssertEquals("The JobTypeList should contain 'GCN'", true, BizObj.JobTypeList.ContainsCode("GCN"));
		}

		public void TestDirectionList()
		{
			AssertEquals("DirectionList.Count", 5, BizObj.DirectionList.Count);
			AssertEquals("The DirectionList should contain 'All'", true, BizObj.DirectionList.ContainsCode(Constants.FreightShipmentDirection.Code.All));
			AssertEquals("The DirectionList should contain 'Import'", true, BizObj.DirectionList.ContainsCode(Constants.FreightShipmentDirection.Code.Import));
			AssertEquals("The DirectionList should contain 'Export'", true, BizObj.DirectionList.ContainsCode(Constants.FreightShipmentDirection.Code.Export));
			AssertEquals("The DirectionList should contain 'Domestic'", true, BizObj.DirectionList.ContainsCode(Constants.FreightShipmentDirection.Code.Domestic));
			AssertEquals("The DirectionList should contain 'Other'", true, BizObj.DirectionList.ContainsCode(Constants.FreightShipmentDirection.Code.Other));
		}

		public void TestModeList()
		{
			AssertEquals("DirectionList.Count", 8, BizObj.ModeList.Count);
			AssertEquals("The ModeList should contain 'ALL'", true, BizObj.ModeList.ContainsCode(PostDateConfigurationLookups.ModeAdditionalCodes.All));
			AssertEquals("The ModeList should contain 'AIR'", true, BizObj.ModeList.ContainsCode(Core.Constants.TransportModes.Air));
			AssertEquals("The ModeList should contain 'SEA'", true, BizObj.ModeList.ContainsCode(Core.Constants.TransportModes.Sea));
			AssertEquals("The ModeList should contain 'ROA'", true, BizObj.ModeList.ContainsCode(Core.Constants.TransportModes.Road));
			AssertEquals("The ModeList should contain 'RAI'", true, BizObj.ModeList.ContainsCode(Core.Constants.TransportModes.Rail));
		}

		public void TestSignificantDateList()
		{
			AssertEquals("SignificantDateList.Count", 1, BizObj.SignificantDateList.Count);
			AssertEquals("SignificantDateList should contain 'Invoice Add Date'", true, BizObj.SignificantDateList.ContainsCode(PostDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate));
		}

		public void TestCompleteSignificantDateList()
		{
			AssertEquals("SignificantDateList.Count", 9, CompleteSignificantDateList.Count);
			AssertEquals("SignificantDateList should contain 'Actual Arrival Date'", true, CompleteSignificantDateList.ContainsCode(PostDateConfigurationLookups.SignificantDateCodes.ActualArrivalDate));
			AssertEquals("SignificantDateList should contain 'Actual Departure Date'", true, CompleteSignificantDateList.ContainsCode(PostDateConfigurationLookups.SignificantDateCodes.ActualDepartureDate));
			AssertEquals("SignificantDateList should contain 'Customs Clearance Date'", true, CompleteSignificantDateList.ContainsCode(PostDateConfigurationLookups.SignificantDateCodes.CustomsClearanceDate));
			AssertEquals("SignificantDateList should contain 'AWB Issue Date'", true, CompleteSignificantDateList.ContainsCode(PostDateConfigurationLookups.SignificantDateCodes.AWBIssueDate));
			AssertEquals("SignificantDateList should contain 'Delivery Date'", true, CompleteSignificantDateList.ContainsCode(PostDateConfigurationLookups.SignificantDateCodes.DeliveryDate));
			AssertEquals("SignificantDateList should contain 'Pickup Date'", true, CompleteSignificantDateList.ContainsCode(PostDateConfigurationLookups.SignificantDateCodes.PickupDate));
			AssertEquals("SignificantDateList should contain 'Job Open Date'", true, CompleteSignificantDateList.ContainsCode(PostDateConfigurationLookups.SignificantDateCodes.JobOpenDate));
			AssertEquals("SignificantDateList should contain 'Invoice Add Date'", true, CompleteSignificantDateList.ContainsCode(PostDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate));
			AssertEquals("SignificantDateList should contain 'Invoice Date'", true, CompleteSignificantDateList.ContainsCode(PostDateConfigurationLookups.SignificantDateCodes.InvoiceDate));
		}

		public void TestSignificantDatePermittedForOthersList()
		{
			AssertEquals("SignificantDateList.Count", 3, SignificantDatePermittedForOthersList.Count);
			AssertEquals("SignificantDateList should contain 'Job Open Date'", true, SignificantDatePermittedForOthersList.ContainsCode(PostDateConfigurationLookups.SignificantDateCodes.JobOpenDate));
			AssertEquals("SignificantDateList should contain 'Customs Clearance Date'", false, SignificantDatePermittedForOthersList.ContainsCode(PostDateConfigurationLookups.SignificantDateCodes.CustomsClearanceDate));
			AssertEquals("SignificantDateList should contain 'Invoice Add Date'", true, SignificantDatePermittedForOthersList.ContainsCode(PostDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate));
			AssertEquals("SignificantDateList should contain 'Invoice Date'", true, SignificantDatePermittedForOthersList.ContainsCode(PostDateConfigurationLookups.SignificantDateCodes.InvoiceDate));
		}

		public void TestSignificantDatePermittedForShipmentsList()
		{
			AssertEquals("SignificantDateList.Count", 10, SignificantDatePermittedForShipmentsList.Count);
			AssertEquals("SignificantDateList should contain 'Actual Arrival Date'", true, SignificantDatePermittedForShipmentsList.ContainsCode(PostDateConfigurationLookups.SignificantDateCodes.ActualArrivalDate));
			AssertEquals("SignificantDateList should contain 'Actual Departure Date'", true, SignificantDatePermittedForShipmentsList.ContainsCode(PostDateConfigurationLookups.SignificantDateCodes.ActualDepartureDate));
			AssertEquals("SignificantDateList should contain 'Customs Clearance Date'", true, SignificantDatePermittedForShipmentsList.ContainsCode(PostDateConfigurationLookups.SignificantDateCodes.CustomsClearanceDate));
			AssertEquals("SignificantDateList should contain 'AWB Issue Date'", true, SignificantDatePermittedForShipmentsList.ContainsCode(PostDateConfigurationLookups.SignificantDateCodes.AWBIssueDate));
			AssertEquals("SignificantDateList should contain 'Delivery Date'", true, SignificantDatePermittedForShipmentsList.ContainsCode(PostDateConfigurationLookups.SignificantDateCodes.DeliveryDate));
			AssertEquals("SignificantDateList should contain 'Pickup Date'", true, SignificantDatePermittedForShipmentsList.ContainsCode(PostDateConfigurationLookups.SignificantDateCodes.PickupDate));
			AssertEquals("SignificantDateList should contain 'Job Open Date'", true, SignificantDatePermittedForShipmentsList.ContainsCode(PostDateConfigurationLookups.SignificantDateCodes.JobOpenDate));
			AssertEquals("SignificantDateList should contain 'Invoice Add Date'", true, SignificantDatePermittedForShipmentsList.ContainsCode(PostDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate));
			AssertEquals("SignificantDateList should contain 'Invoice Date'", true, SignificantDatePermittedForShipmentsList.ContainsCode(PostDateConfigurationLookups.SignificantDateCodes.InvoiceDate));
			AssertEquals("SignificantDateList should contain 'House Bill Issue Date'", true, SignificantDatePermittedForShipmentsList.ContainsCode(PostDateConfigurationLookups.SignificantDateCodes.HouseBillIssueDate));
		}

		public void TestSignificantDatePermittedForDeclarationsList()
		{
			AssertionWithHtml.CombineAssertions(delegate
			{
				AssertEquals("SignificantDateList.Count", 4, SignificantDatePermittedForDeclarationsList.Count);
				AssertEquals("SignificantDateList should contain 'Job Open Date'", true, SignificantDatePermittedForDeclarationsList.ContainsCode(PostDateConfigurationLookups.SignificantDateCodes.JobOpenDate));
				AssertEquals("SignificantDateList should contain 'Customs Clearance Date'", true, SignificantDatePermittedForDeclarationsList.ContainsCode(PostDateConfigurationLookups.SignificantDateCodes.CustomsClearanceDate));
				AssertEquals("SignificantDateList should contain 'Invoice Add Date'", true, SignificantDatePermittedForDeclarationsList.ContainsCode(PostDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate));
				AssertEquals("SignificantDateList should contain 'Invoice Date'", true, SignificantDatePermittedForDeclarationsList.ContainsCode(PostDateConfigurationLookups.SignificantDateCodes.InvoiceDate));
			});
		}

		public void TestBrokerList()
		{
			AssertEquals("BrokerList.Count", 3, BizObj.BrokerList.Count);
			AssertEquals("The BrokerList should contain 'All'", true, BizObj.BrokerList.ContainsCode(PostDateConfigurationLookups.BrokerCodes.All));
			AssertEquals("The BrokerList should contain 'Internal'", true, BizObj.BrokerList.ContainsCode(PostDateConfigurationLookups.BrokerCodes.Internal));
			AssertEquals("The BrokerList should contain 'External'", true, BizObj.BrokerList.ContainsCode(PostDateConfigurationLookups.BrokerCodes.External));
		}

		public void TestCorrectSignificantDateListUsed()
		{
			BizObj.JobType = String.Empty;
			AssertListsAreSame(BizObj.SignificantDateList, CompleteSignificantDateList);

			BizObj.JobType = "ALL";
			AssertListsAreSame(BizObj.SignificantDateList, SignificantDatePermittedForAllList);

			BizObj.JobType = "SHP";
			AssertListsAreSame(BizObj.SignificantDateList, SignificantDatePermittedForShipmentsList);

			BizObj.JobType = "FCN";
			AssertListsAreSame(BizObj.SignificantDateList, SignificantDatePermittedForConsolsList);

			BizObj.JobType = "GCN";
			AssertListsAreSame(BizObj.SignificantDateList, SignificantDatePermittedForConsolsList);

			BizObj.JobType = "BRK";
			AssertListsAreSame(BizObj.SignificantDateList, SignificantDatePermittedForDeclarationsList);

			foreach (ICodeDescription jobType in JobTypeList)
			{
				if (jobType.Code != "ALL" && jobType.Code != "SHP" && jobType.Code != "BRK" && jobType.Code != "FCN" && jobType.Code != "GCN")
				{
					BizObj.JobType = jobType.Code;
					AssertListsAreSame(BizObj.SignificantDateList, SignificantDatePermittedForOthersList);
				}
			}
		}

		public void TestPriorClosedPeriodList()
		{
			AssertEquals("PriorClosedPeriodList.Count", 5, BizObj.PriorClosedPeriodList.Count);
			AssertEquals("The PriorClosedPeriodList should contain 'InvoiceAddDate'", true, BizObj.PriorClosedPeriodList.ContainsCode(PostDateConfigurationLookups.SignificantDatePeriodCodes.InvoiceAddDate));
			AssertEquals("The PriorClosedPeriodList should contain 'EndOfPriorMonth'", true, BizObj.PriorClosedPeriodList.ContainsCode(PostDateConfigurationLookups.SignificantDatePeriodCodes.EndOfPriorMonth));
			AssertEquals("The PriorClosedPeriodList should contain 'EndOfPriorPeriod'", true, BizObj.PriorClosedPeriodList.ContainsCode(PostDateConfigurationLookups.SignificantDatePeriodCodes.EndOfPriorPeriod));
			AssertEquals("The PriorClosedPeriodList should contain 'InvoiceDate'", true, BizObj.PriorClosedPeriodList.ContainsCode(PostDateConfigurationLookups.SignificantDatePeriodCodes.InvoiceDate));
			AssertEquals("The PriorClosedPeriodList should contain 'FirstDayOfFirstOpenPeriodAfterSignificantDate'", true, BizObj.PriorClosedPeriodList.ContainsCode(PostDateConfigurationLookups.SignificantDatePeriodCodes.FirstDayOfFirstOpenPeriodAfterSignificantDate));
		}

		public void TestReversalRuleList()
		{
			AssertEquals("ReversalRuleList.Count", 3, BizObj.ReversalRuleList.Count);
			AssertEquals("The ReversalRuleList should contain 'StandardPostingRules'", true, BizObj.ReversalRuleList.ContainsCode(PostDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules));
			AssertEquals("The ReversalRuleList should contain 'DefaultFromOriginalTransactionPostDateOrCurrentDate'", true, BizObj.ReversalRuleList.ContainsCode(PostDateConfigurationLookups.ReversalRuleCodes.DefaultFromOriginalTransactionPostDateOrCurrentDate));
			AssertEquals("The ReversalRuleList should contain 'DefaultFromOriginalTransactionPostDateOrFirstDayOfFirstOpenPeriod'", true, BizObj.ReversalRuleList.ContainsCode(PostDateConfigurationLookups.ReversalRuleCodes.DefaultFromOriginalTransactionPostDateOrFirstDayOfFirstOpenPeriod));
		}

		#region Implementation

		protected abstract PostDateConfiguration GetNewBizObj { get; }

		protected override void SetUp()
		{
			base.SetUp();
			BizObj = GetNewBizObj;
		}

		PostDateConfiguration BizObj;

		CodeDescriptionPairList fSignificantDatePermittedForAllList;
		CodeDescriptionPairList SignificantDatePermittedForAllList
		{
			get { return fSignificantDatePermittedForAllList ?? (fSignificantDatePermittedForAllList = BizObj.PostDateConfigurationLookups.SignificantDatePermittedForAllList); }
		}

		CodeDescriptionPairList fCompleteSignificantDateList;
		CodeDescriptionPairList CompleteSignificantDateList
		{
			get { return fCompleteSignificantDateList ?? (fCompleteSignificantDateList = PostDateConfigurationLookups.CompleteSignificantDateList); }
		}

		CodeDescriptionPairList fSignificantDatePermittedForOthersList;
		CodeDescriptionPairList SignificantDatePermittedForOthersList
		{
			get { return fSignificantDatePermittedForOthersList ?? (fSignificantDatePermittedForOthersList = BizObj.PostDateConfigurationLookups.SignificantDatePermittedForOthersList); }
		}

		CodeDescriptionPairList fSignificantDatePermittedForConsolsList;
		CodeDescriptionPairList SignificantDatePermittedForConsolsList
		{
			get { return fSignificantDatePermittedForConsolsList ?? (fSignificantDatePermittedForConsolsList = BizObj.PostDateConfigurationLookups.SignificantDatePermittedForConsolsList); }
		}

		CodeDescriptionPairList fSignificantDatePermittedForShipmentsList;
		CodeDescriptionPairList SignificantDatePermittedForShipmentsList
		{
			get { return fSignificantDatePermittedForShipmentsList ?? (fSignificantDatePermittedForShipmentsList = BizObj.PostDateConfigurationLookups.SignificantDatePermittedForShipmentsList); }
		}

		CodeDescriptionPairList fSignificantDatePermittedForDeclarationsList;
		CodeDescriptionPairList SignificantDatePermittedForDeclarationsList
		{
			get { return fSignificantDatePermittedForDeclarationsList ?? (fSignificantDatePermittedForDeclarationsList = BizObj.PostDateConfigurationLookups.SignificantDatePermittedForDeclarationsList); }
		}

		void AssertListsAreSame(CodeDescriptionPairList list1, CodeDescriptionPairList list2)
		{
			AssertEquals("Number of items is different", list1.Count, list2.Count);
			foreach (ICodeDescription item in list1)
			{
				Assert("Lists are different", list2.Contains(item));
			}
		}

		CodeDescriptionPairList fJobTypeList;
		CodeDescriptionPairList JobTypeList
		{
			get { return fJobTypeList ?? (fJobTypeList = BizObj.JobTypeList); }
		}

		#endregion

	}
}