using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(PresentationOfficeCode))]
	sealed class PresentationOfficeCodeTest : Customs.Business.Testing.CusCodeDataTest<PresentationOfficeCode>
	{
		public void TestLookups()
		{
			var officeCode = (PresentationOfficeCode)GetNewBusinessObject();
			AssertType<PresentationOfficeCodeLookups>(officeCode.Lookups);
		}

		public void TestValidation()
		{
			var officeCode = (PresentationOfficeCode)GetNewBusinessObject();
			AssertType<PresentationOfficeCodeValidation>(officeCode.Validation);
		}

		protected override IEnumerable<PresentationOfficeCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var officeCode = GetNewBusinessObject() as PresentationOfficeCode;
			Factory.Save();

			yield return officeCode;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.PresentationOffice = "IETHR800";
			return header.PresentationOfficeCode;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();
	}
}
