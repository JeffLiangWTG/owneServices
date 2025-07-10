using System;
using System.IO;
using System.Text;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Ccsuk.Connection
{
	public struct PayloadData
	{
		public string Header;
		public int ExpectedLength;
		public int ActualLength;
	}

	public class ResponseParser
	{
		/// <summary>
		/// Will peek at the first 3 bytes of a stream, then read the first N bytes (as specified in the first three), work out how long the whole stream is, and read that much.  
		/// Since the stream will stay open after the end of the payload, we can't just use ReadToend()
		/// </summary>
		public (string payload, PayloadData payloadData) ReadPayloadFromStream(Stream stream, Action<string> debugMethod)
		{
			var payloadData = new PayloadData();
			//e.g. stream gives (will give) "05212SM08TESTHOSTAA0201"
			long curPos = stream.CanSeek ? stream.Position : -1;
			// Cursor (|) is now |05212SM08TESTHOSTAA0201
			var buffer = new byte[3];
			var actualReadCount = stream.Read(buffer, 0, 3);  // get back {0,5,2}, cursor is now here:  052|12SM08TESTHOSTAA0201
			if (actualReadCount != 3)
			{
				debugMethod("Could not read header bytes from the stream");
				return ("", payloadData);
			}

			var headerIntroductionThreeBytes = new ZString(Encoding.ASCII.GetString(buffer));
			payloadData.Header = headerIntroductionThreeBytes;

			string wholeHeaderLengthHex = headerIntroductionThreeBytes.Left(2);  // e.g. "05"
			string lengthOfBodyLengthHex = headerIntroductionThreeBytes.Right(1);  // e.g. "2"
			if (lengthOfBodyLengthHex == "\0")
			{
				// Could not read anything but empty bytes from the stream.  This should never happen unless I have misunderstood the protocol or something somewhere became disconnected unexpectedly.
				// Actually it happens when you upload dodgy EDIFACT, e.g. one containing ¬¬¬.  See eDocs of WI00109599
				debugMethod("Could not read anything but empty bytes from the stream");
				return ("", payloadData);
			}

			int lengthOfBodyLength = -1;
			try
			{
				lengthOfBodyLength = Convert.ToInt32(lengthOfBodyLengthHex, 16);  // e.g. 2
			}
			catch (FormatException ex)
			{
				string streamData = "";
				if (stream.CanSeek)
				{
					int ch;
					int charsRead = 0;
					stream.Position = curPos < 1024 ? 0 : curPos - 1024;
					using (var reader = new StreamReader(stream))
					{
						while ((ch = reader.Read()) != -1 && charsRead < 32767)
						{
							charsRead++;
							streamData += (char)ch + ((charsRead % 72 == 0) ? "\r\n" : "");
						}
					}
				}
				else
				{
					streamData = wholeHeaderLengthHex + lengthOfBodyLengthHex;
				}
				throw new InvalidDataException(String.Format("Invalid Hexadecimal character lengthOfBodyLength: '{0}',  Header Position: {1}\r\nStream Data:\r\n{2}", lengthOfBodyLengthHex, curPos, streamData), ex);
			}

			var lengthOfBodyBuffer = new byte[lengthOfBodyLength];

			actualReadCount = stream.Read(lengthOfBodyBuffer, 0, lengthOfBodyLength);  // lengthOfBodyBuffer is now {1,2} and cursor is here:   05212|SM08TESTHOSTAA0201
			if (actualReadCount != lengthOfBodyLength)
			{
				debugMethod("Could not read header bytes for length of body from the stream");
				return ("", payloadData);
			}

			string lengthOfBodyBufferHex = Encoding.ASCII.GetString(lengthOfBodyBuffer); // e.g. "12"
			payloadData.Header += lengthOfBodyBufferHex;
			int sizeOfPayload = Convert.ToInt32(lengthOfBodyBufferHex, 16);  // e.g. 18 decimal (=0x12)

			var bodyArray = new byte[sizeOfPayload];
			var actualLength = stream.Read(bodyArray, 0, sizeOfPayload);

			var bodyString = Encoding.UTF8.GetString(bodyArray, 0, actualLength);

			payloadData.ExpectedLength = sizeOfPayload;
			payloadData.ActualLength = actualLength;

			return (bodyString, payloadData);
		}

		public (Body body, PayloadData payloadData) GetResponseBackFromStream(Stream stream, Action<string> debugMethod)
		{
			Body body = null;
			var content = ReadPayloadFromStream(stream, debugMethod);
			if (content.payload == null)
			{
				debugMethod("Payload was null");
			}
			else if (content.payload.Length == 0)
			{
				debugMethod("Payload was zero bytes long");
			}
			else
			{
				body = LoadMessageFromText(content.payload);
			}
			return (body, content.payloadData);
		}

		public (ExpectedResponseType body, PayloadData payloadData) GetResponseBackFromStream<ExpectedResponseType>(Stream stream, Action<string> debugMethod) where ExpectedResponseType : Body
		{
			var content = ReadPayloadFromStream(stream, debugMethod);
			var body = (ExpectedResponseType)LoadMessageFromText(content.payload);

			return (body, content.payloadData);
		}

		public Body LoadMessageFromText(string bodyText)
		{
			return LoadMessageFromTextCore(bodyText);
		}

		protected virtual Body LoadMessageFromTextCore(string bodyText)
		{
			if (bodyText.StartsWith(HandShake.HandShakeIdentifier))
			{
				HandShakeResponse response = new HandShakeResponse(bodyText);
				return response;
			}
			else if (bodyText.StartsWith(ServiceMessage.ServiceMessageIdentifier))
			{
				return ShortMessageResponse.CreateResponseFromBodyText(bodyText);
			}
			else if (bodyText.Length == 0)
			{
				return null;
			}
			else
			{
				return new CargoMessage(bodyText);
			}
		}
	}
}
