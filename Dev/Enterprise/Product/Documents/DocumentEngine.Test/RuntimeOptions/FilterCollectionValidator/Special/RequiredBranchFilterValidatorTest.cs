using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class RequiredBranchFilterValidatorTest : TestCase
	{
		public void TestValidation()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			LookupField emptyField = new LookupField(factory);
			emptyField.DisplayName = "EmptyFilter";
			emptyField.Value = Guid.Empty;

			MultipleSelectionLookup notCurrentBranchField = new MultipleSelectionLookup(factory);
			notCurrentBranchField.DisplayName = "BadFilter";
			notCurrentBranchField.SetCollectionProvider(CollectionAndModuleIDBuilder.GetCollectionAndModuleID(factory, CollectionProviderTypeCodeDescriptionList.Codes.Branch));
			notCurrentBranchField.BindToList.Add(factory.New<GlbBranch>());

			MultipleSelectionLookup multipleBranchField = new MultipleSelectionLookup(factory);
			multipleBranchField.DisplayName = "MultipleFilter";
			multipleBranchField.SetCollectionProvider(CollectionAndModuleIDBuilder.GetCollectionAndModuleID(factory, CollectionProviderTypeCodeDescriptionList.Codes.Branch));
			multipleBranchField.BindToList.Add(GlbBranch.CurrentBranch);
			multipleBranchField.BindToList.Add(factory.New<GlbBranch>());

			LookupField currentBranchField = new LookupField(factory);
			currentBranchField.DisplayName = "GoodFilter";
			currentBranchField.Value = GlbBranch.CurrentBranch.PK.ToGuid();

			RequiredBranchFilterValidator requiredBranchFilterValidator = new RequiredBranchFilterValidator();
			requiredBranchFilterValidator.Filters.Add(emptyField);
			AssertEquals(false, requiredBranchFilterValidator.IsValid(emptyField));
			string expectedErrorMessage = string.Format("'{0}' should have the login branch. This is required because you have not been granted the security right ({1}).",
					"EmptyFilter", Env.Security.ReportsExternalBranchFilter.DisplayTextPathToSecurityRight);
			AssertEquals(expectedErrorMessage, requiredBranchFilterValidator.GetErrorMessage(emptyField));

			requiredBranchFilterValidator.Filters.Add(notCurrentBranchField);
			AssertEquals(false, requiredBranchFilterValidator.IsValid(notCurrentBranchField));
			expectedErrorMessage = string.Format("'{0}' should have the login branch. This is required because you have not been granted the security right ({1}).",
					"BadFilter", Env.Security.ReportsExternalBranchFilter.DisplayTextPathToSecurityRight);
			AssertEquals(expectedErrorMessage, requiredBranchFilterValidator.GetErrorMessage(notCurrentBranchField));

			requiredBranchFilterValidator.Filters.Add(multipleBranchField);
			AssertEquals(false, requiredBranchFilterValidator.IsValid(multipleBranchField));

			expectedErrorMessage = string.Format("'{0}' should have only the login branch. This is required because you have not been granted the security right ({1}).",
					"MultipleFilter", Env.Security.ReportsExternalBranchFilter.DisplayTextPathToSecurityRight);
			AssertEquals(expectedErrorMessage, requiredBranchFilterValidator.GetErrorMessage(multipleBranchField));

			requiredBranchFilterValidator.Filters.Add(currentBranchField);
			AssertEquals(true, requiredBranchFilterValidator.IsValid(currentBranchField));
		}

		public void TestValidationWithHiddenLookup()
		{
			var factory = new BusinessObjectFactory();
			var branchLookup = new MultipleSelectionLookup(factory);
			branchLookup.DisplayName = "TestFilter";
			branchLookup.SetCollectionProvider(CollectionAndModuleIDBuilder.GetCollectionAndModuleID(factory, CollectionProviderTypeCodeDescriptionList.Codes.Branch));
			branchLookup.BindToList.Add(factory.New<GlbBranch>());

			var requiredBranchFilterValidator = new RequiredBranchFilterValidator();
			requiredBranchFilterValidator.Filters.Add(branchLookup);

			AssertEquals("Precondition", false, branchLookup.IsHidden);
			var expectedErrorMessage = string.Format("'{0}' should have the login branch. This is required because you have not been granted the security right ({1}).",
					"TestFilter", Env.Security.ReportsExternalBranchFilter.DisplayTextPathToSecurityRight);
			AssertEquals(expectedErrorMessage, requiredBranchFilterValidator.GetErrorMessage(branchLookup));

			branchLookup.Style = MultipleSelectionLookup.Styles.None;
			AssertEquals("Precondition", true, branchLookup.IsHidden);
			AssertEquals(true, string.IsNullOrEmpty(requiredBranchFilterValidator.GetErrorMessage(branchLookup)));
		}
	}
}
