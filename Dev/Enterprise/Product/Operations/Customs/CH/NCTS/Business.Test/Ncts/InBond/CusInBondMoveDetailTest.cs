using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(CusInBondMoveDetail))]
sealed class CusInBondMoveDetailTest : EnterpriseBusinessObjectTestCase
{
	public void TestAddressRequirementType() => CombineAssertions(() =>
	{
		AssertType<JobDocAddressRequirement>(CusInBondMoveDetail.ConsignorDocAddressRequirement);
		AssertType<JobDocAddressRequirement>(CusInBondMoveDetail.ConsigneeDocAddressRequirement);
	});

	public void TestConsignorDocAddressStateNotRequired() => AssertDocAddressStateNotRequired(CusInBondMoveDetail.ConsignorDocAddress);

	public void TestConsigneeDocAddressStateNotRequired() => AssertDocAddressStateNotRequired(CusInBondMoveDetail.ConsigneeDocAddress);

	void AssertDocAddressStateNotRequired(JobDocAddress address)
	{
		const string error = "You must enter a state.";
		address.E2_AddressOverride = true;
		address.Validation.ValidateE2_State();
		AssertEquals("E2_State empty and no error", false, address.E2_StateInfo.Notifications.Any(n => n.Message.Contains(error)));
	}

	public void TestConsignorAndConsigneeAlwaysReadOnly() => CombineAssertions(() =>
	{
		var arrivalMovementHeader = CusInBondMoveDetail.MoveHeader;
		AssertAddressesAreReadonly("initial");

		arrivalMovementHeader.BM_NoChangesToReport = !arrivalMovementHeader.BM_NoChangesToReport;
		AssertAddressesAreReadonly("changed");

		arrivalMovementHeader.BM_NoChangesToReport = !arrivalMovementHeader.BM_NoChangesToReport;
		AssertAddressesAreReadonly("changed");

		void AssertAddressesAreReadonly(string info)
		{
			AssertEquals(assertionMessage("Consignor"), true, CusInBondMoveDetail.ConsignorDocAddress.ReadOnly);
			AssertEquals(assertionMessage("Consignee"), true, CusInBondMoveDetail.ConsigneeDocAddress.ReadOnly);
			string assertionMessage(string addressType) => $"{addressType} BM_NoChangesToReport={arrivalMovementHeader.BM_NoChangesToReport} ({info})";
		}
	});

	CusInBondMoveDetail CusInBondMoveDetail => cusInBondMoveDetail ?? (cusInBondMoveDetail = CreateMoveDetail(Factory));
	CusInBondMoveDetail cusInBondMoveDetail;

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateMoveDetail(factory);

	CusInBondMoveDetail CreateMoveDetail(BusinessObjectFactory factory)
	{
		var header = factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		return header.Bills.AddNew().MovementDetail;
	}
}
