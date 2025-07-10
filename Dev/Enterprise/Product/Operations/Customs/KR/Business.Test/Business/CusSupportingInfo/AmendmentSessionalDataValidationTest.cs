using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class AmendmentSessionalDataValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckDutyPenaltyCause()
		{
			sessionalData.DutyPenaltyCause = ZString.Empty;
			AssertNoMessageErrors(sessionalData.DutyPenaltyCauseInfo);

			sessionalData.DutyPenaltyCause = "XX";
			AssertHasMessageErrorContaining(sessionalData.DutyPenaltyCauseInfo, ListValidation.InvalidCodeMessageError);

			sessionalData.DutyPenaltyCause = "01";
			AssertNoMessageErrors(sessionalData.DutyPenaltyCauseInfo);
		}

		public void TestCheckTaxPenaltyCause()
		{
			sessionalData.TaxPenaltyCause = ZString.Empty;
			AssertNoMessageErrors(sessionalData.TaxPenaltyCauseInfo);

			sessionalData.TaxPenaltyCause = "XX";
			AssertHasMessageErrorContaining(sessionalData.TaxPenaltyCauseInfo, ListValidation.InvalidCodeMessageError);

			sessionalData.TaxPenaltyCause = "01";
			AssertNoMessageErrors(sessionalData.TaxPenaltyCauseInfo);
		}

		protected override void SetUp()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var amendmentSessionDataCollection = new AmendmentSessionalDataCollection(instruction);
			sessionalData = amendmentSessionDataCollection.AddNew();
		}
		AmendmentSessionalData sessionalData;
	}
}
