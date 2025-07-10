using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(EMCSGlbCompanyCredentialCollection))]
	public class EMCSGlbCompanyCredentialCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var company = Factory.New<GlbCompany>();
			return new EMCSGlbCompanyCredentialCollection(company);
		}
	}
}
