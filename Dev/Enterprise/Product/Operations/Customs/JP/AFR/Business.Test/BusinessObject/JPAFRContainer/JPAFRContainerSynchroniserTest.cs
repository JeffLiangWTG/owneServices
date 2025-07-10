using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class JPAFRContainerSynchroniserTest : AFRSynchroniserTestCase
	{
		public void TestSynchroniseJPC_ContainerNum()
		{
			container.JC_ContainerNum = "CONT1";
			AssertEquals("CONT1", billContainer.JPC_ContainerNum);

			container.JC_ContainerNum = ZString.Empty;
			AssertEquals(ZString.Empty, billContainer.JPC_ContainerNum);

			container.JC_ContainerNum = "CONT2";
			AssertEquals("CONT2", billContainer.JPC_ContainerNum);

			synchroniser.SetEnabled(false, false);
			container.JC_ContainerNum = "CONT1";
			AssertEquals("CONT2", billContainer.JPC_ContainerNum);
		}

		public void TestSynchroniseJPC_Seal1()
		{
			container.JC_SealNum = "SEAL1";
			AssertEquals("SEAL1", billContainer.JPC_Seal1);

			container.JC_SealNum = ZString.Empty;
			AssertEquals(ZString.Empty, billContainer.JPC_Seal1);

			container.JC_SealNum = "SEAL2";
			AssertEquals("SEAL2", billContainer.JPC_Seal1);

			synchroniser.SetEnabled(false, false);
			container.JC_SealNum = "SEAL1";
			AssertEquals("SEAL2", billContainer.JPC_Seal1);
		}

		public void TestSynchroniseJPC_Seal2()
		{
			container.JC_AdditionalSealNum = "SEAL1";
			AssertEquals("SEAL1", billContainer.JPC_Seal2);

			container.JC_AdditionalSealNum = ZString.Empty;
			AssertEquals(ZString.Empty, billContainer.JPC_Seal2);

			container.JC_AdditionalSealNum = "SEAL2";
			AssertEquals("SEAL2", billContainer.JPC_Seal2);

			synchroniser.SetEnabled(false, false);
			container.JC_AdditionalSealNum = "SEAL1";
			AssertEquals("SEAL2", billContainer.JPC_Seal2);
		}

		public void TestSynchroniseJPC_RC_ContainerType()
		{
			var containerType1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, TestContainerTypeNK);
			var containerType2 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, TestContainerType2NK);

			container.JC_RC = containerType1.PK;
			AssertEquals(containerType1.PK, billContainer.JPC_RC_ContainerType);

			container.JC_RC = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, billContainer.JPC_RC_ContainerType);

			container.JC_RC = containerType1.PK;
			AssertEquals(containerType1.PK, billContainer.JPC_RC_ContainerType);

			synchroniser.SetEnabled(false, false);
			container.JC_RC = containerType2.PK;
			AssertEquals(containerType1.PK, billContainer.JPC_RC_ContainerType);
		}

		public void TestSynchroniseJPC_IsEmpty()
		{
			container.JC_IsEmptyContainer = ZBool.True;
			AssertEquals(ZBool.True, billContainer.JPC_IsEmpty);

			container.JC_IsEmptyContainer = ZBool.False;
			AssertEquals(ZBool.False, billContainer.JPC_IsEmpty);

			synchroniser.SetEnabled(false, false);
			container.JC_IsEmptyContainer = ZBool.True;
			AssertEquals(ZBool.False, billContainer.JPC_IsEmpty);
		}

		public void TestSynchroniseJPC_Ownership()
		{
			container.JC_IsShipperOwned = false;
			CombineAssertions(() =>
			{
				AssertEquals(ZBool.False, billContainer.JPC_OwnershipCodeInfo.ReadOnly);
				AssertEquals("CarrierSupplied is now defaulted", ContainerOwnershipCodeList.Codes.CarrierSupplied, billContainer.JPC_OwnershipCode);
			});

			container.JC_IsShipperOwned = true;
			CombineAssertions(() =>
			{
				AssertEquals(ZBool.True, billContainer.JPC_OwnershipCodeInfo.ReadOnly);
				AssertEquals(ContainerOwnershipCodeList.Codes.ShipperSupplied, billContainer.JPC_OwnershipCode);
			});

			container.JC_IsShipperOwned = false;
			CombineAssertions(() =>
			{
				synchroniser.Synchronise();
				AssertEquals(ZBool.False, billContainer.JPC_OwnershipCodeInfo.ReadOnly);
				AssertEquals(ContainerOwnershipCodeList.Codes.ShipperSupplied, billContainer.JPC_OwnershipCode);
			});

			billContainer.JPC_OwnershipCode = ContainerOwnershipCodeList.Codes.DeconsolidatorSupplied;
			CombineAssertions(() =>
			{
				synchroniser.Synchronise();
				AssertEquals(ZBool.False, billContainer.JPC_OwnershipCodeInfo.ReadOnly);
				AssertEquals(ContainerOwnershipCodeList.Codes.DeconsolidatorSupplied, billContainer.JPC_OwnershipCode);
			});

			synchroniser.SetEnabled(false, false);
			billContainer.JPC_OwnershipCode = ContainerOwnershipCodeList.Codes.CarrierSupplied;
			container.JC_IsShipperOwned = true;
			CombineAssertions(() =>
			{
				AssertEquals(ZBool.False, billContainer.JPC_OwnershipCodeInfo.ReadOnly);
				AssertEquals(ContainerOwnershipCodeList.Codes.CarrierSupplied, billContainer.JPC_OwnershipCode);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			consol = CreateFCLConsol();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;

			container = consol.Containers.AddNew();

			header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;

			bill = header.Bills.AddNew();
			billContainer = bill.Containers.AddNew();

			synchroniser = new JPAFRContainerSynchroniser(billContainer, container);
			synchroniser.Synchronise(true);
		}
		ForwardingContainer container;
		JPAFRBills bill;
		JPAFRContainer billContainer;

		JPAFRContainerSynchroniser synchroniser;
	}
}
