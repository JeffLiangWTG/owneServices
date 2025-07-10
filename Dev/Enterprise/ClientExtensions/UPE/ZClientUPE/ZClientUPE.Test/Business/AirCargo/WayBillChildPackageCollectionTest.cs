using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(WayBillChildPackageCollection))]
	internal class WayBillChildPackageCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestIndex()
		{
			UPEJobRelatedWayBill wayBill1 = Collection.AddNew();
			AssertEquals("Incorrect index", wayBill1, Collection[0]);
			UPEJobRelatedWayBill wayBill2 = Collection.AddNew();
			AssertEquals("Incorrect index", wayBill2, Collection[1]);
		}

		public void TestRelationshipFilter()
		{
			InsertJobRelatedWayBillRecord(ZGuid.NewZGuid(), JobRelatedWayBill.Constants.RelatedWayBillType.Child, "W123", CusHAWBSchema.Constants.Prefix); // Not included
			InsertJobRelatedWayBillRecord(HouseBill.PK, JobRelatedWayBill.Constants.RelatedWayBillType.Child, "W234", CusHAWBSchema.Constants.Prefix); // Included
			InsertJobRelatedWayBillRecord(HouseBill.PK, JobRelatedWayBill.Constants.RelatedWayBillType.Parent, "W345", CusHAWBSchema.Constants.Prefix); // Not Included
			InsertJobRelatedWayBillRecord(HouseBill.PK, JobRelatedWayBill.Constants.RelatedWayBillType.Parent, "W456", "11"); // Not Included
			Collection.Load();
			AssertEquals(1, Collection.Count);
			AssertEquals("W234", Collection[0].EB_WaybillNumber);
		}

		public void TestNewChildDefaults()
		{
			UPEJobRelatedWayBill wayBill = Collection.AddNew();
			AssertEquals(HouseBill.PK, wayBill.EB_ParentID);
			AssertEquals(CusHAWBSchema.Constants.Prefix, wayBill.EB_ParentTableCode);
			AssertEquals(JobRelatedWayBill.Constants.RelatedWayBillType.Child, wayBill.EB_WaybillType);
		}

		#region Implementation
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WayBillChildPackageCollection(HouseBill);
		}

		void InsertJobRelatedWayBillRecord(ZGuid parentID, ZString wayBillType, ZString wayBillNumber, ZString tablePrefix)
		{
			JobRelatedWayBill wayBill = Factory.New<JobRelatedWayBill>();
			wayBill.SuspendValidation();
			wayBill.EB_ParentID = parentID;
			wayBill.EB_WaybillType = wayBillType;
			wayBill.EB_WaybillNumber = wayBillNumber;
			wayBill.EB_ParentTableCode = tablePrefix;
		}

		new WayBillChildPackageCollection Collection
		{
			get
			{
				return (WayBillChildPackageCollection)base.Collection;
			}
		}

		UPECusHAWB HouseBill
		{
			get
			{
				if (fHouseBill == null)
				{
					fHouseBill = Factory.New<UPECusHAWB>();
				}

				return fHouseBill;
			}
		}

		UPECusHAWB fHouseBill;
		#endregion
	}
}
