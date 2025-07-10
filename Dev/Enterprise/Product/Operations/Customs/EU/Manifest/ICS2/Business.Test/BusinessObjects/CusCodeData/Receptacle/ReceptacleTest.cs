using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(Receptacle))]
	sealed class ReceptacleTest : CusCodeDataWithOrderAbstractTest<Receptacle>
	{
		public void TestHumanReadableName()
		{
			AssertEquals("Receptacle", Factory.New<Receptacle>().HumanReadableName);
		}

		public void TestDefaultValues()
		{
			var receptacle = Factory.New<Receptacle>();
			AssertEquals(CusCodeDataTypeList.Codes.EUICS2Receptacle, receptacle.CY_Type);
		}

		public void TestValidation()
		{
			var receptacle = Factory.New<Receptacle>();
			AssertEquals("Validation", typeof(ReceptacleValidation), receptacle.Validation.GetType());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override IEnumerable<Receptacle> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (Receptacle)GetNewBusinessObjectForDeleteTest(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;

			var bill = header.Bills.AddNew();
			return header.Receptacles.AddNew();
		}

		protected override string ExpectedCusCodeDataType => CusCodeDataTypeList.Codes.EUICS2Receptacle;
	}
}
