using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing.UnapprovedTransaction
{
	public class GatewayARTransactionToAPTransactionConverterTest : ARTransactionToAPTransactionConverterBaseTest
	{
		#region Target Job Is Shipment

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsShipment_SameTargetJob_DifferentChargeCodes()
		{
			AssertTargetJobIsShipmentCore(true, false);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsShipment_SameTargetJob_SameChargeCode_CombineInvoiceLinesByChargeCodeRegistryOn()
		{
			AssertTargetJobIsShipmentCore(true, true, true);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsShipment_SameTargetJob_SameChargeCode_CombineInvoiceLinesByChargeCodeRegistryOff()
		{
			AssertTargetJobIsShipmentCore(true, true, false);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsShipment_DifferentTargetJobs_DifferentChargeCodes()
		{
			AssertTargetJobIsShipmentCore(false, false);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsShipment_DifferentTargetJobs_SameChargeCode()
		{
			AssertTargetJobIsShipmentCore(false, true);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsShipment_SameTargetJob_LineSequenceIsNotDuplicated()
		{
			SetupSisterCompaniesChargesCodesAndPeriods(out GlbCompany sisterCompany, out GlbBranch sisterBranch, out AccChargeCode chargeCode1, out AccChargeCode chargeCode2);

			var gatewayConsol1 = TestObjectCreator.CreateGatewayConsol("USLAX", "AUMEL", "C001", receivingGatewayCompany: sisterCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S1111", gatewayConsol1);
			var gatewayConsol2 = TestObjectCreator.CreateGatewayConsol("USLAX", "AUMEL", "C002", receivingGatewayCompany: sisterCompany);
			gatewayConsol2.Shipments.Add(shipment1);
			Factory.Save();

			InvoicingBase periodicInvoice = null;
			var debtorForPeriodicInvoice = GlbCompany.CurrentCompany.OrgProxy;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sisterBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var gatewayJob1 = TestObjectCreator.CreateJob(gatewayConsol1);
				var gatewayJob2 = TestObjectCreator.CreateJob(gatewayConsol2);
				periodicInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForPeriodicInvoice);
				periodicInvoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice_Batching;
				var arInvoiceLine1 = TestObjectCreator.CreateInvoiceLine(periodicInvoice, gatewayJob1, chargeCode1, 100m, TestObjectCreator.AUD, 1m);
				arInvoiceLine1.AL_OH = debtorForPeriodicInvoice.PK;
				arInvoiceLine1.AL_Sequence = 15;
				var charge1 = TestObjectCreator.CreateChargeWithTarget(arInvoiceLine1, shipment1, shipment1);
				var arInvoiceLine2 = TestObjectCreator.CreateInvoiceLine(periodicInvoice, gatewayJob2, chargeCode2, 200m, TestObjectCreator.AUD, 1m);
				arInvoiceLine2.AL_Sequence = 15;
				arInvoiceLine2.AL_OH = debtorForPeriodicInvoice.PK;
				var charge2 = TestObjectCreator.CreateChargeWithTarget(arInvoiceLine2, shipment1, shipment1);
				charge2.JR_OH_SellAccount = debtorForPeriodicInvoice.PK;
				Factory.Save();

				AssertEquals("Precondition: Both AR lines must have the same sequence number.", arInvoiceLine1.AL_Sequence, arInvoiceLine2.AL_Sequence);
				Factory.Save();
			}

			var converter = new UnapprovedTransactionConverter(Factory);
			var convertedAPInvoice = converter.ConvertToAP(periodicInvoice, true);

			AssertEquals(0, convertedAPInvoice.ConsolCosting.ConsolCosts.Count);
			AssertEquals(2, convertedAPInvoice.Lines.Count);
			AssertEquals("Both AP lines have same job.", convertedAPInvoice.Lines[0].AL_JH, convertedAPInvoice.Lines[1].AL_JH);
			AssertNotEquals("AP lines should have different sequence numbers.", convertedAPInvoice.Lines[0].AL_Sequence, convertedAPInvoice.Lines[1].AL_Sequence);

			convertedAPInvoice.RunPreSaveValidation();
			AssertNoErrors(convertedAPInvoice);
		}

		void AssertTargetJobIsShipmentCore(bool isSameTargetJob, bool isSameChargeCode, bool isCombineInvoiceLinesByChargeCodeRegistryOn = false)
		{
			SetupSisterCompaniesChargesCodesAndPeriods(out GlbCompany sisterCompany, out GlbBranch sisterBranch, out AccChargeCode chargeCode1, out AccChargeCode chargeCode2);

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol("USLAX", "AUMEL", "C001", receivingGatewayCompany: sisterCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S1111", gatewayConsol);
			var shipment2 = TestObjectCreator.CreateShipment("S2222", gatewayConsol);
			Factory.Save();

			InvoicingBase arInvoice = null;
			var debtorForARInvoice = GlbCompany.CurrentCompany.OrgProxy;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sisterBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol);
				arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				arInvoice.AH_ConsolidatedInvoiceRef = gatewayConsol.JK_UniqueConsignRef;
				arInvoice.AH_JH = gatewayJob.PK;
				var arInvoiceLine1 = TestObjectCreator.CreateInvoiceLine(arInvoice, gatewayJob, chargeCode1, 100m, TestObjectCreator.AUD, 1m);
				arInvoiceLine1.AL_OH = debtorForARInvoice.PK;
				var charge1 = TestObjectCreator.CreateChargeWithTarget(arInvoiceLine1, shipment1, shipment1);
				var arInvoiceLine2 = TestObjectCreator.CreateInvoiceLine(arInvoice, gatewayJob, isSameChargeCode ? chargeCode1 : chargeCode2, 200m, TestObjectCreator.AUD, 1m);
				arInvoiceLine2.AL_OH = debtorForARInvoice.PK;
				var charge2target = isSameTargetJob ? shipment1 : shipment2;
				var charge2 = TestObjectCreator.CreateChargeWithTarget(arInvoiceLine2, charge2target, charge2target);
				Factory.Save();
			}

			InvoicingBase convertedAPInvoice = null;
			using (AccountingConfigurationRegistry.Instance.CombineInvoiceLinesByChargeCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isCombineInvoiceLinesByChargeCodeRegistryOn))
			{
				var converter = new UnapprovedTransactionConverter(Factory);
				convertedAPInvoice = converter.ConvertToAP(arInvoice, true);
			}

			AssertEquals(0, convertedAPInvoice.ConsolCosting.ConsolCosts.Count);
			var expectedInvoiceLineCount = isSameTargetJob && isSameChargeCode && isCombineInvoiceLinesByChargeCodeRegistryOn ? 1 : 2;
			AssertEquals(expectedInvoiceLineCount, convertedAPInvoice.Lines.Count);

			var invoiceLines = convertedAPInvoice.Lines.Cast<InvoicingLineBase>();

			if (expectedInvoiceLineCount == 1)
			{
				AssertEquals(1, invoiceLines.Count(x => x.ChargeCode.AC_Code == chargeCode1.AC_Code && x.Job.JH_ParentID == shipment1.PK && x.AL_LineAmount == -300m));
			}
			else
			{
				AssertEquals(1, invoiceLines.Count(x => x.ChargeCode.AC_Code == chargeCode1.AC_Code && x.Job.JH_ParentID == shipment1.PK && x.AL_LineAmount == -100m));
				var expectedInvoiceLine2ChargeCode = isSameChargeCode ? chargeCode1.AC_Code : chargeCode2.AC_Code;
				var expectedInvoiceLine2JobParent = isSameTargetJob ? shipment1.PK : shipment2.PK;
				AssertEquals(1, invoiceLines.Count(x => x.ChargeCode.AC_Code == expectedInvoiceLine2ChargeCode && x.Job.JH_ParentID == expectedInvoiceLine2JobParent && x.AL_LineAmount == -200m));
			}

			convertedAPInvoice.RunPreSaveValidation();
			AssertNoErrors(convertedAPInvoice);
		}

		void SetupSisterCompaniesChargesCodesAndPeriods(out GlbCompany sisterCompany, out GlbBranch sisterBranch, out AccChargeCode chargeCode1, out AccChargeCode chargeCode2)
		{
			var sisterCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("SISORG", true, false);
			sisterCompany = TestObjectCreator.CreateNewCompany("SIS", orgProxy: sisterCompanyOrgProxy);
			sisterBranch = TestObjectCreator.CreateNewBranch(sisterCompany, "SIS");
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sisterBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				GlbCompany.CurrentCompany.OrgProxy.CompanyData.OB_IsDebtor = true;
				GlbCompany.CurrentCompany.Factory.Save();
			}

			periodManagementTestHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year, GlbCompany.CurrentCompany.PK);
			periodManagementTestHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year, sisterCompany.PK);

			chargeCode1 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1 for sister company", ChargeType.Margin, 100, TestObjectCreator.GSTFREE1, TestObjectCreator.WHTFREE1, sisterCompany);
			chargeCode2 = TestObjectCreator.CreateChargeCode("CC2", "Charge Code 2 for sister company", ChargeType.Margin, 100, TestObjectCreator.GSTFREE1, TestObjectCreator.WHTFREE1, sisterCompany);
			TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1 for current company", ChargeType.Margin, 100, TestObjectCreator.GSTFREE1, TestObjectCreator.WHTFREE1);
			TestObjectCreator.CreateChargeCode("CC2", "Charge Code 2 for current company", ChargeType.Margin, 100, TestObjectCreator.GSTFREE1, TestObjectCreator.WHTFREE1);
		}

		#endregion

		#region Target Job is Non Gateway

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsNonGateway_SameTargetJob_SameChargeCode_DifferentRelatedJobs_CombineInvoiceLinesByChargeCodeRegistryOff()
		{
			AssertTargetJobIsNonGatewayCore(true, true, false);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsNonGateway_SameTargetJob_SameChargeCode_SameRelatedJob_CombineInvoiceLinesByChargeCodeRegistryOff()
		{
			AssertTargetJobIsNonGatewayCore(true, true, true);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsNonGateway_SameTargetJob_SameChargeCode_DifferentRelatedJobs_CombineInvoiceLinesByChargeCodeRegistryOn()
		{
			AssertTargetJobIsNonGatewayCore(true, true, false, true);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsNonGateway_SameTargetJob_SameChargeCode_SameRelatedJob_CombineInvoiceLinesByChargeCodeRegistryOn()
		{
			AssertTargetJobIsNonGatewayCore(true, true, true, true);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsNonGateway_SameTargetJob_DifferentChargeCodes_DifferentRelatedJobs()
		{
			AssertTargetJobIsNonGatewayCore(true, false, false);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsNonGateway_SameTargetJob_DifferentChargeCodes_SameRelatedJob()
		{
			AssertTargetJobIsNonGatewayCore(true, false, true);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsNonGateway_DifferentTargetJobs_SameChargeCode_DifferentRelatedJobs_CombineInvoiceLinesByChargeCodeRegistryOff()
		{
			AssertTargetJobIsNonGatewayCore(false, true, false);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsNonGateway_DifferentTargetJobs_SameChargeCode_SameRelatedJob_CombineInvoiceLinesByChargeCodeRegistryOff()
		{
			AssertTargetJobIsNonGatewayCore(false, true, true);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsNonGateway_DifferentTargetJobs_SameChargeCode_DifferentRelatedJobs_CombineInvoiceLinesByChargeCodeRegistryOn()
		{
			AssertTargetJobIsNonGatewayCore(false, true, false, true);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsNonGateway_DifferentTargetJobs_SameChargeCode_SameRelatedJob_CombineInvoiceLinesByChargeCodeRegistryOn()
		{
			AssertTargetJobIsNonGatewayCore(false, true, true, true);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsNonGateway_DifferentTargetJobs_DifferentChargeCodes_DifferentRelatedJobs()
		{
			AssertTargetJobIsNonGatewayCore(false, false, false);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsNonGateway_DifferentTargetJobs_DifferentChargeCodes_SameRelatedJob()
		{
			AssertTargetJobIsNonGatewayCore(false, false, true);
		}

		void AssertTargetJobIsNonGatewayCore(bool isSameTargetJob, bool isSameChargeCode, bool isSameRelatedJob, bool isCombineInvoiceLinesByChargeCodeRegistryOn = false)
		{
			SetupSisterCompaniesChargesCodesAndPeriods(out GlbCompany sisterCompany, out GlbBranch sisterBranch, out AccChargeCode chargeCode1, out AccChargeCode chargeCode2);

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol("USLAX", "AUMEL", "C00356", receivingGatewayCompany: sisterCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S1111", gatewayConsol);
			var shipment2 = TestObjectCreator.CreateShipment("S2222", gatewayConsol);
			var forwardingConsol1 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00125");
			forwardingConsol1.Shipments.AddRange(new[] { shipment1, shipment2 });
			var forwardingConsol2 = TestObjectCreator.CreateConsol("NZAKL", "USLAX", "C00233");
			forwardingConsol2.Shipments.AddRange(new[] { shipment1, shipment2 });

			Factory.Save();

			InvoicingBase arInvoice = null;
			var debtorForARInvoice = GlbCompany.CurrentCompany.OrgProxy;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sisterBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol);
				arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				arInvoice.AH_ConsolidatedInvoiceRef = gatewayConsol.JK_UniqueConsignRef;
				arInvoice.AH_JH = gatewayJob.PK;
				var arInvoiceLine1 = TestObjectCreator.CreateInvoiceLine(arInvoice, gatewayJob, chargeCode1, 100m, TestObjectCreator.AUD, 1m);
				arInvoiceLine1.AL_OH = debtorForARInvoice.PK;
				var charge1 = TestObjectCreator.CreateChargeWithTarget(arInvoiceLine1, shipment1, forwardingConsol1);
				var arInvoiceLine2 = TestObjectCreator.CreateInvoiceLine(arInvoice, gatewayJob, isSameChargeCode ? chargeCode1 : chargeCode2, 200m, TestObjectCreator.AUD, 1m);
				arInvoiceLine2.AL_OH = debtorForARInvoice.PK;
				var targetJobForCharge2 = isSameTargetJob ? forwardingConsol1 : forwardingConsol2;
				var relatedJobForCharge2 = isSameRelatedJob ? shipment1 : shipment2;
				var charge2 = TestObjectCreator.CreateChargeWithTarget(arInvoiceLine2, relatedJobForCharge2, targetJobForCharge2);
				Factory.Save();
			}

			InvoicingBase convertedAPInvoice = null;
			using (AccountingConfigurationRegistry.Instance.CombineInvoiceLinesByChargeCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isCombineInvoiceLinesByChargeCodeRegistryOn))
			{
				var converter = new UnapprovedTransactionConverter(Factory);
				convertedAPInvoice = converter.ConvertToAP(arInvoice, true);
			}

			var expectedNumberOfConsolCosts = isSameTargetJob && isSameChargeCode ? 1 : 2;
			AssertEquals(expectedNumberOfConsolCosts, convertedAPInvoice.ConsolCosting.ConsolCosts.Count);
			var consolCosts = convertedAPInvoice.ConsolCosting.ConsolCosts.Cast<JobConsolCost>();
			if (expectedNumberOfConsolCosts == 1)
			{
				var consolCost = consolCosts.FirstOrDefault(x => x.E6_ParentID == forwardingConsol1.PK && x.ChargeCode.AC_Code == chargeCode1.AC_Code && x.E6_OSCostAmount == 300m);
				AssertNotNull(consolCost);
				var apportionCharges = consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>();
				if (isSameRelatedJob)
				{
					AssertEquals(1, apportionCharges.Count(x => x.Job.JH_ParentID == shipment1.PK && x.ChargeCode.AC_Code == chargeCode1.AC_Code && x.JR_OSCostAmt == 300m));
					AssertEquals(1, apportionCharges.Count(x => x.Job.JH_ParentID == shipment2.PK && x.ChargeCode.AC_Code == chargeCode1.AC_Code && x.JR_OSCostAmt == 0m));
				}
				else
				{
					AssertEquals(1, apportionCharges.Count(x => x.Job.JH_ParentID == shipment1.PK && x.ChargeCode.AC_Code == chargeCode1.AC_Code && x.JR_OSCostAmt == 100m));
					AssertEquals(1, apportionCharges.Count(x => x.Job.JH_ParentID == shipment2.PK && x.ChargeCode.AC_Code == chargeCode1.AC_Code && x.JR_OSCostAmt == 200m));
				}
			}
			else
			{
				var firstConsolCost = consolCosts.FirstOrDefault(x => x.E6_ParentID == forwardingConsol1.PK && x.ChargeCode.AC_Code == chargeCode1.AC_Code && x.E6_OSCostAmount == 100m);
				AssertNotNull(firstConsolCost);
				var apportionChargesForFirstConsolCost = firstConsolCost.ApportionmentCharges.Cast<ApportionSplitCharge>();
				AssertEquals(1, apportionChargesForFirstConsolCost.Count(x => x.Job.JH_ParentID == shipment1.PK && x.ChargeCode.AC_Code == chargeCode1.AC_Code && x.JR_OSCostAmt == 100m));
				AssertEquals(1, apportionChargesForFirstConsolCost.Count(x => x.Job.JH_ParentID == shipment2.PK && x.ChargeCode.AC_Code == chargeCode1.AC_Code && x.JR_OSCostAmt == 0m));

				var expectedConsolCost2ChargeCode = isSameChargeCode ? chargeCode1.AC_Code : chargeCode2.AC_Code;
				var expectedConsolCost2Parent = isSameTargetJob ? forwardingConsol1.PK : forwardingConsol2.PK;
				var secondConsolCost = consolCosts.FirstOrDefault(x => x.E6_ParentID == expectedConsolCost2Parent && x.ChargeCode.AC_Code == expectedConsolCost2ChargeCode && x.E6_OSCostAmount == 200m);
				AssertNotNull(secondConsolCost);
				var apportionChargesForSecondConsolCost = secondConsolCost.ApportionmentCharges.Cast<ApportionSplitCharge>();
				AssertEquals(1, apportionChargesForSecondConsolCost.Count(x => x.Job.JH_ParentID == shipment1.PK && x.ChargeCode.AC_Code == expectedConsolCost2ChargeCode && x.JR_OSCostAmt == (isSameRelatedJob ? 200m : 0m)));
				AssertEquals(1, apportionChargesForSecondConsolCost.Count(x => x.Job.JH_ParentID == shipment2.PK && x.ChargeCode.AC_Code == expectedConsolCost2ChargeCode && x.JR_OSCostAmt == (isSameRelatedJob ? 0m : 200m)));
			}

			var expectedNumberOfInvoiceLines = isSameTargetJob && isSameChargeCode && isSameRelatedJob ? 1 : 2;
			AssertEquals(expectedNumberOfInvoiceLines, convertedAPInvoice.Lines.Count);
			var invoiceLines = convertedAPInvoice.Lines.Cast<InvoicingLineBase>();
			if (expectedNumberOfInvoiceLines == 1)
			{
				AssertEquals(1, invoiceLines.Count(x => x.ChargeCode.AC_Code == chargeCode1.AC_Code && x.Job.JH_ParentID == shipment1.PK && x.AL_LineAmount == -300m));
			}
			else
			{
				AssertEquals(1, invoiceLines.Count(x => x.ChargeCode.AC_Code == chargeCode1.AC_Code && x.Job.JH_ParentID == shipment1.PK && x.AL_LineAmount == -100m));
				var expectedInvoiceLine2ChargeCode = isSameChargeCode ? chargeCode1.AC_Code : chargeCode2.AC_Code;
				var expectedInvoiceLine2JobParent = isSameRelatedJob ? shipment1.PK : shipment2.PK;
				AssertEquals(1, invoiceLines.Count(x => x.ChargeCode.AC_Code == expectedInvoiceLine2ChargeCode && x.Job.JH_ParentID == expectedInvoiceLine2JobParent && x.AL_LineAmount == -200m));
			}

			convertedAPInvoice.RunPreSaveValidation();
			AssertNoErrors(convertedAPInvoice);
		}

		#endregion

		#region Target Job is GTW

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsGateway_SameTargetJob_SameChargeCode_DifferentRelatedJobs_CombineInvoiceLinesByChargeCodeRegistryOff()
		{
			AssertTargetJobIsGatewayCore(true, true, false);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsGateway_SameTargetJob_SameChargeCode_SameRelatedJob_CombineInvoiceLinesByChargeCodeRegistryOff()
		{
			AssertTargetJobIsGatewayCore(true, true, true);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsGateway_SameTargetJob_SameChargeCode_DifferentRelatedJobs_CombineInvoiceLinesByChargeCodeRegistryOn()
		{
			AssertTargetJobIsGatewayCore(true, true, false, true);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsGateway_SameTargetJob_SameChargeCode_SameRelatedJob_CombineInvoiceLinesByChargeCodeRegistryOn()
		{
			AssertTargetJobIsGatewayCore(true, true, true, true);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsGateway_SameTargetJob_DifferentChargeCodes_DifferentRelatedJobs()
		{
			AssertTargetJobIsGatewayCore(true, false, false);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsGateway_SameTargetJob_DifferentChargeCodes_SameRelatedJob()
		{
			AssertTargetJobIsGatewayCore(true, false, true);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsGateway_DifferentTargetJobs_SameChargeCode_DifferentRelatedJobs_CombineInvoiceLinesByChargeCodeRegistryOff()
		{
			AssertTargetJobIsGatewayCore(false, true, false);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsGateway_DifferentTargetJobs_SameChargeCode_SameRelatedJob_CombineInvoiceLinesByChargeCodeRegistryOff()
		{
			AssertTargetJobIsGatewayCore(false, true, true);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsGateway_DifferentTargetJobs_SameChargeCode_DifferentRelatedJobs_CombineInvoiceLinesByChargeCodeRegistryOn()
		{
			AssertTargetJobIsGatewayCore(false, true, false, true);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsGateway_DifferentTargetJobs_SameChargeCode_SameRelatedJob_CombineInvoiceLinesByChargeCodeRegistryOn()
		{
			AssertTargetJobIsGatewayCore(false, true, true, true);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsGateway_DifferentTargetJobs_DifferentChargeCodes_DifferentRelatedJobs()
		{
			AssertTargetJobIsGatewayCore(false, false, false);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsGateway_DifferentTargetJobs_DifferentChargeCodes_SameRelatedJob()
		{
			AssertTargetJobIsGatewayCore(false, false, true);
		}

		void AssertTargetJobIsGatewayCore(bool isSameTargetJob, bool isSameChargeCode, bool isSameRelatedJob, bool isCombineInvoiceLinesByChargeCodeRegistryOn = false)
		{
			SetupSisterCompaniesChargesCodesAndPeriods(out GlbCompany sisterCompany, out GlbBranch sisterBranch, out AccChargeCode chargeCode1, out AccChargeCode chargeCode2);
			var receivingGatewayDepartment = Factory.Load<GlbDepartment>(GlbDepartment.CurrentDepartment.PK);
			receivingGatewayDepartment.GE_Misc = false;
			Factory.Save();

			var gatewayConsol1 = TestObjectCreator.CreateGatewayConsol("USLAX", "AUMEL", "C00356", sendingGatewayCompany: sisterCompany, receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S1111", gatewayConsol1);
			var shipment2 = TestObjectCreator.CreateShipment("S2222", gatewayConsol1);
			var gatewayConsol2 = TestObjectCreator.CreateGatewayConsol("NZAKL", "USLAX", "C00256", sendingGatewayCompany: sisterCompany, receivingGatewayCompany: GlbCompany.CurrentCompany);
			gatewayConsol2.Shipments.AddRange(new[] { shipment1, shipment2 });
			var gatewayConsol3 = TestObjectCreator.CreateGatewayConsol("KRSEL", "NZAKL", "C00156", sendingGatewayCompany: sisterCompany, receivingGatewayCompany: GlbCompany.CurrentCompany);
			gatewayConsol3.Shipments.AddRange(new[] { shipment1, shipment2 });

			Factory.Save();

			InvoicingBase arInvoice = null;
			var debtorForARInvoice = GlbCompany.CurrentCompany.OrgProxy;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sisterBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol1);
				arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				arInvoice.AH_ConsolidatedInvoiceRef = gatewayConsol1.JK_UniqueConsignRef;
				arInvoice.AH_JH = gatewayJob.PK;
				var arInvoiceLine1 = TestObjectCreator.CreateInvoiceLine(arInvoice, gatewayJob, chargeCode1, 100m, TestObjectCreator.AUD, 1m);
				arInvoiceLine1.AL_OH = debtorForARInvoice.PK;
				TestObjectCreator.CreateChargeWithTarget(arInvoiceLine1, shipment1, gatewayConsol2);
				var arInvoiceLine2 = TestObjectCreator.CreateInvoiceLine(arInvoice, gatewayJob, isSameChargeCode ? chargeCode1 : chargeCode2, 200m, TestObjectCreator.AUD, 1m);
				arInvoiceLine2.AL_OH = debtorForARInvoice.PK;
				var relatedJobForCharge2 = isSameRelatedJob ? shipment1 : shipment2;
				var targetJobForCharge2 = isSameTargetJob ? gatewayConsol2 : gatewayConsol3;
				TestObjectCreator.CreateChargeWithTarget(arInvoiceLine2, relatedJobForCharge2, targetJobForCharge2);
				Factory.Save();
			}

			InvoicingBase convertedAPInvoice = null;
			using (AccountingConfigurationRegistry.Instance.CombineInvoiceLinesByChargeCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isCombineInvoiceLinesByChargeCodeRegistryOn))
			{
				var converter = new UnapprovedTransactionConverter(Factory);
				convertedAPInvoice = converter.ConvertToAP(arInvoice, true);
			}

			AssertEquals(0, convertedAPInvoice.ConsolCosting.ConsolCosts.Count);

			var expectedNumberOfInvoiceLines = isSameTargetJob && isSameChargeCode && isSameRelatedJob && isCombineInvoiceLinesByChargeCodeRegistryOn ? 1 : 2;
			AssertEquals(expectedNumberOfInvoiceLines, convertedAPInvoice.Lines.Count);

			var invoiceLines = convertedAPInvoice.Lines.Cast<InvoicingLineBase>();
			if (expectedNumberOfInvoiceLines == 1)
			{
				AssertEquals(1, invoiceLines.Count(x => x.ChargeCode.AC_Code == chargeCode1.AC_Code && x.Job.JH_ParentID == gatewayConsol2.PK && x.AL_LineAmount == -300m));
			}
			else
			{
				AssertEquals(1, invoiceLines.Count(x => x.ChargeCode.AC_Code == chargeCode1.AC_Code && x.Job.JH_ParentID == gatewayConsol2.PK && x.AL_LineAmount == -100m));
				var expectedInvoiceLine2ChargeCode = isSameChargeCode ? chargeCode1.AC_Code : chargeCode2.AC_Code;
				var expectedInvoiceLine2JobParent = isSameTargetJob ? gatewayConsol2.PK : gatewayConsol3.PK;
				AssertEquals(1, invoiceLines.Count(x => x.ChargeCode.AC_Code == expectedInvoiceLine2ChargeCode && x.Job.JH_ParentID == expectedInvoiceLine2JobParent && x.AL_LineAmount == -200m));
			}

			convertedAPInvoice.RunPreSaveValidation();
			AssertNoErrors(convertedAPInvoice);
		}

		#endregion

		#region Target is Blank, Consol is GTW in Current Company

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsBlank_ConsolIsGatewayInCurrentCompany_RelatedJobIsBlank_SameChargeCode_CombineInvoiceLinesByChargeCodeRegistryOn()
		{
			AssertTargetJobIsBlankConsolIsGatewayInCurrentCompanyCore(RelatedJobOptions.Blank, true, true);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsBlank_ConsolIsGatewayInCurrentCompany_RelatedJobIsBlank_SameChargeCode_CombineInvoiceLinesByChargeCodeRegistryOff()
		{
			AssertTargetJobIsBlankConsolIsGatewayInCurrentCompanyCore(RelatedJobOptions.Blank, true, false);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsBlank_ConsolIsGatewayInCurrentCompany_RelatedJobIsBlank_DifferentChargeCodes()
		{
			AssertTargetJobIsBlankConsolIsGatewayInCurrentCompanyCore(RelatedJobOptions.Blank, false);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsBlank_ConsolIsGatewayInCurrentCompany_SameRelatedJob_SameChargeCode_CombineInvoiceLinesByChargeCodeRegistryOn()
		{
			AssertTargetJobIsBlankConsolIsGatewayInCurrentCompanyCore(RelatedJobOptions.Same, true, true);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsBlank_ConsolIsGatewayInCurrentCompany_SameRelatedJob_SameChargeCode_CombineInvoiceLinesByChargeCodeRegistryOff()
		{
			AssertTargetJobIsBlankConsolIsGatewayInCurrentCompanyCore(RelatedJobOptions.Same, true, false);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsBlank_ConsolIsGatewayInCurrentCompany_SameRelatedJob_DifferentChargeCodes()
		{
			AssertTargetJobIsBlankConsolIsGatewayInCurrentCompanyCore(RelatedJobOptions.Same, false);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsBlank_ConsolIsGatewayInCurrentCompany_DifferentRelatedJob_SameChargeCode_CombineInvoiceLinesByChargeCodeRegistryOn()
		{
			AssertTargetJobIsBlankConsolIsGatewayInCurrentCompanyCore(RelatedJobOptions.Different, true, true);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsBlank_ConsolIsGatewayInCurrentCompany_DifferentRelatedJob_SameChargeCode_CombineInvoiceLinesByChargeCodeRegistryOff()
		{
			AssertTargetJobIsBlankConsolIsGatewayInCurrentCompanyCore(RelatedJobOptions.Different, true, false);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsBlank_ConsolIsGatewayInCurrentCompany_DifferentRelatedJob_DifferentChargeCodes()
		{
			AssertTargetJobIsBlankConsolIsGatewayInCurrentCompanyCore(RelatedJobOptions.Different, false);
		}

		void AssertTargetJobIsBlankConsolIsGatewayInCurrentCompanyCore(RelatedJobOptions relatedJobOption, bool isSameChargeCode, bool isCombineInvoiceLinesByChargeCodeRegistryOn = false)
		{
			SetupSisterCompaniesChargesCodesAndPeriods(out GlbCompany sisterCompany, out GlbBranch sisterBranch, out AccChargeCode chargeCode1, out AccChargeCode chargeCode2);
			var receivingGatewayDepartment = Factory.Load<GlbDepartment>(GlbDepartment.CurrentDepartment.PK);
			receivingGatewayDepartment.GE_Misc = false;
			Factory.Save();

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol("USLAX", "AUMEL", "C00356", sendingGatewayCompany: sisterCompany, receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S1111", gatewayConsol);
			var shipment2 = TestObjectCreator.CreateShipment("S2222", gatewayConsol);
			Factory.Save();

			InvoicingBase arInvoice = null;
			var debtorForARInvoice = GlbCompany.CurrentCompany.OrgProxy;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sisterBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol);
				arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				arInvoice.AH_ConsolidatedInvoiceRef = gatewayConsol.JK_UniqueConsignRef;
				arInvoice.AH_JH = gatewayJob.PK;
				var arInvoiceLine1 = TestObjectCreator.CreateInvoiceLine(arInvoice, gatewayJob, chargeCode1, 100m, TestObjectCreator.AUD, 1m);
				var relatedJobForCharge1 = relatedJobOption == RelatedJobOptions.Blank ? null : shipment1;
				TestObjectCreator.CreateChargeWithTarget(arInvoiceLine1, relatedJobForCharge1, null);
				var arInvoiceLine2 = TestObjectCreator.CreateInvoiceLine(arInvoice, gatewayJob, isSameChargeCode ? chargeCode1 : chargeCode2, 200m, TestObjectCreator.AUD, 1m);
				var relatedJobForCharge2 = relatedJobOption == RelatedJobOptions.Blank ? null : (relatedJobOption == RelatedJobOptions.Different ? shipment2 : shipment1);
				TestObjectCreator.CreateChargeWithTarget(arInvoiceLine2, relatedJobForCharge2, null);
				Factory.Save();
			}

			InvoicingBase convertedAPInvoice = null;
			using (AccountingConfigurationRegistry.Instance.CombineInvoiceLinesByChargeCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isCombineInvoiceLinesByChargeCodeRegistryOn))
			{
				var converter = new UnapprovedTransactionConverter(Factory);
				convertedAPInvoice = converter.ConvertToAP(arInvoice, true);
			}

			AssertEquals(0, convertedAPInvoice.ConsolCosting.ConsolCosts.Count);

			var expectedNumberOfInvoiceLines = isSameChargeCode && (relatedJobOption == RelatedJobOptions.Blank || relatedJobOption == RelatedJobOptions.Same) && isCombineInvoiceLinesByChargeCodeRegistryOn ? 1 : 2;
			AssertEquals(expectedNumberOfInvoiceLines, convertedAPInvoice.Lines.Count);

			var invoiceLines = convertedAPInvoice.Lines.Cast<InvoicingLineBase>();
			if (expectedNumberOfInvoiceLines == 1)
			{
				AssertEquals(1, invoiceLines.Count(x => x.ChargeCode.AC_Code == chargeCode1.AC_Code && x.Job.JH_ParentID == gatewayConsol.PK && x.AL_LineAmount == -300m));
			}
			else
			{
				AssertEquals(1, invoiceLines.Count(x => x.ChargeCode.AC_Code == chargeCode1.AC_Code && x.Job.JH_ParentID == gatewayConsol.PK && x.AL_LineAmount == -100m));
				var expectedInvoiceLine2ChargeCode = isSameChargeCode ? chargeCode1.AC_Code : chargeCode2.AC_Code;
				AssertEquals(1, invoiceLines.Count(x => x.ChargeCode.AC_Code == expectedInvoiceLine2ChargeCode && x.Job.JH_ParentID == gatewayConsol.PK && x.AL_LineAmount == -200m));
			}

			convertedAPInvoice.RunPreSaveValidation();
			AssertNoErrors(convertedAPInvoice);
		}

		#endregion

		#region Target is Blank, Consol is not GTW in Current Company

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsBlank_ConsolIsNotGatewayInCurrentCompany_RelatedJobIsBlank_SameChargeCode_CombineInvoiceLinesByChargeCodeRegistryOn()
		{
			AssertTargetJobIsBlankConsolIsNotGatewayInCurrentCompanyCore(RelatedJobOptions.Blank, true, true);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsBlank_ConsolIsNotGatewayInCurrentCompany_RelatedJobIsBlank_SameChargeCode_CombineInvoiceLinesByChargeCodeRegistryOff()
		{
			AssertTargetJobIsBlankConsolIsNotGatewayInCurrentCompanyCore(RelatedJobOptions.Blank, true, false);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsBlank_ConsolIsNotGatewayInCurrentCompany_RelatedJobIsBlank_DifferentChargeCodes()
		{
			AssertTargetJobIsBlankConsolIsNotGatewayInCurrentCompanyCore(RelatedJobOptions.Blank, false);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsBlank_ConsolIsNotGatewayInCurrentCompany_SameRelatedJob_SameChargeCode_CombineInvoiceLinesByChargeCodeRegistryOn()
		{
			AssertTargetJobIsBlankConsolIsNotGatewayInCurrentCompanyCore(RelatedJobOptions.Same, true, true);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsBlank_ConsolIsNotGatewayInCurrentCompany_SameRelatedJob_SameChargeCode_CombineInvoiceLinesByChargeCodeRegistryOff()
		{
			AssertTargetJobIsBlankConsolIsNotGatewayInCurrentCompanyCore(RelatedJobOptions.Same, true, false);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsBlank_ConsolIsNotGatewayInCurrentCompany_SameRelatedJob_DifferentChargeCodes()
		{
			AssertTargetJobIsBlankConsolIsNotGatewayInCurrentCompanyCore(RelatedJobOptions.Same, false);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsBlank_ConsolIsNotGatewayInCurrentCompany_DifferentRelatedJob_SameChargeCode_CombineInvoiceLinesByChargeCodeRegistryOn()
		{
			AssertTargetJobIsBlankConsolIsNotGatewayInCurrentCompanyCore(RelatedJobOptions.Different, true, true);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsBlank_ConsolIsNotGatewayInCurrentCompany_DifferentRelatedJob_SameChargeCode_CombineInvoiceLinesByChargeCodeRegistryOff()
		{
			AssertTargetJobIsBlankConsolIsNotGatewayInCurrentCompanyCore(RelatedJobOptions.Different, true, false);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingGatewayARInvoice_TargetJobIsBlank_ConsolIsNotGatewayInCurrentCompany_DifferentRelatedJob_DifferentChargeCodes()
		{
			AssertTargetJobIsBlankConsolIsNotGatewayInCurrentCompanyCore(RelatedJobOptions.Different, false);
		}

		void AssertTargetJobIsBlankConsolIsNotGatewayInCurrentCompanyCore(RelatedJobOptions relatedJobOption, bool isSameChargeCode, bool isCombineInvoiceLinesByChargeCodeRegistryOn = false)
		{
			SetupSisterCompaniesChargesCodesAndPeriods(out GlbCompany sisterCompany, out GlbBranch sisterBranch, out AccChargeCode chargeCode1, out AccChargeCode chargeCode2);

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol("USLAX", "AUMEL", "C00356", receivingGatewayCompany: sisterCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S1111", gatewayConsol);
			var shipment2 = TestObjectCreator.CreateShipment("S2222", gatewayConsol);
			Factory.Save();

			InvoicingBase arInvoice = null;
			var debtorForARInvoice = GlbCompany.CurrentCompany.OrgProxy;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sisterBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol);
				arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				arInvoice.AH_ConsolidatedInvoiceRef = gatewayConsol.JK_UniqueConsignRef;
				arInvoice.AH_JH = gatewayJob.PK;
				var arInvoiceLine1 = TestObjectCreator.CreateInvoiceLine(arInvoice, gatewayJob, chargeCode1, 100m, TestObjectCreator.AUD, 1m);
				var relatedJobForCharge1 = relatedJobOption == RelatedJobOptions.Blank ? null : shipment1;
				TestObjectCreator.CreateChargeWithTarget(arInvoiceLine1, relatedJobForCharge1, null);
				var arInvoiceLine2 = TestObjectCreator.CreateInvoiceLine(arInvoice, gatewayJob, isSameChargeCode ? chargeCode1 : chargeCode2, 200m, TestObjectCreator.AUD, 1m);
				var relatedJobForCharge2 = relatedJobOption == RelatedJobOptions.Blank ? null : (relatedJobOption == RelatedJobOptions.Different ? shipment2 : shipment1);
				TestObjectCreator.CreateChargeWithTarget(arInvoiceLine2, relatedJobForCharge2, null);
				Factory.Save();
			}

			InvoicingBase convertedAPInvoice = null;
			var expectedApportionMethod = string.Empty;
			using (AccountingConfigurationRegistry.Instance.CombineInvoiceLinesByChargeCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isCombineInvoiceLinesByChargeCodeRegistryOn))
			using (AccountingConfigurationRegistry.Instance.ConsolCostDefaultApportionmentMethod.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new ConsolCostDefaultApportionmentMethodConfiguration()))
			{
				var defaultApportionmentMethod = (string)gatewayConsol.GetApportionmentMethod(TestObjectCreator.FRT);
				AssertNotEquals("Pre-condition: Default apportionment method is not manual", AllocationMethod.Manual, defaultApportionmentMethod);
				expectedApportionMethod = relatedJobOption == RelatedJobOptions.Blank ? defaultApportionmentMethod : AllocationMethod.Manual;

				var converter = new UnapprovedTransactionConverter(Factory);
				convertedAPInvoice = converter.ConvertToAP(arInvoice, true);
			}

			var expectedNumberOfConsolCosts = isSameChargeCode ? 1 : 2;
			AssertEquals(expectedNumberOfConsolCosts, convertedAPInvoice.ConsolCosting.ConsolCosts.Count);

			var consolCosts = convertedAPInvoice.ConsolCosting.ConsolCosts.Cast<JobConsolCost>();
			if (expectedNumberOfConsolCosts == 1)
			{
				AssertEquals(1, consolCosts.Count(x => x.E6_ParentID == gatewayConsol.PK && x.ChargeCode.AC_Code == chargeCode1.AC_Code && x.E6_OSCostAmount == 300m));
			}
			else
			{
				AssertEquals(1, consolCosts.Count(x => x.E6_ParentID == gatewayConsol.PK && x.ChargeCode.AC_Code == chargeCode1.AC_Code && x.E6_OSCostAmount == 100m));
				var expectedConsolCost2ChargeCode = isSameChargeCode ? chargeCode1.AC_Code : chargeCode2.AC_Code;
				AssertEquals(1, consolCosts.Count(x => x.E6_ParentID == gatewayConsol.PK && x.ChargeCode.AC_Code == expectedConsolCost2ChargeCode && x.E6_OSCostAmount == 200m));
			}
			consolCosts.ForEach(x => AssertEquals(expectedApportionMethod, x.E6_ApportionmentMethod));

			var expectedNumberOfInvoiceLines = isSameChargeCode ? (relatedJobOption == RelatedJobOptions.Same ? 1 : 2) : (relatedJobOption == RelatedJobOptions.Blank ? 4 : 2);
			AssertEquals(expectedNumberOfInvoiceLines, convertedAPInvoice.Lines.Count);

			var invoiceLines = convertedAPInvoice.Lines.Cast<InvoicingLineBase>();
			if (expectedNumberOfInvoiceLines == 1)
			{
				AssertEquals(1, invoiceLines.Count(x => x.ChargeCode.AC_Code == chargeCode1.AC_Code && x.Job.JH_ParentID == shipment1.PK && x.AL_LineAmount == -300m));
			}
			else if (expectedNumberOfInvoiceLines == 4)
			{
				AssertEquals(1, invoiceLines.Count(x => x.ChargeCode.AC_Code == chargeCode1.AC_Code && x.Job.JH_ParentID == shipment1.PK && x.AL_LineAmount == -50m));
				AssertEquals(1, invoiceLines.Count(x => x.ChargeCode.AC_Code == chargeCode1.AC_Code && x.Job.JH_ParentID == shipment2.PK && x.AL_LineAmount == -50m));
				AssertEquals(1, invoiceLines.Count(x => x.ChargeCode.AC_Code == chargeCode2.AC_Code && x.Job.JH_ParentID == shipment1.PK && x.AL_LineAmount == -100m));
				AssertEquals(1, invoiceLines.Count(x => x.ChargeCode.AC_Code == chargeCode2.AC_Code && x.Job.JH_ParentID == shipment2.PK && x.AL_LineAmount == -100m));
			}
			else
			{
				var expectedInvoiceLine1Amount = (relatedJobOption == RelatedJobOptions.Blank && isSameChargeCode) ? -150m : -100m;
				AssertEquals(1, invoiceLines.Count(x => x.ChargeCode.AC_Code == chargeCode1.AC_Code && x.Job.JH_ParentID == shipment1.PK && x.AL_LineAmount == expectedInvoiceLine1Amount));
				var expectedInvoiceLine2ChargeCode = isSameChargeCode ? chargeCode1.AC_Code : chargeCode2.AC_Code;
				var expectedInvoiceLine2JobParent = relatedJobOption == RelatedJobOptions.Same ? shipment1.PK : shipment2.PK;
				var expectedInvoiceLine2Amount = relatedJobOption == RelatedJobOptions.Blank && isSameChargeCode ? -150m : -200m;
				AssertEquals(1, invoiceLines.Count(x => x.ChargeCode.AC_Code == expectedInvoiceLine2ChargeCode && x.Job.JH_ParentID == expectedInvoiceLine2JobParent && x.AL_LineAmount == expectedInvoiceLine2Amount));
			}

			convertedAPInvoice.RunPreSaveValidation();
			AssertNoErrors(convertedAPInvoice);
		}

		#endregion

		public void TestGeneratedConsolCostsAreSplitBasedUponLineRelatedJobNumber()
		{
			SetupSisterCompaniesChargesCodesAndPeriods(out GlbCompany sisterCompany, out GlbBranch sisterBranch, out AccChargeCode chargeCode, out _);

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol("USLAX", "AUMEL", "C00356", receivingGatewayCompany: sisterCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S1111", gatewayConsol);
			var shipment2 = TestObjectCreator.CreateShipment("S2222", gatewayConsol);
			Factory.Save();

			InvoicingBase arInvoice = null;
			var debtorForARInvoice = GlbCompany.CurrentCompany.OrgProxy;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sisterBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol);
				arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				arInvoice.AH_ConsolidatedInvoiceRef = gatewayConsol.JK_UniqueConsignRef;
				arInvoice.AH_JH = gatewayJob.PK;

				var chargeAmount = 0;
				foreach (var relatedJob in new[] { null, shipment1, shipment2, null, shipment1, shipment2 })
				{
					var arInvoiceLine = TestObjectCreator.CreateInvoiceLine(arInvoice, gatewayJob, chargeCode, chargeAmount += 100, TestObjectCreator.AUD, 1m);
					TestObjectCreator.CreateChargeWithTarget(arInvoiceLine, relatedJob, null);
				}

				Factory.Save();
			}

			InvoicingBase convertedAPInvoice = null;
			var defaultApportionmentMethod = string.Empty;
			using (AccountingConfigurationRegistry.Instance.CombineInvoiceLinesByChargeCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (AccountingConfigurationRegistry.Instance.ConsolCostDefaultApportionmentMethod.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new ConsolCostDefaultApportionmentMethodConfiguration()))
			{
				defaultApportionmentMethod = gatewayConsol.GetApportionmentMethod(TestObjectCreator.FRT);
				AssertNotEquals("Pre-condition: Default apportionment method is not manual", AllocationMethod.Manual, defaultApportionmentMethod);

				var converter = new UnapprovedTransactionConverter(Factory);
				convertedAPInvoice = converter.ConvertToAP(arInvoice, true);
			}

			var consolCosts = convertedAPInvoice.ConsolCosting.ConsolCosts.Cast<JobConsolCost>();
			AssertEquals("charges only differ in related job number should be merged into 1 consol cost", 2, consolCosts.Count());

			var noRelatedCost = consolCosts.First(x => x.E6_ParentID == gatewayConsol.PK && x.ChargeCode.AC_Code == chargeCode.AC_Code && x.E6_OSCostAmount == 500);
			AssertEquals(defaultApportionmentMethod, noRelatedCost.E6_ApportionmentMethod);
			AssertEquals("Amount is split between charges according to method", 2, noRelatedCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Count(x => x.JR_OSCostAmt == 250));

			var shipment1and2Cost = consolCosts.First(x => x.E6_ParentID == gatewayConsol.PK && x.ChargeCode.AC_Code == chargeCode.AC_Code && x.E6_OSCostAmount == 1600);
			AssertEquals(AllocationMethod.Manual, shipment1and2Cost.E6_ApportionmentMethod);
			AssertEquals("Cost for shipment 1 is only apportioned to shipment 1", 1, shipment1and2Cost.ApportionmentCharges.Cast<ApportionSplitCharge>().Count(x => x.JR_JobNumber == shipment1.JobNumber && x.JR_OSCostAmt == 700));
			AssertEquals("Cost for shipment 2 is only apportioned to shipment 2", 1, shipment1and2Cost.ApportionmentCharges.Cast<ApportionSplitCharge>().Count(x => x.JR_JobNumber == shipment2.JobNumber && x.JR_OSCostAmt == 900));
		}

		public void TestGeneratedConsolCostsFinalFlagIsBasedOnPayableFinalFlagRegistry()
		{
			SetupSisterCompaniesChargesCodesAndPeriods(out GlbCompany sisterCompany, out GlbBranch sisterBranch, out AccChargeCode chargeCode, out _);

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol("USLAX", "AUMEL", "C00356", receivingGatewayCompany: sisterCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S1111", gatewayConsol);
			var shipment2 = TestObjectCreator.CreateShipment("S2222", gatewayConsol);
			Factory.Save();

			InvoicingBase arInvoice = null;
			var debtorForARInvoice = GlbCompany.CurrentCompany.OrgProxy;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sisterBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol);
				arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				arInvoice.AH_ConsolidatedInvoiceRef = gatewayConsol.JK_UniqueConsignRef;
				arInvoice.AH_JH = gatewayJob.PK;

				var chargeAmount = 0;
				foreach (var relatedJob in new[] { null, shipment1, shipment2 })
				{
					var arInvoiceLine = TestObjectCreator.CreateInvoiceLine(arInvoice, gatewayJob, chargeCode, chargeAmount += 100, TestObjectCreator.AUD, 1m);
					TestObjectCreator.CreateChargeWithTarget(arInvoiceLine, relatedJob, null);
				}

				Factory.Save();
			}

			InvoicingBase convertedAPInvoice = null;
			var defaultApportionmentMethod = string.Empty;
			using (AccountingConfigurationRegistry.Instance.CombineInvoiceLinesByChargeCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (AccountingConfigurationRegistry.Instance.ConsolCostDefaultApportionmentMethod.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new ConsolCostDefaultApportionmentMethodConfiguration()))
			{
				defaultApportionmentMethod = gatewayConsol.GetApportionmentMethod(TestObjectCreator.FRT);
				AssertNotEquals("Pre-condition: Default apportionment method is not manual", AllocationMethod.Manual, defaultApportionmentMethod);

				var converter = new UnapprovedTransactionConverter(Factory);

				foreach (var registryValue in new[] { true, false })
				{
					using (AccountingConfigurationRegistry.Instance.PayableFinalFlagForConsolCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
					{
						convertedAPInvoice = converter.ConvertToAP(arInvoice, true);
						var consolCosts = convertedAPInvoice.ConsolCosting.ConsolCosts.Cast<JobConsolCost>();
						AssertEquals(2, consolCosts.Count());
						consolCosts.ForEach(x => AssertEquals(registryValue, x.IsFinal));
					}
				}
			}
		}

		enum RelatedJobOptions
		{
			Blank,
			Same,
			Different
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingAPInvoicesFromSisterCompanies_GatewayConsolToGatewayConsol()
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
			Factory.Save();

			ARInvoice arInvoice = null;
			ZString gatewayJobNumer = ZString.Empty;
			var receivingGatewayBranchOrgProxy = Factory.Load<OrgHeader>(GlbBranch.CurrentBranch.OrgProxy.PK);
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sendingGatewayBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				receivingGatewayBranchOrgProxy.CompanyData.OB_IsDebtor = true;
				var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol);
				Factory.Save();

				gatewayJobNumer = gatewayJob.JH_JobNum;

				arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1.0m, receivingGatewayBranchOrgProxy);
				arInvoice.AH_OSExTaxAmount = 750m;
				arInvoice.AH_ConsolidatedInvoiceRef = gatewayConsol.JK_UniqueConsignRef;
				arInvoice.AH_JH = gatewayJob.PK;

				var arInvoiceLine1 = TestObjectCreator.CreateInvoiceLine(arInvoice, gatewayJob, chargeCode1, 150, TestObjectCreator.AUD, 1.0m);
				var arInvoiceLine2 = TestObjectCreator.CreateInvoiceLine(arInvoice, gatewayJob, chargeCode2, 600, TestObjectCreator.AUD, 1.0m);
				var charge1 = TestObjectCreator.CreateCharge(arInvoiceLine1);
				var charge2 = TestObjectCreator.CreateCharge(arInvoiceLine2);
				Factory.Save();
			}

			var converter = new UnapprovedTransactionConverter(Factory);
			var convertedAPInvoice = converter.ConvertToAP(arInvoice, true);

			AssertEquals(0, convertedAPInvoice.ConsolCosting.ConsolCosts.Count);
			AssertEquals(2, convertedAPInvoice.Lines.Count);

			var invoiceLines = convertedAPInvoice.Lines.Cast<InvoicingLineBase>();

			var lineAmountForchargeCode1 = invoiceLines.Where(x => x.ChargeCode.AC_Code == chargeCode1.AC_Code).Sum(x => x.AL_OSAmount);
			AssertEquals(150m, -lineAmountForchargeCode1);
			var lineAmountForchargeCode2 = invoiceLines.Where(x => x.ChargeCode.AC_Code == chargeCode2.AC_Code).Sum(x => x.AL_OSAmount);
			AssertEquals(600m, -lineAmountForchargeCode2);

			Assert(invoiceLines.All(x => x.Job.JH_JobNum == gatewayJobNumer));

			convertedAPInvoice.RunPreSaveValidation();
			AssertNoErrors(convertedAPInvoice);
		}

		[TestDate(2019, 01, 01)]
		public void TestImportingAPInvoicesFromSisterCompanies_DefaultTaxNonZero_OverrideTaxIsZero()
		{
			var defaultTax = TestObjectCreator.GST1;
			var overrideTax = TestObjectCreator.GSTFREE1;
			ImportingAPInvoicesFromSisterCompanies_GatewayConsolToGatewayConsolWithConsolInvoice_TaxOverride(defaultTax, overrideTax, overrideTax);
		}

		[TestDate(2019, 01, 01)]
		public void TestImportingAPInvoicesFromSisterCompanies_DefaultTaxIsZero_OverrideTaxIsZero()
		{
			var defaultTax = TestObjectCreator.GSTFREE1;
			var overrideTax = TestObjectCreator.GSTFREE1;
			ImportingAPInvoicesFromSisterCompanies_GatewayConsolToGatewayConsolWithConsolInvoice_TaxOverride(defaultTax, overrideTax, overrideTax);
		}

		[TestDate(2019, 01, 01)]
		public void TestImportingAPInvoicesFromSisterCompanies_DefaultTaxIsZero_OverrideTaxNonZero()
		{
			var defaultTax = TestObjectCreator.GSTFREE1;
			var overrideTax = TestObjectCreator.GST1;
			var notReportTaxRate = AccTaxRate.GetNOTREPORTTaxID(Factory, GlbCompany.CurrentCompany);
			ImportingAPInvoicesFromSisterCompanies_GatewayConsolToGatewayConsolWithConsolInvoice_TaxOverride(defaultTax, overrideTax, notReportTaxRate);
		}

		[TestDate(2019, 01, 01)]
		public void TestImportingAPInvoicesFromSisterCompanies_DefaultTaxNonZero_OverrideTaxNonZero()
		{
			var defaultTax = TestObjectCreator.GST1;
			var overrideTax = TestObjectCreator.GST1;
			var notReportTaxRate = AccTaxRate.GetNOTREPORTTaxID(Factory, GlbCompany.CurrentCompany);
			ImportingAPInvoicesFromSisterCompanies_GatewayConsolToGatewayConsolWithConsolInvoice_TaxOverride(defaultTax, overrideTax, notReportTaxRate);
		}

		void ImportingAPInvoicesFromSisterCompanies_GatewayConsolToGatewayConsolWithConsolInvoice_TaxOverride(AccTaxRate originalTax, AccTaxRate overrideTax, AccTaxRate expectedTax)
		{
			var sendingGatewayCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("GTWORG", true, false);
			var sendingGatewayCompany = TestObjectCreator.CreateNewCompany("CGA", orgProxy: sendingGatewayCompanyOrgProxy);
			var sendingGatewayBranch = TestObjectCreator.CreateNewBranch(sendingGatewayCompany, "BGA");
			var receivingGatewayDepartment = Factory.Load<GlbDepartment>(GlbDepartment.CurrentDepartment.PK);
			receivingGatewayDepartment.GE_Misc = false;

			periodManagementTestHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year, GlbCompany.CurrentCompany.PK);
			periodManagementTestHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year, sendingGatewayCompany.PK);

			var chargeCode_SendingCompany = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1 for sending company", ChargeType.Margin, 100, TestObjectCreator.GSTFREE1, TestObjectCreator.WHTFREE1, sendingGatewayCompany);
			var chargeCode_ReceivingCompany = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1 for receiving company", ChargeType.Margin, 100, originalTax, TestObjectCreator.WHTFREE1);

			var origin = "USLAX";
			var destination = "AUMEL";
			var consolNumber = "C001";
			TestObjectCreator.CreateTaxOverride(chargeCode_ReceivingCompany, overrideTax.PK, ZGuid.Empty, OrgConstants.ServiceDirection.Code.Import, AccChargeTaxOverrideLookups.Cost
				, "ALL", JobInvoicingConsumerTypes.ForwardingConsol.Code, origin.Substring(0, 2), destination.Substring(0, 2));

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol(origin, destination, consolNumber, sendingGatewayCompany, null);
			var shipment1 = TestObjectCreator.CreateShipment("S001", gatewayConsol);
			Factory.Save();

			ARInvoice arInvoice = null;
			var receivingGatewayBranchOrgProxy = Factory.Load<OrgHeader>(GlbBranch.CurrentBranch.OrgProxy.PK);
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sendingGatewayBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				receivingGatewayBranchOrgProxy.CompanyData.OB_IsDebtor = true;
				Factory.Save();
				var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol);
				arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV1", TestObjectCreator.AUD, 1m, receivingGatewayBranchOrgProxy);
				arInvoice.AH_ConsolidatedInvoiceRef = gatewayConsol.JK_UniqueConsignRef;
				arInvoice.AH_JH = gatewayJob.PK;
				var arInvoiceLine = TestObjectCreator.CreateInvoiceLine(arInvoice, gatewayJob, chargeCode_SendingCompany, 200m, TestObjectCreator.AUD, 1.0m);

				TestObjectCreator.CreateCharge(arInvoiceLine);
				Factory.Save();
			}

			var converter = new UnapprovedTransactionConverter(Factory);
			var convertedAPInvoice = converter.ConvertToAP(arInvoice, true);

			AssertEquals(1, convertedAPInvoice.Lines.Count);

			var invoiceLine = convertedAPInvoice.Lines.Cast<InvoicingLineBase>().First();

			AssertEquals("Overridden tax defaulted to line", expectedTax.PK, invoiceLine.AL_AT);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingAPInvoicesFromSisterCompaniesWithoutExchangeRate()
		{
			var italyCompany = TestObjectCreator.CreateNewCompany("ITL", Core.Constants.CountryCodes.Italy);
			var italyBranch = TestObjectCreator.CreateNewBranch(italyCompany, "ITL");
			OrgHeader italyCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("ITORGPROXY", true, true, "ITMIL");
			italyCompany.GC_OH_OrgProxy = italyCompanyOrgProxy.PK;
			italyCompany.GC_RX_NKLocalCurrency = Enterprise.Core.Constants.CurrencyCodes.EuropeanUnion;
			periodManagementTestHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year, italyCompany.PK);

			var gatewayCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("GTWORG", true, false, "AUMEL");
			var gatewayCompany = TestObjectCreator.CreateNewCompany("CGA", orgProxy: gatewayCompanyOrgProxy);
			var gatewayBranch = TestObjectCreator.CreateNewBranch(gatewayCompany, "BGA");
			periodManagementTestHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year, gatewayCompany.PK);

			TestObjectCreator.GST1.SetRate_ForTestOnly(10, 1, new ZDate(2014, 12, 1), new ZDate(2015, 2, 1));
			var chargeCode1 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1 for gateway company", ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1, gatewayCompany);
			var chargeCode2 = TestObjectCreator.CreateChargeCode("CC2", "Charge Code 2 for gateway company", ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1, gatewayCompany);

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol("ITMIL", "AUMEL", "C001", receivingGatewayCompany: gatewayCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S001", gatewayConsol);
			var shipment2 = TestObjectCreator.CreateShipment("S002", gatewayConsol);
			Factory.Save();

			ARInvoice arInvoice = null;
			var forwardingBranchOrgProxy = Factory.Load<OrgHeader>(GlbBranch.CurrentBranch.OrgProxy.PK);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, gatewayBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				forwardingBranchOrgProxy.CompanyData.OB_IsDebtor = true;
				Factory.Save();

				var job1 = new Job.Loader(shipment1).TryCreateWithoutMutexForTestOnly();
				var job2 = new Job.Loader(shipment2).TryCreateWithoutMutexForTestOnly();
				arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV1", TestObjectCreator.AUD, 1m, forwardingBranchOrgProxy);
				arInvoice.AH_ConsolidatedInvoiceRef = gatewayConsol.JK_UniqueConsignRef;
				arInvoice.AH_JH = ZGuid.Empty;

				var arInvoiceLine1 = TestObjectCreator.CreateInvoiceLine(arInvoice, job1, chargeCode1, 190m, TestObjectCreator.AUD, 1.0m);
				var arInvoiceLine2 = TestObjectCreator.CreateInvoiceLine(arInvoice, job2, chargeCode1, 110m, TestObjectCreator.AUD, 1.0m);

				TestObjectCreator.CreateCharge(arInvoiceLine1);
				TestObjectCreator.CreateCharge(arInvoiceLine2);
				Factory.Save();
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, italyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code);

				TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1 for forwarding company", ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);
				TestObjectCreator.CreateChargeCode("CC2", "Charge Code 2 for forwarding company", ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);

				var converter = new UnapprovedTransactionConverter(Factory);
				var convertedAPInvoice = converter.ConvertToAP(arInvoice, true);
				AssertEquals(1, convertedAPInvoice.ConsolCosting.ConsolCosts.Count);
			}
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingAPInvoicesFromSisterCompanies_GatewayConsolToForwardingConsol()
		{
			var gatewayCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("GTWORG", true, false, "AUMEL");
			var gatewayCompany = TestObjectCreator.CreateNewCompany("CGA", orgProxy: gatewayCompanyOrgProxy);
			var gatewayBranch = TestObjectCreator.CreateNewBranch(gatewayCompany, "BGA");
			periodManagementTestHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year, GlbCompany.CurrentCompany.PK);
			periodManagementTestHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year, gatewayCompany.PK);

			TestObjectCreator.GST1.SetRate_ForTestOnly(10, 1, new ZDate(2014, 12, 1), new ZDate(2015, 2, 1));
			var chargeCode1 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1 for gateway company", ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1, gatewayCompany);
			var chargeCode2 = TestObjectCreator.CreateChargeCode("CC2", "Charge Code 2 for gateway company", ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1, gatewayCompany);
			TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1 for forwarding company", ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);
			TestObjectCreator.CreateChargeCode("CC2", "Charge Code 2 for forwarding company", ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol("USLAX", "AUMEL", "C001", receivingGatewayCompany: gatewayCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S001", gatewayConsol);
			var shipment2 = TestObjectCreator.CreateShipment("S002", gatewayConsol);
			Factory.Save();

			ARInvoice arInvoice = null;
			var forwardingBranchOrgProxy = Factory.Load<OrgHeader>(GlbBranch.CurrentBranch.OrgProxy.PK);
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, gatewayBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				forwardingBranchOrgProxy.CompanyData.OB_IsDebtor = true;
				Factory.Save();

				var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol);
				arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV1", TestObjectCreator.AUD, 1m, forwardingBranchOrgProxy);
				arInvoice.AH_ConsolidatedInvoiceRef = gatewayConsol.JK_UniqueConsignRef;
				arInvoice.AH_JH = gatewayJob.PK;

				var arInvoiceLine1 = TestObjectCreator.CreateInvoiceLine(arInvoice, gatewayJob, chargeCode1, 11m, TestObjectCreator.AUD, 1.0m);
				var arInvoiceLine2 = TestObjectCreator.CreateInvoiceLine(arInvoice, gatewayJob, chargeCode1, 227.65m, TestObjectCreator.AUD, 1.0m);
				var arInvoiceLine3 = TestObjectCreator.CreateInvoiceLine(arInvoice, gatewayJob, chargeCode2, 1234.99m, TestObjectCreator.AUD, 1.0m);
				var arInvoiceLine4 = TestObjectCreator.CreateInvoiceLine(arInvoice, gatewayJob, chargeCode2, 400.01m, TestObjectCreator.AUD, 1.0m);

				TestObjectCreator.CreateCharge(arInvoiceLine1);
				TestObjectCreator.CreateCharge(arInvoiceLine2);
				TestObjectCreator.CreateCharge(arInvoiceLine3);
				TestObjectCreator.CreateCharge(arInvoiceLine4);
				Factory.Save();
			}

			var converter = new UnapprovedTransactionConverter(Factory);
			var convertedAPInvoice = converter.ConvertToAP(arInvoice, true);
			AssertEquals(2, convertedAPInvoice.ConsolCosting.ConsolCosts.Count);

			var consolCostsWithChargeCode1 = convertedAPInvoice.ConsolCosting.ConsolCosts.Where(x => x.ChargeCode.AC_Code == chargeCode1.AC_Code);
			AssertEquals(1, consolCostsWithChargeCode1.Count());
			var consolCost1 = consolCostsWithChargeCode1.First();
			AssertEquals(238.65m, consolCost1.E6_OSCostAmount);
			AssertEquals(238.65m, consolCost1.E6_LocalCostAmount);
			AssertEquals(23.87m, consolCost1.E6_OSGSTAmount_Calc);

			var consolCostsWithChargeCode2 = convertedAPInvoice.ConsolCosting.ConsolCosts.Where(x => x.ChargeCode.AC_Code == chargeCode2.AC_Code);
			AssertEquals(1, consolCostsWithChargeCode2.Count());
			var consolCost2 = consolCostsWithChargeCode2.First();
			AssertEquals(1635m, consolCost2.E6_OSCostAmount);
			AssertEquals(1635m, consolCost2.E6_LocalCostAmount);
			AssertEquals(163.5m, consolCost2.E6_OSGSTAmount_Calc);

			var convertedAPInvoiceLines = convertedAPInvoice.Lines.Cast<InvoicingLineBase>();
			var apLinesWithChargeCode1 = convertedAPInvoiceLines.Where(x => x.ChargeCode.AC_Code == chargeCode1.AC_Code);
			AssertEquals(2, apLinesWithChargeCode1.Count());
			AssertEquals(238.65m, apLinesWithChargeCode1.Sum(x => x.AL_OSExTaxAmount));
			AssertEquals(238.65m, apLinesWithChargeCode1.Sum(x => x.AL_LocalExTaxAmount));
			AssertEquals(23.87m, apLinesWithChargeCode1.Sum(x => x.AL_OSTaxAmount));

			var apLinesWithChargeCode2 = convertedAPInvoiceLines.Where(x => x.ChargeCode.AC_Code == chargeCode2.AC_Code);
			AssertEquals(2, apLinesWithChargeCode2.Count());
			AssertEquals(1635m, apLinesWithChargeCode2.Sum(x => x.AL_OSExTaxAmount));
			AssertEquals(1635m, apLinesWithChargeCode2.Sum(x => x.AL_LocalExTaxAmount));
			AssertEquals(163.5m, apLinesWithChargeCode2.Sum(x => x.AL_OSTaxAmount));

			AssertEquals(2, convertedAPInvoiceLines.Count(x => x.Job.JH_ParentID == shipment1.PK));
			AssertEquals(2, convertedAPInvoiceLines.Count(x => x.Job.JH_ParentID == shipment2.PK));

			convertedAPInvoice.RunPreSaveValidation();
			AssertNoErrors(convertedAPInvoice);
		}

		[TestDate(2015, 01, 01)]
		public void TestImportingAPInvoicesFromSisterCompanies_GatewayConsolToForwardingConsolWithConsolInvoice()
		{
			var gatewayCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("GTWORG", true, false, "AUMEL");
			var gatewayCompany = TestObjectCreator.CreateNewCompany("CGA", orgProxy: gatewayCompanyOrgProxy);
			var gatewayBranch = TestObjectCreator.CreateNewBranch(gatewayCompany, "BGA");
			periodManagementTestHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year, GlbCompany.CurrentCompany.PK);
			periodManagementTestHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year, gatewayCompany.PK);

			TestObjectCreator.GST1.SetRate_ForTestOnly(10, 1, new ZDate(2014, 12, 1), new ZDate(2015, 2, 1));
			var chargeCode1 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1 for gateway company", ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1, gatewayCompany);
			var chargeCode2 = TestObjectCreator.CreateChargeCode("CC2", "Charge Code 2 for gateway company", ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1, gatewayCompany);
			TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1 for forwarding company", ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);
			TestObjectCreator.CreateChargeCode("CC2", "Charge Code 2 for forwarding company", ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol("USLAX", "AUMEL", "C001", receivingGatewayCompany: gatewayCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S001", gatewayConsol);
			var shipment2 = TestObjectCreator.CreateShipment("S002", gatewayConsol);
			Factory.Save();

			ARInvoice arInvoice = null;
			var forwardingBranchOrgProxy = Factory.Load<OrgHeader>(GlbBranch.CurrentBranch.OrgProxy.PK);
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, gatewayBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				forwardingBranchOrgProxy.CompanyData.OB_IsDebtor = true;
				Factory.Save();

				var job1 = new Job.Loader(shipment1).TryCreateWithoutMutexForTestOnly();
				var job2 = new Job.Loader(shipment2).TryCreateWithoutMutexForTestOnly();
				arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV1", TestObjectCreator.AUD, 1m, forwardingBranchOrgProxy);
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
			AssertEquals(1, convertedAPInvoice.ConsolCosting.ConsolCosts.Count);

			var consolCostsWithChargeCode1 = convertedAPInvoice.ConsolCosting.ConsolCosts.Where(x => x.ChargeCode.AC_Code == chargeCode1.AC_Code);
			AssertEquals(1, consolCostsWithChargeCode1.Count());
			var consolCost1 = consolCostsWithChargeCode1.First();
			AssertEquals(300m, consolCost1.E6_OSCostAmount);
			AssertEquals(300m, consolCost1.E6_LocalCostAmount);
			AssertEquals(30m, consolCost1.E6_OSGSTAmount_Calc);

			var convertedAPInvoiceLines = convertedAPInvoice.Lines.Cast<InvoicingLineBase>();
			var apLinesWithChargeCode1 = convertedAPInvoiceLines.Where(x => x.ChargeCode.AC_Code == chargeCode1.AC_Code).ToList();
			AssertEquals(2, apLinesWithChargeCode1.Count);
			AssertContainsExactElementsInAnyOrder(new ZDecimal[] { -190.00m, -110.00m }, apLinesWithChargeCode1.Select(x => x.AL_LineAmount));

			AssertEquals(1, convertedAPInvoiceLines.Count(x => x.Job.JH_ParentID == shipment1.PK));
			AssertEquals(1, convertedAPInvoiceLines.Count(x => x.Job.JH_ParentID == shipment2.PK));

			convertedAPInvoice.RunPreSaveValidation();
			AssertNoErrors(convertedAPInvoice);
		}

		[TestDate(2015, 01, 01)]
		public void TestSisterCompanyConsolInvoiceImport_GatewayConsolToForwardingConsol_TaxDateUseTodaysDate()
		{
			AssertSisterCompanyConsolInvoiceImport_GatewayConsolToForwardingConsol_TaxDate(TaxDateDefaultingOption.Code.Today);
		}

		[TestDate(2015, 01, 01)]
		public void TestSisterCompanyConsolInvoiceImport_GatewayConsolToForwardingConsol_TaxDateUseInvoiceDate()
		{
			AssertSisterCompanyConsolInvoiceImport_GatewayConsolToForwardingConsol_TaxDate(TaxDateDefaultingOption.Code.InvoiceDate);
		}

		void AssertSisterCompanyConsolInvoiceImport_GatewayConsolToForwardingConsol_TaxDate(string taxDateDefaultingOption)
		{
			var gatewayCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("GTWORG", true, false, "AUMEL");
			var gatewayCompany = TestObjectCreator.CreateNewCompany("CGA", orgProxy: gatewayCompanyOrgProxy);
			var gatewayBranch = TestObjectCreator.CreateNewBranch(gatewayCompany, "BGA");
			periodManagementTestHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year, GlbCompany.CurrentCompany.PK);
			periodManagementTestHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year, gatewayCompany.PK);

			TestObjectCreator.GST1.SetRate_ForTestOnly(10, 1, new ZDate(2014, 12, 1), new ZDate(2015, 2, 1));
			var chargeCode1 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1 for gateway company", ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1, gatewayCompany);
			var chargeCode2 = TestObjectCreator.CreateChargeCode("CC2", "Charge Code 2 for gateway company", ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1, gatewayCompany);
			TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1 for forwarding company", ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);
			TestObjectCreator.CreateChargeCode("CC2", "Charge Code 2 for forwarding company", ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol("USLAX", "AUMEL", "C001", receivingGatewayCompany: gatewayCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S001", gatewayConsol);
			var shipment2 = TestObjectCreator.CreateShipment("S002", gatewayConsol);
			Factory.Save();

			ARInvoice arInvoice = null;
			var forwardingBranchOrgProxy = Factory.Load<OrgHeader>(GlbBranch.CurrentBranch.OrgProxy.PK);
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, gatewayBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				forwardingBranchOrgProxy.CompanyData.OB_IsDebtor = true;
				Factory.Save();

				var job1 = new Job.Loader(shipment1).TryCreateWithoutMutexForTestOnly();
				var job2 = new Job.Loader(shipment2).TryCreateWithoutMutexForTestOnly();
				arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV1", TestObjectCreator.AUD, 1m, forwardingBranchOrgProxy);
				arInvoice.AH_ConsolidatedInvoiceRef = gatewayConsol.JK_UniqueConsignRef;
				arInvoice.AH_JH = ZGuid.Empty;
				arInvoice.AH_InvoiceDate = new ZDateTime(2014, 12, 30);

				var arInvoiceLine1 = TestObjectCreator.CreateInvoiceLine(arInvoice, job1, chargeCode1, 190m, TestObjectCreator.AUD, 1.0m);
				var arInvoiceLine2 = TestObjectCreator.CreateInvoiceLine(arInvoice, job2, chargeCode1, 110m, TestObjectCreator.AUD, 1.0m);

				TestObjectCreator.CreateCharge(arInvoiceLine1);
				TestObjectCreator.CreateCharge(arInvoiceLine2);
				Factory.Save();
			}

			var collection = new TaxDateDefaultingOptionCollection();
			var taxDateOption = collection.AddNew();
			taxDateOption.JobType = "FCN";
			taxDateOption.DirectionCode = "ALL";
			taxDateOption.Mode = "ALL";
			taxDateOption.Ledger = "AP";
			taxDateOption.TaxDateOption = taxDateDefaultingOption;
			using (AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
			{
				var converter = new UnapprovedTransactionConverter(Factory);
				var convertedAPInvoice = converter.ConvertToAP(arInvoice, true);
				AssertEquals(1, convertedAPInvoice.ConsolCosting.ConsolCosts.Count);

				var consolCostsWithChargeCode1 = convertedAPInvoice.ConsolCosting.ConsolCosts.Where(x => x.ChargeCode.AC_Code == chargeCode1.AC_Code);
				AssertEquals(1, consolCostsWithChargeCode1.Count());
				var consolCost1 = consolCostsWithChargeCode1.First();

				var convertedAPInvoiceLines = convertedAPInvoice.Lines.Cast<InvoicingLineBase>();
				var apLinesWithChargeCode1 = convertedAPInvoiceLines.Where(x => x.ChargeCode.AC_Code == chargeCode1.AC_Code).ToList();
				AssertEquals(2, apLinesWithChargeCode1.Count);
				AssertContainsExactElementsInAnyOrder(new ZDecimal[] { -190.00m, -110.00m }, apLinesWithChargeCode1.Select(x => x.AL_LineAmount));

				AssertEquals(1, convertedAPInvoiceLines.Count(x => x.Job.JH_ParentID == shipment1.PK));
				AssertEquals(1, convertedAPInvoiceLines.Count(x => x.Job.JH_ParentID == shipment2.PK));

				if (taxDateDefaultingOption == TaxDateDefaultingOption.Code.Today)
				{
					AssertEquals(new ZDate(2015, 1, 1), consolCost1.E6_TaxDate);
					AssertEquals(new ZDate(2015, 1, 1), convertedAPInvoiceLines.First().AL_TaxDate);
					AssertEquals(new ZDate(2015, 1, 1), convertedAPInvoiceLines.Last().AL_TaxDate);
				}
				else if (taxDateDefaultingOption == TaxDateDefaultingOption.Code.InvoiceDate)
				{
					AssertEquals(new ZDate(2014, 12, 30), consolCost1.E6_TaxDate);
					AssertEquals(new ZDate(2014, 12, 30), convertedAPInvoiceLines.First().AL_TaxDate);
					AssertEquals(new ZDate(2014, 12, 30), convertedAPInvoiceLines.Last().AL_TaxDate);
				}
				else
				{
					Fail("Invalid tax date defaulting optoin");
				}

				convertedAPInvoice.RunPreSaveValidation();
				AssertNoErrors(convertedAPInvoice);
			}
		}

		public void TestSisterCompanyConsolInvoiceImportRespectsJobBillingExchangeRateConfigurationForForwardingConsol_ARAPInvoicePostingExchangeRateOptionIsDEF_UseJobExchangeRateDefaultIsTrue_GatewayToForwarding()
		{
			AssertSisterCompanyConsolInvoiceImportRespectsJobBillingExchangeRateConfiguration_GatewayToForwarding(true, true);
		}

		public void TestSisterCompanyConsolInvoiceImportRespectsJobBillingExchangeRateConfigurationForForwardingConsol_ARAPInvoicePostingExchangeRateOptionIsDEF_UseJobExchangeRateDefaultIsFalse_GatewayToForwarding()
		{
			AssertSisterCompanyConsolInvoiceImportRespectsJobBillingExchangeRateConfiguration_GatewayToForwarding(true, false);
		}

		public void TestSisterCompanyConsolInvoiceImportRespectsJobBillingExchangeRateConfigurationForForwardingConsol_ARAPInvoicePostingExchangeRateOptionIsNotDEF_UseJobExchangeRateDefaultIsTrue_GatewayToForwarding()
		{
			AssertSisterCompanyConsolInvoiceImportRespectsJobBillingExchangeRateConfiguration_GatewayToForwarding(false, true);
		}

		public void TestSisterCompanyConsolInvoiceImportRespectsJobBillingExchangeRateConfigurationForForwardingConsol_ARAPInvoicePostingExchangeRateOptionIsNotDEF_UseJobExchangeRateDefaultIsFalse_GatewayToForwarding()
		{
			AssertSisterCompanyConsolInvoiceImportRespectsJobBillingExchangeRateConfiguration_GatewayToForwarding(false, false);
		}

		void AssertSisterCompanyConsolInvoiceImportRespectsJobBillingExchangeRateConfiguration_GatewayToForwarding(bool isRegistryDEF, bool shouldUseJobExRateFlag)
		{
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate(LedgerTypes.AccountsPayable, JobInvoicingConsumerTypes.ForwardingConsol.Code, TransportModes.All, FreightShipmentDirection.Code.All, ExchangeRateTypes.Code.CustomsRate, JobBillingExchangeRatePreference.Code.TodaysRate, 0, true);
			GlbCompany.CurrentCompany.Factory.Save();

			var invoiceDate = ZDateTime.Today.AddDays(-6);
			var creator = new TestObjectCreator(Factory);
			creator.CreateExchangeRate(creator.USD, ExchangeRateTypes.Code.BuyRate, 0.56M, invoiceDate, invoiceDate);
			creator.CreateExchangeRate(creator.USD, ExchangeRateTypes.Code.BuyRate, 0.78M, ZDateTime.Today, ZDateTime.Today.AddDays(30));
			creator.CreateExchangeRate(creator.USD, ExchangeRateTypes.Code.CustomsRate, 3.22M, invoiceDate, invoiceDate);
			creator.CreateExchangeRate(creator.USD, ExchangeRateTypes.Code.CustomsRate, 4.39M, ZDateTime.Today, ZDateTime.Today.AddDays(30));
			differentCompany.OrgProxy.CompanyData.OB_IsCreditor = true;
			Factory.Save();

			var gatewayCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("GTWORG", true, false, "AUMEL");
			var gatewayCompany = TestObjectCreator.CreateNewCompany("CGA", orgProxy: gatewayCompanyOrgProxy);
			var gatewayBranch = TestObjectCreator.CreateNewBranch(gatewayCompany, "BGA");
			periodManagementTestHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year, GlbCompany.CurrentCompany.PK);
			periodManagementTestHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year, gatewayCompany.PK);

			TestObjectCreator.GST1.SetRate_ForTestOnly(10, 1, new ZDate(2014, 12, 1), new ZDate(2015, 2, 1));
			var chargeCode1 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1 for gateway company", ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1, gatewayCompany);
			TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1 for forwarding company", ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol("USLAX", "AUMEL", "C001", receivingGatewayCompany: gatewayCompany);
			TestObjectCreator.CreateShipment("S001", gatewayConsol);
			Factory.Save();

			ARInvoice arInvoice = null;
			var forwardingBranchOrgProxy = Factory.Load<OrgHeader>(GlbBranch.CurrentBranch.OrgProxy.PK);
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, gatewayBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				forwardingBranchOrgProxy.CompanyData.OB_IsDebtor = true;
				Factory.Save();

				var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol);
				arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), creator.USD, 1.0m, forwardingBranchOrgProxy, invoiceDate);
				arInvoice.AH_ConsolidatedInvoiceRef = gatewayConsol.JK_UniqueConsignRef;
				arInvoice.AH_JH = gatewayJob.PK;

				var arInvoiceLine1 = TestObjectCreator.CreateInvoiceLine(arInvoice, gatewayJob, chargeCode1, 100m, TestObjectCreator.USD, 1.0m);
				TestObjectCreator.CreateCharge(arInvoiceLine1);
				Factory.Save();
			}

			using (PostingExRateRegistryAP.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isRegistryDEF ? AccountingConstants.InvoicePostingExchangeRateOption.Default.Code : AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code))
			using (AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, shouldUseJobExRateFlag))
			{
				var converter = new UnapprovedTransactionConverter(Factory);
				var convertedAPInvoice = converter.ConvertToAP(arInvoice, true);

				AssertEquals(1, convertedAPInvoice.ConsolCosting.ConsolCosts.Count);
				AssertEquals(1, convertedAPInvoice.Lines.Count);
				var invoiceLine = convertedAPInvoice.Lines.Cast<InvoicingLineBase>().First();
				var expectedExRate = isRegistryDEF ? (shouldUseJobExRateFlag ? (ZDecimal)4.39m : convertedAPInvoice.AH_ExchangeRate) : (shouldUseJobExRateFlag ? (ZDecimal)3.22m : convertedAPInvoice.AH_ExchangeRate);
				BusinessObjectBaseTestCase.AssertZDecimalEquals("Expected line exchange rate.", expectedExRate, invoiceLine.AL_ExchangeRate, 0.01m);
			}
		}
	}
}
