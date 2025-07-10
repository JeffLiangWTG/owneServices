using System;
using System.Collections.Generic;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BR.Business
{
	public class BRCInboundInterchangeProcessor : BranchInboundInterchangeProcessor
	{
		public BRCInboundInterchangeProcessor(IEnumerable<string> applicationCodes) : base(applicationCodes) { }

		protected override Type TypeOfInterchangeToCreate() => typeof(BREDIInterchange);

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange)
		{
			switch (interchange.EI_InterchangeType)
			{
				case MessageTypeList.Codes.XER:
					return new BRCResponseErrorInboundMessageCreator(Logger);
				case MessageTypeList.Codes.LIS:
					return new BRCImportLicenseStatusInboundMessageCreator(Logger);
				case MessageTypeList.Codes.LIC:
					return new BRCImportLicenseAcceptInboundMessageCreator(Logger);
				case MessageTypeList.Codes.CDC:
				case MessageTypeList.Codes.CDE:
					return new BRCExportInboundMessageCreator(Logger);
				case MessageTypeList.Codes.PUS:
					return new BRCPushNotificationInboundMessageCreator(Logger);
				case MessageTypeList.Codes.CAT:
					return new BRCCatalogInboundMessageCreator(Logger);
				case MessageTypeList.Codes.OPE:
					return new BRCForeignOperatorInboundMessageCreator(Logger);
				case MessageTypeList.Codes.RTT:
					return new BRCRequestTTCEReferenceFileInboundMessageCreator(Logger);
				default:
					return new BRCInboundMessageCreator(Logger);
			}
		}
	}
}
