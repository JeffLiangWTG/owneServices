using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using WiseRates.Tools;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(InvoicingBaseConsolCostImporter))]
	public class InvoicingBaseConsolCostImporterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestImportCostsIntoCosting()
		{
			var consol = Factory.New<ForwardingConsol>();
			var dummyConsolCostings = new [] {
				consol.GetApportionments().CostsCollection.TryAddNew()
				,consol.GetApportionments().CostsCollection.TryAddNew()
				,consol.GetApportionments().CostsCollection.TryAddNew()
				,consol.GetApportionments().CostsCollection.TryAddNew()
				,consol.GetApportionments().CostsCollection.TryAddNew()
			};

			var invoice = Factory.New<APInvoice>();
			var originatingCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);

			var mockedConsolCostImporter = new Mock<IConsolCostImporter>();
			mockedConsolCostImporter.Setup(x => x.ImportCostIntoCosting(invoice, dummyConsolCostings[0], originatingCost, false));
			mockedConsolCostImporter.Setup(x => x.ImportCostsToCollection(
				invoice.ConsolCosting.ConsolCosts
				, It.Is<JobConsolCost[]>(x => x.SequenceEqualIgnoringOrder(dummyConsolCostings.Skip(1), null, true))
				, false)
			);

			using (ObjectFactory.Substitute<IConsolCostImporter>(mockedConsolCostImporter.Object))
			{
				var importer = new InvoicingBaseConsolCostImporter(Factory, originatingCost, invoice);
				AssertNoExceptionThrown(() =>
					importer.ImportCostsIntoCosting(dummyConsolCostings.Cast<BusinessObject>().ToArray())
				);
			}

			mockedConsolCostImporter.Verify(
				x => x.ImportCostIntoCosting(It.IsAny<InvoicingBase>(), It.IsAny<JobConsolCost>(), It.IsAny<JobConsolCost>(), It.IsAny<bool>())
				, Times.Exactly(1));
			mockedConsolCostImporter.Verify(
				x => x.ImportCostsToCollection(It.IsAny<APInvoiceConsolCostCollection>(), It.IsAny<IEnumerable<JobConsolCost>>(), It.IsAny<bool>())
				, Times.Exactly(1));
		}

		public void TestCollectionAlwaysContainsCompanySpecificItems()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			JobConsolCost nonCurrentCompanyCost = consol.GetApportionments().CostsCollection.TryAddNew();
			nonCurrentCompanyCost.E6_GC = creator.NonCurrentCompanyBranch.Company.PK;
			nonCurrentCompanyCost.E6_AC_ChargeCode = creator.CC1.PK;

			JobConsolCost currentCompanyCost = consol.GetApportionments().CostsCollection.TryAddNew();
			currentCompanyCost.E6_GC = GlbCompany.CurrentCompany.PK;
			currentCompanyCost.E6_AC_ChargeCode = creator.CC1.PK;

			Factory.Save();

			APInvoice invoice = Factory.New<APInvoice>();
			JobConsolCost originatingCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			originatingCost.E6_AC_ChargeCode = ZGuid.Invalid;

			BusinessObjectFactory importerFactory = new BusinessObjectFactory();

			InvoicingBaseConsolCostImporter importer = new InvoicingBaseConsolCostImporter(importerFactory, originatingCost, invoice);
			AssertEquals("Should only be one item in collection", 1, importer.CostsCollection.Count);
			AssertEquals("Should show correct item", currentCompanyCost.PK, importer.CostsCollection[0].PK);
		}

		public void TestImportCostsIntoCostingDoesNotOverrideSellSide_LocalCurrency()
		{
			AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			var creator = new TestObjectCreator(Factory);
			creator.Agent.CompanyData.OB_RX_NKARDDefltCurrency = creator.USD.RX_Code;
			creator.CreateExchangeRate(creator.USD, 0.72M);

			var consol = creator.CreateConsol();
			var shipment = creator.CreateShipment("S00001", consol);
			var job = creator.CreateJob(shipment, creator.LocalClient, 1.0M, creator.Agent, 1.0M);

			var consolCost = creator.CreateConsolCost(consol, creator.FRT, creator.Creditor1, 200M, ZBool.True);
			Factory.Save();

			job.Charges[0].JR_OH_SellAccount = creator.Debtor.PK;
			job.Charges[0].JR_RX_NKSellCurrency = Env.CurrentCompany.LocalCurrency.Code;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newObjectCreator = new TestObjectCreator(newFactory);
			var apInvoice = newObjectCreator.CreateInvoice(typeof(APInvoice), creator.AUD, 1M, creator.Creditor1);
			var invoiceConsolCost = newObjectCreator.CreateConsolCost(apInvoice, consol, creator.FRT, 200M, creator.Creditor1);
			AssertEquals("Default Sell Currency", creator.USD.RX_Code, invoiceConsolCost.ApportionmentCharges[0].JR_RX_NKSellCurrency);
			AssertEquals("Default Sell Exchange Rate", 1M, invoiceConsolCost.ApportionmentCharges[0].JR_OSSellExRate);
			AssertEquals("Default Debtor", creator.Agent.PK, invoiceConsolCost.ApportionmentCharges[0].JR_OH_SellAccount);
		}

		public void TestImportCostsIntoCostingDoesNotOverrideSellSide_ForeignCurrency()
		{
			AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			var creator = new TestObjectCreator(Factory);
			creator.Agent.CompanyData.OB_RX_NKARDDefltCurrency = creator.USD.RX_Code;
			creator.Debtor.CompanyData.OB_RX_NKARDDefltCurrency = creator.EUR.RX_Code;
			creator.CreateExchangeRate(creator.USD, 0.72M);
			creator.CreateExchangeRate(creator.EUR, 0.60M);

			var consol = creator.CreateConsol();
			var shipment = creator.CreateShipment("S00001", consol);
			var job = creator.CreateJob(shipment, creator.LocalClient, 1.0M, creator.Agent, 1.0M);

			var consolCost = creator.CreateConsolCost(consol, creator.FRT, creator.Creditor1, 200M, ZBool.True);
			Factory.Save();

			job.Charges[0].JR_OH_SellAccount = creator.Debtor.PK;
			job.Charges[0].JR_RX_NKSellCurrency = creator.EUR.Code;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newObjectCreator = new TestObjectCreator(newFactory);
			var apInvoice = newObjectCreator.CreateInvoice(typeof(APInvoice), creator.AUD, 1M, creator.Creditor1);
			var invoiceConsolCost = newObjectCreator.CreateConsolCost(apInvoice, consol, creator.FRT, 200M, creator.Creditor1);
			AssertEquals("Default Sell Currency", creator.USD.RX_Code, invoiceConsolCost.ApportionmentCharges[0].JR_RX_NKSellCurrency);
			AssertEquals("Default Sell Exchange Rate", 1M, invoiceConsolCost.ApportionmentCharges[0].JR_OSSellExRate);
			AssertEquals("Default Debtor", creator.Agent.PK, invoiceConsolCost.ApportionmentCharges[0].JR_OH_SellAccount);
		}

		public void TestImportForeignCurrencyConsolCostWithRoundingErorsAdjustment()
		{
			ForwardingConsol consol = TestObjectCreator.CreateConsol("AUD", "LAX", "C0001");
			ForwardingShipment shipment1 = TestObjectCreator.CreateShipment("S0001", consol);
			ForwardingShipment shipment2 = TestObjectCreator.CreateShipment("S0002", consol);

			JobConsolCost consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, null);
			consolCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			consolCost.E6_RX_NKCurrency = "USD";
			consolCost.E6_OSCostAmount = 100M;
			consolCost.E6_ExchangeRate = 0.82M;
			consolCost.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.Shipment;

			Factory.Save();

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_RX_NKTransactionCurrency = "AUD";
			JobConsolCost invoiceConsolCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			invoiceConsolCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;

			AssertEquals("Should be 2 Charges", 2, invoiceConsolCost.ApportionmentCharges.Count);
			Assert("Imported Consol Cost hasn't errors", !invoiceConsolCost.HasRowErrors);
			AssertEquals("UnApportionedAmount is zero", 0M, invoiceConsolCost.UnApportionedAmount);
		}

		public void TestCostFilterDoesNotCreateLongExclusionCondition()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();

			APInvoice invoice = Factory.New<APInvoice>();
			JobConsolCost originatingCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			originatingCost.E6_AC_ChargeCode = creator.CC1.PK;

			JobConsolCost cost1 = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			cost1.E6_GC = GlbCompany.CurrentCompany.PK;
			cost1.E6_AC_ChargeCode = creator.CC2.PK;
			cost1.RelatedConsolCostPK = cost1.PK;

			JobConsolCost cost2 = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol2);
			cost2.E6_GC = GlbCompany.CurrentCompany.PK;
			cost2.E6_AC_ChargeCode = creator.CC1.PK;
			cost2.RelatedConsolCostPK = cost2.PK;

			JobConsolCost cost3 = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			cost3.E6_GC = GlbCompany.CurrentCompany.PK;
			cost3.E6_AC_ChargeCode = creator.CC1.PK;
			cost3.RelatedConsolCostPK = cost3.PK;

			BusinessObjectFactory importerFactory = new BusinessObjectFactory();

			InvoicingBaseConsolCostImporter importer = new InvoicingBaseConsolCostImporter(importerFactory, originatingCost, invoice);
			string filter = importer.CostsFilter_ForTestOnly.GetAsWhereClause(true);
			AssertNotContains("Should not be in exclusion list - different ChargeCode", cost1.PK.ToString(), filter);
			AssertNotContains("Should not be in exclusion list - different Consol", cost2.PK.ToString(), filter);
			AssertContains("Should be in exclusions", cost3.PK.ToString(), filter);
		}

		public void TestCostFilterExcludesCostsThatAreAlreadyImportedToAnIncompleteInvoice()
		{
			var creator = new TestObjectCreator(Factory);

			var consol = creator.CreateConsol();
			var shipment1 = creator.CreateShipment("S0001", consol);
			var shipment2 = creator.CreateShipment("S0002", consol);
			_ = creator.CreateJob(shipment1);
			_ = creator.CreateJob(shipment2);
			var cost1 = creator.CreateConsolCost(consol, creator.CC1);
			var cost2 = creator.CreateConsolCost(consol, creator.CC2);
			Factory.Save();

			var invoice = Factory.New<APInvoice>();
			invoice.AH_GB_TaxBranch = GlbBranch.CurrentBranch.PK;
			var originatingCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			originatingCost.E6_GB_CostTaxBranch = GlbBranch.CurrentBranch.PK;

			var importerFactory = new BusinessObjectFactory();
			var importer = new InvoicingBaseConsolCostImporter(importerFactory, originatingCost, invoice);
			AssertEquals("Should two item in collection", 2, importer.CostsCollection.Count);
			AssertCollectionContains("Should contain cost1", cost1.PK, importer.CostsCollection.Select(x => x.PK));
			AssertCollectionContains("Should contain cost2", cost2.PK, importer.CostsCollection.Select(x => x.PK));

			var attribFactory = new BusinessObjectFactory();
			var dummyExistingInvoicePK = new ZGuid(Guid.NewGuid());
			var relaodedCost2 = attribFactory.Load<JobConsolCost>(cost2.PK);
			relaodedCost2.Attributes.Add(JobChargeAttribTypeList.Codes.LinkedToIncompleteInvoice, dummyExistingInvoicePK.ToString());
			attribFactory.Save();

			importerFactory = new BusinessObjectFactory();
			importer = new InvoicingBaseConsolCostImporter(importerFactory, originatingCost, invoice);
			AssertEquals("Should one item in collection", 1, importer.CostsCollection.Count);
			AssertCollectionContains("Should contain cost1", cost1.PK, importer.CostsCollection.Select(x => x.PK));
		}

		public void TestCostsCollectionIncludeDifferentTaxBranchFromInvoice()
		{
			var creator = new TestObjectCreator(Factory);

			var consol = Factory.New<ForwardingConsol>();
			var cost = consol.GetApportionments().CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = creator.CC1.PK;
			cost.E6_GB_CostTaxBranch = GlbBranch.CurrentBranch.PK;

			var cost1 = consol.GetApportionments().CostsCollection.TryAddNew();
			cost1.E6_AC_ChargeCode = creator.CC1.PK;
			cost1.E6_GB_CostTaxBranch = creator.NonCurrentBranch.PK;

			Factory.Save();

			var invoice = Factory.New<APInvoice>();
			invoice.AH_GB_TaxBranch = GlbBranch.CurrentBranch.PK;
			var originatingCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			originatingCost.E6_GB_CostTaxBranch = GlbBranch.CurrentBranch.PK;

			var importerFactory = new BusinessObjectFactory();

			var importer = new InvoicingBaseConsolCostImporter(importerFactory, originatingCost, invoice);
			AssertEquals("Should two item in collection", 2, importer.CostsCollection.Count);
			AssertCollectionContains("Should contain cost", cost.PK, importer.CostsCollection.Select(x => x.PK));
			AssertCollectionContains("Should contain cost1", cost1.PK, importer.CostsCollection.Select(x => x.PK));
		}

		void TestCostCollectionExcludesPostedJobRevenueJournal(bool excluded)
		{
			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			if (excluded)
			{
				AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(companyPK);
			}
			else
			{
				AutoJRJRegistryStatusHelper.SetAutoJRJDisabled_ForTestOnly(companyPK);
			}

			var consol = TestObjectCreator.CreateConsol("USLAX", "AUMEL", "TSTCNSL");
			var shipment = TestObjectCreator.CreateShipment("S00010001", consol.JK_RL_NKLoadPort, consol.JK_RL_NKDischargePort);
			consol.Shipments.Add(shipment);
			var job = TestObjectCreator.CreateJob(shipment);
			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.AALSHI, 100m, false, ZArchitecture.Core.AllocationMethod.ChargeableUnits);
			cost.E6_OH_Creditor = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();

			AssertEquals("IsPosted", false, cost.IsPosted);
			AssertEquals("One Charge", 1, job.Charges.Count);

			var charge = job.Charges[0];
			charge.JR_JH_InternalJob = job.PK;
			charge.JR_GB_InternalBranch = TestObjectCreator.NonCurrentBranch.PK;
			charge.JR_GE_InternalDept = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			AssertEquals("IsPosted", excluded, cost.IsPosted);

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			var importer = new InvoicingBaseConsolCostImporter(Factory, cost, invoice);

			AssertEquals("CostsCollection", excluded ? 0 : 1, importer.CostsCollection.Count);
		}

		public void TestCostCollectionExcludesPostedJobRevenueJournal_Excluded()
		{
			TestCostCollectionExcludesPostedJobRevenueJournal(true);
		}

		public void TestCostCollectionExcludesPostedJobRevenueJournal_Included()
		{
			TestCostCollectionExcludesPostedJobRevenueJournal(false);
		}

		public void TestImportConsolCostWithRelatedApportionChargeFromDB()
		{
			var consol = Factory.New<ForwardingConsol>();

			var consolCost = consol.GetApportionments().CostsCollection.TryAddNew();
			consolCost.E6_GC = GlbCompany.CurrentCompany.PK;
			consolCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			consolCost.E6_RX_NKCurrency = TestObjectCreator.AUD.RX_Code;
			consolCost.E6_ExchangeRate = 1m;
			consolCost.E6_ApportionmentMethod = "SHP";
			consolCost.E6_OSCostAmount = 100M;
			consolCost.E6_LocalCostAmount = 100M;

			ApportionSplitCharge charge = consolCost.ApportionmentCharges.AddNew();
			charge.JR_OSCostAmt = 100M;
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_JH = TestObjectCreator.Job1.PK;

			Factory.Save();

			APInvoice invoice = Factory.New<APInvoice>();
			JobConsolCost originatingCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			originatingCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			originatingCost.E6_OSCostAmount = 100m;
			originatingCost.E6_ApportionmentMethod = "SHP";
			invoice.ImportAllApportionmentsFromCosting();

			AssertEquals("Should have 1 charges imported", 1, invoice.ConsolCosting.ConsolCosts[0].ApportionmentCharges.Count);
			AssertNotNull("Apportion charge should have RelatedApportionChargeFromDB", invoice.ConsolCosting.ConsolCosts[0].ApportionmentCharges[0].RelatedApportionChargeFromDB);
			AssertEquals("RelatedApportionChargeFromDB should equal to real charge", consolCost.ApportionmentCharges[0].PK, invoice.ConsolCosting.ConsolCosts[0].ApportionmentCharges[0].RelatedApportionChargeFromDB.PK);
		}

		TestObjectCreator TestObjectCreator
		{
			get { return TestObjectCreator_innerValue ?? (TestObjectCreator_innerValue = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator TestObjectCreator_innerValue;

		protected override BusinessObject GetNewBusinessObject()
		{
			APInvoice invoice = Factory.New<APInvoice>();
			JobConsolCost originatingCost = invoice.ConsolCosting.ConsolCosts.AddNew();

			return new InvoicingBaseConsolCostImporter(Factory, originatingCost, invoice);
		}
	}
}
