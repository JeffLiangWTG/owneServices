using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.IT.H7.Business.Testing;

[TestedType(typeof(AdditionalInfo))]
sealed public class AdditionalInfoTest : EU.H7.Business.Testing.AdditionalInfoTest<AdditionalInfo>
{
	protected override IEnumerable<AdditionalInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var header = Factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		var packedItem = bill.PackedItems.AddNew();
		var additionalInfoOnPackedItem = packedItem.AdditionalInfos.AddNew();
		Factory.Save();

		yield return additionalInfoOnPackedItem;
	}

	protected override BusinessObject GetNewBusinessObject() => additionalInfo;

	protected override void SetUp()
	{
		var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		var packedItems = bill.PackedItems.AddNew();

		additionalInfo = packedItems.AdditionalInfos.AddNew();
	}
	AdditionalInfo additionalInfo;
}
