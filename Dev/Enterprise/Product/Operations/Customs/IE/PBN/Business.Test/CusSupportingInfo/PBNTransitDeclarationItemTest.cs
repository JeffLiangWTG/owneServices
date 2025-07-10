using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.Business.Testing
{
	[TestedType(typeof(PBNTransitDeclarationItem))]
	sealed class PBNTransitDeclarationItemTest : Customs.Business.Testing.CusSupportingInfoTest<PBNTransitDeclarationItem>
	{
		public void TestHumanReadableName()
		{
			AssertEquals("Transit Reference", transitItem.HumanReadableName);
		}

		public void TestSetDefaultValues()
		{
			CombineAssertions(() =>
			{
				AssertEquals("PBN", transitItem.CSI_Type);
				AssertEquals("NCTS", transitItem.CSI_Code);
				AssertEquals("TBA", transitItem.CSI_Status);
			});
		}

		public void TestGetNewLookups()
		{
			AssertEquals(typeof(PBNTransitDeclarationItemLookups), transitItem.Lookups.GetType());
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<AsycudaManifestHeader>();
			header.FillWithValidTestData();
			transitItem = header.TransitDeclarationCollection.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject() => header.TransitDeclarationCollection.AddNew();

		protected override IEnumerable<PBNTransitDeclarationItem> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var manifestHeader = factory.New<AsycudaManifestHeader>();
			manifestHeader.FillWithValidTestData();
			var result = manifestHeader.TransitDeclarationCollection.AddNew();
			yield return result;
		}

		AsycudaManifestHeader header;
		PBNTransitDeclarationItem transitItem;
	}
}
