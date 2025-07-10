using System.Runtime.CompilerServices;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsArrivalHeaderContainer))]
sealed class NctsArrivalHeaderContainerTest : Customs.Business.Testing.BaseCusInBondContainerTest<NctsArrivalHeaderContainer>
{
	public void TestCusSealType()
	{
		var container = CreateContainer(Factory);
		AssertType(((ICusSealTypeSupporter)container).CusSealType, container.Seals.AddNew());
	}

	public void TestReadOnly() => CombineAssertions(() =>
	{
		ArrivalMovementHeader.BM_NoChangesToReport = true;
		AssertEquals(AssertionMessage(), true, Container.ReadOnly);

		ArrivalMovementHeader.BM_NoChangesToReport = false;
		AssertEquals(AssertionMessage(), false, Container.ReadOnly);

		NctsHeader.EffectiveMessageStatus = CHLogicalStatusList.Codes.Sent;
		AssertEquals(AssertionMessage(), true, Container.ReadOnly);

		Container.NctsArrival.LockFile("test");

		ArrivalMovementHeader.BM_NoChangesToReport = true;
		AssertEquals(AssertionMessage(), true, Container.ReadOnly);

		ArrivalMovementHeader.BM_NoChangesToReport = false;
		AssertEquals(AssertionMessage(), true, Container.ReadOnly);

		NctsHeader.EffectiveMessageStatus = CHLogicalStatusList.Codes.Sent;
		AssertEquals(AssertionMessage(), true, Container.ReadOnly);

		string AssertionMessage([CallerLineNumber] int line = 0) => $"[{line}] Locked={Container.NctsArrival.IsLocked} BM_NoChangesToReport={ArrivalMovementHeader.BM_NoChangesToReport} IsUnloadingRemarksReadOnly={ArrivalMovementHeader.IsUnloadingRemarksReadOnly} EffectiveMessageStatus={NctsHeader.EffectiveMessageStatus}";
	});

	NctsHeader NctsHeader => Container.NctsHeader;

	NctsArrivalMovementHeader ArrivalMovementHeader => NctsHeader.ArrivalMovementHeader;

	NctsArrivalHeaderContainer Container => container ??= CreateContainer(Factory);
	NctsArrivalHeaderContainer container;

	NctsArrivalHeaderContainer CreateContainer(BusinessObjectFactory factory)
	{
		var header = factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		return header.ArrivalHeaderContainers.AddNew();
	}

	protected override BusinessObject GetNewBusinessObject(BusinessObjectFactory factory) => CreateContainer(factory);
}
