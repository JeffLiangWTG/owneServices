using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CommunicationStatusCollection))]
	sealed class CommunicationStatusCollectionTest : CodeDescriptionBoolCollectionAbstractTest<CommunicationStatusCollection>
	{
		public void TestGetClosedFromCode()
		{
			var collection = Collection;
			var element1 = collection.AddNew();
			element1.Code = "ABC";
			element1.Closed = true;
			var element2 = collection.AddNew();
			element2.Code = "XYZ";
			element2.Closed = false;

			AssertEquals(true, collection.GetClosedFromCode("ABC"));
			AssertEquals(false, collection.GetClosedFromCode("XYZ"));
			AssertEquals(false, collection.GetClosedFromCode("!@#"));
		}

		public override void TestDefaultBoolForNewChild()
		{
			AssertEquals(false, new CommunicationStatusCollection(false, false).AddNew().Bool);
			AssertEquals(false, new CommunicationStatusCollection(true, false).AddNew().Bool);
			AssertEquals(true, new CommunicationStatusCollection(false, true).AddNew().Bool);
			AssertEquals(true, new CommunicationStatusCollection(true, true).AddNew().Bool);
			AssertEquals(true, new CommunicationStatusCollection().AddNew().Bool);
		}

		#region Implementation

		protected override CommunicationStatusCollection GetCollectionToTest()
		{
			return new CommunicationStatusCollection(false, true);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CommunicationStatus();
		}

		#endregion
	}
}
