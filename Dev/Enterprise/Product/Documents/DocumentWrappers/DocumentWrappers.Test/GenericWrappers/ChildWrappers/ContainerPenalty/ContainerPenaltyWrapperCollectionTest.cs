using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ContainerPenaltyWrapperCollection))]
	sealed class ContainerPenaltyWrapperCollectionTest : GenericWrapperCollectionTest<ContainerPenaltyWrapperCollection>
	{
		#region Constructors

		public void TestConstructorFromConsol()
		{
			var consol = Factory.New<CommonConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CON01";
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CON02";
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40FR").PK;

			var penalty1 = container1.ExportPenalties.AddNew();
			penalty1.CPY_PenaltyType = Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention;
			var penalty2 = container2.ImportPenalties.AddNew();
			penalty2.CPY_PenaltyType = Core.Constants.ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention;

			var collection = new ContainerPenaltyWrapperCollection(consol, Factory);
			AssertEquals(2, collection.Count);
			AssertEquals("collection[0].Container.ContainerNo", "CON02", collection[0].Container.ContainerNo);
			AssertEquals("collection[0].Container.Type.Code", "40FR", collection[0].Container.Type.Code);
			AssertEquals("collection[0].PenaltyType", "MDD", collection[0].PenaltyType);
			AssertEquals("collection[1].Container.ContainerNo", "CON01", collection[1].Container.ContainerNo);
			AssertEquals("collection[1].Container.Type.Code", "20GP", collection[1].Container.Type.Code);
			AssertEquals("collection[1].PenaltyType", "DET", collection[1].PenaltyType);
		}

		public void TestConstructorFromShipment()
		{
			var consol = Factory.New<CommonConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CON01";
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CON02";
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40FR").PK;

			var shipment = consol.Shipments.AddNew();

			var penalty1 = shipment.PickupPenalties.AddNew();
			penalty1.CPY_PenaltyType = Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention;
			penalty1.CPY_JC_Container = container1.PK;
			var penalty2 = shipment.DeliveryPenalties.AddNew();
			penalty2.CPY_PenaltyType = Core.Constants.ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention;
			penalty2.CPY_JC_Container = container2.PK;

			var collection = new ContainerPenaltyWrapperCollection(shipment, Factory);
			AssertEquals(2, collection.Count);
			AssertEquals("collection[0].Container.ContainerNo", "CON02", collection[0].Container.ContainerNo);
			AssertEquals("collection[0].Container.Type.Code", "40FR", collection[0].Container.Type.Code);
			AssertEquals("collection[0].PenaltyType", "MDD", collection[0].PenaltyType);
			AssertEquals("collection[1].Container.ContainerNo", "CON01", collection[1].Container.ContainerNo);
			AssertEquals("collection[1].Container.Type.Code", "20GP", collection[1].Container.Type.Code);
			AssertEquals("collection[1].PenaltyType", "DET", collection[1].PenaltyType);
		}

		public void TestConstructorFromDeclaration()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var cusContainer1 = declaration.CusContainers.AddNew();
			cusContainer1.CO_ContainerNumber = "CON01";
			cusContainer1.CO_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			var cusContainer2 = declaration.CusContainers.AddNew();
			cusContainer2.CO_ContainerNumber = "CON02";
			cusContainer2.CO_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40FR").PK;

			var penalty1 = cusContainer1.ImportPenalties.AddNew();
			penalty1.CPY_PenaltyType = Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention;
			var penalty2 = cusContainer2.ExportPenalties.AddNew();
			penalty2.CPY_PenaltyType = Core.Constants.ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention;

			var collection = new ContainerPenaltyWrapperCollection(declaration, Factory);
			AssertEquals(2, collection.Count);
			AssertEquals("collection[0].Container.ContainerNo", "CON01", collection[0].Container.ContainerNo);
			AssertEquals("collection[0].Container.Type.Code", "20GP", collection[0].Container.Type.Code);
			AssertEquals("collection[0].PenaltyType", "DET", collection[0].PenaltyType);
			AssertEquals("collection[1].Container.ContainerNo", "CON02", collection[1].Container.ContainerNo);
			AssertEquals("collection[1].Container.Type.Code", "40FR", collection[1].Container.Type.Code);
			AssertEquals("collection[1].PenaltyType", "MDD", collection[1].PenaltyType);
		}

		#endregion

		#region Implementation

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new ContainerPenaltyWrapper(null, Factory);
		}

		protected override ContainerPenaltyWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new ContainerPenaltyWrapperCollection(Factory);
		}

		#endregion
	}
}
