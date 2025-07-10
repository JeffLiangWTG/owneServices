using System;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.HoldAdderRemoverClearer;

namespace Enterprise.Customs.GB.CNS.ServiceTasks.StatusUpdateXML.TextProcessors
{
	// HOLD REMOVED NOTIFICATION                  
	// HOLD NOTIFICATION                  
	class CnsChildProcessor_CmiHld1 : CnsChildProcessor, IPortAuthorityHoldApplicationProvider
	{
		protected override void DoFurtherProcessingForSuccessfullyFoundEntry(Business.Declaration.CusEntryHeader entryHeader)
		{
			base.DoFurtherProcessingForSuccessfullyFoundEntry(entryHeader);
			entryHeader.ApplyOrRemoveHoldOrClear(this);
			var containerNumber = GetFirstGroupMatchFromMessageText(@"Unit Number\s+([A-Za-z0-9]{11})");
			var newStatusCode = DirectionOfApplication == AddOrRemove.HoldAdd ? ContainerStatusCodesList.Codes.HoldIsAdded : ContainerStatusCodesList.Codes.AtLeastOneHoldRemovedOthersOrNoneMayRemain;
			CnsChildProcessor_CerRel2.UpdateContainerToStatus(entryHeader, containerNumber, newStatusCode);
		}

		public string HoldType
		{
			get
			{
				var holdType = "";
				var pattern = @".*\((.*?)\)";
				if (DirectionOfApplication == AddOrRemove.HoldAdd)
				{
					pattern = " is subject to " + pattern;
				}
				else if (DirectionOfApplication == AddOrRemove.HoldRemove)
				{
					pattern = "has been released from " + pattern;
				}
				holdType = GetFirstGroupMatchFromMessageText(pattern);
				return holdType;
			}
		}

		public string HoldAuthority
		{
			get { return ""; }
		}

		public AddOrRemove DirectionOfApplication
		{
			get
			{
				if (receivedEdiMessage.EM_MessageText.Contains("HOLD REMOVED NOTIFICATION"))
				{
					return AddOrRemove.HoldRemove;
				}
				else if (receivedEdiMessage.EM_MessageText.Contains("HOLD NOTIFICATION"))
				{
					return AddOrRemove.HoldAdd;
				}
				throw new NotSupportedException("Cannot determine whether hold is being added or removed");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1054:DoNotHardcodePaths", Justification = "Is a Regex time pattern")]
		public ZDateTime Date
		{
			get
			{
				var result = ZDateTime.Empty;
				string pattern = @"(\d\d-\d\d-\d\d)\s+(\d\d:\d\d)\s+\r?\n?------";
				var match = new Regex(pattern).Match(receivedEdiMessage.EM_MessageText);
				if (match.Success)
				{
					var dateString = match.Groups[1].Value + " " + match.Groups[2].Value;
					ZDateTime.TryParseExact(dateString, out result, "dd-MM-yy HH:mm");
				}
				return result;
			}
		}
	}
}
