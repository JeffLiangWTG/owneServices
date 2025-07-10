using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryLineConfirmedFeeWrapper))]
	class CusEntryLineConfirmedFeeWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMethodOfPaymentDescription()
		{
			SetUpCodeLit();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			var entryLineFee = entryLine.Fees.AddNew();
			entryLineFee.CF_MethodOfPayment = "A";
			var cusEntryLineFeeWrapper = new CusEntryLineConfirmedFeeWrapper(entryLineFee);

			AssertEquals(entryLineFee.MethodOfPaymentDescription, cusEntryLineFeeWrapper.MethodOfPaymentDescription);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CusEntryLineConfirmedFeeWrapper(Factory.New<CusEntryLineFee>());
		}

		protected void SetUpCodeLit()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CCIMethodOfPayment, "CCI Method of Payment");
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CCIMethodOfPayment, "A", "Payment in cash", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		}
	}
}
