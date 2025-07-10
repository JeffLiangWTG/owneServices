using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Business
{
	public class EDIMessageTypeDecider : TypeDecider, Integration.Customs.IE.IEDIMessageTypeDecider
	{
		public override Type GetTypeForBinding() => null;

		public override Type GetTypeForNew() => null;

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory) => row[EDIMessageSchema.Constants.EM_ApplicationCode] switch
		{
			EDIMessage.ApplicationCodes.IECustomsExport => row[EDIMessageSchema.Constants.EM_ReceiveTransmit] switch
			{
				EDIMessage.Direction.Transmit => typeof(AESOutboundEDIMessage),
				EDIMessage.Direction.Receive => typeof(AESInboundEDIMessage),
				_ => throw new ArgumentOutOfRangeException(nameof(row), EDIMessageSchema.Constants.EM_ReceiveTransmit)
			},

			EDIMessage.ApplicationCodes.IECustomsImport => row[EDIMessageSchema.Constants.EM_ReceiveTransmit] switch
			{
				EDIMessage.Direction.Transmit => typeof(AISOutboundEDIMessage),
				EDIMessage.Direction.Receive => typeof(AISInboundEDIMessage),
				_ => throw new ArgumentOutOfRangeException(nameof(row), EDIMessageSchema.Constants.EM_ReceiveTransmit)
			},

			EDIMessage.ApplicationCodes.IECustomsUCC5Import => row[EDIMessageSchema.Constants.EM_ReceiveTransmit] switch
			{
				EDIMessage.Direction.Transmit => typeof(AISUCC5OutboundEDIMessage),
				EDIMessage.Direction.Receive => typeof(AISUCC5InboundEDIMessage),
				_ => throw new ArgumentOutOfRangeException(nameof(row), EDIMessageSchema.Constants.EM_ReceiveTransmit)
			},

			EDIMessage.ApplicationCodes.IECustomsEMCS => ((TypeDecider)ObjectFactory.Get<Integration.Customs.IEEMCS.IEDIMessageTypeDecider>()).GetTypeForLoad(row, factory),

			EDIMessage.ApplicationCodes.IECustomsNCTS => ((TypeDecider)ObjectFactory.Get<Integration.Customs.IENCTS.IEDIMessageTypeDecider>()).GetTypeForLoad(row, factory),

			EDIMessage.ApplicationCodes.IECustomsAndExcise => row[EDIMessageSchema.Constants.EM_ReceiveTransmit] switch
			{
				EDIMessage.Direction.Transmit => typeof(CustomsAndExciseReportOutboundMessage),
				EDIMessage.Direction.Receive => typeof(CustomsAndExciseReportInboundMessage),
				_ => throw new ArgumentOutOfRangeException(nameof(row), EDIMessageSchema.Constants.EM_ReceiveTransmit)
			},

			EDIMessage.ApplicationCodes.IECustomsPBN => ((TypeDecider)ObjectFactory.Get<Integration.Customs.IEPBN.IEDIMessageTypeDecider>()).GetTypeForLoad(row, factory),

			_ => typeof(EDIMessage)
		};
	}
}
