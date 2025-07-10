using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class STREQRMessageProcessor : CMRMessageResponseProcessor
	{
		public STREQRMessageProcessor(LoggingInformation logger)
			: base(logger, CMRMessage.CMRMessageTypes.STREQ, "Status Request Response (STREQR)")
		{
		}

		#region Implementation

		protected override bool DoAdditionalProcessing()
		{
			bool result = base.DoAdditionalProcessing();
			if (result)
			{
				ExportCustomsManifestLines exportLine = incomingMessage.EM_LinkedObject as ExportCustomsManifestLines;
				if (exportLine == null)
				{
					Logger.LogWarning("The incoming STREQR message is not responding to a ExportCustomsManifestLine.  Can't continue.");
					result = false;
				}
				else
				{
					CMR3CharDocumentStatus docStatus = CMR3CharDocumentStatus.GetFromStatusText(statusType);
					exportLine.EL_DocumentStatus = docStatus != null ? docStatus.Code : "";
					CMR3CharDocumentStatusConditions docStatusConditions = CMR3CharDocumentStatusConditions.GetFromStatusText(statusType);
					exportLine.EL_DocumentStatusConditions = docStatusConditions != null ? docStatusConditions.Code : "";
					result = true;
				}
			}
			return result;
		}

		#region Email groups
		protected override ZGuid AcknowledgementEmailGroup
		{
			get
			{
				return Env.Registry.AUCustoms.ExportManifestSendAcknowledgementsToGroup;
			}
		}

		protected override ZString AcknowledgementEmailMode
		{
			get
			{
				return Env.Registry.AUCustoms.ExportManifestSendAcknowledgements;
			}
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get
			{
				return Env.Registry.AUCustoms.ExportManifestSendImpedimentsToGroup;
			}
		}

		protected override ZString ImpedimentEmailMode
		{
			get
			{
				return Env.Registry.AUCustoms.ExportManifestSendImpediments;
			}
		}

		protected override ZGuid ErrorEmailGroup
		{
			get
			{
				return Env.Registry.AUCustoms.ExportManifestSendErrorsToGroup;
			}
		}

		protected override ZString ErrorEmailMode
		{
			get
			{
				return Env.Registry.AUCustoms.ExportManifestSendErrors;
			}
		}
		#endregion

		#endregion
	}
}
