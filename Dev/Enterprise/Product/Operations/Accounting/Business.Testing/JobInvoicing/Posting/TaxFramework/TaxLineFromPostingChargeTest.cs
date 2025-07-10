using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Business.JobInvoicing.Posting.TaxFramework;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class TaxLineFromPostingChargeTest : TestCaseWithFactory
	{
		public void TestConstructorAndProperties()
		{
			AssertExceptionThrown<ArgumentNullException>(() => _ = new TaxLineFromPostingCharge(null, null, null));
			var postingChargeKey = new PostingChargeKey(ZGuid.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, 0);
			AssertExceptionThrown<ArgumentNullException>(() => _ = new TaxLineFromPostingCharge(postingChargeKey, null, null));
			var charge = Factory.New<Charge>();
			charge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge.JR_SellTaxDate = ZDate.Today;
			charge.JR_OSSellExRate = 2M;
			AssertExceptionThrown<ArgumentNullException>(() => _ = new TaxLineFromPostingCharge(postingChargeKey, charge, null));

			ITaxableTransactionLineBase taxLineFromPostingCharge = null;
			AssertNoExceptionThrown(() => taxLineFromPostingCharge = new TaxLineFromPostingCharge(postingChargeKey, charge, new ReadOnlyBusinessObjectFactory()));

			AssertNoExceptionThrown(() => _ = taxLineFromPostingCharge.PK);
			AssertNoExceptionThrown(() => _ = taxLineFromPostingCharge.Factory);
			AssertNoExceptionThrown(() => _ = taxLineFromPostingCharge.Branch);
			AssertNoExceptionThrown(() => _ = taxLineFromPostingCharge.ChargeCode);
			AssertNoExceptionThrown(() => _ = taxLineFromPostingCharge.Currency);
			AssertNoExceptionThrown(() => _ = taxLineFromPostingCharge.TaxDate);
			AssertNoExceptionThrown(() => _ = taxLineFromPostingCharge.JobPK);
			AssertNoExceptionThrown(() => _ = taxLineFromPostingCharge.GetTaxCalculationParameters());
			AssertNoExceptionThrown(() => _ = taxLineFromPostingCharge.BaseOSAmount);
			AssertNoExceptionThrown(() => _ = taxLineFromPostingCharge.LocalAmount);
		}

		[TestDate(2021, 02, 28)]
		public void TestProperties()
		{
			var charge = Factory.New<Charge>();
			charge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge.JR_SellTaxDate = ZDate.Today;
			charge.JR_OSSellExRate = 2M;
			charge.JR_LocalSellAmt = 100M;
			charge.JR_OSSellAmt = 200M;

			var factory = new ReadOnlyBusinessObjectFactory();
			var postingChargeKey = new PostingChargeKey(ZGuid.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, 0);

			var taxLineFromPostingCharge = new TaxLineFromPostingCharge(postingChargeKey, charge, factory);
			var taxLine = (ITaxableTransactionLineBase)taxLineFromPostingCharge;

			AssertEquals("PostingCharge", charge, taxLineFromPostingCharge.PostingCharge);

			var pk = taxLineFromPostingCharge.PK;
			AssertEquals("PK", pk, taxLineFromPostingCharge.PK);

			AssertEquals("Factory", factory, taxLine.Factory);
			AssertEquals("Branch PK", GlbBranch.CurrentBranch.PK, taxLine.Branch.PK);
			AssertEquals("ChargeCode", null, taxLine.ChargeCode);
			AssertEquals("Currency", TestObjectCreator.AUD.RX_Code, taxLine.Currency);
			AssertEquals("TaxDate", ZDate.Today, taxLine.TaxDate);
			AssertEquals("JobPK", ZGuid.Empty, taxLine.JobPK);
			AssertEquals("BaseOSAmount", 0M, taxLine.BaseOSAmount);
			AssertEquals("LocalAmount", 0M, taxLine.LocalAmount);
		}

		public void TestTaxLineBranch()
		{
			var branch1 = TestObjectCreator.CreateBranch("XXX", GlbCompany.CurrentCompany);
			var charge = Factory.New<Charge>();
			charge.JR_GB = ZGuid.Empty;

			var factory = new ReadOnlyBusinessObjectFactory();
			var postingChargeKey = new PostingChargeKey(ZGuid.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, 0);

			ITaxableTransactionLineBase taxLine = new TaxLineFromPostingCharge(postingChargeKey, charge, factory);
			AssertNull(taxLine.Branch);

			charge.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
			charge.JR_GB_SellTaxBranch = branch1.PK;

			taxLine = new TaxLineFromPostingCharge(postingChargeKey, charge, factory);
			AssertEquals(branch1.PK, taxLine.Branch.PK);

			charge.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
			charge.JR_GB_SellTaxBranch = ZGuid.Empty;

			taxLine = new TaxLineFromPostingCharge(postingChargeKey, charge, factory);
			AssertEquals(TestObjectCreator.NonCurrentBranch.PK, taxLine.Branch.PK);
		}

		public void TestTaxLineChargeCode()
		{
			var charge = Factory.New<Charge>();

			var factory = new ReadOnlyBusinessObjectFactory();
			var postingChargeKey = new PostingChargeKey(ZGuid.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, 0);

			ITaxableTransactionLineBase taxLine = new TaxLineFromPostingCharge(postingChargeKey, charge, factory);
			AssertEquals(null, taxLine.ChargeCode);

			charge.JR_AC = TestObjectCreator.CC1.PK;

			taxLine = new TaxLineFromPostingCharge(postingChargeKey, charge, factory);
			AssertEquals(TestObjectCreator.CC1.PK, taxLine.ChargeCode.PK);
		}

		public void TestTaxLineJob()
		{
			var job = Factory.NewJobForTesting<Job>();
			job.JH_JobNum = "S090991";
			Factory.Save();

			var charge = Factory.New<Charge>();

			var factory = new ReadOnlyBusinessObjectFactory();
			var postingChargeKey = new PostingChargeKey(ZGuid.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, 0);

			ITaxableTransactionLineBase taxLine = new TaxLineFromPostingCharge(postingChargeKey, charge, factory);
			AssertEquals(ZGuid.Empty, taxLine.JobPK);

			charge.JR_JH = job.PK;

			taxLine = new TaxLineFromPostingCharge(postingChargeKey, charge, factory);
			AssertEquals(job.PK, taxLine.JobPK);
		}

		public void TestGetTaxCalculationParameters_Consol()
		{
			var consolNumber = "C001001";
			var consol = TestObjectCreator.CreateConsol(consolNum: consolNumber);
			var org = TestObjectCreator.Debtor;
			Factory.Save();

			var branch = GlbBranch.CurrentBranch;

			var postingChargeKey = new PostingChargeKey(org.PK, ZString.Empty, consolNumber, ZGuid.Empty, ZGuid.Empty, 0, branch.PK);

			var charge = Factory.New<Charge>();
			charge.JR_OH_SellAccount = org.PK;

			var factory = new ReadOnlyBusinessObjectFactory();

			ITaxableTransactionLineBase taxLine = new TaxLineFromPostingCharge(postingChargeKey, charge, factory);
			var result = taxLine.GetTaxCalculationParameters();

			AssertEquals(CostSell.Cost, result.CostOrSell);
			AssertEquals(JobInvoicingConsumerTypes.ForwardingConsol.Code, result.JobType);
			AssertEquals(consol.CostSupporter.Direction, result.Direction);
			AssertEquals(consol.TransportMode, result.TransportMode);
			AssertEquals(consol.LoadPort.Code, result.Origin.Code);
			AssertEquals(consol.DischargePort.Code, result.Destination.Code);
			AssertEquals(branch.PK, result.Branch.PK);
		}

		public void TestGetTaxCalculationParameters_Shipment()
		{
			var shipmentNumber = "S001001";
			var shipment = TestObjectCreator.CreateShipment(shipmentNumber, false);
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.JH_JobNum = shipmentNumber;
			var org = TestObjectCreator.Debtor;
			Factory.Save();

			var branch = GlbBranch.CurrentBranch;

			var postingChargeKey = new PostingChargeKey(org.PK, ZString.Empty, shipmentNumber, ZGuid.Empty, ZGuid.Empty, 0, branch.PK);

			var charge = Factory.New<Charge>();
			charge.JR_OH_SellAccount = org.PK;
			charge.JR_JH = job.PK;

			var factory = new ReadOnlyBusinessObjectFactory();

			ITaxableTransactionLineBase taxLine = new TaxLineFromPostingCharge(postingChargeKey, charge, factory);
			var result = taxLine.GetTaxCalculationParameters();

			AssertEquals(JobInvoicingConsumerTypes.Shipment.Code, result.JobType);
			AssertEquals(CostSell.Revenue, result.CostOrSell);
			AssertEquals(org.PK, result.Organisation.PK);
			AssertEquals(branch.PK, result.Branch.PK);
		}

		public void TestGetTaxCalculationParameters_PlaceOfSupply()
		{
			var org = TestObjectCreator.Debtor;
			Factory.Save();
			var branch = GlbBranch.CurrentBranch;
			var codeDescriptionBool = new CodeDescriptionBool() { Code = PlaceOfSupplyTypes.TaxZone.Code, Bool = true };
			var codeDescriptionBoolCollection = new CodeDescriptionBoolCollection() { codeDescriptionBool };

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Canada))
			using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, codeDescriptionBoolCollection))
			using (AccountingMasterFilesRegistry.Instance.UseImportCompanyChargeCodesTaxOverridesforIntercompanyInvoiceImport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var placeOfSupplyCode = "ONTZ";
				var location = PlaceOfSupplyHelper.TryConvertToLocation(GlbCompany.CurrentCompany, placeOfSupplyCode);

				var postingChargeKey = new PostingChargeKey(org.PK, ZString.Empty, "S001001", ZGuid.Empty, ZGuid.Empty, 0, branch.PK, placeOfSupplyCode);

				var charge = Factory.New<Charge>();
				charge.JR_OH_SellAccount = org.PK;
				charge.JR_SellPlaceOfSupply = placeOfSupplyCode;

				var factory = new ReadOnlyBusinessObjectFactory();
				ITaxableTransactionLineBase taxLine = new TaxLineFromPostingCharge(postingChargeKey, charge, factory);
				var result = taxLine.GetTaxCalculationParameters();

				AssertEquals(CostSell.Revenue, result.CostOrSell);
				AssertEquals(org.PK, result.Organisation.PK);
				AssertEquals(branch.PK, result.Branch.PK);
				AssertEquals(location.Code, result.FixedPlaceOfSupply.Code);
			}
		}

		public void TestGetTaxCalculationParameters_SupplyType()
		{
			var postingChargeKey = new PostingChargeKey(ZGuid.Empty, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, 0);

			var expectedSupplyType = SupplyTypeClassificationCodes.LOA;
			var charge = Factory.New<Charge>();
			charge.JR_SellSupplyType = expectedSupplyType;

			var factory = new ReadOnlyBusinessObjectFactory();

			ITaxableTransactionLineBase taxLine = new TaxLineFromPostingCharge(postingChargeKey, charge, factory);
			var result = taxLine.GetTaxCalculationParameters();

			AssertEquals("Supply type should be returned", expectedSupplyType, result.SupplyType);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
