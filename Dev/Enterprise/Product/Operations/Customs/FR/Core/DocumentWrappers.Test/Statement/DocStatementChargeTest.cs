using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.CusStatement;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing;

namespace Enterprise.Customs.FR.DocumentWrappers.Statement.Testing;

sealed class DocStatementChargeTest : DocBaseWrapperTest
{
	protected override DocBaseWrapper GetNewDocumentWrapper()
	{
		var header = Factory.New<CusStatementHeader>();
		header.B2_StatementType = StatementPeriodicityList.Codes.Day;
		var charge = header.ChargesDetail.Charges.AddNew();
		return DocStatementCharge.New(charge, Factory);
	}

	public void TestProperties()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "Method of Payment");

		var frImportMOP1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "3", "Cautionné", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeListAttribute(frImportMOP1.PK, RefCusCodeListAttributeTypes.Codes.Category, Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
		helper.CreateNewOrGetExistingCusCodeListAttribute(frImportMOP1.PK, RefCusCodeListAttributeTypes.Codes.Import, ZString.Empty);
		Factory.Save();

		var header = Factory.New<CusStatementHeader>();
		header.B2_StatementType = StatementPeriodicityList.Codes.Day;
		var charge = header.ChargesDetail.Charges.AddNew();
		charge.B4_ChargeAmount = 10m;
		charge.B4_ChargeType = "A325";
		charge.B4_MethodOfPayment = "3";

		var wrapper = DocStatementCharge.New(charge, Factory);

		AssertEquals(10m, wrapper.Amount);
		AssertEquals("A325", wrapper.TaxCode);
		AssertEquals("Cautionné", wrapper.MethodOfPayment);
	}
}
