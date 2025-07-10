using System;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing.eInvoicing
{
	class ElectronicInvoicingUpdateActionPermissionsTest : TestCaseWithFactory
	{
		#region TestCheckIsComplianceNumberResetAllowed

		public void TestCheckIsComplianceNumberResetAllowed_EnableEInvoicingFunctionalityRegistry()
		{
			var companyPK = ZGuid.NewZGuid();
			var countryFactoryMock = new Mock<IAccountingCountryFactory>();
			var inputDataMock = SetupForCheckIsComplianceNumberResetAllowedSetupTests(companyPK, CountryCodes.Australia, countryFactoryMock.Object);

			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(companyPK.ToGuid(), Guid.Empty, Guid.Empty, false);
			Assert("EnableEInvoicingFunctionality registry is false", GetElectronicInvoicingUpdateActionPermissions().CheckIsComplianceNumberResetAllowed(inputDataMock.Object));

			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(companyPK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Assert("EnableEInvoicingFunctionality registry is true", !GetElectronicInvoicingUpdateActionPermissions().CheckIsComplianceNumberResetAllowed(inputDataMock.Object));
		}

		public void TestCheckIsComplianceNumberResetAllowed_EInvoicingStatus()
		{
			var countryFactoryMock = new Mock<IAccountingCountryFactory>();
			var inputDataMock = SetupForCheckIsComplianceNumberResetAllowedSetupTests(companyPK: ZGuid.NewZGuid(), CountryCodes.Australia, countryFactoryMock.Object);

			inputDataMock.Setup(x => x.EInvoicingStatus).Returns("");
			Assert("EInvoicingStatus is empty", GetElectronicInvoicingUpdateActionPermissions().CheckIsComplianceNumberResetAllowed(inputDataMock.Object));

			inputDataMock.Setup(x => x.EInvoicingStatus).Returns("ANY");
			Assert("EInvoicingStatus is not empty", !GetElectronicInvoicingUpdateActionPermissions().CheckIsComplianceNumberResetAllowed(inputDataMock.Object));
		}

		public void TestCheckIsComplianceNumberResetAllowed_CheckIsComplianceNumberResetAllowedForSubmittedEInvoice()
		{
			var countryFactoryMock = new Mock<IAccountingCountryFactory>().As<IInstanceProvider<IComplianceNumberResetStatus>>();
			var resetStatusMock = new Mock<IComplianceNumberResetStatus>();
			countryFactoryMock.Setup(x => x.Get()).Returns(resetStatusMock.Object);
			var inputDataMock = SetupForCheckIsComplianceNumberResetAllowedSetupTests(companyPK: ZGuid.NewZGuid(), countryCode: "XXX", (IAccountingCountryFactory)countryFactoryMock.Object);

			resetStatusMock.Setup(x => x.CheckIsComplianceNumberResetAllowedForSubmittedEInvoice(inputDataMock.Object)).Returns(true);
			Assert("ComplianceNumber reset is allowed", GetElectronicInvoicingUpdateActionPermissions().CheckIsComplianceNumberResetAllowed(inputDataMock.Object));

			resetStatusMock.Setup(x => x.CheckIsComplianceNumberResetAllowedForSubmittedEInvoice(inputDataMock.Object)).Returns(false);
			Assert("ComplianceNumber reset is not allowed", !GetElectronicInvoicingUpdateActionPermissions().CheckIsComplianceNumberResetAllowed(inputDataMock.Object));
		}

		static Mock<IComplianceNumberResetStatusInputData> SetupForCheckIsComplianceNumberResetAllowedSetupTests(ZGuid companyPK, string countryCode, IAccountingCountryFactory countryFactory)
		{
			var globalCountryFactoryMock = new Mock<IGlobalAccountingCountryFactory>();
			globalCountryFactoryMock.Setup(x => x.GetCountryFactory(countryCode)).Returns(countryFactory);
			ObjectFactory.Substitute(globalCountryFactoryMock.Object);

			var inputDataMock = new Mock<IComplianceNumberResetStatusInputData>();
			inputDataMock.Setup(x => x.CountryCode).Returns(countryCode);
			inputDataMock.Setup(x => x.EInvoicingStatus).Returns("ABC");
			inputDataMock.Setup(x => x.CompanyPK).Returns(companyPK);

			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(companyPK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Assert("Precondition: initial CheckIsComplianceNumberResetAllowed value", !GetElectronicInvoicingUpdateActionPermissions().CheckIsComplianceNumberResetAllowed(inputDataMock.Object));

			return inputDataMock;
		}

		#endregion

		#region CheckComplianceSubTypeAndNumberManualUpdateToAnyValue Tests

		public void TestElectronicInvoicingUpdateActionPermissions_Test_ForCurrentCompany_With_EnableEInvoicingFunctionality_IsTrue()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var countryComplianceFactoryMock = GetICountryComplianceFactory(true);

				using (ObjectFactory.Substitute(countryComplianceFactoryMock.Object))
				{
					ZString message = AssertCorrectLedgersValid(GlbCompany.CurrentCompany);
					AssertEquals(message, AccountingConstants.DisallowToUpdateComplianceSubTypeAndNumber);
				}
			}
		}

		public void TestElectronicInvoicingUpdateActionPermissions_ForNonCurrentCompany_With_EnableEInvoicingFunctionality_IsTrue()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			GlbCompany nonCurrentCompany = creator.NonCurrentCompany;

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(nonCurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var countryComplianceFactoryMock = GetICountryComplianceFactory(true);

				using (ObjectFactory.Substitute(countryComplianceFactoryMock.Object))
				{
					ZString message = AssertCorrectLedgersValid(nonCurrentCompany);
					AssertEquals(message, AccountingConstants.DisallowToUpdateComplianceSubTypeAndNumber);
				}
			}
		}

		public void TestElectronicInvoicingUpdateActionPermissions_ForCurrentCompany_With_EnableEInvoicingFunctionality_IsFalse()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var countryComplianceFactoryMock = GetICountryComplianceFactory(true);

				using (ObjectFactory.Substitute(countryComplianceFactoryMock.Object))
				{
					ZString message = AssertCorrectLedgersValid(GlbCompany.CurrentCompany);
					AssertEquals(message, ZString.Empty);
				}
			}
		}

		public void TestElectronicInvoicingUpdateActionPermissions_ForCurrentCompany_With_EnableEInvoicingFunctionality_IsTrue_And_IsARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowed_IsFalse()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var countryComplianceFactoryMock = GetICountryComplianceFactory(false);

				using (ObjectFactory.Substitute(countryComplianceFactoryMock.Object))
				{
					ZString message = AssertCorrectLedgersValid(GlbCompany.CurrentCompany);
					AssertEquals(message, ZString.Empty);
				}
			}
		}

		ZString AssertCorrectLedgersValid(GlbCompany company)
		{
			ZString message;

			var otherLedgers = from field in typeof(LedgerTypes).GetFields(BindingFlags.Static | BindingFlags.Public)
							   let ledger = (string)field.GetValue(null)
							   where ledger != LedgerTypes.AccountsReceivable
							   select ledger;

			foreach (var ledger in otherLedgers)
			{
				message = ElectronicInvoicingUpdateActionPermissions.CheckComplianceSubTypeAndNumberManualUpdateToAnyValue(ledger, company);
				AssertEquals(string.Format("For {0} when {1}", ledger, ZString.Empty), false, LedgerValid(ledger));
			}

			return ElectronicInvoicingUpdateActionPermissions.CheckComplianceSubTypeAndNumberManualUpdateToAnyValue(LedgerTypes.AccountsReceivable, company);
		}

		bool LedgerValid(string ledger)
		{
			Argument.NotNullOrEmpty(ledger, "Ledger");

			if (ledger == LedgerTypes.AccountsReceivable)
			{
				return true;
			}

			return false;
		}

		#endregion

		#region CheckComplianceSubTypeAndNumberEligibilityWarning Tests

		public void TestCheckComplianceSubTypeAndNumberEligibilityWarning_HasNoMessage_WhenEInvoicingIsDisabled()
		{
			var eInvoicingMock = GetIGlobalEInvoicingObjectFactory(countrySupportsLiteEligibility: false);
			using (ObjectFactory.Substitute(eInvoicingMock.Object))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var message = ElectronicInvoicingUpdateActionPermissions.CheckComplianceSubTypeAndNumberEligibilityWarning(GlbCompany.CurrentCompany);
				AssertEquals("No warning appears when eInvoicing is disabled", message, ZString.Empty);
			}
		}

		public void TestCheckComplianceSubTypeAndNumberEligibilityWarning_HasNoMessage_ForLiteEligibilityCountry()
		{
			var eInvoicingMock = GetIGlobalEInvoicingObjectFactory(countrySupportsLiteEligibility: true);
			using (ObjectFactory.Substitute(eInvoicingMock.Object))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var message = ElectronicInvoicingUpdateActionPermissions.CheckComplianceSubTypeAndNumberEligibilityWarning(GlbCompany.CurrentCompany);
				AssertEquals("No warning appears when eInvoicing is enabled and country supports lite eligibility", message, ZString.Empty);
			}
		}

		public void TestCheckComplianceSubTypeAndNumberEligibilityWarning_HasMessage_ForComplexEligibilityCountry()
		{
			var expectedMessage = "Note that changing the Compliance Number or Compliance Sub-Type will not re-evaluate the transaction for E-Invoicing.";
			var eInvoicingMock = GetIGlobalEInvoicingObjectFactory(countrySupportsLiteEligibility: false);
			using (ObjectFactory.Substitute(eInvoicingMock.Object))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var message = ElectronicInvoicingUpdateActionPermissions.CheckComplianceSubTypeAndNumberEligibilityWarning(GlbCompany.CurrentCompany);
				AssertEquals("A warning should appear when eInvoicing is enabled and country does not support lite eligibility, as eInvoicing pivots are not updated from the BusinessInvoice bizo", message, expectedMessage);
			}
		}

		#endregion

		public void TestGetErrorMessageForARComplianceSubTypeAndNumberUpdate()
		{
			AssertEquals(ZString.Empty, ElectronicInvoicingUpdateActionPermissions.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(Array.Empty<AccTransactionHeader>()));

			var countryComplianceFactoryMock = new Mock<ICountryComplianceFactory>();
			countryComplianceFactoryMock.Setup(c => c.GetIComplianceSubTypeAndNumberUpdateRules(It.IsAny<ZString>())).Returns((IComplianceSubTypeAndNumberUpdateRules)null);

			using (ObjectFactory.Substitute(countryComplianceFactoryMock.Object))
			{
				AssertEquals(ZString.Empty, ElectronicInvoicingUpdateActionPermissions.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(new AccTransactionHeader[] { Factory.NewWithValidTestData<AccTransactionHeader>() }));
			}
		}

		#region Helpers

		Mock<ICountryComplianceFactory> GetICountryComplianceFactory(bool isARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowedValue)
		{
			var countryComplianceFactoryMock = new Mock<ICountryComplianceFactory>();

			var complianceSubTypeAndNumberUpdateRulesMock = new Mock<IComplianceSubTypeAndNumberUpdateRules>();
			complianceSubTypeAndNumberUpdateRulesMock.Setup(x => x.IsARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowed).Returns(isARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowedValue);

			countryComplianceFactoryMock.Setup(c => c.GetIComplianceSubTypeAndNumberUpdateRules(It.IsAny<ZString>())).Returns(complianceSubTypeAndNumberUpdateRulesMock.Object);

			return countryComplianceFactoryMock;
		}

		static Mock<IGlobalAccountingCountryFactory> GetIGlobalEInvoicingObjectFactory(bool countrySupportsLiteEligibility)
		{
			var globalFactoryMock = new Mock<IGlobalAccountingCountryFactory>();

			if (countrySupportsLiteEligibility)
			{
				var countryFactoryMock = new Mock<IAccountingCountryFactory>();
				countryFactoryMock.As<IInstanceProvider<IEInvoicingEligibilityDecider>>();
				globalFactoryMock.Setup(c => c.GetCountryFactory(It.IsAny<ZString>())).Returns(countryFactoryMock.Object);
			}

			return globalFactoryMock;
		}

		static IElectronicInvoicingUpdateActionPermissions GetElectronicInvoicingUpdateActionPermissions() => ObjectFactory.Get<IElectronicInvoicingAccountingObjectFactory>().GetElectronicInvoicingUpdateActionPermissions();

		#endregion
	}
}
