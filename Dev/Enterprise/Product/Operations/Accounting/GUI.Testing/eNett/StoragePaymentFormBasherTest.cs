using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.eNett_Integration;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.eNett.Testing
{
	[TestedType(typeof(ContainerStoragePaymentForm))]
	internal sealed class StoragePaymentFormBasherTest : ZFormBasherTest
	{
		public void TestIButtonPostTextOverrideImplementation()
		{
			using (ContainerStoragePaymentForm form = (ContainerStoragePaymentForm)GetFormToBash())
			{
				IButtonPostTextOverride postTextOverride = form;
				AssertNotNull("Should Implement IButtonPostTextOverride", postTextOverride);
				AssertEquals("PostButtonText should be as expected", "Post && Close", postTextOverride.PostButtonText);
			}
		}

		public void TestConcurrencyWhenProcessingDirectDebit()
		{
			using (AccountingConfigurationRegistry.Instance.ENettRegistration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new EnettRegistrationCode { RegistrationCode = "1", OrganisationPK = TestObjectCreator.AALSHI.PK }))
			{
				using (SemaphoreDbManager.ForceToUseTheSameDbConnection_ForTestOnly())
				{
					TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
					TestObjectCreator.AALSHI.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "12345");
					TestObjectCreator.AddAPBankAccountDetails(TestObjectCreator.AALSHI, ReceiptTypes.eNettDirectDebit, TestObjectCreator.AUD);
				
					var container = CreateContainerForTest();

					var bankAccountCollection = new ENettRegisteredBankAccountCollection();
					var account = bankAccountCollection.AddNew();
					account.BankAccountPK = TestObjectCreator.AUDBankAccount.PK;
					Factory.Save();

					AccountingConfigurationRegistry.Instance.ENettStoragePaymentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CC3.PK.ToGuid());
					AccountingConfigurationRegistry.Instance.ENettRegisteredBankAccount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, bankAccountCollection);

					var payment = CreatePaymentForTest(container);
					
					payment.RunPreSaveValidation();
					AssertNoErrors(payment);
					using (var form = new ContainerStoragePaymentForm(payment))
					{
						form.Show();
						form.Saved += (sender, e) =>
						{
							ObjectFactory.Get<IENettOutboundTransactionProcessRunnerForTest>().ProcessLogs();
						};

						form.FireSaveButton();

						var query = new ZQuery(EDIMessageSchema.EM_MessageSubType, eNettMessageSubTypeList.Codes.ProcessDirectDebit);
						query.AddToFilter(EDIMessageSchema.EM_LinkTable, payment.Invoice.ReceiptPayment.TableName);
						query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, payment.Invoice.ReceiptPayment.PK);
						EDIMessage[] createdMessages = Factory.Load<EDIMessage>(query);
						AssertEquals("Should have created 1 message", 1, createdMessages.Length);
					}
				}
			}
		}

		public void TestOpenSameStoragePaymentFormMultipleTimesAndSave()
		{
			using (AccountingConfigurationRegistry.Instance.ENettRegistration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new EnettRegistrationCode { RegistrationCode = "1", OrganisationPK = TestObjectCreator.AALSHI.PK }))
			{
				using (SemaphoreDbManager.ForceToUseTheSameDbConnection_ForTestOnly())
				{
					var container1 = CreateContainerForTest();
					var payment1 = CreatePaymentForTest(container1);
					var apInvoiceLineWithMutex = (APInvoiceLine)payment1.Invoice.Lines.FirstOrDefault();
					AssertNotNull(apInvoiceLineWithMutex);
					AssertNull(apInvoiceLineWithMutex.CreatingJobHeaderErrorMessage);

					var payment2 = CreatePaymentForTest(container1);
					var apInvoiceLineForTest = (APInvoiceLine)payment2.Invoice.Lines.FirstOrDefault();
					AssertNotNull(apInvoiceLineForTest);
					using (var form = new ContainerStoragePaymentForm(payment2))
					{
						form.FireSaveButton();
						AssertEquals(false,form.LastSaveSucceeded);
						var rowErrorMessages = apInvoiceLineForTest.RowErrors.Select(x => x.Message);
						var expectErrorMsg = "You have created the job 1 on another form, but haven't saved it yet.\r\n" +
												"Please close or save other forms that use job 1 to continue.";
						AssertCollectionContains(expectErrorMsg,rowErrorMessages);
						payment1.Dispose();
					}
				}
			}
		}

		public void TestValidateAllWhenNotGetMutexForJobHeader()
		{
			using (AccountingConfigurationRegistry.Instance.ENettRegistration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new EnettRegistrationCode { RegistrationCode = "1", OrganisationPK = TestObjectCreator.AALSHI.PK }))
			{
				using (SemaphoreDbManager.ForceToUseTheSameDbConnection_ForTestOnly())
				{
					var container1 = CreateContainerForTest();
					var payment1 = CreatePaymentForTest(container1);
					var apInvoiceLineWithMutex = (APInvoiceLine)payment1.Invoice.Lines.FirstOrDefault();
					AssertNotNull(apInvoiceLineWithMutex);
					AssertNull(apInvoiceLineWithMutex.CreatingJobHeaderErrorMessage);

					var payment2 = CreatePaymentForTest(container1);
					var apInvoiceLineForTest = (APInvoiceLine)payment2.Invoice.Lines.FirstOrDefault();
					AssertNotNull(apInvoiceLineForTest);

					payment2.RunPreSaveValidation();
					var rowErrorMessages = apInvoiceLineForTest.RowErrors.Select(x => x.Message);
					var expectErrorMsg = "You have created the job 1 on another form, but haven't saved it yet.\r\n" +
											"Please close or save other forms that use job 1 to continue.";
					AssertCollectionContains(expectErrorMsg,rowErrorMessages);
					payment1.Dispose();
					payment2.Dispose();
				}
			}
		}

		public void TestContainerStorageChargeInvoiceDateBePresentBeforePosting()
		{
			using (AccountingConfigurationRegistry.Instance.ENettRegistration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new EnettRegistrationCode { RegistrationCode = "1", OrganisationPK = TestObjectCreator.AALSHI.PK }))
			using (SemaphoreDbManager.ForceToUseTheSameDbConnection_ForTestOnly())
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
				TestObjectCreator.AALSHI.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "12345");
				TestObjectCreator.AddAPBankAccountDetails(TestObjectCreator.AALSHI, ReceiptTypes.eNettDirectDebit, TestObjectCreator.AUD);

				var container = CreateContainerForTest();

				var bankAccountCollection = new ENettRegisteredBankAccountCollection();
				var account = bankAccountCollection.AddNew();
				account.BankAccountPK = TestObjectCreator.AUDBankAccount.PK;
				Factory.Save();

				AccountingConfigurationRegistry.Instance.ENettStoragePaymentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CC3.PK.ToGuid());
				AccountingConfigurationRegistry.Instance.ENettRegisteredBankAccount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, bankAccountCollection);

				var payment = CreatePaymentForTest(container);

				payment.RunPreSaveValidation();
				AssertNoErrors(payment);
				using (var form = new ContainerStoragePaymentForm(payment))
				{
					form.Show();
					form.Saved += (sender, e) =>
					{
						ObjectFactory.Get<IENettOutboundTransactionProcessRunnerForTest>().ProcessLogs();
					};
					payment.Invoice.AH_InvoiceDate = ZDateTime.Empty;
					form.FireSaveButton();
					Assert(payment.Invoice.InvoiceDate.IsValid);

					var query = new ZQuery(EDIMessageSchema.EM_MessageSubType, eNettMessageSubTypeList.Codes.ProcessDirectDebit);
					query.AddToFilter(EDIMessageSchema.EM_LinkTable, payment.Invoice.ReceiptPayment.TableName);
					query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, payment.Invoice.ReceiptPayment.PK);
					EDIMessage[] createdMessages = Factory.Load<EDIMessage>(query);
					AssertEquals("Should have created 1 message", 1, createdMessages.Length);
				}
			}
		}

		public void TestContainerStorageChargeReceiptPaymentInvoiceDateBePresentBeforePosting()
		{
			using (AccountingConfigurationRegistry.Instance.ENettRegistration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new EnettRegistrationCode { RegistrationCode = "1", OrganisationPK = TestObjectCreator.AALSHI.PK }))
			using (SemaphoreDbManager.ForceToUseTheSameDbConnection_ForTestOnly())
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
				TestObjectCreator.AALSHI.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "12345");
				TestObjectCreator.AddAPBankAccountDetails(TestObjectCreator.AALSHI, ReceiptTypes.eNettDirectDebit, TestObjectCreator.AUD);

				var container = CreateContainerForTest();

				var bankAccountCollection = new ENettRegisteredBankAccountCollection();
				var account = bankAccountCollection.AddNew();
				account.BankAccountPK = TestObjectCreator.AUDBankAccount.PK;
				Factory.Save();

				AccountingConfigurationRegistry.Instance.ENettStoragePaymentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CC3.PK.ToGuid());
				AccountingConfigurationRegistry.Instance.ENettRegisteredBankAccount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, bankAccountCollection);

				var payment = CreatePaymentForTest(container);

				payment.RunPreSaveValidation();
				AssertNoErrors(payment);
				using (var form = new ContainerStoragePaymentForm(payment))
				{
					form.Show();
					payment.Invoice.ReceiptPaymentAH_InvoiceDate = ZDateTime.Empty;
					Assert(!payment.Invoice.ReceiptPaymentAH_InvoiceDate.IsValid);
					form.FireSaveButton();
					Assert(payment.Invoice.ReceiptPaymentAH_InvoiceDate.IsValid);
				}
			}
		}

		IContainerStorageDataProvider CreateContainerForTest()
		{
			var shipment = TestObjectCreator.CreateShipment("1");
			var container = Factory.NewWithValidTestData<CommonContainer>();
			container.JC_ContainerNum = "123";
			container.JC_ArrivalSlotDateTime = ZDateTime.Today;
			var pack = shipment.OuterPackLines.AddNew();
			pack.Containers.Add(container);
			container.LinkedShipment = shipment;
			Factory.Save();
			return container;
		}

		StorageFeeInvoicePayment CreatePaymentForTest(IContainerStorageDataProvider container)
		{
			var payment = new StorageFeeInvoicePayment(container);
			payment.PortCode = "AUSYD";
			payment.Invoice.ExpectedInvoiceTotal = 2500M;
			payment.Initialise();
			payment.Invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			payment.Invoice.Lines[0].AL_OSExTaxAmount = payment.Invoice.ExpectedInvoiceTotal;
			payment.Invoice.Lines[0].AL_GE = TestObjectCreator.FESDepartment.PK;
			return payment;
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var containerStorageDataProvider = new Business.eNett_Integration.Testing.MockContainerStorageDataProvider();
			StorageFeeInvoicePayment storageFeeBizObj = new StorageFeeInvoicePayment(containerStorageDataProvider);
			return new ContainerStoragePaymentForm(storageFeeBizObj);
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		#endregion
	}
}
