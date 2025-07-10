using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Messaging.Business.XmlMessaging.Testing
{
	[TestedType(typeof(XmlEDIInterchangeEDIMessageCollection))]
	public class XmlEDIInterchangeEDIMessageCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestConstructor()
		{
			AssertNotNull(MessageCollection);
		}

		public void TestDefault()
		{
			var message = MessageCollection.AddNew(typeof(XmlEDIMessage));
			AssertEquals("EM_EI", Interchange.PK, message.EM_EI);
		}

		public void TestOnAdded()
		{
			var message = MessageCollection.AddNew(typeof(XmlEDIMessage));
			AssertEquals("EM_ApplicationCode", Interchange.EI_ApplicationCode, message.EM_ApplicationCode);
			AssertEquals("EM_GB", Interchange.EI_GB, message.EM_GB);
			AssertEquals("EM_GE", GlbDepartment.CurrentDepartment.PK, message.EM_GE);
			AssertEquals("EM_Status", XmlEDIMessage.Status.Sent, message.EM_Status);
			AssertEquals("EI_TransportType", EDIInterchange.TransportType.eHub, Interchange.EI_TransportType);
		}

		public void TestOnAddedForTransportType()
		{
			Interchange.EI_TransportType = EDIInterchange.TransportType.eAdaptor;
			MessageCollection.AddNew(typeof(XmlEDIMessage));
			AssertEquals("EI_TransportType", EDIInterchange.TransportType.eAdaptor, Interchange.EI_TransportType);
		}

		#region Implementaiton

		protected override void SetUp()
		{
			base.SetUp();
			Interchange = GetNewInterchange();
			Interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			MessageCollection = GetNewMessageCollection(Interchange);
		}

		protected virtual XmlEDIInterchange GetNewInterchange()
		{
			return Factory.New<XmlEDIInterchange>();
		}

		protected virtual XmlEDIInterchangeEDIMessageCollection GetNewMessageCollection(XmlEDIInterchange interchange)
		{
			return new XmlEDIInterchangeEDIMessageCollection(interchange, Factory);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			XmlEDIInterchange interchange = GetNewInterchange();
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			return GetNewMessageCollection(interchange);
		}

		XmlEDIInterchange Interchange;
		XmlEDIInterchangeEDIMessageCollection MessageCollection;

		#endregion
	}
}
