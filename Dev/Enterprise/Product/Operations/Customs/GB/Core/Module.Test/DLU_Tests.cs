using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Chief.Messaging.DLU;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Module.DLU.Testing
{
	class DLU_Tests
	{
		[TestedType(typeof(DLUMessageFilterStripBusinessObject))]
		class DLUeMessageFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
		{
			public void TestOrganisationQuery()
			{
				var stripBO = (DLUMessageFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				var orgFilter = (ModuleGuidFilter)stripBO["Organisation"];
				AssertNotNull(orgFilter);

				var mockMessage1 = Factory.New<DLUMessageForTesting>();
				mockMessage1.GetMessageReferenceNumberReturns = "123";
				mockMessage1.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
				mockMessage1.OrgPK = Factory.NewWithValidTestData<OrgHeader>().PK;

				var mockMessage2 = Factory.New<DLUMessageForTesting>();
				mockMessage2.GetMessageReferenceNumberReturns = "456";
				mockMessage2.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
				mockMessage2.OrgPK = Factory.NewWithValidTestData<OrgHeader>().PK;
				Factory.Save();

				var messageCollection = new DLUMessageCollection(Factory);
				messageCollection.Load(stripBO.Filter);
				Assert("Coll without limits contains message 1", messageCollection.Contains(mockMessage1));
				Assert("Coll without limits contains message 2", messageCollection.Contains(mockMessage2));

				orgFilter.Property = mockMessage1.OrgPK;
				orgFilter.IsActive = true;  // Comment for reviewer: why isn't it active by default when you set its properties? Pffff, cost me 20 minutes trying to work this out. 
				messageCollection.Load(stripBO.Filter);
				Assert("Message collection with org filter active contains message 1 only", messageCollection.Contains(mockMessage1));
				Assert("Message collection with org filter active contains message 1 only", !messageCollection.Contains(mockMessage2));
			}

			public void TestRelatedMessageQuery()
			{
				var stripBO = (DLUMessageFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				var flagFilter = (ModuleFlagsFilter)stripBO["Has response"];
				AssertNotNull(flagFilter);
				var ediMessageWithLink = Factory.New<DLUMessageForTesting>();
				ediMessageWithLink.GetMessageReferenceNumberReturns = "123";
				ediMessageWithLink.EM_LinkTable = EDIMessage.Schema.TableName;
				ediMessageWithLink.EM_LinkUniqueID = new CargoWise.Types.ZGuid(Guid.NewGuid());

				var ediMessageWithNoLink = Factory.New<DLUMessageForTesting>();
				ediMessageWithNoLink.GetMessageReferenceNumberReturns = "456";
				ediMessageWithNoLink.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
				ediMessageWithLink.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
				Factory.Save();

				var messageCollection = new DLUMessageCollection(Factory);
				messageCollection.Load(stripBO.Filter);
				var message = "Message Collection without limits contains both objects";
				Assert(message, messageCollection.Contains(ediMessageWithNoLink));
				Assert(message, messageCollection.Contains(ediMessageWithLink));

				flagFilter.Property0 = true;
				flagFilter.IsActive = true;
				messageCollection.Load(stripBO.Filter);
				message = "Message Collection limited to Linked=yes shows only the linked message";
				Assert(message, !messageCollection.Contains(ediMessageWithNoLink));
				Assert(message, messageCollection.Contains(ediMessageWithLink));

				flagFilter.Property0 = false;
				flagFilter.IsActive = true;
				messageCollection.Load(stripBO.Filter);
				message = "Message Collection limited to Linked=no shows only the unlinked message";
				Assert(message, messageCollection.Contains(ediMessageWithNoLink));
				Assert(message, !messageCollection.Contains(ediMessageWithLink));
			}

			protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
			{
				return new DLUMessageFilterStripBusinessObject();
			}
		}

		[TestedType(typeof(DLUMessageModule))]
		class DLUMessageModuleBasherTest : ZModuleBasherTest
		{
			protected override ModuleIdentifier GetModuleID()
			{
				return ModuleIDs.Customs.EU.GB.DLUMessage;
			}

			protected override string CountryCode => Core.Constants.CountryCodes.UnitedKingdom;
		}

		[TestedType(typeof(DLUMessageController))]
		class DLUMessageControllerBasherTest : ZControllerBasherTest
		{
			public override Type ControllerToBashType
			{
				get { return typeof(DLUMessageController); }
			}

			protected override string CountryCode
			{
				get { return Core.Constants.CountryCodes.UnitedKingdom; }
			}

			protected override Type GetBusinessObjectType()
			{
				return typeof(Ccsuk.AirCargoInventory.BusinessObjects.Genral.GenralEdiMessage);
			}

			protected override ControllerID GetControllerID()
			{
				return ControllerIDs.Customs.GB.DLUController;
			}
		}

		sealed class DLUMessageForTesting : DLUMessage
		{
			public DLUMessageForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public string GetMessageReferenceNumberReturns { get; set; } = string.Empty;

			protected override string GetMessageReferenceNumber()
			{
				return GetMessageReferenceNumberReturns;
			}
		}
	}
}
