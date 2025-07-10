using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common
{
	public static class CustomsGenericMessageHelper
	{
		public static void PopulateGMDInterchange(EDIInterchange interchange, ZString recipient, ZString interchangeType, ZString bodyText, bool applyXMLEscape = false)
		{
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.GenericMessageDelivery;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_InterchangeType = interchangeType;
			interchange.EI_From = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			interchange.EI_To = recipient;
			interchange.EI_Status = EDIInterchange.Status.eHubQueued;
			interchange.EI_BodyText = applyXMLEscape ? new ZString(System.Security.SecurityElement.Escape(bodyText)) : bodyText;
			interchange.EI_SessionGUID = ZGuid.NewZGuid();
		}

		public static EDIInterchange CreateGMDInterchange(BusinessObjectFactory factory, ZString recipient, ZString interchangeType, ZString bodyText, bool applyXMLEscape = false)
		{
			var interchange = factory.New<EDIInterchange>();
			PopulateGMDInterchange(interchange, recipient, interchangeType, bodyText, applyXMLEscape);
			return interchange;
		}

		public static EDIInterchange CreateGMDInterchange(EDIMessage message, ZString recipient, bool applyXMLEscape = false)
		{
			var result = CreateGMDInterchange(message.Factory, recipient, message.EM_ApplicationCode, message.EM_MessageText, applyXMLEscape);
			result.ContainedMessages.Add(message);
			return result;
		}

		public static EDIInterchange CreatRefDbRepoMessage(BusinessObjectFactory factory, ZString source, ZString subSource, ZString contentType, ZString data, bool applyXMLEscape = false)
		{
			var bodytext = (ZString)string.Format(CultureInfo.CurrentCulture, (NoResString)@"<RefDbRepoMessage><Source>{0}</Source><SubSource>{1}</SubSource><ContentType>{2}</ContentType><Data>{3}</Data></RefDbRepoMessage>", source, subSource, contentType, applyXMLEscape ? System.Security.SecurityElement.Escape(data) : data.ToString());
			var recipient = Environment.Env.Instance.IsProductionSystem ? "CUSTOMS_DATA_REPO" : "CUSTOMS_DATA_REPO_TEST";

			return CreateGMDInterchange(factory, recipient, EDIInterchangeTypeList.Codes.RefDataRepoMessage, bodytext);
		}

		public static EDIInterchange CreateGMDInterchange(EDIMessage message, ZString recipient, ZString bodyText, bool applyXMLEscape = false)
		{
			var result = CreateGMDInterchange(message.Factory, recipient, message.EM_ApplicationCode, bodyText, applyXMLEscape);
			result.ContainedMessages.Add(message);
			return result;
		}
	}
}
