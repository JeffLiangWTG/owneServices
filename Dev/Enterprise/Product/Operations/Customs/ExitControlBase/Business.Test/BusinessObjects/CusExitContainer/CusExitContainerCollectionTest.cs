using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ExitControlBase.Business.Testing;

[TestedType(typeof(CusExitContainerCollection<CusExitContainer>))]
sealed class CusExitContainerCollectionTest : ActiveBusinessObjectCollectionTestCase<CusExitContainerCollection<CusExitContainer>>
{
	public void TestFlags()
	{
		var header = Factory.New<CusExitHeader>();
		var containers = header.CusExitContainers;
		var container1 = containers.AddNew();
		var container2 = containers.AddNew();
		var container3 = containers.AddNew();
		var flags = containers.Flags;
		AssertEquals("flags.HasANonEquipment", true, flags.HasANonEquipment);
		AssertEquals("flags.HasAnEquipment", false, flags.HasAnEquipment);

		container2.CXN_IsEquipment = ZBool.True;
		AssertEquals("flags.HasANonEquipment", true, flags.HasANonEquipment);
		AssertEquals("flags.HasAnEquipment", false, flags.HasAnEquipment);
		flags = containers.Flags;
		AssertEquals("flags.HasANonEquipment", true, flags.HasANonEquipment);
		AssertEquals("flags.HasAnEquipment", true, flags.HasAnEquipment);

		container2.Delete();
		container3.Delete();
		flags = containers.Flags;
		AssertEquals("flags.HasANonEquipment", true, flags.HasANonEquipment);
		AssertEquals("flags.HasAnEquipment", false, flags.HasAnEquipment);
		container1.Delete();
		flags = containers.Flags;
		AssertEquals("flags.HasANonEquipment", false, flags.HasANonEquipment);
		AssertEquals("flags.HasAnEquipment", false, flags.HasAnEquipment);
	}

	public void TestSequenceAutoNum()
	{
		var header = Factory.New<CusExitHeader>();
		var container1 = header.CusExitContainers.AddNew();
		CombineAssertions(() =>
		{
			AssertEquals("container1.CXN_Sequence", (ZShort)1, container1.CXN_Sequence);
			var container2 = header.CusExitContainers.AddNew();
			AssertEquals("container2.CXN_Sequence", (ZShort)2, container2.CXN_Sequence);
			container2.CXN_Sequence = 10;
			var container3 = header.CusExitContainers.AddNew();
			AssertEquals("container3.CXN_Sequence", (ZShort)11, container3.CXN_Sequence);
		});
	}

	protected override CusExitContainerCollection<CusExitContainer> GetCollectionToTest()
	{
		var master = Factory.New<CusExitHeader>();
		return new CusExitContainerCollection<CusExitContainer>(master);
	}
}
