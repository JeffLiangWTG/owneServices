using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Messaging.Business
{
	[TestedType(typeof(EDIMessageCollectionView))]
	sealed class EDIMessageCollectionView_Test : CargoWise.EntityFramework.Testing.BusinessObjectCollectionViewTestCase<EDIMessageCollectionView>
	{
		public void TestMessageOfApplicationCode()
		{
			var filter = new ZQuery();
			filter.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.CMR);
			var view = new EDIMessageCollectionView(MessageCollection, filter);
			AssertEquals("ViewCount", 3, view.Count);
		}

		public void TestMessageOfApplicationCodeAndMessageType()
		{
			var filter = new ZQuery();
			filter.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.CMR);
			filter.AddToFilter(EDIMessageSchema.EM_MessageType, "ESM");

			var view = new EDIMessageCollectionView(MessageCollection, filter);
			AssertEquals("ViewCount", 2, view.Count);
		}

		public void TestMessageOfApplicationCodeAndMessageTypeAndDirection()
		{
			var filter = new ZQuery();
			filter.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.CMR);
			filter.AddToFilter(EDIMessageSchema.EM_MessageType, "ESM");
			filter.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);

			var view = new EDIMessageCollectionView(MessageCollection, filter);
			AssertEquals("ViewCount", 1, view.Count);
			AssertEquals("View[0]", CMRESMMessage1, view[0]);
		}

		#region Implementation

		protected override EDIMessageCollectionView GetCollectionToTest()
		{
			var master = Factory.New<EDIMessage>();
			var messageCollection = new EDIMessageCollection(master, Factory);
			return new EDIMessageCollectionView(messageCollection, new ZQuery());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<EDIMessage>();
		}

		protected override void SetUp()
		{
			ZString cMRApplicationCode = "CMR";
			ZString eMMMessageType = "EMM";
			ZString eSMMessageType = "ESM";
			base.SetUp();
			Master = Factory.New<EDIMessage>();
			MessageCollection = new EDIMessageCollection(Master, Factory);
			CMRESMMessage1 = MessageCollection.AddNew();
			CMRESMMessage1.EM_ApplicationCode = cMRApplicationCode;
			CMRESMMessage1.EM_MessageType = eSMMessageType;
			CMRESMMessage1.EM_ReceiveTransmit = "TRX";

			CMRESMMessage2 = MessageCollection.AddNew();
			CMRESMMessage2.EM_ApplicationCode = cMRApplicationCode;
			CMRESMMessage2.EM_MessageType = eSMMessageType;
			CMRESMMessage2.EM_ReceiveTransmit = "RCV";

			CMREMMMessage = MessageCollection.AddNew();
			CMREMMMessage.EM_ApplicationCode = cMRApplicationCode;
			CMREMMMessage.EM_MessageType = eMMMessageType;
			CMREMMMessage.EM_ReceiveTransmit = "RCV";

			EdificeMessage = MessageCollection.AddNew();
			EdificeMessage.EM_ReceiveTransmit = "RCV";
		}

		EDIMessage CMRESMMessage1;
		EDIMessage CMRESMMessage2;
		EDIMessage CMREMMMessage;
		EDIMessage EdificeMessage;

		EDIMessageCollection MessageCollection;
		EDIMessage Master;

		#endregion

	}
}
