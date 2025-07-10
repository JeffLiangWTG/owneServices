using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class CARSTBusinessObjectLoaderOrCreatorTest : TestCaseWithFactory
	{
		public void TestLoadOrCreateRecordForMessage()
		{
			Creator.fIsInterestedInCARST = false;
			AssertNull("LoadOrCreateRecordForMessage", Creator.LoadOrCreateRecordForMessage(Message));
			Creator.fIsInterestedInCARST = true;
			AssertNotNull("LoadOrCreateRecordForMessage", Creator.LoadOrCreateRecordForMessage(Message));
		}

		public void TestLinkMessageOrCloneOfMessageToBusinessObjectLinks()
		{
			DummyCMRMessageRespondee bizo = Factory.New<DummyCMRMessageRespondee>();
			Creator.LinkMessageOrCloneOfMessageToBusinessObject(bizo, Message);
			AssertEquals("EM_LinkUniqueID", bizo.PK, Message.EM_LinkUniqueID);
		}

		public void TestLinkMessageOrCloneOfMessageToBusinessObjectClones()
		{
			DummyCMRMessageRespondee bizo1 = Factory.New<DummyCMRMessageRespondee>();
			DummyCMRMessageRespondee bizo2 = Factory.New<DummyCMRMessageRespondee>();
			Creator.LinkMessageOrCloneOfMessageToBusinessObject(bizo1, Message);
			AssertEquals("EM_LinkUniqueID", bizo1.PK, Message.EM_LinkUniqueID);
			Creator.LinkMessageOrCloneOfMessageToBusinessObject(bizo2, Message);
			AssertEquals("EM_LinkUniqueID", bizo1.PK, Message.EM_LinkUniqueID);
			AssertEquals("Bizo2 Message Count", 1, Factory.Load(typeof(CMRCARSTMessage), new ZQuery(EDIMessageSchema.EM_LinkUniqueID, bizo2.PK)).Length);
		}

		#region Implementation

		CMRCARSTMessage fMessage;
		CMRCARSTMessage Message
		{
			get
			{
				if (fMessage == null)
				{
					fMessage = Factory.New<CMRCARSTMessage>();
				}
				return fMessage;
			}
		}

		TestHelperCARSTBusinessObjectLoaderOrCreator fCreator;
		TestHelperCARSTBusinessObjectLoaderOrCreator Creator
		{
			get
			{
				if (fCreator == null)
				{
					fCreator = new TestHelperCARSTBusinessObjectLoaderOrCreator();
				}
				return fCreator;
			}
		}

		class TestHelperCARSTBusinessObjectLoaderOrCreator : CARSTBusinessObjectLoaderOrCreator
		{
			protected internal override bool IsInterestedInCARST(CMRCARSTMessage message)
			{
				return fIsInterestedInCARST;
			}

			public bool fIsInterestedInCARST;

			protected internal override BusinessObject[] LoadOrCreateRecordForMessageCore(CMRCARSTMessage message)
			{
				return new BusinessObject[] { message.Factory.New<DummyCMRMessageRespondee>() };
			}
		}

		#endregion

	}
}
