using System.Security;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;

namespace Enterprise.Client.EDI.ServiceTasks.XT.Subscribers
{
	public abstract class LicenceBaseAuditSubscriber : ActualDataChangesAuditSubscriber
	{
		protected virtual EDIInterchange CreateInterchangeMessage(string transportType, string xmlBody, ZDateTime changeDateTime)
		{
			var currentCompany = GlbCompany.GetCurrentCompany(DataFactory);
			var interchange = DataFactory.New<XmlEDIInterchange>();
			interchange.EI_From = currentCompany.LicenceKeyIdentifier;
			interchange.EI_To = RecipientId;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_TransportType = transportType;
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.XMS;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			interchange.EI_SessionGUID = interchange.PK;
			interchange.EI_BodyText = xmlBody;
			interchange.EI_InterchangeNum = null;
			interchange.EI_SystemCreateTimeUtc = changeDateTime;
			interchange.EI_GB = (currentCompany.FirstActiveBranch ?? currentCompany.Branches[0]).PK;
			interchange.EI_IsActive = true;

			return interchange;
		}

		protected virtual string CreateXMLMessage(string serverCode, string licenceType, string password, string oldPassword, string changeType, string enterpriseCode, ZDateTime changeDateTime, string databaseNumber)
		{
			var xmlDoc = new XmlDocument();
			var xml = $@"
	<CredentialChanges> 
		<CredentialChange>
			<ChangeType>{EscapeXml(changeType)}</ChangeType>
			<ChangeDateTime>{EscapeXml(changeDateTime.ToString("s"))}</ChangeDateTime>
			<EnterpriseCode>{EscapeXml(enterpriseCode)}</EnterpriseCode>
			<DatabaseCode>{EscapeXml(serverCode)}</DatabaseCode>
			<DatabaseNumber>{EscapeXml(databaseNumber)}</DatabaseNumber>
			<LicenceType>{EscapeXml(licenceType)}</LicenceType>
			<Password>{EscapeXml(password)}</Password>
			<OldPassword>{EscapeXml(oldPassword)}</OldPassword>
		</CredentialChange>
	</CredentialChanges>";
			xmlDoc.LoadXml(xml);
			xmlDoc.DocumentElement.SetAttribute("xmlns", "http://cargowise.com/xhub/credentials");

			return xmlDoc.OuterXml;
		}

		public BusinessObjectFactory DataFactory => dataFactory ?? (dataFactory = new BusinessObjectFactory { RefreshEnabled = false });
		BusinessObjectFactory dataFactory;

		string RecipientId => "XH";

		static string EscapeXml(string value) => SecurityElement.Escape(value);
	}
}
