using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.GENRAL;
using Enterprise.Customs.GB.Registry;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.ChiefFallback
{
	class FallbackAnnouncementHandler
	{
		public FallbackAnnouncementHandler(List<ZString> lines, EDIMessage inboundMessage, string senderType)
		{
			this.lines = lines;
			this.inboundMessage = inboundMessage;
			this.senderType = senderType;
		}

		internal void DoAllProcessing()
		{
			var message = ParseMessage();
			SetOrUnsetFlags(message);
			SendAnnoucementToStaff(message);
		}

		FallbackMessage ParseMessage()
		{
			var fallbackMessage = new FallbackMessage();
			fallbackMessage.Responsiveness = ParseFirstLine(lines[0]);
			fallbackMessage.Announcement = ParseSecondLine(lines[1]);
			fallbackMessage.OtherDetails = new List<ZString>();
			for (int i = 2; i < lines.Count; i++)
			{
				fallbackMessage.OtherDetails.Add(lines[i]);
			}
			return fallbackMessage;
		}

		static void SetOrUnsetFlags(FallbackMessage message)
		{
			var isBeingInvoked = message.Responsiveness.Action == Actions.Invoked;
			if (message.Responsiveness.Direction == Directions.Import)
			{
				GBCustomsDataRegistry.Instance.ChiefFallbackImports.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isBeingInvoked);
			}
			else if (message.Responsiveness.Direction == Directions.Export)
			{
				GBCustomsDataRegistry.Instance.ChiefFallbackExports.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isBeingInvoked);
			}
			else if (message.Responsiveness.Direction == Directions.Both)
			{
				GBCustomsDataRegistry.Instance.ChiefFallbackExports.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isBeingInvoked);
				GBCustomsDataRegistry.Instance.ChiefFallbackImports.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isBeingInvoked);
			}
		}

		void SendAnnoucementToStaff(FallbackMessage message)
		{
			var prettyPayload = message.FormatNicely();
			GenralMessageParser.DistributePayloadAndUpdateMessageProperties(senderType, GenralPurpose.Codes.FallbackAnnouncement, prettyPayload, inboundMessage, "Fallback");
		}

		FallbackLineResponsiveness ParseFirstLine(string firstLineText)
		{
			// mask is ***FALLBACK***<space><”IMPORT”, “EXPORT” or “IMPORT AND EXPORT”><space><”INVOKED” or “REVOKED”><space>”EDATE”<space><ddmmyyyy><space>ETIME<space><hhmmss>
			var importOrExportOrBoth = string.Format("{0}|{1}|{2}", Directions.Import, Directions.Export, Directions.Both);
			var invokedOrRevoked = string.Format("{0}|{1}", Actions.Invoked, Actions.Revoked);
			var date = @"[0-9]{8}";
			var time = @"[0-9]{6}";
			var regEx = new Regex(string.Format(@"\*\*\*FALLBACK\*\*\* (?<DIRECTION>{0}) (?<ACTION>{1}) EDATE (?<DATE>{2}) ETIME (?<TIME>{3})", importOrExportOrBoth, invokedOrRevoked, date, time));
			var matches = regEx.Matches(firstLineText);
			var lineOne = new FallbackLineResponsiveness();
			foreach (Match match in matches)
			{
				lineOne.Direction = match.Groups["DIRECTION"].Value;
				lineOne.Action = match.Groups["ACTION"].Value;
				lineOne.DateOfResponsiveness = match.Groups["DATE"].Value;
				lineOne.TimeOfResponsiveness = match.Groups["TIME"].Value;
			}
			return lineOne;
		}

		FallbackLineAnnouncement ParseSecondLine(string secondLineText)
		{
			// mask is ***FALLBACK ANNOUNCED***<space>“ADATE”<space><ddmmyyyy><space>ATIME<space><hhmmss>
			var regEx = new Regex(@"\*\*\*FALLBACK ANNOUNCED\*\*\* ADATE (?<DATE>[0-9]{8}) ATIME (?<TIME>[0-9]{6})");
			var matches = regEx.Matches(secondLineText);
			var lineTwo = new FallbackLineAnnouncement();
			foreach (Match match in matches)
			{
				lineTwo.DateOfAnnouncement = match.Groups["DATE"].Value;
				lineTwo.TimeOfAnnouncement = match.Groups["TIME"].Value;
			}
			return lineTwo;
		}

		readonly List<ZString> lines;
		readonly EDIMessage inboundMessage;
		readonly string senderType;
	}
}
