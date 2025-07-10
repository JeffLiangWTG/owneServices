using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.Accounting.Business.Testing
{
	public class BaseChargeTest : TestCaseWithFactory
	{
		#region Chargeable

		public void TestChargeable_GivenIJobInvoicingSupporter_ThenCalculateChargeableShouldUseIsDomesticAndTransportModeToGetConversionFactor()
		{
			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Transport Booking Chargeable Factor Registry (3000 CC/KG = 333.3333 KG/M3)", "3000 CC/KG", TransportBookingChargeableFactor.MetricFactor.ToString());
				AssertEquals("SEA International Chargeable Factor Registry", "1000 KG/M3", FreightDataRegistry.Instance.InternationalChargeableFactorSea.Value.MetricFactor.ToString());
			});

			var supporterMock = new Mock<IJobInvoicingSupporter>();
			supporterMock.Setup(m => m.IsImport).Returns(false);
			supporterMock.Setup(m => m.IsExport).Returns(false);
			supporterMock.Setup(m => m.IsDirectShipment).Returns(false);
			supporterMock.Setup(m => m.IsDomestic).Returns(false);
			supporterMock.Setup(m => m.EditSecurityLock).Returns(false);
			supporterMock.Setup(m => m.OverriddenDepartmentPK).Returns(ZGuid.Empty);
			supporterMock.Setup(m => m.OperationalJobRef).Returns(ZString.Empty);
			supporterMock.Setup(m => m.ActualChargeable).Returns(ZDecimal.Zero);
			supporterMock.Setup(m => m.ActualChargeableUnit).Returns(ZString.Empty);
			supporterMock.Setup(m => m.ActualVolume).Returns(new ZDecimal(10m));
			supporterMock.Setup(m => m.ActualVolumeUnit).Returns(new ZString(Constants.Volume.CubicMetres));
			supporterMock.Setup(m => m.ActualWeight).Returns(new ZDecimal(20m));
			supporterMock.Setup(m => m.ActualWeightUnit).Returns(new ZString(Constants.Weight.Kilograms));
			supporterMock.Setup(m => m.ActualLoadingMeters).Returns(ZDecimal.Zero);
			supporterMock.Setup(m => m.CanCreateInvoicingJob).Returns(true);
			supporterMock.Setup(m => m.TransportMode).Returns((ZString)Constants.TransportModes.Sea);
			supporterMock.Setup(m => m.ContainerMode).Returns((ZString)Constants.ContainerModes.Loose);
			supporterMock.Setup(m => m.DefaultChargeGroup).Returns(ZString.Empty);
			supporterMock.Setup(m => m.ValidateOnInvoicingSupporter_JH_OA_LocalChargesAddr(It.IsAny<ZGuid>())).Returns(ZString.Empty);
			supporterMock.Setup(m => m.ValidateOnInvoicingSupporter_JH_OA_AgentCollectAddr(It.IsAny<ZGuid>())).Returns(ZString.Empty);

			var pluginMock = new Mock<IJobInvoicingPlugIn>();
			pluginMock.Setup(m => m.IsDeleted).Returns(false);
			pluginMock.Setup(m => m.InvoicingSupporter).Returns(supporterMock.Object);
			pluginMock.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			pluginMock.Setup(m => m.Factory).Returns(Factory);
			pluginMock.Setup(m => m.IsInDatabase).Returns(true);

			var job = new Job.Loader(pluginMock.Object).TryCreateWithoutMutexForTestOnly();
			var charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.ChargeableUnitForRevenueApportionment = "T"; // to force CalculateChargeable in BaseCharge.JR_Chargeable

			AssertEquals("Chargeable = 10M3 x 1000KG/M3 = 10,000KG = 10T", "10 T", $"{charge.JR_Chargeable} {charge.JR_ChargeableUnit}");
		}

		public void TestChargeable_GivenIJobInvoicingSupporterWithChargeableFactorSource_ThenCalculateChargeableShouldUseTheOverridenConvertionFactor()
		{
			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Transport Booking Chargeable Factor Registry (3000 CC/KG = 333.3333 KG/M3)", "3000 CC/KG", TransportBookingChargeableFactor.MetricFactor.ToString());
				AssertEquals("SEA International Chargeable Factor Registry", "1000 KG/M3", FreightDataRegistry.Instance.InternationalChargeableFactorSea.Value.MetricFactor.ToString());
			});

			var supporterMock = new Mock<IJobInvoicingSupporterWithChargeableFactorSource>();
			supporterMock.Setup(m => m.IsImport).Returns(false);
			supporterMock.Setup(m => m.IsExport).Returns(false);
			supporterMock.Setup(m => m.IsDirectShipment).Returns(false);
			supporterMock.Setup(m => m.IsDomestic).Returns(false);
			supporterMock.Setup(m => m.EditSecurityLock).Returns(false);
			supporterMock.Setup(m => m.OverriddenDepartmentPK).Returns(ZGuid.Empty);
			supporterMock.Setup(m => m.OperationalJobRef).Returns(ZString.Empty);
			supporterMock.Setup(m => m.ActualChargeable).Returns(ZDecimal.Zero);
			supporterMock.Setup(m => m.ActualChargeableUnit).Returns(ZString.Empty);
			supporterMock.Setup(m => m.ActualVolume).Returns(new ZDecimal(10m));
			supporterMock.Setup(m => m.ActualVolumeUnit).Returns(new ZString(Constants.Volume.CubicMetres));
			supporterMock.Setup(m => m.ActualWeight).Returns(new ZDecimal(20m));
			supporterMock.Setup(m => m.ActualWeightUnit).Returns(new ZString(Constants.Weight.Kilograms));
			supporterMock.Setup(m => m.ActualLoadingMeters).Returns(ZDecimal.Zero);
			supporterMock.Setup(m => m.CanCreateInvoicingJob).Returns(true);
			supporterMock.Setup(m => m.TransportMode).Returns((ZString)Constants.TransportModes.Sea);
			supporterMock.Setup(m => m.ContainerMode).Returns((ZString)Constants.ContainerModes.Loose);
			supporterMock.Setup(m => m.DefaultChargeGroup).Returns(ZString.Empty);
			supporterMock.Setup(m => m.ValidateOnInvoicingSupporter_JH_OA_LocalChargesAddr(It.IsAny<ZGuid>())).Returns(ZString.Empty);
			supporterMock.Setup(m => m.ValidateOnInvoicingSupporter_JH_OA_AgentCollectAddr(It.IsAny<ZGuid>())).Returns(ZString.Empty);
			supporterMock.Setup(m => m.ChargeableFactorSource).Returns(ChargeableFactorSource.TransportBooking);

			var pluginMock = new Mock<IJobInvoicingPlugIn>();
			pluginMock.Setup(m => m.IsDeleted).Returns(false);
			pluginMock.Setup(m => m.InvoicingSupporter).Returns(supporterMock.Object);
			pluginMock.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			pluginMock.Setup(m => m.Factory).Returns(Factory);
			pluginMock.Setup(m => m.IsInDatabase).Returns(true);

			var job = new Job.Loader(pluginMock.Object).TryCreateWithoutMutexForTestOnly();
			var charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.ChargeableUnitForRevenueApportionment = "T"; // to force CalculateChargeable in BaseCharge.JR_Chargeable

			AssertEquals("Chargeable = 10M3 x 333.3333KG/M3 = 3.3333T", "3.333 T", $"{charge.JR_Chargeable} {charge.JR_ChargeableUnit}");
		}

		ChargeableFactor TransportBookingChargeableFactor
		{
			get
			{
				var transportRegistry = ObjectFactory.Get<ITransportBookingRegistryProvider>();
				var chargeableFactorRegistryItem = (ChargeableFactorRegistryItem)transportRegistry.TransportBookingChargeableFactor;
				return chargeableFactorRegistryItem.Value;
			}
		}

		#endregion

		public void TestChargeIsNotDeletedWhenActiveARCashAdvanceRequestExistAndCashAdvanceFunctionalityIsEnabled()
		{
			AssertChargeIsNotDeletedWhenActiveCashAdvanceRequestExist(true, true);
		}
		public void TestChargeIsNotDeletedWhenActiveAPCashAdvanceRequestExistAndCashAdvanceFunctionalityIsEnabled()
		{
			AssertChargeIsNotDeletedWhenActiveCashAdvanceRequestExist(false, true);
		}

		public void TestChargeIsNotDeletedWhenActiveARCashAdvanceRequestExistAndCashAdvanceFunctionalityIsDisabled()
		{
			AssertChargeIsNotDeletedWhenActiveCashAdvanceRequestExist(true, false);
		}
		public void TestChargeIsNotDeletedWhenActiveAPCashAdvanceRequestExistAndCashAdvanceFunctionalityIsDisabled()
		{
			AssertChargeIsNotDeletedWhenActiveCashAdvanceRequestExist(false, false);
		}

		void AssertChargeIsNotDeletedWhenActiveCashAdvanceRequestExist(bool isARCashAdvance, bool isCashAdvanceFunctionalityEnabled)
		{
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isCashAdvanceFunctionalityEnabled))
			using (AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isCashAdvanceFunctionalityEnabled))
			{
				var shipment = TestObjectCreator.CreateShipment("S00001011");
				var job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
				var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "", TestObjectCreator.AUD, 100m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100m, TestObjectCreator.ABIGAS);
				Factory.Save();

				Assert("Advance Payment Request is not created yet.", charge1.CanDelete);

				if (isARCashAdvance)
				{
					charge1.JR_IsARCashAdvance = true;
				}
				else
				{
					charge1.JR_IsAPCashAdvance = true;
				}
				Factory.Save();

				Assert("Advance Payment Request is not created yet.", charge1.CanDelete);

				var ledger = isARCashAdvance ? LedgerTypes.AccountsReceivable : LedgerTypes.AccountsPayable;
				var organization = isARCashAdvance ? TestObjectCreator.ABIGAS : TestObjectCreator.AALSHI;
				var cashAdvanceHeader = TestObjectCreator.CreateCashAdvanceRequestHeader(job.PK, organization.PK, ledger, 100m, 100m, TestObjectCreator.AUD.RX_Code, CashAdvanceStatusCodes.RequestHeader.Requested);
				var cashAdvanceLine1 = TestObjectCreator.CreateCashAdvanceRequestLine(cashAdvanceHeader.PK, 100m, 100m, CashAdvanceStatusCodes.RequestLine.Requested);
				if (isARCashAdvance)
				{
					cashAdvanceHeader.CAH_Ledger = LedgerTypes.AccountsReceivable;
					charge1.JR_CAL_ARLine = cashAdvanceLine1.PK;
				}
				else
				{
					cashAdvanceHeader.CAH_Ledger = LedgerTypes.AccountsPayable;
					charge1.JR_CAL_APLine = cashAdvanceLine1.PK;
				}
				Factory.Save();

				var expectedReasonForNotAbleToDelete = $"Unable to delete the {TestObjectCreator.CC1.AC_Code} charge as it has an active " + (isARCashAdvance ? "AR" : "AP") + " Advance Payment. Please cancel the Advance Payment if you need to delete this charge.";
				if (isCashAdvanceFunctionalityEnabled)
				{
					Assert("cashAdvanceLine1 is in REQ status.", !charge1.CanDelete);
					AssertEquals(expectedReasonForNotAbleToDelete, charge1.ReasonForNotAbleToDelete);
				}
				else
				{
					Assert(charge1.CanDelete);
				}

				cashAdvanceLine1.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
				cashAdvanceLine1.CAL_LocalPaidAmount = cashAdvanceLine1.CAL_OSPaidAmount = 100m;
				cashAdvanceHeader.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Paid;
				cashAdvanceHeader.CAH_LocalPaidAmount = cashAdvanceHeader.CAH_OSPaidAmount = 100m;
				Factory.Save();

				if (isCashAdvanceFunctionalityEnabled)
				{
					Assert("cashAdvanceLine1 is in PAI status.", !charge1.CanDelete);
					AssertEquals(expectedReasonForNotAbleToDelete, charge1.ReasonForNotAbleToDelete);
				}
				else
				{
					Assert(charge1.CanDelete);
				}

				cashAdvanceLine1.CAL_Status = CashAdvanceStatusCodes.RequestLine.Invoiced;
				cashAdvanceHeader.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Invoiced;
				Factory.Save();

				if (isCashAdvanceFunctionalityEnabled)
				{
					Assert("cashAdvanceLine1 is in INV status.", !charge1.CanDelete);
					AssertEquals(expectedReasonForNotAbleToDelete, charge1.ReasonForNotAbleToDelete);
				}
				else
				{
					Assert(charge1.CanDelete);
				}

				cashAdvanceLine1.CAL_Status = CashAdvanceStatusCodes.RequestLine.Cancelled;
				cashAdvanceLine1.CAL_LocalPaidAmount = cashAdvanceLine1.CAL_OSPaidAmount = 0m;
				cashAdvanceHeader.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Cancelled;
				cashAdvanceHeader.CAH_LocalPaidAmount = cashAdvanceHeader.CAH_OSPaidAmount = 0m;
				Factory.Save();

				Assert("cashAdvanceLine1 is in CAN status.", charge1.CanDelete);
			}
		}

		public void TestJR_CostSupplyType_ReadOnly()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX");
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			Factory.Save();
			var charge1 = shipmentJob.Charges.AddNew();
			charge1.JR_E6 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1).PK;
			var charge2 = Factory.NewWithValidTestData<BaseCharge>();

			AssertEquals("Precondition", true, charge1.IsAllCostFieldsReadonly_ForTestOnly);
			AssertEquals(true, charge1.JR_CostSupplyTypeInfo.ReadOnly);

			AssertEquals("Precondition", false, charge2.IsAllCostFieldsReadonly_ForTestOnly);
			AssertEquals(false, charge2.JR_CostSupplyTypeInfo.ReadOnly);
		}

		public void TestJR_SellSupplyType_ReadOnly()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX");
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			Factory.Save();
			var charge1 = shipmentJob.Charges.AddNew();
			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			charge1.JR_AL_ARLine = line.PK;
			var charge2 = Factory.NewWithValidTestData<BaseCharge>();

			AssertEquals("Precondition", true, charge1.IsMainAllFieldsReadonly_ForTestOnly);
			AssertEquals(true, charge1.JR_SellSupplyTypeInfo.ReadOnly);

			AssertEquals("Precondition", false, charge2.IsMainAllFieldsReadonly_ForTestOnly);
			AssertEquals(false, charge2.JR_SellSupplyTypeInfo.ReadOnly);
		}

		#region Charge VAT Info Read-Only

		public void TestChargeCostVATInfo_ReadOnly()
		{
			TestObjectCreator.PrepareAutoJRJTestEnvironment();
			TestObjectCreator.AutoJRJCreditorOrDebtor.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = TestObjectCreator.CreateShipment("S00000001");
			var job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			Func<Charge> createCharge = () => TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 190m, TestObjectCreator.AutoJRJCreditorOrDebtor, TestObjectCreator.AUD, 0m, null);

			Action<Charge, bool> assertAction = (charge, expactedValue) =>
			{
				AssertEquals(expactedValue, charge.JR_CostTaxDateInfo.ReadOnly);
				AssertEquals(expactedValue, charge.JR_AT_CostGSTRateInfo.ReadOnly);
			};

			AssertChargeTaxInfoReadOnly(createCharge, assertAction, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Tax, true, true);
			AssertChargeTaxInfoReadOnly(createCharge, assertAction, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Tax, false, false);
			AssertChargeTaxInfoReadOnly(createCharge, assertAction, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Yes, true, true);
			AssertChargeTaxInfoReadOnly(createCharge, assertAction, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Yes, false, true);
			AssertChargeTaxInfoReadOnly(createCharge, assertAction, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Non, true, false);
			AssertChargeTaxInfoReadOnly(createCharge, assertAction, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Non, false, false);
		}

		public void TestChargeSellVATInfo_ReadOnly()
		{
			TestObjectCreator.PrepareAutoJRJTestEnvironment();
			TestObjectCreator.AutoJRJCreditorOrDebtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = TestObjectCreator.CreateShipment("S00000001");
			var job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			Func<Charge> createCharge = () => TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 0m, null, TestObjectCreator.AUD, 190m, TestObjectCreator.AutoJRJCreditorOrDebtor);

			Action<Charge, bool> assertAction = (charge, expactedValue) =>
			{
				AssertEquals(expactedValue, charge.JR_SellTaxDateInfo.ReadOnly);
				AssertEquals(expactedValue, charge.JR_AT_SellGSTRateInfo.ReadOnly);
			};

			AssertChargeTaxInfoReadOnly(createCharge, assertAction, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Tax, true, true);
			AssertChargeTaxInfoReadOnly(createCharge, assertAction, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Tax, false, false);
			AssertChargeTaxInfoReadOnly(createCharge, assertAction, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Yes, true, true);
			AssertChargeTaxInfoReadOnly(createCharge, assertAction, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Yes, false, true);
			AssertChargeTaxInfoReadOnly(createCharge, assertAction, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Non, true, false);
			AssertChargeTaxInfoReadOnly(createCharge, assertAction, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Non, false, false);
		}

		void AssertChargeTaxInfoReadOnly(Func<Charge> createCharge, Action<Charge, bool> assertAction, string autoJRJStatus, bool isTaxRegNumTheSame, bool expactedValue)
		{
			AccountingConfigurationRegistry.Instance.EnableAutoJobRevenueJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, autoJRJStatus);

			if (isTaxRegNumTheSame)
			{
				TestObjectCreator.AutoJRJCreditorOrDebtor.PrimaryRegistrationNumber.Number = "73004700400";
				TestObjectCreator.AutoJRJChargeBranchOrgProxy.PrimaryRegistrationNumber.Number = "73004700400";
			}
			else
			{
				TestObjectCreator.AutoJRJCreditorOrDebtor.PrimaryRegistrationNumber.Number = "73004700411";
				TestObjectCreator.AutoJRJChargeBranchOrgProxy.PrimaryRegistrationNumber.Number = "73004700499";
			}
			Factory.Save();

			var charge = createCharge();
			charge.JR_APInvoiceNum = "INV000001";
			charge.JR_APInvoiceDate = ZDateTime.Today;

			assertAction(charge, expactedValue);

			charge.Delete();
			Factory.Save();
		}

		#endregion

		public void TestUpdateJR_CostSupplyType_UpdateCostGSTDetails()
		{
			var charge = Factory.NewWithValidTestData<BaseCharge>();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			Factory.Save();

			TestObjectCreator.Creditor1.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			var accountingTestObjectCreator = new MasterFiles.Business.Testing.Accounting.Helpers.AccountingTestObjectCreator(Factory);
			var taxOverride = testObjectCreator.CC1.TaxOverrides.AddNew();
			accountingTestObjectCreator.PopulateTaxOverride(taxOverride, testObjectCreator.GST2.PK);
			Factory.Save();

			AssertNullOrEmpty("Precondition", charge.JR_CostSupplyType);
			AssertEquals("Precondition", testObjectCreator.GST1.PK, charge.JR_AT_CostGSTRate);
			AssertEquals("Precondition", testObjectCreator.GST1.AT_A9_DefaultVatClass, charge.JR_A9_CostVATClass);
			charge.JR_CostSupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC;
			AssertEquals(testObjectCreator.GST2.PK, charge.JR_AT_CostGSTRate);
			AssertEquals(testObjectCreator.GST2.AT_A9_DefaultVatClass, charge.JR_A9_CostVATClass);
		}

		public void TestUpdateJR_SellSupplyType_ResetSellGSTTaxDafault()
		{
			var charge = Factory.NewWithValidTestData<BaseCharge>();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OH_SellAccount = TestObjectCreator.Debtor1.PK;
			Factory.Save();

			TestObjectCreator.Debtor1.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			var accountingTestObjectCreator = new MasterFiles.Business.Testing.Accounting.Helpers.AccountingTestObjectCreator(Factory);
			var taxOverride = testObjectCreator.CC1.TaxOverrides.AddNew();
			accountingTestObjectCreator.PopulateTaxOverride(taxOverride, testObjectCreator.GST2.PK);
			Factory.Save();

			AssertNullOrEmpty("Precondition", charge.JR_SellSupplyType);
			AssertEquals("Precondition", testObjectCreator.GST1.PK, charge.JR_AT_SellGSTRate);
			AssertEquals("Precondition", testObjectCreator.GST1.AT_A9_DefaultVatClass, charge.JR_A9_SellVATClass);
			charge.JR_SellSupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC;
			AssertEquals(testObjectCreator.GST2.PK, charge.JR_AT_SellGSTRate);
			AssertEquals(testObjectCreator.GST2.AT_A9_DefaultVatClass, charge.JR_A9_SellVATClass);
		}

		public void TestPaymentTypeTakesCSHValueWhenCashAccountIsSelected()
		{
			var cashAccount = Factory.NewWithValidTestData<AccBankAccount>();
			cashAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;

			var charge = Factory.NewWithValidTestData<BaseCharge>();
			AssertNotEquals(ReceiptTypes.Cash, charge.JR_PaymentType);

			charge.JR_AB = cashAccount.PK;
			AssertEquals(ReceiptTypes.Cash, charge.JR_PaymentType);
		}

		public void TestUpdateJR_GB_ResetSellGSTTaxDafault()
		{
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			TestObjectCreator.CC1.TaxOverrides.RemoveAndDeleteAll();
			TestObjectCreator.CC1.AC_AT_GSTRate = ZGuid.Empty;
			TestObjectCreator.CreateTaxOverrides(TestObjectCreator.CC1
				, taxOverride =>
				{
					taxOverride.AO_AT = TestObjectCreator.FREEVAT.PK;
					taxOverride.AO_GB = GlbBranch.CurrentBranch.PK;
				}
				, taxOverride =>
				{
					taxOverride.AO_AT = TestObjectCreator.VATSPV.PK;
					taxOverride.AO_SupplyType = SupplyTypeClassificationCodes.LOC;
				});
			Factory.Save();

			var charge = Factory.NewWithValidTestData<BaseCharge>();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_AT_CostGSTRate = Guid.Empty;
			charge.JR_AT_SellGSTRate = Guid.Empty;
			charge.JR_CostSupplyType = SupplyTypeClassificationCodes.LOC;
			charge.JR_SellSupplyType = SupplyTypeClassificationCodes.LOC;
			charge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			charge.JR_OH_SellAccount = TestObjectCreator.Debtor1.PK;
			charge.JR_GB_CostTaxBranch = ZGuid.Empty;
			charge.JR_GB_SellTaxBranch = ZGuid.Empty;

			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			var costPlaceOfSupply = charge.JR_CostPlaceOfSupply;
			var sellPlaceOfSupply = charge.JR_SellPlaceOfSupply;

			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			charge.JR_GB = ZGuid.Empty;
			TestObjectCreator.CC1.ClearGSTRateCacheForTesting();
			CombineAssertions("When EnableBranchLevelTaxOverrideRuleConfigurations is not enabled,JR_GB should not be used to filter.", () =>
			{
				charge.JR_GB = GlbBranch.CurrentBranch.PK;
				AssertEquals(TestObjectCreator.VATSPV.PK, charge.JR_AT_CostGSTRate);
				AssertEquals(TestObjectCreator.VATSPV.PK, charge.JR_AT_SellGSTRate);
			});

			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			charge.JR_GB = ZGuid.Empty;
			TestObjectCreator.CC1.ClearGSTRateCacheForTesting();
			CombineAssertions("When EnableBranchLevelTaxOverrideRuleConfigurations is enabled,JR_GB should be used to filter.", () =>
			{
				charge.JR_GB = GlbBranch.CurrentBranch.PK;
				AssertEquals(TestObjectCreator.FREEVAT.PK, charge.JR_AT_CostGSTRate);
				AssertEquals(TestObjectCreator.FREEVAT.PK, charge.JR_AT_SellGSTRate);
			});

			charge.JR_GB = ZGuid.Empty;
			charge.JR_CostPlaceOfSupply = costPlaceOfSupply;
			charge.JR_SellPlaceOfSupply = sellPlaceOfSupply;
			charge.JR_GB_CostTaxBranch = ZGuid.Empty;
			charge.JR_GB_SellTaxBranch = ZGuid.Empty;
			charge.JR_AT_CostGSTRate = Guid.Empty;
			charge.JR_AT_SellGSTRate = Guid.Empty;
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			CombineAssertions("When EnableTaxBranchReporting is enabled,JR_GB should not trigger GST resetting.", () =>
			{
				charge.JR_GB = GlbBranch.CurrentBranch.PK;
				AssertEquals(ZGuid.Empty, charge.JR_AT_CostGSTRate);
				AssertEquals(ZGuid.Empty, charge.JR_AT_SellGSTRate);
			});
		}

		public void TestUpdateJR_GB_CostTaxBranch_ResetSellGSTTaxDafault()
		{
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var branchForCost = TestObjectCreator.CreateBranch("CST", GlbCompany.CurrentCompany);

			TestObjectCreator.CC1.AC_AT_GSTRate = ZGuid.Empty;
			TestObjectCreator.CC1.TaxOverrides.RemoveAndDeleteAll();
			TestObjectCreator.CreateTaxOverrides(TestObjectCreator.CC1
				, taxOverride =>
				{
					taxOverride.AO_AT = TestObjectCreator.SVAT1.PK;
					taxOverride.AO_GB = branchForCost.PK;
				}
				, taxOverride =>
				{
					taxOverride.AO_AT = TestObjectCreator.VATSPV.PK;
					taxOverride.AO_SupplyType = SupplyTypeClassificationCodes.LOC;
				});
			Factory.Save();

			var charge = Factory.NewWithValidTestData<BaseCharge>();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_GB = ZGuid.Empty;
			charge.JR_GB_CostTaxBranch = ZGuid.Empty;
			charge.JR_CostSupplyType = SupplyTypeClassificationCodes.LOC;
			charge.JR_SellSupplyType = SupplyTypeClassificationCodes.LOC;
			charge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			charge.JR_OH_SellAccount = TestObjectCreator.Debtor1.PK;
			charge.JR_AT_CostGSTRate = Guid.Empty;
			charge.JR_AT_SellGSTRate = Guid.Empty;

			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			charge.JR_GB_CostTaxBranch = ZGuid.Empty;
			charge.JR_AT_CostGSTRate = Guid.Empty;
			charge.JR_AT_SellGSTRate = Guid.Empty;
			TestObjectCreator.CC1.ClearGSTRateCacheForTesting();
			CombineAssertions("When EnableBranchLevelTaxOverrideRuleConfigurations and EnableTaxBranchReporting are not enabled, JR_GB_CostTaxBranch should not trigger GST resetting.", () =>
			{
				charge.JR_GB_CostTaxBranch = branchForCost.PK;
				AssertEquals(ZGuid.Empty, charge.JR_AT_CostGSTRate);
				AssertEquals(ZGuid.Empty, charge.JR_AT_SellGSTRate);
			});

			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			charge.JR_GB_CostTaxBranch = ZGuid.Empty;
			charge.JR_AT_CostGSTRate = Guid.Empty;
			charge.JR_AT_SellGSTRate = Guid.Empty;
			TestObjectCreator.CC1.ClearGSTRateCacheForTesting();
			CombineAssertions("When EnableBranchLevelTaxOverrideRuleConfigurations is not enabled,JR_GB_CostTaxBranch should not trigger GST resetting.", () =>
			{
				charge.JR_GB_CostTaxBranch = branchForCost.PK;
				AssertEquals(ZGuid.Empty, charge.JR_AT_CostGSTRate);
				AssertEquals(ZGuid.Empty, charge.JR_AT_SellGSTRate);
			});

			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			charge.JR_GB_CostTaxBranch = ZGuid.Empty;
			charge.JR_AT_CostGSTRate = Guid.Empty;
			charge.JR_AT_SellGSTRate = Guid.Empty;
			CombineAssertions("When EnableTaxBranchReporting is not enabled,JR_GB_CostTaxBranch should not trigger GST resetting.", () =>
			{
				charge.JR_GB_CostTaxBranch = branchForCost.PK;
				AssertEquals(ZGuid.Empty, charge.JR_AT_CostGSTRate);
				AssertEquals(ZGuid.Empty, charge.JR_AT_SellGSTRate);
			});

			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GB_CostTaxBranch = ZGuid.Empty;
			charge.JR_AT_CostGSTRate = Guid.Empty;
			charge.JR_AT_SellGSTRate = Guid.Empty;
			TestObjectCreator.CC1.ClearGSTRateCacheForTesting();
			CombineAssertions(@"When EnableBranchLevelTaxOverrideRuleConfigurations & EnableTaxBranchReporting are enabled, JR_GB_CostTaxBranch should trigger only Cost GST resetting.", () =>
			{
				charge.JR_GB_CostTaxBranch = branchForCost.PK;
				AssertEquals(TestObjectCreator.SVAT1.PK, charge.JR_AT_CostGSTRate);
				AssertEquals(ZGuid.Empty, charge.JR_AT_SellGSTRate);
			});
		}

		public void TestUpdateJR_GB_SellTaxBranch_ResetSellGSTTaxDafault()
		{
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var branchForSell = TestObjectCreator.CreateBranch("SEL", GlbCompany.CurrentCompany);

			TestObjectCreator.CC1.AC_AT_GSTRate = ZGuid.Empty;
			TestObjectCreator.CC1.TaxOverrides.RemoveAndDeleteAll();
			TestObjectCreator.CreateTaxOverrides(TestObjectCreator.CC1
				, taxOverride =>
				{
					taxOverride.AO_AT = TestObjectCreator.SVAT2.PK;
					taxOverride.AO_GB = branchForSell.PK;
				}
				, taxOverride =>
				{
					taxOverride.AO_AT = TestObjectCreator.VATSPV.PK;
					taxOverride.AO_SupplyType = SupplyTypeClassificationCodes.LOC;
				});
			Factory.Save();

			var charge = Factory.NewWithValidTestData<BaseCharge>();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_GB = ZGuid.Empty;
			charge.JR_GB_SellTaxBranch = ZGuid.Empty;
			charge.JR_CostSupplyType = SupplyTypeClassificationCodes.LOC;
			charge.JR_SellSupplyType = SupplyTypeClassificationCodes.LOC;
			charge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			charge.JR_OH_SellAccount = TestObjectCreator.Debtor1.PK;
			charge.JR_AT_CostGSTRate = Guid.Empty;
			charge.JR_AT_SellGSTRate = Guid.Empty;

			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			charge.JR_GB_SellTaxBranch = ZGuid.Empty;
			charge.JR_AT_CostGSTRate = Guid.Empty;
			charge.JR_AT_SellGSTRate = Guid.Empty;
			TestObjectCreator.CC1.ClearGSTRateCacheForTesting();
			CombineAssertions("When EnableBranchLevelTaxOverrideRuleConfigurations and EnableTaxBranchReporting are not enabled,JR_GB_SellTaxBranch should not trigger GST resetting.", () =>
			{
				charge.JR_GB_SellTaxBranch = branchForSell.PK;
				AssertEquals(ZGuid.Empty, charge.JR_AT_CostGSTRate);
				AssertEquals(ZGuid.Empty, charge.JR_AT_SellGSTRate);
			});

			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			charge.JR_GB_SellTaxBranch = ZGuid.Empty;
			charge.JR_AT_CostGSTRate = Guid.Empty;
			charge.JR_AT_SellGSTRate = Guid.Empty;
			TestObjectCreator.CC1.ClearGSTRateCacheForTesting();
			CombineAssertions("When EnableBranchLevelTaxOverrideRuleConfigurations is not enabled,JR_GB_SellTaxBranch should not trigger GST resetting.", () =>
			{
				charge.JR_GB_SellTaxBranch = branchForSell.PK;
				AssertEquals(ZGuid.Empty, charge.JR_AT_CostGSTRate);
				AssertEquals(ZGuid.Empty, charge.JR_AT_SellGSTRate);
			});

			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			charge.JR_GB_SellTaxBranch = ZGuid.Empty;
			charge.JR_AT_CostGSTRate = Guid.Empty;
			charge.JR_AT_SellGSTRate = Guid.Empty;
			CombineAssertions("When EnableTaxBranchReporting is not enabled,JR_GB_SellTaxBranch should not trigger GST resetting.", () =>
			{
				charge.JR_GB_SellTaxBranch = branchForSell.PK;
				AssertEquals(ZGuid.Empty, charge.JR_AT_CostGSTRate);
				AssertEquals(ZGuid.Empty, charge.JR_AT_SellGSTRate);
			});

			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GB_SellTaxBranch = ZGuid.Empty;
			charge.JR_AT_CostGSTRate = Guid.Empty;
			charge.JR_AT_SellGSTRate = Guid.Empty;
			TestObjectCreator.CC1.ClearGSTRateCacheForTesting();
			CombineAssertions(@"When EnableBranchLevelTaxOverrideRuleConfigurations & EnableTaxBranchReporting are enabled, JR_GB_SellTaxBranch should trigger only Sell GST resetting.", () =>
			{
				charge.JR_GB_SellTaxBranch = branchForSell.PK;
				AssertEquals(TestObjectCreator.SVAT2.PK, charge.JR_AT_SellGSTRate);
				AssertEquals(ZGuid.Empty, charge.JR_AT_CostGSTRate);
			});
		}

		public void TestSetTaxBranchDefault_WhenJR_OH_SellAccountChanged()
		{
			TestObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			TestObjectCreator.Debtor1.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S001", null);
			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			shipmentJob.JH_GB_TaxBranch = TestObjectCreator.NonCurrentBranch.PK;
			var charge = shipmentJob.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;

			AssertSetTaxBranchDefault_WhenJR_OH_SellAccountChanged(true, true);
			AssertSetTaxBranchDefault_WhenJR_OH_SellAccountChanged(true, false);
			AssertSetTaxBranchDefault_WhenJR_OH_SellAccountChanged(false, true);
			AssertSetTaxBranchDefault_WhenJR_OH_SellAccountChanged(false, false);

			void AssertSetTaxBranchDefault_WhenJR_OH_SellAccountChanged(bool enableTaxBranchReporting, bool gstRegistered)
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = gstRegistered;
				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableTaxBranchReporting))
				{
					charge.JR_OH_SellAccount = ZGuid.Empty;
					AssertEquals("Precondition", ZGuid.Empty, charge.JR_GB_SellTaxBranch);

					charge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
					AssertEquals(enableTaxBranchReporting && gstRegistered ? TestObjectCreator.NonCurrentBranch.PK : ZGuid.Empty, charge.JR_GB_SellTaxBranch);

					charge.JR_OH_SellAccount = TestObjectCreator.Debtor1.PK;
					AssertEquals(ZGuid.Empty, charge.JR_GB_SellTaxBranch);
				}
			}
		}

		public void TestSetSellTaxBranchDefaultWhenReadOnly()
		{
			TestObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			TestObjectCreator.Debtor1.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S001", null);
			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			shipmentJob.JH_GB_TaxBranch = TestObjectCreator.NonCurrentBranch.PK;
			var charge = shipmentJob.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				testObjectCreator.ResetSecurityCore();
				Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideSellTaxBranch).IsAllowed = false;
				Assert("Precondition", charge.JR_GB_SellTaxBranch_ReadOnly_ForTestOnly);

				charge.JR_OH_SellAccount = ZGuid.Empty;
				AssertEquals("Precondition", ZGuid.Empty, charge.JR_GB_SellTaxBranch);

				charge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
				AssertEquals(TestObjectCreator.NonCurrentBranch.PK, charge.JR_GB_SellTaxBranch);

				charge.JR_OH_SellAccount = TestObjectCreator.Debtor1.PK;
				AssertEquals(ZGuid.Empty, charge.JR_GB_SellTaxBranch);
			}
		}

		public void TestSetTaxBranchDefault_WhenJR_OH_CostAccountChanged()
		{
			TestObjectCreator.Creditor1.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			TestObjectCreator.Creditor2.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;
			Factory.Save();

			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX");
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			shipmentJob.JH_GB_TaxBranch = TestObjectCreator.NonCurrentBranch.PK;
			var charge = shipmentJob.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;

			AssertSetTaxBranchDefault_WhenJR_OH_CostAccountChanged(true, true);
			AssertSetTaxBranchDefault_WhenJR_OH_CostAccountChanged(true, false);
			AssertSetTaxBranchDefault_WhenJR_OH_CostAccountChanged(false, true);
			AssertSetTaxBranchDefault_WhenJR_OH_CostAccountChanged(false, false);

			void AssertSetTaxBranchDefault_WhenJR_OH_CostAccountChanged(bool enableTaxBranchReporting, bool gstRegistered)
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableTaxBranchReporting))
				{
					charge.JR_OH_CostAccount = ZGuid.Empty;
					AssertEquals("Precondition", ZGuid.Empty, charge.JR_OH_CostAccount);

					charge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
					AssertEquals(enableTaxBranchReporting && gstRegistered ? TestObjectCreator.NonCurrentBranch.PK : ZGuid.Empty, charge.JR_GB_CostTaxBranch);

					charge.JR_OH_CostAccount = TestObjectCreator.Creditor2.PK;
					AssertEquals(ZGuid.Empty, charge.JR_GB_CostTaxBranch);

					var consolCost = testObjectCreator.CreateConsolCost(consol, testObjectCreator.CC1, 100, testObjectCreator.Creditor1);
					charge.JR_E6 = consolCost.PK;
					AssertEquals("Precondition", true, charge.JR_IsApportioned);
					charge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
					AssertEquals(ZGuid.Empty, charge.JR_GB_CostTaxBranch);
				}
			}
		}

		public void TestSetCostTaxBranchDefaultWhenReadOnly()
		{
			TestObjectCreator.Creditor1.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			TestObjectCreator.Creditor2.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;
			Factory.Save();

			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX");
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			shipmentJob.JH_GB_TaxBranch = TestObjectCreator.NonCurrentBranch.PK;
			var charge = shipmentJob.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				testObjectCreator.ResetSecurityCore();
				Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideCostTaxBranch).IsAllowed = false;
				Assert("Precondition", charge.JR_GB_CostTaxBranch_ReadOnly_ForTestOnly);

				charge.JR_OH_CostAccount = ZGuid.Empty;
				AssertEquals("Precondition", ZGuid.Empty, charge.JR_OH_CostAccount);

				charge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
				AssertEquals(TestObjectCreator.NonCurrentBranch.PK, charge.JR_GB_CostTaxBranch);

				charge.JR_OH_CostAccount = TestObjectCreator.Creditor2.PK;
				AssertEquals(ZGuid.Empty, charge.JR_GB_CostTaxBranch);
			}
		}

		public void TestDBHitsForAllowedToLogin()
		{
			var factory1 = new BusinessObjectFactory();
			var staff = factory1.NewWithValidTestData<GlbStaff>();
			var charge1 = factory1.NewWithValidTestData<BaseCharge>();
			factory1.Save();

			GlbStaff.CurrentUser.Factory.ResetDatabaseLoadCount();

			UserLoginPermissions(factory1, charge1);

			var factory2 = new BusinessObjectFactory();

			UserLoginPermissions(factory2, charge1);

			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			AssertDbHits(new Dictionary<string, int>
			{
			}, GlbStaff.CurrentUser.Factory);

			AssertDbHits(new Dictionary<string, int>
			{
			}, factory2);

			void UserLoginPermissions(BusinessObjectFactory factory, BaseCharge charge)
			{
				using (Env.SetTemporaryUserContext(staff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
				{
					var baseCharge = factory.Load<BaseCharge>(charge.PK);
					_ = baseCharge.Job;

					factory.ResetDatabaseLoadCount();

					_ = baseCharge.IsAllowedToModifyThisCharge;
				}
			}
		}

		public void TestInvoiceCurrencyType()
		{
			var charge = Factory.NewWithValidTestData<BaseCharge>();
			charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;

			using (AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Assert(charge.IsInLocalInvoiceCurrencyForPosting(ExchangeRateValidLedgerEnum.AR));
				AssertEquals(InvoiceCurrencyType.NotApplicable, charge.InvoiceCurrencyTypeForAR);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				charge = Factory.NewWithValidTestData<BaseCharge>();
				charge.JR_OH_SellAccount = testObjectCreator.Debtor.PK;
				charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				Assert(charge.IsInLocalInvoiceCurrencyForPosting(ExchangeRateValidLedgerEnum.AR));
				AssertEquals(InvoiceCurrencyType.Local, charge.InvoiceCurrencyTypeForAR);

				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
				Assert(!charge.IsInLocalInvoiceCurrencyForPosting(ExchangeRateValidLedgerEnum.AR));
				AssertEquals(InvoiceCurrencyType.Foreign, charge.InvoiceCurrencyTypeForAR);
			}
		}

		public void TestJR_JH_InternalJobDefaulting()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());
			var consol = TestObjectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment = TestObjectCreator.CreateShipment("S001", consol);

			var orgProxyNotGatewayAgent = TestObjectCreator.CreateOrgHeader("Org1", false, false);
			var testBranch1 = TestObjectCreator.CreateBranch("BR1", "Branch1", GlbCompany.CurrentCompany, orgProxyNotGatewayAgent);

			using (var consolJob = TestObjectCreator.CreateJob(consol))
			using (var shipmentJob = TestObjectCreator.CreateJob(shipment))
			{
				Factory.Save();

				var charge = consolJob.Charges.AddNew();

				foreach (var info in new[] { charge.JR_OH_SellAccountInfo, charge.JR_OH_CostAccountInfo })
				{
					charge.JR_Calc_RelatedJobNumber = string.Empty;
					charge.JR_JH_InternalJob = TestObjectCreator.Job1.PK;
					info.Value = orgProxyNotGatewayAgent.PK;
					AssertEquals("Defaulting Internal Job does not override current value", TestObjectCreator.Job1.PK, charge.JR_JH_InternalJob);

					charge.JR_JH_InternalJob = ZGuid.Empty;
					info.Value = ZGuid.Empty;
					info.Value = orgProxyNotGatewayAgent.PK;
					AssertEquals("Internal Job set to Job when there is no Related Job", consolJob.PK, charge.JR_JH_InternalJob);

					charge.JR_Calc_RelatedJobNumber = shipment.JobNumber;
					info.Value = ZGuid.Empty;
					charge.JR_JH_InternalJob = TestObjectCreator.Job1.PK;
					info.Value = orgProxyNotGatewayAgent.PK;
					AssertEquals("Defaulting Internal Job set to Related Job if set from Sell Account, otherwise does not override current value",
						info == charge.JR_OH_SellAccountInfo ? shipmentJob.PK : TestObjectCreator.Job1.PK,
						charge.JR_JH_InternalJob);

					charge.JR_JH_InternalJob = ZGuid.Empty;
					info.Value = ZGuid.Empty;
					info.Value = orgProxyNotGatewayAgent.PK;
					AssertEquals("Internal Job set to Related Job if set from Sell Account, otherwise use Job",
						info == charge.JR_OH_SellAccountInfo ? shipmentJob.PK : consolJob.PK,
						charge.JR_JH_InternalJob);

					info.Value = ZGuid.Empty;
				}
			}
		}

		public void TestJR_GE_InternalDeptDefaulting()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());
			var consol = TestObjectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment = TestObjectCreator.CreateShipment("S001", consol);

			var orgProxyNotGatewayAgent = TestObjectCreator.CreateOrgHeader("Org1", false, false);
			var testBranch1 = TestObjectCreator.CreateBranch("BR1", "Branch1", GlbCompany.CurrentCompany, orgProxyNotGatewayAgent);

			using (var consolJob = TestObjectCreator.CreateJob(consol))
			using (var shipmentJob = TestObjectCreator.CreateJob(shipment))
			{
				consolJob.JH_GE = TestObjectCreator.FEADepartment.PK;
				shipmentJob.JH_GE = TestObjectCreator.FESDepartment.PK;
				Factory.Save();

				var charge = consolJob.Charges.AddNew();

				foreach (var info in new[] { charge.JR_OH_SellAccountInfo, charge.JR_OH_CostAccountInfo })
				{
					charge.JR_Calc_RelatedJobNumber = string.Empty;
					charge.JR_GE_InternalDept = TestObjectCreator.GEADepartment.PK;
					info.Value = orgProxyNotGatewayAgent.PK;
					AssertEquals("Defaulting Internal Dept does not override current value", TestObjectCreator.GEADepartment.PK, charge.JR_GE_InternalDept);

					charge.JR_GE_InternalDept = ZGuid.Empty;
					info.Value = ZGuid.Empty;
					info.Value = orgProxyNotGatewayAgent.PK;
					AssertEquals("Internal Dept taken from Job when there is no Related Job", TestObjectCreator.FEADepartment.PK, charge.JR_GE_InternalDept);

					charge.JR_Calc_RelatedJobNumber = shipment.JobNumber;
					info.Value = ZGuid.Empty;
					charge.JR_GE_InternalDept = TestObjectCreator.GEADepartment.PK;
					info.Value = orgProxyNotGatewayAgent.PK;
					AssertEquals("Defaulting Internal Dept taken from Related Job if set from Sell Account, otherwise does not override current value",
						info == charge.JR_OH_SellAccountInfo ? TestObjectCreator.FESDepartment.PK : TestObjectCreator.GEADepartment.PK,
						charge.JR_GE_InternalDept);

					charge.JR_GE_InternalDept = ZGuid.Empty;
					info.Value = ZGuid.Empty;
					info.Value = orgProxyNotGatewayAgent.PK;
					AssertEquals("Internal Dept taken from Related Job if set from Sell Account, otherwise take from Job",
						info == charge.JR_OH_SellAccountInfo ? TestObjectCreator.FESDepartment.PK : TestObjectCreator.FEADepartment.PK,
						charge.JR_GE_InternalDept);

					info.Value = ZGuid.Empty;
				}
			}
		}

		public void TestIsApplyingParentGovtChargeCode()
		{
			AssertWhenRegistryOn();

			AssertWhenRegistryOff();

			void AssertWhenRegistryOn()
			{
				AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				var charge = CreateTestObj("S0001", "C00");
				bool cachedValue = false;
				charge.InvokeMethodWhenSetGovtChargeCodeWithConsolCost_ForTestOnly = () =>
				{
					cachedValue = charge.IsApplyingParentGovtChargeCode;
				};

				charge.JR_AC = TestObjectCreator.CC1.PK;

				AssertEquals("IsApplyingParentGovtChargeCode was set to true when charge is consol cost related", true, cachedValue);
				AssertEquals("After evaluation, IsApplyingParentGovtChargeCode value is reset", false, charge.IsApplyingParentGovtChargeCode);
			}

			void AssertWhenRegistryOff()
			{
				AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

				var charge = CreateTestObj("S0002", "C002");
				bool cachedValue = false;
				charge.InvokeMethodWhenSetGovtChargeCodeWithConsolCost_ForTestOnly = () =>
				{
					cachedValue = charge.IsApplyingParentGovtChargeCode;
				};

				charge.JR_AC = TestObjectCreator.CC1.PK;

				AssertEquals("IsApplyingParentGovtChargeCode will NOT be active when registry is off", false, cachedValue);
				AssertEquals(false, charge.IsApplyingParentGovtChargeCode);
			}

			BaseCharge CreateTestObj(string shipmentNum, string consignRef)
			{
				var shipment = TestObjectCreator.CreateShipment(shipmentNum, "AUSYD", "USLAX");
				var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", consignRef);
				consol.Shipments.Add(shipment);

				var shipmentJob = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
				var charge = shipmentJob.Charges.AddNew();
				charge.JR_E6 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1).PK;
				return charge;
			}
		}

		public void TestJR_GB_InternalBranchDefaulting()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());
			var consol = TestObjectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			var branch1 = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("BBB", GlbCompany.CurrentCompany);
			var branch3 = TestObjectCreator.CreateBranch("CCC", GlbCompany.CurrentCompany);

			var orgProxyNotGatewayAgent = TestObjectCreator.CreateOrgHeader("Org1", false, false);
			var testBranch1 = TestObjectCreator.CreateBranch("BR1", "Branch1", GlbCompany.CurrentCompany, orgProxyNotGatewayAgent);

			using (var consolJob = TestObjectCreator.CreateJob(consol))
			using (var shipmentJob = TestObjectCreator.CreateJob(shipment))
			{
				consolJob.JH_GB = branch1.PK;
				shipmentJob.JH_GB = branch2.PK;
				Factory.Save();

				var charge = consolJob.Charges.AddNew();

				foreach (var info in new[] { charge.JR_OH_SellAccountInfo, charge.JR_OH_CostAccountInfo })
				{
					charge.JR_Calc_RelatedJobNumber = string.Empty;
					charge.JR_GB_InternalBranch = branch3.PK;
					info.Value = orgProxyNotGatewayAgent.PK;
					AssertEquals("Defaulting Internal Job from consol when no related job", consolJob.PK, charge.JR_JH_InternalJob);
					AssertEquals("Defaulting Internal Branch taken from internal job's branch", testBranch1.PK, charge.JR_GB_InternalBranch);

					charge.JR_GB_InternalBranch = ZGuid.Empty;
					info.Value = ZGuid.Empty;
					info.Value = orgProxyNotGatewayAgent.PK;
					AssertEquals("Defaulting Internal Job from consol  when no related job", consolJob.PK, charge.JR_JH_InternalJob);
					AssertEquals("Defaulting Internal Branch taken from internal job's branch", testBranch1.PK, charge.JR_GB_InternalBranch);

					charge.JR_Calc_RelatedJobNumber = shipment.JobNumber;
					info.Value = ZGuid.Empty;
					charge.JR_GB_InternalBranch = branch3.PK;
					info.Value = orgProxyNotGatewayAgent.PK;
					AssertEquals("Defaulting Internal Job from Related Job if set from Sell Account, otherwise taken from consol",
						info == charge.JR_OH_SellAccountInfo ? shipmentJob.PK : consolJob.PK,
						charge.JR_JH_InternalJob);
					AssertEquals("Defaulting Internal Branch taken from Related Job if set from Sell Account, otherwise taken from internal job",
						info == charge.JR_OH_SellAccountInfo ? branch2.PK : testBranch1.PK,
						charge.JR_GB_InternalBranch);

					charge.JR_GB_InternalBranch = ZGuid.Empty;
					info.Value = ZGuid.Empty;
					info.Value = orgProxyNotGatewayAgent.PK;
					AssertEquals("Defaulting Internal Job from Related Job if set from Sell Account, otherwise taken from consol",
						info == charge.JR_OH_SellAccountInfo ? shipmentJob.PK : consolJob.PK,
						charge.JR_JH_InternalJob);
					AssertEquals("Defaulting Internal Branch taken from Related Job if set from Sell Account, otherwise taken from internal job",
						info == charge.JR_OH_SellAccountInfo ? branch2.PK : testBranch1.PK,
						charge.JR_GB_InternalBranch);

					info.Value = ZGuid.Empty;
				}
			}
		}

		public void TestInternalFieldsDefaulting_ViaJR_GB()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJWithTaxRegistrationNumberEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());

			const string registeredNumberGroup1 = "21 003 980 130";
			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var registeredNumberCodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			AssertEquals("PreCondition", CountryCodes.Australia, currentCountryCode);

			TestObjectCreator.SetCustomsCodeForOrgHeader(GlbBranch.CurrentBranch.OrgProxy, registeredNumberCodeType, currentCountryCode, registeredNumberGroup1);

			var orgUsedToOnlyOneBranch = TestObjectCreator.CreateOrgHeader("Org1", true, true);
			var branchWIthUniqueOrgProxy = TestObjectCreator.CreateBranch("BR1", "Branch1", GlbCompany.CurrentCompany, orgUsedToOnlyOneBranch);
			TestObjectCreator.SetCustomsCodeForOrgHeader(orgUsedToOnlyOneBranch, registeredNumberCodeType, currentCountryCode, registeredNumberGroup1);

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment);
			job.PlugInData = shipment;

			GlbCompany.CurrentCompany.Factory.Save();
			Factory.Save();

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "Charge 1");

			charge.JR_OH_SellAccount = orgUsedToOnlyOneBranch.PK;
			AssertIsInternalJobInfoDisabled_False(charge, "Defaulting value to shipment since sell account is internal org.", job, branchWIthUniqueOrgProxy.PK);

			charge.JR_GB = ZGuid.Empty;
			AssertIsInternalJobInfoDisabled_True(charge, "When charge branch is cleared, charge's internal fields should be cleared too.");

			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			AssertIsInternalJobInfoDisabled_False(charge, "Defaulting value to shipment since sell account is internal org.", job, branchWIthUniqueOrgProxy.PK);

			charge.JR_OH_SellAccount = ZGuid.Empty;
			charge.JR_OH_CostAccount = orgUsedToOnlyOneBranch.PK;
			AssertIsInternalJobInfoDisabled_False(charge, "Defaulting value to shipment since cost account is internal org.", job, branchWIthUniqueOrgProxy.PK);

			charge.JR_GB = ZGuid.Empty;
			AssertIsInternalJobInfoDisabled_True(charge, "When charge branch is cleared, charge's internal fields should be cleared too.");

			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			AssertIsInternalJobInfoDisabled_False(charge, "Defaulting value to shipment since cost account is internal org.", job, branchWIthUniqueOrgProxy.PK);
		}

		public void TestIsInternalJobInfoDisabled()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJWithTaxRegistrationNumberEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());

			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var registeredNumberCodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			AssertEquals("PreCondition", CountryCodes.Australia, currentCountryCode);

			const string registeredNumberGroup1 = "21 003 980 130";
			const string registeredNumberGroup2 = "21 003 980 130 123";

			GlbBranch.CurrentBranch.OrgProxy.CompanyData.SetAPTaxApplicable(true);
			GlbBranch.CurrentBranch.OrgProxy.CompanyData.SetARTaxApplicable(true);
			TestObjectCreator.SetCustomsCodeForOrgHeader(GlbBranch.CurrentBranch.OrgProxy, registeredNumberCodeType, currentCountryCode, registeredNumberGroup1);

			var orgWithRegisteredNumberGroup2 = TestObjectCreator.CreateOrgHeader("AA1", true, true);
			TestObjectCreator.SetCustomsCodeForOrgHeader(orgWithRegisteredNumberGroup2, registeredNumberCodeType, currentCountryCode, registeredNumberGroup2);
			var branchWithRegisteredNumberGroup2 = TestObjectCreator.CreateBranch("AA1", GlbCompany.CurrentCompany, orgWithRegisteredNumberGroup2);

			var org2WithRegisteredNumberGroup2 = TestObjectCreator.CreateOrgHeader("AA2", true, true);
			TestObjectCreator.SetCustomsCodeForOrgHeader(org2WithRegisteredNumberGroup2, registeredNumberCodeType, currentCountryCode, registeredNumberGroup2);
			var branch2WithRegisteredNumberGroup2 = TestObjectCreator.CreateBranch("AA2", GlbCompany.CurrentCompany, org2WithRegisteredNumberGroup2);

			var orgWithEmptyRegisteredNumber = TestObjectCreator.CreateOrgHeader("BB1", true, true);
			var branchWithEmptyRegisteredNumber = TestObjectCreator.CreateBranch("BB1", GlbCompany.CurrentCompany, orgWithEmptyRegisteredNumber);
			AssertNull("PreCondition", branchWithEmptyRegisteredNumber.OrgProxy.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_CodeType == registeredNumberCodeType && x.OK_RN_NKCodeCountry == currentCountryCode));

			var branchWithEmptyOrgProxy = TestObjectCreator.CreateBranch("BB2", GlbCompany.CurrentCompany);
			branchWithEmptyOrgProxy.GB_OH_OrgProxy = ZGuid.Empty;

			GlbBranch.CurrentBranch.Factory.Save();
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment);
			job.PlugInData = shipment;
			Factory.Save();

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "Charge 1");
			AssertIsInternalJobInfoDisabled(charge.JR_OH_CostAccountInfo);
			AssertIsInternalJobInfoDisabled(charge.JR_OH_SellAccountInfo);

			void AssertIsInternalJobInfoDisabled(ZPropertyInfo propInfoOrgAccount)
			{
				AssertCase_NoOrgAccountInProxy(propInfoOrgAccount);

				AssertCase_DiffernetRegisteredNumber(propInfoOrgAccount);
				AssertCase_DiffernetRegisteredNumber_OnlyBranchIsEmpty(propInfoOrgAccount);
				AssertCase_DiffernetRegisteredNumber_OnlyOrgAccountIsEmpty(propInfoOrgAccount);

				AssertCase_SameRegisteredNumber_Group1(propInfoOrgAccount);
				AssertCase_SameRegisteredNumber_Group2_SameBranchOrgProxy(propInfoOrgAccount);
				AssertCase_SameRegisteredNumber_Group2_DifferentBranchOrgProxy(propInfoOrgAccount);
				AssertCase_SameRegisteredNumber_Empty_SameBranchOrgProxy(propInfoOrgAccount);
				AssertCase_SameRegisteredNumber_Empty_DifferentBranchOrgProxy(propInfoOrgAccount);

				AssertCase_NullChargeBranch_OrgProxyHaveRegisterNumber(propInfoOrgAccount);
				AssertCase_NullChargeBranch_OrgProxyHaveNoRegisterNumber(propInfoOrgAccount);
			}

			void AssertCase_NoOrgAccountInProxy(ZPropertyInfo propInfoOrgAccount)
			{
				charge.JR_GB = GlbBranch.CurrentBranch.PK;
				charge.JR_OH_CostAccount = ZGuid.Empty;
				charge.JR_OH_SellAccount = ZGuid.Empty;
				AssertEquals("PreCondition", GlbBranch.CurrentBranch.PK, charge.JR_GB);
				AssertEquals("PreCondition", null, charge.CostAccount);
				AssertEquals("PreCondition", null, charge.SellAccount);
				var caseInfoForNoOrgAccountInProxy = GetChargeInfo(registeredNumberGroup1, string.Empty);
				AssertIsInternalJobInfoDisabled_True(charge, caseInfoForNoOrgAccountInProxy);
			}

			void AssertCase_DiffernetRegisteredNumber(ZPropertyInfo propInfoOrgAccount)
			{
				propInfoOrgAccount.Value = orgWithRegisteredNumberGroup2.PK;
				AssertEquals("PreCondition", GlbBranch.CurrentBranch.PK, charge.JR_GB);
				AssertEquals("PreCondition", orgWithRegisteredNumberGroup2.PK, propInfoOrgAccount.Value);
				var caseInfoForDiffernetRegisteredNumber = GetChargeInfo(registeredNumberGroup1, registeredNumberGroup2);
				AssertIsInternalJobInfoDisabled_True(charge, caseInfoForDiffernetRegisteredNumber);
			}

			void AssertCase_DiffernetRegisteredNumber_OnlyBranchIsEmpty(ZPropertyInfo propInfoOrgAccount)
			{
				charge.JR_GB = branchWithEmptyRegisteredNumber.PK;
				AssertEquals("PreCondition", branchWithEmptyRegisteredNumber.PK, charge.JR_GB);
				AssertEquals("PreCondition", orgWithRegisteredNumberGroup2.PK, propInfoOrgAccount.Value);
				var caseInfoForDiffernetRegisteredNumber_OnlyBranchIsEmpty = GetChargeInfo(string.Empty, registeredNumberGroup2);
				AssertIsInternalJobInfoDisabled_True(charge, caseInfoForDiffernetRegisteredNumber_OnlyBranchIsEmpty);
			}

			void AssertCase_DiffernetRegisteredNumber_OnlyOrgAccountIsEmpty(ZPropertyInfo propInfoOrgAccount)
			{
				charge.JR_GB = GlbBranch.CurrentBranch.PK;
				propInfoOrgAccount.Value = orgWithEmptyRegisteredNumber.PK;
				AssertEquals("PreCondition", GlbBranch.CurrentBranch.PK, charge.JR_GB);
				AssertEquals("PreCondition", orgWithEmptyRegisteredNumber.PK, propInfoOrgAccount.Value);
				var caseInfoForDiffernetRegisteredNumber_OnlyOrgAccountIsEmpty = GetChargeInfo(registeredNumberGroup2, string.Empty);
				AssertIsInternalJobInfoDisabled_True(charge, caseInfoForDiffernetRegisteredNumber_OnlyOrgAccountIsEmpty);
			}

			void AssertCase_SameRegisteredNumber_Group1(ZPropertyInfo propInfoOrgAccount)
			{
				charge.JR_GB = GlbBranch.CurrentBranch.PK;
				propInfoOrgAccount.Value = GlbBranch.CurrentBranch.OrgProxy.PK;
				AssertEquals("PreCondition", GlbBranch.CurrentBranch.PK, charge.JR_GB);
				AssertEquals("PreCondition", GlbBranch.CurrentBranch.OrgProxy.PK, propInfoOrgAccount.Value);
				Assert("PreCondition, OrgProxy is not unique.", GlbCompany.CurrentCompany.Branches.Count(x => x.GB_OH_OrgProxy == GlbBranch.CurrentBranch.OrgProxy.PK) > 1);
				AssertIsInternalJobInfoDisabled_False(charge, GetChargeInfo(registeredNumberGroup1, registeredNumberGroup1), job, ZGuid.Empty);
			}

			void AssertCase_SameRegisteredNumber_Group2_SameBranchOrgProxy(ZPropertyInfo propInfoOrgAccount)
			{
				charge.JR_GB = branchWithRegisteredNumberGroup2.PK;
				propInfoOrgAccount.Value = orgWithRegisteredNumberGroup2.PK;
				AssertEquals("PreCondition", branchWithRegisteredNumberGroup2.PK, charge.JR_GB);
				AssertEquals("PreCondition", orgWithRegisteredNumberGroup2.PK, propInfoOrgAccount.Value);
				AssertEquals("PreCondition", propInfoOrgAccount.Value, charge.Branch.GB_OH_OrgProxy);
				Assert("PreCondition, OrgProxy is unique.", GlbCompany.CurrentCompany.Branches.Count(x => x.GB_OH_OrgProxy == orgWithRegisteredNumberGroup2.PK) == 1);
				AssertIsInternalJobInfoDisabled_False(charge, GetChargeInfo(registeredNumberGroup2, registeredNumberGroup2), job, branchWithRegisteredNumberGroup2.PK);
			}

			void AssertCase_SameRegisteredNumber_Group2_DifferentBranchOrgProxy(ZPropertyInfo propInfoOrgAccount)
			{
				propInfoOrgAccount.Value = org2WithRegisteredNumberGroup2.PK;
				AssertEquals("PreCondition", branchWithRegisteredNumberGroup2.PK, charge.JR_GB);
				AssertEquals("PreCondition", org2WithRegisteredNumberGroup2.PK, propInfoOrgAccount.Value);
				AssertNotEquals("PreCondition", propInfoOrgAccount.Value, charge.Branch.GB_OH_OrgProxy);
				Assert("PreCondition, OrgProxy is unique.", GlbCompany.CurrentCompany.Branches.Count(x => x.GB_OH_OrgProxy == org2WithRegisteredNumberGroup2.PK) == 1);
				AssertIsInternalJobInfoDisabled_False(charge, GetChargeInfo(registeredNumberGroup2, registeredNumberGroup2), job, branch2WithRegisteredNumberGroup2.PK);
			}

			void AssertCase_SameRegisteredNumber_Empty_SameBranchOrgProxy(ZPropertyInfo propInfoOrgAccount)
			{
				charge.JR_GB = branchWithEmptyRegisteredNumber.PK;
				propInfoOrgAccount.Value = orgWithEmptyRegisteredNumber.PK;
				AssertEquals("PreCondition", branchWithEmptyRegisteredNumber.PK, charge.JR_GB);
				AssertEquals("PreCondition", orgWithEmptyRegisteredNumber.PK, propInfoOrgAccount.Value);
				AssertEquals("PreCondition", propInfoOrgAccount.Value, charge.Branch.GB_OH_OrgProxy);
				Assert("PreCondition, OrgProxy is unique.", GlbCompany.CurrentCompany.Branches.Count(x => x.GB_OH_OrgProxy == orgWithEmptyRegisteredNumber.PK) == 1);
				AssertIsInternalJobInfoDisabled_False(charge, GetChargeInfo(string.Empty, string.Empty), job, branchWithEmptyRegisteredNumber.PK);
			}

			void AssertCase_SameRegisteredNumber_Empty_DifferentBranchOrgProxy(ZPropertyInfo propInfoOrgAccount)
			{
				charge.JR_GB = branchWithEmptyOrgProxy.PK;
				AssertEquals("PreCondition", branchWithEmptyOrgProxy.PK, charge.JR_GB);
				AssertEquals("PreCondition", orgWithEmptyRegisteredNumber.PK, propInfoOrgAccount.Value);
				AssertNotEquals("PreCondition", propInfoOrgAccount.Value, charge.Branch.GB_OH_OrgProxy);
				Assert("PreCondition, OrgProxy is unique.", GlbCompany.CurrentCompany.Branches.Count(x => x.GB_OH_OrgProxy == orgWithEmptyRegisteredNumber.PK) == 1);
				AssertIsInternalJobInfoDisabled_False(charge, GetChargeInfo(string.Empty, string.Empty), job, branchWithEmptyRegisteredNumber.PK);
			}

			void AssertCase_NullChargeBranch_OrgProxyHaveRegisterNumber(ZPropertyInfo propInfoOrgAccount)
			{
				charge.JR_GB = ZGuid.Empty;
				AssertEquals("PreCondition", ZGuid.Empty, charge.JR_GB);

				propInfoOrgAccount.Value = orgWithRegisteredNumberGroup2.PK;
				AssertIsInternalJobInfoDisabled_True(charge, $"Charge Branch is blank, {propInfoOrgAccount.Name} have registered number.");
			}

			void AssertCase_NullChargeBranch_OrgProxyHaveNoRegisterNumber(ZPropertyInfo propInfoOrgAccount)
			{
				charge.JR_GB = ZGuid.Empty;
				AssertEquals("PreCondition", ZGuid.Empty, charge.JR_GB);

				propInfoOrgAccount.Value = orgWithEmptyRegisteredNumber.PK;
				AssertIsInternalJobInfoDisabled_True(charge, $"Charge Branch is blank, {propInfoOrgAccount.Name} have no registered number.");
			}

			string GetChargeInfo(string settingBranchRegisteredNumber, string settingOrgAccountRegisteredNumber)
			{
				var result = new ZStringBuilder();
				result.Append($"[Branch:{charge.Branch?.GB_Code}]");
				result.Append($"[BranchRegisteredNumber:{settingBranchRegisteredNumber}]");
				result.Append($"[CostAccount:{charge.CostAccount?.OH_Code}(Internal:{charge.CostAccount?.IsProxyOrg(charge.Company)})]");
				result.Append($"[SellAccount:{charge.SellAccount?.OH_Code}(Internal:{charge.SellAccount?.IsProxyOrg(charge.Company)})]");
				result.Append($"[InternalOrgAccountRegisteredNumber:{settingOrgAccountRegisteredNumber}]");
				return result.ToString();
			}
		}

		void AssertIsInternalJobInfoDisabled_True(BaseCharge charge, string comment)
		{
			CombineAssertions($"{comment},IsInternalJobInfoDisabled should be true.", () =>
			{
				Assert(nameof(charge.IsInternalJobInfoDisabled), charge.IsInternalJobInfoDisabled);
				Assert(nameof(charge.ShouldCreateCostJRJ), !charge.ShouldCreateCostJRJ);
				Assert(nameof(charge.ShouldCreateSellJRJ), !charge.ShouldCreateSellJRJ);
				AssertEquals(nameof(charge.JR_JH_InternalJob), ZGuid.Empty, charge.JR_JH_InternalJob);
				AssertEquals(nameof(charge.JR_GB_InternalBranch), ZGuid.Empty, charge.JR_GB_InternalBranch);
				AssertEquals(nameof(charge.JR_GE_InternalDept), ZGuid.Empty, charge.JR_GE_InternalDept);
			});
		}

		void AssertIsInternalJobInfoDisabled_False(BaseCharge charge, string comment, Job expectedInternalJob, ZGuid expectedInternalBranch)
		{
			CombineAssertions($"{comment},IsInternalJobInfoDisabled should be false.", () =>
			{
				Assert(nameof(charge.IsInternalJobInfoDisabled), !charge.IsInternalJobInfoDisabled);
				AssertEquals(nameof(charge.JR_JH_InternalJob), expectedInternalJob.PK, charge.JR_JH_InternalJob);
				AssertEquals(nameof(charge.JR_GB_InternalBranch), expectedInternalBranch, charge.JR_GB_InternalBranch);
				AssertEquals(nameof(charge.JR_GE_InternalDept), charge.Job.JH_GE, charge.JR_GE_InternalDept);
			});
		}

		public void TestCostTaxIdAndTaxMessageMapping()
		{
			var taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate1.AT_Type = AccTaxRate.Types.Rated;
			taxRate1.AT_Code = "TaxRate01";
			var taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate2.AT_Type = AccTaxRate.Types.Rated;
			taxRate2.AT_Code = "TaxRate02";
			var taxMsg1 = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg1.A9_Code = "TaxMsg01";
			var taxMsg2 = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg2.A9_Code = "TaxMsg02";
			Factory.Save();

			var config = TestObjectCreator.CreateTaxIdAndTaxMessageCombinationRulesConfiguration(
				(TransactionLineTypes.Cost, taxRate1, taxMsg1),
				(TransactionLineTypes.Cost, taxRate2, taxMsg2)
			);
			AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			var charge = Factory.New<JobCharge>();
			charge.JR_AT_CostGSTRate = taxRate1.PK;
			charge.JR_A9_CostVATClass = taxMsg2.PK;

			var expectedMsg = @"Transaction cannot be saved/posted due to invalid Tax ID and Tax Message combination.
Line Type=CST, Tax ID=TaxRate01, Tax Message=TaxMsg02";
			AssertNoErrors(charge.JR_AT_CostGSTRateInfo);
			AssertHasErrors(expectedMsg, charge.JR_A9_CostVATClassInfo);

			charge.JR_A9_CostVATClass = taxMsg1.PK;
			AssertNoErrors(charge.JR_AT_CostGSTRateInfo);
			AssertNoErrors(charge.JR_A9_CostVATClassInfo);

			taxRate2.AT_A9_DefaultVatClass = taxMsg1.PK;
			charge.JR_AT_CostGSTRate = taxRate2.PK;
			expectedMsg = @"Transaction cannot be saved/posted due to invalid Tax ID and Tax Message combination.
Line Type=CST, Tax ID=TaxRate02, Tax Message=TaxMsg01";
			AssertNoErrors(charge.JR_AT_CostGSTRateInfo);
			AssertHasErrors(expectedMsg, charge.JR_A9_CostVATClassInfo);

			charge.JR_AT_CostGSTRate = ZGuid.Empty;
			AssertNoErrors(charge.JR_AT_CostGSTRateInfo);
			AssertNoErrors(charge.JR_A9_CostVATClassInfo);

			expectedMsg = @"Transaction cannot be saved/posted due to invalid Tax ID and Tax Message combination.
Line Type=CST, Tax ID=TaxRate01, Tax Message=";
			charge.JR_AT_CostGSTRate = taxRate1.PK;
			charge.JR_A9_CostVATClass = ZGuid.Empty;
			AssertNoErrors(charge.JR_AT_CostGSTRateInfo);
			AssertHasErrors(expectedMsg, charge.JR_A9_CostVATClassInfo);

			charge.JR_AT_CostGSTRate = ZGuid.Empty;
			charge.JR_A9_CostVATClass = ZGuid.Empty;
			AssertNoErrors(charge.JR_AT_CostGSTRateInfo);
			AssertNoErrors(charge.JR_A9_CostVATClassInfo);
		}

		public void TestSellTaxIdAndTaxMessageMapping()
		{
			var taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate1.AT_Type = AccTaxRate.Types.Rated;
			taxRate1.AT_Code = "TaxRate01";
			var taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate2.AT_Type = AccTaxRate.Types.Rated;
			taxRate2.AT_Code = "TaxRate02";
			var taxMsg1 = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg1.A9_Code = "TaxMsg01";
			var taxMsg2 = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg2.A9_Code = "TaxMsg02";
			Factory.Save();

			var config = TestObjectCreator.CreateTaxIdAndTaxMessageCombinationRulesConfiguration(
				(TransactionLineTypes.Revenue, taxRate1, taxMsg1),
				(TransactionLineTypes.Revenue, taxRate2, taxMsg2)
			);
			AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			var charge = Factory.New<JobCharge>();
			charge.JR_AT_SellGSTRate = taxRate1.PK;
			charge.JR_A9_SellVATClass = taxMsg2.PK;
			var expectedMsg = @"Transaction cannot be saved/posted due to invalid Tax ID and Tax Message combination.
Line Type=REV, Tax ID=TaxRate01, Tax Message=TaxMsg02";
			AssertNoErrors(charge.JR_AT_SellGSTRateInfo);
			AssertHasErrors(expectedMsg, charge.JR_A9_SellVATClassInfo);

			charge.JR_A9_SellVATClass = taxMsg1.PK;
			AssertNoErrors(charge.JR_AT_SellGSTRateInfo);
			AssertNoErrors(charge.JR_A9_SellVATClassInfo);

			taxRate2.AT_A9_DefaultVatClass = taxMsg1.PK;
			charge.JR_AT_SellGSTRate = taxRate2.PK;
			expectedMsg = @"Transaction cannot be saved/posted due to invalid Tax ID and Tax Message combination.
Line Type=REV, Tax ID=TaxRate02, Tax Message=TaxMsg01";
			AssertNoErrors(charge.JR_AT_SellGSTRateInfo);
			AssertHasErrors(expectedMsg, charge.JR_A9_SellVATClassInfo);

			charge.JR_AT_SellGSTRate = ZGuid.Empty;
			AssertNoErrors(charge.JR_AT_SellGSTRateInfo);
			AssertNoErrors(charge.JR_A9_SellVATClassInfo);

			expectedMsg = @"Transaction cannot be saved/posted due to invalid Tax ID and Tax Message combination.
Line Type=REV, Tax ID=TaxRate01, Tax Message=";
			charge.JR_AT_SellGSTRate = taxRate1.PK;
			charge.JR_A9_SellVATClass = ZGuid.Empty;
			AssertNoErrors(charge.JR_AT_SellGSTRateInfo);
			AssertHasErrors(expectedMsg, charge.JR_A9_SellVATClassInfo);

			charge.JR_AT_SellGSTRate = ZGuid.Empty;
			charge.JR_A9_SellVATClass = ZGuid.Empty;
			AssertNoErrors(charge.JR_AT_SellGSTRateInfo);
			AssertNoErrors(charge.JR_A9_SellVATClassInfo);
		}

		#region Set Supply Type

		public void TestSetSupplyType_JR_ACChange()
		{
			var supplyTypeOverride = TestObjectCreator.FRT.SupplyTypeOverrides.AddNew();
			supplyTypeOverride.ACS_JobType = "SHP";
			supplyTypeOverride.ACS_TransportMode = Constants.TransportModes.All;
			supplyTypeOverride.ACS_Direction = Constants.FreightShipmentDirection.Code.All;
			supplyTypeOverride.ACS_IncoTerm = INCOTermCodes.All;
			supplyTypeOverride.ACS_GE = GlbDepartment.CurrentDepartment.PK.ToGuid();
			supplyTypeOverride.ACS_SupplyType = SupplyTypeClassificationCodes.LOC;
			Factory.Save();

			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var shipment = TestObjectCreator.CreateShipment("S001", true);
			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "Charge 1", TestObjectCreator.AUD, 100m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 100m, TestObjectCreator.Agent);

				AssertEquals(SupplyTypeClassificationCodes.LOC, charge.JR_CostSupplyType);
				AssertEquals(SupplyTypeClassificationCodes.LOC, charge.JR_SellSupplyType);
			}
		}

		public void TestSetSupplyType_JR_GEChange()
		{
			var supplyTypeOverride = TestObjectCreator.FRT.SupplyTypeOverrides.AddNew();
			supplyTypeOverride.ACS_JobType = "SHP";
			supplyTypeOverride.ACS_TransportMode = Constants.TransportModes.All;
			supplyTypeOverride.ACS_Direction = Constants.FreightShipmentDirection.Code.All;
			supplyTypeOverride.ACS_IncoTerm = INCOTermCodes.All;
			supplyTypeOverride.ACS_GE = testObjectCreator.NonCurrentDepartment.PK.ToGuid();
			supplyTypeOverride.ACS_SupplyType = SupplyTypeClassificationCodes.LOA;
			Factory.Save();

			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var shipment = TestObjectCreator.CreateShipment("S001", true);
			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "Charge 1", TestObjectCreator.AUD, 100m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 100m, TestObjectCreator.Agent);

				AssertEquals(GlbDepartment.CurrentDepartment.PK, charge.JR_GE);
				AssertEquals(string.Empty, charge.JR_CostSupplyType);
				AssertEquals(string.Empty, charge.JR_SellSupplyType);

				charge.JR_GE = TestObjectCreator.NonCurrentDepartment.PK;

				AssertEquals(SupplyTypeClassificationCodes.LOA, charge.JR_CostSupplyType);
				AssertEquals(SupplyTypeClassificationCodes.LOA, charge.JR_SellSupplyType);
			}
		}

		public void TestSetSupplyTypeWithChargeTypeOverride_DSB()
		{
			AssertEquals("Precondition", Core.Constants.ChargeType.Margin, TestObjectCreator.FRT.AC_ChargeType);

			var typeOverride = TestObjectCreator.FRT.ChargeTypeOverrides.AddNew();
			typeOverride.AN_JobType = "SHP";
			typeOverride.AN_ChargeType = Core.Constants.ChargeType.Disbursement;
			typeOverride.AN_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			typeOverride.AN_JobDirection = Constants.FreightShipmentDirection.Code.All;
			Factory.Save();

			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AlwaysSetSupplyTypetoDSBIfChargeTypeIsDSB.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var shipment = TestObjectCreator.CreateShipment("S001", true);
			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "Charge 1", TestObjectCreator.AUD, 100m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 100m, TestObjectCreator.Agent);

				AssertEquals(SupplyTypeClassificationCodes.DSB, charge.JR_CostSupplyType);
				AssertEquals(SupplyTypeClassificationCodes.DSB, charge.JR_SellSupplyType);
			}
		}

		#endregion

		public void TestGovtChargeCodeDefaulting()
		{
			TestObjectCreator.SetupOrCreateGovtChargeCodeOverride(TestObjectCreator.CC1, "COS", "SHP", "ALL", "ALL", "CodeForCost");
			TestObjectCreator.SetupOrCreateGovtChargeCodeOverride(TestObjectCreator.CC1, "REV", "SHP", "ALL", "ALL", "CodeForRevenue");
			TestObjectCreator.CC1.AC_GovtChargeCode = "54321";

			Factory.Save();

			var consol = TestObjectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
			var consolJob = new Job.Loader(consol).TryLoadOrCreateWithoutMutexForTestOnly();

			var shipment = TestObjectCreator.CreateShipment("S001");
			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();

			AssertWhenRegistryOff(consolJob, shipmentJob);
			AssertWhenRegistryOn(new[] {
				(consolJob , "54321", "54321"),
				(shipmentJob , "CodeForCost", "CodeForRevenue"),
			});

			void AssertWhenRegistryOff(params Job[] testingJobs)
			{
				AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				CombineAssertions("When registry off , govt charge code should be kept empty.", () =>
				{
					testingJobs.ForEach(job =>
					{
						var jobCharge = job.Charges.AddNew();
						AssertNullOrEmpty(jobCharge.JR_CostGovtChargeCode);
						AssertNullOrEmpty(jobCharge.JR_SellGovtChargeCode);

						jobCharge.JR_AC = TestObjectCreator.CC1.PK;
						AssertNullOrEmpty(jobCharge.JR_CostGovtChargeCode);
						AssertNullOrEmpty(jobCharge.JR_SellGovtChargeCode);

						jobCharge.JR_AC = ZGuid.Empty;
						AssertNullOrEmpty(jobCharge.JR_CostGovtChargeCode);
						AssertNullOrEmpty(jobCharge.JR_SellGovtChargeCode);
					});
				});
			}

			void AssertWhenRegistryOn(params (Job Job, string ExpectGovtCodeForCost, string ExpectGovtCodeForRevenue)[] testingCases)
			{
				AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				testingCases.ForEach(testCase =>
				{
					var jobCharge = testCase.Job.Charges.AddNew();
					AssertNullOrEmpty(jobCharge.JR_CostGovtChargeCode);
					AssertNullOrEmpty(jobCharge.JR_SellGovtChargeCode);

					jobCharge.JR_AC = TestObjectCreator.CC1.PK;
					AssertEquals(testCase.ExpectGovtCodeForCost, jobCharge.JR_CostGovtChargeCode);
					AssertEquals(testCase.ExpectGovtCodeForRevenue, jobCharge.JR_SellGovtChargeCode);

					jobCharge.JR_AC = ZGuid.Empty;
					AssertNullOrEmpty(jobCharge.JR_CostGovtChargeCode);
					AssertNullOrEmpty(jobCharge.JR_SellGovtChargeCode);
				});
			}
		}

		public void TestJR_GB_InternalBranchValidationOfOrgProxyBranches_RelatedShipment()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());
			var orgProxy1 = TestObjectCreator.CreateOrgHeader("Proxy1", true, true);
			var orgProxy2 = TestObjectCreator.CreateOrgHeader("Proxy2", true, true);
			var branch1 = TestObjectCreator.CreateBranch("NW1", GlbCompany.CurrentCompany, orgProxy1);
			var branch2 = TestObjectCreator.CreateBranch("NW2", GlbCompany.CurrentCompany, orgProxy2);
			var consol = TestObjectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment = TestObjectCreator.CreateShipment("S001", consol);

			using (var consolJob = TestObjectCreator.CreateJob(consol))
			using (var shipmentJob = TestObjectCreator.CreateJob(shipment))
			{
				shipmentJob.JH_GB = branch1.PK;
				Factory.Save();

				var charge = TestObjectCreator.CreateCharge(consolJob, TestObjectCreator.FRT, 100, 100);
				charge.JR_Calc_RelatedJobNumber = shipment.JobNumber;

				charge.JR_OH_SellAccount = orgProxy1.PK;
				AssertEquals("Internal branch is defaulted to shipment's branch", branch1.PK, charge.JR_GB_InternalBranch);
				AssertNoErrors(charge.JR_GB_InternalBranchInfo);

				charge.JR_OH_SellAccount = orgProxy2.PK;
				AssertEquals("Internal branch is defaulted to shipment's branch", branch1.PK, charge.JR_GB_InternalBranch);
				AssertHasErrorContaining(charge.JR_GB_InternalBranchInfo, $"The branch you have selected does not match the Sell Account.");
				charge.JR_OH_SellAccount = ZGuid.Empty;

				charge.JR_OH_CostAccount = orgProxy1.PK;
				AssertEquals("Internal branch is defaulted to branch with creditor as org proxy", branch1.PK, charge.JR_GB_InternalBranch);
				AssertNoErrors(charge.JR_GB_InternalBranchInfo);

				charge.JR_OH_CostAccount = orgProxy2.PK;
				AssertEquals("Internal branch is defaulted to branch with creditor as org proxy", branch2.PK, charge.JR_GB_InternalBranch);
				AssertNotEquals("Internal branch is not set to shipment's when creditor is org proxy", shipmentJob.JH_GB, charge.JR_GB_InternalBranch);
				AssertNoErrors(charge.JR_GB_InternalBranchInfo);
			}
		}

		public void TestInternalFieldsWithOrgProxyGatewayAgentsAndNonGatewayAgents()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());

			(OrgHeader org1, OrgHeader org2, OrgHeader org3, GlbBranch branch1, GlbBranch branch2, GlbBranch branch3) GetSetupForProxyBranches()
			{
				var org1 = TestObjectCreator.CreateOrgHeader("Org1", false, false);
				var org2 = TestObjectCreator.CreateOrgHeader("Org2", false, false);
				var org3 = TestObjectCreator.CreateOrgHeader("Org3", false, false);
				var testBranch1 = TestObjectCreator.CreateBranch("BR1", "Branch1", GlbCompany.CurrentCompany, org1);
				var testBranch2 = TestObjectCreator.CreateBranch("BR2", "Branch2", GlbCompany.CurrentCompany, org2);
				var testBranch3 = TestObjectCreator.CreateBranch("BR3", "Branch3", GlbCompany.CurrentCompany, org3);

				var port1 = org1.AppointedGatewayAgentPorts.AddNew();
				port1.O5_OA_AgentOfficeAddress = org1.MainAddress.PK;
				port1.O5_PortOrCountry = "AUSYD";
				port1.O5_AgentDirection = AgentDirectionList.Codes.Both;
				port1.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;

				var port2 = org2.AppointedGatewayAgentPorts.AddNew();
				port2.O5_OA_AgentOfficeAddress = org2.MainAddress.PK;
				port2.O5_PortOrCountry = "NZAKL";
				port2.O5_AgentDirection = AgentDirectionList.Codes.Both;
				port2.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;

				Factory.Save();

				return (org1, org2, org3, testBranch1, testBranch2, testBranch3);
			}

			(var orgProxy1, var orgProxy2, var orgProxy3, var branch1, var branch2, var branch3) = GetSetupForProxyBranches();

			var consol = TestObjectCreator.CreateGatewayConsol(sendingGatewayAgent: orgProxy1, receivingGatewayAgent: orgProxy2);

			using (var job = TestObjectCreator.CreateJob(consol))
			{
				var charge = job.Charges.AddNew();
				charge.JR_OH_CostAccount = orgProxy2.PK;
				Assert("Internal department should default to blank when cost account is gateway", charge.JR_GE_InternalDept.IsEmpty);
				Assert("Internal job should default to blank when cost account is gateway", charge.JR_JH_InternalJob.IsEmpty);
				Assert("Internal branch should default to blank when cost account is gateway", charge.JR_GB_InternalBranch.IsEmpty);

				charge.JR_OH_CostAccount = orgProxy3.PK;
				AssertEquals("Internal department should default to department of org proxy when cost account is not gateway", job.JH_GE, charge.JR_GE_InternalDept);
				AssertEquals("Internal job should default to job of org proxy when cost account is not gateway", job.PK, charge.JR_JH_InternalJob);
				AssertEquals("Internal branch should default to the branch of org proxy when cost account is not gateway", branch3.PK, charge.JR_GB_InternalBranch);
			}
		}

		public void TestResetInternalFieldsToBlankWhenCreditorIsOrgProxyGatewayAgent()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());
			var orgProxy1 = TestObjectCreator.CreateOrgHeader("Proxy1", true, true);
			var orgProxy2 = TestObjectCreator.CreateOrgHeader("Proxy2", true, true);
			var branch1 = TestObjectCreator.CreateBranch("NW1", GlbCompany.CurrentCompany, orgProxy1);
			var branch2 = TestObjectCreator.CreateBranch("NW2", GlbCompany.CurrentCompany, orgProxy2);
			var shipment = TestObjectCreator.CreateShipment("S001", true);

			using (var shipmentJob = TestObjectCreator.CreateJob(shipment))
			{
				shipmentJob.JH_GB = branch1.PK;
				Factory.Save();

				var charge = TestObjectCreator.CreateCharge(shipmentJob, TestObjectCreator.FRT, 100, 100);

				charge.JR_OH_CostAccount = orgProxy1.PK;
				AssertEquals("Internal department should default to shipment's job's department when cost account is not gateway", shipmentJob.JH_GE, charge.JR_GE_InternalDept);
				AssertEquals("Internal job should default to shipment's job when cost account is not gateway", shipmentJob.PK, charge.JR_JH_InternalJob);
				AssertEquals("Internal branch should default to the branch of org proxy when cost account is not gateway", branch1.PK, charge.JR_GB_InternalBranch);

				charge.JR_OH_CostAccount = orgProxy2.PK;
				AssertEquals("Internal department should default to shipment's job's department when cost account is not gateway", shipmentJob.JH_GE, charge.JR_GE_InternalDept);
				AssertEquals("Internal job should default to shipment's job when cost account is not gateway", shipmentJob.PK, charge.JR_JH_InternalJob);
				AssertEquals("Internal branch should default to the branch of org proxy when cost account is not gateway", branch2.PK, charge.JR_GB_InternalBranch);

				var shipmentGateway = shipment.Gateways.AddNew();
				shipmentGateway.ForwarderPK = orgProxy1.PK;

				charge.JR_OH_CostAccount = orgProxy1.PK;
				Assert("Internal department should default to blank when cost account is gateway", charge.JR_GE_InternalDept.IsEmpty);
				Assert("Internal job should default to blank when cost account is gateway", charge.JR_JH_InternalJob.IsEmpty);
				Assert("Internal branch should default to blank when cost account is gateway", charge.JR_GB_InternalBranch.IsEmpty);
			}
		}

		[ExpectNoExceptions]
		public void TestGetChargeTypeAfterDeleteChargeTypeOverride()
		{
			var chargeTypeOverride = TestObjectCreator.CC1.ChargeTypeOverrides.AddNew();
			chargeTypeOverride.AN_ChargeType = Core.Constants.ChargeType.Disbursement;
			chargeTypeOverride.AN_JobType = "ALL";
			chargeTypeOverride.AN_JobDirection = "ALL";
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S001", true);
			using (var testJob = TestObjectCreator.CreateJob(shipment))
			{
				var charge = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Charge 1", TestObjectCreator.AUD, 100m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 100m, TestObjectCreator.Agent);
				charge.JR_APInvoiceNum = "INV001";
				Factory.Save();
				ConcurrentDictionary<string, IAccChargeTypeOverride> cachedValue;

				AssertEquals("Get Override ChargeType: DSB", Core.Constants.ChargeType.Disbursement, charge.ChargeType);
				Factory.TryGetValueFromCacheOnly(FindboxLookupCollections.CachingKey + "JobAccChargeTypeOverrideDictionary", out cachedValue);
				AssertNotNull(cachedValue);
				AssertEquals(1, cachedValue.Count);
				AssertEquals("Cached Override ChargeType: DSB", Core.Constants.ChargeType.Disbursement, cachedValue.FirstOrDefault().Value.AN_ChargeType);

				var newFactory = new BusinessObjectFactory();
				var chargeOverrideInNewFactory = newFactory.Load<AccChargeTypeOverride>(chargeTypeOverride.PK);
				chargeOverrideInNewFactory.Delete();
				newFactory.Save();

				Factory.TryGetValueFromCacheOnly(FindboxLookupCollections.CachingKey + "JobAccChargeTypeOverrideDictionary", out cachedValue);
				AssertNull("Cache Cleared", cachedValue);

				AssertEquals("Get Original ChargeType: MRG", Core.Constants.ChargeType.Margin, charge.ChargeType);

				Factory.TryGetValueFromCacheOnly(FindboxLookupCollections.CachingKey + "JobAccChargeTypeOverrideDictionary", out cachedValue);
				AssertNotNull(cachedValue);
				AssertEquals(1, cachedValue.Count);
				AssertEquals("Cached Original ChargeType: MRG", Core.Constants.ChargeType.Margin, cachedValue.FirstOrDefault().Value.AN_ChargeType);
			}
		}

		public void TestIsAllowedToPostSellCharge()
		{
			#region Setup security

			GlbStaff testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_IsController = false;
			Factory.Save();

			var securityFactory = new BusinessObjectFactory();
			SecurityCore security = new UserLoginController().GetSecurityForUser(testUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);

			var loginSecurity = securityFactory.New<GlbSecurity>();
			loginSecurity.GU_GB = TestObjectCreator.NonCurrentBranch.PK;
			loginSecurity.GU_GE = TestObjectCreator.NonCurrentDepartment.PK;
			loginSecurity.GU_GS = testUser.PK;
			loginSecurity.GU_SecurityRight = security.Login.Code;
			loginSecurity.GU_SecurityItemIsAllowed = false;

			var invoicingSecurity = securityFactory.New<GlbSecurity>();
			invoicingSecurity.GU_GB = Env.CurrentBranch.PK;
			invoicingSecurity.GU_GE = Env.CurrentDepartment.PK;
			invoicingSecurity.GU_GS = testUser.PK;
			invoicingSecurity.GU_SecurityRight = security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowEnterModifyCharges).Code;
			invoicingSecurity.GU_SecurityItemIsAllowed = false;
			security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowEnterModifyCharges).IsAllowed = false;
			securityFactory.Save();

			#endregion

			AssertIsAllowedToPostSellCharge(true, true, false);
			AssertIsAllowedToPostSellCharge(true, false, true);
			AssertIsAllowedToPostSellCharge(false, true, true);
			AssertIsAllowedToPostSellCharge(false, false, true);

			void AssertIsAllowedToPostSellCharge(bool isRegistryOn, bool isUserInteractive, bool result)
			{
				using (Env.SetTemporaryUserContext(testUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
				using (AccountingConfigurationRegistry.Instance.RestrictPostingOfSellChargesVisibleToLoginUserOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isRegistryOn))
				{
					ForwardingShipment shipment = TestObjectCreator.CreateShipment(TestObjectCreator.GetRandomString(9));
					var job = TestObjectCreator.CreateJob(shipment);
					var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge 1", TestObjectCreator.AUD, 100m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 100m, TestObjectCreator.Debtor);
					charge1.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
					charge1.JR_GE = TestObjectCreator.NonCurrentDepartment.PK;

					var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge 2", TestObjectCreator.AUD, 200m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 200m, TestObjectCreator.Debtor);
					charge2.JR_GB = Env.CurrentBranch.PK;
					charge2.JR_GE = Env.CurrentDepartment.PK;

					Factory.Save();

					Assert("Charge 1 can't be viewed.", !charge1.IsAllowedToViewThisCharge);
					Assert("Charge 2 can be viewed.", charge2.IsAllowedToViewThisCharge);

					Globals.IsUserInteractive = isUserInteractive;
					AssertEquals(result, charge1.IsAllowedToPostSellCharge);
					AssertEquals(true, charge2.IsAllowedToPostSellCharge);
				}
			}
		}

		public void TestAddPaymentBasis_ShouldPopulatePaymentBasesWithJobPaymentBases()
		{
			void Test(PaymentBasis[] bases, object[] expectedJobPaymentBases, string message)
			{
				var shipment = TestObjectCreator.CreateShipment(TestObjectCreator.GetRandomString(9));

				using (var job = TestObjectCreator.CreateJob(shipment))
				{
					var charge = job.Charges.AddNew();
					charge.AddPaymentBases(bases, true);

					AssertContainsExactElementsInAnyOrder(message, expectedJobPaymentBases, charge.PaymentBases.Select(p => new
					{
						p.PBS_ChargeableAmount,
						p.PBS_ChargeableUnit,
						p.PBS_MinRate,
						p.PBS_FlatRate,
						p.PBS_PerUnitRate,
						p.PBS_RateUnit,
						p.PBS_RateReference
					}));
				}
			}

			Test(
				bases: new[]
				{
					new PaymentBasis(
						default,
						RateInfo.CreateMIN(1000, "UAH"), AdapterType.Consolidation,
						"McLaren"),

					new PaymentBasis(
						default,
						RateInfo.CreateFLT(2000, "UAH"), AdapterType.Consolidation,
						"McLaren"),

					new PaymentBasis(
						new Quantity(100, "KG"),
						RateInfo.CreateUNT(20, "KG", "UAH"),
						AdapterType.Consolidation, "McLaren")
				},
				expectedJobPaymentBases: new object[]
				{
					new
					{
						PBS_ChargeableAmount = (ZDecimal)100m,
						PBS_ChargeableUnit = (ZString)"KG",
						PBS_MinRate = (ZDecimal)1000m,
						PBS_FlatRate = (ZDecimal)0m,
						PBS_PerUnitRate = (ZDecimal)20m,
						PBS_RateUnit = (ZString)"KG",
						PBS_RateReference = (ZString)"UNT"
					},
					new
					{
						PBS_ChargeableAmount = (ZDecimal)1m,
						PBS_ChargeableUnit = ZString.Empty,
						PBS_MinRate = (ZDecimal)0m,
						PBS_FlatRate = (ZDecimal)2000m,
						PBS_PerUnitRate = (ZDecimal)0m,
						PBS_RateUnit = (ZString)"1",
						PBS_RateReference = (ZString)"FLT"
					}
				},
				message: "Per Unit basis should preserve Min rate details");

			Test(
				bases: new[]
				{
					new PaymentBasis(
						default,
						RateInfo.CreateMIN(1000, "UAH"), AdapterType.Consolidation,
						"McLaren"),

					new PaymentBasis(
						default,
						RateInfo.CreateFLT(200, "UAH"), AdapterType.Consolidation,
						"McLaren"),

					new PaymentBasis(
						new Quantity(100, "KG"),
						RateInfo.CreateUNT(5, "KG", "UAH"),
						AdapterType.Consolidation, "McLaren")
				},
				expectedJobPaymentBases: new object[]
				{
					new
					{
						PBS_ChargeableAmount = (ZDecimal)100m,
						PBS_ChargeableUnit = (ZString)"KG",
						PBS_MinRate = (ZDecimal)1000m,
						PBS_FlatRate = (ZDecimal)200m,
						PBS_PerUnitRate = (ZDecimal)5m,
						PBS_RateUnit = (ZString)"KG",
						PBS_RateReference = (ZString)"MIN"
					}
				},
				message: "Min basis should preserve Per Unit and Flat details");
		}

		public void TestGSTApplicability()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			OrgHeader org1 = TestObjectCreator.AALSHI;
			org1.CompanyData.OB_IsDebtor = true;
			org1.CompanyData.SetARTaxApplicable(true);
			org1.CompanyData.OB_ARWHTApplicable = false;
			org1.OH_FullName = "Test Debtor";
			org1.CompanyData.InvoiceRollupOrGroups[0].PG_InvoicePostingStyle = "DFI";

			OrgHeader org2 = TestObjectCreator.ABIGAS;
			org2.CompanyData.OB_IsCreditor = true;
			org2.CompanyData.SetAPTaxApplicable(true);
			org2.CompanyData.OB_APWHTApplicable = false;
			org2.OH_FullName = "Test Creditor";

			OrgAddress localChargesAddr = Factory.NewWithValidTestData<OrgAddress>();
			OrgAddress agentCollectAddr = Factory.NewWithValidTestData<OrgAddress>();

			Factory.Save();

			AccChargeCode chargeCode = TestObjectCreator.CC1;
			chargeCode.AC_AT_GSTRate = TestObjectCreator.GST1.PK;

			Job testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_JobNum = "123";
			testJob.JH_GB = GlbBranch.CurrentBranch.PK;
			testJob.JH_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "FIS")).PK;
			testJob.JH_OA_LocalChargesAddr = localChargesAddr.PK;
			testJob.JH_OA_AgentCollectAddr = agentCollectAddr.PK;

			Charge testCharge = testJob.Charges.AddNew();
			testCharge.JR_OH_SellAccount = org1.PK;
			testCharge.JR_AC = chargeCode.PK;
			testCharge.JR_InvoiceType = "FIN";
			testCharge.JR_OSSellAmt = 100m;

			Factory.Save();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			Factory.Save();

			JobCollection jobs = new JobCollection(Factory);
			jobs.Add(testJob);

			Assert("Sell TestObjectCreator.GST1 rate should not be actual", !testCharge.IsSellGSTRateActual);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			org1.CompanyData.SetARTaxApplicable(false);
			Factory.Save();

			Assert("Sell TestObjectCreator.GST1 rate should not be actual", !testCharge.IsSellGSTRateActual);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			org1.CompanyData.SetARTaxApplicable(true);
			Factory.Save();

			Assert("Sell TestObjectCreator.GST1 rate should be actual", testCharge.IsSellGSTRateActual);

			testCharge.JR_OH_CostAccount = org2.PK;
			testCharge.JR_AC = chargeCode.PK;
			testCharge.JR_InvoiceType = "FIN";
			testCharge.JR_OSCostAmt = 100m;

			Factory.Save();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			Factory.Save();

			Assert("Cost TestObjectCreator.GST1 rate should not be actual", !testCharge.IsCostGSTRateActual);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			org2.CompanyData.SetAPTaxApplicable(false);
			Factory.Save();

			Assert("Cost TestObjectCreator.GST1 rate should not be actual", !testCharge.IsCostGSTRateActual);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			org2.CompanyData.SetAPTaxApplicable(true);
			Factory.Save();

			Assert("Cost TestObjectCreator.GST1 rate should be actual", testCharge.IsCostGSTRateActual);
		}

		public void TestGSTApplicabilityForCommentChargeCodes()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			OrgHeader debtor = TestObjectCreator.AALSHI;
			debtor.CompanyData.OB_IsDebtor = true;
			debtor.CompanyData.SetARTaxApplicable(true);
			debtor.CompanyData.OB_ARWHTApplicable = false;
			debtor.CompanyData.InvoiceRollupOrGroups[0].PG_InvoicePostingStyle = "DFI";

			OrgHeader creditor = TestObjectCreator.ABIGAS;
			creditor.CompanyData.OB_IsCreditor = true;
			creditor.CompanyData.SetAPTaxApplicable(true);
			creditor.CompanyData.OB_APWHTApplicable = false;

			OrgAddress localChargesAddr = Factory.NewWithValidTestData<OrgAddress>();
			OrgAddress agentCollectAddr = Factory.NewWithValidTestData<OrgAddress>();

			Factory.Save();

			AccChargeCode chargeCode = TestObjectCreator.CC1;
			chargeCode.AC_ChargeType = Constants.ChargeType.Comment;
			chargeCode.AC_AT_GSTRate = TestObjectCreator.GST1.PK;

			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_JobNum = "123";
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "FIS")).PK;
			job.JH_OA_LocalChargesAddr = localChargesAddr.PK;
			job.JH_OA_AgentCollectAddr = agentCollectAddr.PK;

			Charge charge = job.Charges.AddNew();
			charge.JR_OH_SellAccount = debtor.PK;
			charge.JR_AC = chargeCode.PK;
			charge.JR_InvoiceType = "FIN";
			charge.JR_OSSellAmt = 0m;
			charge.JR_OH_CostAccount = creditor.PK;
			charge.JR_AC = chargeCode.PK;
			charge.JR_InvoiceType = "FIN";
			charge.JR_OSCostAmt = 0m;

			Factory.Save();

			debtor.CompanyData.SetARTaxApplicable(false);
			creditor.CompanyData.SetAPTaxApplicable(false);
			Factory.Save();

			Assert("Sell TestObjectCreator.GST1 rate should be actual (because charge code type is CMT)", charge.IsSellGSTRateActual);
			Assert("Cost TestObjectCreator.GST1 rate should be actual (because charge code type is CMT)", charge.IsCostGSTRateActual);
		}

		public void TestResetUnpostedSellTaxDefault()
		{
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_IsDebtor = true;
			org.CompanyData.SetARTaxApplicable(false);
			org.CompanyData.OB_ARWHTApplicable = false;
			org.OH_FullName = "Test Debtor";

			var chargeCode = TestObjectCreator.CC1;
			chargeCode.AC_AT_GSTRate = TestObjectCreator.GST1.PK;
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment);
			job.JH_GB_TaxBranch = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			var testCharge = TestObjectCreator.CreateCharge(job, chargeCode, debtor: org, osSellAmt: 100m);
			testCharge.JR_InvoiceType = "FIN";
			Factory.Save();

			AssertEquals("Pre-condition: when org tax not applicable", true, testCharge.IsSellGSTRateActual);
			AssertEquals(false, testCharge.ChargeCode.IsComment);
			AssertEquals(false, testCharge.IsSellGSTApplicable);
			AssertEquals(true, AccountingMasterFilesUtils.IsTaxBranchApplicable);
			AssertEquals(ZGuid.Empty, testCharge.JR_AT_SellGSTRate);
			AssertEquals(0M, testCharge.JR_Sell_LocalGSTAmount);
			AssertEquals(ZGuid.Empty, testCharge.JR_GB_SellTaxBranch);

			org.CompanyData.SetARTaxApplicable(true);
			Factory.Save();

			AssertEquals("Pre-condition: when org tax applicable", false, testCharge.IsSellGSTRateActual);
			AssertEquals(true, testCharge.IsSellGSTApplicable);

			testCharge.ResetUnpostedSellTaxDefault();

			AssertEquals(true, testCharge.IsSellGSTRateActual);
			AssertEquals(TestObjectCreator.GST1.PK, testCharge.JR_AT_SellGSTRate);
			AssertEquals(10M, testCharge.JR_Sell_LocalGSTAmount);
			AssertEquals(job.JH_GB_TaxBranch, testCharge.JR_GB_SellTaxBranch);
		}

		public void TestResetUnpostedSellTaxDefaultForCommentCharge()
		{
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_IsDebtor = true;
			org.CompanyData.SetARTaxApplicable(true);
			org.CompanyData.OB_ARWHTApplicable = false;
			org.OH_FullName = "Test Debtor";

			var chargeCode = TestObjectCreator.CC1;
			chargeCode.AC_AT_GSTRate = TestObjectCreator.GST1.PK;
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment);
			job.JH_GB_TaxBranch = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			var testCharge = TestObjectCreator.CreateCharge(job, chargeCode, debtor: org, osSellAmt: 100m);
			testCharge.JR_GB_SellTaxBranch = ZGuid.Empty;
			testCharge.JR_InvoiceType = "FIN";
			Factory.Save();

			AssertEquals(false, testCharge.ChargeCode.IsComment);
			AssertEquals(true, testCharge.IsSellGSTApplicable);
			AssertEquals(true, AccountingMasterFilesUtils.IsTaxBranchApplicable);
			AssertEquals(ZGuid.Empty, testCharge.JR_GB_SellTaxBranch);

			testCharge.ChargeCode.AC_ChargeType = Constants.ChargeType.Comment;
			Factory.Save();

			AssertEquals(true, testCharge.ChargeCode.IsComment);

			testCharge.ResetUnpostedSellTaxDefault();

			AssertEquals(ZGuid.Empty, testCharge.JR_GB_SellTaxBranch);

			testCharge.ChargeCode.AC_ChargeType = Constants.ChargeType.Revenue;
			Factory.Save();

			AssertEquals(false, testCharge.ChargeCode.IsComment);

			testCharge.ResetUnpostedSellTaxDefault();

			AssertEquals(job.JH_GB_TaxBranch, testCharge.JR_GB_SellTaxBranch);
		}

		public void TestSuspendCostTaxCalulation()
		{
			TestObjectCreator.GST2.AT_A9_DefaultVatClass = TestObjectCreator.TaxMsg1.PK;
			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX");
			var consolcost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1);
			consolcost.E6_AT_TaxRate = TestObjectCreator.GST2.PK;
			consolcost.E6_A9_VATClass = TestObjectCreator.TaxMsg1.PK;

			var charge = Factory.NewWithValidTestData<BaseCharge>();
			charge.JR_AC = TestObjectCreator.CC1.PK;

			TestObjectCreator.Creditor1.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			var currentBranch = Factory.Load<GlbBranch>(Env.CurrentBranchPK);
			AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(charge.InvoicingJob.PlugInData, CostSell.Cost, PlaceOfSupplyTypes.State.Code, "NSW", branch: currentBranch);
			Factory.Save();

			var accountingTestObjectCreator = new MasterFiles.Business.Testing.Accounting.Helpers.AccountingTestObjectCreator(Factory);
			var taxOverride = TestObjectCreator.CC1.TaxOverrides.AddNew();
			var overrideSetting = accountingTestObjectCreator.PopulateTaxOverride(taxOverride, TestObjectCreator.GST2.PK);
			overrideSetting.AO_SupplyType = SupplyTypeClassificationCodes.LOC;
			Factory.Save();

			AssertSuspendCalculatingFromCostSupplyType();
			AssertSuspendCalculatingFromCostPlaceOfSupply();
			AssertSuspendCalculatingFromCreditor();
			AssertSuspendCalculatingFromTaxRate();
			AssertApplyTaxInfoFromConsolCost();

			void AssertSuspendCalculatingFromCostSupplyType()
			{
				charge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
				charge.JR_CostSupplyType = ZString.Empty;
				charge.JR_CostPlaceOfSupply = ZString.Empty;
				ResetCharge();
				using (charge.SuspendCostTaxCalulation(false))
				{
					charge.JR_CostSupplyType = SupplyTypeClassificationCodes.LOC;
				}
				AssertEquals(ZGuid.Empty, charge.JR_AT_CostGSTRate);
				AssertEquals(ZGuid.Empty, charge.JR_A9_CostVATClass);
			}

			void AssertSuspendCalculatingFromCostPlaceOfSupply()
			{
				charge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
				charge.JR_CostSupplyType = ZString.Empty;
				charge.JR_CostPlaceOfSupply = ZString.Empty;
				ResetCharge();
				using (charge.SuspendCostTaxCalulation(false))
				{
					charge.JR_CostPlaceOfSupply = "NSW";
				}
				AssertEquals(ZGuid.Empty, charge.JR_AT_CostGSTRate);
				AssertEquals(ZGuid.Empty, charge.JR_A9_CostVATClass);
			}

			void AssertSuspendCalculatingFromCreditor()
			{
				charge.JR_CostSupplyType = SupplyTypeClassificationCodes.LOC;
				charge.JR_CostPlaceOfSupply = ZString.Empty;
				charge.JR_OH_CostAccount = ZGuid.Empty;
				ResetCharge();
				using (charge.SuspendCostTaxCalulation(false))
				{
					charge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
				}
				AssertEquals(ZGuid.Empty, charge.JR_AT_CostGSTRate);
				AssertEquals(ZGuid.Empty, charge.JR_A9_CostVATClass);
			}

			void AssertSuspendCalculatingFromTaxRate()
			{
				ResetCharge();
				using (charge.SuspendCostTaxCalulation(false))
				{
					charge.JR_AT_CostGSTRate = TestObjectCreator.GST2.PK;
				}
				AssertEquals(TestObjectCreator.GST2.PK, charge.JR_AT_CostGSTRate);
				AssertEquals(ZGuid.Empty, charge.JR_A9_CostVATClass);
			}

			void AssertApplyTaxInfoFromConsolCost()
			{
				charge.JR_E6 = consolcost.PK;
				ResetCharge();
				using (charge.SuspendCostTaxCalulation(true))
				{
				}
				AssertEquals(TestObjectCreator.GST2.PK, charge.JR_AT_CostGSTRate);
				AssertEquals(TestObjectCreator.TaxMsg1.PK, charge.JR_A9_CostVATClass);
			}

			void ResetCharge()
			{
				charge.JR_AT_CostGSTRate = ZGuid.Empty;
				charge.JR_A9_CostVATClass = ZGuid.Empty;
				AssertEquals("Precondition", ZGuid.Empty, charge.JR_AT_CostGSTRate);
				AssertEquals("Precondition", ZGuid.Empty, charge.JR_A9_CostVATClass);
			}
		}

		public void TestSaveWithNegativeExchangeRateShouldThrowException()
		{
			var company = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, GlbCompany.CurrentCompany.GC_Code);
			company.GC_IsReciprocal = true;
			company.GC_RX_NKLocalCurrency = Constants.CurrencyCodes.Egypt;
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S00001000", "AUSYD", "NZAKL");
			var job = TestObjectCreator.CreateJob(shipment);

			var exRate = TestObjectCreator.CreateExchangeRate(job, TestObjectCreator.USD, 0.032m);
			exRate.OrgType = ExchangeRateOrgTypeEnum.Debtor;
			exRate.JF_CFXPercent = 0m;
			exRate.JF_CFXMinimum = 2m;
			exRate.JF_OH_Org = GlbCompany.CurrentCompany.OrgProxy.PK;
			Factory.Save();

			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.DSBChargeCode, "", GlbCompany.CurrentCompany.LocalCurrency, debtor: GlbCompany.CurrentCompany.OrgProxy);
			charge1.JR_LocalSellAmt = 20m;
			charge1.JR_RX_NKSellCurrency = Constants.CurrencyCodes.UnitedStates;
			charge1.JR_RX_NKCostCurrency = Constants.CurrencyCodes.Egypt;
			Factory.Save();

			var service = Factory.ServiceContainer.GetService<NegativeJobChargeOSSellExRateWhenSaveRecorder>();
			AssertNull(service);

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var exRateFromAnotherFactory = factory2.Load<Accounting.Business.JobInvoicing.ExchangeRate>(exRate.PK);
			exRateFromAnotherFactory.JF_CFXMinimum = 1.9m;
			factory2.Save();

			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.DSBChargeCode, "", GlbCompany.CurrentCompany.LocalCurrency, debtor: GlbCompany.CurrentCompany.OrgProxy);
			charge2.JR_LocalSellAmt = 1m;
			charge2.JR_RX_NKSellCurrency = Constants.CurrencyCodes.UnitedStates;
			charge2.JR_RX_NKCostCurrency = Constants.CurrencyCodes.Egypt;
			var ex = AssertExceptionThrown<OnSavingCriticalCheckException>(() => Factory.Save());
			AssertEquals(nameof(CriticalValidationErrorType.JobChargeOSSellExRateIsNegative), ex.ErrorType);

			service = Factory.ServiceContainer.GetService<NegativeJobChargeOSSellExRateWhenSaveRecorder>();
			AssertNotNull(service);
			AssertEquals("Job Charge PK should be removed after throw Critical Validation", false, service.IsJobChargeExRateNegative(charge2.PK));
		}

		public void TestFactorySavingWithJobHeadersInDifferentCompanies()
		{
			var shipment = TestObjectCreator.CreateShipment("S001");
			var job1 = TestObjectCreator.CreateJob(shipment);
			var chargeCode = TestObjectCreator.CC1;
			TestObjectCreator.CreateCharge(job1, chargeCode, 100m, 100m);
			Factory.Save();

			var anotherCompany = Factory.NewWithValidTestData<GlbCompany>();
			var anotherBranch = Factory.NewWithValidTestData<GlbBranch>();
			anotherBranch.GB_GC = anotherCompany.PK;

			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.CompanyData.OB_IsDebtor = true;
			Factory.Save();

			Job job2;
			Charge job2Charge;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, anotherBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				job2 = TestObjectCreator.CreateJob(shipment);
				Factory.Save();

				var chargeCode2 = TestObjectCreator.CC2;
				job2Charge = TestObjectCreator.CreateCharge(job2, chargeCode2, debtor: debtor, osCostAmt: 200m, osSellAmt: 200m);
				Factory.Save();
			}

			//This sets ISalesRepDefaultingFromControllingCustomer.IsAllowedToDefaultSalesRepFromControllingCustomer = true
			using (FreightDataRegistry.Instance.ControllingCustomerUseSalesRep.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.ControllingCustomerUseSalesRep.SetTemporaryValue(anotherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var newFactoryForLoading = new BusinessObjectFactory();
				//This is necessary because of the logic in Job.GetControllingCustomerForSaleRepOrSubscribeToChange (line 1881)
				newFactoryForLoading.SetContext(BusinessContext.InvoicingPluginGUIExcludingConsol);

				shipment = newFactoryForLoading.Load<ForwardingShipment>(shipment.PK);
				//Need to load the local job in via the loader (the job will eventually be subscribed to the shipments notifications)
				Job job = new Job.Loader(shipment).Load(true);

				//This will load in the other job (for anotherCompany) without setting the shipment as the Parent
				new JobInvoicingDataAccessor(newFactoryForLoading).GetJobsFromShipment(new IJobHeaderParent[] { shipment }, restrictToThisCompany: false);
				newFactoryForLoading.Load<Charge>(job2Charge.PK);

				AssertNoExceptionThrown(() => newFactoryForLoading.Save());
			}
		}

		public void TestSellAccountIsOrgProxy()
		{
			var chargeInAnotherCompany = CreateChargeInNewFactory(GlbCompany.CurrentCompany.OrgProxy);

			var charge = Factory.NewWithValidTestData<BaseCharge>();
			charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
			AssertEquals("PreCondition", true, charge.SellAccountIsOrgProxy);

			var chargeInAnotherCompanyReload = Factory.Load<BaseCharge>(chargeInAnotherCompany.PK);
			AssertNotEquals("PreCondition", GlbCompany.CurrentCompany.PK, chargeInAnotherCompanyReload.JR_GC);
			AssertEquals("Since the JR_GC is not current company, the sell account should not be orgProxy."
				, false
				, chargeInAnotherCompanyReload.SellAccountIsOrgProxy);

			BaseCharge CreateChargeInNewFactory(OrgHeader orgHeader)
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
				{
					var newFactory = new BusinessObjectFactory();
					var charge = newFactory.NewWithValidTestData<BaseCharge>();
					charge.JR_OH_SellAccount = orgHeader.PK;
					newFactory.Save();

					AssertEquals("PreCondition", false, charge.SellAccountIsOrgProxy);
					return charge;
				}
			}
		}

		public void TestCostAccountIsOrgProxy()
		{
			var chargeInAnotherCompany = CreateChargeInNewFactory(GlbCompany.CurrentCompany.OrgProxy);

			var charge = Factory.NewWithValidTestData<BaseCharge>();
			charge.JR_OH_CostAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
			AssertEquals("PreCondition", true, charge.CostAccountIsOrgProxy);

			var chargeInAnotherCompanyReload = Factory.Load<BaseCharge>(chargeInAnotherCompany.PK);
			AssertNotEquals("PreCondition", GlbCompany.CurrentCompany.PK, chargeInAnotherCompanyReload.JR_GC);
			AssertEquals("Since the JR_GC is not current company, the cost account should not be orgProxy."
				, false
				, chargeInAnotherCompanyReload.CostAccountIsOrgProxy);

			BaseCharge CreateChargeInNewFactory(OrgHeader orgHeader)
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
				{
					var newFactory = new BusinessObjectFactory();
					var charge = newFactory.NewWithValidTestData<BaseCharge>();
					charge.JR_OH_CostAccount = orgHeader.PK;
					newFactory.Save();

					AssertEquals("PreCondition", false, charge.CostAccountIsOrgProxy);
					return charge;
				}
			}
		}

		#region Debtor Defaulting for Cross Trade job

		public void TestGetDefaultDebtorForCrossTradeJob()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var configs = new List<ICrossTradeDebtorDefaultingConfigurationItem>()
			{
				GetConfigurationItem(JobInvoicingConsumerTypes.ShipmentCode, Constants.TransportModes.Air, false, true, ChargedPartyForCrossTradeJob.LocalClient),
				GetConfigurationItem(JobInvoicingConsumerTypes.ShipmentCode, Constants.TransportModes.Sea, true, false, ChargedPartyForCrossTradeJob.Agent),
				GetConfigurationItem("ALL", Constants.TransportModes.All, true, false, ChargedPartyForCrossTradeJob.Agent)
			};

			var configProvider = new Mock<ICrossTradeDebtorDefaultingConfigurationProvider>(MockBehavior.Strict);
			configProvider.Setup(p => p.GetConfiguration()).Returns(configs);

			foreach (var isFunctionalityEnabled in new[] { true, false })
			{
				using (AccountingMasterFilesRegistry.Instance.EnableCrossTradeDebtorDefaultingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isFunctionalityEnabled))
				using (ObjectFactory.Substitute(configProvider.Object))
				{
					AssertDefaultDebtor(CreateCrossTradeSHPJob(objectCreator, "S0001" + isFunctionalityEnabled.ToYesNoString(), "AIR", paymentTerm: "CFR", localClient: objectCreator.Debtor, agent: objectCreator.Debtor1, null), expectedPrepaidCollect: Constants.PaymentType.Prepaid, expectedDefaultDebtorPK: objectCreator.Debtor.PK); /* LocalClient (When functionality is enabled),  Consignor (When functionality is disabled) */
					AssertDefaultDebtor(CreateCrossTradeSHPJob(objectCreator, "S0002" + isFunctionalityEnabled.ToYesNoString(), "SEA", paymentTerm: "FOB", localClient: objectCreator.Debtor, agent: objectCreator.Debtor1, null), expectedPrepaidCollect: Constants.PaymentType.Collect, expectedDefaultDebtorPK: (isFunctionalityEnabled ? objectCreator.Debtor1.PK : objectCreator.Debtor.PK));
				}
			}
		}

		public void TestGetDefaultDebtorForCrossTradeJobFromControllingCustomer()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var configs = new List<ICrossTradeDebtorDefaultingConfigurationItem>()
			{
				GetConfigurationItem(JobInvoicingConsumerTypes.ShipmentCode, Constants.TransportModes.Air, false, true, ChargedPartyForCrossTradeJob.ControllingCustomerFallingBackToAgent),
				GetConfigurationItem(JobInvoicingConsumerTypes.ShipmentCode, Constants.TransportModes.Sea, true, false, ChargedPartyForCrossTradeJob.ControllingCustomerFallingBackToLocalClient),
				GetConfigurationItem(JobInvoicingConsumerTypes.QuotedBookingCode, Constants.TransportModes.Air, false, true, ChargedPartyForCrossTradeJob.ControllingCustomerFallingBackToAgent),
				GetConfigurationItem(JobInvoicingConsumerTypes.QuotedBookingCode, Constants.TransportModes.Sea, true, false, ChargedPartyForCrossTradeJob.ControllingCustomerFallingBackToLocalClient),
				GetConfigurationItem("ALL", Constants.TransportModes.Air, false, true, ChargedPartyForCrossTradeJob.ControllingCustomerFallingBackToAgent),
				GetConfigurationItem("ALL", Constants.TransportModes.All, true, false, ChargedPartyForCrossTradeJob.ControllingCustomerFallingBackToLocalClient),
			};

			var controllingCustomerOrg = objectCreator.CreateOrgHeader("ControlDR", isCreditor: false, isDebtor: true, isAPGSTApplicable: false, isAPWHTApplicable: false, isARGSTApplicable: true, isARWHTApplicable: false);
			var controllingCustomerOrg1 = objectCreator.CreateOrgHeader("ControlDR1", isCreditor: false, isDebtor: true, isAPGSTApplicable: false, isAPWHTApplicable: false, isARGSTApplicable: true, isARWHTApplicable: false);
			var controllingCustomerOrg2 = objectCreator.CreateOrgHeader("ControlDR2", isCreditor: false, isDebtor: true, isAPGSTApplicable: false, isAPWHTApplicable: false, isARGSTApplicable: true, isARWHTApplicable: false);
			var iftPickupParty = objectCreator.CreateOrgHeader("iftPicDR", isCreditor: false, isDebtor: true, isAPGSTApplicable: false, isAPWHTApplicable: false, isARGSTApplicable: true, isARWHTApplicable: false);
			var iftDeliveryParty = objectCreator.CreateOrgHeader("iftDelDR", isCreditor: false, isDebtor: true, isAPGSTApplicable: false, isAPWHTApplicable: false, isARGSTApplicable: true, isARWHTApplicable: false);
			var iftPadParty = objectCreator.CreateOrgHeader("iftPadDR", isCreditor: false, isDebtor: true, isAPGSTApplicable: false, isAPWHTApplicable: false, isARGSTApplicable: true, isARWHTApplicable: false);
			var ictPickupParty = objectCreator.CreateOrgHeader("ictPicDR", isCreditor: false, isDebtor: true, isAPGSTApplicable: false, isAPWHTApplicable: false, isARGSTApplicable: true, isARWHTApplicable: false);
			var ictDeliveryParty = objectCreator.CreateOrgHeader("ictDelDR", isCreditor: false, isDebtor: true, isAPGSTApplicable: false, isAPWHTApplicable: false, isARGSTApplicable: true, isARWHTApplicable: false);

			controllingCustomerOrg.SetRelatedParty(iftPickupParty, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty);
			controllingCustomerOrg.SetRelatedParty(iftDeliveryParty, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.All, ZString.Empty);
			controllingCustomerOrg.SetRelatedParty(iftPadParty, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.PickupAndDelivery, Constants.TransportModes.All, ZString.Empty);
			controllingCustomerOrg2.SetRelatedParty(iftPadParty, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.PickupAndDelivery, Constants.TransportModes.All, ZString.Empty);
			controllingCustomerOrg.SetRelatedParty(ictPickupParty, RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty);
			controllingCustomerOrg.SetRelatedParty(ictDeliveryParty, RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.All, ZString.Empty);
			Factory.Save();

			var configProvider = new Mock<ICrossTradeDebtorDefaultingConfigurationProvider>(MockBehavior.Strict);
			configProvider.Setup(p => p.GetConfiguration()).Returns(configs);

			using (AccountingMasterFilesRegistry.Instance.EnableCrossTradeDebtorDefaultingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Substitute(configProvider.Object))
			{
				AssertDefaultDebtor(CreateCrossTradeSHPJob(objectCreator, "S0001", "AIR", paymentTerm: "CFR", localClient: objectCreator.Debtor, agent: objectCreator.Debtor1, controllingCustomer: null), expectedPrepaidCollect: Constants.PaymentType.Prepaid, expectedDefaultDebtorPK: objectCreator.Debtor1.PK); //falling back to agent
				AssertDefaultDebtor(CreateCrossTradeSHPJob(objectCreator, "S0002", "SEA", paymentTerm: "FOB", localClient: objectCreator.Debtor, agent: objectCreator.Debtor1, controllingCustomer: null), expectedPrepaidCollect: Constants.PaymentType.Collect, expectedDefaultDebtorPK: objectCreator.Debtor.PK); //falling back to local client
				AssertDefaultDebtor(CreateCrossTradeSHPJob(objectCreator, "S0003", "SEA", paymentTerm: "FOB", localClient: objectCreator.Debtor, agent: objectCreator.Debtor1, controllingCustomer: controllingCustomerOrg1), expectedPrepaidCollect: Constants.PaymentType.Collect, expectedDefaultDebtorPK: controllingCustomerOrg1.PK); //falling back to controlling customer as there is no IFT party
				AssertDefaultDebtor(CreateCrossTradeSHPJob(objectCreator, "S0004", "SEA", paymentTerm: "FOB", localClient: objectCreator.Debtor, agent: objectCreator.Debtor1, controllingCustomer: controllingCustomerOrg), expectedPrepaidCollect: Constants.PaymentType.Collect, expectedDefaultDebtorPK: iftPickupParty.PK); //IFT Pick Party for ControllingCustomerFallingBackToLocalClient
				AssertDefaultDebtor(CreateCrossTradeSHPJob(objectCreator, "S0005", "AIR", paymentTerm: "CFR", localClient: objectCreator.Debtor, agent: objectCreator.Debtor1, controllingCustomer: controllingCustomerOrg), expectedPrepaidCollect: Constants.PaymentType.Prepaid, expectedDefaultDebtorPK: iftDeliveryParty.PK); //IFT Delivery Party for ControllingCustomerFallingBackToAgent
				AssertDefaultDebtor(CreateCrossTradeSHPJob(objectCreator, "S0006", "AIR", paymentTerm: "CFR", localClient: objectCreator.Debtor, agent: objectCreator.Debtor1, controllingCustomer: controllingCustomerOrg2), expectedPrepaidCollect: Constants.PaymentType.Prepaid, expectedDefaultDebtorPK: iftPadParty.PK); //IFT PAD Party for ControllingCustomerFallingBackToAgent, as there is no IFT delivery party 
				AssertDefaultDebtor(CreateCrossTradeSHPJob(objectCreator, "S0007", "ROA", paymentTerm: "CFR", localClient: objectCreator.Debtor, agent: objectCreator.Debtor1, controllingCustomer: null), expectedPrepaidCollect: Constants.PaymentType.Prepaid, expectedDefaultDebtorPK: ZGuid.Empty); //No debtor, as there is no matching config for debtor defaulting
				AssertDefaultDebtor(CreateCrossTradeBRKJob(objectCreator, "SEA", paymentTerm: "FOB", localClient: objectCreator.Debtor, agent: objectCreator.Debtor1, controllingCustomer: controllingCustomerOrg), expectedPrepaidCollect: Constants.PaymentType.Collect, expectedDefaultDebtorPK: objectCreator.Debtor1.PK, false); //job direction is 'Export', so agent is set as default debtor
				AssertDefaultDebtor(CraeteCrossTradeQSHJob(objectCreator, "AIR", paymentTerm: "CFR", localClient: objectCreator.Debtor, agent: objectCreator.Debtor1, controllingCustomer: controllingCustomerOrg), expectedPrepaidCollect: Constants.PaymentType.Prepaid, expectedDefaultDebtorPK: iftDeliveryParty.PK); //IFT Delivery Party for ControllingCustomerFallingBackToAgent
				AssertDefaultDebtor(CraeteCrossTradeQSHJob(objectCreator, "AIR", paymentTerm: "CFR", localClient: objectCreator.Debtor, agent: objectCreator.Debtor1, controllingCustomer: null), expectedPrepaidCollect: Constants.PaymentType.Prepaid, expectedDefaultDebtorPK: objectCreator.Debtor1.PK); //falling back to agent
				AssertDefaultDebtor(CraeteCrossTradeQSHJob(objectCreator, "SEA", paymentTerm: "FOB", localClient: objectCreator.Debtor, agent: objectCreator.Debtor1, controllingCustomer: null), expectedPrepaidCollect: Constants.PaymentType.Collect, expectedDefaultDebtorPK: objectCreator.Debtor.PK); //falling back to local client
				AssertDefaultDebtor(CraeteCrossTradeQSHJob(objectCreator, "SEA", paymentTerm: "FOB", localClient: objectCreator.Debtor, agent: objectCreator.Debtor1, controllingCustomer: controllingCustomerOrg1), expectedPrepaidCollect: Constants.PaymentType.Collect, expectedDefaultDebtorPK: controllingCustomerOrg1.PK); //falling back to controlling customer as there is no IFT party
				AssertDefaultDebtor(CraeteCrossTradeQSHJob(objectCreator, "SEA", paymentTerm: "FOB", localClient: objectCreator.Debtor, agent: objectCreator.Debtor1, controllingCustomer: controllingCustomerOrg), expectedPrepaidCollect: Constants.PaymentType.Collect, expectedDefaultDebtorPK: iftPickupParty.PK); //IFT Pick Party for ControllingCustomerFallingBackToLocalClient
				AssertDefaultDebtor(CraeteCrossTradeQSHJob(objectCreator, "AIR", paymentTerm: "CFR", localClient: objectCreator.Debtor, agent: objectCreator.Debtor1, controllingCustomer: controllingCustomerOrg2), expectedPrepaidCollect: Constants.PaymentType.Prepaid, expectedDefaultDebtorPK: iftPadParty.PK);//IFT PAD Party for ControllingCustomerFallingBackToAgent, as there is no IFT delivery party
				AssertDefaultDebtor(CraeteCrossTradeQSHJob(objectCreator, "ROA", paymentTerm: "CFR", localClient: objectCreator.Debtor, agent: objectCreator.Debtor1, controllingCustomer: null), expectedPrepaidCollect: Constants.PaymentType.Prepaid, expectedDefaultDebtorPK: ZGuid.Empty);//No debtor, as there is no matching config for debtor defaulting
			}
		}

		Job CreateCrossTradeSHPJob(TestObjectCreator objectCreator, string shipmentNumber, string transportMode, string paymentTerm, OrgHeader localClient, OrgHeader agent, OrgHeader controllingCustomer)
		{
			var shipment = objectCreator.CreateShipment(shipmentNumber, origin: "USCHI", destination: "ITALL", transportMode: transportMode);
			shipment.JS_INCO = paymentTerm;
			var job = objectCreator.CreateJob(shipment, localClientOrg: localClient, 1.0M, agentOrg: agent, 1.0M);

			if (controllingCustomer != null)
			{
				shipment.ControllingCustomerAddress.E2_OA_Address = controllingCustomer.MainAddress.PK;
			}

			var charge = objectCreator.CreateCharge(job, objectCreator.CC1, 200M, 200M);
			charge.JR_AC = objectCreator.FRT.PK;

			Factory.Save();

			return job;
		}

		Job CreateCrossTradeBRKJob(TestObjectCreator objectCreator, string transportMode, string paymentTerm, OrgHeader localClient, OrgHeader agent, OrgHeader controllingCustomer)
		{
			var orgRelated = Factory.New<OrgHeader>();
			orgRelated.OH_Code = "DXZENTBNE";
			orgRelated.OH_FullName = "DXZ Enterprises";
			orgRelated.OH_IsForwarder = true;
			orgRelated.MainAddress.OA_Address1 = "1 Street St";

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.SetRelatedParty(orgRelated, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.Bulk, "USCHI");

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = transportMode;
			declaration.JE_ContainerMode = Constants.ContainerModes.Bulk;
			declaration.JE_RL_NKOrigin = "USCHI";
			declaration.JE_RL_NKFinalDestination = "ITALL";
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_ShipmentIncoTerm = paymentTerm;

			var job = objectCreator.CreateJob(declaration, localClientOrg: localClient, 1.0M, agentOrg: agent, 1.0M);

			if (controllingCustomer != null)
			{
				declaration.JE_OH_ControllingCustomer = controllingCustomer.PK;
			}

			var charge = objectCreator.CreateCharge(job, objectCreator.CC1, 200M, 200M);
			charge.JR_AC = objectCreator.FRT.PK;

			Factory.Save();

			return job;
		}

		Job CraeteCrossTradeQSHJob(TestObjectCreator objectCreator, string transportMode, string paymentTerm, OrgHeader localClient, OrgHeader agent, OrgHeader controllingCustomer)
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_INCO = paymentTerm;
			booking.JS_TransportMode = transportMode;
			booking.JS_RL_NKOrigin = "USCHI";
			booking.JS_RL_NKDestination = "ITALL";

			var job = objectCreator.CreateJob(booking, localClientOrg: localClient, 1.0M, agentOrg: agent, 1.0M);

			if (controllingCustomer != null)
			{
				booking.ControllingCustomerAddress.E2_OA_Address = controllingCustomer.MainAddress.PK;
			}

			var charge = objectCreator.CreateCharge(job, objectCreator.CC1, 200M, 200M);
			charge.JR_AC = objectCreator.FRT.PK;

			Factory.Save();

			return job;
		}

		void AssertDefaultDebtor(Job job, string expectedPrepaidCollect, ZGuid expectedDefaultDebtorPK, bool isCrossTrade = true)
		{
			//I did not use Job.IsCrossTrade here, as it produces a result which is inconsistent with job.GetInvoicingSupporter().IsCrossTrade.
			//I used job.GetInvoicingSupporter().IsCrossTrade, beacuse we use this to calculate direction while defaulting debtor. Please check JobHeader.GetDefaultDebtor to verify.
			AssertEquals("Is a cross trade job", isCrossTrade, job.GetInvoicingSupporter().IsCrossTrade);

			var charge = job.Charges[0];

			var prepaidCollect = job.GetInvoicingSupporter().PaymentTerm.GetPrepaidCollect(CostSell.Revenue, charge.ChargeCode.AC_ChargeGroup);
			AssertEquals("PrepaidCollect", expectedPrepaidCollect, prepaidCollect);

			var debtorPK = charge.JR_OH_SellAccount;
			AssertEquals("Default Debtor", expectedDefaultDebtorPK, debtorPK);
		}

		ICrossTradeDebtorDefaultingConfigurationItem GetConfigurationItem(ZString jobType, ZString transportMode, bool isCollect, bool isPrepaid, ChargedPartyForCrossTradeJob chargedParty)
		{
			var mockConfigItem = new Mock<ICrossTradeDebtorDefaultingConfigurationItem>(MockBehavior.Strict);
			mockConfigItem.Setup(c => c.JobTypeCode).Returns(jobType);
			mockConfigItem.Setup(c => c.TransportModeCode).Returns(transportMode);
			mockConfigItem.Setup(c => c.IsCollect).Returns(isCollect);
			mockConfigItem.Setup(c => c.IsPrepaid).Returns(isPrepaid);
			mockConfigItem.Setup(c => c.BillToParty).Returns(chargedParty);
			return mockConfigItem.Object;
		}
		#endregion

		#region AccChargeCreditorOverride Tests

		#region AccChargeCreditorOverride > Creditor Tests

		public void TestJR_OH_CostAccount_AccChargeCreditorOverride_Creditor_Shipment_NoConsol()
		{
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", paymentTerm: Core.Constants.PaymentType.Collect, creditor: TestObjectCreator.Creditor1.PK);
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", paymentTerm: Core.Constants.PaymentType.Prepaid, creditor: TestObjectCreator.Creditor2.PK);
			Factory.Save();
			var shipment = TestObjectCreator.CreateShipment("S001", origin: "AUSYD", destination: "USLAX");
			AssertCostAccount(shipment, agentOrg: TestObjectCreator.Agent, chargePK: TestObjectCreator.CC1.PK, expectedCostAccount: null);
		}

		public void TestJR_OH_CostAccount_AccChargeCreditorOverride_Creditor_Shipment_NoConsol_AccChargeCreditorOverrideHasAllPaymentTerm()
		{
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", paymentTerm: "ALL", creditor: TestObjectCreator.Creditor1.PK);
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", paymentTerm: Core.Constants.PaymentType.Collect, creditor: TestObjectCreator.Creditor2.PK);
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", paymentTerm: Core.Constants.PaymentType.Prepaid, creditor: TestObjectCreator.Creditor3.PK);
			Factory.Save();
			var shipment = TestObjectCreator.CreateShipment("S001", origin: "AUSYD", destination: "USLAX");
			AssertCostAccount(shipment, agentOrg: TestObjectCreator.Agent, chargePK: TestObjectCreator.CC1.PK, expectedCostAccount: TestObjectCreator.Creditor1.OH_Code);
		}

		public void TestJR_OH_CostAccount_AccChargeCreditorOverride_Creditor_Consol_AllPaymentTerm()
		{
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", paymentTerm: "ALL", creditor: TestObjectCreator.Creditor1.PK);
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", paymentTerm: Core.Constants.PaymentType.Collect, creditor: TestObjectCreator.Creditor2.PK);
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", paymentTerm: Core.Constants.PaymentType.Prepaid, creditor: TestObjectCreator.Creditor3.PK);
			Factory.Save();
			var shipment = TestObjectCreator.CreateShipment("S001", origin: "AUSYD", destination: "USLAX");
			var consol = shipment.Consols.AddNew();
			consol.JK_PrepaidCollect = string.Empty;
			AssertCostAccount(shipment, agentOrg: TestObjectCreator.Agent, chargePK: TestObjectCreator.CC1.PK, expectedCostAccount: TestObjectCreator.Creditor1.OH_Code);
		}

		public void TestJR_OH_CostAccount_AccChargeCreditorOverride_Creditor_Shipment_CollectPaymentTerm()
		{
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", paymentTerm: "ALL", creditor: TestObjectCreator.Creditor1.PK);
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", paymentTerm: Core.Constants.PaymentType.Collect, creditor: TestObjectCreator.Creditor2.PK);
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", paymentTerm: Core.Constants.PaymentType.Prepaid, creditor: TestObjectCreator.Creditor3.PK);
			Factory.Save();
			var shipment = TestObjectCreator.CreateShipment("S001", origin: "AUSYD", destination: "USLAX");
			var consol1 = shipment.Consols.AddNew();
			consol1.JK_PrepaidCollect = Core.Constants.PaymentType.Collect;
			AssertCostAccount(shipment, agentOrg: TestObjectCreator.Agent, chargePK: TestObjectCreator.CC1.PK, expectedCostAccount: TestObjectCreator.Creditor2.OH_Code);
		}

		public void TestJR_OH_CostAccount_AccChargeCreditorOverride_Creditor_Shipment_PrepaidPaymentTerm()
		{
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", paymentTerm: "ALL", creditor: TestObjectCreator.Creditor1.PK);
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", paymentTerm: Core.Constants.PaymentType.Collect, creditor: TestObjectCreator.Creditor2.PK);
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", paymentTerm: Core.Constants.PaymentType.Prepaid, creditor: TestObjectCreator.Creditor3.PK);
			Factory.Save();
			var shipment = TestObjectCreator.CreateShipment("S001", origin: "AUSYD", destination: "USLAX");
			var consol1 = shipment.Consols.AddNew();
			consol1.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			AssertCostAccount(shipment, agentOrg: TestObjectCreator.Agent, chargePK: TestObjectCreator.CC1.PK, expectedCostAccount: TestObjectCreator.Creditor3.OH_Code);
		}

		public void TestJR_OH_CostAccount_AccChargeCreditorOverride_Creditor_Shipment_MultipleConsols_SamePaymentTerm()
		{
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", creditor: TestObjectCreator.Creditor1.PK);
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", paymentTerm: Core.Constants.PaymentType.Collect, creditor: TestObjectCreator.Creditor2.PK);
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", paymentTerm: Core.Constants.PaymentType.Prepaid, creditor: TestObjectCreator.Creditor3.PK);
			Factory.Save();
			var shipment = TestObjectCreator.CreateShipment("S001", origin: "AUSYD", destination: "USLAX");
			var consol1 = shipment.Consols.AddNew();
			consol1.JK_PrepaidCollect = Core.Constants.PaymentType.Collect;
			var consol2 = shipment.Consols.AddNew();
			consol2.JK_PrepaidCollect = Core.Constants.PaymentType.Collect;
			AssertCostAccount(shipment, agentOrg: TestObjectCreator.Agent, chargePK: TestObjectCreator.CC1.PK, expectedCostAccount: TestObjectCreator.Creditor2.OH_Code);
		}

		public void TestJR_OH_CostAccount_AccChargeCreditorOverride_Creditor_Shipment_MultipleConsols_SamePaymentTerm_AccChargeCreditorOverrideHasAllPaymentTerm()
		{
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", creditor: TestObjectCreator.Creditor1.PK);
			Factory.Save();
			var shipment = TestObjectCreator.CreateShipment("S001", origin: "AUSYD", destination: "USLAX");
			var consol1 = shipment.Consols.AddNew();
			consol1.JK_PrepaidCollect = Core.Constants.PaymentType.Collect;
			var consol2 = shipment.Consols.AddNew();
			consol2.JK_PrepaidCollect = Core.Constants.PaymentType.Collect;
			AssertCostAccount(shipment, agentOrg: TestObjectCreator.Agent, chargePK: TestObjectCreator.CC1.PK, expectedCostAccount: TestObjectCreator.Creditor1.OH_Code);
		}

		public void TestJR_OH_CostAccount_AccChargeCreditorOverride_Creditor_Shipment_MultipleConsols_DifferentPaymentTerm()
		{
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", paymentTerm: Core.Constants.PaymentType.Collect, creditor: TestObjectCreator.Creditor2.PK);
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", paymentTerm: Core.Constants.PaymentType.Prepaid, creditor: TestObjectCreator.Creditor3.PK);
			Factory.Save();
			var shipment = TestObjectCreator.CreateShipment("S001", origin: "AUSYD", destination: "USLAX");
			var consol1 = shipment.Consols.AddNew();
			consol1.JK_PrepaidCollect = Core.Constants.PaymentType.Collect;
			var consol2 = shipment.Consols.AddNew();
			consol2.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			AssertCostAccount(shipment, agentOrg: TestObjectCreator.Agent, chargePK: TestObjectCreator.CC1.PK, expectedCostAccount: null);
		}

		public void TestJR_OH_CostAccount_AccChargeCreditorOverride_Creditor_Shipment_MultipleConsols_DifferentPaymentTerm_AccChargeCreditorOverrideHasAllPaymentTerm()
		{
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", paymentTerm: "ALL", creditor: TestObjectCreator.Creditor1.PK);
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", paymentTerm: Core.Constants.PaymentType.Collect, creditor: TestObjectCreator.Creditor2.PK);
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", paymentTerm: Core.Constants.PaymentType.Prepaid, creditor: TestObjectCreator.Creditor3.PK);
			Factory.Save();
			var shipment = TestObjectCreator.CreateShipment("S001", origin: "AUSYD", destination: "USLAX");
			var consol1 = shipment.Consols.AddNew();
			consol1.JK_PrepaidCollect = Core.Constants.PaymentType.Collect;
			var consol2 = shipment.Consols.AddNew();
			consol2.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			AssertCostAccount(shipment, agentOrg: TestObjectCreator.Agent, chargePK: TestObjectCreator.CC1.PK, expectedCostAccount: TestObjectCreator.Creditor1.OH_Code);
		}

		public void TestJR_OH_CostAccount_AccChargeCreditorOverride_Creditor_Shipment()
		{
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", creditor: TestObjectCreator.Creditor1.PK);
			Factory.Save();
			var shipment = TestObjectCreator.CreateShipment("S001", origin: "AUSYD", destination: "USLAX");
			var consol = shipment.Consols.AddNew();
			AssertCostAccount(shipment, agentOrg: TestObjectCreator.Agent, chargePK: TestObjectCreator.CC1.PK, expectedCostAccount: TestObjectCreator.Creditor1.OH_Code);
		}

		public void TestJR_OH_CostAccount_AccChargeCreditorOverride_Creditor_GatewayConsol()
		{
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.GatewayConsolCode, direction: "ALL", transportMode: "ALL", creditor: TestObjectCreator.Creditor1.PK);
			Factory.Save();
			var gatewayConsol = TestObjectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
			AssertCostAccount(gatewayConsol, agentOrg: TestObjectCreator.Agent, chargePK: TestObjectCreator.CC1.PK, expectedCostAccount: TestObjectCreator.Creditor1.OH_Code);
		}

		public void TestJR_OH_CostAccount_AccChargeCreditorOverride_Creditor_Consol()
		{
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.ForwardingConsolCode, direction: "ALL", transportMode: "ALL", creditor: TestObjectCreator.Creditor1.PK);
			Factory.Save();

			var consol = TestObjectCreator.CreateConsol();
			var job = Factory.NewJobForTesting<Job>();
			job.Parent = consol;
			AssertCostAccount(consol, agentOrg: TestObjectCreator.Agent, chargePK: TestObjectCreator.CC1.PK, expectedCostAccount: TestObjectCreator.Creditor1.OH_Code);
		}

		public void TestJR_OH_CostAccount_AccChargeCreditorOverride_Creditor_WorkItem()
		{
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.WorkItemCode, direction: "ALL", transportMode: "ALL", creditor: TestObjectCreator.Creditor1.PK);
			Factory.Save();

			var workItem = Factory.NewWithValidTestData<WorkItem>();
			var job = Factory.NewJobForTesting<Job>();
			job.Parent = workItem;
			AssertCostAccount(workItem, agentOrg: TestObjectCreator.Agent, chargePK: TestObjectCreator.CC1.PK, expectedCostAccount: TestObjectCreator.Creditor1.OH_Code);
		}

		#endregion

		#region AccChargeCreditorOverride > Creditor Role

		public void TestJR_OH_CostAccount_AccChargeCreditorOverride_CreditorRole_Shipment_Import()
		{
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "IMP", transportMode: "ALL", creditorRole: "OSA");
			Factory.Save();
			var shipment = TestObjectCreator.CreateShipment(shipmentNum: "SHP1", origin: "USLAX", destination: "AUSYD");
			var consol = shipment.Consols.AddNew();
			AssertCostAccount(shipment, agentOrg: TestObjectCreator.Agent, chargePK: TestObjectCreator.CC1.PK, expectedCostAccount: TestObjectCreator.Agent.OH_Code);
		}

		public void TestJR_OH_CostAccount_AccChargeCreditorOverride_CreditorRole_Shipment_Export()
		{
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "EXP", transportMode: "ALL", creditorRole: "OSA");
			Factory.Save();
			var shipment = TestObjectCreator.CreateShipment(shipmentNum: "SHP1", origin: "AUSYD", destination: "USLAX");
			var consol = shipment.Consols.AddNew();
			AssertCostAccount(shipment, agentOrg: TestObjectCreator.Agent, chargePK: TestObjectCreator.CC1.PK, expectedCostAccount: TestObjectCreator.Agent.OH_Code);
		}

		public void TestJR_OH_CostAccount_AccChargeCreditorOverride_CreditorRole_Consol_Import()
		{
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.ForwardingConsolCode, direction: "IMP", transportMode: "ALL", creditorRole: "OSA");
			Factory.Save();

			var consol = TestObjectCreator.CreateConsol(origin: "USLAX", destination: "AUSYD", consolNum: "CONSOL-IMPORT");
			consol.JK_OA_SendingForwarderAddress = TestObjectCreator.Creditor3.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = TestObjectCreator.Creditor4.MainAddress.PK;
			var job = Factory.NewJobForTesting<Job>();
			job.Parent = consol;

			AssertEquals("ImportConsol SendingForwarder", TestObjectCreator.Creditor3.OH_Code, consol.SendingForwarder.OH_Code);
			AssertCostAccount(consol, agentOrg: TestObjectCreator.Agent, chargePK: TestObjectCreator.CC1.PK, expectedCostAccount: TestObjectCreator.Creditor3.OH_Code);
		}

		public void TestJR_OH_CostAccount_AccChargeCreditorOverride_CreditorRole_Consol_Export()
		{
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.ForwardingConsolCode, direction: "EXP", transportMode: "ALL", creditorRole: "OSA");
			Factory.Save();

			var consol = TestObjectCreator.CreateConsol(origin: "AUSYD", destination: "USLAX", consolNum: "CONSOL-EXPORT");
			consol.JK_OA_SendingForwarderAddress = TestObjectCreator.Creditor1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = TestObjectCreator.Creditor2.MainAddress.PK;

			var job = Factory.NewJobForTesting<Job>();
			job.Parent = consol;

			AssertEquals("ExportConsol ReceivingForwarder", TestObjectCreator.Creditor2.OH_Code, consol.ReceivingForwarder.OH_Code);
			AssertCostAccount(consol, agentOrg: TestObjectCreator.Agent, chargePK: TestObjectCreator.CC1.PK, expectedCostAccount: TestObjectCreator.Creditor2.OH_Code);
		}

		public void TestJR_OH_CostAccount_AccChargeCreditorOverride_CreditorRole_GatewayConsol_Import()
		{
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.GatewayConsolCode, direction: "IMP", transportMode: "ALL", creditorRole: "OSA");
			Factory.Save();

			var consol = TestObjectCreator.CreateGatewayConsol(origin: "USLAX", destination: "AUSYD", sendingGatewayCompany: TestObjectCreator.NonCurrentCompany, receivingGatewayCompany: GlbCompany.CurrentCompany);
			AssertCostAccount(consol, agentOrg: TestObjectCreator.Agent, chargePK: TestObjectCreator.CC1.PK, expectedCostAccount: consol.SendingForwarder.OH_Code);
		}

		public void TestJR_OH_CostAccount_AccChargeCreditorOverride_CreditorRole_GatewayConsol_Export()
		{
			TestObjectCreator.CreateChargeCreditorOverride(TestObjectCreator.CC1, JobInvoicingConsumerTypes.GatewayConsolCode, direction: "EXP", transportMode: "ALL", creditorRole: "OSA");
			Factory.Save();

			var consol = TestObjectCreator.CreateGatewayConsol(origin: "AUSYD", destination: "USLAX", sendingGatewayCompany: GlbCompany.CurrentCompany, receivingGatewayCompany: TestObjectCreator.NonCurrentCompany);
			AssertCostAccount(consol, agentOrg: TestObjectCreator.Agent, chargePK: TestObjectCreator.CC1.PK, expectedCostAccount: consol.ReceivingForwarder.OH_Code);
		}

		void AssertCostAccount(IJobInvoicingPlugIn jobInvoicingPlugIn, OrgHeader agentOrg, ZGuid chargePK, string expectedCostAccount)
		{
			using (var job = TestObjectCreator.CreateJob(jobInvoicingPlugIn, agentOrg: agentOrg))
			{
				var charge = job.Charges.AddNew();
				charge.JR_AC = chargePK;
				AssertEquals("CostAccount", expectedCostAccount, charge.CostAccount?.OH_Code);
			}
		}

		#endregion

		#endregion

		#region Test HTML Properties

		public void TestHtmlProperty()
		{
			var baseCharge = Factory.NewWithValidTestData<BaseCharge>();
			AssertEquals(ZBlob.Empty, baseCharge.RevenueCalculationDescription);
			AssertEquals(ZBlob.Empty, baseCharge.RevenueCalculationDescription_HTML);
			AssertEquals(ZBlob.Empty, baseCharge.CostCalculationDescription);
			AssertEquals(ZBlob.Empty, baseCharge.CostCalculationDescription_HTML);

			baseCharge.RevenueCalculationDescription_HTML = ZBlob.FromUTF8("<p>123</p>");
			baseCharge.CostCalculationDescription_HTML = ZBlob.FromUTF8("<p>123</p>");

			AssertEquals(@"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl}{\colortbl}{{123}\par}}", ORtfTextUtil.GeneratorInfoRegex.Replace(baseCharge.RevenueCalculationDescription.ToUTF8(), string.Empty));
			AssertEquals("<p>123</p>", baseCharge.RevenueCalculationDescription_HTML.ToUTF8());
			AssertEquals(@"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl}{\colortbl}{{123}\par}}", ORtfTextUtil.GeneratorInfoRegex.Replace(baseCharge.CostCalculationDescription.ToUTF8(), string.Empty));
			AssertEquals("<p>123</p>", baseCharge.CostCalculationDescription_HTML.ToUTF8());
		}

		public void TestHtmlFromTextProperty()
		{
			var baseCharge = Factory.NewWithValidTestData<BaseCharge>();
			AssertEquals(ZBlob.Empty, baseCharge.RevenueCalculationDescription);
			AssertEquals(ZBlob.Empty, baseCharge.RevenueCalculationDescription_HTML);
			AssertEquals(ZBlob.Empty, baseCharge.CostCalculationDescription);
			AssertEquals(ZBlob.Empty, baseCharge.CostCalculationDescription_HTML);

			baseCharge.RevenueCalculationDescription = ZBlob.FromUTF8("1234\r\n5678");
			baseCharge.CostCalculationDescription = ZBlob.FromUTF8("1234\r\n5678");

			AssertEquals("<p>1234</p><p>5678</p>", baseCharge.RevenueCalculationDescription_HTML.ToUTF8());
			AssertEquals("<p>1234</p><p>5678</p>", baseCharge.CostCalculationDescription_HTML.ToUTF8());

			baseCharge.RevenueCalculationDescription = ZBlob.FromUTF8("{\\rtf1\\test\\ansi\\ansicpg1252\\nouicompat\\deflang3081\r\n{\\*\\generator Riched20 10.0.19041}\\viewkind4\\uc1 \\pard rtf\\par\r\n}\r\n");
			baseCharge.CostCalculationDescription = ZBlob.FromUTF8("{\\rtf1\\test\\ansi\\ansicpg1252\\nouicompat\\deflang3081\r\n{\\*\\generator Riched20 10.0.19041}\\viewkind4\\uc1 \\pard rtf\\par\r\n}\r\n");

			AssertEquals("<p>rtf</p>", baseCharge.RevenueCalculationDescription_HTML.ToUTF8());
			AssertEquals("<p>rtf</p>", baseCharge.CostCalculationDescription_HTML.ToUTF8());
		}

		#endregion

		public void TestCostCalculationDescriptionString()
		{
			var cost = Factory.New<BaseCharge>();
			cost.JR_AC = TestObjectCreator.CC1.PK;
			var description = "abcdefg!@#$";
			cost.CostCalculationDescription = ZBlob.FromUTF8(description);
			AssertEquals(description, cost.CostCalculationDescriptionString);
		}

		public void TestRevenueCalculationDescriptionString()
		{
			var cost = Factory.New<BaseCharge>();
			cost.JR_AC = TestObjectCreator.CC1.PK;
			var description = "abcdefg!@#$";
			cost.RevenueCalculationDescription = ZBlob.FromUTF8(description);
			AssertEquals(description, cost.RevenueCalculationDescriptionString);
		}

		public void TestCostCalculationDescriptionString_WithRTFDescription()
		{
			var cost = Factory.New<BaseCharge>();
			cost.JR_AC = TestObjectCreator.CC1.PK;
			var description = "abcdefg!@#$";
			cost.CostCalculationDescription = ZBlob.FromUTF8(new FormattedRtfString().Add(description, System.Drawing.FontStyle.Bold).ToString());
			AssertEquals(description, cost.CostCalculationDescriptionString);
		}

		public void TestRevenueCalculationDescriptionString_WithRTFDescription()
		{
			var cost = Factory.New<BaseCharge>();
			cost.JR_AC = TestObjectCreator.CC1.PK;
			var description = "abcdefg!@#$";
			cost.RevenueCalculationDescription = ZBlob.FromUTF8(new FormattedRtfString().Add(description, System.Drawing.FontStyle.Bold).ToString());
			AssertEquals(description, cost.RevenueCalculationDescriptionString);
		}

		#region Implementation

		protected TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		#endregion
	}
}
