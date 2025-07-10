using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingDependency;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing.Testing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Accounting.Business.JobInvoicing.BranchLevelPostingHelper;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting.Testing
{
	[TestedType(typeof(ChargePoster))]
	public class ChargePosterTest : NonPersistentBusinessObjectTestCase
	{
		#region TestConsolidatedInvoiceRef

		public void TestConsolidatedInvoiceRefWhenOtherJobIsRelated_PostingShipmentJobFirst()
		{
			if (this.GetType() == typeof(PeriodicInvoiceChargePosterTest))
			{
				Assert(true);
			}
			else
			{
				var jobOnShipment = Creator.Job1;
				jobOnShipment.JH_JobNum = "S0001";
				var shipmentCharge1 = Creator.CreateCharge(jobOnShipment, Creator.CC1, "C1", Creator.AUD, 0m, null, Creator.AUD, 100m, Creator.ABIGAS);
				var shipmentCharge2 = Creator.CreateCharge(jobOnShipment, Creator.CC2, "C2", Creator.AUD, 0m, null, Creator.AUD, 100m, Creator.AALSHI);
				var shipmentCharge3 = Creator.CreateCharge(jobOnShipment, Creator.CC3, "C3", Creator.AUD, 0m, null, Creator.AUD, 100m, Factory.NewWithValidTestData<OrgHeader>());
				var shipmentCharge4 = Creator.CreateCharge(jobOnShipment, Creator.CC4, "C4", Creator.AUD, 0m, null, Creator.AUD, 100m, Factory.NewWithValidTestData<OrgHeader>());
				var shipmentCharge5 = Creator.CreateCharge(jobOnShipment, Creator.CC5, "C5", Creator.AUD, 0m, null, Creator.AUD, 100m, Factory.NewWithValidTestData<OrgHeader>());
				var shipmentCharge6 = Creator.CreateCharge(jobOnShipment, Creator.CC6, "C6", Creator.AUD, 0m, null, Creator.AUD, 100m, Factory.NewWithValidTestData<OrgHeader>());
				Factory.Save();

				var jobOnTransport = Creator.Job2;
				jobOnTransport.JH_JobNum = "S0001/E";
				var transportCharge1 = Creator.CreateCharge(jobOnTransport, Creator.CC1, "C1", Creator.AUD, 0m, null, Creator.AUD, 100m, Creator.ABIGAS);
				var transportCharge2 = Creator.CreateCharge(jobOnTransport, Creator.CC2, "C2", Creator.AUD, 0m, null, Creator.AUD, 100m, Creator.AALSHI);
				Factory.Save();

				AssertEquals("Shipment Job Number", "S0001", jobOnShipment.JH_JobNum);
				AssertEquals("Local Transport Job Number on Shipment", "S0001/E", jobOnTransport.JH_JobNum);

				var poster = GetChargePoster();
				var charges = new IReceivablesPostingChargeCollection();
				charges = new IReceivablesPostingChargeCollection();
				charges.Add(shipmentCharge1);
				charges.Add(shipmentCharge2);
				charges.Add(shipmentCharge3);
				charges.Add(shipmentCharge4);
				charges.Add(shipmentCharge5);
				charges.Add(shipmentCharge6);

				var distributedCharges = new PostingChargeDistributor().DistributeCharges(charges);
				AssertEquals("6 collection of charges", 6, distributedCharges.Count);
				foreach (IReceivablesPostingChargeCollection c in distributedCharges)
				{
					poster.Post(c);
				}
				Factory.Save();
				AssertEquals(6, poster.PostedInvoices.Count);
				AssertContainsExactElementsInAnyOrder(new ZString[] { "S0001", "S0001/A", "S0001/B", "S0001/C", "S0001/D", "S0001/E" }, poster.PostedInvoices.ToArray<InvoicingBase>().Select(x => x.AH_ConsolidatedInvoiceRef));

				charges = new IReceivablesPostingChargeCollection();
				charges.Add(transportCharge1);
				charges.Add(transportCharge2);

				distributedCharges = new PostingChargeDistributor().DistributeCharges(charges);
				AssertEquals("2 collection of charges", 2, distributedCharges.Count);
				foreach (IReceivablesPostingChargeCollection c in distributedCharges)
				{
					poster.Post(c);
				}
				Factory.Save();
				AssertEquals(8, poster.PostedInvoices.Count);
				AssertContainsExactElementsInAnyOrder(new ZString[] { "S0001", "S0001/A", "S0001/B", "S0001/C", "S0001/D", "S0001/E", "S0001/E/A", "S0001/E/B" }, poster.PostedInvoices.ToArray<InvoicingBase>().Select(x => x.AH_ConsolidatedInvoiceRef));
			}
		}

		public void TestConsolidatedInvoiceRefWhenOtherJobIsRelated_PostingOtherJobFirst()
		{
			if (this.GetType() == typeof(PeriodicInvoiceChargePosterTest))
			{
				Assert(true);
			}
			else
			{
				var jobOnShipment = Creator.Job1;
				jobOnShipment.JH_JobNum = "S0001";
				var shipmentCharge1 = Creator.CreateCharge(jobOnShipment, Creator.CC1, "C1", Creator.AUD, 0m, null, Creator.AUD, 100m, Creator.ABIGAS);
				var shipmentCharge2 = Creator.CreateCharge(jobOnShipment, Creator.CC2, "C2", Creator.AUD, 0m, null, Creator.AUD, 100m, Creator.AALSHI);
				var shipmentCharge3 = Creator.CreateCharge(jobOnShipment, Creator.CC3, "C3", Creator.AUD, 0m, null, Creator.AUD, 100m, Factory.NewWithValidTestData<OrgHeader>());
				var shipmentCharge4 = Creator.CreateCharge(jobOnShipment, Creator.CC4, "C4", Creator.AUD, 0m, null, Creator.AUD, 100m, Factory.NewWithValidTestData<OrgHeader>());
				var shipmentCharge5 = Creator.CreateCharge(jobOnShipment, Creator.CC5, "C5", Creator.AUD, 0m, null, Creator.AUD, 100m, Factory.NewWithValidTestData<OrgHeader>());
				var shipmentCharge6 = Creator.CreateCharge(jobOnShipment, Creator.CC6, "C6", Creator.AUD, 0m, null, Creator.AUD, 100m, Factory.NewWithValidTestData<OrgHeader>());
				Factory.Save();

				var jobOnTransport = Creator.Job2;
				jobOnTransport.JH_JobNum = "S0001/E";
				var transportCharge1 = Creator.CreateCharge(jobOnTransport, Creator.CC1, "C1", Creator.AUD, 0m, null, Creator.AUD, 100m, Creator.ABIGAS);
				var transportCharge2 = Creator.CreateCharge(jobOnTransport, Creator.CC2, "C2", Creator.AUD, 0m, null, Creator.AUD, 100m, Creator.AALSHI);
				Factory.Save();

				AssertEquals("Shipment Job Number", "S0001", jobOnShipment.JH_JobNum);
				AssertEquals("Local Transport Job Number on Shipment", "S0001/E", jobOnTransport.JH_JobNum);

				var poster = GetChargePoster();
				var charges = new IReceivablesPostingChargeCollection();
				charges.Add(transportCharge1);
				charges.Add(transportCharge2);

				var distributedCharges = new PostingChargeDistributor().DistributeCharges(charges);
				AssertEquals("2 collection of charges", 2, distributedCharges.Count);
				foreach (IReceivablesPostingChargeCollection c in distributedCharges)
				{
					poster.Post(c);
				}
				Factory.Save();
				AssertEquals(2, poster.PostedInvoices.Count);
				AssertContainsExactElementsInAnyOrder(new ZString[] { "S0001/E", "S0001/E/A" }, poster.PostedInvoices.ToArray<InvoicingBase>().Select(x => x.AH_ConsolidatedInvoiceRef));

				charges = new IReceivablesPostingChargeCollection();
				charges.Add(shipmentCharge1);
				charges.Add(shipmentCharge2);
				charges.Add(shipmentCharge3);
				charges.Add(shipmentCharge4);
				charges.Add(shipmentCharge5);
				charges.Add(shipmentCharge6);

				distributedCharges = new PostingChargeDistributor().DistributeCharges(charges);
				AssertEquals("6 collection of charges", 6, distributedCharges.Count);
				foreach (IReceivablesPostingChargeCollection c in distributedCharges)
				{
					poster.Post(c);
				}
				Factory.Save();
				AssertEquals(8, poster.PostedInvoices.Count);
				AssertContainsExactElementsInAnyOrder(new ZString[] { "S0001/E", "S0001/E/A", "S0001", "S0001/A", "S0001/B", "S0001/C", "S0001/D", "S0001/F" }, poster.PostedInvoices.ToArray<InvoicingBase>().Select(x => x.AH_ConsolidatedInvoiceRef));
			}
		}

		public void TestConsolidatedInvoiceRef()
		{
			if (this.GetType() == typeof(PeriodicInvoiceChargePosterTest))
			{
				Assert(true);
			}
			else
			{
				var job = Creator.Job1;
				var poster = GetChargePoster();

				var query = new ZQuery(AccTransactionHeaderSchema.AH_JH, job.PK);
				query.AddToFilter(AccTransactionHeaderSchema.AH_GC, job.JH_GC);
				AssertEquals("No AR Invoice Posted", 0, Factory.Load<AccTransactionHeader>(query).Length);

				#region One Revenue Charge

				var charge1 = Creator.CreateCharge(job, Creator.CC1, "C1", Creator.AUD, 0m, null, Creator.AUD, 100m, Creator.ABIGAS);
				var charges = new IReceivablesPostingChargeCollection();
				charges.Add(charge1);
				Factory.Save();
				var distributedCharges = new PostingChargeDistributor().DistributeCharges(charges);
				AssertEquals("1 collection of charges", 1, distributedCharges.Count);
				foreach (IReceivablesPostingChargeCollection c in distributedCharges)
				{
					poster.Post(c);
				}
				AssertEquals(1, poster.PostedInvoices.Count);
				AssertContainsExactElementsInAnyOrder(new ZString[] { job.JH_JobNum }, poster.PostedInvoices.ToArray<InvoicingBase>().Select(x => x.AH_ConsolidatedInvoiceRef));

				#endregion

				#region Two Revenue Charges with Same Debtors

				var charge2 = Creator.CreateCharge(job, Creator.CC1, "C1", Creator.AUD, 0m, null, Creator.AUD, 100m, Creator.ABIGAS);
				var charge3 = Creator.CreateCharge(job, Creator.CC2, "C2", Creator.AUD, 0m, null, Creator.AUD, 100m, Creator.ABIGAS);
				charges = new IReceivablesPostingChargeCollection();
				charges.Add(charge2);
				charges.Add(charge3);
				Factory.Save();
				distributedCharges = new PostingChargeDistributor().DistributeCharges(charges);
				AssertEquals("1 collection of charges", 1, distributedCharges.Count);
				foreach (IReceivablesPostingChargeCollection c in distributedCharges)
				{
					poster.Post(c);
				}
				AssertEquals(2, poster.PostedInvoices.Count);
				AssertContainsExactElementsInAnyOrder(new ZString[] { job.JH_JobNum, job.JH_JobNum + "/A" }, poster.PostedInvoices.ToArray<InvoicingBase>().Select(x => x.AH_ConsolidatedInvoiceRef));

				#endregion

				#region Two Revenue Charges with Different Debtors

				var charge4 = Creator.CreateCharge(job, Creator.CC1, "C1", Creator.AUD, 0m, null, Creator.AUD, 100m, Creator.ABIGAS);
				var charge5 = Creator.CreateCharge(job, Creator.CC2, "C2", Creator.AUD, 0m, null, Creator.AUD, 100m, Creator.AALSHI);
				charges = new IReceivablesPostingChargeCollection();
				charges.Add(charge4);
				charges.Add(charge5);
				Factory.Save();
				distributedCharges = new PostingChargeDistributor().DistributeCharges(charges);
				AssertEquals("2 collection of charges", 2, distributedCharges.Count);
				foreach (IReceivablesPostingChargeCollection c in distributedCharges)
				{
					poster.Post(c);
				}
				AssertEquals(4, poster.PostedInvoices.Count);
				AssertContainsExactElementsInAnyOrder(new ZString[] { job.JH_JobNum, job.JH_JobNum + "/A", job.JH_JobNum + "/B", job.JH_JobNum + "/C" }, poster.PostedInvoices.ToArray<InvoicingBase>().Select(x => x.AH_ConsolidatedInvoiceRef));

				#endregion

				#region One Revenue Charges with a Dodgy Suffix

				var lastPostedInvoice = Factory.LoadTop1<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef, job.JH_JobNum + "/C"));
				lastPostedInvoice.AH_ConsolidatedInvoiceRef = job.JH_JobNum + "/D";
				Factory.Save();
				var charge6 = Creator.CreateCharge(job, Creator.CC2, "C2", Creator.AUD, 0m, null, Creator.AUD, 100m, Creator.AALSHI);
				charges = new IReceivablesPostingChargeCollection();
				charges.Add(charge6);
				Factory.Save();
				distributedCharges = new PostingChargeDistributor().DistributeCharges(charges);
				AssertEquals("1 collection of charges", 1, distributedCharges.Count);
				foreach (IReceivablesPostingChargeCollection c in distributedCharges)
				{
					poster.Post(c);
				}
				AssertEquals(5, poster.PostedInvoices.Count);
				AssertContainsExactElementsInAnyOrder(new ZString[] { job.JH_JobNum, job.JH_JobNum + "/A", job.JH_JobNum + "/B", job.JH_JobNum + "/D", job.JH_JobNum + "/E" }, poster.PostedInvoices.ToArray<InvoicingBase>().Select(x => x.AH_ConsolidatedInvoiceRef));

				#endregion
			}
		}

		public void TestConsolidatedInvoiceRef_DbLoading()
		{
			if (this.GetType() == typeof(PeriodicInvoiceChargePosterTest))
			{
				Assert(true);
			}
			else
			{
				AssertConsolidatedInvoiceRef_DbLoading(Creator.Job1, true);
				AssertConsolidatedInvoiceRef_DbLoading(Creator.Job2, false);
			}
		}

		void AssertConsolidatedInvoiceRef_DbLoading(Job job, bool isTestingTwoCharges)
		{
			var poster = new ChargePoster(Factory);

			var charge = Creator.CreateCharge(job, Creator.CC1, "C1", Creator.AUD, 0m, null, Creator.AUD, 100m, Creator.ABIGAS);
			Creator.ABIGAS.CompanyData.GenerateARClientNumber();
			Factory.Save();

			#region Post Charges in the Different Factory

			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var creatorInNewFactory = new TestObjectCreator(newFactory);
			var posterInNewFactory = new ChargePoster(newFactory);

			var jobInNewFactory = newFactory.Load<Job>(job.PK);
			var charge1InNewFactory = creatorInNewFactory.CreateCharge(jobInNewFactory, Creator.CC3, "C3", Creator.AUD, 0m, null, Creator.AUD, 100m, Creator.ABIGAS);
			Charge charge2InNewFactory = null;
			if (isTestingTwoCharges)
			{
				charge2InNewFactory = creatorInNewFactory.CreateCharge(jobInNewFactory, Creator.CC4, "C4", Creator.AUD, 0m, null, Creator.AUD, 100m, Creator.AALSHI);
			}
			newFactory.Save();

			AssertEquals(0, posterInNewFactory.PostedInvoices.Count);
			var charges = new IReceivablesPostingChargeCollection();
			charges.Add(charge1InNewFactory);
			if (isTestingTwoCharges)
			{
				charges.Add(charge2InNewFactory);
			}
			var distributedCharges = new PostingChargeDistributor().DistributeCharges(charges);
			if (isTestingTwoCharges)
			{
				AssertEquals("2 collection of charges", 2, distributedCharges.Count);
			}
			else
			{
				AssertEquals("1 collection of charges", 1, distributedCharges.Count);
			}
			foreach (IReceivablesPostingChargeCollection c in distributedCharges)
			{
				posterInNewFactory.Post(c);
			}
			newFactory.Save();
			if (isTestingTwoCharges)
			{
				AssertEquals(2, posterInNewFactory.PostedInvoices.Count);
				AssertContainsExactElementsInAnyOrder(new ZString[] { job.JH_JobNum, job.JH_JobNum + "/A" }, posterInNewFactory.PostedInvoices.ToArray<InvoicingBase>().Select(x => x.AH_ConsolidatedInvoiceRef));
			}
			else
			{
				AssertEquals(1, posterInNewFactory.PostedInvoices.Count);
				AssertContainsExactElementsInAnyOrder(new ZString[] { job.JH_JobNum }, posterInNewFactory.PostedInvoices.ToArray<InvoicingBase>().Select(x => x.AH_ConsolidatedInvoiceRef));
			}

			#endregion

			#region Post Charges in the Original Factory

			AssertEquals(0, poster.PostedInvoices.Count);
			charges = new IReceivablesPostingChargeCollection();
			charges.Add(charge);
			distributedCharges = new PostingChargeDistributor().DistributeCharges(charges);
			AssertEquals("1 collection of charges", 1, distributedCharges.Count);
			foreach (IReceivablesPostingChargeCollection c in distributedCharges)
			{
				poster.Post(c);
			}
			Factory.Save();
			AssertEquals(1, poster.PostedInvoices.Count);
			if (isTestingTwoCharges)
			{
				AssertContainsExactElementsInAnyOrder(new ZString[] { job.JH_JobNum + "/B" }, poster.PostedInvoices.ToArray<InvoicingBase>().Select(x => x.AH_ConsolidatedInvoiceRef));
			}
			else
			{
				AssertContainsExactElementsInAnyOrder(new ZString[] { job.JH_JobNum + "/A" }, poster.PostedInvoices.ToArray<InvoicingBase>().Select(x => x.AH_ConsolidatedInvoiceRef));
			}

			#endregion
		}

		#endregion

		public void TestSellSellReferenceOnARInvoice()
		{
			var creator = new TestObjectCreator(Factory);
			var job1 = creator.Job1;
			job1.JH_OA_AgentCollectAddr = creator.AALSHI.MainAddress.PK;

			ForwardingConsol consol = creator.CreateConsol("AUSYD", "KRSEL", "C00001234");
			consol.SetDefaultReceivingForwarderAddress(creator.Agent);

			var charge1 = creator.CreateCharge(job1, creator.CC1, "CHARGE 1", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.Agent);
			var charge2 = creator.CreateCharge(job1, creator.CC1, "CHARGE 2", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.Agent);
			charge1.JR_OSSellExRate = 0.65m;
			charge2.JR_OSSellExRate = 0.42m;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge1.JR_SellReference = "ABC123";
			charge2.JR_SellReference = "ABC123";

			var distributor = new PostingChargeDistributor();

			var charges = new IReceivablesPostingChargeCollection();
			charges.Add(charge1);
			charges.Add(charge2);

			var results = distributor.DistributeCharges(charges);
			AssertEquals("There should be 1 collections of charges", 1, results.Count);
			AssertEquals("There should be 1 key", 1, results.Keys.Count);
			var key = results.Keys.Cast<PostingChargeKey>().First();

			ChargePoster poster = new ChargePoster(Factory);
			var invoice = poster.Post(results.GetCharges(key));
			AssertEquals("Invoice should have a sell reference", "ABC123", invoice.AH_ChequeOrReference);
		}

		public void TestDistributeAgentCharges_WithOverrideAddressContact()
		{
			var creator = new TestObjectCreator(Factory);
			var job1 = creator.Job1;
			job1.JH_OA_AgentCollectAddr = creator.AALSHI.MainAddress.PK;

			ForwardingConsol consol = creator.CreateConsol("AUSYD", "KRSEL", "C00001234");
			consol.SetDefaultReceivingForwarderAddress(creator.Agent);

			var charge1 = creator.CreateCharge(job1, creator.CC1, "CHARGE 1", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.Agent);
			var charge2 = creator.CreateCharge(job1, creator.CC1, "CHARGE 2", creator.AUD, 100m, creator.ABIGAS, creator.USD, 200m, creator.Agent);
			charge1.JR_OSSellExRate = 0.65m;
			charge2.JR_OSSellExRate = 0.42m;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			creator.CreateAddress(creator.Agent, "0 street");
			creator.CreateContact(creator.Agent, "Alex");

			var address1 = creator.CreateAddress(charge1.SellAccount, "1 street");
			var address2 = creator.CreateAddress(charge2.SellAccount, "2 street");
			var contact1 = creator.CreateContact(charge1.SellAccount, "John");
			var contact2 = creator.CreateContact(charge2.SellAccount, "Bob");
			charge1.JR_OA_SellInvoiceAddress = address1.PK;
			charge1.JR_OC_SellInvoiceContact = contact1.PK;
			charge2.JR_OA_SellInvoiceAddress = address2.PK;
			charge2.JR_OC_SellInvoiceContact = contact2.PK;

			var distributor = new ConsolPostingChargeDistributor(consol);

			var charges = new IReceivablesPostingChargeCollection();
			charges.Add(charge1);
			charges.Add(charge2);
			charges[0].Debtor.MiscServ.OM_FWBillCollectFeesOnSingleInvoice = false;
			charges[1].Debtor.MiscServ.OM_FWBillCollectFeesOnSingleInvoice = false;
			charges[0].DebtorAddressPK = creator.Agent.Addresses[0].PK;
			charges[1].DebtorAddressPK = creator.Agent.Addresses[0].PK;
			charges[0].DebtorContactPK = creator.Agent.Contacts[0].PK;
			charges[1].DebtorContactPK = creator.Agent.Contacts[0].PK;

			var results = distributor.DistributeCharges(charges);
			AssertEquals("Should only be 1 collection of charges", 1, results.Count);

			var key = new PostingChargeKey(consol.ReceivingForwarderPK, "FIN", job1.JH_JobNum, creator.Agent.Addresses[0].PK, creator.Agent.Contacts[0].PK, 0);
			var distributedCharges = results[key];
			AssertNotNull("Should have found relevant invoice", distributedCharges);
			AssertEquals("Should have distributed 2 charges into collection", 2, distributedCharges.Count);

			AssertEquals("Charge1 Address should be updated", creator.Agent.Addresses[0].PK, charge1.JR_OA_SellInvoiceAddress);
			AssertEquals("Charge1 Contact should be updated", creator.Agent.Contacts[0].PK, charge1.JR_OC_SellInvoiceContact);
			AssertEquals("Charge2 Address should be updated", creator.Agent.Addresses[0].PK, charge2.JR_OA_SellInvoiceAddress);
			AssertEquals("Charge2 Contact should be updated", creator.Agent.Contacts[0].PK, charge2.JR_OC_SellInvoiceContact);

			charges.Key = key;
			ChargePoster poster = new ChargePoster(Factory);
			InvoicingBase invoice = poster.Post(charges);

			AssertEquals("Final invoice should have Override Address", creator.Agent.Addresses[0].PK, invoice.AH_OA_InvoiceAddressOverride);
			AssertEquals("Final invoice should have Override Contact", creator.Agent.Contacts[0].PK, invoice.AH_OC_InvoiceContactOverride);
		}

		public void TestPostWithAddressContact()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var charge = job.Charges.AddNew();
			var expectedDebtor = Creator.LocalClient;
			var expectedAddress = Creator.CreateAddress(expectedDebtor);
			var expectedContact = Creator.CreateContact(expectedDebtor);
			charge.JR_OH_SellAccount = expectedDebtor.PK;
			charge.JR_OA_SellInvoiceAddress = expectedAddress.PK;
			charge.JR_OC_SellInvoiceContact = expectedContact.PK;
			InvoicingBase invoice = new ChargePoster(Factory).Post(charge);
			AssertEquals("AH_OH", expectedDebtor.PK, invoice.AH_OH);
			AssertEquals("AH_OA_InvoiceAddressOverride", expectedAddress.PK, invoice.AH_OA_InvoiceAddressOverride);
			AssertEquals("AH_OC_InvoiceContactOverride", expectedContact.PK, invoice.AH_OC_InvoiceContactOverride);
		}

		public void TestOverrideGovtChargeCode()
		{
			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var testObjectCreator = new TestObjectCreator(Factory);
			var chargeCode = testObjectCreator.CreateChargeCode("AAA");
			chargeCode.AC_GovtChargeCode = "GOVT AAA";

			Factory.Save();

			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var charge = job.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_SellGovtChargeCode = "GOVT BBB";
			charge.JR_OH_SellAccount = testObjectCreator.ABIGAS.PK;

			InvoicingBase invoice = new ChargePoster(Factory).Post(charge);
			AssertEquals(1, invoice.Lines.Count);
			AssertEquals(charge.JR_SellGovtChargeCode, invoice.Lines[0].AL_GovtChargeCode);
		}

		public void TestPostWithSellSupplyType()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var chargeCode = testObjectCreator.CreateChargeCode("AAA");
			Factory.Save();

			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var charge = job.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_SellSupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC;
			charge.JR_OH_SellAccount = testObjectCreator.ABIGAS.PK;

			InvoicingBase invoice = new ChargePoster(Factory).Post(charge);
			AssertEquals(1, invoice.Lines.Count);
			AssertEquals(charge.JR_SellSupplyType, invoice.Lines[0].AL_SupplyType);
		}

		public void TestPostWithSellTaxBranch()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var chargeCode = testObjectCreator.CreateChargeCode("AAA");
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			Factory.Save();

			AssertPostWithSellTaxBranch(true);
			AssertPostWithSellTaxBranch(false);

			void AssertPostWithSellTaxBranch(bool enableTaxBranchReporting)
			{
				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableTaxBranchReporting))
				{
					var charge = job.Charges.AddNew();
					charge.JR_AC = chargeCode.PK;
					charge.JR_OH_SellAccount = testObjectCreator.ABIGAS.PK;
					charge.JR_GB_SellTaxBranch = testObjectCreator.NonCurrentBranch.PK;

					var invoice = new ChargePoster(Factory).Post(charge);
					AssertEquals(enableTaxBranchReporting ? testObjectCreator.NonCurrentBranch.PK : ZGuid.Empty, invoice.AH_GB_TaxBranch);
					AssertEquals(1, invoice.Lines.Count);
					AssertEquals(enableTaxBranchReporting ? testObjectCreator.NonCurrentBranch.PK : ZGuid.Empty, invoice.Lines[0].AL_GB_TaxBranch);
				}
			}
		}

		#region Transaction Line Tests

		void AssertPostDateNow(InvoicingLineBase line)
		{
			Assert("Post Date", line.AL_PostDate <= ZDateTime.Now.AddMinutes(1) && line.AL_PostDate >= ZDateTime.Now.AddMinutes(-1));
		}

		public void TestPostBillInLocalCalculatesGSTANDEDUCorrectlyForChargeWithForeignSellCurrency()
		{
			Job job = CreateJob("Z00001011", AALSHI, true, 0, ABIGAS, true, 0);
			AccTaxRate gstAndEdu = new TestObjectCreator(Factory).CreateTaxRate("GSTNEDU", "GST and EDU", AccTaxRate.Types.Rated, 10, AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax, 75, 10);
			ExchangeRate rate = job.ExchangeRates.AddNew();
			rate.JF_RX_NKRateCurrency = USD.RX_Code;
			rate.JF_BaseRate = 0.02m;
			AALSHI.CompanyData.SetARTaxApplicable(true);

			Charge charge1 = CreateCharge(job, MRG100, "Revenue Transaction Test", AUD, 0, ZECTRA, USD, 800, AALSHI);
			charge1.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge1.JR_GB = GlbBranch.CurrentBranch.PK;
			charge1.JR_AT_SellGSTRate = gstAndEdu.PK;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			Charge charge2 = CreateCharge(job, MRG100, "Revenue Transaction Test", AUD, 0, ZECTRA, AUD, 39565.5, AALSHI);
			charge2.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge2.JR_GB = GlbBranch.CurrentBranch.PK;
			charge2.JR_AT_SellGSTRate = gstAndEdu.PK;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			AssertEquals("charge1: Default Tax Amount", 86m, charge1.JR_OSSellGSTAmt_Calc);
			AssertEquals("charge2: Default Tax Amount", 4253.29m, charge2.JR_OSSellGSTAmt_Calc);

			IReceivablesPostingChargeCollection collection = new IReceivablesPostingChargeCollection();
			collection.Key = new PostingChargeKey(AALSHI.PK, InvoiceTypesList.Codes.FinalInvoice, ZGuid.Empty, ZGuid.Empty, 0);
			collection.Add(charge1);
			collection.Add(charge2);

			InvoicingBase invoice = GetChargePoster().Post(collection);
			InvoicingLineBase line1 = (InvoicingLineBase)invoice.Lines.FindByPK(charge1.JR_AL_ARLine);
			AssertEquals("line1: Local Tax Amount", 4300m, line1.AL_LocalTaxAmount);
			AssertEquals("line1: OS Tax Amount", 4300m, line1.AL_OSTaxAmount);

			InvoicingLineBase line2 = (InvoicingLineBase)invoice.Lines.FindByPK(charge2.JR_AL_ARLine);
			AssertEquals("line2: Local Tax Amount", 4253.29m, line2.AL_LocalTaxAmount);
			AssertEquals("line2: OS Tax Amount", 4253.29m, line2.AL_OSTaxAmount);
		}

		[TestDate(2020, 10, 06)]
		public void TestPost_SetLineTaxDate_UseInvoiceDate()
		{
			AssertPost_SetTaxDateBasedOnRegistry(true);
		}

		[TestDate(2020, 10, 06)]
		public void TestPost_SetLineTaxDate_UseTodayDate()
		{
			AssertPost_SetTaxDateBasedOnRegistry(false);
		}

		void AssertPost_SetTaxDateBasedOnRegistry(bool useInvoiceDate)
		{
			var creator = new TestObjectCreator(Factory);

			var configDateTime = ZDateTime.Now.AddMonths(1).ToDateTime();
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.MonthEndSuspension.Code);
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, configDateTime);

			var shipment = creator.CreateShipment("S00001000");
			var job = creator.CreateJob(shipment, false);
			var charge1 = CreateCharge(job, MRG100, "Revenue Transaction Test", AUD, 250M, ZECTRA, AUD, 250M, AALSHI);
			charge1.JR_GE = ZGuid.NewZGuid();
			charge1.JR_GB = ZGuid.NewZGuid();
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge1.JR_AT_SellGSTRate = GST1.PK;
			charge1.JR_SellTaxDate = ZDate.Empty;

			var charge2 = CreateCharge(job, MRG100, "Revenue Transaction Test", AUD, 250M, ZECTRA, AUD, 250M, AALSHI);
			charge2.JR_GE = ZGuid.NewZGuid();
			charge2.JR_GB = ZGuid.NewZGuid();
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge2.JR_AT_SellGSTRate = GST2.PK;
			charge2.JR_SellTaxDate = new ZDate(2020, 10, 15);

			var collection = new TaxDateDefaultingOptionCollection();
			var taxDateOption = collection.AddNew();
			taxDateOption.JobType = "ALL";
			taxDateOption.Ledger = "ALL";
			taxDateOption.DirectionCode = "ALL";
			taxDateOption.Mode = "ALL";
			taxDateOption.TaxDateOption = useInvoiceDate ? "INV" : "TDY";

			using (AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
			{
				var chargePoster = new ChargePoster(Factory);
				var invoice = chargePoster.Post(charge1);
				var invoiceLine1 = invoice.Lines[0];
				if (useInvoiceDate)
				{
					AssertEquals("Expect to use invoice date when the tax date was blank and registry is set to INV", new ZDate(2020, 11, 30), invoiceLine1.AL_TaxDate);
					AssertEquals("Expect same tax date is updated to the charge", new ZDate(2020, 11, 30), charge1.JR_SellTaxDate);
				}
				else
				{
					AssertEquals("Expect to use today's date when the tax date was blank and registry is set to INV", new ZDate(2020, 10, 6), invoiceLine1.AL_TaxDate);
					AssertEquals("Expect same tax date is updated to the charge", new ZDate(2020, 10, 6), charge1.JR_SellTaxDate);
				}

				var chargePoster2 = new ChargePoster(Factory);
				var invoice2 = chargePoster2.Post(charge2);
				var invoiceLine2 = invoice2.Lines[0];
				AssertEquals("Expect registry is ignored when the tax date was pre-set on charge", new ZDate(2020, 10, 15), invoiceLine2.AL_TaxDate);
				AssertEquals("Expect no change is made to the tax date on the charge", new ZDate(2020, 10, 15), charge2.JR_SellTaxDate);
			}
		}

		[TestDate(2020, 10, 06)]
		public void TestBackDatePost_SetLineTaxDate_UseInvoiceDate()
		{
			AssertBackDatePost_SetTaxDateBasedOnRegistrySetting(true);
		}

		[TestDate(2020, 10, 06)]
		public void TestBackDatePost_SetLineTaxDate_UseTodayDate()
		{
			AssertBackDatePost_SetTaxDateBasedOnRegistrySetting(false);
		}

		void AssertBackDatePost_SetTaxDateBasedOnRegistrySetting(bool useInvoiceDate)
		{
			var testShipment = Factory.New<ForwardingShipment>();
			var job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			job.PlugInData = testShipment;

			AALSHI.CompanyData.SetARTaxApplicable(true);
			AALSHI.MiscServ.OM_ARWHTApplicable = true;

			var charge = CreateCharge(job, MRG100, "Revenue Transaction Test", AUD, 250M, ZECTRA, AUD, 250M, AALSHI);
			charge.JR_GE = ZGuid.NewZGuid();
			charge.JR_GB = ZGuid.NewZGuid();
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_AT_SellGSTRate = GST1.PK;
			charge.JR_SellTaxDate = ZDate.Empty;

			var collection = new TaxDateDefaultingOptionCollection();
			var taxDateOption = collection.AddNew();
			taxDateOption.JobType = "ALL";
			taxDateOption.DirectionCode = "ALL";
			taxDateOption.Mode = "ALL";
			taxDateOption.Ledger = "AR";
			taxDateOption.TaxDateOption = useInvoiceDate ? "INV" : "TDY";

			using (AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
			{
				var poster = GetChargePoster();
				var invoice = poster.Post(charge);
				var invoiceLine = invoice.Lines[0];
				AssertEquals(new ZDate(2020, 10, 6), invoiceLine.AL_TaxDate);
				var invoiceDate = new ZDate(2020, 10, 1);
				poster.ChangeTransactionDateOnAllARInvoicesAndCFXLines(invoiceDate, invoiceDate);
				if (useInvoiceDate)
				{
					AssertEquals(new ZDate(2020, 10, 1), invoiceLine.AL_TaxDate);
					AssertEquals(new ZDate(2020, 10, 1), charge.JR_SellTaxDate);
				}
				else
				{
					AssertEquals(new ZDate(2020, 10, 6), invoiceLine.AL_TaxDate);
					AssertEquals(new ZDate(2020, 10, 6), charge.JR_SellTaxDate);
				}
			}
		}

		public void TestChargePost_SetTaxDateBasedOnRegistrySetting_PickupDate()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var testShipment = testObjectCreator.CreateShipment("S001", "AUSYD", "USLAX");
			testShipment.DocsAndCartage.JP_EstimatedPickup = new ZDate(2020, 01, 1);
			testShipment.DocsAndCartage.JP_PickupCartageCompleted = new ZDate(2020, 03, 1);
			var job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			job.PlugInData = testShipment;

			AALSHI.CompanyData.SetARTaxApplicable(true);
			AALSHI.MiscServ.OM_ARWHTApplicable = true;

			AssertChargePost_SetTaxDateBasedOnRegistrySetting(job, TaxDateDefaultingOption.Code.PickupDate, new ZDate(2020, 03, 1));
			testShipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Empty;
			AssertChargePost_SetTaxDateBasedOnRegistrySetting(job, TaxDateDefaultingOption.Code.PickupDate, new ZDate(2020, 01, 1));
		}

		public void TestChargePost_SetTaxDateBasedOnRegistrySetting_DeliveryDate()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var testShipment = testObjectCreator.CreateShipment("S001", "AUSYD", "USLAX");
			testShipment.DocsAndCartage.JP_EstimatedDelivery = new ZDate(2020, 01, 1);
			testShipment.DocsAndCartage.JP_DeliveryCartageCompleted = new ZDate(2020, 03, 1);
			var job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			job.PlugInData = testShipment;

			AALSHI.CompanyData.SetARTaxApplicable(true);
			AALSHI.MiscServ.OM_ARWHTApplicable = true;

			AssertChargePost_SetTaxDateBasedOnRegistrySetting(job, TaxDateDefaultingOption.Code.DeliveryDate, new ZDate(2020, 03, 1));
			testShipment.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Empty;
			AssertChargePost_SetTaxDateBasedOnRegistrySetting(job, TaxDateDefaultingOption.Code.DeliveryDate, new ZDate(2020, 01, 1));
		}

		void AssertChargePost_SetTaxDateBasedOnRegistrySetting(Job job, ZString taxDateDefaultingOption, ZDate expectTaxDate)
		{
			var charge = CreateCharge(job, MRG100, "Revenue Transaction Test", AUD, 250M, ZECTRA, AUD, 250M, AALSHI);
			charge.JR_GE = ZGuid.NewZGuid();
			charge.JR_GB = ZGuid.NewZGuid();
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_AT_SellGSTRate = GST1.PK;
			charge.JR_SellTaxDate = ZDate.Empty;

			var collection = new TaxDateDefaultingOptionCollection();
			var taxDateOption = collection.AddNew();
			taxDateOption.JobType = "SHP";
			taxDateOption.DirectionCode = "ALL";
			taxDateOption.Mode = "ALL";
			taxDateOption.Ledger = "AR";
			taxDateOption.TaxDateOption = taxDateDefaultingOption;

			using (AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
			{
				var poster = GetChargePoster();
				var invoice = poster.Post(charge);
				var invoiceLine = invoice.Lines[0];
				AssertEquals(expectTaxDate, invoiceLine.AL_TaxDate);
				AssertEquals(expectTaxDate, charge.JR_SellTaxDate);
			}
		}

		public void TestPostBillInLocalUsesGSTAmountFromCharge()
		{
			Job job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);

			AALSHI.CompanyData.SetARTaxApplicable(true);
			AALSHI.MiscServ.OM_ARWHTApplicable = true;
			Charge charge = CreateCharge(job, MRG100, "Revenue Transaction Test", AUD, 250M, ZECTRA, AUD, 250M, AALSHI);
			charge.JR_GE = ZGuid.NewZGuid();
			charge.JR_GB = ZGuid.NewZGuid();
			ZShort sequence = 300;
			charge.JR_DisplaySequence = sequence;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			var expectedDate = ZDate.Today.AddDays(4);
			charge.JR_SellTaxDate = expectedDate;

			AssertNotNull("Precondition: Tax Rate should not be null", charge.SellGSTRate);
			AssertEquals("Precondition: Tax Rate ", 10m, charge.SellGSTRate.GetRate_ForTestOnly());
			AssertEquals("Default Tax Amount", 25m, charge.JR_OSSellGSTAmt_Calc);

			InvoicingBase invoice = GetChargePoster().Post(charge);
			InvoicingLineBase invoiceLine = invoice.Lines[0];

			AssertEquals("Line Type", TransactionLineTypes.Revenue, invoiceLine.AL_LineType);
			AssertEquals("Sequence", sequence, invoiceLine.AL_Sequence);
			AssertEquals("Description", "Revenue Transaction Test", invoiceLine.AL_Desc);
			AssertEquals("Unit Quantity", 0, invoiceLine.AL_UnitQty);
			AssertEquals("Unit Price", 0M, invoiceLine.AL_UnitPrice);
			AssertEquals("OS Unit Price", 0M, invoiceLine.AL_OSUnitPrice);
			AssertEquals("Currency", AUD.RX_Code, invoiceLine.AL_RX_NKTransactionCurrency);
			AssertEquals("Exchange Rate", 1M, invoiceLine.AL_ExchangeRate);
			AssertEquals("Post Period", 0, invoiceLine.AL_PostPeriod);
			AssertPostDateNow(invoiceLine);
			AssertEquals("Post To GL", "N", invoiceLine.AL_PostToGL);
			AssertEquals("Reverse Period", 0, invoiceLine.AL_ReversePeriod);
			AssertEquals("Reverse Date", ZDateTime.Empty, invoiceLine.AL_ReverseDate);
			AssertEquals("Reverse to GL", "N", invoiceLine.AL_ReverseToGL);
			AssertEquals("Prevent Invoice Print Grouping", ZBool.False, invoiceLine.AL_PreventInvoicePrintGrouping);
			AssertEquals("Transaction Header PK", invoice.PK, invoiceLine.AL_AH);
			AssertEquals("Job Header", job.PK, invoiceLine.AL_JH);
			AssertEquals("Charge Code", MRG100.PK, invoiceLine.AL_AC);
			AssertEquals("Department", charge.JR_GE, invoiceLine.AL_GE);
			AssertEquals("Branch", charge.JR_GB, invoiceLine.AL_GB);
			AssertEquals("General Ledger", Creator.GLHeader1.PK, invoiceLine.AL_AG);
			AssertEquals("OrgHeader", AALSHI.PK, invoiceLine.AL_OH);
			AssertEquals("GL Percent Of", ZGuid.Empty, invoiceLine.AL_AG_PercentOf);
			AssertEquals("Percentage of Period", 0, invoiceLine.AL_PercentageOfPeriod);

			AssertEquals("OS Ex Tax Amount", 250M, invoiceLine.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 25M, invoiceLine.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 12.5M, invoiceLine.AL_OSWHTAmount);
			AssertEquals("OS Total Amount", 275M, invoiceLine.AL_OverseasTotal);

			AssertEquals("Local Ex Amount", 250M, invoiceLine.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount", 25M, invoiceLine.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount", 12.5M, invoiceLine.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount", 275M, invoiceLine.AL_LocalTotalAmount);

			AssertEquals("Line Amount", 250M, invoiceLine.AL_LineAmount);
			AssertEquals("GST Tax ID", GST1.PK, invoiceLine.AL_AT);
			AssertEquals("GST Tax Date", expectedDate, invoiceLine.AL_TaxDate);
			AssertEquals("Tax Msg ID", InvMsg1.PK, invoiceLine.AL_A9_VATClass);
			AssertEquals("GST Amount", 25M, invoiceLine.AL_GSTVAT);
			AssertEquals("WHT Tax ID", WHT1.PK, invoiceLine.AL_AW);
			AssertEquals("Withholding Tax", 12.5M, invoiceLine.AL_WithholdingTax);
			AssertEquals("OS Amount", 275M, invoiceLine.AL_OSAmount);
		}

		public void TestPostBillInLocalGSTApplicableWHTApplicable()
		{
			Job job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			CreateExchangeRate(job, USD, .7M);

			AALSHI.CompanyData.SetARTaxApplicable(true);
			AALSHI.MiscServ.OM_ARWHTApplicable = true;
			Charge charge = CreateCharge(job, MRG100, "Revenue Transaction Test", USD, 250M, ZECTRA, USD, 350M, AALSHI);
			charge.JR_GE = ZGuid.NewZGuid();
			charge.JR_GB = ZGuid.NewZGuid();
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			ChargePoster poster = GetChargePoster();
			InvoicingBase invoice = poster.Post(charge);
			InvoicingLineBase invoiceLine = invoice.Lines[0];

			AssertEquals("Line Type", TransactionLineTypes.Revenue, invoiceLine.AL_LineType);
			AssertEquals("Sequence", (short)1, invoiceLine.AL_Sequence);
			AssertEquals("Description", "Revenue Transaction Test", invoiceLine.AL_Desc);
			AssertEquals("Unit Quantity", 0, invoiceLine.AL_UnitQty);
			AssertEquals("Unit Price", 0M, invoiceLine.AL_UnitPrice);
			AssertEquals("OS Unit Price", 0M, invoiceLine.AL_OSUnitPrice);
			AssertEquals("Currency", AUD.RX_Code, invoiceLine.AL_RX_NKTransactionCurrency);
			if (poster.OverrideChargeExchangeRatesOnPosting)
			{
				AssertEquals("Exchange Rate", 1M, invoiceLine.AL_ExchangeRate);
			}
			AssertEquals("Post Period", 0, invoiceLine.AL_PostPeriod);
			AssertPostDateNow(invoiceLine);
			AssertEquals("Post To GL", "N", invoiceLine.AL_PostToGL);
			AssertEquals("Reverse Period", 0, invoiceLine.AL_ReversePeriod);
			AssertEquals("Reverse Date", ZDateTime.Empty, invoiceLine.AL_ReverseDate);
			AssertEquals("Reverse to GL", "N", invoiceLine.AL_ReverseToGL);
			AssertEquals("Prevent Invoice Print Grouping", ZBool.False, invoiceLine.AL_PreventInvoicePrintGrouping);
			AssertEquals("Transaction Header PK", invoice.PK, invoiceLine.AL_AH);
			AssertEquals("Job Header", job.PK, invoiceLine.AL_JH);
			AssertEquals("Charge Code", MRG100.PK, invoiceLine.AL_AC);
			AssertEquals("Department", charge.JR_GE, invoiceLine.AL_GE);
			AssertEquals("Branch", charge.JR_GB, invoiceLine.AL_GB);
			AssertEquals("General Ledger", Creator.GLHeader1.PK, invoiceLine.AL_AG);
			AssertEquals("OrgHeader", AALSHI.PK, invoiceLine.AL_OH);
			AssertEquals("GL Percent Of", ZGuid.Empty, invoiceLine.AL_AG_PercentOf);
			AssertEquals("Percentage of Period", 0, invoiceLine.AL_PercentageOfPeriod);

			AssertEquals("OS Ex Tax Amount", 526.32M, invoiceLine.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 52.63M, invoiceLine.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 26.32M, invoiceLine.AL_OSWHTAmount);
			AssertEquals("OS Total Amount", 578.95M, invoiceLine.AL_OverseasTotal);

			AssertEquals("Local Ex Amount", 526.32M, invoiceLine.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount", 52.63M, invoiceLine.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount", 26.32M, invoiceLine.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount", 578.95M, invoiceLine.AL_LocalTotalAmount);

			AssertEquals("Line Amount", 526.32M, invoiceLine.AL_LineAmount);
			AssertEquals("GST Tax ID", GST1.PK, invoiceLine.AL_AT);
			AssertEquals("Tax Msg ID", InvMsg1.PK, invoiceLine.AL_A9_VATClass);
			AssertEquals("GST Amount", 52.63M, invoiceLine.AL_GSTVAT);
			AssertEquals("WHT Tax ID", WHT1.PK, invoiceLine.AL_AW);
			AssertEquals("Withholding Tax", 26.32M, invoiceLine.AL_WithholdingTax);
			AssertEquals("OS Amount", 578.95M, invoiceLine.AL_OSAmount);
		}

		public void TestPostBillInLocalCurrencyGSTNotApplicableWHTApplicable()
		{
			Job job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			CreateExchangeRate(job, USD, .7M);

			ABIGAS.CompanyData.SetARTaxApplicable(false);
			ABIGAS.MiscServ.OM_ARWHTApplicable = true;
			Charge charge = CreateCharge(job, CCODE1, "Revenue Transaction Test", null, 0M, null, USD, 350M, ABIGAS);
			charge.JR_GB = ZGuid.NewZGuid();
			charge.JR_GE = ZGuid.NewZGuid();
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			ChargePoster poster = GetChargePoster();
			InvoicingBase invoice = poster.Post(charge);
			InvoicingLineBase invoiceLine = invoice.Lines[0];

			AssertEquals("Line Type", TransactionLineTypes.Revenue, invoiceLine.AL_LineType);
			AssertEquals("Sequence", (short)1, invoiceLine.AL_Sequence);
			AssertEquals("Description", "Revenue Transaction Test", invoiceLine.AL_Desc);
			AssertEquals("Unit Quantity", 0, invoiceLine.AL_UnitQty);
			AssertEquals("Unit Price", 0M, invoiceLine.AL_UnitPrice);
			AssertEquals("OS Unit Price", 0M, invoiceLine.AL_OSUnitPrice);
			AssertEquals("Currency", AUD.RX_Code, invoiceLine.AL_RX_NKTransactionCurrency);
			if (poster.OverrideChargeExchangeRatesOnPosting)
			{
				AssertEquals("Exchange Rate", 1M, invoiceLine.AL_ExchangeRate);
			}
			AssertEquals("Post Period", 0, invoiceLine.AL_PostPeriod);
			AssertPostDateNow(invoiceLine);
			AssertEquals("Post To GL", "N", invoiceLine.AL_PostToGL);
			AssertEquals("Reverse Period", 0, invoiceLine.AL_ReversePeriod);
			AssertEquals("Reverse Date", ZDateTime.Empty, invoiceLine.AL_ReverseDate);
			AssertEquals("Reverse to GL", "N", invoiceLine.AL_ReverseToGL);
			AssertEquals("Prevent Invoice Print Grouping", ZBool.False, invoiceLine.AL_PreventInvoicePrintGrouping);
			AssertEquals("Transaction Header PK", invoice.PK, invoiceLine.AL_AH);
			AssertEquals("Job Header", job.PK, invoiceLine.AL_JH);
			AssertEquals("Charge Code", CCODE1.PK, invoiceLine.AL_AC);
			AssertEquals("Department", charge.JR_GE, invoiceLine.AL_GE);
			AssertEquals("Branch", charge.JR_GB, invoiceLine.AL_GB);
			AssertEquals("General Ledger", Creator.GLHeader1.PK, invoiceLine.AL_AG);
			AssertEquals("OrgHeader", ABIGAS.PK, invoiceLine.AL_OH);
			AssertEquals("GL Percent Of", ZGuid.Empty, invoiceLine.AL_AG_PercentOf);
			AssertEquals("Percentage of Period", 0, invoiceLine.AL_PercentageOfPeriod);

			AssertEquals("OS Ex Tax Amount", 555.56M, invoiceLine.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 0M, invoiceLine.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 27.78M, invoiceLine.AL_OSWHTAmount);
			AssertEquals("OS Total Amount", 555.56M, invoiceLine.AL_OverseasTotal);

			AssertEquals("Local Ex Amount", 555.56M, invoiceLine.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount", 0M, invoiceLine.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount", 27.78M, invoiceLine.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount", 555.56M, invoiceLine.AL_LocalTotalAmount);

			AssertEquals("Line Amount", 555.56M, invoiceLine.AL_LineAmount);
			AssertEquals("GST Tax ID", ZGuid.Empty, invoiceLine.AL_AT);
			AssertEquals("Tax Msg ID", ZGuid.Empty, invoiceLine.AL_A9_VATClass);
			AssertEquals("GST Amount", 0M, invoiceLine.AL_GSTVAT);
			AssertEquals("WHT Tax ID", WHT1.PK, invoiceLine.AL_AW);
			AssertEquals("Withholding Tax", 27.78M, invoiceLine.AL_WithholdingTax);
			AssertEquals("OS Amount", 555.56M, invoiceLine.AL_OSAmount);
		}

		public void TestPostBillInLocalGSTApplicableWHTNotApplicable()
		{
			Job job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			CreateExchangeRate(job, USD, .7M);

			AALSHI.CompanyData.SetARTaxApplicable(true);
			AALSHI.MiscServ.OM_ARWHTApplicable = false;
			Charge charge = CreateCharge(job, MRG100, "Revenue Transaction Test", USD, 250M, ZECTRA, USD, 350M, AALSHI);
			charge.JR_GB = ZGuid.NewZGuid();
			charge.JR_GE = ZGuid.NewZGuid();
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			InvoicingBase invoice = GetChargePoster().Post(charge);
			InvoicingLineBase invoiceLine = invoice.Lines[0];

			AssertEquals("Line Type", TransactionLineTypes.Revenue, invoiceLine.AL_LineType);
			AssertEquals("Sequence", (short)1, invoiceLine.AL_Sequence);
			AssertEquals("Description", "Revenue Transaction Test", invoiceLine.AL_Desc);
			AssertEquals("Unit Quantity", 0, invoiceLine.AL_UnitQty);
			AssertEquals("Unit Price", 0M, invoiceLine.AL_UnitPrice);
			AssertEquals("OS Unit Price", 0M, invoiceLine.AL_OSUnitPrice);
			AssertEquals("Currency", AUD.RX_Code, invoiceLine.AL_RX_NKTransactionCurrency);
			AssertEquals("Exchange Rate", 1M, invoiceLine.AL_ExchangeRate);
			AssertEquals("Post Period", 0, invoiceLine.AL_PostPeriod);
			AssertPostDateNow(invoiceLine);
			AssertEquals("Post To GL", "N", invoiceLine.AL_PostToGL);
			AssertEquals("Reverse Period", 0, invoiceLine.AL_ReversePeriod);
			AssertEquals("Reverse Date", ZDateTime.Empty, invoiceLine.AL_ReverseDate);
			AssertEquals("Reverse to GL", "N", invoiceLine.AL_ReverseToGL);
			AssertEquals("Prevent Invoice Print Grouping", ZBool.False, invoiceLine.AL_PreventInvoicePrintGrouping);
			AssertEquals("Transaction Header PK", invoice.PK, invoiceLine.AL_AH);
			AssertEquals("Job Header", job.PK, invoiceLine.AL_JH);
			AssertEquals("Charge Code", MRG100.PK, invoiceLine.AL_AC);
			AssertEquals("Department", charge.JR_GE, invoiceLine.AL_GE);
			AssertEquals("Branch", charge.JR_GB, invoiceLine.AL_GB);
			AssertEquals("General Ledger", Creator.GLHeader1.PK, invoiceLine.AL_AG);
			AssertEquals("OrgHeader", AALSHI.PK, invoiceLine.AL_OH);
			AssertEquals("GL Percent Of", ZGuid.Empty, invoiceLine.AL_AG_PercentOf);
			AssertEquals("Percentage of Period", 0, invoiceLine.AL_PercentageOfPeriod);

			AssertEquals("OS Ex Tax Amount", 526.32M, invoiceLine.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 52.63M, invoiceLine.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 0M, invoiceLine.AL_OSWHTAmount);
			AssertEquals("OS Total Amount", 578.95M, invoiceLine.AL_OverseasTotal);

			AssertEquals("Local Ex Amount", 526.32M, invoiceLine.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount", 52.63M, invoiceLine.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount", 0M, invoiceLine.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount", 578.95M, invoiceLine.AL_LocalTotalAmount);

			AssertEquals("Line Amount", 526.32M, invoiceLine.AL_LineAmount);
			AssertEquals("GST Tax ID", GST1.PK, invoiceLine.AL_AT);
			AssertEquals("GST Tax Date", ZDate.Today, invoiceLine.AL_TaxDate);
			AssertEquals("Tax Msg ID", InvMsg1.PK, invoiceLine.AL_A9_VATClass);
			AssertEquals("GST Amount", 52.63M, invoiceLine.AL_GSTVAT);
			AssertEquals("WHT Tax ID", ZGuid.Empty, invoiceLine.AL_AW);
			AssertEquals("Withholding Tax", 0M, invoiceLine.AL_WithholdingTax);
			AssertEquals("OS Amount", 578.95M, invoiceLine.AL_OSAmount);
		}

		public void TestPostBillInLocalGSTNotApplicableWHTNotApplicable()
		{
			Job job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			CreateExchangeRate(job, USD, .7M);

			ABIGAS.CompanyData.SetARTaxApplicable(false);
			ABIGAS.MiscServ.OM_ARWHTApplicable = false;
			Charge charge = CreateCharge(job, CCODE1, "Revenue Transaction Test", null, 0M, null, USD, 350M, ABIGAS);
			charge.JR_GB = ZGuid.NewZGuid();
			charge.JR_GE = ZGuid.NewZGuid();
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			InvoicingBase invoice = GetChargePoster().Post(charge);
			InvoicingLineBase invoiceLine = invoice.Lines[0];

			AssertEquals("Line Type", TransactionLineTypes.Revenue, invoiceLine.AL_LineType);
			AssertEquals("Sequence", (short)1, invoiceLine.AL_Sequence);
			AssertEquals("Description", "Revenue Transaction Test", invoiceLine.AL_Desc);
			AssertEquals("Unit Quantity", 0, invoiceLine.AL_UnitQty);
			AssertEquals("Unit Price", 0M, invoiceLine.AL_UnitPrice);
			AssertEquals("OS Unit Price", 0M, invoiceLine.AL_OSUnitPrice);
			AssertEquals("Currency", AUD.RX_Code, invoiceLine.AL_RX_NKTransactionCurrency);
			AssertEquals("Exchange Rate", 1M, invoiceLine.AL_ExchangeRate);
			AssertEquals("Post Period", 0, invoiceLine.AL_PostPeriod);
			AssertPostDateNow(invoiceLine);
			AssertEquals("Post To GL", "N", invoiceLine.AL_PostToGL);
			AssertEquals("Reverse Period", 0, invoiceLine.AL_ReversePeriod);
			AssertEquals("Reverse Date", ZDateTime.Empty, invoiceLine.AL_ReverseDate);
			AssertEquals("Reverse to GL", "N", invoiceLine.AL_ReverseToGL);
			AssertEquals("Prevent Invoice Print Grouping", ZBool.False, invoiceLine.AL_PreventInvoicePrintGrouping);
			AssertEquals("Transaction Header PK", invoice.PK, invoiceLine.AL_AH);
			AssertEquals("Job Header", job.PK, invoiceLine.AL_JH);
			AssertEquals("Charge Code", CCODE1.PK, invoiceLine.AL_AC);
			AssertEquals("Department", charge.JR_GE, invoiceLine.AL_GE);
			AssertEquals("Branch", charge.JR_GB, invoiceLine.AL_GB);
			AssertEquals("General Ledger", Creator.GLHeader1.PK, invoiceLine.AL_AG);
			AssertEquals("OrgHeader", ABIGAS.PK, invoiceLine.AL_OH);
			AssertEquals("GL Percent Of", ZGuid.Empty, invoiceLine.AL_AG_PercentOf);
			AssertEquals("Percentage of Period", 0, invoiceLine.AL_PercentageOfPeriod);

			AssertEquals("OS Ex Tax Amount", 555.56M, invoiceLine.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 0M, invoiceLine.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 0M, invoiceLine.AL_OSWHTAmount);
			AssertEquals("OS Total Amount", 555.56M, invoiceLine.AL_OverseasTotal);

			AssertEquals("Local Ex Amount", 555.56M, invoiceLine.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount", 0M, invoiceLine.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount", 0M, invoiceLine.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount", 555.56M, invoiceLine.AL_LocalTotalAmount);

			AssertEquals("Line Amount", 555.56M, invoiceLine.AL_LineAmount);
			AssertEquals("GST Tax ID", ZGuid.Empty, invoiceLine.AL_AT);
			AssertEquals("Tax Msg ID", ZGuid.Empty, invoiceLine.AL_A9_VATClass);
			AssertEquals("GST Amount", 0M, invoiceLine.AL_GSTVAT);
			AssertEquals("WHT Tax ID", ZGuid.Empty, invoiceLine.AL_AW);
			AssertEquals("Withholding Tax", 0M, invoiceLine.AL_WithholdingTax);
			AssertEquals("OS Amount", 555.56M, invoiceLine.AL_OSAmount);
		}

		public void TestPostBillInForeignGSTApplicableWHTApplicable()
		{
			Job job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			CreateExchangeRate(job, USD, .7M);

			ABIGAS.CompanyData.SetARTaxApplicable(true);
			ABIGAS.MiscServ.OM_ARWHTApplicable = true;
			Charge charge = CreateCharge(job, MRG100, "Revenue Transaction Test", USD, 250M, ZECTRA, USD, 350M, ABIGAS);
			charge.JR_GB = ZGuid.NewZGuid();
			charge.JR_GE = ZGuid.NewZGuid();
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FreightInvoice;

			InvoicingBase invoice = GetChargePoster().Post(charge);
			InvoicingLineBase invoiceLine = invoice.Lines[0];

			AssertEquals("Line Type", TransactionLineTypes.Revenue, invoiceLine.AL_LineType);
			AssertEquals("Sequence", (short)1, invoiceLine.AL_Sequence);
			AssertEquals("Description", "Revenue Transaction Test", invoiceLine.AL_Desc);
			AssertEquals("Unit Quantity", 0, invoiceLine.AL_UnitQty);
			AssertEquals("Unit Price", 0M, invoiceLine.AL_UnitPrice);
			AssertEquals("OS Unit Price", 0M, invoiceLine.AL_OSUnitPrice);
			AssertEquals("Currency", USD.RX_Code, invoiceLine.AL_RX_NKTransactionCurrency);
			AssertEquals("Exchange Rate", 0.7M, invoiceLine.AL_ExchangeRate);
			AssertEquals("Post Period", 0, invoiceLine.AL_PostPeriod);
			AssertPostDateNow(invoiceLine);
			AssertEquals("Post To GL", "N", invoiceLine.AL_PostToGL);
			AssertEquals("Reverse Period", 0, invoiceLine.AL_ReversePeriod);
			AssertEquals("Reverse Date", ZDateTime.Empty, invoiceLine.AL_ReverseDate);
			AssertEquals("Reverse to GL", "N", invoiceLine.AL_ReverseToGL);
			AssertEquals("Prevent Invoice Print Grouping", ZBool.False, invoiceLine.AL_PreventInvoicePrintGrouping);
			AssertEquals("Transaction Header PK", invoice.PK, invoiceLine.AL_AH);
			AssertEquals("Job Header", job.PK, invoiceLine.AL_JH);
			AssertEquals("Charge Code", MRG100.PK, invoiceLine.AL_AC);
			AssertEquals("Department", charge.JR_GE, invoiceLine.AL_GE);
			AssertEquals("Branch", charge.JR_GB, invoiceLine.AL_GB);
			AssertEquals("General Ledger", Creator.GLHeader1.PK, invoiceLine.AL_AG);
			AssertEquals("OrgHeader", ABIGAS.PK, invoiceLine.AL_OH);
			AssertEquals("GL Percent Of", ZGuid.Empty, invoiceLine.AL_AG_PercentOf);
			AssertEquals("Percentage of Period", 0, invoiceLine.AL_PercentageOfPeriod);

			AssertEquals("OS Ex Tax Amount", 350.00M, invoiceLine.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 35.00M, invoiceLine.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 17.50M, invoiceLine.AL_OSWHTAmount);
			AssertEquals("OS Total Amount", 385.00M, invoiceLine.AL_OverseasTotal);

			AssertEquals("Local Ex Amount", 500.00M, invoiceLine.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount", 50.00M, invoiceLine.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount", 25.00M, invoiceLine.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount", 550.00M, invoiceLine.AL_LocalTotalAmount);

			AssertEquals("Line Amount", 500.00M, invoiceLine.AL_LineAmount);
			AssertEquals("GST Tax ID", GST1.PK, invoiceLine.AL_AT);
			AssertEquals("Tax MSG ID", InvMsg1.PK, invoiceLine.AL_A9_VATClass);
			AssertEquals("GST Amount", 50.00M, invoiceLine.AL_GSTVAT);
			AssertEquals("WHT Tax ID", WHT1.PK, invoiceLine.AL_AW);
			AssertEquals("Withholding Tax", 25.00M, invoiceLine.AL_WithholdingTax);
			AssertEquals("OS Amount", 385.00M, invoiceLine.AL_OSAmount);
		}

		public void TestPostBillInForeignGSTNotApplicableWHTApplicable()
		{
			Job job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			CreateExchangeRate(job, USD, .7M);

			AALSHI.CompanyData.SetARTaxApplicable(false);
			AALSHI.MiscServ.OM_ARWHTApplicable = true;
			Charge charge = CreateCharge(job, CCODE1, "Revenue Transaction Test", null, 0M, null, USD, 350M, AALSHI);
			charge.JR_GB = ZGuid.NewZGuid();
			charge.JR_GE = ZGuid.NewZGuid();
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FreightInvoice;

			InvoicingBase invoice = GetChargePoster().Post(charge);
			InvoicingLineBase invoiceLine = invoice.Lines[0];

			AssertEquals("Line Type", TransactionLineTypes.Revenue, invoiceLine.AL_LineType);
			AssertEquals("Sequence", (short)1, invoiceLine.AL_Sequence);
			AssertEquals("Description", "Revenue Transaction Test", invoiceLine.AL_Desc);
			AssertEquals("Unit Quantity", 0, invoiceLine.AL_UnitQty);
			AssertEquals("Unit Price", 0M, invoiceLine.AL_UnitPrice);
			AssertEquals("OS Unit Price", 0M, invoiceLine.AL_OSUnitPrice);
			AssertEquals("Currency", USD.RX_Code, invoiceLine.AL_RX_NKTransactionCurrency);
			AssertEquals("Exchange Rate", 0.7M, invoiceLine.AL_ExchangeRate);
			AssertEquals("Post Period", 0, invoiceLine.AL_PostPeriod);
			AssertPostDateNow(invoiceLine);
			AssertEquals("Post To GL", "N", invoiceLine.AL_PostToGL);
			AssertEquals("Reverse Period", 0, invoiceLine.AL_ReversePeriod);
			AssertEquals("Reverse Date", ZDateTime.Empty, invoiceLine.AL_ReverseDate);
			AssertEquals("Reverse to GL", "N", invoiceLine.AL_ReverseToGL);
			AssertEquals("Prevent Invoice Print Grouping", ZBool.False, invoiceLine.AL_PreventInvoicePrintGrouping);
			AssertEquals("Transaction Header PK", invoice.PK, invoiceLine.AL_AH);
			AssertEquals("Job Header", job.PK, invoiceLine.AL_JH);
			AssertEquals("Charge Code", CCODE1.PK, invoiceLine.AL_AC);
			AssertEquals("Department", charge.JR_GE, invoiceLine.AL_GE);
			AssertEquals("Branch", charge.JR_GB, invoiceLine.AL_GB);
			AssertEquals("General Ledger", Creator.GLHeader1.PK, invoiceLine.AL_AG);
			AssertEquals("OrgHeader", AALSHI.PK, invoiceLine.AL_OH);
			AssertEquals("GL Percent Of", ZGuid.Empty, invoiceLine.AL_AG_PercentOf);
			AssertEquals("Percentage of Period", 0, invoiceLine.AL_PercentageOfPeriod);

			AssertEquals("OS Ex Tax Amount", 350.00M, invoiceLine.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 0M, invoiceLine.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 17.50M, invoiceLine.AL_OSWHTAmount);
			AssertEquals("OS Total Amount", 350.00M, invoiceLine.AL_OverseasTotal);

			AssertEquals("Local Ex Amount", 500.00M, invoiceLine.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount", 0M, invoiceLine.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount", 25.00M, invoiceLine.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount", 500.00M, invoiceLine.AL_LocalTotalAmount);

			AssertEquals("Line Amount", 500.00M, invoiceLine.AL_LineAmount);
			AssertEquals("GST Tax ID", ZGuid.Empty, invoiceLine.AL_AT);
			AssertEquals("Tax Msg ID", ZGuid.Empty, invoiceLine.AL_A9_VATClass);
			AssertEquals("GST Amount", 0M, invoiceLine.AL_GSTVAT);
			AssertEquals("WHT Tax ID", WHT1.PK, invoiceLine.AL_AW);
			AssertEquals("Withholding Tax", 25.00M, invoiceLine.AL_WithholdingTax);
			AssertEquals("OS Amount", 350.00M, invoiceLine.AL_OSAmount);
		}

		public void TestPostBillInForeignGSTApplicableWHTNotApplicable()
		{
			Job job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			CreateExchangeRate(job, USD, .7M);

			ABIGAS.CompanyData.SetARTaxApplicable(true);
			ABIGAS.MiscServ.OM_ARWHTApplicable = false;
			Charge charge = CreateCharge(job, MRG100, "Revenue Transaction Test", USD, 250M, ZECTRA, USD, 350M, ABIGAS);
			charge.JR_GB = ZGuid.NewZGuid();
			charge.JR_GE = ZGuid.NewZGuid();
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FreightInvoice;

			InvoicingBase invoice = GetChargePoster().Post(charge);
			InvoicingLineBase invoiceLine = invoice.Lines[0];

			AssertEquals("Line Type", TransactionLineTypes.Revenue, invoiceLine.AL_LineType);
			AssertEquals("Sequence", (short)1, invoiceLine.AL_Sequence);
			AssertEquals("Description", "Revenue Transaction Test", invoiceLine.AL_Desc);
			AssertEquals("Unit Quantity", 0, invoiceLine.AL_UnitQty);
			AssertEquals("Unit Price", 0M, invoiceLine.AL_UnitPrice);
			AssertEquals("OS Unit Price", 0M, invoiceLine.AL_OSUnitPrice);
			AssertEquals("Currency", USD.RX_Code, invoiceLine.AL_RX_NKTransactionCurrency);
			AssertEquals("Exchange Rate", 0.7M, invoiceLine.AL_ExchangeRate);
			AssertEquals("Post Period", 0, invoiceLine.AL_PostPeriod);
			AssertPostDateNow(invoiceLine);
			AssertEquals("Post To GL", "N", invoiceLine.AL_PostToGL);
			AssertEquals("Reverse Period", 0, invoiceLine.AL_ReversePeriod);
			AssertEquals("Reverse Date", ZDateTime.Empty, invoiceLine.AL_ReverseDate);
			AssertEquals("Reverse to GL", "N", invoiceLine.AL_ReverseToGL);
			AssertEquals("Prevent Invoice Print Grouping", ZBool.False, invoiceLine.AL_PreventInvoicePrintGrouping);
			AssertEquals("Transaction Header PK", invoice.PK, invoiceLine.AL_AH);
			AssertEquals("Job Header", job.PK, invoiceLine.AL_JH);
			AssertEquals("Charge Code", MRG100.PK, invoiceLine.AL_AC);
			AssertEquals("Department", charge.JR_GE, invoiceLine.AL_GE);
			AssertEquals("Branch", charge.JR_GB, invoiceLine.AL_GB);
			AssertEquals("General Ledger", Creator.GLHeader1.PK, invoiceLine.AL_AG);
			AssertEquals("OrgHeader", ABIGAS.PK, invoiceLine.AL_OH);
			AssertEquals("GL Percent Of", ZGuid.Empty, invoiceLine.AL_AG_PercentOf);
			AssertEquals("Percentage of Period", 0, invoiceLine.AL_PercentageOfPeriod);

			AssertEquals("OS Ex Tax Amount", 350.00M, invoiceLine.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 35.00M, invoiceLine.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 0M, invoiceLine.AL_OSWHTAmount);
			AssertEquals("OS Total Amount", 385.00M, invoiceLine.AL_OverseasTotal);

			AssertEquals("Local Ex Amount", 500.00M, invoiceLine.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount", 50.00M, invoiceLine.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount", 0M, invoiceLine.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount", 550.00M, invoiceLine.AL_LocalTotalAmount);

			AssertEquals("Line Amount", 500.00M, invoiceLine.AL_LineAmount);
			AssertEquals("GST Tax ID", GST1.PK, invoiceLine.AL_AT);
			AssertEquals("Tax Msg ID", InvMsg1.PK, invoiceLine.AL_A9_VATClass);
			AssertEquals("GST Amount", 50.00M, invoiceLine.AL_GSTVAT);
			AssertEquals("WHT Tax ID", ZGuid.Empty, invoiceLine.AL_AW);
			AssertEquals("Withholding Tax", 0M, invoiceLine.AL_WithholdingTax);
			AssertEquals("OS Amount", 385.00M, invoiceLine.AL_OSAmount);
		}

		public void TestPostBillInForeignGSTNotApplicableWHTNotApplicable()
		{
			Job job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			CreateExchangeRate(job, USD, .7M);

			AALSHI.CompanyData.SetARTaxApplicable(false);
			AALSHI.MiscServ.OM_ARWHTApplicable = false;
			Charge charge = CreateCharge(job, CCODE1, "Revenue Transaction Test", USD, 350M, null, USD, 350M, AALSHI);
			charge.JR_GB = ZGuid.NewZGuid();
			charge.JR_GE = ZGuid.NewZGuid();
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FreightInvoice;

			InvoicingBase invoice = GetChargePoster().Post(charge);
			InvoicingLineBase invoiceLine = invoice.Lines[0];

			AssertEquals("Line Type", TransactionLineTypes.Revenue, invoiceLine.AL_LineType);
			AssertEquals("Sequence", (short)1, invoiceLine.AL_Sequence);
			AssertEquals("Description", "Revenue Transaction Test", invoiceLine.AL_Desc);
			AssertEquals("Unit Quantity", 0, invoiceLine.AL_UnitQty);
			AssertEquals("Unit Price", 0M, invoiceLine.AL_UnitPrice);
			AssertEquals("OS Unit Price", 0M, invoiceLine.AL_OSUnitPrice);
			AssertEquals("Currency", USD.RX_Code, invoiceLine.AL_RX_NKTransactionCurrency);
			AssertEquals("Exchange Rate", 0.7M, invoiceLine.AL_ExchangeRate);
			AssertEquals("Post Period", 0, invoiceLine.AL_PostPeriod);
			AssertPostDateNow(invoiceLine);
			AssertEquals("Post To GL", "N", invoiceLine.AL_PostToGL);
			AssertEquals("Reverse Period", 0, invoiceLine.AL_ReversePeriod);
			AssertEquals("Reverse Date", ZDateTime.Empty, invoiceLine.AL_ReverseDate);
			AssertEquals("Reverse to GL", "N", invoiceLine.AL_ReverseToGL);
			AssertEquals("Prevent Invoice Print Grouping", ZBool.False, invoiceLine.AL_PreventInvoicePrintGrouping);
			AssertEquals("Transaction Header PK", invoice.PK, invoiceLine.AL_AH);
			AssertEquals("Job Header", job.PK, invoiceLine.AL_JH);
			AssertEquals("Charge Code", CCODE1.PK, invoiceLine.AL_AC);
			AssertEquals("Department", charge.JR_GE, invoiceLine.AL_GE);
			AssertEquals("Branch", charge.JR_GB, invoiceLine.AL_GB);
			AssertEquals("General Ledger", Creator.GLHeader1.PK, invoiceLine.AL_AG);
			AssertEquals("OrgHeader", AALSHI.PK, invoiceLine.AL_OH);
			AssertEquals("GL Percent Of", ZGuid.Empty, invoiceLine.AL_AG_PercentOf);
			AssertEquals("Percentage of Period", 0, invoiceLine.AL_PercentageOfPeriod);

			AssertEquals("OS Ex Tax Amount", 350.00M, invoiceLine.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 0M, invoiceLine.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 0M, invoiceLine.AL_OSWHTAmount);
			AssertEquals("OS Total Amount", 350.00M, invoiceLine.AL_OverseasTotal);

			AssertEquals("Local Ex Amount", 500.00M, invoiceLine.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount", 0M, invoiceLine.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount", 0M, invoiceLine.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount", 500.00M, invoiceLine.AL_LocalTotalAmount);

			AssertEquals("Line Amount", 500.00M, invoiceLine.AL_LineAmount);
			AssertEquals("GST Tax ID", ZGuid.Empty, invoiceLine.AL_AT);
			AssertEquals("Tax Msg ID", ZGuid.Empty, invoiceLine.AL_A9_VATClass);
			AssertEquals("GST Amount", 0M, invoiceLine.AL_GSTVAT);
			AssertEquals("WHT Tax ID", ZGuid.Empty, invoiceLine.AL_AW);
			AssertEquals("Withholding Tax", 0M, invoiceLine.AL_WithholdingTax);
			AssertEquals("OS Amount", 350.00M, invoiceLine.AL_OSAmount);
		}

		public void TestPostBillInForeignGSTApplicable_UpliftApplicable()
		{
			ABIGAS.CompanyData.SetARTaxApplicable(true);
			ABIGAS.MiscServ.OM_ARWHTApplicable = false;
			ABIGAS.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, 5d, 10d);
			ABIGAS.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea, 5d, 10d);
			ABIGAS.CompanyData.Factory.Save();

			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "INBOM";
			shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			using (Job job = Job.CreateWithMutex(Factory, shipment))
			{
				job.PlugInData = shipment;
				job.LocalChargesPK = ABIGAS.PK;
				job.JH_JobNum = "S00001";
				CreateExchangeRate(job, USD, .85M);

				AccChargeCode fRT = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);
				fRT.AC_AT_GSTRate = GST1.PK;
				Charge charge = CreateCharge(job, fRT, "Revenue Transaction Test", USD, 100M, ZECTRA, USD, 100M, ABIGAS);
				charge.JR_GB = ZGuid.NewZGuid();
				charge.JR_GE = ZGuid.NewZGuid();

				AssertEquals("Charge.OSCostAmount", 100.00M, charge.JR_OSCostAmt);
				AssertEquals("Charge.LocalCostAmount", 117.65M, charge.JR_LocalCostAmt);
				AssertEquals("Charge.OSSellAmount", 100.00M, charge.JR_OSSellAmt);
				AssertEquals("Charge.LocalSellAmount + Uplift 10.00", 127.65M, charge.JR_LocalSellAmt);

				ChargePoster poster = GetChargePoster();
				InvoicingBase invoice = poster.Post(charge);
				InvoicingLineBase invoiceLine = invoice.Lines[0];

				AssertEquals("Line Type", TransactionLineTypes.Revenue, invoiceLine.AL_LineType);
				AssertEquals("Sequence", (short)1, invoiceLine.AL_Sequence);
				AssertEquals("Description", "Revenue Transaction Test", invoiceLine.AL_Desc);
				AssertEquals("Unit Quantity", 0, invoiceLine.AL_UnitQty);
				AssertEquals("Unit Price", 0M, invoiceLine.AL_UnitPrice);
				AssertEquals("OS Unit Price", 0M, invoiceLine.AL_OSUnitPrice);
				AssertEquals("Currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, invoiceLine.AL_RX_NKTransactionCurrency);
				if (poster.OverrideChargeExchangeRatesOnPosting)
				{
					AssertEquals("Exchange Rate", 1M, invoiceLine.AL_ExchangeRate);
				}
				AssertEquals("Post Period", 0, invoiceLine.AL_PostPeriod);
				AssertPostDateNow(invoiceLine);
				AssertEquals("Post To GL", "N", invoiceLine.AL_PostToGL);
				AssertEquals("Reverse Period", 0, invoiceLine.AL_ReversePeriod);
				AssertEquals("Reverse Date", ZDateTime.Empty, invoiceLine.AL_ReverseDate);
				AssertEquals("Reverse to GL", "N", invoiceLine.AL_ReverseToGL);
				AssertEquals("Prevent Invoice Print Grouping", ZBool.False, invoiceLine.AL_PreventInvoicePrintGrouping);
				AssertEquals("Transaction Header PK", invoice.PK, invoiceLine.AL_AH);
				AssertEquals("Job Header", job.PK, invoiceLine.AL_JH);
				AssertEquals("Charge Code", fRT.PK, invoiceLine.AL_AC);
				AssertEquals("Department", charge.JR_GE, invoiceLine.AL_GE);
				AssertEquals("Branch", charge.JR_GB, invoiceLine.AL_GB);
				AssertEquals("General Ledger", fRT.AC_AG_RevenueAccount, invoiceLine.AL_AG);
				AssertEquals("OrgHeader", ABIGAS.PK, invoiceLine.AL_OH);
				AssertEquals("GL Percent Of", ZGuid.Empty, invoiceLine.AL_AG_PercentOf);
				AssertEquals("Percentage of Period", 0, invoiceLine.AL_PercentageOfPeriod);

				AssertEquals("OS Ex Tax Amount", 127.65M, invoiceLine.AL_OSExTaxAmount);
				AssertEquals("OS Tax Amount is calculated from LocalAmount with uplift", 12.77M, invoiceLine.AL_OSTaxAmount);
				AssertEquals("OS WHT Amount", 0M, invoiceLine.AL_OSWHTAmount);
				AssertEquals("OS Total Amount", 140.42M, invoiceLine.AL_OverseasTotal);

				AssertEquals("Local Ex Amount", 127.65M, invoiceLine.AL_LocalExTaxAmount);
				AssertEquals("Local Tax Amount", 12.77M, invoiceLine.AL_LocalTaxAmount);
				AssertEquals("Local WHT Amount", 0M, invoiceLine.AL_LocalWHTAmount);
				AssertEquals("Local Total Amount", 140.42M, invoiceLine.AL_LocalTotalAmount);

				AssertEquals("Line Amount", 127.65M, invoiceLine.AL_LineAmount);
				AssertEquals("GST Tax ID", GST1.PK, invoiceLine.AL_AT);
				AssertEquals("Tax Msg ID", InvMsg1.PK, invoiceLine.AL_A9_VATClass);
				AssertEquals("GST Amount", 12.77M, invoiceLine.AL_GSTVAT);
				AssertEquals("WHT Tax ID", ZGuid.Empty, invoiceLine.AL_AW);
				AssertEquals("Withholding Tax", 0M, invoiceLine.AL_WithholdingTax);
				AssertEquals("OS Amount", 140.42M, invoiceLine.AL_OSAmount);
			}
		}

		public void TestPostCharges_DbHits()
		{
			var job = CreateJob("Z00001011", AALSHI, true, 0M, ABIGAS, true, 0m);
			CreateExchangeRate(job, USD, 0.5M);

			for (int index = 0; index < 10; index++)
			{
				var childJob = CreateJob("WHATEVER" + (index + 1), AALSHI, true, 0m, ABIGAS, true, 0m);
				childJob.JH_JH_ParentJob = job.PK;

				var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
				childJob.JH_ParentID = shipment.PK;

				var charge = CreateCharge(childJob, CCODE1, "Revenue Transaction Test", AUD, 0M, null, AUD, 100M * (index + 1), AALSHI);
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				charge.JR_RX_NKSellInvoiceCurrency = "USD";
			}

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var expectedHits = new Dictionary<string, int>
			{
				{ AccAllowedBranchDepartmentComboSchema.Constants.TableName, 1 },
				{ AccChargeCodeSchema.Constants.TableName, 1 },
				{ AccChargeGLPostingOverrideSchema.Constants.TableName, 3 },
				{ AccChargeRevRecOverrideSchema.Constants.TableName, 1 },
				{ AccExchangeRateConfigurationViewSchema.Constants.TableName, 1 },
				{ ExchangeRateCurrencyConfiguration.Schema.TableName, 1 },
				{ AccGLHeaderSchema.Constants.TableName, 1 },
				{ AccPeriodManagementSchema.Constants.TableName, 1 },
				{ AccTransactionHeaderSchema.Constants.TableName, 1 },
				{ AccTransactionLinesSchema.Constants.TableName, 1 },
				{ AccGLHeaderSubAccountSchema.Constants.TableName, 1 },
				{ JobChargeSchema.Constants.TableName, 2 },
				{ JobChargeRevRecognitionSchema.Constants.TableName, 1 },
				{ JobExRateSchema.Constants.TableName, 1 },
				{ JobHeaderSchema.Constants.TableName, 2 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgARTermsSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 2 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ AccChargeTypeOverrideSchema.Constants.TableName, 1 },
				// Below here are expected to be cached in UberFactory
				{ RefCountrySchema.Constants.TableName, 1 },
				// GlbBranch table db hit expected to be 2. The below two places db hits happended when posting an invoice.
				// 1. AutoAccTransactionHeader --> GlbBranch Branch => (GlbBranch)base.Factory.Load(typeof(GlbBranch), AH_GB)
				// 2. AccTransactionLinesLookups --> AccountingMasterFilesUtils.GetBranchesOfCurrentCompany(Factory) 
				// In this method, GlbBranch table db hit is happening
				{ GlbBranchSchema.Constants.TableName, 2 },
			};
			RowFactory.ClearSpecificTableFromUberFactory(RefCountrySchema.Constants.TableName);
			// GlbBranch table is in the Registry Cache table similar to RefCountry which is also in the registry.
			// In order to get consistent db hit counter, we need to clear the cache for these tables.
			RowFactory.RemoveCachedTablesTemporarily(GlbBranchSchema.Constants.TableName);
			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			using (AssertDbHitsWithUsefulQueryInformation(expectedHits, otherFactory))
			{
				var jobInOtherFactory = otherFactory.Load<Job>(job.PK);

				var charges = new IReceivablesPostingChargeCollection();
				charges.Key = new PostingChargeKey(ABIGAS.PK, InvoiceTypesList.Codes.FinalInvoice, "Z00001011", ZGuid.Empty, ZGuid.Empty, 0);
				charges.AddRange((IReceivablesPostingCharge[])jobInOtherFactory.Charges.ToArray(typeof(IReceivablesPostingCharge)));
				var chargePoster = new ChargePoster(otherFactory);
				chargePoster.Post(charges);
			}
		}

		public void TestPostChargesWithUnsortedDisplayOrder()
		{
			Job job = CreateJob("Z00001001", ABIGAS, true, 10, ZECTRA, true, 10);
			job.PlugInData = Factory.New<ForwardingShipment>();
			Charge charge1 = CreateCharge(job, MRG100, "Test Charge 1", null, 0, null, AUD, 300, ABIGAS);
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			Charge charge2 = CreateCharge(job, CCODE1, "Test Charge 2", null, 0, null, AUD, 500, ABIGAS);
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			charge1.JR_DisplaySequence = 2;
			charge2.JR_DisplaySequence = 1;

			PostingChargeCollection distributedCharges = new PostingChargeDistributor().DistributeCharges(job.ReceivableCharges);
			AssertEquals("1 collection of charges", 1, distributedCharges.Count);

			ARInvoice invoice = null;
			ChargePoster poster = GetChargePoster();

			ZDateTime now = ZDateTime.Now;
			foreach (IReceivablesPostingChargeCollection charges in distributedCharges)
			{
				invoice = (ARInvoice)poster.Post(charges);
			}

			AssertEquals("Should be two invoice lines", 2, invoice.Lines.Count);
			foreach (InvoicingLineBase line in invoice.Lines)
			{
				AssertNoErrors("AL_Sequence should not have any errors before validation", line.AL_SequenceInfo);
				line.RunPreSaveValidation();
				AssertNoErrors("AL_Sequence should not have any errors after validation", line.AL_SequenceInfo);
			}
		}

		#endregion

		#region Invoice Header

		public void TestInvoiceIsDisbursementSetForDisbursementType()
		{
			Job job = CreateJob("Z00001001", ABIGAS, true, 10, ZECTRA, true, 10);
			Charge charge = CreateCharge(job, MRG100, "Test Charge", null, 0, null, AUD, 300, ABIGAS);
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			Job job2 = CreateJob("Z00001002", ABIGAS, true, 10, ZECTRA, true, 10);
			Charge charge2 = CreateCharge(job2, MRG100, "Test Charge2", null, 0, null, AUD, 300, ABIGAS);
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice;

			PostingChargeCollection distributedCharges = new PostingChargeDistributor().DistributeCharges(job.ReceivableCharges);
			AssertEquals("1 collection of charges", 1, distributedCharges.Count);

			ARInvoice invoice = null;

			foreach (IReceivablesPostingChargeCollection charges in distributedCharges)
			{
				invoice = (ARInvoice)GetChargePoster().Post(charges);
				AssertEquals("Charge is not disbursement style, so invoice is not disbursement", false, invoice.AH_IsDisbursementCalc);
			}

			distributedCharges = new PostingChargeDistributor().DistributeCharges(job2.ReceivableCharges);
			AssertEquals("1 collection of charges", 1, distributedCharges.Count);

			foreach (IReceivablesPostingChargeCollection charges in distributedCharges)
			{
				invoice = (ARInvoice)GetChargePoster().Post(charges);
				AssertEquals("Charge IS disbursement type, so invoice is disbursement", true, invoice.AH_IsDisbursementCalc);
			}
		}

		public void TestInvoiceTransactionCategorySetForARInvoicesAndCreditNotes()
		{
			Job job = CreateJob("Z00001001", ABIGAS, true, 10, ZECTRA, true, 10);
			Charge charge = CreateCharge(job, MRG100, "Test Charge", null, 0, null, AUD, 300, ABIGAS);
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			PostingChargeCollection distributedCharges = new PostingChargeDistributor().DistributeCharges(job.ReceivableCharges);
			AssertEquals("1 collection of charges", 1, distributedCharges.Count);

			foreach (IReceivablesPostingChargeCollection charges in distributedCharges)
			{
				ARInvoice invoice = (ARInvoice)GetChargePoster().Post(charges);
				AssertEquals("ARInvoice should has TransactionCatrgory equal to the Charge InvoiceType",
						charge.JR_InvoiceType, invoice.AH_TransactionCategory);
			}

			Job job2 = CreateJob("Z00001001", ABIGAS, true, 10, ZECTRA, true, 10);
			Charge charge2 = CreateCharge(job2, MRG100, "Test Charge", null, 0, null, AUD, -300, ABIGAS);
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice;

			distributedCharges = new PostingChargeDistributor().DistributeCharges(job2.ReceivableCharges);
			AssertEquals("1 collection of charges", 1, distributedCharges.Count);

			foreach (IReceivablesPostingChargeCollection charges in distributedCharges)
			{
				ARCreditNote creditNote = (ARCreditNote)GetChargePoster().Post(charges);
				AssertEquals("ARCreditNote should has empty TransactionCatrgory", charge2.JR_InvoiceType, creditNote.AH_TransactionCategory);
			}
		}

		public void TestSetARInvoiceValues()
		{
			ABIGAS.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			ABIGAS.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = 2;
			ABIGAS.CompanyData.CreateOrLoadARTerm(InvoiceTypesList.Codes.FreightInvoice).PY_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			ABIGAS.CompanyData.CreateOrLoadARTerm(InvoiceTypesList.Codes.FreightInvoice).PY_InvoiceDays = 10;
			Factory.Save();

			Job job = CreateJob("Z00001001", ABIGAS, true, 10, ZECTRA, true, 10);

			CreateExchangeRate(job, USD, .7M);
			CreateExchangeRate(job, GBP, .4M);

			Charge charge = CreateCharge(job, MRG100, "Test Charge", null, 0, null, AUD, 300, ABIGAS);
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FreightInvoice;

			ChargePoster poster = GetChargePoster();
			ARInvoice invoice = (ARInvoice)poster.Post(charge);
			CreateInvoiceLine(invoice, USD, .5M, 10M);

			AssertEquals("Ledger Type", LedgerTypes.AccountsReceivable, invoice.AH_Ledger);
			AssertEquals("Transaction Count", 1, (int)invoice.AH_TransactionCount);
			AssertEquals("Transaction Type", TransactionTypes.Invoice, invoice.AH_TransactionType);
			AssertEquals("Job Header", job.PK, invoice.AH_JH);
			AssertEquals("Branch", job.JH_GB, invoice.AH_GB);
			AssertEquals("Department", job.JH_GE, invoice.AH_GE);
			AssertEquals("Description", ExpectedARInvoiceDescription, invoice.AH_Desc);
			AssertEquals("Client", charge.JR_OH_SellAccount, invoice.AH_OH);
			AssertEquals("DebtorAddress", ZGuid.Empty, invoice.AH_OA_InvoiceAddressOverride);
			AssertEquals("DebtorContact", ZGuid.Empty, invoice.AH_OC_InvoiceContactOverride);
			AssertEquals("Currency", charge.JR_RX_NKSellCurrency, invoice.AH_RX_NKTransactionCurrency);
			AssertEquals("Exchange Rate", charge.JR_OSSellExRate, invoice.AH_ExchangeRate);
			if (poster.ShouldSetConsolidatedInvoiceRef)
			{
				AssertEquals("Consolidate Invoice Ref", "Z00001001", invoice.AH_ConsolidatedInvoiceRef);
			}
			AssertEquals("Invoice Term", Constants.InvoiceTerms.CashOnDelivery, invoice.AH_InvoiceTerm);
			AssertEquals("Invoice Term Days", (byte)10, invoice.AH_InvoiceTermDays);
		}

		public void TestSetARInvoiceAddressContact()
		{
			var job = CreateJob("Z00001001", ABIGAS, true, 10, ZECTRA, true, 10);
			var expectedDebtor = ABIGAS;
			var expectedAddress = Creator.CreateAddress(expectedDebtor);
			var expectedContact = Creator.CreateContact(expectedDebtor);
			var charge = CreateCharge(job, MRG100, "Test Charge", null, 0, null, AUD, 300, expectedDebtor);
			charge.JR_OA_SellInvoiceAddress = expectedAddress.PK;
			charge.JR_OC_SellInvoiceContact = expectedContact.PK;

			ChargePoster poster = GetChargePoster();
			var invoice = (ARInvoice)poster.Post(charge);

			AssertEquals("Debtor", expectedDebtor.PK, invoice.AH_OH);
			AssertEquals("DebtorAddress", expectedAddress.PK, invoice.AH_OA_InvoiceAddressOverride);
			AssertEquals("DebtorContact", expectedContact.PK, invoice.AH_OC_InvoiceContactOverride);
		}

		protected virtual string ExpectedARInvoiceDescription
		{
			get { return "Z00001001"; }
		}

		public void TestSetARCreditNoteValues()
		{
			var arTerm = ABIGAS.CompanyData.LoadARTermForAllInvoiceTypes();
			arTerm.PY_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			arTerm.PY_InvoiceDays = 20;
			Factory.Save();

			var job = CreateJob("Z00001003", ABIGAS, true, 10, ZECTRA, true, 10);
			CreateExchangeRate(job, USD, .7M);
			CreateExchangeRate(job, GBP, .4M);

			var charge = CreateCharge(job, MRG100, "Test Charge", null, 0, null, AUD, -300, ABIGAS);
			Factory.Save();

			var poster = GetChargePoster();
			var creditNote = (ARCreditNote)poster.Post(charge);
			CreateInvoiceLine(creditNote, USD, .5M, 10M);

			AssertEquals("Ledger Type", LedgerTypes.AccountsReceivable, creditNote.AH_Ledger);
			AssertEquals("Transaction Count", 1, (int)creditNote.AH_TransactionCount);
			AssertEquals("Transaction Type", TransactionTypes.CreditNote, creditNote.AH_TransactionType);
			AssertEquals("Job Header", job.PK, creditNote.AH_JH);
			AssertEquals("Branch", job.JH_GB, creditNote.AH_GB);
			AssertEquals("Department", job.JH_GE, creditNote.AH_GE);
			AssertEquals("Description", ExpectedARCreditNoteDescription, creditNote.AH_Desc);
			AssertEquals("Client", charge.JR_OH_SellAccount, creditNote.AH_OH);
			AssertEquals("DebtorAddress", ZGuid.Empty, creditNote.AH_OA_InvoiceAddressOverride);
			AssertEquals("DebtorContact", ZGuid.Empty, creditNote.AH_OC_InvoiceContactOverride);
			AssertEquals("Currency", charge.JR_RX_NKSellCurrency, creditNote.AH_RX_NKTransactionCurrency);
			AssertEquals("Exchange Rate", charge.JR_OSSellExRate, creditNote.AH_ExchangeRate);
			if (poster.ShouldSetConsolidatedInvoiceRef)
			{
				AssertEquals("Consolidate Invoice Ref", "Z00001003", creditNote.AH_ConsolidatedInvoiceRef);
			}
			AssertEquals("Invoice Term", Constants.InvoiceTerms.CashOnDelivery, creditNote.AH_InvoiceTerm);
			AssertEquals("Invoice Term Days", (byte)20, creditNote.AH_InvoiceTermDays);
			Assert("Original Reference Start Date empty", creditNote.AH_OriginalReferenceStartDate.IsEmpty);
			Assert("Original Reference End Date empty", creditNote.AH_OriginalReferenceEndDate.IsEmpty);
		}

		public void TestSetARCreditNoteValues_ReferenceDates()
		{
			using (InvoiceBaseValidationTest.InitaliseComplianceFactory(areOriginalTransactionReferenceFieldsMandatory: true))
			{
				var job = CreateJob("Z00001003", ABIGAS, true, 10, ZECTRA, true, 10);
				CreateExchangeRate(job, EUR, .6M);
				var charge = CreateCharge(job, MRG100, "Test Charge PT", null, 0, null, AUD, -300, ABIGAS);
				Factory.Save();

				var poster = GetChargePoster();
				var creditNote = (ARCreditNote)poster.Post(charge);
				CreateInvoiceLine(creditNote, EUR, .7M, 10M);

				AssertEquals("Ledger Type", LedgerTypes.AccountsReceivable, creditNote.AH_Ledger);
				AssertEquals("Transaction Count", 1, (int)creditNote.AH_TransactionCount);
				AssertEquals("Transaction Type", TransactionTypes.CreditNote, creditNote.AH_TransactionType);
				AssertEquals("Job Header", job.PK, creditNote.AH_JH);
				AssertEquals("Branch", job.JH_GB, creditNote.AH_GB);
				AssertEquals("Department", job.JH_GE, creditNote.AH_GE);
				AssertEquals("Currency", charge.JR_RX_NKSellCurrency, creditNote.AH_RX_NKTransactionCurrency);
				AssertEquals("Exchange Rate", charge.JR_OSSellExRate, creditNote.AH_ExchangeRate);
				Assert("Invoice Date not empty", !creditNote.AH_InvoiceDate.IsEmpty);
				var expectedDate = creditNote.AH_InvoiceDate.Date;
				AssertEquals("Original Reference Start Date", expectedDate, creditNote.AH_OriginalReferenceStartDate);
				AssertEquals("Original Reference End Date", expectedDate, creditNote.AH_OriginalReferenceEndDate);
			}
		}

		public void TestSetARCreditNoteAddressContact()
		{
			var job = CreateJob("Z00001001", ABIGAS, true, 10, ZECTRA, true, 10);
			var expectedDebtor = ABIGAS;
			var expectedAddress = Creator.CreateAddress(expectedDebtor);
			var expectedContact = Creator.CreateContact(expectedDebtor);
			var charge = CreateCharge(job, MRG100, "Test Charge", null, 0, null, AUD, -300, expectedDebtor);
			charge.JR_OA_SellInvoiceAddress = expectedAddress.PK;
			charge.JR_OC_SellInvoiceContact = expectedContact.PK;

			ChargePoster poster = GetChargePoster();
			var invoice = (ARCreditNote)poster.Post(charge);
			AssertEquals("Debtor", expectedDebtor.PK, invoice.AH_OH);
			AssertEquals("DebtorAddress", expectedAddress.PK, invoice.AH_OA_InvoiceAddressOverride);
			AssertEquals("DebtorContact", expectedContact.PK, invoice.AH_OC_InvoiceContactOverride);
		}

		protected virtual string ExpectedARCreditNoteDescription
		{
			get { return "Z00001003"; }
		}

		protected virtual bool ShouldCheckConsolidatedInvoiceRef
		{
			get
			{
				return true;
			}
		}

		public void TestConsolInvoiceJobReference()
		{
			ZECTRA.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			ZECTRA.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = 10;
			Factory.Save();

			TestBranch.GB_OH_OrgProxy = Creator.ABIGAS.PK;

			ForwardingConsol consol = Creator.CreateConsol("AUSYD", "KRSEL", "C00001234");
			ForwardingShipment shipment = Creator.CreateShipment("S00000001");
			consol.Shipments.Add(shipment);

			Job job = Creator.CreateJob(shipment);
			job.JH_UniqueJobInvoiceNumber = (ZShort)28;

			Charge charge = CreateCharge(job, MRG100, "Test Charge", null, 0, null, AUD, 300, ZECTRA);

			Factory.Save();

			IReceivablesPostingChargeCollection charges = new IReceivablesPostingChargeCollection();
			charges.Key = new PostingChargeKey(Creator.Agent.PK, InvoiceTypesList.Codes.FinalInvoice, "C00001234", ZGuid.Empty, ZGuid.Empty, 0);
			charges.Add(charge);

			ARInvoice invoice = (ARInvoice)GetChargePoster().Post(charges);
			CreateInvoiceLine(invoice, USD, .5M, 10M);

			AssertEquals("Ledger Type", LedgerTypes.AccountsReceivable, invoice.AH_Ledger);
			AssertEquals("Transaction Count", 1, (int)invoice.AH_TransactionCount);
			AssertEquals("Transaction Type", TransactionTypes.Invoice, invoice.AH_TransactionType);
			Assert("Job Header", invoice.AH_JH.IsEmpty);
			AssertEquals("Branch", job.JH_GB, invoice.AH_GB);
			AssertEquals("Description", "FREIGHT CONSOL INVOICE", invoice.AH_Desc);
			AssertEquals("Client", charge.JR_OH_SellAccount, invoice.AH_OH);
			AssertEquals("Currency", charge.JR_RX_NKSellCurrency, invoice.AH_RX_NKTransactionCurrency);
			AssertEquals("Exchange Rate", charge.JR_OSSellExRate, invoice.AH_ExchangeRate);
			AssertEquals("Consolidate Invoice Ref", "C00001234", invoice.AH_ConsolidatedInvoiceRef);
			AssertEquals("Invoice Term", Constants.InvoiceTerms.CashOnDelivery, invoice.AH_InvoiceTerm);
			AssertEquals("Invoice Term Days", (byte)10, invoice.AH_InvoiceTermDays);
		}

		#endregion

		public void TestPostingCharge_UpdateInvoiceLineAndChargeDescriptions()
		{
			var shipment = Creator.CreateShipment("S00001");
			var job = Creator.CreateJob(shipment, false);
			var charge1 = Creator.CreateCharge(job, Creator.CC1, "ChargeDesc1", null, 0, null, AUD, 100, ABIGAS);
			var charge2 = Creator.CreateCharge(job, Creator.CC2, "ChargeDesc2", null, 0, null, AUD, 200, ABIGAS);
			charge1.JR_InvoiceType = charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			Factory.Save();

			var accountingMasterFilesDependencyFactoryMock = new Mock<IAccountingMasterFilesDependencyFactory>();
			var chargeCodeOverrideRulesRankerMock = new Mock<IChargeCodeOverrideRulesRanker>();
			var chargeComplianceDescriptionPostingHelperMock = new Mock<IChargeComplianceDescriptionPostingHelper>();
			ObjectFactory.Substitute(accountingMasterFilesDependencyFactoryMock.Object);
			accountingMasterFilesDependencyFactoryMock.Setup(x => x.GetChargeCodeOverrideRulesRanker()).Returns(chargeCodeOverrideRulesRankerMock.Object);
			accountingMasterFilesDependencyFactoryMock.Setup(x => x.GetChargeComplianceDescriptionPostingHelper()).Returns(chargeComplianceDescriptionPostingHelperMock.Object);

			chargeCodeOverrideRulesRankerMock.Setup(x => x.GetBestSellComplianceDescriptionOverride(Factory, charge1.ChargeCode.ChargeComplianceDescriptions, It.IsAny<ZString?>(), It.IsAny<ZString?>(), It.IsAny<ZString>())).Returns(ZString.Empty);
			Assert("Precondition: SellComplianceDescription", charge1.SellComplianceDescription.IsEmpty);

			chargeCodeOverrideRulesRankerMock.Setup(x => x.GetBestSellComplianceDescriptionOverride(Factory, charge2.ChargeCode.ChargeComplianceDescriptions, It.IsAny<ZString?>(), It.IsAny<ZString?>(), It.IsAny<ZString>())).Returns("CHARGE_DESC");
			Assert("Precondition: SellComplianceDescription", !charge2.SellComplianceDescription.IsEmpty);

			IReceivablesPostingChargeCollection receivablesPostingCharges = new IReceivablesPostingChargeCollection();
			receivablesPostingCharges.Key = new PostingChargeKey(ABIGAS.PK, InvoiceTypesList.Codes.FinalInvoice, "S00001", ZGuid.Empty, ZGuid.Empty, 0);
			receivablesPostingCharges.Add(charge1);
			receivablesPostingCharges.Add(charge2);
			var invoice = GetChargePoster().Post(receivablesPostingCharges);
			var invoiceLine1 = invoice.Lines[0];
			var invoiceLine2 = invoice.Lines[1];

			chargeComplianceDescriptionPostingHelperMock.Verify(x => x.UpdateInvoiceLineAndChargeDescriptions(It.IsAny<IDescriptionSetter>(), It.IsAny<ISellComplianceDescription>()), Times.Exactly(2));
			chargeComplianceDescriptionPostingHelperMock.Verify(x => x.UpdateInvoiceLineAndChargeDescriptions(invoiceLine1, charge1), Times.Once);
			chargeComplianceDescriptionPostingHelperMock.Verify(x => x.UpdateInvoiceLineAndChargeDescriptions(invoiceLine2, charge2), Times.Once);
		}

		public void TestSetTransactionHeaderBranch_Job()
		{
			if (this.GetType() == typeof(PeriodicInvoiceChargePosterTest))
			{
				Assert(true);
			}
			else
			{
				var job = CreateJob("Z00001155", ABIGAS, true, 10, ZECTRA, true, 10);
				var expectedBranchPK = Creator.CreateBranch("AA1", GlbCompany.CurrentCompany).PK;
				job.JH_GB = expectedBranchPK;

				var charge = CreateCharge(job, MRG100, "Charge", null, 0, null, AUD, 300, ABIGAS);
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				charge.JR_GB = Creator.NonCurrentBranch.PK;

				Factory.Save();

				var mockIAccountingDependencyFactory = new Mock<IAccountingDependencyFactory>();
				var jobCostingPlugInHelpersMock = new Mock<IJobCostingPlugInHelpers>();
				var branchLevelPostingHelperMock = new Mock<IBranchLevelPostingHelper>();
				ObjectFactory.Substitute(mockIAccountingDependencyFactory.Object);

				mockIAccountingDependencyFactory.Setup(x => x.GetJobCostingPlugInHelpers()).Returns(jobCostingPlugInHelpersMock.Object);
				mockIAccountingDependencyFactory.Setup(x => x.GetBranchLevelPostingHelper()).Returns(branchLevelPostingHelperMock.Object);

				var initialBranchPKValueInPassedTransaction = ZGuid.Empty;
				branchLevelPostingHelperMock.Setup(x => x.SetTransactionHeaderBranch(It.IsAny<ARInvoice>(), InvoiceProcessingLevelIsAllowingToResetBranch.Creation, It.IsAny<ITransactionBranchCalculationDataProviderFromJobCharge>()))
					.Callback<TransactionHeaderWithLines, InvoiceProcessingLevelIsAllowingToResetBranch, ITransactionBranchCalculationDataProviderFromJobCharge>(
						(transaction, invoiceLevel, charges) =>
						{
							initialBranchPKValueInPassedTransaction = transaction.AH_GB;
						}
					);

				var receivablesPostingCharges = new IReceivablesPostingChargeCollection();
				receivablesPostingCharges.Key = new PostingChargeKey(ABIGAS.PK, InvoiceTypesList.Codes.FinalInvoice, "Z00001155", ZGuid.Empty, ZGuid.Empty, 0);
				receivablesPostingCharges.Add(charge);
				var poster = GetChargePoster();
				var invoice = (ARInvoice)GetChargePoster().Post(receivablesPostingCharges);

				AssertEquals("initialBranchPKValueInPassedTransaction: ", expectedBranchPK, initialBranchPKValueInPassedTransaction);
				branchLevelPostingHelperMock.Verify(x => x.SetTransactionHeaderBranch(It.IsAny<ARInvoice>(), It.IsAny<InvoiceProcessingLevelIsAllowingToResetBranch>(), It.IsAny<ITransactionBranchCalculationDataProviderFromJobCharge>()), Times.Once);
				branchLevelPostingHelperMock.Verify(x => x.SetTransactionHeaderBranch(invoice, InvoiceProcessingLevelIsAllowingToResetBranch.Creation, receivablesPostingCharges));
			}
		}

		public void TestSetTransactionHeaderBranch_AgentInvoiceCreationPostingChargeCollection()
		{
			if (this.GetType() == typeof(PeriodicInvoiceChargePosterTest))
			{
				Assert(true);
			}
			else
			{
				var job = CreateJob("Z00001155", ABIGAS, true, 10, ZECTRA, true, 10);
				var expectedBranchPK = Creator.CreateBranch("AA1", GlbCompany.CurrentCompany).PK;

				var charge = CreateCharge(job, MRG100, "Charge", null, 0, null, AUD, 300, ABIGAS);
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				charge.JR_GB = Creator.NonCurrentBranch.PK;

				Factory.Save();

				var mockIAccountingDependencyFactory = new Mock<IAccountingDependencyFactory>();
				var jobCostingPlugInHelpersMock = new Mock<IJobCostingPlugInHelpers>();
				var branchLevelPostingHelperMock = new Mock<IBranchLevelPostingHelper>();
				ObjectFactory.Substitute(mockIAccountingDependencyFactory.Object);

				mockIAccountingDependencyFactory.Setup(x => x.GetJobCostingPlugInHelpers()).Returns(jobCostingPlugInHelpersMock.Object);
				mockIAccountingDependencyFactory.Setup(x => x.GetBranchLevelPostingHelper()).Returns(branchLevelPostingHelperMock.Object);

				var initialBranchPKValueInPassedTransaction = ZGuid.Empty;
				branchLevelPostingHelperMock.Setup(x => x.SetTransactionHeaderBranch(It.IsAny<ARInvoice>(), InvoiceProcessingLevelIsAllowingToResetBranch.Creation, It.IsAny<ITransactionBranchCalculationDataProviderFromJobCharge>()))
					.Callback<TransactionHeaderWithLines, InvoiceProcessingLevelIsAllowingToResetBranch, ITransactionBranchCalculationDataProviderFromJobCharge>(
						(transaction, invoiceLevel, charges) =>
						{
							initialBranchPKValueInPassedTransaction = transaction.AH_GB;
						}
					);

				var receivablesPostingCharges = new AgentInvoiceCreationPostingChargeCollection();
				receivablesPostingCharges.Key = new PostingChargeKey(ABIGAS.PK, InvoiceTypesList.Codes.FinalInvoice, "Z00001155", ZGuid.Empty, ZGuid.Empty, 0);
				receivablesPostingCharges.SetInvoicePostingBranch(expectedBranchPK);
				receivablesPostingCharges.Add(charge);
				receivablesPostingCharges.SetDebtor(ABIGAS.PK);
				receivablesPostingCharges.SetInvoicePostingDepartment(charge.JR_GE);
				receivablesPostingCharges.SetIsBillInLocalCurrency(false);
				receivablesPostingCharges.SetJobNumber(job.JH_JobNum);
				receivablesPostingCharges.SetJobPK(job.PK);
				receivablesPostingCharges.SetPostingJob(job);
				receivablesPostingCharges.PostingCurrency = "AUD";
				receivablesPostingCharges.PostingCurrencyExchangeRate = 1;
				var poster = GetChargePoster();
				var invoice = (ARInvoice)GetChargePoster().Post(receivablesPostingCharges);

				AssertEquals("initialBranchPKValueInPassedTransaction: ", expectedBranchPK, initialBranchPKValueInPassedTransaction);
				branchLevelPostingHelperMock.Verify(x => x.SetTransactionHeaderBranch(It.IsAny<ARInvoice>(), It.IsAny<InvoiceProcessingLevelIsAllowingToResetBranch>(), It.IsAny<ITransactionBranchCalculationDataProviderFromJobCharge>()), Times.Once);
				branchLevelPostingHelperMock.Verify(x => x.SetTransactionHeaderBranch(invoice, InvoiceProcessingLevelIsAllowingToResetBranch.Creation, receivablesPostingCharges));
			}
		}

		public void TestSetTransactionHeaderBranch_Consol_WhenGetBranchFromAgents()
		{
			if (this.GetType() == typeof(PeriodicInvoiceChargePosterTest))
			{
				Assert(true);
			}
			else
			{
				var expectedBranch = Creator.CreateBranch("AA1", GlbCompany.CurrentCompany);
				var consol = Creator.CreateConsol("AUSYD", "KRSEL", "C00001234");
				var shipment = Creator.CreateShipment("S00000001", consol);
				var job = Creator.CreateJob(shipment, false);
				var charge = CreateCharge(job, Creator.FRT, "Transaction Test", AUD, 3M, null, AUD, 4M, Creator.ABIGAS);
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

				Factory.Save();

				var mockIAccountingDependencyFactory = new Mock<IAccountingDependencyFactory>();
				var jobCostingPlugInHelpersMock = new Mock<IJobCostingPlugInHelpers>();
				var branchLevelPostingHelperMock = new Mock<IBranchLevelPostingHelper>();
				ObjectFactory.Substitute(mockIAccountingDependencyFactory.Object);

				mockIAccountingDependencyFactory.Setup(x => x.GetJobCostingPlugInHelpers()).Returns(jobCostingPlugInHelpersMock.Object);
				mockIAccountingDependencyFactory.Setup(x => x.GetBranchLevelPostingHelper()).Returns(branchLevelPostingHelperMock.Object);

				jobCostingPlugInHelpersMock.Setup(mock => mock.FindBranchFromConsolAgentsAndJobHeaders(consol, GlbCompany.CurrentCompany.PK, Factory)).Returns(expectedBranch);

				var initialBranchPKValueInPassedTransaction = ZGuid.Empty;
				branchLevelPostingHelperMock.Setup(x => x.SetTransactionHeaderBranch(It.IsAny<ARInvoice>(), InvoiceProcessingLevelIsAllowingToResetBranch.Creation, It.IsAny<ITransactionBranchCalculationDataProviderFromJobCharge>()))
					.Callback<TransactionHeaderWithLines, InvoiceProcessingLevelIsAllowingToResetBranch, ITransactionBranchCalculationDataProviderFromJobCharge>(
						(transaction, invoiceLevel, charges) =>
						{
							initialBranchPKValueInPassedTransaction = transaction.AH_GB;
						}
					);

				var receivablesPostingCharges = new IReceivablesPostingChargeCollection();
				receivablesPostingCharges.Key = new PostingChargeKey(Creator.Agent.PK, InvoiceTypesList.Codes.FinalInvoice, "C00001234", ZGuid.Empty, ZGuid.Empty, 0);
				receivablesPostingCharges.Add(charge);
				var poster = GetChargePoster();
				var invoice = (ARInvoice)GetChargePoster().Post(receivablesPostingCharges);

				AssertEquals("initialBranchPKValueInPassedTransaction: ", expectedBranch.PK, initialBranchPKValueInPassedTransaction);

				jobCostingPlugInHelpersMock.Verify(x => x.FindBranchFromConsolAgentsAndJobHeaders(It.IsAny<IJobCostingPlugIn>(), It.IsAny<ZGuid>(), It.IsAny<BusinessObjectFactory>()), Times.Once);
				jobCostingPlugInHelpersMock.Verify(x => x.FindBranchFromConsolAgentsAndJobHeaders(consol, GlbCompany.CurrentCompany.PK, Factory));

				branchLevelPostingHelperMock.Verify(x => x.SetTransactionHeaderBranch(It.IsAny<ARInvoice>(), It.IsAny<InvoiceProcessingLevelIsAllowingToResetBranch>(), It.IsAny<ITransactionBranchCalculationDataProviderFromJobCharge>()), Times.Exactly(2));
				branchLevelPostingHelperMock.Verify(x => x.SetTransactionHeaderBranch(invoice, InvoiceProcessingLevelIsAllowingToResetBranch.Creation, receivablesPostingCharges));
			}
		}

		public void TestSetTransactionHeaderBranch_Consol_WhenGetBranchFromAgentsAndJobHeadersReturnNull()
		{
			if (this.GetType() == typeof(PeriodicInvoiceChargePosterTest))
			{
				Assert(true);
			}
			else
			{
				var consol = Creator.CreateConsol("AUSYD", "KRSEL", "C00001234");
				var shipment = Creator.CreateShipment("S00000001", consol);
				var job = Creator.CreateJob(shipment, false);
				var charge = CreateCharge(job, Creator.FRT, "Transaction Test", AUD, 3M, null, AUD, 4M, Creator.ABIGAS);
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

				Factory.Save();

				var mockIAccountingDependencyFactory = new Mock<IAccountingDependencyFactory>();
				var jobCostingPlugInHelpersMock = new Mock<IJobCostingPlugInHelpers>();
				var branchLevelPostingHelperMock = new Mock<IBranchLevelPostingHelper>();
				ObjectFactory.Substitute(mockIAccountingDependencyFactory.Object);

				mockIAccountingDependencyFactory.Setup(x => x.GetJobCostingPlugInHelpers()).Returns(jobCostingPlugInHelpersMock.Object);
				mockIAccountingDependencyFactory.Setup(x => x.GetBranchLevelPostingHelper()).Returns(branchLevelPostingHelperMock.Object);

				jobCostingPlugInHelpersMock.Setup(mock => mock.FindBranchFromConsolAgentsAndJobHeaders(consol, GlbCompany.CurrentCompany.PK, Factory)).Returns((GlbBranch)null);

				var initialBranchPKValueInPassedTransaction = ZGuid.Empty;
				branchLevelPostingHelperMock.Setup(x => x.SetTransactionHeaderBranch(It.IsAny<ARInvoice>(), InvoiceProcessingLevelIsAllowingToResetBranch.Creation, It.IsAny<ITransactionBranchCalculationDataProviderFromJobCharge>()))
					.Callback<TransactionHeaderWithLines, InvoiceProcessingLevelIsAllowingToResetBranch, ITransactionBranchCalculationDataProviderFromJobCharge>(
						(transaction, invoiceLevel, charges) =>
						{
							initialBranchPKValueInPassedTransaction = transaction.AH_GB;
						}
					);

				var receivablesPostingCharges = new IReceivablesPostingChargeCollection();
				receivablesPostingCharges.Key = new PostingChargeKey(Creator.Agent.PK, InvoiceTypesList.Codes.FinalInvoice, "C00001234", ZGuid.Empty, ZGuid.Empty, 0);
				receivablesPostingCharges.Add(charge);
				var poster = GetChargePoster();
				var invoice = (ARInvoice)GetChargePoster().Post(receivablesPostingCharges);

				AssertEquals("initialBranchPKValueInPassedTransaction: ", GlbBranch.CurrentBranch.PK, initialBranchPKValueInPassedTransaction);

				jobCostingPlugInHelpersMock.Verify(x => x.FindBranchFromConsolAgentsAndJobHeaders(It.IsAny<IJobCostingPlugIn>(), It.IsAny<ZGuid>(), It.IsAny<BusinessObjectFactory>()), Times.Once);
				jobCostingPlugInHelpersMock.Verify(x => x.FindBranchFromConsolAgentsAndJobHeaders(consol, GlbCompany.CurrentCompany.PK, Factory));

				branchLevelPostingHelperMock.Verify(x => x.SetTransactionHeaderBranch(It.IsAny<ARInvoice>(), It.IsAny<InvoiceProcessingLevelIsAllowingToResetBranch>(), It.IsAny<ITransactionBranchCalculationDataProviderFromJobCharge>()), Times.Exactly(2));
				branchLevelPostingHelperMock.Verify(x => x.SetTransactionHeaderBranch(invoice, InvoiceProcessingLevelIsAllowingToResetBranch.Creation, receivablesPostingCharges));
			}
		}

		public void TestBranchLevelPosting_Job()
		{
			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			var testObjectCreator = new TestObjectCreator(Factory);

			var shipment = testObjectCreator.CreateShipment("S0010001");
			var job = testObjectCreator.CreateJob(shipment, ABIGAS, 0, ZECTRA, 0);

			var charge1 = CreateCharge(job, MRG100, "Test Charge", null, 0, null, USD, 300, ABIGAS);
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			var charge2 = CreateCharge(job, MRG100, "Test Charge", null, 0, null, USD, 300, ABIGAS);
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			charge1.JR_SellReference = "ABC123";
			charge2.JR_SellReference = "ABC123";

			charge1.JR_GB = GlbBranch.CurrentBranch.PK;
			charge2.JR_GB = testObjectCreator.NonCurrentBranch.PK;

			PostingChargeCollection distributedCharges = new PostingChargeDistributor().DistributeCharges(job.ReceivableCharges);
			AssertEquals("2 collection of charges", 2, distributedCharges.Count);

			ARInvoice invoice = null;
			var poster = GetChargePoster();

			ZDateTime now = ZDateTime.Now;
			foreach (IReceivablesPostingChargeCollection charges in distributedCharges)
			{
				invoice = (ARInvoice)poster.Post(charges);
			}

			//set base rates to non-zero to allow saving;
			foreach (var r in job.ExchangeRates.Cast<ExchangeRate>().Where(r => r.JF_BaseRate.IsEmpty))
			{
				r.JF_BaseRate = 1m;
			}

			Factory.Save();

			if (this.GetType() == typeof(PeriodicInvoiceChargePosterTest))
			{
				//ToDo: check if for periodic invoice we do need this special case!
				Assert(true);
			}
			else
			{
				AssertEquals("Invoice Found", 2, poster.GetInvoices(ABIGAS).Length);

				var query = new ZQuery(AccTransactionHeaderSchema.AH_OH, ABIGAS.PK);
				query.AddToFilter(AccTransactionHeaderSchema.AH_GB, GlbBranch.CurrentBranch.PK);
				AssertEquals(1, poster.PostedInvoices.Find(query).Length);

				query = new ZQuery(AccTransactionHeaderSchema.AH_OH, ABIGAS.PK);
				query.AddToFilter(AccTransactionHeaderSchema.AH_GB, testObjectCreator.NonCurrentBranch.PK);
				AssertEquals(1, poster.PostedInvoices.Find(query).Length);
			}
		}

		public void TestBranchLevelPosting_Consol()
		{
			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			var testObjectCreator = new TestObjectCreator(Factory);

			var consol = testObjectCreator.CreateConsol("AUSYD", "KRSEL", "C00001234");
			var shipment = testObjectCreator.CreateShipment("S00000001");
			consol.Shipments.Add(shipment);

			var job = testObjectCreator.CreateJob(shipment, testObjectCreator.ABIGAS, 0, testObjectCreator.Agent, 0);

			var charge1 = CreateCharge(job, MRG100, "Test Charge", null, 0, null, USD, 300, ABIGAS);
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			var charge2 = CreateCharge(job, MRG100, "Test Charge", null, 0, null, USD, 300, ABIGAS);
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			charge1.JR_SellReference = "ABC123";
			charge2.JR_SellReference = "ABC123";

			charge1.JR_GB = testObjectCreator.NonCurrentBranch.PK;
			charge2.JR_GB = testObjectCreator.NonCurrentBranch.PK;

			//set base rates to non-zero to allow saving;
			foreach (var r in job.ExchangeRates.Cast<ExchangeRate>().Where(r => r.JF_BaseRate.IsEmpty))
			{
				r.JF_BaseRate = 1m;
			}

			Factory.Save();

			IReceivablesPostingChargeCollection charges = new IReceivablesPostingChargeCollection();
			charges.Key = new PostingChargeKey(testObjectCreator.Agent.PK, InvoiceTypesList.Codes.FinalInvoice, "C00001234", ZGuid.Empty, ZGuid.Empty, 0);
			charges.Add(charge1);
			charges.Add(charge2);

			var poster = GetChargePoster();
			ARInvoice invoice = (ARInvoice)poster.Post(charges);

			ZDateTime now = ZDateTime.Now;

			var arInvoices = poster.GetInvoices(ABIGAS);
			AssertEquals("Invoice Found", 1, arInvoices.Length);
			AssertEquals(charge1.JR_GB, arInvoices[0].AH_GB);
		}

		public void TestPlaceOfSupplyLevelPosting_Job()
		{
			using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForReceivableTransactions.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var testObjectCreator = new TestObjectCreator(Factory);

				var shipment = testObjectCreator.CreateShipment("S0010001");
				var job = testObjectCreator.CreateJob(shipment, ABIGAS, 0, ZECTRA, 0);

				var charge1 = CreateCharge(job, MRG100, "Test Charge", null, 0, null, USD, 300, ABIGAS);
				charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

				var charge2 = CreateCharge(job, MRG100, "Test Charge", null, 0, null, USD, 300, ABIGAS);
				charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

				charge1.JR_SellReference = "ABC123";
				charge2.JR_SellReference = "ABC123";

				charge1.JR_SellPlaceOfSupply = "NSW";
				charge2.JR_SellPlaceOfSupply = "WA";

				var distributedCharges = new PostingChargeDistributor().DistributeCharges(job.ReceivableCharges);
				AssertEquals("2 collection of charges", 2, distributedCharges.Count);

				ARInvoice invoice = null;
				var poster = GetChargePoster();

				var now = ZDateTime.Now;
				foreach (IReceivablesPostingChargeCollection charges in distributedCharges)
				{
					invoice = (ARInvoice)poster.Post(charges);
				}

				//set base rates to non-zero to allow saving;
				foreach (var r in job.ExchangeRates.Cast<ExchangeRate>().Where(r => r.JF_BaseRate.IsEmpty))
				{
					r.JF_BaseRate = 1m;
				}

				Factory.Save();

				if (this.GetType() == typeof(PeriodicInvoiceChargePosterTest))
				{
					//ToDo: check if for periodic invoice we do need this special case!
					Assert(true);
				}
				else
				{
					AssertEquals("Invoice Found", 2, poster.GetInvoices(ABIGAS).Length);
					AssertPlaceOfSupplyOnPostedInvoices("NSW");
					AssertPlaceOfSupplyOnPostedInvoices("WA");
				}

				void AssertPlaceOfSupplyOnPostedInvoices(ZString placeOfSupply)
				{
					var query = new ZQuery(AccTransactionHeaderSchema.AH_OH, ABIGAS.PK);
					query.AddToFilter(AccTransactionHeaderSchema.AH_PlaceOfSupply, placeOfSupply);
					var foundInvoices = poster.PostedInvoices.Find(query);
					AssertEquals(1, foundInvoices.Length);
					Assert(((InvoicingBase)foundInvoices[0]).Lines.Cast<InvoicingLineBase>().All(x => x.AL_PlaceOfSupply == placeOfSupply));
				}
			}
		}

		public void TestJobRefNum()
		{
			Job job = CreateJob("Z00001001", ABIGAS, true, 10, ZECTRA, true, 10);
			job.PlugInData = Factory.New<ForwardingShipment>();
			Charge charge = CreateCharge(job, MRG100, "Test Charge", null, 0, null, USD, 300, ABIGAS);
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			PostingChargeCollection distributedCharges = new PostingChargeDistributor().DistributeCharges(job.ReceivableCharges);
			AssertEquals("1 collection of charges", 1, distributedCharges.Count);

			ARInvoice invoice = null;
			ChargePoster poster = GetChargePoster();

			ZDateTime now = ZDateTime.Now;
			foreach (IReceivablesPostingChargeCollection charges in distributedCharges)
			{
				invoice = (ARInvoice)poster.Post(charges);
			}

			//set base rates to non-zero to allow saving;
			foreach (var r in job.ExchangeRates.Cast<ExchangeRate>().Where(r => r.JF_BaseRate.IsEmpty))
			{
				r.JF_BaseRate = 1m;
			}

			Factory.Save();

			if (this.GetType() == typeof(PeriodicInvoiceChargePosterTest))
			{
				Assert(true);
			}
			else
			{
				AssertEquals("Invoice Found", 1, poster.GetInvoices(AUD, ABIGAS, "Z00001001").Length);
				AssertEquals("Invoice Not Found", 0, poster.GetInvoices(AUD, ABIGAS, "Z000010011").Length);
			}
		}

		public void TestChangeTransactionDescriptionOnAllARInvoicesAndCFXLines()
		{
			Job job = CreateJob("Z00001001", ABIGAS, true, 10, ZECTRA, true, 10);
			job.PlugInData = Factory.New<ForwardingShipment>();
			Charge charge = CreateCharge(job, MRG100, "Test Charge", null, 0, null, USD, 300, ABIGAS);
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			PostingChargeCollection distributedCharges = new PostingChargeDistributor().DistributeCharges(job.ReceivableCharges);
			AssertEquals("1 collection of charges", 1, distributedCharges.Count);

			ARInvoice invoice = null;
			ChargePoster poster = GetChargePoster();

			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(Enterprise.ZArchitecture.Environment.EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertNotEquals(0, charge.JR_CFXAmt);
			foreach (IReceivablesPostingChargeCollection charges in distributedCharges)
			{
				invoice = (ARInvoice)poster.Post(charges);
			}

			Assert("Invoice Description", invoice.AH_Desc == "Z00001001" || invoice.AH_Desc == "AR PERIODIC INVOICE");
			AssertEquals("CFX Description", "JOB COSTING JOURNAL (CFX)", charge.CFXLine.TransactionHeader.AH_Desc);

			poster.ChangeTransactionDescriptionOnAllARInvoicesAndCFXLines("Test Description");
			job.ExchangeRates.Cast<ExchangeRate>().FirstOrDefault().JF_BaseRate = 1m; //set to non-zero to allow saving;
			Factory.Save();

			AssertEquals("Invoice Description", "Test Description", invoice.AH_Desc);
			AssertEquals("CFX Description", "Test Description", charge.CFXLine.TransactionHeader.AH_Desc);
		}

		public void TestChangeTransactionDateOnAllARInvoicesAndCFXLines_OtherTaxes()
		{
			var invoice1 = Factory.New<ARInvoice>();
			var taxParent1 = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice1);
			var invoice2 = Factory.New<ARCreditNote>();
			var taxParent2 = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice2);
			var invoice3 = Factory.New<ARInvoice>();
			var taxParent3 = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice3);

			var poster = GetChargePoster();
			poster.PostedInvoices.AddRange(invoice1, invoice2);

			var taxProcessorMock = new Mock<ITaxProcessor>(MockBehavior.Strict);
			ObjectFactory.Substitute(taxProcessorMock.Object);

			var postDatesByTaxParentPK = new Dictionary<ZGuid, ZDateTime>();
			taxProcessorMock.Setup(x => x.UpdatePostDate(It.IsAny<ITaxRecordParent>())).Callback((ITaxRecordParent parent) => postDatesByTaxParentPK[parent.PK] = parent.PostDate);

			var invoiceDate = ZDateTime.Now.AddDays(-5);
			var postDate = ZDateTime.Now.AddDays(-15);
			poster.ChangeTransactionDateOnAllARInvoicesAndCFXLines(invoiceDate, postDate);
			taxProcessorMock.Verify(x => x.UpdatePostDate(It.IsAny<ITaxRecordParent>()), Times.Exactly(2));
			taxProcessorMock.Verify(x => x.UpdatePostDate(taxParent1), Times.Once);
			taxProcessorMock.Verify(x => x.UpdatePostDate(taxParent2), Times.Once);
			taxProcessorMock.Verify(x => x.UpdatePostDate(taxParent3), Times.Never);

			AssertEquals(2, postDatesByTaxParentPK.Count);
			AssertEquals(postDate, postDatesByTaxParentPK[invoice1.PK]);
			AssertEquals(postDate, postDatesByTaxParentPK[invoice2.PK]);
		}

		public void TestChangeTransactionDateOnAllARInvoicesAndCFXLines()
		{
			Job job = CreateJob("Z00001001", ABIGAS, true, 10, ZECTRA, true, 10);
			job.PlugInData = Factory.New<ForwardingShipment>();
			Charge charge = CreateCharge(job, MRG100, "Test Charge", null, 0, null, USD, 300, ABIGAS);
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			PostingChargeCollection distributedCharges = new PostingChargeDistributor().DistributeCharges(job.ReceivableCharges);
			AssertEquals("1 collection of charges", 1, distributedCharges.Count);

			ARInvoice invoice = null;
			ChargePoster poster = GetChargePoster();

			ZDateTime now = ZDateTime.Now;
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(Enterprise.ZArchitecture.Environment.EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertNotEquals(0, charge.JR_CFXAmt);
			foreach (IReceivablesPostingChargeCollection charges in distributedCharges)
			{
				invoice = (ARInvoice)poster.Post(charges);
			}

			AssertEquals("Invoice Date should be now", now.Date, invoice.AH_InvoiceDate.Date);

			ZDateTime invoiceDate = now.AddDays(-5);
			ZDateTime postDate = now.AddDays(-15);
			poster.ChangeTransactionDateOnAllARInvoicesAndCFXLines(invoiceDate, postDate);
			charge.CostExchangeRate.SetBuyRate_ForTestOnly(1m); //set non-zero rate to allow saving
			Factory.Save();

			AssertEquals("Invoice Date should be 5 days ago", invoiceDate.Date, invoice.AH_InvoiceDate.Date);
			AssertEquals("Invoice Date should be 5 days ago", invoiceDate.Date, invoice.AH_DueDate.Date);
			AssertEquals("Post Date should be 15 days ago", postDate.Date, invoice.AH_PostDate.Date);
			AssertEquals("Line Post Date should be 15 days ago", postDate.Date, invoice.Lines[0].AL_PostDate.Date);
			AssertEquals("Line Reverse Date should be 15 days ago", postDate.Date, invoice.Lines[0].AL_ReverseDate.Date);
			AssertEquals("CFX Post Date should be 15 days ago", postDate.Date, charge.CFXLine.TransactionHeader.AH_PostDate.Date);
			AssertEquals("CFX Line Post Date should be 15 days ago", postDate.Date, charge.CFXLine.AL_PostDate.Date);
			AssertEquals("CFX Line Reverse Date should be 15 days ago", postDate.Date, charge.CFXLine.AL_ReverseDate.Date);
		}

		public void TestChangeTransactionDateOnAllARInvoicesAndCFXLines_EXTAccountingSystem()
		{
			Job job = CreateJob("Z00001001", ABIGAS, true, 10, ZECTRA, true, 10);
			job.PlugInData = Factory.New<ForwardingShipment>();
			Charge charge = CreateCharge(job, MRG100, "Test Charge", null, 0, null, USD, 300, ABIGAS);
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			PostingChargeCollection distributedCharges = new PostingChargeDistributor().DistributeCharges(job.ReceivableCharges);
			AssertEquals("1 collection of charges", 1, distributedCharges.Count);

			ARInvoice invoice = null;
			ChargePoster poster = GetChargePoster();

			ZDateTime now = ZDateTime.Now;
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(Enterprise.ZArchitecture.Environment.EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			var onHoldTerms = new OnHoldTerms();
			onHoldTerms.Terms = ARInvoiceTermsList.FromInvoiceDate.Code;
			onHoldTerms.TermDays = 7;

			var orgCreditControlCollection = new OrgsEvaluatedForCreditControlCollection();

			OrganisationRegistry.Instance.OnHoldTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, onHoldTerms);
			AccountingMasterFilesRegistry.Instance.CreditControlApprovalMode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.CreditControlApprovalModes.ApproveInExternalSystem.Code);
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControl.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, orgCreditControlCollection);

			AssertNotEquals(0, charge.JR_CFXAmt);
			foreach (IReceivablesPostingChargeCollection charges in distributedCharges)
			{
				invoice = (ARInvoice)poster.Post(charges);
			}

			AssertShouldGetARTermFromDefaultConfiguration(invoice, now.Date);

			ZDateTime invoiceDate = now.AddDays(-5);
			ZDateTime postDate = now.AddDays(-15);
			poster.ChangeTransactionDateOnAllARInvoicesAndCFXLines(invoiceDate, postDate);
			charge.CostExchangeRate.SetBuyRate_ForTestOnly(1m); //set non-zero rate to allow saving
			Factory.Save();

			AssertShouldGetARTermFromDefaultConfiguration(invoice, now.Date.AddDays(-5));
		}

		void AssertShouldGetARTermFromDefaultConfiguration(ARInvoice invoice, ZDate date)
		{
			CombineAssertions("Should get a ARTerm from the default configuration when Use ARInvoice Terms And Term Days When Credit Is OnHold is false.", () =>
			{
				AssertEquals("Default Use ARInvoice Terms And Term Days When Credit Is OnHold", false, OrganisationRegistry.Instance.UseARInvoiceTermsAndTermDaysWhenCreditIsOnHold.Value);
				AssertEquals("Default Invoice terms", ARInvoiceTermsList.CashOnDelivery.Code, invoice.AH_InvoiceTerm);
				AssertEquals("Default Term days", (ZByte)0, invoice.AH_InvoiceTermDays);
				AssertEquals("Due date", date, invoice.AH_DueDate.Date);
			});
		}

		public void TestChangeTransactionDateOnAllARInvoicesAndCFXLinesForSHPTerms()
		{
			ABIGAS.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = InvoiceTermsList.FromShipmentDate.Code;
			ABIGAS.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = 30;
			Factory.Save();

			Job job = CreateJob("Z00001001", ABIGAS, true, 10, ZECTRA, true, 10);
			job.PlugInData = Factory.New<ForwardingShipment>();
			Charge charge = CreateCharge(job, MRG100, "Test Charge", null, 0, null, USD, 300, ABIGAS);
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			PostingChargeCollection distributedCharges = new PostingChargeDistributor().DistributeCharges(job.ReceivableCharges);
			AssertEquals("1 collection of charges", 1, distributedCharges.Count);

			ARInvoice invoice = null;
			ChargePoster poster = GetChargePoster();

			ZDateTime now = ZDateTime.Now;
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(Enterprise.ZArchitecture.Environment.EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertNotEquals(0, charge.JR_CFXAmt);
			foreach (IReceivablesPostingChargeCollection charges in distributedCharges)
			{
				invoice = (ARInvoice)poster.Post(charges);
			}

			AssertEquals("Invoice Date should be now", now.Date, invoice.AH_InvoiceDate.Date);

			ZDateTime invoiceDate = now.AddDays(-5);
			ZDateTime postDate = now.AddDays(-15);
			poster.ChangeTransactionDateOnAllARInvoicesAndCFXLines(invoiceDate, postDate);
			job.ExchangeRates.Cast<ExchangeRate>().FirstOrDefault().JF_BaseRate = 1m; //set non-zero to allow saving
			Factory.Save();

			AssertEquals("Invoice Date should be 5 days ago", invoiceDate.Date, invoice.AH_InvoiceDate.Date);
			AssertEquals("Invoice Date should be 5 days ago", invoiceDate.AddDays(30).Date, invoice.AH_DueDate.Date);
			AssertEquals("Post Date should be 15 days ago", postDate.Date, invoice.AH_PostDate.Date);
			AssertEquals("Line Post Date should be 15 days ago", postDate.Date, invoice.Lines[0].AL_PostDate.Date);
			AssertEquals("Line Reverse Date should be 15 days ago", postDate.Date, invoice.Lines[0].AL_ReverseDate.Date);
			AssertEquals("CFX Post Date should be 15 days ago", postDate.Date, charge.CFXLine.TransactionHeader.AH_PostDate.Date);
			AssertEquals("CFX Line Post Date should be 15 days ago", postDate.Date, charge.CFXLine.AL_PostDate.Date);
			AssertEquals("CFX Line Reverse Date should be 15 days ago", postDate.Date, charge.CFXLine.AL_ReverseDate.Date);
		}

		public void TestLocalAmountRecalculatedOnJobChargeLinesWhenCollectInvoiceExRateChanged()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.SetDefaultReceivingForwarderAddress(ABIGAS);

			var jobConsol = consol as IJobCostingPlugIn;
			AssertEquals(jobConsol.AgentToInvoice()?.PK, jobConsol.ReceivingAgentAPInvoicingParty.PK);

			Job job = CreateJob("Z00001001", ABIGAS, true, 10, ZECTRA, true, 10);
			CreateExchangeRate(job, USD, 0.7423M);
			CreateExchangeRate(job, EUR, 0.5467M);
			Charge charge1 = CreateCharge(job, MRG100, "Test Charge", null, 0, null, USD, 300, ABIGAS);
			Charge charge2 = CreateCharge(job, MRG100, "Test Charge", null, 0, null, AUD, 150, ABIGAS);
			Charge charge3 = CreateCharge(job, MRG100, "Test Charge", null, 0, null, EUR, 275, ABIGAS);

			charge1.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge3.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			PostingChargeCollection distributedCharges = new ConsolPostingChargeDistributor(consol).DistributeCharges(job.ReceivableCharges);
			AssertEquals("1 collection of charges", 1, distributedCharges.Count);
			foreach (IReceivablesPostingChargeCollection charges in distributedCharges)
			{
				charges.PostingCurrency = creator.USD.RX_Code;
				charges.PostingCurrencyExchangeRate = 0.65m;
			}

			ARInvoice invoice = null;
			ChargePoster poster = GetChargePoster();

			AssertEquals("Original Charge local sell value before running poster", 404.15m, charge1.JR_LocalSellAmt);
			AssertEquals("Original Charge local sell value before running poster", 150.00m, charge2.JR_LocalSellAmt);
			AssertEquals("Original Charge local sell value before running poster", 503.02m, charge3.JR_LocalSellAmt);

			ZDateTime now = ZDateTime.Now;
			foreach (IReceivablesPostingChargeCollection charges in distributedCharges)
			{
				invoice = (ARInvoice)poster.Post(charges);
			}

			AssertEquals("Charge local sell value should be recalculated to the same as original", 503.02m, charge3.JR_LocalSellAmt);
			AssertEquals("Charge local sell value should be recalculated to the same as original", 150.00m, charge2.JR_LocalSellAmt);
			if (poster.OverrideChargeExchangeRatesOnPosting)
			{
				AssertEquals("Charge local sell value should be recalculated", 461.54m, charge1.JR_LocalSellAmt);
			}

			AssertEquals("Foreign amounts should not change", 275m, charge3.JR_OSSellAmt);
			AssertEquals("Foreign amounts should not change", 150m, charge2.JR_OSSellAmt);
			AssertEquals("Foreign amounts should not change", 300m, charge1.JR_OSSellAmt);

			AssertEquals("Sell Ex Rate should not be recalculated where posting currency not equal to charge currency",
					0.5467m, charge3.JR_OSSellExRate);
			AssertEquals("Sell Ex Rate should not be recalculated where posting currency not equal to charge currency",
					1m, charge2.JR_OSSellExRate);
			if (poster.OverrideChargeExchangeRatesOnPosting)
			{
				AssertEquals("Sell Ex Rate should be recalculated where posting currency equal to charge currency and posting exrate differs",
						0.65m, charge1.JR_OSSellExRate);
			}
		}

		public void TestChargesHasNoPostingReceivableChargesBusinessContextBeforeAndAfterPosting()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var charge = CreateCharge(job, testObjectCreator.FRT, "Revenue Transaction Test", AUD, 3M, null, AUD, 4M, AALSHI);

			Assert(!charge.HasContext(BusinessContext.PostingReceivableCharges));
			var invoice = new ChargePosterForPostingReceivableChargesBusinessContextTest(Factory).Post(charge);
			Assert(!charge.HasContext(BusinessContext.PostingReceivableCharges));
			AssertEquals(1, invoice.Lines.Count);
			Assert(charge.IsRevenuePosted);
		}

		class ChargePosterForPostingReceivableChargesBusinessContextTest : ChargePoster
		{
			public ChargePosterForPostingReceivableChargesBusinessContextTest(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override void SetInvoiceLineDetails(InvoicingLineBase line, IReceivablesPostingCharge charge,
				ZDateTime postTime, ZDate invoiceDate, ZString ledger, ZString postingCurrency, ZDecimal postingCurrencyExchangeRate,
				IReceivablesPostingChargeCollection charges)
			{
				base.SetInvoiceLineDetails(line, charge, postTime, invoiceDate, ledger, postingCurrency, postingCurrencyExchangeRate, charges);
				Assert(((Charge)charge).HasContext(BusinessContext.PostingReceivableCharges));
			}
		}

		public void TestPostForeignInvoiceInLocalWithTaxAndSmallExRate()
		{
			Job job = CreateJob("Z00001011", AALSHI, true, 0M, ABIGAS, true, 0m);
			CreateExchangeRate(job, USD, 0.0237M);

			AALSHI.CompanyData.SetARTaxApplicable(true);
			AALSHI.MiscServ.OM_ARWHTApplicable = true;
			var taxRate = new TestObjectCreator(Factory).CreateTaxRate("GST2", string.Empty, 7);
			CCODE1.AC_AT_GSTRate = taxRate.PK;

			Charge charge1 = CreateCharge(job, CCODE1, "Revenue Transaction Test", USD, 3.1M, null, USD, 3.10M, AALSHI);
			Charge charge2 = CreateCharge(job, CCODE1, "Revenue Transaction Test", USD, 11.13M, null, USD, 11.13M, AALSHI);

			charge1.JR_AT_SellGSTRate = taxRate.PK;
			charge1.JR_GB = ZGuid.NewZGuid();
			charge1.JR_GE = ZGuid.NewZGuid();
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			charge2.JR_AT_SellGSTRate = taxRate.PK;
			charge2.JR_GB = ZGuid.NewZGuid();
			charge2.JR_GE = ZGuid.NewZGuid();
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			IReceivablesPostingChargeCollection charges = new IReceivablesPostingChargeCollection();
			charges.AddRange((IReceivablesPostingCharge[])job.Charges.ToArray(typeof(IReceivablesPostingCharge)));
			PostingChargeKey key = new PostingChargeKey(AALSHI.PK, "FIN", job.JH_JobNum, ZGuid.Empty, ZGuid.Empty, 0);
			charges.Key = key;
			InvoicingBase invoice = GetChargePoster().Post(charges);
			InvoicingLineBase invoiceLine1 = invoice.Lines[0];
			InvoicingLineBase invoiceLine2 = invoice.Lines[1];

			AssertEquals("Currency of invoice should be local", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, invoice.AH_RX_NKTransactionCurrency);

			AssertEquals("OS Tax Amount - only when posting in local - ( ROUND( Round(3.1 / 0.0237, 2) * 0.07), 2",
																			9.16m, invoiceLine1.AL_OSTaxAmount);
			AssertEquals("OS Tax Amount - only when posting in local - ( ROUND( Round(11.13 / 0.0237, 2) * 0.07), 2",
																			32.87m, invoiceLine2.AL_OSTaxAmount);

			AssertEquals("Invoice total should be 600.42", 600.42m, invoice.AH_OSExTaxAmount);
			AssertEquals("GST Total", 42.03m, invoice.AH_OSTaxAmount);
		}

		public virtual void TestPostInvoiceForChargesWithMultipleForeignCurrencies()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_RL_NKLoadPort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.SetDefaultReceivingForwarderAddress(AALSHI);

			var jobConsol = consol as IJobCostingPlugIn;
			AssertEquals(jobConsol.AgentToInvoice()?.PK, jobConsol.ReceivingAgentAPInvoicingParty.PK);

			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();

			using (Job job1 = Job.CreateWithMutex(Factory, shipment1))
			using (Job job2 = Job.CreateWithMutex(Factory, shipment2))
			{
				SetupJob(job1, "S00001000", ABIGAS, true, 10, AALSHI, true, 10);
				CreateExchangeRate(job1, EUR, 0.54m);
				Charge charge1 = CreateCharge(job1, MRG100, "Test Charge", EUR, 0, null, EUR, 300, AALSHI);
				charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				AssertEquals("Local Sell on Charge 1", 617.28m, charge1.JR_LocalSellAmt);

				SetupJob(job2, "S00001001", ABIGAS, true, 10, AALSHI, true, 10);
				CreateExchangeRate(job2, GBP, 0.35m);
				Charge charge2 = CreateCharge(job2, MRG100, "Test Charge", GBP, 0, null, GBP, 300, AALSHI);
				charge2.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
				AssertEquals("Local Sell on Charge 2", 857.14m, charge2.JR_LocalSellAmt);

				IReceivablesPostingChargeCollection charges = new IReceivablesPostingChargeCollection();
				charges.Add(charge1);
				charges.Add(charge2);

				ConsolPostingChargeDistributor distributor = new ConsolPostingChargeDistributor(consol);
				PostingChargeCollection distributedCharges = distributor.DistributeCharges(charges);

				AssertEquals("1 collection of charges", 1, distributedCharges.Count);

				PostingChargeKey key = new PostingChargeKey(AALSHI.PK, "", "C00001000", ZGuid.Empty, ZGuid.Empty, 0);
				charges = distributedCharges[key];
				AssertNotNull("Should have found charges for posting", charges);

				charges.PostingCurrency = USD.RX_Code;
				charges.PostingCurrencyExchangeRate = 0.65m;

				foreach (IReceivablesPostingCharge charge in charges)
				{
					charge.InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
				}

				ChargePoster poster = GetChargePoster();
				poster.Post(charges);
				AssertEquals("Should have made only 1 invoice", 1, poster.PostedInvoices.Count);
				ARInvoice invoice = (ARInvoice)poster.PostedInvoices[0];
				AssertEquals("Should have 2 lines on invoice", 2, invoice.Lines.Count);
				if (poster.OverrideChargeExchangeRatesOnPosting)
				{
					AssertEquals("Invoice currency should be USD", USD.RX_Code, invoice.AH_RX_NKTransactionCurrency);
					AssertEquals("Invoice Exchange Rate should be 0.65", 0.65m, invoice.AH_ExchangeRate);
				}
				AssertEquals("Invoice total in USD", 918.25m, invoice.AH_OSExTaxAmount);

				ARInvoiceLine line1 = (ARInvoiceLine)invoice.Lines[0];
				ARInvoiceLine line2 = (ARInvoiceLine)invoice.Lines[1];

				AssertEquals("Line 1 local amount", 555.56m, line1.AL_LocalExTaxAmount);
				AssertEquals("Line 2 local amount", 857.14m, line2.AL_LocalExTaxAmount);

				AssertEquals("Line 1 currency", USD.RX_Code, line1.AL_RX_NKTransactionCurrency);
				AssertEquals("Line 2 currency", USD.RX_Code, line2.AL_RX_NKTransactionCurrency);

				AssertEquals("Line 1 OS Amount", 361.11m, line1.AL_OSAmount);
				AssertEquals("Line 2 OS Amount", 557.14m, line2.AL_OSAmount);
			}
		}

		public void TestGroupChargesTogetherOnPosting_WithoutGrouping()
		{
			Job job = CreateJob("Z00001001", ABIGAS, true, 10, ZECTRA, true, 10);
			InvoicingParam pluginData = new InvoicingParam();
			pluginData.ConsumerType = JobInvoicingConsumerTypes.Shipment;
			job.PlugInData = pluginData;

			AssertEquals("Precondition", JobInvoicingConsumerTypes.Shipment.CodeAndDescription,
					job.PlugInData.InvoicingSupporter.ConsumerType.CodeAndDescription);

			Charge charge1 = CreateCharge(job, MRG100, "Test Charge", null, 0, null, AUD, 100, ABIGAS);
			Charge charge2 = CreateCharge(job, MRG100, "Test Charge", null, 0, null, AUD, 200, ABIGAS);
			Charge charge3 = CreateCharge(job, CCODE1, "Test Charge", null, 0, null, AUD, 300, ABIGAS);
			Charge charge4 = CreateCharge(job, CCODE1, "Test Charge", null, 0, null, AUD, 400, ABIGAS);
			Charge charge5 = CreateCharge(job, CCODE1, "Test Charge", null, 0, null, AUD, 500, ABIGAS);

			charge1.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice;
			charge3.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice;
			charge4.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice;
			charge5.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice;

			PostingChargeCollection distributedCharges = new PostingChargeDistributor().DistributeCharges(job.ReceivableCharges);
			AssertEquals("1 collection of charges", 1, distributedCharges.Count);

			ARInvoice invoice = null;
			ChargePoster poster = GetChargePoster();

			foreach (IReceivablesPostingChargeCollection charges in distributedCharges)
			{
				invoice = (ARInvoice)poster.Post(charges);
			}

			AssertNotNull("Invoice should not be null", invoice);
			AssertEquals("Invoice Lines", 5, invoice.Lines.Count);

			AssertLineDetailsAreCorrect(invoice.Lines[0], "Line 1", MRG100, AUD, 100m, 0m, 100m);
			AssertLineDetailsAreCorrect(invoice.Lines[1], "Line 2", MRG100, AUD, 200m, 0m, 200m);
			AssertLineDetailsAreCorrect(invoice.Lines[2], "Line 3", CCODE1, AUD, 300m, 0m, 300m);
			AssertLineDetailsAreCorrect(invoice.Lines[3], "Line 4", CCODE1, AUD, 400m, 0m, 400m);
			AssertLineDetailsAreCorrect(invoice.Lines[4], "Line 5", CCODE1, AUD, 500m, 0m, 500m);

			AssertEquals("Charge1 JR_AL_ARLine", charge1.JR_AL_ARLine, invoice.Lines[0].PK);
			AssertEquals("Charge2 JR_AL_ARLine", charge2.JR_AL_ARLine, invoice.Lines[1].PK);
			AssertEquals("Charge3 JR_AL_ARLine", charge3.JR_AL_ARLine, invoice.Lines[2].PK);
			AssertEquals("Charge4 JR_AL_ARLine", charge4.JR_AL_ARLine, invoice.Lines[3].PK);
			AssertEquals("Charge5 JR_AL_ARLine", charge5.JR_AL_ARLine, invoice.Lines[4].PK);
		}

		public void TestGroupChargesTogetherOnPosting_WithoutGroupingAndTax()
		{
			ABIGAS.CompanyData.SetARTaxApplicable(true);

			Job job = CreateJob("Z00001001", ABIGAS, true, 10, ZECTRA, true, 10);
			InvoicingParam pluginData = new InvoicingParam();
			pluginData.ConsumerType = JobInvoicingConsumerTypes.Shipment;
			job.PlugInData = pluginData;

			AssertEquals("Precondition", JobInvoicingConsumerTypes.Shipment.CodeAndDescription,
					job.PlugInData.InvoicingSupporter.ConsumerType.CodeAndDescription);

			Charge charge1 = CreateCharge(job, MRG100, "Test Charge", null, 0, null, AUD, 100, ABIGAS);
			Charge charge2 = CreateCharge(job, MRG100, "Test Charge", null, 0, null, AUD, 200, ABIGAS);
			Charge charge3 = CreateCharge(job, CCODE1, "Test Charge", null, 0, null, AUD, 300, ABIGAS);
			Charge charge4 = CreateCharge(job, CCODE1, "Test Charge", null, 0, null, AUD, 400, ABIGAS);
			Charge charge5 = CreateCharge(job, CCODE1, "Test Charge", null, 0, null, AUD, 500, ABIGAS);

			charge1.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice;
			charge3.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice;
			charge4.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice;
			charge5.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice;

			PostingChargeCollection distributedCharges = new PostingChargeDistributor().DistributeCharges(job.ReceivableCharges);
			AssertEquals("1 collection of charges", 1, distributedCharges.Count);

			ARInvoice invoice = null;
			ChargePoster poster = GetChargePoster();

			foreach (IReceivablesPostingChargeCollection charges in distributedCharges)
			{
				invoice = (ARInvoice)poster.Post(charges);
			}

			AssertNotNull("Invoice should not be null", invoice);
			AssertEquals("Invoice Lines", 5, invoice.Lines.Count);

			AssertLineDetailsAreCorrect(invoice.Lines[0], "Line 1", MRG100, AUD, 100m, 10m, 110m);
			AssertLineDetailsAreCorrect(invoice.Lines[1], "Line 2", MRG100, AUD, 200m, 20m, 220m);
			AssertLineDetailsAreCorrect(invoice.Lines[2], "Line 3", CCODE1, AUD, 300m, 30m, 330m);
			AssertLineDetailsAreCorrect(invoice.Lines[3], "Line 4", CCODE1, AUD, 400m, 40m, 440m);
			AssertLineDetailsAreCorrect(invoice.Lines[4], "Line 5", CCODE1, AUD, 500m, 50m, 550m);

			AssertEquals("Charge1 JR_AL_ARLine", charge1.JR_AL_ARLine, invoice.Lines[0].PK);
			AssertEquals("Charge2 JR_AL_ARLine", charge2.JR_AL_ARLine, invoice.Lines[1].PK);
			AssertEquals("Charge3 JR_AL_ARLine", charge3.JR_AL_ARLine, invoice.Lines[2].PK);
			AssertEquals("Charge4 JR_AL_ARLine", charge4.JR_AL_ARLine, invoice.Lines[3].PK);
			AssertEquals("Charge5 JR_AL_ARLine", charge5.JR_AL_ARLine, invoice.Lines[4].PK);
		}

		public void TestGroupChargesTogetherOnPosting_WithoutGroupingAndTaxInForeignCurrency()
		{
			ABIGAS.CompanyData.SetARTaxApplicable(true);

			Job job = CreateJob("Z00001001", ABIGAS, true, 10, ZECTRA, true, 10);
			CreateExchangeRate(job, USD, 0.7m);
			InvoicingParam pluginData = new InvoicingParam();
			pluginData.ConsumerType = JobInvoicingConsumerTypes.Shipment;
			job.PlugInData = pluginData;

			AssertEquals("Precondition", JobInvoicingConsumerTypes.Shipment.CodeAndDescription,
					job.PlugInData.InvoicingSupporter.ConsumerType.CodeAndDescription);

			Charge charge1 = CreateCharge(job, MRG100, "Test Charge", null, 0, null, USD, 100, ABIGAS);
			Charge charge2 = CreateCharge(job, MRG100, "Test Charge", null, 0, null, USD, 200, ABIGAS);
			Charge charge3 = CreateCharge(job, CCODE1, "Test Charge", null, 0, null, USD, 300, ABIGAS);
			Charge charge4 = CreateCharge(job, CCODE1, "Test Charge", null, 0, null, USD, 400, ABIGAS);
			Charge charge5 = CreateCharge(job, CCODE1, "Test Charge", null, 0, null, USD, 500, ABIGAS);

			charge1.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge3.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge4.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge5.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			PostingChargeCollection distributedCharges = new PostingChargeDistributor().DistributeCharges(job.ReceivableCharges);
			AssertEquals("1 collection of charges", 1, distributedCharges.Count);

			ARInvoice invoice = null;
			ChargePoster poster = GetChargePoster();

			foreach (IReceivablesPostingChargeCollection charges in distributedCharges)
			{
				invoice = (ARInvoice)poster.Post(charges);
			}

			AssertNotNull("Invoice should not be null", invoice);
			AssertEquals("Invoice Lines", 5, invoice.Lines.Count);

			AssertLineDetailsAreCorrect(invoice.Lines[0], "Line 1", MRG100, USD,
					100m, 10m, 110m, 142.86m, 14.29m, 157.15m);

			AssertLineDetailsAreCorrect(invoice.Lines[1], "Line 2", MRG100, USD,
					200m, 20m, 220m, 285.71m, 28.57m, 314.28m);

			AssertLineDetailsAreCorrect(invoice.Lines[2], "Line 3", CCODE1, USD,
					300m, 30m, 330m, 428.57m, 42.86m, 471.43m);

			AssertLineDetailsAreCorrect(invoice.Lines[3], "Line 4", CCODE1, USD,
					400m, 40m, 440m, 571.43m, 57.14m, 628.57m);

			AssertLineDetailsAreCorrect(invoice.Lines[4], "Line 5", CCODE1, USD,
					500m, 50m, 550m, 714.29m, 71.43m, 785.72m);

			AssertEquals("Charge1 JR_AL_ARLine", charge1.JR_AL_ARLine, invoice.Lines[0].PK);
			AssertEquals("Charge2 JR_AL_ARLine", charge2.JR_AL_ARLine, invoice.Lines[1].PK);
			AssertEquals("Charge3 JR_AL_ARLine", charge3.JR_AL_ARLine, invoice.Lines[2].PK);
			AssertEquals("Charge4 JR_AL_ARLine", charge4.JR_AL_ARLine, invoice.Lines[3].PK);
			AssertEquals("Charge5 JR_AL_ARLine", charge5.JR_AL_ARLine, invoice.Lines[4].PK);
		}

		void AssertLineDetailsAreCorrect(InvoicingLineBase line, ZString description,
				AccChargeCode chargeCode, RefCurrency currency,
				ZDecimal oSExTaxAmount, ZDecimal oSTaxAmount, ZDecimal overseasTotal)
		{
			AssertLineDetailsAreCorrect(line, description, chargeCode, currency,
					oSExTaxAmount, oSTaxAmount, overseasTotal,
					oSExTaxAmount, oSTaxAmount, overseasTotal);
		}

		void AssertLineDetailsAreCorrect(InvoicingLineBase line, ZString description,
				AccChargeCode chargeCode, RefCurrency currency,
				ZDecimal oSExTaxAmount, ZDecimal oSTaxAmount, ZDecimal overseasTotal,
				ZDecimal localExTaxAmount, ZDecimal localTaxAmount, ZDecimal localTotal)
		{
			AssertEquals(description + " ChargeCode", chargeCode.AC_Code, line.ChargeCode.AC_Code);
			AssertEquals(description + " Currency", currency.RX_Code, line.AL_RX_NKTransactionCurrency);

			AssertEquals(description + " AL_OSExTaxAmount", oSExTaxAmount, line.AL_OSExTaxAmount);
			AssertEquals(description + " AL_OSTaxAmount", oSTaxAmount, line.AL_OSTaxAmount);
			AssertEquals(description + " AL_OverseasTotal", overseasTotal, line.AL_OverseasTotal);

			AssertEquals(description + " AL_LocalExTaxAmount", localExTaxAmount, line.AL_LocalExTaxAmount);
			AssertEquals(description + " AL_LocalTaxAmount", localTaxAmount, line.AL_LocalTaxAmount);
			AssertEquals(description + " AL_LocalTotalAmount", localTotal, line.AL_LocalTotalAmount);
		}

		public void TestPostInvoicesAndPaymentsToLoginBranch()
		{
			Job job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			CreateExchangeRate(job, USD, .7M);
			job.JH_GB = TestBranch.PK;
			job.JH_GE = TestDepartment.PK;

			ABIGAS.CompanyData.SetARTaxApplicable(false);
			ABIGAS.MiscServ.OM_ARWHTApplicable = true;
			Charge charge = CreateCharge(job, CCODE1, "Revenue Transaction Test", null, 0M, null, USD, 350M, ABIGAS);
			charge.JR_GB = TestBranch.PK;
			charge.JR_GE = TestDepartment.PK;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			Charge charge2 = CreateCharge(job, CCODE1, "Revenue Transaction Test2", null, 0M, null, USD, 350M, ABIGAS);
			charge2.JR_GB = TestBranch.PK;
			charge2.JR_GE = TestDepartment.PK;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			bool originalValue = AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.Value;

			try
			{
				AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				InvoicingBase invoice = GetChargePoster().Post(charge);
				InvoicingLineBase invoiceLine = invoice.Lines[0];
				AssertEquals("Department", ExpectedDepartmentPK, invoice.AH_GE);
				AssertEquals("Department", TestDepartment.PK, invoiceLine.AL_GE);
				AssertEquals("Branch", TestBranch.PK, invoiceLine.AL_GB);
				AssertEquals("Branch", ExpectedBranchPK, invoice.AH_GB);

				AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				invoice = GetChargePoster().Post(charge2);
				invoiceLine = invoice.Lines[0];
				AssertEquals("Department", ExpectedDepartmentPK, invoice.AH_GE);
				AssertEquals("Department", TestDepartment.PK, invoiceLine.AL_GE);
				AssertEquals("Branch", TestBranch.PK, invoiceLine.AL_GB);
				AssertEquals("Branch", GlbBranch.CurrentBranch.PK, invoice.AH_GB);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, originalValue);
			}
		}

		public void TestPostInvoicesAndPaymentsToLoginBranchConsolInvoice()
		{
			TestBranch.GB_OH_OrgProxy = Creator.ABIGAS.PK;
			TestBranch2.GB_OH_OrgProxy = Creator.AALSHI.PK;

			ForwardingConsol consol = Creator.CreateConsol("AUSYD", "KRSEL", "C00001234");
			ForwardingShipment shipment1 = Creator.CreateShipment("S00000001");
			ForwardingShipment shipment2 = Creator.CreateShipment("S00000002");
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);
			Job job1 = Creator.CreateJob(shipment1);
			Job job2 = Creator.CreateJob(shipment2);
			CreateExchangeRate(job1, USD, .7M);
			CreateExchangeRate(job2, USD, .7M);
			job1.JH_GB = TestBranch.PK;
			job1.JH_GE = TestDepartment.PK;
			job1.JH_OA_AgentCollectAddr = Creator.Agent.MainAddress.PK;
			job2.JH_GB = TestBranch.PK;
			job2.JH_GE = TestDepartment.PK;
			job2.JH_OA_AgentCollectAddr = Creator.Agent.MainAddress.PK;

			Charge charge1 = CreateCharge(job1, CCODE1, "Revenue Transaction Test", null, 0M, null, USD, 350M, Creator.Agent);
			Charge charge2 = CreateCharge(job2, CCODE1, "Revenue Transaction Test", null, 0M, null, USD, 350M, Creator.Agent);
			charge1.JR_GB = TestBranch.PK;
			charge1.JR_GE = TestDepartment.PK;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge2.JR_GB = TestBranch.PK;
			charge2.JR_GE = TestDepartment.PK;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			Factory.Save();

			IReceivablesPostingChargeCollection charges = new IReceivablesPostingChargeCollection();
			charges.Key = new PostingChargeKey(Creator.Agent.PK, InvoiceTypesList.Codes.FinalInvoice, "C00001234", ZGuid.Empty, ZGuid.Empty, 0);
			charges.Add(charge1);
			charges.Add(charge2);

			InvoicingBase invoice = GetChargePoster().Post(charges);

			AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "KRSEL";
			consol.JK_OA_SendingForwarderAddress = TestBranch2.OrgProxy.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			((IReversing)invoice).GenerateReverseTransaction(true);
			charge1.ClearRevenueLink();
			charge2.ClearRevenueLink();
			invoice = GetChargePoster().Post(charges);
			AssertEquals("System will use branch of sending agent proxy for export consol", TestBranch2.PK, invoice.AH_GB);

			consol.JK_RL_NKLoadPort = "KRSEL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_ReceivingForwarderAddress = TestBranch2.OrgProxy.MainAddress.PK;
			((IReversing)invoice).GenerateReverseTransaction(true);
			charge1.ClearRevenueLink();
			charge2.ClearRevenueLink();
			invoice = GetChargePoster().Post(charges);
			AssertEquals("System will use branch of receiving agent proxy for import consol", TestBranch2.PK, invoice.AH_GB);

			consol.JK_RL_NKLoadPort = "AUPER";
			consol.JK_RL_NKDischargePort = "AUMEL";
			((IReversing)invoice).GenerateReverseTransaction(true);
			charge1.ClearRevenueLink();
			charge2.ClearRevenueLink();
			invoice = GetChargePoster().Post(charges);
			AssertEquals("System will use branch of Jobheader for domestic consol", TestBranch.PK, invoice.AH_GB);

			consol.JK_RL_NKLoadPort = "KRSEL";
			consol.JK_RL_NKDischargePort = "KRPUS";
			AssertEquals("System will use branch of Jobheader for foreign consol", TestBranch.PK, invoice.AH_GB);

			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;

			((IReversing)invoice).GenerateReverseTransaction(true);
			charge1.ClearRevenueLink();
			charge2.ClearRevenueLink();
			invoice = GetChargePoster().Post(charges);
			AssertEquals("System will use branch of Jobheader When there are same Jobheaders", TestBranch.PK, invoice.AH_GB);

			job2.JH_GB = TestBranch3.PK;
			((IReversing)invoice).GenerateReverseTransaction(true);
			charge1.ClearRevenueLink();
			charge2.ClearRevenueLink();
			invoice = GetChargePoster().Post(charges);
			AssertEquals("Fall back to Login Branch When there are different Jobheaders", GlbBranch.CurrentBranch.PK, invoice.AH_GB);

			job1.JH_ParentID = ZGuid.Empty;
			job1.JH_ParentTableCode = string.Empty;
			job2.JH_ParentID = ZGuid.Empty;
			job2.JH_ParentTableCode = string.Empty;
			AssertEquals("Fall back to Login Branch When there are no Jobheaders", GlbBranch.CurrentBranch.PK, invoice.AH_GB);
		}

		protected virtual ZGuid ExpectedDepartmentPK
		{
			get { return TestDepartment.PK; }
		}

		protected virtual ZGuid ExpectedBranchPK
		{
			get { return TestBranch.PK; }
		}

		public void TestPostDSBRevenueChangeInvoiceTypeWhenLocalAmtLessThanTreatDisbursementsAsStandard()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CompanyData.OB_IsDebtor = true;
			org1.MiscServ.OM_ARTreatDisbursementsAsStandardValue = 150;
			org1.OH_FullName = "Test Debtor";

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.CompanyData.OB_IsDebtor = true;
			org2.MiscServ.OM_ARTreatDisbursementsAsStandardValue = 0;
			org2.OH_FullName = "Test Debtor 2";

			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			Job testJob = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			testJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			testJob.JH_JobNum = "S0001000";
			AccChargeCode disbursementChargeCode = creator.DSBChargeCode;
			ExchangeRate uSDRate = creator.CreateExchangeRate(testJob, USD, 0.65m);
			ExchangeRate gBPRate = creator.CreateExchangeRate(testJob, GBP, 1.65m);

			Charge testCharge = testJob.Charges.AddNew();
			testCharge.JR_OH_SellAccount = org1.PK;
			testCharge.JR_AC = creator.DSBChargeCode.PK;
			testCharge.JR_RX_NKSellCurrency = USD.RX_Code;
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.DestinationChargesInvoice;
			testCharge.JR_LocalSellAmt = 15m;

			Charge testChargeDisbursementInForeignCurrency = testJob.Charges.AddNew();
			testChargeDisbursementInForeignCurrency.JR_OH_SellAccount = org1.PK;
			testChargeDisbursementInForeignCurrency.JR_AC = creator.DSBChargeCode.PK;
			testChargeDisbursementInForeignCurrency.JR_RX_NKSellCurrency = USD.RX_Code;
			testChargeDisbursementInForeignCurrency.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInForeignCurrency;
			testChargeDisbursementInForeignCurrency.JR_LocalSellAmt = 30m;
			testChargeDisbursementInForeignCurrency.JR_OSSellAmt = 30m;

			Charge testChargeDisbursementInForeignCurrency2 = testJob.Charges.AddNew();
			testChargeDisbursementInForeignCurrency2.JR_OH_SellAccount = org1.PK;
			testChargeDisbursementInForeignCurrency2.JR_AC = creator.DSBChargeCode.PK;
			testChargeDisbursementInForeignCurrency2.JR_RX_NKSellCurrency = USD.RX_Code;
			testChargeDisbursementInForeignCurrency2.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInForeignCurrency;
			testChargeDisbursementInForeignCurrency2.JR_LocalSellAmt = 45m;

			Charge testChargeDisbursementInForeignCurrencyNegative = testJob.Charges.AddNew();
			testChargeDisbursementInForeignCurrencyNegative.JR_OH_SellAccount = org1.PK;
			testChargeDisbursementInForeignCurrencyNegative.JR_AC = creator.DSBChargeCode.PK;
			testChargeDisbursementInForeignCurrencyNegative.JR_RX_NKSellCurrency = GBP.RX_Code;
			testChargeDisbursementInForeignCurrencyNegative.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInForeignCurrency;
			testChargeDisbursementInForeignCurrencyNegative.JR_LocalSellAmt = 30m;
			testChargeDisbursementInForeignCurrencyNegative.JR_OSSellAmt = 30m;

			Charge testChargeDisbursementInForeignCurrencyNegative2 = testJob.Charges.AddNew();
			testChargeDisbursementInForeignCurrencyNegative2.JR_OH_SellAccount = org1.PK;
			testChargeDisbursementInForeignCurrencyNegative2.JR_AC = creator.DSBChargeCode.PK;
			testChargeDisbursementInForeignCurrencyNegative2.JR_RX_NKSellCurrency = GBP.RX_Code;
			testChargeDisbursementInForeignCurrencyNegative2.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInForeignCurrency;
			testChargeDisbursementInForeignCurrencyNegative2.JR_OH_CostAccount = creator.ABIGAS.PK;
			testChargeDisbursementInForeignCurrencyNegative2.JR_APInvoiceDate = ZDateTime.Today;
			testChargeDisbursementInForeignCurrencyNegative2.JR_APInvoiceNum = "INV0001";
			testChargeDisbursementInForeignCurrencyNegative2.JR_LocalSellAmt = -45m;

			Charge testChargeDisbursementInForeignCurrency_Batching = testJob.Charges.AddNew();
			testChargeDisbursementInForeignCurrency_Batching.JR_OH_SellAccount = org1.PK;
			testChargeDisbursementInForeignCurrency_Batching.JR_AC = creator.DSBChargeCode.PK;
			testChargeDisbursementInForeignCurrency_Batching.JR_RX_NKSellCurrency = USD.RX_Code;
			testChargeDisbursementInForeignCurrency_Batching.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching;
			testChargeDisbursementInForeignCurrency_Batching.JR_LocalSellAmt = 30m;
			testChargeDisbursementInForeignCurrency_Batching.JR_OSSellAmt = 30m;

			Charge testChargeDibursementInvoice = testJob.Charges.AddNew();
			testChargeDibursementInvoice.JR_OH_SellAccount = org1.PK;
			testChargeDibursementInvoice.JR_AC = creator.DSBChargeCode.PK;
			testChargeDibursementInvoice.JR_RX_NKSellCurrency = AUD.RX_Code;
			testChargeDibursementInvoice.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice;
			testChargeDibursementInvoice.JR_LocalSellAmt = 60m;

			Charge testChargeDibursementInvoice2 = testJob.Charges.AddNew();
			testChargeDibursementInvoice2.JR_OH_SellAccount = org1.PK;
			testChargeDibursementInvoice2.JR_AC = creator.DSBChargeCode.PK;
			testChargeDibursementInvoice2.JR_RX_NKSellCurrency = AUD.RX_Code;
			testChargeDibursementInvoice2.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice;
			testChargeDibursementInvoice2.JR_LocalSellAmt = 75m;

			Charge testChargeDibursementInvoice_Batching = testJob.Charges.AddNew();
			testChargeDibursementInvoice_Batching.JR_OH_SellAccount = org1.PK;
			testChargeDibursementInvoice_Batching.JR_AC = creator.DSBChargeCode.PK;
			testChargeDibursementInvoice_Batching.JR_RX_NKSellCurrency = AUD.RX_Code;
			testChargeDibursementInvoice_Batching.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice_Batching;
			testChargeDibursementInvoice_Batching.JR_LocalSellAmt = 60m;

			CommonShipment shipment2 = Factory.NewWithValidTestData<CommonShipment>();
			Job testJob2 = new Job.Loader(Factory, shipment2).TryCreateWithoutMutexForTestOnly();
			testJob2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			testJob2.JH_JobNum = "S0001001";
			ExchangeRate uSDRate2 = creator.CreateExchangeRate(testJob2, USD, 0.65m);
			ExchangeRate gBPRate2 = creator.CreateExchangeRate(testJob2, GBP, 1.65m);

			Charge testChargeDisbursementInvoiceAboveLimit = testJob2.Charges.AddNew();
			testChargeDisbursementInvoiceAboveLimit.JR_OH_SellAccount = org1.PK;
			testChargeDisbursementInvoiceAboveLimit.JR_AC = creator.DSBChargeCode.PK;
			testChargeDisbursementInvoiceAboveLimit.JR_RX_NKSellCurrency = AUD.RX_Code;
			testChargeDisbursementInvoiceAboveLimit.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice;
			testChargeDisbursementInvoiceAboveLimit.JR_LocalSellAmt = 600m;

			Charge testChargeDisbursementInForeignCurrencyAboveLimitNegative = testJob2.Charges.AddNew();
			testChargeDisbursementInForeignCurrencyAboveLimitNegative.JR_OH_SellAccount = org1.PK;
			testChargeDisbursementInForeignCurrencyAboveLimitNegative.JR_AC = creator.DSBChargeCode.PK;
			testChargeDisbursementInForeignCurrencyAboveLimitNegative.JR_RX_NKSellCurrency = GBP.RX_Code;
			testChargeDisbursementInForeignCurrencyAboveLimitNegative.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInForeignCurrency;
			testChargeDisbursementInForeignCurrencyAboveLimitNegative.JR_OH_CostAccount = creator.ABIGAS.PK;
			testChargeDisbursementInForeignCurrencyAboveLimitNegative.JR_APInvoiceDate = ZDateTime.Today;
			testChargeDisbursementInForeignCurrencyAboveLimitNegative.JR_APInvoiceNum = "INV0002";
			testChargeDisbursementInForeignCurrencyAboveLimitNegative.JR_LocalSellAmt = -600m;

			Charge testChargeDisbursementInForeignCurrencyAboveLimit = testJob2.Charges.AddNew();
			testChargeDisbursementInForeignCurrencyAboveLimit.JR_OH_SellAccount = org1.PK;
			testChargeDisbursementInForeignCurrencyAboveLimit.JR_AC = creator.DSBChargeCode.PK;
			testChargeDisbursementInForeignCurrencyAboveLimit.JR_RX_NKSellCurrency = USD.RX_Code;
			testChargeDisbursementInForeignCurrencyAboveLimit.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInForeignCurrency;
			testChargeDisbursementInForeignCurrencyAboveLimit.JR_LocalSellAmt = 600m;

			Charge testChargeDisbursementInForeignCurrencyForSecondOrganigationNegative = testJob2.Charges.AddNew();
			testChargeDisbursementInForeignCurrencyForSecondOrganigationNegative.JR_OH_SellAccount = org2.PK;
			testChargeDisbursementInForeignCurrencyForSecondOrganigationNegative.JR_AC = creator.DSBChargeCode.PK;
			testChargeDisbursementInForeignCurrencyForSecondOrganigationNegative.JR_RX_NKSellCurrency = GBP.RX_Code;
			testChargeDisbursementInForeignCurrencyForSecondOrganigationNegative.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInForeignCurrency;
			testChargeDisbursementInForeignCurrencyForSecondOrganigationNegative.JR_OH_CostAccount = creator.ABIGAS.PK;
			testChargeDisbursementInForeignCurrencyForSecondOrganigationNegative.JR_APInvoiceDate = ZDateTime.Today;
			testChargeDisbursementInForeignCurrencyForSecondOrganigationNegative.JR_APInvoiceNum = "INV0003";
			testChargeDisbursementInForeignCurrencyForSecondOrganigationNegative.JR_LocalSellAmt = -600m;

			Charge testChargeDisbursementInForeignCurrencyForSecondOrganigation = testJob2.Charges.AddNew();
			testChargeDisbursementInForeignCurrencyForSecondOrganigation.JR_OH_SellAccount = org2.PK;
			testChargeDisbursementInForeignCurrencyForSecondOrganigation.JR_AC = creator.DSBChargeCode.PK;
			testChargeDisbursementInForeignCurrencyForSecondOrganigation.JR_RX_NKSellCurrency = USD.RX_Code;
			testChargeDisbursementInForeignCurrencyForSecondOrganigation.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInForeignCurrency;
			testChargeDisbursementInForeignCurrencyForSecondOrganigation.JR_LocalSellAmt = 600m;

			Factory.Save();

			InvoicingPostManager poster = new InvoicingPostManager(testJob);
			poster.CreateTransactions(JobInvoicingPostingOption.Revenue);
			InvoicingPostManager poster2 = new InvoicingPostManager(testJob2);
			poster2.CreateTransactions(JobInvoicingPostingOption.Revenue);

			AssertEquals(4, poster.Poster.PostedInvoices.Count);
			AssertEquals("Invoice should be Final", false, poster.Poster.PostedInvoices[0].IsDisbursementOrFinal);
			AssertEquals("Charge should be posted", true, testCharge.IsRevenuePosted);
			AssertEquals("Charge InvoiceType should not be changed", InvoiceTypesList.Codes.DestinationChargesInvoice, testCharge.JR_InvoiceType);
			AssertEquals("Invoice should be Final", false, poster.Poster.PostedInvoices[1].IsDisbursementOrFinal);
			AssertEquals("Charge should be posted", true, testChargeDisbursementInForeignCurrency.IsRevenuePosted);
			AssertEquals("Charge InvoiceType should be changed", InvoiceTypesList.Codes.ForeignCurrencyInvoice, testChargeDisbursementInForeignCurrency.JR_InvoiceType);
			AssertEquals("Invoice should be Final", false, poster.Poster.PostedInvoices[2].IsDisbursementOrFinal);
			AssertEquals("Charge should be posted", true, testChargeDisbursementInForeignCurrencyNegative.IsRevenuePosted);
			AssertEquals("Charge InvoiceType should be changed", InvoiceTypesList.Codes.ForeignCurrencyInvoice, testChargeDisbursementInForeignCurrencyNegative.JR_InvoiceType);
			AssertEquals("Invoice should be Final", false, poster.Poster.PostedInvoices[3].IsDisbursementOrFinal);
			AssertEquals("Charge should be posted", false, testChargeDisbursementInForeignCurrency_Batching.IsRevenuePosted);

			AssertEquals(5, poster2.Poster.PostedInvoices.Count);
			AssertEquals("Invoice should be Disbursement", true, poster2.Poster.PostedInvoices[0].IsDisbursementOrFinal);
			AssertEquals("Charge should be posted", true, testChargeDisbursementInvoiceAboveLimit.IsRevenuePosted);
			AssertEquals("Charge InvoiceType should not be changed", InvoiceTypesList.Codes.DisbursementInvoice, testChargeDisbursementInvoiceAboveLimit.JR_InvoiceType);
			AssertEquals("Invoice should be Disbursement", true, poster2.Poster.PostedInvoices[1].IsDisbursementOrFinal);
			AssertEquals("Charge should be posted", true, testChargeDisbursementInvoiceAboveLimit.IsRevenuePosted);
			AssertEquals("Charge InvoiceType should not be changed", InvoiceTypesList.Codes.DisbursementInForeignCurrency, testChargeDisbursementInForeignCurrencyAboveLimitNegative.JR_InvoiceType);
			AssertEquals("Invoice should be Disbursement", true, poster2.Poster.PostedInvoices[2].IsDisbursementOrFinal);
			AssertEquals("Charge should be posted", true, testChargeDisbursementInForeignCurrencyAboveLimit.IsRevenuePosted);
			AssertEquals("Charge InvoiceType should not be changed", InvoiceTypesList.Codes.DisbursementInForeignCurrency, testChargeDisbursementInForeignCurrencyAboveLimit.JR_InvoiceType);
			AssertEquals("Invoice should be always Disbursement for TreatDisbursementsAsStandardValue is zero", true, poster2.Poster.PostedInvoices[3].IsDisbursementOrFinal);
			AssertEquals("Charge should be posted", true, testChargeDisbursementInForeignCurrencyForSecondOrganigationNegative.IsRevenuePosted);
			AssertEquals("Charge InvoiceType should not be changed", InvoiceTypesList.Codes.DisbursementInForeignCurrency, testChargeDisbursementInForeignCurrencyForSecondOrganigationNegative.JR_InvoiceType);
			AssertEquals("Invoice should be always Disbursement for TreatDisbursementsAsStandardValue is zero", true, poster2.Poster.PostedInvoices[4].IsDisbursementOrFinal);
			AssertEquals("Charge should be posted", true, testChargeDisbursementInForeignCurrencyForSecondOrganigation.IsRevenuePosted);
			AssertEquals("Charge InvoiceType should not be changed", InvoiceTypesList.Codes.DisbursementInForeignCurrency, testChargeDisbursementInForeignCurrencyForSecondOrganigation.JR_InvoiceType);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestPostForeignCurrencyInvoiceWithOverrideLocalSellAmount()
		{
			bool isReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;
			BusinessObjectFactory factory = new BusinessObjectFactory();
			GlbCompany company = null;
			try
			{
				company = factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
				company.GC_IsReciprocal = true;
				factory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

				Job job = CreateJob("Z00001011", AALSHI, true, 0M, ABIGAS, true, 0m);
				CreateExchangeRate(job, USD, 7.78m);

				AALSHI.CompanyData.SetARTaxApplicable(true);
				AALSHI.MiscServ.OM_ARWHTApplicable = true;
				Charge charge1 = CreateCharge(job, CCODE1, "Revenue Transaction Test", USD, 65.32M, null, USD, 65.32M, AALSHI);
				GST1.SetRateNumerator_ForTestOnly(7);
				charge1.JR_AT_SellGSTRate = GST1.PK;
				charge1.JR_GB = ZGuid.NewZGuid();
				charge1.JR_GE = ZGuid.NewZGuid();
				charge1.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
				AssertEquals("Default value for Local Sell Amt", 508.19m, charge1.JR_LocalSellAmt);
				charge1.JR_LocalSellAmt = 508.20m;
				AssertEquals("Overriden value for Local Sell Amt", 508.20m, charge1.JR_LocalSellAmt);

				IReceivablesPostingChargeCollection charges = new IReceivablesPostingChargeCollection();
				charges.AddRange((IReceivablesPostingCharge[])job.Charges.ToArray(typeof(IReceivablesPostingCharge)));
				PostingChargeKey key = new PostingChargeKey(AALSHI.PK, InvoiceTypesList.Codes.ForeignCurrencyInvoice, job.JH_JobNum, ZGuid.Empty, ZGuid.Empty, 0);
				charges.Key = key;
				InvoicingBase invoice = GetChargePoster().Post(charges);
				InvoicingLineBase invoiceLine1 = invoice.Lines[0];

				AssertEquals("Invoice Currency", Constants.CurrencyCodes.UnitedStates, invoice.AH_RX_NKTransactionCurrency);
				AssertEquals("OSExTaxAmount", 65.32m, invoiceLine1.AL_OSExTaxAmount);
				AssertEquals("LocalExTaxAmount", 508.20m, invoiceLine1.AL_LocalExTaxAmount);
			}
			finally
			{
				company.GC_IsReciprocal = isReciprocal;
				factory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
			}
		}

		public void TestPostingForeignCurrencyChargePreservesDebtorJobExchangeRate_FIN()
		{
			AssertPostingForeignCurrencyChargePreservesDebtorJobExchangeRate(InvoiceTypesList.Codes.FinalInvoice, AUD.Code);
		}

		public void TestPostingForeignCurrencyChargePreservesDebtorJobExchangeRate_CUR()
		{
			AssertPostingForeignCurrencyChargePreservesDebtorJobExchangeRate(InvoiceTypesList.Codes.ForeignCurrencyInvoice, USD.Code);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		void AssertPostingForeignCurrencyChargePreservesDebtorJobExchangeRate(string invoiceType, string expectedTransactionCurrency)
		{
			bool isReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;
			BusinessObjectFactory factory = new BusinessObjectFactory();
			GlbCompany company = null;
			try
			{
				company = factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
				company.GC_IsReciprocal = true;
				factory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

				var job = CreateJob("Z00001011", AALSHI, true, 0m, ABIGAS, true, 0m);
				var exRateDEB = CreateExchangeRate(job, USD, 0.78m);
				exRateDEB.JF_OrgType = ExchangeRateOrgTypeEnum.Debtor.ToCode();
				exRateDEB.JF_OH_Org = AALSHI.PK;
				exRateDEB.JF_IsTransformed = false;

				var charge = CreateCharge(job, CCODE1, "Revenue Transaction Test", USD, 100m, null, USD, 100m, AALSHI);
				charge.JR_InvoiceType = invoiceType;
				AssertEquals("Default value for Local Sell Amt", 78m, charge.JR_LocalSellAmt);

				AssertEquals(2, job.ExchangeRates.Count);
				var exRateCRD = job.ExchangeRates.Cast<ExchangeRate>().First(x => x.PK != exRateDEB.PK);
				exRateCRD.JF_BaseRate = 0.75m;
				AssertEquals(USD.Code, exRateCRD.JF_RX_NKRateCurrency);
				AssertEquals(ExchangeRateOrgTypeEnum.Creditor.ToCode(), exRateCRD.JF_OrgType);
				Assert(exRateCRD.JF_OH_Org.IsEmpty);
				Assert(!exRateCRD.JF_IsTransformed);

				var charges = new IReceivablesPostingChargeCollection();
				charges.AddRange(job.Charges.Cast<IReceivablesPostingCharge>());
				var key = new PostingChargeKey(AALSHI.PK, invoiceType, job.JH_JobNum, ZGuid.Empty, ZGuid.Empty, 0);
				charges.Key = key;
				var invoice = GetChargePoster().Post(charges);
				AssertEquals("Transaction currency", expectedTransactionCurrency, invoice.AH_RX_NKTransactionCurrency);

				AssertEquals(2, job.ExchangeRates.Count);
				Assert("Old Exchange Rates", job.ExchangeRates.Cast<ExchangeRate>().All(x => x.PK == exRateDEB.PK || x.PK == exRateCRD.PK));
				Assert("DEB rate should be Transformed to keep after posting", exRateDEB.JF_IsTransformed);
				Assert(!exRateCRD.JF_IsTransformed);
			}
			finally
			{
				company.GC_IsReciprocal = isReciprocal;
				factory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
			}
		}

		[TestDate(2015, 01, 01)]
		public void TestPost_GetDefaultARInvoiceDate()
		{
			var creator = new TestObjectCreator(Factory);

			//MonthEndSuspension configed, hidden registry CurrentInvoiceDate configed as not current year/month
			var configDateTime = ZDateTime.Now.AddMonths(1).ToDateTime();
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.MonthEndSuspension.Code);
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, configDateTime);

			var shipment = creator.CreateShipment("S00001000");
			var job = creator.CreateJob(shipment, false);
			var charge = creator.CreateCharge(job, creator.CC1, "Desc",
					creator.AUD, 100m, creator.AALSHI,
					creator.AUD, 100m, creator.ABIGAS);
			var chargePoster = new ChargePoster(Factory);
			var invoice = chargePoster.Post(charge);
			Factory.Save();

			AssertEquals("should be the last day of the configed year/month",
					new ZDateTime(configDateTime.Year, configDateTime.Month, DateTime.DaysInMonth(configDateTime.Year, configDateTime.Month))
					, invoice.AH_InvoiceDate);

			AssertEquals("should be the last day of the configed year/month",
					new ZDateTime(configDateTime.Year, configDateTime.Month, DateTime.DaysInMonth(configDateTime.Year, configDateTime.Month))
					, invoice.AH_PostDate);
		}

		public void TestInvoiceTermOverriddenMessage()
		{
			bool prevValue = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			try
			{
				var creator = new TestObjectCreator(Factory);
				var expectedMessage = "[ABIGAS] : Invoice terms are CUS - 4 Days from Customs Clearance date. As there is no CLR event on this job, Invoice Term bases on Shipment date";

				GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
				creator.ABIGAS.OH_IsCreditor = true;
				creator.ABIGAS.OH_IsDebtor = true;
				OrgARTerms term = creator.ABIGAS.CompanyData.ARTerms[0];
				creator.ABIGAS.CompanyData.OB_ARVATConfig = "DEF";
				term.PY_InvoiceClass = OrgARTermsLookups.InvoiceTypes.All.Code;
				term.PY_InvoiceTerm = InvoiceTermsList.FromCustomsClearanceDate.Code;
				term.PY_InvoiceDays = 4;

				var shipment = creator.CreateShipment("S000012", "AUSYD", "USCHI");
				shipment.JS_E_ARV = new ZDateTime(2015, 07, 25);
				var job = creator.CreateJob(shipment, creator.ABIGAS, 1.0m, creator.Agent, 1.0M);

				var charge1 = creator.CreateCharge(job, creator.CC1, "CHARGE 1", creator.AUD, 100m, creator.Creditor1, creator.USD, 200m, creator.ABIGAS);
				var charge2 = creator.CreateCharge(job, creator.CC1, "CHARGE 2", creator.AUD, 100m, creator.Creditor1, creator.USD, 200m, creator.ABIGAS);
				charge1.JR_OSSellExRate = 0.65m;
				charge2.JR_OSSellExRate = 0.42m;
				charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				charge1.JR_SellReference = "ABC123";
				charge2.JR_SellReference = "ABC123";

				//set base rates to non-zero to allow saving;
				foreach (var r in job.ExchangeRates.Cast<ExchangeRate>().Where(r => r.JF_BaseRate.IsEmpty))
				{
					r.JF_BaseRate = 1m;
				}

				Factory.Save();

				var distributor = new PostingChargeDistributor();
				var charges = new IReceivablesPostingChargeCollection();
				charges.Add(charge1);
				charges.Add(charge2);

				var results = distributor.DistributeCharges(charges);
				var key = results.Keys.Cast<PostingChargeKey>().First();
				ChargePoster poster = new ChargePoster(Factory);
				var invoice = poster.Post(results.GetCharges(key));

				AssertEquals("Invoice Term Overridde notification", expectedMessage, invoice.MessageForInvoiceTermOverriding);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = prevValue;
			}
		}

		public void TestPostingChargesWithSellInvoiceCurrency()
		{
			var job = CreateJob("Z00001011", AALSHI, true, 0M, ABIGAS, true, 0m);
			CreateExchangeRate(job, USD, 0.5M);

			var charge1 = CreateCharge(job, CCODE1, "Revenue Transaction Test", AUD, 0M, null, AUD, 100M, AALSHI);
			var charge2 = CreateCharge(job, CCODE1, "Revenue Transaction Test", AUD, 0M, null, AUD, 200M, AALSHI);

			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge1.JR_RX_NKSellInvoiceCurrency = "USD";
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge2.JR_RX_NKSellInvoiceCurrency = "USD";

			var charges = new IReceivablesPostingChargeCollection();
			charges.AddRange((IReceivablesPostingCharge[])job.Charges.ToArray(typeof(IReceivablesPostingCharge)));
			var distributedCharges = new PostingChargeDistributor().DistributeCharges(charges);
			AssertEquals("One charge in collection", 1, distributedCharges.Count);
			var chargePoster = GetChargePoster();

			var invoices = new List<InvoicingBase>();
			foreach (IReceivablesPostingChargeCollection c in distributedCharges)
			{
				var invoice = chargePoster.Post(c);
				invoices.Add(invoice);
			}
			AssertEquals("Should create 1 invoice", 1, invoices.Count);
			AssertEquals("Invoice should have 2 lines", 2, invoices[0].Lines.Count);
			AssertEquals("Currency of invoice should be sell invoice currency(USD)", "USD", invoices[0].AH_RX_NKTransactionCurrency);

			AssertEquals("OS Tax Amount", 50m, invoices[0].Lines[0].AL_OSAmount);
			AssertEquals("OS Tax Amount", 100m, invoices[0].Lines[1].AL_OSAmount);

			AssertEquals("Invoice total", 150m, invoices[0].AH_OSExTaxAmount);
		}

		public void TestPostingChargesWithSellInvoiceCurrencyAndLocalCurrency()
		{
			AssertPostingChargesWithSellInvoiceCurrencyAndLocalCurrency(withCFX: false);
		}

		public void TestPostingChargesWithSellInvoiceCurrencyWithCFXAndLocalCurrency()
		{
			AssertPostingChargesWithSellInvoiceCurrencyAndLocalCurrency(withCFX: true);

			AssertNoExceptionThrown("To make sure it passes Critical Validation", () => Factory.Save());
		}

		void AssertPostingChargesWithSellInvoiceCurrencyAndLocalCurrency(bool withCFX)
		{
			var job = CreateJob("Z00001011", AALSHI, true, 0M, ABIGAS, true, 0m);
			CreateExchangeRate(job, USD, 0.5M);
			if (withCFX)
			{
				AALSHI?.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 5m);
			}

			var charge1 = CreateCharge(job, CCODE1, "Revenue Transaction Test", AUD, 0M, null, AUD, 100M, AALSHI);
			var charge2 = CreateCharge(job, CCODE1, "Revenue Transaction Test", AUD, 0M, null, AUD, 200M, AALSHI);

			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge1.JR_RX_NKSellInvoiceCurrency = "USD";
			charge1.JR_AT_SellGSTRate = GST1.PK;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge2.JR_AT_SellGSTRate = GST1.PK;

			var charges = new IReceivablesPostingChargeCollection();
			charges.AddRange((IReceivablesPostingCharge[])job.Charges.ToArray(typeof(IReceivablesPostingCharge)));
			var distributedCharges = new PostingChargeDistributor().DistributeCharges(charges);
			AssertEquals("Two charges in collection", 2, distributedCharges.Count);
			var chargePoster = GetChargePoster();

			var invoices = new List<InvoicingBase>();
			foreach (IReceivablesPostingChargeCollection c in distributedCharges)
			{
				var invoice = chargePoster.Post(c);
				invoices.Add(invoice);
			}
			AssertEquals("Should create 2 invoices", 2, invoices.Count);

			var localCurrencyInvoice = invoices.FirstOrDefault(x => x.AH_RX_NKTransactionCurrency == "AUD");
			var sellInvoiceCurrencyInvoice = invoices.FirstOrDefault(x => x.AH_RX_NKTransactionCurrency == "USD");

			if (withCFX)
			{
				AssertEquals("sellInvoiceCurrencyInvoice OS Ex Tax takes into account CFX", 52.5m, sellInvoiceCurrencyInvoice.AH_OSExTaxAmount);
				AssertEquals("sellInvoiceCurrencyInvoice OS Tax takes into account CFX", 5.25m, sellInvoiceCurrencyInvoice.AH_OSTaxAmount);
				AssertEquals("sellInvoiceCurrencyInvoice local total takes into account CFX", 105m, sellInvoiceCurrencyInvoice.AH_InvoiceAmount);
			}
			else
			{
				AssertEquals("sellInvoiceCurrencyInvoice OS Ex Tax", 50m, sellInvoiceCurrencyInvoice.AH_OSExTaxAmount);
				AssertEquals("sellInvoiceCurrencyInvoice OS Tax", 5m, sellInvoiceCurrencyInvoice.AH_OSTaxAmount);
				AssertEquals("sellInvoiceCurrencyInvoice local total", 100m, sellInvoiceCurrencyInvoice.AH_InvoiceAmount);
			}
			AssertEquals("sellInvoiceCurrencyInvoice should have 1 lines", 1, sellInvoiceCurrencyInvoice.Lines.Count);

			AssertEquals("localCurrencyInvoice Ex Tax", 200m, localCurrencyInvoice.AH_OSExTaxAmount);
			AssertEquals("localCurrencyInvoice Tax", 20m, localCurrencyInvoice.AH_OSTaxAmount);
			AssertEquals("localCurrencyInvoice should have 1 lines", 1, localCurrencyInvoice.Lines.Count);
		}

		public void TestPostingChargesWithSellInvoiceCurrencyAndOtherForeignCurrency()
		{
			var job = CreateJob("Z00001011", AALSHI, true, 0M, ABIGAS, true, 0m);
			CreateExchangeRate(job, USD, 0.5M);
			CreateExchangeRate(job, EUR, 0.4M);

			var charge1 = CreateCharge(job, CCODE1, "Revenue Transaction Test", AUD, 0M, null, AUD, 100M, AALSHI);
			var charge2 = CreateCharge(job, CCODE1, "Revenue Transaction Test", EUR, 0M, null, EUR, 200M, AALSHI);

			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge1.JR_RX_NKSellInvoiceCurrency = "USD";
			charge1.JR_AT_SellGSTRate = GST1.PK;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge2.JR_AT_SellGSTRate = GST1.PK;

			var charges = new IReceivablesPostingChargeCollection();
			charges.AddRange((IReceivablesPostingCharge[])job.Charges.ToArray(typeof(IReceivablesPostingCharge)));
			var distributedCharges = new PostingChargeDistributor().DistributeCharges(charges);
			AssertEquals("Two charges in collection of charges", 2, distributedCharges.Count);
			var chargePoster = GetChargePoster();

			var invoices = new List<InvoicingBase>();
			foreach (IReceivablesPostingChargeCollection c in distributedCharges)
			{
				var invoice = chargePoster.Post(c);
				invoices.Add(invoice);
			}
			AssertEquals("Should create 2 invoices", 2, invoices.Count);

			var sellInvoiceCurrencyInvoice = invoices.FirstOrDefault(x => x.AH_RX_NKTransactionCurrency == "USD");
			var otherForeignCurrencyInvoice = invoices.FirstOrDefault(x => x.AH_RX_NKTransactionCurrency == "EUR");

			AssertEquals("sellInvoiceCurrencyInvoice Ex Tax", 50m, sellInvoiceCurrencyInvoice.AH_OSExTaxAmount);
			AssertEquals("sellInvoiceCurrencyInvoice OS Tax", 5m, sellInvoiceCurrencyInvoice.AH_OSTaxAmount);
			AssertEquals("sellInvoiceCurrencyInvoice local total", 100m, sellInvoiceCurrencyInvoice.AH_InvoiceAmount);
			AssertEquals("sellInvoiceCurrencyInvoice should have 1 lines", 1, sellInvoiceCurrencyInvoice.Lines.Count);

			AssertEquals("OtherForeignCurrencyInvoice OS Ex Tax", 200m, otherForeignCurrencyInvoice.AH_OSExTaxAmount);
			AssertEquals("sellInvoiceCurrencyInvoice OS Tax", 20m, otherForeignCurrencyInvoice.AH_OSTaxAmount);
			AssertEquals("sellInvoiceCurrencyInvoice local total", 500m, otherForeignCurrencyInvoice.AH_InvoiceAmount);
			AssertEquals("OtherForeignCurrencyInvoice should have 1 lines", 1, otherForeignCurrencyInvoice.Lines.Count);
		}

		public void TestPostingReceivableChargesForFactoryLevelContext()
		{
			var job = CreateJob("Z00001011", AALSHI, true, 0M, ABIGAS, true, 0m);

			var charge1 = CreateCharge(job, CCODE1, "Revenue Transaction Test", AUD, 0M, null, AUD, 100M, AALSHI);

			var charges = new IReceivablesPostingChargeCollection();
			charges.Add(charge1);
			var distributedCharges = new PostingChargeDistributor().DistributeCharges(charges);

			var chargePoster = GetChargePoster();

			Assert("Percondition : should not has PostingReceivableChargesForFactoryLevel Context", !Factory.HasContext(BusinessContext.PostingReceivableChargesForFactoryLevel));

			foreach (IReceivablesPostingChargeCollection c in distributedCharges)
			{
				chargePoster.Post(c);
			}

			Assert(Factory.HasContext(BusinessContext.PostingReceivableChargesForFactoryLevel));

			Factory.Save();

			Assert(Factory.HasContext(BusinessContext.PostingReceivableChargesForFactoryLevel));
		}

		public void TestPostingChargesWithZeroLocalSellTaxAmount()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Indonesia))
			using (GlbCompany.CurrentCompany.TemporarilySetCurrency(Core.Constants.CurrencyCodes.UnitedStates))
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = false;
				var job = CreateJob("Z00001011", AALSHI, true, 0M, ABIGAS, true, 0m);
				var idrCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "IDR"));
				CreateExchangeRate(job, idrCurrency, 14476M);
				var charge = CreateCharge(job, CCODE1, "test", USD, 0M, null, idrCurrency, 7238M, AALSHI);
				charge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
				GST1.SetRate_ForTestOnly(1, 1);
				charge.JR_AT_SellGSTRate = GST1.PK;

				var charges = new IReceivablesPostingChargeCollection();
				charges.AddRange((IReceivablesPostingCharge[])job.Charges.ToArray(typeof(IReceivablesPostingCharge)));
				var distributedCharges = new PostingChargeDistributor().DistributeCharges(charges);
				var chargePoster = GetChargePoster();
				var invoices = new List<InvoicingBase>();
				foreach (IReceivablesPostingChargeCollection c in distributedCharges)
				{
					invoices.Add(chargePoster.Post(c));
				}

				Assert(Factory.HasContext(BusinessContext.PostingReceivableChargesForTaxCalculation));
				AssertEquals("Should create 1 invoices", 1, invoices.Count);
				var invoice = invoices[0];
				AssertEquals("Should create 1 invoice line", 1, invoice.Lines.Count);
				AssertEquals(7238m, invoice.AH_OSTotalAmount);
				AssertEquals(0m, invoice.AH_OSTaxAmount);
				var line = invoice.Lines[0];
				AssertEquals(7238m, line.AL_OSExTaxAmount);
				AssertEquals(0m, line.AL_OSTaxAmount);
			}
		}

		[TestDate(2023, 2, 10)]
		public void TestBackDatePostingWithZeroLocalSellTaxAmountAndTaxRateChanged_CUR()
		{
			AssertBackDatePostingWithZeroLocalSellTaxAmountAndTaxRateChanged(InvoiceTypesList.Codes.ForeignCurrencyInvoice, 5m, 0.5m, 0.25m, 10m, false);
		}

		[TestDate(2023, 2, 10)]
		public void TestBackDatePostingWithZeroLocalSellTaxAmountAndTaxRateChanged_FIN()
		{
			AssertBackDatePostingWithZeroLocalSellTaxAmountAndTaxRateChanged(InvoiceTypesList.Codes.FinalInvoice, 0.5m, 0.05m, 0.03m, 1m, true);
		}

		protected virtual void AssertBackDatePostingWithZeroLocalSellTaxAmountAndTaxRateChanged(string invoiceType, Decimal oSExTaxAmount, Decimal oSTaxAmount, Decimal whtTaxAmount, Decimal exChangeRate, bool isBillInLocalCurrency)
		{
			var taxDateOptioncollection = new TaxDateDefaultingOptionCollection();
			var taxDateOption = taxDateOptioncollection.AddNew();
			taxDateOption.JobType = "ALL";
			taxDateOption.DirectionCode = "ALL";
			taxDateOption.Mode = "ALL";
			taxDateOption.Ledger = "AR";
			taxDateOption.TaxDateOption = "INV";

			using (AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, taxDateOptioncollection))
			{
				var job = Creator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				CreateExchangeRate(job, USD, 10M);

				var taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
				taxRate.SetRate_ForTestOnly(10, 1, new ZDate(2022, 12, 1), new ZDate(2022, 12, 31));
				taxRate.SetRate_ForTestOnly(5, 10, new ZDate(2023, 1, 1), new ZDate(2023, 1, 31));
				taxRate.SetRate_ForTestOnly(2, 10, new ZDate(2023, 2, 1), new ZDate(2023, 12, 31));

				var charge = CreateCharge(job, CCODE1, "test", USD, 0M, null, USD, 5M, AALSHI);
				charge.JR_InvoiceType = invoiceType;
				charge.JR_AT_SellGSTRate = taxRate.PK;
				charge.JR_SellTaxDate = ZDate.Empty;
				charge.JR_AW_SellWHTRate = Creator.WHT1.PK;
				Factory.Save();

				var chargePoster = GetChargePoster();
				var line = chargePoster.Post(charge).Lines[0];

				AssertEquals("Precondition: BillInLocalCurrency", isBillInLocalCurrency, charge.BillInLocalCurrency);

				AssertExchangeRateAndAmountWhenBackingDate(line, "after posting, localTax is zero",
					0m, exChangeRate, oSExTaxAmount, whtTaxAmount, charge);

				var newInvoiceDate = new ZDate(2023, 1, 15);
				chargePoster.ChangeTransactionDateOnAllARInvoicesAndCFXLines(newInvoiceDate, newInvoiceDate);
				AssertExchangeRateAndAmountWhenBackingDate(line, "after backing date, localTax is zero",
					0m, exChangeRate, oSExTaxAmount, whtTaxAmount, charge);

				newInvoiceDate = new ZDate(2022, 12, 15);
				chargePoster.ChangeTransactionDateOnAllARInvoicesAndCFXLines(newInvoiceDate, newInvoiceDate);
				AssertExchangeRateAndAmountWhenBackingDate(line, "after backing date, localTax is not zero",
					oSTaxAmount, exChangeRate, oSExTaxAmount, whtTaxAmount, charge);

				charge.JR_RX_NKSellCurrency = "EUR";
				charge.JR_OSSellExRate = 20m;
				charge.JR_InvoiceType = invoiceType;
				newInvoiceDate = new ZDate(2023, 1, 15);
				chargePoster.ChangeTransactionDateOnAllARInvoicesAndCFXLines(newInvoiceDate, newInvoiceDate);
				AssertExchangeRateAndAmountWhenBackingDate(line, "charge currency different to posting currency, after backing date, localTax is zero",
					0m, exChangeRate, oSExTaxAmount, whtTaxAmount, charge);

				newInvoiceDate = new ZDate(2022, 12, 15);
				chargePoster.ChangeTransactionDateOnAllARInvoicesAndCFXLines(newInvoiceDate, newInvoiceDate);
				AssertExchangeRateAndAmountWhenBackingDate(line, "charge currency different to posting currency, after backing date, localTax is not zero",
					Utilities.Round(oSTaxAmount / 2, 2), exChangeRate, oSExTaxAmount, whtTaxAmount, charge);
			}
		}

		void AssertExchangeRateAndAmountWhenBackingDate(InvoicingLineBase line, string description, decimal osTaxAmt, decimal exchangeRate, decimal osExTaxAmt, decimal whtAmt, Charge charge)
		{
			CombineAssertions($"Only copy AL_OSTaxAmount from charge to line, and charge is reLinked to line: {description}", () =>
			{
				AssertEquals("AL_OSTaxAmount", osTaxAmt, line.AL_OSTaxAmount);
				AssertEquals("AL_ExchangeRate", exchangeRate, line.AL_ExchangeRate);
				AssertEquals("AL_OSExTaxAmount", osExTaxAmt, line.AL_OSExTaxAmount);
				AssertEquals("AL_OSWHTAmount", whtAmt, line.AL_OSWHTAmount);
				AssertEquals("AL_LocalExTaxAmount", 0.5m, line.AL_LocalExTaxAmount);
				AssertEquals("AL_TaxDate", charge.JR_SellTaxDate, line.AL_TaxDate);

				AssertEquals("JR_AL_ARLine", line.PK, charge.JR_AL_ARLine);
			});
		}

		#region DueDate

		[TestDate(2004, 5, 10)]
		public void TestSetDueDateForForwardingShipment()
		{
			AssertDueDateForARInvoice(typeof(ForwardingShipment));
		}

		[TestDate(2004, 5, 10)]
		public void TestSetDueDateForAgencyBooking()
		{
			AssertDueDateForARInvoice(ObjectFactory.GetType<Freight.Integration.Agency.IAgencyBooking>());
		}

		void AssertDueDateForARInvoice(Type shipmentType)
		{
			ZDateTime expectedShipmentDate = new ZDateTime(2004, 5, 15);
			ZDateTime expectedDueDate = new ZDateTime(2004, 6, 5);

			ABIGAS.CompanyData.CreateOrLoadARTerm(InvoiceTypesList.Codes.FinalInvoice).PY_InvoiceTerm = Constants.InvoiceTerms.FromShipmentDate;
			ABIGAS.CompanyData.CreateOrLoadARTerm(InvoiceTypesList.Codes.FinalInvoice).PY_InvoiceDays = 21;

			Job job = CreateJob("Z00001001", ABIGAS, true, 10, ZECTRA, true, 10);

			CommonShipment testShipment = Factory.New(shipmentType) as CommonShipment;
			job.PlugInData = testShipment;

			testShipment.JS_UniqueConsignRef = "Z00001001";
			testShipment.JS_RL_NKOrigin = "USLAX";
			testShipment.JS_RL_NKDestination = "CNSHA";

			testShipment.JS_E_ARV = expectedShipmentDate;

			Factory.Save();

			Charge charge = CreateCharge(job, MRG100, "Test Charge", null, 0, null, AUD, 300, ABIGAS);
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			PostingChargeCollection distributedCharges = new PostingChargeDistributor().DistributeCharges(job.ReceivableCharges);
			AssertEquals("1 collection of charges", 1, distributedCharges.Count);

			ARInvoice invoice = null;
			ChargePoster poster = GetChargePoster();

			ZDateTime now = ZDateTime.Now;
			foreach (IReceivablesPostingChargeCollection charges in distributedCharges)
			{
				invoice = (ARInvoice)poster.Post(charges);
			}

			AssertEquals("Invoice Date should be now", now, invoice.AH_InvoiceDate.Date);
			AssertEquals("Invoice Date should be now", expectedDueDate, invoice.AH_DueDate.Date);
		}

		[TestDate(2018, 10, 10)]
		public void TestCalculateDueDateBasedOnOnHoldTermsRegistryForEXTSystems_NoApprovalRequestCreated_NoOrgExemptForCreditCheck()
		{
			SetupAndAssertCalculateDueDateBasedOnOnHoldTermsRegistryForEXTSystems(false, false, true);
		}

		[TestDate(2018, 10, 10)]
		public void TestCalculateDueDateBasedOnOnHoldTermsRegistryForEXTSystems_NoApprovalRequestCreated_OrgExemptForCreditCheck()
		{
			SetupAndAssertCalculateDueDateBasedOnOnHoldTermsRegistryForEXTSystems(false, false, false);
		}

		[TestDate(2018, 10, 10)]
		public void TestCalculateDueDateBasedOnOnHoldTermsRegistryForEXTSystems_PendingApproval_NoOrgExemptForCreditCheck()
		{
			SetupAndAssertCalculateDueDateBasedOnOnHoldTermsRegistryForEXTSystems(true, false, true);
		}

		[TestDate(2018, 10, 10)]
		public void TestCalculateDueDateBasedOnOnHoldTermsRegistryForEXTSystems_PendingApproval_OrgExemptForCreditCheck()
		{
			SetupAndAssertCalculateDueDateBasedOnOnHoldTermsRegistryForEXTSystems(true, false, false);
		}

		[TestDate(2018, 10, 10)]
		public void TestCalculateDueDateBasedOnOnHoldTermsRegistryForEXTSystems_CreditRequestApproved_NoOrgExemptForCreditCheck()
		{
			SetupAndAssertCalculateDueDateBasedOnOnHoldTermsRegistryForEXTSystems(true, true, true);
		}

		[TestDate(2018, 10, 10)]
		public void TestCalculateDueDateBasedOnOnHoldTermsRegistryForEXTSystems_CreditRequestApproved_OrgExemptForCreditCheck()
		{
			SetupAndAssertCalculateDueDateBasedOnOnHoldTermsRegistryForEXTSystems(true, true, false);
		}

		void SetupAndAssertCalculateDueDateBasedOnOnHoldTermsRegistryForEXTSystems(bool createApprovalRequest, bool isApproved, bool overrideOrgExemptionForCreditCheck)
		{
			var onHoldTerms = new OnHoldTerms();
			onHoldTerms.Terms = ARInvoiceTermsList.FromInvoiceDate.Code;
			onHoldTerms.TermDays = 7;

			var orgCreditControlCollection = new OrgsEvaluatedForCreditControlCollection();

			using (OrganisationRegistry.Instance.OnHoldTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, onHoldTerms))
			using (AccountingMasterFilesRegistry.Instance.CreditControlApprovalMode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.CreditControlApprovalModes.ApproveInExternalSystem.Code))
			{
				if (overrideOrgExemptionForCreditCheck)
				{
					AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControl.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, orgCreditControlCollection);
				}

				var chargeCode = Creator.FRT;
				var debtor = Creator.Debtor;
				var shipment = Creator.CreateShipment("S001001", false);
				var job = Creator.CreateJob(shipment, false);
				var charge = Creator.CreateCharge(job, chargeCode, "freight", Creator.AUD, 100M, Creator.LocalClient, Creator.AUD, 120M, debtor);

				if (createApprovalRequest)
				{
					var factory = new BusinessObjectFactory();
					var approvalRequest = factory.New<CreditControlledDocumentsApproval>();
					approvalRequest.Initialize(shipment, ZGuid.Empty, new int[] { 3 });
					if (isApproved)
					{
						approvalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
					}
					factory.Save();
				}

				var invoiceDate = ZDateTime.Today;
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

				PostingChargeCollection distributedCharges = new PostingChargeDistributor().DistributeCharges(job.ReceivableCharges);
				AssertEquals("1 collection of charges", 1, distributedCharges.Count);

				ARInvoice invoice = null;
				ChargePoster poster = GetChargePoster();

				ZDateTime now = ZDateTime.Now;
				foreach (IReceivablesPostingChargeCollection charges in distributedCharges)
				{
					invoice = (ARInvoice)poster.Post(charges);
				}

				CombineAssertions("Should get a ARTerm from the default configuration when Use ARInvoice Terms And Term Days When Credit Is OnHold is false.", () =>
				{
					AssertEquals("Default Use ARInvoice Terms And Term Days When Credit Is OnHold", false, OrganisationRegistry.Instance.UseARInvoiceTermsAndTermDaysWhenCreditIsOnHold.Value);
					AssertEquals("Default Invoice terms", ARInvoiceTermsList.CashOnDelivery.Code, invoice.AH_InvoiceTerm);
					AssertEquals("Default Term days", (ZByte)0, invoice.AH_InvoiceTermDays);
					AssertEquals("Due date", invoiceDate, invoice.AH_DueDate);
				});
			}
		}

		[TestDate(2013, 10, 15)]
		public void TestSetDueDateCorrectlyWhenConsolsHaveMultipleShipments()
		{
			ZDateTime expectedShipmentDate = new ZDateTime(2013, 10, 15);
			ZDateTime expectedDueDate = new ZDateTime(2013, 10, 20);

			OrgARTerms term = ABIGAS.CompanyData.CreateOrLoadARTerm("ALL");
			term.PY_InvoiceTerm = Constants.InvoiceTerms.FromShipmentDate;
			term.PY_InvoiceDays = 5;

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_OA_ReceivingForwarderAddress = ABIGAS.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = ABIGAS.MainAddress.PK;

			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S00001001";
			shipment1.JS_RL_NKOrigin = "USLAX";
			shipment1.JS_RL_NKDestination = "CNSHA";
			shipment1.JS_E_ARV = expectedShipmentDate;
			Job job1 = CreateJob("S00001001", ABIGAS, true, 10, ZECTRA, true, 10);
			job1.PlugInData = shipment1;

			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S00001002";
			shipment2.JS_RL_NKOrigin = "USLAX";
			shipment2.JS_RL_NKDestination = "CNSHA";
			shipment2.JS_E_ARV = expectedShipmentDate;
			Job job2 = CreateJob("S00001002", ABIGAS, true, 10, ZECTRA, true, 10);
			job2.PlugInData = shipment2;

			Factory.Load<GlbDepartment>(GlbDepartment.CurrentDepartment.PK).GE_Misc = false; // To pass Job Charge Validation;
			Factory.Save();

			Charge charge1 = CreateCharge(job1, MRG100, "Test Charge", null, 0, null, AUD, 300, ABIGAS);
			charge1.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge1.JR_GB = GlbBranch.CurrentBranch.PK;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			Charge charge2 = CreateCharge(job2, MRG100, "Test Charge", null, 0, null, AUD, 300, ABIGAS);
			charge2.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge2.JR_GB = GlbBranch.CurrentBranch.PK;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			var jobs = new[] { job1, job2 };
			TransactionCreatorHashtable transactions = new ConsolInvoicingPostManager(Factory, jobs, consol, new ApportionmentListing(Factory, consol)).CreateTransactions(JobInvoicingPostingOption.Agent);
			AssertEquals(1, transactions.ARTransactionsCount);
			TransactionHeader invoice = transactions.GetAllARTransactions()[0];
			AssertEquals("expect invoice date same as shipment date", expectedShipmentDate, invoice.AH_InvoiceDate);
			AssertEquals("expect due date is shipment date + 5 days.", expectedDueDate, invoice.AH_DueDate);
		}

		[ExpectNoExceptions]
		public void TestPostOverseasAgentChargesFromConsolShouldNotRaiseExceptionDueToReceivingAgentSameWithChargesDebtor()
		{
			AssertPostOverseasAgentChargesFromConsol("S00001001", true, false);
			AssertPostOverseasAgentChargesFromConsol("S00001002", false, false);
			AssertPostOverseasAgentChargesFromConsol("S00001003", false, true);
		}

		void AssertPostOverseasAgentChargesFromConsol(string jobNumber, bool firstChargeUseForeignCurrencyInvoiceType, bool secondChargeUseLocalSellCurrency)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
			consol.JK_OA_ReceivingForwarderAddress = ABIGAS.MainAddress.PK;

			var jobConsol = consol as IJobCostingPlugIn;
			AssertNotEquals(jobConsol.AgentToInvoice()?.PK, jobConsol.ReceivingAgentAPInvoicingParty.PK);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = jobNumber;
			var job = CreateJob(jobNumber, AALSHI, true, 10, ABIGAS, true, 10);
			job.PlugInData = shipment;
			Factory.Save();

			var charge1 = CreateCharge(job, MRG100, "Test Charge1", AUD, 100, null, USD, 100, ABIGAS);
			charge1.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge1.JR_GB = GlbBranch.CurrentBranch.PK;
			if (firstChargeUseForeignCurrencyInvoiceType)
			{
				charge1.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			}
			else
			{
				charge1.JR_RX_NKSellInvoiceCurrency = EUR.Code;
				charge1.SellInvoiceExchangeRate.SetBaseRate(0.85m);
				charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			}

			var sellcurrency = secondChargeUseLocalSellCurrency ? AUD : EUR;
			var charge2 = CreateCharge(job, MRG100, "Test Charge2", AUD, 100, null, sellcurrency, 100, ABIGAS);
			charge2.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge2.JR_GB = GlbBranch.CurrentBranch.PK;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			Factory.Save();

			var distributedCharges = new ConsolPostingChargeDistributor(consol).DistributeCharges(job.ReceivableCharges);
			AssertEquals(2, distributedCharges.Count);

			var invoices = new List<Invoice>();
			foreach (IReceivablesPostingChargeCollection charges in distributedCharges)
			{
				var invoice = (ARInvoice)GetChargePoster().Post(charges);
				Factory.Save();
				invoices.Add(invoice);
			}

			AssertEquals(2, invoices.Count);
			var currencies = invoices.Select(x => x.AH_RX_NKTransactionCurrency);
			AssertEquals(2, currencies.Count());
			AssertCollectionContains("AUD", currencies, true);
			AssertCollectionContains(firstChargeUseForeignCurrencyInvoiceType ? "USD" : "EUR", currencies, true);
		}

		public void TestCalculateAmountsFromChargeReportErrorWithChargeIsRevenuePosted()
		{
			ChargePoster poster = GetChargePoster();

			var taxRate = new TestObjectCreator(Factory).CreateTaxRate("GST2", string.Empty, 7);
			CCODE1.AC_AT_GSTRate = taxRate.PK;

			var job = CreateJob("Z00001011", AALSHI, true, 0M, ABIGAS, true, 0m);
			CreateExchangeRate(job, USD, 4M);
			var charge = CreateCharge(job, CCODE1, "test", USD, 0M, null, USD, 100M, AALSHI);
			charge.JR_AT_SellGSTRate = taxRate.PK;

			var charges = new IReceivablesPostingChargeCollection();
			charges.AddRange((IReceivablesPostingCharge[])job.Charges.ToArray(typeof(IReceivablesPostingCharge)));
			var distributedCharges = new PostingChargeDistributor().DistributeCharges(charges);
			var chargePoster = GetChargePoster();

			InvoicingBase invoice = null;

			AssertEquals("1 collection of charges", 1, distributedCharges.Count);

			foreach (IReceivablesPostingChargeCollection c in distributedCharges)
			{
				invoice = chargePoster.Post(c);
			}

			AssertEquals("1 collection of invoice lines", 1, invoice.Lines.Count);
			Assert("Precondition", charge.JR_IsRevenuePosted);
			ErrorReporter.Clear();

			poster.CopyExchangeRateAndAmount(invoice.Lines[0], charge, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, 1);

			AssertEquals("Function CalculateAmountsFromCharge cannot be used if charge is revenue Posted, otherwise it is possible to return an incorrect Line Tax Amount.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#endregion

		#region Implementation

		bool OlCCODE1GSTRegistered;
		bool OlCCODE1WHTRegistered;
		protected TestObjectCreator Creator;
		protected GlbCompany TestCompany;
		protected GlbBranch TestBranch;
		protected GlbBranch TestBranch2;
		protected GlbBranch TestBranch3;
		protected GlbDepartment TestDepartment;

		protected override void SetUp()
		{
			base.SetUp();

			Creator = new TestObjectCreator(Factory);

			OlCCODE1GSTRegistered = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			OlCCODE1WHTRegistered = GlbCompany.CurrentCompany.GC_IsWHTRegistered;
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = true;

			TestCompany = GlbCompany.CurrentCompany;
			TestBranch = Creator.CreateBranch("AAA", "AAA NAME", TestCompany);
			TestBranch2 = Creator.CreateBranch("BBB", "BBB NAME", TestCompany);
			TestBranch3 = Creator.CreateBranch("CCC", "CCC NAME", TestCompany);
			TestDepartment = Creator.NonCurrentDepartment;

			Factory.Save();
		}

		protected override void TearDown()
		{
			base.TearDown();

			GlbCompany.CurrentCompany.GC_IsWHTRegistered = OlCCODE1WHTRegistered;
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = OlCCODE1GSTRegistered;
		}

		ChargePoster GetChargePoster()
		{
			return (ChargePoster)GetNewBusinessObject();
		}

		#region Create Business Objects

		Job CreateJob(ZString jobNumber, OrgHeader localClient, bool billLocalClientInLocalCurrency, decimal localClientCFX,
				OrgHeader agent, bool billAgentInLocalCurrency, decimal agentCFX)
		{
			Job job = Factory.NewJobForTesting<Job>();
			SetupJob(job, jobNumber, localClient, billLocalClientInLocalCurrency, localClientCFX, agent, billAgentInLocalCurrency, agentCFX);
			return job;
		}

		void SetupJob(Job job, ZString jobNumber, OrgHeader localClient, bool billLocalClientInLocalCurrency, decimal localClientCFX,
				OrgHeader agent, bool billAgentInLocalCurrency, decimal agentCFX)
		{
			job.JH_JobNum = jobNumber;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.LocalChargesPK = localClient.PK;
			job.AgentCollectPK = agent.PK;

			localClient?.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", localClientCFX);
			agent?.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", agentCFX);
		}

		ExchangeRate CreateExchangeRate(Job parentJob, RefCurrency currency, decimal buyRate)
		{
			ExchangeRate exchangeRate = parentJob.ExchangeRates.AddNew();
			exchangeRate.JF_RX_NKRateCurrency = currency.RX_Code;
			exchangeRate.JF_BaseRate = buyRate;
			return exchangeRate;
		}

		Charge CreateCharge(Job parentJob, AccChargeCode chargeCode, ZString desc, RefCurrency costCurrency, ZDecimal oSCostAmt, OrgHeader creditor,
				RefCurrency sellCurrency, ZDecimal oSSellAmt, OrgHeader debtor)
		{
			Charge charge = parentJob.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_Desc = desc;

			charge.JR_OH_CostAccount = creditor != null ? creditor.PK : ZGuid.Empty;
			charge.JR_RX_NKCostCurrency = costCurrency != null ? costCurrency.RX_Code : ZString.Empty;
			charge.JR_OSCostAmt = oSCostAmt;

			charge.JR_OH_SellAccount = debtor.PK;
			charge.JR_RX_NKSellCurrency = sellCurrency.RX_Code;
			charge.JR_OSSellAmt = oSSellAmt;
			return charge;
		}

		#endregion

		#region AUD

		RefCurrency fAUD;
		RefCurrency AUD
		{
			get
			{
				if (fAUD == null)
				{
					fAUD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
				}
				return fAUD;
			}
		}

		#endregion

		#region USD

		RefCurrency fUSD;
		RefCurrency USD
		{
			get
			{
				if (fUSD == null)
				{
					fUSD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
				}
				return fUSD;
			}
		}

		#endregion

		#region GBP

		RefCurrency fGBP;
		RefCurrency GBP
		{
			get
			{
				if (fGBP == null)
				{
					fGBP = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "GBP");
				}
				return fGBP;
			}
		}

		#endregion

		#region EUR

		RefCurrency fEUR;
		RefCurrency EUR
		{
			get
			{
				if (fEUR == null)
				{
					fEUR = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "EUR");
				}
				return fEUR;
			}
		}

		#endregion

		#region ABIGAS

		OrgHeader fABIGAS;
		OrgHeader ABIGAS
		{
			get
			{
				if (fABIGAS == null)
				{
					fABIGAS = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");
				}
				return fABIGAS;
			}
		}

		#endregion

		#region AALSHI

		OrgHeader fAALSHI;
		OrgHeader AALSHI
		{
			get
			{
				if (fAALSHI == null)
				{
					fAALSHI = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "AALSHI");
				}
				return fAALSHI;
			}
		}

		#endregion

		#region ZECTRA

		OrgHeader fZECTRA;
		OrgHeader ZECTRA
		{
			get
			{
				if (fZECTRA == null)
				{
					fZECTRA = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ZECTRA");
				}
				return fZECTRA;
			}
		}

		#endregion

		AccChargeCode fMRG100;
		AccChargeCode MRG100
		{
			get { return fMRG100 ?? (fMRG100 = Creator.CreateChargeCode("MRG100", "Margin 100 With GST & WHT", Constants.ChargeType.Margin, 100, GST1, WHT1)); }
		}

		AccInvMsg fInvMsg1;
		AccInvMsg InvMsg1
		{
			get
			{
				if (fInvMsg1 == null)
				{
					fInvMsg1 = Factory.NewWithValidTestData<AccInvMsg>();
					fInvMsg1.A9_Code = "MSG1";
					fInvMsg1.A9_Description = "MSG1 Description";
					fInvMsg1.A9_EnglishMsg = "MSG1 English Msg";
					fInvMsg1.A9_LocalMsg = "MSG1 Local Msg";
					fInvMsg1.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					fInvMsg1.A9_IsActive = true;
				}
				return fInvMsg1;
			}
		}

		AccTaxRate fGST1;
		AccTaxRate GST1
		{
			get
			{
				if (fGST1 == null)
				{
					fGST1 = Creator.GST1;
					fGST1.AT_A9_DefaultVatClass = InvMsg1.PK;
				}

				return fGST1;
			}
		}

		AccTaxRate fGST2;
		AccTaxRate GST2
		{
			get
			{
				if (fGST2 == null)
				{
					fGST2 = Creator.GST2;
				}

				return fGST2;
			}
		}

		AccWithholding WHT1
		{
			get { return Creator.WHT1; }
		}

		protected AccChargeCode fCCODE1;
		protected AccChargeCode CCODE1
		{
			get { return fCCODE1 ?? (fCCODE1 = Creator.CreateChargeCode("ZUB", "Zubin Test", Constants.ChargeType.Margin, 100, GST1, WHT1)); }
		}

		InvoicingLineBase CreateInvoiceLine(InvoicingBase invoice, RefCurrency currency, decimal exchangeRate, decimal aH_OSExTaxAmount)
		{
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_RX_NKTransactionCurrency = currency.RX_Code;
			line.AL_ExchangeRate = exchangeRate;
			line.AL_OSExTaxAmount = aH_OSExTaxAmount;
			return line;
		}

		#endregion
	}
}
