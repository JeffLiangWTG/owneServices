using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting.Testing
{
	public class ConsolPostingChargeDistributorTest : PostingChargeDistributorTest
	{
		public void TestIsCombinedShipmentCharges_ForBuyersConsol()
		{
			var creator = new TestObjectCreator(Factory);
			var shipment1 = Consol.Shipments.AddNew();
			var loader = new Job.Loader(shipment1);
			var header1 = loader.TryCreateWithoutMutexForTestOnly();
			header1.JH_JobNum = "s1";
			Consol.JK_ConsolMode = Constants.ContainerModes.BuyersConsol;
			creator.ABIGAS.CompanyData.OB_ARBuyersConsolInvoicingStyle = Constants.ConsolInvoicingStyles.ApportionInvoiceMaster;
			header1.JH_OA_LocalChargesAddr = creator.ABIGAS.MainAddress.PK;
			shipment1.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;

			var charge1 = creator.CreateCharge(header1, creator.CC1, "CHARGE 1", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.ABIGAS);
			var distributor = new ConsolPostingChargeDistributor(Consol);
			AssertEquals("Precondition", false, distributor.IsOverseasAgentCharge(charge1.JR_OH_SellAccount));
			AssertEquals(true, distributor.IsCombinedShipmentCharges(charge1));
		}

		public void TestIsCombinedShipmentCharges_ForCollectFeesOnSingleInvoice()
		{
			var creator = new TestObjectCreator(Factory);
			var job1 = creator.Job1;
			job1.JH_OA_AgentCollectAddr = creator.AALSHI.MainAddress.PK;
			Consol.JK_RL_NKLoadPort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
			Consol.JK_RL_NKDischargePort = "USLAX";
			Consol.SetDefaultReceivingForwarderAddress(creator.Agent);

			var charge1 = creator.CreateCharge(job1, creator.CC1, "CHARGE 1", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.Agent);
			var distributor = new ConsolPostingChargeDistributor(Consol);
			AssertEquals("Precondition", true, distributor.IsOverseasAgentCharge(charge1.JR_OH_SellAccount));
			AssertEquals(true, distributor.IsCombinedShipmentCharges(charge1));

			charge1.SellAccount.MiscServ.OM_FWBillCollectFeesOnSingleInvoice = false;
			AssertEquals(false, distributor.IsCombinedShipmentCharges(charge1));
		}

		public void TestDistributeAgentChargesGroupOnBCN()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job1 = creator.Job1;
			Job job2 = creator.Job2;

			ForwardingShipment shipment1 = Consol.Shipments.AddNew();
			ForwardingShipment shipment2 = Consol.Shipments.AddNew();

			Consol.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.BuyersConsol;

			Job.Loader loader = new Job.Loader(shipment1);
			Job header1 = loader.TryCreateWithoutMutexForTestOnly();
			header1.JH_JobNum = "s1";
			loader = new Job.Loader(shipment2);
			Job header2 = loader.TryCreateWithoutMutexForTestOnly();
			header2.JH_JobNum = "s2";

			header1.JH_OA_LocalChargesAddr = creator.ABIGAS.MainAddress.PK;

			shipment1.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;

			Charge charge1 = creator.CreateCharge(header1, creator.CC1, "CHARGE 1", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.ABIGAS);
			Charge charge2 = creator.CreateCharge(header1, creator.CC1, "CHARGE 1", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.AALSHI);

			Charge charge3 = creator.CreateCharge(header2, creator.CC1, "CHARGE 2", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.ABIGAS);
			Charge charge4 = creator.CreateCharge(header2, creator.CC1, "CHARGE 1", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.Agent);

			IReceivablesPostingChargeCollection charges = new IReceivablesPostingChargeCollection();

			charges.Add(charge1);
			charges.Add(charge2);
			charges.Add(charge3);
			charges.Add(charge4);

			ConsolPostingChargeDistributor distributor = new ConsolPostingChargeDistributor(Consol);
			PostingChargeCollection results = distributor.DistributeCharges(charges);

			AssertEquals("Should only be 2 collection of charges", 4, results.Count);

			OrganisationsDataRegistry.Instance.BuyersConsolInvoicingStyle.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Constants.ConsolInvoicingStyles.ApportionInvoiceMaster);

			results = distributor.DistributeCharges(charges);

			AssertEquals("Should only be 3 collection of charges, ABIGASs combined cos it's lead shipment's local client", 3, results.Count);
		}

		public void TestIgnoreSellReferenceWhenPostingGroupedBCNInvoices()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job1 = creator.Job1;
			Job job2 = creator.Job2;

			ForwardingShipment shipment1 = Consol.Shipments.AddNew();
			ForwardingShipment shipment2 = Consol.Shipments.AddNew();

			Consol.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.BuyersConsol;

			Job.Loader loader = new Job.Loader(shipment1);
			Job header1 = loader.TryCreateWithoutMutexForTestOnly();
			header1.JH_JobNum = "s1";
			loader = new Job.Loader(shipment2);
			Job header2 = loader.TryCreateWithoutMutexForTestOnly();
			header2.JH_JobNum = "s2";

			header1.JH_OA_LocalChargesAddr = creator.ABIGAS.MainAddress.PK;

			shipment1.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;

			Charge charge1 = creator.CreateCharge(header1, creator.CC1, "CHARGE 1", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.ABIGAS);
			Charge charge2 = creator.CreateCharge(header1, creator.CC1, "CHARGE 1", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.AALSHI);

			Charge charge3 = creator.CreateCharge(header2, creator.CC1, "CHARGE 2", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.ABIGAS);
			Charge charge4 = creator.CreateCharge(header2, creator.CC1, "CHARGE 1", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.Agent);
			charge1.JR_SellReference = "888888";
			charge1.JR_SellReference = "999999";
			charge1.JR_SellReference = "111111";
			charge1.JR_SellReference = "222222";

			IReceivablesPostingChargeCollection charges = new IReceivablesPostingChargeCollection();

			charges.Add(charge1);
			charges.Add(charge2);
			charges.Add(charge3);
			charges.Add(charge4);

			OrganisationsDataRegistry.Instance.BuyersConsolInvoicingStyle.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Constants.ConsolInvoicingStyles.ApportionInvoiceMaster);

			ConsolPostingChargeDistributor distributor = new ConsolPostingChargeDistributor(Consol);
			PostingChargeCollection results = distributor.DistributeCharges(charges);

			AssertEquals("Should be 3 collection of charges because we're ignoring sell reference here", 3, results.Count);
		}

		public void TestIgnoreSellReferenceWhenPostingAgentChargesWithSingleAgentInvoice()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job1 = creator.Job1;
			Job job2 = creator.Job2;

			job1.JH_OA_AgentCollectAddr = creator.Agent.MainAddress.PK;
			job2.JH_OA_AgentCollectAddr = creator.Agent.MainAddress.PK;

			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
			transport.JW_RL_NKDiscPort = "USLAX";

			Charge charge1 = creator.CreateCharge(job1, creator.CC1, "CHARGE 1", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.Agent);
			Charge charge2 = creator.CreateCharge(job2, creator.CC1, "CHARGE 2", creator.AUD, 100m, creator.ABIGAS, creator.GBP, 200m, creator.Agent);
			charge1.JR_OSSellExRate = 0.65m;
			charge2.JR_OSSellExRate = 0.42m;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge1.JR_SellReference = "111111";
			charge2.JR_SellReference = "222222";

			ConsolPostingChargeDistributor distributor = new ConsolPostingChargeDistributor(Consol);

			IReceivablesPostingChargeCollection charges = new IReceivablesPostingChargeCollection();
			charges.Add(charge1);
			charges.Add(charge2);

			Assert("Must be True by default", creator.Agent.MiscServ.OM_FWBillCollectFeesOnSingleInvoice);
			PostingChargeCollection results = distributor.DistributeCharges(charges);
			AssertEquals("Should be 1 collection of charges", 1, results.Count);

			PostingChargeKey key = new PostingChargeKey(creator.Agent.PK, InvoiceTypesList.Codes.FinalInvoice, Consol.JK_UniqueConsignRef, ZGuid.Empty, ZGuid.Empty, 0);

			IReceivablesPostingChargeCollection distributedCharges = results[key];
			AssertNotNull("Should have found relevant invoice", distributedCharges);
			AssertEquals("Should have distributed 2 charges into collection", 2, distributedCharges.Count);
		}

		public void TestSplitInvoicesBySellReferenceWhenSingleAgentInvoiceIsOff()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job1 = creator.Job1;
			Job job2 = creator.Job2;

			job1.JH_OA_AgentCollectAddr = creator.Agent.MainAddress.PK;
			job2.JH_OA_AgentCollectAddr = creator.Agent.MainAddress.PK;

			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
			transport.JW_RL_NKDiscPort = "USLAX";

			Charge charge1 = creator.CreateCharge(job1, creator.CC1, "CHARGE 1", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.Agent);
			Charge charge2 = creator.CreateCharge(job2, creator.CC1, "CHARGE 2", creator.AUD, 100m, creator.ABIGAS, creator.GBP, 200m, creator.Agent);
			Charge charge3 = creator.CreateCharge(job1, creator.CC1, "CHARGE 1", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.Agent);
			Charge charge4 = creator.CreateCharge(job2, creator.CC1, "CHARGE 2", creator.AUD, 100m, creator.ABIGAS, creator.GBP, 200m, creator.Agent);
			charge1.JR_OSSellExRate = 0.65m;
			charge2.JR_OSSellExRate = 0.42m;
			charge3.JR_OSSellExRate = 0.65m;
			charge4.JR_OSSellExRate = 0.42m;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge3.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge4.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			charge1.JR_SellReference = "111111";
			charge2.JR_SellReference = "222222";
			charge3.JR_SellReference = "333333";
			charge4.JR_SellReference = "444444";

			ConsolPostingChargeDistributor distributor = new ConsolPostingChargeDistributor(Consol);

			IReceivablesPostingChargeCollection charges = new IReceivablesPostingChargeCollection();
			charges.Add(charge1);
			charges.Add(charge2);
			charges.Add(charge3);
			charges.Add(charge4);

			creator.Agent.MiscServ.OM_FWBillCollectFeesOnSingleInvoice = false;
			var results = distributor.DistributeCharges(charges);
			AssertEquals("Should be 4 collections of charges", 4, results.Count);

			var actual = results.Cast<IReceivablesPostingChargeCollection>().Select(x => x.Key.SellReference).ToArray();
			var expected = new ZString[] { "111111", "222222", "333333", "444444" };

			AssertContainsExactElementsInAnyOrder("Should find all keys", expected, actual);
		}

		[ExpectNoExceptions]
		public void TestDistributeAgentChargesGroupOnBCNDoesNotThrowNullReferenceExceptionWhenNoLocalCharges()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job1 = creator.Job1;
			Job job2 = creator.Job2;

			ForwardingShipment shipment1 = Consol.Shipments.AddNew();
			ForwardingShipment shipment2 = Consol.Shipments.AddNew();

			Consol.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.BuyersConsol;

			Job.Loader loader = new Job.Loader(shipment1);
			Job header1 = loader.TryCreateWithoutMutexForTestOnly();
			header1.JH_JobNum = "s1";
			loader = new Job.Loader(shipment2);
			Job header2 = loader.TryCreateWithoutMutexForTestOnly();
			header2.JH_JobNum = "s2";

			shipment1.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;

			Charge charge1 = creator.CreateCharge(header1, creator.CC1, "CHARGE 1", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.ABIGAS);
			Charge charge2 = creator.CreateCharge(header1, creator.CC1, "CHARGE 1", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.AALSHI);

			Charge charge3 = creator.CreateCharge(header2, creator.CC1, "CHARGE 2", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.ABIGAS);
			Charge charge4 = creator.CreateCharge(header2, creator.CC1, "CHARGE 1", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.Agent);

			IReceivablesPostingChargeCollection charges = new IReceivablesPostingChargeCollection();

			charges.Add(charge1);
			charges.Add(charge2);
			charges.Add(charge3);
			charges.Add(charge4);

			ConsolPostingChargeDistributor distributor = new ConsolPostingChargeDistributor(Consol);
			PostingChargeCollection results = distributor.DistributeCharges(charges);
		}

		public void TestDistributeAgentCharges()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job1 = creator.Job1;
			Job job2 = creator.Job2;

			job1.JH_OA_AgentCollectAddr = creator.AALSHI.MainAddress.PK;
			job2.JH_OA_AgentCollectAddr = creator.AALSHI.MainAddress.PK;

			Consol.JK_RL_NKLoadPort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
			Consol.JK_RL_NKDischargePort = "USLAX";

			Consol.SetDefaultReceivingForwarderAddress(creator.Agent);
			AssertEquals("AgentToInvoice", creator.Agent.PK, Consol.AgentToInvoice()?.PK);

			Charge charge1 = creator.CreateCharge(job1, creator.CC1, "CHARGE 1", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.Agent);
			Charge charge2 = creator.CreateCharge(job2, creator.CC1, "CHARGE 2", creator.AUD, 100m, creator.ABIGAS, creator.GBP, 200m, creator.Agent);
			charge1.JR_OSSellExRate = 0.65m;
			charge2.JR_OSSellExRate = 0.42m;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			ConsolPostingChargeDistributor distributor = new ConsolPostingChargeDistributor(Consol);

			IReceivablesPostingChargeCollection charges = new IReceivablesPostingChargeCollection();
			charges.Add(charge1);
			charges.Add(charge2);

			PostingChargeCollection results = distributor.DistributeCharges(charges);
			AssertEquals("Should only be 1 collection of charges", 1, results.Count);

			PostingChargeKey key = new PostingChargeKey(Consol.ReceivingForwarderPK, "", Consol.JK_UniqueConsignRef, ZGuid.Empty, ZGuid.Empty, 0);

			IReceivablesPostingChargeCollection distributedCharges = results[key];
			AssertNotNull("Should have found relevant invoice", distributedCharges);
			AssertEquals("Should have distributed 2 charges into collection", 2, distributedCharges.Count);
		}

		public void TestDistributeAgentCharges_TaxBranch()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var creator = new TestObjectCreator(Factory);
				var job1 = creator.Job1;
				var job2 = creator.Job2;

				job1.JH_OA_AgentCollectAddr = creator.AALSHI.MainAddress.PK;
				job2.JH_OA_AgentCollectAddr = creator.AALSHI.MainAddress.PK;

				Consol.JK_RL_NKLoadPort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
				Consol.JK_RL_NKDischargePort = "USLAX";

				Consol.SetDefaultReceivingForwarderAddress(creator.Agent);
				AssertEquals("AgentToInvoice", creator.Agent.PK, Consol.AgentToInvoice()?.PK);

				var charge1 = creator.CreateCharge(job1, creator.CC1, "CHARGE 1", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.Agent);
				var charge2 = creator.CreateCharge(job1, creator.CC1, "CHARGE 2", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.Agent);
				var charge3 = creator.CreateCharge(job2, creator.CC1, "CHARGE 3", creator.AUD, 100m, creator.ABIGAS, creator.GBP, 200m, creator.Agent);
				charge1.JR_GB_SellTaxBranch = creator.NonCurrentBranch.PK;
				charge2.JR_GB_SellTaxBranch = GlbBranch.CurrentBranch.PK;
				charge3.JR_GB_SellTaxBranch = creator.NonCurrentBranch.PK;

				var distributor = new ConsolPostingChargeDistributor(Consol);

				var charges = new IReceivablesPostingChargeCollection();
				charges.Add(charge1);
				charges.Add(charge2);
				charges.Add(charge3);

				AssertEquals("Precondition: IsOverseasAgentCharge", Consol.AgentToInvoice().PK, creator.Agent.PK);
				AssertEquals("Precondition: OM_FWBillCollectFeesOnSingleInvoice", true, creator.Agent.MiscServ.OM_FWBillCollectFeesOnSingleInvoice);

				var results = distributor.DistributeCharges(charges);
				AssertEquals("Should only be 2 collections of charges", 2, results.Count);

				var key1 = results.Keys.OfType<PostingChargeKey>().First(x => x.TaxBranch == creator.NonCurrentBranch.PK);
				var distributedCharges = results[key1];
				AssertNotNull("Should have found relevant charges", distributedCharges);
				AssertEquals("Should have distributed 1 charge into collection", 2, distributedCharges.Count);
				AssertContainsExactElementsInAnyOrder(new[] { charge1.PK, charge3.PK }, distributedCharges.OfType<Charge>().Select(x => x.PK));

				var key2 = results.Keys.OfType<PostingChargeKey>().First(x => x.TaxBranch == GlbBranch.CurrentBranch.PK);
				distributedCharges = results[key2];
				AssertNotNull("Should have found relevant charges", distributedCharges);
				AssertEquals("Should have distributed 1 charge into collection", 1, distributedCharges.Count);
				AssertEquals(charge2.PK, ((Charge)distributedCharges[0]).PK);
			}
		}

		public void TestDistributeAgentCharges_PlaceOfSupply()
		{
			using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForReceivableTransactions.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var creator = new TestObjectCreator(Factory);
				var job1 = creator.Job1;
				var job2 = creator.Job2;

				job1.JH_OA_AgentCollectAddr = creator.AALSHI.MainAddress.PK;
				job2.JH_OA_AgentCollectAddr = creator.AALSHI.MainAddress.PK;

				Consol.JK_RL_NKLoadPort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
				Consol.JK_RL_NKDischargePort = "USLAX";

				Consol.SetDefaultReceivingForwarderAddress(creator.Agent);
				AssertEquals("AgentToInvoice", creator.Agent.PK, Consol.AgentToInvoice()?.PK);

				var charge1 = creator.CreateCharge(job1, creator.CC1, "CHARGE 1", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.Agent);
				var charge2 = creator.CreateCharge(job2, creator.CC1, "CHARGE 2", creator.AUD, 100m, creator.ABIGAS, creator.GBP, 200m, creator.Agent);
				charge1.JR_OSSellExRate = 0.65m;
				charge2.JR_OSSellExRate = 0.42m;
				charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				charge2.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
				charge1.JR_SellPlaceOfSupply = "NSW";
				charge2.JR_SellPlaceOfSupply = "WA";

				var distributor = new ConsolPostingChargeDistributor(Consol);

				var charges = new IReceivablesPostingChargeCollection();
				charges.Add(charge1);
				charges.Add(charge2);

				var results = distributor.DistributeCharges(charges);
				AssertEquals("Should only be 2 collections of charges", 2, results.Count);

				var key1 = new PostingChargeKey(Consol.ReceivingForwarderPK, "", Consol.JK_UniqueConsignRef, ZGuid.Empty, ZGuid.Empty, 0, placeOfSupply: "NSW");
				var distributedCharges = results[key1];
				AssertNotNull("Should have found relevant charges", distributedCharges);
				AssertEquals("Should have distributed 1 charge into collection", 1, distributedCharges.Count);
				AssertEquals(charge1.PK, ((Charge)distributedCharges[0]).PK);

				var key2 = new PostingChargeKey(Consol.ReceivingForwarderPK, "", Consol.JK_UniqueConsignRef, ZGuid.Empty, ZGuid.Empty, 0, placeOfSupply: "WA");
				distributedCharges = results[key2];
				AssertNotNull("Should have found relevant charges", distributedCharges);
				AssertEquals("Should have distributed 1 charge into collection", 1, distributedCharges.Count);
				AssertEquals(charge2.PK, ((Charge)distributedCharges[0]).PK);
			}
		}

		public void TestDistributeAgentCharges_WithSellInvoiceCurrency()
		{
			var creator = new TestObjectCreator(Factory);
			var job1 = creator.Job1;
			var job2 = creator.Job2;

			job1.JH_OA_AgentCollectAddr = creator.AALSHI.MainAddress.PK;
			job2.JH_OA_AgentCollectAddr = creator.AALSHI.MainAddress.PK;

			Consol.JK_RL_NKLoadPort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
			Consol.JK_RL_NKDischargePort = "USLAX";

			Consol.SetDefaultReceivingForwarderAddress(creator.Agent);
			AssertEquals("AgentToInvoice", creator.Agent.PK, Consol.AgentToInvoice()?.PK);

			var charge1 = creator.CreateCharge(job1, creator.CC1, "CHARGE 1", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.Agent);
			var charge2 = creator.CreateCharge(job2, creator.CC1, "CHARGE 2", creator.AUD, 100m, creator.ABIGAS, creator.GBP, 200m, creator.Agent);
			charge1.JR_OSSellExRate = 0.65m;
			charge2.JR_OSSellExRate = 0.42m;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge1.JR_RX_NKSellInvoiceCurrency = creator.EUR.RX_Code;

			var distributor = new ConsolPostingChargeDistributor(Consol);

			var charges = new IReceivablesPostingChargeCollection();
			charges.Add(charge1);
			charges.Add(charge2);

			var results = distributor.DistributeCharges(charges);
			AssertEquals("Should only be 1 collection of charges", 1, results.Count);

			var key = new PostingChargeKey(Consol.ReceivingForwarderPK, "", Consol.JK_UniqueConsignRef, ZGuid.Empty, ZGuid.Empty, 0);

			var distributedCharges = results[key];
			AssertNotNull("Should have found relevant invoice", distributedCharges);
			AssertEquals("Should have distributed 2 charges into collection", 2, distributedCharges.Count);
		}

		public void TestDistributeAgentCharges_WithAddressContact()
		{
			var creator = new TestObjectCreator(Factory);
			var job1 = creator.Job1;
			var job2 = creator.Job2;

			job1.JH_OA_AgentCollectAddr = creator.AALSHI.MainAddress.PK;
			job2.JH_OA_AgentCollectAddr = creator.AALSHI.MainAddress.PK;

			Consol.JK_RL_NKLoadPort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
			Consol.JK_RL_NKDischargePort = "USLAX";

			Consol.SetDefaultReceivingForwarderAddress(creator.Agent);
			AssertEquals("AgentToInvoice", creator.Agent.PK, Consol.AgentToInvoice()?.PK);

			var chargeAddressContacts = new Dictionary<Charge, Tuple<ZGuid, ZGuid>>();
			Action<Charge> addChargeAddressContacts = charge => chargeAddressContacts.Add(charge, new Tuple<ZGuid, ZGuid>(charge.JR_OA_SellInvoiceAddress, charge.JR_OC_SellInvoiceContact));
			Action<Charge> assertChargeAddressContacts = charge =>
			{
				AssertEquals("Address", chargeAddressContacts[charge].Item1, charge.JR_OA_SellInvoiceAddress);
				AssertEquals("Contact", chargeAddressContacts[charge].Item2, charge.JR_OC_SellInvoiceContact);
			};

			var charge1 = creator.CreateCharge(job1, creator.CC1, "CHARGE 1", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.Agent);
			var charge2 = creator.CreateCharge(job2, creator.CC1, "CHARGE 2", creator.AUD, 100m, creator.ABIGAS, creator.GBP, 200m, creator.Agent);
			charge1.JR_OSSellExRate = 0.65m;
			charge2.JR_OSSellExRate = 0.42m;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			var address1 = creator.CreateAddress(charge1.SellAccount, "1 street");
			var address2 = creator.CreateAddress(charge2.SellAccount, "2 street");
			var contact1 = creator.CreateContact(charge1.SellAccount, "John");
			var contact2 = creator.CreateContact(charge2.SellAccount, "Bob");
			charge1.JR_OA_SellInvoiceAddress = address1.PK;
			charge1.JR_OC_SellInvoiceContact = contact1.PK;
			charge2.JR_OA_SellInvoiceAddress = address2.PK;
			charge2.JR_OC_SellInvoiceContact = contact2.PK;

			var distributor = new ConsolPostingChargeDistributor(Consol);

			var charges = new IReceivablesPostingChargeCollection();
			charges.Add(charge1);
			charges.Add(charge2);

			var results = distributor.DistributeCharges(charges);
			AssertEquals("Should only be 1 collection of charges", 1, results.Count);

			var key = new PostingChargeKey(Consol.ReceivingForwarderPK, "", Consol.JK_UniqueConsignRef, ZGuid.Empty, ZGuid.Empty, 0);

			var distributedCharges = results[key];
			AssertNotNull("Should have found relevant invoice", distributedCharges);
			AssertEquals("Should have distributed 2 charges into collection", 2, distributedCharges.Count);

			AssertEquals("Charge1 Address should be reset", ZGuid.Empty, charge1.JR_OA_SellInvoiceAddress);
			AssertEquals("Charge1 Contact should be reset", ZGuid.Empty, charge1.JR_OC_SellInvoiceContact);
			AssertEquals("Charge2 Address should be reset", ZGuid.Empty, charge2.JR_OA_SellInvoiceAddress);
			AssertEquals("Charge2 Contact should be reset", ZGuid.Empty, charge2.JR_OC_SellInvoiceContact);
		}

		public void TestDistributeAgentChargesDependsOnOM_FWBillCollectFeesOnSingleInvoice()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job1 = creator.Job1;
			Job job2 = creator.Job2;

			job1.JH_OA_AgentCollectAddr = creator.Agent.MainAddress.PK;
			job2.JH_OA_AgentCollectAddr = creator.Agent.MainAddress.PK;

			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
			transport.JW_RL_NKDiscPort = "USLAX";

			Charge charge1 = creator.CreateCharge(job1, creator.CC1, "CHARGE 1", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.Agent);
			Charge charge2 = creator.CreateCharge(job2, creator.CC1, "CHARGE 2", creator.AUD, 100m, creator.ABIGAS, creator.GBP, 200m, creator.Agent);
			charge1.JR_OSSellExRate = 0.65m;
			charge2.JR_OSSellExRate = 0.42m;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			ConsolPostingChargeDistributor distributor = new ConsolPostingChargeDistributor(Consol);

			IReceivablesPostingChargeCollection charges = new IReceivablesPostingChargeCollection();
			charges.Add(charge1);
			charges.Add(charge2);

			Assert("Must be True by default", creator.Agent.MiscServ.OM_FWBillCollectFeesOnSingleInvoice);
			PostingChargeCollection results = distributor.DistributeCharges(charges);
			AssertEquals("Should be 1 collection of charges", 1, results.Count);

			PostingChargeKey key = new PostingChargeKey(creator.Agent.PK, InvoiceTypesList.Codes.FinalInvoice, Consol.JK_UniqueConsignRef, ZGuid.Empty, ZGuid.Empty, 0);

			IReceivablesPostingChargeCollection distributedCharges = results[key];
			AssertNotNull("Should have found relevant invoice", distributedCharges);
			AssertEquals("Should have distributed 2 charges into collection", 2, distributedCharges.Count);

			creator.Agent.MiscServ.OM_FWBillCollectFeesOnSingleInvoice = false;
			results = distributor.DistributeCharges(charges);
			AssertEquals("Should be 2 collections of charges", 2, results.Count);

			key = new PostingChargeKey(creator.Agent.PK, InvoiceTypesList.Codes.FinalInvoice, job1.JH_JobNum, ZGuid.Empty, ZGuid.Empty, 0);
			distributedCharges = results[key];
			AssertNotNull("Should have found relevant invoice", distributedCharges);
			AssertEquals("Should have distributed 1 charge into collection", 1, distributedCharges.Count);

			key = new PostingChargeKey(creator.Agent.PK, InvoiceTypesList.Codes.FinalInvoice, job2.JH_JobNum, ZGuid.Empty, ZGuid.Empty, 0);
			distributedCharges = results[key];
			AssertNotNull("Should have found relevant invoice", distributedCharges);
			AssertEquals("Should have distributed 1 charge into collection", 1, distributedCharges.Count);
		}

		public void TestDistributeAgentChargesDependsOnOM_FWBillCollectFeesOnSingleInvoice_PlaceOfSupply()
		{
			using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForReceivableTransactions.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var creator = new TestObjectCreator(Factory);
				var job1 = creator.Job1;
				var job2 = creator.Job2;

				job1.JH_OA_AgentCollectAddr = creator.Agent.MainAddress.PK;
				job2.JH_OA_AgentCollectAddr = creator.Agent.MainAddress.PK;

				var transport = Consol.Transports[0];
				transport.JW_RL_NKLoadPort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
				transport.JW_RL_NKDiscPort = "USLAX";

				var charge1 = creator.CreateCharge(job1, creator.CC1, "CHARGE 1", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.Agent);
				var charge2 = creator.CreateCharge(job2, creator.CC1, "CHARGE 2", creator.AUD, 100m, creator.ABIGAS, creator.GBP, 200m, creator.Agent);
				charge1.JR_OSSellExRate = 0.65m;
				charge2.JR_OSSellExRate = 0.42m;
				charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				charge1.JR_SellPlaceOfSupply = "NSW";
				charge2.JR_SellPlaceOfSupply = "WA";

				var distributor = new ConsolPostingChargeDistributor(Consol);

				var charges = new IReceivablesPostingChargeCollection();
				charges.Add(charge1);
				charges.Add(charge2);

				Assert("Must be True by default", creator.Agent.MiscServ.OM_FWBillCollectFeesOnSingleInvoice);
				PostingChargeCollection results = distributor.DistributeCharges(charges);
				AssertEquals("Should be 2 collections of charges", 2, results.Count);

				var key1 = new PostingChargeKey(creator.Agent.PK, InvoiceTypesList.Codes.FinalInvoice, Consol.JK_UniqueConsignRef, ZGuid.Empty, ZGuid.Empty, 0, placeOfSupply: "NSW");
				var distributedCharges = results[key1];
				AssertNotNull("Should have found relevant charges", distributedCharges);
				AssertEquals("Should have 1 charge into collection", 1, distributedCharges.Count);
				AssertEquals(charge1.PK, ((Charge)distributedCharges[0]).PK);

				var key2 = new PostingChargeKey(creator.Agent.PK, InvoiceTypesList.Codes.FinalInvoice, Consol.JK_UniqueConsignRef, ZGuid.Empty, ZGuid.Empty, 0, placeOfSupply: "WA");
				distributedCharges = results[key2];
				AssertNotNull("Should have found relevant charges", distributedCharges);
				AssertEquals("Should have 1 charge into collection", 1, distributedCharges.Count);
				AssertEquals(charge2.PK, ((Charge)distributedCharges[0]).PK);
			}
		}

		public void TestDistributeAgentChargesDependsOnOM_FWBillCollectFeesOnSingleInvoice_WithAddressContact()
		{
			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			var address2 = Factory.NewWithValidTestData<OrgAddress>();
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();

			var creator = new TestObjectCreator(Factory);
			var job1 = creator.Job1;
			var job2 = creator.Job2;

			job1.JH_OA_AgentCollectAddr = creator.Agent.MainAddress.PK;
			job2.JH_OA_AgentCollectAddr = creator.Agent.MainAddress.PK;

			var transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
			transport.JW_RL_NKDiscPort = "USLAX";

			var chargeAddressContacts = new Dictionary<Charge, Tuple<ZGuid, ZGuid>>();
			Action<Charge> addChargeAddressContacts = charge => chargeAddressContacts.Add(charge, new Tuple<ZGuid, ZGuid>(charge.JR_OA_SellInvoiceAddress, charge.JR_OC_SellInvoiceContact));
			Action<Charge> assertChargeAddressContacts = charge =>
			{
				AssertEquals("Address", chargeAddressContacts[charge].Item1, charge.JR_OA_SellInvoiceAddress);
				AssertEquals("Contact", chargeAddressContacts[charge].Item2, charge.JR_OC_SellInvoiceContact);
			};

			var charge1 = creator.CreateCharge(job1, creator.CC1, "CHARGE 1", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.Agent);
			var charge2 = creator.CreateCharge(job2, creator.CC1, "CHARGE 2", creator.AUD, 100m, creator.ABIGAS, creator.GBP, 200m, creator.Agent);
			charge1.JR_OSSellExRate = 0.65m;
			charge2.JR_OSSellExRate = 0.42m;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge1.JR_OA_SellInvoiceAddress = address1.PK;
			charge1.JR_OC_SellInvoiceContact = contact1.PK;
			charge2.JR_OA_SellInvoiceAddress = address2.PK;
			charge2.JR_OC_SellInvoiceContact = contact2.PK;
			addChargeAddressContacts(charge1);
			addChargeAddressContacts(charge2);

			var distributor = new ConsolPostingChargeDistributor(Consol);

			var charges = new IReceivablesPostingChargeCollection();
			charges.Add(charge1);
			charges.Add(charge2);

			creator.Agent.MiscServ.OM_FWBillCollectFeesOnSingleInvoice = false;
			var results = distributor.DistributeCharges(charges);
			AssertEquals("Should be 2 collections of charges", 2, results.Count);

			var key = new PostingChargeKey(creator.Agent.PK, InvoiceTypesList.Codes.FinalInvoice, job1.JH_JobNum, address1.PK, contact1.PK, 0);
			var distributedCharges = results[key];
			AssertNotNull("Should have found relevant invoice", distributedCharges);
			AssertEquals("Should have distributed 1 charge into collection", 1, distributedCharges.Count);

			key = new PostingChargeKey(creator.Agent.PK, InvoiceTypesList.Codes.FinalInvoice, job2.JH_JobNum, address2.PK, contact2.PK, 0);
			distributedCharges = results[key];
			AssertNotNull("Should have found relevant invoice", distributedCharges);
			AssertEquals("Should have distributed 1 charge into collection", 1, distributedCharges.Count);

			assertChargeAddressContacts(charge1);
			assertChargeAddressContacts(charge2);

			creator.Agent.MiscServ.OM_FWBillCollectFeesOnSingleInvoice = true;
			Assert("Must be True by default", creator.Agent.MiscServ.OM_FWBillCollectFeesOnSingleInvoice);
			results = distributor.DistributeCharges(charges);
			AssertEquals("Should be 1 collection of charges", 1, results.Count);

			key = new PostingChargeKey(creator.Agent.PK, InvoiceTypesList.Codes.FinalInvoice, Consol.JK_UniqueConsignRef, ZGuid.Empty, ZGuid.Empty, 0);

			distributedCharges = results[key];
			AssertNotNull("Should have found relevant invoice", distributedCharges);
			AssertEquals("Should have distributed 2 charges into collection", 2, distributedCharges.Count);

			AssertEquals("Charge1 Address should be reset", ZGuid.Empty, charge1.JR_OA_SellInvoiceAddress);
			AssertEquals("Charge1 Contact should be reset", ZGuid.Empty, charge1.JR_OC_SellInvoiceContact);
			AssertEquals("Charge2 Address should be reset", ZGuid.Empty, charge2.JR_OA_SellInvoiceAddress);
			AssertEquals("Charge2 Contact should be reset", ZGuid.Empty, charge2.JR_OC_SellInvoiceContact);
		}

		public void TestDistributeAgentChargesDependsOnOM_FWBillCollectFeesOnSingleInvoice_TaxBranch()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var creator = new TestObjectCreator(Factory);
				var job1 = creator.Job1;
				var job2 = creator.Job2;

				job1.JH_OA_AgentCollectAddr = creator.Agent.MainAddress.PK;
				job2.JH_OA_AgentCollectAddr = creator.Agent.MainAddress.PK;

				var transport = Consol.Transports[0];
				transport.JW_RL_NKLoadPort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
				transport.JW_RL_NKDiscPort = "USLAX";

				var charge1 = creator.CreateCharge(job1, creator.CC1, "CHARGE 1", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.Agent);
				var charge2 = creator.CreateCharge(job2, creator.CC1, "CHARGE 2", creator.AUD, 100m, creator.ABIGAS, creator.GBP, 200m, creator.Agent);
				var charge3 = creator.CreateCharge(job2, creator.CC1, "CHARGE 2", creator.AUD, 100m, creator.ABIGAS, creator.GBP, 200m, creator.Agent);
				charge1.JR_GB_SellTaxBranch = creator.NonCurrentBranch.PK;
				charge2.JR_GB_SellTaxBranch = GlbBranch.CurrentBranch.PK;
				charge3.JR_GB_SellTaxBranch = creator.NonCurrentBranch.PK;

				var distributor = new ConsolPostingChargeDistributor(Consol);

				var charges = new IReceivablesPostingChargeCollection();
				charges.Add(charge1);
				charges.Add(charge2);
				charges.Add(charge3);

				AssertEquals("Precondition: IsAgentCharge", true, Consol.IsAgentCharge(charge1));
				AssertEquals("Precondition: IsAgentCharge", true, Consol.IsAgentCharge(charge2));
				AssertEquals("Precondition: IsAgentCharge", true, Consol.IsAgentCharge(charge3));
				AssertEquals("Precondition: OM_FWBillCollectFeesOnSingleInvoice", true, creator.Agent.MiscServ.OM_FWBillCollectFeesOnSingleInvoice);

				Assert("Must be True by default", creator.Agent.MiscServ.OM_FWBillCollectFeesOnSingleInvoice);
				PostingChargeCollection results = distributor.DistributeCharges(charges);
				AssertEquals("Should be 2 collections of charges", 2, results.Count);

				var key1 = results.Keys.OfType<PostingChargeKey>().First(x => x.TaxBranch == creator.NonCurrentBranch.PK);
				var distributedCharges = results[key1];
				AssertNotNull("Should have found relevant charges", distributedCharges);
				AssertEquals("Should have 1 charge into collection", 2, distributedCharges.Count);
				AssertContainsExactElementsInAnyOrder(new[] { charge1.PK, charge3.PK }, distributedCharges.OfType<Charge>().Select(x => x.PK));

				var key2 = results.Keys.OfType<PostingChargeKey>().First(x => x.TaxBranch == GlbBranch.CurrentBranch.PK);
				distributedCharges = results[key2];
				AssertNotNull("Should have found relevant charges", distributedCharges);
				AssertEquals("Should have 1 charge into collection", 1, distributedCharges.Count);
				AssertEquals(charge2.PK, ((Charge)distributedCharges[0]).PK);
			}
		}

		#region posting groups

		public void TestDistributeDefaultChargesWithPostingGroups()
		{
			var tax1 = Factory.NewWithValidTestData<AccTaxRate>();
			var tax2 = Factory.NewWithValidTestData<AccTaxRate>();

			tax1.AT_PostingGroupId = 1;
			tax2.AT_PostingGroupId = 2;

			Factory.Save();

			var creator = new TestObjectCreator(Factory);

			var charges = new IReceivablesPostingChargeCollection();
			Job job = Factory.NewJobForTesting<Job>();
			Charge charge1 = creator.CreateCharge(job, creator.CC1, "CHARGE 1", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.ABIGAS);
			Charge charge2 = creator.CreateCharge(job, creator.CC1, "CHARGE 1", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.ABIGAS);
			charges.Add(charge1);
			charges.Add(charge2);

			charge1.JR_AT_SellGSTRate = tax1.PK;
			charge2.JR_AT_SellGSTRate = tax2.PK;

			var distributor = GetDistributor();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				Assert(!AccTaxRate.IsPostingGroupsEnabled("AU"));
				var results = distributor.DistributeCharges(charges);
				AssertEquals(1, results.Count);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("VN"))
			{
				Assert(AccTaxRate.IsPostingGroupsEnabled("VN"));

				var results = distributor.DistributeCharges(charges);
				AssertEquals(2, results.Count);

				var key1 = results.Keys.Cast<PostingChargeKey>().First(x => x.TaxRatePostingGroupId == 1);
				var key2 = results.Keys.Cast<PostingChargeKey>().First(x => x.TaxRatePostingGroupId == 2);

				AssertEquals(1, results[key1].Count);
				AssertEquals(1, results[key2].Count);

				AssertEquals((ZShort)1, results[key1][0].TaxRatePostingGroupId);
				AssertEquals((ZShort)2, results[key2][0].TaxRatePostingGroupId);
			}
		}

		public void TestDistributeAgentChargesWithPostingGroups()
		{
			var tax1 = Factory.NewWithValidTestData<AccTaxRate>();
			var tax2 = Factory.NewWithValidTestData<AccTaxRate>();

			tax1.AT_PostingGroupId = 1;
			tax2.AT_PostingGroupId = 2;

			Factory.Save();

			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job1 = creator.Job1;
			Job job2 = creator.Job2;

			job1.JH_OA_AgentCollectAddr = creator.AALSHI.MainAddress.PK;
			job2.JH_OA_AgentCollectAddr = creator.AALSHI.MainAddress.PK;

			Consol.JK_RL_NKLoadPort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
			Consol.JK_RL_NKDischargePort = "USLAX";

			Consol.SetDefaultReceivingForwarderAddress(creator.Agent);
			AssertEquals("AgentToInvoice", creator.Agent.PK, Consol.AgentToInvoice()?.PK);

			Charge charge1 = creator.CreateCharge(job1, creator.CC1, "CHARGE 1", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.Agent);
			Charge charge2 = creator.CreateCharge(job2, creator.CC1, "CHARGE 2", creator.AUD, 100m, creator.ABIGAS, creator.GBP, 200m, creator.Agent);
			charge1.JR_OSSellExRate = 0.65m;
			charge2.JR_OSSellExRate = 0.42m;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			ConsolPostingChargeDistributor distributor = new ConsolPostingChargeDistributor(Consol);

			IReceivablesPostingChargeCollection charges = new IReceivablesPostingChargeCollection();
			charges.Add(charge1);
			charges.Add(charge2);

			charge1.JR_AT_SellGSTRate = tax1.PK;
			charge2.JR_AT_SellGSTRate = tax2.PK;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				Assert(!AccTaxRate.IsPostingGroupsEnabled("AU"));
				PostingChargeCollection results = distributor.DistributeCharges(charges);
				AssertEquals("Should only be 1 collection of charges", 1, results.Count);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("VN"))
			{
				Assert(AccTaxRate.IsPostingGroupsEnabled("VN"));
				PostingChargeCollection results = distributor.DistributeCharges(charges);
				AssertEquals("Should only be 2 collection of charges", 2, results.Count);

				var key1 = results.Keys.Cast<PostingChargeKey>().First(x => x.TaxRatePostingGroupId == 1);
				var key2 = results.Keys.Cast<PostingChargeKey>().First(x => x.TaxRatePostingGroupId == 2);

				AssertEquals(1, results[key1].Count);
				AssertEquals(1, results[key2].Count);

				AssertEquals((ZShort)1, results[key1][0].TaxRatePostingGroupId);
				AssertEquals((ZShort)2, results[key2][0].TaxRatePostingGroupId);
			}
		}

		public void TestDistributeAgentChargesDependsOnOM_FWBillCollectFeesOnSingleInvoiceWithPostingGroups()
		{
			var tax1 = Factory.NewWithValidTestData<AccTaxRate>();
			var tax2 = Factory.NewWithValidTestData<AccTaxRate>();

			tax1.AT_PostingGroupId = 1;
			tax2.AT_PostingGroupId = 2;

			Factory.Save();

			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job1 = creator.Job1;
			Job job2 = creator.Job2;

			job1.JH_OA_AgentCollectAddr = creator.Agent.MainAddress.PK;
			job2.JH_OA_AgentCollectAddr = creator.Agent.MainAddress.PK;

			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
			transport.JW_RL_NKDiscPort = "USLAX";

			Charge charge1 = creator.CreateCharge(job1, creator.CC1, "CHARGE 1", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.Agent);
			Charge charge2 = creator.CreateCharge(job2, creator.CC1, "CHARGE 2", creator.AUD, 100m, creator.ABIGAS, creator.GBP, 200m, creator.Agent);
			charge1.JR_OSSellExRate = 0.65m;
			charge2.JR_OSSellExRate = 0.42m;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			var distributor = new ConsolPostingChargeDistributor(Consol);

			var charges = new IReceivablesPostingChargeCollection();
			charges.Add(charge1);
			charges.Add(charge2);

			charge1.JR_AT_SellGSTRate = tax1.PK;
			charge2.JR_AT_SellGSTRate = tax2.PK;

			Assert("Must be True by default", creator.Agent.MiscServ.OM_FWBillCollectFeesOnSingleInvoice);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				Assert(!AccTaxRate.IsPostingGroupsEnabled("AU"));
				var results = distributor.DistributeCharges(charges);
				AssertEquals("Should be 1 collection of charges", 1, results.Count);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("VN"))
			{
				Assert(AccTaxRate.IsPostingGroupsEnabled("VN"));
				var results = distributor.DistributeCharges(charges);
				AssertEquals("Should be 2 collection of charges", 2, results.Count);

				var key1 = results.Keys.Cast<PostingChargeKey>().First(x => x.TaxRatePostingGroupId == 1);
				var key2 = results.Keys.Cast<PostingChargeKey>().First(x => x.TaxRatePostingGroupId == 2);

				AssertEquals(1, results[key1].Count);
				AssertEquals(1, results[key2].Count);

				AssertEquals((ZShort)1, results[key1][0].TaxRatePostingGroupId);
				AssertEquals((ZShort)2, results[key2][0].TaxRatePostingGroupId);
			}
		}

		#endregion

		#region Implementation

		protected override PostingChargeDistributor GetDistributor()
		{
			return new ConsolPostingChargeDistributor(Consol);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Consol = Factory.New<ForwardingConsol>();
			Consol.JK_UniqueConsignRef = "C00001000";
		}

		ForwardingConsol Consol;

		#endregion
	}
}
