using System.Collections.Generic;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Cognos.Testing
{
	[TestedType(typeof(CognosAccGLAccountDescriptorExtraInfo))]
	class ClientCognosAccGLAccountDescriptorExtraInfoTest : EnterpriseBusinessObjectTestCase
	{
		public void TestReportDeveloperExceptionIfSubClassificationCodeIsInvalid()
		{
			ExtraInfo.T9_SubClassificationCode = "__";
			AssertEquals("Invalid sub classification code specified. This should either be 'AGE' or 'CRD' or 'DEB'", ErrorReporter.LastMessageReported);
			AssertEquals("CognosAccGLAccountDescriptorExtraInfo_SubClassificationCode", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
			ExtraInfo.T9_SubClassificationCode = CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByAge;
			AssertEquals("", ErrorReporter.LastKeyReported);
			ExtraInfo.T9_SubClassificationCode = CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByCreditor;
			AssertEquals("", ErrorReporter.LastKeyReported);
			ExtraInfo.T9_SubClassificationCode = CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByDebtor;
			AssertEquals("", ErrorReporter.LastKeyReported);
		}

		public void TestT9_ReconciliationTotalAccountInfo()
		{
			Assert("Default should be readonly", ExtraInfo.T9_ReconciliationTotalAccountInfo.ReadOnly);
			ExtraInfo.ShouldReconciliateTotal = true;
			Assert("Should not be read-only if ShouldReconciliateTotal is true", !ExtraInfo.T9_ReconciliationTotalAccountInfo.ReadOnly);
		}

		public void TestShouldReconciliateTotal_LazyLoaded()
		{
			ExtraInfo.T9_ReconciliationTotalAccount = "MEH";
			FieldInfo fShouldReconciliateTotalField = ExtraInfo.GetType().GetField("fShouldReconciliateTotal", BindingFlags.Instance | BindingFlags.NonPublic);
			fShouldReconciliateTotalField.SetValue(ExtraInfo, null);
			Assert("Should lazy load the boolean value by checking if ReconciliationTotalAccount is empty", ExtraInfo.ShouldReconciliateTotal);
			ExtraInfo.T9_ReconciliationTotalAccount = "";
			fShouldReconciliateTotalField.SetValue(ExtraInfo, null);
			Assert("Should lazy load the boolean value by checking if ReconciliationTotalAccount is empty", !ExtraInfo.ShouldReconciliateTotal);
		}

		public void TestShouldReconciliateTotal_SetHasChangesAndRefreshBinding()
		{
			Assert("Pre-condition", !ExtraInfo.HasChanges);
			bool t9_ReconciliationTotalAccountInfoRefreshBindingCalled = false;
			ExtraInfo.T9_ReconciliationTotalAccountInfo.ValueChanged += delegate
			{
				t9_ReconciliationTotalAccountInfoRefreshBindingCalled = true;
			};
			ExtraInfo.ShouldReconciliateTotal = true;
			Assert("Should set HasChanges to true", ExtraInfo.HasChanges);
			Assert("Should call RefreshBinding on T9_ReconciliationTotalAccountInfoR", t9_ReconciliationTotalAccountInfoRefreshBindingCalled);
		}

		public void TestOnSaving_SubclassifiedByDebtor()
		{
			ExtraInfo.FillWithValidTestData();
			JASOrgCreditorGroup creditorGroup = ExtraInfo.MappedCreditorGroups.AddNew();
			creditorGroup.FillWithValidTestData();
			ExtraInfo.T9_SubClassificationCode = CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByCreditor;
			Factory.Save();
			CognosAccGLAccountDescriptorExtraInfo extraInfoFromOtherFactory = new BusinessObjectFactory().Load<CognosAccGLAccountDescriptorExtraInfo>(ExtraInfo.PK);
			CognosCreditorMapping[] creditorMappings = GetExtraInfoCreditorMappings(extraInfoFromOtherFactory);
			AssertEquals("The mapped creditor groups should be saved in the DB", 1, extraInfoFromOtherFactory.MappedCreditorGroups.Count);
			AssertEquals("The mapped creditor groups should be saved in the DB", 1, creditorMappings.Length);
			JASOrgDebtorGroup debtorGroup = extraInfoFromOtherFactory.MappedDebtorGroups.AddNew();
			debtorGroup.FillWithValidTestData();
			extraInfoFromOtherFactory.T9_SubClassificationCode = CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByDebtor;
			extraInfoFromOtherFactory.Factory.Save();
			AssertEquals("Creditor mapping should be removed on saving because debtor is selected", 0, extraInfoFromOtherFactory.MappedCreditorGroups.Count);
			Assert("Creditor mapping should be deleted", creditorMappings[0].IsDeleted);
			Assert("Creditor group should not be deleted, only the mapping", !creditorGroup.IsDeleted);
			AssertEquals("Debtor mapping should not be removed on saving because debtor is selected", 1, extraInfoFromOtherFactory.MappedDebtorGroups.Count);
		}

		public void TestOnSaving_SubclassifiedByCreditor()
		{
			ExtraInfo.FillWithValidTestData();
			JASOrgDebtorGroup debtorGroup = ExtraInfo.MappedDebtorGroups.AddNew();
			debtorGroup.FillWithValidTestData();
			ExtraInfo.T9_SubClassificationCode = CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByDebtor;
			Factory.Save();
			CognosAccGLAccountDescriptorExtraInfo extraInfoFromOtherFactory = new BusinessObjectFactory().Load<CognosAccGLAccountDescriptorExtraInfo>(ExtraInfo.PK);
			CognosDebtorMapping[] debtorMappings = GetExtraInfoDebtorMappings(extraInfoFromOtherFactory);
			AssertEquals("The mapped debtor groups should be saved in the DB", 1, extraInfoFromOtherFactory.MappedDebtorGroups.Count);
			AssertEquals("The mapped debtor groups should be saved in the DB", 1, debtorMappings.Length);
			JASOrgCreditorGroup creditorGroup = extraInfoFromOtherFactory.MappedCreditorGroups.AddNew();
			creditorGroup.FillWithValidTestData();
			extraInfoFromOtherFactory.T9_SubClassificationCode = CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByCreditor;
			extraInfoFromOtherFactory.Factory.Save();
			AssertEquals("Debtor mapping should be removed on saving because creditor is selected", 0, extraInfoFromOtherFactory.MappedDebtorGroups.Count);
			Assert("Debtor mapping should be deleted", debtorMappings[0].IsDeleted);
			Assert("Debtor group should not be deleted, only the mapping", !debtorGroup.IsDeleted);
			AssertEquals("Creditor mapping should not be removed on saving because creditor is selected", 1, extraInfoFromOtherFactory.MappedCreditorGroups.Count);
		}

		public void TestOnSaving_SubclassifiedByAge()
		{
			ExtraInfo.FillWithValidTestData();
			JASOrgDebtorGroup debtorGroup = ExtraInfo.MappedDebtorGroups.AddNew();
			debtorGroup.FillWithValidTestData();
			JASOrgCreditorGroup creditorGroup = ExtraInfo.MappedCreditorGroups.AddNew();
			creditorGroup.FillWithValidTestData();
			CognosCreditorMapping[] creditorMappings = GetExtraInfoCreditorMappings(ExtraInfo);
			CognosDebtorMapping[] debtorMappings = GetExtraInfoDebtorMappings(ExtraInfo);
			AssertEquals("Pre-condition", 1, creditorMappings.Length);
			AssertEquals("Pre-condition", 1, debtorMappings.Length);
			ExtraInfo.T9_SubClassificationCode = CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByAge;
			Factory.Save();
			Assert("Creditor mapping should be deleted", creditorMappings[0].IsDeleted);
			AssertEquals("Creditor mapping should be removed on saving because age is selected", 0, ExtraInfo.MappedCreditorGroups.Count);
			Assert("Debtor mapping should be deleted", debtorMappings[0].IsDeleted);
			AssertEquals("Debtor mapping should be removed on saving because age is selected", 0, ExtraInfo.MappedDebtorGroups.Count);
			Assert("Creditor group should not be deleted, only the mapping", !creditorGroup.IsDeleted);
			Assert("Debtor group should not be deleted, only the mapping", !debtorGroup.IsDeleted);
		}

		public void TestDelete()
		{
			AssertEquals("Pre-condition", 0, GetExtraInfoCreditorMappings(ExtraInfo).Length);
			AssertEquals("Pre-condition", 0, GetExtraInfoDebtorMappings(ExtraInfo).Length);
			JASOrgCreditorGroup creditorGroup1 = ExtraInfo.MappedCreditorGroups.AddNew();
			JASOrgCreditorGroup creditorGroup2 = ExtraInfo.MappedCreditorGroups.AddNew();
			JASOrgDebtorGroup debtorGroup1 = ExtraInfo.MappedDebtorGroups.AddNew();
			JASOrgDebtorGroup debtorGroup2 = ExtraInfo.MappedDebtorGroups.AddNew();
			CognosCreditorMapping[] creditorMappings = GetExtraInfoCreditorMappings(ExtraInfo);
			CognosDebtorMapping[] debtorMappings = GetExtraInfoDebtorMappings(ExtraInfo);
			AssertEquals("Should be mapped", 2, creditorMappings.Length);
			AssertEquals("Should be mapped", 2, debtorMappings.Length);
			ExtraInfo.Delete();
			Assert("Should be deleted", ExtraInfo.IsDeleted);
			Assert("Mapping should be removed and deleted", creditorMappings[0].IsDeleted);
			Assert("Mapping should be removed and deleted", creditorMappings[1].IsDeleted);
			Assert("Mapping should be removed and deleted", debtorMappings[0].IsDeleted);
			Assert("Mapping should be removed and deleted", debtorMappings[1].IsDeleted);
			AssertEquals("Mapping should be removed from the pivot collection", 0, ExtraInfo.MappedCreditorGroups.Count);
			AssertEquals("Mapping should be removed from the pivot collection", 0, ExtraInfo.MappedDebtorGroups.Count);
			Assert("Should not be deleted, only mappings are deleted", !creditorGroup1.IsDeleted);
			Assert("Should not be deleted, only mappings are deleted", !creditorGroup2.IsDeleted);
			Assert("Should not be deleted, only mappings are deleted", !debtorGroup1.IsDeleted);
			Assert("Should not be deleted, only mappings are deleted", !debtorGroup2.IsDeleted);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals("Should be initialised on SetDefaultValues()", CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByDebtor, ExtraInfo.T9_SubClassificationCode);
			AssertEquals("Should be initialised on SetDefaultValues()", true, ExtraInfo.T9_IsPublished);
		}

		public void TestIsSubClassificationAccount()
		{
			Assert("Pre-condition", !ExtraInfo.IsSubClassificationAccount);
			ExtraInfo.GLAccountDescriptor.AJ_Language = Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger;
			ExtraInfo.GLAccountDescriptor.AJ_ReportCategory = CognosAccGLAccountDescriptor.CognosSubClassificationAccountType;
			ExtraInfo.GLAccountDescriptor.AJ_ReportType = CognosAccGLAccountDescriptor.ReportTypeCOA;
			Assert(ExtraInfo.IsSubClassificationAccount);
		}

		public void TestIsSubClassifyingAnotherSubClassificationAccount()
		{
			ExtraInfo.GLAccountDescriptor.AJ_Language = Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger;
			ExtraInfo.GLAccountDescriptor.AJ_ReportCategory = CognosAccGLAccountDescriptor.CognosSubClassificationAccountType;
			ExtraInfo.GLAccountDescriptor.AJ_ReportType = CognosAccGLAccountDescriptor.ReportTypeCOA;
			ExtraInfo.T9_AJ_AccountToBeSubClassified = Factory.New<CognosAccGLAccountDescriptor>().PK;
			Assert(!ExtraInfo.IsSubClassifyingAnotherSubClassificationAccount);
			ExtraInfo.AccountToBeSubClassified.AJ_Language = Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger;
			ExtraInfo.AccountToBeSubClassified.AJ_ReportCategory = CognosAccGLAccountDescriptor.CognosSubClassificationAccountType;
			ExtraInfo.AccountToBeSubClassified.AJ_ReportType = CognosAccGLAccountDescriptor.ReportTypeCOA;
			Assert(ExtraInfo.IsSubClassifyingAnotherSubClassificationAccount);
		}

		public void TestAccountToBeSubClassified()
		{
			ExtraInfo.T9_AJ_AccountToBeSubClassified = Factory.New<CognosAccGLAccountDescriptor>().PK;
			AssertNotNull(ExtraInfo.AccountToBeSubClassified);
		}

		public void TestIsSubClassifiedByDebtor()
		{
			ExtraInfo.T9_SubClassificationCode = CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByDebtor;
			Assert("Should be true", ExtraInfo.IsSubClassifiedByDebtor);
			ExtraInfo.T9_SubClassificationCode = CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByCreditor;
			Assert("Should be false", !ExtraInfo.IsSubClassifiedByDebtor);
			ExtraInfo.T9_SubClassificationCode = CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByAge;
			Assert("Should be false", !ExtraInfo.IsSubClassifiedByDebtor);
		}

		public void TestIsSubClassifiedByCreditor()
		{
			ExtraInfo.T9_SubClassificationCode = CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByDebtor;
			Assert("Should be false", !ExtraInfo.IsSubClassifiedByCreditor);
			ExtraInfo.T9_SubClassificationCode = CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByCreditor;
			Assert("Should be true", ExtraInfo.IsSubClassifiedByCreditor);
			ExtraInfo.T9_SubClassificationCode = CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByAge;
			Assert("Should be false", !ExtraInfo.IsSubClassifiedByCreditor);
		}

		public void TestIsSubClassifiedByAge()
		{
			ExtraInfo.T9_SubClassificationCode = CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByDebtor;
			Assert("Should be false", !ExtraInfo.IsSubClassifiedByAge);
			ExtraInfo.T9_SubClassificationCode = CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByCreditor;
			Assert("Should be false", !ExtraInfo.IsSubClassifiedByAge);
			ExtraInfo.T9_SubClassificationCode = CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByAge;
			Assert("Should be true", ExtraInfo.IsSubClassifiedByAge);
		}

		public void TestMappedDebtorGroups()
		{
			ExtraInfo.GLAccountDescriptor.AJ_Language = Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger;
			ExtraInfo.GLAccountDescriptor.AJ_ReportCategory = CognosAccGLAccountDescriptor.CognosSubClassificationAccountType;
			ExtraInfo.GLAccountDescriptor.AJ_ReportType = CognosAccGLAccountDescriptor.ReportTypeCOA;
			ExtraInfo.IsSubClassifiedByDebtor = true;
			AssertHasErrors("No Debtor Groups mapped, should have error", ExtraInfo.IsSubClassifiedByDebtorInfo);
			AssertEquals(typeof(ManyToManyCognosDebtorCollection), ExtraInfo.MappedDebtorGroups.GetType());
			Assert("Should be loaded in the getter", ExtraInfo.MappedDebtorGroups.IsLoaded);
			Assert("Should be registered as editable child object", ExtraInfo.IsRegisteredEditableChildObject(ExtraInfo.MappedDebtorGroups));
			ExtraInfo.MappedDebtorGroups.AddNew();
			AssertNoErrors("Should call ValidateIsSubClassifiedByDebtor when collection count changed", ExtraInfo.IsSubClassifiedByDebtorInfo);
		}

		public void TestMappedCreditorGroups()
		{
			ExtraInfo.GLAccountDescriptor.AJ_Language = Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger;
			ExtraInfo.GLAccountDescriptor.AJ_ReportCategory = CognosAccGLAccountDescriptor.CognosSubClassificationAccountType;
			ExtraInfo.GLAccountDescriptor.AJ_ReportType = CognosAccGLAccountDescriptor.ReportTypeCOA;
			ExtraInfo.IsSubClassifiedByCreditor = true;
			AssertHasErrors("No Creditor Groups mapped, should have error", ExtraInfo.IsSubClassifiedByCreditorInfo);
			AssertEquals(typeof(ManyToManyCognosCreditorCollection), ExtraInfo.MappedCreditorGroups.GetType());
			Assert("Should be loaded in the getter", ExtraInfo.MappedCreditorGroups.IsLoaded);
			Assert("Should be registered as editable child object", ExtraInfo.IsRegisteredEditableChildObject(ExtraInfo.MappedCreditorGroups));
			ExtraInfo.MappedCreditorGroups.AddNew();
			AssertNoErrors("Should call ValidateIsSubClassifiedByCreditor when collection count changed", ExtraInfo.IsSubClassifiedByCreditorInfo);
		}

		public void TestUniqueIndexFailureHandler()
		{
			ErrorReporter.Clear();
			BusinessObjectFactory otherFactory;
			using (Env.SetTemporaryUserContext(User.PostMasterUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var account = Factory.NewWithValidTestData<CognosAccGLAccountDescriptor>();
				account.AJ_LocalAccountNumber = "101";
				Factory.Save();
				account.ExtraInfo.FillWithValidTestData();
				account.ExtraInfo.HasChanges = true;
				otherFactory = new BusinessObjectFactory();
				CognosAccGLAccountDescriptor accountInOtherFactory = otherFactory.Load<CognosAccGLAccountDescriptor>(account.PK);
				accountInOtherFactory.ExtraInfo.FillWithValidTestData();
				accountInOtherFactory.ExtraInfo.HasChanges = true;
				Factory.Save();
			}

			try
			{
				otherFactory.Save();
				Fail("Save should fail");
			}
			catch (ZSaveException ex)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZExceptionReporting.HandleSaveException(ex);
				AssertEquals("should have shown an error", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("The Cognos Account settings for Account '101' has already been configured by another user. Please close and re-open the form.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region Implementation
		CognosCreditorMapping[] GetExtraInfoCreditorMappings(CognosAccGLAccountDescriptorExtraInfo extraInfo)
		{
			ZQuery filter = new ZQuery(ClientCognosSubClassificationAccountCreditorMappingSchema.T7_T9, extraInfo.PK);
			return extraInfo.Factory.Load<CognosCreditorMapping>(filter);
		}

		CognosDebtorMapping[] GetExtraInfoDebtorMappings(CognosAccGLAccountDescriptorExtraInfo extraInfo)
		{
			ZQuery filter = new ZQuery(ClientCognosSubClassificationAccountDebtorMappingSchema.T8_T9, extraInfo.PK);
			return extraInfo.Factory.Load<CognosDebtorMapping>(filter);
		}

		protected override Dictionary<string, IZType> CachedValueForSettingValueCallsRefreshBindingTestCore
		{
			get
			{
				Dictionary<string, IZType> result = base.CachedValueForSettingValueCallsRefreshBindingTestCore;
				result[ClientCognosAccGLAccountDescriptorExtraInfoSchema.Constants.T9_SubClassificationCode] = (ZString)CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByCreditor;
				return result;
			}
		}

		CognosAccGLAccountDescriptorExtraInfo ExtraInfo
		{
			get
			{
				if (fExtraInfo == null)
				{
					CognosAccGLAccountDescriptor account = Factory.New<CognosAccGLAccountDescriptor>();
					// AccGLAccountDescriptor is not valid with empty AJ_DebitCredit.
					account.AJ_DebitCredit = Core.Constants.DebitCredit.Debit;
					fExtraInfo = account.ExtraInfo;
				}

				return fExtraInfo;
			}
		}

		CognosAccGLAccountDescriptorExtraInfo fExtraInfo;
		#endregion
	}
}
