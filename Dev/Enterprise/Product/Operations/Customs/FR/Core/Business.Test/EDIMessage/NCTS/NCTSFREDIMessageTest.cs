using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	[TestedType(typeof(NCTSFREDIMessage))]
	class NCTSFREDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestMessageDataObject()
		{
			var message = Factory.New<NCTSFREDIMessage>();
			message.EM_MessageSubType = ZString.Empty;
			AssertNull("MessageDatObject should be null when EM_MessageSubType is not valid.", message.MessageDataObject);

			message.EM_MessageSubType = TP5ResponseMessageSubTypeList.Codes.DeclarationAcceptance;
			AssertType<CC004CMessageDataObject>("MessageDataObject should be of type CC004CMessageDataObject when EM_MessageSubType is '004'.", message.MessageDataObject);

			message = Factory.New<NCTSFREDIMessage>();
			message.EM_MessageSubType = TP5ResponseMessageSubTypeList.Codes.InvalidationDecision;
			AssertType<CC009CMessageDataObject>("MessageDataObject should be of type CC009CMessageDataObject when EM_MessageSubType is '009'.", message.MessageDataObject);

			message = Factory.New<NCTSFREDIMessage>();
			message.EM_MessageSubType = TP5ResponseMessageSubTypeList.Codes.DiscrepanciesAtDestination;
			AssertType<CC019CMessageDataObject>("MessageDataObject should be of type CC019CMessageDataObject when EM_MessageSubType is '019'.", message.MessageDataObject);

			message = Factory.New<NCTSFREDIMessage>();
			message.EM_MessageSubType = TP5ResponseMessageSubTypeList.Codes.NotificationToAmendDeclaration;
			AssertType<CC022CMessageDataObject>("MessageDataObject should be of type CC022CMessageDataObject when EM_MessageSubType is '022'.", message.MessageDataObject);

			message = Factory.New<NCTSFREDIMessage>();
			message.EM_MessageSubType = TP5ResponseMessageSubTypeList.Codes.GoodsReleaseNotification;
			AssertType<CC025CMessageDataObject>("MessageDataObject should be of type CC025CMessageDataObject when EM_MessageSubType is '025'.", message.MessageDataObject);

			message = Factory.New<NCTSFREDIMessage>();
			message.EM_MessageSubType = TP5ResponseMessageSubTypeList.Codes.MrnAllocated;
			AssertType<CC028CMessageDataObject>("MessageDataObject should be of type CC028CMessageDataObject when EM_MessageSubType is '028'.", message.MessageDataObject);

			message = Factory.New<NCTSFREDIMessage>();
			message.EM_MessageSubType = TP5ResponseMessageSubTypeList.Codes.ReleasedForTransit;
			AssertType<CC029CMessageDataObject>("MessageDataObject should be of type CC029CMessageDataObject when EM_MessageSubType is '029'.", message.MessageDataObject);

			message = Factory.New<NCTSFREDIMessage>();
			message.EM_MessageSubType = TP5ResponseMessageSubTypeList.Codes.RecoveryNotification;
			AssertType<CC035CMessageDataObject>("MessageDataObject should be of type CC035CMessageDataObject when EM_MessageSubType is '035'.", message.MessageDataObject);

			message = Factory.New<NCTSFREDIMessage>();
			message.EM_MessageSubType = TP5ResponseMessageSubTypeList.Codes.UnloadingPermission;
			AssertType<CC043CMessageDataObject>("MessageDataObject should be of type CC043CMessageDataObject when EM_MessageSubType is '043'.", message.MessageDataObject);

			message = Factory.New<NCTSFREDIMessage>();
			message.EM_MessageSubType = TP5ResponseMessageSubTypeList.Codes.WriteOffNotification;
			AssertType<CC045CMessageDataObject>("MessageDataObject should be of type CC045CMessageDataObject when EM_MessageSubType is '045'.", message.MessageDataObject);

			message = Factory.New<NCTSFREDIMessage>();
			message.EM_MessageSubType = TP5ResponseMessageSubTypeList.Codes.GuaranteeNotValid;
			AssertType<CC055CMessageDataObject>("MessageDataObject should be of type CC055CMessageDataObject when EM_MessageSubType is '055'.", message.MessageDataObject);

			message = Factory.New<NCTSFREDIMessage>();
			message.EM_MessageSubType = TP5ResponseMessageSubTypeList.Codes.RejectionFromOfficeOfDeparture;
			AssertType<CC056CMessageDataObject>("MessageDataObject should be of type CC056CMessageDataObject when EM_MessageSubType is '056'.", message.MessageDataObject);

			message = Factory.New<NCTSFREDIMessage>();
			message.EM_MessageSubType = TP5ResponseMessageSubTypeList.Codes.RejectionFromOfficeOfDestination;
			AssertType<CC057CMessageDataObject>("MessageDataObject should be of type CC057CMessageDataObject when EM_MessageSubType is '057'.", message.MessageDataObject);

			message = Factory.New<NCTSFREDIMessage>();
			message.EM_MessageSubType = TP5ResponseMessageSubTypeList.Codes.RequestOnNonArrivedMovement;
			AssertType<CC140CMessageDataObject>("MessageDataObject should be of type CC140CMessageDataObject when EM_MessageSubType is '140'.", message.MessageDataObject);

			message = Factory.New<NCTSFREDIMessage>();
			message.EM_MessageSubType = TP5ResponseMessageSubTypeList.Codes.ForwardedIncidentNotificationToEd;
			AssertType<CC182CMessageDataObject>("MessageDataObject should be of type CC182CMessageDataObject when EM_MessageSubType is '182'.", message.MessageDataObject);

			message = Factory.New<NCTSFREDIMessage>();
			message.EM_MessageSubType = TP5ResponseMessageSubTypeList.Codes.FunctionalRejection;
			AssertType<CD906CMessageDataObject>("MessageDataObject should be of type CD906CMessageDataObject when EM_MessageSubType is '906'.", message.MessageDataObject);

			message = Factory.New<NCTSFREDIMessage>();
			message.EM_MessageSubType = TP5ResponseMessageSubTypeList.Codes.XmlNack;
			AssertType<CC917CMessageDataObject>("MessageDataObject should be of type CC917CMessageDataObject when EM_MessageSubType is '917'.", message.MessageDataObject);

			message = Factory.New<NCTSFREDIMessage>();
			message.EM_MessageSubType = TP5ResponseMessageSubTypeList.Codes.PositiveAcknowledge928;
			AssertType<CC928CMessageDataObject>("MessageDataObject should be of type CC928CMessageDataObject when EM_MessageSubType is '928'.", message.MessageDataObject);

			message = Factory.New<NCTSFREDIMessage>();
			message.EM_MessageSubType = TP5ResponseMessageSubTypeList.Codes.StatusUpdateNotification;
			AssertType<CCF02CMessageDataObject>("MessageDataObject should be of type CCF02CMessageDataObject when EM_MessageSubType is 'F02'.", message.MessageDataObject);

			message = Factory.New<NCTSFREDIMessage>();
			message.EM_MessageSubType = TP5ResponseMessageSubTypeList.Codes.PositiveAcknowledgeF03;
			AssertType<CCF03CMessageDataObject>("MessageDataObject should be of type CCF03CMessageDataObject when EM_MessageSubType is 'F03'.", message.MessageDataObject);
		}

		public void TestDefaultValueSetToTP5()
		{
			var message = Factory.New<NCTSFREDIMessage>();
			AssertEquals("EM_MessageType should be updated to TP5 for NCTS.", MessageTypeList.Codes.TP5, message.EM_MessageType);
		}
	}
}
