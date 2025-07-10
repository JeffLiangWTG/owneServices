using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.Testing
{
	[TestedType(typeof(ChiefRelatedConsolCollection))]
	class ChiefRelatedConsolCollectionTests : ActiveBusinessObjectCollectionTestCase<ChiefRelatedConsolCollection>
	{
		public void TestCollectionCountChange()
		{
			// This is here and not in the "right" place becasue the CW.EntityFrameWork can't see GenPivot
			var coll = GetCollectionToTest();
			coll.CollectionCountChange += new CollectionCountChangedEventHandler(CountChange_handler);
			var child = (ForwardingConsol)GetNewElementToAddToTheCollection();
			child.JK_AgentsReference = "POO";
			coll.AddRange(new BusinessObject[] { child });
			AssertEquals(true, added);
			AssertEquals(false, removed);
			AssertEquals("POO", keyToCheckCorrectObject);
			keyToCheckCorrectObject = "";
			coll.RemoveFromRelationship(child);
			AssertEquals(false, added);
			AssertEquals(true, removed);
			AssertEquals("POO", keyToCheckCorrectObject);
		}

		protected override ChiefRelatedConsolCollection GetCollectionToTest()
		{
			var parent = Factory.New<ForwardingConsol>();
			parent.JK_RL_NKLoadPort = "GBLHR";
			parent.JK_RL_NKDischargePort = "USATL";
			return new ChiefRelatedConsolCollection(parent);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var child = Factory.New<ForwardingConsol>();
			child.JK_RL_NKLoadPort = "GBLHR";
			child.JK_RL_NKDischargePort = "USATL";
			return child;
		}

		bool added;
		bool removed;
		string keyToCheckCorrectObject;

		void CountChange_handler(object s, CollectionCountChangedEventArgs e)
		{
			added = e.ItemAdded;
			removed = e.ItemRemoved;
			keyToCheckCorrectObject = ((ForwardingConsol)e.BizObject).JK_AgentsReference;
		}

		public void TestAllowNew()
		{
			var coll = new ChiefRelatedConsolCollectionForTest(Factory.New<ForwardingConsol>());
			Assert(!coll.AllowNewExposed);
		}

		class ChiefRelatedConsolCollectionForTest : ChiefRelatedConsolCollection
		{
			public ChiefRelatedConsolCollectionForTest(ForwardingConsol parent)
				: base(parent)
			{ }

			public bool AllowNewExposed
			{
				get { return base.AllowNew; }
			}
		}
	}
}
