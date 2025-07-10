using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting.Testing
{
	public class OverseasAgentChargesPosterTest : TestCaseWithFactory
	{
		public void TestProcessMoreThanOneCompany()
		{
			#region Setup

			ZString transportMode = "AIR";
			ZString origin = "AUSYD";
			ZString destination = "USLAX";

			SetupAgent();
			Creator.SetupProfitShareRelationship(GlbCompany.CurrentCompany.OrgProxy, Creator.Agent, 60m, 40m, origin, destination, transportMode);

			Factory.Save();

			SetupConsolAndShipments(transportMode, origin, destination, Creator.Agent);

			Factory.Save();

			JobCollection jobs = new JobCollection(Factory);
			jobs.Load();

			Job shipment1Job = jobs.GetJobForOperationsPlugin(Shipment1);
			Job shipment2Job = jobs.GetJobForOperationsPlugin(Shipment2);

			shipment1Job.AddCurrency(Creator.USD, ExchangeRateValidLedgerEnum.None);
			shipment2Job.AddCurrency(Creator.USD, ExchangeRateValidLedgerEnum.None);

			Charge shipment1Charge = shipment1Job.Charges.AddNew();
			Charge shipment1AgentCharge1 = shipment1Job.Charges.AddNew();
			Charge shipment2Charge = shipment2Job.Charges.AddNew();
			Charge shipment2AgentCharge1 = shipment2Job.Charges.AddNew();

			foreach (var exRate in shipment1Job.ExchangeRates.Cast<ExchangeRate>().Where(r => r.JF_RX_NKRateCurrency == Creator.USD.RX_Code))
			{
				exRate.JF_BaseRate = 0.777m;
				exRate.JF_IsTransformed = true;
			}

			foreach (var exRate in shipment2Job.ExchangeRates.Cast<ExchangeRate>().Where(r => r.JF_RX_NKRateCurrency == Creator.USD.RX_Code))
			{
				exRate.JF_BaseRate = 0.778m;
				exRate.JF_IsTransformed = true;
			}

			SetupShipmentCharge(shipment1Charge, 80m, 180m, Creator.FRT.PK);
			SetupShipmentCharge(shipment1AgentCharge1, 160m, 200m, Creator.CC1.PK);

			SetupShipmentCharge(shipment2Charge, 100m, 200m, Creator.FRT.PK);
			SetupShipmentCharge(shipment2AgentCharge1, 160m, 280m, Creator.CC1.PK);

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, Creator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				SetSinglePeriod();

				SetupAgent();
				Factory.Save();

				var otherCompanyFactory = new BusinessObjectFactory();
				var shipment1 = otherCompanyFactory.Load<ForwardingShipment>(Shipment1.PK);
				var shipment2 = otherCompanyFactory.Load<ForwardingShipment>(Shipment2.PK);

				var job1 = Job.CreateWithMutex(otherCompanyFactory, shipment1);
				var job2 = Job.CreateWithMutex(otherCompanyFactory, shipment2);

				var rate1 = job1.AddCurrency(Creator.USD, 0.777m, ExchangeRateValidLedgerEnum.None);
				rate1.JF_IsTransformed = true;
				var rate2 = job2.AddCurrency(Creator.USD, 0.777m, ExchangeRateValidLedgerEnum.None);
				rate2.JF_IsTransformed = true;

				var shipment1AgentCharge = job1.Charges.AddNew();
				var shipment2AgentCharge = job2.Charges.AddNew();

				SetupShipmentCharge(shipment1AgentCharge, 80m, 120m, Creator.CC1.PK);
				SetupShipmentCharge(shipment2AgentCharge, 110m, 190m, Creator.CC1.PK);

				otherCompanyFactory.Save();
			}

			#endregion

			var poster = new OverseasAgentChargesPoster(Consol);
			var notify = new NotificationBuffer();
			poster.Process(notify);
			string expectedNotify = @"Started posting Agent Charges on the DEM - Demo Company
Finished posting Agent Charges on the DEM - Demo Company
Started posting Agent Charges on the EDI - Eagle Datamation International
Finished posting Agent Charges on the EDI - Eagle Datamation International
";
			AssertEquals("Profit Share was posted for two Companies", expectedNotify, notify.AsString);

			var currentCompanyInvoices = Factory.Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.AH_GC, Env.CurrentCompany.PK));
			var otherCompanyInvoices = Factory.Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.AH_GC, Creator.NonCurrentCompany.PK));

			AssertEquals(5, currentCompanyInvoices.Length);
			AssertEquals(6, otherCompanyInvoices.Length);
		}

		public void TestProcessWithIncorrectRegistrySetup()
		{
			#region Setup

			ZString transportMode = "AIR";
			ZString origin = "AUSYD";
			ZString destination = "USLAX";

			SetupAgent();
			Creator.SetupProfitShareRelationship(GlbCompany.CurrentCompany.OrgProxy, Creator.Agent, 60m, 40m, origin, destination, transportMode);

			Factory.Save();

			SetupConsolAndShipments(transportMode, origin, destination, Creator.Agent);

			Factory.Save();

			JobCollection jobs = new JobCollection(Factory);
			jobs.Load();

			Job shipment1Job = jobs.GetJobForOperationsPlugin(Shipment1);
			Job shipment2Job = jobs.GetJobForOperationsPlugin(Shipment2);

			shipment1Job.AddCurrency(Creator.USD, ExchangeRateValidLedgerEnum.None);
			shipment2Job.AddCurrency(Creator.USD, ExchangeRateValidLedgerEnum.None);

			Charge shipment1Charge = shipment1Job.Charges.AddNew();
			Charge shipment1AgentCharge = shipment1Job.Charges.AddNew();
			Charge shipment2Charge = shipment2Job.Charges.AddNew();
			Charge shipment2AgentCharge = shipment2Job.Charges.AddNew();

			foreach (var exRate in shipment1Job.ExchangeRates.Cast<ExchangeRate>().Where(r => r.JF_RX_NKRateCurrency == Creator.USD.RX_Code))
			{
				exRate.JF_BaseRate = 0.777m;
			}

			foreach (var exRate in shipment2Job.ExchangeRates.Cast<ExchangeRate>().Where(r => r.JF_RX_NKRateCurrency == Creator.USD.RX_Code))
			{
				exRate.JF_BaseRate = 0.778m;
			}

			SetupShipmentCharge(shipment1Charge, 80m, 180m, Creator.FRT.PK);
			SetupShipmentCharge(shipment1AgentCharge, 160m, 200m, Creator.CC1.PK);

			SetupShipmentCharge(shipment2Charge, 100m, 200m, Creator.FRT.PK);
			SetupShipmentCharge(shipment2AgentCharge, 160m, 280m, Creator.CC1.PK);

			AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);

			Factory.Save();

			#endregion

			var poster = new OverseasAgentChargesPoster(Consol);
			var notify = new NotificationBuffer();

			AssertEquals("Should not have sent email", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			poster.Process(notify);
			string expectedNotify = @"Started posting Agent Charges on the EDI - Eagle Datamation International
Incorrect Registry setting located at Accounting -> Job Invoicing -> Profit Share -> Profit Share Charge Code";
			AssertContains("Registry error was reported", expectedNotify, notify.AsString);

			var postedInvoices = Factory.Load<InvoicingBase>(new ZQuery());
			AssertEquals("Some transactions have been posted", 4, postedInvoices.Length);
			AssertNull("But no Profit Share", postedInvoices.FirstOrDefault(x => x.AH_Desc.StartsWith("PS")));

			AssertEquals("Should have sent email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestProcessWithNothingToPost()
		{
			#region Setup

			ZString transportMode = "AIR";
			ZString origin = "AUSYD";
			ZString destination = "USLAX";

			SetupAgent();
			Factory.Save();

			SetupConsolAndShipments(transportMode, origin, destination, Creator.Agent);
			Factory.Save();

			JobCollection jobs = new JobCollection(Factory);
			jobs.Load();

			Job shipment1Job = jobs.GetJobForOperationsPlugin(Shipment1);
			Job shipment2Job = jobs.GetJobForOperationsPlugin(Shipment2);

			shipment1Job.AddCurrency(Creator.USD, ExchangeRateValidLedgerEnum.None);
			shipment2Job.AddCurrency(Creator.USD, ExchangeRateValidLedgerEnum.None);

			foreach (var exRate in shipment1Job.ExchangeRates.Cast<ExchangeRate>().Where(r => r.JF_RX_NKRateCurrency == Creator.USD.RX_Code))
			{
				if (exRate.JF_BaseRate == 0)
				{
					exRate.JF_BaseRate = 1m;
				}
				exRate.JF_IsTransformed = true;
			}

			foreach (var exRate in shipment2Job.ExchangeRates.Cast<ExchangeRate>().Where(r => r.JF_RX_NKRateCurrency == Creator.USD.RX_Code))
			{
				if (exRate.JF_BaseRate == 0)
				{
					exRate.JF_BaseRate = 1m;
				}
				exRate.JF_IsTransformed = true;
			}

			Charge shipment1Charge = shipment1Job.Charges.AddNew();
			Charge shipment2Charge = shipment2Job.Charges.AddNew();

			SetupShipmentCharge(shipment1Charge, 80m, 180m, Creator.FRT.PK);
			SetupShipmentCharge(shipment2Charge, 100m, 200m, Creator.FRT.PK);
			shipment1Charge.JR_OSSellAmt = 0m;
			shipment2Charge.JR_OSSellAmt = 0m;

			Consol.GetConsolFreightCosts().First().Delete();

			Factory.Save();

			#endregion

			var poster = new OverseasAgentChargesPoster(Consol);
			var notify = new NotificationBuffer();

			AssertEquals("Should not have sent email", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			poster.Process(notify);
			AssertContains("Nothing to post", string.Empty, notify.AsString);

			var postedInvoices = Factory.Load<InvoicingBase>(new ZQuery());
			AssertEquals("No transactions should be posted", 0, postedInvoices.Length);

			AssertEquals("No email should be sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestProcessWithJobOnHold()
		{
			#region Setup

			ZString transportMode = "AIR";
			ZString origin = "AUSYD";
			ZString destination = "USLAX";

			SetupAgent();
			Creator.SetupProfitShareRelationship(GlbCompany.CurrentCompany.OrgProxy, Creator.Agent, 60m, 40m, origin, destination, transportMode);

			Factory.Save();

			SetupConsolAndShipments(transportMode, origin, destination, Creator.Agent);

			Factory.Save();

			JobCollection jobs = new JobCollection(Factory);
			jobs.Load();

			Job shipment1Job = jobs.GetJobForOperationsPlugin(Shipment1);
			Job shipment2Job = jobs.GetJobForOperationsPlugin(Shipment2);

			shipment1Job.AddCurrency(Creator.USD, ExchangeRateValidLedgerEnum.None);
			shipment2Job.AddCurrency(Creator.USD, ExchangeRateValidLedgerEnum.None);

			Charge shipment1Charge = shipment1Job.Charges.AddNew();
			Charge shipment1AgentCharge = shipment1Job.Charges.AddNew();
			Charge shipment2Charge = shipment2Job.Charges.AddNew();
			Charge shipment2AgentCharge = shipment2Job.Charges.AddNew();

			foreach (var exRate in shipment1Job.ExchangeRates.Cast<ExchangeRate>().Where(r => r.JF_RX_NKRateCurrency == Creator.USD.RX_Code))
			{
				exRate.JF_BaseRate = 0.777m;
			}

			foreach (var exRate in shipment2Job.ExchangeRates.Cast<ExchangeRate>().Where(r => r.JF_RX_NKRateCurrency == Creator.USD.RX_Code))
			{
				exRate.JF_BaseRate = 0.778m;
			}

			SetupShipmentCharge(shipment1Charge, 80m, 180m, Creator.FRT.PK);
			SetupShipmentCharge(shipment1AgentCharge, 160m, 200m, Creator.CC1.PK);

			SetupShipmentCharge(shipment2Charge, 100m, 200m, Creator.FRT.PK);
			SetupShipmentCharge(shipment2AgentCharge, 160m, 280m, Creator.CC1.PK);

			shipment1Job.JH_Status = JobHeaderStatus.WorkOnHold.Code;

			Factory.Save();

			#endregion

			var poster = new OverseasAgentChargesPoster(Consol);
			var notify = new NotificationBuffer();

			AssertEquals("Should not have sent email", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			poster.Process(notify);
			string expectedNotify = @"Started posting Agent Charges on the EDI - Eagle Datamation International
Charges from the following job(s) cannot be posted because S00001000 is on hold.";
			AssertContains("Job on Hold was reported", expectedNotify, notify.AsString);

			var postedInvoices = Factory.Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef, "S00001000").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			AssertEquals("No transactions should be posted for the S00001000", 0, postedInvoices.Length);

			AssertEquals("Should have sent email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestProcess()
		{
			#region Setup

			SetupAgent();

			ZString transportMode = "AIR";
			ZString origin = "AUSYD";
			ZString destination = "USLAX";

			Creator.SetupProfitShareRelationship(GlbCompany.CurrentCompany.OrgProxy, Creator.Agent, 60m, 40m, origin, destination, transportMode);

			Factory.Save();

			SetupConsolAndShipments(transportMode, origin, destination, Creator.Agent);

			Factory.Save();

			var jobs = new JobCollection(Factory);
			jobs.Load();

			var shipment1Job = jobs.GetJobForOperationsPlugin(Shipment1);
			var shipment2Job = jobs.GetJobForOperationsPlugin(Shipment2);

			var shipment1Charge = shipment1Job.Charges.AddNew();
			var shipment1AgentCharge = shipment1Job.Charges.AddNew();
			var shipment2Charge = shipment2Job.Charges.AddNew();
			var shipment2AgentCharge = shipment2Job.Charges.AddNew();

			SetupShipmentCharge(shipment1Charge, 80m, 180m, Creator.FRT.PK, 100m, 150m);
			SetupShipmentCharge(shipment1AgentCharge, 160m, 280m, Creator.CC1.PK, 160m, 200m);
			SetupShipmentCharge(shipment2Charge, 100m, 200m, Creator.FRT.PK, 120m, 180m);
			SetupShipmentCharge(shipment2AgentCharge, 160m, 280m, Creator.CC1.PK, 160m, 200m);

			foreach (var exRate in shipment1Job.ExchangeRates.Cast<ExchangeRate>().Where(r => r.JF_RX_NKRateCurrency == Creator.USD.RX_Code))
			{
				exRate.JF_BaseRate = 0.777m;
			}

			foreach (var exRate in shipment2Job.ExchangeRates.Cast<ExchangeRate>().Where(r => r.JF_RX_NKRateCurrency == Creator.USD.RX_Code))
			{
				exRate.JF_BaseRate = 0.778m;
			}

			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Factory.Save();

			#endregion

			var poster = new OverseasAgentChargesPoster(Consol);
			var notify = new NotificationBuffer();
			poster.Process(notify);

			CombineAssertions(() =>
			{
				string expectedNotify = @"Started posting Agent Charges on the EDI - Eagle Datamation International
Finished posting Agent Charges on the EDI - Eagle Datamation International
";
				AssertEquals(expectedNotify, notify.AsString);

				var apInvoices = Factory.Load<InvoicingBase>(GetInvoicesAndCreditNotesQuery(LedgerTypes.AccountsPayable));
				AssertEquals("Should be 1 invoice", 1, apInvoices.Length);

				var apInvoice = (APInvoice)apInvoices[0];
				AssertEquals("Invoice should be for zero", 0m, apInvoice.AH_OSExTaxAmount);
				AssertEquals("Invoice should have 4 lines", 4, apInvoice.Lines.Count);

				var postedInvoices = new InvoicingBaseCollection(Factory, GetInvoicesAndCreditNotesQuery(LedgerTypes.AccountsReceivable));
				postedInvoices.Load();

				AssertEquals("Should have posted 2 AR invoices and 5 Credit Notes", 7, postedInvoices.Count);

				var found = postedInvoices.Find(new ZQuery(AccTransactionHeaderSchema.AH_OSTotal, -250m));
				Assert("Found Invoice for -250", found != null && found.Any());
				var agentCreditNote = (ARCreditNote)found[0];
				AssertEquals("ExchangeRate", 0.775m, agentCreditNote.AH_ExchangeRate);
				AssertEquals("AH_OSExTaxAmount", 250m, agentCreditNote.AH_OSExTaxAmount);
				AssertEquals("Should have 2 lines", 2, agentCreditNote.Lines.Count);

				found = postedInvoices.Find(new ZQuery(AccTransactionHeaderSchema.AH_OSTotal, -46.17m));
				Assert("Found CreditNote for 46.17", found != null && found.Any());
				agentCreditNote = (ARCreditNote)found[0];
				AssertEquals("ExchangeRate", 1m, agentCreditNote.AH_ExchangeRate);
				AssertEquals("AH_OSExTaxAmount", 46.17m, agentCreditNote.AH_OSExTaxAmount);
				AssertEquals("Should have 1 line", 1, agentCreditNote.Lines.Count);

				found = postedInvoices.Find(new ZQuery(AccTransactionHeaderSchema.AH_OSTotal, 508m));
				Assert("Found Invoice for 508", found != null && found.Any());

				var agentInvoice = (ARInvoice)found[0];
				AssertEquals("ExchangeRate", 0.775m, agentInvoice.AH_ExchangeRate);
				AssertEquals("AH_OSExTaxAmount", 480m, agentInvoice.AH_OSExTaxAmount);
				AssertEquals("Should have 2 lines", 2, agentInvoice.Lines.Count);

				found = postedInvoices.Find(new ZQuery(AccTransactionHeaderSchema.AH_OSTotal, 488m));
				Assert("Found Invoice for 488", found != null && found.Any());
				agentInvoice = (ARInvoice)found[0];
				AssertEquals("ExchangeRate", 0.775m, agentInvoice.AH_ExchangeRate);
				AssertEquals("AH_OSExTaxAmount", 460m, agentInvoice.AH_OSExTaxAmount);
				AssertEquals("Should have 2 lines", 2, agentInvoice.Lines.Count);

				found = postedInvoices.Find(new ZQuery(AccTransactionHeaderSchema.AH_OSTotal, -51.16m));
				Assert("Found CreditNote for 51.16", found != null && found.Any());
				agentCreditNote = (ARCreditNote)found[0];
				AssertEquals("ExchangeRate", 1m, agentCreditNote.AH_ExchangeRate);
				AssertEquals("AH_OSExTaxAmount", 51.16m, agentCreditNote.AH_OSExTaxAmount);
				AssertEquals("Should have 1 line", 1, agentCreditNote.Lines.Count);

				AssertEquals("Should not have sent email", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			});
		}

		public void TestProcessNotifyUserWarningMessage()
		{
			#region Setup

			SetupAgent();

			ZString transportMode = "AIR";
			ZString origin = "AUSYD";
			ZString destination = "USLAX";

			Creator.SetupProfitShareRelationship(GlbCompany.CurrentCompany.OrgProxy, Creator.Agent, 60m, 40m, origin, destination, transportMode);

			Factory.Save();

			SetupConsolAndShipments(transportMode, origin, destination, Creator.Agent);

			Factory.Save();

			var jobs = new JobCollection(Factory);
			jobs.Load();

			var shipment1Job = jobs.GetJobForOperationsPlugin(Shipment1);
			var shipment2Job = jobs.GetJobForOperationsPlugin(Shipment2);

			var shipment1Charge = shipment1Job.Charges.AddNew();
			var shipment1AgentCharge = shipment1Job.Charges.AddNew();
			var shipment2Charge = shipment2Job.Charges.AddNew();
			var shipment2AgentCharge = shipment2Job.Charges.AddNew();

			SetupShipmentCharge(shipment1Charge, 80m, 180m, Creator.FRT.PK, 100m, 150m);
			SetupShipmentCharge(shipment1AgentCharge, 160m, 280m, Creator.CC1.PK, 160m, 200m);
			SetupShipmentCharge(shipment2Charge, 100m, 200m, Creator.FRT.PK, 120m, 180m);
			SetupShipmentCharge(shipment2AgentCharge, 160m, 280m, Creator.CC1.PK, 160m, 200m);

			foreach (var exRate in shipment1Job.ExchangeRates.Cast<ExchangeRate>().Where(r => r.JF_RX_NKRateCurrency == Creator.USD.RX_Code))
			{
				exRate.JF_BaseRate = 0.777m;
			}

			foreach (var exRate in shipment2Job.ExchangeRates.Cast<ExchangeRate>().Where(r => r.JF_RX_NKRateCurrency == Creator.USD.RX_Code))
			{
				exRate.JF_BaseRate = 0.778m;
			}

			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Factory.Save();

			#endregion

			var poster = new DummyOverseasAgentChargePoster(Consol);
			var notify = new NotificationBuffer();
			poster.Process(notify);
			string expectedNotify = @"Started posting Agent Charges on the EDI - Eagle Datamation International
this is only for test warning message!
Finished posting Agent Charges on the EDI - Eagle Datamation International
";
			AssertEquals(expectedNotify, notify.AsString);
		}

		ZQuery GetInvoicesAndCreditNotesQuery(string ledgerType)
		{
			var transactionTypes = new[] { TransactionTypes.CreditNote, TransactionTypes.Invoice };

			var query = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, ledgerType);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, transactionTypes);
			query.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

			return query;
		}

		TestObjectCreator Creator;
		ForwardingConsol Consol;
		ForwardingShipment Shipment1;
		ForwardingShipment Shipment2;
		Guid OriginalGroupPK;

		void SetSinglePeriod()
		{
			new AccountingPeriodTestHelper(Factory).SetupSinglePeriod(1, DateTime.Today.AddMonths(-1), DateTime.Today.AddMonths(1));
		}

		Guid SetPostingNotificationGroup()
		{
			var originalGroupPK = AccountingConfigurationRegistry.Instance.JobPostingNotificationGroup.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty);

			var group = Factory.New<GlbGroup>();
			group.Staff.Add(Factory.Load<GlbStaff>(new ZGuid(GlbStaff.CurrentUser.PK)));
			var currentStaffMember = Factory.Load<GlbStaff>(new ZGuid(GlbStaff.CurrentUser.PK));
			currentStaffMember.GS_EmailAddress = "blahblah@whatever.example";
			Factory.Save();
			AccountingConfigurationRegistry.Instance.JobPostingNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			return originalGroupPK;
		}

		void SetupAgent()
		{
			Creator.Agent.CompanyData.OB_IsCreditor = ZBool.True;
			Creator.Agent.CompanyData.OB_IsDebtor = ZBool.True;
			Creator.Agent.CompanyData.SetAPTaxApplicable(ZBool.True);
			Creator.Agent.CompanyData.SetARTaxApplicable(ZBool.True);
			Creator.Agent.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			Creator.Agent.MiscServ.OM_ARWHTApplicable = ZBool.False;

			Creator.Agent.MiscServ.OM_FWBillCollectFeesOnSingleInvoice = ZBool.False;
		}

		void SetupConsolAndShipments(ZString transportMode, ZString origin, ZString destination, OrgHeader agent)
		{
			Consol = Factory.New<ForwardingConsol>();
			Factory.Save();

			Shipment1 = Consol.Shipments.AddNew();
			Shipment1.JS_TransportMode = transportMode;
			Shipment1.JS_RL_NKOrigin = origin;
			Shipment1.JS_RL_NKDestination = destination;
			Shipment1.JS_OH_DeliveryAgent = agent.PK;

			Shipment2 = Consol.Shipments.AddNew();
			Shipment2.JS_TransportMode = transportMode;
			Shipment2.JS_RL_NKOrigin = origin;
			Shipment2.JS_RL_NKDestination = destination;
			Shipment2.JS_OH_DeliveryAgent = agent.PK;

			Consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
			Consol.JK_PrepaidCollect = Enterprise.Core.Constants.PaymentType.Collect;
			Consol.JK_RL_NKLoadPort = origin;
			Consol.JK_RL_NKDischargePort = destination;
			Consol.SetDefaultReceivingForwarderAddress(Creator.Agent);
			Consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);

			ApportionmentListing apps = new ApportionmentListing(Factory, Consol);
			JobConsolCost freightCost = apps.CostsCollection.TryAddNew();
			freightCost.E6_AC_ChargeCode = Creator.FRT.PK;
			freightCost.E6_RX_NKCurrency = Creator.USD.RX_Code;
			freightCost.E6_OH_Creditor = Creator.Agent.PK;
			freightCost.E6_ExchangeRate = 0.775m;
			freightCost.E6_OSCostAmount = 250m;
			freightCost.E6_ApportionmentMethod = "SHP";
			freightCost.E6_InvoiceNum = "abcxyz";
			freightCost.E6_InvoiceDate = ZDateTime.Now;
			freightCost.E6_PaymentDate = ZDateTime.Now;
		}

		void SetupShipmentCharge(Charge shipmentCharge, decimal costAmt, decimal sellAmt, ZGuid chargeCode, decimal? agentCostAmt = null, decimal? agentSellAmt = null)
		{
			shipmentCharge.JR_AC = chargeCode;
			shipmentCharge.JR_RX_NKSellCurrency = Creator.USD.RX_Code;
			shipmentCharge.JR_RX_NKCostCurrency = Creator.USD.RX_Code;
			shipmentCharge.JR_OSSellAmt = sellAmt;
			shipmentCharge.JR_OH_SellAccount = Creator.Agent.PK;
			shipmentCharge.JR_OSCostAmt = costAmt;
			shipmentCharge.JR_IsIncludedInProfitShare = true;
			shipmentCharge.JR_AgentDeclaredSellAmt = agentSellAmt ?? sellAmt;
			shipmentCharge.JR_AgentDeclaredCostAmt = agentCostAmt ?? costAmt;
			shipmentCharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
		}

		protected override void SetUp()
		{
			base.SetUp();
			Creator = new TestObjectCreator(Factory);

			SetSinglePeriod();
			OriginalGroupPK = SetPostingNotificationGroup();
		}

		protected override void TearDown()
		{
			base.TearDown();

			AccountingConfigurationRegistry.Instance.JobPostingNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OriginalGroupPK);
		}

		public class DummyOverseasAgentChargePoster : OverseasAgentChargesPoster
		{
			public DummyOverseasAgentChargePoster(IJobCostingPlugIn consol)
				: base(consol)
			{ }

			protected override ConsolInvoicingPostManager GetPostManager(Job[] jobs, ApportionmentListing costs, IJobCostingPlugIn consol)
			{
				return new DummyConsolInvoicingPostManager(new BusinessObjectFactory(), jobs, consol);
			}
		}

		public class DummyConsolInvoicingPostManager : ConsolInvoicingPostManager
		{
			public DummyConsolInvoicingPostManager(BusinessObjectFactory fallbackFactory, IEnumerable<Job> jobs, IJobCostingPlugIn consol)
				: base(fallbackFactory, jobs, consol)
			{ }

			protected override UserMessageEventArgs BuildInvoiceDateInFutureWarningMessage(TransactionCreatorHashtable transactions)
			{
				return new UserMessageEventArgs("this is only for test warning message!");
			}
		}
	}
}
