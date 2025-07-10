using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsDepartureCargoDescTraderValidatorTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Exception expected when NtcsDepartureCargoDesc is null", () => new NctsDepartureCargoDescTraderValidator(null));
			AssertExceptionThrown<ArgumentNullException>("Exception expected when NtcsDepartureCargoDesc.Header is null", () => new NctsDepartureCargoDescTraderValidator(Factory.New<NctsDepartureCargoDesc>()));
		});
	}

	public void TestValidateConsignorRelatedToGoodsItemsCount()
	{
		AssertHasDeclarationMustHaveMoreGoodsItemWhenParticipantsTypeIsGroupageMessageError(nameof(goodsItem.Consignor), goodsItem.Consignor, nctsHeader.Consignor);
	}

	public void TestValidateConsigneeRelatedToGoodsItemsCount()
	{
		AssertHasDeclarationMustHaveMoreGoodsItemWhenParticipantsTypeIsGroupageMessageError(nameof(goodsItem.Consignee), goodsItem.Consignee, nctsHeader.Consignee);
	}

	public void TestValidateConsignorRelatedToOtherGoodsItems()
	{
		AssertHasDeclarationMustHaveDifferentOrganizationsAtGoodsItemLevelWhenParticipantsTypeIsGroupageMessageError(nameof(goodsItem.Consignor), x => x.Consignor, nctsHeader.Consignor);
	}

	public void TestValidateConsigneeRelatedToOtherGoodsItems()
	{
		AssertHasDeclarationMustHaveDifferentOrganizationsAtGoodsItemLevelWhenParticipantsTypeIsGroupageMessageError(nameof(goodsItem.Consignee), x => x.Consignee, nctsHeader.Consignee);
	}

	public void TestValidateConsignorTraderRelatedToHeaderLevelForStandard()
	{
		AssertHasParticipantTypeShouldBeGroupageMessageError(nameof(goodsItem.Consignor), goodsItem.Consignor, nctsHeader.Consignor);
	}

	public void TestValidateConsigneeTraderRelatedToHeaderLevelForStandard()
	{
		AssertHasParticipantTypeShouldBeGroupageMessageError(nameof(goodsItem.Consignee), goodsItem.Consignee, nctsHeader.Consignee);

		var phase5Header = Factory.New<NctsHeader>();
		phase5Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		phase5Header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		var phase5GoodsItem = phase5Header.Bills.AddNew().GoodsItems.AddNew();
		AssertHasParticipantTypeShouldNotBeGroupageMessageErrorForConsignee(nameof(phase5GoodsItem.Consignee), phase5GoodsItem.Consignee, phase5Header.Consignee);
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		goodsItem = AddNewGoodsItem();
	}
	NctsHeader nctsHeader;
	NctsDepartureCargoDesc goodsItem;

	void AssertHasDeclarationMustHaveMoreGoodsItemWhenParticipantsTypeIsGroupageMessageError(string traderName, JobDocAddress trader, JobDocAddress traderHeaderLevel)
	{
		var expectedMessageError = "To declare Consignor and Consignee at Goods Items level (Participants = GRP), you must have more than one Goods Item";
		SetMovementAsGroupage();

		AssertEquals($"[PRE-CONDITION] {traderName} is empty at Header Level, {traderName}.OrganisationPK ", ZGuid.Empty, traderHeaderLevel.OrganisationPK);

		trader.OrganisationPK = GetNewOrganizationPK();
		AssertHasMessageErrorContaining("When declaration is GRP and has only one goods item", trader.OrganisationPKInfo, expectedMessageError);

		AddNewGoodsItem();
		trader.Validation.ValidateOrganisationPK();
		AssertNoMessageErrorContaining("When declaration is GRP and has more than one goods item", trader.OrganisationPKInfo, expectedMessageError);
	}

	void AssertHasDeclarationMustHaveDifferentOrganizationsAtGoodsItemLevelWhenParticipantsTypeIsGroupageMessageError(string traderName, Func<NctsDepartureCargoDesc, JobDocAddress> getTrader, JobDocAddress traderHeaderLevel)
	{
		var expectedMessageError = "To declare Consignor and Consignee at Goods Items level (Participants = GRP), they have to be different organizations, not the same in every item";

		SetMovementAsGroupage();

		AssertEquals($"[PRE-CONDITION] {traderName} is empty at Header Level, {traderName}.OrganisationPK ", ZGuid.Empty, traderHeaderLevel.OrganisationPK);

		var consignorA = Factory.New<OrgHeader>();
		var jobDocAddressGoodsItem1 = getTrader(goodsItem);
		jobDocAddressGoodsItem1.OrganisationPK = consignorA.PK;
		AssertNoMessageErrorContaining($"When declaration is GRP and has more than one goods item with different {traderName}", jobDocAddressGoodsItem1.OrganisationPKInfo, expectedMessageError);

		var goodsItem2 = AddNewGoodsItem();
		var jobDocAddressGoodsItem2 = getTrader(goodsItem2);

		jobDocAddressGoodsItem2.OrganisationPK = consignorA.PK;

		AssertHasMessageErrorContaining($"When declaration is GRP and has more than one goods item with same {traderName}", jobDocAddressGoodsItem2.OrganisationPKInfo, expectedMessageError);

		var consignorB = Factory.New<OrgHeader>();
		jobDocAddressGoodsItem2.OrganisationPK = consignorB.PK;
		AssertNoMessageErrorContaining($"When declaration is GRP and has more than one goods item with different {traderName}", jobDocAddressGoodsItem2.OrganisationPKInfo, expectedMessageError);
	}

	void AssertHasParticipantTypeShouldNotBeGroupageMessageErrorForConsignee(string traderName, JobDocAddress traderAddress, JobDocAddress traderHeaderLevel)
	{
		var expectedMessageError = $"Participants is STD and the {traderName} is empty in Departure Declaration Header TAB, even if there are {traderName}s entered at a goods item level. Change the participants to GRP if you want to declare more than one {traderName} or enter {traderName} at header level and remove it from goods items for STD participants";

		nctsHeader.MovementHeader.ParticipantType = NctsParticipantTypeList.Codes.StandardOneSupplierOneImporter;

		traderHeaderLevel.OrganisationPK = ZGuid.Empty;
		traderAddress.OrganisationPK = GetNewOrganizationPK();

		traderAddress.Validation.ValidateOrganisationPK();
		AssertNoMessageErrorContaining($"When declaration is GRP and {traderName} is declared at Goods Item level", traderAddress.OrganisationPKInfo, expectedMessageError);
	}

	void AssertHasParticipantTypeShouldBeGroupageMessageError(string traderName, JobDocAddress traderAddress, JobDocAddress traderHeaderLevel)
	{
		var expectedMessageError = $"Participants is STD and the {traderName} is empty in Departure Declaration Header TAB, even if there are {traderName}s entered at a goods item level. Change the participants to GRP if you want to declare more than one {traderName} or enter {traderName} at header level and remove it from goods items for STD participants";

		nctsHeader.MovementHeader.ParticipantType = NctsParticipantTypeList.Codes.StandardOneSupplierOneImporter;

		traderHeaderLevel.OrganisationPK = ZGuid.Empty;
		traderAddress.OrganisationPK = GetNewOrganizationPK();

		traderAddress.Validation.ValidateOrganisationPK();
		AssertHasMessageErrorContaining($"When declaration is STD and {traderName} is declared at Goods Item level", traderAddress.OrganisationPKInfo, expectedMessageError);

		nctsHeader.MovementHeader.ParticipantType = NctsParticipantTypeList.Codes.GroupageManySuppliersAndManyImporters;
		traderAddress.Validation.ValidateOrganisationPK();
		AssertNoMessageErrorContaining($"When declaration is GRP and {traderName} is declared at Goods Item level", traderAddress.OrganisationPKInfo, expectedMessageError);

		nctsHeader.MovementHeader.ParticipantType = NctsParticipantTypeList.Codes.StandardOneSupplierOneImporter;
		traderAddress.OrganisationPK = ZGuid.Empty;
		AssertNoMessageErrorContaining($"When declaration is STD and {traderName} is empty at Goods Item level", traderAddress.OrganisationPKInfo, expectedMessageError);
	}

	void SetMovementAsGroupage() => nctsHeader.MovementHeader.ParticipantType = NctsParticipantTypeList.Codes.GroupageManySuppliersAndManyImporters;
	NctsDepartureCargoDesc AddNewGoodsItem() => nctsHeader.MovementHeader.GoodsItems.AddNew();
	ZGuid GetNewOrganizationPK() => Factory.New<OrgHeader>().PK;
}
