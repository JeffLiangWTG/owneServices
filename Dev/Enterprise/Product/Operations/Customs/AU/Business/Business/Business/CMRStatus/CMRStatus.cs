using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Edifact;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CMRStatus
	{
		protected CUSRESMessage GetLastCUSRESOfType(EDIMessageCollection messages, ZString messageType)
		{
			EDIMessage[] view = messages.GetMatchingMessages(EDIMessage.ApplicationCodes.CMR, new ZString[] { messageType }, EDIMessage.Direction.Receive);
			if (view.Length > 0)
			{
				return view[0].GetAutoEdifactMessageUsingNamedFactory(AuEdifactMessageFactory.AUCMessageFactory, new UNOCCMRCharacterSet()) as CUSRESMessage;
			}

			return null;
		}

		protected CMRDocumentStatus GetDocumentStatus(CUSRESMessage cUSRES)
		{
			if (cUSRES != null)
			{
				ZString statusString = GetStatusString(cUSRES);
				foreach (CMRDocumentStatus status in new CMRDocumentStatusList())
				{
					if (statusString.IndexOf(status.Code) != -1)
					{
						return status;
					}
				}
			}
			return null;
		}

		protected CMRNoticeStatus GetNoticeStatus(CUSRESMessage cUSRES)
		{
			if (cUSRES != null)
			{
				ZString statusString = GetStatusString(cUSRES);
				foreach (CMRNoticeStatus status in new CMRNoticeStatusList())
				{
					if (statusString.IndexOf(status.Code) != -1)
					{
						return status;
					}
				}
			}
			return null;
		}

		protected CMRMovementStatus GetMovementStatus(CUSRESMessage cUSRES)
		{
			if (cUSRES != null)
			{
				ZString statusString = GetStatusString(cUSRES);
				foreach (CMRMovementStatus status in new CMRMovementStatusList())
				{
					if (statusString.IndexOf(status.Code) != -1)
					{
						return status;
					}
				}
			}
			return null;
		}

		protected CMRDocumentStatusConditions GetDocumentStatusConditions(CUSRESMessage cUSRES)
		{
			if (cUSRES != null)
			{
				ZString statusString = GetStatusString(cUSRES);
				foreach (CMRDocumentStatusConditions statusConditions in new CMRDocumentStatusConditionsList())
				{
					if (statusString.IndexOf(statusConditions.Code) != -1)
					{
						return statusConditions;
					}
				}
			}
			return null;
		}

		protected DocumentStatusAndDocumentStatusConditions[] GetLineDocumentStatusAndDocumentStatusConditions(EDIMessageCollection messages, ZString messageType)
		{
			EDIMessage[] view = messages.GetMatchingMessages(EDIMessage.ApplicationCodes.CMR, new ZString[] { messageType }, EDIMessage.Direction.Receive);
			LineDetails[] lineStatus = GetLineStatusString(view);
			DocumentStatusAndDocumentStatusConditions[] result = new DocumentStatusAndDocumentStatusConditions[lineStatus.Length];

			for (int i = 0; i < lineStatus.Length; i++)
			{
				result[i] = new DocumentStatusAndDocumentStatusConditions();
				foreach (CMRDocumentStatus status in new CMRDocumentStatusList())
				{
					if (lineStatus[i].Status.IndexOf(status.Code) != -1)
					{
						result[i].DocumentStatus = status;
					}
				}
				foreach (CMRDocumentStatusConditions statusConditions in new CMRDocumentStatusConditionsList())
				{
					if (lineStatus[i].Status.IndexOf(statusConditions.Code) != -1)
					{
						result[i].DocumentStatusConditions = statusConditions;
					}
				}
				result[i].CAN = lineStatus[i].CAN;
			}
			return result;
		}

		ZString GetStatusString(CUSRESMessage cUSRES)
		{
			return cUSRES.FTX[0].TextLiteral.FreeTextValue1;
		}

		LineDetails[] GetLineStatusString(EDIMessage[] messages)
		{
			ZInt maximumLineNumber = -1;
			LineDetails[][] lineDetailsArrayArray = new LineDetails[messages.Length][];
			int arrayIndex = 0;
			foreach (EDIMessage message in messages)
			{
				lineDetailsArrayArray[arrayIndex] = GetLineStatusStrings(message.GetAutoEdifactMessageUsingNamedFactory(AuEdifactMessageFactory.AUCMessageFactory, new UNOCCMRCharacterSet()) as CUSRESMessage);
				if (lineDetailsArrayArray[arrayIndex].Length > maximumLineNumber)
				{
					maximumLineNumber = lineDetailsArrayArray[arrayIndex].Length;
				}

				arrayIndex++;
			}

			LineDetails[] result = new LineDetails[maximumLineNumber];

			for (int i = lineDetailsArrayArray.Length - 1; i >= 0; i--)
			{
				for (int j = 0; j < lineDetailsArrayArray[i].Length; j++)
				{
					if (lineDetailsArrayArray[i][j] != null)
					{
						result[j] = lineDetailsArrayArray[i][j];
					}
				}
			}
			return result;
		}

		LineDetails[] GetLineStatusStrings(CUSRESMessage cUSRES)
		{
			LineDetails[] result = System.Array.Empty<LineDetails>();
			if (cUSRES != null)
			{
				result = new LineDetails[GetNumberOfLines(cUSRES)];
				foreach (SegmentGroup6 group6 in cUSRES.Group6)
				{
					LineDetails details = null;
					ZInt lineNumber = -1;
					foreach (SegmentGroup11 group11 in group6.Group11)
					{
						ZInt.TryParse(group11.CST[0].GoodsItemNumber, out lineNumber);
						if (lineNumber > 0)
						{
							details = new LineDetails();
							details.Status = group11.FTX[0].TextLiteral.FreeTextValue1;
						}
					}
					if (lineNumber != -1 && details != null)
					{
						foreach (RFFSegment rFF in group6.RFF)
						{
							if (rFF.Reference.ReferenceFunctionCodeQualifier.ToString() == ReferenceFunctionCodeQualifierList.TaxExemptionLicenceNumber)
							{
								details.ExportDeclarationExemptionCode = rFF.Reference.ReferenceIdentifier;
							}
							else if (rFF.Reference.ReferenceFunctionCodeQualifier.ToString() == ReferenceFunctionCodeQualifierList.TransactionReferenceNumber)
							{
								details.CAN = rFF.Reference.ReferenceIdentifier;
							}
						}
						result[lineNumber - 1] = details;
					}
				}
			}
			return result;
		}

		ZInt GetNumberOfLines(CUSRESMessage cUSRES)
		{
			ZInt maximumLineNumber = -1;
			foreach (SegmentGroup6 group6 in cUSRES.Group6)
			{
				foreach (SegmentGroup11 group11 in group6.Group11)
				{
					ZInt lineNumber;
					ZInt.TryParse(group11.CST[0].GoodsItemNumber, out lineNumber);
					if (lineNumber > maximumLineNumber)
					{
						maximumLineNumber = lineNumber;
					}
				}
			}
			return maximumLineNumber;
		}

		class LineDetails
		{
			public ZString Status;
			public ZString ExportDeclarationExemptionCode;
			public ZString CAN;
		}
	}
}
