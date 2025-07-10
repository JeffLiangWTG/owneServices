using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Messaging.Business.XmlMessaging
{
	public class XmlEDIInterchange : EDIInterchange, IXmlEDIInterchange
	{
		public XmlEDIInterchange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EI_IsActive = true;
			EI_ApplicationCode = EDIInterchange.ApplicationCodes.XMS;
			EI_Status = EDIInterchange.Status.eHubQueued;
			EI_TransportType = EDIInterchange.TransportType.eHub;
			EI_SystemCreateTimeUtc = ZDateTime.UtcNow;
			EI_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
			EI_SystemLastEditTimeUtc = EI_SystemCreateTimeUtc;
			EI_SystemLastEditUser = EI_SystemCreateUser;
			EI_SessionGUID = PK;
		}

		#region BusinessObjectCollections
		public new XmlEDIInterchangeEDIMessageCollection ContainedMessages
		{
			get
			{
				if (fContainedMessages == null)
				{
					fContainedMessages = GetNewContainedMessagesCollection();
					fContainedMessages.Load();
					fContainedMessages.IsManagedForDataRefresh = true;
				}
				return fContainedMessages;
			}
		}
		new XmlEDIInterchangeEDIMessageCollection fContainedMessages;

		protected new XmlEDIInterchangeEDIMessageCollection GetNewContainedMessagesCollection()
		{
			return new XmlEDIInterchangeEDIMessageCollection(this, Factory);
		}

		#endregion

		protected override ZString GetInterchangeNumber()
		{
			if (NumberStrategy == null)
			{
				return Environment.Env.Instance.NumberFountains.XmlEDIInterchangeNumber.GetNextFormatted(Factory);
			}

			return base.GetInterchangeNumber();
		}

		#region IXmlEDIInterchange Members

		public ZString GetInterchangeTypeFromFileFormat(string fileFormat)
		{
			switch (fileFormat)
			{
				case EDICommunicationsModeFileFormatList.Codes.XML:
				case EDICommunicationsModeFileFormatList.Codes.EXL:
					return EDIInterchangeTypeList.Codes.XMS;

				case EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent:
				case EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment:
					return EDIInterchangeTypeList.Codes.XDC;

				default:
					return EDIInterchangeTypeList.Codes.Unknown;
			}
		}

		public IXmlEDIMessage AddNeweHubMessage()
		{
			var message = ContainedMessages.AddNew();
			message.EM_ECC_CommunicationPartyConfig = EI_ECC_CommunicationPartyConfig;
			return message;
		}

		public void AddMessage(IEDIMessage message)
		{
			ContainedMessages.Add(message as BusinessObject);
		}

		protected override void UpdateEDIMessageTransportType()
		{
			foreach (XmlEDIMessage message in ContainedMessages)
			{
				message.EM_TransportType = EI_TransportType;
			}
		}

		#endregion
	}
}
