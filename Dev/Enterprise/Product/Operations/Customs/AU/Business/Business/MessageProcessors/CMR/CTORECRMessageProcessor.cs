using System.Collections;
using Enterprise.BatchProcessor;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CTORECRMessageProcessor : CMRMessageResponseProcessor
	{
		public CTORECRMessageProcessor(LoggingInformation logger)
			: base(logger, CMRMessage.CMRMessageTypes.CTOREC, "CTO Receival Notice Response(CTORECR)")
		{
		}

		#region Implementation

		protected override bool DoAdditionalProcessing()
		{
			if (!base.DoAdditionalProcessing())
			{
				return false;
			}

			ArrayList movementStatus = new ArrayList();
			ArrayList statusDescriptions = new ArrayList();
			foreach (SegmentGroup6 group6 in cUSRES.Group6)
			{
				foreach (SegmentGroup11 group11 in group6.Group11)
				{
					foreach (FTXSegment fTX in group11.FTX)
					{
						movementStatus.Add(fTX.TextLiteral.FreeTextValue1);
						statusDescriptions.Add(fTX.TextLiteral.FreeTextValue2);
					}
				}
			}
			if (movementStatus.Count == 1)
			{
				statusType = movementStatus[0].ToString();
				statusDescription = statusDescriptions[0].ToString();

				switch (movementStatus[0].ToString())
				{
					case "LOAD":
						incomingMessage.EM_MessageSubType = CMRMessage.MovementStatusResponseSubTypes.Load;
						break;
					case "DO NOT LOAD":
						incomingMessage.EM_MessageSubType = CMRMessage.MovementStatusResponseSubTypes.DoNotLoad;
						break;
					case "HOLD FOR CUSTOMS":
						incomingMessage.EM_MessageSubType = CMRMessage.MovementStatusResponseSubTypes.HoldForCustoms;
						break;
					default:
						incomingMessage.EM_MessageSubType = CMRMessage.MovementStatusResponseSubTypes.Unknown;
						break;
				}
			}
			return true;
		}

		#endregion
	}
}
