using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public class InterchangeHeaderTextBuilder
{
	public InterchangeHeaderTextBuilder AppendStaff(ZString value)
	{
		AppendKeyValuePair(XmlElementName.Staff, value);
		return this;
	}

	public InterchangeHeaderTextBuilder AppendNode(ZString value)
	{
		AppendKeyValuePair(XmlElementName.Node, value);
		return this;
	}

	public InterchangeHeaderTextBuilder AppendMessageType(ZString value)
	{
		AppendKeyValuePair(XmlElementName.MessageType, value);
		return this;
	}
	public InterchangeHeaderTextBuilder AppendAccountNumber(ZString value)
	{
		AppendKeyValuePair(XmlElementName.AccountNumber, value);
		return this;
	}

	public InterchangeHeaderTextBuilder AppendHeader(ZString value)
	{
		AppendKeyValuePair(XmlElementName.Header, value);
		return this;
	}

	public InterchangeHeaderTextBuilder AppendFileName(ZString value)
	{
		AppendKeyValuePair(XmlElementName.FileName, value);
		return this;
	}

	public InterchangeHeaderTextBuilder AppendTrackingID(ZGuid value)
	{
		AppendKeyValuePair(XmlElementName.TrackingID, value.ToString());
		return this;
	}

	public ZString Build()
	{
		using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
		{
			using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings() { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment }))
			{
				xmlWriter.WriteStartElement(XmlElementName.ITMessage);
				foreach (var item in valuePairList)
				{
					xmlWriter.WriteElementString(item.Key, item.Value);
				}
			}
			return stringWriter.ToString();
		}
	}

	readonly List<KeyValuePair<ZString, ZString>> valuePairList = new List<KeyValuePair<ZString, ZString>>();

	void AppendKeyValuePair(ZString key, ZString value) => valuePairList.Add(new KeyValuePair<ZString, ZString>(key, value));

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant Xml Element Name")]
	public static class XmlElementName
	{
		public const string ITMessage = "ITMessage";
		public const string Staff = "Staff";
		public const string Node = "Node";
		public const string MessageType = "MessageType";
		public const string AccountNumber = "AccountNumber";
		public const string Header = "Header";
		public const string FileName = "FileName";
		public const string TrackingID = "eHubTrackingIDFromSentInterchange";
	}
}
