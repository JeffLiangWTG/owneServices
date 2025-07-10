using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CargoWise.Common;

namespace Enterprise.Client.EDI.ServiceTasks.EventLogs
{
	class EventLogsCallStackExtractor
	{
		readonly IEnumerable<XElement> eventData;
		readonly XNamespace xmlPath;
		State state;

		public State State { get { return state; } }

		public EventLogsCallStackExtractor(IEnumerable<XElement> eventData, XNamespace xmlPath)
		{
			Argument.NotNull(eventData, "eventData");
			Argument.NotNull(xmlPath, "xmlPath");
			this.eventData = eventData;
			this.xmlPath = xmlPath;
			InitialState();
		}

		/// <summary>
		/// To classify the XElement
		/// If we do not want to collect UnimportantData or NormalData. Delete them in GetData().
		/// </summary>
		void InitialState()
		{
			state = State.UnimportantData;
			foreach (var inner in eventData)
			{
				string data = inner.Value;
				if (string.IsNullOrEmpty(data))
				{
					continue;
				}
				if (data.Contains("Stack:") || data.Contains("at "))
				{
					state = State.HavingCallStackData;
					return;
				}
				else if (CheckPassedNormalData(data))
				{
					state = State.NormalData;
				}
			}
		}

		public XElement GetData()
		{
			return GetDataXElement();
		}

		XElement GetDataXElement()
		{
			XElement convertedData = new XElement(xmlPath + "ConvertedData", string.Empty);
			foreach (var inner in eventData)
			{
				string data = inner.Value;
				if (string.IsNullOrEmpty(data))
				{
					continue;
				}
				if (data.Contains("Stack:") || data.Contains("at "))
				{
					//Wrapper class for reading Data element that start with 'at' and covert them into CallStack elements.
					new DataCallStackReader(data, xmlPath).ExtractCallStack(convertedData);
					break;
				}
				else if (CheckPassedNormalData(data))
				{
					convertedData.Add(new XElement(xmlPath + "Message", data));
				}
			}
			return convertedData;
		}

		/// <summary>
		/// Check if this Data has more than 3 meaning words
		/// Check not hex number and directory address
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		bool CheckPassedNormalData(string data)
		{
			int countWords = data.Split().Length;
			return countWords > 2 && !IsOnlyHexInString(data) && !IsDirectoryAddress(data);
		}

		bool IsOnlyHexInString(string data)
		{
			// For C-style hex notation (0xFF) you can use @"\A\b(0[xX])?[0-9a-fA-F]+\b\Z"
			return Regex.IsMatch(data, @"\A\b[0-9a-fA-F][0-9a-fA-F]([ ]*[0-9a-fA-F][0-9a-fA-F])*\b\Z");
		}

		bool IsDirectoryAddress(string data)
		{
			// Contains '\' of C:\Any\Folder and not contain space, tab or new line.
			return Regex.IsMatch(data, @"^(.:\\|\\)") || data.StartsWith("\\");
		}
	}
}
