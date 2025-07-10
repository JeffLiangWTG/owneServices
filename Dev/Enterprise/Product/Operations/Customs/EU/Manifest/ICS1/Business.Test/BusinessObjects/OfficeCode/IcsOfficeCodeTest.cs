using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.Business.Testing
{
	[TestedType(typeof(IcsOfficeCode))]
	class IcsOfficeCodeTest : Customs.Business.Testing.CusCodeDataTest<IcsOfficeCode>
	{
		protected override IEnumerable<IcsOfficeCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var customsOffice = header.EUCustomsOffices.AddNew();
			customsOffice.CY_Data = "D";
			yield return customsOffice;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return Factory.NewWithValidTestData<AsycudaManifestHeader>().EUCustomsOffices.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<AsycudaManifestHeader>().EUCustomsOffices.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();

			applicationContext = ApplicationBusinessProviderTestContext.CreateDefaultIcsContext();
		}

		protected override void TearDown()
		{
			base.TearDown();

			applicationContext.Dispose();
		}

		ApplicationBusinessProviderTestContext applicationContext;
	}
}
