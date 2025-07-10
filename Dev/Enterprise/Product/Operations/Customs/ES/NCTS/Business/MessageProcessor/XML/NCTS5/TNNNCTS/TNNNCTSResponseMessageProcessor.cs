using System;
using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CCTNNC_v515.CCTNNCV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class TNNNCTSResponseMessageProcessor : NCTS5CommonResponseMessageProcessor<Cctnncv1Sal, TNNNCTSMessagePrettyFormatter>
	{
		public TNNNCTSResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		const string TNNOKCode = "A";

		protected override string MessageFriendlyNameCore => (NoResString)"NCTS TNN Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameCCTNNCV1Sal;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration };

		protected override TNNNCTSMessagePrettyFormatter GetNewMessagePrettyFormatter(Cctnncv1Sal response, EDIMessage message, NctsHeader nctsHeader) => new TNNNCTSMessagePrettyFormatter(response);

		protected override ZString ProcessAcceptedDeclaration(Cctnncv1Sal response, EDIMessage message, NctsHeader nctsHeader)
		{
			if (!nctsHeader.IsArrivalMovement)
			{
				throw new InvalidOperationException(SetNctsMessageFailedLogDescription(nctsHeader, "Departure"));
			}

			var departureHeaderTNN = nctsHeader.ArrivalMovementHeader.HeaderTNN ?? throw new InvalidOperationException(Res.GetString("C3AD733B-08C2-448D-B50C-7F4E80276381", "Arrival has no TNN Departure associated so can't process response message"));
			if (response.ControlRespuesta.CodigoRespuesta == TNNOKCode)
			{
				departureHeaderTNN.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
			}

			return ZString.Empty;
		}

		protected override void SetExtraCHStatus(NctsHeader businessObject, ZString messageStatus)
		{
			var departureHeaderTNN = businessObject.ArrivalMovementHeader?.HeaderTNN;
			if (departureHeaderTNN != null)
			{
				departureHeaderTNN.MovementHeader.BM_MessageStatus = messageStatus;
			}
		}

		const string XsdSchemaNameCCTNNCV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.Incoming.CCTNNCV1Sal.xsd";
	}
}
