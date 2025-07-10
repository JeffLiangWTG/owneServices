using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using static Enterprise.Core.Constants.Customs.Universal;
using CHRefCusCodeList = Enterprise.Customs.CH.Business.UniversalReferenceConstants.RefCusCodeList;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class AdditionalTransitOperationValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCSI_IssuerType()
	{
		var refDataTestHelper = new RefDataTestHelper(Factory);
		refDataTestHelper.CreateCodeList(CHRefCusCodeList.PassarTypes.N1150).CreateCode("T1");
		Factory.Save();

		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(AdditionalTransitOperation.CSI_IssuerTypeInfo, "T9", "T1");
	}

	public void TestCheckCSI_ReferenceNumber()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(AdditionalTransitOperation.CSI_ReferenceNumberInfo);
	}

	public void TestCheckCSI_Description()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(AdditionalTransitOperation.CSI_DescriptionInfo);
	}

	public void TestCheckCSI_Quantity()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(AdditionalTransitOperation.CSI_QuantityInfo);
	}

	public void TestCheckCSI_PackQty()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(AdditionalTransitOperation.CSI_PackQtyInfo);
	}

	public void TestCheckCSI_PackType()
	{
		var refDataTestHelper = new RefDataTestHelper(Factory);
		refDataTestHelper.CreateCodeList(RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, RefDataGrouping.Codes.UnitedNationsRecommendations).CreateCode("01");
		Factory.Save();

		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(AdditionalTransitOperation.CSI_PackTypeInfo, "02", "01");
	}

	public void TestCheckCSI_Status()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(AdditionalTransitOperation.CSI_StatusInfo, new ZString[] { "X" }, new ZString[] { "Y", "N" });
		ValidationTestHelper.AssertFieldIsNotMandatory(AdditionalTransitOperation.CSI_StatusInfo);
	}

	AdditionalTransitOperation AdditionalTransitOperation => additionalTransitOperation ?? (additionalTransitOperation = CreateAdditionalTransitOperations());
	AdditionalTransitOperation additionalTransitOperation;

	AdditionalTransitOperation CreateAdditionalTransitOperations()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		return nctsHeader.ArrivalMovementHeader.AdditionalTransitOperations.AddNew();
	}
}
