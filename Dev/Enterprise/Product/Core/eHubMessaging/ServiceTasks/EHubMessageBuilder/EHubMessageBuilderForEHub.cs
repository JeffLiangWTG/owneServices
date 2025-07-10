using System;
using System.Globalization;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder
{
	public class EHubMessageBuilderForEHub : EHubMessageBuilderForXml
	{
		public EHubMessageBuilderForEHub(EDIInterchange interchange, INotifications notifier)
			: base(interchange, notifier)
		{
		}

		protected override bool RequiresMessage()
		{
			return false;
		}

		protected override string GetSchemaName()
		{
			switch (interchange.EI_InterchangeType)
			{
				case EDIInterchangeTypeList.Codes.MSS:
					return EDIInterchangeTypeList.Descriptions.MSS;
				case EDIInterchangeTypeList.Codes.MSF:
					return EDIInterchangeTypeList.Descriptions.MSF;
				case EDIInterchangeTypeList.Codes.MessageStatusAcknowledgment:
					return EDIInterchangeTypeList.Descriptions.MessageStatusAcknowledgment;
				case EDIInterchangeTypeList.Codes.EHubRegistryUpdate:
					return EDIInterchangeTypeList.Descriptions.EHubRegistryUpdate;
				case EDIInterchangeTypeList.Codes.Configuration:
				case EDIInterchangeTypeList.Codes.ForwarderConfiguration:
				case EDIInterchangeTypeList.Codes.TWCustomsLicensing:
					return EDIInterchangeTypeList.Descriptions.Configuration;
				case EDIInterchangeTypeList.Codes.ITCustomsRequestResponse:
					return EDIInterchangeTypeList.Descriptions.ITCustomsRequestResponse;
				default:
					throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Schema name not found for interchange type '{0}'", interchange.EI_InterchangeType));
			}
		}

		protected override IeHubMessage BuildCore()
		{
			return new eHubMessage(
				interchange.EI_SessionGUID.ToGuid(),
				interchange.EI_From,
				interchange.EI_To,
				MessageSchemaType.Xml,
				interchange.EI_ApplicationCode,
				GetSchemaName(),
				new MemoryStream(System.Text.Encoding.UTF8.GetBytes(interchange.EI_BodyText))
				);
		}
	}
}
