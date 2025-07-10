using System.Collections;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AuthorityToDeal : D99BCUSRESInfoProvider
	{
		public AuthorityToDeal(CUSRESMessage edifactMessage)
			: base(edifactMessage)
		{
		}

		CUSRESMessage ATDMessage
		{
			get { return CUSRES; }
		}

		public override ZString DocumentName
		{
			get { return "ATD"; }
		}

		#region SecurityCode

		public ZString SecurityCode
		{
			get
			{
				if (fSecurityCode.IsEmpty)
				{
					foreach (SegmentGroup3 group3 in ATDMessage.Group3)
					{
						foreach (RFFSegment rFF in group3.RFF)
						{
							if (rFF.Reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.ComplianceCodeNumber)
							{
								fSecurityCode = rFF.Reference.ReferenceIdentifier;
								break;
							}
						}
					}
				}

				return fSecurityCode;
			}
		}
		ZString fSecurityCode;

		#endregion

		#region Authority to Deal Date Issued

		public ZString AuthorityToDealDateIssued
		{
			get
			{
				if (fAuthorityToDealDateIssued.IsEmpty)
				{
					foreach (DTMSegment dTM in ATDMessage.DTM)
					{
						if (dTM.DateTimePeriod.DateTimePeriodFunctionCodeQualifier == DateTimePeriodFunctionCodeQualifierList.ClearanceDateCustoms)
						{
							ZDateTime date = GetDate(dTM);
							fAuthorityToDealDateIssued = date.ToShortDateString();
						}
					}
				}

				return fAuthorityToDealDateIssued;
			}
		}
		ZString fAuthorityToDealDateIssued;

		#endregion

		#region Authority To Deal Action Reason

		public ZString AuthorityToDealActionReason
		{
			get
			{
				if (fAuthorityToDealActionReason.IsEmpty)
				{
					foreach (FTXSegment fTX in ATDMessage.FTX)
					{
						if (fTX.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.CustomsClearanceInstructionImport)
						{
							fAuthorityToDealActionReason = fTX.TextLiteral.FreeTextValue1;
							break;
						}
					}
				}

				return fAuthorityToDealActionReason;
			}
		}
		ZString fAuthorityToDealActionReason;

		#endregion

		#region TotalNumberOfPacakges

		public ZString TotalNumberOfPacakges
		{
			get
			{
				if (fTotalNumberOfPacakges.IsEmpty)
				{
					foreach (CNTSegment cNT in ATDMessage.CNT)
					{
						if (cNT.Control.ControlTotalTypeCodeQualifier == ControlTotalTypeCodeQualifierList.TotalNumberOfPackages)
						{
							fTotalNumberOfPacakges = cNT.Control.ControlValue;
							break;
						}
					}
				}

				return fTotalNumberOfPacakges;
			}
		}
		ZString fTotalNumberOfPacakges;

		#endregion

		#region WarehouseEstablishmentIdentifier

		public ZString WarehouseEstablishmentIdentifier
		{
			get
			{
				if (fWarehouseEstablishmentIdentifier.IsEmpty)
				{
					foreach (LOCSegment lOC in ATDMessage.LOC)
					{
						if (lOC.LocationFunctionCodeQualifier == LocationFunctionCodeQualifierList.Warehouse)
						{
							fWarehouseEstablishmentIdentifier = lOC.LocationIdentification.LocationNameCode;
							break;
						}
					}
				}

				return fWarehouseEstablishmentIdentifier;
			}
		}
		ZString fWarehouseEstablishmentIdentifier;

		#endregion

		#region Lines

		public AuthorityToDealLineConditionDetails[] Lines
		{
			get
			{
				if (fLines == null)
				{
					ArrayList result = new ArrayList();
					foreach (SegmentGroup6 group6 in ATDMessage.Group6)
					{
						foreach (SegmentGroup13 group13 in group6.Group13)
						{
							result.Add(new AuthorityToDealLineConditionDetails(group13));
						}
					}

					fLines = (AuthorityToDealLineConditionDetails[])result.ToArray(typeof(AuthorityToDealLineConditionDetails));
				}

				return fLines;
			}
		}
		AuthorityToDealLineConditionDetails[] fLines;

		#endregion

		#region Customs Officer Name

		public ZString CustomsOfficerName
		{
			get
			{
				if (fCustomsOfficerName.IsEmpty && NADSegmentForCustomsOfficerInfo != null)
				{
					fCustomsOfficerName = NADSegmentForCustomsOfficerInfo.NameAndAddress.NameAndAddressLine3;
				}

				return fCustomsOfficerName;
			}
		}
		ZString fCustomsOfficerName;

		#endregion

		#region Workgroup Name

		public ZString WorkgroupName
		{
			get
			{
				if (fWorkgroupName.IsEmpty && NADSegmentForCustomsOfficerInfo != null)
				{
					fWorkgroupName = NADSegmentForCustomsOfficerInfo.NameAndAddress.NameAndAddressLine2;
				}

				return fWorkgroupName;
			}
		}
		ZString fWorkgroupName;

		#endregion

		#region Customs Officer State

		public ZString CustomsOfficerState
		{
			get
			{
				if (fCustomsOfficerState.IsEmpty && NADSegmentForCustomsOfficerInfo != null)
				{
					fCustomsOfficerState = NADSegmentForCustomsOfficerInfo.NameAndAddress.NameAndAddressLine1;
				}

				return fCustomsOfficerState;
			}
		}
		ZString fCustomsOfficerState;

		#endregion

		#region Customs Officer Email Address

		public ZString CustomsOfficerEmailAddress
		{
			get
			{
				if (fCustomsOfficerEmailAddress.IsEmpty)
				{
					fCustomsOfficerEmailAddress = GetCommunicationInfoFromCOMSegment(CommunicationNumberCodeQualifierList.ElectronicMail);
				}

				return fCustomsOfficerEmailAddress;
			}
		}

		ZString fCustomsOfficerEmailAddress;

		#endregion

		#region Customs Officer Fax Number

		public ZString CustomsOfficerFaxNumber
		{
			get
			{
				if (fCustomsOfficerFaxNumber.IsEmpty)
				{
					fCustomsOfficerFaxNumber = GetCommunicationInfoFromCOMSegment(CommunicationNumberCodeQualifierList.Telefax);
				}

				return fCustomsOfficerFaxNumber;
			}
		}

		ZString fCustomsOfficerFaxNumber;

		#endregion

		#region Customs Officer Telephone Number

		public ZString CustomsOfficerTelephoneNumber
		{
			get
			{
				if (fCustomsOfficerTelephoneNumber.IsEmpty)
				{
					fCustomsOfficerTelephoneNumber = GetCommunicationInfoFromCOMSegment(CommunicationNumberCodeQualifierList.Telephone);
				}

				return fCustomsOfficerTelephoneNumber;
			}
		}

		ZString fCustomsOfficerTelephoneNumber;

		#endregion

		#region Implementation

		NADSegment NADSegmentForCustomsOfficerInfo
		{
			get
			{
				if (fNADSegmentForCustomsOfficerInfo == null)
				{
					foreach (SegmentGroup1 group1 in ATDMessage.Group1)
					{
						foreach (NADSegment nAD in group1.NAD)
						{
							if (nAD.PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.Customs)
							{
								fNADSegmentForCustomsOfficerInfo = nAD;
								return fNADSegmentForCustomsOfficerInfo;
							}
						}
					}
				}

				return fNADSegmentForCustomsOfficerInfo;
			}
		}
		NADSegment fNADSegmentForCustomsOfficerInfo;

		ZString GetCommunicationInfoFromCOMSegment(CommunicationNumberCodeQualifierList codeToGetInfoFor)
		{
			foreach (SegmentGroup1 group1 in ATDMessage.Group1)
			{
				foreach (SegmentGroup2 group2 in group1.Group2)
				{
					foreach (COMSegment cOM in group2.COM)
					{
						if (cOM.CommunicationContact.CommunicationNumberCodeQualifier == codeToGetInfoFor)
						{
							return cOM.CommunicationContact.CommunicationNumber;
						}
					}
				}
			}

			return ZString.Empty;
		}

		#endregion
	}
}
