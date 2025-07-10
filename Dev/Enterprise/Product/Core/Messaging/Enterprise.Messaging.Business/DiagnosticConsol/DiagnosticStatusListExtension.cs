using CargoWise.Types;

namespace Enterprise.Messaging.Business
{
	public partial class DiagnosticStatusList
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public static ZString GetExtendedStatusDescription(ZString statusCode)
		{
			switch (statusCode)
			{
				case Codes.TestMessageAtQUEStatus:
					return Res.GetString("940b262d-fed2-460a-860c-5a617cbfa036", "Check that the 'Customs Interchange Sender' Service Tasks is running.\r\nCheck the 'Customs Interchange Sender' service task log.");
				case Codes.TestMessageStatusInvalid:
				case Codes.NoOutInterchange:
				case Codes.NoOutEmail:
				case Codes.OutInterchangeStatusInvalid:
				case Codes.LookingForOutMailItem:
					return Res.GetString("2338b8c9-d289-4cbd-b400-6c48d835246e", "Check the 'Customs Interchange Sender' service task log.");
				case Codes.OutMailAtQUEStatus:
					return Res.GetString("7a6418fd-32d2-443a-8537-7972910754ac", "Check that the 'Outbound Mail' Service Tasks is running.\r\nCheck the 'Outbound Mail' service task log.");
				case Codes.OutMailStatusInvalid:
					return Res.GetString("8781f53a-23dd-43cb-b29e-d15a83d434f0", "Check the 'Outbound Mail' service task log.");
				case Codes.WaitingForACK:
					return Res.GetString("e2732603-d48b-4188-9a8f-c3d6d4a7f850", "No acknowledgement has been received from Customs.\r\n\tCheck that the 'Inbound Mail' Service Task is running, and check its log\r\nCheck if there are any Emails from Customs ({0}) that have failed.\r\n\tOther possible causes are:\r\n\tMail server problem on your side, check your mail server to see mail is actually being sent and that mail is being received\r\n\tCustoms are down or experiencing problems, check the Customs web site and with Customs\r\n\tYour digital certificate is invalid or not registered with Customs.", "cargo@ccf.customs.gov.au");
				case Codes.WaitingForResponse:
					return Res.GetString("2CDB35DA-546A-4247-95B6-7F7D4C728F45", "No business reply has been received from Customs.\r\nSince the message has been acknowledged this indicates that your digital certificate and the message channel to and from Customs are OK.\r\nCustoms may be down or experiencing problems, check the Customs web site and with Customs.\r\nCheck if there are any Emails from Customs ({0}) that have failed.\r\nCheck the 'Customs Interchange Retriever' service task log.", "cargo@ccf.customs.gov.au");
				case Codes.WaitingForTestMessageResponse:
					return Res.GetString("84fd7458-8cd3-4047-b124-b7072edd22e0", "No reply message has been received.\r\nCheck if there are any inbound emails that have failed.\r\nCheck the 'Customs Interchange Retriever' service task log.");
				case Codes.OutInterchangeRejected:
					return Res.GetString("8eaeb5e4-23c7-4117-a992-17962b1c57e8", "This is most likely caused by an invalid (or incompatible) setup in CargoWise One or at Customs.\r\nCheck that the Site ID (on Company record) and Branch code (registry) are the same as set in the User Site in the ICS.");
				case Codes.OKUnexpectedStatus:
				case Codes.BusinessReplyAtQUEStatus:
					return Res.GetString("6EC061B9-8771-4190-B8D2-67ADDEE6E541", "Check the 'Customs Message Processor' service task log.");
				case Codes.InInterchangeAtQUEStatus:
				case Codes.InInterchangeProcessed:
				case Codes.InInterchangeStatusInvalid:
					return Res.GetString("44AC89C5-714A-4345-A5AC-DFC07BA65BF8", "Check the 'Customs Interchange Retriever' service task log.");
				default:
					return Res.GetString("fd81bc89-1482-4a90-936c-751598fdae57", "No additional information available");
			}
		}
	}
}
