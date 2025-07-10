using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(MovementReferenceNumberSupportingInfo))]
sealed class MovementReferenceNumberSupportingInfoTest : CusSupportingInfoTest<MovementReferenceNumberSupportingInfo>
{
	public void TestCaptions()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Caption CSI_LineNo", "Sequence Number", MRN.CSI_LineNoInfo.Description);
			AssertEquals("Caption CSI_ReferenceNUmber", "MRN", MRN.CSI_ReferenceNumberInfo.Description);
			AssertEquals("Caption CSI_Status", "Seals State Valid", MRN.CSI_StatusInfo.Description);
			AssertEquals("Caption CSI_Description", "Additional Text", MRN.CSI_DescriptionInfo.Description);
		});
	}

	public void TestCSI_LineNoReadOnly()
	{
		AssertEquals("CSI_LineNoInfo ReadOnly", true, MRN.CSI_LineNoInfo.ReadOnly);
	}

	public void TestCSI_LineNo() => CombineAssertions(() =>
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		var collection = nctsHeader.ArrivalMovementHeader.MovementReferenceNumbers;

		var mrn1 = collection.AddNew();
		AssertEquals("1st MRN", 1, mrn1.CSI_LineNo);

		var mrn2 = collection.AddNew();
		AssertEquals("2nd MRN", 2, mrn2.CSI_LineNo);

		mrn1.CSI_LineNo = 2;
		AssertEquals("1st MRN renumbered", 1, MRN.CSI_LineNo);
	});

	public void TestPropertiesReadOnlyIfLockedARN() => CombineAssertions(() =>
	{
		using (new LockForEditTestHelper(Factory, EUJobMessageTypeList.Codes.NctsArrivalNotification, DeclarationTabPages.Codes.NctsArrivalNotification))
		{
			MRN.Parent.Header.LockFile("Test lock");
			AssertProperties("Locked", true);
			MRN.Parent.Header.UnlockFile("Test unlock");
			AssertProperties("Unlocked", false);
		}

		void AssertProperties(string assertionMessage, bool expectedReadOnly)
		{
			AssertEquals($"{assertionMessage} - CSI_ReferenceNumberInfo.ReadOnly", expectedReadOnly, MRN.CSI_ReferenceNumberInfo.ReadOnly);
			AssertEquals($"{assertionMessage} - CSI_StatusInfo.ReadOnly", expectedReadOnly, MRN.CSI_StatusInfo.ReadOnly);
			AssertEquals($"{assertionMessage} - CSI_DescriptionInfo.ReadOnly", expectedReadOnly, MRN.CSI_DescriptionInfo.ReadOnly);
		}
	});

	public void TestReadOnly() => CombineAssertions(() =>
	{
		MRN.ReadOnly = true;
		AssertEquals("Set to true", true, MRN.ReadOnly);
		MRN.ReadOnly = false;
		AssertEquals("Set to false", false, MRN.ReadOnly);
		MRN.Parent.BM_MessageStatus = LogicalStatusList.Codes.Sent;
		AssertEquals("ReadOnly by parent", true, MRN.ReadOnly);
	});

	public void TestYesNoListsAreTranslatable()
	{
		NCTSTestHelper.AssertYesNoListsAreTranslatable(MRN);
	}

	protected override BusinessObject GetNewBusinessObject() => CreateNewMRN(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateNewMRN(factory);

	MovementReferenceNumberSupportingInfo MRN => mrn ?? (mrn = CreateNewMRN(Factory));
	MovementReferenceNumberSupportingInfo mrn;

	MovementReferenceNumberSupportingInfo CreateNewMRN(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		nctsHeader.ArrivalMovementHeader.MultipleMRNIndicator = true;
		return nctsHeader.ArrivalMovementHeader.MovementReferenceNumbers.AddNew();
	}
}
