using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocCashAdvanceRequestLine))]
	sealed class DocCashAdvanceRequestLineTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocCashAdvanceRequestLine.New(Line, Factory), };
		}

		public void TestAllProperties()
		{
			Line.CAL_OSAmount = 100m;
			Line.CAL_OSPaidAmount = 80m;
			Line.CAL_LocalAmount = 50m;
			Line.CAL_LocalPaidAmount = 40m;
			Line.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
			var header = Factory.NewWithValidTestData<AccCashAdvanceRequestHeader>();
			header.CAH_Ledger = LedgerTypes.AccountsReceivable;
			Line.CAL_CAH_RequestHeader = header.PK;
			var charge = Factory.NewWithValidTestData<JobCharge>();
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "CC1";
			chargeCode.AC_Desc = "CC1 Desc";
			charge.JR_AC = chargeCode.PK;
			charge.JR_CAL_ARLine = Line.PK;
			AssertEquals("OSAmount", 100m, LineWrapper.OSAmount);
			AssertEquals("OSPaidAmount", 80m, LineWrapper.OSPaidAmount);
			AssertEquals("LocalAmount", 50m, LineWrapper.LocalAmount);
			AssertEquals("LocalPaidAmount", 40m, LineWrapper.LocalPaidAmount);
			AssertEquals("Status", "PAI", LineWrapper.Status);
			AssertEquals("Description", "CC1 Desc", LineWrapper.Description);
		}

		AccCashAdvanceRequestLine Line;
		DocCashAdvanceRequestLine LineWrapper;

		AccCashAdvanceRequestLine GetLine()
		{
			return Factory.New<AccCashAdvanceRequestLine>();
		}

		DocCashAdvanceRequestLine GetLineWrapper()
		{
			return (DocCashAdvanceRequestLine)GetDocumentWrappers()[0];
		}

		protected override void SetUp()
		{
			Line = GetLine();
			LineWrapper = GetLineWrapper();
			AssertNotNull(LineWrapper);
			base.SetUp();
		}
	}
}
