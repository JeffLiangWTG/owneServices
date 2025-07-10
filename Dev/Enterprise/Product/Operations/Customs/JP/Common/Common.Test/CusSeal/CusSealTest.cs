using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(CusSeal))]
	class CusSealTest : EnterpriseBusinessObjectTestCase
	{
		public void TestBK_SealNumber()
		{
			var seal = Factory.New<CusSeal>();
			var propertyInfo = DataBoundResourceStrings.GetDataForProperty(seal.BK_SealNumberInfo);
			AssertEquals("Seal Number", propertyInfo.Caption);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => Seal;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => Seal;

		BaseCusContainer Container => container ??= Factory.NewWithValidTestData<BaseCusContainer>();
		BaseCusContainer container;

		CusSealCollection<CusSeal> SealCollection => sealCollection ??= new CusSealCollection<CusSeal>(Container, 4);
		CusSealCollection<CusSeal> sealCollection;

		CusSeal Seal
		{
			get
			{
				if (seal == null)
				{
					seal = SealCollection.AddNew();
					seal.BK_SealNumber = "Seal1";
				}

				return seal;
			}
		}
		CusSeal seal;
	}
}
