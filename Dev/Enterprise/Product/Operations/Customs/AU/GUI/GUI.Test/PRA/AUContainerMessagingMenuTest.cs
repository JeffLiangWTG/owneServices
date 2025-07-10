using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using MessageType = Enterprise.Customs.Common.AU.PRAMessageTypeConstants.MessageType;

namespace Enterprise.Customs.AU.PRA.GUI.Testing
{
	sealed class AUContainerMessagingMenuTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestConstruct()
		{
			using (AUContainerMessagingMenu testMenu = new AUContainerMessagingMenu())
			{
			}
		}

		public void TestCFSContainerWorks()
		{
			using (TestPRAMenu menu = new TestPRAMenu())
			{
				PRADataBuilder testDataBuilderForPRA = new PRADataBuilder(Factory);
				menu.PRAContainer = testDataBuilderForPRA.GetCFSContainer(MessagePartyPairList.Codes.PandOSydneyPortBotany, "AU", "EMC", "AU", "ABC Vessel", "1234567", "Voyage", "USLAX", "USLAX", "123456", "CONT789021", "1234", "APRD", "4000", 5000m, 2000m, "12345");
				AssertEquals("Menu.HasBeenSent before Submit", false, menu.HasBeenSent);
				menu.ExposedSendMessage(MessageType.Submit);
				AssertEquals("Menu.HasBeenSent after Submit", true, menu.HasBeenSent);
			}
		}

		public void TestFreightContainerWorks()
		{
			using (TestPRAMenu menu = new TestPRAMenu())
			{
				PRADataBuilder testDataBuilderForPRA = new PRADataBuilder(Factory);
				menu.PRAContainer = testDataBuilderForPRA.GetFreightContainer(MessagePartyPairList.Codes.PandOSydneyPortBotany, "AU", "123", "AU", "ABC Vessel", "1234567", "Voyage", "USLAX", "USLAX", "123456", "CONT789021", "1234", "APRD", "4000", 5000m, 2000m, "12345");
				AssertEquals("Menu.HasBeenSent before Submit", false, menu.HasBeenSent);
				menu.ExposedSendMessage(MessageType.Submit);
				AssertEquals("Menu.HasBeenSent after Submit", true, menu.HasBeenSent);
			}
		}

		public void TestFreightContainerToQBPHD()
		{
			using (TestPRAMenu menu = new TestPRAMenu())
			{
				PRADataBuilder testDataBuilderForPRA = new PRADataBuilder(Factory);
				menu.PRAContainer = testDataBuilderForPRA.GetFreightContainer(MessagePartyPairList.Codes.PortOfHedland, "AU", "123", "AU", "ABC Vessel", "1234567", "Voyage", "USLAX", "USLAX", "123456", "CONT789021", "1234", "APRD", "4000", 5000m, 2000m, "12345");
				AssertEquals("Menu.HasBeenSent before Submit", false, menu.HasBeenSent);
				menu.ExposedSendMessage(MessageType.Submit);
				AssertEquals("Menu.HasBeenSent after Submit", true, menu.HasBeenSent);
			}
		}

		public void TestCustomsContainerWorks()
		{
			using (TestPRAMenu menu = new TestPRAMenu())
			{
				PRADataBuilder testDataBuilderForPRA = new PRADataBuilder(Factory);
				menu.PRAContainer = testDataBuilderForPRA.GetCustomsContainer(MessagePartyPairList.Codes.PandOSydneyPortBotany, "AU", "EMC", "AU", "ABC Vessel", "1234567", "Voyage", "USLAX", "USLAX", "AUSYD", "33333", "123456", "CONT789021", "1234", "APRD", "4000", 5000m, 8000m, "12345");
				AssertEquals("Menu.HasBeenSent before Submit", false, menu.HasBeenSent);
				menu.ExposedSendMessage(MessageType.Submit);
				AssertEquals("Menu.HasBeenSent after Submit", true, menu.HasBeenSent);
			}
		}

		public void TestConcurrencyErrorDoesNotCauseException()
		{
			using (TestPRAMenu menu = new TestPRAMenu())
			{
				menu.DoActualBuild = true;
				menu.ForceConcurrencyError = true;
				PRADataBuilder testDataBuilderForPRA = new PRADataBuilder(Factory);
				CusContainer container = testDataBuilderForPRA.GetCustomsContainer(MessagePartyPairList.Codes.PandOSydneyPortBotany, "AU", "EMC", "AU", "ABC Vessel", "1234567", "Voyage", "USLAX", "USLAX", "AUSYD", "33333", "123456", "CONT789021", "1234", "APRD", "4000", 5000m, 8000m, "12345");
				menu.PRAContainer = container;
				Factory.Save();
				menu.ExposedSendMessage(MessageType.Submit);
				container.Messages.Load();
				AssertEquals("Message has not been generated", 0, container.Messages.Count);
				menu.ForceConcurrencyError = false;
				menu.ExposedSendMessage(MessageType.Submit);
				container.Messages.Load();
				AssertEquals("Message has been generated", 1, container.Messages.Count);
				Assert("Message is in database", container.Messages[0].IsInDatabase);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "DEVELOPER COMPANY";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			branch.GB_OH_OrgProxy = org.PK;
			Factory.Save();
			GlbStaff.CurrentUser.GS_FullName = "Developer";
			GlbStaff.CurrentUser.GS_EmailAddress = "Developer@edi.com.au";
			GlbStaff.CurrentUser.GS_GB_HomeBranch = branch.PK;
		}

		sealed class TestPRAMenu : AUContainerMessagingMenu
		{
			public TestPRAMenu() : base()
			{
				HasBeenSent = false;
			}

			public bool ForceConcurrencyError;

			public bool DoActualBuild;

			public bool HasBeenSent;

			public void ExposedSendMessage(MessageType messageType) => SendMessage(messageType);

			protected override EDIMessage BuildMessageAndPostIt(IPRAMessagingData dataLayer, MessageType messageType)
			{
				if (DoActualBuild)
				{
					if (ForceConcurrencyError)
					{
						BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
						anotherFactory.RefreshEnabled = false;
						CusContainer container = anotherFactory.Load<CusContainer>(((CusContainer)PRAContainer).PK);
						container.CO_Seal = "A";
						anotherFactory.Save();
						((CusContainer)PRAContainer).CO_Seal = "B";
					}

					return base.BuildMessageAndPostIt(dataLayer, messageType);
				}
				else
				{
					HasBeenSent = true;
					return null;
				}
			}
		}
	}
}
