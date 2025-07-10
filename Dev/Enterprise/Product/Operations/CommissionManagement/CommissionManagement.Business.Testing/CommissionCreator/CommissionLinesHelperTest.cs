using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business.Testing
{
	public class CommissionLinesHelperTest : CommissionCreatorTestCase
	{
		#region Percentage Commission Recipients

		public void TestAddPercentageCommissionRecipients()
		{
			var transactionHeader = Factory.New<ARInvoice>();
			var commissionHeader = Factory.New<AccCommissionHeader>();
			commissionHeader.CH0_AH_Source = transactionHeader.PK;
			var lineGroup = commissionHeader.LineGroups.AddNew();
			lineGroup.CLG_RX_NKTransactionCurrency = "AUD";
			lineGroup.CLG_TransactionAmount = 1500;
			lineGroup.CLG_RX_NKCommissionCurrency = "USD";
			lineGroup.CLG_TotalCommissionableAmount = 1000;

			var helper = new CommissionLinesHelperForTesting();
			helper.AddPercentageCommissionLines_ExposedForTesting(lineGroup, CommissionAgreementAndRatesForTesting);

			AssertEquals(2, lineGroup.Lines.Count);

			var commissionLine1 = lineGroup.Lines.First(x => x.CL0_CAT == AgreementPctRecipient1Rate.PK);
			CombineAssertions("commissionLine1 Properties", () =>
			{
				AssertEquals(AccCommissionLineSchema.Constants.CL0_GS_NKStaff, "ADL", commissionLine1.CL0_GS_NKStaff);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_RX_NKTransactionCurrency, "AUD", commissionLine1.CL0_RX_NKTransactionCurrency);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_TransactionAmount, (ZDecimal)1500, commissionLine1.CL0_TransactionAmount);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_CommissionType, CommissionTypes.Codes.PCT, commissionLine1.CL0_CommissionType);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_RX_NKCommissionCurrency, "USD", commissionLine1.CL0_RX_NKCommissionCurrency);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_TotalCommissionableAmount, (ZDecimal)1000, commissionLine1.CL0_TotalCommissionableAmount);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_ShareTotal, (ZShort)4, commissionLine1.CL0_ShareTotal);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_SharePortion, (ZShort)1, commissionLine1.CL0_SharePortion);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_ShareCommissionAmount, (ZDecimal)250, commissionLine1.CL0_ShareCommissionAmount);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_EntityPercentage, (ZDecimal)50, commissionLine1.CL0_EntityPercentage);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_EntityCommissionAmount, (ZDecimal)125, commissionLine1.CL0_EntityCommissionAmount);
			});

			var commissionLine2 = lineGroup.Lines.First(x => x.CL0_CAT == AgreementPctRecipient2Rate.PK);
			CombineAssertions("commissionLine2 Properties", () =>
			{
				AssertEquals(AccCommissionLineSchema.Constants.CL0_OH_Party, PartyA.PK, commissionLine2.CL0_OH_Party);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_RX_NKTransactionCurrency, "AUD", commissionLine2.CL0_RX_NKTransactionCurrency);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_TransactionAmount, (ZDecimal)1500, commissionLine2.CL0_TransactionAmount);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_CommissionType, CommissionTypes.Codes.PCT, commissionLine2.CL0_CommissionType);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_RX_NKCommissionCurrency, "USD", commissionLine2.CL0_RX_NKCommissionCurrency);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_TotalCommissionableAmount, (ZDecimal)1000, commissionLine2.CL0_TotalCommissionableAmount);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_ShareTotal, (ZShort)4, commissionLine2.CL0_ShareTotal);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_SharePortion, (ZShort)3, commissionLine2.CL0_SharePortion);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_ShareCommissionAmount, (ZDecimal)750, commissionLine2.CL0_ShareCommissionAmount);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_EntityPercentage, (ZDecimal)10, commissionLine2.CL0_EntityPercentage);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_EntityCommissionAmount, (ZDecimal)75, commissionLine2.CL0_EntityCommissionAmount);
			});
		}

		public void TestAddPercentageCommissionRecipients_ReinstateLines()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_JobNum = "S0001005";

			var transactionHeader = Factory.New<ARInvoice>();
			var commissionHeader = Factory.New<AccCommissionHeader>();
			commissionHeader.CH0_AH_Source = transactionHeader.PK;
			commissionHeader.CH0_CA0 = CommissionAgreementAndRatesForTesting.CommissionAgreement.PK;
			commissionHeader.CH0_SnapshotDateTime = ZDateTime.Now;
			commissionHeader.CH0_SnapshotEventCode = AccCommissionHeaderSnapshotEventList.Codes.JobClosure;
			commissionHeader.CH0_GroupingSourceTableCode = jobHeader.TablePrefix;
			commissionHeader.CH0_GroupingSourceID = jobHeader.PK;
			commissionHeader.CH0_GC = GlbCompany.CurrentCompany.PK;

			var lineGroup = commissionHeader.LineGroups.AddNew();
			lineGroup.CLG_RX_NKTransactionCurrency = "AUD";
			lineGroup.CLG_TransactionAmount = 1500;
			lineGroup.CLG_RX_NKCommissionCurrency = "USD";
			lineGroup.CLG_TotalCommissionableAmount = 1000;
			lineGroup.CLG_AC = chargeCode.PK;
			var recipient = CommissionAgreementAndRatesForTesting.CommissionAgreement.Recipients[0];

			var commissionLine = lineGroup.Lines.AddNew();
			using (commissionLine.GetValidationSuspender())
			{
				commissionLine.CL0_CAT = recipient.Rates[0].PK;
				commissionLine.CL0_GS_NKStaff = recipient.CAR_GS_NKStaff;
				commissionLine.CL0_OH_Party = recipient.CAR_OH_Party;

				commissionLine.CL0_CommissionType = CommissionTypes.Codes.PCT;
				commissionLine.CL0_RX_NKTransactionCurrency = lineGroup.CLG_RX_NKTransactionCurrency;
				commissionLine.CL0_TransactionAmount = lineGroup.CLG_TransactionAmount;
				commissionLine.CL0_RX_NKCommissionCurrency = lineGroup.CLG_RX_NKCommissionCurrency;
				commissionLine.CL0_TotalCommissionableAmount = lineGroup.CLG_TotalCommissionableAmount;
				commissionLine.CL0_ShareTotal = 1;
				commissionLine.CL0_SharePortion = 1;
				commissionLine.CL0_ShareCommissionAmount = (commissionLine.CL0_ShareTotal > 0) ? ((double)commissionLine.CL0_TotalCommissionableAmount * (double)commissionLine.CL0_SharePortion / (double)commissionLine.CL0_ShareTotal) : 0d;
				commissionLine.CL0_EntityPercentage = recipient.Rates[0].CAT_CommissionPercentage;
				commissionLine.CL0_EntityCommissionAmount = commissionLine.CL0_ShareCommissionAmount * commissionLine.CL0_EntityPercentage / 100m;
				commissionLine.CL0_ShouldReinstate = true;
			}

			Factory.Save();

			var helper = new CommissionLinesHelperForTesting();
			helper.AddPercentageCommissionLines_ExposedForTesting(lineGroup, CommissionAgreementAndRatesForTesting);

			AssertEquals(2, lineGroup.Lines.Count(x => x.CL0_ShareTotal > 1));

			var commissionLine1 = lineGroup.Lines.First(x => x.CL0_CAT == AgreementPctRecipient1Rate.PK && x.CL0_ShareTotal > 1);
			CombineAssertions("commissionLine1 Properties", () =>
			{
				AssertEquals(AccCommissionLineSchema.Constants.CL0_GS_NKStaff, "ADL", commissionLine1.CL0_GS_NKStaff);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_RX_NKTransactionCurrency, "AUD", commissionLine1.CL0_RX_NKTransactionCurrency);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_TransactionAmount, (ZDecimal)1500, commissionLine1.CL0_TransactionAmount);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_CommissionType, CommissionTypes.Codes.PCT, commissionLine1.CL0_CommissionType);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_RX_NKCommissionCurrency, "USD", commissionLine1.CL0_RX_NKCommissionCurrency);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_TotalCommissionableAmount, (ZDecimal)1000, commissionLine1.CL0_TotalCommissionableAmount);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_ShareTotal, (ZShort)4, commissionLine1.CL0_ShareTotal);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_SharePortion, (ZShort)1, commissionLine1.CL0_SharePortion);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_ShareCommissionAmount, (ZDecimal)250, commissionLine1.CL0_ShareCommissionAmount);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_EntityPercentage, (ZDecimal)50, commissionLine1.CL0_EntityPercentage);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_EntityCommissionAmount, (ZDecimal)125, commissionLine1.CL0_EntityCommissionAmount);
			});

			var commissionLine2 = lineGroup.Lines.First(x => x.CL0_CAT == AgreementPctRecipient2Rate.PK);
			CombineAssertions("commissionLine2 Properties", () =>
			{
				AssertEquals(AccCommissionLineSchema.Constants.CL0_OH_Party, PartyA.PK, commissionLine2.CL0_OH_Party);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_RX_NKTransactionCurrency, "AUD", commissionLine2.CL0_RX_NKTransactionCurrency);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_TransactionAmount, (ZDecimal)1500, commissionLine2.CL0_TransactionAmount);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_CommissionType, CommissionTypes.Codes.PCT, commissionLine2.CL0_CommissionType);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_RX_NKCommissionCurrency, "USD", commissionLine2.CL0_RX_NKCommissionCurrency);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_TotalCommissionableAmount, (ZDecimal)1000, commissionLine2.CL0_TotalCommissionableAmount);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_ShareTotal, (ZShort)4, commissionLine2.CL0_ShareTotal);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_SharePortion, (ZShort)3, commissionLine2.CL0_SharePortion);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_ShareCommissionAmount, (ZDecimal)750, commissionLine2.CL0_ShareCommissionAmount);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_EntityPercentage, (ZDecimal)10, commissionLine2.CL0_EntityPercentage);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_EntityCommissionAmount, (ZDecimal)75, commissionLine2.CL0_EntityCommissionAmount);
			});
		}

		#endregion

		#region Fixed Commission Recipients

		public void TestCreateFixedCommissionRecipients()
		{
			var transactionHeader = Factory.New<ARInvoice>();
			var commissionHeader = Factory.New<AccCommissionHeader>();
			commissionHeader.CH0_AH_Source = transactionHeader.PK;
			commissionHeader.CH0_OH_Customer = CommissionAgreementCustomer.PK;
			var lineGroup = commissionHeader.LineGroups.AddNew();
			lineGroup.CLG_TotalCommissionableAmount = 1;

			var helper = new CommissionLinesHelperForTesting();
			helper.CreateFixedCommissionLines_ExposedForTesting(commissionHeader, CommissionAgreementAndRatesForTesting);

			AssertEquals(2, commissionHeader.Lines.Count);

			var commissionRecipient1 = commissionHeader.Lines.First(x => x.CL0_CAT == AgreementFixRecipient1Rate.PK);
			CombineAssertions("commissionRecipient1 Properties", () =>
			{
				AssertEquals(AccCommissionLineSchema.Constants.CL0_GS_NKStaff, "SCW", commissionRecipient1.CL0_GS_NKStaff);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_RX_NKTransactionCurrency, "AUD", commissionRecipient1.CL0_RX_NKTransactionCurrency);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_TransactionAmount, (ZDecimal)500, commissionRecipient1.CL0_TransactionAmount);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_CommissionType, CommissionTypes.Codes.FIX, commissionRecipient1.CL0_CommissionType);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_RX_NKCommissionCurrency, "AUD", commissionRecipient1.CL0_RX_NKCommissionCurrency);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_TotalCommissionableAmount, (ZDecimal)500, commissionRecipient1.CL0_TotalCommissionableAmount);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_ShareTotal, (ZShort)4, commissionRecipient1.CL0_ShareTotal);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_SharePortion, (ZShort)1, commissionRecipient1.CL0_SharePortion);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_ShareCommissionAmount, (ZDecimal)125, commissionRecipient1.CL0_ShareCommissionAmount);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_EntityPercentage, (ZDecimal)100, commissionRecipient1.CL0_EntityPercentage);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_EntityCommissionAmount, (ZDecimal)125, commissionRecipient1.CL0_EntityCommissionAmount);
			});

			var commissionRecipient2 = commissionHeader.Lines.First(x => x.CL0_CAT == AgreementFixRecipient2Rate.PK);
			CombineAssertions("commissionRecipient2 Properties", () =>
			{
				AssertEquals(AccCommissionLineSchema.Constants.CL0_OH_Party, PartyB.PK, commissionRecipient2.CL0_OH_Party);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_RX_NKTransactionCurrency, "USD", commissionRecipient2.CL0_RX_NKTransactionCurrency);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_TransactionAmount, (ZDecimal)100, commissionRecipient2.CL0_TransactionAmount);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_CommissionType, CommissionTypes.Codes.FIX, commissionRecipient2.CL0_CommissionType);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_RX_NKCommissionCurrency, "USD", commissionRecipient2.CL0_RX_NKCommissionCurrency);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_TotalCommissionableAmount, (ZDecimal)100, commissionRecipient2.CL0_TotalCommissionableAmount);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_ShareTotal, (ZShort)4, commissionRecipient2.CL0_ShareTotal);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_SharePortion, (ZShort)3, commissionRecipient2.CL0_SharePortion);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_ShareCommissionAmount, (ZDecimal)75, commissionRecipient2.CL0_ShareCommissionAmount);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_EntityPercentage, (ZDecimal)100, commissionRecipient2.CL0_EntityPercentage);
				AssertEquals(AccCommissionLineSchema.Constants.CL0_EntityCommissionAmount, (ZDecimal)75, commissionRecipient2.CL0_EntityCommissionAmount);
			});
		}

		public void TestCreateFixedCommissionRecipients_DoNotCreateIfNoLineGroups()
		{
			var transactionHeader = Factory.New<ARInvoice>();
			var commissionHeader = Factory.New<AccCommissionHeader>();
			commissionHeader.CH0_AH_Source = transactionHeader.PK;
			commissionHeader.CH0_OH_Customer = CommissionAgreementCustomer.PK;

			AssertEquals("Precondition", 0, commissionHeader.LineGroups.Count);

			var helper = new CommissionLinesHelperForTesting();
			helper.CreateFixedCommissionLines_ExposedForTesting(commissionHeader, CommissionAgreementAndRatesForTesting);

			AssertEquals("Should only create fixed commission lines if there are any line groups", 0, commissionHeader.Lines.Count);
		}

		public void TestCreateFixedCommissionRecipients_DoNotCreateIfLineGroupCommissionAmountNotGreaterThanZero()
		{
			var transactionHeader = Factory.New<ARInvoice>();
			var commissionHeader = Factory.New<AccCommissionHeader>();
			commissionHeader.CH0_AH_Source = transactionHeader.PK;
			commissionHeader.CH0_OH_Customer = CommissionAgreementCustomer.PK;
			var lineGroup = commissionHeader.LineGroups.AddNew();
			lineGroup.CLG_TotalCommissionableAmount = 0;

			var helper = new CommissionLinesHelperForTesting();
			helper.CreateFixedCommissionLines_ExposedForTesting(commissionHeader, CommissionAgreementAndRatesForTesting);
			AssertEquals("Should only create fixed commission lines if total commission amount greater than zero", 0, commissionHeader.Lines.Count);

			lineGroup.CLG_TotalCommissionableAmount = 1;
			helper.CreateFixedCommissionLines_ExposedForTesting(commissionHeader, CommissionAgreementAndRatesForTesting);
			AssertEquals("Should have created fixed commission lines", 2, commissionHeader.Lines.Count);
		}

		public void TestCreateFixedCommissionRecipients_DoNotCreateIfAlreadyExists()
		{
			var transactionHeader = Factory.NewWithValidTestData<ARInvoice>();
			var commissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeader.CH0_AH_Source = transactionHeader.PK;
			commissionHeader.CH0_OH_Customer = CommissionAgreementCustomer.PK;
			commissionHeader.CH0_GroupingSourceID = transactionHeader.PK;
			commissionHeader.CH0_GroupingSourceTableCode = transactionHeader.TablePrefix;
			var lineGroup = commissionHeader.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery()));
			lineGroup.CLG_TotalCommissionableAmount = 1;

			var helper = new CommissionLinesHelperForTesting();
			helper.CreateFixedCommissionLines_ExposedForTesting(commissionHeader, CommissionAgreementAndRatesForTesting);
			AssertEquals("Should have created fixed commission lines", 2, commissionHeader.Lines.Count);

			helper.CreateFixedCommissionLines_ExposedForTesting(commissionHeader, CommissionAgreementAndRatesForTesting);
			AssertEquals("Should not create fixed commission lines again since already exists for transaction", 2, commissionHeader.Lines.Count);

			var commissionHeader2 = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeader2.CH0_AH_Source = transactionHeader.PK;
			commissionHeader2.CH0_OH_Customer = CommissionAgreementCustomer.PK;
			commissionHeader2.CH0_GroupingSourceID = transactionHeader.PK;
			commissionHeader2.CH0_GroupingSourceTableCode = transactionHeader.TablePrefix;
			var lineGroup2 = commissionHeader2.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery()));
			lineGroup2.CLG_TotalCommissionableAmount = 1;

			helper.CreateFixedCommissionLines_ExposedForTesting(commissionHeader2, CommissionAgreementAndRatesForTesting);
			AssertEquals("Should not create fixed commission lines again since already exists for transaction", 0, commissionHeader2.Lines.Count);

			Factory.Save();

			helper.CreateFixedCommissionLines_ExposedForTesting(commissionHeader, CommissionAgreementAndRatesForTesting);
			AssertEquals("Should not create fixed commission lines again since already exists for transaction", 2, commissionHeader.Lines.Count);

			helper.CreateFixedCommissionLines_ExposedForTesting(commissionHeader2, CommissionAgreementAndRatesForTesting);
			AssertEquals("Should not create fixed commission lines again since already exists for transaction", 0, commissionHeader2.Lines.Count);
		}

		#endregion

		#region Should Create Commission Line

		public void TestShouldCreateCommissionLine_NoLines()
		{
			var helper = new CommissionLinesHelperForTesting();

			var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader.AH_TransactionNum = "0001000";

			var commissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeader.CH0_AH_Source = transactionHeader.PK;
			commissionHeader.CH0_GroupingSourceID = transactionHeader.PK;
			commissionHeader.CH0_GroupingSourceTableCode = transactionHeader.TablePrefix;
			Factory.Save();

			Assert("Transaction has no lines so we should create commissions.", helper.ShouldCreateCommissionLine(null, commissionHeader, null));
		}

		public void TestShouldCreateCommissionLine_NoLinesToReinstate()
		{
			var helper = new CommissionLinesHelperForTesting();

			var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader.AH_TransactionNum = "0001000";

			var agreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();

			var commissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeader.CH0_AH_Source = transactionHeader.PK;
			commissionHeader.CH0_CA0 = agreement.PK;
			commissionHeader.CH0_GroupingSourceID = transactionHeader.PK;
			commissionHeader.CH0_GroupingSourceTableCode = transactionHeader.TablePrefix;

			Enumerable.Range(0, 3).ForEach(x => commissionHeader.Lines.AddNew().CL0_ShouldReinstate = false);
			Factory.Save();

			var accLines = Factory.Load<AccCommissionLine>(new ZQuery());
			var viewLines = Factory.Load<ViewCommissionLine>(new ZQuery());

			AssertEquals("Pre-Condition", 3, accLines.Length);
			AssertEquals("Pre-Condition", 3, viewLines.Length);

			accLines.ForEach(line => Assert("CL0_ShouldReinstate should be false", !line.CL0_ShouldReinstate));
			viewLines.ForEach(line => Assert("VCL_ShouldReinstate should be false", !line.VCL_ShouldReinstate));

			Assert("Transaction has no lines to reinstate so we should NOT create commissions.", !helper.ShouldCreateCommissionLine(null, commissionHeader, null));
		}

		public void TestShouldCreateCommissionLine_OneLineToReinstate()
		{
			var helper = new CommissionLinesHelperForTesting();

			var transactionDetails = CommissionTestObjectCreator.CreateNewTransactionWithCommissionableLines(Factory, 3, true, LedgerTypes.AccountsPayable, transactionNum: "0001000");

			var accLines = Factory.Load<AccCommissionLine>(new ZQuery());
			AssertEquals("Pre-Condition", 3, accLines.Length);

			accLines[0].CL0_ShouldReinstate = false;
			accLines[1].CL0_ShouldReinstate = false;
			Factory.Save();

			var viewLines = Factory.Load<ViewCommissionLine>(new ZQuery());
			AssertEquals("Pre-Condition", 3, viewLines.Length);

			AssertEquals("Only 1 line should be flagged to be reinstated.", 1, accLines.Count(line => line.CL0_ShouldReinstate));
			AssertEquals("Only 1 line should be flagged to be reinstated.", 1, viewLines.Count(line => line.VCL_ShouldReinstate));

			Assert("Transaction has lines to reinstate so we SHOULD create commissions.", helper.ShouldCreateCommissionLine(null, transactionDetails.AccCommissionHeader, transactionDetails.RecipientRatePair));
		}

		public void TestShouldCreateCommissionLine_ShouldNotReinstateFlaggedLine()
		{
			var helper = new CommissionLinesHelperForTesting();

			var agreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();

			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_Code = "TST1";

			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_Code = "TST2";

			var recipientRatePair = CommissionTestObjectCreator.GetNewRecipientRatePair(Factory);

			var transactionDetails1 = CommissionTestObjectCreator.CreateNewTransactionWithCommissionableLines(Factory, 1, false, LedgerTypes.AccountsPayable, chargeCode1, "0001000", recipientRatePair: recipientRatePair);
			var transactionDetails2 = CommissionTestObjectCreator.CreateNewTransactionWithCommissionableLines(Factory, 1, false, LedgerTypes.AccountsPayable, chargeCode2, existingHeader: transactionDetails1.AccTransactionHeader, recipientRatePair: recipientRatePair);

			transactionDetails1.AccCommissionHeader.CH0_CA0 = agreement.PK;
			transactionDetails2.AccCommissionHeader.CH0_CA0 = agreement.PK;

			var line = transactionDetails2.AccCommissionHeader.LineGroups[0].Lines.FirstOrDefault();
			line.CL0_ShouldReinstate = false;
			Factory.Save();

			var newHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			newHeader.CH0_AH_Source = transactionDetails1.AccCommissionHeader.CH0_AH_Source;
			newHeader.CH0_CA0 = agreement.PK;
			newHeader.CH0_GroupingSourceID = transactionDetails1.AccCommissionHeader.CH0_AH_Source;
			newHeader.CH0_GroupingSourceTableCode = transactionDetails1.AccTransactionHeader.TablePrefix;

			var newLineGroupThatShouldntBeReinstated = newHeader.LineGroups.AddNew();
			newLineGroupThatShouldntBeReinstated.CLG_AC = chargeCode2.PK;
			newLineGroupThatShouldntBeReinstated.CLG_RX_NKTransactionCurrency = transactionDetails2.RecipientRatePair.Rate.CAT_RX_NKCommissionCurrency;

			Factory.Save();

			Assert("Should return false as an existing line for the same invoice, charge code, currency and staff/party exists, and was flagged as !CL0_ShouldReinstate.", !helper.ShouldCreateCommissionLine(newLineGroupThatShouldntBeReinstated, null, transactionDetails2.RecipientRatePair));
		}

		public void TestShouldCreateCommissionLine_ShouldReinstateNonFlaggedLine()
		{
			var helper = new CommissionLinesHelperForTesting();

			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_Code = "TST1";

			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_Code = "TST2";

			var recipientRatePair = CommissionTestObjectCreator.GetNewRecipientRatePair(Factory);

			var transactionDetails1 = CommissionTestObjectCreator.CreateNewTransactionWithCommissionableLines(Factory, 1, false, LedgerTypes.AccountsPayable, chargeCode1, "0001000", recipientRatePair: recipientRatePair);
			var transactionDetails2 = CommissionTestObjectCreator.CreateNewTransactionWithCommissionableLines(Factory, 1, false, LedgerTypes.AccountsPayable, chargeCode2, existingHeader: transactionDetails1.AccTransactionHeader, recipientRatePair: recipientRatePair);

			var newHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			newHeader.CH0_AH_Source = transactionDetails1.AccCommissionHeader.CH0_AH_Source;
			newHeader.CH0_GroupingSourceTableCode = transactionDetails1.AccTransactionHeader.TablePrefix;
			newHeader.CH0_GroupingSourceID = transactionDetails1.AccTransactionHeader.PK;

			var newLineGroupThatShouldBeReinstated = newHeader.LineGroups.AddNew();
			newLineGroupThatShouldBeReinstated.CLG_AC = chargeCode2.PK;
			newLineGroupThatShouldBeReinstated.CLG_RX_NKTransactionCurrency = transactionDetails2.RecipientRatePair.Rate.CAT_RX_NKCommissionCurrency;

			Factory.Save();

			Assert("Should return true as an existing line for the same invoice, charge code, currency and staff/party exists, and was marked as CL0_ShouldReinstate.", helper.ShouldCreateCommissionLine(newLineGroupThatShouldBeReinstated, null, transactionDetails2.RecipientRatePair));
		}

		public void TestShouldCreateCommissionLine_MultipleCommissionAgreements_Fixed()
		{
			var agreement1 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			var agreement2 = Factory.NewWithValidTestData<OrgCommissionAgreement>();

			var transactionDetails = CommissionTestObjectCreator.CreateNewTransactionWithCommissionableLines(Factory, 3, true, LedgerTypes.AccountsPayable, transactionNum: "0001000");
			transactionDetails.AccCommissionHeader.CH0_CA0 = agreement1.PK;

			transactionDetails.AccCommissionHeader.Lines.ForEach(line => { line.Cancel(); line.CL0_ShouldReinstate = false; });

			var commissionHeader2 = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeader2.CH0_AH_Source = transactionDetails.AccTransactionHeader.PK;
			commissionHeader2.CH0_CA0 = agreement2.PK;

			Factory.Save();

			var helper = new CommissionLinesHelperForTesting();
			Assert("First Commission Header should not create lines", !helper.ShouldCreateCommissionLine(null, transactionDetails.AccCommissionHeader, transactionDetails.RecipientRatePair));
			Assert("Second Commission Header should create lines", helper.ShouldCreateCommissionLine(null, commissionHeader2, transactionDetails.RecipientRatePair));
		}

		public void TestShouldCreateCommissionLine_MultipleCommissionAgreements_Percentage()
		{
			var agreement1 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			var agreement2 = Factory.NewWithValidTestData<OrgCommissionAgreement>();

			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_Code = "TST1";

			var transactionDetails = CommissionTestObjectCreator.CreateNewTransactionWithCommissionableLines(Factory, 3, false, LedgerTypes.AccountsPayable, chargeCode1, "0001000");
			transactionDetails.AccCommissionHeader.CH0_CA0 = agreement1.PK;
			transactionDetails.AccCommissionHeader.Lines.ForEach(line => { line.Cancel(); line.CL0_ShouldReinstate = false; });

			var commissionHeader2 = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeader2.CH0_AH_Source = transactionDetails.AccTransactionHeader.PK;
			commissionHeader2.CH0_CA0 = agreement2.PK;

			var lineGroup = commissionHeader2.LineGroups.AddNew();
			lineGroup.CLG_AC = chargeCode1.PK;

			Factory.Save();

			var helper = new CommissionLinesHelperForTesting();
			Assert("Line Group in first Commission Header should not create lines", !helper.ShouldCreateCommissionLine(transactionDetails.AccCommissionHeader.LineGroups.First(), transactionDetails.AccCommissionHeader, transactionDetails.RecipientRatePair));
			Assert("Line Group in second Commission Header should create lines", helper.ShouldCreateCommissionLine(lineGroup, commissionHeader2, transactionDetails.RecipientRatePair));
		}

		public void TestCommissionLinesCreated_SingleInvoiceMultipleJobs()
		{
			SetupTransactionsAndCommissions();

			Job1.Close(null, null);

			Factory.Save();
			AssertEquals(1, Factory.GetDatabaseCount(typeof(ViewCommissionLine)));

			Job2.Close(null, null);

			Factory.Save();
			AssertEquals("1 line from Job1 + 2 lines from Job2", 3, Factory.GetDatabaseCount(typeof(ViewCommissionLine)));
		}

		public void TestShouldCreateCommissionLine_SingleInvoiceMultipleJobs()
		{
			var agreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();

			var chargeCodeABC = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCodeABC.AC_IsCommissionable = true;
			chargeCodeABC.AC_Code = "ABC";

			var chargeCodeDEF = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCodeDEF.AC_IsCommissionable = true;
			chargeCodeDEF.AC_Code = "DEF";

			var shipment1 = TestObjectCreator.CreateShipment("1001");
			var job1 = TestObjectCreator.CreateJob(shipment1);

			var shipment2 = TestObjectCreator.CreateShipment("1002");
			var job2 = TestObjectCreator.CreateJob(shipment2);

			var transactionWithABCLine1 = CommissionTestObjectCreator.CreateNewTransactionWithCommissionableLines(Factory, 1, false, LedgerTypes.AccountsPayable, chargeCodeABC, "10001", job: job1, recipientRatePair: RecipientRatePair);
			transactionWithABCLine1.AccCommissionHeader.CH0_CA0 = agreement.PK;

			var commissionHeaderABC = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderABC.CH0_AH_Source = transactionWithABCLine1.AccTransactionHeader.PK;
			commissionHeaderABC.CH0_GroupingSourceID = job2.PK;
			commissionHeaderABC.CH0_GroupingSourceTableCode = job2.TablePrefix;
			commissionHeaderABC.CH0_CA0 = agreement.PK;

			var lineGroupABC = commissionHeaderABC.LineGroups.AddNew();
			lineGroupABC.CLG_AC = chargeCodeABC.PK;

			var commissionHeaderDEF = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderDEF.CH0_AH_Source = transactionWithABCLine1.AccTransactionHeader.PK;
			commissionHeaderDEF.CH0_GroupingSourceID = job2.PK;
			commissionHeaderDEF.CH0_GroupingSourceTableCode = job2.TablePrefix;
			commissionHeaderDEF.CH0_CA0 = agreement.PK;

			var lineGroupDEF = commissionHeaderDEF.LineGroups.AddNew();
			lineGroupDEF.CLG_AC = chargeCodeDEF.PK;

			Factory.Save();

			var helper = new CommissionLinesHelperForTesting();
			Assert(helper.ShouldCreateCommissionLine(lineGroupABC, commissionHeaderABC, RecipientRatePair));
			Assert(helper.ShouldCreateCommissionLine(lineGroupDEF, commissionHeaderDEF, RecipientRatePair));
		}

		public void TestShouldCreateCommissionLine_SingleInvoiceMultipleChargeCodesDifferentJobs()
		{
			var agreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();

			var shipment1 = TestObjectCreator.CreateShipment("1001");
			var shipment2 = TestObjectCreator.CreateShipment("1002");
			var shipment3 = TestObjectCreator.CreateShipment("1003");
			var job1 = TestObjectCreator.CreateJob(shipment1);
			var job2 = TestObjectCreator.CreateJob(shipment2);
			var job3 = TestObjectCreator.CreateJob(shipment3);

			var chargeCodeABC = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCodeABC.AC_IsCommissionable = true;
			chargeCodeABC.AC_Code = "ABC";

			var chargeCodeDEF = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCodeDEF.AC_IsCommissionable = true;
			chargeCodeDEF.AC_Code = "DEF";

			var chargeCodeGHI = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCodeGHI.AC_IsCommissionable = true;
			chargeCodeGHI.AC_Code = "GHI";

			var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader.AH_TransactionNum = "10001";

			var commissionHeaderABC = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderABC.CH0_AH_Source = transactionHeader.PK;
			commissionHeaderABC.CH0_CA0 = agreement.PK;
			commissionHeaderABC.CH0_GroupingSourceID = job1.PK;
			commissionHeaderABC.CH0_GroupingSourceTableCode = job1.TablePrefix;

			var lineGroupABC = commissionHeaderABC.LineGroups.AddNew();
			lineGroupABC.CLG_AC = chargeCodeABC.PK;
			lineGroupABC.CLG_RX_NKTransactionCurrency = "AUD";

			var commissionHeaderDEF = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderDEF.CH0_AH_Source = transactionHeader.PK;
			commissionHeaderDEF.CH0_CA0 = agreement.PK;
			commissionHeaderDEF.CH0_GroupingSourceID = job2.PK;
			commissionHeaderDEF.CH0_GroupingSourceTableCode = job2.TablePrefix;

			var lineGroupDEF = commissionHeaderDEF.LineGroups.AddNew();
			lineGroupDEF.CLG_AC = chargeCodeDEF.PK;
			lineGroupDEF.CLG_RX_NKTransactionCurrency = "AUD";

			var commissionHeaderGHI = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderGHI.CH0_AH_Source = transactionHeader.PK;
			commissionHeaderGHI.CH0_CA0 = agreement.PK;
			commissionHeaderGHI.CH0_GroupingSourceID = job3.PK;
			commissionHeaderGHI.CH0_GroupingSourceTableCode = job3.TablePrefix;

			var lineGroupGHI = commissionHeaderGHI.LineGroups.AddNew();
			lineGroupGHI.CLG_AC = chargeCodeGHI.PK;
			lineGroupGHI.CLG_RX_NKTransactionCurrency = "AUD";

			Factory.Save();

			var helper = new CommissionLinesHelperForTesting();
			Assert(helper.ShouldCreateCommissionLine(lineGroupABC, commissionHeaderDEF, RecipientRatePair));
			Assert(helper.ShouldCreateCommissionLine(lineGroupDEF, commissionHeaderDEF, RecipientRatePair));
			Assert(helper.ShouldCreateCommissionLine(lineGroupGHI, commissionHeaderGHI, RecipientRatePair));

			CommissionTestObjectCreator.SetLineValuesFromRecipientRatePair(lineGroupABC.Lines.AddNew(), RecipientRatePair);
			CommissionTestObjectCreator.SetLineValuesFromRecipientRatePair(lineGroupDEF.Lines.AddNew(), RecipientRatePair);
			CommissionTestObjectCreator.SetLineValuesFromRecipientRatePair(lineGroupGHI.Lines.AddNew(), RecipientRatePair);

			Factory.Save();
			Assert("Line already exists for this, so a new line should be created.", helper.ShouldCreateCommissionLine(lineGroupABC, commissionHeaderABC, RecipientRatePair));
			Assert("Line already exists for this, so a new line should be created.", helper.ShouldCreateCommissionLine(lineGroupDEF, commissionHeaderDEF, RecipientRatePair));
			Assert("Line already exists for this, so a new line should be created.", helper.ShouldCreateCommissionLine(lineGroupGHI, commissionHeaderGHI, RecipientRatePair));
		}

		public void TestShouldCreateCommissionLine_SingleInvoiceSameChargeCodes()
		{
			SetupTransactionsAndCommissions(defaultChargeCode: TestObjectCreator.CC1);
			AssertEquals("Pre-condition", 0, Factory.GetDatabaseCount(typeof(ViewCommissionLine)));

			Job2.Close(null, null);
			Factory.Save();

			var transactionLineQuery = new ZQuery(AccTransactionLinesSchema.AL_AC, TestObjectCreator.CC1.PK);
			transactionLineQuery.AddToFilter(new ZQuery(AccTransactionLinesSchema.AL_JH, Job2.PK), JoinCondition.And);

			AssertEquals("2 Transaction lines + 2 Accrual lines should exist for Job2 with charge code CC1", 4, Factory.GetDatabaseCount(typeof(AccTransactionLines), transactionLineQuery));
			AssertEquals("All lines should be merged.", 1, Factory.GetDatabaseCount(typeof(ViewCommissionLine)));
		}

		#endregion

		#region Test Data

		void SetupTestData()
		{
			PartyA = Factory.NewWithValidTestData<OrgHeader>();
			PartyB = Factory.NewWithValidTestData<OrgHeader>();
			CommissionAgreementCustomer = Factory.NewWithValidTestData<OrgHeader>();

			Opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			CommissionAgreement = Opportunity.ApprovedCommissionAgreements.AddNew();
			CommissionAgreement.CA0_OH_Customer = CommissionAgreementCustomer.PK;
			CommissionAgreement.FillWithValidTestData();

			AgreementPctRecipient1 = CommissionAgreement.Recipients.AddNew();
			AgreementPctRecipient1.CAR_GS_NKStaff = "ADL";
			AgreementPctRecipient1.CAR_CommissionType = CommissionTypes.Codes.PCT;
			AgreementPctRecipient1.CAR_Share = 1;
			AgreementPctRecipient1Rate = AgreementPctRecipient1.Rates.AddNew();
			AgreementPctRecipient1Rate.CAT_CommissionPercentage = 50;

			AgreementPctRecipient2 = CommissionAgreement.Recipients.AddNew();
			AgreementPctRecipient2.CAR_OH_Party = PartyA.PK;
			AgreementPctRecipient2.CAR_CommissionType = CommissionTypes.Codes.PCT;
			AgreementPctRecipient2.CAR_Share = 3;
			AgreementPctRecipient2Rate = AgreementPctRecipient2.Rates.AddNew();
			AgreementPctRecipient2Rate.CAT_CommissionPercentage = 10;

			AgreementFixRecipient1 = CommissionAgreement.Recipients.AddNew();
			AgreementFixRecipient1.CAR_GS_NKStaff = "SCW";
			AgreementFixRecipient1.CAR_CommissionType = CommissionTypes.Codes.FIX;
			AgreementFixRecipient1.CAR_Share = 1;
			AgreementFixRecipient1Rate = AgreementFixRecipient1.Rates.AddNew();
			AgreementFixRecipient1Rate.CAT_RX_NKCommissionCurrency = "AUD";
			AgreementFixRecipient1Rate.CAT_CommissionAmount = 500;

			AgreementFixRecipient2 = CommissionAgreement.Recipients.AddNew();
			AgreementFixRecipient2.CAR_OH_Party = PartyB.PK;
			AgreementFixRecipient2.CAR_CommissionType = CommissionTypes.Codes.FIX;
			AgreementFixRecipient2.CAR_Share = 3;
			AgreementFixRecipient2Rate = AgreementFixRecipient2.Rates.AddNew();
			AgreementFixRecipient2Rate.CAT_RX_NKCommissionCurrency = "USD";
			AgreementFixRecipient2Rate.CAT_CommissionAmount = 100d;

			Factory.Save();

			CommissionAgreementAndRatesForTesting = new CommissionAgreementAndRatesForTesting();
			CommissionAgreementAndRatesForTesting.CommissionAgreement = CommissionAgreement;
			CommissionAgreementAndRatesForTesting.RecipientRatePairs =
				new[] { AgreementPctRecipient1Rate, AgreementPctRecipient2Rate, AgreementFixRecipient1Rate, AgreementFixRecipient2Rate }
				.Select(x => new RecipientRatePair(x.CommissionAgreementRecipient, x))
				.ToArray();
		}

		OrgHeader PartyA;
		OrgHeader PartyB;
		OrgHeader CommissionAgreementCustomer;
		OrgOpportunity Opportunity;
		OrgCommissionAgreement CommissionAgreement;
		OrgCommissionAgreementRecipient AgreementPctRecipient1;
		OrgCommissionAgreementRecipientRate AgreementPctRecipient1Rate;
		OrgCommissionAgreementRecipient AgreementPctRecipient2;
		OrgCommissionAgreementRecipientRate AgreementPctRecipient2Rate;
		OrgCommissionAgreementRecipient AgreementFixRecipient1;
		OrgCommissionAgreementRecipientRate AgreementFixRecipient1Rate;
		OrgCommissionAgreementRecipient AgreementFixRecipient2;
		OrgCommissionAgreementRecipientRate AgreementFixRecipient2Rate;
		CommissionAgreementAndRatesForTesting CommissionAgreementAndRatesForTesting;

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			SetupTestData();
		}

		RecipientRatePair RecipientRatePair => recipientRatePair ?? (recipientRatePair = CommissionTestObjectCreator.GetNewRecipientRatePair(Factory));
		RecipientRatePair recipientRatePair;

		OrgHeader Org1;
		OrgHeader Org2;
		Job Job1;
		Job Job2;
		InvoicingBase Invoice;
		InvoicingLineBase Job1Line1;
		InvoicingLineBase Job2Line1;
		InvoicingLineBase Job2Line2;

		void SetupTransactionsAndCommissions(bool createInvoiceOnJob = true, AccChargeCode defaultChargeCode = null)
		{
			Org1 = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = Org1.Addresses.AddNew();
			address1.OA_Address1 = "72 O'Riordan St";

			Org2 = Factory.NewWithValidTestData<OrgHeader>();
			var address2 = Org2.Addresses.AddNew();
			address2.OA_Address1 = "27 O'Riordan St";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ACL";

			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();

			var commissionAgreement1 = opportunity1.ApprovedCommissionAgreements.AddNew();
			commissionAgreement1.CA0_OH_Customer = Org1.PK;
			commissionAgreement1.FillWithValidTestData();
			commissionAgreement1.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			commissionAgreement1.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			commissionAgreement1.CA0_EffectiveDate = ZDate.Today;

			var item1 = commissionAgreement1.ProductItems.AddNew(true, "SHP");
			item1.CAI_Type = OrgCommissionAgreementItemTypes.Codes.Product;

			var agreementPctRecipient1 = commissionAgreement1.Recipients.AddNew();
			agreementPctRecipient1.CAR_GS_NKStaff = "ACL";
			agreementPctRecipient1.CAR_CommissionType = CommissionTypes.Codes.PCT;
			agreementPctRecipient1.CAR_Share = 1;

			var agreementPctRecipient1Rate = agreementPctRecipient1.Rates.AddNew();
			agreementPctRecipient1Rate.CAT_CommissionPercentage = 10;

			var shipment1 = TestObjectCreator.CreateShipment("1001");
			var shipment2 = TestObjectCreator.CreateShipment("1002");

			Job1 = TestObjectCreator.CreateJob(shipment1);
			Job1.JH_OA_LocalChargesAddr = address1.PK;
			AssertEquals("Precondition", Org1, Job1.LocalCharges);

			Job2 = TestObjectCreator.CreateJob(shipment2);
			Job2.JH_OA_LocalChargesAddr = address1.PK;
			AssertEquals("Precondition", Org1, Job2.LocalCharges);

			Invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1);
			Invoice.AH_InvoiceAmount = 1000;
			Invoice.AH_OH = Org1.PK;

			TestObjectCreator.CC1.AC_IsCommissionable = true;
			TestObjectCreator.CC2.AC_IsCommissionable = false;

			Job1Line1 = TestObjectCreator.CreateInvoiceLine(Invoice, TestObjectCreator.AUD, 1, 100, 10, 0);
			Job1Line1.AL_LineType = TransactionLineTypes.Revenue;
			Job1Line1.AL_JH = createInvoiceOnJob ? Job1.PK : ZGuid.Empty;
			Job1Line1.AL_AC = defaultChargeCode != null ? defaultChargeCode.PK : TestObjectCreator.CC1.PK;

			Job2Line1 = TestObjectCreator.CreateInvoiceLine(Invoice, TestObjectCreator.AUD, 1, 200, 10, 0);
			Job2Line1.AL_LineType = TransactionLineTypes.Revenue;
			Job2Line1.AL_JH = createInvoiceOnJob ? Job2.PK : ZGuid.Empty;
			Job2Line1.AL_AC = defaultChargeCode != null ? defaultChargeCode.PK : TestObjectCreator.CC1.PK;

			Job2Line2 = TestObjectCreator.CreateInvoiceLine(Invoice, TestObjectCreator.AUD, 1, 300, 10, 0);
			Job2Line2.AL_LineType = TransactionLineTypes.Revenue;
			Job2Line2.AL_JH = createInvoiceOnJob ? Job2.PK : ZGuid.Empty;
			Job2Line2.AL_AC = defaultChargeCode != null ? defaultChargeCode.PK : TestObjectCreator.CC2.PK;

			if (createInvoiceOnJob)
			{
				TestObjectCreator.CreateCharge(Job1Line1);
				TestObjectCreator.CreateCharge(Job2Line1);
				TestObjectCreator.CreateCharge(Job2Line2);
			}

			Factory.Save();
		}

		#endregion
	}

	#region Classes

	class CommissionLinesHelperForTesting : CommissionLinesHelper
	{
		public void AddPercentageCommissionLines_ExposedForTesting(AccCommissionLineGroup commissionLineGroup, ICommissionAgreementAndRates agreementAndRates)
		{
			AddPercentageCommissionLines(commissionLineGroup, agreementAndRates);
		}

		public void CreateFixedCommissionLines_ExposedForTesting(AccCommissionHeader commissionHeader, ICommissionAgreementAndRates agreementAndRates)
		{
			CreateFixedCommissionLines(commissionHeader, agreementAndRates);
		}
	}

	#endregion
}
