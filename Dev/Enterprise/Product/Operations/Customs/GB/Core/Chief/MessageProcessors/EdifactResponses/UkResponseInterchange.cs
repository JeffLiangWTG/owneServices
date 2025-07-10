using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Chief.CusRes
{
	public class UkResponseInterchange
	{
		readonly string inputString;
		string inputStringWithUNA;

		public string ApplicationCode
		{
			get;
			private set;
		}

		BusinessObjectFactory sharedFactory
		{
			get;
			set;
		}

		public UkResponseInterchange(string ukResponseText, string applicationCode, BusinessObjectFactory sharedFactory)
		{
			Argument.NotNullOrEmpty(ukResponseText, "ukControlMessageResponseText");
			Argument.NotNullOrEmpty(applicationCode, "applicationCode");

			this.sharedFactory = sharedFactory;
			this.inputString = ukResponseText;
			this.ApplicationCode = applicationCode;
		}

		public EDIInterchange CreateInterchange(ZString badgeOfRecipient)
		{
			EDIInterchange interchangeWeAreMakingFromInboundResponse;
			try
			{
				inputStringWithUNA = PossiblyAddUnaHeaderToIllegalInterchangeIfNeeded(inputString);
				interchangeWeAreMakingFromInboundResponse = EDIInterchange.CreateNewInterchangeFromString(sharedFactory, inputStringWithUNA, this.ApplicationCode);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				// If the response we get is not edifact and it's not "Must be your own company" (which is already handled further up the stack) then there's not a lot we can do :S
				interchangeWeAreMakingFromInboundResponse = sharedFactory.New<EDIInterchange>();
				interchangeWeAreMakingFromInboundResponse.EI_BodyText = this.inputString;
				EDIMessage message = interchangeWeAreMakingFromInboundResponse.ContainedMessages.AddNew();
				message.EM_ReceiveTransmit = "RCV";
				message.EM_MessageText = inputString + EDIMessage.MessageNumberPlaceHolder;
				message.MessageNumberStrategy = new Business.GbMessageNumberStrategy(sharedFactory, "HMRC/" + this.ApplicationCode);
			}
			if (interchangeWeAreMakingFromInboundResponse.EI_From.IsEmpty)
			{
				interchangeWeAreMakingFromInboundResponse.EI_From = "HMRC";
			}
			interchangeWeAreMakingFromInboundResponse.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchangeWeAreMakingFromInboundResponse.EI_Status = EDIInterchange.Status.Queued;
			interchangeWeAreMakingFromInboundResponse.EI_IsActive = true;
			if (interchangeWeAreMakingFromInboundResponse.EI_To.IsEmpty)
			{
				interchangeWeAreMakingFromInboundResponse.EI_To = badgeOfRecipient;
			}
			return interchangeWeAreMakingFromInboundResponse;
		}

		public static string PossiblyAddUnaHeaderToIllegalInterchangeIfNeeded(string unbInterchangePossiblyWithoutUna)
		{
			if (unbInterchangePossiblyWithoutUna == null)
			{ throw new ArgumentNullException(nameof(unbInterchangePossiblyWithoutUna), "Precondition - argument cannot be null"); }

			/*
			From: Knowles, Russell D [mailto:russellknowles@mcpplc.com]
			Sent: 13 October 2008 18:58
			To: Daniel Clarke
			Cc: Inman, Peter
			Subject: RE: Further technical question about "UCZ" and UNA

			Daniel,
			Answers to your questions: -
			•	Yes UCZ should read UNZ, we already have a note to change in next release of document.
			•	Yes what we do is a slight incorrect misuse of the EDIFACT standard. Without the UNA we still expect the MCP character set of {, #, and \ to be used. This is a legacy issue from older screen driven interfaces and has been like this for the last 15 years.

			I hope this helps.
			Regards
			Russ
			*/

			if (unbInterchangePossiblyWithoutUna == null)
			{ return null; }

			if (unbInterchangePossiblyWithoutUna.StartsWith("UNA"))
			{
				// already had a UNA header, do nothing
				return unbInterchangePossiblyWithoutUna;
			}
			else
			{
				//   header that MCP omit looks like this:  UNA\#.? {
				if (unbInterchangePossiblyWithoutUna.StartsWith("UNB#") && unbInterchangePossiblyWithoutUna.Contains("{") && unbInterchangePossiblyWithoutUna.Contains(@"\"))
				{
					// it's likely that MCP are implying UNA\#.? {
					string impliedUna = @"UNA\#.? {";
					return impliedUna + unbInterchangePossiblyWithoutUna;
				}
			}
			return unbInterchangePossiblyWithoutUna;
		}
	}
}
