using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ManifestMessageResponseProcessor : CMRMessageResponseProcessor
	{
		public ManifestMessageResponseProcessor(LoggingInformation logger, ZString messageCode, ZString messageName)
			: base(logger, messageCode, messageName)
		{
		}

		#region Implementation

		protected override bool DoAdditionalProcessing()
		{
			bool result = false;
			ZString exemptionCode = GetExemptionCode(cUSRES);

			ZString cRN = GetCRN(cUSRES);

			ZString mMN = GetMMN(cUSRES);

			ForwardingConsol consol = incomingMessage.EM_LinkedObject as ForwardingConsol;
			ExportCustomsManifestHeader header = incomingMessage.EM_LinkedObject as ExportCustomsManifestHeader;
			if (consol != null)
			{
				FreightConsolWrapper consolWrapper = new FreightConsolWrapper(consol);

				if (statusType == "WITHDRAWN")
				{
					AUCusEntryNumber permit = consolWrapper.GetPermit();
					if (permit != null)
					{
						permit.Delete();
					}
				}
				else
				{
					if (!cRN.IsEmpty)
					{
						AUCusEntryNumber permit = consolWrapper.GetPermit()
							?? consolWrapper.CreateCusEntryNumber();

						permit.CE_EntryNum = cRN;
					}
					if (!mMN.IsEmpty)
					{
						AUCusEntryNumber permit = consolWrapper.GetMainManifestNumber()
							?? consolWrapper.CreateMainManifestNumber();

						permit.CE_EntryNum = mMN;
					}
				}

				//				ZString CAN = GetCAN(CUSRES);
				//				if (!CAN.IsEmpty)
				//				{
				//					AUCusEntryNumber Permit = ConsolWrapper.GetCustomsAuthorityNumber();
				//					if (Permit == null) Permit = ConsolWrapper.CreateCustomsAuthorityNumber();
				//					Permit.CE_EntryNum = CAN;
				//				}
				result = true;
			}
			else if (header != null)
			{
				if (statusType == "WITHDRAWN")
				{
					header.ED_CAN = ZString.Empty;
				}
				else
				{
					if (!cRN.IsEmpty)
					{
						header.ED_CAN = cRN;
					}
					else if (!mMN.IsEmpty)
					{
						header.ED_CAN = mMN;
					}
				}

				CMR3CharDocumentStatus docStatus = CMR3CharDocumentStatus.GetFromStatusText(statusType);
				header.ED_DocumentStatus = docStatus != null ? docStatus.Code : "";
				CMR3CharDocumentStatusConditions docStatusConditions = CMR3CharDocumentStatusConditions.GetFromStatusText(statusType);
				header.ED_DocumentStatusConditions = docStatusConditions != null ? docStatusConditions.Code : "";
				foreach (SegmentGroup6 group6 in cUSRES.Group6)
				{
					foreach (SegmentGroup11 group11 in group6.Group11)
					{
						int lineNumber = int.Parse(group11.CST[0].GoodsItemNumber);
						ZString lineStatusType = group11.FTX[0].TextLiteral.FreeTextValue1;
						ExportCustomsManifestLines line = header.Lines.GetLine(lineNumber);
						if (line != null)
						{
							CMR3CharDocumentStatus lineDocStatus = CMR3CharDocumentStatus.GetFromStatusText(lineStatusType);
							line.EL_DocumentStatus = lineDocStatus != null ? lineDocStatus.Code : "";
							CMR3CharDocumentStatusConditions lineDocStatusConditions = CMR3CharDocumentStatusConditions.GetFromStatusText(lineStatusType);
							line.EL_DocumentStatusConditions = lineDocStatusConditions != null ? lineDocStatusConditions.Code : "";
						}
					}
				}
				result = true;
			}
			return result;
		}

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

		#region Data Retrievers

		protected ZString GetMMN(CUSRESMessage message)
		{
			foreach (SegmentGroup3 group3 in message.Group3)
			{
				foreach (RFFSegment rFF in group3.RFF)
				{
					if (rFF.Reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.CargoManifestNumber.ToString())
					{
						return rFF.Reference.ReferenceIdentifier;
					}
				}
			}
			return ZString.Empty;
		}

		protected ZString GetCRN(CUSRESMessage message)
		{
			foreach (SegmentGroup3 group3 in message.Group3)
			{
				foreach (RFFSegment rFF in group3.RFF)
				{
					if (rFF.Reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.ConsolidatedInvoiceNumber.ToString())
					{
						return rFF.Reference.ReferenceIdentifier;
					}
				}
			}
			return ZString.Empty;
		}

		protected ZString GetExemptionCode(CUSRESMessage message)
		{
			foreach (SegmentGroup6 group6 in message.Group6)
			{
				foreach (RFFSegment rFF in group6.RFF)
				{
					if (rFF.Reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.TaxExemptionLicenceNumber.ToString())
					{
						return rFF.Reference.ReferenceIdentifier;
					}
				}
			}
			return ZString.Empty;
		}

		#endregion

		#endregion
	}
}
