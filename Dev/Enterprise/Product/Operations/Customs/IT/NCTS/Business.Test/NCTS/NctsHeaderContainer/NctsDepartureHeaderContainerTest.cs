using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(NctsDepartureHeaderContainer))]
sealed class NctsDepartureHeaderContainerTest : Customs.Business.Testing.BaseCusInBondContainerTest<NctsDepartureHeaderContainer>
{
	public void TestHumanReadableName()
	{
		var (_, container) = GetHeaderAndContainer();
		AssertEquals("HumanReadableName", "Container", container.HumanReadableName);
	}

	public void TestValidationType_Phase4()
	{
		var (header, container) = GetHeaderAndContainer();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		AssertType<NctsDepartureHeaderContainerPhase4Validation>("Validation Type from EU", container.Validation);
	}

	public void TestValidationType_Phase5()
	{
		var (header, container) = GetHeaderAndContainer();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		AssertType<NctsDepartureHeaderContainerPhase5Validation>("Validation Type", container.Validation);
	}

	public void TestEffectiveContainerNumber()
	{
		var container = Factory.New<NctsDepartureHeaderContainer>();
		container.BC_ContainerNum = "CNTNUM123";

		CombineAssertions(() =>
		{
			container.BC_Mode = "CNT";
			AssertEquals("When Mode is CNT, EffectiveContainerNumber", "CNTNUM123", container.EffectiveContainerNumber);

			container.BC_Mode = "NCT";
			AssertEquals("When Mode is NCT, EffectiveContainerNumber", "", container.EffectiveContainerNumber);

			container.BC_Mode = "XXX";
			AssertEquals("When Mode is XXX, EffectiveContainerNumber", "CNTNUM123", container.EffectiveContainerNumber);
		});
	}

	(NctsHeader, NctsDepartureHeaderContainer) GetHeaderAndContainer()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		var container = header.DepartureHeaderContainers.AddNew();
		return (header, container);
	}

	protected override BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var header = factory.New<NctsHeader>();
		header.BH_HeaderType = NctsMovementType.Codes.Departure;
		return header.DepartureHeaderContainers.AddNew();
	}
}
