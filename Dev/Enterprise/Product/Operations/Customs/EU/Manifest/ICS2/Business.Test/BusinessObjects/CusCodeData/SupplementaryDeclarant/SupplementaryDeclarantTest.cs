using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(SupplementaryDeclarant))]
	sealed class SupplementaryDeclarantTest : CusCodeDataTest<SupplementaryDeclarant>
	{
		public void TestHumanReadableName()
		{
			AssertEquals("Supplementary Declarant", Factory.New<SupplementaryDeclarant>().HumanReadableName);
		}

		public void TestMaxLengthOfProperties()
		{
			var supplementaryDeclarant = (SupplementaryDeclarant)GetNewBusinessObject();
			CombineAssertions("Max length", () =>
			{
				AssertEquals(17, supplementaryDeclarant.CY_DataInfo.MaxLength);
				AssertEquals(3, supplementaryDeclarant.CY_CodeInfo.MaxLength);
			});
		}

		public void TestDefaultValues()
		{
			var supplementaryDeclarant = Factory.New<SupplementaryDeclarant>();
			AssertEquals(CusCodeDataTypeList.Codes.EUICS2SupplementaryDeclarant, supplementaryDeclarant.CY_Type);
		}

		public void TestLookups()
		{
			var supplementaryDeclarant = Factory.New<SupplementaryDeclarant>();
			AssertEquals("Lookups", typeof(SupplementaryDeclarantLookups), supplementaryDeclarant.Lookups.GetType());
		}

		public void TestValidation()
		{
			var supplementaryDeclarant = Factory.New<SupplementaryDeclarant>();
			AssertEquals("Validation", typeof(SupplementaryDeclarantValidation), supplementaryDeclarant.Validation.GetType());
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override IEnumerable<SupplementaryDeclarant> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (SupplementaryDeclarant)GetNewBusinessObjectForDeleteTest(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F10;

			var bill = header.Bills.AddNew();
			return bill.SupplementaryDeclarants.AddNew();
		}

		#endregion
	}
}
