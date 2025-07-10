using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.DataTransfer;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.Accounting.GUI.ARAP.AutoAllocationAndPrinting;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Accounting.Business.AccountingConstants;
using static Enterprise.Accounting.Module.TransactionModuleStrip;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Module.Testing
{
	public abstract class TransactionModuleStripTest : FilterGridModuleWithMultipleReversingTest
	{
		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			fTransactionModule = (TransactionModuleStrip)ZModuleFactory.Instance.Create(GetModuleID());
			Menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();
			vat3 = TestObjectCreator.CreateTaxRate("VAT3", "", 3);
			vat3.AT_PostingGroupId = 0;
			vat5 = TestObjectCreator.CreateTaxRate("VAT5", "", 5);
			vat5.AT_PostingGroupId = 1;
		}

		protected override void TearDown()
		{
			if (fTransactionModule != null)
			{
				fTransactionModule.Dispose();
			}
			base.TearDown();
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			collection.AddRange(GetBusinessObjectsToGetControllersFor());
		}

		protected override void PrepareModuleForBashing(ZEmbeddedModule module)
		{
			base.PrepareModuleForBashing(module);

			var bizos = new BusinessObject[]
			{
					CreateJournal(),
					CreateInvoice(),
					CreateCreditNote(),
					CreateReceipt(),
					CreatePayment(),
			};

			Factory.Save();

			var filterGridModule = (ZFilterGridModule)module;
			var collection = (BusinessObjectCollection)filterGridModule.GridCollection;

			collection.Load();

			AssertContainsExactElementsInAnyOrder(bizos.Select(b => b.HumanReadableName), collection.Cast<BusinessObject>().Select(b => b.HumanReadableName));
		}

		protected AccTaxRate vat3;
		protected AccTaxRate vat5;

		protected MenuItem[] Menu;

		protected TransactionModuleStrip fTransactionModule;
		protected TransactionModuleStrip TestTransactionModule
		{
			get { return fTransactionModule; }
		}

		protected override string CountryCode
		{
			get { return "AU"; }
		}

		protected abstract Invoice GetExistingInvoiceForTest { get; }

		protected virtual void SetupTransactionFilter() { }

		protected OrgHeader GetInactiveOrg()
		{
			OrgHeader newOrg = Factory.NewWithValidTestData<OrgHeader>();
			newOrg.OH_IsActive = false;
			newOrg.OH_IsDebtor = true;
			newOrg.OH_IsCreditor = true;

			return newOrg;
		}

		protected GlbDepartment GetTestDept()
		{
			GlbDepartment newDept = Factory.NewWithValidTestData<GlbDepartment>();
			newDept.GE_IsActive = false;
			Factory.Save();
			return newDept;
		}

		protected ARInvoice GetARInvoiceWithInactiveDept()
		{
			ARInvoice newARInv = Factory.NewWithValidTestData<ARInvoice>();
			newARInv.AH_GE = InactiveDept.PK;
			newARInv.AH_OH = InactiveOrg.PK;
			TestObjectCreator.CreateInvoiceLine(newARInv, newARInv.TransactionCurrency, newARInv.AH_ExchangeRate, 100m, 0m, 0m, 100m, 0m, 0m);
			newARInv.AH_FullyPaidDate = ZDateTime.Empty;
			Factory.Save();
			return newARInv;
		}

		protected APInvoice GetAPInvoiceWithInactiveDept()
		{
			APInvoice newAPInv = Factory.NewWithValidTestData<APInvoice>();
			newAPInv.AH_OH = InactiveOrg.PK;
			newAPInv.AH_GE = InactiveDept.PK;
			TestObjectCreator.CreateInvoiceLine(newAPInv, newAPInv.TransactionCurrency, newAPInv.AH_ExchangeRate, 100m, 0m, 0m, 100m, 0m, 0m);
			newAPInv.AH_FullyPaidDate = ZDateTime.Empty;
			Factory.Save();

			return newAPInv;
		}

		protected ARInvoice GetARInvoiceWithInactiveOrg(bool shouldSaveInvoice = true)
		{
			ARInvoice newARInv = Factory.NewWithValidTestData<ARInvoice>();
			newARInv.AH_OH = InactiveOrg.PK;
			var line = TestObjectCreator.CreateInvoiceLine(newARInv, newARInv.TransactionCurrency, newARInv.AH_ExchangeRate, 100m, 0m, 0m, 100m, 0m, 0m);
			line.AL_AC = TestObjectCreator.CC1.PK;
			newARInv.AH_FullyPaidDate = ZDateTime.Empty;
			if (shouldSaveInvoice)
			{
				Factory.Save();
			}
			return newARInv;
		}

		protected APInvoice GetAPInvoiceWithInactiveOrg(bool shouldSaveInvoice = true)
		{
			APInvoice newAPInv = Factory.NewWithValidTestData<APInvoice>();
			newAPInv.AH_OH = InactiveOrg.PK;
			TestObjectCreator.CreateInvoiceLine(newAPInv, newAPInv.TransactionCurrency, newAPInv.AH_ExchangeRate, 100m, 0m, 0m, 100m, 0m, 0m);
			newAPInv.AH_FullyPaidDate = ZDateTime.Empty;
			if (shouldSaveInvoice)
			{
				Factory.Save();
			}

			return newAPInv;
		}

		protected APInvoice APInv;
		protected ARInvoice ARInv;
		protected OrgHeader InactiveOrg;
		protected GlbDepartment InactiveDept;
		protected abstract ControllerID InvoiceControllerID { get; }
		protected abstract ControllerID DiscountControllerID { get; }
		protected abstract ControllerID OverpaymentControllerID { get; }
		protected abstract ControllerID ExchangeDifferenceControllerID { get; }
		public object ModifyTransactionDescriptionsSecurity { get; private set; }

		protected abstract AccTransactionHeader GetInvoiceToTestControllerID();

		#endregion

		public void TestAuditTransactionMenuItem()
		{
			if (TestTransactionModule.GetType() == typeof(APTransactionModuleStrip) || TestTransactionModule.GetType() == typeof(ARTransactionModuleStrip))
			{
				const string creator = "JYW";
				string auditor = GlbStaff.CurrentUser.GS_Code;

				var invoice = GetInvoiceToTestControllerID();
				invoice.AH_SystemCreateUser = creator;
				var invoice1 = GetInvoiceToTestControllerID();
				invoice1.AH_SystemCreateUser = creator;

				Factory.Save();

				AssertNotEquals("Precondition: The two users should not be identical.", creator, auditor);
				AssertEquals("Precondition: Auditer should be empty.", ZString.Empty, invoice.AH_GS_NKAuditedBy);
				AssertEquals("Precondition: Auditer should be empty.", ZString.Empty, invoice1.AH_GS_NKAuditedBy);

				using (TestTransactionModule)
				{
					using (ZForm form = new ZForm())
					{
						var menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();
						form.Controls.Add(TestTransactionModule.EmbeddedControl);
						form.Show();

						TestTransactionModule.PerformSearch_ForTest();

						var testCollection = (BusinessObjectCollection)TestTransactionModule.GetNewGridCollection_ForTestOnly();
						testCollection.Load();
						AssertEquals("Precondition: There should be 2 records.", 2, testCollection.Count);
						AssertEquals("Precondition: Allow to audit.", true, TestTransactionModule.AuditSecurityCheckpoint_ForTestOnly.IsAllowed);

						TestTransactionModule.DisplayGrid.SelectAllElements();
						UnitTestUserNotification.Instance.ClearMessages();
						menu.FindByText(AccountingConstants.AuditAndCashActionText.AuditTransactionText).PerformClick();

						testCollection.Load();
						foreach (var inv in testCollection)
						{
							AssertEquals(auditor, ((Invoice)inv).AH_GS_NKAuditedBy);
						}
						AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
					}
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestUndoAuditTransactionMenuItem()
		{
			if (TestTransactionModule.GetType() == typeof(APTransactionModuleStrip) || TestTransactionModule.GetType() == typeof(ARTransactionModuleStrip))
			{
				const string creator = "JYW";
				string auditor = GlbStaff.CurrentUser.GS_Code;

				var invoice = GetInvoiceToTestControllerID();
				invoice.AH_SystemCreateUser = creator;
				invoice.AH_GS_NKAuditedBy = auditor;
				var invoice1 = GetInvoiceToTestControllerID();
				invoice1.AH_SystemCreateUser = creator;
				invoice1.AH_GS_NKAuditedBy = auditor;

				Factory.Save();

				AssertNotEquals("Precondition: The two users should not be identical.", creator, auditor);
				AssertEquals("Precondition: Auditer should be auditor.", auditor, invoice.AH_GS_NKAuditedBy);
				AssertEquals("Precondition: Auditer should be auditor.", auditor, invoice1.AH_GS_NKAuditedBy);

				using (TestTransactionModule)
				{
					using (ZForm form = new ZForm())
					{
						var menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();
						form.Controls.Add(TestTransactionModule.EmbeddedControl);
						form.Show();

						TestTransactionModule.PerformSearch_ForTest();

						var testCollection = (BusinessObjectCollection)TestTransactionModule.GetNewGridCollection_ForTestOnly();
						testCollection.Load();
						AssertEquals("Precondition: There should be 2 records.", 2, testCollection.Count);
						AssertEquals("Precondition: Allow to undo audit.", true, TestTransactionModule.UndoAuditSecurityCheckpoint_ForTestOnly.IsAllowed);

						TestTransactionModule.DisplayGrid.SelectAllElements();
						UnitTestUserNotification.Instance.ClearMessages();
						menu.FindByText(AccountingConstants.AuditAndCashActionText.UndoAuditTransactionText).PerformClick();

						testCollection.Load();
						foreach (var inv in testCollection)
						{
							AssertEquals(ZString.Empty, ((Invoice)inv).AH_GS_NKAuditedBy);
						}
						AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
					}
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestRecordCashierMenuItem()
		{
			if (TestTransactionModule.GetType() == typeof(APTransactionModuleStrip) || TestTransactionModule.GetType() == typeof(ARTransactionModuleStrip))
			{
				const string creator = "JYW";
				string auditor = GlbStaff.CurrentUser.GS_Code;

				var arReceipt = Factory.NewWithValidTestData<ARReceipt>();
				arReceipt.AH_SystemCreateUser = creator;
				var arPayment = Factory.NewWithValidTestData<ARPayment>();
				arPayment.AH_SystemCreateUser = creator;
				var apReceipt = Factory.NewWithValidTestData<APReceipt>();
				apReceipt.AH_SystemCreateUser = creator;
				var apPayment = Factory.NewWithValidTestData<APPayment>();
				apPayment.AH_SystemCreateUser = creator;

				Factory.Save();

				AssertNotEquals("Precondition: The two users should not be identical.", creator, auditor);
				AssertEquals("Precondition: Auditer should be empty.", ZString.Empty, arReceipt.AH_GS_NKCashier);
				AssertEquals("Precondition: Auditer should be empty.", ZString.Empty, arPayment.AH_GS_NKCashier);
				AssertEquals("Precondition: Auditer should be empty.", ZString.Empty, apReceipt.AH_GS_NKCashier);
				AssertEquals("Precondition: Auditer should be empty.", ZString.Empty, apPayment.AH_GS_NKCashier);

				using (TestTransactionModule)
				{
					using (ZForm form = new ZForm())
					{
						var menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();
						form.Controls.Add(TestTransactionModule.EmbeddedControl);
						form.Show();

						TestTransactionModule.PerformSearch_ForTest();

						var testCollection = (BusinessObjectCollection)TestTransactionModule.GetNewGridCollection_ForTestOnly();
						testCollection.Load();
						AssertEquals("Precondition: There should be 2 records.", 2, testCollection.Count);
						AssertEquals("Precondition: Allow to record cashier.", true, TestTransactionModule.RecordCashierSecurityCheckpoint_ForTestOnly.IsAllowed);

						TestTransactionModule.DisplayGrid.SelectAllElements();
						UnitTestUserNotification.Instance.ClearMessages();
						menu.FindByText(AccountingConstants.AuditAndCashActionText.RecordCashierText).PerformClick();

						testCollection.Load();
						foreach (var transaction in testCollection)
						{
							AssertEquals(auditor, ((AccTransactionHeader)transaction).AH_GS_NKCashier);
						}
						AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
					}
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestClearCashierMenuItem()
		{
			if (TestTransactionModule.GetType() == typeof(APTransactionModuleStrip) || TestTransactionModule.GetType() == typeof(ARTransactionModuleStrip))
			{
				const string creator = "JYW";
				string cashier = GlbStaff.CurrentUser.GS_Code;

				var invoice = GetInvoiceToTestControllerID();
				invoice.AH_SystemCreateUser = creator;
				invoice.AH_GS_NKCashier = cashier;
				var invoice1 = GetInvoiceToTestControllerID();
				invoice1.AH_SystemCreateUser = creator;
				invoice1.AH_GS_NKCashier = cashier;

				Factory.Save();

				AssertNotEquals("Precondition: The two users should not be identical.", creator, cashier);
				AssertEquals("Precondition: Cashier should be cashier.", cashier, invoice.AH_GS_NKCashier);
				AssertEquals("Precondition: Cashier should be cashier.", cashier, invoice1.AH_GS_NKCashier);

				using (TestTransactionModule)
				{
					using (ZForm form = new ZForm())
					{
						var menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();
						form.Controls.Add(TestTransactionModule.EmbeddedControl);
						form.Show();

						TestTransactionModule.PerformSearch_ForTest();

						var testCollection = (BusinessObjectCollection)TestTransactionModule.GetNewGridCollection_ForTestOnly();
						testCollection.Load();
						AssertEquals("Precondition: There should be 2 records.", 2, testCollection.Count);
						AssertEquals("Precondition: Allow to clean cashier.", true, TestTransactionModule.ClearCashierSecurityCheckpoint_ForTestOnly.IsAllowed);

						TestTransactionModule.DisplayGrid.SelectAllElements();
						UnitTestUserNotification.Instance.ClearMessages();
						menu.FindByText(AccountingConstants.AuditAndCashActionText.ClearCashierText).PerformClick();

						testCollection.Load();
						foreach (var inv in testCollection)
						{
							AssertEquals(ZString.Empty, ((Invoice)inv).AH_GS_NKCashier);
						}
						AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
					}
				}
			}
			else
			{
				Assert(true);
			}
		}

		#region Compliance Documents test cases

		public void TestMenuItemAddedCorrectly()
		{
			if (TestTransactionModule.GetType() == typeof(APTransactionModuleStrip) || TestTransactionModule.GetType() == typeof(ARTransactionModuleStrip))
			{
				using (TestTransactionModule)
				{
					using (var form = new ZForm())
					{
						var menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();
						var menuitem = menu.FindByText("Create Compliance Document Records");
						AssertNull(menuitem);
					}
				}

				using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
				{
					var menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();
					var menuitem = menu.FindByText("Create Compliance Document Records");
					AssertNotNull(menuitem);

					var subMenuItem1 = menuitem.MenuItems.FindByText("Roll-up by Charge Code");
					AssertNotNull(subMenuItem1);
					var subMenuItem2 = menuitem.MenuItems.FindByText("No Roll-up");
					AssertNotNull(subMenuItem2);
				}
			}
			else
			{
				using (TestTransactionModule)
				{
					using (var form = new ZForm())
					{
						var menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();
						var menuitem = menu.FindByText("Create Compliance Document Records");
						AssertNull(menuitem);
					}
				}

				using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
				{
					var menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();
					var menuitem = menu.FindByText("Create Compliance Document Records");
					AssertNull(menuitem);
				}
			}
		}

		public void TestCreateComplianceDocumentRecordsWithRollup()
		{
			Action createAction = (() =>
			{
				#region Create Transactions

				var ac1 = TestObjectCreator.CreateChargeCode("AC1");
				ac1.AC_AT_GSTRate = vat3.PK;
				ac1.AC_Desc = "desc";

				var ac2 = TestObjectCreator.CreateChargeCode("AC2");
				ac2.AC_AT_GSTRate = vat5.PK;
				ac2.AC_Desc = "desc";

				var ac3 = TestObjectCreator.CreateChargeCode("AC3");
				ac3.AC_AT_GSTRate = vat3.PK;
				ac3.AC_Desc = "desc";

				var invoice = GetInvoiceToTestControllerID() as InvoicingBase;
				invoice.AH_OH = TestObjectCreator.Agent.PK;
				var invoiceLine = invoice.Lines.AddNew() as InvoicingLineBase;
				invoiceLine.AL_JH = TestObjectCreator.Job1.PK;
				invoiceLine.AL_AC = ac1.PK;
				invoiceLine.AL_AT = vat3.PK;

				var charge = Factory.NewWithValidTestData<JobCharge>();
				charge.JR_JH = TestObjectCreator.Job1.PK;
				charge.JR_AC = ac1.PK;
				charge.JR_AT_CostGSTRate = invoiceLine.AL_AT;
				charge.JR_AL_APLine = invoice is APInvoice ? invoiceLine.PK : ZGuid.Empty;
				charge.JR_AL_ARLine = invoice is ARInvoice ? invoiceLine.PK : ZGuid.Empty;

				var invoiceLine2 = invoice.Lines.AddNew() as InvoicingLineBase;
				invoiceLine2.AL_JH = TestObjectCreator.Job1.PK;
				invoiceLine2.AL_AC = ac3.PK;
				invoiceLine2.AL_AT = vat3.PK;

				var charge2 = Factory.NewWithValidTestData<JobCharge>();
				charge2.JR_JH = TestObjectCreator.Job1.PK;
				charge2.JR_AC = ac3.PK;
				charge2.JR_AT_CostGSTRate = invoiceLine2.AL_AT;
				charge2.JR_AL_APLine = invoice is APInvoice ? invoiceLine2.PK : ZGuid.Empty;
				charge2.JR_AL_ARLine = invoice is ARInvoice ? invoiceLine2.PK : ZGuid.Empty;

				var invoiceLine3 = invoice.Lines.AddNew() as InvoicingLineBase;
				invoiceLine3.AL_JH = TestObjectCreator.Job1.PK;
				invoiceLine3.AL_AC = ac2.PK;
				invoiceLine3.AL_AT = vat5.PK;

				var charge3 = Factory.NewWithValidTestData<JobCharge>();
				charge3.JR_JH = TestObjectCreator.Job1.PK;
				charge3.JR_AC = ac2.PK;
				charge3.JR_AT_CostGSTRate = invoiceLine3.AL_AT;
				charge3.JR_AL_APLine = invoice is APInvoice ? invoiceLine3.PK : ZGuid.Empty;
				charge3.JR_AL_ARLine = invoice is ARInvoice ? invoiceLine3.PK : ZGuid.Empty;

				var invoice1 = GetInvoiceToTestControllerID() as InvoicingBase;
				invoice1.AH_OH = TestObjectCreator.Agent.PK;
				var invoiceLine4 = invoice1.Lines.AddNew() as InvoicingLineBase;
				invoiceLine4.AL_JH = TestObjectCreator.Job2.PK;
				invoiceLine4.AL_AC = ac1.PK;
				invoiceLine4.AL_AT = vat3.PK;

				var charge4 = Factory.NewWithValidTestData<JobCharge>();
				charge4.JR_JH = TestObjectCreator.Job2.PK;
				charge4.JR_AC = ac1.PK;
				charge4.JR_AT_CostGSTRate = invoiceLine4.AL_AT;
				charge4.JR_AL_APLine = invoice1 is APInvoice ? invoiceLine4.PK : ZGuid.Empty;
				charge4.JR_AL_ARLine = invoice1 is ARInvoice ? invoiceLine4.PK : ZGuid.Empty;

				Factory.Save();

				#endregion
			});

			Action assertAction = (() =>
			{
				var headers = Factory.Load<AccComplianceDocumentHeader>(new ZQuery());
				AssertEquals(2, headers.Length);
				var lines = Factory.Load<AccComplianceDocumentLine>(new ZQuery());
				AssertEquals(3, lines.Length);
				var pivot = Factory.Load<AccComplianceDocumentPivot>(new ZQuery());
				AssertEquals(4, pivot.Length);
				Assert(!UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Compliance Records created successfully with Roll-up by Charge Code.", UnitTestUserNotification.Instance.LastMessage.Text);
			});

			AssertCreateComplianceDocumentRecords(createAction, assertAction, true);
		}

		public void TestCreateComplianceDocumentRecordsWithoutRollup()
		{
			Action createAction = (() =>
			{
				var ac1 = TestObjectCreator.CreateChargeCode("AC1");
				ac1.AC_AT_GSTRate = vat3.PK;
				ac1.AC_Desc = "desc";

				var ac2 = TestObjectCreator.CreateChargeCode("AC2");
				ac2.AC_AT_GSTRate = vat5.PK;
				ac2.AC_Desc = "desc";

				var invoice = GetInvoiceToTestControllerID() as InvoicingBase;
				invoice.AH_OH = TestObjectCreator.Agent.PK;
				var invoiceLine = invoice.Lines.AddNew() as InvoicingLineBase;
				invoiceLine.AL_JH = TestObjectCreator.Job1.PK;
				invoiceLine.AL_AC = ac1.PK;
				invoiceLine.AL_AT = vat3.PK;

				var charge = Factory.NewWithValidTestData<JobCharge>();
				charge.JR_JH = TestObjectCreator.Job1.PK;
				charge.JR_LocalCostAmt = 10m;
				charge.JR_OSCostAmt = 10m;
				charge.JR_AC = ac1.PK;
				charge.JR_AT_CostGSTRate = invoiceLine.AL_AT;
				charge.JR_AL_APLine = invoice is APInvoice ? invoiceLine.PK : ZGuid.Empty;
				charge.JR_AL_ARLine = invoice is ARInvoice ? invoiceLine.PK : ZGuid.Empty;

				var invoiceLine2 = invoice.Lines.AddNew() as InvoicingLineBase;
				invoiceLine2.AL_JH = TestObjectCreator.Job1.PK;
				invoiceLine2.AL_AC = ac2.PK;
				invoiceLine2.AL_AT = vat5.PK;
				var charge2 = Factory.NewWithValidTestData<JobCharge>();
				charge2.JR_JH = TestObjectCreator.Job1.PK;
				charge2.JR_AC = ac2.PK;
				charge2.JR_AT_CostGSTRate = invoiceLine2.AL_AT;
				charge2.JR_AL_APLine = invoice is APInvoice ? invoiceLine2.PK : ZGuid.Empty;
				charge2.JR_AL_ARLine = invoice is ARInvoice ? invoiceLine2.PK : ZGuid.Empty;

				Factory.Save();
			});

			Action assertAction = (() =>
			{
				var headers = Factory.Load<AccComplianceDocumentHeader>(new ZQuery());
				AssertEquals(2, headers.Length);
				var lines = Factory.Load<AccComplianceDocumentLine>(new ZQuery());
				AssertEquals(2, lines.Length);
				var pivot = Factory.Load<AccComplianceDocumentPivot>(new ZQuery());
				AssertEquals(2, pivot.Length);
				Assert(!UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Compliance Records created successfully without Roll-up.", UnitTestUserNotification.Instance.LastMessage.Text);
			});

			AssertCreateComplianceDocumentRecords(createAction, assertAction, false);
		}

		public void TestCreateComplianceDocumentRecordsWithAllNegativeCompliances()
		{
			AssertCreateComplianceDocumentRecordsWithAllNegativeCompliances(true);
		}

		public void TestCreateComplianceDocumentRecordsWithAllNegativeCompliances2()
		{
			AssertCreateComplianceDocumentRecordsWithAllNegativeCompliances(false);
		}

		void AssertCreateComplianceDocumentRecordsWithAllNegativeCompliances(bool flag)
		{
			Action createAction = () =>
			{
				AccountingMasterFilesRegistry.Instance.AllowNegativeComplianceDocumentLines.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, flag);

				if (TestTransactionModule is ARTransactionModuleStrip)
				{
					var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
					var arInvLine = (ARInvoiceLine)arInvoice.Lines.AddNew();
					arInvLine.AL_AG = TestObjectCreator.GLHeader1.PK;
					arInvLine.AL_OSExTaxAmount = -100m;
					arInvLine.AL_AC = TestObjectCreator.FRT.PK;
					arInvLine.AL_AT = TestObjectCreator.GST1.PK;

					var arInvoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV002", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor1);
					var arInvLine1 = (ARInvoiceLine)arInvoice1.Lines.AddNew();
					arInvLine1.AL_AG = TestObjectCreator.GLHeader1.PK;
					arInvLine1.AL_OSExTaxAmount = -100m;
					arInvLine1.AL_AC = TestObjectCreator.FRT.PK;
					arInvLine1.AL_AT = TestObjectCreator.GST1.PK;
				}
				else
				{
					var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1m, -100m, -5m, 0m, -100m, -5m, 0m);
					apInvoice.AH_OH = TestObjectCreator.TestOrganisation.PK;
					var apInvLine = apInvoice.Lines.First() as APInvoiceLine;
					apInvLine.AL_AG = TestObjectCreator.GLHeader1.PK;
					apInvLine.AL_AC = TestObjectCreator.CommentChargeCode.PK;
					apInvLine.AL_AT = TestObjectCreator.GST1.PK;
					apInvLine.AL_OSExTaxAmount = -100m;
					apInvLine.ComplianceDocumentNumber = "AA00000001";

					var apInvoice1 = TestObjectCreator.CreateAPInvoice<APInvoice>("INV002", TestObjectCreator.AUD, 1m, -100m, -5m, 0m, -100m, -5m, 0m);
					apInvoice1.AH_OH = TestObjectCreator.TestOrganisation.PK;
					var apInvLine1 = apInvoice1.Lines.First() as APInvoiceLine;
					apInvLine1.AL_AG = TestObjectCreator.GLHeader1.PK;
					apInvLine1.AL_AC = TestObjectCreator.CommentChargeCode.PK;
					apInvLine1.AL_AT = TestObjectCreator.GST1.PK;
					apInvLine1.AL_OSExTaxAmount = -100m;
					apInvLine1.ComplianceDocumentNumber = "AA00000002";
				}

				Factory.Save();
			};

			Action assertAction = () =>
			{
				var headers = Factory.Load<AccComplianceDocumentHeader>(new ZQuery());
				AssertEquals(0, headers.Length);
				var lines = Factory.Load<AccComplianceDocumentLine>(new ZQuery());
				AssertEquals(0, lines.Length);
				var pivot = Factory.Load<AccComplianceDocumentPivot>(new ZQuery());
				AssertEquals(0, pivot.Length);
				AssertEquals(flag ? ComplianceDocumentNegativeHeadersMessage : ComplianceDocumentNegativeLinesMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			};

			AssertCreateComplianceDocumentRecords(createAction, assertAction, false);
		}

		public void TestCreateComplianceDocumentRecordsWithPartNegativeCompliances()
		{
			AssertCreateComplianceDocumentRecordsWithPartNegativeCompliances(true);
		}

		public void TestCreateComplianceDocumentRecordsWithPartNegativeCompliances2()
		{
			AssertCreateComplianceDocumentRecordsWithPartNegativeCompliances(false);
		}

		void AssertCreateComplianceDocumentRecordsWithPartNegativeCompliances(bool flag)
		{
			Action createAction = () =>
			{
				AccountingMasterFilesRegistry.Instance.AllowNegativeComplianceDocumentLines.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, flag);

				if (TestTransactionModule is ARTransactionModuleStrip)
				{
					var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
					var arInvLine = (ARInvoiceLine)arInvoice.Lines.AddNew();
					arInvLine.AL_AG = TestObjectCreator.GLHeader1.PK;
					arInvLine.AL_OSExTaxAmount = -100m;
					arInvLine.AL_AC = TestObjectCreator.FRT.PK;
					arInvLine.AL_AT = TestObjectCreator.GST1.PK;

					var arInvoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV002", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor1);
					var arInvLine1 = (ARInvoiceLine)arInvoice1.Lines.AddNew();
					arInvLine1.AL_AG = TestObjectCreator.GLHeader1.PK;
					arInvLine1.AL_OSExTaxAmount = -100m;
					arInvLine1.AL_AC = TestObjectCreator.FRT.PK;
					arInvLine1.AL_AT = TestObjectCreator.GST1.PK;

					var arInvoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV003", TestObjectCreator.AUD, 1m, TestObjectCreator.TestOrganisation);
					var arInvLine2 = (ARInvoiceLine)arInvoice2.Lines.AddNew();
					arInvLine2.AL_AG = TestObjectCreator.GLHeader1.PK;
					arInvLine2.AL_OSExTaxAmount = 100m;
					arInvLine2.AL_AC = TestObjectCreator.FRT.PK;
					arInvLine2.AL_AT = TestObjectCreator.GST1.PK;
				}
				else
				{
					var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1m, -100m, -5m, 0m, -100m, -5m, 0m);
					apInvoice.AH_OH = TestObjectCreator.Creditor1.PK;
					var apInvLine = apInvoice.Lines.First() as APInvoiceLine;
					apInvLine.AL_AG = TestObjectCreator.GLHeader1.PK;
					apInvLine.AL_AC = TestObjectCreator.CommentChargeCode.PK;
					apInvLine.AL_AT = TestObjectCreator.GST1.PK;
					apInvLine.AL_OSExTaxAmount = -100m;
					apInvLine.ComplianceDocumentNumber = "AA00000001";

					var apInvoice1 = TestObjectCreator.CreateAPInvoice<APInvoice>("INV002", TestObjectCreator.AUD, 1m, -100m, -5m, 0m, -100m, -5m, 0m);
					apInvoice1.AH_OH = TestObjectCreator.Creditor2.PK;
					var apInvLine1 = apInvoice1.Lines.First() as APInvoiceLine;
					apInvLine1.AL_AG = TestObjectCreator.GLHeader1.PK;
					apInvLine1.AL_AC = TestObjectCreator.CommentChargeCode.PK;
					apInvLine1.AL_AT = TestObjectCreator.GST1.PK;
					apInvLine1.AL_OSExTaxAmount = -100m;
					apInvLine1.ComplianceDocumentNumber = "AA00000002";

					var apInvoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("INV003", TestObjectCreator.AUD, 1m, 100m, 5m, 0m, 100m, 5m, 0m);
					apInvoice2.AH_OH = TestObjectCreator.TestOrganisation.PK;
					var apInvLine2 = apInvoice2.Lines.First() as APInvoiceLine;
					apInvLine2.AL_AG = TestObjectCreator.GLHeader1.PK;
					apInvLine2.AL_AC = TestObjectCreator.CommentChargeCode.PK;
					apInvLine2.AL_AT = TestObjectCreator.GST1.PK;
					apInvLine2.AL_OSExTaxAmount = 100m;
					apInvLine2.ComplianceDocumentNumber = "AA00000003";
				}

				Factory.Save();
			};

			Action assertAction = () =>
			{
				var headers = Factory.Load<AccComplianceDocumentHeader>(new ZQuery());
				AssertEquals(1, headers.Length);
				var lines = Factory.Load<AccComplianceDocumentLine>(new ZQuery());
				AssertEquals(1, lines.Length);
				var pivot = Factory.Load<AccComplianceDocumentPivot>(new ZQuery());
				AssertEquals(1, pivot.Length);

				if (TestTransactionModule is ARTransactionModuleStrip)
				{
					AssertEquals(GetComplianceDocumentNegativeMessageWithTransNum("00001000, 00001001"), UnitTestUserNotification.Instance.LastMessage.Text);
				}
				else
				{
					AssertEquals(GetComplianceDocumentNegativeMessageWithTransNum("INV001, INV002"), UnitTestUserNotification.Instance.LastMessage.Text);
				}
			};

			AssertCreateComplianceDocumentRecords(createAction, assertAction, false);
		}

		void AssertCreateComplianceDocumentRecords(Action createAction, Action assertAction, bool rollup)
		{
			if (TestTransactionModule.GetType() == typeof(APTransactionModuleStrip) || TestTransactionModule.GetType() == typeof(ARTransactionModuleStrip))
			{
				using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					createAction();

					using (TestTransactionModule)
					using (var form = new ZForm())
					{
						var menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();
						form.Controls.Add(TestTransactionModule.EmbeddedControl);
						form.Show();

						TestTransactionModule.PerformSearch_ForTest();

						var testCollection = (BusinessObjectCollection)TestTransactionModule.GetNewGridCollection_ForTestOnly();
						testCollection.Load();

						TestTransactionModule.DisplayGrid.SelectAllElements();
						UnitTestUserNotification.Instance.ClearMessages();
						menu.FindByText("Create Compliance Document Records").MenuItems.FindByText(rollup ? "Roll-up by Charge Code" : "No Roll-up").PerformClick();
					}

					assertAction();
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestCheckComplianceDocumentRecordExisted()
		{
			if (TestTransactionModule.GetType() == typeof(APTransactionModuleStrip) || TestTransactionModule.GetType() == typeof(ARTransactionModuleStrip))
			{
				using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
				{
					var ac1 = TestObjectCreator.CreateChargeCode("AC1");
					ac1.AC_AT_GSTRate = vat3.PK;
					ac1.AC_Desc = "desc";

					var invoice = GetInvoiceToTestControllerID() as InvoicingBase;
					invoice.AH_OH = TestObjectCreator.Agent.PK;
					var invoiceLine = invoice.Lines.AddNew() as InvoicingLineBase;
					invoiceLine.AL_JH = TestObjectCreator.Job1.PK;
					invoiceLine.AL_AC = ac1.PK;
					invoiceLine.AL_AT = vat3.PK;

					var charge = Factory.NewWithValidTestData<JobCharge>();
					charge.JR_JH = TestObjectCreator.Job1.PK;
					charge.JR_AC = ac1.PK;
					charge.JR_AT_CostGSTRate = invoiceLine.AL_AT;
					charge.JR_AL_APLine = invoice is APInvoice ? invoiceLine.PK : ZGuid.Empty;
					charge.JR_AL_ARLine = invoice is ARInvoice ? invoiceLine.PK : ZGuid.Empty;

					Factory.Save();

					using (TestTransactionModule)
					using (var form = new ZForm())
					{
						var menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();
						form.Controls.Add(TestTransactionModule.EmbeddedControl);
						form.Show();

						TestTransactionModule.PerformSearch_ForTest();

						var testCollection = (BusinessObjectCollection)TestTransactionModule.GetNewGridCollection_ForTestOnly();
						testCollection.Load();

						TestTransactionModule.DisplayGrid.SelectAllElements();
						UnitTestUserNotification.Instance.ClearMessages();
						var createComplianceMenu = menu.FindByText("Create Compliance Document Records").MenuItems.FindByText("No Roll-up");
						createComplianceMenu.PerformClick();

						var headers = Factory.Load<AccComplianceDocumentHeader>(new ZQuery());
						AssertEquals(1, headers.Length);
						var lines = Factory.Load<AccComplianceDocumentLine>(new ZQuery());
						AssertEquals(1, lines.Length);
						var pivot = Factory.Load<AccComplianceDocumentPivot>(new ZQuery());
						AssertEquals(1, pivot.Length);
						Assert(!UnitTestUserNotification.Instance.LastMessage.WasError);
						AssertEquals("Compliance Records created successfully without Roll-up.", UnitTestUserNotification.Instance.LastMessage.Text);

						createComplianceMenu.PerformClick();

						headers = Factory.Load<AccComplianceDocumentHeader>(new ZQuery());
						AssertEquals(1, headers.Length);
						lines = Factory.Load<AccComplianceDocumentLine>(new ZQuery());
						AssertEquals(1, lines.Length);
						pivot = Factory.Load<AccComplianceDocumentPivot>(new ZQuery());
						AssertEquals(1, pivot.Length);
						AssertEquals("Some of compliance document records has been created in selected transactions before.", UnitTestUserNotification.Instance.LastMessage.Text);

						//void compiance document and create again
						headers[0].Void();
						Factory.Save();
						UnitTestUserNotification.Instance.ClearMessages();
						createComplianceMenu.PerformClick();
						Assert(!UnitTestUserNotification.Instance.LastMessage.WasError);
						AssertEquals("Compliance Records created successfully without Roll-up.", UnitTestUserNotification.Instance.LastMessage.Text);

						createComplianceMenu.PerformClick();
						AssertEquals("Some of compliance document records has been created in selected transactions before.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestCheckHasLineWithoutTaxID()
		{
			Action createAction = () =>
			{
				if (TestTransactionModule is APTransactionModuleStrip)
				{
					var apInvoice = Factory.NewWithValidTestData<APInvoice>();
					var apInvoiceLine = apInvoice.Lines.AddNew() as APInvoiceLine;
					apInvoiceLine.AL_AC = ZGuid.Empty;
					apInvoiceLine.AL_AG = TestObjectCreator.GLHeader1.PK;
				}
				else
				{
					var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
					var arInvoiceLine = arInvoice.Lines.AddNew() as ARInvoiceLine;
					arInvoiceLine.AL_AC = ZGuid.Empty;
					arInvoiceLine.AL_AG = TestObjectCreator.GLHeader1.PK;
				}
				Factory.Save();
			};

			Action assertAction = () =>
			{
				var headers = Factory.Load<AccComplianceDocumentHeader>(new ZQuery());
				AssertEquals(0, headers.Length);
				var lines = Factory.Load<AccComplianceDocumentLine>(new ZQuery());
				AssertEquals(0, lines.Length);
				var pivot = Factory.Load<AccComplianceDocumentPivot>(new ZQuery());
				AssertEquals(0, pivot.Length);
				AssertEquals("Some of selected transaction does not have a tax rate.", UnitTestUserNotification.Instance.LastMessage.Text);
			};

			AssertCreateComplianceDocumentRecords(createAction, assertAction, false);
		}

		public void TestCheckAccTaxRateIsNullWithCMTChargeForMultipleTransaction()
		{
			AssertCheckAccTaxRateIsNullWithCMTCharge(() => { CreateInvoiceAction().Invoke(); CreatePaymentAction().Invoke(); }, 0, 0, 0);
		}

		public void TestCheckAccTaxRateIsNullWithCMTChargeForInvoice()
		{
			AssertCheckAccTaxRateIsNullWithCMTCharge(CreateInvoiceAction(), 1, 1, 1);
		}

		Action CreateInvoiceAction()
		{
			return () =>
			{
				if (TestTransactionModule is APTransactionModuleStrip)
				{
					var apInvoice1 = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1m, -100m, -5m, 0m, -100m, -5m, 0m);
					apInvoice1.AH_OH = TestObjectCreator.Creditor2.PK;
					var apInvLine1 = apInvoice1.Lines.First() as APInvoiceLine;
					apInvLine1.AL_AG = TestObjectCreator.GLHeader1.PK;
					apInvLine1.AL_AC = TestObjectCreator.CommentChargeCode.PK;
					apInvLine1.AL_AT = ZGuid.Empty;
					apInvLine1.AL_OSExTaxAmount = 100m;
					apInvLine1.ComplianceDocumentNumber = "AA00000001";

					var apInvoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("INV002", TestObjectCreator.AUD, 1m, 100m, 5m, 0m, 100m, 5m, 0m);
					apInvoice2.AH_OH = TestObjectCreator.TestOrganisation.PK;
					var apInvLine2 = apInvoice2.Lines.First() as APInvoiceLine;
					apInvLine2.AL_AG = TestObjectCreator.GLHeader1.PK;
					apInvLine2.AL_AC = TestObjectCreator.CommentChargeCode.PK;
					apInvLine2.AL_AT = TestObjectCreator.GST1.PK;
					apInvLine2.AL_OSExTaxAmount = 200m;
					apInvLine2.ComplianceDocumentNumber = "AA00000002";
				}
				else
				{
					var arInvoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor1);
					var arInvLine1 = (ARInvoiceLine)arInvoice1.Lines.AddNew();
					arInvLine1.AL_AG = TestObjectCreator.GLHeader1.PK;
					arInvLine1.AL_OSExTaxAmount = 100m;
					arInvLine1.AL_AC = TestObjectCreator.CommentChargeCode.PK;
					arInvLine1.AL_AT = ZGuid.Empty;
					arInvLine1.ChargeCode.AC_Code = "CMT";

					var arInvoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV002", TestObjectCreator.AUD, 1m, TestObjectCreator.TestOrganisation);
					var arInvLine2 = (ARInvoiceLine)arInvoice2.Lines.AddNew();
					arInvLine2.AL_AG = TestObjectCreator.GLHeader1.PK;
					arInvLine2.AL_OSExTaxAmount = 200m;
					arInvLine2.AL_AC = TestObjectCreator.FRT.PK;
					arInvLine2.AL_AT = TestObjectCreator.GST1.PK;
				}
			};
		}

		void AssertCheckAccTaxRateIsNullWithCMTCharge(Action createTransactionAction, int expectedComplianceDocumentHeaderCount, int expectedComplianceDocumentLineCount, int expectedComplianceDocumentPivotCount)
		{
			Action createAction = () =>
			{
				createTransactionAction();
				Factory.Save();
			};

			Action assertAction = () =>
			{
				var headers = Factory.Load<AccComplianceDocumentHeader>(new ZQuery());
				AssertEquals("compliance document header count", expectedComplianceDocumentHeaderCount, headers.Length);
				var lines = Factory.Load<AccComplianceDocumentLine>(new ZQuery());
				AssertEquals("compliance document line count", expectedComplianceDocumentLineCount, lines.Length);
				var pivot = Factory.Load<AccComplianceDocumentPivot>(new ZQuery());
				AssertEquals("compliance document pivot count", expectedComplianceDocumentPivotCount, pivot.Length);
			};

			AssertCreateComplianceDocumentRecords(createAction, assertAction, false);
		}

		Action CreatePaymentAction()
		{
			return () =>
			{
				if (TestTransactionModule is APTransactionModuleStrip)
				{
					TestObjectCreator.CreateAPPayment(1, 10, ZDateTime.Today, ZDateTime.Today, TestObjectCreator.AALSHI.PK, TestObjectCreator.AUDBankAccount.PK);
				}
				else
				{
					TestObjectCreator.CreateARPayment(1, 20, ZDateTime.Today, ZDateTime.Today, TestObjectCreator.AALSHI.PK, TestObjectCreator.AUDBankAccount.PK);
				}
			};
		}

		Action CreateAdjustmentNoteAction()
		{
			return () =>
			{
				if (TestTransactionModule is APTransactionModuleStrip)
				{
					TestObjectCreator.CreateAdjustmentNote<APAdjustmentNote>("001", 100, 10, ZDateTime.Today, TestObjectCreator.Agent.PK);
				}
				else
				{
					TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("002", 200, 20, ZDateTime.Today, TestObjectCreator.Agent.PK);
				}
			};
		}

		public void TestCheckTransactionIsInvoiceOrCreditNoteForAdjustmentNote()
		{
			AssertCheckTransactionIsInvoiceOrCreditNote(CreateAdjustmentNoteAction());
		}

		public void TestCheckTransactionIsInvoiceOrCreditNoteForPayment()
		{
			AssertCheckTransactionIsInvoiceOrCreditNote(CreatePaymentAction());
		}

		public void TestCheckTransactionIsInvoiceOrCreditNoteForMultipleTransaction()
		{
			AssertCheckTransactionIsInvoiceOrCreditNote(() => { CreateAdjustmentNoteAction().Invoke(); CreatePaymentAction().Invoke(); });
		}

		void AssertCheckTransactionIsInvoiceOrCreditNote(Action createTransactionAction)
		{
			Action createAction = (() =>
			{
				createTransactionAction();
				Factory.Save();
			});

			Action assertAction = (() =>
			{
				var headers = Factory.Load<AccComplianceDocumentHeader>(new ZQuery());
				AssertEquals(0, headers.Length);
				var lines = Factory.Load<AccComplianceDocumentLine>(new ZQuery());
				AssertEquals(0, lines.Length);
				var pivot = Factory.Load<AccComplianceDocumentPivot>(new ZQuery());
				AssertEquals(0, pivot.Length);
				AssertEquals("Selected transactions should be Invoices or Credit Notes.", UnitTestUserNotification.Instance.LastMessage.Text);
			});

			AssertCreateComplianceDocumentRecords(createAction, assertAction, false);
		}

		public void TestCheckHasTransactionCancelled()
		{
			Action createAction = (() =>
			{
				Invoice invoice = null;
				if (TestTransactionModule is APTransactionModuleStrip)
				{
					invoice = Factory.NewWithValidTestData<APInvoice>();
					var apInvoiceLine = invoice.Lines.AddNew() as APInvoiceLine;
				}
				else
				{
					invoice = Factory.NewWithValidTestData<ARInvoice>();
					var arInvoiceLine = invoice.Lines.AddNew() as ARInvoiceLine;
				}
				invoice.AH_IsCancelled = true;
				invoice.Lines[0].AL_AG = TestObjectCreator.GLHeader1.PK;
				TransactionMatchLink matchLink = ((IMatching)invoice).CurrentMatchGroup.AddNew(); // to pass IsCancelled check
				matchLink.AP_AH = invoice.PK;
				TestObjectCreator.SetupMatchLinkMatchDate(matchLink);
				Factory.Save();
			});

			Action assertAction = (() =>
			{
				var headers = Factory.Load<AccComplianceDocumentHeader>(new ZQuery());
				AssertEquals(0, headers.Length);
				var lines = Factory.Load<AccComplianceDocumentLine>(new ZQuery());
				AssertEquals(0, lines.Length);
				var pivot = Factory.Load<AccComplianceDocumentPivot>(new ZQuery());
				AssertEquals(0, pivot.Length);
				AssertEquals("No compliance document record created. Compliance document record cannot be created for canceled transactions.", UnitTestUserNotification.Instance.LastMessage.Text);
			});

			AssertCreateComplianceDocumentRecords(createAction, assertAction, false);
		}

		#region Prompt To Print Compliance Document test cases

		public abstract void TestPromptToPrintComplianceDocumentWithRollup();

		public abstract void TestPromptToPrintComplianceDocumentWithoutRollup();

		protected void AssertForPromptToPrintComplianceDocument()
		{
			AssertContains("Do you want to print Compliance Document?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("LastFormShownDialogForTest should be DocDeliveryForm", "DocDeliveryForm", ZFormModaliser.LastFormShownDialogForTest.GetType().Name);
			AssertEquals("LastIBusinessShownOnDialogForTest should be DeliveryInstructions", typeof(DeliveryInstructions), ZFormModaliser.LastIBusinessShownOnDialogForTest.GetType());
		}

		protected void AssertForNotPromptToPrintComplianceDocument()
		{
			AssertNotContains("Do you want to print Compliance Document?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertNull("LastFormShownDialogForTest should be null", ZFormModaliser.LastFormShownDialogForTest);
			AssertNull("LastIBusinessShownOnDialogForTest should be null", ZFormModaliser.LastIBusinessShownOnDialogForTest);
		}

		protected void PromptToPrintComplianceDocumentCore(Action assertAction, bool isRollup, bool isPromptToPrintComplianceDocument)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Enterprise.Core.Constants.CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.PromptToPrintComplianceDocumentOnCreation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isPromptToPrintComplianceDocument))
			{
				var stmMenu = Factory.Load<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Triplicate Cash Register GUI").AddToFilter(StmMenuItemSchema.SU_BusinessContext, "ARComplianceDocument")).First();
				TestObjectCreator.SetupComplianceSequence(stmMenu.PK, "TDP", "AAA", 1, 100, 1);
				TestObjectCreator.SetupComplianceSequence(stmMenu.PK, "TCD", "AAA", 1, 100, 1);

				var invoice = GetInvoiceToTestControllerID() as InvoicingBase;
				invoice.AH_OH = TestObjectCreator.Agent.PK;
				var invoiceLine = invoice.Lines.AddNew() as InvoicingLineBase;
				var chargeList = invoiceLine.ChargeList;
				chargeList.Load();
				invoiceLine.GenericCharge = chargeList[0].PK;
				invoiceLine.AL_OSExTaxAmount = 10m;
				invoiceLine.AL_AT = TestObjectCreator.GST1.PK;

				var creditNote = CreateCreditNote();
				creditNote.AH_OH = TestObjectCreator.Agent.PK;
				var creditNoteLine = creditNote.Lines.AddNew() as InvoicingLineBase;
				chargeList = creditNoteLine.ChargeList;
				chargeList.Load();
				creditNoteLine.GenericCharge = chargeList[0].PK;
				creditNoteLine.AL_OSExTaxAmount = 10m;
				creditNoteLine.AL_AT = TestObjectCreator.GST1.PK;
				Factory.Save();

				fTransactionModule.Dispose();
				using (fTransactionModule = (TransactionModuleStrip)ZModuleFactory.Instance.Create(GetModuleID()))
				using (var form = new ZForm())
				{
					var menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();
					if (menu.FindByText("Create Compliance Document Records") != null)
					{
						form.Controls.Add(TestTransactionModule.EmbeddedControl);
						form.Show();

						TestTransactionModule.PerformSearch_ForTest();

						var testCollection = (BusinessObjectCollection)TestTransactionModule.GetNewGridCollection_ForTestOnly();
						testCollection.Load();

						ZFormModaliser.LastFormShownDialogForTest = null;
						ZFormModaliser.LastIBusinessShownOnDialogForTest = null;

						UnitTestUserNotification.Instance.ClearMessages();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

						TestTransactionModule.DisplayGrid.SelectAllElements(x => x.PK == invoice.PK || x.PK == creditNote.PK);

						menu.FindByText("Create Compliance Document Records").MenuItems.FindByText(isRollup ? "Roll-up by Charge Code" : "No Roll-up").PerformClick();

						AssertNotNull(invoice.GetTransactionGeneratedComplianceDocument());
						AssertNotNull(creditNote.GetTransactionGeneratedComplianceDocument());
						AssertContains(isRollup ? "Compliance Records created successfully with Roll-up by Charge Code" : "Compliance Records created successfully without Roll-up",
							UnitTestUserNotification.Instance.LastMessage.Text);

						assertAction();
					}
					else
					{
						Assert(true);
					}
				}
			}
		}

		#endregion

		#endregion

		#region EInvoicing test cases

		public virtual void TestResetStatusToQueuedMenuItem_IsOnlyVisibleIfEnableEInvoicingFunctionalityRegistryIsOn()
		{
			var registry = AccountingMasterFilesRegistry.Instance;
			Assert("Registry is off by default", !registry.EnableEInvoicingFunctionality.Value);
			Assert("Registry for AP is off by default", !registry.EnableEInvoicingFunctionalityForPayables.Value);
			var regFunctionality = ModuleID == ModuleIDs.APTransaction ? registry.EnableEInvoicingFunctionalityForPayables : registry.EnableEInvoicingFunctionality;

			var currCompany = GlbCompany.CurrentCompany;
			foreach (var country in new[] { CountryCodes.Italy, CountryCodes.Spain })
			{
				using (currCompany.TemporarilySetCountry(country))
				{
					using (regFunctionality.SetTemporaryValue(currCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
					using (var testModule = (TransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
					{
						Assert("Registry is off", !regFunctionality.Value);
						var resetStatusToQueuedMenuItem = testModule.GetNewActionMenuItems_ForTestOnly().FindByText(testModule.ResetStatusToQueuedText_ForTestOnly);
						AssertNull("'Reset Status to Queued' Menu Item should not be visible when 'Enable E-Reporting Functionality' registry is off", resetStatusToQueuedMenuItem);
					}

					using (regFunctionality.SetTemporaryValue(currCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					using (var testModule = (TransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
					{
						Assert("Registry is on", regFunctionality.Value);
						var resetStatusToQueuedMenuItem = testModule.GetNewActionMenuItems_ForTestOnly().FindByText(testModule.ResetStatusToQueuedText_ForTestOnly);
						AssertNotNull("'Reset Status to Queued' Menu Item should be visible when 'Enable E-Reporting Functionality' regtry is on", resetStatusToQueuedMenuItem);
					}
				}
			}
		}

		public virtual void TestResetStatusToQueuedMenuItem_IsOnlyAvailableOnForCountriesSupportingEInvoicing()
		{
			var eInvoicingMock = new Mock<IGlobalEInvoicingObjectFactory>();
			ObjectFactory.Substitute(eInvoicingMock.Object);

			AssertSupportingElectronicInvoicing(false, false, false);
			AssertSupportingElectronicInvoicing(false, true, false);
			AssertSupportingElectronicInvoicing(true, false, false);
			AssertSupportingElectronicInvoicing(true, true, true);

			void AssertSupportingElectronicInvoicing(bool expectedRegistry, bool doesCountrySupport, bool expectedResult)
			{
				eInvoicingMock.Setup(x => x.DoesCountrySupportElectronicInvoicing(It.IsAny<ZString>())).Returns(doesCountrySupport);

				var registry = AccountingMasterFilesRegistry.Instance;
				var regFunctionality = ModuleID == ModuleIDs.APTransaction ? registry.EnableEInvoicingFunctionalityForPayables : registry.EnableEInvoicingFunctionality;
				using (regFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, expectedRegistry))
				using (var testModule = (TransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
				{
					var resetStatusToQueuedMenuItem = testModule.GetNewActionMenuItems_ForTestOnly().FindByText(testModule.ResetStatusToQueuedText_ForTestOnly);
					if (expectedResult)
					{
						AssertNotNull("'Reset Status to Queued' Menu Item should be available for company.", resetStatusToQueuedMenuItem);
					}
					else
					{
						AssertNull("'Reset Status to Queued' Menu Item should not be available for company", resetStatusToQueuedMenuItem);
					}
				}
				eInvoicingMock.VerifyAll();
			}
		}

		[TestDate(2006, 5, 10)]
		public virtual void TestResetStatusToQueuedMenuItem_TransactionPivotStatusConcurrency_WithReloadingTransactionsInNewFactory()
		{
			if (ModuleID != ModuleIDs.ARTransaction)
			{
				Assert("TODO: test concurrency for not AR Transactions", true);
				return;
			}

			var registry = AccountingMasterFilesRegistry.Instance;
			var currCompany = GlbCompany.CurrentCompany;
			var currCompGuid = currCompany.PK.ToGuid();
			var today = ZDateTime.Today;

			var batchNum = 0;
			using (currCompany.TemporarilySetCountry(CountryCodes.Italy))
			using (registry.EReportingComplianceDate.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, today.AddDays(-10).ToDateTime()))
			using (registry.EnableEInvoicingFunctionality.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, true))
			{
				TestObjectCreator.CreateTestPeriods(today);
				var invoice1 = CreateInvoice();
				var invoice2 = CreateInvoice();
				Factory.Save();

				var creditNote1 = CreateReverseTransaction(invoice1);
				var creditNote2 = CreateReverseTransaction(invoice2);
				Factory.Save();

				AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
				AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);

				var batch1PK = TestObjectCreator.CreateEInvoicingBatch(++batchNum);
				var batch2PK = TestObjectCreator.CreateEInvoicingBatch(++batchNum);
				var batch3PK = TestObjectCreator.CreateEInvoicingBatch(++batchNum);
				var batch4PK = TestObjectCreator.CreateEInvoicingBatch(++batchNum);
				Factory.Save();

				var batch1 = Factory.Load<AccEInvoicingBatch>(batch1PK);
				var batch2 = Factory.Load<AccEInvoicingBatch>(batch2PK);
				var batch3 = Factory.Load<AccEInvoicingBatch>(batch3PK);
				var batch4 = Factory.Load<AccEInvoicingBatch>(batch4PK);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice1.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batch1PK, today, today, ZBool.True);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice2.PK, EInvoicingPivotState.BatchedWithError, "This transaction is batched with errors", batch2PK, today, today, ZBool.True);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(creditNote1.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batch3PK, today, today, ZBool.True, EInvoicingPivotActionType.Cancel);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(creditNote2.PK, EInvoicingPivotState.BatchedWithError, "This transaction is batched with errors", batch4PK, today, today, ZBool.True, EInvoicingPivotActionType.Cancel);

				AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batch1PK, today, today, ZBool.True);
				AssertPivotDetails(invoice2.PK, EInvoicingPivotState.BatchedWithError, "This transaction is batched with errors", batch2PK, today, today, ZBool.True);
				AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batch3PK, today, today, ZBool.True, EInvoicingPivotActionType.Cancel);
				AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.BatchedWithError, "This transaction is batched with errors", batch4PK, today, today, ZBool.True, EInvoicingPivotActionType.Cancel);

				using (var testModule1 = (TransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
				using (var form1 = new ZForm())
				{
					form1.Controls.Add(testModule1.EmbeddedControl);
					form1.Show();

					var resetStatusToQueuedMenuItemInModule1 = testModule1.GetNewActionMenuItems_ForTestOnly().FindByText(testModule1.ResetStatusToQueuedText_ForTestOnly);
					AssertNotNull("Reset Status to Queued Menu Item should exist", resetStatusToQueuedMenuItemInModule1);

					testModule1.PerformSearch_ForTest();
					var collection = testModule1.GridCollection as BusinessObjectCollection;
					collection.Load();
					AssertEquals($"{batchNum} transactions in the grid", batchNum, collection.Count);

					testModule1.DisplayGrid.SelectAllElements();

					using (var testModule2 = (TransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
					using (var form2 = new ZForm())
					{
						form2.Controls.Add(testModule2.EmbeddedControl);
						form2.Show();

						var resetStatusToQueuedMenuItemInModule2 = testModule2.GetNewActionMenuItems_ForTestOnly().FindByText(testModule2.ResetStatusToQueuedText_ForTestOnly);
						AssertNotNull("Reset Status to Queued Menu Item should exist", resetStatusToQueuedMenuItemInModule2);

						testModule2.PerformSearch_ForTest();
						collection = testModule2.GridCollection as BusinessObjectCollection;
						collection.Load();
						AssertEquals($"{batchNum} transactions in the grid", batchNum, collection.Count);

						testModule2.DisplayGrid.SelectAllElements();

						var userNotif2 = UnitTestUserNotification.Instance;
						userNotif2.ClearMessagesAndAnswers();
						resetStatusToQueuedMenuItemInModule2.PerformClick();
						AssertEquals("Eligible transactions were successfully re-queued.", userNotif2.LastMessage.Text);
						Assert(userNotif2.LastMessage.WasInformation);

						AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
						AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
						AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
						AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
					}

					var userNotif1 = UnitTestUserNotification.Instance;
					userNotif1.ClearMessagesAndAnswers();
					resetStatusToQueuedMenuItemInModule1.PerformClick();
					AssertMultilineASCIIEquals("Postcondition: second re-queue perform where is no eligible transaction to re-queue", @"Previously queued for e-Reporting transactions will be re-queued if they have the following statuses:

- 'FAL' - Fail, or
- 'BER' - Batched with errors, or
- 'SNT' - Sent (when you have appropriate security rights).

No eligible transactions found to be re-queued.", userNotif1.LastMessage.Text);
					Assert(userNotif1.LastMessage.WasError);

					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch1.AIB_Status);
					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch2.AIB_Status);
					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch3.AIB_Status);
					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch4.AIB_Status);
					AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
					AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
				}
			}
		}

		public void TestPenaltyTaxInfoActionMenuItemVisibility()
		{
			var expectedMenuItemTextForKoreaSouth = "Additional Tax Information";

			using (var testModule = (TransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			{
				using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
				{
					var expectedExists = ModuleID == ModuleIDs.ARTransaction;
					AssertEquals(expectedExists, testModule.GetNewActionMenuItems_ForTestOnly().Any(x => x.Text == expectedMenuItemTextForKoreaSouth));
				}

				using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, false))
				{
					AssertEquals(false, testModule.GetNewActionMenuItems_ForTestOnly().Any(x => x.Text == expectedMenuItemTextForKoreaSouth));
				}
			}
		}

		public virtual void TestSetStatusToAwaitMenuItem_IsOnlyVisibleIfEnableEInvoicingFunctionalityRegistryIsOn()
		{
			AssertIsOnlyVisibleIfEnableEInvoicingFunctionalityRegistryIsOn("Await Review", (module) => module.SetStatusToAwaitText_ForTestOnly);
		}

		[TestDate(2021, 11, 10)]
		public virtual void TestAuthorizeAndSendMenuItem_IsOnlyVisibleIfEnableEInvoicingFunctionalityRegistryIsOn()
		{
			AssertIsOnlyVisibleIfEnableEInvoicingFunctionalityRegistryIsOn("Authorize And Send", (module) => module.AuthorizeAndSendText_ForTestOnly);
		}

		void AssertIsOnlyVisibleIfEnableEInvoicingFunctionalityRegistryIsOn(string messageText, Func<TransactionModuleStrip, MultilingualString> getMenuItemText)
		{
			var registry = AccountingMasterFilesRegistry.Instance;

			Assert("Registry is off by default", !registry.EnableEInvoicingFunctionality.Value);
			Assert("Registry for AP is off by default", !registry.EnableEInvoicingFunctionalityForPayables.Value);
			using (var testModule = (TransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			{
				var menuText = "E-Reporting Authorization";

				var setStatusMenu = testModule.GetNewActionMenuItems_ForTestOnly().FindByText(menuText);
				AssertNull($"'{messageText}' menu should not be visible when 'Enable Functionality' registry is OFF", setStatusMenu);

				(var regFunctionality, var regComplianceDate, var regDefaultStatus) = GetRegistryItemsForEInvoicing();
				if (regFunctionality == null)
				{
					Assert("Test only applies to AR and AP modules", true);
					return;
				}

				var egCompany = TestObjectCreator.CreateCompanyAndBranch(CountryCodes.Egypt);
				var itCompany = TestObjectCreator.CreateCompanyAndBranch(CountryCodes.Italy);
				Factory.Save();

				using (TestObjectCreator.SwitchEnvToCompany(egCompany))
				using (regFunctionality.SetTemporaryValue(egCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					AssertNull($"'{messageText}' menu should not be visible when country is not Italy", setStatusMenu);
				}

				using (TestObjectCreator.SwitchEnvToCompany(itCompany))
				using (regFunctionality.SetTemporaryValue(itCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					setStatusMenu = testModule.GetNewActionMenuItems_ForTestOnly().FindByText(menuText);
					var menuItemText = getMenuItemText(testModule);
					MenuItem setStatusMenuItem = null;

					if (ModuleID == ModuleIDs.APTransaction)
					{
						AssertNotNull($"'{messageText}' menu should be visible when E-Reporting registry items are all OK", setStatusMenu);
						setStatusMenuItem = setStatusMenu.MenuItems.FindByText(menuItemText);
						AssertNotNull($"'{messageText}' menu item should be visible when E-Reporting registry items are all OK", setStatusMenuItem);
					}
					else if (ModuleID == ModuleIDs.ARTransaction)
					{
						AssertNull($"'{messageText}' menu should not be visible when 'E-Reporting Submit Pivot Default Status' is not PEN", setStatusMenu);

						using (regDefaultStatus?.SetTemporaryValue(itCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, EInvoicingPivotState.Pending))
						{
							setStatusMenu = testModule.GetNewActionMenuItems_ForTestOnly().FindByText(menuText);
							AssertNotNull($"'{messageText}' menu should be visible when E-Reporting registry items are all OK", setStatusMenu);
							setStatusMenuItem = setStatusMenu.MenuItems.FindByText(menuItemText);
							AssertNotNull($"'{messageText}' menu item should be visible when E-Reporting registry items are all OK", setStatusMenuItem);
						}
					}
				}
			}
		}

		[TestDate(2021, 11, 11)]
		public virtual void TestSetStatusToAwaitMenuItem_TransactionPivotStatusConcurrency_WithReloadingTransactionsInNewFactory()
		{
			string messageText = "Await Review";
			string okMessageText = "Transactions status were successfully set to Awaiting Review.";
			string errorMessageText = @"You can only Review transactions where the E-Reporting status is 'PEN' - Pending.
No transactions will be reviewed.";
			var infoMsg = $"Awaiting Review status set by user CWSupport on day {ZDateTime.Today}. To send the invoice, click 'Authorize and Send''";

			AssertTransactionPivotStatusConcurrency_WithReloadingTransactionsInNewFactory(messageText, okMessageText, errorMessageText, infoMsg, EInvoicingPivotState.AwaitingReview);
		}

		[TestDate(2021, 11, 10)]
		public virtual void TestAuthorizeAndSendMenuItem_TransactionPivotStatusConcurrency_WithReloadingTransactionsInNewFactory()
		{
			string okMessageText = "Transactions are now queued for sending.";
			string errorMessageText = @"You can only Authorize and Send transactions where the E-Reporting status is 'PEN - Pending' or 'AWA - Awaiting Review'.
No transactions will be set.";

			AssertTransactionPivotStatusConcurrency_WithReloadingTransactionsInNewFactory("Authorize And Send", okMessageText, errorMessageText, "", EInvoicingPivotState.Queued);
		}

		void AssertTransactionPivotStatusConcurrency_WithReloadingTransactionsInNewFactory(string messageText, string okMessageText, string errorMessageText, string infoMsg, ZString expectedPivotState)
		{
			var isStatusToAwaitTest = expectedPivotState == EInvoicingPivotState.AwaitingReview;

			var currDate = ZDateTime.Today;
			var currCompany = GlbCompany.CurrentCompany;
			var currCompGuid = currCompany.PK.ToGuid();

			(var regFunctionality, var regComplianceDate, var regDefaultStatus) = GetRegistryItemsForEInvoicing();
			if (regFunctionality == null)
			{
				Assert(true);
				return;
			}

			using (regFunctionality.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, true))
			using (regDefaultStatus?.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, EInvoicingPivotState.Pending))
			using (regComplianceDate?.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, currDate.ToDateTime().AddDays(-10)))
			using (currCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				TestObjectCreator.CreateTestPeriods(currDate);

				AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate.AT_Code = "Test";
				taxRate.AT_Type = AccTaxRate.Types.ReverseRated;
				Factory.Save();

				var invoice1 = CreateInvoice();
				var line1 = TestObjectCreator.CreateInvoiceLine(invoice1, TestObjectCreator.GLHeader1.PK, 100);
				line1.AL_AT = taxRate.PK;
				var invoice2 = CreateInvoice();
				var line2 = TestObjectCreator.CreateInvoiceLine(invoice2, TestObjectCreator.GLHeader1.PK, 200);
				line2.AL_AT = taxRate.PK;
				Factory.Save();

				Assert("Precondition: eInvoicing enabled", invoice1.IsEligibleToCreateEInvoicingTransactionPivot);
				Assert("Precondition: eInvoicing enabled", invoice2.IsEligibleToCreateEInvoicingTransactionPivot);
				AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Pending);
				AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Pending);

				using (var testModule1 = (TransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
				using (var form1 = new ZForm())
				{
					form1.Controls.Add(testModule1.EmbeddedControl);
					form1.Show();
					var userNotif = UnitTestUserNotification.Instance;
					var menuText = "E-Reporting Authorization";

					var subMenu = testModule1.GetNewActionMenuItems_ForTestOnly().FindByText(menuText);
					AssertNotNull($"'{messageText}' menu should exist", subMenu);
					var menuItemInModule1 = subMenu.MenuItems.FindByText(isStatusToAwaitTest ? testModule1.SetStatusToAwaitText_ForTestOnly : testModule1.AuthorizeAndSendText_ForTestOnly);
					AssertNotNull($"'{messageText}' menu item should exist", menuItemInModule1);

					testModule1.PerformSearch_ForTest();
					var collection = testModule1.GridCollection as BusinessObjectCollection;
					collection.Load();
					AssertEquals("2 transactions in the grid", 2, collection.Count);

					testModule1.DisplayGrid.SelectAllElements();

					using (var testModule2 = (TransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
					using (var form2 = new ZForm())
					{
						form2.Controls.Add(testModule2.EmbeddedControl);
						form2.Show();

						var subMenu2 = testModule2.GetNewActionMenuItems_ForTestOnly().FindByText(menuText);
						AssertNotNull($"'{messageText}' menu should exist", subMenu);
						var menuItemInModule2 = subMenu2.MenuItems.FindByText(isStatusToAwaitTest ? testModule2.SetStatusToAwaitText_ForTestOnly : testModule2.AuthorizeAndSendText_ForTestOnly);
						AssertNotNull($"'{messageText}' menu item should exist", menuItemInModule2);

						testModule2.PerformSearch_ForTest();
						collection = testModule2.GridCollection as BusinessObjectCollection;
						collection.Load();
						AssertEquals("2 transactions in the grid", 2, collection.Count);

						testModule2.DisplayGrid.SelectAllElements();

						userNotif.ClearMessagesAndAnswers();
						menuItemInModule2.PerformClick();
						AssertEquals(okMessageText, userNotif.LastMessage.Text);
						Assert(userNotif.LastMessage.WasInformation);

						AssertPivotDetails(invoice1.PK, expectedPivotState, infoMsg, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
						AssertPivotDetails(invoice2.PK, expectedPivotState, infoMsg, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					}

					userNotif.ClearMessagesAndAnswers();
					menuItemInModule1.PerformClick();
					AssertMultilineASCIIEquals(errorMessageText, userNotif.LastMessage.Text);
					Assert(userNotif.LastMessage.WasError);

					AssertPivotDetails(invoice1.PK, expectedPivotState, infoMsg, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(invoice2.PK, expectedPivotState, infoMsg, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				}
			}
		}

		protected void AssertPivotDetails(ZGuid parentPk, ZString expectedStatus, ZString expectedErrorDescription, ZGuid expectedBatchPK, ZDateTime expectedLastResponseReceived, ZDateTime expectedSentTime, ZBool expectedIsNotifiedByEmail, string actionType = EInvoicingPivotActionType.Submit)
		{
			var pivot = new BusinessObjectFactory().LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, parentPk).AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, actionType));
			AssertNotNull(pivot);
			AssertEquals(expectedStatus, pivot.AIP_Status);
			AssertEquals(expectedErrorDescription, pivot.AIP_ErrorDescription);
			AssertEquals(expectedBatchPK, pivot.AIP_AIB);
			AssertEquals(expectedLastResponseReceived, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals(expectedSentTime, pivot.AIP_LastSentTimeUtc);
			AssertEquals(expectedIsNotifiedByEmail, pivot.AIP_IsNotifiedByEmail);
		}

		protected void AssertPivotDetails(ZGuid parentPk, ZString expectedStatus)
		{
			AssertPivotDetails(parentPk, expectedStatus, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
		}

		(BooleanRegistryItem, DateTimeRegistryItem, CodePairRegistryItem) GetRegistryItemsForEInvoicing()
		{
			var registry = AccountingMasterFilesRegistry.Instance;
			BooleanRegistryItem regFunction = null;
			DateTimeRegistryItem regDate = null;
			CodePairRegistryItem regStatus = null;
			if (ModuleID == ModuleIDs.ARTransaction)
			{
				regFunction = registry.EnableEInvoicingFunctionality;
				regDate = registry.EReportingComplianceDate;
				regStatus = registry.EReportingSubmitPivotDefaultStatus;
			}
			else if (ModuleID == ModuleIDs.APTransaction)
			{
				regFunction = registry.EnableEInvoicingFunctionalityForPayables;
				regDate = registry.EReportingComplianceDateForPayables;
				regStatus = registry.EReportingSubmitPivotDefaultStatusForPayables;
			}
			return (regFunction, regDate, regStatus);
		}

		public virtual void TestSignElectronicInvoiceActionMenuItemVisibility()
		{
			var countriesQuery = new ZQuery();
			countriesQuery.OrderBy = RefCountrySchema.Constants.RN_Code;
			var countries = Factory.Load<RefCountry>(countriesQuery);

			foreach (var country in countries)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country.Code))
				{
					var actionMenuItems = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();
					var menuItem = actionMenuItems.FindByText("Sign Electronic Invoice");
					AssertNull("Sign Electronic Invoice menu item only exists on AR Transaction module", menuItem);
				}
			}
		}

		public void TestResetStatusToDeliveredMenuItem()
		{
			var enableEReportingReg = ModuleID == ModuleIDs.APTransaction
										? AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables
										: AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality;
			var complianceDateReg = ModuleID == ModuleIDs.APTransaction
										? AccountingMasterFilesRegistry.Instance.EReportingComplianceDateForPayables
										: AccountingMasterFilesRegistry.Instance.EReportingComplianceDate;

			using (complianceDateReg.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (enableEReportingReg.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var testModule = (TransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Romania))
			{
				var actionMenuItems = testModule.GetNewActionMenuItems_ForTestOnly();
				var menuItem = actionMenuItems.FindByText("Reset Status to Delivered");
				AssertResetStatusToDeliveredMenuItem(menuItem);
			}

			using (complianceDateReg.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (enableEReportingReg.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var testModule = (TransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var actionMenuItems = testModule.GetNewActionMenuItems_ForTestOnly();
				var menuItem = actionMenuItems.FindByText("Reset Status to Delivered");
				AssertNull("'Reset Status to Delivered' should NOT be shown in Company beyond Romania", menuItem);
			}
		}

		public virtual void AssertResetStatusToDeliveredMenuItem(MenuItem menuItem)
		{
			AssertNotNull("'Reset Status to Delivered' should be shown in Romania Company", menuItem);
		}

		public virtual void TestClickResetStatusToDelivered()
		{
			TestObjectCreator.CreateARInvoice<ARInvoice>("0001", TestObjectCreator.AUD, 0.5m, TestObjectCreator.Debtor);
			TestObjectCreator.CreateAPInvoice<APInvoice>("0002", TestObjectCreator.AUD, 1m, -100m, -5m, 0m, -100m, -5m, 0m);
			Factory.Save();

			var enableEReportingReg = ModuleID == ModuleIDs.APTransaction
							? AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables
							: AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality;
			var complianceDateReg = ModuleID == ModuleIDs.APTransaction
										? AccountingMasterFilesRegistry.Instance.EReportingComplianceDateForPayables
										: AccountingMasterFilesRegistry.Instance.EReportingComplianceDate;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Romania))
			using (complianceDateReg.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (enableEReportingReg.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var testModule = (TransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			using (var form = new ZForm())
			{
				form.Controls.Add(testModule.EmbeddedControl);
				form.Show();

				var actionMenuItems = testModule.GetNewActionMenuItems_ForTestOnly();
				var menuItem = actionMenuItems.FindByText("Reset Status to Delivered");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertEquals("Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);

				testModule.PerformSearch_ForTest();
				Application.DoEvents();

				testModule.DisplayGrid.SelectAllElements();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertContains("Previously delivered (DLV) e-Reporting transactions will be re-queued to delivered if they have the following statuses", UnitTestUserNotification.Instance.LastMessage.Text);

				form.Close();
			}
		}

		#endregion

		public void TestOverrideCashFlowCategoryMenuItem()
		{
			Menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();
			AssertNotNull("Menu item should exist", Menu.FindByText(TestTransactionModule.OverrideCashFlowCategoryMenuText_ForTestOnly));

			using (ZForm form1 = new ZForm())
			{
				form1.Controls.Add(TestTransactionModule.EmbeddedControl);
				form1.Show();
				TestTransactionModule.PerformSearch_ForTest();

				TestTransactionModule.ModifyCashFlowCategoryForPostedTransactionsSecurity_ForTestOnly.IsAllowed = false;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Menu.FindByText(TestTransactionModule.OverrideCashFlowCategoryMenuText_ForTestOnly).PerformClick();
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
				Assert("Security message should be shown to the user", UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));

				TestTransactionModule.ModifyCashFlowCategoryForPostedTransactionsSecurity_ForTestOnly.IsAllowed = true;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Menu.FindByText(TestTransactionModule.OverrideCashFlowCategoryMenuText_ForTestOnly).PerformClick();
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
				AssertEquals("The information should read as follows: ", "Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);

				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);
				Journal journal = CreateJournal();
				Factory.Save();

				TestTransactionModule.PerformSearch_ForTest();
				BusinessObjectCollection collection = TestTransactionModule.GridCollection as BusinessObjectCollection;
				collection.Load();
				AssertEquals("TestCollection should contain new transactions", 1, collection.Count);

				TestTransactionModule.DisplayGrid.SelectAllElements();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Menu.FindByText(TestTransactionModule.OverrideCashFlowCategoryMenuText_ForTestOnly).PerformClick();
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
				AssertEquals("The information should read as follows: ", "You can only override cash flow category for receipt or payment.", UnitTestUserNotification.Instance.LastMessage.Text);

				TestTransactionModule.DisplayGrid.UnSelectAll();

				Receipt receipt = CreateReceipt();
				Factory.Save();

				TestTransactionModule.PerformSearch_ForTest();
				collection.Load();
				AssertEquals("TestCollection should contain new transactions", 2, collection.Count);

				TestTransactionModule.DisplayGrid.SelectAllElements();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Menu.FindByText(TestTransactionModule.OverrideCashFlowCategoryMenuText_ForTestOnly).PerformClick();
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
				AssertEquals("The information should read as follows: ", "You can only override cash flow category for receipt or payment.", UnitTestUserNotification.Instance.LastMessage.Text);

				journal.DeleteFromDB();

				Payment payment = CreatePayment();
				Factory.Save();

				TestTransactionModule.PerformSearch_ForTest();

				collection.Load();
				AssertEquals("TestCollection should contain new transactions", 2, collection.Count);

				TestTransactionModule.DisplayGrid.SelectAllElements();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Menu.FindByText(TestTransactionModule.OverrideCashFlowCategoryMenuText_ForTestOnly).PerformClick();

				AssertType("LastFormShownDialogForTest", typeof(OverrideReceiptPaymentCashFlowCategoryForm), ZFormModaliser.LastFormShownForTest);

				OverrideReceiptPaymentCashFlowCategoryForm form = (OverrideReceiptPaymentCashFlowCategoryForm)ZFormModaliser.LastFormShownForTest;
				OverrideReceiptPaymentCashFlowCategoryHelper bizo = (OverrideReceiptPaymentCashFlowCategoryHelper)form.BusinessEntity;

				bizo.WrappedObjects[0].DisplayCashFlowCategoryOverride = (bizo.WrappedObjects[0] is Receipt) ? "F01" : "O01";
				bizo.WrappedObjects[1].DisplayCashFlowCategoryOverride = (bizo.WrappedObjects[1] is Receipt) ? "F01" : "O01";

				ContinueWithSave saveResult = form.FireSaveButton();

				AssertEquals("Precondition: form should be saved correctly", ContinueWithSave.Yes, saveResult);

				var newFactory = new BusinessObjectFactory();
				var receiptInNewFactory = newFactory.Load<ReceiptPaymentBase>(receipt.PK);
				var paymentInNewFactory = newFactory.Load<ReceiptPaymentBase>(payment.PK);

				AssertEquals("Overridden Cash Flow Category should be updated", "F01", receiptInNewFactory.DisplayCashFlowCategoryOverride);
				AssertEquals("Overridden Cash Flow Category should be updated", "O01", paymentInNewFactory.DisplayCashFlowCategoryOverride);
			}
		}

		public void TestOverrideTransactionDescription()
		{
			Menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();
			AssertNotNull("Menu item should exist", Menu.FindByText(TestTransactionModule.OverrideTransactionDescriptionMenuText_ForTestOnly));

			using (TransactionModuleStrip module = (TransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			{
				Menu = module.GetNewActionMenuItems_ForTestOnly();
				using (ZForm form1 = new ZForm())
				{
					form1.Controls.Add(module.EmbeddedControl);
					form1.Show();

					BusinessObjectCollection collection = module.GridCollection as BusinessObjectCollection;
					collection.Load();
					AssertEquals("TestCollection should be empty", 0, collection.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					Menu.FindByText(module.OverrideTransactionDescriptionMenuText_ForTestOnly).PerformClick();
					AssertEquals("Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNull("LastFormShownDialogForTest", ZFormModaliser.LastFormShownForTest);

					TestTransactionModule.ModifyTransactionDescriptionsSecurity_ForTestOnly.IsAllowed = false;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					Menu.FindByText(module.OverrideTransactionDescriptionMenuText_ForTestOnly).PerformClick();
					AssertEquals("Error " + TestTransactionModule.ModifyTransactionDescriptionsSecurity_ForTestOnly.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertNull("LastFormShownDialogForTest", ZFormModaliser.LastFormShownForTest);
					TestTransactionModule.ModifyTransactionDescriptionsSecurity_ForTestOnly.IsAllowed = true;

					TestObjectCreator.CreateTestPeriods(ZDateTime.Now);
					var invoice1 = this.CreateInvoice();
					var invoice2 = this.CreateInvoice();

					Factory.Save();
					module.PerformSearch_ForTest();
					collection.Load();
					AssertEquals("TestCollection should contain new invoices", 2, collection.Count);

					module.DisplayGrid.SelectAllElements();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					Menu.FindByText(module.OverrideTransactionDescriptionMenuText_ForTestOnly).PerformClick();
					AssertType("LastFormShownDialogForTest", typeof(OverrideTransactionDescriptionForm), ZFormModaliser.LastFormShownForTest);
					OverrideTransactionDescriptionForm form = (OverrideTransactionDescriptionForm)ZFormModaliser.LastFormShownForTest;
					OverrideTransactionDescriptionHelper bizo = (OverrideTransactionDescriptionHelper)form.BusinessEntity;
					bizo.WrappedObjects[0].AH_Desc = "My test description";
					bizo.WrappedObjects[1].AH_Desc = "My test description";
					ContinueWithSave saveResult = form.FireSaveButton();
					AssertEquals("Precondition: form should be saved correctly", ContinueWithSave.Yes, saveResult);
					AssertEquals("Should have created one invoice", "My test description", invoice1.AH_Desc);
					AssertEquals("Should have created one invoice", "My test description", invoice2.AH_Desc);
				}
			}
		}

		public void TestOverrideAddressContactMenuItem()
		{
			Menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();
			AssertNotNull("Menu item should exist", Menu.FindByText(TestTransactionModule.OverrideAddressContactMenuText_ForTestOnly));

			using (ZForm form1 = new ZForm())
			{
				form1.Controls.Add(TestTransactionModule.EmbeddedControl);
				form1.Show();
				TestTransactionModule.PerformSearch_ForTest();

				TestTransactionModule.ModifyAddressContactForPostedTransactionsSecurity_ForTestOnly.IsAllowed = false;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Menu.FindByText(TestTransactionModule.OverrideAddressContactMenuText_ForTestOnly).PerformClick();
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
				Assert("Security message should be shown to the user", UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));

				TestTransactionModule.ModifyAddressContactForPostedTransactionsSecurity_ForTestOnly.IsAllowed = true;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Menu.FindByText(TestTransactionModule.OverrideAddressContactMenuText_ForTestOnly).PerformClick();
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
				AssertEquals("The information should read as follows: ", "Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);

				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);

				Journal journal = CreateJournal();
				Factory.Save();

				TestTransactionModule.PerformSearch_ForTest();
				BusinessObjectCollection collection = TestTransactionModule.GridCollection as BusinessObjectCollection;
				collection.Load();
				AssertEquals("TestCollection should contain new invoices", 1, collection.Count);

				TestTransactionModule.DisplayGrid.SelectAllElements();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Menu.FindByText(TestTransactionModule.OverrideAddressContactMenuText_ForTestOnly).PerformClick();
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
				AssertEquals("The information should read as follows: ", "You can only override address and contact for Invoices, Credit Notes, Adjustment Notes and Payments.", UnitTestUserNotification.Instance.LastMessage.Text);

				TestTransactionModule.DisplayGrid.UnSelectAll();

				InvoicingBase invoice = CreateInvoice();
				invoice.AH_OH = TestObjectCreator.ABIGAS.PK;
				invoice.AH_OSTotalAmount = 100M;
				invoice.AH_TransactionNum = "INV001";

				JobCharge jobCharge = null;
				if (invoice is ARInvoice) //Attaching job to AR invoice only as we will be asserting JR_OA_SellInvoiceAddress and JR_OC_SellInvoiceContact
				{
					var shipment = TestObjectCreator.CreateShipment("S0034322");
					var job = TestObjectCreator.CreateJob(shipment, false, false);

					invoice.AH_JH = job.PK;
					var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, TestObjectCreator.FRT.PK);
					line.AL_JH = job.PK;
					jobCharge = TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.FRT, TestObjectCreator.AUD);
				}

				Factory.Save();

				TestTransactionModule.PerformSearch_ForTest();
				collection.Load();
				AssertEquals("TestCollection should contain new invoices", 2, collection.Count);

				TestTransactionModule.DisplayGrid.SelectAllElements();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Menu.FindByText(TestTransactionModule.OverrideAddressContactMenuText_ForTestOnly).PerformClick();
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
				AssertEquals("The information should read as follows: ", "You can only override address and contact for Invoices, Credit Notes, Adjustment Notes and Payments.", UnitTestUserNotification.Instance.LastMessage.Text);

				journal.DeleteFromDB();

				InvoicingBase creditNote = CreateCreditNote();
				creditNote.AH_OH = TestObjectCreator.ABIGAS.PK;
				creditNote.AH_OSTotalAmount = 100M;
				creditNote.AH_TransactionNum = "CRD001";

				Factory.Save();

				TestTransactionModule.PerformSearch_ForTest();

				collection.Load();
				AssertEquals("TestCollection should contain new invoices", 2, collection.Count);

				TestTransactionModule.DisplayGrid.SelectAllElements();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Menu.FindByText(TestTransactionModule.OverrideAddressContactMenuText_ForTestOnly).PerformClick();

				AssertType("LastFormShownDialogForTest", typeof(OverrideInvoiceAddressContactForm), ZFormModaliser.LastFormShownForTest);
				OverrideInvoiceAddressContactForm form = (OverrideInvoiceAddressContactForm)ZFormModaliser.LastFormShownForTest;
				OverrideInvoiceAddressContactHelper bizo = (OverrideInvoiceAddressContactHelper)form.BusinessEntity;

				var tableHints = GetTableHintsForOpeningAddressContactOverrideForm(invoice);
				// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
				AssertMaxDbHits(tableHints.Select(x => x.Value).Sum(), bizo.Factory);
				AssertEquals("2 business object in grid for modifying address and contact", 2, bizo.WrappedObjects.Count);

				var overrideAddress = TestObjectCreator.CreateAddress(TestObjectCreator.ABIGAS);
				var overrideContact = TestObjectCreator.CreateContact(TestObjectCreator.ABIGAS);

				Factory.Save();

				bizo.WrappedObjects[0].DisplayInvoiceAddressOverride = overrideAddress.PK;
				bizo.WrappedObjects[0].DisplayInvoiceContactOverride = overrideContact.PK;

				bizo.WrappedObjects[1].DisplayInvoiceAddressOverride = overrideAddress.PK;
				bizo.WrappedObjects[1].DisplayInvoiceContactOverride = overrideContact.PK;

				ContinueWithSave saveResult = form.FireSaveButton();

				AssertEquals("Precondition: form should be saved correctly", ContinueWithSave.Yes, saveResult);

				var newFactory = new BusinessObjectFactory();
				var invoiceInNewFactory = newFactory.Load<InvoicingBase>(invoice.PK);
				var creditNoteInNewFactory = newFactory.Load<InvoicingBase>(creditNote.PK);

				AssertEquals("Overridden address should be updated", overrideAddress.PK, invoiceInNewFactory.DisplayInvoiceAddressOverride);
				AssertEquals("Overridden contact should be updated", overrideContact.PK, invoiceInNewFactory.DisplayInvoiceContactOverride);

				AssertEquals("Overridden address should be updated", overrideAddress.PK, creditNoteInNewFactory.DisplayInvoiceAddressOverride);
				AssertEquals("Overridden contact should be updated", overrideContact.PK, creditNoteInNewFactory.DisplayInvoiceContactOverride);

				if (invoice is ARInvoice)
				{
					var jobChargeInNewFactory = newFactory.Load<JobCharge>(jobCharge.PK);
					AssertEquals("Job Charge override address is updated as well", invoiceInNewFactory.DisplayInvoiceAddressOverride, jobCharge.JR_OA_SellInvoiceAddress);
					AssertEquals("Job Charge override contact is updated as well", invoiceInNewFactory.DisplayInvoiceContactOverride, jobCharge.JR_OC_SellInvoiceContact);
				}
			}
		}

		public void TestOverrideAddressContactMenuItemWithPayment()
		{
			Menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();
			AssertNotNull("Menu item should exist", Menu.FindByText(TestTransactionModule.OverrideAddressContactMenuText_ForTestOnly));

			using (ZForm form1 = new ZForm())
			{
				form1.Controls.Add(TestTransactionModule.EmbeddedControl);
				form1.Show();
				TestTransactionModule.PerformSearch_ForTest();

				TestTransactionModule.ModifyAddressContactForPostedTransactionsSecurity_ForTestOnly.IsAllowed = true;

				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);

				TestTransactionModule.DisplayGrid.UnSelectAll();
				var collection = TestTransactionModule.GridCollection as BusinessObjectCollection;

				var payment = CreatePayment();
				payment.AH_OH = TestObjectCreator.ABIGAS.PK;

				Factory.Save();

				TestTransactionModule.PerformSearch_ForTest();
				collection.Load();
				AssertEquals("TestCollection should contain new payment", 1, collection.Count);

				TestTransactionModule.DisplayGrid.SelectAllElements();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Menu.FindByText(TestTransactionModule.OverrideAddressContactMenuText_ForTestOnly).PerformClick();

				AssertType("LastFormShownDialogForTest", typeof(OverrideInvoiceAddressContactForm), ZFormModaliser.LastFormShownForTest);
				OverrideInvoiceAddressContactForm form = (OverrideInvoiceAddressContactForm)ZFormModaliser.LastFormShownForTest;
				OverrideInvoiceAddressContactHelper bizo = (OverrideInvoiceAddressContactHelper)form.BusinessEntity;

				var overrideAddress = TestObjectCreator.CreateAddress(TestObjectCreator.ABIGAS);
				var overrideContact = TestObjectCreator.CreateContact(TestObjectCreator.ABIGAS);

				Factory.Save();

				bizo.WrappedObjects[0].DisplayInvoiceAddressOverride = overrideAddress.PK;
				bizo.WrappedObjects[0].DisplayInvoiceContactOverride = overrideContact.PK;

				ContinueWithSave saveResult = form.FireSaveButton();

				AssertEquals("Precondition: form should be saved correctly", ContinueWithSave.Yes, saveResult);

				var newFactory = new BusinessObjectFactory();
				var paymentInNewFactory = newFactory.Load<Payment>(payment.PK);

				AssertEquals("Overridden address should be updated", overrideAddress.PK, paymentInNewFactory.DisplayInvoiceAddressOverride);
				AssertEquals("Overridden contact should be updated", overrideContact.PK, paymentInNewFactory.DisplayInvoiceContactOverride);
			}
		}

		public void TestOverrideAgreedPaymentMethodAndDueDateMenuItem()
		{
			Menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();
			var menuItem = Menu.FindByText(TestTransactionModule.OverrideAgreedPaymentMethodMenuText_ForTestOnly);
			AssertNotNull("Menu item should exist", menuItem);

			using (var form1 = new ZForm())
			{
				var informationMessage = "You can only override agreed payment method or due date for Invoices, Credit Notes, Adjustment Notes and Journals.";
				var utun = UnitTestUserNotification.Instance;

				form1.Controls.Add(TestTransactionModule.EmbeddedControl);
				form1.Show();
				TestTransactionModule.PerformSearch_ForTest();

				TestTransactionModule.ModifyAgreedPaymentMethodForPostedTransactionsSecurity_ForTestOnly.IsAllowed = false;
				TestTransactionModule.ModifyDueDateForPostedTransactionsSecurity_ForTestOnly.IsAllowed = false;

				utun.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertNotNull(utun.LastMessage);
				Assert("Security message should be shown to the user", utun.LastMessage.Contains(SecurityCore.SecurityErrorMessage));

				TestTransactionModule.ModifyAgreedPaymentMethodForPostedTransactionsSecurity_ForTestOnly.IsAllowed = true;
				TestTransactionModule.ModifyDueDateForPostedTransactionsSecurity_ForTestOnly.IsAllowed = true;

				utun.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertNotNull(utun.LastMessage);
				AssertEquals("The information should read as follows: ", "Please select a record in the grid.", utun.LastMessage.Text);

				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);

				var payment = CreatePayment();

				Factory.Save();

				TestTransactionModule.PerformSearch_ForTest();
				var collection = TestTransactionModule.GridCollection as BusinessObjectCollection;
				collection.Load();
				AssertEquals("TestCollection should contain new invoices", 1, collection.Count);

				var grid = TestTransactionModule.DisplayGrid;
				grid.SelectAllElements();

				utun.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertNotNull(utun.LastMessage);
				AssertEquals("The information should read as follows: ", informationMessage, utun.LastMessage.Text);

				grid.UnSelectAll();

				var today = ZDateTime.Today;

				var journal = CreateJournal();
				journal.AH_OH = TestObjectCreator.TestOrganisation.PK;
				journal.AH_TransactionNum = "CRD0J1";
				journal.AH_DueDate = today;
				journal.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
				journal.AH_OSTotal = -500M;
				journal.AH_InvoiceAmount = -500M;
				journal.AH_OutstandingAmount = -500M;

				var invoice = CreateInvoice();
				invoice.AH_OH = TestObjectCreator.TestOrganisation.PK;
				invoice.AH_OSTotalAmount = 100M;
				invoice.AH_TransactionNum = "INV001";
				invoice.AH_DueDate = today;

				Factory.Save();

				TestTransactionModule.PerformSearch_ForTest();
				collection.Load();
				AssertEquals("TestCollection should contain new invoices", 3, collection.Count);

				grid.SelectAllElements();

				utun.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertNotNull(utun.LastMessage);
				AssertEquals("The information should read as follows: ", informationMessage, utun.LastMessage.Text);

				payment.DeleteFromDB();

				var creditNote = CreateCreditNote();
				creditNote.AH_OH = TestObjectCreator.TestOrganisation.PK;
				creditNote.AH_OSTotalAmount = 100M;
				creditNote.AH_TransactionNum = "CRD001";
				creditNote.AH_DueDate = today;

				Factory.Save();

				TestTransactionModule.PerformSearch_ForTest();

				collection.Load();
				AssertEquals("TestCollection should contain new invoices", 3, collection.Count);

				grid.SelectAllElements();

				utun.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertEquals("No warning message should appear", true, utun.LastMessage.WasNone);

				var lastForm = ZFormModaliser.LastFormShownForTest;
				AssertType("LastFormShownDialogForTest", typeof(OverrideTransactionAgreedPaymentMethodForm), lastForm);
				var form = (OverrideTransactionAgreedPaymentMethodForm)lastForm;
				var bizo = (OverrideTransactionAgreedPaymentMethodHelper)form.BusinessEntity;

				AssertEquals("3 business object in grid for modifying Agreed Payment Method", 3, bizo.WrappedObjects.Count);

				var invBaseObjs = bizo.WrappedObjects.Cast<TransactionHeader>();
				var invObj = invBaseObjs.First(x => x.AH_TransactionType == TransactionTypes.Invoice);
				var cnObj = invBaseObjs.First(x => x.AH_TransactionType == TransactionTypes.CreditNote);
				var jrnObj = invBaseObjs.First(x => x.AH_TransactionType == TransactionTypes.Journal);

				var tomorrow = today.AddDays(1);

				invObj.AH_AgreedPaymentMethodOverride = OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck;
				cnObj.AH_AgreedPaymentMethodOverride = OrgConstants.CreditAgreedPaymentMethods.Code.DebitCard;
				jrnObj.AH_AgreedPaymentMethodOverride = OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck;

				invObj.AH_DueDate = tomorrow;
				cnObj.AH_DueDate = tomorrow;
				jrnObj.AH_DueDate = tomorrow;

				ContinueWithSave saveResult = form.FireSaveButton();

				AssertEquals("Precondition: form should be saved correctly", ContinueWithSave.Yes, saveResult);

				var newFactory = new BusinessObjectFactory();
				var invoiceInNewFactory = newFactory.Load<InvoicingBase>(invoice.PK);
				var creditNoteInNewFactory = newFactory.Load<InvoicingBase>(creditNote.PK);
				var journalInNewFactory = newFactory.Load<TransactionHeader>(journal.PK);

				AssertEquals("Overridden Agreed Payment Method should be updated", OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck, invoiceInNewFactory.AH_AgreedPaymentMethodOverride);
				AssertEquals("Overridden Agreed Payment Method should be updated", OrgConstants.CreditAgreedPaymentMethods.Code.DebitCard, creditNoteInNewFactory.AH_AgreedPaymentMethodOverride);
				AssertEquals("Overridden Agreed Payment Method should be updated", OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck, journalInNewFactory.AH_AgreedPaymentMethodOverride);

				AssertEquals("Overridden Due Date should be updated", tomorrow, invoiceInNewFactory.AH_DueDate);
				AssertEquals("Overridden Due Date should be updated", tomorrow, creditNoteInNewFactory.AH_DueDate);
				AssertEquals("Overridden Due Date should be updated", tomorrow, journalInNewFactory.AH_DueDate);
			}
		}

		public void TestOverrideMatchStatusMenuItem()
		{
			Menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();
			AssertNotNull("Menu item should exist", Menu.FindByText(TestTransactionModule.OverrideMatchStatusMenuText_ForTestOnly.Caption));

			using (ZForm form1 = new ZForm())
			{
				form1.Controls.Add(TestTransactionModule.EmbeddedControl);
				form1.Show();
				TestTransactionModule.PerformSearch_ForTest();

				TestTransactionModule.ModifyMatchStatusAndReasonSecurity_ForTestOnly.IsAllowed = false;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Menu.FindByText(TestTransactionModule.OverrideMatchStatusMenuText_ForTestOnly.Caption).PerformClick();
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
				Assert("Security message should be shown to the user", UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));

				TestTransactionModule.ModifyMatchStatusAndReasonSecurity_ForTestOnly.IsAllowed = true;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Menu.FindByText(TestTransactionModule.OverrideMatchStatusMenuText_ForTestOnly.Caption).PerformClick();
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
				AssertEquals("The information should read as follows: ", "Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);

				var collection = TestTransactionModule.GridCollection as BusinessObjectCollection;
				var payment = CreatePayment();
				Factory.Save();

				TestTransactionModule.PerformSearch_ForTest();
				collection.Load();
				TestTransactionModule.DisplayGrid.SelectAllElements();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Menu.FindByText(TestTransactionModule.OverrideMatchStatusMenuText_ForTestOnly.Caption).PerformClick();

				AssertType("LastFormShownDialogForTest", typeof(OverrideMatchStatusForm), ZFormModaliser.LastFormShownForTest);

				var form = (OverrideMatchStatusForm)ZFormModaliser.LastFormShownForTest;
				var bizo = (OverrideMatchStatusHelper)form.BusinessEntity;

				bizo.WrappedObjects[0].AH_MatchStatus = "UAC";
				bizo.WrappedObjects[0].AH_MatchStatusReasonCode = "ADV";

				var saveResult = form.FireSaveButton();

				AssertEquals("Precondition: form should be saved correctly", ContinueWithSave.Yes, saveResult);

				var newFactory = new BusinessObjectFactory();
				var paymentInNewFactory = newFactory.Load<ReceiptPaymentBase>(payment.PK);
				AssertEquals("AH_MatchStatus should be updated", "UAC", paymentInNewFactory.AH_MatchStatus);
				AssertEquals("AH_MatchStatusReasonCode should be updated", "ADV", paymentInNewFactory.AH_MatchStatusReasonCode);
			}
		}
		public void TestUniversalCopyIsntAccessableInMenuForTM()
		{
			using (var form1 = new ZForm())
			{
				form1.Controls.Add(TestTransactionModule.EmbeddedControl);
				form1.Show();
				TestTransactionModule.PerformSearch_ForTest();
				AssertNull(TestTransactionModule.DisplayGrid.ContextMenu.MenuItems.FindByName("UniversalCopy"));
			}
		}

		Dictionary<string, int> GetTableHintsForOpeningAddressContactOverrideForm(InvoicingBase invoice)
		{
			var tableHints = new Dictionary<string, int>();
			tableHints.Add(AccTransactionHeader.Schema.TableName, 1);
			tableHints.Add(StmData.Schema.TableName, 1);
			tableHints.Add(OrgHeader.Schema.TableName, 1);
			tableHints.Add(OrgAddress.Schema.TableName, 1);
			tableHints.Add(OrgAddressCapability.Schema.TableName, 1);
#if WINZOR
			tableHints.Add(vw_AccTransactionHeaderTax.Schema.TableName, 1);
#endif
			if (invoice is ARInvoice)
			{
				tableHints.Add(JobHeader.Schema.TableName, 1);
			}
			/* THESE EXTRA DB HITS ARE SPARED AS TRANSACTIONHEADER.DefaultOrgAddressPK PROPERTY WAS UPDATED */
			//tableHints.Add(JobHeader.Schema.TableName, 1); 
			//tableHints.Add("ViewGenericJob", 1);
			//tableHints.Add("JobShipment", 1);
			//tableHints.Add(JobCharge.Schema.TableName, 1);
			//tableHints.Add("JobExRate", 1);
			//tableHints.Add(JobDocAddress.Schema.TableName, 1);
			//tableHints.Add("JobDeclaration", 2);
			//tableHints.Add("JobConShipLink", 1);
			//tableHints.Add("WhsDocketJobPivot", 1);
			//tableHints.Add(GlbBranch.Schema.TableName, 1);
			//tableHints.Add(GlbDepartment.Schema.TableName, 1);
			//tableHints.Add(GlbStaff.Schema.TableName, 1);

			return tableHints;
		}

		public void TestHandleSumSelectedTransactions()
		{
			if (TestTransactionModule.GetType() == typeof(APTransactionModuleStrip) || TestTransactionModule.GetType() == typeof(ARTransactionModuleStrip))
			{
				new AccountingPeriodTestHelper(Factory).SetupPeriods();

				Journal journal1 = CreateJournal();
				journal1.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
				journal1.AH_OSTotal = -500M;
				journal1.AH_InvoiceAmount = -500M;
				journal1.AH_OutstandingAmount = -500M;

				Journal journal2 = CreateJournal();
				journal2.AH_OH = journal1.AH_OH;

				Factory.Save();

				Menu = TestTransactionModule.GetNewAdditionalMenuItems_ForTestOnly();
				AssertNotNull("Menu item should exist", Menu.FindByText(TestTransactionModule.SumSelectedTransactionsMenuItemName_ForTestOnly));

				using (ZForm form = new ZForm())
				{
					form.Controls.Add(TestTransactionModule.EmbeddedControl);
					form.Show();
					TestTransactionModule.PerformSearch_ForTest();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					TestTransactionModule.HandleSumSelectedTransactions_ForTestOnly(this, EventArgs.Empty);

					AssertEquals("Please select transaction(s) to sum", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNull("LastFormShownDialogForTest", ZFormModaliser.LastFormShownForTest);

					TestTransactionModule.DisplayGrid.SelectAllElements();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					TestTransactionModule.HandleSumSelectedTransactions_ForTestOnly(this, EventArgs.Empty);

					AssertEquals("One or more transaction(s) has zero outstanding amount. Please re-select transactions.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNull("LastFormShownDialogForTest", ZFormModaliser.LastFormShownForTest);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					TestTransactionModule.DisplayGrid.UnSelect(GetRowIndex(TestTransactionModule.DisplayGrid, journal2.AH_TransactionNum));
					TestTransactionModule.HandleSumSelectedTransactions_ForTestOnly(this, EventArgs.Empty);

					AssertType("LastFormShownDialogForTest", typeof(TransactionCurrencySummaryForm), ZFormModaliser.LastFormShownForTest);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var newFactory = new BusinessObjectFactory();
					newFactory.RefreshEnabled = false;
					var journal = newFactory.Load<TransactionHeader>(journal1.PK);
					var link = ((IMatching)journal).CurrentMatchGroup.AddNew();
					link.AP_AH = journal.PK;
					link.AP_Amount = -500M;
					link.AP_MatchDate = ZDate.Today;
					journal.AH_OutstandingAmount = 0M;

					var headerToMatch = newFactory.NewWithValidTestData<AccTransactionHeader>();
					headerToMatch.AH_InvoiceAmount = 500M;
					var linkToMatch1 = ((IMatching)journal).CurrentMatchGroup.AddNew();
					linkToMatch1.AP_AH = headerToMatch.PK;
					linkToMatch1.AP_Amount = 500M;
					linkToMatch1.AP_MatchDate = ZDate.Today;
					newFactory.Save();

					TestTransactionModule.DisplayGrid.Select(GetRowIndex(TestTransactionModule.DisplayGrid, journal1.AH_TransactionNum));
					TestTransactionModule.HandleSumSelectedTransactions_ForTestOnly(this, EventArgs.Empty);
					AssertEquals("One or more transaction(s) has zero outstanding amount. Please re-select transactions.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			else
			{
				Assert(true);
			}
		}

		#region TestMatch

		public void TestMatchActionMenuItem()
		{
			AssertNotNull("Menu item should exist", Menu.FindByText("Match"));
		}

		public void TestMatchWithInvalidOrgHeader()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			Journal journal = CreateJournal();
			journal.AH_OH = Guid.Empty;
			journal.AH_OSTotal = 200M;
			journal.AH_InvoiceAmount = 200M;
			journal.AH_OutstandingAmount = 200M;

			Factory.Save();

			using (ZForm form = new ZForm())
			{
				form.Controls.Add(TestTransactionModule.EmbeddedControl);
				form.Show();
				TestTransactionModule.PerformSearch_ForTest();
				TestTransactionModule.DisplayGrid.SelectAllElements();
				TestTransactionModule.HandleMatch_ForTestOnly(this, EventArgs.Empty);

				AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
				AssertEquals("The transaction you are trying to match is missing an organization and cannot be matched.",
					UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestHandleMatchWithConcurrencyError()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			Journal journal1 = CreateJournal();
			journal1.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			journal1.AH_OSTotal = -500M;
			journal1.AH_InvoiceAmount = -500M;
			journal1.AH_OutstandingAmount = -500M;
			journal1.AH_Desc = "This is for matching";

			Journal journal2 = CreateJournal();
			journal2.AH_OH = journal1.AH_OH;
			journal2.AH_OSTotal = 200M;
			journal2.AH_InvoiceAmount = 200M;
			journal2.AH_OutstandingAmount = 200M;
			journal2.AH_Desc = "This is not for matching";

			Journal journal3 = CreateJournal();
			journal3.AH_OH = journal1.AH_OH;
			journal3.AH_OSTotal = 500M;
			journal3.AH_InvoiceAmount = 500M;
			journal3.AH_OutstandingAmount = 500M;
			journal3.AH_Desc = "This is for matching";

			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var journal1InNewFactory = newFactory.Load(journal1.TablePrefix, journal1.PK) as Journal;
			var journal3InNewFactory = newFactory.Load(journal1.TablePrefix, journal3.PK) as Journal;

			using (ZForm form = new ZForm())
			{
				form.Controls.Add(TestTransactionModule.EmbeddedControl);
				form.Show();

				TestTransactionModule.AddAdditionalDisplayFilter = null;
				TestTransactionModule.PerformSearch_ForTest();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull(TestTransactionModule.MatchingFormShown_ForTestOnly);
				TestTransactionModule.DisplayGrid.SelectAllElements();
				journal1InNewFactory.AH_Desc += " with an error";
				journal3InNewFactory.AH_Desc += " with an error";
				newFactory.Save();

				TestTransactionModule.HandleMatch_ForTestOnly(this, EventArgs.Empty);
				AssertNotNull("Since outstanding amount is not zero, matching should not be run and show a matching form instead", TestTransactionModule.MatchingFormShown_ForTestOnly);
				AssertEquals("Opening form controllerID unmatched", TestTransactionModule.ExpectedOpenedMatchFormID_ForTestOnly, TestTransactionModule.MatchingFormShown_ForTestOnly.ControllerID);
				Assert("For now , we should make sure it is a windows form implment", TestTransactionModule.MatchingFormShown_ForTestOnly is Form);
				Assert("Opening form should regist in global, avoiding some triky issue", OpenedFormCache.GetInstance().FormCache.Values.Contains((Form)TestTransactionModule.MatchingFormShown_ForTestOnly));
				((Form)TestTransactionModule.MatchingFormShown_ForTestOnly).Close();

				TestTransactionModule.AddAdditionalDisplayFilter = result =>
				{
					var query = new ZQuery(AccTransactionHeaderSchema.AH_Desc, "This is for matching with an error");
					result.AddToFilter(query);
				};
				TestTransactionModule.PerformSearch_ForTest();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				TestTransactionModule.DisplayGrid.SelectAllElements();
				journal1InNewFactory.AH_Desc += " with another error";
				journal3InNewFactory.AH_Desc += " with another error";
				newFactory.Save();

				TestTransactionModule.HandleMatch_ForTestOnly(this, EventArgs.Empty);
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
				AssertEquals("Due to a concurrency error, matching failed when saving, so manual matching is not attempted and an error is shown",
					"This match could not be saved due to another user accessing transactions included in the match. Please close and re-open the module before trying to match again.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestHandleMatch()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			Journal journal1 = CreateJournal();
			journal1.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			journal1.AH_OSTotal = -500M;
			journal1.AH_InvoiceAmount = -500M;
			journal1.AH_OutstandingAmount = -500M;

			Journal journal2 = CreateJournal();
			journal2.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			journal2.AH_OSTotal = 200M;
			journal2.AH_InvoiceAmount = 200M;
			journal2.AH_OutstandingAmount = 200M;

			Journal journal3 = CreateJournal();
			journal3.AH_OH = journal1.AH_OH;

			Journal journal4 = CreateJournal();
			journal4.AH_OH = journal1.AH_OH;
			journal4.AH_OSTotal = 500M;
			journal4.AH_InvoiceAmount = 500M;
			journal4.AH_OutstandingAmount = 500M;

			Factory.Save();

			using (ZForm form = new ZForm())
			{
				form.Controls.Add(TestTransactionModule.EmbeddedControl);
				form.Show();
				TestTransactionModule.PerformSearch_ForTest();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				TestTransactionModule.HandleMatch_ForTestOnly(this, EventArgs.Empty);
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
				AssertEquals("The information should read as follows: ", "Please select transaction(s) to match", UnitTestUserNotification.Instance.LastMessage.Text);

				TestTransactionModule.MatchCheckpoint_ForTestOnly.IsAllowed = false;
				TestTransactionModule.DisplayGrid.SelectAllElements();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				TestTransactionModule.HandleMatch_ForTestOnly(this, EventArgs.Empty);
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
				Assert("The information should read as follows: ",
					UnitTestUserNotification.Instance.LastMessage.Text.Contains("You do not have the appropriate security rights to run this function."));

				TestTransactionModule.MatchCheckpoint_ForTestOnly.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				TestTransactionModule.HandleMatch_ForTestOnly(this, EventArgs.Empty);
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
				AssertEquals("The information should read as follows: ", "One or more transaction(s) has zero outstanding amount. Matching process terminated",
					UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				TestTransactionModule.DisplayGrid.UnSelect(GetRowIndex(TestTransactionModule.DisplayGrid, journal3.AH_TransactionNum));
				TestTransactionModule.HandleMatch_ForTestOnly(this, EventArgs.Empty);
				AssertNotNull(ZFormModaliser.LastFormShownDialogForTest);
				Assert(ZFormModaliser.LastFormShownDialogForTest is PrimaryOrgSelectorForm);

				TestTransactionModule.DisplayGrid.UnSelect(GetRowIndex(TestTransactionModule.DisplayGrid, journal2.AH_TransactionNum));
				TestTransactionModule.HandleMatch_ForTestOnly(this, EventArgs.Empty);
				AssertEquals("The information should read as follows: ", "Transactions Matched Successfully: Match Group Number M00001000", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		ZInt GetRowIndex(ZFilterGrid grid, ZString transactionNumber)
		{
			for (int i = 0; i < grid.VisibleRowCount; i++)
			{
				if ((ZString)grid[i, 3] == transactionNumber)
				{
					return i;
				}
			}
			return -1;
		}

		protected abstract Journal CreateJournal();
		protected abstract InvoicingBase CreateInvoice();
		protected abstract InvoicingBase CreateCreditNote();
		protected abstract Receipt CreateReceipt();
		protected abstract Payment CreatePayment();
		InvoicingBase CreateReverseTransaction(InvoicingBase invoice)
		{
			var creditNote = CreateCreditNote();
			creditNote.OriginalTransaction = invoice;
			creditNote.AH_TransactionBelongsToGroup = invoice.PK;
			creditNote.IsReverseTransaction = true;

			AccTransactionMatchLink link1 = Factory.New<AccTransactionMatchLink>();

			link1.AP_AH = invoice.PK;
			link1.AP_Amount = invoice.AH_OutstandingAmount;
			link1.AP_MatchDate = invoice.AH_PostDate;
			link1.AP_MatchGroupNum = "100100";

			TransactionMatchLinkGroup matchlinks = new TransactionMatchLinkGroup(invoice.Factory);
			matchlinks.Add(link1);
			return creditNote;
		}

		#endregion

		protected string GetExpectedPrintErrorMessage(ZString transactionNumber, ZString transactionType, bool isInactiveOrg)
		{
			ZStringBuilder expectedMessage = new ZStringBuilder("The following transaction(s) cannot be printed\r\n");
			if (isInactiveOrg)
			{
				expectedMessage.AppendLine(string.Format("Transaction {0} cannot be printed because it is for an inactive organization", transactionNumber));
			}
			else
			{
				expectedMessage.AppendLine(string.Format("Transaction {0} cannot be printed because it is {1}", transactionNumber, transactionType));
			}

			return expectedMessage.ToString();
		}

		public void TestGetNewController()
		{
			ZController invoiceController = TestTransactionModule.GetNewController_ForTestOnly(GetInvoiceToTestControllerID());
			AssertEquals("Invoice Controller should be for invoices", InvoiceControllerID, invoiceController.ID);

			invoiceController = TestTransactionModule.GetNewController_ForTestOnly(null);
			AssertEquals("Invoice Controller should be for invoices", InvoiceControllerID, invoiceController.ID);
		}

		public void TestGetMiscellaneousTransactionController()
		{
			ZController controller = TestTransactionModule.ControllerFromTransactionType_ForTestOnly(ZArchitecture.Core.TransactionTypes.Discount);
			AssertEquals("Controller is for discounts", DiscountControllerID, controller.ID);

			controller = TestTransactionModule.ControllerFromTransactionType_ForTestOnly(ZArchitecture.Core.TransactionTypes.ExchangeDifference);
			AssertEquals("Controller is for Ex Differences", ExchangeDifferenceControllerID, controller.ID);

			controller = TestTransactionModule.ControllerFromTransactionType_ForTestOnly(ZArchitecture.Core.TransactionTypes.Overpayment);
			AssertEquals("Controller is for OVerpayments", OverpaymentControllerID, controller.ID);
		}

		[ExpectNoExceptions()]
		public void TestPopupModuleDoesNotBlowUp()
		{
			using (var findBox = new ZCodeFindBox())
			{
				IModuleDecisionProvider provider = new ZArchitecture.Modules.Internal.PopupModuleDecisionProvider(findBox);
				fTransactionModule.OverrideModuleDecisionProvider(provider);

				using (var modulePopup = new ZArchitecture.GUI.Internal.EmbeddedModulePopup(fTransactionModule))
				{
					modulePopup.Show();
				}
			}
		}

		#region TestPrintingToolBarButtons

		public virtual void TestPrintingToolBarButtons()
		{
			ToolBarButton[] buttons = TestTransactionModule.ToolBarButtons;
			ToolBarButton printButton = buttons.FindByText("Print");
			AssertNotNull("There should be a Button named Print", printButton);
			AssertNotNull("The Print button should have a dropdown menu", printButton.DropDownMenu);
			AssertEquals("There should be 3 items in the menuItem Array", 3, printButton.DropDownMenu.MenuItems.Count);
			AssertNotNull("There should be a suboption called 'PrintTransaction'", printButton.DropDownMenu.MenuItems.FindByText(TestTransactionModule.PrintTransactionMenuText_ForTestOnly));
			AssertNotNull("There should be a suboption called 'PrintMatchDoc'", printButton.DropDownMenu.MenuItems.FindByText(TestTransactionModule.PrintMatchDocMenuText_ForTestOnly));
			AssertNotNull("There should be a suboption called 'Print Accounting Journal'", printButton.DropDownMenu.MenuItems.FindByText("Print Accounting Journal"));
		}

		public virtual void TestChinaHasPrintAccountingVoucherMenuItem()
		{
			string oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
				ToolBarButton[] buttons = TestTransactionModule.ToolBarButtons;
				ToolBarButton printButton = buttons.FindByText("Print");
				var menuItems = printButton.DropDownMenu.MenuItems;
				AssertNotNull("There should be a suboption called 'PrintAccountingVoucher'", menuItems.FindByText(TestTransactionModule.PrintAccountingVoucherText_ForTestOnly));
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}
		}

		public virtual void TestTaiwanHasPrintAccountingVoucherMenuItem()
		{
			string oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
				ToolBarButton[] buttons = TestTransactionModule.ToolBarButtons;
				ToolBarButton printButton = buttons.FindByText("Print");
				var menuItems = printButton.DropDownMenu.MenuItems;
				AssertNotNull("There should be a suboption called 'PrintAccountingVoucher'", menuItems.FindByText(TestTransactionModule.PrintAccountingVoucherText_ForTestOnly));
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}
		}
		#endregion

		#region TestInactiveOrgDoesNotAllowReversing

		public void TestInactiveOrgDoesNotAllowReversing()
		{
			ZForm reverseForm = null;
			using (ZForm parentForm = new ZForm())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				try
				{
					TestTransactionModule.SetFormsModalTo(parentForm);

					InactiveOrg = GetInactiveOrg();
					SetupTransactionFilter();
					APInv = GetAPInvoiceWithInactiveOrg();
					ARInv = GetARInvoiceWithInactiveOrg();

					Assert("Precondition: no messages shown", UnitTestUserNotification.Instance.LastMessage.WasNone);
					TestTransactionModule.ShowDeleteForm_ForTestOnly(GetExistingInvoiceForTest);

					reverseForm = (ZForm)ZFormModaliser.ActiveForm;

					Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals("The information should read as follows: ", "This transaction cannot be reversed because it is for an inactive organization",
						UnitTestUserNotification.Instance.LastMessage.Text);

					InactiveOrg.OH_IsActive = true;
					Factory.Save();
					TestTransactionModule.ShowDeleteForm_ForTestOnly(GetExistingInvoiceForTest);

					reverseForm = (ZForm)ZFormModaliser.ActiveForm;
					AssertNotNull("ReversingForm should pop up", reverseForm);
					Assert("ReverseForm should be the CreditNoteForm", reverseForm is CreditNoteForm);
				}
				finally
				{
					if (reverseForm != null)
					{
						reverseForm.Dispose();
					}
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}
		}

		#endregion

		#region TestInactiveDeptDoesNotAllowCopy

		public void TestInactiveDeptDoesNotAllowCopy()
		{
			ZForm copyForm = null;

			using (ZForm parentForm = new ZForm())
			{
				try
				{
					TestTransactionModule.SetFormsModalTo(parentForm);
					InactiveDept = GetTestDept();
					GlbDepartment inactiveDeptLine1 = GetTestDept();
					GlbDepartment inactiveDeptLine2 = GetTestDept();
					InactiveOrg = GetInactiveOrg();

					SetupTransactionFilter();
					APInv = GetAPInvoiceWithInactiveOrg(false);
					APInv.Lines[0].AL_GE = inactiveDeptLine1.PK;
					InvoicingLineBase aPInvLine = TestObjectCreator.CreateInvoiceLine(APInv, APInv.TransactionCurrency, APInv.AH_ExchangeRate, 80m, 0m, 0m, 100m, 0m, 0m);
					ARInv = GetARInvoiceWithInactiveOrg(false);
					ARInv.Lines[0].AL_GE = inactiveDeptLine1.PK;
					InvoicingLineBase aRInvLine = TestObjectCreator.CreateInvoiceLine(ARInv, ARInv.TransactionCurrency, ARInv.AH_ExchangeRate, 80m, 0m, 0m, 100m, 0m, 0m);
					Factory.Save();

					Assert("Precondition: no messages shown", UnitTestUserNotification.Instance.LastMessage.WasNone);
					APInv.AH_GE = InactiveDept.PK;
					ARInv.AH_GE = InactiveDept.PK;
					aPInvLine.AL_GE = inactiveDeptLine2.PK;
					aRInvLine.AL_GE = inactiveDeptLine2.PK;
					InactiveOrg.OH_IsActive = true;

					InactiveDept.GE_IsActive = false;
					inactiveDeptLine1.GE_IsActive = true;
					inactiveDeptLine2.GE_IsActive = true; //two Transaction lines is true
					TestTransactionModule.ShowTemplateCopyForm_ForTestOnly(GetExistingInvoiceForTest);

					Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
					AssertEquals("The information should read as follows: ", TransactionModuleStrip.CopyInactiveDepartment,
														UnitTestUserNotification.Instance.LastMessage.Text);

					InactiveDept.GE_IsActive = true;
					inactiveDeptLine1.GE_IsActive = true;
					inactiveDeptLine2.GE_IsActive = true;
					TestTransactionModule.ShowTemplateCopyForm_ForTestOnly(GetExistingInvoiceForTest);

					copyForm = (ZForm)ZFormModaliser.ActiveForm;
					AssertNotNull("Copy form should pop up", copyForm);
					Assert("CopyForm should be the InvoiceForm", copyForm is InvoiceForm);

					InactiveDept.GE_IsActive = true;
					inactiveDeptLine1.GE_IsActive = true;
					inactiveDeptLine2.GE_IsActive = false; //Header is true, one line true, another line false.
					TestTransactionModule.ShowTemplateCopyForm_ForTestOnly(GetExistingInvoiceForTest);

					Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
					AssertEquals("The information should read as follows: ", TransactionModuleStrip.CopyInactiveLineDepartmentFromHeader,
														UnitTestUserNotification.Instance.LastMessage.Text);
				}
				finally
				{
					if (copyForm != null)
					{
						copyForm.Dispose();
					}
				}
			}
		}
		#endregion

		#region TestInactiveOrgDoesNotAllowCopy

		public void TestInactiveOrgDoesNotAllowCopy()
		{
			ZForm copyForm = null;
			using (ZForm parentForm = new ZForm())
			{
				try
				{
					TestTransactionModule.SetFormsModalTo(parentForm);
					InactiveOrg = GetInactiveOrg();
					SetupTransactionFilter();
					APInv = GetAPInvoiceWithInactiveOrg();
					ARInv = GetARInvoiceWithInactiveOrg();

					Assert("Precondition: no messages shown", UnitTestUserNotification.Instance.LastMessage.WasNone);
					TestTransactionModule.ShowTemplateCopyForm_ForTestOnly(GetExistingInvoiceForTest);

					copyForm = (ZForm)ZFormModaliser.ActiveForm;

					Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.WasInformation);
					AssertEquals("The information should read as follows: ", "This transaction cannot be copied because it is for an inactive organization",
						UnitTestUserNotification.Instance.LastMessage.Text);

					InactiveOrg.OH_IsActive = true;
					TestTransactionModule.ShowTemplateCopyForm_ForTestOnly(GetExistingInvoiceForTest);

					copyForm = (ZForm)ZFormModaliser.ActiveForm;
					AssertNotNull("Copy form should pop up", copyForm);
					Assert("CopyForm should be the InvoiceForm", copyForm is InvoiceForm);
				}
				finally
				{
					if (copyForm != null)
					{
						copyForm.Dispose();
					}
				}
			}
		}

		#endregion

		#region TestInactiveOrgDoesNotAllowPrinting

		public void TestInactiveOrgDoesNotAllowPrinting()
		{
			InactiveOrg = GetInactiveOrg();
			SetupTransactionFilter();
			APInv = GetAPInvoiceWithInactiveOrg();
			ARInv = GetARInvoiceWithInactiveOrg();

			Assert("Precondition: no messages shown", UnitTestUserNotification.Instance.LastMessage.WasNone);
			using (ZForm form = new ZForm())
			{
				form.Controls.Add(TestTransactionModule.EmbeddedControl);
				form.Show();
				TestTransactionModule.PerformSearch_ForTest();
				TestTransactionModule.HandlePrint_ForTestOnly(this, EventArgs.Empty);
				Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("The information should read as follows: ",
					GetExpectedPrintErrorMessage(GetModuleID().Name == "AREnquiry" || GetModuleID().Name == "ARTransaction" ? ARInv.AH_TransactionNum : APInv.AH_TransactionNum, "", true),
					UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region TestInactiveOrgDoesNotAllowPrintMatchReport

		public void TestInactiveOrgDoesNotAllowPrintMatchReport()
		{
			InactiveOrg = GetInactiveOrg();
			SetupTransactionFilter();
			APInv = GetAPInvoiceWithInactiveOrg();
			ARInv = GetARInvoiceWithInactiveOrg();

			Assert("Precondition: no messages shown", UnitTestUserNotification.Instance.LastMessage.WasNone);
			using (ZForm form = new ZForm())
			{
				form.Controls.Add(TestTransactionModule.EmbeddedControl);
				form.Show();
				TestTransactionModule.PerformSearch_ForTest();
				TestTransactionModule.HandlePrintMatchingReport_ForTestOnly(this, EventArgs.Empty);
				Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("The information should read as follows: ", "The match report for this transaction cannot be printed because the organization is inactive",
					UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region TestPopupModuleHasCorrectBindToLists

		[ExpectNoExceptions]
		public void TestPopupModuleHasCorrectBindToLists()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			AccTransactionHeader aRInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			aRInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			aRInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			aRInvoice.AH_OH = testOrg.PK;
			aRInvoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			aRInvoice.AH_GB = GlbBranch.CurrentBranch.PK;

			AccTransactionHeader aRInvoice2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			aRInvoice2.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			aRInvoice2.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			aRInvoice2.AH_OH = testOrg.PK;
			aRInvoice2.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			aRInvoice2.AH_GB = GlbBranch.CurrentBranch.PK;

			Factory.Save();

			using (ZFilterGridModule queryClaimModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.ARAccQueryClaim))
			{
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(queryClaimModule.EmbeddedControl);
					form.Show();
					form.Size = new System.Drawing.Size(1024, 768);
					((IFilterGridModuleInternalsForTesting)queryClaimModule).PerformSearch();

					foreach (Control testControl in form.Controls)
					{
						if (testControl is ZFilterStripControl)
						{
							foreach (Control subCtrl in (testControl as ZFilterStripControl).Controls)
							{
								if (subCtrl is ZPanel)
								{
									foreach (Control panelCtrl in (subCtrl as ZPanel).Controls)
									{
										if (panelCtrl is ZArchitecture.GUI.Internal.ZPopupFindBox && panelCtrl.Name == "InvoiceGuidFindBox")
										{
											(panelCtrl as ZArchitecture.GUI.Internal.ZPopupFindBox).SelectFromPopupForm();
											Form popupForm = (Form)panelCtrl.GetType().GetProperty("PopupForm", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(panelCtrl, null);
											popupForm.Size = new System.Drawing.Size(1024, 768);
											popupForm.Refresh();
											popupForm.Update();
											popupForm.Dispose();
										}
									}
								}
							}
						}
					}
				}
			}
		}

		#endregion

		#region TestPerformSearch
		[ExpectNoExceptions()]
		public virtual void TestPerformSearch()
		{
			if (TestTransactionModule.FilterBusinessObject is ARTransactionFilterStripBusinessObject ||
									TestTransactionModule.FilterBusinessObject is APTransactionFilterStripBusinessObject)
			{
				TransactionFilterStripBusinessObject testFilterBizO = (TransactionFilterStripBusinessObject)TestTransactionModule.FilterBusinessObject;

				ModuleTextFilter transactionNumberFilter = ((ModuleTextFilter)testFilterBizO[Business.AccountingUtils.NumberFilterTypes.TransactionNumber]);
				transactionNumberFilter.IsActive = true;
				transactionNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				TestTransactionModule.PerformSearch_ForTest();
				AssertNull("LastMessage", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region TurkeyEInvoiceTests

		public void TestRequestEInvoicePDFCopyMenuItemIsExistForTurkeyCompanyARAPTransactions()
		{
			var menuItemDesc = "Request e-Invoice PDF Copy";
			var turkeyBranch = TestObjectCreator.CreateBranchWithCompany("TRIST");
			var afghanistanBranch = TestObjectCreator.CreateBranchWithCompany("AFBIN");

			Factory.Save();

			fTransactionModule.Dispose();

			AssertRequestMenuItemForTurkey(menuItemDesc, turkeyBranch.PK.ToGuid(), " button should be in Action menu when logged in with a Turkey company and Turkey Compliance Feature is active", DateTime.Today.AddDays(-1));
			AssertRequestMenuItemForTurkey(menuItemDesc, turkeyBranch.PK.ToGuid(), " menu item should not be present when Turkey Compliance Features TestTransactionModule is disabled.", null, false);
			AssertRequestMenuItemForTurkey(menuItemDesc, afghanistanBranch.PK.ToGuid(), " menu item should not be present in Afghanistan (because there are NO sub-types configured for it)", null, false);
			AssertRequestMenuItemForTurkey(menuItemDesc, afghanistanBranch.PK.ToGuid(), " menu item should not be present in Afghanistan (because there are NO sub-types configured for it)", DateTime.Today.AddDays(-1), false);
		}

		public void TestRequestEInvoiceTransactionStatusUpdateMenuItemIsExistForTurkeyCompanyARAPTransactions()
		{
			var menuItemDesc = "Request e-Invoice Transaction Status Update";
			var turkeyBranch = TestObjectCreator.CreateBranchWithCompany("TRIST");
			var afghanistanBranch = TestObjectCreator.CreateBranchWithCompany("AFBIN");

			Factory.Save();

			fTransactionModule.Dispose();

			AssertRequestMenuItemForTurkey(menuItemDesc, turkeyBranch.PK.ToGuid(), " button should be in Action menu when logged in with a Turkey company and Turkey Compliance Feature is active", DateTime.Today.AddDays(-1));
			AssertRequestMenuItemForTurkey(menuItemDesc, turkeyBranch.PK.ToGuid(), " menu item should not be present when Turkey Compliance Features TestTransactionModule is disabled.", null, false);
			AssertRequestMenuItemForTurkey(menuItemDesc, afghanistanBranch.PK.ToGuid(), " menu item should not be present in Afghanistan (because there are NO sub-types configured for it)", null, false);
			AssertRequestMenuItemForTurkey(menuItemDesc, afghanistanBranch.PK.ToGuid(), " menu item should not be present in Afghanistan (because there are NO sub-types configured for it)", DateTime.Today.AddDays(-1), false);
		}

		void AssertRequestMenuItemForTurkey(string menuItemDesc, Guid branchPk, string assertMessage, DateTime? date, bool assertValue = true)
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingTurkey_Receivables(branchPk, date))
			using (fTransactionModule = new ARTransactionModuleStrip())
			{
				Menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();

				var requestMenuItem = Menu.FindByText(menuItemDesc);
				AssertEquals(menuItemDesc + assertMessage, assertValue, requestMenuItem != null);
			}

			using (TestObjectCreator.SetUpForTestingEInvoicingTurkey_Payables(branchPk, date))
			using (fTransactionModule = new APTransactionModuleStrip())
			{
				Menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();

				var requestMenuItem = Menu.FindByText(menuItemDesc);
				AssertEquals(menuItemDesc + assertMessage, assertValue, requestMenuItem != null);
			}
		}

		#endregion

		public void TestComplianceSubTypeAndNumberAllocationMenuItem()
		{
			var italyCompany = TestObjectCreator.CreateNewCompany("ITL", Core.Constants.CountryCodes.Italy);
			var italyBranch = TestObjectCreator.CreateNewBranch(italyCompany, "ITL");
			OrgHeader italyCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("ITORGPROXY", true, true, "ITMIL");
			italyCompany.GC_OH_OrgProxy = italyCompanyOrgProxy.PK;

			var afghanistanCompany = TestObjectCreator.CreateNewCompany("AFG", Core.Constants.CountryCodes.Afghanistan);
			var afghanistanBranch = TestObjectCreator.CreateNewBranch(afghanistanCompany, "AFB");
			OrgHeader afghanistanCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("AFGORGPROX", true, true, "AFBIN");
			afghanistanCompany.GC_OH_OrgProxy = afghanistanCompanyOrgProxy.PK;

			Factory.Save();

			fTransactionModule.Dispose();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), italyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (fTransactionModule = (TransactionModuleStrip)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				Menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();

				MenuItem allocateComplianceInfoMenuItem = Menu.FindByText("Update Compliance Sub Type and/or Number");
				AssertNotNull("Compliance allocation menu item should be present in Italy (because there are sub-types configured for it and Compliance Document module disabled.)", allocateComplianceInfoMenuItem);
				using (ZForm form = new ZForm())
				{
					var arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "1", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
					arInvoice1.AH_OH = TestObjectCreator.AALSHI.PK;
					arInvoice1.AH_ComplianceSubType = ItalyComplianceInfo.ComplianceSubTypeCodes.ARI;
					var arInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "2", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
					arInvoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
					arInvoice2.AH_ComplianceSubType = ItalyComplianceInfo.ComplianceSubTypeCodes.ARI;

					var apInvoice1 = TestObjectCreator.CreateAPInvoice<APInvoice>("1", TestObjectCreator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m, TestObjectCreator.ABIGAS);
					apInvoice1.AH_ComplianceSubType = ItalyComplianceInfo.ComplianceSubTypeCodes.API;
					var apInvoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("2", TestObjectCreator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m, TestObjectCreator.ABIGAS);
					apInvoice2.AH_ComplianceSubType = ItalyComplianceInfo.ComplianceSubTypeCodes.API;

					Factory.Save();

					form.Controls.Add(TestTransactionModule.EmbeddedControl);
					form.Show();
					TestTransactionModule.PerformSearch_ForTest();

					TestTransactionModule.DisplayGrid.Select(1);
					AssertEquals(1, TestTransactionModule.DisplayGrid.SelectedElements.Length);
					allocateComplianceInfoMenuItem.PerformClick();
					var lastShownForm = TestTransactionModule.LastShownInvoicePrintingForm_ForTestOnly;
					AssertEquals("System should show the Class A Invoice allocation form", typeof(ClassAInvoiceForm), lastShownForm.GetType());
					if (lastShownForm != null)
					{
						lastShownForm.Close();
						if (!lastShownForm.IsDisposed)
						{
							lastShownForm.Dispose();
						}
					}
				}
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), italyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (fTransactionModule = (TransactionModuleStrip)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				Menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();

				MenuItem allocateComplianceInfoMenuItem = Menu.FindByText("Allocate Compliance Sub-Type and Number");
				AssertNull("Compliance allocation menu item should not be present when Compliance Document moduel enabled.", allocateComplianceInfoMenuItem);
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), afghanistanBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (fTransactionModule = (TransactionModuleStrip)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				Menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();

				MenuItem allocateComplianceInfoMenuItem = Menu.FindByText("Allocate Compliance Sub-Type and Number");
				AssertNull("Compliance allocation menu item should not be present in Afghanistan (because there are NO sub-types configured for it)", allocateComplianceInfoMenuItem);
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), afghanistanBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (fTransactionModule = (TransactionModuleStrip)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				Menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();

				MenuItem allocateComplianceInfoMenuItem = Menu.FindByText("Allocate Compliance Sub-Type and Number");
				AssertNull("Compliance allocation menu item should not be present in Afghanistan (because there are NO sub-types configured for it and Compliance Document Module enabled.)", allocateComplianceInfoMenuItem);
			}
		}

		public void TestLockComplianceBookMenuItem()
		{
			var italyCompany = TestObjectCreator.CreateNewCompany("ITL", Core.Constants.CountryCodes.Italy);
			var italyBranch = TestObjectCreator.CreateNewBranch(italyCompany, "ITL");
			OrgHeader italyCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("ITORGPROXY", true, true, "ITMIL");
			italyCompany.GC_OH_OrgProxy = italyCompanyOrgProxy.PK;

			var afghanistanCompany = TestObjectCreator.CreateNewCompany("AFG", Core.Constants.CountryCodes.Afghanistan);
			var afghanistanBranch = TestObjectCreator.CreateNewBranch(afghanistanCompany, "AFB");
			OrgHeader afghanistanCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("AFGORGPROX", true, true, "AFBIN");
			afghanistanCompany.GC_OH_OrgProxy = afghanistanCompanyOrgProxy.PK;

			var chinaCompany = TestObjectCreator.CreateNewCompany("CHN", Core.Constants.CountryCodes.China);
			var chinaBranch = TestObjectCreator.CreateNewBranch(chinaCompany, "NJG");
			OrgHeader chinaCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("CHNORGPROXY", true, true, "NJGIM");
			chinaCompany.GC_OH_OrgProxy = chinaCompanyOrgProxy.PK;

			Factory.Save();

			fTransactionModule.Dispose();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), italyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (fTransactionModule = (TransactionModuleStrip)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				Menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();

				var lockComplianceBookMenuItem = Menu.FindByText("Lock Counter Compliance Book");
				AssertNotNull("Lock Counter Compliance Book menu item should be present in Italy (because there are sub-types configured for it and present not in china)", lockComplianceBookMenuItem);
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(TestTransactionModule.EmbeddedControl);
					form.Show();

					Env.Security.ComplianceSequencesModifyLockRelease.IsAllowed = true;
					Env.Security.ComplianceSequencesModifyReleaseOtherStaff.IsAllowed = true;

					lockComplianceBookMenuItem.PerformClick();

					var lastShownForm = TestTransactionModule.LastShownInvoicePrintingForm_ForTestOnly;
					AssertEquals("LastFormShownDialogForTest", typeof(LockReleaseComplianceBookForm), lastShownForm.GetType());

					if (lastShownForm != null)
					{
						lastShownForm.Close();
						if (!lastShownForm.IsDisposed)
						{
							lastShownForm.Dispose();
						}
					}

					Env.Security.ComplianceSequencesModifyLockRelease.IsAllowed = true;
					Env.Security.ComplianceSequencesModifyReleaseOtherStaff.IsAllowed = false;

					lockComplianceBookMenuItem.PerformClick();

					var lastShownForm2 = TestTransactionModule.LastShownInvoicePrintingForm_ForTestOnly;
					AssertEquals("LastFormShownDialogForTest", typeof(LockReleaseComplianceBookForm), lastShownForm2.GetType());

					if (lastShownForm2 != null)
					{
						lastShownForm2.Close();
						if (!lastShownForm2.IsDisposed)
						{
							lastShownForm2.Dispose();
						}
					}

					Env.Security.ComplianceSequencesModifyLockRelease.IsAllowed = false;
					Env.Security.ComplianceSequencesModifyReleaseOtherStaff.IsAllowed = true;

					lockComplianceBookMenuItem.PerformClick();
					AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

" + Env.Security.ComplianceSequencesModifyLockRelease.DisplayTextPathToSecurityRight, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();

					Env.Security.ComplianceSequencesModifyLockRelease.IsAllowed = false;
					Env.Security.ComplianceSequencesModifyReleaseOtherStaff.IsAllowed = false;

					lockComplianceBookMenuItem.PerformClick();
					AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

" + Env.Security.ComplianceSequencesModifyLockRelease.DisplayTextPathToSecurityRight, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), italyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (fTransactionModule = (TransactionModuleStrip)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				Menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();

				Assert(GlbCompany.CurrentCompany.Country.SupportComplianceSubType);

				MenuItem lockComplianceBookMenuItem = Menu.FindByText("Lock Compliance Invoice Book");
				AssertNull("Lock Compliance Invoice Book menu item should not be present when Compliance Document Module enabled.", lockComplianceBookMenuItem);
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), chinaBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (fTransactionModule = (TransactionModuleStrip)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				Menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();

				Assert(GlbCompany.CurrentCompany.Country.SupportComplianceSubType);
				Assert(!GlbCompany.CurrentCompany.Country.HasAccComplianceSequence);

				var lockComplianceBookMenuItem = Menu.FindByText("Lock Counter Compliance Book");
				AssertNull("Lock Counter Compliance Book menu item should not be present in china (because china not have AccComplianceSequence Module)", lockComplianceBookMenuItem);
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), afghanistanBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (fTransactionModule = (TransactionModuleStrip)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				Menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();

				Assert(!GlbCompany.CurrentCompany.Country.SupportComplianceSubType);

				MenuItem lockComplianceBookMenuItem = Menu.FindByText("Lock Counter Compliance Book");
				AssertNull("Lock Counter Compliance Book menu item should not be present in Afghanistan (because there are NO sub-types configured for it)", lockComplianceBookMenuItem);
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), afghanistanBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (fTransactionModule = (TransactionModuleStrip)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				Menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();

				Assert(!GlbCompany.CurrentCompany.Country.SupportComplianceSubType);

				MenuItem lockComplianceBookMenuItem = Menu.FindByText("Lock Compliance Invoice Book");
				AssertNull("Lock Compliance Invoice Book menu item should not be present in Afghanistan (because there are NO sub-types configured for it and Compliance Document Module enabled.) ", lockComplianceBookMenuItem);
			}
		}

		public void TestReleaseComplianceBookMenuItem()
		{
			var italyCompany = TestObjectCreator.CreateNewCompany("ITL", Core.Constants.CountryCodes.Italy);
			var italyBranch = TestObjectCreator.CreateNewBranch(italyCompany, "ITL");
			OrgHeader italyCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("ITORGPROXY", true, true, "ITMIL");
			italyCompany.GC_OH_OrgProxy = italyCompanyOrgProxy.PK;

			var afghanistanCompany = TestObjectCreator.CreateNewCompany("AFG", Core.Constants.CountryCodes.Afghanistan);
			var afghanistanBranch = TestObjectCreator.CreateNewBranch(afghanistanCompany, "AFB");
			OrgHeader afghanistanCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("AFGORGPROX", true, true, "AFBIN");
			afghanistanCompany.GC_OH_OrgProxy = afghanistanCompanyOrgProxy.PK;

			var chinaCompany = TestObjectCreator.CreateNewCompany("CHN", Core.Constants.CountryCodes.China);
			var chinaBranch = TestObjectCreator.CreateNewBranch(chinaCompany, "NJG");
			OrgHeader chinaCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("CHNORGPROXY", true, true, "NJGIM");
			chinaCompany.GC_OH_OrgProxy = chinaCompanyOrgProxy.PK;

			Factory.Save();

			fTransactionModule.Dispose();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), italyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (fTransactionModule = (TransactionModuleStrip)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				Menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();

				var releaseComplianceBookMenuItem = Menu.FindByText("Release Counter Compliance Book");
				AssertNotNull("Release Counter Compliance Book menu item should be present in Italy (because there are sub-types configured for it and present not in china)", releaseComplianceBookMenuItem);
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(TestTransactionModule.EmbeddedControl);
					form.Show();

					Env.Security.ComplianceSequencesModifyLockRelease.IsAllowed = true;
					Env.Security.ComplianceSequencesModifyReleaseOtherStaff.IsAllowed = true;

					releaseComplianceBookMenuItem.PerformClick();

					var lastShownForm = TestTransactionModule.LastShownInvoicePrintingForm_ForTestOnly;
					AssertEquals("LastFormShownDialogForTest", typeof(LockReleaseComplianceBookForm), lastShownForm.GetType());

					if (lastShownForm != null)
					{
						lastShownForm.Close();
						if (!lastShownForm.IsDisposed)
						{
							lastShownForm.Dispose();
						}
					}

					Env.Security.ComplianceSequencesModifyLockRelease.IsAllowed = false;
					Env.Security.ComplianceSequencesModifyReleaseOtherStaff.IsAllowed = true;

					releaseComplianceBookMenuItem.PerformClick();

					var lastShownForm2 = TestTransactionModule.LastShownInvoicePrintingForm_ForTestOnly;
					AssertEquals("LastFormShownDialogForTest", typeof(LockReleaseComplianceBookForm), lastShownForm2.GetType());

					if (lastShownForm2 != null)
					{
						lastShownForm2.Close();
						if (!lastShownForm2.IsDisposed)
						{
							lastShownForm2.Dispose();
						}
					}

					Env.Security.ComplianceSequencesModifyLockRelease.IsAllowed = true;
					Env.Security.ComplianceSequencesModifyReleaseOtherStaff.IsAllowed = false;

					releaseComplianceBookMenuItem.PerformClick();

					var lastShownForm3 = TestTransactionModule.LastShownInvoicePrintingForm_ForTestOnly;
					AssertEquals("LastFormShownDialogForTest", typeof(LockReleaseComplianceBookForm), lastShownForm3.GetType());

					if (lastShownForm3 != null)
					{
						lastShownForm3.Close();
						if (!lastShownForm3.IsDisposed)
						{
							lastShownForm3.Dispose();
						}
					}

					Env.Security.ComplianceSequencesModifyLockRelease.IsAllowed = false;
					Env.Security.ComplianceSequencesModifyReleaseOtherStaff.IsAllowed = false;

					releaseComplianceBookMenuItem.PerformClick();
					AssertEquals(@"You do not have the appropriate security rights to run this function. 

If you require access to lock/release ‘CTR’ Allocation Level Compliance Invoice Book, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

" + Env.Security.ComplianceSequencesModifyLockRelease.DisplayTextPathToSecurityRight + @"

If you require access to release ‘CTR’ Allocation Level Compliance Invoice Book locked by other staff, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

" + Env.Security.ComplianceSequencesModifyReleaseOtherStaff.DisplayTextPathToSecurityRight, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), italyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (fTransactionModule = (TransactionModuleStrip)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				Menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();

				Assert(GlbCompany.CurrentCompany.Country.SupportComplianceSubType);

				MenuItem releaseComplianceBookMenuItem = Menu.FindByText("Release Compliance Invoice Book");
				AssertNull("Release Compliance Invoice Book menu item should not be present in Italy when Compliance Document Module enabled.", releaseComplianceBookMenuItem);
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), chinaBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (fTransactionModule = (TransactionModuleStrip)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				Menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();

				Assert(GlbCompany.CurrentCompany.Country.SupportComplianceSubType);
				Assert(!GlbCompany.CurrentCompany.Country.HasAccComplianceSequence);

				var releaseComplianceBookMenuItem = Menu.FindByText("Release Counter Compliance Book");
				AssertNull("Release Counter Compliance Book menu item should not be present in china (because china not have AccComplianceSequence Module)", releaseComplianceBookMenuItem);
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), afghanistanBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (fTransactionModule = (TransactionModuleStrip)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				Menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();

				Assert(!GlbCompany.CurrentCompany.Country.SupportComplianceSubType);

				MenuItem releaseComplianceBookMenuItem = Menu.FindByText("Release Counter Compliance Book");
				AssertNull("Release Counter Compliance Book menu item should not be present in Afghanistan (because there are NO sub-types configured for it)", releaseComplianceBookMenuItem);
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), afghanistanBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (fTransactionModule = (TransactionModuleStrip)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				Menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();

				Assert(!GlbCompany.CurrentCompany.Country.SupportComplianceSubType);

				MenuItem releaseComplianceBookMenuItem = Menu.FindByText("Release Compliance Invoice Book");
				AssertNull("Release Compliance Invoice Book menu item should not be present in Afghanistan (because there are NO sub-types configured for it and Compliance Document Module enabled.)", releaseComplianceBookMenuItem);
			}
		}

		public void TestPrintAccountingJournalForINV()
		{
			using (ZForm form = new ZForm())
			{
				AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
				AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader2.PK.ToGuid());

				TestTransactionModule.PerformSearch_ForTest();
				var testCollection = (BusinessObjectCollection)TestTransactionModule.GetNewGridCollection_ForTestOnly();
				testCollection.Load();
				AssertEquals(0, testCollection.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				TestTransactionModule.HandlePrintAccountingJournal_ForTestOnly(null, new EventArgs());
				AssertEquals("Please select transaction(s) to print.", UnitTestUserNotification.Instance.LastMessage.Text);

				var arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "1", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
				arInvoice1.AH_OH = TestObjectCreator.AALSHI.PK;
				arInvoice1.Lines[0].AL_AG = TestObjectCreator.GLHeader1.PK;

				var arInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "2", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
				arInvoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
				arInvoice2.Lines[0].AL_AG = TestObjectCreator.GLHeader1.PK;

				var apInvoice1 = TestObjectCreator.CreateAPInvoice<APInvoice>("1", TestObjectCreator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m, TestObjectCreator.ABIGAS);
				apInvoice1.Lines[0].AL_AG = TestObjectCreator.GLHeader2.PK;

				var apInvoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("2", TestObjectCreator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m, TestObjectCreator.ABIGAS);
				apInvoice2.Lines[0].AL_AG = TestObjectCreator.GLHeader2.PK;

				Factory.Save();

				form.Controls.Add(TestTransactionModule.EmbeddedControl);
				form.Show();

				TestTransactionModule.PerformSearch_ForTest();

				testCollection = (BusinessObjectCollection)TestTransactionModule.GetNewGridCollection_ForTestOnly();
				testCollection.Load();
				AssertEquals(2, testCollection.Count);

				TestTransactionModule.DisplayGrid.SelectAllElements();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				TestTransactionModule.HandlePrintAccountingJournal_ForTestOnly(null, new EventArgs());
				AssertEquals("There are 2 accounting journals to print. Do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[ExpectNoExceptions]
		public virtual void TestPrintingMultipleTransactions()
		{
			var mockIPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
			using (new PrintTaskUIProviderFactory.OverriderForTesting(mockIPrintTaskUIProvider.Object))
			using (ZForm form = new ZForm())
			{
				var arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "1", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
				arInvoice1.AH_OH = TestObjectCreator.AALSHI.PK;
				var arInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "2", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
				arInvoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
				var apInvoice1 = TestObjectCreator.CreateAPInvoice<APInvoice>("1", TestObjectCreator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m, TestObjectCreator.ABIGAS);
				var apInvoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("2", TestObjectCreator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m, TestObjectCreator.ABIGAS);
				Factory.Save();

				form.Controls.Add(TestTransactionModule.EmbeddedControl);
				form.Show();
				TestTransactionModule.PerformSearch_ForTest();

				TestTransactionModule.DisplayGrid.SelectAllElements();
				AssertEquals(2, TestTransactionModule.DisplayGrid.SelectedElements.Length);

				TestTransactionModule.HandlePrint_ForTestOnly(this, EventArgs.Empty);
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[ExpectNoExceptions]
		public virtual void TestPrintingDoesNotThrowExceptionsWithNoTransactionResult()
		{
			using (ZForm form = new ZForm())
			{
				form.Controls.Add(TestTransactionModule.EmbeddedControl);
				form.Show();
				TestTransactionModule.PerformSearch_ForTest();

				AssertEquals(0, TestTransactionModule.DisplayGrid.SelectedElements.Length);
				AssertEquals(null, TestTransactionModule.CurrentBusinessObjectInGrid_ForTestOnly);

				TestTransactionModule.HandlePrint_ForTestOnly(this, EventArgs.Empty);
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAllowEdit()
		{
			Assert(TestTransactionModule.AllowEdit);
		}

		public void TestForActiveStatusAndAuditFiltersInTheFilterCollection()
		{
			using (APEnquiryModule testModule = new APEnquiryModule())
			{
				ModuleTextFilter activeFilter = (ModuleTextFilter)testModule.FilterBusinessObject["Active Status"];
				AssertNotNull("The 'Active Status' should exist in the APEnquiry module", activeFilter);
				Assert("The 'Active Status' should be visible always", activeFilter.Visibility == FilterVisibility.Visible);

				var creatingUserFilter = (ModuleNkFilter)testModule.FilterBusinessObject["Creating User"];
				AssertNotNull("The 'Creating User' should exist in the APEnquiry module", creatingUserFilter);

				var creatingTimeFilter = (ModuleDateFilter)testModule.FilterBusinessObject["Created Time"];
				AssertNotNull("The 'Created Time' should exist in the APEnquiry module", creatingUserFilter);
			}
		}

		public virtual void TestExportQueryWithFilter()
		{
			var testFilterBizO = TestTransactionModule.FilterBusinessObject;
			var transactionFilter = (ModuleDateFilter)testFilterBizO["Created Time"];
			transactionFilter.IsActive = true;
			transactionFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			transactionFilter.Property1 = new ZDateTime(2015, 12, 10); //local time
			transactionFilter.Property2 = new ZDateTime(2015, 12, 20); //local time
			TestTransactionModule.PerformSearch_ForTest();

			var exportQuery = TestTransactionModule.ExportQuery_ForTestOnly.GetAsWhereClause(true);

			var queryExpected1 = string.Format(@"AH_GC = CONVERT('{0}', 'System.Guid') and AH_Ledger = '{1}'"
				, GlbCompany.CurrentCompany.PK.ToString(), GetModuleID().Name.Substring(0, 2));

			var queryExpected2 = "AH_SystemCreateTimeUtc >= #2015-12-09 14:00:00.000# and AH_SystemCreateTimeUtc <= #2015-12-20 13:59:00.000#)"; //UTC time

			AssertContains(queryExpected1, exportQuery);
			AssertContains(queryExpected2, exportQuery);
		}

		public virtual void TestDisplayResultsQueryWithFilter()
		{
			var testFilterBizO = TestTransactionModule.FilterBusinessObject;
			var transactionFilter = (ModuleDateFilter)testFilterBizO["Created Time"];
			transactionFilter.IsActive = true;
			transactionFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			transactionFilter.Property1 = new ZDateTime(2015, 12, 10); //local time
			transactionFilter.Property2 = new ZDateTime(2015, 12, 20); //local time
			TestTransactionModule.PerformSearch_ForTest();

			var testQuery = TestTransactionModule.GetDisplayResultsQuery_ForTestOnly();
			var displayResultsQuery = testQuery.GetAsWhereClause(true);
			AssertEquals("MaximumRows should be 1001", 1001, testQuery.MaximumRows);

			var queryExpected1 = string.Format(@"AH_GC = CONVERT('{0}', 'System.Guid') and AH_Ledger = '{1}'"
				, GlbCompany.CurrentCompany.PK.ToString(), GetModuleID().Name.Substring(0, 2));

			var queryExpected2 = "AH_SystemCreateTimeUtc >= #2015-12-09 14:00:00.000# and AH_SystemCreateTimeUtc <= #2015-12-20 13:59:00.000#"; //UTC time
			AssertContains(queryExpected1, displayResultsQuery);
			AssertContains(queryExpected2, displayResultsQuery);
		}

		public void TestDeleteMenuItemText()
		{
			AssertEquals("&Reverse", TestTransactionModule.GetDeleteMenuItemText_ForTestOnly().Caption);
			AssertEquals("Creates a new reversed item(s) to offset the currently selected item(s) (shortcut Del)", TestTransactionModule.GetDeleteMenuItemText_ForTestOnly().FullDescription);
		}

		#region ImportRemittanceFileEventHandler

		public abstract void TestImportRemittanceFileSecurityCheckpoint();

		public void TestImportRemittanceFileMenuItem()
		{
			MenuItem[] actionMenu = TestTransactionModule.FormActionMenu;
			MenuItem importRemittanceFileMenuItem = actionMenu.FindByText("Actions").MenuItems.FindByText("Data Transfer").MenuItems.FindByText("Import Remittance File");
			AssertNotNull("Menu item should exist", importRemittanceFileMenuItem);

			importRemittanceFileMenuItem.PerformClick();
			Assert(ZFormModaliser.LastFormShownDialogForTest is DataImporterForm);
			Assert(((DataImporterForm)ZFormModaliser.LastFormShownDialogForTest).Importer is PaymentReceiptRemittanceDataImporter);

			importRemittanceFileMenuItem.PerformClick();
			AssertEquals("None ", UnitTestUserNotification.Instance.LastMessage.ToString());

			TestTransactionModule.ImportRemittanceFileSecurityCheckpoint_ForTestOnly.IsAllowed = false;
			importRemittanceFileMenuItem.PerformClick();
			AssertEquals("Error " + TestTransactionModule.ImportRemittanceFileSecurityCheckpoint_ForTestOnly.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportData_AutoPrintChequeBook()
		{
			var sydBranch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "SYD");
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, sydBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				TestObjectCreator.CreateTestPeriods(new ZDateTime(2009, 01, 01));
				TestObjectCreator.ABIGAS.OH_IsCreditor = true;
				TestObjectCreator.AALSHI.OH_IsDebtor = true;

				TestObjectCreator.AUDBankAccount.AB_Code = "AUD";
				TestObjectCreator.AUDChequeBook.AK_Code = "AUDC";
				TestObjectCreator.AUDChequeBook.AK_GB = GlbBranch.CurrentBranch.PK;
				TestObjectCreator.USDBankAccount.AB_Code = "USD";

				PaymentChequeNumberAllocator.DummyPaymentChequeNumberAllocator test_Allocator = null;
				AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));
				AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionLines)));
				AssertNull("Precondition: ", test_Allocator);

				InvoicingBase invoice1 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "APINV123456", TestObjectCreator.AUD, 1M);
				invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
				TestObjectCreator.CreateInvoiceLine(invoice1, TestObjectCreator.AUD, 1M, 100M, 0M, 0M);
				InvoicingBase invoice2 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "APINV987654", TestObjectCreator.AUD, 1M);
				invoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
				TestObjectCreator.CreateInvoiceLine(invoice2, TestObjectCreator.AUD, 1M, 300M, 0M, 0M);
				InvoicingBase invoice3 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "APINV2", TestObjectCreator.AUD, 1M);
				invoice3.AH_OH = TestObjectCreator.ABIGAS.PK;
				TestObjectCreator.CreateInvoiceLine(invoice3, TestObjectCreator.AUD, 1M, 100M, 0M, 0M);

				InvoicingBase invoice4 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "00001000", TestObjectCreator.USD, 0.67M);
				invoice4.AH_OH = TestObjectCreator.AALSHI.PK;
				TestObjectCreator.CreateInvoiceLine(invoice4, TestObjectCreator.USD, 0.67M, 1500M, 0M, 0M);
				InvoicingBase invoice5 = TestObjectCreator.CreateInvoice(typeof(ARCreditNote), "00001000", TestObjectCreator.USD, 0.67M);
				invoice5.AH_OH = TestObjectCreator.ABIGAS.PK;
				TestObjectCreator.CreateInvoiceLine(invoice5, TestObjectCreator.USD, 0.67M, 800M, 0M, 0M);
				InvoicingBase invoice6 = TestObjectCreator.CreateInvoice(typeof(APCreditNote), "apcredit", TestObjectCreator.USD, 0.67M);
				invoice6.AH_OH = TestObjectCreator.AALSHI.PK;
				TestObjectCreator.CreateInvoiceLine(invoice6, TestObjectCreator.USD, 0.67M, 300M, 0M, 0M);

				TestObjectCreator.SetupAutoPrintChequeBook(TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook, Factory);

				Factory.Save();

				string pathToTestFile = BaseSourcePath +
					@"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\PaymentReceiptRemittanceFile\Testing\PayRecRemittance.csv";
				using (StreamReader reader = new StreamReader(pathToTestFile))
				{
					PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
					importer.OnSavePaymentWithChequeNumberAllocator += (object sender, EventArgs e) =>
					{
						PaymentApprovalBase paymentApproval = sender as PaymentApprovalBase;
						test_Allocator = new PaymentChequeNumberAllocator.DummyPaymentChequeNumberAllocator(paymentApproval, PaymentChequeNumberAllocator.PrintingMode.PaymentApproval, paymentApproval.Factory);
						BusinessObjectFactory.SaveTogether(test_Allocator.GetFactoriesForTest());
					};

					NotificationBuffer notificationBuffer = new NotificationBuffer();
					importer.ImportData(reader, pathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

					Assert(!notificationBuffer.HasErrors);
					Assert(!notificationBuffer.HasWarnings);

					ZQuery query = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment);
					query.AddToFilter(AccTransactionHeaderSchema.AH_GC, invoice1.AH_GC);
					APPayment[] payment = Factory.Load<APPayment>(query);
					AssertEquals(1, payment.Length);
					AssertEquals(500M, payment[0].AH_OSTotalAmount);
					AssertEquals(500M, payment[0].AH_LocalTotalAmount);
					AssertEquals(0M, payment[0].AH_Calc_OSOutstandingAmount);
					AssertEquals("000001", payment[0].AH_ChequeOrReference);
					AssertEquals(new ZDateTime(2009, 01, 15), payment[0].AH_PostDate);

					AssertEquals(0M, invoice1.AH_Calc_OSOutstandingAmount);
					AssertEquals(0M, invoice2.AH_Calc_OSOutstandingAmount);
					AssertEquals(0M, invoice3.AH_Calc_OSOutstandingAmount);

					Assert("Auto printing should be performed", test_Allocator.ChequeWasAutoPrinted);
					AssertEquals("Printing called only once", 1, test_Allocator.PrintingCalled_Counter);
					AssertEquals("PaymentPrinter should have correct Printer passed to", payment[0].ChequeBookBizO.AK_SQ, test_Allocator.PrinterPassedForAutoPrinting);

					TransactionMatchLink matchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, payment[0].PK));
					AssertNotNull(matchLink);
					TransactionMatchLink invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice1.PK));
					AssertNotNull(invoiceMatchLink);
					AssertEquals(matchLink.AP_MatchGroupNum, invoiceMatchLink.AP_MatchGroupNum);
					invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice2.PK));
					AssertNotNull(invoiceMatchLink);
					AssertEquals(matchLink.AP_MatchGroupNum, invoiceMatchLink.AP_MatchGroupNum);
					invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice3.PK));
					AssertNotNull(invoiceMatchLink);
					AssertEquals(matchLink.AP_MatchGroupNum, invoiceMatchLink.AP_MatchGroupNum);

					query = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Receipt);
					query.AddToFilter(AccTransactionHeaderSchema.AH_GC, invoice1.AH_GC);
					ARReceipt[] receipt = Factory.Load<ARReceipt>(query);
					AssertEquals(1, receipt.Length);
					AssertEquals(1000M, receipt[0].AH_OSTotalAmount);
					AssertEquals(1500M, receipt[0].AH_LocalTotalAmount);
					AssertEquals(0M, receipt[0].AH_Calc_OSOutstandingAmount);
					AssertEquals("103", receipt[0].AH_ChequeOrReference);
					AssertEquals(new ZDateTime(2009, 01, 15), receipt[0].AH_PostDate);

					AssertEquals(0M, invoice4.AH_Calc_OSOutstandingAmount);
					AssertEquals(0M, invoice5.AH_Calc_OSOutstandingAmount);
					AssertEquals(0M, invoice6.AH_Calc_OSOutstandingAmount);

					matchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, receipt[0].PK));
					AssertNotNull(matchLink);
					invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice4.PK));
					AssertNotNull(invoiceMatchLink);
					AssertEquals(matchLink.AP_MatchGroupNum, invoiceMatchLink.AP_MatchGroupNum);
					invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice5.PK));
					AssertNotNull(invoiceMatchLink);
					AssertEquals(matchLink.AP_MatchGroupNum, invoiceMatchLink.AP_MatchGroupNum);
					invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice6.PK));
					AssertNotNull(invoiceMatchLink);
					AssertEquals(matchLink.AP_MatchGroupNum, invoiceMatchLink.AP_MatchGroupNum);
				}
			}
		}

		#endregion

		#region MatchTransactionsMenuItem
		public void TestMatchTransactionsMenuItem()
		{
			var menu = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();
			if (TestTransactionModule.GetType() == typeof(APTransactionModuleStrip) || TestTransactionModule.GetType() == typeof(ARTransactionModuleStrip))
			{
				AssertNotNull("Match Transactions Menu item should exist", menu.FindByText(TestTransactionModule.ViewMatchedTransactionsMenuItemText_ForTestOnly.Caption));
			}
			else
			{
				AssertNull("Match Transactions Menu item should not exist on other modules", menu.FindByText(TestTransactionModule.ViewMatchedTransactionsMenuItemText_ForTestOnly.Caption));
				Assert("No other tests are relevant to view match transactions", true);
				return;
			}

			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			var transaction = CreateTransactionsForMatchTransactionPopup();
			Factory.Save();

			using (ZForm form = new ZForm())
			{
				form.Controls.Add(TestTransactionModule.EmbeddedControl);
				form.Show();
				TestTransactionModule.PerformSearch_ForTest();
				AssertEquals("Three transactions should be found on the grid", 3, TestTransactionModule.DisplayGrid.VisibleRowCount);

				TestTransactionModule.DisplayGrid.UnSelectAll();
				AssertEquals("No transactions should be selected on the grid", 0, TestTransactionModule.DisplayGrid.SelectedRowCount);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				TestTransactionModule.HandleViewMatchedTransactions_ForTestOnly(this, EventArgs.Empty);

				AssertEquals("Error should be shown if no transactions selected", "Please select one transaction to search for matches.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("LastFormShownDialogForTest", ZFormModaliser.LastFormShownDialogForTest);

				TestTransactionModule.DisplayGrid.SelectAllElements();
				AssertEquals("Three transactions should be selected on the grid", 3, TestTransactionModule.DisplayGrid.SelectedRowCount);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				TestTransactionModule.HandleViewMatchedTransactions_ForTestOnly(this, EventArgs.Empty);

				AssertEquals("Error should be shown if more than one transaction selected", "Please select one transaction to search for matches.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("LastFormShownDialogForTest", ZFormModaliser.LastFormShownDialogForTest);

				TestTransactionModule.DisplayGrid.UnSelectAll();
				var transactionIdxInGrid = GetRowIndex(TestTransactionModule.DisplayGrid, transaction.AH_TransactionNum);
				AssertNotEquals("The transaction should be found on the grid.", -1, transactionIdxInGrid);
				TestTransactionModule.DisplayGrid.Select(transactionIdxInGrid);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				TestTransactionModule.HandleViewMatchedTransactions_ForTestOnly(this, EventArgs.Empty);

				AssertType("LastFormShownDialogForTest", typeof(ZArchitecture.GUI.Internal.EmbeddedModulePopup), ZFormModaliser.ActiveForm);
				var lastForm = (ZArchitecture.GUI.Internal.EmbeddedModulePopup)ZFormModaliser.ActiveForm;
				AssertEquals("The OK button should be hidden via a slightly obtuse property", false, lastForm.RequireAtLeastOneItemToBeSelected);
				AssertType("Popup module", ExpectedModuleTypeForMatchTransactionPopup, lastForm.Module_ForTest);
				var actualModule = (MatchingModule)lastForm.Module_ForTest;

				var transactionNumberFilter = (ModuleNumberFilter)actualModule.FilterBusinessObject.ModuleFilters[AccountingUtils.NumberFilterTypes.TransactionNumber];
				AssertEquals("Transaction Number Filter value should be the AR/AP transaction number", transaction.AH_TransactionNum, transactionNumberFilter.Property);
				Assert("Transaction Number Filter should be read only", transactionNumberFilter.ReadOnly);
				AssertEquals("Transaction Number Filter should be always visible", FilterVisibility.AlwaysVisible, transactionNumberFilter.Visibility);

				var transactionTypeFilter = (ModuleTextFilter)actualModule.FilterBusinessObject.ModuleFilters[AccountingUtils.ModesAndTypesFilterTypes.TransactionType];
				AssertEquals("Transaction Type Filter value should be the AR/AP transaction type", transaction.AH_TransactionType, transactionTypeFilter.Property);
				Assert("Transaction Type Filter should be read only", transactionTypeFilter.ReadOnly);
				AssertEquals("Transaction Type Filter should be always visible", FilterVisibility.AlwaysVisible, transactionTypeFilter.Visibility);

				var filterObject = (MatchingBaseFilterBusinessObject)actualModule.FilterBusinessObject;
				AssertEquals("Ledger Filter value should be the AR/AP ledger", filterObject.LedgerFilterValue, transaction.AH_Ledger);

				lastForm.Close();
				form.Close();
			}
		}

		protected abstract InvoicingBase CreateTransactionsForMatchTransactionPopup();

		protected abstract Type ExpectedModuleTypeForMatchTransactionPopup { get; }

		public void TestMatchTransactionsMenuItem_AppliesSecurityCheckpoint()
		{
			using (ZForm form = new ZForm())
			{
				form.Controls.Add(TestTransactionModule.EmbeddedControl);
				form.Show();
				TestTransactionModule.PerformSearch_ForTest();

				var checkpointForTest = SecurityCheckpointForMatchTransactions;
				var originalValue = checkpointForTest.IsAllowed;
				try
				{
					checkpointForTest.IsAllowed = false;
					AssertSecurityCheckpoint(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

" + checkpointForTest.DisplayTextPathToSecurityRight.ToString());

					checkpointForTest.IsAllowed = true;
					AssertSecurityCheckpoint("Please select one transaction to search for matches.");
				}
				finally
				{
					checkpointForTest.IsAllowed = originalValue;
				}
			}

			void AssertSecurityCheckpoint(string expectedMessageText)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				TestTransactionModule.HandleViewMatchedTransactions_ForTestOnly(this, EventArgs.Empty);

				AssertEquals(expectedMessageText, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("LastFormShownDialogForTest", ZFormModaliser.LastFormShownForTest);
			}
		}

		protected abstract SecurityCheckpoint SecurityCheckpointForMatchTransactions { get; }
		#endregion

		public virtual void TestTransactionPKsAreRegisteredForWHTAmountCalculation()
		{
			var bizo = CreateInvoice();
			Factory.Save();

			using (ZForm form = new ZForm())
			{
				form.Controls.Add(TestTransactionModule.EmbeddedControl);
				form.Show();

				var loaderMock = new Mock<IWHTAmountLoader>();
				loaderMock.SetupProperty(x => x.IsWHTRealizationInProgress);
				loaderMock.Setup(x => x.RegisterForLoadingWHTAmounts(bizo.PK));
				TestTransactionModule.WHTAmountLoaderSubstituter_ForTestOnly = (factory) => TaxFrameworkObjectFactory.SubstituteWHTAmountLoader_ForTestOnly(factory, loaderMock.Object);
				TestTransactionModule.PerformSearch_ForTest();
				loaderMock.VerifySet(x => x.IsWHTRealizationInProgress = false);
				loaderMock.Verify(x => x.RegisterForLoadingWHTAmounts(bizo.PK));
				AssertContainsExactElementsInAnyOrder(new ZGuid[] { bizo.PK }, TestTransactionModule.GridCollection.Cast<BusinessObject>().Select(b => b.PK));
				loaderMock.VerifyAll();
			}
		}

		public virtual void TestTransactionPKsAreRegisteredForWHTAmountCalculation_NoTransactionInTheGrid()
		{
			using (ZForm form = new ZForm())
			{
				form.Controls.Add(TestTransactionModule.EmbeddedControl);
				form.Show();

				var loaderMock = new Mock<IWHTAmountLoader>();
				loaderMock.SetupProperty(x => x.IsWHTRealizationInProgress);
				loaderMock.Setup(x => x.RegisterForLoadingWHTAmounts(It.IsAny<ZGuid[]>()));
				TestTransactionModule.WHTAmountLoaderSubstituter_ForTestOnly = (factory) => TaxFrameworkObjectFactory.SubstituteWHTAmountLoader_ForTestOnly(factory, loaderMock.Object);
				TestTransactionModule.PerformSearch_ForTest();
				loaderMock.VerifySet(x => x.IsWHTRealizationInProgress = false, Times.Never);
				loaderMock.Verify(x => x.RegisterForLoadingWHTAmounts(It.IsAny<ZGuid[]>()), Times.Never);
				AssertEquals(0, TestTransactionModule.GridCollection.Count);
			}
		}
	}

	public class DataImporterForm_ForTestOnly : DataImporterForm, IDataImporterFormForTestOnly
	{
		public DataImporterForm_ForTestOnly(DataImporterBusinessObject businessEntity, string formCaption, BillingInterfaceName interfaceName)
			: base(businessEntity, formCaption, interfaceName)
		{
		}

		public void ImportFromFileExposed(ZString fileName)
		{
			ImportFromFile(fileName);
		}

		public DataImporterForm ConvertToDataImporterForm()
		{
			return this;
		}
	}
}
