using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;
using PermitTransactionCategoryList = Enterprise.Customs.Business.PermitTransactionCategoryList;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CusPermitHeader))]
public class CusPermitHeaderTest : Customs.Business.Testing.BaseCusPermitHeaderTest
{
	public void TestAllowNewLineTransactions()
	{
		var permit = Factory.NewWithValidTestData<CusPermitHeader>();

		CombineAssertions(() =>
		{
			Assert("IsTransactionsApplicable", PermitHeader.IsTransactionsApplicable());

			var transactionList = (IBindingList)permit.CusPermitLineTransactions;

			permit.TransactionCategory = ZString.Empty;
			AssertEquals("Don't allow new transactions when category is empty", false, transactionList.AllowNew);

			permit.TransactionCategory = PermitTransactionCategoryList.Codes.CUM;
			AssertEquals("Allow new transactions only when qty/val is not empty and category is CUM.", true, transactionList.AllowNew);

			permit.TransactionCategory = PermitTransactionCategoryList.Codes.VAL;
			AssertEquals("Don't allow new transactions when category is VAL", false, ((IBindingList)permit.CusPermitLineTransactions).AllowNew);

			permit.CPH_QtyValIndicator = ZString.Empty;
			permit.TransactionCategory = PermitTransactionCategoryList.Codes.CUM;
			AssertEquals("Allow new transactions only when qty/val is not empty and categary is CUM.", false, transactionList.AllowNew);
		});
	}

	public void TestPermitCountrySpecificInstructionShouldBeOfCHType()
	{
		var permit = Factory.New<CusPermitHeader>();
		AssertType<PermitCountrySpecificInstruction>(permit.CountrySpecificInstruction);
	}

	public void TestCheckPropertyMaxLength()
	{
		var permit = Factory.New<CusPermitHeader>();
		AssertEquals(3, permit.CPH_TypeInfo.MaxLength);
	}

	public void TestValidationType()
	{
		var permit = Factory.New<BaseCusPermitHeader>();
		AssertType<CusPermitHeaderValidation>(permit.Validation);
	}

	public void TestLookupsType()
	{
		var permit = Factory.New<BaseCusPermitHeader>();
		AssertType<CusPermitHeaderLookups>(permit.Lookups);
	}

	public void TestCountrySpecificInstructionType()
	{
		var permit = Factory.New<BaseCusPermitHeader>();
		AssertType<PermitCountrySpecificInstruction>(permit.CountrySpecificInstruction);
	}
}
