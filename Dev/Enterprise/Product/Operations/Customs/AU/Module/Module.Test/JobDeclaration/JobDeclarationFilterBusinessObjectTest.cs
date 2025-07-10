using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using Constants = Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(JobDeclarationFilterBusinessObject))]
	sealed class JobDeclarationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestCOLSLodgementReferenceNumberFilter() => CombineAssertions(() =>
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.COLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				var filterBO = new JobDeclarationFilterBusinessObject();
				var declaration1 = Factory.New<JobDeclaration>();
				var entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
				var colsHeader1 = Factory.New<QuarantineColsHeader>();
				colsHeader1.QCH_CH_CusEntryHeader = entryHeader1.PK;
				var lrnNumber1 = CusEntryNumber.New(colsHeader1, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
				lrnNumber1.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
				lrnNumber1.CE_EntryNum = "LRN1";
				var declaration2 = Factory.New<JobDeclaration>();
				var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
				var colsHeader2 = Factory.New<QuarantineColsHeader>();
				colsHeader2.QCH_CH_CusEntryHeader = entryHeader2.PK;
				var lrnNumber2 = CusEntryNumber.New(colsHeader2, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
				lrnNumber2.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
				lrnNumber2.CE_EntryNum = "LRN2";
				var declaration3 = Factory.New<JobDeclaration>();
				var entryHeader3 = declaration3.ActiveEntryHeaders.AddNew();
				var colsHeader3 = Factory.New<QuarantineColsHeader>();
				colsHeader3.QCH_CH_CusEntryHeader = entryHeader3.PK;
				Factory.Save();

				var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.COLSLodgementReferenceNumber];
				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				filter.Property = "L";
				var declarations = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals("Two declarations are filtered", 2, declarations.Length);
				AssertEquals("Filtered declaration's PK", true, declarations.Contains(declaration1));
				AssertEquals("Filtered declaration's PK", true, declarations.Contains(declaration2));

				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = "LRN1";
				declarations = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals("One declaration is filtered", 1, declarations.Length);
				AssertEquals("Filtered declaration's PK", declaration1.PK, declarations[0].PK);

				filter.Property = "LRN3";
				declarations = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals("No declaration is filtered", 0, declarations.Length);

				filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
				declarations = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals("One declaration is filtered", 1, declarations.Length);
				AssertEquals("Filtered declaration's PK", declaration3.PK, declarations[0].PK);

				filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
				declarations = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals("Two declarations are filtered", 2, declarations.Length);
				AssertEquals("Filtered declaration's PK", true, declarations.Contains(declaration1));
				AssertEquals("Filtered declaration's PK", true, declarations.Contains(declaration2));
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.COLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			{
				var filterBO = new JobDeclarationFilterBusinessObject();
				var filter = filterBO[DeclarationFilterConstants.COLSLodgementReferenceNumber];
				AssertNull("This filter is only available when COLS is enabled", filter);
			}
		});

		public void TestCOLSLodgementReferenceNumberStatusFilter() => CombineAssertions(() =>
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.COLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				var filterBO = new JobDeclarationFilterBusinessObject();
				var declaration1 = Factory.New<JobDeclaration>();
				var entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
				var colsHeader1 = Factory.New<QuarantineColsHeader>();
				colsHeader1.QCH_CH_CusEntryHeader = entryHeader1.PK;
				var lrnNumber1 = CusEntryNumber.New(colsHeader1, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
				lrnNumber1.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
				lrnNumber1.CE_EntryStatus = COLSEntryStatusList.Codes.LrnActive;
				lrnNumber1.CE_EntryNum = "LRN1";
				var declaration2 = Factory.New<JobDeclaration>();
				var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
				var colsHeader2 = Factory.New<QuarantineColsHeader>();
				colsHeader2.QCH_CH_CusEntryHeader = entryHeader2.PK;
				var lrnNumber2 = CusEntryNumber.New(colsHeader2, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
				lrnNumber2.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
				lrnNumber2.CE_EntryStatus = COLSEntryStatusList.Codes.LrnInactive;
				lrnNumber2.CE_EntryNum = "LRN2";
				var declaration3 = Factory.New<JobDeclaration>();
				var entryHeader3 = declaration3.CustomsEntryHeaders.AddNew();
				var colsHeader3 = Factory.New<QuarantineColsHeader>();
				colsHeader3.QCH_CH_CusEntryHeader = entryHeader3.PK;
				var lrnNumber3 = CusEntryNumber.New(colsHeader3, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
				lrnNumber3.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
				lrnNumber3.CE_EntryStatus = ZString.Empty;
				lrnNumber3.CE_EntryNum = "LRN3";
				Factory.Save();

				var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.COLSLodgementReferenceNumberStatus];
				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = COLSEntryStatusList.Codes.LrnActive;
				var declarations = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals("One declaration is filtered", 1, declarations.Length);
				AssertEquals("Filtered declaration's PK", declaration1.PK, declarations[0].PK);

				filter.Property = COLSEntryStatusList.Codes.LrnInactive;
				declarations = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals("One declaration is filtered", 1, declarations.Length);
				AssertEquals("Filtered declaration's PK", declaration2.PK, declarations[0].PK);

				filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
				declarations = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals("One declaration is filtered", 1, declarations.Length);
				AssertEquals("Filtered declaration's PK", declaration3.PK, declarations[0].PK);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.COLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			{
				var filterBO = new JobDeclarationFilterBusinessObject();
				var filter = filterBO[DeclarationFilterConstants.COLSLodgementReferenceNumberStatus];
				AssertNull("This filter is only available when COLS is enabled", filter);
			}
		});

		public void TestCOLSEntryStatusFilter() => CombineAssertions(() =>
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.COLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				var filterBO = new JobDeclarationFilterBusinessObject();
				var declaration1 = Factory.New<JobDeclaration>();
				var entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
				var colsHeader1 = Factory.New<QuarantineColsHeader>();
				colsHeader1.QCH_CH_CusEntryHeader = entryHeader1.PK;
				colsHeader1.QCH_LodgementStatus = COLSLodgementStatusList.Codes.Completed;
				var declaration2 = Factory.New<JobDeclaration>();
				var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
				var colsHeader2 = Factory.New<QuarantineColsHeader>();
				colsHeader2.QCH_CH_CusEntryHeader = entryHeader2.PK;
				colsHeader2.QCH_LodgementStatus = COLSLodgementStatusList.Codes.Escalated;
				var declaration3 = Factory.New<JobDeclaration>();
				var entryHeader3 = declaration3.CustomsEntryHeaders.AddNew();
				var colsHeader3 = Factory.New<QuarantineColsHeader>();
				colsHeader3.QCH_CH_CusEntryHeader = entryHeader3.PK;
				colsHeader3.QCH_LodgementStatus = ZString.Empty;
				Factory.Save();

				var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.COLSEntryStatus];
				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = COLSLodgementStatusList.Codes.Completed;
				var declarations = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals("One declaration is filtered", 1, declarations.Length);
				AssertEquals("Filtered declaration's PK", declaration1.PK, declarations[0].PK);

				filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
				declarations = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals("Two declarations are filtered", 2, declarations.Length);
				AssertContainsExactElementsInAnyOrder("Filtered declarations' PK", new ZGuid[] { declaration2.PK, declaration3.PK }, new ZGuid[] { declarations[0].PK, declarations[1].PK });

				filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
				declarations = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals("One declaration is filtered", 1, declarations.Length);
				AssertEquals("Filtered declaration's PK", declaration3.PK, declarations[0].PK);

				filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
				declarations = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals("Two declarations are filtered", 2, declarations.Length);
				AssertContainsExactElementsInAnyOrder("Filtered declarations' PK", new ZGuid[] { declaration1.PK, declaration2.PK }, new ZGuid[] { declarations[0].PK, declarations[1].PK });
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.COLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			{
				var filterBO = new JobDeclarationFilterBusinessObject();
				var filter = filterBO[DeclarationFilterConstants.COLSEntryStatus];
				AssertNull("This filter is only available when COLS is enabled", filter);
			}
		});

		public void TestCOLSMessageStatusFilter() => CombineAssertions(() =>
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.COLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				var filterBO = new JobDeclarationFilterBusinessObject();
				var declaration1 = Factory.New<JobDeclaration>();
				var entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
				var colsHeader1 = Factory.New<QuarantineColsHeader>();
				colsHeader1.QCH_CH_CusEntryHeader = entryHeader1.PK;
				colsHeader1.QCH_MessageStatus = COLSHeaderStatusList.Codes.FailedAddDocument;
				var declaration2 = Factory.New<JobDeclaration>();
				var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
				var colsHeader2 = Factory.New<QuarantineColsHeader>();
				colsHeader2.QCH_CH_CusEntryHeader = entryHeader2.PK;
				colsHeader2.QCH_MessageStatus = COLSHeaderStatusList.Codes.SuccessfulAddDocument;
				var declaration3 = Factory.New<JobDeclaration>();
				var entryHeader3 = declaration3.CustomsEntryHeaders.AddNew();
				var colsHeader3 = Factory.New<QuarantineColsHeader>();
				colsHeader3.QCH_CH_CusEntryHeader = entryHeader3.PK;
				colsHeader3.QCH_MessageStatus = COLSHeaderStatusList.Codes.FailedAddLodgement;
				var declaration4 = Factory.New<JobDeclaration>();
				var entryHeader4 = declaration4.CustomsEntryHeaders.AddNew();
				var colsHeader4 = Factory.New<QuarantineColsHeader>();
				colsHeader4.QCH_CH_CusEntryHeader = entryHeader4.PK;
				colsHeader4.QCH_MessageStatus = COLSHeaderStatusList.Codes.SuccessfulAddLodgement;
				var declaration5 = Factory.New<JobDeclaration>();
				var entryHeader5 = declaration5.CustomsEntryHeaders.AddNew();
				var colsHeader5 = Factory.New<QuarantineColsHeader>();
				colsHeader5.QCH_CH_CusEntryHeader = entryHeader5.PK;
				colsHeader5.QCH_MessageStatus = ZString.Empty;
				Factory.Save();

				var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.COLSMessageStatus];
				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = COLSHeaderStatusList.Codes.FailedAddDocument;
				var declarations = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals("One declaration is filtered", 1, declarations.Length);
				AssertEquals("Filtered declaration's PK", declaration1.PK, declarations[0].PK);

				filter.Property = COLSHeaderStatusList.Codes.SuccessfulAddDocument;
				declarations = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals("One declaration is filtered", 1, declarations.Length);
				AssertEquals("Filtered declaration's PK", declaration2.PK, declarations[0].PK);

				filter.Property = DeclarationFilterConstants.COLSExtraMessageStatus.Failed;
				declarations = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals("Two declarations with failed cols message status are filtered", 2, declarations.Length);
				AssertContainsExactElementsInAnyOrder("Filtered declarations' PK", new ZGuid[] { declaration1.PK, declaration3.PK }, declarations.Select(x => x.PK));

				filter.Property = DeclarationFilterConstants.COLSExtraMessageStatus.Success;
				declarations = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals("Two declarations with success cols message status are filtered", 2, declarations.Length);
				AssertContainsExactElementsInAnyOrder("Filtered declarations' PK", new ZGuid[] { declaration2.PK, declaration4.PK }, declarations.Select(x => x.PK));

				filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
				declarations = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals("Three declarations with non-success cols message status are filtered", 3, declarations.Length);
				AssertContainsExactElementsInAnyOrder("Filtered declarations' PK", new ZGuid[] { declaration1.PK, declaration3.PK, declaration5.PK }, declarations.Select(x => x.PK));

				filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
				declarations = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals("One declaration is filtered", 1, declarations.Length);
				AssertEquals("Filtered declaration's PK", declaration5.PK, declarations[0].PK);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.COLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			{
				var filterBO = new JobDeclarationFilterBusinessObject();
				var filter = filterBO[DeclarationFilterConstants.COLSMessageStatus];
				AssertNull("This filter is only available when COLS is enabled", filter);
			}
		});

		public void TestCustomsEntryPaymentDate()
		{
			JobDeclaration beforeFromDateJobDec = CreateJobDeclarationWithPaymentMessage(FromDate.AddYears(-1));
			JobDeclaration fromDateJobDec = CreateJobDeclarationWithPaymentMessage(FromDate);
			JobDeclaration toDateJobDec = CreateJobDeclarationWithPaymentMessage(ToDate);
			Factory.Save();
			ModuleDateFilter filter = (ModuleDateFilter)filterBO[DeclarationFilterConstants.DateFilterTypes.EntryPaymentDate];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.IsActive = true;
			filter.Property1 = FromDate;
			JobDeclaration[] jobDecs = (JobDeclaration[])Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			ArrayList result = new ArrayList(jobDecs);
			AssertCollectionContains(fromDateJobDec, result);
			AssertCollectionContains(toDateJobDec, result);
			AssertCollectionNotContains(beforeFromDateJobDec, result);
		}

		public void TestJE_DateTimeFilterTypeFilterControlFilterWhenJE_DateFilterTypeIsPaymentDate_To()
		{
			JobDeclaration beforeFromDateJobDec = CreateJobDeclarationWithPaymentMessage(FromDate.AddYears(-1));
			JobDeclaration fromDateJobDec = CreateJobDeclarationWithPaymentMessage(FromDate);
			JobDeclaration toDateJobDec = CreateJobDeclarationWithPaymentMessage(ToDate);
			JobDeclaration afterToDateJobDec = CreateJobDeclarationWithPaymentMessage(FromDate.AddYears(1));
			Factory.Save();
			ModuleDateFilter filter = (ModuleDateFilter)filterBO[DeclarationFilterConstants.DateFilterTypes.EntryPaymentDate];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.IsActive = true;
			filter.Property2 = ToDate;
			JobDeclaration[] jobDecs = (JobDeclaration[])Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			ArrayList result = new ArrayList(jobDecs);
			AssertCollectionContains(fromDateJobDec, result);
			AssertCollectionContains(toDateJobDec, result);
			AssertCollectionContains(beforeFromDateJobDec, result);
			AssertCollectionNotContains(afterToDateJobDec, result);
		}

		public void TestJE_DateTimeFilterTypeFilterControlFilterWhenJE_DateFilterTypeIsPaymentDate_FromAndTo()
		{
			JobDeclaration beforeFromDateJobDec = CreateJobDeclarationWithPaymentMessage(FromDate.AddYears(-1));
			JobDeclaration fromDateJobDec = CreateJobDeclarationWithPaymentMessage(FromDate);
			JobDeclaration toDateJobDec = CreateJobDeclarationWithPaymentMessage(ToDate);
			JobDeclaration afterToDateJobDec = CreateJobDeclarationWithPaymentMessage(FromDate.AddYears(+1));
			Factory.Save();
			ModuleDateFilter filter = (ModuleDateFilter)filterBO[DeclarationFilterConstants.DateFilterTypes.EntryPaymentDate];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			filter.Property1 = FromDate;
			filter.Property2 = ToDate;
			filter.IsActive = true;
			JobDeclaration[] jobDecs = (JobDeclaration[])Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			ArrayList result = new ArrayList(jobDecs);
			AssertCollectionContains(fromDateJobDec, result);
			AssertCollectionContains(toDateJobDec, result);
			AssertCollectionNotContains(beforeFromDateJobDec, result);
			AssertCollectionNotContains(afterToDateJobDec, result);
		}

		public void TestJE_DateTimeFilterTypeFilterControlFilterWhenJE_DateFilterTypeIsPaymentDate_FromAndToAreBlank()
		{
			JobDeclaration beforeFromDateJobDec = CreateJobDeclarationWithPaymentMessage(FromDate.AddYears(-1));
			JobDeclaration fromDateJobDec = CreateJobDeclarationWithPaymentMessage(FromDate);
			JobDeclaration toDateJobDec = CreateJobDeclarationWithPaymentMessage(ToDate);
			JobDeclaration afterToDateJobDec = CreateJobDeclarationWithPaymentMessage(FromDate.AddYears(+1));
			Factory.Save();
			ModuleDateFilter filter = (ModuleDateFilter)filterBO[DeclarationFilterConstants.DateFilterTypes.EntryPaymentDate];
			filter.IsActive = true;
			JobDeclaration[] jobDecs = (JobDeclaration[])Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			ArrayList result = new ArrayList(jobDecs);
			AssertCollectionContains(fromDateJobDec, result);
			AssertCollectionContains(toDateJobDec, result);
			AssertCollectionContains(beforeFromDateJobDec, result);
			AssertCollectionContains(afterToDateJobDec, result);
		}

		public void TestLookups()
		{
			AssertEquals("Lookups should be of correct type", typeof(JobDeclarationFilterLookups), this.filterBO.Lookups.GetType());
		}

		public void TestJE_CMRMessageStatus_NotSentFilter()
		{
			JobDeclaration notSentCMRDeclaration = Factory.New<JobDeclaration>();
			notSentCMRDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			notSentCMRDeclaration.JE_MessageStatus = "";
			JobDeclaration notSentLegacyDeclaration = Factory.New<JobDeclaration>();
			notSentLegacyDeclaration.JE_ApplicationCode = "";
			notSentLegacyDeclaration.JE_MessageStatus = "";
			JobDeclaration sentCMRDeclaration = Factory.New<JobDeclaration>();
			sentCMRDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			sentCMRDeclaration.JE_MessageStatus = "XXX";
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.CustomsMessageStatus];
			AssertFilterMatches("Not filtering when entry status empty", notSentCMRDeclaration, notSentLegacyDeclaration, sentCMRDeclaration);
			filter.Property = DeclarationFilterConstants.EntryStatus.NotSentForFilter;
			filter.IsActive = true;
			AssertFilterMatches("Should return only the CMR declaration", notSentCMRDeclaration);
		}

		public void TestConsignmentReferenceNumberFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration1.JE_PartShipConsignmentReference = "X1";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration2.JE_PartShipConsignmentReference = "1X";
			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.JE_PartShipConsignmentReference = "1X";
			declaration3.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Assert("PreCondition", declaration3.JE_PartShipConsignmentReference.IsEmpty);
			Factory.Save();
			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.ConsignmentRefNumber];
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "X";
			filter.IsActive = true;
			Assert(declaration1.MatchesFilter(filterBO.Filter));
			Assert(!declaration2.MatchesFilter(filterBO.Filter));
			Assert(!declaration3.MatchesFilter(filterBO.Filter));
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			Assert(declaration1.MatchesFilter(filterBO.Filter));
			Assert(declaration2.MatchesFilter(filterBO.Filter));
			Assert(!declaration3.MatchesFilter(filterBO.Filter));
		}

		public void TestJE_CMREntryStatus()
		{
			var filter = (EntryStatusFilter)filterBO[filterBO.EntryStatusText];
			var comparisonOperatorCodes = filter.ComparisonOperator_List.ToArray().Select(c => c.Code);
			AssertContainsExactElementsInAnyOrder(new string[] { "exact", "not equal", "is blank", "is not blank" }, comparisonOperatorCodes);
			filter.Property = "AAA";
			JobDeclarationCollection collection = new JobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Should not return any", 0, collection.Count);
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_EntryStatus = "HBW";
			Factory.Save();
			filter.Property = "HBW";
			collection.Load(filterBO.Filter);
			AssertEquals("Should return one dec", 1, collection.Count);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			AssertEquals("ComparisonOperator should have been reset when changed to a value out of the list.", filter.ComparisonOperator, ModuleTextFilter.ComparisonConstants.Exact);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			AssertEquals("ComparisonOperator should have been changed to a value in the list.", filter.ComparisonOperator, ModuleTextFilter.ComparisonConstants.NotEqual);
		}

		public void TestMessageStatus()
		{
			ModuleTextFilter filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.CustomsMessageStatus];
			filter.Property = CustomsEntryStatus.FailAmendment.Code;
			filter.IsActive = true;
			JobDeclaration testDec1 = Factory.New<JobDeclaration>();
			testDec1.JE_MessageStatus = CustomsEntryStatus.FailFormalLodge.Code;
			JobDeclaration testDec2 = Factory.New<JobDeclaration>();
			testDec2.JE_MessageStatus = CustomsEntryStatus.ClearAmendment.Code;
			Factory.Save();
			JobDeclarationCollection collection = new JobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Not decs", 0, collection.Count);
			filter.Property = CustomsEntryStatus.ClearAmendment.Code;
			collection = new JobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Decs", 1, collection.Count);
			filter.Property = ZString.Empty;
			collection = new JobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Decs", 2, collection.Count);
		}

		public void TestConsolidatedEntryStatus()
		{
			filterBO.ParentModuleID = ModuleIDs.Customs.ConsolidatedDeclaration;
			var filter = (EntryStatusFilter)filterBO["Entry Status"];
			AssertEquals("Parent Module ID set", ModuleIDs.Customs.ConsolidatedDeclaration, filterBO.ParentModuleID);
			AssertEquals("RFC is the filtered status for declarations ready for consolidation", "RFC", filter.Property);
			AssertEquals("RFC is always effective", FilterVisibility.AlwaysAppliedAndHidden, filter.Visibility);
		}

		public void TestWHSStatus()
		{
			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.WHSStatus];
			filter.Property = Enterprise.Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreated;
			filter.IsActive = true;
			var testDec1 = Factory.New<JobDeclaration>();
			var testDec1Entry = testDec1.CustomsEntryHeaders.AddNew();
			testDec1.WarehouseTransactionStatus = Enterprise.Customs.Business.WarehouseTransactionStatusList.Codes.InwardUpdated;
			var testDec2 = Factory.New<JobDeclaration>();
			var testDec2Entry = testDec2.CustomsEntryHeaders.AddNew();
			testDec2.WarehouseTransactionStatus = Enterprise.Customs.Business.WarehouseTransactionStatusList.Codes.InwardCanceled;
			var testDec3 = Factory.New<JobDeclaration>();
			var testDec3Entry = testDec3.CustomsEntryHeaders.AddNew();
			testDec3.WarehouseTransactionStatus = Enterprise.Customs.Business.WarehouseTransactionStatusList.Codes.InwardCanceled;
			Factory.Save();
			var collection = new JobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Not decs", 0, collection.Count);
			filter.Property = Enterprise.Customs.Business.WarehouseTransactionStatusList.Codes.InwardUpdated;
			collection = new JobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Decs", 1, collection.Count);
			AssertCollectionContains(testDec1, collection);
			filter.Property = ZString.Empty;
			collection = new JobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Decs", 3, collection.Count);
			AssertCollectionContains(testDec1, collection);
			AssertCollectionContains(testDec2, collection);
			AssertCollectionContains(testDec3, collection);
			filter.Property = Enterprise.Customs.Business.WarehouseTransactionStatusList.Codes.InwardCanceled;
			collection = new JobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Decs", 2, collection.Count);
			AssertCollectionContains(testDec2, collection);
			AssertCollectionContains(testDec3, collection);
		}

		public void TestConsolidatedCargoStatus()
		{
			ModuleTextFilter filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.CustomsConsolidatedCargoStatus];
			filter.Property = CMRConsolidatedCargoStatuses.Codes.WithdrawnCargoReportHadBeenWithdrawn;
			filter.IsActive = true;
			JobDeclaration testDec1 = Factory.New<JobDeclaration>();
			testDec1.JE_ConsolidatedCargoStatus = CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement;
			JobDeclaration testDec2 = Factory.New<JobDeclaration>();
			testDec2.JE_ConsolidatedCargoStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			JobDeclaration testDec3 = Factory.New<JobDeclaration>();
			testDec3.JE_ConsolidatedCargoStatus = CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation;
			JobDeclaration testDec4 = Factory.New<JobDeclaration>();
			testDec4.JE_ConsolidatedCargoStatus = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			JobDeclaration testDec5 = Factory.New<JobDeclaration>();
			testDec5.JE_ConsolidatedCargoStatus = CMRConsolidatedCargoStatuses.Codes.TranshipCargoIsForTranshipmentATranshipmentNumberWillBeGeneratedAndTransmittedWithStatus;
			JobDeclaration testDec6 = Factory.New<JobDeclaration>();
			testDec6.JE_ConsolidatedCargoStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			JobDeclaration testDec7 = Factory.New<JobDeclaration>();
			testDec7.JE_ConsolidatedCargoStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			JobDeclaration testDec8 = Factory.New<JobDeclaration>();
			testDec8.JE_ConsolidatedCargoStatus = ZString.Empty;
			Factory.Save();
			JobDeclarationCollection collection = new JobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Not decs", 0, collection.Count);
			filter.Property = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			collection = new JobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Clear Decs", 3, collection.Count);
			filter.Property = CMRConsolidatedCargoStatuses.Codes.TranshipCargoIsForTranshipmentATranshipmentNumberWillBeGeneratedAndTransmittedWithStatus;
			collection = new JobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Tranship Decs", 1, collection.Count);
			filter.Property = ZString.Empty;
			collection = new JobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("All Decs", 8, collection.Count);
			filter.Property = DeclarationFilterConstants.ConsolidatedCargoStatus.NotClearForFilter;
			collection = new JobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Not Clear Decs", 2, collection.Count);
		}

		public void TestNature10TypeFilter()
		{
			ModuleTextFilter filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NatureType];
			filter.Property = DeclarationFilterConstants.NatureTypes.Nature30;
			filter.IsActive = true;
			JobDeclaration testDec1 = Factory.New<JobDeclaration>();
			testDec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = testDec1.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(testDec1.MergeManager);
			JobDeclaration testDec2 = Factory.New<JobDeclaration>();
			testDec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader2 = testDec2.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_IsPackToBondForLine = true;
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(testDec2.MergeManager);
			Factory.Save();
			JobDeclarationCollection collection = new JobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Not decs", 0, collection.Count);
			filter.Property = DeclarationFilterConstants.NatureTypes.Nature10;
			collection = new JobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Decs", 1, collection.Count);
			AssertEquals("Expected PK", testDec1.PK, collection[0].PK);
		}

		public void TestNature20TypeFilter()
		{
			ModuleTextFilter filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NatureType];
			filter.Property = DeclarationFilterConstants.NatureTypes.Nature30;
			filter.IsActive = true;
			JobDeclaration testDec1 = Factory.New<JobDeclaration>();
			testDec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = testDec1.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(testDec1.MergeManager);
			JobDeclaration testDec2 = Factory.New<JobDeclaration>();
			testDec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader2 = testDec2.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_IsPackToBondForLine = true;
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(testDec2.MergeManager);
			Factory.Save();
			JobDeclarationCollection collection = new JobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Not decs", 0, collection.Count);
			filter.Property = DeclarationFilterConstants.NatureTypes.Nature20;
			collection = new JobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Decs", 1, collection.Count);
			AssertEquals("Expected PK", testDec2.PK, collection[0].PK);
		}

		public void TestNature30TypeFilter()
		{
			ModuleTextFilter filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NatureType];
			filter.Property = DeclarationFilterConstants.NatureTypes.Nature20;
			filter.IsActive = true;
			JobDeclaration testDec1 = Factory.New<JobDeclaration>();
			testDec1.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(testDec1.MergeManager);
			JobDeclaration testDec2 = Factory.New<JobDeclaration>();
			testDec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(testDec2.MergeManager);
			Factory.Save();
			JobDeclarationCollection collection = new JobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Not decs", 0, collection.Count);
			filter.Property = DeclarationFilterConstants.NatureTypes.Nature30;
			collection = new JobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Decs", 1, collection.Count);
			AssertEquals("Expected PK", testDec1.PK, collection[0].PK);
		}

		public void TestJE_ShowPaid()
		{
			JobDeclaration testDec1 = Factory.New<JobDeclaration>();
			JobDeclaration testDec2 = Factory.New<JobDeclaration>();
			JobDeclaration testDec3 = Factory.New<JobDeclaration>();
			JobDeclaration testDec4 = Factory.New<JobDeclaration>();
			CusEntryHeader header2 = testDec2.CustomsEntryHeaders.AddNew();
			header2.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.PayPending;
			CusEntryHeader header3 = testDec3.CustomsEntryHeaders.AddNew();
			header3.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.Paid;
			CusEntryHeader header4 = testDec4.CustomsEntryHeaders.AddNew();
			header4.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.Refunded;
			Factory.Save();
			JobDeclarationCollection collection = new JobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Should have returned jobs", 4, collection.Count);
			ModuleTextFilter filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.PaymentStatusText];
			filter.Property = DeclarationFilterConstants.PaymentStatus.Paid;
			filter.IsActive = true;
			collection.Load(filterBO.Filter);
			AssertEquals("Should only show paid", 2, collection.Count);
			filter.Property = DeclarationFilterConstants.PaymentStatus.NotPaid;
			collection.Load(filterBO.Filter);
			AssertEquals("Should show unpaid", 2, collection.Count);
		}

		public void TestProduceTypeFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var quarantineHeader1 = declaration1.Invoices.AddNew().QuarantineExDocHeader;
			quarantineHeader1.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var quarantineHeader2 = declaration2.Invoices.AddNew().QuarantineExDocHeader;
			quarantineHeader2.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var quarantineHeader3 = declaration3.Invoices.AddNew().QuarantineExDocHeader;
			quarantineHeader3.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			Factory.Save();
			var collection = new JobDeclarationCollection(Factory);
			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.QuarantineProduceType];
			filter.Property = EXDOCCommodityCodes.Codes.Dairy;
			filter.IsActive = true;
			collection.Load(filterBO.Filter);
			AssertEquals("Two Dairy Declarations", 2, collection.Count);
			filter.Property = "M";
			collection.Load(filterBO.Filter);
			AssertEquals("One Meat Declarations", 1, collection.Count);
			filter.Property = EXDOCCommodityCodes.Codes.Fish;
			collection.Load(filterBO.Filter);
			AssertEquals("No Fish Declarations", 0, collection.Count);
			filter.Property = EXDOCCommodityCodes.Codes.Fish;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			collection.Load(filterBO.Filter);
			AssertEquals("Not Fish Declarations", 3, collection.Count);
		}

		public void TestExportPermitNumberFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var quarantineHeader1 = declaration1.Invoices.AddNew().QuarantineExDocHeader;
			var epnNumber = CusEntryNumber.New(quarantineHeader1, CusEntryNumber.EntryType.ExdocPermitNumber, declaration1.CountryCode);
			epnNumber.CE_EntryNum = "PID1040059";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var quarantineHeader2 = declaration2.Invoices.AddNew().QuarantineExDocHeader;
			var epnNumber2 = CusEntryNumber.New(quarantineHeader2, CusEntryNumber.EntryType.ExdocPermitNumber, declaration2.CountryCode);
			epnNumber2.CE_EntryNum = "PID1040061";
			Factory.Save();
			var collection = new JobDeclarationCollection(Factory);
			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.ExportPermitNumber];
			filter.Property = "PID10400";
			filter.IsActive = true;
			collection.Load(filterBO.Filter);
			AssertEquals("Two declarations with starting permit", 2, collection.Count);
			filter.Property = "PID1040061";
			collection.Load(filterBO.Filter);
			AssertEquals("One Declarations equal to permit", 1, collection.Count);
			filter.Property = "PID1355087";
			collection.Load(filterBO.Filter);
			AssertEquals("No Declarations equal to permit", 0, collection.Count);
		}

		public void TestRFPNumberFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var quarantineHeader1 = declaration1.Invoices.AddNew().QuarantineExDocHeader;
			var rfpNumber = CusEntryNumber.New(quarantineHeader1, CusEntryNumber.EntryType.RequestForPermitStatus, declaration1.CountryCode);
			rfpNumber.CE_EntryNum = "1040059";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var quarantineHeader2 = declaration2.Invoices.AddNew().QuarantineExDocHeader;
			var rfpNumber2 = CusEntryNumber.New(quarantineHeader2, CusEntryNumber.EntryType.RequestForPermitStatus, declaration2.CountryCode);
			rfpNumber2.CE_EntryNum = "1040061";
			Factory.Save();
			var collection = new JobDeclarationCollection(Factory);
			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.RFPNumber];
			filter.Property = "10400";
			filter.IsActive = true;
			collection.Load(filterBO.Filter);
			AssertEquals("Two declarations with starting RFP Number", 2, collection.Count);
			filter.Property = "1040059";
			collection.Load(filterBO.Filter);
			AssertEquals("One Declarations equal to RFP Number", 1, collection.Count);
			filter.Property = "1355087";
			collection.Load(filterBO.Filter);
			AssertEquals("No Declarations equal to RFP NUmber", 0, collection.Count);
		}

		public void TestRFPStatusFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var quarantineHeader1 = declaration1.Invoices.AddNew().QuarantineExDocHeader;
			var rfpNumber = CusEntryNumber.New(quarantineHeader1, CusEntryNumber.EntryType.RequestForPermitStatus, declaration1.CountryCode);
			rfpNumber.CE_EntryStatus = "ORD";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var quarantineHeader2 = declaration2.Invoices.AddNew().QuarantineExDocHeader;
			var rfpNumber2 = CusEntryNumber.New(quarantineHeader2, CusEntryNumber.EntryType.RequestForPermitStatus, declaration2.CountryCode);
			rfpNumber2.CE_EntryStatus = "ORD";
			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var quarantineHeader3 = declaration3.Invoices.AddNew().QuarantineExDocHeader;
			var rfpNumber3 = CusEntryNumber.New(quarantineHeader3, CusEntryNumber.EntryType.RequestForPermitStatus, declaration3.CountryCode);
			rfpNumber3.CE_EntryStatus = "FIN";
			Factory.Save();
			var collection = new JobDeclarationCollection(Factory);
			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.RFPStatus];
			filter.Property = EXDOCComplianceStatusCodes.Codes.Order;
			filter.IsActive = true;
			collection.Load(filterBO.Filter);
			AssertEquals("Two declarations with starting RFP Status", 2, collection.Count);
			filter.Property = EXDOCComplianceStatusCodes.Codes.Final;
			collection.Load(filterBO.Filter);
			AssertEquals("One Declarations equal to RFP Status", 1, collection.Count);
			filter.Property = EXDOCComplianceStatusCodes.Codes.Completed;
			collection.Load(filterBO.Filter);
			AssertEquals("No Declarations equal to RFP Status", 0, collection.Count);
			AssertEquals(5, filter.MaxLength);
			filter.Property = "FINAL";
			collection.Load(filterBO.Filter);
			AssertEquals("One Declarations equal to RFP Status 'FIN'", 1, collection.Count);
		}

		public void TestCommercialInvoicePaymentDate()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			ModuleDateFilter filter = (ModuleDateFilter)filterBO[DeclarationFilterConstants.DateFilterTypes.CommercialInvoicePaymentDate];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2003, 1, 1);
			filter.Property2 = new ZDateTime(2003, 1, 30);
			filter.IsActive = true;
			invoice.JZ_PaymentDate = new ZDateTime(2003, 1, 15);
			Factory.Save();
			JobDeclarationCollection collection = new JobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Should have 1 match", 1, collection.Count);
			filter.Property1 = new ZDateTime(2003, 1, 30);
			collection.Load(filterBO.Filter);
			AssertEquals("Should have no matches", 0, collection.Count);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new JobDeclarationFilterBusinessObject();

		JobDeclarationFilterBusinessObject filterBO;
		protected override void SetUp()
		{
			base.SetUp();
			filterBO = new JobDeclarationFilterBusinessObject();
		}

		void AssertFilterMatches(string message, params JobDeclaration[] expectedMatches)
		{
			JobDeclaration[] actualMatches = (JobDeclaration[])Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals(message, expectedMatches.Length, actualMatches.Length);
			foreach (JobDeclaration expectedMatch in expectedMatches)
			{
				AssertCollectionContains(message, expectedMatch, actualMatches);
			}
		}

		JobDeclaration CreateJobDeclarationWithPaymentMessage(ZDateTime createDateTime)
		{
			JobDeclaration result = Factory.New<JobDeclaration>();
			result.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			result.FillWithValidTestData();
			CusEntryHeader header = Factory.New<CusEntryHeader>();
			header.FillWithValidTestData();
			header.CH_JE = result.PK;
			EDIMessage message = Factory.New<EDIMessage>();
			message.FillWithValidTestData();
			message.EM_MessageType = "PAR";
			message.EM_SystemCreateTimeUtc = createDateTime;
			message.EM_LinkedObject = header;
			return result;
		}

		ZDateTime FromDate => new ZDateTime(2005, 01, 02, 5, 30, 2);

		ZDateTime ToDate => new ZDateTime(2005, 04, 02, 9, 42, 13);
	}
}
