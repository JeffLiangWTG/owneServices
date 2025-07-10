using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(ARLMessage))]
	public class ARLMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestEM_MessageSubType()
		{
			var message = Factory.New<ARLMessage>();
			message.SetSystemDefinedValue(EDIMessage.Schema.XMLCustomsMessageType, new ZString(ARLMessageTypes.Codes.DailyNotice));
			AssertEquals("EM_MessageSubType", ARLMessageTypes.Codes.DailyNotice, message.EM_MessageSubType);
			message = Factory.New<ARLMessage>();
			message.EM_MessageSubType = ARLMessageTypes.Codes.StatementOfAccount;
			AssertEquals("EM_MessageSubType", ARLMessageTypes.Codes.StatementOfAccount, message.EM_MessageSubType);
		}

		public void TestIControllerIDProviderMembers()
		{
			var message = Factory.New<ARLMessage>();
			IControllerIDProvider provider = message;
			AssertEquals("ControllerID", ControllerIDs.Customs.CA.K84Reports, provider.ControllerID);
			AssertEquals("BusinessObjectPK", message.PK.ToGuid(), provider.BusinessObjectPK);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = (ARLMessage)GetNewBusinessObject();
			result.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			return result;
		}

		protected override bool CanPersistedObjectBeDeleted => false;

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<ARLMessage>();
		}

		public virtual void TestDefaultValues()
		{
			AssertEquals("EM_ApplicationCode", XmlEDIMessage.ApplicationCodes.UniversalDataMessaging, message.EM_ApplicationCode);
			AssertEquals("EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
			AssertEquals("ShouldShowInterpretation", true, message.ShouldShowInterpretation);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			message = (EDIMessage)GetNewBusinessObject();
			message.EM_MessageText = "Message Number = " + EDIMessage.MessageNumberPlaceHolder;
		}

		EDIMessage message;

		#endregion
	}
}
