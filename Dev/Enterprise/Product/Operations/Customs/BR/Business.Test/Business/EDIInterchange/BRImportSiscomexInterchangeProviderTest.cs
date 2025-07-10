using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class BRImportSiscomexInterchangeProviderTest : InterchangeProviderTestCase
	{
		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection)
		{
			return new BRImportSiscomexInterchangeProvider(collection);
		}

		public void TestAddFileOrDocumentImportSiscomex()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_DeclarationReference = "TEST_BO";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var message = entryHeader.Messages.AddNew(typeof(BREDIMessage)) as BREDIMessage;
			message.EM_MessageType = MessageTypeList.Codes.ISW;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_ApplicationReference = "000";
			message.EM_MessageText = "MESSAGE TEXT";

			var messages = new NonDependentEDIMessageCollection(Factory);
			messages.AddRange(new BREDIMessage[] { message });
			var provider = new BRImportSiscomexInterchangeProvider(messages);
			provider.PackCollatedMessagesIntoInterchanges();
			Factory.Save();

			var docs = declaration.DocManagerInfo.AllEDocs;
			AssertEquals(1, docs.Count);
			var doc = docs[0];
			AssertEquals("TEST_BO.xml", doc.FileName);
			AssertEquals(Core.Constants.RefDocTypes.RequestDocument, doc.DocType);

			using (var streamReader = new StreamReader(doc.GetImageDataReader(), Encoding.ASCII))
			{
				AssertEquals("MESSAGE TEXT", streamReader.ReadToEnd());
			}
		}

		public override void TestMessagesPopulateNewInterchange()
		{
			var messageText = "Message Text";
			var message1 = CreateAndPopulateMessage(MessageTypeList.Codes.CDI, ImportSiscomexActionCodeList.Codes.ORI, messageText);
			var message2 = CreateAndPopulateMessage(MessageTypeList.Codes.CDI, ImportSiscomexActionCodeList.Codes.ORI, messageText);
			var messages = new NonDependentEDIMessageCollection(Factory);
			messages.AddRange(new BREDIMessage[] { message1, message2 });
			var provider = new BRImportSiscomexInterchangeProvider(messages);
			provider.PackCollatedMessagesIntoInterchanges();

			var interchanges = provider.Interchanges;
			CombineAssertions(() =>
			{
				AssertEquals("Number Of Interchanges", 2, interchanges.Length);
				messages.Cast<EDIMessage>().ForEach(message =>
				{
					var interchange = message.Interchange;
					AssertEquals("Message Status", Constants.EDIMessageStatusCodes.Manual, interchange.ContainedMessages[0].EM_Status);
					AssertEquals("To", BREDIInterchange.BRCustoms, interchange.EI_To);
					AssertEquals("From", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
					AssertEquals("Interchange Status", Constants.EDIMessageStatusCodes.Manual, interchange.EI_Status);
					AssertEquals("Interchange Type ", MessageTypeList.Codes.CDI, interchange.EI_InterchangeType);
					AssertEquals("Application Code ", BREDIInterchange.ApplicationCodes.BRCustoms, interchange.EI_ApplicationCode);
					AssertEquals("Transport Type", EDIInterchange.TransportType.tXT, interchange.EI_TransportType);
					AssertEquals("Header Text", @"{""custom.MessageSubType"":""ORI"",""custom.ReferenceNumber"":""21BR0000022649""}", interchange.EI_HeaderText);
					AssertEquals("Body Text", "Message Text", interchange.EI_BodyText.ToString());
					AssertEquals("Footer Text", ZString.Empty, interchange.EI_FooterText);
				});
			});
		}

		BREDIMessage CreateAndPopulateMessage(string messageType, string messageSubType, string messageBody)
		{
			var message = Factory.New<BREDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.BRCustoms;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_MessageText = messageBody;
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = messageSubType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ApplicationReference = "21BR0000022649";
			return message;
		}
	}
}
