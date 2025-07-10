using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing
{
	public class TaxIdAndTaxMessageMappingHelperTest : TestCaseWithFactory
	{
		#region ValidateMapping

		public void TestValidateMapping_LineTypes()
		{
			AssertEquals(null, TaxMappingHelper.ValidateMapping(TransactionLineTypes.Cost, taxRate1, taxMsg1));
			AssertEquals(GetErrorMsg("CST", "TaxRate01", "TaxMsg02"), TaxMappingHelper.ValidateMapping(TransactionLineTypes.Cost, taxRate1, taxMsg2));
		}

		public void TestCreateValidator()
		{
			PrepareRegistryValue(AccountingMasterFilesConstants.TaxIDAndTaxMessageValidationOption.TaxMessage);
			var validator = TaxMappingHelper.CreateValidator_ForTestOnly(GlbCompany.CurrentCompany.PK);
			AssertType<TaxIdAndTaxMessageMappingLimitTaxMessagesValidator>(validator);

			PrepareRegistryValue(AccountingMasterFilesConstants.TaxIDAndTaxMessageValidationOption.TaxID);
			validator = TaxMappingHelper.CreateValidator_ForTestOnly(GlbCompany.CurrentCompany.PK);
			AssertType<TaxIdAndTaxMessageMappingLimitTaxIDsValidator>(validator);

			using (Env.Instance.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, TestObjectCreator.NonCurrentBranch.PK.ToGuid(), Env.Instance.CurrentDepartment.PK))
			{
				PrepareRegistryValue(AccountingMasterFilesConstants.TaxIDAndTaxMessageValidationOption.TaxMessage);
			}
			AssertType<TaxIdAndTaxMessageMappingLimitTaxMessagesValidator>(TaxMappingHelper.CreateValidator_ForTestOnly(TestObjectCreator.NonCurrentBranch.Company.PK));

			using (Env.Instance.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, TestObjectCreator.NonCurrentBranch.PK.ToGuid(), Env.Instance.CurrentDepartment.PK))
			{
				PrepareRegistryValue(AccountingMasterFilesConstants.TaxIDAndTaxMessageValidationOption.TaxID);
			}
			AssertType<TaxIdAndTaxMessageMappingLimitTaxIDsValidator>(TaxMappingHelper.CreateValidator_ForTestOnly(TestObjectCreator.NonCurrentBranch.Company.PK));
		}

		#endregion

		#region ValidateMappingForTaxOverride

		public void TestValidateMappingForTaxOverride_ParentIsChargeCode()
		{
			var lineType = new[] { TransactionLineTypes.Cost };

			var taxOverrideValid = TestObjectCreator.CreateTaxOverride(
				TestObjectCreator.FRT,
				taxCodePK: taxRate1.PK,
				vatClassPK: taxMsg1.PK
			);
			AssertEquals(null, TaxMappingHelper.ValidateMappingForTaxOverride(lineType, taxOverrideValid));
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("We should get same result even swtich to another company", null, TaxMappingHelper.ValidateMappingForTaxOverride(lineType, taxOverrideValid));
			}

			var taxOverrideInvalid = TestObjectCreator.CreateTaxOverride(
				TestObjectCreator.FRT,
				taxCodePK: taxRate1.PK,
				vatClassPK: taxMsg2.PK
			);
			AssertEquals(GetErrorMsgForTaxOverride("CST", "TaxRate01", "TaxMsg02"), TaxMappingHelper.ValidateMappingForTaxOverride(lineType, taxOverrideInvalid));
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("We should get same result even swtich to another company", GetErrorMsgForTaxOverride("CST", "TaxRate01", "TaxMsg02"), TaxMappingHelper.ValidateMappingForTaxOverride(lineType, taxOverrideInvalid));
			}
		}

		public void TestValidateMappingForTaxOverride_ParentIsTaxOverrideGroup()
		{
			var lineType = new[] { TransactionLineTypes.Cost };
			var group = Factory.NewWithValidTestData<AccTaxOverrideGroup>();

			var taxOverrideValid = TestObjectCreator.CreateTaxOverride(
				TestObjectCreator.FRT,
				taxCodePK: taxRate1.PK,
				vatClassPK: taxMsg1.PK
			);
			AssertEquals(null, TaxMappingHelper.ValidateMappingForTaxOverride(lineType, taxOverrideValid));
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("Do not have error since registry setting is default for NonCurrentCompany.", null, TaxMappingHelper.ValidateMappingForTaxOverride(lineType, taxOverrideValid));
			}

			var taxOverrideInvalid = TestObjectCreator.CreateTaxOverride(
				group,
				taxCodePK: taxRate1.PK,
				vatClassPK: taxMsg2.PK
			);
			AssertEquals(GetErrorMsgForTaxOverride("CST", "TaxRate01", "TaxMsg02"), TaxMappingHelper.ValidateMappingForTaxOverride(lineType, taxOverrideInvalid));
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("Do not have error since registry setting is default for NonCurrentCompany.", null, TaxMappingHelper.ValidateMappingForTaxOverride(lineType, taxOverrideInvalid));
			}
		}

		public void TestValidateMappingForTaxOverride_MixType()
		{
			PrepareRegistryValue(AccountingMasterFilesConstants.TaxIDAndTaxMessageValidationOption.TaxID
				, (TransactionLineTypes.Revenue, taxRate1, taxMsg1)
				, (TransactionLineTypes.Cost, taxRate1, taxMsg1)
			);

			var lineTypes = new[] { TransactionLineTypes.Cost, TransactionLineTypes.Revenue };

			var taxOverrideValid = TestObjectCreator.CreateTaxOverride(
				TestObjectCreator.FRT,
				taxCodePK: taxRate1.PK,
				vatClassPK: taxMsg1.PK
			);
			AssertEquals(null, TaxMappingHelper.ValidateMappingForTaxOverride(lineTypes, taxOverrideValid));

			var taxOverrideInvalid_Msg02 = TestObjectCreator.CreateTaxOverride(
				TestObjectCreator.FRT,
				taxCodePK: taxRate1.PK,
				vatClassPK: taxMsg2.PK
			);
			var expectedMsg = @"Tax Override cannot be saved due to invalid Tax ID and Tax Message combination.
Line Type=CST, Tax ID=TaxRate01, Tax Message=TaxMsg02
Line Type=REV, Tax ID=TaxRate01, Tax Message=TaxMsg02";

			AssertEquals(expectedMsg, TaxMappingHelper.ValidateMappingForTaxOverride(lineTypes, taxOverrideInvalid_Msg02));

			var taxOverrideInvalid_EmptyMsg = TestObjectCreator.CreateTaxOverride(
				TestObjectCreator.FRT,
				taxCodePK: taxRate1.PK,
				vatClassPK: ZGuid.Empty
			);
			var expectedMsgForNull = @"Tax Override cannot be saved due to invalid Tax ID and Tax Message combination.
Line Type=CST, Tax ID=TaxRate01, Tax Message=
Line Type=REV, Tax ID=TaxRate01, Tax Message=";

			AssertEquals(expectedMsgForNull, TaxMappingHelper.ValidateMappingForTaxOverride(lineTypes, taxOverrideInvalid_EmptyMsg));
		}

		#endregion

		#region ValidateMappingForLine

		public void TestValidateMappingForLine()
		{
			var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000001", TestObjectCreator.AUD, 1m, 190m, 0m, 190m, 0m);
			var line = header.Lines[0];

			line.AL_AT = taxRate1.PK;
			line.AL_A9_VATClass = taxMsg1.PK;

			AssertEquals(null, TaxMappingHelper.ValidateMappingForLine(line));

			line.AL_A9_VATClass = taxMsg2.PK;

			AssertEquals(GetErrorMsg("CST", "TaxRate01", "TaxMsg02"), TaxMappingHelper.ValidateMappingForLine(line));
		}

		public void TestValidateMappingForReversedTransaction_InDatabase()
		{
			var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000001", TestObjectCreator.AUD, 1m, 190m, 0m, 190m, 0m);
			var line = header.Lines[0];
			line.AL_AT = taxRate1.PK;
			line.AL_A9_VATClass = taxMsg2.PK;

			using (AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.DefaultValue))
			{
				Factory.Save();
			}

			var reversingFactory = new ReversingFactory();
			var reversingBase = reversingFactory.NewReversing(header);
			reversingBase.Reverse();

			AssertEquals("Original trasaction line is valid", null, TaxMappingHelper.ValidateMappingForLine(line));
			AssertEquals("Reversed transaction line is valid", null, TaxMappingHelper.ValidateMappingForLine(((TransactionHeaderWithLines)reversingBase.ReverseTransaction).Lines[0]));
		}

		public void TestValidateMappingForReversedTransaction_NotInDatabase()
		{
			var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000001", TestObjectCreator.AUD, 1m, 190m, 0m, 190m, 0m);
			var line = header.Lines[0];
			line.AL_AT = taxRate1.PK;
			line.AL_A9_VATClass = taxMsg2.PK;

			var reversingFactory = new ReversingFactory();
			var reversingBase = reversingFactory.NewReversing(header);
			reversingBase.Reverse();

			AssertEquals("Original trasaction line is invalid", GetErrorMsg("CST", "TaxRate01", "TaxMsg02"), TaxMappingHelper.ValidateMappingForLine(line));
			AssertEquals("Reversed transaction line is valid", null, TaxMappingHelper.ValidateMappingForLine(((TransactionHeaderWithLines)reversingBase.ReverseTransaction).Lines[0]));
		}

		public void TestValidateMappingForLine_PostedLines()
		{
			var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000001", TestObjectCreator.AUD, 1m, 190m, 0m, 190m, 0m);
			var line = header.Lines[0];
			line.AL_AT = taxRate1.PK;
			line.AL_A9_VATClass = taxMsg2.PK;

			AssertEquals("Precondition", false, line.IsInDatabase);
			AssertEquals(GetErrorMsg("CST", "TaxRate01", "TaxMsg02"), TaxMappingHelper.ValidateMappingForLine(line));

			using (AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.DefaultValue))
			{
				Factory.Save();
			}

			AssertEquals("Precondition", true, line.IsInDatabase);
			AssertEquals("Should skip validation because original Line Type is NOT UCT and the line is in database", null, TaxMappingHelper.ValidateMappingForLine(line));
		}

		public void TestValidateMappingForLine_UnapprovedCostLine()
		{
			var invoice = Factory.NewWithValidTestData<UAInvoice>();
			invoice.AH_TransactionNum = "1111";
			var line = invoice.Lines.AddNew();
			line.AL_AT = taxRate1.PK;
			line.AL_A9_VATClass = taxMsg2.PK;
			Factory.Save();

			AssertEquals("Precondition", true, line.IsInDatabase);
			AssertEquals("Precondition", TransactionLineTypes.UnapprovedCost, line.AL_LineTypeInfo.Value);
			AssertEquals("We do not validate UnapprovedCost(UCT) line.", null, TaxMappingHelper.ValidateMappingForLine(line));

			line.AL_LineType = TransactionLineTypes.Cost;

			AssertEquals("Precondition", true, line.IsInDatabase);
			AssertEquals("Precondition", TransactionLineTypes.Cost, line.AL_LineTypeInfo.Value);
			AssertEquals("Should do validation because current Line Type is CST.", GetErrorMsg("CST", "TaxRate01", "TaxMsg02"), TaxMappingHelper.ValidateMappingForLine(line));
		}

		public void TestValidateMappingForLine_HasSkipTaxIdAndTaxMessageMappingValidationContextShouldSkipValidation()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var line = invoice.Lines.AddNew();
			line.AL_AT = taxRate1.PK;
			line.AL_A9_VATClass = taxMsg2.PK;
			line.AL_LineType = TransactionLineTypes.Cost;

			AssertEquals(GetErrorMsg("CST", "TaxRate01", "TaxMsg02"), TaxMappingHelper.ValidateMappingForLine(line));

			((InvoicingLineBase)line).SetContext(BusinessContext.SkipTaxIdAndTaxMessageMappingValidation);

			AssertEquals("Should skip validation.", null, TaxMappingHelper.ValidateMappingForLine(line));
		}

		public void TestAddStampDutyLine_HasSkipTaxIdAndTaxMessageMappingValidationContext()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Italy);

			var taxRate = AccTaxRate.FindExistingTaxRate(Factory, "ART7", AccTaxRate.Types.Rated, Core.Constants.CountryCodes.Italy);

			var taxRateExempt = AccTaxRate.FindExistingTaxRate(Factory, "EXEMPT", AccTaxRate.Types.Exempt, Core.Constants.CountryCodes.Italy);

			Factory.Save();

			AccountingConfigurationRegistry.Instance.TaxIDsAttractingStampDuty.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, taxRate.PK.ToString());
			AccountingConfigurationRegistry.Instance.StampDutyFixedAmount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 1.81m);
			AccountingConfigurationRegistry.Instance.StampDutyThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 77.47m);

			var stampDutyChargeCode = TestObjectCreator.CreateChargeCode("BOLLO", "Stamp Duty", "MRG", 1m, taxRateExempt, TestObjectCreator.WHTFREE1);
			AccountingConfigurationRegistry.Instance.StampDutyChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, stampDutyChargeCode.PK.ToGuid());

			var italyOrg = TestObjectCreator.CreateOrgHeader("ITORG", false, true, "ITROM");

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = italyOrg.PK;
			var invoiceLine = invoice.AddStampDutyLine();

			AssertEquals(true, invoiceLine.HasContext(BusinessContext.SkipTaxIdAndTaxMessageMappingValidation));

			var creditNote = Factory.NewWithValidTestData<ARCreditNote>();
			creditNote.AH_OH = italyOrg.PK;
			var creditNoteLine = creditNote.AddStampDutyLine();

			AssertEquals(true, creditNoteLine.HasContext(BusinessContext.SkipTaxIdAndTaxMessageMappingValidation));
		}

		public void TestValidateMappingForLine_TransactionBelongsToGroup_IsCancelled()
		{
			var line = Factory.New<AccTransactionLines>();
			AssertNull("Precondition", line.TransactionHeader);
			AssertNoExceptionThrown(() => TaxMappingHelper.ValidateMappingForLine(line));

			var header = Factory.New<AccTransactionHeader>();
			line.AL_AH = header.PK;
			AssertNotNull("Precondition", line.TransactionHeader);

			line.AL_LineType = TransactionLineTypes.Cost;
			line.AL_AT = taxRate1.PK;
			line.AL_A9_VATClass = taxMsg2.PK;
			AssertEquals("Precondition", ZGuid.Empty, line.TransactionHeader.AH_TransactionBelongsToGroup);
			AssertEquals("Precondition", false, line.TransactionHeader.AH_IsCancelled);
			AssertEquals(GetErrorMsg("CST", "TaxRate01", "TaxMsg02"), TaxMappingHelper.ValidateMappingForLine(line));

			line.TransactionHeader.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();
			line.TransactionHeader.AH_IsCancelled = false;
			AssertEquals(GetErrorMsg("CST", "TaxRate01", "TaxMsg02"), TaxMappingHelper.ValidateMappingForLine(line));

			line.TransactionHeader.AH_TransactionBelongsToGroup = ZGuid.Empty;
			line.TransactionHeader.AH_IsCancelled = true;
			AssertEquals(GetErrorMsg("CST", "TaxRate01", "TaxMsg02"), TaxMappingHelper.ValidateMappingForLine(line));

			line.TransactionHeader.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();
			line.TransactionHeader.AH_IsCancelled = true;
			AssertEquals("Should skip validation because the header was reversed", null, TaxMappingHelper.ValidateMappingForLine(line));
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();

			PrepareTaxInfo();
			TaxMappingHelper = new TaxIdAndTaxMessageMappingHelper(); 
			PrepareRegistryValue(AccountingMasterFilesConstants.TaxIDAndTaxMessageValidationOption.TaxID, (TransactionLineTypes.Cost, taxRate1, taxMsg1));
		}

		#region Implementation

		void PrepareRegistryValue(string validationOptionType, params (string LineType, AccTaxRate TaxRate, AccInvMsg TaxMessage)[] settings)
		{
			var config = TestObjectCreator.CreateTaxIdAndTaxMessageCombinationRulesConfiguration(settings);
			config.ValidationOption = validationOptionType;
			AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);
		}

		void PrepareTaxInfo()
		{
			taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate1.AT_Type = AccTaxRate.Types.Rated;
			taxRate1.AT_Code = "TaxRate01";

			taxMsg1 = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg1.A9_Code = "TaxMsg01";

			taxMsg2 = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg2.A9_Code = "TaxMsg02";

			Factory.Save();
		}

		string GetErrorMsg(string lineType, string taxId, string taxMsg) => $@"Transaction cannot be saved/posted due to invalid Tax ID and Tax Message combination.
Line Type={lineType}, Tax ID={taxId}, Tax Message={taxMsg}";

		string GetErrorMsgForTaxOverride(string lineType, string taxId, string taxMsg) => $@"Tax Override cannot be saved due to invalid Tax ID and Tax Message combination.
Line Type={lineType}, Tax ID={taxId}, Tax Message={taxMsg}";

		AccTaxRate taxRate1;
		AccInvMsg taxMsg1, taxMsg2;

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		TaxIdAndTaxMessageMappingHelper TaxMappingHelper;

		#endregion
	}
}
