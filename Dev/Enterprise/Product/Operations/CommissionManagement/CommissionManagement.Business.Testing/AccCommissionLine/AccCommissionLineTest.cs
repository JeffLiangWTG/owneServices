using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(AccCommissionLine))]
	internal class AccCommissionLineTest : EnterpriseBusinessObjectTestCase
	{
		#region Override

		[TestDate(2000, 1, 1)]
		public void TestMarkAsOverriden()
		{
			var commissionHeader = Factory.New<AccCommissionHeader>();
			var commissionLine = commissionHeader.Lines.AddNew();
			commissionLine.CL0_ApprovedDateTimeUtc = new ZDateTime(2000, 1, 1);
			commissionLine.CL0_CommissionType = CommissionTypes.Codes.PCT;
			commissionLine.CL0_EntityPercentage = 50;
			commissionLine.CL0_EntityCommissionAmount = 100;
			commissionLine.CL0_GS_NKStaff = "ADL";
			commissionLine.CL0_OH_Party = ZGuid.Empty;
			commissionLine.CL0_PaidDateTimeUtc = new ZDateTime(2000, 2, 2);
			commissionLine.CL0_RX_NKCommissionCurrency = "USD";
			commissionLine.CL0_RX_NKTransactionCurrency = "AUD";
			commissionLine.CL0_ShareCommissionAmount = 200;
			commissionLine.CL0_SharePortion = 2;
			commissionLine.CL0_ShareTotal = 5;
			commissionLine.CL0_TotalCommissionableAmount = 500;
			commissionLine.CL0_TransactionAmount = 750;

			AssertEquals(false, commissionLine.IsOverriden);
			Assert(commissionLine.CL0_ShouldReinstate);

			commissionLine.MarkAsOverriden();

			AssertEquals(true, commissionLine.IsOverriden);
			AssertEquals(new ZDateTime(2000, 1, 1), commissionLine.CL0_OverridenDateTimeUtc);

			var newLines = commissionHeader.Lines.Where(x => x.PK != commissionLine.PK).ToArray();
			AssertEquals("Should have created override line", 1, newLines.Length);
			var overrideLine = newLines[0];

			AssertEquals(ZDateTime.Empty, overrideLine.CL0_ApprovedDateTimeUtc);
			AssertEquals(commissionLine.PK, overrideLine.CL0_BelongsToGroup);
			AssertEquals(new ZDateTime(2000, 1, 1), overrideLine.CL0_OverridenDateTimeUtc);
			AssertEquals(commissionLine.CL0_CAT, overrideLine.CL0_CAT);
			AssertEquals(CommissionTypes.Codes.PCT, overrideLine.CL0_CommissionType);
			AssertEquals((ZDecimal)(50), overrideLine.CL0_EntityPercentage);
			AssertEquals((ZDecimal)(-100), overrideLine.CL0_EntityCommissionAmount);
			AssertEquals("ADL", overrideLine.CL0_GS_NKStaff);
			AssertEquals(ZGuid.Empty, overrideLine.CL0_OH_Party);
			AssertEquals(ZDateTime.Empty, overrideLine.CL0_PaidDateTimeUtc);
			AssertEquals(ZDateTime.Empty, overrideLine.CL0_CancelledDateTimeUtc);
			AssertEquals(commissionHeader.PK, overrideLine.CL0_ParentID);
			AssertEquals(commissionHeader.TablePrefix, overrideLine.CL0_ParentTableCode);
			AssertEquals("USD", overrideLine.CL0_RX_NKCommissionCurrency);
			AssertEquals("AUD", overrideLine.CL0_RX_NKTransactionCurrency);
			AssertEquals((ZDecimal)(-200), overrideLine.CL0_ShareCommissionAmount);
			AssertEquals((ZByte)2, overrideLine.CL0_SharePortion);
			AssertEquals((ZByte)5, overrideLine.CL0_ShareTotal);
			AssertEquals((ZDecimal)(-500), overrideLine.CL0_TotalCommissionableAmount);
			AssertEquals((ZDecimal)(-750), overrideLine.CL0_TransactionAmount);
			AssertEquals(true, overrideLine.CL0_ShouldReinstate);
			AssertEquals(true, commissionLine.CL0_ShouldReinstate);
		}

		[TestDate(2000, 1, 1)]
		public void TestMarkAsOverriden_CancelOriginalAndNewLineIfOriginalLineNotPaidYet()
		{
			var commissionHeader = Factory.New<AccCommissionHeader>();
			var commissionLine = commissionHeader.Lines.AddNew();
			commissionLine.CL0_PaidDateTimeUtc = ZDateTime.Empty;

			AssertEquals(false, commissionLine.IsOverriden);

			commissionLine.MarkAsOverriden();

			AssertEquals(true, commissionLine.IsOverriden);
			AssertEquals(new ZDateTime(2000, 1, 1), commissionLine.CL0_OverridenDateTimeUtc);
			AssertEquals(new ZDateTime(2000, 1, 1), commissionLine.CL0_CancelledDateTimeUtc);

			var newLines = commissionHeader.Lines.Where(x => x.PK != commissionLine.PK).ToArray();
			AssertEquals("Should have created override line", 1, newLines.Length);
			var overrideLine = newLines[0];

			AssertEquals(new ZDateTime(2000, 1, 1), overrideLine.CL0_OverridenDateTimeUtc);
			AssertEquals(new ZDateTime(2000, 1, 1), overrideLine.CL0_CancelledDateTimeUtc);
		}

		public void TestCommissionHeader()
		{
			var commissionHeader = Factory.New<AccCommissionHeader>();
			var commissionLine = commissionHeader.Lines.AddNew();

			AssertEquals("CL0_ParentTableCode should be CH0", AccCommissionHeaderSchema.Constants.Prefix, commissionLine.CL0_ParentTableCode);

			AssertNull(commissionLine.LineGroup);

			var header = commissionLine.CommissionHeader;
			AssertNotNull(header);

			AssertEquals(header, commissionHeader);
		}

		public void TestLineGroup()
		{
			var commissionHeader = Factory.New<AccCommissionHeader>();
			var commissionLineGroup = commissionHeader.LineGroups.AddNew();
			var commissionLine = commissionLineGroup.Lines.AddNew();

			AssertEquals("CL0_ParentTableCode should be CLG", AccCommissionLineGroupSchema.Constants.Prefix, commissionLine.CL0_ParentTableCode);

			AssertNull(commissionLine.CommissionHeader);

			var lineGroup = commissionLine.LineGroup;
			AssertNotNull(lineGroup);

			AssertEquals(lineGroup, commissionLineGroup);
		}

		public void TestShouldCreateReversalLine()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var commissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();

			var commissionLineGroup = commissionHeader.LineGroups.AddNew();
			commissionLineGroup.CLG_AC = chargeCode.PK;

			var commissionLine = commissionLineGroup.Lines.AddNew();
			Assert("Pre-condition", commissionLine.CL0_ShouldReinstate);

			var lines = Factory.Load<AccCommissionLine>(new ZQuery());
			AssertEquals("Pre-Condition", 1, lines.Length);

			commissionLine.MarkAsOverriden();
			Factory.Save();

			AssertNotEquals("CL0_OverridenDateTimeUtc should have been set", ZDateTime.Empty, commissionLine.CL0_OverridenDateTimeUtc);

			lines = Factory.Load<AccCommissionLine>(new ZQuery());
			AssertEquals("Reversal line should have been created", 2, lines.Length);
			AssertEquals("MarkAsOverriden() should NOT have set original line CL0_ShouldReinstate to false", true, commissionLine.CL0_ShouldReinstate);

			commissionLine.CL0_OverridenDateTimeUtc = ZDateTime.Empty;
			commissionLine.CL0_CancelledDateTimeUtc = ZDateTime.Empty;
			commissionLine.CL0_ShouldReinstate = false;
			commissionLine.MarkAsOverriden();

			lines = Factory.Load<AccCommissionLine>(new ZQuery());
			AssertEquals("Reversal line should not have been created", 2, lines.Length);
		}

		public void TestPopulateReversalLine()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var commissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();

			var commissionLineGroup = commissionHeader.LineGroups.AddNew();
			commissionLineGroup.CLG_AC = chargeCode.PK;

			var originalLine = commissionLineGroup.Lines.AddNew();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Test";
			staff.GS_Code = "TST";

			originalLine.CL0_GS_NKStaff = staff.GS_Code;
			originalLine.CL0_RX_NKTransactionCurrency = "AUD";
			originalLine.CL0_TransactionAmount = 420;
			originalLine.CL0_CommissionType = "PCT";
			originalLine.CL0_RX_NKCommissionCurrency = "AUD";
			originalLine.CL0_TotalCommissionableAmount = 420;
			originalLine.CL0_ShareCommissionAmount = 42.0d;
			originalLine.CL0_EntityPercentage = 10d;
			originalLine.CL0_EntityCommissionAmount = 42.0d;
			originalLine.CL0_OverridenDateTimeUtc = ZDateTime.BrettsBirthday;

			var reversalLine = Factory.New<AccCommissionLine>();

			AccCommissionLine.PopulateReversalLine(reversalLine, originalLine);

			AssertEquals(originalLine.PK, reversalLine.CL0_BelongsToGroup);
			AssertEquals(originalLine.CL0_CAT, reversalLine.CL0_CAT);
			AssertEquals(originalLine.CL0_GS_NKStaff, reversalLine.CL0_GS_NKStaff);
			AssertEquals(originalLine.CL0_OH_Party, reversalLine.CL0_OH_Party);
			AssertEquals(originalLine.CL0_RX_NKTransactionCurrency, reversalLine.CL0_RX_NKTransactionCurrency);
			AssertEquals(-originalLine.CL0_TransactionAmount, reversalLine.CL0_TransactionAmount);
			AssertEquals(originalLine.CL0_CommissionType, reversalLine.CL0_CommissionType);
			AssertEquals(originalLine.CL0_RX_NKCommissionCurrency, reversalLine.CL0_RX_NKCommissionCurrency);
			AssertEquals(-originalLine.CL0_TotalCommissionableAmount, reversalLine.CL0_TotalCommissionableAmount);
			AssertEquals(originalLine.CL0_SharePortion, reversalLine.CL0_SharePortion);
			AssertEquals(originalLine.CL0_ShareTotal, reversalLine.CL0_ShareTotal);
			AssertEquals(-originalLine.CL0_ShareCommissionAmount, reversalLine.CL0_ShareCommissionAmount);
			AssertEquals(originalLine.CL0_EntityPercentage, reversalLine.CL0_EntityPercentage);
			AssertEquals(-originalLine.CL0_EntityCommissionAmount, reversalLine.CL0_EntityCommissionAmount);

			AssertEquals(ZDateTime.Empty, reversalLine.CL0_ApprovedDateTimeUtc);
			AssertEquals(ZDateTime.Empty, reversalLine.CL0_OverridenDateTimeUtc);
			AssertEquals(ZDateTime.Empty, reversalLine.CL0_PaidDateTimeUtc);
			AssertEquals(ZBool.True, reversalLine.CL0_ShouldReinstate);

			Assert(originalLine.IsCancelled);
			Assert(reversalLine.IsCancelled);
		}

		#endregion

		#region Delete

		public void TestDelete()
		{
			var lineA = Factory.New<AccCommissionLine>();
			var lineB = Factory.New<AccCommissionLine>();
			var request = Factory.New<AccCommissionApprovalRequest>();
			var itemA = request.Items.AddNew();
			itemA.CRI_CL0 = lineA.PK;
			var itemB = request.Items.AddNew();
			itemB.CRI_CL0 = lineB.PK;

			lineA.Delete();

			CombineAssertions(() =>
			{
				AssertEquals("itemA", true, itemA.IsDeleted);
				AssertEquals("itemB", false, itemB.IsDeleted);
				AssertEquals("request", false, request.IsDeleted);
			});
		}

		#endregion

		#region Decimals

		public void TestZDecimalsHaveCorrectDecimalPlacesAccCommissionLine()
		{
			var line = Factory.New<AccCommissionLine>();

			var transactionList = new List<string>
			{
				nameof(line.CL0_TransactionAmount)
			};

			var commissionList = new List<string>
			{
				nameof(line.CL0_EntityCommissionAmount),
				nameof(line.CL0_ShareCommissionAmount),
				nameof(line.CL0_TotalCommissionableAmount)
			};

			var percentList = new List<string>
			{
				nameof(line.CL0_EntityPercentage)
			};

			var tester = new DecimalPlacesAttributeTester(line);
			tester.CheckNonLocalCurrency(transactionList, nameof(line.TransactionDecimals), nameof(line.CL0_RX_NKTransactionCurrency), line);
			tester.CheckNonLocalCurrency(commissionList, nameof(line.CommissionDecimals), nameof(line.CL0_RX_NKCommissionCurrency), line);
			tester.CheckConstant(percentList, nameof(line.PercentageDecimals), Core.Constants.DecimalPlaces.DefaultNumberOfDecimalsForPercentages);
		}

		#endregion

	}
}
