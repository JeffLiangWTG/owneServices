using System.Runtime.CompilerServices;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(CusSeal))]
sealed class CusSealTest : EnterpriseBusinessObjectTestCase
{
	public void TestICusCodeDataTypeSupporter()
	{
		AssertEquals(typeof(UnloadingRemarks), Seal.GetCusCodeDataTypes()[CusCodeDataTypeList.Codes.UnloadingRemarks]);
	}

	public void TestUnloadingRemarksText()
	{
		CombineAssertions(() =>
		{
			Seal.BK_SealNumber = "12345";
			AssertNullOrEmpty("initially empty", Seal.UnloadingRemarksText);

			Seal.UnloadingRemarksText = "Unloading Remarks 1";
			AssertEquals("Value", "Unloading Remarks 1", Seal.UnloadingRemarksText);
		});
	}

	public void TestBK_UnloadingState_ReadOnly()
	{
		AssertReadOnly(true, ZBool.True, EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW);
		AssertReadOnly(true, ZBool.True, EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC);
		AssertReadOnly(true, ZBool.True, EU.NCTS.Business.NctsUnloadedStateList.Codes.DAM);
		AssertReadOnly(true, ZBool.True, EU.NCTS.Business.NctsUnloadedStateList.Codes.DIF);
		AssertReadOnly(true, ZBool.True, EU.NCTS.Business.NctsUnloadedStateList.Codes.MIS);

		AssertReadOnly(true, ZBool.True, EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW);
		AssertReadOnly(false, ZBool.False, EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC);
		AssertReadOnly(false, ZBool.False, EU.NCTS.Business.NctsUnloadedStateList.Codes.DAM);
		AssertReadOnly(false, ZBool.False, EU.NCTS.Business.NctsUnloadedStateList.Codes.DIF);
		AssertReadOnly(false, ZBool.False, EU.NCTS.Business.NctsUnloadedStateList.Codes.MIS);

		AssertReadOnly(true, ZBool.False, EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC, messageStatus: CHLogicalStatusList.Codes.Acknowledged);
		AssertReadOnly(true, ZBool.False, EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC, messageStatus: CHLogicalStatusList.Codes.Sent);

		void AssertReadOnly(bool expectedReadOnly, ZBool noChangesToReport, string unloadingState, string messageStatus = "")
		{
			NctsHeader.EffectiveMessageStatus = messageStatus;
			ArrivalMovementHeader.BM_NoChangesToReport = noChangesToReport;
			Seal.BK_UnloadingState = unloadingState;
			var assertionMessage = $"MessageStatus={NctsHeader.EffectiveMessageStatus} BM_NoChangesToReport={ArrivalMovementHeader.BM_NoChangesToReport} BK_UnloadingState={Seal.BK_UnloadingState}";
			AssertEquals(assertionMessage, expectedReadOnly, Seal.BK_UnloadingStateInfo.ReadOnly);
		}
	}

	public void TestBK_UnloadingState_EmptyUnloadingRemarks()
	{
		Seal.UnloadingRemarksText = "Unloading Remarks description";
		Seal.BK_UnloadingState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DIF;
		AssertNotNullOrEmpty("Unloading Remarks should be set when Unloaded State = DIF", Seal.UnloadingRemarksText);
		Seal.BK_UnloadingState = EU.NCTS.Business.NctsUnloadedStateList.Codes.MIS;
		AssertNotNullOrEmpty("Unloading Remarks should be set when Unloaded State = MIS", Seal.UnloadingRemarksText);
		Seal.BK_UnloadingState = EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
		AssertNotNullOrEmpty("Unloading Remarks should be set when Unloaded State = NEW", Seal.UnloadingRemarksText);
		Seal.BK_UnloadingState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;
		AssertNullOrEmpty("Unloading Remarks should be empty when Unloaded State = DEC", Seal.UnloadingRemarksText);
	}

	public void TestReadOnly() => CombineAssertions(() =>
	{
		ArrivalMovementHeader.BM_NoChangesToReport = true;
		AssertEquals(AssertionMessage(), true, Seal.ReadOnly);

		ArrivalMovementHeader.BM_NoChangesToReport = false;
		AssertEquals(AssertionMessage(), false, Seal.ReadOnly);

		NctsHeader.EffectiveMessageStatus = CHLogicalStatusList.Codes.Sent;
		AssertEquals(AssertionMessage(), true, Seal.ReadOnly);

		Seal.Header.LockFile("test");

		ArrivalMovementHeader.BM_NoChangesToReport = true;
		AssertEquals(AssertionMessage(), true, Seal.ReadOnly);

		ArrivalMovementHeader.BM_NoChangesToReport = false;
		AssertEquals(AssertionMessage(), true, Seal.ReadOnly);

		NctsHeader.EffectiveMessageStatus = CHLogicalStatusList.Codes.Sent;
		AssertEquals(AssertionMessage(), true, Seal.ReadOnly);

		string AssertionMessage([CallerLineNumber] int line = 0) => $"[{line}] Locked={Seal.Header.IsLocked} BM_NoChangesToReport={ArrivalMovementHeader.BM_NoChangesToReport} IsUnloadingRemarksReadOnly={ArrivalMovementHeader.IsUnloadingRemarksReadOnly}";
	});

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateSeal(factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => CreateSeal(Factory);

	NctsHeader NctsHeader => (NctsHeader)Seal.Header;

	NctsArrivalMovementHeader ArrivalMovementHeader => NctsHeader.ArrivalMovementHeader;

	CusSeal Seal => seal??= CreateSeal(Factory);
	CusSeal seal;

	CusSeal CreateSeal(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Arrival;
		var seal = nctsHeader.ArrivalHeaderContainers.AddNew().Seals.AddNew();
		seal.BK_SealNumber = "S1";
		return seal;
	}
}
