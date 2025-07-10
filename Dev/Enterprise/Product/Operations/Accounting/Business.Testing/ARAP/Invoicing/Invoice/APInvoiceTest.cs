using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using VarianceSigns = Enterprise.Accounting.Registry.Business.CostVarianceApprovalAuthorisationRequirement.VarianceSigns;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APInvoice))]
	public class APInvoiceTest : InvoiceTest
	{
		public void TestBusinessObjectsWithRelatedEvents()
		{
			var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			var invoice = Factory.NewWithValidTestData<APInvoice>();

			AssertEquals(0, invoice.BusinessObjectsWithRelatedEvents.Length);

			draftInvoice.AIH_AH_PostedTransactionHeader = invoice.PK;
			AssertEquals(1, invoice.BusinessObjectsWithRelatedEvents.Length);
			AssertCollectionContains(draftInvoice, invoice.BusinessObjectsWithRelatedEvents);
		}

		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<APInvoice>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		public void TestReceiptPaymentAH_AB()
		{
			var cashAccount = TestObjectCreator.CreateBankAccount("TST", "Test Cash Account", TestObjectCreator.AUD, TestObjectCreator.GLHeader1);
			cashAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;

			var invoice = Factory.New<APInvoice>();
			AssertNotEquals(ReceiptTypes.Cash, invoice.ReceiptPaymentAH_ReceiptType);
			invoice.ReceiptPaymentAH_AB = cashAccount.PK;
			AssertEquals("Setting a cash account should set receipt type to CSH", ReceiptTypes.Cash, invoice.ReceiptPaymentAH_ReceiptType);
		}

		public void TestRoundingErrorIsNotReported()
		{
			var creator = new TestObjectCreator(Factory);

			var shipment = creator.CreateShipment("S0001");
			var job = creator.CreateJob(shipment);
			var charge = creator.CreateCharge(job, creator.FRT, "Desc", creator.AUD, 111.11m, creator.Creditor1, creator.AUD, 1000.11m, creator.Debtor);

			Factory.Save();

			var invoice = creator.CreateInvoice(typeof(APInvoice), creator.AUD, 1m) as APInvoice;
			var importer = new InvoicingBaseBulkChargeImporter(invoice);

			importer.LoadJobsCollection();
			importer.Import();

			AssertEquals(1, invoice.Lines.Count);

			invoice.AH_RX_NKTransactionCurrency = "JPY";
			invoice.AH_ExchangeRate = 1.5m;

			AssertEquals(2, charge.JR_OSCostAmt.DecimalPlaces);
			AssertEquals(0, invoice.Lines[0].AL_OSExTaxAmount.DecimalPlaces);

			invoice.SubmittedFromInvoicingForm = true;
			AssertNoExceptionThrown(Factory.Save);

			AssertEquals(0, charge.JR_OSCostAmt.DecimalPlaces);
			AssertEquals(0, invoice.Lines[0].AL_OSExTaxAmount.DecimalPlaces);
		}

		public void TestAH_ChequeOrReference_ReadOnly()
		{
			var factory = new BusinessObjectFactory();
			var apInvoice = factory.NewWithValidTestData<APInvoice>();
			AssertEquals(false, apInvoice.AH_ChequeOrReferenceInfo.ReadOnly);

			factory.SetContext(BusinessContext.PayableOrder);
			AssertEquals(true, apInvoice.AH_ChequeOrReferenceInfo.ReadOnly);
		}

		public void TestAH_TransactionNum_ReadOnly()
		{
			var factory = new BusinessObjectFactory();
			var apInvoice = factory.NewWithValidTestData<APInvoice>();
			AssertEquals(false, apInvoice.AH_TransactionNumInfo.ReadOnly);

			factory.SetContext(BusinessContext.PayableOrder);
			AssertEquals(true, apInvoice.AH_TransactionNumInfo.ReadOnly);
		}

		[ExpectNoExceptions("No timeout or internal Error should be returned from the SQL server")]
		public void TestAPLineLoadingQueryWorksForALargeNumberOfLines()
		{
			var keysToGetFromDB = new HashSet<CostVarianceApprovalHelper.CostVarianceKey>();
			GlbBranch sYD = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "SYD");
			GlbDepartment fEA = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FEA");
			AccChargeCode cAF = TestObjectCreator.CC1;
			ZGuid[] jobPks = new ZGuid[15002];

			AssertNotNull(sYD);
			AssertNotNull(fEA);

			Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001001"));
			Charge charge = TestObjectCreator.CreateCharge(job, cAF, "",
				TestObjectCreator.AUD, 100M, TestObjectCreator.Creditor1,
				TestObjectCreator.AUD, 0M, null);
			charge.JR_GB = sYD.PK;
			charge.JR_GE = fEA.PK;

			Job job2 = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001002"));
			Charge charge2 = TestObjectCreator.CreateCharge(job2, cAF, "",
				TestObjectCreator.AUD, 100M, TestObjectCreator.Creditor1,
				TestObjectCreator.AUD, 0M, null);
			charge2.JR_GB = sYD.PK;
			charge2.JR_GE = fEA.PK;

			Factory.Save();

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;
			var line1 = TestObjectCreator.CreateInvoiceLine(invoice, job, cAF, 100M) as APInvoiceLine;
			line1.AL_GB = sYD.PK;
			line1.AL_GE = fEA.PK;
			var line2 = TestObjectCreator.CreateInvoiceLine(invoice, job2, cAF, 100M) as APInvoiceLine;
			line2.AL_GB = sYD.PK;
			line2.AL_GE = fEA.PK;
			keysToGetFromDB.Add(new CostVarianceApprovalHelper.CostVarianceKey(line1));
			keysToGetFromDB.Add(new CostVarianceApprovalHelper.CostVarianceKey(line2));

			for (int i = 2; i < jobPks.Length; i++)
			{
				var apportionCharge = Factory.New<ApportionSplitCharge>();
				apportionCharge.JR_GB = sYD.PK;
				keysToGetFromDB.Add(new CostVarianceApprovalHelper.CostVarianceKey(apportionCharge));
			}

			invoice.CalculateAuthorisationByLinesDBHitCount_ForTestOnly = 0;
			invoice.RefreshLineUnpostedCosts_ForTestOnly(keysToGetFromDB);

			AssertEquals(1, invoice.CalculateAuthorisationByLinesDBHitCount_ForTestOnly);
		}

		public void TestTotalPositiveCostVarianceAuthorisationRequired()
		{
			AssertTotalCostVarianceAuthorisationRequired(VarianceSigns.Plus);
		}

		public void TestTotalNegativeCostVarianceAuthorisationRequired()
		{
			AssertTotalCostVarianceAuthorisationRequired(VarianceSigns.Minus);
		}

		void AssertTotalCostVarianceAuthorisationRequired(ZString varianceSign)
		{
			GlbBranch sYD = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "SYD");
			GlbDepartment fEA = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FEA");
			AccChargeCode cAF = TestObjectCreator.CC1;

			AssertNotNull(sYD);
			AssertNotNull(fEA);

			var chargeAmount = varianceSign == VarianceSigns.Plus ? 100m : 300m;

			Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001001"));
			Charge charge = TestObjectCreator.CreateCharge(job, cAF, "",
				TestObjectCreator.AUD, chargeAmount, TestObjectCreator.Creditor1,
				TestObjectCreator.AUD, 0M, null);
			charge.JR_GB = sYD.PK;
			charge.JR_GE = fEA.PK;

			Job job2 = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001002"));
			Charge charge2 = TestObjectCreator.CreateCharge(job2, cAF, "",
				TestObjectCreator.AUD, chargeAmount, TestObjectCreator.Creditor1,
				TestObjectCreator.AUD, 0M, null);
			charge2.JR_GB = sYD.PK;
			charge2.JR_GE = fEA.PK;

			Factory.Save();

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;
			APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();
			line.AL_JH = job.PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			line.AL_AC = cAF.PK;
			line.AL_GB = sYD.PK;
			line.AL_GE = fEA.PK;
			line.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			line.AL_ExchangeRate = 1M;

			APInvoiceLine line2 = (APInvoiceLine)invoice.Lines.AddNew();
			line2.AL_JH = job2.PK;
			line2.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			line2.AL_AC = cAF.PK;
			line2.AL_GB = sYD.PK;
			line2.AL_GE = fEA.PK;
			line2.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			line2.AL_ExchangeRate = 1M;

			CostVarianceApproval valuesForTest = new CostVarianceApproval();
			valuesForTest.VarianceCalculationStyle = Core.Constants.CostVarianceCalculationStyle.LocalExTaxAmount;
			valuesForTest.VarianceComparisonOption = Core.Constants.CostVarianceComparisonOption.Job;
			CostVarianceApprovalAuthorisationRequirement upTo1 = valuesForTest.AuthorisationRequirements.AddNew();
			upTo1.VarianceSign = varianceSign;
			upTo1.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			upTo1.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			upTo1.Amount = 100M;
			upTo1.MonitorTotalInvoiceVariance = true;
			upTo1.TotalInvoiceVarianceAmount = 150M;

			CostVarianceApprovalAuthorisationRequirement upTo2 = valuesForTest.AuthorisationRequirements.AddNew();
			upTo2.VarianceSign = varianceSign;
			upTo2.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			upTo2.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			upTo2.Amount = 200M;
			upTo2.MonitorTotalInvoiceVariance = true;
			upTo2.TotalInvoiceVarianceAmount = 300M;

			CostVarianceApprovalAuthorisationRequirement above = valuesForTest.AuthorisationRequirements.AddNew();
			above.VarianceSign = varianceSign;
			above.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			above.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			above.Amount = upTo2.Amount;
			above.MonitorTotalInvoiceVariance = true;
			above.TotalInvoiceVarianceAmount = 300M;

			Env.Security.CostVarianceApprovalLevel1.IsAllowed = true;
			Env.Security.CostVarianceApprovalLevel2.IsAllowed = false;

			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertTotalCostVarianceAuthorisationRequiredResult(line, line2, invoice, varianceSign,
			APInvoice.CostVarianceAuthorisationRequiredType.NoAuthorisationRequired, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired,
			APInvoice.CostVarianceAuthorisationRequiredType.HasAuthorisationRights, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly,
			APInvoice.CostVarianceAuthorisationRequiredType.AuthorisationRequired, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly);

			upTo2.MonitorTotalInvoiceVariance = false;
			above.MonitorTotalInvoiceVariance = false;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertTotalCostVarianceAuthorisationRequiredResult(line, line2, invoice, varianceSign,
			APInvoice.CostVarianceAuthorisationRequiredType.NoAuthorisationRequired, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired,
			APInvoice.CostVarianceAuthorisationRequiredType.HasAuthorisationRights, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly,
			APInvoice.CostVarianceAuthorisationRequiredType.HasAuthorisationRights, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly);

			upTo1.MonitorTotalInvoiceVariance = false;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertTotalCostVarianceAuthorisationRequiredResult(line, line2, invoice, varianceSign,
			APInvoice.CostVarianceAuthorisationRequiredType.NoAuthorisationRequired, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired,
			APInvoice.CostVarianceAuthorisationRequiredType.NoAuthorisationRequired, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired,
			APInvoice.CostVarianceAuthorisationRequiredType.NoAuthorisationRequired, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly);

			valuesForTest.AuthorisationRequirements.Remove(upTo1);
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertTotalCostVarianceAuthorisationRequiredResult(line, line2, invoice, varianceSign,
			APInvoice.CostVarianceAuthorisationRequiredType.NoAuthorisationRequired, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly,
			APInvoice.CostVarianceAuthorisationRequiredType.NoAuthorisationRequired, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly,
			APInvoice.CostVarianceAuthorisationRequiredType.NoAuthorisationRequired, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly);

			upTo2.MonitorTotalInvoiceVariance = true;
			above.MonitorTotalInvoiceVariance = true;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertTotalCostVarianceAuthorisationRequiredResult(line, line2, invoice, varianceSign,
			APInvoice.CostVarianceAuthorisationRequiredType.HasAuthorisationRights, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly,
			APInvoice.CostVarianceAuthorisationRequiredType.HasAuthorisationRights, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly,
			APInvoice.CostVarianceAuthorisationRequiredType.AuthorisationRequired, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly);
		}

		void AssertTotalCostVarianceAuthorisationRequiredResult(APInvoiceLine line, APInvoiceLine line2, APInvoice invoice, ZString varianceSign,
			APInvoice.CostVarianceAuthorisationRequiredType costVarianceAuthorisationRequiredType1, string authorisationLevel1,
			APInvoice.CostVarianceAuthorisationRequiredType costVarianceAuthorisationRequiredType2, string authorisationLevel2,
			APInvoice.CostVarianceAuthorisationRequiredType costVarianceAuthorisationRequiredType3, string authorisationLevel3)
		{
			invoice.ClearCachedCostVarianceApproval_ForTestOnly();

			int expectedDBHits = 0;
			if (invoice.CalculateAuthorisationByLinesDBHitCount_ForTestOnly == 0)
			{
				expectedDBHits++;
			}

			invoice.CalculateAuthorisationByLinesDBHitCount_ForTestOnly = 0;
			var lineAmount = varianceSign == VarianceSigns.Plus ? 150m : 250m;
			line.AL_OSExTaxAmount = lineAmount;
			line2.AL_OSExTaxAmount = lineAmount;
			AssertEquals(costVarianceAuthorisationRequiredType1, invoice.TotalCostVarianceAuthorisationRequired);
			AssertEquals("One db hit for fist call and then only cached data should be used.", expectedDBHits, invoice.CalculateAuthorisationByLinesDBHitCount_ForTestOnly);
			AssertEquals(authorisationLevel1, invoice.AuthorisationLevel);
			AssertEquals("Latest data should be received for AuthorisationLevel, so +1 db hit.", ++expectedDBHits, invoice.CalculateAuthorisationByLinesDBHitCount_ForTestOnly);

			lineAmount = varianceSign == VarianceSigns.Plus ? 190m : 210m;
			line.AL_OSExTaxAmount = lineAmount;
			line2.AL_OSExTaxAmount = lineAmount;
			AssertEquals(costVarianceAuthorisationRequiredType2, invoice.TotalCostVarianceAuthorisationRequired);
			AssertEquals("Data from cache should be used without any db hits", expectedDBHits, invoice.CalculateAuthorisationByLinesDBHitCount_ForTestOnly);
			AssertEquals(authorisationLevel2, invoice.AuthorisationLevel);
			AssertEquals("Latest data should be received for AuthorisationLevel, so +1 db hit.", ++expectedDBHits, invoice.CalculateAuthorisationByLinesDBHitCount_ForTestOnly);

			lineAmount = varianceSign == VarianceSigns.Plus ? 290m : 110m;
			line.AL_OSExTaxAmount = lineAmount;
			line2.AL_OSExTaxAmount = lineAmount;
			AssertEquals(costVarianceAuthorisationRequiredType3, invoice.TotalCostVarianceAuthorisationRequired);
			AssertEquals("Data from cache should be used without any db hits", expectedDBHits, invoice.CalculateAuthorisationByLinesDBHitCount_ForTestOnly);
			AssertEquals(authorisationLevel3, invoice.AuthorisationLevel);
			AssertEquals("Latest data should be received for AuthorisationLevel, so +1 db hit.", ++expectedDBHits, invoice.CalculateAuthorisationByLinesDBHitCount_ForTestOnly);
		}

		public void TestTotalMixedCostVarianceAuthorisationRequired()
		{
			GlbBranch sYD = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "SYD");
			GlbDepartment fEA = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FEA");
			AccChargeCode cAF = TestObjectCreator.CC1;

			AssertNotNull(sYD);
			AssertNotNull(fEA);

			Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001001"));
			Charge charge = TestObjectCreator.CreateCharge(job, cAF, "",
				TestObjectCreator.AUD, 100M, TestObjectCreator.Creditor1,
				TestObjectCreator.AUD, 0M, null);
			charge.JR_GB = sYD.PK;
			charge.JR_GE = fEA.PK;

			Job job2 = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001002"));
			Charge charge2 = TestObjectCreator.CreateCharge(job2, cAF, "",
				TestObjectCreator.AUD, 300M, TestObjectCreator.Creditor1,
				TestObjectCreator.AUD, 0M, null);
			charge2.JR_GB = sYD.PK;
			charge2.JR_GE = fEA.PK;

			Factory.Save();

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;
			APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();
			line.AL_JH = job.PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			line.AL_AC = cAF.PK;
			line.AL_GB = sYD.PK;
			line.AL_GE = fEA.PK;
			line.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			line.AL_ExchangeRate = 1M;

			APInvoiceLine line2 = (APInvoiceLine)invoice.Lines.AddNew();
			line2.AL_JH = job2.PK;
			line2.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			line2.AL_AC = cAF.PK;
			line2.AL_GB = sYD.PK;
			line2.AL_GE = fEA.PK;
			line2.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			line2.AL_ExchangeRate = 1M;

			var valuesForTest = new CostVarianceApproval();
			valuesForTest.VarianceCalculationStyle = Core.Constants.CostVarianceCalculationStyle.LocalExTaxAmount;
			valuesForTest.VarianceComparisonOption = Core.Constants.CostVarianceComparisonOption.Job;
			var upTo1 = valuesForTest.AuthorisationRequirements.AddNew();
			upTo1.VarianceSign = VarianceSigns.Plus;
			upTo1.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			upTo1.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			upTo1.Amount = 50M;
			upTo1.MonitorTotalInvoiceVariance = true;
			upTo1.TotalInvoiceVarianceAmount = 100M;

			var upTo2 = valuesForTest.AuthorisationRequirements.AddNew();
			upTo2.VarianceSign = VarianceSigns.Plus;
			upTo2.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			upTo2.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			upTo2.Amount = 100M;
			upTo2.MonitorTotalInvoiceVariance = true;
			upTo2.TotalInvoiceVarianceAmount = 150M;

			var above = valuesForTest.AuthorisationRequirements.AddNew();
			above.VarianceSign = VarianceSigns.Plus;
			above.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			above.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			above.Amount = upTo2.Amount;
			above.MonitorTotalInvoiceVariance = true;
			above.TotalInvoiceVarianceAmount = upTo2.TotalInvoiceVarianceAmount;

			var upToNegative = valuesForTest.AuthorisationRequirements.AddNew();
			upToNegative.VarianceSign = VarianceSigns.Minus;
			upToNegative.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			upToNegative.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			upToNegative.Amount = 50M;
			upToNegative.MonitorTotalInvoiceVariance = true;
			upToNegative.TotalInvoiceVarianceAmount = 100M;

			var aboveNegative = valuesForTest.AuthorisationRequirements.AddNew();
			aboveNegative.VarianceSign = VarianceSigns.Minus;
			aboveNegative.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			aboveNegative.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			aboveNegative.Amount = upToNegative.Amount;
			aboveNegative.MonitorTotalInvoiceVariance = true;
			aboveNegative.TotalInvoiceVarianceAmount = upToNegative.TotalInvoiceVarianceAmount;

			Env.Security.CostVarianceApprovalLevel1.IsAllowed = true;
			Env.Security.CostVarianceApprovalLevel2.IsAllowed = false;

			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertTotalCostVarianceAuthorisationRequiredResult(line, line2, invoice, VarianceSigns.Plus,
			APInvoice.CostVarianceAuthorisationRequiredType.HasAuthorisationRights, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly,
			APInvoice.CostVarianceAuthorisationRequiredType.HasAuthorisationRights, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly,
			APInvoice.CostVarianceAuthorisationRequiredType.AuthorisationRequired, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly);

			AssertTotalCostVarianceAuthorisationRequiredResult(line, line2, invoice, VarianceSigns.Minus,
			APInvoice.CostVarianceAuthorisationRequiredType.HasAuthorisationRights, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly,
			APInvoice.CostVarianceAuthorisationRequiredType.HasAuthorisationRights, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly,
			APInvoice.CostVarianceAuthorisationRequiredType.HasAuthorisationRights, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly);

			upTo2.MonitorTotalInvoiceVariance = false;
			above.MonitorTotalInvoiceVariance = false;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertTotalCostVarianceAuthorisationRequiredResult(line, line2, invoice, VarianceSigns.Plus,
			APInvoice.CostVarianceAuthorisationRequiredType.HasAuthorisationRights, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly,
			APInvoice.CostVarianceAuthorisationRequiredType.HasAuthorisationRights, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly,
			APInvoice.CostVarianceAuthorisationRequiredType.HasAuthorisationRights, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly);

			AssertTotalCostVarianceAuthorisationRequiredResult(line, line2, invoice, VarianceSigns.Minus,
			APInvoice.CostVarianceAuthorisationRequiredType.HasAuthorisationRights, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly,
			APInvoice.CostVarianceAuthorisationRequiredType.HasAuthorisationRights, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly,
			APInvoice.CostVarianceAuthorisationRequiredType.HasAuthorisationRights, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly);

			upTo1.MonitorTotalInvoiceVariance = false;
			upToNegative.MonitorTotalInvoiceVariance = false;
			aboveNegative.MonitorTotalInvoiceVariance = false;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertTotalCostVarianceAuthorisationRequiredResult(line, line2, invoice, VarianceSigns.Plus,
			APInvoice.CostVarianceAuthorisationRequiredType.NoAuthorisationRequired, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly,
			APInvoice.CostVarianceAuthorisationRequiredType.NoAuthorisationRequired, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly,
			APInvoice.CostVarianceAuthorisationRequiredType.NoAuthorisationRequired, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly);

			AssertTotalCostVarianceAuthorisationRequiredResult(line, line2, invoice, VarianceSigns.Minus,
			APInvoice.CostVarianceAuthorisationRequiredType.NoAuthorisationRequired, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly,
			APInvoice.CostVarianceAuthorisationRequiredType.NoAuthorisationRequired, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly,
			APInvoice.CostVarianceAuthorisationRequiredType.NoAuthorisationRequired, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly);

			valuesForTest.AuthorisationRequirements.Remove(upTo1);
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertTotalCostVarianceAuthorisationRequiredResult(line, line2, invoice, VarianceSigns.Plus,
			APInvoice.CostVarianceAuthorisationRequiredType.NoAuthorisationRequired, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly,
			APInvoice.CostVarianceAuthorisationRequiredType.NoAuthorisationRequired, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly,
			APInvoice.CostVarianceAuthorisationRequiredType.NoAuthorisationRequired, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly);

			AssertTotalCostVarianceAuthorisationRequiredResult(line, line2, invoice, VarianceSigns.Minus,
			APInvoice.CostVarianceAuthorisationRequiredType.NoAuthorisationRequired, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly,
			APInvoice.CostVarianceAuthorisationRequiredType.NoAuthorisationRequired, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly,
			APInvoice.CostVarianceAuthorisationRequiredType.NoAuthorisationRequired, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly);

			upTo2.MonitorTotalInvoiceVariance = true;
			above.MonitorTotalInvoiceVariance = true;
			upToNegative.MonitorTotalInvoiceVariance = true;
			aboveNegative.MonitorTotalInvoiceVariance = true;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertTotalCostVarianceAuthorisationRequiredResult(line, line2, invoice, VarianceSigns.Plus,
			APInvoice.CostVarianceAuthorisationRequiredType.HasAuthorisationRights, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly,
			APInvoice.CostVarianceAuthorisationRequiredType.HasAuthorisationRights, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly,
			APInvoice.CostVarianceAuthorisationRequiredType.AuthorisationRequired, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly);

			AssertTotalCostVarianceAuthorisationRequiredResult(line, line2, invoice, VarianceSigns.Minus,
			APInvoice.CostVarianceAuthorisationRequiredType.HasAuthorisationRights, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly,
			APInvoice.CostVarianceAuthorisationRequiredType.HasAuthorisationRights, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly,
			APInvoice.CostVarianceAuthorisationRequiredType.HasAuthorisationRights, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly);
		}

		public void TestEnettAllowMultiCurrencyPaymentPropertyValue()
		{
			APInvoice invoice = Factory.New<APInvoice>();
			EDIMessage message = invoice.EDIMessages.AddNew();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.eNett;
			message.EM_MessageType = "ENE";
			message.EM_MessageSubType = eNettMessageSubTypeList.Codes.GetNewInvoices;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Received;

			message.EM_MessageText = "blah blah xml <AllowMultiCurrencyPayment>true</AllowMultiCurrencyPayment> more blah blah";
			AssertEquals(true, invoice.EnettAllowMultiCurrencyPaymentPropertyValue);

			message.EM_MessageText = "blah blah xml <AllowMultiCurrencyPayment>false</AllowMultiCurrencyPayment> more blah blah";
			AssertEquals(false, invoice.EnettAllowMultiCurrencyPaymentPropertyValue);

			message.EM_MessageText = "nothing like <AllowMultiCurrencyPayment>";
			AssertEquals(false, invoice.EnettAllowMultiCurrencyPaymentPropertyValue);
		}

		public void TestValidationForIncompleteTransaction()
		{
			var invoice = (InvoicingBase)new BusinessObjectFactory().New(GetExpectedBusinessObjectType());

			invoice.Factory.SetContext(BusinessContext.SavingIncompleteTransaction);
			AssertEquals("Validation Type for Incomplete", typeof(IncompleteInvoicingBaseValidation), invoice.Validation.GetType());

			invoice.Factory.RemoveContext(BusinessContext.SavingIncompleteTransaction);
			AssertNotEquals("Validation Type for Regular", typeof(IncompleteInvoicingBaseValidation), invoice.Validation.GetType());
		}

		public void TestMultiInvoiceChargeTransfromLocalAmount()
		{
			var helper = new TestObjectCreator(Factory);
			var consol = helper.CreateConsol();
			var shipment = helper.CreateShipment("S001", consol);
			var consolCost = helper.CreateConsolCost(consol, helper.FRT);
			consolCost.E6_OSCostAmount = 500m;
			consolCost.E6_IsTaxAmountOverridden = true;
			consolCost.E6_OSGSTAmount_Calc = 50m;
			consolCost.E6_ApportionmentMethod = "GWT";
			AssertEquals(1, consolCost.ApportionmentCharges.Count);
			Factory.Save();

			var invoice1 = CreateInvoiceWithLineAndCharge(helper, consol, "INV001", helper.GST1, 500m, 50m);
			var invoice2 = CreateInvoiceWithLineAndCharge(helper, consol, "INV002", helper.GST1, 600m, 60m);

			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestMultiInvoiceChargeTransfromTaxRate()
		{
			var helper = new TestObjectCreator(Factory);
			var consol = helper.CreateConsol();
			var shipment = helper.CreateShipment("S001", consol);
			var consolCost = helper.CreateConsolCost(consol, helper.FRT);
			consolCost.E6_OSCostAmount = 500m;
			consolCost.E6_IsTaxAmountOverridden = true;
			consolCost.E6_OSGSTAmount_Calc = 50m;
			consolCost.E6_ApportionmentMethod = "GWT";
			AssertEquals(1, consolCost.ApportionmentCharges.Count);
			Factory.Save();

			var invoice1 = CreateInvoiceWithLineAndCharge(helper, consol, "INV001", helper.GST1, 500m, 50m);
			var invoice2 = CreateInvoiceWithLineAndCharge(helper, consol, "INV002", helper.GSTFREE1, 500m, 0m);

			AssertNoExceptionThrown(() => Factory.Save());
		}

		APInvoice CreateInvoiceWithLineAndCharge(TestObjectCreator helper, ForwardingConsol consol, string invNum, AccTaxRate taxRate, decimal amtExTax, decimal taxAmt)
		{
			var invoice1 = helper.CreateAPInvoice<APInvoice>(invNum, helper.AUD, 1, amtExTax, taxAmt, 0m, amtExTax, taxAmt, 0m);
			invoice1.AH_OH = helper.AALSHI.PK;

			var cost1 = invoice1.ConsolCosting.ConsolCosts.AddNew();
			cost1.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.CostSupporter.PK, consol.CostSupporter.Type);
			cost1.E6_AC_ChargeCode = helper.FRT.PK;
			cost1.E6_AT_TaxRate = taxRate.PK;
			cost1.E6_OSCostAmount = amtExTax;
			cost1.E6_OSGSTAmount_Calc = taxAmt;
			cost1.E6_ApportionmentMethod = "GWT";
			cost1.SetIsUsedForApportionment();
			invoice1.ImportSingleCost(cost1, (InvoicingLineBase)invoice1.Lines.AddNew());
			invoice1.SetContext(BusinessContext.ImportMultipleInvoicesWithSameTransformer);
			return invoice1;
		}

		public void TestDefaultValues()
		{
			APInvoice invoice1 = Factory.New<APInvoice>();

			AssertEquals("Ledger Type", ZArchitecture.Core.LedgerTypes.AccountsPayable, invoice1.AH_Ledger);
			AssertEquals("Transaction Count", 1, (int)invoice1.AH_TransactionCount);

			AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			APInvoice invoice2 = Factory.New<APInvoice>();

			Assert("AH_PostedToEFT must default to false due to local currency.", !invoice2.AH_PostedToEFT);

			AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			APInvoice invoice3 = Factory.New<APInvoice>();

			Assert("AH_PostedToEFT must default to false due to local currency.", !invoice3.AH_PostedToEFT);
		}

		public void TestSetInvoiceAsPaidValues()
		{
			ZDateTime now = ZDateTime.Now;
			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_LocalExTaxAmount = 100M;
			invoice.AH_LocalTaxAmount = 10M;

			AssertEquals("Invoice Approved", ZBool.False, invoice.AH_InvoiceApproved);
			AssertEquals("Fully Paid Date", ZDateTime.Empty, invoice.AH_FullyPaidDate);
			AssertEquals("Outstanding Amount", -110M, invoice.AH_OutstandingAmount);

			invoice.SetInvoiceAsPaid(now);

			AssertEquals("Invoice Approved", ZBool.True, invoice.AH_InvoiceApproved);
			AssertEquals("Fully Paid Date", now, invoice.AH_FullyPaidDate);
			AssertEquals("Outstanding Amount", 0M, invoice.AH_OutstandingAmount);
		}

		public void TestAH_OSTaxAmount_ReadOnly()
		{
			APInvoice invoice = Factory.New<APInvoice>();
			AssertEquals("AH_OSTaxAmount read only always", true, invoice.AH_OSTaxAmountInfo.ReadOnly);
		}

		public void TestTaxRates()
		{
			AccTaxRate activeTaxRate = Factory.NewWithValidTestData<AccTaxRate>();
			activeTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			AccTaxRate otherCountryActiveTaxRate = Factory.NewWithValidTestData<AccTaxRate>();
			otherCountryActiveTaxRate.AT_RN_NKCountry = "GB";
			AccTaxRate inactiveTaxRate = Factory.NewWithValidTestData<AccTaxRate>();
			inactiveTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			inactiveTaxRate.AT_IsActive = false;

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.TaxRates.Load();
			AssertEquals("Should contain active tax rate", true, invoice.TaxRates.Contains(activeTaxRate));
			AssertEquals("Should not contain other country tax rate", false, invoice.TaxRates.Contains(otherCountryActiveTaxRate));
			AssertEquals("Should not contain inactive tax rate", false, invoice.TaxRates.Contains(inactiveTaxRate));
		}

		public void TestDocManagerCode()
		{
			APInvoice invoice = (APInvoice)Factory.New(GetExpectedBusinessObjectType());
			AssertEquals("Code should be PIN. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", "PIN", ((IDocManagerSupport)invoice).DocManagerInfo.DocManagerCode);
		}

		#region CreateClaim

		public void TestCreateClaim()
		{
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			AssertEquals("There are no claims.", 0, Factory.GetDatabaseCount(typeof(AccQueryClaim)));
			AssertEquals("There are no UA credit notes", 0, Factory.GetDatabaseCount(typeof(UACreditNote)));

			invoice.CreateClaim(false).AY_OC = Factory.NewWithValidTestData<OrgContact>().PK;
			Factory.Save();

			AssertEquals("There are claims.", 1, Factory.GetDatabaseCount(typeof(AccQueryClaim)));
			AssertEquals("There are UA credit notes", 1, Factory.GetDatabaseCount(typeof(UACreditNote)));

			APAccQueryClaim claim = Factory.LoadTop1<APAccQueryClaim>(new ZQuery(AccQueryClaimSchema.AY_AH, invoice.PK));
			UACreditNote creditNote = claim.CreateAndAttachRelatedCreditNote();
			Factory.Save();

			AssertEquals("Transaction belongs to group not empty", false, invoice.AH_TransactionBelongsToGroup.IsEmpty);
			AssertEquals("Transaction belongs to group", invoice.AH_TransactionBelongsToGroup, creditNote.AH_TransactionBelongsToGroup);
			AssertEquals("Transaction Num", claim.AY_QueryClaimReference, creditNote.AH_TransactionNum);
		}

		public void TestCreateClaimWithDefaultLines()
		{
			UnapprovedTransactionTestHelper helper = new UnapprovedTransactionTestHelper(Factory, TestObjectCreator);
			GlbCompany currentCompany = GlbCompany.CurrentCompany;
			GlbBranch currentBranch = GlbBranch.CurrentBranch;

			SetupCostVarianceForClaim(helper.SourceCompany.GC_Code);

			Job helperJob = null;
			InvoicingBase invoice = null;

			try
			{
				helper.SetupSource();
				helper.ChangeCurrentCompanyViaBranch(helper.SourceCompany, helper.SourceCompanyBranch);
				helperJob = helper.CreateJobAndPost(PostType.Invoice, "1");

				helper.ChangeCurrentCompanyViaBranch(helper.TargetCompany, helper.TargetBranch);
				helper.EnsureCC1ChargeCodeInDBForCurrentCompany();
				helper.SetupTarget();
				helper.CreateJobForTargetDebtor(PostType.Invoice, "1");
				var converter = new UnapprovedTransactionConverter(Factory);
				AssertEquals("Prerequisite: candidates collection should hold one item", 1, converter.Candidates.Count);
				var systerCompanyInvoice = (InvoicingBase)converter.Candidates[0];
				invoice = converter.ConvertToAP(systerCompanyInvoice, false);
				invoice.InitialiseApprovingWithClaim(systerCompanyInvoice);
				invoice.Lines[0].AL_GE = TestObjectCreator.FESDepartment.PK;
				AssertNoErrors(invoice);

				var claim = invoice.CreateClaim(true);
				var uaCreditNote = claim.RelatedUnapprovedCreditNote;
				AssertNotNull("UACreditNote should be created", uaCreditNote);
				uaCreditNote.RunPreSaveValidation();
				AssertNoErrors("All UACreditNote fields should be valid", uaCreditNote);
				var relatedClaim = uaCreditNote.RelatedClaim;
				AssertNotNull("Related Claim should be created", relatedClaim);
				relatedClaim.RunPreSaveValidation();
				AssertNoErrors("All Claim fields should be valid", relatedClaim);
			}
			finally
			{
				helper.ChangeCurrentCompanyViaBranch(currentCompany, currentBranch);
				if (helperJob != null)
				{
					helperJob.Dispose();
				}

				if (invoice != null)
				{
					invoice.Lines[0].Job.Dispose();
				}
			}
		}

		public void TestCreateClaim_AmountSigns()
		{
			SetupCostVarianceForClaim(GlbCompany.CurrentCompany.GC_Code);

			var invoice = SetupAPInvoiceForClaim();
			var line = invoice.Lines[0];

			var claim = invoice.CreateClaim(true);
			var uaCreditNote = claim.RelatedUnapprovedCreditNote;
			AssertNotNull("UACreditNote should be created", uaCreditNote);
			AssertEquals("RelatedUnapprovedCreditNote Lines.Count", 1, uaCreditNote.Lines.Count);
			var uaLine = uaCreditNote.Lines[0];
			AssertNotEquals("Precondition: AL_LineAmount", 0m, uaLine.AL_LineAmount);
			AssertEquals("AL_LocalExTaxAmount", line.AL_LocalExTaxAmount, uaLine.AL_LocalExTaxAmount);
			AssertEquals("AL_LineAmount - different sign", line.AL_LineAmount, -uaLine.AL_LineAmount);
		}

		public void TestCreateClaimWhenCreditNoteIsNotAllowed()
		{
			SetupCostVarianceForClaim(GlbCompany.CurrentCompany.GC_Code);

			var invoice = SetupAPInvoiceForClaim();

			AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var claim = invoice.CreateClaim(true);
			AssertNotNull("claim", claim);
			AssertNull("RelatedUnapprovedCreditNote when not allowed", claim.RelatedUnapprovedCreditNote);

			AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			claim = invoice.CreateClaim(true);
			AssertNotNull("claim", claim);
			AssertNotNull("RelatedUnapprovedCreditNote when allowed", claim.RelatedUnapprovedCreditNote);
		}

		InvoicingBase SetupAPInvoiceForClaim()
		{
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "UA1", TestObjectCreator.AUD, 1, 100, 10, 100, 10);
			var uaCollection = new UnapprovedTransactionCandidateCollection(Factory);
			uaCollection.Add(arInvoice);

			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var invoice = TestObjectCreator.CreateInvoice(typeof(UAInvoice), "UA1");
			TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 100);
			invoice.InitialiseApprovingWithClaim(arInvoice);
			AssertNoErrors(invoice);
			return invoice;
		}

		static void SetupCostVarianceForClaim(ZString postingCompanyCode)
		{
			CostVarianceApproval valuesForTest = new CostVarianceApproval();
			valuesForTest.VarianceCalculationStyle = Core.Constants.CostVarianceCalculationStyle.LocalExTaxAmount;
			valuesForTest.VarianceComparisonOption = Core.Constants.CostVarianceComparisonOption.Job;
			CostVarianceApprovalAuthorisationRequirement upTo1 = valuesForTest.AuthorisationRequirements.AddNew();
			upTo1.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			upTo1.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			upTo1.Amount = 1M;
			CostVarianceApprovalAuthorisationRequirement above = valuesForTest.AuthorisationRequirements.AddNew();
			above.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			above.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			above.Amount = 1M;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			IntercompanyPostingConfigurationCollection postingConfigCollection = new IntercompanyPostingConfigurationCollection();
			IntercompanyPostingConfiguration postingConfig = postingConfigCollection.AddNew();
			postingConfig.Company = postingCompanyCode;
			postingConfig.MaxCostVarianceApprovalLevel = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			AccountingConfigurationRegistry.Instance.IntercompanyPostingConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, postingConfigCollection);
		}

		#endregion

		public void TestAH_RequisitionDateIsDefaultedFromAH_DueDate()
		{
			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_RequisitionDate = ZDateTime.Empty;
			Assert("Prerequisite: AH_RequisitionDate is empty", invoice.AH_RequisitionDate.IsEmpty);
			invoice.AH_DueDate = ZDateTime.BrettsBirthday;
			AssertEquals(ZDateTime.BrettsBirthday, invoice.AH_RequisitionDate);
		}

		public void TestIncompleteInvoiceCriticalValidationInfo_INTransactionWithoutLines()
		{
			using (ServiceContainerSuspenderHelper.AccTransactionHeaderWithLinesCriticalValidationActivatorService.Instance.GetActivator())
			{
				var emptyFactory = Factory.CreateNewFactory();
				var invoice = new TransactionCreator().CreateTransaction(emptyFactory, LedgerTypes.AccountsPayable, TransactionTypes.Invoice) as APInvoice;
				invoice.SaveAsIncomplete();
				var invoice2 = new TransactionCreator().CreateTransaction(emptyFactory, LedgerTypes.AccountsPayable, TransactionTypes.Invoice) as APInvoice;
				emptyFactory.Save();

				var invoiceWithoutLine = emptyFactory.NewWithValidTestData<APInvoice>();
				invoice.MoveFromIncompleteToPayableLedger();

				AssertContains("Precondition", "There is no data collected for this PK", CriticalValidationInfoCollectorService.GetOrCreateService(emptyFactory).GetInfo(invoice.PK, CriticalValidationInfoCollectorServiceKeyType.INTransactionWithoutLines));

				var ex = AssertExceptionThrown<OnSavingCriticalCheckException<AccTransactionHeader>>(() => emptyFactory.Save());

				AssertContains("INTransactionWithoutLines:", ex.DeveloperErrorMessage);
				AssertContains("Incomplete AP Invoice to post:", ex.DeveloperErrorMessage);
				AssertContains("> Line Count: 1", ex.DeveloperErrorMessage);
				AssertContains("New AP Invoice:", ex.DeveloperErrorMessage);
				AssertContains("> Line Count: 0", ex.DeveloperErrorMessage);

				ErrorReporter.Instance.Clear();
			}
		}

		public void TestIncompleteInvoiceCriticalValidationInfo_INTransactionSerializedData()
		{
			using (ServiceContainerSuspenderHelper.AccTransactionHeaderWithLinesCriticalValidationActivatorService.Instance.GetActivator())
			{
				var invoice = Factory.NewWithValidTestData<APInvoice>();
				AssertEquals("Precondition", false, invoice.Lines.Any());
				invoice.SaveAsIncomplete();

				var newFactory = new BusinessObjectFactory();
				var invoiceInNewFactory = newFactory.Load<APInvoice>(invoice.PK);

				var service = CriticalValidationInfoCollectorService.GetOrCreateService(newFactory);
				AssertNotContains("Precondition", "<IncompleteTransactions>", service.GetInfo(invoice.PK, CriticalValidationInfoCollectorServiceKeyType.INTransactionSerializedData));
				invoiceInNewFactory.RestoreSavedData();
				AssertContains("<IncompleteTransactions>", service.GetInfo(invoice.PK, CriticalValidationInfoCollectorServiceKeyType.INTransactionSerializedData));

				invoiceInNewFactory.MoveFromIncompleteToPayableLedger();
				var ex = AssertExceptionThrown<OnSavingCriticalCheckException<AccTransactionHeader>>(() => newFactory.Save());
				AssertContains("INTransactionSerializedData:", ex.DeveloperErrorMessage);
				AssertContains("<IncompleteTransactions>", ex.DeveloperErrorMessage);
				AssertContains(@"Tips for developers: please check whether the Transaction is deserialized from the old dirty XML data without lines.
Transactions can be saved as incomplete invoice without lines before [WI00545480 - Critical Validation: This transaction should have lines].
If there is no line in the XML data and
•	it's old data (created before this WI), users should enter lines and fix it by themselves.
•	or it's new data (created after this WI), please find out how this data is generated.", ex.DeveloperErrorMessage);

				ErrorReporter.Instance.Clear();
			}
		}

		public void TestAutoCreateComplianceDocumentWhenPostIncompleteInvoice()
		{
			AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			TestObjectCreator.AALSHI.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = Constants.OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge;
			TestObjectCreator.AALSHI.CompanyData.OB_ARCreateVATComplianceDocumentOnPosting = Constants.OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge;
			Factory.Save();

			var incompleteInvoice = new TransactionCreator().CreateTransaction(Factory, LedgerTypes.AccountsPayable, TransactionTypes.Invoice) as APInvoice;
			incompleteInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			incompleteInvoice.Lines.Cast<InvoicingLineBase>().ForEach(x => x.AL_OH = TestObjectCreator.AALSHI.PK);
			incompleteInvoice.SaveAsIncomplete();

			AssertEquals("Pre-condition", true, incompleteInvoice.IsInDatabase);
			var query = incompleteInvoice.GetComplianceDocumentByTransactionQuery_ForTestOnly(incompleteInvoice.PK);
			var complianceDocuments = Factory.Load<AccComplianceDocumentHeader>(query);
			AssertEquals(0, complianceDocuments.Length);
			AssertEquals("Auto-create compliance document feature does NOT apply to incomplete transactions", false, incompleteInvoice.CanCreateComplianceDocument);

			incompleteInvoice.MoveFromIncompleteToPayableLedger();

			AssertEquals("Auto-create compliance document feature applied when post an incomplete transaction", true, incompleteInvoice.CanCreateComplianceDocument);

			Factory.Save();

			complianceDocuments = Factory.Load<AccComplianceDocumentHeader>(query);
			AssertEquals("Compliance document created once posted", 1, complianceDocuments.Length);
		}

		public void TestDraftInvoiceWillBeSavedWhenSavingIncompleteInvoice()
		{
			var incompleteInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV1", organisation: TestObjectCreator.Creditor1);
			TestObjectCreator.CreateInvoiceLine(incompleteInvoice, TestObjectCreator.GLHeader1.PK, 100);

			var mockStatusUpdater = new Mock<IAccDraftInvoiceStatusChangeValidator>();
			mockStatusUpdater.Setup(h => h.CanChangeStatusTo(It.IsAny<AccDraftInvoiceHeader>(), It.IsAny<string>())).Returns(new AccDraftInvoiceStatusUpdateValidationResult { CanUpdate = true });
			ObjectFactory.Substitute(mockStatusUpdater.Object);

			var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			draftInvoice.AIH_AH_PostedTransactionHeader = incompleteInvoice.PK;
			draftInvoice.AIH_Status = Core.Constants.AccDraftInvoiceHeaderStatus.Processed;

			incompleteInvoice.SaveAsIncomplete();

			AssertEquals("PreCondition", true, incompleteInvoice.IsInDatabase);

			CombineAssertions("DraftInvoice should be saved.", () => {
				AssertEquals("IsInDatabase", true, draftInvoice.IsInDatabase);
				var sourceDraftInvoice = new BusinessObjectFactory().Load<AccDraftInvoiceHeader>(draftInvoice.PK);
				AssertEquals("AIH_AH_PostedTransactionHeader", incompleteInvoice.PK, sourceDraftInvoice.AIH_AH_PostedTransactionHeader);
				AssertEquals("AIH_Status", Core.Constants.AccDraftInvoiceHeaderStatus.Processed, sourceDraftInvoice.AIH_Status);
			});
		}

		public void TestIncompleteInvoiceCanBeDeletedWhenBeingLinkedByDraftInvoice()
		{
			var incompleteInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV1", organisation: TestObjectCreator.Creditor1);
			TestObjectCreator.CreateInvoiceLine(incompleteInvoice, TestObjectCreator.GLHeader1.PK, 100);
			incompleteInvoice.SaveAsIncomplete();

			var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			draftInvoice.AIH_AH_PostedTransactionHeader = incompleteInvoice.PK;
			draftInvoice.AIH_Status = Core.Constants.AccDraftInvoiceHeaderStatus.Processed;
			Factory.Save();

			AssertEquals("PreCondition, draft invoice's link should be saved.", incompleteInvoice.PK, new BusinessObjectFactory().Load<AccDraftInvoiceHeader>(draftInvoice.PK).AIH_AH_PostedTransactionHeader);
			AssertEquals("PreCondition, draft invoice's status should be saved", Core.Constants.AccDraftInvoiceHeaderStatus.Processed, new BusinessObjectFactory().Load<AccDraftInvoiceHeader>(draftInvoice.PK).AIH_Status);
			AssertEquals("PreCondition", true, incompleteInvoice.IsInDatabase);

			incompleteInvoice.Delete();
			Factory.Save();

			var sourceDraftInvoice = new BusinessObjectFactory().Load<AccDraftInvoiceHeader>(draftInvoice.PK);
			AssertEquals("incompleteInvoice should be removed from DB.", null, new BusinessObjectFactory().Load<AccTransactionHeader>(incompleteInvoice.PK));
			AssertEquals("draft invoice's link should be cleared.", ZGuid.Empty, sourceDraftInvoice.AIH_AH_PostedTransactionHeader);
			AssertEquals("draft invoice's status should be reset", Core.Constants.AccDraftInvoiceHeaderStatus.ApprovedForPosting, sourceDraftInvoice.AIH_Status);
		}

		public void TestIncompleteInvoiceCanNotBeDeletedWhenBeingLinkedByMultipleDraftInvoice()
		{
			var incompleteInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV1", organisation: TestObjectCreator.Creditor1);
			TestObjectCreator.CreateInvoiceLine(incompleteInvoice, TestObjectCreator.GLHeader1.PK, 100);
			incompleteInvoice.SaveAsIncomplete();
			IncompleteInvoiceBOIsSavedByFactoryServiceProvider.DeregisterInvoiceFromSaveOnlyInvoiceHeader(incompleteInvoice);

			CreateDraftInvoiceLink(incompleteInvoice.PK);
			CreateDraftInvoiceLink(incompleteInvoice.PK);
			Factory.Save();

			var linkedDraftInvoice = new BusinessObjectFactory().Load<AccDraftInvoiceHeader>(new ZQuery(AccDraftInvoiceHeaderSchema.AIH_AH_PostedTransactionHeader, incompleteInvoice.PK));
			AssertEquals("PreCondition", 2, linkedDraftInvoice.Length);
			AssertEquals("PreCondition", true, incompleteInvoice.IsInDatabase);

			incompleteInvoice.Delete();
			var exception = AssertExceptionThrown<ZSaveException>(() => Factory.Save());
			AssertContains("The DELETE statement conflicted with the REFERENCE constraint \"AccDraftInvoiceHeader_AIH_AH_PostedTransactionHeader_FK2_AccTransactionHeader_RRR_120N\"", exception.Message);
			AssertContains("table \"dbo.AccDraftInvoiceHeader\", column 'AIH_AH_PostedTransactionHeader'", exception.Message);

			void CreateDraftInvoiceLink(ZGuid invoicePK)
			{
				var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
				draftInvoice.AIH_AH_PostedTransactionHeader = invoicePK;
				draftInvoice.AIH_Status = Core.Constants.AccDraftInvoiceHeaderStatus.Processed;
			}
		}

		#region MatchedWithTNFJournalNum

		public void TestMatchWithTNFJournal()
		{
			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.USD, 1.2m, TestObjectCreator.AALSHI);
			apInvoice.SubmittedFromInvoicingForm = true;
			apInvoice.AH_TransactionNum = "T001";
			var line = (APInvoiceLine)apInvoice.Lines.AddNew();

			line.AL_AC = TestObjectCreator.CC1.PK;
			TestObjectCreator.Job1.JH_OA_LocalChargesAddr = ZGuid.Empty;
			line.AL_JH = TestObjectCreator.Job1.PK;
			line.AL_OSExTaxAmount = 250m;
			Factory.Save();

			AssertEquals("should not found any matched journal", ZString.Empty, ((APInvoice)apInvoice).MatchedWithTNFJournalNum);

			var otherFactory1 = new BusinessObjectFactory();
			var journal1 = otherFactory1.NewWithValidTestData<APJournal>();
			journal1.AH_TransactionCategory = Constants.TransactionCategory.Codes.TransactionNotFound;
			journal1.AH_OH = TestObjectCreator.AALSHI.PK;
			journal1.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			journal1.AH_ExchangeRate = 1.2m;
			journal1.AH_ChequeOrReference = "T001";
			journal1.AH_InvoiceAmount = 200;
			journal1.AH_OutstandingAmount = 200;
			journal1.AH_OSTotal = 200;
			otherFactory1.Save();

			var apInvoiceInOtherFactory1 = otherFactory1.Load<APInvoice>(apInvoice.PK);
			AssertEquals("should found the matched journal", journal1.AH_TransactionNum, apInvoiceInOtherFactory1.MatchedWithTNFJournalNum);

			apInvoiceInOtherFactory1.AH_TransactionNum = "I001";
			AssertEquals("should not found any matched journal", ZString.Empty, apInvoiceInOtherFactory1.MatchedWithTNFJournalNum);

			apInvoiceInOtherFactory1.AH_TransactionNum = "T001";
			journal1.AH_ChequeOrReference = "T002";
			var journal2 = otherFactory1.NewWithValidTestData<APJournal>();
			journal2.AH_TransactionCategory = Constants.TransactionCategory.Codes.TransactionNotFound;
			journal2.AH_OH = TestObjectCreator.AALSHI.PK;
			journal2.AH_GC = GlbCompany.CurrentCompany.PK;
			journal2.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			journal2.AH_ExchangeRate = 1.2m;
			journal2.AH_ChequeOrReference = "T001";
			journal2.AH_InvoiceAmount = 0;
			journal2.AH_OutstandingAmount = 0;
			journal2.AH_OSTotal = 0;
			otherFactory1.Save();

			var otherFactory2 = new BusinessObjectFactory();
			var apInvoiceInOtherFactory2 = otherFactory2.Load<APInvoice>(apInvoice.PK);
			AssertEquals("should not found any matched journal", ZString.Empty, apInvoiceInOtherFactory2.MatchedWithTNFJournalNum);
		}

		#endregion

		#region Get Journal for Matching

		public void TestGetJournalForMatching()
		{
			var apInvoice001 = CreateAPInvoice(transactionNum: "T001");
			var apInvoice002 = CreateAPInvoice(transactionNum: "T002");
			Factory.Save();

			var testApInvoice001 = new BusinessObjectFactory().Load<APInvoice>(apInvoice001.PK);
			var testApInvoice002 = new BusinessObjectFactory().Load<APInvoice>(apInvoice002.PK);
			CombineAssertions("Should not find any matched journal - no journal in DB.",
				() =>
				{
					AssertEquals("T001", null, GetJournalForMatching(testApInvoice001));
					AssertEquals("T002", null, GetJournalForMatching(testApInvoice002));
				});

			using (TestObjectCreator.SwitchEnvToCompany(TestObjectCreator.NonCurrentCompany))
			{
				_ = CreateAPJournal(chequeOrReference: "T001", amount: 200m);
				Factory.Save();
			}
			AssertEquals("Should not find the matched journal - different company.", null, GetJournalForMatching(testApInvoice001));

			var currentCompanyJournal = CreateAPJournal(chequeOrReference: "T001", amount: 200m);
			Factory.Save();
			AssertEquals("Should find the matched journal - same company.", currentCompanyJournal.PK, GetJournalForMatching(testApInvoice001).PK);

			_ = CreateAPJournal(chequeOrReference: "T002", amount: 0m);
			Factory.Save();
			AssertEquals("Should not found any matched journal - TNF journal has no outstanding amount.", null, GetJournalForMatching(testApInvoice002));

			APJournal CreateAPJournal(string chequeOrReference, decimal amount)
			{
				var journal = Factory.NewWithValidTestData<APJournal>();
				journal.AH_TransactionCategory = Constants.TransactionCategory.Codes.TransactionNotFound;
				journal.AH_OH = TestObjectCreator.AALSHI.PK;
				journal.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
				journal.AH_ExchangeRate = 1m;
				journal.AH_ChequeOrReference = chequeOrReference;
				journal.AH_InvoiceAmount = amount;
				journal.AH_OutstandingAmount = amount;
				journal.AH_OSTotal = amount;
				return journal;
			}

			InvoicingBase CreateAPInvoice(string transactionNum)
			{
				var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.USD, 1.2m, TestObjectCreator.AALSHI);
				apInvoice.SubmittedFromInvoicingForm = true;
				apInvoice.AH_TransactionNum = transactionNum;

				var line = (APInvoiceLine)apInvoice.Lines.AddNew();
				line.AL_AC = TestObjectCreator.CC1.PK;
				TestObjectCreator.Job1.JH_OA_LocalChargesAddr = ZGuid.Empty;
				line.AL_JH = TestObjectCreator.Job1.PK;
				line.AL_OSExTaxAmount = 250;

				return apInvoice;
			}
		}

		Journal.Journal GetJournalForMatching(InvoicingBase invoice)
		{
			var methodInfo = typeof(InvoicingBase).GetMethod("GetJournalForMatching", BindingFlags.NonPublic | BindingFlags.Instance);
			return methodInfo.Invoke(invoice, new object[] { Constants.TransactionCategory.Codes.TransactionNotFound }) as Journal.Journal;
		}

		#endregion

		public void TestCalculateAuthorisationByLinesNotExecutedWhenContextSetToCASS()
		{
			var valuesForTest = new CostVarianceApproval();
			valuesForTest.VarianceCalculationStyle = Core.Constants.CostVarianceCalculationStyle.LocalExTaxAmount;
			valuesForTest.VarianceComparisonOption = Core.Constants.CostVarianceComparisonOption.Job;
			CostVarianceApprovalAuthorisationRequirement upTo1 = valuesForTest.AuthorisationRequirements.AddNew();
			upTo1.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			upTo1.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			upTo1.Amount = 100M;
			upTo1.MonitorTotalInvoiceVariance = true;
			upTo1.TotalInvoiceVarianceAmount = 150M;
			CostVarianceApprovalAuthorisationRequirement above = valuesForTest.AuthorisationRequirements.AddNew();
			above.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			above.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			above.Amount = upTo1.Amount;
			above.MonitorTotalInvoiceVariance = true;
			above.TotalInvoiceVarianceAmount = 150M;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			Env.Security.CostVarianceApprovalLevel1.IsAllowed = true;
			Env.Security.CostVarianceApprovalLevel2.IsAllowed = false;

			var cAF = TestObjectCreator.CC1;

			Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001001"));
			Charge charge = TestObjectCreator.CreateCharge(job, cAF, "", TestObjectCreator.AUD, 100M, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 0M, null);
			Factory.Save();

			var invoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1, TestObjectCreator.Creditor1);
			var line = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job, cAF, TestObjectCreator.AUD, 1, "", 150);

			invoice.CalculateAuthorisationByLinesDBHitCount_ForTestOnly = 0;
			invoice.RunPreSaveValidation();
			AssertEquals("Database Hit Count without CASS context", 1, invoice.CalculateAuthorisationByLinesDBHitCount_ForTestOnly);

			Factory.SetContext(BusinessContext.CASS);
			invoice.CalculateAuthorisationByLinesDBHitCount_ForTestOnly = 0;
			invoice.RunPreSaveValidation();
			AssertEquals("Database Hit Count with CASS context", 0, invoice.CalculateAuthorisationByLinesDBHitCount_ForTestOnly);

			invoice.ClearCachedCostVarianceApproval_ForTestOnly();
			invoice.CostVarianceApprovalHelper.CalculateAuthorisationByLines();
			var costVarianceApproval = Factory.GetCachedValue<CostVarianceApproval>(CostVarianceApprovalExtension.CostVarianceApprovalExtensionHelper.GetCostVarianceApprovalKey_ForTestOnly(invoice.Company), () => null);
			AssertNull("CostVarianceApproval is not cached", costVarianceApproval);
		}

		public void TestLineChargeAmountSumQueryUsesTVP()
		{
			const string expectedQuery =
@"SELECT AL_PK, AL_JH, AL_LineType, AL_AC, AL_GB, AL_GE, AL_LineAmount
FROM dbo.AccTransactionLines
WHERE AL_JH IN (SELECT Value FROM @JobPKs)
AND AL_LineType IN ('WIP','REV','CST')
AND (AL_LineType IN ('REV','CST') OR (AL_LineType = 'WIP' AND AL_ReverseDate IS NULL))

UNION ALL

SELECT JR_PK, JR_JH, 'WIP', JR_AC, JR_GB, JR_GE, -JR_LocalSellAmt
FROM dbo.JobCharge
WHERE JR_JH IN (SELECT Value FROM @JobPKs)
AND JR_AL_ARLine IS NULL 
AND JR_LocalSellAmt <> 0";

			var sYD = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "SYD");
			var fEA = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FEA");
			var cAF = TestObjectCreator.CC1;

			var apInvoice = Factory.NewWithValidTestData<APInvoice>();

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001001"), false);

			var invLine = (APInvoiceLine)apInvoice.Lines.AddNew();
			invLine.AL_JH = job.PK;
			invLine.AL_LineType = TransactionLineTypes.Cost;
			invLine.AL_AC = cAF.PK;
			invLine.AL_GB = sYD.PK;
			invLine.AL_GE = fEA.PK;
			invLine.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			invLine.AL_ExchangeRate = 1M;
			invLine.AL_LocalExTaxAmount = 100M;
			invLine.AL_OSExTaxAmount = 100M;

			apInvoice.GetLineChargeAmountSumFromCache(invLine);

			AssertContains(expectedQuery, SqlEventTracker.Instance.LastSqlQuery);
		}

		[TestDate(2019, 12, 02)]
		public void TestPaymentDateOfConsolCostShouldBeEqualsToPaymentDateOfApportionedChargesForAPTransactionPosting()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "test001");
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			TestObjectCreator.CreateJob(shipment);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC11, 100);
			Factory.Save();

			var creditor = TestObjectCreator.Creditor1;
			creditor.CompanyData.OB_APPaymentTerms = Constants.InvoiceTerms.FromInvoiceDate;
			creditor.CompanyData.OB_APPaymentTermDays = 5;
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, creditor);

			var newConsolCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			newConsolCost.E6_AC_ChargeCode = TestObjectCreator.CC11.PK;

			var importer = new InvoicingBaseConsolCostImporter(Factory, newConsolCost, invoice);
			importer.ImportCostsIntoCosting(new BusinessObject[] { consolCost });
			invoice.ImportAllApportionmentsFromCosting();
			invoice.SetContext(BusinessContext.ImportMultipleInvoicesWithSameTransformer);

			creditor.CompanyData.OB_APPaymentTermDays = 10;
			invoice.AH_InvoiceDate = ZDateTime.Now.AddDays(-1);

			Factory.Save();
			Assert(true);
		}

		[ExpectNoExceptions]
		public override void TestBizObjectFields()
		{
			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Setup(x => x.Get().GetReversalStatusCodeReferenceType()).Returns("EEE");
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			base.TestBizObjectFields();
		}

		[TestedType(typeof(APInvoice))]
		public class APInvoiceMatchingTest : InvoicingBaseMatchingTest
		{
			protected override InvoicingBase GetNewInvoice()
			{
				return Factory.New<APInvoice>();
			}
		}

		#region CustomLogReferenceSuffix

		public void TestSaveLogCustomLogReferenceSuffix()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV001", TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 200M);
			Factory.Save();

			new APInvoiceReversing(invoice as APInvoice).Reverse();
			var reverseInvoice = invoice.ReverseInvoice;
			reverseInvoice.AH_TransactionNum = "CRD001";
			Factory.Save();

			var storageFile = invoice.DocManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 0, 1, 2, 3 }), "testFileName1", "ACV", description: "StorageFile type file");
			invoice.AH_Desc = "Des";
			Factory.Save();

			var logs = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, invoice.PK));
			AssertEquals(1, logs.Count(x => x.SL_Reference == "AP|INV|Reversed"));
			AssertEquals(2, logs.Count(x => x.SL_SE_NKEvent == Events.EditedARecordCode && x.SL_Reference.IsEmpty));

			storageFile = reverseInvoice.DocManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 0, 1, 2, 3 }), "testFileName1", "ACV", description: "StorageFile type file");
			reverseInvoice.AH_Desc = "Des";
			Factory.Save();

			logs = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, reverseInvoice.PK));
			AssertEquals(1, logs.Count(x => x.SL_Reference == "AP|CRD|Reversed"));
			AssertEquals(2, logs.Count(x => x.SL_SE_NKEvent == Events.EditedARecordCode && x.SL_Reference.IsEmpty));
		}

		#endregion

		#region Implementation

		TestObjectCreator fTestObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		#endregion

		protected override bool ShouldExpectTaxTotal => true;

		protected override bool CouldHaveAssociatedDraftInvoice => true;
	}
}
