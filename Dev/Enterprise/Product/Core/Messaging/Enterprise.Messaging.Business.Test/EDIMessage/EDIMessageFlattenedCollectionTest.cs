using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Messaging.Business.Testing
{
	[TestedType(typeof(EDIMessageFlattenedCollection))]
	public class EDIMessageFlattenedCollectionTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionTestCase
	{
		public void TestConstructor()
		{
			AssertNotNull(FlattenedCollection);
		}

		public void TestCollectionContainsAllMessagesInBaseCollectionWhenConstructed()
		{
			BaseCollection.AddNew();
			BaseCollection.AddNew();
			BaseCollection.AddNew();
			AssertNull(fFlattenedCollection);
			AssertEquals("ElementsInCollection", 3, FlattenedCollection.Count);
		}

		public void TestAddingAnElementToBaseCollectionAddsOneToCollection()
		{
			BaseCollection.AddNew();
			AssertEquals("ElementsInCollection", 1, FlattenedCollection.Count);
			BaseCollection.AddNew();
			AssertEquals("ElementsInCollection", 2, FlattenedCollection.Count);
		}

		public void TestRemovingAnElementToBaseCollectionRemovesOneFromCollection()
		{
			BaseCollection.AddNew();
			BaseCollection.AddNew();
			AssertEquals("ElementsInCollection", 2, FlattenedCollection.Count);
			BaseCollection.Remove(BaseCollection[1]);
			AssertEquals("ElementsInCollection", 1, FlattenedCollection.Count);
		}

		public void TestAddingMessageWithInterchangeResponse()
		{
			BaseCollection.AddNew();
			BaseCollection.AddNew();
			AssertEquals("ElementsInCollection", 2, FlattenedCollection.Count);
			BaseCollection.Add(MessageWithAnInterchangeAcknowledgement);
			AssertEquals("ElementsInCollection", 4, FlattenedCollection.Count);
		}

		public void TestLoadLeavesTheCollectionIntact()
		{
			BaseCollection.AddNew();
			BaseCollection.AddNew();
			BaseCollection.Add(MessageWithAnInterchangeAcknowledgement);
			AssertEquals("ElementsInCollection", 4, FlattenedCollection.Count);
			FlattenedCollection.Load();
			AssertEquals("ElementsInCollection", 4, FlattenedCollection.Count);
		}

		public void TestRemovingAMessageWithInterchangeResponse()
		{
			BaseCollection.AddNew();
			BaseCollection.AddNew();
			AssertEquals("ElementsInCollection", 2, FlattenedCollection.Count);
			BaseCollection.Add(MessageWithAnInterchangeAcknowledgement);
			AssertEquals("ElementsInCollection", 4, FlattenedCollection.Count);
			BaseCollection.Remove(MessageWithAnInterchangeAcknowledgement);
			AssertEquals("ElementsInCollection", 2, FlattenedCollection.Count);
		}

		public void TestDontAddAcknowledgementsToARejectionOnlyFlattenedCollection()
		{
			var collection = new EDIMessageFlattenedCollection(BaseCollection, new ZQuery(EDIMessageSchema.EM_MessageSubType, nameof(Core.Constants.AUCTLMessageSubType.REJ)), ZString.Empty);
			BaseCollection.Add(MessageWithAnInterchangeAcknowledgement);
			AssertEquals("ElementsInCollection", 1, collection.Count);
			BaseCollection.Add(MessageWithAnInterchangeRejection);
			AssertEquals("ElementsInCollection", 3, collection.Count);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return FlattenedCollection;
		}

		EDIMessageFlattenedCollection FlattenedCollection
		{
			get
			{
				if (fFlattenedCollection == null)
				{
					fFlattenedCollection = new EDIMessageFlattenedCollection(BaseCollection, null, ZString.Empty);
				}
				return fFlattenedCollection;
			}
		}
		EDIMessageFlattenedCollection fFlattenedCollection;

		EDIMessageCollection BaseCollection
		{
			get
			{
				if (fBaseCollection == null)
				{
					fBaseCollection = new EDIMessageCollection(Master, Factory);
				}
				return fBaseCollection;
			}
		}
		EDIMessageCollection fBaseCollection;

		BusinessObject Master
		{
			get
			{
				if (fMaster == null)
				{
					fMaster = Factory.New(typeof(EDIMessage));
				}
				return fMaster;
			}
		}
		BusinessObject fMaster;

		EDIMessage MessageWithAnInterchangeAcknowledgement
		{
			get
			{
				if (fMessageWithAnInterchangeAcknowledgement == null)
				{
					fMessageWithAnInterchangeAcknowledgement = InterchangeWithAnAcknowledgement.ContainedMessages.AddNew();
				}
				return fMessageWithAnInterchangeAcknowledgement;
			}
		}
		EDIMessage fMessageWithAnInterchangeAcknowledgement;

		EDIInterchange InterchangeWithAnAcknowledgement
		{
			get
			{
				if (fInterchangeWithAnAcknowledgement == null)
				{
					fInterchangeWithAnAcknowledgement = Factory.New<EDIInterchange>();
					AddAcknowledgement(fInterchangeWithAnAcknowledgement);
				}
				return fInterchangeWithAnAcknowledgement;
			}
		}
		EDIInterchange fInterchangeWithAnAcknowledgement;

		EDIMessage MessageWithAnInterchangeRejection
		{
			get
			{
				if (fMessageWithAnInterchangeRejection == null)
				{
					fMessageWithAnInterchangeRejection = InterchangeWithARejection.ContainedMessages.AddNew();
				}
				return fMessageWithAnInterchangeRejection;
			}
		}
		EDIMessage fMessageWithAnInterchangeRejection;

		EDIInterchange InterchangeWithARejection
		{
			get
			{
				if (fInterchangeWithARejection == null)
				{
					fInterchangeWithARejection = Factory.New<EDIInterchange>();
					AddRejection(fInterchangeWithARejection);
				}
				return fInterchangeWithARejection;
			}
		}
		EDIInterchange fInterchangeWithARejection;

		void AddAcknowledgement(EDIInterchange interchange)
		{
			var ackInterchange = interchange.InterchangeAcknowledgementMessages.AddNew();
			ackInterchange.EM_MessageSubType = nameof(Core.Constants.AUCTLMessageSubType.ACK);
		}

		void AddRejection(EDIInterchange interchange)
		{
			var ackInterchange = interchange.InterchangeAcknowledgementMessages.AddNew();
			ackInterchange.EM_MessageSubType = nameof(Core.Constants.AUCTLMessageSubType.REJ);
		}

		#endregion
	}
}
