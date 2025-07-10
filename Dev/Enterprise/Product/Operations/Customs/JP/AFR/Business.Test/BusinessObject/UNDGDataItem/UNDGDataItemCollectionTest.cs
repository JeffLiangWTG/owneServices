using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(UNDGDataItemCollection))]
	sealed class UNDGDataItemCollectionTest : ActiveBusinessObjectCollectionTestCase<UNDGDataItemCollection>
	{
		protected override UNDGDataItemCollection GetCollectionToTest()
		{
			return new UNDGDataItemCollection(Bill);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var item = Factory.New<UNDGDataItem>();
			item.DI_ParentTableCode = Bill.TablePrefix;
			item.DI_ParentID = Bill.PK;

			return item;
		}

		JPAFRBills Bill
		{
			get
			{
				if (bill == null)
				{
					var header = Factory.New<JPAFRHeader>();
					bill = header.Bills.AddNew();
				}

				return bill;
			}
		}
		JPAFRBills bill;
	}
}
