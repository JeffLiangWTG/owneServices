using System.Collections;
using System.Text;
using Enterprise.Edifact;

namespace Enterprise.Messaging.MessageProcessors
{
	/// <summary>
	/// Summary description for MessageProcessingUtils.
	/// </summary>
	public class MessageProcessingUtils
	{
		public static string[][][] ReadMessage(UNCharacterSet characterSet, string messageString)
		{
			MessageProcessingUtils mpu = new MessageProcessingUtils();
			return mpu.GetMessageAsArray(characterSet, messageString);
		}

		public string[][][] GetMessageAsArray(UNCharacterSet characterSet, string messageString)
		{
			this.CharacterSet = characterSet;
			MessagePointer = 0;

			ArrayList result = new ArrayList();
			bool finished = false;
			while (!finished)
			{
				result.Add(ReadSegment(messageString));
				if (messageString.Length <= MessagePointer)
				{
					finished = true;
				}
			}
			return (string[][][])result.ToArray(typeof(string[][]));
		}

		#region Implementation
		protected string[][] ReadSegment(string messageString)
		{
			ArrayList result = new ArrayList();
			bool finished = false;
			while (!finished)
			{
				result.Add(ReadElement(messageString));
				if (messageString[MessagePointer] == CharacterSet.SegmentDelimiterChar)
				{
					finished = true;
					MessagePointer++;
				}
			}
			return (string[][])result.ToArray(typeof(string[]));
		}

		protected string[] ReadElement(string messageString)
		{
			ArrayList result = new ArrayList();
			bool finished = false;
			while (!finished)
			{
				string subElement = ReadSubElement(messageString);
				result.Add(subElement);
				if (messageString[MessagePointer] == CharacterSet.ElementDelimiterChar)
				{
					finished = true;
					MessagePointer++;
				}
				else if (messageString[MessagePointer] == CharacterSet.SegmentDelimiterChar)
				{
					finished = true;
				}
			}
			return (string[])result.ToArray(typeof(string));
		}

		protected string ReadSubElement(string messageString)
		{
			StringBuilder result = new StringBuilder();
			bool finished = false;
			bool escaped = false;
			while (!finished)
			{
				if (!escaped)
				{
					if (messageString[MessagePointer] == CharacterSet.EscapeCharacterChar)
					{
						escaped = true;
						MessagePointer++;
					}
					else if (messageString[MessagePointer] == CharacterSet.SubElementDelimiterChar)
					{
						finished = true;
						MessagePointer++;
					}
					else if (messageString[MessagePointer] == CharacterSet.ElementDelimiterChar)
					{
						finished = true;
					}
					else if (messageString[MessagePointer] == CharacterSet.SegmentDelimiterChar)
					{
						finished = true;
					}
					else
					{
						result.Append(messageString[MessagePointer]);
						MessagePointer++;
					}
				}
				else
				{
					result.Append(messageString[MessagePointer]);
					escaped = false;
					MessagePointer++;
				}
			}
			return result.ToString();
		}

		int MessagePointer;
		UNCharacterSet CharacterSet;
		#endregion
	}
}
