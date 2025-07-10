using System;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public static class ProcessorHelper
	{
		public static void SendEmail(BusinessObjectFactory factory, string originalSendUser, bool isFailure, string messageBody, string emailSubject, Integration.IRegistryItem dataRegistry)
		{
			EmailDef emailDef = new EmailDef();
			emailDef.Subject = emailSubject;
			emailDef.Body = messageBody;
			emailDef.FromAddress = Env.Instance.Registry.SMTPDefaultDoNotReplyEmailAddress;

			GlbStaff originalSender = null;
			if (!string.IsNullOrEmpty(originalSendUser))
			{
				ZQuery staffQuery = new ZQuery(GlbStaffSchema.GS_Code, originalSendUser);
				originalSender = factory.LoadTop1<GlbStaff>(staffQuery);
			}
			var originalSenderEmailAddress = originalSender?.GS_EmailAddress ?? ZString.Empty;

			SendEmailToOriginalSenderOrGroupIfSenderInvalid(factory, emailDef, originalSenderEmailAddress, isFailure, dataRegistry);
		}

		static void SendEmailToOriginalSenderOrGroupIfSenderInvalid(BusinessObjectFactory factory, EmailDef email, ZString emailAddressToSendTo, bool hasErrorsOrAnomalies, Integration.IRegistryItem dataRegistry)
		{
			if (email != null)
			{
				var registryItem = dataRegistry;
				var registryItemGroupPK = GetEmailGroupPK(registryItem);
				var emailRepCal = new EmailRecipientCalculator(GetEmailSendMode(dataRegistry), registryItemGroupPK, emailAddressToSendTo, ZGuid.Empty);
				var supportMessageSuppressRegistry = registryItem as ISupportMessageSuppressRegistry;

				var shouldSendErrorEmailsOnly = supportMessageSuppressRegistry?.ShouldSEndErrorsOnly(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), ZGuid.Empty) ?? false;
				if (!shouldSendErrorEmailsOnly || (shouldSendErrorEmailsOnly && hasErrorsOrAnomalies))
				{
					emailRepCal.SendNotifications(factory, email, registryItem);
				}
			}
		}

		static ZString GetEmailSendMode(Integration.IRegistryItem dataRegistry)
		{
			var result = GroupNotification.StaffMemberOrNominatedGroup;
			var groupNotificationRegistry = dataRegistry.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty) as GroupNotification;
			if (groupNotificationRegistry != null)
			{
				result = groupNotificationRegistry.SendMode;
			}
			return result;
		}

		static ZGuid GetEmailGroupPK(Integration.IRegistryItem registryItem)
		{
			var result = ZGuid.Empty;
			if (registryItem != null)
			{
				result = ((GroupNotification)registryItem.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty)).SendGroupPK;
			}
			return result;
		}

		public static string GetElementTextByTagNameWithDefaultValue(XmlDocument xmlDoc, string myTagName, string defaultValue = "")
		{
			var outputValue = defaultValue;
			var nodeList = xmlDoc.GetElementsByTagName(myTagName);

			if (nodeList != null && nodeList.Count > 0)
			{
				outputValue = nodeList[0].InnerText;
			}

			return outputValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Xml strings")]
		public static string AdaptMessageToExpectedFormat(ZString messageText)
		{
			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(messageText);
			var messageType = xmlDocument.GetElementsByTagName("MesTypMES20")[0].InnerText;
			var nodeToKeep = xmlDocument.GetElementsByTagName(messageType)[0];

			var outputXmlDocument = new XmlDocument();

			var importedNode = outputXmlDocument.ImportNode(nodeToKeep, true);
			outputXmlDocument.AppendChild(importedNode);
			outputXmlDocument.DocumentElement.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
			outputXmlDocument.DocumentElement.SetAttribute("xmlns:xsd", "http://www.w3.org/2001/XMLSchema");
			outputXmlDocument.DocumentElement.SetAttribute("xmlns", "http://ncts.dgtaxud.ec/" + messageType);

			var xmlnsEmptyAttribute = outputXmlDocument.CreateAttribute("xmlns");
			xmlnsEmptyAttribute.Value = "";

			var reponseDeclarationNodes = outputXmlDocument.FirstChild.ChildNodes;
			foreach (XmlNode reponseDeclarationNode in reponseDeclarationNodes)
			{
				reponseDeclarationNode.Attributes.SetNamedItem(xmlnsEmptyAttribute);
			}

			return outputXmlDocument.InnerXml;
		}
	}
}
