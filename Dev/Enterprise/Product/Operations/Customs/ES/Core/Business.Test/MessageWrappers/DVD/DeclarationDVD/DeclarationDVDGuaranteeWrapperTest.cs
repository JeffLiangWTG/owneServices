using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DeclarationDVDGuaranteeWrapperTest : WrapperHelperTest<DeclarationDVDGuaranteeWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if guarantee is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","guarantee"), () => new DeclarationDVDGuaranteeWrapper(null));
		}

		public void TestGRNReference()
		{
			CombineAssertions(() =>
			{
				guarantee.PW_BondNumber = "reference";
				guarantee.PW_BondType = ZString.Empty;
				AssertEquals("Expected filled GRNReference when Type is empty", "reference", wrapper.GRNReference);

				guarantee.PW_BondType = "1";
				AssertEquals("Expected empty GRNReference when Type is not empty", ZString.Empty, wrapper.GRNReference);
			});
		}

		public void TestNoGRNReference()
		{
			CombineAssertions(() =>
			{
				guarantee.PW_BondNumber = "reference";
				guarantee.PW_BondType = "1";
				AssertEquals("Expected filled NoGRNReference when Type is not empty", "reference", wrapper.NoGRNReference);

				guarantee.PW_BondType = ZString.Empty;
				AssertEquals("Expected empty NoGRNReference when Type is empty", ZString.Empty, wrapper.NoGRNReference);
			});
		}

		public void TestAccessCode()
		{
			AssertEquals("Expected empty AccessCode", ZString.Empty, wrapper.AccessCode);
		}

		public void TestCurrency()
		{
			AssertEquals("Expected empty Currency", ZString.Empty, wrapper.Currency);
		}

		public void TestAmount()
		{
			AssertEquals("Expected empty Amount", ZDecimal.Zero, wrapper.Amount);
		}

		public void TestOffice()
		{
			guarantee.PW_BondFiledPort = "ES009999";
			AssertEquals("Expected filled Office", "ES009999", wrapper.Office);
		}
		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			guarantee = declaration.Guarantees.AddNew();

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();

			wrapper = new DeclarationDVDGuaranteeWrapper(guarantee);
		}

		JobDeclaration declaration;
		ESGuarantee guarantee;
		DeclarationDVDGuaranteeWrapper wrapper;

		protected override DeclarationDVDGuaranteeWrapper GetProvider() => wrapper;
	}
}
