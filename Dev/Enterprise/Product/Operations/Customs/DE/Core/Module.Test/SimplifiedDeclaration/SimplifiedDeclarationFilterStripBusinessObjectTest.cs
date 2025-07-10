using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using RepresentationTypeList = Enterprise.Customs.EU.Business.RepresentationTypeList;

namespace Enterprise.Customs.DE.Module.Testing
{
	[TestedType(typeof(SimplifiedDeclarationFilterStripBusinessObject))]
	sealed class SimplifiedDeclarationFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestRegistrationNumberFilterProperties()
		{
			var registrationNumberFilter = (ModuleTextFilter)filter[SimplifiedDeclarationFilterStripBusinessObject.Schema.RegistrationNumber];
			registrationNumberFilter.IsActive = true;
			var comparisonOperatorList = registrationNumberFilter.ComparisonOperator_List.GetAllCodes();

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Registration Number", registrationNumberFilter.Description);
				AssertEquals("Category", FilterCategories.NumbersAndReferences, registrationNumberFilter.Category);
				AssertEquals("MaxLength", CusReconEntrySchema.CRE_OriginalEntryNumber.MaxLength, registrationNumberFilter.MaxLength);
				AssertCollectionNotContains("No IsBlank", ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank, comparisonOperatorList);
				AssertCollectionNotContains("No IsNotBlank", ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank, comparisonOperatorList);
			});
		}

		public void TestRegistrationNumberFilter()
		{
			var entry1 = Factory.New<CusReconEntry>();
			entry1.CRE_OriginalEntryNumber = "DE172821";
			var entry2 = Factory.New<CusReconEntry>();
			entry2.CRE_OriginalEntryNumber = "DE932878";
			var entry3 = Factory.New<CusReconEntry>();
			entry3.CRE_OriginalEntryNumber = "AU718221";

			var registrationNumberFilter = (ModuleTextFilter)filter[SimplifiedDeclarationFilterStripBusinessObject.Schema.RegistrationNumber];
			registrationNumberFilter.IsActive = true;

			CombineAssertions(() =>
			{
				registrationNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				registrationNumberFilter.Property = "DE932878";
				AssertMatch("Exact", false, true, false);

				registrationNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				registrationNumberFilter.Property = "DE";
				AssertMatch("StartsWith", true, true, false);

				registrationNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				registrationNumberFilter.Property = "21";
				AssertMatch("Contains", true, false, true);

				registrationNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				registrationNumberFilter.Property = "DE932878";
				AssertMatch("NotEqual", true, false, true);

				registrationNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
				registrationNumberFilter.Property = "DE";
				AssertMatch("NotStartsWith", false, false, true);

				registrationNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
				registrationNumberFilter.Property = "17";
				AssertMatch("NotContain", false, true, true);
			});

			void AssertMatch(ZString message, bool match1, bool match2, bool match3)
			{
				AssertEquals(message + "->entry1", match1, entry1.MatchesFilter(filter.Filter));
				AssertEquals(message + "->entry2", match2, entry2.MatchesFilter(filter.Filter));
				AssertEquals(message + "->entry3", match3, entry3.MatchesFilter(filter.Filter));
			}
		}

		public void TestLocalClearanceDateFilterProperties()
		{
			var localClearanceDateFilter = (ModuleDateFilter)filter[SimplifiedDeclarationFilterStripBusinessObject.Schema.LocalClearanceDate];
			localClearanceDateFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Local Clearance Date", localClearanceDateFilter.Description);
				AssertEquals("Category", FilterCategories.Dates, localClearanceDateFilter.Category);
				AssertEquals("Visibility", FilterVisibility.AlwaysVisible, localClearanceDateFilter.Visibility);
			});
		}

		public void TestLocalClearanceDateFilter()
		{
			var entry1 = Factory.New<CusReconEntry>();
			entry1.CRE_EntryDate = new ZDate(2020, 01, 15);
			var entry2 = Factory.New<CusReconEntry>();
			entry2.CRE_EntryDate = new ZDate(2020, 02, 01);
			var entry3 = Factory.New<CusReconEntry>();
			entry3.CRE_EntryDate = new ZDate(2020, 02, 25);

			var localClearanceDate = (ModuleDateFilter)filter[SimplifiedDeclarationFilterStripBusinessObject.Schema.LocalClearanceDate];
			localClearanceDate.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			localClearanceDate.Property1 = new ZDate(2020, 02, 01);
			localClearanceDate.Property2 = new ZDate(2020, 02, 28);
			localClearanceDate.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("entry1", false, entry1.MatchesFilter(filter.Filter));
				AssertEquals("entry2", true, entry2.MatchesFilter(filter.Filter));
				AssertEquals("entry3", true, entry3.MatchesFilter(filter.Filter));
			});
		}

		public void TestEntryTypeFilterProperties()
		{
			var entryTypeFilter = (ModuleTextFilter)filter[SimplifiedDeclarationFilterStripBusinessObject.Schema.EntryType];
			entryTypeFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Entry Type", entryTypeFilter.Description);
				AssertEquals("Category", FilterCategories.ModesAndTypes, entryTypeFilter.Category);
				AssertArrayEqualsByElements("ComparisonOperators", new[] { ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact }, entryTypeFilter.ComparisonOperator_List.GetAllCodes());
				AssertEquals("CodesAsString", "AAV, AZ, AZL, VAV, VZA, VZL", ((CodeDescriptionPairList)entryTypeFilter.List).CodesAsString);
			});
		}

		public void TestEntryTypeFilter()
		{
			var entry1 = Factory.New<CusReconEntry>();
			entry1.CRE_EntryType = ImportDeclarationTypeList.Codes.AAV;
			var entry2 = Factory.New<CusReconEntry>();
			entry2.CRE_EntryType = ImportDeclarationTypeList.Codes.AZL;
			var entry3 = Factory.New<CusReconEntry>();
			entry3.CRE_EntryType = ImportDeclarationTypeList.Codes.AAV;

			var entryTypeFilter = (ModuleTextFilter)filter[SimplifiedDeclarationFilterStripBusinessObject.Schema.EntryType];
			entryTypeFilter.IsActive = true;

			CombineAssertions(() =>
			{
				entryTypeFilter.Property = ImportDeclarationTypeList.Codes.AAV;
				AssertMatch("AAV", true, false, true);

				entryTypeFilter.Property = ImportDeclarationTypeList.Codes.AZL;
				AssertMatch("AZL", false, true, false);
			});

			void AssertMatch(ZString message, bool match1, bool match2, bool match3)
			{
				AssertEquals(message + "->entry1", match1, entry1.MatchesFilter(filter.Filter));
				AssertEquals(message + "->entry2", match2, entry2.MatchesFilter(filter.Filter));
				AssertEquals(message + "->entry3", match3, entry3.MatchesFilter(filter.Filter));
			}
		}

		public void TestRepresentationTypeFilterProperties()
		{
			var representationTypeFilter = (ModuleTextFilter)filter[SimplifiedDeclarationFilterStripBusinessObject.Schema.RepresentationType];
			representationTypeFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Rep. Type", representationTypeFilter.Description);
				AssertEquals("Category", FilterCategories.ModesAndTypes, representationTypeFilter.Category);
				AssertArrayEqualsByElements("ComparisonOperators", new[] { ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact }, representationTypeFilter.ComparisonOperator_List.GetAllCodes());
				AssertEquals("CodesAsString", "SEL, DIR, IND", ((CodeDescriptionPairList)representationTypeFilter.List).CodesAsString);
			});
		}

		public void TestRepresentationTypeFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			var entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
			var entryHeader2 = declaration1.CustomsEntryHeaders.AddNew();
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			var entryHeader3 = declaration2.CustomsEntryHeaders.AddNew();

			var entry1 = CreateCusReconEntry(entryHeader1.PK);
			var entry2 = CreateCusReconEntry(entryHeader2.PK);
			var entry3 = CreateCusReconEntry(entryHeader3.PK);
			Factory.Save();

			var representationTypeFilter = (ModuleTextFilter)filter[SimplifiedDeclarationFilterStripBusinessObject.Schema.RepresentationType];
			representationTypeFilter.IsActive = true;

			CombineAssertions(() =>
			{
				representationTypeFilter.Property = RepresentationTypeList.Codes._2Direct;
				AssertMatch("DIR", false, false, true);

				representationTypeFilter.Property = RepresentationTypeList.Codes._1Self;
				AssertMatch("SEL", true, true, false);
			});

			void AssertMatch(ZString message, bool match1, bool match2, bool match3)
			{
				AssertEquals(message + "->entry1", match1, entry1.MatchesFilter(filter.Filter));
				AssertEquals(message + "->entry2", match2, entry2.MatchesFilter(filter.Filter));
				AssertEquals(message + "->entry3", match3, entry3.MatchesFilter(filter.Filter));
			}
		}

		public void TestDeclarantFilterProperties()
		{
			var declarantFilter = (ModuleTextFilter)filter[SimplifiedDeclarationFilterStripBusinessObject.Schema.Declarant];
			declarantFilter.IsActive = true;
			var comparisonOperatorList = declarantFilter.ComparisonOperator_List.GetAllCodes();

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Declarant", declarantFilter.Description);
				AssertEquals("Category", FilterCategories.Organisations, declarantFilter.Category);
				AssertEquals("MaxLength", OrgHeaderSchema.OH_Code.MaxLength, declarantFilter.MaxLength);
				AssertCollectionNotContains("No IsBlank", ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank, comparisonOperatorList);
				AssertCollectionNotContains("No IsNotBlank", ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank, comparisonOperatorList);
			});
		}

		public void TestDeclarantFilter()
		{
			AssertOrgAddressFilter(SimplifiedDeclarationFilterStripBusinessObject.Schema.Declarant, entry => entry.CRE_OA_DeclarantAddressInfo);
		}

		public void TestImporterFilterProperties()
		{
			var importerFilter = (ModuleTextFilter)filter[SimplifiedDeclarationFilterStripBusinessObject.Schema.Importer];
			importerFilter.IsActive = true;
			var comparisonOperatorList = importerFilter.ComparisonOperator_List.GetAllCodes();

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Importer", importerFilter.Description);
				AssertEquals("Category", FilterCategories.Organisations, importerFilter.Category);
				AssertEquals("MaxLength", OrgHeaderSchema.OH_Code.MaxLength, importerFilter.MaxLength);
				AssertCollectionNotContains("No IsBlank", ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank, comparisonOperatorList);
				AssertCollectionNotContains("No IsNotBlank", ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank, comparisonOperatorList);
			});
		}

		public void TestImporterFilter()
		{
			AssertOrgAddressFilter(SimplifiedDeclarationFilterStripBusinessObject.Schema.Importer, entry => entry.CRE_OA_ImporterAddressInfo);
		}

		public void TestRepresentativeFilterProperties()
		{
			var representativeFilter = (ModuleTextFilter)filter[SimplifiedDeclarationFilterStripBusinessObject.Schema.Representative];
			representativeFilter.IsActive = true;
			var comparisonOperatorList = representativeFilter.ComparisonOperator_List.GetAllCodes();

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Representative", representativeFilter.Description);
				AssertEquals("Category", FilterCategories.Organisations, representativeFilter.Category);
				AssertEquals("MaxLength", OrgHeaderSchema.OH_Code.MaxLength, representativeFilter.MaxLength);
				AssertCollectionNotContains("No IsBlank", ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank, comparisonOperatorList);
				AssertCollectionNotContains("No IsNotBlank", ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank, comparisonOperatorList);
			});
		}

		public void TestRepresentativeFilter()
		{
			AssertOrgAddressFilter(SimplifiedDeclarationFilterStripBusinessObject.Schema.Representative, entry => entry.CRE_OA_RepresentativeAddressInfo);
		}

		public void TestRepresentedPartyFilterProperties()
		{
			var representedPartyFilter = (ModuleTextFilter)filter[SimplifiedDeclarationFilterStripBusinessObject.Schema.RepresentedParty];
			representedPartyFilter.IsActive = true;
			var comparisonOperatorList = representedPartyFilter.ComparisonOperator_List.GetAllCodes();

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Represented Party", representedPartyFilter.Description);
				AssertEquals("Category", FilterCategories.Organisations, representedPartyFilter.Category);
				AssertEquals("MaxLength", OrgHeaderSchema.OH_Code.MaxLength, representedPartyFilter.MaxLength);
				AssertCollectionNotContains("No IsBlank", ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank, comparisonOperatorList);
				AssertCollectionNotContains("No IsNotBlank", ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank, comparisonOperatorList);
			});
		}

		public void TestRepresentedPartyFilter()
		{
			AssertOrgAddressFilter(SimplifiedDeclarationFilterStripBusinessObject.Schema.RepresentedParty, entry => entry.CRE_OA_BuyingAgentAddressInfo);
		}

		public void TestBranchCodeFilterProperties()
		{
			var branchFilter = (ModuleGuidFilter)filter[SimplifiedDeclarationFilterStripBusinessObject.Schema.Branch];
			branchFilter.IsActive = true;
			var comparisonOperatorList = branchFilter.ComparisonOperator_List.GetAllCodes();

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Branch", branchFilter.Description);
				AssertEquals("Category", FilterCategories.Organisations, branchFilter.Category);
				AssertEquals("SupportsBlankComparisonOperators", false, branchFilter.SupportsBlankComparisonOperators);
			});
		}

		public void TestBranchCodeFilter()
		{
			GlbBranch glbBranch1 = Factory.New<GlbBranch>();
			GlbBranch glbBranch2 = Factory.New<GlbBranch>();
			var entry1 = Factory.New<CusReconEntry>();
			entry1.CRE_GB_Branch = glbBranch1.PK;
			var entry2 = Factory.New<CusReconEntry>();
			entry2.CRE_GB_Branch = glbBranch2.PK;
			var entry3 = Factory.New<CusReconEntry>();
			entry3.CRE_GB_Branch = glbBranch1.PK;

			var branchFilter = (ModuleGuidFilter)filter[SimplifiedDeclarationFilterStripBusinessObject.Schema.Branch];
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
				AssertEquals(message + "->entry1", match1, entry1.MatchesFilter(filter.Filter));
				AssertEquals(message + "->entry2", match2, entry2.MatchesFilter(filter.Filter));
				AssertEquals(message + "->entry3", match3, entry3.MatchesFilter(filter.Filter));
			}
		}

		public void TestNumberOfRowsFilter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			CreateCusReconEntry(entryHeader.PK);
			CreateCusReconEntry(entryHeader.PK);
			CreateCusReconEntry(entryHeader.PK);
			Factory.Save();

			var numberOfRowsFilter = (ModuleTextFilter)filter[SimplifiedDeclarationFilterStripBusinessObject.Schema.NumberOfRows];
			numberOfRowsFilter.IsActive = true;

			CombineAssertions(() =>
			{
				numberOfRowsFilter.Property = 2.ToString();
				AssertEquals(2, Factory.Load<CusReconEntry>(filter.Filter).Length);

				numberOfRowsFilter.Property = 4.ToString();
				AssertEquals(3, Factory.Load<CusReconEntry>(filter.Filter).Length);
			});
		}

		public void TestNumberOfRowsFilterProperties()
		{
			var numberOfRowsFilter = (ModuleTextFilter)filter[SimplifiedDeclarationFilterStripBusinessObject.Schema.NumberOfRows];
			numberOfRowsFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Number Of Rows", numberOfRowsFilter.Description);
				AssertEquals("Category", FilterCategories.Other, numberOfRowsFilter.Category);
			});
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new SimplifiedDeclarationFilterStripBusinessObject();
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var result = base.GetFiltersExcludedFromSubgroupCheckForCommonTables();
			result.Add(TableFilter(OrgAddressSchema.Constants.TableName, SimplifiedDeclarationFilterStripBusinessObject.Schema.Declarant));
			result.Add(TableFilter(OrgAddressSchema.Constants.TableName, SimplifiedDeclarationFilterStripBusinessObject.Schema.Importer));
			result.Add(TableFilter(OrgAddressSchema.Constants.TableName, SimplifiedDeclarationFilterStripBusinessObject.Schema.Representative));
			result.Add(TableFilter(OrgAddressSchema.Constants.TableName, SimplifiedDeclarationFilterStripBusinessObject.Schema.RepresentedParty));
			result.Add(TableFilter(OrgHeaderSchema.Constants.TableName, SimplifiedDeclarationFilterStripBusinessObject.Schema.Declarant));
			result.Add(TableFilter(OrgHeaderSchema.Constants.TableName, SimplifiedDeclarationFilterStripBusinessObject.Schema.Importer));
			result.Add(TableFilter(OrgHeaderSchema.Constants.TableName, SimplifiedDeclarationFilterStripBusinessObject.Schema.Representative));
			result.Add(TableFilter(OrgHeaderSchema.Constants.TableName, SimplifiedDeclarationFilterStripBusinessObject.Schema.RepresentedParty));
			return result;
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheck()
		{
			var result = base.GetFiltersExcludedFromSubgroupCheck();
			result.Add(TableFilter(OrgAddressSchema.Constants.TableName, SimplifiedDeclarationFilterStripBusinessObject.Schema.Declarant));
			result.Add(TableFilter(OrgAddressSchema.Constants.TableName, SimplifiedDeclarationFilterStripBusinessObject.Schema.Importer));
			result.Add(TableFilter(OrgAddressSchema.Constants.TableName, SimplifiedDeclarationFilterStripBusinessObject.Schema.Representative));
			result.Add(TableFilter(OrgAddressSchema.Constants.TableName, SimplifiedDeclarationFilterStripBusinessObject.Schema.RepresentedParty));
			result.Add(TableFilter(OrgHeaderSchema.Constants.TableName, SimplifiedDeclarationFilterStripBusinessObject.Schema.Declarant));
			result.Add(TableFilter(OrgHeaderSchema.Constants.TableName, SimplifiedDeclarationFilterStripBusinessObject.Schema.Importer));
			result.Add(TableFilter(OrgHeaderSchema.Constants.TableName, SimplifiedDeclarationFilterStripBusinessObject.Schema.Representative));
			result.Add(TableFilter(OrgHeaderSchema.Constants.TableName, SimplifiedDeclarationFilterStripBusinessObject.Schema.RepresentedParty));
			result.Add(TableFilter(CusEntryHeaderSchema.Constants.TableName, SimplifiedDeclarationFilterStripBusinessObject.Schema.RepresentationType));
			result.Add(TableFilter(JobDeclarationSchema.Constants.TableName, SimplifiedDeclarationFilterStripBusinessObject.Schema.RepresentationType));
			return result;
		}

		void AssertOrgAddressFilter(string filterName, Func<CusReconEntry, ZPropertyInfo> addressGetter)
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entry1 = GetNewCusRecEntry(entryHeader.PK, "DE172821", addressGetter);
			var entry2 = GetNewCusRecEntry(entryHeader.PK, "DE932878", addressGetter);
			var entry3 = GetNewCusRecEntry(entryHeader.PK, "AU718221", addressGetter);
			Factory.Save();

			var addressFilter = (ModuleTextFilter)filter[filterName];
			addressFilter.IsActive = true;

			CombineAssertions(() =>
			{
				addressFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				addressFilter.Property = "DE932878";
				AssertMatch("Exact", false, true, false);

				addressFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				addressFilter.Property = "DE";
				AssertMatch("StartsWith", true, true, false);

				addressFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				addressFilter.Property = "21";
				AssertMatch("Contains", true, false, true);

				addressFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				addressFilter.Property = "DE932878";
				AssertMatch("NotEqual", true, false, true);

				addressFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
				addressFilter.Property = "DE";
				AssertMatch("NotStartsWith", false, false, true);

				addressFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
				addressFilter.Property = "17";
				AssertMatch("NotContain", false, true, true);
			});

			void AssertMatch(ZString message, bool match1, bool match2, bool match3)
			{
				AssertEquals(message + "->entry1", match1, entry1.MatchesFilter(filter.Filter));
				AssertEquals(message + "->entry2", match2, entry2.MatchesFilter(filter.Filter));
				AssertEquals(message + "->entry3", match3, entry3.MatchesFilter(filter.Filter));
			}
		}

		CusReconEntry GetNewCusRecEntry(ZGuid entryHeaderPK, string orgCode, Func<CusReconEntry, ZPropertyInfo> addressGetter)
		{
			var reconEntry = Factory.New<CusReconEntry>();
			reconEntry.CRE_CH_OriginalEntry = entryHeaderPK;
			reconEntry.CRE_EntryDate = ZDate.Today;
			reconEntry.CRE_EntryType = "AA";

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = orgCode;
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			var addressProperty = addressGetter(reconEntry);
			addressProperty.Value = orgAddress.PK;
			if (addressProperty.Name != nameof(CusReconEntry.CRE_OA_DeclarantAddress))
			{
				reconEntry.CRE_OA_DeclarantAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			}

			return reconEntry;
		}

		CusReconEntry CreateCusReconEntry(ZGuid entryHeaderPK)
		{
			var entry = Factory.New<CusReconEntry>();
			entry.CRE_CH_OriginalEntry = entryHeaderPK;
			entry.CRE_EntryDate = ZDate.Today;
			entry.CRE_EntryType = "AA";
			entry.CRE_OA_DeclarantAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			return entry;
		}

		protected override void SetUp()
		{
			base.SetUp();
			filter = new SimplifiedDeclarationFilterStripBusinessObject();
		}
		SimplifiedDeclarationFilterStripBusinessObject filter;
	}
}
