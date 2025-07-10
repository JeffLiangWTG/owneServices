using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(CusEntryLineFee))]
	class CusEntryLineFeeTest : Customs.Business.Testing.CusEntryLineFeeTest
	{
		public void TestTypeDecider()
		{
			Assert("Update Customs.Business.CusEntryLineFee to include a decider for this class", Factory.New(typeof(Customs.Business.CusEntryLineFee)).GetType() == GetExpectedBusinessObjectType());
		}

		public void TestChargeTypeDescription()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Brazil, "TT1", "Test Description");
			ReferenceTestDataHelper.CreateReferenceDataForILFFeeTypeList(Factory);

			Factory.Save();

			var cusFee = Factory.New<CusEntryLineFee>();

			cusFee.CF_ChargeType = "TT1";
			AssertEquals("Test Description", cusFee.ChargeTypeDescription);

			cusFee.CF_ChargeType = "F1ND";
			AssertEquals("Import License fine without discount for goods shipped before the approval date", cusFee.ChargeTypeDescription);

			cusFee.CF_ChargeType = "F1D5";
			AssertEquals("Import License fine with 50% discount for goods shipped before the approval date", cusFee.ChargeTypeDescription);
		}

		public void TestResetData()
		{
			var line = Factory.New<CusEntryLine>();

			var fee = line.Fees.AddNew();
			fee.CF_ChargeAmount = 30m;
			fee.CF_BaseValue = 20m;
			fee.CF_Rate = 10m;
			fee.CF_MethodOfCalculation = "TT";

			line.ResetTotalsAndCachedValues();

			CombineAssertions(() =>
			{
				Assert("CF_ChargeAmount must be Zero", fee.CF_ChargeAmount.IsEmpty);
				Assert("CF_BaseValue must be Zero", fee.CF_BaseValue.IsEmpty);
				Assert("CF_Rate must be Zero", fee.CF_Rate.IsEmpty);
				Assert("CF_MethodOfCalculation must be Empty", fee.CF_MethodOfCalculation.IsEmpty);
			});
		}

		public void TestIsEmpty()
		{
			var line = Factory.New<CusEntryLine>();

			var fee = line.Fees.AddNew();
			fee.CF_ChargeAmount = 30m;
			fee.CF_BaseValue = 20m;
			fee.CF_Rate = 10m;
			fee.CF_MethodOfCalculation = "TT";
			Assert("IsEmpty", !fee.IsEmpty);

			fee.CF_ChargeAmount = 0m;
			Assert("IsEmpty", !fee.IsEmpty);
			fee.CF_BaseValue = 0m;
			Assert("IsEmpty", !fee.IsEmpty);
			fee.CF_Rate = 0m;
			Assert("IsEmpty", fee.IsEmpty);
		}

		public void TestIsQuantityPerUnit()
		{
			var line = Factory.New<CusEntryLine>();

			var fee = line.Fees.AddNew();
			fee.CF_ChargeAmount = 30m;
			fee.CF_BaseValue = 20m;
			fee.CF_Rate = 10m;
			fee.CF_MethodOfCalculation = "TT";
			Assert("IsQuantityPerUnit", !fee.IsQuantityPerUnit);

			fee.CF_MethodOfCalculation = "QPU";
			Assert("IsQuantityPerUnit", fee.IsQuantityPerUnit);

			fee.CF_MethodOfCalculation = string.Empty;
			Assert("IsQuantityPerUnit", !fee.IsQuantityPerUnit);
		}

		public void TestShouldDeleteIfChargeAmountIsZero()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var line = declaration.CustomsEntryHeaders.AddNew().AllEntryLines.AddNew();

			var fee1 = line.Fees.AddNew();
			AssertShouldDeleteIfChargeAmountIsZero(fee1, true);

			var fee2 = line.Fees.AddNew();
			fee2.CF_BaseValue = 10;
			AssertShouldDeleteIfChargeAmountIsZero(fee2, false);

			var fee3 = line.Fees.AddNew();
			fee3.CF_Rate = 10;
			AssertShouldDeleteIfChargeAmountIsZero(fee3, false);

			void AssertShouldDeleteIfChargeAmountIsZero(CusEntryLineFee fee, bool shouldDelete)
			{
				CombineAssertions($"CF_BaseValue={fee.CF_BaseValue}, CF_Rate={fee.CF_Rate}", () =>
				{
					fee.CF_ChargeAmount = 0m;
					Factory.Save();

					AssertEquals($"CF_ChargeAmount=0, IsInDatabase", !shouldDelete, fee.IsInDatabase);

					fee.CF_ChargeAmount = 10m;
					Factory.Save();

					AssertEquals($"CF_ChargeAmount=10, IsInDatabase", true, fee.IsInDatabase);
					AssertEquals($"CF_ChargeAmount=10, IsDeleted", false, fee.IsDeleted);

					fee.CF_ChargeAmount = 0m;
					Factory.Save();

					AssertEquals($"CF_ChargeAmount=0, IsInDatabase", !shouldDelete, fee.IsInDatabase);
					AssertEquals($"CF_ChargeAmount=0, IsDeleted", shouldDelete, fee.IsDeleted);
				});
			}
		}

		public void TestFormalEntryLineNumbers()
		{
			var invoiceLine1 = Factory.New<JobComInvoiceLine>();
			var invoiceLine2 = Factory.New<JobComInvoiceLine>();
			var invoiceLine3 = Factory.New<JobComInvoiceLine>();
			var invoiceLine4 = Factory.New<JobComInvoiceLine>();
			var invoiceLine5 = Factory.New<JobComInvoiceLine>();

			var entryLine1 = Factory.New<CusEntryLine>();
			entryLine1.CL_LineNumber = 1;
			entryLine1.InvoiceLines.Add(invoiceLine1);
			invoiceLine1.JI_CL = entryLine1.PK;
			var fee1 = entryLine1.Fees.AddNew();

			var entryLine2 = Factory.New<CusEntryLine>();
			entryLine2.CL_LineNumber = 3;
			entryLine2.InvoiceLines.Add(invoiceLine2);
			invoiceLine2.JI_CL = entryLine2.PK;
			var fee2 = entryLine2.Fees.AddNew();

			var entryLine3 = Factory.New<CusEntryLine>();
			entryLine3.CL_LineNumber = 2;
			entryLine3.InvoiceLines.Add(invoiceLine3);
			invoiceLine3.JI_CL = entryLine3.PK;
			var fee3 = entryLine3.Fees.AddNew();

			invoiceLine4.JI_CL = entryLine3.PK;

			CombineAssertions(() =>
			{
				AssertEquals("Fee1 FormalEntryLineNumbers should be", "1", fee1.FormalEntryLineNumbers);
				AssertEquals("Fee2 FormalEntryLineNumbers should be", "3", fee2.FormalEntryLineNumbers);
				AssertEquals("Fee3 FormalEntryLineNumbers should be", "2", fee3.FormalEntryLineNumbers);
			});

			var entryLineAll = Factory.New<CusEntryLine>();
			entryLineAll.InvoiceLines.Add(invoiceLine1);
			entryLineAll.InvoiceLines.Add(invoiceLine2);
			entryLineAll.InvoiceLines.Add(invoiceLine3);
			entryLineAll.InvoiceLines.Add(invoiceLine4);
			entryLineAll.InvoiceLines.Add(invoiceLine5);

			var feeAll = entryLineAll.Fees.AddNew();
			AssertEquals("1, 2, 3", feeAll.FormalEntryLineNumbers);
		}

		protected override int ExpectedCF_BaseValueDecimalPlaces => 4;
	}
}
