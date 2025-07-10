using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(SupportingDocument))]
	class SupportingDocumentsTest : CusSupportingInfoTest<SupportingDocument>
	{
		public void TestDocumentDescription()
		{
			var today = ZDateTime.Now;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("IC2DT", "IC2DT");
			helper.CreateCusCodeList("EUN", "IC2DT", "Y001", "Wholly obtained in Lebanon and transported directly from that country to the Community.", yesterday, tomorrow);

			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			var supportingDocuments = header.SupportingDocuments.AddNew();

			supportingDocuments.CSI_Code = "Y001";
			AssertEquals("Wholly obtained in Lebanon and transported directly from that country to the Community.", supportingDocuments.DocumentDescription);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			return header.SupportingDocuments.AddNew();
		}

		protected override IEnumerable<SupportingDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
			var supportingDocumentsOnHeader = header.SupportingDocuments.AddNew();

			var bill = header.Bills.AddNew();
			var supportingDocumentsOnBill = bill.SupportingDocuments.AddNew();

			var pack = bill.Packs.AddNew();
			var supportingDocumentsOnPack = pack.SupportingDocuments.AddNew();

			Factory.Save();

			yield return supportingDocumentsOnHeader;
			yield return supportingDocumentsOnBill;
			yield return supportingDocumentsOnPack;
		}
	}
}
