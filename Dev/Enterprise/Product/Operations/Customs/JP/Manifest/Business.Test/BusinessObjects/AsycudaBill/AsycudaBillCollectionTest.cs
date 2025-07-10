using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBillCollection))]
	sealed class AsycudaBillCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultsForNewChild()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			var bill = header.Bills.AddNew();
			AssertEquals(OrgCusCode.JapanCodeTypes.LPC, bill.ABL_ConsigneeRegNoType);
			AssertEquals(OrgCusCode.JapanCodeTypes.FSB, bill.ABL_ShipperRegNoType);
			AssertEquals(Core.Constants.Weight.Kilograms, bill.ABL_GrossWeightUQ);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			var bill2 = header.Bills.AddNew();
			AssertEquals(ZString.Empty, bill2.ABL_ConsigneeRegNoType);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			var bill3 = header.Bills.AddNew();
			AssertEquals(OrgCusCode.JapanCodeTypes.LPC, bill3.ABL_ShipperRegNoType);
			AssertEquals(OrgCusCode.JapanCodeTypes.LPC, bill3.ABL_ConsigneeRegNoType);
			AssertEquals(OrgCusCode.JapanCodeTypes.LPC, bill3.ABL_NotifyPartyRegNoType);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			var bill4 = header.Bills.AddNew();
			AssertEquals(ZString.Empty, bill4.ABL_ShipperRegNoType);
			AssertEquals(ZString.Empty, bill4.ABL_NotifyPartyRegNoType);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return header.Bills;
		}
	}
}
