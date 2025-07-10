using System;
using System.Text;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Ccsuk.Connection
{
	public class CompleteMessage
	{
		public CompleteMessage(Body body)
		{
			this.Body = body;
		}

		public CompleteMessage()
		{
		}

		public Body Body { get; set; }

		public ZString PacketToDeliver
		{
			get
			{
				return Header.ToUpper() + Body.PayloadAsString;
			}
		}

		public string Header
		{
			get
			{
				var payloadBytes = Encoding.UTF8.GetBytes(Body.PayloadAsString);
				// e.g. payload is "SM08TESTHOSTAA0201", 18 chars long
				long lengthOfPayload = payloadBytes.Length;   // e.g. 18
				string sizeOfMessageBodyLengthInHex = Convert.ToString(lengthOfPayload, 16);  // e.g. "12" (18dec in hex, into string)
				int lengthOf_SizeOfMessageBodyLengthInHex = sizeOfMessageBodyLengthInHex.Length;  // e.g. "12".Length = 2
				string lengthOf_SizeOfMessageBodyLengthInHex_InHex = Convert.ToString(lengthOf_SizeOfMessageBodyLengthInHex, 16);  // e.g. "2"
				string bodyLengthDiscussion = lengthOf_SizeOfMessageBodyLengthInHex_InHex + sizeOfMessageBodyLengthInHex; //e.g. "212"
				int lengthOf_bodyLengthDiscussion = bodyLengthDiscussion.Length;  // e.g. 3
				int headerLengthBlock = 2; // defined by ccsuk as 2
				int lengthOf_WholeHeader = headerLengthBlock + lengthOf_bodyLengthDiscussion;  // e.g. 5  (=2+3)
				string lengthOf_WholeHeader_InHex = Convert.ToString(lengthOf_WholeHeader, 16);  // e.g. "5"
				if (lengthOf_WholeHeader_InHex.Length == 1)
				{
					lengthOf_WholeHeader_InHex = "0" + lengthOf_WholeHeader_InHex; // pad with zero, e.g. "05"
				}

				return lengthOf_WholeHeader_InHex + bodyLengthDiscussion;  // e.g. "05212"
			}
		}
	}
}
