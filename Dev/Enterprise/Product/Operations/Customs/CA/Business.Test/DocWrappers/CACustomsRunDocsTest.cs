using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.Customs;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CACustomsRunDocsTest : CustomsRunDocsTest
	{
		[ExpectNoExceptions]
		public void TestB3ImportDocument()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, ForwardingShipmentDocumentSupporter.CACustomsDocList.B3CurrentData);
			RunDocument();

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, ForwardingShipmentDocumentSupporter.CACustomsDocList.B3AsLodged);
			RunDocument();
		}

		#region Implementation

		public override BusinessObject GetBusinessObject
		{
			get
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
				declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
				declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "123456";
				declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "654321";
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				var entryHeader = declaration.GetEntryHeaderFor(Enterprise.Customs.CA.Business.MessageTypeList.Codes.B3CUSDEC);

				const string interchangeText = @"UNB+UNOA:3+YUSAIRXPN+INETCECPT+110607:0915+7696'UNG+CUSDEC+U10207V1+KI+110607:0915+700+UN+S:99B+10207YUSENT'UNH+679+CUSDEC:S:99B:UN'BGM+:::AB+419+9'RFF+TN:400004228'UNS+D'UNS+S'UNT+57+679'UNE+1+700'UNZ+1+7696'";
				entryHeader.Messages.Add(CreateMessageFromInterchangeString(Factory, interchangeText));
				entryHeader.Messages.Add(CreateMessage(Factory, Enterprise.Customs.CA.Business.MessageTypeList.Codes.B3CUSDEC, Enterprise.Customs.Common.Shared.EntryStatusList.Codes.Clear, EDIMessage.Direction.Receive, ZDateTime.Now.AddDays(1)));
				return declaration;
			}
		}

		static Enterprise.Messaging.Business.EDIMessage CreateMessageFromInterchangeString(BusinessObjectFactory factory, string interchangeString)
		{
			var interchange = EDIInterchange.CreateNewInterchangeFromString(factory, interchangeString.Replace("\r\n", "'"), EDIMessage.ApplicationCodes.CAIMP, false, true);
			interchange.SpawnMessagesFromInterchageTextAndMarkAsReceived();
			var message = interchange.ContainedMessages[0];
			message.EM_SystemCreateTimeUtc = ZDateTime.Now;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Sent;
			return factory.Load<EDIMessage>(message.PK);
		}

		static Enterprise.Messaging.Business.EDIMessage CreateMessage(BusinessObjectFactory factory, string messageType, string subType, string direction, ZDateTime date, string text = "")
		{
			var message = factory.New<Enterprise.Messaging.Business.EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = subType;
			message.EM_ReceiveTransmit = direction;
			message.EM_Status = message.IsTransmitMessage ? EDIMessage.Status.Sent : EDIMessage.Status.Received;
			message.EM_SystemCreateTimeUtc = date;
			message.EM_MessageText = text;
			return factory.Load<EDIMessage>(message.PK);
		}

		protected override void SetUp()
		{
			storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Canada);
			base.SetUp();
		}

		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.SetCountry(storedCountry);
			base.TearDown();
		}

		ZString storedCountry;

		#endregion

	}
}
