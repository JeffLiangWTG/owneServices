using System;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CINImportResponseMessageDataObject : MessageDataObject, ICINResponseDataProvider
	{
		public CINImportResponseMessageDataObject(FREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier()
		{
			return new CINImportResponsePrettier(this);
		}

		public ZString MessageID => ResponseMessageID;

		public ZBool Success => true; // TODO - Business Logic yet to be defined

		#region Simulate Response Object Model - The response has not yet been defined
		ZString ResponseMessageID
		{
			get
			{
				var attribute = ResponseHeader?.Attribute("messageId");
				return attribute == null ? ZString.Empty : new ZString(attribute.Value);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Element strings")]
		XElement ResponseHeader => ResponseCINMessage?.Element("Header");
		XElement ResponseCINMessage => Response?.Root;

		XDocument Response => response ?? (response = ExtractResponse());
		XDocument response;

		XDocument ExtractResponse()
		{
			try
			{
				var doc = XDocument.Parse(EDIMessage.EM_MessageText);

				return doc;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return null;
			}
		}
		#endregion
	}
}
