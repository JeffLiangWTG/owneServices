using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.Business.Testing
{
	[TestedType(typeof(PBNCustomsDeclarationItem))]
	sealed class PBNCustomsDeclarationItemTest : Customs.Business.Testing.CusSupportingInfoTest<PBNCustomsDeclarationItem>
	{
		public void TestHumanReadableName()
		{
			AssertEquals("Customs Reference", declarationItem.HumanReadableName);
		}

		public void TestSetDefaultValues()
		{
			CombineAssertions(() =>
			{
				AssertEquals("PBN", declarationItem.CSI_Type);
				AssertEquals("TBA", declarationItem.CSI_Status);
			});
		}

		public void TestGetNewLookups()
		{
			AssertEquals(typeof(PBNCustomsDeclarationItemLookups), declarationItem.Lookups.GetType());
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<AsycudaManifestHeader>();
			header.FillWithValidTestData();
			declarationItem = header.CustomsReferenceCollection.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return header.CustomsReferenceCollection.AddNew();
		}

		protected override IEnumerable<PBNCustomsDeclarationItem> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var manifestHeader = factory.New<AsycudaManifestHeader>();
			manifestHeader.FillWithValidTestData();
			var result = manifestHeader.CustomsReferenceCollection.AddNew();
			result.CSI_Code = "AES";
			yield return result;
		}

		AsycudaManifestHeader header;
		PBNCustomsDeclarationItem declarationItem;
	}
}
