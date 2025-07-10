using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class AESCommonLineWrapperTest : WrapperHelperTest<AESCommonLineWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if entryLine is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","entryLine"), () => GetWrapper(null));

				var entryLine = Factory.New<CusEntryLine>();
				AssertExceptionThrown("Constructor Throws Exception if InvoiceLines is null", typeof(ArgumentOutOfRangeException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value '0' cannot be less than or equal to 0.","InvoiceLines"), () => GetWrapper(entryLine));
			});
		}

		public void TestSequenceNumber()
		{
			entryLine.CL_LineNumber = 3;
			AssertEquals("Expected filled SequenceNumber", "3", wrapper.SequenceNumber);
		}

		public void TestStatisticalValue()
		{
			entryLine.CL_StatisticalValue = 3.25m;
			AssertEquals("Expected filled StatisticalValue", 3.25m, wrapper.StatisticalValue);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge", true, mergeResult);

			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

			wrapper = GetWrapper(entryLine);
		}
		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusEntryLine entryLine;
		AESCommonLineWrapper wrapper;

		AESCommonLineWrapper GetWrapper(CusEntryLine entryLine) => new AESCommonLineWrapper(entryLine);

		protected override AESCommonLineWrapper GetProvider() => wrapper;
	}
}
