using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Module.Testing
{
	[TestedType(typeof(CusReconDeclarationFilterStripBusinessObject))]
	sealed class CusReconDeclarationFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new CusReconDeclarationFilterStripBusinessObject();

		public void TestFilterNumbersAndReferencesGroup()
		{
			var filter = new CusReconDeclarationFilterStripBusinessObject();
			var numberAndReferenceGroup = filter.Where(x => x.Category == FilterCategories.NumbersAndReferences);
			AssertEquals(4, numberAndReferenceGroup.Count());
			AssertNumberAndReferenceGroup(CusReconDeclarationFilterStripBusinessObject.Schema.JobNumber, 6);
			AssertNumberAndReferenceGroup(CusReconDeclarationFilterStripBusinessObject.Schema.ImportEntryNumber, 6);
			AssertNumberAndReferenceGroup(CusReconDeclarationFilterStripBusinessObject.Schema.RefundDeclarationNumber, 6);
			AssertNumberAndReferenceGroup(CusReconDeclarationFilterStripBusinessObject.Schema.RefundApprovalNo, 8);

			void AssertNumberAndReferenceGroup(string filterName, int operatorCount)
			{
				var moduleNumberFilter = (ModuleNumberFilter)filter[filterName];
				Assert(numberAndReferenceGroup.Contains(moduleNumberFilter));

				AssertEquals(operatorCount, moduleNumberFilter.ComparisonOperator_List.Count);
				Assert(moduleNumberFilter.ComparisonOperator_List.ContainsCode(ModuleNumberFilter.ComparisonConstants.Exact));
				Assert(moduleNumberFilter.ComparisonOperator_List.ContainsCode(ModuleNumberFilter.ComparisonConstants.StartsWith));
				Assert(moduleNumberFilter.ComparisonOperator_List.ContainsCode(ModuleNumberFilter.ComparisonConstants.Contains));
				Assert(moduleNumberFilter.ComparisonOperator_List.ContainsCode(ModuleNumberFilter.ComparisonConstants.NotEqual));
				Assert(moduleNumberFilter.ComparisonOperator_List.ContainsCode(ModuleNumberFilter.ComparisonConstants.NotStartsWith));
				Assert(moduleNumberFilter.ComparisonOperator_List.ContainsCode(ModuleNumberFilter.ComparisonConstants.NotContain));
				if (filterName == CusReconDeclarationFilterStripBusinessObject.Schema.RefundApprovalNo)
				{
					Assert(moduleNumberFilter.ComparisonOperator_List.ContainsCode(ModuleNumberFilter.ComparisonConstants.IsBlank));
					Assert(moduleNumberFilter.ComparisonOperator_List.ContainsCode(ModuleNumberFilter.ComparisonConstants.IsNotBlank));
				}

				AssertEquals(ModuleNumberFilter.ComparisonConstants.StartsWith, moduleNumberFilter.ComparisonOperator_List.DefaultCode);
			}
		}

		public void TestFilterStatusAndFlagsGroup()
		{
			var filter = new CusReconDeclarationFilterStripBusinessObject();
			var statusAndFlagsGroup = filter.Where(x => x.Category == FilterCategories.StatusAndFlags);
			AssertEquals(2, statusAndFlagsGroup.Count());
			AssertStatusAndFlagsGroup(CusReconDeclarationFilterStripBusinessObject.Schema.MessageStatus);
			AssertStatusAndFlagsGroup(CusReconDeclarationFilterStripBusinessObject.Schema.EntryStatus);

			void AssertStatusAndFlagsGroup(string filterName)
			{
				var moduleTextFilter = (ModuleTextFilter)filter[filterName];
				Assert(statusAndFlagsGroup.Contains(moduleTextFilter));

				AssertEquals(4, moduleTextFilter.ComparisonOperator_List.Count);
				Assert(moduleTextFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.Exact));
				Assert(moduleTextFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.NotEqual));
				Assert(moduleTextFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsBlank));
				Assert(moduleTextFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsNotBlank));

				AssertEquals(ModuleTextFilter.ComparisonConstants.Exact, moduleTextFilter.ComparisonOperator_List.DefaultCode);
			}
		}

		public void TestFilterLocationGroup()
		{
			var filter = new CusReconDeclarationFilterStripBusinessObject();
			var locationsGroup = filter.Where(x => x.Category == FilterCategories.Locations);
			AssertEquals(1, locationsGroup.Count());

			var moduleNkFilter = (ModuleNkFilter)filter[CusReconDeclarationFilterStripBusinessObject.Schema.CustomsOffice];
			Assert(locationsGroup.Contains(moduleNkFilter));

			AssertEquals(2, moduleNkFilter.ComparisonOperator_List.Count);
			Assert(moduleNkFilter.ComparisonOperator_List.ContainsCode(ModuleNkFilter.ComparisonConstants.Exact));
			Assert(moduleNkFilter.ComparisonOperator_List.ContainsCode(ModuleNkFilter.ComparisonConstants.NotEqual));

			AssertEquals(ModuleNkFilter.ComparisonConstants.Exact, moduleNkFilter.ComparisonOperator_List.DefaultCode);
		}

		public void TestFilterOrganisationsGroup()
		{
			var filter = new CusReconDeclarationFilterStripBusinessObject();
			var organisationsGroup = filter.Where(x => x.Category == FilterCategories.Organisations);
			AssertEquals(1, organisationsGroup.Count());

			var moduleGuidFilter = (ModuleGuidFilter)filter[CusReconDeclarationFilterStripBusinessObject.Schema.Payer];
			Assert(organisationsGroup.Contains(moduleGuidFilter));

			AssertEquals(5, moduleGuidFilter.ComparisonOperator_List.Count);
			Assert(moduleGuidFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.Exact));
			Assert(moduleGuidFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.NotEqual));
			Assert(moduleGuidFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsBlank));
			Assert(moduleGuidFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsNotBlank));
			Assert(moduleGuidFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.FiltersMatch));

			AssertEquals(ModuleTextFilter.ComparisonConstants.Exact, moduleGuidFilter.ComparisonOperator_List.DefaultCode);
		}

		public void TestFilterDatesGroup()
		{
			var filter = new CusReconDeclarationFilterStripBusinessObject();
			var datesGroup = filter.Where(x => x.Category == FilterCategories.Dates);
			AssertEquals(2, datesGroup.Count());
			AssertDatesGroup(CusReconDeclarationFilterStripBusinessObject.Schema.AcceptedDate);
			AssertDatesGroup(CusReconDeclarationFilterStripBusinessObject.Schema.RefundApprovalDate);

			void AssertDatesGroup(string filterName)
			{
				var moduleDateFilter = (ModuleDateFilter)filter[filterName];
				Assert(datesGroup.Contains(moduleDateFilter));
				AssertDateFilter(moduleDateFilter, 40);
			}
		}

		public void TestFilterAuditInformation()
		{
			var filter = new CusReconDeclarationFilterStripBusinessObject();
			var auditInformationGroup = filter.Where(x => x.Category == FilterCategories.AuditInformation);
			AssertEquals(7, auditInformationGroup.Count());

			var moduleTextFilter = (ModuleTextFilter)filter[FilterDescriptions.CreatedOnWeb];
			AssertEquals(3, moduleTextFilter.List.Count);

			var list = (ReadOnlyCodeDescriptionPairList)moduleTextFilter.List;
			list.ContainsCode("ALL");
			list.ContainsCode("WEB");
			list.ContainsCode("ENT");
			AssertEquals("ALL", moduleTextFilter.DefaultProperty);

			AssertText(FilterDescriptions.CreatingUser);
			AssertDate(FilterDescriptions.CreatedTime);
			AssertText(FilterDescriptions.LastEditUser);
			AssertDate(FilterDescriptions.LastEditTime);

			void AssertDate(string filterName)
			{
				var moduleDateFilter = (ModuleDateFilter)filter[filterName];
				Assert(auditInformationGroup.Contains(moduleDateFilter));
				AssertDateFilter(moduleDateFilter, 36);

				if (filter == FilterDescriptions.CreatedTime)
				{
					AssertEquals(ModuleDateFilter.DateRangeSearchTexts.Last3Mths, moduleDateFilter.PropertySearch);
				}
			}

			void AssertText(string filterName)
			{
				var moduleNkFilter = (ModuleNkFilter)filter[filterName];
				Assert(auditInformationGroup.Contains(moduleNkFilter));

				AssertEquals(6, moduleNkFilter.ComparisonOperator_List.Count);
				Assert(moduleNkFilter.ComparisonOperator_List.ContainsCode(ModuleNkFilter.ComparisonConstants.Exact));
				Assert(moduleNkFilter.ComparisonOperator_List.ContainsCode(ModuleNkFilter.ComparisonConstants.NotEqual));
				Assert(moduleNkFilter.ComparisonOperator_List.ContainsCode(ModuleNkFilter.ComparisonConstants.IsBlank));
				Assert(moduleNkFilter.ComparisonOperator_List.ContainsCode(ModuleNkFilter.ComparisonConstants.IsNotBlank));
				Assert(moduleNkFilter.ComparisonOperator_List.ContainsCode(ModuleNkFilter.ComparisonConstants.CurrentUser));
				Assert(moduleNkFilter.ComparisonOperator_List.ContainsCode(ModuleNkFilter.ComparisonConstants.FiltersMatch));
			}
		}

		void AssertDateFilter(ModuleDateFilter moduleDateFilter, int listCount)
		{
			AssertEquals(listCount, moduleDateFilter.PropertySearch_List.Count);

			AssertEquals(moduleDateFilter.PropertySearch_List[0].Code, ZString.Empty);
			AssertEquals(moduleDateFilter.PropertySearch_List[1].Code, ModuleDateFilter.DateRangeSearchTexts.Today);
			AssertEquals(moduleDateFilter.PropertySearch_List[2].Code, ModuleDateFilter.DateRangeSearchTexts.ThisWeek);
			AssertEquals(moduleDateFilter.PropertySearch_List[3].Code, ZString.Empty);
			AssertEquals(moduleDateFilter.PropertySearch_List[4].Code, "Past Dates Category");
			AssertEquals(moduleDateFilter.PropertySearch_List[5].Code, ModuleDateFilter.DateRangeSearchTexts.Yesterday);
			AssertEquals(moduleDateFilter.PropertySearch_List[6].Code, ModuleDateFilter.DateRangeSearchTexts.LastWeek);
			AssertEquals(moduleDateFilter.PropertySearch_List[7].Code, ModuleDateFilter.DateRangeSearchTexts.Last7Days);
			AssertEquals(moduleDateFilter.PropertySearch_List[8].Code, ModuleDateFilter.DateRangeSearchTexts.Last14Days);
			AssertEquals(moduleDateFilter.PropertySearch_List[9].Code, ModuleDateFilter.DateRangeSearchTexts.LastMonth);
			AssertEquals(moduleDateFilter.PropertySearch_List[10].Code, ModuleDateFilter.DateRangeSearchTexts.LastCalendarMonth);
			AssertEquals(moduleDateFilter.PropertySearch_List[11].Code, ModuleDateFilter.DateRangeSearchTexts.Last2Mths);
			AssertEquals(moduleDateFilter.PropertySearch_List[12].Code, ModuleDateFilter.DateRangeSearchTexts.Last3Mths);
			AssertEquals(moduleDateFilter.PropertySearch_List[13].Code, ModuleDateFilter.DateRangeSearchTexts.Last6Mths);
			AssertEquals(moduleDateFilter.PropertySearch_List[14].Code, ModuleDateFilter.DateRangeSearchTexts.Last12Mths);
			AssertEquals(moduleDateFilter.PropertySearch_List[15].Code, ModuleDateFilter.Past);
			AssertEquals(moduleDateFilter.PropertySearch_List[16].Code, ZString.Empty);
			AssertEquals(moduleDateFilter.PropertySearch_List[17].Code, "Future Dates Category");
			AssertEquals(moduleDateFilter.PropertySearch_List[18].Code, ModuleDateFilter.DateRangeSearchTexts.Tomorrow);
			AssertEquals(moduleDateFilter.PropertySearch_List[19].Code, ModuleDateFilter.DateRangeSearchTexts.NextWeek);
			AssertEquals(moduleDateFilter.PropertySearch_List[20].Code, ModuleDateFilter.DateRangeSearchTexts.Next7Days);
			AssertEquals(moduleDateFilter.PropertySearch_List[21].Code, ModuleDateFilter.DateRangeSearchTexts.Next14Days);
			AssertEquals(moduleDateFilter.PropertySearch_List[22].Code, ModuleDateFilter.DateRangeSearchTexts.NextMonth);
			AssertEquals(moduleDateFilter.PropertySearch_List[23].Code, ModuleDateFilter.DateRangeSearchTexts.NextCalendarMonth);
			AssertEquals(moduleDateFilter.PropertySearch_List[24].Code, ModuleDateFilter.DateRangeSearchTexts.Next2Mths);
			AssertEquals(moduleDateFilter.PropertySearch_List[25].Code, ModuleDateFilter.DateRangeSearchTexts.Next3Mths);
			AssertEquals(moduleDateFilter.PropertySearch_List[26].Code, ModuleDateFilter.DateRangeSearchTexts.Next6Mths);
			AssertEquals(moduleDateFilter.PropertySearch_List[27].Code, ModuleDateFilter.DateRangeSearchTexts.Next12Mths);
			AssertEquals(moduleDateFilter.PropertySearch_List[28].Code, ModuleDateFilter.Future);
			AssertEquals(moduleDateFilter.PropertySearch_List[29].Code, ZString.Empty);
			AssertEquals(moduleDateFilter.PropertySearch_List[30].Code, "Ranges Category");
			AssertEquals(moduleDateFilter.PropertySearch_List[31].Code, ModuleDateFilter.SpecifiedDateRange);
			AssertEquals(moduleDateFilter.PropertySearch_List[32].Code, ModuleDateFilter.SpecifiedDateTimeRange);
			AssertEquals(moduleDateFilter.PropertySearch_List[33].Code, ModuleDateFilter.SpecifiedHourOffsetRange);
			AssertEquals(moduleDateFilter.PropertySearch_List[34].Code, ModuleDateFilter.SpecifiedWorkHourOffsetRange);
			AssertEquals(moduleDateFilter.PropertySearch_List[35].Code, ModuleDateFilter.SpecifiedDayOffsetRange);

			if (listCount == 40)
			{
				AssertEquals(moduleDateFilter.PropertySearch_List[36].Code, ZString.Empty);
				AssertEquals(moduleDateFilter.PropertySearch_List[37].Code, "Date Entered Category");
				AssertEquals(moduleDateFilter.PropertySearch_List[38].Code, ModuleDateFilter.HasDateEntered);
				AssertEquals(moduleDateFilter.PropertySearch_List[39].Code, ModuleDateFilter.HasNoDateEntered);
			}
		}

		public void TestFilterOtherGroup()
		{
			var filter = new CusReconDeclarationFilterStripBusinessObject();
			var otherGroup = filter.Where(x => x.Category == FilterCategories.Other);
			AssertEquals(1, otherGroup.Count());
			Assert(otherGroup.Contains(filter["Custom SQL Filter"]));
		}

		public void TestSearchNumberAndReferencesGroup()
		{
			SetUpData();
			var filter = new CusReconDeclarationFilterStripBusinessObject();
			var importEntryNumberFilter = (ModuleTextFilter)filter[CusReconDeclarationFilterStripBusinessObject.Schema.ImportEntryNumber];
			importEntryNumberFilter.Property = "40615220000002U";
			importEntryNumberFilter.IsActive = true;

			var coll = new Customs.Business.CusReconDeclarationCollection<CusReconDeclaration>(Factory, "KRC");
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("B00178566", coll[0].CRD_JobReferenceNumber);

			filter = new CusReconDeclarationFilterStripBusinessObject();
			var refundDeclarationNumberFilter = (ModuleTextFilter)filter[CusReconDeclarationFilterStripBusinessObject.Schema.RefundDeclarationNumber];
			refundDeclarationNumberFilter.Property = "40615220000005U";
			refundDeclarationNumberFilter.IsActive = true;

			coll = new Customs.Business.CusReconDeclarationCollection<CusReconDeclaration>(Factory, "KRC");
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("B00178567", coll[0].CRD_JobReferenceNumber);

			filter = new CusReconDeclarationFilterStripBusinessObject();
			var refundApprovalNoFilter = (ModuleTextFilter)filter[CusReconDeclarationFilterStripBusinessObject.Schema.RefundApprovalNo];
			refundApprovalNoFilter.Property = "030641304545";
			refundApprovalNoFilter.IsActive = true;

			coll = new Customs.Business.CusReconDeclarationCollection<CusReconDeclaration>(Factory, "KRC");
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("B00178566", coll[0].CRD_JobReferenceNumber);
		}

		public void TestSearchOrganisationsGroup()
		{
			SetUpData();
			var filter = new CusReconDeclarationFilterStripBusinessObject();
			var payerFilter = (ModuleGuidFilter)filter[CusReconDeclarationFilterStripBusinessObject.Schema.Payer];
			payerFilter.Property = payerAddress.Header.PK;
			payerFilter.IsActive = true;

			var coll = new Customs.Business.CusReconDeclarationCollection<CusReconDeclaration>(Factory, "KRC");
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("B00178567", coll[0].CRD_JobReferenceNumber);
		}

		public void TestSearchDatesGroup()
		{
			SetUpData();
			var filter = new CusReconDeclarationFilterStripBusinessObject();
			var acceptedDateFilter = (ModuleDateFilter)filter[CusReconDeclarationFilterStripBusinessObject.Schema.AcceptedDate];
			acceptedDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			acceptedDateFilter.Property1 = new ZDateTime("2022-02-09");
			acceptedDateFilter.Property2 = new ZDateTime("2022-02-10");
			acceptedDateFilter.IsActive = true;

			var coll = new Customs.Business.CusReconDeclarationCollection<CusReconDeclaration>(Factory, "KRC");
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("B00178566", coll[0].CRD_JobReferenceNumber);

			filter = new CusReconDeclarationFilterStripBusinessObject();
			var refundApprovalDateFilter = (ModuleDateFilter)filter[CusReconDeclarationFilterStripBusinessObject.Schema.RefundApprovalDate];
			refundApprovalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			refundApprovalDateFilter.Property1 = new ZDateTime("2022-03-08");
			refundApprovalDateFilter.Property2 = new ZDateTime("2022-03-09");
			refundApprovalDateFilter.IsActive = true;

			coll = new Customs.Business.CusReconDeclarationCollection<CusReconDeclaration>(Factory, "KRC");
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("B00178567", coll[0].CRD_JobReferenceNumber);
		}

		void SetUpData()
		{
			var branchPK = GlbBranch.CurrentBranch.PK;
			var payer = Factory.New<OrgHeader>();
			payer.OH_Category = "BUS";
			payer.OH_Code = "RK1";
			payerAddress = payer.MainAddress;

			var reconDeclaration_1 = Factory.New<CusReconDeclaration>();
			reconDeclaration_1.CRD_ApplicationCode = "KRC";
			reconDeclaration_1.CRD_GB_Branch = branchPK;
			reconDeclaration_1.CRD_JobReferenceNumber = "B00178566";
			reconDeclaration_1.CRD_CustomsStatus = "OST";
			reconDeclaration_1.CRD_MessageStatus = "ANT";
			reconDeclaration_1.CRD_CustomsOffice = "010";
			reconDeclaration_1.CRD_OA_DeclarantAddress = ZGuid.Empty;
			var entry1 = Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
			var reconEntry_1 = reconDeclaration_1.CusReconEntries.AddNew();
			reconEntry_1.CRE_CH_OriginalEntry = entry1.PK;
			reconEntry_1.CRE_OriginalEntryNumber = "40615220000002U";
			reconEntry_1.CRE_EntryDate = ZDate.Today;
			reconEntry_1.CRE_EntryType = "AA";
			reconEntry_1.CRE_OA_DeclarantAddress = payer.MainAddress.PK;
			var entryNum5UL_1 = Factory.New<CusEntryNumber>();
			entryNum5UL_1.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNum5UL_1.CE_EntryNum = "40615220000004U";
			entryNum5UL_1.CE_IssueDate = new ZDateTime("2022-02-10");
			entryNum5UL_1.CE_ParentID = reconDeclaration_1.PK;
			var entryNum5UO_1 = Factory.New<CusEntryNumber>();
			entryNum5UO_1.CE_EntryNum = "030641304545";
			entryNum5UO_1.CE_IssueDate = new ZDateTime("2022-02-09");
			entryNum5UO_1.CE_EntryType = ElectronicDocumentTypeList.Codes._5UO;
			entryNum5UO_1.CE_ParentID = reconDeclaration_1.PK;

			var reconDeclaration_2 = Factory.New<CusReconDeclaration>();
			reconDeclaration_2.CRD_ApplicationCode = "KRC";
			reconDeclaration_2.CRD_GB_Branch = branchPK;
			reconDeclaration_2.CRD_JobReferenceNumber = "B00178567";
			reconDeclaration_2.CRD_CustomsStatus = "OAC";
			reconDeclaration_2.CRD_MessageStatus = "DMS";
			reconDeclaration_2.CRD_CustomsOffice = "020";
			reconDeclaration_2.CRD_OA_DeclarantAddress = payerAddress.PK;
			var entry2 = Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
			var reconEntry_2 = reconDeclaration_2.CusReconEntries.AddNew();
			reconEntry_2.CRE_CH_OriginalEntry = entry2.PK;
			reconEntry_2.CRE_OriginalEntryNumber = "40615220000003U";
			reconEntry_2.CRE_EntryDate = ZDate.Today;
			reconEntry_2.CRE_EntryType = "AA";
			reconEntry_2.CRE_OA_DeclarantAddress = payer.MainAddress.PK;
			var entryNum5UL_2 = Factory.New<CusEntryNumber>();
			entryNum5UL_2.CE_EntryNum = "40615220000005U";
			entryNum5UL_2.CE_IssueDate = new ZDateTime("2022-03-10");
			entryNum5UL_2.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNum5UL_2.CE_ParentID = reconDeclaration_2.PK;
			var entryNum5UO_2 = Factory.New<CusEntryNumber>();
			entryNum5UO_2.CE_EntryNum = "030641304546";
			entryNum5UO_2.CE_IssueDate = new ZDateTime("2022-03-09");
			entryNum5UO_2.CE_EntryType = ElectronicDocumentTypeList.Codes._5UO;
			entryNum5UO_2.CE_ParentID = reconDeclaration_2.PK;
			Factory.Save();
		}
		OrgAddress payerAddress;
	}
}
