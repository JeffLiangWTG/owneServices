using System;
using System.Collections.Generic;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Module.Testing
{
	[TestedType(typeof(MonthlyClosingFilterBusinessObject))]
	class MonthlyClosingFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestJobNumberFilterProperties()
		{
			var jobNumberFilter = (ModuleTextFilter)filter[MonthlyClosingFilterBusinessObject.Schema.JobNumber];
			jobNumberFilter.IsActive = true;
			var comparisonOperatorList = jobNumberFilter.ComparisonOperator_List.GetAllCodes();

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Job Number", jobNumberFilter.Description);
				AssertEquals("Category", FilterCategories.NumbersAndReferences, jobNumberFilter.Category);
				AssertEquals("MaxLength", CusReconDeclarationSchema.CRD_JobReferenceNumber.MaxLength, jobNumberFilter.MaxLength);
				AssertCollectionNotContains("No IsBlank", ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank, comparisonOperatorList);
				AssertCollectionNotContains("No IsNotBlank", ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank, comparisonOperatorList);
			});
		}

		public void TestJobNumberFilter()
		{
			var declaration1 = Factory.New<CusReconDeclaration>();
			declaration1.CRD_JobReferenceNumber = "DE172821";
			var declaration2 = Factory.New<CusReconDeclaration>();
			declaration2.CRD_JobReferenceNumber = "DE932878";
			var declaration3 = Factory.New<CusReconDeclaration>();
			declaration3.CRD_JobReferenceNumber = "AU718221";

			var jobNumberFilter = (ModuleTextFilter)filter[MonthlyClosingFilterBusinessObject.Schema.JobNumber];
			jobNumberFilter.IsActive = true;

			CombineAssertions(() =>
			{
				jobNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				jobNumberFilter.Property = "DE932878";
				AssertMatch("Exact", false, true, false);

				jobNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				jobNumberFilter.Property = "DE";
				AssertMatch("StartsWith", true, true, false);

				jobNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				jobNumberFilter.Property = "21";
				AssertMatch("Contains", true, false, true);

				jobNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				jobNumberFilter.Property = "DE932878";
				AssertMatch("NotEqual", true, false, true);

				jobNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
				jobNumberFilter.Property = "DE";
				AssertMatch("NotStartsWith", false, false, true);

				jobNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
				jobNumberFilter.Property = "17";
				AssertMatch("NotContain", false, true, true);
			});

			void AssertMatch(ZString message, bool match1, bool match2, bool match3)
			{
				AssertEquals(message + "->declaration1", match1, declaration1.MatchesFilter(filter.Filter));
				AssertEquals(message + "->declaration2", match2, declaration2.MatchesFilter(filter.Filter));
				AssertEquals(message + "->declaration3", match3, declaration3.MatchesFilter(filter.Filter));
			}
		}

		public void TestAccountingPeriodFromFilter()
		{
			AssertDateFilter(MonthlyClosingFilterBusinessObject.Schema.AccountingPeriodFrom, (dec, date) => dec.CRD_PeriodFrom = date, "Period From");
		}

		public void TestAccountingPeriodToFilter()
		{
			AssertDateFilter(MonthlyClosingFilterBusinessObject.Schema.AccountingPeriodTo, (dec, date) => dec.CRD_PeriodTo = date, "Period To");
		}

		public void TestAuthorizationNumberFilterProperties()
		{
			var authorizationNumberFilter = (ModuleTextFilter)filter[MonthlyClosingFilterBusinessObject.Schema.AuthorizationNumber];
			authorizationNumberFilter.IsActive = true;
			var comparisonOperatorList = authorizationNumberFilter.ComparisonOperator_List.GetAllCodes();

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Authorization Number", authorizationNumberFilter.Description);
				AssertEquals("Category", FilterCategories.NumbersAndReferences, authorizationNumberFilter.Category);
				AssertEquals("MaxLength", CusPermitHeaderSchema.CPH_Number.MaxLength, authorizationNumberFilter.MaxLength);
				AssertCollectionNotContains("No IsBlank", ModuleTextFilter.ComparisonConstants.IsBlank, comparisonOperatorList);
				AssertCollectionNotContains("No IsNotBlank", ModuleTextFilter.ComparisonConstants.IsNotBlank, comparisonOperatorList);
			});
		}

		public void TestAuthorizationNumberFilter()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var authorization1 = orgHeader.CreateAuthorisationRecord(Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, "DEACR000001");
			var authorization2 = orgHeader.CreateAuthorisationRecord(Customs.Business.CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "DEEIR000001");
			var authorization3 = orgHeader.CreateAuthorisationRecord(Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "DEIPO000001");

			var declaration1 = Factory.New<CusReconDeclaration>();
			declaration1.CRD_JobReferenceNumber = "REF1";
			declaration1.CRD_CPH_ReconClearanceAuthorisation = authorization1.PK;

			var declaration2 = Factory.New<CusReconDeclaration>();
			declaration2.CRD_JobReferenceNumber = "REF2";
			declaration2.CRD_CPH_ReconClearanceAuthorisation = authorization2.PK;

			var declaration3 = Factory.New<CusReconDeclaration>();
			declaration3.CRD_JobReferenceNumber = "REF3";
			declaration3.CRD_CPH_ReconClearanceAuthorisation = authorization3.PK;
			Factory.Save();

			var authorizationNumberFilter = (ModuleTextFilter)filter[MonthlyClosingFilterBusinessObject.Schema.AuthorizationNumber];
			authorizationNumberFilter.IsActive = true;

			CombineAssertions(() =>
			{
				authorizationNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				authorizationNumberFilter.Property = "DEACR000001";
				AssertMatch("Exact", true, false, false);

				authorizationNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				authorizationNumberFilter.Property = "DE";
				AssertMatch("StartsWith", true, true, true);

				authorizationNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				authorizationNumberFilter.Property = "EIR";
				AssertMatch("Contains", false, true, false);

				authorizationNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				authorizationNumberFilter.Property = "DEIPO000001";
				AssertMatch("NotEqual", true, true, false);

				authorizationNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
				authorizationNumberFilter.Property = "DE";
				AssertMatch("NotStartsWith", false, false, false);

				authorizationNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
				authorizationNumberFilter.Property = "ACR";
				AssertMatch("NotContain", false, true, true);
			});

			void AssertMatch(ZString message, bool match1, bool match2, bool match3)
			{
				AssertEquals(message + "->declaration1", match1, declaration1.MatchesFilter(filter.Filter));
				AssertEquals(message + "->declaration2", match2, declaration2.MatchesFilter(filter.Filter));
				AssertEquals(message + "->declaration3", match3, declaration3.MatchesFilter(filter.Filter));
			}
		}

		public void TestDeclarationTypeFilter()
		{
			var declaration1 = Factory.New<CusReconDeclaration>();
			declaration1.CRD_DeclarationType = Customs.Business.CusReconDeclarationTypeList.Codes.FRC;
			var declaration2 = Factory.New<CusReconDeclaration>();
			declaration2.CRD_DeclarationType = Customs.Business.CusReconDeclarationTypeList.Codes.IWP;
			var declaration3 = Factory.New<CusReconDeclaration>();
			declaration3.CRD_DeclarationType = Customs.Business.CusReconDeclarationTypeList.Codes.FRC;
			var declarationTypeFilter = (ModuleTextFilter)filter[MonthlyClosingFilterBusinessObject.Schema.DeclarationType];
			declarationTypeFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Declaration Type", declarationTypeFilter.Description);
				AssertEquals("Category", FilterCategories.ModesAndTypes, declarationTypeFilter.Category);
				AssertEquals("Comparison Operator", ModuleTextFilter.ComparisonConstants.Exact, declarationTypeFilter.ComparisonOperator_List.CodesAsString);

				declarationTypeFilter.Property = Customs.Business.CusReconDeclarationTypeList.Codes.FRC;
				AssertMatch("FRC", true, false, true);

				declarationTypeFilter.Property = Customs.Business.CusReconDeclarationTypeList.Codes.IWP;
				AssertMatch("IWP", false, true, false);
			});

			void AssertMatch(ZString message, bool match1, bool match2, bool match3)
			{
				AssertEquals(message + "->declaration1", match1, declaration1.MatchesFilter(filter.Filter));
				AssertEquals(message + "->declaration2", match2, declaration2.MatchesFilter(filter.Filter));
				AssertEquals(message + "->declaration3", match3, declaration3.MatchesFilter(filter.Filter));
			}
		}

		public void TestCustomsStatusFilterProperties()
		{
			var customsStatusFilter = (ModuleTextFilter)filter[MonthlyClosingFilterBusinessObject.Schema.CustomsStatus];
			customsStatusFilter.IsActive = true;
			var comparisonOperatorList = customsStatusFilter.ComparisonOperator_List.GetAllCodes();

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Customs Status", customsStatusFilter.Description);
				AssertEquals("Category", FilterCategories.StatusAndFlags, customsStatusFilter.Category);
				AssertEquals("MaxLength", CusReconDeclarationSchema.CRD_CustomsStatus.MaxLength, customsStatusFilter.MaxLength);
				AssertCollectionNotContains("No IsBlank", ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank, comparisonOperatorList);
				AssertCollectionNotContains("No IsNotBlank", ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank, comparisonOperatorList);
			});
		}

		public void TestCustomsStatusFilter()
		{
			var declaration1 = Factory.New<CusReconDeclaration>();
			declaration1.CRD_CustomsStatus = "RC2";
			var declaration2 = Factory.New<CusReconDeclaration>();
			declaration2.CRD_CustomsStatus = "ERR";
			var customsStatusFilter = (ModuleTextFilter)filter[MonthlyClosingFilterBusinessObject.Schema.CustomsStatus];
			customsStatusFilter.IsActive = true;

			CombineAssertions(() =>
			{
				customsStatusFilter.Property = "RC2";
				AssertEquals("RC2->declaration1", true, declaration1.MatchesFilter(filter.Filter));
				AssertEquals("RC2->declaration2", false, declaration2.MatchesFilter(filter.Filter));

				customsStatusFilter.Property = "ERR";
				AssertEquals("ERR->declaration1", false, declaration1.MatchesFilter(filter.Filter));
				AssertEquals("ERR->declaration2", true, declaration2.MatchesFilter(filter.Filter));
			});
		}

		public void TestRegistrationNumberFilterProperties()
		{
			var registrationNumberFilter = (ModuleTextFilter)filter[MonthlyClosingFilterBusinessObject.Schema.RegistrationNumber];
			registrationNumberFilter.IsActive = true;
			var comparisonOperatorList = registrationNumberFilter.ComparisonOperator_List.GetAllCodes();

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Registration Number", registrationNumberFilter.Description);
				AssertEquals("Category", FilterCategories.NumbersAndReferences, registrationNumberFilter.Category);
				AssertEquals("MaxLength", CusEntryNumSchema.CE_EntryNum.MaxLength, registrationNumberFilter.MaxLength);
				AssertCollectionNotContains("No IsBlank", ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank, comparisonOperatorList);
				AssertCollectionNotContains("No IsNotBlank", ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank, comparisonOperatorList);
			});
		}

		public void TestRegistrationNumberFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<CusReconDeclaration>();
			var entryNumber = CusEntryNumber.New(declaration1, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			entryNumber.CE_EntryNum = "12345678";
			var declaration2 = Factory.NewWithValidTestData<CusReconDeclaration>();
			var entryNumber2 = CusEntryNumber.New(declaration2, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			entryNumber2.CE_EntryNum = "23456789";
			Factory.Save();

			var registrationNumberFilter = (ModuleTextFilter)filter[MonthlyClosingFilterBusinessObject.Schema.RegistrationNumber];
			registrationNumberFilter.IsActive = true;

			CombineAssertions(() =>
			{
				registrationNumberFilter.Property = "12345678";
				AssertEquals("12345678->declaration1", true, declaration1.MatchesFilter(filter.Filter));
				AssertEquals("12345678->declaration2", false, declaration2.MatchesFilter(filter.Filter));

				registrationNumberFilter.Property = "23456789";
				AssertEquals("23456789->declaration1", false, declaration1.MatchesFilter(filter.Filter));
				AssertEquals("23456789->declaration2", true, declaration2.MatchesFilter(filter.Filter));
			});
		}

		public void TestDeclarationBranchFilterProperties()
		{
			var branchFilter = (ModuleGuidFilter)filter[MonthlyClosingFilterBusinessObject.Schema.DeclarationBranch];
			branchFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Declaration Branch", branchFilter.Description);
				AssertEquals("Category", FilterCategories.Organisations, branchFilter.Category);
			});
		}

		public void TestDeclarationBranchFilter()
		{
			GlbBranch glbBranch1 = Factory.New<GlbBranch>();
			GlbBranch glbBranch2 = Factory.New<GlbBranch>();
			var declaration1 = Factory.New<CusReconDeclaration>();
			declaration1.CRD_GB_Branch = glbBranch1.PK;
			var declaration2 = Factory.New<CusReconDeclaration>();
			declaration2.CRD_GB_Branch = glbBranch2.PK;
			var declaration3 = Factory.New<CusReconDeclaration>();
			declaration3.CRD_GB_Branch = glbBranch1.PK;

			var branchFilter = (ModuleGuidFilter)filter[MonthlyClosingFilterBusinessObject.Schema.DeclarationBranch];
			branchFilter.IsActive = true;

			CombineAssertions(() =>
			{
				branchFilter.Property = glbBranch1.PK;
				AssertMatch("Branch 1", true, false, true);
				branchFilter.Property = glbBranch2.PK;
				AssertMatch("Branch 2", false, true, false);
			});

			void AssertMatch(ZString message, bool match1, bool match2, bool match3)
			{
				AssertEquals(message + "->declaration1", match1, declaration1.MatchesFilter(filter.Filter));
				AssertEquals(message + "->declaration2", match2, declaration2.MatchesFilter(filter.Filter));
				AssertEquals(message + "->declaration3", match3, declaration3.MatchesFilter(filter.Filter));
			}
		}

		public void TestDeclarantFilter_Properties()
		{
			var declarantFilter = (ModuleGuidFilter)filter[MonthlyClosingFilterBusinessObject.Schema.Declarant];
			declarantFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Declarant", declarantFilter.Description);
				AssertEquals("Category", FilterCategories.Organisations, declarantFilter.Category);
			});
		}

		public void TestDeclarantFilter() => TestOrganisationAddressFilter(CusReconDeclarationSchema.CRD_OA_DeclarantAddress, MonthlyClosingFilterBusinessObject.Schema.Declarant);

		public void TestRepresentativeFilter_Properties()
		{
			var representativeFilter = (ModuleGuidFilter)filter[MonthlyClosingFilterBusinessObject.Schema.Representative];
			representativeFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Representative", representativeFilter.Description);
				AssertEquals("Category", FilterCategories.Organisations, representativeFilter.Category);
			});
		}

		public void TestRepresentativeFilter() => TestOrganisationAddressFilter(CusReconDeclarationSchema.CRD_OA_RepresentativeAddress, MonthlyClosingFilterBusinessObject.Schema.Representative);

		public void TestRepresentedPartyFilter_Properties()
		{
			var representedPartyFilter = (ModuleGuidFilter)filter[MonthlyClosingFilterBusinessObject.Schema.RepresentedParty];
			representedPartyFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Represented Party", representedPartyFilter.Description);
				AssertEquals("Category", FilterCategories.Organisations, representedPartyFilter.Category);
			});
		}

		public void TestRepresentedPartyFilter() => TestOrganisationAddressFilter(CusReconDeclarationSchema.CRD_OA_BuyingAgentAddress, MonthlyClosingFilterBusinessObject.Schema.RepresentedParty);

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheck()
		{
			var exclusions = base.GetFiltersExcludedFromSubgroupCheck();
			exclusions.Add(TableFilter(CusPermitHeaderSchema.Constants.TableName, MonthlyClosingFilterBusinessObject.Schema.AuthorizationNumber));
			return exclusions;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new MonthlyClosingFilterBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			filter = new MonthlyClosingFilterBusinessObject();
		}
		MonthlyClosingFilterBusinessObject filter;

		void AssertDateFilter(string filterName, Action<CusReconDeclaration, ZDate> dateSetter, ZString expectedDescription)
		{
			var declaration1 = Factory.New<CusReconDeclaration>();
			dateSetter(declaration1, new ZDate(2020, 01, 15));
			var declaration2 = Factory.New<CusReconDeclaration>();
			dateSetter(declaration2, new ZDate(2020, 02, 01));
			var declaration3 = Factory.New<CusReconDeclaration>();
			dateSetter(declaration3, new ZDate(2020, 02, 25));
			var declaration4 = Factory.New<CusReconDeclaration>();
			dateSetter(declaration4, ZDate.Empty);

			var dateFilter = (ModuleDateFilter)filter[filterName];
			dateFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("Description", expectedDescription, dateFilter.Description);
				AssertEquals("Category", FilterCategories.Dates, dateFilter.Category);

				dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				dateFilter.Property1 = new ZDate(2020, 02, 01);
				dateFilter.Property2 = new ZDate(2020, 02, 28);
				AssertMatch("Between 01/02/2020 - 28/02/2020", false, true, true, false);

				dateFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
				AssertMatch("Has Date", true, true, true, false);

				dateFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
				AssertMatch("No Date", false, false, false, true);
			});

			void AssertMatch(ZString message, bool match1, bool match2, bool match3, bool match4)
			{
				AssertEquals(message + "->declaration1", match1, declaration1.MatchesFilter(filter.Filter));
				AssertEquals(message + "->declaration2", match2, declaration2.MatchesFilter(filter.Filter));
				AssertEquals(message + "->declaration3", match3, declaration3.MatchesFilter(filter.Filter));
				AssertEquals(message + "->declaration4", match4, declaration4.MatchesFilter(filter.Filter));
			}
		}

		void TestOrganisationAddressFilter(SchemaColumn addressColumn, string filterName)
		{
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "ORG1";
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "ORG2";
			var declaration1 = Factory.New<CusReconDeclaration>();
			declaration1[addressColumn] = orgHeader1.MainAddress.PK;
			var declaration2 = Factory.New<CusReconDeclaration>();
			declaration2[addressColumn] = orgHeader2.MainAddress.PK;
			var declaration3 = Factory.New<CusReconDeclaration>();
			declaration3[addressColumn] = orgHeader1.MainAddress.PK;
			Factory.Save();

			var filer = (ModuleGuidFilterForOrg)filter[filterName];
			filer.IsActive = true;

			CombineAssertions(() =>
			{
				filer.Property = orgHeader1.PK;
				AssertMatch("orgHeader1", true, false, true);
				filer.Property = orgHeader2.PK;
				AssertMatch("orgHeader2", false, true, false);
			});

			void AssertMatch(ZString message, bool match1, bool match2, bool match3)
			{
				AssertEquals(message + "->declaration1", match1, declaration1.MatchesFilter(filter.Filter));
				AssertEquals(message + "->declaration2", match2, declaration2.MatchesFilter(filter.Filter));
				AssertEquals(message + "->declaration3", match3, declaration3.MatchesFilter(filter.Filter));
			}
		}
	}
}
