using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(InvoicePrintingController))]
	public class InvoicePrintingControllerTest : ZControllerBasherTest
	{
		public void TestGetForm_ReturnsForm_WhenEInvoicingDisabled()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				foreach (var transactionType in SupportedTransactionTypes)
				{
					using (var form = RunTestAndReturnForm(transactionType))
					{
						AssertType(typeof(ClassAInvoiceForm), form);
						AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public void TestGetForm_ReturnsNullFormAndError_WhenExplicitlyDisabled()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var countryComplianceFactoryMockDisallowed = GetICountryComplianceFactory(isARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowed: true);
				using (ObjectFactory.Substitute(countryComplianceFactoryMockDisallowed.Object))
				{
					foreach (var transactionType in SupportedTransactionTypes)
					{
						using (var form = RunTestAndReturnForm(transactionType))
						{
							AssertEquals(null, form);
							AssertEquals(AccountingConstants.DisallowToUpdateComplianceSubTypeAndNumber, UnitTestUserNotification.Instance.LastMessage.Text);
						}
					}
				}
			}
		}

		public void TestGetForm_ReturnsForm_WhenNotDisabled_AndEInvoicingEnabled_AndLiteEligibility()
		{
			var eInvoicingMockLite = GetIGlobalEInvoicingObjectFactory(countrySupportsLiteEligibility: true);
			using (ObjectFactory.Substitute(eInvoicingMockLite.Object))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var countryComplianceFactoryMockAllowed = GetICountryComplianceFactory(isARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowed: false);
				using (ObjectFactory.Substitute(countryComplianceFactoryMockAllowed.Object))
				{
					foreach (var transactionType in SupportedTransactionTypes)
					{
						using (var form = RunTestAndReturnForm(transactionType))
						{
							AssertType(typeof(ClassAInvoiceForm), form);
							AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
						}
					}
				}

				var countryComplianceFactoryMockNull = GetICountryComplianceFactory(isARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowed: null);
				using (ObjectFactory.Substitute(countryComplianceFactoryMockNull.Object))
				{
					foreach (var transactionType in SupportedTransactionTypes)
					{
						using (var form = RunTestAndReturnForm(transactionType))
						{
							AssertType(typeof(ClassAInvoiceForm), form);
							AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
						}
					}
				}
			}
		}

		public void TestGetForm_ReturnsFormAndWarning_WhenNotDisabled_AndEInvoicingEnabled_AndComplexEligibility()
		{
			var countryComplianceFactoryMockAllowed = GetICountryComplianceFactory(isARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowed: false);
			using (ObjectFactory.Substitute(countryComplianceFactoryMockAllowed.Object))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var eInvoicingMockComplex = GetIGlobalEInvoicingObjectFactory(countrySupportsLiteEligibility: false);
				using (ObjectFactory.Substitute(eInvoicingMockComplex.Object))
				{
					foreach (var transactionType in SupportedTransactionTypes)
					{
						using (var form = RunTestAndReturnForm(transactionType))
						{
							AssertType(typeof(ClassAInvoiceForm), form);
							AssertEquals(AccountingConstants.ComplianceSubTypeAndNumberEligibilityWarning, UnitTestUserNotification.Instance.LastMessage.Text);
						}
					}
				}
			}
		}

		#region Implementations

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var bizO = Factory.NewWithValidTestData<GovernmentInvoice>();
			Factory.Save();
			return bizO;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.InvoicePrinting;
		}

		public override void TestNewForm()
		{
			AssertNull("NewForm should be null", Controller.ShowNewForm());
		}

		public override void TestViewForm()
		{
			AssertNull("ViewForm should be null", Controller.ShowViewForm(GetBusinessObjectThatIsInTheDatabase()));
		}

		public override void TestDeleteForm()
		{
			AssertNull("DeleteForm should be null", Controller.ShowDeleteForm(GetBusinessObjectThatIsInTheDatabase()));
		}

		#endregion

		#region Helpers

		readonly string[] SupportedTransactionTypes = new[]
		{
			TransactionTypes.Invoice,
			TransactionTypes.CreditNote,
			TransactionTypes.AdjustmentNote
		};

		IZForm RunTestAndReturnForm(ZString transactionType)
		{
			var invoice = Factory.NewWithValidTestData<GovernmentInvoice>();
			invoice.AH_TransactionType = transactionType;
			var controller = new InvoicePrintingController();
			UnitTestUserNotification.Instance.ClearMessages();
			return controller.GetForm_ForTestOnly(invoice);
		}

		static Mock<ICountryComplianceFactory> GetICountryComplianceFactory(bool? isARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowed)
		{
			var countryComplianceFactoryMock = new Mock<ICountryComplianceFactory>();

			if (isARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowed.HasValue)
			{
				var complianceSubTypeAndNumberUpdateRulesMock = new Mock<IComplianceSubTypeAndNumberUpdateRules>();
				complianceSubTypeAndNumberUpdateRulesMock.Setup(x => x.IsARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowed).Returns(isARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowed.Value);

				countryComplianceFactoryMock.Setup(c => c.GetIComplianceSubTypeAndNumberUpdateRules(It.IsAny<ZString>())).Returns(complianceSubTypeAndNumberUpdateRulesMock.Object);
			}

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

		#endregion
	}
}
