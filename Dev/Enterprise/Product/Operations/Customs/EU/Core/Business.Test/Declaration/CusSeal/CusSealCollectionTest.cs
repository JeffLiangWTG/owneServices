using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(CusSealCollection))]
	class CusSealCollectionTest : ActiveBusinessObjectCollectionTestCase<CusSealCollection>
	{
		public void TestAllowNewDefault()
		{
			var equipment = Factory.New<CusEquipment>();
			var seals = new CusSealCollection(equipment);
			for (int i = 0; i < 99; i++)
			{
				seals.AddNew();
			}
			var bindingList = (IBindingList)seals;
			AssertEquals("When Count is 99", true, bindingList.AllowNew);

			seals.AddNew();
			AssertEquals("When Count is 100", false, bindingList.AllowNew);
		}

		public void TestAllowNew_DependOfParent()
		{
			var equipment = Factory.New<CusEquipment>();
			var moq = new Mock<CusEquipment>(MockBehavior.Default, equipment.Factory, ((INeedRow)equipment).Row)
			{
				CallBase = true
			};
			var cusSealCollectionSupporterMoq = moq.As<ICusSealCollectionSupporter>();
			var seals = new CusSealCollection(moq.Object);
			var bindingList = (IBindingList)seals;

			cusSealCollectionSupporterMoq.Setup(x => x.AllowNewCusSeal).Returns(true);
			AssertEquals("seals allowNew should be equal to container.AllowNewCusSeal = true.", true, bindingList.AllowNew);

			cusSealCollectionSupporterMoq.Setup(x => x.AllowNewCusSeal).Returns(false);
			AssertEquals("seals allowNew should be equal to container.AllowNewCusSeal = false", false, bindingList.AllowNew);
		}

		public void TestConstructorAndDefaults()
		{
			var declaration = Factory.New<JobDeclaration>();

			var equipment = declaration.Equipments.AddNew();
			var equipmentCollection = equipment.Seals;
			AssertContains(
				"Should set correct BK_ParentTableCode for filter.",
				CusSealSchema.Constants.BK_ParentTableCode + " = '" + equipment.TablePrefix + "'"
				, equipmentCollection.CompleteFilter.LiteralTextADO
			);
			var equipmentSeal = equipmentCollection.AddNew();
			AssertEquals("New element should have correct BK_ParentTableCode.", equipment.TablePrefix, equipmentSeal.BK_ParentTableCode);

			var container = declaration.CusContainers.AddNew();
			var containerCollection = container.AdditionalSeals;
			AssertContains(
				"Should set correct BK_ParentTableCode for filter.",
				CusSealSchema.Constants.BK_ParentTableCode + " = '" + container.TablePrefix + "'"
				, containerCollection.CompleteFilter.LiteralTextADO
			);
			var containerSeal = containerCollection.AddNew();
			AssertEquals("New element should have correct BK_ParentTableCode.", container.TablePrefix, containerSeal.BK_ParentTableCode);
		}

		protected override CusSealCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			var equipment = declaration.Equipments.AddNew();
			equipment.CEQ_IdentificationNumber = "E1";
			return new CusSealCollection(equipment);
		}
	}
}
