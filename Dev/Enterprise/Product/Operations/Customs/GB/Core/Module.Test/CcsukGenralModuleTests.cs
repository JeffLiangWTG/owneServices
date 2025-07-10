using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Testing;
using Enterprise.Customs.GB.Module.CcsukAirInventoryCombinedMasterAndHouse.Testing;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Module.Testing
{
	class CcsukGenralMessageControllerTest : ZControllerTest
	{
		public void TestSecurityCheckpoints()
		{
			var controller = new CcsukGenralMessageController();
			var npbo = new NonPersistentGenralEdiMessageForNew(Factory);
			var editableEdiEMessage = Factory.New<GenralEdiMessage>();
			AssertEquals(Env.Security.AirCcsukGenral, controller.GetCheckPointForDelete(editableEdiEMessage));
			AssertEquals(Env.Security.AirCcsukGenralNew, controller.GetCheckPointForNew(npbo));
			AssertEquals(Env.Security.AirCcsukGenralView, controller.GetCheckPointForView(editableEdiEMessage));
			AssertEquals(Env.Security.AirCcsukGenral, controller.GetCheckPointForEdit(editableEdiEMessage));
		}
	}

	class CcsukGenralMessageModuleTest : ZModuleTest
	{
	}

	[TestedType(typeof(CcsukGenralMessageFilterStripBusinessObject))]
	class CcsukGenralMessageFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestBranchAndCompanyLimit()
		{
			CcsukAirInventoryCombinedMasterAndHouseFilterStripBusinessObjectTest.RunTestBranchAndCompanyLimit<CcsukGenralMessageFilterStripBusinessObject, GenralEdiMessage>(
				delegate
				{
					var mock = Factory.NewMoq<GenralEdiMessageDummyForTest_123456>();
					mock.Protected().Setup<string>("GetMessageReferenceNumber").Returns("123465");
					return mock.Object;
				}, Factory, false);
		}

		public void TestLookups()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var stripBO = (CcsukGenralMessageFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var pairs = new Dictionary<string, string>();
			pairs.Add("Direction", "RCV");
			pairs.Add("Local Profile", "CUKFFW98000LXA");
			pairs.Add("Purpose", "FBK");
			pairs.Add("Status", "SNT");
			foreach (var pair in pairs)
			{
				var filter = (ModuleTextFilter)stripBO[pair.Key];
				AssertNotNull(filter.List);
				Assert(pair.Key + " list should contain " + pair.Value, ((CodeDescriptionPairList)filter.List).ContainsCode(pair.Value));
			}
		}

		public void TestSenderAndRecipient()
		{
			var stripBO = (CcsukGenralMessageFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var localFilter = (ModuleTextFilter)stripBO["Local Profile"];
			var otherPartyFilter = (ModuleTextFilter)stripBO["PIMA"];
			SetUpTestMessages();
			ediMessage1.EM_MessageOwner = "DJC";
			ediMessage1.EM_ApplicationReference = "LSM";
			ediMessage2.EM_MessageOwner = "LSM";
			ediMessage2.EM_ApplicationReference = "DJC";
			Factory.Save();
			var coll = new GenralEdiMessageCollection(Factory);
			localFilter.Property = "LSM";
			localFilter.IsActive = true;
			otherPartyFilter.Property = "DJC";
			otherPartyFilter.IsActive = true;
			coll.Load(stripBO.Filter);
			Assert("Coll limited to LSM-->DJC", !coll.Contains(ediMessage1));
			Assert("Coll limited to LSM-->DJC", coll.Contains(ediMessage2));

			localFilter.Property = "DJC";
			otherPartyFilter.Property = "LSM";
			coll.Load(stripBO.Filter);
			Assert("Coll limited to DJC-->LSM", coll.Contains(ediMessage1));
			Assert("Coll limited to DJC-->LSM", !coll.Contains(ediMessage2));
		}

		public void TestRelatedMessageQuery()
		{
			var stripBO = (CcsukGenralMessageFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var flagFilter = (ModuleFlagsFilter)stripBO["Has related message"];
			AssertNotNull(flagFilter);

			SetUpTestMessages();
			Factory.Save();

			var coll = new GenralEdiMessageCollection(Factory);
			coll.Load(stripBO.Filter);
			Assert("Coll without limits contains both objects", coll.Contains(ediMessage2));
			Assert("Coll without limits contains both objects", coll.Contains(ediMessage1));

			flagFilter.Property0 = true;
			flagFilter.IsActive = true;
			coll.Load(stripBO.Filter);
			Assert("Coll limited to Linked=yes shows only the linked message", !coll.Contains(ediMessage2));
			Assert("Coll limited to Linked=yes shows only the linked message", coll.Contains(ediMessage1));

			flagFilter.Property0 = false;
			flagFilter.IsActive = true;
			coll.Load(stripBO.Filter);
			Assert("Coll limited to Linked=no shows only the unlinked message", coll.Contains(ediMessage2));
			Assert("Coll limited to Linked=no shows only the unlinked message", !coll.Contains(ediMessage1));
		}

		void SetUpTestMessages()
		{
			var mockMessage1 = Factory.NewMoq<GenralEdiMessageDummyForTest_123>();
			mockMessage1.Protected().Setup<string>("GetMessageReferenceNumber").Returns("123");
			ediMessage1 = mockMessage1.Object;
			ediMessage1.EM_LinkTable = EDIMessage.Schema.TableName;
			ediMessage1.EM_LinkUniqueID = new CargoWise.Types.ZGuid(Guid.NewGuid());
			var mockMessage2 = Factory.NewMoq<GenralEdiMessageDummyForTest_1234>();
			mockMessage2.Protected().Setup<string>("GetMessageReferenceNumber").Returns("1234");
			ediMessage2 = mockMessage2.Object;
			ediMessage2.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			ediMessage1.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CcsukGenralMessageFilterStripBusinessObject();
		}

		GenralEdiMessage ediMessage1;
		GenralEdiMessage ediMessage2;
	}

	[TestedType(typeof(CcsukGenralMessageModule))]
	class CcsukGenralModuleBasherTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Customs.EU.GB.CcsukGenralMessage;
		}

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedKingdom;
	}

	[TestedType(typeof(CcsukGenralMessageController))]
	class CcsukGenralControllerBasherTest : ZControllerBasherTest
	{
		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.UnitedKingdom; }
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(GenralEdiMessage);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.GB.CcsukGenralMessage;
		}
	}

	[TestedType(typeof(GenralEdiMessageDummyForTest_123))]
	public class GenralEdiMessageDummyForTest_123Test : EnterpriseBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestGenralEdiMessageDummy()
		{
			_ = Factory.NewMoq<GenralEdiMessageDummyForTest_123>();
			Factory.Save();
		}
	}

	[TestedType(typeof(GenralEdiMessageDummyForTest_1234))]
	public class GenralEdiMessageDummyForTest_1234Test : EnterpriseBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestGenralEdiMessageDummy()
		{
			_ = Factory.NewMoq<GenralEdiMessageDummyForTest_1234>();
			Factory.Save();
		}
	}

	[TestedType(typeof(GenralEdiMessageDummyForTest_123456))]
	public class GenralEdiMessageDummyForTest_123456Test : EnterpriseBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestGenralEdiMessageDummy()
		{
			_ = Factory.NewMoq<GenralEdiMessageDummyForTest_123456>();
			Factory.Save();
		}
	}

	public class GenralEdiMessageDummyForTest_123 : GenralEdiMessage
	{
		public GenralEdiMessageDummyForTest_123(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		protected override string GetMessageReferenceNumber()
		{
			return "123";
		}
	}

	public class GenralEdiMessageDummyForTest_1234 : GenralEdiMessage
	{
		public GenralEdiMessageDummyForTest_1234(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		protected override string GetMessageReferenceNumber()
		{
			return "1234";
		}
	}

	public class GenralEdiMessageDummyForTest_123456 : GenralEdiMessage
	{
		public GenralEdiMessageDummyForTest_123456(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		protected override string GetMessageReferenceNumber()
		{
			return "123456";
		}
	}
}
