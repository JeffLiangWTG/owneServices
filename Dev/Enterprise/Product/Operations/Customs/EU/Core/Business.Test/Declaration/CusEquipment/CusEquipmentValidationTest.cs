using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class CusEquipmentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestParent()
		{
			CusEquipment parent = Factory.New<CusEquipment>();
			AssertEquals(parent.Validation.Parent, parent);
		}

		public void TestCheckCEQ_IdentificationNumber()
		{
			var warning = "An Equipment should be linked to at least one package line. Please go to the Packaging tab -> sub tab Packing Details and select appropriate Equipments(s)";
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invLine = invoice.InvoiceLines.AddNew();
				var package = declaration.Packages.AddNew();
				var equipment = declaration.Equipments.AddNew();

				equipment.Validation.ValidateCEQ_IdentificationNumber();
				AssertNoMessageErrorContaining(equipment.CEQ_IdentificationNumberInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoWarning("Container No. - no warning when equipments not required", equipment.CEQ_IdentificationNumberInfo, warning);

				equipment.CEQ_IdentificationNumber = "EQ001";
				package.CW_ContainerNoOrEquipmentNo = equipment.CEQ_IdentificationNumber;
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				equipment.Validation.ValidateCEQ_IdentificationNumber();
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(equipment.CEQ_IdentificationNumberInfo);
				AssertHasWarning("Container No. has correct warning", equipment.CEQ_IdentificationNumberInfo, warning);

				var piv = invLine.PackagesPivot.AddNew();
				piv.CHC_CW = package.PK;
				equipment.Validation.ValidateCEQ_IdentificationNumber();
				AssertNoWarning("Container No. - no warning when package linked", equipment.CEQ_IdentificationNumberInfo, warning);
			}
		}
	}
}
