using Enterprise.Customs.NZ.Business.Declaration.FormalEntry;
using NUnit.Framework;
using CusEntryLine = Enterprise.Customs.NZ.Business.Declaration.CusEntryLine;
using JobDeclaration = Enterprise.Customs.NZ.Business.Declaration.JobDeclaration;

namespace Enterprise.DocumentWrappers.Customs.NZ.FormalEntry.Testing
{
	[TestedType(typeof(DocCusEntryHeader))]
	sealed class DocCusEntryHeaderTest : Base.Testing.DocBaseCusEntryHeaderAbstractTest<CusEntryHeader, DocCusEntryHeader>
	{
		#region TestTotalLines
		public void TestTotalLines()
		{
			EntryHeaderInternal.MergedLines.AddNew();
			EntryHeaderInternal.MergedLines.AddNew();
			DocCusEntryHeader docEntryHeader = CreateEntryHeaderWrapper(EntryHeaderInternal);
			AssertEquals("DocEntryHeader.TotalLines", 2, docEntryHeader.TotalLines);
		}
		#endregion

		#region TestTotalDuty
		public void TestTotalDuty()
		{
			CusEntryLine entryLine = EntryHeaderInternal.MergedLines.AddNew();
			entryLine.DutyAmount = 12.3m;
			DocCusEntryHeader docEntryHeader = CreateEntryHeaderWrapper(EntryHeaderInternal);
			AssertEquals("DocEntryHeader.TotalDuty", "$12.30", docEntryHeader.TotalDuty);
		}
		#endregion

		#region TestTotalGST
		public void TestTotalGST()
		{
			CusEntryLine entryLine = EntryHeaderInternal.MergedLines.AddNew();
			entryLine.GSTAmount = 12.3m;
			DocCusEntryHeader docEntryHeader = CreateEntryHeaderWrapper(EntryHeaderInternal);
			AssertEquals("DocEntryHeader.TotalGST", "$12.30", docEntryHeader.TotalGST);
		}
		#endregion

		#region TestTotalMisc
		public void TestTotalLevies()
		{
			CusEntryLine entryLine = EntryHeaderInternal.MergedLines.AddNew();
			entryLine.ALACLevyAmount = 12.3m;
			DocCusEntryHeader docEntryHeader = CreateEntryHeaderWrapper(EntryHeaderInternal);
			AssertEquals("DocEntryHeader.TotalLevies", "$12.30", docEntryHeader.TotalLevies);
		}
		#endregion

		#region TestEntryFee
		public void TestEntryFee()
		{
			EntryHeaderInternal.EntryFeeAmount = 11m;
			EntryHeaderInternal.EntryFeeGST = 1.1m;
			DocCusEntryHeader docEntryHeader = CreateEntryHeaderWrapper(EntryHeaderInternal);
			AssertEquals("DocEntryHeader.EntryFee", "$12.10", docEntryHeader.EntryFee);
		}

		public void TestEntryFeeExGST()
		{
			EntryHeaderInternal.EntryFeeAmount = 73.3m;
			DocCusEntryHeader docEntryHeader = CreateEntryHeaderWrapper(EntryHeaderInternal);
			AssertEquals("DocEntryHeader.EntryFeeExGST", "$73.30", docEntryHeader.EntryFeeExGST);
		}

		public void TestEntryFeeGST()
		{
			EntryHeaderInternal.EntryFeeGST = 13.5m;
			DocCusEntryHeader docEntryHeader = CreateEntryHeaderWrapper(EntryHeaderInternal);
			AssertEquals("DocEntryHeader.EntryFeeGST", "$13.50", docEntryHeader.EntryFeeGST);
		}

		#endregion

		#region TestTotalPayableNZD
		public void TestTotalPayableNZD()
		{
			CusEntryLine entryLine = EntryHeaderInternal.MergedLines.AddNew();
			entryLine.DutyAmount = 12.3m;
			entryLine.GSTAmount = 12.3m;
			entryLine.ACCFuelLevyAmount = 12.3m;
			DocCusEntryHeader docEntryHeader = CreateEntryHeaderWrapper(EntryHeaderInternal);
			AssertEquals("DocEntryHeader.TotalPayableNZD", "$36.90", docEntryHeader.TotalPayableNZD);
		}
		#endregion

		#region TestPackages
		public override void TestPackages()
		{
			DocCusEntryHeader docCusEntryHeader = CreateEntryHeaderWrapper(EntryHeaderInternal);
			AssertEquals("Packages", EntryHeaderInternal.PackagesCount, docCusEntryHeader.Packages);
		}
		#endregion

		public void TestGrandTotalAmountAndDescriptionForPayment()
		{
			Declaration.JE_MessageType = Enterprise.Customs.NZ.Business.JobMessageTypeList.Codes.Import;
			CusEntryLine entryLine = EntryHeaderInternal.MergedLines.AddNew();
			entryLine.DutyAmount = 100.00m;
			EntryHeaderInternal.EntryFeeAmount = 12.22m;
			DocCusEntryHeader docEntryHeader = CreateEntryHeaderWrapper(EntryHeaderInternal);
			AssertEquals("DocEntryHeader.GrandTotalAmount", "$112.22", docEntryHeader.GrandTotalAmount);
			AssertEquals("DocEntryHeader.GrandTotalDescription", DocCusEntryHeader.PaymentDescription, docEntryHeader.GrandTotalDescription);
		}

		public void TestGrandTotalAmountAndDescriptionForRefund()
		{
			Declaration.JE_MessageType = Enterprise.Customs.NZ.Business.JobMessageTypeList.Codes.Export;
			CusEntryLine entryLine = EntryHeaderInternal.MergedLines.AddNew();
			entryLine.DutyCreditAmount = 100.00m;
			EntryHeaderInternal.EntryFeeAmount = 12.22m;
			DocCusEntryHeader docEntryHeader = CreateEntryHeaderWrapper(EntryHeaderInternal);
			AssertEquals("DocEntryHeader.GrandTotalAmount", "$87.78", docEntryHeader.GrandTotalAmount);
			AssertEquals("DocEntryHeader.GrandTotalDescription", DocCusEntryHeader.RefundDescription, docEntryHeader.GrandTotalDescription);
		}

		public void TestGrandTotalAmountAndDescriptionForDepositRefund()
		{
			Declaration.JE_MessageType = Enterprise.Customs.NZ.Business.JobMessageTypeList.Codes.Export;
			CusEntryLine entryLine = EntryHeaderInternal.MergedLines.AddNew();
			EntryHeaderInternal.EntryFeeAmount = 5.75m;
			entryLine.DepositRefundAmount = 19296.87m;
			DocCusEntryHeader docEntryHeader = CreateEntryHeaderWrapper(EntryHeaderInternal);
			AssertEquals("DocEntryHeader.GrandTotalAmount", "$19291.12", docEntryHeader.GrandTotalAmount);
			AssertEquals("DocEntryHeader.GrandTotalDescription", DocCusEntryHeader.RefundDescription, docEntryHeader.GrandTotalDescription);
		}

		#region Implementation

		#region Declaration
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = Enterprise.Customs.NZ.Business.JobMessageTypeList.Codes.Import;
					declaration.JE_MessageSubType = Enterprise.Customs.NZ.Business.JobMessageSubTypeList.Codes.Normal;
				}
				return declaration;
			}
		}
		JobDeclaration declaration;
		#endregion

		protected override CusEntryHeader GetNewEntryHeader()
		{
			return (CusEntryHeader)Declaration.CustomsEntryHeaders.AddNew();
		}

		protected override DocCusEntryHeader CreateEntryHeaderWrapper(CusEntryHeader entryHeader)
		{
			return DocCusEntryHeader.New(entryHeader, Factory);
		}

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.NewZealand; }
		}

		#endregion
	}
}
