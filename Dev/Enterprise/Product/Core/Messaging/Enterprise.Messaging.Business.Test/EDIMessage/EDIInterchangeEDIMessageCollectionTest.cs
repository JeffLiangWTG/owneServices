using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Messaging.Testing
{
	[TestedType(typeof(EDIInterchangeEDIMessageCollection))]
	public class EDIInterchangeEDIMessageCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestConstructor()
		{
			AssertNotNull(MessageCollection);
		}

		public void TestDefault()
		{
			var message = MessageCollection.AddNew(typeof(EDIMessage));
			AssertEquals("EM_EI", Interchange.PK, message.EM_EI);
		}

		public void TestOnAdded()
		{
			Interchange.EI_ApplicationCode = "123";
			Interchange.EI_TransportType = "EAD";
			Interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			var message = MessageCollection.AddNew(typeof(EDIMessage));
			AssertEquals("EM_ApplicationCode", Interchange.EI_ApplicationCode, message.EM_ApplicationCode);
			AssertEquals("EM_GB", Interchange.EI_GB, message.EM_GB);
			AssertEquals("EM_GE", GlbDepartment.CurrentDepartment.PK, message.EM_GE);

			AssertEquals("EM_GE", Interchange.EI_ApplicationCode, message.EM_MessageType);
			AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);

			AssertEquals("EM_MessageSubType", "XXX", message.EM_MessageSubType);
			AssertEquals("EM_TransportType", "EAD", message.EM_TransportType);
		}

		#region Implementaiton

		protected override void SetUp()
		{
			base.SetUp();
			Interchange = GetNewInterchange();
			Interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			MessageCollection = GetNewMessageCollection(Interchange);
		}

		protected virtual EDIInterchange GetNewInterchange()
		{
			return Factory.New<EDIInterchange>();
		}

		protected virtual EDIInterchangeEDIMessageCollection GetNewMessageCollection(EDIInterchange interchange)
		{
			return new EDIInterchangeEDIMessageCollection(interchange, Factory);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var interchange = GetNewInterchange();
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			return GetNewMessageCollection(interchange);
		}

		EDIInterchange Interchange;
		EDIInterchangeEDIMessageCollection MessageCollection;

		#endregion
	}
}
