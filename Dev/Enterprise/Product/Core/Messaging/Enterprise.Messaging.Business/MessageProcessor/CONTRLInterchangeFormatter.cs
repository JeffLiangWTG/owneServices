using System;
using System.Text;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Edifact.D96B.Messages.CONTRL;
using Enterprise.Edifact.D96B.Segments;
using Enterprise.Messaging.Business;

namespace Enterprise.Messaging.MessageProcessors
{
	public class CONTRLInterchangeFormatter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer only string")]
		public string GetInterchangeWithErrorPointers(EDIInterchange interchange, string cONTRLMessage, UNCharacterSet characterSet)
		{
			CONTRLMessage myMessage = new CONTRLMessage();
			myMessage.Parse(characterSet, cONTRLMessage);

			StringBuilder resultBuilder = new StringBuilder();
			resultBuilder.Append(GetHeaderWithErrorPointers(interchange.EI_HeaderText, myMessage));
			foreach (EDIMessage message in interchange.ContainedMessages)
			{
				try
				{
					resultBuilder.Append(
						GetMessageWithErrorPointers(
						message.EM_MessageNum,
						message.EM_MessageText.ToString().Split(message.CharacterSet.SegmentDelimiterChar),
						myMessage));
				}
				catch (Exception exception) when (!exception.IsCriticalException())
				{
					resultBuilder.Append("Could not interpret outgoing message\r\n");
				}
			}
			resultBuilder.Append(UNOACharacterSet.FromUNOB(interchange.EI_FooterText) + "\r\n");
			return resultBuilder.ToString();
		}

		#region Implementation
		protected string GetHeaderWithErrorPointers(ZString headerText, CONTRLMessage myMessage)
		{
			StringBuilder resultBuilder = new StringBuilder();
			resultBuilder.Append(UNOACharacterSet.FromUNOB(headerText).TrimEnd('\'').Replace("'", "\r\n"));
			foreach (UCISegment uCI in myMessage.UCI)
			{
				ActionCode interchangeAction = ActionCode.Get(uCI.ActionCoded.ToString());
				SyntaxError error = null;
				if (!string.IsNullOrEmpty(uCI.SyntaxErrorCoded))
				{
					error = new SyntaxError(uCI.SyntaxErrorCoded.ToString());
				}

				resultBuilder.Append(" << " + interchangeAction.Description);
				if (error != null)
				{
					resultBuilder.Append(" - " + error.Description);
				}
			}
			resultBuilder.Append("\r\n");
			return resultBuilder.ToString();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		protected string GetMessageWithErrorPointers(string sendersReference, string[] outGoingMessage, CONTRLMessage myMessage)
		{
			StringBuilder resultBuilder = new StringBuilder();
			int segmentNumber = 0;

			string sendersRef = "";
			foreach (string valueItem in outGoingMessage)
			{
				if (valueItem.Length > 0)
				{
					segmentNumber++;
					resultBuilder.Append(UNOACharacterSet.FromUNOB(valueItem));
					foreach (SegmentGroup1 group1 in myMessage.Group1)
					{
						foreach (UCMSegment uCM in group1.UCM)
						{
							sendersRef = uCM.MessageReferenceNumber;
							if (valueItem.StartsWith("UNH", StringComparison.InvariantCulture) && sendersRef == sendersReference)
							{
								ActionCode messageAction = ActionCode.Get(uCM.ActionCoded.ToString());
								resultBuilder.Append(" << " + messageAction.Description);
								SyntaxError error = null;
								if (!string.IsNullOrEmpty(uCM.SyntaxErrorCoded.ToString()))
								{
									error = new SyntaxError(uCM.SyntaxErrorCoded.ToString());
									resultBuilder.Append(" - " + error.Description);
								}
							}
						}
						if (sendersRef == sendersReference)
						{
							foreach (SegmentGroup2 group2 in group1.Group2)
							{
								string messageSegmentNumber = "";
								foreach (UCSSegment uCS in group2.UCS)
								{
									messageSegmentNumber = segmentNumber.ToString();
									if (uCS.SegmentPositionInMessage == segmentNumber.ToString())
									{
										if (!string.IsNullOrEmpty(uCS.SyntaxErrorCoded.ToString()))
										{
											SyntaxError segmentError = new SyntaxError(uCS.SyntaxErrorCoded.ToString());
											resultBuilder.Append(" << " + segmentError.Description);
										}
										foreach (UCDSegment uCD in group2.UCD)
										{
											if (messageSegmentNumber == segmentNumber.ToString())
											{
												SyntaxError elementError = new SyntaxError(uCD.SyntaxErrorCoded.ToString());
												resultBuilder.Append(" << Position (" + uCD.DataElementIdentification.ErroneousDataElementPositionInSegment + ":" + uCD.DataElementIdentification.ErroneousComponentDataElementPosition + ") - " + elementError.Description);
											}
										}
									}
								}
							}
						}
					}
					resultBuilder.Append("\r\n");
				}
			}
			return resultBuilder.ToString();
		}
		#endregion

	}
}
