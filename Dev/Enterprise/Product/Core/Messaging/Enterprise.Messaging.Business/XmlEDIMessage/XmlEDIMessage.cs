using System.Data;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Messaging.Business.XmlMessaging
{
	public class XmlEDIMessage : EDIMessage, IXmlEDIMessage
	{
		public XmlEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_IsActive = true;
			EM_ApplicationCode = EDIMessage.ApplicationCodes.XMS;
			EM_Status = EDIMessage.Status.Sent;
			EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			EM_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
			EM_SystemLastEditTimeUtc = EM_SystemCreateTimeUtc;
			EM_SystemLastEditUser = EM_SystemCreateUser;
			EM_MessageType = MessageType;
		}

		public const string MessageType = EDIMessageTypeList.Codes.XMS;

		protected override void PopulateMessageNumber()
		{
			if (MessageNumberStrategy != null)
			{
				EM_MessageNum = MessageNumberStrategy.GetMessageReferenceNumber();
			}
			else if (EM_MessageNum.IsEmpty)
			{
				EM_MessageNum = Env.NumberFountains.XmlEDIMessageNumber.GetNextFormatted(Factory);
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (!IsTransmitMessage && !IsInDatabase)
			{
				PopulateMessageNumber();
			}
		}

		#region IXmlEDIMessage Members

		public XElement Content
		{
			set
			{
				EM_MessageText = value.ToString();
			}
			get
			{
				var messageText = EM_MessageText;
				if (messageText.IsEmpty)
				{
					return null;
				}
				return XElement.Parse(messageText);
			}
		}

		public void SetContent(Stream stream)
		{
			using (var reader = XmlReader.Create(stream))
			{
				Content = XElement.Load(reader);
			}
		}

		public void SetMessageTypeFromStream(Stream stream)
		{
			EM_MessageSubType = XmlMessageHelper.GetMessageSubTypeFromXml(stream);
		}

		#endregion
	}
}
