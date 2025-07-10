using System;
using System.Xml.Linq;
using CargoWise.Common;

namespace Enterprise.Client.EDI.ServiceTasks.EventLogs
{
	class DataCallStackReader
	{
		readonly string data;
		readonly XNamespace xmlPath;

		public DataCallStackReader(string data, XNamespace xmlPath)
		{
			Argument.NotNullOrEmpty(data, "data");
			Argument.NotNull(xmlPath, "xmlPath");
			this.data = data;
			this.xmlPath = xmlPath;
		}

		public void ExtractCallStack(XElement convertedData)
		{
			Argument.NotNull(convertedData, "convertedData");

			var list = Array.ConvertAll(data.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None), p => p.Trim());
			foreach (var item in list)
			{
				AddElement(item, convertedData);
			}
		}

		/// <summary>
		/// Convert each line in the Data Element into XML
		/// Skip if This line is empty or null
		/// If the length of the line is less than the substring length of if argument
		/// Turn into "Message" element
		/// </summary>
		/// <param name="item">Could be empty string. If emptystring, skip this method</param>
		/// <param name="convertedData"></param>
		/// <returns></returns>
		void AddElement(string item, XElement convertedData)
		{
			Argument.NotNull(convertedData, "convertedData");

			if (item.StartsWith("at"))
			{
				AddElementWithoutRemoveWords(convertedData, item, "Call");
			}
			else if (item.StartsWith("Application:"))
			{
				AddElementWithRemoveWords(convertedData, item, "Application:", "Source");
			}
			else if (item.StartsWith("Description:"))
			{
				AddElementWithRemoveWords(convertedData, item, "Description:", "Message");
			}
			else if (item.StartsWith("Exception Info:"))
			{
				AddElementWithRemoveWords(convertedData, item, "Exception Info:", "ExceptionType");
			}
			else if (item.StartsWith("Framework Version:"))
			{
				AddElementWithRemoveWords(convertedData, item, "Framework Version:", "VersionNumber");
			}
			else if (item.StartsWith("Stack:"))
			{
			}
			else if (!String.IsNullOrEmpty(item))
			{
				AddElementWithoutRemoveWords(convertedData, item, "Message");
			}
		}

		void AddElement(XElement convertedData, string item, int length, string xmlElement)
		{
			int atIndex = 0;
			string convertedString = item.Substring(atIndex + length).Trim();
			convertedData.Add(new XElement(xmlPath + xmlElement, convertedString));
		}

		void AddElementWithoutRemoveWords(XElement convertedData, string item, string xmlElement)
		{
			AddElement(convertedData, item, 0, xmlElement);
		}

		void AddElementWithRemoveWords(XElement convertedData, string item, string fromString, string toString)
		{
			AddElement(convertedData, item, fromString.Length, toString);
		}
	}
}
