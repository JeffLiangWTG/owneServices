using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(HRCMAdditionalInfo))]
	sealed class HRCMAdditionalInfoTest : CusSupportingInfoTest<HRCMAdditionalInfo>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var billScreening = Factory.NewWithValidTestData<AsycudaBillScreening>();
			var additionalInfo = billScreening.AdditionalInfos.AddNew();
			return additionalInfo;
		}

		protected override IEnumerable<HRCMAdditionalInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;

			var billScreeningOnHeader = header.BillScreenings.AddNew();
			var additionalInfoOnBillScreeningOnHeader = billScreeningOnHeader.AdditionalInfos.AddNew();

			var bill = header.Bills.AddNew();
			var billScreeningOnBill = bill.BillScreenings.AddNew();
			var additionalInfoOnBillScreeningOnBill = billScreeningOnBill.AdditionalInfos.AddNew();

			Factory.Save();

			yield return additionalInfoOnBillScreeningOnHeader;
			yield return additionalInfoOnBillScreeningOnBill;
		}
	}
}
