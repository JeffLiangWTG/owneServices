using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(CusEquipment))]
	class CusEquipmentTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetNewValidation()
		{
			var cusEquipment = Factory.New<CusEquipment>();
			AssertType<CusEquipmentValidation>(cusEquipment.Validation);
		}

		public void TestCEQ_IdentificationNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var equipment = declaration.Equipments.AddNew();
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(equipment.CEQ_IdentificationNumberInfo, string.Empty, "Identification Number", mediumCaption: "ID Number", shortCaption: "ID", fullDescription: string.Empty);
		}

		public void TestDelete()
		{
			var seal1 = Equipment.Seals.AddNew();
			seal1.BK_SealNumber = "S1";
			var seal2 = Equipment.Seals.AddNew();
			seal2.BK_SealNumber = "S2";

			Equipment.Delete();

			AssertEquals(true, seal1.IsDeleted);
			AssertEquals(true, seal2.IsDeleted);
		}

		public void TestOnSaving()
		{
			var declaration = Factory.New<JobDeclaration>();
			var equipment = declaration.Equipments.AddNew();
			Factory.Save();

			AssertEquals(true, equipment.IsDeleted);

			equipment = declaration.Equipments.AddNew();
			equipment.CEQ_IdentificationNumber = "E1";
			Factory.Save();
			AssertEquals(false, equipment.IsDeleted);
		}

		public void TestSeals()
		{
			AssertType<CusSealCollection>(Equipment.Seals);
		}

		public void TestCusSealType()
		{
			AssertType(((ICusSealTypeSupporter)Equipment).CusSealType, Equipment.Seals.AddNew());
		}

		public void TestShortSequenceNumberGenerator()
		{
			var declaration = Factory.New<JobDeclaration>();
			var equipment = declaration.Equipments.AddNew();

			AssertType<ShortSequenceNumberGenerator>("CusEquipment should have a ShortSequenceNumberGenerator", equipment.SealsSequenceNumberGenerator);
			var seal1 = equipment.Seals.AddNew();
			var seal2 = equipment.Seals.AddNew();

			AssertEquals("Should have set the correct BK_SequenceNumber.", (short)1, seal1.BK_SequenceNumber);
			AssertEquals("Should have set the correct BK_SequenceNumber.", (short)2, seal2.BK_SequenceNumber);
		}

		public void TestCEQ_IdentificationNumber_PackageDefault()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				var equipment1 = declaration.Equipments.AddNew();

				var bill1 = declaration.Bills.AddNew();
				bill1.CU_BillType = BillTypeList.Codes.HouseBill;
				bill1.CU_BillNum = "HB123";
				var packingGroup = bill1.PackingGroups[0];
				var package1 = packingGroup.Packages[0];
				var package2 = packingGroup.Packages.AddNew();

				equipment1.CEQ_IdentificationNumber = "EQ001";
				AssertEquals("One equipment only - package 1", "EQ001", package1.CW_ContainerNoOrEquipmentNo);
				AssertEquals("One equipment only - package 2", "EQ001", package2.CW_ContainerNoOrEquipmentNo);

				package1.CW_ContainerNoOrEquipmentNo = string.Empty;
				var container = declaration.CusContainers.AddNew();
				container.CO_ContainerNumber = "C001";
				equipment1.CEQ_IdentificationNumber = "EQ000";
				AssertEquals("One equipment and one container - package 1 not set", string.Empty, package1.CW_ContainerNoOrEquipmentNo);
				AssertEquals("One equipment and one container - package 2 udpated", "EQ000", package2.CW_ContainerNoOrEquipmentNo);

				package1.CW_ContainerNoOrEquipmentNo = string.Empty;
				container.Delete();
				equipment1 = declaration.Equipments.AddNew();
				var equipment2 = declaration.Equipments.AddNew();
				equipment1.CEQ_IdentificationNumber = "EQ002";
				equipment2.CEQ_IdentificationNumber = "EQ003";
				AssertEquals("Two equipments - package 1 not set", string.Empty, package1.CW_ContainerNoOrEquipmentNo);
				AssertEquals("Two equipments - package 2 not udpated", "EQ000", package2.CW_ContainerNoOrEquipmentNo);
			}

			declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				var bill1 = declaration.Bills.AddNew();
				bill1.CU_BillType = BillTypeList.Codes.HouseBill;
				bill1.CU_BillNum = "HB123";
				var packingGroup = bill1.PackingGroups[0];
				var package = packingGroup.Packages[0];
				var equipment = declaration.Equipments.AddNew();
				equipment.CEQ_IdentificationNumber = "EQ001";
				AssertEquals("Equipments not required. Pack not set", string.Empty, package.CW_ContainerNoOrEquipmentNo);
			}
		}

		CusEquipment Equipment
		{
			get
			{
				if (fEquipment == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					fEquipment = declaration.Equipments.AddNew();
					fEquipment.CEQ_IdentificationNumber = "E1";
				}
				return fEquipment;
			}
		}
		CusEquipment fEquipment;
	}
}
