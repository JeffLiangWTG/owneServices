using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusSCAContainerCollectionForOceanBill))]
	sealed class CusSCAContainerCollectionForOceanBillTest : ActiveBusinessObjectCollectionTestCase<CusSCAContainerCollectionForOceanBill>
	{
		#region Implementation

		CusSCAOceanBill cusSCAOceanBill;
		CusSCAOceanBill CusSCAOceanBill
		{
			get
			{
				if (cusSCAOceanBill == null)
				{
					cusSCAOceanBill = Factory.New<CusSCAOceanBill>();
				}
				return cusSCAOceanBill;
			}
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CusSCAContainer>();
		}

		protected override CusSCAContainerCollectionForOceanBill GetCollectionToTest()
		{
			return new CusSCAContainerCollectionForOceanBill(CusSCAOceanBill);
		}

		#endregion
	}
}
