using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPECusHAWBCollection))]
	public class UPECusHAWBCollectionTest : BusinessObjectCollectionTestCase
	{
		public override void TestSuspendCountChanged()
		{
			Assert(true);
		}

		public void TestIndexor()
		{
			UPECusHAWBCollection.AddNew();
			AssertEquals(typeof(UPECusHAWB), UPECusHAWBCollection[0].GetType());
		}

		public void TestNewAddNew()
		{
			AssertEquals(typeof(UPECusHAWB), UPECusHAWBCollection.AddNew().GetType());
		}

		public void TestNewdAddNew_WithBizOType()
		{
			AssertEquals(typeof(TestUPECusHAWB), UPECusHAWBCollection.AddNew(typeof(TestUPECusHAWB)).GetType());
		}

		public void TestAllowNew()
		{
			AssertEquals(false, UPECusHAWBCollection.AllowNew);
		}

		public override void TestRemoveFromRelationship()
		{
			UPECusHAWB uPECusHAWB = UPECusHAWBCollection.AddNew();
			UPECusHAWBCollection.Remove(uPECusHAWB);
			AssertEquals("Remove Not Allowed", 1, UPECusHAWBCollection.Count);
		}

		public void TestRemoveAndDeleteAll()
		{
			UPECusHAWB uPECusHAWB = UPECusHAWBCollection.AddNew();
			UPECusHAWBCollection.RemoveAndDeleteAll();
			AssertEquals("Remove Not Allowed", 1, UPECusHAWBCollection.Count);
		}

		public override void TestDelete()
		{
			UPECusHAWB uPECusHAWB = UPECusHAWBCollection.AddNew();
			UPECusHAWBCollection.RemoveAndDelete(uPECusHAWB);
			AssertEquals("Remove Not Allowed", 1, UPECusHAWBCollection.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new UPECusHAWBCollection(Factory);
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			UPECusHAWBCollection = new UPECusHAWBCollection(Factory);
		}

		UPECusHAWBCollection UPECusHAWBCollection;
		class TestUPECusHAWB : UPECusHAWB
		{
			public TestUPECusHAWB(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
		}
	}
}
