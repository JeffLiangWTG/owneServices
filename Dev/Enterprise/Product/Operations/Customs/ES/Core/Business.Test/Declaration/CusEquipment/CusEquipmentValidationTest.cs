using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	class CusEquipmentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCEQ_IdentificationNumber()
		{
			CombineAssertions(() =>
			{
				var messageError = "If equipment is used, seals are mandatory. If equipment has not seals, it must not be included in the declaration.";

				var declaration = Factory.New<JobDeclaration>();
				var equipment = declaration.Equipments.AddNew();

				equipment.CEQ_IdentificationNumber = ZString.Empty;
				AssertNoMessageErrorContaining(equipment.CEQ_IdentificationNumberInfo, messageError);

				equipment.CEQ_IdentificationNumber = "AH3";
				AssertHasMessageErrorContaining(equipment.CEQ_IdentificationNumberInfo, messageError);

				equipment.Seals.AddNew();
				AssertNoMessageErrorContaining(equipment.CEQ_IdentificationNumberInfo, messageError);
			});
		}
	}
}
