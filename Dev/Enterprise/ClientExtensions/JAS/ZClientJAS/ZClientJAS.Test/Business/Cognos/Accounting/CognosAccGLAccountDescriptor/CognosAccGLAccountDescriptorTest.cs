using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Cognos.Testing
{
	[TestedType(typeof(CognosAccGLAccountDescriptor))]
	class CognosAccGLAccountDescriptorTest : AccGLAccountDescriptorTest
	{
		#region ExtraInfo
		public void TestExtraInfo()
		{
			AssertNotNull("Should lazy create ExtraInfo if it does not exist in the Database", CognosGLAccount.ExtraInfo);
			Assert("HasChanges should be set to false on creation", !CognosGLAccount.ExtraInfo.HasChanges);
			CognosGLAccount.ExtraInfo.T9_ReconciliationTotalAccount = "MEH";
			Factory.Save();
			BusinessObjectFactory otherFactory = new BusinessObjectFactory();
			CognosAccGLAccountDescriptor cognosGLAccountFromOtherFactory = otherFactory.Load<CognosAccGLAccountDescriptor>(CognosGLAccount.PK);
			AssertNotNull("Should be loaded from the database", cognosGLAccountFromOtherFactory.ExtraInfo);
			Assert("Should be loaded from the database", cognosGLAccountFromOtherFactory.ExtraInfo.IsInDatabase);
			Assert("Should be loaded from the database", !cognosGLAccountFromOtherFactory.ExtraInfo.HasChanges);
			AssertEquals("Should not create a new one", CognosGLAccount.ExtraInfo.PK, cognosGLAccountFromOtherFactory.ExtraInfo.PK);
		}

		public void TestExtraInfoForBinding()
		{
			AssertNotNull("Should lazy create ExtraInfoForBinding", CognosGLAccount.ExtraInfoForBinding);
			Assert("Should be registered as an editable child object", CognosGLAccount.IsRegisteredEditableChildObject(CognosGLAccount.ExtraInfoForBinding));
			Assert("Should contain ExtraInfo", CognosGLAccount.ExtraInfoForBinding.Contains(CognosGLAccount.ExtraInfo));
			AssertEquals("Should only have 1 element", 1, CognosGLAccount.ExtraInfoForBinding.Count);
		}

		public void TestHasExtraInfoBeenCreated()
		{
			Assert("Pre-condition", !CognosGLAccount.HasExtraInfoBeenCreated);
			object lazyLoadExtraInfo = CognosGLAccount.ExtraInfo;
			Assert(CognosGLAccount.HasExtraInfoBeenCreated);
			CognosGLAccount.ExtraInfo.ShouldReconciliateTotal = true;
			CognosGLAccount.ExtraInfo.T9_ReconciliationTotalAccount = "MEH";
			Factory.Save();
			CognosAccGLAccountDescriptor accountFromOtherFactory = new BusinessObjectFactory().Load<CognosAccGLAccountDescriptor>(CognosGLAccount.PK);
			Assert("Should load from DB", accountFromOtherFactory.HasExtraInfoBeenCreated);
		}

		#endregion
		#region GroupingFlags
		public void TestGroupingFlags()
		{
			AssertNotNull("Should lazy create GroupingFlags if it does not exist in the Database", CognosGLAccount.GroupingFlags);
			Assert("HasChanges should be set to false on creation", !CognosGLAccount.GroupingFlags.HasChanges);
			CognosGLAccount.GroupingFlags.T4_Branch = 2;
			Factory.Save();
			BusinessObjectFactory otherFactory = new BusinessObjectFactory();
			CognosAccGLAccountDescriptor cognosGLAccountFromOtherFactory = otherFactory.Load<CognosAccGLAccountDescriptor>(CognosGLAccount.PK);
			AssertNotNull("Should be loaded from the database", cognosGLAccountFromOtherFactory.GroupingFlags);
			Assert("Should be loaded from the database", cognosGLAccountFromOtherFactory.GroupingFlags.IsInDatabase);
			Assert("Should be loaded from the database", !cognosGLAccountFromOtherFactory.GroupingFlags.HasChanges);
			AssertEquals("Should not create a new one", CognosGLAccount.GroupingFlags.PK, cognosGLAccountFromOtherFactory.GroupingFlags.PK);
		}

		public void TestGroupingFlagsForBinding()
		{
			AssertNotNull("Should lazy create GroupingFlagsForBinding", CognosGLAccount.GroupingFlagsForBinding);
			Assert("Should be registered as an editable child object", CognosGLAccount.IsRegisteredEditableChildObject(CognosGLAccount.GroupingFlagsForBinding));
			Assert("Should contain GroupingFlags", CognosGLAccount.GroupingFlagsForBinding.Contains(CognosGLAccount.GroupingFlags));
			AssertEquals("Should only have 1 element", 1, CognosGLAccount.GroupingFlagsForBinding.Count);
		}

		public void TestHasGroupingFlagsBeenCreated()
		{
			Assert("Pre-condition", !CognosGLAccount.HasGroupingFlagsBeenCreated);
			object lazyLoadGroupingFlags = CognosGLAccount.GroupingFlags;
			Assert(CognosGLAccount.HasGroupingFlagsBeenCreated);
			CognosGLAccount.GroupingFlags.T4_Branch = 1;
			Factory.Save();
			CognosAccGLAccountDescriptor accountFromOtherFactory = new BusinessObjectFactory().Load<CognosAccGLAccountDescriptor>(CognosGLAccount.PK);
			Assert("Should load from DB", accountFromOtherFactory.HasGroupingFlagsBeenCreated);
		}

		#endregion
		public void TestIsCognosSubClassificationAccount()
		{
			Assert("Pre-condition", !CognosGLAccount.IsCognosSubClassificationAccount);
			CognosGLAccount.AJ_Language = Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger;
			Assert("Account type is not a Sub-Classification account", !CognosGLAccount.IsCognosSubClassificationAccount);
			CognosGLAccount.AJ_ReportCategory = Core.Constants.AccountType.Alternate;
			Assert("Account type is not a Sub-Classification account", !CognosGLAccount.IsCognosSubClassificationAccount);
			CognosGLAccount.AJ_ReportCategory = CognosAccGLAccountDescriptor.CognosSubClassificationAccountType;
			Assert(CognosGLAccount.IsCognosSubClassificationAccount);
			CognosGLAccount.AJ_Language = Core.Constants.GLLanguages.Tamil;
			CognosGLAccount.AJ_ReportCategory = CognosAccGLAccountDescriptor.CognosSubClassificationAccountType;
			Assert("Account Language is not a Cognos Language", !CognosGLAccount.IsCognosSubClassificationAccount);
		}

		public void TestRequiresCognosConsolidationAccount()
		{
			Assert("Pre-condition", !CognosGLAccount.RequiresCognosConsolidationAccount);
			CognosGLAccount.AJ_Language = Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger;
			Assert("Account Number does not contain dot", !CognosGLAccount.RequiresCognosConsolidationAccount);
			CognosGLAccount.AJ_LocalAccountNumber = "123";
			Assert("Account Number does not contain dot", !CognosGLAccount.RequiresCognosConsolidationAccount);
			CognosGLAccount.AJ_LocalAccountNumber = "123.123";
			Assert(CognosGLAccount.RequiresCognosConsolidationAccount);
			CognosGLAccount.AJ_Language = Core.Constants.Languages.Albanian;
			Assert("Account Language is not a Cognos Language", !CognosGLAccount.RequiresCognosConsolidationAccount);
		}

		public void TestIsCognosGLLanguage()
		{
			Assert("Not a Cognos GL Language", !CognosGLAccount.IsCognosGLLanguage);
			CognosGLAccount.AJ_Language = Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger;
			Assert(CognosGLAccount.IsCognosGLLanguage);
		}

		public void TestReadOnlynessForSubClassificationAccount()
		{
			CognosGLAccount.AJ_Language = Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger;
			CognosGLAccount.AJ_ReportCategory = CognosAccGLAccountDescriptor.CognosSubClassificationAccountType;
			Assert("Should be read-only", CognosGLAccount.ParentGLHeaderPKInfo.ReadOnly);
			Assert("Should be read-only", CognosGLAccount.AJ_AJ_AlternativeNumInfo.ReadOnly);
			Assert("Should be read-only", CognosGLAccount.AJ_AJ_CarriedForwardAccountInfo.ReadOnly);
			Assert("Should be read-only", CognosGLAccount.AJ_AJ_ConsolidationNumInfo.ReadOnly);
			Assert("Should be read-only", CognosGLAccount.AJ_AJ_HeaderDependsOnTotalInfo.ReadOnly);
			Assert("Should be read-only", CognosGLAccount.AJ_AJ_PercentNumInfo.ReadOnly);
			Assert("Should be read-only", CognosGLAccount.AJ_PrintSequenceInfo.ReadOnly);
			Assert("Should be read-only", CognosGLAccount.AJ_TotalLevelInfo.ReadOnly);
			CognosGLAccount.AJ_ReportCategory = Core.Constants.AccountType.BalanceSheetAccount;
			Assert("Should not be read-only", !CognosGLAccount.ParentGLHeaderPKInfo.ReadOnly);
			Assert("Should not be read-only", !CognosGLAccount.AJ_AJ_AlternativeNumInfo.ReadOnly);
			Assert("Should not be read-only", !CognosGLAccount.AJ_AJ_ConsolidationNumInfo.ReadOnly);
			Assert("Should not be read-only", !CognosGLAccount.AJ_AJ_PercentNumInfo.ReadOnly);
		}

		public void TestAJ_GLAccountType_List()
		{
			CognosGLAccount.AJ_Language = Core.Constants.GLLanguages.Urdu;
			Assert("Should not contain CognosSubClassificationAccountType if not a Cognos Language", !CognosGLAccount.AJ_GLAccountType_List.ContainsCode(CognosAccGLAccountDescriptor.CognosSubClassificationAccountType));
			AssertEquals("Should use the base list", new CodeDescriptionPairList(OLookUpEditType.GLAccountDescriptorType).CodesAsString, CognosGLAccount.AJ_GLAccountType_List.CodesAsString);
			CognosGLAccount.AJ_Language = Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger;
			Assert("Should contain CognosSubClassificationAccountType", CognosGLAccount.AJ_GLAccountType_List.ContainsCode(CognosAccGLAccountDescriptor.CognosSubClassificationAccountType));
			AssertEquals("Cognos Sub-Classification", CognosGLAccount.AJ_GLAccountType_List[CognosAccGLAccountDescriptor.CognosSubClassificationAccountType].Description);
		}

		public void TestDeleteCognosGLAccount()
		{
			CognosGLAccount.ExtraInfo.T9_ReconciliationTotalAccount = "MEH";
			CognosGLAccount.GroupingFlags.T4_Branch = 1;
			Factory.Save();
			CognosGLAccount.Delete();
			Assert("Should be deleted", CognosGLAccount.IsDeleted);
			Assert("ExtraInfo should be deleted when CognosGLAccount deleted", CognosGLAccount.ExtraInfo.IsDeleted);
			Assert("GroupingFlags should be deleted when CognosGLAccount deleted", CognosGLAccount.GroupingFlags.IsDeleted);
		}

		public void TestShouldNotUnnecessarilySaveExtraInfoAndGroupingFlagsToDB()
		{
			CognosAccGLAccountDescriptorExtraInfo lazyCreatedExtraInfo = CognosGLAccount.ExtraInfo;
			CognosGroupingFlags lazyCreatedGroupingFlags = CognosGLAccount.GroupingFlags;
			Assert("Sanity check", !lazyCreatedExtraInfo.HasChanges);
			Assert("Sanity check", !lazyCreatedGroupingFlags.HasChanges);
			Factory.Save();
			Assert("Should be deleted from Factory", lazyCreatedExtraInfo.IsDeleted);
			Assert("Should be deleted from Factory", lazyCreatedGroupingFlags.IsDeleted);
			AssertEquals("Should still have one element in the collection", 1, CognosGLAccount.ExtraInfoForBinding.Count);
			AssertEquals("Should still have one element in the collection", 1, CognosGLAccount.GroupingFlagsForBinding.Count);
			Assert("Should be a new ExtraInfo object, should not use the deleted ExtraInfo", lazyCreatedExtraInfo.PK != CognosGLAccount.ExtraInfoForBinding[0].PK);
			Assert("Should be a new GroupingFlags object, should not use the deleted GroupingFlags", lazyCreatedGroupingFlags.PK != CognosGLAccount.GroupingFlagsForBinding[0].PK);
		}

		CognosAccGLAccountDescriptor CognosGLAccount
		{
			get
			{
				if (fCognosGLAccount == null)
				{
					fCognosGLAccount = Factory.New<CognosAccGLAccountDescriptor>();
					// AccGLAccountDescriptor is not valid with empty AJ_DebitCredit.
					fCognosGLAccount.AJ_DebitCredit = Core.Constants.DebitCredit.Debit;
				}

				return fCognosGLAccount;
			}
		}

		CognosAccGLAccountDescriptor fCognosGLAccount;
	}
}
