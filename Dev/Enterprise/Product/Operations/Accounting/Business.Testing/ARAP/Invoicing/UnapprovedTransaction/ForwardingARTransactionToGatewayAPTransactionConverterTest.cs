using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing.UnapprovedTransaction
{
	public class ForwardingARTransactionToGatewayAPTransactionConverterTest : ARTransactionToAPTransactionConverterBaseTest
	{
		[TestDate(2015, 01, 01)]
		public void TestImportingAPInvoicesFromSisterCompanies_GatewayConsolToGatewayConsolWithConsolInvoice()
		{
			var sendingGatewayCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("GTWORG", true, false);
			var sendingGatewayCompany = TestObjectCreator.CreateNewCompany("CGA", orgProxy: sendingGatewayCompanyOrgProxy);
			var sendingGatewayBranch = TestObjectCreator.CreateNewBranch(sendingGatewayCompany, "BGA");
			var receivingGatewayDepartment = Factory.Load<GlbDepartment>(GlbDepartment.CurrentDepartment.PK);
			receivingGatewayDepartment.GE_Misc = false;

			periodManagementTestHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year, GlbCompany.CurrentCompany.PK);
			periodManagementTestHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year, sendingGatewayCompany.PK);

			var chargeCode1 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1 for sending company", ChargeType.Margin, 100, TestObjectCreator.GSTFREE1, TestObjectCreator.WHTFREE1, sendingGatewayCompany);
			TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1 for receiving company", ChargeType.Margin, 100, TestObjectCreator.GSTFREE1, TestObjectCreator.WHTFREE1);
			var chargeCode2 = TestObjectCreator.CreateChargeCode("CC2", "Charge Code 2 for sending company", ChargeType.Margin, 100, TestObjectCreator.GSTFREE1, TestObjectCreator.WHTFREE1, sendingGatewayCompany);
			TestObjectCreator.CreateChargeCode("CC2", "Charge Code 2 for receiving company", ChargeType.Margin, 100, TestObjectCreator.GSTFREE1, TestObjectCreator.WHTFREE1);

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol("USLAX", "AUMEL", "C001", sendingGatewayCompany, GlbCompany.CurrentCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S001", gatewayConsol);
			var shipment2 = TestObjectCreator.CreateShipment("S002", gatewayConsol);
			var gatewayJobInCurrentCompany = TestObjectCreator.CreateJob(gatewayConsol);
			Factory.Save();

			ARInvoice arInvoice = null;
			var receivingGatewayBranchOrgProxy = Factory.Load<OrgHeader>(GlbBranch.CurrentBranch.OrgProxy.PK);
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sendingGatewayBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				receivingGatewayBranchOrgProxy.CompanyData.OB_IsDebtor = true;
				var job1 = new Job.Loader(shipment1).TryCreateWithoutMutexForTestOnly();
				var job2 = new Job.Loader(shipment2).TryCreateWithoutMutexForTestOnly();
				Factory.Save();

				arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1.0m, receivingGatewayBranchOrgProxy);
				arInvoice.AH_ConsolidatedInvoiceRef = gatewayConsol.JK_UniqueConsignRef;
				arInvoice.AH_JH = ZGuid.Empty;

				var arInvoiceLine1 = TestObjectCreator.CreateInvoiceLine(arInvoice, job1, chargeCode1, 190m, TestObjectCreator.AUD, 1.0m);
				var arInvoiceLine2 = TestObjectCreator.CreateInvoiceLine(arInvoice, job2, chargeCode1, 110m, TestObjectCreator.AUD, 1.0m);

				TestObjectCreator.CreateCharge(arInvoiceLine1);
				TestObjectCreator.CreateCharge(arInvoiceLine2);
				Factory.Save();
			}

			var converter = new UnapprovedTransactionConverter(Factory);
			var convertedAPInvoice = converter.ConvertToAP(arInvoice, true);

			AssertEquals(0, convertedAPInvoice.ConsolCosting.ConsolCosts.Count);
			AssertEquals(2, convertedAPInvoice.Lines.Count);

			var invoiceLines = convertedAPInvoice.Lines.Cast<InvoicingLineBase>().ToList();

			AssertContainsExactElementsInAnyOrder(new ZDecimal[] { -190.00m, -110.00m }, invoiceLines.Select(x => x.AL_LineAmount));

			AssertEquals(2, invoiceLines.Count(x => x.AL_JH == gatewayJobInCurrentCompany.PK));

			convertedAPInvoice.RunPreSaveValidation();
			AssertNoErrors(convertedAPInvoice);
		}

		[TestDate(2015, 01, 01)]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestShipmentJobsAreNotCreatedWhenAllLinesAreImportedToGatewayJob()
		{
			SetupSisterCompanyGatewayConsolChargeCodesAndPeriods(out GlbBranch forwardingBranch, out AccChargeCode chargeCode, out ForwardingConsol gatewayConsol, out ForwardingShipment shipment1, out ForwardingShipment shipment2);
			var gatewayJobInCurrentCompany = TestObjectCreator.CreateJob(gatewayConsol);
			gatewayJobInCurrentCompany.MarkLightValidationAsValidForTesting();

			ARInvoice arInvoice;
			var gatewayBranchOrgProxy = Factory.Load<OrgHeader>(GlbBranch.CurrentBranch.OrgProxy.PK);
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, forwardingBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var shipment1Job = TestObjectCreator.CreateJob(shipment1);
				var shipment2Job = TestObjectCreator.CreateJob(shipment2);
				gatewayBranchOrgProxy.CompanyData.OB_IsDebtor = true;
				Factory.Save();

				arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1.0m, gatewayBranchOrgProxy);
				arInvoice.AH_OSTotalAmount = 110m;
				arInvoice.AH_ConsolidatedInvoiceRef = gatewayConsol.JK_UniqueConsignRef;
				Assert("Posted from Forwarding Consol", arInvoice.AH_JH.IsEmpty);

				var aRInvoiceLine1 = TestObjectCreator.CreateInvoiceLine(arInvoice, shipment1Job, chargeCode, 10m, TestObjectCreator.AUD, 1.0m);
				var aRInvoiceLine2 = TestObjectCreator.CreateInvoiceLine(arInvoice, shipment2Job, chargeCode, 20m, TestObjectCreator.AUD, 1.0m);

				TestObjectCreator.CreateCharge(aRInvoiceLine1);
				TestObjectCreator.CreateCharge(aRInvoiceLine2);

				Factory.Save();
			}

			Assert("Precondition : gatewayJob JH_IsValid must be true to simulate GUI problem.", gatewayJobInCurrentCompany.LightValidationIsValid);
			var jobLoadFactory = new BusinessObjectFactory();
			AssertEquals("Precondition: GatewayBillingChargeCodes registry is empty.", ZGuid.Empty.ToString(), AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.Value);
			AssertNull("Precondition: Shipment 1 should not have a job in current company", new JobHeader.Loader(jobLoadFactory, shipment1).Load());
			AssertNull("Precondition: Shipment 2 should not have a job in current company", new JobHeader.Loader(jobLoadFactory, shipment2).Load());

			var converter = new UnapprovedTransactionConverter(Factory);
			var convertedAPInvoice = converter.ConvertToAP(arInvoice, false);

			AssertEquals(0, convertedAPInvoice.ConsolCosting.ConsolCosts.Count);
			var apInvoiceLines = convertedAPInvoice.Lines.Cast<InvoicingLineBase>();
			AssertEquals(2, apInvoiceLines.Count());
			AssertEquals(1, apInvoiceLines.Count(x => x.ChargeCode.AC_Code == chargeCode.AC_Code &&
													  x.AL_LineAmount == -10m &&
													  x.Job.PK == gatewayJobInCurrentCompany.PK &&
													  x.RelatedJobFromIntercompanyInvoiceImport.JobPk == shipment1.PK &&
													  x.RelatedJobFromIntercompanyInvoiceImport.JobNumber == shipment1.JS_UniqueConsignRef));

			AssertEquals(1, apInvoiceLines.Count(x => x.ChargeCode.AC_Code == chargeCode.AC_Code &&
													  x.AL_LineAmount == -20m &&
													  x.Job.PK == gatewayJobInCurrentCompany.PK &&
													  x.RelatedJobFromIntercompanyInvoiceImport.JobPk == shipment2.PK &&
													  x.RelatedJobFromIntercompanyInvoiceImport.JobNumber == shipment2.JS_UniqueConsignRef));

			convertedAPInvoice.RunPreSaveValidation();
			AssertNoErrors(convertedAPInvoice);
			convertedAPInvoice.Factory.Save();

			Assert("Postcondition : AP invoice is posted", convertedAPInvoice.IsPosted);
			AssertNull("Postcondition: Shipment 1 should not have a job in current company", new JobHeader.Loader(jobLoadFactory, shipment1).Load());
			AssertNull("Postcondition: Shipment 2 should not have a job in current company", new JobHeader.Loader(jobLoadFactory, shipment2).Load());
		}

		#region GatewayBillingChargeCodesRegistry Tests

		[TestDate(2015, 01, 01)]
		public void TestImportSisterCompanyInvoiceWithGatewayBillingChargeCodesRegistry_RegistryIsEmpty()
		{
			CreateARInvoiceForGatewayBillingChargeCodesRegistryTest(out _, out ForwardingShipment shipment1, out ForwardingShipment shipment2, out Job gatewayJobInCurrentCompany, out ARInvoice arInvoice);

			AssertEquals("Precondition: GatewayBillingChargeCodes registry is empty.", ZGuid.Empty.ToString(), AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.Value);

			var converter = new UnapprovedTransactionConverter(Factory);
			var convertedAPInvoice = converter.ConvertToAP(arInvoice, true);

			AssertEquals(0, convertedAPInvoice.ConsolCosting.ConsolCosts.Count);
			var apInvoiceLines = convertedAPInvoice.Lines.Cast<InvoicingLineBase>();
			AssertEquals(2, apInvoiceLines.Count());
			AssertEquals(1, apInvoiceLines.Count(x => x.ChargeCode.AC_Code == TestObjectCreator.CC3.AC_Code &&
													  x.AL_LineAmount == -10m &&
													  x.Job.PK == gatewayJobInCurrentCompany.PK &&
													  x.RelatedJobFromIntercompanyInvoiceImport.JobPk == shipment1.PK &&
													  x.RelatedJobFromIntercompanyInvoiceImport.JobNumber == shipment1.JS_UniqueConsignRef));

			AssertEquals(1, apInvoiceLines.Count(x => x.ChargeCode.AC_Code == TestObjectCreator.CC6.AC_Code &&
													  x.AL_LineAmount == -20m &&
													  x.Job.PK == gatewayJobInCurrentCompany.PK &&
													  x.RelatedJobFromIntercompanyInvoiceImport.JobPk == shipment2.PK &&
													  x.RelatedJobFromIntercompanyInvoiceImport.JobNumber == shipment2.JS_UniqueConsignRef));

			convertedAPInvoice.RunPreSaveValidation();
			AssertNoErrors(convertedAPInvoice);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportSisterCompanyInvoiceWithGatewayBillingChargeCodesRegistry_AllChargeCodesAreNotGatewayBillingRelated()
		{
			CreateARInvoiceForGatewayBillingChargeCodesRegistryTest(out _, out ForwardingShipment shipment1, out ForwardingShipment shipment2, out _, out ARInvoice arInvoice);

			var converter = new UnapprovedTransactionConverter(Factory);
			using (AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, TestObjectCreator.FRT.PK.ToString()))
			{
				var convertedAPInvoice = converter.ConvertToAP(arInvoice, true);
				AssertEquals(0, convertedAPInvoice.ConsolCosting.ConsolCosts.Count);
				var apInvoiceLines = convertedAPInvoice.Lines.Cast<InvoicingLineBase>();
				AssertEquals(2, apInvoiceLines.Count());
				AssertEquals(1, apInvoiceLines.Count(x => x.ChargeCode.AC_Code == TestObjectCreator.CC2.AC_Code &&
														  x.AL_LineAmount == -10m &&
														  x.Job.PK == shipment1.Job.PK &&
														  x.RelatedJobFromIntercompanyInvoiceImport.JobPk == ZGuid.Empty &&
														  x.RelatedJobFromIntercompanyInvoiceImport.JobNumber == ZString.Empty));

				AssertEquals(1, apInvoiceLines.Count(x => x.ChargeCode.AC_Code == TestObjectCreator.CC4.AC_Code &&
														  x.AL_LineAmount == -20m &&
														  x.Job.PK == shipment2.Job.PK &&
														  x.RelatedJobFromIntercompanyInvoiceImport.JobPk == ZGuid.Empty &&
														  x.RelatedJobFromIntercompanyInvoiceImport.JobNumber == ZString.Empty));

				convertedAPInvoice.RunPreSaveValidation();
				AssertNoErrors(convertedAPInvoice);
			}
		}

		[TestDate(2015, 01, 01)]
		public void TestImportSisterCompanyInvoiceWithGatewayBillingChargeCodesRegistry_SomeChargeCodesAreGatewayBillingRelatedAndSomeChargeCodesNotGatewayRelated()
		{
			CreateARInvoiceForGatewayBillingChargeCodesRegistryTest(out _, out ForwardingShipment shipment1, out ForwardingShipment shipment2, out Job gatewayJobInCurrentCompany, out ARInvoice arInvoice);

			var converter = new UnapprovedTransactionConverter(Factory);
			using (AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, TestObjectCreator.CC3.PK.ToString()))
			{
				var convertedAPInvoice = converter.ConvertToAP(arInvoice, true);
				AssertEquals(0, convertedAPInvoice.ConsolCosting.ConsolCosts.Count);
				var apInvoiceLines = convertedAPInvoice.Lines.Cast<InvoicingLineBase>();
				AssertEquals(2, apInvoiceLines.Count());
				AssertEquals(1, apInvoiceLines.Count(x => x.ChargeCode.AC_Code == TestObjectCreator.CC3.AC_Code &&
														  x.AL_LineAmount == -10m &&
														  x.Job.PK == gatewayJobInCurrentCompany.PK &&
														  x.RelatedJobFromIntercompanyInvoiceImport.JobPk == shipment1.PK &&
														  x.RelatedJobFromIntercompanyInvoiceImport.JobNumber == shipment1.JS_UniqueConsignRef));

				AssertEquals(1, apInvoiceLines.Count(x => x.ChargeCode.AC_Code == TestObjectCreator.CC4.AC_Code &&
														  x.AL_LineAmount == -20m &&
														  x.Job.PK == shipment2.Job.PK &&
														  x.RelatedJobFromIntercompanyInvoiceImport.JobPk == ZGuid.Empty &&
														  x.RelatedJobFromIntercompanyInvoiceImport.JobNumber == ZString.Empty));

				convertedAPInvoice.RunPreSaveValidation();
				AssertNoErrors(convertedAPInvoice);
			}
		}

		[TestDate(2015, 01, 01)]
		public void TestImportSisterCompanyInvoiceWithGatewayBillingChargeCodesRegistry_DoNotCreateGatewayJobIfNotRequired()
		{
			CreateARInvoiceForGatewayBillingChargeCodesRegistryTest(out ForwardingConsol gatewayConsol, out ForwardingShipment shipment1, out ForwardingShipment shipment2, out _, out ARInvoice arInvoice, shouldCreateGatewayJob: false);
			AssertGatewayJobDoesNotExistInCurrentCompany("Precondition : Gateway Job should not exist in current company before importing AP invoice", gatewayConsol.PK);

			var converter = new UnapprovedTransactionConverter(Factory);
			using (AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, TestObjectCreator.CC3.PK.ToString()))
			{
				var convertedAPInvoice = converter.ConvertToAP(arInvoice, true);
				AssertEquals(0, convertedAPInvoice.ConsolCosting.ConsolCosts.Count);
				var apInvoiceLines = convertedAPInvoice.Lines.Cast<InvoicingLineBase>();
				AssertEquals(2, apInvoiceLines.Count());
				AssertEquals(1, apInvoiceLines.Count(x => x.ChargeCode.AC_Code == TestObjectCreator.CC2.AC_Code &&
														  x.AL_LineAmount == -10m &&
														  x.Job.PK == shipment1.Job.PK &&
														  x.RelatedJobFromIntercompanyInvoiceImport.JobPk == ZGuid.Empty &&
														  x.RelatedJobFromIntercompanyInvoiceImport.JobNumber == ZString.Empty));

				AssertEquals(1, apInvoiceLines.Count(x => x.ChargeCode.AC_Code == TestObjectCreator.CC4.AC_Code &&
														  x.AL_LineAmount == -20m &&
														  x.Job.PK == shipment2.Job.PK &&
														  x.RelatedJobFromIntercompanyInvoiceImport.JobPk == ZGuid.Empty &&
														  x.RelatedJobFromIntercompanyInvoiceImport.JobNumber == ZString.Empty));

				convertedAPInvoice.RunPreSaveValidation();
				AssertNoErrors(convertedAPInvoice);

				var gatewayJob = convertedAPInvoice.LineJobsWithMutex.FirstOrDefault(x => x.JH_ParentID == gatewayConsol.PK);
				AssertNotNull("Gateway job should be created and available in memory", gatewayJob);
				convertedAPInvoice.Factory.Save();

				AssertGatewayJobDoesNotExistInCurrentCompany("Postcondition : Gateway Job should not exist in current company after posting AP invoice", gatewayConsol.PK);
			}

			void AssertGatewayJobDoesNotExistInCurrentCompany(string message, ZGuid gatewayConsolPK)
			{
				var gatewayJobQuery = new ZQuery(JobHeaderSchema.JH_ParentID, gatewayConsolPK).AddToFilter(JobHeaderSchema.JH_GC, Env.CurrentCompanyPK);
				var gatewayJobInCurrentCompany = new BusinessObjectFactory().LoadTop1<Job>(gatewayJobQuery);
				AssertNull(message, gatewayJobInCurrentCompany);
			}
		}

		[TestDate(2015, 01, 01)]
		public void TestImportSisterCompanyInvoiceWithGatewayBillingChargeCodesRegistry_WhenChargeCodeIsNotGatewayClearInvoiceLineDescriptionChargeCodeAndGenericChargeBeforeSettingInvoicingJobToShipment()
		{
			CreateARInvoiceForGatewayBillingChargeCodesRegistryTest(out _, out ForwardingShipment shipment1, out ForwardingShipment shipment2, out _, out ARInvoice arInvoice, shouldCreateChargeCodeMappingWithoutLocalClient: false);

			var converter = new UnapprovedTransactionConverter(Factory);
			using (AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, TestObjectCreator.FRT.PK.ToString()))
			{
				var convertedAPInvoice = converter.ConvertToAP(arInvoice, true);
				AssertEquals(0, convertedAPInvoice.ConsolCosting.ConsolCosts.Count);
				var apInvoiceLines = convertedAPInvoice.Lines.Cast<InvoicingLineBase>();
				AssertEquals(2, apInvoiceLines.Count());
				AssertEquals(1, apInvoiceLines.Count(x => x.AL_AC == TestObjectCreator.CC2.PK &&
														  x.AL_Desc == TestObjectCreator.CC2.AC_Desc &&
														  x.AL_LineAmount == -10m &&
														  x.Job.PK == shipment1.Job.PK &&
														  x.RelatedJobFromIntercompanyInvoiceImport.JobPk == ZGuid.Empty &&
														  x.RelatedJobFromIntercompanyInvoiceImport.JobNumber == ZString.Empty));

				AssertEquals(1, apInvoiceLines.Count(x => x.AL_AC == ZGuid.Empty &&
														  x.AL_Desc == "XYZ Charge Code" &&
														  x.AL_LineAmount == -20m &&
														  x.Job.PK == shipment2.Job.PK &&
														  x.RelatedJobFromIntercompanyInvoiceImport.JobPk == ZGuid.Empty &&
														  x.RelatedJobFromIntercompanyInvoiceImport.JobNumber == ZString.Empty));

				convertedAPInvoice.RunPreSaveValidation();
				AssertHasError(apInvoiceLines.First(x => x.AL_AC == ZGuid.Empty).AL_ACInfo, TransactionLine.EmptyChargeCodeAndGLHeaderError);
			}
		}

		[TestDate(2015, 01, 01)]
		public void TestImportSisterCompanyInvoiceWithGatewayBillingChargeCodesRegistry_WhenMatchingChargeCodeIsNotAvailable_RegistryEmpty()
		{
			CreateARInvoiceForGatewayBillingChargeCodesRegistryTest(out _, out ForwardingShipment shipment1, out ForwardingShipment shipment2, out Job gatewayJobInCurrentCompany, out ARInvoice arInvoice, shouldCreateChargeCodeMapping: false);

			AssertEquals("Precondition: GatewayBillingChargeCodes registry is empty.", ZGuid.Empty.ToString(), AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.Value);

			var converter = new UnapprovedTransactionConverter(Factory);
			var convertedAPInvoice = converter.ConvertToAP(arInvoice, true);

			AssertEquals(0, convertedAPInvoice.ConsolCosting.ConsolCosts.Count);
			var apInvoiceLines = convertedAPInvoice.Lines.Cast<InvoicingLineBase>();
			AssertEquals(2, apInvoiceLines.Count());
			AssertEquals(1, apInvoiceLines.Count(x => x.AL_AC == ZGuid.Empty &&
													  x.AL_Desc == "ABC Charge Code" &&
													  x.AL_LineAmount == -10m &&
													  x.Job.PK == gatewayJobInCurrentCompany.PK &&
													  x.RelatedJobFromIntercompanyInvoiceImport.JobPk == shipment1.PK &&
													  x.RelatedJobFromIntercompanyInvoiceImport.JobNumber == shipment1.JS_UniqueConsignRef));

			AssertEquals(1, apInvoiceLines.Count(x => x.AL_AC == ZGuid.Empty &&
													  x.AL_Desc == "XYZ Charge Code" &&
													  x.AL_LineAmount == -20m &&
													  x.Job.PK == gatewayJobInCurrentCompany.PK &&
													  x.RelatedJobFromIntercompanyInvoiceImport.JobPk == shipment2.PK &&
													  x.RelatedJobFromIntercompanyInvoiceImport.JobNumber == shipment2.JS_UniqueConsignRef));

			convertedAPInvoice.RunPreSaveValidation();
			apInvoiceLines.All(x => x.AL_ACInfo.HasError(TransactionLine.EmptyChargeCodeAndGLHeaderError));
		}

		[TestDate(2015, 01, 01)]
		public void TestImportSisterCompanyInvoiceWithGatewayBillingChargeCodesRegistry_WhenMatchingChargeCodeIsNotAvailable_RegistryIsNotEmpty()
		{
			CreateARInvoiceForGatewayBillingChargeCodesRegistryTest(out _, out ForwardingShipment shipment1, out ForwardingShipment shipment2, out Job gatewayJobInCurrentCompany, out ARInvoice arInvoice, shouldCreateChargeCodeMapping: false);

			using (AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, TestObjectCreator.FRT.PK.ToString()))
			{
				var converter = new UnapprovedTransactionConverter(Factory);
				var convertedAPInvoice = converter.ConvertToAP(arInvoice, true);

				AssertEquals(0, convertedAPInvoice.ConsolCosting.ConsolCosts.Count);
				var apInvoiceLines = convertedAPInvoice.Lines.Cast<InvoicingLineBase>();
				AssertEquals(2, apInvoiceLines.Count());
				AssertEquals(1, apInvoiceLines.Count(x => x.AL_AC == ZGuid.Empty &&
														  x.AL_Desc == "ABC Charge Code" &&
														  x.AL_LineAmount == -10m &&
														  x.Job.PK == shipment1.Job.PK &&
														  x.RelatedJobFromIntercompanyInvoiceImport.JobPk == ZGuid.Empty &&
														  x.RelatedJobFromIntercompanyInvoiceImport.JobNumber == ZString.Empty));

				AssertEquals(1, apInvoiceLines.Count(x => x.AL_AC == ZGuid.Empty &&
														  x.AL_Desc == "XYZ Charge Code" &&
														  x.AL_LineAmount == -20m &&
														  x.Job.PK == shipment2.Job.PK &&
														  x.RelatedJobFromIntercompanyInvoiceImport.JobPk == ZGuid.Empty &&
														  x.RelatedJobFromIntercompanyInvoiceImport.JobNumber == ZString.Empty));

				convertedAPInvoice.RunPreSaveValidation();
				apInvoiceLines.All(x => x.AL_ACInfo.HasError(TransactionLine.EmptyChargeCodeAndGLHeaderError));
			}
		}

		void CreateARInvoiceForGatewayBillingChargeCodesRegistryTest(out ForwardingConsol gatewayConsol, out ForwardingShipment shipment1, out ForwardingShipment shipment2, out Job gatewayJobInCurrentCompany, out ARInvoice arInvoice, bool shouldCreateGatewayJob = true, bool shouldCreateChargeCodeMapping = true, bool shouldCreateChargeCodeMappingWithoutLocalClient = true)
		{
			SetupSisterCompanyGatewayConsolChargeCodesAndPeriods(out GlbBranch forwardingBranch, out _, out gatewayConsol, out shipment1, out shipment2, shouldCreateChargeCodes: false);
			TestObjectCreator.GST1.SetRateNumerator_ForTestOnly(0);

			var chargeCodeABC = TestObjectCreator.CreateChargeCode("ABC", "ABC Charge Code", ChargeType.Margin, 100, TestObjectCreator.GSTFREE1, TestObjectCreator.WHTFREE1, forwardingBranch.Company);
			var chargeCodeXYZ = TestObjectCreator.CreateChargeCode("XYZ", "XYZ Charge Code", ChargeType.Margin, 100, TestObjectCreator.GSTFREE1, TestObjectCreator.WHTFREE1, forwardingBranch.Company);

			if (shouldCreateChargeCodeMapping)
			{
				var globalChargeCodeMap1 = TestObjectCreator.CreateGlobalChargeCodeMapWithPivot("INT1", "Intercompany Charge Code 1", ZGuid.Empty, chargeCodeABC.PK, LedgerTypes.AccountsReceivable);
				if (shouldCreateChargeCodeMappingWithoutLocalClient)
				{
					TestObjectCreator.CreateGlobalChargeCodeMapPivot(globalChargeCodeMap1, TestObjectCreator.CC1.PK, LedgerTypes.AccountsPayable, ZGuid.Empty);
				}
				TestObjectCreator.CreateGlobalChargeCodeMapPivot(globalChargeCodeMap1, TestObjectCreator.CC2.PK, LedgerTypes.AccountsPayable, TestObjectCreator.LocalClient.PK);
				TestObjectCreator.CreateGlobalChargeCodeMapPivot(globalChargeCodeMap1, TestObjectCreator.CC3.PK, LedgerTypes.AccountsPayable, TestObjectCreator.LocalClient2.PK);

				var globalChargeCodeMap2 = TestObjectCreator.CreateGlobalChargeCodeMapWithPivot("INT2", "Intercompany Charge Code 2", ZGuid.Empty, chargeCodeXYZ.PK, LedgerTypes.AccountsReceivable);
				if (shouldCreateChargeCodeMappingWithoutLocalClient)
				{
					TestObjectCreator.CreateGlobalChargeCodeMapPivot(globalChargeCodeMap2, TestObjectCreator.CC4.PK, LedgerTypes.AccountsPayable, ZGuid.Empty);
				}
				TestObjectCreator.CreateGlobalChargeCodeMapPivot(globalChargeCodeMap2, TestObjectCreator.CC5.PK, LedgerTypes.AccountsPayable, TestObjectCreator.LocalClient.PK);
				TestObjectCreator.CreateGlobalChargeCodeMapPivot(globalChargeCodeMap2, TestObjectCreator.CC6.PK, LedgerTypes.AccountsPayable, TestObjectCreator.LocalClient2.PK);
			}

			var shipment1JobInCurrentCompany = TestObjectCreator.CreateJob(shipment1, false, localClientOrg: TestObjectCreator.LocalClient);
			var shipment2JobInCurrentCompany = TestObjectCreator.CreateJob(shipment2, false);
			gatewayJobInCurrentCompany = shouldCreateGatewayJob ? TestObjectCreator.CreateJob(gatewayConsol, false, localClientOrg: TestObjectCreator.LocalClient2) : null;
			Factory.Save();

			AssertEquals(TestObjectCreator.LocalClient.PK, shipment1JobInCurrentCompany.LocalChargesPK);
			AssertEquals(ZGuid.Empty, shipment2JobInCurrentCompany.LocalChargesPK);
			if (shouldCreateGatewayJob)
			{
				AssertEquals(TestObjectCreator.LocalClient2.PK, gatewayJobInCurrentCompany.LocalChargesPK);
			}

			var gatewayBranchOrgProxy = Factory.Load<OrgHeader>(GlbBranch.CurrentBranch.OrgProxy.PK);
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, forwardingBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var shipment1JobInSisterCompany = TestObjectCreator.CreateJob(shipment1);
				var shipment2JobInSisterCompany = TestObjectCreator.CreateJob(shipment2);
				gatewayBranchOrgProxy.CompanyData.OB_IsDebtor = true;
				Factory.Save();

				arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1.0m, gatewayBranchOrgProxy);
				arInvoice.AH_OSTotalAmount = 30m;
				arInvoice.AH_ConsolidatedInvoiceRef = gatewayConsol.JK_UniqueConsignRef;
				Assert("Posted from Forwarding Consol", arInvoice.AH_JH.IsEmpty);

				var aRInvoiceLine1 = TestObjectCreator.CreateInvoiceLine(arInvoice, shipment1JobInSisterCompany, chargeCodeABC, 10m, TestObjectCreator.AUD, 1.0m);
				var aRInvoiceLine2 = TestObjectCreator.CreateInvoiceLine(arInvoice, shipment2JobInSisterCompany, chargeCodeXYZ, 20m, TestObjectCreator.AUD, 1.0m);
				TestObjectCreator.CreateCharge(aRInvoiceLine1);
				TestObjectCreator.CreateCharge(aRInvoiceLine2);
				Factory.Save();
			}
		}

		#endregion

		[TestDate(2015, 01, 01)]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestImportSisterCompanyInvoiceWithCombineInvoiceLinesByChargeCodeRegistryOn() => AssertImportSisterCompanyInvoiceWithCombineInvoiceLinesByChargeCodeRegistrySettings(true);

		[TestDate(2015, 01, 01)]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestImportSisterCompanyInvoiceWithCombineInvoiceLinesByChargeCodeRegistryOff() => AssertImportSisterCompanyInvoiceWithCombineInvoiceLinesByChargeCodeRegistrySettings(false);

		void AssertImportSisterCompanyInvoiceWithCombineInvoiceLinesByChargeCodeRegistrySettings(bool isRegistryOn)
		{
			SetupSisterCompanyGatewayConsolChargeCodesAndPeriods(out GlbBranch forwardingBranch, out AccChargeCode chargeCode, out ForwardingConsol gatewayConsol, out ForwardingShipment shipment1, out ForwardingShipment shipment2);
			TestObjectCreator.CreateJob(gatewayConsol);

			ARInvoice arInvoice;
			var gatewayBranchOrgProxy = Factory.Load<OrgHeader>(GlbBranch.CurrentBranch.OrgProxy.PK);
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, forwardingBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var shipment1Job = TestObjectCreator.CreateJob(shipment1);
				var shipment2Job = TestObjectCreator.CreateJob(shipment2);
				gatewayBranchOrgProxy.CompanyData.OB_IsDebtor = true;
				Factory.Save();

				arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1.0m, gatewayBranchOrgProxy);
				arInvoice.AH_OSTotalAmount = 110m;
				arInvoice.AH_ConsolidatedInvoiceRef = gatewayConsol.JK_UniqueConsignRef;
				Assert("Posted from Forwarding Consol", arInvoice.AH_JH.IsEmpty);

				var aRInvoiceLine1 = TestObjectCreator.CreateInvoiceLine(arInvoice, shipment1Job, chargeCode, 20m, TestObjectCreator.AUD, 1.0m);
				var aRInvoiceLine2 = TestObjectCreator.CreateInvoiceLine(arInvoice, shipment2Job, chargeCode, 10m, TestObjectCreator.AUD, 1.0m);
				var aRInvoiceLine3 = TestObjectCreator.CreateInvoiceLine(arInvoice, shipment1Job, chargeCode, 50m, TestObjectCreator.AUD, 1.0m);
				var aRInvoiceLine4 = TestObjectCreator.CreateInvoiceLine(arInvoice, shipment2Job, chargeCode, 30m, TestObjectCreator.AUD, 1.0m);

				TestObjectCreator.CreateCharge(aRInvoiceLine1);
				TestObjectCreator.CreateCharge(aRInvoiceLine2);
				TestObjectCreator.CreateCharge(aRInvoiceLine3);
				TestObjectCreator.CreateCharge(aRInvoiceLine4);

				Factory.Save();
			}

			var converter = new UnapprovedTransactionConverter(Factory);
			InvoicingBase convertedAPInvoice;
			using (AccountingConfigurationRegistry.Instance.CombineInvoiceLinesByChargeCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isRegistryOn))
			{
				convertedAPInvoice = converter.ConvertToAP(arInvoice, true);
			}

			AssertEquals(0, convertedAPInvoice.ConsolCosting.ConsolCosts.Count);
			var apInvoiceLines = convertedAPInvoice.Lines.Cast<InvoicingLineBase>();

			AssertEquals(-arInvoice.AH_OSTotalAmount, apInvoiceLines.Sum(x => x.AL_OSAmount));
			apInvoiceLines.ForEach(aPLine =>
			{
				AssertEquals("Each AP line job header should be the gateway job header", gatewayConsol.Job.PK, aPLine.AL_JH);
				AssertEquals("Each AP line job number should be the gateway consol number", gatewayConsol.JK_UniqueConsignRef, aPLine.JobNumber);
				AssertEquals("Each AP line branch should equal the gateway consol's job header branch", gatewayConsol.Job.Branch.PK, aPLine.Branch.PK);
				AssertEquals("Each AP line deparment should equal the gateway consol's job header department", gatewayConsol.Job.Department.PK, aPLine.Department.PK);
			});

			if (isRegistryOn)
			{
				AssertEquals(2, apInvoiceLines.Count());
				AssertEquals(1, apInvoiceLines.Count(x => x.ChargeCode.AC_Code == chargeCode.AC_Code && x.AL_LineAmount == -70m && x.RelatedJobFromIntercompanyInvoiceImport.JobPk == shipment1.PK));
				AssertEquals(1, apInvoiceLines.Count(x => x.ChargeCode.AC_Code == chargeCode.AC_Code && x.AL_LineAmount == -40m && x.RelatedJobFromIntercompanyInvoiceImport.JobPk == shipment2.PK));
			}
			else
			{
				AssertEquals(4, apInvoiceLines.Count());
				AssertEquals(1, apInvoiceLines.Count(x => x.ChargeCode.AC_Code == chargeCode.AC_Code && x.AL_LineAmount == -20m && x.RelatedJobFromIntercompanyInvoiceImport.JobPk == shipment1.PK));
				AssertEquals(1, apInvoiceLines.Count(x => x.ChargeCode.AC_Code == chargeCode.AC_Code && x.AL_LineAmount == -10m && x.RelatedJobFromIntercompanyInvoiceImport.JobPk == shipment2.PK));
				AssertEquals(1, apInvoiceLines.Count(x => x.ChargeCode.AC_Code == chargeCode.AC_Code && x.AL_LineAmount == -50m && x.RelatedJobFromIntercompanyInvoiceImport.JobPk == shipment1.PK));
				AssertEquals(1, apInvoiceLines.Count(x => x.ChargeCode.AC_Code == chargeCode.AC_Code && x.AL_LineAmount == -30m && x.RelatedJobFromIntercompanyInvoiceImport.JobPk == shipment2.PK));
			}

			convertedAPInvoice.RunPreSaveValidation();
			AssertNoErrors(convertedAPInvoice);
		}

		[TestDate(2015, 01, 01)]
		public void TestAPLineSequenceIsUniqueWhenImportingSisterCompanyARInvoiceFromForwardingConsolToGatewayConsol()
		{
			SetupSisterCompanyGatewayConsolChargeCodesAndPeriods(out GlbBranch forwardingBranch, out AccChargeCode chargeCode, out ForwardingConsol gatewayConsol, out ForwardingShipment shipment1, out ForwardingShipment shipment2);

			ARInvoice aRInvoice;
			var gatewayBranchOrgProxy = Factory.Load<OrgHeader>(GlbBranch.CurrentBranch.OrgProxy.PK);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, forwardingBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var shipment1Job = TestObjectCreator.CreateJob(shipment1);
				var shipment2Job = TestObjectCreator.CreateJob(shipment2);
				gatewayBranchOrgProxy.CompanyData.OB_IsDebtor = true;
				Factory.Save();

				aRInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1.0m, gatewayBranchOrgProxy);
				aRInvoice.AH_OSTotalAmount = 3m;
				aRInvoice.AH_ConsolidatedInvoiceRef = gatewayConsol.JK_UniqueConsignRef;
				Assert(aRInvoice.AH_JH.IsEmpty);

				var aRInvoiceLine1 = TestObjectCreator.CreateInvoiceLine(aRInvoice, shipment1Job, TestObjectCreator.FRT, 1m, TestObjectCreator.AUD, 1.0m);
				var aRInvoiceLine2 = TestObjectCreator.CreateInvoiceLine(aRInvoice, shipment2Job, TestObjectCreator.FRT, 2m, TestObjectCreator.AUD, 1.0m);
				aRInvoiceLine1.AL_Sequence = 1;
				aRInvoiceLine2.AL_Sequence = 1;
				AssertEquals("Precondition: Both AR lines must have the same sequence number.", aRInvoiceLine1.AL_Sequence, aRInvoiceLine2.AL_Sequence);
				var charge1 = TestObjectCreator.CreateCharge(aRInvoiceLine1);
				var charge2 = TestObjectCreator.CreateCharge(aRInvoiceLine2);
				Factory.Save();
			}

			var converter = new UnapprovedTransactionConverter(Factory);
			var convertedAPInvoice = converter.ConvertToAP(aRInvoice, true);

			AssertEquals(2, convertedAPInvoice.Lines.Count);
			AssertEquals("Both AP lines have same job.", convertedAPInvoice.Lines[0].AL_JH, convertedAPInvoice.Lines[1].AL_JH);
			AssertNotEquals("AP lines should have different sequence numbers.", convertedAPInvoice.Lines[0].AL_Sequence, convertedAPInvoice.Lines[1].AL_Sequence);
		}

		void SetupSisterCompanyGatewayConsolChargeCodesAndPeriods(out GlbBranch forwardingBranch, out AccChargeCode chargeCode, out ForwardingConsol gatewayConsol, out ForwardingShipment shipment1, out ForwardingShipment shipment2, bool shouldCreateChargeCodes = true)
		{
			var forwardingCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("FDWORG", true, false);
			var forwardingCompany = TestObjectCreator.CreateNewCompany("CFW", orgProxy: forwardingCompanyOrgProxy);
			forwardingBranch = TestObjectCreator.CreateNewBranch(forwardingCompany, "BFW");
			var receivingGatewayDepartment = Factory.Load<GlbDepartment>(GlbDepartment.CurrentDepartment.PK);
			receivingGatewayDepartment.GE_Misc = false;

			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year, GlbCompany.CurrentCompany.PK);
			periodManagementTestHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year, forwardingCompany.PK);

			chargeCode = null;
			if (shouldCreateChargeCodes)
			{
				chargeCode = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1 for sending company", ChargeType.Margin, 100, TestObjectCreator.GSTFREE1, TestObjectCreator.WHTFREE1, forwardingCompany);
				TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1 for receiving company", ChargeType.Margin, 100, TestObjectCreator.GSTFREE1, TestObjectCreator.WHTFREE1);
			}

			gatewayConsol = TestObjectCreator.CreateGatewayConsol("HKHKG", "AUSYD", "C0000558", receivingGatewayCompany: GlbCompany.CurrentCompany);
			shipment1 = TestObjectCreator.CreateShipment("S001", gatewayConsol);
			shipment2 = TestObjectCreator.CreateShipment("S002", gatewayConsol);
			Factory.Save();
		}
	}
}
