using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.GenericCharge;
using Enterprise.Accounting.Business.GlobalChargeCode;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices.Testing
{
	public class ApportionmentBuilderTest : TestCaseWithFactory
	{
		public void TestFindConsolPK()
		{
			NotificationBuffer notify = new NotificationBuffer();
			TransactionBuilderConfig config = new TransactionBuilderConfig();
			ApportionmentBuilder builder = new ApportionmentBuilder(new NotificationManager(new NotificationBuffer()), config);

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00002000";
			consol.JK_MasterBillNum = "NEWCONSOL";
			Factory.Save();

			ZGuid consolPKFindByConsolNumber = builder.GetConsolPK(Factory, consol.JK_UniqueConsignRef, ZString.Empty);
			AssertEquals("Consol found (searching by Consol Number)", true, consolPKFindByConsolNumber.IsValid);

			ZGuid consolPKFindByConsolMasterBillNumber = builder.GetConsolPK(Factory, ZString.Empty, consol.JK_MasterBillNum);
			AssertEquals("Consol found (searching by Master Bill Number)", true, consolPKFindByConsolMasterBillNumber.IsValid);
		}

		public void TestImportTxnHeaderUsingConsolNumber()
		{
			TxnLine.ChargeCode = ObjectCreator.CC1.AC_Code;
			TxnLine.ConsolOrJobType = Xsd.TxnLineConsolOrJobType.CSL;
			TxnLine.ConsolOrJobTypeSpecified = true;
			TxnLine.ConsolOrJobNo = Consol1.JK_UniqueConsignRef;
			TxnLine.HouseBIllNo = ZString.Empty;
			TxnLine.MasterBillNo = ZString.Empty;
			TxnLine.OsInvoiceAmtExclTax = TxnHeaderMapper.GetXmlFinancialValue(200M, ObjectCreator.USD, typeof(APInvoice));

			Factory.Save();

			Invoice = Factory.New<APInvoice>();

			TransactionBuilderConfig config = new TransactionBuilderConfig();
			ApportionmentBuilder builder = new ApportionmentBuilder(new NotificationManager(new NotificationBuffer()), config);

			builder.AddApportionmentToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");

			AssertEquals(1, Invoice.ConsolCosting.ConsolCosts.Count);
			AssertEquals("Invoice Lines Count", 2, Invoice.Lines.Count);

			AssertEquals("CHG", Invoice.ConsolCosting.ConsolCosts[0].E6_ApportionmentMethod);

			ZQuery invoiceLineForShipment1Filter = new ZQuery(AccTransactionLinesSchema.AL_AH, Invoice.PK);
			invoiceLineForShipment1Filter.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, Job1.PK);
			InvoicingLineBase invoiceLineForShipment1 = (InvoicingLineBase)Factory.LoadTop1(typeof(APInvoiceLine), invoiceLineForShipment1Filter);

			AssertNotNull("There should be an invoice line relating to job 1", invoiceLineForShipment1);
			AssertEquals("Invoice Line Value for Shipment 1 Value", 100M, invoiceLineForShipment1.AL_OSExTaxAmount);

			ZQuery invoiceLineForShipment2Filter = new ZQuery(AccTransactionLinesSchema.AL_AH, Invoice.PK);
			invoiceLineForShipment2Filter.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, Job2.PK);
			InvoicingLineBase invoiceLineForShipment2 = (InvoicingLineBase)Factory.LoadTop1(typeof(APInvoiceLine), invoiceLineForShipment2Filter);

			AssertNotNull("There should be an invoice line relating to job 2", invoiceLineForShipment2);
			AssertEquals("Invoice Line for Shipment 2 Value", 100M, invoiceLineForShipment2.AL_OSExTaxAmount);
		}

		public void TestImportConsolCostsForAPCreditNote()
		{
			TxnLine.ChargeCode = ObjectCreator.CC1.AC_Code;
			TxnLine.ConsolOrJobType = Xsd.TxnLineConsolOrJobType.CSL;
			TxnLine.ConsolOrJobTypeSpecified = true;
			TxnLine.ConsolOrJobNo = Consol1.JK_UniqueConsignRef;
			TxnLine.HouseBIllNo = ZString.Empty;
			TxnLine.MasterBillNo = ZString.Empty;
			TxnLine.OsInvoiceAmtExclTax = TxnHeaderMapper.GetXmlFinancialValue(200M, ObjectCreator.USD, typeof(APCreditNote));

			Factory.Save();

			APCreditNote creditNote = Factory.New<APCreditNote>();

			TransactionBuilderConfig config = new TransactionBuilderConfig();
			ApportionmentBuilder builder = new ApportionmentBuilder(new NotificationManager(new NotificationBuffer()), config);

			builder.AddApportionmentToInvoiceBusinessObject(creditNote, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");

			AssertEquals("Invoice Lines Count", 2, creditNote.Lines.Count);

			ZQuery invoiceLineForShipment1Filter = new ZQuery(AccTransactionLinesSchema.AL_AH, creditNote.PK);
			invoiceLineForShipment1Filter.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, Job1.PK);
			InvoicingLineBase invoiceLineForShipment1 = (InvoicingLineBase)Factory.LoadTop1(typeof(APCreditNoteLine), invoiceLineForShipment1Filter);

			AssertNotNull("There should be an invoice line relating to job 1", invoiceLineForShipment1);
			AssertEquals("Invoice Line Value for Shipment 1 Value", 100M, invoiceLineForShipment1.AL_OSExTaxAmount);

			ZQuery invoiceLineForShipment2Filter = new ZQuery(AccTransactionLinesSchema.AL_AH, creditNote.PK);
			invoiceLineForShipment2Filter.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, Job2.PK);
			InvoicingLineBase invoiceLineForShipment2 = (InvoicingLineBase)Factory.LoadTop1(typeof(APCreditNoteLine), invoiceLineForShipment2Filter);

			AssertNotNull("There should be an invoice line relating to job 2", invoiceLineForShipment2);
			AssertEquals("Invoice Line for Shipment 2 Value", 100M, invoiceLineForShipment2.AL_OSExTaxAmount);
		}

		void AddJobCharge(Job job, JobConsolCost cost)
		{
			Charge charge = job.Charges.AddNew();
			charge.JR_E6 = cost.PK;
			charge.JR_JH = job.PK;
			charge.JR_AC = cost.ChargeCode.PK;
		}

		void AssertApportionmentMethodForImportingTxnLine(bool hasExistingConsolCost, bool hasTwoConsolCostInDB, bool hasApportionmentMethodInTxnLine, Xsd.TxnLineConsolApportionmentMethod lineApportionmentMethod, ZString expectedApportionmentMethod, ZDecimal expectedLine1Amount, ZDecimal expectedLine2Amount)
		{
			var newConfig = ConsolCostDefaultApportionmentMethodConfiguration.Create_ForTestOnly(AllocationMethod.ChargeableUnits);
			AccountingConfigurationRegistry.Instance.ConsolCostDefaultApportionmentMethod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newConfig);

			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment1 = testObjectCreator.CreateShipment("S001001", consol);
			var shipment1Job = testObjectCreator.CreateJob(shipment1, false, false);

			var shipment2 = testObjectCreator.CreateShipment("S001002", consol);
			var shipment2Job = testObjectCreator.CreateJob(shipment2, false, false);
			shipment1.JS_ActualChargeable = 100M;
			shipment2.JS_ActualChargeable = 50M;

			if (hasExistingConsolCost)
			{
				var unpostedConsolCost = consol.GetApportionments().CostsCollection.TryAddNew();
				unpostedConsolCost.E6_OH_Creditor = ObjectCreator.AALSHI.PK;
				unpostedConsolCost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
				unpostedConsolCost.E6_OSCostAmount = 100m;
				unpostedConsolCost.E6_ApportionmentMethod = AllocationMethod.Manual;

				unpostedConsolCost.ApportionmentCharges[0].JR_IsUsedForApportionment = true;
				unpostedConsolCost.ApportionmentCharges[0].JR_E6 = unpostedConsolCost.PK;
				unpostedConsolCost.ApportionmentCharges[0].JR_OSCostAmt = 90m;
				AddJobCharge(shipment1Job, unpostedConsolCost);

				unpostedConsolCost.ApportionmentCharges[1].JR_IsUsedForApportionment = true;
				unpostedConsolCost.ApportionmentCharges[1].JR_E6 = unpostedConsolCost.PK;
				unpostedConsolCost.ApportionmentCharges[1].JR_OSCostAmt = 10m;
				AddJobCharge(shipment2Job, unpostedConsolCost);

				if (hasTwoConsolCostInDB)
				{
					var anotherUnpostedConsolCost = consol.GetApportionments().CostsCollection.TryAddNew();
					anotherUnpostedConsolCost.E6_OH_Creditor = ObjectCreator.ABIGAS.PK;
					anotherUnpostedConsolCost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
					anotherUnpostedConsolCost.E6_OSCostAmount = 20m;
					anotherUnpostedConsolCost.E6_ApportionmentMethod = AllocationMethod.Shipment;
				}
			}

			TxnLine.ChargeCode = ObjectCreator.CC1.AC_Code;
			TxnLine.ConsolOrJobType = Xsd.TxnLineConsolOrJobType.CSL;
			TxnLine.ConsolOrJobTypeSpecified = true;
			TxnLine.ConsolOrJobNo = consol.JK_UniqueConsignRef;
			if (hasApportionmentMethodInTxnLine)
			{
				TxnLine.ConsolApportionmentMethod = lineApportionmentMethod;
			}
			else
			{
				TxnLine.ConsolApportionmentMethodSpecified = false;
			}
			TxnLine.HouseBIllNo = ZString.Empty;
			TxnLine.MasterBillNo = ZString.Empty;
			TxnLine.OsInvoiceAmtExclTax = TxnHeaderMapper.GetXmlFinancialValue(1500M, ObjectCreator.USD, typeof(APInvoice));

			Factory.Save();

			Invoice = Factory.New<APInvoice>();
			Invoice.AH_OH = ObjectCreator.AALSHI.PK;

			TransactionBuilderConfig config = new TransactionBuilderConfig();
			ApportionmentBuilder builder = new ApportionmentBuilder(new NotificationManager(new NotificationBuffer()), config);

			builder.AddApportionmentToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");

			AssertEquals(1, Invoice.ConsolCosting.ConsolCosts.Count);
			AssertEquals("Invoice Lines Count", 2, Invoice.Lines.Count);

			AssertEquals(expectedApportionmentMethod, Invoice.ConsolCosting.ConsolCosts[0].E6_ApportionmentMethod);

			ZQuery invoiceLineForShipment1Filter = new ZQuery(AccTransactionLinesSchema.AL_AH, Invoice.PK);
			invoiceLineForShipment1Filter.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, shipment1Job.PK);
			InvoicingLineBase invoiceLineForShipment1 = (InvoicingLineBase)Factory.LoadTop1(typeof(APInvoiceLine), invoiceLineForShipment1Filter);

			AssertNotNull("There should be an invoice line relating to job 1", invoiceLineForShipment1);
			AssertEquals("Invoice Line Value for Shipment 1 Value", expectedLine1Amount, invoiceLineForShipment1.AL_OSExTaxAmount);

			ZQuery invoiceLineForShipment2Filter = new ZQuery(AccTransactionLinesSchema.AL_AH, Invoice.PK);
			invoiceLineForShipment2Filter.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, shipment2Job.PK);
			InvoicingLineBase invoiceLineForShipment2 = (InvoicingLineBase)Factory.LoadTop1(typeof(APInvoiceLine), invoiceLineForShipment2Filter);

			AssertNotNull("There should be an invoice line relating to job 2", invoiceLineForShipment2);
			AssertEquals("Invoice Line for Shipment 2 Value", expectedLine2Amount, invoiceLineForShipment2.AL_OSExTaxAmount);
		}

		public void Test_ImportHandleAppMethod_ConsolCostMatchedAndAppMethodIsSpecifiedInLine()
		{
			AssertApportionmentMethodForImportingTxnLine(true, false, true, Xsd.TxnLineConsolApportionmentMethod.SHP, "SHP", 750m, 750m);
		}

		public void Test_ImportHandleAppMethod_OneConsolCostInDBAndAppMethodIsNotSpecifiedInLine()
		{
			AssertApportionmentMethodForImportingTxnLine(true, false, false, Xsd.TxnLineConsolApportionmentMethod.SHP, "MAN", 1350m, 150m);
		}

		public void Test_ImportHandleAppMethod_TwoConsolCostInDBAndAppMethodIsNotSpecifiedInLine()
		{
			AssertApportionmentMethodForImportingTxnLine(true, true, false, Xsd.TxnLineConsolApportionmentMethod.SHP, "MAN", 1350m, 150m);
		}

		public void Test_ImportHandleAppMethod_ConsolCostNotExistsAndAppMethodIsSpecifiedInLine()
		{
			AssertApportionmentMethodForImportingTxnLine(false, false, true, Xsd.TxnLineConsolApportionmentMethod.SHP, "SHP", 750m, 750m);
		}

		public void Test_ImportHandleAppMethod_ConsolCostNotExistsAndAppMethodIsNotSpecifiedInLine()
		{
			//Fall back to registry value
			AssertApportionmentMethodForImportingTxnLine(false, false, false, Xsd.TxnLineConsolApportionmentMethod.SHP, "CHG", 1000m, 500m);
		}

		public void TestImportTxnHeaderUsingConsolNumberAndApportionmentMethod()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment1 = testObjectCreator.CreateShipment("S001001", consol);
			var shipment1Job = testObjectCreator.CreateJob(shipment1, false, false);

			var shipment2 = testObjectCreator.CreateShipment("S001002", consol);
			var shipment2Job = testObjectCreator.CreateJob(shipment2, false, false);

			shipment1.JS_ActualWeight = 100M;
			shipment2.JS_ActualWeight = 50M;

			TxnLine.ChargeCode = ObjectCreator.CC1.AC_Code;
			TxnLine.ConsolOrJobType = Xsd.TxnLineConsolOrJobType.CSL;
			TxnLine.ConsolOrJobTypeSpecified = true;
			TxnLine.ConsolOrJobNo = consol.JK_UniqueConsignRef;
			TxnLine.ConsolApportionmentMethod = Xsd.TxnLineConsolApportionmentMethod.GWT;
			TxnLine.HouseBIllNo = ZString.Empty;
			TxnLine.MasterBillNo = ZString.Empty;
			TxnLine.OsInvoiceAmtExclTax = TxnHeaderMapper.GetXmlFinancialValue(150M, ObjectCreator.USD, typeof(APInvoice));

			Factory.Save();

			Invoice = Factory.New<APInvoice>();

			TransactionBuilderConfig config = new TransactionBuilderConfig();
			ApportionmentBuilder builder = new ApportionmentBuilder(new NotificationManager(new NotificationBuffer()), config);

			builder.AddApportionmentToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");

			AssertEquals(1, Invoice.ConsolCosting.ConsolCosts.Count);
			AssertEquals("Invoice Lines Count", 2, Invoice.Lines.Count);

			AssertEquals("GWT", Invoice.ConsolCosting.ConsolCosts[0].E6_ApportionmentMethod);

			ZQuery invoiceLineForShipment1Filter = new ZQuery(AccTransactionLinesSchema.AL_AH, Invoice.PK);
			invoiceLineForShipment1Filter.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, shipment1Job.PK);
			InvoicingLineBase invoiceLineForShipment1 = (InvoicingLineBase)Factory.LoadTop1(typeof(APInvoiceLine), invoiceLineForShipment1Filter);

			AssertNotNull("There should be an invoice line relating to job 1", invoiceLineForShipment1);
			AssertEquals("Invoice Line Value for Shipment 1 Value", 100M, invoiceLineForShipment1.AL_OSExTaxAmount);

			ZQuery invoiceLineForShipment2Filter = new ZQuery(AccTransactionLinesSchema.AL_AH, Invoice.PK);
			invoiceLineForShipment2Filter.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, shipment2Job.PK);
			InvoicingLineBase invoiceLineForShipment2 = (InvoicingLineBase)Factory.LoadTop1(typeof(APInvoiceLine), invoiceLineForShipment2Filter);

			AssertNotNull("There should be an invoice line relating to job 2", invoiceLineForShipment2);
			AssertEquals("Invoice Line for Shipment 2 Value", 50M, invoiceLineForShipment2.AL_OSExTaxAmount);
		}

		public void TestImportTxnHeaderUsingMasterBillNumber()
		{
			TxnLine.ChargeCode = ObjectCreator.CC1.AC_Code;
			TxnLine.ConsolOrJobType = Xsd.TxnLineConsolOrJobType.CSL;
			TxnLine.ConsolOrJobTypeSpecified = true;
			TxnLine.ConsolOrJobNo = ZString.Empty;
			TxnLine.HouseBIllNo = ZString.Empty;
			TxnLine.MasterBillNo = Consol1.JK_MasterBillNum;
			TxnLine.OsInvoiceAmtExclTax = TxnHeaderMapper.GetXmlFinancialValue(200M, ObjectCreator.USD, typeof(APInvoice));

			Factory.Save();

			Builder.AddApportionmentToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");

			AssertEquals("Invoice Lines Count", 2, Invoice.Lines.Count);

			ZQuery invoiceLineForShipment1Filter = new ZQuery(AccTransactionLinesSchema.AL_AH, Invoice.PK);
			invoiceLineForShipment1Filter.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, Job1.PK);
			InvoicingLineBase invoiceLineForShipment1 = (InvoicingLineBase)Factory.LoadTop1(typeof(APInvoiceLine), invoiceLineForShipment1Filter);

			AssertNotNull("There should be an invoice line relating to job 1", invoiceLineForShipment1);
			AssertEquals("Invoice Line Value for Shipment 1 Value", 100M, invoiceLineForShipment1.AL_OSExTaxAmount);

			ZQuery invoiceLineForShipment2Filter = new ZQuery(AccTransactionLinesSchema.AL_AH, Invoice.PK);
			invoiceLineForShipment2Filter.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, Job2.PK);
			InvoicingLineBase invoiceLineForShipment2 = (InvoicingLineBase)Factory.LoadTop1(typeof(APInvoiceLine), invoiceLineForShipment2Filter);

			AssertNotNull("There should be an invoice line relating to job 2", invoiceLineForShipment2);
			AssertEquals("Invoice Line for Shipment 2 Value", 100M, invoiceLineForShipment2.AL_OSExTaxAmount);
		}

		public void TestImportValidChargeCode()
		{
			ZString chargeCodeThatDoesExist = ObjectCreator.CC1.AC_Code;
			Factory.Save();

			ZQuery chargeCodeFilter = new ZQuery(ViewGenericChargeSchema.VC_Code, chargeCodeThatDoesExist);
			chargeCodeFilter.AddToFilter(ViewGenericChargeSchema.VC_GC, GlbCompany.CurrentCompany.PK);
			chargeCodeFilter.AddToFilter(ViewGenericChargeSchema.VC_IsGLAccount, ZBool.False);
			AssertEquals("Precondition: Charge Code exists", 1, Factory.GetDatabaseCount(typeof(GenericCharge), chargeCodeFilter));

			TxnLine.ChargeCode = chargeCodeThatDoesExist;
			TxnLine.Department = "FIA";

			Builder.AddApportionmentToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1:");

			AssertEquals("TransactionHeader Lines Count", 2, Invoice.Lines.Count);
			AssertNotNull("TransactionHeader Line 1: Charge Code should not be null", Invoice.Lines[0].ChargeCode);
			AssertEquals("TransactionHeader Line 1: Charge Code", ObjectCreator.CC1.AC_Code, Invoice.Lines[0].ChargeCode.AC_Code);
			AssertNotNull("TransactionHeader Line 2: Charge Code should not be null", Invoice.Lines[1].ChargeCode);
			AssertEquals("TransactionHeader Line 2: Charge Code", ObjectCreator.CC1.AC_Code, Invoice.Lines[1].ChargeCode.AC_Code);
		}

		public void TestImportValidChargeCodeWhenMappingExistsForCreditor()
		{
			AssertChargeCodeMappingForOrganisation();
		}

		protected void AssertChargeCodeMappingForOrganisation()
		{
			ZString chargeCodeThatDoesExist = ObjectCreator.CC1.AC_Code;
			ZString chargeCodeToMapTo = ObjectCreator.CC2.AC_Code;
			OrgHeader org = GetOrgForChargeCodeMappingTest();
			OrgPatternMatchOverride patternMatchOverride = org.CreatePatternMatchOverrideForTest();
			patternMatchOverride.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
			patternMatchOverride.OO_LocalCode = chargeCodeToMapTo;
			patternMatchOverride.OO_ForeignCode = chargeCodeThatDoesExist;

			Factory.Save();

			ZQuery chargeCodeFilter = new ZQuery(ViewGenericChargeSchema.VC_Code, chargeCodeThatDoesExist);
			chargeCodeFilter.AddToFilter(ViewGenericChargeSchema.VC_GC, GlbCompany.CurrentCompany.PK);
			chargeCodeFilter.AddToFilter(ViewGenericChargeSchema.VC_IsGLAccount, ZBool.False);
			AssertEquals("Precondition: Charge Code exists", 1, Factory.GetDatabaseCount(typeof(GenericCharge), chargeCodeFilter));

			SetXMLInvoiceLineChargeCode(TxnLine, chargeCodeThatDoesExist);
			TxnLine.Department = "FIA";
			Invoice.AH_OH = ObjectCreator.ABIGAS.PK;

			Builder.AddApportionmentToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1:");

			AssertEquals("TransactionHeader Lines Count", 2, Invoice.Lines.Count);
			AssertNotNull("TransactionHeader Line 1: Charge Code should not be null", Invoice.Lines[0].ChargeCode);
			AssertEquals("TransactionHeader Line 1: Charge Code", ObjectCreator.CC2.AC_Code, Invoice.Lines[0].ChargeCode.AC_Code);
			AssertNotNull("TransactionHeader Line 2: Charge Code should not be null", Invoice.Lines[1].ChargeCode);
			AssertEquals("TransactionHeader Line 2: Charge Code", ObjectCreator.CC2.AC_Code, Invoice.Lines[1].ChargeCode.AC_Code);
		}

		public void TestImportInvalidChargeCode_DoesNotExist()
		{
			ZString chargeCodeThatDoesNotExist = "ZZZZZ";
			AssertEquals("Precondition: ZZZZZ Charge Code does not exist", 0, Factory.GetDatabaseCount(typeof(AccChargeCode), new ZQuery(AccChargeCodeSchema.AC_Code, chargeCodeThatDoesNotExist)));

			TxnLine.ChargeCode = chargeCodeThatDoesNotExist;
			Builder.AddApportionmentToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");

			TestHelper.AssertNotificationsContainsErrorMessage(Notify, "Error: Transaction AP INV 00001000: Line 1: No matches were found for the following Charge Code: ZZZZZ");
		}

		public void TestChargeCodeOrganisationMapping()
		{
			AccChargeCode chargeCodeThatDoesExist = ObjectCreator.CC1;
			AccChargeCode chargeCodeToMapTo = ObjectCreator.CC2;
			OrgHeader org = GetOrgForChargeCodeMappingTest();

			GlobalChargeCodeMapOrganization globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapOrganization>();
			globalChargeCode.YG_OH = org.PK;
			globalChargeCode.YG_Code = chargeCodeThatDoesExist.AC_Code;
			GlobalChargeCodeMapPivotOrganization pivot = globalChargeCode.PivotCollection.AddNew();
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			pivot.YP_AC = chargeCodeToMapTo.PK;

			Factory.Save();

			SetXMLInvoiceLineChargeCode(TxnLine, chargeCodeThatDoesExist.AC_Code);
			TxnLine.Department = "FIA";
			Invoice.AH_OH = ObjectCreator.ABIGAS.PK;

			Builder.AddApportionmentToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1:");

			AssertEquals("TransactionHeader Lines Count", 2, Invoice.Lines.Count);
			AssertNotNull("TransactionHeader Line 1: Charge Code should not be null", Invoice.Lines[0].ChargeCode);
			AssertEquals("TransactionHeader Line 1: Charge Code", ObjectCreator.CC2.AC_Code, Invoice.Lines[0].ChargeCode.AC_Code);
			AssertNotNull("TransactionHeader Line 2: Charge Code should not be null", Invoice.Lines[1].ChargeCode);
			AssertEquals("TransactionHeader Line 2: Charge Code", ObjectCreator.CC2.AC_Code, Invoice.Lines[1].ChargeCode.AC_Code);
		}

		public void TestChargeCodeIntercompanyMapping()
		{
			AccChargeCode chargeCodeThatDoesExist = ObjectCreator.CC1;
			AccChargeCode chargeCodeToMapTo = ObjectCreator.CC2;
			OrgHeader org = GetOrgForChargeCodeMappingTest();
			GlbCompany aRCompany = Factory.NewWithValidTestData<GlbCompany>();
			aRCompany.GC_OH_OrgProxy = org.PK;
			GlbBranch aRCompanyBranch = aRCompany.Branches.AddNew();
			aRCompanyBranch.GB_OH_OrgProxy = ObjectCreator.ZECTRA.PK;
			chargeCodeThatDoesExist.AC_GC = aRCompany.PK;

			GlobalChargeCodeMapIntercompany globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			GlobalChargeCodeMapPivotIntercompany aPPivot = globalChargeCode.PivotCollection.AddNew();
			aPPivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			aPPivot.YP_AC = chargeCodeToMapTo.PK;
			GlobalChargeCodeMapPivotIntercompany aRPivot = globalChargeCode.PivotCollection.AddNew();
			aRPivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			aRPivot.YP_AC = chargeCodeThatDoesExist.PK;

			Factory.Save();

			SetXMLInvoiceLineChargeCode(TxnLine, chargeCodeThatDoesExist.AC_Code);
			TxnLine.Department = "FIA";
			Invoice.AH_OH = ObjectCreator.ABIGAS.PK;

			Builder.AddApportionmentToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1:");

			AssertEquals("TransactionHeader Lines Count", 2, Invoice.Lines.Count);
			AssertNotNull("TransactionHeader Line 1: Charge Code should not be null", Invoice.Lines[0].ChargeCode);
			AssertEquals("TransactionHeader Line 1: Charge Code", ObjectCreator.CC2.AC_Code, Invoice.Lines[0].ChargeCode.AC_Code);
			AssertNotNull("TransactionHeader Line 2: Charge Code should not be null", Invoice.Lines[1].ChargeCode);
			AssertEquals("TransactionHeader Line 2: Charge Code", ObjectCreator.CC2.AC_Code, Invoice.Lines[1].ChargeCode.AC_Code);

			Invoice = Factory.NewWithValidTestData<APInvoice>();
			Invoice.AH_OH = ObjectCreator.ZECTRA.PK;

			Builder.AddApportionmentToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1:");

			AssertEquals("TransactionHeader Lines Count", 2, Invoice.Lines.Count);
			AssertNotNull("TransactionHeader Line 1: Charge Code should not be null", Invoice.Lines[0].ChargeCode);
			AssertEquals("TransactionHeader Line 1: Charge Code", ObjectCreator.CC2.AC_Code, Invoice.Lines[0].ChargeCode.AC_Code);
			AssertNotNull("TransactionHeader Line 2: Charge Code should not be null", Invoice.Lines[1].ChargeCode);
			AssertEquals("TransactionHeader Line 2: Charge Code", ObjectCreator.CC2.AC_Code, Invoice.Lines[1].ChargeCode.AC_Code);
		}

		public void TestImportValidTaxRate()
		{
			TxnLine.TaxCode = ObjectCreator.GST1.AT_Code;
			Factory.Save();
			Builder.AddApportionmentToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, ZString.Empty);
			AssertEquals("Tax Code", ObjectCreator.GST1.AT_Code, Invoice.Lines[0].TaxRate.AT_Code);
		}

		public void TestImportInvalidTaxRate()
		{
			TxnLine.TaxCode = "BLAHBLAH";
			Builder.AddApportionmentToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");
			string expectedMessage = "Error: Transaction AP INV 00001000: Line 1: No matches were found for the following Tax Rate: BLAHBLAH";
			TestHelper.AssertNotificationsContainsErrorMessage(Notify, expectedMessage);
		}

		public void TestSetTaxAmount()
		{
			Builder.AddApportionmentToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");
			Assert(!Notify.HasErrors);
			AssertEquals("Apportionment Tax Amount", 0m, Invoice.Lines[0].AL_OSTaxAmount);
			AssertEquals("Apportionment Tax Amount", 0m, Invoice.Lines[1].AL_OSTaxAmount);

			TxnLine.OsTaxAmount = TxnHeaderMapper.GetXmlFinancialValue(10M, ObjectCreator.AUD, typeof(APInvoice));
			Builder.AddApportionmentToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");
			TestHelper.AssertNotificationsContainsErrorMessage(Notify, "Consol Cost Tax Amount cannot be set if there is no Tax Code");
			AssertEquals("Apportionment Tax Amount", 0m, Invoice.Lines[2].AL_OSTaxAmount);
			AssertEquals("Apportionment Tax Amount", 0m, Invoice.Lines[3].AL_OSTaxAmount);

			Notify.Clear();
			TxnLine.TaxCode = ObjectCreator.GSTFREE1.AT_Code;
			Builder.AddApportionmentToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");
			TestHelper.AssertNotificationsContainsErrorMessage(Notify, "Consol Cost Tax Amount cannot be set if Tax Rate is zero");
			AssertEquals("Apportionment Tax Amount", 0m, Invoice.Lines[4].AL_OSTaxAmount);
			AssertEquals("Apportionment Tax Amount", 0m, Invoice.Lines[5].AL_OSTaxAmount);

			Notify.Clear();
			TxnLine.TaxCode = ObjectCreator.GST1.AT_Code;
			Builder.AddApportionmentToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");
			Assert(!Notify.HasErrors);
			AssertEquals("Apportionment Tax Amount", 5m, Invoice.Lines[6].AL_OSTaxAmount);
			AssertEquals("Apportionment Tax Amount", 5m, Invoice.Lines[7].AL_OSTaxAmount);
		}

		#region TestOverrideSystemExchangeRate

		public void TestOverrideSystemExchangeRate_UseJobExRate()
		{
			AssertOverrideSystemExchangeRate(true);
		}

		public void TestOverrideSystemExchangeRate()
		{
			AssertOverrideSystemExchangeRate(false);
		}

		public void AssertOverrideSystemExchangeRate(bool expectedPostedToEFTValue)
		{
			AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, expectedPostedToEFTValue);

			Shipment1.JS_ActualChargeable = 50;
			Shipment2.JS_ActualChargeable = 150;

			ObjectCreator.CreateExchangeRate(ObjectCreator.USD, 0.5M);
			TxnLine.OverrideSystemExchangeRate = true;
			TxnLine.OsInvoiceAmtExclTax = TxnHeaderMapper.GetXmlFinancialValue(200M, ObjectCreator.USD, typeof(APInvoice));
			TxnLine.LocalInvoiceAmtExclTax = TxnHeaderMapper.GetXmlFinancialValue(70M, ObjectCreator.USD, typeof(APInvoice));

			Invoice = Factory.New<APInvoice>();
			Invoice.ExchangeRate.Currency = TxnLine.OsInvoiceAmtExclTax.CurrencyCode;
			AssertEquals("Precondition: AH_PostedToEFT", expectedPostedToEFTValue, Invoice.AH_PostedToEFT);
			AssertEquals("Precondition: Invoice.AH_ExchangeRate before import", 0.5M, Invoice.AH_ExchangeRate);
			Builder.AddApportionmentToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");
			Assert(!Notify.HasErrors);

			AssertEquals("Invoice Lines Count", 2, Invoice.Lines.Count);

			var invoiceLineForShipment1Filter = new ZQuery(AccTransactionLinesSchema.AL_AH, Invoice.PK);
			invoiceLineForShipment1Filter.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, Job1.PK);
			var invoiceLineForShipment1 = Factory.LoadTop1<APInvoiceLine>(invoiceLineForShipment1Filter);

			AssertNotNull("There should be an invoice line relating to job 1", invoiceLineForShipment1);
			AssertEquals("Invoice Line Exchange Rate for Shipment 1", 2.857143M, invoiceLineForShipment1.AL_ExchangeRate);
			AssertEquals("Invoice Line OS Amount for Shipment 1", 50M, invoiceLineForShipment1.AL_OSExTaxAmount);
			AssertEquals("Invoice Line Local Amount for Shipment 1", 17.5M, invoiceLineForShipment1.AL_LocalExTaxAmount);

			var invoiceLineForShipment2Filter = new ZQuery(AccTransactionLinesSchema.AL_AH, Invoice.PK);
			invoiceLineForShipment2Filter.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, Job2.PK);
			var invoiceLineForShipment2 = Factory.LoadTop1<APInvoiceLine>(invoiceLineForShipment2Filter);

			AssertNotNull("There should be an invoice line relating to job 2", invoiceLineForShipment2);
			AssertEquals("Invoice Line Exchange Rate for Shipment 2", 2.857143M, invoiceLineForShipment2.AL_ExchangeRate);
			AssertEquals("Invoice Line OS Amount for Shipment 2", 150M, invoiceLineForShipment2.AL_OSExTaxAmount);
			AssertEquals("Invoice Line Local Amount for Shipment 2", 52.5M, invoiceLineForShipment2.AL_LocalExTaxAmount);

			TxnLine.OverrideSystemExchangeRate = false;
			Invoice = Factory.New<APInvoice>();
			Invoice.ExchangeRate.Currency = TxnLine.OsInvoiceAmtExclTax.CurrencyCode;
			AssertEquals("Precondition: AH_PostedToEFT", expectedPostedToEFTValue, Invoice.AH_PostedToEFT);
			AssertEquals("Precondition: Invoice.AH_ExchangeRate before import", 0.5M, Invoice.AH_ExchangeRate);
			Builder.AddApportionmentToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");

			AssertEquals("Invoice Lines Count", 2, Invoice.Lines.Count);

			invoiceLineForShipment1Filter = new ZQuery(AccTransactionLinesSchema.AL_AH, Invoice.PK);
			invoiceLineForShipment1Filter.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, Job1.PK);
			invoiceLineForShipment1 = Factory.LoadTop1<APInvoiceLine>(invoiceLineForShipment1Filter);

			AssertNotNull("There should be an invoice line relating to job 1", invoiceLineForShipment1);
			AssertEquals("Invoice Line Exchange Rate for Shipment 1", 0.5M, invoiceLineForShipment1.AL_ExchangeRate);
			AssertEquals("Invoice Line OS Amount for Shipment 1", 50M, invoiceLineForShipment1.AL_OSExTaxAmount);
			AssertEquals("Invoice Line Local Amount for Shipment 1", 100M, invoiceLineForShipment1.AL_LocalExTaxAmount);

			invoiceLineForShipment2Filter = new ZQuery(AccTransactionLinesSchema.AL_AH, Invoice.PK);
			invoiceLineForShipment2Filter.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, Job2.PK);
			invoiceLineForShipment2 = Factory.LoadTop1<APInvoiceLine>(invoiceLineForShipment2Filter);

			AssertNotNull("There should be an invoice line relating to job 2", invoiceLineForShipment2);
			AssertEquals("Invoice Line Exchange Rate for Shipment 2", 0.5M, invoiceLineForShipment2.AL_ExchangeRate);
			AssertEquals("Invoice Line OS Amount for Shipment 2", 150M, invoiceLineForShipment2.AL_OSExTaxAmount);
			AssertEquals("Invoice Line Local Amount for Shipment 2", 300M, invoiceLineForShipment2.AL_LocalExTaxAmount);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			Invoice = Factory.New<APInvoice>();
			TestObjectCreator.FillInvoiceWithMinimumTestData(Invoice);
			Notify = new NotificationBuffer();
			TransactionBuilderConfig config = new TransactionBuilderConfig();
			config.RunExtraValidation = false;
			Builder = new ApportionmentBuilder(new NotificationManager(Notify), config);
			ObjectCreator = new TestObjectCreator(Factory);
			TestHelper = new NotificationTestHelper();

			Consol1 = ObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001234");
			Shipment1 = ObjectCreator.CreateShipment("S00001001", "AUSYD", "NZAKL", Consol1);
			Job1 = ObjectCreator.CreateJob(Shipment1);
			Shipment2 = ObjectCreator.CreateShipment("S00001002", "AUSYD", "NZAKL", Consol1);
			Job2 = ObjectCreator.CreateJob(Shipment2);

			TxnLine = new Xsd.TxnLine();
			TxnLine.ChargeCode = ObjectCreator.CC1.AC_Code;
			TxnLine.ConsolOrJobType = Xsd.TxnLineConsolOrJobType.CSL;
			TxnLine.ConsolOrJobTypeSpecified = true;
			TxnLine.ConsolOrJobNo = Consol1.JK_UniqueConsignRef;
			TxnLine.HouseBIllNo = ZString.Empty;
			TxnLine.MasterBillNo = ZString.Empty;
			TxnLine.OsInvoiceAmtExclTax = TxnHeaderMapper.GetXmlFinancialValue(200M, ObjectCreator.USD, typeof(APInvoice));

			Factory.Save();

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		protected override void TearDown()
		{
			Job1.Dispose();
			Job2.Dispose();
			base.TearDown();
		}

		ValueObjectImportContext ImportContext
		{
			get
			{
				if (fImportContext == null)
				{
					fImportContext = new ValueObjectImportContext(Factory, Notify);
				}
				return fImportContext;
			}
		}
		ValueObjectImportContext fImportContext;

		protected virtual void SetXMLInvoiceLineChargeCode(Xsd.TxnLine txnLine, ZString chargeCodeThatDoesExist)
		{
			txnLine.ChargeCode = chargeCodeThatDoesExist;
		}

		protected virtual OrgHeader GetOrgForChargeCodeMappingTest()
		{
			return ObjectCreator.ABIGAS;
		}

		protected virtual void GetNewLineBuilder()
		{
			Builder = new ApportionmentBuilder(new NotificationManager(Notify), new TransactionBuilderConfig());
		}

		protected ApportionmentBuilder Builder;
		protected NotificationBuffer Notify;
		Xsd.TxnLine TxnLine;
		APInvoice Invoice;
		protected TestObjectCreator ObjectCreator;
		NotificationTestHelper TestHelper;

		ForwardingConsol Consol1;
		ForwardingShipment Shipment1;
		Job Job1;
		ForwardingShipment Shipment2;
		Job Job2;

		#endregion
	}
}
