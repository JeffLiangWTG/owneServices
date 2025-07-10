using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	[TestedType(typeof(AdditionalInfo))]
	public class AdditionalInfoTest : EU.H7.Business.Testing.AdditionalInfoTest<AdditionalInfo>
	{
		public void TestMaxLength()
		{
			AssertEquals(5, additionalInfo.CSI_CodeInfo.MaxLength);
		}

		public void TestCaptionResourceString()
		{
			CombineAssertions(() =>
			{
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(additionalInfo.CSI_CodeInfo, (string[])null, "Code", "Code", "Code", "Code within relevant additional information list.");
			});
		}

		protected override IEnumerable<AdditionalInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			Factory.Save();

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

			Factory.Save();
		}
		AdditionalInfo additionalInfo;
	}
}
