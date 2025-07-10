using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSCAPivotCollectionForHouseBill))]
	sealed class CusSCAPivotCollectionForHouseBillCollectionTest : BusinessObjectCollectionTestCase
	{
		CusSCAOceanBill OceanBill
		{
			get
			{
				if (fOceanBill == null)
				{
					fOceanBill = Factory.New<CusSCAOceanBill>();
				}
				return fOceanBill;
			}
		}
		CusSCAOceanBill fOceanBill;

		CusSCAHouse HouseBill
		{
			get
			{
				if (fHouseBill == null)
				{
					fHouseBill = OceanBill.HouseBills.AddNew();
				}
				return fHouseBill;
			}
		}
		CusSCAHouse fHouseBill;

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusSCAPivotCollectionForHouseBill(HouseBill);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CusSCAContainer container = OceanBill.Containers.AddNew();
			return container.Pivots.AddNew();
		}

		protected override void SetUp()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			base.SetUp();
		}
	}
}
