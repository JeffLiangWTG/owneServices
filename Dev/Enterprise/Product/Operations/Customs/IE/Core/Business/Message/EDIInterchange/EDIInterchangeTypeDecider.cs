using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.IE;

namespace Enterprise.Customs.IE.Business
{
	public class EDIInterchangeTypeDecider : TypeDecider, IEDIInterchangeTypeDecider
	{
		public override Type GetTypeForBinding() => null;

		public override Type GetTypeForNew() => null;

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory) => (
			new ZString(row[EDIInterchangeSchema.Constants.EI_ReceiveTransmit]).ToUpperInvariant().ToString(),
			new ZString(row[EDIInterchangeSchema.Constants.EI_ApplicationCode]).ToUpperInvariant().ToString(),
			new ZString(row[EDIInterchangeSchema.Constants.EI_InterchangeType]).ToUpperInvariant().ToString()) switch
		{
			(EDIInterchange.Direction.Transmit, EDIInterchange.ApplicationCodes.IECustomsExport, AESOutgoingMessageTypeList.Codes.DocumentUpload) => typeof(AESOutboundMessageDataProviderEDIInterchange),
			(EDIInterchange.Direction.Transmit, EDIInterchange.ApplicationCodes.IECustomsUCC5Import, AISUploadDocumentsMessageTypeList.Codes.IM483) => typeof(AISUCC5OutboundMessageDataProviderEDIInterchange),
			_ => typeof(EDIInterchange)
		};
	}
}
