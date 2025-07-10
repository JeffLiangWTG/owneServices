using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(OrgImpAddInfo))]
	sealed class OrgImpAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSerialiseAndDeserialise()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var organisationWrapper = OrgHeaderWrapper.New(organisation);
			organisationWrapper.ZO_TypeOfBusiness = "Importer of baby products";
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var organisationLoaded = factory2.Load<OrgHeader>(organisation.PK);
			var organisationWrapper2 = OrgHeaderWrapper.New(organisationLoaded);
			AssertEquals("Importer of baby products", organisationWrapper2.ZO_TypeOfBusiness);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			return new OrgImpAddInfo((ZPropertyInfoString)organisation.CountryData.OV_ImportCustomsDefaultAddInfoInfo);
		}
	}
}
