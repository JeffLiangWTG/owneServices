using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class COLSReassessmentMessageBuilder : COLSMessageBuilder<COLSReassessmentMessage>
	{
		public COLSReassessmentMessageBuilder(QuarantineColsHeader colsHeader, ZString additionalComment , ZString reassessmentReason, ZBool documentationRequired)
			: base(colsHeader)
		{
			this.reassessmentReason = reassessmentReason;
			this.additionalComment = additionalComment;
			this.documentationRequired = documentationRequired;
		}
		readonly ZString additionalComment;
		readonly ZString reassessmentReason;
		readonly ZBool documentationRequired;

		const int ContactNameMaxLength = 40;
		const int EmailAddressMaxLength = 40;
		const int PhoneNumberMaxLength = 10;

		protected override ZString MessageType => AUCOLSMessageTypeList.Codes.RequestAReassessment;

		protected override BusinessObject MessageParent => colsHeader;

		protected override ZBool IgnoreNullProperties => true;

		protected override COLSReassessmentMessage GetMessageData()
		{
			var declaration = colsHeader.JobDeclaration;
			var responsibleParty = colsHeader.ResponsibleParty;
			var phoneNumberFormatter = new PhoneNumberFormatterAndValidator();
			var phoneNum = phoneNumberFormatter.FormatLocal(responsibleParty.E2_Phone.IsEmpty ? responsibleParty.E2_Mobile : responsibleParty.E2_Phone, "").KeepNumericCharacters();
			var branchPK = declaration?.Branch?.PK.ToGuid() ?? Guid.Empty;
			var branchID = Env.Registry.AUCustoms.GetLocalCustomsBranchIdentifierForBranch(branchPK) ?? string.Empty;
			var directionRequests = new List<COLSDirectionRequests>();
			foreach (QuarantineColsDirection direction in colsHeader.Directions)
			{
				if (!direction.QCD_Direction.IsEmpty)
				{
					var directionAddress = direction.AAAddress;
					var containerNumber = direction.Container?.CO_ContainerNumber ?? ZString.Empty;
					var entryLineNumber = direction.CusEntryLine?.UniqueKey ?? ZString.Empty;
					directionRequests.Add(new COLSDirectionRequests
					{
						direction = direction.QCD_Direction,
						directionLineContainer = containerNumber.IsEmpty ? entryLineNumber : containerNumber,
						treatmentType = direction.QCD_TreatmentType,
						location = directionAddress.Address1,
						aaname = directionAddress.E2_CompanyName,
						aanumber = directionAddress.E2_GovRegNum
					});
				}
			}

			return new COLSReassessmentMessage
			{
				originalLrn = colsHeader.LRN,
				brokerBranchId = branchID.ToUpper(),
				reassessmentReason = reassessmentReason,
				contactName = responsibleParty.E2_Contact.Left(ContactNameMaxLength),
				contactPhone = phoneNum.Left(PhoneNumberMaxLength),
				contactEmail = responsibleParty.E2_Email.Left(EmailAddressMaxLength),
				thirdPartyNotificationEmail = colsHeader.QCH_AlsoNotifyEmail.IsEmpty ? null : colsHeader.QCH_AlsoNotifyEmail.ToString(),
				additionalComments = additionalComment,
				generalDeclaration = "True",
				directionRequests = directionRequests,
				documentationRequired = documentationRequired,
			};
		}
	}
}
