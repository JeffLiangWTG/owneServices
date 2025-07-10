using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.AsycudaCustoms.Business.CusEntryHeader;
using CusEntryLine = Enterprise.Customs.AsycudaCustoms.Business.CusEntryLine;

namespace Enterprise.Customs.AsycudaCustoms.Module.Testing
{
	[TestedType(typeof(EntryHeaderFilterBusinessObject))]
	class EntryHeaderFilterBusinessObjectTest : Customs.Module.Testing.EntryHeaderFilterBusinessObjectAbstractTest
	{
		public void TestliabilityClearedFilter()
		{
			using (CreateDisposableRiskEnabledSwitcher(true))
			{
				var declaration = Factory.New<JobDeclaration>();
				var (entry1, entryLine1, riskManagement1) = CreateEntryAndRisk(declaration, 10m, 5m, 3m, 10m, 5m, 3m);
				var (entry2, entryLine2, _) = CreateEntryAndRisk(declaration, 10m, 5m, 3m, 10m, 5m, 3m);
				var (entry3, _, _) = CreateEntryAndRisk(declaration, 10m, 5m, 3m, 10m, 5m, 3m);
				var (entry4, _, _) = CreateEntryAndRisk(declaration, 12m, 5m, 3m, 10m, 5m, 3m);

				entry1.ResetTotalsAndCachedValues();
				entryLine1.CL_CustomsValue = 12m;
				entry2.ResetTotalsAndCachedValues();
				entryLine2.CL_CustomsValue = 12m;
				Factory.Save();
				riskManagement1.CSI_Value = 12m;
				Factory.Save();

				CombineAssertions(() =>
				{
					var filterObj = new EntryHeaderFilterBusinessObject();
					var liabilityClearedFilter = (ModuleFlagsFilter)filterObj[EntryHeaderFilterBusinessObject.EntryHeaderFilterConstants.LiabilityCleared];
					AssertNotNull(liabilityClearedFilter);

					liabilityClearedFilter.Property0 = true;
					liabilityClearedFilter.Property1 = false;
					liabilityClearedFilter.IsActive = true;
					AssertEquals("entry1 has one logged LLR event and one cancelled LLR event", true, entry1.MatchesFilter(filterObj.Filter));
					AssertEquals("entry2 has one cancelled LLR event", false, entry2.MatchesFilter(filterObj.Filter));
					AssertEquals("entry3 has one logged LLR event", true, entry3.MatchesFilter(filterObj.Filter));
					AssertEquals("entry4 has no LLR event", false, entry4.MatchesFilter(filterObj.Filter));

					liabilityClearedFilter.Property0 = false;
					liabilityClearedFilter.Property1 = true;
					AssertEquals("entry1 has one logged LLR event and one cancelled LLR event", false, entry1.MatchesFilter(filterObj.Filter));
					AssertEquals("entry2 has one cancelled LLR event", true, entry2.MatchesFilter(filterObj.Filter));
					AssertEquals("entry3 has one logged LLR event", false, entry3.MatchesFilter(filterObj.Filter));
					AssertEquals("entry4 has no LLR event", true, entry4.MatchesFilter(filterObj.Filter));
				});
			}
		}

		(CusEntryHeader, CusEntryLine, RiskManagement) CreateEntryAndRisk(JobDeclaration declaration, decimal customsValue, decimal netWeight, decimal customsQuantity, decimal riskValue, decimal riskQuantity, decimal riskQuantity2)
		{
			declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_CEI_Instruction = instruction.PK;
			var entryLine1 = entry1.AllEntryLines.AddNew();
			entryLine1.CL_CustomsValue = customsValue;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_CEI = instruction.PK;
			invoiceLine1.JI_NetWeight = netWeight;
			invoiceLine1.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine1.JI_CustomsQuantity = customsQuantity;

			var riskManagement1 = instruction.RiskManagements.AddNew();
			riskManagement1.CSI_Value = riskValue;
			riskManagement1.CSI_Quantity = riskQuantity;
			riskManagement1.CSI_Quantity2 = riskQuantity2;

			return (entry1, entryLine1, riskManagement1);
		}

		IDisposable CreateDisposableRiskEnabledSwitcher(bool riskEnabled)
		{
			return ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
				Universal.Constants.FunctionalityTypes.Risk,
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode,
				ZDateTime.Today,
				value: riskEnabled);
		}

		public void TestAcquittedDate()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_BondAcquittedDate = new ZDate(2019, 7, 1);
			var declaration2 = Factory.New<JobDeclaration>();
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_BondAcquittedDate = new ZDate(2015, 7, 1);
			Factory.Save();
			var filterObj = new EntryHeaderFilterBusinessObject().ModuleFilters;
			var aquittedDateFilter = (ModuleDateFilter)filterObj[EntryHeaderFilterBusinessObject.EntryHeaderFilterConstants.AcquittedDate];
			aquittedDateFilter.IsActive = true;
			aquittedDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			aquittedDateFilter.Property1 = new ZDateTime(2015, 06, 1);
			aquittedDateFilter.Property2 = new ZDateTime(2015, 08, 1);
			var query = filterObj.GetFilterQuery(new ModuleFilter[] { aquittedDateFilter });
			AssertEquals(true, entry2.MatchesFilter(query));
			AssertEquals(false, entry1.MatchesFilter(query));
			aquittedDateFilter.Property1 = new ZDateTime(2019, 06, 1);
			aquittedDateFilter.Property2 = new ZDateTime(2019, 08, 1);
			query = filterObj.GetFilterQuery(new ModuleFilter[] { aquittedDateFilter });
			AssertEquals(false, entry2.MatchesFilter(query));
			AssertEquals(true, entry1.MatchesFilter(query));
		}

		public void TestValidToDate()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_BondValidToDate = new ZDate(2019, 7, 1);
			var declaration2 = Factory.New<JobDeclaration>();
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_BondValidToDate = new ZDate(2010, 7, 1);
			Factory.Save();
			var filterObj = new EntryHeaderFilterBusinessObject().ModuleFilters;
			var acquitByDateFilter = (ModuleDateFilter)filterObj[EntryHeaderFilterBusinessObject.EntryHeaderFilterConstants.AcquitByDate];
			acquitByDateFilter.IsActive = true;
			acquitByDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			acquitByDateFilter.Property1 = new ZDateTime(2019, 06, 1);
			acquitByDateFilter.Property2 = new ZDateTime(2019, 08, 1);
			var query = filterObj.GetFilterQuery(new ModuleFilter[] { acquitByDateFilter });
			AssertEquals(true, entry1.MatchesFilter(query));
			AssertEquals(false, entry2.MatchesFilter(query));
			acquitByDateFilter.Property1 = new ZDateTime(2010, 06, 1);
			acquitByDateFilter.Property2 = new ZDateTime(2010, 08, 1);
			query = filterObj.GetFilterQuery(new ModuleFilter[] { acquitByDateFilter });
			AssertEquals(false, entry1.MatchesFilter(query));
			AssertEquals(true, entry2.MatchesFilter(query));
		}

		public void TestContinuousGuaranteeQueryFilter()
		{
			var filterObj = new EntryHeaderFilterBusinessObject();
			var continuousGuaranteeQueryFilter = (ModuleFlagsFilter)filterObj[EntryHeaderFilterBusinessObject.EntryHeaderFilterConstants.ContinuousGuarantee];
			AssertNotNull(continuousGuaranteeQueryFilter);
			var entry1 = SetupEntryHeaderWithGuarantees("XX1", "CON", "CON", "LIN", 100m, null);
			var entry2 = SetupEntryHeaderWithGuarantees("XX2", "STB", "REL", "UNL", 220m, null);
			var declaration = Factory.New<JobDeclaration>();
			var entry3_NoLinkedInstruction = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			continuousGuaranteeQueryFilter.Property0 = true;
			continuousGuaranteeQueryFilter.Property1 = false;
			continuousGuaranteeQueryFilter.IsActive = true;
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(!entry2.MatchesFilter(filterObj.Filter));
			Assert(!entry3_NoLinkedInstruction.MatchesFilter(filterObj.Filter));
			continuousGuaranteeQueryFilter.Property0 = false;
			continuousGuaranteeQueryFilter.Property1 = true;
			Assert(!entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
			Assert(entry3_NoLinkedInstruction.MatchesFilter(filterObj.Filter));
		}

		public void TestLinkedGuaranteeQueryFilter()
		{
			var filterObj = new EntryHeaderFilterBusinessObject();
			var linkedGuaranteeQueryFilter = (ModuleGuidFilter)filterObj[EntryHeaderFilterBusinessObject.EntryHeaderFilterConstants.LinkedGuarantee];
			AssertNotNull(linkedGuaranteeQueryFilter);
			var guaranteeHeader1 = CreateCusGuarantee("TT1");
			var guaranteeHeader2 = CreateCusGuarantee("TT2");
			var entry1 = SetupEntryHeaderWithGuarantees("XX1", "CON", "CON", "LIN", 100m, guaranteeHeader1);
			var entry2 = SetupEntryHeaderWithGuarantees("XX2", "STB", "REL", "UNL", 200m, guaranteeHeader2);
			var declaration = Factory.New<JobDeclaration>();
			var entry3_NoLinkedInstruction = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			linkedGuaranteeQueryFilter.Property = guaranteeHeader1.PK;
			linkedGuaranteeQueryFilter.IsActive = true;
			linkedGuaranteeQueryFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(!entry2.MatchesFilter(filterObj.Filter));
			Assert(!entry3_NoLinkedInstruction.MatchesFilter(filterObj.Filter));
			linkedGuaranteeQueryFilter.Property = ZGuid.NewZGuid();
			Assert(!entry1.MatchesFilter(filterObj.Filter));
			Assert(!entry2.MatchesFilter(filterObj.Filter));
			Assert(!entry3_NoLinkedInstruction.MatchesFilter(filterObj.Filter));
			linkedGuaranteeQueryFilter.Property = ZGuid.Empty;
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
			Assert(entry3_NoLinkedInstruction.MatchesFilter(filterObj.Filter));
			linkedGuaranteeQueryFilter.Property = guaranteeHeader1.PK;
			linkedGuaranteeQueryFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			Assert(!entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
			Assert(entry3_NoLinkedInstruction.MatchesFilter(filterObj.Filter));
		}

		public void TestGuaranteeAmountQueryFilter()
		{
			var filterObj = new EntryHeaderFilterBusinessObject();
			var guaranteeAmountQueryFilter = (ModuleNumberRangeFilter)filterObj[EntryHeaderFilterBusinessObject.EntryHeaderFilterConstants.GuaranteeAmount];
			AssertNotNull(guaranteeAmountQueryFilter);
			var entry1 = SetupEntryHeaderWithGuarantees("XX1", "CON", "CON", "LIN", 100m, null);
			var entry2 = SetupEntryHeaderWithGuarantees("XX2", "STB", "REL", "UNL", 200m, null);
			guaranteeAmountQueryFilter.Property1 = 50m;
			guaranteeAmountQueryFilter.Property2 = 150m;
			guaranteeAmountQueryFilter.IsActive = true;
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(!entry2.MatchesFilter(filterObj.Filter));
			guaranteeAmountQueryFilter.Property1 = 150m;
			guaranteeAmountQueryFilter.Property2 = 250m;
			Assert(!entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
			guaranteeAmountQueryFilter.Property1 = 50m;
			guaranteeAmountQueryFilter.Property2 = 250m;
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
		}

		public void TestGuaranteeActivityQueryFilter()
		{
			var filterObj = new EntryHeaderFilterBusinessObject();
			var guaranteeActivityQueryFilter = (ModuleTextFilter)filterObj[EntryHeaderFilterBusinessObject.EntryHeaderFilterConstants.GuaranteeActivity];
			AssertNotNull(guaranteeActivityQueryFilter);
			var entry1 = SetupEntryHeaderWithGuarantees("XX1", "CON", "CON", "LIN", 100m, null);
			var entry2 = SetupEntryHeaderWithGuarantees("XX2", "STB", "REL", "UNL", 200m, null);
			var declaration = Factory.New<JobDeclaration>();
			var entry3_NoLinkedInstruction = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			guaranteeActivityQueryFilter.Property = "CON";
			guaranteeActivityQueryFilter.IsActive = true;
			guaranteeActivityQueryFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(!entry2.MatchesFilter(filterObj.Filter));
			Assert(!entry3_NoLinkedInstruction.MatchesFilter(filterObj.Filter));
			guaranteeActivityQueryFilter.Property = "REL";
			Assert(!entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
			Assert(!entry3_NoLinkedInstruction.MatchesFilter(filterObj.Filter));
			guaranteeActivityQueryFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(!entry2.MatchesFilter(filterObj.Filter));
			Assert(entry3_NoLinkedInstruction.MatchesFilter(filterObj.Filter));
			guaranteeActivityQueryFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			Assert(!entry1.MatchesFilter(filterObj.Filter));
			Assert(!entry2.MatchesFilter(filterObj.Filter));
			Assert(entry3_NoLinkedInstruction.MatchesFilter(filterObj.Filter));
			guaranteeActivityQueryFilter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
			Assert(!entry3_NoLinkedInstruction.MatchesFilter(filterObj.Filter));
		}

		public void TestGuaranteeStatusQueryFilter()
		{
			var filterObj = new EntryHeaderFilterBusinessObject();
			var guaranteeStatusQueryFilter = (ModuleTextFilter)filterObj[EntryHeaderFilterBusinessObject.EntryHeaderFilterConstants.GuaranteeStatus];
			AssertNotNull(guaranteeStatusQueryFilter);
			var entry1 = SetupEntryHeaderWithGuarantees("XX1", "CON", "CON", "LIN", 100m, null);
			var entry2 = SetupEntryHeaderWithGuarantees("XX2", "STB", "REL", "UNL", 200m, null);
			var declaration = Factory.New<JobDeclaration>();
			var entry3_NoLinkedInstruction = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			guaranteeStatusQueryFilter.Property = "LIN";
			guaranteeStatusQueryFilter.IsActive = true;
			guaranteeStatusQueryFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(!entry2.MatchesFilter(filterObj.Filter));
			Assert(!entry3_NoLinkedInstruction.MatchesFilter(filterObj.Filter));
			guaranteeStatusQueryFilter.Property = "UNL";
			Assert(!entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
			Assert(!entry3_NoLinkedInstruction.MatchesFilter(filterObj.Filter));
			guaranteeStatusQueryFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(!entry2.MatchesFilter(filterObj.Filter));
			Assert(entry3_NoLinkedInstruction.MatchesFilter(filterObj.Filter));
			guaranteeStatusQueryFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			Assert(!entry1.MatchesFilter(filterObj.Filter));
			Assert(!entry2.MatchesFilter(filterObj.Filter));
			Assert(entry3_NoLinkedInstruction.MatchesFilter(filterObj.Filter));
			guaranteeStatusQueryFilter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
			Assert(!entry3_NoLinkedInstruction.MatchesFilter(filterObj.Filter));
		}

		public void TestModuleFiltersAreAdded()
		{
			var moduleFilters = new List<ZString>() { EntryHeaderFilterBusinessObject.EntryHeaderFilterConstants.AcquitByDate, EntryHeaderFilterBusinessObject.EntryHeaderFilterConstants.AcquittedDate };
			var filterObj = GetNewFilterStripBusinessObject();
			foreach (var filter in moduleFilters)
			{
				AssertNotNull(filterObj[filter]);
			}
		}

		public void TestDeclarationCreatedTimeUTCFilter()
		{
			var filterObj = new EntryHeaderFilterBusinessObject();
			var declarationCreatedTimeUTCFilter = (ModuleDateFilter)filterObj[EntryHeaderFilterBusinessObject.EntryHeaderFilterConstants.DeclarationCreatedTimeUTC];
			AssertNotNull(declarationCreatedTimeUTCFilter);
			AssertEquals(FilterVisibility.AlwaysVisible, declarationCreatedTimeUTCFilter.Visibility);
			AssertEquals(ModuleDateFilter.DateRangeSearchTexts.Last3Mths, declarationCreatedTimeUTCFilter.PropertySearch);

			var entry1 = SetupEntryHeaderWithPermits(ZDateTime.Empty, ZDateTime.Today.AddDays(-2), ZDateTime.Empty, ZDateTime.Empty);
			Factory.Save();

			declarationCreatedTimeUTCFilter.IsActive = true;
			declarationCreatedTimeUTCFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			declarationCreatedTimeUTCFilter.Property1 = ZDateTime.UtcToday.AddDays(-2);
			declarationCreatedTimeUTCFilter.Property2 = ZDateTime.UtcToday.AddDays(2);
			CombineAssertions(() =>
			{
				Assert("match date range", entry1.MatchesFilter(filterObj.Filter));

				entry1.Declaration.JE_SystemCreateTimeUtc = ZDateTime.UtcToday.AddDays(4);
				Factory.Save();
				Assert("not match date range", !entry1.MatchesFilter(filterObj.Filter));

				TestConnection.ExecuteNonQuery(@"
IF EXISTS (SELECT NULL FROM sys.triggers WHERE name = 'TG_JobDeclaration_AuditDetailsAreNotMissing_Update')
BEGIN
	DISABLE TRIGGER TG_JobDeclaration_AuditDetailsAreNotMissing_Update ON JobDeclaration
END");
				entry1.Declaration.JE_SystemCreateTimeUtc = ZDateTime.Empty;
				Factory.Save();
				Assert("not match date empty", !entry1.MatchesFilter(filterObj.Filter));
				TestConnection.ExecuteNonQuery(@"
IF EXISTS (SELECT NULL FROM sys.triggers WHERE name = 'TG_JobDeclaration_AuditDetailsAreNotMissing_Update')
BEGIN
	ENABLE TRIGGER TG_JobDeclaration_AuditDetailsAreNotMissing_Update ON JobDeclaration
END");

				declarationCreatedTimeUTCFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
				Assert("match date HasNoDateEntered", entry1.MatchesFilter(filterObj.Filter));

				declarationCreatedTimeUTCFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
				Assert("not match date HasDateEntered", !entry1.MatchesFilter(filterObj.Filter));
			});
		}

		public void TestExpiredQueryFilter()
		{
			var filterObj = new EntryHeaderFilterBusinessObject();
			var expiredQueryFilter = (ModuleFlagsFilter)filterObj[EntryHeaderFilterBusinessObject.EntryHeaderFilterConstants.TransitExpired];
			AssertNotNull(expiredQueryFilter);

			var entry1 = SetupEntryHeaderWithPermits(ZDateTime.Empty, ZDateTime.Today.AddDays(-2), ZDateTime.Empty, ZDateTime.Empty);
			var entry2 = SetupEntryHeaderWithPermits(ZDateTime.Empty, ZDateTime.Today.AddDays(2), ZDateTime.Today, ZDateTime.Empty);
			var entry3 = SetupEntryHeaderWithPermits(ZDateTime.Today, ZDateTime.Today.AddDays(-2), ZDateTime.Empty, ZDateTime.Empty);
			Factory.Save();

			expiredQueryFilter.IsActive = true;
			expiredQueryFilter.Property0 = true;
			CombineAssertions(() =>
			{
				Assert("match entry 1 Expired", entry1.MatchesFilter(filterObj.Filter));
				Assert("not match entry 2 not Expired", !entry2.MatchesFilter(filterObj.Filter));
				Assert("not match entry 3 not Expired", !entry3.MatchesFilter(filterObj.Filter));

				expiredQueryFilter.Property0 = false;
				Assert("not match entry 1 Expired", !entry1.MatchesFilter(filterObj.Filter));
				Assert("match entry 2 not Expired", entry2.MatchesFilter(filterObj.Filter));
				Assert("match entry 3 not Expired", entry3.MatchesFilter(filterObj.Filter));
			});
		}

		public void TestCompletedQueryFilter()
		{
			var filterObj = new EntryHeaderFilterBusinessObject();
			var completedQueryFilter = (ModuleFlagsFilter)filterObj[EntryHeaderFilterBusinessObject.EntryHeaderFilterConstants.TransitCompleted];
			AssertNotNull(completedQueryFilter);

			var entry1 = SetupEntryHeaderWithPermits(ZDateTime.BrettsBirthday, ZDateTime.Empty, ZDateTime.BrettsBirthday, ZDateTime.Empty);
			var entry2 = SetupEntryHeaderWithPermits(ZDateTime.BrettsBirthday, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.BrettsBirthday);
			var entry3 = SetupEntryHeaderWithPermits(ZDateTime.Empty, ZDateTime.BrettsBirthday, ZDateTime.Empty, ZDateTime.Empty);
			var entry4 = SetupEntryHeaderWithPermits(ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
			Factory.Save();

			completedQueryFilter.IsActive = true;
			completedQueryFilter.Property0 = true;
			CombineAssertions(() =>
			{
				Assert("match entry 1 completed", entry1.MatchesFilter(filterObj.Filter));
				Assert("not match entry 2 not completed", !entry2.MatchesFilter(filterObj.Filter));
				Assert("not match entry 3 not completed", !entry3.MatchesFilter(filterObj.Filter));
				Assert("not match entry 4 not completed", !entry4.MatchesFilter(filterObj.Filter));

				completedQueryFilter.Property0 = false;
				Assert("not match entry 1 completed", !entry1.MatchesFilter(filterObj.Filter));
				Assert("match entry 2 not completed", entry2.MatchesFilter(filterObj.Filter));
				Assert("match entry 3 not completed", entry3.MatchesFilter(filterObj.Filter));
				Assert("match entry 4 not completed", entry4.MatchesFilter(filterObj.Filter));
			});
		}

		public void TestPermitNumberQueryFilter()
		{
			var filterObj = new EntryHeaderFilterBusinessObject();
			var permitNumberQueryFilter = (ModuleTextFilter)filterObj[EntryHeaderFilterBusinessObject.EntryHeaderFilterConstants.TransitPermitNumber];
			AssertNotNull(permitNumberQueryFilter);

			var entry1 = SetupEntryHeaderWithPermits("123", "456");
			var entry2 = SetupEntryHeaderWithPermits("", "");
			Factory.Save();

			permitNumberQueryFilter.Property = "123";
			permitNumberQueryFilter.IsActive = true;
			permitNumberQueryFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			CombineAssertions(() =>
			{
				Assert("match entry 1 123", entry1.MatchesFilter(filterObj.Filter));
				Assert("not match entry 2", !entry2.MatchesFilter(filterObj.Filter));

				permitNumberQueryFilter.Property = "xxx";
				Assert("not match entry 1", !entry1.MatchesFilter(filterObj.Filter));
				Assert("not match entry 2", !entry2.MatchesFilter(filterObj.Filter));

				permitNumberQueryFilter.Property = "123";
				permitNumberQueryFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
				Assert("not match entry 1 NotEqual", !entry1.MatchesFilter(filterObj.Filter));
				Assert("match entry 2 NotEqual", entry2.MatchesFilter(filterObj.Filter));

				permitNumberQueryFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
				Assert("not match entry 1 IsBlank", !entry1.MatchesFilter(filterObj.Filter));
				Assert("match entry 2 IsBlank", entry2.MatchesFilter(filterObj.Filter));

				permitNumberQueryFilter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
				Assert("match entry 1 IsNotBlank", entry1.MatchesFilter(filterObj.Filter));
				Assert("not match entry 2 IsNotBlank", !entry2.MatchesFilter(filterObj.Filter));
			});
		}

		public void TestEarliestExpiryDateQueryFilter()
		{
			var filterObj = new EntryHeaderFilterBusinessObject();
			var earliestExpiryDateQueryFilter = (ModuleDateFilter)filterObj[EntryHeaderFilterBusinessObject.EntryHeaderFilterConstants.TransitEarliestExpiryDate];
			AssertNotNull(earliestExpiryDateQueryFilter);

			var entry1 = SetupEntryHeaderWithPermitsEarliestExpiryDate(ZDateTime.BrettsBirthday);
			var entry2 = SetupEntryHeaderWithPermitsEarliestExpiryDate(ZDateTime.Empty);
			var entry3 = SetupEntryHeaderWithPermitsEarliestExpiryDate(ZDateTime.Today.AddDays(2));
			Factory.Save();

			earliestExpiryDateQueryFilter.IsActive = true;
			earliestExpiryDateQueryFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			earliestExpiryDateQueryFilter.Property1 = ZDateTime.BrettsBirthday.AddDays(-1);
			earliestExpiryDateQueryFilter.Property2 = ZDateTime.BrettsBirthday.AddDays(1);

			CombineAssertions(() =>
			{
				Assert("match entry 1 date range 1", entry1.MatchesFilter(filterObj.Filter));
				Assert("not match entry 2 empty", !entry2.MatchesFilter(filterObj.Filter));
				Assert("not match entry 3 date range 1", !entry3.MatchesFilter(filterObj.Filter));

				earliestExpiryDateQueryFilter.Property1 = ZDateTime.BrettsBirthday.AddDays(1);
				earliestExpiryDateQueryFilter.Property2 = ZDateTime.Today.AddDays(4);
				Assert("not match entry 1 date range 2", !entry1.MatchesFilter(filterObj.Filter));
				Assert("not match entry 2 empty", !entry2.MatchesFilter(filterObj.Filter));
				Assert("match entry 3 date range 2", entry3.MatchesFilter(filterObj.Filter));

				earliestExpiryDateQueryFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
				Assert("match entry 1 HasDateEntered", entry1.MatchesFilter(filterObj.Filter));
				Assert("not match entry 2 HasDateEntered", !entry2.MatchesFilter(filterObj.Filter));
				Assert("match entry 3 HasDateEntered", entry3.MatchesFilter(filterObj.Filter));

				earliestExpiryDateQueryFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
				Assert("not match entry 1 HasNoDateEntered", !entry1.MatchesFilter(filterObj.Filter));
				Assert("match entry 2 HasNoDateEntered", entry2.MatchesFilter(filterObj.Filter));
				Assert("not match entry 3 HasNoDateEntered", !entry3.MatchesFilter(filterObj.Filter));
			});
		}

		public void TestEntryNumber()
		{
			var filterObj = new EntryHeaderFilterBusinessObject();
			var entryNumberQueryFilter = (ModuleNumberFilter)filterObj[ModuleEntryHeaderCollection.FilterConstants.EntryNumber];

			entryNumberQueryFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			entryNumberQueryFilter.Property = "En";
			entryNumberQueryFilter.IsActive = true;

			const string messageType = JobMessageTypeList.Codes.Export;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = messageType;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.EntryNumber = "EntryHeader";
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals(messageType, entryHeader.CusEntryNumber.CE_EntryType);
				AssertNotEquals("MRN", entryHeader.CusEntryNumber.CE_EntryType);
				AssertEquals("no 'MRN' limit in the filter", true, entryHeader.MatchesFilter(filterObj.Filter));
			});
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new EntryHeaderFilterBusinessObject();

		CusEntryHeader SetupEntryHeaderWithGuarantees(ZString instructionStyle, ZString bondType, ZString activityCode, ZString status, ZDecimal amount, CusGuaranteeHeader guaranteeHeader)
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = instructionStyle;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			Factory.Save();
			var guarantee = entryInstruction.Guarantee;
			guarantee.PW_BondType = bondType;
			guarantee.PW_ActivityCode = activityCode;
			guarantee.PW_BondNumber2 = "REF";
			guarantee.PW_BondAmount = amount;
			guarantee.PW_CPH_Guarantee = guaranteeHeader?.PK ?? ZGuid.Empty;
			guarantee.PW_Status = status;
			Factory.Save();
			return entryHeader;
		}

		CusEntryHeader SetupEntryHeaderWithPermits(ZDateTime arrivalDate1, ZDateTime expiryDate1, ZDateTime arrivalDate2, ZDateTime expiryDate2)
		{
			return SetupEntryHeaderWithPermits(arrivalDate1, expiryDate1, ZString.Empty, arrivalDate2, expiryDate2, ZString.Empty);
		}

		CusEntryHeader SetupEntryHeaderWithPermits(ZString permitNumber1, ZString permitNumber2)
		{
			return SetupEntryHeaderWithPermits(ZDateTime.Empty, ZDateTime.Empty, permitNumber1, ZDateTime.Empty, ZDateTime.Empty, permitNumber2);
		}

		CusEntryHeader SetupEntryHeaderWithPermitsEarliestExpiryDate(ZDateTime earliestExpiryDate)
		{
			return earliestExpiryDate.IsEmpty ? SetupEntryHeaderWithPermits(ZDateTime.Empty, ZDateTime.Empty, ZString.Empty, ZDateTime.Empty, ZDateTime.Empty, ZString.Empty)
				: SetupEntryHeaderWithPermits(ZDateTime.Empty, earliestExpiryDate, ZString.Empty, ZDateTime.Empty, earliestExpiryDate.AddDays(1), ZString.Empty);
		}

		CusEntryHeader SetupEntryHeaderWithPermits(ZDateTime arrivalDate1, ZDateTime expiryDate1, ZString permitNumber1, ZDateTime arrivalDate2, ZDateTime expiryDate2, ZString permitNumber2)
		{
			var dec = Factory.New<JobDeclaration>();
			var entryInstruction = Factory.New<Business.CusEntryInstruction>();
			entryInstruction.CEI_JE = dec.PK;
			var moveHeaders = entryInstruction.CusInBondPermitsHeaders;

			if (!arrivalDate1.IsEmpty || !expiryDate1.IsEmpty || !permitNumber1.IsEmpty)
			{
				var moveHeader1 = moveHeaders.AddNew();
				moveHeader1.BM_ArrivalDate = arrivalDate1;
				moveHeader1.BM_Calc_ValidityDate = expiryDate1;
				moveHeader1.BM_Calc_PermitNumber = permitNumber1;
			}
			if (!arrivalDate2.IsEmpty || !expiryDate2.IsEmpty || !permitNumber2.IsEmpty)
			{
				var moveHeader2 = moveHeaders.AddNew();
				moveHeader2.BM_ArrivalDate = arrivalDate2;
				moveHeader2.BM_Calc_ValidityDate = expiryDate2;
				moveHeader2.BM_Calc_PermitNumber = permitNumber2;
			}

			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			Factory.Save();

			return entryHeader;
		}

		CusGuaranteeHeader CreateCusGuarantee(ZString number)
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_RN_NKCountryCode = MasterFiles.Business.GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			guaranteeHeader.CPH_Number = number;
			guaranteeHeader.CPH_ApplicationCode = "GUA";
			guaranteeHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
			guaranteeHeader.CPH_EndDate = ZDate.Today.AddDays(1);
			var transaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transaction.CPL_Reference = "Entry Reference";
			transaction.CPL_TranValue = 1000m;
			transaction.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Confirmed;
			transaction.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.CUM;
			transaction.CPL_AppId = "Job Number";
			transaction.CPL_IsAggregated = ZBool.False;
			transaction.CPL_Comment = "Instruction Desc.";
			transaction.CPL_Procedure = "AAA";
			Factory.Save();
			return guaranteeHeader;
		}
	}
}
