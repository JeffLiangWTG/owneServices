using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(LocalGlbBranchCollectionProvider))]
	sealed class LocalGlbBranchCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		public void TestValidationAndDefaultAdded()
		{
			var factory = new BusinessObjectFactory();
			var nonCurrentCompany = factory.NewWithValidTestData<GlbCompany>();
			nonCurrentCompany.GC_Code = "TST";
			var nonCurrentCompanyBranch = factory.NewWithValidTestData<GlbBranch>();
			nonCurrentCompanyBranch.GB_GC = nonCurrentCompany.PK;
			nonCurrentCompanyBranch.GB_Code = "TS1";
			var nonCurrentCompanyBranch2 = factory.NewWithValidTestData<GlbBranch>();
			nonCurrentCompanyBranch2.GB_GC = nonCurrentCompany.PK;
			nonCurrentCompanyBranch2.GB_Code = "TS2";

			var localBranchProvider = new LocalGlbBranchCollectionProvider(Factory);
			localBranchProvider.Collection.Add(nonCurrentCompanyBranch);
			localBranchProvider.Collection.Add(nonCurrentCompanyBranch2);
			localBranchProvider.Collection.Add(GlbBranch.CurrentBranch);
			var localBranchLookup = new MultipleSelectionLookup(Factory);
			localBranchLookup.GroupName = "C";
			localBranchLookup.GroupDescription = "Branch";
			localBranchLookup.DisplayName = "Branches";
			localBranchLookup.SetCollectionProvider(localBranchProvider);

			Provider.AddValidationAndDefault(localBranchLookup, null);
			AssertEquals("Conditional validator has been added", 1, localBranchLookup.Validators.Count);
			AssertEquals("Conditional validator has expected type", typeof(LocalGlbBranchCollectionValidator), localBranchLookup.Validators[0].GetType());
		}

		public void TestCollection_LocalBranchValidation()
		{
			var factory = new BusinessObjectFactory();
			var nonCurrentCompany = factory.NewWithValidTestData<GlbCompany>();
			nonCurrentCompany.GC_Code = "TST";
			var nonCurrentCompanyBranch = factory.NewWithValidTestData<GlbBranch>();
			nonCurrentCompanyBranch.GB_GC = nonCurrentCompany.PK;
			nonCurrentCompanyBranch.GB_Code = "TS1";
			var nonCurrentCompanyBranch2 = factory.NewWithValidTestData<GlbBranch>();
			nonCurrentCompanyBranch2.GB_GC = nonCurrentCompany.PK;
			nonCurrentCompanyBranch2.GB_Code = "TS2";

			var localBranchProvider = new LocalGlbBranchCollectionProvider(Factory);
			localBranchProvider.Collection.Add(nonCurrentCompanyBranch);
			localBranchProvider.Collection.Add(nonCurrentCompanyBranch2);
			localBranchProvider.Collection.Add(GlbBranch.CurrentBranch);
			var localBranchLookup = new MultipleSelectionLookup(Factory);
			localBranchLookup.GroupName = "C";
			localBranchLookup.GroupDescription = "Branch";
			localBranchLookup.DisplayName = "Branches";
			localBranchLookup.SetCollectionProvider(localBranchProvider);
			Provider.AddValidationAndDefault(localBranchLookup, null);
			AssertEquals("Only Local Branch can be chosen here. Please remove TS1, TS2.", localBranchLookup.ValidationError);

			localBranchLookup.BindToList.RemoveFromRelationship(nonCurrentCompanyBranch);
			AssertEquals("Only Local Branch can be chosen here. Please remove TS2.", localBranchLookup.ValidationError);

			localBranchLookup.BindToList.RemoveFromRelationship(nonCurrentCompanyBranch2);
			AssertEquals(string.Empty, localBranchLookup.ValidationError);
		}

		protected override Type ExpectedCollectionType => typeof(GlbBranchCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.GlbBranch;

		protected override int ExpectedMaxLength => GlbBranchSchema.GB_Code.MaxLength;
	}
}
