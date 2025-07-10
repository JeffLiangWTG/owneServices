using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class XmlEDIMessageTypeDeciderTest : TestCaseWithFactory
	{
		public void TestTypeDecider()
		{
			var message = Factory.New<EDIMessage>();
			AssertEquals(typeof(XmlEDIMessage), decider.GetXmlEDIMessageType(((INeedRow)message).Row, Factory));
			message = Factory.New<EDIMessage>();
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			var genAddOn = Factory.New<GenAddOnColumn>();
			genAddOn.XA_Name = EDIMessage.Schema.XMLCustomsMessageType;
			genAddOn.XA_Data = UniversalEventMessageTypes.Codes.D4Notices;
			genAddOn.XA_ParentTableCode = EDIMessageSchema.Constants.Prefix;
			genAddOn.XA_ParentID = message.PK;
			AssertEquals(typeof(UniversalEventMessage), decider.GetXmlEDIMessageType(((INeedRow)message).Row, Factory));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			decider = new XmlEDIMessageTypeDecider();
		}

		XmlEDIMessageTypeDecider decider;

		#endregion

	}
}
