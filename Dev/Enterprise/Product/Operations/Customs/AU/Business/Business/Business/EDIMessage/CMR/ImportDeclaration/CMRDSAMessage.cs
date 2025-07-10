using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRDSAMessage : CMRImportDeclarationMessage
	{
		public CMRDSAMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString GetStatusCore()
		{
			if (CUSRES != null && CUSRES.Group3.Count > 0)
			{
				return GetReference(CUSRES.Group3, ReferenceFunctionCodeQualifierList.GovernmentQualityAssuranceAndControlLevelNumber);
			}
			return "Unknown";
		}

		protected override ZString GetStatusDescriptionCore()
		{
			return ZString.Empty;
		}

		Dictionary<int, ZString> LineLevelCargoStatus
		{
			get
			{
				if (lineLevelCargoStatus == null)
				{
					lineLevelCargoStatus = new Dictionary<int, ZString>();
					if (CUSRES != null && CUSRES.Group6.Count > 0)
					{
						foreach (SegmentGroup6 group6 in CUSRES.Group6)
						{
							if (group6.DOC.Count > 0 && group6.DOC[0].DocumentMessageName.DocumentNameCode == "S")
							{
								ZShort lineNumber = -1;
								if (ZShort.TryParse(group6.DOC[0].DocumentMessageDetails.DocumentMessageNumber, out lineNumber) && lineNumber > 0 && !lineLevelCargoStatus.ContainsKey(lineNumber))
								{
									var status = GetFTXSegmentValue(group6, "CONSOLIDATED STATUS");
									lineLevelCargoStatus.Add(lineNumber, status);
								}
							}
						}
					}
				}
				return lineLevelCargoStatus;
			}
		}
		Dictionary<int, ZString> lineLevelCargoStatus;

		protected override ZString GetCargoStatusForTransportLineCore(ZShort cargoLineNumber)
		{
			ZString result = ZString.Empty;
			if (LineLevelCargoStatus.Count > 0)
			{
				LineLevelCargoStatus.TryGetValue(EnsureMinimumLineNumber(cargoLineNumber), out result);
			}

			if (result.IsEmpty)
			{
				result = "UNKNOWN";
			}

			return result;
		}

		protected override ZString GetCustomsStatusFromMessageCore()
		{
			ZString result = ZString.Empty;
			if (CUSRES != null && CUSRES.Group3.Count > 0)
			{
				result = GetReference(CUSRES.Group3, ReferenceFunctionCodeQualifierList.GovernmentQualityAssuranceAndControlLevelNumber);
			}
			return result;
		}

		public override ZString AdditionalInfoForStatusSectionOfReport()
		{
			ZStringBuilder result = new ZStringBuilder();
			if (CUSRES != null && CUSRES.Group6.Count > 0)
			{
				ZString impedementsHeading = "Impediments:\r\n";
				foreach (SegmentGroup6 group6 in CUSRES.Group6)
				{
					if (group6.DOC.Count > 0 && group6.DOC[0].DocumentMessageName.DocumentNameCode == "I")
					{
						if (!impedementsHeading.IsEmpty)
						{
							result.Append(impedementsHeading);
							impedementsHeading = ZString.Empty;
						}
						result.Append(GetOneGroup6Representation(group6));
					}
				}
			}
			return result.ToString();
		}

		ZString GetOneGroup6Representation(SegmentGroup6 group6)
		{
			ZStringBuilder result = new ZStringBuilder();
			foreach (RFFSegment rFF in group6.RFF)
			{
				result.Append(GetRFFSegmentDescription(rFF) + "\r\n");
			}
			foreach (FTXSegment fTX in group6.FTX)
			{
				if (fTX.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.GeneralInformation)
				{
					result.Append("Advice Note: " + fTX.TextLiteral.FreeTextValue1 + "\r\n");
				}
				else if (fTX.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.ResponseFreeText)
				{
					result.Append("Profile Reason Description: " + fTX.TextLiteral.FreeTextValue1 + "\r\n");
				}
				else
				{
					result.Append(fTX.TextSubjectCodeQualifier.ToString() + ": " + fTX.TextLiteral.FreeTextValue1 + "\r\n");
				}
			}
			if (group6.DTM.Count > 0)
			{
				ZString screeningExpiryDate = ZString.Empty;
				ZString screeningExpiryTime = ZString.Empty;
				foreach (DTMSegment dTM in group6.DTM)
				{
					if (dTM.DateTimePeriod.DateTimePeriodFunctionCodeQualifier == DateTimePeriodFunctionCodeQualifierList.ExpirationDateTimeOfCustomsDocument)
					{
						if (dTM.DateTimePeriod.DateTimePeriodFormatCode == DateTimePeriodFormatCodeList.Ccyymmdd)
						{
							screeningExpiryDate = dTM.DateTimePeriod.DateTimePeriodValue;
						}
						else if (dTM.DateTimePeriod.DateTimePeriodFormatCode == DateTimePeriodFormatCodeList.Hhmm)
						{
							screeningExpiryTime = dTM.DateTimePeriod.DateTimePeriodValue;
						}
						else
						{
							result.Append(dTM.DateTimePeriod.DateTimePeriodFunctionCodeQualifier.ToString() + ":" + dTM.DateTimePeriod.DateTimePeriodFormatCode.ToString() + ": " + dTM.DateTimePeriod.DateTimePeriodValue + "\r\n");
						}
					}
					else
					{
						result.Append(dTM.DateTimePeriod.DateTimePeriodFunctionCodeQualifier.ToString() + ":" + dTM.DateTimePeriod.DateTimePeriodFormatCode.ToString() + ": " + dTM.DateTimePeriod.DateTimePeriodValue + "\r\n");
					}
				}
				ZDateTime screeningExpiryDateTime = ZDateTime.Empty;
				ZDateTime.TryParseExact(screeningExpiryDate + screeningExpiryTime, out screeningExpiryDateTime, "yyyyMMddHHmm");
				result.Append("Screening Period Expiry: ");
				if (screeningExpiryDateTime.IsValid)
				{
					result.Append(screeningExpiryDateTime.ToString("ddMMMyyyy HH:mm"));
				}
				else
				{
					result.Append(screeningExpiryDate + " " + screeningExpiryTime);
				}
				result.Append(" \r\n");
			}
			foreach (GISSegment gIS in group6.GIS)
			{
				result.Append("Customs Indicator: ");
				if (gIS.ProcessingIndicator_X.ProcessingIndicatorDescriptionCode == "LCL")
				{
					result.Append("Linked to Cargo Report Line");
				}
				else if (gIS.ProcessingIndicator_X.ProcessingIndicatorDescriptionCode == "CLF")
				{
					result.Append("Consolidation");
				}
				else if (gIS.ProcessingIndicator_X.ProcessingIndicatorDescriptionCode == "CLS")
				{
					result.Append("SAC");
				}
				else
				{
					result.Append(gIS.ProcessingIndicator_X.ProcessingIndicatorDescriptionCode);
				}
				result.Append(" \r\n");
			}
			foreach (SegmentGroup11 group11 in group6.Group11)
			{
				foreach (FTXSegment fTX in group11.FTX)
				{
					result.Append(fTX.TextLiteral.FreeTextValue1 + ": " + fTX.TextLiteral.FreeTextValue2 + "\r\n");
				}
			}
			foreach (SegmentGroup13 group13 in group6.Group13)
			{
				foreach (ERPSegment eRP in group13.ERP)
				{
					if (eRP.ErrorPointDetails.MessageItemNumber.Length > 0)
					{
						result.Append("Risk Line Number: " + eRP.ErrorPointDetails.MessageItemNumber + "\r\n");
					}
				}
				foreach (RFFSegment rFF in group13.RFF)
				{
					result.Append("Linked Import Declaration ");
					if (rFF.Reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.AccountPartysReference)
					{
						result.Append("Identifier: " + rFF.Reference.ReferenceIdentifier);
					}
					else if (rFF.Reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.SendersReferenceToTheOriginalMessage)
					{
						result.Append("Document Type: " + rFF.Reference.ReferenceIdentifier);
					}
					else if (rFF.Reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.SourceDocumentInternalReference)
					{
						result.Append("SAC Indicator: " + rFF.Reference.ReferenceIdentifier);
					}
					else
					{
						result.Append(rFF.Reference.ReferenceFunctionCodeQualifier.ToString() + ": " + rFF.Reference.ReferenceIdentifier);
					}
					result.Append("\r\n");
				}
			}
			result.Append("\r\n");
			return result.ToString();
		}

		ZString GetRFFSegmentDescription(RFFSegment rFF)
		{
			string rfq = rFF.Reference.ReferenceFunctionCodeQualifier.ToString();
			if (rfq == ReferenceFunctionCodeQualifierList.UnitLoadDeviceEGContainerIdentificationNumber)
			{
				return "Container: " + rFF.Reference.ReferenceIdentifier;
			}
			else if (rfq == ReferenceFunctionCodeQualifierList.StatementNumber)
			{
				return "Impediment Document Version Number: " + rFF.Reference.ReferenceIdentifier;
			}
			else if (rfq == ReferenceFunctionCodeQualifierList.UniqueMarketReference)
			{
				ZString result = new CMRImpedimentTypes().GetDescriptionFromCode(rFF.Reference.ReferenceIdentifier);
				return "Impediment Type: " + (result.IsEmpty ? rFF.Reference.ReferenceIdentifier : result.ToString());
			}
			else if (rfq == ReferenceFunctionCodeQualifierList.GovernmentAgencyReferenceNumber)
			{
				ZString result = rFF.Reference.ReferenceIdentifier;
				switch (rFF.Reference.ReferenceIdentifier)
				{
					case "C":
						result = "ACS";
						break;
					case "Q":
						result = "QUARANTINE";
						break;
					case "F":
						result = "FOOD";
						break;
				}
				return "Agency / Program: " + result;
			}
			else if (rfq == ReferenceFunctionCodeQualifierList.CustomsItemNumber)
			{
				ZInt riskNumber = ZInt.ParseSafe(rFF.Reference.ReferenceIdentifier, 0);
				ZQuery query = new ZQuery(CMRCommunityProtectionRiskSchema.CK_Identifier, riskNumber);
				query.AddToFilter(CMRCommunityProtectionRiskSchema.CK_EndDate, ZDateTime.Empty);
				CMRCommunityProtectionRisk risk = Factory.LoadTop1<CMRCommunityProtectionRisk>(query);
				return "Risk Identifier: " + rFF.Reference.ReferenceIdentifier + (risk != null ? ": " + risk.CK_Description : "");
			}
			return rfq + ": " + rFF.Reference.ReferenceIdentifier;
		}

		public override ZString StatusOfLinesReport
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();
				if (CUSRES != null && CUSRES.Group6.Count > 0)
				{
					result.Append("\r\n");
					foreach (SegmentGroup6 group6 in CUSRES.Group6)
					{
						if (group6.DOC.Count > 0 && group6.DOC[0].DocumentMessageName.DocumentNameCode != "I")
						{
							result.Append(GetStatusDescriptionOfOneLine(group6));
						}
					}
				}
				return result.ToString();
			}
		}

		ZString GetStatusDescriptionOfOneLine(SegmentGroup6 group6)
		{
			ZStringBuilder result = new ZStringBuilder();
			result.Append("Status for Packing (Transport) Line: " + group6.DOC[0].DocumentMessageDetails.DocumentMessageNumber + "\r\n");
			ZShort lineNumber = -1;
			ZShort.TryParse(group6.DOC[0].DocumentMessageDetails.DocumentMessageNumber, out lineNumber);
			CusEntryHeader entryHeader = EM_LinkedObject as CusEntryHeader;
			Package pack = null;
			if (entryHeader != null && entryHeader.Declaration != null && lineNumber > 0)
			{
				foreach (Package package in entryHeader.Declaration.Packages)
				{
					if (package.PackingGroup != null && EnsureMinimumLineNumber(package.PackingGroup.CR_HouseContainerNumber) == lineNumber)
					{
						pack = package;
						break;
					}
				}
			}
			else
			{
				PackingGroup packingGroup = EM_LinkedObject as PackingGroup;
				if (packingGroup != null && packingGroup.Packages.Count > 0)
				{
					pack = packingGroup.Packages[0];
				}
			}
			ZString status = GetFTXSegmentValue(group6, "CONSOLIDATED STATUS");
			if (status.IsEmpty)
			{
				status = "UNKNOWN";
			}

			result.Append("***CONSOLIDATED CARGO STATUS: ***");
			result.Append(status);
			result.Append("***\r\n");
			if (group6.Group11.Count > 0)
			{
				result.Append("ACSDec/ACSCR/AQISDec/AQISCR: ");
				result.Append(AbbreviatedStatusDescription(group6));
				result.Append("\r\n");
			}
			if (pack != null)
			{
				if (!pack.CW_ContainerNoOrEquipmentNo.IsEmpty)
				{
					result.Append("Container: " + pack.CW_ContainerNoOrEquipmentNo + "\r\n");
				}
				if (!pack.CW_HouseBill.IsEmpty)
				{
					result.Append(pack.CW_HouseBill + "\r\n");
				}
			}
			result.Append(GetOneGroup6Representation(group6));
			return result.ToString();
		}

		ZString AbbreviatedStatusDescription(SegmentGroup6 group6)
		{
			ZString aCSCR = "N";
			ZString aQISCR = "N";
			foreach (GISSegment gIS in group6.GIS)
			{
				if (gIS.ProcessingIndicator_X.ProcessingIndicatorDescriptionCode == "LCL")
				{
					aCSCR = "Y";
					aQISCR = "Y";
					break;
				}
			}
			ZString aCSDec = "Y";
			ZString aQISDec = "Y";
			ZString tempValue = GetFTXSegmentValue(group6, "IMPORT DEC ACS EVALUATED");
			if (!tempValue.IsEmpty)
			{
				aCSDec = tempValue.Left(1);
			}

			if (aCSDec == "Y" && GetFTXSegmentValue(group6, "IMPORT DECLARATION PAID") == "NO")
			{
				aCSDec = "$";
			}

			tempValue = GetFTXSegmentValue(group6, "CARGO REPORT ACS EVALUATED");
			if (!tempValue.IsEmpty)
			{
				aCSCR = tempValue.Left(1);
			}

			tempValue = GetFTXSegmentValue(group6, "IMPORT DEC AQIS EVALUATED");
			if (!tempValue.IsEmpty)
			{
				aQISDec = tempValue.Left(1);
			}

			tempValue = GetFTXSegmentValue(group6, "CARGO REPORT AQIS EVALUATED");
			if (!tempValue.IsEmpty)
			{
				aQISCR = tempValue.Left(1);
			}

			return aCSDec + "/" + aCSCR + "/" + aQISDec + "/" + aQISCR;
		}

		protected override ZString AbbreviatedStatusDescriptionForTransportLineCore(ZShort cargoLineNumber)
		{
			if (CUSRES != null && CUSRES.Group6.Count > 0)
			{
				cargoLineNumber = EnsureMinimumLineNumber(cargoLineNumber);
				foreach (SegmentGroup6 group6 in CUSRES.Group6)
				{
					if (group6.DOC.Count > 0 && group6.DOC[0].DocumentMessageName.DocumentNameCode == "S")
					{
						ZShort lineNumber = -1;
						ZShort.TryParse(group6.DOC[0].DocumentMessageDetails.DocumentMessageNumber, out lineNumber);
						if (lineNumber == cargoLineNumber && group6.Group11.Count > 0)
						{
							return AbbreviatedStatusDescription(group6);
						}
					}
				}
			}
			return ZString.Empty;
		}

		public ZString GetStatusDescriptionOfLine(ZShort cargoLineNumber)
		{
			ZStringBuilder result = new ZStringBuilder();
			if (CUSRES != null && CUSRES.Group6.Count > 0)
			{
				cargoLineNumber = EnsureMinimumLineNumber(cargoLineNumber);
				foreach (SegmentGroup6 group6 in CUSRES.Group6)
				{
					if (group6.DOC.Count > 0 && group6.DOC[0].DocumentMessageName.DocumentNameCode == "S")
					{
						ZShort lineNumber = -1;
						ZShort.TryParse(group6.DOC[0].DocumentMessageDetails.DocumentMessageNumber, out lineNumber);
						if (lineNumber == cargoLineNumber)
						{
							result.Append(GetStatusDescriptionOfOneLine(group6));
							break;
						}
					}
				}
			}
			return result.ToString();
		}

		ZString GetFTXSegmentValue(SegmentGroup6 group6, string identifier)
		{
			foreach (SegmentGroup11 group11 in group6.Group11)
			{
				if (group11.FTX.Count > 0 && group11.FTX[0].TextLiteral.FreeTextValue1 == identifier)
				{
					return group11.FTX[0].TextLiteral.FreeTextValue2;
				}
			}
			return ZString.Empty;
		}

		ZShort EnsureMinimumLineNumber(ZShort lineNumber) => lineNumber > 0 ? lineNumber : (ZShort)1;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.DSA;
		}
	}
}
