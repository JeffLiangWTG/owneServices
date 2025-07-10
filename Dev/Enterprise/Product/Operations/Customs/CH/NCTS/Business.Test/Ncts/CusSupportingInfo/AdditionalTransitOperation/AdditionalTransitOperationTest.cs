using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(AdditionalTransitOperation))]
class AdditionalTransitOperationTest : CusSupportingInfoTest<AdditionalTransitOperation>
{
	public void TestGetNewValidation()
	{
		AssertType<AdditionalTransitOperationValidation>(AdditionalTransitOperation.Validation);
	}

	public void TestGetNewLookups()
	{
		AssertType<AdditionalTransitOperationLookups>(AdditionalTransitOperation.Lookups);
	}

	public void TestCaptions() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(AdditionalTransitOperation.CSI_LineNoInfo, caption: "Sequence Number", shortCaption: "Seq. No.");
		CaptionTestHelper.AssertCaptions(AdditionalTransitOperation.CSI_IssuerTypeInfo, caption: "Add. Transit Code", shortCaption: "Add. Transit");
		CaptionTestHelper.AssertCaptions(AdditionalTransitOperation.CSI_ReferenceNumberInfo, caption: "Reference Number", shortCaption: "Reference");
		CaptionTestHelper.AssertCaptions(AdditionalTransitOperation.CSI_DescriptionInfo, caption: "Description");
		CaptionTestHelper.AssertCaptions(AdditionalTransitOperation.CSI_QuantityInfo, caption: "Gross Mass");
		CaptionTestHelper.AssertCaptions(AdditionalTransitOperation.CSI_PackQtyInfo, caption: "Number of Packages", shortCaption: "No. Packages");
		CaptionTestHelper.AssertCaptions(AdditionalTransitOperation.CSI_PackTypeInfo, caption: "Type of Package");
		CaptionTestHelper.AssertCaptions(AdditionalTransitOperation.CSI_StatusInfo, caption: "State of Seals Valid");
	});

	public void TestCSI_LineNo() => CombineAssertions(() =>
	{
		var collection = AdditionalTransitOperation.Parent.AdditionalTransitOperations;

		AssertEquals("1st", 1, AdditionalTransitOperation.CSI_LineNo);

		var additionalTransitOperation2 = collection.AddNew();
		AssertEquals("2nd", 2, additionalTransitOperation2.CSI_LineNo);

		AdditionalTransitOperation.CSI_LineNo = 2;
		AssertEquals("2nd renumbered", 1, additionalTransitOperation2.CSI_LineNo);
	});

	public void TestCSI_IssuerType()
	{
		AssertEquals("MaxLength", 2, AdditionalTransitOperation.CSI_IssuerTypeInfo.MaxLength);
	}

	public void TestCSI_ReferenceNumber()
	{
		AssertEquals("MaxLength", 70, AdditionalTransitOperation.CSI_ReferenceNumberInfo.MaxLength);
	}

	public void TestCSI_Description()
	{
		AssertEquals("MaxLength", 512, AdditionalTransitOperation.CSI_DescriptionInfo.MaxLength);
	}

	public void TestCSI_UnitOfQuantity() => CombineAssertions(() =>
	{
		AssertEquals("Default", Core.Constants.Weight.Kilograms, AdditionalTransitOperation.CSI_UnitOfQuantity);
		AssertEquals("ReadOnly", true, AdditionalTransitOperation.CSI_UnitOfQuantityInfo.ReadOnly);
		AssertEquals("MaxLength", 2, AdditionalTransitOperation.CSI_UnitOfQuantityInfo.MaxLength);
	});

	public void TestCSI_PackQty()
	{
		AssertEquals("MaxLength", 8, AdditionalTransitOperation.CSI_PackQtyInfo.MaxLength);
	}

	public void TestCSI_PackType()
	{
		AssertEquals("PK", AdditionalTransitOperation.CSI_PackType);
	}

	public void TestCSI_Status()
	{
		AssertEquals("MaxLength", 3, AdditionalTransitOperation.CSI_StatusInfo.MaxLength);
	}

	public void TestReadOnly() => CombineAssertions(() =>
	{
		using (new LockForEditTestHelper(Factory, EUJobMessageTypeList.Codes.NctsArrivalNotification, DeclarationTabPages.Codes.NctsArrivalAdditionalGoodsInformation))
		{
			AssertEquals("Unlocked", false, AdditionalTransitOperation.ReadOnly);
			AdditionalTransitOperation.Parent.Header.LockFile("test");
			AssertEquals("Locked", true, AdditionalTransitOperation.ReadOnly);
		}
	});

	public void TestYesNoListsAreTranslatable()
	{
		NCTSTestHelper.AssertYesNoListsAreTranslatable(AdditionalTransitOperation);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateAdditionalTransitOperation(factory);

	AdditionalTransitOperation AdditionalTransitOperation => additionalTransitOperation ?? (additionalTransitOperation = CreateAdditionalTransitOperation(Factory));
	AdditionalTransitOperation additionalTransitOperation;

	static AdditionalTransitOperation CreateAdditionalTransitOperation(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		return nctsHeader.ArrivalMovementHeader.AdditionalTransitOperations.AddNew();
	}
}
