using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(TemporaryStorageAdditionalInfo))]
sealed class TemporaryStorageAdditionalInfoTest : Customs.Business.Testing.CusSupportingInfoTest<TemporaryStorageAdditionalInfo>
{
	public void TestLookups()
	{
		var additionalInfo = Factory.New<TemporaryStorageAdditionalInfo>();
		AssertType<TemporaryStorageAdditionalInfoLookups>(additionalInfo.Lookups);
	}

	public void TestValidation()
	{
		var additionalInfo = Factory.New<TemporaryStorageAdditionalInfo>();
		var validation = additionalInfo.Validation;
		AssertType<TemporaryStorageAdditionalInfoValidation>("Validation Type", validation);
	}

	public void TestCSI_SubTypeList_DefaultValue()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var bill = header.Bills.AddNew();
		var packedItem = bill.PackedItems.AddNew();
		var addInfo = packedItem.AdditionalInfos.AddNew();

		AssertEquals("Defaut selected CSI_SubType must be 'INF' for 'Kind' field in TempStorage AddInfo", AdditionalInfoSubTypeList.Codes.AdditionalInformation, addInfo.CSI_SubType);
	}

	public void TestCSI_Description_MaxLength()
	{
		var additionalInfo = Factory.New<TemporaryStorageAdditionalInfo>();
		AssertEquals(512, additionalInfo.CSI_DescriptionInfo.MaxLength);
	}

	protected override IEnumerable<TemporaryStorageAdditionalInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var header = factory.New<TemporaryStorageHeader>();
		var bill = header.Bills.AddNew();
		var packedItem = bill.PackedItems.AddNew();
		yield return packedItem.AdditionalInfos.AddNew();
	}
}
