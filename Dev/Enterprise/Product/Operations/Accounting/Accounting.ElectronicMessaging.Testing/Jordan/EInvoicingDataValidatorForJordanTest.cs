using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation.Testing;
using Enterprise.Accounting.ElectronicMessaging.Jordan;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Testing.Jordan
{
	public class EInvoicingDataValidatorForJordanTest : BaseEInvoicingDataValidatorTest
	{
		public void TestCheckClientIdAndSecret()
		{
			var expectedError = "Jordan E-Invoicing Client ID and Secret Key must be set against the registry 'E-Invoicing Credentials'.";
			var arInvoice = Prepare();
			AssertValidationResult(arInvoice, expectedError, hasError: true);

			var eInvoicingCredentials = AccountingElectronicMessagingRegistry.Instance.JordanEInvoicingCredentials.Value;
			eInvoicingCredentials.ClientId = "ClientId";
			eInvoicingCredentials.ClientSecret = "ClientSecret";
			AccountingElectronicMessagingRegistry.Instance.JordanEInvoicingCredentials.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, eInvoicingCredentials);

			AssertValidationResult(arInvoice, expectedError, hasError: false);
		}

		public void TestCheckRegistrationNumberForProxy_WhenHasBranchProxy()
		{
			var proxyLevel = "Branch";
			var arInvoice = Prepare();

			TestCheckRegistrationNumberForProxy(arInvoice, arInvoice.Branch.OrgProxy, proxyLevel);
		}

		public void TestCheckRegistrationNumberForProxy_WhenHasCompanyProxy()
		{
			var proxyLevel = "Company";
			var arInvoice = Prepare();
			arInvoice.Branch.GB_OH_OrgProxy = ZGuid.Empty;

			TestCheckRegistrationNumberForProxy(arInvoice, arInvoice.Company.OrgProxy, proxyLevel);
		}

		void TestCheckRegistrationNumberForProxy(InvoicingBase invoice, OrgHeader orgHeader, string proxyLevel)
		{
			AssertRegistrationNumber(invoice, orgHeader, OrgCusCode.CodeTypes.GSTCode, "111", Constants.CountryCodes.Jordan, expectedError: $"Please record JO GST - Government GST Code against the Login {proxyLevel} Organization Proxy.");
			AssertRegistrationNumber(invoice, orgHeader, JordanOrgCusCodeInfo.OrgCusCodes.BusinessActivityNumber, "123", Constants.CountryCodes.Jordan, expectedError: $"Please record JO BAN - Business Activity Number against the Login {proxyLevel} Organization Proxy.");
		}

		void AssertRegistrationNumber(InvoicingBase arInvoice, OrgHeader orgHeader, ZString codeType, ZString number, ZString countryCode, string expectedError)
		{
			orgHeader.CustomsCodes.RemoveAll();
			AssertValidationResult(arInvoice, expectedError);

			orgHeader.CustomsCodes.AddNew(codeType, number, countryCode);
			AssertValidationResult(arInvoice, expectedError, hasError: false);
		}

		void AssertValidationResult(InvoicingBase transaction, string expectedError, bool hasError = true)
		{
			var errorMessages = EInvoicingDataValidator.ValidateTransaction(transaction, TestPivot);
			var containError = errorMessages.Contains(expectedError);
			Assert(hasError ? containError : !containError);
		}

		InvoicingBase Prepare()
		{
			TestObjectCreator.SetCurrentCompanyCountryCode(Constants.CountryCodes.Jordan);
			var testBranch = TestObjectCreator.CreateBranch("TBN", GlbCompany.CurrentCompany, TestObjectCreator.Debtor1);

			var debtor = TestObjectCreator.Debtor;
			debtor.OH_RL_NKClosestPort = "AUSYD";
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = TestObjectCreator.Debtor1.PK;

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1m, debtor);
			arInvoice.AH_GB = testBranch.PK;
			TestObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);

			Factory.Save();

			return arInvoice;
		}

		AccEInvoicingTransactionPivot TestPivot
		{
			get
			{
				if (testPivot == null)
				{
					testPivot = Factory.New<AccEInvoicingTransactionPivot>();
					testPivot.AIP_ActionType = Constants.EInvoicingPivotActionType.Submit;
				}

				return testPivot;
			}
		}

		AccEInvoicingTransactionPivot testPivot;

		public override BaseEInvoicingDataValidator GetEInvoicingDataValidatorForTest() =>
				new EInvoicingDataValidatorForJordan(GlbCompany.CurrentCompany);

		BaseEInvoicingDataValidator EInvoicingDataValidator => eInvoicingDataValidator ?? (eInvoicingDataValidator = GetEInvoicingDataValidatorForTest());
		BaseEInvoicingDataValidator eInvoicingDataValidator;
	}
}
