using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(BillContainerCollection))]
	public class BillContainerCollectionTest : Customs.Business.Testing.BaseHouseBillContainerCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new BillContainerCollection(HouseBill);
		}

		protected new BillContainerCollection Containers
		{
			get
			{
				return (BillContainerCollection)base.Containers;
			}
		}

		protected override Customs.Business.BaseBillContainerCollection CreateContainers()
		{
			return new BillContainerCollection(HouseBill);
		}

		protected new Bill HouseBill
		{
			get
			{
				return (Bill)base.HouseBill;
			}
		}
	}
}
