using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(Charge))]
	public class ChargeTest : ChargeWithCostTest
	{
		public interface ITestChargeCreditorDefaulting : IJobHeaderParent, IChargeCreditorDefaulting { }

		public void TestCostAccount_WhenSetChargeCodeAndCreditorForNewAutoRated()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			using var job = TestObjectCreator.CreateJob(shipment);

			var mockParent = new Mock<ITestChargeCreditorDefaulting>();
			var oldParentForDisposing = job.Parent;
			job.Parent = mockParent.Object;

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;
			mockParent.Setup(x => x.DefaultCreditorPK).Returns(creditor.PK);
			Factory.SetContext(BusinessContext.AutoRating);

			var charge = job.Charges.AddNew();
			charge.JR_OH_CostAccount = ZGuid.Empty;
			var chargeCode = TestObjectCreator.CC1;
			
			charge.JR_AC = chargeCode.PK;
			charge.SetChargeCodeAndCreditorForNewAutoRated(chargeCode, CostSell.Revenue, true, ZGuid.Empty, ZGuid.Empty, true);
			AssertEquals("when JR_AC = chargeCode.PK, and autorating revenue, JR_AC doesn't change", chargeCode.PK, charge.JR_AC);
			AssertEquals("when JR_AC = chargeCode.PK, and autorating revenue, JR_OH_CostAccount doesn't change, stay blank", ZGuid.Empty, charge.JR_OH_CostAccount);

			charge.SetChargeCodeAndCreditorForNewAutoRated(chargeCode, CostSell.Cost, true, ZGuid.Empty, ZGuid.Empty, true);
			AssertEquals("when JR_AC = chargeCode.PK, and autorating cost, JR_AC doesn't change", chargeCode.PK, charge.JR_AC);
			AssertEquals("when JR_AC = chargeCode.PK, and autorating cost, JR_OH_CostAccount doesn't change, stay blank", ZGuid.Empty, charge.JR_OH_CostAccount);

			charge.JR_AC = ZGuid.Empty;
			charge.SetChargeCodeAndCreditorForNewAutoRated(chargeCode, CostSell.Revenue, true, ZGuid.Empty, ZGuid.Empty, true);
			AssertEquals("when JR_AC != chargeCode.PK, and autorating revenue, set JR_AC to chargeCode.PK", chargeCode.PK, charge.JR_AC);
			AssertEquals("when JR_AC != chargeCode.PK, and autorating revenue, JR_OH_CostAccount doesn't change, stay blank", ZGuid.Empty, charge.JR_OH_CostAccount);

			charge.JR_AC = ZGuid.Empty;
			charge.SetChargeCodeAndCreditorForNewAutoRated(chargeCode, CostSell.Cost, true, ZGuid.Empty, ZGuid.Empty, true);
			AssertEquals("when JR_AC != chargeCode.PK, and autorating cost, set JR_AC to chargeCode.PK", chargeCode.PK, charge.JR_AC);
			AssertEquals("when JR_AC != chargeCode.PK and JR_OH_CostAccount is empty, and autorating cost, automatically set JR_OH_CostAccount", creditor.PK, charge.JR_OH_CostAccount);

			charge.JR_AC = ZGuid.Empty;
			ZGuid randomGuid = new ZGuid("7393E243-A801-44AA-8809-B8C637F69765");
			charge.JR_OH_CostAccount = randomGuid;
			charge.SetChargeCodeAndCreditorForNewAutoRated(chargeCode, CostSell.Cost, true, ZGuid.Empty, ZGuid.Empty, true);
			AssertEquals("when JR_AC != chargeCode.PK and JR_OH_CostAccount is not empty, autorating cost, JR_OH_CostAccount doesn't change", randomGuid, charge.JR_OH_CostAccount);

			creditor.OH_IsCreditor = false;
			charge.JR_AC = ZGuid.Empty;
			charge.JR_OH_CostAccount = ZGuid.Empty;
			charge.SetChargeCodeAndCreditorForNewAutoRated(chargeCode, CostSell.Cost, true, ZGuid.Empty, ZGuid.Empty, true);
			AssertEquals("when the creditor is invalid, and autorating cost, leave JR_OH_CostAccount blank", ZGuid.Empty, charge.JR_OH_CostAccount);

			job.Parent = oldParentForDisposing;
		}

		public void TestRatingOverrideComment_ShouldSupportUnicode()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment);

			var charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OSCostAmt = 10m;
			charge.JR_OH_CostAccount = AALSHI.PK;
			charge.JR_CostRatingOverrideComment = "成本";

			charge.JR_OSSellAmt = 20m;
			charge.JR_OH_SellAccount = ABIGAS.PK;
			charge.JR_SellRatingOverrideComment = "卖";

			AssertNoErrors("JR_CostRatingOverrideComment", charge.JR_CostRatingOverrideCommentInfo);
			AssertNoErrors("JR_SellRatingOverrideComment", charge.JR_SellRatingOverrideCommentInfo);

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var charge2 = factory2.Load<Charge>(charge.PK);
			AssertEquals("JR_CostRatingOverrideComment", "成本", charge2.JR_CostRatingOverrideComment);
			AssertEquals("JR_SellRatingOverrideComment", "卖", charge2.JR_SellRatingOverrideComment);
		}

		public void TestRatingOverrideComment_ShouldAllow50Characters()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment);

			var charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OSCostAmt = 10m;
			charge.JR_OH_CostAccount = AALSHI.PK;
			charge.JR_CostRatingOverrideComment = "ABCDEFGHIJABCDEFGHIJABCDEFGHIJABCDEFGHIJABCDEFGHIJ";

			charge.JR_OSSellAmt = 20m;
			charge.JR_OH_SellAccount = ABIGAS.PK;
			charge.JR_SellRatingOverrideComment = "ABCDEFGHIJABCDEFGHIJABCDEFGHIJABCDEFGHIJABCDEFGHIJ";

			AssertNoErrors("JR_CostRatingOverrideComment", charge.JR_CostRatingOverrideCommentInfo);
			AssertNoErrors("JR_SellRatingOverrideComment", charge.JR_SellRatingOverrideCommentInfo);

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var charge2 = factory2.Load<Charge>(charge.PK);
			AssertEquals("JR_CostRatingOverrideComment", "ABCDEFGHIJABCDEFGHIJABCDEFGHIJABCDEFGHIJABCDEFGHIJ", charge2.JR_CostRatingOverrideComment);
			AssertEquals("JR_SellRatingOverrideComment", "ABCDEFGHIJABCDEFGHIJABCDEFGHIJABCDEFGHIJABCDEFGHIJ", charge2.JR_SellRatingOverrideComment);
		}

		#region Cash Advance Defaulting

		public void TestCashAdvanceFlagIsDefaultedOnlyWhenCashAdvanceFunctionalityIsEnabled()
		{
			var debtor = TestObjectCreator.Debtor;
			RemoveAndDeleteAllConfigurationsForTest(debtor);
			CreateCashAdvanceJobConfigurationForCompany(GlbCompany.CurrentCompany, "ALL", LedgerTypes.AccountsReceivable, Constants.TransportModes.All, Constants.FreightShipmentDirection.Code.All, CashAdvanceDefaultingOption.All);
			GlbCompany.CurrentCompany.Factory.Save();

			var importAirShipment = TestObjectCreator.CreateShipment("S00001001", true);
			importAirShipment.JS_RL_NKOrigin = "SGSIN";
			importAirShipment.JS_RL_NKDestination = "AUBNE";
			importAirShipment.JS_TransportMode = Constants.TransportModes.Air;

			using (var job = TestObjectCreator.CreateJob(importAirShipment))
			{
				using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 10m, 10m);
					charge.JR_OH_SellAccount = debtor.PK;
					Assert("IsARCashAdvance Flag not defaulted because EnableReceivablesCashAdvanceFunctionality registry is disabled.", !charge.JR_IsARCashAdvance);
				}

				using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 20m, 20m);
					charge.JR_OH_SellAccount = debtor.PK;
					Assert("IsARCashAdvance Flag defaulted because EnableReceivablesCashAdvanceFunctionality registry is enabled.", charge.JR_IsARCashAdvance);
				}
			}
		}

		public void TestCashAdvanceFlagIsDefaultedWhenSpecificConfigurationAvailableAtOrganizationLevel()
		{
			var debtor = TestObjectCreator.Debtor;
			RemoveAndDeleteAllConfigurationsForTest(debtor);
			CreateCashAdvanceJobConfigurationForCompany(GlbCompany.CurrentCompany, "ALL", LedgerTypes.AccountsReceivable, Constants.TransportModes.All, Constants.FreightShipmentDirection.Code.All, CashAdvanceDefaultingOption.None);
			CreateCashAdvanceJobConfigurationForCompany(GlbCompany.CurrentCompany, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsReceivable, Constants.TransportModes.Air, Constants.FreightShipmentDirection.Code.Import, CashAdvanceDefaultingOption.None);
			GlbCompany.CurrentCompany.Factory.Save();
			CreateCashAdvanceJobConfigurationForOrganization(debtor, "ALL", LedgerTypes.AccountsReceivable, Constants.TransportModes.All, Constants.FreightShipmentDirection.Code.All, CashAdvanceDefaultingOption.None);
			CreateCashAdvanceJobConfigurationForOrganization(debtor, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsReceivable, Constants.TransportModes.Air, Constants.FreightShipmentDirection.Code.Import, CashAdvanceDefaultingOption.All);
			Factory.Save();

			var importAirShipment = TestObjectCreator.CreateShipment("S00001001", true);
			importAirShipment.JS_RL_NKOrigin = "SGSIN";
			importAirShipment.JS_RL_NKDestination = "AUBNE";
			importAirShipment.JS_TransportMode = Constants.TransportModes.Air;
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var job = TestObjectCreator.CreateJob(importAirShipment))
			{
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 10m, 10m);
				charge.JR_OH_SellAccount = debtor.PK;
				Assert("IsARCashAdvance Flag ticked because defaulting option of matching specifc config on organization is ALL.", charge.JR_IsARCashAdvance);
			}
		}

		public void TestCashAdvanceFlagIsDefaultedWhenGenericConfigurationAvailableAtOrganizationLevel()
		{
			var debtor = TestObjectCreator.Debtor;
			RemoveAndDeleteAllConfigurationsForTest(debtor);
			CreateCashAdvanceJobConfigurationForCompany(GlbCompany.CurrentCompany, "ALL", LedgerTypes.AccountsReceivable, Constants.TransportModes.All, Constants.FreightShipmentDirection.Code.All, CashAdvanceDefaultingOption.None);
			CreateCashAdvanceJobConfigurationForCompany(GlbCompany.CurrentCompany, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsReceivable, Constants.TransportModes.Air, Constants.FreightShipmentDirection.Code.Import, CashAdvanceDefaultingOption.None);
			GlbCompany.CurrentCompany.Factory.Save();
			CreateCashAdvanceJobConfigurationForOrganization(debtor, "ALL", LedgerTypes.AccountsReceivable, Constants.TransportModes.All, Constants.FreightShipmentDirection.Code.All, CashAdvanceDefaultingOption.All);
			CreateCashAdvanceJobConfigurationForOrganization(debtor, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsReceivable, Constants.TransportModes.Sea, Constants.FreightShipmentDirection.Code.Import, CashAdvanceDefaultingOption.None);
			Factory.Save();

			var importAirShipment = TestObjectCreator.CreateShipment("S00001001", true);
			importAirShipment.JS_RL_NKOrigin = "SGSIN";
			importAirShipment.JS_RL_NKDestination = "AUBNE";
			importAirShipment.JS_TransportMode = Constants.TransportModes.Air;
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var job = TestObjectCreator.CreateJob(importAirShipment))
			{
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 10m, 10m);
				charge.JR_OH_SellAccount = debtor.PK;
				Assert("IsARCashAdvance Flag ticked because defaulting option of matching generic config on organization is ALL.", charge.JR_IsARCashAdvance);
			}
		}

		public void TestCashAdvanceFlagIsDefaultedWhenSpecificConfigurationAvailableAtCompanyLevel()
		{
			var debtor = TestObjectCreator.Debtor;
			RemoveAndDeleteAllConfigurationsForTest(debtor);
			CreateCashAdvanceJobConfigurationForCompany(GlbCompany.CurrentCompany, "ALL", LedgerTypes.AccountsReceivable, Constants.TransportModes.All, Constants.FreightShipmentDirection.Code.All, CashAdvanceDefaultingOption.None);
			CreateCashAdvanceJobConfigurationForCompany(GlbCompany.CurrentCompany, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsReceivable, Constants.TransportModes.Air, Constants.FreightShipmentDirection.Code.Import, CashAdvanceDefaultingOption.All);
			GlbCompany.CurrentCompany.Factory.Save();
			CreateCashAdvanceJobConfigurationForOrganization(debtor, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsReceivable, Constants.TransportModes.Sea, Constants.FreightShipmentDirection.Code.Import, CashAdvanceDefaultingOption.None);
			Factory.Save();

			var importAirShipment = TestObjectCreator.CreateShipment("S00001001", true);
			importAirShipment.JS_RL_NKOrigin = "SGSIN";
			importAirShipment.JS_RL_NKDestination = "AUBNE";
			importAirShipment.JS_TransportMode = Constants.TransportModes.Air;
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var job = TestObjectCreator.CreateJob(importAirShipment))
			{
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 10m, 10m);
				charge.JR_OH_SellAccount = debtor.PK;
				Assert("IsARCashAdvance Flag ticked because defaulting option of matching specific config on company is ALL.", charge.JR_IsARCashAdvance);
			}
		}

		public void TestCashAdvanceFlagIsDefaultedWhenGenericConfigurationAvailableAtCompanyLevel()
		{
			var debtor = TestObjectCreator.Debtor;
			RemoveAndDeleteAllConfigurationsForTest(debtor);
			CreateCashAdvanceJobConfigurationForCompany(GlbCompany.CurrentCompany, "ALL", LedgerTypes.AccountsReceivable, Constants.TransportModes.All, Constants.FreightShipmentDirection.Code.All, CashAdvanceDefaultingOption.All);
			CreateCashAdvanceJobConfigurationForCompany(GlbCompany.CurrentCompany, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsReceivable, Constants.TransportModes.Sea, Constants.FreightShipmentDirection.Code.Import, CashAdvanceDefaultingOption.None);
			GlbCompany.CurrentCompany.Factory.Save();
			CreateCashAdvanceJobConfigurationForOrganization(debtor, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsReceivable, Constants.TransportModes.Sea, Constants.FreightShipmentDirection.Code.Import, CashAdvanceDefaultingOption.None);
			Factory.Save();

			var importAirShipment = TestObjectCreator.CreateShipment("S00001001", true);
			importAirShipment.JS_RL_NKOrigin = "SGSIN";
			importAirShipment.JS_RL_NKDestination = "AUBNE";
			importAirShipment.JS_TransportMode = Constants.TransportModes.Air;
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var job = TestObjectCreator.CreateJob(importAirShipment))
			{
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 10m, 10m);
				Assert("IsARCashAdvance Flag not ticked yet.", !charge.JR_IsARCashAdvance);
				charge.JR_OH_SellAccount = debtor.PK;
				Assert("IsARCashAdvance Flag ticked because defaulting option of matching generic config on company is ALL.", charge.JR_IsARCashAdvance);
			}
		}

		public void TestCashAdvanceFlagIsDefaultedWhenMatchingChargeCodeIsAvailableOnJobConfig()
		{
			var debtor = TestObjectCreator.Debtor;
			RemoveAndDeleteAllConfigurationsForTest(debtor);
			CreateCashAdvanceJobConfigurationForOrganization(debtor, "ALL", LedgerTypes.AccountsReceivable, Constants.TransportModes.All, Constants.FreightShipmentDirection.Code.All, CashAdvanceDefaultingOption.None);
			var configToUse = CreateCashAdvanceJobConfigurationForOrganization(debtor, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsReceivable, Constants.TransportModes.Air, Constants.FreightShipmentDirection.Code.Import, CashAdvanceDefaultingOption.SelectedChargeCodesAndGroups);
			configToUse.ChargeCodes.RemoveAndDeleteAll();
			var selectedChargeCode1 = configToUse.ChargeCodes.AddNew();
			selectedChargeCode1.JCT_ParentId = TestObjectCreator.CC1.PK;
			Factory.Save();

			var importAirShipment = TestObjectCreator.CreateShipment("S00001001", true);
			importAirShipment.JS_RL_NKOrigin = "SGSIN";
			importAirShipment.JS_RL_NKDestination = "AUBNE";
			importAirShipment.JS_TransportMode = Constants.TransportModes.Air;
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var job = TestObjectCreator.CreateJob(importAirShipment))
			{
				var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, 10m, 10m);
				charge1.JR_OH_SellAccount = debtor.PK;
				Assert("IsARCashAdvance Flag not ticked because matching config does not have CC2 as selected charge code.", !charge1.JR_IsARCashAdvance);
				var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 10m, 10m);
				charge2.JR_OH_SellAccount = debtor.PK;
				Assert("IsARCashAdvance Flag ticked because matching config has CC1 as selected charge code.", charge2.JR_IsARCashAdvance);
			}
		}

		public void TestCashAdvanceFlagIsDefaultedWhenMatchingChargeGroupIsAvailableOnJobConfig()
		{
			var debtor = TestObjectCreator.Debtor;
			RemoveAndDeleteAllConfigurationsForTest(debtor);
			CreateCashAdvanceJobConfigurationForOrganization(debtor, "ALL", LedgerTypes.AccountsReceivable, Constants.TransportModes.All, Constants.FreightShipmentDirection.Code.All, CashAdvanceDefaultingOption.None);
			var configToUse = CreateCashAdvanceJobConfigurationForOrganization(debtor, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsReceivable, Constants.TransportModes.Air, Constants.FreightShipmentDirection.Code.Import, CashAdvanceDefaultingOption.SelectedChargeCodesAndGroups);
			configToUse.ChargeGroups.RemoveAndDeleteAll();
			var selectedChargeGroup = configToUse.ChargeGroups.AddNew();
			selectedChargeGroup.JCT_Code = ChargeCodeGroupList.Codes.Freight;
			var freightChargeCode = TestObjectCreator.CreateChargeCode("TC1");
			freightChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			var brokerageChargeCode = TestObjectCreator.CreateChargeCode("TC2");
			brokerageChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Brokerage;
			Factory.Save();

			var importAirShipment = TestObjectCreator.CreateShipment("S00001001", true);
			importAirShipment.JS_RL_NKOrigin = "SGSIN";
			importAirShipment.JS_RL_NKDestination = "AUBNE";
			importAirShipment.JS_TransportMode = Constants.TransportModes.Air;
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var job = TestObjectCreator.CreateJob(importAirShipment))
			{
				var charge1 = TestObjectCreator.CreateCharge(job, brokerageChargeCode, 10m, 10m);
				charge1.JR_OH_SellAccount = debtor.PK;
				Assert("IsARCashAdvance Flag not ticked because matching config does not have 'Brokerage' as selected charge group.", !charge1.JR_IsARCashAdvance);
				var charge2 = TestObjectCreator.CreateCharge(job, freightChargeCode, 10m, 10m);
				charge2.JR_OH_SellAccount = debtor.PK;
				Assert("IsARCashAdvance Flag ticked because matching config has 'Freight' as selected charge group.", charge2.JR_IsARCashAdvance);
			}
		}

		public void TestCashAdvanceFlagIsDefaultedWhenChargeDebtorChangeFromEmptyToNonEmpty()
		{
			var debtor = TestObjectCreator.Debtor;
			RemoveAndDeleteAllConfigurationsForTest(debtor);
			CreateCashAdvanceJobConfigurationForOrganization(debtor, "ALL", LedgerTypes.AccountsReceivable, Constants.TransportModes.All, Constants.FreightShipmentDirection.Code.All, CashAdvanceDefaultingOption.None);
			CreateCashAdvanceJobConfigurationForOrganization(debtor, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsReceivable, Constants.TransportModes.Air, Constants.FreightShipmentDirection.Code.Import, CashAdvanceDefaultingOption.All);
			Factory.Save();

			var importAirShipment = TestObjectCreator.CreateShipment("S00001001", true);
			importAirShipment.JS_RL_NKOrigin = "SGSIN";
			importAirShipment.JS_RL_NKDestination = "AUBNE";
			importAirShipment.JS_TransportMode = Constants.TransportModes.Air;
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var job = TestObjectCreator.CreateJob(importAirShipment))
			{
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 10m, 10m);
				Assert("Debtor not set yet", charge.JR_OH_SellAccount.IsEmpty);
				Assert("IsARCashAdvance Flag not ticked because debtor is empty.", !charge.JR_IsARCashAdvance);
				charge.JR_OH_SellAccount = debtor.PK;
				Assert("IsARCashAdvance Flag ticked because defaulting option of matching specific config on organization is ALL.", charge.JR_IsARCashAdvance);
			}
		}

		public void TestCashAdvanceFlagIsDefaultedWhenChargeDebtorChangeFromOneDebtorToAnotherDebtor()
		{
			var debtor = TestObjectCreator.Debtor;
			RemoveAndDeleteAllConfigurationsForTest(debtor);
			CreateCashAdvanceJobConfigurationForOrganization(debtor, "ALL", LedgerTypes.AccountsReceivable, Constants.TransportModes.All, Constants.FreightShipmentDirection.Code.All, CashAdvanceDefaultingOption.None);
			CreateCashAdvanceJobConfigurationForOrganization(debtor, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsReceivable, Constants.TransportModes.Air, Constants.FreightShipmentDirection.Code.Import, CashAdvanceDefaultingOption.All);
			Factory.Save();

			var importAirShipment = TestObjectCreator.CreateShipment("S00001001", true);
			importAirShipment.JS_RL_NKOrigin = "SGSIN";
			importAirShipment.JS_RL_NKDestination = "AUBNE";
			importAirShipment.JS_TransportMode = Constants.TransportModes.Air;
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var job = TestObjectCreator.CreateJob(importAirShipment))
			{
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 10m, 10m);
				charge.JR_OH_SellAccount = debtor.PK;
				Assert("IsARCashAdvance Flag ticked because defaulting option of matching specifc config on organization is ALL.", charge.JR_IsARCashAdvance);
				charge.JR_OH_SellAccount = TestObjectCreator.Debtor1.PK;
				Assert("IsARCashAdvance Flag unticked because defaulting option of matching generic config on organization is NON.", !charge.JR_IsARCashAdvance);
			}
		}

		public void TestCashAdvanceFlagIsDefaultedWhenChargeOSSellAmountChangedFromZeroToPositiveAmount()
		{
			var debtor = TestObjectCreator.Debtor;
			RemoveAndDeleteAllConfigurationsForTest(debtor);
			CreateCashAdvanceJobConfigurationForOrganization(debtor, "ALL", LedgerTypes.AccountsReceivable, Constants.TransportModes.All, Constants.FreightShipmentDirection.Code.All, CashAdvanceDefaultingOption.None);
			CreateCashAdvanceJobConfigurationForOrganization(debtor, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsReceivable, Constants.TransportModes.Air, Constants.FreightShipmentDirection.Code.Import, CashAdvanceDefaultingOption.All);
			Factory.Save();

			var importAirShipment = TestObjectCreator.CreateShipment("S00001001", true);
			importAirShipment.JS_RL_NKOrigin = "SGSIN";
			importAirShipment.JS_RL_NKDestination = "AUBNE";
			importAirShipment.JS_TransportMode = Constants.TransportModes.Air;
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var job = TestObjectCreator.CreateJob(importAirShipment))
			{
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 0m, 0m);
				charge.JR_OH_SellAccount = debtor.PK;
				Assert("OS Sell amount is zero.", charge.JR_OSSellAmt.IsEmpty);
				Assert("IsARCashAdvance Flag not ticked because OS Sell Amount is zero", !charge.JR_IsARCashAdvance);
				charge.JR_OSSellAmt = 10m;
				Assert("IsARCashAdvance Flag ticked because defaulting option of matching specific config on organization is ALL.", charge.JR_IsARCashAdvance);
				charge.JR_IsARCashAdvance = false;
				charge.JR_OSSellAmt = 20m;
				Assert("IsARCashAdvance Flag not ticked again.", !charge.JR_IsARCashAdvance);
			}
		}

		public void TestCashAdvanceFlagIsDefaultedWhenChargeOSSellAmountChangedFromNegativeToPositiveAmount()
		{
			var debtor = TestObjectCreator.Debtor;
			RemoveAndDeleteAllConfigurationsForTest(debtor);
			CreateCashAdvanceJobConfigurationForOrganization(debtor, "ALL", LedgerTypes.AccountsReceivable, Constants.TransportModes.All, Constants.FreightShipmentDirection.Code.All, CashAdvanceDefaultingOption.None);
			CreateCashAdvanceJobConfigurationForOrganization(debtor, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsReceivable, Constants.TransportModes.Air, Constants.FreightShipmentDirection.Code.Import, CashAdvanceDefaultingOption.All);
			Factory.Save();

			var importAirShipment = TestObjectCreator.CreateShipment("S00001001", true);
			importAirShipment.JS_RL_NKOrigin = "SGSIN";
			importAirShipment.JS_RL_NKDestination = "AUBNE";
			importAirShipment.JS_TransportMode = Constants.TransportModes.Air;
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var job = TestObjectCreator.CreateJob(importAirShipment))
			{
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 10m, -10m);
				charge.JR_OH_SellAccount = debtor.PK;
				Assert("OS Sell amount is negative.", charge.JR_OSSellAmt < 0m);
				Assert("IsARCashAdvance Flag not ticked because OS Sell Amount is negative", !charge.JR_IsARCashAdvance);
				charge.JR_OSSellAmt = 10m;
				Assert("IsARCashAdvance Flag ticked because defaulting option of matching specific config on organization is ALL.", charge.JR_IsARCashAdvance);
				charge.JR_IsARCashAdvance = false;
				charge.JR_OSSellAmt = 20m;
				Assert("IsARCashAdvance Flag not ticked again.", !charge.JR_IsARCashAdvance);
			}
		}

		public void TestCashAdvanceFlagIsNOTDefaultedWhenItIsAlreadySetToTrue()
		{
			var debtor = TestObjectCreator.Debtor;
			RemoveAndDeleteAllConfigurationsForTest(debtor);
			CreateCashAdvanceJobConfigurationForOrganization(debtor, "ALL", LedgerTypes.AccountsReceivable, Constants.TransportModes.All, Constants.FreightShipmentDirection.Code.All, CashAdvanceDefaultingOption.None);
			CreateCashAdvanceJobConfigurationForOrganization(debtor, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsReceivable, Constants.TransportModes.Air, Constants.FreightShipmentDirection.Code.Import, CashAdvanceDefaultingOption.All);
			Factory.Save();

			var importAirShipment = TestObjectCreator.CreateShipment("S00001001", true);
			importAirShipment.JS_RL_NKOrigin = "SGSIN";
			importAirShipment.JS_RL_NKDestination = "AUBNE";
			importAirShipment.JS_TransportMode = Constants.TransportModes.Air;
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (var job = TestObjectCreator.CreateJob(importAirShipment))
			{
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 10m, -10m);
				charge.JR_OH_SellAccount = debtor.PK;
				Assert("OS Sell amount is negative.", charge.JR_OSSellAmt < 0m);
				Assert("IsARCashAdvance Flag not ticked because OS Sell Amount is negative", !charge.JR_IsARCashAdvance);
				charge.JR_OSSellAmt = 10m;
				Assert("IsARCashAdvance Flag ticked because defaulting option of matching specific config on organization is ALL.", charge.JR_IsARCashAdvance);

				RemoveAndDeleteAllConfigurationsForTest(debtor);
				charge.JR_OSSellAmt = 0m;
				Assert("IsARCashAdvance Flag still remained ticked, although there is no defaulting option that matches this org.", charge.JR_IsARCashAdvance);
				charge.JR_OSSellAmt = 20m;
				Assert("IsARCashAdvance Flag still remained ticked, although there is no defaulting option that matches this org.", charge.JR_IsARCashAdvance);
			}
		}

		void RemoveAndDeleteAllConfigurationsForTest(OrgHeader debtor)
		{
			var cleaningFactory = new BusinessObjectFactory();
			var debtorInCleaningFactory = cleaningFactory.Load<OrgHeader>(debtor.PK);
			debtorInCleaningFactory.CompanyData.AccARCashAdvanceConfigurations.RemoveAndDeleteAll();
			var companyInCleaningFactory = cleaningFactory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			companyInCleaningFactory.CashAdvanceConfigurations.RemoveAndDeleteAll();
			cleaningFactory.Save();
		}

		AccCashAdvanceDefaultingConfiguration CreateCashAdvanceJobConfigurationForOrganization(OrgHeader organization, ZString jobType, ZString ledgerType, ZString transportMode, ZString direction, ZString defaultingOption)
		{
			var config = organization.CompanyData.AccARCashAdvanceConfigurations.AddNew();
			config.CAC_JobType = jobType;
			config.CAC_ServiceDirection = direction;
			config.CAC_TransportMode = transportMode;
			config.CAC_Ledger = ledgerType;
			config.CAC_DefaultingOption = defaultingOption;
			return config;
		}

		void CreateCashAdvanceJobConfigurationForCompany(GlbCompany company, ZString jobType, ZString ledgerType, ZString transportMode, ZString direction, ZString defaultingOption)
		{
			var config = company.CashAdvanceConfigurations.AddNew();
			config.CAC_JobType = jobType;
			config.CAC_ServiceDirection = direction;
			config.CAC_TransportMode = transportMode;
			config.CAC_Ledger = ledgerType;
			config.CAC_DefaultingOption = defaultingOption;
		}

		#endregion

		public void TestPaymentTypeCollectionIsCached()
		{
			var cacheKey = "Charge_PaymentTypes";
			var expectedDBCommand = $@"[{Db.DatabaseName}].[dbo].[DataRegGetValueNOD]
Params
@Name: 'EnableEPaymentFunctionality'
@Owner: '{GlbCompany.CurrentCompany.PK}'";

			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job);

			var cachedPaymentTypes = Factory.GetCachedValue<CodeDescriptionPairList>(cacheKey, () => null);
			AssertNull("PaymentTypes are not cached yet.", cachedPaymentTypes);

			Factory.ClearCachedValue<CodeDescriptionPairList>(cacheKey);
			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			using (TestConnection.TrackExecutedCommands())
			{
				var paymentTypes = charge.PaymentTypes;
				AssertContainsExactElementsInAnyOrder("First call to PaymentTypes should check registry value", new[] { expectedDBCommand }, TestConnection.ExecutedCommands.Select(x => x.Trim()));
			}

			cachedPaymentTypes = Factory.GetCachedValue<CodeDescriptionPairList>(cacheKey, () => null);
			AssertNotNull("PaymentTypes are cached.", cachedPaymentTypes);
			AssertEquals(9, cachedPaymentTypes.Count);

			using (TestConnection.TrackExecutedCommands())
			{
				var paymentTypes = charge.PaymentTypes;
				AssertCollectionNotContains("Second call to PaymentTypes should NOT check registry value, we should use cached Payment Types", new[] { expectedDBCommand }, TestConnection.ExecutedCommands.Select(x => x.Trim()));
			}
		}

		public void TestSellComplianceDescription_ResourceStringDataAttribute()
		{
			var charge = Factory.New<Charge>();
			var resourceStringData = Enterprise.Customs.Business.ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(charge.SellComplianceDescriptionInfo);

			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Sell Compliance Description", resourceStringData.Caption);
				AssertEquals("ShortCaption", "Sell Comp. Desc.", resourceStringData.ShortCaption);
			});
		}

		public void TestSellComplianceDescription()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var job = TestObjectCreator.CreateJob(shipment, false);

			var accountingMasterFilesDependencyFactoryMock = new Mock<IAccountingMasterFilesDependencyFactory>();
			var chargeCodeOverrideRulesRankerMock = new Mock<IChargeCodeOverrideRulesRanker>();
			ObjectFactory.Substitute(accountingMasterFilesDependencyFactoryMock.Object);
			accountingMasterFilesDependencyFactoryMock.Setup(x => x.GetChargeCodeOverrideRulesRanker()).Returns(chargeCodeOverrideRulesRankerMock.Object);

			var charge = job.Charges.AddNew();
			charge.JR_AL_ARLine = Factory.New<AccTransactionLines>().PK;
			charge.ARLine.AL_LineType = TransactionLineTypes.Revenue;
			Assert("Precondition: IsRevenuePosted", charge.IsRevenuePosted);
			Assert(charge.SellComplianceDescription.IsEmpty);
			chargeCodeOverrideRulesRankerMock.Verify(x => x.GetBestSellComplianceDescriptionOverride(It.IsAny<BusinessObjectFactory>(), It.IsAny<IBusinessObjectCollection>(), It.IsAny<ZString?>(), It.IsAny<ZString?>(), It.IsAny<ZString>()), Times.Never);

			charge = Factory.New<Charge>();
			Assert("Precondition: IsRevenuePosted", !charge.IsRevenuePosted);
			AssertNull("Precondition: InvoicingJob", charge.InvoicingJob);
			AssertNull("Precondition: JobType", charge.InvoicingJob?.JobType);
			Assert(charge.SellComplianceDescription.IsEmpty);
			chargeCodeOverrideRulesRankerMock.Verify(x => x.GetBestSellComplianceDescriptionOverride(It.IsAny<BusinessObjectFactory>(), It.IsAny<IBusinessObjectCollection>(), It.IsAny<ZString?>(), It.IsAny<ZString?>(), It.IsAny<ZString>()), Times.Never);

			charge = job.Charges.AddNew();
			Assert("Precondition: IsRevenuePosted", !charge.IsRevenuePosted);
			AssertNotNull("Precondition: InvoicingJob", charge.InvoicingJob);
			AssertNotNull("Precondition: JobType", charge.InvoicingJob.JobType);
			AssertNull("Precondition: ChargeCode", charge.ChargeCode);
			Assert(charge.SellComplianceDescription.IsEmpty);
			chargeCodeOverrideRulesRankerMock.Verify(x => x.GetBestSellComplianceDescriptionOverride(It.IsAny<BusinessObjectFactory>(), It.IsAny<IBusinessObjectCollection>(), It.IsAny<ZString?>(), It.IsAny<ZString?>(), It.IsAny<ZString>()), Times.Never);

			charge.JR_AC = TestObjectCreator.CC1.PK;
			AssertNotNull("Precondition: ChargeCode", charge.ChargeCode);

			var jobType = JobInvoicingConsumerTypes.ShipmentCode;
			var transportMode = shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			var sellSupplyType = charge.JR_SellSupplyType = SupplyTypeClassificationCodes.INT;

			chargeCodeOverrideRulesRankerMock.Setup(x => x.GetBestSellComplianceDescriptionOverride(Factory, charge.ChargeCode.ChargeComplianceDescriptions, It.IsAny<ZString?>(), It.IsAny<ZString?>(), It.IsAny<ZString>())).Returns(ZString.Empty);

			Assert(charge.SellComplianceDescription.IsEmpty);
			chargeCodeOverrideRulesRankerMock.Verify(x => x.GetBestSellComplianceDescriptionOverride(It.IsAny<BusinessObjectFactory>(), It.IsAny<IBusinessObjectCollection>(), It.IsAny<ZString?>(), It.IsAny<ZString?>(), It.IsAny<ZString>()), Times.Once);
			chargeCodeOverrideRulesRankerMock.Verify(x => x.GetBestSellComplianceDescriptionOverride(Factory, charge.ChargeCode.ChargeComplianceDescriptions, jobType, transportMode, sellSupplyType));

			chargeCodeOverrideRulesRankerMock.Reset();
			chargeCodeOverrideRulesRankerMock.Setup(x => x.GetBestSellComplianceDescriptionOverride(Factory, charge.ChargeCode.ChargeComplianceDescriptions, It.IsAny<ZString?>(), It.IsAny<ZString?>(), It.IsAny<ZString>())).Returns("TEST");

			AssertEquals("TEST", charge.SellComplianceDescription);
			chargeCodeOverrideRulesRankerMock.Verify(x => x.GetBestSellComplianceDescriptionOverride(It.IsAny<BusinessObjectFactory>(), It.IsAny<IBusinessObjectCollection>(), It.IsAny<ZString?>(), It.IsAny<ZString?>(), It.IsAny<ZString>()), Times.Once);
			chargeCodeOverrideRulesRankerMock.Verify(x => x.GetBestSellComplianceDescriptionOverride(Factory, charge.ChargeCode.ChargeComplianceDescriptions, jobType, transportMode, sellSupplyType));
		}

		#region ISellComplianceDescription Tests

		public void TestISellComplianceDescriptionImplementation_Description()
		{
			var charge = Factory.New<Charge>();
			var sellComplianceDescription = charge as ISellComplianceDescription;

			charge.JR_Desc = ZString.Empty;
			AssertEquals(ZString.Empty, sellComplianceDescription.Description);

			charge.JR_Desc = "TEST-CHARGE-DESC";
			AssertEquals("TEST-CHARGE-DESC", sellComplianceDescription.Description);

			sellComplianceDescription.Description = "CHARGEDESCRIPTION";
			AssertEquals("CHARGEDESCRIPTION", charge.JR_Desc);
		}

		public void TestISellComplianceDescriptionImplementation_SellComplianceDescription()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;

			var accountingMasterFilesDependencyFactoryMock = new Mock<IAccountingMasterFilesDependencyFactory>();
			var chargeCodeOverrideRulesRankerMock = new Mock<IChargeCodeOverrideRulesRanker>();
			ObjectFactory.Substitute(accountingMasterFilesDependencyFactoryMock.Object);
			accountingMasterFilesDependencyFactoryMock.Setup(x => x.GetChargeCodeOverrideRulesRanker()).Returns(chargeCodeOverrideRulesRankerMock.Object);

			var sellComplianceDescription = charge as ISellComplianceDescription;
			chargeCodeOverrideRulesRankerMock.Setup(x => x.GetBestSellComplianceDescriptionOverride(Factory, charge.ChargeCode.ChargeComplianceDescriptions, It.IsAny<ZString?>(), It.IsAny<ZString?>(), It.IsAny<ZString>())).Returns(ZString.Empty);
			AssertEquals(ZString.Empty, sellComplianceDescription.SellComplianceDescription);

			chargeCodeOverrideRulesRankerMock.Setup(x => x.GetBestSellComplianceDescriptionOverride(Factory, charge.ChargeCode.ChargeComplianceDescriptions, It.IsAny<ZString?>(), It.IsAny<ZString?>(), It.IsAny<ZString>())).Returns("DESCRIPTION");
			AssertEquals("DESCRIPTION", sellComplianceDescription.SellComplianceDescription);
		}

		#endregion

		#region Set Property Value Only If Old Value Is Not Same As New Value

		public void TestJR_ACIsSetOnlyIfOldValueIsNotSameAsNewValue()
		{
			AssertPropertyValueIsSetOnlyIfOldValueIsNotSameAsNewValue(JobChargeSchema.Constants.JR_AC, TestObjectCreator.CC1.PK, TestObjectCreator.CC2.PK);
		}

		public void TestJR_AgentDeclaredCostAmtIsSetOnlyIfOldValueIsNotSameAsNewValue()
		{
			AssertPropertyValueIsSetOnlyIfOldValueIsNotSameAsNewValue(JobChargeSchema.Constants.JR_AgentDeclaredCostAmt, (ZDecimal)20m, (ZDecimal)100m);
		}

		public void TestJR_AgentDeclaredSellAmtIsSetOnlyIfOldValueIsNotSameAsNewValue()
		{
			AssertPropertyValueIsSetOnlyIfOldValueIsNotSameAsNewValue(JobChargeSchema.Constants.JR_AgentDeclaredSellAmt, (ZDecimal)20m, (ZDecimal)100m);
		}

		public void TestJR_CostRatedIsSetOnlyIfOldValueIsNotSameAsNewValue()
		{
			AssertPropertyValueIsSetOnlyIfOldValueIsNotSameAsNewValue(JobChargeSchema.Constants.JR_CostRated, (ZBool)true, (ZBool)false);
		}

		public void TestJR_DescIsSetOnlyIfOldValueIsNotSameAsNewValue()
		{
			AssertPropertyValueIsSetOnlyIfOldValueIsNotSameAsNewValue(JobChargeSchema.Constants.JR_Desc, (ZString)"Administration", (ZString)"Bank Charges");
		}

		public void TestJR_EstimatedRevenueIsSetOnlyIfOldValueIsNotSameAsNewValue()
		{
			AssertPropertyValueIsSetOnlyIfOldValueIsNotSameAsNewValue(JobChargeSchema.Constants.JR_EstimatedRevenue, (ZDecimal)20m, (ZDecimal)100m);
		}

		public void TestJR_LocalCostAmtIsSetOnlyIfOldValueIsNotSameAsNewValue()
		{
			AssertPropertyValueIsSetOnlyIfOldValueIsNotSameAsNewValue(JobChargeSchema.Constants.JR_LocalCostAmt, (ZDecimal)20m, (ZDecimal)100m);
		}

		public void TestJR_OH_SellAccountIsSetOnlyIfOldValueIsNotSameAsNewValue()
		{
			AssertPropertyValueIsSetOnlyIfOldValueIsNotSameAsNewValue(JobChargeSchema.Constants.JR_OH_SellAccount, TestObjectCreator.Debtor.PK, TestObjectCreator.Debtor1.PK);
		}

		public void TestJR_OrderReferenceIsSetOnlyIfOldValueIsNotSameAsNewValue()
		{
			AssertPropertyValueIsSetOnlyIfOldValueIsNotSameAsNewValue(JobChargeSchema.Constants.JR_OrderReference, (ZString)"S00001050", (ZString)"S00001058");
		}

		public void TestJR_SellRatedIsSetOnlyIfOldValueIsNotSameAsNewValue()
		{
			AssertPropertyValueIsSetOnlyIfOldValueIsNotSameAsNewValue(JobChargeSchema.Constants.JR_SellRated, (ZBool)true, (ZBool)false);
		}

		public void TestJR_LocalSellAmtIsSetOnlyIfOldValueIsNotSameAsNewValue()
		{
			AssertPropertyValueIsSetOnlyIfOldValueIsNotSameAsNewValue(JobChargeSchema.Constants.JR_LocalSellAmt, (ZDecimal)20m, (ZDecimal)40m);
		}

		void AssertPropertyValueIsSetOnlyIfOldValueIsNotSameAsNewValue(ZString porpertyName, IZType value1, IZType value2)
		{
			var expectedWarningMessage = "Any changes will require authorization from your supervisor or accountant upon saving and/or posting.";

			var jobParent = Factory.New<DummyParent>();
			((DummyParentJobInvoicingSupporter)jobParent.InvoicingSupporter).fEditSecurityLock = true;
			var job = TestObjectCreator.CreateJob(jobParent, createWithMutex: false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 10m, 10m);
			var propertyInfo = charge.FindPropertyInfo(porpertyName);
			propertyInfo.Value = value1;

			AssertEquals(value1, propertyInfo.Value);
			AssertHasRowWarning(charge, expectedWarningMessage);

			charge.ClearRowNotifications();
			propertyInfo.Value = value1;

			AssertEquals(value1, propertyInfo.Value);
			AssertNoRowWarningContaining(charge, expectedWarningMessage);

			charge.ClearRowNotifications();
			propertyInfo.Value = value2;

			AssertEquals(value2, propertyInfo.Value);
			AssertHasRowWarning(charge, expectedWarningMessage);
		}

		#endregion

		public void TestAPCashAdvanceRequest()
		{
			Job job = CreateJob("Z00001012", ZECTRA, true, 5M, ABIGAS, true, 10M);
			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			AssertNull(charge.APCashAdvanceRequestLine);
			AssertNull(charge.APCashAdvanceRequestHeader);
			charge.JR_IsAPCashAdvance = true;

			var cashAdvance = Factory.NewWithValidTestData<AccCashAdvanceRequestHeader>();
			cashAdvance.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Paid;
			cashAdvance.CAH_RequestReferenceNumber = "100001";
			var cashAdvanceLine = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			cashAdvanceLine.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
			cashAdvanceLine.CAL_CAH_RequestHeader = cashAdvance.PK;
			charge.JR_CAL_APLine = cashAdvanceLine.PK;
			AssertNotNull(charge.APCashAdvanceRequestLine);
			AssertNotNull(charge.APCashAdvanceRequestHeader);
			AssertEquals(cashAdvanceLine.PK, charge.APCashAdvanceRequestLine.PK);
			AssertEquals(cashAdvance.PK, charge.APCashAdvanceRequestHeader.PK);
		}

		public void TestARCashAdvanceRequest()
		{
			Job job = CreateJob("Z00001012", ZECTRA, true, 5M, ABIGAS, true, 10M);
			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			AssertNull(charge.ARCashAdvanceRequestLine);
			AssertNull(charge.ARCashAdvanceRequestHeader);
			charge.JR_IsARCashAdvance = false;
			AssertEquals(ZString.Empty, charge.ARCashAdvanceRequestStatus);
			AssertEquals(ZString.Empty, charge.ARCashAdvanceRequestStatusDescription);
			AssertEquals(ZString.Empty, charge.ARCashAdvanceRequestID);

			charge.JR_IsARCashAdvance = true;
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Pending, charge.ARCashAdvanceRequestStatus);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.PendingDescription, charge.ARCashAdvanceRequestStatusDescription);
			AssertEquals(ZString.Empty, charge.ARCashAdvanceRequestID);

			var cashAdvance = Factory.NewWithValidTestData<AccCashAdvanceRequestHeader>();
			cashAdvance.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Paid;
			cashAdvance.CAH_RequestReferenceNumber = "100001";
			var cashAdvanceLine = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			cashAdvanceLine.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
			cashAdvanceLine.CAL_CAH_RequestHeader = cashAdvance.PK;
			charge.JR_CAL_ARLine = cashAdvanceLine.PK;
			AssertNotNull(charge.ARCashAdvanceRequestLine);
			AssertNotNull(charge.ARCashAdvanceRequestHeader);
			AssertEquals(cashAdvanceLine.PK, charge.ARCashAdvanceRequestLine.PK);
			AssertEquals(cashAdvance.PK, charge.ARCashAdvanceRequestHeader.PK);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Paid, charge.ARCashAdvanceRequestStatus);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.PaidDescription, charge.ARCashAdvanceRequestStatusDescription);
			AssertEquals("100001", charge.ARCashAdvanceRequestID);
		}

		public void TestJR_IsARCashAdvance_ReadOnly()
		{
			Job job = CreateJob("Z00001012", ZECTRA, true, 5M, ABIGAS, true, 10M);
			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			AssertNull(charge.ARCashAdvanceRequestHeader);

			charge.JR_IsARCashAdvance = true;
			Assert(!charge.IsRevenuePosted);
			Assert(!charge.JR_IsARCashAdvance_ReadOnly);

			charge.JR_AL_ARLine = ZGuid.Empty;
			Assert(!charge.IsRevenuePosted);
			var cashAdvance = Factory.NewWithValidTestData<AccCashAdvanceRequestHeader>();
			cashAdvance.CAH_RequestReferenceNumber = "100001";

			var cashAdvanceLine = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			cashAdvanceLine.CAL_CAH_RequestHeader = cashAdvance.PK;
			charge.JR_CAL_ARLine = cashAdvanceLine.PK;
			cashAdvance.Lines.Add(cashAdvanceLine);

			foreach (var code in CashAdvanceStatusCodes.RequestLine.CodesList.GetAllCodes())
			{
				cashAdvance.Lines[0].CAL_Status = code;
				if (code == CashAdvanceStatusCodes.RequestHeader.Pending || code == CashAdvanceStatusCodes.RequestHeader.Cancelled)
				{
					Assert("Should not be readonly if pending/cancelled", !charge.JR_IsARCashAdvance_ReadOnly);
				}
				else
				{
					Assert("Should be readonly if other than pending/cancelled", charge.JR_IsARCashAdvance_ReadOnly);
				}
			}

			charge.JR_CAL_ARLine = ZGuid.Empty;
			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_LineType = TransactionLineTypes.Revenue;
			charge.JR_AL_ARLine = line.PK;
			Assert(charge.IsRevenuePosted);
			Assert("Should be readonly if REV posted", charge.JR_IsARCashAdvance_ReadOnly);
		}

		public void TestJR_SellSupplyType_ReadOnly()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S001");
			shipment.JS_ActualChargeable = 100.0m;
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var charge = TestObjectCreator.CreateCharge(job);
			AssertEquals("Precondition", false, charge.IsMainAllFieldsReadonly_ForTestOnly);

			charge.ParentConsolRevenue = new ConsolRevenue.ConsolRevenue(new ConsolRevenue.ConsolRevenueMaster(consol, Factory)) { ApportionmentMethod = AllocationMethod.GrossWeight };
			AssertEquals(true, charge.JR_SellSupplyTypeInfo.ReadOnly);

			charge.ParentConsolRevenue = null;
			AssertEquals(false, charge.JR_SellSupplyTypeInfo.ReadOnly);

			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			charge.JR_AL_ARLine = line.PK;
			AssertEquals("Precondition", true, charge.IsMainAllFieldsReadonly_ForTestOnly);
			AssertEquals(true, charge.JR_SellSupplyTypeInfo.ReadOnly);
		}

		public void TestJR_CostSupplyType_ReadOnly()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S001");
			shipment.JS_ActualChargeable = 100.0m;
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var charge = TestObjectCreator.CreateCharge(job);
			AssertEquals("Precondition", false, charge.IsAllCostFieldsReadonly_ForTestOnly);

			charge.ParentConsolRevenue = new ConsolRevenue.ConsolRevenue(new ConsolRevenue.ConsolRevenueMaster(consol, Factory)) { ApportionmentMethod = AllocationMethod.GrossWeight };
			AssertEquals(true, charge.JR_CostSupplyTypeInfo.ReadOnly);

			charge.ParentConsolRevenue = null;
			AssertEquals(false, charge.JR_CostSupplyTypeInfo.ReadOnly);

			charge.JR_E6 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1).PK;
			AssertEquals("Precondition", true, charge.IsAllCostFieldsReadonly_ForTestOnly);
			AssertEquals(true, charge.JR_CostSupplyTypeInfo.ReadOnly);
		}

		public void TestChargeDescription_EnableLocalChargeCodeDescription_QuickBooking()
			=> AssertChargeDescription_EnableLocalChargeCodeDescription(QuotedBooking.New(QuoteBookingType.QuickBooking, Factory));

		public void TestChargeDescription_EnableLocalChargeCodeDescription_BookingWithQuote()
			=> AssertChargeDescription_EnableLocalChargeCodeDescription(QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory));

		void AssertChargeDescription_EnableLocalChargeCodeDescription(QuotedBooking quotedBooking)
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Desc = "Charge Description 1";
			chargeCode.AC_LocalLanguageDescription = "Charge Local Description 1 速度";

			var helper = new TestHelper(Factory);
			var localClient = helper.NewOrgHeader();
			localClient.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var nonLocalClient = helper.NewOrgHeader();
			nonLocalClient.OH_RL_NKClosestPort = "USLAX";

			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var job = new Job.Loader(quotedBooking).TryLoadOrCreateWithoutMutexForTestOnly())
			{
				var charge = job.Charges.AddNew();
				charge.JR_AC = chargeCode.PK;

				CombineAssertions("BaseCharge.SetInitialChargeDescription: Charge description should depends on Debtor", () =>
				{
					AssertEquals("Local Client", "Charge Description 1", charge.JR_Desc);

					charge.JR_OH_SellAccount = nonLocalClient.PK;
					charge.JR_AC = ZGuid.Empty;
					charge.JR_AC = chargeCode.PK;
					AssertEquals("Initialize ChargeCode and set Debtor to NonLocalClient", "Charge Description 1", charge.JR_Desc);

					charge.JR_OH_SellAccount = localClient.PK;
					charge.JR_AC = ZGuid.Empty;
					charge.JR_AC = chargeCode.PK;
					AssertEquals("Initialize ChargeCode and set Debtor to LocalClient", "Charge Local Description 1 速度", charge.JR_Desc);
				});

				CombineAssertions("ChargeWithCost.JR_OH_SellAccount: Charge description should depends on Debtor", () =>
				{
					charge.JR_OH_SellAccount = nonLocalClient.PK;
					AssertEquals("Set Debtor to NonLocalClient", "Charge Description 1", charge.JR_Desc);

					charge.JR_OH_SellAccount = localClient.PK;
					AssertEquals("Set Debtor to LocalClient", "Charge Local Description 1 速度", charge.JR_Desc);
				});
			}
		}

		public void TestChargeDescription_EnableLocalChargeCodeDescription_OneOffQuote()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Desc = "Charge Description 1";
			chargeCode.AC_LocalLanguageDescription = "Charge Local Description 1 速度";

			var helper = new TestHelper(Factory);
			var localClient = helper.NewOrgHeader();
			localClient.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var nonLocalClient = helper.NewOrgHeader();
			nonLocalClient.OH_RL_NKClosestPort = "USLAX";

			var oneOffQuote = CreateOneOffQuote("LSE", "CFR", localClient, "AUBNE", "NZAKL", 1m, 1m);

			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var job = new Job.Loader(oneOffQuote).TryLoadOrCreateWithoutMutexForTestOnly())
			{
				var charge = job.Charges.AddNew();
				charge.JR_AC = chargeCode.PK;

				CombineAssertions("BaseCharge.SetInitialChargeDescription: OneOffQuote charge description should depends on Client and not SellAccount(Debtor)", () =>
				{
					AssertEquals("Local Client", "Charge Local Description 1 速度", charge.JR_Desc);

					charge.JR_OH_SellAccount = nonLocalClient.PK;
					charge.JR_AC = ZGuid.Empty;
					charge.JR_AC = chargeCode.PK;
					AssertEquals("SellAccount(Debtor) - NonLocalClient", "Charge Local Description 1 速度", charge.JR_Desc);

					charge.JR_OH_SellAccount = localClient.PK;
					charge.JR_AC = ZGuid.Empty;
					charge.JR_AC = chargeCode.PK;
					AssertEquals("SellAccount(Debtor) - LocalClient", "Charge Local Description 1 速度", charge.JR_Desc);

					oneOffQuote.Quote.QuotationClientAddress.OrganisationPK = nonLocalClient.PK;
					charge.JR_AC = ZGuid.Empty;
					charge.JR_AC = chargeCode.PK;
					AssertEquals("QuotationClientAddress - NonLocalClient", "Charge Description 1", charge.JR_Desc);

					charge.JR_OH_SellAccount = localClient.PK;
					charge.JR_AC = ZGuid.Empty;
					charge.JR_AC = chargeCode.PK;
					AssertEquals("SellAccount(Debtor) - LocalClient - after modifying QuotationClientAddress", "Charge Description 1", charge.JR_Desc);

					oneOffQuote.Quote.QuotationClientAddress.OrganisationPK = localClient.PK;
					charge.JR_AC = ZGuid.Empty;
					charge.JR_AC = chargeCode.PK;
					AssertEquals("QuotationClientAddress - LocalClient", "Charge Local Description 1 速度", charge.JR_Desc);
				});

				CombineAssertions("Updating SellAccount(Debtor)/QuotationClientAddress: OneOffQuote charge description should depends on Client and not SellAccount(Debtor)", () =>
				{
					AssertEquals("Local Client", "Charge Local Description 1 速度", charge.JR_Desc);

					charge.JR_OH_SellAccount = nonLocalClient.PK;
					AssertEquals("SellAccount(Debtor) - NonLocalClient", "Charge Local Description 1 速度", charge.JR_Desc);

					charge.JR_OH_SellAccount = localClient.PK;
					AssertEquals("SellAccount(Debtor) - LocalClient", "Charge Local Description 1 速度", charge.JR_Desc);

					oneOffQuote.Quote.QuotationClientAddress.OrganisationPK = nonLocalClient.PK;
					AssertEquals("QuotationClientAddress - Non Local Client", "Charge Description 1", charge.JR_Desc);

					charge.JR_OH_SellAccount = localClient.PK;
					AssertEquals("SellAccount(Debtor) - LocalClient - after modifying QuotationClientAddress", "Charge Description 1", charge.JR_Desc);

					oneOffQuote.Quote.QuotationClientAddress.OrganisationPK = localClient.PK;
					AssertEquals("QuotationClientAddress - Local Client", "Charge Local Description 1 速度", charge.JR_Desc);
				});
			}
		}

		QuotedBooking CreateOneOffQuote(string mode, string paymentTerms, OrgHeader client, string origin, string destination, decimal weight, decimal volume)
		{
			var quotePK = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted).PK;
			var quotedBooking = QuotedBooking.New(quotePK, ZGuid.Empty, Factory);
			if (quotedBooking.ClientDocAddress != null && client?.MainAddress != null)
			{
				quotedBooking.ClientDocAddress.E2_OA_Address = client.MainAddress.PK;
			}

			quotedBooking.Mode = mode;
			quotedBooking.PaymentTerms = paymentTerms;
			quotedBooking.Origin = origin;
			quotedBooking.Destination = destination;
			quotedBooking.Weight = weight;
			quotedBooking.Volume = volume;

			return quotedBooking;
		}

		public void TestIsSelectedForImportWithBulkChargeImportCollection()
		{
			var collection = new ChargeForBulkChargeImportCollection(Factory.NewJobForTesting<InvoicingBaseBulkChargeImporterDependentJob>());
			var charge = collection.AddNew();
			Assert(!charge.IsSelectedForImport);
			charge.IsSelectedForImport = true;
			Assert(charge.IsSelectedForImport);
		}

		public void TestIsSelectedForImportWithoutBulkChargeImportCollection()
		{
			var charge = Factory.NewWithValidTestData<Charge>();
			Assert(!charge.IsSelectedForImport);
			charge.IsSelectedForImport = true;
			var isSelectedForImportInfoChanged = false;
			charge.IsSelectedForImportInfo.ValueChanged += delegate
			{ isSelectedForImportInfoChanged = true; };
			Assert(!charge.IsSelectedForImport);
			Assert(!isSelectedForImportInfoChanged);
		}

		public void TestJR_OSSellInvoiceExRate_ForDisplayInfo()
		{
			var charge = (Charge)TestCharge;
			AssertEquals("Sell Invoice Ex. Rate", "Sell Invoice Ex. Rate", charge.JR_OSSellInvoiceExRate_ForDisplayInfo.HumanReadableName);
		}

		public void TestLocalSellTaxAmount_TaxDate()
		{
			var charge = (Charge)TestCharge;
			var arPostingCharge = (IReceivablesPostingCharge)charge;
			charge.JR_AT_SellGSTRate = TestObjectCreator.GSTANDQST1WithDates.PK;
			charge.JR_LocalSellInvoiceAmt_ForTestOnly = 100;
			AssertEquals(nameof(arPostingCharge.LocalSellTaxAmount), 14.98m, arPostingCharge.LocalSellTaxAmount);

			charge.JR_SellTaxDate = TestObjectCreator.GSTANDQST1WithDates_DateWithNoRate;
			AssertEquals(nameof(arPostingCharge.LocalSellTaxAmount), 9.5m, arPostingCharge.LocalSellTaxAmount);
		}

		public void TestPostChargesFromDifferentJobsWithDifferentInvoiceExRate()
		{
			AssertPostChargesFromDifferentJobsWithDifferentInvoiceExRate();
		}

		public void TestPostChargesFromDifferentJobsWithDifferentInvoiceExRateWithCFX()
		{
			AssertPostChargesFromDifferentJobsWithDifferentInvoiceExRate(true);
		}

		void AssertPostChargesFromDifferentJobsWithDifferentInvoiceExRate(bool withCFX = false)
		{
			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "DEF");
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, withCFX);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_ARBuyersConsolInvoicingStyle = Constants.ConsolInvoicingStyles.Master;

			var masterShipment = TestObjectCreator.CreateMasterShipment("S001", null);
			var masterJob = new Job.Loader(masterShipment).TryCreateWithoutMutexForTestOnly();
			masterJob.LocalChargesPK = org.PK;

			TestObjectCreator.SetExchangeRate(masterJob, TestObjectCreator.USD, 0.7m, TestObjectCreator.Debtor.PK, ExchangeRateOrgTypeEnum.Debtor);
			var charge1 = TestObjectCreator.CreateCharge(masterJob, TestObjectCreator.CC1, "desc", TestObjectCreator.AUD, 0, null, TestObjectCreator.AUD, 100m, TestObjectCreator.Debtor);
			charge1.JR_RX_NKSellInvoiceCurrency = "USD";

			var subShipment = TestObjectCreator.CreateShipmentWithCoLoadMaster("S002", null, masterShipment);
			var subJob = new Job.Loader(subShipment).TryCreateWithoutMutexForTestOnly();
			subJob.LocalChargesPK = org.PK;

			TestObjectCreator.SetExchangeRate(subJob, TestObjectCreator.USD, 0.8m, TestObjectCreator.Debtor.PK, ExchangeRateOrgTypeEnum.Debtor);
			var charge2 = TestObjectCreator.CreateCharge(subJob, TestObjectCreator.CC1, "desc", TestObjectCreator.AUD, 0, null, TestObjectCreator.AUD, 200m, TestObjectCreator.Debtor);
			charge2.JR_RX_NKSellInvoiceCurrency = "USD";

			if (withCFX)
			{
				charge1.JR_LineCFX = 2m;
				charge2.JR_LineCFX = 2m;
			}

			Factory.Save();

			AssertEquals("should include charges from subshipment", 2, new BusinessObjectFactory().Load<Job>(masterJob.PK).Charges.Count);

			var charges = new IReceivablesPostingChargeCollection();
			charges.Key = new PostingChargeKey(TestObjectCreator.Debtor.PK, InvoiceTypesList.Codes.FinalInvoice, "C00001234", ZGuid.Empty, ZGuid.Empty, 0);
			charges.Add(charge1);
			charges.Add(charge2);

			ErrorReporter.Clear();
			var invoice = new ChargePoster(Factory).Post(charges);
			Factory.Save();
			AssertEquals("LastMessageReported", "", ErrorReporter.LastMessageReported);

			var invoiceLine1 = invoice.Lines[0];
			var invoiceLine2 = invoice.Lines[1];

			AssertEquals("Invoice Currency", Constants.CurrencyCodes.UnitedStates, invoice.AH_RX_NKTransactionCurrency);
			AssertEquals("Invoice Currency Ex Rate", 0.7m, invoice.AH_ExchangeRate);

			AssertEquals(invoiceLine1.PK, charge1.JR_AL_ARLine);
			AssertEquals(invoiceLine2.PK, charge2.JR_AL_ARLine);

			if (withCFX)
			{
				AssertEquals("OSExTaxAmount", 71.4m, invoiceLine1.AL_OSExTaxAmount);
				AssertEquals("LocalExTaxAmount", 102m, invoiceLine1.AL_LocalExTaxAmount);
				AssertEquals("OSExTaxAmount", 142.8m, invoiceLine2.AL_OSExTaxAmount);
				AssertEquals("LocalExTaxAmount", 204m, invoiceLine2.AL_LocalExTaxAmount);

				AssertNotNull("Charge1 CFX Line", charge1.CFXLine);
				AssertEquals("Charge1 CFX Line Amount", -2m, charge1.CFXLine.AL_LineAmount);
				AssertNotNull("Charge2 CFX Line", charge2.CFXLine);
				AssertEquals("Charge2 CFX Line Amount", -4m, charge2.CFXLine.AL_LineAmount);
			}
			else
			{
				AssertEquals("OSExTaxAmount", 70m, invoiceLine1.AL_OSExTaxAmount);
				AssertEquals("LocalExTaxAmount", 100m, invoiceLine1.AL_LocalExTaxAmount);
				AssertEquals("OSExTaxAmount", 140m, invoiceLine2.AL_OSExTaxAmount);
				AssertEquals("LocalExTaxAmount", 200m, invoiceLine2.AL_LocalExTaxAmount);
			}

			AssertEquals("Charge1 Sell Currency", Constants.CurrencyCodes.Australia, charge1.JR_RX_NKSellCurrency);
			AssertEquals("Charge1 Sell Ex Rate", 1m, charge1.JR_OSSellExRate);
			AssertEquals("Charge1 OSSellAmount", 100m, charge1.JR_OSSellAmt);
			AssertEquals("Charge1 LocalSellAmount", 100m, charge1.JR_LocalSellAmt);

			AssertEquals("Charge2 Sell Currency", Constants.CurrencyCodes.Australia, charge2.JR_RX_NKSellCurrency);
			AssertEquals("Charge1 Sell Ex Rate", 1m, charge2.JR_OSSellExRate);
			AssertEquals("Charge2 OSSellAmount", 200m, charge2.JR_OSSellAmt);
			AssertEquals("Charge2 LocalSellAmount", 200m, charge2.JR_LocalSellAmt);
		}

		public void TestJR_SellInvoice_LocalGSTAmount_TaxDate()
		{
			var charge = (Charge)TestCharge;
			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.Code;
			charge.JR_AT_SellGSTRate = TestObjectCreator.GSTANDQST1WithDates.PK;
			charge.JR_LocalSellInvoiceAmt_ForTestOnly = 100;
			Assert($"Precondition: {nameof(charge.BillInInvoiceCurrency)}", charge.BillInInvoiceCurrency);
			AssertEquals(nameof(charge.JR_SellInvoice_LocalGSTAmount), 14.98m, charge.JR_SellInvoice_LocalGSTAmount);
			AssertEquals(nameof(charge.JR_Calc_LocalSellInvoiceExtraTaxAmt), 9.98m, charge.JR_Calc_LocalSellInvoiceExtraTaxAmt);

			charge.JR_SellTaxDate = TestObjectCreator.GSTANDQST1WithDates_DateWithNoRate;
			AssertEquals(nameof(charge.JR_SellInvoice_LocalGSTAmount), 9.5m, charge.JR_SellInvoice_LocalGSTAmount);
			AssertEquals(nameof(charge.JR_Calc_LocalSellInvoiceExtraTaxAmt), 9.5m, charge.JR_Calc_LocalSellInvoiceExtraTaxAmt);
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesCharge()
		{
			var charge = Factory.NewWithValidTestData<Charge>();

			var localList = new List<string>
				{
					nameof(charge.ProfitShareForCharge),
					nameof(charge.JR_Sell_LocalGSTAmount),
					nameof(charge.JR_Sell_LocalSellAmount),
					nameof(charge.JR_TotalLocalRevenue)
				};

			var osSellList = new List<string>
				{
					nameof(charge.JR_Sell_InvoiceOSAmount)
				};

			var osSellInvoiceList = new List<string>
				{
					nameof(charge.JR_OSSellInvoiceAmt),
					nameof(charge.JR_OSSellInvoiceGSTAmt)
				};

			var exList = new List<string>
				{
					nameof(charge.JR_OSSellInvoiceExRate)
				};

			var weightList = new List<string>
				{
					nameof(charge.GrossWeight),
					nameof(charge.ChargeableUnits)
				};

			var tester = new DecimalPlacesAttributeTester(charge);
			tester.CheckLocalCurrency(localList, nameof(charge.LocalCurrencyDecimals));
			tester.CheckNonLocalCurrency(osSellList, nameof(charge.OSSellCurrencyDecimals), nameof(charge.JR_RX_NKSellCurrency), charge);
			tester.CheckNonLocalCurrency(osSellInvoiceList, nameof(charge.OSSellInvoiceCurrencyDecimals), nameof(charge.JR_RX_NKSellInvoiceCurrency), charge);
			tester.CheckExchangeRate(exList, nameof(charge.ExchangeRateDecimalPlaces));
			tester.CheckConstant(weightList, nameof(charge.WeightVolumeDecimals), DefaultNumberOfDecimals.Schema.DefaultNumberOfDecimalsForWeightAndVolumeUnits);
		}

		public void TestChargeJR_RX_NKSellInvoiceCurrencyShouldBeResetWhenChargeSellAccountIsChanged()
		{
			var localClient = TestObjectCreator.AALSHI;
			var overseasAgent = TestObjectCreator.ABIGAS;
			localClient.CompanyData.OB_RX_NKARDDefltCurrency = "AUD";
			overseasAgent.CompanyData.OB_RX_NKARDDefltCurrency = "USD";

			SetOrgInvoiceRollupOrGroup(localClient, InvoicePostingOptionsList.Codes.FinalInvoiceOnly, string.Empty);
			SetOrgInvoiceRollupOrGroup(overseasAgent, InvoicePostingOptionsList.Codes.DisbursementFreightForeignAndFinal, Constants.CurrencyCodes.UnitedStates);
			AccountingMasterFilesRegistry.Instance.EnableCrossTradeDebtorDefaultingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var shipment = TestObjectCreator.CreateShipment("S00001000", "NZAKL", "USLAX");
			shipment.ConsigneePK = localClient.PK;
			shipment.ConsignorPK = overseasAgent.PK;
			var job = TestObjectCreator.CreateJob(shipment, false);

			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_INCO = Constants.IncoTerms.DeliveredDutyPaid;
			Assert(job.IsCrossTrade);

			var charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.FRT.PK;
			AssertEquals("charge sell account is defaulted with the overseas Agent", overseasAgent.PK, charge.JR_OH_SellAccount);
			AssertEquals("overseas agent default currency is USD", Constants.CurrencyCodes.UnitedStates, charge.JR_SellCurrency);
			AssertEquals("Overseas agent Invoice RollupOrGroup rules will default the invoice type to FRT", InvoiceTypesList.Codes.FreightInvoice, charge.JR_InvoiceType);
			AssertEquals("Sell invoice currency is empty because the invoice type is FRT", string.Empty, charge.JR_RX_NKSellInvoiceCurrency);

			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			AssertEquals("The sell invoice currency is updated when the invoice type is changed", Constants.CurrencyCodes.UnitedStates, charge.JR_RX_NKSellInvoiceCurrency);

			charge.JR_OH_SellAccount = localClient.PK;
			AssertEquals("Sell Invoice Currency should be reset to empty when the debtor is changed", string.Empty, charge.JR_RX_NKSellInvoiceCurrency);

			var newBranch = GlbCompany.CurrentCompany.ActiveBranches.First(x => x.PK != GlbBranch.CurrentBranch.PK);
			AssertNotNull(newBranch);

			var invoiceRollupOrGroupCollection = new InvoiceRollupOrGroupCollection();
			SetInvoiceRollupOrGroupCollectionCurrency(invoiceRollupOrGroupCollection, "EUR");
			OrganisationRegistry.Instance.InvoiceRollupOrGroup.SetValue(Guid.Empty, newBranch.PK.ToGuid(), Guid.Empty, invoiceRollupOrGroupCollection);
			Factory.Save();

			var previousBranchPK = charge.JR_GB;
			charge.JR_GB = newBranch.PK;
			AssertEquals("Sell Invoice Currency should be changed using the new registry value", "EUR", charge.JR_RX_NKSellInvoiceCurrency);

			var newdepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_IsActive, 1).AddToFilter(GlbDepartmentSchema.PK, SQLComparisonOperator.NotEqual, GlbDepartment.CurrentDepartment.PK));
			AssertNotNull(newdepartment);

			SetInvoiceRollupOrGroupCollectionCurrency(invoiceRollupOrGroupCollection, "CHF");
			OrganisationRegistry.Instance.InvoiceRollupOrGroup.SetValue(Guid.Empty, Guid.Empty, newdepartment.PK.ToGuid(), invoiceRollupOrGroupCollection);
			Factory.Save();

			charge.JR_GB = previousBranchPK;
			AssertEquals(string.Empty, charge.JR_RX_NKSellInvoiceCurrency);

			var previousDepartmentPK = charge.JR_GE;
			charge.JR_GE = newdepartment.PK;
			AssertEquals("Sell Invoice Currency should be changed using the new registry value", "CHF", charge.JR_RX_NKSellInvoiceCurrency);
			charge.JR_GE = previousDepartmentPK;
			AssertEquals(string.Empty, charge.JR_RX_NKSellInvoiceCurrency);

			void SetOrgInvoiceRollupOrGroup(OrgHeader org, string invoicePostingStyle, string invoicePostingCurrency)
			{
				org.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
				var orgInvoiceRollupOrGroup = org.CompanyData.InvoiceRollupOrGroups.AddNew();
				orgInvoiceRollupOrGroup.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
				orgInvoiceRollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
				orgInvoiceRollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
				orgInvoiceRollupOrGroup.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
				orgInvoiceRollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.None;
				orgInvoiceRollupOrGroup.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.None;
				orgInvoiceRollupOrGroup.PG_InvoicePostingStyle = invoicePostingStyle;
				orgInvoiceRollupOrGroup.PG_RX_NKInvoicePostingCurrency = invoicePostingCurrency;
				org.Factory.Save();
			}

			void SetInvoiceRollupOrGroupCollectionCurrency(InvoiceRollupOrGroupCollection collection, string invoicePostingCurrency)
			{
				collection.RemoveAndDeleteAll();
				var invoiceRollupOrGroupValue = collection.AddNew();
				invoiceRollupOrGroupValue.JobType = "ALL";
				invoiceRollupOrGroupValue.ServiceDirection = OrgConstants.ServiceDirection.Code.All;
				invoiceRollupOrGroupValue.TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
				invoiceRollupOrGroupValue.GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
				invoiceRollupOrGroupValue.GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.None;
				invoiceRollupOrGroupValue.InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.None;
				invoiceRollupOrGroupValue.InvoicePostingStyle = InvoicePostingOptionsList.Codes.FinalInvoiceOnly;
				invoiceRollupOrGroupValue.InvoicePostingCurrency = invoicePostingCurrency;
			}
		}

		public void TestChargeableRateForRevenueApportionment()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S001");
			shipment.JS_ActualChargeable = 100.0m;
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var charge = TestObjectCreator.CreateCharge(job);
			charge.ParentConsolRevenue = new ConsolRevenue.ConsolRevenue(new ConsolRevenue.ConsolRevenueMaster(consol, Factory)) { ApportionmentMethod = AllocationMethod.GrossWeight };
			AssertEquals("Precondition: ChargeableUnits", 100m, charge.ChargeableUnits);

			charge.JR_LocalSellAmt = 150;
			AssertEquals("1.5", charge.ChargeableRateForRevenueApportionment);

			charge.JR_LocalSellAmt = 0;
			AssertEquals("0", charge.ChargeableRateForRevenueApportionment);

			shipment.JS_ActualChargeable = 0;
			AssertEquals("0", charge.ChargeableRateForRevenueApportionment);

			charge.JR_LocalSellAmt = 10;
			AssertEquals("0", charge.ChargeableRateForRevenueApportionment);

			shipment.JS_ActualChargeable = 20;
			AssertEquals("0.5", charge.ChargeableRateForRevenueApportionment);

			charge.ParentConsolRevenue.ApportionmentMethod = AllocationMethod.Revenue;
			AssertEquals("Not Applicable", charge.ChargeableRateForRevenueApportionment);

			charge.ParentConsolRevenue = null;
			AssertEquals("Not Applicable", charge.ChargeableRateForRevenueApportionment);
		}

		public void TestChangeInvoiceTypeUpdatesLineCFXAndSellExRate()
		{
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var shipment = CommonShipment.New(Factory);
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "INBOM";

			var orgFactory = new BusinessObjectFactory();
			var client = orgFactory.NewWithValidTestData<OrgHeader>();
			client.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			client.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, 10m);

			var agent = orgFactory.NewWithValidTestData<OrgHeader>();
			agent.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			agent.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, 20m);

			var otherOrg = orgFactory.NewWithValidTestData<OrgHeader>();
			otherOrg.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			otherOrg.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, 2.5m);

			orgFactory.Save();

			using (Job job = JobInvoicing.Job.CreateWithMutex(Factory, shipment))
			{
				job.PlugInData = shipment;
				job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

				client.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 5m);

				job.LocalChargesPK = client.PK;
				job.AgentCollectPK = agent.PK;

				var exchRate = job.ExchangeRates.AddNew();
				exchRate.JF_RX_NKRateCurrency = TestObjectCreator.USD.RX_Code;
				exchRate.JF_BaseRate = 0.8m;

				var charge = job.Charges.AddNew();
				charge.JR_AC = Env.Registry.FreightChargeCode;
				charge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
				charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;

				charge.JR_OH_SellAccount = otherOrg.PK;
				AssertEquals("Line CFX is taken directly from org, as it's not an org on the job", 2.5m, charge.JR_LineCFX);

				var expectedZeroCFXRequires = new Dictionary<string, bool>
				{
					[InvoiceTypesList.Codes.ForeignCurrencyInvoice] = true,
					[InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching] = true,
					[InvoiceTypesList.Codes.DisbursementInForeignCurrency] = true,
					[InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching] = true,
					[InvoiceTypesList.Codes.FreightInvoice] = true,
					[InvoiceTypesList.Codes.FreightInvoice_Batching] = true,
					[InvoiceTypesList.Codes.SelfBillingInvoice] = true,
					[InvoiceTypesList.Codes.SelfBillingInvoice_Batching] = true,
					[InvoiceTypesList.Codes.SelfBillingInvoice_Batching] = true,
					[InvoiceTypesList.Codes.FinalInvoice] = false,
					[InvoiceTypesList.Codes.FinalInvoice_Batching] = false,
					[InvoiceTypesList.Codes.DestinationChargesInvoice] = false,
					[InvoiceTypesList.Codes.DestinationChargesInvoice_Batching] = false,
					[InvoiceTypesList.Codes.InvoicePerTaxCode] = false,
					[InvoiceTypesList.Codes.InvoicePerTaxCode_Batching] = false,
					[InvoiceTypesList.Codes.DisbursementInvoice] = false,
					[InvoiceTypesList.Codes.DisbursementInvoice_Batching] = false,
					[InvoiceTypesList.Codes.DoNotPost] = false,
				};
				var invoiceTypeList = (new InvoiceTypesList()).GetAllCodes();
				AssertContainsExactElementsInAnyOrder($"All existing invoice type codes must be listed in the {nameof(expectedZeroCFXRequires)} dictionary", (new InvoiceTypesList()).GetAllCodes(), expectedZeroCFXRequires.Keys);

				foreach (var kvp in expectedZeroCFXRequires)
				{
					charge.JR_InvoiceType = kvp.Key;

					if (kvp.Value)
					{
						AssertEquals("Line CFX should be 0 for " + kvp.Key, 0m, charge.JR_LineCFX);
						AssertEquals("JR_OSSellExRate should be 0.8", 0.8m, charge.JR_OSSellExRate);
					}
					else
					{
						AssertEquals("Line CFX should be 2.5 for " + kvp.Key, 2.5m, charge.JR_LineCFX);
						AssertEquals("JR_OSSellExRate should be recalculated with new JR_LineCFX to 0.78", 0.78m, charge.JR_OSSellExRate);
					}

					charge.JR_InvoiceType = kvp.Value ? InvoiceTypesList.Codes.FinalInvoice : InvoiceTypesList.Codes.ForeignCurrencyInvoice;
				}
			}
		}

		public void TestJR_OSSellExRateIsAlwaysReadOnly()
		{
			// A typical scenario when it is always ReadOnly
			var job = TestObjectCreator.CreateJob("S0001", null, 0, null, 0);
			var charge = job.Charges.AddNew();
			Assert("Always read only", charge.JR_OSSellExRateInfo.ReadOnly);
			charge.JR_AC = TestObjectCreator.CC1.PK;
			Assert("Always read only", charge.JR_OSSellExRateInfo.ReadOnly);
			charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			Assert("Always read only", charge.JR_OSSellExRateInfo.ReadOnly);
			charge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			Assert("Always read only", charge.JR_OSSellExRateInfo.ReadOnly);
			charge.JR_OSSellAmt = 100m;
			Assert("Always read only", charge.JR_OSSellExRateInfo.ReadOnly);
			charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			Assert("Always read only", charge.JR_OSSellExRateInfo.ReadOnly);

			// The only scenario from the previous implementation when it was ReadOnly even unposted
			using (var gatewayJob = TestObjectCreator.SetupGatewayLegacyJobAndEnableJRJ())
			{
				var accTransactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
				accTransactionHeader.AH_TransactionType = TransactionTypes.JobRevenueJournal;
				var accTransactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
				accTransactionLine.AL_AH = accTransactionHeader.PK;

				var chargeCode = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT")).First();
				var consolCost = Factory.NewWithValidTestData<JobConsolCost>();

				var charge1 = gatewayJob.Charges.AddNew();
				Assert("Always read only", charge1.JR_OSSellExRateInfo.ReadOnly);
				charge1.JR_AC = chargeCode.PK;
				charge1.JR_OH_SellAccount = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				charge1.JR_E6_GatewaySellHeader = consolCost.PK;
				charge1.JR_JH_InternalJob = gatewayJob.PK;
				charge1.JR_GB_InternalBranch = charge1.JR_GB;
				charge1.JR_GE_InternalDept = charge1.JR_GE;

				var charge2 = gatewayJob.Charges.AddNew();
				Assert("Always read only", charge2.JR_OSSellExRateInfo.ReadOnly);
				charge2.JR_AL_ARLine = accTransactionLine.PK;
				charge2.JR_AC = chargeCode.PK;
				charge2.JR_E6_GatewaySellHeader = consolCost.PK;

				Assert("Always read only", charge1.JR_OSSellExRateInfo.ReadOnly);
				Assert("Always read only", charge2.JR_OSSellExRateInfo.ReadOnly);
			}
		}

		public void TestCopyPersistentValuesFrom_OnlyFieldsValidToCopyAndInValidOrder_Errors()
		{
			var sourceCharge = Factory.New<Charge>();
			var destinationCharge = Factory.New<Charge>();

			AssertExceptionThrown(typeof(ArgumentNullException), () => destinationCharge.CopyPersistentValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(null));

			destinationCharge.CopyPersistentValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(sourceCharge);
			AssertContains("Destination charges must belong to a job.\r\nCharge: PK = ", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			var job = TestObjectCreator.CreateJob("S0001", null, 0, null, 0);
			job.JH_GC = ZGuid.Empty;
			destinationCharge.JR_JH = job.PK;
			destinationCharge.CopyPersistentValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(sourceCharge);
			AssertContains("Source charge must belong to a job.\r\nCharge: PK = ", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			var job2 = TestObjectCreator.CreateJob("S0002", null, 0, null, 0);
			sourceCharge.JR_JH = job2.PK;
			destinationCharge.CopyPersistentValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(sourceCharge);
			AssertContains("Destination charge job must have company set.\r\nCharge: PK = ", ErrorReporter.LastMessageReported);
			AssertContains("\r\nJob:", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			job.JH_GC = TestObjectCreator.NonCurrentCompany.PK;
			destinationCharge.CopyPersistentValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(sourceCharge);
			AssertContains(string.Format("Charges must belong to the same company job: destination company '{0}', source company '{1}'.\r\nDestination Charge: PK = ",
				TestObjectCreator.NonCurrentCompany.GC_Code, GlbCompany.CurrentCompany.GC_Code), ErrorReporter.LastMessageReported);
			AssertContains("\r\nDestination Job: Job Number = S0001, PK = ", ErrorReporter.LastMessageReported);
			AssertContains("\r\nSource Charge: PK = ", ErrorReporter.LastMessageReported);
			AssertContains("\r\nSource Job: Job Number = S0002, PK = ", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			job2.JH_GC = TestObjectCreator.NonCurrentCompany.PK;
			using (destinationCharge.Calculations.SuspendCalculations())
			{
				destinationCharge.CopyPersistentValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(sourceCharge);
				Assert("Calculations.Enabled must be the same as before copy", !destinationCharge.Calculations.Enabled_ForTestOnly);
			}

			destinationCharge.CopyPersistentValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(sourceCharge);
			Assert("Calculations.Enabled must be the same as before copying", destinationCharge.Calculations.Enabled_ForTestOnly);
		}

		public void TestCopyPersistentValuesFrom_OnlyFieldsValidToCopyAndInValidOrder()
		{
			var excludedFields = new[]
			{
					JobChargeSchema.JR_JH.Name,
					JobChargeSchema.JR_AL_APLine.Name,
					JobChargeSchema.JR_AL_ARLine.Name,
					JobChargeSchema.JR_AL_CFXLine.Name,
					JobChargeSchema.JR_E6.Name,
					JobChargeSchema.JR_E6_GatewaySellHeader.Name,
					JobChargeSchema.JR_OP_Product.Name,
					JobChargeSchema.JR_ChargeType.Name,
					JobChargeSchema.JR_DisplaySequence.Name,
					JobChargeSchema.JR_MarginPercentage.Name,
					JobChargeSchema.JR_OSCostGSTAmt.Name,
					JobChargeSchema.JR_IsCostTaxAmountOverridden.Name,
					JobChargeSchema.JR_SystemCreateTimeUtc.Name,
					JobChargeSchema.JR_SystemCreateUser.Name,
					JobChargeSchema.JR_SystemLastEditTimeUtc.Name,
					JobChargeSchema.JR_SystemLastEditUser.Name,
					JobChargeSchema.JR_CAL_APLine.Name,
					JobChargeSchema.JR_CAL_ARLine.Name,
					JobChargeSchema.JR_IsSpotCost.Name,
					JobChargeSchema.JR_ProFormaCost.Name,
					JobChargeSchema.JR_ProFormaRevenue.Name
				};
			var fieldsCannotBeSetDirectly = new string[] { JobChargeSchema.JR_ProFormaCost.Name, JobChargeSchema.JR_ProFormaRevenue.Name };

			var job = TestObjectCreator.CreateJob("S0001", null, 0, null, 0);
			job.JH_ParentTableCode = RatingHeaderSchema.Constants.Prefix;

			var exRate = job.ExchangeRates.AddNew();
			exRate.OrgType = ExchangeRateOrgTypeEnum.Debtor;
			exRate.JF_RX_NKRateCurrency = "USD";
			exRate.JF_IsTransformed = true;
			exRate.JF_BaseRate = 2.57m;
			exRate.JF_CFXPercent = 3.2m;

			Factory.Save();

			var wip = TestObjectCreator.CreateWIP();
			var accrual = TestObjectCreator.CreateWIP();
			var cfxLine = TestObjectCreator.CreateRevenueLine(Factory.New<Charge>(), Factory.New<ARInvoice>().PK);
			var cashAdvanceLine_AR = Factory.New<AccCashAdvanceRequestLine>();
			var cashAdvanceLine_AP = Factory.New<AccCashAdvanceRequestLine>();
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C0001");
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1);
			consolCost.E6_TaxDate = ZDate.Today.AddDays(-10);
			consolCost.E6_IsTaxAmountOverridden = true;
			var expectedFieldValuesByNames = new Dictionary<string, IZType>();
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_JH.Name, job.PK);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_AL_APLine.Name, accrual.PK);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_AL_ARLine.Name, wip.PK);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_AL_CFXLine.Name, cfxLine.PK);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_E6.Name, consolCost.PK);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_E6_GatewaySellHeader.Name, consolCost.PK);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_OP_Product.Name, ZGuid.NewZGuid());
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_ChargeType.Name, TestObjectCreator.CC1.AC_ChargeType);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_DisplaySequence.Name, (ZShort)234);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_MarginPercentage.Name, TestObjectCreator.CC1.AC_MarginPercentage);

			expectedFieldValuesByNames.Add(JobChargeSchema.JR_AC.Name, TestObjectCreator.CC1.PK);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_Desc.Name, (ZString)"Desc");
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_GB.Name, TestObjectCreator.NonCurrentBranch.PK);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_GC.Name, TestObjectCreator.NonCurrentCompany.PK);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_GE.Name, TestObjectCreator.NonCurrentDepartment.PK);

			expectedFieldValuesByNames.Add(JobChargeSchema.JR_OH_CostAccount.Name, TestObjectCreator.Creditor1.PK);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_OH_SellAccount.Name, TestObjectCreator.Debtor.PK);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_OA_SellInvoiceAddress.Name, TestObjectCreator.CreateAddress(TestObjectCreator.Debtor).PK);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_OC_SellInvoiceContact.Name, TestObjectCreator.CreateContact(TestObjectCreator.Debtor).PK);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_RX_NKCostCurrency.Name, TestObjectCreator.EUR.RX_Code);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_RX_NKSellCurrency.Name, TestObjectCreator.USD.RX_Code);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_OSCostExRate.Name, (ZDecimal)1.34m);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_OSSellExRate.Name, (ZDecimal)2.57m);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_InvoiceType.Name, (ZString)InvoiceTypesList.Codes.FreightInvoice_Batching);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_LineCFX.Name, (ZDecimal)3.2m);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_RX_NKSellInvoiceCurrency.Name, ZString.Empty);

			expectedFieldValuesByNames.Add(JobChargeSchema.JR_AT_CostGSTRate.Name, TestObjectCreator.GST1.PK);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_AT_SellGSTRate.Name, TestObjectCreator.GSTFREE1.PK);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_CostTaxDate.Name, ZDate.Today.AddDays(-10));
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_SellTaxDate.Name, ZDate.Today.AddDays(-5));
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_A9_CostVATClass.Name, Factory.NewWithValidTestData<AccInvMsg>().PK);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_A9_SellVATClass.Name, Factory.NewWithValidTestData<AccInvMsg>().PK);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_AW_CostWHTRate.Name, TestObjectCreator.WHTFREE1.PK);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_AW_SellWHTRate.Name, TestObjectCreator.WHT1.PK);

			expectedFieldValuesByNames.Add(JobChargeSchema.JR_OSCostAmt.Name, (ZDecimal)100m);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_LocalCostAmt.Name, (ZDecimal)101m);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_IsCostTaxAmountOverridden.Name, ZBool.True);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_OSCostGSTAmt.Name, (ZDecimal)10m);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_OSCostWHTAmt.Name, (ZDecimal)103m);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_EstimatedCost.Name, (ZDecimal)104m);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_DeclaredOSCostAmt.Name, (ZDecimal)105m);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_ProFormaCost.Name, ZBool.True);

			expectedFieldValuesByNames.Add(JobChargeSchema.JR_OSSellAmt.Name, (ZDecimal)200m);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_LocalSellAmt.Name, (ZDecimal)201m);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_OSSellWHTAmt.Name, (ZDecimal)203m);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_EstimatedRevenue.Name, (ZDecimal)204m);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_ProFormaRevenue.Name, ZBool.True);

			expectedFieldValuesByNames.Add(JobChargeSchema.JR_APInvoiceNum.Name, (ZString)"INV321");
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_APInvoiceDate.Name, ZDateTime.Today.AddDays(-10));
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_APDocumentReceivedDate.Name, ZDateTime.Today.AddDays(-12));
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_PaymentDate.Name, ZDateTime.Today.AddDays(-11));
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_PaymentType.Name, (ZString)ReceiptTypes.Cheque);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_AB.Name, TestObjectCreator.AUDBankAccount.PK);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_AK.Name, TestObjectCreator.AUDChequeBook.PK);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_ChequeNo.Name, (ZString)"004312");
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_CostReference.Name, (ZString)"Test Cost Ref");
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_SellReference.Name, (ZString)"Test Sell Ref");

			expectedFieldValuesByNames.Add(JobChargeSchema.JR_CostRated.Name, ZBool.True);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_CostRatingOverride.Name, ZBool.True);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_CostRatingOverrideComment.Name, (ZString)"Test Cost comment");
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_SellRated.Name, ZBool.True);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_SellRatingOverride.Name, ZBool.True);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_SellRatingOverrideComment.Name, (ZString)"Test Sell comment");

			expectedFieldValuesByNames.Add(JobChargeSchema.JR_IsIncludedInProfitShare.Name, ZBool.True);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_AgentDeclaredCostAmt.Name, (ZDecimal)2);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_AgentDeclaredSellAmt.Name, (ZDecimal)3);

			expectedFieldValuesByNames.Add(JobChargeSchema.JR_APLinePostingStatus.Name, (ZString)"SSS");
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_ARLinePostingStatus.Name, (ZString)"PPP");

			expectedFieldValuesByNames.Add(JobChargeSchema.JR_APNumberOfSupportingDocuments.Name, (ZByte)6);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_ARNumberOfSupportingDocuments.Name, (ZByte)4);

			expectedFieldValuesByNames.Add(JobChargeSchema.JR_OrderReference.Name, (ZString)"TEST ORDER");
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_PreventInvoicePrintGrouping.Name, ZBool.True);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_ProductQuantity.Name, (ZDecimal)77);

			expectedFieldValuesByNames.Add(JobChargeSchema.JR_GB_InternalBranch.Name, ZGuid.Empty);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_GE_InternalDept.Name, ZGuid.Empty);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_JH_InternalJob.Name, ZGuid.Empty);

			expectedFieldValuesByNames.Add(JobChargeSchema.JR_JR_RevenueLine.Name, ZGuid.Empty);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_LineType.Name, (ZString)"BTH");

			expectedFieldValuesByNames.Add(JobChargeSchema.JR_CostGovtChargeCode.Name, ZString.Empty);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_SellGovtChargeCode.Name, ZString.Empty);

			expectedFieldValuesByNames.Add(JobChargeSchema.JR_SellPlaceOfSupplyType.Name, ZString.Empty);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_CostPlaceOfSupplyType.Name, ZString.Empty);

			expectedFieldValuesByNames.Add(JobChargeSchema.JR_SellPlaceOfSupply.Name, ZString.Empty);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_CostPlaceOfSupply.Name, ZString.Empty);

			expectedFieldValuesByNames.Add(JobChargeSchema.JR_CostSupplyType.Name, ZString.Empty);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_SellSupplyType.Name, ZString.Empty);

			expectedFieldValuesByNames.Add(JobChargeSchema.JR_GB_CostTaxBranch.Name, TestObjectCreator.NonCurrentBranch.PK);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_GB_SellTaxBranch.Name, TestObjectCreator.NonCurrentBranch.PK);

			expectedFieldValuesByNames.Add(JobChargeSchema.JR_SystemCreateTimeUtc.Name, ZDateTime.Today);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_SystemCreateUser.Name, (ZString)"NEW");
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_SystemLastEditTimeUtc.Name, ZDateTime.Today);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_SystemLastEditUser.Name, (ZString)"NEW");

			expectedFieldValuesByNames.Add(JobChargeSchema.JR_IsAPCashAdvance.Name, ZBool.True);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_IsARCashAdvance.Name, ZBool.True);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_CAL_APLine.Name, cashAdvanceLine_AP.PK);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_CAL_ARLine.Name, cashAdvanceLine_AR.PK);
			expectedFieldValuesByNames.Add(JobChargeSchema.JR_IsSpotCost.Name, ZBool.True);

			var sourceCharge = Factory.New<Charge>();
			sourceCharge.Calculations.SuspendCalculations();
			var destinationCharge = Factory.New<Charge>();
			var job2 = TestObjectCreator.CreateJob("S0002", null, 0, null, 0);
			destinationCharge.JR_JH = job2.PK;
			this.AssertCopyPersistentValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(destinationCharge, sourceCharge, Charge.PersistentFieldsToCopyAndInValidOrder, excludedFields, expectedFieldValuesByNames,
					(destination, source) => destination.CopyPersistentValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(source), fieldsCannotBeSetDirectly);
		}

		public void TestInvoiceTypeSetterUpdatesSellInvoiceCurrency()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var orgHeader = testObjectCreator.CreateOrgHeader("TSTORG", false, true);

			var allGroup = orgHeader.CompanyData.InvoiceRollupOrGroups[0];
			allGroup.PG_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			allGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			allGroup.PG_TransportMode = "ALL";
			allGroup.PG_GroupOrSubTotal = "DEF";
			allGroup.PG_GroupOrSubtotalStyle = "DEF";
			allGroup.PG_InvoiceLineDisplayOption = "DEF";
			allGroup.PG_InvoicePostingStyle = Enterprise.ZArchitecture.Core.InvoicePostingOptionsList.Codes.DisbursementAndFinal;
			allGroup.PG_RX_NKInvoicePostingCurrency = "USD";

			var airGroup = orgHeader.CompanyData.InvoiceRollupOrGroups.AddNew();
			airGroup.PG_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			airGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Export;
			airGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
			airGroup.PG_GroupOrSubTotal = "DEF";
			airGroup.PG_GroupOrSubtotalStyle = "DEF";
			airGroup.PG_InvoiceLineDisplayOption = "DEF";
			airGroup.PG_InvoicePostingStyle = Enterprise.ZArchitecture.Core.InvoicePostingOptionsList.Codes.ForeignCurrencyInvoiceAndFinal;
			airGroup.PG_RX_NKInvoicePostingCurrency = "EUR";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "GBLON";
			shipment.JS_TransportMode = "AIR";
			shipment.JS_PackingMode = "";

			Factory.Save();

			Job job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			Charge charge = job.Charges.AddNew();
			charge.JR_AC = Factory.NewWithValidTestData<AccChargeCode>().PK;
			charge.JR_OH_SellAccount = orgHeader.PK;

			AssertEquals("Invoice type should be FIN", "FIN", charge.JR_InvoiceType);
			AssertEquals("Sell Invoice Currency should be set from matching RollUpGroup", "EUR", charge.JR_RX_NKSellInvoiceCurrency);

			charge.JR_RX_NKSellCurrency = "USD";

			AssertEquals("Invoice type should be CUR", "CUR", charge.JR_InvoiceType);
			AssertEquals("Sell Invoice Currency should be reset due to changed InvoiceType", string.Empty, charge.JR_RX_NKSellInvoiceCurrency);

			shipment.JS_TransportMode = "SEA";
			charge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			AssertEquals("Invoice type should be FIN", "FIN", charge.JR_InvoiceType);
			AssertEquals("Sell Invoice Currency should be set from matching RollUpGroup", "USD", charge.JR_RX_NKSellInvoiceCurrency);

			charge.JR_InvoiceType = InvoiceTypesList.Codes.InvoicePerTaxCode;

			AssertEquals("Invoice type should be ITC as set above", "ITC", charge.JR_InvoiceType);
			AssertEquals("Sell Invoice Currency should be preserved even set Invoice Type is not applicable the InvoicePostingStyle from matching RollUpGroup", "USD", charge.JR_RX_NKSellInvoiceCurrency);
		}

		public void TestHasValidDataForCostPosting()
		{
			var shipment = TestObjectCreator.CreateShipment("S1");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var org = Factory.New<OrgHeader>();

			var charge = job.Charges.AddNew();
			charge.JR_OH_CostAccount = org.PK;
			AssertEquals(false, charge.HasValidDataForCostPosting);

			charge.JR_APInvoiceDate = ZDateTime.Now;
			AssertEquals(false, charge.HasValidDataForCostPosting);

			charge.JR_APInvoiceNum = "1";
			AssertEquals(false, charge.HasValidDataForCostPosting);

			using (charge.Calculations.SuspendCalculations())
			{
				charge.JR_OSCostAmt = 100m;
				charge.JR_LocalCostAmt = 0m;
				AssertNotEquals("Pre-condition", 0m, charge.JR_OSCostAmt);
				AssertEquals("Pre-condition", 0m, charge.JR_LocalCostAmt);
				AssertEquals(false, charge.HasCostAmount);
				AssertEquals(false, charge.HasValidDataForCostPosting);
			}

			using (charge.Calculations.SuspendCalculations())
			{
				charge.JR_OSCostAmt = 0m;
				charge.JR_LocalCostAmt = 100m;
				AssertEquals("Pre-condition", 0m, charge.JR_OSCostAmt);
				AssertNotEquals("Pre-condition", 0m, charge.JR_LocalCostAmt);
				AssertEquals(false, charge.HasCostAmount);
				AssertEquals(false, charge.HasValidDataForCostPosting);
			}

			charge.JR_OSCostAmt = 0m;
			charge.JR_LocalCostAmt = 0m;
			AssertEquals("Pre-condition", 0m, charge.JR_OSCostAmt);
			AssertEquals("Pre-condition", 0m, charge.JR_LocalCostAmt);
			AssertEquals(false, charge.HasCostAmount);
			AssertEquals(false, charge.HasValidDataForCostPosting);

			charge.JR_OSCostAmt = 100m;
			AssertNotEquals("Pre-condition", 0m, charge.JR_OSCostAmt);
			AssertNotEquals("Pre-condition", 0m, charge.JR_LocalCostAmt);
			AssertEquals(true, charge.HasCostAmount);
			AssertEquals(true, charge.HasValidDataForCostPosting);

			charge.JR_InvoiceType = InvoiceTypesList.Codes.DoNotPost;
			AssertEquals(true, charge.HasValidDataForCostPosting);

			charge.JR_OH_CostAccount = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			AssertEquals(true, charge.HasValidDataForCostPosting);

			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid()))
			{
				AssertEquals(false, charge.HasValidDataForCostPosting);
			}

			charge.JR_OH_CostAccount = org.PK;

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_Ledger = LedgerTypes.AccountsPayable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			APInvoiceLine invoiceLine = (APInvoiceLine)invoice.Lines.AddNew();
			invoiceLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			charge.JR_AL_APLine = invoiceLine.PK;
			AssertEquals("It is posted already.", false, charge.HasValidDataForCostPosting);
		}

		public void TestHasValidDataForRevenuePosting()
		{
			var shipment = TestObjectCreator.CreateShipment("S1");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var org = Factory.New<OrgHeader>();

			var charge = job.Charges.AddNew();
			AssertEquals(false, charge.HasValidDataForRevenuePosting);

			job.JH_Status = JobHeaderStatus.WorkOnHold.Code;
			AssertEquals(false, charge.HasValidDataForRevenuePosting);

			job.JH_Status = JobHeaderStatus.InvoiceOnHold.Code;
			AssertEquals(false, charge.HasValidDataForRevenuePosting);

			job.JH_Status = JobHeaderStatus.Working.Code;
			AssertEquals(false, charge.HasValidDataForRevenuePosting);

			charge.JR_OH_SellAccount = org.PK;
			AssertEquals(false, charge.HasValidDataForRevenuePosting);

			charge.JR_OSSellAmt = 100m;
			AssertEquals(true, charge.HasValidDataForRevenuePosting);

			charge.JR_OH_SellAccount = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			AssertEquals(true, charge.HasValidDataForRevenuePosting);

			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid()))
			{
				AssertEquals(false, charge.HasValidDataForRevenuePosting);
			}

			charge.JR_OH_SellAccount = org.PK;

			charge.JR_InvoiceType = InvoiceTypesList.Codes.DoNotPost;
			AssertEquals(false, charge.HasValidDataForRevenuePosting);

			ARInvoice invoice = Factory.New<ARInvoice>();
			invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			ARInvoiceLine invoiceLine = (ARInvoiceLine)invoice.Lines.AddNew();
			invoiceLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			charge.JR_AL_APLine = invoiceLine.PK;
			AssertEquals("It is posted already.", false, charge.HasValidDataForRevenuePosting);
		}

		public void TestIsValidForAutoRevenuePosting()
		{
			var company = TestObjectCreator.CreateNewCompany("DMO");
			var organizationAU = TestObjectCreator.CreateOrgHeader("DAU", true, true);
			TestObjectCreator.CreateCustomsCodes(organizationAU, "AU", "ABN", "41065894724");
			var organizationRO = TestObjectCreator.CreateOrgHeader("DRO", true, true);
			TestObjectCreator.CreateCustomsCodes(organizationRO, "RO", "ABN", "00001");
			Factory.Save();

			var branchA01 = TestObjectCreator.CreateBranch("A01", company);
			var branchA02 = TestObjectCreator.CreateBranch("A02", company);
			branchA01.GB_OH_OrgProxy = organizationAU.PK;
			branchA02.GB_OH_OrgProxy = organizationRO.PK;
			company.GC_OH_OrgProxy = organizationAU.PK;
			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.EnableAutoJobRevenueJournals.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Tax))
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchA01.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var shipment = TestObjectCreator.CreateShipment("S00001");
				var job = TestObjectCreator.CreateJob(shipment);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, 0m, 20m);
				charge.JR_OH_SellAccount = organizationAU.PK;
				charge.JR_GB = branchA01.PK;
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				AssertEquals(true, charge.IsValidForAutoRevenuePosting);

				using (AccountingConfigurationRegistry.Instance.EnableAutoJobRevenueJournals.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Non))
				{
					AssertEquals(false, charge.IsValidForAutoRevenuePosting);
				}

				charge.JR_OH_SellAccount = organizationRO.PK;
				AssertEquals(false, charge.IsValidForAutoRevenuePosting);

				job.Dispose();
			}
		}

		public void TestCriticalValidationForCostPosting()
		{
			var shipment = TestObjectCreator.CreateShipment("S1");
			var job = TestObjectCreator.CreateJob(shipment, false);

			var invoice = Factory.New<APInvoice>();
			invoice.AH_Ledger = LedgerTypes.AccountsPayable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.AH_JH = job.PK;
			invoice.SubmittedFromInvoicingForm = true;
			invoice.AH_TransactionNum = "TEST_TRANSACTIONNUM";
			var invoiceLine = (APInvoiceLine)invoice.Lines.AddNew();
			invoiceLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			invoiceLine.AL_AC = TestObjectCreator.CC1.PK;
			invoiceLine.AL_JH = job.PK;
			invoiceLine.AL_RX_NKTransactionCurrency = "USD";
			invoiceLine.AL_ExchangeRate = 1.140188946m;
			invoiceLine.AL_LocalExTaxAmount = 920.90m;

			AssertNoExceptionThrown(() => { Factory.Save(); });
			AssertEquals("Invoice is Posted.", true, invoice.IsPosted);

			var newFactory = new BusinessObjectFactory();
			var newInvoice = newFactory.Load<APInvoice>(invoice.PK);
			AssertEquals("charge and line OS cost amount should be same", newInvoice.Lines[0].AL_OSAmount, -job.Charges[0].JR_OSCostAmt);
			AssertEquals("charge and line local cost amount should be same", newInvoice.Lines[0].AL_LineAmount, -job.Charges[0].JR_LocalCostAmt);
		}

		#region MatchedWithTNFJournalNum

		public void TestMatchWithTNFJournal()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment);

			var charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			charge.JR_OSCostAmt = 250m;
			charge.InvoicingJob.ExchangeRates[0].JF_BaseRate = 1m;
			charge.InvoicingJob.ExchangeRates[1].JF_BaseRate = 1m;
			charge.JR_OH_CostAccount = AALSHI.PK;
			charge.JR_APInvoiceNum = "T001";
			charge.InvoicingJob.ExchangeRates[0].JF_BaseRate = 1m;
			charge.InvoicingJob.ExchangeRates[1].JF_BaseRate = 1m;
			Factory.Save();

			AssertEquals("should not found any matched journal", ZString.Empty, charge.MatchedWithTNFJournalNum);

			var otherFactory1 = new BusinessObjectFactory();
			var journal1 = otherFactory1.NewWithValidTestData<APJournal>();
			journal1.AH_TransactionCategory = Constants.TransactionCategory.Codes.TransactionNotFound;
			journal1.AH_OH = AALSHI.PK;
			journal1.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			journal1.AH_ExchangeRate = 1m;
			journal1.AH_ChequeOrReference = "T001";
			journal1.AH_InvoiceAmount = 200;
			journal1.AH_OutstandingAmount = 200;
			journal1.AH_OSTotal = 200;
			otherFactory1.Save();

			var chargeInOtherFactory1 = otherFactory1.Load<Charge>(charge.PK);
			AssertEquals("should found the matched journal", journal1.AH_TransactionNum, chargeInOtherFactory1.MatchedWithTNFJournalNum);

			chargeInOtherFactory1.JR_APInvoiceNum = "I001";
			AssertEquals("should not found any matched journal", ZString.Empty, chargeInOtherFactory1.MatchedWithTNFJournalNum);

			chargeInOtherFactory1.JR_APInvoiceNum = "T001";
			journal1.AH_ChequeOrReference = "T002";
			var journal2 = otherFactory1.NewWithValidTestData<APJournal>();
			journal2.AH_TransactionCategory = Constants.TransactionCategory.Codes.TransactionNotFound;
			journal2.AH_OH = AALSHI.PK;
			journal2.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			journal2.AH_ExchangeRate = 1m;
			journal2.AH_ChequeOrReference = "T001";
			journal2.AH_InvoiceAmount = 0;
			journal2.AH_OutstandingAmount = 0;
			journal2.AH_OSTotal = 0;
			otherFactory1.Save();

			var otherFactory2 = new BusinessObjectFactory();
			var chargeInOtherFactory2 = otherFactory2.Load<Charge>(charge.PK);
			AssertEquals("should not found any matched journal", ZString.Empty, chargeInOtherFactory2.MatchedWithTNFJournalNum);
		}

		#endregion

		#region OnSaved

		public void TestOnSaved_EditSecurity()
		{
			string warningText = "Any changes will require authorization from your supervisor or accountant upon saving and/or posting.";

			DummyParent parent = Factory.New<DummyParent>();
			((DummyParentJobInvoicingSupporter)parent.InvoicingSupporter).fEditSecurityLock = true;

			Job job = new Job.Loader(parent).TryLoadOrCreateWithoutMutexForTestOnly();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			Charge charge = ((Job)parent.Job).Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;

			AssertEquals("Expected to have the '" + warningText + "' warning when locked.", true, HasWarning(charge, warningText));

			Factory.Save();

			AssertEquals("NOT Expected to have the '" + warningText + "' warning when locked.", false, HasWarning(charge, warningText));
		}

		#endregion

		#region OnSaving

		public void TestOnSaving_DisableAutoRatingCalculationLogsIfAmountChanged()
		{
			CalculationLog costingLog = new CalculationLog();
			costingLog.IsCosting = true;

			CalculationLogsWrapper costingWrapper = new CalculationLogsWrapper();
			costingWrapper.Logs.Add(costingLog);

			CalculationLog revenueLog = new CalculationLog();
			revenueLog.IsCosting = false;

			CalculationLogsWrapper revenueWrapper = new CalculationLogsWrapper();
			revenueWrapper.Logs.Add(revenueLog);

			Charge charge = CreateRatedFRTCharge(25, 35);

			CalculationLogsLoader.Save(charge, costingWrapper);
			CalculationLogsLoader.Save(charge, revenueWrapper);

			Factory.Save();
			AssertEquals("Precondition", false, CalculationLogsLoader.Load(charge, CalculationLogsLoader.LoadOption.Costing).IsDisabled);
			AssertEquals("Precondition", false, CalculationLogsLoader.Load(charge, CalculationLogsLoader.LoadOption.Revenue).IsDisabled);

			charge.JR_OSCostAmt = 26;
			Factory.Save();
			AssertEquals("Wrapper disabled", true, CalculationLogsLoader.Load(charge, CalculationLogsLoader.LoadOption.Costing).IsDisabled);
			AssertEquals("Wrapper not disabled", false, CalculationLogsLoader.Load(charge, CalculationLogsLoader.LoadOption.Revenue).IsDisabled);

			charge.JR_OSSellAmt = 36;
			Factory.Save();
			AssertEquals("Wrapper disabled", true, CalculationLogsLoader.Load(charge, CalculationLogsLoader.LoadOption.Costing).IsDisabled);
			AssertEquals("Wrapper disabled", true, CalculationLogsLoader.Load(charge, CalculationLogsLoader.LoadOption.Revenue).IsDisabled);

			charge = CreateRatedFRTCharge(0, 30);

			CalculationLogsLoader.Save(charge, costingWrapper);
			CalculationLogsLoader.Save(charge, revenueWrapper);

			AssertEquals("Precondition", false, CalculationLogsLoader.Load(charge, CalculationLogsLoader.LoadOption.Costing).IsDisabled);
			AssertEquals("Precondition", false, CalculationLogsLoader.Load(charge, CalculationLogsLoader.LoadOption.Revenue).IsDisabled);

			charge.JR_OSSellAmt = 31;
			Factory.Save();

			AssertEquals("Overriding sell with disable both wrappers", true, CalculationLogsLoader.Load(charge, CalculationLogsLoader.LoadOption.Costing).IsDisabled);
			AssertEquals("Overriding sell with disable both wrappers", true, CalculationLogsLoader.Load(charge, CalculationLogsLoader.LoadOption.Revenue).IsDisabled);
		}

		Charge CreateRatedFRTCharge(int cost, int sell)
		{
			var charge = Factory.NewWithValidTestData<Charge>();
			using (charge.SuppressAutoRatingOverride())
			{
				charge.JR_AC = Env.Registry.FreightChargeCode;
				charge.JR_OSCostAmt = cost;
				charge.JR_CostRated = true;
				charge.JR_OSSellAmt = sell;
				charge.JR_SellRated = true;
			}
			return charge;
		}

		#endregion

		#region Validate Row

		public void TestValidateRow()
		{
			string warningText = "Any changes will require authorization from your supervisor or accountant upon saving and/or posting.";

			DummyParent parent = Factory.New<DummyParent>();
			Charge charge = ((Job)parent.Job).Charges.AddNew();
			charge.StopGSTAmountOfUnApportionedChargeFromBeingOverridden.GetSuspender();
			charge.JR_IsCostTaxAmountOverridden = true;

			IDictionary<string, IList<string>> result = new SortedDictionary<string, IList<string>>();
			List<string> errors = new List<string>();

			string[] includedNonPersitentProperties =
			{
					BaseCharge.Schema.JR_OSCostGSTAmt_Calc
				};

			foreach (ZPropertyInfo info in charge.ZPropertyInfoHash)
			{
				if (!info.HasSetter)
				{
					continue;
				}

				if (!info.IsPersistent && !includedNonPersitentProperties.Contains(info.Name))
				{
					continue;
				}

				if (info is ZWrappedPropertyInfo)
				{
					continue;
				}

				if (info.Name == JobChargeSchema.Constants.JR_JH)
				{
					continue;
				}

				if (info.Name == "JR_RatingOverrideComment")
				{
					continue;
				}

				if (info.Name == "JR_APNumberOfSupportingDocuments")
				{
					continue;
				}

				if (info.Name == "JR_ARNumberOfSupportingDocuments")
				{
					continue;
				}

				if (info.Name == "JR_ProFormaCost")
				{
					continue;
				}

				if (info.Name == "JR_ProFormaRevenue")
				{
					continue;
				}

				if (info.Name == "JR_A9_SellVATClass")
				{
					continue;
				}

				if (info.Name == "JR_A9_CostVATClass")
				{
					continue;
				}

				if (info.Name == "JR_OA_SellInvoiceAddress")
				{
					continue;
				}

				if (info.Name == "JR_OC_SellInvoiceContact")
				{
					continue;
				}

				if (info.Name == "JR_GC")
				{
					continue;
				}

				if (info.Name == "JR_OSCostExRate")
				{
					continue;
				}

				if (info.Name == "JR_OSSellExRate")
				{
					continue;
				}

				if (info.Name == "JR_OSCostGSTAmt")
				{
					continue;
				}

				if (info.Name == "JR_SystemCreateTimeUtc")
				{
					continue;
				}

				if (info.Name == "JR_SystemCreateUser")
				{
					continue;
				}

				if (info.Name == "JR_SystemLastEditTimeUtc")
				{
					continue;
				}

				if (info.Name == "JR_SystemLastEditUser")
				{
					continue;
				}

				if (info.Name == "JR_IsSpotCost")
				{
					continue;
				}

				try
				{
					charge.StopGSTAmountOfUnApportionedChargeFromBeingOverridden.GetSuspender();
					IZType oldValue = info.Value;

					((DummyParentJobInvoicingSupporter)parent.InvoicingSupporter).fEditSecurityLock = true;

					charge.ClearRowNotifications();
					info.Value = GetValue(info, 1);
					info.Value = GetValue(info, 2);

					if (!HasWarning(charge, warningText))
					{
						errors.Add("Expected to have the '" + warningText + "' warning when locked.");
					}

					((DummyParentJobInvoicingSupporter)parent.InvoicingSupporter).fEditSecurityLock = false;

					charge.ClearRowNotifications();
					info.Value = GetValue(info, 3);

					if (HasWarning(charge, warningText))
					{
						errors.Add("NOT expected to have the '" + warningText + "' warning when NOT locked.");
					}

					info.Value = oldValue;
				}
				catch (Exception ex)
				{
					errors.Add(ex.ToString());
				}

				if (errors.Count > 0)
				{
					result.Add(info.Name, errors);
					errors = new List<string>();
				}
			}

			AssertGroupedErrorList("These properties have some errors...", result);
		}

		bool HasWarning(Charge charge, string text)
		{
			foreach (INotification notification in charge.RowWarnings)
			{
				if (text == notification.Message)
				{
					return true;
				}
			}
			return false;
		}

		IZType GetValue(ZPropertyInfo info, int seed)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			if (info.PropertyType == typeof(ZString))
			{
				char c = (char)(seed + 'a');
				return new ZString(c, Math.Min(info.MaxLength, 100));
			}
			else if (info.PropertyType == typeof(ZInt))
			{
				return (ZInt)seed;
			}
			else if (info.PropertyType == typeof(ZDecimal))
			{
				return new ZDecimal(seed);
			}
			else if (info.PropertyType == typeof(ZBool))
			{
				if (seed % 2 == 0)
				{
					return new ZBool(true);
				}
				else
				{
					return new ZBool(false);
				}
			}
			else if (info.PropertyType == typeof(ZBlob))
			{
				return new ZBlob(new byte[] { (byte)seed });
			}
			else if (info.PropertyType == typeof(ZByte))
			{
				return (ZByte)seed;
			}
			else if (info.PropertyType == typeof(ZDate))
			{
				return ZDate.Today.AddDays(-seed);
			}
			else if (info.PropertyType == typeof(ZDateTime))
			{
				return ZDateTime.Today.AddDays(-seed);
			}
			else if (info.PropertyType == typeof(ZShort))
			{
				return (ZShort)seed;
			}
			else if (info.PropertyType == typeof(ZGuid))
			{
				return ZGuid.NewZGuid();
			}
			else
			{
				throw new NotSupportedException("and what am i supposed to do with a '" + info.PropertyType.FullName + "'?");
			}
		}

		class DummyParent : CommonShipment, IJobInvoicingPlugIn
		{
			public DummyParent(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
				Job header = new Job.Loader(this).TryLoadOrCreateWithoutMutexForTestOnly();
				header.Parent = this;
			}

			protected override CommonShipmentInvoicingSupporter GetNewInvoicingSupporter()
			{
				return new DummyParentJobInvoicingSupporter(this);
			}
		}

		class DummyParentJobInvoicingSupporter : CommonShipmentInvoicingSupporter
		{
			public DummyParentJobInvoicingSupporter(DummyParent parent)
				: base(parent)
			{
			}

			public override bool EditSecurityLock
			{
				get
				{
					return fEditSecurityLock;
				}
			}
			public bool fEditSecurityLock;
		}

		#endregion

		#region IsTaxApplicable

		public void TestDocDataProviderReflectorMember_ForMacroList()
		{
			var docDataProviderReflector = new DocumentEngine.ReflectiveFieldMap.DocDataProviderReflector(typeof(Charge));
			var expectedProps = new List<string> {
					"JR_Calc_HasDebtorAcceptedThisSellCharge",
					"JR_OSSellInvoiceAmt",
					"JR_OSSellInvoiceAmt_ForDisplay",
					"JR_OSSellInvoiceExRate",
					"JR_OSSellInvoiceExRate_ForDisplay",
					"MatchedWithTNFJournalNum",
					"CostDueOverseasAgent",
					"IsProfitShareCharge",
					"ProfitShareForCharge",
					"IsSelectedForAutoPopulation",
					"JR_RelatedConsolRef",
					"JR_Sell_LocalSellAmount",
					"JR_Sell_InvoiceOSAmount",
					"JR_OSSellInvoiceGSTAmt",
					"JR_OSSellInvoiceGSTAmt_ForDisplay",
					"ChargeCodePrintSequence",
					"IsUsedForApportionment",
					"IsSelectedForImport",
					"JR_TotalLocalRevenue",
					"ShouldDisplayTaxApplicabilityForRatingHeader",
					"ChargeableUnits",
					"GrossWeight",
					"GrossVolume",
					"ChargeableRateForRevenueApportionment"
				};
			var errors = new List<string>();
			CombineAssertions(() =>
			{
				foreach (string propName in expectedProps)
				{
					Assert(FormattableString.Invariant($"should contain prop:{propName}"), docDataProviderReflector.Members.Any(member => member.GetFullPath() == propName));
				}
			});
		}

		public void TestShouldDisplayTaxApplicabilityForRatingHeader()
		{
			var chargeCode = Factory.Load<AccChargeCode>(RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value);

			var jobPlugIn = Freight.QuotedBookings.Business.QuotedBooking.New(Freight.Integration.QuoteBookingType.SpotQuote, Factory) as IJobInvoicingPlugIn;
			AssertShouldDisplayTaxApplicabilityForRatingHeader(true);

			jobPlugIn = TestObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.AgencyBooking);
			AssertShouldDisplayTaxApplicabilityForRatingHeader(false);

			void AssertShouldDisplayTaxApplicabilityForRatingHeader(bool shouldDisplayTax)
			{
				var job = TestObjectCreator.CreateJob(jobPlugIn, false);

				Charge charge = TestObjectCreator.CreateCharge(job, chargeCode);
				Assert(!charge.ShouldDisplayTaxApplicabilityForRatingHeader);

				chargeCode.ClearGSTRateCacheForTesting();
				chargeCode.AC_AT_GSTRate = TestObjectCreator.GST1.PK;
				Factory.Save();

				AssertEquals(shouldDisplayTax, charge.ShouldDisplayTaxApplicabilityForRatingHeader);
			}
		}

		public void TestShouldDisplayTaxApplicabilityForRatingHeaderWithCustomsStatus()
		{
			var quotedBooking = Freight.QuotedBookings.Business.QuotedBooking.New(Freight.Integration.QuoteBookingType.SpotQuote, Factory);
			AccChargeCode chargeCode = Factory.Load<AccChargeCode>(RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value);

			var job = TestObjectCreator.CreateJob(quotedBooking, false);
			Charge charge = TestObjectCreator.CreateCharge(job, chargeCode);

			AccChargeTaxOverride taxOverride1 = chargeCode.TaxOverrides.AddNew();
			taxOverride1.AO_CostSellAll = "ALL";
			taxOverride1.AO_Direction = "ALL";
			taxOverride1.AO_IncoTerm = "ALL";
			taxOverride1.AO_JobType = "ALL";
			taxOverride1.AO_Origin = "ALL";
			taxOverride1.AO_Destination = "ALL";
			taxOverride1.AO_TaxRegCntryOrGroup = "ALL";
			taxOverride1.AO_AT = TestObjectCreator.GSTFREE1.PK;

			AccChargeTaxOverride taxOverride2 = chargeCode.TaxOverrides.AddNew();
			taxOverride2.AO_CostSellAll = "ALL";
			taxOverride2.AO_Direction = "ALL";
			taxOverride2.AO_IncoTerm = "ALL";
			taxOverride2.AO_JobType = "ALL";
			taxOverride2.AO_Origin = "ALL";
			taxOverride2.AO_Destination = "ALL";
			taxOverride2.AO_TaxRegCntryOrGroup = "ALL";
			taxOverride2.AO_AT = TestObjectCreator.GST1WithDates.PK;
			taxOverride2.AO_TransportMode = "SEA";

			Factory.Save();
			chargeCode.ClearGSTRateCacheForTesting();

			Assert(!charge.ShouldDisplayTaxApplicabilityForRatingHeader);

			quotedBooking.TransportMode = "SEA";
			Assert(charge.ShouldDisplayTaxApplicabilityForRatingHeader);

			taxOverride1.AO_CostSellAll = AccChargeTaxOverrideLookups.Cost;
			taxOverride2.AO_CostSellAll = AccChargeTaxOverrideLookups.Cost;
			Factory.Save();
			chargeCode.ClearGSTRateCacheForTesting();
			Assert("Cost tax rate should be taken into account.", charge.ShouldDisplayTaxApplicabilityForRatingHeader);

			charge.JR_CostTaxDate = TestObjectCreator.GST1WithDates_DateWithNoRate;
			Assert("Tax Date points to period with no rate", !charge.ShouldDisplayTaxApplicabilityForRatingHeader);

			taxOverride1.AO_CostSellAll = AccChargeTaxOverrideLookups.Revenue;
			taxOverride2.AO_CostSellAll = AccChargeTaxOverrideLookups.Revenue;
			Factory.Save();
			chargeCode.ClearGSTRateCacheForTesting();
			Assert("Revenue tax rate should be taken into account.", charge.ShouldDisplayTaxApplicabilityForRatingHeader);

			charge.JR_SellTaxDate = TestObjectCreator.GST1WithDates_DateWithNoRate;
			Assert("Tax Date points to period with no rate", !charge.ShouldDisplayTaxApplicabilityForRatingHeader);
		}

		public void TestShouldDisplayTaxApplicabilityForRatingHeaderConsidersPlaceOfSupply()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var newBranch = TestObjectCreator.CreateNewBranch(GlbCompany.CurrentCompany, "BR1");
				newBranch.GB_OH_OrgProxy = TestObjectCreator.ActiveOrg.PK;
				GlbCompany.CurrentCompany.Factory.Save();
				using (EnvProxy.Instance.SetTemporaryUserContext(Env.CurrentUser.LoginName, newBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var charge = PrepareTaxDefaultDataSetupForIndia(creditorDebtorAndOrgProxyAreInSameState: false, addALXRule: false) as Charge;

					charge.ChargeCode.ClearGSTRateCacheForTesting();
					charge.JR_CostPlaceOfSupply = "DL";
					Assert(charge.ShouldDisplayTaxApplicabilityForRatingHeader);

					charge.ChargeCode.ClearGSTRateCacheForTesting();
					charge.JR_CostPlaceOfSupply = "";
					Assert(!charge.ShouldDisplayTaxApplicabilityForRatingHeader);

					charge.ChargeCode.ClearGSTRateCacheForTesting();
					charge.JR_SellPlaceOfSupply = "DL";
					Assert(charge.ShouldDisplayTaxApplicabilityForRatingHeader);

					charge.ChargeCode.ClearGSTRateCacheForTesting();
					charge.JR_SellPlaceOfSupply = "";
					Assert(!charge.ShouldDisplayTaxApplicabilityForRatingHeader);
				}
			}
		}

		#endregion

		public void TestShouldValidateJobHeaderWhenChargeAmountsChange()
		{
			JobProfitLossReasonCodeCollection plReasonCodes = new JobProfitLossReasonCodeCollection();
			JobProfitLossReasonCode plReasonCode = plReasonCodes.AddNew();
			plReasonCode.Code = "TST";
			plReasonCode.Description = (NoResString)"Test";
			AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, plReasonCodes);

			JobProfitLossRequiringReasonParameters plRequiringReasonParameters = new JobProfitLossRequiringReasonParameters();
			plRequiringReasonParameters.ProfitThreshold = 10M;
			plRequiringReasonParameters.JobStatusCollection.AddNew().Code = TestJob.JobStatusList[TestJob.JobStatusList.Count - 1].Code;
			AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, plRequiringReasonParameters);

			var parent = Factory.New<DummyParent>();
			Job job = new Job.Loader(parent).TryLoadOrCreateWithoutMutexForTestOnly();
			job.JH_JobNum = "S00001000";
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_ProfitLossReasonCode = "";
			job.LocalChargesPK = TestObjectCreator.LocalClient.PK;

			Charge charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OSCostAmt = 100m;
			charge.JR_OSSellAmt = 100m;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			Factory.Save();
			job.RunPreSaveValidation();
			Assert(!job.ShouldValidateOnSave);
			charge.JR_OSCostAmt = 50m;
			Assert(job.ShouldValidateOnSave);
		}

		void SetCurrentCompanyCountryCode(string code)
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.StartsWith, code)).Code;
		}

		void SetCurrentCompanyCurrencyCode(string code)
		{
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.Equal, code)).RX_Code;
		}

		#region IApportionedCharge Members

		public void TestIApportionedCharge_ChargeableUnits()
		{
			ForwardingConsol consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C00001");
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Factory.Save();

			ForwardingShipment shipment1 = TestObjectCreator.CreateShipment("S00001", consol);
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment1.JS_ActualChargeable = 200.0m;
			shipment1.JS_ActualWeight = 500m;
			shipment1.JS_UnitOfWeight = Core.Constants.Weight.Pounds;
			shipment1.JS_ActualVolume = 0m;
			shipment1.JS_UnitOfVolume = Core.Constants.Volume.CubicFeet;

			ForwardingShipment shipment2 = TestObjectCreator.CreateShipment("S00002", consol);
			shipment2.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment2.JS_ActualChargeable = 200.0m;
			shipment2.JS_ActualWeight = 500m;
			shipment2.JS_UnitOfWeight = Core.Constants.Weight.Pounds;
			shipment2.JS_ActualVolume = 0m;
			shipment2.JS_UnitOfVolume = Core.Constants.Volume.Litre;

			Job job = TestObjectCreator.CreateJob(shipment1, false);
			Charge charge = job.Charges.AddNew();

			var apps = consol.GetApportionments();
			JobConsolCost jobConsolCost = apps.CostsCollection.TryAddNew();
			charge.JR_E6 = jobConsolCost.PK;

			AssertEquals(226.796m, ((IApportionedCharge)charge).ChargeableUnits);
			apps.ReleaseMutexes();
		}

		public void TestIApportionedCharge_GrossWeight()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			Job testJob = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			Charge charge = testJob.Charges.AddNew();
			testJob.PlugInData = shipment;

			shipment.JS_ActualWeight = 35m;
			AssertEquals(35m, ((IApportionedCharge)charge).GrossWeight);
		}

		public void TestIApportionedCharge_GrossVolume()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var testJob = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			var charge = testJob.Charges.AddNew();
			testJob.PlugInData = shipment;

			shipment.JS_ActualVolume = 4m;
			AssertEquals(4m, ((IApportionedCharge)charge).GrossVolume);
		}

		public void TestIApportionedCharge_IsUsedForApportionment()
		{
			var charge = Factory.New<Charge>();
			charge.IsUsedForApportionment = false;
			AssertEquals(false, (((IApportionedCharge)charge).IsUsedForApportionment));

			charge.IsUsedForApportionment = true;
			AssertEquals(true, (((IApportionedCharge)charge).IsUsedForApportionment));
		}

		#endregion

		#region Lookups

		public void TestPaymentTypesLookup()
		{
			Charge charge = Factory.New<Charge>();

			foreach (ICodeDescription pair in charge.PaymentTypes)
			{
				Assert("Not expected to contain " + pair.Code, pair.Code == ReceiptTypes.Cheque || pair.Code == ReceiptTypes.Cash || pair.Code == ReceiptTypes.CreditCard || pair.Code == ReceiptTypes.DirectDebit || pair.Code == ReceiptTypes.EFT || pair.Code == ReceiptTypes.ScheduledEFT || pair.Code == ReceiptTypes.CollectionRequest || pair.Code == ReceiptTypes.eNettDirectDebit || pair.Code == ReceiptTypes.eNettCreditCard);
			}
		}

		public void TestIsSavedByFactory()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var testJob = Factory.NewJobForTesting<Job>();
			var charge = testJob.Charges.AddNew();
			var jobCharge = Factory.Load<JobCharge>(charge.PK);

			AssertNull(testJob.PlugInData);
			Assert(testJob.IsSavedByFactory);
			Assert(charge.IsSavedByFactory);
			AssertEquals("JobCharge.IsSavedByFactory should be the same as Charge", charge.IsSavedByFactory, jobCharge.IsSavedByFactory);

			testJob.PlugInData = shipment;
			AssertNotNull(testJob.PlugInData);
			Assert(testJob.IsSavedByFactory);
			Assert(charge.IsSavedByFactory);
			AssertEquals("JobCharge.IsSavedByFactory should be the same as Charge", charge.IsSavedByFactory, jobCharge.IsSavedByFactory);

			((IBusinessObjectInternals)shipment).MarkAsDeleted();
			AssertNotNull(testJob.PlugInData);
			Assert(!testJob.IsSavedByFactory);
			Assert(!charge.IsSavedByFactory);
			AssertEquals("JobCharge.IsSavedByFactory should be the same as Charge", charge.IsSavedByFactory, jobCharge.IsSavedByFactory);

			var consolCost = Factory.New<JobConsolCost>();
			var incompleteInvoice = Factory.New<APInvoice>();
			incompleteInvoice.AH_Ledger = LedgerTypes.IncompleteTransactions;
			consolCost.ParentAPInvoice = incompleteInvoice;

			testJob = Factory.NewJobForTesting<Job>();
			charge = testJob.Charges.AddNew();
			jobCharge = Factory.Load<JobCharge>(charge.PK);

			AssertNull(testJob.PlugInData);
			Assert(charge.IsSavedByFactory);
			AssertEquals("JobCharge.IsSavedByFactory should be the same as Charge", charge.IsSavedByFactory, jobCharge.IsSavedByFactory);

			charge.JR_E6 = consolCost.PK;
			Assert(!charge.IsSavedByFactory);
			AssertEquals("JobCharge.IsSavedByFactory should be the same as Charge", charge.IsSavedByFactory, jobCharge.IsSavedByFactory);
		}

		public void TestCommentChargeCodeIncludedInChargeList()
		{
			AccChargeCode testChargeCode = Factory.New<AccChargeCode>();
			testChargeCode.AC_Code = "TESTCMT";
			testChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			testChargeCode.AC_MarginPercentage = 0.0m;
			testChargeCode.AC_DepartmentFilterList = "ALL";
			testChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;

			Charge testCharge = Factory.New<Charge>();
			var chargeCodes = new AccChargeCodeCollection(Factory, testCharge.Lookups.ChargeCodes.CompleteFilter);
			chargeCodes.Load();

			AccChargeCode retrievedCharge = chargeCodes.FindByPK(testChargeCode.PK) as AccChargeCode;
			AssertNotNull(retrievedCharge);
			AssertEquals(Core.Constants.ChargeType.Comment, retrievedCharge.AC_ChargeType);
			AssertEquals(testChargeCode.AC_Code, retrievedCharge.AC_Code);
		}

		public void TestCanClearAutoratedCostOrSell_CommentChargeCode()
		{
			var shipment = Factory.New<ForwardingShipment>();
			Factory.Save();
			var testJob = Factory.NewJobForTesting<Job>();
			var charge = testJob.Charges.AddNew();
			var adapterIDs = new ZString[] { shipment.RatingAdapter.OperationalJobCode };

			AssertEquals("Prerequisites: charge is created with CostRatingBehavior automatically be REA", JobChargeLookups.ReAutorateCharge, charge.JR_Calc_CostRatingBehavior);
			Assert("Prerequisites: rating not overriden", !charge.JR_CostRatingOverride && !charge.JR_SellRatingOverride);
			Assert("Prerequisites: not posted or apportioned", !charge.JR_IsApportioned && !charge.JR_IsPosted);
			Assert("Prerequisites: has no payment basis", !charge.CostPaymentBases.Any());
			Assert("Prerequisites: has no payment basis", !charge.SellPaymentBases.Any());

			Assert("Without payment basis but with REA, can clear autorated cost", charge.CanReautorate(CostSell.Cost, adapterIDs));
			Assert("Without payment basis but with REA, can clear autorated sell", charge.CanReautorate(CostSell.Revenue, adapterIDs));

			charge.JR_Calc_CostRatingBehavior = JobChargeLookups.CreateNewCharge;
			charge.JR_Calc_SellRatingBehavior = JobChargeLookups.CreateNewCharge;
			Assert("Without payment basis but with NEW, cannot clear autorated cost", !charge.CanReautorate(CostSell.Cost, adapterIDs));
			Assert("Without payment basis but with NEW, cannot clear autorated sell", !charge.CanReautorate(CostSell.Revenue, adapterIDs));

			charge.JR_Calc_CostRatingBehavior = JobChargeLookups.ReAutorateCharge;
			charge.JR_Calc_SellRatingBehavior = JobChargeLookups.ReAutorateCharge;
			var paymentBasis = TestObjectCreator.CreatePaymentBasis(20, adapterIDs[0]);
			charge.AddPaymentBases(new[] { paymentBasis }, true);
			charge.AddPaymentBases(new[] { paymentBasis }, false);

			Assert("Should be possible to clear autorated cost", charge.CanReautorate(CostSell.Cost, adapterIDs));
			Assert("Should be possible to clear autorated sell", charge.CanReautorate(CostSell.Revenue, adapterIDs));

			charge.JR_AC = TestObjectCreator.CommentChargeCode.PK;

			Assert("Prerequisites: still has payment basis", charge.CostPaymentBases.Any());
			Assert("Prerequisites: still has payment basis", charge.SellPaymentBases.Any());
			Assert("Should still be possible to clear autorated cost", charge.CanReautorate(CostSell.Cost, adapterIDs));
			Assert("Should still be possible to clear autorated sell", charge.CanReautorate(CostSell.Revenue, adapterIDs));

			charge.JR_Calc_CostRatingBehavior = JobChargeLookups.CreateNewCharge;
			charge.JR_Calc_SellRatingBehavior = JobChargeLookups.CreateNewCharge;
			Assert("Cannot clear autorated cost", !charge.CanReautorate(CostSell.Cost, adapterIDs));
			Assert("Cannot clear autorated sell", !charge.CanReautorate(CostSell.Revenue, adapterIDs));
		}

		#endregion

		#region Invoice Calculator

		public void TestInvoiceCalculator()
		{
			BusinessObjectFactory orgFactory = new BusinessObjectFactory();
			OrgHeader debtor = orgFactory.NewWithValidTestData<OrgHeader>();
			debtor.OH_IsDebtor = true;
			debtor.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			OrgInvoiceRollupOrGroup group = debtor.CompanyData.InvoiceRollupOrGroups.AddNew();
			group.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			group.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			group.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			group.PG_InvoicePostingStyle = InvoicePostingOptionsList.Codes.FinalInvoiceOnly;
			orgFactory.Save();

			Job job = Factory.NewJobForTesting<Job>();

			Charge charge = job.Charges.AddNew();
			AccChargeCode testChargeCode = Factory.New<AccChargeCode>();
			charge.JR_AC = testChargeCode.PK;

			AssertEquals("No debtor selected, Invoice Type should not be set", ZString.Empty, charge.JR_InvoiceType);
			charge.JR_OH_SellAccount = debtor.PK;
			AssertEquals("Changing debtor should recalculate invoice type", InvoiceTypesList.Codes.FinalInvoice, charge.JR_InvoiceType);

			AccChargeCode otherChargeCode = Factory.New<AccChargeCode>();
			charge.JR_InvoiceType = ZString.Empty;
			charge.JR_AC = otherChargeCode.PK;
			AssertEquals("Changing charge code should recalculate invoice type", InvoiceTypesList.Codes.FinalInvoice, charge.JR_InvoiceType);

			charge.JR_InvoiceType = ZString.Empty;
			charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			AssertEquals("Changing currency should recalculate invoice type", InvoiceTypesList.Codes.FinalInvoice, charge.JR_InvoiceType);
		}

		#endregion

		#region Property Info Override Tests

		public void TestJR_DisplaySequenceInfo()
		{
			Job job = CreateJob("Z00001012", ZECTRA, true, 5M, ABIGAS, true, 10M);
			Charge charge1 = job.Charges.AddNew();
			AccChargeCode testChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			charge1.JR_AC = testChargeCode.PK;
			charge1.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;

			AssertEquals("Display Sequence Info must be editable", false, charge1.JR_DisplaySequenceInfo.ReadOnly);

			InvoicingBase invoice = new ChargePoster(Factory).Post(charge1);
			Factory.Save();

			AssertEquals("Display Sequence Info must be readonly", true, charge1.JR_DisplaySequenceInfo.ReadOnly);

			JobInvoicingReverser reverser = new JobInvoicingReverser(job);
			reverser.ReverseAllInvoices("test", "tst");
			reverser.ReversingFactory.Save();

			AssertEquals("Display Sequence Info must be editable", false, charge1.JR_DisplaySequenceInfo.ReadOnly);
		}

		#endregion

		public void TestJR_DisplaySequence_IncrementsWhenCopiedWithExistingChargesOnJob()
		{
			TestJob.Charges.RemoveAndDeleteAll();

			var jobCharge1 = TestJob.Charges.AddNew();
			jobCharge1.JR_AC = TestObjectCreator.CC1.PK;

			var jobCharge2 = TestJob.Charges.AddNew();
			jobCharge2.JR_AC = TestObjectCreator.CC2.PK;
			jobCharge2.JR_DisplaySequence = 3;

			var quote = Factory.NewWithValidTestData<Quote>();
			quote.TH_QuoteNumber = TestObjectCreator.GetRandomString(4);
			quote.TH_OneTimeQuote = true;
			Factory.Save();

			var quoteJob = new Job.Loader(quote).TryLoadOrCreateWithoutMutexForTestOnly();
			var quoteCharge1 = quoteJob.Charges.AddNew();
			var quoteCharge2 = quoteJob.Charges.AddNew();
			quoteCharge2.JR_DisplaySequence = 5;

			AssertEquals("Pre-condition", (ZShort)1, quoteCharge1.JR_DisplaySequence);
			AssertEquals("Pre-condition", (ZShort)5, quoteCharge2.JR_DisplaySequence);

			TestJob.LocalChargesPK = ZGuid.Empty;
			TestJob.JH_TH_NKQuoteNumber = quote.TH_QuoteNumber;

			AssertEquals("Expected charges to be copied from the quote", 4, TestJob.Charges.Count);
			AssertEquals("Existing charges on the job should not have display sequence changed", (ZShort)1, TestJob.Charges[0].JR_DisplaySequence);
			AssertEquals((ZShort)3, TestJob.Charges[1].JR_DisplaySequence);
			AssertEquals("Expected 4 as we should add 1 from the quote to 3, the last sequence number", (ZShort)4, TestJob.Charges[2].JR_DisplaySequence);
			AssertEquals((ZShort)5, TestJob.Charges[3].JR_DisplaySequence);
		}

		public void TestCanUpdateSell()
		{
			Job job = CreateJob("Z00001012", ZECTRA, true, 5M, ABIGAS, true, 10M);
			Charge chrg = job.Charges.AddNew();
			chrg.JR_AC = Env.Registry.FreightChargeCode;

			AssertEquals(false, chrg.JR_IsRevenuePosted);
			AssertEquals(false, chrg.IsDisbursementCharge);

			IQuickCalculatorCharge quickCalculator = chrg;
			AssertEquals(true, quickCalculator.CanUpdateSell);

			chrg.JR_ChargeType = Core.Constants.ChargeType.Disbursement;
			AssertEquals(false, quickCalculator.CanUpdateSell);

			chrg.JR_AL_ARLine = Factory.New<AccTransactionLines>().PK;
			chrg.ARLine.AL_LineType = TransactionLineTypes.Revenue;
			AssertEquals(true, chrg.JR_IsRevenuePosted);
			AssertEquals(false, quickCalculator.CanUpdateSell);

			chrg.JR_ChargeType = Core.Constants.ChargeType.Revenue;
			AssertEquals(false, quickCalculator.CanUpdateSell);
		}

		public void TestJR_SellPlaceOfSupply_ReadOnly()
		{
			var charge1 = Factory.NewWithValidTestData<Charge>();
			Assert(!charge1.IsRevenuePosted);
			Assert(!charge1.JR_SellPlaceOfSupplyInfo.ReadOnly);
			charge1.JR_AL_ARLine = Factory.New<AccTransactionLines>().PK;
			charge1.ARLine.AL_LineType = TransactionLineTypes.Revenue;
			Assert(charge1.JR_IsRevenuePosted);
			Assert(charge1.JR_SellPlaceOfSupplyInfo.ReadOnly);

			var charge2 = Factory.NewWithValidTestData<Charge>();
			Assert(!charge2.JR_SellPlaceOfSupplyInfo.ReadOnly);
			AccountingConfigurationRegistry.Instance.PreventOperatorFromChangingRatedLine.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			charge2.JR_SellRated = true;
			Assert(charge2.JR_SellPlaceOfSupplyInfo.ReadOnly);
			charge2.JR_SellRated = false;
			Assert(!charge2.JR_SellPlaceOfSupplyInfo.ReadOnly);
			charge2.JR_SellRated = true;
			AccountingConfigurationRegistry.Instance.PreventOperatorFromChangingRatedLine.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			Assert(!charge2.JR_SellPlaceOfSupplyInfo.ReadOnly);

			var newFactory = new BusinessObjectFactory();
			var charge3 = newFactory.NewWithValidTestData<JobCharge>();
			Assert(!charge3.IsInDatabaseAndReadyForRevenuePosting);
			Assert(!charge3.JR_SellPlaceOfSupplyInfo.ReadOnly);
			charge3.Job.JH_Status = JobHeaderStatus.JobReadyForRevenuePosting.Code;
			newFactory.Save();
			Assert(charge3.IsInDatabaseAndReadyForRevenuePosting);
			Assert(charge3.JR_SellPlaceOfSupplyInfo.ReadOnly);
		}

		public void TestJR_CostPlaceOfSupply_ReadOnly()
		{
			var charge1 = Factory.NewWithValidTestData<Charge>();
			Assert(!charge1.IsCostPosted);
			Assert(!charge1.JR_CostPlaceOfSupplyInfo.ReadOnly);
			charge1.JR_AL_APLine = Factory.New<AccTransactionLines>().PK;
			charge1.APLine.AL_LineType = TransactionLineTypes.Cost;
			Assert(charge1.IsCostPosted);
			Assert(charge1.JR_CostPlaceOfSupplyInfo.ReadOnly);

			var charge2 = Factory.NewWithValidTestData<Charge>();
			Assert(!charge2.JR_CostPlaceOfSupplyInfo.ReadOnly);
			AccountingConfigurationRegistry.Instance.PreventOperatorFromChangingRatedLine.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			charge2.JR_CostRated = true;
			Assert(charge2.JR_CostPlaceOfSupplyInfo.ReadOnly);
			charge2.JR_CostRated = false;
			Assert(!charge2.JR_CostPlaceOfSupplyInfo.ReadOnly);
			charge2.JR_CostRated = true;
			AccountingConfigurationRegistry.Instance.PreventOperatorFromChangingRatedLine.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			Assert(!charge2.JR_CostPlaceOfSupplyInfo.ReadOnly);

			var charge3 = Factory.NewWithValidTestData<Charge>();
			Assert(!charge3.JR_CostPlaceOfSupplyInfo.ReadOnly);
			Assert(!charge3.JR_IsApportioned);
			charge3.JR_E6 = ZGuid.NewZGuid();
			Assert(charge3.JR_IsApportioned);
			Assert(charge3.JR_CostPlaceOfSupplyInfo.ReadOnly);

			var charge4 = Factory.New<JobCharge>();
			Assert(!charge4.IsRevenuePostedWithManualJobRevenueJournal);
			Assert(!charge4.JR_CostPlaceOfSupplyInfo.ReadOnly);
			var header = Factory.New<AccTransactionHeader>();
			header.AH_TransactionType = TransactionTypes.JobRevenueJournal;
			var line = Factory.New<AccTransactionLines>();
			charge4.JR_AL_ARLine = line.PK;
			line.AL_AH = header.PK;
			Assert(charge4.IsRevenuePostedWithManualJobRevenueJournal);
			Assert(charge4.JR_CostPlaceOfSupplyInfo.ReadOnly);

			var newFactory = new BusinessObjectFactory();
			var charge5 = newFactory.NewWithValidTestData<JobCharge>();
			Assert(!charge5.IsInDatabaseAndReadyForCostPosting);
			Assert(!charge5.JR_CostPlaceOfSupplyInfo.ReadOnly);
			charge5.Job.JH_Status = JobHeaderStatus.JobReadyForCostPosting.Code;
			newFactory.Save();
			Assert(charge5.IsInDatabaseAndReadyForCostPosting);
			Assert(charge5.JR_CostPlaceOfSupplyInfo.ReadOnly);
		}

		public void TestPlaceOfSupplyIsReadOnly()
		{
			var charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_AC = TestObjectCreator.CommentChargeCode.PK;
			Assert(!charge.IsCostPosted);
			Assert(!charge.JR_CostPlaceOfSupplyInfo.ReadOnly);
			Assert(!charge.JR_SellPlaceOfSupplyInfo.ReadOnly);

			charge.JR_AC = TestObjectCreator.FRT.PK;
			Assert(!charge.JR_SellPlaceOfSupplyInfo.ReadOnly);
			Assert(!charge.JR_CostPlaceOfSupplyInfo.ReadOnly);

			var revChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			revChargeCode.AC_ChargeType = Constants.ChargeType.Revenue;
			charge.JR_AC = revChargeCode.PK;
			Assert(charge.JR_CostPlaceOfSupplyInfo.ReadOnly);
			Assert(!charge.JR_SellPlaceOfSupplyInfo.ReadOnly);
		}

		public void TestCanUpdateCost()
		{
			Job job = CreateJob("Z00001012", ZECTRA, true, 5M, ABIGAS, true, 10M);
			var fRTCharge = job.Charges.AddNew();
			fRTCharge.JR_AC = Env.Registry.FreightChargeCode;

			AssertEquals(false, fRTCharge.IsRevenueCharge);
			AssertEquals(false, fRTCharge.JR_IsCostPosted);
			AssertEquals(false, fRTCharge.JR_IsApportioned);

			IQuickCalculatorCharge quickCalculator = fRTCharge;
			AssertEquals(true, quickCalculator.CanUpdateCost);

			fRTCharge.JR_ChargeType = Core.Constants.ChargeType.Revenue;
			AssertEquals(false, quickCalculator.CanUpdateCost);

			var fRTConsolCost = Factory.New<JobConsolCost>();
			fRTCharge.JR_E6 = fRTConsolCost.PK;
			AssertEquals(true, fRTCharge.JR_IsApportioned);
			AssertEquals(false, quickCalculator.CanUpdateCost);

			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			var apInvLine = (APInvoiceLine)apInvoice.Lines.AddNew();
			apInvLine.AL_OSAmount = 200m;
			fRTCharge.JR_AL_APLine = apInvLine.PK;
			AssertEquals(true, fRTCharge.JR_IsCostPosted);
			AssertEquals(false, quickCalculator.CanUpdateCost);
		}

		public void TestIQuickCalculatorCharge()
		{
			OrgHeader someNewOrg = Factory.New<OrgHeader>();
			someNewOrg.OH_Code = "SNO";
			Job job = CreateJob("Z00001012", ZECTRA, true, 5M, ABIGAS, true, 10M);
			Charge chrg = job.Charges.AddNew();
			chrg.JR_AC = Env.Registry.FreightChargeCode;

			IQuickCalculatorCharge quickCalculator = chrg;
			AssertEquals(true, quickCalculator.CanUpdateSell);
			AssertEquals(true, quickCalculator.CanUpdateCost);
			AssertEquals(job, quickCalculator.InvoicingJob);
			AssertEquals(Env.Registry.FreightChargeCode, quickCalculator.ChargeCode.PK);
			AssertEquals(chrg.Factory, quickCalculator.Factory);

			AutoRateInfo result = new AutoRateInfo(Factory);
			result.Attributes.Add(JobChargeAttribTypeList.Codes.MinimumRateUsed, "True");

			result.AddFlatPaymentBasis(300m, "Z00001012", "AUD");
			quickCalculator.SetAmount(CostSell.Cost, result, null);

			result.Bases.Clear();
			result.AddFlatPaymentBasis(500m, "Z00001012", "AUD");
			quickCalculator.SetAmount(CostSell.Revenue, result, someNewOrg);

			AssertEquals(300m, chrg.JR_OSCostAmt);
			AssertEquals(500m, chrg.JR_OSSellAmt);
			AssertEquals(someNewOrg.PK, chrg.JR_OH_SellAccount);
			AssertEquals(false, chrg.JR_SellRated);
			AssertEquals(false, chrg.JR_CostRated);
			Assert(!chrg.JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.MinimumRateUsed).IsEmpty);

			result.Attributes.Clear();
			quickCalculator.SetAmount(CostSell.Revenue, result, someNewOrg);
			Assert(chrg.JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.MinimumRateUsed).IsEmpty);

			AccTransactionLines revLine = Factory.New<AccTransactionLines>();
			revLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;

			AccTransactionLines costLine = Factory.New<AccTransactionLines>();
			costLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;

			chrg.JR_AL_APLine = costLine.PK;
			chrg.JR_AL_ARLine = revLine.PK;
			AssertEquals(false, quickCalculator.CanUpdateSell);
			AssertEquals(false, quickCalculator.CanUpdateCost);

			result.Attributes.Add(JobChargeAttribTypeList.Codes.MinimumRateUsed, "True");
			result.ChargeCode = chrg.ChargeCode;
			quickCalculator.SetAmount(CostSell.Revenue, result, someNewOrg);
			Assert(chrg.JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.MinimumRateUsed).IsEmpty);

			job.LocalChargesPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			quickCalculator.SetAmount(CostSell.Revenue, result, someNewOrg);
			Assert(chrg.JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.MinimumRateUsed).IsEmpty);
			Assert(!job.Charges[1].JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.MinimumRateUsed).IsEmpty);
		}

		public void TestIsCalculationDescriptionRelevant()
		{
			Charge chrg = Factory.New<Charge>();
			var accChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chrg.JR_AC = accChargeCode.PK;

			InvoiceRollupOrGroup item = OrganisationRegistry.Instance.InvoiceRollupOrGroup.Value.GetBestMatch(OrgConstants.ServiceDirection.Code.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code);
			InvoiceRollupOrGroupCollection value = new InvoiceRollupOrGroupCollection();
			value.Add(item);

			//----------------All-vs-Insurance-------------------//
			TestIsCalculationDescriptionRelevant_Helper(InvoiceDescriptionOptionsList.Codes.All, ChargeCodeGroupList.Codes.Insurance, true, value, chrg);

			//----------------All-vs-Freight---------------------//
			TestIsCalculationDescriptionRelevant_Helper(InvoiceDescriptionOptionsList.Codes.All, ChargeCodeGroupList.Codes.Freight, true, value, chrg);

			//----------------AllExRate-vs-Insurance-------------------//
			TestIsCalculationDescriptionRelevant_Helper(InvoiceDescriptionOptionsList.Codes.AllExRate, ChargeCodeGroupList.Codes.Insurance, true, value, chrg);

			//----------------AllExRate-vs-Freight---------------------//
			TestIsCalculationDescriptionRelevant_Helper(InvoiceDescriptionOptionsList.Codes.AllExRate, ChargeCodeGroupList.Codes.Freight, true, value, chrg);

			//----------------Freight-vs-Freight---------------------//
			TestIsCalculationDescriptionRelevant_Helper(InvoiceDescriptionOptionsList.Codes.Freight, ChargeCodeGroupList.Codes.Freight, true, value, chrg);

			//----------------Freight-vs-Insurance---------------------//
			TestIsCalculationDescriptionRelevant_Helper(InvoiceDescriptionOptionsList.Codes.Freight, ChargeCodeGroupList.Codes.Insurance, false, value, chrg);

			//----------------FreightExRate-vs-Freight---------------------//
			TestIsCalculationDescriptionRelevant_Helper(InvoiceDescriptionOptionsList.Codes.FreightExRate, ChargeCodeGroupList.Codes.Freight, true, value, chrg);

			//----------------FreightExRate-vs-Insurance---------------------//
			TestIsCalculationDescriptionRelevant_Helper(InvoiceDescriptionOptionsList.Codes.FreightExRate, ChargeCodeGroupList.Codes.Insurance, false, value, chrg);

			//----------------FreightFOB-vs-Origin---------------------//
			TestIsCalculationDescriptionRelevant_Helper(InvoiceDescriptionOptionsList.Codes.FreightFOB, ChargeCodeGroupList.Codes.Origin, true, value, chrg);

			//----------------FreightFOB-vs-Loading---------------------//
			TestIsCalculationDescriptionRelevant_Helper(InvoiceDescriptionOptionsList.Codes.FreightFOB, ChargeCodeGroupList.Codes.Loading, true, value, chrg);

			//----------------FreightFOB-vs-Freight---------------------//
			TestIsCalculationDescriptionRelevant_Helper(InvoiceDescriptionOptionsList.Codes.FreightFOB, ChargeCodeGroupList.Codes.Freight, true, value, chrg);

			//----------------FreightFOB-vs-Insurance---------------------//
			TestIsCalculationDescriptionRelevant_Helper(InvoiceDescriptionOptionsList.Codes.FreightFOB, ChargeCodeGroupList.Codes.Insurance, false, value, chrg);

			//----------------FreightFOBExRate-vs-Origin---------------------//
			TestIsCalculationDescriptionRelevant_Helper(InvoiceDescriptionOptionsList.Codes.FreightFOBExRate, ChargeCodeGroupList.Codes.Origin, true, value, chrg);

			//----------------FreightFOBExRate-vs-Loading---------------------//
			TestIsCalculationDescriptionRelevant_Helper(InvoiceDescriptionOptionsList.Codes.FreightFOBExRate, ChargeCodeGroupList.Codes.Loading, true, value, chrg);

			//----------------FreightFOBExRate-vs-Freight---------------------//
			TestIsCalculationDescriptionRelevant_Helper(InvoiceDescriptionOptionsList.Codes.FreightFOBExRate, ChargeCodeGroupList.Codes.Freight, true, value, chrg);

			//----------------FreightFOBExRate-vs-Insurance---------------------//
			TestIsCalculationDescriptionRelevant_Helper(InvoiceDescriptionOptionsList.Codes.FreightFOBExRate, ChargeCodeGroupList.Codes.Insurance, false, value, chrg);
		}

		[ExpectNoExceptions("No System.NullReferenceException Should occur")]
		public void TestIsCalculationDescriptionRelevantHandlesNullAccChargeCode()
		{
			Charge chrg = Factory.New<Charge>();
			chrg.JR_AC = new ZGuid();

			var item = OrganisationRegistry.Instance.InvoiceRollupOrGroup.Value.GetBestMatch(OrgConstants.ServiceDirection.Code.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code);
			item.InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.FreightExRate;
			var value = new InvoiceRollupOrGroupCollection();
			value.Add(item);

			OrganisationRegistry.Instance.InvoiceRollupOrGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, value);
			bool result = chrg.IsCalculationDescriptionRelevant;
		}

		void TestIsCalculationDescriptionRelevant_Helper(string invoiceDescriptionOptionsList, string chargeCodeGroupListCode, bool assert, InvoiceRollupOrGroupCollection value, Charge chrg)
		{
			value[0].InvoiceLineDisplayOption = invoiceDescriptionOptionsList;
			chrg.ChargeCode.AC_ChargeGroup = chargeCodeGroupListCode;
			OrganisationRegistry.Instance.InvoiceRollupOrGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, value);
			AssertEquals(assert, chrg.IsCalculationDescriptionRelevant);
		}

		public void TestIsCalculationDescriptionRelevantWithDifferentTransportModes()
		{
			InvoiceRollupOrGroupCollection registryValue = OrganisationRegistry.Instance.InvoiceRollupOrGroup.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			InvoiceRollupOrGroup invoiceOrGroupSetting = registryValue.AddNew();
			invoiceOrGroupSetting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			invoiceOrGroupSetting.ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			invoiceOrGroupSetting.TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			invoiceOrGroupSetting.InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.None;
			invoiceOrGroupSetting.GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			invoiceOrGroupSetting.GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.FRT;
			invoiceOrGroupSetting.InvoicePostingStyle = InvoicePostingOptionsList.Codes.FinalInvoiceOnly;

			invoiceOrGroupSetting = registryValue.AddNew();
			invoiceOrGroupSetting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			invoiceOrGroupSetting.ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			invoiceOrGroupSetting.TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
			invoiceOrGroupSetting.InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.AllExRate;
			invoiceOrGroupSetting.GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			invoiceOrGroupSetting.GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.FRT;
			invoiceOrGroupSetting.InvoicePostingStyle = InvoicePostingOptionsList.Codes.FinalInvoiceOnly;

			OrganisationRegistry.Instance.InvoiceRollupOrGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			var shipment = TestObjectCreator.CreateShipment("S00981");
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			var job = TestObjectCreator.CreateJob(shipment, false);
			var jobCharge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "test charge", TestObjectCreator.AUD, 100m, null, TestObjectCreator.AUD, 120m, TestObjectCreator.AALSHI);
			AssertEquals("IsCalculationDescriptionRelevant", true, jobCharge.IsCalculationDescriptionRelevant);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("IsCalculationDescriptionRelevant", false, jobCharge.IsCalculationDescriptionRelevant);
		}

		#region Overriden Properties Tests

		public void TestManualCharge_SetsSellAndCostRatingOverriden()
		{
			var chargeCodePK = TestObjectCreator.InsertMarginChargeCode(75).PK;

			var ch = Factory.NewWithValidTestData<Charge>();
			ch.JR_SellRated = false;
			ch.JR_CostRated = false;
			ch.JR_AC = chargeCodePK;

			Assert("Prerequisite", !ch.JR_CostRatingOverride && !ch.JR_SellRatingOverride);
			ch.JR_OSCostAmt = 10;

			Assert("JR_CostRatingOverride should be true", ch.JR_CostRatingOverride);
			Assert("JR_LocalSellAmt has changed", ch.JR_OSSellAmt > 0);
			Assert("JR_SellRatingOverride has not changed", !ch.JR_SellRatingOverride);

			ch.JR_OSSellAmt = 34;

			Assert("JR_SellRatingOverride has changed", ch.JR_SellRatingOverride);

			var ch2 = Factory.NewWithValidTestData<Charge>();
			ch2.JR_SellRated = false;
			ch2.JR_CostRated = false;
			ch2.JR_AC = chargeCodePK;

			Assert("Prerequisite", !ch2.JR_CostRatingOverride && !ch2.JR_SellRatingOverride);
			ch2.JR_OSSellAmt = 34;

			Assert("JR_SellRatingOverride has changed", ch2.JR_SellRatingOverride);
			Assert("JR_CostRatingOverride has not changed", !ch2.JR_CostRatingOverride);

			ch2.JR_OSCostAmt = 10;

			Assert("JR_CostRatingOverride has changed", ch2.JR_CostRatingOverride);
		}

		public void TestJR_EstimatedCost()
		{
			Charge ch = Factory.NewWithValidTestData<Charge>();
			ch.JR_OSCostAmt = 10;
			AssertEquals("JR_EstimatedCost should update, too", (ZDecimal)10, ch.JR_EstimatedCost);
			ch.JR_OSCostAmt = 20;
			AssertEquals("JR_EstimatedCost should update again", (ZDecimal)20, ch.JR_EstimatedCost);

			Factory.Save();
			ch.JR_OSCostAmt = 30;
			AssertEquals("JR_EstimatedCost should not update again as charge is saved", (ZDecimal)20, ch.JR_EstimatedCost);
		}

		public void TestJR_EstimatedRevenue()
		{
			Charge ch = Factory.NewWithValidTestData<Charge>();
			ch.JR_OSSellAmt = 10;
			AssertEquals("JR_EstimatedRevenue should update, too", (ZDecimal)10, ch.JR_EstimatedRevenue);
			ch.JR_OSSellAmt = 20;
			AssertEquals("JR_EstimatedRevenue should update again", (ZDecimal)20, ch.JR_EstimatedRevenue);

			Factory.Save();
			ch.JR_OSSellAmt = 30;
			AssertEquals("JR_EstimatedRevenue should not update again as charge is saved", (ZDecimal)20, ch.JR_EstimatedRevenue);
		}

		#endregion

		public void TestCostDueOverseasAgent()
		{
			OrgHeader agent = Factory.New<OrgHeader>();
			OrgHeader agent2 = Factory.New<OrgHeader>();

			agent.OH_Code = "AG1";
			agent2.OH_Code = "AG2";

			Job job = Factory.NewJobForTesting<Job>();
			job.AgentCollectPK = agent.PK;

			Charge charge1 = job.Charges.AddNew();
			charge1.JR_OH_CostAccount = agent.PK;

			Charge charge2 = job.Charges.AddNew();
			charge2.JR_OH_CostAccount = agent2.PK;

			Charge charge3 = job.Charges.AddNew();

			AssertEquals(true, charge1.CostDueOverseasAgent);
			AssertEquals(false, charge2.CostDueOverseasAgent);
			AssertEquals(false, charge3.CostDueOverseasAgent);
		}

		public void TestIsProfitShareCharge()
		{
			Job job = Factory.NewJobForTesting<Job>();

			Charge charge1 = job.Charges.AddNew();
			charge1.JR_AC = AccountingConfigurationRegistry.Instance.ProfitShareAdjustmentChargeCode.Value;

			Charge charge2 = job.Charges.AddNew();
			charge2.JR_AC = Env.Registry.FreightChargeCode;

			AssertEquals(true, charge1.IsProfitShareCharge);
			AssertEquals(false, charge2.IsProfitShareCharge);
		}

		public void TestProfitShareForCharge()
		{
			BusinessObjectFactory orgFactory = new BusinessObjectFactory();
			OrgHeader consignee = orgFactory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignor = orgFactory.NewWithValidTestData<OrgHeader>();
			OrgHeader sendingAgent = orgFactory.NewWithValidTestData<OrgHeader>();
			OrgHeader deliveryAgent = orgFactory.NewWithValidTestData<OrgHeader>();
			orgFactory.Save();

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(sendingAgent);
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OH_DeliveryAgent = deliveryAgent.PK;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_TransportMode = "AIR";

			Job testJob = Factory.NewJobForTesting<Job>();
			testJob.PlugInData = shipment;
			AssertNull("No profit share agreement found", testJob.ProfitShareAgreement);

			BusinessObjectFactory profitShareFactory = new BusinessObjectFactory();
			OrgAgentRelationship agentRelationship = profitShareFactory.New<OrgAgentRelationship>();
			agentRelationship.O3_OH_SendingAgent = sendingAgent.PK;
			agentRelationship.O3_OH_ReceivingAgent = deliveryAgent.PK;

			OrgProfitShareDetails profitShareAgreement = agentRelationship.ProfitShareDetails.AddNew();
			profitShareAgreement.O4_FreightMode = "AIR";
			profitShareAgreement.O4_SendingPortOrCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			profitShareAgreement.O4_ReceivingPortOrCountry = "US";
			profitShareAgreement.O4_EndDate = ZDateTime.Today.AddDays(10);
			profitShareAgreement.O4_StartDate = ZDateTime.Today.AddDays(-10);

			OrgProfitShareParty party = profitShareAgreement.PartyDetails.AddNew();
			party.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent;
			party.PS_PartyProfitSharePercent = 40m;

			profitShareFactory.Save();
			AssertEquals("Profit share agreement found", profitShareAgreement.PK, testJob.ProfitShareAgreement.PK);

			Charge charge = testJob.Charges.AddNew();
			charge.JR_IsIncludedInProfitShare = true;
			charge.JR_AgentDeclaredSellAmt = 300m;
			charge.JR_AgentDeclaredCostAmt = 100m;
			AssertEquals(80m, charge.ProfitShareForCharge);

			charge = testJob.Charges.AddNew();
			charge.JR_IsIncludedInProfitShare = true;
			charge.JR_AgentDeclaredSellAmt = 200m;
			charge.JR_AgentDeclaredCostAmt = 100m;

			AssertEquals("JR_AgentDeclaredSellAmt before setting a CMT type Charge", 200m, charge.JR_AgentDeclaredSellAmt);
			AssertEquals("JR_AgentDeclaredCostAmt before setting a CMT type Charge", 100m, charge.JR_AgentDeclaredCostAmt);

			charge.JR_AC = TestObjectCreator.CommentChargeCode.PK;
			Assert("JR_IsIncludedInProfitShare should be false for CMT type Charge", !charge.JR_IsIncludedInProfitShare);
			AssertEquals("JR_AgentDeclaredSellAmt after setting a CMT type Charge", 0m, charge.JR_AgentDeclaredSellAmt);
			AssertEquals("JR_AgentDeclaredCostAmt after setting a CMT type Charge", 0m, charge.JR_AgentDeclaredCostAmt);

			charge.JR_IsIncludedInProfitShare = true;
			Assert("JR_IsIncludedInProfitShare should be false for CMT type Charge", !charge.JR_IsIncludedInProfitShare);
			AssertEquals("JR_AgentDeclaredSellAmt after setting a CMT type Charge", 0m, charge.JR_AgentDeclaredSellAmt);
			AssertEquals("JR_AgentDeclaredCostAmt after setting a CMT type Charge", 0m, charge.JR_AgentDeclaredCostAmt);
		}

		public void TestGivenAgencyProfitShareProfileForControllingCustomer_WhenGetProfitShareAgreement_ThenProfileShouldNotBeMatched()
		{
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.DocAddresses.AddNew(controllingCustomer.MainAddress, DocAddressType.ControllingCustomer);

			var testJob = Factory.NewJobForTesting<Job>();
			testJob.PlugInData = shipment;
			AssertNull("No profit share agreement found", testJob.ProfitShareAgreement);

			var agentRelationship = Factory.New<OrgAgentRelationship>();
			agentRelationship.O3_OH_SendingAgent = controllingCustomer.PK;
			agentRelationship.O3_ProfitShareType = "AGY";

			var profitShareAgreement = agentRelationship.ProfitShareDetails.AddNew();
			profitShareAgreement.O4_EndDate = ZDateTime.Today.AddDays(10);
			profitShareAgreement.O4_StartDate = ZDateTime.Today.AddDays(-10);

			Factory.Save();
			AssertNull("No profit share agreement found, controlling customer will not be used to match", testJob.ProfitShareAgreement);
		}

		public void TestGivenAgencyProfitShareProfileForControllingAgent_WhenGetProfitShareAgreement_ThenProfileShouldBeMatched()
		{
			var controllingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.DocAddresses.AddNew(controllingAgent.MainAddress, DocAddressType.ControllingAgent);

			var testJob = Factory.NewJobForTesting<Job>();
			testJob.PlugInData = shipment;
			AssertNull("No profit share agreement found", testJob.ProfitShareAgreement);

			var agentRelationship = Factory.New<OrgAgentRelationship>();
			agentRelationship.O3_OH_SendingAgent = controllingAgent.PK;
			agentRelationship.O3_ProfitShareType = "AGY";

			var profitShareAgreement = agentRelationship.ProfitShareDetails.AddNew();
			profitShareAgreement.O4_EndDate = ZDateTime.Today.AddDays(10);
			profitShareAgreement.O4_StartDate = ZDateTime.Today.AddDays(-10);

			Factory.Save();
			AssertEquals("Profit share agreement found, controlling agent will be used to match", profitShareAgreement.PK, testJob.ProfitShareAgreement.PK);
		}

		public void TestIsSelectedForAutoPopulate()
		{
			Charge ch = Factory.New<Charge>();
			ch.IsSelectedForAutoPopulation = true;
			Assert(ch.IsSelectedForAutoPopulation);
		}

		public void TestLineCFXReadOnly()
		{
			Charge chrg = Factory.New<Charge>();
			AssertEquals("Line CFX is always readonly", true, chrg.JR_LineCFXInfo.ReadOnly);
		}

		public void TestUpdateLineCFXWithSellInvoiceCurrencyWhenSellCurrencyIsLocal()
		{
			#region Data Set Up

			ExchangeRateReader.GetReaderInstance().ClearCache();
			var rateFactory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(rateFactory);
			creator.CreateExchangeRate(creator.USD, "BUY", 1.3m, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			rateFactory.Save();

			var shipment = CommonShipment.New(Factory);
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "INBOM";

			var orgFactory = new BusinessObjectFactory();
			var debtor = orgFactory.NewWithValidTestData<OrgHeader>();
			debtor.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			debtor.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, 10m);

			var agent = orgFactory.NewWithValidTestData<OrgHeader>();
			agent.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			agent.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, 20m);

			var otherOrg = orgFactory.NewWithValidTestData<OrgHeader>();
			otherOrg.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			otherOrg.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, 15m);

			orgFactory.Save();

			#endregion

			using (var job = JobInvoicing.Job.CreateWithMutex(Factory, shipment))
			{
				job.PlugInData = shipment;
				job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

				job.LocalChargesPK = debtor.PK;
				job.AgentCollectPK = agent.PK;

				var charge = job.Charges.AddNew();
				charge.JR_AC = Env.Registry.FreightChargeCode;
				charge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
				charge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				charge.JR_OH_SellAccount = ZGuid.Empty;
				AssertNull("RevenueExchangeRate", charge.RevenueExchangeRate);
				AssertNull("SellInvoiceExchangeRate", charge.SellInvoiceExchangeRate);
				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
				Assert("BillInInvoiceCurrency", charge.BillInInvoiceCurrency);
				AssertNotNull("SellInvoiceExchangeRate", charge.SellInvoiceExchangeRate);
				AssertEquals("JR_OSSellInvoiceExRate", 1.3m, charge.JR_OSSellInvoiceExRate);
				var usdRatePk = AssertChargesSellInvoiceExchangeRate(charge, null, ZGuid.Empty);
				charge.JR_OSSellAmt = 100m;

				AssertEquals("Precondition: JobInvoicingCFXEnabled is false by default", false, AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));

				charge.JR_OH_SellAccount = debtor.PK;
				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
				AssertEquals("Line CFX is taken from Job - same as org", 10m, charge.JR_LineCFX);
				AssertEquals("SellExchangeRateType", ExchangeRateType.Buy, charge.SellExchangeRateType);
				AssertEquals("JR_OSSellExRate", 1m, charge.JR_OSSellExRate);
				AssertEquals("JR_OSSellInvoiceExRate", 1.3m, charge.JR_OSSellInvoiceExRate);
				usdRatePk = AssertChargesSellInvoiceExchangeRate(charge, debtor, usdRatePk);
				AssertEquals("JR_LocalSellAmt", 100m, charge.JR_LocalSellAmt);
				AssertEquals("JR_CFXAmt", 0m, charge.JR_CFXAmt);
				AssertEquals("JR_OSSellInvoiceExRate", 1.3m, charge.JR_OSSellInvoiceExRate);
				usdRatePk = charge.SellInvoiceExchangeRate.ExchangeRatePk;

				AssertEquals("JR_LocalSellInvoiceAmt", 110m, charge.JR_LocalSellInvoiceAmt);
				AssertEquals("JR_OSSellInvoiceAmt", 143m, charge.JR_OSSellInvoiceAmt);

				charge.JR_OH_SellAccount = agent.PK;
				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
				AssertEquals("Line CFX is taken from Job - same as org", 20m, charge.JR_LineCFX);
				AssertEquals("SellExchangeRateType", ExchangeRateType.Buy, charge.SellExchangeRateType);
				AssertEquals("JR_OSSellExRate", 1m, charge.JR_OSSellExRate);

				AssertEquals("JR_LocalSellAmt", 100m, charge.JR_LocalSellAmt);
				AssertEquals("JR_CFXAmt", 0m, charge.JR_CFXAmt);
				AssertEquals("JR_OSSellInvoiceExRate", 1.3m, charge.JR_OSSellInvoiceExRate);
				usdRatePk = AssertChargesSellInvoiceExchangeRate(charge, agent, usdRatePk);
				AssertEquals("JR_LocalSellInvoiceAmt", 120m, charge.JR_LocalSellInvoiceAmt);
				AssertEquals("JR_OSSellInvoiceAmt", 156m, charge.JR_OSSellInvoiceAmt);
				usdRatePk = charge.SellInvoiceExchangeRate.ExchangeRatePk;

				charge.JR_OH_SellAccount = otherOrg.PK;
				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
				AssertEquals("Line CFX is taken from Job - same as org", 15m, charge.JR_LineCFX);
				AssertEquals("SellExchangeRateType", ExchangeRateType.Buy, charge.SellExchangeRateType);
				AssertEquals("JR_LocalSellAmt", 100m, charge.JR_LocalSellAmt);
				AssertEquals("JR_OSSellExRate", 1m, charge.JR_OSSellExRate);
				AssertEquals("JR_CFXAmt", 0m, charge.JR_CFXAmt);
				AssertEquals("JR_OSSellInvoiceExRate", 1.3m, charge.JR_OSSellInvoiceExRate);
				usdRatePk = AssertChargesSellInvoiceExchangeRate(charge, otherOrg, usdRatePk);
				AssertEquals("JR_LocalSellInvoiceAmt", 115m, charge.JR_LocalSellInvoiceAmt);
				AssertEquals("JR_OSSellInvoiceAmt", 149.5m, charge.JR_OSSellInvoiceAmt);

				AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

				charge.JR_OH_SellAccount = debtor.PK;
				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
				AssertEquals("Line CFX is taken from Job - same as org", 10m, charge.JR_LineCFX);
				AssertEquals("SellExchangeRateType", ExchangeRateType.Buy, charge.SellExchangeRateType);
				AssertEquals(1m, charge.JR_OSSellExRate);
				AssertEquals("JR_LocalSellAmt", 100m, charge.JR_LocalSellAmt);
				AssertEquals("JR_CFXAmt", 10m, charge.JR_CFXAmt);
				AssertEquals("JR_OSSellInvoiceExRate", 1.3m, charge.JR_OSSellInvoiceExRate);
				usdRatePk = AssertChargesSellInvoiceExchangeRate(charge, debtor, usdRatePk);
				AssertEquals("JR_LocalSellInvoiceAmt", 110m, charge.JR_LocalSellInvoiceAmt);
				AssertEquals("JR_OSSellInvoiceAmt", 143m, charge.JR_OSSellInvoiceAmt);
				usdRatePk = charge.SellInvoiceExchangeRate.ExchangeRatePk;

				charge.JR_OH_SellAccount = agent.PK;
				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
				AssertEquals("Line CFX is taken from Job - same as org", 20m, charge.JR_LineCFX);
				AssertEquals("SellExchangeRateType", ExchangeRateType.Buy, charge.SellExchangeRateType);
				AssertEquals("JR_OSSellExRate", 1m, charge.JR_OSSellExRate);
				AssertEquals("JR_LocalSellAmt", 100m, charge.JR_LocalSellAmt);
				AssertEquals("JR_CFXAmt", 20m, charge.JR_CFXAmt);
				AssertEquals("JR_OSSellInvoiceExRate", 1.3m, charge.JR_OSSellInvoiceExRate);
				usdRatePk = AssertChargesSellInvoiceExchangeRate(charge, agent, usdRatePk);
				AssertEquals("JR_LocalSellInvoiceAmt", 120m, charge.JR_LocalSellInvoiceAmt);
				AssertEquals("JR_OSSellInvoiceAmt", 156m, charge.JR_OSSellInvoiceAmt);

				charge.JR_OH_SellAccount = otherOrg.PK;
				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
				AssertEquals("Line CFX is taken from Job - same as org", 15m, charge.JR_LineCFX);
				AssertEquals("SellExchangeRateType", ExchangeRateType.Buy, charge.SellExchangeRateType);
				AssertEquals("JR_OSSellExRate", 1m, charge.JR_OSSellExRate);
				AssertEquals("JR_LocalSellAmt", 100m, charge.JR_LocalSellAmt);
				AssertEquals("JR_CFXAmt", 15m, charge.JR_CFXAmt);
				AssertEquals("JR_OSSellInvoiceExRate", 1.3m, charge.JR_OSSellInvoiceExRate);
				AssertChargesSellInvoiceExchangeRate(charge, otherOrg, usdRatePk);
				AssertEquals("JR_LocalSellInvoiceAmt", 115m, charge.JR_LocalSellInvoiceAmt);
				AssertEquals("JR_OSSellInvoiceAmt", 149.5m, charge.JR_OSSellInvoiceAmt);
			}
		}

		ZGuid AssertChargesSellInvoiceExchangeRate(Charge charge, OrgHeader debtor, ZGuid previousExRatePk)
		{
			AssertNotNull("SellInvoiceExchangeRate", charge.SellInvoiceExchangeRate);
			AssertNotEquals("ExchangeRatePk", previousExRatePk, charge.SellInvoiceExchangeRate.ExchangeRatePk);
			AssertEquals("Rate", charge.JR_OSSellInvoiceExRate, charge.SellInvoiceExchangeRate.Rate);
			AssertEquals("OrgType", ExchangeRateOrgTypeEnum.Debtor, charge.SellInvoiceExchangeRate.OrgType);
			AssertEquals("OrgPk", debtor != null ? debtor.PK : ZGuid.Empty, charge.SellInvoiceExchangeRate.OrgPk);

			return charge.SellInvoiceExchangeRate.ExchangeRatePk;
		}

		public void TestOsSellInvoiceExRateOverrideForPosting()
		{
			var job = TestObjectCreator.CreateJob("S000010001", TestObjectCreator.LocalClient, 1.0m, TestObjectCreator.Agent, 1.0m);
			TestObjectCreator.CreateExchangeRate(job, TestObjectCreator.USD, 0.7m);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "", null, 0m, null, "1001", TestObjectCreator.EUR, 100m, TestObjectCreator.Debtor);
			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.Code;

			charge.SetContext(BusinessContext.PostingReceivableCharges);

			Assert(charge.HasContext(BusinessContext.PostingReceivableCharges));
			Assert(!charge.IsRevenuePosted);
			Assert(charge.BillInInvoiceCurrency);
			Assert(!charge.BIllInInvoiceCurrencySameAsSellCurrency);

			AssertEquals("With Posting context but without osSellInvoiceExRateOverrideForPosting value, charge JR_OSSellInvoiceExRate should return the GetBuyExchangeRate()", 0.7m, charge.JR_OSSellInvoiceExRate);

			charge.OverrideOSSellInvoiceExRateForPosting(1.5m);

			AssertEquals("With Posting context and osSellInvoiceExRateOverrideForPosting value, charge JR_OSSellInvoiceExRate should return the osSellInvoiceExRateOverrideForPosting value", 1.5m, charge.JR_OSSellInvoiceExRate);

			charge.RemoveContext(BusinessContext.PostingReceivableCharges);

			Assert(!charge.HasContext(BusinessContext.PostingReceivableCharges));
			AssertEquals("Without Posting context charge JR_OSSellInvoiceExRate should return the job exchange rate value", 0.7m, charge.JR_OSSellInvoiceExRate);
		}

		public void TestChangeOfSellInvoiceCurrencyWithCFX()
		{
			AssertChangeOfSellInvoiceCurrencyUpdatesSellInvoiceExRateAndCFX(enableCFX: true);
		}

		public void TestChangeOfSellInvoiceCurrencyNoCFX()
		{
			AssertChangeOfSellInvoiceCurrencyUpdatesSellInvoiceExRateAndCFX(enableCFX: false);
		}

		void AssertChangeOfSellInvoiceCurrencyUpdatesSellInvoiceExRateAndCFX(bool enableCFX)
		{
			#region Data Set Up

			ExchangeRateReader.GetReaderInstance().ClearCache();
			var rateFactory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(rateFactory);
			creator.CreateExchangeRate(creator.USD, "BUY", 1.3m, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			creator.CreateExchangeRate(creator.EUR, "BUY", 1.4m, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			rateFactory.Save();

			var shipment = CommonShipment.New(Factory);
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "INBOM";

			var orgFactory = new BusinessObjectFactory();
			var debtor = orgFactory.NewWithValidTestData<OrgHeader>();
			debtor.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			debtor.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, 10m);

			var agent = orgFactory.NewWithValidTestData<OrgHeader>();
			agent.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			agent.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, 20m);

			orgFactory.Save();

			#endregion

			using (AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, enableCFX))
			using (var job = JobInvoicing.Job.CreateWithMutex(Factory, shipment))
			{
				job.PlugInData = shipment;
				job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

				job.LocalChargesPK = debtor.PK;
				job.AgentCollectPK = agent.PK;

				var charge = job.Charges.AddNew();
				charge.JR_AC = Env.Registry.FreightChargeCode;
				charge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
				charge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
				charge.JR_OH_SellAccount = debtor.PK;
				AssertNull("RevenueExchangeRate", charge.RevenueExchangeRate);
				AssertNull("SellInvoiceExchangeRate", charge.SellInvoiceExchangeRate);
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				charge.JR_OSSellAmt = 120m;
				AssertEquals("JR_OSSellExRate", 1m, charge.JR_OSSellExRate);
				AssertEquals("JR_OSSellInvoiceExRate", decimal.Zero, charge.JR_OSSellInvoiceExRate);
				AssertEquals("JR_LineCFX", decimal.Zero, charge.JR_LineCFX);
				AssertEquals("JR_CFXAmt", decimal.Zero, charge.JR_CFXAmt);

				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
				Assert(charge.BillInInvoiceCurrency);
				AssertNull("RevenueExchangeRate", charge.RevenueExchangeRate);
				AssertEquals("JR_OSSellExRate", 1m, charge.JR_OSSellExRate);
				AssertEquals("JR_OSSellInvoiceExRate", 1.3m, charge.JR_OSSellInvoiceExRate);
				var sellInvoiceRatePk = AssertChargesSellInvoiceExchangeRate(charge, debtor, ZGuid.Empty);
				AssertEquals("JR_LineCFX is updated as we call OnRevenueExchangeRateChanged", 10m, charge.JR_LineCFX);
				AssertEquals("JR_CFXAmt", enableCFX ? 12m : decimal.Zero, charge.JR_CFXAmt);

				charge.JR_RX_NKSellInvoiceCurrency = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
				Assert(!charge.BillInInvoiceCurrency);
				AssertNull("RevenueExchangeRate", charge.RevenueExchangeRate);
				AssertNull("SellInvoiceExchangeRate", charge.SellInvoiceExchangeRate);
				AssertEquals("JR_LineCFX is updated as we call OnRevenueExchangeRateChanged", decimal.Zero, charge.JR_LineCFX);
				AssertEquals("JR_CFXAmt", decimal.Zero, charge.JR_CFXAmt);

				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.RX_Code;
				Assert(charge.BillInInvoiceCurrency);
				AssertNull("RevenueExchangeRate", charge.RevenueExchangeRate);
				AssertEquals("JR_OSSellExRate", 1m, charge.JR_OSSellExRate);
				AssertEquals("JR_OSSellInvoiceExRate", 1.4m, charge.JR_OSSellInvoiceExRate);
				sellInvoiceRatePk = AssertChargesSellInvoiceExchangeRate(charge, debtor, sellInvoiceRatePk);
				AssertEquals("JR_LineCFX is updated as we call OnRevenueExchangeRateChanged", 10m, charge.JR_LineCFX);
				AssertEquals("JR_CFXAmt", enableCFX ? 12m : decimal.Zero, charge.JR_CFXAmt);

				charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
				Assert(charge.BillInInvoiceCurrency);
				AssertNotNull("RevenueExchangeRate", charge.RevenueExchangeRate);
				AssertEquals("JR_OSSellExRate", 1.17m, charge.JR_OSSellExRate);
				AssertEquals("JR_OSSellInvoiceExRate", 1.4m, charge.JR_OSSellInvoiceExRate);
				sellInvoiceRatePk = AssertChargesSellInvoiceExchangeRate(charge, debtor, charge.RevenueExchangeRate.ExchangeRatePk);
				AssertEquals("JR_LineCFX", 10m, charge.JR_LineCFX);
				AssertEquals("JR_CFXAmt", enableCFX ? 12m : decimal.Zero, charge.JR_CFXAmt);

				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
				Assert(charge.BillInInvoiceCurrency);
				AssertNotNull("RevenueExchangeRate", charge.RevenueExchangeRate);
				AssertEquals("JR_OSSellExRate", 1.3m, charge.JR_OSSellExRate);
				AssertEquals("JR_OSSellInvoiceExRate", 1.3m, charge.JR_OSSellInvoiceExRate);
				AssertChargesSellInvoiceExchangeRate(charge, debtor, sellInvoiceRatePk);
				AssertEquals("JR_LineCFX is updated as we call OnRevenueExchangeRateChanged", decimal.Zero, charge.JR_LineCFX);
				AssertEquals("JR_CFXAmt", decimal.Zero, charge.JR_CFXAmt);
			}
		}

		public void TestUpdateLineCFXWithSellInvoiceCurrencyWhenSellCurrencyIsForeign()
		{
			#region Data Set Up

			ExchangeRateReader.GetReaderInstance().ClearCache();
			var rateFactory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(rateFactory);
			creator.CreateExchangeRate(creator.USD, "BUY", 1.3m, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			creator.CreateExchangeRate(creator.EUR, "BUY", 1.4m, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			rateFactory.Save();

			var shipment = CommonShipment.New(Factory);
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "INBOM";

			var orgFactory = new BusinessObjectFactory();
			var debtor = orgFactory.NewWithValidTestData<OrgHeader>();
			debtor.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			debtor.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, 10m);

			var agent = orgFactory.NewWithValidTestData<OrgHeader>();
			agent.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			agent.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, 20m);

			var otherOrg = orgFactory.NewWithValidTestData<OrgHeader>();
			otherOrg.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			otherOrg.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, 15m);

			orgFactory.Save();

			#endregion

			using (var job = JobInvoicing.Job.CreateWithMutex(Factory, shipment))
			{
				job.PlugInData = shipment;
				job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

				job.LocalChargesPK = debtor.PK;
				job.AgentCollectPK = agent.PK;

				var charge = job.Charges.AddNew();
				charge.JR_AC = Env.Registry.FreightChargeCode;
				charge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
				charge.JR_OH_SellAccount = ZGuid.Empty;
				AssertNull("RevenueExchangeRate", charge.RevenueExchangeRate);
				AssertNull("SellInvoiceExchangeRate", charge.SellInvoiceExchangeRate);
				charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
				AssertNotNull("RevenueExchangeRate", charge.RevenueExchangeRate);
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.RX_Code;
				Assert(charge.BillInInvoiceCurrency);
				var sellInvoiceRatePk = AssertChargesSellInvoiceExchangeRate(charge, null, ZGuid.Empty);
				charge.JR_OSSellAmt = 130m;

				AssertEquals("Precondition: JobInvoicingCFXEnabled is false by default", false, AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));

				charge.JR_OH_SellAccount = debtor.PK;
				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.RX_Code;
				AssertEquals("Line CFX is taken from Job - same as org", 10m, charge.JR_LineCFX);
				AssertEquals("SellExchangeRateType", ExchangeRateType.Sell, charge.SellExchangeRateType);
				AssertEquals("JR_OSSellExRate", 1.17m, charge.JR_OSSellExRate);
				AssertEquals("JR_LocalSellAmt", 111.11m, charge.JR_LocalSellAmt);
				AssertEquals("JR_CFXAmt", 0m, charge.JR_CFXAmt);
				AssertEquals("JR_OSSellInvoiceExRate", 1.4m, charge.JR_OSSellInvoiceExRate);
				sellInvoiceRatePk = AssertChargesSellInvoiceExchangeRate(charge, debtor, sellInvoiceRatePk);
				AssertEquals("JR_LocalSellInvoiceAmt", 111.11m, charge.JR_LocalSellInvoiceAmt);
				AssertEquals("JR_OSSellInvoiceAmt", 155.55m, charge.JR_OSSellInvoiceAmt);

				charge.JR_OH_SellAccount = agent.PK;
				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.RX_Code;
				AssertEquals("Line CFX is taken from Job - same as org", 20m, charge.JR_LineCFX);
				AssertEquals("SellExchangeRateType", ExchangeRateType.Sell, charge.SellExchangeRateType);
				AssertEquals("JR_OSSellExRate", 1.04m, charge.JR_OSSellExRate);
				AssertEquals("JR_LocalSellAmt", 125m, charge.JR_LocalSellAmt);
				AssertEquals("JR_CFXAmt", 0m, charge.JR_CFXAmt);
				AssertEquals("JR_OSSellInvoiceExRate", 1.4m, charge.JR_OSSellInvoiceExRate);
				sellInvoiceRatePk = AssertChargesSellInvoiceExchangeRate(charge, agent, sellInvoiceRatePk);
				AssertEquals("JR_LocalSellInvoiceAmt", 125m, charge.JR_LocalSellInvoiceAmt);
				AssertEquals("JR_OSSellInvoiceAmt", 175m, charge.JR_OSSellInvoiceAmt);

				charge.JR_OH_SellAccount = otherOrg.PK;
				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.RX_Code;
				AssertEquals("Line CFX is taken from Job - same as org", 15m, charge.JR_LineCFX);
				AssertEquals("SellExchangeRateType", ExchangeRateType.Sell, charge.SellExchangeRateType);
				AssertEquals("JR_OSSellExRate", 1.105m, charge.JR_OSSellExRate);
				AssertEquals("JR_LocalSellAmt", 117.65m, charge.JR_LocalSellAmt);
				AssertEquals("JR_CFXAmt", 0m, charge.JR_CFXAmt);
				AssertEquals("JR_OSSellInvoiceExRate", 1.4m, charge.JR_OSSellInvoiceExRate);
				AssertEquals("JR_LocalSellInvoiceAmt", 117.65m, charge.JR_LocalSellInvoiceAmt);
				AssertEquals("JR_OSSellInvoiceAmt", 164.71m, charge.JR_OSSellInvoiceAmt);

				AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

				charge.JR_OH_SellAccount = debtor.PK;
				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.RX_Code;
				AssertEquals("Line CFX is taken from Job - same as org", 10m, charge.JR_LineCFX);
				AssertEquals("SellExchangeRateType", ExchangeRateType.Sell, charge.SellExchangeRateType);
				AssertEquals(1.17m, charge.JR_OSSellExRate);
				AssertEquals("JR_LocalSellAmt", 111.11m, charge.JR_LocalSellAmt);
				AssertEquals("JR_CFXAmt", 11.11m, charge.JR_CFXAmt);
				AssertEquals("JR_OSSellInvoiceExRate", 1.4m, charge.JR_OSSellInvoiceExRate);
				sellInvoiceRatePk = AssertChargesSellInvoiceExchangeRate(charge, debtor, sellInvoiceRatePk);
				AssertEquals("JR_LocalSellInvoiceAmt", 111.11m, charge.JR_LocalSellInvoiceAmt);
				AssertEquals("JR_OSSellInvoiceAmt", 155.55m, charge.JR_OSSellInvoiceAmt);

				charge.JR_OH_SellAccount = agent.PK;
				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.RX_Code;
				AssertEquals("Line CFX is taken from Job - same as org", 20m, charge.JR_LineCFX);
				AssertEquals("SellExchangeRateType", ExchangeRateType.Sell, charge.SellExchangeRateType);
				AssertEquals("JR_OSSellExRate", 1.04m, charge.JR_OSSellExRate);
				AssertEquals("JR_LocalSellAmt", 125m, charge.JR_LocalSellAmt);
				AssertEquals("JR_CFXAmt", 25m, charge.JR_CFXAmt);
				AssertEquals("JR_OSSellInvoiceExRate", 1.4m, charge.JR_OSSellInvoiceExRate);
				sellInvoiceRatePk = AssertChargesSellInvoiceExchangeRate(charge, agent, sellInvoiceRatePk);
				AssertEquals("JR_LocalSellInvoiceAmt", 125m, charge.JR_LocalSellInvoiceAmt);
				AssertEquals("JR_OSSellInvoiceAmt", 175m, charge.JR_OSSellInvoiceAmt);

				charge.JR_OH_SellAccount = otherOrg.PK;
				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.RX_Code;
				AssertEquals("Line CFX is taken from Job - same as org", 15m, charge.JR_LineCFX);
				AssertEquals("SellExchangeRateType", ExchangeRateType.Sell, charge.SellExchangeRateType);
				AssertEquals("JR_OSSellExRate", 1.105m, charge.JR_OSSellExRate);
				AssertEquals("JR_LocalSellAmt", 117.65m, charge.JR_LocalSellAmt);
				AssertEquals("JR_CFXAmt", 17.65m, charge.JR_CFXAmt);
				AssertEquals("JR_OSSellInvoiceExRate", 1.4m, charge.JR_OSSellInvoiceExRate);
				sellInvoiceRatePk = AssertChargesSellInvoiceExchangeRate(charge, otherOrg, sellInvoiceRatePk);
				AssertEquals("JR_LocalSellInvoiceAmt", 117.65m, charge.JR_LocalSellInvoiceAmt);
				AssertEquals("JR_OSSellInvoiceAmt", 164.71m, charge.JR_OSSellInvoiceAmt);
			}
		}

		public void TestSetRevenueTransactionLine()
		{
			Job job = CreateJob("Z00001012", ZECTRA, true, 5M, ABIGAS, true, 10M);
			ExchangeRate rate1 = CreateExchangeRate(job, TestObjectCreator.USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(job, TestObjectCreator.GBP, .4M);

			Charge chrg = job.Charges.AddNew();
			chrg.JR_AC = TestObjectCreator.MRG100.PK;
			chrg.JR_Desc = "Revenue Transaction Test";
			chrg.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			chrg.JR_OSCostAmt = 250m;
			chrg.JR_OH_CostAccount = AALSHI.PK;
			chrg.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			chrg.JR_OSSellAmt = 350m;
			chrg.JR_OH_SellAccount = ABIGAS.PK;

			Factory.Save();

			ZGuid wIPPK = chrg.WIP.PK;

			AssertNotNull("WIP Exists", chrg.WIP);
			AssertNull("Revenue Doesn't Exist", chrg.Revenue);

			ZDateTime now = ZDateTime.Now;
			ARInvoice invoice = Factory.New<ARInvoice>();
			ARInvoiceLine line = (ARInvoiceLine)invoice.Lines.AddNew();
			line.AL_PostDate = now;
			((IReceivablesPostingCharge)chrg).SetRevenueTransactionLine(line);

			WIP oldWIP = Factory.Load<WIP>(wIPPK);

			AssertNull("WIP Doesn't Exist", chrg.WIP);
			AssertEquals("WIP Reversed", true, oldWIP.IsReversed);
			AssertNotNull("Revenue Exists", chrg.Revenue);
			AssertEquals("Post Date", now, chrg.Revenue.AL_PostDate);
			AssertNotNull("Line.RelatedJobCharge", line.RelatedJobCharge);
			AssertEquals("They are for the same DataRow", chrg.PK, line.RelatedJobCharge.PK);
			AssertEquals("They are same BizO", chrg, line.RelatedJobCharge);
			Assert("OldWIP must be in the Charge ARLineValueHistory", chrg.GetARLineValueHistory().Contains(wIPPK));
			Assert("OldWIP must be in the Charge ARLineValueHistory", line.RelatedJobCharge.GetARLineValueHistory().Contains(wIPPK));
		}

		public void TestIDefaultLandedCostInput()
		{
			TestJob.JH_GE = GlbDepartment.CurrentDepartment.PK;

			AccChargeCode disbusementCharge = Factory.Load<AccChargeCode>(RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value);
			Charge newCharge1 = TestJob.Charges.AddNew();
			newCharge1.JR_AC = disbusementCharge.PK;
			newCharge1.JR_LocalSellAmt = 1000m;

			ZQuery filter = new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			filter.AddToFilter(AccChargeCodeSchema.PK, SQLComparisonOperator.NotEqual, RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value);
			filter.AddToFilter(AccChargeCodeSchema.PK, SQLComparisonOperator.NotEqual, Env.Registry.FreightChargeCode);

			AccChargeCode chargeCode = Factory.LoadTop1<AccChargeCode>(filter);
			Charge newCharge2 = TestJob.Charges.AddNew();
			newCharge2.JR_AC = chargeCode.PK;
			newCharge2.JR_LocalSellAmt = 1000m;

			AssertEquals("FK to charge Code", disbusementCharge.PK, ((IDefaultLandedCostInput)newCharge1).FKToChargeCode);
			AssertEquals("Amount", 1000m, ((IDefaultLandedCostInput)newCharge1).AmountToDistribute.Amount);
			AssertEquals("Currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, ((IDefaultLandedCostInput)newCharge1).AmountToDistribute.Currency.Code);
			AssertEquals("ExRate", 1m, ((IDefaultLandedCostInput)newCharge1).ExchangeRate);
			AssertEquals("IsValidToImport as this is Disbursement Charge", false, ((IDefaultLandedCostInput)newCharge1).IsValidToImport);

			AssertEquals("FK to charge Code", chargeCode.PK, ((IDefaultLandedCostInput)newCharge2).FKToChargeCode);
			AssertEquals("IsValidToImport", true, ((IDefaultLandedCostInput)newCharge2).IsValidToImport);

			AccChargeCode freightCharge = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);
			newCharge2.JR_AC = freightCharge.PK;
			AssertEquals("is valid to import", true, ((IDefaultLandedCostInput)newCharge2).IsValidToImport);

			DocumentsDataRegistry.Instance.DoNotPullZeroAmountsFromBillingTab.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, true);
			AssertEquals("is valid to import", false, ((IDefaultLandedCostInput)newCharge2).IsValidToImport);
			DocumentsDataRegistry.Instance.DoNotPullZeroAmountsFromBillingTab.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, false);

			BaseJobDeclaration dec = Factory.New<BaseJobDeclaration>();
			CusEntryHeader entryHeader = dec.CustomsEntryHeaders.AddNew();
			TestJob.JH_ParentID = dec.PK;
			TestJob.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			newCharge1.ClearfDisbursementChargeCodesForTesting();
			AssertEquals("Not IsValidToImport as this is Disbursement Charge", false, ((IDefaultLandedCostInput)newCharge1).IsValidToImport);
			newCharge2.ClearfDisbursementChargeCodesForTesting();
			AssertEquals("IsValidToImport", true, ((IDefaultLandedCostInput)newCharge2).IsValidToImport);

			EntryChargeTypeSettingCollection registrySettingCollection = new EntryChargeTypeSettingCollection(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			EntryChargeTypeSetting entryChargeTypeSetting = registrySettingCollection.AddNew();
			entryChargeTypeSetting.AC_ChargeCode = newCharge2.JR_AC;
			entryChargeTypeSetting.ChargeType = "DTY";
			RatingDataRegistry.Instance.EntryChargeTypesAndCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registrySettingCollection);
			newCharge2.ClearfDisbursementChargeCodesForTesting();
			AssertEquals("Not IsValidToImport now that charge code has been added to registry", false, ((IDefaultLandedCostInput)newCharge2).IsValidToImport);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Netherlands))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var header = declaration.CustomsEntryHeaders.AddNew();
				TestJob.JH_ParentID = declaration.PK;
				TestJob.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
				newCharge1.ClearfDisbursementChargeCodesForTesting();
				AssertEquals("Not IsValidToImport as this is Disbursement Charge", false, ((IDefaultLandedCostInput)newCharge1).IsValidToImport);
			}
		}

		public void TestLocalSellAmt()
		{
			bool originalIsGSTRegistered = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			string currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			try
			{
				Charge ch = Factory.New<Charge>();
				SetCurrentCompanyCountryCode(Core.Constants.CountryCodes.Iceland);
				SetCurrentCompanyCurrencyCode(Core.Constants.CurrencyCodes.Iceland);
				ch.JR_LocalSellAmt = 22.22m;
				AssertEquals("Local Sell amount should be rounded to int for icelandic company.", 22m, ch.JR_LocalSellAmt);
				ch.JR_LocalSellAmt = 44.50m;
				AssertEquals("Local Sell amount should be rounded to int.", 45.00m, ch.JR_LocalSellAmt);

				SetCurrentCompanyCurrencyCode(Core.Constants.CurrencyCodes.Australia);
				ch.JR_LocalSellAmt = 22.22m;
				AssertEquals("Local Sell amount shouldn't be rounded to int for not icelandic countries .", 22.22m, ch.JR_LocalSellAmt);

				SetCurrentCompanyCurrencyCode(Core.Constants.CurrencyCodes.Iceland);
				SetCurrentCompanyCountryCode(Core.Constants.CountryCodes.Australia);
				ch.JR_LocalSellAmt = 22.22m;
				AssertEquals("Local Sell amount shouldn't be rounded to int for not icelandic countries .", 22.22m, ch.JR_LocalSellAmt);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = originalIsGSTRegistered;
				SetCurrentCompanyCountryCode(currentCountry);
			}
		}

		public void TestJR_TotalLocalRevenue()
		{
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			var rate = CreateExchangeRate(job, TestObjectCreator.USD, .7M);
			var charge = CreateCharge(job, MRG100, "Cost Transaction Test", TestObjectCreator.USD, 250M, ZECTRA, TestObjectCreator.USD, 350M, AALSHI);

			AssertEquals("CFX Amount", 26.32m, charge.JR_CFXAmt);
			AssertEquals("Total Local Revenue should be sell - cfx ALWAYS", 526.32m, charge.JR_TotalLocalRevenue);

			charge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_OSSellAmt = 500m;
			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
			Assert("BillInInvoiceCurrency", charge.BillInInvoiceCurrency);
			Assert("BillInInvoiceCurrencyWithLocalSellCurrency", charge.BillInInvoiceCurrencyWithLocalSellCurrency);

			AssertEquals("CFX Amount", 25m, charge.JR_CFXAmt);
			AssertEquals("Total Local Revenue", 525m, charge.JR_TotalLocalRevenue);
		}

		public void TestJR_TotalTaxExpenseRevenueAndCost()
		{
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M);
			var apInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV001", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M);
			Factory.Save();

			var job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			var charge = CreateCharge(job, MRG100, "Test", TestObjectCreator.USD, 250M, ZECTRA, TestObjectCreator.USD, 350M, AALSHI);

			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);

			var arInvLine = arInvoice.Lines[0];
			taxProcessorMock.Setup(t => t.GetTaxExpenses(Factory, arInvLine.PK)).Returns(new[] { (ZDate.Today, new ZDecimal(2.05)) });

			AssertEquals(0m, charge.JR_TotalTaxExpenseRevenue);
			Assert(!charge.IsTaxExpense);
			taxProcessorMock.Verify(t => t.GetTaxExpenses(Factory, arInvLine.PK), Times.Never());

			charge.JR_AL_ARLine = arInvLine.PK;
			AssertEquals(2.05m, charge.JR_TotalTaxExpenseRevenue);
			taxProcessorMock.Verify(t => t.GetTaxExpenses(Factory, arInvLine.PK), Times.Once());
			Assert(charge.IsTaxExpense);

			var apInvLine = apInvoice.Lines[0];
			taxProcessorMock.Setup(t => t.GetTaxExpenses(Factory, apInvLine.PK)).Returns(new[] { (ZDate.Today, new ZDecimal(1.56m)) });

			AssertEquals(0m, charge.JR_TotalTaxExpenseCost);
			Assert(charge.IsTaxExpense);
			taxProcessorMock.Verify(t => t.GetTaxExpenses(Factory, apInvLine.PK), Times.Never());

			charge.JR_AL_APLine = apInvLine.PK;
			AssertEquals(1.56m, charge.JR_TotalTaxExpenseCost);
			taxProcessorMock.Verify(t => t.GetTaxExpenses(Factory, apInvLine.PK), Times.Once());
			Assert(charge.IsTaxExpense);

			arInvoice.AH_TransactionType = TransactionTypes.JobRevenueJournal;
			arInvLine.AL_LineType = TransactionLineTypes.Cost;
			AssertEquals(0m, charge.JR_TotalTaxExpenseRevenue);
			Assert(charge.IsTaxExpense);

			apInvLine.AL_LineType = TransactionLineTypes.Revenue;
			AssertEquals(0m, charge.JR_TotalTaxExpenseCost);
			Assert(!charge.IsTaxExpense);
		}

		public void TestCFXAmountWhenPostingWithSellInvCurrency()
		{
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			var sellInvoiceCurrencyRate = CreateExchangeRate(job, TestObjectCreator.USD, 2M);
			var charge = CreateCharge(job, MRG100, "CFX Amount When Posting Test", GlbCompany.CurrentCompany.LocalCurrency, 0M, ZECTRA, GlbCompany.CurrentCompany.LocalCurrency, 33M, AALSHI);
			var expectedCFXAmount = 1.65m;
			charge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
			charge.JR_OSSellAmt = 33m;
			AssertEquals("CFX Amount", expectedCFXAmount, charge.JR_CFXAmt);

			Factory.SetContext(BusinessContext.PostingReceivableCharges);
			AssertEquals("CFX Amount", expectedCFXAmount, charge.JR_CFXAmt);
		}

		public void TestCFXAmountWhenPostingWithSellInvCurrencyAndSellCurrency()
		{
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			Factory.RemoveContext(BusinessContext.PostingReceivableCharges);
			var expectedCFXAmount = 1.16m;
			var eurSellCurrencyRate = CreateExchangeRate(job, TestObjectCreator.EUR, 1.5m);
			eurSellCurrencyRate.JF_CFXPercent = 5m;
			var cnySellCurrencyRate = CreateExchangeRate(job, TestObjectCreator.CNY, 0.7m);
			cnySellCurrencyRate.JF_CFXPercent = 7m;
			var charge = CreateCharge(job, MRG100, "CFX Amount When Posting Test", TestObjectCreator.EUR, 0M, ZECTRA, TestObjectCreator.EUR, 33M, AALSHI);
			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
			charge.JR_RX_NKSellCurrency = TestObjectCreator.EUR.RX_Code;
			charge.JR_OSSellAmt = 33m;

			AssertEquals("CFX Amount", expectedCFXAmount, charge.JR_CFXAmt);

			Factory.SetContext(BusinessContext.PostingReceivableCharges);
			AssertEquals("CFX Amount", expectedCFXAmount, charge.JR_CFXAmt);

			charge.JR_RX_NKSellCurrency = TestObjectCreator.CNY.RX_Code;
			AssertEquals("CFX Amount", expectedCFXAmount, charge.JR_CFXAmt);
		}

		public void TestSellRateWithoutCFXAndSellInvoiceRateWithoutCFXAreIndependent()
		{
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			var sellInvoiceCurrencyRate = CreateExchangeRate(job, TestObjectCreator.USD, 2M);
			var rateSellCurrency = CreateExchangeRate(job, TestObjectCreator.EUR, 1.5M);
			var charge = CreateCharge(job, MRG100, "SellRateWithoutCFX And SellInvoiceRateWithoutCFX are independent Test", GlbCompany.CurrentCompany.LocalCurrency, 0M, ZECTRA, GlbCompany.CurrentCompany.LocalCurrency, 33M, AALSHI);

			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
			charge.JR_RX_NKSellCurrency = TestObjectCreator.EUR.RX_Code;
			AssertEquals("SellInvoiceRateWithoutCFX", 2M, charge.SellInvoiceRateWithoutCFX_ForTestOnly);
			AssertEquals("SellRateWithoutCFX", 1.5M, charge.SellRateWithoutCFX);

			sellInvoiceCurrencyRate.JF_BaseRate = 2.8M;
			AssertEquals("SellInvoiceRateWithoutCFX", 2.8M, charge.SellInvoiceRateWithoutCFX_ForTestOnly);
			AssertEquals("SellRateWithoutCFX", 1.5M, charge.SellRateWithoutCFX);

			rateSellCurrency.JF_BaseRate = 1.8M;
			AssertEquals("SellInvoiceRateWithoutCFX", 2.8M, charge.SellInvoiceRateWithoutCFX_ForTestOnly);
			AssertEquals("SellRateWithoutCFX", 1.8M, charge.SellRateWithoutCFX);
		}

		public void TestSellInvoiceRateWithoutCFXIsGettingUpdatedByOverrideOSSellInvoiceExRateForPosting()
		{
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			var sellInvoiceCurrencyRate = CreateExchangeRate(job, TestObjectCreator.USD, 2M);
			var charge = CreateCharge(job, MRG100, "SellInvoiceRateWithoutCFX Is Getting Updated By OverrideOSSellInvoiceExRateForPosting Test", GlbCompany.CurrentCompany.LocalCurrency, 0M, ZECTRA, GlbCompany.CurrentCompany.LocalCurrency, 33M, AALSHI);
			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
			AssertEquals("SellInvoiceRateWithoutCFX", 2M, charge.SellInvoiceRateWithoutCFX_ForTestOnly);

			charge.OverrideOSSellInvoiceExRateForPosting(3m);
			AssertEquals("SellInvoiceRateWithoutCFX", 3M, charge.SellInvoiceRateWithoutCFX_ForTestOnly);
		}

		public void TestJR_ChequeNoIsReadOnlyWhenAutoAllocationIsEnabled()
		{
			Job job = CreateJob("Z00001012", ZECTRA, true, 5M, ABIGAS, true, 10M);
			ExchangeRate rate1 = CreateExchangeRate(job, TestObjectCreator.USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(job, TestObjectCreator.GBP, .4M);
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();

			Charge chrg = job.Charges.AddNew();
			chrg.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			chrg.JR_AC = TestObjectCreator.MRG100.PK;
			chrg.JR_Desc = "Revenue Transaction Test";
			chrg.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			chrg.JR_OSCostAmt = 250m;
			chrg.JR_OH_CostAccount = AALSHI.PK;
			chrg.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			chrg.JR_OSSellAmt = 350m;
			chrg.JR_OH_SellAccount = ABIGAS.PK;
			chrg.JR_AB = testBank.PK;

			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook(testBank);
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			Assert("JR_ChequeNo should not be read only yet", !chrg.JR_ChequeNoInfo.ReadOnly);
			chrg.JR_AK = testBookWithAutoAllocation.PK;
			Assert("JR_ChequeNo should be read only as AutoAllocation is enabled now", chrg.JR_ChequeNoInfo.ReadOnly);
			chrg.JR_AK = testChequeBook.PK;
			Assert("JR_ChequeNo should not be read only again, as cheque book changed to not IAutoPrint", !chrg.JR_ChequeNoInfo.ReadOnly);
		}

		public void TestJR_AgentDeclaredAmounts_ShouldBeUpdatedRegardlessOf_JR_IsIncludedInProfitShare()
		{
			var charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_RX_NKCostCurrency = "USD";
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_OSCostAmt = 1000m;
			charge.JR_OSCostExRate = 1.3m;
			charge.JR_OSSellAmt = 2000m;
			charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(1.5m);
			AssertChargeAndAgentAmounts(charge);

			var charge2 = Factory.NewWithValidTestData<Charge>();
			charge2.JR_RX_NKCostCurrency = "USD";
			charge2.JR_RX_NKSellCurrency = "USD";
			charge2.JR_OSCostAmt = 1000m;
			charge2.JR_OSCostExRate = 1.3m;
			charge2.JR_OSSellAmt = 2000m;
			charge2.RevenueExchangeRate.SetBuyRate_ForTestOnly(1.5m);
			charge2.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;
			charge2.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;

			AssertChargeAndAgentAmounts(charge2);
		}

		void AssertChargeAndAgentAmounts(Charge charge)
		{
			AssertEquals("JR_AgentDeclaredCostAmt", 1000m, charge.JR_AgentDeclaredCostAmt);
			AssertEquals("JR_AgentDeclaredSellAmt", 2000m, charge.JR_AgentDeclaredSellAmt);
			AssertEquals("JR_AgentDeclaredCostAmtLocal", 769.23m, charge.JR_AgentDeclaredCostAmtLocal);
			AssertEquals("JR_AgentDeclaredSellAmtLocal", 1333.33m, charge.JR_AgentDeclaredSellAmtLocal);

			charge.JR_IsIncludedInProfitShare = true;
			AssertEquals("JR_AgentDeclaredCostAmt", 1000m, charge.JR_AgentDeclaredCostAmt);
			AssertEquals("JR_AgentDeclaredSellAmt", 2000m, charge.JR_AgentDeclaredSellAmt);
			AssertEquals("JR_AgentDeclaredCostAmtLocal", 769.23m, charge.JR_AgentDeclaredCostAmtLocal);
			AssertEquals("JR_AgentDeclaredSellAmtLocal", 1333.33m, charge.JR_AgentDeclaredSellAmtLocal);
		}

		public void TestJR_AgentDeclaredAmounts_ShouldBeUpdatedRegardlessOf_JR_IsIncludedInProfitShare_MixedCurrency()
		{
			var job = TestObjectCreator.CreateJob("S000010001", TestObjectCreator.LocalClient, 1.0m, TestObjectCreator.Agent, 1.0m);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, TestObjectCreator.CC1.AC_Desc, TestObjectCreator.AUD, 120m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 240m, TestObjectCreator.Debtor);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, TestObjectCreator.CC2.AC_Desc, TestObjectCreator.USD, 250m, TestObjectCreator.Creditor1, TestObjectCreator.USD, 250m, TestObjectCreator.Debtor);
			//job.ExchangeRates[0].JF_BaseRate = job.ExchangeRates[0].JF_SellRate = 0.5m;
			AssertEquals(2, job.ExchangeRates.Count);
			job.ExchangeRates[0].JF_BaseRate = 0.5m;
			job.ExchangeRates[1].JF_BaseRate = 0.5m;
			charge2.JR_OSSellAmt = 250m;

			AssertAgentDeclaredAmounts(false, charge1, 120m, 240m, 120m, 240m);
			AssertAgentDeclaredAmounts(false, charge2, 250m, 250m, 500m, 500m);

			AssertAgentDeclaredAmounts(true, charge1, 120m, 240m, 120m, 240m);
			AssertAgentDeclaredAmounts(true, charge2, 250m, 250m, 500m, 500m);
		}

		public void TestIsSavedByFactory_WithEvaluationInfo()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var shipment = objectCreator.CreateShipment("S00001");
			var job = objectCreator.CreateJob(shipment, objectCreator.LocalClient, 0M, objectCreator.Agent, 0M);
			Factory.Save();

			var expectedEvaluationInfo1 = @"IsSavedByFactoryBase: Yes
AnyOtherInstanceWithDifferentIsSavedByFactoryValue: No
IsDeleted: No
IsForConsolCostForIncompleteInvoice: No
IsInvoicingJobNotNull: Yes
IsInvoicingPlugInDataNull: No
IsPluginDeleted: No";
			var expectedEvaluationInfo2 = @"IsSavedByFactoryBase: Yes
AnyOtherInstanceWithDifferentIsSavedByFactoryValue: No
IsDeleted: No
IsForConsolCostForIncompleteInvoice: No
IsInvoicingJobNotNull: No
IsInvoicingPlugInDataNull: No
IsPluginDeleted: No";

			foreach (var chargeWithIsSavedByFactoryResult in new[]
					{
							(new { Charge = job.Charges.AddNew(), IsSavedByFactory = true, ExpectedMessage = expectedEvaluationInfo1 }),
							(new { Charge = Factory.New<Charge>(), IsSavedByFactory = false, ExpectedMessage = expectedEvaluationInfo2 })
						})
			{
				AssertEquals("Charges.IsSavedByFactory should be the same as Charge", chargeWithIsSavedByFactoryResult.IsSavedByFactory, chargeWithIsSavedByFactoryResult.Charge.IsSavedByFactory);
				AssertMultilineASCIIEquals("Charges.IsSavedByFactory Evaluation Info", chargeWithIsSavedByFactoryResult.ExpectedMessage, chargeWithIsSavedByFactoryResult.Charge.GetIsSavedByFactoryEvaluationInfo());
			}
		}

		void AssertAgentDeclaredAmounts(bool isIncludedInProfitShareChecked, Charge charge, decimal expectedJR_AgentDeclaredCostAmt, decimal expectedJR_AgentDeclaredSellAmt, decimal expectedJR_AgentDeclaredCostAmtLocal, decimal expectedJR_AgentDeclaredSellAmtLocal)
		{
			charge.JR_IsIncludedInProfitShare = isIncludedInProfitShareChecked;
			AssertEquals(string.Format("JR_AgentDeclaredCostAmt (Before JR_IsIncludedInProfitShare is Checked-{0})", isIncludedInProfitShareChecked ? "On" : "Off"), expectedJR_AgentDeclaredCostAmt, charge.JR_AgentDeclaredCostAmt);
			AssertEquals(string.Format("JR_AgentDeclaredSellAmt (Before JR_IsIncludedInProfitShare is Checked-{0})", isIncludedInProfitShareChecked ? "On" : "Off"), expectedJR_AgentDeclaredSellAmt, charge.JR_AgentDeclaredSellAmt);
			AssertEquals(string.Format("JR_AgentDeclaredCostAmtLocal (Before JR_IsIncludedInProfitShare is Checked-{0})", isIncludedInProfitShareChecked ? "On" : "Off"), expectedJR_AgentDeclaredCostAmtLocal, charge.JR_AgentDeclaredCostAmtLocal);
			AssertEquals(string.Format("JR_AgentDeclaredSellAmtLocal (Before JR_IsIncludedInProfitShare is Checked-{0})", isIncludedInProfitShareChecked ? "On" : "Off"), expectedJR_AgentDeclaredSellAmtLocal, charge.JR_AgentDeclaredSellAmtLocal);
		}

		#region Autorating Tests

		public void TestReadOnlyForRatedChargesWhenPreventFlagIsTrue()
		{
			AccountingConfigurationRegistry.Instance.PreventOperatorFromChangingRatedLine.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			Assert("Pre-condition - registry true for prevent autorated lines from being changed", AccountingConfigurationRegistry.Instance.PreventOperatorFromChangingRatedLine.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));

			Charge ratedCharge = Factory.New<Charge>();
			ratedCharge.JR_LocalCostAmt = 200;
			ratedCharge.JR_OSCostAmt = 220;
			ratedCharge.JR_RX_NKCostCurrency = TestObjectCreator.GBP.RX_Code;

			Assert("Cost fields not readonly", !ratedCharge.JR_LocalCostAmtInfo.ReadOnly);
			Assert("Cost fields not readonly", !ratedCharge.JR_OSCostAmtInfo.ReadOnly);
			Assert("Cost fields not readonly", !ratedCharge.JR_RX_NKCostCurrencyInfo.ReadOnly);
			Assert("Cost fields not readonly", !ratedCharge.JR_OH_CostAccountInfo.ReadOnly);

			Assert("Sell fields not readonly", !ratedCharge.JR_LocalSellAmtInfo.ReadOnly);
			Assert("Sell fields not readonly", !ratedCharge.JR_RX_NKSellCurrencyInfo.ReadOnly);
			Assert("Sell fields not readonly", !ratedCharge.JR_OH_SellAccountInfo.ReadOnly);

			ratedCharge.JR_CostRated = true;

			Assert("Cost fields readonly", ratedCharge.JR_LocalCostAmtInfo.ReadOnly);
			Assert("Cost fields readonly", ratedCharge.JR_OSCostAmtInfo.ReadOnly);
			Assert("Cost fields readonly", ratedCharge.JR_RX_NKCostCurrencyInfo.ReadOnly);
			Assert("Cost Creditor not readonly as no creditor was specified", !ratedCharge.JR_OH_CostAccountInfo.ReadOnly);

			ratedCharge.JR_OH_CostAccount = ZECTRA.PK;
			ratedCharge.JR_CostRated = true;
			Assert("Cost Creditor readonly as creditor was specified", ratedCharge.JR_OH_CostAccountInfo.ReadOnly);

			Assert("Sell fields not readonly", !ratedCharge.JR_LocalSellAmtInfo.ReadOnly);
			Assert("Sell fields not readonly", !ratedCharge.JR_RX_NKSellCurrencyInfo.ReadOnly);
			Assert("Sell fields not readonly", !ratedCharge.JR_OH_SellAccountInfo.ReadOnly);

			ratedCharge.JR_LocalSellAmt = 500;
			ratedCharge.JR_OSSellAmt = 587;
			ratedCharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			ratedCharge.JR_OH_SellAccount = ABIGAS.PK;

			Assert("Sell fields not readonly", !ratedCharge.JR_LocalSellAmtInfo.ReadOnly);
			Assert("Sell fields not readonly", !ratedCharge.JR_RX_NKSellCurrencyInfo.ReadOnly);
			Assert("Sell fields not readonly", !ratedCharge.JR_OH_SellAccountInfo.ReadOnly);

			ratedCharge.JR_SellRated = true;
			Assert("Sell fields readonly", ratedCharge.JR_LocalSellAmtInfo.ReadOnly);
			Assert("Sell fields readonly", ratedCharge.JR_OSSellAmtInfo.ReadOnly);
			Assert("Sell fields readonly", ratedCharge.JR_RX_NKSellCurrencyInfo.ReadOnly);
			Assert("Sell fields readonly", ratedCharge.JR_OH_SellAccountInfo.ReadOnly);
		}

		public void TestReadOnlyForRatedChargesWhenPreventFlagIsFalse()
		{
			AccountingConfigurationRegistry.Instance.PreventOperatorFromChangingRatedLine.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			Assert("Pre-condition - registry false for prevent autorated lines from being changed", !AccountingConfigurationRegistry.Instance.PreventOperatorFromChangingRatedLine.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));

			Charge ratedCharge = Factory.New<Charge>();
			ratedCharge.JR_LocalCostAmt = 200;
			ratedCharge.JR_OSCostAmt = 220;
			ratedCharge.JR_RX_NKCostCurrency = TestObjectCreator.GBP.RX_Code;

			Assert("Cost fields not readonly", !ratedCharge.JR_LocalCostAmtInfo.ReadOnly);
			Assert("Cost fields not readonly", !ratedCharge.JR_OSCostAmtInfo.ReadOnly);
			Assert("Cost fields not readonly", !ratedCharge.JR_RX_NKCostCurrencyInfo.ReadOnly);
			Assert("Cost fields not readonly", !ratedCharge.JR_OH_CostAccountInfo.ReadOnly);

			Assert("Sell fields not readonly", !ratedCharge.JR_LocalSellAmtInfo.ReadOnly);
			Assert("Sell fields not readonly", !ratedCharge.JR_RX_NKSellCurrencyInfo.ReadOnly);
			Assert("Sell fields not readonly", !ratedCharge.JR_OH_SellAccountInfo.ReadOnly);

			ratedCharge.JR_CostRated = true;

			Assert("Cost fields not readonly", !ratedCharge.JR_LocalCostAmtInfo.ReadOnly);
			Assert("Cost fields not readonly", !ratedCharge.JR_OSCostAmtInfo.ReadOnly);
			Assert("Cost fields not readonly", !ratedCharge.JR_RX_NKCostCurrencyInfo.ReadOnly);
			Assert("Cost Creditor not readonly as no creditor was specified", !ratedCharge.JR_OH_CostAccountInfo.ReadOnly);

			ratedCharge.JR_OH_CostAccount = ZECTRA.PK;
			ratedCharge.JR_CostRated = true;
			Assert("Cost Creditor not readonly as reg flag is false", !ratedCharge.JR_OH_CostAccountInfo.ReadOnly);

			Assert("Sell fields not readonly", !ratedCharge.JR_LocalSellAmtInfo.ReadOnly);
			Assert("Sell fields not readonly", !ratedCharge.JR_RX_NKSellCurrencyInfo.ReadOnly);
			Assert("Sell fields not readonly", !ratedCharge.JR_OH_SellAccountInfo.ReadOnly);

			ratedCharge.JR_LocalSellAmt = 500;
			ratedCharge.JR_OSSellAmt = 587;
			ratedCharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			ratedCharge.JR_OH_SellAccount = ABIGAS.PK;

			Assert("Sell fields not readonly", !ratedCharge.JR_LocalSellAmtInfo.ReadOnly);
			Assert("Sell fields not readonly", !ratedCharge.JR_RX_NKSellCurrencyInfo.ReadOnly);
			Assert("Sell fields not readonly", !ratedCharge.JR_OH_SellAccountInfo.ReadOnly);

			ratedCharge.JR_SellRated = true;
			Assert("Sell fields not readonly", !ratedCharge.JR_LocalSellAmtInfo.ReadOnly);
			Assert("Sell fields not readonly", !ratedCharge.JR_OSSellAmtInfo.ReadOnly);
			Assert("Sell fields not readonly", !ratedCharge.JR_RX_NKSellCurrencyInfo.ReadOnly);
			Assert("Sell fields not readonly", !ratedCharge.JR_OH_SellAccountInfo.ReadOnly);
		}

		#endregion

		#region Comment Charge Code ReadOnly Status

		public void TestReadOnlyPropertiesForCommentChargeCode()
		{
			Charge testCharge = SetUpForReadOnlyTest(InvoiceTypesList.Codes.FinalInvoice);

			testCharge.UpdateJR_AT_SellGSTRateReadOnly_ForTestOnly();
			testCharge.UpdateJR_AT_CostGSTRateReadOnly_ForTestOnly();

			AssertForCommentCharge(testCharge);
			Assert("Cost TestObjectCreator.GST1 Rate", testCharge.JR_AT_CostGSTRateInfo.ReadOnly);
			Assert("Sell TestObjectCreator.GST1 Rate", testCharge.JR_AT_SellGSTRateInfo.ReadOnly);
		}

		Charge SetUpForReadOnlyTest(string invoiceType)
		{
			ZECTRA.CompanyData.SetARTaxApplicable(true);
			ZECTRA.CompanyData.SetAPTaxApplicable(true);

			AccChargeCode testChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			testChargeCode.AC_Code = "TESTCMT";
			testChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			testChargeCode.AC_MarginPercentage = 0.0m;
			testChargeCode.AC_DepartmentFilterList = "ALL";
			testChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;

			Job job = CreateJob("Z00001012", ZECTRA, true, 5M, ABIGAS, true, 10M);

			Charge testCharge = job.Charges.AddNew();
			testCharge.JR_OH_SellAccount = ZECTRA.PK;
			testCharge.JR_AC = testChargeCode.PK;
			testCharge.JR_InvoiceType = invoiceType;
			return testCharge;
		}

		public void TestReadOnlyPropertiesForCommentChargeCodeWithTaxInvoiceType()
		{
			AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			Charge testCharge = SetUpForReadOnlyTest(InvoiceTypesList.Codes.InvoicePerTaxCode);

			testCharge.UpdateJR_AT_SellGSTRateReadOnly_ForTestOnly();
			testCharge.UpdateJR_AT_CostGSTRateReadOnly_ForTestOnly();

			AssertForCommentCharge(testCharge);
			Assert("Cost TestObjectCreator.GST1 Rate", testCharge.JR_AT_CostGSTRateInfo.ReadOnly);
			Assert("Sell TestObjectCreator.GST1 Rate", testCharge.JR_AT_SellGSTRateInfo.ReadOnly);
		}

		void AssertForCommentCharge(Charge testCharge)
		{
			Assert("OS Cost Amount", testCharge.JR_OSCostAmtInfo.ReadOnly);
			Assert("Local Cost Amount", testCharge.JR_LocalCostAmtInfo.ReadOnly);
			Assert("Local Account", testCharge.JR_OH_CostAccountInfo.ReadOnly);
			Assert("Cost Currency", testCharge.JR_RX_NKCostCurrencyInfo.ReadOnly);

			Assert("OS Sell Amount", testCharge.JR_OSSellAmtInfo.ReadOnly);
			Assert("Local Sell Amount", testCharge.JR_LocalSellAmtInfo.ReadOnly);
			Assert("Sell Currency", !testCharge.JR_RX_NKSellCurrencyInfo.ReadOnly);

			Assert("Cost Payment Bank Account", testCharge.JR_ABInfo.ReadOnly);
			Assert("AP Invoice Date", testCharge.JR_APInvoiceDateInfo.ReadOnly);
			Assert("AP Document Received Date", testCharge.JR_APDocumentReceivedDateInfo.ReadOnly);
			Assert("AP Invoice Num", testCharge.JR_APInvoiceNumInfo.ReadOnly);
			Assert("AR Invoice Number", testCharge.JR_ARInvoiceNumberInfo.ReadOnly);
			Assert("Supplier Cost Reference", testCharge.JR_CostReferenceInfo.ReadOnly);
			Assert("Payment Date", testCharge.JR_PaymentDateInfo.ReadOnly);
			Assert("Payment Type", testCharge.JR_PaymentTypeInfo.ReadOnly);

			Assert("Cheque Book", testCharge.JR_AKInfo.ReadOnly);
			Assert("Cheque Reference", testCharge.JR_ChequeNoInfo.ReadOnly);

			Assert("Profit Share Enabled", testCharge.JR_IsIncludedInProfitShareInfo.ReadOnly);
			Assert("Agent Decalred Cost Amount", testCharge.JR_AgentDeclaredCostAmtInfo.ReadOnly);
			Assert("Agent Decalred Sell Amount", testCharge.JR_AgentDeclaredSellAmtInfo.ReadOnly);
		}

		public void TestReadOnlyPropertiesForCommentChargeCodeAfterSave()
		{
			Charge testCharge = SetUpForReadOnlyTest(InvoiceTypesList.Codes.FinalInvoice);
			Factory.Save();

			AssertForCommentCharge(testCharge);
		}

		public void TestReadOnlyPropertiesForCommentChargeCodeAfterLoad()
		{
			Charge testCharge = SetUpForReadOnlyTest(InvoiceTypesList.Codes.FinalInvoice);
			Factory.Save();

			Charge retrievedCharge = Factory.Load(typeof(Charge), testCharge.PK) as Charge;
			AssertForCommentCharge(retrievedCharge);
		}

		public void TestJR_InvoiceType_ReadOnly()
		{
			Charge testCharge = SetUpForReadOnlyTest(InvoiceTypesList.Codes.FinalInvoice);
			AssertEquals("IsRevenuePosted", false, testCharge.JR_IsRevenuePosted);
			AssertEquals("InvoiceType ReadOnly", false, testCharge.JR_InvoiceTypeInfo.ReadOnly);

			testCharge.JR_AL_ARLine = Factory.New<AccTransactionLines>().PK;
			testCharge.ARLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			AssertEquals("IsRevenuePosted", true, testCharge.JR_IsRevenuePosted);
			AssertEquals("InvoiceType ReadOnly", true, testCharge.JR_InvoiceTypeInfo.ReadOnly);

			testCharge.ARLine.AL_LineType = "";
			AssertEquals("IsRevenuePosted", false, testCharge.JR_IsRevenuePosted);
			AssertEquals("InvoiceType ReadOnly", false, testCharge.JR_InvoiceTypeInfo.ReadOnly);
		}

		public void TestReadOnlyPropertiesForCreditorThatHasSelfBillingInvoicesFlagSet()
		{
			Job job = CreateJob("Z00001012", ZECTRA, true, 5M, ABIGAS, true, 10M);
			Charge charge = job.Charges.AddNew();
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_AC = TestObjectCreator.CC1.PK;

			AssertEquals("AP Invoice Date readonly", false, charge.JR_APInvoiceDateInfo.ReadOnly);
			AssertEquals("AP Document Received Date readonly", false, charge.JR_APDocumentReceivedDateInfo.ReadOnly);
			AssertEquals("AP Invoice Num readonly", false, charge.JR_APInvoiceNumInfo.ReadOnly);
			AssertEquals("Payment Date readonly", false, charge.JR_PaymentDateInfo.ReadOnly);

			charge.JR_APInvoiceNum = "123456";
			charge.JR_APInvoiceDate = charge.JR_PaymentDate = ZDateTime.Today;
			TestObjectCreator.ABIGAS.CompanyData.OB_APCostsSelfBilled = true;
			charge.JR_OH_CostAccount = TestObjectCreator.ABIGAS.PK;
			AssertEquals("AP Invoice Date", ZDateTime.Empty, charge.JR_APInvoiceDate);
			AssertEquals("AP Invoice Num", ZString.Empty, charge.JR_APInvoiceNum);
			AssertEquals("Payment Date", ZDateTime.Empty, charge.JR_PaymentDate);
			AssertEquals("AP Invoice Date readonly", true, charge.JR_APInvoiceDateInfo.ReadOnly);
			AssertEquals("AP Document Received Date readonly", true, charge.JR_APDocumentReceivedDateInfo.ReadOnly);
			AssertEquals("AP Invoice Num readonly", true, charge.JR_APInvoiceNumInfo.ReadOnly);
			AssertEquals("Payment Date readonly", true, charge.JR_PaymentDateInfo.ReadOnly);
		}

		#endregion

		#region Profit Share Agent Declared Amounts Read Only

		public void TestProfitShareAgentDeclaredAmountsReadOnly()
		{
			SecurityCheckpoint checkPoint = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.ModifyProfitShareAmounts);
			bool isAllowed = checkPoint.IsAllowed;
			try
			{
				Charge charge = Factory.NewWithValidTestData<Charge>();
				Assert(!charge.JR_AgentDeclaredCostAmtInfo.ReadOnly);
				Assert(!charge.JR_AgentDeclaredCostAmtLocalInfo.ReadOnly);
				Assert(!charge.JR_AgentDeclaredSellAmtInfo.ReadOnly);
				Assert(!charge.JR_AgentDeclaredSellAmtLocalInfo.ReadOnly);

				ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_UniqueConsignRef = "S0001111";
				Job job = CreateJob(shipment.JS_UniqueConsignRef, ZECTRA, true, 5M, ABIGAS, true, 10M);
				job.Parent = shipment;
				charge = job.Charges.AddNew();
				Assert(!charge.JR_AgentDeclaredCostAmtInfo.ReadOnly);
				Assert(!charge.JR_AgentDeclaredCostAmtLocalInfo.ReadOnly);
				Assert(!charge.JR_AgentDeclaredSellAmtInfo.ReadOnly);
				Assert(!charge.JR_AgentDeclaredSellAmtLocalInfo.ReadOnly);

				checkPoint.IsAllowed = false;
				Assert(charge.JR_AgentDeclaredCostAmtInfo.ReadOnly);
				Assert(charge.JR_AgentDeclaredCostAmtLocalInfo.ReadOnly);
				Assert(charge.JR_AgentDeclaredSellAmtInfo.ReadOnly);
				Assert(charge.JR_AgentDeclaredSellAmtLocalInfo.ReadOnly);
			}
			finally
			{
				checkPoint.IsAllowed = isAllowed;
			}
		}

		#endregion

		public void TestJR_LocalSellInvoiceAmtAndOtherSellInvoiceCurrencyProperties()
		{
			var shipment = TestObjectCreator.CreateShipment("S1");
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.LocalChargesPK = TestObjectCreator.AALSHI.PK;
			job.AgentCollectPK = TestObjectCreator.ABIGAS.PK;

			var charge = job.Charges.AddNew();
			charge.JR_RX_NKSellCurrency = TestObjectCreator.EUR.RX_Code;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			charge.JR_OSSellAmt = 1000m;
			charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(0.5m);
			Assert("Sell Invoice Currency is not set", charge.JR_RX_NKSellInvoiceCurrency.IsEmpty);
			AssertEquals("BillInLocalCurrency", true, charge.BillInLocalCurrency);
			AssertEquals("BillInInvoiceCurrency", false, charge.BillInInvoiceCurrency);

			AssertSellInvoiceCurrencyProperties(charge, 2000m, 2000m, 0m, 0m, 0m, "", "", "");

			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
			AssertEquals("BillInLocalCurrency", false, charge.BillInLocalCurrency);
			AssertEquals("BillInInvoiceCurrency", true, charge.BillInInvoiceCurrency);
			var exRate = charge.SellInvoiceExchangeRate;
			AssertNotNull(exRate);
			exRate.SetBuyRate_ForTestOnly(0m);
			AssertEquals("Job Buy Rate", 0m, exRate.Rate);
			AssertEquals("JF_SellRate", 0m, exRate.SellRate);

			AssertSellInvoiceCurrencyProperties(charge, 2000m, 2000m, 0m, 0m, 0m, "0.00", "0.00", "0.000000");

			exRate.SetBuyRate_ForTestOnly(0.5m);
			exRate.EnsureWillNotBeAutoDeleted(); //we need it to be picked up when we set SellInvoiceCurrency back to USD;
			AssertEquals("JF_SellRate", 0.5m, exRate.SellRate);

			AssertSellInvoiceCurrencyProperties(charge, 2000m, 2000m, 1000m, 100m, 0.5m, "1,000.00", "100.00", "0.500000");

			charge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.LocalCurrency.Code;
			charge.JR_OSSellAmt = 2000m;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;

			AssertSellInvoiceCurrencyProperties(charge, 2000m, 2000m, 1000m, 100m, 0.5m, "1,000.00", "100.00", "0.500000");

			Assert("Not a Local Client Charge", !charge.IsLocalClientCharge);
			Assert("Not an Agent Charge", !charge.IsAgentCharge);

			AssertSellInvoiceCurrencyProperties(charge, 2000m, 2000m, 1000m, 100m, 0.5m, "1,000.00", "100.00", "0.500000");

			charge.JR_OH_SellAccount = job.LocalChargesPK;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
			charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			Assert("Is Local Client Charge", charge.IsLocalClientCharge);
			Assert("Not an Agent Charge", !charge.IsAgentCharge);

			AssertSellInvoiceCurrencyProperties(charge, 2000m, 2000m, 1000m, 100m, 0.5m, "1,000.00", "100.00", "0.500000");

			//setting CFX directly on exchange rate - configuration won't take effect
			var usdRate = job.ExchangeRates.Cast<ExchangeRate>().First(e => e.JF_RX_NKRateCurrency == "USD");
			usdRate.JF_CFXPercent = 5m;

			AssertSellInvoiceCurrencyProperties(charge, 2000m, 2100m, 1050m, 105m, 0.5m, "1,050.00", "105.00", "0.500000");

			usdRate.JF_OH_Org = ZGuid.Empty;

			charge.JR_OH_SellAccount = job.AgentCollectPK;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
			charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			Assert("Not a Local Client Charge", !charge.IsLocalClientCharge);
			Assert("Is Agent Charge", charge.IsAgentCharge);

			usdRate.JF_CFXPercent = 0m;

			AssertSellInvoiceCurrencyProperties(charge, 2000m, 2000m, 1000m, 100m, 0.5m, "1,000.00", "100.00", "0.500000");

			usdRate.JF_CFXPercent = 7m;

			AssertSellInvoiceCurrencyProperties(charge, 2000m, 2140m, 1070m, 107m, 0.5m, "1,070.00", "107.00", "0.500000");

			var revLine = Factory.NewWithValidTestData<ARInvoiceLine>();
			charge.JR_AL_ARLine = revLine.PK;

			AssertNotEquals("Currencies are different", charge.JR_RX_NKSellInvoiceCurrency, revLine.AL_RX_NKTransactionCurrency);
			AssertSellInvoiceCurrencyProperties(charge, 2000m, 2140m, 1070m, 107m, 0.5m, "1,070.00", "107.00", "0.500000");

			revLine.AL_RX_NKTransactionCurrency = charge.JR_RX_NKSellInvoiceCurrency;
			revLine.AL_ExchangeRate = 0.55m;
			revLine.AL_LineAmount = 2200;
			revLine.AL_GSTVAT = 220m;
			revLine.AL_OSAmount = 1210m;

			AssertSellInvoiceCurrencyProperties(charge, 2000m, 2200m, 1210m, 121m, 0.55m, "1,210.00", "121.00", "0.550000");
		}

		void AssertSellInvoiceCurrencyProperties(Charge charge, ZDecimal localAmt, ZDecimal localInvoiceAmt, ZDecimal osSellInvoiceAmt, ZDecimal osSellInvoiceGSTAmt, ZDecimal exRate,
			string osSellInvoiceAmt_Display, string osSellInvoiceGSTAmt_Display, string exRate_Display)
		{
			AssertEquals("JR_LocalSellAmt", localAmt, charge.JR_LocalSellAmt);
			AssertEquals("JR_LocalSellInvoiceAmt", localInvoiceAmt, charge.JR_LocalSellInvoiceAmt);

			AssertEquals("JR_OSSellInvoiceAmt", osSellInvoiceAmt, charge.JR_OSSellInvoiceAmt);
			AssertEquals("JR_OSSellInvoiceGSTAmt", osSellInvoiceGSTAmt, charge.JR_OSSellInvoiceGSTAmt);
			AssertEquals("JR_OSSellInvoiceExRate", exRate, charge.JR_OSSellInvoiceExRate);

			AssertEquals("JR_OSSellInvoiceAmt_ForDisplay", osSellInvoiceAmt_Display, charge.JR_OSSellInvoiceAmt_ForDisplay);
			AssertEquals("JR_OSSellInvoiceGSTAmt_ForDisplay", osSellInvoiceGSTAmt_Display, charge.JR_OSSellInvoiceGSTAmt_ForDisplay);
			AssertEquals("JR_OSSellInvoiceExRate_ForDisplay", exRate_Display, charge.JR_OSSellInvoiceExRate_ForDisplay);
		}

		public void TestCalculateTaxAtLineLevel()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			var systemSubUnitRatio = GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio;
			var systemIsGSTRegistered = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			try
			{
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = 0;
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
				testObjectCreator.GST1.SetRateNumerator_ForTestOnly(5);

				RefCurrency tWD = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.Taiwan);

				Job testJob = Factory.NewJobForTesting<Job>();
				testJob.JH_GB = GlbBranch.CurrentBranch.PK;
				testJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
				testJob.JH_JobNum = "Z00001234";

				AALSHI.CompanyData.SetARTaxApplicable(true);
				ABIGAS.CompanyData.SetARTaxApplicable(true);

				Charge charge1 = CreateChargeForTest(testJob, testObjectCreator.CC1, "Desc", tWD, 90m, AALSHI, tWD, 90m, ABIGAS);
				AssertOSChargeAmounts(charge1, 90m, 5m, 90m, 5m);

				Charge charge2 = CreateChargeForTest(testJob, testObjectCreator.CC1, "Desc", tWD, 110m, AALSHI, tWD, 110m, ABIGAS);
				AssertOSChargeAmounts(charge1, 90m, 5m, 90m, 5m);
				AssertOSChargeAmounts(charge2, 110m, 6m, 110m, 6m);

				Charge charge3 = CreateChargeForTest(testJob, testObjectCreator.CC1, "Desc", tWD, 130m, AALSHI, tWD, 130m, ABIGAS);
				AssertOSChargeAmounts(charge1, 90m, 5m, 90m, 5m);
				AssertOSChargeAmounts(charge2, 110m, 6m, 110m, 6m);
				AssertOSChargeAmounts(charge3, 130m, 7m, 130m, 7m);
			}
			finally
			{
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = systemSubUnitRatio;
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = systemIsGSTRegistered;
			}
		}

		void SetCurrentCountryToCode(string code)
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.StartsWith, code)).Code;
		}

		public void TestBalancingLineTaxAmountWhenCurrentCountryAndCurrencyIsIcelandic()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			var systemSubUnitRatio = GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio;
			var systemIsGSTRegistered = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			try
			{
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = 0;
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
				testObjectCreator.GST1.SetRateNumerator_ForTestOnly(10);

				string currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
				RefCurrency iSK = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.Iceland);

				SetCurrentCountryToCode(Core.Constants.CountryCodes.Iceland);
				SetCurrentCompanyCurrencyCode(Core.Constants.CurrencyCodes.Iceland);
				Job testJob = Factory.NewJobForTesting<Job>();
				testJob.JH_GB = GlbBranch.CurrentBranch.PK;
				testJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
				testJob.JH_JobNum = "Z00001234";

				AALSHI.CompanyData.SetAPTaxApplicableIgnoringRegistrySetting(true);
				AALSHI.CompanyData.SetARTaxApplicable(true);
				ABIGAS.CompanyData.SetARTaxApplicable(true);

				Charge charge1 = CreateChargeForTest(testJob, testObjectCreator.CC1, "Desc", iSK, 100m, AALSHI, iSK, 203m, ABIGAS);
				AssertOSChargeAmounts(charge1, 100m, 10m, 203m, 20m);

				Charge charge2 = CreateChargeForTest(testJob, testObjectCreator.CC1, "Desc", iSK, 100m, AALSHI, iSK, 102m, ABIGAS);
				AssertOSChargeAmounts(charge1, 100m, 10m, 203m, 20m);
				AssertOSChargeAmounts(charge2, 100m, 10m, 102m, 10m);

				Charge charge3 = CreateChargeForTest(testJob, testObjectCreator.CC1, "Desc", iSK, 100m, AALSHI, iSK, 11m, ABIGAS);
				AssertOSChargeAmounts(charge1, 100m, 10m, 203m, 20m);
				AssertOSChargeAmounts(charge2, 100m, 10m, 102m, 10m);
				AssertOSChargeAmounts(charge3, 100m, 10m, 11m, 1m);
			}
			finally
			{
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = systemSubUnitRatio;
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = systemIsGSTRegistered;
			}
		}

		void AssertOSChargeAmounts(Charge charge, decimal osCostAmt, decimal osCostTaxAmt, decimal osSellAmt, decimal osSellTaxAmt)
		{
			AssertEquals("Overseas Cost Amt", osCostAmt, charge.JR_OSCostAmt);
			AssertEquals("Overseas Cost Tax Amt", osCostTaxAmt, charge.JR_OSCostGSTAmt_Calc);

			AssertEquals("Overseas Sell Amt", osSellAmt, charge.JR_OSSellAmt);
			AssertEquals("Overseas Sell Tax Amt", osSellTaxAmt, charge.JR_OSSellGSTAmt_Calc);
		}

		protected Charge CreateChargeForTest(Job parentJob, AccChargeCode chargeCode, ZString desc, RefCurrency costCurrency, ZDecimal oSCostAmt, OrgHeader creditor,
RefCurrency sellCurrency, ZDecimal oSSellAmt, OrgHeader debtor)
		{
			Charge charge = parentJob.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_Desc = desc;

			charge.JR_OH_CostAccount = creditor != null ? creditor.PK : ZGuid.Empty;
			charge.JR_RX_NKCostCurrency = costCurrency.RX_Code;
			if ((charge.CostExchangeRate?.Rate ?? 0m) == 0m)
			{
				charge.CostExchangeRate?.SetBuyRate_ForTestOnly(1m);
			}
			charge.JR_OSCostAmt = oSCostAmt;

			charge.JR_OH_SellAccount = debtor.PK;
			charge.JR_RX_NKSellCurrency = sellCurrency.RX_Code;
			if ((charge.RevenueExchangeRate?.Rate ?? 0m) == 0m)
			{
				charge.RevenueExchangeRate?.SetBuyRate_ForTestOnly(1m);
			}
			charge.JR_InvoiceType = ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;

			charge.JR_OSSellAmt = oSSellAmt;

			return charge;
		}

		public void TestJR_RelatedConsolRef()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "TESTREF";
			Factory.Save();
			Charge charge = Factory.New<Charge>();
			AssertEquals(ZString.Empty, charge.JR_RelatedConsolRef);
			JobConsolCost consolCost = consol.GetApportionments().CostsCollection.TryAddNew();
			charge.JR_E6 = consolCost.PK;
			AssertEquals("TESTREF", charge.JR_RelatedConsolRef);
		}

		#region ConsumerAdditionalData

		public void TestConsumerAdditionalData()
		{
			var charge = (Charge)ACharge;
			charge.Job.Parent = new JobHeaderWithAdditionalDataPropertiesForTest();

			var chargeCustomProperties = ((ICustomPropertyContainer)charge).CustomProperties;
			AssertNotNull(chargeCustomProperties);
			AssertEquals("123", chargeCustomProperties.First(c => c.Identifier == "TESTSTRING").GetValue(charge));
			AssertEquals(1m, chargeCustomProperties.First(c => c.Identifier == "TESTDECIMAL").GetValue(charge));
		}

		class JobHeaderWithAdditionalDataPropertiesForTest : NonPersistentBusinessObject, IJobHeaderParent, IJobInvoicingAdditionalData
		{
			#region IJobHeaderParent

			string IJobNumber.JobNumber
			{
				get { return "JobHeader"; }
			}

			void IJobHeaderParent.SetJobNumberFieldOnSaving()
			{
			}

			void IJobHeaderParent.OnJobCreating(JobHeader job)
			{
			}

			void IJobHeaderParent.OnJobCreated(JobHeader job)
			{
			}

			void IJobHeaderParent.OnJobDeleting(JobHeader job)
			{
			}

			void IJobHeaderParent.OnJobDeleted(JobHeader job)
			{
			}

			bool IJobHeaderParent.AllowInvoiceDeletion
			{
				get { return true; }
			}

			#endregion

			#region IJobInvoicingAdditionalData

			public CustomPropertyContainer<JobCharge> GetAdditionalProperties()
			{
				var additionalProperties = new CustomPropertyContainer<JobCharge>();
				additionalProperties.AddCustomProperty("TESTSTRING", "TESTCAPTION", "123");
				additionalProperties.AddCustomProperty("TESTDECIMAL", typeof(ZDecimal), c => 1m);
				return additionalProperties;
			}

			#endregion
		}

		#endregion

		public void TestJR_Sell_LocalGSTAmountIncludingTaxAdjustment_ForeignCurrencyChargeAndLocalCurrencyInvoice_LocalTaxMustBeCalculatedFromLocalAmount()
		{
			TestObjectCreator.ABIGAS.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			TestObjectCreator.SetExchangeRate(TestJob, TestObjectCreator.USD, 0.815M);
			TestObjectCreator.GST1.SetRateNumerator_ForTestOnly(10);
			RefCurrency chargeCurrency = TestObjectCreator.USD;
			Charge testCharge1 = TestObjectCreator.CreateCharge(TestJob, TestObjectCreator.CC1, "Desc", chargeCurrency, 0M, TestObjectCreator.AALSHI, chargeCurrency, 2.44M, TestObjectCreator.ABIGAS);
			testCharge1.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			testCharge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			testCharge1.JR_GE = TestObjectCreator.FESDepartment.PK;

			Charge testCharge2 = TestObjectCreator.CreateCharge(TestJob, TestObjectCreator.CC5, "Desc", chargeCurrency, 0M, TestObjectCreator.AALSHI, chargeCurrency, 3.44M, TestObjectCreator.ABIGAS);
			testCharge2.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			testCharge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			testCharge2.JR_GE = TestObjectCreator.FESDepartment.PK;

			Factory.Save();

			AssertNotEquals("Precondition: Charge1 OS TestObjectCreator.GST1 Amout should have nonzero value.", 0M, testCharge1.JR_OSSellGSTAmt_Calc);
			AssertNotEquals("Precondition: Charge2 OS TestObjectCreator.GST1 Amout should have nonzero value.", 0M, testCharge2.JR_OSSellGSTAmt_Calc);

			InvoicingPostManager postManager = new InvoicingPostManager(TestJob);
			postManager.CreateTransactions(JobInvoicingPostingOption.Revenue);

			AssertEquals("Precondition: an Invoice should be posted.", 1, postManager.Poster.PostedInvoices.Count);
			InvoicingBase postedInvoice = postManager.Poster.PostedInvoices[0];
			AssertNotEquals("Precondition: Posted Invoice Local TestObjectCreator.GST1 Amout should have nonzero value.", 0M, postedInvoice.AH_LocalTaxAmount);

			testCharge1.ClearRevenueLink();
			testCharge2.ClearRevenueLink();
			Assert("Precondition: Charge1 must NOT be posted.", !testCharge1.IsRevenuePosted);
			Assert("Precondition: Charge2 must NOT be posted.", !testCharge2.IsRevenuePosted);

			AssertEquals("JR_Sell_LocalGSTAmountIncludingTaxAdjustment must be the same as on coresponding posted invoice line.", postedInvoice.Lines[0].AL_LocalGSTAmount, testCharge1.JR_Sell_LocalGSTAmount);
			AssertEquals("JR_Sell_LocalGSTAmountIncludingTaxAdjustment must be the same as on coresponding posted invoice line.", postedInvoice.Lines[1].AL_LocalGSTAmount, testCharge2.JR_Sell_LocalGSTAmount);
			AssertEquals("JR_Sell_LocalGSTAmountIncludingTaxAdjustment must be the same as on posted invoice.", testCharge1.JR_Sell_LocalGSTAmount + testCharge2.JR_Sell_LocalGSTAmount, postedInvoice.AH_LocalTaxAmount);
		}

		public void TestJR_Sell_LocalGSTAmountIncludingTaxAdjustment_ForeignCurrencyChargeAndInvoice_LocalTaxMustBeCalculatedFromOsAmount()
		{
			TestObjectCreator.SetExchangeRate(TestJob, TestObjectCreator.USD, 0.815M);
			TestObjectCreator.GST1.SetRateNumerator_ForTestOnly(10);
			RefCurrency chargeCurrency = TestObjectCreator.USD;
			TestObjectCreator.ABIGAS.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			Charge testCharge1 = TestObjectCreator.CreateCharge(TestJob, TestObjectCreator.CC1, "Desc", chargeCurrency, 0M, TestObjectCreator.AALSHI, chargeCurrency, 2.44M, TestObjectCreator.ABIGAS);
			testCharge1.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			testCharge1.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			testCharge1.JR_GE = TestObjectCreator.FESDepartment.PK;

			Charge testCharge2 = TestObjectCreator.CreateCharge(TestJob, TestObjectCreator.CC5, "Desc", chargeCurrency, 0M, TestObjectCreator.AALSHI, chargeCurrency, 3.44M, TestObjectCreator.ABIGAS);
			testCharge2.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			testCharge2.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			testCharge2.JR_GE = TestObjectCreator.FESDepartment.PK;

			AssertNotEquals("Precondition: Charge1 OS TestObjectCreator.GST1 Amout should have nonzero value.", 0M, testCharge1.JR_OSSellGSTAmt_Calc);
			AssertNotEquals("Precondition: Charge2 OS TestObjectCreator.GST1 Amout should have nonzero value.", 0M, testCharge2.JR_OSSellGSTAmt_Calc);

			InvoicingPostManager postManager = new InvoicingPostManager(TestJob);
			postManager.CreateTransactions(JobInvoicingPostingOption.Revenue);

			AssertEquals("Precondition: an Invoice should be posted.", 1, postManager.Poster.PostedInvoices.Count);
			InvoicingBase postedInvoice = postManager.Poster.PostedInvoices[0];
			AssertNotEquals("Precondition: Posted Invoice Local TestObjectCreator.GST1 Amout should have nonzero value.", 0M, postedInvoice.AH_LocalTaxAmount);

			testCharge1.ClearRevenueLink();
			testCharge2.ClearRevenueLink();
			Assert("Precondition: Charge1 must NOT be posted.", !testCharge1.IsRevenuePosted);
			Assert("Precondition: Charge2 must NOT be posted.", !testCharge2.IsRevenuePosted);

			AssertEquals("JR_Sell_LocalGSTAmountIncludingTaxAdjustment must be the same as on coresponding posted invoice line.", postedInvoice.Lines[0].AL_LocalGSTAmount, testCharge1.JR_Sell_LocalGSTAmount);
			AssertEquals("JR_Sell_LocalGSTAmountIncludingTaxAdjustment must be the same as on coresponding posted invoice line.", postedInvoice.Lines[1].AL_LocalGSTAmount, testCharge2.JR_Sell_LocalGSTAmount);
			AssertEquals("JR_Sell_LocalGSTAmountIncludingTaxAdjustment must be the same as on posted invoice.", testCharge1.JR_Sell_LocalGSTAmount + testCharge2.JR_Sell_LocalGSTAmount, postedInvoice.AH_LocalTaxAmount);
		}

		public void TestTaxCalculationSameBetweenARInvoiceAndShipment()
		{
			var systemSubUnitRatio = GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio;
			var systemIsGSTRegistered = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			try
			{
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = 100;
				TestObjectCreator.SetCurrentCompanyReciprocal(true);
				var rate = TestObjectCreator.CreateTaxRate("SERANDED", "GST and EDU Rate 1", AccTaxRate.Types.Rated, 12, AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax, 3, 1);

				var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("10001"));
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("10001", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
				var line = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "line1", 3985.15m);
				line.AL_AT = rate.PK;

				var jobcharge = TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1, TestObjectCreator.AUD);

				Factory.Save();

				AssertEquals("Precondition: OSTaxAmount", 492.56m, line.AL_OSTaxAmount);
				AssertEquals("Precondition: LocalTaxAmount", 492.56m, line.AL_LocalTaxAmount);

				var testCharge1 = TestObjectCreator.CreateCharge(TestJob, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 3985.15m, TestObjectCreator.ABIGAS);
				testCharge1.JR_AT_SellGSTRate = rate.PK;

				Factory.Save();

				AssertEquals("Invoice Line OSTaxAmount Should be the same as job charge OSSellGSTAmount", line.AL_OSTaxAmount, testCharge1.JR_OSSellGSTAmt_Calc);
				AssertEquals("Invoice Line LocalTaxAmount Should be the same as job charge Sell_LocalGSTAmountIncludingTaxAdjustment", line.AL_LocalTaxAmount, testCharge1.JR_Sell_LocalGSTAmount);
				AssertEquals("Invoice Line LocalTaxAmount Should be the same as job charge LocalSellTaxAmount", line.AL_LocalTaxAmount, ((IReceivablesPostingCharge)testCharge1).LocalSellTaxAmount);
			}
			finally
			{
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = systemSubUnitRatio;
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = systemIsGSTRegistered;
			}
		}

		public void TestTaxCalculationSameBetweenAPInvoiceAndShipment()
		{
			var systemSubUnitRatio = GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio;
			var systemIsGSTRegistered = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			try
			{
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = 100;
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;
				var rate = TestObjectCreator.CreateTaxRate("SERANDED", "GST and EDU Rate 1", AccTaxRate.Types.Rated, 12, AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax, 3, 1);

				var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("10001"));
				APInvoice invoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m);
				var line = TestObjectCreator.CreateAPInvoiceLine(invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "line1", 3985.15m);
				line.AL_AT = rate.PK;

				var jobcharge = TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1, TestObjectCreator.AUD);

				Factory.Save();

				AssertEquals("Precondition: OSTaxAmount", 492.56m, line.AL_OSTaxAmount);
				AssertEquals("Precondition: LocalTaxAmount", 492.56m, line.AL_LocalTaxAmount);

				var testCharge1 = TestObjectCreator.CreateCharge(TestJob, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 3985.15m, TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 0m, TestObjectCreator.ABIGAS);
				testCharge1.JR_AT_CostGSTRate = rate.PK;

				Factory.Save();

				AssertEquals("Invoice Line OSTaxAmount Should be the same as job charge OSSellGSTAmount", line.AL_OSTaxAmount, testCharge1.JR_OSCostGSTAmt_Calc);
				AssertEquals("Invoice Line LocalTaxAmount Should be the same as job charge JR_Cost_LocalGSTAmount", line.AL_LocalTaxAmount, testCharge1.JR_Cost_LocalGSTAmount);
			}
			finally
			{
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = systemSubUnitRatio;
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = systemIsGSTRegistered;
			}
		}

		public void TestDontCreateWIPsAndAccrualsOnBillingExporting()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			GlbCompany otherCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			otherCompany.GC_RN_NKCountryCode = "US"; // this makes the above shipment an import (which is 'IMM' revenue recognition)
			otherCompany.GC_RX_NKLocalCurrency = "USD";
			TestObjectCreator creator = new TestObjectCreator(Factory);
			AccChargeCode chargeCode = creator.CC1;
			// need to setup registry of this company, rather than override on the charge code.
			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting = valuesForTest.AddNew();
			setting.JobType = "SHP";
			setting.DirectionCode = "OTH";
			setting.Mode = Core.Constants.TransportModes.All;
			setting.BrokerCode = "ALL";
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			Factory.Save();

			Job job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Charge charge = job.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_OSCostAmt = 100m;

			shipment.JS_E_ARV = ZDateTime.Now;
			Factory.Save();
			AssertNull("Should be no accrual because of revenue recognition config", charge.Accrual);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			// when being used in the log walker, the 'Factory.Save' is executed in a different company to the job's company, which breaks revenue recognition and other configuration behaviours
			shipment = newFactory.Load<ForwardingShipment>(shipment.PK);
			ChargeCollection charges = ((Job)shipment.Job).Charges;

			using (new TemporaryUserContext() { BranchPK = otherCompany.Branches[0].PK.ToGuid(), DepartmentPK = Env.CurrentDepartment.PK }.Set())
			{
				newFactory.Save();
			}

			AssertNull("Should be no transaction lines for job", Factory.LoadTop1<TransactionLine>(new ZQuery(AccTransactionLinesSchema.AL_JH, job.PK).AddToFilter(AccTransactionLinesSchema.AL_GC, job.JH_GC)));
		}

		public void TestChargeCodePrintSequence()
		{
			Charge c = Factory.New<Charge>();

			AssertEquals("Should return default", (ZShort)0, c.ChargeCodePrintSequence);

			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			c.JR_AC = chargeCode.PK;
			c.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			chargeCode.AC_PrintSequence = 10;
			Factory.Save();

			AssertEquals("Should return ChargeCode.AC_PrintSequence", (ZShort)10, c.ChargeCodePrintSequence);

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			c.JR_OH_SellAccount = org.PK;
			c.JR_InvoiceType = InvoiceTypesList.Codes.FreightInvoice; // just added

			AccClientInvoiceOrder order1 = org.InvoiceOrders.AddNew();
			order1.AI_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching;
			order1.AI_AC = c.JR_AC;
			order1.AI_PrintOrder = 40;

			AccClientInvoiceOrder order2 = org.InvoiceOrders.AddNew();
			order2.AI_InvoiceType = InvoiceTypesList.Codes.FreightInvoice;
			order2.AI_AC = c.JR_AC;
			order2.AI_PrintOrder = 20;

			Factory.Save();

			AssertEquals("Should return order.AI_PrintOrder", (ZShort)20, c.ChargeCodePrintSequence);

			order1.AI_InvoiceType = "ALL";

			Factory.Save();

			AssertEquals("Should return order.AI_PrintOrder", (ZShort)40, c.ChargeCodePrintSequence);

			order1.AI_InvoiceType = string.Empty;

			Factory.Save();

			AssertEquals("Should return order.AI_PrintOrder", (ZShort)40, c.ChargeCodePrintSequence);
		}

		#region Test ICharge Members

		public void TestIsBillInLocalCurrency()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var charge = job.Charges.AddNew();
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FreightInvoice;
			AssertEquals(charge.BillInLocalCurrency, ((ICharge)charge).IsBillInLocalCurrency);
		}

		#endregion

		public void TestIReceivablesPostingChargeMembers()
		{
			var importShipment = TestObjectCreator.CreateShipment("IMPORT_SHP");
			importShipment.JS_RL_NKOrigin = "USLAX";
			importShipment.JS_RL_NKDestination = "AUSYD";
			importShipment.JS_INCO = Constants.IncoTerms.FreeOnBoard;

			using (var job = JobInvoicing.Job.CreateWithMutex(Factory, importShipment))
			{
				var expectedDebtor = TestObjectCreator.LocalClient;
				var expectedAddress = TestObjectCreator.CreateAddress(expectedDebtor);
				var expectedContact = TestObjectCreator.CreateContact(expectedDebtor);

				job.LocalChargesPK = expectedDebtor.PK;
				var charge = job.Charges.AddNew();

				var chargeCode_ODOC = TestObjectCreator.CreateChargeCode("ODC1");
				chargeCode_ODOC.AC_ChargeGroup = "ORG";

				charge.JR_AC = chargeCode_ODOC.PK;
				charge.JR_OH_SellAccount = expectedDebtor.PK;
				charge.JR_OA_SellInvoiceAddress = expectedAddress.PK;
				charge.JR_OC_SellInvoiceContact = expectedContact.PK;

				AssertNotEquals("For the test to be effective these two addresses must be different", expectedDebtor.MainAddress.PK, expectedAddress.PK);

				var castCharge = charge as IReceivablesPostingCharge;
				AssertEquals("Debtor", expectedDebtor.PK, castCharge.Debtor.PK);
				AssertEquals("DebtorAddress PK", expectedAddress.PK, castCharge.DebtorAddressPK);
				AssertEquals("DebtorContact PK", expectedContact.PK, castCharge.DebtorContactPK);

				castCharge.DebtorAddressPK = ZGuid.Empty;
				AssertEquals("DebtorAddress PK", ZGuid.Empty, castCharge.DebtorAddressPK);

				castCharge.DebtorContactPK = ZGuid.Empty;
				AssertEquals("DebtorContact PK", ZGuid.Empty, castCharge.DebtorContactPK);

				var expectedContact2 = TestObjectCreator.CreateContact(expectedDebtor, "Ben");
				job.LocalChargesPK = expectedDebtor.PK;
				job.JH_OA_LocalChargesAddr = expectedDebtor.MainAddress.PK;
				job.JH_OC_LocalBillingContact = expectedContact2.PK;
				charge.JR_OA_SellInvoiceAddress = ZGuid.Empty;
				charge.JR_OC_SellInvoiceContact = ZGuid.Empty;
				castCharge.InitializeSellAddressContact();
				AssertEquals("Address is defaulted because debtor matches local client.", expectedDebtor.MainAddress.PK, charge.JR_OA_SellInvoiceAddress);
				AssertEquals("Contact is defaulted because debtor matches local client.", expectedContact2.PK, charge.JR_OC_SellInvoiceContact);

				charge.JR_OA_SellInvoiceAddress = expectedAddress.PK;
				charge.JR_OC_SellInvoiceContact = expectedContact.PK;
				castCharge.InitializeSellAddressContact();
				AssertEquals("No defaulting occurs due to address being set already.", expectedAddress.PK, charge.JR_OA_SellInvoiceAddress);
				AssertEquals("No defaulting occurs due to contact being set already.", expectedContact.PK, charge.JR_OC_SellInvoiceContact);

				charge.JR_OA_SellInvoiceAddress = ZGuid.Empty;
				charge.JR_OC_SellInvoiceContact = ZGuid.Empty;
				job.LocalChargesPK = ZGuid.Empty;
				job.AgentCollectPK = expectedDebtor.PK;
				job.JH_OA_AgentCollectAddr = expectedAddress.PK;
				castCharge.InitializeSellAddressContact();
				AssertEquals("Address is defaulted because debtor matches overseas agent.", expectedAddress.PK, charge.JR_OA_SellInvoiceAddress);
				AssertEquals("Contact is only defaulted when debtor matches local client. Doesn't default for overseas agent.", ZGuid.Empty, charge.JR_OC_SellInvoiceContact);

				charge.JR_OA_SellInvoiceAddress = expectedDebtor.MainAddress.PK;
				charge.JR_OC_SellInvoiceContact = expectedContact.PK;
				castCharge.InitializeSellAddressContact();
				AssertEquals("No defaulting occurs due to address being set already.", expectedDebtor.MainAddress.PK, charge.JR_OA_SellInvoiceAddress);
				AssertEquals("No defaulting occurs due to contact being set already.", expectedContact.PK, charge.JR_OC_SellInvoiceContact);

				charge.JR_OA_SellInvoiceAddress = ZGuid.Empty;
				charge.JR_OC_SellInvoiceContact = ZGuid.Empty;
				job.LocalChargesPK = TestObjectCreator.LocalClient.PK;
				job.AgentCollectPK = TestObjectCreator.Agent.PK;
				charge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
				castCharge.InitializeSellAddressContact();
				AssertEquals("Defaulting occurs because the address was empty", TestObjectCreator.Debtor.AddressForSendingARDocuments.PK, charge.JR_OA_SellInvoiceAddress);
				AssertEquals("No defaulting occurs due to debtor not matching local client.", ZGuid.Empty, charge.JR_OC_SellInvoiceContact);

				//TaxRatePostingGroupId
				charge.JR_AT_SellGSTRate = ZGuid.Empty;
				AssertNull(null, charge.SellGSTRate);
				AssertEquals(AccTaxRate.DefaultPostingGroupID, castCharge.TaxRatePostingGroupId);

				var tax1 = Factory.NewWithValidTestData<AccTaxRate>();
				tax1.AT_PostingGroupId = 9;

				charge.JR_AT_SellGSTRate = tax1.PK;
				AssertNotNull(null, charge.SellGSTRate);
				AssertEquals(new ZShort(9), castCharge.TaxRatePostingGroupId);

				charge.JR_SellPlaceOfSupply = ZString.Empty;
				AssertEquals("SellPlaceOfSupply", ZString.Empty, castCharge.SellPlaceOfSupply);

				charge.JR_SellPlaceOfSupply = "NSW";
				AssertEquals("SellPlaceOfSupply", "NSW", castCharge.SellPlaceOfSupply);
			}
		}

		public void TestDeleteForDataRefreshOnConcurrencyErrorShouldDeleteUnlinkedACRWIP()
		{
			var testChargeCode = TestObjectCreator.CreateChargeCode("CC9");
			var chargeRevRecOverride = testChargeCode.RevenueRecOverrides.AddNew();

			chargeRevRecOverride.JobType = "SHP";
			chargeRevRecOverride.DirectionCode = "ALL";
			chargeRevRecOverride.Mode = "ALL";
			chargeRevRecOverride.RecognitionDateOptionCode = "PIC";

			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S0099");
			var job = TestObjectCreator.CreateJob(shipment);

			var charge1 = TestObjectCreator.CreateCharge(job, testChargeCode, 1, 1);
			var charge2 = TestObjectCreator.CreateCharge(job, testChargeCode, 2, 2);
			var charge3 = TestObjectCreator.CreateCharge(job, testChargeCode, 3, 4);

			Factory.Save();

			AssertEquals(job.Charges.Count, 3);
			AssertEquals(charge1.CostRecognition, "PIC");
			AssertEquals(charge1.IsCostRecognized, false);
			AssertNull(charge1.Accrual);

			var userB = new BusinessObjectFactory();
			userB.RefreshEnabled = false;
			var chargeCodeInOtherFactory = userB.Load<AccChargeCode>(testChargeCode.PK);
			chargeCodeInOtherFactory.RevenueRecOverrides.RemoveAndDeleteAll();
			userB.Save();

			var userVictim = new BusinessObjectFactory();
			userVictim.RefreshEnabled = false;
			var userVictimCharge1 = userVictim.Load<Charge>(charge1.PK);
			AssertEquals(userVictimCharge1.CostRecognition, "IMM");
			AssertEquals(userVictimCharge1.IsCostRecognized, false);
			AssertNull(userVictimCharge1.Accrual);
			userVictimCharge1.JR_LocalCostAmt = 99;

			var userC = new BusinessObjectFactory();
			userC.RefreshEnabled = false;
			var charge1InOtherFactory = userC.Load<Charge>(charge1.PK);
			charge1InOtherFactory.Delete();
			userC.Save();

			var msgBoxMessage = string.Empty;
			Accrual unlinkedACR = null;
			WIP unlinkedWIP = null;
			var handler = new Customs.Business.Testing.NotificationHandlerForTest();
			try
			{
				msgBoxMessage = string.Empty;
				userVictim.Save();
			}
			catch (ZSaveConcurrencyException ex)
			{
				AssertEquals(userVictimCharge1.IsCostRecognized, true);
				unlinkedACR = userVictimCharge1.Accrual;
				unlinkedWIP = userVictimCharge1.WIP;
				AssertNotNull(unlinkedACR);
				AssertNotNull(unlinkedWIP);
				Assert(!unlinkedACR.IsDeleted);
				Assert(!unlinkedWIP.IsDeleted);

				ZExceptionReporting.HandleZSaveConcurrencyException(ex, handler, true);
				msgBoxMessage = handler.ReportInformationMessage;

				Assert(userVictimCharge1.IsDeleted);
				Assert(userVictimCharge1.JR_AL_APLine.IsEmpty);
				Assert(userVictimCharge1.JR_AL_ARLine.IsEmpty);
				AssertNull(userVictimCharge1.Accrual);
				AssertNull(userVictimCharge1.WIP);
			}

			AssertContains(
@"While you have been working with this form, another user has made changes.

The system will now try to combine your changes with those of the other user.
After you click 'OK', the form will merge your changes with changes made by other user.

However, the fields will have warning messages explaining the other user's changes.
Please review the form carefully before clicking the 'Save' button again.

The following objects have been deleted:
JobCharge", msgBoxMessage);

			Assert(userVictimCharge1.IsDeleted);

			//the unlinked the ACR/WIP should also be deleted.
			//critical validation would be triggered, if not deleted.
			Assert(unlinkedACR.IsDeleted);
			Assert(unlinkedWIP.IsDeleted);

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestShouldDeleteChargeSuccessfullyWithConcurrencyMerge()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001234");
			var job = TestObjectCreator.CreateJob(shipment);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100, 100);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var shipmentLoadByOtherUser = newFactory.Load<ForwardingShipment>(shipment.PK);
			var chargeToBeDelete = ((Job)shipmentLoadByOtherUser.Job).Charges[0];

			charge.JR_AgentDeclaredCostAmt = 33m;
			Assert("AgentDeclaredCostAmt should be changed", charge.JR_AgentDeclaredCostAmtInfo.HasChanges);
			Factory.Save();

			Assert("Accrual shoud not be reversed", !chargeToBeDelete.Accrual.IsReversed);
			Assert("WIP shoud not be reversed", !chargeToBeDelete.WIP.IsReversed);
			var chargeToBeDeletedPK = chargeToBeDelete.PK;
			var relatedAccrualPK = chargeToBeDelete.Accrual.PK;
			var relatedWIPPK = chargeToBeDelete.WIP.PK;
			var ex = AssertExceptionThrown<ZSaveConcurrencyException>("Should throw concurrency exception.", () => { chargeToBeDelete.Delete(); newFactory.Save(); });
			ZExceptionReporting.HandleSaveException(ex);
			AssertNoExceptionThrown("Should save successfully.", () => { newFactory.Save(); });

			var reLoadFactory = new BusinessObjectFactory();
			AssertNull(reLoadFactory.Load<Charge>(chargeToBeDeletedPK));
			Assert(reLoadFactory.Load<Accrual>(relatedAccrualPK).IsReversed);
			Assert(reLoadFactory.Load<WIP>(relatedWIPPK).IsReversed);
		}

		public void TestEnterpriseBusinessObjectIsAudited()
		{
			var columns = new SchemaColumn[]
			{
				JobChargeSchema.PK,
				JobChargeSchema.JR_Desc
			};
			AuditLogsHelperForTesting.AssertColumnsExistsInAuditDb(Factory, JobChargeSchema.PK.TableSchema.SqlSchemaName, JobChargeSchema.PK.TableName, columns);
		}

		public void TestPeriodicInvoiceConfigurationShouldPickUpPeriodicConfiguration()
		{
			//CREATE SHIPMENT WITH SPECIFIC TRANSPORTMODE
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S1234");
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RS_NKServiceLevel = "STD";

			//CREATE JOB AND JOBCHARGE
			Job job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "CC1", TestObjectCreator.AUD, 1000, TestObjectCreator.Creditor1, "1234", TestObjectCreator.AUD, 1000, TestObjectCreator.ABIGAS);

			//SET UP INVOICETYPE FOR JOBCHARGE WITH DEFAULT VALUE OF "FIN"
			charge.JR_InvoiceType = "FIN";
			AssertEquals("InvoiceType is FIN", "FIN", charge.JR_InvoiceType);

			//CREATING PERIODIC INVOICING CONFIGURATION FOR NEW COMPANY
			var orgHeader = TestObjectCreator.AALSHI;
			var invoiceType = orgHeader.CompanyData.InvoiceTypes.AddNew();
			invoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			invoiceType.PI_ServiceDirection = "ALL";
			invoiceType.PI_TransportMode = "ALL";
			invoiceType.PI_RS_NKServiceLevel = "STD";
			invoiceType.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			invoiceType.PI_Calc_IsInclude = InvoiceTypeChargeInclusionTypeList.Codes.ALL;

			//RESET DEBTOR OF THE JOBCHARGE TO TRIGGER INVOICE TYPE MATCHING
			var newDebtor = TestObjectCreator.AALSHI;
			charge.JR_OH_SellAccount = newDebtor.PK;

			Factory.ClearQueryCache();
			Factory.Save();

			//THE SYSTEM SHOULD PICK UP GENERIC "ALL" PERIODIC INVOICE CONFIGURATION
			AssertEquals("For Shipment with TransportMode 'AIR' should have picked up the TransportMode 'ALL' Configuration setting the InvoiceType to FID (Final Periodic Invoice)", "FID", charge.JR_InvoiceType);
		}

		#region Rating Behaviors

		public void TestRatingBehaviors()
		{
			var newChargeCode = Factory.New<AccChargeCode>();
			newChargeCode.AC_IsActive = false;
			newChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			newChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;

			var testCharge = Factory.New<Charge>();

			AssertEquals("Cost Rating Behavior", JobChargeLookups.ReAutorateCharge, testCharge.JR_Calc_CostRatingBehavior);
			AssertEquals("Sell Rating Behavior", JobChargeLookups.ReAutorateCharge, testCharge.JR_Calc_SellRatingBehavior);

			AssertEquals("Cost Rating Behavior", JobChargeLookups.ReAutorateCharge, testCharge.JR_Calc_CostRatingBehavior);
			AssertEquals("CostRatingOverride is checked", false, testCharge.JR_CostRatingOverride);

			testCharge.JR_Calc_CostRatingBehavior = JobChargeLookups.CreateNewCharge;

			AssertEquals("CostRatingOverride is not checked", true, testCharge.JR_CostRatingOverride);
			AssertEquals("SellRatingOverride is checked", false, testCharge.JR_SellRatingOverride);

			testCharge.JR_Calc_CostRatingBehavior = JobChargeLookups.ReAutorateCharge;

			AssertEquals("CostRatingOverride is checked", false, testCharge.JR_CostRatingOverride);
			AssertEquals("SellRatingOverride is checked", false, testCharge.JR_SellRatingOverride);

			testCharge.JR_Calc_SellRatingBehavior = JobChargeLookups.CreateNewCharge;

			AssertEquals("CostRatingOverride is checked", false, testCharge.JR_CostRatingOverride);
			AssertEquals("SellRatingOverride is not checked", true, testCharge.JR_SellRatingOverride);

			testCharge.JR_Calc_SellRatingBehavior = JobChargeLookups.ReAutorateCharge;

			AssertEquals("CostRatingOverride is checked", false, testCharge.JR_CostRatingOverride);
			AssertEquals("SellRatingOverride is checked", false, testCharge.JR_SellRatingOverride);

			testCharge.JR_CostRatingOverride = true;
			testCharge.JR_SellRatingOverride = true;

			AssertEquals("SellRatingBehavior is wrong", JobChargeLookups.CreateNewCharge, testCharge.JR_Calc_SellRatingBehavior);
			AssertEquals("CostRatingBehavior is wrong", JobChargeLookups.CreateNewCharge, testCharge.JR_Calc_CostRatingBehavior);

			testCharge.JR_CostRatingOverride = false;
			testCharge.JR_SellRatingOverride = false;

			var tr = Factory.NewWithValidTestData<AccTaxRate>();
			var thCost = Factory.NewWithValidTestData<AccTransactionHeader>();
			var tlCost = Factory.NewWithValidTestData<AccTransactionLines>();
			tlCost.AL_AH = thCost.PK;
			tlCost.AL_LineType = TransactionLineTypes.Cost;
			tlCost.AL_LineAmount = -200;
			tlCost.AL_OSAmount = -200;
			tlCost.AL_RX_NKTransactionCurrency = "AUD";
			tlCost.AL_RevRecognitionType = "IMM";

			testCharge.JR_AL_APLine = tlCost.PK;
			testCharge.JR_AT_SellGSTRate = tr.PK;

			Assert(testCharge.JR_IsCostPosted);

			var thSell = Factory.NewWithValidTestData<AccTransactionHeader>();
			var tlSell = Factory.NewWithValidTestData<AccTransactionLines>();
			tlSell.AL_AH = thSell.PK;
			tlSell.AL_LineType = TransactionLineTypes.Revenue;
			tlSell.AL_LineAmount = 200;
			tlSell.AL_OSAmount = 200;
			tlSell.AL_RX_NKTransactionCurrency = "AUD";
			tlSell.AL_RevRecognitionType = "IMM";

			testCharge.JR_AL_ARLine = tlSell.PK;

			Assert(testCharge.JR_IsRevenuePosted);

			AssertEquals("CostRatingBehavior is wrong", JobChargeLookups.StopFromAutorating, testCharge.JR_Calc_CostRatingBehavior);
			AssertEquals("SellRatingBehavior is wrong", JobChargeLookups.StopFromAutorating, testCharge.JR_Calc_SellRatingBehavior);

			testCharge.JR_CostRatingOverride = true;
			testCharge.JR_SellRatingOverride = true;

			AssertEquals("SellRatingBehavior is wrong", JobChargeLookups.CreateNewCharge, testCharge.JR_Calc_SellRatingBehavior);
			AssertEquals("CostRatingBehavior is wrong", JobChargeLookups.CreateNewCharge, testCharge.JR_Calc_CostRatingBehavior);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestRatingBehaviors_GroupCompanySellCharges()
		{
			var shipment = TestObjectCreator.CreateShipment("SHIP100216");
			var creditorCompany = TestObjectCreator.CreateCompanyAndBranch("CHZRH");
			var debtorCompany = TestObjectCreator.CreateCompanyAndBranch("AUMEL");
			var fisDepartment = TestObjectCreator.FISDepartment;
			var globalChargeCode = TestObjectCreator.CreateGlobalChargeCode("GLBCHG");
			Factory.Save();

			using (TestObjectCreator.SwitchEnvToCompany(creditorCompany, department: fisDepartment))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly())
			{
				var localChargeCode = globalChargeCode.ChildChargeCodes.Single(c => c.AC_GC == Env.CurrentCompanyPK);
				TestObjectCreator.CreateChargeWithPaymentBasis(job, localChargeCode, debtorCompany.OrgProxy, 20m);
				Factory.Save();
			}

			using (TestObjectCreator.SwitchEnvToCompany(debtorCompany, department: fisDepartment))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly())
			{
				job.JH_GE = TestObjectCreator.FISDepartment.PK;
				AssertEquals("Pre-condition", 0, job.Charges.Count);

				var groupCompanyChargesForJob = new GroupCompanyChargesForJob(job);
				var foundGroupCompanySellCharge = groupCompanyChargesForJob.ChargesForDebtor.Cast<GroupCompanyCharge>().Single();
				AssertNotNull("Pre-condition: should find a group company charge", foundGroupCompanySellCharge);
				AssertEquals("Pre-condition: this group company sell charge should create a cost charge", "Create", foundGroupCompanySellCharge.AcceptActionDescription);

				job.GroupCompanyChargesForDebtor.AcceptSellChargesAsCosts(new[] { foundGroupCompanySellCharge });

				var groupCompanyCostCharge = job.Charges.Cast<Charge>().Single();
				AssertNotNull("Accepting group company charge should have created local charge", globalChargeCode);
				AssertEquals(globalChargeCode.AC_Code, groupCompanyCostCharge.ChargeCode.AC_Code);

				AssertEquals("Should not be able to autorate an accepted cost charge", JobChargeLookups.StopFromAutorating, groupCompanyCostCharge.JR_Calc_CostRatingBehavior);
				AssertEquals("Expected default", JobChargeLookups.ReAutorateCharge, groupCompanyCostCharge.JR_Calc_SellRatingBehavior);
			}

			AssertNoExceptionThrown("", () => { Factory.Save(); });
		}

		public void TestInvalidateGroupChargeMatch()
		{
			var shipment = TestObjectCreator.CreateShipment("SHIP100216");
			var creditorCompany = TestObjectCreator.CreateCompanyAndBranch("CHZRH");
			var debtorCompany = TestObjectCreator.CreateCompanyAndBranch("AUMEL");
			var fisDepartment = TestObjectCreator.FISDepartment;
			var globalChargeCode1 = TestObjectCreator.CreateGlobalChargeCode("GLBCHG1");
			Factory.Save();
			using (TestObjectCreator.SwitchEnvToCompany(creditorCompany, department: fisDepartment))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				var chargeCode1 = globalChargeCode1.ChildChargeCodes.Single(c => c.AC_GC == Env.CurrentCompanyPK);

				var creditorSellCharge1 = TestObjectCreator.CreateChargeWithPaymentBasis(job, chargeCode1, debtorCompany.OrgProxy, 20m);
				AssertEquals("PRE", false, creditorSellCharge1.JR_Calc_HasDebtorAcceptedThisSellCharge);
				AssertEquals("PRE", false, creditorSellCharge1.JR_Calc_CostRatingBehavior_ReadOnly);

				AssertEquals(true, creditorSellCharge1.IsGroupChargeSellCalculated_ForTestOnly);
				AssertEquals(true, creditorSellCharge1.IsGroupChargeCostCalculated_ForTestOnly);

				creditorSellCharge1.InvalidateGroupChargeMatch(true);
				AssertEquals("invalidating the creditor (sell charge) resets calculated sell state", false, creditorSellCharge1.IsGroupChargeSellCalculated_ForTestOnly);
				AssertEquals("invalidating the creditor (sell charge) does not rest the calculated cost state", true, creditorSellCharge1.IsGroupChargeCostCalculated_ForTestOnly);

				AssertEquals("PRE", false, creditorSellCharge1.JR_Calc_HasDebtorAcceptedThisSellCharge);
				AssertEquals("PRE", false, creditorSellCharge1.JR_Calc_CostRatingBehavior_ReadOnly);
				AssertEquals(true, creditorSellCharge1.IsGroupChargeSellCalculated_ForTestOnly);
				AssertEquals(true, creditorSellCharge1.IsGroupChargeCostCalculated_ForTestOnly);
				creditorSellCharge1.InvalidateGroupChargeMatch(false);
				AssertEquals("invalidating the debtor (cost charge) does not reset calculated sell state", true, creditorSellCharge1.IsGroupChargeSellCalculated_ForTestOnly);
				AssertEquals("invalidating the debtor (cost charge) does reset the calculated cost state", false, creditorSellCharge1.IsGroupChargeCostCalculated_ForTestOnly);
			}
		}

		public void TestJR_Calc_HasDebtorAcceptedThisSellCharge()
		{
			var shipment = TestObjectCreator.CreateShipment("SHIP100216");
			var creditorCompany = TestObjectCreator.CreateCompanyAndBranch("CHZRH");
			var debtorCompany = TestObjectCreator.CreateCompanyAndBranch("AUMEL");
			var fisDepartment = TestObjectCreator.FISDepartment;
			var globalChargeCode1 = TestObjectCreator.CreateGlobalChargeCode("GLBCHG1");
			var globalChargeCode2 = TestObjectCreator.CreateGlobalChargeCode("GLBCHG2");
			var globalChargeCode3 = TestObjectCreator.CreateGlobalChargeCode("GLBCHG3");
			Factory.Save();

			ZGuid creditorSellCharge1PK;
			ZGuid creditorSellCharge2PK;
			ZGuid creditorSellCharge3PK;

			using (TestObjectCreator.SwitchEnvToCompany(creditorCompany, department: fisDepartment))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				var chargeCode1 = globalChargeCode1.ChildChargeCodes.Single(c => c.AC_GC == Env.CurrentCompanyPK);
				var chargeCode2 = globalChargeCode2.ChildChargeCodes.Single(c => c.AC_GC == Env.CurrentCompanyPK);
				var chargeCode3 = globalChargeCode3.ChildChargeCodes.Single(c => c.AC_GC == Env.CurrentCompanyPK);

				var creditorSellCharge1 = TestObjectCreator.CreateChargeWithPaymentBasis(job, chargeCode1, debtorCompany.OrgProxy, 20m);
				var creditorSellCharge2 = TestObjectCreator.CreateChargeWithPaymentBasis(job, chargeCode2, debtorCompany.OrgProxy, 40m);
				var creditorSellCharge3 = TestObjectCreator.CreateChargeWithPaymentBasis(job, chargeCode3, debtorCompany.OrgProxy, 50m);

				creditorSellCharge1PK = creditorSellCharge1.PK;
				creditorSellCharge2PK = creditorSellCharge2.PK;
				creditorSellCharge3PK = creditorSellCharge3.PK;

				AssertEquals("Pre-condition", false, creditorSellCharge1.JR_Calc_HasDebtorAcceptedThisSellCharge);
				AssertEquals("Pre-condition", false, creditorSellCharge2.JR_Calc_HasDebtorAcceptedThisSellCharge);
				AssertEquals("Pre-condition", false, creditorSellCharge3.JR_Calc_HasDebtorAcceptedThisSellCharge);
				Factory.Save();
			}

			using (TestObjectCreator.SwitchEnvToCompany(debtorCompany, department: fisDepartment))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				job.JH_GE = TestObjectCreator.FISDepartment.PK;

				var debtorGroupCompanyCharges = new GroupCompanyChargesForJob(job)
					.ChargesForDebtor
					.Cast<GroupCompanyCharge>()
					.ToArray();

				AssertEquals("Should have found all three charges", 3, debtorGroupCompanyCharges.Length);

				job.GroupCompanyChargesForDebtor.AcceptSellChargesAsCosts(debtorGroupCompanyCharges);

				var debtorCostCharges = job.Charges.Cast<Charge>().ToArray();

				var acceptedCharge1 = debtorCostCharges.Single(x => x.ChargeCode.AC_Code == "GLBCHG1");
				var acceptedCharge2 = debtorCostCharges.Single(x => x.ChargeCode.AC_Code == "GLBCHG2");
				var acceptedCharge3 = debtorCostCharges.Single(x => x.ChargeCode.AC_Code == "GLBCHG3");

				AssertEquals("Charge should exist and credit the creditor OrgProxy", creditorCompany.GC_OH_OrgProxy, acceptedCharge1.JR_OH_CostAccount);
				AssertEquals("Charge should exist and credit the creditor OrgProxy", creditorCompany.GC_OH_OrgProxy, acceptedCharge2.JR_OH_CostAccount);
				AssertEquals("Charge should exist and credit the creditor OrgProxy", creditorCompany.GC_OH_OrgProxy, acceptedCharge3.JR_OH_CostAccount);

				acceptedCharge2.JR_OH_CostAccount = ZGuid.Empty;
				acceptedCharge3.Delete();
				Factory.Save();
			}

			var newFactory = new BusinessObjectFactory(); //clear cache of group company charges
			using (TestObjectCreator.SwitchEnvToCompany(creditorCompany, department: fisDepartment))
			using (var job = new Job.Loader(newFactory, shipment).Load())
			{
				var charges = job.Charges.Cast<Charge>().ToArray();
				var creditorSellCharge1 = charges.Single(x => x.PK == creditorSellCharge1PK);
				var creditorSellCharge2 = charges.Single(x => x.PK == creditorSellCharge2PK);
				var creditorSellCharge3 = charges.Single(x => x.PK == creditorSellCharge3PK);

				AssertEquals(true, creditorSellCharge1.JR_Calc_HasDebtorAcceptedThisSellCharge);
				AssertEquals("Charge was accepted but creditor was changed so is no longer group company charge", false, creditorSellCharge2.JR_Calc_HasDebtorAcceptedThisSellCharge);
				AssertEquals("Charge was deleted so was not accepted", false, creditorSellCharge3.JR_Calc_HasDebtorAcceptedThisSellCharge);
			}
		}

		#endregion

		public void TestPeriodicInvoiceConfigurationShouldNotPickUpPeriodicConfigurationWhenModesDoesNotMatch()
		{
			//CREATE SHIPMENT WITH SPECIFIC TRANSPORTMODE
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S1234");
			shipment.JS_TransportMode = "AIR";

			//CREATE JOB AND JOBCHARGE
			Job job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "CC1", TestObjectCreator.AUD, 1000, TestObjectCreator.Creditor1, "1234", TestObjectCreator.AUD, 1000, TestObjectCreator.ABIGAS);

			//SET UP INVOICETYPE FOR JOBCHARGE WITH DEFAULT VALUE OF "FIN"
			charge.JR_InvoiceType = "FIN";
			AssertEquals("InvoiceType is FIN", "FIN", charge.JR_InvoiceType);

			//CREATING PERIODIC INVOICING CONFIGURATION FOR NEW COMPANY
			var orgHeader = TestObjectCreator.AALSHI;
			var invoiceType = orgHeader.CompanyData.InvoiceTypes.AddNew();
			invoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			invoiceType.PI_ServiceDirection = "ALL";
			invoiceType.PI_TransportMode = "SEA";
			invoiceType.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			invoiceType.PI_Calc_IsInclude = InvoiceTypeChargeInclusionTypeList.Codes.ALL;

			//RESET DEBTOR OF THE JOBCHARGE TO TRIGGER INVOICE TYPE MATCHING
			var newDebtor = TestObjectCreator.AALSHI;
			charge.JR_OH_SellAccount = newDebtor.PK;

			Factory.ClearQueryCache();
			Factory.Save();

			//THE SYSTEM SHOULD NOT PICK UP PERIODIC INVOICE CONFIGURATION, THE VALUE SHOULD REMAIN THE SAME
			AssertEquals("For Shipment with TransportMode 'AIR' should NOT have picked up the TransportMode 'SEA' Configuration leaving the InvoiceType FIN (Final Invoice)", "FIN", charge.JR_InvoiceType);
		}

		public void TestPeriodicInvoiceConfigurationShouldNotPickUpSpecificPeriodicConfigurationForGenericSetup()
		{
			//CREATE SHIPMENT WITH GENERIC TRANSPORTMODE
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S1234");
			shipment.JS_TransportMode = "";

			//CREATE JOB AND JOBCHARGE
			Job job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "CC1", TestObjectCreator.AUD, 1000, TestObjectCreator.Creditor1, "1234", TestObjectCreator.AUD, 1000, TestObjectCreator.ABIGAS);

			//SET UP INVOICETYPE FOR JOBCHARGE WITH DEFAULT VALUE OF "FIN"
			charge.JR_InvoiceType = "FIN";
			AssertEquals("InvoiceType is FIN", "FIN", charge.JR_InvoiceType);

			//CREATING PERIODIC INVOICING CONFIGURATION FOR NEW COMPANY
			var orgHeader = TestObjectCreator.AALSHI;
			var invoiceType = orgHeader.CompanyData.InvoiceTypes.AddNew();
			invoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			invoiceType.PI_ServiceDirection = "ALL";
			invoiceType.PI_TransportMode = "AIR";
			invoiceType.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			invoiceType.PI_Calc_IsInclude = InvoiceTypeChargeInclusionTypeList.Codes.ALL;

			//RESET DEBTOR OF THE JOBCHARGE TO TRIGGER INVOICE TYPE MATCHING
			var newDebtor = TestObjectCreator.AALSHI;
			charge.JR_OH_SellAccount = newDebtor.PK;

			Factory.ClearQueryCache();
			Factory.Save();

			//THE SYSTEM SHOULD NOT PICK UP PERIODIC INVOICE CONFIGURATION, THE VALUE SHOULD REMAIN THE SAME
			AssertEquals("For Shipment with TransportMode empty ('ALL') should NOT have picked up the TransportMode 'AIR' Configuration leaving the InvoiceType FIN (Final Invoice)", "FIN", charge.JR_InvoiceType);
		}

		public void TestPeriodicInvoiceConfigurationShouldPickUpGenericPeriodicConfigurationForGenericSetup()
		{
			//CREATE SHIPMENT WITH GENERIC TRANSPORTMODE
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S1234");
			shipment.JS_TransportMode = "";
			shipment.JS_RS_NKServiceLevel = "STD";

			//CREATE JOB AND JOBCHARGE
			Job job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "CC1", TestObjectCreator.AUD, 1000, TestObjectCreator.Creditor1, "1234", TestObjectCreator.AUD, 1000, TestObjectCreator.ABIGAS);

			//SET UP INVOICETYPE FOR JOBCHARGE WITH DEFAULT VALUE OF "FIN"
			charge.JR_InvoiceType = "FIN";
			AssertEquals("InvoiceType is FIN", "FIN", charge.JR_InvoiceType);

			//CREATING PERIODIC INVOICING CONFIGURATION FOR NEW COMPANY
			var orgHeader = TestObjectCreator.AALSHI;
			var invoiceType = orgHeader.CompanyData.InvoiceTypes.AddNew();
			invoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			invoiceType.PI_ServiceDirection = "ALL";
			invoiceType.PI_TransportMode = "ALL";
			invoiceType.PI_RS_NKServiceLevel = "STD";
			invoiceType.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			invoiceType.PI_Calc_IsInclude = InvoiceTypeChargeInclusionTypeList.Codes.ALL;

			//RESET DEBTOR OF THE JOBCHARGE TO TRIGGER INVOICE TYPE MATCHING
			var newDebtor = TestObjectCreator.AALSHI;
			charge.JR_OH_SellAccount = newDebtor.PK;

			Factory.ClearQueryCache();
			Factory.Save();

			//THE SYSTEM SHOULD NOT PICK UP PERIODIC INVOICE CONFIGURATION, THE VALUE SHOULD REMAIN THE SAME
			AssertEquals("For Shipment with TransportMode empty ('ALL') should have picked up the TransportMode 'ALL' Configuration setting the InvoiceType to FID (Final Periodic Invoice)", "FID", charge.JR_InvoiceType);
		}

		#region Performance Testing

		public void TestJR_OSSellAmtDoesNotRecalculteJR_EstimatedRevenueIfNewValueIsSameAsCurrentValue()
		{
			var charge = CreateCharge();

			AssertEquals(ZDecimal.Zero, charge.JR_EstimatedRevenue);
			charge.JR_OSSellAmt = 10m;
			AssertEquals("JR_EstimatedRevenue should be calculated", 10m, charge.JR_EstimatedRevenue);

			charge.JR_EstimatedRevenue = ZDecimal.Zero;
			charge.JR_OSSellAmt = 10m;
			AssertEquals("JR_EstimatedRevenue should not be calculated because current JR_OSSellAmt is same as new value",
				ZDecimal.Zero, charge.JR_EstimatedRevenue);

			charge.JR_OSSellAmt = 20m;
			AssertEquals("JR_EstimatedRevenue should be calculated because now we are setting a different value",
				20m, charge.JR_EstimatedRevenue);
		}

		public void TestJR_AT_CostGSTRateDoesNotRunRowValidationIfNewValueIsSameAsCurrentValue()
		{
			var charge = CreateCharge();
			UpdateChargeLocalAndOsCostAmtsToProduceRowValidationError(charge);

			Assert(!charge.HasRowErrors);
			charge.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;
			AssertHasRowError("JR_AT_CostGSTRate should run row validation", charge, rowErrorMessage);

			charge.RemoveRowError(rowErrorMessage);
			Assert(!charge.HasRowErrors);
			charge.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;
			AssertNoRowErrors(@"JR_AT_CostGSTRate should not run row validation because current JR_AT_CostGSTRate
					is same	as new value", charge);

			charge.JR_AT_CostGSTRate = TaxRate2.PK;
			AssertHasRowError("JR_AT_CostGSTRate should run row validation because now we are setting a different value",
				charge, rowErrorMessage);
		}

		public void TestJR_AT_SellGSTRateDoesNotRunRowValidationIfNewValueIsSameAsCurrentValue()
		{
			var charge = CreateCharge();
			UpdateChargeLocalAndOsCostAmtsToProduceRowValidationError(charge);

			Assert(!charge.HasRowErrors);
			charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			AssertHasRowError("JR_AT_SellGSTRate should run row validation", charge, rowErrorMessage);

			charge.RemoveRowError(rowErrorMessage);
			Assert(!charge.HasRowErrors);
			charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			AssertNoRowErrors(@"JR_AT_SellGSTRate should not run row validation because current JR_AT_SellGSTRate
					is same as new value", charge);

			charge.JR_AT_SellGSTRate = TaxRate2.PK;
			AssertHasRowError("JR_AT_SellGSTRate should run row validation because now we are setting a different value",
				charge, rowErrorMessage);
		}

		public void TestJR_AW_CostWHTRateDoesNotRunRowValidationIfNewValueIsSameAsCurrentValue()
		{
			var charge = CreateCharge();
			UpdateChargeLocalAndOsCostAmtsToProduceRowValidationError(charge);

			Assert(!charge.HasRowErrors);
			charge.JR_AW_CostWHTRate = TestObjectCreator.WHT1.PK;
			AssertHasRowError("JR_AW_CostWHTRate should run row validation", charge, rowErrorMessage);

			charge.RemoveRowError(rowErrorMessage);
			Assert(!charge.HasRowErrors);
			charge.JR_AW_CostWHTRate = TestObjectCreator.WHT1.PK;
			AssertNoRowErrors(@"JR_AW_CostWHTRate should not run row validation because current JR_AW_CostWHTRate
					is same as new value", charge);

			charge.JR_AW_CostWHTRate = WHTRate2.PK;
			AssertHasRowError("JR_AW_CostWHTRate should run row validation because now we are setting a different value",
				charge, rowErrorMessage);
		}

		public void TestJR_AW_SellWHTRateDoesNotRunRowValidationIfNewValueIsSameAsCurrentValue()
		{
			var charge = CreateCharge();
			UpdateChargeLocalAndOsCostAmtsToProduceRowValidationError(charge);

			Assert(!charge.HasRowErrors);
			charge.JR_AW_SellWHTRate = TestObjectCreator.WHT1.PK;
			AssertHasRowError("JR_AW_SellWHTRate should run row validation", charge, rowErrorMessage);

			charge.RemoveRowError(rowErrorMessage);
			Assert(!charge.HasRowErrors);
			charge.JR_AW_SellWHTRate = TestObjectCreator.WHT1.PK;
			AssertNoRowErrors(@"JR_AW_SellWHTRate should not run row validation because current JR_AW_SellWHTRate
					is same as new value", charge);

			charge.JR_AW_SellWHTRate = WHTRate2.PK;
			AssertHasRowError("JR_AW_SellWHTRate should run row validation because now we are setting a different value",
				charge, rowErrorMessage);
		}

		public void TestJR_LineCFXDoesNotRunRowValidationIfNewValueIsSameAsCurrentValue()
		{
			var charge = CreateCharge();
			UpdateChargeLocalAndOsCostAmtsToProduceRowValidationError(charge);

			Assert(!charge.HasRowErrors);
			charge.JR_LineCFX = 2m;
			AssertHasRowError("JR_LineCFX should run row validation", charge, rowErrorMessage);

			charge.RemoveRowError(rowErrorMessage);
			Assert(!charge.HasRowErrors);
			charge.JR_LineCFX = 2m;
			AssertNoRowErrors("JR_LineCFX should not run row validation because current JR_LineCFX is same as new value",
				charge);

			charge.JR_LineCFX = 3m;
			AssertHasRowError("JR_LineCFX should run row validation because now we are setting a different value",
				charge, rowErrorMessage);
		}

		public void TestJR_OSSellExRateDoesNotRunRowValidationIfNewValueIsSameAsCurrentValue()
		{
			var charge = CreateCharge();
			UpdateChargeLocalAndOsCostAmtsToProduceRowValidationError(charge);

			Assert(!charge.HasRowErrors);
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_OSSellExRate = 2m;
			AssertHasRowError("JR_OSSellExRate should run row validation", charge, rowErrorMessage);

			charge.RemoveRowError(rowErrorMessage);
			Assert(!charge.HasRowErrors);
			charge.JR_OSSellExRate = 2m;
			AssertNoRowErrors(@"JR_OSSellExRate should not run row validation because current JR_OSSellExRate
					is same as new value", charge);

			charge.JR_OSSellExRate = 3m;
			AssertHasRowError("JR_OSSellExRate should run row validation because now we are setting a different value",
				charge, rowErrorMessage);
		}

		public void TestJR_RX_NKSellCurrencyDoesNotRunRowValidationIfNewValueIsSameAsCurrentValue()
		{
			var charge = CreateCharge();
			UpdateChargeLocalAndOsCostAmtsToProduceRowValidationError(charge);

			Assert(!charge.HasRowErrors);
			charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			AssertHasRowError("JR_RX_NKSellCurrency should run row validation", charge, rowErrorMessage);

			charge.RemoveRowError(rowErrorMessage);
			Assert(!charge.HasRowErrors);
			charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			AssertNoRowErrors(@"JR_RX_NKSellCurrency should not run row validation because current JR_RX_NKSellCurrency
					is same as new value", charge);

			charge.JR_RX_NKSellCurrency = TestObjectCreator.EUR.RX_Code;
			AssertHasRowError("JR_RX_NKSellCurrency should run row validation because now we are setting a different value",
				charge, rowErrorMessage);
		}

		#region helpers

		Charge CreateCharge()
		{
			var shipment = TestObjectCreator.CreateShipment("S1234");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			return charge;
		}

		void UpdateChargeLocalAndOsCostAmtsToProduceRowValidationError(Charge charge)
		{
			charge.JR_LocalCostAmt = 15m;
			Factory.Save();

			string updateCommand = string.Format(@"
UPDATE dbo.JobCharge
SET
	JR_OSCostAmt = 30,
	JR_SystemLastEditTimeUtc = GETUTCDATE(),
	JR_SystemLastEditUser = '~BP'
WHERE
	JR_PK = '{0}'", charge.PK);
			((IDbConnected)Factory).Connection.ExecuteNonQuery(updateCommand);

			charge.Reload();
		}

		readonly ZString rowErrorMessage = "OS cost amount should be same as local cost amount when cost currency is local currency.";

		public AccTaxRate TaxRate2
		{
			get
			{
				return fTaxRate2 ?? (fTaxRate2 = TestObjectCreator.CreateTaxRate("GST2", "GST Rate 2", AccTaxRate.Types.Rated, 20, string.Empty, 0, 1));
			}
		}
		AccTaxRate fTaxRate2;

		public AccWithholding WHTRate2
		{
			get
			{
				return fWHTRate2 ?? (fWHTRate2 = TestObjectCreator.CreateOrLoadWithholdingTax("WHT2", "WHT Rate 2", 4m));
			}
		}
		AccWithholding fWHTRate2;

		#endregion

		#endregion

		#region IPostingCharge

		public void TestIPostingCharge_BillInLocalCurrency()
		{
			var shipment = TestObjectCreator.CreateShipment("S1");
			var job = TestObjectCreator.CreateJob(shipment, false);

			var charge = job.Charges.AddNew();
			charge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			Assert("Sell Invoice Currency is not set", charge.JR_RX_NKSellInvoiceCurrency.IsEmpty);
			AssertEquals("BillInLocalCurrency", true, charge.BillInLocalCurrency);
			AssertEquals("BillInInvoiceCurrency", false, charge.BillInInvoiceCurrency);

			var castCharge = charge as IPostingCharge;
			AssertEquals("Bill in Local Currency", true, castCharge.BillInLocalCurrency);

			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			AssertEquals("BillInLocalCurrency", false, charge.BillInLocalCurrency);
			AssertEquals("BillInInvoiceCurrency", true, charge.BillInInvoiceCurrency);
			AssertEquals("Bill in Local Currency", false, castCharge.BillInLocalCurrency);

			charge.JR_RX_NKSellCurrency = TestObjectCreator.EUR.RX_Code;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			AssertEquals("BillInLocalCurrency", false, charge.BillInLocalCurrency);
			AssertEquals("BillInInvoiceCurrency", false, charge.BillInInvoiceCurrency);
			AssertEquals("Bill in Local Currency", false, castCharge.BillInLocalCurrency);
		}

		#endregion

		#region IReceivablesPostingCharge

		public void TestIReceivablesPostingCharge_SellCurrency()
		{
			var shipment = TestObjectCreator.CreateShipment("S1");
			var job = TestObjectCreator.CreateJob(shipment, false);

			var charge = job.Charges.AddNew();
			charge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			Assert("Sell Invoice Currency is not set", charge.JR_RX_NKSellInvoiceCurrency.IsEmpty);
			AssertEquals("BillInLocalCurrency", true, charge.BillInLocalCurrency);
			AssertEquals("BillInInvoiceCurrency", false, charge.BillInInvoiceCurrency);

			var castCharge = charge as IReceivablesPostingCharge;
			AssertEquals("Local Currency", "AUD", castCharge.SellCurrency.RX_Code);

			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			AssertEquals("BillInLocalCurrency", false, charge.BillInLocalCurrency);
			AssertEquals("BillInInvoiceCurrency", true, charge.BillInInvoiceCurrency);
			castCharge = charge;
			AssertEquals("Sell Invoice Currency", "USD", castCharge.SellCurrency.RX_Code);

			charge.JR_RX_NKSellCurrency = TestObjectCreator.EUR.RX_Code;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			AssertEquals("BillInLocalCurrency", false, charge.BillInLocalCurrency);
			AssertEquals("BillInInvoiceCurrency", false, charge.BillInInvoiceCurrency);
			AssertEquals("Foreign Currency", "EUR", castCharge.SellCurrency.RX_Code);
		}

		public void TestIReceivablesPostingCharge_OSSellAmount()
		{
			var shipment = TestObjectCreator.CreateShipment("S1");
			var job = TestObjectCreator.CreateJob(shipment, false);

			var charge = job.Charges.AddNew();
			charge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_OSSellAmt = 100m;
			Assert("Sell Invoice Currency is not set", charge.JR_RX_NKSellInvoiceCurrency.IsEmpty);
			AssertEquals("BillInLocalCurrency", true, charge.BillInLocalCurrency);
			AssertEquals("BillInInvoiceCurrency", false, charge.BillInInvoiceCurrency);

			var castCharge = charge as IReceivablesPostingCharge;
			AssertEquals("JR_OSSellAmt", 100m, castCharge.OSSellAmount);

			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.AUD.RX_Code;
			AssertEquals("BillInLocalCurrency", true, charge.BillInLocalCurrency);
			AssertEquals("BillInInvoiceCurrency", false, charge.BillInInvoiceCurrency);
			AssertEquals("JR_OSSellInvoiceAmt", 0m, charge.JR_OSSellInvoiceAmt);
			AssertEquals("JR_OSSellInvoiceAmt_ForDisplay", "", charge.JR_OSSellInvoiceAmt_ForDisplay);
			AssertEquals("OSSellAmount same as JR_OSSellAmt", 100m, castCharge.OSSellAmount);

			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
			var exRate = ((IExchangeRateProvider)job).GetExchangeRate(TestObjectCreator.USD.RX_Code, ZGuid.Empty, ExchangeRateOrgTypeEnum.Debtor.ToLedger()); //.GetRateThatContainsCurrency(TestObjectCreator.USD, true);
			AssertNotNull(exRate);
			exRate.SetBuyRate_ForTestOnly(0.5m);
			AssertNotEquals("Not the same as JR_OSSellAmt", charge.JR_OSSellAmt, castCharge.OSSellAmount);
			AssertEquals("JR_OSSellInvoiceAmt", 50m, charge.JR_OSSellInvoiceAmt);
			AssertEquals("JR_OSSellInvoiceAmt_ForDisplay", "50.00", charge.JR_OSSellInvoiceAmt_ForDisplay);
			AssertEquals("OSSellAmount same as JR_OSSellInvoiceAmt", charge.JR_OSSellInvoiceAmt, castCharge.OSSellAmount);

			var revLine = Factory.NewWithValidTestData<ARInvoiceLine>();
			charge.JR_AL_ARLine = revLine.PK;
			revLine.AL_ExchangeRate = 0.55m;

			AssertNotEquals("Currencies are different", charge.JR_RX_NKSellInvoiceCurrency, revLine.AL_RX_NKTransactionCurrency);
			AssertNotEquals("Not the same as JR_OSSellAmt", charge.JR_OSSellAmt, castCharge.OSSellAmount);
			AssertEquals("JR_OSSellInvoiceAmt", 50m, charge.JR_OSSellInvoiceAmt);
			AssertEquals("JR_OSSellInvoiceAmt_ForDisplay", "50.00", charge.JR_OSSellInvoiceAmt_ForDisplay);
			AssertEquals("OSSellAmount same as JR_OSSellInvoiceAmt", charge.JR_OSSellInvoiceAmt, castCharge.OSSellAmount);

			revLine.AL_RX_NKTransactionCurrency = charge.JR_RX_NKSellInvoiceCurrency;
			revLine.AL_ExchangeRate = 0.55m;
			revLine.AL_LineAmount = 100m;

			AssertNotEquals("Not the same as JR_OSSellAmt", charge.JR_OSSellAmt, castCharge.OSSellAmount);
			AssertEquals("JR_OSSellInvoiceAmt", 55m, charge.JR_OSSellInvoiceAmt);
			AssertEquals("JR_OSSellInvoiceAmt_ForDisplay", "55.00", charge.JR_OSSellInvoiceAmt_ForDisplay);
			AssertEquals("OSSellAmount same as JR_OSSellInvoiceAmt", charge.JR_OSSellInvoiceAmt, castCharge.OSSellAmount);
		}

		public void TestIReceivablesPostingCharge_OSSellTaxAmount()
		{
			var shipment = TestObjectCreator.CreateShipment("S1");
			var job = TestObjectCreator.CreateJob(shipment, false);

			var charge = job.Charges.AddNew();
			charge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			charge.JR_OSSellAmt = 100m;
			Assert("Sell Invoice Currency is not set", charge.JR_RX_NKSellInvoiceCurrency.IsEmpty);
			AssertEquals("BillInLocalCurrency", true, charge.BillInLocalCurrency);
			AssertEquals("BillInInvoiceCurrency", false, charge.BillInInvoiceCurrency);
			AssertEquals("JR_OSSellGSTAmt", 10m, charge.JR_OSSellGSTAmt_Calc);
			AssertEquals("JR_OSSellInvoiceGSTAmt", 0m, charge.JR_OSSellInvoiceGSTAmt);
			AssertEquals("JR_OSSellInvoiceGSTAmt_ForDisplay", "", charge.JR_OSSellInvoiceGSTAmt_ForDisplay);

			var castCharge = charge as IReceivablesPostingCharge;
			AssertEquals("JR_OSSellAmt", 100m, castCharge.OSSellAmount);
			AssertEquals("JR_OSSellGSTAmt", 10m, castCharge.OSSellTaxAmount);

			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.AUD.RX_Code;
			AssertEquals("BillInLocalCurrency", true, charge.BillInLocalCurrency);
			AssertEquals("BillInInvoiceCurrency", false, charge.BillInInvoiceCurrency);
			AssertEquals("JR_OSSellInvoiceAmt same as JR_OSSellAmt", 100m, castCharge.OSSellAmount);
			AssertEquals("JR_OSSellInvoiceGSTAmt same as JR_OSSellGSTAmt", 10m, castCharge.OSSellTaxAmount);
			AssertEquals("JR_OSSellInvoiceGSTAmt", 0m, charge.JR_OSSellInvoiceGSTAmt);
			AssertEquals("JR_OSSellInvoiceGSTAmt_ForDisplay", "", charge.JR_OSSellInvoiceGSTAmt_ForDisplay);

			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
			var exRate = ((IExchangeRateProvider)job).GetExchangeRate(TestObjectCreator.USD.RX_Code, ZGuid.Empty, ExchangeRateValidLedgerEnum.AR);
			AssertNotNull(exRate);
			exRate.SetBuyRate_ForTestOnly(0.5m);
			AssertEquals("JR_OSSellInvoiceAmt", charge.JR_OSSellInvoiceAmt, castCharge.OSSellAmount);
			AssertNotEquals("Not the same as JR_OSSellGSTAmt", charge.JR_OSSellGSTAmt_Calc, castCharge.OSSellTaxAmount);
			AssertEquals("JR_OSSellInvoiceGSTAmt", 5m, charge.JR_OSSellInvoiceGSTAmt);
			AssertEquals("JR_OSSellInvoiceGSTAmt_ForDisplay", "5.00", charge.JR_OSSellInvoiceGSTAmt_ForDisplay);
			AssertEquals("OSSellTaxAmount same as JR_OSSellInvoiceAmt", charge.JR_OSSellInvoiceGSTAmt, castCharge.OSSellTaxAmount);
		}

		public void TestIReceivablesPostingCharge_SellExchangeRate()
		{
			var shipment = TestObjectCreator.CreateShipment("S1");
			var job = TestObjectCreator.CreateJob(shipment, false);

			var charge = job.Charges.AddNew();
			charge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			Assert("Sell Invoice Currency is not set", charge.JR_RX_NKSellInvoiceCurrency.IsEmpty);
			AssertEquals("BillInLocalCurrency", true, charge.BillInLocalCurrency);
			AssertEquals("BillInInvoiceCurrency", false, charge.BillInInvoiceCurrency);
			AssertEquals("JR_OSSellExRate", 1m, charge.JR_OSSellExRate);

			var castCharge = charge as IReceivablesPostingCharge;
			AssertEquals("JR_OSSellExRate", 1m, castCharge.SellExchangeRate);

			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.AUD.RX_Code;
			AssertEquals("BillInLocalCurrency", true, charge.BillInLocalCurrency);
			AssertEquals("BillInInvoiceCurrency", false, charge.BillInInvoiceCurrency);
			AssertEquals("JR_OSSellInvoiceExRate same as JR_OSSellExRate", 1m, castCharge.SellExchangeRate);
			AssertEquals("JR_OSSellExRate_ForDisplay", "", charge.JR_OSSellInvoiceExRate_ForDisplay);

			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
			AssertEquals("BillInLocalCurrency", false, charge.BillInLocalCurrency);
			AssertEquals("BillInInvoiceCurrency", true, charge.BillInInvoiceCurrency);
			var exRate = ((IExchangeRateProvider)job).GetExchangeRate(TestObjectCreator.USD.RX_Code, ZGuid.Empty, ExchangeRateValidLedgerEnum.AR);
			AssertNotNull(exRate);
			exRate.SetBuyRate_ForTestOnly(0.5m);
			AssertNotEquals("Not the same as JR_OSSellExRate", charge.JR_OSSellExRate, castCharge.SellExchangeRate);
			AssertEquals("SellExchangeRate", 0.5m, charge.JR_OSSellInvoiceExRate);
			AssertEquals("JR_OSSellExRate_ForDisplay", "0.500000", charge.JR_OSSellInvoiceExRate_ForDisplay);
			AssertEquals("SellExchangeRate", charge.JR_OSSellInvoiceExRate, castCharge.SellExchangeRate);

			job.AddCurrency(TestObjectCreator.EUR, ExchangeRateValidLedgerEnum.AR);
			exRate = ((IExchangeRateProvider)job).GetExchangeRate(TestObjectCreator.EUR.RX_Code, ZGuid.Empty, ExchangeRateValidLedgerEnum.AR);
			AssertNotNull(exRate);
			exRate.SetBuyRate_ForTestOnly(0.7m);

			charge.JR_RX_NKSellCurrency = TestObjectCreator.EUR.RX_Code;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			AssertEquals("BillInLocalCurrency", true, charge.BillInLocalCurrency);
			AssertEquals("BillInInvoiceCurrency", false, charge.BillInInvoiceCurrency);
			AssertEquals("JR_OSSellExRate", 0.7m, charge.JR_OSSellExRate);
			AssertEquals("JR_OSSellInvoiceAmt", 0m, charge.JR_OSSellInvoiceExRate);
			AssertEquals("JR_OSSellExRate_ForDisplay", "", charge.JR_OSSellInvoiceExRate_ForDisplay);
			AssertEquals("SellExchangeRate same as JR_OSSellExRate", charge.JR_OSSellExRate, castCharge.SellExchangeRate);
		}

		public void TestIReceivablesPostingCharge_InvoiceSellExchangeRate_NoCfx()
		{
			AssertIReceivablesPostingCharge_InvoiceSellExchangeRate(setupCFX: false, enableCFX: false);
		}

		public void TestIReceivablesPostingCharge_InvoiceSellExchangeRate_NoCfxSetUpButEnabled()
		{
			AssertIReceivablesPostingCharge_InvoiceSellExchangeRate(setupCFX: false, enableCFX: true);
		}

		public void TestIReceivablesPostingCharge_InvoiceSellExchangeRate_WithCfxSetupButNotEnabled()
		{
			AssertIReceivablesPostingCharge_InvoiceSellExchangeRate(setupCFX: true, enableCFX: false);
		}

		public void TestIReceivablesPostingCharge_InvoiceSellExchangeRate_WithCfxSetupAndEnabled()
		{
			AssertIReceivablesPostingCharge_InvoiceSellExchangeRate(setupCFX: true, enableCFX: true);
		}

		void AssertIReceivablesPostingCharge_InvoiceSellExchangeRate(bool setupCFX, bool enableCFX)
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();
			var rateFactory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(rateFactory);
			creator.CreateExchangeRate(creator.USD, "BUY", 0.55m, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			creator.CreateExchangeRate(creator.EUR, "BUY", 0.75m, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			rateFactory.Save();

			var orgFactory = new BusinessObjectFactory();
			var debtor = orgFactory.NewWithValidTestData<OrgHeader>();
			debtor.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			if (setupCFX)
			{
				debtor.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, 10m);
			}
			orgFactory.Save();

			var shipment = TestObjectCreator.CreateShipment("S1");
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "INBOM";
			var job = TestObjectCreator.CreateJob(shipment, false);

			using (AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, enableCFX))
			{
				var charge = job.Charges.AddNew();
				charge.JR_OH_SellAccount = debtor.PK;
				charge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				Assert("Sell Invoice Currency is not set", charge.JR_RX_NKSellInvoiceCurrency.IsEmpty);
				AssertEquals("BillInLocalCurrency", true, charge.BillInLocalCurrency);
				AssertEquals("BillInInvoiceCurrency", false, charge.BillInInvoiceCurrency);
				AssertEquals("JR_OSSellExRate", 1m, charge.JR_OSSellExRate);

				var castCharge = charge as IReceivablesPostingCharge;
				AssertEquals("JR_OSSellExRate", 1m, castCharge.SellExchangeRate);
				AssertEquals("InvoiceSellExchangeRate", 1m, castCharge.InvoiceSellExchangeRate);

				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.AUD.RX_Code;
				AssertEquals("BillInLocalCurrency", true, charge.BillInLocalCurrency);
				AssertEquals("BillInInvoiceCurrency", false, charge.BillInInvoiceCurrency);
				AssertEquals("JR_OSSellInvoiceExRate same as JR_OSSellExRate", 1m, castCharge.SellExchangeRate);
				AssertEquals("InvoiceSellExchangeRate", 1m, castCharge.InvoiceSellExchangeRate);

				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
				AssertEquals("BillInLocalCurrency", false, charge.BillInLocalCurrency);
				AssertEquals("BillInInvoiceCurrency", true, charge.BillInInvoiceCurrency);
				AssertNotNull("SellInvoiceExchangeRate", charge.SellInvoiceExchangeRate);
				AssertEquals("SellInvoiceExchangeRate.Rate", 0.55m, charge.SellInvoiceExchangeRate.Rate);
				AssertEquals("SellInvoiceExchangeRate.CFXPercent", setupCFX ? 10m : decimal.Zero, charge.SellInvoiceExchangeRate.CFXPercent);
				AssertNotEquals("Not the same as JR_OSSellExRate", charge.JR_OSSellExRate, castCharge.SellExchangeRate);
				AssertEquals("JR_OSSellInvoiceAmt", 0.55m, charge.JR_OSSellInvoiceExRate);
				AssertEquals("JR_OSSellInvoiceAmt", charge.JR_OSSellInvoiceExRate, castCharge.SellExchangeRate);
				AssertEquals("InvoiceSellExchangeRate", setupCFX ? 0.495m : 0.55m, castCharge.InvoiceSellExchangeRate);

				charge.JR_RX_NKSellCurrency = TestObjectCreator.EUR.RX_Code;
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
				AssertEquals("BillInLocalCurrency", false, charge.BillInLocalCurrency);
				AssertEquals("BillInInvoiceCurrency", true, charge.BillInInvoiceCurrency);
				AssertEquals("JR_OSSellExRate", setupCFX ? 0.675m : 0.75m, charge.JR_OSSellExRate);
				AssertEquals("SellExchangeRate", 0.55m, castCharge.SellExchangeRate);
				AssertEquals("InvoiceSellExchangeRate", setupCFX ? 0.495m : 0.55m, castCharge.InvoiceSellExchangeRate);

				var revLine = Factory.NewWithValidTestData<ARInvoiceLine>();
				charge.JR_AL_ARLine = revLine.PK;
				revLine.AL_ExchangeRate = 0.6m;

				AssertNotEquals("Currencies are different", charge.JR_RX_NKSellInvoiceCurrency, revLine.AL_RX_NKTransactionCurrency);
				AssertEquals("BillInLocalCurrency", false, charge.BillInLocalCurrency);
				AssertEquals("BillInInvoiceCurrency", true, charge.BillInInvoiceCurrency);
				AssertEquals("JR_OSSellExRate", setupCFX ? 0.675m : 0.75m, charge.JR_OSSellExRate);
				AssertEquals("SellExchangeRate", 0.55m, castCharge.SellExchangeRate);
				AssertEquals("InvoiceSellExchangeRate", setupCFX ? 0.495m : 0.55m, castCharge.InvoiceSellExchangeRate);

				revLine.AL_RX_NKTransactionCurrency = charge.JR_RX_NKSellInvoiceCurrency;
				revLine.AL_ExchangeRate = 0.6m;
				AssertEquals("BillInLocalCurrency", false, charge.BillInLocalCurrency);
				AssertEquals("BillInInvoiceCurrency", true, charge.BillInInvoiceCurrency);
				AssertEquals("JR_OSSellExRate", setupCFX ? 0.675m : 0.75m, charge.JR_OSSellExRate);
				AssertEquals("SellExchangeRate from Revenue Line", 0.6m, castCharge.SellExchangeRate);
				AssertEquals("InvoiceSellExchangeRate from Job Ex Rate", setupCFX ? 0.495m : 0.55m, castCharge.InvoiceSellExchangeRate);
			}
		}

		public void TestIReceivablesPostingCharge_OsExTaxAmount()
		{
			var shipment = TestObjectCreator.CreateShipment("S1");
			var job = TestObjectCreator.CreateJob(shipment, false);

			var jpy = RefCurrency.LoadFromCurrencyCode(Factory, "JPY");
			var usd = RefCurrency.LoadFromCurrencyCode(Factory, "USD");

			var jpyRate = job.ExchangeRates.AddNew();
			jpyRate.JF_RX_NKRateCurrency = jpy.RX_Code;
			jpyRate.JF_BaseRate = 100m;

			var usdRate = job.ExchangeRates.AddNew();
			usdRate.JF_RX_NKRateCurrency = jpy.RX_Code;
			usdRate.JF_BaseRate = 10m;

			var charge = job.Charges.AddNew();
			charge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			charge.JR_OSSellAmt = 100m;
			Assert("Precondition - charge BillInLocalCurrency", charge.BillInLocalCurrency);
			Assert("Precondition - charge BillInInvoiceCurrency", !charge.BillInInvoiceCurrency);

			var castCharge = charge as IReceivablesPostingCharge;
			AssertNotNull(castCharge);
			AssertEquals("OsExTaxAmount when bill in local currency", 100m, castCharge.OsExTaxAmount);

			charge.JR_RX_NKSellInvoiceCurrency = jpy.RX_Code;
			Assert("Precondition - charge BillInLocalCurrency", !charge.BillInLocalCurrency);
			Assert("Precondition - charge BillInInvoiceCurrency", charge.BillInInvoiceCurrency);
			AssertEquals("OsExTaxAmount when bill in invoice currency", 10000m, castCharge.OsExTaxAmount);

			charge.JR_RX_NKSellInvoiceCurrency = ZString.Empty;
			charge.JR_RX_NKSellCurrency = usd.RX_Code;
			charge.JR_OSSellAmt = 150m;
			charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(10m);
			Assert("Precondition - charge BillInLocalCurrency", charge.BillInLocalCurrency);
			Assert("Precondition - charge BillInInvoiceCurrency", !charge.BillInInvoiceCurrency);
			AssertEquals("OsExTaxAmount when bill in invoice currency", 15m, castCharge.OsExTaxAmount);

			charge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			Assert("Precondition - charge BillInLocalCurrency", !charge.BillInLocalCurrency);
			Assert("Precondition - charge BillInInvoiceCurrency", !charge.BillInInvoiceCurrency);
			AssertEquals("OsExTaxAmount when bill in invoice currency", 150m, castCharge.OsExTaxAmount);
		}

		public void TestIReceivablesPostingCharge_OsTaxAmount()
		{
			var shipment = TestObjectCreator.CreateShipment("S1");
			var job = TestObjectCreator.CreateJob(shipment, false);

			TestObjectCreator.GST1.SetRateNumerator_ForTestOnly(5);
			var jpy = RefCurrency.LoadFromCurrencyCode(Factory, "JPY");
			var usd = RefCurrency.LoadFromCurrencyCode(Factory, "USD");

			var jpyRate = job.ExchangeRates.AddNew();
			jpyRate.JF_RX_NKRateCurrency = jpy.RX_Code;
			jpyRate.JF_BaseRate = 100m;

			var usdRate = job.ExchangeRates.AddNew();
			usdRate.JF_RX_NKRateCurrency = jpy.RX_Code;
			usdRate.JF_BaseRate = 10m;

			var charge = job.Charges.AddNew();
			charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			charge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			charge.JR_OSSellAmt = 10.05m;
			Assert("Precondition - charge1 BillInLocalCurrency", charge.BillInLocalCurrency);
			Assert("Precondition - charge1 BillInInvoiceCurrency", !charge.BillInInvoiceCurrency);

			var castCharge = charge as IReceivablesPostingCharge;
			AssertNotNull(castCharge);
			AssertEquals("OsTaxAmount when bill in local currency", 0.5m, castCharge.OsTaxAmount);

			charge.JR_RX_NKSellInvoiceCurrency = jpy.RX_Code;
			Assert("Precondition - charge1 BillInLocalCurrency", !charge.BillInLocalCurrency);
			Assert("Precondition - charge1 BillInInvoiceCurrency", charge.BillInInvoiceCurrency);
			AssertEquals("OsExTaxAmount when bill in invoice currency", 50m, castCharge.OsTaxAmount);

			charge.JR_RX_NKSellInvoiceCurrency = ZString.Empty;
			charge.JR_RX_NKSellCurrency = usd.RX_Code;
			charge.JR_OSSellAmt = 150m;
			charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(10m);
			Assert("Precondition - charge1 BillInLocalCurrency", charge.BillInLocalCurrency);
			Assert("Precondition - charge1 BillInInvoiceCurrency", !charge.BillInInvoiceCurrency);
			AssertEquals("OsTaxAmount when bill in invoice currency", 0.75m, castCharge.OsTaxAmount);

			charge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			Assert("Precondition - charge1 BillInLocalCurrency", !charge.BillInLocalCurrency);
			Assert("Precondition - charge1 BillInInvoiceCurrency", !charge.BillInInvoiceCurrency);
			AssertEquals("OsExTaxAmount when bill in invoice currency", 7.5m, castCharge.OsTaxAmount);
		}

		#endregion

		#region SellInvoiceCurrency

		public void TestJR_OSSellInvoiceGSTAmt()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			var exRate = job.ExchangeRates.AddNew();
			exRate.JF_RX_NKRateCurrency = TestObjectCreator.USD.Code;
			exRate.JF_BaseRate = 0.5M;

			var charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_JH = job.PK;
			charge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
			charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			charge.JR_LocalSellAmt = 100M;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_RX_NKSellInvoiceCurrency = "USD";
			AssertEquals("Precondition:", true, charge.BillInInvoiceCurrency);
			AssertEquals("Precondition:", 50M, charge.JR_OSSellInvoiceAmt);
			AssertEquals("JR_OSSellInvoiceGSTAmt", 5M, charge.JR_OSSellInvoiceGSTAmt);
		}

		public void TestJR_OSSellInvoiceGSTAmt_ReadOnly()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			var charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_JH = job.PK;
			charge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
			charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			charge.JR_LocalSellAmt = 100M;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			AssertEquals("Precondition:", false, charge.BillInInvoiceCurrency);
			AssertEquals("JR_OSSellInvoiceGSTAmt is readonly", true, charge.JR_OSSellInvoiceGSTAmtInfo.ReadOnly);

			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_RX_NKSellInvoiceCurrency = "USD";
			AssertEquals("Precondition:", true, charge.BillInInvoiceCurrency);
			AssertEquals("JR_OSSellInvoiceGSTAmt is readonly", true, charge.JR_OSSellInvoiceGSTAmtInfo.ReadOnly);
		}

		public void TestJR_OSSellInvoiceAmt()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			var exRate = job.ExchangeRates.AddNew();
			exRate.JF_RX_NKRateCurrency = TestObjectCreator.USD.Code;
			exRate.JF_BaseRate = 0.5m;

			var charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_JH = job.PK;
			charge.JR_LocalSellAmt = 100M;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_RX_NKSellInvoiceCurrency = "USD";

			AssertEquals("Precondition:", true, charge.BillInInvoiceCurrency);
			AssertEquals("JR_OSSellInvoiceAmt", 50M, charge.JR_OSSellInvoiceAmt);

			exRate.JF_BaseRate = 3m;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_RX_NKSellInvoiceCurrency = "USD";
			AssertEquals("Precondition:", true, charge.BillInInvoiceCurrency);

			charge.JR_OSSellAmt = 0.01M;
			AssertEquals("JR_OSSellInvoiceAmt", 0.01M, charge.JR_OSSellInvoiceAmt);
		}

		public void TestJR_OSSellInvoiceAmt_ReadOnly()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			var charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_JH = job.PK;
			charge.JR_LocalSellAmt = 100M;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			AssertEquals("Precondition:", false, charge.BillInInvoiceCurrency);
			AssertEquals("JR_OSSellInvoiceAmt is readonly", true, charge.JR_OSSellInvoiceAmtInfo.ReadOnly);

			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_RX_NKSellInvoiceCurrency = "USD";
			AssertEquals("Precondition:", true, charge.BillInInvoiceCurrency);
			AssertEquals("JR_OSSellInvoiceAmt is readonly", true, charge.JR_OSSellInvoiceAmtInfo.ReadOnly);
		}

		public void TestSettingJR_RX_NKSellInvoiceCurrencyAddsJobExchangeRate()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			var charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_JH = job.PK;
			charge.JR_LocalSellAmt = 100M;

			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_RX_NKSellInvoiceCurrency = "CAD";

			AssertEquals("Precondition:", true, charge.BillInInvoiceCurrency);
			AssertEquals("Should add exchange rate on the job", 1, job.ExchangeRates.Count);
			AssertEquals("Should add exchange rate on the job", "CAD", job.ExchangeRates[0].JF_RX_NKRateCurrency);
		}

		public void TestJR_RX_NKSellInvoiceCurrency_ReadOnly()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			var charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_JH = job.PK;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_OSSellExRate = 1.5M;
			charge.JR_LocalSellAmt = 100M;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			AssertEquals("Precondition:", false, charge.BillInInvoiceCurrency);
			AssertEquals("Precondition:", true, charge.JR_RX_NKSellInvoiceCurrency_ReadOnly_ForTestOnly);

			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			AssertEquals("Sell Invoice currency can be editted", false, charge.JR_RX_NKSellInvoiceCurrency_ReadOnly_ForTestOnly);
		}

		#endregion

		#region Exchange Rates

		public void TestUpdateSellInvoiceExchangeRateIsCalledInConstructor()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment, false);

			var charge = Factory.New<Charge>();

			charge.JR_JH = job.PK;
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
			charge.JR_OSSellAmt = 100m;
			AssertNull(charge.SellInvoiceExchangeRate);

			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.Code;
			Assert(charge.BillInInvoiceCurrency);

			var eurRate = charge.SellInvoiceExchangeRate;
			AssertNotNull(eurRate);
			eurRate.SetBuyRate_ForTestOnly(0.5m);

			AssertEquals(100m, charge.JR_LocalSellAmt);
			AssertEquals(50m, charge.JR_OSSellInvoiceAmt);

			Factory.Save();

			// charge gets wired up with SellInvoiceExchangeRate by calling UpdateSellInvoiceExchangeRate in constructor
			var newFactory = new BusinessObjectFactory();
			var loadedCharge = newFactory.Load<Charge>(charge.PK);
			var loadedJob = newFactory.Load<Job>(job.PK);

			var jobExRate = loadedJob.ExchangeRates.Cast<ExchangeRate>().FirstOrDefault(r => r.JF_RX_NKRateCurrency == eurRate.CurrencyCode && r.OrgType == ExchangeRateOrgTypeEnum.Debtor && r.JF_OH_Org == eurRate.OrgPk);
			AssertNotNull(jobExRate);
			jobExRate.JF_BaseRate = 2m;

			AssertEquals(100m, loadedCharge.JR_LocalSellAmt);
			AssertEquals(200m, loadedCharge.JR_OSSellInvoiceAmt);
			AssertNotNull(loadedCharge.SellInvoiceExchangeRate);
			AssertEquals(loadedCharge.SellInvoiceExchangeRate.ExchangeRatePk, jobExRate.PK);

			newFactory = new BusinessObjectFactory();
			loadedCharge = newFactory.Load<Charge>(charge.PK);

			Assert("No Exchange Rates in the newFactory", !newFactory.Load<ExchangeRate>(new ZQuery() { FetchOnlyFromLocalCache = true }).Any());

			AssertNotNull("Sell Invoice Exchange Rate wrapper should be created on the first access", loadedCharge.SellInvoiceExchangeRate);
			AssertEquals(eurRate.ExchangeRatePk, loadedCharge.SellInvoiceExchangeRate.ExchangeRatePk);

			var periodicInvocingFactory = new BusinessObjectFactory();
			periodicInvocingFactory.SetContext(BusinessContext.PeriodicInvoicePosting);
			loadedCharge = periodicInvocingFactory.Load<Charge>(charge.PK);
			Assert("No Exchange Rates in the periodicInvocingFactory", !periodicInvocingFactory.Load<ExchangeRate>(new ZQuery() { FetchOnlyFromLocalCache = true }).Any());
		}

		[DisableZeroExchangeRateOverriding]
		public void TestChargeCfxAndSellExRateAreUpdatedFromRelatedJobExRateWithCFX()
		{
			AssertChargeCfxAndSellExRateAreUpdatedFromRelatedJobExRate();
		}

		public void TestChargeCfxAndSellExRateAreUpdatedFromRelatedJobExRateWithoutCFX()
		{
			AssertChargeCfxAndSellExRateAreUpdatedFromRelatedJobExRate(enableCFX: false);
		}

		void AssertChargeCfxAndSellExRateAreUpdatedFromRelatedJobExRate(bool enableCFX = true)
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();
			var newFactory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(newFactory);
			creator.CreateExchangeRate(creator.USD, "BUY", 0.75m, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			creator.CreateExchangeRate(creator.EUR, "BUY", 0.82m, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			newFactory.Save();

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment, false);

			var cfxConfig = TestObjectCreator.ABIGAS.CompanyData.AccCFXConfigurations.AddNew();
			cfxConfig.JCF_JobType = "SHP";
			cfxConfig.JCF_TransportMode = "ALL";
			cfxConfig.JCF_ServiceDirection = "ALL";
			cfxConfig.JCF_CFXPercentage = 5m;

			using (AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, enableCFX))
			{
				var charge = Factory.New<Charge>();
				charge.JR_JH = job.PK;
				AssertNull(charge.RevenueExchangeRate);
				AssertEquals(1m, charge.JR_OSSellExRate);
				Assert("Always read only", charge.JR_OSSellExRateInfo.ReadOnly);
				AssertEquals(decimal.Zero, charge.JR_LineCFX);

				charge.JR_AC = TestObjectCreator.CC1.PK;
				charge.JR_OSSellAmt = 100m;
				AssertEquals(100m, charge.JR_LocalSellAmt);
				AssertNull(charge.RevenueExchangeRate);
				AssertEquals(1m, charge.JR_OSSellExRate);
				Assert("Always read only", charge.JR_OSSellExRateInfo.ReadOnly);
				AssertEquals(decimal.Zero, charge.JR_LineCFX);
				AssertEquals(decimal.Zero, charge.JR_CFXAmt);

				var usdDebtorJobRate = job.ExchangeRates.AddRate(TestObjectCreator.USD, 0.79m, ZGuid.Empty, ExchangeRateOrgTypeEnum.Debtor);

				charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
				AssertNotNull(charge.RevenueExchangeRate);
				var usdGenericRate = charge.RevenueExchangeRate;
				AssertEquals(usdDebtorJobRate.PK, usdGenericRate.ExchangeRatePk);
				AssertEquals(0.79m, usdGenericRate.Rate);
				AssertEquals(0.79m, usdGenericRate.SellRate);
				AssertEquals(0.79m, charge.JR_OSSellExRate);
				Assert("Always read only", charge.JR_OSSellExRateInfo.ReadOnly);
				AssertEquals(decimal.Zero, charge.JR_LineCFX);

				usdDebtorJobRate.JF_BaseRate = 0.77m;
				AssertEquals(0.77m, usdGenericRate.Rate);
				AssertEquals(0.77m, usdGenericRate.SellRate);
				AssertEquals(0.77m, charge.JR_OSSellExRate);
				Assert("Always read only", charge.JR_OSSellExRateInfo.ReadOnly);
				AssertEquals(decimal.Zero, charge.JR_LineCFX);

				charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
				charge.JR_OSSellAmt = 100m;

				var usdRate = charge.RevenueExchangeRate;
				AssertNotNull(usdRate);
				AssertNotEquals(usdDebtorJobRate.PK, usdRate.ExchangeRatePk);
				var usdJobRate = Factory.Load<ExchangeRate>(usdRate.ExchangeRatePk);
				AssertEquals(0.75m, usdRate.Rate);
				AssertEquals(0.7125m, usdRate.SellRate);
				AssertEquals(0.7125m, charge.JR_OSSellExRate);
				Assert("Always read only", charge.JR_OSSellExRateInfo.ReadOnly);
				AssertEquals(140.35m, charge.JR_LocalSellAmt);
				AssertEquals(5m, charge.JR_LineCFX);
				AssertEquals(enableCFX ? 7.02m : decimal.Zero, charge.JR_CFXAmt);

				usdJobRate.JF_BaseRate = 0.8m;
				AssertEquals(0.8m, usdRate.Rate);
				AssertEquals(131.58m, charge.JR_LocalSellAmt);
				AssertEquals(0.76m, charge.JR_OSSellExRate);
				Assert("Always read only", charge.JR_OSSellExRateInfo.ReadOnly);
				AssertEquals(5m, charge.JR_LineCFX);
				AssertEquals(enableCFX ? 6.58m : decimal.Zero, charge.JR_CFXAmt);

				var eurJobRate = job.ExchangeRates.AddRate(TestObjectCreator.EUR, 0m, TestObjectCreator.ABIGAS.PK, ExchangeRateOrgTypeEnum.Debtor);

				charge.JR_RX_NKSellCurrency = TestObjectCreator.EUR.RX_Code;
				charge.JR_OSSellAmt = 100m;

				AssertNotNull(charge.RevenueExchangeRate);
				var eurRate = charge.RevenueExchangeRate;
				AssertEquals(eurJobRate.PK, eurRate.ExchangeRatePk);
				AssertNotEquals(usdRate.ExchangeRatePk, eurRate.ExchangeRatePk);

				AssertEquals(0.82m, eurRate.Rate);
				AssertEquals("CFX correction should be applied", 0.779m, charge.JR_OSSellExRate);
				Assert("Always read only", charge.JR_OSSellExRateInfo.ReadOnly);
				AssertEquals(100m, charge.JR_OSSellAmt);
				AssertEquals(128.37m, charge.JR_LocalSellAmt);
				AssertEquals(5m, charge.JR_LineCFX);
				AssertEquals(enableCFX ? 6.42m : decimal.Zero, charge.JR_CFXAmt);

				Factory.Save();
				AssertNotNull("There should be a WIP", charge.WIP);
				AssertEquals(1m, charge.WIP.AL_ExchangeRate);
				AssertEquals(enableCFX ? -121.95m : -128.37m, charge.WIP.AL_LineAmount);
				AssertEquals(enableCFX ? 6.42m : decimal.Zero, charge.JR_CFXAmt);
				AssertNull(charge.CFXLine);

				eurJobRate.JF_BaseRate = 0.85m;
				AssertEquals(0.85m, eurRate.Rate);
				AssertEquals("CFX correction should be applied", 0.8075m, charge.JR_OSSellExRate);
				AssertEquals(123.84m, charge.JR_LocalSellAmt);
				AssertEquals(100m, charge.JR_OSSellAmt);
				AssertEquals(5m, charge.JR_LineCFX);
				AssertEquals(enableCFX ? 6.19m : decimal.Zero, charge.JR_CFXAmt);

				// Prepare to post
				charge.ReverseWIP(ZDateTime.Now);
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("000001", TestObjectCreator.EUR, 0.825m, TestObjectCreator.ABIGAS);
				var invoiceLine = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC1, TestObjectCreator.EUR, 0.825m, "Test", 100m);

				charge.JR_AL_ARLine = invoiceLine.PK;
				charge.ARLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
				AssertEquals(0.825m, invoiceLine.AL_ExchangeRate);
				AssertEquals(121.21m, invoiceLine.AL_LineAmount);
				Assert(charge.IsRevenuePosted);
				AssertEquals(0.8075m, charge.JR_OSSellExRate);
				Assert("Always read only", charge.JR_OSSellExRateInfo.ReadOnly);

				eurJobRate.JF_BaseRate = 0.7m;
				AssertEquals("Should be no effect on the charge Ex Rate", 0.8075m, charge.JR_OSSellExRate);

				// Update Sell Ex Rate to get AR Invoice Line passing Critical Validation
				charge.JR_OSSellExRate = 0.825m;
				AssertEquals("CFX adjustment applied to the Ex Rate", 0.825m, charge.JR_OSSellExRate);

				Factory.Save();
				AssertEquals(0.825m, charge.JR_OSSellExRate);
				Assert("Always read only", charge.JR_OSSellExRateInfo.ReadOnly);
				AssertEquals(5m, charge.JR_LineCFX);

				AssertNull("Not attached to any Exchange Rates", charge.RevenueExchangeRate);
				eurJobRate.JF_BaseRate = 0.86m;
				AssertEquals("Should be no effect on the charge Ex Rate", 0.825m, charge.JR_OSSellExRate);
				Assert("Always read only", charge.JR_OSSellExRateInfo.ReadOnly);
			}
		}

		[ExpectNoExceptions]
		public void TestJR_OSSellInvoiceExRateWhenSetJR_RX_NKSellInvoiceCurrency()
		{
			var creator = new TestObjectCreator(Factory);
			creator.LocalClient.CompanyData.OB_RX_NKARDDefltCurrency = creator.AUD.Code;
			creator.LocalClient.CompanyData.OB_RX_NKAPDefltCurrency = creator.AUD.Code;
			var shipment = creator.CreateShipment("S001001");
			var job = creator.CreateJob(shipment, false, false);

			var charge = creator.CreateCharge(job, creator.CC1, 0, 0);
			AssertEquals(0m, charge.JR_OSSellInvoiceAmt);
			charge.JR_RX_NKSellInvoiceCurrency = creator.USD.Code;
			AssertEquals(1, job.ExchangeRates.Count);
			job.ExchangeRates[0].JF_BaseRate = 2m;
			Assert("Not marked as entered manually. Could be removed when not in use.", !job.ExchangeRates[0].JF_IsTransformed);
			AssertEquals("should be 2m", 2m, charge.JR_OSSellInvoiceExRate);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedJob = newFactory.Load<Job>(job.PK);
			var reloadedCharge = reloadedJob.Charges[0];
			reloadedCharge.JR_RX_NKSellInvoiceCurrency = creator.AUD.Code;
			AssertEquals("should be 0m", 0m, reloadedCharge.JR_OSSellInvoiceExRate);
			AssertEquals("Unused Ex Rate should be removed", 0, reloadedJob.ExchangeRates.Count);
		}

		public void TestIReceivablesPostingChargeLocalSellTaxAmount()
		{
			var shipment = TestObjectCreator.CreateShipment("S1");
			var job = TestObjectCreator.CreateJob(shipment, false);

			var charge = job.Charges.AddNew();
			charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			TestObjectCreator.GST1.SetRate_ForTestOnly(1, 1);
			charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			charge.JR_InvoiceType = "CUR";
			charge.JR_OSSellExRate = 14476m;
			charge.JR_OSSellAmt = 7200;

			var postingCharge = charge as IReceivablesPostingCharge;
			Assert(!charge.BillInInvoiceCurrency);
			AssertEquals("Sell Ex Rate reset to 1 from Job Ex Rate on setting OS Sell Amount", 1m, charge.JR_OSSellExRate);
			AssertEquals(72m, charge.JR_Sell_LocalGSTAmount);
			AssertEquals(charge.JR_Sell_LocalGSTAmount, postingCharge.LocalSellTaxAmount);

			charge.JR_InvoiceType = "FIN";
			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.RX_Code;
			Assert(charge.BillInInvoiceCurrency);
			AssertEquals(72m, charge.JR_SellInvoice_LocalGSTAmount);
			AssertEquals(charge.JR_SellInvoice_LocalGSTAmount, postingCharge.LocalSellTaxAmount);
		}

		#region Rounding Cases

		public void TestTaxesAmounts_WhenSellCurrencyIsLocal_InvoiceCurrencyIsForeign_InvoiceTypeIsUpdated()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Mexico))
			using (AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;

				var expectedOverseasTaxAmount = 0.70M;
				var expectedLocalTaxAmount = 14.62M;

				var taxRate = TestObjectCreator.CreateTaxRate("IVAREC", "Gravado Std. 25% of base & 6% Ret.", AccTaxRate.Types.Rated, 4, "RET", 15, 10, "MX");
				var cc10 = CreateChargeCode(taxRate);

				var job = CreateJob("Z00001011", TestObjectCreator.Debtor, true);
				TestObjectCreator.SetExchangeRate(job, TestObjectCreator.USD, 20.88m, TestObjectCreator.Debtor.PK, ExchangeRateOrgTypeEnum.Debtor);

				var charge = CreateJobCharge(job, cc10, "Test", GlbCompany.CurrentCompany.LocalCurrency, 585.69M, TestObjectCreator.Debtor);
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				AssertBillInInvoiceCurrency(charge, expectedOverseasTaxAmount, expectedLocalTaxAmount);

				charge.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice;
				AssertBillInInvoiceCurrency(charge, expectedOverseasTaxAmount, expectedLocalTaxAmount);

				charge.JR_InvoiceType = InvoiceTypesList.Codes.InvoicePerTaxCode;
				AssertBillInInvoiceCurrency(charge, expectedOverseasTaxAmount, expectedLocalTaxAmount);

				charge.JR_InvoiceType = InvoiceTypesList.Codes.DestinationChargesInvoice;
				AssertBillInInvoiceCurrency(charge, expectedOverseasTaxAmount, expectedLocalTaxAmount);

				var postingCharge = charge as IReceivablesPostingCharge;
				var invoice = (new ChargePoster(Factory)).Post(postingCharge);
				var invoiceLine = invoice.Lines[0];

				AssertEquals("AL_OSTaxAmount", expectedOverseasTaxAmount, invoiceLine.AL_OSTaxAmount);
				AssertEquals("AL_LocalTaxAmount", expectedLocalTaxAmount, invoiceLine.AL_LocalTaxAmount);
			}
		}

		public void TestTaxesAmounts_WhenSellCurrencyIsForeign_InvoiceCurrencyIsForeign_InvoiceTypeIsUpdated()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Mexico))
			using (AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;

				var expectedOverseasTaxAmount = 236.47M;
				var expectedLocalTaxAmount = 4937.49M;

				var taxRate = TestObjectCreator.CreateTaxRate("IVARET", "RateWithExtraRate", AccTaxRate.Types.Rated, 16, "RET", 4, 1, "MX");
				var cc10 = CreateChargeCode(taxRate);

				var job = CreateJob("Z00001011", TestObjectCreator.Debtor, true);
				TestObjectCreator.SetExchangeRate(job, TestObjectCreator.USD, 20.88m, TestObjectCreator.Debtor.PK, ExchangeRateOrgTypeEnum.Debtor);
				TestObjectCreator.SetExchangeRate(job, TestObjectCreator.EUR, 21.77m, TestObjectCreator.Debtor.PK, ExchangeRateOrgTypeEnum.Debtor);

				var charge = CreateJobCharge(job, cc10, "Test", TestObjectCreator.EUR, 1890.08M, TestObjectCreator.Debtor);
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				AssertBillInInvoiceCurrency(charge, expectedOverseasTaxAmount, expectedLocalTaxAmount);

				charge.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice;
				AssertBillInInvoiceCurrency(charge, expectedOverseasTaxAmount, expectedLocalTaxAmount);

				charge.JR_InvoiceType = InvoiceTypesList.Codes.InvoicePerTaxCode;
				AssertBillInInvoiceCurrency(charge, expectedOverseasTaxAmount, expectedLocalTaxAmount);

				charge.JR_InvoiceType = InvoiceTypesList.Codes.DestinationChargesInvoice;
				AssertBillInInvoiceCurrency(charge, expectedOverseasTaxAmount, expectedLocalTaxAmount);

				var postingCharge = charge as IReceivablesPostingCharge;
				var invoice = (new ChargePoster(Factory)).Post(postingCharge);
				var invoiceLine = invoice.Lines[0];

				AssertEquals("AL_OSTaxAmount", expectedOverseasTaxAmount, invoiceLine.AL_OSTaxAmount);
				AssertEquals("AL_LocalTaxAmount", expectedLocalTaxAmount, invoiceLine.AL_LocalTaxAmount);
			}
		}

		void AssertBillInInvoiceCurrency(Charge charge, ZDecimal expectedOverseasTaxAmount, ZDecimal expectedLocalTaxAmount)
		{
			charge.JR_RX_NKSellInvoiceCurrency = "USD";

			Factory.Save();

			Assert(charge.BillInInvoiceCurrency);
			AssertEquals("JR_OSSellInvoiceGSTAmt", expectedOverseasTaxAmount, charge.JR_OSSellInvoiceGSTAmt);
			AssertEquals("Sell Inv. Tax Amt", expectedOverseasTaxAmount.ToString(), charge.JR_OSSellInvoiceGSTAmt_ForDisplay);
			AssertEquals("JR_SellInvoice_LocalGSTAmount", expectedLocalTaxAmount, charge.JR_SellInvoice_LocalGSTAmount);
		}

		public void TestTaxesAmounts_WhenSellCurrencyIsForeign_InvoiceCurrencyIsEmpty_InvoiceTypeIsUpdated()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Mexico))
			using (AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;

				var expectedTaxAmount = 305.73M;

				var taxRate = TestObjectCreator.CreateTaxRate("IVAREC", "Gravado Std. 25% of base & 6% Ret.", AccTaxRate.Types.Rated, 4, "RET", 15, 10, "MX");
				var cc10 = CreateChargeCode(taxRate);

				var job = CreateJob("Z00001011", TestObjectCreator.Debtor, true);
				TestObjectCreator.SetExchangeRate(job, TestObjectCreator.USD, 20.88m, TestObjectCreator.Debtor.PK, ExchangeRateOrgTypeEnum.Debtor);

				var charge = CreateJobCharge(job, cc10, "Test", TestObjectCreator.USD, 585.69M, TestObjectCreator.Debtor);
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				AssertBillInLocalCurrency(charge, expectedTaxAmount, null);

				charge.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice;
				AssertBillInLocalCurrency(charge, expectedTaxAmount, null);

				charge.JR_InvoiceType = InvoiceTypesList.Codes.InvoicePerTaxCode;
				AssertBillInLocalCurrency(charge, expectedTaxAmount, null);

				charge.JR_InvoiceType = InvoiceTypesList.Codes.DestinationChargesInvoice;
				AssertBillInLocalCurrency(charge, expectedTaxAmount, null);

				var postingCharge = charge as IReceivablesPostingCharge;
				var invoice = (new ChargePoster(Factory)).Post(postingCharge);
				var invoiceLine = invoice.Lines[0];

				AssertEquals("AL_OSTaxAmount", expectedTaxAmount, invoiceLine.AL_OSTaxAmount);
				AssertEquals("AL_LocalTaxAmount", expectedTaxAmount, invoiceLine.AL_LocalTaxAmount);
			}
		}

		public void TestTaxesAmounts_WhenSellCurrencyIsForeign_InvoiceCurrencyIsLocal_InvoiceTypeIsUpdated()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Mexico))
			using (AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;

				var expectedTaxAmount = 11.95M;

				var taxRate = TestObjectCreator.CreateTaxRate("IVAREC", "Gravado Std. 25% of base & 6% Ret.", AccTaxRate.Types.Rated, 4, "RET", 15, 10, "MX");
				var cc10 = CreateChargeCode(taxRate);

				var job = CreateJob("Z00001011", TestObjectCreator.Debtor, true);
				TestObjectCreator.SetExchangeRate(job, TestObjectCreator.USD, 20.88m, TestObjectCreator.Debtor.PK, ExchangeRateOrgTypeEnum.Debtor);

				var charge = CreateJobCharge(job, cc10, "Test", TestObjectCreator.USD, 22.91M, TestObjectCreator.Debtor);
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				AssertBillInLocalCurrency(charge, expectedTaxAmount, GlbCompany.CurrentCompany.LocalCurrency.Code);

				charge.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice;
				AssertBillInLocalCurrency(charge, expectedTaxAmount, GlbCompany.CurrentCompany.LocalCurrency.Code);

				charge.JR_InvoiceType = InvoiceTypesList.Codes.InvoicePerTaxCode;
				AssertBillInLocalCurrency(charge, expectedTaxAmount, GlbCompany.CurrentCompany.LocalCurrency.Code);

				charge.JR_InvoiceType = InvoiceTypesList.Codes.DestinationChargesInvoice;
				AssertBillInLocalCurrency(charge, expectedTaxAmount, GlbCompany.CurrentCompany.LocalCurrency.Code);

				var postingCharge = charge as IReceivablesPostingCharge;
				var invoice = (new ChargePoster(Factory)).Post(postingCharge);
				var invoiceLine = invoice.Lines[0];

				AssertEquals("AL_OSTaxAmount", expectedTaxAmount, invoiceLine.AL_OSTaxAmount);
				AssertEquals("AL_LocalTaxAmount", expectedTaxAmount, invoiceLine.AL_LocalTaxAmount);
			}
		}

		void AssertBillInLocalCurrency(Charge charge, ZDecimal expectedTaxAmount, ZString? sellInvoiceCurrency)
		{
			charge.JR_RX_NKSellInvoiceCurrency = sellInvoiceCurrency.GetValueOrDefault();

			Factory.Save();

			Assert(charge.BillInLocalCurrency);

			AssertEquals("JR_OSSellInvoiceGSTAmt", 0M, charge.JR_OSSellInvoiceGSTAmt);
			AssertEquals("Sell Inv. Tax Amt", ZString.Empty, charge.JR_OSSellInvoiceGSTAmt_ForDisplay);
			AssertEquals("JR_SellInvoice_LocalGSTAmount", 0M, charge.JR_SellInvoice_LocalGSTAmount);
			AssertEquals("JR_Sell_LocalGSTAmount", expectedTaxAmount, charge.JR_Sell_LocalGSTAmount);
		}

		#region Create Business

		Job CreateJob(ZString jobNumber, OrgHeader localClient, bool billLocalClientInLocalCurrency)
		{
			Job job = Factory.NewJobForTesting<Job>();
			job.JH_JobNum = jobNumber;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.LocalChargesPK = localClient.PK;

			return job;
		}

		AccChargeCode CreateChargeCode(AccTaxRate taxRate)
		{
			var chargeCode = TestObjectCreator.CreateChargeCode("CC10", "Test", Constants.ChargeType.Revenue, 0, taxRate, null);
			chargeCode.Company.GC_IsReciprocal = true;

			return chargeCode;
		}

		Charge CreateJobCharge(Job parentJob, AccChargeCode chargeCode, ZString desc, RefCurrency sellCurrency, ZDecimal oSSellAmt, OrgHeader debtor)
		{
			Charge charge = parentJob.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_Desc = desc;

			charge.JR_OH_SellAccount = debtor.PK;

			charge.JR_RX_NKSellCurrency = sellCurrency.RX_Code;
			charge.JR_OSSellAmt = oSSellAmt;

			return charge;
		}

		#endregion

		#endregion

		public void TestDeletedSellInvoiceExchangeRate_DoesNotCauseException_WhenValidated()
		{
			var shipment = TestObjectCreator.CreateShipment("S1");
			var job = TestObjectCreator.CreateJob(shipment, false);

			var chargeFRT = job.Charges.AddNew();
			chargeFRT.JR_AC = TestObjectCreator.FRT.PK;
			chargeFRT.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
			chargeFRT.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			chargeFRT.JR_OSSellAmt = 100m;
			chargeFRT.JR_OSCostAmt = 200m;
			chargeFRT.JR_RX_NKCostCurrency = TestObjectCreator.AUD.RX_Code;
			chargeFRT.JR_RX_NKSellCurrency = TestObjectCreator.EUR.RX_Code;

			var chargeCC1 = job.Charges.AddNew();
			chargeCC1.JR_AC = TestObjectCreator.CC1.PK;
			chargeCC1.JR_OH_SellAccount = TestObjectCreator.Debtor1.PK;
			chargeCC1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			chargeCC1.JR_OSSellAmt = 150m;
			chargeCC1.JR_OSCostAmt = 250m;
			chargeCC1.JR_RX_NKCostCurrency = TestObjectCreator.AUD.RX_Code;
			chargeCC1.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;

			AssertEquals("Precondition: one exchange rate", 1, job.ExchangeRates.Count);
			AssertNull("Precondition: no sell exchange rate for FRT line", chargeFRT.SellInvoiceExchangeRate);
			AssertNull("Precondition: no sell exchange rate for CC1 line", chargeCC1.SellInvoiceExchangeRate);

			Factory.Save();

			// Printing an AWB document can add a new exchange rate (depending on registry). See WI00372844 for more details.
			var newExchangeRateInFactory1 = job.ExchangeRates.AddNew();
			newExchangeRateInFactory1.JF_BaseRate = 1.3m;
			newExchangeRateInFactory1.JF_OrgType = ExchangeRateOrgTypeEnum.Debtor.ToCode();
			newExchangeRateInFactory1.JF_OH_Org = TestObjectCreator.Debtor1.PK;
			newExchangeRateInFactory1.JF_RX_NKRateCurrency = TestObjectCreator.USD.Code;

			Factory.SetContext(BusinessContext.ConvertingAmountsForExportAWBHeader);
			newExchangeRateInFactory1.ShouldDeleteDuplicateExchangeRateBeforeSave = true;

			chargeCC1.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
			AssertNotNull("Precondition: after the sell invoice currency is added the exchange rate for CC1 line should be available", chargeCC1.SellInvoiceExchangeRate);
			AssertEquals("Precondition: CC1 line exchange rate can only be from factory1; no other matching rate exists.", newExchangeRateInFactory1.PK, chargeCC1.SellInvoiceExchangeRate.ExchangeRatePk);

			// An exchange rate in database with identical data should be used in preference to the AWB rate.
			var factory2 = Factory.CreateNewFactory();
			factory2.RefreshEnabled = false;
			var jobInFactory2 = factory2.Load<Job>(job.PK);
			var newExchangeRateInFactory2 = jobInFactory2.ExchangeRates.AddNew();
			newExchangeRateInFactory2.JF_BaseRate = 1.2m;
			newExchangeRateInFactory2.JF_OrgType = ExchangeRateOrgTypeEnum.Debtor.ToCode();
			newExchangeRateInFactory2.JF_OH_Org = TestObjectCreator.Debtor1.PK;
			newExchangeRateInFactory2.JF_RX_NKRateCurrency = TestObjectCreator.USD.Code;

			factory2.Save();
			AssertEquals("Precondition: CC1 line exchange rate remains from factory1, as it is unaware of the new exchange rate.", newExchangeRateInFactory1.PK, chargeCC1.SellInvoiceExchangeRate.ExchangeRatePk);

			Factory.RefreshEnabled = false;
			AssertEquals("Data refresh bus must be disabled for this issue to occur", false, Factory.RefreshEnabled);
			Factory.Save();
			Assert(newExchangeRateInFactory1.IsDeleted);
			AssertEquals($"Even when data refresh bus is disabled, the exchange rate in factory2 ({newExchangeRateInFactory2.PK}) should be reloaded; the deleted factory1 rate ({newExchangeRateInFactory1.PK}) should not be referenced.", newExchangeRateInFactory2.PK, chargeCC1.SellInvoiceExchangeRate.ExchangeRatePk);
			AssertEquals("After refresh, the Base Rate from database should be used.", newExchangeRateInFactory2.JF_BaseRate, chargeCC1.SellInvoiceExchangeRate.Rate);

			CombineAssertions("Precondition: conditions required to access SellInvoiceExchangeRate.Rate", () =>
			{
				Assert(chargeCC1.BillInInvoiceCurrency);
				Assert(!chargeCC1.IsRevenuePosted);
				Assert(!chargeCC1.BIllInInvoiceCurrencySameAsSellCurrency);
				AssertNotNull(chargeCC1.SellInvoiceExchangeRate);
			});

			AssertNoExceptionThrown("After exchange rate has been deleted, validation should succeed. Deleted exchange rate from factory1 must not be accessed.", () => chargeCC1.Validation.ValidateAll());
		}

		#endregion

		#region Captions

		public void TestGetJR_JH_InternalJobCaption()
		{
			var consol = TestObjectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
			using (var job = TestObjectCreator.CreateJob(consol))
			{
				Assert("Precondition: this job is gateway", job.IsGatewayBillingJob());
				var caption = Charge.GetJR_JH_InternalJobCaption(job);
				AssertEquals("Internal Job", caption.Caption);
				AssertEquals(@"Please enter or select an internal job related to this charge. To record gateway revenue as cost on a specific job, enter or select this job number.
To apportion gateway revenue as cost on multiple shipments attached to this consol, set Internal Job to the current consol number and review the Gateway Sell Apportionment tab.", caption.FullDescription);

				Assert("Precondition: this job is not gateway", !TestObjectCreator.Job1.IsGatewayBillingJob());
				caption = Charge.GetJR_JH_InternalJobCaption(TestObjectCreator.Job1);
				AssertNull(caption);
			}
		}

		#endregion

		#region Post Charge with Orgnisation Proxy

		public void TestHasValidDataForCostPosting_SkipAutoJRJ_JobCharge()
		{
			TestObjectCreator.PrepareAutoJRJTestEnvironment(false);

			var shipment = TestObjectCreator.CreateShipment("S00000001");
			var job = TestObjectCreator.CreateJob(shipment);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 190m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 0m, null);
			charge.JR_APInvoiceNum = "INV000001";
			charge.JR_APInvoiceDate = ZDateTime.Today;
			Factory.Save();

			AssertAutoJRJBehaviorForCharge(charge, () => charge.HasValidDataForCostPosting);
		}

		public void TestHasValidDataForRevenuePosting_SkipAutoJRJ_JobCharge()
		{
			TestObjectCreator.PrepareAutoJRJTestEnvironment(false);

			var shipment = TestObjectCreator.CreateShipment("S00000001");
			var job = TestObjectCreator.CreateJob(shipment);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 0m, null, TestObjectCreator.AUD, 190m, TestObjectCreator.AALSHI);
			Factory.Save();

			AssertAutoJRJBehaviorForCharge(charge, () => charge.HasValidDataForRevenuePosting);
		}

		public void TestHasValidDataForCostPosting_SkipAutoJRJ_ConsolApportionedCharge()
		{
			TestObjectCreator.PrepareAutoJRJTestEnvironment(false);

			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S00000001", consol);
			var job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			var apportionList = new ApportionmentListing(Factory, consol);
			var consolCost = apportionList.CostsCollection.TryAddNew();
			consolCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			consolCost.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
			consolCost.E6_LocalCostAmount = 190m;
			consolCost.E6_InvoiceNum = "INV000001";
			consolCost.E6_InvoiceDate = ZDateTime.Today;
			Factory.Save();

			var query = new ZDBOnlyQuery(typeof(Charge));
			query.AddToFilter(JobChargeSchema.JR_E6, consolCost.PK);
			var charges = Factory.Load<Charge>(query);
			AssertEquals("Should only created one apportioned charge", 1, charges.Length);

			var charge = charges[0];
			AssertAutoJRJBehaviorForCharge(charge, () => charge.HasValidDataForCostPosting);
		}

		void AssertAutoJRJBehaviorForCharge(Charge charge, Func<bool> behaviorToAssert)
		{
			TestObjectCreator.AALSHI.PrimaryRegistrationNumber.Number = "73004700411";
			TestObjectCreator.ABIGAS.PrimaryRegistrationNumber.Number = "73004700499";

			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			AssertEquals("Auto JRJ enabled, JRJ Tax registration number disabled, and creditor/debtor have different registration numbers", false, behaviorToAssert());

			AutoJRJRegistryStatusHelper.SetAutoJRJWithTaxRegistrationNumberEnabled_ForTestOnly();

			AssertEquals("Auto JRJ enabled, JRJ Tax registration number enabled, and creditor/debtor have different registration numbers", true, behaviorToAssert());

			AutoJRJRegistryStatusHelper.SetAutoJRJDisabled_ForTestOnly();

			AssertEquals("Auto JRJ disabled, JRJ Tax registration number disabled, and creditor/debtor have different registration numbers", true, behaviorToAssert());

			TestObjectCreator.AALSHI.PrimaryRegistrationNumber.Number = "73004700400";
			TestObjectCreator.ABIGAS.PrimaryRegistrationNumber.Number = "73004700400";

			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			AssertEquals("Auto JRJ enabled, JRJ Tax registration number disabled, and creditor/debtor share the same registration number", false, behaviorToAssert());

			AutoJRJRegistryStatusHelper.SetAutoJRJWithTaxRegistrationNumberEnabled_ForTestOnly();

			AssertEquals("Auto JRJ enabled, JRJ Tax registration number enabled, and creditor/debtor share the same registration number", false, behaviorToAssert());

			AutoJRJRegistryStatusHelper.SetAutoJRJDisabled_ForTestOnly();

			AssertEquals("Auto JRJ disabled, JRJ Tax registration number disabled, and creditor/debtor share the same registration number", true, behaviorToAssert());
		}

		#endregion

		[SuspendCriticalValidation]
		public void TestTaxTransactionsLinkedToJobChargeForDisplay()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var arInvoice = testObjectCreator.CreateInvoice(typeof(ARInvoice), "INV1", organisation: org);
			var arInvoiceLine = testObjectCreator.CreateInvoiceLine(arInvoice, 100);

			var taxRecord1 = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord1.ATT_AH = arInvoice.PK;
			var taxLinePivot1 = Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxRecord1.PK))[0];
			taxLinePivot1.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(arInvoiceLine));

			Charge chrg = Factory.NewWithValidTestData<Charge>();
			arInvoiceLine.AL_GovtChargeCode = chrg.ChargeCode.AC_Code;
			chrg.JR_AL_ARLine = arInvoiceLine.PK;
			chrg.JR_OSSellAmt = 100;
			Factory.Save();

			AssertEquals(1, chrg.TaxTransactionsLinkedToJobChargeForDisplay.TaxTransactionsLinkedToJobCharge.Count);

			var apInvoice = testObjectCreator.CreateInvoice(typeof(APInvoice), "INV2", organisation: org);
			var apInvoiceLine = testObjectCreator.CreateInvoiceLine(apInvoice, 100);

			var taxRecord3 = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord3.ATT_AH = apInvoice.PK;
			var taxLinePivot3 = Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxRecord3.PK))[0];
			taxLinePivot3.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(apInvoiceLine));

			chrg.JR_OSCostAmt = 100;
			chrg.JR_AL_APLine = apInvoiceLine.PK;
			Factory.Save();

			AssertEquals(2, chrg.TaxTransactionsLinkedToJobChargeForDisplay.TaxTransactionsLinkedToJobCharge.Count);
		}

		public void TestJR_LocalSellAmt_JobChargeLocalSellAmountNotMatchingSignOfOsSellAmount()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var charge = Factory.New<Charge>();
			charge.JR_JH = job.PK;

			charge.SetContext(BusinessContext.JobChargeAfterOnSaving);

			AssertContains("Precondition: GetInfoJobChargeOsSellAmountNotMatchingSignOfLocalSellAmount() first time", "There is no data collected for this PK", GetInfoJobChargeOsSellAmountNotMatchingSignOfLocalSellAmount());

			AssertEquals(ZDecimal.Zero, charge.JR_LocalSellAmt);
			AssertContains("Should not report error when job charge Local sell amount is 0", "There is no data collected for this PK", GetInfoJobChargeOsSellAmountNotMatchingSignOfLocalSellAmount());

			AssertEquals(ZDecimal.Zero, charge.JR_OSSellAmt);
			AssertContains("Should not report error when job charge OS sell amount is 0", "There is no data collected for this PK", GetInfoJobChargeOsSellAmountNotMatchingSignOfLocalSellAmount());

			charge.JR_LocalSellAmt = 50m;
			AssertNotEquals("Precondition: jobCharge.JR_LocalSellAmt", 50, charge.JR_LocalSellAmt);
			AssertNotEquals("Precondition: jobCharge.JR_OSSellAmt", 50, charge.JR_OSSellAmt);

			AssertEquals(Math.Sign(charge.JR_LocalSellAmt), Math.Sign(charge.JR_OSSellAmt));
			AssertContains("Should not report error when job charge OS and Local Amounts have same signs", "There is no data collected for this PK", GetInfoJobChargeOsSellAmountNotMatchingSignOfLocalSellAmount());

			using (charge.Calculations.SuspendCalculations())
			{
				charge.JR_LocalSellAmt = -100m;
				AssertNotEquals("Precondition: Signs of Job Charge OS and Local Sell Amounts", Math.Sign(charge.JR_LocalSellAmt), Math.Sign(charge.JR_OSSellAmt));

				var actualInfo = GetInfoJobChargeOsSellAmountNotMatchingSignOfLocalSellAmount();
				var expectedInfo =
					$@"
JobChargeOsSellAmountNotMatchingSignOfLocalSellAmount:
{JobChargeSchema.JR_LocalSellAmt.Name} has been changed from 50 to -100.
";
				AssertContains("Should report an error as the OS and Local Amounts signs not matching", expectedInfo, actualInfo);

				charge.RemoveContext(BusinessContext.JobChargeAfterOnSaving);
			}

			string GetInfoJobChargeOsSellAmountNotMatchingSignOfLocalSellAmount() => CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOsSellAmountNotMatchingSignOfLocalSellAmount);
		}

		public void TestImplementSpecialDataRefreshPolicyInterface()
		{
			Assert(Factory.NewWithValidTestData<Charge>() is IShouldSkipDataRefreshUpdateForDeletedSubscriber);
		}

		public void TestSet_JR_AC_ShouldNotHaveExceptionWhenDefaultCreditorPKIsZGuidInvalid()
		{
			var mockParent = new Mock<IJobHeaderParent>();
			mockParent.Setup(x => x.Factory).Returns(Factory);
			var mockChargeCreditorDefaulting = mockParent.As<IChargeCreditorDefaulting>();

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.Parent = mockParent.Object;
			var charge = Factory.New<Charge>();
			charge.JR_JH = job.PK;
			Factory.SetContext(BusinessContext.AutoRating);

			Assert(Factory.HasAnyOfContexts(BusinessContextSets.GetDoNotDefaultInvalidCreditorSet()));

			mockChargeCreditorDefaulting.Setup(x => x.DefaultCreditorPK).Returns(ZGuid.Invalid);
			AssertNoExceptionThrown(() => { charge.JR_AC = new ZGuid(); });

			mockChargeCreditorDefaulting.Setup(x => x.DefaultCreditorPK).Returns(ZGuid.Empty);
			AssertNoExceptionThrown(() => { charge.JR_AC = new ZGuid(); });

			mockChargeCreditorDefaulting.Setup(x => x.DefaultCreditorPK).Returns(ZGuid.Missing);
			AssertNoExceptionThrown(() => { charge.JR_AC = new ZGuid(); });

			var validId = new ZGuid(new Guid("7FDF0128-BCF9-4D80-B93E-1CE2AE88F2FD"));
			mockChargeCreditorDefaulting.Setup(x => x.DefaultCreditorPK).Returns(validId);
			AssertNoExceptionThrown(() => { charge.JR_AC = new ZGuid(); });
		}

		public void TestSellExchangeRateShouldUpdateWhenSellInvoiceCurrencyIsForeignAndNotSameAsSellCurrency()
		{
			TestObjectCreator.SetCurrentCompanyReciprocal(true);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvoicePostingExchangeRateOption.Default.Code);

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.BuyRate, 0.55m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.SellRate, 0.65m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.EUR, Constants.ExchangeRateTypes.Code.BuyRate, 0.75m);

			var exRateARConfig1 = TestObjectCreator.LocalClient.CompanyData.AccARExchangeRateConfigurations.AddNew();
			exRateARConfig1.JCE_JobType = "ALL";
			exRateARConfig1.JCE_ServiceDirection = "ALL";
			exRateARConfig1.JCE_TransportMode = "ALL";
			exRateARConfig1.JCE_InvoiceCurrencyType = Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign;
			exRateARConfig1.GetCurrencyConfig(ZString.Empty, ZDate.Empty).JCT_ExRateType = Constants.ExchangeRateTypes.Code.BuyRate;

			var exRateARConfig2 = TestObjectCreator.LocalClient.CompanyData.AccARExchangeRateConfigurations.AddNew();
			exRateARConfig2.JCE_JobType = "ALL";
			exRateARConfig2.JCE_ServiceDirection = "ALL";
			exRateARConfig2.JCE_TransportMode = "ALL";
			exRateARConfig2.JCE_InvoiceCurrencyType = Constants.InvoicePostingExchangeRateCurrencyType.Code.Local;
			exRateARConfig2.GetCurrencyConfig(ZString.Empty, ZDate.Empty).JCT_ExRateType = Constants.ExchangeRateTypes.Code.SellRate;

			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S0004", saveIt: true);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			job.PlugInData = shipment;

			using (AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.SetTemporaryValue(job.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "Revenue Transaction Test 1", null, 0m, null, null, 0M, TestObjectCreator.LocalClient);
				charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
				charge.SetContext(BusinessContext.PostingReceivableCharges);
				AssertEquals("JR_OSSellExRate will use the sell rate of USD", 0.65m, charge.RevenueExchangeRate.Rate);

				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.RX_Code;
				AssertEquals("JR_OSSellInvoiceExRate will use the buy rate of EUR", 0.75m, charge.JR_OSSellInvoiceExRate);
				AssertEquals("RevenueExchangeRate will use the buy rate of USD", 0.55m, charge.RevenueExchangeRate.Rate);
			}
		}

	#region Implementation

	protected override BaseCharge GetChargeWithValidData(Job invoicingJob, AccChargeCode chargeCode)
		{
			return TestObjectCreator.CreateCharge(invoicingJob, chargeCode);
		}

		public override void TestOnLoadedDoesNotCreateOrLoadOtherObjects()
		{
			Assert(true);   // Work Item W00024746
		}

		protected override void AssertReadOnlyOnRevenueRelatedProperties(bool expectedReadOnly)
		{
			base.AssertReadOnlyOnRevenueRelatedProperties(expectedReadOnly);

			AssertEquals("JR_InvoiceType", expectedReadOnly, TestCharge.JR_InvoiceTypeInfo.ReadOnly);
		}

		protected override void AssertReadOnlyOnRevenueAndCostRelatedProperties(bool expectedReadOnly)
		{
			base.AssertReadOnlyOnRevenueAndCostRelatedProperties(expectedReadOnly);

			AssertEquals("JR_IsIncludedInProfitShare", expectedReadOnly, TestCharge.JR_IsIncludedInProfitShareInfo.ReadOnly);
			AssertEquals("JR_AgentDeclaredCostAmt", expectedReadOnly, TestCharge.JR_AgentDeclaredCostAmtInfo.ReadOnly);
			AssertEquals("JR_AgentDeclaredCostAmtLocal", expectedReadOnly, TestCharge.JR_AgentDeclaredCostAmtLocalInfo.ReadOnly);
			AssertEquals("JR_AgentDeclaredSellAmt", expectedReadOnly, TestCharge.JR_AgentDeclaredSellAmtInfo.ReadOnly);
			AssertEquals("JR_AgentDeclaredSellAmtLocal", expectedReadOnly, TestCharge.JR_AgentDeclaredSellAmtLocalInfo.ReadOnly);
		}

		#endregion
	}
}
