using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(MatchEPaymentRecipientsBulk))]
	public class MatchEPaymentRecipientsBulkTest : MatchEPaymentRecipientsTest
	{
		public void TestDefaultPaymentReason()
		{
			TestMatchEPaymentRecipientsBulk.CurrentBeneficiary = null;
			TestMatchEPaymentRecipientsBulk.CreditorPK = ZGuid.Empty;
			Assert("Creditor field is blank, then the Default Payment Reason is blank.", TestMatchEPaymentRecipientsBulk.DefaultPaymentReason.IsEmpty);

			TestMatchEPaymentRecipientsBulk.CreditorPK = TestObjectCreator.Creditor1.PK;
			AssertEquals("Creditor field is entered, then default the Default Payment Reason from the registry.", EPaymentReasonCodes.OFXReasonCodes.ServicesTrade, TestMatchEPaymentRecipientsBulk.DefaultPaymentReason);
			TestMatchEPaymentRecipientsBulk.DefaultPaymentReason = "ABC";
			AssertHasError(TestMatchEPaymentRecipientsBulk.DefaultPaymentReasonInfo, "Enter a valid selection.");
			TestMatchEPaymentRecipientsBulk.DefaultPaymentReason = ZString.Empty;
			AssertNoError(TestMatchEPaymentRecipientsBulk.DefaultPaymentReasonInfo, "Enter a valid selection.");
			TestMatchEPaymentRecipientsBulk.DefaultPaymentReason = EPaymentReasonCodes.OFXReasonCodes.BusinessConsultancyAndPRSevices;
			AssertNoError(TestMatchEPaymentRecipientsBulk.DefaultPaymentReasonInfo, "Enter a valid selection.");
		}

		public void TestDefaultPaymentReasonReadOnlyness()
		{
			TestMatchEPaymentRecipientsBulk.CurrentBeneficiary = null;
			TestMatchEPaymentRecipientsBulk.CreditorPK = ZGuid.Empty;
			Assert("Recipient is unmatched and Creditor field is blank, then the Default Payment Reason is read only.", TestMatchEPaymentRecipientsBulk.DefaultPaymentReasonInfo.ReadOnly);

			TestMatchEPaymentRecipientsBulk.CreditorPK = TestObjectCreator.Creditor1.PK;
			Assert("Recipient is unmatched and Creditor field is entered, then the Default Payment Reason is enabled.", !TestMatchEPaymentRecipientsBulk.DefaultPaymentReasonInfo.ReadOnly);

			var beneficiary = Factory.NewWithValidTestData<AccEPaymentBeneficiary>();
			TestMatchEPaymentRecipientsBulk.CurrentBeneficiary = beneficiary;
			var accountDetails = Factory.NewWithValidTestData<AccAPAccountDetails>();
			accountDetails.A1_EPaymentBeneficiaryId = beneficiary.PK;
			Assert("Recipient is matched, then the Default Payment Reason is read only.", TestMatchEPaymentRecipientsBulk.DefaultPaymentReasonInfo.ReadOnly);
		}

		public void TestCreditorPK()
		{
			TestObjectCreator.Creditor1.MiscServ.OM_AB_APDefaultBankAccount = TestObjectCreator.AUDBankAccount.PK;
			TestObjectCreator.Creditor1.CompanyData.OB_APCreditAgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.CreditCard;

			TestMatchEPaymentRecipientsBulk.PaymentReferenceType = EPaymentReferenceTypes.FreeText;
			TestMatchEPaymentRecipientsBulk.PaymentReference = "123456789";
			TestMatchEPaymentRecipientsBulk.CreditorPK = TestObjectCreator.Creditor1.PK;
			AssertNoErrors(TestMatchEPaymentRecipientsBulk.CreditorPKInfo);
			AssertEquals("INV", TestMatchEPaymentRecipientsBulk.PaymentReferenceType);
			AssertEquals(ZString.Empty, TestMatchEPaymentRecipientsBulk.PaymentReference);

			AssertEquals(TestObjectCreator.AUDBankAccount.PK, TestMatchEPaymentRecipientsBulk.DefaultBankAccountPK);
			AssertEquals(OrgConstants.CreditAgreedPaymentMethods.Code.CreditCard, TestMatchEPaymentRecipientsBulk.AgreedPaymentMethod);

			TestMatchEPaymentRecipientsBulk.PaymentReferenceType = EPaymentReferenceTypes.FreeText;
			TestMatchEPaymentRecipientsBulk.PaymentReference = "123456789";
			TestMatchEPaymentRecipientsBulk.CreditorPK = ZGuid.Empty;
			AssertNoErrors(TestMatchEPaymentRecipientsBulk.CreditorPKInfo);
			Assert(TestMatchEPaymentRecipientsBulk.DefaultBankAccountPK.IsEmpty);
			Assert(TestMatchEPaymentRecipientsBulk.AgreedPaymentMethod.IsEmpty);
			Assert(!TestMatchEPaymentRecipientsBulk.AllowOverrideDefault);
			Assert(TestMatchEPaymentRecipientsBulk.DefaultPaymentReason.IsEmpty);
			AssertEquals(ZString.Empty, TestMatchEPaymentRecipientsBulk.PaymentReferenceType);
			AssertEquals(ZString.Empty, TestMatchEPaymentRecipientsBulk.PaymentReference);

			TestMatchEPaymentRecipientsBulk.CreditorPK = ZGuid.Invalid;
			AssertHasError(TestMatchEPaymentRecipientsBulk.CreditorPKInfo, "Enter a valid selection.");
		}

		public void TestCreditorPK_DefaultEPaymentReference()
		{
			var collection = new DefaultEPaymentReferenceCollection();
			var reference = collection.AddNew();
			reference.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			reference.ReferenceType = EPaymentReferenceTypes.FreeText;
			reference.Reference = "Test Reference";
			AccountingMasterFilesRegistry.Instance.DefaultPaymentReference.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			TestMatchEPaymentRecipientsBulk.CreditorPK = ZGuid.Empty;
			AssertEquals(ZString.Empty, TestMatchEPaymentRecipientsBulk.PaymentReferenceType);
			AssertEquals(ZString.Empty, TestMatchEPaymentRecipientsBulk.PaymentReference);

			TestMatchEPaymentRecipientsBulk.CreditorPK = TestObjectCreator.Creditor1.PK;
			AssertEquals(EPaymentReferenceTypes.FreeText, TestMatchEPaymentRecipientsBulk.PaymentReferenceType);
			AssertEquals("Test Reference", TestMatchEPaymentRecipientsBulk.PaymentReference);
		}

		public void TestCreditorPK_ReadOnly()
		{
			TestMatchEPaymentRecipientsBulk.CurrentBeneficiary = null;
			Assert(TestMatchEPaymentRecipientsBulk.CreditorPKInfo.ReadOnly);

			var beneficiary = Factory.NewWithValidTestData<AccEPaymentBeneficiary>();
			TestMatchEPaymentRecipientsBulk.CurrentBeneficiary = beneficiary;
			var accountDetails = Factory.NewWithValidTestData<AccAPAccountDetails>();
			accountDetails.A1_EPaymentBeneficiaryId = ZGuid.Empty;
			Assert(!TestMatchEPaymentRecipientsBulk.CreditorPK_ReadOnly);

			accountDetails.A1_EPaymentBeneficiaryId = beneficiary.PK;
			Assert(TestMatchEPaymentRecipientsBulk.CreditorPK_ReadOnly);
		}

		public void TestDefaultBankAccountPK()
		{
			var bankAccount1 = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount1.AB_GC = GlbCompany.CurrentCompany.PK;
			bankAccount1.AB_GB = GlbBranch.CurrentBranch.PK;
			var bankAccount2 = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount2.AB_GC = GlbCompany.CurrentCompany.PK;
			bankAccount2.AB_GB = TestObjectCreator.NonCurrentBranch.PK;
			var bankAccount3 = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount3.AB_GC = TestObjectCreator.NonCurrentCompany.PK;

			TestMatchEPaymentRecipientsBulk.DefaultBankAccountPK = ZGuid.Empty;
			AssertNoErrors(TestMatchEPaymentRecipientsBulk.DefaultBankAccountPKInfo);
			TestMatchEPaymentRecipientsBulk.DefaultBankAccountPK = bankAccount1.PK;
			AssertNoErrors(TestMatchEPaymentRecipientsBulk.DefaultBankAccountPKInfo);
			TestMatchEPaymentRecipientsBulk.DefaultBankAccountPK = bankAccount2.PK;
			AssertHasError(TestMatchEPaymentRecipientsBulk.DefaultBankAccountPKInfo, "Enter a valid selection.");
			TestMatchEPaymentRecipientsBulk.DefaultBankAccountPK = bankAccount3.PK;
			AssertHasError(TestMatchEPaymentRecipientsBulk.DefaultBankAccountPKInfo, "Enter a valid selection.");
		}

		public void TestDefaultBankAccountPK_ReadOnly()
		{
			TestMatchEPaymentRecipientsBulk.CreditorPK = ZGuid.Empty;
			Assert(TestMatchEPaymentRecipientsBulk.DefaultBankAccountPKInfo.ReadOnly);

			TestMatchEPaymentRecipientsBulk.CreditorPK = TestObjectCreator.Creditor1.PK;
			TestMatchEPaymentRecipientsBulk.AllowOverrideDefault = true;
			Assert(!TestMatchEPaymentRecipientsBulk.DefaultBankAccountPKInfo.ReadOnly);

			TestMatchEPaymentRecipientsBulk.AllowOverrideDefault = false;
			Assert(TestMatchEPaymentRecipientsBulk.DefaultBankAccountPKInfo.ReadOnly);
		}

		public void TestAgreedPaymentMethod()
		{
			TestMatchEPaymentRecipientsBulk.AgreedPaymentMethod = ZString.Empty;
			AssertNoErrors(TestMatchEPaymentRecipientsBulk.AgreedPaymentMethodInfo);

			foreach (var method in Env.Registry.PayablesCreditAgreedPaymentMethodsList.GetAllCodes())
			{
				TestMatchEPaymentRecipientsBulk.AgreedPaymentMethod = method;
				AssertNoErrors(TestMatchEPaymentRecipientsBulk.AgreedPaymentMethodInfo);
			}

			TestMatchEPaymentRecipientsBulk.AgreedPaymentMethod = "XXX";
			AssertHasError(TestMatchEPaymentRecipientsBulk.AgreedPaymentMethodInfo, "Enter a valid selection.");
		}

		public void TestAgreedPaymentMethod_ReadOnly()
		{
			TestMatchEPaymentRecipientsBulk.CreditorPK = ZGuid.Empty;
			Assert(TestMatchEPaymentRecipientsBulk.AgreedPaymentMethodInfo.ReadOnly);

			TestMatchEPaymentRecipientsBulk.CreditorPK = TestObjectCreator.Creditor1.PK;
			TestMatchEPaymentRecipientsBulk.AllowOverrideDefault = true;
			Assert(!TestMatchEPaymentRecipientsBulk.AgreedPaymentMethodInfo.ReadOnly);

			TestMatchEPaymentRecipientsBulk.AllowOverrideDefault = false;
			Assert(TestMatchEPaymentRecipientsBulk.AgreedPaymentMethodInfo.ReadOnly);
		}

		public void TestAllowOverrideDefault()
		{
			TestObjectCreator.Creditor1.MiscServ.OM_AB_APDefaultBankAccount = TestObjectCreator.AUDBankAccount.PK;
			TestObjectCreator.Creditor1.CompanyData.OB_APCreditAgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.CreditCard;
			TestMatchEPaymentRecipientsBulk.CreditorPK = TestObjectCreator.Creditor1.PK;
			AssertEquals(TestObjectCreator.AUDBankAccount.PK, TestMatchEPaymentRecipientsBulk.DefaultBankAccountPK);
			AssertEquals(OrgConstants.CreditAgreedPaymentMethods.Code.CreditCard, TestMatchEPaymentRecipientsBulk.AgreedPaymentMethod);

			TestMatchEPaymentRecipientsBulk.AllowOverrideDefault = true;
			TestMatchEPaymentRecipientsBulk.DefaultBankAccountPK = ZGuid.Empty;
			TestMatchEPaymentRecipientsBulk.AgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck;
			Assert(TestMatchEPaymentRecipientsBulk.DefaultBankAccountPK.IsEmpty);
			AssertEquals(OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck, TestMatchEPaymentRecipientsBulk.AgreedPaymentMethod);

			TestMatchEPaymentRecipientsBulk.AllowOverrideDefault = false;
			AssertEquals(TestObjectCreator.AUDBankAccount.PK, TestMatchEPaymentRecipientsBulk.DefaultBankAccountPK);
			AssertEquals(OrgConstants.CreditAgreedPaymentMethods.Code.CreditCard, TestMatchEPaymentRecipientsBulk.AgreedPaymentMethod);
		}

		public void TestAllowOverrideDefault_ReadOnly()
		{
			TestMatchEPaymentRecipientsBulk.CurrentBeneficiary = null;
			Assert(TestMatchEPaymentRecipientsBulk.AllowOverrideDefaultInfo.ReadOnly);

			var beneficiary = Factory.NewWithValidTestData<AccEPaymentBeneficiary>();
			TestMatchEPaymentRecipientsBulk.CurrentBeneficiary = beneficiary;
			var accountDetails = Factory.NewWithValidTestData<AccAPAccountDetails>();
			accountDetails.A1_EPaymentBeneficiaryId = ZGuid.Empty;
			TestMatchEPaymentRecipientsBulk.CreditorPK = ZGuid.Empty;
			Assert(TestMatchEPaymentRecipientsBulk.AllowOverrideDefault_ReadOnly);
			TestMatchEPaymentRecipientsBulk.CreditorPK = TestObjectCreator.Creditor1.PK;
			Assert(!TestMatchEPaymentRecipientsBulk.AllowOverrideDefault_ReadOnly);

			accountDetails.A1_EPaymentBeneficiaryId = beneficiary.PK;
			Assert(TestMatchEPaymentRecipientsBulk.AllowOverrideDefault_ReadOnly);
		}

		public void TestPaymentReferenceType()
		{
			TestMatchEPaymentRecipientsBulk.CreditorPK = ZGuid.Empty;
			TestMatchEPaymentRecipientsBulk.PaymentReferenceType = "";
			AssertEquals(false, TestMatchEPaymentRecipientsBulk.PaymentReferenceTypeInfo.HasErrors());
			TestMatchEPaymentRecipientsBulk.PaymentReferenceType = "A";
			AssertEquals("A", TestMatchEPaymentRecipientsBulk.PaymentReferenceType);
			AssertHasError(TestMatchEPaymentRecipientsBulk.PaymentReferenceTypeInfo, "Enter a valid selection.");

			TestMatchEPaymentRecipientsBulk.CreditorPK = TestObjectCreator.Creditor1.PK;
			TestMatchEPaymentRecipientsBulk.PaymentReferenceType = "";
			AssertHasError(TestMatchEPaymentRecipientsBulk.PaymentReferenceTypeInfo, "Payment Reference Type is Mandatory for E-Payments. Please select a Payment Reference Type.");
			TestMatchEPaymentRecipientsBulk.PaymentReferenceType = "A";
			AssertEquals(true, TestMatchEPaymentRecipientsBulk.PaymentReferenceTypeInfo.HasErrors());

			foreach (CodeDescriptionPair referenceType in TestMatchEPaymentRecipientsBulk.PaymentReferenceTypeList)
			{
				TestMatchEPaymentRecipientsBulk.PaymentReferenceType = referenceType.Code;
				AssertEquals(referenceType.Code, TestMatchEPaymentRecipientsBulk.PaymentReferenceType);
				AssertEquals(false, TestMatchEPaymentRecipientsBulk.PaymentReferenceTypeInfo.HasErrors());
			}
		}

		public void TestPaymentReferenceType_ReadOnly()
		{
			var beneficiary = Factory.NewWithValidTestData<AccEPaymentBeneficiary>();
			TestMatchEPaymentRecipientsBulk.CurrentBeneficiary = beneficiary;

			var accountDetails = Factory.NewWithValidTestData<AccAPAccountDetails>();
			accountDetails.A1_EPaymentBeneficiaryId = ZGuid.Empty;

			TestMatchEPaymentRecipientsBulk.CreditorPK = ZGuid.Empty;
			AssertEquals(true, TestMatchEPaymentRecipientsBulk.PaymentReferenceTypeInfo.ReadOnly);

			TestMatchEPaymentRecipientsBulk.CreditorPK = TestObjectCreator.Creditor1.PK;
			AssertEquals(false, TestMatchEPaymentRecipientsBulk.PaymentReferenceTypeInfo.ReadOnly);

			accountDetails.A1_EPaymentBeneficiaryId = beneficiary.PK;
			AssertEquals(true, TestMatchEPaymentRecipientsBulk.PaymentReferenceTypeInfo.ReadOnly);
		}

		public void TestOnPaymentReferenceTypeChanged()
		{
			TestMatchEPaymentRecipientsBulk.PaymentReferenceType = "";

			var isMehtodCalled = false;
			TestMatchEPaymentRecipientsBulk.OnPaymentReferenceTypeChanged += (s, e) => {
				isMehtodCalled = true;
			};

			TestMatchEPaymentRecipientsBulk.PaymentReferenceType = "";
			AssertEquals(false, isMehtodCalled);

			TestMatchEPaymentRecipientsBulk.PaymentReferenceType = "A";
			AssertEquals(true, isMehtodCalled);
		}

		public void TestPaymentReferenceTypeList()
		{
			var list = (TestMatchEPaymentRecipientsBulk.PaymentReferenceTypeList as CodeDescriptionPairList).Cast<CodeDescriptionPair>();

			AssertEquals(3, list.Count());
			AssertContainsExactElementsInAnyOrder(new string[] {
				"TXT - Free Text", "INV - Invoice Numbers", "PRN - Payment Reference Number" }, list.Select(x => x.CodeAndDescription));
		}

		public void TestPaymentReference()
		{
			foreach (CodeDescriptionPair referenceType in TestMatchEPaymentRecipientsBulk.PaymentReferenceTypeList)
			{
				TestMatchEPaymentRecipientsBulk.PaymentReferenceType = referenceType.Code;
				TestMatchEPaymentRecipientsBulk.PaymentReference = ZString.Empty;
				AssertEquals(ZString.Empty, TestMatchEPaymentRecipientsBulk.PaymentReference);
				TestMatchEPaymentRecipientsBulk.ValidateAll();
				if (referenceType.Code == EPaymentReferenceTypes.FreeText)
				{
					AssertHasError(TestMatchEPaymentRecipientsBulk.PaymentReferenceInfo, "Please enter a value.");
				}
				else
				{
					AssertEquals(false, TestMatchEPaymentRecipientsBulk.PaymentReferenceInfo.HasErrors());
				}

				TestMatchEPaymentRecipientsBulk.PaymentReference = "A";
				AssertEquals("A", TestMatchEPaymentRecipientsBulk.PaymentReference);
				AssertEquals(false, TestMatchEPaymentRecipientsBulk.PaymentReferenceInfo.HasErrors());
			}

			AssertNoExceptionThrown(() => TestMatchEPaymentRecipientsBulk.PaymentReference = "012345678901234567890123456789");

			var ex = AssertExceptionThrown<Exception>(() => TestMatchEPaymentRecipientsBulk.PaymentReference = "012345678901234567890123456789X");
			AssertContains("MaxLengthExceededException", ex.GetType().Name);
			AssertContains("The maximum length of this property is 30 characters, but 31 were entered.", ex.Message);
			ErrorReporter.Clear();
		}

		public void TestPaymentReference_ReadOnly()
		{
			var beneficiary = Factory.NewWithValidTestData<AccEPaymentBeneficiary>();
			TestMatchEPaymentRecipientsBulk.CurrentBeneficiary = beneficiary;

			var accountDetails = Factory.NewWithValidTestData<AccAPAccountDetails>();
			accountDetails.A1_EPaymentBeneficiaryId = ZGuid.Empty;

			TestMatchEPaymentRecipientsBulk.CreditorPK = ZGuid.Empty;
			AssertEquals(true, TestMatchEPaymentRecipientsBulk.PaymentReferenceInfo.ReadOnly);

			TestMatchEPaymentRecipientsBulk.CreditorPK = TestObjectCreator.Creditor1.PK;
			AssertEquals(true, TestMatchEPaymentRecipientsBulk.PaymentReferenceInfo.ReadOnly);

			TestMatchEPaymentRecipientsBulk.AllowOverrideDefault = false;
			foreach (CodeDescriptionPair referenceType in TestMatchEPaymentRecipientsBulk.PaymentReferenceTypeList)
			{
				TestMatchEPaymentRecipientsBulk.PaymentReferenceType = referenceType.Code;

				if (referenceType.Code == EPaymentReferenceTypes.FreeText)
				{
					AssertEquals(false, TestMatchEPaymentRecipientsBulk.PaymentReferenceInfo.ReadOnly);

					accountDetails.A1_EPaymentBeneficiaryId = beneficiary.PK;
					AssertEquals(true, TestMatchEPaymentRecipientsBulk.PaymentReferenceInfo.ReadOnly);
				}
				else
				{
					AssertEquals(true, TestMatchEPaymentRecipientsBulk.PaymentReferenceInfo.ReadOnly);
				}
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			testMatchEPaymentRecipientsBulk_internalValue = (MatchEPaymentRecipientsBulk)GetNewBusinessObject();
		}

		MatchEPaymentRecipientsBulk TestMatchEPaymentRecipientsBulk
		{
			get { return testMatchEPaymentRecipientsBulk_internalValue; }
		}
		protected MatchEPaymentRecipientsBulk testMatchEPaymentRecipientsBulk_internalValue;

		#endregion
	}
}
