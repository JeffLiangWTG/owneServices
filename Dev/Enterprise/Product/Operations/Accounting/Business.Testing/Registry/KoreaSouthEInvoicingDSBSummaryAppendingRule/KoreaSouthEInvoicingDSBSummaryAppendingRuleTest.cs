using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(KoreaSouthEInvoicingDSBSummaryAppendingRule))]
	class KoreaSouthEInvoicingDSBSummaryAppendingRuleTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestTaxIdPK()
		{
			var taxRate1 = Factory.New<AccTaxRate>();
			taxRate1.FillWithValidTestData();
			taxRate1.AT_PostingGroupId = 1;

			var taxRate2 = Factory.New<AccTaxRate>();
			taxRate2.FillWithValidTestData();
			taxRate2.AT_PostingGroupId = 2;

			Factory.Save();

			var bizObj = BizObj;

			AssertEquals("Order should be editable.", false, bizObj.TaxIdPKInfo.ReadOnly);

			bizObj.TaxIdPK = taxRate1.PK;
			AssertEquals(taxRate1.PK, bizObj.TaxIdPK);

			bizObj.TaxIdPK = taxRate2.PK;
			AssertEquals(taxRate2.PK, bizObj.TaxIdPK);
		}

		public void TestPostingGroup()
		{
			var taxRate1 = Factory.New<AccTaxRate>();
			taxRate1.FillWithValidTestData();
			taxRate1.AT_PostingGroupId = 1;

			var taxRate2 = Factory.New<AccTaxRate>();
			taxRate2.FillWithValidTestData();
			taxRate2.AT_PostingGroupId = 2;

			Factory.Save();

			var bizObj = BizObj;

			AssertEquals(ZShort.Zero, bizObj.PostingGroup);

			bizObj.TaxIdPK = taxRate1.PK;
			AssertEquals(taxRate1.AT_PostingGroupId, bizObj.PostingGroup);

			bizObj.TaxIdPK = taxRate2.PK;
			AssertEquals(taxRate2.AT_PostingGroupId, bizObj.PostingGroup);
		}

		public void TestOrder()
		{
			var bizObj = BizObj;

			AssertEquals("Order should be editable.", false, bizObj.OrderInfo.ReadOnly);

			bizObj.Order = 2;
			AssertEquals(2, bizObj.Order);

			bizObj.Order = 3;
			AssertEquals(3, bizObj.Order);
		}

		public void TestValidateOrder_CheckNumberNotNegative()
		{
			BizObj.OrderInfo.ClearAllNotifications();
			BizObj.Order = -1;
			BizObj.RunPreSaveValidation();
			Assert(BizObj.OrderInfo.HasError("Please enter an 'Order' greater than or equal to 0."));

			BizObj.Order = 0;
			BizObj.RunPreSaveValidation();
			Assert(!BizObj.OrderInfo.HasError("Please enter an 'Order' greater than or equal to 0."));
		}

		public void TestValidateDuplicateTaxId()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = company1.PK;
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();

			var taxRate1 = Factory.New<AccTaxRate>();
			taxRate1.FillWithValidTestData();
			taxRate1.AT_PostingGroupId = 1;
			taxRate1.AT_RN_NKCountry = Core.Constants.CountryCodes.KoreaSouth;

			Factory.Save();

			var collection = new KoreaSouthEInvoicingDSBSummaryAppendingRuleCollection(new FallbackLevel(company1.PK.ToGuid(), branch1.PK.ToGuid(), department1.PK.ToGuid()), Factory);
			var appendingRule1 = collection.AddNew();
			appendingRule1.Order = 0;
			appendingRule1.TaxIdPK = taxRate1.PK;
			var appendingRule2 = collection.AddNew();
			appendingRule2.Order = 0;

			appendingRule1.RunPreSaveValidation();
			Assert(!appendingRule1.RowNotifications.HasErrors());

			appendingRule2.TaxIdPK = taxRate1.PK;

			appendingRule1.RunPreSaveValidation();
			AssertContains("Duplicated Tax ID is not allowed.", appendingRule1.RowNotifications.GetErrors().FirstOrDefault().Message);
		}

		public void TestValidateOrder_ValidatePostGourpWithInconsistentOrder()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = company1.PK;
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();

			var taxRate1 = Factory.New<AccTaxRate>();
			taxRate1.FillWithValidTestData();
			taxRate1.AT_PostingGroupId = 1;
			taxRate1.AT_RN_NKCountry = Core.Constants.CountryCodes.KoreaSouth;

			var taxRate2 = Factory.New<AccTaxRate>();
			taxRate2.FillWithValidTestData();
			taxRate2.AT_PostingGroupId = 1;
			taxRate2.AT_RN_NKCountry = Core.Constants.CountryCodes.Australia;

			Factory.Save();

			var collection = new KoreaSouthEInvoicingDSBSummaryAppendingRuleCollection(new FallbackLevel(company1.PK.ToGuid(), branch1.PK.ToGuid(), department1.PK.ToGuid()), Factory);
			var appendingRule1 = collection.AddNew();
			appendingRule1.Order = 0;
			appendingRule1.TaxIdPK = taxRate1.PK;
			var appendingRule2 = collection.AddNew();
			appendingRule2.Order = 0;
			appendingRule2.TaxIdPK = taxRate2.PK;

			appendingRule1.RunPreSaveValidation();
			Assert("0 should not be validated.", !appendingRule1.RowNotifications.HasErrors());

			appendingRule1.Order = 1;

			Assert(!appendingRule1.RowNotifications.HasErrors());

			appendingRule2.Order = 2;

			appendingRule1.RunPreSaveValidation();
			AssertContains("The Tax ID with the same posting group number must correspond to the same Order Number.", appendingRule1.RowNotifications.GetErrors().FirstOrDefault().Message);
		}

		public void TestValidateOrder_ValidateDuplicatedOrder()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = company1.PK;
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();

			var taxRate1 = Factory.New<AccTaxRate>();
			taxRate1.FillWithValidTestData();
			taxRate1.AT_PostingGroupId = 1;
			taxRate1.AT_RN_NKCountry = Core.Constants.CountryCodes.KoreaSouth;

			var taxRate2 = Factory.New<AccTaxRate>();
			taxRate2.FillWithValidTestData();
			taxRate2.AT_PostingGroupId = 2;
			taxRate2.AT_RN_NKCountry = Core.Constants.CountryCodes.Australia;

			Factory.Save();

			var collection = new KoreaSouthEInvoicingDSBSummaryAppendingRuleCollection(new FallbackLevel(company1.PK.ToGuid(), branch1.PK.ToGuid(), department1.PK.ToGuid()), Factory);
			var appendingRule1 = collection.AddNew();
			appendingRule1.Order = 0;
			appendingRule1.TaxIdPK = taxRate1.PK;
			var appendingRule2 = collection.AddNew();
			appendingRule2.Order = 0;
			appendingRule2.TaxIdPK = taxRate2.PK;

			appendingRule1.RunPreSaveValidation();
			Assert("0 should not be validated.", !appendingRule1.RowNotifications.HasErrors());

			appendingRule1.Order = 1;
			appendingRule2.Order = 1;

			appendingRule1.RunPreSaveValidation();
			Assert(appendingRule1.RowNotifications.HasErrors());
			AssertContains("Duplicated Order Number is not allowed.", appendingRule1.RowNotifications.GetErrors().FirstOrDefault().Message);
		}

		public void TestValidateOrder_ValidateExceededOrder()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = company1.PK;
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();

			var taxRate1 = Factory.New<AccTaxRate>();
			taxRate1.FillWithValidTestData();
			taxRate1.AT_PostingGroupId = 1;
			taxRate1.AT_RN_NKCountry = Core.Constants.CountryCodes.KoreaSouth;

			var taxRate2 = Factory.New<AccTaxRate>();
			taxRate2.FillWithValidTestData();
			taxRate2.AT_PostingGroupId = 2;
			taxRate2.AT_RN_NKCountry = Core.Constants.CountryCodes.Australia;

			Factory.Save();

			var collection = new KoreaSouthEInvoicingDSBSummaryAppendingRuleCollection(new FallbackLevel(company1.PK.ToGuid(), branch1.PK.ToGuid(), department1.PK.ToGuid()), Factory);
			var appendingRule1 = collection.AddNew();
			appendingRule1.TaxIdPK = taxRate1.PK;
			appendingRule1.Order = 0;

			appendingRule1.RunPreSaveValidation();
			Assert("0 should not be validated.", !appendingRule1.RowNotifications.HasErrors());

			appendingRule1.Order = 1;
			appendingRule1.RunPreSaveValidation();
			Assert(!appendingRule1.RowNotifications.HasErrors());

			appendingRule1.Order = 2;
			appendingRule1.RunPreSaveValidation();
			AssertContains("Number '2' is exceeded the total number of items.", appendingRule1.RowNotifications.GetErrors().FirstOrDefault().Message);

			var appendingRule2 = collection.AddNew();
			appendingRule2.TaxIdPK = taxRate2.PK;
			appendingRule2.Order = 0;
			appendingRule1.RunPreSaveValidation();
			Assert(!appendingRule1.RowNotifications.HasErrors());

			collection.RemoveAndDelete(appendingRule2);
			appendingRule1.RunPreSaveValidation();
			AssertContains("Number '2' is exceeded the total number of items.", appendingRule1.RowNotifications.GetErrors().FirstOrDefault().Message);

			appendingRule1.Order = 3;
			Assert(!appendingRule1.RowNotifications.HasErrors());

			appendingRule1.RunPreSaveValidation();
			AssertContains("Number '3' is exceeded the total number of items.", appendingRule1.RowNotifications.GetErrors().FirstOrDefault().Message);

			appendingRule1.Order = 1;
			Assert(!appendingRule1.RowNotifications.HasErrors());
		}

		public void TestTaxIds()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = company1.PK;
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = company2.PK;
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();

			Factory.Save();

			var rule = new KoreaSouthEInvoicingDSBSummaryAppendingRule(new FallbackLevel(company1.PK.ToGuid(), branch1.PK.ToGuid(), department1.PK.ToGuid()), Factory);
			AssertEquals("(AT_RN_NKCountry = 'KR') and (AT_RN_NKCountry = 'KR' and (AT_IsActive = 1 and AT_Code <> 'EXCLUDE' and AT_Code <> 'NOTREPORT'))", rule.TaxIds.CompleteFilter.LiteralTextADO);

			rule = new KoreaSouthEInvoicingDSBSummaryAppendingRule(new FallbackLevel(company2.PK.ToGuid(), branch2.PK.ToGuid(), department2.PK.ToGuid()), Factory);
			AssertEquals("(AT_RN_NKCountry = 'AU') and (AT_RN_NKCountry = 'AU' and (AT_IsActive = 1 and AT_Code <> 'EXCLUDE' and AT_Code <> 'NOTREPORT'))", rule.TaxIds.CompleteFilter.LiteralTextADO);
		}

		public void TestValidateValidTaxId()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = company1.PK;
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();

			var taxRate1 = Factory.New<AccTaxRate>();
			taxRate1.FillWithValidTestData();
			taxRate1.AT_RN_NKCountry = Core.Constants.CountryCodes.KoreaSouth;

			var taxRate2 = Factory.New<AccTaxRate>();
			taxRate2.FillWithValidTestData();
			taxRate2.AT_RN_NKCountry = Core.Constants.CountryCodes.Australia;

			var taxRate3 = Factory.New<AccTaxRate>();
			taxRate3.FillWithValidTestData();
			taxRate3.AT_Code = KoreaSouthEInvoicingDSBSummaryAppendingRule.ExludeTaxCodes.EXCLUDE;
			taxRate3.AT_RN_NKCountry = Core.Constants.CountryCodes.KoreaSouth;

			var taxRate4 = Factory.New<AccTaxRate>();
			taxRate4.FillWithValidTestData();
			taxRate4.AT_Code = KoreaSouthEInvoicingDSBSummaryAppendingRule.ExludeTaxCodes.NOTREPORT;
			taxRate4.AT_RN_NKCountry = Core.Constants.CountryCodes.KoreaSouth;

			Factory.Save();

			var rule = new KoreaSouthEInvoicingDSBSummaryAppendingRule(new FallbackLevel(company1.PK.ToGuid(), branch1.PK.ToGuid(), department1.PK.ToGuid()), Factory);
			rule.TaxIdPK = ZGuid.Empty;
			rule.RunPreSaveValidation();
			AssertContains("Enter a valid selection.", rule.RowNotifications.GetErrors().FirstOrDefault().Message);

			rule.TaxIdPK = ZGuid.Invalid;
			rule.RunPreSaveValidation();
			AssertContains("Enter a valid selection.", rule.RowNotifications.GetErrors().FirstOrDefault().Message);

			rule.TaxIdPK = company1.PK;
			rule.RunPreSaveValidation();
			AssertContains("Enter a valid selection.", rule.RowNotifications.GetErrors().FirstOrDefault().Message);

			rule.TaxIdPK = taxRate2.PK;
			rule.RunPreSaveValidation();
			AssertContains("TaxID should can reload with TaxIds.CompleteFilter.", "Enter a valid selection.", rule.RowNotifications.GetErrors().FirstOrDefault().Message);

			rule.TaxIdPK = taxRate1.PK;
			rule.RunPreSaveValidation();
			Assert(!rule.RowNotifications.HasErrors());

			rule.TaxIdPK = taxRate3.PK;
			rule.RunPreSaveValidation();
			AssertContains("EXCLUDE is excluded.", "Enter a valid selection.", rule.RowNotifications.GetErrors().FirstOrDefault().Message);

			rule.TaxIdPK = taxRate4.PK;
			rule.RunPreSaveValidation();
			AssertContains("NOTREPORT is excluded.", "Enter a valid selection.", rule.RowNotifications.GetErrors().FirstOrDefault().Message);
		}

		public void TestCopyValuesToClone_SetSourceCurrentFallbackLevel()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = company1.PK;
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = company2.PK;
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();

			var taxRate1 = Factory.New<AccTaxRate>();
			taxRate1.FillWithValidTestData();
			taxRate1.AT_RN_NKCountry = Core.Constants.CountryCodes.KoreaSouth;

			Factory.Save();

			var ruleSource = new KoreaSouthEInvoicingDSBSummaryAppendingRule();
			ruleSource.CurrentFallbackLevel = null;
			ruleSource.TaxIdPK = taxRate1.PK;
			ruleSource.Order = 1;

			var fallbackLevel1 = new FallbackLevel(company1.PK.ToGuid(), branch1.PK.ToGuid(), department1.PK.ToGuid());
			var rule1 = (KoreaSouthEInvoicingDSBSummaryAppendingRule)ruleSource.Clone(fallbackLevel1, Factory);

			AssertEquals(taxRate1.PK, rule1.TaxIdPK);
			AssertEquals(1, rule1.Order);
			AssertEquals(fallbackLevel1, ruleSource.CurrentFallbackLevel);

			var fallbackLevel2 = new FallbackLevel(company2.PK.ToGuid(), branch2.PK.ToGuid(), department2.PK.ToGuid());
			var rule2 = (KoreaSouthEInvoicingDSBSummaryAppendingRule)ruleSource.Clone(fallbackLevel2, Factory);

			AssertEquals(taxRate1.PK, rule2.TaxIdPK);
			AssertEquals(1, rule2.Order);
			AssertEquals(fallbackLevel1, ruleSource.CurrentFallbackLevel);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new KoreaSouthEInvoicingDSBSummaryAppendingRule(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new KoreaSouthEInvoicingDSBSummaryAppendingRule(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected new KoreaSouthEInvoicingDSBSummaryAppendingRule BizObj
		{
			get { return (KoreaSouthEInvoicingDSBSummaryAppendingRule)base.BizObj; }
		}
	}
}
