using System;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing;

[TestedType(typeof(NctsArrivalCargoDesc))]
sealed class NctsArrivalCargoDescTest : NctsArrivalCargoDescAbstractTest<NctsHeader>
{
	public void TestValidationType()
	{
		AssertType<NctsArrivalCargoDescValidation>(arrivalCargoDesc.Validation);
	}

	public void TestPackageType()
	{
		AssertType<NctsPackageCollection<NctsArrivalCargoDesc>>(arrivalCargoDesc.Packages);
	}

	public void TestAdditionalInfos()
	{
		AssertType<EU.NCTS.Business.NctsAdditionalInfoCollection<NctsAdditionalInfo>>(arrivalCargoDesc.AdditionalInfos);
	}

	public void TestSupportingDocuments()
	{
		AssertType<EU.NCTS.Business.NctsSupportingDocumentCollection<NctsSupportingDocument>>(arrivalCargoDesc.SupportingDocuments);
	}

	public void TestFees()
	{
		AssertType<Customs.Business.CusInBondFeeCollection<NctsCargoDescFee>>(arrivalCargoDesc.Fees);
	}

	public void TestValidation()
	{
		AssertType<NctsArrivalCargoDescValidation>(arrivalCargoDesc.Validation);
	}

	public void TestTariffType()
	{
		AssertEquals("TariffType is Import", Constants.TariffTypes.Import, arrivalCargoDesc.TariffType);
	}

	public void TestUnloadedGoodsItem()
	{
		arrivalCargoDesc.BY_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DIF;
		AssertType<NctsUnloadedCargoDesc>(arrivalCargoDesc.UnloadedGoodsItem);
	}

	public void TestBY_UnloadedState_Attributes() => CombineAssertions(() =>
	{
		_ = AssertEntity<NctsArrivalCargoDesc>()
			.HasProperty(x => x.BY_UnloadedState)
			.WithAttribute<ReadOnlyMemberAttribute>(x => x.Member == "IsUnloadedStateReadOnly");

		var unloadedStateInfo = arrivalCargoDesc.BY_UnloadedStateInfo;

		AssertEquals("ReadOnly when BM_NoChangesToReport is true", true, unloadedStateInfo.ReadOnly);

		arrivalCargoDesc.Header.ArrivalMovementHeader.BM_NoChangesToReport = false;
		AssertEquals("ReadOnly when BM_NoChangesToReport is false, status is NEW and original status was NEW", true, unloadedStateInfo.ReadOnly);

		AssertEquals("ReadOnly when status is NEW and original status was NEW", true, unloadedStateInfo.ReadOnly);

		arrivalCargoDesc.BY_UnloadedState = "DEC";
		AssertEquals("Not ReadOnly when status is DEC and original status was NEW", false, arrivalCargoDesc.BY_UnloadedStateInfo.ReadOnly);

		Factory.Save();

		AssertEquals("Not ReadOnly when status is DEC and original status was not NEW", false, arrivalCargoDesc.BY_UnloadedStateInfo.ReadOnly);

		arrivalCargoDesc.BY_UnloadedState = "NEW";
		AssertEquals("Not ReadOnly when status is NEW and original status was not NEW", false, arrivalCargoDesc.BY_UnloadedStateInfo.ReadOnly);

		arrivalCargoDesc.Header.ArrivalMovementHeader.BM_NoChangesToReport = true;
		AssertEquals("ReadOnly when BM_NoChangesToReport is true, status is NEW and original status was not NEW", true, unloadedStateInfo.ReadOnly);

		arrivalCargoDesc.Header.ArrivalMovementHeader.BM_NoChangesToReport = false;
		AssertEquals("Not ReadOnly when BM_NoChangesToReport is false, status is NEW and original status was not NEW", false, unloadedStateInfo.ReadOnly);
	});

	public void TestLiabilityFormattedTariff()
	{
		CombineAssertions(() =>
		{
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				NCTSTestHelper.AddNctsArrivalMovementHeaderForLiabilityTestToHeader(Factory, header);
				arrivalCargoDesc.BY_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
				arrivalCargoDesc.BY_HarmonisedTariff = "2222222222";
				AssertEquals("LiabilityFormattedTariff should be as other FormattedTariff", "2222.22.22 22", arrivalCargoDesc.LiabilityFormattedTariff);

				arrivalCargoDesc.LiabilityFormattedTariff = "2222.22.50 50";
				AssertNotEquals("LiabilityTariff should not be set from LiabilityFormattedTariff", "2222222222", arrivalCargoDesc.LiabilityTariff);
				AssertEquals("LiabilityFormattedTariff should be the same", "2222.22.50 50", arrivalCargoDesc.LiabilityFormattedTariff);
			}
		});
	}

	protected override BusinessObject GetNewBusinessObject() => arrivalCargoDesc;

	protected override BusinessObject GetBusinessObjectForFetchForLoad()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		var bill = header.Bills.AddNew();
		var arrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();
		arrivalCargoDesc.FillWithValidTestData(TestBusinessObjectKind.MinimumRequiredToSave | TestBusinessObjectKind.PopulateDependentCollections, Array.Empty<PropertyDescriptor>());
		return arrivalCargoDesc;
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		var bill = header.Bills.AddNew();
		arrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();
	}
	NctsHeader header;
	NctsArrivalCargoDesc arrivalCargoDesc;

	protected override ZString CountryCode => Core.Constants.CountryCodes.Spain;
}
