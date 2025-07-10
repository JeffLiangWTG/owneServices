using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	sealed class CusEntryLineFeeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMethodOfPaymentList()
		{
			SetUpRefData();
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			var lineFee = entryLine.Fees.AddNew();

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = "IMP";
				lineFee.CF_ChargeType = "B00";
				AssertEquals("When MsgType = IMP, Charge Type = B00, Method Of Payment ElementsAsString", "123 - Method Of Payment For Test\r\nDEF - VAT Deferred\r\nNBL - Non-billable Tax", lineFee.Lookups.MethodOfPaymentList.ElementsAsString);

				lineFee.CF_ChargeType = "A00";
				AssertEquals("When MsgType = IMP, Charge Type = A00, Method Of Payment ElementsAsString", "123 - Method Of Payment For Test\r\nNBL - Non-billable Tax", lineFee.Lookups.MethodOfPaymentList.ElementsAsString);

				declaration.JE_MessageType = "EXP";
				lineFee.CF_ChargeType = "B00";
				AssertEquals("When MsgType = EXP, Charge Type = B00, IsUCC6 should be true", true, declaration.IsUCC6);
				AssertEquals("When MsgType = EXP, Charge Type = B00, IsUCC6 will be true, then MethodOfPayment code list should be 104IM type", "A - Payment in cash", lineFee.Lookups.MethodOfPaymentList.ElementsAsString);
			});
		}

		void SetUpRefData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "Method of Payment");

			var methodOfPayment = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "123", "Method Of Payment For Test", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(methodOfPayment.PK, RefCusCodeListAttributeTypes.Codes.Category, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
			helper.CreateNewOrGetExistingCusCodeListAttribute(methodOfPayment.PK, RefCusCodeListAttributeTypes.Codes.Import, ZString.Empty);
			helper.CreateNewOrGetExistingCusCodeListAttribute(methodOfPayment.PK, RefCusCodeListAttributeTypes.Codes.Export, ZString.Empty);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CCIMethodOfPayment, "CCI Method of Payment");
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CCIMethodOfPayment, "A", "Payment in cash", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		}
	}
}
