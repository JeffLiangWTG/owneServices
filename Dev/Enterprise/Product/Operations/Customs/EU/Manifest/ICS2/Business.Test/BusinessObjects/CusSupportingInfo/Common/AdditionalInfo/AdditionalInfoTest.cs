using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(AdditionalInfo))]
	sealed class AdditionalInfoTest : CusSupportingInfoTest<AdditionalInfo>
	{
		public void TestHumanReadableName()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var additionalInfo = header.AdditionalInfos.AddNew();
			AssertEquals("Additional Info", additionalInfo.HumanReadableName);
		}

		public void TestAdditionalInfoDescription()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var additionalInfo = header.AdditionalInfos.AddNew();

			additionalInfo.CSI_Code = "10600";
			AssertEquals("AdditionalInfoDescription is populated if code is valid", "Consignee Unknown", additionalInfo.AdditionalInfoDescription);

			additionalInfo.CSI_Code = "11111";
			AssertEquals("AdditionalInfoDescription is set to empty if code is invalid", string.Empty, additionalInfo.AdditionalInfoDescription);
		}

		public void TestCSI_CodeReadOnly()
		{
			var additionalInfo = Factory.New<AdditionalInfo>();
			Assert("Code is not read-only as both fields are empty", !additionalInfo.CSI_CodeInfo.ReadOnly);

			additionalInfo.CSI_Code = "10600";
			Assert("Code is not read-only as Code was entered", !additionalInfo.CSI_CodeInfo.ReadOnly);

			additionalInfo.CSI_Code = string.Empty;
			additionalInfo.CSI_Description = "A non-empty description";
			Assert("Code is read-only as Text is not empty", additionalInfo.CSI_CodeInfo.ReadOnly);

			additionalInfo.CSI_Code = "10600";
			Assert("Code is not read-only as both Code and Text is not empty", !additionalInfo.CSI_CodeInfo.ReadOnly);
		}

		public void TestCSI_DescriptionReadOnly()
		{
			var additionalInfo = Factory.New<AdditionalInfo>();
			Assert("Text is not read-only as both fields are empty", !additionalInfo.CSI_DescriptionInfo.ReadOnly);

			additionalInfo.CSI_Code = "10600";
			Assert("Text is read-only as Code was entered", additionalInfo.CSI_DescriptionInfo.ReadOnly);

			additionalInfo.CSI_Code = string.Empty;
			additionalInfo.CSI_Description = "A non-empty description";
			Assert("Text is not read-only as Text was entered", !additionalInfo.CSI_DescriptionInfo.ReadOnly);

			additionalInfo.CSI_Code = "10600";
			Assert("Text is not read-only as both Code and Text is not empty", !additionalInfo.CSI_DescriptionInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			return header.AdditionalInfos.AddNew();
		}

		protected override IEnumerable<AdditionalInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
			var additionalInfoOnHeader = header.AdditionalInfos.AddNew();

			var bill = header.Bills.AddNew();
			var additionalInfoOnBill = bill.AdditionalInfos.AddNew();

			var pack = bill.Packs.AddNew();
			var additionalInfoOnPack = pack.AdditionalInfos.AddNew();

			Factory.Save();

			yield return additionalInfoOnHeader;
			yield return additionalInfoOnBill;
			yield return additionalInfoOnPack;
		}
	}
}
