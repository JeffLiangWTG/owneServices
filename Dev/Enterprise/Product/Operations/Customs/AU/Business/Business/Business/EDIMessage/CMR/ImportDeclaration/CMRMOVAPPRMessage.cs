using System.Data;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRMOVAPPRMessage : CMRImportDeclarationMessage
	{
		public CMRMOVAPPRMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.MOVAPP;
		}

		public override ZString GetReport()
		{
			StringBuilder result = new StringBuilder();
			if (CUSRES != null)
			{
				result.Append(GetRFFReport());
				result.Append(GetFTXReport());
				result.Append(GetErrorReport());
			}
			return result.ToString();
		}

		ZString GetFTXReport()
		{
			StringBuilder result = new StringBuilder();
			if (CUSRES.Group3.Count > 0)
			{
				foreach (RFFSegment currentRFF in CUSRES.Group3[0].RFF)
				{
					if (currentRFF.Reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.OriginatorsReference)
					{
						result.Append("Declaration Reference:\t" + currentRFF.Reference.ReferenceIdentifier);
					}
					else if (currentRFF.Reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.ApplicationReferenceNumber)
					{
						result.Append("Movement Application No:\t" + currentRFF.Reference.ReferenceIdentifier);
					}
				}
			}
			return result.ToString();
		}

		ZString GetRFFReport()
		{
			StringBuilder result = new StringBuilder();
			if (CUSRES.FTX.Count > 0)
			{
				foreach (FTXSegment currentFTX in CUSRES.FTX)
				{
					if (currentFTX.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.AdditionalConditions)
					{
						result.Append("Movement Application Additional Condition:\t" + currentFTX.TextLiteral.FreeTextValue1);
					}
					else if (currentFTX.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.StatusDetails)
					{
						result.Append("Status for " + currentFTX.TextLiteral.FreeTextValue1 + ":\t" + currentFTX.TextLiteral.FreeTextValue2);
					}
				}
			}
			return result.ToString();
		}

		ZString GetErrorReport()
		{
			StringBuilder result = new StringBuilder();
			if (CUSRES.Group4.Count > 0)
			{
				foreach (SegmentGroup4 currentGroup4 in CUSRES.Group4)
				{
					result.Append(currentGroup4.ERP[0].ErrorPointDetails.MessageSubItemNumber + currentGroup4.FTX[0].TextLiteral.FreeTextValue1);
				}
			}
			return result.ToString();
		}
	}
}
