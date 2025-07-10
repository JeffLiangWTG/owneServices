using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.EU.TemporaryStorage.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Module.Testing
{
	[TestedType(typeof(SumAFilterStripBusinessObject))]
	class SumAFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestJobNumber()
		{
			AssertModuleTextFilter(CusTempStorageJobHeaderSchema.Constants.SJH_JobReference, "Job #");
		}

		public void TestCustomer()
		{
			var customer1 = Factory.NewWithValidTestData<OrgHeader>();
			var customer2 = Factory.NewWithValidTestData<OrgHeader>();

			var header1 = GetNewTempStorageJobHeader();
			header1.SJH_OH_Customer = customer1.PK;

			var header2 = GetNewTempStorageJobHeader();
			header2.SJH_OH_Customer = customer2.PK;

			Factory.Save();

			var filter = new SumAFilterStripBusinessObject();
			var customerFilter = (ModuleGuidFilter)filter["Customer"];

			customerFilter.Property = ZGuid.Empty;
			customerFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("Empty filter->header1", true, header1.MatchesFilter(filter.Filter));
				AssertEquals("Empty filter->header2", true, header2.MatchesFilter(filter.Filter));

				customerFilter.Property = customer2.PK;
				AssertEquals("Filter customer2->header1", false, header1.MatchesFilter(filter.Filter));
				AssertEquals("Filter customer2->header2", true, header2.MatchesFilter(filter.Filter));
			});
		}

		[TestDate(2017, 10, 30)]
		public void TestArrivalDate()
		{
			var header1 = GetNewTempStorageJobHeader();
			header1.SJH_ArrivalDate = ZDate.Today;

			var header2 = GetNewTempStorageJobHeader();
			header2.SJH_ArrivalDate = ZDate.Today.AddDays(1);

			Factory.Save();

			var filter = new SumAFilterStripBusinessObject();
			var arrivalDateFilter = (ModuleDateFilter)filter["Arrival Date"];
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = ZDate.Today;
			arrivalDateFilter.Property2 = ZDate.Today;
			arrivalDateFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("Filter today->header1", true, header1.MatchesFilter(filter.Filter));
				AssertEquals("Filter today->header2", false, header2.MatchesFilter(filter.Filter));

				arrivalDateFilter.Property1 = ZDate.Today.AddDays(1);
				arrivalDateFilter.Property2 = ZDate.Today.AddDays(1);

				AssertEquals("Filter tomorrow->header1", false, header1.MatchesFilter(filter.Filter));
				AssertEquals("Filter tomorrow->header2", true, header2.MatchesFilter(filter.Filter));
			});
		}

		[TestDate(2017, 10, 30)]
		public void TestPresentationDate()
		{
			var header1 = GetNewTempStorageJobHeader();
			header1.SJH_PresentationDate = ZDate.Today;

			var header2 = GetNewTempStorageJobHeader();
			header2.SJH_PresentationDate = ZDate.Today.AddDays(1);

			Factory.Save();

			var filter = new SumAFilterStripBusinessObject();
			var arrivalDateFilter = (ModuleDateFilter)filter["Presentation Date"];
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = ZDate.Today;
			arrivalDateFilter.Property2 = ZDate.Today;
			arrivalDateFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("Filter today->header1", true, header1.MatchesFilter(filter.Filter));
				AssertEquals("Filter today->header2", false, header2.MatchesFilter(filter.Filter));

				arrivalDateFilter.Property1 = ZDate.Today.AddDays(1);
				arrivalDateFilter.Property2 = ZDate.Today.AddDays(1);

				AssertEquals("Filter tomorrow->header1", false, header1.MatchesFilter(filter.Filter));
				AssertEquals("Filter tomorrow->header2", true, header2.MatchesFilter(filter.Filter));
			});
		}

		public void TestTransportMode()
		{
			AssertModuleTextFilter(CusTempStorageJobHeaderSchema.Constants.SJH_TransportMode, "Transport Mode");
		}

		public void TestApplicationCode()
		{
			AssertModuleTextFilter(CusTempStorageJobHeaderSchema.Constants.SJH_AppCode, "Application Code");
		}

		public void TestCustomsOffice()
		{
			AssertModuleTextFilter(CusTempStorageJobHeaderSchema.Constants.SJH_CustomsOffice, "Customs Office");
		}

		public void TestLoading()
		{
			var header1 = GetNewTempStorageJobHeader();
			header1.SJH_RL_NKLoading = "ABC";

			var header2 = GetNewTempStorageJobHeader();
			header2.SJH_RL_NKLoading = "DEF";

			Factory.Save();

			var filter = new SumAFilterStripBusinessObject();
			var customsOfficeFilter = (ModuleNkFilter)filter["Loading"];

			customsOfficeFilter.Property = ZString.Empty;
			customsOfficeFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("Empty filter->header1", true, header1.MatchesFilter(filter.Filter));
				AssertEquals("Empty filter->header2", true, header2.MatchesFilter(filter.Filter));

				customsOfficeFilter.Property = "DEF";
				AssertEquals("Filter DEF->header1", false, header1.MatchesFilter(filter.Filter));
				AssertEquals("Filter DEF->header2", true, header2.MatchesFilter(filter.Filter));
			});
		}

		public void TestReferenceNumber()
		{
			AssertModuleTextFilter(CusTempStorageJobHeaderSchema.Constants.SJH_ReferenceNumber, "Reference Number");
		}

		public void TestPreviousReferenceNumber()
		{
			AssertModuleTextFilter(CusTempStorageJobHeaderSchema.Constants.SJH_PreviousReferenceNumber, "Previous Reference Number");
		}

		public void TestCurrentDeclarationType()
		{
			var header1 = GetNewTempStorageJobHeader();
			CreateNewTempStorageDec(header1, type: TemporaryStorageDeclarationTypeList.Codes.ChangeCustodyInformation);
			var header1Job2 = CreateNewTempStorageDec(header1, type: TemporaryStorageDeclarationTypeList.Codes.CustomsPresentationLedger);
			CreateNewTempStorageDec(header1, type: TemporaryStorageDeclarationTypeList.Codes.ChangeDisposalEntitledTrader);

			var header2 = GetNewTempStorageJobHeader();
			CreateNewTempStorageDec(header2, type: TemporaryStorageDeclarationTypeList.Codes.CustomsPresentationLedger);
			var header2Job2 = CreateNewTempStorageDec(header2, type: TemporaryStorageDeclarationTypeList.Codes.ChangeCustodyInformation);
			CreateNewTempStorageDec(header2, type: TemporaryStorageDeclarationTypeList.Codes.ChangeDisposalEntitledTrader);

			var header3 = GetNewTempStorageJobHeader();
			CreateNewTempStorageDec(header3, type: TemporaryStorageDeclarationTypeList.Codes.ChangeCustodyInformation);
			var header3Job2 = CreateNewTempStorageDec(header3, type: TemporaryStorageDeclarationTypeList.Codes.CustomsPresentationLedger);
			CreateNewTempStorageDec(header3, type: TemporaryStorageDeclarationTypeList.Codes.ChangeDisposalEntitledTrader);
			Factory.Save();

			header1Job2.STH_AdditionalInformation = "STH_SystemLastEditTimeUtc is smalldatetime, so need to add at least one minute";
			PropertyChangeSubscription.PropertyChanged += IncrementSystemLastEditTime;
			Factory.Save();

			header2Job2.STH_AdditionalInformation = "STH_SystemLastEditTimeUtc is smalldatetime, so need to add at least one minute";
			PropertyChangeSubscription.PropertyChanged += IncrementSystemLastEditTime;
			Factory.Save();

			header3Job2.STH_AdditionalInformation = "STH_SystemLastEditTimeUtc is smalldatetime, so need to add at least one minute";
			PropertyChangeSubscription.PropertyChanged += IncrementSystemLastEditTime;
			Factory.Save();

			var filter = new SumAFilterStripBusinessObject();
			var declarationTypeFilter = (ModuleTextFilter)filter["Current Declaration Type"];
			declarationTypeFilter.Property = ZString.Empty;
			declarationTypeFilter.IsActive = true;

			void AssertMatch(ZString message, bool match1, bool match2, bool match3)
			{
				AssertEquals(message + "->header1", match1, header1.MatchesFilter(filter.Filter));
				AssertEquals(message + "->header2", match2, header2.MatchesFilter(filter.Filter));
				AssertEquals(message + "->header3", match3, header3.MatchesFilter(filter.Filter));
			}

			CombineAssertions(() =>
			{
				AssertMatch("Empty filter", true, true, true);

				declarationTypeFilter.Property = TemporaryStorageDeclarationTypeList.Codes.CustomsPresentationLedger;
				AssertMatch("Filter for type 'CUSPRL'", true, false, true);

				declarationTypeFilter.Property = TemporaryStorageDeclarationTypeList.Codes.ChangeCustodyInformation;
				AssertMatch("Filter for type 'CHGTST'", false, true, false);

				declarationTypeFilter.Property = TemporaryStorageDeclarationTypeList.Codes.ChangeDisposalEntitledTrader;
				AssertMatch("Filter for type 'CHGOFF'", false, false, false);
			});
		}

		public void TestCurrentMessageStatus()
		{
			var header1 = GetNewTempStorageJobHeader();
			CreateNewTempStorageDec(header1, messageStatus: EDIMessageStatusList.Codes.Failed);
			var header1Job2 = CreateNewTempStorageDec(header1, messageStatus: EDIMessageStatusList.Codes.Queued);
			CreateNewTempStorageDec(header1, messageStatus: EDIMessageStatusList.Codes.Withdrawn);

			var header2 = GetNewTempStorageJobHeader();
			CreateNewTempStorageDec(header2, messageStatus: EDIMessageStatusList.Codes.Queued);
			var header2Job2 = CreateNewTempStorageDec(header2, messageStatus: EDIMessageStatusList.Codes.Failed);
			CreateNewTempStorageDec(header2, messageStatus: EDIMessageStatusList.Codes.Withdrawn);

			var header3 = GetNewTempStorageJobHeader();
			CreateNewTempStorageDec(header3, messageStatus: EDIMessageStatusList.Codes.Withdrawn);
			var header3Job2 = CreateNewTempStorageDec(header3, messageStatus: EDIMessageStatusList.Codes.Queued);
			CreateNewTempStorageDec(header3, messageStatus: EDIMessageStatusList.Codes.Failed);
			Factory.Save();

			header1Job2.STH_AdditionalInformation = "STH_SystemLastEditTimeUtc is smalldatetime, so need to add at least one minute";
			PropertyChangeSubscription.PropertyChanged += IncrementSystemLastEditTime;
			Factory.Save();

			header2Job2.STH_AdditionalInformation = "STH_SystemLastEditTimeUtc is smalldatetime, so need to add at least one minute";
			PropertyChangeSubscription.PropertyChanged += IncrementSystemLastEditTime;
			Factory.Save();

			header3Job2.STH_AdditionalInformation = "STH_SystemLastEditTimeUtc is smalldatetime, so need to add at least one minute";
			PropertyChangeSubscription.PropertyChanged += IncrementSystemLastEditTime;
			Factory.Save();

			var filter = new SumAFilterStripBusinessObject();
			var messageStatusFilter = (ModuleTextFilter)filter["Current Message Status"];
			messageStatusFilter.Property = ZString.Empty;
			messageStatusFilter.IsActive = true;

			void AssertMatch(ZString message, bool match1, bool match2, bool match3)
			{
				AssertEquals(message + "->header1", match1, header1.MatchesFilter(filter.Filter));
				AssertEquals(message + "->header2", match2, header2.MatchesFilter(filter.Filter));
				AssertEquals(message + "->header3", match3, header3.MatchesFilter(filter.Filter));
			}

			CombineAssertions(() =>
			{
				AssertMatch("Empty filter", true, true, true);

				messageStatusFilter.Property = EDIMessageStatusList.Codes.Queued;
				AssertMatch("Filter for type 'QUE'", true, false, true);

				messageStatusFilter.Property = EDIMessageStatusList.Codes.Failed;
				AssertMatch("Filter for message status 'FAL'", false, true, false);

				messageStatusFilter.Property = EDIMessageStatusList.Codes.Withdrawn;
				AssertMatch("Filter for type 'WDW'", false, false, false);
			});
		}

		public void TestCurrentRegistrationNumber()
		{
			var header1 = GetNewTempStorageJobHeader();
			CreateNewTempStorageDec(header1, registrationNumber: "AB11111111");
			var header1Job2 = CreateNewTempStorageDec(header1, registrationNumber: "CD22222222");
			CreateNewTempStorageDec(header1, registrationNumber: "RN33333333");

			var header2 = GetNewTempStorageJobHeader();
			CreateNewTempStorageDec(header2, registrationNumber: "CD22222222");
			var header2Job2 = CreateNewTempStorageDec(header2, registrationNumber: "AB11111111");
			CreateNewTempStorageDec(header2, registrationNumber: "RN33333333");

			var header3 = GetNewTempStorageJobHeader();
			CreateNewTempStorageDec(header3, registrationNumber: "AB11111111");
			var header3Job2 = CreateNewTempStorageDec(header3, registrationNumber: "CD22222222");
			CreateNewTempStorageDec(header3, registrationNumber: "RN33333333");
			Factory.Save();

			header1Job2.STH_AdditionalInformation = "STH_SystemLastEditTimeUtc is smalldatetime, so need to add at least one minute";
			PropertyChangeSubscription.PropertyChanged += IncrementSystemLastEditTime;
			Factory.Save();

			header2Job2.STH_AdditionalInformation = "STH_SystemLastEditTimeUtc is smalldatetime, so need to add at least one minute";
			PropertyChangeSubscription.PropertyChanged += IncrementSystemLastEditTime;
			Factory.Save();

			header3Job2.STH_AdditionalInformation = "STH_SystemLastEditTimeUtc is smalldatetime, so need to add at least one minute";
			PropertyChangeSubscription.PropertyChanged += IncrementSystemLastEditTime;
			Factory.Save();

			var filter = new SumAFilterStripBusinessObject();
			var registrationNumberFilter = (ModuleTextFilter)filter["Current Registration No."];
			registrationNumberFilter.Property = ZString.Empty;
			registrationNumberFilter.IsActive = true;

			void AssertMatch(ZString message, bool match1, bool match2, bool match3)
			{
				AssertEquals(message + "->header1", match1, header1.MatchesFilter(filter.Filter));
				AssertEquals(message + "->header2", match2, header2.MatchesFilter(filter.Filter));
				AssertEquals(message + "->header3", match3, header3.MatchesFilter(filter.Filter));
			}

			CombineAssertions(() =>
			{
				AssertMatch("Empty filter", true, true, true);

				registrationNumberFilter.Property = "CD22222222";
				AssertMatch("Filter for registration 'CD22222222'", true, false, true);

				registrationNumberFilter.Property = "AB11111111";
				AssertMatch("Filter for registration 'AB11111111'", false, true, false);

				registrationNumberFilter.Property = "RN33333333";
				AssertMatch("Filter for registration 'RN33333333'", false, false, false);
			});
		}

		public void TestBranch()
		{
			var org1 = Factory.NewWithValidTestData<GlbCompany>();
			var org2 = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = org1.Branches.AddNew();
			branch1.FillWithValidTestData();
			var branch2 = org1.Branches.AddNew();
			branch2.FillWithValidTestData();
			var branch3 = org2.Branches.AddNew();
			branch3.FillWithValidTestData();
			Factory.Save();

			var header1 = GetNewTempStorageJobHeader();
			header1.SJH_GB = branch1.PK;
			var header2 = GetNewTempStorageJobHeader();
			header2.SJH_GB = branch2.PK;
			Factory.Save();

			var filter = new SumAFilterStripBusinessObject();
			var branchFilter = (ModuleGuidFilter)filter["Branch"];

			branchFilter.Property = ZGuid.Empty;
			branchFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("Empty filter->header1", true, header1.MatchesFilter(filter.Filter));
				AssertEquals("Empty filter->header2", true, header2.MatchesFilter(filter.Filter));

				branchFilter.Property = branch2.PK;
				AssertEquals("Filter branch2->header1", false, header1.MatchesFilter(filter.Filter));
				AssertEquals("Filter branch2->header2", true, header2.MatchesFilter(filter.Filter));
			});
		}

		void AssertModuleTextFilter(string propertyName, string filterName)
		{
			var header1 = GetNewTempStorageJobHeader();
			header1[propertyName] = "ABC";

			var header2 = GetNewTempStorageJobHeader();
			header2[propertyName] = "DEF";

			Factory.Save();

			var filter = new SumAFilterStripBusinessObject();
			var customsOfficeFilter = (ModuleTextFilter)filter[filterName];

			customsOfficeFilter.Property = ZString.Empty;
			customsOfficeFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("Empty filter->header1", true, header1.MatchesFilter(filter.Filter));
				AssertEquals("Empty filter->header2", true, header2.MatchesFilter(filter.Filter));

				customsOfficeFilter.Property = "DEF";
				AssertEquals("Filter DEF->header1", false, header1.MatchesFilter(filter.Filter));
				AssertEquals("Filter DEF->header2", true, header2.MatchesFilter(filter.Filter));
			});
		}

		CusTempStorageJobHeader GetNewTempStorageJobHeader()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			header.SJH_GB = GlbBranch.CurrentBranch.PK;
			header.SJH_AppCode = TemporaryStorageApplicationCodeList.Codes.SumA;
			return header;
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var result = new List<Tuple<string, string>>();

			result.Add(TableFilter(CusTempStorageJobHeaderSchema.Constants.TableName, "Loading"));

			result.AddRange(base.GetFiltersExcludedFromSubgroupCheckForCommonTables());
			return result;
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheck()
		{
			var result = new List<Tuple<string, string>>();

			result.Add(TableFilter(CusTempStorageJobHeaderSchema.Constants.TableName, "Loading"));

			result.AddRange(base.GetFiltersExcludedFromSubgroupCheckForCommonTables());
			return result;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new SumAFilterStripBusinessObject();

		static CusTempStorageDec CreateNewTempStorageDec(CusTempStorageJobHeader header, string type = TemporaryStorageDeclarationTypeList.Codes.ChangeOfSpecificOrderTerm, string messageStatus = EDIMessageStatusList.Codes.ProcessedOK, string registrationNumber = "")
		{
			var dec = header.PRLCONCusTempStorageDecs.AddNew();
			dec.STH_DeclarationType = type;
			dec.STH_MessageStatus = messageStatus;

			if (!string.IsNullOrEmpty(registrationNumber))
			{
				dec.CusEntryNumber.CE_EntryNum = registrationNumber;
			}

			return dec;
		}

		static void IncrementSystemLastEditTime(object sender, ZPropertyValueChangedEventArgs e)
		{
			if (e.Property.Name == "STH_SystemLastEditTimeUtc")
			{
				PropertyChangeSubscription.PropertyChanged -= IncrementSystemLastEditTime;
				e.Property.Value = ((ZDateTime)e.OldValue).AddMinutes(1);
			}
		}

		public void TestGetFilterInflators()
		{
			Type[] expectedFilterInflatorTypes =
			[
				typeof(ApplicationCodeFilterInflator),
				typeof(ArrivalDateFilterInflator),
				typeof(BranchFilterInflator),
				typeof(CurrentDeclarationTypeFilterInflator),
				typeof(CurrentMessageStatusFilterInflator),
				typeof(CurrentRegistrationNumberFilterInflator),
				typeof(CustomerFilterInflator),
				typeof(CustomsOfficeFilterInflator),
				typeof(JobNumberFilterInflator),
				typeof(LoadingFilterInflator),
				typeof(PresentationDateFilterInflator),
				typeof(PreviousReferenceNumberFilterInflator),
				typeof(ReferenceNumberFilterInflator),
				typeof(TransportModeFilterInflator)
			];
			var filterStrip = new SumAFilterStripBusinessObjectForTest();
			var filterInflators = filterStrip.GetFilterInflators_Exposed();
			AssertContainsExactElementsInAnyOrder(expectedFilterInflatorTypes, filterInflators.Select(inf => inf.GetType()));
		}

		sealed class SumAFilterStripBusinessObjectForTest : SumAFilterStripBusinessObject
		{
			public List<IFilterInflator> GetFilterInflators_Exposed() => GetFilterInflators();
		}
	}
}
