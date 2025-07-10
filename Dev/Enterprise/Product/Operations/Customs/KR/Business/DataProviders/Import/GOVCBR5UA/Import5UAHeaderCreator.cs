using System;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class Import5UAHeaderCreator
	{
		public Import5UAHeader Create(CusEntryHeader entry, IImport5UASessionDetails sessionDetails, DateTime amendmentDeclarationDate, int amendmentVersionNo, ZString amendmentPenaltyType)
		{
			var import5UAHeaderData = new Import5UAHeader();
			var declaration = entry.Declaration;
			import5UAHeaderData.ImportDeclarationNumber = entry.EntryNumber;
			import5UAHeaderData.DeclarationCustomsOffice = declaration.JE_CustomsOffice;
			import5UAHeaderData.DeclarationCustomsDivision = declaration.JE_CustomsDivision;
			import5UAHeaderData.AmendmentDeclarationDate = amendmentDeclarationDate;
			import5UAHeaderData.AmendmentVersionNo = amendmentVersionNo;
			import5UAHeaderData.PenaltyType = amendmentPenaltyType;
			import5UAHeaderData.ExemptionProcessCode = ExemptionProcessCode.B;
			import5UAHeaderData.PenaltyExemptionReasonsCode = sessionDetails.PenaltyExemptionReasonCode;
			import5UAHeaderData.PenaltyExemptionReason = sessionDetails.PenaltyExemptionReason;

			if (declaration.BrokerAddress != null)
			{
				import5UAHeaderData.Declarant = new Organisation(RoleType.Declarant)
				{
					CompanyName = declaration.BrokerAddress.CompanyName,
					RepresentativeName = declaration.BrokerAddress.Header?.GetRepresentativeName() ?? string.Empty
				};
			}

			import5UAHeaderData.SequenceNo = sessionDetails.DutyPenaltyExemption5UASequenceNumber;

			import5UAHeaderData.PenaltyExemptionAmount = sessionDetails.PenaltyExemptionAmount;
			return import5UAHeaderData;
		}
	}
}
